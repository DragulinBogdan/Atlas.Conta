# SAF-B8 — review advers Codex

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
