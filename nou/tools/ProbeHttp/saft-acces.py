"""SC-SAFT-13/22/36: D406 L pe cub pe host privat izolat; accesul complet, plățile și linia operată."""
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
    parser.add_argument('--host', default='http://127.0.0.1:5089')
    parser.add_argument('--baza', required=True)
    args = parser.parse_args()
    tokens, roles, periods, documents, pairings = {}, [], [], [], []
    partner = None
    year = 2022
    marker = 'SAFT-S2-' + uuid.uuid4().hex[:6]
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

    saft = f'/api/proiectii/saft?an={year}&luna=1'
    xml = f'/api/proiectii/saft/xml?an={year}&luna=1'

    with psycopg.connect(dsn, autocommit=True) as conn:
        assert conn.execute('SELECT current_database()').fetchone()[0] == args.baza
        assert conn.execute('SELECT count(*) FROM "PerioadeFiscale" WHERE "An"=%s', (year,)).fetchone()[0] == 0
        for name in ('Admin', 'Cititor', 'User'):
            login(name)
        society = conn.execute('SELECT "ID","CodFiscal" FROM "Societati"').fetchone()
        conn.execute('UPDATE "Societati" SET "CodFiscal"=%s WHERE "ID"=%s', ('RO12345674', society[0]))
        try:
            for month in (1, 2):
                pid = uuid.uuid4(); periods.append(pid)
                conn.execute('INSERT INTO "PerioadeFiscale" ("ID","An","Luna","Inchisa") VALUES (%s,%s,%s,false)', (pid, year, month))
            partner = call('Admin', '/api/odata/Partener', 'POST', {
                'Cod': marker, 'Denumire': 'Furnizor SAF-T S2', 'Tara': 'RO', 'CodFiscal': 'RO12345674',
                'InregistratTva': True}, 201)['ID']
            stock, tip, tva = lookup('Gestiune', 'Cod', 'MAG1'), lookup('TipMaterial', 'Cod', '628'), lookup('TipTva', 'Cod', 'N21')
            fct_body = {'Numar': marker + '-F', 'Data': f'{year}-01-10', 'PredatorId': partner, 'PrimitorId': stock,
                'Linii': [{'TipMaterialId': tip, 'TipTvaId': tva, 'Cantitate': 1, 'PretUnitar': 100}]}
            fct = call('Admin', '/api/fct', 'POST', fct_body, 201)['Id']
            documents.append(('fct', fct))
            call('Admin', f'/api/fct/{fct}/opereaza', 'POST', {})
            casa, trz = lookup('ContPropriu', 'Cod', 'CASA'), lookup('TipMaterial', 'Cod', 'TRZ')
            plt = call('Admin', '/api/plt', 'POST', {'Data': f'{year}-01-12', 'PredatorId': casa, 'PrimitorId': partner,
                'TipInstrument': 'DispozitieCasa', 'Linii': [{'TipMaterialId': trz, 'Valoare': 70}]}, 201)['Id']
            documents.append(('plt', plt))
            call('Admin', f'/api/plt/{plt}/opereaza', 'POST', {})
            pairings.append(call('Admin', '/api/imperecheri', 'POST',
                {'DocumentStingatorId': plt, 'DocumentId': fct, 'Suma': 50, 'Data': f'{year}-01-20'}, 201))

            for kind, target, field, value, member in (
                    ('Rand', POSTARE, 'Criteria', '[Valoare] > 100', None),
                    ('Membru', POSTARE, 'Members', 'Valoare', 'Valoare'),
                    ('Metadate', PARTENER, 'Criteria', f"StartsWith([Cod], '{marker}')", None)):
                role, user, permission, restriction = [uuid.uuid4() for _ in range(4)]
                name = 'SaftS2-' + kind + '-' + user.hex[:6]
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

                for path in (saft, xml):
                    refused = json.dumps(call(name, path, expected=403), ensure_ascii=False)
                    expected_type = 'Partener' if target == PARTENER else 'Postare' + ('.' + member if member else '')
                    assert 'SAFT_ACCES_INCOMPLET' in refused and expected_type in refused, (name, path, refused)
                    assert '121' not in refused and '100.00' not in refused, (name, path, refused)
                print('PASS SC-SAFT-36', kind, '403 SAFT_ACCES_INCOMPLET pe sumar și fișier, fără sume', flush=True)

            for path in (saft, xml):
                refused = json.dumps(call('User', path, expected=403), ensure_ascii=False)
                assert 'SAFT_ACCES_INCOMPLET' in refused and 'citi' in refused, refused
            print('PASS SC-SAFT-36 User fără drept pe Postare: 403 pe ambele uși', flush=True)

            before = {}
            for name in ('Admin', 'Cititor'):
                summary = call(name, saft)
                assert summary['Refuzuri'] == [] and summary['Plati'] == 1 and summary['FacturiPrimite'] == 1, (name, summary)
                content = call(name, xml)
                root = ET.fromstring(content)
                ns = {'s': root.tag.split('}')[0][1:]}
                lines = root.findall('.//s:Payments/s:Payment/s:PaymentLine', ns)
                amounts = sorted(float(l.find('s:PaymentLineAmount/s:Amount', ns).text) for l in lines)
                refs = sorted((l.findtext('s:SourceDocumentID', None, ns) or '-') for l in lines)
                assert amounts == [20.0, 50.0] and refs == ['-', marker + '-F'], (name, amounts, refs)
                taxes = root.findall('.//s:GeneralLedgerEntries//s:TaxInformation/s:TaxAmount/s:Amount', ns)
                assert sum(float(t.text) for t in taxes) == 21, (name, [t.text for t in taxes])
                before[name] = content
                print('PASS SC-SAFT-13', name, 'acces complet: 200, plata 50 F + 20 rest, TVA 21 în GL', flush=True)

            read = call('Admin', f'/api/fct/{fct}')
            edited = dict(fct_body)
            edited['Linii'] = [{**fct_body['Linii'][0], 'Id': read['Linii'][0]['Id'], 'Cantitate': 5}]
            refused = json.dumps(call('Admin', f'/api/fct/{fct}', 'PUT', edited, expected=422), ensure_ascii=False)
            assert call('Admin', xml) == before['Admin'], 'fișierul s-a schimbat după refuzul editării'
            print('PASS SC-SAFT-22 linia operată refuzată pe ușa publică (422), fișierul L neschimbat:', refused[:160], flush=True)
        finally:
            for role, user, permission, restriction, table in roles:
                with conn.transaction():
                    conn.execute('DELETE FROM "PermissionPolicyUserLoginInfo" WHERE "UserForeignKey"=%s', (user,))
                    conn.execute(f'DELETE FROM "PermissionPolicy{table}PermissionsObject" WHERE "ID"=%s', (restriction,))
                    conn.execute('DELETE FROM "PermissionPolicyTypePermissionObject" WHERE "ID"=%s', (permission,))
                    conn.execute('DELETE FROM "PermissionPolicyRolePermissionPolicyUser" WHERE "UsersID"=%s', (user,))
                    conn.execute('DELETE FROM "PermissionPolicyUser" WHERE "ID"=%s', (user,))
                    conn.execute('DELETE FROM "PermissionPolicyRoleBase" WHERE "ID"=%s', (role,))
            for pairing in pairings:
                pid = pairing.get('Id') if isinstance(pairing, dict) else None
                if pid:
                    call('Admin', f'/api/imperecheri/{pid}', 'DELETE', expected=204)
            for kind, doc in reversed(documents):
                call('Admin', f'/api/{kind}/{doc}/anuleaza', 'POST', {})
                call('Admin', f'/api/{kind}/{doc}', 'DELETE', expected=204)
            if partner:
                call('Admin', f'/api/odata/Partener({partner})', 'DELETE')
            for pid in periods:
                conn.execute('DELETE FROM "PerioadeFiscale" WHERE "ID"=%s', (pid,))
            conn.execute('UPDATE "Societati" SET "CodFiscal"=%s WHERE "ID"=%s', (society[1], society[0]))


if __name__ == '__main__':
    main()
