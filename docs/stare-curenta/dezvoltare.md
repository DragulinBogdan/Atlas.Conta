# Dezvoltare

**Actualizat: 2026-10-15.** [Index](README.md)

Organizarea sursei, modelul și persistența, serverele și bazele de dezvoltare, migrațiile, codegen-ul și build-ul.

## Organizarea sursei

| Zonă | Responsabilitate |
|---|---|
| `nou/Atlas.Conta.BackOffice/Atlas.Conta.BackOffice.Module` | Model, motor, DTO/Apply, proiecții, ANAF, SAF-T, seed și migrări (42d) |
| `nou/Atlas.Conta.BackOffice/Atlas.Conta.BackOffice.WebApi` | Contracte HTTP, securizarea comenzilor, OData și integrarea hostului (42f) |
| `nou/Atlas.Conta.BackOffice/Atlas.Conta.BackOffice.Blazor.Server` | Host XAF, administrare și actualizarea explicită a bazei (23a) |
| `nou/Atlas.Conta.Client` | React: citiri, proiecții, consolele comenzilor, editorii de politici și nomenclatoare și contractele generate; paginile de document sunt înghețate (43e, 104d) |
| `nou/Atlas.Conta.Nucleu` | Nucleul pur al cubului de postări: tipuri, conservare, unitate, FIFO, evaluare, repartizare, TVA, storno, motor, coordonatele de repartitor și contraponderea — fără niciun pachet; consumat DOAR de `Module` (`Declaratii/`), referit direct și de ModelCheck ca unealtă (90b, 90l) |
| `nou/Atlas.Conta.Nucleu/Atlas.Conta.Nucleu.Teste` | Invarianții nucleului ca proprietăți pe generatoare proprii și testul de arhitectură (90l) |
| `nou/tools/ModelCheck` | Verificarea modelului și scenarii de domeniu pe PostgreSQL (23) |
| `nou/tools/ProbeHttp` | Probe ale contractului HTTP și ale permisiunilor reale (80i, 81j) |
| `nou/tools/Import1C` | Import operațional și reconcilierea sursei (45f); `ANALYZE` după ultima scriere (SAFT-r5) |
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

### Migrațiile

EF Core Migrations este mecanismul de evoluție a schemei. Actualizarea
automată a schemei prin XAF este dezactivată. Module este comun celor două
hosturi; schimbările incompatibile se livrează coordonat. (23a, 42f)

Lanțul pornește din migrația de bază `20261010151835_InitialCreate`
(C111-D7; 102e, D9-D6) și continuă incremental (120):
`20261012114235_OriginiPartide` adaugă proiecția provenienței fără backfill
(TR-D9b); `20261013073914_SensPePostare` redenumește coloana `Latura` a lui
`Postare` și a view-ului `PostareVizual` în `Sens`;
`20261013082803_SensTvaPeMapari` redenumește în `SensTva` coloanele de sens
ale mapărilor D300, D394 și SAF-T (TR-D9b);
`20261013214047_CorectieValoareLot` adaugă pe `DocumentDetalii` coloana
nulabilă `CorectieValoareLot` (119). Migrația de bază este
generată din model, plus SQL-ul brut pe care
modelul nu-l declară: `Postare` partiționată pe `Spatiu` cu cheia
`(Spatiu, ID)`, cele două partiții, FK-urile pe partiții — inclusiv cele trei
spre `Repartitori`, prin `Repartitor`, `PartenerFiscal` și `Loc` —, cei 14
indecși pe partiții și pe părinte (cu `IX_Postare_Fisa_Cont` și cu
`IX_Postare_Tranzactie_Pereche`, pe `(TranzactieId, Pereche)`, care ține și
loc de index pe tranzacție; probați de `STR-SCHEMA-4`), constrângerile `CK_Postare_*` (cu `CK_Postare_Contrapondere`
și `CK_Postare_BlocFiscal`), funcția `cub_partida_id` și view-ul listei de
evidență, `PostareVizual`. `Down` le șterge cu `DROP` explicit. `ToView` e
exclus din snapshot, deci `has-pending-model-changes` rămâne curat.
Snapshot-ul EF
rămâne cu cheia `ID` (divergență declarată, probată de `STR-SCHEMA`).
Migrațiile nu transformă date. Migrațiile de dinainte sunt
istorie în git; bazele create pe un lanț vechi nu se actualizează, se
recreează (102b); clonele de import și de perf de dinaintea migrației curente
rămân pe schema veche și Import1C cere `--recreeaza`. Lanțul crește prin
migrații până la „rotund”, unde se recomprimă într-o singură migrație de
bază, cu proba că schema rezultată e identică; de la prima bază care trebuie
păstrată nu se mai recomprimă. (120)

Comanda `dotnet ef` primește mereu `--context BackOfficeEFCoreDbContext` și
se rulează fără `--no-build`. (23a, 89g)

### Maparea TPH

Cele trei ierarhii (`Document`, `DocumentDetaliu`, `Repartitor`) sunt TPH:
câte o tabelă pe rădăcină, discriminatorul `ClrType` cu valorile implicite
ale EF și index pe el. Proprietățile omonime ale frunzelor-surori împart
coloana, prin bucla generică `AplicaColoanePartajate`; nu există
configurare de coloană per proprietate. Două proprietăți omonime cu tip de
stocare sau facete diferite opresc construirea modelului, cu toate
coliziunile într-un singur mesaj; se redenumește una dintre ele, nu se
adaugă prefix. O clasă nouă cu nume mai lung decât coloana discriminatorului
produce singură o migrație `ALTER COLUMN`. (89a, 89b, 89c)

### Actualizarea bazei și seed-ul

Actualizarea bazei se execută explicit prin hostul Blazor, cu opțiunile
`--updateDatabase --forceUpdate --silent`. WebApi verifică compatibilitatea
și nu devine al doilea updater automat. Baza țintă se verifică înainte de
orice comandă care aplică migrări sau seed. (23a, 42f)

Seed-ul este specific profilului și idempotent. Pe tipurile cu proveniență
trece printr-un singur helper (`ContaSeeder.Aliniaza`): caută rândul pe cheia
indexului unic, îl creează cu timbru dacă lipsește, îl aliniază la cod dacă
poartă timbrul seed-ului (câmpurile scalare ne-cheie, fiecare corecție
tipărită `tip / cheie / câmp: vechi → nou`), îl lasă neatins dacă e manual și
nu îl recreează dacă există un `RefuzSeed` pe cheia lui. Un seed care ar
schimba o cheie aruncă.
`Seed` întoarce `RaportSeed` cu contoare per tabel (create / corectate /
manuale / șterse); a doua trecere pe o bază aliniată nu creează și nu
corectează nimic. Câmpurile de stare de runtime (`PoliticaNumerotare.
UrmatorulNumar`) și cele deținute de alt pas al seed-ului (`TipTva.Activ`,
`ContImplicitId` derivat) nu intră în aliniere. Rândurile nomenclatoarelor de
nucleu fără proveniență (`RandD300`, `Judet`, `UnitateMasura`) se rescriu
autoritar; reseed-ul nu suprascrie datele societății. (69b, 73a, 83a–d, 84a, 104i)

Profilurile nu se amestecă în aceeași bază. `SetareProfil` și rotunjirea sunt
stabile după inițializare. (36c, 52a)

### Integritatea în schemă

Unicitatea politicilor și a codurilor de nomenclator este în schemă, prin
indexuri unice nefiltrate: ștergerea e fizică, deci cheia unui rând șters se
poate reface. Ancora `TipDocument.ClrType` este unică tot
așa (`IX_TipuriDocument_ClrType`); discriminatorul documentelor nu are FK
spre ea, iar corespondența clase concrete ↔ seed o probează ModelCheck. (81c, 89a, 104f)

Curățenia de scenă din ModelCheck (`Purja`) șterge fizic, prin SQL, și
emulează cascada pe FK-urile obligatorii: rândurile care nu pot exista fără
părinte pleacă înaintea lui. Schema nu cascadează în afara compozițiilor
(104g). Probele care cer o etichetă de nomenclator ascunsă (SC-CIT-29/40/80)
șterg rândul într-o tranzacție rulată înapoi, cu FK-urile suspendate
(`AscundereControlata`). (F13-D2, 104g)

Absența FK-ului pentru `Lot.LinieIntrareId` este intenționată pentru ciclul de
inserare; integritatea este verificată de mecanismele domeniului. (26e)

### Versiunile pachetelor

Versiunile backend sunt centralizate în `Directory.Packages.props`.
Pachetele Atlas.DXF și DevExpress folosesc intervalul flotant al liniei de
versiune (`26.1.*`); versiunea restaurată trebuie să fie coerentă între
proiecte. (39a, 41e)
Clientul folosește pnpm și versiunile declarate în `package.json` și lockfile.

## Serverele și bazele de dezvoltare

Dezvoltarea folosește două servere Postgres 18, ambele cu
`postgres`/`postgres`:

| Server | Port | Cine îl folosește |
|---|---|---|
| containerul `contapal-postgres-1` | 5444 | hosturile (Blazor, WebApi), probele HTTP din `nou/tools/ProbeHttp`, implicitul `dotnet ef`, rețeta `perf-saft-container.ps1` |
| instanța nativă de pe gazdă | 5446 | rulările grele: ModelCheck, Import1C, scara transversală și scara de volum |

Rulările grele stau pe instanța nativă fiindcă un dus-întors din gazdă spre
container trece prin proxy-ul Docker Desktop, iar ele fac sute de mii de
dus-întorsuri. Pe mașina de dezvoltare instanța nativă e serviciul Windows
`PostgreSQL`, cu pornire automată. Ordonarea textelor e cea a Windows
(`en-US`), nu glibc (`en_US.utf8`) ca în container. Citirile mari merg
invers: pe baza de volum, balanța, stocul și partidele cu rest au aceleași
planuri și aceleași rânduri atinse ca în container, dar durează mai mult pe
nativ (`docs/nucleu/c111-pas6-scriere-si-volum.md`). O cifră a scării de
volum se compară numai cu una de pe aceeași instanță.

Șabloanele de conexiune ale uneltelor numesc gazda `127.0.0.1`, nu
`localhost`. După o trezire din hibernare, Npgsql 10 pe `Host=localhost`
poate rămâne în negocierea GSS până la expirarea conexiunii, deși serverul
răspunde, iar psql se conectează; cu `127.0.0.1` sau cu
`GSS Encryption Mode=Disable` conexiunea se deschide imediat. ModelCheck
bugetar raportează atunci „Baza nu există încă — doar validare de model” și
iese cu cod 2, deși baza există. Șirurile hosturilor (`appsettings*.json`,
șirul de design din `BackOfficeDbContext`) numesc aceeași gazdă, pe 5444.

`Conexiunea` din `tools/ModelCheck/Program.cs` țintește 5446.
`MODELCHECK_CONEXIUNE_EXTRA` înlocuiește cheile șablonului, nu le repetă
(`Port=5432` din rețetele de container, de exemplu): o cheie repetată oprește
construcția modelului XAF din proba D85.

Bazele de dezvoltare se recreează, nu se repară: o bază care nu corespunde
codului se șterge (`DROP DATABASE`) și se reface prin comenzi (102b). Rețeta,
cu `CS = Host=127.0.0.1;Port=5444;Username=postgres;Password=postgres` pentru
bazele hosturilor și același șir cu `Port=5446` pentru cele ale ModelCheck:

| Bază | Recrearea |
|---|---|
| `Atlas.Conta.BackOffice` (bugetar, pe 5444; baza ModelCheck bugetar e cea cu același nume de pe 5446, cu aceeași rețetă) | din `Module`: `dotnet ef database update --context BackOfficeEFCoreDbContext --connection "$CS;Database=Atlas.Conta.BackOffice"`; apoi din `Blazor.Server`: `dotnet run --no-launch-profile -- --updateDatabase --forceUpdate --silent` cu `ProfilContabil=Bugetar` și `ConnectionStrings__ConnectionString=EFCoreProvider=Postgres;$CS;Database=Atlas.Conta.BackOffice` în mediu |
| `Atlas.Conta.ModelCheck.Privat` | o recreează ModelCheck (`MigrateAsync` + seed) |
| `Atlas.Conta.BackOffice.Privat` (baza hosturilor) | `dotnet ef database update … --connection "$CS;Database=Atlas.Conta.BackOffice.Privat"`, apoi updater-ul Blazor cu `appsettings.json` (Privat): seed plus utilizatorii `Admin`/`User`/`Cititor`/`Configurator`, fără documente |
| `Atlas.Conta.Import1C.Flax` și clonele ei | nu se recreează implicit; Import1C `--recreeaza` la nevoie (091-r4) |

## Probele HTTP și fixture-urile lor

Pe baza Privat din seed, `partide-cub.py` își creează singur fixture-ul. (89g, 102b)

Citirile cumulate au o probă pe două conexiuni și un contor de instrucțiuni
SQL (SC-CIT-77). Scrierea globală prin ObjectSpace secured este refuzată
înaintea accesului la date (SC-CIT-78). Accesul real pe rând și membru este
verificat separat prin `nou/tools/ProbeHttp/stoc-snapshot-cub.py --baza
NUMELE_BAZEI_IZOLATE` (SC-CIT-74), cu seed privat și utilizatori.

Comenzile de document au proba HTTP proprie, cu fixture creat și șters de ea:
`nou/tools/ProbeHttp/comenzi-coaja.py` (404/403/422 pe cele cinci comenzi,
ușa altui tip, fără scriere la refuz). (104b)

Matricea generală `refuzuri.ps1` rulează pe baza Privat din seed cu fixture
propriu pe prima lună deschisă a lanțului: un furnizor NeinregistratRo, o FCT
operată cu plata conex operată și împerecherea lor, un ITV draft pe aceeași
lună și un angajat, desfăcute în ordine inversă în `finally`. Rămân auditul
și cele două `RefuzSeed` ale rândurilor de politică de probă (104i, chei fixe:
a doua rulare nu adaugă nimic). Plafonul de 500 al candidaților DVI e probat
în ModelCheck, nu pe HTTP. Baza e identică înainte și după. (104-r5)
Matricea cere bază nouă: pe una pe care a rulat `explicatii.py` pică pe
starea lăsată de acesta (documente din 2012, politici de probă), nu pe
produs. (C111)

Matricea conține și ușa explicației (X-D4): tranzacția facturii fixture-ului
se află din jurnal, `Admin`, `Cititor` și `Configurator` primesc 200, iar
`User` 404, la fel ca pe un id inexistent. 403
`EXPLICATIE_ACCES_INCOMPLET` cere un rol cu citire restricționată și rămâne
al probei `explicatii.py`. (X-D8)

Blocajul scrierii are proba HTTP proprie,
`nou/tools/ProbeHttp/scriere-ocupata.py`: ține
`pg_advisory_xact_lock(97000)` pe o a doua conexiune și cere, sub blocaj,
200 imediat pe citire, pe dry-run și pe salvarea fără detalii noi, 422
`SCRIERE_OCUPATA` fără nimic scris pe operare și pe salvarea cu o linie
nouă, apoi 200 pe aceeași operare după eliberare. Fixture-ul e al probei și
se desface în `finally`. Durează cât două timpuri de comandă (un minut).
Rețeta care recreează baza din seed, pornește hostul pe 5089 și rulează
matricea de două ori, `scriere-ocupata.py`, `comenzi-coaja.py` și
`explicatii.py` (ultima, fiindcă lasă un bon stornat):
`run-verificari/x6-http.ps1`. (X-D6, X-D8)

Desfacerea facturii se înscrie imediat după crearea ei și redescoperă din
ID-ul FCT plata conex și împerecherile; după `finally`, matricea verifică pe
API absența identităților fixture-ului (cod 3 la rezidu). `-CadeDupa <punct>`
injectează o cădere după o mutație a fixture-ului, iar
`refuzuri-caderi.ps1` le parcurge pe toate și cere cod 2 și zero rezidu la
fiecare. (104-r5, review C104 R2)

## Contracte generate și build

OpenAPI se extrage din configurația hostului, fără pornirea serviciilor
găzduite, dar cu serverul bazei din șirul de conexiune al WebApi pornit: cu
el oprit, `swagger tofile` pică pe verificarea de compatibilitate XAF.
Schema bazei nu contează.
Fără serverul hosturilor, `ConnectionStrings__ConnectionString` din mediu
îndreaptă procesul spre altă bază. Metadata este derivată din model
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
probele de model și fără celelalte scene; orice tip de pe cub se poate
rula selectiv pe privat. Codurile sunt ale catalogului (`docs/nucleu/scenarii/`,
inclusiv `DESCHIDERE`, `IMO`, `X`, `C111`); un cod necunoscut sau un tip fără nicio
scenă pe profilul cerut iese cu exit 2, nu verde. Scenele și tipurile lor
stau în `ScenelePeTip` (`Suita/ScenelePeTip.cs`), aceeași listă pe care suita
integrală o rulează în ordine; un scenariu nou intră acolo cu tipul lui, altfel
filtrul nu-l vede. Suita integrală pe ambele profiluri rămâne gate-ul de commit.
`--scenarii C111` rulează catalogul formelor coordonatelor
(`docs/nucleu/scenarii/C111.md`, `ScenariiC111.cs`, anul 2033): cazurile
`citiri`, `stoc`, `suport`, `deschidere` și `nir` pe ambele profiluri,
`decont`, `avans`, `tva-explicit`, `tva-partener`, `saft` și `pozitie` numai
pe privat. Fiecare caz își asertează repartitorul, partenerul fiscal, locul,
contraponderea și unitatea pe postare, plus soldurile filtrate, dinamic și
din snapshot. Un caz căzut se raportează ca FAIL și nu oprește celelalte.
(091 (e), 091-r1)

### Structura ModelCheck

`Program.cs` e numai dispecerul (argumente, conexiune, migrații, secvența
apelurilor); fiecare scenă veche e o clasă în `Suita/` cu `Ruleaza(Suita s)`,
iar contextul comun (`Check`, provider, profil, ajutoarele de perioadă) e
instanța `Suita`, fără stare statică. O probă care emite aserții din rânduri
citite din bază le ordonează pe o cheie a scenei, nu pe ordinea bazei; purja
de scenă prinde codurile pe prefixul marcajului. (TR-D9a)

## Îmbinările pe chei nulabile ale cubului

Unitatea, repartitorul și documentul sunt nulabile pe `Postare`. O îmbinare
LINQ cu ambele părți nulabile iese din EF cu
`a = b OR (a IS NULL AND b IS NULL)`, pe care Postgres o execută pătratic.
Cu o parte ne-nulă iese egalitate simplă. Aceeași formă iese dintr-un
`Contains` peste o subinterogare, ca `EXISTS` corelat. Soldurile de partide
se îmbină cu documentul deschizător numai prin `Partide.CuOrigine`. Citirile
unui singur document (`Partide.PropriiAle`, din care vin restul și
disponibilul, și `Partide.MiscariPePartidele`) pornesc de la partidele
deschise de el și țin partea ne-nulă pe origini, ca postările să fie căutate
prin index, nu parcurse. `SC-CIT-110` probează că cititorii de partide și
snapshot-ul chiar sunt executați și nu au forma. (D9-D10 (b))
Perechea (`Postare.Pereche`, nulabilă) se îmbină în SQL scris de mână, în
fișa contului și în registrul jurnal: egalitate simplă, care folosește
indexul pe `(TranzactieId, Pereche)`. Ocolirea din LINQ cu `??` dă o
expresie pe care indexul n-o poate folosi. (C111-Q20)

`RAMURA-NUL` (`ProbeRamuraDeNul`) caută forma în fiecare comandă SQL pe care
EF o emite în proces, pe orice context, în rularea integrală și în
`--scenarii`. O potrivire contează numai când cele două părți vin din surse
diferite; compararea a două coloane ale aceluiași rând nu e îmbinare. Trei
aserții pe profil:

- detectorul prinde o îmbinare martor a lui `Postare` cu ea însăși pe unitate
  și repartitor, executată la fiecare rulare;
- nicio formă în afara celor admise nominal;
- fiecare formă admisă apare în rulare (numai în integrală).

O formă se identifică prin apelantul care execută interogarea și coloanele
comparate. Se admite nominal numai împreună cu egalitatea simplă pe cheia
selectivă care corelează aceleași două surse, iar admiterea cade dacă acea
egalitate dispare din comandă. Admise:

| Apelant | Coloanele cu ramură de nul | Cheia selectivă |
|---|---|---|
| `Explicatii.VerificaAcoperire` | `DocumentId` | cheia primară a tranzacției-sursă (`ID = ExplicatieDinId`) |
| `Explicatii.VerificaAcoperire` | `Unitate` | cheia primară a postării inversate (`ID = InversaDinId`) |
| `Invarianti.VerificaFiscal` | blocul fiscal (8 coloane, cu `PartenerFiscal` și `Loc`) | cheia primară a postării inversate |
| `Invarianti.VerificaPerechi` | documentul, linia, produsul, codurile bugetare, cantitatea | aceeași tranzacție (`TranzactieId`) |
| `AcoperireInvarianti.PerecheLangaTransformare` (unealtă) | `DocumentId`, `LinieId` | aceeași tranzacție |

La un eșec, proba scrie SQL-ul fiecărei forme în `%TEMP%/ramura-nul/`. O
formă nouă se corectează ținând o parte ne-nulă (`?? Guid.Empty` pe partea
agregată sau, când cealaltă parte e `Postare` neagregată, pe partea mică, ca
indexul postărilor să rămână folosibil; ori `!= null` pe ambele părți ale
unui `Contains`); se admite numai
când ramura de nul e filtru lângă o cheie selectivă.

## Limitele implementării

- Baza nu verifică tipul unui rând: discriminatorul `ClrType` al documentelor
  nu are FK spre `TipDocument.ClrType`, iar un FK spre o frunză (de exemplu
  `Lot.GestiuneId`) acceptă în schemă id-ul oricărui repartitor. Pe ușa
  securizată tipul îl verifică gardianul; pe ușa de sistem (Import1C, seed)
  nimic nu-l verifică la scriere, iar abaterile le găsesc doar probele
  ModelCheck și SQL-ul din `--dump-integritate-tph` rulat după import. (89a,
  89e)
- Cu pregătirea automată a comenzilor, citirile de sold pe o listă de loturi
  (`Fapte.SolduriLoturi`, `Loturi.VerificaSoldIntermediar`) primesc un plan
  generic mai lent: cu limitele de dată necunoscute, planificatorul alege
  indexul pe dată în locul celui pe `(Unitate, Data)` și filtrează lotul după
  citire. Import1C o folosește; hosturile nu.
- `SursaCont` și mai mulți membri ai politicilor nu au `[XafDisplayName]`;
  ecranele poartă caption-ul în cod. (84-r5)
- Găzduirea clientului și a API-ului pe aceeași origine e contractul de
  livrare. Hostul are `UseStaticFiles`, dar proiectul WebApi nu copiază
  automat build-ul React și nu are fallback-ul rutelor SPA; publicarea cere și
  configurarea JWT pentru mediul țintă. (43e, 55g)
- Pe containerul Postgres de dezvoltare, cu `/dev/shm` de 64 MB, sortarea
  paralelă a listei `PostareVizual` pică (`could not resize shared memory
  segment`). (D9-A12)

<!-- gen:decizii -->
## Decizii

Deciziile care numesc pagina în `Arii:` (24; titlurile în [index](../decizii/README.md); generat de `docskit gen`, nu se editează): 21, 23, 26 (amendată de 112), 35, 36 (amendată de 71), 39, 40, 41, 42 (amendată de 104, 106), 43 (amendată de 104), 45 (amendată de 75), 52, 56 (amendată de 77, 81), 69, 73 (amendată de 74, 105), 80, 81, 84, 89, 90 (amendată de 91, 92, 93, 95, 96, 110, 119), 91 (amendată de 107, 118, 120), 102, 104 (amendată de 106), 120.
<!-- /gen:decizii -->
