# 86. Pasul 5, felia 25 — DVI (declarația vamală de import) ca tip de document: TVA-ul în vamă intră în evidență, D300 rd. 24/25 și 7/22, codurile SAF-T de import

- **Data**: 2026-09-13
- **Stare**: activă (închide 83-r4; amendează 83g — poarta spre DVI e deschisă; DVI-D4/D9 din contract amendate la pasul 1)
- **Docs**: `docs/api/p5-felia25-dvi-contract.md` (DVI-D1…D10 + §Închidere), `nou/.../Module/BusinessObjects/Documente/Dvi.cs` (`Dvi`, `DviFactura`), `nou/.../Module/BusinessObjects/Comun/Interfete.cs` (`IVerificabilLaCommit`), `nou/.../Module/Motor/GardianEditare.cs` (dispecerul), `nou/.../Module/DatabaseUpdate/ProfilPrivat.cs` (tipurile de import, politica DVI), `nou/.../Module/Api/Dvi/`, `nou/.../WebApi/API/Conta/DviController.cs`, `nou/Atlas.Conta.Client/src/felii/dvi/`, `nou/tools/ModelCheck/Program.cs` (`E2E-DVI`, `E2E-API-DVI`), `nou/tools/ProbeHttp/refuzuri.ps1`, migrația `20260912193852_F25Dvi`

## Regula durabilă

**DVI — declarația vamală de import** (închide 83-r4). (a) `Dvi : Document`,
tip propriu în nomenclatorul de tipuri (20), cu liniile pe `DocumentDetaliu`
de BAZĂ: `Valoare` = valoarea în vamă (baza), `ValoareTva` = taxa declarată
(culeasă; 0 = se naște din cotă la operare, 48b), `TipMaterial` = bunurile
vămuite, `Cantitate` 0, `Lot` null; `Numar` = MRN-ul cules (fără politică de
numerotare, ca FCT); `Predator` = repartitorul CĂRUIA i se datorează taxa
(biroul vamal cu cont implicit 446, sau comisionarul cu 401), `Primitor` =
unitatea internă; furnizorul extern NU e latură. (b) **Motorul de operare
neatins**: DVI n-are `RegulaContare`/`RegulaStoc` — linia fără regulă și
fără conturi explicite nu postează valoarea, iar `PoliticaTva(DVI,
Deductibil, RepartitorPredator, fallback 446)` produce nota de TVA (4426 =
contrapartida; la `TaxareInversa` 4426 = 4427) și rândul de `RegistruTva`
(`Achizitie`, partener = Predator, baza = `Valoare`). Zero mișcări de stoc,
zero note pe valoare. (c) **Tipurile de import sunt date**: `TipTva.DeImport`
(bool) e singura cale de a spune „de import" fără coduri în cod (29); seed-ul
privat pune `IMP21`/`IMP11` (`Normal`, 301204/301205 → D300 rd. 24/25) și
`IMPTI21`/`IMPTI11` (`TaxareInversa`, 300604/300605 → rd. 7, oglinda 22 din
`RandD300.OglindaA`), toate `DeImport`, plus `IMP` (0%, rămâne al facturii);
D394 nemapat deliberat; `PoliticaTvaImplicit(DVI, orice clasă, IMP21)` +
ancora `TipTvaImplicit`; bugetar = doar ancora `DVI`. (d) `Dvi.ValideazaOperare`
(hook polimorf): MRN nevid, cel puțin o linie, Predator `Partener`, Primitor
`UnitateInterna`, fiecare linie cu `TipTva` `DeImport` și cotă > 0 (`IMP` e
refuzat: „TVA-ul în vamă are cotă"), `Valoare` > 0, facturile legate
`Operat`. (e) **Legătura n→m** `DviFactura { DviId, FacturaId }` (forma
`Imperechere`), index unic pe pereche, FK `Restrict`: EVIDENȚĂ, nu sursa
cifrelor — o factură pe mai multe DVI, o DVI cu zero facturi; creare/ștergere
DOAR cât DVI e `Draft`, fără editare, factura `Operat` la creare; după operare
înghețată cu documentul (stornoul e in-place: rândurile inverse stau pe
același document); anularea/stornarea unei FCT
legate NU se refuză (86-r2), starea facturii se arată. Nu e pe OData: se
scrie prin `FacturiIds` din agregat (agregatul ÎNTREG, nu delta). (f)
**`IVerificabilLaCommit`** = punct de extensie GENERIC al gardianului: regula
stă pe entitate (`Verifica(os, erori)`), `GardianEditare.Verifica` o cheamă
prin interfață înaintea switch-ului, FĂRĂ gardul de ștergere (regula vede și
`Delete`) — singura atingere a `Motor/*` în felie; gardianul rulează doar pe
ușile securizate, deci Import1C (ușa non-secured) nu e afectat și proba
supremă nu s-a cerut. (g) **DVI NU e document stins**: `PoateFiStins = false`;
taxa în vamă se plătește ca orice taxă (precedentul ITV/4423) printr-o plată
către biroul vamal pe contul lui implicit, fără imperechere; `Total (brut)` e
ascuns pe view-urile DVI, `Baza` („Valoare în vamă") și `Taxa` („TVA în
vamă") sunt `[NotMapped]` pe DetailView; `RepartitorImplicitCredit =
PredatorId` (creditul de 446 poartă vama, nu unitatea). (h) **Ușa
`api/dvi`**: agregat cules ca NTC (POST/PUT/DELETE pe Draft, comenzile
standard), `facturi-candidate` (perioadă obligatorie → 400 înaintea gate-ului
pe instanță; FCT operate, implicit doar clasa fiscală `ExtraUe`, `toate=true`
= orice; `dviId` exclude legăturile salvate; cere și citirea pe
`FacturaIntrare` → 403) întoarce plicul `{ Candidati[], MaiSunt }` cu plafon
500 decis PE INTEROGARE, înaintea filtrului de clasă (care e al legii, în
memorie); `TipMaterialSugeratId` = tipul dominant pe liniile facturii;
totalurile pe server (42c); id de DVI pe ușa NTC/FCT = 404 automat (TPT). Tot
ce poate refuza se rezolvă ÎNAINTEA oricărui `CreateObject` (F3-D5) — un
orfan `DviFactura` cu `FacturaId` gol s-ar persista la commit-ul următor pe
un OS cu viață lungă. (i) **Clientul** `felii/dvi`: lista pe perioadă în URL
ca filtru real, detaliul pe `DocumentShell`, lookup-ul `TipTva` filtrat
`DeImport eq true`, implicitul de pe ancora `DVI` cu partenerul = Predatorul,
linia nouă ia `TipMaterialSugeratId` al ultimei facturi adăugate (niciodată
peste alegerea operatorului), panoul „Facturi de import" cu popup pe
candidați (perioada = luna declarației, mesaj vizibil când `MaiSunt`),
`rutaTip('DVI')`, coloana „De import" în grila tipurilor de TVA; nimic
calculat în TS. (j) **XAF**: `[TipDetaliu(typeof(DocumentDetaliu))]` comută
`Detalii` pe `DocumentDetaliu_ListView` (per CLASĂ de detaliu, nu per tip —
al doilea tip care l-ar declara ar împărți grila), grila `DviFactura` nested
în mod `Client` (85) cu coloane pe CĂI IMBRICATE (`Factura.Numar/Data/
Predator/Stare`) declarate prin escape-hatch-ul `ListView(id, …)` — fluent-ul
Atlas.DXF n-are `Column` pe string. (k) **Probele**: `E2E-DVI` (planul,
registrele, D300 cu oglinda 7 → 22 automată, jurnalul, D394 gol, plata ca
taxă cu 446 net 0, gardianul pe calea reală `GardianEditare.Verifica`,
refuzurile de operare, storno, anularea FCT legate), `E2E-API-DVI` (agregatul
cap-coadă, candidații, plafonul cu ambele capete, regresia orfanului),
`refuzuri.ps1` 156/156 cu 39 de oracole DVI, smoke în browser pe Privat
cap-coadă; DVI-r7 închisă: proiecția SAF-T D406 iterează `RegistruTva` fără
filtru de tip, deci codurile de import ies singure.

## Context

Decizia 83g a lăsat factura furnizorului extern pe `IMP` (cotă 0,
`Neimpozabil`, nemapat): „linia nu poartă taxă și spune de ce", cu poarta
spre DVI ca restanță (83-r4). Fără DVI, TVA-ul plătit în vamă nu avea unde să
intre în evidență: rd. 24/25 (importuri cu taxa plătită efectiv) și rd. 7/22
(amânarea plății, art. 326 (4)–(5)) rămâneau goale, iar SAF-T-ul nu putea
emite codurile de import — care sunt ale DVI-ului, nu ale facturii.

Prioritatea s-a decis pe contractul IM (`p5-felia-izolare-motor-contract.md`,
criteriul de pornire): niciun semnal real pentru izolarea motorului, deci
singura gaură FUNCȚIONALĂ declarată a produsului privat a luat rândul. Două
explorări read-only (2026-09-12) au fixat premisele: legacy n-a avut
niciodată DVI (`COD_TARIF_VAMAL`/`TVA_AMANAT` = coloane invizibile, moarte);
baza Flax pe 2025 are 9 FCT extra-UE (MA/GB/NO), majoritatea servicii, fără
nicio DVI, iar exportul 1C nu menționează vama — deci proba e pe SCENĂ, nu pe
date; nomenclatorul SAF-T (`RO_SAFT_SchemaDefCod_16.02.2026.xlsx`) are DOUĂ
familii de coduri de import pe cotă; motorul era deja pregătit (o linie fără
regulă de contare postează doar TVA-ul din `PoliticaTva`).

## Tranșări

**(a) De ce `Dvi : Document`, nu `NotaContabila`.** Nota cere conturi
explicite pe fiecare linie și postează valoarea; DVI nu postează nimic pe
valoare. Derivata directă cu liniile pe baza `DocumentDetaliu` folosește
exact mecanismul existent din `MotorOperare` (linia fără regulă și fără
conturi explicite e sărită la note, dar intră în nota de TVA și în
`RegistruTva`). `TipMaterial` rămâne NOT NULL pe bază; API-ul îl sugerează
din facturi în loc să inventeze un tip de detaliu cu schemă proprie.

**(b) Două familii de tipuri, nu un `IMP21` singur.** 83g vorbea de un singur
tip; nomenclatorul arată că importul cu taxa plătită în vamă (301204/301205 →
rd. 24/25) și cel cu amânarea plății (300604/300605 → rd. 7/22) sunt regimuri
diferite. Alegerea e a operatorului, pe linie; `DeImport` le grupează fără
cod. Rd. 22 nu se mapează: e oglinda lui 7 și o pune proiecția.

**(c) Predatorul = cui i se datorează taxa.** Cu `RepartitorPredator` ca sursă
a contrapartidei, biroul vamal (cont implicit 446) sau comisionarul (401) dau
nota corectă din date, iar rândul de `RegistruTva` primește partenerul.
Furnizorul extern rămâne pe facturile legate. Cazul comisionarului care
refacturează taxa (dublă postare cu factura lui) e restanță (86-r10).

**(d) DVI nu e document stins — amendament la pasul 1, pe cifre.** Contractul
cerea „plata de 210 stinge DVI". Măsurat: `ImperechereService.Total` =
Σ(`Valoare` + `ValoareTva`) pe toate liniile = 1815 la o datorie reală de
210, rest 1605; singurul hook (`LiniiCreanta`) filtrează linii, nu schimbă
formula, iar `DocumenteCuRest` e o uniune per tip concret (RDC e exclus din
același motiv). Repararea cerea motorul de operare + ramura în uniune —
deschiderea proba supremă (Import1C integral) pentru o cifră pe care produsul
n-o cere: taxele nu se sting prin imperechere (ITV/4423 se plătește cu o
plată simplă). `PoateFiStins = false` e obligatoriu, nu opțional: cu
`SensDeStins` doar scos, `ValideazaCreare` ACCEPTA tăcut (plafonul plății
oferă un singur sens). Restul polimorf pe DVI = 86-r11.

**(e) Gardianul fără punct de extensie — `IVerificabilLaCommit`.**
`GardianEditare.Verifica` e un `switch` pe tipuri; singurele reguli generice
sunt cele de interfață (`ICuCautare`, `ICuProvenienta`), verificate înaintea
lui. Un `case DviFactura` ar fi pus frunza în motor; interfața pune regula pe
entitate și lasă gardianului un dispecer de cinci linii, fără gardul de
ștergere (dezlegarea de pe un document operat e tot o scriere refuzată).
Probele au fost mutate pe CALEA REALĂ (`GardianEditare.Verifica(os)` pe un OS
propriu per refuz). Import1C nu trece prin gardian (ușa non-secured), deci
DVI-D9 a fost amendat: motorul de OPERARE neatins, dispecerul permis.

**(f) `Observatii` nu există pe model** — contractul îl presupunea; a ieșit
la pasul 2 fără a redeschide schema. **`Aplica`** rămâne numele comun al
feliilor (creare + actualizare), nu `Creeaza`/`Actualizeaza`.

**(g) Plicul candidaților.** Un tablou plafonat tăcut la 500 ar fi ascuns
facturi extra-UE vechi (plafonul se aplică la interogare, ÎNAINTEA filtrului
de clasă fiscală, care e funcția legii, în memorie). `MaiSunt` decis pe
`Take(plafon + 1)` spune operatorului să îngusteze perioada; proba forțează
ambele capete (plafon 2 → `true`; plafon = numărul rândurilor → `false`).

**(h) Referința rezolvată după `CreateObject` — defect propriu, reparat.**
Prima formă a `ReconciliazaFacturi` crea legătura și abia apoi cerea factura;
la un id inexistent, în OS-ul viu rămânea un `DviFactura` cu `FacturaId` gol,
persistat de primul commit de după (23503 dintr-un `Aplica` VALID). Pe HTTP
e invizibil (OS-ul moare cu cererea); pe ModelCheck/batch e coruptie tăcută.
Aceeași formă, MASCATĂ de navigația `Detalii`, există în
`NotaContabilaApply`/`FacturaIntrareApply` (86-r14).

**(i) Două constatări preexistente, măsurate pe drum.** Mesajul refuzului
`PoateFiStins` din `ImperechereService` e scris pentru viramente (86-r12).
Dimensiunea Repartitor a plăților e inversată față de conturi: latura de terț
poartă contul propriu, cea de trezorerie partenerul — fișa lui 446 filtrată
pe biroul vamal rămâne la −210 după plată, deși contul e net 0 (86-r13).
Niciuna nu e a feliei; proba măsoară soldul pe CONT.

**(j) Seed-ul mută cifre din probe existente.** Patru tipuri de TVA, un
implicit generic și patru mapări D300 au schimbat numerele hardcodate din
zece probe (`D3-V1`, `F23-V2/V3`, `F24-V8`); s-au schimbat DOAR numerele, iar
două texte cu cifre stale („azi cinci", „exact 7 perechi") au devenit
formulări fără cifră, contra listei din seed. `ImpliciteTva` a devenit
`ClasaFiscalaPartener?` (rândul „orice clasă"), cu potrivirea de idempotență
în memorie.

**(l) Smoke-ul XAF (pasul 4) a lovit două defecte de ecran, ambele în
baseline.** Grila liniilor DVI arăta coloana `Document` ca GUID: `[TipDetaliu(
typeof(DocumentDetaliu))]` comută pe ListView-ul de CLASĂ al bazei, care
păstrează navigația spre părinte (`Document` n-are `DefaultProperty`, 85b) —
ascunsă. `FacturaIntrare_LookupListView` n-avea `Numar`/`Data`/`Predator`/
`Stare` (începea cu `Scadență`; o factură fără scadență/PV/plată era un rând
gol, imposibil de ales): `ListaRoot<T>` țintește doar `_ListView`, deci
simptomul tratat acolo era netratat pe lookup — defect PREEXISTENT, DVI e
primul ecran care alege o factură dintr-un lookup; cele patru coloane sunt
acum în față (86-r18 pentru ordinea curată și pentru familia lookup-urilor).
Navigația XAF n-are intrare per tip (doar `Document` + `Imperechere`; tipurile
vin din dropdown-ul `New` și din URL) — DVI respectă convenția și e singurul
cu denumire în română în dropdown (84-r5). Pe DVI operat/stornat XAF ascunde
`New`/`Delete` pe grilele nested, deci refuzul gardianului „doar cât e Draft"
nu se vede în ecran; rămâne probat pe calea reală în ModelCheck. După
selecție lookup-ul afișează GUID-ul facturii (85b, 86-r17).

**(k) Scena `E2E-API-DVI` folosește `IMP` (0%) pe toate facturile.** O rulare
care crapă otrăvește rularea următoare (soldurile 4426/4427 se citesc
cumulat); cu facturi fără taxă, scena nu poate mișca 4426 nici măcar la un
crash. Curățenia e la începutul și la sfârșitul blocului.

## Review advers

Agent separat, read-only, pe `git diff main..HEAD` + ciorna deciziei
(2026-09-13). Zero blocante. Verdictele și ce s-a făcut:

- **R1 (de reparat, reparat)** — unicitatea perechii `DviFactura` era prin
  INTEROGARE, care nu vede rândurile noi ale aceluiași commit: în grila XAF
  nested, două legături pe aceeași factură cu un singur Save ar fi dat 23505
  din bază, nu mesajul de gardian. `Verifica` vede acum și `ModifiedObjects`
  (noi, neșterse, aceeași pereche); proba `DVI-V10b` o măsoară pe calea reală.
- **R2 (docs, reparat)** — „stornoul nu clonează (`DocumentSursa`)" descria un
  mecanism inexistent (stornoul e in-place); „plată → rest 0" din regula de
  oprire contrazicea D4 amendat. Ambele rescrise.
- **R3 (reparat)** — `Valoare` negativă era acceptată pe draft prin PUT
  (refuz abia la operare), asimetric cu `ValoareTva`; acum 422 la salvare.
- **R4 (reparat)** — pe PUT-ul unui document operat, laturile se rezolvau
  ÎNAINTEA pre-check-ului de stare, deci o latură invizibilă dădea „referință
  invizibilă" în loc de „nu mai e Draft"; ordinea inversată (tot înaintea
  oricărui `CreateObject`).
- **R5 (probe, reparat)** — cele două blocuri din februarie 2026 își purjau
  doar marcajul propriu; un crash în `E2E-API-DVI` ar fi picat fals `E2E-DVI`
  la rularea următoare. `CurataDvi` purjează ambele marcaje, iar `E2E-DVI`
  re-măsoară precondiția lunii libere.
- **N2 (reparat preventiv)** — `Rezolva.Cere<FacturaIntrare>` per id ar fi
  aruncat cast (500) pe id-ul unui obiect deja urmărit cu alt tip (propriul
  id al declarației în `FacturiIds`); facturile se rezolvă printr-o singură
  interogare pe id-uri, cu refuz de domeniu pe lipsă.
- **N1 / N3 (neprobate, risc mic)** — navigația `Dvi` la commit în grila XAF
  (proba = smoke-ul XAF al pasului 4) și DELETE pe draft cu legături pe ușa
  securizată (ștergerea e amânată, FK-ul nu se atinge).
- Confirmate: niciun bypass al gardianului pe vreo ușă vie, nicio scriere de
  registre în afara motorului, nicio cale prin care DVI ajunge document stins,
  niciun calcul în TS, niciun simbol de cont sau cod de TVA în cod, ordinea
  refuzurilor 80 respectată, `Lista` fără N+1, `NrFacturi` prin filtrul
  global `GCRecord`.
- Cosmetice tăiate: cifra de probă din comentariul `ContaUiBaseline`,
  „al 17-lea derivat" din două comentarii. Rămase (fără impact): `Stare` pe
  lista DVI fără `labelEnum`, `capAntet` cu schema `DviWriteDto` pentru
  `Baza`/`Taxa`.

## Cifrele

| | la deschidere | la închidere |
|---|---|---|
| ModelCheck bugetar | 927 (IM, 2026-09-10) | 955 / 0 FAIL |
| ModelCheck privat | 978 (IM, 2026-09-10) | 1068 / 0 FAIL |
| `refuzuri.ps1` | 117 | 156 / 156 |
| `Motor/*` | — | 5 linii (dispecerul `IVerificabilLaCommit`) |
| Import1C integral | — | necerut (motorul de operare neatins) |

## Ce rămâne deschis (restanțele deciziei)

- **86-r1** taxele vamale și accizele în costul de achiziție (ajustare de cost pe lot).
- **86-r2** anularea/stornarea unei FCT legate la o DVI operată nu se refuză.
- **86-r3** RLF pe DVI (retur de import / re-export).
- **86-r4** amânarea plății în vamă ca politică de partener.
- **86-r5** scadență pe DVI; **86-r6** unicitatea MRN-ului.
- **86-r7** — închisă de 86 (SAF-T D406 emite rândul DVI cu 301204; probat).
- **86-r8** `DviFactura` în „Explică"/OData; **86-r9** curățenia datelor Flax (reMarkable NO pe `TI19`).
- **86-r10** comisionarul vamal care plătește taxa și o refacturează.
- **86-r11** DVI ca document stins (restul polimorf + ramura în `DocumenteCuRest`).
- **86-r12** mesajul refuzului `PoateFiStins` e al viramentului.
- **86-r13** dimensiunea Repartitor a plăților e inversată față de conturi (preexistentă).
- **86-r14** `CreateObject` înaintea lui `Rezolva.Cere` în `NotaContabilaApply`/`FacturaIntrareApply`.
- **86-r15** `IMP` (0%) rămâne `DeImport`, deci lookup-ul liniei DVI îl propune și operarea îl refuză — capcană de culegere acceptată (filtrul pe cotă ar fi al doilea criteriu).
- **86-r16** `DocumentDetaliu_ListView` e per CLASĂ de detaliu: al doilea tip cu `[TipDetaliu(typeof(DocumentDetaliu))]` ar împărți grila și caption-urile cu DVI.
- **86-r17** `Document` fără `DefaultProperty` (85b): lookup-urile și grilele de legătură (`Imperechere`, `DviFactura`) afișează GUID-ul după selecție — un membru de afișare `[NotMapped]` pe `Document` (precedentul `Lot.Eticheta`) e decizie proprie.
- **86-r18** lookup-urile de documente: `ListaRoot<T>` țintește doar `_ListView`, deci `_LookupListView` rămâne cu coloanele generate (intercalate, fără identificare în față) — familia întreagă, nu doar FCT.
