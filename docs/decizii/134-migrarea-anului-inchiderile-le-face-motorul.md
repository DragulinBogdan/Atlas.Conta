# 134 — Migrarea anului: închiderile lunii și exercițiul le face motorul, iar conectorul ține poziția acolo unde postează documentele

- Data: 2026-10-17
- Stare: propusă
- Execuție: etapa E4, pasul 3 (2026-10-17), branch `e4-pas3-an`; anul 2025 cu bază recreată și `--inchide-lunile`: 12/12 luni, contractele 1–5 îndeplinite, perioadele și exercițiul 2025 închise, 1:23:10
- Arii: import, perioade, imobilizari, partide
- Amendează: 133 (g)
- Închide: 107-r7, 107-r8, 107-r1, S-r5, E3-r6, FZ-r3
- Docs: `docs/import/e4-migrarea-contract.md` (pasul 3); `docs/stare-curenta/import.md`

## Regula durabilă

**Pe baza de import, închiderile le face motorul: amortizarea, TVA-ul, rezultatul lunii și exercițiul. Rândurile 1C pe care le acoperă nu se transcriu, iar diferența față de ele e divergență cu nume. Conectorul pune fiecare sold pe coordonata pe care postează documentele.**

(a) **Rezultatul lunii îl închide IRZ-ul motorului** (124 a), generat după ITV. Rândurile închiderii de lună 1C dintre `121` și un cont-sursă al politicii `Lunar` (6x, 7x) nu se transcriu. Diferența 1C − Atlas pe perechea de conturi e divergența „închiderea rezultatului”. Perechea se ține într-o singură orientare, iar rândul de orientare inversă intră cu semn schimbat. Așa, reclasificarea IRZ între cheile aceluiași cont se anulează, iar netul pe cont e diferența deja numită pe 6x/7x care trece în 121.

(b) **Exercițiul de import se închide prin comenzile motorului.** `--inchide-lunile` închide fiecare lună fără eșec, cu constatările ei acceptate pe cheie. După decembrie închide exercițiul (124 d), tot cu constatările acceptate pe cheie. Soldurile de la 01.01 ale anului următor sunt soldurile de la 31.12 (124 e). Se compară:
- pe cont, cu `Balanta` 1C la 1 ianuarie (contractul 1 al lui decembrie);
- pe stoc, cu `BalantaNivel3` (contractul 3);
- pe partide, cu `BalantaNivel1` (contractul 5).

(c) **Poziția fără fișă stă pe SEDIU la deschidere.** Pe conturile de imobilizări (20x, 21x, 28x), soldul de control al deschiderii poartă repartitorul pe care postează documentele: SEDIU, implicitul notei. Gardianul poziției fără fișă grupează pe repartitor. Fără SEDIU, o notă care creditează poziția ar vedea o a doua poziție, goală.

(d) **Puntea NTC e la bani.** Linia notei-punte se rotunjește la scara de bani. Ce taie rotunjirea e divergența „Punte NTC: valoarea sursei peste scara de bani” (107-r8). O linie rotunjită la zero se sare.

(e) **Trecerea 2 aplică perechile în ordinea datei stingătorului.** Departajarea se face pe vedere, stingător și țintă, ordinal (107-r7). Raportul contractului 5 e ordonat pe cheia partidei și pe documentul 1C. Reproductibilitatea (116, E4-D7) se judecă pe chei naturale: legătura `ProdusRealocat` poartă id-ul Atlas al produsului și se compară pe codul lui.

(f) **Contractul 5 mai numește un fapt și judecă refuzul pe existență.** Faptul nou: rândul unei note care mărește partida numită în sensul ei. Motorul îl ține pe partida notei (115), iar 1C îl pune pe poziție. O partidă „are refuz” când trecerea 2 a refuzat cel puțin o stingere pe ea, chiar dacă refuzurile de semn opus se anulează în sumă.

(g) **Casarea unui MF cu fișă e CAS**, cu cauza Casare, ca vânzarea (133 g). CAS-ul acoperă rândurile documentului 1C cu subconto MF cu fișă. Restul rândurilor se transcriu.

(h) **Partidele neatinse de trecerea 2 se atribuie pe document**, ca cele picate (132 e). Una explicată pe nume se numără „pe document”. Restul rămân declarate cu sumă: stingerile prin facturi rămân în afara trecerii 2 (107-r1).

## Context

Pasul 3 al etapei E4 cere anul 2025 12/12, IRZ pe decembrie, exercițiul închis, soldurile de la 01.01.2026 față de 1C, reproductibilitatea (E4-D7) și FZ-r3. Prima rulare a anului (`an1`, fără `--inchide-lunile`) a fost verde pe ianuarie–mai. Iunie a picat pe 107-r8. O lună cu eșec nu-și generează AMO și ITV, iar cronologia strictă a amortizării a dus lunile de după în cascadă. Iulie a mai scos (c): Operatia 31.07 credita 212.1 cu 117.283 pe un MF fără parametri, rezerva din reevaluarea clădirii vândute, iar gardianul a refuzat-o. Rularea a căzut în octombrie pe timeout-ul contractului 5. Cauza era `Contains` pe o subinterogare nulabilă în EF.

Până la acest pas, importul transcria închiderea de lună 1C și lăsa sold pe cheile fine ale lui 6x și 7x. `--inchide-lunile` cobora `RezNeinchis` la avertisment pe baza de import. Exercițiul n-ar fi trecut de `EX-SOLD-REZIDUAL`.

## Tranșări

- (a) IRZ de motor, nu transcrierea 1C: închiderea 1C e pe cont, iar 124 închide pe atomul complet. Transcrierea plus IRZ ar fi închis de două ori pe cont și ar fi lăsat 121 cu divergențe fără pereche. Cu IRZ-ul de motor, regula e cea de la AMO (133 e) și ITV: motorul generează, iar rândurile 1C acoperite devin comparație.
- (b) Soldul pe cont se compară cu `Balanta`, nu cu `BalantaNivel3`. La 2026-01-01, `BalantaNivel3` are doar 28 de conturi (cele cu trei niveluri de subconto). Pe cele 27 comune cu `Balanta` e egală cu ea. Textul pasului 3 din contract numea `BalantaNivel3` pentru tot.
- (c) SEDIU în conector, nu gardian relaxat: gardianul pe repartitor e regula modelului (SC-IMO-27, altă gestiune). Deschiderea alege repartitorul, iar `SoldInitial.Repartitor` exista deja.
- (e) E4-D7, pe ianuarie–martie 2025, cu același binar pe două baze recreate (A 22:15, B 20:52): 111.577 de legături, 7.308 solduri de partidă și 17.429 de solduri de lot, identice pe chei naturale. Cele 395 de legături `ProdusRealocat` diferă doar prin id, iar pe codul produsului sunt identice. Raportul de reconciliere avea aceleași rânduri, dar ordinea documentelor dintr-o partidă atribuită varia. Cauza erau unirea cheilor și alegerea legăturii fără ordine. Acum ordinea e fixă. 116-r2 trece în „după PoC”: departajarea loturilor pe identificator n-a produs nicio diferență.
- (h) atribuire, nu declarare globală: 132 a lăsat judecata pentru pasul 3. Atribuirea numește ce se poate numi, iar declararea cu sumă rămâne pentru rest. Atribuirea citește sursa pe lot, o trecere pe lună pentru toate pozițiile: `flax.DefalcareNote` (14 mil. de rânduri) și `NoteContabile` n-au indecși. Cu o citire pe poziție, contractul lunar urca de la 5–20 s la 1,5–2 min.

## Ce rămâne deschis

- Reevaluarea MF din 31.12.2025 (212.3 = 105.1, 261.494,13) intră ca notă pe poziția fără fișă. Atlas n-are document de reevaluare pe fișă, iar fișa rămâne la valoarea de dinainte. E limită numită a importului.
- SAF-T L pe decembrie 2025 se oprește pe maparea cotei de 19% pe facturile din decembrie (`SAFT_MAPARE_LIPSA`). Intră la pasul 4, cu DUK.

## Istoric

- 2026-10-17: scrisă la pasul 3 al etapei E4; propusă owner-ului.
