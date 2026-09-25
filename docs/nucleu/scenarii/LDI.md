# LDI — lista diferențelor de inventar

2026-09-24. **Implementat; validare la sfârșitul fișierului.** Reguli: 093, TR-D7b T-D9,
T-D13, `../tr-d7b-ldi-contract.md`, 091 (a–d).

## Recensământ

Recensământul comun deja executat pe clona Flax TrD7b:
`recensamant-asm-ldi-nir.sql`, rezultat
`run-nucleu/tr-d7b/pas5-asm/recensamant.json`. LDI: 18 documente, 22 linii,
toate Plus pe 371; două documente cu mai multe produse. Zero minusuri,
documente mixte, autogenerate sau întârziate. Așteptările minusului și
ciclului complet vin din contract, nu din cele 18 exemple externe.

## Matrice numerică

În exemplele comune: S = cont stoc 302 (bugetar 302.01.00), C = cheltuială
602 (602.01.00), V = venit 7588 (791.00.00). O postare reală poartă lot,
produs și gestiune; contraponderea plusului e Inventar, a minusului Consum,
fără unitate. Ambele capete au cantități opuse și aceeași valoare pozitivă.
Fără TVA, partidă sau valută. Proveniență: regulă, dacă nu se spune altfel.

| ID | Scenariu și așteptare independentă | Rezultat |
|---|---|---|
| SC-LDI-01 | Plus 2 × 15: D S/C V 30; lot nou 2/30; contrapondere −2 în Inventar. Dry-run fără efecte; prețul lotului încă 0 nu schimbă declarația. | acceptat |
| SC-LDI-02 | FCT/NIR 10/100; Minus 4: D C/C S 40, lot 6/60; contrapondere +4 în Consum. Cantitate de draft +4 sau −4 cu direcție Minus dă aceeași declarație. | acceptat |
| SC-LDI-03 | Sursă 5/100; listă Minus 2/40 și Plus 3/21: patru postări, sursa 3/60 și lot nou 3/21, fără compensarea liniilor. | acceptat |
| SC-LDI-04 | Storno Plus 2/30 în aceeași lună: original intact, invers −2/−30, lot 0/0; repetarea stornoului refuzată fără efecte. | acceptat/refuz stare |
| SC-LDI-05 | Minus 4/40 în ianuarie; închidere; storno în februarie: sursa 6/60 la 31.01 și 10/100 în februarie. | acceptat |
| SC-LDI-06 | Anulare listei mixte din 03 în lună deschisă: zero postări proprii, sursa 5/100, plusul 0/0; reoperare identică. | acceptat |
| SC-LDI-07 | Plus 2/30; corecție după închiderea lunii: inversare 2/30, document legat cu lot nou; plus nou 3/60, lot original 0/0, nou 3/60. | acceptat |
| SC-LDI-08 | Lot 3/10; minus 1 și 2 pe două linii în același document: cost 3,33 și 6,67; sold 0/0. | acceptat |
| SC-LDI-09 | Lot 3/10; trei documente Minus 1: cub 3,33 + 3,34 + 3,33, sursa 0/0. | TR-D8: sold propriu al cubului |
| SC-LDI-10 | Plus 1/25 urmat de BCS 1/25: storno plus refuzat; inversarea BCS permite storno plus, sold 0/0. | refuz dependență, apoi acceptat |
| SC-LDI-11 | Minus 1/10 urmat de Plus pe același produs: lot nou distinct, solduri separate, fără reutilizarea lotului vechi. | acceptat |
| SC-LDI-12 | Data 05.01, înregistrare 05.02 după închiderea lui ianuarie: plusul apare doar în februarie; înregistrare în ianuarie refuzată atomic. | acceptat/refuz perioadă |
| SC-LDI-13 | Direcție absentă, cantitate zero, lot absent, preț zero/negativ, plus cu lot străin sau gestiune greșită, minus pe lot propriu, stoc insuficient. | coduri stabile + zero efecte |
| SC-LDI-14 | Tip/produs incompatibil, natură fără stoc, cont absent, regulă contabilă absentă ori regulă de stoc neacoperită; Custodie explicit neacoperită. | refuz atomic |
| SC-LDI-15 | Pe toate operările, anulările și inversările: postări exacte, fără fapte fiscale și fără partidă; SC-X-14. | acceptat |
| SC-LDI-16 | Citirea operandului pentru 2 și 51 de linii în ObjectSpace nou: același număr de interogări, ≤16; declarație deterministă. | acceptat |
| SC-LDI-17 | Plus 2/30, Minus 1/15 pe marfă: D 371/C 7588 30, D 607/C 371 15; lot 1/15. | acceptat privat |
| SC-LDI-18 | Folosinta: FCT/NIR MAG1 4/100 → BTR MAG2 2/50 → LDI Minus 1/25 → BCS Minus 1/25; MAG1 2/50, MAG2 0/0, conturi 303.02.00 și 603. BTR cu ieșiri refuză storno; inversarea BCS, LDI, BTR reface MAG1 4/100 și MAG2 0/0. | acceptat/refuz dependenți |
| SC-LDI-19 | Folosinta: Plus LDI MAG2 1/25; anulare/reoperare, apoi storno, lot 0/0; D 303.02.00/C 791.00.00 25 și inverse. | acceptat |
| SC-LDI-20 | Plus 0,001 bucăți la preț pozitiv 0,001: cantitate 0,001 și valoare rotunjită 0; două postări, fără inventarea unui ban. | acceptat |
| SC-LDI-21 | Folosinta: Plus MAG2 2/50; corecție după închiderea lunii, noul plus 3/60 pe lot nou; la 31.01 original 2/50, în februarie original 0/0 și nou 3/60. | acceptat |
| SC-LDI-22 | Lot OF creat real prin LDI sub politica Magazie; restaurare Folosinta. BTR nou refuză stocul inexistent pe noua cheie. Storno inversează exact Magazie 1/25, original intact; diagnosticul arată separat cheia veche și inversul, chiar la sold net zero. | refuz fără fallback / inversare acceptată |

## Limite

Cititorii comuni și snapshot-urile pe cub sunt TR-D8; reevaluarea și
compensarea, propriul sold al cubului și reziduurile duale rămân T-r13/TR-D9.
Custodie rămâne neacoperită, aprobat de owner; Folosinta intră acum pe
întregul lanț în gestiune reală, conform 093 și LDI-B3. Privat nu are o
clasă Folosinta seed-uită; lanțul special este probat pe bugetar, iar
declarantul rămâne generic. Consumul BCS păstrează lotul pe locul de consum;
contraponderea minusului LDI rămâne Consum fără unitate.

## Execuție

### Validare Folosinta (2026-09-24)

`pwsh -NoProfile -File nou/tools/ModelCheck/scripts/verifica.ps1 -Suita Scenarii -Tip LDI -Profil Ambele -Sufix .CodexBCS`:
**176 bugetar / 124 privat OK, zero FAIL**, exit 0 comun,
`run-verificari/20260924-001328-835/rezultat.json`. Include SC-LDI-18/19/21/22
pe bugetar. Prima încercare (`20260924-001132-997`) a oprit proba pe simbolul
inexistent 603.02.00 din exemplu; corectura documentată în LDI-B3 folosește
603 din politica existentă, fără modificarea contării.

Regresie integrală finală:
`pwsh -NoProfile -File nou/tools/ModelCheck/scripts/verifica.ps1 -Suita Integral -Profil Ambele -Sufix .CodexBCS`:
**2.179 bugetar / 3.221 privat OK, zero FAIL**, exit 0 comun, build fără
avertismente: `run-verificari/20260924-001419-601/rezultat.json`.
Nucleul nu a fost modificat după proba 178/178 de mai jos.
Raportul feliei: `run-nucleu/tr-d7b/pas5-ldi/raport.md`.

### Rulări preliminare (2026-09-23)

Rulare selectivă pentru domeniul implementat Magazie/Marfuri:
`pwsh -NoProfile -File nou/tools/ModelCheck/scripts/verifica.ps1 -Suita Scenarii -Tip LDI -Profil Ambele -Sufix .CodexBCS`.
Rezultat: **120 bugetar / 125 privat OK, zero FAIL**, exit 0 comun,
`run-verificari/20260923-234548-087/rezultat.json`.
Această rulare precedă implementarea SC-LDI-18/19 și aprobarea coordonatelor
Folosinta prin 093; nu constituie proba ramurii speciale.

Nucleu: `pwsh -NoProfile -File nou/tools/ModelCheck/scripts/verifica.ps1 -Suita Nucleu`:
**178/178**, zero omise, exit 0,
`run-verificari/20260923-235301-386/rezultat.json`.
Aceste rezultate sunt intermediare; LDI nu este declarat închis.

Regresie integrală: `pwsh -NoProfile -File nou/tools/ModelCheck/scripts/verifica.ps1 -Suita Integral -Profil Ambele -Sufix .CodexBCS`:
**2.122 bugetar / 3.221 privat OK, zero FAIL**, exit 0 comun,
`run-verificari/20260923-235337-338/rezultat.json`.
Această rulare validează domeniul implementat; nu acoperă Folosinta.

Actualizare TR-D8 în lucru (2026-09-25): SC-LDI-22 verifică transferul
lotului istoric după schimbarea TipStoc. Coordonatele cubului păstrate
permit BTR; registrul original rămâne cu TipStoc istoric. Anularea BTR și
storno LDI restabilesc soldurile fără reclasificarea postărilor originale.
