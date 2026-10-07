"""D9-D9: utilizatorii temporari ai matricei de acces pe `Postare`, pentru `refuzuri.ps1 -Baza <bază>`.

`creeaza` scrie trei roluri `ReadOnlyAllByDefault` cu câte un utilizator (parola goală a lui `User`) și
tipărește numele lor ca JSON; `sterge` le șterge după marcaj. Necesită psycopg.
  FaraPostare   — citirea refuzată pe tipul `Postare`;
  RandPostare   — criteriu de rând pe `Postare`;
  MembruPostare — restricție de membru pe `Postare`.
"""
import argparse
import json
import uuid

import psycopg

POSTARE = 'Atlas.Conta.BackOffice.Module.Cub.Postare'
# (tipul, tabela restricției sau None pentru tip, câmpul, valoarea)
ROLURI = {
    'FaraPostare': [(POSTARE, None, None, None)],
    'RandPostare': [(POSTARE, 'Object', 'Criteria', '[Valoare] > 100')],
    'MembruPostare': [(POSTARE, 'Member', 'Members', 'Valoare')],
}


def sterge(conn, marcaj):
    tipar = marcaj + '-%'
    roluri = '(SELECT "ID" FROM "PermissionPolicyRoleBase" WHERE "Name" LIKE %s)'
    permisiuni = f'(SELECT "ID" FROM "PermissionPolicyTypePermissionObject" WHERE "RoleID" IN {roluri})'
    utilizatori = '(SELECT "ID" FROM "PermissionPolicyUser" WHERE "UserName" LIKE %s)'
    with conn.transaction():
        for tabela in ('Object', 'Member'):
            conn.execute(f'DELETE FROM "PermissionPolicy{tabela}PermissionsObject" WHERE "TypePermissionObjectID" IN {permisiuni}', (tipar,))
        conn.execute(f'DELETE FROM "PermissionPolicyTypePermissionObject" WHERE "RoleID" IN {roluri}', (tipar,))
        conn.execute(f'DELETE FROM "PermissionPolicyRolePermissionPolicyUser" WHERE "RolesID" IN {roluri}', (tipar,))
        conn.execute(f'DELETE FROM "PermissionPolicyUserLoginInfo" WHERE "UserForeignKey" IN {utilizatori}', (tipar,))
        conn.execute('DELETE FROM "PermissionPolicyUser" WHERE "UserName" LIKE %s', (tipar,))
        conn.execute('DELETE FROM "PermissionPolicyRoleBase" WHERE "Name" LIKE %s', (tipar,))


def creeaza(conn, marcaj):
    nume = {}
    with conn.transaction():
        for fel, restrictii in ROLURI.items():
            rol, utilizator = uuid.uuid4(), uuid.uuid4()
            nume[fel] = f'{marcaj}-{fel}'
            conn.execute('INSERT INTO "PermissionPolicyRoleBase" ("ID","Name","Discriminator","IsAdministrative","CanEditModel","PermissionPolicy","IsAllowPermissionPriority") VALUES (%s,%s,%s,false,false,1,false)', (rol, nume[fel], 'PermissionPolicyRole'))
            conn.execute('INSERT INTO "PermissionPolicyUser" ("ID","UserName","StoredPassword","Discriminator","IsActive","ChangePasswordOnFirstLogon","AccessFailedCount") SELECT %s,%s,"StoredPassword","Discriminator",true,false,0 FROM "PermissionPolicyUser" WHERE "UserName"=%s', (utilizator, nume[fel], 'User'))
            conn.execute('INSERT INTO "PermissionPolicyRolePermissionPolicyUser" ("RolesID","UsersID") VALUES (%s,%s)', (rol, utilizator))
            conn.execute('INSERT INTO "PermissionPolicyUserLoginInfo" ("ID","UserForeignKey","LoginProviderName","ProviderUserKey") VALUES (%s,%s,%s,%s)', (uuid.uuid4(), utilizator, 'Password', str(utilizator)))
            for tip, tabela, camp, valoare in restrictii:
                permisiune = uuid.uuid4()
                if tabela is None:
                    conn.execute('INSERT INTO "PermissionPolicyTypePermissionObject" ("ID","RoleID","TargetTypeFullName","ReadState") VALUES (%s,%s,%s,0)', (permisiune, rol, tip))
                else:
                    conn.execute('INSERT INTO "PermissionPolicyTypePermissionObject" ("ID","RoleID","TargetTypeFullName") VALUES (%s,%s,%s)', (permisiune, rol, tip))
                    conn.execute(f'INSERT INTO "PermissionPolicy{tabela}PermissionsObject" ("ID","TypePermissionObjectID","ReadState","{camp}") VALUES (%s,%s,0,%s)', (uuid.uuid4(), permisiune, valoare))
    return nume


def main():
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument('actiune', choices=('creeaza', 'sterge'))
    parser.add_argument('--baza', required=True, help='Baza hostului pe care rulează matricea.')
    parser.add_argument('--marcaj', required=True, help='Prefixul numelor de rol și de utilizator.')
    args = parser.parse_args()
    dsn = f'host=localhost port=5444 dbname={args.baza} user=postgres password=postgres'
    with psycopg.connect(dsn, autocommit=True) as conn:
        assert conn.execute('SELECT current_database()').fetchone()[0] == args.baza
        sterge(conn, args.marcaj)
        if args.actiune == 'creeaza':
            assert conn.execute('SELECT count(*) FROM "PermissionPolicyUser" WHERE "UserName"=%s', ('User',)).fetchone()[0] == 1, \
                'Baza nu are utilizatorul `User` (parola lui se copiază).'
            print(json.dumps(creeaza(conn, args.marcaj)))


if __name__ == '__main__':
    main()
