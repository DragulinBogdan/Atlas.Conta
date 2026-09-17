# Restanțele și amânările cu nume

**Actualizat: 2026-09-17.** [Index](README.md)

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
| 51e | `PoliticaEvaluare` (CMP) | deschisă |
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
| 75-r2 | scalarea rezoluției de tip (`CoduriTipPeTipuri` liniar cu baza; `ApiProiectii. CoduriTip` pe hot-path API) | deschisă |
| 75-r3 | `--continua` fals-roșu pe bază importată (idempotența doar prin re-rulare integrală) | deschisă |
| 75-r4 | `PretEvaluare` 6 zecimale pe cantități mari vs invariantul 46d | deschisă |
| 75-r5 | stornoul fără timbru propriu în oracolul golirii | deschisă |
| 76-r1 | netarea plafonului e per (repartitor × LATURĂ), nu per CONT (55 chei / 36 note amestecă conturi de clasă 4 pe aceeași latură; expunere reală 3 chei) | deschisă |
| 76-r2 | `caStins` se scade din AMBELE sensuri — inatacabil azi, **devine real când un tip cu partener pe latură capătă capacitate bidirecțională** | deschisă |
| 76-r3 | perf `AsignatFataDe` (entități polimorfe, chemat de 2 × nr. contrapartide) | deschisă |
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
| 81-r4 | `Repartitor.Cod` fără unicitate (spațiu partajat pe TPT) | deschisă |
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
| 85-r6 | modul `Server` include navigațiile coloanelor ascunse (`Index = -1`); curatoria = scoase din modelul view-ului (`HideMembers`/`VisibleInListView(false)`), de măsurat pe listele grele | deschisă |
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
| F27-r1 | reclasificarea pe 1174 a erorilor semnificative din exerciții anterioare; pragul de semnificație ca politică (decizia 88) | deschisă |
| F27-r2 | scadențar/aging pe partidele deschise (aceeași listă + scadența + bucket-uri) (88) | deschisă |
| F27-r3 | cursa închidere ↔ operare prin blocarea verigii perioadei (88) | închisă la pasul 0 — F1 probat pe ambele capete, restanța nu s-a activat |
| F27-r4 | închiderea de an ca operație distinctă (121 → 1174/117, soldurile de deschidere ale anului nou) (88) | deschisă |
| F27-r5 | D406/D300/D394 rectificative ca FIȘIER (marcajul de rectificativă în XML/PDF; proiecțiile expun deja conținutul) (88) | deschisă |
| F27-r6 | constatări de închidere pe reconcilierea 1C (documente neimportate în P): conectorul, nu mecanismul (88) | deschisă |
| F27-r7 | soldul în lookup-urile de partener din culegere (coloană prin `sold-parteneri`) (88) | deschisă |
| F27-r8 | concurența între operatori (25f) rămâne parcată; felia rezolvă doar cursa perioadei (88) | deschisă |
| F27-r9 | editabilitatea datei de înregistrare pe documentele GENERATE (o primesc la creare, din sursă sau din lună) (88) | deschisă |
| F27-r10 | SAF-T: inițialul de stoc rămâne pe registrul integral (mutarea pe referință cere schimbarea semanticii lui `Randuri` din `SoldPeTipStocNeraportat` — decizie de raportare) (88) | deschisă |
| F27-r11 | dimensionarea conturilor de terț pe PARTENER: azi `Repartitor` urmează laturile documentului, deci `sold-parteneri` nu e creanța per partener (familia 64h/73-r12/86-r13) (88) | deschisă |
| F27-r12 | integritatea snapshot-ului memorată în istoric (rânduri + sume la închidere, constatare + probă, referința fără rânduri refuzată); azi un rând șters direct din bază dă o balanță tăcut greșită (88, review 8b) | deschisă |
| F27-r13 | perioadele fiscale ale unei baze noi: seed-ul scrie 12 luni ale unui AN HARDCODAT (2026), iar crearea se poate face doar din XAF — nu din React și nu prin OData (lipsă de ergonomie, nu funcțională; continuă 53i) (88) | deschisă |
| F27-r14 | costul de CADRU al unei cereri (58 de instrucțiuni SQL de bootstrap de securitate per ObjectSpace + hidratare + serializare): motivul pentru care fișa de cont ratează ținta end-to-end deși calea ei de date costă 7 ms (88) | deschisă |
| F27-r15 | ținta de perf a balanței analitice (< 100 ms) era calibrată pe ianuarie; pe decembrie, cu 71.167 de grupe `Cont × Repartitor`, nu e atingibilă în forma de azi — de re-calibrat sau de pre-agregat (88) | deschisă |
| F27-r16 | forma proiecției `DocumenteCuRest`: candidații de la 59 (uniunea tuturor documentelor operate) și forma legăturilor se rezolvă ÎMPREUNĂ — corelarea legăturii duce panoul filtrat la 82 ms, dar calea neplafonată a constatării de rest scadent de la 220 ms la 1,02 s (respinsă motivat, cu cifre; niciun index nu lipsește) (88) | deschisă |
| F27-r17 | ordinea totală a listei `op1` din D394: cheia de ordonare nu e totală pentru persoanele fizice fără cod cu aceeași denumire, deci două generări ale aceleiași luni pot diferi la rând (familia 72-r7) (88) | deschisă |
| F27-r18 | coloana cu data înregistrării în LISTELE React de documente (câmpul e cules și afișat pe formulare; coloana ar fi trecut pragul de atingeri al pasului 3) (88) | deschisă |
