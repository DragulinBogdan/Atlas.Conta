# Pasul 5, felia 13 — motor/structură post-D300 (contract)

Data fixării: 2026-08-25. Șablonul feliilor F2–F12 (`p5-felia-*-contract.md`).
Deciziile F13-D1…F13-D6 sunt **PIN-UITE** — agenții de implementare nu le
redeschid; orice nepotrivire cu realitatea codului se RAPORTEAZĂ, nu se
normalizează tăcut.

## Scop

Felia D300 (decizia 69) a lăsat trei restanțe de MOTOR/STRUCTURĂ, nu de
raportare: **69-r4** (`TvaService` calculează TVA pe taxare inversă și pe
LIVRARE), **69-r7** (ștergerea amânată nu e exersată în ModelCheck),
**69-r5** (două forme de 400 pe același endpoint). Li se adaugă un singur
candidat vechi, aditiv și ieftin: **67e** (gardian de nomenclator pentru ciclul
din `Cont.Parinte`).

**Explicit EXCLUSE din felie** (decizii proprii, nu se iau „în trecere"): 64h
(dimensiunea Repartitor pe rândul de bani — schimbă conținutul registrului
pentru toate tipurile), 36f (TVA la încasare), 51e (`PoliticaEvaluare`/CMP),
63f (laturile interne pe `Calitati`, retrofit `MaterializeazaValori` BTR — felie
de API/client, nu de motor).

## Cauzele mecanice (constatate, explorare read-only 2026-08-25)

- **r4.** `Motor/TvaService.cs:38-43` unește `Normal` și `TaxareInversa` în
  același `case` — regula e per REGIM, fără sens. `Motor/MotorOperare.cs:233-237`
  postează 4426=4427 pe ramura TI IGNORÂND `politicaTva.Directie` (pe care
  ramura `else` o citește). Sensul există doar în `RegistruTvaService.cs:61`.
  `TipTva` n-are câmp de sens aplicabil; TI21/TI19 nu sunt implicite nicăieri
  (`SeedTipTvaImplicit`) și niciun filtru nu interzice TI pe FCL. D300 ocolește
  prin excepția „TI pe livrare" (`Proiectii/D300Proiectii.cs:329-330`), iar
  D3-V3 (`ModelCheck/Program.cs:9620-9788`) FIXEAZĂ comportamentul actual.
  Import1C compensează la sursă (`Import1C/Vanzare1C.cs:74-89`: `tipTva = null`
  pe liniile FCL cu cotă TI) — importul nu trece niciodată TI pe FCL prin motor.
  Singurul test de motor pe TI e pe FCT (`Program.cs:418-437`); FCL-TI nu e
  exersat la nivel de `RegistruContabil`.
- **r7.** `UseDeferredDeletion` are DOUĂ suprasarcini (DX
  `DeferredDeletion/EFCoreDeferredDeletionExtension.cs`): cea pe `ModelBuilder`
  (:50, filtrul global `GCRecord = 0`) RULEAZĂ și în ModelCheck (e în
  `BackOfficeDbContext.OnModelCreating:134-136`); cea pe `DbContextOptionsBuilder`
  (:65, `EFCoreDeferredDeletionInterceptor`) LIPSEȘTE din ambele builder-e ale
  ModelCheck (`Program.cs:82-85`, `112-117`). E metodă publică, independentă de
  `AddEFCore`. Consecință: `os.Delete` șterge fizic și CASCADEAZĂ prin FK
  (D4 balanță, `Program.cs:7937-7950`); F5 (`:9299-9354`) simulează ștergerea
  logică prin `UPDATE … GCRecord = 1`. ModelCheck NU recreează baza (`MigrateAsync`,
  `:92`), doar migrează — seed-ul rulează la fiecare rulare.
- **r5.** `ContaApiController` (`:52-54`) poartă `[ApiController]`; `Startup.cs`
  n-are `ConfigureApiBehaviorOptions` → la eșec de binding iese
  `ValidationProblemDetails`, nedeclarat în `openapi.json` (toate `400` referă
  `EroriDto`) și IGNORAT de `nucleu/http.ts:36-60` (cade pe eroarea generică, fără
  detaliul per-câmp). 400-ul de domeniu (`BadRequest(EroriDto.Din(erori))`) e pe
  proiecții (`RegistruJurnalController:30`, `TvaControllere:49,90`,
  `BalantaController:54,113`, `D300Controller:72`, `FisaContController:58`).
- **67e.** `Cont.Parinte` e editabil (OData CRUD pe nomenclatoare vii + XAF);
  balanța pliată oprește ciclul prin gardă de vizitare, nu îl RAPORTEAZĂ.

## Deciziile

### F13-D1 — Taxarea inversă are SENS: pe direcția Colectat nu există TVA

Regula de fond (Cod fiscal art. 331: beneficiarul aplică autolichidarea;
furnizorul emite factura FĂRĂ TVA, cu mențiunea „taxare inversă"): un `TipTva`
cu `Regim = TaxareInversa` pe un document a cărui `PoliticaTva.Directie` e
`Colectat` (FCL, RDC) produce **`ValoareTva = 0` și NICIUN rând contabil de
TVA**. Pe `Deductibil` (FCT, DEC, RLF) autolichidarea rămâne cum e azi:
`ValoareTva = net × cotă`, 4426 = 4427.

Sursa sensului = **`PoliticaTva.Directie`**, politică-dată existentă (36b), NU
un câmp nou pe `TipTva` și NU un hook polimorf pe frunză: tipul fără
`PoliticaTva` nu calculează TVA oricum (motorul nu postează TVA fără politică),
deci „sensul" e deja proprietatea tipului de document, nu a liniei.

Implementare, în trei locuri, cu aceeași semantică:
1. `TvaService.CalculeazaValori` primește `DirectieTva? directie` (parametru
   explicit, fără default): `TaxareInversa` × `Colectat` ⇒ ramura
   `default` (`Valoare = net; ValoareTva = 0`). `directie == null` (tip fără
   politică) ⇒ comportamentul de azi (nu se pierde nimic: fără politică nu
   există rând TVA). TOȚI apelanții trec direcția: `CalculeazaLaCulegere` o
   rezolvă din `PoliticaTva` a tipului documentului (`MotorOperare.GasesteTipDocument`
   + `FirstOrDefault<PoliticaTva>`), Apply-urile (FCT/FCL/DEC) și
   `PregatesteOperare` la fel — un helper `TvaService.DirectiePentru(os, doc)`
   (o interogare, cache-uibilă per document în Apply).
2. `MotorOperare.cs:228-249`: ramura `TaxareInversa` cere
   `politicaTva.Directie == Deductibil` ca să posteze 4426=4427; pe `Colectat`
   nu produce rând (ca `Scutit`). Gard, nu doar consecință a lui `ValoareTva = 0`.
3. Override-ul de `ValoareTva` cules (56: „doar pe regimuri cu TVA separat"):
   pe TI × Colectat un `ValoareTva ≠ 0` cules = **refuz** în
   `ValideazaOperare`/Apply cu mesaj de domeniu („Taxarea inversă pe livrare nu
   poartă TVA; linia N are TVA cules X") — nu se ignoră tăcut (62f: „un gard
   care tace devine capcană").

Consecințe obligatorii:
- `RegistruTvaService.Cifre` NU se schimbă (citește `ValoareTva`, acum 0 nativ);
  rândul FCL-TI apare în jurnal cu `Baza = net, Tva = 0` (68: liniile fără TVA
  apar legal).
- **Excepția F2 din `D300Proiectii.cs:329-330` DISPARE**; D3-V3 se rescrie:
  rd. 13 cu `Baza = 300, Tva = null` (coloana nu există) și
  `pierdutTva == 0` NATIV, `Avertismente.Count == 0`; premisa din comentariu
  („taxa CALCULATĂ n-are unde să meargă") se înlocuiește cu regula D1.
- Test NOU de motor în ModelCheck, lângă `:418-437`: FCL + TI21 → linia are
  `ValoareTva = 0`, `RegistruContabil` fără 4426/4427, `RegistruTva` cu
  `Tva = 0`; FCL + TI21 cu `ValoareTva` cules ≠ 0 → refuz la operare; RLF + TI
  (Deductibil) postează 4426=4427 în continuare; storno-ul FCL-TI nu naște
  rânduri de TVA. Ambele profiluri (56f) — la bugetar TI n-are seed, testul
  se sare cu motiv explicit dacă nomenclatorul nu-l are.
- Import1C: comentariul compensator din `Vanzare1C.cs:75-89` se ACTUALIZEAZĂ
  (motivul rămâne „ca în sursă: 1C nu scrie rând de TVA", nu „motorul
  inventează 4426=4427"); `tipTva = null` se PĂSTREAZĂ (schimbarea ar muta baza
  pe rd. 13 al D300 — decizie de import, nu a feliei). Flag-ul
  `TvaLivrareTaxareInversa` rămâne (contor de raport).

### F13-D2 — ModelCheck primește interceptorul de ștergere amânată

Opțiunea (i): `.UseDeferredDeletion()` pe AMBELE builder-e din
`ModelCheck/Program.cs` (`:82-85`, `:112-117`), ca `os.Delete` să facă în
harness EXACT ce face în host (probele pe calea reală, 66h). Consecințe:
- Scenele care se bazau pe cascada FIZICĂ (D4 balanță `:7937-7966`,
  `CurataBal()` `:7648-7656`, orice cleanup de scenă) se rescriu: curățenia de
  scenă = **purjare explicită prin SQL** (`DELETE` brut pe entitățile de test,
  în ordinea FK), NU `os.Delete` — curățenia nu e probă, e infrastructură.
  Regula pentru agent: `os.Delete` rămâne DOAR unde ștergerea logică e chiar
  obiectul probei.
- F5 (`:9299-9354`) folosește `os.Delete(mapare)` + `CommitChanges` în locul
  `UPDATE … GCRecord = 1` și verifică `GCRecord = 1` în tabelă (proba mecanismului
  real: 69b) — plus cel puțin o probă a lui 57f (`EsteSters`: ștergerea amânată
  a unei linii de draft e invizibilă la Committing pentru gardian).
- Baza ModelCheck nu se recreează ⇒ rândurile `GCRecord = 1` se acumulează
  între rulări. Acceptat DOAR dacă niciun check nu numără rânduri fără filtrul
  global (SQL brut trebuie să pună `GCRecord = 0` explicit — 66). Agentul
  inventariază toate `ExecuteSql`/`SqlBrut` care citesc tabele cu
  `IDeferredDeletion` și raportează cele fără filtru.
- **Regulă de oprire**: dacă după activare pică mai mult de ~10 scene din
  cauze care NU sunt „cleanup pe cascadă" (adică o probă de fond care
  depindea de ștergerea fizică), agentul se oprește și raportează lista —
  poate fi semnul unei presupuneri de model, nu de harness.

### F13-D3 — Un singur 400 pe sârmă: `EroriDto`

`Startup.cs`: `ConfigureApiBehaviorOptions(o => o.InvalidModelStateResponseFactory = …)`
traduce `ModelState` în `EroriDto` — un element per (câmp, mesaj), formatul
`"{câmp}: {mesaj}"`, câmpul gol omis. Statusul rămâne 400. Declarația:
`[ProducesResponseType(typeof(EroriDto), 400)]` pe `ContaApiController` (clasa
de bază) — atributele per acțiune existente rămân (nu se dublează în openapi;
dacă Swashbuckle le dublează, se scot cele per acțiune, nu invers). `openapi.json`
regenerat OFFLINE (WebApi oprit — memoria `verifica-drift-webapi-oprit`), driftul
verde. Clientul: `nucleu/http.ts:42-50` — comentariul se actualizează (400 =
întotdeauna `EroriDto` acum), ramura generică rămâne ca plasă pentru
non-JSON. Proba pe CALEA REALĂ (HTTP, userul `User`): dată malformată pe
`/api/proiectii/d300` → `400 {Erori:["perioada: …"]}`; JSON invalid pe un PUT →
`400 {Erori:[…]}`; un GUID malformat în rută cu constraint `:guid` rămâne 404
(comportament de rutare, nu de binding — se documentează, nu se „repară").

### F13-D4 — Gardian de nomenclator pentru ciclul din `Cont.Parinte`

Aditiv, în `GardianEditare` (familia secured, ambele host-uri, 55a): la
Committing, pentru orice `Cont` nou/modificat cu `ParinteId != null`, se
urmărește lanțul de părinți (pe FK, în OS-ul curent — părintele poate fi tot
nou în același commit) cu limită de vizitare; dacă ajunge la cont sau
depășește limita ⇒ refuz cu mesaj de domeniu („Contul X nu poate fi propriul
strămoș: lanțul …"). XAF: eroarea iese ca validare (mecanismul existent al
gardianului); OData: 422 (60a). Garda de vizitare din `BalantaPliata` (67e)
RĂMÂNE (apărare în adâncime pentru datele deja intrate). Test în ModelCheck:
ciclu direct (A→A), ciclu prin doi (A→B→A), lanț legitim de 3 nivele trece.

### F13-D5 — Proba supremă

`TvaService` schimbă semnătura pentru toți apelanții și `MotorOperare` capătă un
gard nou ⇒ schimbare de motor ⇒ **Import1C re-rulat integral cu raport
IDENTIC cu baseline-ul** (precedentul DIM-4), ca pas de închidere, proces
detașat + monitor (memoria `rulari-lungi-proces-detasat`). Așteptarea
argumentată: identic, fiindcă importul nu trece TI pe FCL; o diferență = defect
al feliei, nu al importului.

### F13-D6 — Ce NU face felia

Nu adaugă câmp de sens pe `TipTva`, nu filtrează lookup-urile de TipTva pe
FCL (TI pe FCL e legitim — factura se emite CU regim TI, fără TVA), nu atinge
`SeedTipTvaImplicit`, nu schimbă comportamentul Import1C, nu introduce
`TvaSuprascris` (56, rămâne restanță), nu atinge 64h/36f/51e/63f.

## Pașii (un agent per pas, verificare independentă + commit după fiecare)

1. **D1 motor** — `TvaService` + `MotorOperare` + validarea override-ului +
   apelanții (XAF culegere, Apply FCT/FCL/DEC, `PregatesteOperare`) + testele
   noi de motor + D3-V3 rescris + excepția F2 scoasă + comentariul Import1C.
   ModelCheck verde pe AMBELE profiluri.
2. **D2 harness** — interceptorul în ModelCheck, cleanup-urile rescrise, F5 pe
   mecanismul real, proba 57f, inventarul SQL-urilor brute fără filtru.
3. **D3 WebApi + client** — factory-ul, atributul pe bază, openapi regenerat,
   `http.ts`, proba HTTP.
4. **D4 gardian** — `GardianEditare` + test ModelCheck.
5. **D5** — Import1C re-rulat, raport diff = gol.
6. Review advers (agent separat) → fixuri de main → decizia 70 + CLAUDE.md +
   istoric + ștergerea memoriei de handoff.

## Regula de oprire (toți pașii)

Orice nepotrivire între contract și cod (un apelant al lui `CalculeazaValori`
neprevăzut, un test care pică din cauză ne-explicată de contract, un shape de
400 pe care factory-ul nu-l acoperă) se RAPORTEAZĂ cu `path:line` și dovada —
nu se normalizează tăcut. Niciun commit de către agenți; nu se ating
`docs/decizii`, `CLAUDE.md`.

## Închidere

Se completează la review: defectele găsite, ce rămâne deschis (candidați:
`TvaSuprascris`, filtrarea lookup-ului TipTva pe FCL ca afordanță, 400 pe
GUID malformat în rută).
