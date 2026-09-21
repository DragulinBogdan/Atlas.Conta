# Indexii per uz pentru f2 și f3 — proiectați DIN lotul Q01–Q14, înainte de măsurători

Regula pasului: setul se fixează O DATĂ, cu justificare scrisă, se creează,
se face `VACUUM (ANALYZE)`, apoi se măsoară. Nu se ajustează după ce s-au
văzut cifre. f3 primește EXACT același set (creat pe partiția-părinte de
spațiu, Postgres îl propagă pe sub-partițiile pe an).

Indexii de identitate există deja din pasul 1 și NU intră în bugetul de 6:
PK (`Spatiu,ID` pe f2 / `Spatiu,Data,ID` pe f3), `TranzactieId`,
`DocumentId`, `LinieId`.

## Setul (6 indexi noi pe tot cubul: 3 Contabil, 2 Stoc, 1 Fiscal)

| Id | Partiție | Definiție | Interogările servite |
|---|---|---|---|
| C1 | Contabil | `("Cont","Data") INCLUDE ("Latura","Valoare","Partener","Unitate")` | Q01, Q02, Q03, Q05, Q06, Q11 (și Q04 ca rezervă) |
| C2 | Contabil | `("Partener","Cont","Data") WHERE "Partener" IS NOT NULL` | Q04 |
| C3 | Contabil | `("Data")` | Q12 |
| S1 | Stoc | `("Gestiune","Unitate","Data") INCLUDE ("Cantitate","Valoare")` | Q09, Q10 |
| S2 | Stoc | `("Produs","Data") INCLUDE ("Cantitate","Valoare","Gestiune","Unitate")` | Q09b (calea FIFO reală: produs × gestiune) |
| F1 | Fiscal | `("PerioadaDeclarare","CodTvaId") INCLUDE ("RolTva","Valoare","Partener","DocumentId","TranzactieId","LinieId")` | Q07, Q08 |

## De ce fiecare

**C1** e indexul-atelier al spațiului contabil. Toate interogările de balanță,
fișă, solduri și terți intră pe `Cont` și taie pe `Data`; măsura (`Valoare`),
sensul (`Latura`) și cele două coordonate pe care se grupează (`Partener` la
Q02/Q05/Q11, `Unitate` la Q06) sunt în `INCLUDE`, deci agregatele pot fi
`Index Only Scan` fără atingerea heap-ului. Q01/Q02 n-au filtru selectiv —
acolo pariul e că parcurgerea integrală a unui index mai îngust decât rândul
(≈76 B contra ≈182 B) bate `Seq Scan`-ul paralel; planificatorul decide, iar
rezultatul se raportează ca atare.

**C2** e singurul index proiectat pentru o singură interogare. Q04 („fișa
contului × partener") intră pe partener, nu pe cont: în cub partenerul cel mai
activ pe 4111 are 3.034 postări din 144.241, deci o cheie care începe cu
`Partener` reduce cu două ordine de mărime față de C1. Fără `INCLUDE`: fișa
întoarce rânduri, deci heap-ul se atinge oricum. Parțial pe `Partener IS NOT
NULL` — 533.194 din 697.660 de postări contabile au partener la ×1.

**C3** servește Q12 (rândurile unei luni, citite integral pentru SAF-T GLE) și
ramura `Data ≤ granița` a lui Q01/Q11 când planificatorul o preferă.
**BRIN considerat și respins**, cu cifra: `pg_stats.correlation` pe `Data` e
0,9996 pe `f2.Postare_Contabil`, 0,862 pe Stoc, −0,776 pe Fiscal — deci BRIN
ar fi viabil și de ~200× mai mic. Singura interogare condusă de `Data` din lot
(Q12) EXTRAGE rândurile unei ferestre înguste, unde tid-urile exacte ale unui
b-tree bat un bitmap lossy pe intervale de 128 de pagini; iar pe f3 fereastra
e deja tăiată de partiționarea pe an. Nu se creează.

**S1** acoperă ambele interogări de stoc: Q09 intră pe `Gestiune` și grupează
pe `Unitate` (lot), Q10 grupează chiar pe `(Gestiune, Unitate)` peste tot
istoricul. Cu `Cantitate` și `Valoare` în `INCLUDE`, Q10 e `Index Only Scan` +
`GroupAggregate` fără sortare (cheia indexului e deja ordinea grupării).

**S2** e calea FIFO reală: `StocService.AlocaFifoTolerant` intră pe produs
(`SolduriService.MiscariCumulate(os, data, produsId)`), apoi filtrează
gestiunea. Q09 din spec e scrisă pe gestiune; varianta de egalitate Q09b —
cea care oglindește F0 — filtrează produs ȘI gestiune, și pe ea se judecă S2.

**F1** face Q07 și Q08 `Index Only Scan`: ambele intră pe `PerioadaDeclarare`
(o lună din 12 la ×1, din 120 la ×10) și grupează pe cheia fiscală. `INCLUDE`
poartă tot ce citesc cele două: rolul, măsura, partenerul și cele trei
identități de cauză. Cheia `(PerioadaDeclarare, CodTvaId)` permite și filtrul
pe sens al lui Q07, care se traduce în `CodTvaId ∈ mulțime mică`.

## Candidați RESPINȘI înainte de măsurare (și de ce)

- **`("Unitate","Data") WHERE "Unitate" IS NOT NULL` pe Contabil** (sugerat de
  spec pentru partide). Q06 — singura interogare de partide din lot — NU intră
  pe unitate: filtrul ei selectiv e `Cont ∈ conturile cu RolTert ≠ 0`, iar
  unitatea e doar cheia de grupare, purtată de `INCLUDE`-ul lui C1. Un index
  pe `Unitate` ar fi parcurs integral (≈400.000 de intrări la ×1) în loc de
  cele ≈312.000 ale ramurii de cont. Lookup-ul per partidă (SAF-T Payments
  `SourceDocumentID`, fișa unei partide) nu e în Q01–Q14 — se notează ca
  neprobat, nu se indexează pe speculație.
- **`("Unitate","Data")` pe Stoc** (sold pe lot). Aceeași logică: Q10 grupează
  pe `(Gestiune, Unitate)`, deci prefixul corect e cel al lui S1; fișa unui
  singur lot nu e în lot.
- **`("LinieId")` pe Fiscal** — există deja ca index de identitate
  (`ix_f2_linie`), care servește a doua interogare a lui Q12.

## DUPĂ măsurare: ce a atins planificatorul

| Id | folosit la ×1 | folosit la ×10 | mărime ×10 (f2+f3) | verdict |
|---|---|---|---|---|
| C1 `(Cont, Data) INCLUDE (…)` | doar Q03c (f2) și Q03 (f3) | **niciodată** | **1.024 MB** | **SCOS** |
| C2 `(Partener, Cont, Data)` parțial | Q04 | Q04 | 311 MB | păstrat |
| C3 `(Data)` | Q03, Q03c, Q11, Q12, Q12b, Q01+S, Q02+S | Q03, Q03c, Q12, Q12b, Q01+S, Q02+S, Q11+S | 94 MB | păstrat |
| S1 `(Gestiune, Unitate, Data) INCLUDE (…)` | doar Q09 | **niciodată** | **360 MB** | **SCOS** |
| S2 `(Produs, Data) INCLUDE (…)` | Q09b | Q09b | 456 MB | păstrat |
| F1 `(PerioadaDeclarare, CodTvaId) INCLUDE (…)` | Q07, Q08 | Q07, Q08 | 390 MB | păstrat |
| `ix_sold_cont` | toate cele cinci +S | Q01+S, Q02+S, Q05+S, Q11+S | — | păstrat |
| `ix_sold_unitate` | — (plan pe `ix_sold_cont`) | Q06+S | — | păstrat |

Pariul lui C1 a PICAT, și se vede de ce în plan: pe partiția Contabil,
Postgres alege `Parallel Seq Scan` peste toate agregatele largi (Q01, Q02, Q05,
Q06, Q11), pentru că un `Index Only Scan` pe un index de ≈76 B/intrare nu bate
citirea secvențială a unei tabele de ≈182 B/rând cu doi lucrători paraleli. Iar
acolo unde filtrul E selectiv (Q03, Q04) planul intră pe `Data` sau pe
`Partener`, nu pe `Cont`. `Cont` nu e o coordonată selectivă pe datele reale:
4111 singur ține 144.241 din cele 697.660 de postări contabile.

## Iterația declarată (singura permisă)

C1 și S1 scoase la ×10 — **1.384 MB de index mai puțin** — și toate
interogările de cub re-rulate. Cifrele ambelor stări:

| Q | F2 cu C1+S1 | F2 fără | F3 cu | F3 fără |
|---|---|---|---|---|
| Q01 | 467,6 | **423,8** (−9%) | 549,6 | 547,5 |
| Q02 | 977,0 | **865,9** (−11%) | 1.132 | 1.119 |
| Q06 | 636,3 | **567,1** (−11%) | 624,7 | 651,7 (+4%) |
| Q10 | 625,5 | **523,6** (−16%) | 570,6 | 580,0 |
| Q03 | 1.714 | 1.757 (+3%) | 1.196 | 1.248 (+4%) |
| Q09 | 178,6 | 167,5 (−6%) | 184,0 | 183,5 |
| restul | \|Δ\| ≤ 4%, sub pragul de zgomot al seriei | | | |

Scoaterea lor nu costă nimic; pe patru interogări **câștigă**, pentru că 1,4 GB
de index care nu servește la citire concurează totuși pentru `shared_buffers`
și pentru cache-ul sistemului de fișiere, iar scanările secvențiale largi
plătesc diferența în pagini citite de pe disc.

**Setul final: 4 indexi per uz pe tot cubul** — C2, C3, S2, F1 — plus PK și cei
trei de identitate. Tabelul complet: `pas2/out/tabel-x10c.md`.
