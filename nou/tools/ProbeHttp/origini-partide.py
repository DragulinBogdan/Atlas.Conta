"""SC-D9B-10: originea ascunsă pe HTTP, valori vizibile păstrate, fără OData pentru proiecție."""
import argparse
import json
import urllib.error
import urllib.parse
import urllib.request
import uuid
import xml.etree.ElementTree as ET

import psycopg


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--host', required=True)
    parser.add_argument('--baza', required=True)
    args = parser.parse_args()
    assert args.baza.startswith('Atlas.Conta.Http.Privat.'), 'Cere o bază HTTP izolată.'
    tokens, documents, permissions = {}, [], []
    supplier = role = user = period = None
    name = 'SC-D9B-HTTP-' + uuid.uuid4().hex[:8]
    date = '1976-01-05'

    def request(who, path, method='GET', data=None):
        headers = {'Content-Type': 'application/json'}
        if who:
            headers['Authorization'] = 'Bearer ' + tokens[who]
        req = urllib.request.Request(args.host + path, method=method, headers=headers,
            data=json.dumps(data).encode() if data is not None else None)
        try:
            with urllib.request.urlopen(req, timeout=90) as response:
                raw = response.read().decode()
                return response.status, json.loads(raw) if raw[:1] in ('{', '[', '"') else raw
        except urllib.error.HTTPError as error:
            return error.code, error.read().decode()

    def call(who, path, method='GET', data=None):
        status, body = request(who, path, method, data)
        assert 200 <= status < 300, (method, path, status, str(body)[:1200])
        return body

    def login(who):
        tokens[who] = call(None, '/api/Authentication/Authenticate', 'POST', {'userName': who, 'password': ''})

    def lookup(entity, field, value):
        query = urllib.parse.urlencode({'$filter': f"{field} eq '{value}'"})
        return call('Admin', '/api/odata/' + entity + '?' + query)['value'][0]['ID']

    with psycopg.connect(host='127.0.0.1', port=5446, dbname=args.baza, user='postgres', password='postgres', autocommit=True) as conn:
        login('Admin')
        cont = lookup('Cont', 'Simbol', '401')
        assert conn.execute('SELECT count(*) FROM "Conturi" WHERE "ID"=%s', (uuid.UUID(cont),)).fetchone()[0] == 1
        assert conn.execute('SELECT count(*) FROM "PerioadeFiscale" WHERE "An"=1976').fetchone()[0] == 0
        try:
            period = uuid.uuid4()
            conn.execute('INSERT INTO "PerioadeFiscale" ("ID","An","Luna","Inchisa") VALUES (%s,1976,1,false)', (period,))
            supplier = call('Admin', '/api/odata/Partener', 'POST', {'Cod': name, 'Denumire': name, 'Tara': 'RO',
                'TipPersoana': 'Juridica', 'InregistratTva': False})['ID']
            invoice = call('Admin', '/api/fct', 'POST', {'Numar': name, 'Data': date, 'PredatorId': supplier,
                'PrimitorId': lookup('Gestiune', 'Cod', 'MAG1'), 'Linii': [{'TipMaterialId': lookup('TipMaterial', 'Cod', '628'),
                'TipTvaId': lookup('TipTva', 'Cod', 'SFD'), 'Cantitate': 1, 'PretUnitar': 100}]})['Id']
            documents.append(('fct', invoice))
            call('Admin', f'/api/fct/{invoice}/opereaza', 'POST', {})
            payment = call('Admin', '/api/plt', 'POST', {'Data': date, 'PredatorId': lookup('ContPropriu', 'Cod', 'CASA'),
                'PrimitorId': supplier, 'TipInstrument': 'DispozitieCasa',
                'Linii': [{'TipMaterialId': lookup('TipMaterial', 'Cod', 'TRZ'), 'Valoare': 40}]})['Id']
            documents.append(('plt', payment))
            call('Admin', f'/api/plt/{payment}/opereaza', 'POST', {})
            call('Admin', '/api/imperecheri', 'POST', {'DocumentStingatorId': payment, 'DocumentId': invoice, 'Suma': 40, 'Data': date})
            unit = str(conn.execute('SELECT "ID" FROM "OriginiPartide" WHERE "DocumentId"=%s', (uuid.UUID(invoice),)).fetchone()[0])
            role, user = uuid.uuid4(), uuid.uuid4()
            with conn.transaction():
                conn.execute('INSERT INTO "PermissionPolicyRoleBase" ("ID","Name","Discriminator","IsAdministrative","CanEditModel","PermissionPolicy","IsAllowPermissionPriority") VALUES (%s,%s,\'PermissionPolicyRole\',false,false,1,false)', (role, name))
                conn.execute('INSERT INTO "PermissionPolicyUser" ("ID","UserName","StoredPassword","Discriminator","IsActive","ChangePasswordOnFirstLogon","AccessFailedCount") SELECT %s,%s,"StoredPassword","Discriminator",true,false,0 FROM "PermissionPolicyUser" WHERE "UserName"=\'User\'', (user, name))
                conn.execute('INSERT INTO "PermissionPolicyRolePermissionPolicyUser" ("RolesID","UsersID") VALUES (%s,%s)', (role, user))
                conn.execute('INSERT INTO "PermissionPolicyUserLoginInfo" ("ID","UserForeignKey","LoginProviderName","ProviderUserKey") VALUES (%s,%s,\'Password\',%s)', (uuid.uuid4(), user, str(user)))
                for target, criterion in [('Cub.Postare', 'DocumentId = {' + invoice + '}'), ('BusinessObjects.Document', 'ID = {' + invoice + '}')]:
                    permission, restriction = uuid.uuid4(), uuid.uuid4()
                    permissions.append((permission, restriction))
                    conn.execute('INSERT INTO "PermissionPolicyTypePermissionObject" ("ID","RoleID","TargetTypeFullName") VALUES (%s,%s,%s)',
                        (permission, role, 'Atlas.Conta.BackOffice.Module.' + target))
                    conn.execute('INSERT INTO "PermissionPolicyObjectPermissionsObject" ("ID","TypePermissionObjectID","ReadState","Criteria") VALUES (%s,%s,0,%s)', (restriction, permission, criterion))
            login(name)
            assert request(name, f'/api/fct/{invoice}')[0] == 404
            assert request(name, f'/api/plt/{payment}')[0] == 200
            path = f'/api/proiectii/partide?laData={date}&contrapartidaId={supplier}'
            for who, rest, doc in [('Admin', 60, invoice), (name, 40, None)]:
                rows = call(who, path)['data']
                row = next(r for r in rows if r['UnitateId'].lower() == unit)
                assert row['Rest'] == rest and row['DocumentId'] == doc, row
                if doc is None:
                    assert row.get('Numar') is None, row
                print('PASS SC-D9B-10 HTTP', 'admin' if who == 'Admin' else 'restrictiv', rest, flush=True)
            metadata = call('Admin', '/api/odata/$metadata')
            sets = {node.attrib['Name'] for node in ET.fromstring(metadata).iter()
                if node.tag.endswith('}EntitySet') and node.attrib.get('EntityType', '').endswith('.OriginePartidaPersistata')}
            for entity in sorted(sets | {'OriginePartidaPersistata', 'OriginiPartide'}):
                for method, suffix, body in [('GET', '', None), ('POST', '', {'ID': str(uuid.uuid4())}),
                        ('PATCH', f'({unit})', {'PartenerId': supplier}), ('DELETE', f'({unit})', None)]:
                    status, _ = request('Admin', '/api/odata/' + entity + suffix, method, body)
                    assert status in (404, 405), (entity, method, status)
                    print('PASS SC-D9B-10 fără endpoint OData', entity, method, status, flush=True)
            assert conn.execute('SELECT count(*) FROM "OriginiPartide" WHERE "DocumentId"=%s', (uuid.UUID(invoice),)).fetchone()[0] == 1
        finally:
            for kind, doc in reversed(documents):
                state = call('Admin', f'/api/{kind}/{doc}')
                for link in call('Admin', f'/api/imperecheri/{doc}/stingeri')['Imperecheri']:
                    call('Admin', '/api/imperecheri/' + link['Id'], 'DELETE')
                if state['Stare'] == 'Operat':
                    call('Admin', f'/api/{kind}/{doc}/anuleaza', 'POST', {})
                call('Admin', f'/api/{kind}/{doc}', 'DELETE')
            if supplier:
                call('Admin', f'/api/odata/Partener({supplier})', 'DELETE')
            with conn.transaction():
                for permission, restriction in reversed(permissions):
                    conn.execute('DELETE FROM "PermissionPolicyObjectPermissionsObject" WHERE "ID"=%s', (restriction,))
                    conn.execute('DELETE FROM "PermissionPolicyTypePermissionObject" WHERE "ID"=%s', (permission,))
                if role:
                    conn.execute('DELETE FROM "PermissionPolicyRolePermissionPolicyUser" WHERE "RolesID"=%s', (role,))
                    conn.execute('DELETE FROM "PermissionPolicyUserLoginInfo" WHERE "UserForeignKey"=%s', (user,))
                    conn.execute('DELETE FROM "PermissionPolicyUser" WHERE "ID"=%s', (user,))
                    conn.execute('DELETE FROM "PermissionPolicyRoleBase" WHERE "ID"=%s', (role,))
                if period:
                    conn.execute('DELETE FROM "PerioadeFiscale" WHERE "ID"=%s AND NOT "Inchisa"', (period,))
            for _, doc in documents:
                assert conn.execute('SELECT count(*) FROM "OriginiPartide" WHERE "DocumentId"=%s', (uuid.UUID(doc),)).fetchone()[0] == 0
            print('PASS SC-D9B-10 curățenie origini', flush=True)


if __name__ == '__main__':
    main()
