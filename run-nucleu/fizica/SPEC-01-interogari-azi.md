# Explorare 01: interogările de azi ale rapoartelor, ca SQL rulabil (pasul 2 „fizica" al `docs/nucleu/nucleu-cub-design.md`)

Scop: pentru fiecare din cele 14 interogări de mai jos, SQL-ul pe care îl
emite (sau pe care îl echivalează exact) implementarea curentă, rulabil în
psql pe baza Postgres `Atlas.Conta.Import1C.Flax` (docker
`contapal-postgres-1`: `docker exec contapal-postgres-1 psql -U postgres -d
Atlas.Conta.Import1C.Flax`). Ele devin linia de bază (forma F0) a măsurătorilor
de fizică. Nu propui nimic, nu judeci; inventariezi și verifici că rulează.

Citește întâi (scurt): `docs/nucleu/nucleu-coordonate-rapoarte.md` §3
(tabelul „Reconstruibilitatea, raport cu raport" — de acolo vine lista) și
`docs/api/p5-perf-masuratori.md` §Metodă + addendumurile (au SQL deja capturat
pentru fișă, `DocumenteCuRest`, `RegistruTva`, SAF-T S — refolosește-l).
Inventarele `run-nucleu/coordonate/0{1..4}-*.md` au `fișier:linie` pentru
fiecare proiecție; pornește de la ele, nu re-explora.

## Interogările (parametrii se aleg pe Flax ca „cazul greu": contul/partenerul/gestiunea cu cele mai multe rânduri; luna = 2025-06 și 2025-12)

| # | Raport | Forma cerută |
|---|---|---|
| Q01 | Balanță plată sintetică pe 2025 (sold inițial + rulaje + sold final, per cont) | ce face `ContabilProiectii.Balanta` (cu sau fără `SolduriPerioadaContabil`, spune ce folosește azi și DE CE — 12/2025 e închisă pe Flax) |
| Q02 | Balanță analitică cont × partener (4111 și 401), 2025 | idem, grup pe repartitor |
| Q03 | Fișa contului 4111 pe 2025, cu sold cumulativ | `ContabilProiectii.FisaCont` (fereastra) |
| Q04 | Fișa 4111 × un partener (cel mai activ), 2025 | idem filtrat |
| Q05 | Sold cont × partener la 2025-12-31 (toți partenerii pe 4111) | `SoldContRepartitor` sau echivalentul |
| Q06 | Documente cu rest la 2025-12-31 (partide deschise) | forma REALĂ de azi (`PartideDeschise` + `Imperecheri`; felia 27 în perf-masuratori) |
| Q07 | Jurnal cumpărări 2025-06 (per document × cotă) | `TvaProiectii` |
| Q08 | D394 2025-06 (partener × cotă × op, cu NrFact) | `D394Proiectii` |
| Q09 | Sold pe loturi într-o gestiune (cea mai activă) la 2025-06-30 (intrarea FIFO: loturile cu cantitate > 0, ordonate) | `StocProiectii` / motorul FIFO |
| Q10 | PhysicalStock la 2025-12-31 (toate loturile cu Σcant > 0, cu valoare) | `SaftProiectii` S / `SaftStocuri` |
| Q11 | SAF-T Customers/Suppliers (sold inițial + rulaje + final pe partener × cont pe 2025-06) | `SaftProiectii` terți |
| Q12 | GLE pentru 2025-06: toate rândurile contabile ale lunii cu documentul lor (număr, tip) | `SaftProiectii` GLE |
| Q13 | Citirea unui document pentru storno: toate rândurile din cele 3 registre ale unui document cu multe linii (cel cu cele mai multe rânduri contabile) | `MotorOperare` storno |
| Q14 | Scrierea: INSERT-urile pe care le face operarea unui document cu ~50 de linii (forma rândurilor pe cele 3 registre) — scrii un INSERT … SELECT echivalent dintr-un document existent, cu ID-uri noi, într-o TRANZACȚIE cu ROLLBACK | `MotorOperare` materializare |

Pentru fiecare:
1. `fișier:linie` al proiecției/metodei pe care o oglindește;
2. filtrele exacte (`GCRecord`, `Stare`, `Data` vs `DataInregistrare` vs
   `PerioadaAn/Luna`, `Storno`) — copiate din cod, nu presupuse;
3. SQL-ul rulabil, salvat în `run-nucleu/fizica/f0/qNN.sql` (identificatori
   cu ghilimele, fără parametri: valorile alese scrise literal, cu un
   comentariu `-- ales: …` care spune cum le-ai ales);
4. rulat O DATĂ cu `EXPLAIN (ANALYZE, BUFFERS)` — notezi doar timpul total și
   numărul de rânduri returnate (nu planul), în tabelul din raport.

Preferință: SQL-ul REAL emis de EF acolo unde e deja capturat în
`p5-perf-masuratori.md` sau `run-f28/pas3/*.sql`/`explain-*.txt`; altfel
oglindire de mână a LINQ-ului, cu nota „oglindit” în tabel.

Reguli:
- Citește codul, nu ghici; fiecare filtru are `fișier:linie`.
- NU modifici baza (doar SELECT/EXPLAIN; Q14 în tranzacție cu ROLLBACK).
- NU atingi cod de producție și nu comiți.
- Regulă de oprire: dacă o proiecție nu are echivalent SQL (calculează în
  memorie peste entități), scrii SQL-ul părții care se poate și notezi
  explicit ce rămâne în memorie; nu inventezi o formă „mai bună”.
- Rezultatul: `run-nucleu/fizica/01-interogari-azi.md` (română, cod slim) +
  fișierele `f0/qNN.sql`. Răspunsul final către coordonator: max 15 linii —
  ce ai acoperit, ce n-a avut echivalent SQL, cele 3 surprize (filtre
  neașteptate, calcule în memorie, indexi nefolosiți).
