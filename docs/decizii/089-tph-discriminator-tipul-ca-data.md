# 89. Pasul 5, felia 28 — TPH cu discriminatorul mapat `ClrType` pe `Document`, `DocumentDetaliu` și `Repartitor`; coloanele partajate fără prefix de tip; coloana de frunză se citește doar pe o mulțime restrânsă pe tip; tipul țintei unui FK spre frunză ținut de gardian și probat în bază; tipul documentului ca dată, printr-un singur cititor; căutarea după cheie a unui tip ne-rădăcină pe rădăcina ierarhiei, cu tipul verificat; lanțul de migrații resetat

- **Data**: 2026-09-18
- **Stare**: activă (amendează 3 și 16; amendează IM-D10 din contractul de izolare a motorului; închide 75-r2)
- **Docs**: `docs/api/p5-felia28-tph-contract.md` (F28-D1…D8, pașii 0–4, §„Ce NU intră"), `docs/api/p5-felia28-pas0-spike.md` (maparea rezultată, FK-ul discriminator, EXPLAIN-urile), `docs/api/p5-perf-masuratori.md` §Felia 28, `nou/.../Module/BusinessObjects/BackOfficeDbContext.cs` (`UseTphMappingStrategy`, `HasDiscriminator`, `AplicaColoanePartajate`, `TipStocare`), `nou/.../Module/BusinessObjects/Documente/Document.cs` (`Document.ClrType`, `DocumentDetaliu.ClrType`), `nou/.../Module/BusinessObjects/Nomenclatoare/Repartitori.cs` (`Repartitor.ClrType`), `nou/.../Module/BusinessObjects/Politici/Politici.cs` (`TipDocument.ClrType` read-only), `nou/.../Module/Api/CititorTipDocument.cs`, `nou/.../Module/Api/{ApiProiectii,OperareApi}.cs`, `nou/.../Module/Saft/SaftProiectii.cs`, `nou/.../Module/Motor/GardianEditare.cs` (regula (o), `FkSpreFrunze`), `nou/.../Module/Motor/RandDupaCheie.cs`, `nou/.../Module/Api/{Refuzuri,Rezolva}.cs` (`RandDeAltTip`), `nou/.../Module/Motor/{PerioadaService,CorectieService}.cs`, `nou/.../WebApi/API/Conta/CorectieController.cs`, `nou/tools/BackfillTva/Reconciliere.cs`, `nou/tools/ModelCheck/Program.cs` (`VerificaF28`, `F28-A…K`), `nou/tools/ModelCheck/IntegritateTph.cs` (`--dump-integritate-tph`), `nou/tools/ModelCheck/ScanareGetObjectByKey.cs` (F28-N); migrația `20260918113542_InitialCreate` (singura din lanț)

## Regula durabilă

**Tipul concret al unui rând e o DATĂ pe rând, nu o tabelă: cele trei
ierarhii stau fiecare pe tabela rădăcinii, cu discriminatorul mapat
`ClrType`. Discriminatorul e o ETICHETĂ citită ca dată, nu un comutator de
comportament.**

(a) **TPH pe `Document`, `DocumentDetaliu` și `Repartitor`, cu
discriminatorul MAPAT `ClrType`.** `UseTphMappingStrategy()` +
`HasDiscriminator(x => x.ClrType)` pe fiecare rădăcină, cu valorile IMPLICITE
ale EF (numele scurt al clasei CLR), fără nicio `HasValue` scrisă de mână.
Proprietatea e `string` cu setter protejat, read-only în XAF, cu caption
„Tip”: vizibilă în grile, filtrabilă și folosibilă în criterii, fără să poată
fi scrisă din cod de aplicație. Pe `Document` valoarea e exact ancora
`TipDocument.ClrType` (20). **Discriminatorul nu are FK spre
`TipDocument.ClrType` și nu are navigație**: cheia alternativă cere valoare
nenulă la `Add`, iar calea XAF (`CreateObject`: seed, „New”, OData) face
`Add` înaintea atribuirii. Garanția „orice document are ancoră” o țin
generatorul de discriminator al EF, setter-ul protejat, proba F28-A (clase
concrete ↔ seed 1:1, în ambele sensuri) și indexul unic filtrat
`IX_TipuriDocument_ClrType`. Nivelul abstract `DocumentTrezorerie` rămâne
declarat explicit în model. Fiecare rădăcină are index pe `ClrType`; indexul
compus `(ClrType, DocumentId)` pe `DocumentDetalii` NU se pune (planificatorul
nu-l alege). Lungimea coloanei o dă convenția EF: o clasă nouă cu nume mai
lung produce singură o migrație `ALTER COLUMN`. Niciun `switch` pe valoarea
lui `ClrType` în `Motor/*`; rezoluția conexului rămâne prin
`TipDocument.ClrType` (IM-D9).

(b) **Coloanele frunzelor sunt partajate, fără prefix de tip, iar o coloană
de frunză se citește DOAR prin tipul frunzei.** Proprietățile cu același nume
pe frunze-surori stau pe ACEEAȘI coloană, numită ca proprietatea
(`GetColumnName() == Name` pe toate proprietățile celor trei ierarhii).
Regula de citire, pe adevărul EF sub TPH: **`as` sau cast pe frunză NU
filtrează pe tip** — `(l as AsamblareDetaliu).Directie` emite
`SELECT d."Directie"` fără `CASE`, deci citește și valoarea fratelui care
împarte coloana. O coloană de frunză se citește numai (1) pe o mulțime deja
restrânsă pe tip — `GetObjectsQuery<Frunza>()`, `OfType<Frunza>()`, liniile
unui document al cărui `[TipDetaliu]` e frunza — sau (2) prin forma
`x is Frunza ? ((Frunza)x).Prop : null`, singura care emite
`CASE WHEN "ClrType" …`. Niciodată prin entitatea de bază și niciodată în SQL
brut fără filtru pe `ClrType`. Premisa care face sigure citirile pe liniile
unui document („liniile unui document sunt frunza declarată de el, un subtip
al ei sau baza”) e PROBATĂ (F28-H), nu presupusă. Coloanele partajate fizic
pe tabela bazei NU devin contract al bazei: 54c rămâne literal.

(c) **Partajarea e generică și are gardian zgomotos pe TIPUL DE STOCARE.**
O singură buclă în `OnModelCreating` (`AplicaColoanePartajate`, rulată după
`AplicaScaraNumerica`, ca facetele să fie finale), nicio configurare per
proprietate. Două proprietăți omonime pe frunze-surori împart coloana dacă au
același tip de stocare (tipul providerului după value converter; la enum,
tipul de bază) și aceleași facete (nullabilitate, lungime, precizie, scară,
tip de coloană, unicode, lungime fixă); altfel modelul nu se construiește, cu
toate coliziunile într-un singur mesaj. Două enumuri diferite peste `integer`
împart coloana (`Directie`, `Fel` pe `DocumentDetaliu`), fiindcă frunza e
citită doar prin tipul ei (b). Compararea nullabilității e deliberată; plasa
de siguranță din spatele gardianului e validatorul EF
`ValidateSharedColumnsCompatibility`. EF produce UN constraint FK și UN index
per coloană partajată, cu condiția ca frunzele care împart un FK să aibă
același `OnDelete`.

(d) **Nullabilitatea frunzelor stă în CLR, nu în CHECK.** Coloanele
frunzelor sunt nullable în tabelă, fără CHECK per discriminator;
obligativitățile de domeniu rămân în gardian, validări și motor (33d).
Invariantul fizic „o coloană declarată pe tipuri derivate e NULL pe rândurile
oricărui alt tip” e probat în bază (F28-J), inclusiv pe tipurile valoare.
`Cod`/`Denumire` pe `Repartitor`, declarate pe bază, rămân NOT NULL + CHECK
(77-r2).

(e) **Tipul țintei unui FK spre un tip ne-rădăcină îl ține gardianul pe ușa
securizată și îl probează ModelCheck pe ușa de sistem.** Sub TPH FK-ul ține
doar id-ul rădăcinii. `GardianEditare` regula (o): FK-urile spre un tip
NE-rădăcină al unei ierarhii cu discriminator se descoperă GENERIC din
metadata EF (`FkSpreFrunze`, nu listă); la obiect nou sau FK schimbat, ținta
se citește prin `GetObjectByKey` pe rădăcină — tracker-ul întâi, deci și o
țintă nouă din același commit; securitatea aplicată — și trebuie să aibă
discriminatorul tipului țintă sau al unui subtip. Altfel 422 de domeniu
(„rândul ales e X, nu Y”) sau referința e invizibilă; o țintă ștearsă logic e
tratată ca invizibilă. `FkSpreFrunze` aruncă pe un FK compus sau spre cheie
alternativă care țintește o frunză: o formă pe care regula n-o poate verifica
nu se sare tăcut. Ușa de sistem (Import1C, seed, Migrare) nu trece prin
gardian; integritatea ei o probează F28-I, cu SQL generat din metadata
(`IntegritateTph.cs`), rulat și pe bazele de import prin
`--dump-integritate-tph`. Comportamentele `OnDelete` rămân cele dinainte;
FK-urile de moștenire frunză→bază dispar.

(f) **Tipul documentului ca dată: un singur cititor.**
`Api/CititorTipDocument` răspunde „ce tip au documentele astea” printr-o
PROIECȚIE `{ID, ClrType}` pe ObjectSpace-ul primit, deci cu securitatea lui:
`Clase` (id → discriminator), `Coduri` (id → `TipDocument.Cod` printr-o a
doua interogare pe `TipDocument`; `null` pentru id inexistent sau invizibil),
`Clasa` (discriminator → tip concret, cache static). Niciun consumator nu
materializează polimorf un document doar ca să-i afle clasa. Consumatorul
care are nevoie de INSTANȚĂ (hook polimorf) rămâne pe materializare
(`ImperechereService.AsignatFataDe`, hook-ul `SensDeStins`). `ClrType` e
exclus explicit din copia corecției (`ExcluseDocument`/`ExcluseLinie`).

(g) **Lanțul de migrații s-a resetat o dată, la 2026-09-18.** Singura
migrație e `20260918113542_InitialCreate`; **migrațiile de dinainte de
2026-09-18 sunt istorie în git, nu în lanț**, iar trimiterile din docs la ele
rămân text istoric. 23a rămâne: lanțul nou e canonic și crește prin migrații.
Bazele de dev s-au recreat, fără migrare de date (greenfield asumat):
bugetarul prin `dotnet ef database update` + seed prin Blazor
`--updateDatabase --forceUpdate --silent`, `Atlas.Conta.ModelCheck.Privat`
recreată de ModelCheck, `Atlas.Conta.Import1C.Flax` prin `--recreeaza`.
`Atlas.Conta.BackOffice.Privat`, baza probelor HTTP, e clona noului import
plus updater (re-seed pentru `Cititor`/`Configurator`), cu lanțul perioadelor
REDESCHIS integral: zero închideri, ca înainte de felie. `Flax.Api` e clona
importului integral.

(h) **Probele feliei țin regulile, pe ambele profiluri.** F28-A (seed
`TipDocument` ↔ clase concrete 1:1, discriminator == `ClrType`, abstractele
sărite), F28-B (niciun prefix: `GetColumnName() == Name`, toate pe tabela
rădăcinii, coloanele din `information_schema` == modelul), F28-C (indexul pe
`ClrType`, în model și în `pg_indexes`), F28-D (refuzul regulii (o), existent
și nou în același commit, id inexistent; țintele corecte trec), F28-E
(cititorul == `ClasaReala` pe toate cele 20 de tipuri concrete; `null` pe id
inexistent), F28-F (setter ne-public; `CreateObject` completează `ClrType` pe
cele 38 de tipuri concrete; nicio valoare NULL, goală sau fără clasă în bază),
F28-G (`ClrType` persistent, read-only, caption „Tip”, precondițiile D85-M2),
F28-H (liniile unui document sunt frunza lui, un subtip sau baza), F28-I
(ținta fiecărui FK spre frunză are discriminatorul corect; 9 FK-uri), F28-J
(coloanele derivate NULL pe rândurile altor tipuri; 74 de coloane), F28-K
(utilizatorul XAF nou + login-ul lui în același commit trec regula (o)),
F28-L…N pentru (i). Proba supremă a motorului rămâne neschimbată: Import1C
integral cu raport de reconciliere identic pe conținut sortat cu
baseline-ul.

(i) **Un rând după cheie, pentru un tip ne-rădăcină, se caută pe RĂDĂCINA
ierarhiei, iar tipul se verifică după.** Identity map-ul EF e per rădăcină în
orice strategie de moștenire. Cu `PreFetchReferenceProperties`, XAF
(`QueryIncludeGenerator.GetObjectByKey`) întoarce intrarea URMĂRITĂ fără să-i
verifice tipul. Deci `GetObjectByKey<Frunza>(id)` pe un id urmărit ca altă
frunză nu e o întrebare sigură: dă `InvalidCastException` (500) sau, în
forma negenerică, obiectul greșit, iar rezultatul depinde de starea
tracker-ului. Singura cale este `Motor/RandDupaCheie`:
- `Oricare` caută pe rădăcină și, dacă n-a găsit, pe tipul cerut, cu
  permisiunile lui;
- `Ca<T>` dă `null` și pe un rând de alt tip;
- `Cere<T>` adaugă refuzul.

Refuzul are o singură frază, `Refuzuri.RandDeAltTip`: „rândul ales (id) e X,
nu Y”. O folosesc `Rezolva.Cere/Optional`, gardianul (o), `DviFactura` și
`DocumentTrezorerie.ValideazaOperare`, deci răspunsul e același 422,
determinist, oricum ar fi tracker-ul. În `nou/Atlas.Conta.BackOffice` nu mai
există niciun `GetObjectByKey<Frunza>`. Probele:
- F28-L: `Rezolva` dă aceeași frază cu ținta urmărită și neurmărită;
- F28-M: DVI și latura pereche;
- F28-N: scanează sursa, ca `GetObjectByKey<Frunza>` să nu reapară.

---

## Context

Deciziile 3 și 16 mapaseră cele trei ierarhii TPT: `Document` (20 de
derivate pe două niveluri), `DocumentDetaliu` (12) și `Repartitor` (5). Pe
schema de dinainte de felie, 15 din cele 20 de tabele derivate ale lui
`Document` aveau ZERO coloane proprii; `DocumentDetaliu` avea 82 de coloane
brute pe 33 de nume distincte; `Repartitor` avea nume disjuncte (Partener 16,
ContPropriu 2, Angajat 1, Gestiune/UnitateInterna 0). TPT era, aici, un
discriminator implementat ca tabele cu o singură coloană, join-uite la
fiecare citire polimorfă.

Costurile erau documentate în `p5-perf-masuratori.md`:

- `CoduriTip` 1,4 s pe o lună de import și incidentul de 11 s: materializare
  polimorfă doar ca să se citească clasa CLR;
- `RegistruTva` în modul `Server`, 805 ms (EXPLAIN): INNER JOIN pe tabela
  derivată TPT a lui `DocumentDetalii`, cu 9 LEFT JOIN (85-r6);
- `CoduriTipPeTipuri`, ~19 interogări per proiecție SAF-T (75-r2);
- fiecare interogare pe bază din motor (dependenți, copii, storno, drafturi în
  perioadă, documente stinse) plătea 20 de LEFT JOIN-uri + `CASE`.

Zece locuri din cod afirmau „sub TPT nu există discriminator” și
materializau entități ca să afle clasa. Docs EF Core: „TPT shows inferior
performance when compared to TPH in many cases”. XAF EF Core nu are nicio
restricție pe TPH (restricțiile lui sunt cheile compuse, keyless, owned).

Motivul feliei, declarat: tipul devine dată pe rând; taxa de join dispare de
pe citirile pe bază, deci contractele IM-D4 „Relații” și „Politici” se scriu
și se măsoară pe maparea finală, fără ocoluri care ar deveni canonice;
greenfield asumat. Nu s-au schimbat ierarhia CLR, hook-urile, `[TipDetaliu]`,
dispecerizarea polimorfă sau invarianții. Motivul expunerii ca membru mapat
(owner, 2026-09-18): tipul vizibil în grile (compatibil cu
`Server`/`ServerView`), filtrabil prin OData și criterii XAF, folosibil în
`Appearance`, rapoarte și permisiuni pe obiect, punct de extensie pentru ce
e „per tip”.

Testul contra invarianților: **II** neatins — discriminatorul e etichetă, nu
comutator, iar coloana partajată nu devine contract al bazei (54c); **III**
întărit — `CoduriTip` nu mai are nevoie de excepția „mulțime mărginită”;
**IV** neatins — `TipDocument` rămâne ancora politicilor; 20 întărit — valoarea
discriminatorului e derivată din cod, nu un al doilea nomenclator; 23a
păstrat — lanțul se resetează, nu se ocolește; 54b/54c păstrate.

## Pașii executați

Un agent per pas, verificare independentă de main, commit per pas, review
advers la închidere. Branch `p5-f28-tph`.

- **Pasul 0 — spike-ul de mapare** (`4e9fd53`): TPH pe cele trei ierarhii,
  `AplicaColoanePartajate`, indexurile pe `ClrType`, cele 45 de migrații
  șterse, `InitialCreate` nou. Coloane: `Documente` 37, `DocumentDetalii` 45,
  `Repartitori` 28, niciun prefix; UN FK și UN index per coloană partajată,
  fără configurare de nume (`DocumentDetalii` 17 FK-uri, `Documente` 7,
  `Repartitori` 3). Valorile discriminatorului coincid cu seed-ul pe 20/20
  de tipuri concrete. Import1C `--recreeaza --pana-la 1` pe o bază de spike:
  9 min 47 s, luna 01/2025 identică pe conținut sortat cu baseline-ul.
  EXPLAIN: `RegistruTva` Server are 4 JOIN-uri (TPT avea 35), 1,38 ms pe
  baza de spike (necomparabil cu 805 ms: altă bază, alt volum).
- **Pasul 1 — bazele și ModelCheck** (`c5fd577`): bazele de dev recreate pe
  `InitialCreate`; probele dependente de forma TPT rescrise pe intenție
  (F21-D5: filtrul NTC e `"ClrType"` cu `'InchidereTva'` în SQL, nu tabela
  `InchideriTva`; D15-V1 pe `information_schema` al `Repartitori`; titlurile
  „sub TPT”; `Tabel == "Parteneri"` e eticheta de grup din
  `VerificareProfilService`, nu tabelă, deci rămâne); probele F28-A, B, C, F,
  G. Metadata: +41 `"ClrType": "Tip"`, idempotentă. ModelCheck bugetar
  1283/0, privat 1439/0.
- **Pasul 2 — tipul ca dată + gardianul FK** (`3c4193f`): `CititorTipDocument`
  și consumatorii mutați (f); regula (o) (e); probele F28-D și F28-E;
  comentariile false sub TPH tăiate sau reduse la `// 89`. ModelCheck bugetar
  1287/0, privat 1443/0; zero schimbare de sârmă.
- **Pasul 3 — probele supreme**: vezi §„Probele supreme (pasul 3)”.
- **Pasul 4 — review advers** (`461a212`), fixul defectului preexistent scos
  la iveală de el (`14bc649`, (i)) și docs.

Consumatorii mutați la pasul 2: `ApiProiectii.CoduriTip` (semnătura
păstrată, toți apelanții neatinși); SAF-T §7 și S3 pe cititor —
`CoduriTipPeTipuri` și `IdsDocumenteDeTip` au DISPĂRUT (75-r2 închisă);
`PerioadaService.DraftInPerioada` pe proiecție, fără materializare polimorfă;
`CorectieController.RefuzPeTipulConcret` pe proiecție + `Clasa`;
`OperareApi.Eticheta` pe `ClrType` (nu mai întoarce „…Proxy”);
`BackfillTva/Reconciliere` pe cititor și pe discriminator. Dezproxarea
inline din `CoduriTip` și `Reconciliere` a dispărut odată cu ei.

## Tranșări

**(1) Coliziunile se judecă pe tipul de STOCARE, nu pe tipul CLR
(amendament la textul F28-D2).** Contractul cerea excepție la „tip CLR sau
facete diferite”. Pe `DocumentDetaliu`, `Directie` (`DirectieAsamblare` /
`DirectieDiferenta`) și `Fel` (`FelLiniePif` / `FelLinieIesire`) sunt enumuri
diferite peste `integer`. A le despărți în coloane ar fi însemnat fie prefix
de tip (interzis de preferința owner-ului), fie redenumiri în model fără
motiv de domeniu. Frunza se citește doar prin tipul ei (b), deci coloana
partajată nu amestecă semanticile. Orice altă diferență rămâne excepție
zgomotoasă.

**(2) FK-ul discriminator → `TipDocument.ClrType` a căzut; fallback-ul
declarat în F28-D1 a fost ratificat.** Modelul și migrația treceau, iar
proba SQL confirma refuzul unui document fără ancoră. Import1C a picat însă
la seed, înaintea oricărui document: „Unable to track an entity of type
'TipDocument' because alternate key property 'ClrType' is null…”.
`IObjectSpace.CreateObject` face `DbContext.Add` imediat, cu cheia
alternativă încă nulă, iar același tipar îl folosesc seed-ul, „New” din XAF
și OData. Varianta cu placeholder `ClrType = ''` a fost PROBATĂ pe o bază de
unică folosință și RESPINSĂ: două `TipDocument` noi în același context se
ciocnesc pe cheie; un rând rămas cu `''` se salvează fără verificare (ar fi
cerut încă un CHECK); cheia alternativă e NEFILTRATĂ, deci re-seed-ul după o
ștergere logică dă `duplicate key` (probat); iar cheia nu mai poate fi
modificată deloc. Fără FK și fără navigație, cititorul (f) folosește forma
proiecției + a doua interogare pe `TipDocument`, iar unicitatea ancorei e
indexul unic filtrat `IX_TipuriDocument_ClrType`.

**(3) Indexul compus `(ClrType, DocumentId)` nu se pune.** A/B cu indexul
creat temporar: planificatorul nu-l alege pe nicio interogare
(`RegistruTva` Server, `DraftInPerioada`, liniile unei frunze pe document,
liniile unei frunze pe lună); planurile și timpii rămân aceiași.
`IX_DocumentDetalii_DocumentId` e deja selectiv, iar
`IX_DocumentDetalii_ClrType` acoperă scanările pe tip.

**(4) D4-V2 și D16-V2 (bugetar) își fac singure premisa anti-vacuitate.**
Recrearea bazei a scos la iveală un defect latent al probelor: își luau
premisa dintr-un draft orfan istoric al bazei. Fiecare își construiește acum
scena (`FctBugetaraOperata`: FCT cu linie CAP21 operată prin motor în 2026,
purjată la final); asertul de fond n-a fost atins.

**(5) Regula de citire a coloanelor de frunză a fost CORECTATĂ la review
(contractul greșea).** F28-D2 afirma că `(x as Frunza).Prop` e sigur în LINQ
fiindcă EF ar emite `CASE WHEN ClrType IN (…)`. Fals, demonstrat cu
`ToQueryString`: `as`/cast emite coloana goală; doar `is ? :` emite `CASE`.
Regula corectă e (b). Cele 7 comentarii „`CASE` pe discriminator” scrise la
pasul 2 și cele 2 despre „cast-ul pe frunză” au fost rescrise pe adevăr, iar
premisa care face sigure citirile existente a devenit probă (F28-H).

**(6) Integritatea tipului pe ușa de sistem se probează, nu se presupune.**
Sub TPT FK-ul spre o frunză ținea tabela frunzei, deci baza verifica tipul.
Sub TPH gardianul (e) acoperă doar ușa securizată. În loc de CHECK-uri sau
triggere, ModelCheck generează din metadata EF SQL-ul care caută rândurile
greșite (F28-I pe FK-uri, F28-J pe coloanele fraților), iar
`--dump-integritate-tph <cale>` scrie același SQL pentru bazele de import,
unde se rulează după proba supremă.

**(7) Ierarhia utilizatorilor XAF e tot TPH, iar regula (o) o prinde
legitim.** Descoperirea generică a găsit 9 FK-uri: cele 8 de domeniu
(`DviFactura.FacturaId`/`DviId`, `DocumentTrezorerie.LaturaPerecheId`,
`Lot.GestiuneId`, `Imobilizare.ResponsabilId`, `Societate.ContBancarId`,
`FacturaIesire.GestiuneDescarcareId`, `FacturaIntrare.PlataContPropriuId`)
plus `ApplicationUserLoginInfo → ApplicationUser`. Regula nu face excepție
pentru el; F28-K probează că fluxul real (utilizator nou + login-ul lui în
același commit) trece.

**(8) Privat rămâne clona bazei de import plus re-seed.** Așa era și
înainte. F28-D5 o recreează din noul import, cu updater-ul (re-seed pentru
`Cititor`/`Configurator`) și cu lanțul perioadelor redeschis integral, adică
fără nicio închidere: starea pe care o presupun probele din `refuzuri.ps1`.
Prima rulare, pe o clonă cu 12/12 luni închise, a dat 292/294. Cele două
eșecuri țin de mediu, nu de cod: #278 cere „nicio perioadă închisă”, iar #70
are un subiect DVI dependent de conținut.

**(9) Defectul de căutare după cheie era PREEXISTENT, nu adus de TPH; s-a
fixat în felie (`14bc649`).** L-a scos la iveală o investigație pornită din
review, pe întrebarea „ce întoarce `GetObjectByKey` pe un id de alt tip”.
Identity map-ul EF era per rădăcină și sub TPT, iar prefetch-ul XAF întoarce
intrarea urmărită fără să-i verifice tipul, pe ambele hosturi. Căile reale
afectate:
- `POST /api/fcl` cu `GestiuneDescarcareId = PrimitorId`;
- FCT cu `PlataContPropriuId = PredatorId`;
- TRZ cu `LaturaPerecheId` = documentul-sursă prefetch-uit;
- `DviFactura{DviId = X, FacturaId = X}` (acolo regula entității rula
  înaintea regulii (o)).

Cu ținta neurmărită, aceleași cereri dădeau 422 „nu există”; cu ea urmărită,
500 sau obiectul greșit. Fixul nu e o verificare locală în fiecare loc, ci o
singură cale (i): toate `GetObjectByKey<Frunza>` din Module și WebApi trec
prin `RandDupaCheie.Ca<T>` (scanarea acoperă 93 de apeluri). Ocolișurile din
`DviApply`/`ExplicaApply` și comentariile despre capcană au dispărut. F28-L,
M și N PICAU toate trei pe codul vechi. `refuzuri.ps1` a primit 9 probe HTTP
noi: `Imobilizare.ResponsabilId` ×3, `Societate.ContBancarId` ×5 și FCL cu
`GestiuneDescarcareId = PrimitorId` ×1.

## Review advers

Agent separat, pe scenariile din contract (cast pe frunză fără filtru în SQL
brut; FK spre frunză cu id de alt tip prin API și OData;
`GetObjectsQuery<NotaContabila>` pe un ITV; coloană partajată citită prin
bază; discriminator fără ancoră; corecția unui document cu linii de tip bază;
`Purja` pe ierarhii; securitate pe tip cu `GetObjectByKey<Document>`).
Verdict: **0 MAJOR, 2 MEDIU, 2 MINOR, 3 observații**, toate tratate în
`461a212`, niciuna cerând migrație.

| # | Sev. | Constatarea | Tratarea |
|---|---|---|---|
| R1 | MEDIU | `as`/cast pe frunză NU filtrează pe tip sub TPH; contractul și 9 comentarii afirmau contrariul | regula (b) corectată; comentariile rescrise; F28-H probează premisa citirilor existente |
| R2 | MEDIU | ușa de sistem pierduse verificarea de tip din bază pe FK-urile spre frunze | `ModelCheck/IntegritateTph.cs`: F28-I (9 FK-uri) și F28-J (74 de coloane), SQL din metadata; `--dump-integritate-tph` pentru bazele de import |
| R3 | MINOR | `FkSpreFrunze` sărea tăcut FK-urile compuse sau spre cheie alternativă | aruncă zgomotos pe o formă pe care regula (o) n-o poate verifica |
| R4 | MINOR | regula (o) pe ierarhia utilizatorilor XAF neprobată pe fluxul real | F28-K |
| O1 | obs. | ținta ștearsă logic a unui FK spre frunză e refuzată ca invizibilă | acceptat: e comportamentul oricărei referințe spre un rând șters logic |
| O2 | obs. | backstop-ul coliziunilor | validatorul EF `ValidateSharedColumnsCompatibility`; compararea nullabilității în gardian e alegere deliberată |
| O3 | obs. | S3 din SAF-T trimite toate id-urile istoricului prin `= ANY` | corect semantic; perf măsurat la pasul 3: 78.287 de id-uri costă 16,3 + 41,6 ms, 105.530 costă 20 + 55 ms; costul de 1,4–1,8 s din addendumul 6 NU revine |

Investigația pornită din scenariul „securitate pe tip cu
`GetObjectByKey<Document>`” a găsit defectul preexistent din tranșarea (9),
demonstrat empiric și fixat în `14bc649` (regula (i)).

## Cifrele

| | la deschidere (88) | pasul 1 | pasul 2 | review | la închidere (`RandDupaCheie`) |
|---|---|---|---|---|---|
| ModelCheck bugetar | 1278 / 0 FAIL | 1283 / 0 | 1287 / 0 | 1291 / 0 | **1294 / 0 FAIL** |
| ModelCheck privat | 1434 / 0 FAIL | 1439 / 0 | 1443 / 0 | 1447 / 0 | **1450 / 0 FAIL** |
| `refuzuri.ps1` | 285 / 285 | | | | 294 / 294 (9 probe noi) |
| Migrații în lanț | 45 | 1 | 1 | 1 | 1 (`20260918113542_InitialCreate`) |
| Coloane `Documente` / `DocumentDetalii` / `Repartitori` | TPT, 20 + 12 + 5 tabele derivate | 37 / 45 / 28 | | | 37 / 45 / 28 |

La fiecare pas: `has-pending-model-changes` curat, build 0/0 și zero
schimbare de sârmă.

## Probele supreme (pasul 3)

Regula de oprire F28-D8. Artefactele sunt în `run-f28/pas3/`.

- **Import1C integral** pe `Atlas.Conta.Import1C.Flax`, `--recreeaza
  --cititori --inchide-lunile`, pe codul `3c4193f`:
  - 15:45:04 → 19:06:45, exit 0, „CONTRACT INDEPLINIT, 932 avertismente”;
  - 03:21:20 în total, față de 2 h 14 min la F27. Diferența e o pauză de
    ~1 h 20 min la începutul lunii 09/2025, pusă pe presiunea de memorie a
    mașinii (4,7 GB liberi din 32). Ritmul pe lună a fost identic cu F27,
    ~8–9 min;
  - `--reclasifica` după: exit 0 (23 de avertismente).
- **Raportul de reconciliere** `nou/tools/Import1C/reconciliere-20260918-154628.txt`
  (467 de linii) e **IDENTIC pe conținut sortat** cu baseline-ul
  `reconciliere-20260917-121343.txt`. Diferă doar antetul cu data.
- **Închiderile**: 12/12, fiecare cu „constatări acceptate: niciuna”, în 0,7 /
  1,2 / 1,9 / 2,2 / 2,6 / 3,3 / 4,7 / 4,3 / 4,8 / 4,6 / 4,9 / 5,4 s.
  Snapshot-urile per lună sunt identice cu F27. Referința 12/2025 are 184.780
  de rânduri contabile, 7.914 de stoc și 201.046 de partide.
- **`SolduriService.Reconstruieste`**: **0 diferențe** (contabil 184.780,
  stoc 7.914, partide 201.046), în 10,4 s.
- **Integritatea TPH pe baza de import** (`integritate-tph.sql`, scris de
  `--dump-integritate-tph` și rulat pe Flax):
  - 103 interogări, **0 încălcări**;
  - 14.206.963 de rânduri verificate: linii 337.596, FK-uri 47.155, coloane
    13.822.212;
  - 15 verificări sunt vacue pe import, fiindcă importul nu are
    AMO/DEC/DVI/CAS/PIF/BPR, `DviFactura`, latură pereche, gestiune de
    descărcare, plată din cont propriu, responsabil de imobilizare sau
    login-uri. Pe acestea le acoperă ModelCheck, pe scenă.
- **`Flax.Api`** e recreată ca clonă a importului integral. **Privat** e clonă
  + updater, cu lanțul redeschis integral.
- **`refuzuri.ps1`** pe host viu Privat, după re-seed: 294/294 (prima rulare pe clona cu 12/12 închise a dat 292/294 — #278 și #70 de mediu, nu de cod; după redeschiderea integrală a lanțului pe Privat și descoperirea subiectului #70 pe fereastra care conține un furnizor `NeinregistratRo`: 294/294, `run-f28/pas4/refuzuri.log`).
- **`verifica:drift`**: exit 0, fără drift. **`pnpm build`**: exit 0.
- **Perf A/B** (metoda și tabelul complet în `p5-perf-masuratori.md` §Felia 28):
  - A = clona înghețată `Atlas.Conta.Import1C.Flax.TPT` pe codul `ba20faa`;
    B = `Flax.TPH` pe `14bc649`;
  - aceeași mașină, una după alta, aceeași stare: 11 luni închise, 12/2025
    deschisă, referința 11/2025 = 171.396 / 7.790 / 184.458;
  - `VACUUM ANALYZE` pe ambele, câte două treceri pe fiecare parte, cu hostul
    repornit. Zgomotul e ±5–10 %.

| Probă | A: TPT (trecerea 1 / 2) | B: TPH (trecerea 1 / 2) | Δ | Verdict |
|---|---|---|---|---|
| fișa contului `4111`, decembrie | 144 / 126 ms | 61 / 55 ms | −57 % | mai bun |
| balanța analitică, decembrie | 211 / 206 ms | 201 / 192 ms | −5 % | zgomot |
| `sold-parteneri` | 212 / 218 ms | 220 / 206 ms | — | zgomot |
| `sold-stoc` | 52 / 54 ms | 57 / 50 ms | — | zgomot |
| balanța sintetică, decembrie | 55 / 53 ms | 48 / 44 ms | −15 % | mai bun |
| operarea unui FCT cu 49 de linii (în proces) | 404 / 424 ms | 230 / 199 ms | −48 % | mai bun |
| `documente-cu-rest` | 176 / 188 ms | 169 / 158 ms | −8 % | zgomot / ușor mai bun |
| `CoduriTip`, extrasul cu 335 de stingeri, HTTP | 214 / 247 ms | 23 / 23 ms | ≈ 10× | mai bun |
| `CoduriTip`, în proces | 35 ms | 1,6 ms | ≈ 20× | mai bun |
| `RegistruTva` `Server`, EF în proces | 875 ms | 14,5 ms | ≈ 60× | mai bun |
| `RegistruTva` `Server`, EXPLAIN ANALYZE | 1.099 / 1.703 ms | 12 ms | ≈ 100× | mai bun |
| D406 S 09/2025, în proces | 2,5 s | 2,5 s | — | zgomot |
| D406 S 12/2025, în proces, la RECE | 2,7 s | 3,0 s (intercalat 2,7 → 2,9) | +0,2 s (+7 %) | mai prost DOAR la rece — F28-r5 |
| D406 S prin HTTP, cald | 09: 1224–1298 ms; 12: 1232–1274 ms | 09: 1081–1198 ms; 12: 1243–1277 ms | — | zgomot |

**EXPLAIN pe cifrele atribuite TPT.**

- `RegistruTva` `Server`: A are 42 de JOIN-uri și Hash Join pe cele 338.594
  de rânduri din `DocumentDetalii`, cu planificare 15–18 ms și execuție
  1,1–1,7 s. B are 4 JOIN-uri, top-N heapsort și Nested Loop pe PK, cu
  planificare 2 ms și execuție 12 ms.
- `CoduriTip`: A materializează polimorf (`= ANY` cu 77 de JOIN-uri, bind
  41 + execuție 25 ms, seq scan pe toate frunzele). B face
  `SELECT "ID","ClrType" … = ANY`, cu Index Scan pe PK, 0,46 + 0,2 ms. Pe
  toată cererea, SQL-ul scade de la 127 + 30 ms la 2,6 + 1,1 ms.
- D406: în A, rezoluția tipului costa 20 de listări per tip, adică 60 de
  instrucțiuni și 166,8 ms. SQL-ul total al proiecției scade de la 675 la
  525 ms (12/2025) și de la 637 la 480 ms (09/2025).

**Verdictul A/B.** Nicio cifră caldă nu e mai proastă peste zgomot. Singura
regresie e D406 S pe 12/2025 la RECE, +0,2 s, și are cauza demonstrată: e un
cost unic per proces.
- În ACELAȘI proces, prima chemare costă 2509/2552 ms în A și 2621/2707 ms
  în B.
- Medianele chemărilor calde sunt 1553/1706 ms în A și 1621/1659 ms în B,
  adică egale; SQL-ul e mai mic în B.
- Cauza probabilă e JIT-ul sau compilarea EF a formei noi de interogare, dar
  nu e izolată prin profil. Conform F28-D8, rămâne restanță cu nume: F28-r5.
- Ieșirea D406 e identică între A și B după normalizarea GUID-urilor.

## Ce rămâne deschis (restanțele deciziei)

Restanțele poartă numele din contract (`F28-rN`).

- **F28-r1** simplificarea `CandidatiPereche<T, TOpus>` și a uniunii per tip
  din `ImperecheriProiectii`: jumătatea „tip” e rezolvată de discriminator,
  jumătatea „contrapartida per tip” e semantică, nu mapare. Se rezolvă
  împreună cu F27-r16.
- **F28-r2** index unic pe `Repartitor.Cod` (81-r4): mecanismul a devenit
  trivial — toate familiile stau fizic pe aceeași tabelă —, dar datele de
  import au coliziuni legitime între familii.
- **F28-r3** partiționarea sau `CLUSTER` pe `ClrType`: fără cifră care s-o
  ceară.
- **F28-r4** dezproxarea tipului rămâne în patru copii
  (`MotorOperare.ClasaReala`, `Refuzuri.TipReal`, în `GardianEditare`
  `VerificaCodDenumire` inline și `TipDomeniu`). F28-D6 cerea reducerea la
  cele două semantici existente; au dispărut doar copiile inline din
  `CoduriTip` și `Reconciliere`, odată cu consumatorii lor. Se reduce la
  atingerea gardianului.
- **F28-r5** D406 S la rece: +0,2 s (+7 %) pe 12/2025, cost per proces. SQL-ul
  e mai mic în B, iar chemările calde sunt egale. Cauza (JIT sau compilarea EF
  a formei noi de interogare) nu e izolată prin profil.
- Observație, nu restanță: câteva sub-puncte ale unor decizii active își
  motivează regula pe mecanica TPT (63, 77a, 80, 81a, 86 — „404 automat
  (TPT)”, „`GetObjectByKey` sub TPT aruncă pe proxy-ul altei frunze”, „coloană
  pe baza TPT”). Efectul lor e acoperit de probele existente (ModelCheck
  verde pe ambele profiluri, `refuzuri.ps1`) și de scenariile review-ului
  (`GetObjectsQuery<NotaContabila>` pe un ITV, `GetObjectByKey<Document>`).
  Motivarea din textul lor e istorică și nu se rescrie. La 81a („`GetObjectByKey`
  sub TPT aruncă pe proxy-ul altei frunze”), cauza reală era identity map-ul
  per rădăcină, independent de strategie; regula de azi e (i).
