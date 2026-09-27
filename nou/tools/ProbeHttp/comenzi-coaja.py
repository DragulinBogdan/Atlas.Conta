"""104b: coaja comenzii pe ușa HTTP reală.

Pe un draft NIR creat de probă, fiecare comandă de document (opereaza, anuleaza, storneaza,
valideaza, corecteaza) trece prin `ComenziDocument` + `DreptComandaXaf`:
User (nu vede documentele) -> 404, Cititor și Configurator (văd, fără Write) -> 403,
Admin: id inexistent și id pe ușa altui tip -> 404, domeniul -> 422, validarea -> 200 cu erori.
Niciun refuz nu scrie: draftul rămâne Draft, fără postări în cub. Draftul se șterge la final.
Necesită hostul WebApi privat pe baza dată (utilizatorii re-seed-uiți) și psycopg.
"""
import argparse
import json
import ssl
import urllib.error
import urllib.parse
import urllib.request
import uuid
from datetime import date

import psycopg

UTILIZATORI = ('Admin', 'Cititor', 'User', 'Configurator')
INVIZIBIL = 'nu există sau nu e vizibil'


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('--baza', default='Atlas.Conta.BackOffice.Privat')
    parser.add_argument('--url', default='https://localhost:5001')
    options = parser.parse_args()
    tokens = {}
    rezultate = []

    def call(user, path, method='GET', data=None):
        headers = {'Content-Type': 'application/json'}
        if user:
            headers['Authorization'] = 'Bearer ' + tokens[user]
        req = urllib.request.Request(options.url + path, headers=headers, method=method,
            data=json.dumps(data).encode() if data is not None else None)
        try:
            with urllib.request.urlopen(req, timeout=90, context=ssl._create_unverified_context()) as response:
                return response.status, response.read().decode()
        except urllib.error.HTTPError as error:
            return error.code, error.read().decode()

    def proba(eticheta, user, path, asteptat, contine=None, data=None):
        status, body = call(user, path, 'POST', data)
        ok = status == asteptat and (contine is None or contine in body)
        rezultate.append(ok)
        print(f"{'PASS' if ok else 'FAIL'} {eticheta} [{user}] -> {status} (așteptat {asteptat}) {body[:140]}")
        return status, body

    def prima(entity):
        status, body = call('Admin', f'/api/odata/{entity}?$top=1')
        assert status == 200, (entity, status, body[:300])
        return json.loads(body)['value'][0]['ID']

    for name in UTILIZATORI:
        status, body = call(None, '/api/Authentication/Authenticate', 'POST', {'userName': name, 'password': ''})
        assert status == 200, (name, status, body[:300])
        tokens[name] = body.strip('"')

    azi = date.today().isoformat()
    status, body = call('Admin', '/api/nir', 'POST', {
        'Data': azi, 'PredatorId': prima('Partener'), 'PrimitorId': prima('Gestiune'), 'Linii': []})
    assert status == 201, (status, body[:500])
    nir = json.loads(body)['Id']
    try:
        comenzi = (
            ('opereaza', f'/api/nir/{nir}/opereaza', None),
            ('anuleaza', f'/api/nir/{nir}/anuleaza', None),
            ('storneaza', f'/api/nir/{nir}/storneaza', {'Data': azi}),
            ('valideaza', f'/api/nir/{nir}/valideaza', None),
            ('corecteaza', f'/api/documente/{nir}/corecteaza', {'Data': azi, 'Motiv': 'EroareMateriala'}),
        )
        for nume, cale, corp in comenzi:
            proba(nume, 'User', cale, 404, INVIZIBIL, corp)
            proba(nume, 'Cititor', cale, 403, 'modifica „NIR”', corp)
            proba(nume, 'Configurator', cale, 403, 'modifica „NIR”', corp)
            proba(nume + ' id inexistent', 'Admin', cale.replace(nir, str(uuid.uuid4())), 404, INVIZIBIL, corp)
        for nume, cale, corp in comenzi[:3] + comenzi[4:]:
            proba(nume + ' pe draft fără linii', 'Admin', cale, 422, None, corp)
        status, body = proba('valideaza pe draft fără linii', 'Admin', f'/api/nir/{nir}/valideaza', 200)
        assert json.loads(body).get('Erori'), body[:500]
        proba('operare id NIR pe ușa FCT', 'Admin', f'/api/fct/{nir}/opereaza', 404, INVIZIBIL)
        proba('operare id NIR pe ușa NTC', 'Admin', f'/api/ntc/{nir}/opereaza', 404, INVIZIBIL)

        with psycopg.connect(f'host=localhost port=5444 dbname={options.baza} user=postgres password=postgres',
                autocommit=True) as conn:
            assert conn.execute('select current_database()').fetchone()[0] == options.baza
            stare = conn.execute('SELECT "Stare" FROM "Documente" WHERE "ID"=%s', (nir,)).fetchone()[0]
            postari = conn.execute('SELECT count(*) FROM "Postare" WHERE "DocumentId"=%s', (nir,)).fetchone()[0]
            ok = stare == 0 and postari == 0
            rezultate.append(ok)
            print(f"{'PASS' if ok else 'FAIL'} niciun refuz nu scrie: Stare={stare}, postări={postari}")
    finally:
        status, _ = call('Admin', f'/api/nir/{nir}', 'DELETE')
        print(f'curățenie: DELETE /api/nir/{nir} -> {status}')
    print(f'Total: {len(rezultate)} probe, {rezultate.count(True)} PASS, {rezultate.count(False)} FAIL.')
    raise SystemExit(0 if all(rezultate) else 1)


if __name__ == '__main__':
    main()
