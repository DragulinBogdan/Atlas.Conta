"""104g/104i: ștergerea fizică pe ușa HTTP securizată.

(1) Admin șterge un nomenclator referit (TipMaterial 371, TipTva TI21) -> 422 de domeniu, rândul rămâne.
(2) Configurator șterge o mapare D394 de seed -> 204, rândul dispare fizic și rămâne un RefuzSeed
    (dreptul rolului pe RefuzSeed trece prin securitatea reală a SaveChanges).
Necesită hostul WebApi privat pe baza dată și psycopg. Maparea se restaurează în finally; se șterge
doar refuzul creat de ștergere, identificat după ID, iar refuzurile anterioare (inclusiv unul străin
pus de probă) rămân intacte.
"""
import argparse
import json
import ssl
import urllib.error
import urllib.parse
import urllib.request
import uuid

import psycopg


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('--baza', default='Atlas.Conta.BackOffice.Privat')
    parser.add_argument('--url', default='https://localhost:5001')
    options = parser.parse_args()
    base = options.url
    tokens = {}

    def call(user, path, method='GET', data=None):
        headers = {'Content-Type': 'application/json'}
        if user:
            headers['Authorization'] = 'Bearer ' + tokens[user]
        req = urllib.request.Request(base + path, headers=headers, method=method,
            data=json.dumps(data).encode() if data is not None else None)
        try:
            with urllib.request.urlopen(req, timeout=90, context=ssl._create_unverified_context()) as response:
                return response.status, response.read().decode()
        except urllib.error.HTTPError as error:
            return error.code, error.read().decode()

    def lookup(entity, field, value):
        status, body = call('Admin', '/api/odata/' + entity + '?' + urllib.parse.urlencode(
            {'$filter': f"{field} eq '{value}'"}))
        assert status == 200, (entity, status, body[:500])
        return json.loads(body)['value'][0]['ID']

    for name in ('Admin', 'Configurator'):
        status, body = call(None, '/api/Authentication/Authenticate', 'POST', {'userName': name, 'password': ''})
        assert status == 200, (name, status, body[:300])
        tokens[name] = body.strip('"')

    with psycopg.connect(f'host=localhost port=5444 dbname={options.baza} user=postgres password=postgres',
            autocommit=True) as conn:
        assert conn.execute('select current_database()').fetchone()[0] == options.baza

        for entity, table, field, value in (('TipMaterial', 'TipuriMaterial', 'Cod', '371'),
                                            ('TipTva', 'TipuriTva', 'Cod', 'TI21')):
            ident = lookup(entity, field, value)
            status, body = call('Admin', f'/api/odata/{entity}({ident})', 'DELETE')
            erori = json.loads(body).get('Erori', []) if body.startswith('{') else []
            ramas = conn.execute(f'SELECT count(*) FROM "{table}" WHERE "ID"=%s', (ident,)).fetchone()[0]
            print(f'{entity} {value} referit: DELETE -> {status} {erori}; rândul rămâne: {ramas == 1}')
            assert status == 422 and erori and ramas == 1, (entity, status, body[:800])

        ned = lookup('TipTva', 'Cod', 'NED21')
        cursor = conn.execute('SELECT * FROM "MapariD394" WHERE "TipTvaId"=%s AND "Sens"=1', (ned,))
        coloane = [c.name for c in cursor.description]
        rand = cursor.fetchone()
        assert rand is not None, 'Maparea de seed NED21/Achiziție lipsește.'
        mapare = dict(zip(coloane, rand))

        def refuzuri_d394():
            return dict(conn.execute('SELECT "ID", "Cheie" FROM "RefuzuriSeed" WHERE "Tip"=%s',
                                     ('MapareD394',)).fetchall())

        strain = uuid.uuid4()
        conn.execute('INSERT INTO "RefuzuriSeed" ("ID", "Tip", "Cheie", "La") VALUES (%s, %s, %s, now())',
                     (strain, 'MapareD394', json.dumps({'Proba': f'refuz-strain-{strain}'})))
        inainte = refuzuri_d394()
        noi = {}
        try:
            status, body = call('Configurator', f"/api/odata/MapareD394({mapare['ID']})", 'DELETE')
            fizic = conn.execute('SELECT count(*) FROM "MapariD394" WHERE "ID"=%s', (mapare['ID'],)).fetchone()[0]
            dupa = refuzuri_d394()
            noi = {i: c for i, c in dupa.items() if i not in inainte}
            print(f'Configurator șterge MapareD394 NED21/Achiziție: DELETE -> {status}; rând fizic: {fizic}; '
                  f'refuzuri noi: {list(noi.values())}; refuzurile anterioare intacte: {dupa.items() >= inainte.items()}')
            assert status in (200, 204), (status, body[:800])
            assert fizic == 0 and len(noi) == 1 and str(ned) in next(iter(noi.values()))
            assert dupa.items() >= inainte.items()
        finally:
            for ident in noi:
                conn.execute('DELETE FROM "RefuzuriSeed" WHERE "ID"=%s', (ident,))
            conn.execute('DELETE FROM "RefuzuriSeed" WHERE "ID"=%s', (strain,))
            if conn.execute('SELECT count(*) FROM "MapariD394" WHERE "ID"=%s', (mapare['ID'],)).fetchone()[0] == 0:
                conn.execute(f'INSERT INTO "MapariD394" ({", ".join(chr(34) + c + chr(34) for c in coloane)}) '
                             f'VALUES ({", ".join(["%s"] * len(coloane))})', rand)
        ramase = refuzuri_d394()
        del inainte[strain]
        print(f'Curățenia: refuzurile dinaintea probei rămân exact ({len(inainte)}): {ramase == inainte}')
        assert ramase == inainte
    print('OK: refuz FK 422 pe nomenclatoare referite; ștergere fizică + RefuzSeed pe politica de seed.')


if __name__ == '__main__':
    main()
