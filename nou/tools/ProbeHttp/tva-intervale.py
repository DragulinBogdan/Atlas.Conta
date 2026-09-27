"""R6: diagnostic, recalcul, FK și confidențialitatea sursei pe HTTP secured."""
import argparse
import json
import urllib.error
import urllib.parse
import urllib.request
import uuid
import time
from pathlib import Path
import psycopg


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--host', default='http://127.0.0.1:5096')
    parser.add_argument('--baza', required=True)
    parser.add_argument('--asteapta-browser', action='store_true')
    args = parser.parse_args()
    assert args.baza.endswith('.CodexR6Http'), 'Proba scrie numai în clona izolată R6.'
    tokens, documents, types, policies, periods, roles = {}, [], [], [], [], []
    supplier = material = None
    year = 2024
    tag = 'HTTP-R6-' + uuid.uuid4().hex[:8]

    def call(user, path, method='GET', data=None, expected=200):
        headers = {'Content-Type': 'application/json'}
        if user: headers['Authorization'] = 'Bearer ' + tokens[user]
        request = urllib.request.Request(args.host + path, headers=headers, method=method,
            data=json.dumps(data).encode() if data is not None else None)
        try:
            with urllib.request.urlopen(request, timeout=90) as response:
                status, body = response.status, response.read().decode()
        except urllib.error.HTTPError as error:
            status, body = error.code, error.read().decode()
        assert status == expected, (user, method, path, status, body[:1800])
        return json.loads(body) if body and body[0] in '[{"' else body

    def login(name):
        tokens[name] = call(None, '/api/Authentication/Authenticate', 'POST', {'userName': name, 'password': ''})

    def rows(entity, query=''):
        return call('Admin', '/api/odata/' + entity + ('?' + urllib.parse.urlencode({'$filter': query}) if query else ''))['value']

    def lookup(entity, field, value):
        return rows(entity, f"{field} eq '{value}'")[0]

    def create(price, tax=None, source=None, avans=False, tva=None):
        line = {'TipMaterialId': material if avans else service['ID'], 'TipTvaId': tva or types[0],
            'Cantitate': 1, 'PretUnitar': price, 'LinieAvansId': source}
        if tax is not None: line['ValoareTva'] = tax
        doc = call('Admin', '/api/fct', 'POST', {'Numar': tag + str(len(documents)),
            'Data': f'{year}-08-15', 'PredatorId': supplier, 'PrimitorId': stock, 'Linii': [line]}, 201)
        documents.append(doc['Id'])
        return doc

    def report(user='Admin'):
        return call(user, f'/api/proiectii/diagnostic-tva?dataStart={year}-08-01&dataEnd={year}-08-31&take=500')['data']

    with psycopg.connect(f'host=localhost port=5444 dbname={args.baza} user=postgres password=postgres', autocommit=True) as conn:
        assert conn.execute('SELECT current_database()').fetchone()[0] == args.baza
        assert conn.execute('SELECT count(*) FROM "PerioadeFiscale" WHERE "An"=%s', (year,)).fetchone()[0] == 0
        for name in ['Admin', 'Cititor', 'User', 'Configurator']: login(name)
        service = lookup('TipMaterial', 'Cod', '628')
        stock = lookup('Gestiune', 'Cod', 'MAG1')['ID']
        vat_account = lookup('Cont', 'Simbol', '4426')['ID']
        try:
            for month in range(1, 9):
                pid = uuid.uuid4(); periods.append(pid)
                conn.execute('INSERT INTO "PerioadeFiscale" ("ID","An","Luna","Inchisa") VALUES (%s,%s,%s,false)', (pid, year, month))
            supplier = call('Admin', '/api/odata/Partener', 'POST', {'Cod': tag, 'Denumire': tag, 'Tara': 'RO', 'InregistratTva': True}, 201)['ID']
            material = call('Admin', '/api/odata/TipMaterial', 'POST', {'Cod': tag, 'Denumire': tag,
                'ClasaId': service['ClasaId'], 'ContImplicitId': service['ContImplicitId'], 'RegularizareAvans': True}, 201)['ID']
            for rate in [19, 21]:
                tip = call('Admin', '/api/odata/TipTva', 'POST', {'Cod': tag + str(rate), 'Denumire': tag,
                    'Regim': 'Normal', 'Cota': rate, 'ContTvaDeductibilId': vat_account, 'Activ': True}, 201)['ID']
                types.append(tip)
            call('Admin', f'/api/odata/TipTva({types[0]})', 'PATCH',
                {'ValabilDeLa': '2025-08-01', 'ValabilPanaLa': '2025-07-31'}, 422)
            d = create(100, 19)
            assert d['Linii'][0]['TvaCules'] is True
            call('Configurator', f'/api/odata/TipTva({types[0]})', 'PATCH', {'Cota': 21}, 204)
            assert any(r['DocumentId'] == d['Id'] and r['Cod'] == 'TVA_TAXA_DIFERITA_DE_COTA' for r in report('Cititor'))
            call('Admin', '/api/fct/' + d['Id'], 'PUT', {
                'Numar': d['Numar'], 'Data': d['Data'], 'PredatorId': supplier, 'PrimitorId': stock,
                'Linii': [{'Id': d['Linii'][0]['Id'], 'TipMaterialId': service['ID'],
                    'TipTvaId': types[0], 'Cantitate': 1, 'PretUnitar': 100, 'ValoareTva': None}]})
            preserved = call('Admin', '/api/fct/' + d['Id'])
            assert preserved['Linii'][0]['TvaCules'] is True and preserved['Linii'][0]['ValoareTva'] == 19
            result = call('Admin', '/api/fct/' + d['Id'] + '/opereaza', 'POST', {})
            assert any(m.startswith('TVA_TAXA_DIFERITA_DE_COTA: linia 1 —') for m in result['Mesaje'])
            preserved = call('Admin', '/api/fct/' + d['Id'])
            assert preserved['Linii'][0]['TvaCules'] is True and preserved['Linii'][0]['ValoareTva'] == 19
            call('Admin', '/api/fct/' + d['Id'] + '/anuleaza', 'POST', {})
            print('PASS R6 HTTP: PUT cu baza neschimbată și TVA null păstrează taxa culeasă 19 inclusiv la operare la 21%', flush=True)
            recalc = f"/api/documente/{d['Id']}/recalculeaza-tva"
            selection = {'Linii': [d['Linii'][0]['Id']]}
            call(None, recalc, 'POST', selection, 401)
            call('User', recalc, 'POST', selection, 404)
            call('Cititor', recalc, 'POST', selection, 403)
            call('Admin', recalc, 'POST', {'Linii': []}, 422)
            call('Admin', recalc, 'POST', selection, 204)
            after = call('Admin', '/api/fct/' + d['Id'])
            assert after['Linii'][0]['ValoareTva'] == 21 and after['Linii'][0]['TvaCules'] is False
            call('User', '/api/proiectii/diagnostic-tva', expected=403)
            call(None, '/api/proiectii/diagnostic-tva', expected=401)
            print('PASS R6 HTTP: 401/404/403/422, recalcul explicit și marcaj citit', flush=True)
            call('Admin', f'/api/odata/TipTva({types[0]})', 'PATCH', {'Cota': 19}, 204)
            source = create(100, avans=True)
            call('Admin', '/api/fct/' + source['Id'] + '/opereaza', 'POST', {})
            final = create(-100, -21, source['Linii'][0]['Id'], True, types[1])
            result = call('Admin', '/api/fct/' + final['Id'] + '/opereaza', 'POST', {})
            assert any(m.startswith('TVA_AVANS_CALIFICARE_DIFERITA: linia 1 —') for m in result['Mesaje'])
            assert all(final['Linii'][0]['Id'] not in m for m in result['Mesaje'])
            assert all(source['Id'] not in m and source['Linii'][0]['Id'] not in m and '19%' not in m for m in result['Mesaje'])
            assert any(r['DocumentId'] == final['Id'] and r['Cod'] == 'TVA_AVANS_CALIFICARE_DIFERITA' for r in report())
            for kind, target, field, value in [
                ('Object', 'Atlas.Conta.BackOffice.Module.BusinessObjects.Document', 'Criteria', 'ID = {' + source['Id'] + '}'),
                ('Member', 'Atlas.Conta.BackOffice.Module.Cub.Postare', 'Members', 'Valoare'),
                ('Member', 'Atlas.Conta.BackOffice.Module.Cub.Postare', 'Members', 'CotaTva')]:
                role, user, permission, restriction = [uuid.uuid4() for _ in range(4)]
                name = tag + kind + str(len(roles))
                roles.append((role, user, permission, restriction, kind))
                conn.execute('INSERT INTO "PermissionPolicyRoleBase" ("ID","Name","Discriminator","IsAdministrative","CanEditModel","PermissionPolicy","IsAllowPermissionPriority") VALUES (%s,%s,%s,false,false,1,false)', (role, name, 'PermissionPolicyRole'))
                conn.execute('INSERT INTO "PermissionPolicyUser" ("ID","UserName","StoredPassword","Discriminator","IsActive","ChangePasswordOnFirstLogon","AccessFailedCount") SELECT %s,%s,"StoredPassword","Discriminator",true,false,0 FROM "PermissionPolicyUser" WHERE "UserName"=%s', (user, name, 'User'))
                conn.execute('INSERT INTO "PermissionPolicyRolePermissionPolicyUser" ("RolesID","UsersID") VALUES (%s,%s)', (role, user))
                conn.execute('INSERT INTO "PermissionPolicyTypePermissionObject" ("ID","RoleID","TargetTypeFullName") VALUES (%s,%s,%s)', (permission, role, target))
                conn.execute(f'INSERT INTO "PermissionPolicy{kind}PermissionsObject" ("ID","TypePermissionObjectID","ReadState","{field}") VALUES (%s,%s,0,%s)', (restriction, permission, value))
                conn.execute('INSERT INTO "PermissionPolicyUserLoginInfo" ("ID","UserForeignKey","LoginProviderName","ProviderUserKey") VALUES (%s,%s,%s,%s)', (uuid.uuid4(), user, 'Password', str(user)))
                login(name)
                hidden = report(name)
                if kind == 'Object':
                    own = [r for r in hidden if r['DocumentId'] == final['Id']]
                    assert any(r['Cod'] == 'TVA_AVANS_REFERINTA_INVALIDA' for r in own), own
                    assert not any(r['DocumentId'] == source['Id'] for r in hidden), hidden
                else:
                    assert not any(r['DocumentId'] in [source['Id'], final['Id']] for r in hidden), hidden
                print('PASS R6 HTTP: sursă/membru ascuns', kind, flush=True)
            draft_source = create(100, avans=True)
            reference = create(-50, source=draft_source['Linii'][0]['Id'], avans=True)
            call('Admin', '/api/fct/' + draft_source['Id'], 'DELETE', expected=422)
            assert call('Admin', '/api/fct/' + reference['Id'])['Linii'][0]['LinieAvansId'] == draft_source['Linii'][0]['Id']
            print('PASS R6 HTTP: FK NO ACTION, ștergere refuzată atomic cu 422', flush=True)
            if args.asteapta_browser:
                folder = Path(__file__).resolve().parents[3] / 'run-verificari' / 'r6'
                release = folder / 'browser-terminat'
                assert not release.exists()
                (folder / 'browser-fixture.json').write_text(json.dumps({
                    'Draft': d['Id'], 'Sursa': source['Id'], 'Finala': final['Id'],
                    'Referinta': reference['Id'], 'Tip': types[0], 'Tag': tag}), encoding='utf-8')
                print('READY R6 browser: fixture disponibil 20 minute', flush=True)
                deadline = time.monotonic() + 1200
                while not release.exists() and time.monotonic() < deadline: time.sleep(1)

        finally:
            for d in reversed(documents):
                state = call('Admin', '/api/fct/' + d)
                if state['Stare'] == 'Operat': call('Admin', '/api/fct/' + d + '/anuleaza', 'POST', {})
                call('Admin', '/api/fct/' + d, 'DELETE', expected=204)
            for entity, identifiers in [('TipTva', types), ('TipMaterial', [material]), ('Partener', [supplier])]:
                for ident in identifiers:
                    if ident: call('Admin', f'/api/odata/{entity}({ident})', 'DELETE', expected=200)
            for role, user, permission, restriction, kind in reversed(roles):
                conn.execute(f'DELETE FROM "PermissionPolicy{kind}PermissionsObject" WHERE "ID"=%s', (restriction,))
                conn.execute('DELETE FROM "PermissionPolicyTypePermissionObject" WHERE "ID"=%s', (permission,))
                conn.execute('DELETE FROM "PermissionPolicyRolePermissionPolicyUser" WHERE "RolesID"=%s', (role,))
                conn.execute('DELETE FROM "PermissionPolicyUserLoginInfo" WHERE "UserForeignKey"=%s', (user,))
                conn.execute('DELETE FROM "PermissionPolicyUser" WHERE "ID"=%s', (user,))
                conn.execute('DELETE FROM "PermissionPolicyRoleBase" WHERE "ID"=%s', (role,))
            conn.execute('DELETE FROM "RefuzuriSeed" WHERE "Cheie" LIKE %s', ('%' + tag + '%',))
            for pid in periods: conn.execute('DELETE FROM "PerioadeFiscale" WHERE "ID"=%s', (pid,))
            print('PASS R6 HTTP: fixture curățat', flush=True)


if __name__ == '__main__':
    main()
