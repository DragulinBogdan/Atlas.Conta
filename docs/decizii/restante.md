# Restanțele și amânările cu nume

**Actualizat: 2026-09-21.** [Index](README.md)

Backlog-ul dezvoltatorului: fiecare rând e o amânare declarată într-o decizie.
Identificatorul spune unde e textul integral — `36f` = sub-punctul (f) al
deciziei 36, `69-r1` = restanța 1 a deciziei 69, `D4-r1` = restanțele
D394 (decizia 71), `DIM-4` = pasul 4 al feliei dimensiunilor (decizia 54),
`C1a` = `docs/architecture-notes-2026-07-28.md`. Numele stă aici ca să nu se
piardă; motivul și contextul stau doar în fișierul deciziei. O restanță
închisă își schimbă starea, nu dispare. Limitele produsului, în forma
văzută de utilizator, sunt în
[`stare-curenta/limite-curente.md`](../stare-curenta/limite-curente.md).

| Id | Restanța | Stare |
|---|---|---|
| 21 | defalcarea multi-sursă (F) | deschisă |
| 31f | importul extraselor, imperecherea pe poziții | deschisă |
| 31e | `GenereazaChitanta` (fluxul BF) | deschisă |
| 34g | satelitul partenerilor (IBAN/delegați/contact/adrese multiple; adresa e pe `Partener`, 72a), valute | deschisă |
| 36f | TVA la încasare, facturi nesosite 408/4428, rotunjirea per document×cotă, prorata/ajustări | deschisă |
| 37g | comenzi, regenerarea DSC la recepția NIR, multi-gestiune per factură, rezervarea de stoc | deschisă |
| 46f | imperecherea returului, toleranța ASM, disciplina de apelant ITV | deschisă |
| 51e | `PoliticaEvaluare` (CMP) — sub 90c devine parametrul „unitatea de evaluare” al unității nominalizate | deschisă |
| 52h | consumul ASM pe proveniență, reziduul TRANZIT | deschisă |
| 53i | culegerea de produs pe ASM, localizarea shell-ului, perioadele fiscale manuale (≠ 2026 se adaugă de mână — continuată ca F27-r13), `Data` pe conexe | parțial: ASM închis de 76b |
| DIM-4 | curatoria grilei registrului, vizibilitatea dimensiunilor per profil (`SetareProfil`) | deschisă |
| 55g | JWT secrets la deploy, `$metadata` expune tot modelul, `Lot.Eticheta` pe OData | deschisă |
| 62g/66j | finisaj de client și ecrane XAF (`lista-react.md`) | deschisă |
| 63f | laturile interne pe `Calitati`, retrofit `MaterializeazaValori` pe BTR | deschisă |
| 64h | dimensiunea Repartitor pe rândul de bani (decizie proprie) | deschisă |
| 64k | comisionul bancar, valuta | deschisă |
| 67e | gardian de nomenclator pentru ciclul din `Cont.Parinte` | deschisă |
| 68j | smoke vizual, storno/regimuri fără TVA în backfill | deschisă |
| 69-r1 | versionarea formularului D300 | deschisă |
| 69-r2 | rândurile intracomunitare/agricultori/pro-rata/secțiunile A-B | deschisă |
| 69-r3 | regularizările pe cauză juridică | deschisă |
| 69-r6 | fișierul XML D300 | deschisă |
| 70-r1 | refuzurile gardianului pe scrierile OData ies `400 text/plain` (închisă de 77g + 80d) | închisă de 77g + 80d |
| 70-r2 | smoke XAF al gardianului de ciclu | deschisă |
| 70-r3 | poziția „linia N" fără criteriu | deschisă |
| 70-r4 | `DirectiePentru` per `ObjectChanged` | deschisă |
| 70-r5 | mesajele de binding în engleză | deschisă |
| 70-r6 | `TvaSuprascris` | deschisă |
| D4-r1 | istoricul statutului de TVA (canonicul = registrul ANAF) | deschisă |
| D4-r2 | adresa PF fără CNP | deschisă |
| D4-r3 | `N` + `tip_document` | deschisă |
| D4-r4 | data primirii facturii | deschisă |
| D4-r5 | categoria `op11` (codul NC e pe produs, 73b) | deschisă |
| D4-r6 | bonurile fiscale (G, I.1) | deschisă |
| D4-r7 | I.2 facturi/plaje/autofacturi/anulate/simplificate | deschisă |
| D4-r8 | I.3 | deschisă |
| D4-r9 | sumele TVA la încasare (I.4/I.5) | deschisă |
| D4-r10 | CAEN/I.6/opțiune (antetul/reprezentantul sunt pe `Societate`, 73a) | deschisă |
| D4-r11 | partenerul cu două coduri (SM + RO) | deschisă |
| D4-r12 | achizițiile de pe DEC fără furnizor | deschisă |
| D4-r13 | XML D394 (35c; precedentul `SaftXml`) | deschisă |
| 72-r1 | rate-limit ANAF cross-request | deschisă |
| 72-r2 | adresa hibridă 1C + ANAF fără proveniență per câmp | deschisă |
| 72-r3 | `IntrariStricate` = fatală după commit | deschisă |
| 72-r4 | radierea (`StareInregistrare`) neconsumată | deschisă |
| 72-r5 | smoke vizual XAF al acțiunii ANAF | deschisă |
| 72-r6 | `RegistruContraAnaf` real abia la a doua rulare | deschisă |
| 72-r7 | ordinea la egalitate în raportul de reconciliere | deschisă |
| 72-r8 | `CuiInterogabil("00")` | deschisă |
| 72-r9 | ecranul React de partener | deschisă |
| 72-r10 | `User` ⇒ 404 vs 403 pe comanda ANAF (închisă de 80g) | închisă de 80g |
| 73-r2 | `T`/`R`/nerezidenți | deschisă |
| 73-r3 | segmentarea | deschisă |
| 73-r4 | nomenclatorul NC8 + corecturile `ml`/`pac`/TARIC | deschisă |
| 73-r5 | `Produs.UM` string | deschisă |
| 73-r6 | ecranul React `Societate`, `CodNc`/UM pe produs | deschisă |
| 73-r7 | OpANAF 1783/2021 (termene/praguri) | deschisă |
| 73-r8 | kitul DUK J2.2.8 vs J2.2.15, `-d` inutilizabil | deschisă |
| 73-r9 | `ContFaraRol` pe facturile de achiziție Flax (date) | deschisă |
| 73-r10 | `Neincluse` întreg în sumar | deschisă |
| 73-r11 | `UnitPrice` la 2 zecimale, cantitatea negativă nemăsurată | deschisă |
| 73-r12 | 64h (`PartenerulRandului`) | deschisă |
| 73-r13 | `100030` ≠ `InactivFiscal` | deschisă |
| 73-r14 | conducătorul rupt la spațiu, `InregistratTva` derivat | deschisă |
| 73-r15 | `CuiPrefixDublat` nemăsurat | deschisă |
| 73-r16 | multi-valută | deschisă |
| 73-r17 | categoria `op11` | deschisă |
| 73-r18 | contact/IBAN parteneri | deschisă |
| 73-r19 | 422 pe CUI gol/invalid nemăsurat pe HTTP | deschisă |
| 74-r1 | `Owners`/`8038`, custodie | deschisă |
| 74-r2 | `ShipTo/From` pe BTR, `MovementComments` | deschisă |
| 74-r3 | `InvoiceNo` duplicat în L (date 1C) | deschisă |
| 74-r5 | soldul `Consum` neraportat (220 k / 340 k) | deschisă |
| 74-r7 | `Neincluse` per produs în sumar | deschisă |
| 74-r8 | stornoul S doar pe scenă | deschisă |
| 74-r10 | `T`/segmentare pe S | deschisă |
| 74-r11 | D17-V6 doar privat, proba ștergerii logice | deschisă |
| 74-r12 | ecran React `PoliticaMiscareSaft` (închisă de 81i) | închisă de 81i |
| 74-r13 | DUK J2.2.18 | deschisă |
| 74-r14 | gardian produs de stoc fără cont | deschisă |
| 74-r15 | `FaraCodNc` pe Flax | deschisă |
| 75-r1 | ASM UI: derivarea valorii produsului din consum | închisă de 76c |
| 75-r2 | scalarea rezoluției de tip (`CoduriTipPeTipuri` liniar cu baza; `ApiProiectii. CoduriTip` pe hot-path API) | închisă de 89f (`CoduriTipPeTipuri`/`IdsDocumenteDeTip` au dispărut; tipul se citește din discriminator prin `CititorTipDocument`) |
| 75-r3 | `--continua` fals-roșu pe bază importată (idempotența doar prin re-rulare integrală) | deschisă |
| 75-r4 | `PretEvaluare` 6 zecimale pe cantități mari vs invariantul 46d | deschisă |
| 75-r5 | stornoul fără timbru propriu în oracolul golirii | deschisă |
| 76-r1 | netarea plafonului e per (repartitor × LATURĂ), nu per CONT (55 chei / 36 note amestecă conturi de clasă 4 pe aceeași latură; expunere reală 3 chei) | deschisă |
| 76-r2 | `caStins` se scade din AMBELE sensuri — inatacabil azi, **devine real când un tip cu partener pe latură capătă capacitate bidirecțională** | deschisă |
| 76-r3 | perf `AsignatFataDe` (entități polimorfe, chemat de 2 × nr. contrapartide); hook-urile de stingere dispar sub 90e | deschisă (se închide la TR-D9) |
| 76-r4 | gate-ul comenzilor e pe `Document`, nu pe tipul feliei (422 vs 404 pe aceeași cauză; închisă de 80b) | închisă de 80b |
| 76-r5 | `Candidati` sub-raportează pe ușa secured, iar `User` pe ușa de scriere e refuzat de primul FK invizibil, nu de o permisiune (familia 72-r10; închisă de 80b/80f) | închisă de 80b/80f |
| 76-r6 | patru itemi de client în `lista-react.md`: căutarea sensibilă la diacritice în TOATE lookup-urile remote (colație `unaccent`/ICU sau coloană shadow — decizie de bază de date), `Lookup` care refetchează eticheta per instanță, limita convenției 61b pe valorile din PRECOMPLETARE, `window.confirm` moștenit pe ștergere (toți patru închiși de 77) | închisă de 77 |
| 77-r1 | BTR fără convenția 61b | deschisă |
| 77-r2 | `Cod`/`Denumire` neobligatorii pe nicio ușă (închisă de 77k) | închisă de 77k |
| 77-r3 | editarea `PoliticaMiscareSaft` din React (închisă de 81i), comanda ANAF de lot | parțial: editarea închisă de 81i, comanda de lot deschisă |
| 77-r4 | `CodFiscal`/`Iban`/`Marca` în afara lui `Cautare` | deschisă |
| 77-r5 | precompletarea nu distinge alegerea operatorului; invalidarea nu reîmprospătează `SelectBox`-urile montate | deschisă |
| 77-r6 | `displayExpr` de nucleu pentru `TipMaterial` | deschisă |
| 77-r7 | `Cautare` fără index (seq scan pe 20 k rânduri; cifra decide; calea: GIN `pg_trgm`, 78e) | deschisă |
| 77-r8 | permisiunea pe OData `text/plain` pe server (închisă de 80) | închisă de 80 |
| 78-r1 | căutarea din grilele XAF rămâne sensibilă la diacritice (nu trec prin `DataSourceLoader`; asumat, 44/53) | deschisă |
| 79-r1 | acțiunea XAF „Generează închiderea" (închisă 2026-09-02) | închisă 2026-09-02 |
| 79-r2 | `PoliticaInchidereTva` pe OData + ecran React (închisă de 81i) | închisă de 81i |
| 79-r3 | închiderea perioadei fiscale din client (53i) | închisă de 88 (React `/perioade`: lanț, verificare cu bife, închidere, redeschidere, istoric) |
| 79-r4 | mesajul `[Range]` în engleză pe `genereaza` (70-r5) | deschisă |
| 79-r5 | storno-ul unei închideri la o dată din ALTĂ lună ⇒ previzualizarea lunii raportează `FaraSold` (cauza greșită; data implicită din ecran e cea corectă) | deschisă |
| 79-r6 | cine are drept de citire pe `InchidereTva` vede prin previzualizare soldurile de TVA ale societății fără drept pe `RegistruContabil` (consecința asumată a lui 79b; închisă de 80e) | închisă de 80e |
| 79-r7 | „Verifică" activ pe ne-Draft arată refuzul ca eroare (convenția NTC, transversală) | deschisă |
| 80-r1 | pe conducta `ODataStore` a clientului (lookup-uri, `byKey`) refuzul ajunge ca `statusText`, nu ca mesajul serverului (limita DevExtreme `errorFromResponse`) | deschisă |
| 80-r2 | mesajul plasei DevExpress din `SaveChanges` (permisiuni pe membru) iese 403 `EroriDto` cu text englezesc (70-r5) | deschisă |
| 80-r3 | `ReportController` (scaffold) păstrează `NotFound()` gol | deschisă |
| 80-r4 | refuzurile pe `$expand` OData nemăsurate | deschisă |
| 80-r5 | captionul din mesajele 403 e numele CLR pe WebApi (fără model de aplicație XAF în host; `Refuzuri.Caption` are calea) | deschisă |
| 80-r6 | `400 "Incorrect body."` englezesc prin filtrul OData (70-r5) | deschisă |
| 80-r7 | `distribuie-valoarea` (ASM) întoarce sume din prețurile loturilor pe ușa non-secured fără drept pe `Lot` (familia 79-r6; nu e sumă peste registru) | deschisă |
| 81-r1 | implicitul pe achiziția extra-UE / de la neînregistrat RO (cad pe ancora N21) | deschisă |
| 81-r2 | seed-ul nu corectează rândurile `DinSeed` | deschisă |
| 81-r3 | rândul editat înaintea migrației F23 e marcat seed de backfill | deschisă |
| 81-r4 | `Repartitor.Cod` fără unicitate (spațiu partajat pe TPT) — din 89 spațiul de coduri e fizic aceeași tabelă (`Repartitori`, TPH), deci indexul ar fi trivial; unicitatea rămâne blocată de coliziunile legitime între familii din datele de import (continuată ca F28-r2) | deschisă |
| 81-r5 | PATCH fără schimbare = 204 pentru orice rol (capcană de probă) | deschisă |
| 81-r6 | `User` pe `api/implicite` ⇒ `Niciuna` | deschisă |
| 81-r7 | XAF: lookup-urile `TipTva` fără filtrul `Activ`, baseline lipsă pe 8 politici (44/53) | deschisă |
| 81-r8 | `SursaCont.Explicit == 0` pe rând nou (ecranele feliei 24) | închisă de 84d |
| 81-r9 | `400 "Incorrect body."` (80-r6) | deschisă |
| 83-r1 | recrearea unui rând de seed șters, la runtime | deschisă |
| 83-r2 | D394 tip `N` (achiziții de la neînregistrați) negenerat, `NIM × Achiziție` nemapat | deschisă |
| 83-r3 | codul SAF-T al lui `IMP` din nomenclator | deschisă |
| 83-r4 | DVI ca tip de document (`IMP21`, legătura n→m cu facturile de import, rândurile D300) | închisă de 86 |
| 83-r5 | `Configurator` în XAF (permisiuni de navigație) | deschisă |
| 83-r6 | = 81-r3 asumată | deschisă |
| 84-r1 | `SDD`/`SFD` fără cod SAF-T de achiziție | deschisă |
| 84-r2 | `N9.CodSafTLivrare` 310310 (rd. 10.1) vs maparea pe rd. 11 (310357/310358) | deschisă |
| 84-r3 | raportul de profil fără categoria „rând de seed lipsă" (lipsa unui `TipTva` referit de mapări aruncă din seed) | deschisă |
| 84-r4 | întoarcerea din panou nu focalizează rândul; `Unitate` fără controller OData | deschisă |
| 84-r5 | captions (`SursaCont`, membrii politicilor) fără `[XafDisplayName]` | deschisă |
| 84-r6 | gate-ul `explica` pe TIP, nu pe obiect; `tip` rezolvat înaintea gate-ului (oracol pe ancore, ca `api/implicite`) | deschisă |
| 84-r7 | rolul `Configurator` e al release-ului (permisiunile se reaplică; pe RELEASE fără user) | deschisă |
| 84-r8 | `Cont.DimensiuniObligatorii` bugetar intră în aliniere pe `DinSeed` (expunerea lui 81-r3) | deschisă |
| 84-r9 | ordinea candidaților la egalitate = ordinea bazei; dublul pe cheie pică pe index, nu pe seed | deschisă |
| 84-r10 | = F24-r1 `Import1C/Catalog.IncarcaContare` pe `Potrivire` | deschisă |
| 84-r11 | deep insert OData pentru `Configurator` și smoke XAF pe gardurile rescrise neprobate | deschisă |
| 84-r12 | stocul pe LDI nu depinde de semn (28a; observație) | deschisă |
| 85-r1 | `RegistruContabil`/`RegistruTva` pe `ServerView` după cifra de pagină pe `Server` | închisă de 85 (pe cifre, `p5-perf-masuratori.md`) |
| 85-r2 | `InstantFeedback`/`InstantFeedbackView` doar pe view-ul care trece pragul (p95 pagină > ~300 ms), cu proba în browser | deschisă |
| 85-r3 | alte proprietăți nemapate care ajung în liste (`Total` pe DetailView rămâne; regula 85g le refuză din ModelCheck) | deschisă |
| 85-r4 | `PageSize` implicit pe grile (cifra pe probă) | deschisă |
| 85-r5 | `RegulaStoc_ListView` cu override `Server` redundant cu `Options` (se curăță la atingere) | deschisă |
| 85-r6 | modul `Server` include navigațiile coloanelor ascunse (`Index = -1`); curatoria = scoase din modelul view-ului (`HideMembers`/`VisibleInListView(false)`), de măsurat pe listele grele — din 89 cauza de join a moștenirii dispare (`RegistruTva` Server: 4 JOIN-uri în loc de 35 pe baza de spike), navigațiile coloanelor incluse rămân | deschisă |
| 85-r7 | gruparea pe `Server`/`ServerView` încarcă primele rânduri ale fiecărui grup (chei + entități per grup) | deschisă |
| 85-r8 | `Refresh` execută pagina de două ori pe `Server` | deschisă |
| 85-r9 | layout-ul salvat al utilizatorului poate ascunde toate coloanele; pe `ServerView` celulele rămân goale până la Refresh (curatoria grilelor, DIM-4) | deschisă |
| 85-r10 | coloana `Produs` goală în grila Detalii a FCT `WIS26511` (date sau afișare) | deschisă |
| C1a | fluxul comenzilor (`docs/architecture-notes-2026-07-28.md`) | deschisă |
| 86-r1 | taxele vamale și accizele în costul de achiziție (ajustare de cost pe lot) | deschisă |
| 86-r2 | anularea/stornarea unei FCT legate la o DVI operată nu se refuză | deschisă |
| 86-r3 | RLF pe DVI (retur de import / re-export) | deschisă |
| 86-r4 | amânarea plății în vamă ca politică de partener | deschisă |
| 86-r5 | scadență pe DVI | deschisă |
| 86-r6 | unicitatea MRN-ului | deschisă |
| 86-r7 | SAF-T D406 emite rândul DVI cu codul de import | închisă de 86 (probată în `E2E-DVI`) |
| 86-r8 | `DviFactura` în „Explică"/OData | deschisă |
| 86-r9 | curățenia datelor Flax (reMarkable NO marcat `TI19`) | deschisă |
| 86-r10 | comisionarul vamal care plătește taxa și o refacturează (dublă postare) | deschisă |
| 86-r11 | DVI ca document stins: restul polimorf în `ImperechereService.Total` + ramura în `DocumenteCuRest` | deschisă |
| 86-r12 | mesajul refuzului `PoateFiStins` din `ImperechereService` e scris pentru viramente | deschisă |
| 86-r13 | dimensiunea Repartitor a plăților e inversată față de conturi (latura de terț poartă contul propriu) — preexistentă | deschisă |
| 86-r14 | `CreateObject` înaintea lui `Rezolva.Cere` în `NotaContabilaApply`/`FacturaIntrareApply` (orfan mascat de navigație) | deschisă |
| 86-r15 | `IMP` (0%) rămâne `DeImport`: lookup-ul liniei DVI îl propune, operarea îl refuză | deschisă |
| 86-r16 | `DocumentDetaliu_ListView` e per clasă de detaliu: al doilea tip cu `[TipDetaliu(typeof(DocumentDetaliu))]` ar împărți grila cu DVI | deschisă |
| 86-r17 | `Document` fără `DefaultProperty` (85b): lookup-urile și grilele de legătură afișează GUID-ul după selecție | deschisă |
| 86-r18 | lookup-urile de documente (`_LookupListView`) rămân cu coloanele generate, fără identificarea în față; `ListaRoot<T>` țintește doar `_ListView` | deschisă |
| F26-r1 | activele din 231 (în curs): PIF care postează 21x = 231 (decizia 87) | deschisă |
| F26-r2 | deductibilitatea valorii rămase la ieșire (art. 28 (17)) ca `Fel` nou de regulă pe rândul `Iesire` (87) | deschisă |
| F26-r3 | legătura CAS ↔ FCL de vânzare ca evidență (tiparul `DviFactura`) (87) | deschisă |
| F26-r4 | degresiva AD2; amortizarea pe unități de producție (87) | deschisă |
| F26-r5 | transferul ca rând explicit de registru (`Fel = Transfer`, valoare 0) (87) | deschisă |
| F26-r6 | reevaluarea (locul rezervat în registru; 21x = 105 prin politică extinsă; 105 → 1175 la ieșire) (87) | deschisă |
| F26-r7 | acțiunea XAF de generare AMO (87) | deschisă |
| F26-r8 | SAF-T D406 Assets + AssetTransactions din `RegistruImobilizari` (87) | deschisă |
| F26-r9 | D101 / impozitul pe profit ca proiecție peste `AmortizareDeductibila` (87) | deschisă |
| F26-r10 | ajustările pentru depreciere (29x), leasingul, obiectele de inventar date în folosință (8035) (87) | deschisă |
| F26-r11 | repartizarea cheltuielii cu amortizarea pe mai multe centre de cost cu coeficienți (87) | deschisă |
| F26-r12 | `RegistruImobilizari` pe `ServerView` (85) (87) | deschisă |
| F26-r13 | migrarea fișelor din 1C (`IntroducereSolduriInitialeMF` → PIF de deschidere cu inițialele): conectorul, nu mecanismul (87) | deschisă |
| F26-r14 | eligibilitatea metodei fiscale pe categorie (accelerata doar pe echipamente/calculatoare, art. 28 (12)) — azi doar documentată (87) | deschisă |
| F26-r15 | cele 4 poziții-părinte din catalog cu benzile pe sub-variante fără cod: fără verificare a duratei fiscale până la o decizie (87) | deschisă |
| F26-r16 | clasificația bugetară a cheltuielii cu amortizarea (codul economic) | închisă la pasul 2b (dimensiune pe fișă, `Imobilizare.CodEconomicId`, 87a/87g) |
| F26-r17 | brutul fiscal 0 pe linia PIF nu se poate exprima (`ValoareFiscala` 0 = implicit `Valoare`) (87) | deschisă |
| F26-r18 | anularea unui eveniment PIF din luna unei AMO operate e refuzată deși rândul lunar nu depinde de el (`Data >=` vs „luna >") — refuz fals, pe partea sigură (87) | deschisă |
| F26-r19 | lookup-urile XAF ale fișei (tip pe natură) și ale liniei PIF (fișe pe stare și loc) nefiltrate (87) | deschisă |
| F26-r20 | `Clasificare` căutabilă în lookup-ul XAF doar pe denumire; `Valoare` a regulii de deductibilitate formatată monetar la `Procent` (87) | deschisă |
| F26-r21 | filtrele `FilterRow` pe coloanele cu `Lookup` de enum (`Cauza`) neverificate în browser (87) | deschisă |
| F26-r22 | două `genereaza` concurente pe aceeași lună creează două drafturi care se blochează reciproc (ca ITV) (87) | deschisă |
| F27-r1 | reclasificarea pe 1174 a erorilor semnificative din exerciții anterioare; pragul de semnificație ca politică (decizia 88); sub 90k devine rând de politică (sink pe an închis → 1174) | deschisă (TR-D9) |
| F27-r2 | scadențar/aging pe partidele deschise (aceeași listă + scadența + bucket-uri) (88) | deschisă |
| F27-r3 | cursa închidere ↔ operare prin blocarea verigii perioadei (88) | închisă la pasul 0 — F1 probat pe ambele capete, restanța nu s-a activat |
| F27-r4 | închiderea de an ca operație distinctă (121 → 1174/117, soldurile de deschidere ale anului nou) (88) | deschisă |
| F27-r5 | D406/D300/D394 rectificative ca FIȘIER (marcajul de rectificativă în XML/PDF; proiecțiile expun deja conținutul) (88) | deschisă |
| F27-r6 | constatări de închidere pe reconcilierea 1C (documente neimportate în P): conectorul, nu mecanismul (88) | deschisă |
| F27-r7 | soldul în lookup-urile de partener din culegere (coloană prin `sold-parteneri`) (88) | deschisă |
| F27-r8 | concurența între operatori (25f) rămâne parcată; felia rezolvă doar cursa perioadei (88) | deschisă |
| F27-r9 | editabilitatea datei de înregistrare pe documentele GENERATE (o primesc la creare, din sursă sau din lună) (88) | deschisă |
| F27-r10 | SAF-T: inițialul de stoc rămâne pe registrul integral (mutarea pe referință cere schimbarea semanticii lui `Randuri` din `SoldPeTipStocNeraportat` — decizie de raportare) (88) | deschisă |
| F27-r11 | dimensionarea conturilor de terț pe PARTENER: azi `Repartitor` urmează laturile documentului, deci `sold-parteneri` nu e creanța per partener (familia 64h/73-r12/86-r13) (88); rezolvată STRUCTURAL de 90c (partenerul se scrie din unitate) | deschisă (TR-D9) |
| F27-r12 | integritatea snapshot-ului memorată în istoric (rânduri + sume la închidere, constatare + probă, referința fără rânduri refuzată); azi un rând șters direct din bază dă o balanță tăcut greșită (88, review 8b) | deschisă |
| F27-r13 | perioadele fiscale ale unei baze noi: seed-ul scrie 12 luni ale unui AN HARDCODAT (2026), iar crearea se poate face doar din XAF — nu din React și nu prin OData (lipsă de ergonomie, nu funcțională; continuă 53i) (88) | deschisă |
| F27-r14 | costul de CADRU al unei cereri (58 de instrucțiuni SQL de bootstrap de securitate per ObjectSpace + hidratare + serializare): motivul pentru care fișa de cont ratează ținta end-to-end deși calea ei de date costă 7 ms (88) | deschisă |
| F27-r15 | ținta de perf a balanței analitice (< 100 ms) era calibrată pe ianuarie; pe decembrie, cu 71.167 de grupe `Cont × Repartitor`, nu e atingibilă în forma de azi — de re-calibrat sau de pre-agregat (88) | deschisă |
| F27-r16 | forma proiecției `DocumenteCuRest`: candidații de la 59 (uniunea tuturor documentelor operate) și forma legăturilor se rezolvă ÎMPREUNĂ — corelarea legăturii duce panoul filtrat la 82 ms, dar calea neplafonată a constatării de rest scadent de la 220 ms la 1,02 s (respinsă motivat, cu cifre; niciun index nu lipsește) (88) | deschisă |
| F27-r17 | ordinea totală a listei `op1` din D394: cheia de ordonare nu e totală pentru persoanele fizice fără cod cu aceeași denumire, deci două generări ale aceleiași luni pot diferi la rând (familia 72-r7) (88) | deschisă |
| F27-r18 | coloana cu data înregistrării în LISTELE React de documente (câmpul e cules și afișat pe formulare; coloana ar fi trecut pragul de atingeri al pasului 3) (88) | deschisă |
| F28-r1 | simplificarea `CandidatiPereche<T, TOpus>` și a uniunii per tip din `ImperecheriProiectii` (jumătatea „tip” rezolvată de discriminator, „contrapartida per tip” e semantică); împreună cu F27-r16 (89); `ImperecheriProiectii` dispare sub 90e | deschisă (TR-D8/D9) |
| F28-r2 | index unic pe `Repartitor.Cod` (81-r4): mecanismul trivial sub TPH, blocat de coliziunile legitime între familii din import (89) | deschisă |
| F28-r3 | partiționarea sau `CLUSTER` pe `ClrType` — fără cifră care s-o ceară (89) | deschisă |
| F28-r4 | dezproxarea tipului în patru copii (`ClasaReala`, `TipReal`, `VerificaCodDenumire` inline, `TipDomeniu`); de redus la cele două semantici la atingerea gardianului (89) | deschisă |
| F28-r5 | D406 S la rece +0,2 s (+7 %) pe 12/2025: cost per proces (SQL-ul mai mic în TPH, chemările calde egale); cauza (JIT/compilarea EF a formei noi) neizolată prin profil (89) | deschisă |
| TR-r2 | notele pe conturi de stoc fără lot (2.997 pe Flax, cinci corespondențe): conectorul decide per corespondență — postare de valoare pe lot sau divergență declarată (90); intră cu tipul NTC, la TR-D7b | deschisă |
| TR-r3 | fizica re-măsurată cu postarea de stoc unificată și felul `Transfer` exclus: `Spatiu` re-definit, FK per partiție, indexul `(Unitate, Data)` (90) | deschisă |
| TR-r4 | recepția fără factură (NIR pe aviz, 408): cerință de produs de confirmat; NIR rămâne tip până atunci (90) | deschisă |
| TR-r5 | DSC din FCL trece testul documentului-copil azi; de re-judecat dacă descărcarea devine clonă fără alegere de loturi (90) | deschisă |
| TR-r6 | conectorul 1C: data reală a împerecherii (artefact 2026-09-18) și deschiderea de terți per partener / per factură deschisă (65,5 % din soldul de terț) (90); cubul scrie `Imperechere.Data` ca atare (S-D13), deci artefactul rămâne al CONECTORULUI și se raportează, nu se corectează în cub (TR-D7a) | deschisă |
| TR-r7 | sink-urile bugetare (Gratuit/Folosință/Custodie) ca gestiuni virtuale / cont 803x: fără cifre, probate la primul seed bugetar pe cub cu injectivitatea (90) | deschisă |
| TR-r8 | rulajul brut per partidă nu e sumă sub tranzacția de transfer; fișa partidei se randează ca fereastră (90) | deschisă |
| TR-r9 | probele de formă din ModelCheck (≈300): inventar rescrie/șterge la TR-D8/D9, nu înainte (90) | deschisă |
| TR-r10 | deschiderea ca tranzacție de fel `Deschidere` fără document, dar CU unitate și partener pe terți; `Deschidere.cs` scrie tranzacția, nu rânduri bloc (90); intră la TR-D7b, odată cu tipurile rămase | deschisă |
| TR-r11 | `Numar`, `DataScadenta`, `Autogenerat`, `DocumentSursa` rămân atribute ale documentului scrise la operare, sub gardianul (a) (90) | deschisă |
| TR-r12 | Δ de sold 3xx (+585.404,66 pe Flax) între registrul de stoc și cel contabil de azi: tranșată prin contractul 1 al reconcilierii 1C; constatare, nu consecință acceptată (90); intră la TR-D7b, cu tipurile care o produc | deschisă |
| FZ-r1 | granul lui `Sold` contra snapshot-urile de azi (și dacă un read model mai grosier merită ca al doilea): gate la TR-D8, nu condiție prealabilă; FZ-r2 măsurată 2026-09-19 (fișa 348 contra 248 ms) și absorbită (90) | deschisă |
| FZ-r3 | lookup-ul per partidă (SAF-T Payments, fișa partidei): index `(Unitate, Data)` pe Contabil probat pe o interogare reală (90, TR-D8) | deschisă |
| FZ-r4 | creșterea reală a coordonatelor pe un istoric lung: `Sold` la 1,2 GB și cifrele +S sunt limite inferioare (90) | deschisă |
| FZ-r5 | rândul de stoc unificat cu postarea 3xx: economie ≤ 9 % — tranșat de 90g (o singură postare); re-măsurarea rămâne TR-r3 | închisă prin 90g |
| FZ-r6 | partiționarea pe an: redeschisă doar când citirile integrale de istoric trec pragul pe baza reală; rămâne PhysicalStock (90) | deschisă |
| FZ-r7 | reperul `TaxInformation` din GLE (`LinieId` contra `PerioadaDeclarare`): întrebare de design SAF-T (90) | deschisă |
| FZ-r8 | costul FK la scară, încărcarea în bloc, `VACUUM`/bloat sub scrieri reale, interogări concurente cu operarea serializată (90) | deschisă |
| FZ-r9 | BRIN pe `Data` reevaluat pe forma cu `Sold` (90) | deschisă |
| FZ-r10 | împerecherea ca tranzacție pe date cu împerecheri datate real (90, TR-r6) | deschisă |
| IM-r1 | nivelul specific per tip ca serviciu — absorbită de declarantul per frunză (90, TR-D6b) | închisă la TR-D9 |
| IM-r2 | adaptorul EF direct și hostul fără XAF: nucleul e pur, singurul consumator e `Module`; decizie proprie după TR-D9 (90) | deschisă |
| IM-r3 | `IDocument`/`ILinie` peste bază — absorbită: operandul închis e DTO (90) | închisă la TR-D9 |
| IM-r4 | identificatorul semantic al tipului + fabrici — depășită de 89 (`CititorTipDocument.Clasa`) și de regimul dual ca dată (90) | închisă |
| IM-r5 | async efectiv în host-uri: felie proprie cu cifră, după TR-D9 (90) | deschisă |
| N-r1 | invariantul 7 al designului (motorul reproduce baseline-ul Import1C) nu e testabil în nucleul pur: proba supremă a lui TR-D7/D10, nu test de formă (TR-D6a) | deschisă |
| N-r2 | capătul virtual al cantității pe postarea de terț cu gestiune virtuală Furnizor/Client, C2 peste toate postările (N-D4, lărgește litera 090g): CONFIRMAT de pilotul FCT (TR-D6b) — invizibil proiecțiilor pe gestiune reală și pe unitate-lot; `GestiuniVirtuale` sunt constante ale nucleului și C5 nu cere unitate pe gestiune virtuală (profilul fără `RolTert` n-are partidă pe 401) (TR-D6a) | închisă |
| N-r3 | evaluarea ieșirii pe raportul curent al unității (N-D7) diferă de prețul înghețat al lotului de azi pe loturile cu corecție: MĂSURAT la TR-D6b (`NUC-BCS-N-R3-*`: lot 20 buc / 300 lei, preț înghețat 10, consum 5 ⇒ 50 azi, 75 în nucleu, Δ = +25); documentele de azi nu pot da două prețuri pe același lot, Δ apare doar din deschideri/import; intră în diferențele declarate ale reconcilierii TR-D7 (TR-D6a) | închisă |
| N-r4 | TVA decisă pe document × cotă și postată per linie prin Hamilton pe fiecare semn (090j) contra rotunjirii per linie de azi: MĂSURAT la TR-D6b (`NUC-FCT-N-R4-*`: 3 × 0,01 la 21 % ⇒ 0,00 azi, 0,01 în nucleu, Δ = 0,01 pe o singură linie); taxa culeasă e autoritară cu toleranța de pilot `0,01 × liniile cu TVA` (B-r1 o face rând de politică) (TR-D6a) | închisă |
| N-r5 | stornoul unei postări ATRIBUITE (reevaluare) intră în selecție (090i) și poartă cauza altui document, dar contrapartida ei inversată e nedefinită ⇒ tranzacția de storno nu conservă valoarea; se tranșează la TR-D9 odată cu reevaluarea (TR-D6a) | deschisă |
| N-r6 | amendamente de literă ale contractului TR-D6a consemnate în cod: cazurile `Decizie` poartă `Linie`; `Motor.Transfera` ia `Mutare`, nu `Declaratie` (fără decizii/ipoteze pe transfer); ierarhia închisă ține prin constructor `private protected`, copy-constructorul rămâne `protected` (CS8878); `Unitate with { Fel }` și `Declaratie with { Document, Miscari }` nu se re-validează inter-câmp — se construiesc din nou (TR-D6a) | deschisă |
| N-r7 | ordinea FIFO a nucleului e `Guid.CompareTo` (componentă cu componentă) prin `Fifo.Intai`; `ORDER BY uuid` în Postgres are altă ordine ⇒ la TR-D7 candidații se sortează în nucleu, nu în SQL, altfel „ultima ia restul” cade pe alt lot la date egale (TR-D6a) | deschisă |
| N-r8 | stornoul unei tranzacții de fel `Transfer` e de fel `Storno` și intră în citirile cu `includeTransfer = false`; pe proiecțiile pe cont contribuie zero (±v pe aceeași latură), dar apare ca rând în listările de jurnal — regula listării rămâne a lui TR-D8, acum că `Transfer` se persistă (TR-D6a, TR-D7a) | deschisă |
| N-r9 | `Repartizare.Hamilton` poate depăși `decimal` la ponderi ~1e20 × total ~1e8; inaccesibil cu baze ≤ 1e10; dacă un apelant trimite preț × cantitate brute ca ponderi, se normalizează întâi (TR-D6a) | deschisă |
| B-r1 | toleranța taxei culese, constantă de pilot în `Fapte.Operand`, refuza facturi pe care motorul vechi le operează (TR-D6b): devine `PoliticaTva.TolerantaTaxa`, MĂSURATĂ pe Flax la TR-D7a (275 de documente refuzate la 0,01 pe linia cotei, 17 la 0,10, maximul 55,87 — abateri reale ale datelor culese, nu rotunjire) și închisă ca politică OPȚIONALĂ: `null` = taxa culeasă autoritară, fără gard, ca azi; valoarea de produs rămâne S-r1 (TR-D7a) | închisă prin S-D15 |
| B-r2 | sensul laturilor trezoreriei e tip-dependent și declarantul unic nu-l cunoaște (TR-D6b): devenit dată pe `TipDocument` (`LaturaContPropriu` = `Predator` pe plată, `Primitor` pe încasare, seed pe ambele profiluri), cu refuzul `LATURA_CONT_PROPRIU_NEPOTRIVITA`; `CONT_PROPRIU_LIPSA` rămâne pentru lipsă (TR-D7a) | închisă prin S-D7 |
| B-r3 | recepția facturii (TR-D3) n-are regulă de contare proprie pe FCT: contrapartida se ia de pe regula `FCT/Serviciu`/`Cheltuiala` sau din politica de TVA; la TR-D7, când NIR-ul conex dispare, regula recepției devine rând de politică pe FCT (`FCT/Stoc`) (TR-D6b) | deschisă |
| B-r4 | taxarea inversă n-are azi fapt fiscal colectat (un singur rând `RegistruTva`, sens achiziție): piciorul 4427 al declarantului FCT rămâne fără `CodTva`; jurnalul de vânzări al autolichidării se tranșează la TR-D7/D8 (TR-D6b) | deschisă |
| B-r5 | linia FCT care numește un lot recepționat pe aviz (`NIR` manual) ar posta `408 = 401` (TR-D3): CĂUTAT în motor la TR-D7a — fluxul NU există (NIR-ul manual postează `3xx = 401`, iar linia de stoc a facturii își naște singură lotul la culegere); `408` apare doar ca reclasificare de DATE la import (`HandlerFactura`), pe 17 facturi ale bazei Flax. Rămâne deschisă cu constatarea: nu se implementează fără oracol (TR-D7a) | deschisă |
| B-r6 | `Valuta`/`Curs` sunt câmpuri de frunză pe `FacturaIntrare` fără interfață declarată: operandul le lasă `null`; `IDocumentCuValuta` la primul consumator real (partida în valută, TR-D9) (TR-D6b) | deschisă |
| B-r7 | o linie cu DOUĂ conturi cu `RolTert` era nedefinită în declaranți (TR-D6b): 0 cazuri pe BCS/PLT/INC, 25 de facturi pe Flax (`408`, `4091`, `4092` contra `401`). Închisă ca REGULĂ, nu ca refuz: linia numește partidă pe AMBELE capete, fiecare pe contul lui (TR-D7a) | închisă prin S-D16 |
| B-r8 | gestiunile virtuale (`GestiuniVirtuale.Furnizor/Client/Consum`) sunt constante deterministe fără rând de nomenclator: devin rânduri `DinSeed` cu aceleași id-uri dacă un raport le cere nume (TR-D7/D8) (TR-D6b) | deschisă |
| B-r9 | `Document.Declarant() == null` era regimul dual al pilotului (TR-D6b): gardul e acum dată (`TipDocument.PosteazaInCub`), iar un tip marcat a cărui clasă nu declară e eroare de configurare la operare, nu regim tăcut (TR-D7a) | închisă prin S-D3 |
| B-r10 | linia fără regulă de contare: motorul vechi o SARE tăcut, declaranții o REFUZĂ (TR-D6b). Gard CONFIRMAT la TR-D7a: 0 cazuri pe cele 53.449 de documente ale pilotului pe Flax; singura apariție era o linie de natură `Tehnica` pe o factură a unei scene ModelCheck, ale cărei 100 de lei dispăreau din contare — scena corectată, refuzul rămâne regula (TR-D7a) | închisă |
| B-r11 | `DocumentDetaliu` n-avea coloană de poziție, iar N-D7 și splitul PLT depind de ordinea liniilor (TR-D6b): `Pozitie` se atribuie o dată, în `SaveChanges`-ul contextului, și e citită de AMBELE motoare (`OrderBy(Pozitie).ThenBy(ID)` în cele cinci enumerări ale motorului vechi și în operand); ordinea la cereri concurente rămâne S-r9 (TR-D7a) | închisă prin S-D6 |
| S-r1 | valoarea de produs a lui `PoliticaTva.TolerantaTaxa`: seed-ul privat e `null` (fără gard), dar pe Flax 275 de facturi se abat peste 0,01 pe linia cotei, 17 peste 0,10 și 15 perechi document × cotă peste 0,50, cu maximul 55,87 (`ROYAL250801852`) — owner-ul decide valoarea și dacă abaterile mari se raportează conectorului (TR-D7a) | deschisă |
| S-r2 | deciziile și ipotezele contractului (`AlocareFifo`, `ValoareIesire`, `SoldUnitateCitit`) nu se persistă; un cititor de audit („de ce a costat atât”) le cere la TR-D8 (TR-D7a) | deschisă |
| S-r3 | gestiunile virtuale n-au FK pe `Postare` (id-uri fără rând de nomenclator); B-r8 le face rânduri `DinSeed` și atunci FK-ul pe `Gestiune` intră pe ambele partiții (TR-D7a) | deschisă |
| S-r4 | snapshot-ul EF declară cheia `ID` și `FK_Postare_Tranzactie_TranzactieId` pe părinte, baza are cheia `(Spatiu, ID)` și FK-uri per partiție: divergență DECLARATĂ cât timp XAF EF Core nu suportă chei compuse, probată de `STR-SCHEMA` — orice migrație viitoare pe `Postare`/`Tranzactie` se scrie în SQL (TR-D7a) | deschisă |
| S-r5 | pe Flax 709 împerecheri de plată se plafonează la restul partidei stinsului și una (`SED00001497-4`, 0,05) se sare fiindcă stinsul n-are rest pe contul de referință: fapt de DATE al conectorului, de raportat în reconcilierea 1C (TR-D7a) | deschisă |
| S-r6 | `PosteazaInCub` e aliniată de seed la fiecare updater, ca orice rând `DinSeed`: o bază pe care flag-ul a fost oprit manual are documente operate fără tranzacție `Operare` în cub până la TR-D9, iar litera (e) a reconcilierii le raportează — coloana devine read-only în UI sau iese din aliniere, de decis până la TR-D9 (TR-D7a) | deschisă |
| S-r7 | un lot născut de o linie „în roșu” (valoare negativă, cantitate pozitivă) se evaluează negativ tăcut, ca în motorul vechi: invariantul „un lot nu se evaluează negativ” e pierdut odată cu admiterea semnului în `Operare` (TR-D7a) | deschisă |
| S-r8 | dry-run-ul nu fixează prețul și data lotului născut de document, deci contractul lui diferă de cel real pe `Unitate.Deschisa`; fără refuz fals azi (TR-D7a) | deschisă |
| S-r9 | `Pozitie` la cereri concurente poate da dubluri (două ObjectSpace-uri calculează `max + 1` din aceeași bază); ordinea rămâne deterministă prin `ThenBy(ID)`, dar proba promite `1..n` (TR-D7a) | deschisă |
| S-r10 | purja scenei `VerificaSaftStocuri` șterge `TipMaterial`-ul de scenă fără `ReguliContare`-le pe care seeder-ul i le atașează: o cădere în mijlocul scenei lasă un reziduu care blochează DEFINITIV rulările următoare pe acea bază, până la ștergerea manuală (TR-D7a) | deschisă |
| S-r11 | dry-run-ul (`Valideaza`) prinde din declarație doar `OperareException`; o excepție de alt fel (`InvalidOperationException` din `Contractare`, `ArgumentException` din `N.Unitate`) iese 500, nu 422 — de tranșat la primul caz real sau la TR-D8, când dry-run-ul capătă cititor (TR-D7a) | deschisă |
| T-r1 | 090 (a) „EXACT o tranzacție `Operare`" devine „cel mult una `Operare` și cel mult una `Transfer`, cel puțin una" pentru tipurile cu linii care nu schimbă contul (BTR, ASM); litera (e) amendată; textul deciziei 090 nu se rescrie, amendamentul e în contractul TR-D7b (T-D2) | aplicată la pasul 1 (2026-09-21): `Declaratie.Mutari`, `Contract.Tranzactii`, litera (e) pe `Operare` ⊕ `Transfer` de stoc; rămâne deschisă până ASM (pasul 5) probează ramura `Operare` pe scenă |
| T-r2 | reziduul valoric lăsat de RLF pe lotul golit (valoare fiscală ≠ raportul lotului) contrazice 090 (j); se rezolvă la TR-D9 prin re-evaluarea unității cu reziduul spre 658/758 din politică (TR-D7b) | deschisă |
| T-r3 | postările NTC pe conturi cu `RolTert` care rămân FĂRĂ partener după (B) și pe 3xx fără lot (TR-r2): declarate până la TR-D9; litera (f) le exclude nominal (TR-D7b) | deschisă |
| T-r4 | `Deschidere.cs:43-50` și decizia 047 afirmă că 1C nu defalcă soldul de terț pe partener la 01.01 — FALS: defalcarea e în `BalantaNivel3`; se corectează la pasul 6, TR-r6 (deschiderea) și TR-r10 se închid (TR-D7b) | deschisă |
| T-r5 | DVI deschide partidă pe 446 prin S-D16 deși `PoateFiStins = false`; hook-ul e al registrelor până la TR-D9 (TR-D7b) | deschisă |
| T-r6 | `Custodie` pe bugetar: cont 803x sau gestiune virtuală, după cum are planul bugetar contul — constatat la pasul 5 (TR-D7b) | deschisă |
| T-r7 | diferența declarată T-D2.2 (BTR: `round(q × PretUnitar)` în registre contra raportului curent în cub, 535 documente / 28,60 lei absolut pe Flax) și T-D4.2 (DSC: 842 documente / Σ +31,01 lei, |Δ| max 16,50) e invizibilă reconcilierii (a)–(g) și devine vizibilă la citirile pe cub per gestiune × lot; TR-D8 o raportează, nu o absoarbe (TR-D7b) | deschisă |
| T-r8 | `RegimTva.Capitalizat` pe o linie de FCL nu se desface în bază + taxă (FCT o face prin `Netele`); pe Flax nu există, pe bugetar nu e politică de TVA — de pin-uit la TR-D9 dacă apare (TR-D7b, pasul 2) | deschisă |
| T-r9 | `TipDocument.LaturaContPropriu` (B-r2, dată de seed) e redundantă cu contractul structural `Plata`/`Incasare.Laturi()` (T-D13): nimeni n-o mai citește; coloana și rândul de seed se scot la prima migrație care atinge `TipDocument`, nu acum (T-D13 (d): nicio migrație în pasul 2b) (TR-D7b, pasul 2b) | deschisă |
| IM-r7 | extensia per client a modelului (`DbContext` de extensie / migrații per client): spike separat înaintea oricărei decizii de produs (90) | deschisă |
