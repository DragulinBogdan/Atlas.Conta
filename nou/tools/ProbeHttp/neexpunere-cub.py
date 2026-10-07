"""D9-D9, D9-A12: `Postare`, `Tranzactie` și `PostareVizual` nu sunt expuse pe HTTP și nu intră în contractul clientului.

Cere OData și REST pentru cele trei tipuri (ca `Admin`, ca refuzul să nu fie de permisiune), caută `EntitySet`-ul lor în
`$metadata` și în artefactele generate ale clientului. Dovedește neexpunerea, nu gardul scrierii (acela e în
ModelCheck, `D9-P4-GARD-*`). Necesită hostul WebApi pornit.
"""
import argparse
import json
import re
import urllib.error
import urllib.request
from pathlib import Path

TIPURI = ('Postare', 'Tranzactie', 'PostareVizual')
RUTE = [f'/api/odata/{tip}{sufix}' for tip in TIPURI for sufix in ('', '?$top=1', '(00000000-0000-0000-0000-000000000000)')] \
    + ['/api/postare', '/api/postari', '/api/tranzactie', '/api/tranzactii', '/api/cub/postari', '/api/cub/tranzactii']


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--host', default='https://localhost:5001')
    parser.add_argument('--client', default=str(Path(__file__).resolve().parents[2] / 'Atlas.Conta.Client' / 'src' / 'generated'))
    args = parser.parse_args()
    import ssl
    context = ssl._create_unverified_context()
    esecuri = []

    def call(path, method='GET', token=None, data=None):
        headers = {'Content-Type': 'application/json'}
        if token:
            headers['Authorization'] = 'Bearer ' + token
        request = urllib.request.Request(args.host.rstrip('/') + path,
            data=json.dumps(data).encode() if data is not None else None, headers=headers, method=method)
        try:
            with urllib.request.urlopen(request, timeout=90, context=context) as response:
                return response.status, response.read().decode()
        except urllib.error.HTTPError as error:
            return error.code, error.read().decode()

    def proba(nume, ok, detaliu=''):
        print(('PASS ' if ok else 'FAIL ') + nume + (f' — {detaliu}' if detaliu and not ok else ''), flush=True)
        if not ok:
            esecuri.append(nume)

    status, token = call('/api/Authentication/Authenticate', 'POST', data={'userName': 'Admin', 'password': ''})
    assert status == 200, ('autentificare', status, token[:300])
    token = token.strip('"')

    martor, _ = call('/api/odata/Partener?$top=1', token=token)
    proba('martor: `GET /api/odata/Partener` răspunde 200 (ruta OData e vie)', martor == 200, str(martor))
    for ruta in RUTE:
        status, corp = call(ruta, token=token)
        proba(f'`GET {ruta}` nu găsește rută (404)', status == 404, f'{status} {corp[:200]}')
    for tip in TIPURI:
        status, corp = call(f'/api/odata/{tip}', 'POST', token=token, data={})
        proba(f'`POST /api/odata/{tip}` nu găsește rută (404 sau 405)', status in (404, 405), f'{status} {corp[:200]}')

    status, metadata = call('/api/odata/$metadata', token=token)
    proba('`GET /api/odata/$metadata` răspunde 200', status == 200, str(status))
    # XAF descrie în `$metadata` orice tip al contextului ca `EntityType`; expunerea e `EntitySet`-ul.
    seturi = re.findall(r'<EntitySet\b[^>]*\bEntityType="([^"]+)"', metadata)
    proba('martor: `$metadata` are `EntitySet` pentru `Partener` și numai `EntityType` pentru `Document`',
        any(s.endswith('.Partener') for s in seturi) and not any(s.endswith('.Document') for s in seturi)
        and '<EntityType Name="Document"' in metadata)
    referinte = {}
    for nume, corp in re.findall(r'<EntityType Name="([^"]+)"(.*?)</EntityType>', metadata, flags=re.S):
        for tinta in re.findall(r'<NavigationProperty\b[^>]*\bType="[^"]*\bCub\.(\w+)\)?"', corp):
            referinte.setdefault(tinta, set()).add(nume)
    for tip in TIPURI:
        proba(f'`$metadata` nu are `EntitySet` pentru `{tip}`', status == 200 and not any(s.endswith('.' + tip) for s in seturi))
        straini = sorted(referinte.get(tip, set()) - set(TIPURI))
        proba(f'niciun tip din afara cubului nu navighează spre `{tip}`', status == 200 and not straini, str(straini[:3]))

    generat = Path(args.client)
    openapi = json.loads((generat / 'openapi.json').read_text(encoding='utf-8'))
    tipuri = json.loads((generat / 'metadata.json').read_text(encoding='utf-8'))['Tipuri']
    ts = (generat / 'api-types.ts').read_text(encoding='utf-8')
    for tip in TIPURI:
        proba(f'`openapi.json` nu are schema `{tip}`', tip not in openapi['components']['schemas'])
        proba(f'`metadata.json` nu are tipul `{tip}`', tip not in tipuri)
        proba(f'`api-types.ts` nu are schema `{tip}`', not re.search(rf'(?m)^\s+{tip}\??:\s', ts))
    rute = [r for r in openapi['paths'] if re.search(r'(?i)/(postare|postari|tranzactie|tranzactii)\b', r)]
    proba('`openapi.json` nu are nicio rută a celor două tipuri', not rute, str(rute[:3]))

    print(f'Total: {len(esecuri)} FAIL.', flush=True)
    raise SystemExit(1 if esecuri else 0)


if __name__ == '__main__':
    main()
