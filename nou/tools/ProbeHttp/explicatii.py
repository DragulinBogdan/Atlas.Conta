"""SC-CIT-99: explicația deciziei pe ușa HTTP — acces complet, refuzuri, storno.

Necesită psycopg și hostul privat pornit pe o clonă de unică folosință, cu seed și utilizatori.
Rolurile temporare sunt curățate în finally; documentele rămân (bonul e stornat), deci baza nu se refolosește.
"""
import argparse
import json
import urllib.error
import urllib.parse
import urllib.request
import uuid

import psycopg

POSTARE = 'Atlas.Conta.BackOffice.Module.Cub.Postare'
DETALIU = 'Atlas.Conta.BackOffice.Module.BusinessObjects.DocumentDetaliu'
REFUZ = 'EXPLICATIE_ACCES_INCOMPLET'


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--host', default='http://127.0.0.1:5089')
    parser.add_argument('--baza', required=True, help='Clona de unică folosință a hostului.')
    args = parser.parse_args()
    tokens, roles = {}, []
    year = 2012
    marker = 'SC-CIT99-' + uuid.uuid4().hex[:6]
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

    with psycopg.connect(dsn, autocommit=True) as conn:
        assert conn.execute('SELECT current_database()').fetchone()[0] == args.baza
        assert conn.execute('SELECT count(*) FROM "PerioadeFiscale" WHERE "An"=%s', (year,)).fetchone()[0] == 0
        for name in ('Admin', 'Cititor', 'User'):
            login(name)
        marker_cont = lookup('Cont', 'Simbol', '628')
        assert conn.execute('SELECT count(*) FROM "Conturi" WHERE "ID"=%s AND "Simbol"=%s',
            (uuid.UUID(marker_cont), '628')).fetchone()[0] == 1, 'Hostul nu folosește baza probei.'
        try:
            conn.execute('INSERT INTO "PerioadeFiscale" ("ID","An","Luna","Inchisa") VALUES (%s,%s,1,false)',
                (uuid.uuid4(), year))
            stock, place = lookup('Gestiune', 'Cod', 'MAG1'), lookup('UnitateInterna', 'Cod', 'SEDIU')
            tip, tva = lookup('TipMaterial', 'Cod', '302'), lookup('TipTva', 'Cod', 'SFD')
            supplier = call('Admin', '/api/odata/Partener', 'POST', {'Cod': marker, 'Denumire': 'Furnizor explicații'}, 201)['ID']
            lines = []
            for q, price in ((10, 10), (5, 5)):
                product = call('Admin', '/api/odata/Produs', 'POST', {'Cod': marker + '-' + str(price),
                    'Denumire': 'Produs explicații', 'UM': 'BUC', 'TipMaterialId': tip}, 201)['ID']
                lines.append({'TipMaterialId': tip, 'TipTvaId': tva, 'ProdusId': product, 'Cantitate': q, 'PretUnitar': price})
            fct = call('Admin', '/api/fct', 'POST', {'Numar': marker, 'Data': f'{year}-01-05',
                'PredatorId': supplier, 'PrimitorId': stock, 'Linii': lines}, 201)['Id']
            lots = [r['LotId'] for r in call('Admin', f'/api/fct/{fct}')['Linii']]
            conex = call('Admin', f'/api/fct/{fct}/opereaza', 'POST', {}).get('ConexId')
            if conex:
                call('Admin', f'/api/nir/{conex}/opereaza', 'POST', {})
            bcs = call('Admin', '/api/bcs', 'POST', {'Data': f'{year}-01-10', 'PredatorId': stock, 'PrimitorId': place,
                'Linii': [{'TipMaterialId': tip, 'LotId': lots[0], 'Cantitate': 4},
                          {'TipMaterialId': tip, 'LotId': lots[1], 'Cantitate': 1}]}, 201)['Id']
            assert call('Admin', f'/api/bcs/{bcs}/valideaza', 'POST', {})['Erori'] == []
            assert conn.execute('SELECT count(*) FROM "Tranzactie" WHERE "DocumentId"=%s', (uuid.UUID(bcs),)).fetchone()[0] == 0
            print('PASS SC-CIT-99 dry-run-ul nu persistă nimic', flush=True)
            call('Admin', f'/api/bcs/{bcs}/opereaza', 'POST', {})
            operare = str(conn.execute('SELECT "ID" FROM "Tranzactie" WHERE "DocumentId"=%s AND "Fel"=1',
                (uuid.UUID(bcs),)).fetchone()[0])
            path = f'/api/proiectii/explicatii/{operare}'

            for name in ('Admin', 'Cititor'):
                e = call(name, path)
                origin, = e['Origini']
                first, second = origin['Linii']
                assert e['Fel'] == 'Operare' and e['DocumentId'] == bcs and origin['PurtatorId'] == operare, e
                assert origin['Declarant'] == 'DeclarantBonConsum' and origin['Versiune'] == 2, origin
                assert origin['Politici'] and all(p['Fel'] == 'RegulaContare' and not p['Schimbata']
                    and p['VersiuneCurenta'] == p['Versiune'] for p in origin['Politici']), origin['Politici']
                assert all(c['RegulaId'] for line in origin['Linii'] for c in line['Conturi']), origin['Linii']
                assert (origin['PerioadaAn'], origin['PerioadaLuna']) == (year, 1), origin
                out1, out2 = first['Iesiri'][0], second['Iesiri'][0]
                assert (out1['Unitate']['Id'], out1['Cantitate'], out1['Valoare']) == (lots[0], 4, 40), out1
                assert (out1['SoldInainte']['Debit'], out1['SoldInainte']['Cantitate']) == (100, 10), out1
                assert (out2['Unitate']['Id'], out2['Cantitate'], out2['Valoare']) == (lots[1], 1, 5), out2
                assert (out2['SoldInainte']['Debit'], out2['SoldInainte']['Cantitate']) == (25, 5), out2
                assert out1['Sursa'] is None and len(first['Conturi']) == 2, first
                print('PASS SC-CIT-99', name, 'acces complet: 200, 4 din 10/100 = 40 și 1 din 5/25 = 5', flush=True)

            # D9-A8: regula editată pe ușa OData a hostului își crește contorul; explicația veche o arată „schimbată”.
            retinuta, = call('Admin', path)['Origini'][0]['Politici']
            rand = f"/api/odata/RegulaContare({retinuta['RandId']})"
            # Contul creditor explicit e rezervă pe sursa TipMaterial: editarea lui nu schimbă postările.
            regula = call('Admin', rand)
            initial = regula['ContCreditId']
            assert initial is None and regula['SursaContCredit'] == 'TipMaterial' and regula['ContDebitId'], regula
            for valoare in (regula['ContDebitId'], initial):
                request = urllib.request.Request(args.host.rstrip('/') + rand, data=json.dumps({'ContCreditId': valoare}).encode(),
                    headers={'Content-Type': 'application/json', 'Authorization': 'Bearer ' + tokens['Admin']}, method='PATCH')
                with urllib.request.urlopen(request, timeout=90) as response:
                    assert response.status in (200, 204), response.status
            dupa, = call('Admin', path)['Origini'][0]['Politici']
            assert call('Admin', rand)['ContCreditId'] == initial
            assert dupa['Schimbata'] and dupa['Versiune'] == retinuta['Versiune'] \
                and dupa['VersiuneCurenta'] == retinuta['Versiune'] + 2, (retinuta, dupa)
            print('PASS SC-CIT-111 regula editată de două ori pe OData: contorul curent +2, explicația veche „schimbată”', flush=True)

            for name, target in (('User', path), ('Admin', f'/api/proiectii/explicatii/{uuid.uuid4()}')):
                refused = json.dumps(call(name, target, expected=404), ensure_ascii=False)
                assert 'nu există sau nu e vizibil' in refused and 'Iesiri' not in refused, refused
            print('PASS SC-CIT-99 tranzacție invizibilă sau inexistentă: același 404', flush=True)

            for kind, target, field, value, expected_type in (
                    ('Membru', POSTARE, 'Members', 'Valoare', 'Postare.Valoare'),
                    ('Linie', DETALIU, 'Criteria', '[Cantitate] = 1', 'DocumentDetaliu'),
                    ('Istoric', POSTARE, 'Criteria', f'[Data] < #{year}-01-08#', 'Postare')):
                role, user, permission, restriction = [uuid.uuid4() for _ in range(4)]
                name = 'Expl-' + kind + '-' + user.hex[:6]
                table = 'Member' if field == 'Members' else 'Object'
                roles.append((role, user, permission, restriction, table))
                with conn.transaction():
                    conn.execute('INSERT INTO "PermissionPolicyRoleBase" ("ID","Name","Discriminator","IsAdministrative","CanEditModel","PermissionPolicy","IsAllowPermissionPriority") VALUES (%s,%s,%s,false,false,1,false)', (role, name, 'PermissionPolicyRole'))
                    conn.execute('INSERT INTO "PermissionPolicyUser" ("ID","UserName","StoredPassword","Discriminator","IsActive","ChangePasswordOnFirstLogon","AccessFailedCount") SELECT %s,%s,"StoredPassword","Discriminator",true,false,0 FROM "PermissionPolicyUser" WHERE "UserName"=%s', (user, name, 'User'))
                    conn.execute('INSERT INTO "PermissionPolicyRolePermissionPolicyUser" ("RolesID","UsersID") VALUES (%s,%s)', (role, user))
                    conn.execute('INSERT INTO "PermissionPolicyTypePermissionObject" ("ID","RoleID","TargetTypeFullName") VALUES (%s,%s,%s)', (permission, role, target))
                    conn.execute(f'INSERT INTO "PermissionPolicy{table}PermissionsObject" ("ID","TypePermissionObjectID","ReadState","{field}") VALUES (%s,%s,0,%s)', (restriction, permission, value))
                    conn.execute('INSERT INTO "PermissionPolicyUserLoginInfo" ("ID","UserForeignKey","LoginProviderName","ProviderUserKey") VALUES (%s,%s,%s,%s)', (uuid.uuid4(), user, 'Password', str(user)))
                login(name)
                body = call(name, path, expected=403)
                refused = json.dumps(body, ensure_ascii=False)
                assert list(body) == ['Erori'] and len(body['Erori']) == 1, body
                assert REFUZ in refused and expected_type in refused, (name, refused)
                assert all(urma not in refused for urma in ('Iesiri', 'SoldInainte', 'Valoare"', lots[0], lots[1])), (name, refused)
                print('PASS SC-CIT-99', kind, '403', REFUZ, 'pe tranzacție vizibilă, fără nicio valoare derivată', flush=True)

            call('Admin', f'/api/bcs/{bcs}/storneaza', 'POST', {'Data': f'{year}-01-20'})
            storno = str(conn.execute('SELECT "ID" FROM "Tranzactie" WHERE "DocumentId"=%s AND "Fel"=2',
                (uuid.UUID(bcs),)).fetchone()[0])
            e = call('Admin', f'/api/proiectii/explicatii/{storno}')
            origin, = e['Origini']
            assert e['Fel'] == 'Storno' and origin['PurtatorId'] == operare and len(origin['Linii']) == 2, e
            assert conn.execute('SELECT count(*) FROM "Tranzactie" WHERE "ID"=%s AND "Explicatie" IS NULL AND "ExplicatieDinId" IS NULL',
                (uuid.UUID(storno),)).fetchone()[0] == 1
            print('PASS SC-CIT-99 storno: fără explicație proprie, ușa întoarce explicația originalului', flush=True)
        finally:
            with conn.transaction():
                for role, user, permission, restriction, table in reversed(roles):
                    conn.execute(f'DELETE FROM "PermissionPolicy{table}PermissionsObject" WHERE "ID"=%s', (restriction,))
                    conn.execute('DELETE FROM "PermissionPolicyTypePermissionObject" WHERE "ID"=%s', (permission,))
                    conn.execute('DELETE FROM "PermissionPolicyRolePermissionPolicyUser" WHERE "RolesID"=%s', (role,))
                    conn.execute('DELETE FROM "PermissionPolicyUserLoginInfo" WHERE "UserForeignKey"=%s', (user,))
                    conn.execute('DELETE FROM "PermissionPolicyUser" WHERE "ID"=%s', (user,))
                    conn.execute('DELETE FROM "PermissionPolicyRoleBase" WHERE "ID"=%s', (role,))


if __name__ == '__main__':
    main()
