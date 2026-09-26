"""SC-CIT-87: citiri fiscale securizate, depunere și concurență pe host privat izolat."""
import argparse
import concurrent.futures
import json
import time
import urllib.error
import urllib.parse
import urllib.request
import uuid
import xml.etree.ElementTree as ET
import psycopg


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--host', default='http://127.0.0.1:5089')
    parser.add_argument('--baza', required=True)
    args = parser.parse_args()
    tokens, roles, periods, documents = {}, [], [], []
    supplier = None
    itv = None
    closed = False
    versions = {}
    year = 2021
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
        assert status == expected, (method, path, status, body[:1800])
        return (json.loads(body) if body[:1] in ('{', '[', '"') else body) if body else None

    def login(name):
        tokens[name] = call(None, '/api/Authentication/Authenticate', 'POST', {'userName': name, 'password': ''})

    def lookup(entity, field, value):
        q = urllib.parse.urlencode({'$filter': f"{field} eq '{value}'"})
        return call('Admin', '/api/odata/' + entity + '?' + q)['value'][0]['ID']

    with psycopg.connect(dsn, autocommit=True) as conn:
        assert conn.execute('SELECT current_database()').fetchone()[0] == args.baza
        assert conn.execute('SELECT count(*) FROM "PerioadeFiscale" WHERE "An"=%s OR "Inchisa"', (year,)).fetchone()[0] == 0
        for name in ('Admin', 'Cititor', 'User'):
            login(name)
        marker = lookup('Cont', 'Simbol', '628')
        assert conn.execute('SELECT count(*) FROM "Conturi" WHERE "ID"=%s', (uuid.UUID(marker),)).fetchone()[0] == 1
        society = conn.execute('SELECT "ID","CodFiscal" FROM "Societati" WHERE "GCRecord"=0').fetchone()
        assert society is not None
        conn.execute('UPDATE "Societati" SET "CodFiscal"=%s WHERE "ID"=%s', ('RO12345674', society[0]))
        try:
            for month in (1, 2):
                pid = uuid.uuid4(); periods.append(pid)
                conn.execute('INSERT INTO "PerioadeFiscale" ("ID","An","Luna","Inchisa") VALUES (%s,%s,%s,false)', (pid, year, month))
            supplier = call('Admin', '/api/odata/Partener', 'POST', {
                'Cod': 'SC-CIT87-' + uuid.uuid4().hex[:8], 'Denumire': 'Furnizor fiscal HTTP',
                'Tara': 'RO', 'CodFiscal': 'RO12345674', 'InregistratTva': True}, 201)['ID']
            stock, tip, tva = lookup('Gestiune', 'Cod', 'MAG1'), lookup('TipMaterial', 'Cod', '628'), lookup('TipTva', 'Cod', 'N21')
            interval = f'dataStart={year}-01-01&dataEnd={year}-01-31'
            for price in (100, 25):
                d = call('Admin', '/api/fct', 'POST', {'Numar': 'SC-CIT87-' + str(price), 'Data': f'{year}-01-15',
                    'PredatorId': supplier, 'PrimitorId': stock,
                    'Linii': [{'TipMaterialId': tip, 'TipTvaId': tva, 'Cantitate': 1, 'PretUnitar': price}]}, 201)['Id']
                documents.append(d)
                call('Admin', f'/api/fct/{d}/opereaza', 'POST', {})
                for form in ('D300', 'D394'):
                    export = call('Admin', '/api/proiectii/' + form.lower() + '?' + interval)
                    version = export['VersiuneExportata']
                    assert len(version) == 64
                    if price == 100:
                        versions[form] = version
                    else:
                        path = f'/api/depuneri-declaratii/{form}/{year}/1'
                        refused = call('Admin', path, 'POST', {'VersiuneExportata': versions[form]}, 422)
                        assert 'DEPUNERE_VERSIUNE_DEPASITA' in json.dumps(refused)
                        assert call('Admin', path)['ConfirmataLa'] is None
                        assert version != versions[form]
                        versions[form] = version
                        print('PASS SC-CIT-88', form, 'export 100/21 depășit; export nou 125/26,25', flush=True)
            users = [('Admin', 125, 26.25), ('Cititor', 125, 26.25), ('User', 0, 0)]
            for kind, field, value, base, tax in (
                ('Object', 'Criteria', 'DocumentId = {' + documents[1] + '}', 100, 21),
                ('Object', 'Criteria', 'RolTva = 2', 125, 0),
                ('Member', 'Members', 'Valoare', 0, 0)):
                role, user, permission, restriction = [uuid.uuid4() for _ in range(4)]
                name = 'CodexCIT87-' + kind + '-' + user.hex[:8]
                roles.append((role, user, permission, restriction, kind))
                with conn.transaction():
                    conn.execute('INSERT INTO "PermissionPolicyRoleBase" ("ID","Name","Discriminator","IsAdministrative","CanEditModel","PermissionPolicy","IsAllowPermissionPriority") VALUES (%s,%s,%s,false,false,1,false)', (role, name, 'PermissionPolicyRole'))
                    conn.execute('INSERT INTO "PermissionPolicyUser" ("ID","UserName","StoredPassword","Discriminator","IsActive","ChangePasswordOnFirstLogon","AccessFailedCount") SELECT %s,%s,"StoredPassword","Discriminator",true,false,0 FROM "PermissionPolicyUser" WHERE "UserName"=%s', (user, name, 'User'))
                    conn.execute('INSERT INTO "PermissionPolicyRolePermissionPolicyUser" ("RolesID","UsersID") VALUES (%s,%s)', (role, user))
                    conn.execute('INSERT INTO "PermissionPolicyTypePermissionObject" ("ID","RoleID","TargetTypeFullName") VALUES (%s,%s,%s)', (permission, role, 'Atlas.Conta.BackOffice.Module.Cub.Postare'))
                    conn.execute(f'INSERT INTO "PermissionPolicy{kind}PermissionsObject" ("ID","TypePermissionObjectID","ReadState","{field}") VALUES (%s,%s,0,%s)', (restriction, permission, value))
                    conn.execute('INSERT INTO "PermissionPolicyUserLoginInfo" ("ID","UserForeignKey","LoginProviderName","ProviderUserKey") VALUES (%s,%s,%s,%s)', (uuid.uuid4(), user, 'Password', str(user)))
                login(name); users.append((name, base, tax))

            interval = f'dataStart={year}-01-01&dataEnd={year}-01-31'
            def verify(stage):
                for name, base, tax in users:
                    if name == 'User':
                        for path in ('/api/proiectii/jurnal-tva?sens=Achizitie&' + interval,
                                     '/api/proiectii/decont-tva?' + interval,
                                     '/api/proiectii/d300?' + interval,
                                     '/api/proiectii/d394?' + interval,
                                     f'/api/proiectii/saft/xml?an={year}&luna=1'):
                            call(name, path, expected=403)
                        print('PASS SC-CIT-87', stage, 'User refuz 403', flush=True)
                        continue
                    journal = call(name, '/api/proiectii/jurnal-tva?sens=Achizitie&' + interval)['data']
                    assert sum(r['Baza'] for r in journal) == base and sum(r['Tva'] for r in journal) == tax, (stage, name, journal)
                    totals = call(name, '/api/proiectii/decont-tva?' + interval)['data']
                    assert sum(r['Baza'] for r in totals) == base and sum(r['Tva'] for r in totals) == tax, (stage, name, totals)
                    d300 = call(name, '/api/proiectii/d300?' + interval)
                    assert next((r['Tva'] for r in d300['Randuri'] if r['Cod'] == '30'), 0) == tax, (stage, name, d300)
                    d394 = call(name, '/api/proiectii/d394?' + interval)
                    assert sum(r['Baza'] for r in d394['Operatiuni']) == base and sum(r['Tva'] or 0 for r in d394['Operatiuni']) == tax, (stage, name, d394)
                    if name != 'User':
                        xml = call(name, f'/api/proiectii/saft/xml?an={year}&luna=1')
                        root = ET.fromstring(xml)
                        ns = {'s': root.tag.split('}')[0][1:]}
                        taxes = root.findall('.//s:GeneralLedgerEntries//s:TaxInformation/s:TaxAmount/s:Amount', ns)
                        assert sum(float(t.text) for t in taxes) == tax, (stage, name, [t.text for t in taxes])
                    print('PASS SC-CIT-87', stage, name.split('-')[:2], base, tax, flush=True)

            verify('deschis')
            itv = call('Admin', '/api/itv/genereaza', 'POST',
                {'An': year, 'Luna': 1, 'UnitateId': lookup('UnitateInterna', 'Cod', 'SEDIU')})['DocumentId']
            assert itv
            call('Admin', f'/api/itv/{itv}/opereaza', 'POST', {})
            findings = call('Admin', f'/api/perioade/{year}/1/verificare')
            call('Admin', f'/api/perioade/{year}/1/inchide', 'POST', {'Acceptate': [f['Cheie'] for f in findings]})
            closed = True
            verify('închis')
            call('Admin', '/api/perioade/reconstruieste', 'POST', {})
            verify('reconstruit')
            path = f'/api/depuneri-declaratii/D394/{year}/1'
            for user, expected in ((None, 401), ('Cititor', 403), ('User', 404)):
                call(user, path, 'POST', {'VersiuneExportata': versions['D394']}, expected)
            call('Admin', path, 'POST', {'VersiuneExportata': ''}, 400)
            with concurrent.futures.ThreadPoolExecutor() as pool:
                with conn.transaction():
                    conn.execute('SELECT pg_advisory_xact_lock_shared(103300394)')
                    pending = pool.submit(call, 'Admin', path, 'POST', {'VersiuneExportata': versions['D394']})
                    time.sleep(.3)
                    assert not pending.done(), 'Depunerea nu așteaptă tranzacția fiscală.'
                first = pending.result(timeout=30)
            assert call('Admin', path, 'POST', {'VersiuneExportata': versions['D394']}) == first
            assert call('Admin', path)['VersiuneExportata'] == versions['D394']
            d300path = f'/api/depuneri-declaratii/D300/{year}/1'
            call('Admin', d300path, 'POST', {'VersiuneExportata': versions['D300']})
            for name, base, tax in users[3:]:
                partial = call(name, '/api/proiectii/d394?' + interval)['VersiuneExportata']
                refused = call('Admin', path, 'POST', {'VersiuneExportata': partial}, 422)
                assert 'DEPUNERE_VERSIUNE_DEPASITA' in json.dumps(refused)
            print('PASS SC-CIT-88 versiunile mascate nu pot fi confirmate', flush=True)
            call('Admin', f'/api/perioade/{year}/1/redeschide', 'POST', {'Motiv': 'SC-CIT87 refuz după depunere'})
            closed = False
            refused = call('Admin', f'/api/fct/{documents[0]}/anuleaza', 'POST', {}, 422)
            assert 'TVA_DEJA_DECLARATA' in json.dumps(refused)
            print('PASS SC-CIT-87 depunere: 401/400/404/403, idempotentă, așteaptă scrierea; anulare refuzată', flush=True)
        finally:
            conn.execute('UPDATE "Societati" SET "CodFiscal"=%s WHERE "ID"=%s', (society[1], society[0]))
            if closed:
                call('Admin', f'/api/perioade/{year}/1/redeschide', 'POST', {'Motiv': 'Curățarea SC-CIT87'})
            conn.execute('DELETE FROM "DepuneriDeclaratii" WHERE "Perioada"=%s AND "VersiuneExportata" = ANY(%s)', (year*100+1, list(versions.values())))
            if itv:
                if call('Admin', f'/api/itv/{itv}')['Stare'] == 'Operat':
                    call('Admin', f'/api/itv/{itv}/anuleaza', 'POST', {})
                call('Admin', f'/api/itv/{itv}', 'DELETE', expected=204)
            for d in reversed(documents):
                state = call('Admin', f'/api/fct/{d}')
                if state['Stare'] == 'Operat':
                    call('Admin', f'/api/fct/{d}/anuleaza', 'POST', {})
                call('Admin', f'/api/fct/{d}', 'DELETE', expected=204)
            if supplier:
                call('Admin', f'/api/odata/Partener/{supplier}', 'DELETE')
            with conn.transaction():
                for role, user, permission, restriction, kind in reversed(roles):
                    conn.execute(f'DELETE FROM "PermissionPolicy{kind}PermissionsObject" WHERE "ID"=%s', (restriction,))
                    conn.execute('DELETE FROM "PermissionPolicyTypePermissionObject" WHERE "ID"=%s', (permission,))
                    conn.execute('DELETE FROM "PermissionPolicyRolePermissionPolicyUser" WHERE "RolesID"=%s', (role,))
                    conn.execute('DELETE FROM "PermissionPolicyUserLoginInfo" WHERE "UserForeignKey"=%s', (user,))
                    conn.execute('DELETE FROM "PermissionPolicyUser" WHERE "ID"=%s', (user,))
                    conn.execute('DELETE FROM "PermissionPolicyRoleBase" WHERE "ID"=%s', (role,))
                for pid in reversed(periods):
                    conn.execute('DELETE FROM "InchideriPerioade" WHERE "PerioadaId"=%s', (pid,))
                    conn.execute('DELETE FROM "PerioadeFiscale" WHERE "ID"=%s AND NOT "Inchisa"', (pid,))


if __name__ == '__main__':
    main()
