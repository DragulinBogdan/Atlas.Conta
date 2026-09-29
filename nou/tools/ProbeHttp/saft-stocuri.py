"""SC-SAFT-48: D406 S pe cub pe host privat izolat; accesul complet, mișcarea FCT și refuzul categoriei lipsă."""
import argparse
import json
import urllib.error
import urllib.parse
import urllib.request
import uuid
import xml.etree.ElementTree as ET
import psycopg

POSTARE = 'Atlas.Conta.BackOffice.Module.Cub.Postare'
PARTENER = 'Atlas.Conta.BackOffice.Module.BusinessObjects.Partener'


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--host', default='http://127.0.0.1:5091')
    parser.add_argument('--baza', required=True)
    args = parser.parse_args()
    tokens, roles, periods, documents = {}, [], [], []
    partner = product = None
    year = 2024
    marker = 'SAFT-S3-' + uuid.uuid4().hex[:6]
    dsn = f'host=localhost port=5444 dbname={args.baza} user=postgres password=postgres'

    def call(user, path, method='GET', data=None, expected=200):
        headers = {'Content-Type': 'application/json'}
        if user:
            headers['Authorization'] = 'Bearer ' + tokens[user]
        request = urllib.request.Request(args.host.rstrip('/') + path,
            data=json.dumps(data).encode() if data is not None else None, headers=headers, method=method)
        try:
            with urllib.request.urlopen(request, timeout=90) as response:
                status, body = response.status, response.read().decode()
        except urllib.error.HTTPError as error:
            status, body = error.code, error.read().decode()
        assert status == expected, (method, path, user, status, body[:1800])
        return (json.loads(body) if body[:1] in ('{', '[', '"') else body) if body else None

    def login(name):
        tokens[name] = call(None, '/api/Authentication/Authenticate', 'POST', {'userName': name, 'password': ''})

    def lookup(entity, field, value):
        q = urllib.parse.urlencode({'$filter': f"{field} eq '{value}'"})
        return call('Admin', '/api/odata/' + entity + '?' + q)['value'][0]['ID']

    sumar = f'/api/proiectii/saft/stocuri?an={year}&luna=1'
    xml = f'/api/proiectii/saft/stocuri/xml?an={year}&luna=1'

    with psycopg.connect(dsn, autocommit=True) as conn:
        assert conn.execute('SELECT current_database()').fetchone()[0] == args.baza
        assert conn.execute('SELECT count(*) FROM "PerioadeFiscale" WHERE "An"=%s', (year,)).fetchone()[0] == 0
        assert conn.execute('SELECT count(*) FROM "Conturi" WHERE "CategorieStoc" IS NOT NULL').fetchone()[0] > 0, \
            'baza nu are categoriile de stoc seed-uite'
        for name in ('Admin', 'Cititor', 'User'):
            login(name)
        society = conn.execute('SELECT "ID","CodFiscal" FROM "Societati"').fetchone()
        conn.execute('UPDATE "Societati" SET "CodFiscal"=%s WHERE "ID"=%s', ('RO12345674', society[0]))
        cont37 = conn.execute('SELECT "ID","CategorieStoc","DinSeed" FROM "Conturi" WHERE "Simbol"=%s', ('37',)).fetchone()
        try:
            for month in (1, 2):
                pid = uuid.uuid4(); periods.append(pid)
                conn.execute('INSERT INTO "PerioadeFiscale" ("ID","An","Luna","Inchisa") VALUES (%s,%s,%s,false)', (pid, year, month))
            partner = call('Admin', '/api/odata/Partener', 'POST', {
                'Cod': marker, 'Denumire': 'Furnizor SAF-T S3', 'Tara': 'RO', 'CodFiscal': 'RO12345674',
                'InregistratTva': True}, 201)['ID']
            stock, tip, tva = lookup('Gestiune', 'Cod', 'MAG1'), lookup('TipMaterial', 'Cod', '371'), lookup('TipTva', 'Cod', 'N21')
            product = call('Admin', '/api/odata/Produs', 'POST', {
                'Cod': marker + '-P', 'Denumire': 'Marfă SAF-T S3', 'UM': 'BUC', 'TipMaterialId': tip}, 201)['ID']
            fct = call('Admin', '/api/fct', 'POST', {'Numar': marker + '-F', 'Data': f'{year}-01-10', 'PredatorId': partner,
                'PrimitorId': stock, 'Linii': [{'TipMaterialId': tip, 'ProdusId': product, 'TipTvaId': tva,
                'Cantitate': 10, 'PretUnitar': 10}]}, 201)['Id']
            documents.append(('fct', fct))
            call('Admin', f'/api/fct/{fct}/opereaza', 'POST', {})

            for kind, target, field, value, member in (
                    ('Rand', POSTARE, 'Criteria', '[Valoare] > 100', None),
                    ('Membru', POSTARE, 'Members', 'Valoare', 'Valoare'),
                    ('Metadate', PARTENER, 'Criteria', f"StartsWith([Cod], '{marker}')", None)):
                role, user, permission, restriction = [uuid.uuid4() for _ in range(4)]
                name = 'SaftS3-' + kind + '-' + user.hex[:6]
                table = 'Member' if member else 'Object'
                roles.append((role, user, permission, restriction, table))
                with conn.transaction():
                    conn.execute('INSERT INTO "PermissionPolicyRoleBase" ("ID","Name","Discriminator","IsAdministrative","CanEditModel","PermissionPolicy","IsAllowPermissionPriority") VALUES (%s,%s,%s,false,false,1,false)', (role, name, 'PermissionPolicyRole'))
                    conn.execute('INSERT INTO "PermissionPolicyUser" ("ID","UserName","StoredPassword","Discriminator","IsActive","ChangePasswordOnFirstLogon","AccessFailedCount") SELECT %s,%s,"StoredPassword","Discriminator",true,false,0 FROM "PermissionPolicyUser" WHERE "UserName"=%s', (user, name, 'User'))
                    conn.execute('INSERT INTO "PermissionPolicyRolePermissionPolicyUser" ("RolesID","UsersID") VALUES (%s,%s)', (role, user))
                    conn.execute('INSERT INTO "PermissionPolicyTypePermissionObject" ("ID","RoleID","TargetTypeFullName") VALUES (%s,%s,%s)', (permission, role, target))
                    conn.execute(f'INSERT INTO "PermissionPolicy{table}PermissionsObject" ("ID","TypePermissionObjectID","ReadState","{field}") VALUES (%s,%s,0,%s)', (restriction, permission, value))
                    conn.execute('INSERT INTO "PermissionPolicyUserLoginInfo" ("ID","UserForeignKey","LoginProviderName","ProviderUserKey") VALUES (%s,%s,%s,%s)', (uuid.uuid4(), user, 'Password', str(user)))
                login(name)
                for path in (sumar, xml):
                    refused = json.dumps(call(name, path, expected=403), ensure_ascii=False)
                    expected_type = 'Partener' if target == PARTENER else 'Postare' + ('.' + member if member else '')
                    assert 'SAFT_ACCES_INCOMPLET' in refused and expected_type in refused, (name, path, refused)
                    assert '100.00' not in refused and '100,00' not in refused, (name, path, refused)
                print('PASS SC-SAFT-48', kind, '403 SAFT_ACCES_INCOMPLET pe sumarul și fișierul S, fără sume', flush=True)

            for path in (sumar, xml):
                refused = json.dumps(call('User', path, expected=403), ensure_ascii=False)
                assert 'SAFT_ACCES_INCOMPLET' in refused, refused
            print('PASS SC-SAFT-48 User fără drept pe Postare: 403 pe ambele uși S', flush=True)

            for name in ('Admin', 'Cititor'):
                summary = call(name, sumar)
                assert summary['Refuzuri'] == [] and summary['MiscariStoc'] >= 1 and summary['StocFizic'] >= 1, (name, summary)
                root = ET.fromstring(call(name, xml))
                ns = {'s': root.tag.split('}')[0][1:]}
                assert root.findtext('.//s:Header/s:HeaderComment', None, ns) == 'C'
                mine = [m for m in root.findall('.//s:MovementOfGoods/s:StockMovement', ns)
                        if m.findtext('s:DocumentReference/s:DocumentNumber', None, ns) == marker + '-F']
                assert len(mine) == 1 and mine[0].findtext('s:MovementType', None, ns) == '10', (name, len(mine))
                line = mine[0].find('s:StockMovementLine', ns)
                assert float(line.findtext('s:Quantity', None, ns)) == 10 and float(line.findtext('s:BookValue', None, ns)) == 100
                assert line.findtext('s:ShipTo/s:WarehouseID', None, ns) == 'MAG1' and line.findtext('s:AccountID', None, ns) == '371'
                print('PASS SC-SAFT-48', name, 'acces complet: 200, FCT 10 +10/+100 pe 371, ShipTo MAG1', flush=True)

            conn.execute('UPDATE "Conturi" SET "CategorieStoc"=NULL WHERE "ID"=%s', (cont37[0],))
            summary = call('Admin', sumar)
            assert any(r['Cod'] == 'SAFT_CATEGORIE_LIPSA' and '371' in r['Mesaj'] for r in summary['Refuzuri']), summary['Refuzuri']
            refused = json.dumps(call('Admin', xml, expected=422), ensure_ascii=False)
            assert 'SAFT_CATEGORIE_LIPSA' in refused, refused
            print('PASS SC-SAFT-48 categorie lipsă: sumar 200 cu refuzul, fișier 422 cu EroriDto', flush=True)
        finally:
            conn.execute('UPDATE "Conturi" SET "CategorieStoc"=%s, "DinSeed"=%s WHERE "ID"=%s', (cont37[1], cont37[2], cont37[0]))
            for role, user, permission, restriction, table in roles:
                with conn.transaction():
                    conn.execute('DELETE FROM "PermissionPolicyUserLoginInfo" WHERE "UserForeignKey"=%s', (user,))
                    conn.execute(f'DELETE FROM "PermissionPolicy{table}PermissionsObject" WHERE "ID"=%s', (restriction,))
                    conn.execute('DELETE FROM "PermissionPolicyTypePermissionObject" WHERE "ID"=%s', (permission,))
                    conn.execute('DELETE FROM "PermissionPolicyRolePermissionPolicyUser" WHERE "UsersID"=%s', (user,))
                    conn.execute('DELETE FROM "PermissionPolicyUser" WHERE "ID"=%s', (user,))
                    conn.execute('DELETE FROM "PermissionPolicyRoleBase" WHERE "ID"=%s', (role,))
            for kind, doc in reversed(documents):
                call('Admin', f'/api/{kind}/{doc}/anuleaza', 'POST', {})
                call('Admin', f'/api/{kind}/{doc}', 'DELETE', expected=204)
            if product:
                conn.execute('DELETE FROM "Loturi" WHERE "ProdusId"=%s', (product,))
                conn.execute('DELETE FROM "Produse" WHERE "ID"=%s', (product,))
            if partner:
                call('Admin', f'/api/odata/Partener({partner})', 'DELETE')
            for pid in periods:
                conn.execute('DELETE FROM "PerioadeFiscale" WHERE "ID"=%s', (pid,))
            conn.execute('UPDATE "Societati" SET "CodFiscal"=%s WHERE "ID"=%s', (society[1], society[0]))


if __name__ == '__main__':
    main()
