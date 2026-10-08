# Restanțele și amânările cu nume

**Actualizat: 2026-10-08 (B-r3 închisă, decizia 112; 110-r1 închisă, decizia 113; 110-r3 restrânsă; 110-r6 deschisă).** [Index](README.md)

Backlog-ul dezvoltatorului: fiecare rând e o amânare declarată într-o decizie.
Identificatorul spune unde e textul integral — `36f` = sub-punctul (f) al
deciziei 36, `69-r1` = restanța 1 a deciziei 69, `D4-r1` = restanțele
D394 (decizia 71), `DIM-4` = pasul 4 al feliei dimensiunilor (decizia 54),
`C1a` = `docs/architecture-notes-2026-07-28.md`. `NG-rN` =
`docs/consultations/2026-10-05-nod-si-galeata/README.md`, secțiunea „Ce s-a reținut”. Numele stă aici ca să nu se
piardă; motivul și contextul stau doar în fișierul deciziei. O restanță
închisă își schimbă starea, nu dispare. Limitele produsului, în forma
văzută de utilizator, sunt în
[`stare-curenta/limite-curente.md`](../stare-curenta/limite-curente.md).

**Stările (triajul 091, 2026-09-22, litera (k))**: `activă` = blochează
„rotund" (regula de oprire a PoC-ului, 091 (g)); `după PoC` = cerință de
produs, ecran, host sau perf, se rejudecă pe modelul rotund; `migrare` =
conectorul 1C, Flax, date, intră în felia de migrare (091-r4); `cade la
TR-D9` = mecanismul dispare cu registrele, nevoia rămasă se rejudecă pe cub;
`depășită de 091` = era a gate-ului Import1C. O restanță nouă intră direct
cu una din stări; lista `activă` trebuie să încapă pe un ecran.

| Id | Restanța | Stare |
|---|---|---|
| 21 | defalcarea multi-sursă (F) | după PoC (091) |
| 31f | importul extraselor, imperecherea pe poziții | după PoC (091) |
| 31e | `GenereazaChitanta` (fluxul BF) | după PoC (091) |
| 34g | satelitul partenerilor (IBAN/delegați/contact/adrese multiple; adresa e pe `Partener`, 72a), valute | după PoC (091) |
| 36f | TVA la încasare, facturi nesosite 408/4428, rotunjirea per document×cotă, prorata/ajustări | după PoC (091) |
| 37g | comenzi, regenerarea DSC la recepția NIR, multi-gestiune per factură, rezervarea de stoc | după PoC (091) |
| 46f | imperecherea returului, toleranța ASM, disciplina de apelant ITV | după PoC (091) |
| 51e | `PoliticaEvaluare` (CMP) — sub 90c devine parametrul „unitatea de evaluare” al unității nominalizate | activă (091); TR-D9b (110) |
| 52h | consumul ASM pe proveniență, reziduul TRANZIT | după PoC (091) |
| 53i | culegerea de produs pe ASM, localizarea shell-ului, perioadele fiscale manuale (≠ 2026 se adaugă de mână — continuată ca F27-r13), `Data` pe conexe | după PoC (091); parțial: ASM închis de 76b |
| DIM-4 | curatoria grilei registrului, vizibilitatea dimensiunilor per profil (`SetareProfil`) | după PoC (091) |
| 55g | JWT secrets la deploy, `$metadata` expune tot modelul, `Lot.Eticheta` pe OData | după PoC (091) |
| 62g/66j | finisaj de client și ecrane XAF (`lista-react.md`) | după PoC (091) |
| 63f | laturile interne pe `Calitati`, retrofit `MaterializeazaValori` pe BTR | închisă la TR-D9a (110), fără obiect: cele trei resturi nu privesc registrele (inventar §12) |
| 64h | dimensiunea Repartitor pe rândul de bani (decizie proprie) | închisă la TR-D9a (110): niciun cititor de sold nu mai grupează pe laturile documentului (inventar §12); partenerul stă pe postare, probat de cele 14 Check-uri `D9-A10` și de SC-PLT-08 |
| 64k | comisionul bancar, valuta | după PoC (091) |
| 67e | gardian de nomenclator pentru ciclul din `Cont.Parinte` | după PoC (091) |
| 68j | smoke vizual, storno/regimuri fără TVA în backfill | după PoC (091) |
| 69-r1 | versionarea formularului D300 | după PoC (091) |
| 69-r2 | rândurile intracomunitare/agricultori/pro-rata/secțiunile A-B | după PoC (091) |
| 69-r3 | regularizările pe cauză juridică | după PoC (091) |
| 69-r6 | fișierul XML D300 | după PoC (091) |
| 70-r1 | refuzurile gardianului pe scrierile OData ies `400 text/plain` (închisă de 77g + 80d) | închisă de 77g + 80d |
| 70-r2 | smoke XAF al gardianului de ciclu | după PoC (091) |
| 70-r3 | poziția „linia N" fără criteriu | după PoC (091) |
| 70-r4 | `DirectiePentru` per `ObjectChanged` | după PoC (091) |
| 70-r5 | mesajele de binding în engleză | după PoC (091) |
| 70-r6 | `TvaSuprascris` | după PoC (091) |
| D4-r1 | istoricul statutului de TVA (canonicul = registrul ANAF) | după PoC (091) |
| D4-r2 | adresa PF fără CNP | după PoC (091) |
| D4-r3 | `N` + `tip_document` | după PoC (091) |
| D4-r4 | data primirii facturii | după PoC (091) |
| D4-r5 | categoria `op11` (codul NC e pe produs, 73b) | după PoC (091) |
| D4-r6 | bonurile fiscale (G, I.1) | după PoC (091) |
| D4-r7 | I.2 facturi/plaje/autofacturi/anulate/simplificate | după PoC (091) |
| D4-r8 | I.3 | după PoC (091) |
| D4-r9 | sumele TVA la încasare (I.4/I.5) | după PoC (091) |
| D4-r10 | CAEN/I.6/opțiune (antetul/reprezentantul sunt pe `Societate`, 73a) | după PoC (091) |
| D4-r11 | partenerul cu două coduri (SM + RO) | după PoC (091) |
| D4-r12 | achizițiile de pe DEC fără furnizor | după PoC (091) |
| D4-r13 | XML D394 (35c; precedentul `SaftXml`) | după PoC (091) |
| 72-r1 | rate-limit ANAF cross-request | după PoC (091) |
| 72-r2 | adresa hibridă 1C + ANAF fără proveniență per câmp | după PoC (091) |
| 72-r3 | `IntrariStricate` = fatală după commit | după PoC (091) |
| 72-r4 | radierea (`StareInregistrare`) neconsumată | după PoC (091) |
| 72-r5 | smoke vizual XAF al acțiunii ANAF | după PoC (091) |
| 72-r6 | `RegistruContraAnaf` real abia la a doua rulare | după PoC (091) |
| 72-r7 | ordinea la egalitate în raportul de reconciliere | după PoC (091) |
| 72-r8 | `CuiInterogabil("00")` | după PoC (091) |
| 72-r9 | ecranul React de partener | după PoC (091) |
| 72-r10 | `User` ⇒ 404 vs 403 pe comanda ANAF (închisă de 80g) | închisă de 80g |
| 73-r2 | `T`/`R`/nerezidenți | după PoC (091) |
| 73-r3 | segmentarea | după PoC (091) |
| 73-r4 | nomenclatorul NC8 + corecturile `ml`/`pac`/TARIC | după PoC (091) |
| 73-r5 | `Produs.UM` string | după PoC (091) |
| 73-r6 | ecranul React `Societate`, `CodNc`/UM pe produs | după PoC (091) |
| 73-r7 | OpANAF 1783/2021 (termene/praguri) | după PoC (091) |
| 73-r8 | kitul DUK J2.2.8 vs J2.2.15, `-d` inutilizabil | închisă 2026-09-29 de TR-D8 S0-R1/R2: kitul local e J2.2.18 publicat (SHA-256 pin-uit), verificat înainte și după fiecare rulare |
| 73-r9 | `ContFaraRol` pe facturile de achiziție Flax (date) | migrare (091) |
| 73-r10 | `Neincluse` întreg în sumar | după PoC (091) |
| 73-r11 | `UnitPrice` la 2 zecimale, cantitatea negativă nemăsurată | după PoC (091) |
| 73-r12 | 64h (`PartenerulRandului`) | după PoC (091) |
| 73-r13 | `100030` ≠ `InactivFiscal` | după PoC (091) |
| 73-r14 | conducătorul rupt la spațiu, `InregistratTva` derivat | după PoC (091) |
| 73-r15 | `CuiPrefixDublat` nemăsurat | după PoC (091) |
| 73-r16 | multi-valută | după PoC (091) |
| 73-r17 | categoria `op11` | după PoC (091) |
| 73-r18 | contact/IBAN parteneri | după PoC (091) |
| 73-r19 | 422 pe CUI gol/invalid nemăsurat pe HTTP | după PoC (091) |
| 74-r1 | `Owners`/`8038`, custodie | după PoC (091) |
| 74-r2 | `ShipTo/From` pe BTR, `MovementComments` | după PoC (091) |
| 74-r3 | `InvoiceNo` duplicat în L (date 1C) | migrare (091) |
| 74-r5 | soldul `Consum` neraportat (220 k / 340 k) | după PoC (091) |
| 74-r7 | `Neincluse` per produs în sumar | după PoC (091) |
| 74-r8 | stornoul S doar pe scenă | după PoC (091) |
| 74-r10 | `T`/segmentare pe S | după PoC (091) |
| 74-r11 | D17-V6 doar privat, proba ștergerii logice | după PoC (091) |
| 74-r12 | ecran React `PoliticaMiscareSaft` (închisă de 81i) | închisă de 81i |
| 74-r13 | DUK J2.2.18 | închisă 2026-09-29 de TR-D8 S0-R2 pentru L; S se reverifică pe manifest la S3 |
| 74-r14 | gardian produs de stoc fără cont | după PoC (091) |
| 74-r15 | `FaraCodNc` pe Flax | migrare (091) |
| 75-r1 | ASM UI: derivarea valorii produsului din consum | închisă de 76c |
| 75-r2 | scalarea rezoluției de tip (`CoduriTipPeTipuri` liniar cu baza; `ApiProiectii. CoduriTip` pe hot-path API) | închisă de 89f (`CoduriTipPeTipuri`/`IdsDocumenteDeTip` au dispărut; tipul se citește din discriminator prin `CititorTipDocument`) |
| 75-r3 | `--continua` fals-roșu pe bază importată (idempotența doar prin re-rulare integrală) | migrare (091) |
| 75-r4 | `PretEvaluare` 6 zecimale pe cantități mari vs invariantul 46d | activă, TR-D9b (110): `AsamblareDetaliu.PretEvaluare` supraviețuiește tăierii; valoarea produsului ca repartizare a consumului |
| 75-r5 | stornoul fără timbru propriu în oracolul golirii | depășită de 091 |
| 76-r1 | netarea plafonului e per (repartitor × LATURĂ), nu per CONT (55 chei / 36 note amestecă conturi de clasă 4 pe aceeași latură; expunere reală 3 chei) | activă, TR-D9b (110): hook-urile de stingere nu citesc registre și au rămas neatinse (D9-Q4 = A) |
| 76-r2 | `caStins` se scade din AMBELE sensuri — inatacabil azi, **devine real când un tip cu partener pe latură capătă capacitate bidirecțională** | activă, TR-D9b (110): hook-urile de stingere nu citesc registre și au rămas neatinse (D9-Q4 = A) |
| 76-r3 | perf `AsignatFataDe` (entități polimorfe, chemat de 2 × nr. contrapartide); hook-urile de stingere dispar sub 90e | activă, TR-D9b (110): hook-urile de stingere nu citesc registre și au rămas neatinse (D9-Q4 = A) |
| 76-r4 | gate-ul comenzilor e pe `Document`, nu pe tipul feliei (422 vs 404 pe aceeași cauză; închisă de 80b) | închisă de 80b |
| 76-r5 | `Candidati` sub-raportează pe ușa secured, iar `User` pe ușa de scriere e refuzat de primul FK invizibil, nu de o permisiune (familia 72-r10; închisă de 80b/80f) | închisă de 80b/80f |
| 76-r6 | patru itemi de client în `lista-react.md`: căutarea sensibilă la diacritice în TOATE lookup-urile remote (colație `unaccent`/ICU sau coloană shadow — decizie de bază de date), `Lookup` care refetchează eticheta per instanță, limita convenției 61b pe valorile din PRECOMPLETARE, `window.confirm` moștenit pe ștergere (toți patru închiși de 77) | închisă de 77 |
| 77-r1 | BTR fără convenția 61b | depășită de 104 (2026-09-27): paginile React de culegere sunt înghețate (104d); soarta lor e 104-r4 |
| 77-r2 | `Cod`/`Denumire` neobligatorii pe nicio ușă (închisă de 77k) | închisă de 77k |
| 77-r3 | editarea `PoliticaMiscareSaft` din React (închisă de 81i), comanda ANAF de lot | după PoC (091); parțial: editarea închisă de 81i, comanda de lot deschisă |
| 77-r4 | `CodFiscal`/`Iban`/`Marca` în afara lui `Cautare` | după PoC (091) |
| 77-r5 | precompletarea nu distinge alegerea operatorului; invalidarea nu reîmprospătează `SelectBox`-urile montate | după PoC (091) |
| 77-r6 | `displayExpr` de nucleu pentru `TipMaterial` | depășită de 104 (2026-09-27): privea selectoarele paginilor React de culegere, înghețate (104d); soarta lor e 104-r4 |
| 77-r7 | `Cautare` fără index (seq scan pe 20 k rânduri; cifra decide; calea: GIN `pg_trgm`, 78e) | după PoC (091) |
| 77-r8 | permisiunea pe OData `text/plain` pe server (închisă de 80) | închisă de 80 |
| 78-r1 | căutarea din grilele XAF rămâne sensibilă la diacritice (nu trec prin `DataSourceLoader`; asumat, 44/53) | după PoC (091) |
| 79-r1 | acțiunea XAF „Generează închiderea" (închisă 2026-09-02) | închisă 2026-09-02 |
| 79-r2 | `PoliticaInchidereTva` pe OData + ecran React (închisă de 81i) | închisă de 81i |
| 79-r3 | închiderea perioadei fiscale din client (53i) | închisă de 88 (React `/perioade`: lanț, verificare cu bife, închidere, redeschidere, istoric) |
| 79-r4 | mesajul `[Range]` în engleză pe `genereaza` (70-r5) | după PoC (091) |
| 79-r5 | storno-ul unei închideri la o dată din ALTĂ lună ⇒ previzualizarea lunii raportează `FaraSold` (cauza greșită; data implicită din ecran e cea corectă) | după PoC (091) |
| 79-r6 | cine are drept de citire pe `InchidereTva` vede prin previzualizare soldurile de TVA ale societății fără drept pe `RegistruContabil` (consecința asumată a lui 79b; închisă de 80e) | închisă de 80e |
| 79-r7 | „Verifică" activ pe ne-Draft arată refuzul ca eroare (convenția NTC, transversală) | după PoC (091) |
| 80-r1 | pe conducta `ODataStore` a clientului (lookup-uri, `byKey`) refuzul ajunge ca `statusText`, nu ca mesajul serverului (limita DevExtreme `errorFromResponse`) | după PoC (091) |
| 80-r2 | mesajul plasei DevExpress din `SaveChanges` (permisiuni pe membru) iese 403 `EroriDto` cu text englezesc (70-r5) | după PoC (091) |
| 80-r3 | `ReportController` (scaffold) păstrează `NotFound()` gol | după PoC (091) |
| 80-r4 | refuzurile pe `$expand` OData nemăsurate | după PoC (091) |
| 80-r5 | captionul din mesajele 403 e numele CLR pe WebApi (fără model de aplicație XAF în host; `Refuzuri.Caption` are calea) | după PoC (091) |
| 80-r6 | `400 "Incorrect body."` englezesc prin filtrul OData (70-r5) | după PoC (091) |
| 80-r7 | `distribuie-valoarea` (ASM) întoarce sume din prețurile loturilor pe ușa non-secured fără drept pe `Lot` (familia 79-r6; nu e sumă peste registru) | după PoC (091) |
| 81-r1 | implicitul pe achiziția extra-UE / de la neînregistrat RO (cad pe ancora N21) | după PoC (091) |
| 81-r2 | seed-ul nu corectează rândurile `DinSeed` | după PoC (091) |
| 81-r3 | rândul editat înaintea migrației F23 e marcat seed de backfill | după PoC (091) |
| 81-r4 | `Repartitor.Cod` fără unicitate (spațiu partajat pe TPT) — din 89 spațiul de coduri e fizic aceeași tabelă (`Repartitori`, TPH), deci indexul ar fi trivial; unicitatea rămâne blocată de coliziunile legitime între familii din datele de import (continuată ca F28-r2) | după PoC (091) |
| 81-r5 | PATCH fără schimbare = 204 pentru orice rol (capcană de probă) | după PoC (091) |
| 81-r6 | `User` pe `api/implicite` ⇒ `Niciuna` | după PoC (091) |
| 81-r7 | XAF: lookup-urile `TipTva` fără filtrul `Activ`, baseline lipsă pe 8 politici (44/53) | după PoC (091) |
| 81-r8 | `SursaCont.Explicit == 0` pe rând nou (ecranele feliei 24) | închisă de 84d |
| 81-r9 | `400 "Incorrect body."` (80-r6) | după PoC (091) |
| 83-r1 | recrearea unui rând de seed șters, la runtime | după PoC (091) |
| 83-r2 | D394 tip `N` (achiziții de la neînregistrați) negenerat, `NIM × Achiziție` nemapat | după PoC (091) |
| 83-r3 | codul SAF-T al lui `IMP` din nomenclator | după PoC (091) |
| 83-r4 | DVI ca tip de document (`IMP21`, legătura n→m cu facturile de import, rândurile D300) | închisă de 86 |
| 83-r5 | `Configurator` în XAF (permisiuni de navigație) | după PoC (091) |
| 83-r6 | = 81-r3 asumată | după PoC (091) |
| 84-r1 | `SDD`/`SFD` fără cod SAF-T de achiziție | după PoC (091) |
| 84-r2 | `N9.CodSafTLivrare` 310310 (rd. 10.1) vs maparea pe rd. 11 (310357/310358) | după PoC (091) |
| 84-r3 | raportul de profil fără categoria „rând de seed lipsă" (lipsa unui `TipTva` referit de mapări aruncă din seed) | după PoC (091) |
| 84-r4 | întoarcerea din panou nu focalizează rândul; `Unitate` fără controller OData | după PoC (091) |
| 84-r5 | captions (`SursaCont`, membrii politicilor) fără `[XafDisplayName]` | după PoC (091) |
| 84-r6 | gate-ul `explica` pe TIP, nu pe obiect; `tip` rezolvat înaintea gate-ului (oracol pe ancore, ca `api/implicite`) | după PoC (091) |
| 84-r7 | rolul `Configurator` e al release-ului (permisiunile se reaplică; pe RELEASE fără user) | după PoC (091) |
| 84-r8 | `Cont.DimensiuniObligatorii` bugetar intră în aliniere pe `DinSeed` (expunerea lui 81-r3) | după PoC (091) |
| 84-r9 | ordinea candidaților la egalitate = ordinea bazei; dublul pe cheie pică pe index, nu pe seed | după PoC (091) |
| 84-r10 | = F24-r1 `Import1C/Catalog.IncarcaContare` pe `Potrivire` | după PoC (091) |
| 84-r11 | deep insert OData pentru `Configurator` și smoke XAF pe gardurile rescrise neprobate | după PoC (091) |
| 84-r12 | stocul pe LDI nu depinde de semn (28a; observație) | după PoC (091) |
| 85-r1 | `RegistruContabil`/`RegistruTva` pe `ServerView` după cifra de pagină pe `Server` | închisă de 85 (pe cifre, `p5-perf-masuratori.md`) |
| 85-r2 | `InstantFeedback`/`InstantFeedbackView` doar pe view-ul care trece pragul (p95 pagină > ~300 ms), cu proba în browser | după PoC (091) |
| 85-r3 | alte proprietăți nemapate care ajung în liste (`Total` pe DetailView rămâne; regula 85g le refuză din ModelCheck) | după PoC (091) |
| 85-r4 | `PageSize` implicit pe grile (cifra pe probă) | după PoC (091) |
| 85-r5 | `RegulaStoc_ListView` cu override `Server` redundant cu `Options` (se curăță la atingere) | după PoC (091) |
| 85-r6 | modul `Server` include navigațiile coloanelor ascunse (`Index = -1`); curatoria = scoase din modelul view-ului (`HideMembers`/`VisibleInListView(false)`), de măsurat pe listele grele — din 89 cauza de join a moștenirii dispare (`RegistruTva` Server: 4 JOIN-uri în loc de 35 pe baza de spike), navigațiile coloanelor incluse rămân | după PoC (091) |
| 85-r7 | gruparea pe `Server`/`ServerView` încarcă primele rânduri ale fiecărui grup (chei + entități per grup) | după PoC (091) |
| 85-r8 | `Refresh` execută pagina de două ori pe `Server` | după PoC (091) |
| 85-r9 | layout-ul salvat al utilizatorului poate ascunde toate coloanele; pe `ServerView` celulele rămân goale până la Refresh (curatoria grilelor, DIM-4) | după PoC (091) |
| 85-r10 | coloana `Produs` goală în grila Detalii a FCT `WIS26511` (date sau afișare) | după PoC (091) |
| C1a | fluxul comenzilor (`docs/architecture-notes-2026-07-28.md`) | după PoC (091) |
| 86-r1 | taxele vamale și accizele în costul de achiziție (ajustare de cost pe lot) | activă (091); TR-D9b (110) |
| 86-r2 | anularea/stornarea unei FCT legate la o DVI operată nu se refuză | activă (091) |
| 86-r3 | RLF pe DVI (retur de import / re-export) | după PoC (091) |
| 86-r4 | amânarea plății în vamă ca politică de partener | după PoC (091) |
| 86-r5 | scadență pe DVI | după PoC (091) |
| 86-r6 | unicitatea MRN-ului | după PoC (091) |
| 86-r7 | SAF-T D406 emite rândul DVI cu codul de import | închisă de 86 (probată în `E2E-DVI`) |
| 86-r8 | `DviFactura` în „Explică"/OData | după PoC (091) |
| 86-r9 | curățenia datelor Flax (reMarkable NO marcat `TI19`) | migrare (091) |
| 86-r10 | comisionarul vamal care plătește taxa și o refacturează (dublă postare) | după PoC (091) |
| 86-r11 | DVI ca document stins: restul polimorf în `ImperechereService.Total` + ramura în `DocumenteCuRest` | activă, TR-D9b (110): hook-urile de stingere nu citesc registre și au rămas neatinse (D9-Q4 = A) |
| 86-r12 | mesajul refuzului `PoateFiStins` din `ImperechereService` e scris pentru viramente | după PoC (091) |
| 86-r13 | dimensiunea Repartitor a plăților e inversată față de conturi (latura de terț poartă contul propriu) — preexistentă | închisă la TR-D9a (110): niciun cititor de sold nu mai grupează pe laturile documentului (inventar §12); partenerul stă pe postare, probat de cele 14 Check-uri `D9-A10` și de SC-PLT-08 |
| 86-r14 | `CreateObject` înaintea lui `Rezolva.Cere` în `NotaContabilaApply`/`FacturaIntrareApply` (orfan mascat de navigație) | după PoC (091) |
| 86-r15 | `IMP` (0%) rămâne `DeImport`: lookup-ul liniei DVI îl propune, operarea îl refuză | după PoC (091) |
| 86-r16 | `DocumentDetaliu_ListView` e per clasă de detaliu: al doilea tip cu `[TipDetaliu(typeof(DocumentDetaliu))]` ar împărți grila cu DVI | după PoC (091) |
| 86-r17 | `Document` fără `DefaultProperty` (85b): lookup-urile și grilele de legătură afișează GUID-ul după selecție | după PoC (091) |
| 86-r18 | lookup-urile de documente (`_LookupListView`) rămân cu coloanele generate, fără identificarea în față; `ListaRoot<T>` țintește doar `_ListView` | după PoC (091) |
| F26-r1 | activele din 231 (în curs): PIF care postează 21x = 231 (decizia 87) | după PoC (091) |
| F26-r2 | deductibilitatea valorii rămase la ieșire (art. 28 (17)) ca `Fel` nou de regulă pe rândul `Iesire` (87) | după PoC (091) |
| F26-r3 | legătura CAS ↔ FCL de vânzare ca evidență (tiparul `DviFactura`) (87) | după PoC (091) |
| F26-r4 | degresiva AD2; amortizarea pe unități de producție (87) | după PoC (091) |
| F26-r5 | transferul ca rând explicit de registru (`Fel = Transfer`, valoare 0) (87) | după PoC (110): se rejudecă pe cub, unde transferul e tranzacție proprie |
| F26-r6 | reevaluarea (locul rezervat în registru; 21x = 105 prin politică extinsă; 105 → 1175 la ieșire) (87) | după PoC (091) |
| F26-r7 | acțiunea XAF de generare AMO (87) | după PoC (091) |
| F26-r8 | SAF-T D406 Assets + AssetTransactions din `RegistruImobilizari` (87) | după PoC (091) |
| F26-r9 | D101 / impozitul pe profit ca proiecție peste `AmortizareDeductibila` (87) | după PoC (091) |
| F26-r10 | ajustările pentru depreciere (29x), leasingul, obiectele de inventar date în folosință (8035) (87) | după PoC (091) |
| F26-r11 | repartizarea cheltuielii cu amortizarea pe mai multe centre de cost cu coeficienți (87) | după PoC (091) |
| F26-r12 | `RegistruImobilizari` pe `ServerView` (85) (87) | după PoC (091) |
| F26-r13 | migrarea fișelor din 1C (`IntroducereSolduriInitialeMF` → PIF de deschidere cu inițialele): conectorul, nu mecanismul (87) | migrare (091); pe Flax, cu deschiderea în cub (107), vânzarea unei imobilizări pe 214 fără fișă e refuzată de gardianul 098 (`POZITIE_FARA_FISA_NEGATIVA`, 10.03.2025) și blochează ITV-ul lunii — devine condiție a migrării |
| F26-r14 | eligibilitatea metodei fiscale pe categorie (accelerata doar pe echipamente/calculatoare, art. 28 (12)) — azi doar documentată (87) | după PoC (091) |
| F26-r15 | cele 4 poziții-părinte din catalog cu benzile pe sub-variante fără cod: fără verificare a duratei fiscale până la o decizie (87) | după PoC (091) |
| F26-r16 | clasificația bugetară a cheltuielii cu amortizarea (codul economic) | închisă la pasul 2b (dimensiune pe fișă, `Imobilizare.CodEconomicId`, 87a/87g) |
| F26-r17 | brutul fiscal 0 pe linia PIF nu se poate exprima (`ValoareFiscala` 0 = implicit `Valoare`) (87) | după PoC (091) |
| F26-r18 | anularea unui eveniment PIF din luna unei AMO operate e refuzată deși rândul lunar nu depinde de el (`Data >=` vs „luna >") — refuz fals, pe partea sigură (87) | activă (091) |
| F26-r19 | lookup-urile XAF ale fișei (tip pe natură) și ale liniei PIF (fișe pe stare și loc) nefiltrate (87) | după PoC (091) |
| F26-r20 | `Clasificare` căutabilă în lookup-ul XAF doar pe denumire; `Valoare` a regulii de deductibilitate formatată monetar la `Procent` (87) | după PoC (091) |
| F26-r21 | filtrele `FilterRow` pe coloanele cu `Lookup` de enum (`Cauza`) neverificate în browser (87) | după PoC (091) |
| F26-r22 | două `genereaza` concurente pe aceeași lună creează două drafturi care se blochează reciproc (ca ITV) (87) | după PoC (091) |
| F27-r1 | reclasificarea pe 1174 a erorilor semnificative din exerciții anterioare; pragul de semnificație ca politică (decizia 88); sub 90k devine rând de politică (sink pe an închis → 1174) | după PoC (091) |
| F27-r2 | scadențar/aging pe partidele deschise (aceeași listă + scadența + bucket-uri) (88) | după PoC (091) |
| F27-r3 | cursa închidere ↔ operare prin blocarea verigii perioadei (88) | închisă la pasul 0 — F1 probat pe ambele capete, restanța nu s-a activat |
| F27-r4 | închiderea de an ca operație distinctă (121 → 1174/117, soldurile de deschidere ale anului nou) (88) | activă (091) |
| F27-r5 | D406/D300/D394 rectificative ca FIȘIER (marcajul de rectificativă în XML/PDF; proiecțiile expun deja conținutul) (88) | după PoC (091) |
| F27-r6 | constatări de închidere pe reconcilierea 1C (documente neimportate în P): conectorul, nu mecanismul (88) | migrare (091) |
| F27-r7 | soldul în lookup-urile de partener din culegere (coloană prin `sold-parteneri`) (88) | după PoC (091) |
| F27-r8 | concurența între operatori (25f) rămâne parcată; felia rezolvă doar cursa perioadei (88) | închisă 2026-10-03 (TR-D8 transversal, X-D6): comenzile care scriu sunt seriale per bază prin blocajul din `TranzactieComanda`; rezultatele seriale pe lot, partidă și perioadă probate pe două conexiuni, SC-X-15…SC-X-23 |
| F27-r9 | editabilitatea datei de înregistrare pe documentele GENERATE (o primesc la creare, din sursă sau din lună) (88) | după PoC (091) |
| F27-r10 | SAF-T: inițialul de stoc rămâne pe registrul integral (mutarea pe referință cere schimbarea semanticii lui `Randuri` din `SoldPeTipStocNeraportat` — decizie de raportare) (88) | închisă de 105 (SAF-B8: ruta pe registre scoasă; Opening S citește snapshot-ul cubului) |
| F27-r11 | dimensionarea conturilor de terț pe PARTENER: azi `Repartitor` urmează laturile documentului, deci `sold-parteneri` nu e creanța per partener (familia 64h/73-r12/86-r13) (88); rezolvată STRUCTURAL de 90c (partenerul se scrie din unitate) | închisă la TR-D9a (110): niciun cititor de sold nu mai grupează pe laturile documentului (inventar §12); partenerul stă pe postare, probat de cele 14 Check-uri `D9-A10` și de SC-PLT-08 |
| F27-r12 | integritatea snapshot-ului memorată în istoric (rânduri + sume la închidere, constatare + probă, referința fără rânduri refuzată); azi un rând șters direct din bază dă o balanță tăcut greșită (88, review 8b) | după PoC (110): snapshot-ul e al cubului; integritatea lui memorată în istoric rămâne de făcut |
| F27-r13 | perioadele fiscale ale unei baze noi: seed-ul scrie 12 luni ale unui AN HARDCODAT (2026), iar crearea se poate face doar din XAF — nu din React și nu prin OData (lipsă de ergonomie, nu funcțională; continuă 53i) (88) | după PoC (091) |
| F27-r14 | costul de CADRU al unei cereri (58 de instrucțiuni SQL de bootstrap de securitate per ObjectSpace + hidratare + serializare): motivul pentru care fișa de cont ratează ținta end-to-end deși calea ei de date costă 7 ms (88) | după PoC (091) |
| F27-r15 | ținta de perf a balanței analitice (< 100 ms) era calibrată pe ianuarie; pe decembrie, cu 71.167 de grupe `Cont × Repartitor`, nu e atingibilă în forma de azi — de re-calibrat sau de pre-agregat (88) | după PoC (091) |
| F27-r16 | forma proiecției `DocumenteCuRest`: candidații de la 59 (uniunea tuturor documentelor operate) și forma legăturilor se rezolvă ÎMPREUNĂ — corelarea legăturii duce panoul filtrat la 82 ms, dar calea neplafonată a constatării de rest scadent de la 220 ms la 1,02 s (respinsă motivat, cu cifre; niciun index nu lipsește) (88) | activă (110): proiecția moartă `TotalStingere` a ieșit și îmbinarea pe chei nulabile e corectată (D9-D10 a, pasul 5c); criteriul de formă propriu-zis e re-amânat explicit de owner (2026-10-06), cu cifra — 2,7 s de SQL pentru 488.733 de partide cu rest la 5 milioane de postări —, până la decizia 111 |
| F27-r17 | ordinea totală a listei `op1` din D394: cheia de ordonare nu e totală pentru persoanele fizice fără cod cu aceeași denumire, deci două generări ale aceleiași luni pot diferi la rând (familia 72-r7) (88) | după PoC (091) |
| F27-r18 | coloana cu data înregistrării în LISTELE React de documente (câmpul e cules și afișat pe formulare; coloana ar fi trecut pragul de atingeri al pasului 3) (88) | după PoC (091) |
| F28-r1 | simplificarea `CandidatiPereche<T, TOpus>` și a uniunii per tip din `ImperecheriProiectii` (jumătatea „tip” rezolvată de discriminator, „contrapartida per tip” e semantică); împreună cu F27-r16 (89); `ImperecheriProiectii` dispare sub 90e | activă (110): `CandidatiPereche<T, TOpus>` și uniunea per tip rămân; se judecă împreună cu F27-r16, la decizia 111 |
| F28-r2 | index unic pe `Repartitor.Cod` (81-r4): mecanismul trivial sub TPH, blocat de coliziunile legitime între familii din import (89) | după PoC (091) |
| F28-r3 | partiționarea sau `CLUSTER` pe `ClrType` — fără cifră care s-o ceară (89) | după PoC (091) |
| F28-r4 | dezproxarea tipului în patru copii (`ClasaReala`, `TipReal`, `VerificaCodDenumire` inline, `TipDomeniu`); de redus la cele două semantici la atingerea gardianului (89) | după PoC (091) |
| F28-r5 | D406 S la rece +0,2 s (+7 %) pe 12/2025: cost per proces (SQL-ul mai mic în TPH, chemările calde egale); cauza (JIT/compilarea EF a formei noi) neizolată prin profil (89) | după PoC (091) |
| TR-r2 | notele pe conturi de stoc fără lot (2.997 pe Flax, cinci corespondențe): conectorul decide per corespondență — postare de valoare pe lot sau divergență declarată (90); intră cu tipul NTC, la TR-D7b | migrare (091) |
| TR-r3 | fizica re-măsurată cu postarea de stoc unificată și felul `Transfer` exclus: `Spatiu` re-definit, FK per partiție, indexul `(Unitate, Data)` (90) | după PoC (091) |
| TR-r4 | recepția fără factură (NIR pe aviz, 408): cerință de produs de confirmat; NIR rămâne tip până atunci (90) | după PoC (091) |
| TR-r5 | DSC din FCL trece testul documentului-copil azi; de re-judecat dacă descărcarea devine clonă fără alegere de loturi (90) | după PoC (091) |
| TR-r6 | conectorul 1C: data reală a împerecherii (artefact 2026-09-18) și deschiderea de terți per partener / per factură deschisă (65,5 % din soldul de terț) (90); cubul scrie `Imperechere.Data` ca atare (S-D13), deci artefactul rămâne al CONECTORULUI și se raportează, nu se corectează în cub (TR-D7a) | închisă 2026-10-01 (107 c/g): data faptului și partidele inițiale per document, M1 |
| TR-r7 | sink-urile bugetare Gratuit/Custodie și injectivitatea SAF-T rămân deschise; Folosință păstrează gestiunea reală pe lanțul FCT/NIR/BTR/BCS/LDI (093, LDI-B3) | activă (091) |
| TR-r8 | rulajul brut per partidă nu e sumă sub tranzacția de transfer; fișa partidei se randează ca fereastră (90) | activă (091) |
| TR-r9 | probele de formă din ModelCheck (≈300): inventar rescrie/șterge la TR-D8/D9, nu înainte (90) | închisă la TR-D9a (110): oracolul și martorii au ieșit, aserțiile de regulă sunt re-țintite pe cititorii cubului (D9-D7; `tr-d9-inventar-modelcheck.md`, `tr-d9-pas3-retintire.md`) |
| TR-r10 | deschiderea generică fără document, cu loturi și partide; detalierea înlocuiește soldul bloc și refuză diferențele (094, DES-B1…B4). Conectorul 1C rămâne separat în 091-r4/T-r4 | activă: mecanism implementat (SC-DES-01…10); review advers 2026-09-24 cu MAJOR-1 și MEDIU-1…8, corecturi în curs (098d); cititorii și consumul stocului inițial rămân TR-D8 |
| TR-r11 | `Numar`, `DataScadenta`, `Autogenerat`, `DocumentSursa` rămân atribute ale documentului scrise la operare, sub gardianul (a) (90) | activă (091) |
| TR-r12 | Δ de sold 3xx (+585.404,66 pe Flax) între registrul de stoc și cel contabil de azi: tranșată prin contractul 1 al reconcilierii 1C; constatare, nu consecință acceptată (90); intră la TR-D7b, cu tipurile care o produc | migrare (091); cifra pe ianuarie 2025 e în `tr-d9-pas5b-probe.md` (registrul de stoc depășea registrul contabil cu 14.191,45 pe 371; cubul = registrul contabil + 5,03) |
| FZ-r1 | granul lui `Sold` contra snapshot-urile de azi (și dacă un read model mai grosier merită ca al doilea): gate la TR-D8, nu condiție prealabilă; FZ-r2 măsurată 2026-09-19 (fișa 348 contra 248 ms) și absorbită (90) | închisă 2026-10-04 (X-D5), cu cifra: balanța din snapshot atinge 1.926 de rânduri la orice istoric, cea recitită 7.827 după 12 luni, dar la acel volum recitirea durează 8,1 ms și snapshot-ul 12,9 ms; nu se adaugă un al doilea read model, rapoartele API rămân pe citirea vizibilă (104b) |
| FZ-r3 | lookup-ul per partidă (SAF-T Payments, fișa partidei): index `(Unitate, Data)` pe Contabil probat pe o interogare reală (90, TR-D8) | migrare (091-r4), planul închis la X-D5; TR-D8 S2 a fixat interogarea reală (`Citiri.Plati`: transferurile și originile pe `Unitate = ANY`, `Data ≤ capăt`), planul pe volum rămâne de măsurat la gate-ul transversal de perf; SAF-B8 (2026-09-30) a măsurat interogarea pe scara sintetică: 17,7 ms la 1.025 de partide, plus pragul de transport al tablourilor mari (`docs/api/p5-perf-masuratori.md`); X-D5 (2026-10-04): indexul `(Unitate, Data)` există pe ambele partiții și e ales fără constrângere la k = 64, m = 12 de plățile SAF-T L, partidele cu rest, dry-run-ul consumului, închidere și reconstrucție — planul e închis, pragul pe volum real rămâne al migrării |
| SAFT-r1 | explorarea unui document de compensare care își generează postările (declarant propriu) și devine sursa Payments 02/97; azi compensarea e NTC, în afara Payments (TR-D8 S2-R2, `docs/nucleu/tr-d8-saft-contract.md`) | după PoC (091) |
| SAFT-r2 | mișcările de stoc numai valorice (cod 60/90/100/101/130/140/180) și `Cauza?` în cheia `PoliticaMiscareSaft`: intră odată cu primul producător pe lot care le cere; azi linia cu cantitate 0 și valoare nenulă e refuz `SAFT_MISCARE_VALORICA` (TR-D8 S3-R1, `docs/nucleu/tr-d8-saft-contract.md`) | după PoC (091) |
| SAFT-r3 | valorile `CodAvertismentSaft`/`CauzaNeincludere` emise numai de ruta pe registre, scoasă la SAF-B8, rămân fără producător; ștergerea lor atinge metadata și codegen-ul clientului (TR-D8 B8-R2, `docs/nucleu/tr-d8-saft-contract.md`) | închisă 2026-10-01: scoase 5 cauze (`FaraContrapartida`, `TipFaraSectiuneFacturi`, `FaraCodMiscare`, `FaraContStoc`, `CodMiscareNecunoscut`) și 11 avertismente (`ContFaraRolPeFactura`, `LinieFaraContrapartida`, `TertLipsaPeMiscare`, `ProdusFaraContStoc`, `SoldNegativ`, `SoldPeTipStocNeraportat`, `MovementReferenceTrunchiat`, `NumarDocumentDuplicat`, `DataPostariiInAfaraPerioadei`, `RolTertMixt`, `ReziduValoricFaraCantitate`), cu textele lor din `Mesaj`; valorile numerice rămase neschimbate; `TipTvaFaraCodSaft` rămâne (îl emite diagnosticul TVA, 103); probele ModelCheck că vechiul cod nu revine trec pe literal; metadata regenerată, openapi neatins (codurile sunt string pe sârmă, 57a); integrala 3.269 / 4.477, `run-verificari/20261001-002414-346` |
| SAFT-r5 | după o inserare masivă (migrare, import, redeschiderea unei baze), statisticile Postgres rămân vechi până la autovacuum; citirea faptelor fiscale (`Cub/Citiri/Fiscale`) degenerează atunci în Nested Loop (60 ms în loc de 1–3 ms la 642 de tranzacții). Unealta de migrare/import rulează `ANALYZE` la final (TR-D8 B8-RV3, `docs/nucleu/tr-d8-saft-contract.md`) | închisă 2026-10-01: Import1C și Migrare rulează `ANALYZE` după ultima scriere (`Program.cs`, marcat `SAFT-r5`); rularea pe treaptă a scenei perf rămâne în `PerfSaft` |
| SAFT-r4 | balanța SAF-T (inițialul de cont L și S) recitește toate postările pe ObjectSpace-ul securizat; cu accesul complet verificat (SAF-D4) poate porni din snapshot-ul contabil, ca pozițiile S (TR-D8 B8-R3, `docs/api/p5-perf-masuratori.md`) | închisă 2026-10-04 (TR-D8 transversal, X-D5): balanța de cont și soldurile terților din L și S pornesc din snapshot (`CitireCumul.Integrala`); rândurile atinse pe `Postare` nu depind de istoric (9.878 / 9.752 / 9.752 la m = 0 / 6 / 12); `SaftAcces` cere și perioadele și snapshot-ul contabil |
| FZ-r4 | creșterea reală a coordonatelor pe un istoric lung: `Sold` la 1,2 GB și cifrele +S sunt limite inferioare (90) | după PoC (091) |
| FZ-r5 | rândul de stoc unificat cu postarea 3xx: economie ≤ 9 % — tranșat de 90g (o singură postare); re-măsurarea rămâne TR-r3 | închisă prin 90g |
| FZ-r6 | partiționarea pe an: redeschisă doar când citirile integrale de istoric trec pragul pe baza reală; rămâne PhysicalStock (90) | după PoC (091) |
| FZ-r7 | reperul `TaxInformation` din GLE (`LinieId` contra `PerioadaDeclarare`): întrebare de design SAF-T (90) | după PoC (091) |
| FZ-r8 | costul FK la scară, încărcarea în bloc, `VACUUM`/bloat sub scrieri reale, interogări concurente cu operarea serializată (90) | după PoC (091) |
| FZ-r9 | BRIN pe `Data` reevaluat pe forma cu `Sold` (90) | după PoC (091) |
| FZ-r10 | împerecherea ca tranzacție pe date cu împerecheri datate real (90, TR-r6) | închisă pe date 2026-10-01 (107 g); mecanismul e cel de azi |
| IM-r1 | nivelul specific per tip ca serviciu — absorbită de declarantul per frunză (90, TR-D6b) | închisă la TR-D9 (confirmată de 110) |
| IM-r2 | adaptorul EF direct și hostul fără XAF: nucleul e pur, singurul consumator e `Module`; decizie proprie după TR-D9 (90) | după PoC (091) |
| IM-r3 | `IDocument`/`ILinie` peste bază — absorbită: operandul închis e DTO (90) | închisă la TR-D9 (confirmată de 110) |
| IM-r4 | identificatorul semantic al tipului + fabrici — depășită de 89 (`CititorTipDocument.Clasa`) și de regimul dual ca dată (90) | închisă |
| IM-r5 | async efectiv în host-uri: felie proprie cu cifră, după TR-D9 (90) | după PoC (091) |
| N-r1 | invariantul 7 al designului (motorul reproduce baseline-ul Import1C) nu e testabil în nucleul pur: proba supremă a lui TR-D7/D10, nu test de formă (TR-D6a) | depășită de 091 |
| N-r2 | capătul virtual al cantității pe postarea de terț cu gestiune virtuală Furnizor/Client, C2 peste toate postările (N-D4, lărgește litera 090g): CONFIRMAT de pilotul FCT (TR-D6b) — invizibil proiecțiilor pe gestiune reală și pe unitate-lot; `GestiuniVirtuale` sunt constante ale nucleului și C5 nu cere unitate pe gestiune virtuală (profilul fără `RolTert` n-are partidă pe 401) (TR-D6a) | închisă |
| N-r3 | evaluarea ieșirii pe raportul curent al unității (N-D7) diferă de prețul înghețat al lotului de azi pe loturile cu corecție: MĂSURAT la TR-D6b (`NUC-BCS-N-R3-*`: lot 20 buc / 300 lei, preț înghețat 10, consum 5 ⇒ 50 azi, 75 în nucleu, Δ = +25); documentele de azi nu pot da două prețuri pe același lot, Δ apare doar din deschideri/import; intră în diferențele declarate ale reconcilierii TR-D7 (TR-D6a) | închisă |
| N-r4 | TVA decisă pe document × cotă și postată per linie prin Hamilton pe fiecare semn (090j) contra rotunjirii per linie de azi: MĂSURAT la TR-D6b (`NUC-FCT-N-R4-*`: 3 × 0,01 la 21 % ⇒ 0,00 azi, 0,01 în nucleu, Δ = 0,01 pe o singură linie); taxa culeasă e autoritară cu toleranța de pilot `0,01 × liniile cu TVA` (B-r1 o face rând de politică) (TR-D6a) | închisă; de la 109 linia și registrele poartă aceeași taxă ca nucleul (Δ = 0) |
| N-r5 | stornoul unei postări ATRIBUITE (reevaluare) intră în selecție (090i) și poartă cauza altui document, dar contrapartida ei inversată e nedefinită ⇒ tranzacția de storno nu conservă valoarea; se tranșează la TR-D9 odată cu reevaluarea (TR-D6a) | activă (091); TR-D9b (110) |
| N-r6 | amendamente de literă ale contractului TR-D6a consemnate în cod: cazurile `Decizie` poartă `Linie`; `Motor.Transfera` ia `Mutare`, nu `Declaratie` (fără decizii/ipoteze pe transfer); ierarhia închisă ține prin constructor `private protected`, copy-constructorul rămâne `protected` (CS8878); `Unitate with { Fel }` și `Declaratie with { Document, Miscari }` nu se re-validează inter-câmp — se construiesc din nou (TR-D6a) | activă (091) |
| N-r7 | ordinea FIFO a nucleului e `Guid.CompareTo` (componentă cu componentă) prin `Fifo.Intai`; `ORDER BY uuid` în Postgres are altă ordine ⇒ la TR-D7 candidații se sortează în nucleu, nu în SQL, altfel „ultima ia restul” cade pe alt lot la date egale (TR-D6a) | activă (091) |
| N-r8 | stornoul unei tranzacții de fel `Transfer` e de fel `Storno` și intră în citirile cu `includeTransfer = false`; pe proiecțiile pe cont contribuie zero (±v pe aceeași latură), dar apare ca rând în listările de jurnal — regula listării rămâne a lui TR-D8, acum că `Transfer` se persistă (TR-D6a, TR-D7a) | închisă 2026-10-03 (X-D7 d, 108b): regula listării în `stare-curenta/dezvoltare-si-validare.md`, SC-CIT-107; intrările comune probate de X-D2 (proba `N-r8`, SC-CIT-100…102); consumatorul SAF-T închis local (105) |
| N-r9 | `Repartizare.Hamilton` poate depăși `decimal` la ponderi ~1e20 × total ~1e8; inaccesibil cu baze ≤ 1e10; dacă un apelant trimite preț × cantitate brute ca ponderi, se normalizează întâi (TR-D6a) | după PoC (091) |
| B-r1 | toleranța taxei culese, constantă de pilot în `Fapte.Operand`, refuza facturi pe care motorul vechi le operează (TR-D6b): devine `PoliticaTva.TolerantaTaxa`, MĂSURATĂ pe Flax la TR-D7a (275 de documente refuzate la 0,01 pe linia cotei, 17 la 0,10, maximul 55,87 — abateri reale ale datelor culese, nu rotunjire) și închisă ca politică OPȚIONALĂ: `null` = taxa culeasă autoritară, fără gard, ca azi; valoarea de produs rămâne S-r1 (TR-D7a) | închisă prin S-D15 |
| B-r2 | sensul laturilor trezoreriei e tip-dependent și declarantul unic nu-l cunoaște (TR-D6b): devenit dată pe `TipDocument` (`LaturaContPropriu` = `Predator` pe plată, `Primitor` pe încasare, seed pe ambele profiluri), cu refuzul `LATURA_CONT_PROPRIU_NEPOTRIVITA`; `CONT_PROPRIU_LIPSA` rămâne pentru lipsă (TR-D7a) | închisă prin S-D7 |
| B-r3 | recepția facturii (TR-D3) n-are regulă de contare proprie pe FCT: contrapartida se ia de pe regula `FCT/Serviciu`/`Cheltuiala` sau din politica de TVA; la TR-D7, când NIR-ul conex dispare, regula recepției devine rând de politică pe FCT (`FCT/Stoc`) (TR-D6b) | închisă 2026-10-08 (112): regulă proprie `FCT/Stoc` pe ambele profiluri, fără împrumut din politica de TVA sau din regula altei naturi; fără regulă, recepția e refuzată |
| B-r4 | Autocolectare distinctă la achiziția TI, cu Sens=Achiziție; D300 citește cele două obligații fără oglindire dublă, D394 o achiziție (103e) | închisă 2026-09-26: SC-CIT-81, matricea tipurilor și integralele verzi |
| B-r5 | linia FCT care numește un lot recepționat pe aviz (`NIR` manual) ar posta `408 = 401` (TR-D3): CĂUTAT în motor la TR-D7a — fluxul NU există (NIR-ul manual postează `3xx = 401`, iar linia de stoc a facturii își naște singură lotul la culegere); `408` apare doar ca reclasificare de DATE la import (`HandlerFactura`), pe 17 facturi ale bazei Flax. Rămâne deschisă cu constatarea: nu se implementează fără oracol (TR-D7a) | după PoC (091) |
| B-r6 | `Valuta`/`Curs` sunt câmpuri de frunză pe `FacturaIntrare` fără interfață declarată: operandul le lasă `null`; `IDocumentCuValuta` la primul consumator real (partida în valută, TR-D9) (TR-D6b) | activă (091); TR-D9b (110) |
| B-r7 | o linie cu DOUĂ conturi cu `RolTert` era nedefinită în declaranți (TR-D6b): 0 cazuri pe BCS/PLT/INC, 25 de facturi pe Flax (`408`, `4091`, `4092` contra `401`). Închisă ca REGULĂ, nu ca refuz: linia numește partidă pe AMBELE capete, fiecare pe contul lui (TR-D7a) | închisă prin S-D16 |
| B-r8 | gestiunile virtuale (`GestiuniVirtuale.Furnizor/Client/Consum`) sunt constante deterministe fără rând de nomenclator: devin rânduri `DinSeed` cu aceleași id-uri dacă un raport le cere nume (TR-D7/D8) (TR-D6b) | activă (091) |
| B-r9 | `Document.Declarant() == null` era regimul dual al pilotului (TR-D6b): gardul e acum dată (`TipDocument.PosteazaInCub`), iar un tip marcat a cărui clasă nu declară e eroare de configurare la operare, nu regim tăcut (TR-D7a) | închisă prin S-D3 |
| B-r10 | linia fără regulă de contare: motorul vechi o SARE tăcut, declaranții o REFUZĂ (TR-D6b). Gard CONFIRMAT la TR-D7a: 0 cazuri pe cele 53.449 de documente ale pilotului pe Flax; singura apariție era o linie de natură `Tehnica` pe o factură a unei scene ModelCheck, ale cărei 100 de lei dispăreau din contare — scena corectată, refuzul rămâne regula (TR-D7a) | închisă |
| B-r11 | `DocumentDetaliu` n-avea coloană de poziție, iar N-D7 și splitul PLT depind de ordinea liniilor (TR-D6b): `Pozitie` se atribuie o dată, în `SaveChanges`-ul contextului, și e citită de AMBELE motoare (`OrderBy(Pozitie).ThenBy(ID)` în cele cinci enumerări ale motorului vechi și în operand); ordinea la cereri concurente rămâne S-r9 (TR-D7a) | închisă prin S-D6 |
| S-r1 | valoarea de produs a lui `PoliticaTva.TolerantaTaxa`: seed-ul privat e `null` (fără gard), dar pe Flax 275 de facturi se abat peste 0,01 pe linia cotei, 17 peste 0,10 și 15 perechi document × cotă peste 0,50, cu maximul 55,87 (`ROYAL250801852`) — owner-ul decide valoarea și dacă abaterile mari se raportează conectorului (TR-D7a) | migrare (091) |
| S-r2 | deciziile și ipotezele contractului (`AlocareFifo`, `ValoareIesire`, `SoldUnitateCitit`) nu se persistă; un cititor de audit („de ce a costat atât”) le cere la TR-D8 (TR-D7a) | închisă 2026-10-03 (TR-D8 transversal, X-D4): explicația contractului e persistată pe prima lui tranzacție (`Tranzactie.Explicatie`), citită prin `Cub.Citiri.Explicatii` și `GET api/proiectii/explicatii/{tranzactieId}`; invariantul de audit rulează în `INV-CUB`; SC-CIT-96…99 |
| S-r3 | gestiunile virtuale n-au FK pe `Postare` (id-uri fără rând de nomenclator); B-r8 le face rânduri `DinSeed` și atunci FK-ul pe `Gestiune` intră pe ambele partiții (TR-D7a) | activă (091) |
| S-r4 | snapshot-ul EF declară cheia `ID` și `FK_Postare_Tranzactie_TranzactieId` pe părinte, baza are cheia `(Spatiu, ID)` și FK-uri per partiție: divergență DECLARATĂ cât timp XAF EF Core nu suportă chei compuse, probată de `STR-SCHEMA` — orice migrație viitoare pe `Postare`/`Tranzactie` se scrie în SQL (TR-D7a) | activă (091) |
| S-r5 | pe Flax 709 împerecheri de plată se plafonează la restul partidei stinsului și una (`SED00001497-4`, 0,05) se sare fiindcă stinsul n-are rest pe contul de referință: fapt de DATE al conectorului, de raportat în reconcilierea 1C (TR-D7a) | migrare (091) |
| S-r6 | `PosteazaInCub` e aliniată de seed la fiecare updater, ca orice rând `DinSeed`: o bază pe care flag-ul a fost oprit manual are documente operate fără tranzacție `Operare` în cub până la TR-D9, iar litera (e) a reconcilierii le raportează — coloana devine read-only în UI sau iese din aliniere, de decis până la TR-D9 (TR-D7a) | închisă la TR-D9a (110): coloana `PosteazaInCub` și alinierea ei au ieșit (D9-D5) |
| S-r7 | un lot născut de o linie „în roșu” (valoare negativă, cantitate pozitivă) se evaluează negativ tăcut, ca în motorul vechi: invariantul „un lot nu se evaluează negativ” e pierdut odată cu admiterea semnului în `Operare` (TR-D7a) | activă (091) |
| S-r8 | dry-run-ul nu fixează prețul și data lotului născut de document, deci contractul lui diferă de cel real pe `Unitate.Deschisa`; fără refuz fals azi (TR-D7a) | activă (091) |
| S-r9 | `Pozitie` la cereri concurente poate da dubluri (două ObjectSpace-uri calculează `max + 1` din aceeași bază); ordinea rămâne deterministă prin `ThenBy(ID)`, dar proba promite `1..n` (TR-D7a) | închisă 2026-10-03 (TR-D8 transversal, X-D6): maximul se citește sub blocajul scrierii, în tranzacția salvării; SC-X-20 |
| S-r10 | purja scenei `VerificaSaftStocuri` șterge `TipMaterial`-ul de scenă fără `ReguliContare`-le pe care seeder-ul i le atașează: o cădere în mijlocul scenei lasă un reziduu care blochează rulările următoare (TR-D7a) | închisă 2026-09-23: purja șterge întâi regulile `DinSeed` pe tipurile marcate ale scenei; recuperare probată pe reziduul real al unei rulări întrerupte |
| S-r11 | dry-run-ul (`Valideaza`) prinde din declarație doar `OperareException`; o excepție de alt fel (`InvalidOperationException` din `Contractare`, `ArgumentException` din `N.Unitate`) iese 500, nu 422 — de tranșat la primul caz real sau la TR-D8, când dry-run-ul capătă cititor (TR-D7a) | închisă 2026-10-03 (TR-D8 transversal, X-D4 f): `Contractare` întoarce refuzul `DECLARATIE_INVALIDA` pentru `ArgumentException` din nucleu și pentru declarantul fără declarație și fără refuz; proba `S-r11` în scena SC-CIT-96 |
| T-r1 | 090 (a) „EXACT o tranzacție `Operare`" devine „cel mult una `Operare` și cel mult una `Transfer`, cel puțin una" pentru tipurile cu linii care nu schimbă contul (BTR, ASM); litera (e) amendată; textul deciziei 090 nu se rescrie, amendamentul e în contractul TR-D7b (T-D2) | închisă 2026-09-23: după pasul 1, SC-ASM-04/05/06/08 probează Operare, forma mixtă și inversarea completă pe ambele profiluri |
| T-r2 | reziduul valoric lăsat de RLF pe lotul golit (valoare fiscală ≠ raportul lotului) contrazice 090 (j); se rezolvă la TR-D9 prin re-evaluarea unității cu reziduul spre 658/758 din politică (TR-D7b) | activă (091); TR-D9b (110) |
| T-r3 | postările NTC pe conturi cu `RolTert` care rămân FĂRĂ partener după (B) și pe 3xx fără lot (TR-r2): declarate până la TR-D9; litera (f) le exclude nominal (TR-D7b) | limită declarată (110 j): regula rămâne — fără partener nu se inventează partidă; excluderea nominală din reconciliere a dispărut odată cu ea |
| T-r4 | `Deschidere.cs:43-50` și decizia 047 afirmă că 1C nu defalcă soldul de terț pe partener la 01.01 — FALS: defalcarea e în `BalantaNivel3`; se corectează la pasul 6, TR-r6 (deschiderea) și TR-r10 se închid (TR-D7b) | închisă 2026-10-01 (107 c): partidele inițiale din `BalantaNivel3` prin `Materializare.Deschide`, M1 |
| T-r5 | DVI deschide partidă pe contrapartida cu RolTert (401 probat; 446 numai dacă este configurat astfel), deși `PoateFiStins = false`; hook-ul e al registrelor până la TR-D9 (TR-D7b, DVI-B3) | activă, TR-D9b (110): hook-urile de stingere nu citesc registre și au rămas neatinse (D9-Q4 = A) |
| T-r6 | `Custodie` pe bugetar: explicit neacoperită la LDI; de definit contul politicii dintre cele zece 803x și perechea cantitativă/valorică (093d, LDI-B3) | activă (091) |
| T-r7 | diferența declarată T-D2.2 (BTR: `round(q × PretUnitar)` în registre contra raportului curent în cub, 535 documente / 28,60 lei absolut pe Flax) și T-D4.2 (DSC: 842 documente / Σ +31,01 lei, |Δ| max 16,50) e invizibilă reconcilierii (a)–(g) și devine vizibilă la citirile pe cub per gestiune × lot; TR-D8 o raportează, nu o absoarbe (TR-D7b) | închisă la TR-D9a (110): nu mai există registru de comparat; valoarea liniei e decizia contractului (D9-D3) |
| T-r8 | `RegimTva.Capitalizat` pe o linie de FCL nu se desface în bază + taxă (FCT o face prin `Netele`); pe Flax nu există, pe bugetar nu e politică de TVA — de pin-uit la TR-D9 dacă apare (TR-D7b, pasul 2) | după PoC (091) |
| T-r9 | `TipDocument.LaturaContPropriu` (B-r2, dată de seed) e redundantă cu contractul structural `Plata`/`Incasare.Laturi()` (T-D13): nimeni n-o mai citește; coloana și rândul de seed se scot la prima migrație care atinge `TipDocument`, nu acum (T-D13 (d): nicio migrație în pasul 2b) (TR-D7b, pasul 2b) | închisă la TR-D9a (110): cititorul SAF-T e portat pe `Document.Laturi()` (pasul 4), coloana `LaturaContPropriu` a ieșit (pasul 7) |
| T-r10 | DVI fără net: baza vamală distinctă în Carte=Fiscal, conform DVI-B1…B7 (`docs/nucleu/tr-d7b-dvi-baza-fiscala-contract.md`); declarant, normalizare, 20 de scenarii și SC-X-14 implementate | închisă 2026-09-23; gate integral 1.829/2.897 OK, nucleu 165/165; manifestele în `scenarii/DVI.md` |
| T-r11 | Cititorii pe cont la TR-D8: balanța, fișa și rulajele folosesc o singură intrare cu `Carte=Contabil`; probe pe rulaje și solduri filtrate pe partener cu DVI fiscală prezentă. Filtrul din ReconciliereCub și SC-DVI-16 sunt condiții ale DVI, nu închid portarea cititorilor de producție. Contract: DVI-B4 | închisă 2026-10-04 (108, X-D7 g): intrarea unică e `Citiri.Contabil`, iar proba X-D2 refuză orice cititor de producție pe registre în afara listei nominale; probele cu DVI fiscală prezentă sunt SC-DVI și SC-CIT |
| T-r12 | ASM: transformarea n→m, varianta A cu Δ local, storno complet și scenarii. Contract aprobat ASM-B2…B7 în `docs/nucleu/tr-d7b-asm-transformare-contract.md`; catalog `ASM.md` | închisă 2026-09-23: review aplicat, MEDIU-2/MINOR-3/MINOR-4 corectate, MEDIU-1 amânat explicit de owner în T-r15; integral 2.003/3.097 OK, nucleu 178/178 |
| T-r13 | Reziduul valoric transversal dual pe lot × gestiune × cont: diagnostic cu proveniență și probe BCS/ASM 1+1+1. Închiderea cere evaluări noi pe soldul complet al cubului ȘI tratarea explicită a soldurilor istorice divergente; schimbarea cititorului nu șterge istoricul. Contract ASM-B7 | închisă la TR-D9a (110): un singur sold, al cubului; bazele sunt recreate, fără istoric divergent (D9-D3, 102) |
| T-r14 | Cititorii TR-D8 (jurnal, fișă, GL, MovementOfGoods) exclud structural contraponderile Transformare, cu probe; diagnosticul Comparabil le exclude cu numărul raportat. Contract ASM-B2/B7 | închisă 2026-10-03 (X-D7 c): `Imobilizari.PozitiiFaraFisa` exclude contraponderile ca celelalte intrări; SC-CIT-106 pe cele șapte intrări comune, SC-CIT-15, SC-SAFT-10, SC-ASM-16; consumatorul SAF-T închis local (105) |
| T-r15 | Delimitarea diferențelor contabile ASM: Operare exclusă nominal din (a), număr și diferențe raportate în (h), probe independente obligatorii. Contract D8-B4 / T-D10 / ASM-B7 | închisă 2026-09-24: aprobată de owner, implementată și probată pe ambele profiluri; integral 2.616/3.707 OK, manifest 20260924-101646-615; T-r13 rămâne activă |
| T-r16 | BTR cu schimbare de cont: declarantul și culegerea nu au cont destinație; excepția ASM nu se extinde la BTR fără decizie și scenarii proprii (TR-D7b) | după PoC (091) |
| IM-r7 | extensia per client a modelului (`DbContext` de extensie / migrații per client): spike separat înaintea oricărei decizii de produs (90) | după PoC (091) |
| 091-r1 | filtrul `--scenarii <TIP>` în ModelCheck: un tip într-un minut pe o bază de profil; azi scenele rulează doar în suita integrală (091) | închisă 2026-09-22 (`ScenelePeTip`; toate tipurile de pe cub în ~15 s pe privat) |
| 091-r2 | recensământul pe clona Flax per tip rămas (NTC, ITV, RDC, RLF, ASM, LDI, NIR), o dată, înaintea scenariilor lui; cifrele în coloana „Proveniență” a fișierului tipului (091) | activă (091) |
| 091-r3 | testul de arhitectură care refuză accesul la `Postare` în afara cititorilor comuni (`Module/Cub/Citiri`); `Transfer` și stornourile lui excluse acolo, nu per raport (091, TR-D8) | închisă 2026-09-28: proba ModelCheck `091-r3` scanează sursa de producție; accesele din `Motor/` și WebApi sunt mutate pe `Cub/Citiri`, excluderea `Transfer` stă în `Citiri.Contabil.Postari` |
| 091-r4 | felia de migrare, cu decizie proprie, după „rotund”: conectorul 1C repornit pe modelul final, deschiderea de terți din `BalantaNivel3` (T-r4), stingerile 2024, reconcilierea 1C ca raport de diferențe (091) | migrare (091); prima felie M1 făcută 2026-10-01 (107): deschiderea și stingerile 2024; rămân 107-r1…r8 și rularea integrală 12/12 cu contractele 1–5 verzi (M1-D10 amendat 2026-10-04), după TR-D9 |
| 107-r1 | stingerile prin facturi (avansul pe 419/409 consumat de factură, factura sosită pe 408, creditul de client consumat de factura următoare) nu intră în trecerea 2; partidele rămân „neatinse", declarate în contractul 5 (107) | migrare (091) |
| 107-r2 | rândurile de terț fără partener din anul importat rămân în punte; maparea lor pe partenerul generic ar închide partidele generice (461, 411.8) (107) | migrare (091) |
| 107-r3 | drift-ul contractului 1 față de baseline-ul 2026-09-21 (TVA ±0,03 pe linie pe sute de facturi, evaluarea RLF, consecința în ITV): scenarii + decizie pe motor (091 r5), nu normalizare în conector (107) | închisă prin 109 (2026-10-04): cauza era conectorul fără marcajul `TvaCules` (103i), nu motorul; după marcare contractele 1 și 2 sunt verzi la ban pe ianuarie. Diferența de fond găsită pe drum (taxa liniei ≠ taxa postată pe documentele nemarcate) e închisă tot de 109 |
| 107-r4 | valuta: partidele inițiale în valută cu curs propriu (azi în lei, plafonate la rest la stingere) (107) | activă, TR-D9b (110) |
| 107-r5 | partenerul generic de migrare la go-live: procedura de rezolvare prin NTC de reclasificare și excluderea din D394/SAF-T ca decizie de produs (107) | migrare (091) |
| 107-r6 | performanța Import1C: 25 → 50 min/lună pe Flax față de 8–10 la baseline 2026-09-21 (~5×); după gate-ul TR-D8 ianuarie durează 21:27 (2026-10-04, ~2,5× baseline), creșterea pe luni neremăsurată; scara X-D5 nu acoperă calea importului, de profilat înaintea oricărei rulări integrale (107) | activă (091) |
| 107-r7 | ordinea stingerilor din aceeași compensare e nedeterministă între rulări (ianuarie: 42 apoi 41 partide neexplicate); de ordonat cronologic în trecerea 2 (107) | migrare (091) |
| 107-r8 | nota-punte a vânzării cu valoare pe 3 zecimale (iunie 2025, 122,408) refuzată de gardianul de scară; rotunjirea la bani în handler, divergența declarată (107) | migrare (091) |
| 107-r9 | documentele care mișcă direct partida inițială la operare (Compensare, Operatia cu partida nominalizată) nu trec prin trecerea 2; diferența față de sursă rămâne FAIL etichetat în contractul 5 (107) | migrare (091) |
| 107-r10 | încasările inline ale retailului (`RaportDeVanzariCuAmanunt`, copiii `#inc`) nu sunt enumerate de trecerea 2; stingerea lor pe o poziție de deschidere nu ajunge pe partida inițială (107) | migrare (091) |
| 091-r5 | un caz apărut la migrare care contrazice catalogul devine scenariu nou plus decizie; oracolul normalizat (`CubDinRegistre`, `Normalizari`) nu se mai extinde (091) | migrare (091) |
| 109-r1 | contractele 1 și 2 ale reconcilierii 1C citesc `RegistruContabil` (`ReconciliereLuna`); trec pe cititorii cubului (109) | închisă la TR-D9a (110): Import1C citește cubul (pasul 5); rularea-diagnostic pe ianuarie e în `tr-d9-pas8-inchiderea.md` |
| 109-r2 | în ecranul XAF, recalculul unei linii mută taxa și pe celelalte linii nemarcate ale documentului; reafișarea lor de probat în browser (109) | închisă 2026-10-04: probată în browser; proba a găsit conflictul de versiune la operarea după salvarea liniei, corectat prin 109 (f) |
| 109-r3 | 46 de împerecheri pe documente refuzate „peste restul nestins al documentului” în ianuarie pe Flax, față de 6 la baseline; clasa de bani mărunți a dispărut, 35 netriate (109) | migrare (091) |
| 109-r4 | tabul liniei nedeschis din grila documentului nu reîmprospătează tabul documentului la salvare; documentul arată valorile vechi până la Refresh (109) | după PoC |
| 095-r1 | DEC → contract IMO → PIF/AMO/CAS complete pe cub, inclusiv cartea fiscală și istoricul fișei, înaintea TR-D8 (095), PIF cu suport obligatoriu aprobat (097); urmărirea partidelor separată de RolTert (096) | activă: producători și cititor de fișă implementați, SC-IMO-01…25 verzi pe ambele profiluri; review advers făcut 2026-09-24, corecturi în curs (098b/c/d) |
| 096-r1 | Diagnostic și tratare explicită la TR-D8 pentru postările istorice fără unitate pe conturi cu UrmarestePartide; migrația atributului nu reconstruiește istoria (096d) | depășită de 102 (2026-09-25): bazele de dezvoltare se recreează, istoria nu se diagnostichează |
| 097-r1 | diagnosticul istoriei PIF fără fișă/origine/suport înaintea activării cititorilor (097) | închisă 2026-10-03 (X-D7 e): `Imobilizari.VerificaAcoperire` în `INV-CUB`, cu mutanții `IMO-FISA`, `IMO-CAUZA`, `IMO-ORIGINE`, `IMO-SUPORT`, `IMO-REGISTRU` |
| 097-r2 | auditul anulării fizice a suportului eliberat; proveniența fără FK restrictiv (097, 091j) | activă; se tranșează în TR-D9b (110) |
| 097-r3 | blocajul tranzacțional comun pe bază, limitat la IMO și suportul lui; mecanismul general rămâne 091 (g)(4) (097) | după PoC (110): blocajul IMO rămâne; se rejudecă odată cu X-r1 |
| 098-r1 | conturile de contrapartidă ale deltei NIR conex per profil, aprobate de owner înaintea codului (098a); privat fixat prin 099(e), bugetar în tr-d8-nir-delta-contract.md §NIR-D4 | închisă 2026-09-24: owner-ul aprobă maparea bugetară, inclusiv 35x PeDrum și 428.01.02 pentru personal; implementată în felia NIR delta, în așteptarea review-ului |
| 098-r2 | recensământul pe clona Flax al conexelor NIR editate față de sursă (098a) | închisă 2026-09-24: 17.814 conexe FCT, zero diferențe de laturi sau multiset lot/tip/cantitate/valoare pe clona Flax.Api; SQL recensamant-nir-delta.sql, rezultatul în run-nucleu/tr-d8/nir-delta/recensamant.json; nu certifică fluxul nou |
| 098-r3 | garda registrelor după storno/anulare NIR acoperit cu consum: registrul negativ poate refuza alte operații pe lot; limită acceptată de owner, fără compatibilizare (NIR-D5) | închisă la TR-D9a (110): garda registrului nu mai există; SC-X-27 arată operația pe lot acceptată după stornarea NIR-ului acoperit cu consum (jumătatea „refuzată azi” nu exista: `tr-d9-pas2-probe.md`) |
| 099-r1 | LDI adoptă cauza diferenței: minus imputabil pe partener, perisabilitate, neimputabilă; plus (099) | activă (091) |
| 099-r2 | efectele fiscale per cauză a diferenței: ajustarea TVA, deductibilitatea (099) | după PoC (091) |
| 099-r3 | BTR cu lipsă la primire, pe clasificarea cauzei diferenței (099) | după PoC (091) |

| 100-r1 | Acoperirea bugetară a partidelor pe cele trei conturi DinSeed, ciclurile FCT/PLT, FCL/INC, NTC și deschidere; refuz istoric conform 096-r1 | închisă 2026-09-25: SC-CIT-41…48 și cataloagele tipurilor, integral 3.036/4.026 OK; raportul/snapshot-ul rămân la TR-D8 |

| 101-r1 | Efect obligatoriu al împerecherii, refuz atomic, candidați/disponibil pe cub și diagnostic al legăturilor istorice fără efect (101) | închisă 2026-09-25 în C102: D-2 și D-3 corectate (SC-CIT-66/67), diagnosticul istoric depășit de 102; anterior activă, redeschisă 2026-09-25 de owner după review-ul advers (`comunicari/2026-09-25-1237`): NTC consumă retroactiv partida stinsă la o dată ulterioară (D-2); ținta deschisă prin Transfer e oferită în panou, dar refuzată de comandă (D-3). Închiderea anterioară: SC-CIT-49…65, integrale 3.107/4.103 OK |
| 102-r1 | Scoaterea codului de compatibilitate (102c), invarianții devin probe ModelCheck (102d), comprimarea migrațiilor tranșată (102e); felia de curățenie înaintea snapshot-ului de stoc | închisă 2026-09-25 (C102, contractul §Închiderea) |
| 102-r2 | Alinierea deciziilor, stare-curenta și restanțelor cu 102(c) | închisă 2026-09-25 (C102) |
| 102-r3 | Rețeta de recreare a bazelor de dezvoltare și recrearea lor (102b) | închisă 2026-09-25 (C102; rețeta în `stare-curenta/dezvoltare-si-validare.md`) |
| 102-r4 | Desfacerea automată fără efect în cub: refuz sau invariant `INV-CUB` (102) | închisă 2026-10-03 (X-D7 f, X-Q3 = A): refuz `IMPERECHERE_FARA_EFECT` pe ramura negativă, SC-CIT-108 |
| 102-r5 | Perf `Fapte.PartideDisponibile` pe volum: agregarea temporală în SQL (102) | după PoC; măsurat în X-D5 (2026-10-04): dry-run-ul notei stingătoare citește toate partidele deschise ale partenerului, 1.770 / 4.390 / 7.006 rânduri la m = 0 / 6 / 12, 24–26 ms |

| 103-r1 | Felia fiscală D8-B8: fapte istorice, perioade distincte, corecții/depunere, cititori comuni și probe SC-CIT-79…87 (103) | închisă 2026-09-26: 3.182/4.214 OK, 180/180 Nucleu, HTTP/browser și A/B; restul SAF-T și gate-ul transversal de performanță rămân TR-D8 |
| 103-r2 | Intervale TVA și avertismente/raport de impact, cu referință la linia avansului (103h); contract `tr-d8-tva-intervale-contract.md`, SC-CIT-90…94; delimitare rămasă: Proveniența fiscală a ajustărilor RDC/RLF/reduceri (R6-B4) | activă numai pentru proveniența fiscală a ajustărilor RDC/RLF/reduceri; R6 implementat și verificat 2026-09-28, review advers închis; consumul/restul avansului rămâne în afara R6 |
| 103-r3 | Validarea L3 a semnului taxei culese față de baza FCT/FCL și înlocuirea proxy-ului `ILinieCuAvans` pentru linia de factură; detalii în 103 | activă, la următoarea atingere a validării L3; nu blochează închiderea R6 |

| 104-r1 | Felia C104: entități proprii și ștergere fizică, coaja comenzii fără OS dat de apelant, culegerea unică L3, aria React, review advers (104, pașii 1–5) | închisă 2026-09-27: pașii 1–5 pe `c104-straturi`, review Codex R1–R3 plus R4/R5 corectate, integral verde pe ambele profiluri |
| 104-r2 | Coaja comenzii fără `IObjectSpace` și hook-urile cu `os` de pe entitate mutate în declarant/coajă (104b) | după PoC (110, D9-Q4 = A) |
| 104-r3 | Filtrarea pe `Activ` în lookup-urile documentelor noi (104, L3/L4) | după PoC |
| 104-r4 | Paginile React de culegere: scoase sau generate din metadate (104d) | după PoC |
| 104-r5 | Fixture propriu al matricei `refuzuri.ps1` (subiecții creați și șterși de probă), înaintea pasului 5 din C104 | închisă 2026-09-27: 294/294 PASS de două ori pe baza Privat din seed |
| 106-r1 | Motivul indisponibilității unei comenzi în XAF Blazor: tooltip-ul pe acțiunea dezactivată se probează în browser; altfel alt purtător (106d) | închisă 2026-10-04: probat în browser pe Draft și pe Operat; ștergerea standard XAF legată de `Sterge` |
| 106-r2 | Acțiuni XAF pentru comenzile proprii ale tipurilor (Regenerează, Distribuie, Stinge), azi doar în React (106c) | după PoC |
| 106-r3 | Planul de conturi și D300 pe `DxTreeListEditor` în mod plat (Key/ParentKey), cu opt-out de la `Server` (106, 85a) | după PoC |
| 106-r4 | Axa 2 a formatului: coloanele grilelor de linii declarate pe roluri, blocajele câmpurilor-rezultat puse o singură dată; primitiva `Columns`/`ReadOnly` în Atlas.DXF 26.1.4.10 (106h) | închisă 2026-10-01 |
| 106-r5 | StateMachine și ViewVariants scoase din `Startup.cs` și `RequiredModuleTypes`; `HCategory` scos din `DbContext` (106g) — partea `HCategory` e făcută la TR-D9a pasul 7 (2026-10-07) | activă |
| 106-r6 | BCS/BTR/RLF calculează `Valoare` dar stau pe grila generică și pe dialogul comun cu DVI; blocajul rezultatului cere detaliu propriu (106h) | după PoC |
| 106-r7 | Regimul stornării nu numără nominalizarea partenerului unei legături vii când ea nu vine din transferul legăturii (și legătura manuală fără transfer în cub); refuzul `PARTIDA_CU_DEPENDENTI` rămas după desfacere apare la comandă (106k, SC-REGIM-14) | după PoC; owner 2026-10-04: rămâne la comandă |
| X-r1 | Rafinarea blocajului scrierii: granularitatea pe gestiune + partener și scoaterea salvării de draft de sub blocajul comenzilor; pragul de așteptare separat de timpul comenzii (108) | după PoC; se deschide la așteptare măsurată peste un prag fixat de owner |
| X-r2 | Subiectul permisiunii care păzește cifrele citite din cub, după tăierea tipurilor de registru; azi `RegistrulCitibil` (108) | închisă la TR-D9a (110): dreptul unic e citirea completă pe `Postare` (D9-D9); `RegistrulCitibil` a ieșit |
| X-r3 | Raportul de stoc listează capătul de consum al BCS (postarea de debit poartă lotul ca unitate, pe contul de cheltuială); semantica raportului e întrebare pentru owner (108) | activă: întrebare deschisă pentru owner |
| NG-r1 | Balanța și fișa contului filtrate pe gestiune sau pe material nu văd transferurile (BTR, ASM): valoarea rămâne la sursă. Citit din cod, nerulat | activă: de probat, apoi întrebare pentru owner (semantica filtrului pe conturile de stoc) |
| NG-r2 | Contul lotului vine din tipul curent de material al produsului; schimbarea tipului după recepție poate da refuz de stoc insuficient cu stoc existent. Dedus, nedemonstrat | activă, TR-D9b: de probat |
| NG-r3 | ASM pe același cont, cu trecerea între conturi lăsată bonului de producție; nu intră cât timp BPR e rezervat (019) | după PoC; se deschide cu BPR |
| NG-r4 | Raportul soldului fără unitate, detaliat pe cont și coordonată (loturi, partide, fișe); numai citire | după PoC; candidat TR-D9b |
| D9-r1 | Gardul analizei obligatorii pe mutări și transformări (BTR, ASM): nepăzite și înainte, și după tăiere (TR-D9a, D9-D13) | după PoC |
| D9-r3 | Explicația de audit dincolo de nivelul 1: conținutul regulii la momentul operării (jurnal de audit sau politici cu istoric propriu) și nomenclatoarele care decid contarea (D9-A8) | după PoC |
| D9-r4 | Cititorii care folosesc cheia de pereche `Postare.Pereche`: contrapartida din fișa contului și conturile corespondente din registrul jurnal (D9-A2) | după PoC |
| 110-r1 | `RegulaContare.PastreazaSemn` nu mai e citit de niciun declarant: se editează și apare în „Explică”, fără efect pe postări; se scoate sau se leagă (constatat la TR-D9a pasul 7c) | închisă 2026-10-08 (113): câmpul e scos din model, schemă, „Explică” și client, fără schimbare de postări |
| 110-r2 | Nota-punte a reclasificării 371 → 3028 / 303 (`SED#-P`) dublează pe cont ce postează acum ASM-ul `SED#-R` pe loturi; puntea cade pentru ASM-urile care postează (`tr-d9-pas5b-probe.md`, A-1) | migrare |
| 110-r3 | Propunerea owner-ului din 2026-10-06: coordonatele cubului ne-nule, cu `Guid.Empty` în loc de NULL (18 coloane `Guid?` pe `Postare`, circa 190 de comparații cu nul în modul, 37 în nucleu, FK-uri pe documente, repartitori, produse și loturi). Restrânsă 2026-10-08 la cele 11 coordonate de sold; cele 7 coloane de referință și de bloc fiscal rămân nulabile | de hotărât în contractul TR-D9b / decizia 111 („Înaintea deciziei”, pct. 6) |
| 110-r4 | Scenele vechi ale ModelCheck fără filtru `--scenarii` (lista și codurile propuse: `tr-d9-pas6c-spargere-modelcheck.md`); mutarea lor schimbă poziția în log, deci cere linie de bază nouă | după PoC |
| 110-r5 | Nota contabilă operată fără nicio postare nu mai e văzută de niciun invariant (SC-CIT-34 proba acoperirea registru → cub); acoperirea politicii de mișcare SAF-T se probează într-o singură direcție (D17-V1: fiecare pereche produsă are politică; „politica nu are perechi în plus” nu mai are probă) | după PoC |
| 110-r6 | Izolarea probelor de politica reală: `ScenariiTvaIntervale` și `ScenariiFiscale` purjează toate politicile de TVA pe FCT care nu sunt din seed, cu un filtru necorelat cu tipul de TVA al scenei; `explicatii.py` editează pe HTTP regula de contare reală din seed, deci cere bază de unică folosință (`tr-d9-pas8-inchiderea.md` §9, §11) | după PoC |
