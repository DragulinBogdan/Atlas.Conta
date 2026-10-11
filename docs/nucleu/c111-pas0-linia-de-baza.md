# C111, pasul 0 — linia de bază

- Tip: analiza
- Data: 2026-10-10
- Alimentează: 111 (raport de pas al contractului C111)

**Data:** 2026-10-10. **Stare:** măsurat. **Cod:** `bff2317` (main `1cc9d93`
plus documentele feliei și comanda `--amprente`; niciun cod de produs
schimbat). Contractul: [C111](c111-coordonate-contract.md), D9.

Urmele, neversionate: `run-verificari/c111-pas0/` și
`run-verificari/20261010-101909-039/`.

## Rezumat

| Ce | Cifra |
|---|---|
| Integrala ModelCheck, bugetar | 3.578 OK, 0 FAIL, 109 s |
| Integrala ModelCheck, privat | 4.859 OK, 0 FAIL, 185 s |
| Import Flax, ianuarie 2025 | 7 min 53 s; 16.597 documente, 18.770 tranzacții, 81.774 postări, 2.152 împerecheri |
| Reconcilierea importului | 10 verificări picate |
| Amprentele, două importuri ale aceluiași binar | identice pe toate cele cinci secțiuni; raportul de reconciliere identic |
| Amprentele, trei postări modificate de mână | toate cele cinci secțiuni diferă |

## Integrala

`scripts/verifica.ps1 -Suita Integral -Profil Ambele -Sufix .C111P0
-PregatesteBaze`, pe instanța uneltelor (5446). Bazele
`Atlas.Conta.BackOffice.C111P0` și `Atlas.Conta.ModelCheck.Privat.C111P0`
rămân, ca linie de bază a pasului 1. Pe privat: `INV-CUB` pe 57 de scene,
`RAMURA-NUL` pe 144.763 de comenzi SQL, cu 5 forme admise nominal.

## Importul

`Import1C --recreeaza --cititori --pana-la 1`, binar Release, pe
`Atlas.Conta.Import1C.Flax.C111P0`. Baza rămâne: e referința pasului 6.

## Amprentele

`ModelCheck --amprente Atlas.Conta.Import1C.Flax.C111P0 2025 1 <director>`
(`docs/stare-curenta/dezvoltare-si-validare.md`).

| Secțiune | Rânduri | SHA-256 |
|---|---|---|
| `balanta.txt` | 95 | `FA86C7EE…48DFE258` |
| `stoc.txt` | 13.591 | `BA68000E…847E9510` |
| `partide.txt` | 11.865 | `47B93111…EEA28F93` |
| `d300.json` | 57 | `3A156D87…7C5C2DDA` |
| `d394.json` | 2.596 | `1E321D1D…55F4D9A3` |

D394 are 2.583 de operațiuni, ca la pasul 0 al TR-D9b.

**Stabilitatea.** Un al doilea import, cu același binar, pe o bază nouă:
`amprente.txt` identic, zero rânduri diferite pe fiecare secțiune, raportul
de reconciliere sortat identic, aceleași patru numărători.

**Martorul.** Pe a doua bază, trei postări modificate de mână: partenerul
scos de pe o postare fiscală, gestiunea schimbată pe o postare de stoc, un leu
adăugat pe o postare de partidă. Rânduri diferite față de referință: balanța
2, stocul 3, partidele 2, D300 2, D394 7. Baza modificată e ștearsă.

## SAF-T

- **Modulul S** se generează pe baza de import (`Import1C --saft-s 2025 1`):
  15.758 de intrări de stoc fizic, 8.618 mișcări, cusătura S1 verde.
  Validatorul oficial refuză un cod de marfă (`85171200`), ca dată a sursei.
  Fișierul XML poartă 8.124 de identificatori generați, deci nu se compară
  la octet între importuri; se compară raportul lui pe secțiuni
  (`saft-s-2025-01-raport.txt`), fără antet și fără timpi.
- **Modulul L** nu se generează pe ianuarie 2025: produsul îl refuză cu
  `SAFT_MAPARE_LIPSA` (maparea de taxe pentru cota de 19 %). Pe import nu
  există deci amprentă pentru L; oracolul lui rămân scenele SAF-T din
  ModelCheck.

Regula de oprire 1 a contractului e precizată în consecință.

## Ce a rămas pe disc

| Bază (5446) | Mărime | Rost |
|---|---|---|
| `Atlas.Conta.Import1C.Flax.C111P0` | 108 MB | referința pasului 6 |
| `Atlas.Conta.BackOffice.C111P0`, `Atlas.Conta.ModelCheck.Privat.C111P0` | câte 17 MB | integrala de bază |
