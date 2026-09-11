# 83 — Seed-ul și proveniența: re-seed-ul corectează doar `DinSeed`, golurile privat (NIM / IMP cu poarta spre DVI), rolul `Configurator`

- **Data**: 2026-09-10
- **Stare**: activă, executată de 84 (`docs/api/p5-felia24-politici-explica-contract.md`; 83j amendată de 84j — ștergerea unui `TipTva` referit se refuză; 83-r3 tranșată: `IMP` fără cod SAF-T, `NIM` cu 308302)
- **Docs**: `docs/decizii/081-p5-felia23-implicite-politici.md` (81-r1, 81-r2, 81-r3, 81k), `docs/invarianti.md` (IV, V), `Module/BusinessObjects/Comun/ICuProvenienta.cs`, `Module/DatabaseUpdate/ContaSeeder.cs`, `Module/DatabaseUpdate/ProfilPrivat.cs`, `Module/DatabaseUpdate/Updater.cs`, `Module/Motor/GardianEditare.cs`, `Module/Motor/VerificareProfilService.cs`, `Module/Motor/ImpliciteService.cs`

## Context

Felia 24 (81k: „Explică" + ecranele `RegulaContare`/`RegulaStoc`/`MapareD300`/
`MapareD394`/`PoliticaTva`/`Conex`/`Validare`) expune clientului semantica
seed-ului. Trei întrebări lăsate deschise de decizia 81 devin ireversibile
după primele editări din client, deci se tranșează ÎNAINTE:

1. **81-r2** — ce face re-seed-ul cu un rând `DinSeed` al cărui conținut s-a
   schimbat între release-uri. Azi seed-ul e „insert dacă lipsește" pe toate
   politicile; timbrul se aprinde doar la creare, gardianul îl stinge la orice
   scriere securizată, rândul șters logic nu se recreează (se raportează). Un
   rând al profilului corectat în cod rămâne, pe bazele existente, în forma
   primei seed-uiri, iar raportul de profil nu-l vede: e timbrat seed și arată
   sănătos.
2. **81-r1** — achiziția de la neînregistrat RO și achiziția extra-UE n-au rând
   de politică implicită. Faptul care contează: **absența unui rând NU e
   tăcere** — rezolvarea cade pe ancora tipului (`N21`), care arată plauzibil
   pe orice linie. Lista „nemapate cu motiv" din D300/D394 nu se transferă:
   acolo lipsa înseamnă „nu se declară", aici înseamnă „21% standard". Iar o
   linie fără `TipTva` nu intră în `RegistruTva` (68), deci achiziția ar lipsi
   din jurnalul de cumpărări — „nu propune nimic" nu e o ieșire onestă.
3. **Cine editează politicile.** Azi singurul scriitor e `Administrators`;
   `Cititori` e read-only dev-only (80g); `Default` vede doar obiectele proprii.
   Ușa OData pe politici e deschisă cu invarianții în gardian (81e), iar pasul
   zero al gardianului (80c) cere `CanWrite` per obiect — un rol nou nu cere
   cod în motor.

Fapte de domeniu fixate în discuție (2026-09-10):

- **SFD e natura OPERAȚIUNII** (art. 292: medical, financiar, chirie,
  educație), nu a partenerului: furnizorul poate fi înregistrat în scopuri de
  TVA și să emită facturi scutite. Nu decurge din clasa fiscală, deci NU e
  rând de politică pe clasă; vine prin override-urile existente (81b):
  `Partener.TipTvaImplicit` (furnizorul face doar operațiuni scutite) sau
  `Produs.TipTvaImplicit` (produsul „Chirie"), corectabile pe linie.
- **NIM decurge din CLASĂ**: furnizorul nu e persoană impozabilă sau aplică
  regimul special de scutire — nu există fapt de TVA pe linie. Asta e exact
  forma unui rând `PoliticaTvaImplicit`.
- **Importurile există** (puține) și **DVI-ul se implementează** oricând;
  factura de import se culege acum, deci implicitul ei trebuie să existe cu
  poarta spre DVI deschisă.

## Tranșări

**(a) Re-seed-ul CORECTEAZĂ rândurile `DinSeed = true`, și numai pe ele.**
Timbrul devine proprietate: rândul seed-ului e al seed-ului, rândul editat
(`false`, stins de gardian) e al clientului și nu se atinge, rândul șters
logic rămâne șters (decizia 4 rămâne). Precedentele din nucleu se
generalizează: `RandD300` (69a) și `Judet` (72b) se rescriu deja autoritar.
Amendează formularea din 81d („reseed-ul nu îl reactivează pe un rând
existent" rămâne adevărată — timbrul nu se re-aprinde; ce se schimbă e că
rândul care ÎL ARE se aliniază la cod).

**(b) Cheia nu se schimbă prin seed.** Cheia = indexul unic filtrat al
tabelului (81c). Seed-ul rescrie DOAR câmpurile ne-cheie ale rândului găsit pe
cheia lui. O schimbare de cheie (redenumire de cod, mutare de ancoră) e pas de
migrație de date, cu motiv, niciodată upsert tăcut; două rânduri seed cu aceeași
cheie e eroare de seed, nu de bază.

**(c) Seed-urile derivate** (`SeedContare6xxDin3xx`, `SeedContareVanzare`,
familia „incremental pe TipMaterial") păstrează regula „cheie acoperită manual
= neatinsă", dar cheia acoperită de un rând `DinSeed` se RECALCULEAZĂ din
simbol (același helper, aceleași excepții per profil). `SeedTipuriTvaInactive`
rămâne cum e (stinge doar `DinSeed`).

**(d) Ieșirea seed-ului = auditul re-seed-ului.** Ușa de sistem nu trece prin
AuditTrail, deci consola e singura urmă: fiecare corecție se tipărește
`tip / cheie / câmp: vechi → nou`, cu contor per tabel; rândurile manuale pe
chei de seed și rândurile șterse se listează ca azi. O rulare fără nicio
corecție tipărește contorul zero. Nicio corecție nu e condiționată de un flag:
`--forceUpdate` e deja contractul de re-seed (23a).

**(e) 81-r3 e asumată, nu tratată**: un rând editat înaintea migrației F23 și
timbrat de backfill va fi corectat de primul re-seed sub (a). Expunerea reală
e zero la data deciziei — nu există bază de client, bazele de dev și Flax se
re-seed-uiesc. Exact de asta regula se fixează acum, înaintea primei editări
din client; de aici înainte gardianul stinge timbrul la editare, iar (a) nu mai
poate atinge un rând editat.

**(f) Golul „neînregistrat RO" primește rând de seed**, nu tăcere:
`FCT × NeinregistratRo → NIM` și `RLF × NeinregistratRo → NIM`. Consecință
numită, nu ascunsă: `NIM × Achiziție` e „nemapat deliberat" pe D394 (seed-ul
privat), iar tipul `N` (achiziții de la neînregistrați) nu e niciodată țintă
(71c) — achizițiile de la neînregistrați RO rămân în afara secțiunii C a
D394. Dacă practica cere declararea lor cu `N`, e restanță de D394 (83-r2),
nu o schimbare a implicitului.

**(g) Golul „import" primește tip propriu, cu poarta spre DVI.** Fără regim
nou în enum — motorul se comportă identic cu Neimpozabil (nicio TVA pe linia
facturii), diferența e de identitate și de raportare:

- `TipTva` nou `IMP` („Achiziție din import — TVA prin DVI"), cotă 0, regim
  `Neimpozabil`, codul SAF-T de achiziție din import luat din nomenclatorul
  ANAF la implementare (nu se ghicește; dacă nomenclatorul n-are cod distinct
  pentru factura de import, se documentează alegerea în seed, 83-r3).
- `PoliticaTvaImplicit`: `FCT × ExtraUe → IMP`, `RLF × ExtraUe → IMP`.
  Produsul nu-și impune cota (regim diferit ⇒ regimul bate, 81b).
- `MapareD300`: `IMP × Achiziție` nemapat deliberat, motiv „baza și taxa se
  declară din DVI"; `MapareD394`: nemapat (partener extra-UE nu se declară).
  Rândul de `RegistruTva` există, cu TVA 0 — factura apare în jurnalul de
  cumpărări la bază (68).
- **Poarta**: DVI = tip de document propriu (familia 36f), care poartă tipul
  cu TVA real (`IMP21`, deductibil pe 4426 contra contului de TVA în vamă),
  se leagă la facturile de import și alimentează rândurile D300 de import. O
  DVI acoperă mai multe facturi, deci legătura NU e `DocumentSursa`; forma ei
  e a designului DVI (83-r4). Până atunci `IMP` e onest: linia nu poartă TVA
  și spune de ce.
- Bugetar: zero rânduri (81b rămâne).

**(h) Rolul `Configurator`**, seed-uit lângă `Administrators`/`Cititori`/
`Default`, separat de Admin: Read pe tot domeniul (ca `Cititori`),
Create/Write/Delete DOAR pe tipurile `ICuProvenienta` (politicile +
`TipTva`/`Cont`/`ClasaProdus`/`TipMaterial`), nimic pe documente, registre,
useri, roluri, `Societate`, `SetareProfil`. Ancora `TipDocument` rămâne
read-only pentru toată lumea (81e, prin gardian). Contabilul care configurează
nu e administratorul de useri; separarea e DATE (rânduri de permisiuni), nu
cod. Userul dev `Configurator` (fără parolă, ca `Cititor`) intră în
`refuzuri.ps1` ca al patrulea oracol (80i).

**(i) O singură listă a tipurilor configurabile.** Cele 17 tipuri
`ICuProvenienta` sunt azi enumerate de mână în gardian (`switch`) și în
raportul de profil (`Tabel<T>`). Lista devine una (`Politici.TipuriConfigurabile`,
în Module), consumată de seed-ul rolului (h), de gardian, de raport și de
seed-ul (a); probă ModelCheck: orice tip care implementează `ICuProvenienta`
e în listă, iar rolul `Configurator` are Write pe fiecare. Un tip nou fără
drept pentru Configurator pică zgomotos.

**(j) Rândul de seed ȘTERS rămâne șters și sub (a).** Cazul „am șters din
greșeală rândul de seed" nu are azi ieșire în afara recreării manuale —
restanță cu nume (83-r1), candidată pentru o comandă pe ușa non-secured cu
gate Admin care rulează ACEEAȘI funcție de seed per tabel.

## Testul contra invarianților

- **IV** (structura e cod, politica e date; politica nu inventează
  comportament): (a)–(c) nu schimbă cine decide conținutul politicii, doar
  cine e proprietarul fiecărui rând; `IMP` e rând de nomenclator sub un regim
  existent, fără mecanism nou; `Configurator` e permisiune, nu cod.
- **V** (sursele externe sunt evidență): nomenclatorul SAF-T ANAF dă codul
  lui `IMP`, nu semantica lui; semantica e a legii (TVA la import se
  datorează în vamă).
- **II** (motorul nu cunoaște frunzele): neatins — nicio ramură nouă în
  motor pe `IMP`; TVA nenulă pe o linie `IMP` cade sub gărzile existente ale
  regimului `Neimpozabil`.
- **III**: neatins.

## Review

- **Re-seed-ul poate „repara" un rând pe care clientul îl considera al lui?**
  Doar dacă timbrul e aprins, adică rândul n-a fost niciodată scris pe ușa
  securizată. Editarea prin XAF trece prin același gardian (55a), deci stinge
  timbrul la fel. Rămâne expus doar cazul (e), asumat.
- **Corecția tăcută a unei chei**: interzisă de (b); seed-ul care vrea altă
  cheie scrie pas de migrație de date.
- **`NIM` pentru neînregistrați ascunde o declarație?** Nu: consecința pe
  D394 e numită în (f) și în 83-r2; implicitul e corectabil pe linie.
- **`IMP` sub `Neimpozabil` minte în SAF-T?** Codul de taxă e al rândului
  (`IMP`), nu al regimului; dacă nomenclatorul n-are cod propriu pentru
  factura furnizorului extern, alegerea se documentează (83-r3), nu se
  împrumută tăcut codul lui `NIM`.
- **`Configurator` fără Write pe `Cont`?** Are: `Cont` e `ICuProvenienta`
  (seed-ul îl tratează ca al lui), iar `Cont` scriibil pe OData rămâne
  restanța 81k („`Cont` scriibil") — rolul primește dreptul, ușa OData
  rămâne `ReadOnly` (70g) până la felia care o deschide.
- **Un tip `ICuProvenienta` nou nu apare în rol**: (i) pică proba, nu
  utilizatorul.

## Verificări (regula de oprire a execuției, în felia 24)

ModelCheck, ambele profiluri: rând seed modificat pe ușa de sistem → re-seed
îl readuce; rând editat pe ușa securizată → re-seed îl lasă; rând șters → nu
se recreează, apare în raport; cheia derivată acoperită manual → neatinsă;
`IMP`/`NIM` propuse de `ImpliciteService` pe clasele lor, cu sursa
`Politica`; proba (i). `refuzuri.ps1` cu `Configurator`: 200/204 pe politici,
403 pe documente, registre, useri. Re-rularea integrală Import1C rămâne
identică cu baseline-ul (seed-ul privat capătă rânduri, nu schimbă conturi).

## Ce rămâne deschis

- **83-r1** recrearea unui rând de seed șters, la runtime (aceeași funcție de
  seed per tabel, pe ușa non-secured, gate Admin).
- **83-r2** D394 tip `N` (achiziții de la neînregistrați) nu se generează
  niciodată; `NIM × Achiziție` rămâne nemapat deliberat (71c, familia D4-r*).
- **83-r3** codul SAF-T al lui `IMP`: se ia din nomenclatorul ANAF la
  implementare; lipsa unui cod distinct se documentează în seed.
- **83-r4** DVI ca tip de document (familia 36f): tipul `IMP21`, legătura
  n→m cu facturile de import, rândurile D300 de import, jurnalul.
- **83-r5** `Configurator` în XAF: permisiunile de navigație pe itemul
  „Politici" (rolul e definit pe tipuri; XAF cere și navigație — 44/53).
- **83-r6** 81-r3 (rând editat pre-F23, timbrat de backfill) rămâne asumată,
  cu expunere zero la 2026-09-10.
