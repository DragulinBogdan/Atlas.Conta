"""X-D6 pe ușa HTTP: blocajul scrierii ținut din afară, pe o a doua conexiune.

Cât timp altă sesiune ține `pg_advisory_xact_lock(97000)`: citirea și dry-run-ul răspund imediat,
salvarea fără detalii noi trece, iar comanda și salvarea cu detalii noi ies 422 `SCRIERE_OCUPATA`
după timpul comenzii, fără nicio scriere. După eliberare, aceeași comandă trece.
Fixture propriu (furnizor + FCT), desfăcut în `finally`.
Necesită hostul WebApi privat pe baza dată și psycopg.
"""
import argparse
import json
import ssl
import time
import urllib.error
import urllib.parse
import urllib.request
import uuid

import psycopg

REFUZ = 'SCRIERE_OCUPATA'
BLOCAJ = 'SELECT pg_advisory_xact_lock(97000)'
IMEDIAT = 5.0


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--baza', required=True)
    parser.add_argument('--url', default='https://localhost:5001')
    parser.add_argument('--timp-comanda', type=float, default=30.0, help='Timpul de comandă al hostului, în secunde.')
    options = parser.parse_args()
    token = None
    rezultate = []
    dsn = f'host=localhost port=5444 dbname={options.baza} user=postgres password=postgres'

    def call(path, method='GET', data=None):
        headers = {'Content-Type': 'application/json'}
        if token:
            headers['Authorization'] = 'Bearer ' + token
        req = urllib.request.Request(options.url + path, headers=headers, method=method,
            data=json.dumps(data).encode() if data is not None else None)
        start = time.monotonic()
        try:
            with urllib.request.urlopen(req, timeout=options.timp_comanda * 3,
                    context=ssl._create_unverified_context()) as response:
                status, body = response.status, response.read().decode()
        except urllib.error.HTTPError as error:
            status, body = error.code, error.read().decode()
        return status, body, time.monotonic() - start

    def fixture(path, method='GET', data=None, asteptat=200):
        status, body, _ = call(path, method, data)
        assert status == asteptat, (method, path, status, body[:600])
        return json.loads(body) if body[:1] in ('{', '[') else body

    def verdict(eticheta, ok, masurat):
        rezultate.append(ok)
        print(f"{'PASS' if ok else 'FAIL'} {eticheta} — {masurat}", flush=True)

    def prima(entity, filtru=None):
        path = f'/api/odata/{entity}?$top=1'
        if filtru:
            path += '&$filter=' + urllib.parse.quote(filtru)
        return fixture(path)['value'][0]['ID']

    def refuz_ocupat(eticheta, path, method, data):
        status, body, durata = call(path, method, data)
        erori = json.loads(body).get('Erori', []) if body[:1] == '{' else []
        verdict(eticheta, status == 422 and len(erori) == 1 and erori[0].startswith(REFUZ + ':')
            and durata >= options.timp_comanda * 0.8, f'{status} în {durata:.1f} s: {body[:160]}')

    status, body, _ = call('/api/Authentication/Authenticate', 'POST', {'userName': 'Admin', 'password': ''})
    assert status == 200, (status, body[:300])
    token = body.strip('"')

    marker = 'X6-' + uuid.uuid4().hex[:8]
    luna = min((p for p in fixture('/api/perioade') if not p['Inchisa']), key=lambda p: (p['An'], p['Luna']))
    data = f"{luna['An']}-{luna['Luna']:02d}-15"
    furnizor = fct = None
    try:
        furnizor = fixture('/api/odata/Partener', 'POST', {
            'Cod': f'PROBA-{marker}', 'Denumire': 'Furnizor probă X-D6', 'Tara': 'RO',
            'TipPersoana': 'Juridica', 'InregistratTva': False}, 201)['ID']
        linie = {'TipMaterialId': prima('TipMaterial', "Cod eq '628'"), 'TipTvaId': prima('TipTva', "Cod eq 'N21'"),
            'Cantitate': 1, 'PretUnitar': 100}
        corp = {'Numar': marker, 'Data': data, 'PredatorId': furnizor, 'PrimitorId': prima('Gestiune'), 'Linii': [linie]}
        fct = fixture('/api/fct', 'POST', corp, 201)['Id']
        citita = fixture(f'/api/fct/{fct}')
        fara_detalii_noi = dict(corp, NumarPV='X6', Linii=[dict(linie, Id=citita['Linii'][0]['Id'])])
        al_doilea = dict(corp, Numar=marker + '-B')

        with psycopg.connect(dsn) as poarta, psycopg.connect(dsn, autocommit=True) as martor:
            assert martor.execute('select current_database()').fetchone()[0] == options.baza
            poarta.execute(BLOCAJ)

            status, body, durata = call(f'/api/fct/{fct}')
            verdict('citirea sub blocaj', status == 200 and durata < IMEDIAT, f'{status} în {durata:.1f} s')
            status, body, durata = call(f'/api/fct/{fct}/valideaza', 'POST')
            verdict('dry-run-ul sub blocaj', status == 200 and durata < IMEDIAT and not json.loads(body).get('Erori'),
                f'{status} în {durata:.1f} s: {body[:120]}')
            status, body, durata = call(f'/api/fct/{fct}', 'PUT', fara_detalii_noi)
            verdict('salvarea fără detalii noi sub blocaj', status == 200 and durata < IMEDIAT,
                f'{status} în {durata:.1f} s: {body[:120]}')

            refuz_ocupat('operarea sub blocaj', f'/api/fct/{fct}/opereaza', 'POST', {})
            refuz_ocupat('salvarea cu detalii noi sub blocaj', '/api/fct', 'POST', al_doilea)

            stare = martor.execute('SELECT "Stare" FROM "Documente" WHERE "ID"=%s', (uuid.UUID(fct),)).fetchone()[0]
            tranzactii = martor.execute('SELECT count(*) FROM "Tranzactie" WHERE "DocumentId"=%s',
                (uuid.UUID(fct),)).fetchone()[0]
            scrise = martor.execute('SELECT count(*) FROM "Documente" WHERE "Numar"=%s', (al_doilea['Numar'],)).fetchone()[0]
            verdict('refuzul nu scrie', stare == 0 and tranzactii == 0 and scrise == 0,
                f'Stare={stare}, tranzacții={tranzactii}, documente noi={scrise}')
            poarta.rollback()

        status, body, durata = call(f'/api/fct/{fct}/opereaza', 'POST', {})
        verdict('după eliberare, aceeași comandă trece', status == 200 and durata < IMEDIAT,
            f'{status} în {durata:.1f} s: {body[:120]}')
    finally:
        if fct:
            for ramas in fixture('/api/fct?' + urllib.parse.urlencode(
                    {'filter': json.dumps(['Numar', 'startswith', marker])}))['data']:
                if ramas['Stare'] == 'Operat':
                    print('curățenie: anuleaza ->', call(f"/api/fct/{ramas['Id']}/anuleaza", 'POST', {})[0])
                print('curățenie: DELETE fct ->', call(f"/api/fct/{ramas['Id']}", 'DELETE')[0])
        if furnizor:
            print('curățenie: DELETE partener ->', call(f'/api/odata/Partener({furnizor})', 'DELETE')[0])
    print(f'Total: {len(rezultate)} probe, {rezultate.count(True)} PASS, {rezultate.count(False)} FAIL.')
    raise SystemExit(0 if all(rezultate) else 1)


if __name__ == '__main__':
    main()
