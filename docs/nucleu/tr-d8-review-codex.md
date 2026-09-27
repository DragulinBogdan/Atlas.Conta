# TR-D8 — review propriu Codex

2026-09-25. Review al implementării în lucru, început peste `c10d0fe` și
completat pentru snapshot-ul de stoc peste `f5dfce6`, înainte de închiderea
etapei. D8-B6 A este aprobată de owner și consemnată în 100.
Review-ul nu certifică întregul TR-D8. Nu a fost trimis un mesaj intermediar către Claude.

## Corecturile review-ului fiscal R1–R5 — 2026-09-26

Review-ul Claude 1018 este confirmat. R1–R5 sunt corectate pe același diff,
fără commit. R6 este numai specificație în `tr-d8-tva-intervale-contract.md`,
103(h) și SC-CIT-90…94; referința la avans cu mai multe cote cere review.

- R1: `Fiscale.Versiune` calculează SHA-256 pe faptele comune ordonate,
  formular și lună; D300/D394 emit versiunea în DTO. Citirea raportului și
  a versiunii folosește același snapshot repeatable-read. Confirmarea
  recalculează sub blocaj exclusiv și refuză `DEPUNERE_VERSIUNE_DEPASITA`.
  Nu există duplicare a regulilor de sumare sau un registru paralel.
  SC-CIT-88 probează fapt nou, identități schimbate la aceleași sume,
  formular/perioadă diferite, idempotentă și scriere pe altă conexiune în
  timpul snapshot-ului. UI exportă JSON cu amprentă și confirmă versiunea
  aleasă. Browserul a găsit necesitatea recitirii la reexport; butonul
  recitește acum raportul și elimină eroarea vechiului export după succes.
- R2: enum-ul/câmpul `DeclarareIntarziata`, seed-ul, operandul și parametrii
  inerți au ieșit. Atribuirea folosește 103. Așteptările PDT verifică
  direcția și faptele reale; proba schimbării unei politici fără efect a
  ieșit. Metadata/OpenAPI/types nu mai conțin atributul.
- R3: `CK_Postare_FiscalComplet` impune calificarea și reperele nenule,
  inclusiv primirea la achiziții. SC-CIT-89 verifică 14 eliminări individuale
  pe postări reale, refuz PostgreSQL 23514 și rollback, pe ambele profiluri.
  Mutantul vechi cu reper absent este înlocuit de proba bazei; invarianții
  de calificare între roluri, autolichidare, duplicare și inversă rămân.
- R4: antetul D8-B8 reflectă aprobarea și implementarea.
- R5: paragraful cu rulările 103 a ieșit din `dezvoltare-si-validare`;
  evidența rămâne aici și în `istoric-plan-de-lucru`, fără copiere în capul
  paginii de stare la fiecare rerulare.

Validări finale:

- FISCALE ambele profiluri: `run-verificari/20260926-111519-176`, exit 0.
- Integral: **3.207 bugetar / 4.237 privat OK**, zero FAIL, exit 0, build
  fără avertismente; `run-verificari/20260926-111832-851/rezultat.json`.
  Comandă: `pwsh -NoProfile -File nou/tools/ModelCheck/scripts/verifica.ps1 -Suita Integral -Profil Ambele -Sufix .FiscalCub`.
  Wrapper-ul `run-verificari/fiscal-r1-integral.ps1` compilează și generează
  metadata înaintea integralei. Atlas.DXF 26.1.4.9 / DevExpress 26.1.4.
  Nu s-au editat sursele compilate în timpul execuției. Nucleul nu este
  modificat în această corectură; cifra 180/180 de mai jos aparține feliei anterioare.
- HTTP real: `python -X utf8 nou/tools/ProbeHttp/fiscal-cub.py --baza Atlas.Conta.BackOffice.Privat.FiscalHttp`,
  `run-verificari/fiscal-r1-http.log/.err` (stderr gol). Matricea de acces
  trece în toate cele trei stări. R1 refuză versiunile depășite pentru
  D300 și D394, precum și versiunile obținute pe document/taxă/valoare
  mascate; exportul nou este confirmat. Blocajul și refuzul anulării rămân verzi.
- Browser D394: 100/21 → export → încă 25/5,25 → refuzul versiunii vechi;
  reexport 125/26,25 → confirmare la 26.09.2026, 11:32:59. Owner-ul a salvat
  cele două fișiere în `D:\Temp`; am verificat conținutul și amprentele lor.
  Al doilea corespunde exact versiunii confirmate. Dovezi numerice:
  `run-verificari/fiscal-r1-export-verificat.json`. Fișierele owner-ului rămân.
- WebApi și client build reușite; după corectura de reexport,
  `fiscal-r1-client-final.exit` = 0. Metadata/OpenAPI/types stabile la a doua
  regenerare (`fiscal-r1-drift.log`). Comenzi: `pwsh -NoProfile -File run-verificari/fiscal-r1-client.ps1`,
  apoi `fiscal-drift.ps1` și `fiscal-r1-client-final.ps1`. Configurația
  temporară de restore fixează versiunile deja folosite de integrală;
  configurația de pachete a repo-ului rămâne neatinsă.
- Migrația `20260926081425_FiscalComplet` este aplicată pe cele trei baze
  principale și pe baza izolată HTTP. Control independent: cinci migrații,
  CHECK validat, coloana veche absentă, zero postări/depuneri; fixture-urile
  2021/2022 sunt curățate. `fiscal-r1-baze-verificate.json`,
  `fiscal-r1-migrari.log`, `fiscal-r1-migrari.exit` = 0. EF nu găsește drift
  de model. Nu s-au recreat baze. Hosturile proprii de test au fost oprite.
- `git diff --check` trece. Rămân avertismentele EF tool 10.0.9/runtime
  10.0.12 și Vite chunk mare.

Limitele amprentei: certifică faptele cubului din luna/formularul ales,
nu XML-ul ANAF, recipisa, mapările curente sau parametrii externi D300.
Exportul disponibil este DTO JSON. Calculul amprentei adaugă o citire și
materializarea faptelor lunii; A/B din felia inițială nu măsoară acest cost
nou. Nu am reluat benchmark-ul și nu pretind menținerea latenței de 59 ms.
Planurile și măsurarea pe volumul țintă rămân înaintea SourceDocuments,
conform review-ului; nu s-a introdus index sau snapshot fiscal aici.

## Felia fiscală 103 — review propriu inițial, 2026-09-26

Sursa comună este `Cub.Citiri.Fiscale`: măsurile sunt valorile semnate ale
postărilor cu rol fiscal, din ambele cărți. Regimul/cota/importul și cele
două atribuiri fiscale se păstrează la scriere; corecția tehnică păstrează
identitatea fiscală, fără a introduce o factură nouă în D394.

Probleme găsite și corectate în această felie:

- înregistrarea absentă din documentul DEC trebuia normalizată înaintea
  atribuirii fiscale; data implicită rămâne data documentului;
- D300 nu poate scădea TVA capitalizată încă o dată când o politică o mapează
  direct în afara totalului deductibil; regularizarea capitalizată păstrează
  baza, fără a fabrica taxă dedusă;
- SAF-T nu mai numără taxa capitalizată și în GL, și ca reziduu fiscal;
- anularea operării după o depunere confirmată ar fi șters faptul declarat;
  acum se refuză și după redeschiderea contabilă;
- ștergerea draftului unei corecții tehnice ar lăsa numai inversa: refuz de
  domeniu; confirmarea depunerii așteaptă finalizarea corecției;
- confirmarea depunerii și scrierile fiscale folosesc blocaj tranzacțional
  comun; proba HTTP ține deschisă o tranzacție fiscală pe altă conexiune și
  demonstrează că depunerea așteaptă;
- fără citire pe întregul tip `Postare`, mascarea EF a coordonatelor producea
  o eroare SQL în grupări. Controllerele refuză 403 înainte de interogare;
  restricțiile pe obiect și pe membru rămân exercitate prin providerul real;
- browserul a găsit coloana Rol fiscal goală: generatorul metadata include
  acum și enumurile proprietăților persistente din alte assembly-uri.

Validări încheiate:

- ModelCheck integral: **3.182 bugetar / 4.214 privat OK**, zero FAIL, exit 0,
  build fără avertismente. `run-verificari/20260925-234727-019/rezultat.json`.
  Sursele compilate nu au fost modificate în timpul rulării.
- Nucleu: **180/180**, zero omise, exit 0;
  `run-verificari/20260925-235511-876/rezultat.json`.
- HTTP `fiscal-cub.py`: șase configurații de acces × trei stări
  (deschis/închis/reconstruit), fiecare pe jurnal/decont/D300/D394 și
  TaxInformation GL. Admin/Cititor 125/26,25; un document refuzat 100/21;
  rolul Taxă refuzat 125/0; Valoare refuzată 0/0; User 403. Confirmare cu
  401/400/404/403, idempotentă și serializată; anulare după depunere 422.
  `run-verificari/fiscal-http-probe4.log`, exit 0, fixture curățat.
- Browser: datele facturii urmează implicit înregistrarea, dar păstrează
  primirea introdusă explicit; pagina politicii afișează cele 39 de mapări.
  Rolurile Taxă/Autocolectare sunt vizibile. D394 pentru ianuarie 2021,
  pe fixture-ul izolat, afișează 10 facturi, bază 100.000 și TVA 21.000;
  confirmarea `UI-FISCAL-103` apare în pagină la 26.09.2026, 00:10:20.
  Jurnalul afișează separat datele istorice, perioadele D300/D394 și
  marcajele Regularizare/Inversă. Confirmarea este internă, fără transmitere ANAF.

Verificarea finală a livrării:

- build WebApi fără avertismente/erori; build client reușit, cu avertismentul
  Vite existent pentru chunk peste 500 KB. Metadata/OpenAPI/types sunt stabile
  la regenerare: `run-verificari/fiscal-client-validat.log`,
  `fiscal-final-clean.log` și `fiscal-drift.log`;
- migrațiile fiscale și seed-ul sunt aplicate pe `Atlas.Conta.BackOffice`,
  `Atlas.Conta.BackOffice.Privat` și `Atlas.Conta.ModelCheck.Privat`.
  Verificarea independentă confirmă patru migrații, zero postări și respectiv
  0/39/39 mapări. Bazele nu au fost recreate. Dovada:
  `run-verificari/fiscal-migrari-verificate.json`. EF păstrează avertismentul
  uneltei 10.0.9 față de runtime 10.0.12;
- fixture-ul HTTP/perf a fost curățat: zero documente active, postări,
  confirmări și perioade 2021 în baza izolată. Rândurile documentelor șterse
  logic nu sunt declarate șterse fizic;
- sursa locală `F:\dev\Atlas.DXF\nuget` este accesibilă. Validarea finală
  folosește Atlas.DXF 26.1.4.6 și DevExpress 26.1.4, fără schimbarea
  configurației de pachete a proiectului; restore-ul de verificare a fixat
  temporar versiunile în `run-verificari/`;
- `git diff --check` trece. Modificările rămân necomise, împreună cu
  corecturile C1–C5 ale snapshot-ului de stoc.

Încercările intermediare nu sunt ascunse: prima integrală bugetară a găsit
metadata/lista politicilor nealiniate; prima integrală privată a găsit cele
cinci abateri de așteptări/contorizare corectate ulterior. Prima probă HTTP a
expus refuzul lipsă pe tip; următoarele două au corectat fixture-ul (sintaxa
GUID a criteriului și ITV înaintea închiderii). Niciuna nu este prezentată
ca execuție verde. Procesele au folosit modul nesupravegheat; această serie
nu a produs excepții scăpate către dialogul sistemului de operare.

Limite: `SourceDocuments` integral, restul portării SAF-T, reconcilierea,
auditul și performanța exportului complet rămân TR-D8. Verificarea refuzului
pe `Postare.Valoare` privește sumele fiscale din `TaxInformation`, nu toate
sumele documentelor exportate. Datele de identificare/clasificare ale
partenerului nu sunt istoricizate în această felie. Regimurile necontractate
(TVA la încasare, pro-rata, deduceri parțiale) nu sunt certificate.

### Măsurare A/B a feliei fiscale

Aceeași bază privată izolată, 120 FCT de servicii operate prin motor,
100 linii per document, distribuite în 12 luni: **12.000 fapte fiscale** și
12.000 rânduri RegistruTva. Codul anterior este extras din `07c79e0` într-un
harness izolat; codul nou este DLL-ul validat. Un warm-up, apoi șapte citiri
complete, fiecare într-un ObjectSpace nou, fără paginare; mediane locale.
Provider nesecurizat pe ambele căi: costul filtrării secured este probat
funcțional prin HTTP, nu măsurat în acest benchmark.

| Citire | Registre anterior | Cub | Rezultat independent |
|---|---:|---:|---|
| Jurnal, an întreg | 5,53 ms | 58,97 ms | 120 rânduri; bază 1.200.000, TVA 252.000 pe ambele căi |
| D394, an întreg | 9,55 ms | 58,84 ms | o poziție, 120 facturi; bază 1.200.000, TVA 252.000 pe ambele căi |
| Materializare SAF-T L, luna 1 | 50,96 ms | 60,05 ms | 10 facturi/1.000 linii; taxa GL corectă 21.000 în cub, 42.000 în cititorul anterior care o atașa ambelor laturi |

Costul fiscal este măsurat și prin materializarea SAF-T care îl consumă;
nu pretindem că cei 60 ms măsoară numai fiscalul sau serializarea XML.
Datele brute: `run-verificari/fiscal-perf/result.json`,
`run-verificari/fiscal-perf4.log`; sursele harness-ului sunt în același
director. Diferența de sumă SAF-T este intenționată și verificată prin
postările independente; nu este declarată paritate cu rezultatul greșit.

**Concluzie de performanță:** aproximativ 10,7× pentru jurnal și 6,2×
pentru D394, deși latența absolută rămâne sub 60 ms pe acest fixture.
Agregarea pe faptul fiscal și apoi pe raport are un cost față de registrul
pregrupat. Acest volum nu certifică scalarea la milioane de linii și nu
justifică introducerea unui snapshot fiscal necontractat. Măsurarea pe
volumul țintă și optimizarea planurilor rămân punct explicit al gate-ului
transversal TR-D8/T-r11, înaintea închiderii întregii etape.

## Probleme găsite și corectate

### Corecturile C1–C5 peste 07c79e0 (review 2051)

- **C1:** `SnapshotStocCub` aplicată pe `Atlas.Conta.BackOffice`,
  `Atlas.Conta.BackOffice.Privat` și `Atlas.Conta.ModelCheck.Privat`.
  Controlul ulterior confirmă ambele migrații, coloanele noi și zero rânduri
  în snapshot-ul de stoc. Nu s-au recreat baze și nu s-au șters reziduuri.
  Dovezi: `run-verificari/snapshot-review-migrari.log` și
  `snapshot-review-date.log`. EF nu detectează schimbări de model restante;
  păstrează avertismentul de versiune a uneltei 10.0.9 față de runtime 10.0.12.
- **C2:** `Loturi.Miscari` deține semnul și data deschiderii;
  `Grupeaza` deține cheia și agregarea LINQ. Materializarea/reconstrucția
  SQL pornesc din aceeași proiecție. Contractul de aliasuri SQL rămâne probat.
- **C3:** `CumulPerioade.Citeste` compune referința, snapshot-ul și fereastra
  într-o singură instrucțiune pentru contabil/stoc/partide. Granița contabilă
  rămâne distinctă de data finală. Evaluarea documentului exclus folosește
  referința strict anterioară; excluderea istorică generală rămâne directă.
  SC-CIT-76 vede evaluarea 10 → 12 → 10, SC-CIT-77 închide pe altă conexiune
  după compunere și verifică atât cifrele, cât și numărul instrucțiunilor.
- **C4:** toate scrierile globale și sursele SQL refuză securitatea activă.
  SC-CIT-78 verifică refuzul înaintea accesării datelor printr-un proxy;
  **nu** este prezentată ca probă a filtrării reale. Aceasta este SC-CIT-74 HTTP.
  Prima gardă, bazată numai pe `ISecuredObjectSpace`, a produs HTTP 500 la
  închiderea autorizată. Providerul DevExpress 26.1 creează aceeași clasă și
  pe ușa nesecurizată, cu `ISecurityEnabledOption.EnableSecurity=false`.
  Verificarea acestei opțiuni este acum comună citirii și scrierii; nu
  exceptăm administratorul și nu modificăm autorizarea controllerelor.
  Prima execuție HTTP este roșie: `snapshot-review-http.log/.err`.
  Reluarea corectată: `snapshot-review-http2.log/.err`, **15/15**, exit 0.
- **C5:** caption-uri Cont/Produs/Gestiune și metadata regenerate, tuple
  consecvente, using-uri curățate, scenariul împărțit lizibil. Istoricul de
  felie a ieșit din stare-curenta; două pasaje vechi despre partide au fost
  înlocuite cu trimitere la regula actuală, fără o a doua descriere.

Validarea selectivă a expus întâi o eroare în fixture-ul SC-CIT-76: schimbasem
data BCS în februarie, dar nu data înregistrării. Contractarea refuza corect.
Am corectat ambele date și am făcut refuzul explicit în mesajul probei;
reluarea este verde pe ambele profiluri (`20260925-211432-783`). Prima
integrală, `20260925-211753-103`, are 3.178/4.178 OK, dar precede corectarea
gărzii găsită prin HTTP; nu o folosim pentru a certifica versiunea finală.

Clientul a fost regenerat repetat fără drift pe metadata/OpenAPI/types și
build-ul trece (`snapshot-review-client2.log/.err`); avertismentul Vite
despre dimensiunea bundle-ului rămâne. HTTP confirmă 15/125 pentru
Admin/Cititor, 10/100 pentru restricția pe rând, 15/0 la membrul Valoare
refuzat și zero rânduri pentru User, înainte/după închidere/reconstrucție.
Curățarea este verificată independent în `snapshot-review-cleanup.log`:
zero documente active și zero entități temporare active/perioade/snapshot-uri
ale probei. Hostul de test este oprit. Nu s-a reluat browserul/XAF;
nu există modificări funcționale de UI în această felie.

Baze cu sufix, numai inventariate pentru decizia owner-ului:

```
Atlas.Conta.BackOffice.C102R1
Atlas.Conta.BackOffice.CodexC102Review
Atlas.Conta.BackOffice.Privat.CodexC102Http
Atlas.Conta.BackOffice.Privat.SnapshotHttp
Atlas.Conta.BackOffice.SnapshotStoc
Atlas.Conta.ModelCheck.Privat.C102R1
Atlas.Conta.ModelCheck.Privat.CodexC102Review
Atlas.Conta.ModelCheck.Privat.SnapshotStoc
```

Performanța pe istoric mare rămâne nemăsurată. O instrucțiune SQL probează
atomicitatea citirii față de închidere, nu timpul de răspuns. Nu am reluat
matricea HTTP completă contabil/partide în această felie; toate cele trei
citiri au probe ModelCheck, iar SC-CIT-74 exercită ramura comună de securitate.
Fără commit. D8-B8 și SC-CIT-79…87 sunt propuneri fiscale pentru owner,
fără implementare și fără a închide B-r4 ori întregul TR-D8.

Comenzile de reproducere pentru această corecție:

**Validare finală C1–C5:** `run-verificari/20260925-213600-946/rezultat.json`,
exit 0, **3.178 bugetar / 4.178 privat OK**, zero FAIL; build 0 warnings / 0 errors.
Sursele C# nu s-au modificat în timpul rulării sau după ea.
`dllSha256` ModelCheck:
`AF1EC22B3AB8B45132EF0F90E32608C4E46523B03F49F1E32DE7C71835994A64`.
Modulul verificat în ModelCheck și în hostul HTTP are același SHA256:
`A7D2B5B92FD9DF8E57E8CC64BDDE8F83D1030D8F8410358E2FD9F10EBE60D2BF`.

```powershell
pwsh -NoProfile -File nou/tools/ModelCheck/scripts/verifica.ps1 -Suita Integral -Profil Ambele -Sufix .SnapshotStoc
python -X utf8 nou/tools/ProbeHttp/stoc-snapshot-cub.py --baza Atlas.Conta.BackOffice.Privat.SnapshotHttp
```

Proba HTTP cere hostul izolat pe portul 5089, cu seed și utilizatori;
wrapper-ul local `run-verificari/snapshot-http-host.ps1` arată configurația
folosită. Rulează secvențial cu verificarea grea, în mod nesupravegheat.

### Constatările feliilor precedente

1. **Refuzul stocului venea prea târziu la anulare/storno.** Verificarea
   din materializator putea refuza după modificarea tracker-ului registrelor.
   Un apel ulterior pe același ObjectSpace vedea acele schimbări necomise.
   `Loturi.VerificaRetragere` verifică acum inversa cantitativă înaintea
   ștergerii conexelor, inversării împerecherilor și modificării registrelor.
   Gardul materializatorului rămâne pentru tranzacția completă. Regresia
   BTR verifică refuzul urmat de anulare/storno valid în același ObjectSpace.
2. **Refuzul cubului venea după asignarea numărului.** `Opereaza` cere
   refuzurile declarației înaintea numerotării și a materializării.
   Aceasta dublează momentan contractarea; costul trebuie inclus în
   măsurătoarea de performanță a etapei, înaintea unei optimizări.
3. **Explicația S3 din SAF-T avea altă sursă decât totalul.** Totalul
   balanței fusese portat, dar componentele pe tipuri de document încă
   citeau RegistruContabil. Deschiderea canonică a expus diferența în
   D17-V6/F7. Componenta contabilă folosește acum `Contabil.Postari`,
   inclusiv deschiderea și excluderea transferurilor; proba nu a fost slăbită.
4. **Scenele vechi nu aveau suportul de stoc în cub.** Loturile sintetice
   din probele BCS/BTR/FCL/LDI și SAF-T au primit deschidere canonică sau
   recepție FCT → NIR. Suplimentarea stocului nu creează o a doua deschidere.
   Registrele păstrate în fixture sunt oracolul explicit al regimului dual.
5. **Catalogul FIFO promitea mai mult decât verifica.** SC-CIT-39 verifică
   acum explicit excluderea altui cont și a altei gestiuni, prin mutații
   controlate cu rollback, pe lângă prioritatea pinului și alocarea locală.
6. **Ștergerea împerecherii nu elibera cubul.** Cu FCT 100 și PLT automată
   100, ștergerea linkului lăsa factura stinsă în cub. Comanda de ștergere
   scrie transferul compensator și elimină linkul în aceeași tranzacție.
   Ușile API/XAF păstrează gate-ul Delete; CRUD-ul direct este refuzat
   independent de dreptul de a citi Postare. SC-CIT-46/48 verifică
   soldurile și păstrarea postărilor inițiale.
7. **Inversa pregătită de corecție era invizibilă gardului de dependențe.**
   F27-R3b refuza factura complet stinsă în luna închisă, deși aceeași
   comandă pregătise inversa stingerii. Gardul include rândurile noi din
   tracker, fără dublarea celor persistate; tăierea zilnică rămâne.
8. **Stornoul nominalizării automate putea dubla desfacerea.** La storno
   direct, inversa Operare eliberează deja factura. La storno după desfacere,
   transferul compensator este atribuit nominalizării inițiale și se
   inversează împreună cu aceasta. SC-CIT-47 verifică ambele căi peste
   închidere, inclusiv păstrarea soldului din ianuarie.
9. **Reîmperecherea manuală pe aceeași factură prelua atribuirea automată.**
   Proba suplimentară SC-CIT-46 a reprodus două solduri greșite după storno
   și un refuz ulterior PARTIDA_CU_DEPENDENTI
   (`run-verificari/20260925-073311-635/scenarii-bugetar.log`). Atribuirea
   se aplică acum numai desfacerii nominalizării automate; transferul
   manual și inversa lui se compensează separat.

10. **Partida era confundată cu documentul în raport/snapshot.** Raportul
    general și snapshot-ul păstrează unitate × cont × partener, inclusiv
    deschiderile fără document și identitatea istorică. SC-CIT-49…54 verifică
    două partide ale aceleiași note, deschiderea și reconstrucția cu diferență
    introdusă deliberat; citirea secured nu consumă snapshot-ul global.
11. **Legătura putea exista fără efect în cub.** Decizia 101 este aprobată.
    Comanda refuză ținta incompatibilă, ambiguă sau insuficientă înaintea
    scrierii; o nominalizare existentă se asociază fără a o dubla. Proveniența
    transferului permite desfacerea exactă a fiecărei legături, chiar când
    două transferuri privesc aceeași pereche. SC-CIT-55…60 verifică refuzul
    repetat în același ObjectSpace, diagnosticul istoric și asocierile parțiale.
12. **Soldul final ascundea indisponibilitatea la o dată anterioară.**
    Transferul și asocierea unei nominalizări verifică și efectele ulterioare.
    Pentru nominalizarea automată, disponibilul sursei este minimul pe fiecare
    dată deja scrisă. SC-CIT-61/62 păstrează stingerile viitoare și nu consumă
    retroactiv suma eliberată ulterior.
13. **Plata automată încă citea sursa din registre.** FCT 159,5 cu recepție
    și serviciu nominaliza numai 100 cât timp NIR era Draft. `Fapte.Sursa`
    citește acum partidele sursei din cub; integralul verifică plata înaintea
    operării NIR, apoi operează recepția pentru comparația cu oracolul dual.
    F27-R3c păstrează și cazul facturii cu înregistrare întârziată.
14. **Transferul general pierdea ținta deschiderii.** Când comanda indică
    explicit o partidă de deschidere, aceasta este ținta, fără a cere un
    document fictiv. SC-DES-11…14 verifică consumul succesiv și inversarea.

15. **Gardul de acoperire confunda costul fără partidă cu o lipsă.**
    Controlul pe o latură contabilă fixă nu este suficient la vânzare/retur.
    Se compară acum totalul de decontare al antetului cu valoarea partidelor
    Operare. Antetul servește numai diagnosticului de activare; raportul,
    disponibilul și snapshot-ul nu citesc cifra lui. SC-CIT-43/65 disting
    lipsa reală de unități de costul legitim fără partidă.
16. **Probe vechi cereau comportamentul înlocuit de 101.** F19-D16 admitea
    PLT 401 ↔ INC 4111 fără transfer; SC-CIT-63 cere refuz cu solduri intacte.
    Proba NUC-PLT-SPLIT folosea o plată deliberat nemigrată; verifică acum
    refuzul ei și folosește o plată pe cub pentru nominalizarea 61/60.
    PAR-V22/23 păstrează totalul RDC −121, dar verifică datoria 121 din cub,
    fără cei 30 ai costului. Nu se elimină probele numerice.

## Limite care împiedică închiderea TR-D8

- **Partidele bugetare:** D8-B6 A este aprobată (100). Seed-ul activează
  401.01.00, 404.01.00 și 411.01.01 DinSeed; cititorul comun și diagnosticul
  istoriei sunt implementate și verificate (100-r1 închisă). Raportul și
  snapshot-ul sunt acum portate; gardul refuză și politicile manuale care
  produc documente stingibile fără partide. Validarea feliei 101 este mai jos.
- **Fiscal/SAF-T:** TVA, D300/D394 și majoritatea secțiunilor SAF-T nu sunt
  încă portate. Versiunea istorică TVA trebuie păstrată, nu recitită din
  nomenclatorul curent. Portarea GLA/Customers/Suppliers și S3 contabil
  nu certifică întregul export.
- **Activarea stocului și politica modificată în mers:** diagnosticul
  contabil de la pornire nu dovedește încă toate transferurile cantitative.
  Trebuie probat istoricul de stoc incomplet și refuzată producerea unor
  fapte noi numai în registre după dezactivarea `PosteazaInCub`.
- **Concurența pe lot:** probele numerice secvențiale și gardul zilnic nu
  dovedesc serializarea a două consumuri concurente. Este necesară o probă
  pe două conexiuni, inclusiv cursa consum–retragere, înainte de certificare.
- **Regula de oprire:** auditul S-r2, controlul arhitectural 091-r3,
  măsurătorile A/B și verificarea tuturor consumatorilor din inventar
  rămân obligatorii. Scrierea registrelor nu se elimină aici.

## Snapshot-ul de stoc — review și validare (2026-09-25)

Snapshot-ul este portat pe cub. Review-ul propriu nu a identificat un
blocant în această felie; limitele transversale de mai sus rămân deschise.

- **Cheia și data FIFO:** materializarea, citirea și reconstrucția folosesc
  lot × cont × produs × gestiune și păstrează `Deschisa`. Nu comprimă
  contul de stoc cu cel de cheltuială sau gestiunea reală cu contraponderea
  virtuală. Se elimină numai cheia cu ambele măsuri zero; reziduul RLF
  0/−0,01 rămâne vizibil în raport, fără disponibil FIFO.
- **Sursa și independența reconstrucției:** `Loturi.Cumulate` combină ultima
  referință cu postările ulterioare. Reconstrucția recitește întregul cub;
  controlul SOL din ModelCheck agregă separat postările în memorie.
  Alterarea deliberată a valorii cu +7 și a datei FIFO este vizibilă prin
  cititorul optimizat înainte de reconstrucție, apoi raportată și reparată.
  Astfel proba nu poate trece doar prin ocolirea snapshot-ului.
- **Regimul dual:** cititorii registrului folosesc direct `RegistruStoc`.
  Proba FCT fără NIR operat → BTR → BCS păstrează −6 în registru, distinct
  de 4 în cub; snapshot-ul nu contaminează citirea R folosită de ASM.
- **Permisiuni și excluderi:** un ObjectSpace secured sau o citire cu
  document exclus recitește postările. Snapshot-ul global nu poate ocoli
  drepturile pe rând/membru și nu poate ascunde documentul exclus.
  Gardul zilnic continuă să verifice istoricul integral.

Validare pe sursele stabilizate:

- ModelCheck integral: **3.166 bugetar / 4.166 privat OK**, zero FAIL,
  exit 0, build fără avertismente. Dovezi:
  `run-verificari/20260925-182506-479/rezultat.json` și logurile celor două
  profiluri. Catalogul adaugă SC-CIT-69…75; SC-CIT-74 este proba HTTP.
  Prima integrală bugetară a semnalat metadata neregenerată după schimbarea
  modelului; metadata a fost regenerată, apoi integrala a trecut pe ambele
  profiluri. Nu prezentăm prima rulare ca verde.
- HTTP SC-CIT-74: **15/15 verificări**, înainte de închidere, după închidere
  și după reconstrucție. Admin/Cititor = 15 bucăți / 125; restricție pe rând
  = 10/100; membrul Valoare refuzat = 15/0; User = zero rânduri.
  Proba durabilă: `nou/tools/ProbeHttp/stoc-snapshot-cub.py --baza
  Atlas.Conta.BackOffice.Privat.SnapshotHttp`;
  log `run-verificari/snapshot-http-probe-final.log`.
  Prima execuție a trecut toate verificările, dar curățarea încerca ștergerea
  directă a NIR-ului generat; proba corectată îl elimină prin anularea FCT.
  Execuția finală, inclusiv curățarea, are exit 0.
  Controlul SQL ulterior confirmă zero documente active, produse/parteneri
  temporari activi, roluri/utilizatori temporari, perioade și snapshot-uri
  ale probei: `run-verificari/snapshot-http-cleanup.log`. Hostul este oprit.
- Metadata/OpenAPI/types: regenerarea repetată păstrează toate cele trei
  hash-uri. Build-ul clientului trece; rămâne avertismentul Vite despre
  dimensiunea bundle-ului. Log: `run-verificari/snapshot-final-client.log`.
  EF raportează model sincronizat cu migrațiile în
  `run-verificari/snapshot-artifacts.log`.
- Schema nouă este verificată pe baze proaspete, cu sufix `.SnapshotStoc`
  și `.SnapshotHttp`. Migrația nu convertește snapshot-uri vechi; bazele
  principale nu au fost recreate (102b). Nu există modificări de UI în
  această felie; nu revendicăm o nouă probă în browser.

Performanța pe istoric mare nu este certificată. Citirea registrului dual
și citirile secured/cu excludere folosesc istoricul integral; măsurătorile
A/B pe aceeași bază rămân obligatorii la închiderea TR-D8. Nu s-a trimis
un mesaj intermediar către Claude.

## Validare finală a feliei partidelor (101)

- ModelCheck integral, rulat secvențial: **3.107 bugetar / 4.103 privat OK**,
  zero FAIL, exit 0, build fără avertismente. Dovezi:
  `run-verificari/20260925-091707-216/integral-bugetar.log` și
  `run-verificari/20260925-092938-922/rezultat.json`.
  Prima pereche de rulări a expus așteptări private vechi, corectate conform
  101; privatul a fost rerulat integral. Nu prezentăm primul rezultat agregat
  ca fiind verde. După validarea finală s-au schimbat numai comentarii C#,
  documentația și o așteptare HTTP (405 pe ruta OData fără POST).
- Client: metadata/OpenAPI/types regenerate și build reușit;
  regenerarea repetată păstrează cele trei hash-uri. Loguri
  `run-verificari/trd8-101-client-final.log` și `trd8-101-drift-final.log`.
  Avertismentul Vite despre dimensiunea bundle-ului rămâne.
- HTTP, `citiri-cub.py --partide`: raportul și panoul respectă permisiunile
  înainte de închidere, după închidere și după reconstrucție. Admin/Cititor
  = 125, restricție pe rând = 100, membrul Valoare refuzat = 0, User fără
  rânduri. Două partide pe același cont și document păstrează partenerii,
  100 și 25. Log: `run-verificari/trd8-101-http-report2.log`.
  Browserul confirmă cele două rânduri, contul, data, sensul și documentul.
- HTTP, `partide-cub.py`: ștergere User 404 / Cititor 403 / Admin 204;
  creare User/Cititor 403, document lipsă 404, OData POST 405;
  comanda autorizată 40 → 201, cererea 61 peste restul 60 → 422 fără efect;
  ștergerea și anularea păstrează FCT −100 / PLT zero. Browserul confirmă
  total 100 / asignat 40 / rest 60 și candidatul cu disponibil 60.
  Log: `run-verificari/trd8-101-http-payment2.log`.
  Migrațiile au fost aplicate pe ambele baze izolate CodexBCS.
  Curățarea a fost verificată independent: zero documente active și zero
  parteneri/roluri temporare ale probelor în baza privată izolată.
- Verificarea XAF de mai jos este cea din felia precedentă, nu o nouă
  rulare XAF după 101. Matricea generală `refuzuri.ps1` rămâne necertificată
  pe baza goală; probele țintite nu o înlocuiesc.

## Validare anterioară (100, contabil și stoc)

- ModelCheck integral: **3.036 bugetar / 4.026 privat OK**, zero FAIL,
  exit 0, build fără avertismente. C# nemodificat în timpul rulării:
  `run-verificari/20260925-073729-162/rezultat.json`.
  După această rulare s-au corectat numai comentarii și eticheta probei
  RolTert; logica verificată a rămas aceeași.
- Client: metadata/OpenAPI/types regenerate, build reușit; a doua generare
  păstrează toate cele trei hash-uri. Loguri `trd8-partide-client` și
  `trd8-partide-drift` în `run-verificari/`. Vite păstrează avertismentul
  privind dimensiunea bundle-ului.
- HTTP SC-CIT-25: două execuții complete, înainte de închidere, după
  închidere și după reconstrucție. Admin/Cititor = 125, restricția pe rând
  = 100, restricția pe Valoare = 0; User primește liste goale.
  `run-verificari/trd8-http-final.log`; rolurile și perioadele temporare
  au fost eliminate. Comandă: `python -X utf8 nou/tools/ProbeHttp/citiri-cub.py`.
- Raport de stoc prin HTTP și browser, pe FCT → NIR real: cont 371,
  cantitate 3, valoare 10, cost unitar afișat 3,333333 și total 10,00.
  Fixture-ul a fost anulat/șters prin API; baza izolată are zero documente active.
- SC-CIT-46/48 HTTP: User 404, Cititor 403, Admin DELETE 204; FCT −100,
  PLT +100 după desfacere și PLT zero după anulare. Aceeași comandă prin
  acțiunea XAF „Șterge împerecherea”: rând dispărut din grilă, aceleași
  solduri verificate independent în cub. Proba durabilă
  `nou/tools/ProbeHttp/partide-cub.py` (opțional `--prin-xaf`); loguri
  `trd8-partide-http.log` / `trd8-partide-xaf.log`. Zero documente și
  parteneri activi ai fixture-ului după curățare; hosturile de test oprite.
  Hostul XAF a cerut profilul Windows normal pentru Data Protection;
  cheile existente nu au fost șterse sau reconfigurate.
- **Limită de validare:** `nou/tools/ProbeHttp/refuzuri.ps1` a fost pornit,
  dar matricea completă nu poate fi declarată trecută pe baza goală.
  După pregătirea ITV, descoperirea se oprește la lipsa unei împerecheri;
  proba DVI de mai jos presupune și peste 500 de facturi candidate
  (`MaiSunt=true`). Aceste precondiții țin de vechea bază populată/importată.
  Proba nouă SC-CIT-25 nu este prezentată ca înlocuitor al întregii matrice.

**Verdict:** felia contabilă, cititorul operațional/raportul/snapshot-ul de stoc și
raportul/snapshot-ul/citirile operaționale ale partidelor au probe verzi în
domeniul de mai sus. 101-r1 este închisă. Etapa TR-D8 rămâne deschisă; limitele
nominale din review sunt lucru obligatoriu, nu excepții aprobate.
