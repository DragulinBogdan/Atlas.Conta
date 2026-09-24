# DEC — decontul titularului

2026-09-24. Specificat înaintea codului, 095 și DEC-B1…B3.
Recensământ read-only `Atlas.Conta.Import1C.Flax.TrD7b`: zero documente
Decont/PIF/AMO/CAS; nu constituie oracol. An propriu scenei: 2018.
E = cheltuiala profilului (628 / 628.00.00), A = 542 / 542.01.00.
Titularul este Angajat. Sumele sunt constante independente de registre.
Urmărirea partidei A este aprobată prin 096; titularul rămâne Angajat.

| ID | Scenariu și așteptare | Rezultat / stare |
|---|---|---|
| SC-DEC-01 | Fără TVA, 100: D E 100, C A 100, zero cantități; dry-run fără efecte | acceptat, verificat |
| SC-DEC-02 | Două linii 40 și 60: total 100, o partidă a titularului pe A; creditori expliciți diferiți au partide distincte | acceptat, verificat |
| SC-DEC-03 | Storno în ianuarie: inversele exacte −100, sold zero; a doua stornare refuzată | verificat pe profilurile aplicabile |
| SC-DEC-04 | Storno în februarie după închiderea lui ianuarie: ianuarie păstrează 100, februarie net zero; TVA inversei în februarie | verificat pe profilurile aplicabile |
| SC-DEC-05 | Anulare: zero efecte, reoperare produce aceleași valori | verificat pe profilurile aplicabile |
| SC-DEC-06 | Corecție după închidere: original 100, invers −100, document nou 80; sold 80, original imuabil | verificat pe profilurile aplicabile |
| SC-DEC-07 | DEC 100, PLT 40: rest 60; desfacere: 100; storno după desfacere permis; dependenți blochează inversarea directă | verificat pe profilurile aplicabile |
| SC-DEC-08 | Privat normal 100/21: D E 100, D 4426 21, C A 121; Capitalizat 121: bază 100 + taxă 21 pe E; TI21: E/A 100 și 4426/4427 21 | verificat pe profilurile aplicabile |
| SC-DEC-09 | Bugetar CAP21: cost 121, fără fapt TVA; privat două cote 100/21 și 100/11: total A 232; taxă culeasă 20,99 păstrată | verificat pe profilurile aplicabile |
| SC-DEC-10 | Cont explicit net diferit: E/cont explicit 100; taxa 4426/A 21 continuă prin politica TVA, fără mutarea ei după net | verificat pe profilurile aplicabile |
| SC-DEC-11 | Sumă nepozitivă, lipsă regulă/cont, latură internă ca titular, perioadă închisă: refuz, zero efecte | verificat pe profilurile aplicabile |
| SC-DEC-12 | Cantitate culeasă zero cu preț 100: pregătire q=1, valoare 100; cantitatea nu se postează în cub | verificat pe profilurile aplicabile |
| SC-DEC-13 | 2/51 linii: citire pe set, număr constant de interogări | verificat pe profilurile aplicabile |
| SC-DEC-14 | Document întârziat: dată fizică ianuarie, înregistrare februarie, contabil și declarare deductibilă în februarie | verificat pe profilurile aplicabile |
| SC-DEC-15 | Titular cu partidă de avans pe A rămâne Angajat și nu intră ca Supplier în SAF-T | verificat pe profilurile aplicabile |
| SC-DEC-16 | PLT 50 pe angajat: D A 50 cu partidă, C casă 50; rolul comercial rămâne Niciunul. Operand DEC cu UrmarestePartide=false păstrează suma 100 fără unitate | verificat pe profilurile aplicabile |
| SC-DEC-17 | PLT avans 150 → DEC 100 → INC restituire 50: după prima stingere rest avans 50, rest decont 0; după restituire toate cele trei partide au sold 0 | verificat pe profilurile aplicabile |

Citirile de producție și reconstrucția Sold rămân TR-D8. Cazurile fiscale
sunt active numai pe profilul cu PoliticaTva; configurarea se citește din
seed, nu se schimbă PosteazaInCub în scenă.

## Verificare 2026-09-24

- `pwsh -NoProfile -File nou/tools/ModelCheck/scripts/verifica.ps1 -Suita Scenarii -Tip DEC -Profil Ambele -Sufix .CodexBCS`: exit 0, `run-verificari/20260924-083815-639/rezultat.json` (lotul inițial).
- `pwsh -NoProfile -File nou/tools/ModelCheck/scripts/verifica.ps1 -Suita Integral -Profil Ambele -Sufix .CodexBCS`: **2.413 bugetar / 3.503 privat**, zero FAIL, build fără avertismente, exit 0. Manifest: `run-verificari/20260924-085048-477/rezultat.json`.
- Citiri operand pentru 2/51 linii: **7/7 bugetar, 8/8 privat** în suita integrală finală.
- Migrația `20260924053041_UrmarirePartide` aplicată numai pe cele două baze `.CodexBCS`; seed-ul menține rolul Niciunul pe 542. Metadata, OpenAPI și tipurile TypeScript regenerate; compilare cu TypeScript local 5.9.3, exit 0.
- Recensământ reproductibil: `recensamant-dec-imo.sql`, tranzacție read-only pe `Atlas.Conta.Import1C.Flax.TrD7b`: 0 pentru fiecare dintre cele patru tipuri.

SC-DEC-11 verifică lipsa regulii/contului pe operandul închis al declarantului;
refuzurile de valoare/linii/latură/perioadă trec și prin comenzile reale, cu
zero efecte după refuz. SC-DEC-16 probează atributul oprit pe operand și
avansul bugetar de 50 pe calea reală. Celelalte scene folosesc documente reale,
ObjectSpace nou per comandă și purjă finală. Raport: `run-nucleu/tr-d7c/dec/raport.md`.

Limite: citirile de producție și snapshot-urile rămân pe registre până la
TR-D8; migrația nu reconstruiește unitățile istorice (096-r1). Oracolul
normalizat rămâne înghețat: proba veche NUC-PLT pe avans 542 a fost înlocuită
cu așteptarea numerică SC-DEC-16, nu cu o nouă normalizare. Nicio portare
PIF/AMO/CAS și niciun import 1C nu sunt incluse în această validare.
