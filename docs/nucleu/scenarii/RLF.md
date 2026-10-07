# RLF — retur la furnizor

2026-09-23: așteptări specificate înaintea implementării. Reguli: TR-D7b
T-D6/T-D8, 076(d–g), 088(h), 090(i), 091(a–e). `ScenariiRlf`, anul2009,
marcaj `E2E-SC-RLF`, documente reale FCT→NIR conex pentru fixture, comenzi
în ObjectSpace-uri noi și curățenie în `finally`.

Privat: marfă371, furnizor401, TVA4426/4427. Bugetar: fără politici RLF,
fără activare pe cub. Valorile sunt RON; cantitatea și valoarea returului
sunt semnate negativ de operare. Valoarea fiscală este cantitate×prețul
lotului, nu soldul preluat la golire; reziduul valoric este declarat prin
T-D6/T-r2 și nu se ascunde într-o ajustare de preț.

| ID | Pași și așteptare numerică | Rezultat | Proveniență | Proba | Stare |
|---|---|---|---|---|---|
| SC-RLF-01 | Recepție10/100; RLF2 fără TVA: D371−20 cu Q−2, C401−20 cu Q+2 pe capătul virtual Furnizor. Stoc8/80, partida proprie RLF+20; partida facturii rămâne−100. Dry-run fără scriere. | acceptat | T-D6 | `ScenariiRlf` / ID | verificat, privat |
| SC-RLF-02 | Două loturi10/100 și5/100; retur2 N21 și3 N11: stoc8/80 și2/40; baze−20/−60, taxe−4,20/−6,60; partida RLF+90,80. | acceptat | recensământ multicotă și multiline | `ScenariiRlf` / ID | verificat, privat |
| SC-RLF-03 | Storno RLF2/20 în aceeași lună: +20 pe ambele laturi, Q invers, stoc10/100, partida0; originalele intacte, repetarea refuzată. | acceptat / refuz stare | 090i | `ScenariiRlf` / ID | verificat, privat |
| SC-RLF-04 | Anulare RLF2/20 → zero efecte proprii, stoc10/100; reoperare →8/80, fără dublarea semnului. | acceptat | 076e | `ScenariiRlf` / ID | verificat, privat |
| SC-RLF-05 | Recepție3 cu preț0,333333 și valoare1; ieșire DSC2 de0,67; RLF ultima bucată la0,33 → stoc0/0. Separat, recepție3×10,006667=30,02; DSC1 apoi DSC1 (10,01+10,01), RLF ultima bucată10,01 → cantitate0, reziduu−0,01. | acceptat, reziduu declarat | T-D6, T-r2 | `ScenariiRlf` / ID | verificat, privat |
| SC-RLF-06 | Recepție100 → RLF20 → NTC401=401:20 pe furnizor: FCT−80, RLF0; storno RLF cu nominalizare activă refuzat atomic; storno NTC redeschide FCT−100/RLF+20. | acceptat / refuz dependență | T-D6, 090e | `ScenariiRlf` / ID | verificat, privat |
| SC-RLF-07 | RLF2/20 N21 în ianuarie închis: anulare refuzată; storno februarie +20/+4,20, fiscal februarie; stoc8/80 la31.01,10/100 în februarie. | acceptat pentru storno | 088 | `ScenariiRlf` / ID | verificat, privat |
| SC-RLF-08 | Corecție RLF2 N21 în februarie: original inversat, draft legat schimbat la1 →−10/−2,10, fiscal ianuarie; stoc9/90, partida nouă+12,10. | acceptat | 088h | `ScenariiRlf` / ID | verificat, privat |
| SC-RLF-09 | RLF Data05.01, înregistrat05.02: stoc10/100 la31.01,8/80 la05.02, partida+20 numai în februarie. | acceptat | 088 | `ScenariiRlf` / ID | verificat, privat |
| SC-RLF-10 | Lot10/100, retur2 TI21: baza−20 pe371, taxă−4,20 pe4426 contra4427; partida401+20; fără fapt colectat suplimentar (B-r4). | acceptat, limită declarată | recensământ:96 linii TI | `ScenariiRlf` / ID | verificat, privat |
| SC-RLF-11 | Fără linii / lot absent / cantitate0 / Tip incompatibil / TVA capitalizat / cantitate peste stoc: refuz cu zero efecte. Coduri de declarație unde operandul permite; insuficiența stocului prin gardianul registrelor. | refuzat | T-D6, 076 | `ScenariiRlf` / ID | verificat, privat |
| SC-RLF-12 | Bugetar: zero politici RLF. Dry-run și operare refuzate cu `TIP_FARA_DECLARATIE`, fără număr cules, zero efecte. Înaintea tăierii dry-run-ul trecea și operarea cădea pe numerotarea absentă. | refuzat | seed; D9-D5, D9-A6 (I6) | `ScenariiRlf` / ID | verificat, bugetar (TR-D9a, pasul 6) |
| SC-RLF-13 | După lotul gol cu reziduu−0,01 din SC05, RDC readuce1 la10,01: lot1/10. DSC golește lotul la soldul cubului10: lot0/0. Valoarea notei RLF și partida10,01 rămân intacte. | acceptat, diferență de evaluare declarată | review T-D7b pas8, T-D6/T-r2/T-r7 | `ScenariiRlf` / ID | verificat, privat |
| SC-RLF-14 | Bugetar, același RLF cu număr cules: același refuz `TIP_FARA_DECLARATIE`, documentul rămâne Draft; lotul rămâne 10/100. Înaintea tăierii rămânea Operat fără nicio postare. | refuzat | D9-D5, schimbarea 4 | `ScenariiRlf` / ID | verificat, bugetar (TR-D9a, pasul 6) |

Recensământ read-only `Atlas.Conta.Import1C.Flax.TrD7b`, script
`recensamant-retururi-dvi.sql`:398 documente/477 linii,35 multiline,
maximum11 linii, o multicotă; toate liniile pe lot, cantitate și valoare
negative, zero loturi create de retur, zero întârziate/corecții legate.
TVA:276 N19,104 N21,59 TI19,37 TI21,o linie fără TVA.

Cititorii comuni și proiecțiile reconstruibile rămân TR-D8; reziduul,
nominalizarea directă pe factura originală și registrele sunt TR-D9.
Lanțul cu ASM din SC-X-09 se probează la migrarea ASM, în pasul5;
scenariile prezente nu îl declară acoperit.

## Validare

`verifica.ps1 -Suita Scenarii -Tip RLF -Profil Ambele -Sufix .CodexBCS`:
3 OK bugetar / 72 OK privat, zero FAIL, inclusiv SC-RLF-13. Manifest:
`run-verificari/20260923-124252-904/rezultat.json`.

Gate integral pe aceleași baze: 1.746 OK bugetar / 2.455 OK privat,
zero FAIL, exit0 la build și ambele rulări. Manifest:
`run-verificari/20260923-124346-478/rezultat.json`.

## TR-D9a, pasul 2 (2026-10-05): valoarea liniei la prețul de intrare

Sursa (b) din D9-D3 ([contractul TR-D9a](../tr-d9-taierea-contract.md)): linia de retur ia `cantitate × Lot.PretUnitar`
și declarantul o postează ca `ValoareDeclarata`. Nu se schimbă la tăiere;
cifrele de mai jos sunt măsurate pe LINII (`ScenariiRlf`, privat) și rămân
aceleași după pasul 6. Linia operată poartă semnul operării.

| ID | Pasul | Valoarea liniei |
|---|---|---|
| SC-RLF-05 | retur 1 din lotul 3 × 0,333333 după DSC 2 | −0,33; lot 0/0 |
| SC-RLF-05 | retur 1 din lotul 3 × 10,006667 după două DSC de 1 | −10,01, nu soldul rămas 10,00; lot 0/−0,01 |
| SC-RLF-05 | același retur, anulat și reoperat | −10,01; lot din nou 0/−0,01 |
| SC-RLF-08 | corecția în februarie, cantitate 1 | −10,00 |
| SC-RLF-13 | RDC readuce 1 pe lotul cu reziduu | costul −10,01; lot 1/10 |
| SC-RLF-13 | DSC golește lotul | 10,00, evaluată din sold (sursa a); lot 0/0 |

Returul pe un lot inițial fără preț de intrare: SC-DES-22 în
[DESCHIDERE](DESCHIDERE.md).
