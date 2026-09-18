# Felia 28 — pasul 0: spike-ul de mapare TPH (notă)

Data: 2026-09-18. Branch `p5-f28-tph`. Contract: `p5-felia28-tph-contract.md`.

**Stare: executat, închis.** FK-ul discriminator a căzut pe calea XAF (§3);
main-ul a RATIFICAT fallback-ul declarat în F28-D1: fără constraint FK și fără
navigație. Motivul: cheia alternativă cere `ClrType` nenul la `Add`, iar
`CreateObject` (seed, „New" din XAF, OData) face `Add` înaintea atribuirii;
placeholder-ul `string.Empty` ar muta problema (două rânduri noi în același
context se ciocnesc pe cheie, `''` se salvează fără verificare, cheia e
nefiltrată). Garanția „orice document are ancoră" o țin: valorile
discriminatorului sunt generate de EF din clasele concrete (nu se pot scrie
din cod, setter protejat), proba F28-D7 (a) în ModelCheck (1:1 clase concrete
↔ seed) și indexul unic filtrat `IX_TipuriDocument_ClrType`. Cititorul din
F28-D6 folosește forma fără navigație.

## 1. Deciziile main-ului aplicate (le scrie 089)

- **Coliziunile enum: varianta (A).** Gardianul compară TIPUL DE STOCARE
  (tipul providerului după value converter; la enum, tipul de bază) plus
  facetele (nullabilitate, lungime, precizie, scară, tip de coloană,
  unicode, lungime fixă). Amendamentul lui F28-D2: „tip CLR” devine „tip de
  stocare”; contractul nu e editat.
  - Motivul: pe `DocumentDetaliu`, `Directie` (`DirectieAsamblare` /
    `DirectieDiferenta`) și `Fel` (`FelLiniePif` / `FelLinieIesire`) sunt
    enumuri diferite peste `integer`. Frunza e citită doar prin tipul ei
    (F28-D2), deci coloana partajată nu amestecă semanticile.
  - Codul: `BackOfficeDbContext.cs:735-771` (`AplicaColoanePartajate`,
    `TipStocare`). Orice altă diferență rămâne excepție zgomotoasă, cu toate
    coliziunile listate într-un singur mesaj.
- **`TipDocument.ClrType` e read-only în XAF.** Are
  `[ModelDefault("AllowEdit","False")]` (`Politici.cs:31-32`).
- **Proba F28-D7 (a) sare tipurile abstracte** (notă pentru pasul 1). EF dă
  valori și lui `Document`, `DocumentTrezorerie` și `Repartitor`, dar aceste
  valori n-au rânduri și nici ancoră în seed.
- **De făcut la pasul 2** (cu gardianul F28-D4): refuzul de domeniu (422) pe
  ușa securizată pentru ștergerea unui `TipDocument` și pentru modificarea
  lui `ClrType`.

## 2. Maparea rezultată (`Migrations/20260918113542_InitialCreate.cs`)

### Coloanele

| Tabelă | Coloane | Așteptat | Tipul lui `ClrType` |
|---|---|---|---|
| `Documente` (`:1102`) | 37 | ~37 | `varchar(34)` NOT NULL |
| `DocumentDetalii` (`:1941`) | 45 | ~45 | `varchar(34)` NOT NULL |
| `Repartitori` (`:971`) | 28 | ~27 (+`ClrType`) | `varchar(21)` NOT NULL |

- `Documente`: ID, ClrType, Numar, Data, DataInregistrare, PredatorId, PrimitorId, Stare, DataOperare, DocumentSursaId, Autogenerat, CorecteazaId, MotivCorectie, TotalStingere, NumarPV, DataPV, TipInstrument, NumarExtras, DataExtras, LaturaPerecheId, DataScadenta, GestiuneDescarcareId, CodCpv, TethysId, Valuta, Curs, GenereazaPlata, PlataContPropriuId, PlataNumar, PlataData, PlataTipInstrument, GenereazaChitanta, ChitantaNumar, ChitantaData, Cauza, GCRecord, OptimisticLockField.
- `DocumentDetalii`: ID, ClrType, DocumentId, TipMaterialId, LotId, Cantitate, Valoare, TipTvaId, ValoareTva, AngajamentId, ImobilizareId, ValoareFiscala, ValoareDeductibila, Luni, ContDebitId, ContCreditId, RepartitorDebitId, RepartitorCreditId, CentruCostId, CodEconomicId, Directie, ProdusId, PretEvaluare, DataExpirare, LotFabricatie, Descriere, PretUnitar, LinieSursaId, SursaFinantareId, CodFunctionalId, ProiectId, CodCpv, Fel, AmortizareInitiala, AmortizareFiscalaInitiala, LuniAmortizateInitial, Metoda, DurataLuni, ValoareReziduala, MetodaFiscala, DurataFiscalaLuni, CategorieFiscala, UtilizareExclusiva, GCRecord, OptimisticLockField.
- `Repartitori`: ID, ClrType, Cod, Denumire, Calitati, Activ, ContImplicitId, Cautare, Marca, Iban, EsteBanca, CodFiscal, RegistruComert, TipPersoana, Tara, InregistratTva, TvaLaIncasare, TipTvaImplicitId, Strada, Numar, DetaliiAdresa, Localitate, CodPostal, JudetId, DataSincronizareAnaf, InactivFiscal, GCRecord, OptimisticLockField.

Niciun nume de coloană nu are prefix de tip. Lungimea discriminatorului o
pune EF singur, prin convenția lui de lungime. O clasă nouă cu nume mai lung
decât lungimea curentă a coloanei produce, automat, o migrație
`ALTER COLUMN`.

Când materializează polimorf, EF aliasează coloanele partajate de tipuri
diferite (`d1."Directie" AS "Directie0"`, `d1."Fel" AS "Fel0"`). E
comportamentul normal, fără efect asupra schemei.

### FK-urile și indexurile pe coloanele partajate: NU se dublează

EF produce UN constraint și UN index per coloană + principal, fără nicio
configurare de nume (bucla nu atinge FK-urile).

- **`DocumentDetalii`, 17 FK-uri**, câte unul pentru: AngajamentId,
  CodEconomicId, CodFunctionalId, ContCreditId, ContDebitId, LinieSursaId
  (→ DocumentDetalii, Restrict), DocumentId (Cascade), ImobilizareId
  (→ Imobilizari, Restrict), LotId, ProdusId, ProiectId, CentruCostId,
  RepartitorCreditId, RepartitorDebitId, SursaFinantareId, TipMaterialId,
  TipTvaId.
  - `ImobilizareId` e declarat pe trei frunze și produce totuși un singur
    `FK_DocumentDetalii_Imobilizari_ImobilizareId`.
  - Fiecare FK are un singur `IX_DocumentDetalii_{Col}`.
- **`Documente`, 7 FK-uri**: CorecteazaId, DocumentSursaId, LaturaPerecheId,
  GestiuneDescarcareId, PlataContPropriuId, PredatorId, PrimitorId.
- **`Repartitori`, 3 FK-uri**: ContImplicitId, JudetId, TipTvaImplicitId.
- **Indexurile pe discriminator**, create de bucla generică pe fiecare
  rădăcină TPH: `IX_Documente_ClrType`, `IX_DocumentDetalii_ClrType`,
  `IX_Repartitori_ClrType`.
- **Condiția pentru un singur constraint:** frunzele care împart un FK au
  același `OnDelete`. Azi îl au (cele trei `ImobilizareId` și cele două
  `LinieSursaId` sunt Restrict).

### Valorile discriminatorului vs seed-ul `TipDocument.ClrType`

Valoarea implicită EF e numele scurt al clasei. Seed-ul folosește
`nameof(Clasa)` (`ContaSeeder.cs:277-316`). Rezultatul: 20 din 20 de tipuri
concrete coincid, iar pe baza de spike 0 documente au `ClrType` fără ancoră.

| Tip CLR = discriminator = seed `ClrType` | Cod |
|---|---|
| FacturaIntrare / FacturaIesire / NIR / BonConsum / NotaTransfer | FCT / FCL / NIR / BCS / BTR |
| RaportProductie / ListaDiferenteInventar / Decont / Plata / Incasare | BPR / LDI / DEC / PLT / INC |
| DescarcareGestiune / NotaContabila / InchidereTva / Asamblare / ReturFurnizor | DSC / NTC / ITV / ASM / RLF |
| ReturClient / Dvi / PunereInFunctiune / IesireImobilizare / AmortizareLunara | RDC / DVI / PIF / CAS / AMO |

## 3. FK-ul discriminator → `TipDocument.ClrType`: fallback declarat (ABATERE de la decizia 2 a main-ului)

**Ce s-a întâmplat:**

- **Modelul trece.** `migrations add` și `database update` au trecut cu FK-ul
  și cu cheia alternativă `AK_TipuriDocument_ClrType`. Proba SQL a confirmat
  refuzul unui document fără ancoră (`ForeignKeyViolation
  "FK_Documente_TipuriDocument_ClrType"`).
- **Import1C a picat însă la seed**, înaintea oricărui document:

```
System.InvalidOperationException: Unable to track an entity of type 'TipDocument' because alternate key
property 'ClrType' is null. If the alternate key is not used in a relationship, then consider using a
unique index instead. Unique indexes may contain nulls, while alternate keys may not.
   at …NullableKeyIdentityMap`1.Add(InternalEntityEntry entry)
   at …DbContext.Add(Object entity)
   at DevExpress.ExpressApp.EFCore.EFCoreObjectSpace.CreateObjectCore(Type type)
   at …ContaSeeder.Aliniaza[T](…) in ContaSeeder.cs:line 119
   at …ContaSeeder.SeedTipuriDocument(IObjectSpace os) in ContaSeeder.cs:line 319
```

**Cauza:** `IObjectSpace.CreateObject` face `DbContext.Add` imediat, cu cheia
alternativă încă nulă; valoarea vine abia după. Același tipar îl folosesc
seed-ul, butonul „New” din XAF și OData. Deci, pe calea XAF, EF refuză
crearea oricărui `TipDocument` cât `ClrType` e o cheie alternativă fără
valoare la creare. Am aplicat fallback-ul declarat în F28-D1 (fără
constraint, fără navigație); garanția rămâne proba (a). Tot pe fallback am
repus indexul unic filtrat `IX_TipuriDocument_ClrType` (`:631`), care
redevine singura unicitate a ancorei motorului.

**Varianta cu FK, probată separat** pe o bază de unică folosință
(`Atlas.Conta.F28.SpikeFk`, creată și ștearsă). Codul are în plus un
inițializator `ClrType = string.Empty` pe `TipDocument`.

| Scenariu (EF + proxy-uri, ca în XAF) | Rezultat |
|---|---|
| `CreateProxy` + `Add`, apoi `ClrType` setat; de două ori la rând, apoi `SaveChanges` (tiparul seed-ului) | OK |
| două `TipDocument` noi în același context înainte de completare | `InvalidOperationException: … another instance with the same key value for {'ClrType'} is already being tracked` |
| modificarea `ClrType` pe un rând salvat | `InvalidOperationException: The property 'TipDocument.ClrType' is part of a key and so cannot be modified …` |
| salvare cu `ClrType` rămas `''` | OK (fără CHECK ar trece un rând fără sens) |

**Costurile variantei FK:**

- cheia alternativă e **nefiltrată**: re-seed-ul după o ștergere logică
  dă `duplicate key … "AK_TipuriDocument_ClrType"`, probat în prima rundă;
- un placeholder pe entitatea de politică, cu cele două eșecuri din tabel.

Ambele variante se reinstalează în două minute. Varianta FK (DbContext +
`Document.cs` cu navigația) e salvată în scratchpad-ul sesiunii
(`…/scratchpad/varianta-fk/`). Decide main-ul/owner-ul: fallback (arborele
de acum) sau FK + placeholder + CHECK `ClrType <> ''`.

**Ce face seed-ul cu `TipDocument`.** Ca orice politică, trece prin
`ContaSeeder.Aliniaza` (`ContaSeeder.cs:102-140`), cu cheia `Cod`:

- rândul viu `DinSeed` se aliniază; o cheie schimbată aruncă;
- rândul viu manual rămâne neatins;
- rândul găsit doar printre cele șterse (`IgnoreQueryFilters`) **NU se
  recreează** („ȘTERS de utilizator, nu se recreează”);
- rândul lipsă se creează.

Nicio cale de seed, Import1C sau ModelCheck (`Purja` nu atinge politicile)
nu șterge un `TipDocument`. Import1C `--recreeaza` aruncă baza întreagă.
Concluzia: pe FK, conflictul cu cheia alternativă nefiltrată ar apărea doar
dacă un utilizator creează din XAF un tip cu un `ClrType` deja ocupat de un
rând șters logic sub alt `Cod`.

## 4. Import1C pe o lună (baza de spike, schema de fallback)

`Import1C.exe "<EServicesFlx>" "<…Database=Atlas.Conta.F28.Spike>"
--recreeaza --pana-la 1`, rulat detașat (Start-Process). A durat
14:36:04 → 14:45:51, adică **9 min 47 s** în total. Din ele, deschiderea
(solduri + stoc, 11.454 loturi) ia ~2 min, iar luna 01/2025 7 min 43 s
pentru 15.000 de documente sursă (14.985 importate, 15 sărite).

Import1C s-a încheiat „fără eșecuri”. Pe baza de spike au rezultat:

| Documente | DocumentDetalii | Repartitori | RegistruTva |
|---|---|---|---|
| 16.597 | 27.376 | 3.055 | 7.242 |

Reconcilierea lunii are toate verificările OK. Secțiunea „Luna 01/2025” din
`reconciliere-20260918-143724.txt` e **identică pe conținut sortat** cu
aceeași secțiune din baseline-ul `reconciliere-20260917-121343.txt`
(diferă doar o linie goală de separator).

## 5. EXPLAIN și decizia pe indexul compus `(ClrType, DocumentId)`

SQL-ul real emis de EF, capturat prin `ToQueryString()` într-un harness
temporar (șters). `EXPLAIN (ANALYZE, BUFFERS)` după `ANALYZE`, a treia
rulare.

- **`RegistruTva` Server**: `RegistruTva` cu `Include` pe `Document`,
  `Detaliu`, `Partener`, `TipTva`, `OrderByDescending(Data)`, `Take(20)`.
  - Rezultatul: **4 JOIN-uri** (TPT avea 35). Planul face Top-N pe
    `RegistruTva`, apoi Nested Loop cu Index Scan pe `PK_Documente`,
    `PK_DocumentDetalii` și `PK_Repartitori`. Hash Join-ul peste
    `DocumentDetalii` a dispărut.
  - Execution Time: 1,38 ms pe 7.242 de rânduri. Nu e comparabil cu cele
    805 ms (bază și volum diferite); cifra A/B e la pasul 3.
- **`DraftInPerioada`**: `Documente` filtrat pe `Stare = 0` și pe
  `DataInregistrare` în lună, `NOT ID = ANY(@raportate)`, ordonat,
  `LIMIT 201`.
  - Rezultatul: Seq Scan pe `Documente` (toate cele 16.597 de rânduri sunt
    în lună, deci filtrul nu e selectiv), 1,0 ms. Nu atinge `DocumentDetalii`.
- **Liniile unei frunze pe document**:
  `FacturiIntrareDetalii.Where(DocumentId == x)` produce
  `WHERE "ClrType" = 'FacturaIntrareDetaliu' AND … "DocumentId" = …`.
  - Rezultatul: Index Scan pe `IX_DocumentDetalii_DocumentId` + filtru pe
    `ClrType`, 0,01–0,08 ms.
- **Liniile unei frunze pe lună**: `FacturiIesireDetalii` cu join pe
  `Document.DataInregistrare`.
  - Rezultatul: Bitmap Index Scan pe `IX_DocumentDetalii_ClrType`, apoi Hash
    Join, 3,4–3,7 ms.
- **A/B cu indexul compus** creat temporar: planificatorul **nu îl alege pe
  niciuna** dintre interogările de mai sus; planurile și timpii rămân aceiași.

**Decizia: indexul compus `(ClrType, DocumentId)` NU se pune.**
`IX_DocumentDetalii_DocumentId` e deja selectiv (câteva linii per document),
iar `IX_DocumentDetalii_ClrType` acoperă scanările pe tip. `InitialCreate`
rămâne cel generat.

## 6. Fișierele atinse

| Fișier | Ce |
|---|---|
| `nou/…/Module/BusinessObjects/BackOfficeDbContext.cs:209-222` | TPT → TPH pe cele trei ierarhii, `HasDiscriminator(x => x.ClrType)`, `DocumentTrezorerie` declarat explicit; comentariul „Documente (TPT)” tăiat |
| `…/BackOfficeDbContext.cs:551`, `:733-771` | `AplicaColoanePartajate`, chemat după `AplicaScaraNumerica`, ca facetele să fie finale: `SetColumnName(Name)` generic pe proprietățile declarate ale derivatelor, gardianul pe tipul de stocare plus facete, indexul pe discriminator |
| `…/BackOfficeDbContext.cs:625-631` | `AplicaUnicitatiPolitici`: neschimbat net (unicitatea filtrată pe `TipDocument.Cod` și pe `ClrType` rămâne) |
| `…/BusinessObjects/Documente/Document.cs:87-89` | `Document.ClrType` (setter protejat, `AllowEdit=False`, „Tip”) |
| `…/BusinessObjects/Documente/Document.cs:329-331` | `DocumentDetaliu.ClrType` |
| `…/BusinessObjects/Nomenclatoare/Repartitori.cs:16-18` | `Repartitor.ClrType` |
| `…/BusinessObjects/Politici/Politici.cs:31` | `TipDocument.ClrType` read-only în XAF |
| `…/Module/Migrations/` | cele 45 de migrații + snapshot-ul, șterse (`git rm`); `20260918113542_InitialCreate` nou, necomis |

Build: `dotnet build` pe `Atlas.Conta.BackOffice.slnx`, `tools/ModelCheck`,
`tools/Import1C`, `tools/BackfillTva`, `tools/Migrare` dă 0 erori și 0
avertismente. `dotnet ef migrations has-pending-model-changes` e curat.

## 7. Observații pentru pașii următori (neatinse)

- `tools/BackfillTva/Reconciliere.cs:259` citește
  `FindEntityType(tip).GetTableName()` și face `SELECT "ID" FROM "{tabela}"`.
  Sub TPH tabela e `Documente` pentru orice tip, deci interogarea întoarce
  TOATE documentele. Compilează, dar e greșită semantic până la pasul 2.
- `CorectieService.Copiaza` (`Motor/CorectieService.cs:138`) copiază și
  `ClrType`. Copia e inofensivă: aceeași clasă înseamnă aceeași valoare.
  Excluderea explicită rămâne pentru pasul 2.
- Comentariile „TPT” din `BackOfficeDbContext.cs` (`:105`, `:579`, cele din
  `AplicaColoanaCautare`/`AplicaScaraNumerica`) rămân pentru pasul 2
  (F28-D7).
- Hostul XAF nu a fost pornit; `ClrType` în layout se vede la
  `--dump-metadata` (pasul 1).
- `Atlas.Conta.F28.Spike` rămâne cu schema `InitialCreate` și luna 01/2025
  importată.
