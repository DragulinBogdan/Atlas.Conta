# 01 — Interogările de azi ale rapoartelor, ca SQL rulabil (forma F0)

Pasul 2 („fizica") al `docs/nucleu/nucleu-cub-design.md`. Inventar, nu propunere:
pentru fiecare din cele 14 interogări, SQL-ul pe care îl emite (sau pe care îl
echivalează exact) implementarea curentă, rulat o dată cu
`EXPLAIN (ANALYZE, BUFFERS)` pe `Atlas.Conta.Import1C.Flax`.

Fișierele: `f0/q01.sql` … `f0/q14.sql` (fiecare cu antetul lui: `fișier:linie`,
filtrele copiate din cod, comentariul `-- ales:` pe fiecare valoare literală) și
planurile în `f0/explain-qNN.txt`.

## Starea bazei în care s-au măsurat

304.382 rânduri `RegistruContabil`, 283.498 `RegistruStoc`, 90.732 `RegistruTva`,
205.186 documente operate (toate `Stare = Operat`).
**Toate cele 12 luni ale lui 2025 sunt închise**, 2026 e deschisă ⇒
`SolduriService.Referinte` = **o singură referință, (2025, 12)**, cu 184.780 de
rânduri de snapshot contabil, 7.914 de stoc și 201.046 de partide. Asta decide
jumătate din tabelul de mai jos și e prima surpriză (§Surprize 1).

Alegerea parametrilor („cazul greu"), derivată pe baza asta, nu preluată:
cont `4111` = `01a0b48c-a8b2-73ce-a6cd-a045b80364a8` (108.906 rânduri pe debit +
35.335 pe credit = 144.241 atomi; locul 2 e `371` cu 67.330); cont `401` =
`01a0b48c-a8aa-7e36-8f14-4fa2fa4ea009`; repartitorul cel mai activ pe 4111 =
`Sediul central` (107.033); gestiunea cea mai activă = `TRANZIT`
(`01a0b48c-d7ca-73c5-88b4-c0b508a78342`, 58.703 rânduri); documentul cu cele mai
multe rânduri contabile = `NotaContabila SED00000038` (228); documentul de
operare = FCT `FA/EU-2500084872` (49 de linii, 50 de rânduri contabile, 49
fiscale). ID-urile bazei s-au schimbat la re-crearea clonei în felia 28 — cele
din `p5-perf-masuratori.md` §Felia 27 nu mai sunt valabile.

## Tabelul

| # | Raport | Fel | ms | rânduri | Accesul dominant |
|---|---|---|---|---|---|
| Q01 | Balanță plată sintetică 2025 | SQL real (EF, `run-f28/pas3/saft-B09-4.sql`) | 62,9 | 119 | `Parallel Seq Scan` × 2 pe `RegistruContabil` |
| Q02 | Balanță analitică cont × repartitor 2025, la 4111/401 | oglindit | 143,6 | 1.985 | idem + `Sort` 608k rânduri, apoi `Merge Join` |
| Q03 | Fișa contului 4111, 2025 | SQL real (brut, din cod) | 233,2 | 144.239 | `Bitmap Index Scan` pe `ContDebitId`/`ContCreditId` + `WindowAgg` |
| Q04 | Fișa 4111 × repartitorul cel mai activ | SQL real (brut, din cod) | 194,6 | 107.033 | + `Bitmap Index Scan` pe `DimensiuniCredit_RepartitorId` |
| Q05 | Sold cont × repartitor la 31.12, pe 4111 | oglindit | 99,9 | 1.584 | `Index Scan` pe snapshot; ramura de registru e VIDĂ |
| Q06 | Documente cu rest la 31.12 (nefiltrat, fără `LIMIT`) | oglindit (V0) | 102,9 | 91.139 | `Parallel Seq Scan` pe `PartideDeschise`; `Imperecheri` prin index, 0 rânduri |
| Q07 | Jurnal cumpărări 2025-06 | oglindit | 17,5 | 1.543 | `Parallel Seq Scan` pe `RegistruTva` (vezi §Surprize 4) |
| Q08 | D394 2025-06 (doar agregatul) | oglindit | 24,2 | 4.725 | `Seq Scan` pe `RegistruTva` |
| Q09 | FIFO: loturi cu sold > 0 în TRANZIT la 30.06 | oglindit | 0,7 | 30 | `Index Scan` pe `Loturi`/`RegistruStoc` |
| Q10 | SAF-T PhysicalStock la 31.12 | SQL real (EF, `run-f28/pas3/saft-B12-0.sql`) | 238,6 | 110.786 | `Parallel Index Scan` pe `IX_RegistruStoc_LotId` |
| Q11 | SAF-T Customers/Suppliers 2025-06 | oglindit | 112,6 | 43.031 | `Index Scan` pe `IX_RegistruContabil_Data` |
| Q12 | SAF-T GLE 2025-06 (rândurile lunii) | oglindit | 3,4 | 23.563 | `Index Scan` pe `IX_RegistruContabil_Data` |
| Q13 | Citirea unui document pentru storno (3 registre) | oglindit | 0,04 + 0,94 + 0,01 | 0 + 228 + 0 | 3 × `Index Scan` pe `…_DocumentId` |
| Q14 | Operarea unui document cu 49 de linii (3 registre) | echivalent `INSERT … SELECT`, tranzacție cu `ROLLBACK` | 0,06 + 57,2 + 8,7 | 0 + 50 + 49 | 24 de triggere FK (vezi §Surprize 3) |

O singură rulare per interogare, ca în spec — cifrele sunt orientative pentru
FORMĂ, nu mediane calde; nu se compară cu `p5-perf-masuratori.md`, unde
măsurătoarea e end-to-end HTTP.

## Ce NU are echivalent SQL (regula de oprire)

Niciun raport nu s-a oprit complet, dar cinci se termină în memorie, și acolo
stau exact coloanele ne-aditive din §3 al sintezei de coordonate:

- **Q08 / D394**: SQL-ul e doar agregatul pe `(Document × Storno × Partener ×
  Sens × TipTva × Cotă)`. `NrFact` (argmax pe `|Σ Tva|` per document, cu
  departajare pe cota mai mare, `D394Proiectii.cs:422-435`), normalizarea CUI cu
  tăierea repetată a prefixului `RO` (`:205-225`), re-decizia tipului de partener
  pe CUI (`:332-344`) și cele două rezumate sunt C#.
- **Q09 / FIFO**: SQL-ul dă mulțimea candidaților, NEORDONATĂ. Ordinea FIFO
  însăși (`Lot.Data`, apoi `Lot.ID`) se face în C#, după un
  `os.GetObjectByKey<Lot>` **per lot** (`StocService.cs:272-273`) — N+1 pe
  numărul de loturi cu sold.
- **Q10 / PhysicalStock**: mulțimea `TipStoc`-urilor raportate, cheile
  (deschideri ∪ închideri ∪ mișcări), `StockAccountNo`, `UnitPrice` din
  `Lot.PretUnitar` și cele patru avertismente — toate peste agregatul
  materializat.
- **Q11 / Customers-Suppliers**: `PartenerulRandului` (`SaftProiectii.cs:469`),
  identitatea SAF-T cu prefixele `00`…`06`, `AccountID` ca argmax pe mișcare și
  cumularea partenerilor cu același identificator.
- **Q12 / GLE**: trei interogări plate (rânduri contabile, rânduri fiscale,
  antete de document) plus a patra pentru codul de tip; toată structura
  Journal / Transaction / TransactionLine, `TransactionDate = MIN(Data)`,
  identitățile de latură și `TaxInformation` pe cheia `(DetaliuId, Storno)` sunt
  asamblare în memorie.
- Mărunt, dar peste tot: **`DocumentTip` nu e coloană nicăieri** — se completează
  post-materializare, pe pagină (`ContabilProiectii.cs:1094-1102`), în Q03, Q04,
  Q07 și Q12.

## Surprize

**1. Snapshot-ul de la 2025-12 nu servește NICIO balanță a lui 2025 — și
servește integral soldurile la 31.12.** Balanța și fișa cer
`Referinta(os, dataStart − 1)` (`ContabilProiectii.cs:331`, `:801`); pentru anul
2025 asta înseamnă `2024-12-31`, iar singura referință a bazei se încheie la
`2025-12-31` ⇒ **null** ⇒ citire integrală din registru (Q01, Q02, Q03, Q04).
Aceeași bază, aceeași zi: `SoldParteneri` și `DocumenteCuRest` cer
`Referinta(os, laData = 2025-12-31)` ⇒ o găsesc ⇒ citesc DOAR snapshot, iar
ramura de registru rămâne în SQL ca `Data > '2025-12-31' AND Data <= '2025-12-31'`,
vidă prin construcție (`f0/q05.sql`, `SolduriService.cs:247`). Două regimuri de
fizică opuse pe același registru, decise de un parametru de graniță.

**2. Pe 4111, dimensiunea `Repartitor` nu e partenerul.** Din cele 144.241 de
rânduri ale contului, 107.033 poartă o `UnitateInterna` (`Sediul central`) și
31.381 un `ContPropriu`; doar **5.825** poartă un `Partener`, cel mai activ fiind
`FURTUNA MARIUS` cu 114 rânduri. Pe `401`, primul repartitor e gestiunea
`TRANZIT` (50.170). Deci „balanța analitică cont × partener" (Q02) și „sold cont
× partener" (Q05) nu sunt rapoarte de partener pe datele de azi — confirmă pe
cifre nota din `run-nucleu/coordonate/01-contabil.md` §5.6 și amendamentul §5.4
al sintezei (partenerul trebuie să stea pe postarea de terț).

**3. Scrierea: 50 de rânduri contabile declanșează 20 de triggere FK, ~33 din
cele 57 ms.** Fiecare din cele 16 coloane plate de dimensiune are FK propriu, plus
cele două conturi, documentul și linia. Cel mai scump:
`FK_RegistruContabil_Produse_DimensiuniCredit_MaterialId`, **17,4 ms** la 50 de
apeluri (`f0/explain-q14.txt`). `RegistruTva` mai adaugă 4 triggere, din care
`FK_RegistruTva_TipuriTva_TipTvaId` 3,2 ms la 49 de apeluri. Costul de scriere al
unei postări e azi dominat de verificarea coordonatelor, nu de inserție.

**4. Index existent și nefolosit: `IX_RegistruTva_PerioadaAn_PerioadaLuna`.**
`TvaProiectii.IntreLuni` scrie predicatul ca EXPRESIE aritmetică
(`PerioadaAn * 100 + PerioadaLuna >= 202506`, `TvaProiectii.cs:145-157`), pe care
niciun b-tree pe cele două coloane nu o poate satisface ⇒ `Seq Scan` peste toate
cele 90.732 de rânduri, pentru o fereastră de o lună (Q07, Q08). Scris direct
(`PerioadaAn = 2025 AND PerioadaLuna = 6`), Postgres alege `Index Only Scan` și
citește 6.980 de rânduri în **1,6 ms**. Verificat, nu dedus. Nu e o propunere de
fix — e o constatare de fizică pentru F0.

**5. `DocumenteCuRest` costă în funcție de cât de departe e `laData` de ultima
închidere.** Forma nefiltrată și fără `LIMIT` — cea pe care o materializează
`PerioadaService.RestScadent` — a costat 220–232 ms în felia 27 (referința era
11/2025, fereastra de `Imperecheri` deschisă) și **102,9 ms** acum, pe aceeași
bază: cu `laData` chiar la granița referinței, fereastra e vidă, `Imperecheri` se
atinge prin `IX_Imperecheri_Data` cu 0 rânduri, iar `Nested Loop` cu
„2.184.985 rânduri respinse" din diagnosticul de atunci dispare complet. Cauza
nu era numărul de partide, ci lărgimea ferestrei deschise.

**6. Filtrul de grilă pe cont nu coboară sub `GROUP BY` (Q02).** `Balanta` n-are
parametru `contId`; restrângerea la 4111/401 vine din `loadOptions`, deci stă
deasupra agregatului. Postgres nu împinge predicatul, dar `Merge Join`-ul cu cele
două conturi oprește agregatul devreme (2.318 grupe emise din ~71.000). Sortarea
celor 608k de rânduri unpivotate se plătește totuși întreagă: 94 din cele 143 ms.

## Reproducere

`docker exec contapal-postgres-1 psql -U postgres -d Atlas.Conta.Import1C.Flax -Atf /tmp/<fișier>`
după `docker cp`. Q13 și Q14 au mai multe instrucțiuni în același fișier; Q14
rulează între `BEGIN` și `ROLLBACK` — verificat după rulare că numărul de rânduri
al celor trei registre e neschimbat (304.382 / 90.732 / 283.498). Nicio scriere
persistată, niciun fișier de producție atins.
