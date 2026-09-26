"""SC-CIT-46/48: desfacerea unei plăți automate prin API, cu verificarea dreptului Delete.

Necesită hostul WebApi privat pe baza dată (implicit Atlas.Conta.BackOffice.Privat) și psycopg. Fixture-ul este eliminat în finally.
"""
import json
import argparse
import time
import urllib.error
import urllib.parse
import urllib.request
import uuid
from pathlib import Path

import psycopg


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('--prin-xaf', action='store_true', help='Așteaptă cel mult 5 minute ștergerea prin acțiunea XAF.')
    parser.add_argument("--semnal-browser")
    parser.add_argument('--baza', default='Atlas.Conta.BackOffice.Privat')
    parser.add_argument('--url', default='http://127.0.0.1:5000')
    options = parser.parse_args()
    base = options.url
    tokens, documents, links = {}, [], []

    def call(user, path, method='GET', data=None, expected=200):
        headers = {'Content-Type': 'application/json'}
        if user:
            headers['Authorization'] = 'Bearer ' + tokens[user]
        req = urllib.request.Request(base + path, headers=headers, method=method,
            data=json.dumps(data).encode() if data is not None else None)
        try:
            with urllib.request.urlopen(req, timeout=90) as response:
                status, body = response.status, response.read().decode()
        except urllib.error.HTTPError as error:
            status, body = error.code, error.read().decode()
        assert status == expected, (method, path, status, body[:1200])
        if path == '/api/Authentication/Authenticate':
            return body.strip('"')
        return json.loads(body) if body else None

    def lookup(entity, field, value):
        return call('Admin', '/api/odata/' + entity + '?' + urllib.parse.urlencode(
            {'$filter': f"{field} eq '{value}'"}))['value'][0]['ID']

    with psycopg.connect(f'host=localhost port=5444 dbname={options.baza} user=postgres password=postgres',
            autocommit=True) as conn:
        assert conn.execute('select current_database()').fetchone()[0] == options.baza
        for name in ('Admin', 'Cititor', 'User'):
            tokens[name] = call(None, '/api/Authentication/Authenticate', 'POST', {'userName': name, 'password': ''})
        debit = lookup('Cont', 'Simbol', '628')
        assert conn.execute('SELECT count(*) FROM "Conturi" WHERE "ID"=%s AND "Simbol"=%s',
            (uuid.UUID(debit), '628')).fetchone()[0] == 1, 'Hostul nu folosește baza izolată.'
        assert conn.execute('SELECT count(*) FROM "PerioadeFiscale" WHERE "An"=2026 AND "Luna"=1 AND NOT "Inchisa"').fetchone()[0] == 1
        supplier = call('Admin', '/api/odata/Partener', 'POST',
            {'Cod': 'CODEX-D8-P-' + uuid.uuid4().hex[:8], 'Denumire': 'Proba partide'}, expected=201)['ID']
        try:
            cash = lookup('ContPropriu', 'Cod', 'CASA')
            stock = lookup('Gestiune', 'Cod', 'MAG1')
            tip = lookup('TipMaterial', 'Cod', '628')
            sfd = lookup('TipTva', 'Cod', 'SFD')
            f = call('Admin', '/api/fct', 'POST', {'Numar': 'CODEX-D8-P', 'Data': '2026-01-15',
                'PredatorId': supplier, 'PrimitorId': stock, 'GenereazaPlata': True,
                'PlataContPropriuId': cash, 'PlataNumar': 'CODEX-D8-P-PLT', 'PlataData': '2026-01-15',
                'PlataTipInstrument': 'DispozitieCasa',
                'Linii': [{'TipMaterialId': tip, 'TipTvaId': sfd, 'Cantitate': 1, 'PretUnitar': 100}]}, expected=201)['Id']
            documents.append(('fct', f))
            result = call('Admin', f'/api/fct/{f}/opereaza', 'POST', {})
            payment = result['ConexId']
            assert payment
            documents.append(('plt', payment))
            call('Admin', f'/api/plt/{payment}/opereaza', 'POST', {})
            panel = call('Admin', f'/api/imperecheri/{payment}/stingeri')
            link = panel['Imperecheri'][0]['Id']
            links.append(link)
            unit = conn.execute('SELECT "Unitate" FROM "Postare" WHERE "DocumentId"=%s AND "FelUnitate"=2 LIMIT 1', (uuid.UUID(f),)).fetchone()[0]

            def net(unit_id):
                return conn.execute('SELECT coalesce(sum(CASE WHEN "Latura"=1 THEN "Valoare" ELSE -"Valoare" END),0) FROM "Postare" WHERE "Unitate"=%s AND "Carte"=1', (unit_id,)).fetchone()[0]

            assert net(unit) == 0
            call('User', f'/api/imperecheri/{link}', 'DELETE', expected=404)
            call('Cititor', f'/api/imperecheri/{link}', 'DELETE', expected=403)
            assert net(unit) == 0
            if options.prin_xaf:
                print(f'XAF: șterge împerecherea {link} a plății {payment} prin acțiunea Șterge împerecherea.', flush=True)
                deadline = time.monotonic() + 300
                while call('Admin', f'/api/imperecheri/{payment}/stingeri')['Imperecheri']:
                    assert time.monotonic() < deadline, 'Acțiunea XAF nu a fost executată în 5 minute.'
                    time.sleep(1)
            else:
                call('Admin', f'/api/imperecheri/{link}', 'DELETE', expected=204)
            links.remove(link)
            assert net(unit) == -100
            own = conn.execute('SELECT DISTINCT "Unitate" FROM "Postare" WHERE "DocumentId"=%s AND "FelUnitate"=2 AND "Unitate"<>%s', (uuid.UUID(payment), unit)).fetchone()[0]
            assert net(own) == 100
            print('PASS SC-CIT-48: User 404, Cititor 403; ștergere ' + ('XAF' if options.prin_xaf else 'HTTP 204') + '; FCT -100 / PLT +100')
            request = {'DocumentStingatorId': payment, 'DocumentId': f, 'Suma': 40, 'Data': '2026-01-15'}
            call('User', '/api/imperecheri', 'POST', request, expected=403)
            call('Cititor', '/api/imperecheri', 'POST', request, expected=403)
            call('Admin', '/api/imperecheri', 'POST', {**request, 'DocumentId': str(uuid.uuid4())}, expected=404)
            call('Admin', '/api/odata/Imperechere', 'POST', request, expected=405)
            manual = call('Admin', '/api/imperecheri', 'POST', request, expected=201)['Id']
            links.append(manual)
            assert net(unit) == -60 and net(own) == 60
            call('Admin', '/api/imperecheri', 'POST', {**request, 'Suma': 61}, expected=422)
            assert net(unit) == -60 and net(own) == 60
            assert call('Admin', f'/api/imperecheri/{payment}/stingeri')['Ramas'] == 60
            query = urllib.parse.urlencode({'documentCurentId': payment, 'contrapartidaId': supplier, 'sens': 'Datorie'})
            candidates = call('Admin', '/api/proiectii/documente-cu-rest?' + query)['data']
            assert len(candidates) == 1 and candidates[0]['Disponibil'] == 60
            print('PASS 101 HTTP: creare autorizată, refuz atomic 61 > 60, CRUD refuzat, candidat 60', flush=True)
            if options.semnal_browser:
                print(f'BROWSER READY /plt/{payment}', flush=True)
                deadline = time.monotonic() + 300
                while not Path(options.semnal_browser).exists():
                    assert time.monotonic() < deadline, 'Browser timeout'
                    time.sleep(1)
            call('Admin', f'/api/imperecheri/{manual}', 'DELETE', expected=204)
            links.remove(manual)
            assert net(unit) == -100 and net(own) == 100
            call('Admin', f'/api/plt/{payment}/anuleaza', 'POST', {})
            assert net(unit) == -100 and net(own) == 0
            print('PASS SC-CIT-46 HTTP: anularea plății după desfacere păstrează FCT -100')
        finally:
            for link in links:
                call('Admin', f'/api/imperecheri/{link}', 'DELETE', expected=204)
            for route, doc in reversed(documents):
                value = call('Admin', f'/api/{route}/{doc}')
                if value['Stare'] == 'Operat':
                    call('Admin', f'/api/{route}/{doc}/anuleaza', 'POST', {})
                call('Admin', f'/api/{route}/{doc}', 'DELETE', expected=204)
            call('Admin', f'/api/odata/Partener/{supplier}', 'DELETE', expected=200)


if __name__ == '__main__':
    main()
