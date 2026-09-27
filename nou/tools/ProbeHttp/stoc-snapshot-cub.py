"""SC-CIT-74: raportul de stoc respectă permisiunile peste închidere și reconstrucție.

Necesită psycopg și hostul privat pornit pe o bază izolată cu seed și utilizatori.
Datele și rolurile temporare sunt curățate în finally; auditul HTTP rămâne.
"""
import argparse
import json
import urllib.error
import urllib.parse
import urllib.request
import uuid

import psycopg


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--host', default='http://127.0.0.1:5089')
    parser.add_argument('--baza', required=True, help='Baza izolată a hostului; fără perioade închise.')
    args = parser.parse_args()
    tokens, roles, periods = {}, [], []
    document = None
    suppliers, products, lots = [], [], []
    operated = closed = False
    year = 2000
    dsn = f'host=localhost port=5444 dbname={args.baza} user=postgres password=postgres'

    def call(user, path, method='GET', data=None):
        headers = {'Content-Type': 'application/json'}
        if user:
            headers['Authorization'] = 'Bearer ' + tokens[user]
        request = urllib.request.Request(args.host.rstrip('/') + path,
            data=json.dumps(data).encode() if data is not None else None,
            headers=headers, method=method)
        try:
            with urllib.request.urlopen(request, timeout=90) as response:
                body = response.read().decode()
                return (json.loads(body) if body[:1] in ('{', '[', '"') else body) if body else None
        except urllib.error.HTTPError as error:
            raise RuntimeError(f'{method} {path}: {error.code}: {error.read().decode()[:1500]}') from error

    def login(name):
        tokens[name] = call(None, '/api/Authentication/Authenticate', 'POST',
            {'userName': name, 'password': ''})

    def lookup(entity, field, value):
        query = urllib.parse.urlencode({'$filter': f"{field} eq '{value}'"})
        return call('Admin', '/api/odata/' + entity + '?' + query)['value'][0]['ID']

    with psycopg.connect(dsn, autocommit=True) as conn:
        assert conn.execute('SELECT current_database()').fetchone()[0] == args.baza
        assert conn.execute('SELECT count(*) FROM "PerioadeFiscale" WHERE "An"=%s', (year,)).fetchone()[0] == 0
        assert conn.execute('SELECT count(*) FROM "PerioadeFiscale" WHERE "Inchisa"').fetchone()[0] == 0
        for user in ('Admin', 'Cititor', 'User'):
            login(user)
        marker = lookup('Cont', 'Simbol', '628')
        assert conn.execute('SELECT count(*) FROM "Conturi" WHERE "ID"=%s AND "Simbol"=%s',
            (uuid.UUID(marker), '628')).fetchone()[0] == 1, 'Hostul nu folosește baza izolată a probei.'
        try:
            for month in (1, 2):
                period_id = uuid.uuid4()
                periods.append(period_id)
                conn.execute('INSERT INTO "PerioadeFiscale" ("ID","An","Luna","Inchisa") VALUES (%s,%s,%s,false)',
                    (period_id, year, month))
            stock = lookup('Gestiune', 'Cod', 'MAG1')
            tip = lookup('TipMaterial', 'Cod', '371')
            tva = lookup('TipTva', 'Cod', 'SFD')
            supplier = call('Admin', '/api/odata/Partener', 'POST',
                {'Cod':'SC-CIT74-' + uuid.uuid4().hex[:8], 'Denumire':'Furnizor snapshot stoc'})['ID']
            suppliers.append(supplier)
            lines = []
            for q, price in ((10, 10), (5, 5)):
                product = call('Admin', '/api/odata/Produs', 'POST',
                    {'Cod':'SC-CIT74-' + uuid.uuid4().hex[:8], 'Denumire':'Produs snapshot stoc',
                     'UM':'BUC', 'TipMaterialId':tip})['ID']
                products.append(product)
                lines.append({'TipMaterialId':tip, 'TipTvaId':tva, 'ProdusId':product, 'Cantitate':q, 'PretUnitar':price})
            result = call('Admin', '/api/fct', 'POST', {'Numar':'SC-CIT74', 'Data':f'{year}-01-15',
                'PredatorId':supplier, 'PrimitorId':stock, 'Linii':lines})
            document = result['Id']
            lots = [r['LotId'] for r in call('Admin', f'/api/fct/{document}')['Linii']]
            call('Admin', f'/api/fct/{document}/opereaza', 'POST', {})
            operated = True
            users = [('Admin', 125), ('Cititor', 125), ('User', 0)]
            for kind in ('Object', 'Member'):
                role, user, permission, restriction = [uuid.uuid4() for _ in range(4)]
                name = 'CodexCIT74-' + kind + '-' + user.hex[:8]
                roles.append((role, user, permission, restriction, kind))
                with conn.transaction():
                    conn.execute('INSERT INTO "PermissionPolicyRoleBase" ("ID","Name","Discriminator","IsAdministrative","CanEditModel","PermissionPolicy","IsAllowPermissionPriority") VALUES (%s,%s,%s,false,false,1,false)', (role, name, 'PermissionPolicyRole'))
                    conn.execute('INSERT INTO "PermissionPolicyUser" ("ID","UserName","StoredPassword","Discriminator","IsActive","ChangePasswordOnFirstLogon","AccessFailedCount") SELECT %s,%s,"StoredPassword","Discriminator",true,false,0 FROM "PermissionPolicyUser" WHERE "UserName"=%s', (user, name, 'User'))
                    conn.execute('INSERT INTO "PermissionPolicyRolePermissionPolicyUser" ("RolesID","UsersID") VALUES (%s,%s)', (role, user))
                    conn.execute('INSERT INTO "PermissionPolicyTypePermissionObject" ("ID","RoleID","TargetTypeFullName") VALUES (%s,%s,%s)', (permission, role, 'Atlas.Conta.BackOffice.Module.Cub.Postare'))
                    field, value = ('Criteria', 'Valoare = 25') if kind == 'Object' else ('Members', 'Valoare')
                    conn.execute(f'INSERT INTO "PermissionPolicy{kind}PermissionsObject" ("ID","TypePermissionObjectID","ReadState","{field}") VALUES (%s,%s,0,%s)', (restriction, permission, value))
                    conn.execute('INSERT INTO "PermissionPolicyUserLoginInfo" ("ID","UserForeignKey","LoginProviderName","ProviderUserKey") VALUES (%s,%s,%s,%s)', (uuid.uuid4(), user, 'Password', str(user)))
                login(name)
                users.append((name, 100 if kind == 'Object' else 0))

            def verify(stage):
                for name, expected in users:
                    rows = call(name, f'/api/proiectii/sold-stoc?laData={year}-02-28')['data']
                    assert sum(r['Valoare'] for r in rows) == expected, (stage, name, rows)
                    expected_q = 0 if name == 'User' else 10 if expected == 100 else 15
                    assert sum(r['Cantitate'] for r in rows) == expected_q, (stage, name, rows)
                    if name == 'User': assert not rows
                    else:
                        assert {r['LotId'] for r in rows}.issubset(set(lots))
                        assert all(r['LotData'] == f'{year}-01-15' and r['ContSimbol'] == '371' for r in rows)
                    print('PASS SC-CIT-74', stage, name.split('-')[0:2], expected_q, expected, flush=True)

            verify('înainte de închidere')
            findings = call('Admin', f'/api/perioade/{year}/1/verificare')
            call('Admin', f'/api/perioade/{year}/1/inchide', 'POST', {'Acceptate': [f['Cheie'] for f in findings]})
            closed = True
            assert conn.execute('SELECT count(*) FROM "SolduriPerioadaStoc" WHERE "An"=%s AND "Luna"=1', (year,)).fetchone()[0] > 0
            verify('după închidere')
            call('Admin', '/api/perioade/reconstruieste', 'POST', {})
            verify('după reconstrucție')
        finally:
            errors = []
            def cleanup(action):
                try:
                    action()
                except Exception as error:
                    errors.append(str(error))
            if closed:
                cleanup(lambda: call('Admin', f'/api/perioade/{year}/1/redeschide', 'POST', {'Motiv': 'Curățarea SC-CIT-74'}))
            if operated:
                cleanup(lambda: call('Admin', f'/api/fct/{document}/anuleaza', 'POST', {}))
            if document:
                cleanup(lambda: call('Admin', f'/api/fct/{document}', 'DELETE'))
            for supplier in reversed(suppliers):
                cleanup(lambda supplier=supplier: call('Admin', f'/api/odata/Partener/{supplier}', 'DELETE'))
            for product in reversed(products):
                cleanup(lambda product=product: call('Admin', f'/api/odata/Produs/{product}', 'DELETE'))
            with conn.transaction():
                for role, user, permission, restriction, kind in reversed(roles):
                    conn.execute(f'DELETE FROM "PermissionPolicy{kind}PermissionsObject" WHERE "ID"=%s', (restriction,))
                    conn.execute('DELETE FROM "PermissionPolicyTypePermissionObject" WHERE "ID"=%s', (permission,))
                    conn.execute('DELETE FROM "PermissionPolicyRolePermissionPolicyUser" WHERE "RolesID"=%s', (role,))
                    conn.execute('DELETE FROM "PermissionPolicyUserLoginInfo" WHERE "UserForeignKey"=%s', (user,))
                    conn.execute('DELETE FROM "PermissionPolicyUser" WHERE "ID"=%s', (user,))
                    conn.execute('DELETE FROM "PermissionPolicyRoleBase" WHERE "ID"=%s', (role,))
                if not errors:
                    for period_id in reversed(periods):
                        conn.execute('DELETE FROM "InchideriPerioade" WHERE "PerioadaId"=%s', (period_id,))
                        conn.execute('DELETE FROM "PerioadeFiscale" WHERE "ID"=%s AND NOT "Inchisa"', (period_id,))
            if errors:
                raise RuntimeError('Curățare incompletă: ' + '; '.join(errors))


if __name__ == '__main__':
    main()
