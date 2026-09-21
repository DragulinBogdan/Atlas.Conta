# Inventar — rapoartele CONTABILE (lot 01)

Toate căile relative la `nou/Atlas.Conta.BackOffice/`, prescurtate:
`M/` = `Atlas.Conta.BackOffice.Module/`, `W/` = `Atlas.Conta.BackOffice.WebApi/`.
Inventar pur: ce citește implementarea de azi, cu `fișier:linie`.

Acoperite: balanța plată, balanța pliată pe plan, fișa contului, registrul
jurnal, soldul pe cont × repartitor, documentele cu rest (singura „fișă de
partener" existentă), plus dimensiunile bugetare pe ieșirea lor.

---

## 0. Substratul comun al primelor cinci rapoarte

`RegistruContabil` (`M/BusinessObjects/Registre/Registre.cs:43-161`) e o tabelă
de PERECHI: `ContDebitId` / `ContCreditId` / `Valoare`, plus două seturi de câte
8 dimensiuni plate (`DimensiuniDebit_*` / `DimensiuniCredit_*`, coloane mapate
prin `[Column]`, `Registre.cs:61-125`), `Data`, `NumarNota`, `Storno`,
`DocumentId?`, `DetaliuId?`.

Unpivot-ul spre atomi (`ContabilProiectii.Atomi`,
`M/Proiectii/ContabilProiectii.cs:266-312`): un rând de registru → doi
`AtomContabil` prin `Concat`, fiecare purtând dimensiunile LATURII lui.
`AtomContabil` (`ContabilProiectii.cs:38-54`) are exact: `Data`, `ContId`,
`Debit`, `Credit` și cele 8 FK-uri de dimensiune. **Nu are `DocumentId`, nu are
`NumarNota`, nu are `Storno`** — balanța și soldurile nu pot atinge documentul.

`SolduriService.AtomiCumulati` (`M/Motor/SolduriService.cs:226-248`) înlocuiește
coada istoricului cu snapshot-ul ultimei perioade de referință: rândurile
`SoldPerioadaContabil` devin atomi datați la sfârșitul referinței
(`SolduriService.cs:233-246`), concatenați cu registrul de după
(`SolduriService.cs:247`). Referința = ultima lună închisă + fiecare decembrie
închis (`SolduriService.cs:75-89`), aleasă prin `Referinta(os, panaLa)`
(`SolduriService.cs:207-215`).

Ușa comună: REST, `ContaApiController` + `DataSourceLoader`. Nicio ieșire XAF
pentru aceste cinci rapoarte (XAF expune doar ListView-ul brut al registrului,
`Registre.cs:19-20`). Client React: `Atlas.Conta.Client/src/App.tsx:158-168`.

---

## 1. Balanța de verificare (plată)

### 1.1 Identitate
Balanța de verificare, fără cod legal. Proiecție
`ContabilProiectii.Balanta` (`M/Proiectii/ContabilProiectii.cs:320-458`), DTO
`BalantaRand` (`:83-101`), agregat intern `AgregatBalanta` (`:66-77`), ordine
`OrdineBalanta` (`:474-483`). Ușă REST `GET /api/proiectii/balanta`
(`W/API/Conta/BalantaController.cs:21-67`). Ecran
`Atlas.Conta.Client/src/felii/raportare/Balanta.tsx`.

### 1.2 Granul rândului
- mod sintetic (`analitic=false`): **cont** (`ContabilProiectii.cs:410`)
- mod analitic (`analitic=true`): **cont × repartitor** (`:379`)

Modul NU se deduce: netarea nu e aditivă, deci cele două chei dau cifre
diferite și amândouă corecte (`:354-360`).

### 1.3 Tabelul coloanelor

| câmp | sursă | felul sursei | fișier:linie |
|---|---|---|---|
| `ContId` | `RegistruContabil.ContDebitId` / `.ContCreditId` (unpivot) | registru | `ContabilProiectii.cs:280,295` |
| `ContSimbol` | `Conturi.Simbol`, LEFT JOIN pe rezultatul agregat | nomenclator | `:400,429` |
| `ContDenumire` | `Conturi.Denumire`, LEFT JOIN | nomenclator | `:401,430` |
| `RepartitorId` | `Dimensiuni{Debit,Credit}_RepartitorId` | registru | `:283,298` |
| `RepartitorDenumire` | `Repartitori.Denumire`, LEFT JOIN | nomenclator | `:403` (null constant în sintetic, `:431`) |
| `InitialDebit` | `SUM(CASE WHEN Data < dataStart THEN Valoare)` pe latura debit | registru + snapshot | `:383,413` |
| `InitialCredit` | idem, latura credit | registru + snapshot | `:384,414` |
| `RulajDebit` | `SUM(CASE WHEN Data >= dataStart THEN Valoare)` debit | registru | `:385,415` |
| `RulajCredit` | idem credit | registru | `:386,416` |
| `SoldInitialDebit` | `max(InitialDebit − InitialCredit, 0)` | calcul | `:441,451` |
| `SoldInitialCredit` | `max(InitialCredit − InitialDebit, 0)` | calcul | `:441,452` |
| `SoldFinalDebit` | `max(Init + Rulaj net, 0)` | calcul | `:442,455` |
| `SoldFinalCredit` | `max(−(Init + Rulaj net), 0)` | calcul | `:442,456` |

Partea de INIȚIAL vine din `SoldPerioadaContabil.Debit/Credit`
(`SolduriService.cs:236-237`) ori de câte ori există o perioadă de referință
care se încheie până la `dataStart − 1` (`ContabilProiectii.cs:331`) — deci
sursa aceleiași coloane e `snapshot` sau `registru` după starea lanțului de
perioade.

### 1.4 Filtrele și cheile de agregare
- **`dataStart` nu e filtru**: e granița dinăuntrul agregării care decide ce
  înseamnă „inițial" (`:316-319`, `:383-386`). `dataEnd` taie cumulul
  (`SolduriService.cs:229,247`).
- **Reperul de dată e `RegistruContabil.Data`**, nu `Document.Data` și nici
  `DataInregistrare` (`ContabilProiectii.cs:279,294`). Documentul nu se atinge.
- Cele 8 dimensiuni sunt filtre **pe atomi, înainte de `GROUP BY`**, fiecare pe
  latura lui (`:339-346`). Consecință scrisă în cod: sub filtru de dimensiune
  `Σ debit ≠ Σ credit` (`:336-338`).
- `loadOptions` (sortare / paginare / filtrare de grilă) rămâne DEASUPRA
  rezultatului agregat (`BalantaController.cs:63-66`).
- **Storno intră în sumă**, nu se filtrează: registrul e append-only și soldul e
  suma lui algebrică (`ContabilProiectii.cs:307-309`).
- Rândurile de deschidere (`DocumentId == null`) intră la fel (`:310-311`).
- Soft delete: via `HasQueryFilter` XAF pe calea LINQ; securitatea prin
  ObjectSpace-ul securizat (`BalantaController.cs:56`).
- Validare de cerere: `dataStart`/`dataEnd` obligatorii, 400 dacă lipsesc
  (`BalantaController.cs:46-54`).

### 1.5 Ce citește din DOCUMENT și nu din registru
**Nimic.** `AtomContabil` nu are `DocumentId` (`ContabilProiectii.cs:38-54`), iar
`Atomi` nu atinge navigația `Document` (`:310-311`). Balanța e integral
reconstruibilă din postări + snapshot + nomenclatorul de conturi.

### 1.6 Cazuri speciale
- **Join LEFT pe `Cont`, deliberat** (`:362-373`): `Cont` e ștergibil logic și
  poate fi invizibil de securitate; cu INNER JOIN linia dispărea din balanță și
  `Σ RulajDebit ≠ Σ RulajCredit` fără explicație. Contul nerezolvat iese cu
  simbol `null`.
- **Două interogări, nu una** (`:377-435`): în modul sintetic `RepartitorId`
  iese constant `null` ca simplă coloană, niciodată ca cheie de join — `NULL`
  fără tip în cheie e tipizat `text` de Postgres și join-ul pică (`:60-65`).
- **Ordine totală obligatorie** (`:460-473`): `ContSimbol, ContId` (+
  `RepartitorId` analitic), altfel `LIMIT/OFFSET` pe cheie ne-unică poate
  dubla sau pierde rânduri.
- Rândurile cu tot zero nu ajung în snapshot: `HAVING SUM(Debit) <> 0 OR
  SUM(Credit) <> 0` (`SolduriService.cs:317`), iar cheia absentă = zero.

---

## 2. Balanța pliată pe planul de conturi

### 2.1 Identitate
`ContabilProiectii.BalantaPlan` (`ContabilProiectii.cs:581-684`), DTO
`BalantaPlanRand` (`:131-158`). Ușă REST `GET /api/proiectii/balanta-plan`
(`W/API/Conta/BalantaController.cs:81-120`). Ecran
`Atlas.Conta.Client/src/felii/raportare/BalantaPlan.tsx`.

### 2.2 Granul rândului
**Nod al planului de conturi** (un cont, la orice nivel al arborelui), cu
cifrele proprii + ale tuturor descendenților. Fără mod analitic: cheia
`cont × repartitor` ar fi o a doua ierarhie (`:577-580`).

### 2.3 Tabelul coloanelor

| câmp | sursă | felul sursei | fișier:linie |
|---|---|---|---|
| `ContId` | frunza sau strămoșul ei | calcul | `:607,628` |
| `ContSimbol` / `ContDenumire` | `Conturi.Simbol` / `.Denumire` | nomenclator | `:596-598,608-609` |
| `ParinteId` | `Conturi.ParinteId`, păstrat DOAR dacă părintele e el însuși vizibil | nomenclator + calcul | `:614-615` |
| `Nivel` | adâncimea pe lanțul de părinți VIZIBILI | calcul | `:639-648` |
| `AreCopii` | calculat peste mulțimea PĂSTRATĂ după trunchiere | calcul | `:671-675` |
| `AreMiscareProprie` | contul apare ca frunză în balanța plată | calcul | `:622` |
| `InitialDebit/Credit`, `RulajDebit/Credit` | sumele BRUTE ale frunzelor, cumulate în sus pe arbore | calcul peste balanța plată | `:630-633` |
| `SoldInitial*`, `SoldFinal*` | netate la NIVELUL NODULUI, din brutele lui | calcul | `:651-658` |

Frunzele sunt chiar `Balanta(analitic: false)` (`:588-590`) — nicio a doua
agregare.

### 2.4 Filtrele și cheile de agregare
Aceiași parametri de proiecție ca balanța plată, aplicați tot pe atomi
(`:588-590`). `nivelMaxim` taie RÂNDURI, nu sume (`:660-666`); refuzat sub 1 cu
400 (`BalantaController.cs:110-111`). **Nu există `loadOptions`**: un arbore nu
se paginează (`BalantaController.cs:74-77`). Pliul se face în MEMORIE, pe tot
tabloul (`:566-569`).

### 2.5 Ce citește din DOCUMENT și nu din registru
Nimic (moștenit de la balanța plată).

### 2.6 Cazuri speciale
- **Regula centrală**: se cumulează BRUTELE în sus, se NETEAZĂ la fiecare nod,
  niciodată invers (`:556-561`). Deci raportul NU e obținibil prin gruparea
  balanței plate în client.
- **Gardă de ciclu** pe `Cont.Parinte` (navigație editabilă din UI): `HashSet`
  de vizitate la urcare (`:627-628`) și la măsurarea adâncimii (`:641-643`).
- Contul cu părinte invizibil devine rădăcină (`:610-615`) — fiecare frunză
  contribuie la EXACT o rădăcină.
- Cifrele unui nod NU sunt suma copiilor afișați când nodul are postări proprii
  pe un cont sumator; diferența e semnalată prin `AreMiscareProprie`
  (`:142-146`).

---

## 3. Fișa contului / Cartea mare

### 3.1 Identitate
`ContabilProiectii.FisaCont` (`ContabilProiectii.cs:764-960`), DTO
`FisaContRand` (`:177-201`), tip intern de materializare `FisaContSql`
(`:228-243`), gate `CaleaBrutaEchivalenta` (`:1000-1020`), ordine `OrdineFisa`
(`:1025-1029`). Ușă REST `GET /api/proiectii/fisa-cont`
(`W/API/Conta/FisaContController.cs:15-128`). Ecran
`Atlas.Conta.Client/src/felii/raportare/FisaCont.tsx`.

**Singurul loc din repo cu SQL BRUT** (`ContabilProiectii.cs:688`): soldul
curent per rând e o funcție de fereastră, inexprimabilă în LINQ.

### 3.2 Granul rândului
**Atom** = o latură a unui rând de registru pe contul cerut. Un rând cu același
cont pe ambele laturi produce, corect, două rânduri de fișă — cheia e perechea
(`Id`, `Sens`), niciodată `Id` singur (`:172-175`).

A doua granulă, într-o singură linie: **rândul sintetic de snapshot**, `Sens =
"S"`, `Id = Guid.Empty`, datat la sfârșitul perioadei de referință
(`:847-861`).

### 3.3 Tabelul coloanelor

| câmp | sursă | felul sursei | fișier:linie |
|---|---|---|---|
| `Id` | `RegistruContabil."ID"` (zero pe rândul sintetic) | registru | `:815,849` |
| `Data` | `RegistruContabil."Data"` (sfârșitul referinței pe cel sintetic) | registru / snapshot | `:816,850` |
| `NumarNota` | `RegistruContabil."NumarNota"` | registru | `:817,851` |
| `Sens` | literal `'D'` / `'C'` după latura pe care stă contul; `'S'` sintetic | calcul | `:808-809,818,852` |
| `Debit` | `Valoare` pe latura D, altfel 0; `SUM(s."Debit")` sintetic | registru / snapshot | `:808-809,853` |
| `Credit` | simetric | registru / snapshot | `:808-809,854` |
| `SoldCurent` | `SUM(Debit − Credit) OVER (ORDER BY Data, Id, Sens DESC ROWS UNBOUNDED PRECEDING)` | calcul (fereastră SQL) | `:908-911` |
| `ContrapartidaId` | contul OPUS de pe același rând de registru | registru | `:821` |
| `ContrapartidaSimbol` | `Conturi.Simbol`, LEFT JOIN | nomenclator | `:899,917` |
| `RepartitorDenumire` | `Repartitori.Denumire` pe dimensiunea laturii, LEFT JOIN | nomenclator | `:900,918` |
| `DocumentId` | `RegistruContabil."DocumentId"` (nullable) | registru | `:822` |
| `DocumentTip` | **constant NULL din SQL**, completat în memorie peste pagină din `Document.ClrType` → `TipDocument.Cod` | document + politică | `:902,884-888`, `:1094-1102`, `M/Api/CititorTipDocument.cs:13-43` |
| `DocumentNumar` | **`Documente."Numar"`**, LEFT JOIN | document | `:903,919` |
| `Storno` | `RegistruContabil."Storno"` | registru | `:823` |

Cele 8 dimensiuni sunt proiectate cu nume UNIFORME pe ambele ramuri ale uniunii
(prefixul laturii dispare), ca filtrul să se scrie o dată (`:780-797`).

### 3.4 Filtrele și cheile de agregare
Trei niveluri, fiecare cu rolul lui (`:724-737`):
- (a) unpivot pe UN cont, tăiat la `Data <= dataEnd` și, când există referință,
  la `Data > sfârșitul ei` (`:825,827-828`);
- (b) filtrele de dimensiune, **înainte de fereastră** (`:864-882,915`) — „fișa
  pe proiectul X" are și soldul inițial al proiectului X;
- (c) `Data >= dataStart` + etichetele, în exterior (`:920`) — rândurile
  anterioare contribuie la sold fără să se afișeze.

Reperul de dată e `RegistruContabil.Data`. Nicio agregare în afara ferestrei.
`Storno` nu filtrează. Sortarea și gruparea din grilă sunt REFUZATE pe server
(`FisaContController.cs:68-69`) — soldul curent are sens doar în ordinea
cumulării; filtrarea rămâne permisă, deliberat (`FisaContController.cs:76-80`).

### 3.5 Ce citește din DOCUMENT și nu din registru
1. **`DocumentNumar`** — `LEFT JOIN "Documente" doc ON doc."ID" = f."DocumentId"`
   (`ContabilProiectii.cs:919`, proiectat `:903`).
2. **`DocumentTip`** — nu e coloană deloc: se rezolvă post-materializare, pe
   pagină, din `Document.ClrType` și `TipDocument.Cod`
   (`ContabilProiectii.cs:1094-1102`; `M/Api/CititorTipDocument.cs:17-21,33-37`).
   Consecință scrisă în cod: filtrarea și sortarea de grilă pe `DocumentTip`
   „văd" tot null (`:884-888`, `FisaContController.cs:122-125`).

### 3.6 Cazuri speciale
- **Soft delete scris de mână**: `"GCRecord" = 0` explicit pe registru ȘI pe
  fiecare tabelă alăturată (`:825,859,917,918,919`) — calea LINQ ar fi adăugat-o
  singură (`:698-703`).
- **Gate fail-closed în două bucăți, obligatoriu** (`:962-999`,
  `FisaContController.cs:108-115`): contul se rezolvă prin ObjectSpace SECURIZAT
  (invizibil ⇒ 404), apoi `CaleaBrutaEchivalenta` numără rândurile contului pe
  calea securizată și pe cea brută (`:1001-1019`); diferă ⇒ 403. `SqlQuery` nu
  trece prin `SecurityQueryCompiler`, iar premisa „registrul nu e filtrat" s-a
  dovedit FALSĂ la probă (`:709-722`).
- **Ordinea e cuplată în trei locuri** — fereastra SQL, `OrderBy`-ul LINQ și
  `OrdineFisa()` (`:909,959,1025-1029`): `DataSourceLoader` șterge `OrderBy`-ul,
  deci singura care ajunge sub `LIMIT/OFFSET` e ultima (`:744-754`).
- **Santinela `repartitorNul`** (`:756-763`, `:844-845,874-875`): a treia valoare
  a filtrului, fiindcă „fără repartitor" e o cheie de grupare legitimă și
  majoritară pe baza de import; combinată cu `repartitorId` ⇒ 400
  (`FisaContController.cs:54-56`).
- Al doilea tip (`FisaContSql`) există fiindcă `SqlQuery<T>` înregistrează tipul
  ca entitate ad-hoc sub `UseChangeTrackingProxies`: fără proiecția spre POCO
  sigilat, pe sârmă pleca proxy-ul XAF cu `ObjectSpace`, `LazyLoader` etc.
  (`:203-227,930-948`).
- Rândul sintetic poartă exact coordonatele filtrului, ca să treacă neschimbat
  prin nivelul (b) (`:832-843`).

---

## 4. Registrul jurnal

### 4.1 Identitate
`ContabilProiectii.RegistruJurnal` (`ContabilProiectii.cs:1040-1077`), DTO
`JurnalRand` (`:248-261`), ordine `OrdineJurnal` (`:1082-1085`). Ușă REST
`GET /api/proiectii/registru-jurnal` (`W/API/Conta/RegistruJurnalController.cs`).
Ecran `Atlas.Conta.Client/src/felii/raportare/RegistruJurnal.tsx`,
rută `/jurnal` (`App.tsx:168`).

### 4.2 Granul rândului
**Rândul BRUT al registrului** (nota cu debit ↔ credit pe aceeași linie), nu
atomul: unpivotat, fiecare notă ar apărea de două ori (`:1032-1033`).

### 4.3 Tabelul coloanelor

| câmp | sursă | felul sursei | fișier:linie |
|---|---|---|---|
| `Id` | `RegistruContabil.ID` | registru | `:1052` |
| `Data` | `RegistruContabil.Data` | registru | `:1053` |
| `NumarNota` | `RegistruContabil.NumarNota` | registru | `:1054` |
| `ContDebitId` | `RegistruContabil.ContDebitId` | registru | `:1055` |
| `ContDebitSimbol` | `Cont.Simbol` prin navigație (LEFT JOIN EF) | nomenclator | `:1056` |
| `ContCreditId` / `ContCreditSimbol` | idem, latura credit | registru / nomenclator | `:1057-1058` |
| `Valoare` | `RegistruContabil.Valoare` | registru | `:1059` |
| `DocumentId` | `RegistruContabil.DocumentId` (nullable) | registru | `:1060` |
| `DocumentTip` | NULL în proiecție, completat peste pagină | document + politică | `:1062`, `:1094-1102` |
| `DocumentNumar` | **`Document.Numar`** prin navigație nullable | document | `:1065` |
| `Storno` | `RegistruContabil.Storno` | registru | `:1066` |

### 4.4 Filtrele și cheile de agregare
**Nicio agregare.** `dataStart`/`dataEnd` sunt filtre SIMPLE și opționale — o
listare cronologică n-are noțiune de „inițial" (`:1037-1039,1047-1048`).
Reperul e `RegistruContabil.Data`. `Storno` nu filtrează. Sortarea din grilă e
PERMISĂ (n-are sold curent de rupt), `OrdineJurnal()` e doar un default
(`:1079-1081`). Calea e LINQ obișnuit, deci soft delete și securitate rămân
automate (`:1034-1035`). `IgnoreAutoIncludes()` taie cele 16 navigații de
dimensiuni puse de `BackOfficeDbContext` (`:1043-1046`).

### 4.5 Ce citește din DOCUMENT și nu din registru
1. **`DocumentNumar`** = `r.Document.Numar` (`:1065`) — navigație nullable,
   LEFT JOIN, `null` pe rândurile de deschidere.
2. **`DocumentTip`** — post-materializare, din `Document.ClrType` +
   `TipDocument.Cod` (`:1062`, `RegistruJurnalController.cs:41`).

### 4.6 Cazuri speciale
- Ordinea implicită NU supraviețuiește paginării: `DataSourceLoader` adaugă
  `OrderBy` (nu `ThenBy`) și fără `sort=` ar ordona după `Id` singur, adică după
  ordinea de INSERARE, care pe rânduri retroactive nu e cea cronologică
  (`:1068-1075`).
- Validarea se rezumă la `dataStart <= dataEnd` (`RegistruJurnalController.cs:29-30`).
- Rândurile de deschidere (`DocumentId == null`) apar cu tip și număr goale.

---

## 5. Soldul pe cont × repartitor („sold de partener")

### 5.1 Identitate
`ContabilProiectii.SoldParteneri` (`ContabilProiectii.cs:490-538`), DTO
`SoldPartenerRand` (`:107-120`), ordine `OrdineSoldParteneri` (`:542-546`).
Ușă REST `GET /api/proiectii/sold-parteneri`
(`W/API/Conta/SoldPartenerController.cs`). Ecran
`Atlas.Conta.Client/src/felii/raportare/SoldParteneri.tsx`, rută
`/sold-parteneri`.

**`ParteneriController.cs` nu conține nimic de raportare**: are doar comenzile
de sincronizare ANAF (`W/API/Conta/ParteneriController.cs:60,105`), iar citirea
partenerilor trece prin OData (`:10-17`). Nu există fișă de partener acolo.

### 5.2 Granul rândului
**cont × repartitor**, la o dată (`:508`). Fără noțiune de perioadă: e partea de
SOLD a balanței analitice (`:485-489`).

### 5.3 Tabelul coloanelor

| câmp | sursă | felul sursei | fișier:linie |
|---|---|---|---|
| `ContId` | `RegistruContabil.Cont{Debit,Credit}Id` prin atomi | registru | `:510` |
| `ContSimbol` / `ContDenumire` | `Conturi`, LEFT JOIN | nomenclator | `:528-529` |
| `RepartitorId` | `Dimensiuni{Debit,Credit}_RepartitorId` | registru | `:511` |
| `RepartitorDenumire` | `Repartitori.Denumire`, LEFT JOIN | nomenclator | `:531` |
| `Debit` | `SUM(Debit)` cumulat până la `laData` | registru + snapshot | `:512` |
| `Credit` | `SUM(Credit)` cumulat | registru + snapshot | `:513` |
| `SoldDebitor` | `max(Debit − Credit, 0)` | calcul | `:525,534` |
| `SoldCreditor` | `max(Credit − Debit, 0)` | calcul | `:525,535` |

### 5.4 Filtrele și cheile de agregare
`laData` e granița cumulului, nu filtru de grilă (`:496`,
`SoldPartenerController.cs:13-16`); default onest „azi"
(`SoldPartenerController.cs:43`), dar `default(DateOnly)` explicit ⇒ 400
(`:37-39`). `contId` + cele 8 dimensiuni se aplică pe atomi, înainte de
`GROUP BY` (`:497-505`). Reperul de dată e `RegistruContabil.Data`.
**Rândurile cu sold net zero se omit** (`:537`).

### 5.5 Ce citește din DOCUMENT și nu din registru
**Nimic.**

### 5.6 Cazuri speciale
- **`RepartitorDenumire` NU e partenerul contului de terț**: e dimensiunea
  LATURII rândului de registru — scris explicit în ecran
  (`SoldParteneri.tsx:107-109`). Restul pe partener se citește din alt raport
  (§6).
- Rândul „fără repartitor" (`RepartitorId == null`) e legitim, păstrat de LEFT
  JOIN (`:516-517`); drill-down-ul spre fișă îi pune santinela `repartitorNul`
  (`SoldParteneri.tsx:78-81`).
- Ordine totală pe `ContSimbol, ContId, RepartitorId` (`:540-546`).
- Coincide la cent cu modul analitic al balanței: aceiași atomi cumulați
  (`:103-106`).

---

## 6. Documentele cu rest (singura „fișă de partener" existentă)

Inclus fiindcă e suprafața pe care ecranul de solduri o numește explicit drept
locul restului PER PARTENER (`SoldParteneri.tsx:108-109`). Proiecția e în
`ImperecheriProiectii`, deci la granița lotului — inventar sumar.

### 6.1 Identitate
`ImperecheriProiectii.DocumenteCuRest`
(`M/Proiectii/ImperecheriProiectii.cs:110-225`), DTO `DocumentCuRestRand`
(`:27-48`), antet intern `AntetCuRest` (`:59-69`). Ușă REST
`GET /api/proiectii/documente-cu-rest` (`W/API/Conta/DocumenteCuRestController.cs:22-55`).

### 6.2 Granul rândului
**document** (un document operat cu rest nenul), nu cont × partener.

### 6.3 Tabelul coloanelor

| câmp | sursă | felul sursei | fișier:linie |
|---|---|---|---|
| `DocumentId` | `Document.ID` | document | `:120,126,142,150,156,166` |
| `Tip` | literal per ramură a uniunii („FCT", „FCL", „PLT", „INC", „DEC", „RDC") | calcul | `:120,126,142,150,156,166` |
| `Numar` | **`Document.Numar`** | document | `:120,126,…` |
| `Data` | **`Document.Data`** | document | `:120,126,…` |
| `ContrapartidaId` / `ContrapartidaDenumire` | **`Document.PredatorId`/`PrimitorId`** + `Repartitor.Denumire`, după tip | document + nomenclator | `:122,128,144,152,158,168` |
| `Sens` | Datorie/Creanță, constantă per tip de document | calcul | `:123,129,145,153,159,169` |
| `Total` | **`Document.TotalStingere ?? 0`** | document | `:123,129,…` |
| `Asignat` | `Σ Imperechere.Suma` pe ambele roluri | document (legături) | `:93-104,195-196` |
| `Rest` | `restReferinta − Σ asignări din fereastră` | snapshot + calcul | `:202-204,215-223` |

### 6.4 Filtrele și cheile de agregare
`Stare == Operat` pe fiecare ramură (`:118,124,139,147,154,164`);
`DataInregistrare <= laData` (`:171-172`); `ContrapartidaId`; `Sens`.
Baza de pornire e `PartidaDeschisa.Rest` al ultimei perioade de referință
(`:202-204`), peste care se aplică doar împerecherile de după (`:195`).
`Plata`/`Incasare` între două conturi proprii se exclud (`:140,148`).

### 6.5 Ce citește din DOCUMENT și nu din registru
**TOT.** Raportul nu atinge niciun registru: antetul documentului
(`Numar`, `Data`, `DataInregistrare`, `Stare`, `TotalStingere`,
`PredatorId`/`PrimitorId`) plus tabela de `Imperecheri`. Singura contribuție
materializată e `PartidaDeschisa.Rest`, ea însăși derivată din documente
(`M/Motor/SolduriService.cs:114-125`).

### 6.6 Cazuri speciale
- Uniunea e scrisă **tip cu tip**, cu sensul și latura de contrapartidă
  hardcodate per tip — singurul raport contabil al lotului cu vocabular pe tipuri
  concrete.
- `PartidaDeschisa` se scrie cu `TotalStingere − Σ Imperechere.Suma` filtrat pe
  `Imperechere.Data`, iar antetul pe `Document.DataInregistrare`
  (`SolduriService.cs:123,135,138`) — două repere de dată în aceeași cifră.
- Rândurile cu rest zero se omit din snapshot (`SolduriService.cs:124`).

---

## 7. Dimensiunile bugetare pe ieșirea rapoartelor contabile

Nomenclatoarele: `CodFunctional`, `CodEconomic`, `SursaFinantare`, `Proiect`
derivă din `Dimensiune` (cod + denumire + coloana generată `Cautare`,
`M/BusinessObjects/Nomenclatoare/DimensiuniBugetare.cs:13-34`); `Unitate` e clasă
proprie cu aceeași formă (`:38-41`); `Repartitor`, `Produs` (material) și
`Repartitor` (centru de cost) sunt nomenclatoare de sine stătătoare.

Rezolvarea e un coalesce generic pe componente, la SCRIERE, nu la citire —
ordinea canonică per latură: linie → override(latură) → comun(regulă) → default
de antet (`M/Motor/DimensiuniResolver.cs:56-72`). Rezultatul e ÎNGHEȚAT pe rândul
de registru în cele 16 coloane plate (`Registre.cs:61-125`), citit și scris prin
`Dimensiuni{Debit,Credit}()` / `AplicaDimensiuni*` (`Registre.cs:130-153`).

**Cum apar în IEȘIREA rapoartelor din lot:**

| raport | dimensiunile ca FILTRU | dimensiunile ca COLOANĂ de ieșire |
|---|---|---|
| balanță plată | toate 8, pe atomi, înainte de `GROUP BY` (`ContabilProiectii.cs:339-346`) | numai `RepartitorId` + `RepartitorDenumire`, și doar în modul analitic (`:402-403`) |
| balanță pe plan | toate 8, moștenite (`:588-590`) | niciuna |
| fișa contului | toate 8, înainte de fereastră (`:864-882`) | numai `RepartitorDenumire` (`:900`) |
| registru jurnal | **niciuna** (`:1040-1048`) | niciuna |
| sold pe repartitor | toate 8 (`:497-505`) | numai `RepartitorId` + `RepartitorDenumire` (`:530-531`) |
| documente cu rest | niciuna | niciuna |

Consecința structurală: **după `GROUP BY` coloana de dimensiune nu mai există**
(`ContabilProiectii.cs:317-319`) — „balanța pe proiectul X" înseamnă „însumează
doar mișcările proiectului X", nu „arată coloana Proiect". Cele 7 dimensiuni
non-repartitor nu sunt niciodată vizibile pe un rând de raport contabil, doar
selectabile. Snapshot-ul le păstrează pe toate 8 în cheie
(`M/BusinessObjects/Registre/SolduriPerioada.cs:26-49`,
`SolduriService.cs:60-63`), deci rollup-ul rămâne aditiv peste ele.

Clientul le transportă prin URL, ca set închis
(`Atlas.Conta.Client/src/nucleu/urlStare.ts:90-104`), și le propagă la
drill-down (`Balanta.tsx:89`, `BalantaPlan.tsx:128`, `SoldParteneri.tsx:74`).
