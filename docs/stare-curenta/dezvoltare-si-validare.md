# Dezvoltare și validare

**Actualizat: 2026-09-22.** [Index](README.md)

## Organizarea sursei

| Zonă | Responsabilitate |
|---|---|
| `nou/Atlas.Conta.BackOffice/Atlas.Conta.BackOffice.Module` | Model, motor, DTO/Apply, proiecții, ANAF, SAF-T, seed și migrări (42d) |
| `nou/Atlas.Conta.BackOffice/Atlas.Conta.BackOffice.WebApi` | Contracte HTTP, securizarea comenzilor, OData și integrarea hostului (42f) |
| `nou/Atlas.Conta.BackOffice/Atlas.Conta.BackOffice.Blazor.Server` | Host XAF, administrare și actualizarea explicită a bazei (23a) |
| `nou/Atlas.Conta.Client` | React, formulare, raportare și contractele generate (43e) |
| `nou/Atlas.Conta.Nucleu` | Nucleul pur al cubului de postări: tipuri, conservare, unitate, FIFO, evaluare, repartizare, TVA, storno, motor, gestiunile virtuale — fără niciun pachet; consumat DOAR de `Module` (`Declaratii/`), referit direct și de ModelCheck ca unealtă (90b, 90l) |
| `nou/Atlas.Conta.Nucleu/Atlas.Conta.Nucleu.Teste` | Invarianții nucleului ca proprietăți pe generatoare proprii și testul de arhitectură (90l) |
| `nou/tools/ModelCheck` | Verificarea modelului și scenarii de domeniu pe PostgreSQL (23) |
| `nou/tools/ProbeHttp` | Probe ale contractului HTTP și ale permisiunilor reale (80i, 81j) |
| `nou/tools/Import1C` | Import operațional și reconcilierea sursei (45f) |
| `nou/tools/Migrare` | Prototipul migrării nomenclatoarelor și soldurilor legacy (34, 35a) |
| `legacy`, `db` | Dovezi despre aplicația și datele vechi (21, 35b) |

Într-un repository cu `.codegraph/`, explorarea codului începe cu CodeGraph.
Sursa curentă rămâne autoritatea când indexul semnalează informații depășite.
Lipsa indexului nu cere crearea lui automată.

Terminatorii de linie sunt LF în tot repo-ul, impuși de `.gitattributes`
(`* text=auto eol=lf`); CRLF doar pe `*.cmd`/`*.bat`. `legacy/` și
`db/export/` sunt `-text`: octeții evidenței rămân neatinși. `.editorconfig`
cere LF editorilor. Uneltele care scriu CRLF (`dotnet ef`, template-urile VS)
nu produc diff, fiindcă git normalizează la `add`. Commit-ul de normalizare
e în `.git-blame-ignore-revs`.

## Model și persistență

EF Core Migrations este mecanismul de evoluție a schemei. Actualizarea
automată a schemei prin XAF este dezactivată. Module este comun celor două
hosturi; schimbările incompatibile se livrează coordonat. (23a, 42f)

Lanțul de migrații a fost resetat la 2026-09-18: singura migrație este
`20260918113542_InitialCreate`, generată din modelul TPH. Migrațiile de
dinainte sunt istorie în git, nu în lanț; bazele create pe lanțul vechi nu
se actualizează, se recreează. Lanțul nou crește prin migrații, ca înainte.
Comanda `dotnet ef` primește mereu `--context BackOfficeEFCoreDbContext` și
se rulează fără `--no-build`. (23a, 89g)

Bazele de dezvoltare se recreează astfel:

| Bază | Recrearea |
|---|---|
| `Atlas.Conta.BackOffice` (bugetar) | `dotnet ef database update`, apoi seed prin Blazor `--updateDatabase --forceUpdate --silent`, cu `ProfilContabil` în mediu |
| `Atlas.Conta.ModelCheck.Privat` | o recreează ModelCheck |
| `Atlas.Conta.Import1C.Flax` | Import1C `--recreeaza` |
| `Atlas.Conta.Import1C.Flax.Api` | clonă a bazei de import după importul integral |
| `Atlas.Conta.BackOffice.Privat` | clonă a bazei de import, plus updater prin Blazor (rolurile și utilizatorii `Cititor`/`Configurator` pentru `refuzuri.ps1`), cu lanțul perioadelor redeschis integral: probele HTTP presupun zero închideri |

(89g)

Cele trei ierarhii (`Document`, `DocumentDetaliu`, `Repartitor`) sunt TPH:
câte o tabelă pe rădăcină, discriminatorul `ClrType` cu valorile implicite
ale EF și index pe el. Proprietățile omonime ale frunzelor-surori împart
coloana, prin bucla generică `AplicaColoanePartajate`; nu există
configurare de coloană per proprietate. Două proprietăți omonime cu tip de
stocare sau facete diferite opresc construirea modelului, cu toate
coliziunile într-un singur mesaj; se redenumește una dintre ele, nu se
adaugă prefix. O clasă nouă cu nume mai lung decât coloana discriminatorului
produce singură o migrație `ALTER COLUMN`. (89a, 89b, 89c)

Actualizarea bazei se execută explicit prin hostul Blazor, cu opțiunile
`--updateDatabase --forceUpdate --silent`. WebApi verifică compatibilitatea
și nu devine al doilea updater automat. Baza țintă se verifică înainte de
orice comandă care aplică migrări sau seed. (23a, 42f)

Seed-ul este specific profilului și idempotent. Pe tipurile cu proveniență
trece printr-un singur helper (`ContaSeeder.Aliniaza`): caută rândul pe cheia
indexului unic, îl creează cu timbru dacă lipsește, îl aliniază la cod dacă
poartă timbrul seed-ului (câmpurile scalare ne-cheie, fiecare corecție
tipărită `tip / cheie / câmp: vechi → nou`), îl lasă neatins dacă e manual și
îl raportează dacă e șters logic. Un seed care ar schimba o cheie aruncă.
`Seed` întoarce `RaportSeed` cu contoare per tabel (create / corectate /
manuale / șterse); a doua trecere pe o bază aliniată nu creează și nu
corectează nimic. Câmpurile de stare de runtime (`PoliticaNumerotare.
UrmatorulNumar`) și cele deținute de alt pas al seed-ului (`TipTva.Activ`,
`ContImplicitId` derivat) nu intră în aliniere. Rândurile nomenclatoarelor de
nucleu fără proveniență (`RandD300`, `Judet`, `UnitateMasura`) se rescriu
autoritar; reseed-ul nu suprascrie datele societății. (69b, 73a, 83a–d, 84a)

Profilurile nu se amestecă în aceeași bază. `SetareProfil` și rotunjirea sunt
stabile după inițializare. (36c, 52a)

Unicitatea politicilor și a codurilor de nomenclator este în schemă, prin
indexuri unice filtrate pe `GCRecord = 0`: un rând șters logic nu este dublu,
iar cheia lui se poate reface. Ancora `TipDocument.ClrType` este unică tot
așa (`IX_TipuriDocument_ClrType`); discriminatorul documentelor nu are FK
spre ea, iar corespondența clase concrete ↔ seed o probează ModelCheck. (81c, 89a)

Absența FK-ului pentru `Lot.LinieIntrareId` este intenționată pentru ciclul de
inserare; integritatea este verificată de mecanismele domeniului. (26e)

Versiunile backend sunt centralizate în `Directory.Packages.props`.
Pachetele Atlas.DXF și DevExpress folosesc intervalul flotant al liniei de
versiune (`26.1.*`); versiunea restaurată trebuie să fie coerentă între
proiecte. (39a, 41e)
Clientul folosește pnpm și versiunile declarate în `package.json` și lockfile.

## Contracte generate și build

OpenAPI poate fi extras offline din configurația hostului, fără pornirea
serviciilor găzduite și fără baza de date. Metadata este derivată din model
prin instrumentul dedicat; nu se extrage dintr-un model de ecran XAF.
Artefactele generate sunt versionate împreună cu sursa care le definește. (43d, 56)

Comenzile uzuale, din rădăcina repository-ului, sunt:

```powershell
dotnet build nou/tools/ModelCheck/ModelCheck.csproj
dotnet build nou/Atlas.Conta.BackOffice/Atlas.Conta.BackOffice.WebApi/Atlas.Conta.BackOffice.WebApi.csproj
pnpm --dir nou/Atlas.Conta.Client build
pnpm --dir nou/Atlas.Conta.Client verifica:drift
dotnet test nou/Atlas.Conta.Nucleu/Atlas.Conta.Nucleu.slnx
dotnet run --project nou/tools/ModelCheck --no-build -- --scenarii BCS,FCT privat
```

`ModelCheck --scenarii <TIP>[,<TIP>…] [privat]` rulează doar scenele
catalogului care probează tipurile cerute, pe baza profilului (migrare +
seed pe privat, migrațiile aplicate pe bugetar), fără metadata, fără
probele de model și fără celelalte scene: toate tipurile de pe cub în
~15 s pe privat. Codurile sunt ale catalogului (`docs/nucleu/scenarii/`,
inclusiv `DESCHIDERE`, `IMO`, `X`); un cod necunoscut sau un tip fără nicio
scenă pe profilul cerut iese cu exit 2, nu verde. Scenele și tipurile lor
stau în `ScenelePeTip` (`Program.cs`), aceeași listă pe care suita integrală
o rulează în ordine; un scenariu nou intră acolo cu tipul lui, altfel filtrul
nu-l vede. Suita integrală pe ambele profiluri rămâne gate-ul de commit.
(091 (e), 091-r1)

Verificarea de drift regenerează contractele și refuză diferențele față de
fișierele versionate. O schimbare intenționată de contract se regenerează și
se examinează înainte de includerea artefactelor în modificare. (43d, 56)

## Verificări proporționale cu modificarea

| Schimbare | Verificare necesară |
|---|---|
| Model, motor, politici, proiecții | Build și ModelCheck pe ambele profiluri (23) |
| DTO, atribute, expunere API | Build WebApi, regenerare și verificarea contractelor; probe HTTP pentru comportamentul afectat (56, 80i) |
| Autorizare | Probe HTTP cu rolurile reale; o probă pe context nesecurizat nu demonstrează securitatea (80i, 81j) |
| Formular sau interacțiune | Build client și verificarea fluxului în browser (66) |
| Mod de acces al unui ListView XAF, proprietate nouă afișată în liste | ModelCheck (`D85-M1`, `D85-M2`, `D85-R1…R3`) și deschiderea listei în browser pe baza de import: sort, filtru, grupare, detaliu din listă, culegere pe document nou (85h) |
| Import sau schimbare amplă de postare/evaluare | Import și reconciliere față de baza de referință (54) |
| Tip derivat nou, proprietate nouă pe frunză, FK spre o frunză | ModelCheck pe ambele profiluri (`F28-*`); după un import, `--dump-integritate-tph` rulat pe baza de import (89e, 89h) |
| Nucleul pur (`Atlas.Conta.Nucleu`) | `dotnet test` pe soluția nucleului: testul de arhitectură și invarianții 1–6 ca proprietăți (≥ 500 de cazuri fiecare); ModelCheck doar dacă e atins `Module` (90l) |
| Declarant, operand, `Fapte.Operand`, oracolul pilotului | ModelCheck pe AMBELE profiluri: probele `NUC-*` (egalitate exactă cu registrele normalizate, conservare, determinism, `≤ 16` interogări per operand) plus `Metadata clientului e la zi` (o proprietate nouă pe `Document` intră în metadata clientului — de aceea `Declarant()` e metodă) (TR-D6b) |
| Entitățile sau migrațiile cubului (`Postare`, `Tranzactie`) | ModelCheck pe ambele profiluri: probele `STR-SCHEMA-*` (partiționarea LIST, cheia `(Spatiu, ID)`, setul ÎNCHIS de FK-uri per partiție, indexii, absența timbrelor XAF); migrația se scrie în SQL, nu se lasă generată (S-D2, S-r4) |
| Contractul laturilor (`Document.Laturi()`, T-D13) | ModelCheck pe ambele profiluri, ultima scenă (`VerificaLaturi`): `STR-LATURI-CONTRACT` (fiecare `TipDocument` din seed → clasa → contract cu părți nevide; metoda e abstractă, deci și compilatorul o cere), `STR-LATURI-REFUZ` (latura de partea greșită refuzată pe ușa declarației și pe ușa entității cu ACEEAȘI linie `COD: mesaj`; calitatea lipsă numită; un tip fără declarant refuzat pe ușa entității), `STR-LATURA` (PLT inversată = doar `PREDATOR_NEPOTRIVIT`, înaintea declarantului). Probele de laturi ale tipurilor asertează CODUL, nu textul vechi. Pe date reale: recensământul laturilor pe clona Flax (contract T-D13) și gate-urile pașilor 1–2 fără refuz nou |
| Materializare, declarant al unui tip migrat, împerecherea ca `Transfer` | ModelCheck pe ambele profiluri: probele `STR-*` pe scenele BCS, Trezorerie și FCT — operare, roundtrip, storno, anulare, refuz, configurație, poziție, transfer, latură, corecție, reconciliere — cu comutarea locală a regimului (`ProbeCub.Migrat`/`Nemigrat`/`CuToleranta`, cu restaurare) și purja rândurilor de cub ale documentelor scenei (S-D8) |
| Tip trecut pe `PosteazaInCub` | fișierul tipului din `docs/nucleu/scenarii/` complet și verde pe ambele profiluri (în lucru: `--scenarii <TIP>`; la commit: suita integrală): ciclul 1–8 (operare, linii multiple, storno în perioadă și peste graniță, anulare, corecție în perioadă închisă, stingere, citiri) + cazurile-limită aplicabile + lanțurile `SC-X-*` care îl ating; așteptările scrise de mână din regula contabilă, nu din registre sau oracol (091 (a)–(c)). `--declaratie-pe-baza` și `--reconciliere-cub` rămân unelte de diagnostic pentru migrare, nu gate (S-D9 amendat de 091) |
| Documentație | Concordanță cu implementarea, link-uri locale și diff |

ModelCheck verifică modelul și execută scenarii de integrare, inclusiv probe
pure pe funcțiile de potrivire și de seed. Probele mapării TPH (`F28-A…K`)
țin: seed-ul `TipDocument` ↔ clasele concrete, 1:1; coloanele fără prefix de
tip și schema bazei egală cu modelul; indexul pe `ClrType`; refuzul
gardianului pe un FK spre frunză cu ținta de alt tip; cititorul de tip egal
cu clasa reală pe toate tipurile; `ClrType` completat de EF și nescriibil
din cod; `ClrType` read-only în modelul aplicației, absent din layout-ul
oricărui DetailView și coloană vizibilă exact pe listele care amestecă
tipuri; liniile unui document
de tipul declarat de el; ținta fiecărui FK spre frunză de tipul corect;
coloanele frunzelor NULL pe rândurile altor tipuri. Ultimele trei rulează
SQL generat din metadata EF (`IntegritateTph.cs`), iar
`ModelCheck --dump-integritate-tph <cale.sql>` scrie același SQL pentru a fi
rulat pe o bază de import, pe care ModelCheck nu o atinge. (89e, 89h)

Nucleul pur se probează prin `Atlas.Conta.Nucleu.Teste` (xunit.v3,
154 teste): testul de arhitectură ține referințele assembly-ului la
`System.*`/`netstandard` și `.csproj`-ul fără `PackageReference`/
`ProjectReference`; invarianții 2–6 din `docs/nucleu/nucleu-cub-design.md`
§10 rulează ca proprietăți pe generatoare proprii (`Gen`, `Proprietate`:
sămânță fixă per caz, cazul picat se reproduce izolat), cu perturbări pe o
singură postare și cu contra-proba regulii vechi acolo unde regula nouă
diferă declarat (evaluarea pe raportul curent contra prețului înghețat).
Invariantul 7 (baseline-ul Import1C) nu e testabil în nucleu și rămâne
proba supremă a lui TR-D7/D10 (N-r1). Reflecția probează că niciun record
public n-are setter ne-`init`; egalitatea `Tranzactie`/`Declaratie`/
`Contract` e structurală. (N-D12)

Declaranții pilotului (BCS, PLT/INC, FCT) se probează în ModelCheck, pe
ambele profiluri, prin `ProbeNucleu.Proba` (`nou/tools/ModelCheck/Nucleu/`):
după operarea prin motorul vechi, documentul se contractează prin
`Contractare.Contracteaza` și postările lui se compară EXACT (multiset) cu
registrele scrise de motorul vechi, transformate în cub prin `CubDinRegistre`
(portul mapării fizicii) și normalizate DOAR prin funcțiile numite ale
diferențelor declarate (`Normalizari`, contractul TR-D6b B-D8); orice reziduu
al unei normalizări e avertisment tipărit și pică proba. Scenele proprii
(`VerificaNucleuBcs`, `VerificaNucleuTrezorerie`) își purjează documentele
(altfel `PAR-V*` văd partide în plus). Cifrele consemnate, nu normalizate:
`NUC-BCS-N-R3-*` (Δ = +25 pe lotul corectat) și `NUC-FCT-N-R4-*` (Δ = 0,01
pe taxa per document). Capcane: două ModelCheck-uri (sau un ModelCheck și un
`dotnet build`) concurente își blochează DLL-urile — o singură rulare o
dată, construită ÎNAINTE (`--no-build` pe un binar vechi probează codul
vechi); redirectarea `*>` din PowerShell scrie log-ul UTF-16 — rețeta
`run-nucleu/tr-d6b/pas4-final/run.sh` (bash) scrie UTF-8 și numără
`OK`/`FAIL`. (TR-D6b)

Gate-ul de reconciliere al cubului are două unelte, ambele în ModelCheck și
ambele ieșind înainte de bootstrap: (S-D9)

- `ModelCheck --declaratie-pe-baza <baza> <COD…> [--raport <director>]` —
  READ-ONLY, în loturi de 200 de documente cu ObjectSpace nou per lot:
  contractul declarantului contra oracolul registrelor normalizate, pe fiecare
  document operat al tipurilor cerute. Raportul dă, per tip: egale, refuzate pe
  cod cu id-uri exemplu, diferite pe fel de reziduu, excepțiile DECLARATE ale
  oracolului, histograma abaterii taxei culese și, pe tipurile de trezorerie,
  transferurile scrise, cele plafonate la restul partidei și cele sărite.
- `ModelCheck --reconciliere-cub <baza>` — SQL pe set, toleranță 0, pe
  tipurile cu `PosteazaInCub`: (a) Σ valoare per grup × cont × latură × lună,
  (b) Σ cantitate per lot × lună pe spațiul Stoc, din `Operare` ⊕ transferul
  de stoc (T-D2), (c) TVA per tip × sens × rol × perioadă, (d) Σ D = Σ C per
  carte în fiecare tranzacție, (e) per document operat al unui tip migrat cel
  mult o `Operare`, cel mult un `Transfer` de stoc (postări în spațiul Stoc) și
  cel puțin una din ele, și niciuna dintre cele două pe celelalte (T-D2, T-r1),
  (f) Σ per partidă la ultima perioadă închisă, (g) TVA pe postările de storno.
  Gate-ul `--declaratie-pe-baza` pe BTR se citește „100 % egal în afara celor
  535 declarate în T-D2.2" (oracolul pliază rândurile de stoc ± ale aceluiași
  lot, fără picior contabil, într-un `Transfer`); pe DSC „100 % egal în afara
  celor 842 declarate în T-D4.2" (normalizarea T-D4.1: piciorul contabil fără
  stoc al unei linii care doar iese își pierde gestiunea în oracol, fiindcă în
  cub e pe gestiunea virtuală `Client`; normalizarea T-D13: același picior
  primește terțul de pe primitorul extern, pe care rândul vechi nu-l poartă —
  numărată în `Normalizari.Contoare` și tipărită de gate ca
  `Normalizari.Contoare ×n`, spre deosebire de avertismente, care pică
  probele); pe FCL 100 % egal. FCL și DSC sunt
  fiecare grupul lui (DSC nu e conex). Grupul unui FCT e documentul ∪ NIR-ul lui conex; grupurile cu
  conex neoperat se RAPORTEAZĂ separat, nu se numără ca Δ. Litera (f) e vacuă
  cât timp un tip nemigrat mai postează pe conturi cu `RolTert`, iar nota se
  tipărește.

Rețeta probei supreme e `run-nucleu/tr-d7a/import/run.ps1`: Import1C integral
(`--recreeaza --cititori --inchide-lunile`), apoi `--reclasifica`,
`--reconciliere-cub`, `--dump-integritate-tph` și `diff-sortat.py`, care compară
raportul de reconciliere cu baseline-ul pe conținut sortat.

Capcane măsurate ale acestor probe: o SINGURĂ rulare ModelCheck o dată — două
concurente crapă în purje și lasă reziduu (`TipuriMaterial` cu codul
`E2E-SAFT-S-TIP` plus `RegulaContare` `DinSeed` pe care seeder-ul i-o
re-atașează la fiecare rulare), care blochează definitiv rulările următoare pe
acea bază până e șters manual (S-r10). Interogările pe catalogul Postgres cer
cast explicit: `partattrs` e `int2vector` indexat de la 0, `conkey` e `int2[]`
de la 1, iar `partstrat` e `"char"` și cere `::text` înainte de concatenare,
altfel interogarea pică și oprește rularea. O clonă a bazei de import poartă
valoarea DEFAULT a unei coloane noi, nu valoarea de seed (`TolerantaTaxa` 0
contra `null`): se aliniază înainte de gate, altfel refuzurile sunt ale bazei,
nu ale codului. Gate-ul re-contractează documente pe o bază cu perioadele deja
închise; artefactul „document datat exact la sfârșitul perioadei de referință
refuzat `STOC_INSUFICIENT`" (felia 31) a dispărut la T-D4.3: citirea fără
documentul curent ia referința strict înaintea datei. Oracolul citește registrele ORDONAT pe
(document, poziția liniei, linie, id), altfel ordinea heap-ului schimbă
nominalizarea între rulări și aceeași probă alternează OK/FAIL.

Căutarea după cheie a unui tip ne-rădăcină trece prin `RandDupaCheie`
(rădăcina ierarhiei, apoi tipul verificat), niciodată prin
`GetObjectByKey<Frunza>`: cu prefetch-ul XAF, acela întoarce intrarea
urmărită fără verificarea tipului. F28-L (aceeași frază cu ținta urmărită și
neurmărită), F28-M (DVI și latura pereche) și F28-N țin regula. F28-N e o plasă
pe SURSĂ: scanează `nou/Atlas.Conta.BackOffice` și pică la orice apel
`GetObjectByKey<T>` cu `T` tip ne-rădăcină. (89i) Nu are strategie de securitate
XAF; autorizarea se probează prin `nou/tools/ProbeHttp/refuzuri.ps1`, cu
rolurile Admin, Cititor, User și Configurator. (80i, 81j, 84c)

Modurile de acces ale listelor XAF sunt probate pe modelul REAL al
aplicației Blazor: ModelCheck construiește hostul cu `Startup` din
Blazor.Server pe calea `--updateDatabase`, fără circuit
(`ModelAplicatie.cs`, `D85-M0`) și numără comenzile SQL printr-un
interceptor (`NumaratorSql.cs`). Probele: modul fiecărui view (`D85-M1`),
precondițiile oricărui `ServerView`/`InstantFeedbackView` — coloane și
`DefaultProperty` mapate sau calculate, fără cast pe selecție, fără regulă
Appearance pe membru nevizibil (`D85-M2`) — o pagină `ServerView` cu
`Lot.Eticheta` calculată identică cu C#, inclusiv rotunjirea (`D85-R1`), o
pagină `Server` = un query plus COUNT (`D85-R2`) și grila nested `Client`
care vede liniile nesalvate (`D85-R3`). (85h)

ModelCheck referă proiectul Blazor.Server: build-ul lui pică pe DLL-uri
blocate cât timp hostul Blazor rulează din același `bin` (același tipar ca
`verifica:drift` cu WebApi pornit). Se oprește hostul înainte de build. (85h)

**ModelCheck scrie în baze de date.** Profilul bugetar implicit folosește
baza configurată de aplicație (`Atlas.Conta.BackOffice` în configurația
curentă); profilul privat folosește baza dedicată
`Atlas.Conta.ModelCheck.Privat`. Nu se tratează ca suită izolată, sigură de
rulat pe orice configurație. Conexiunea și compatibilitatea schemei se
verifică înainte de execuție. Două rulări concurente pe aceleași baze se
strică reciproc; variabila de mediu `MODELCHECK_BAZA_SUFIX` mută ambele baze
pe un sufix (privatul se creează singur, bugetarul cere o clonă a bazei
aplicației). (84)

Lipsa bazei sau migrările neaplicate pot lăsa doar verificarea modelului
executată. Codul de ieșire singur nu dovedește rularea scenariilor; jurnalul
trebuie să confirme execuția lor și absența eșecurilor.

Scenariile își curăță datele marcate. Întreruperea procesului poate lăsa
documente care influențează alte scenarii; curățarea se limitează la datele
identificate ale testului, în baza verificată. Auditul se păstrează. (70e, 81h)

Procesele lungi se lansează cu jurnal și cod de ieșire capturat. Pe Windows,
procesele de fundal se lansează cu fereastra ascunsă. Validatorul DUK cere
un director temporar accesibil procesului. Jurnalele publicate nu includ
parole, tokenuri sau adrese de feed cu credențiale. (50d, 73f)

## Import operațional 1C

Importul lucrează într-o bază dedicată, cu mapări explicite și verificare
prealabilă. Documentele sunt operate prin motor în ordinea timestamp-ului
sursei; nu se copiază registre pentru a evita regulile domeniului.
Tranzacția operațională este per document. (45a, 47a, 50a)

`MigrareLegatura` asigură legătura și idempotenta importului. Recuperarea
după un commit fără legătură și reluarea drafturilor folosesc identitatea
stabilă de import. Configurația legacy nu este importată ca limbaj de
politici, iar instrumentele de migrare nu impun o bibliotecă de domeniu
comună cu aplicația veche. (45f, 47b, 50a)

Nomenclatoarele sunt create la nevoie. Identitatea materialului importat
ține cont de catalog și cont; lotul, de document × produs × cont. Mapările
de cont sunt explicite și se verifică înainte de import. (47d, 48c, 50a)

NTC este punte numai pentru cazurile permise explicit, nu fallback universal
pentru documente nerecunoscute. Transformările și transferurile sunt
clasificate în ASM, BTR sau NTC după faptul economic. FCL importată postează
venitul; DSC folosește loturile identificate de sursă. (49a, 49d, 75b)

`--inchide-lunile` închide fiecare lună imediat după importul ei, prin
`PerioadaService.Inchide` cu toate constatările curente acceptate (politica de
închidere a bazei de import coboară `ItvLipsa` la avertisment — politică, nu
ocolire). E proba supremă a soldurilor materializate: raportul de reconciliere
trebuie să rămână IDENTIC cu baseline-ul rulării fără închideri, adică importul
citește peste snapshot-uri, nu peste registrul integral, și scrie aceleași
cifre. Verificat 2026-09-17 pe anul 2025: 12/12 luni închise, 0 constatări per
lună, raport identic cu `reconciliere-20260914-164035.txt`, `Reconstruieste`
0 diferențe pe contabil, stoc și partide. (F27-D1, F27-D3)

Proba supremă rămâne aceeași după trecerea pe TPH: importul integral cu
`--recreeaza --cititori --inchide-lunile` trebuie să dea un raport IDENTIC pe
conținut sortat cu baseline-ul curent
(`nou/tools/Import1C/reconciliere-20260917-121343.txt`), iar
`Reconstruieste` 0 diferențe. După ea se rulează pe baza de import SQL-ul
din `--dump-integritate-tph`: zero rânduri pe fiecare interogare. Verificat
pe TPH la 2026-09-18: raportul `reconciliere-20260918-154628.txt` e identic pe
conținut sortat cu baseline-ul; 12/12 luni închise fără constatări;
`Reconstruieste` a dat 0 diferențe; integritatea TPH a dat 0 încălcări în 103
interogări. 15 dintre ele sunt vacue pe import (tipuri și legături pe care
importul nu le produce), iar pe acelea le acoperă ModelCheck. (89g, 89h)

**Din 2026-09-22 (091) proba supremă nu mai e importul, ci catalogul de
scenarii** (`docs/nucleu/scenarii/README.md`): așteptări scrise de mână,
ciclul complet per tip, lanțurile transversale, pe ambele profiluri; clona
Flax rămâne sursă de întrebări (recensământ, 091-r2), Import1C e felia de
migrare (091-r4), după „rotund" (091 (g)). Paragraful de mai jos e
istoricul probei până la felia 32, pasul 2b.

Pe cubul persistat proba supremă avea aceeași formă: importul integral cu
tipurile migrate marcate `PosteazaInCub` trebuie să dea exit 0, ZERO refuzuri
ale declarației, raport identic pe conținut sortat cu baseline-ul, 12/12 luni
închise, `Reconstruieste` 0 diferențe, `--dump-integritate-tph` 0 încălcări și
`--reconciliere-cub` 0 rânduri Δ pe toate literele; `refuzuri.ps1` se reface pe
clona privată a noului import. Măsurat la 2026-09-21: Import1C integral pe Flax (`--recreeaza --cititori --inchide-lunile`, 2026-09-21): exit 0, 1 h 57 min (3 h 21 min la felia 28), raportul `nou/tools/Import1C/reconciliere-20260921-035646.txt` IDENTIC pe conținut sortat cu baseline-ul feliei 28, ZERO refuzuri ale declarației, 12/12 luni închise cu 0 constatări, `--reconciliere-cub` 0 rânduri Δ pe (a)–(g) — (f) vacuă: cele 9 conturi cu rol de terț sunt atinse și de tipuri nemigrate —, integritatea TPH 0 încălcări în 107 interogări, cubul cu 70.373 tranzacții / 252.092 postări / 16.924 transferuri (PLT → FCT; INC → FCL fără transfer, FCL fiind nemigrat), `refuzuri.ps1` 294/294 PASS pe `Atlas.Conta.BackOffice.Privat` refăcută din import cu perioadele redeschise. (S-D10)

## Reconciliere și migrare legacy

Reconcilierea recitește PostgreSQL după operare. Compară pe luni conturile,
TVA-ul, creanțele/datoriile și stocul pe produs × gestiune. Toleranța numerică
este 0,005; o diferență neexplicată este eșec, nu motiv pentru ajustarea
ascunsă a valorii postate. Explicațiile trebuie sprijinite de documentele
sursei. (45e, 47e, 51d)

Rulajele pe lot nu sunt țintă când identitatea lotului nu este comparabilă
structural. Evaluarea exactă, excepția returului fiscal și efectele
retroactivității se verifică separat. Probele deliberate de sabotaj trebuie
să demonstreze că reconcilierea detectează abaterile. (45e, 47a, 75c)

Formula amortizării se reconciliază cu cifrele postate în 1C prin blocul
`RECONCILIERE-MF` din ModelCheck, condiționat de fixture-ul gitignored
`1C/mf/` (parametrii datați și rândurile lunare per activ): recalculul cu
`AmortizareService.CotaLunara` se compară lună cu lună, potrivirile și
diferențele se raportează cu activ, lună, așteptat și postat; blocul nu pică
pe diferențe, doar pe fixture malformat. Import1C nu generează documente de
imobilizări; după orice atingere a motorului de operare, raportul integral
trebuie să rămână identic cu baseline-ul. (87c, 87k)

Prototipul legacy migrează nomenclatoare și solduri de deschidere la granița
aleasă. Istoricul rămâne în sursă. Deschiderile contabile folosesc convenția
de cont de deschidere, iar soldurile terților nu sunt transformate în facturi
inventate. Legăturile de migrare fac reluarea identificabilă și idempotentă. (34a, 34b, 34d)
