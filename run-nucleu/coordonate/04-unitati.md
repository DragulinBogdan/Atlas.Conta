# 04 — Rapoartele pe UNITĂȚI (lot / partidă / fișă)

Inventar al datelor pe care implementarea curentă le CITEȘTE ca să producă
fiecare raport. Fără propuneri, fără comparație cu cubul.

Prescurtări de cale (toate sub `nou/Atlas.Conta.BackOffice/`):
`M/` = `Atlas.Conta.BackOffice.Module/`, `W/` = `Atlas.Conta.BackOffice.WebApi/`,
`C/` = `nou/Atlas.Conta.Client/src/`.

Cele patru registre: `M/BusinessObjects/Registre/Registre.cs`
(`RegistruStoc:21`, `RegistruContabil:43`, `RegistruTva:203`),
`M/BusinessObjects/Registre/RegistruImobilizari.cs:12`.
Snapshot-urile: `M/BusinessObjects/Registre/SolduriPerioada.cs`
(`SoldPerioadaContabil:13`, `SoldPerioadaStoc:62`, `PartidaDeschisa:88`).

---

## A. STOC

### A0. Ce NU există (constatare, nu judecată)

- **Fișa de magazie** (mișcările unui produs/lot în timp, cu sold curent după
  fiecare rând) **nu există ca raport**. Căutare pe tot repo-ul: niciun
  `FisaMagazie` / `FisaStoc` / rută `fisa-stoc`; în client, `C/felii/raportare/`
  are doar `Balanta.tsx`, `BalantaPlan.tsx`, `FisaCont.tsx`, `RegistruJurnal.tsx`,
  `SoldParteneri.tsx`, iar `C/felii/stoc/` are un singur ecran, `SoldStoc.tsx`.
  Singurul „istoric de lot" citibil de om e ListView-ul brut `RegistruStoc` din
  XAF (A3).
- Nu există raport de **cost mediu / cost unitar curent** al unei poziții de
  stoc: se afișează `Lot.PretUnitar` (prețul ÎNGHEȚAT la creare), nu
  `Valoare / Cantitate` al soldului.

### A1. Soldul de stoc

**1. Identitate.** „Sold stoc”, fără cod legal.
Proiecție: `M/Proiectii/StocProiectii.cs:55` (`StocProiectii.SoldStoc`),
rândul `SoldStocRand:20`. Sursa de atomi: `M/Motor/SolduriService.cs:271`
(`MiscariCumulate`). Ușa: REST `GET /api/proiectii/sold-stoc`,
`W/API/Conta/SoldStocController.cs:25`. Client: `C/felii/stoc/SoldStoc.tsx:14`.
Geamănul de motor, pe aceeași cheie: `M/Motor/StocService.cs:204` (`Sold`) și
`:57` (`SolduriLaData`).

**2. Granul rândului.** `Lot × Repartitor(gestiune) × TipStoc`
(`StocProiectii.cs:57`), adică exact cheia `CheieStoc`
(`M/Motor/StocService.cs:7`). Unitatea nominalizată E lotul; gestiunea și
TipStoc sunt coordonate ale lui, nu ale produsului.

**3. Tabelul coloanelor.**

| câmp | sursă | fel | fișier:linie |
|---|---|---|---|
| `LotId` | `RegistruStoc.LotId` / `SoldPerioadaStoc.LotId` | registru / snapshot | `StocProiectii.cs:71`, `SolduriService.cs:282,294` |
| `RepartitorId` | `RegistruStoc.RepartitorId` / idem snapshot | registru / snapshot | `StocProiectii.cs:72` |
| `TipStoc` | enum → string prin `CASE` în SQL | calcul | `StocProiectii.cs:78-84` |
| `ProdusId` | `Lot.ProdusId` | nomenclator | `StocProiectii.cs:85` |
| `ProdusCod` | `Lot.Produs.Cod` | nomenclator | `StocProiectii.cs:86` |
| `ProdusDenumire` | `Lot.Produs.Denumire` | nomenclator | `StocProiectii.cs:87` |
| `ProdusUM` | `Lot.Produs.UM` (text liber, nu FK-ul SAF-T) | nomenclator | `StocProiectii.cs:88` |
| `LotData` | `Lot.Data` | nomenclator | `StocProiectii.cs:89` |
| `LotPretUnitar` | `Lot.PretUnitar` — preț ÎNGHEȚAT la creare, NU `Valoare/Cantitate` | nomenclator | `StocProiectii.cs:90`; scris în `M/Motor/MotorOperare.cs:344` |
| `GestiuneDenumire` | `Repartitor.Denumire` | nomenclator | `StocProiectii.cs:91` |
| `Cantitate` | `SUM(RegistruStoc.Cantitate)` peste rândul sintetic + rulaje | registru + snapshot | `StocProiectii.cs:62` |
| `Valoare` | `SUM(RegistruStoc.Valoare)` idem | registru + snapshot | `StocProiectii.cs:63` |

**4. Filtrele și cheile de agregare.**
- Data reperului e **`RegistruStoc.Data` = `Document.DataInregistrare`**
  (scrisă la `MotorOperare.cs:354`; stornoul la `dataStorno`, `:650`). `Data`
  documentului fizic NU intră în registru.
- `laData` (parametru al proiecției, `SoldStocController.cs:25`) taie
  `r.Data <= panaLa` (`SolduriService.cs:275`).
- Sursa nu e registrul brut, ci **snapshot + rulaje**: dacă există o perioadă
  de referință (ultima închisă sau un decembrie închis,
  `SolduriService.cs:75` `Referinte`, `:207` `Referinta`), se citește
  `SoldPerioadaStoc` al ei ca UN rând sintetic datat la sfârșitul referinței,
  plus rândurile de registru de după (`SolduriService.cs:288-297`).
- **Nu filtrează `Storno`**: registrul e append-only, rândurile inverse se
  anulează algebric (`StocProiectii.cs:46-47`).
- **Nu filtrează pe `Document.Stare`**: rândurile de registru există doar pentru
  documentele operate.
- Se omit cheile cu `Cantitate == 0 ȘI Valoare == 0` (`StocProiectii.cs:65`);
  aceeași regulă la scrierea snapshot-ului (`SolduriService.cs:331` `HAVING`).
- Rândurile de DESCHIDERE ale migrării au `DocumentId IS NULL` și intră normal
  (`Registre.cs:34-35`, `SolduriService.cs:337`).

**5. Ce citește din DOCUMENT și nu din registru.** Nimic în raportul de sold.
DAR: `RegistruStoc.Data` E o copie a lui `Document.DataInregistrare`, iar
`Lot.Data` / `Lot.PretUnitar` sunt copii ale liniei de intrare
(`MotorOperare.cs:344-345`), deci „prețul lotului” afișat vine din nomenclator,
nu din registru.

**6. Cazuri speciale.**
- **Regula de golire (D18-D2)**, `M/Motor/StocService.cs:41` `ValoareGolire`,
  aplicată la `:103` `AplicaValoareIesire`: dacă o mișcare negativă duce cheia
  exact la 0, valoarea ieșirii = TOT soldul valoric de dinainte, nu
  `preț × cantitate`. Cenții cad pe contul de cheltuială. Decizia se ia **la
  momentul operării**, pe registrul văzut atunci ⇒ operarea retro lasă reziduu
  (limita F1, `StocService.cs:88-98`), declarat în SAF-T ca
  `ReziduValoricFaraCantitate`.
- Documentele cu `IDocumentCuIesireFiscala` (RLF) sunt **sărite** de regula de
  golire (`StocService.cs:105`): reziduul rămâne pe lot prin contract.
- Oracolul `VerificaGoliri` (`StocService.cs:166`) clasifică fiecare ieșire:
  `Exacta` / `CuValoare` / `Fiscala` / `ReDeschisaRetro` / `Negolita`, cu
  `Reziduu = Valoare − round(Cantitate × pretLot)` (`:178`).
- Gardianul de sold `VerificaSoldIntermediar` (`StocService.cs:216`) cere sold
  ≥ 0 la **sfârșitul fiecărei zile**, nu per mișcare — ordinea intra-zi nu e
  definită.
- Un membru nou de `TipStoc` fără rând în lanțul `CASE` ar apărea ca
  `ProductieNeterminata` (`StocProiectii.cs:84`).

### A2. Loturile (nomenclator + alocarea FIFO)

**1. Identitate.** `Lot`, `M/BusinessObjects/Nomenclatoare/ProdusLot.cs:79`.
Ușa: XAF `NavigationItem("Nomenclatoare")` (`:77`) + OData generic
`api/odata/Lot` (ruta comună, `W/Startup.cs:398`). Fără ecran React, fără
controller REST. Alocarea FIFO: `M/Motor/StocService.cs:259`
(`AlocaFifoTolerant`) și `:291` (`AlocaFifo`); consumatorul de descărcare:
`M/Motor/DescarcareService.cs:56` (`Genereaza`).

**2. Granul rândului.** Un lot = `Produs × Gestiune × Data × PretUnitar`,
creat de linia de intrare (NIR / plus de inventar / raport de producție).

**3. Tabelul coloanelor (ale unității).**

| câmp | sursă | fel | fișier:linie |
|---|---|---|---|
| `Eticheta` | `Produs.Denumire · Data · round(PretUnitar,4)` — calculată ȘI în C#, ȘI ca expresie XAF în SQL | calcul | `ProdusLot.cs:83,85-98` |
| `ProdusId` | coloană proprie | nomenclator | `ProdusLot.cs:101` |
| `PretUnitar` | `round6(linie.Valoare / linie.Cantitate)` la crearea lotului | document (înghețat) | `ProdusLot.cs:104`; scris în `MotorOperare.cs:344` |
| `GestiuneId` | coloană proprie | nomenclator | `ProdusLot.cs:105` |
| `Data` | `Document.DataInregistrare` al documentului de intrare | document (înghețat) | `ProdusLot.cs:107`; `MotorOperare.cs:345` |
| `DataExpirare`, `LotFabricatie` | `ILinieCuAtributeLot` de pe linia de intrare | document | `ProdusLot.cs:108-109`; `MotorOperare.cs:346-348` |
| `LinieIntrareId` | linia-mamă; **coloană FĂRĂ FK** (ciclu de inserție) | document | `ProdusLot.cs:115` (nota la `:110-114`) |

**4. Filtrele și cheile de agregare (FIFO).**
`AlocaFifoTolerant` (`StocService.cs:262-267`): `MiscariCumulate(os, data,
produsId)` filtrat pe `RepartitorId == gestiune` și `TipStoc`, grupat pe
`LotId`, păstrând `Sold > 0`; ordinea e `Lot.Data`, apoi `Lot.ID` ca tiebreak
(`:273`). `dejaAlocat` scade alocările necomise din aceeași generare (`:276`).
Varianta strictă aruncă la rest (`:295`).

**5. Ce citește din DOCUMENT și nu din registru.**
`Lot.PretUnitar`, `Lot.Data`, `Lot.DataExpirare`, `Lot.LotFabricatie`,
`Lot.LinieIntrareId` — toate copiate de pe linia de intrare la operare. Prețul
lotului **nu e reconstruibil** din registru: registrul poartă `Valoare`, nu
prețul, iar `Valoare` poate fi rescrisă de regula de golire.
La descărcare, `DescarcareService.Genereaza` citește `Lot.PretUnitar`
(`DescarcareService.cs:120-123`) ca să precompleteze `Valoare`
(`:141` — `round2(cantitate × pretUnitar)`).

**6. Cazuri speciale.**
- Ordinea de alocare din `DescarcareService.cs:89`: liniile cu **pin de lot**
  (`LotId != null`) înaintea celor doar-produs; pinul NU are fallback FIFO
  (`:96-105`).
- `RestNedescarcat` (`DescarcareService.cs:22`) numără acoperirea din DSC-urile
  copil ale ACELEIAȘI facturi (`DocumentSursaId == fcl.ID`), în stările Draft
  SAU Operat, excluzând Stornat (`:38-44`); liniile DSC manuale
  (`LinieSursa == null`) nu intră.
- Generatorul nu aruncă niciodată la stoc insuficient — restul e backorder
  (`DescarcareService.cs:11-13`).
- Ștergerea unui lot e refuzată dacă are rânduri de registru
  (`M/Motor/LoturiCulegereService.cs:291`).

### A3. Ce citește XAF pe `RegistruStoc` (ListView brut)

**1. Identitate.** `RegistruStoc_ListView`, generat de XAF, curatat în
`M/UI/ContaUiBaseline.cs:151-156`. `[ForbidCRUD]` pe ambele view-uri
(`Registre.cs:20`). Fără proiecție proprie: grila leagă direct entitatea.

**2. Granul rândului.** Un rând de registru = o mișcare (document × linie ×
cheie de stoc).

**3. Coloanele.** Toate coloanele scalare ale entității (`Data`, `TipStoc`,
`Cantitate`, `Valoare`, `Storno`) — `registru`. FK-urile sunt ascunse prin
convenție (`ContaUiBaseline.cs:141`), iar navigațiile `Lot`, `Document`,
`Detaliu` sunt **scoase explicit** (`:153-156`): `Lot` fiindcă afișarea
`Eticheta` ar citi `Produs` LAZY = N+1 pe 282k rânduri (nota `:142-150`);
`Document`/`Detaliu` fiindcă n-au `DefaultProperty` și ar ieși ca GUID.
`Repartitor` rămâne vizibil.

**4. Filtre.** Niciunul implicit; modul de acces e `ServerView` pe registre
(decizia 85). Paginarea e a grilei.

**5. Din document.** Nimic — coloanele navigaționale spre document sunt tăiate.

**6. Cazuri speciale.** Identitatea lotului NU se poate citi din această listă:
se citește pe documentul-sursă sau prin proiecția A1.

### A4. `SoldPerioadaStoc` (snapshot-ul de perioadă)

**1. Identitate.** `M/BusinessObjects/Registre/SolduriPerioada.cs:62`,
scris de `M/Motor/SolduriService.cs:321` (`ScrieStoc`), SQL brut pe Postgres.
Ușa: niciuna (nici REST, nici ecran) — e consumat de `MiscariCumulate` și de
comanda de reconstrucție (`SolduriService.cs:176` `Reconstruieste`).

**2. Granul.** `An × Luna × Lot × Repartitor × TipStoc`, cumulat de la
începutul bazei până la sfârșitul perioadei.

**3. Coloanele.** `An`, `Luna` (scalari, nu dată), `LotId`, `RepartitorId`,
`TipStoc`, `Cantitate`, `Valoare` — toate `snapshot` derivat din `registru`
(`SolduriPerioada.cs:62-83`).

**4. Filtre / agregare.** Scriere incrementală din `P−1` dacă există
(`SolduriService.cs:96`), altfel `SUM` integral peste `Data <= sfârșitul lui P`
(`:329`, `SursaStoc`). `GROUP BY LotId, RepartitorId, TipStoc` cu
`HAVING SUM(Cantitate) <> 0 OR SUM(Valoare) <> 0` (`:330-331`). Perioadele de
referință: ultima închisă + fiecare decembrie închis (`:75-89`).

**5. Din document.** Nimic.

**6. Cazuri speciale.** Ștergere FIZICĂ, nu `GCRecord` (`:147-151`). Cheile
integral zero se omit — taie ~93% din rânduri pe stoc (nota `:315-316`).

---

## B. IMOBILIZĂRI

Registrul e **al patrulea**, scris exclusiv prin `IDocumentCuRegistruPropriu`
de PIF / CAS / AMO (`M/BusinessObjects/Documente/Imobilizari.cs:187`, `:426`,
`:616`), chemat din `M/Motor/MotorOperare.cs:400-401`.

### B1. Fișa mijlocului fix

**1. Identitate.** `FisaImobilizareDto`,
`M/Api/Imo/ImobilizariDtos.cs:52`; producător `M/Api/Imo/ImobilizariApply.cs:12`
(`Fisa`); aritmetica `M/Motor/AmortizareService.cs:76` (`Situatie`).
Ușa: REST `GET /api/imobilizari/{id}/fisa?laData=`,
`W/API/Conta/ImobilizariController.cs:23`. Client:
`C/felii/imobilizari/ImobilizareDetaliu.tsx`, `PanouFisa.tsx`.

**2. Granul rândurilor.** Două secțiuni:
- antetul + `Situatie`: **o fișă** (unitate cu cantitate 1), la `laData`;
- `Randuri`: **un rând de `RegistruImobilizari`** (fișă × document × linie ×
  fel), ordonat `Data, Fel, ID` (`ImobilizariApply.cs:46`).

**3. Tabelul coloanelor — antetul fișei.**

| câmp | sursă | fel | fișier:linie |
|---|---|---|---|
| `NumarInventar`, `Denumire` | `Imobilizare.*` | nomenclator | `ImobilizariApply.cs:16` |
| `TipMaterialCod/Denumire` | `Imobilizare.TipMaterial.*` | nomenclator | `:18` |
| `ClasificareCod/Denumire` | `ClasificareImobilizari.*` (catalog HG 2139/2004) | politica | `:19` |
| `DurataFiscalaMinLuni/MaxLuni` | `Clasificare.DurataMinAni × 12` / `DurataMaxAni × 12` | politica + calcul | `:20-21`, `:41-42` |
| `LocDenumire`, `CentruCostDenumire`, `CodEconomicCod`, `ResponsabilNume` | navigații de pe fișă | nomenclator | `:22-23` |
| `Stare` | `Imobilizare.Stare` — scrisă DE MOTOR la operarea PIF/CAS | document | `:16`; scrisă în `Imobilizari.cs:212`, `:445` |
| `DataPunereInFunctiune` | `Imobilizare.DataPunereInFunctiune` = **`PunereInFunctiune.Data`** (data FIZICĂ, nu `DataInregistrare`) | document | `:17`; scrisă în `Imobilizari.cs:214` |
| `DataIesire` | idem, din `IesireImobilizare.Data` | document | `:17`; `Imobilizari.cs:446` |

**3b. Tabelul coloanelor — `Situatie` (cele trei cifre + parametrii).**

| câmp | sursă | fel | fișier:linie |
|---|---|---|---|
| `Valoare` (brut contabil) | `SUM(RegistruImobilizari.Valoare)` pe `Data <= laData` | registru | `AmortizareService.cs:86` |
| `ValoareFiscala` | `SUM(ValoareFiscala)` | registru | `:86` |
| `Amortizare` (**contabilă**) | `SUM(Amortizare)` | registru | `:87` |
| `AmortizareFiscala` (**fiscală**) | `SUM(AmortizareFiscala)` | registru | `:87` |
| `AmortizareDeductibila` (**deductibilă**) | `SUM(AmortizareDeductibila)` — **PERSISTATĂ pe rând**, nu recitită din politică la raportare | registru | `:88`; scrisă în `Imobilizari.cs:624` |
| `Luni` | `SUM(Luni)` | registru | `:88` |
| `NetContabil` | `Valoare − Amortizare` | calcul | `:26` |
| `NetFiscal` | `ValoareFiscala − AmortizareFiscala` | calcul | `:27` |
| `Metoda`, `DurataLuni`, `ValoareReziduala`, `MetodaFiscala`, `DurataFiscalaLuni`, `CategorieFiscala`, `UtilizareExclusiva` | **coalesce ÎNAPOI** peste rândurile-EVENIMENT (Intrare/Modernizare/Revizuire/Reevaluare) nestornate | registru + calcul | `:89-92`, `:114-119`, `:66-69` |
| `DataUltimEveniment` | `Data` a ultimului eveniment ≤ `laData` | registru | `:93` |

**3c. Tabelul coloanelor — un rând (`RandImobilizareDto:28`).**
`Data`, `Fel`, `Storno`, `Valoare`, `ValoareFiscala`, `Amortizare`,
`AmortizareFiscala`, `AmortizareDeductibila`, `Luni`, `Metoda`, `DurataLuni`,
`ValoareReziduala`, `MetodaFiscala`, `DurataFiscalaLuni`, `CategorieFiscala`,
`UtilizareExclusiva` — toate `registru` (`ImobilizariApply.cs:128-132`).
`DocumentNumar` = `Document.Numar`, `document` (`:133`).
`DocumentTip` = codul ancorei `TipDocument`, prin `ApiProiectii.CoduriTip`
(`:31`, `:50`) — `politica`/`nomenclator`.

**4. Filtrele și cheile de agregare.**
- Reperul registrului e `RegistruImobilizari.Data = Document.DataInregistrare`
  (`Imobilizari.cs:191`, `:431`, `:619`); stornoul la data stornării.
- `laData` taie `r.Data <= panaLa` (`ImobilizariApply.cs:126`,
  `AmortizareService.cs:98`).
- **Sumele NU filtrează `Storno`** (`AmortizareService.cs:86-88`): rândul invers
  se anulează algebric. **Rezolvarea PARAMETRILOR filtrează**: evenimentul
  stornat iese din coalesce prin `DetaliuId` (`:79-81`).
- Fișa nu are snapshot de perioadă: se citește integral din registru.

**5. Ce citește din DOCUMENT și nu din registru.**
- `Imobilizare.Stare`, `DataPunereInFunctiune`, `DataIesire` — scrise pe
  NOMENCLATOR de motor, din `Document.Data` (nu `DataInregistrare`)
  (`Imobilizari.cs:212-214`, `:445-446`). Nu sunt în niciun registru.
- `Document.Numar` și codul de tip al ancorei, pe fiecare rând
  (`ImobilizariApply.cs:133`, `:31`).
- Banda de durată fiscală vine din catalogul `ClasificareImobilizari`, legat pe
  fișă, nu din registru (`ImobilizariApply.cs:20-21`).

**6. Cazuri speciale.**
- **Rândul de AMO nu poartă `Valoare` / `ValoareFiscala`** — doar cele trei
  cifre de amortizare și `Luni` (`Imobilizari.cs:616-629`). Brutul e adus
  exclusiv de PIF.
- PIF-ul scrie `AmortizareDeductibila = AmortizareFiscalaInitiala`
  (`Imobilizari.cs:198`), iar pe `Revizuire` pune `ValoareFiscala = 0`
  (`:195`).
- CAS (ieșirea) scrie **un singur rând per fișă**, cu sumele NEGATE ale
  situației la data ieșirii (`Imobilizari.cs:429-439`) și `Luni = 0` — deci
  ieșirea duce fișa la zero pe toate cele cinci măsuri.
- `null` pe un parametru al unui eveniment înseamnă „neschimbat” (F26-D2,
  `RegistruImobilizari.cs:32`; coalesce la `AmortizareService.cs:114`).

### B2. Registrul imobilizărilor

**1. Identitate.** `RegistruImobilizariDto`, `M/Api/Imo/ImobilizariDtos.cs:93`;
producător `M/Api/Imo/ImobilizariApply.cs:65` (`Registru`). Ușa: REST
`GET /api/imobilizari/registru?laData=`,
`W/API/Conta/ImobilizariController.cs:36` — **raport întreg, fără
`loadOptions`**: totalurile sunt ale registrului, nu ale unei pagini (nota
`:32`). Client: `C/felii/imobilizari/RegistruImobilizari.tsx`.

**2. Granul rândului.** **O fișă pusă în funcțiune**, la `laData`
(`ImobilizariApply.cs:66-67`), ordonată pe `NumarInventar` ordinal (`:80`).

**3. Tabelul coloanelor (`RandRegistruImobilizariDto:76`).**

| câmp | sursă | fel | fișier:linie |
|---|---|---|---|
| `NumarInventar`, `Denumire` | `Imobilizare.*` | nomenclator | `ImobilizariApply.cs:69` |
| `TipMaterialCod` | `Imobilizare.TipMaterial.Cod` | nomenclator | `:70` |
| `LocDenumire` | `Imobilizare.Loc.Denumire` — locul CURENT de pe fișă, nu `RegistruImobilizari.RepartitorId` (locul la data faptului) | nomenclator | `:70` |
| `Stare` | `Imobilizare.Stare` | document | `:69`, `:85` |
| `Brut` | `Situatie.Valoare` | registru | `:86` |
| `BrutFiscal` | `Situatie.ValoareFiscala` | registru | `:86` |
| `AmortizareCumulata` | `Situatie.Amortizare` | registru | `:87` |
| `AmortizareFiscalaCumulata` | `Situatie.AmortizareFiscala` | registru | `:87` |
| `DeductibilCumulat` | `Situatie.AmortizareDeductibila` | registru | `:88` |
| `NetContabil` / `NetFiscal` | diferențe (`AmortizareService.cs:26-27`) | calcul | `:89` |
| `Luni` | `Situatie.Luni` | registru | `:90` |
| `Total*` (7 totaluri) | `Sum` peste liniile DTO, **în memorie** | calcul | `:97-103` |

**4. Filtrele și cheile de agregare.**
Populația = fișele cu `DataPunereInFunctiune != null` (`:67`) — fișele „Noi”
lipsesc, fișele IEȘITE RĂMÂN (cu zerouri, vezi B1 §6). O singură citire de
registru pentru toate fișele, apoi `Situatie` în memorie (`:74-77`).
Fără filtru pe `Storno`, ca la fișă.

**5. Ce citește din DOCUMENT și nu din registru.**
`Stare`, prin nomenclatorul fișei (scris de motor din document). `LocDenumire`
e locul CURENT de pe fișă, deși registrul poartă locul LA DATA FAPTULUI
(`RegistruImobilizari.cs:48-50`) — câmpul există în registru și nu e citit de
niciun raport.

**6. Cazuri speciale.**
- Totalurile se calculează în C#, peste lista completă — raportul nu e paginat.
- O fișă fără niciun rând primește `Situatie` peste listă goală (`:81`), deci
  zerouri; nu dispare din registru.
- `AmortizareService.Situatie` e apelată o dată per fișă, deci `Coalesce`
  rulează per fișă în memorie.

### B3. Planul de amortizare (previzualizarea + documentul AMO)

**1. Identitate.** Nu există un „plan de amortizare” multi-lunar (grafic pe
toată durata). Ce există:
- **previzualizarea lunii**: `PrevizualizareAmoDto`, produsă de
  `M/Api/Amo/AmoApply.cs:117`, peste `M/Motor/AmortizareService.cs:247`
  (`Previzualizeaza`) → `:261` (`Analizeaza`) → `:332` (`Calcul`).
  Ușa: `GET /api/amo/previzualizare?an=&luna=`, `W/API/Conta/AmoController.cs:51`.
- **documentul AMO**: `AmoReadDto` din `M/Api/Amo/AmoApply.cs:48`,
  `GET /api/amo/{id}` (`AmoController.cs:35`); lista, `AmoApply.cs:13`,
  `GET /api/amo` (`:25`).
Client: `C/felii/amo/AmoDetaliu.tsx`.

**2. Granul rândului.** `LinieAmortizare` = **fișă × lună**
(`AmortizareService.cs:50`, generate în bucla `:371-397`).

**3. Tabelul coloanelor (`LinieAmortizare` / `LinieAmoDto`).**

| câmp | sursă | fel | fișier:linie |
|---|---|---|---|
| `ImobilizareId`, `NumarInventar`, `Denumire` | `Imobilizare.*` | nomenclator | `AmortizareService.cs:343`, `:392` |
| `TipMaterialId` | `Imobilizare.TipMaterialId` | nomenclator | `:343` |
| `Contabil` | `Cifra(fiscal: false)` — cote lunare însumate iterativ | calcul peste registru | `:382`, `:423-456` |
| `Fiscal` | `Cifra(fiscal: true)` | calcul peste registru | `:383` |
| `Deductibil` | `Deductibil(fiscal, categorie, exclusiv, reguli, ultimaZi)` | calcul peste politica | `:390`, `:164-183` |
| `Luni` | `LuniDeRecuperat(…)` = datorate − acoperite, plafonat la durata rămasă | calcul peste registru | `:379`, `:403-415` |
| `ContCheltuialaId` / `ContAmortizareId` | `PoliticaAmortizare.ContCheltuialaAmortizareId` / `ContAmortizareId`, pe `TipMaterialId` | politica | `:355-360`, `:394-395` |
| `LocId` | `Imobilizare.LocId` | nomenclator | `:396` |
| `CentruCostId`, `CodEconomicId` | `Imobilizare.*` | nomenclator | `:396` |
| `Motiv` / `MotivEticheta` / `BlocantId` | verdictul gardienilor de cronologie | perioada + document | `AmoApply.cs:121-123`; `AmortizareService.cs:270-324` |
| `Total*` (3) | `Sum` peste linii, în memorie | calcul | `AmoApply.cs:126-128` |
| `Stale` (pe AMO Draft) | re-rulează previzualizarea și compară liniile | calcul | `AmoApply.cs:78-83` |

**4. Filtrele și cheile de agregare.**
- Populația de fișe: `DataPunereInFunctiune != null && < primaZi` și
  (`DataIesire == null || > ultimaZi`) — **după DATE, nu după `Stare`**
  (`AmortizareService.cs:339-341`, nota `:337-338`).
- Baza de calcul se citește la **sfârșitul lunii ultimului eveniment**
  (`Cifra`, `:425-426`), iar restul curent la `M−1` (`referinta`, `:373`).
  Excepție declarată: fișa fără eveniment la `M−1` dar cu rânduri în `M` (PIF
  întârziat) se citește la sfârșitul lui `M` (`:374-376`).
- Regulile de deductibilitate se filtrează pe valabilitate la `ultimaZi`
  (`:362-364`); câștigă regula cu `DeLa` maxim (`:173`), întâi `PlafonLunar`,
  apoi `Procent` (`:182`).
- Gardieni în ordine (`Analizeaza`): fișă fără politică → amortizare vie în
  lună → amortizare ulterioară (cronologie) → draft anterior → lună precedentă
  lipsă → perioadă închisă → fără fișe (`:270-324`).

**5. Ce citește din DOCUMENT și nu din registru.**
- `Imobilizare.DataPunereInFunctiune` / `DataIesire` — criteriul de
  ELIGIBILITATE al lunii; nu există în registru (`:340-341`, `:379`).
- Existența altor documente `AmortizareLunara` (vii, ulterioare, drafturi
  anterioare, operarea lunii precedente) — patru interogări pe
  `Document.Stare`/`Data` (`:273-307`).
- `AmoReadDto` citește liniile din `AmortizareLunaraDetaliu`, nu din registru
  (`AmoApply.cs:59-74`): `Valoare`/`ValoareFiscala`/`ValoareDeductibila`,
  conturile și `RepartitorDebitId`.

**6. Cazuri speciale.**
- Trei metode: `Liniara` (`:135`), `Degresiva` AD1 cu trecere la liniar
  (`:140-161`), `Accelerata` (50%/an în primul an, `:128`; baza se recitește la
  capătul fazei accelerate, `:429-433`).
- Cota lunii e **plafonată la restul curent** (`:132`); ultima lună ia restul
  (`Liniara` cu `LuniRamase <= 0`, `:136`; `Degresiva` la `:157-158`).
- `luni > 1` = RECUPERARE: cotele se însumează ITERATIV, cu pragurile avansate
  la fiecare pas (`:448-454`).
- Rezidualul contabil se scade din baza de amortizat; pe fiscal e 0 (`:437`).
- Linia cu `contabil == 0` iese **fără conturi** (`:394-395`).
- Prima fișă fără `PoliticaAmortizare` completă oprește generarea cu motiv
  (`:386-389`, `:270-271`).
- `genereaza` SCRIE ori de câte ori luna e liberă (`AmoApply.cs:144-155`).

### B4. Ce citește XAF pe `RegistruImobilizari`

ListView declarat integral în `M/UI/ContaUiBaseline.cs:844-859`: `Data`,
`Imobilizare`, `Fel`, `Valoare`, `ValoareFiscala`, `Amortizare`,
`AmortizareFiscala`, `AmortizareDeductibila`, `Luni`, `Repartitor`
(**locul la data faptului** — singurul loc din aplicație unde se vede),
`Storno`. `Document`/`Detaliu` scoase (fără `DefaultProperty`).
`[ForbidCRUD]` pe ambele view-uri (`RegistruImobilizari.cs:10`).

---

## C. PARTIDE / ÎMPERECHERI

Notă: **nu există un „registru de terți”**. Soldurile de terți se citesc din
`RegistruContabil` prin `ContabilProiectii.SoldParteneri`
(`W/API/Conta/SoldPartenerController.cs:42`) — acel raport e pe granul
`cont × 8 dimensiuni`, nu pe granul PARTIDĂ, și aparține lotului contabil.
Unitatea „partidă” din acest lot e **documentul-sursă**, nu partenerul.

### C0. Ce NU există

- **Diferențele de curs nu există**: `Imperechere`
  (`M/BusinessObjects/Documente/Trezorerie.cs:654`) n-are valută, curs sau
  valoare în valută; `PartidaDeschisa` (`SolduriPerioada.cs:88`) are doar
  `Rest` în lei. Singurele `Valuta`/`Curs` din model sunt pe antetul
  `FacturaIntrare` (`M/BusinessObjects/Documente/FacturaIntrare.cs:37-38`),
  informative — nicio reevaluare, niciun 665/765 nicăieri în `M/Motor/`.
- Nu există un raport de **vechime a partidelor** (aging pe benzi).

### C1. Documentele cu rest (partidele deschise ca raport)

**1. Identitate.** `DocumentCuRestRand`,
`M/Proiectii/ImperecheriProiectii.cs:27`; proiecția `:110` (`DocumenteCuRest`).
Ușa: REST `GET /api/proiectii/documente-cu-rest?contrapartidaId=&sens=&laData=`,
`W/API/Conta/DocumenteCuRestController.cs:30`.
Consumatori: panourile de stingere ale FCT/FCL/PLT/INC/DEC/NTC din client.

**2. Granul rândului.** **Un document operat** cu `Rest > 0` la `laData`
(`:248`). Unitatea = documentul-sursă; partenerul e atribut al ei.

**3. Tabelul coloanelor.**

| câmp | sursă | fel | fișier:linie |
|---|---|---|---|
| `DocumentId` | cheia antetului | document | `ImperecheriProiectii.cs:229` |
| `Tip` | **LITERAL per ramură** a uniunii („FCT”/„FCL”/„PLT”/„INC”/„DEC”/„RDC”) | calcul | `:120,126,142,150,156,166` |
| `Numar` | `Document.Numar` | document | `:120` etc. |
| `Data` | `Document.Data` (data FIZICĂ a hârtiei) | document | `:120` etc. |
| `ContrapartidaId` | `PredatorId` sau `PrimitorId`, **după tip** | document | `:122,128,144,152,158,168` |
| `ContrapartidaDenumire` | `Repartitor.Denumire` al laturii alese | nomenclator | idem |
| `Sens` | **LITERAL per ramură** (`Datorie`/`Creanta`); dublează hook-ul polimorf `Document.SensDeStins` | calcul | `:123` etc.; nota `:38-42` |
| `Total` | `Document.TotalStingere ?? 0` — **fapt SCRIS de motor la operare** | document | `:123`; scris în `M/Motor/MotorOperare.cs:396-397` |
| `Rest` | `restReferinta − Σ(imperecheri din fereastra deschisă)` | snapshot + calcul | `:221-223` |
| `Asignat` | `Total − Rest` (diferență, nu a treia agregare) | calcul | `:240` |

**4. Filtrele și cheile de agregare.**
- `Stare == Operat` pe fiecare ramură (`:118,124,139,147,154,164`).
- `laData` taie pe **`DataInregistrare`** (`:172`), în timp ce coloana afișată
  e `Data`.
- Restul se compune în trei feluri (`:221-222`): `PartidaDeschisa.Rest` al
  ultimei perioade de referință; sau `Total`, pentru documentul înregistrat
  DUPĂ referință; sau `0`, pentru documentul de dinainte absent din partide
  (era stins integral).
- Fereastra deschisă = `Imperechere.Data > sfârșitul referinței && <= laData`,
  unpivot pe AMBELE laturi, **algebric** (rândurile inverse intră cu semn)
  (`:93-104`, `:195-197`).
- Candidații: partidele referinței ∪ documentele de după ea ∪ documentele
  atinse de o stingere din fereastră (`:226-227`).
- Filtrul final `Rest > 0` (`:248`); `contrapartidaId` și `sens` se aplică pe
  antetul deja normalizat (`:173-181`).

**5. Ce citește din DOCUMENT și nu din registru.**
**Tot raportul.** Niciun registru nu intră: `Stare`, `Numar`, `Data`,
`DataInregistrare`, `PredatorId`/`PrimitorId` și `TotalStingere` sunt toate de
pe antetul documentului; restul vine din `PartidaDeschisa` (snapshot derivat
din documente + `Imperecheri`, nu din registre) și din tabela `Imperecheri`.
`TotalStingere` în sine e `round2(Σ LiniiCreanta.(Valoare + ValoareTva))`
(`MotorOperare.cs:396-397`), deci o sumă de LINII, nu de postări.

**6. Cazuri speciale.**
- **Uniune per tip concret**, nu un query pe `Document`: contrapartida e o
  latură diferită per tip (`:12-15`).
- Picioarele de **virament intern** sunt EXCLUSE din PLT/INC prin testul
  `!(Predator is ContPropriu && Primitor is ContPropriu)` (`:138-148`).
- **RDC** intră cu `TotalStingere` deja filtrat prin `LiniiCreanta`
  (`M/BusinessObjects/Documente/Retururi.cs:190-191`: doar liniile fără lot),
  deci brutul de venit, nu Σ tuturor liniilor.
- `Sens` e literal SQL care DUBLEAZĂ `SensDeStins` (funcție de tip,
  netraductibilă în SQL) — consistența se probează în ModelCheck (`:40-42`).
- Un `sens` necunoscut dă **400**, nu filtru tăcut
  (`DocumenteCuRestController.cs:45-49`).

### C2. `PartidaDeschisa` (snapshot-ul de partide)

**1. Identitate.** `M/BusinessObjects/Registre/SolduriPerioada.cs:88`, scris de
`M/Motor/SolduriService.cs:107` (`MaterializeazaPartide`), SQL brut.
**Fără ușă**: niciun controller, niciun ListView XAF, niciun ecran — consumat
de `ImperecheriProiectii` (`:202-204`), de `Reconstruieste`
(`SolduriService.cs:176`), de ModelCheck și de Import1C.

**2. Granul rândului.** `An × Luna × Document`, la o perioadă DE REFERINȚĂ.

**3. Coloanele.** `An`, `Luna`, `DocumentId`, `Rest`
(`SolduriPerioada.cs:88-101`).

**4. Filtrele și agregarea.** SQL la `SolduriService.cs:114-125`:
`Documente` cu `GCRecord = 0`, `Stare = Operat`, `TotalStingere IS NOT NULL`,
`DataInregistrare <= sfârșitul perioadei`, LEFT JOIN pe unpivot-ul
`Imperecheri` pe ambele laturi cu `Data <= sfârșit` (`:132-140`), păstrând
`TotalStingere − COALESCE(Asignat,0) <> 0`. **NU e incrementală** (nota
`:102-105`): restul e diferența a două cumulate.

**5. Din document.** Tot: `Stare`, `TotalStingere`, `DataInregistrare`.

**6. Cazuri speciale.** Rândurile cu rest zero se omit — partida închisă nu mai
e partidă (`SolduriPerioada.cs:86`). Include și resturile NEGATIVE
(criteriul e `<> 0`, nu `> 0`).

### C3. Panoul de stingeri al unui document

**1. Identitate.** `StingeriDto`, `M/Api/Trz/ImperechereDtos.cs:66`; producător
`M/Api/Trz/ImperechereApply.cs:83` (`Stingeri`). Cifrele vin din
`M/Motor/ImperechereService.cs:30` (`Total`), `:45` (`Asignat`), `:50`
(`Ramas`). Ușa: REST `GET /api/imperecheri/{documentId}/stingeri`,
`W/API/Conta/ImperecheriController.cs:96`.

**2. Granul rândurilor.** Antetul: un document. `Imperecheri`: **un rând
`Imperechere`** în care documentul apare pe ORICARE dintre cele două roluri.

**3. Tabelul coloanelor.**

| câmp | sursă | fel | fișier:linie |
|---|---|---|---|
| `Total` | `Document.TotalStingere` (fapt scris; aruncă dacă documentul e operat fără el) | document | `ImperechereService.cs:30-41` |
| `Asignat` | `Σ Imperechere.Suma` pe ambele coloane, algebric | calcul | `:45-48` |
| `Ramas` | `Total − Asignat` | calcul | `:50-51` |
| `SensCandidati` | `Document.SensDeStins(os).Opus()` — hook polimorf de TIP | calcul | `ImperechereApply.cs:127` |
| `Id`, `Suma`, `Data`, `Autogenerat`, `InverseazaId` | `Imperechere.*` | document (tabela de legături) | `ImperechereApply.cs:99` |
| `EsteStingator` | `CASE` în SQL pe rolul documentului interogat | calcul | `:100` |
| `CelalaltDocumentId` / `CelalaltNumar` | latura opusă, prin navigație | document | `:101-103` |
| `CelalaltTip` | cod de `TipDocument` prin `ApiProiectii.CoduriTip` | politica | `:119`, `:143` |
| `Desfacuta` | există un rând cu `InverseazaId == r.ID` | calcul | `:109-113` |
| `PerioadaDeschisa` | `GardianPerioada.VerificaDeschisa(Data)`, o rezolvare per lună atinsă | perioada | `:116-117`, `:151-158` |

**4. Filtre / ordonare.** `DocumentStingatorId == id || DocumentId == id`
(`:92`); ordonare CRONOLOGIC după `Data` a CELEILALTE părți, tiebreak `ID`
(`:96-97`).

**5. Ce citește din DOCUMENT și nu din registru.** Tot, din nou:
`TotalStingere`, `Numar`, `Data`, și `SensDeStins`, care e o metodă pe TIPUL
CLR al documentului, nu o coloană.

**6. Cazuri speciale.**
- Un document poate apărea pe AMBELE roluri (lanțul avans↔decont↔regularizare):
  panoul lui are rânduri cu `EsteStingator` diferit
  (`ImperechereDtos.cs:96-99`).
- `Total` aruncă `OperareException` pe un document operat fără `TotalStingere`
  scris — semnalul „re-operați documentul” (`ImperechereService.cs:38-40`).

### C4. Invarianții stingerii (ce se citește ca să se VALIDEZE o împerechere)

`M/Motor/ImperechereService.cs:189` (`ValideazaCreare`). Nu e raport, dar
citește exact datele pe care le-ar cere un raport de plafoane:
- `Document.CapacitateStingere(os)` — dicționar `contrapartidă → plafon per
  SENS`, hook polimorf de tip (`:219`);
- `Document.PoateFiStins(os)` (`:232`), `Document.SensDeStins(os)` (`:265`);
- laturile `PredatorId`/`PrimitorId` ale documentului stins (`:240-242`);
- `AsignatFataDe(stingator, contrapartidă, sens)` (`:327-362`) — materializează
  POLIMORF documentele stinse de acest stingător (`:340-343`) ca să le citească
  `SensDeStins`; un document care nu declară sens se scade din AMBELE jumătăți
  (`:354-355`, conservator prin construcție);
- `GardianPerioada.VerificaDeschisa(data)` (`:67`) — al șaselea apelant;
- `DataInregistrare` a ambelor documente: faptul de stingere nu le poate
  preceda (`:197-200`).

### C5. Ce citește XAF pe `Imperechere`

ListView declarat în `M/UI/ContaUiBaseline.cs:190-197`: `Data` (index 0,
sortată descrescător), `DocumentStingator`, `Document`, `Suma`, `Inverseaza`,
`Autogenerat`. FK-urile ascunse (`:187`). E **singurul loc din XAF unde
desfacerea (rândul invers) se citește** (nota `:188-189`). Comenzile
`Imperecheaza` / `Desface`: `M/Controllers/ImperechereController.cs:43`, `:56`.
