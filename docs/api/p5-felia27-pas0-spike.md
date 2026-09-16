# Felia 27, pasul 0 — spike: cursa perioadei (F27-D1) și costul soldurilor (F27-D3)

Data: 2026-09-16. Stare: spike încheiat, fără cod de producție.
Contractul: `docs/api/p5-felia27-perioade-solduri-contract.md` (§Scop, F27-D1,
F27-D3, pasul 0, F27-r3).

Codul experimentelor stă în scratchpad-ul sesiunii
(`scratchpad/spike/` — SQL + `masoara.sh` + `lock-proba.sh`; `scratchpad/spike/ef/`
— program C# cu `ProjectReference` spre Module). Nimic din el nu intră în repo.

**Baza de experiment**: `Atlas.Conta.Import1C.Flax` (Postgres 18.4 în
`contapal-postgres-1:5444`), 304.382 rânduri `RegistruContabil`, 283.498
`RegistruStoc`, 24 perioade 2025–2026 (toate deschise), date între 12/2024 și
12/2025. Indexurile `spike_*` și tabela `spike_sold_contabil` au fost create,
măsurate și ȘTERSE; verificat la final: 0 indexuri `spike%`, 0 tabele `spike%`,
0 perioade de sondă, 24 de perioade, 0 închise.

**Metoda de măsurare**: `EXPLAIN (ANALYZE, BUFFERS)`, 6 rulări, prima aruncată,
mediana celor 5 calde — metoda din `docs/api/p5-perf-masuratori.md:96-110`.

---

## Incident de mediu, raportat înainte de rezultate

Working tree-ul principal (`D:\Dev\Atlas.Conta`, branch
`p5-f27-perioade-solduri`) era, în timpul spike-ului, **ocupat de pasul 1**:
`PerioadaFiscala` mutată în fișier propriu cu `InchisaLa`, migrația
`20260916123442_F27Pas1PerioadaLant`, `PerioadaService`, `PerioadeController`.
Primul build al programului de probă a picat pe `column "InchisaLa" of relation
"PerioadeFiscale" does not exist` — modelul pasului 1 contra schemei Flax
nemigrate.

Rezolvat prin **worktree pinuit pe commit-ul contractului** (`353ed3e`, detached,
în scratchpad), spre care arată `ProjectReference`-ul programului de probă. Toate
cifrele de mai jos sunt măsurate contra modelului de la `353ed3e`, nu contra
lucrului în curs al pasului 1.

---

# Întrebarea A — cursa închidere ↔ operare (F27-D1)

## A.0. Primitivele de blocare, confirmate pe două conexiuni concurente

`scratchpad/spike/lock-proba.sh`: sesiunea A ține o tranzacție deschisă 3 s cu un
lock pe rândul `PerioadeFiscale` 12/2025; sesiunea B pornește după 1 s cu
`lock_timeout = 12s` și se măsoară cât așteaptă.

| Proba | A ține | B cere | B a așteptat | Verdict |
|---|---|---|---|---|
| A1 | `FOR SHARE` | `FOR UPDATE` | 2.215 ms | închiderea AȘTEAPTĂ operarea |
| A2 | `FOR UPDATE` | `FOR SHARE` | 2.249 ms | operarea AȘTEAPTĂ închiderea |
| A3 | `FOR SHARE` | `UPDATE "Inchisa"` | 2.225 ms | scrierea directă a lui `Inchisa` așteaptă |
| A4 | `FOR SHARE` | `FOR SHARE` | 266 ms | două operări concurente NU se blochează |

A3 e cea care contează cel mai mult: **închiderea nu are nevoie de `FOR UPDATE`
ca să fie blocată** — `UPDATE`-ul pe care EF îl emite pentru `Inchisa = true` ia
el însuși lock-ul exclusiv și așteaptă. `FOR UPDATE` rămâne totuși necesar la
ÎNCEPUTUL tranzacției de închidere, altfel `SUM`-ul rulează înainte de lock.

A4 spune că `FOR SHARE` nu introduce conflicte între operări — spre deosebire de
F3 (vezi mai jos).

**Planul lui `FOR SHARE` pe `PerioadeFiscale`** (cerut explicit de pasul 0):

```
LockRows  (cost=0.00..29.96 rows=1 width=7) (actual time=0.038..0.039 rows=1.00 loops=1)
  Buffers: shared hit=2
  ->  Seq Scan on "PerioadeFiscale"  (actual time=0.008..0.009 rows=1.00 loops=1)
        Filter: (("An" = 2025) AND ("Luna" = 12))
        Rows Removed by Filter: 23
Execution Time: 0.065 ms
```

0,065 ms, 2 buffere, seq scan pe 24 de rânduri. **Nu schimbă planul operării**:
e un statement separat, nu o modificare a interogărilor existente.

## A.1. Ce spun sursele (DevExpress 26.1.3 + EF Core 10.0.12)

Citate verificate pe surse, nu deduse:

- **XAF nu deschide nicio tranzacție la commit.** `EFCoreObjectSpace.DoCommit`
  (`EFCoreObjectSpace.cs:474-502`) face un singur `DbContext.SaveChanges()` gol.
  Singurul `BeginTransaction` din tot arborele `DevExpress.ExpressApp.EFCore` e în
  `Updating/EFCoreDatabaseSchemaUpdater.cs:104` (schema updater, fără legătură).
- **EF se ÎNROLEAZĂ în tranzacția curentă.** `BatchExecutor.Execute` (decompilat,
  Relational 10.0.12) deschide tranzacție proprie doar dacă
  `connection.CurrentTransaction == null`; altfel ia ramura `else`, creează un
  savepoint și folosește tranzacția existentă. Commit-ul îl face doar pentru
  tranzacția pe care a deschis-o el.
- **`SavingChanges` rulează ÎNAINTE de deschiderea tranzacției.**
  `DbContext.SaveChanges` cheamă `SaveChangesStarting` (deci
  `ISaveChangesInterceptor.SavingChanges`) înainte de `StateManager.SaveChanges`,
  care abia el ajunge la `BatchExecutor`. **Consecință: SQL brut executat într-un
  `ISaveChangesInterceptor` rulează în AUTOCOMMIT, în afara tranzacției
  commit-ului — varianta „F2 pe `ISaveChangesInterceptor`" e moartă.**
- **`IDbTransactionInterceptor.TransactionStarted` rulează cu tranzacția
  deschisă**, inclusiv pentru cea IMPLICITĂ a lui `SaveChanges`
  (`RelationalConnection.BeginTransaction` cheamă `TransactionLogger.TransactionStarted`
  după `ConnectionBeginTransaction`). **Dar `CurrentTransaction` nu e încă setat**
  la acel moment (se setează în `CreateRelationalTransaction`, după return), deci
  `Database.ExecuteSql` de acolo NU prinde tranzacția — comanda trebuie legată
  explicit de `DbTransaction`-ul primit ca parametru.
- **`OptimisticLockField` E concurrency token**: `BaseObject.cs:76-78`
  `[ConcurrencyCheck, NotMapped]`, reintrodus în model de
  `modelBuilder.UseOptimisticLock()` — apelat în Conta la
  `BackOfficeDbContext.cs:173`, materializat în migrație
  (`InitialCreate.Designer.cs:50-54`, `.IsConcurrencyToken()`). Incrementat de
  `EFCoreOptimisticLockInterceptor.SavingChanges`
  (`DataLocking/EFCoreOptimisticLockInterceptor.cs:49-84`) pe entitățile
  `Modified`/`Deleted`.
- **Interceptorul de optimistic lock NU e activ pe toate căile.** Îl înregistrează
  `AddEFCore` (`ObjectSpaceProviderBuilderExtensions.cs:74`, `:111`) și
  `AddSecuredEFCore` (`DevExpress.EntityFrameworkCore.Security/ApplicationBuilder/
  OSProviderBuilderExtensions.cs:73`) — deci hosturile Conta îl au
  (`WebApi/Startup.cs:308` folosește `AddSecuredEFCore`). Uneltele standalone
  (ModelCheck, Import1C, Migrare) construiesc providerul de mână și **nu** îl
  înregistrează — ModelCheck chiar o declară, `Program.cs:127-130`.

## A.2. Ce spun experimentele (program C# pe calea reală XAF)

`scratchpad/spike/ef/Program.cs`, cu `EFCoreObjectSpaceProvider<BackOfficeEFCoreDbContext>`
construit ca în ModelCheck. Verificarea „a blocat?" se face cu un al doilea
conector Npgsql independent, care încearcă `UPDATE "PerioadeFiscale" SET
"Inchisa" = "Inchisa"` cu `lock_timeout = 800ms` și raportează `55P03`.

Rezultatul rulării finale: 11 probe, 1 FAIL — iar acel FAIL e chiar constatarea
centrală despre F2 (vezi F2-b).

### F1 — tranzacție explicită în adaptor: **FUNCȚIONEAZĂ**

```
OK   F1-a CommitChanges NU arunca desi apelantul deschisese deja tranzactia
OK   F1-b FOR SHARE tine randul perioadei blocat pentru un scriitor concurent
     cat timp tranzactia apelantului e deschisa, si inainte si dupa CommitChanges
OK   F1-c rollback-ul tranzactiei explicite anuleaza si ce a scris CommitChanges
OK   F1-d dupa rollback randul perioadei e liber pentru scriitori
```

Forma probată, integral:

```csharp
using var os = provider.CreateObjectSpace();
var db = ((EFCoreObjectSpace)os).DbContext;     // precedentul ContabilProiectii.cs:790
using var tx = db.Database.BeginTransaction();
var inchisa = db.Database.SqlQuery<bool>(FormattableStringFactory.Create(
    "SELECT \"Inchisa\" AS \"Value\" FROM \"PerioadeFiscale\" "
    + "WHERE \"An\" = {0} AND \"Luna\" = {1} FOR SHARE", an, luna)).Single();
// … comanda, inclusiv os.CommitChanges() …
tx.Commit();
```

Ce cere: cast-ul la `EFCoreObjectSpace` (deja folosit în producție în trei
locuri), `SqlQuery` cu `AS "Value"` pentru scalar, și `"GCRecord" = 0` EXPLICIT
în predicat (convenția 66 — `SqlQuery` nu trece prin filtrul global).

Ce risc: **niciunul nou măsurat**. F1-c arată chiar un câștig — unitatea de lucru
devine tranzacția apelantului, deci un rollback anulează și ce a scris
`CommitChanges`; azi asta e adevărat doar prin accidentul „un singur
`SaveChanges`".

### F2 — blocare în `IDbTransactionInterceptor.TransactionStarted`: **FUNCȚIONEAZĂ CONDIȚIONAT**

```
OK   F2-a la un CommitChanges cu mai multe entitati interceptorul se declanseaza
     si FOR SHARE-ul lui chiar blocheaza un scriitor concurent
FAIL F2-b la un CommitChanges cu O SINGURA entitate interceptorul NU se declanseaza
OK   F2-c cu AutoTransactionBehavior.Always interceptorul se declanseaza si la o
     singura entitate, iar FOR SHARE-ul lui blocheaza
```

Două capcane, ambele probate, ambele tăcute:

1. **`AutoTransactionBehavior.WhenNeeded` (implicit) sare peste tranzacție** când
   commit-ul încape într-un singur batch. Măsurat: la 6 entități interceptorul se
   declanșează o dată; la UNA — zero. Adică gardianul pur și simplu **nu rulează**,
   fără niciun semn. Remediul probat: `db.Database.AutoTransactionBehavior =
   AutoTransactionBehavior.Always` pe `DbContext`-ul comenzii (F2-c trece).
2. **Semnătura interfeței.** `IDbTransactionInterceptor.TransactionStarted` are
   `TransactionEndEventData`, nu `TransactionEventData`. O clasă care implementează
   interfața cu parametrul greșit **compilează fără warning** (implementarea
   implicită a interfeței rămâne activă) și interceptorul nu se declanșează
   niciodată. Prima rulare a spike-ului a pierdut exact așa trei probe. Forma
   corectă e clasa de bază cu `override`, care face greșeala imposibilă:

```csharp
sealed class GardianPerioadaInterceptor : DbTransactionInterceptor {
    public override DbTransaction TransactionStarted(DbConnection connection,
            TransactionEndEventData eventData, DbTransaction result) { … }
}
```

Costul: exact **1 statement în plus per operare** (interceptorul se declanșează o
singură dată per `SaveChanges`, măsurat). Avantajul real: acoperă ORICE cale care
scrie registre, nu doar cele patru metode ale adaptorului.

### F3 — concurență optimistă pe rândul perioadei: **FUNCȚIONEAZĂ, cu conflict fals garantat**

```
OK   F3-a doua CommitChanges pe ACELASI rand de perioada, ambele cu bump fortat,
     il refuza pe al doilea
     MASURAT: exceptia lui B = UserFriendlyException: The object you are trying
     to save was changed by another user. Please refresh data.
     MASURAT: OptimisticLockField e concurrency token in model = True
```

Bump-ul forțat funcționează fără a atinge nicio valoare de domeniu:
`db.Entry(p).Property(nameof(PerioadaFiscala.Inchisa)).IsModified = true` →
entitatea trece în `Modified` → interceptorul DevExpress incrementează
`OptimisticLockField` → `UPDATE … WHERE "ID" = @id AND "OptimisticLockField" = @orig`
→ al doilea commit prinde 0 rânduri. `Inchisa` rămâne `false` (F3-b).

F3 închide cursa în AMBELE sensuri, ceea ce e mai mult decât pare:
- operarea comite între `SUM`-ul închiderii și commit-ul ei ⇒ commit-ul închiderii
  eșuează, deci snapshot-ul greșit nu se persistă;
- închiderea comite între `VerificaDeschisa` și commit-ul operării ⇒ operarea
  eșuează, deci documentul nu intră în perioada închisă.

**Riscul, cuantificat**: orice două operări în ACEEAȘI lună ale căror ferestre se
suprapun se resping reciproc — nu probabilistic, ci prin construcție, fiindcă
ambele ar bump-a același rând. Fereastra nu e commit-ul, ci toată durata comenzii
(`VerificaDeschisa` e primul lucru din `CalculeazaSiValideaza`, commit-ul e
ultimul din `Opereaza`). Pe un document mare cu FIFO, asta înseamnă secunde. E o
regresie NOUĂ pe multi-operator, exact ce 25f/F27-r8 lăsau parcat — **nu doar
parcat, ci înrăutățit**. În plus, mesajul ajunge la operator ca „obiectul a fost
modificat de alt utilizator", care pe un document e o minciună.

A patra problemă: F3 e activ doar unde e înregistrat
`EFCoreOptimisticLockInterceptor` — pe hosturi da, în ModelCheck/Import1C NU, deci
probele din ModelCheck n-ar exersa gardianul fără să-l înregistreze anume.

### F4 — hibrid

Funcționează, fiindcă F1 și F2 funcționează separat și sunt compatibile (F2-a a
rulat peste tranzacția implicită; F1 peste cea explicită; A1/A2 arată că
`FOR SHARE` și `FOR UPDATE` se exclud corect indiferent cine le ia).

**La întrebarea directă din spec — „F1-ul «interzis» de regula de oprire e în
realitate doar închiderea, nu operarea?" — răspunsul măsurat e NU, F1 e viabil și
pe operare**, și motivul e de citire a contractului, nu de tehnică:

- Regula de oprire spune „dacă blocarea cere tranzacție explicită **în afara
  ObjectSpace-ului**". Tranzacția din F1 e a `DbContext`-ului ObjectSpace-ului
  însuși, obținută prin cast-ul deja canonic (`ContabilProiectii.cs:790-793`,
  `GardianEditare.cs:1036-1044`). Nu e `TransactionScope`, nu e a doua conexiune,
  nu e un ObjectSpace paralel.
- `MotorOperare` rămâne NEATINS în ambele variante: gardianul trăiește în
  `GardianPerioada`, tranzacția în `OperareApi` (adaptorul), care e deja declarat
  „tranzacția integral a motorului" în contractul lui de apelant.

Dacă lead-ul citește regula strict (orice `BeginTransaction` în jurul operării o
declanșează), F4 literal rămâne disponibil și probat: operarea pe F2, închiderea
pe F1. Interpretarea regulii e decizie de contract, nu rezultat de măsurare — o
las explicit deschisă, nu o normalizez.

### F5 — sigilare în doi pași: **respinsă**

Nu e nevoie de ea: F1 și F2 funcționează amândouă, deci nu suntem în cazul
„nimic nu blochează". Ce ar costa: rupe invariantul F27-D3 „rândurile snapshot
există ⇔ perioada e închisă" pe toată durata dintre cele două commit-uri și la
orice eșec între ele. Marcajul `InchisaLa = null` cât nu e snapshot l-ar face
tehnic recuperabil, dar introduce o a treia stare a perioadei („închisă fără
solduri") pe care toți consumatorii lui `SolduriService` ar trebui s-o știe, ca
să nu citească un snapshot inexistent ca „sold zero". Preț mare pentru o problemă
pe care celelalte forme o rezolvă.

## A.3. Recomandarea pentru A

**F4 în varianta F1 pe ambele capete**:

- **Închiderea** (cod NOU al feliei): tranzacție explicită proprie —
  `BeginTransaction` → `FOR UPDATE` pe rândul perioadei → verificarea F27-D2 →
  `SUM` → snapshot + partide → `Inchisa = true` → `CommitChanges` → `Commit`.
- **Operarea**: tranzacție explicită în `OperareApi`, în jurul celor patru metode
  (3 linii fiecare); `GardianPerioada.VerificaDeschisa` face citirea prin
  `SqlQuery … FOR SHARE` cu `"GCRecord" = 0` explicit, **în locul**
  `FirstOrDefault`-ului de azi, nu pe lângă el — deci net **zero statement-uri în
  plus**, nu unul. Regula „perioadă absentă = închisă" rămâne: 0 rânduri ⇒ excepție.

De ce F1 și nu F2 pe operare, deși F2 e mai „la margine": precondiția lui F2 e
`AutoTransactionBehavior.Always`, un setting global a cărui omisiune dezactivează
gardianul **tăcut** (F2-b), plus capcana de semnătură care compilează curat. Un
gardian de graniță absolută (decizia 14) nu are voie să depindă de o condiție
invizibilă. F1 e explicit la locul apelului și se probează direct.

F2 rămâne alternativa dacă `OperareApi` trebuie să rămână neatins; atunci
`AutoTransactionBehavior.Always` și `DbTransactionInterceptor` (clasa, nu
interfața) sunt OBLIGATORII și trebuie probate ca atare în ModelCheck.

F3 se respinge: rezolvă cursa, dar cumpără o regresie pe multi-operator pe care
F1 și F2 nu o au (A4 arată că două `FOR SHARE` coexistă).

## A.4. Constatare colaterală — unicitatea `(An, Luna)` lipsește

`PerioadeFiscale` are DOAR `PK_PerioadeFiscale` pe `ID`; niciun index pe
`(An, Luna)`, unic sau nu. Duplicatele sunt posibile azi, iar
`GardianPerioada.VerificaDeschisa` folosește `FirstOrDefault` — ar alege
nedeterminist între două rânduri ale aceleiași luni, unul deschis și unul închis.
Măsurat pe Flax: 0 duplicate acum, deci nimic de reparat, doar de împiedicat.

Recomandare pentru pasul 1, în forma convenției 60a (unicitățile pe tipuri cu
ștergere amânată se filtrează, altfel rândul șters blochează pentru totdeauna
re-crearea aceleiași chei — precedentele `Judet.Cod`, `UnitateMasura.Cod`,
`DviFactura`):

```csharp
modelBuilder.Entity<PerioadaFiscala>().HasIndex(p => new { p.An, p.Luna }).IsUnique()
    .HasFilter("\"GCRecord\" = 0");
```

Indexul servește și blocarea: lookup-ul lui `FOR SHARE` devine index scan în loc
de seq scan — irelevant azi la 24 de rânduri, corect la 60+.

---

# Întrebarea B — costul `SUM`-ului și mărimea snapshot-ului (F27-D3)

## B.1. Cifrele măsurate

Cheia contabilă = cheia COMPLETĂ a atomului: `ContId` + cele 8 dimensiuni ale
LATURII (`DimensiuniDebit_*` pentru latura D, `DimensiuniCredit_*` pentru C),
unpivot `UNION ALL` peste `RegistruContabil` cu `SUM(Debit)` și `SUM(Credit)`
separate. Cheia de stoc = `(LotId, RepartitorId, TipStoc)`.

| Interogare | Mediana (5 calde) | Rânduri rezultat | Plan |
|---|---|---|---|
| Contabil, `Data <= 2025-12-31` (tot istoricul) | **299,6 ms** | 187.378 | Parallel Seq Scan ×2 → Sort (external merge, 13 MB/worker) → Finalize GroupAggregate |
| Contabil, o lună (12/2025) | **70,6 ms** | 24.914 | Parallel Seq Scan ×2 → Finalize HashAggregate |
| Contabil, o lună, CU index pe `Data` | **37,9 ms** | 24.914 | Index Scan ×2 → HashAggregate |
| Stoc, `Data <= 2025-12-31` | **163,9 ms** | 110.786 | Index Scan (`IX_RegistruStoc_LotId`) → Incremental Sort → GroupAggregate |
| Stoc, o lună (12/2025) | **16,2 ms** | 11.120 | Parallel Seq Scan → Sort → GroupAggregate |
| Stoc, o lună, CU index pe `Data` | **14,1 ms** | 11.120 | Index Scan → HashAggregate |

Chei „moarte" (candidate de omis din snapshot):

| | Chei totale | Debit și Credit cumulate ambele 0 | Sold net 0 dar rulaj ≠ 0 |
|---|---|---|---|
| Contabil | 187.378 | 2.598 (1,4 %) | 46 |
| Stoc | 110.786 | **102.872 (92,9 %)** | — |

Asimetria e structurală, nu accidentală: un lot consumat integral iese din stoc
cu cantitate și valoare 0 și rămâne mort pentru totdeauna; un cont × repartitor ×
material rareori ajunge exact la zero pe ambele laturi.

**Scrierea la închidere**, măsurată pe tabela `spike_sold_contabil`:

| Operația | Timp | Rânduri |
|---|---|---|
| Prima închidere (`SUM` integral ≤ 30.11.2025 + INSERT) | **963 ms** | 173.592 |
| Incrementală: snapshot(11/2025) + rulaje(12/2025) → snapshot(12/2025) | **701 ms** | 187.378 |

**Egalitatea probată**: snapshot-ul construit incremental pentru 12/2025 e
identic, la cent și la rând, cu `SUM`-ul direct pe tot istoricul — 187.378 =
187.378, `EXCEPT ALL` gol în ambele sensuri. Invariantul din F27-D3 e verificabil
exact în forma asta.

**Nota de implementare care iese din măsurătoare**: scrierea incrementală trebuie
făcută `UNION ALL` + `GROUP BY`, **nu** `JOIN` între snapshot(P−1) și rulaje(P).
Cele 8 coloane de dimensiune sunt nullable, iar un join pe ele ar cere
`IS NOT DISTINCT FROM`, care nu e hashable — planul ar cădea pe nested loop peste
187 k × 25 k rânduri. `GROUP BY` tratează `NULL`-urile ca egale și dă hash
aggregate. Aceeași observație se aplică indexului unic al tabelei de snapshot:
pe Postgres 15+ (aici 18.4) cheia se declară `UNIQUE NULLS NOT DISTINCT`.

## B.2. Extrapolarea la 5 ani

Baza acoperă ~12,5 luni cu 304.382 rânduri ⇒ ~24.350 rânduri/lună.

| | Azi (12,5 luni) | 5 ani (60 luni), liniar |
|---|---|---|
| Rânduri `RegistruContabil` | 304 k | ~1,46 M |
| `SUM` integral pe cheia completă | 300 ms | **~1,5 s** |
| Chei în snapshot-ul ultimei perioade | 187 k | ~905 k |

Extrapolarea `SUM`-ului e liniară cu o rezervă în favoarea prudenței: planul e
deja `Sort` cu `external merge` pe disc (13 MB/worker), deci la volum de 5 ori mai
mare sortarea crește ușor superliniar. Chiar și cu un factor 2 de siguranță
rămânem la ~3 s.

**Față de pragul contractului (30 s): suntem sub el cu un factor de ~20× azi
extrapolat la 5 ani, ~100× la volumul de azi. Cheia completă a atomului NU se
reproiectează din motiv de TIMP.**

## B.3. Ce contrazice așteptarea: nu timpul, ci MĂRIMEA

Creșterea numărului de chei cumulate, lună cu lună (măsurată, nu estimată):

| Luna | Chei cumulate | Chei noi în lună |
|---|---|---|
| 01/2025 | 22.935 | 22.879 |
| 03/2025 | 63.002 | 27.133 |
| 06/2025 | 107.356 | 22.525 |
| 09/2025 | 146.298 | 24.872 |
| 12/2025 | 187.378 | 24.914 |

Setul de chei crește cu ~14.950/lună și **nu scade niciodată** — o cheie odată
apărută rămâne în toate snapshot-urile următoare, fiindcă snapshot(P) e cumulat
de la începutul bazei.

Consecința, dacă se păstrează câte un snapshot pentru FIECARE perioadă închisă,
cum e scris F27-D3:

| | 1 an | 5 ani |
|---|---|---|
| Rânduri `RegistruContabil` | 304 k | ~1,46 M |
| Rânduri `SoldPerioadaContabil` (Σ pe perioade) | ~1,33 M | **~27 M** |

Adică **tabela de accelerare ajunge de ~19× mai mare decât registrul pe care îl
accelerează** (~6 GB la o lățime de rând de ~230 B, plus indexul de căutare).
Pe o bază per client, asta e o consecință pe care contractul nu o numește: pragul
scris în F27-D3 e doar de timp, iar de timp suntem confortabil.

Ce o produce: cheia completă are `MaterialId`, cu 19.884 materiale distincte.
Măsurat, tot pe Flax:

| Cheia | Chei distincte |
|---|---|
| completă (cont + 8 dimensiuni) | 187.378 |
| fără `MaterialId` | 72.910 |
| `cont × repartitor` | 72.910 |
| conturi | 119 |

„Fără material" și „cont × repartitor" dau același număr fiindcă pe profilul
privat celelalte șase dimensiuni (cod funcțional, cod economic, sursă, unitate,
proiect, centru de cost) sunt `NULL` peste tot — sunt dimensiunile bugetare.
Pe o bază bugetară cifrele vor fi altele și trebuie remăsurate.

## B.4. Recomandarea pentru B

Cheia completă se PĂSTREAZĂ (D3 rămâne bun: rollup aditiv, pragul de timp e
respectat cu marjă). Trei ajustări de propus înainte de pasul 2, în ordinea
raportului beneficiu/risc:

1. **Omite cheile integral zero din snapshot.** Pe stoc taie 92,9 % din rânduri
   (110.786 → 7.914) fără nicio pierdere: un lot cu cantitate 0 și valoare 0
   contribuie 0 la orice rollup, iar „absent" și „zero" sunt același răspuns
   pentru `SolduriService`. Pe contabil câștigul e 1,4 % — se aplică totuși,
   pentru simetria regulii. Costul: `SolduriService` trebuie să trateze cheia
   absentă ca zero, ceea ce oricum trebuie să facă pentru cheile apărute după
   ultima închidere.
2. **Nu păstra un snapshot pentru fiecare perioadă închisă.** Păstrează ultima
   perioadă închisă + capetele de an (de care F27-D2/D7 au nevoie pentru
   arieratele la 31.12). Storage-ul scade de la ~27 M la ~3,6 M rânduri la 5 ani,
   iar ștergerea la redeschidere devine banală. Consumatorii din fereastra
   deschisă nu simt nimic: ei cer `snapshot(ultima închisă) + rulaje`. O
   interogare la o dată istorică între capete cade pe snapshot-ul de an + rulaje,
   nu pe `SUM` integral. **Asta cere reformularea invariantului F27-D3** din
   „snapshot ⇔ perioada e închisă" în „snapshot ⇔ perioadă de referință (ultima
   închisă sau capăt de an)" — e o modificare de contract, deci decizia
   lead-ului, nu a pasului 0.
3. **Indexul pe `Data` pentru rulaje**, în forma probată
   (`CREATE INDEX … ON "RegistruContabil" ("Data") WHERE "GCRecord" = 0`, la fel
   pe stoc): taie rulajele lunii de la 70,6 la 37,9 ms pe contabil (−46 %) și de
   la 16,2 la 14,1 ms pe stoc, schimbând planul din parallel seq scan în index
   scan. Nu ajută `SUM`-ul integral (rămâne seq scan, corect — citește tot).
   Intră ca migrație în pasul 2, nu în pasul 1.

Reconstrucția (`POST api/perioade/reconstruieste`) rămâne în buget: ~1,5 s per
perioadă la 5 ani × 60 de perioade ≈ 90 s pentru o comandă de administrator care
raportează diferențele. Dacă se adoptă (2), reconstrucția are de refăcut doar
capetele.

---

## Regula de oprire, verdict

- **A**: nu s-a atins. Există cel puțin două forme care funcționează fără să
  atingă `MotorOperare` (F1 și F2), ambele probate pe calea reală. **F27-r3 NU se
  activează** — cursa se rezolvă în felie. Singura întrebare rămasă e de citire a
  contractului, nu de tehnică: dacă „tranzacție explicită în afara
  ObjectSpace-ului" acoperă și tranzacția `DbContext`-ului propriu al
  ObjectSpace-ului. Dacă da, F4 literal (operarea pe F2) e gata de folosit.
- **B**: nu s-a atins. `SUM`-ul integral pe cheia completă = 0,30 s azi, ~1,5 s
  extrapolat la 5 ani, față de pragul de 30 s. **Cheia NU se reproiectează.**
  Dar mărimea snapshot-ului (~27 M rânduri la 5 ani în forma scrisă a lui D3) e o
  consecință neenumerată în contract și cere cele două ajustări de mai sus.

## Ce a contrazis harta sau contractul

1. **`ISaveChangesInterceptor` nu poate fi seam-ul blocării** — rulează înaintea
   deschiderii tranzacției, deci în autocommit. Varianta enumerată în spec ca
   alternativă pentru F2 e moartă; doar `IDbTransactionInterceptor` merge.
2. **`AutoTransactionBehavior.WhenNeeded` poate sări complet peste tranzacție**,
   deci un gardian montat pe tranzacție NU rulează la commit-urile mici. Nu apare
   nicăieri în hartă și e exact felul de premisă falsă care trece nedetectată.
3. **`OptimisticLockField` e concurrency token și pe `PerioadaFiscala`**, dar
   interceptorul care îl incrementează NU e înregistrat pe căile standalone
   (ModelCheck/Import1C/Migrare) — deci orice probă de concurență scrisă în
   ModelCheck trebuie să-l înregistreze explicit, altfel trece degeaba.
4. **Costul real al lui D3 nu e timpul, ci storage-ul** — contrar premisei
   „pragul e 30 s". Registrul e mai mic decât tabela care îl accelerează.
5. Working tree-ul principal era ocupat de pasul 1 în timpul spike-ului; măsurarea
   a cerut un worktree pinuit pe commit-ul contractului.
