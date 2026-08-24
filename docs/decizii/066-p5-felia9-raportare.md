# Decizia 66 — Pasul 5, felia 9 — raportarea pe registre

- **Data**: 2026-08-17 (primul commit în jurnal)
- **Stare**: activă
- **Rezumat durabil**: `CLAUDE.md` §66
- **Docs**: docs/api/p5-felia-raportare-contract.md

---

**Pasul 5, felia 9 — raportarea pe registre (balanță, fișă de cont,
registru-jurnal) — executată** (contract + închidere:
`docs/api/p5-felia-raportare-contract.md`, R-D1…R-D10; smoke browser de
main pe clona de import, review advers cu 4 defecte de fond, toate fixate).
Prima felie a pasului 5 care nu adaugă un tip de document, ci **suprafața de
citire** care lipsea: clientul avea 11 felii de scriere și o singură
proiecție (`SoldStoc`) — registrele se puteau produce, dar nu se puteau
*citi*. Zero migrații, motorul neatins. Tranșările:
(a) **Atomul de raportare = unpivot pe laturi, o singură definiție**:
`RegistruContabil` e o tabelă de PERECHI, deci orice raport pe cont cere
forma unpivotată — un rând dă doi atomi, prin `Concat` (tradus în `UNION
ALL`), fiecare purtând **dimensiunile laturii lui**, ceea ce face analiticul
posibil fără nicio coloană nouă. Precedentul: `DocumenteCuRest` (57c).
(b) **Perioada și dimensiunile sunt PARAMETRI de proiecție, nu filtre de
grilă**: `dataStart` nu filtrează rezultatul, ci decide ce ÎNSEAMNĂ soldul
inițial (graniță în interiorul agregării), iar filtrele de dimensiune se
aplică pe atomi, înaintea lui `GROUP BY`, cât timp coloana încă există —
fiecare pe LATURA lui. `DataSourceLoader` rămâne deasupra doar pentru ce e
legitim al lui.
(c) **Balanța = o singură agregare cu patru sume condiționate**
(`SUM(CASE WHEN …)`), nu două plus join: forma naivă ar fi cerut FULL OUTER
JOIN — inexprimabil în LINQ — și ar fi pierdut exact conturile cu sold
inițial și zero mișcare. `IgnoreAutoIncludes()` explicit contra celor 16
navigații `AutoInclude` ale registrului (41c), verificat pe SQL-ul generat.
(d) **Netarea nu e aditivă ⇒ modul se CERE, nu se deduce**: 401 cu un
furnizor pe debit 100 și altul pe credit 200 dă analitic `D100/C200`,
sintetic `C100` — ambele corecte, la niveluri diferite. Corolar impus în
client: sumarele de grup stau **exclusiv pe rulaje**, niciodată pe `Sold*`;
de aceea felia nu oferă rollup pe planul de conturi.
(e) **Fișa = primul SQL brut din repo** (`Database.SqlQuery<T>` pe
`EFCoreObjectSpace.DbContext`): soldul curent per rând e o funcție de
fereastră, iar alternativele cad (cumul în client încalcă 42c și minte la
paginare; agregat corelat e O(n²)). Fereastra se calculează peste tot ce e
`<= dataEnd`, filtrul `>= dataStart` se aplică în exterior — soldul include
inițialul fără o a doua interogare. Soft delete-ul NU e automat pe calea
brută: `GCRecord = 0` pus explicit pe patru tabele, probat comportamental.
**Două tipuri, nu unul**: `SqlQuery<T>` înregistrează T ca entitate ad-hoc,
iar sub `UseChangeTrackingProxies` (24) DTO-ul sigilat aruncă, iar cel
unsealed pleacă pe sârmă ca PROXY, cu membri XAF absenți din schema OpenAPI.
(f) **Ordinea fișei nu era optimizată de EF — era ȘTEARSĂ de
`DataSourceLoader`** (găsit de măsurătoarea de perf, în afara mandatului ei):
biblioteca își inventează o sortare când cererea n-are `sort=`
(`FindSortableMember` → primul membru numit „Id"), o compilează ca
`Queryable.OrderBy`, iar `SelectExpression.ApplyOrdering` face
`_orderings.Clear()`. `SoldCurent` rămânea corect, dar rândurile ieșeau în
altă ordine decât cea în care fusese cumulat. Fix: ordinea se declară în
forma pe care biblioteca o CONSUMĂ, cu seam-ul în Module ca testul să
exercite codul de producție. Testul de regresie a fost **văzut picând**.
(g) **Review advers — 4 defecte, toate în afara drumului fericit prin
construcție**: fișa **ocolea securitatea** (un utilizator fără nicio
permisiune primea registrul complet, iar `ContrapartidaId` pe fiecare rând
deschidea toată cartea mare) — închis prin gate fail-closed care **măsoară**
echivalența celor două căi per cerere, în loc s-o afirme; balanța analitică
pagina pe cheie ne-unică; drill-down-ul pe rândul „fără repartitor" deschidea
fișa nefiltrată (`Guid?` nu poate exprima „absent" — null e deja „fără
filtru"); `Cont` invizibil făcea balanța să piardă tăcut atomi prin INNER
JOIN, rupând partida dublă. **Ordinea fixurilor a contat**: fără gate, LEFT
JOIN-ul ar fi arătat sume pe conturi fără nume unui utilizator fără drept pe
plan — un fix care deschide o scurgere închizând alta.
(h) **Tiparul comun al tuturor defectelor feliei**: un check exista, dar nu
pe CALEA REALĂ — proiecțiile erau verificate ca `IQueryable`, niciodată prin
`DataSourceLoader`; cusătura fișă↔balanță doar pe sintetic; partida dublă
doar pe o bază cu toate conturile vizibile; securitatea deloc (ModelCheck
rulează pe provider nesecurizat, deci un check ar fi trecut și cu gate-ul
șters — proba ei e HTTP, iar motivul e scris în cod).
(i) **Perf**: tot sub 300ms pe clona de import — balanță 59/88ms
(lună/an), analitică 84/269ms, jurnal 63/77ms, fișa contului celui mai
traficat 214–286ms, cu cost **constant indiferent de `skip`** (dominanta e
`WindowAgg`, structural). Niciun index adăugat (disciplina 59).
(j) Rămase, ne-blocante: `HeaderFilter` trunchiat la 100 de valori distincte
(plafonul din `Incarca` taie grupurile); `DocumentTip` divergent după
permisiuni (pre-existent, benign prin 61a); cele 8 dimensiuni fără UI, doar
pass-through din URL.
