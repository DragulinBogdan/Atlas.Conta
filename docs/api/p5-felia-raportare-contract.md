# Pasul 5, felia 9 — raportarea pe registre (balanță, fișă de cont, registru-jurnal)

Prima felie a pasului 5 care nu adaugă un tip de document, ci **suprafața de
citire** care lipsea. Până acum clientul are 11 felii de scriere și o singură
proiecție (`SoldStoc`) — un operator poate produce registre, dar nu le poate
*citi*: nici balanță, nici fișă de cont, nici listare cronologică. Arhitectura
declară de la decizia 35d că „raportarea trăiește pe registre", cusătura de
citire (42c) e probată de la spike, iar `RegistruContabil` e plat din DIM-3;
felia asta încasează toate trei.

Valoarea colaterală, la fel de importantă: balanța devine **suprafața cu care
verifici tot ce s-a construit** — fiecare felie de scriere de până acum se
confirmă sau se infirmă acolo.

## Scope

**Intră:** trei proiecții peste `RegistruContabil` + endpoint-urile lor +
trei ecrane de client.

**Nu intră, cu motiv:** jurnalele de TVA (achiziții/livrări) sunt per
**document** prin natură — baza impozabilă, cota și partenerul stau pe
`DocumentDetaliu`, nu în registru — și se leagă direct de D300/D394/SAF-T,
amânate explicit la 36f. Sunt felie proprie, peste altă sursă.

**Fără migrație, fără atingerea motorului.** Felia e integral aditivă și
read-only; dacă un pas cere o schimbare de model, e semn că s-a greșit ceva.

---

## Deciziile feliei

### R-D1 — Atomul de raportare: unpivot pe laturi, o singură definiție

`RegistruContabil` e o tabelă de **perechi** (cont debitor, cont creditor,
valoare). Orice raport pe cont are deci nevoie de forma unpivotată: fiecare rând
produce **doi atomi** — `(ContDebit, +valoare pe debit)` și
`(ContCredit, +valoare pe credit)` — prin `Concat`. Precedentul exact:
`DocumenteCuRest` (57c), care unpivotează laturile trezoreriei.

Atomul poartă și **dimensiunile laturii lui** (cele 8 FK-uri `Debit*` pentru
atomul de debit, `Credit*` pentru cel de credit) — asta e ce face analiticul
posibil fără nicio coloană nouă.

**Un singur atom, partajat de toate cele trei proiecții.** Un al doilea exemplar
ar diverge tăcut de primul — aceeași regulă care a urcat `EtichetaLot` și
`Copii` în `ApiProiectii`.

### R-D2 — Perioada și dimensiunile sunt PARAMETRI de proiecție, nu filtre de grilă

Distincția e load-bearing, nu stilistică:

- **`dataStart` determină ce ÎNSEAMNĂ soldul inițial.** Nu e un filtru peste
  rezultat — e o graniță în interiorul agregării. Un filtru `DataSourceLoader`
  peste rândurile de ieșire n-are cum s-o exprime.
- **Filtrele pe dimensiuni trebuie aplicate ÎNAINTE de agregare.** „Balanța pe
  proiectul X" înseamnă „însumează doar mișcările proiectului X", nu „arată
  rândurile a căror coloană Proiect e X" — coloana aia nici nu există după un
  `GROUP BY` pe cont.

Deci: perioada + cele 8 dimensiuni + modul analitic intră ca **query params**;
`DataSourceLoader` rămâne deasupra pentru ce e legitim al lui — sortare,
paginare, filtrare pe coloanele de ieșire (simbol, denumire), grupare.

### R-D3 — Balanța: o singură agregare cu sume condiționate

Forma naivă (două agregări + join) cere un FULL OUTER JOIN — inexprimabil în
LINQ — și ar pierde exact conturile cu sold inițial dar fără mișcare în
perioadă. Forma corectă e **un singur `GROUP BY`** peste atomii cu
`Data <= dataEnd`, cu patru sume condiționate:

```
InitialDebit  = Σ (Data <  dataStart ? Debit  : 0)
InitialCredit = Σ (Data <  dataStart ? Credit : 0)
RulajDebit    = Σ (Data >= dataStart ? Debit  : 0)
RulajCredit   = Σ (Data >= dataStart ? Credit : 0)
```

EF le traduce în `SUM(CASE WHEN …)` — o singură trecere peste tabelă. Contul cu
sold inițial și zero mișcare apare cu rulaje 0, cum trebuie.

### R-D4 — Netarea soldurilor se face la nivelul cheii de grupare; două moduri explicite

Soldul unui cont se prezintă **netat**: `net = (iniD − iniC) + (rulD − rulC)`;
`net > 0` ⇒ sold final debitor, altfel creditor. Idem pentru soldul inițial.

Capcana: **netarea nu e aditivă.** Contul 401 cu furnizorul A pe sold debitor
100 și furnizorul B pe sold creditor 200 dă, analitic, `D 100 / C 200`, dar
sintetic `C 100`. Ambele cifre sunt corecte — la niveluri diferite. Deci modul
nu se poate deduce, se **cere explicit**: parametrul `analitic`.

- `analitic=false` (implicit): cheia = `Cont`.
- `analitic=true`: cheia = `Cont × Repartitor` — exact „analiticele se derivă din
  dimensiuni la raportare" (decizia 10), fără niciun analitic persistat în plan.

Celelalte 7 dimensiuni rămân **filtre** în felia asta; promovarea oricăreia la
cheie de grupare e aditivă (același parametru, altă cheie) și se face la cerință
reală, nu preventiv.

### R-D5 — Ierarhia de conturi nu se rulează în sus (și de ce grila nu minte)

`Cont.Parinte`/`Sumator` există, dar felia listează **doar conturile de mișcare**.
Motivul e R-D4: pe un grup de grilă (clasă, grupă), **rulajele se pot însuma, dar
soldurile NU** — un total de sold pe grup ar fi o cifră falsă afișată cu aceeași
autoritate ca restul.

Consecință impusă în client, nu doar afirmată: grupurile de grilă poartă sume
**exclusiv pe coloanele de rulaj**. Rollup-ul real pe plan (cu netare corectă la
fiecare nivel) e felie proprie dacă apare cerința.

### R-D6 — Fișa de cont: SQL brut cu window function, ordine FIXĂ

Soldul curent per rând e prin definiție `sold_inițial + Σ(rândurile de dinainte)`
— o funcție de fereastră. Cele trei alternative și de ce cad:

- **cumul în client** — încalcă direct „nimic nu se calculează în TS" (42c) și
  minte la orice paginare;
- **agregat corelat per rând** (`Σ` peste rândurile `<=` curentul) — O(n²), pe
  305k rânduri nu e o opțiune;
- **fără sold curent** — pierde exact ce se caută într-o fișă.

Deci: `SUM(...) OVER (ORDER BY data, id)` în SQL parametrizat, expus ca
`IQueryable` prin `DbContext.Database.SqlQuery<T>` (EF Core 8+ compune
`Where`/`Skip`/`Take` peste el), deci `DataSourceLoader` rămâne deasupra pentru
paginare.

Două consecințe **obligatorii**, altfel proiecția minte:

1. **Ordinea e fixă cronologic** — sortarea din grilă se dezactivează pe ecranul
   de fișă. Un sold curent reordonat după alt criteriu e o coloană de cifre fără
   sens, afișată ca și cum ar avea.
2. **Parametrizare strictă** (fără interpolare de string în SQL).

Notă de cusătură, de scris în cod: calea SQL brut **ocolește filtrele de
securitate XAF**. E acceptabilă aici pentru că `RegistruContabil` n-are
restricții pe rând (nimeni n-are Write pe registre — 42a, iar citirea nu e
filtrată); dacă asta se schimbă vreodată, fișa e primul loc care trebuie
reevaluat.

### R-D7 — Rândurile de storno intră; rândurile de deschidere se afișează ca atare

Registrul e append-only și soldul E suma lui algebrică — rândurile inverse se
anulează singure (aceeași regulă, cu același comentariu, ca la `SoldStoc`).
Nicio proiecție nu filtrează `Storno`.

Rândurile cu `DocumentId == null` sunt soldurile de deschidere scrise de migrare
(25e/34d): apar normal, cu documentul gol — nu se ascund și nu se marchează
special.

### R-D8 — Codul de tip al documentului: `CoduriTip`, pe pagină

Fișa și jurnalul arată documentul-sursă cu link. Sub TPT nu există discriminator,
deci codul de tip nu vine din SQL — se rezolvă în memorie prin
`ApiProiectii.CoduriTip` (60b: un singur query polimorf pe mulțime, nu
`GetObjectByKey` în buclă), **peste pagina deja materializată**. Clientul
rutează prin `rutaTip` (61a) — tip fără felie rămâne text, nu link mort.

### R-D9 — Registrul-jurnal: rândurile BRUTE, nu atomii

Jurnalul e listarea cronologică a notelor așa cum au fost scrise: `Data`,
`NumarNota`, cont debitor, cont creditor, valoare, document. Unpivotat ar dubla
fiecare notă — atomul e pentru agregare pe cont, nu pentru listare.

### R-D10 — Client: grup „Rapoarte", URL = starea, drill-down balanță → fișă

Trei ecrane noi, `remoteOperations` complete ca la `SoldStoc`. Perioada și modul
trăiesc în query string (43c: URL-ul E starea globală — deep-link și refresh
gratis); un rând de balanță duce la fișa contului **cu perioada păstrată**.

---

## Contractul de ieșire

```
GET /api/proiectii/balanta
    ?dataStart&dataEnd&analitic&repartitorId&materialId&codFunctionalId
    &codEconomicId&sursaFinantareId&unitateId&proiectId&centruCostId
 → { ContId, ContSimbol, ContDenumire, RepartitorId?, RepartitorDenumire?,
     InitialDebit, InitialCredit, SoldInitialDebit, SoldInitialCredit,
     RulajDebit, RulajCredit, SoldFinalDebit, SoldFinalCredit }

GET /api/proiectii/fisa-cont?contId&dataStart&dataEnd&<dimensiuni>
 → { Id, Data, NumarNota, Sens ("D"|"C"), Debit, Credit, SoldCurent,
     ContrapartidaId, ContrapartidaSimbol, RepartitorDenumire?,
     DocumentId?, DocumentTip?, DocumentNumar?, Storno }

GET /api/proiectii/registru-jurnal?dataStart&dataEnd
 → { Id, Data, NumarNota, ContDebitSimbol, ContCreditSimbol, Valoare,
     DocumentId?, DocumentTip?, DocumentNumar?, Storno }
```

Proiecțiile în `Module/Proiectii/ContabilProiectii.cs` (`IQueryable` pur,
testabile în ModelCheck); controllerele subțiri în `WebApi/API/Conta/`, pe
`Secured(typeof(RegistruContabil))` + `Incarca`.

## Verificări (ModelCheck, ambele profiluri)

Proiecția e un al doilea adevăr dacă nu e legată de primul — precedentul D9
(`SoldStoc` == `StocService.Sold`). Aici legăturile sunt:

1. **Partidă dublă**: `Σ RulajDebit == Σ RulajCredit` peste toată balanța, pe
   orice interval.
2. **Balanța == recomputare naivă** în memorie, pe un eșantion de conturi.
3. **Analitic ⇒ sintetic**: rulajele balanței analitice, însumate per cont, ==
   rulajele celei sintetice (rulajele SUNT aditive — R-D4). Soldurile,
   deliberat, NU se compară așa.
4. **Continuitate**: `SoldInițial(perioada N+1) == SoldFinal(perioada N)`.
5. **Cusătura fișă ↔ balanță**: ultimul `SoldCurent` din fișa unui cont pe o
   perioadă == `SoldFinal` din balanță, pe același cont și aceeași perioadă.
   Verificarea cea mai valoroasă a feliei: leagă calea SQL brut de cea LINQ.
6. **Jurnal**: `Σ Valoare` == suma directă din registru pe interval.

**Perf**: măsurat pe clona de import (~305k rânduri ⇒ ~610k atomi), cu metoda
și pragurile din `p5-perf-masuratori.md` (59). Balanța pe un an și fișa unui
cont cu trafic mare sunt cifrele care contează.

## Riscurile pin-uite (ce țintește review-ul advers)

1. **Window function × `DataSourceLoader`**: ce se întâmplă la sortare impusă din
   grilă, la filtrare compusă, la paginare adâncă — soldul curent rămâne coerent
   sau devine decor?
2. **Netarea la nivelul greșit**: analitic vs sintetic pe un cont cu solduri de
   ambele sensuri (401 cu furnizor pe debit); totaluri de grup pe coloane de sold.
3. **Granițele de dată**: `< dataStart` vs `<= dataEnd`; ziua exactă a
   dataStart; o perioadă de o singură zi; rânduri de storno căzând de partea
   cealaltă a graniței față de rândurile pe care le storneaza.
4. **Contul cu sold inițial și zero mișcare** — apare în balanță? (trebuie).
   Contul cu mișcare care se netează la zero — apare?
5. **Securitate**: SQL brut vs filtrele XAF; parametrizarea (injection).
6. **Perf**: 610k atomi, `GROUP BY` cu patru `CASE`; balanța analitică (cheie
   dublă); fișa pe contul cel mai traficat.
7. **Dimensiuni ca filtru**: filtrul se aplică pre-agregare pe latura CORECTĂ?
   (atomul de debit filtrat pe dimensiunea de debit, nu pe cea de credit).
8. **Rândurile de deschidere** (`DocumentId == null`) — nu pică nicăieri pe o
   navigație presupusă nenulă.

## Regula de oprire

Felia e închisă când: cele 6 verificări ModelCheck trec pe ambele profiluri,
cifrele de perf sunt măsurate și scrise, iar în browser, pe clona bazei de
import, se poate face drumul complet — **balanță pe o lună → drill-down pe un
cont → fișa lui cu sold curent care se închide exact pe soldul final din
balanță → link către documentul unei linii**.

Explicit NU în regula de oprire: export (PDF/Excel), formatare de raport
tipărit, rollup pe planul de conturi, jurnale de TVA.
