# Dezvoltare și validare

**Actualizat: 2026-09-18.** [Index](README.md)

## Organizarea sursei

| Zonă | Responsabilitate |
|---|---|
| `nou/Atlas.Conta.BackOffice/Atlas.Conta.BackOffice.Module` | Model, motor, DTO/Apply, proiecții, ANAF, SAF-T, seed și migrări (42d) |
| `nou/Atlas.Conta.BackOffice/Atlas.Conta.BackOffice.WebApi` | Contracte HTTP, securizarea comenzilor, OData și integrarea hostului (42f) |
| `nou/Atlas.Conta.BackOffice/Atlas.Conta.BackOffice.Blazor.Server` | Host XAF, administrare și actualizarea explicită a bazei (23a) |
| `nou/Atlas.Conta.Client` | React, formulare, raportare și contractele generate (43e) |
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
```

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
| Documentație | Concordanță cu implementarea, link-uri locale și diff |

ModelCheck verifică modelul și execută scenarii de integrare, inclusiv probe
pure pe funcțiile de potrivire și de seed. Probele mapării TPH (`F28-A…K`)
țin: seed-ul `TipDocument` ↔ clasele concrete, 1:1; coloanele fără prefix de
tip și schema bazei egală cu modelul; indexul pe `ClrType`; refuzul
gardianului pe un FK spre frunză cu ținta de alt tip; cititorul de tip egal
cu clasa reală pe toate tipurile; `ClrType` completat de EF și nescriibil
din cod; `ClrType` read-only în modelul aplicației; liniile unui document
de tipul declarat de el; ținta fiecărui FK spre frunză de tipul corect;
coloanele frunzelor NULL pe rândurile altor tipuri. Ultimele trei rulează
SQL generat din metadata EF (`IntegritateTph.cs`), iar
`ModelCheck --dump-integritate-tph <cale.sql>` scrie același SQL pentru a fi
rulat pe o bază de import, pe care ModelCheck nu o atinge. (89e, 89h)

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
