# SAF-B8 — review advers Codex

## Corecție RV1.2 aplicată de Codex (2026-09-30)

**B8-RV1.2 rezolvată; toate constatările B8 sunt închise.** Owner-ul a cerut
aplicarea corecției. Ambele ramuri de factură au acum martor pe sursa reală;
21 de probe adverse sunt respinse, iar raportul păstrează cele 111 diferențe
legitime, toate clasificate. Integrala pe ambele rute este verde:
**3.269 / 4.521 OK**, zero FAIL. Codul este reverificat de Claude și comis pe `tr-d8-saft-ab-rv1`.
[Implementarea, comenzile și limitele](tr-d8-saft-b8-rv12-corectie.md).

## Reverificare pe `b3272de`, clasificator `3a4372d` (2026-09-30)

**B8-RV1.1 este închis. B8-RV1 rămâne deschis prin B8-RV1.2 / P2,
pe cele două ramuri SAF-B5 ale facturilor. B8-RV2 și B8-RV3 rămân închise.**
Codul de producție și ModelCheck de pe branch-ul curent sunt identice
cu `37ac61c`; schimbarea executabilă revizuită este clasificatorul istoric
`SaftAb.cs` de la `3a4372d`.

### RV1.1 închis — martorul pe document respinge redistribuirea

Am executat clasificatorul nemodificat pe copii ale exportului scenei S3,
cu documente și postări reale:

- controlul NIR 10/100 → FCT 10/100 trece;
- mutantul FCT 9/90 + NIR 1/10 este respins când cubul cere FCT 10/100 și NIR zero;
- controlul GL mutat integral de pe NIR pe FCT trece;
- mutantul GL care mută câte 1 pe debit și credit înapoi pe NIR este respins,
  deși păstrează atât totalul perechii, cât și echilibrul fiecărui document;
- codul 10 → 80 și ASM −3,32 când cubul cere −3,34 rămân respinse.

`MiscareaCubului` și `GlulCubului` leagă acum rezultatul nou de postările
fiecărui document, suplimentar totalului perechii. Contraexemplul RV1.1
nu mai trece.

### B8-RV1.2 / P2 — facturile SAF-B5 acceptă sume fără suport în sursă

Loc: `nou/tools/ModelCheck/SaftAb.cs` **la `3a4372d`**, metoda
`Clasificator.Factura`, condițiile de la **237–243** și **260–264**.
Prima ramură cere perechea FCT/NIR, existența registrului pe conex, data
rădăcinii și totaluri interne coerente. A doua păstrează liniile vechi și
cere ca diferențele de net/brut să fie suma liniilor adăugate. Niciuna nu
leagă valorile acceptate din factura nouă de postările/faptele documentului.

**Da, clasa facturii semnalată în predarea lui Claude este aceeași familie
de defecte ca RV1/RV1.1:** o condiție necesară pentru explicație este tratată
ca dovadă suficientă pentru valori arbitrare din DTO. Nu este suficient ca
GL-ul sau mișcarea altui rând comparat să aibă martor corect.

Primul contraexemplu folosește factura reală DES, `E2E-SC-DES-1`, 5 × 10,
fără TVA. Cubul are debit 50 pe contul de stoc. Copia veche omite doar
factura, situație prezentă și în raportul A/B arhivat. Controlul nou are
o linie 50, net/brut 50 și trece. Mutantul păstrează identitatea, data,
cantitatea și TVA zero, dar pune **linia/netul/brutul la 500**. Comparatorul
îl acceptă: **o diferență SAF-B5, zero perechi neînchise**, cubul rămânând 50.
Proba `REVIEW-B8-RV12-FACTURA` eșuează exact pe refuzul așteptat.

Al doilea contraexemplu folosește factura reală D16-V2,
`E2E-SAFT-FCT-EUR`: net 200, brut 242, cu linia 628 de 100/TVA 21 și
linia 371 de 100/TVA 21. Copia veche păstrează doar 628, net 100/brut 121,
ca în raportul arhivat. Controlul trece. În copia nouă mutantă, linia 371
devine **500/TVA 105**, netul **600**, brutul **726**, iar 628 rămâne intactă.
Cubul are în continuare debit 100 pe 371. Și acest mutant este acceptat:
**o diferență SAF-B5, zero perechi neînchise**.
`REVIEW-B8-RV12-EXTRA` eșuează pe refuzul așteptat; martorul sursei și
controlul corect trec. TVA rămâne 21%, deci nici coerența cotei nu închide
lacuna. Cele două ramuri necesită aceeași corecție de fond.

**Condiția de închidere:** legați factura nouă și liniile adăugate de
sursele reale ale documentului/evenimentului, conform regulii de proiecție
(cont, cantitate, bază și taxă), cu verificarea câmpurilor care nu trebuie
să se schimbe. Egalitățile interne pot rămâne verificări suplimentare.
Păstrați controalele corecte și adăugați mutanți pentru ambele ramuri;
reverificați apoi cele 111 diferențe pe starea cu ambele rute.

Aceasta este o lacună a certificării A/B, nu dovada că exportul de producție
emite facturile inventate. Probele modifică numai copii ale DTO-urilor;
nu schimbă postările și nu reintroduc ruta veche în produs.

### Verificările acestei reverificări

Proba inițială: `verifica.ps1 -Suita Scenarii -Tip SAFT,DESCHIDERE
-Profil Ambele -Sufix .CodexSaftS3R`,
`run-verificari/20260930-231518-978`: exact un FAIL intenționat,
`REVIEW-B8-RV12-FACTURA`. Controalele și mutanții RV1.1 trec.
Clasificatorul a fost copiat nemodificat din `3a4372d`; scriptul și patch-ul
sunt în `run-verificari/saft-b8-rv11-review/advers.py`, respectiv `advers.patch`.

Proba celeilalte ramuri: `verifica.ps1 -Suita Scenarii -Tip SAFT
-Profil Privat -Sufix .CodexSaftS3R`,
`run-verificari/20260930-231922-568`: exact un FAIL intenționat,
`REVIEW-B8-RV12-EXTRA`, controalele trecute. Scriptul suplimentar este
`saft-b8-rv11-review/extra.py`, iar patch-ul `extra.patch`, în `run-verificari/`.

După restaurarea byte-identică a celor trei fișiere și eliminarea helperului,
`verifica.ps1 -Suita Scenarii -Tip SAFT,DESCHIDERE -Profil Ambele
-Sufix .CodexSaftS3R` este verde: **155 bugetar / 397 privat OK**, zero FAIL,
exit 0, `run-verificari/20260930-232243-624`. DLL SHA-256:
`6E25C65C545A658FD8B506B68EC651AA31FC7F23DFAEED42E58C74DDE5D79B8E`.
`git diff -- nou` este gol. XSD-ul, cele cinci JAR-uri și nomenclatorul
păstrează hash-urile review-ului anterior (`saft-b8-rv11-review/pinuri-dupa.json`).
Predarea modifică numai documentația; `tmp/` preexistent este neatins.
Fără commit.

Integrala istorică a autorului (`20260930-230311-066`, 3.269 / 4.500) și
raportul regenerat au fost inspectate; proba proprie este izolată, nu o
nouă execuție integrală a rutei vechi. Integrala proprie anterioară pe codul
principal neschimbat rămâne `20260930-221831-487`, 3.269 / 4.477 OK.
Nu am repetat perf, HTTP sau browserul pentru schimbarea exclusivă a
clasificatorului istoric. SAFT-r4/r5 și gate-ul transversal rămân deschise.

## Reverificare pe `37ac61c` (2026-09-30)

**B8-RV2 și B8-RV3 sunt închise. B8-RV1 rămâne deschis prin
B8-RV1.1 / P2.** Cele două contraexemple inițiale A/B (cod 10 → 80 și
ASM −3,32 în loc de −3,34) sunt acum respinse. Clasificatorul întărit de
la `ddcac79` acceptă însă o redistribuire nejustificată a recepției între
FCT și NIR, dacă totalul perechii rămâne egal.

### B8-RV1.1 / P2 — totalul perechii nu dovedește delta reală a NIR-ului

Loc: `nou/tools/ModelCheck/SaftAb.cs` la **`ddcac79`**, `Miscare`, liniile
314–325, în special condiția `delta` de la 317. Aceasta cere numai ca
documentul să fie conexul și să existe linii în ambele DTO-uri. Urmează
cumularea Q/V pe pereche, lot × gestiune × storno × cod. Verificarea exactă
`MiscareaCubului(...) == n` este folosită la 329 numai pentru ASM.

Contraexemplul folosește recepția reală 10 × 10 din scena S3 (SC-SAFT-07).
Registrul NIR poartă 10/100; cubul poartă recepția 10/100 pe FCT și nicio
postare pe lot pe NIR-ul egal. Pentru verificarea clasificatorului am
construit două copii controlate ale DTO-ului, păstrând celelalte secțiuni:

| Variantă | FCT Q/V | NIR Q/V | Verdict A/B |
|---|---|---|---|
| vechi, poziția recepției din registru | absent | 10/100 | referință |
| nou, control corect | 10/100 | absent | acceptat: 2 diferențe SAF-B5, 0 perechi neînchise |
| nou, mutant | **9/90** | **1/10** | **acceptat:** FCT = SAF-B5, NIR = S3-R1, 0 perechi neînchise |

Codul rămâne 10, lotul, gestiunea și stornoul rămân aceleași; totalul
mișcărilor și PhysicalStock rămân neschimbate. NIR-ul mutant are referință
distinctă. El inventează totuși o mișcare pe un document care n-a produs-o
și micșorează recepția FCT. Acest lucru nu este justificat de SAF-B5 sau
S3-R1. Condițiile noi privind poziția și codul nu îl resping.

Proba execută `LunaNoua`, `Compara`, `Clasa` și `PerechiNeinchise` din
clasificatorul `ddcac79`, fără modificarea acestuia. Este o probă izolată a
martorului pe documente reale, nu o nouă rulare completă a exportului vechi
și nici afirmația că exportul de producție generează acest mutant.

**Închidere:** martorul SAF-B5/S3-R1 trebuie să lege Q/V nou de postările
reale ale fiecărui document × eveniment × lot × gestiune din lună, inclusiv
absența mișcării dacă NIR-ul are delta zero. Egalitatea cumulată poate
rămâne o verificare suplimentară. Adăugați mutantul 9/90 + 1/10, păstrați
controlul 10/100 + absent și reverificați raportul A/B pe starea cu ambele
rute. Corecția poate reutiliza martorul exact deja introdus pentru ASM.

### B8-RV2 închis — oracol independent și comparație în ambele sensuri

`OracolStocFizic` își determină domeniul din postări și categoria efectivă
a contului, independent de export. SC-SAFT-50 trece pe două conturi în
lunile 1–3. Omiterea contului 371 păstrează cele 22 de postări în oracolul
din ianuarie și produce exact 3 chei lipsă, cu contul 371 numit.

Am extins verificarea cu șase mutanți în fiecare dintre cele trei luni:
toate pozițiile omise; contul 371 omis; poziție duplicată; poziție omisă
și înlocuită cu un duplicat; poziție străină cu sold zero; ClosingValue +1.
**Toți cei 18 mutanți sunt respinși**, iar cele trei DTO-uri originale trec,
inclusiv martie fără mișcări. SC-SAFT-51 (coerența L↔S) trece în integrală.

### B8-RV3 închis — reproducere în condițiile contractului amendat

Rulare proprie: `run-verificari/perf-saft-20260930-222637`, baza
`Atlas.Conta.ModelCheck.Privat.CodexSaftS3R`, scriptul
`nou/tools/ModelCheck/scripts/perf-saft-container.ps1 -Sufix .CodexSaftS3R`.
Perf și DUK au exit 0. Auditul artefactelor confirmă:

- toate cele **48 de măsurători**: k={1,4,16,64} × m={0,6,12} × L/S × rece/cald;
- procese noi pentru fiecare punct/modul, a doua rulare caldă, working set
  măsurat, ASM în unitatea de volum;
- 43 comenzi SQL pe L și 26 pe S, constante pe întreaga matrice;
- criteriile inițiale de durată totală și alocări, toleranță 25%, trecute
  pe toate scările și pe ambele faze;
- saltul maxim de timp la cald, din tabelul rotunjit: L 58→83 ms (×1,43),
  S 26→41 ms (×1,58), sub limita ×5;
- șase seturi de planuri la k=64, inclusiv m=12; șase XML-uri calde k=64
  acceptate de DUK J2.2.18 fără atenționări; XSD valid în măsurări.

**Judecata condițiilor.** Topologia B8-RV3-P este legitimă pentru această
măsurare: elimină proxy-ul local documentat și păstrează durata totală,
inclusiv SQL. Nu certifică latența căii Windows → portul publicat de Docker.
`ANALYZE` este o precondiție explicit aprobată prin B8-RV3-A; nu schimbă
pragurile, iar starea fără statistici actualizate rămâne delimitată prin
SAFT-r5. SAFT-r4/r5 și gate-ul transversal rămân deschise. Rularea proprie
confirmă rezultatul în aceste condiții, fără relaxări suplimentare.

### Verificări ale reverificării

- Integrala proprie prin `verifica.ps1 -Suita Integral -Profil Ambele
  -Sufix .CodexSaftS3R`: **3.269 / 4.477 OK**, zero FAIL, exit 0,
  `run-verificari/20260930-221831-487`.
- Proba adversă finală prin `verifica.ps1 -Suita Scenarii -Tip SAFT
  -Profil Privat -Sufix .CodexSaftS3R`,
  `run-verificari/20260930-223442-520`: exact **un FAIL așteptat**,
  `REVIEW-B8-RV-SPLIT`. Controlul direct al sursei pe lot, perechea corectă,
  respingerea codului și a Δ ASM trec. Cele 18 mutații D18 sunt respinse.
  Scriptul reproductibil și patch-ul sunt `saft-b8-rv-review/advers.py`
  și `advers.patch`; helperul nemodificat provine din
  `git show ddcac79:nou/tools/ModelCheck/SaftAb.cs`.
- După eliminarea probelor temporare, `verifica.ps1 -Suita Scenarii
  -Tip SAFT -Profil Ambele -Sufix .CodexSaftS3R`: **42 / 281 OK**, zero
  FAIL, exit 0, `run-verificari/20260930-223705-537`. DLL-ul revine exact
  la hash-ul integralei și al perf-ului; `git diff -- nou` este gol.
- Perf a rulat secvențial după integrală, sub mutex-ul verificărilor, cu
  același DLL: `870149840E72533CC2918EA15C0CC8CC6C4F2254FCCEBF2C1C879312EC9DEBB7`.
  Scriptul de lansare, auditul matricei și logurile sunt în
  `run-verificari/saft-b8-rv-review/`.
- XSD-ul, cele cinci JAR-uri și nomenclatorul restaurate după incident
  corespund pinurilor review-ului anterior (`pinuri-restaurate.json`) și
  rămân identice după toate rulările (`pinuri-dupa.json`).
- Nu am repetat browserul și suita HTTP: corecția `37ac61c` atinge harness-ul
  și documentația, fără schimbări de cod de producție față de review-ul B8.
  Raportul A/B integral regenerat de autor a fost inspectat; reverificarea
  proprie a clasificatorului este proba izolată descrisă mai sus.

Predarea modifică numai documentația. Directorul neversionat `tmp/` exista
la începutul reverificării și nu a fost modificat de aceasta. Fără commit.

## Istoricul review-ului inițial pe `8172f93`

Data: 2026-09-30. Bază: `3461636`; HEAD revizuit: `8172f93d3600077a199a0728d334f8679ebc06a3`
(`tr-d8-saft-b8`). Contract: [B8-D1…D8](tr-d8-saft-contract.md#b8--gate-ul-final-saf-t-contract-pentru-aprobare-2026-09-30).

**Verdict: deschis, trei constatări P2.** Integrala curentă trece. Două
contraexemple executate arată că martorii clasificării A/B sunt prea largi;
un al treilea arată pierderea independenței recalculului D18-V1. Măsurarea
perf nu acoperă experimentul aprobat și schimbă criteriile după măsurare.
Acestea sunt defecte ale probelor de închidere; nu demonstrează că exportul
curent emite acele valori greșite. Review-urile S0–S3 rămân închise.

## B8-RV1 / P2 — clasele A/B acceptă diferențe în afara regulii explicate

Loc: `nou/tools/ModelCheck/SaftAb.cs` **la `900cf7b`**, `Clasificator.Miscare`,
liniile 272–294; fișierul a fost șters la pasul următor. Martorii arhivați:
[raportul A/B](tr-d8-saft-ab.md), clasele SAF-B5, S3-R1 și S3-R2.

Ramura document ↔ conex adună Q/V fără să condiționeze codul mișcării.
Ramura ASM cere Q egală, `Abs(Vvechi − Vnou) <= 0.01` și conservarea
valorii în postările reale ale tranzacției. Nu cere ca valoarea nouă din
export să fie valoarea acelor postări. Astfel, conservarea cubului nu
validează valoarea modificată din DTO.

Contraexemple executate cu clasificatorul istoric nemodificat, pe copii
controlate ale DTO-ului scenei reale S3 din ianuarie 2041:

| Mutant | Așteptare | Rezultat |
|---|---|---|
| FCT: `MovementType` și `MovementSubType` 10 → 80, aceleași cantități și valori | respins: recepția nu devine transfer prin mutarea sursei FCT/NIR | 1 diferență, clasificată SAF-B5, 0 perechi neînchise |
| ASM: vechi −3,33, nou −3,32, în timp ce consumul din cub este −3,34 | respins: diferența de un ban are direcția greșită | 1 diferență, clasificată S3-R2, 0 perechi neînchise |

Metodă: `LunaNoua`, `Compara`, `Clasa` pe fiecare diferență și
`PerechiNeinchise`, cu aceeași condiție de acceptare ca gate-ul istoric.
Această probă izolează predicatul clasificatorului; nu pretinde o nouă
rulare integrală a exportului vechi. Mutantul de cod pornește de la două
copii ale exportului curent, iar mutantul ASM fixează explicit perechea
vechi/nou de mai sus. Defectul rezultă și direct din ramurile clasificatorului.

Mutanții arhivați de +1 nu ajung la aceste cazuri: codul nu schimbă suma,
iar +1 depășește toleranța ASM. Slăbiciunea martorului Payments, declarată
de autor, rămâne relevantă pentru același gate: conservarea brutului și a
sumei liniilor nu demonstrează alocarea pe referințe.

**Închidere:** fiecare clasă trebuie să limiteze câmpurile care pot diferi
și să verifice exact ținta din regulă/sursă. Pentru recepții, codul se
verifică față de politica aplicabilă; pentru ASM, valoarea și sensul Δ se
leagă de postările evenimentului și ale lotului. Pentru Payments se
verifică referințele și sumele alocate. Se adaugă mutanții respectivi și se
reverifică raportul final pe starea cu ambele rute, într-un checkout și cu
baze izolate. Nu este necesară reintroducerea permanentă a rutei vechi.

## B8-RV2 / P2 — D18-V1 își restrânge oracolul după rezultatul verificat

Loc: [Program.cs](../../nou/tools/ModelCheck/Program.cs), liniile 13889–13913.

`conturiStoc` vine din `saft.StocFizic`, apoi recalculul postărilor este
filtrat cu `conturiStoc.Contains(p.Cont)`. Dacă exportul omite toate
pozițiile unui cont raportabil, contul dispare și din oracol. Verificarea
`cheiLipsaNaiv` nu îl mai poate găsi, deși mesajul afirmă că nicio cheie cu
sold nu lipsește. Harta B8-R2 declară păstrată acoperirea D18-V1.

Am extras fără modificare expresiile blocului curent D18-V1 și le-am
executat în fixture-ul S3, care are mai multe conturi de stoc:

- original: 12 poziții, 22 postări în recalcul, 0 diferențe, 0 chei lipsă;
- mutant: elimin toate cele **3 poziții 371**, rămân 9 poziții;
- recalculul se restrânge la 14 postări și declară tot **0 diferențe,
  0 chei lipsă**, cu rezultat `true`.

Nu afirm că toate celelalte aserțiuni numerice D17 ar accepta mutantul;
contraexemplul privește garanția distinctă de completitudine a D18-V1.

**Închidere:** domeniul conturilor/pozițiilor așteptate trebuie determinat
independent din postări și configurația de raportare. Compararea acoperă
cheile în ambele sensuri și sumele, inclusiv cazul în care un cont întreg
lipsește. Se păstrează o scenă cu minimum două conturi și mutantul de mai sus.

## B8-RV3 / P2 — gate-ul perf trece un experiment diferit de B8-D3

Locuri: [Program.cs](../../nou/tools/ModelCheck/Program.cs), liniile 671–674;
[PerfSaft.cs](../../nou/tools/ModelCheck/PerfSaft.cs), `Unitate` 94–108,
`Masoara` 110–151, `Evalueaza` 158–189; contract B8-D3 și B8-R3;
[măsurătorile arhivate](../api/p5-perf-masuratori.md#tr-d8-saf-b8-2026-09-30--saf-t-l-și-s-pe-cub-scara-sintetică).

Auditul artefactului `run-verificari/perf-saft-20260930-162802/perf-saft.md`
și al codului confirmă:

| Cerință aprobată | Executat |
|---|---|
| k ∈ {1,4,16,64} × m ∈ {0,6,12} | k=1,4,16,64,256 numai la m=0; la m=6 și m=12 numai k=16 |
| planuri Payments și Opening S la k=64,m=12 | acel punct nu este generat |
| rece = proces nou, pool gol | același proces, `ClearAllPools` și GC; JIT rămâne cald |
| vârful setului de lucru | `GC.GetTotalMemory(false)` eșantionat, adică memorie gestionată |
| unitate de volum inclusiv ASM | `Unitate` nu conține ASM |
| durată și alocări cel mult ×5 la fiecare pas k×4 | timp fără SQL; alocări până la ×6, etichetate „n log n” |

Lipsesc șase puncte aprobate: pentru fiecare m=6 și m=12, k=1,4,64.
Punctul suplimentar k=256 este util, dar nu le înlocuiește.
Tabelul autorului încalcă pragul inițial de timp chiar în scara aprobată:
L, m=0, k16→64: **35→323 ms, ×9,23**, peste ×5.
Pe extensia k64→256, alocările L sunt **11,2→59,2 MiB, ×5,29**, iar timpul
S **23→264 ms, ×11,48**. Rotunjirea tabelului nu explică aceste depășiri.

**Judecata cerută despre „precizare”.** Separarea costurilor SQL, server și
client este utilă. Reproducerea cu psycopg susține că saltul nu este
specific Npgsql; nu demonstrează singură mecanismul ACK presupus și nu
transformă timpul total în timp fără SQL. Relaxarea alocărilor de la 25%
la 50% și reducerea matricei sunt modificări de criteriu, nu simple
precizări. Un raport ×5,3 la un pas nu demonstrează ordinul n log n.

B8-Q3 = A aprobă explicit numai componentele S pe fereastra lunii plus
soldul inițial. Nu aprobă aceste schimbări ale experimentului sau ale
pragurilor. SAFT-r4 este deja declarată: balanța scanează istoric pe server.
Nu o raportez drept defect nou. Numărul rândurilor întoarse nu măsoară
scanarea pe server; planurile și această delimitare trebuie păstrate.

**Închidere:** completați matricea, ASM, procesul rece, măsurarea setului de
lucru și planurile la punctul cerut; evaluați criteriile aprobate. Dacă se
schimbă cerințele, owner-ul trebuie să aprobe explicit amendamentul, cu
pragurile fixate înaintea noii măsurări. Până atunci, „toate criteriile OK”
descrie numai evaluatorul modificat, nu trecerea B8-D3.

## Verificări și limite

Comenzi ModelCheck, pe bazele proprii `.CodexSaftS3R`, secvențial:

```powershell
pwsh -NoProfile -File nou/tools/ModelCheck/scripts/verifica.ps1 -Suita Integral -Profil Ambele -Sufix .CodexSaftS3R
pwsh -NoProfile -File nou/tools/ModelCheck/scripts/verifica.ps1 -Suita Scenarii -Tip SAFT -Profil Privat -Sufix .CodexSaftS3R
pwsh -NoProfile -File nou/tools/ModelCheck/scripts/verifica.ps1 -Suita Scenarii -Tip SAFT -Profil Ambele -Sufix .CodexSaftS3R
```

- Integrala pe HEAD curat: **3.269 bugetar / 4.470 privat**, zero FAIL,
  exit 0, `run-verificari/20260930-200732-448`.
- A doua comandă, cu probe adverse temporare: exact **3 FAIL așteptate**
  (`REVIEW-B8-AB-COD`, `REVIEW-B8-AB-DELTA`, `REVIEW-B8-D18`),
  `run-verificari/20260930-201458-268`. Coerența L↔S trece în lunile 1–3:
  soldul net Closing din L = `ClosingBalanta` din S, iar suma componentelor
  = diferența pe cont, inclusiv NTC fără lot și luna fără mișcări.
- Sursele restaurate, a treia comandă: **42 bugetar / 274 privat OK**,
  zero FAIL, exit 0, `run-verificari/20260930-201858-800`. Hash-ul DLL revine
  exact la cel al integralei:
  `3AAB9D1D68CC810117F1887A0B55C4F6FA00DD1C0902758AE4E566B735620C9C`.
- L în lunile 1–3 și luna fără rulaj, S în lunile 1–3: XSD/DUK valide,
  zero atenționări; manifestele sunt în `tmp/atlas-saft/{s0,s3}-*` din
  rularea finală. Probe de refuz și mutanții negativi incluși.
- XSD-ul, kitul DUK (toate cele cinci JAR-uri din manifest) și nomenclatorul au
  aceleași hash-uri după rulare: `run-verificari/saft-b8-review/pinuri-dupa.json`.
- `dotnet build nou/Atlas.Conta.BackOffice/Atlas.Conta.BackOffice.WebApi/Atlas.Conta.BackOffice.WebApi.csproj --nologo`:
  succes, zero warnings/errors, în modul nesupravegheat.
- `node node_modules/typescript/bin/tsc -b`, în `nou/Atlas.Conta.Client`:
  exit 0. UI B8 verificat în surse; nu am repetat smoke-ul de browser al autorului.
- HTTP pe un singur host propriu, `http://127.0.0.1:5092`, baza
  `Atlas.Conta.BackOffice.Privat.CodexSaftS3Http`: `saft-acces.py` **7 PASS**,
  `saft-stocuri.py` **10 PASS**, `fiscal-cub.py` **22 PASS**,
  `refuzuri.ps1` **294/294 PASS**. Comenzile și logurile sunt în
  `run-verificari/saft-b8-review/http.ps1`, respectiv fișierele `*.log`.
  Comparația înainte/după a numărului de rânduri din cele 12 tabele
  inventariate nu are diferențe (`http-baseline.json`, `http-audit.log`);
  hostul a fost oprit.
- Perf: audit al codului și al artefactelor autorului, fără o nouă rulare
  completă `--perf-saft`. `perf-audit.py` și rezultatul sunt în
  `run-verificari/saft-b8-review/`.

Reproducerea probelor adverse: `run-verificari/saft-b8-review/advers.py`
aplică scena și copiază clasificatorul istoric extras cu
`git show 900cf7b:nou/tools/ModelCheck/SaftAb.cs`; patch-ul și logurile sunt
în același director. Toate modificările temporare de cod au fost eliminate.
Predarea conține numai documentația review-ului, fără commit.
