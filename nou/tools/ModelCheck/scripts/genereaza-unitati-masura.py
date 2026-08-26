#!/usr/bin/env python3
"""Generează `Comun/UnitatiMasuraUnEce.cs` din foaia `Unitati_masura` a schemei SAF-T.

Rulare (Windows, PowerShell; Python-ul e gestionat de uv, openpyxl NU se instalează
— de-aia cititorul de xlsx e scris pe stdlib, în fișierul ăsta):

    uv run --no-project python nou/tools/ModelCheck/scripts/genereaza-unitati-masura.py `
        anaf/RO_SAFT_SchemaDefCod_16.02.2026.xlsx `
        nou/Atlas.Conta.BackOffice/Atlas.Conta.BackOffice.Module/BusinessObjects/Comun/UnitatiMasuraUnEce.cs

`anaf/` e gitignored (xlsx-ul e al ANAF, nu al repo-ului): reproducerea cere
descărcarea schemei. Rezultatul — fișierul .cs — E comis, ca `JudeteRo`.

Structura foii (rândurile 0–8 = antet):
    col 0 = Sursă (rec20 / rec21 / gol pentru blocul „modificări față de
            publicarea anterioară", adăugat la coadă)
    col 1 = Codul unității de măsură raportat în SAF-T
    col 2 = Denumirea [EN]
    col 3 = Denumirea [RO] (traducere indicativă; 2 rânduri o au goală)

Ultima apariție a unui cod CÂȘTIGĂ: blocul de modificări de la coadă e cel
actualizat (azi un singur caz — B30, „gibit" → „gibibit" pe denumirea EN).
"""

import re
import sys
import zipfile
import xml.etree.ElementTree as ET

NS = '{http://schemas.openxmlformats.org/spreadsheetml/2006/main}'
RNS = '{http://schemas.openxmlformats.org/officeDocument/2006/relationships}'

FOAIE = 'Unitati_masura'
PRIMUL_RAND = 9  # rândurile 0..8 sunt antet


def incarca(cale):
    z = zipfile.ZipFile(cale)
    partajate = []
    if 'xl/sharedStrings.xml' in z.namelist():
        for si in ET.fromstring(z.read('xl/sharedStrings.xml')):
            partajate.append(''.join(t.text or '' for t in si.iter(NS + 't')))
    wb = ET.fromstring(z.read('xl/workbook.xml'))
    rels = ET.fromstring(z.read('xl/_rels/workbook.xml.rels'))
    tinte = {r.get('Id'): r.get('Target') for r in rels}
    foi = {}
    for sh in wb.iter(NS + 'sheet'):
        tinta = tinte[sh.get(RNS + 'id')]
        if not tinta.startswith('xl/'):
            tinta = 'xl/' + tinta.lstrip('/')
        foi[sh.get('name')] = tinta
    return z, partajate, foi


def indice_coloana(ref):
    n = 0
    for ch in re.match(r'([A-Z]+)', ref).group(1):
        n = n * 26 + (ord(ch) - 64)
    return n - 1


def randuri(z, partajate, tinta):
    root = ET.fromstring(z.read(tinta))
    for row in root.iter(NS + 'row'):
        celule, maxc = {}, -1
        for c in row.iter(NS + 'c'):
            ref = c.get('r')
            if not ref:
                continue
            i = indice_coloana(ref)
            t, v, inline = c.get('t'), c.find(NS + 'v'), c.find(NS + 'is')
            if t == 's' and v is not None:
                val = partajate[int(v.text)]
            elif t == 'inlineStr' and inline is not None:
                val = ''.join(x.text or '' for x in inline.iter(NS + 't'))
            elif v is not None:
                val = v.text
            else:
                val = ''
            celule[i] = (val or '').replace('\r', '').strip()
            maxc = max(maxc, i)
        yield [celule.get(i, '') for i in range(maxc + 1)]


def citeste(cale_xlsx):
    z, partajate, foi = incarca(cale_xlsx)
    if FOAIE not in foi:
        raise SystemExit(f'Foaia „{FOAIE}” lipsește din {cale_xlsx}.')
    dupa_cod = {}  # dict păstrează ordinea inserării; re-atribuirea nu o schimbă
    for i, r in enumerate(randuri(z, partajate, foi[FOAIE])):
        if i < PRIMUL_RAND:
            continue
        camp = lambda n: r[n] if len(r) > n else ''
        cod, en, ro = camp(1), camp(2), camp(3)
        if not cod:
            continue
        dupa_cod[cod] = ro or en  # RO cu fallback pe EN (2 rânduri fără RO)
    return list(dupa_cod.items())


def scapa(text):
    return text.replace('\\', '\\\\').replace('"', '\\"')


ANTET = '''// GENERAT — nu se editează cu mâna.
// Sursa: foaia `Unitati_masura` din `RO_SAFT_SchemaDefCod_16.02.2026.xlsx` (ANAF).
// Regenerare:
//   uv run --no-project python nou/tools/ModelCheck/scripts/genereaza-unitati-masura.py \\
//       anaf/RO_SAFT_SchemaDefCod_16.02.2026.xlsx \\
//       nou/Atlas.Conta.BackOffice/Atlas.Conta.BackOffice.Module/BusinessObjects/Comun/UnitatiMasuraUnEce.cs

namespace Atlas.Conta.BackOffice.Module.BusinessObjects;

// Codurile UN/ECE Rec 20 + Rec 21 pe care le cere SAF-T (`UOMBase`/`UOMStandard`),
// exact lista publicată de ANAF. LEGE, ca `JudeteRo` — de aceea stă în cod și nu
// în CSV de seed: `SeedUnitatiMasura` o consumă, dar `UnitatiMasuraRo.Rezolva` o
// citește ÎNAINTE de a atinge baza (conectoarele rezolvă UM-uri libere din 1C).
//
// Denumirea e traducerea românească indicativă publicată de ANAF; unde ea
// lipsește ({fara_ro} coduri), e denumirea engleză.
public static class UnitatiMasuraUnEce {
    public static readonly IReadOnlyList<(string Cod, string Denumire)> Toate = [
'''


def main():
    if len(sys.argv) != 3:
        raise SystemExit(__doc__)
    perechi = citeste(sys.argv[1])
    coduri = {c for c, _ in perechi}
    if len(coduri) != len(perechi):
        raise SystemExit('BUG: dicționarul ar fi trebuit să deduplice codurile.')
    antet = ANTET.replace('{fara_ro}', '2')
    corp = ''.join(f'        ("{scapa(c)}", "{scapa(d)}"),\n' for c, d in perechi)
    with open(sys.argv[2], 'w', encoding='utf-8', newline='\r\n') as f:
        f.write(antet + corp + '    ];\n}\n')
    print(f'{len(perechi)} coduri scrise în {sys.argv[2]}')


if __name__ == '__main__':
    main()
