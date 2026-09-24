# 97. PIF nominalizează valoarea contabilă cu suport obligatoriu

- Data: 2026-09-24
- Stare: aprobată de owner, activă; (b) precizată de 098 ca invariant de poziție
- Docs: `docs/nucleu/tr-d7c-imobilizari-contract.md` IMO-B2; 087(e), 090, 094, 095.

## Regula durabilă

(a) PIF nominalizează pe fișă valoare contabilă existentă. Intrarea și
modernizarea nu creează valoare contabilă nouă. Suportul este o FCT operată,
o notă contabilă operată sau deschiderea; lipsa ori insuficiența suportului
refuză atomic operația. Se verifică separat brutul și amortizarea inițială.

(b) Nominalizarea este Transfer pe același cont și aceeași latură, din
poziția fără fișă pe fișa aleasă, cu dimensiunile suportului. Nu schimbă
soldul sau rulajele generale. Alocarea reține proveniența, împiedică
supraconsumul inclusiv concurent și păstrează plafonul pe linia FCT.

(c) Suportul alocat nu poate fi anulat sau inversat cât timp nominalizarea
este vie. Inversa PIF eliberează exact alocarea sa; auditul alocărilor nu
devine sursă paralelă de solduri.

(d) Baza fiscală se postează distinct în Carte=Fiscal: debitul contului
activului pe fișă se echilibrează cu creditul aceluiași cont fără fișă.
Amortizarea fiscală inițială este credit pe fișă și debit fără fișă pe
contul amortizării. Perechile sunt Operare, fără TVA; contraponderile nu
intră în situația fișei, în contabilitate sau în partide.

(e) 087(e) este amendată pentru intrările fără suport, iar 090 este
concretizată pentru nominalizarea fișei. Restricția stornării în luna
documentului din 087(g) rămâne în vigoare. Implementarea și activarea
PIF/AMO/CAS respectă contractul complet și precedă TR-D8 (095).

## Alegerea owner-ului

Owner-ul a aprobat explicit „A — nominalizare cu suport obligatoriu”.
Varianta B, prin care PIF ar crea valoare nouă cu o contrapartidă explicită,
nu este adoptată. Cazul fără suport (brut 1.200, cumulat 200, balanță zero)
devine refuz numeric probat, nu o poziție anonimă negativă fabricată.

## Urmărire

Implementarea și probele sunt urmărite prin 095-r1. Catalog: `scenarii/IMO.md`.

- 097-r1: diagnosticul istoriei PIF fără fișă, origine sau suport precedă
  activarea cititorilor TR-D8; seed-ul refuză azi istoricul incomplet, dar
  nu îl raportează pe o bază existentă.
- 097-r2: proveniența suportului nu are FK restrictiv; după inversarea PIF,
  suportul eliberat poate fi anulat fizic. Auditul anulării fizice rămâne
  pentru TR-D9 (091j); referința singură nu conservă conținutul sursei.
- 097-r3: comenzile IMO și anularea/stornarea suportului se serializează
  printr-un blocaj tranzacțional comun pe bază. Nu este mecanismul general
  al concurenței; direcția 091 (g)(4) rămâne blocajul per unitate.
