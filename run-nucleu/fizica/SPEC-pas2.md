# Pas 2 (fizica): interogările pe cub, indexii per uz, snapshot-ul, măsurătorile la ×1 și ×10, egalitatea

Contractul: `run-nucleu/fizica/CONTRACT.md` (FZ-D5…D9 sunt decizii). Intrări:
- bazele `Atlas.Conta.Nucleu.Fizica.x1` și `.x10` (pasul 1): schema `public`
  = F0 (registrele de azi), `f1`/`f2`/`f3`."Postare" = cele trei forme ale
  cubului, `cub."Tranzactie"`, `cub."CodTva"`; raportul pasului 1 în
  `run-nucleu/fizica/pas1/out/`;
- `run-nucleu/fizica/01-interogari-azi.md` + `f0/qNN.sql` (SQL-ul de azi,
  parametrii „cazul greu" — ID-urile de cont/partener/gestiune/document —
  se PREIAU de acolo, nu se realeg); `f0/explain-qNN.txt`;
- `docs/nucleu/nucleu-coordonate-rapoarte.md` §3 (forma pe cub a fiecărui
  raport) și `02-semantica-mapare.md` (ce e Partener/Gestiune/Unitate în cub).

Mediu: Postgres 18.4 în docker `contapal-postgres-1`
(`docker exec contapal-postgres-1 psql -U postgres -d <baza> ...`; scripturile
prin `docker cp` + `-f`). Ieșirile în `run-nucleu/fizica/pas2/`. Nimic nu se
atinge în Flax, în `nou/`, în `public` (F0) — F0 se citește, nu se indexează.

## A. Interogările pe cub (`pas2/cub/qNN.sql`, cu `{T}` în locul tabelei)

Un singur text per interogare; `{T}` se înlocuiește cu `f1."Postare"`,
`f2."Postare"`, `f3."Postare"`. Forma = sume pe coordonate (pasul 1 §3);
din document se citesc DOAR atribute prin `Cauza` (număr, tip), niciodată
cifre. Granul de ieșire = cel al lui `f0/qNN.sql`, ca rezultatele să fie
comparabile. Concret:

| # | Pe cub |
|---|---|
| Q01 | `Spatiu=1`; per `Cont`: Σ Valoare pe `Latura` pentru `Data < 2025-01-01` (SI) și `2025` (rulaje); același gran ca F0 |
| Q02 | idem, per (`Cont` ∈ {4111, 401}, `Partener`). Semantica diferă de F0 (acolo „repartitorul laturii", care pe 4111 e UnitateInterna/ContPropriu): scrii și `f0/q02b.sql` = varianta F0 cu partenerul luat de pe ORICARE latură (regula din amendamentul pasului 1), iar egalitatea se verifică față de q02b; diferența q02 ↔ q02b se raportează ca fapt |
| Q03 | fișa 4111 pe 2025: postările cu `Cont=4111`, ordonate pe `Data, TranzactieId`, sold cumulativ = fereastră Σ(D) − Σ(C); contrapartida = `string_agg` a celorlalte conturi ale tranzacției (join pe `TranzactieId`) — măsori CU și FĂRĂ contrapartidă (două cifre) |
| Q04 | Q03 filtrată pe `Partener` = partenerul cel mai activ pe 4111 în cub (îl alegi cu o interogare și îl scrii); `f0/q04b.sql` = varianta F0 cu partenerul de pe oricare latură, pentru egalitate |
| Q05 | `Cont=4111`, `Data ≤ 2025-12-31`, per `Partener`: Σ D, Σ C, sold — FĂRĂ snapshot; varianta +S: din `cub."Sold"` la 2025-12-31 (vezi C) |
| Q06 | postările cu `Cont` ∈ conturile `RolTert<>0`, `Data ≤ 2025-12-31`, per `Unitate`: Σ D − Σ C, `HAVING ≠ 0`; +S: idem din `Sold`. Egalitatea față de F0 se face pe intersecția tipurilor de document (F0 are partide și pe NotaTransfer/NIR/DSC — se raportează câte și de ce, nu se maschează) |
| Q07 | `Spatiu=3`, `PerioadaDeclarare=202506`, `CodTva.Sens=1`; per (`DocumentId`, `TranzactieId`, `CodTvaId`): Σ Valoare pe `RolTva` (Bază, Taxă); pentru afișare join pe `Documente` (`Numar`, `Data`, `ClrType`) |
| Q08 | `Spatiu=3`, `PerioadaDeclarare=202506`; per (`DocumentId`, `TranzactieId`, `Partener`, `CodTvaId`): Σ Bază, Σ Taxă — doar agregatul, ca F0 |
| Q09 | `Spatiu=2`, `Gestiune=TRANZIT`, `Data ≤ 2025-06-30`; per `Unitate`: Σ Cantitate, Σ Valoare, `MIN(Data)`; `HAVING Σ Cantitate > 0`, `ORDER BY MIN(Data), Unitate` — ordinea FIFO în SQL (azi e C#, N+1) |
| Q10 | `Spatiu=2`, `Data ≤ 2025-12-31`, gestiunile reale (nu sink-ul `UnitateInterna` — potrivește filtrul de `TipStoc` din `f0/q10.sql`); per (`Gestiune`, `Unitate`): Σ Cantitate, Σ Valoare, `HAVING Σ Cantitate ≠ 0` |
| Q11 | `Spatiu=1`, `Cont` ∈ `RolTert<>0`; per (`Partener`, `Cont`, `Latura`): Σ pentru `Data < 2025-06-01` (SI) și luna 06 (rulaje); +S: SI din `Sold` la 2025-05-31 dacă există, altfel de la ultimul an închis + postările de după (vezi C) |
| Q12 | `Spatiu=1`, `Data` în 2025-06: toate postările cu `Tranzactie` (Fel, Data) și `Documente` (`Numar`, `ClrType`); a doua interogare: postările `Spatiu=3` ale acelorași `LinieId` (TaxInformation) |
| Q13 | toate postările cu `DocumentId` = documentul cu 228 de rânduri (toate spațiile, o singură interogare) |
| Q14 | scrierea: într-o tranzacție cu `ROLLBACK`, INSERT al postărilor documentului FCT de 49 de linii (copiate din cub cu `ID` și `TranzactieId` noi, + rândul de `Tranzactie`) pe f1, f2, f3; mediană din 5. Apoi, la ×1 și doar pe f2: `ALTER TABLE … ADD FOREIGN KEY … NOT VALID` pe `Cont`, `Partener`, `Gestiune`, `Produs`, `Unitate` (spre `Conturi`/`Repartitori`/`Produse`/`Loturi`) și `TranzactieId`, re-măsori Q14 (costul FK-urilor la scriere, comparabil cu cele 24 de triggere ale F0), apoi le scoți |

Regulă: dacă o interogare nu se poate scrie ca sume pe coordonate fără să
citească cifre din document, o scrii cât se poate, marchezi ce lipsește și
raportezi; nu inventezi coloane.

## B. Indexii per uz pentru f2 și f3 (`pas2/indexi-f2.sql`, aceiași pe f3)

Proiectezi indexii DIN interogările de mai sus, nu din intuiție:
- pe partiția Contabil: candidații naturali sunt `(Cont, Data)` cu
  `INCLUDE (Latura, Valoare, Partener)` (fișă, balanță pe cont, solduri);
  `(Partener, Cont, Data)` parțial `WHERE Partener IS NOT NULL` (terți);
  `(Unitate, Data)` parțial `WHERE Unitate IS NOT NULL` (partide);
  `(Data)` (GLE, balanță pe interval) — sau BRIN pe `Data` dacă inserția e
  cronologică (verifici corelația fizică cu `pg_stats.correlation` pe `Data`
  și spui cifra);
- Stoc: `(Gestiune, Unitate, Data)`, `(Unitate, Data)`, `(Produs, Data)`;
- Fiscal: `(PerioadaDeclarare, CodTvaId)` cu `INCLUDE (RolTva, Valoare,
  Partener, DocumentId)`, `(LinieId)`;
- identitate (deja există): `TranzactieId`, `DocumentId`, `LinieId`.

Maxim 6 indexi per partiție. Pentru fiecare index scrii în
`pas2/indexi-f2.md` interogările pe care le servește și, după măsurare, dacă
planificatorul l-a FOLOSIT (din EXPLAIN). Un index nefolosit de nicio
interogare se scoate din set și se spune. f3 primește exact același set (pe
partițiile pe an, Postgres le creează per sub-partiție).

## C. Snapshot-ul `cub."Sold"` (+S)

`Sold(Spatiu, Granita date, Cont, Latura, Partener, Gestiune, Produs,
Unitate, CodTvaId, RolTva, Carte, CodFunctional, CodEconomic, SursaFinantare,
UnitateOrganizatorica, Proiect, CentruCost, Cantitate, Valoare)` = Σ pe
TOATE coordonatele, pe fiecare spațiu, la granițe: la ×1 la 2024-12-31 și
2025-12-31; la ×10 la fiecare 31 decembrie 2016…2025. Se umple din f2 cu un
`INSERT … SELECT … GROUP BY`; timpul de construcție și mărimea se raportează.
Interogările +S (Q01, Q02, Q05, Q06, Q11) citesc `Sold` la cea mai apropiată
graniță ≤ data cerută + postările de după ea. Probă de egalitate:
`Sold` la 2025-12-31 == Σ directă din f2 pe același gran (număr de abateri = 0).

## D. Măsurătorile

Forme: F0 (`public`), F1, F2, F3, F2+S, F3+S (S doar la Q01/02/05/06/11).
Scări: x1, x10. Disciplina FZ-D8: `EXPLAIN (ANALYZE, BUFFERS)`, 6 rulări,
prima aruncată, mediana celor 5 (raportezi și min/max), `shared hit/read`
din prima rulare caldă. Setările Postgres NEatinse (raportează `SHOW
shared_buffers/work_mem/max_parallel_workers_per_gather/jit` o dată).

Ordinea: toate interogările pe o formă, apoi forma următoare (nu interpola
formele). La ×10 rulezi întâi ×1 complet, apoi ×10.

Ieșiri:
- `pas2/explain/qNN-<forma>-<scara>.txt` (a treia rulare, integrală);
- `pas2/rezultate.md`: un tabel per scară, rând = interogare, coloane = forme,
  celula = mediană ms (min–max); sub el `hit/read`; rândul de mărimi
  (tabelă, indexi, per partiție) per formă din `pas1/out/marimi.txt` +
  `Sold`;
- `pas2/egalitate.md`: la ×1, pentru fiecare Q01–Q13, rezultatul F0 (sau
  q02b/q04b) și F2 dump-uite sortate (`\copy … TO`) și diff-uite; scrii
  numărul de rânduri identice/diferite și, la diferență, PRIMELE 5 rânduri
  diferite cu explicația mecanică (nu „probabil"); Q06 și Q10 pe intersecția
  declarată;
- `pas2/scriere.md`: Q14 pe f1/f2/f3 fără FK, f2 cu FK NOT VALID, F0 (din
  `f0/explain-q14.txt`), la ×1 și ×10 (fără FK la ×10).

## Reguli

- Nu modifici F0 (`public`), nu atingi Flax, nu atingi `nou/`, nu comiți.
- Indexii se creează O DATĂ, înaintea măsurătorilor pe f2/f3 (după ce ai
  scris justificarea); `ANALYZE` după creare. Nu ajustezi indexii după ce ai
  văzut cifre („tuning în buclă") — dacă vezi un index nefolosit sau un plan
  prost, îl NOTEZI cu EXPLAIN-ul, nu re-măsori cu alt set. O singură iterație
  e permisă și declarată: dacă un index proiectat e ignorat pe TOATE
  interogările lui, îl scoți și re-rulezi doar acele interogări, cu ambele
  cifre raportate.
- Regula de oprire: dacă ×10 nu există sau reconcilierea pasului 1 e roșie,
  te oprești. Dacă o interogare pe cub durează > 60 s la ×10, o rulezi o
  singură dată, notezi și mergi mai departe.
- Rulările lungi (seria ×10) ca proces detașat cu log, nu task de fundal al
  harness-ului (secerat la 10 min).
- Raportul final (max 25 de linii): tabelul medianelor la ×1 și ×10 comprimat
  (interogare: F0 / F1 / F2 / F3 / +S), egalitatea (câte din 13 identice, care
  nu și de ce), indexii folosiți/nefolosiți, ce cumpără snapshot-ul, Q14 cu
  și fără FK, cele 3 surprize.
