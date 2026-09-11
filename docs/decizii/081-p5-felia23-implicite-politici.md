# 81. Pasul 5, felia 23 — implicitele de culegere și întreținerea politicilor: regimul e al partenerului, cota e a produsului; politicile se deschid pe OData cu invarianții în gardian, unicitatea în schemă și proveniența ca timbru

- **Data**: 2026-09-02
- **Stare**: activă (închide 79-r2, 74-r12 și 77-r3 pe `PoliticaMiscareSaft`; amendează 56 pe politici — ReadOnly-ul pe OData a murit; amendează 38d/P1 §8 — implicitul de TVA nu mai e doar al tipului de document; 81d amendată de 83a — re-seed-ul corectează rândurile `DinSeed`; 81-r1 și 81-r2 închise de 83; 81-r8 și ecranele din 81k închise de 84)
- **Docs**: `docs/api/p5-felia23-implicite-politici-contract.md` (F23-D1…D12 + §Închidere), `nou/.../Module/Motor/ImpliciteService.cs`, `nou/.../Module/Motor/VerificareProfilService.cs`, `nou/.../Module/BusinessObjects/Comun/ClasaFiscala.cs`, `nou/.../Module/BusinessObjects/Comun/ICuProvenienta.cs`, `nou/.../Module/Motor/GardianEditare.cs` (ramurile D5), `nou/.../Module/Migrations/20260902184753_F23ImpliciteSiPolitici.cs`, `nou/.../WebApi/API/Conta/ImpliciteController.cs`, `nou/.../WebApi/API/Conta/PoliticiController.cs`, `nou/Atlas.Conta.Client/src/felii/politici/`, `nou/tools/ProbeHttp/refuzuri.ps1` (blocul „politici")

## Regula durabilă

**Felia 23 — implicitele de culegere + întreținerea politicilor.** (a)
Trei feluri de implicite, aplicate la CULEGERE, o singură sursă
(`ImpliciteService`): de SUBIECT (`Partener/Produs.TipTvaImplicit`, ca
`ContImplicit`), de POLITICĂ (`PoliticaTvaImplicit`, tabel tipizat cu
prioritate declarată), de SESIUNE (al clientului); doar pe linia NOUĂ fără
valoare, absența pe linia existentă = golire (56). Produsul liniei prin
`DocumentDetaliu.ProdusCules()` polimorf; partenerul prin `Any` pe
nomenclator (`GetObjectByKey<Partener>` sub TPT aruncă pe proxy-ul altei
frunze). (b) **Regimul e al partenerului, cota e a produsului**:
`ClasaFiscala.APartenerului` (funcția legii din D394, mutată în `Comun`);
R = override-ul partenerului → rândul cel mai SPECIFIC → ancora tipului;
P = implicitul produsului; rezultat = P dacă același regim, altfel R ?? P;
tipul INACTIV sare treapta cu motiv; partener lipsă/inexistent/invizibil =
UN text (80a). Seed privat doar ce e sigur în lege (FCL/RDC × UE/extra-UE
→ SDD, FCT/RLF × UE → TI21); bugetar zero. (c) Unicitatea în SCHEMĂ: 15
indexuri unice filtrate `GCRecord = 0` (politicile per tip, `RegulaStoc`/
`RegulaContare`/`PoliticaTvaImplicit` cu `NULLS NOT DISTINCT`, codurile
nomenclatoarelor, `Cont.Simbol`); `TipTva.Activ` (lookup-urile filtrează;
N19/TI19/CAP19 inactive din seed). (d) **Proveniența = `DinSeed`**
(`ICuProvenienta`, 17 tipuri): seed-ul timbrează DOAR ce creează, backfill
o singură dată în migrație, gardianul stinge la EDITARE și refuză
`DinSeed = true` pe nou; timbrul stins supraviețuiește re-seed-ului. (e)
Ușa OData se DESCHIDE pe politici (12 + `PoliticaTvaImplicit` + `TipTva`;
amendează 56), cu invarianții în `GardianEditare` (`TipDocument` = ancoră
read-only; cotă ∈ [0,100]; dezactivare refuzată cât e implicit; `Explicit
⇒ cont`; `Format` cules compunabil; `MapareD300/D394` prin aceleași funcții
ca atributele). (f) `GET api/implicite/tip-tva` pe ușa SECURIZATĂ: `User`
⇒ `Niciuna` (ce nu vezi nu-ți poate fi propus); clientul arată SURSA sub
câmp, zero calcul în TS. (g) `GET api/politici/verificare` = raportul de
profil (manuale, FK spre șterse, inactiv referit, tip fără ancoră, goluri
D300/D394): gate `CanRead` pe toate tipurile citite + calcul pe ușa
NON-SECURED (pe cea filtrată raportul era FALS, nu gol; 73g/80e). (h)
Auditul MĂSURAT pe OData, istoric per rând (`AuditedObject/TypeName+Key`,
`$expand=UserObject`). (i) `GrilaPolitica`: citire prin `storeOData`,
scriere prin `http.ts` (mesajul `EroriDto` în rând; 80-r1), coloane COD
per politică; 7 ecrane + lookup `TipTvaImplicit` pe Partener/Produs
(închide 79-r2, 74-r12, 77-r3 pe `PoliticaMiscareSaft`). (j) Blocul
„politici" în `refuzuri.ps1` (80 de probe). (k) Felia 24 = „Explică" +
restul ecranelor de politici. Restanțe 81-r1…r9 → jurnal.

## Context

Discuția de arhitectură din 2026-09-02 („extindere în cod vs. în date; cum
definim valorile implicite") a constatat că granița cod/date (decizia 4) e
respectată, dar că două lucruri lipsesc și sunt legate prin aceeași ușă:

- **Implicitele de culegere** aveau un singur mecanism pe o singură axă:
  `TipDocument.TipTvaImplicit`, aplicat la culegere pe ambele uși (38d). Cazul
  real — livrare intracomunitară, produs cu cotă redusă, partener cu regim
  propriu — cădea mereu pe N21, iar operatorul corecta de mână.
- **Politicile pe OData** erau toate `ReadOnly`, cu motivul „ar fi politică
  editată pe ușa din dos, în afara oricărei validări de profil". Motivul a
  expirat la 77g/77k/80c: gardianul stă pe ușa COMUNĂ. Ce rămâne adevărat e
  că regulile XAF Validation nu rulează pe API (55b), deci fiecare politică
  deschisă cere invarianții ei în `GardianEditare`.
- **Nouă politici per tip de document** n-aveau index unic, deși motorul le
  citește cu `FirstOrDefault`: un al doilea rând pe același tip, creabil azi
  din XAF, făcea motorul NEDETERMINIST, tăcut. Nici codurile nomenclatoarelor
  (`TipTva.Cod`, `Cont.Simbol`, …) nu erau unice, deși seed-ul le tratează ca
  chei, iar `Cautare.cs` afirma că indexurile există.
- **Proveniența rândurilor** nu exista: patru semantici de re-seed și niciun
  marcaj seed vs. manual — un rând seed-uit și editat de client rămânea
  divergent pentru totdeauna, invizibil.
- **Gardianul** acoperea din 12 politici doar `PoliticaMiscareSaft`.
- **Auditul pe OData** era corect prin construcție, dar nemăsurat, iar
  `AuditDataItemPersistent` nu era expus nicăieri.

Mecanica de dedesubt, măsurată pe surse: `TipTva` = cotă × regim; clasa
fiscală a partenerului există deja ca funcție a legii (`D394Proiectii.
TipPartener`, 71b) peste `TariUe`; Postgres 18 ⇒ `NULLS NOT DISTINCT`
(Npgsql EF 10.0.3 `AreNullsDistinct(false)`); `GardianEditare.Verifica` e
`public static` și probabil direct în ModelCheck.

## Decizia

**(a) Trei feluri de implicite, aplicate la CULEGERE, o singură sursă.**
Implicit de SUBIECT = câmp pe nomenclator (`Partener.TipTvaImplicit`,
`Produs.TipTvaImplicit`, ca `ContImplicit`); implicit de POLITICĂ = tabel
tipizat cu cheie de potrivire și prioritate declarată (`PoliticaTvaImplicit`);
implicit de SESIUNE (data de azi, ultima gestiune) rămâne al clientului. Toate
se aplică o singură dată, pe linia NOUĂ fără valoare; pe linia existentă
absența e golire deliberată (56 neatins). Sursa unică e `ImpliciteService`
(static, FK-uri + `IObjectSpace`), cu trei apelanți: controllerul XAF, cele
cinci Apply-uri (prin wrapper-ul `TvaService.AplicaTipTvaImplicit`, semnătură
neschimbată) și endpoint-ul de citire. Produsul liniei vine prin contractul
polimorf `DocumentDetaliu.ProdusCules()` (cinci frunze îl suprascriu), nu prin
`is`; partenerul documentului prin întrebarea pusă NOMENCLATORULUI
(`Any` pe `Partener` pe cele două laturi — **nu `GetObjectByKey<Partener>`, care
sub TPT face cast pe proxy-ul altei frunze și aruncă**; găsit pe host de regula
de oprire, cu proba care MATERIALIZEAZĂ laturile).

**(b) Regimul e al partenerului, cota e a produsului.** `ClasaFiscalaPartener`
(`InregistratRo`/`NeinregistratRo`/`Ue`/`ExtraUe`) = valorile lui
`TipPartener` din D394, mutat în `Comun/ClasaFiscala.APartenerului` (D394
deleagă, cifre neschimbate — probate). `PoliticaTvaImplicit(TipDocument ×
ClasaFiscala? × ValabilDeLa?) → TipTva`, index unic `NULLS NOT DISTINCT`
filtrat `GCRecord = 0`. Rezolvarea: **R** (regimul) = override-ul partenerului
→ rândul de politică cel mai SPECIFIC (clasa exactă bate `null`; la egalitate
`ValabilDeLa` cel mai recent, „dintotdeauna" pierde) → ancora tipului; **P**
(cota) = implicitul produsului; rezultatul = `P` dacă `P.Regim == R.Regim`
(pâinea rămâne 11% de la orice furnizor înregistrat; `CAP11` bate `CAP21`
la fel), altfel `R ?? P` (livrarea intracomunitară e scutită indiferent de
produs). Un tip INACTIV sare TREAPTA (nu se caută „al doilea cel mai specific
rând" — ar fi un al doilea mecanism ascuns), cu motiv. Partenerul lipsă,
inexistent sau invizibil primesc UN SINGUR text (fără oracol de existență,
80a). Seed privat DOAR ce e sigur în lege: `FCL/RDC × Ue/ExtraUe → SDD`,
`FCT/RLF × Ue → TI21`; achiziția extra-UE și de la neînregistrat RO rămân
NEseed-uite, declarate (restanță). Bugetar: zero rânduri.

**(c) Unicitatea și viața nomenclatoarelor de politică intră în SCHEMĂ.**
`TipTva.Activ` (lookup-urile de culegere filtrează; un tip inactiv pe o linie
existentă e istorie legitimă; `N19`/`TI19`/`CAP19` inactive din seed). 15
indexuri unice filtrate `GCRecord = 0`: cele 6 politici pe `TipDocumentId`,
`RegulaStoc(Tip, Latura, ClasaId)` și `RegulaContare(Tip, TipMaterialId,
NaturaFiltru, SemnFiltru)` și `PoliticaTvaImplicit` cu `NULLS NOT DISTINCT`,
`TipDocument(Cod)`, `TipDocument(ClrType)`, `TipTva/ClasaProdus/TipMaterial
(Cod)`, `Cont(Simbol)`. Interogarea de dubluri a rulat ÎNAINTE pe 6 baze de
dev: 0. `Repartitor.Cod` NU intră (spațiu partajat pe TPT; restanță).
`Cautare.cs` spune acum adevărul.

**(d) Proveniența = `DinSeed`, scris de seed la CREARE, șters de gardian la
EDITARE, backfill o singură dată în migrație.** `ICuProvenienta` pe cele 12
politici + `PoliticaTvaImplicit` + `TipTva`/`Cont`/`ClasaProdus`/`TipMaterial`
(17 tipuri; nu pe repartitori, nu pe `Societate`). Forma din contract („seed-ul
marchează și ce GĂSEȘTE") a fost RESPINSĂ la pasul 1: re-marcarea la
`--forceUpdate` ștergea exact divergența pe care flag-ul există s-o arate.
Forma finală: seed-ul timbrează DOAR rândurile pe care le creează; migrația
`F23ImpliciteSiPolitici` face `UPDATE … SET DinSeed = TRUE` pe cele 16 tabele
existente („tot ce exista la migrație e considerat livrat"; `PoliticiTvaImplicit`
e creată goală de aceeași migrație); gardianul, pe ușa securizată, stinge
timbrul pe orice obiect existent modificat și REFUZĂ `DinSeed = true` pe obiect
nou. Probat: timbrul stins supraviețuiește re-seed-ului. Limite declarate: un
rând editat manual ÎNAINTE de migrație e marcat seed (nedetectabil); un PATCH
care nu schimbă nimic nu ajunge la gardian (EF nu are obiect modificat — nici
plasa de permisiuni nu rulează, 204 și pentru `Cititor`; nimic nu se scrie);
`DinSeed = true` retrimis pe rând existent se STINGE (204), nu se refuză.
Semantica de re-seed nu s-a schimbat („seed-ul corectează doar rândurile
`DinSeed`" = restanță cu nume).

**(e) Ușa OData se deschide pe politici, cu invarianții în gardian.** CRUD pe
cele 12 politici + `PoliticaTvaImplicit` + `TipTva`; `ClasaProdus`,
`AuditDataItemPersistent`, `AuditEFCoreWeakReference` `ReadOnly`; rămân
`ReadOnly` cu motiv propriu `Cont` (planul de conturi rămâne în XAF, 70g),
`ContPropriu`, `UnitateInterna`, legea (`RandD300`/`Judet`/`UnitateMasura`),
`Lot`, clasificația bugetară, `Repartitor`. Ramurile noi din `GardianEditare`
(regula rămâne și ca atribut XAF unde există — două jumătăți de prezentare, o
regulă de fond, 77k): proveniența; `TipDocument` = ancoră (rând nou refuzat,
`Cod`/`ClrType` server-owned, ștergere refuzată); `TipTva.Cota ∈ [0,100]`,
dezactivarea refuzată cât timp e implicit undeva (cu lista referințelor);
`PoliticaTvaImplicit` (țintă activă, MESAJUL unicității înaintea constraint-ului);
`PoliticaTva`/`RegulaContare` `Explicit ⇒ cont`, `SemnFiltru ∈ {−1,null,+1}`,
`TipMaterial ⇒ fără NaturaFiltru`; `RegulaStoc.Semn ∈ {−1,+1}`;
`PoliticaNumerotare`: `Serie` nevidă, `UrmatorulNumar ≥ 1`, **`Format` cules
compunabil** (nu „nevid" — `Format` e opțional în motor și seed-ul nu-l scrie;
deviere de la contract, măsurată: regula literală ar fi refuzat propriile
rânduri ale seed-ului); `PoliticaScadenta.ZileDefault ≥ 0`;
`PoliticaInchidereTva` toate patru conturi sau niciunul; `MapareD300`
(`EsteDeOperatiuni`/`FaraAscendentMapat` = funcții statice chemate ȘI de
atribut, ȘI de gardian); `MapareD394.TintaPermisa`. Capcană pentru ecranele
viitoare: `SursaCont.Explicit == 0`, deci un rând nou de `RegulaContare`/
`PoliticaTva` pornește „Explicit fără cont" și e refuzat la prima salvare dacă
formularul nu propune altă sursă.

**(f) Endpoint-ul de implicite: serverul spune, clientul arată.** `GET
api/implicite/tip-tva?tipDocument&partenerId&produsId&data` pe ușa
SECURIZATĂ (nicio cifră de registru; maparea cod → ancoră pe ușa non-secured ca
400 să însemne doar „cod necunoscut", nu „nu-ți e vizibil"). Consecință
declarată (amendează F23-D6): `User`, care nu vede nomenclatoarele, primește
`Sursa = Niciuna`, nu implicitul generic — ce nu vezi nu-ți poate fi propus, și
e exact ce ar face și PUT-ul lui pe același OS securizat; mutarea rezolvării pe
ușa non-secured ar fi precompletat un tip pe care salvarea nu-l scrie. Clientul
cere implicitul pe linie NOUĂ cu `TipTva` gol la deschidere și la schimbarea
produsului, precompletează perechea (id, etichetă) prin convenția 77c și
arată SURSA sub câmp; zero calcul în TS (43b).

**(g) Raportul de verificare a profilului** (`VerificareProfilService.
Raporteaza`): rândurile manuale (`DinSeed = false`, plafon 200/tabel cu
rezumat), FK-uri de politică spre rânduri șterse logic, `TipTva` inactiv referit
ca implicit, tip cu `PoliticaTva` fără ancoră, golurile de mapare D300/D394
(seed-ul le ARUNCĂ prin aceeași funcție `GoluriMapari*`, raportul le ARATĂ).
**Ruta `GET api/politici/verificare` cere `CanRead` pe TOATE tipurile citite
(17 `ICuProvenienta` + `Partener` + `Produs`, prin reflecție) și calculează pe
ușa NON-SECURED** — pe cea filtrată raportul era FALS, nu gol (`User` primea 20
de constatări inventate: „N21 nu există în bază"; clasa 73g/80e). Amendează
F23-D8: `User` ⇒ 403, nu 200 gol; 69g rămâne al listelor.

**(h) Auditul se MĂSOARĂ pe OData și devine istoric per rând.** Rândurile
`AuditDataItemPersistent` apar pe scrierile OData (probat pe HTTP), se
filtrează pe `AuditedObject/TypeName` (numele CLR complet) + `AuditedObject/Key`
(Guid ca string), utilizatorul din `$expand=UserObject`
(`UserObject.DefaultString`); rolul `Default` vede doar rândurile proprii.
Rândurile de audit produse de probe rămân (un jurnal nu se șterge).

**(i) Grila comună de politică în client** (`GrilaPolitica`): `DataGrid` cu
editare pe rând peste un `CustomStore` care CITEȘTE prin `storeOData` și SCRIE
prin `api.ts` (`http.ts`), ca mesajul `EroriDto` să ajungă în rând (pe conducta
`ODataStore` s-ar pierde, 80-r1); coloanele sunt COD per politică (43a);
`DinSeed` = coloană read-only „seed / manual"; panoul „Istoric" din (h).
Ecranele feliei: `PoliticaTvaImplicit`, `TipTva`, `TipDocument` (doar
implicitul), `PoliticaMiscareSaft` (editabilă — 77-r3 pe itemul ăsta),
`PoliticaScadenta`, `PoliticaNumerotare`, `PoliticaInchidereTva` (79-r2); pe
`Partener`/`Produs` un lookup `TipTvaImplicit` filtrat `Activ`.

**(j) Securitatea se măsoară pe HTTP** (80i): blocul „politici" din
`refuzuri.ps1`, fără urme, cu probele care SCHIMBĂ o valoare (un PATCH identic
e vacuu — precedentul m4/80) și proba proveniență DOAR pe rând nou.

**(k) Ce NU intră** (felia 24): implicitele pentru laturi, publicarea felului
laturilor în metadata, „Explică" (rezolvarea motorului pe un context — cere
extragerea potrivirii din `MotorOperare` ca funcție pură, ceea ce reduce
oglinzile de mână din 64), ecranele `RegulaContare`/`RegulaStoc`/`MapareD300`/
`MapareD394`/`PoliticaTva`/`Conex`/`Validare`, seed-ul care corectează
`DinSeed`, export/import de politici între baze, rolul „Configurator",
`Repartitor.Cod` unic, XAF (filtrul `Activ`, baseline-ul pe cele 8 politici),
`Cont` scriibil.

## Review advers și probe

Închidere 2026-09-09 (pașii 5–6). Scenariile din contract, față de probele
comise în pașii 1–4 și una nouă:

| Scenariu | Verdict |
|---|---|
| Implicitul suprascrie o alegere (client sau PUT) | nu: în client update funcțional pe linia NOUĂ cu `TipTva` gol (pas 4, smoke); în `Apply` doar liniile fără `TipTvaId` în payload (56, neatins) |
| `Partener.TipTvaImplicit` inactiv | sare TREAPTA cu motiv (F23-V2) |
| Două rânduri de politică cu `ValabilDeLa` egal | refuzate: mesajul gardianului înaintea indexului `NULLS NOT DISTINCT` (F23-V4, pas 3 pe HTTP) |
| `DinSeed` reînviat printr-un PATCH fără schimbare | nu se scrie nimic — nici gardian, nici plasa de permisiuni (81-r5) |
| Gardianul vede un rând ȘTERS LOGIC ca dublu | **nu** (probă nouă F23-V4/„șters logic", ambele profiluri): gardianul și indexul citesc doar `GCRecord = 0`, cheia se reface, rândul șters rămâne în tabelă |
| `TipDocument` nou prin OData | 422 „ancoră" (pas 2/3) |
| Auditul atribuit altui utilizator | `UserObject` = utilizatorul apelului, măsurat pe HTTP (pas 3) |
| `User` ca oracol de existență prin `implicite` | motiv IDENTIC pentru partener lipsă / inexistent / invizibil (F23-V2 + pas 3) |
| `Cititor` pe `verificare` vede `RandManual` | 200 cu constatările societății — e Read, asumat; `User` ⇒ 403 (g) |

Defecte de fond găsite pe parcurs de regula de oprire, nu de review: `GetObjectByKey<Partener>`
sub TPT (pas 2, fixat cu probă care materializează laturile); raportul de profil
FALS pe ușa securizată (pas 2, amendează F23-D8). Devieri de la contract:
F23-D4 (seed-ul timbrează doar ce creează), F23-D5 (`Format` compunabil, nu
nevid), F23-D6 (`User` ⇒ `Niciuna`), F23-D8 (`User` ⇒ 403).

Cifre: ModelCheck privat 978 / bugetar 927, 0 FAIL (F23-V1…V6 + proba nouă);
`refuzuri.ps1` 80 PASS / 0 FAIL; smoke în browser (a)–(g) + `Cititor` + `User`
PASS; `verifica:drift` verde.

## Ce rămâne deschis

- **81-r1** achiziția extra-UE (import cu TVA în vamă) și achiziția de la
  neînregistrat RO n-au rând de politică implicită: cad pe ancora N21.
  Importul e document propriu (familia 36f); neînregistratul cere o decizie
  proprie (linia n-are fapt de TVA — `null`? `NIM`?).
- **81-r2** seed-ul nu corectează rândurile `DinSeed` („insert dacă lipsește"
  rămâne): un rând al profilului care s-a schimbat de la livrare rămâne în
  forma primei seed-uiri pe bazele existente; flag-ul face corecția posibilă,
  nu o și face.
- **81-r3** un rând editat manual ÎNAINTE de migrația F23 e marcat seed de
  backfill (nedetectabil).
- **81-r4** `Repartitor.Cod` fără unicitate în schemă (spațiu partajat pe TPT
  între parteneri/gestiuni/angajați; bazele de import pot avea coliziuni).
- **81-r5** PATCH-ul OData fără schimbare trece 204 pentru orice rol (EF nu
  are obiect modificat ⇒ nici gardian, nici plasa de permisiuni); nimic nu se
  scrie, deci nu e gaură — dar e capcană de probă.
- **81-r6** `User` pe `api/implicite/tip-tva` primește `Niciuna` (nu vede
  nomenclatoarele): implicitul e al ușii securizate; un rol cu drept pe
  documente dar fără Read pe `TipTva` nu poate culege TVA oricum.
- **81-r7** XAF: lookup-urile de `TipTva` nu filtrează `Activ`; baseline-ul UI
  lipsește pe 8 politici (FK-uri brute în grile) — asumat (44/53).
- **81-r8** `SursaCont.Explicit == 0`: ecranele viitoare de `RegulaContare`/
  `PoliticaTva` trebuie să propună sursa pe rând nou (felia 24).
- **81-r9** `400 "Incorrect body."` englezesc pe POST-urile OData cu corp care
  nu se leagă (80-r6, deja declarată).
