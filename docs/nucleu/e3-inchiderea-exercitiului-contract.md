# E3 — închiderea exercițiului: rezultatul lunar în 121, exercițiul ca stare, anul nou din soldurile de la 31.12

- Tip: contract
- Data: 2026-10-16
- Trigger: owner, deschiderea etapei E3 după curățenia din PR #53 (2026-10-16)
- Livrează: F-PER-02…F-PER-06 (etapa E3 din [scop](../scop.md))
- Arii: perioade, politici, documente, motor, stoc, raportare, api, ecrane, verificare
- Decizii: 004, 014, 015, 029, 035, 088, 104, 119
- Stare: deschis

Owner-ul a tranșat la 2026-10-16, pe întrebările pasului 0: pe privat 121
trece în anul nou, iar transferul în 1171 se face după aprobarea
situațiilor (E3-D2); 6/7 se închid lunar, prin document generat (E3-D3);
lanțul perioadelor rămâne strict (E3-D6); bugetarul intră cu execuția
bugetară (E3-D9).

Azi nimic nu închide conturile de venituri și cheltuieli: 6 și 7 cumulează
de la începutul bazei, iar 121 nu primește rezultatul decât prin notă
manuală. Balanța nu are graniță de an: coloana `Initial` e suma întregului
istoric anterior perioadei (`ContabilProiectii`), deci în anul al doilea
„sumele precedente” cuprind și anul trecut. Corecția valorică judecă anul
închis după anul calendaristic al consumului (`CV_AN_INCHIS`, R8-9), în
locul unei operații explicite (CV-r2). Perioada fiscală are lanțul,
istoricul și snapshot-ul capătului de an (088), dar nu are noțiunea de
exercițiu închis (F27-r4).

## Scop

1. Închiderea lunară a conturilor de rezultat ca document generat și
   operat, cu conturile date de politică; constatarea ei la închiderea
   lunii.
2. Exercițiul închis ca stare a motorului: închiderea și redeschiderea
   lui, cu verificarea și istoricul.
3. Rândurile anuale ale închiderii (bugetar: 121 → 117, 489 → 117,
   execuția bugetară), din aceeași politică.
4. Balanța pe an: soldul inițial al anului și sumele precedente de la 1
   ianuarie.
5. `CV_AN_INCHIS` judecat pe exercițiul închis prin operație (CV-r2).
6. Ecranele XAF și ușile HTTP pentru comenzi, coloanele noi ale balanței
   în React, scenariile și regresia pe ambele profiluri.

## Ce NU intră

- Repartizarea profitului pe privat (121 → 1171, 129 → 121, dividende,
  rezerva legală 129 = 1061): rămâne notă contabilă manuală în anul
  următor (E3-D2). Documentul generat al repartizării e restanță.
- Impozitul pe profit (691 = 4411) și D101: nota rămâne manuală.
- Exercițiul financiar diferit de anul calendaristic: exercițiul e anul
  calendaristic (limită scrisă).
- Perioada de ajustare (luna 13): lanțul rămâne strict (E3-D6).
- Situațiile financiare anuale (bilanțul, contul de profit și pierdere,
  formularele bugetare): E3 le dă balanța corectă, nu formularele.
- Import1C: conectorul nu se atinge. Pe baza de import, o lună închisă
  deja de 1C trebuie să genereze închiderea fără linii (proba din pasul 1).

## D-uri pin-uite

### E3-D1 — regulile contabile (OMFP 1802, OMFP 1917)

- Conturile de cheltuieli (clasa 6) se închid prin creditare, cu debitul
  în 121; conturile de venituri (clasa 7) se închid prin debitare, cu
  creditul în 121. Suma închisă pe o cheie este soldul net al cheii la
  sfârșitul lunii, în sensul opus soldului.
- 1174 este rezultatul reportat din corectarea erorilor (F27-r1), nu
  destinația rezultatului. Profitul nerepartizat și pierderea neacoperită
  stau pe 1171 (privat); pe bugetar, rezultatul patrimonial se reportează
  în 117 (planul bugetar are 117.00).
- Pe bugetar, legacy-ul închidea anul prin „nota de deschidere an”: soldul
  conturilor 121 și 489 trecea în 117, cu dimensiunile păstrate (unitate,
  proiect, cod funcțional, cod economic, repartitor;
  `db/export/proc/spGenerareNotaDeschidere.sql`). E evidență, nu canon
  (21): E3 preia regula, nu procedura.

Textele vechi care spun „121 → 1174” (F-PER-02, F27-r4, SC-X-12) se
corectează în pasul 0.

### E3-D2 — 121 pe privat trece în anul nou (owner, 2026-10-16)

Închiderea exercițiului nu mută 121 pe privat. Soldul de la 31.12 devine
soldul inițial al anului următor, iar transferul în 1171 (cu stingerea
lui 129) se face după aprobarea situațiilor financiare, prin notă
contabilă. Seed-ul privat nu are rânduri anuale.

### E3-D3 — închiderea lunară e un document generat (owner, 2026-10-16)

- Frunză nouă `InchidereRezultat : NotaContabila`, pe tiparul închiderii
  de TVA: generare, previzualizare, regenerare, câte un document per lună
  și unitate, datat în ultima zi a lunii. E operație (invariantul I): intră
  în cub prin declarația fluxului, ca orice notă.
- Liniile vin din soldurile de la sfârșitul lunii (citirea cumulată
  `Integrala`) pe conturile-sursă din politică, câte o linie per cheie cu
  sold nenul. Cheia e atomul contabil complet: contul plus coordonatele
  laturii.
- Pe destinație se păstrează numai coordonatele declarate pe rândul de
  politică (E3-D4). Restul se pliază: mai multe chei-sursă dau o singură
  linie pe destinație. Coordonatele fiscale (`CodTva`,
  `PerioadaDeclarare`, `ReperFiscal`, `PartenerFiscal`) nu trec niciodată
  pe destinație.
- Închiderea e ultimul document al lunii. Un document operat după ea în
  aceeași lună și pe un cont-sursă lasă sold, iar constatarea
  `REZ-NEINCHIS` îl prinde (E3-D5); regenerarea refolosește tiparul ITV.
- Motorul nu cunoaște simbolurile: conturile-sursă și destinația sunt
  rânduri de politică (29). Un cont-sursă e dat prin părinte (6, 7), deci
  prinde și analiticele adăugate de client.

### E3-D4 — politica închiderii conturilor

Tabel nou `PoliticaInchidereConturi`, cu proveniență (`DinSeed`, 84a).
Un rând are:

- contul-sursă (sintetic sau grupă; prinde descendenții);
- contul-destinație;
- momentul: `Lunar` (în fiecare închidere lunară) sau `Anual` (numai la
  capătul exercițiului, E3-D7);
- ordinea: rândurile se aplică în ordine, pe soldul curent, ca 121 → 117
  să vadă rezultatul lunii închise înaintea lui;
- coordonatele păstrate pe destinație (flag-uri pe coordonate). Gardianul
  refuză un rând care nu păstrează coordonatele obligatorii ale
  destinației (decizia 15).

Seed-ul:

| Profil | Sursă → destinație | Moment |
|---|---|---|
| Privat | 6 → 121, 7 → 121 | Lunar |
| Bugetar | 6 → 121, 7 → 121 | Lunar |
| Bugetar | 121 → 117, 489 → 117 | Anual |
| Bugetar | execuția bugetară (E3-D9) | Anual |

Politica alege conturile, nu comportamentul (4): mecanismul este același
pe ambele profiluri.

### E3-D5 — constatarea `REZ-NEINCHIS` la închiderea lunii

Al cincilea fel de constatare de conținut în `PoliticaInchidere`: la
sfârșitul lunii, conturile-sursă cu moment `Lunar` au sold. Cheia e
`REZ-NEINCHIS`, fără sufix, ca `ITV-LIPSA`. Severitatea din seed:
`Blocant` pe ambele profiluri, fiindcă un sold rămas pe 6 sau 7 strică
soldul inițial al anului următor. Documentul de închidere aflat încă în
lucru nu se repetă ca `DRAFT-IN-PERIOADA` (regula „un fapt dă o singură
constatare”).

### E3-D6 — lanțul rămâne strict (owner, 2026-10-16)

Ajustările de an se fac cu decembrie deschis. Lunile anului nou se
operează, dar ianuarie nu se închide înaintea lui decembrie. Perioada de
ajustare (luna 13) e restanță: se deschide când lanțul strict blochează
un caz real.

### E3-D7 — exercițiul închis e starea lui decembrie

- Decembrie (`PerioadaFiscala`, luna 12) primește starea exercițiului:
  `ExercitiuInchis` și momentul închiderii, ale motorului, refuzate pe
  calea securizată ca starea perioadei (F27-D1). Istoricul refolosește
  `InchiderePerioada`, cu două feluri noi: închiderea și redeschiderea
  exercițiului. Nu există entitate separată: lanțul, blocajul și istoricul
  sunt deja ale perioadei.
- Închiderea exercițiului cere decembrie închis. Constatările ei sunt
  structurale: decembrie nedefinit sau deschis, exercițiul deja închis,
  exercițiul precedent definit și deschis. De conținut este una: conturile
  din politică (`Lunar` și `Anual`) au sold la 31.12, cu cheia
  `EX-SOLD-REZIDUAL`.
- Cât timp exercițiul e închis, redeschiderea lui decembrie se refuză.
  Redeschiderea exercițiului cere un motiv scris și se poate face numai pe
  ultimul exercițiu închis.
- Rândurile `Anual` intră în închiderea lunară a lui decembrie, după
  rândurile `Lunar`. Data lor e 31.12, deci nu se postează nimic în anul
  nou, iar redeschiderea exercițiului nu are nimic de stornat.
  **De confirmat de owner:** pe bugetar, data lui 121 → 117 (31.12 sau
  01.01, pe care o sugerează numele notei legacy). Dacă răspunsul e 01.01,
  rândurile anuale devin document propriu, generat de închiderea
  exercițiului, iar redeschiderea lui le stornează.

### E3-D8 — deschiderea anului nu postează; balanța are graniță de an

- Soldurile curg prin cub: soldul la 01.01 este soldul la 31.12. Nu se
  scrie nicio tranzacție `Deschidere` (aceea rămâne a migrării,
  invariantul I). Snapshot-ul decembrie închis rămâne referința capătului
  de an (F27-D3).
- Balanța și balanța pe plan primesc perechea `SoldInceputAn` (debit și
  credit, netă pe cheie, la 01.01 al anului `dataStart`). Coloana
  `Initial` devine „sume precedente”: `SoldInceputAn` plus rulajele de la
  01.01 până înaintea perioadei. Soldul inițial și soldul final rămân
  neschimbate. Granița e anul calendaristic, independent de închiderea
  exercițiului: o bază fără închideri dă aceleași cifre ca una cu
  închideri.
- Fișa de cont nu se schimbă: soldul anterior este deja net. Rămâne doar
  de verificat că SAF-T citește soldul de deschidere net (proba din pasul
  3).

### E3-D9 — execuția bugetară intră în E3 (owner, 2026-10-16)

Închiderile execuției bugetare sunt rânduri `Anual` în seed-ul bugetar,
pe același mecanism; nu se adaugă cod specific bugetarului. **De
confirmat de owner înaintea pasului 2:** lista exactă a rândurilor (de
exemplu 520.02 / 521.02 → 521.03, rezultatul execuției anului curent în
cel al anilor precedenți; închiderea finanțării 770 se face deja prin
7 → 121) și coordonatele păstrate pe fiecare (cod funcțional, cod
economic, sursă). Procedurile legacy `spNotaInchidere*` nu sunt în
`db/export/proc/`. Dacă owner-ul le are, ele sunt evidența.

### E3-D10 — `CV_AN_INCHIS` pe exercițiul închis (CV-r2)

Consumul beneficiar al unei corecții valorice dă `CV_AN_INCHIS` când anul
lui are exercițiul închis, nu când e anterior calendaristic anului
corecției. Nucleul primește prin context ultimul exercițiu închis (o
valoare, nu perioada), fără să cunoască perioadele. Teste de nucleu pe
ambele ramuri: an anterior deschis (acceptat) și an închis (refuzat).

## Restanțe

E3 preia F27-r4 și CV-r2. Restanțele noi ale etapei se numesc `E3-rN`;
candidații de acum: repartizarea profitului ca document generat (E3-D2),
perioada de ajustare (E3-D6), exercițiul diferit de anul calendaristic.

## Pași

| Pas | Conținut | Probă | Stare |
|---|---|---|---|
| 0 | contractul, scopul E3, rândurile din catalog, textele „1174” corectate; owner pe E3-D7 (data bugetară) și E3-D9 (rândurile) | docskit verde | în lucru |
| 1 | `PoliticaInchidereConturi` cu seed-ul; `InchidereRezultat` cu generare, previzualizare, regenerare; `REZ-NEINCHIS`; migrația | familia de scenarii `EX`, definită în acest pas; ModelCheck pe ambele profiluri; generare fără linii pe o lună închisă de 1C | — |
| 2 | exercițiul pe decembrie: comenzile, constatările, istoricul; rândurile `Anual`; `CV_AN_INCHIS` pe exercițiu | familia `EX`; SC-X-12; teste de nucleu; ModelCheck pe ambele profiluri | — |
| 3 | balanța pe an (E3-D8): proiecția, DTO-ul, openapi, coloana în React | SC-CIT pe doi ani; drift openapi; SAF-T neschimbat pe lunile de probă | — |
| 4 | XAF: comenzile pe perioadă și exercițiu, lista închiderilor; ușile HTTP cu refuzurile (80) | `refuzuri.ps1` pe host viu; browser | — |
| 5 | regresia integrală, review advers, decizia, închiderea | integrala pe ambele profiluri; nucleu; HTTP; docskit | — |

## Regula de oprire

Lucrul se oprește și revine la owner când:

1. o regulă contabilă cerută de scenariu nu se poate exprima ca rând de
   politică și ar cere un simbol de cont sau o ramură pe profil în cod;
2. plierea coordonatelor pe destinație (E3-D3) intră în conflict cu un
   cititor existent (D300, D394, SAF-T, partide);
3. rândurile execuției bugetare (E3-D9) nu sunt confirmate la începutul
   pasului 2;
4. granița de an din balanță (E3-D8) schimbă o cifră deja probată în afara
   coloanelor `Initial` (soldurile, SAF-T, D300);
5. un pas consumă de două ori estimarea lui fără probă verde.
