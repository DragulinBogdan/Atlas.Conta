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

> **Închis** (2026-08-17, felia 10): rollup-ul există, ca ecran separat —
> `docs/api/p5-felia-balanta-plan-contract.md`. Decizia de aici rămâne
> neschimbată: pe balanța PLATĂ totalurile stau în continuare doar pe rulaje;
> cifrele de nivel superior se citesc pe celălalt ecran, unde sunt rânduri
> calculate pe server (brutele cumulate, netarea refăcută la fiecare nod), nu
> totaluri de grilă.

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
securitate XAF**.

**Corectat după review-ul advers (D1).** Prima formulare a acestei note spunea
că e acceptabilă „pentru că `RegistruContabil` n-are restricții pe rând (…iar
citirea nu e filtrată)" — o PREMISĂ, și una falsă: probat cu token, un
utilizator fără nicio permisiune primea gol de la balanță și de la
`/api/odata/Cont`, dar registrul COMPLET de la fișă — cu `ContrapartidaId` pe
fiecare rând, adică toată cartea mare, plimbându-te din contrapartidă în
contrapartidă.

Premisa nu se re-afirmă, se **dovedește**, per cerere, **fail closed**:

1. `contId` se rezolvă prin ObjectSpace-ul **securizat** — inexistent SAU
   invizibil ⇒ **404**, fără sondare de existență (tiparul
   `ComandaAutorizata`);
2. echivalența celor două căi se **măsoară** (`ContabilProiectii.CaleaBrutaEchivalenta`):
   numărul rândurilor de registru ale contului văzute prin
   `GetObjectsQuery<RegistruContabil>()` vs. cele văzute de SQL-ul brut pe
   aceeași mulțime. Diferă ⇒ **403**, cu mesaj care trimite la balanță/jurnal.
   Numărătoarea e o dovadă, nu o euristică: interogarea securizată e
   `predicatul ∧ criteriile de securitate`, deci poate doar scoate rânduri —
   egalitate ⟺ n-a scos niciunul.

Zona nu e acoperită de ModelCheck și nici nu poate fi (unealta rulează pe un
provider standalone, NEsecurizat — acolo cele două căi sunt egale prin
construcție, deci un check ar trece și cu gate-ul șters). Proba ei e HTTP, pe
utilizatori cu drepturi diferite.

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

---

## Închidere (2026-08-17)

- [x] **Contract îndeplinit.** ModelCheck final **bugetar 649 OK / 0 FAIL**,
  **privat 302 OK / 0 FAIL** (re-rulate de main la fiecare pas, nu doar
  raportate): +16 la balanță, +20 la fișă/jurnal, +5 la ordinea fișei, +5 la
  fixurile review-ului. Soluția întreagă (inclusiv Blazor.Server) și clientul
  compilează; `pnpm verifica:drift` verde după commit. **Zero migrații** — felia
  a rămas integral aditivă și read-only, cum cerea scope-ul.
- [x] **Regula de oprire, probată în browser de main** pe clona bazei de import:
  balanța ian. 2025 (95 conturi, `Σ 49.574.505,71` identic pe ambele rulaje —
  partida dublă vizibilă, coloanele de sold fără totaluri) → drill-down pe 3028
  → fișa cu sold curent care pornește exact din `Sold inițial debitor` (2.024,59)
  și se închide, pe pagina 3, pe `2.342,70` = `Sold final debitor` din balanță →
  link către documentul liniei. Jurnalul: cronologic, storno afișat ca valoare
  negativă pe corespondența originală (46a).
- [x] **Perf măsurată** (addendum în `p5-perf-masuratori.md`): tot sub 300ms —
  balanță sintetică 59/88ms (lună/an), analitică 84/269ms, jurnal 63/77ms, fișa
  contului celui mai traficat 214–286ms, **inclusiv calea de grupare
  server-side** (57–202ms), pe care n-o măsurase nimeni. `DocumenteCuRest`
  re-măsurat ca reper (423ms vs ~410ms în 59) ⇒ cifrele rămân comparabile între
  felii. `EXPLAIN`: fișa folosește deja `Bitmap Index Scan` pe indecșii de cont,
  costul dominant e `WindowAgg` — structural (R-D6 cere istoricul integral) și
  **constant indiferent de `skip`**. Niciun index adăugat (disciplina 59).

### Ce a scos măsurătoarea, în afara mandatului ei

**Ordinea fișei nu era optimizată de EF — era ȘTEARSĂ de `DataSourceLoader`.**
SQL-ul real conținea `ORDER BY a."Id"`, nu cele trei chei din cod (26 din
144.248 rânduri diferite ca poziție pe contul 4111). `SoldCurent` rămânea corect
(fereastra e în SQL-ul nostru), dar rândurile puteau fi AFIȘATE în altă ordine
decât cea în care soldul fusese cumulat — adică exact coloana de cifre fără sens
pe care R-D6 există s-o prevină.

Lanțul, demonstrat pe surse decompilate: `Incarca` pune mereu `Take` ⇒
`HasPaging`; DTO-ul n-are `[Key]` ⇒ `PrimaryKey` gol; în ramura asta biblioteca
își **inventează** o sortare (`EFSorting.FindSortableMember` → primul membru
numit „Id"), o compilează ca `Queryable.OrderBy` (nu `ThenBy`), iar EF Core
`SelectExpression.ApplyOrdering` face `_orderings.Clear()`.

Sub-întrebarea gravă — dacă `Id`, care NU e unic pe fișă, e tratat ca cheie și
poate deduplica rânduri — are răspuns **negativ**, verificat: `AdHocMapper`
scoate convențiile de cheie și face `HasNoKey()`.

Testul de regresie a fost **văzut picând** (4 verificări FAIL cu fixul dat
înapoi). Scena are Id-uri EXPLICITE în ordine inversă față de dată — pe UUIDv7
naturale ar fi fost flaky-verde. Invariantul verificat e cel real: `SoldCurent`
al fiecărui rând == cumulul rândurilor **de dinaintea lui în ordinea afișată**,
continuat peste paginare — nu „ultimul sold e corect", care iese bun și dintr-o
secvență amestecată.

### Review advers — 4 defecte de fond, toate închise

Toate în afara drumului fericit **prin construcție**: niciunul nu se manifestă pe
balanța sintetică a unei luni, cu Admin, pe un plan intact.

1. **Fișa ocolea securitatea** (scurgere completă). `User` primea gol de la
   balanță și de la OData, dar registrul COMPLET de la fișă; cu
   `ContrapartidaId` pe fiecare rând, toată cartea mare. Închis prin gate
   fail-closed — vezi R-D6, unde premisa a fost rescrisă.
2. **Balanța analitică pagina pe cheie ne-unică** — același mecanism al
   bibliotecii ca la ordinea fișei, în celălalt loc. Fix: ordine totală, și
   ca **tiebreak când clientul sortează**, nu doar ca default (un click pe
   antetul „Cont" reproducea cheia ne-unică pe calea reală).
3. **Drill-down pe rândul analitic „fără repartitor"** deschidea fișa
   NEFILTRATĂ, cu soldul SINTETIC afișat ca al rândului. Cauza era în contract,
   nu în client: `Guid?` nu poate exprima „repartitor absent" — null înseamnă
   deja „fără filtru". Fix: a treia valoare (`repartitorNul`).
4. **`Cont` invizibil** (șters logic sau nevăzut prin securitate) ⇒ balanța
   pierdea tăcut atomi prin `INNER JOIN` și partida dublă se rupea în footer,
   în timp ce fișa aceluiași cont mergea. Fix: `LEFT JOIN`, cu contul nerezolvat
   marcat. **Ordinea a contat**: fără gate-ul de la (1), LEFT JOIN-ul ar fi
   arătat sume pe conturi fără nume unui utilizator fără drept pe plan — un fix
   care deschide o scurgere închizând alta.

Tiparul comun al lui (1)–(4): fiecare a fost posibil pentru că **un check
exista, dar nu pe calea reală** — proiecțiile erau verificate ca `IQueryable`,
niciodată prin `DataSourceLoader`; cusătura fișă↔balanță era verificată doar pe
sintetic; partida dublă doar pe o bază cu toate conturile vizibile; securitatea
deloc (ModelCheck rulează pe provider nesecurizat, deci un check ar fi trecut și
cu gate-ul șters — motivul e scris în cod, proba e HTTP).

### Rămase, ne-blocante

- **`HeaderFilter` trunchiat la 100 de valori distincte** (măsurat: 100 din 105
  pe „Cont debitor" în jurnal): plafonul din `Incarca` taie GRUPURILE, nu
  rândurile. Peste 500 de valori rămâne trunchiat indiferent de scroll.
- **`DocumentTip` divergează după permisiuni** (pre-existent): un utilizator
  care nu vede `TipDocument` primește numele clasei CLR în loc de cod. Benign
  prin 61a (`rutaTip` nu recunoaște ⇒ text, nu link mort), dar e o divergență de
  contract funcție de cine cere.
- **Cele 8 dimensiuni n-au UI** — se acceptă doar din URL (pass-through), fără
  culegere. Scop declarat; promovarea oricăreia la cheie de grupare rămâne
  aditivă (R-D4).
- Restul listelor de citire (`Lista` per felie, `SoldStoc`, `DocumenteCuRest`)
  primesc în continuare ordinea inventată de bibliotecă. Azi n-are ce rupe —
  niciuna n-are ordine documentată — dar oricare capătă una trebuie s-o declare
  la fel, altfel dispare identic.
- Contul care se netează exact la zero apare cu toate cifrele 0 (corect
  matematic, zgomot într-o balanță tipărită).
