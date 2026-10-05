# TR-D9a — tăierea registrelor și a regimului dual: contract pentru aprobare

- Data: 2026-10-05
- Stare: **draft, neaprobat**. D9-Q1…D9-Q4 sunt tranșate de owner,
  2026-10-05 (A/A/A/A). Așteaptă review-ul advers Codex al contractului și
  aprobarea owner-ului, înaintea codului.
  Branch `tr-d9-taierea`, tăiat din `main` = `0bcd8b0` (după PR #21).
- Bază: 090 (a)(l)(m) și rândul TR-D9 din „Ordinea și regula de oprire";
  091 (g)(6), (h), (k); 108 (a) și „Ce rămâne deschis"; lista nominală X-D2
  (`nou/tools/ModelCheck/ProbeCititoriRegistre.Lista.cs`); restanțele cu
  starea `cade la TR-D9` și `activă, TR-D9` din `docs/decizii/restante.md`.
- Închiderea: decizia **110**.

## De ce două felii (D9-Q1 = A)

TR-D9 din 090 este „unitățile și tăierea". Cele două jumătăți au naturi
opuse:

- **tăierea** scoate cod și nu adaugă comportament contabil. Cubul e deja
  singura sursă citită (108 a); rămâne de scos al doilea scriitor și tot ce
  există numai pentru el;
- **unitățile** adaugă comportament nou: reevaluarea cu `Atribuit`, stornoul
  după reevaluare (091 h, N-r5), reziduul RLF (T-r2), costul vamal pe lot
  (86-r1), valuta pe partidă (B-r6, 107-r4), `PoliticaEvaluare` (51e). Se
  scriu scenarii-întâi.

Făcute împreună, reevaluarea ar trebui scrisă și în registre sau regimul dual
ar trebui rupt la mijlocul feliei, iar review-ul n-ar putea separa „am scos"
de „am schimbat". Tranșat de owner: **TR-D9a = tăierea** (contractul de
față, închide 091 (g)(6)); **TR-D9b = unitățile** (contract propriu, după,
închide 091 (g)(3)). „Rotund" le cere pe amândouă.

## Ce spun sursele că înseamnă tăierea

| Sursa | Cerința | Litera de aici |
|---|---|---|
| 090 (a) | dispar cele patru registre, `Document.TotalStingere`, `TipStoc` ca cheie de registru, flag-ul de storno pe rând, `NumarNota`, `Lot.PretUnitar` ca fapt înghețat | D9-D2, D9-D3, D9-D6, D9-D8 |
| 090 (l) | tăierea e greenfield, fără migrare de date; lanțul de migrații se resetează | D9-D6 |
| 090 (m) | din XAF mor cele patru grile pe registre; re-țintirea pe `Postare` e dezghețarea declarată | D9-D9 |
| 090 rândul TR-D9 | motorul vechi dispare; ModelCheck cu probele de formă rescrise; `--dump-integritate-tph` zero; `verifica:drift` zero sau driftul declarat | D9-D2, D9-D7, D9-D15 |
| 091 (g)(6) | citirile de bază din cub și registrele vechi tăiate, fără migrare | D9-D1 |
| 091 (c)(f) | oracolul normalizat nu mai e probă; Import1C compilează, nu se extinde | D9-D7, D9-D11 |
| 108 (a) | clasa 1 a listei nominale (scriitorii duali) și evidența XAF cad la TR-D9 | D9-D2, D9-D9 |
| 108, amendamentul `PartideCuRest` | criteriul de formă amânat la TR-D9, odată cu F27-r16 | D9-D10 |
| X-r2 | subiectul permisiunii pe cifrele citite din cub, după tăierea tipurilor de registru | D9-D9 |
| TR-r9 | inventarul rescrie/șterge al probelor de formă | D9-D7 |
| restanțele `cade la TR-D9` | mecanismul dispare cu registrele; nevoia rămasă se rejudecă pe cub | D9-D13 |

## Fapte măsurate (main `0bcd8b0`, 2026-10-05)

Cifrele sunt numărate pe sursă, nu estimate; inventarul nominal complet e
livrabilul pasului 1.

**Producție.**

- Lista nominală X-D2 are 75 de intrări: 26 scriitor dual, 7 martor, 2
  evidență XAF, 4 autorizare, 6 mapare, 30 legătură (`Imperechere`, rămâne).
  Scriitorii duali stau în 6 fișiere: `MotorOperare` (3), `CorectieService`
  (1), `GardianEditare` (1), `StocService` (7), `Fapte` + `DeclarantAsamblare`
  (3, absorbția ASM-B6), `Documente/Imobilizari.cs` (11).
- `MotorOperare.CalculeazaSiValideaza` construiește încă planul registrelor:
  mișcările de stoc pe `RegulaStoc`, notele pe `RegulaContare`, pasul TVA,
  `RegistruTvaService.Deriva`. Trei lucruri din el **nu** le face contractul
  cubului singur:
  1. scrie `Valoare` pe linia de ieșire din soldul REGISTRULUI
     (`StocService.AplicaValoareIesire`); declaranții evaluează separat, din
     soldul cubului (`N.Evaluare.Iesire`), deci linia și postarea au azi două
     surse;
  2. verifică dimensiunile obligatorii per cont pe notele registrului
     (`VerificaDimensiuniObligatorii`). Pe partea cubului, `VerificaLatura` e
     chemată numai de `ReceptiiConexe` și de `Materializare.Deschidere`;
     `Declaratii/`, `Contractare` și nucleul nu au gardul;
  3. fixează `Lot.PretUnitar`, citit apoi de doi declaranți (ASM pentru
     absorbție, NIR), de `Fapte.LotFapt`, `DescarcareService`, estimarea de
     draft (`BazaLinie`, `ProdusLot`) și de nouă DTO-uri (`LotPret`).
- `PosteazaInCub`: 16 apariții în 6 fișiere. Seed-ul are 20 de tipuri; BPR
  nu declară (rezervat, 19), iar DSC, ITV, RDC, RLF, DVI sunt inerte pe
  bugetar (fără politici, `PosteazaInCub = false`).
- `Document.TotalStingere`: scrisă de motor la operare, citită în 6 locuri
  de `ImperecheriProiectii.DocumenteCuRest`, păzită de gardian.
- Hook-urile de stingere (`SensDeStins`, `PoateFiStins`, `CapacitateStingere`,
  `SursaStingeriiAutomate`): 8 apeluri în producție (`ImperechereService` 6,
  `NotaContabilaApply` 1, `ImperechereApply` 1). Nu citesc registre.
- `RegulaStoc` mai e consumată, în afara planului vechi, de LDI
  (`DeclarantDiferenteInventar`), DSC (`DescarcareService`, `FacturaIesire`),
  `Fapte` (operand) și „Explică". `TipStoc` trăiește și ca
  `Cont.CategorieStoc` și cheie a `PoliticaMiscareSaft`.
- `NumarNota`: coloană pe `RegistruContabil`; proiecțiile contabile de pe cub
  pun deja numărul documentului în câmpul cu acest nume.
- Autorizare: `RegistrulCitibil` pe trei controllere (ITV, AMO, Imobilizări)
  și `PerioadeController.TipuriInsumate`.
- XAF: patru liste `[NavigationItem("Registre")]`, configurate în
  `ContaUiBaseline`. Blazor.Server nu referă registrele.

**ModelCheck** (71 de fișiere, 46.887 de linii).

- `Program.cs`: 33.241 de linii, 1.755 `Check(`. Referințele la registre
  sunt pe 508 linii; o parte sunt purje. Pe o delimitare euristică a
  unităților de nivel sus, 55 din 98 ating registrele, iar cele care ating
  registrele sau oracolul țin 27.996 de linii și 1.560 de `Check`. Cifra
  exactă e a inventarului.
- `Nucleu/` e oracolul registre → cub: `CubDinRegistre` (540), `Normalizari`
  (807), `ReconciliereCub` (577), `GatePeBaza` (633), `Comparabil` (187),
  `DiagnosticValoriStoc` (118), `ProbeCub` (328), `ProbeNucleu` (103).
- Scenele de catalog care folosesc registrul ca al doilea martor: 12 fișiere
  `Scenarii*.cs`, între 1 și 5 linii fiecare, plus `ScenaDocumente` (7),
  `AcoperireInvarianti` (7), `PerfCub.Evaluare` (4).

**Unelte și client.**

- Import1C: 57 de referințe la registre și la motorul vechi, în 10 fișiere
  (`ReconciliereLuna` 19, `Deschidere` 8, `Sabotaj` 8, `Alocare` 6, `Reluare`
  5). Migrare: 7. BackfillTva: 8 (unealta derivă și scrie `RegistruTva`).
- Client: `RegistruImobilizari.tsx`, `FisaCont.tsx`, `RegistruJurnal.tsx`,
  `D300.tsx`, `App.tsx` și `generated/api-types.ts`.

## D9-D1 — Perimetrul

**Intră**: scoaterea scriitorilor duali și a planului registrelor din motor
(D9-D2); valoarea liniei de ieșire dintr-o singură sursă (D9-D3); portarea
gardurilor planului vechi care nu sunt ale registrelor (D9-D4); scoaterea
regimului ca dată (D9-D5); schema, migrațiile și bazele (D9-D6); ModelCheck
fără oracol, cu probele de regulă re-țintite (D9-D7); `RegulaStoc` fără
rânduri moarte (D9-D8); evidența XAF și autorizarea (D9-D9); proiecțiile de
rest fără `TotalStingere` (D9-D10); uneltele (D9-D11); clientul și contractul
HTTP (D9-D12); verdictul restanțelor (D9-D13); documentația și invarianții
(D9-D14).

**Nu intră**: reevaluarea, `Atribuit` ca mecanism de produs, stornoul după
reevaluare, valuta, `PoliticaEvaluare`, costul vamal pe lot (TR-D9b);
re-cheierea `PoliticaMiscareSaft` pe cont (TR-r7); hostul fără XAF și async
(IM-r2, IM-r5); rafinarea blocajului scrierii (X-r1); migrarea 1C (091-r4);
orice cerință de produs nouă (090 m).

Felia nu schimbă nicio cifră postată în cub, cu o singură excepție declarată:
D9-D3 (evaluarea ieșirii nu mai trece prin soldul registrului).

## D9-D2 — Un singur scriitor: coaja comenzii după tăiere

`MotorOperare` rămâne coaja comenzii (L2). Ordinea după tăiere:

1. gardurile de stare, dată și perioadă; blocajele (108 e), neschimbate;
2. `PregatesteOperare`;
3. validările care nu sunt ale registrelor: TVA cules la taxare inversă,
   `PoliticaValidare`, `ValideazaOperare`;
4. contractul (`Contractare`), cu refuzurile lui;
5. gardurile pe postările contractului: analiza obligatorie (D9-D4), soldul
   intermediar al loturilor, poziția fără fișă, recepțiile conexe;
6. materializarea în cub, cu explicația (108 c);
7. scrierile pe document: numărul, scadența, loturile născute, valoarea
   liniilor de ieșire (D9-D3), conexul, secundarul, starea;
8. împerecherea automată la operare.

`Valideaza` (dry-run) rulează 1–5 și nu scrie nimic. Un refuz din orice pas
anulează întreaga tranzacție de comandă.

Dispar: `PlanOperare.Miscari`, `Note` și `RanduriTva`; `PotrivesteReguliStoc`;
pasul de note și pasul TVA din plan; `RegistruTvaService.Deriva` și rândul
`RandTva`; cele trei scrieri de registru din `Opereaza`, `AnuleazaOperarea`
și `Storneaza`; `IDocumentCuRegistruPropriu` cu cele 11 metode din
`Documente/Imobilizari.cs`; ramura `RegistruTva` din `CorectieService`;
absorbția ASM-B6 (`Fapte.SolduriLoturiRegistru`, câmpul din `Operand`, ramura
din `DeclarantAsamblare`); `StocService` cu cheia `(Lot, Repartitor, TipStoc)`,
în afară de ce pasul 1 dovedește că e regulă pură cu un consumator rămas.

Ce din serviciile vechi e regulă cu consumator viu rămâne; inventarul numește
membru cu membru ce rămâne din `RegistruTvaService`, `AmortizareService`,
`InchidereTvaService`, `DescarcareService` și `LoturiCulegereService`. Un
membru fără apelant după tăiere se șterge.

## D9-D3 — Valoarea liniei de ieșire e a contractului

**Regula.** Pe o linie care scoate stoc, `Valoare` este decizia
`ValoareIesire` a contractului: evaluarea pe soldul cubului, în secvența
liniilor, ultima ia restul (090 j). Coaja o scrie pe linie la operare, în
tranzacția comenzii. Linia operată și postarea ei poartă aceeași valoare;
egalitatea e invariant `INV-CUB`, cu mutant.

**Sursele declarate rămân.** Ieșirea a cărei valoare nu vine din sold
(`ValoareDeclarata`, 108 c: RLF și RDC din linie, NIR-minus din recepție) nu
se schimbă.

**Draftul.** Valoarea afișată înaintea operării e estimare din raportul
curent al unității, citit prin `Citiri.Loturi` într-o singură interogare per
document. Dry-run-ul arată valoarea pe care operarea ar scrie-o.

**ASM.** Absorbția Δ față de registru dispare. Ținta 0/0 din ASM-B7 devine
probă: trei BCS sau ASM de câte 1 dintr-un lot 3/10 lasă lotul la 0/0, nu la
0/−0,01. Validările de frunză care judecă valoarea finală (invariantul
valoric ASM) se mută după contract sau în declarant; se tranșează la pas, cu
scenariu.

**`Lot.PretUnitar`.** Declaranții și `Fapte.LotFapt` nu-l mai citesc. Coloana
dispare (090 a) dacă după portare nu rămâne consumator; altfel rămâne numai
ca preț de intrare cules, cu consumatorul numit și restanță. DTO-urile
`LotPret` își păstrează numele pe sârmă și iau raportul curent.

**Ce se închide.** T-r13 și T-r7: bazele se recreează (102 b), deci nu există
istoric divergent de tratat; SC-ASM-26 (stornoul unui original dual) rămâne
fără obiect și se marchează așa în catalog. Limita „golirea se decide la
momentul operării" (retro-ul lasă reziduu) rămâne, declarată; eliminarea ei e
reevaluare (TR-D9b).

Aceasta e singura schimbare de cifră a feliei. Scenariile care o probează se
scriu înaintea codului (pasul 2) și se listează în catalog cu cifra veche și
cea nouă.

## D9-D4 — Gardurile planului vechi care supraviețuiesc

**Regula.** Fiecare refuz al planului vechi are, înaintea ștergerii, unul din
două verdicte scrise în inventar: *echivalent pe contract* (cod de refuz +
scenariu care îl probează) sau *al registrului, moare*. Niciun refuz nu
dispare fără rând în tabel.

Cunoscute înaintea inventarului:

| Refuzul de azi | Verdict |
|---|---|
| dimensiunile obligatorii per cont, pe nota de registru | trece pe postările contractului (cont × latură × analiză); maparea `Repartitor` → partener sau gestiune și `Material` → produs e cea deja folosită de `Materializare.Deschidere`; cod de refuz propriu, scenarii pe bugetar |
| contul debitor/creditor nu se poate rezolva | echivalent în `Contari.Rezolva`; de confirmat pe fiecare tip |
| linia intră în regulile de stoc fără lot | echivalent în declaranți (`LotLipsa`); de confirmat pe fiecare tip |
| tipul de TVA șters sau fără cont de TVA | de confirmat în `Declaratii/Fiscal` |
| soldul negativ pe cheia de registru | moare; soldul cubului e păzit de `Loturi.VerificaSoldIntermediar` |
| `NirRegimInactiv` | moare cu regimul |
| linia fără regulă de contare, sărită tăcut de planul vechi | deja refuz în declaranți (B-r10) |

Gardul se adaugă pe partea contractului **înaintea** tăierii (pasul 2), cât
timp planul vechi încă rulează: aceleași documente trebuie refuzate de
amândouă.

## D9-D5 — `PosteazaInCub` dispare; tipul care nu declară

Coloana, alinierea din seed, `Materializare.AreTranzactii`, gardul
`POSTEAZA_IN_CUB_IREVERSIBIL` și ramurile `if (!tip.PosteazaInCub)` dispar.
108 (h) rămâne fără obiect; amendamentul se consemnează în 110. S-r6 se
închide.

Orice tip operabil declară. Operarea unui tip fără declarant (BPR) sau fără
politică pe profil (DSC, ITV, RDC, RLF, DVI pe bugetar) este refuz cu un
singur cod, pe toate ușile. Ce se întâmplă azi la operarea unui tip inert pe
bugetar se măsoară la pasul 1; dacă azi trece, refuzul e schimbare de
comportament declarată, cu scenariu.

Ramura `IMPERECHERE_FARA_EFECT` pentru „ambele documente trebuie să posteze
în cub" dispare; refuzul pe efect nul rămâne (108 i).

## D9-D6 — Schema, migrațiile, bazele

**Dispar**: tabelele `RegistruContabil`, `RegistruStoc`, `RegistruTva`,
`RegistruImobilizari`; coloanele `Document.TotalStingere`,
`TipDocument.PosteazaInCub`, `TipDocument.LaturaContPropriu` (T-r9),
`Lot.PretUnitar` (condiționat de D9-D3).

**Rămân**: `Tranzactie` și `Postare`; `SoldPerioadaContabil` și
`SoldPerioadaStoc` (snapshot-urile cubului, 088, 103); `Imperechere`
(legătura, 101); `RegulaStoc` și `TipStoc` în forma din D9-D8.

Lanțul de migrații se resetează într-un singur `InitialCreate`, pe
precedentul C102 (102 e). SQL-ul cubului — partițiile, cheia compusă,
FK-urile per partiție, indecșii cititorilor, explicația — se pliază în el,
scris de mână (S-r4). `STR-SCHEMA` rămâne proba divergenței declarate dintre
snapshot-ul EF și bază. Bazele de dezvoltare se recreează (102 b); nu există
migrare de date.

Dacă nu cere mai mult decât scoaterea din model, `HCategory` iese din
`DbContext` în același reset (106-r5); altfel rămâne restanță.

## D9-D7 — ModelCheck: oracolul moare, probele de regulă se re-țintesc (TR-r9)

(a) **Oracolul dispare.** `Nucleu/CubDinRegistre`, `Normalizari`,
`Comparabil`, `ReconciliereCub`, `GatePeBaza`, `DiagnosticValoriStoc`,
modurile `--reconciliere-cub` și `--declaratie-pe-baza`, `STR-RECONCILIERE`,
acoperirile registru → cub din `INV-CUB` (contabilă, cantitativă de stoc, a
fișelor) și mutanții lor. Nu mai există al doilea scriitor de comparat.
108 (g) și (j) se amendează în 110. Invarianții interni ai cubului rămân:
echilibrul, conservarea transferului, explicația, taxa postată = taxa liniei
(109), plus cel nou din D9-D3.

(b) **Probele pe sursă.** Lista nominală X-D2 se reduce la clasa `Legatura`.
Se adaugă lista numelor interzise în `nou/` (cele patru registre,
`PosteazaInCub`, `TotalStingere`, `IDocumentCuRegistruPropriu`, `StocService`,
`CubDinRegistre`): reapariția unuia pică `--probe-sursa`. Proba cititorilor
de `Postare` (091-r3) și a blocajului scrierii rămân neschimbate.

(c) **Trei verdicte pe fiecare referință** din scenele `Program.cs` și
`Scenarii*.cs`, în inventarul pasului 1:

- *purjă* — linia dispare;
- *formă* — aserția moare: numărul de rânduri de registru, `NumarNota`,
  flag-ul `Storno`, cheia `TipStoc`, egalitatea registru = cub;
- *regulă* — aserția se re-țintește pe intrarea comună a cubului, cu aceeași
  cifră: `Citiri.Contabil`, `Loturi`, `Partide`, `Fiscale`, `Imobilizari`.

O aserție de regulă nu se șterge pe motiv că e „acoperită de catalog" fără
să fie numit rândul `SC-…` care o acoperă.

(d) **Re-țintirea se face sub regimul dual** (pasul 3), înaintea tăierii.
Cât timp registrele încă se scriu, aserția veche și cea re-țintită rulează pe
aceeași scenă și dau aceeași cifră; abia apoi cea veche se șterge. O aserție
care nu se poate re-ținti fără altă cifră e ori diferență declarată deja
(N-r3, T-D2.2, T-D4.2, D9-D3), ori defect: se raportează, nu se ajustează.

(e) **Contabilitatea probelor.** Numărul de `Check` înainte și după se
raportează pe scenă; fiecare scădere are rând în inventar.

## D9-D8 — `TipStoc` și `RegulaStoc` după tăiere

Cheia de registru `(Lot, Repartitor, TipStoc)` dispare. `TipStoc` rămâne
vocabular de politică: categoria contului de stoc și cheia
`PoliticaMiscareSaft`.

Un rând `RegulaStoc` fără consumator după tăiere iese din seed, pe ambele
profiluri, și din „Explică": politica nu descrie comportament pe care motorul
nu-l are (4). Consumatorii rămași se numesc în inventarul pasului 1. Dacă
după tăiere `RegulaStoc` nu mai are niciun consumator care să nu poată citi
structura din conturi, tabela dispare cu totul; altfel rămâne, redusă.

Re-cheierea `PoliticaMiscareSaft` pe `(TipDocument, Cont, Semn, Gestiune
virtuală)` din 090 (g) nu intră: rămâne TR-r7.

## D9-D9 — Evidența XAF și subiectul permisiunii (D9-Q2 = A, D9-Q3 = A)

Cele patru liste XAF ale registrelor dispar cu tipurile lor. Tranșat de
owner:

- le înlocuiește o singură listă XAF read-only pe `Postare`, în mod
  `ServerView` (85), sub „Registre", cu tranzacția, felul, documentul,
  contul, latura, partenerul, gestiunea, unitatea și cele trei măsuri; fără
  editare, fără detaliu editabil. E dezghețarea declarată de 090 (m);
- subiectul permisiunii devine dreptul de citire pe `Postare`, unic. Cele
  trei porți `RegistrulCitibil` și `TipuriInsumate` îl cer; matricea
  `refuzuri.ps1` se adaptează și rămâne 300/300. Separarea contabil /
  imobilizări de azi se pierde, declarat.

`Postare` și `Tranzactie` devin tipuri vizibile pe ușa securizată XAF. Ce
urmează din asta se pin-uiește aici, fiindcă azi cubul nu apare în UI și nu
intră în metadata clientului (S-D1):

- gardianul le refuză scrierea pe orice ușă securizată, ca azi registrelor
  (14); proba e pe HTTP, pe OData și pe REST;
- nu se expun prin OData și nu intră în metadata clientului React; proba:
  niciunul nu apare în `api-types.ts` și în `$metadata`;
- lista nu ocolește regula `Transfer` (108 b): e evidență brută, cu felul
  tranzacției ca coloană, nu raport pe cont. Nu are totaluri.

## D9-D10 — Proiecțiile de rest fără `TotalStingere` (F27-r16, F28-r1)

`ImperecheriProiectii.DocumenteCuRest` nu mai citește `Document.TotalStingere`:
totalul, restul și candidații vin din `Cub.Citiri.Partide`. Ruta și DTO-ul
rămân (React le consumă); uniunea per tip și `CandidatiPereche<T, TOpus>` se
reduc la ce cere semantica.

Criteriul de formă amânat de owner la X-D5 (`PartideCuRest`: accesul la
`Postare` nu depinde de istoric) se judecă aici: ori se închide — documentul
deschizător al partidei se citește fără parcurgerea tuturor postărilor de
partidă —, ori owner-ul îl re-amână explicit, cu cifra. Nu se închide prin
tăcere.

Hook-urile de stingere nu citesc registre și rămân neatinse în felie
(D9-Q4 = A): `ImperechereService` și cele două adaptoare le cheamă ca azi.

## D9-D11 — Uneltele

- **Import1C** rămâne înghețat (091 f): compilează, nu se extinde. Citirile
  lui de registre trec pe intrările comune ale cubului; contractele 1 și 2
  ale reconcilierii (`ReconciliereLuna`) citesc cubul (109-r1). `Sabotaj`,
  `Alocare` și `Reluare` se portează numai cât cere compilarea; ce nu mai are
  sens fără registre se șterge, cu rând în inventar. O rulare pe ianuarie se
  face la închidere ca diagnostic, cu raportul comparat cu
  `run-verificari/r3-ian-final2/`; nu e gate (091 d).
- **Migrare**: se portează sau se șterge, după ce arată inventarul.
- **BackfillTva** scrie `RegistruTva` și nu are obiect pe cub: se șterge.

## D9-D12 — Clientul React și contractul HTTP

Numele de raport rămân (`RegistruImobilizariDto`, registrul-jurnal): sunt
rapoarte, nu tabele. Din `api-types.ts` și din metadata ies numai tipurile
entităților de registru și câmpurile scoase (`posteazaInCub`,
`totalStingere`); driftul e declarat în contract la pasul 6, cu lista, și
`verifica:drift` trebuie să fie zero după regenerare. Paginile React de
detaliu rămân înghețate (104 d); listele și proiecțiile se ating numai cât
cere un câmp scos.

## D9-D13 — Verdictul restanțelor

Propunere; se confirmă la aprobare și se scrie în `restante.md` la închidere.

| Id | Verdict |
|---|---|
| S-r6, T-r7, T-r13, 098-r3, IM-r1, IM-r3 | se închid prin tăiere (D9-D2, D9-D3, D9-D5) |
| 64h, 86-r13, F27-r11 | dimensiunea `Repartitor` a rândului de registru dispare, partenerul stă pe postare (090 c); se închid dacă pasul 1 confirmă că niciun cititor nu mai grupează pe laturile documentului, cu proba pe `Partener` |
| 75-r4 | se închide dacă `Lot.PretUnitar` dispare (D9-D3) |
| TR-r9 | în felie (D9-D7) |
| F27-r16, F28-r1 | în felie (D9-D10) |
| X-r2 | în felie (D9-D9, D9-Q3) |
| 109-r1 | în felie (D9-D11) |
| T-r9 | în felie (D9-D6) |
| 106-r5 | parțial în felie (`HCategory`), restul rămâne |
| 76-r1, 76-r2, 76-r3, 86-r11, T-r5 | hook-urile de stingere: TR-D9b (D9-Q4 = A) |
| 104-r2 | `după PoC` (D9-Q4 = A) |
| N-r5, T-r2, 86-r1, B-r6, 107-r4, 51e | TR-D9b |
| 097-r2 | nu e a tăierii; rămâne activă, se tranșează în TR-D9b |
| 097-r3 | blocajul IMO rămâne; se rejudecă odată cu X-r1 (după PoC) |
| F27-r12 | snapshot-ul e al cubului; rămâne, `după PoC` |
| F26-r5 | se rejudecă pe cub; `după PoC` |
| T-r3 | regula rămâne (fără partener nu se inventează partidă); excluderea nominală din reconciliere dispare; limită declarată |
| B-r3 | se rejudecă la pasul 4: conexul nu mai are registre de servit |
| 63f | se judecă la pasul 1, pe cod |

Rândurile de catalog cu starea `amânat la TR-D9` se împart la fel: cele care
privesc registrele (limita duală din SC-X-01, garda din NIR, SC-ASM-19) se
închid aici; reevaluările și compensările (SC-X-05/06/07, SC-X-09, SC-X-13)
trec la TR-D9b.

## D9-D14 — Documentația și invarianții

În același commit cu pasul care schimbă comportamentul: `stare-curenta/`
pierde regimul dual, registrele și limitele lor; `limite-curente` pierde
limitele duale; `docs/invarianti.md` trece din „starea-țintă până la TR-D9"
în literă curentă (I, III, VI). Decizia 110 consemnează amendamentele la 108
(a, g, h, j) și la 090 (l). CLAUDE.md §Stare se rescrie ca stare, fără
rezumat de felie (091 l).

## D9-D15 — Pașii, verificarea, regula de oprire

Un commit per pas; integrala verde pe ambele profiluri la fiecare.

| Pas | Ce | Schimbă comportament |
|---|---|---|
| 0 | contractul aprobat; review advers Codex al contractului | nu |
| 1 | inventarul nominal, în `docs/nucleu/tr-d9-inventar.md`: refuzurile planului vechi cu verdict (D9-D4); fiecare referință ModelCheck cu verdict (D9-D7 c); consumatorii `RegulaStoc`, `Lot.PretUnitar`, `TotalStingere`; operarea tipului inert azi; 63f | nu |
| 2 | scenariile și gardurile adăugate înaintea tăierii: analiza obligatorie pe postări, tipul care nu declară, scenariile D9-D3 cu cifra veche și cea nouă | numai refuzuri echivalente |
| 3 | re-țintirea probelor de regulă pe cititorii cubului, sub regimul dual (D9-D7 d) | nu |
| 4 | tăierea: motorul vechi, valoarea liniei din contract, regimul ca dată, oracolul, probele pe sursă (D9-D2, D9-D3, D9-D5, D9-D7 a/b) | D9-D3 |
| 5 | schema, `InitialCreate`, seed-ul fără rânduri moarte, recrearea bazelor (D9-D6, D9-D8) | nu |
| 6 | evidența XAF, autorizarea, proiecțiile de rest, clientul, driftul (D9-D9, D9-D10, D9-D12) | D9-D9 |
| 7 | uneltele (D9-D11) | nu |
| 8 | închiderea: integrala, nucleul, `--probe-sursa`, `refuzuri.ps1` pe host viu, `PerfCub`, proba în browser a operării și a listei noi, docs, decizia 110, restanțele, review advers Codex | nu |

Pasul 1 citește tot `Program.cs`; e pasul pentru care se propune delegarea
(multiagent-delivery, un agent, cu verificarea main-ului pe eșantion).

**Regula de oprire.** Felia e închisă când sunt adevărate împreună:

1. niciun nume din lista interzisă (D9-D7 b) nu apare în `nou/`, probat de
   `--probe-sursa`;
2. integrala e verde pe ambele profiluri, pe baze recreate din
   `InitialCreate`; numărul de `Check` se reconciliază cu inventarul;
3. catalogul de scenarii e verde, cu cifrele neschimbate în afara celor
   declarate la D9-D3;
4. nucleul trece integral; `refuzuri.ps1` trece de două ori cu aceleași
   rezultate; `verifica:drift` e zero după regenerare; `--dump-metadata` e
   la zi; `--dump-integritate-tph` e zero;
5. scara `PerfCub` își păstrează criteriile de formă; criteriul
   `PartideCuRest` e închis sau re-amânat explicit (D9-D10); costul comenzii
   fără registre se raportează A/B pe aceeași bază;
6. `stare-curenta/` și invarianții nu mai descriu regimul dual;
7. review-ul advers Codex e închis și decizia 110 e scrisă.

**Oprire înaintea termenului** (se raportează owner-ului, nu se ocolește):
un refuz al planului vechi fără echivalent exprimabil pe contract; un cititor
de producție al registrelor în afara listei nominale; o aserție de regulă
care nu se re-țintește fără altă cifră nedeclarată; o cifră de catalog
schimbată în afara D9-D3.

## D9-Q — tranșările owner-ului (2026-10-05)

- **D9-Q1 — împărțirea = A.** TR-D9a tăierea, apoi TR-D9b unitățile, cu
  contracte separate. Respinsă: o singură felie TR-D9.
- **D9-Q2 — evidența XAF = A.** O listă read-only pe `Postare`. Respinse:
  nicio listă în XAF; liste separate pe domenii.
- **D9-Q3 — subiectul permisiunii = A.** Citirea pe `Postare`, unic.
  Respinsă: subiecte pe domeniu.
- **D9-Q4 — hook-urile care nu citesc registre = A.** Rămân în afara feliei:
  hook-urile de stingere trec la TR-D9b, 104-r2 trece la `după PoC`.
  Respinse: stingerea în TR-D9a; amândouă în TR-D9a.

## Ce NU intră (amânări cu nume)

TR-D9b (N-r5, T-r2, 86-r1, B-r6, 107-r4, 51e, 097-r2 și hook-urile de
stingere cu 76-r1…r3, 86-r11, T-r5); 104-r2; TR-r7; X-r1; IM-r2; IM-r5; 091-r4 și rularea integrală 12/12;
104-r3, 104-r4; restanțele `după PoC`.
