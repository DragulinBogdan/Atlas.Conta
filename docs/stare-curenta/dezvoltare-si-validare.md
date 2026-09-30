# Dezvoltare și validare

**Actualizat: 2026-10-04.** [Index](README.md)

TR-D8 în lucru peste `c10d0fe`: rapoartele contabile, snapshot-ul contabil,
evaluarea operațională pe lot, pin/FIFO DSC, raportul de stoc și
raportul/snapshot-ul/citirile operaționale ale partidelor folosesc
intrările comune ale cubului. C102 (102) scoate codul de compatibilitate cu
bazele de dezvoltare: hosturile nu scanează istoria la pornire, iar
invarianții cubului rulează în ModelCheck (`INV-CUB`, `Cub.Citiri.Invarianti`).

`INV-CUB` (`tools/ModelCheck/AcoperireInvarianti.cs`) rulează în
`ScenaDocumente.Ruleaza` după scenă și înaintea purjei ei, pe faptele
produse, inclusiv stornourile. Fiecare ramură fără probă proprie are un
mutant într-o tranzacție anulată, rulat o dată per profil pe prima scenă cu
faptele potrivite: `DESCHIDERE-EGALA` (registrul istoric egal cu deschiderea
cubului trece), `DESCHIDERE`, `PARTIDE` (partidă fără partener), `POLITICA`
(totalul de decontare), `IMO-FISA`, `IMO-CAUZA`, `IMO-ORIGINE` (inversa unei
fișe fără `InversaDin`), `IMO-SUPORT` (transfer valoric pe fișă fără suport),
`IMO-REGISTRU` și cei șapte ai explicației (X-D4): `EXPLICATIE-LIPSA` (explicația ștearsă),
`-REFERINTA` (referința mutată pe alt document), `-STORNO` (cantitatea
inversei pe lot schimbată), `-IESIRE` (valoarea unei `ValoareIesire` +0,01),
`-DECLARATA` (`ValoareDeclarata` scoasă), `-EVALUARE` (soldul citit +1.000),
`-STINGERE` (`AlocareFifo` scoase). Review-ul implementării (X-RI1, X-RI2) a
adăugat cinci: `EXPLICATIE-MECANISM` (ieșirile evaluate redenumite
`ValoareDeclarata`, cu aceleași cifre și fără soldurile citite),
`-DECLARANT` (aceeași substituție, cu numele declarantului schimbat într-unul
care declară valori), `-SOLD-FIFO` (soldul citit al partidei stinse scos),
`-SOLD-FIFO-MIC` (soldul citit pus sub suma alocată) și `TRANSFER-CONT`
(numai contul capătului de destinație al unui transfer pe lot). Ramurile
acoperirii registru ↔ cub, echilibrului și provenienței au probele SC-CIT-23,
SC-CIT-34 și SC-CIT-10. Acoperirea cantitativă a stocului
(`Loturi.VerificaAcoperire`, `CITIRE_ISTORIC_STOC_INCOMPLET`, X-D7 a) rulează
prima dintre acoperiri și are probele SC-CIT-103…105. Mutantul
`EXPLICATIE-STORNO` aplică aceeași abatere și rândului de registru, ca să
ajungă la ramura explicației. La final, profilul cere cel puțin o scenă verificată și,
pe integrală, toate ramurile ucise, iar purja nu lasă postări. La prima
rulare, proba a găsit trei totaluri de stins care nu corespundeau cubului:
taxarea inversă (SC-FCT-10), contul explicit fără partide (SC-DEC-10) și
factura cu avans (SC-NIR-30/avans). Totalul se scrie acum din cub (102).
Integrala după corectură: **3.142 bugetar / 4.141 privat OK**, zero FAIL,
build fără avertismente, `run-verificari/20260925-163947-708/`.

Validarea curentă (C102 cu review-ul Codex R1/R2): **3.146 bugetar / 4.145
privat OK**, zero FAIL, exit 0, build fără avertismente,
`run-verificari/20260925-174625-732/`. La închiderea C102: 3.083/4.079,
`run-verificari/c102/final-bugetar.log` și `final-privat.log`; cifra scăzuse
față de 3.107/4.103 pentru că probele compatibilității scoase au ieșit
(SC-CIT-08/24, SC-NTC-20, avertismentele seed-ului, backfill-urile).
Clientul compilează; metadata/OpenAPI/types sunt stabile la regenerare.
Decizia 101 este implementată; 101-r1, redeschisă de review-ul advers, e închisă în C102 (D-2/D-3, SC-CIT-66…68). Anterior: SC-CIT-49…65,
HTTP raport/panou cu securitate pe rând și membru peste închidere/reconstrucție,
comandă de împerechere și refuz atomic, browser raport și candidat 100/40/60.
Logurile `trd8-101-http-report2`, `trd8-101-http-payment2`,
`trd8-101-client-final`, `trd8-101-drift-final` sunt în `run-verificari/`.
Decizia 100 este implementată: SC-CIT-41…48 și lanțurile comerciale sunt
verzi pe ambele profiluri. SC-CIT-46/48 trec și prin HTTP și prin acțiunea
XAF, cu verificarea numerică a cubului; fixture-urile sunt curățate.
`nou/tools/ProbeHttp/partide-cub.py` oferă proba HTTP și opțiunea
`--prin-xaf`, care așteaptă cel mult cinci minute acțiunea din browser.
HTTP SC-CIT-25 a trecut de două ori, inclusiv după închidere și reconstrucție;
raportul de stoc este verificat prin HTTP și browser (3/10 pe contul 371).
Matricea generală `refuzuri.ps1` își creează singură subiecții (104-r5);
plafonul candidaților DVI se probează în ModelCheck.
Catalogul: [CITIRI.md](../nucleu/scenarii/CITIRI.md).
Review-ul propriu și limitele: [tr-d8-review-codex.md](../nucleu/tr-d8-review-codex.md).
Proba HTTP durabilă `nou/tools/ProbeHttp/citiri-cub.py` verifică accesul
pe rând și membru înainte/după închidere și reconstrucție, pe host privat
cu baza izolată `.CodexBCS`. Tokenurile rămân în memorie; fixture-ul și
rolurile temporare se curăță în finally.

Accesul la cub (091-r3): codul de producție (`Module`, `WebApi`,
`Blazor.Server`) obține rânduri `Postare`/`Tranzactie` numai prin
`Cub/Citiri`. Excepțiile numite sunt scriitorul (`Cub/Materializare*`),
faptul sursei recepției conexe (`Cub/ReceptiiConexe.cs`) și maparea EF
(`BackOfficeDbContext`). Consumatorii compun peste intrările comune
(filtre, proiecții, eticheta `Storno`), fără să refacă domeniul. Proba
`091-r3` (`tools/ModelCheck/ProbeCititoriCub.cs`) scanează sursa înaintea
bazei, deci rulează și fără bază. La fel probele regimului pe stare
(`tools/ModelCheck/ProbeRegim.cs`, 106e): prin reflecție, fiecare comandă
`RecordEdit` a unui controller pe `DetailView` de `Document` numește prin
sufixul ID-ului o comandă din `RegimDocument.ComenziCunoscute`; prin scanare
sintactică, nicio affordance `Poate*` din `Module/Api/` nu se calculează din
`StareDocument` și niciun controller nu poartă cheia „Stare”. Catalogul
`REGIM` (`ScenariiRegim.cs`, anul 2012) rulează cu `--scenarii REGIM`. Caută rădăcinile de interogare,
`ModifiedObjects.OfType`, `CreateObject`, `Set<>`, `typeof` pe
`GetObjects`, SQL pe tabelele cubului, navigarea `Tranzactie.Postari` și
`DbContext.Postari`. Pică și pe o excepție care nu mai are acces. Scanarea e
sintactică: tipul calificat cu `N.`/`Nucleu.` e al nucleului, restul contează
ca rând al cubului. ModelCheck e probă independentă și citește cubul direct
(D8-B1). Excluderea `Transfer` și a inversei lui stă numai în
`Citiri.Contabil.Postari`, după originea `InversaDin` (N-r8). `Loturi` și
`Partide` includ transferul prin contract.

`Transfer` pe cititorii comuni (N-r8, gate-ul transversal TR-D8 X-D2): proba
`N-r8` (`tools/ModelCheck/ProbeTransferCititori.cs`) cere ca fiecare intrare
publică din `Cub/Citiri` care întoarce rânduri de cub să aibă regimul
declarat. `Contabil.Postari`, `Contabil.Jurnal` și `Plati.Postari` exclud
`Transfer` și inversele lui. `Loturi.Postari` și `Partide.Postari` le includ.
`Fiscale.Postari` și `Imobilizari.PozitiiFaraFisa` nu filtrează felul:
domeniul lor e dat de coordonate, iar regula nu li se aplică.
`FelTranzactie.Transfer` apare în producție numai în doisprezece membri
numiți: scriitorul (cinci membri din `Cub/Materializare*`), producătorul
`DeclarantAsamblare`, doi cititori comuni (`Loturi.VerificaRetragere`,
`Plati.Alocari`), trei martori (`Imobilizari.VerificaAcoperire`,
`Explicatii.VerificaAcoperire`, `Invarianti.VerificaTransferuri`) și eticheta
mișcării din SAF-T. Un consumator
care refiltrează `Transfer` e detectat (mutant). SC-CIT-100…102 numără, pe
fiecare intrare, rândurile `Transfer` și inversele lor din BTR stornat, din
împerecherea desfăcută și din ASM-ul mixt stornat.

Regula listării (N-r8, X-D7 d): jurnalul și fișa de cont nu listează
`Transfer` și nici inversa lui, fiindcă pornesc din `Contabil.Postari`.
O listare pe unitate (lot, partidă) pornește din `Loturi.Postari` sau
`Partide.Postari`, care le întorc pe amândouă, și arată felul rândului din
`Tranzactie.Fel`; inversa unui transfer apare ca `Storno`. Listarea nu
refiltrează felul (mutantul probei `N-r8`). SC-CIT-107 o probează pe
`ContabilProiectii.RegistruJurnal`, pe `FisaCont` și pe cele două intrări pe
unitate. Contraponderile Transformare (T-r14) nu ajung prin nicio intrare
comună: `Contabil`, `Partide` și `Imobilizari.PozitiiFaraFisa` aplică
`Transformare.FaraContrapondere`, `Loturi` cere unitate, `Fiscale` cere tip de
TVA, iar `Plati` pornește din `Contabil`. SC-CIT-106 numără zero pe toate
șapte, SC-CIT-15 și SC-SAFT-10 pe rapoarte, iar SC-ASM-16 probează că
diagnosticul `Comparabil` le exclude cu numărul raportat.

Accesul la registre (X-D2): `RegistruContabil`, `RegistruStoc`, `RegistruTva`,
`RegistruImobilizari` și `Imperechere` se ating în producție numai prin lista
nominală din `tools/ModelCheck/ProbeCititoriRegistre.Lista.cs`. O intrare
numește fișierul, membrul, registrul și rolul. Un membru nou într-un fișier
deja permis rămâne încălcare. Proba `X-D2` (`ProbeCititoriRegistre.cs`) citește
arborele sintactic al sursei (`SursaProductie.cs`) și numără trei feluri de
utilizare: mențiunea tipului, inclusiv prin alias `using`; numele tabelei
într-un literal; apelul unui purtător. Purtător este orice membru al cărui tip
declarat conține o colecție de rânduri de registru, plus lista declarată
`Purtatori` pentru rezultatele netipizate: soldurile din `StocService`,
`Operand.SolduriLoturiRegistru` și `Partide.NominalizataLibera`. `nameof` și
definiția tipului nu contează. Proba pică și pe o intrare rămasă fără
utilizare, iar șase mutanți îi probează detecția. În `Proiectii/`, `Api/`,
`Culegere/`, `Saft/`, `Declaratii/` și WebApi cele patru registre apar numai
ca cheie de autorizare; singura excepție numită este absorbția ASM-B6 din
`DeclarantAsamblare`.

Lista nominală (72 de intrări, 88 de utilizări fișier × membru × registru):

| Clasa | Membrii | Rolul |
|---|---|---|
| maparea EF | `BackOfficeEFCoreDbContext`: `OnModelCreating` și cele cinci `DbSet` | definiția și maparea |
| 1. scriitor dual | `MotorOperare`: `Opereaza`, `AnuleazaOperarea`, `Storneaza` | scriu, șterg și inversează rândurile din `RegistruContabil`, `RegistruStoc`, `RegistruTva` |
| | `CorectieService.Corecteaza` | reatribuie perioada inversei în `RegistruTva` |
| | `GardianEditare.Verifica` | refuză scrierea celor patru registre pe ușile securizate (14) |
| | `StocService`: `MiscariRegistru`, `SolduriLaData`, `AplicaValoareIesire`, `VerificaSoldIntermediar` | valoarea ieșirii și garda de sold ale rândului de registru |
| | `StocService`: `Sold`, `AlocaFifoTolerant`, `AlocaFifo` | fără apelant de producție; oracol al probelor |
| | `Fapte.SolduriLoturiRegistru`, `Fapte.Operand`, `DeclarantAsamblare.Declara` | absorbția Δ a ASM față de soldul registrului (ASM-B6) |
| | `PunereInFunctiune`, `IesireImobilizare`, `AmortizareLunara`: `MaterializeazaRegistrul`, `EliminaRegistrul`, `StorneazaRegistrul`; `PunereInFunctiune.RanduriProprii`, `Inverseaza` | scriu `RegistruImobilizari` |
| 2. martor | `Invarianti.Verifica`, `Loturi.VerificaAcoperire`, `Imobilizari.VerificaAcoperire` | acoperirea cubului față de registru (`INV-CUB`, X-D7 a, 097-r1) |
| | `Materializare.Deschide`, `LoturiLiniiSterse.Curata` | urma lotului în `RegistruStoc` |
| | `GardianEditare.VerificaTipTva`, `Imobilizare.Verifica` | referința care oprește ștergerea nomenclatorului |
| 3. evidență XAF | `ContaUiBaseline`: `AscundeFkuriBrute`, `Imobilizari` | listele registrelor |
| autorizare | `RegistrulCitibil` din `ItvController`, `AmoController`, `ImobilizariController`; `PerioadeController.TipuriInsumate` | dreptul de citire pe tipul registrului păzește cifrele (F22-D5, 80e) |
| legătură | `ImperechereService` (8), `GardianEditare` (4), `MotorOperare.VerificaFaraImperecheri`, `Partide.NominalizataLibera`, `Materializare.Imperecheaza`, `ApiProiectii.AreImperecheri`, `ImperechereApply` (3), `ImperechereController` (5), `ImperecheriController` (3) | `Imperechere` este legătura explicită, nu registru; restul și candidații vin din `Partide` |

Clasele 1–3 sunt ale contractului X-D2. Maparea, autorizarea și legătura le-a
cerut prima rulare. Clasa 1 și evidența XAF cad la TR-D9; `Imperechere` rămâne.
Prima rulare a găsit un singur defect: supraîncărcarea
`TvaProiectii.IntreLuni(IQueryable<RegistruTva>)`, fără apelant de producție, a
ieșit din `Proiectii/`; oracolul pe registrul fiscal stă acum în ModelCheck.
`ImperecheriProiectii.Asignari` și martorul `RegistruTva` din `TvaProiectii`,
numite în contract, nu mai există.

Blocajul scrierii (X-D6): proba `X-D6` (`ProbeBlocajScriere.cs`) ține lista
nominală a membrilor care intră sub blocaj (21 de intrări: comenzile, cele
două uși din tranzacția apelantului și salvarea detaliilor noi). Un membru
nou care cheamă `TranzactieComanda.Incepe`/`Asigura` pică proba până e
adăugat, cu rolul lui; la fel o deschidere de tranzacție în afara
`TranzactieComanda` și a citirii declarate (`Fiscale.DeschideCitirea`) sau
cheia blocajului scrisă în alt fișier. Scena `ScenariiConcurenta`
(`--scenarii X`) probează pe captura SQL că fiecare fel de comandă începe cu
blocajul și că citirile nu îl iau (SC-X-22), apoi matricea rezultatelor
seriale pe două conexiuni (SC-X-15…SC-X-21, SC-X-23). O scenă care ține
`TranzactieComanda.Incepe` pe un ObjectSpace și cheamă pe același fir o
comandă sau o salvare de detalii noi pe altul se blochează singură până la
`SCRIERE_OCUPATA`.

`ModelCheck --probe-sursa [--lista]` rulează numai probele pe sursă (104c-S1,
091-r3, X-D2, N-r8, X-D6), fără bază; `--lista` tipărește utilizările reale,
din care se actualizează lista nominală.

Validarea pasului 1 al gate-ului transversal (X-D2, 2026-10-03): **3.277
bugetar / 4.485 privat OK**, zero FAIL, exit 0,
`run-verificari/20261003-173905-898/`, pe clonele `.ClaudeX1`. Clona bugetară
a cerut aplicarea a două migrații: baza-sursă `Atlas.Conta.BackOffice` este în
urma codului cu `IntervaleTvaSiAvans` și `S3CategorieStoc`.

Pasul 2 al gate-ului transversal (X-D4, 2026-10-03). Migrația
`ExplicatieTranzactie` e scrisă în SQL (S-r4): două coloane, două CHECK-uri,
FK-ul și indexul lui pe `Tranzactie`; relația e și în modelul EF, ca
inserarea și ștergerea să respecte ordinea. `Explicatii.VerificaAcoperire`
intră în lista N-r8 ca martor. Proba HTTP `nou/tools/ProbeHttp/explicatii.py`
rulează pe o clonă de unică folosință a unei baze private cu seed: bonul
rămâne stornat, deci baza nu se refolosește (rețeta:
`run-verificari/x2-expl-http.ps1`). Validare: nucleu **180/180**; integrala
**3.309 bugetar / 4.518 privat OK**, zero FAIL, exit 0, build fără
avertismente, 53 de scene per profil sub `INV-CUB`,
`run-verificari/20261003-182901-833/`, pe clonele `.ClaudeX2`; SC-CIT-99 8/8
PASS, `run-verificari/x2-expl-http/proba.log`; OpenAPI și tipurile TS
regenerate, cu schimbări numai aditive în `api-types.ts`.

Pasul 3 al gate-ului transversal (X-D6, 2026-10-03). Fără migrație și fără
schimbare de contract HTTP. Trei probe existente presupuneau două comenzi
simultan în secțiunea de scriere și s-au rescris pe modelul serial:
SC-DES-05 (a doua deschidere așteaptă și e refuzată de domeniu; indexul unic
se probează direct în bază), SC-DES-21 (așteptarea e pe blocajul scrierii)
și SC-IMO-32 (blocajul IMO străin se ține fără blocajul scrierii).
Validare: integrala **3.361 bugetar / 4.570 privat OK**, zero FAIL, exit 0,
54 de scene per profil sub `INV-CUB`, `run-verificari/20261003-211029-736/`, pe clonele
`.ClaudeX2`; `--probe-sursa` verde.

Pasul 4 al gate-ului transversal (X-D7, 2026-10-03). Fără migrație și fără
schimbare de contract HTTP. `ContaSeeder.SeedTipuriDocument` este public, ca
proba seed-ului să-l poată chema nesalvat. Validare: integrala **3.387
bugetar / 4.597 privat OK**, zero FAIL, exit 0, 54 de scene per profil sub
`INV-CUB`, `run-verificari/20261003-214312-683/`, pe clonele `.ClaudeX2`;
`--probe-sursa` verde.

Pasul 5 al gate-ului transversal (X-D5 + X-D3, 2026-10-04): vezi „Scara
transversală de perf”. Migrația `IndecsiCititoriCub` adaugă cinci indecși.

Pasul 6 al gate-ului transversal (X-D8, 2026-10-04). Fără schimbare de
producție. Validare: integrala la `ff08a8c` **3.387 bugetar / 4.597 privat
OK**, zero FAIL, exit 0, `run-verificari/20261004-081014-672/`, pe clonele
`.ClaudeX2`; nucleu **180/180**; `--probe-sursa` verde; `verifica:drift`
exit 0; probele HTTP în `run-verificari/x6-http/` (matricea 300/300 de două
ori, `scriere-ocupata.py` 7/7, `comenzi-coaja.py` 28/28, `explicatii.py`
8/8). Regula feliei: decizia 108.

Review-ul implementării (Codex, 2026-10-04) a adus patru observații,
X-RI1…X-RI4, toate corectate: mecanismul explicației ținut de declarant și
soldul citit al partidei stinse, conservarea pe cont a transferului
persistat, planul respins care invalidează măsurarea, controlul D300 pe
rezultatul D300. Validare după corecturi: integrala **3.394 bugetar / 4.604
privat OK**, zero FAIL, `run-verificari/20261004-092403-010/`;
`--probe-sursa` verde; scara completă
`run-verificari/perf-cub-20261004-093104/`, 1.526 / 996 OK, zero FAIL.
Reverificarea Codex la `7223abc` a închis X-RI1…X-RI4, cu integrala proprie
3.394 / 4.604 OK (`run-verificari/20261004-102241-857/`). TR-D8 este închis
(decizia 108) și mers în main prin PR #15.

Cititorii TVA/D300/D394/TaxInformation sunt portați prin 103. Snapshot-ul de stoc folosește cubul.
Nucleu: **180/180**, zero omise, exit 0:
`run-verificari/20260924-124628-047/rezultat.json`.
Comenzile, încercările intermediare și limitele sunt în
[DESCHIDERE.md](../nucleu/scenarii/DESCHIDERE.md), NIR în
[NIR.md](../nucleu/scenarii/NIR.md); lanțul Folosinta în
[LDI.md](../nucleu/scenarii/LDI.md); review-ul ASM în
[ASM.md](../nucleu/scenarii/ASM.md). S-r10 este reparată: purja SAF-T
șterge întâi regulile `DinSeed` atașate tipurilor temporare ale scenei.

## Organizarea sursei

| Zonă | Responsabilitate |
|---|---|
| `nou/Atlas.Conta.BackOffice/Atlas.Conta.BackOffice.Module` | Model, motor, DTO/Apply, proiecții, ANAF, SAF-T, seed și migrări (42d) |
| `nou/Atlas.Conta.BackOffice/Atlas.Conta.BackOffice.WebApi` | Contracte HTTP, securizarea comenzilor, OData și integrarea hostului (42f) |
| `nou/Atlas.Conta.BackOffice/Atlas.Conta.BackOffice.Blazor.Server` | Host XAF, administrare și actualizarea explicită a bazei (23a) |
| `nou/Atlas.Conta.Client` | React: citiri, proiecții, consolele comenzilor, editorii de politici și nomenclatoare și contractele generate; paginile de document sunt înghețate (43e, 104d) |
| `nou/Atlas.Conta.Nucleu` | Nucleul pur al cubului de postări: tipuri, conservare, unitate, FIFO, evaluare, repartizare, TVA, storno, motor, gestiunile virtuale — fără niciun pachet; consumat DOAR de `Module` (`Declaratii/`), referit direct și de ModelCheck ca unealtă (90b, 90l) |
| `nou/Atlas.Conta.Nucleu/Atlas.Conta.Nucleu.Teste` | Invarianții nucleului ca proprietăți pe generatoare proprii și testul de arhitectură (90l) |
| `nou/tools/ModelCheck` | Verificarea modelului și scenarii de domeniu pe PostgreSQL (23) |
| `nou/tools/ProbeHttp` | Probe ale contractului HTTP și ale permisiunilor reale (80i, 81j) |
| `nou/tools/Import1C` | Import operațional și reconcilierea sursei (45f); `ANALYZE` după ultima scriere (SAFT-r5) |
| `nou/tools/Migrare` | Prototipul migrării nomenclatoarelor și soldurilor legacy (34, 35a); `ANALYZE` după ultima scriere (SAFT-r5) |
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

Lanțul de migrații a fost comprimat la 2026-09-25 (C102, 102e): baza inițială
este `20260925110419_InitialCreate`, generată din model, plus SQL-ul
brut pe care modelul nu-l declară (`Postare` partiționată pe `Spatiu` cu
cheia `(Spatiu, ID)`, FK-urile și indecșii pe partiții, constrângerile
`CK_Postare_*`, funcția `cub_partida_id`). Migrațiile nu transformă date.
Proba A/B: `pg_dump --schema-only` pe baza din lanțul vechi complet și pe
baza din migrația comprimată are aceleași 593 de instrucțiuni. După
normalizarea ordinii coloanelor și a tokenurilor `pg_dump`, diferă numai 10
valori `DEFAULT` lăsate de `AddColumn` pe 5 tabele (`UrmarestePartide`,
`Pozitie`, cele șase câmpuri din `PartideDeschise`, `TolerantaTaxa`,
`PosteazaInCub`), pe care modelul nu le declară. Migrațiile de dinainte
sunt istorie în git; bazele create pe lanțul vechi nu se actualizează, se
recreează (102b). Lanțul crește prin migrații, ca înainte.
`20260925151359_SnapshotStocCub` înlocuiește cheia snapshot-ului de stoc
cu lot/cont/produs/gestiune și păstrează data deschiderii. Nu convertește
snapshot-uri vechi. Pe o tabelă goală migrația se aplică direct; datele
incompatibile cer recrearea bazei conform 102(b).
Citirile cumulate au o probă pe două conexiuni și un contor de instrucțiuni
SQL (SC-CIT-77). Scrierea globală prin ObjectSpace secured este refuzată
înaintea accesului la date (SC-CIT-78). Accesul real pe rând și membru este
verificat separat prin `nou/tools/ProbeHttp/stoc-snapshot-cub.py --baza
NUMELE_BAZEI_IZOLATE` (SC-CIT-74), cu seed privat și utilizatori.
Dovezile rulărilor și inventarul bazelor sunt în review și în
`docs/decizii/istoric-plan-de-lucru.md`.

Comanda `dotnet ef` primește mereu `--context BackOfficeEFCoreDbContext` și
se rulează fără `--no-build`. (23a, 89g)

Bazele de dezvoltare se recreează, nu se repară: o bază care nu corespunde
codului se șterge (`DROP DATABASE`) și se reface prin comenzi (102b). Rețeta,
cu `CS = Host=localhost;Port=5444;Username=postgres;Password=postgres`:

| Bază | Recrearea |
|---|---|
| `Atlas.Conta.BackOffice` (bugetar; și baza ModelCheck bugetar) | din `Module`: `dotnet ef database update --context BackOfficeEFCoreDbContext --connection "$CS;Database=Atlas.Conta.BackOffice"`; apoi din `Blazor.Server`: `dotnet run --no-launch-profile -- --updateDatabase --forceUpdate --silent` cu `ProfilContabil=Bugetar` și `ConnectionStrings__ConnectionString=EFCoreProvider=Postgres;$CS;Database=Atlas.Conta.BackOffice` în mediu |
| `Atlas.Conta.ModelCheck.Privat` | o recreează ModelCheck (`MigrateAsync` + seed) |
| `Atlas.Conta.BackOffice.Privat` (baza hosturilor) | `dotnet ef database update … --connection "$CS;Database=Atlas.Conta.BackOffice.Privat"`, apoi updater-ul Blazor cu `appsettings.json` (Privat): seed plus utilizatorii `Admin`/`User`/`Cititor`/`Configurator`, fără documente |
| `Atlas.Conta.Import1C.Flax` și clonele ei | nu se recreează implicit; Import1C `--recreeaza` la nevoie (091-r4) |

Recrearea din 2026-09-25 (C102) a șters toate bazele Atlas.Conta de pe
5444 (clonele de import, review, perf `Nucleu.Fizica.x1/x10`, CodexBCS,
ClaudeRev) și a refăcut cele trei de mai sus. Pe baza Privat din seed,
`partide-cub.py` își creează singur fixture-ul. (89g, 102b)
Comenzile de document au proba HTTP proprie, cu fixture creat și șters de ea:
`nou/tools/ProbeHttp/comenzi-coaja.py` (404/403/422 pe cele cinci comenzi,
ușa altui tip, fără scriere la refuz). (104b)
Matricea generală `refuzuri.ps1` rulează pe baza Privat din seed cu fixture
propriu pe prima lună deschisă a lanțului: un furnizor NeinregistratRo, o FCT
operată cu plata conex operată și împerecherea lor, un ITV draft pe aceeași
lună și un angajat, desfăcute în ordine inversă în `finally`. Rămân auditul
și cele două `RefuzSeed` ale rândurilor de politică de probă (104i, chei fixe:
a doua rulare nu adaugă nimic). Plafonul de 500 al candidaților DVI e probat
în ModelCheck, nu pe HTTP. Măsurat 2026-09-27 pe `c104-straturi`: 294/294
PASS de două ori consecutiv, baza identică înainte și după. (104-r5)
Matricea conține și ușa explicației (X-D4): tranzacția facturii fixture-ului
se află din jurnal, `Admin`, `Cititor` și `Configurator` primesc 200, iar
`User` 404, la fel ca pe un id inexistent. 403
`EXPLICATIE_ACCES_INCOMPLET` cere un rol cu citire restricționată și rămâne
al probei `explicatii.py`. Măsurat 2026-10-04 pe `tr-d8-transversal`, pe o
bază privată recreată din seed: 300/300 PASS de două ori consecutiv. (X-D8)
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

Unicitatea politicilor și a codurilor de nomenclator este în schemă, prin
indexuri unice nefiltrate: ștergerea e fizică, deci cheia unui rând șters se
poate reface. Ancora `TipDocument.ClrType` este unică tot
așa (`IX_TipuriDocument_ClrType`); discriminatorul documentelor nu are FK
spre ea, iar corespondența clase concrete ↔ seed o probează ModelCheck. (81c, 89a, 104f)

Curățenia de scenă din ModelCheck (`Purja`) șterge fizic, prin SQL, și
emulează cascada pe FK-urile obligatorii: rândurile care nu pot exista fără
părinte pleacă înaintea lui. Schema nu mai cascadează în afara compozițiilor
(104g). Probele care cer o etichetă de nomenclator ascunsă (SC-CIT-29/40/80)
șterg rândul într-o tranzacție rulată înapoi, cu FK-urile suspendate
(`AscundereControlata`). (F13-D2, 104g)

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
rulate selectiv pe privat; durata depinde de grupul ales. Codurile sunt ale catalogului (`docs/nucleu/scenarii/`,
inclusiv `DESCHIDERE`, `IMO`, `X`); un cod necunoscut sau un tip fără nicio
scenă pe profilul cerut iese cu exit 2, nu verde. Scenele și tipurile lor
stau în `ScenelePeTip` (`Program.cs`), aceeași listă pe care suita integrală
o rulează în ordine; un scenariu nou intră acolo cu tipul lui, altfel filtrul
nu-l vede. Suita integrală pe ambele profiluri rămâne gate-ul de commit.
(091 (e), 091-r1)

### Rulare reproductibilă (`scripts/verifica.ps1`)

Din rădăcină, cu PowerShell 7 și .NET 10:

```powershell
pwsh -NoProfile -File nou/tools/ModelCheck/scripts/verifica.ps1 -Suita Scenarii -Tip BCS -Profil Ambele -Sufix .CodexBCS -PregatesteBaze
pwsh -NoProfile -File nou/tools/ModelCheck/scripts/verifica.ps1 -Suita Nucleu
pwsh -NoProfile -File nou/tools/ModelCheck/scripts/verifica.ps1 -Suita Integral -Profil Ambele -Sufix .CodexBCS
pwsh -NoProfile -File nou/tools/ModelCheck/scripts/verifica.ps1 -Suita Infrastructura
```

`-PregatesteBaze` (Python cu `psycopg`) clonează bazele locale de profil
`Atlas.Conta.BackOffice` și `Atlas.Conta.ModelCheck.Privat` cu sufixul dat
(`CREATE DATABASE … TEMPLATE`, localhost:5444, postgres/postgres): sursele
nu se modifică și trebuie să nu aibă conexiuni active; o clonă existentă se
păstrează, nu se reface. Clona poartă schema și seed-ul sursei: bugetarul
cere migrațiile aplicate, privatul migrează și aliniază seed-ul prin
ModelCheck. Baza bugetară absentă sau cu migrații neaplicate dă **exit 2**,
fără rezumat.

Scriptul compilează, rulează profilurile succesiv (bugetar, apoi privat) și
se oprește la primul exit nenul; excepția unei scene selectate e raportată
cu stack trace și exit 1. Un mutex refuză două invocări simultane pe aceeași
sesiune Windows (nu coordonează hosturi sau comenzi lansate manual; regula
unei singure rulări grele rămâne). Hostul Blazor trebuie oprit dacă folosește
același `bin`. Artefactele sunt în `run-verificari/<timestamp>/` (ignorat de
git): log per etapă și `rezultat.json` cu commit, fișierele modificate,
profil, bazele exacte, SHA-256 al DLL-ului ModelCheck, argumente, durate și
coduri de ieșire — cu modificări locale, manifestul (nu commit-ul) identifică
sursa testată.

Pe Windows, wrapperul activează local procesului modul fără dialoguri de
eroare critică sau crash (`SEM_FAILCRITICALERRORS | SEM_NOGPFAULTERRORBOX`),
moștenit de procesele copil, și restaurează modul anterior în `finally`.
Nu schimbă configurarea globală Windows. Mecanismul este documentat de
[Microsoft — SetErrorMode](https://learn.microsoft.com/en-us/windows/win32/api/errhandlingapi/nf-errhandlingapi-seterrormode).
O excepție .NET necapturată păstrează stack trace-ul în log și codul nenul
al etapei; wrapperul oprește seria cu exit 1, fără a aștepta închiderea
unui dialog. Protecția se aplică rulărilor prin acest wrapper, nu comenzilor
lansate separat.

`-Suita Infrastructura` probează acest comportament fără acces la baze:
compilează o consolă izolată, verifică moștenirea modului, provoacă o
excepție necapturată și apoi pornește cu succes un al doilea proces.
Doar procesele acestei probe au termen de 30 s; wrapperul nu introduce un
timeout general pentru ModelCheck. Validat la 2026-09-23:
`run-verificari/20260923-224135-701/rezultat.json`, exit 0; căderea produce
`0xe0434352` / `-532462766`, stack trace prezent, procesul următor exit 0,
fără intervenție. Compilarea probei: zero avertismente și erori.

Refuzurile se probează pe două uși: codul stabil (`CoduriRefuz`, linia
`COD: mesaj`) pe ușa declarației (`Materializare.Refuzuri`); pe ușa entității
(`ComenziDocument.Valideaza` / `Opereaza`) validarea veche a clasei și gardienii
registrelor refuză cu text ÎNAINTEA declarantului, deci acolo se asertează
familia mesajului și absența efectelor, iar un refuz al registrului se
marchează ca atare în fișierul tipului (până la TR-D8). Cataloagele
[BCS, FCT, PLT/INC, BTR și FCL/DSC](../nucleu/scenarii/README.md) au primele
loturi independente verificate (2026-09-23), cu fixture prin documentele
reale și comenzile `ComenziDocument`; probele `NUC-*` rămân regresie. Sumele directe ale scenariului
peste cub nu înlocuiesc verificarea cititorilor comuni și a `Sold` la TR-D8.

Cataloagele [NTC](../nucleu/scenarii/NTC.md) și [ITV](../nucleu/scenarii/ITV.md)
au probe independente în `ScenariiNtc` și `ScenariiItv`, cu recensământ
reproductibil în `recensamant-ntc-itv.sql`. NTC este activat pe ambele
profiluri, ITV numai privat; bugetarul probează `ProfilInert`. Decizia 092
extinde cheia partidei la document × cont × partener; SC-NTC-13 verifică
separarea, SC-NTC-20 compatibilitatea cu identitatea istorică. SC-NTC-22
verifică dependența FIFO între data nominalizării și data inversării ei.
Rezultatele rulărilor sunt consemnate în fișierele tipurilor.

Verificarea de drift regenerează contractele și refuză diferențele față de
fișierele versionate. O schimbare intenționată de contract se regenerează și
se examinează înainte de includerea artefactelor în modificare. (43d, 56)

### Scara transversală de perf (`scripts/perf-cub-container.ps1`)

`ModelCheck --perf-cub [privat]` construiește scena de volum (k unități în
luna măsurată, m luni închise de istoric, faptele „o dată per bază”) și
măsoară fiecare cititor comun într-un proces nou, rece și cald, pe ușa
securizată și pe cea nesecurizată. Fiecare cifră citită se compară cu
așteptarea scenei. La k maxim, citirile se reexecută sub `EXPLAIN (ANALYZE,
BUFFERS)`, cu planul ales și fără scanare secvențială. Un plan respins e
eroarea măsurării, iar o operație fără plan pentru fiecare citire pică
criteriul; proba `X-D5-PLAN`, care rulează în orice ModelCheck, ține regula
(X-RI3). Înaintea purjei rulează
reconcilierea integrală, `INV-CUB` și diagnosticul ASM-B7 pe toată baza.
Rețeta rulează ambele profiluri în containerul din rețeaua Postgres și
validează XML-urile SAF-T cu DUK; parametrii `-Profil`, `-Istoric`, `-Trepte`
și `-Operatii` restrâng rularea. Scara completă durează circa 35 de minute și
este o rulare grea: nu se suprapune cu alta pe aceleași baze. O rulare
întreruptă lasă scena în bază; următoarea o purjează la pornire, dar datele
societății de pe profilul privat rămân cele ale scenei și se refac de mână.
Un cititor nou care citește cumulat sau pe interval intră în `PerfCub.Operatii`
cu ruta și cifrele lui de control. (X-D5, X-D3)

## Verificări proporționale cu modificarea

| Schimbare | Verificare necesară |
|---|---|
| Model, motor, politici, proiecții | Build și ModelCheck pe ambele profiluri (23) |
| DTO, atribute, expunere API | Build WebApi, regenerare și verificarea contractelor; probe HTTP pentru comportamentul afectat (56, 80i) |
| Autorizare | Probe HTTP cu rolurile reale; o probă pe context nesecurizat nu demonstrează securitatea (80i, 81j) |
| Formular sau interacțiune | Build client și verificarea fluxului în browser (66) |
| Culegerea (L3: precompletare, formula valorii, normalizare, regulile culegerii) | ModelCheck pe ambele profiluri: `104c-S1` (adaptorii `Api/` și `Controllers/` nu cheamă serviciile culegerii, scanare pe sursă), `104c-E1` (aceeași linie pe calea XAF și pe calea API), `104c-E2`, `104c-V1…V2` (refuzurile gardianului). Probele de scară și de rol trec prin ușa cu gardian (`OsCuGardian`). Fluxul XAF se verifică în browser: creare, produs, cantitate și preț, salvare (104c) |
| Mod de acces al unui ListView XAF, proprietate nouă afișată în liste | ModelCheck (`D85-M1`, `D85-M2`, `D85-R1…R3`) și deschiderea listei în browser pe baza de import: sort, filtru, grupare, detaliu din listă, culegere pe document nou (85h) |
| Schimbare de postare/evaluare | Catalogul cu așteptări independente, apoi ModelCheck integral pe ambele profiluri (091); importul și reconcilierea externă aparțin feliei de migrare |
| Tip derivat nou, proprietate nouă pe frunză, FK spre o frunză | ModelCheck pe ambele profiluri (`F28-*`); după un import, `--dump-integritate-tph` rulat pe baza de import (89e, 89h) |
| Nucleul pur (`Atlas.Conta.Nucleu`) | `dotnet test` pe soluția nucleului: testul de arhitectură și invarianții 1–6 ca proprietăți (≥ 500 de cazuri fiecare); ModelCheck doar dacă e atins `Module` (90l) |
| Declarant, operand, `Fapte.Operand`, oracolul pilotului | Scenariile independente ale tipului, apoi ModelCheck pe AMBELE profiluri; `NUC-*` păstrează comparația normalizată ca regresie, conservarea, determinismul și `≤ 16` interogări per operand. `Metadata clientului e la zi` verifică proprietățile noi pe `Document` (TR-D6b, amendat de 091) |
| Citire nouă din cub, în orice proiect de producție | Intrare în `Cub/Citiri`, apoi ModelCheck: `091-r3` refuză accesul la `Postare`/`Tranzactie` în afara ei și a excepțiilor numite (scanare pe sursă, fără bază) |
| Entitățile sau migrațiile cubului (`Postare`, `Tranzactie`) | ModelCheck pe ambele profiluri: probele `STR-SCHEMA-*` (partiționarea LIST, cheia `(Spatiu, ID)`, setul ÎNCHIS de FK-uri per partiție, indexii, absența timbrelor XAF); migrația se scrie în SQL, nu se lasă generată (S-D2, S-r4) |
| Contractul laturilor (`Document.Laturi()`, T-D13) | ModelCheck pe ambele profiluri, ultima scenă (`VerificaLaturi`): `STR-LATURI-CONTRACT` (fiecare `TipDocument` din seed → clasa → contract cu părți nevide; metoda e abstractă, deci și compilatorul o cere), `STR-LATURI-REFUZ` (latura de partea greșită refuzată pe ușa declarației și pe ușa entității cu ACEEAȘI linie `COD: mesaj`; calitatea lipsă numită; un tip fără declarant refuzat pe ușa entității), `STR-LATURA` (PLT inversată = doar `PREDATOR_NEPOTRIVIT`, înaintea declarantului). Probele de laturi ale tipurilor asertează CODUL, nu textul vechi. Pe date reale: recensământul laturilor pe clona Flax (contract T-D13); după 091 clona e sursă de recensământ, nu gate |
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
Invariantul 7 al designului (baseline-ul Import1C) și N-r1 sunt depășite de
091; proba supremă este catalogul de scenarii. Reflecția probează că niciun record
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

Diagnosticul reconcilierii cubului are două unelte, ambele în ModelCheck și
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

Exit-ul `--reconciliere-cub` depinde numai de (a)–(g). Diagnosticul valoric
pe lot × gestiune × cont din ASM-B7 rămâne raport; identifică și numără
separat mișcările din afara domeniului Magazie/Marfuri/Folosinta, fără să excludă
postările cubului cu istoric lipsă. ASM mixt este probat prin
`NUC-ASM-RECONCILIERE`: Operare ASM este exclusă nominal din (a), numai în
regimul dual (D8-B4 aprobat de owner, T-r15). (h) raportează exact D 40/C 40
față de zero în registre, 1 document și 4 postări Operare. (a)–(g) rămân
fără diferențe, exit 0; raportul declară excepția. Verificările independente
pe cub rămân obligatorii; egalitatea completă cub–registre nu este afirmată.

Rețeta istorică a verificării importului este `run-nucleu/tr-d7a/import/run.ps1`: Import1C integral
(`--recreeaza --cititori --inchide-lunile`), apoi `--reclasifica`,
`--reconciliere-cub`, `--dump-integritate-tph` și `diff-sortat.py`, care compară
raportul de reconciliere cu baseline-ul pe conținut sortat. După 091 aceasta
aparține migrării; nu se execută pentru validarea unei felii de motor.

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
un director temporar accesibil procesului. Validarea D406 (XSD + DUK) folosește
numai artefactele pin-uite prin SHA-256 în `ManifestD406`: XSD v249 versionat
în `nou/tools/ModelCheck/Anaf/` (namespace substituit declarat), kitul DUK
J2.2.18 și nomenclatorul din `anaf/` (gitignored; alt director prin
`ATLAS_ANAF`). Kitul absent sau schimbat (inclusiv în timpul rulării) pică
proba, nu o sare; perioada validatorului vine din antetul fișierului, fiindcă
ea alege nomenclatorul. Fiecare rulare scrie `manifest-d406.json` lângă
fișierele validate, cu proveniența liniilor GL și a facturilor pentru
fișierele certificate. (S0-R1…R4, S0-R8) Jurnalele publicate nu includ
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

Verificarea istorică a trecerii pe TPH a cerut ca importul integral cu
`--recreeaza --cititori --inchide-lunile` să dea un raport IDENTIC pe
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
