# Pasul 5, felia 15 — identitatea extinsă a partenerului + sincronizarea ANAF — CONTRACT

> Felia pregătitoare a SAF-T (D406): adresa structurată pe `Partener`,
> nomenclatorul de județe, serviciul care umple/verifică partenerul din
> registrul ANAF (`PlatitorTva`), extinderea Import1C cu adresele din 1C.
> Amendează 71b (clauza „satelitul 34g nu se deschide pentru adresă") și
> închide D4-r1 în forma „statutul de azi = registrul ANAF, la cerere".
> Precedentul de formă: `p5-felia-d394-design.md`.

## Ce a arătat explorarea (fapte, nu design)

- `Partener` (`Repartitori.cs:30-77`) are exact `CodFiscal`, `RegistruComert`,
  `TipPersoana`, `Tara` (ISO-2, normalizat în setter, gard în
  `GardianEditare.VerificaPartener`), `InregistratTva`, `TvaLaIncasare`.
  **Zero câmpuri de adresă/contact.** `Partener` e pe OData cu CRUD
  (`WebApi/Startup.cs:62`); nu există DTO/Apply/ecran React de partener —
  scrierea trece prin OData, ecranul prin XAF.
- SAF-T `AddressStructure` (xlsx 16.02.2026, foaia `5. Structures`): `City`
  și `Country` **obligatorii**, `Region` = cod din `ISO3166-1A2 - RO Dept
  Codes` (forma `RO-CJ`, 42 de valori, doar pentru RO), restul opționale;
  lungimi: stradă 70, număr 18, detaliu 70, clădire 35, localitate 35, cod
  poștal 18, județ 35, țară 2. `CompanyStructure.Address` = 1..*, obligatoriu
  pe fiecare Customer/Supplier. e-Factura cere aceleași câmpuri (BT-35…40).
- 1C exportă azi cu fallback `'Nespecificat'`/`'Nedeclarat'` pe City/Street
  (`spSAFTParteneri.sql:40-92`) și mapare hardcodată denumire județ → ISO.
- **1C are adresele**: `flax.InfoRg_InformatiaDeContact` (`Type='Adresa'`,
  `Gen… in ('Sediu social partener','Punct de lucru partener')`) — 119.104
  din 129.329 parteneri (92%); coloanele fizice: `Field1` cod poștal,
  `Field3` județ (denumire), `Field4` localitate, `Field6` stradă, `Field7`
  număr, `Field8` clădire, `Present` adresa concatenată, `CodJudet` = codul
  de județ din CNP (8 Brașov, 15 Dâmbovița, 29 Prahova, 40 București, 51/52
  Călărași/Giurgiu; 22% completat). Maparea e demonstrată în
  `vwDetaliiPartener.sql:104-120`. **Import1C nu citește nicio adresă**
  (`FlaxDb.cs:142-147`).
- **ANAF `PlatitorTva` = greenfield**: nimic în `D:\Dev` (nici `Atlas.Anaf`,
  care e exclusiv e-Factura). Contractul oficial (`doc_WS_V9.txt`):
  `POST https://webservicesp.anaf.ro/api/PlatitorTvaRest/v9/tva`, JSON
  `[{ "cui": <număr>, "data": "AAAA-LL-ZZ" }]`, **≤ 100 CUI/apel, 1 apel/s**;
  răspuns `{cod, message, found[], notFound[]}`; `found[]` = `date_generale`
  (`cui, denumire, adresa, nrRegCom, telefon, codPostal, stare_inregistrare,
  cod_CAEN, iban, statusRO_e_Factura, forma_juridica…`),
  `inregistrare_scop_Tva.scpTVA`, `inregistrare_RTVAI.statusTvaIncasare`,
  `stare_inactiv.statusInactivi`, `inregistrare_SplitTVA`,
  `adresa_sediu_social` (prefix `s`: `sdenumire_Strada, snumar_Strada,
  sdenumire_Localitate, scod_Localitate, sdenumire_Judet, scod_Judet,
  scod_JudetAuto, stara, sdetalii_Adresa, scod_Postal`) și
  `adresa_domiciliu_fiscal` (prefix `d`, aceleași câmpuri). `scod_JudetAuto`
  = indicativul auto (`CJ`, `B`) ⇒ `RO-` + indicativ e codul ISO direct.
- Repo: niciun `HttpClient`/apel HTTP ieșitor, nicio secțiune proprie în
  `appsettings`, niciun serviciu de Module în DI (toate sunt `static class` +
  `IObjectSpace`; modulele XAF nu înregistrează servicii). `ComandaAutorizata`
  e cablat pe `Document` — o comandă pe partener cere gate propriu.
  `ModelCheck` creează `Partener` în 16 scene ⇒ niciun câmp de adresă nu
  poate fi obligatoriu în model.
- Invarianți: V (ANAF = conector, evidență) — dar pe axa „înregistrat în
  scopuri de TVA" registrul ANAF **este** canonicul (D4-r1); IV.3 (nimic
  mutabil la runtime pe `SetareProfil`).

## De ce adresa e câmp pe frunză, județul e nomenclator, iar ANAF e comandă

- Adresa = caracteristică a partenerului, cerută de două formulare legale;
  forma owned a murit (54c) ⇒ coloane PLATE pe `Partener`, nullable toate.
- Județul are trei chei legale (ISO `RO-CJ`, indicativ auto `CJ`, cod CNP
  `12`) și o denumire; e FK pe partener și lookup în UI ⇒ nomenclator
  seed-uit de nucleu, `ForbidCRUD` (tiparul `RandD300`), nu constantă în cod
  (`TariUe` rămâne constantă: nu e FK, n-are denumire de afișat).
- Sincronizarea ANAF nu e politică și nu e motor: e o comandă pe nomenclator
  care scrie câmpuri obișnuite + un timbru server-owned. Nimic din registre
  nu se mișcă; reconcilierea nu e atinsă.

## Deciziile de fixat

### D15-D1 — Modelul: adresa PLATĂ pe `Partener` + nomenclatorul `Judet`

Pe `Partener` (toate nullable, `[MaxLength]` = lungimile SAF-T ⇒ `varchar(n)`
în migrație; `FieldSize` nu produce lungime în EF — constatare din F14):

| Câmp | Tip | Caption | SAF-T / ANAF / 1C |
|---|---|---|---|
| `Strada` | string(70) | „Stradă" | StreetName / `*denumire_Strada` / `Field6` |
| `Numar` | string(18) | „Număr" | Number / `*numar_Strada` / `Field7` |
| `DetaliiAdresa` | string(70) | „Bloc, scară, etaj, ap." | AdditionalAddressDetail (+Building pliat) / `*detalii_Adresa` / `Field8` + rest |
| `Localitate` | string(35) | „Localitate" | City / `*denumire_Localitate` / `Field4` |
| `CodPostal` | string(18) | „Cod poștal" | PostalCode / `*cod_Postal` / `Field1` |
| `JudetId` / `Judet` | FK nullable → `Judet` | „Județ" | Region / `RO-`+`*cod_JudetAuto` / `CodJudet` sau `Field3` |
| `DataSincronizareAnaf` | DateTime? | „Sincronizat ANAF la" | **server-owned** (55a): scris doar de serviciu, pe ușa non-secured; `GardianEditare` refuză schimbarea lui pe secured |
| `InactivFiscal` | bool | „Inactiv fiscal (ANAF)" | `stare_inactiv.statusInactivi`; doar evidență, nicio consecință în motor |

`Tara` rămâne cod ISO-2 fără nomenclator (SAF-T nu cere denumirea țării).
`Judet` = `{ Cod "RO-CJ" (cheie unică), Denumire "Cluj", CodAuto "CJ",
CodCnp 12 }`, nucleu, seed din foaia `ISO3166-1A2 - RO Dept Codes` (42
rânduri; seed-ul RESCRIE câmpurile ne-cheie, ca `RandD300`), `ForbidCRUD`,
pe OData ReadOnly, `[NavigationItem("Nomenclatoare")]`. Lookup static:
`Judet.DupaCodAuto`, `DupaCodCnp`, `DupaDenumire` (normalizare: trim,
majuscule, fără diacritice, `-`/spații duble pliate, prefixele
`MUNICIPIUL`/`JUDETUL` tăiate) — **funcții pure în Module**, testabile.

Gardian (`VerificaPartener`, ambele host-uri): `JudetId != null && Tara != "RO"`
⇒ refuz („județul e al adreselor din România"). Nicio altă validare (SAF-T:
„nicio validare pentru codul poștal"). Regula XAF pereche pe `Judet` nu
există (FK-ul nu poate purta regula — 40b); gardianul ajunge.

Migrația: `AddAdresaPartener` (coloanele + tabela `Judete` + FK + index unic
pe `Judete.Cod`); aplicată și pe `BackOffice.Privat` și pe clonele Flax
(decizia 63). Layout XAF: al treilea grup `GrupAdresa` „Adresă" pe
`Partener`, cu toate câmpurile noi declarate; `DataSincronizareAnaf` și
`InactivFiscal` read-only în grupul Fiscal. `--dump-metadata` regenerat.

Amendament 71b: „satelitul 34g se deschide cât cere adresa structurată";
contactul (email/telefon), IBAN-ul partenerului, delegații, adresele
multiple (punct de lucru) rămân 34g.

### D15-D2 — Clientul `PlatitorTva`: o clasă, fără DI în Module

`Module/Anaf/PlatitorTvaClient.cs` — clasă concretă cu constructor
`(HttpClient http, string url = UrlImplicit)`; hosturile o construiesc:
WebApi prin `services.AddHttpClient<PlatitorTvaClient>()` +
`Configuration["Anaf:PlatitorTvaUrl"]` (secțiune nouă, opțională; lipsa ⇒
URL-ul oficial v9 din cod), Import1C prin `new HttpClient()`. `System.Net.Http.Json`
din BCL, niciun pachet nou. Nu se copiază `Atlas.Anaf` (are trei defecte de
DI și niciun POST JSON); se preia doar ideea de clasificare a răspunsului.

Contract:
```
Task<RaspunsPlatitorTva> Interogheaza(IReadOnlyList<long> cuiuri, DateOnly data, CancellationToken ct)
```
- împarte în loturi de **100**, respectă **1 apel/s** (așteptare între loturi;
  nicio paralelizare); un lot eșuat NU oprește restul — rezultatul are
  `Gasiti[]`, `Negasiti[]`, `Erori[] {lot, mesaj, tranzitorie}`;
- răspunsul se deserializează într-un DTO care ține **doar câmpurile
  consumate** (D3) + `Cui`; restul se ignoră (`JsonSerializerOptions` tolerant
  la câmpuri necunoscute și la `cod`/`message` lipsă — comportament raportat
  de utilizatori pe v9);
- `cui` se trimite ca număr (contractul), `data` = ziua interogării;
- HTTP ≠ 2xx / timeout / JSON invalid ⇒ eroare de lot cu `tranzitorie` (5xx,
  timeout, rețea) vs. fatală (4xx, JSON) — fără retry automat în client
  (apelantul decide; Import1C reia lotul o dată la tranzitorie).

Ce se **cere** de la răspuns (restul nu intră): `date_generale.{cui, denumire,
nrRegCom, codPostal, stare_inregistrare}`, `inregistrare_scop_Tva.scpTVA`,
`inregistrare_RTVAI.statusTvaIncasare`, `stare_inactiv.statusInactivi`,
`adresa_domiciliu_fiscal.*` și `adresa_sediu_social.*` (câmpurile
`*denumire_Strada, *numar_Strada, *denumire_Localitate, *cod_JudetAuto,
*cod_Postal, *detalii_Adresa`).

### D15-D3 — Regula de merge: „gol se umple, diferit se raportează, canonicul bate"

`Module/Anaf/SincronizareAnafService.cs`, **static, pur pe date**:
```
static RezultatSincronizare Aplica(Partener p, DateAnaf d, bool suprascrie, DateTime acum)
static IReadOnlyList<Partener> Candidati(IObjectSpace os, IEnumerable<Guid> ids)   // filtrul din D4
static long? CuiInterogabil(Partener p)                                             // normalizarea
```
`Aplica` NU face HTTP și NU comite: primește partenerul și datele ANAF deja
mapate (`DateAnaf` = DTO-ul din D2 aplatizat), scrie pe obiect și întoarce
`RezultatSincronizare { Modificari[] {camp, vechi, nou}, Diferente[] {camp,
cules, anaf}, Avertismente[] }`. Regulile, per câmp:

| Câmp | Regula |
|---|---|
| `InregistratTva` ← `scpTVA`, `TvaLaIncasare` ← `statusTvaIncasare`, `InactivFiscal` ← `statusInactivi` | **canonic (D4-r1)**: se scrie ÎNTOTDEAUNA; schimbarea = `Modificare` |
| `Denumire` | niciodată suprascrisă implicit (e eticheta contabilului); diferit ⇒ `Diferenta`; `suprascrie` ⇒ se scrie |
| `RegistruComert` | gol ⇒ umple; diferit ⇒ `Diferenta`; `suprascrie` ⇒ scrie |
| adresa (`Strada, Numar, DetaliiAdresa, Localitate, CodPostal, JudetId`) | ca UN bloc: sursa = domiciliul fiscal dacă are `Localitate`, altfel sediul social; blocul local gol (toate cele 6 nule) ⇒ se umple; altfel câmp cu câmp: egal ⇒ nimic, diferit ⇒ `Diferenta` (sau scriere la `suprascrie`) |
| `JudetId` | `Judet.DupaCodAuto(*cod_JudetAuto)`; negăsit ⇒ `Avertisment`, câmpul rămâne |
| lungimi | valoarea ANAF mai lungă decât coloana ⇒ tăiată la `MaxLength` + `Avertisment` |
| `DataSincronizareAnaf` | = `acum` (UTC) la orice răspuns `găsit`, chiar fără modificări |
| `notFound` | nicio scriere; `Avertisment` „CUI-ul nu figurează la ANAF"; timbrul NU se pune |

Egalitatea de text = ordinal, după trim și pliere de spații (nu case-insensitive:
„STR. AVRAM IANCU" ≠ „Str. Avram Iancu" e o diferență reală pe care ANAF o
normalizează în felul lui — se raportează, nu se ascunde).

`CuiInterogabil`: `CodFiscal` trim, majuscule, `RO` tăiat, doar cifre, 2–10
cifre ⇒ `long`; altfel `null`. **Candidat** = `Tara == "RO"` și
`CuiInterogabil != null` și nu (`TipPersoana == Fizica` cu 13 cifre — CNP-ul
nu se interoghează). Necandidații ies în rezultat ca `Sarit {id, motiv}`.

### D15-D4 — REST: comandă pe partener, gate propriu, ușa non-secured

`ParteneriController` (`api/parteneri`), doar comenzi (citirea rămâne OData):
- `POST api/parteneri/{id}/sincronizeaza-anaf?suprascrie=false` ⇒ 200
  `SincronizareAnafDto` (rezultatul din D3 + `gasit`, `cui`), 404 partener
  inexistent, 403 fără `CanWrite` pe partener, 422 `EroriDto` (necandidat,
  eroare ANAF fatală), **503** `EroriDto` la eroare tranzitorie ANAF (nu 422:
  clientul poate reîncerca).
- `POST api/parteneri/sincronizeaza-anaf` body `{ ids: Guid[] (1..500),
  suprascrie }` ⇒ 200 `SincronizareAnafLotDto { rezultate[], sarite[],
  erori[] }`; 400 `EroriDto` pe listă goală/peste 500 (5 apeluri ANAF, ~5 s —
  plafonul ține cererea sub timeout-urile obișnuite; „toți partenerii" e
  treaba Import1C/CLI, nu a unui request).
- Gate: `ComandaAutorizata` se generalizează în `ComandaAutorizata<T>(Guid id,
  …)` (`GetObjectByKey<T>` + `CanWrite`) — varianta pe `Document` devine
  apelul cu `T = Document`; pentru lot: fiecare id trece gate-ul, cele
  refuzate ies în `sarite` cu motiv, nu 403 pe tot lotul.
- Scrierea pe ușa **non-secured** (58c: `DataSincronizareAnaf` e server-owned)
  într-un singur `CommitChanges` per partener (lotul = n commit-uri; eșecul
  unuia nu strică restul).
- `openapi.json`/`api-types.ts` regenerate; `verifica:drift` idempotent.

### D15-D5 — XAF: acțiunea „Sincronizează din ANAF" pe `Partener`

`SimpleAction` pe DetailView `Partener` (și ListView, multi-selecție ≤ 500),
în Blazor.Server, care rulează D2+D3 pe un OS **non-secured** propriu (58c),
apoi reîncarcă view-ul; rezultatul (modificări/diferențe/avertismente) ca
mesaj XAF. Aceasta e calea umană a feliei — clientul React n-are ecran de
partener și nu-l primește aici (→ `lista-react.md`).

### D15-D6 — Import1C: adresele din 1C, apoi ANAF ca pas final opțional

- `FlaxDb`: interogare nouă `AdresaPartener(hexId)` / `AdreseParteneri(ids)`
  pe `flax.InfoRg_InformatiaDeContact` cu EXACT filtrul și ordinea din
  `vwDetaliiPartener.sql:104-120` (sediu social înaintea punctului de lucru,
  `SimpleKey desc`), **fără** excluderea PF (sursa are, luăm; D4-r2). `record
  FlaxAdresa(CodPostal, JudetDenumire, CodJudetCnp, Localitate, Strada, Numar,
  Cladire, Prezentare)`. Proba contractului de coloane intră în `--cititori`.
- `AplicaAdresa(Partener e, FlaxAdresa a)` simetric cu `AplicaClasificare`:
  se aplică la materializare (`AsiguraPartener`) și în `--reclasifica` peste
  toți partenerii legați, **doar pe bloc gol** (adresa culeasă/ANAF nu se
  rescrie din 1C). Județul: `CodJudet` (CNP) întâi, apoi denumirea; ambele
  eșuate ⇒ contor `JudetNerezolvat` + `DetaliiAdresa` primește ce rămâne.
  `Cladire` se pliază în `DetaliiAdresa`. Lungimi peste `MaxLength` ⇒ tăiere +
  contor. Contoare noi în raport: `AdresePreluate`, `FaraAdresaInSursa`,
  `JudetDinCodCnp`, `JudetDinDenumire`, `JudetNerezolvat`, `AdreseTrunchiate`.
- `--anaf` (flag nou, opțional, și în `--reclasifica --anaf`): după
  clasificare, D2+D3 peste TOȚI candidații legați (Flax: ~20k parteneri ⇒
  ~200 apeluri ⇒ ~4 min), `suprascrie = false`, commit per lot de 100;
  raportul: `AnafGasiti`, `AnafNegasiti`, `AnafModificari` per câmp,
  `AnafDiferente` per câmp, `AnafErori`. Rulare detașată (jurnal 50d).
- **Reconcilierea nu se atinge**: raportul `reconciliere-*.txt` rămâne
  IDENTIC cu baseline-ul (nomenclatorul se îmbogățește, cifrele nu se mișcă).
  D394 pe clona Flax: diferențele de clasificare aduse de ANAF (`--anaf`) se
  RAPORTEAZĂ (câți parteneri și-au schimbat tipul, Σ pe cartușe înainte/după)
  — sunt corecții, nu regresii; evidența bate eticheta (34f, 71h).

## Ce NU intră, cu motiv

- Contact (email/telefon), IBAN pe partener, delegați, adrese multiple → 34g
  (SAF-T `Contact`/`BankAccount` sunt opționale pe partener).
- Nomenclator de țări cu denumiri → 34g (SAF-T validează codul, nu cere numele).
- Istoricul statutului de TVA (perioade `data_inceput/sfarsit_ScpTVA`) →
  D4-r1 rămâne pentru „statutul la data documentului"; azi = statutul de azi.
- Antetul societății proprii (`CompanyHeaderStructure`: adresă, contact,
  IBAN obligatorii) → D4-r10, în felia SAF-T (e `SetareProfil`/entitate
  proprie, nu partener).
- Ecran React de partener → `lista-react.md` (prima felie de nomenclator din
  client e o decizie de finisaj, nu de model).
- Sincronizare automată/periodică (job) → nu: comanda e la cerere; un job e
  infrastructură de host care nu există încă.
- `RegistruComert` din ANAF ca sursă canonică → nu: e etichetă, se raportează.
- Split TVA, CAEN, forma juridică, e-Factura status → nu se persistă (niciun
  formular nu le cere azi).

## Riscurile pin-uite (ținta review-ului advers)

1. Timbrul `DataSincronizareAnaf` scris pe ușa secured (OData PUT) — gardianul
   trebuie să-l refuze ca pe `Autogenerat`.
2. `suprascrie = true` care rescrie `Denumire` cu denumirea ANAF în lot —
   e explicit, dar lotul de 500 cu `suprascrie` e ireversibil: rezultatul
   trebuie să listeze fiecare `Modificare` cu valoarea veche.
3. Merge-ul pe bloc gol vs. câmp cu câmp: un partener cu doar `Localitate`
   culeasă nu e „bloc gol" ⇒ strada nu se umple ⇒ trebuie `Diferenta` pe
   strada goală? Nu: câmp gol individual se umple și el (gol ⇒ umple, per
   câmp), diferența e doar la valori ne-goale diferite. Verificat în V2.
4. `notFound` pe CUI valid (ANAF întoarce 200 cu `notFound` și la CUI
   radiat) — nu se atinge `InregistratTva` (rămâne ce era; avertisment).
5. Rate-limit: două cereri REST concurente ⇒ două clienți ⇒ 2 apeluri/s ⇒
   ANAF poate răspunde 429/5xx ⇒ tranzitoriu ⇒ 503; acceptat (single-operator
   azi, 25f), documentat.
6. `Judet` seed pe bugetar: nucleu, deci pe AMBELE profiluri; `VerificaProfil`
   neatins.
7. CUI cu 2–10 cifre trimis ca număr: zerourile din față (nu există la CUI)
   — dar `CodFiscal` cu „RO 123 45" (spații) trebuie să dea 12345.
8. Gate-ul `ComandaAutorizata<T>`: refactorul nu schimbă comportamentul pe
   `Document` (ModelCheck/HTTP existente rămân verzi).

## Verificări (ModelCheck, ambele profiluri; HTTP pe clona Flax.Api)

- **V1** modelul: cele 8 câmpuri pe frunza `Partener` (nu pe `Repartitor`),
  `MaxLength` = lungimile SAF-T; `Judet` seed = 42, `RO-B` și `RO-CJ`
  prezente, `DupaCodCnp(40) == RO-B`, `DupaCodAuto("CJ")`,
  `DupaDenumire("Municipiul Bucureşti") == RO-B`, `DupaDenumire("Bistrita
  Nasaud") == RO-BN`; gardianul: `JudetId` cu `Tara = DE` ⇒ refuz;
  `DataSincronizareAnaf` schimbat pe secured ⇒ refuz; pe non-secured ⇒ trece.
- **V2** `Aplica` pe date fabricate, fără HTTP: bloc gol ⇒ toate cele 6 se
  umplu + `Modificari` = 6; câmp gol individual ⇒ se umple; valoare diferită ⇒
  `Diferenta`, obiect neatins; `suprascrie` ⇒ scris + `Modificare` cu vechi;
  `scpTVA` false peste `InregistratTva` true ⇒ scris (canonic); `Denumire`
  diferită fără `suprascrie` ⇒ neatinsă; județ necunoscut ⇒ avertisment;
  stradă de 90 de caractere ⇒ 70 + avertisment; `notFound` ⇒ nicio
  modificare, fără timbru; `CuiInterogabil("RO 123 45") == 12345`,
  CNP ⇒ null, `Tara = DE` ⇒ necandidat.
- **V3** deserializarea: un răspuns v9 real salvat în `ModelCheck/Fixtures`
  (anonimizat la un CUI public, ex. ANAF 4192953… — se ia unul real la
  implementare) + un răspuns fără `cod`/`message` + un răspuns cu câmpuri
  necunoscute ⇒ toate parsează.
- **V4** HTTP pe calea reală (clona `Atlas.Conta.Import1C.Flax.Api`, WebApi
  pornit): `User` fără Write ⇒ 403; `Admin` ⇒ 200 cu rezultat; partener DE ⇒
  422; lot de 501 ⇒ 400; PUT OData care schimbă `DataSincronizareAnaf` ⇒ 4xx.
  Un apel REAL la ANAF pe un CUI cunoscut (nu în ModelCheck: suita rămâne
  offline și deterministă).
- **V5** Import1C pe clona Flax: `--reclasifica` cu adrese ⇒ contoarele
  (așteptat ≈ 92% `AdresePreluate` pe partenerii legați); apoi `--anaf`:
  cifrele de găsiți/negăsiți/modificări; `reconciliere-*.txt` IDENTIC cu
  baseline-ul; D394 09/2025 înainte/după `--anaf` cu diferențele explicate
  per cauză.
- `verifica:drift` idempotent; `--dump-metadata` comis.

## Regula de oprire

Agentul se oprește și raportează (nu normalizează tăcut) dacă:
- contractul v9 real diferă de cel de mai sus (câmpuri lipsă, `cui` refuzat
  ca număr, alt prefix) — se aduce răspunsul brut, nu se ghicește;
- un câmp de adresă trebuie făcut obligatoriu ca să treacă ceva;
- `ComandaAutorizata<T>` cere schimbarea semanticii pe `Document`;
- migrația atinge altceva decât `Parteneri` + `Judete`;
- raportul de reconciliere Import1C diferă de baseline cu o singură linie;
- ModelCheck pică pe un profil.

**Explicit NU în regula de oprire**: avertismente ANAF pe date reale
(județ nerezolvat, trunchieri, negăsiți) — se raportează cu cifre.

## Pașii (un agent per pas, verificare independentă + commit după fiecare)

1. **Model**: D1 integral (câmpuri, `Judet` + seed, migrație pe toate bazele,
   gardian, layout, V1, `--dump-metadata`). Fără HTTP.
2. **Serviciul**: D2 + D3 (`Module/Anaf/`), V2 + V3, acțiunea XAF (D5).
3. **REST**: D4 (`ParteneriController`, `ComandaAutorizata<T>`, DTO-uri),
   codegen, V4 pe clona Flax.Api.
4. **Import1C**: D6, V5 pe clona Flax (rulare detașată).
5. **Închidere**: review advers, fix-uri de main, decizia 72 + §71b amendat,
   CLAUDE.md, README jurnal, istoric, `lista-react.md`, memoria de handoff.
