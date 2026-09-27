"""SC-CIT-25: rapoarte HTTP cu permisiuni pe postări, inclusiv după închidere.

Necesită psycopg și hostul privat pornit pe baza izolată CodexBCS.
Datele și rolurile temporare sunt curățate în finally; auditul HTTP rămâne.
"""
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
    parser.add_argument('--host', default='http://127.0.0.1:5089')
    parser.add_argument('--partide', action='store_true', help='Include SC-CIT-52/54 pe două partide ale aceleiași note.')
    parser.add_argument('--semnal-browser', help='Fișier creat după verificarea UI; așteptare maximă 5 minute.')
    args = parser.parse_args()
    tokens, roles, periods = {}, [], []
    document = None
    suppliers = []
    operated = closed = False
    year = 2000
    dsn = 'host=localhost port=5444 dbname=Atlas.Conta.ModelCheck.Privat.CodexBCS user=postgres password=postgres'

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
        assert conn.execute('SELECT current_database()').fetchone()[0] == 'Atlas.Conta.ModelCheck.Privat.CodexBCS'
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
            location = lookup('UnitateInterna', 'Cod', 'SEDIU')
            tip = lookup('TipMaterial', 'Cod', 'TRZ')
            debit = lookup('Cont', 'Simbol', '628')
            credit1, credit2 = lookup('Cont', 'Simbol', '401'), lookup('Cont', 'Simbol', '462')
            if args.partide:
                credit2 = credit1
                for index in range(2):
                    supplier = call('Admin', '/api/odata/Partener', 'POST',
                        {'Cod': 'CODEX-CIT54-' + uuid.uuid4().hex[:8], 'Denumire': f'Proba partide {index + 1}'})['ID']
                    suppliers.append(supplier)
            result = call('Admin', '/api/ntc', 'POST', {'Data': f'{year}-01-15',
                'PredatorId': location, 'PrimitorId': location, 'Linii': [
                    {'TipMaterialId': tip, 'ContDebitId': debit, 'ContCreditId': credit1, 'Valoare': 100, 'RepartitorCreditId': suppliers[0] if suppliers else None},
                    {'TipMaterialId': tip, 'ContDebitId': debit, 'ContCreditId': credit2, 'Valoare': 25, 'RepartitorCreditId': suppliers[1] if suppliers else None}]})
            document = result['Id']
            call('Admin', f'/api/ntc/{document}/opereaza', 'POST', {})
            operated = True
            users = [('Admin', 125), ('Cititor', 125), ('User', 0)]
            for kind in ('Object', 'Member'):
                role, user, permission, restriction = [uuid.uuid4() for _ in range(4)]
                name = 'CodexCIT-' + kind + '-' + user.hex[:8]
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

            def verify(after_close):
                period = f'dataStart={year}-01-01&dataEnd={year}-01-31'
                for name, expected in users:
                    journal = call(name, '/api/proiectii/registru-jurnal?' + period)['data']
                    balance = call(name, '/api/proiectii/balanta?' + period)['data']
                    assert sum(r['Debit'] for r in journal) == expected, (name, journal)
                    assert sum(r['RulajDebit'] for r in balance) == expected, (name, balance)
                    if args.partide:
                        rows = call(name, f'/api/proiectii/partide?laData={year}-01-31')['data']
                        assert sum(r['Rest'] for r in rows) == expected, (name, rows)
                        assert all(r['DocumentId'] == document
                            and r['Sens'] == 'Datorie' and r['DocumentTip'] == 'NTC' for r in rows), (name, rows)
                        if expected == 125:
                            assert len(rows) == 2 and len({r['UnitateId'] for r in rows}) == 2
                            assert {r['ContrapartidaId'] for r in rows} == set(suppliers)
                        if name != 'User':
                            panel = call(name, f'/api/imperecheri/{document}/stingeri')
                            assert panel['Ramas'] == expected and panel['Total'] == expected, (name, panel)
                        print('PASS SC-CIT-52/54', name.split('-')[0:2], 'închis' if after_close else 'deschis', expected, flush=True)
                    if name == 'User':
                        assert not journal and not balance
                        continue
                    ledger = call(name, '/api/proiectii/fisa-cont?contId=' + debit + '&' + period)['data']
                    assert ledger and ledger[-1]['SoldCurent'] == expected, (name, ledger)
                    if expected == 125:
                        assert len(journal) == 4 and len({(r['Id'], r['Spatiu']) for r in journal}) == 4
                        assert all((r['ContrapartidaId'] == credit1 if args.partide else r['ContrapartidaId'] is None)
                            and r['ContrapartidaSimbol'] == ('401' if args.partide else '401, 462') for r in ledger)
                    if after_close:
                        feb = call(name, f'/api/proiectii/balanta?dataStart={year}-02-01&dataEnd={year}-02-28')['data']
                        assert sum(r['InitialDebit'] for r in feb) == expected, (name, feb)
                    print('PASS SC-CIT-25', name.split('-')[0:2], 'închis' if after_close else 'deschis', expected)

            verify(False)
            if args.semnal_browser:
                print(f'BROWSER READY /partide?laData={year}-01-31', flush=True)
                deadline = time.monotonic() + 300
                while not Path(args.semnal_browser).exists() and time.monotonic() < deadline:
                    time.sleep(1)
                assert Path(args.semnal_browser).exists(), 'Verificarea UI nu a confirmat în 5 minute.'
            findings = call('Admin', f'/api/perioade/{year}/1/verificare')
            call('Admin', f'/api/perioade/{year}/1/inchide', 'POST', {'Acceptate': [f['Cheie'] for f in findings]})
            closed = True
            assert conn.execute('SELECT count(*) FROM "SolduriPerioadaContabil" WHERE "An"=%s AND "Luna"=1', (year,)).fetchone()[0] > 0
            verify(True)
            call('Admin', '/api/perioade/reconstruieste', 'POST', {})
            verify(True)
        finally:
            errors = []
            def cleanup(action):
                try:
                    action()
                except Exception as error:
                    errors.append(str(error))
            if closed:
                cleanup(lambda: call('Admin', f'/api/perioade/{year}/1/redeschide', 'POST', {'Motiv': 'Curățarea SC-CIT-25'}))
            if operated:
                cleanup(lambda: call('Admin', f'/api/ntc/{document}/anuleaza', 'POST', {}))
            if document:
                cleanup(lambda: call('Admin', f'/api/ntc/{document}', 'DELETE'))
            for supplier in reversed(suppliers):
                cleanup(lambda supplier=supplier: call('Admin', f'/api/odata/Partener/{supplier}', 'DELETE'))
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
