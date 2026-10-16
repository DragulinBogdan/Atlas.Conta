# Verificare

**Actualizat: 2026-10-16.** [Index](README.md)

ModelCheck, scenariile, probele pe sursă, pe model și HTTP, scările de perf
și documentația.

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
| Declarant, operand, `Fapte.Operand` | Scenariile independente ale tipului, apoi ModelCheck pe AMBELE profiluri; `NUC-*` (`ProbeNucleu.Proba`) probează contractul acceptat, conservarea, determinismul și `≤ 16` interogări per operand. `Metadata clientului e la zi` verifică proprietățile noi pe `Document` (TR-D6b, amendat de 091) |
| Citire nouă din cub, în orice proiect de producție | Intrare în `Cub/Citiri`, apoi ModelCheck: `091-r3` refuză accesul la `Postare`/`Tranzactie` în afara ei și a excepțiilor numite (scanare pe sursă, fără bază) |
| Lista de evidență a cubului (`PostareVizual`, view-ul, controllerul, gardul) | ModelCheck pe ambele profiluri: `STR-VIZUAL-1…5` (paritatea coloanelor cu `Postare`, un rând per postare, marcajul contraponderii și eticheta partidei, gardul la activare pe rol cu criteriu de rând, cele trei refuzuri ale gardianului) și `D9-P4-LISTA-1` pe modelul real al hostului; `neexpunere-cub.py` pe host viu; lista în browser (D9-A12) |
| Entitățile sau migrațiile cubului (`Postare`, `Tranzactie`) | ModelCheck pe ambele profiluri: probele `STR-SCHEMA-*` (partiționarea LIST, cheia `(Spatiu, ID)`, setul ÎNCHIS de FK-uri per partiție, indexii, absența timbrelor XAF); migrația se scrie în SQL, nu se lasă generată (S-D2, S-r4) |
| Contractul laturilor (`Document.Laturi()`, T-D13) | ModelCheck pe ambele profiluri, ultima scenă (`VerificaLaturi`): `STR-LATURI-CONTRACT` (fiecare `TipDocument` din seed → clasa → contract cu părți nevide; metoda e abstractă, deci și compilatorul o cere), `STR-LATURI-REFUZ` (latura de partea greșită refuzată pe ușa declarației și pe ușa entității cu ACEEAȘI linie `COD: mesaj`; calitatea lipsă numită; un tip fără declarant refuzat pe ușa entității), `STR-LATURA` (PLT inversată = doar `PREDATOR_NEPOTRIVIT`, înaintea declarantului). Probele de laturi ale tipurilor asertează CODUL, nu textul vechi. Pe date reale: recensământul laturilor pe clona Flax (contract T-D13); după 091 clona e sursă de recensământ, nu gate |
| Materializare, declarantul unui tip, împerecherea ca `Transfer` | ModelCheck pe ambele profiluri: probele `STR-*` pe scenele BCS, Trezorerie și FCT — operare, roundtrip, storno, anulare, refuz, configurație, poziție, transfer, latură, corecție — cu toleranța taxei comutată local (`ProbeCub.CuToleranta`, cu restaurare) și purja rândurilor de cub ale documentelor scenei (S-D8) |
| Tip care declară (orice tip operabil) | fișierul tipului din `docs/nucleu/scenarii/` complet și verde pe ambele profiluri (în lucru: `--scenarii <TIP>`; la commit: suita integrală): ciclul 1–8 (operare, linii multiple, storno în perioadă și peste graniță, anulare, corecție în perioadă închisă, stingere, citiri) + cazurile-limită aplicabile + lanțurile `SC-X-*` care îl ating; așteptările scrise de mână din regula contabilă, nu din registre sau oracol (091 (a)–(c)). `--declaratie-pe-baza` și `--reconciliere-cub` rămân unelte de diagnostic pentru migrare, nu gate (S-D9 amendat de 091) |
| Documentație | Concordanță cu implementarea, link-uri locale și diff |

## Rulare reproductibilă (`scripts/verifica.ps1`)

Din rădăcină, cu PowerShell 7 și .NET 10:

```powershell
pwsh -NoProfile -File nou/tools/ModelCheck/scripts/verifica.ps1 -Suita Scenarii -Tip BCS -Profil Ambele -Sufix .CodexBCS -PregatesteBaze
pwsh -NoProfile -File nou/tools/ModelCheck/scripts/verifica.ps1 -Suita Nucleu
pwsh -NoProfile -File nou/tools/ModelCheck/scripts/verifica.ps1 -Suita Integral -Profil Ambele -Sufix .CodexBCS
pwsh -NoProfile -File nou/tools/ModelCheck/scripts/verifica.ps1 -Suita Infrastructura
```

`-PregatesteBaze` (Python cu `psycopg`) clonează bazele locale de profil
`Atlas.Conta.BackOffice` și `Atlas.Conta.ModelCheck.Privat` cu sufixul dat
(`CREATE DATABASE … TEMPLATE`, 127.0.0.1:5446, postgres/postgres): sursele
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
timeout general pentru ModelCheck.

Suitele cititorilor de partide și sold rulează prin același wrapper, pe
profilul privat și pe o bază cu sufix: `-Suita D9bReferinta` (referința A a
citirilor și a scrierii pe partide), `-Suita D9bScriere` (numai comenzile, pe
drafturile păstrate), `-Suita D9bHttp` (proba HTTP a originilor partidelor),
`-Suita D9bComparatie` și `-Suita D9bRaport` (comparația A/B). Cifrele,
artefactele SQL și limita copiilor sintetice ale referinței sunt în
[raportul referinței](../nucleu/tr-d9b-pas3-referinta.md). (TR-D9b)

## Condițiile rulării ModelCheck

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

ModelCheck referă proiectul Blazor.Server: build-ul lui pică pe DLL-uri
blocate cât timp hostul Blazor rulează din același `bin` (același tipar ca
`verifica:drift` cu WebApi pornit). Se oprește hostul înainte de build. (85h)

Scenariile își curăță datele marcate. Întreruperea procesului poate lăsa
documente care influențează alte scenarii; curățarea se limitează la datele
identificate ale testului, în baza verificată. Auditul se păstrează. (70e, 81h)

Curățenia unei scene nu selectează după `DinSeed`
(`ScenaDocumente.CurataPolitica`). SC-CIT-111 folosește un tip de material și
o regulă proprii; purja elimină numai regulile și refuzurile de seed ale
acelui tip. SC-CIT-111-IZOLARE verifică reluarea după editare/ștergere, cu
reguli BCS străine și refuzuri preexistente păstrate. SC-X-26 urmează aceeași
regulă: regula ei stă pe un tip de material propriu, iar refuzul de seed lăsat
de ștergerea prin gardian e curățat pe identitate. Providerul suitei și cel al
Import1C poartă `EFCoreOptimisticLockInterceptor`, ca hosturile
(`AddSecuredEFCore`): contorul rândului de politică crește la salvare și în
harness, altfel SC-CIT-111 n-ar măsura ce face hostul. (D9-A8)

Procesele lungi se lansează cu jurnal și cod de ieșire capturat. Pe Windows,
procesele de fundal se lansează cu fereastra ascunsă. Jurnalele publicate nu
includ parole, tokenuri sau adrese de feed cu credențiale. (50d, 73f)

Validatorul DUK cere un director temporar accesibil procesului. Validarea
D406 (XSD + DUK) folosește numai artefactele pin-uite prin SHA-256 în
`ManifestD406`: XSD v249 versionat în `nou/tools/ModelCheck/Anaf/`
(namespace substituit declarat), kitul DUK J2.2.18 și nomenclatorul din
`anaf/` (gitignored; alt director prin `ATLAS_ANAF`). Kitul absent sau
schimbat (inclusiv în timpul rulării) pică proba, nu o sare; perioada
validatorului vine din antetul fișierului, fiindcă ea alege nomenclatorul.
Fiecare rulare scrie `manifest-d406.json` lângă fișierele validate, cu
proveniența liniilor GL și a facturilor pentru fișierele certificate.
(S0-R1…R4, S0-R8)

## Capcanele rulării

- O SINGURĂ rulare ModelCheck o dată: două concurente crapă în purje și lasă
  reziduu (`TipuriMaterial` cu codul `E2E-SAFT-S-TIP` plus `RegulaContare`
  `DinSeed` pe care seeder-ul i-o re-atașează la fiecare rulare). Purja
  SAF-T șterge întâi regulile `DinSeed` atașate tipurilor temporare ale
  scenei, fără ca asta să permită rulări concurente. (S-r10)
- Două ModelCheck-uri (sau un ModelCheck și un `dotnet build`) concurente își
  blochează DLL-urile: o singură rulare o dată, construită ÎNAINTE
  (`--no-build` pe un binar vechi probează codul vechi). Redirectarea `*>` din
  PowerShell scrie log-ul UTF-16; rețeta
  `run-nucleu/tr-d6b/pas4-final/run.sh` (bash) scrie UTF-8 și numără
  `OK`/`FAIL`. (TR-D6b)
- Interogările pe catalogul Postgres cer cast explicit: `partattrs` e
  `int2vector` indexat de la 0, `conkey` e `int2[]` de la 1, iar `partstrat`
  e `"char"` și cere `::text` înainte de concatenare, altfel interogarea pică
  și oprește rularea.
- O clonă a bazei de import poartă valoarea DEFAULT a unei coloane noi, nu
  valoarea de seed (`TolerantaTaxa` 0 contra `null`): se aliniază înainte de
  o rulare pe clonă, altfel refuzurile sunt ale bazei, nu ale codului.
- Citirea fără documentul curent ia referința strict înaintea datei, deci un
  document datat exact la sfârșitul perioadei de referință, re-contractat pe o
  bază cu perioadele deja închise, nu e refuzat `STOC_INSUFICIENT`. (T-D4.3)
- Un reziduu lăsat de o cădere în scena F27 nu e curățat de integrala
  următoare (PER/SOL rulează înainte și pică pe precondiții globale); se
  purjează prin funcțiile celor două scene. (TR-D9a)
- O rulare-diagnostic care dezactivează un gard și cade lasă documente în
  baza bugetară; scena e2e își curăță singură reziduul la următoarea
  integrală, dar `--scenarii` nu, deci după o cădere se rulează întâi
  integrala. (TR-D9a)

## Catalogul de scenarii

ModelCheck nu compară cubul cu un al doilea scriitor. Corectitudinea o poartă
catalogul de scenarii (091) și invarianții interni ai cubului
([`INV-CUB`](#invarianții-cubului-inv-cub)). (TR-D9a)

Refuzurile se probează pe două uși. Pe ușa declarației
(`Materializare.Refuzuri`) refuzul poartă codul stabil (`CoduriRefuz`, linia
`COD: mesaj`). Pe ușa entității (`ComenziDocument.Valideaza` / `Opereaza`,
prin `MotorOperare`) rulează întâi gardurile comenzii și validarea clasei
(`Document.ValideazaOperare`), care refuză cu text ÎNAINTEA declarantului;
acolo se asertează familia mesajului și absența efectelor. Refuzul laturii
poartă aceeași linie `COD: mesaj` pe ambele uși (`STR-LATURI-REFUZ`). (T-D13)

Cataloagele [BCS, FCT, PLT/INC, BTR și FCL/DSC](../nucleu/scenarii/README.md)
au loturi independente, cu fixture prin documentele reale și comenzile
`ComenziDocument`; probele `NUC-*` rămân regresie. Sumele directe ale
scenariului peste cub nu înlocuiesc verificarea cititorilor comuni. (TR-D8)

Cataloagele [NTC](../nucleu/scenarii/NTC.md) și [ITV](../nucleu/scenarii/ITV.md)
au probe independente în `ScenariiNtc` și `ScenariiItv`, cu recensământ
reproductibil în `recensamant-ntc-itv.sql`. NTC este activat pe ambele
profiluri, ITV numai privat; bugetarul probează `ProfilInert`. Decizia 092
extinde cheia partidei la document × cont × partener; SC-NTC-13 verifică
separarea. SC-NTC-22
verifică dependența FIFO între data nominalizării și data inversării ei.
Rezultatele rulărilor sunt consemnate în fișierele tipurilor.

Familia [EX](../nucleu/scenarii/EX.md) (`ScenariiEx`, `--scenarii EX`)
probează închiderea conturilor de rezultat pe ambele profiluri, pe anul 1975:
soldurile lunii sunt cumulate de la începutul bazei, deci orice istoric
anterior ar intra în așteptări. Probele care închid luni pentru alt scop
(perioadele, acceptarea, soldurile materializate, concurența, PerfCub) țin
`REZ-NEINCHIS` pe `Ignorat` sau acceptabil cât rulează, ca `ITV-LIPSA`, și
restaurează severitatea din seed. (E3-D5)

Comenzile, încercările intermediare și limitele sunt în
[DESCHIDERE.md](../nucleu/scenarii/DESCHIDERE.md), NIR în
[NIR.md](../nucleu/scenarii/NIR.md); lanțul Folosinta în
[LDI.md](../nucleu/scenarii/LDI.md); review-ul ASM în
[ASM.md](../nucleu/scenarii/ASM.md); citirile comune în
[CITIRI.md](../nucleu/scenarii/CITIRI.md), cu review-ul propriu și limitele în
[tr-d8-review-codex.md](../nucleu/tr-d8-review-codex.md).
[Catalogul D9B](../nucleu/scenarii/D9B.md) verifică comportamentul cititorilor
de partide separat de proiecția persistată a originilor. (TR-D9b)

Gardul analizei obligatorii pe mișcările contractului are proba lui directă,
`GARD-ANALIZA` (`ProbeGardAnaliza`, pe funcția pură, fără bază), și proba
prin comandă SC-FCT-12…14. Scena `ScenariiTaiere` (codul `X`) fixează numeric
tipul care nu declară (SC-X-25), regula de contare fără consumator (SC-X-26)
și lotul după stornarea recepției (SC-X-27). Scenele BCS, ASM, RLF, RDC și
DESCHIDERE asertează și valoarea LINIILOR, nu numai postările. (TR-D9a)

Plafonul candidaților DVI se probează în ModelCheck.

## Invarianții cubului (`INV-CUB`)

Hosturile nu scanează istoria la pornire; invarianții cubului
(`Cub.Citiri.Invarianti`) rulează în ModelCheck, ca `INV-CUB`. (102)

`INV-CUB` (`tools/ModelCheck/AcoperireInvarianti.cs`) rulează în
`ScenaDocumente.Ruleaza` după scenă și înaintea purjei ei, pe faptele
produse, inclusiv stornourile. Invarianții: proveniența inverselor,
calificarea fiscală, taxa postată = taxa liniei, echilibrul pe tranzacție și
carte, conservarea transferului, unitățile de partidă, proveniența fișelor,
forma coordonatelor (`CITIRE_COORDONATE_INVALIDE`, C111-D2), inversa fiscală
cu partenerul și locul originalului (`CITIRE_FISCAL_INVERSA`) și explicația
(ieșirile = deciziile, soldul citit, FIFO, declarantul), inclusiv valoarea
liniei = decizia ieșirii, cu semnul cantității (`CITIRE_EXPLICATIE_LINIE`,
D9-D3). Explicația corecțiilor valorice pe lot are invariantul ei,
`CITIRE_EXPLICATIE_VALORICA`: fiecare postare pe lot fără cantitate și
atribuită dintr-o `Operare` are exact o decizie, cotele rezultă din ponderi,
compensarea urmează beneficiarul inversat. (CV-D8)

Scriitorul stornoului garantează proveniența inversei; `INV-CUB` verifică
numai ce scrierea nu garantează prin construcție (schema ține deja, prin
`CK_Postare_FelUnitate`, unitatea și nașterea partidei). Forma coordonatelor:
partenerul fiscal numai cu fapt fiscal; locul numai cu fapt fiscal sau pe
partidă; faptul fiscal cu loc; cantitatea nemarcată numai pe lot; lotul pe
repartitor intern; partida pe repartitor extern; locul, repartitor intern;
contraponderea transformării fără repartitor, unitate, valoare, valută sau
fapt fiscal, pe orice postare marcată, cu sau fără pereche. O bază care nu
corespunde codului se recreează, nu se repară. (098, 099, C111-D2, 102b, 102d)

Fiecare ramură fără probă proprie are un mutant într-o tranzacție anulată,
rulat o dată per profil pe prima scenă cu faptele potrivite
(`AcoperireInvarianti.cs`): `PARTIDE` (partidă fără repartitor), `IMO-CAUZA`
(fișă fără linie), `IMO-ORIGINE` (inversa unei fișe fără `InversaDin`),
`IMO-SUPORT` (transfer valoric pe fișă fără suport), `ORDINAL-BAZA`,
`ORDINAL-GOL` și `ORDINAL-DUBLU` (postările tranzacției nenumărate de la 1
fără goluri, `CITIRE_ORDINAL_INVALID`), cei cinci ai originii partidei
(`ORIGINE-LIPSA`, `-FARA-SUPORT`, `-STRAINA`, `-ORDINAL`, `-REFERINTA`, pe
`OriginiPartide`, cu `OriginiPartide.Verifica` apelat direct; TR-D9b) și cei
șapte ai explicației (X-D4): `EXPLICATIE-LIPSA`
(explicația ștearsă), `-REFERINTA` (referința mutată pe alt document),
`-STORNO` (cantitatea inversei pe lot schimbată), `-IESIRE` (valoarea unei
`ValoareIesire` +0,01), `-DECLARATA` (`ValoareDeclarata` scoasă), `-EVALUARE`
(soldul citit +1.000), `-STINGERE` (`AlocareFifo` scoase).

Mecanismul explicației și soldul citit au cinci mutanți (X-RI1, X-RI2):
`EXPLICATIE-MECANISM` (ieșirile evaluate redenumite `ValoareDeclarata`, cu
aceleași cifre și fără soldurile citite), `-DECLARANT` (aceeași substituție,
cu numele declarantului schimbat într-unul care declară valori), `-SOLD-FIFO`
(soldul citit al partidei stinse scos), `-SOLD-FIFO-MIC` (soldul citit pus
sub suma alocată) și `TRANSFER-CONT` (numai contul capătului de destinație
al unui transfer pe lot). Mutanții `INV-CUB` probează linia explicației pe
BCS, BTR și ASM cu `Transfer` (`LINIE-*`, `LINIE-SEMN-*`, `IESIRE-SEMN-*`: o
decizie cu semn opus sau o linie cu altă valoare e refuzată); mutanții
`VALORICA-*` probează explicația corecțiilor valorice. (D9-D3, CV-D8)

Perechea are mutanții ei: `PERECHE-RUPTA` (o postare a perechii fără ordinal)
și `PERECHE-LIPSA` (ambele postări fără ordinal) (D9-A2); `PERECHE-DATA` (o
postare a perechii la altă dată) ține marginea de interval a citirii
contrapartidei (C111). Mutanți separați pentru ordinal, cauză, valoare,
valută, cantitate și latură, apoi pierderea ambelor ordinale ale unei mutări
valorice/cantitative lângă o transformare, apelează direct invariantul
perechii, în tranzacții anulate, ca alt invariant să nu mascheze rezultatul
(D9-F-R1). Ținta fiecărui mutant al perechii e aleasă determinist, după `ID`.
Fiecare disjuncție a predicatului perechii are mutantul ei (documentul,
deschiderea cu ordinal, latura și sumele transferului, pe lângă cele de mai
sus). Izolarea celor cinci din urmă e probată și invers: cu disjuncția scoasă
din predicat supraviețuiește numai mutantul ei
(`nou/tools/ModelCheck/scripts/izolare-mutanti.ps1`, pasul 8 §11). Rețeta
ține blocajul verificărilor cât timp sursa invariantului e modificată.

Forma coordonatelor (C111-D2) are doisprezece mutanți. Cinci apelează direct
`VerificaCoordonate` și cer `CITIRE_COORDONATE_INVALIDE`:
`COORD-CANTITATE-NEMARCATA` (cantitate fără lot și fără contrapondere),
`COORD-LOT-NEINTERN` (lot pe repartitor extern), `COORD-PARTIDA-NEEXTERN`
(partidă pe repartitor intern), `COORD-LOC-NEINTERN` (loc pe repartitor
extern) și `COORD-TRANSFORMARE-PE-PERECHE` (perechea unei postări obișnuite,
marcată transformare, care altfel ar ieși întreagă din evidență). Patru sunt
opriți de schemă înaintea invariantului și cer `CK_Postare_BlocFiscal`:
`COORD-PARTENER-FARA-FAPT`, `COORD-LOC-FARA-FAPT` (loc pe lot),
`COORD-LOC-FARA-UNITATE` (loc pe o postare fără unitate) și
`COORD-FAPT-FARA-LOC`; ramurile corespunzătoare ale invariantului nu se pot
aprinde cât există constrângerea. `COORD-LOC-PE-PARTIDA` e martorul: locul pe
o postare cu partidă și fără fapt fiscal trece (C111-Q14).
`FISCAL-INVERSA-PARTENER` și `FISCAL-INVERSA-LOC` schimbă partenerul fiscal,
respectiv locul inversei și cer `CITIRE_FISCAL_INVERSA`. Mutanții sunt uciși
pe ambele profiluri.

Invarianții trec pe o scenă curată (SC-CIT-23); echilibrul și proveniența au
probele SC-CIT-34 (`CITIRE_CUB_DEZECHILIBRAT`) și SC-CIT-10
(`CITIRE_PROVENIENTA_LIPSA`). Acoperirea cantitativă a stocului
(`Loturi.VerificaAcoperire`, `CITIRE_ISTORIC_STOC_INCOMPLET`, X-D7 a) rulează
prima dintre acoperiri și are probele SC-CIT-103…105. La final, profilul cere
cel puțin o scenă verificată și, pe integrală, toate ramurile ucise, iar
purja nu lasă postări.

Totalul de stins al documentului se scrie din cub; taxarea inversă
(SC-FCT-10), contul explicit fără partide (SC-DEC-10) și factura cu avans
(SC-NIR-30/avans) îl probează. (102)

## Declaranții pilotului

Declaranții se probează în ModelCheck, pe ambele profiluri, prin
`ProbeNucleu.Proba` (`nou/tools/ModelCheck/Nucleu/`), chemată după operare pe
scenele e2e, pe scenele API și pe scenele proprii `VerificaNucleu*` (BCS, BTR,
FCL/DSC, FCT, trezorerie): documentul se contractează din nou prin
`Contractare.Contracteaza`, iar proba cere contractul acceptat,
`Conservare.Verifica` gol, determinismul (două contractări dau contracte
egale) și `Fapte.Operand` în cel mult `ProbeNucleu.PragInterogari` (16)
interogări. Scenele proprii își purjează documentele (altfel `PAR-V*` văd
partide în plus). `NUC-BCS-N-R3-*` fixează evaluarea pe raportul curent pe un
lot al cărui preț înghețat diferă de raport, iar `NUC-FCT-N-R4-*` taxa
documentului pe o singură linie. (TR-D6b)

## Accesul la cub (probele pe sursă)

### Cititorii cubului

Codul de producție (`Module`, `WebApi`, `Blazor.Server`) obține rânduri
`Postare`/`Tranzactie` numai prin `Cub/Citiri`. Excepțiile numite sunt
scriitorul (`Cub/Materializare*`), faptul sursei recepției conexe
(`Cub/ReceptiiConexe.cs`) și maparea EF (`BackOfficeDbContext`).
Consumatorii compun peste intrările comune (filtre, proiecții, eticheta
`Storno`), fără să refacă domeniul: rapoartele contabile, snapshot-ul
contabil, evaluarea operațională pe lot, pin/FIFO DSC, raportul de stoc,
raportul, snapshot-ul și citirile operaționale ale partidelor, cititorii
TVA/D300/D394/TaxInformation (103) și snapshot-ul de stoc. (091-r3, TR-D8)

Proba `091-r3` (`tools/ModelCheck/ProbeCititoriCub.cs`) scanează sursa
înaintea bazei, deci rulează și fără bază. Caută rădăcinile de interogare,
`ModifiedObjects.OfType`, `CreateObject`, `Set<>`, `typeof` pe `GetObjects`,
SQL pe tabelele cubului, navigarea `Tranzactie.Postari` și
`DbContext.Postari`. Pică și pe o excepție care nu mai are acces. Scanarea e
sintactică: tipul calificat cu `N.`/`Nucleu.` e al nucleului, restul contează
ca rând al cubului. ModelCheck e probă independentă și citește cubul direct
(D8-B1). (091-r3)

Consumatorii citesc din `Cub/Citiri` identități numite: titularul partidei
(`Partide.Titular`, `Partide.Nete`, `Partide.MiscariPeTitulari`,
`Partide.UnitatiAle`), terțul comercial (`PostareJurnal.TertComercial`),
locul lotului (`Loturi.PostariCitite`), repartitorul și suportul poziției
fără fișă (`Imobilizari.PozitiiCitite`) și axa analitică
(`Contabil.Analitice`). Regula identităților e în
[domeniu-si-operare](documente.md), §„Identitățile cititorilor”. (C111-D10)

### `Transfer` pe cititorii comuni

Excluderea `Transfer` și a inversei lui stă numai în
`Citiri.Contabil.Postari`, după originea `InversaDin`; `Loturi` și `Partide`
includ transferul prin contract. Proba `N-r8`
(`tools/ModelCheck/ProbeTransferCititori.cs`) cere ca fiecare intrare
publică din `Cub/Citiri` care întoarce rânduri de cub să aibă regimul
declarat. `Contabil.Postari`, `Contabil.Jurnal` și `Plati.Postari` exclud
`Transfer` și inversele lui. `Loturi.Postari` și `Partide.Postari` le includ.
`Fiscale.Postari` și `Imobilizari.PozitiiFaraFisa` nu filtrează felul:
domeniul lor e dat de coordonate, iar regula nu li se aplică. (N-r8, X-D2)

`FelTranzactie.Transfer` apare în producție numai în doisprezece membri
numiți: scriitorul (cinci membri din `Cub/Materializare*`), producătorul
`DeclarantAsamblare`, doi cititori comuni (`Loturi.VerificaRetragere`,
`Plati.Alocari`), trei martori (`Imobilizari.VerificaAcoperire`,
`Explicatii.VerificaAcoperire`, `Invarianti.VerificaTransferuri`) și eticheta
mișcării din SAF-T. Un consumator care refiltrează `Transfer` e detectat
(mutant). SC-CIT-100…102 numără, pe fiecare intrare, rândurile `Transfer` și
inversele lor din BTR stornat, din împerecherea desfăcută și din ASM-ul mixt
stornat. (N-r8)

Jurnalul și fișa de cont nu listează `Transfer` și nici inversa lui, fiindcă
pornesc din `Contabil.Postari`. O listare pe unitate (lot, partidă) pornește
din `Loturi.Postari` sau `Partide.Postari`, care le întorc pe amândouă, și
arată felul rândului din `Tranzactie.Fel`; inversa unui transfer apare ca
`Storno`. Listarea nu refiltrează felul (mutantul probei `N-r8`). SC-CIT-107
o probează pe `ContabilProiectii.RegistruJurnal`, pe `FisaCont` și pe cele
două intrări pe unitate. (N-r8, X-D7 d)

Contraponderile Transformare nu ajung prin nicio intrare comună: `Contabil`,
`Partide` și `Imobilizari.PozitiiFaraFisa` aplică
`Transformare.FaraContrapondere`, `Loturi` cere unitate, `Fiscale` cere tip de
TVA, iar `Plati` pornește din `Contabil`. SC-CIT-106 numără zero pe toate
șapte, SC-CIT-15 și SC-SAFT-10 pe rapoarte, iar SC-ASM-16 probează că
diagnosticul `Comparabil` le exclude cu numărul raportat. (T-r14)

### Regimul pe stare

Probele regimului pe stare (`tools/ModelCheck/ProbeRegim.cs`) rulează la fel
de devreme: prin reflecție, fiecare comandă `RecordEdit` a unui controller pe
`DetailView` de `Document` numește prin sufixul ID-ului o comandă din
`RegimDocument.ComenziCunoscute`; prin scanare sintactică, nicio affordance
`Poate*` din `Module/Api/` nu se calculează din `StareDocument`, niciun
controller nu poartă cheia „Stare” și niciun tip din `BusinessObjects/` nu
decide `Sterge`. (106e, 106j) Catalogul `REGIM` (`ScenariiRegim.cs`, anul
2013) rulează cu `--scenarii REGIM`.

Conformitatea regim ↔ comandă (SC-X-24) rulează în fiecare scenă:
`ScenaDocumente.Anuleaza`, `Storneaza` și `Corecteaza` citesc regimul
înaintea comenzii și numără abaterile în ambele sensuri. Un refuz lăsat
comenzii se dovedește pe starea documentului (`ProbeRegim.Clasifica`): pe
valori, pe data cerută ori pe limita 106-r7, al cărei număr îl declară scena
(`LimiteRegim`). O scenă care probează concurența sau prima instrucțiune SQL
a comenzii cheamă `ComenziDocument` direct, nu helperul. Un gardian nou de
dependență pe calea anulării sau a stornării intră ca predicat `Motiv*` în
`Motor/GardieniRetragere`; altfel SC-X-24 cade la prima scenă care îl
atinge. (106k)

### Legătura `Imperechere`

`Imperechere` se atinge în producție numai prin lista nominală din
`tools/ModelCheck/ProbeCititoriRegistre.Lista.cs`. O intrare numește
fișierul, membrul și rolul. Un membru nou într-un fișier deja permis rămâne
încălcare. Proba `X-D2` (`ProbeCititoriRegistre.cs`) citește arborele
sintactic al sursei (`SursaProductie.cs`) și numără trei feluri de utilizare:
mențiunea tipului, inclusiv prin alias `using`; numele tabelei într-un
literal; apelul unui purtător. Purtător este orice membru al cărui tip
declarat conține o colecție de rânduri de legătură, plus lista declarată
`Purtatori` pentru rezultatele netipizate (`Partide.NominalizataLibera`).
`nameof` și definiția tipului nu contează. Proba pică și pe o intrare rămasă
fără utilizare, iar mutanții îi probează detecția. (X-D2)

Lista nominală are o singură clasă, legătura: `Imperechere` este legătura
explicită, nu registru, și rămâne. Intrările numesc maparea EF
(`BackOfficeEFCoreDbContext`: `OnModelCreating` și `DbSet`-ul), lista XAF a
legăturii (`ContaUiBaseline.AscundeFkuriBrute`), `ImperechereService`,
`GardianEditare`, `MotorOperare.MotivImperecheri`,
`Partide.NominalizataLibera`, `Materializare.Imperecheaza`,
`ImperechereApply`, `ImperechereController`, `ImperecheriController` și
`PerioadeController.TipuriInsumate`. Ultima pune `Imperechere` între tipurile
însumate de verificarea închiderii (80e); cifrele din cub le păzește citirea
completă pe `Postare` (D9-D9). (X-D2)

Proba numelor interzise (`ProbeCititoriRegistre.NumeInterzise.cs`, în
`--probe-sursa`): în sursa C# și TypeScript din `nou/` nicio referință de
cod, pe identificator întreg, la cele patru entități de registru, la membrii
`PosteazaInCub` și `TotalStingere`, la `IDocumentCuRegistruPropriu`,
`StocService` și `CubDinRegistre`; excepțiile sunt fișierul probei și
`src/generated/`. Comentariile, literalii de cale (specificatorii de modul
TypeScript) și numele de raport păstrate de D9-D12 nu sunt referințe de cod;
zece mutanți fixează fiecare caz. În `Motor/`, `Cub/`, `Declaratii/`,
`Proiectii/`, `Api/`, `Culegere/`, `Saft/` și WebApi cele patru registre nu
apar deloc. (D9-D7 b)

### Blocajul scrierii

Proba `X-D6` (`ProbeBlocajScriere.cs`) ține lista nominală a membrilor care
intră sub blocaj: comenzile, cele două uși din tranzacția apelantului și
salvarea detaliilor noi. Un membru nou care cheamă
`TranzactieComanda.Incepe`/`Asigura` pică proba până e adăugat, cu rolul
lui; la fel o deschidere de tranzacție în afara `TranzactieComanda` și a
citirii declarate (`Fiscale.DeschideCitirea`) sau cheia blocajului scrisă în
alt fișier. (X-D6)

Scena `ScenariiConcurenta` (`--scenarii X`) probează pe captura SQL că
fiecare fel de comandă începe cu blocajul și că citirile nu îl iau
(SC-X-22), apoi matricea rezultatelor seriale pe două conexiuni
(SC-X-15…SC-X-21, SC-X-23). Probele concurenței sunt pe modelul serial:
SC-DES-05 (a doua deschidere așteaptă și e refuzată de domeniu; indexul unic
se probează direct în bază), SC-DES-21 (așteptarea e pe blocajul scrierii)
și SC-IMO-32 (blocajul IMO străin se ține fără blocajul scrierii). O scenă
care ține `TranzactieComanda.Incepe` pe un ObjectSpace și cheamă pe același
fir o comandă sau o salvare de detalii noi pe altul se blochează singură
până la `SCRIERE_OCUPATA`. (X-D6)

### Căutarea după cheie

Căutarea după cheie a unui tip ne-rădăcină trece prin `RandDupaCheie`
(rădăcina ierarhiei, apoi tipul verificat), niciodată prin
`GetObjectByKey<Frunza>`: cu prefetch-ul XAF, acela întoarce intrarea
urmărită fără verificarea tipului. F28-L (aceeași frază cu ținta urmărită și
neurmărită), F28-M (DVI și latura pereche) și F28-N țin regula. F28-N e o plasă
pe SURSĂ: scanează `nou/Atlas.Conta.BackOffice` și pică la orice apel
`GetObjectByKey<T>` cu `T` tip ne-rădăcină. (89i)

### Rularea probelor pe sursă

`ModelCheck --probe-sursa [--lista]` rulează numai probele pe sursă (104c-S1,
091-r3, X-D2, N-r8, X-D6, 106c/d), fără bază; `--lista` tipărește utilizările reale,
din care se actualizează lista nominală.

## Probele pe modelul aplicației

ModelCheck verifică modelul și execută scenarii de integrare, inclusiv probe
pure pe funcțiile de potrivire și de seed. `ContaSeeder.SeedTipuriDocument`
este public, ca proba seed-ului să-l poată chema nesalvat. (X-D7)

Probele mapării TPH (`F28-A…K`) țin: seed-ul `TipDocument` ↔ clasele
concrete, 1:1; coloanele fără prefix de tip și schema bazei egală cu modelul;
indexul pe `ClrType`; refuzul gardianului pe un FK spre frunză cu ținta de
alt tip; cititorul de tip egal cu clasa reală pe toate tipurile; `ClrType`
completat de EF și nescriibil din cod; `ClrType` read-only în modelul
aplicației, absent din layout-ul oricărui DetailView și coloană vizibilă
exact pe listele care amestecă tipuri; liniile unui document de tipul
declarat de el; ținta fiecărui FK spre frunză de tipul corect; coloanele
frunzelor NULL pe rândurile altor tipuri. Ultimele trei rulează SQL generat
din metadata EF (`IntegritateTph.cs`), iar
`ModelCheck --dump-integritate-tph <cale.sql>` scrie același SQL pentru a fi
rulat pe o bază de import, pe care ModelCheck nu o atinge. (89e, 89h)

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

Probele axei 2 (`tools/ModelCheck/ProbeLinii.cs`) rulează pe modelul XAF
real, în rularea integrală, cu registrul baseline-ului construit pe loc:
fiecare grilă tipizată de linii începe cu coloanele rolurilor în ordinea
vocabularului și ascunde rolurile nepurtate; fiecare membru `ReadOnly` e
blocat pe grilă și pe dialogul liniei; pe fiecare tip cu detaliu propriu care
calculează `Valoare` (`BazaLinie` suprascris, `Valoare` nu e intrare),
`Valoare` e `ReadOnly` sau o regulă `[Appearance]` o dezactivează pe linia
pe care `BazaLinie` o calculează (FCT: linia obișnuită; linia de corecție o
culege, CV-D1). (106h-1…3)

## Nucleul pur

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

## Probele HTTP și autorizarea

ModelCheck nu are strategie de securitate XAF; autorizarea se probează prin
`nou/tools/ProbeHttp/refuzuri.ps1`, cu rolurile Admin, Cititor, User și
Configurator. (80i, 81j, 84c) Matricea generală `refuzuri.ps1` își creează
singură subiecții. (104-r5)

Proba HTTP `nou/tools/ProbeHttp/citiri-cub.py` verifică accesul pe rând și
membru înainte/după închidere și reconstrucție, pe host privat cu bază
izolată. Tokenurile rămân în memorie; fixture-ul și rolurile temporare se
curăță în finally.

`nou/tools/ProbeHttp/partide-cub.py` oferă proba HTTP a stingerilor și
opțiunea `--prin-xaf`, care așteaptă cel mult cinci minute acțiunea din
browser. SC-CIT-46/48 trec și prin HTTP și prin acțiunea XAF, cu verificarea
numerică a cubului. (100)

Proba HTTP `nou/tools/ProbeHttp/explicatii.py` rulează pe o clonă de unică
folosință a unei baze private cu seed: bonul rămâne stornat, deci baza nu se
refolosește (rețeta: `run-verificari/x2-expl-http.ps1`). (X-D4)

`verifica.ps1 -Suita D9bHttp -Profil Privat -Sufix <sufix>` pornește hostul
izolat pe 5093, baza `Atlas.Conta.Http.Privat<sufix>` pe 5446, seed privat și
utilizatorii reali; rulează matricea `refuzuri.ps1 -Baza` completă și
`origini-partide.py`. Condițiile sunt în
[raportul originilor](../nucleu/tr-d9b-pas4-origini.md). (TR-D9b)

## Comparația A/B a cititorilor

Comparația cititorilor de partide și sold (`-Suita D9bComparatie`,
`D9bScriere`, `D9bRaport`) rulează secvențial, pe o bază B nouă; ambele
variante se măsoară pe aceeași bază. Seria de sistem rulează pe ușa
nesecurizată a hosturilor. SOLDURI nu are prag: acolo A și B rulează același
SQL, deci harness-ul măsoară perechea și compară rezultatele; pragurile rămân
pe perechile ORIGINI și REST. (D9B-D7) Protocolul, artefactele și limitele
sunt în [raportul comparației](../nucleu/tr-d9b-pas5-comparatie.md) și în
[raportul redenumirii `Sens`](../nucleu/tr-d9b-pas6-sens.md).

## Scara transversală de perf (`scripts/perf-cub.ps1`)

Integrala nu rulează scala: după o schimbare de regulă, scena ei se verifică
separat. Comparația cu regimul dual e în
`docs/nucleu/tr-d9-pas8-inchiderea.md` §3.

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
Rețeta rulează ambele profiluri pe instanța nativă (5446), pe bazele cu
sufixul dat (`-PregatesteBaze` le clonează din cele de dezvoltare), și
validează XML-urile SAF-T cu DUK; parametrii `-Profil`, `-Istoric`, `-Trepte`
și `-Operatii` restrâng rularea. Comenzile de scriere se cronometrează la
construcția scenei, deci apar și într-o rulare restrânsă la un singur
cititor. Scara completă este o rulare grea: nu se suprapune cu alta pe
aceleași baze. O rulare
întreruptă lasă scena în bază; următoarea o purjează la pornire, dar datele
societății de pe profilul privat rămân cele ale scenei și se refac de mână.
Un cititor nou care citește cumulat sau pe interval intră în `PerfCub.Operatii`
cu ruta și cifrele lui de control. Cronologia corecțiilor de valoare are
operația ei, `CRON-N`: dry-run-ul unui consum antedatat pe lotul lung, ca
`CorectiiValorice.VerificaCronologia` să caute pe intervalul cu volum
(CV-D5). (X-D5, X-D3)

## Scara de volum (`scripts/scara-volum.ps1`)

`ModelCheck --scara-volum privat` păstrează scena scării transversale la
m = 12, k = 64 și o multiplică în lățime, direct în SQL, pe treptele ×1, ×10,
×100 și cea care trece de 5 milioane de postări. Fiecare copie are
identități proprii pentru tranzacții, postări, documente, loturi și
partenerii scenei (cei purtați de `Repartitor` sau de `PartenerFiscal`);
partidele își primesc identitatea prin `cub_partida_id`. Postările de
deschidere clonate rămân pe tranzacția unică de deschidere și își continuă
ordinalele.
La fiecare treaptă rulează probele de corectitudine (numărători și sume
egale cu factorul × scena, originile partidelor recunoscute de cititorul
produsului), reconstrucția snapshot-urilor pe calea produsului și măsurarea
cititorilor comuni, rece și cald, pe ruta securizată și pe cea de sistem,
cu planurile. O măsurare se oprește la 10 minute, reconstrucția la 30. E
raport, fără prag. Rețeta rulează pe instanța nativă (5446), pe o clonă cu
sufix (`-PregatesteBaze` o face din baza privată de dezvoltare),
care rămâne bază de citire: liniile de document, registrele și împerecherile
nu se multiplică. Rularea completă e rulare grea; treapta finală se rulează
singură cu `-Factori F`. Rezultatele și limitele scării:
`docs/nucleu/tr-d9-pas5b-probe.md` §3. (D9-A1) Scriptul numește
explicit coloanele fiecărei tabele clonate; proba `SCARA-COLOANE`, care
rulează în orice ModelCheck, le compară cu coloanele scrise ale bazei, ca o
coloană nouă să pice acolo, nu la următoarea rulare a scării. (C111)

Un singur cititor se re-măsoară pe baza scării fără a o reface: procesul-copil
`--perf-cub-masura` sau `--scara-volum-reconstructie`, cu
`MODELCHECK_BAZA_SUFIX` pe sufixul bazei, pe aceeași instanță ca rețeta.

## Amprentele unei baze (`--amprente`)

`ModelCheck --amprente <bază> <an> <lună> <director>` scrie sumele unei baze
pe chei naturale, ca două baze cu identificatori diferiți să se poată compara
la `diff`: balanța pe cont × lună, stocul pe cont × produs × gestiune × lună,
partidele pe cont × titular × data deschiderii, D300 și D394 pe luna cerută,
plus trei secțiuni analitice, citite direct din coloanele postării: cont ×
repartitor de sold (`Repartitor`), cont × partener fiscal × loc
(`PartenerFiscal`, `Loc`), cont × contrapondere (`Contrapondere`). Gestiunea
stocului și titularul partidei sunt tot `Repartitor`.
`amprente.txt` ține numărul de rânduri și SHA-256 pe fiecare secțiune. Numai
citire; rulează pe orice bază de pe instanța uneltelor, inclusiv pe una de
import. Două importuri ale aceluiași binar dau amprente identice
(`docs/nucleu/c111-pas0-linia-de-baza.md`). (C111-D9)

`ModelCheck --perf-fisa <bază> <simbol cont> <an> <lună> <director>` măsoară
fișa contului pe o bază numită — contul întreg, contul pe repartitorul lui
cel mai încărcat și contul fără terț — și registrul jurnal al lunii, citite
integral: cinci rulări la cald, mediana, cu numărul de rânduri și câte au
contrapartidă. Scrie în director SQL-ul produsului, cu parametrii în antet.
Numai citire. (C111 S9, `docs/nucleu/c111-pas6-import.md`,
`docs/nucleu/c111-pas7-client-si-inchidere.md`)

## Verificarea importului

Rețeta verificării importului este `run-nucleu/tr-d7a/import/run.ps1`:
Import1C integral (`--recreeaza --cititori --inchide-lunile`), apoi
`--reclasifica`, `--reconciliere-cub`, `--dump-integritate-tph` și
`diff-sortat.py`, care compară raportul de reconciliere cu baseline-ul pe
conținut sortat. Rețeta aparține migrării; nu se execută pentru validarea unei
felii de motor. (091)

## Driftul contractelor

Verificarea de drift regenerează contractele și refuză diferențele față de
fișierele versionate. O schimbare intenționată de contract se regenerează și
se examinează înainte de includerea artefactelor în modificare. (43d, 56)

## Documentația

### Forma documentației

Forma documentației urmează kitul de guvernanță
(`~/.claude/PROJECT_GOVERNANCE.md`), aplicat prin decizia 118 pe structura
existentă: invarianți, stare curentă pe responsabilități, decizii numerotate
cu „Regula durabilă", restanțe cu nume. Textele vechi se aduc la formă la
atingere sau prin script, nu prin rescriere în masă. (118a)

Antetul unei decizii are câmpurile `Data`, `Stare`, `Execuție`, `Arii`,
`Amendează`, `Închide`, `Docs`, câte o linie; `Stare` e `propusă`, `activă`,
`amendată de N` sau `depășită de N`, iar aprobarea, PR-ul și review-ul stau
în `Execuție`; titlul e `# N — afirmația`. (118b) Un sub-punct amendat sau
depășit poartă la început marcajul `[amendat de N]` sau `[depășit de N]`, cu
textul păstrat; `Amendează: N (x)` în antetul lui M și marcajul din N (x)
sunt cele două capete ale aceleiași legături. (118c) Decizia depășită
integral primește banner și stă în `decizii/arhiva/`. (118d)

Termenii stau în [termeni.md](termeni.md), pe trei secțiuni: domeniu,
tehnic, identificatori; un termen intră acolo la prima lui folosire într-o
pagină de stare curentă, în același commit. (118g)

Fișierele din afara `decizii/` și `stare-curenta/` declară
`Tip: contract | analiza | review | catalog | note`; fără `Tip` sunt jurnal,
cu linkurile verificate și fără alte cerințe. (118h) Restanțele au și
categoria `docs`; `închisă prin N` e singura stare de închidere. (118i)

### Vederea generată

Vederea generată o scrie `docskit gen`, din rădăcina repo-ului, în același
commit cu sursa: indexul din `decizii/README.md` între markerii `gen:index`,
în trei grupe (118e); blocul `gen:decizii` de la sfârșitul fiecărei pagini de
aici, cu deciziile care o numesc în `Arii` (118f); lista fișierelor din
`docs/README.md` între `gen:fisiere`, doar cu titluri; și `docs/stare.md`
integral (rezumat cu cifre, deciziile în vigoare pe arie ca numere cu starea,
depășite, restanțe deschise pe stare, felii deschise, decizii invocate din
cod, paginile cu `Actualizat`). Generatul e ținut mic pentru contextul
agentului: `stare.md` sub 10 KB, blocul paginii o linie de numere, titlurile
din index tăiate la 120 de caractere; titlul întreg stă doar în fișierul
deciziei. `stare.md` nu poartă data generării și nici date din git, ca a doua
rulare să scrie zero. `python -X utf8 nou/tools/docs/normalizeaza-antete.py --scrie`
rămâne doar pentru normalizarea antetelor vechi; nu scrie indexul. (118j)

### Gardienii documentației (`docskit check`)

Gardienii mecanici îi rulează `docskit check` (unealta kitului, dotnet global
tool din repo-ul `DocsKit`, configurată în `docs/docskit.json`): `linkuri`
(fiecare link relativ și fiecare ancoră, pe slug GitHub cu diacritice),
`antete` (câmpurile per tip, vocabularul `Stare` și `Tip`), `generat`
(vederea generată la zi), `trimiteri` (cele două capete ale amendării și ale
depășirii), `restante` (`Închide` ⇔ `închisă prin`, corp la sursă, stări din
vocabular, lista `activă` sub 45), `arii` (fiecare rând al registrului
numește o arie declarată, 118l), `cod` (`// 42b` are decizia și
sub-punctul), `cai` (căile în backtick cu rădăcină `nou/`, `docs/`, `db/`,
`legacy/` există, doar în stratul curent), `actualizat` (`Actualizat` ≥
ultima schimbare a textului scris de mână al paginii; blocul generat nu
contează, deci regenerarea nu cere dată nouă), `orfane` (stratul curent e legat), `termeni`
(ancore unice în glosar), `tabele` (fiecare rând `| … |` stă sub antet și
separator, fără linie goală între rânduri; rândul rămas în afară se randează
ca text). Un gardian intră la `eroare` când are zero constatări; nivelul
fiecăruia stă în `docs/docskit.json`, iar cifrele celor rămași la
`avertisment` în 118-r1. `check`
și `metrics --check` verzi sunt condiție a PR-ului, rulate local, cu ieșirile
atașate; nu există CI și nici hook de sesiune. Citările din cod și metricile sar folderele `bin`,
`obj`, `node_modules`, `.vs`, `Migrations` și `generated` (ieșirea
codegen-ului OpenAPI). (118k)

### Metricile codului (`docskit metrics`)

Forma codului o măsoară `docskit metrics`, pe `nou/`: C# cu ținta 400 și
plafonul 600 de linii, TypeScript cu 300 și 500, plus comentariile cu marcaje
narative. Doar din metrici ies `nou/tools` (ModelCheck, Import1C), fișierul
generat `UnitatiMasuraUnEce.cs` și maparea EF `BackOfficeDbContext.cs`
(`cod.excluseDinMetrici`); citările `// N` din ele rămân verificate de
gardianul `cod`. Linia zero e `docs/docskit.baseline.json` și ține numărul
de linii al fiecărui fișier peste plafon. (118k)

`docskit metrics --check` pică la regres și la linie învechită. Regres: un
fișier nou peste plafon, un fișier deja peste plafon care a crescut cu mai
mult de toleranța limbajului față de liniile înregistrate, mai multe
comentarii narative. Linie învechită (îmbunătățire neînregistrată): un fișier
coborât sub plafon, unul rămas peste dar scăzut cu mai mult de toleranță, mai
puține comentarii narative. Toleranța e de 30 de linii pe ambele limbaje
(`cod.limbaje.*.toleranta`) și se măsoară de la linia zero, nu de la ultima
rulare, deci creșterile mici se adună până o depășesc; plafonul rămâne fix,
iar sub plafon un fișier crește liber. Linia se rescrie cu `docskit metrics
--scrie`, care mută și centrul benzii; un regres se repară în cod, nu se
înregistrează, iar PR-ul numește ce a intrat în linie. Limite: `build` (HTML static) există în unealtă, dar nu e adoptat
aici (118-r2); `/docs-curatenie` și `docs-init` vin cu 118-r4. (118k)

## Limitele implementării

- Ramurile „partener fiscal fără fapt”, „loc fără temei” și „fapt fără loc”
  ale lui `VerificaCoordonate` nu se pot aprinde cât există
  `CK_Postare_BlocFiscal`: le ține constrângerea, nu invariantul. Stornoul cu
  doi originali cu parteneri fiscali diferiți nu e produs de declaranții de
  azi; forma nu e probată, iar invariantul 5 îl țin mutanții
  `FISCAL-INVERSA-*`. Invariantul 5 compară partenerul fiscal și locul
  inversei numai pe postările cu fapt fiscal: locul unei inverse pe partidă
  fără fapt fiscal e păstrat de copierea stornoului, nu verificat de
  `INV-CUB` (C111-Q14). Felul necunoscut al unui repartitor e probat numai la
  recitire. (C111-D2)
- Nu există un al doilea scriitor cu care cubul să fie comparat:
  corectitudinea valorică o poartă catalogul de scenarii și invarianții
  interni ai cubului (echilibrul, conservarea transferului, explicația, taxa
  și valoarea liniei). Conservarea singură nu dovedește cifra. (D9-D7 a)
- `RAMURA-NUL` prinde egalitățile pe coloane nulabile în tot SQL-ul pe care
  EF îl emite într-o rulare ModelCheck, deci numai pe citirile pe care o
  scenă le execută: un cititor neatins de nicio scenă nu e văzut. Proba
  citește forma SQL-ului, nu planul: cele cinci forme admise nominal sunt
  admise pe cheia selectivă de lângă ele, nu pe o cifră la volum. Un SQL scris
  de mână cu `IS NOT DISTINCT FROM` nu e căutat.
- Scara transversală e sintetică și mică și n-are prag absolut. Proba din
  plan se evaluează cu scanarea secvențială interzisă, fiindcă la acest volum
  planificatorul o alege legitim; planul ales e raportat alături. Pragul pe
  volum real rămâne al migrării. (X-D5, FZ-r3)
- Scara de volum e tot sintetică: scena scării transversale multiplicată în
  lățime, citită de utilizatorul administrativ, pe Postgres neconfigurat. Nu
  măsoară scrierea, închiderea de lună, cititorii fiscali și SAF-T, nici
  filtrele de rând ale unui rol restrâns. (D9-A1)
- Proba X-D2 e sintactică: un rezultat netipizat derivat din legătura
  `Imperechere` se urmărește la apelanți numai dacă membrul e declarat în
  `Purtatori`; un
  purtător nou, nedeclarat, nu e văzut. (X-D2)
- Probele de concurență pun cele două comenzi în coada blocajului ținut de
  scenă și le lasă să ruleze în ordinea cozii: dovedesc rezultatul serial și
  așteptarea pe conexiuni distincte, nu măsoară debitul. Pe ușa HTTP și pe
  contextul securizat e probat numai refuzul `SCRIERE_OCUPATA` și ce nu ia
  blocajul (`scriere-ocupata.py`), nu matricea rezultatelor seriale. Nici
  nominalizarea concurentă nu are debit măsurat. (X-D6)
- `DECLARATIE_INVALIDA` e probată pe coaja comenzii, cu o cantitate în afara
  scării ținută în memorie; un declanșator persistabil pe ușa HTTP nu există,
  iar răspunsul ușii pentru acest cod nu e probat separat. (S-r11)
- Invariantul explicației nu recitește istoricul: verifică soldul citit al
  unei partide stinse numai ca prezență și ca plafon al alocărilor, deci un
  sold citit alterat în sus trece. O valoare declarată e comparată cu
  postarea ei și cu sursa permisă declarantului, nu recalculată din linia sau
  recepția de origine. (X-RI1)
- Matricea `refuzuri.ps1` probează ușa explicației numai cu cei patru
  utilizatori ai ei (200 și 404). 403 `EXPLICATIE_ACCES_INCOMPLET` cere un rol
  cu citire restricționată și rămâne al probei `explicatii.py`, care lasă un
  bon stornat pe baza ei. (X-D8)
- Refuzurile asupra navigațiilor OData expandate nu au acoperire completă prin
  probe HTTP pe roluri restrânse. (80-r4)
- Rândul „fără terț” al soldurilor pe parteneri e probat cu sume pe
  repartitor intern și pe cont propriu, nu și cu sume fără niciun repartitor.
  Coloanele repartitor, partener fiscal, loc și contrapondere ale listei
  `PostareVizual` sunt probate în ModelCheck (`STR-VIZUAL-*`), nu în browser.
  (C111-Q13, D9-A12)
- Un document operat înaintea migrației explicației n-ar avea explicație, iar
  invariantul l-ar refuza: baza se recreează. (X-D4, 102b)

<!-- gen:decizii -->
## Decizii

Deciziile care numesc pagina în `Arii:` (32; titlurile în [index](../decizii/README.md); generat de `docskit gen`, nu se editează): 1, 2, 3 (amendată de 89), 4, 6, 7, 8, 14, 17, 23, 24, 28, 30, 43 (amendată de 104), 50, 56 (amendată de 77, 81), 66, 70, 73 (amendată de 74, 105), 80, 81, 84, 85, 89, 90 (amendată de 91, 92, 93, 95, 96, 110, 119), 91 (amendată de 107, 118, 120), 102, 104 (amendată de 106), 106, 118 (amendată de 121), 120, 121.
<!-- /gen:decizii -->
