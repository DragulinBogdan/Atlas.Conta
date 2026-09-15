# 87. Pasul 5, felia 26 — Imobilizări și amortizare, contabil și fiscal: fișa ca nomenclator subțire, un singur registru append-only de evenimente și luni, PIF/CAS ca documente operate, AMO generată lunar cu trei cifre din `AmortizareService`, conturile și regulile de deductibilitate exclusiv din politică, versionate în timp

- **Data**: 2026-09-15
- **Stare**: activă (concretizează 9 — imobilizările ca modul propriu, prin documente și registru; F26-D10 din contract amendat la pasul 3, F26-D13 la pasul 2)
- **Docs**: `docs/api/p5-felia26-imobilizari-contract.md` (F26-D1…D16, §Pașii cu notele *Executat*, §Închidere), `nou/.../Module/BusinessObjects/Nomenclatoare/Imobilizari.cs` (`Imobilizare`, `ClasificareImobilizari`, `RegistruImobilizari`, `PoliticaAmortizare`, `RegulaDeductibilitate`), `nou/.../Module/BusinessObjects/Documente/Imobilizari.cs` (`PunereInFunctiune`, `IesireImobilizare`, `AmortizareLunara` + detaliile, hook-urile, gardianul fișei), `nou/.../Module/BusinessObjects/Comun/Interfete.cs` (`IDocumentCuRegistruPropriu`), `nou/.../Module/Motor/AmortizareService.cs`, `nou/.../Module/Motor/MotorOperare.cs` (cele trei dispecere), `nou/.../Module/Motor/GardianEditare.cs` (registrul al patrulea), `nou/.../Module/DatabaseUpdate/{ContaSeeder,ProfilPrivat,ProfilBugetar}.cs` + `SeedData/catalog-mf.csv`, `nou/.../Module/Api/{Imo,Pif,Cas,Amo}/`, `nou/.../WebApi/API/Conta/{Imobilizari,Pif,Cas,Amo}Controller.cs`, `nou/.../WebApi/Startup.cs` (seturile OData), `nou/Atlas.Conta.Client/src/felii/{imobilizari,pif,cas,amo,politici}/`, `nou/tools/ModelCheck/Program.cs` (`E2E-IMO`, `E2E-API-IMO`, `RECONCILIERE-MF`), `nou/tools/ProbeHttp/refuzuri.ps1`, migrațiile `20260914113154_F26Imobilizari`, `20260914162054_F26CodEconomicImobilizare`; raportul explorării 1C: `1C/07-mijloace-fixe.md`

## Regula durabilă

**Imobilizările** — evidența pe tiparele existente, fără mecanism nou în
nucleu. (a) **Fișa `Imobilizare` e nomenclator subțire**: identitate
(`NumarInventar` unic, `Denumire`, `Cautare`), `TipMaterial` de clasă F (contul
de imobilizare = contul implicit al tipului, 26b), `Clasificare` opțională din
catalog, `Loc : Repartitor` (dimensiunea notelor), `CentruCost`,
`Responsabil`, `CodEconomic` (dimensiunea bugetară a cheltuielii, copiată pe
liniile AMO/CAS), plus starea materializată de motor (`Noua | InFunctiune |
Iesita`, `DataPunereInFunctiune`, `DataIesire`). Metoda, durata, reziduala,
categoria fiscală, valoarea NU sunt pe fișă: sunt FAPTE datate în registru,
scrise de documente; „parametrii curenți" = proiecția ultimului eveniment.
Gardianul fișei (`IVerificabilLaCommit`): tipul material e de clasă de
imobilizări (natura `Imobilizare`, la creare și la schimbare) și se schimbă
doar în `Noua`; `Loc` și `CodEconomic` în `Noua` și `InFunctiune`; nimic în
`Iesita`; ștergere doar `Noua`, fără rânduri de registru și fără linii de
documente (PIF/CAS/AMO, chiar Draft) care o poartă; câmpurile motorului le
scrie doar motorul. (b) **Un singur registru `RegistruImobilizari`, append-only**: rând
per (fișă, dată, `Fel` ∈ Intrare | Modernizare | Revizuire | Amortizare |
Iesire | Reevaluare-rezervat), cu efectele semnate pe brut / brut fiscal /
cumulat contabil / fiscal / deductibil / `Luni`, parametrii doar pe
evenimente (null = neschimbați), `Repartitor` = locul la data faptului,
`DocumentId`/`DetaliuId`, `Storno`. Situația la o dată = sumele coloanelor +
parametrii ultimului eveniment ≤ dată (coalesce înapoi). Scris DOAR de motor;
protejat de `GardianEditare` ca celelalte trei registre; fișa, registrul
imobilizărilor, D101 și SAF-T Assets sunt sume peste el. Contabil vs fiscal =
doi parametri și două cifre în ACELAȘI registru: amortizarea fiscală nu
postează și n-are document, dar e fapt lunar înghețat, nu recalcul.
(c) **Contractul motorului = `IDocumentCuRegistruPropriu`** (`Materializeaza`
/ `Elimina` / `StorneazaRegistrul`), consumat de `MotorOperare` prin EXACT
trei dispecere pe interfață (8 linii) — nucleul `Document`/`DocumentDetaliu`
și celelalte servicii neatinse; dependențele (fapte ulterioare ale fișelor)
le refuză FRUNZA cu `OperareException`; proba supremă (Import1C integral,
raport identic cu baseline-ul F18) s-a cerut și a trecut. (d) **Politica**:
`PoliticaAmortizare` per tip material de clasă F (cont de amortizare,
cheltuială cu amortizarea, cheltuială la cedare; privat 205→2805,
208→2808, 212→2812, 2131/2132/2133→2813, 214→2814 cu 6811/6583, 211 fără
rând; bugetar frunzele 280.08.01/280.08.09/281.03.01/281.03.03 cu 681.01.00/
691.00.00); `RegulaDeductibilitate` = legea ca DATE cu valabilitate
(`Categorie`, `DoarNeexclusiv`, `Fel` ∈ PlafonLunar | Procent, `Valoare`,
`DeLa`/`PanaLa`, `Temei`; privat: plafon 1 500/lună pe vehicule ≤ 9 locuri
neexclusive din 2012-02-01, sediul în locuință 0 % din 2024-01-01 și 50 % din
2026-01-01; bugetar zero rânduri); `ClasificareImobilizari` = catalogul HG
2139/2004 seed-uit din CSV (590 din 598 de poziții; banda de ani dă
`[min×12, max×12]` pentru durata fiscală, verificată la PIF/Revizuire când
fișa are clasificare). Ambele politici sunt în `TipuriConfigurabile`
(`Configurator` le scrie), aliniate de seed pe `DinSeed`; niciun simbol de
cont și nicio cifră de lege în cod în afara `DatabaseUpdate/`. Cadrul pentru
evoluția legii: o cerință nouă care încape într-un rând cu `DeLa` e seed;
un fel nou de regulă e mecanism nou în cod, mic și numit; cifrele deja
postate nu se recalculează. (e) **PIF** (`PunereInFunctiune`, unitate → loc,
linii per fișă cu `Fel` ∈ Intrare | Modernizare | Revizuire) NU postează:
scrie evenimentele și parametrii în registru și materializează starea fișei.
`Intrare` cere fișă `Noua` și parametri completi; `Modernizare`/`Revizuire`
cer `InFunctiune`; `Valoare > 0` la Intrare/Modernizare, 0 la Revizuire;
`Degresiva` cere durată multiplu de 12; inițialele (amortizare/luni) doar pe
intrarea FĂRĂ linie sursă; o fișă apare o singură dată pe un PIF (un
document = un eveniment per fișă); duratele revizuite depășesc lunile deja
amortizate, iar lunile inițiale nu depășesc duratele. Linia sursă = linia unei FCT operate de clasă F,
cu o singură aritmetică a consumului (`ConsumatPeLiniiSursa`: Σ liniilor
PIF nestornate ≤ valoarea liniei) pentru hook și pentru `linii-sursa`.
Cronologie: refuz dacă există AMO operată într-o lună > luna PIF-ului;
anularea/stornarea sunt refuzate dacă fișele au fapte ulterioare nestornate
(`VerificaFaraFapteUlterioare`, comună cu AMO); la Intrare readuc fișa în
`Noua`. (f) **CAS** (`IesireImobilizare`, loc → unitate, `Cauza` ∈ Casare |
Vanzare | Lipsa): culegerea = lista fișelor; liniile le produce SERVERUL din
`AmortizareService.LiniiIesire` — per fișă nota `AmortizareCumulata` (28x =
21x pe cumulat, omisă la cumulat 0) și `ValoareRamasa` (6583 = 21x pe rest,
omisă la net 0), conturile din politica tipului material (fără politică =
refuz), `Repartitor` = locul, `CodEconomic` de pe fișă; `ValideazaOperare`
recalculează la `Data` și refuză liniile care nu mai corespund; refuz dacă
există AMO operată cu luna ≥ luna ieșirii (luna ieșirii nu se amortizează),
iar anularea/stornarea ieșirii sunt refuzate pe același gardian (lunile de
după s-au generat fără fișele ieșite); după operare, mesaj consultativ dacă
luna precedentă n-are AMO operată deși fișele erau în funcțiune; rând
`Iesire` cu toate cifrele negative, fișa `Iesita`. (g) **AMO** (`AmortizareLunara`, document
GENERAT pe tiparul ITV, `Autogenerat`, `PoateFiStins = false`, unitatea
internă ca parametru, `Data` = ultima zi a lunii): `Motor/AmortizareService`
e SINGURA aritmetică — `CotaLunara` pură, aplicată de trei ori per fișă
(contabil, fiscal, apoi `Deductibil` din regulile valabile la dată: per
`(Categorie, Fel)` rândul cu `DeLa` maxim ≤ dată, `DoarNeexclusiv` sărit la
utilizare exclusivă, plafon apoi procent, fără regulă = fiscal). `Liniara`:
cota = `RotunjesteBani(valoare de amortizat / luni rămase)` FIXATĂ la
ultimul eveniment, `suma = min(cota, rest)`, ultima lună absoarbe restul;
**baza „la ultimul eveniment" = situația la SFÂRȘITUL lunii evenimentului**
— luna evenimentului postează încă cota veche, parametrii noi curg din luna
următoare indiferent de zi (formula observată în Flax). `Accelerata`: 50 %
din brut în primele 12 luni de la punere, apoi liniar. `Degresiva` AD1: k
după ani (1,5 / 2 / 2,5), trecere la liniar, a 12-a lună absoarbe restul
anului. Eligibilitatea e pe DATE: punere < prima zi a lunii, ieșire > ultima
zi, rest contabil sau fiscal > 0. Linia cu contabil 0 și fiscal > 0 rămâne
fără conturi (nu postează 0). `Analizeaza` = ordinea gardienilor o singură
dată (`FisaFaraPolitica` → `AmortizareVie` → `NeCronologica` →
`DraftAnterior` → `LunaLipsa` → `PerioadaInchisa` → `FaraFise`): motiv la
raport, refuz la comandă; `Previzualizeaza` nu scrie, `genereaza` scrie ori
de câte ori luna e liberă (79), `regenereaza` doar pe Draft. Operarea cere
`Data` = ultima zi a lunii și aceeași unitate pe ambele laturi (antetul
draftului rămâne al generatorului), apoi recalculează și refuză dacă
mulțimea liniilor diferă pe cheia (fișă, contabil, fiscal, deductibil,
conturi, loc, centru de cost, cod economic) — criteriul e același pentru
`Stale` din API (`LiniileCorespund`); postarea = per linie debit cheltuială /
credit amortizare pe cifra CONTABILĂ, rând `Amortizare` cu cele trei cifre
și `Luni` 1. Anularea/stornarea unei AMO sunt refuzate dacă există AMO
operată ulterioară SAU fapte ulterioare ale fișelor ei (ieșire,
modernizare, revizuire) — cronologia lunilor, nu doar a amortizărilor.
**Stornoul oricărui document de imobilizări (PIF, CAS, AMO) se datează în
luna documentului** (`VerificaLunaStornarii`): situația la o dată e o sumă
de rânduri ≤ dată, iar un rând invers datat în altă lună ar lăsa lunile
dintre document și storno cu o situație falsă. (h) **Transferul** = schimbarea `Loc` pe fișă, administrativ,
fără document: următoarea AMO postează pe noul loc, istoricul = rândurile
lunare; **reevaluarea** are locul rezervat (`Fel`, coloanele), nu
mecanismul. (i) **Ușile**: nomenclatoarele pe OData — `Imobilizare` CRUD cu
gardianul pe ușa comună (422 prin `RefuzOdataFilter`), `ClasificareImobilizari`
`ReadOnly()`, cele două politici ca seturi; REST doar ce OData nu poate:
`GET api/imobilizari/{id}/fisa?laData=` (antet, banda catalogului, situația
la dată cu parametrii curenți, rândurile registrului) și `GET
api/imobilizari/registru?laData=` (o linie per fișă pusă în funcțiune,
dintr-o singură citire, totaluri pe server), ambele non-secured cu al doilea
drept pe `RegistruImobilizari` după 404-ul instanței; `api/pif` = agregat
cules (PUT doar Draft, toate liniile rezolvate înaintea antetului) +
`linii-sursa` (filtrul de clasă F în SQL înaintea plafonului 500, plic
`{ Candidati, MaiSunt }`, `Rest` prin aceeași aritmetică); `api/cas` =
antet + `Fise[]`, liniile produse de server; `api/amo` = oglinda ITV
(`previzualizare` cu `Motiv`/`MotivEticheta`/`BlocantId`, `genereaza` cu
`CreareAutorizata`, `regenereaza`, `Stale` pe `GET {id}`, comenzile).
Gate-urile 401 → 400 → 404 → 403 → 422 și `EroriDto` (80); id PIF/CAS/AMO pe
ușa NTC = 404. (j) **Clientul și XAF**: fișa ca ecran de nomenclator pe OData
cu panoul „Fișa" (`laData` în URL) și registrul la `/imobilizari/registru`;
PIF cu lookup de fișă filtrat pe locul primitorului și pe stare, dialogul
liniilor de clasă F cu prefill `Valoare = Rest`, parametrii pre-completați
pe `Revizuire` din situația citită; CAS cu selectorul de fișe pe locul
predatorului, liniile de citire; AMO = oglinda ITV; politicile pe
`GrilaPolitica`; nimic calculat în TS. Rutele `/nou` primesc `key="nou"` în
tot clientul (la tranziția `/:id` → `/nou` detaliul se remontează). XAF: doar
cât cere ModelCheck (`ContaUiBaseline`, grile `Client` pe culegere, liniile
CAS/AMO read-only, `RegistruImobilizari` `Server`, captions); operarea prin
`DocumentOperareController`; fără acțiune XAF de generare. (k) **Probele**:
`E2E-IMO` (`IMO-V*`, ambele profiluri: lanțul lunar, modernizare, revizuire,
storno cu rânduri inverse în ambele registre, anulare/storno necronologice
și sub fapte ulterioare refuzate, `LunaLipsa`/`PerioadaInchisa`, anti-stale,
regulile cu `DeLa`, vehiculul plafonat/exclusiv, reziduala, duratele
independente, CAS după AMO, transferul, fișa fără politică, probele pure
Citan/Zebra/CENTRU IT/invertor/accelerată/degresivă), `E2E-API-IMO`
(`API-IMO-V*`: fișă → FCT → `linii-sursa` → PIF → previzualizare/generare/
regenerare → revizuire ⇒ `Stale` → CAS → `fisa`/`registru` → refuzurile
422), `RECONCILIERE-MF` (formula contra cifrelor postate în Flax; nu pică pe
diferențe, le raportează), ≈ 60 de oracole în `refuzuri.ps1`, smoke React și
XAF pe Privat, Import1C integral cu raport identic.

## Context

Imobilizările erau modulul ABSENT: `NaturaClasa.Imobilizare` era doar
eticheta la achiziție (FCT cu linie de clasă F postează contul implicit al
tipului contra 404, fără lot, fără NIR), iar decizia 9 le lăsase „modul
separat, prin note". Contractul IM (izolarea motorului de `IObjectSpace`)
n-a primit semnalul de pornire din criteriul lui; dintre candidați,
imobilizările erau singura gaură FUNCȚIONALĂ mare a produsului privat cu
cerință fiscală obligatorie (amortizarea fiscală pe durata normală din
catalog, art. 28 Cod fiscal) și cu proba disponibilă în Flax.

Patru explorări read-only (2026-09-14) au fixat premisele: motorul n-avea
nimic pentru imobilizări, ambele planuri au conturile; ITV e o `NotaContabila`
cu liniile calculate de serviciu (tiparul generatorului lunar); registrele
sunt trei, materializate în `Opereaza`, șterse în `AnuleazaOperarea`,
inversate în `Storneaza` (un al patrulea cere dispecer în cele trei puncte);
1C (Flax) ține cardul doar cu identitatea, parametrii sunt rânduri DATATE
scrise de documente, iar formula e verificată la ban pe activele integral
amortizate: cotă fixată la ultimul eveniment, ultima lună absoarbe restul,
prima amortizare în luna DE DUPĂ punerea în funcțiune. Două runde de
întrebări cu owner-ul (12 + 9) au tranșat contractul: trei cifre pe lună
(contabilă, fiscală, deductibilă) într-un singur registru, conturile și
regulile de deductibilitate din politică versionată, privat-first cu
bugetarul ca seed.

## Tranșări

**(a) De ce fișă subțire + registru, nu parametri pe fișă.** Editarea
duratei sau a metodei pe nomenclator ar rupe reproductibilitatea: cifrele
postate n-ar mai avea intrările calculului lor. Ca în 1C, parametrii sunt
rânduri datate scrise de documente (`Intrare`, `Revizuire`); situația la
orice dată e o sumă plus coalesce înapoi pe evenimente. Fișa poartă doar
identitatea, dimensiunile și starea materializată. Transferul (D8) rămâne
edit administrativ pe `Loc` pentru că nu mișcă valoare și nu postează;
istoricul locului e pe rândurile lunare.

**(b) Un singur registru, nu două (mișcări / amortizări) și nu proiecție
peste `RegistruContabil`.** Aceeași cheie (fișă × dată), același ciclu de
viață (anulare șterge, storno inversează), aceeași citire (o singură sumă);
separarea ar dubla protecția, hook-ul și proiecțiile. Proiecția peste
`RegistruContabil` nu poate exista: PIF/Revizuire nu postează, deci valoarea
de intrare, parametrii și lunile n-ar avea registru (III). Amortizarea
lunară apare și în `RegistruContabil` (nota 68x = 28x cu `DetaliuId`): două
registre, două fapte, ca la recepție; atribuirea per fișă e a lui
`RegistruImobilizari`.

**(c) Contabil vs fiscal = două cifre, un registru.** Amortizarea fiscală nu
postează și n-are document propriu — un registru fiscal separat ar fi
„registru fără document" (I). E însă fapt lunar ÎNGHEȚAT pe rândul lunar,
calculat de aceeași funcție din aceiași parametri datați și din regulile
valabile la acea dată; legea schimbată în timpul vieții activului schimbă
doar lunile viitoare, prin rând nou cu `DeLa`. Deductibilul e a treia cifră
pentru că plafonul/procentul nu schimbă amortizarea fiscală, ci partea din
ea recunoscută la impozitul pe profit (D101 = restanță peste această
coloană).

**(d) Formula: baza la SFÂRȘITUL lunii evenimentului (oprire raportată la
pasul 2).** Contractul spunea „cota citită la ultimul eveniment"; pe fixture
s-a văzut că 1C postează în luna evenimentului încă cota veche și aplică
parametrii noi din luna următoare indiferent de zi (invertorul modernizat pe
21.09.2023 → 1 314,55 din 10.2023; CENTRU IT reevaluat pe 01.07.2024 →
1 255,50 din 08.2024). Formula noastră citește deci valoarea de amortizat la
sfârșitul lunii evenimentului și restul/lunile la sfârșitul lunii
precedente. Cifrele contractului (150,00 după modernizare; 4 800/44 după
revizuire) rămân identice. Reconcilierea de mai jos confirmă pe 118 active.

**(e) Eligibilitatea pe DATE, nu pe `Stare`.** O fișă ieșită într-o lună
ulterioară e legitimă pe AMO-ul lunii curente (regenerarea unei luni
trecute după o ieșire). Verificarea `InFunctiune` din `ValideazaOperare` a
fost înlocuită de compararea mulțimii de linii, care e și criteriul `Stale`.

**(f) Linia cu contabil 0 și fiscal > 0 rămâne fără conturi.** Motorul sare
linia explicită fără conturi; cu conturi ar posta o notă de 0. Locul rămâne
obligatoriu (dimensiunea rândului de registru); conturile se cer doar la
`Valoare ≠ 0`. `Degresiva` cere durată multiplu de 12 (graficul e pe ani),
refuz la PIF.

**(g) Codul economic — dimensiune pe FIȘĂ (F26-r16, tranșată de owner la
pasul 2b).** Pe bugetar contul de cheltuială cu amortizarea are defalcarea
`E` în planul instituției, deci motorul cere cod economic pe latura de debit,
iar nici fișa, nici politica, nici linia nu-l purtau (conturile CAS au `S`,
de aceea pasul 1 nu lovise). Alternativele (pe politică: același tip
material pe coduri diferite; pe linie: culegere la fiecare lună; defalcare
`S` în seed: ascunde cerința planului) au pierdut în fața dimensiunii pe
fișă, editabilă administrativ ca `Loc`, copiată pe liniile AMO/CAS la
generare și verificată anti-stale. Celelalte dimensiuni bugetare
(funcțional, sursă, proiect) intră pe aceeași ușă dacă un plan le cere.

**(h) Nomenclatoarele pe OData, nu REST (D10 amendat la pasul 3).**
Contractul cerea `api/imobilizari` CRUD și `api/clasificari`; convenția
repo-ului (F2-D4, 42f, precedentul `ParteneriController`) pune
nomenclatoarele pe OData cu gardianul pe ușa comună. REST a păstrat doar ce
OData nu poate: `fisa` și `registru` (sume peste registru, 42c, cu al doilea
drept pe `RegistruImobilizari`). `AmortizareVieId?` a devenit `BlocantId` +
`BlocantNumar`/`BlocantStare` (acoperă și `NeCronologica`/`DraftAnterior`).

**(i) O singură aritmetică a consumului liniei sursă și un singur criteriu
anti-stale.** `ConsumatPeLiniiSursa` (pe lot, o interogare grupată) e
consumată de hook cu un singur id și de `linii-sursa`; filtrul de clasă F e
în SQL, înaintea plafonului 500 (`MaiSunt` = „mai sunt linii de clasă F în
perioadă", filtrul pe rest în memorie). `LiniileCorespund` extras din
`ValideazaOperare` e consumat de `Stale` (mesajul neschimbat). Liniile CAS
le produce serverul din `LiniiIesire`: clientul nu poate cere conturi.

**(j) Toate liniile rezolvate înaintea antetului (`PifApply`, F3-D5).**
Precedentul 86h: un `CreateObject` înaintea lui `Rezolva.Cere` lasă orfan
persistabil pe un OS cu viață lungă. `DviApply` are aceeași latență,
semnalată, neatinsă.

**(k) Clientul: bifa `Utilizare exclusivă` pleacă explicit `false`.** O
bifă nu poate spune „necules"; implicitul e neexclusiv, adică plafonul
fiscal se aplică — alegerea conservatoare. Prefill-ul pe `Revizuire` e
CITIRE din `fisa?laData=` (banda, situația curentă), nu calcul. Defectul de
TIPAR găsit la smoke — la `/:id` → `/nou` (Back) detaliul nu se remonta și
starea locală supraviețuia, un Back plus Creează dubla documentul — s-a
fixat generic (`key="nou"` pe toate rutele `/nou`).

**(l) Smoke-ul XAF a găsit un defect MAJOR de motor de registru (pasul 5).**
Anularea/stornarea unei AMO verifica doar o AMO ulterioară, nu și faptele
ulterioare ale fișelor ei: `AMO-4` s-a anulat sub `CAS-2` operată, lăsând
ieșirea cu cumulatul orfan (−100 fără +100). `VerificaFaraFapteUlterioare`
a PIF-ului a devenit comună (statică, `id`/`data` explicite) și AMO o
apelează după cronologia lunilor; probele `IMO-V51b…V51d` (decembrie
stornată + ieșire operată pe 20.12 ⇒ anularea și stornarea lui noiembrie
refuzate; după stornarea ieșirii anularea trece) pe ambele profiluri, plus
re-verificarea pe ușa REST (422). Tot la smoke: `PoliticaAmortizare` fără
`HideForeignKeys` (patru FK brute în listă), captions pe clasele de linii.

**(m) Catalogul: 590 din 598.** Opt sub-variante „a)/b)" fără cod propriu
sunt sărite; cei patru părinți ai lor rămân fără bandă (F26-r15: fără
verificare a duratei fiscale pe ei până la o decizie). `ClasificareImobilizari`
nu e `ICuProvenienta` (ar fi intrat forțat în `TipuriConfigurabile`); seed
prin upsert pe cod, ca `RandD300`.

## Reconcilierea cu 1C (Flax) — `RECONCILIERE-MF`

Fixture-ul (gitignored, `1C/mf/esantion-*.csv`, extras SELECT-only din Flax):
118 active, 2 559 rânduri lunare, 01.2023 → 2025. Recalcul cu `CotaLunara`
din parametrii datați, comparat lună cu lună cu suma postată de 1C:

| | rânduri | % |
|---|---|---|
| potriviri exacte | 2 502 | 97,77 |
| „ultima lună" (sub cotă, restul absorbit) | 44 | 1,72 |
| diferențe | 13 | 0,51 |
| **explicat** | **2 559** | **100** (99,49 % pe formulă) |

1 274 de rânduri au evenimentul înaintea primei luni din fixture, deci
restul lor nu e determinabil și se compară doar cu cota. Cele 13 diferențe
sunt TOATE din 05.2023 și vin dintr-un document MANUAL de recuperare (nu din
închiderea lunii), care cumulează mai multe luni într-un singur rând:

| activ | luna | așteptat | postat | Δ |
|---|---|---|---|---|
| SEDSED022 | 2023-05 | 161,87 | 971,22 | 809,35 |
| SEDSED053 | 2023-05 | 398,56 | 2 391,32 | 1 992,76 |
| SEDSED054 | 2023-05 | 161,87 | 971,22 | 809,35 |
| SEDSED055 | 2023-05 | 161,87 | 971,22 | 809,35 |
| SEDSED056 | 2023-05 | 161,87 | 971,22 | 809,35 |
| SEDSED057 | 2023-05 | 161,87 | 971,22 | 809,35 |
| SEDSED058 | 2023-05 | 138,91 | 833,47 | 694,56 |
| SEDSED059 | 2023-05 | 138,91 | 833,47 | 694,56 |
| SEDSED060 | 2023-05 | 138,91 | 833,47 | 694,56 |
| SEDSED061 | 2023-05 | 138,91 | 833,47 | 694,56 |
| SEDSED062 | 2023-05 | 138,91 | 833,47 | 694,56 |
| SEDSED063 | 2023-05 | 138,91 | 833,47 | 694,56 |
| SEDSED064 | 2023-05 | 138,64 | 831,86 | 693,22 |

Zero neexplicate. Blocul nu pică pe diferențe (21, 35b): le raportează;
pică doar pe fixture malformat.

## Review advers

Agent separat (2026-09-15), în worktree cu `MODELCHECK_BAZA_SUFIX`, cu
scenarii concrete de exploatare pe motor, ușile HTTP și client; fiecare
defect demonstrat printr-o probă care afirmă comportamentul așteptat
(`IMO-R1…R9`, scena în 2028, ambele profiluri) sau prin răspuns HTTP pe
hostul viu. Verdict inițial: două defecte MAJORE și șapte MEDII, toate cu
fix în frunze (`Documente/Imobilizari.cs`, `Nomenclatoare/Imobilizari.cs`,
`Politici.cs`, `AmortizareService.cs`, `PifApply.cs`), niciunul în
`MotorOperare`. Fix-urile le-a aplicat main-ul; probele au fost portate în
`ModelCheck` și trec.

- **R1 (MAJOR, reparat)** — două linii `Intrare` pe aceeași fișă în același
  PIF treceau (fișa e `Noua` pentru fiecare linie) și dublau brutul.
  `ValideazaOperare` refuză o fișă pe mai multe linii ale aceluiași
  document; `PifApply` refuză dublura la culegere.
- **R2 (MAJOR, reparat)** — anularea/stornarea unei ieșiri cu AMO operate
  pentru luna ieșirii sau ulterioare TRECEA: fișa revenea în funcțiune cu
  lunile acelea neamortizate și nimic n-o semnala (`LunaLipsa` vede doar
  luna precedentă). `EliminaRegistrul`/`StorneazaRegistrul` ale CAS au
  primit gardianul simetric celui de la operare (aceeași interogare).
- **R3/R4 (MEDIU, reparate printr-o singură regulă)** — stornoul unei
  ieșiri sau al unei amortizări datat într-o lună ulterioară lăsa situația
  la dată falsă între cele două date (aprilie neamortizat; februarie
  regenerată = două luni la 29.02). Regula 87g: un document de imobilizări
  se stornează cu o dată din luna lui (`VerificaLunaStornarii`, pe PIF, CAS
  și AMO). Alternativa (excluderea perechii rând + invers din `Situatie`
  indiferent de dată) ar fi schimbat semantica „la dată" a registrului (III).
- **R5 (MEDIU, reparat)** — `Revizuire` cu durata ≤ lunile deja amortizate
  era acceptată și vărsa tot restul într-o lună (luni rămase 0). Duratele
  revizuite trebuie să depășească lunile amortizate; la `Intrare`, lunile
  inițiale nu pot depăși duratele.
- **R6 (MEDIU, reparat)** — centrul de cost al fișei nu era în cheia
  anti-stale: un draft generat înaintea schimbării lui posta cu centrul
  vechi. `CentruCost` e a noua componentă a cheii.
- **R7 (MEDIU, reparat)** — fișa accepta un tip material de clasă de stoc
  (`POST` pe OData → 201): gardianul fișei cere natura `Imobilizare` la
  creare și la schimbarea tipului; `PoliticaAmortizare` implementează
  `IVerificabilLaCommit` cu aceeași regulă (nu prin `GardianEditare`, care e
  în `Motor/`).
- **R8 (MEDIU, reparat)** — ștergerea unei fișe referite de un PIF Draft
  trecea (FK-ul `Restrict` e inert sub ștergerea logică) și lăsa linia
  orfană; gardianul refuză ștergerea cât fișa e purtată de linii PIF/CAS/AMO.
- **R9 (MEDIU, reparat)** — ieșirea unei fișe neamortizate posta o notă de
  ZERO (28x = 21x de 0): `LiniiIesire` omite linia cumulatului la 0, ca pe
  cea a netului; gardianul CAS compară doar mulțimile.
- **Abateri nedeclarate, tranșate**: (i) D6 „`MesajeDupaOperare`
  avertizează dacă luna precedentă n-are AMO operată" era neimplementat —
  implementat (mesaj consultativ după commit, doar dacă fișele ieșite erau
  în funcțiune înaintea lunii precedente); (ii) antetul draftului AMO era
  editabil în XAF (`Data` într-o altă lună trecea la operare dacă liniile
  coincideau) — operarea cere `Data` = ultima zi a lunii și aceeași unitate
  pe ambele laturi; (iii) `Autogenerat = false` pe AMO e deliberat și
  consistent cu ITV (`Autogenerat` = latura generată a unui conex, nu
  „document generat de comandă"); (iv) `ValoareFiscala` 0 pe linia PIF
  (brut fiscal nerecunoscut integral) nu se poate exprima — `PregatesteOperare`
  o egalează cu `Valoare` la 0 (F26-r17); (v) `Comun/Scara.cs` primește
  numele noi de coloane în scara BANI (legitim, nedeclarat în note).
- **Observație lăsată ca restanță (R9 al raportului, măsurat)**:
  `VerificaFaraFapteUlterioare` folosește `Data >=`, iar operarea „luna >":
  o modernizare din luna unei AMO deja operate se operează (corect), dar
  anularea ei e refuzată deși rândul lunar nu s-a calculat pe ea — refuz
  fals, pe partea sigură (F26-r18).
- Confirmate OK: `Motor/*` conform D14 (diff verificat linie cu linie);
  niciun simbol de cont în cod; plafonul liniei sursă (stornoul eliberează,
  draftul consumă); cronologia PIF/AMO/CAS; anti-stale pe regulile
  șterse/modificate între previzualizare și operare; `Previzualizeaza` fără
  scrieri; rotunjirea prin convenția bazei; eligibilitatea pe date;
  bugetarul cu dimensiunea pe debit; `User` 404/403 pe `fisa`/`registru`;
  `RegistruImobilizari` absent din OData; `ApiEnum` fără fail-open;
  `Configurator` pe cele două politici; nimic calculat în TS; `key="nou"`
  fără efecte pe alte tranziții.
- Cosmetice rămase, fără impact: enum-urile CLR în mesaje (convenția
  serverului); titlurile de grupă ale catalogului cu `Cod` = text (CSV-ul
  ANAF); `Valoare` a regulii formatată monetar și la `Procent`; două
  `genereaza` concurente creează două drafturi care se blochează reciproc
  (identic cu ITV); `Situatie.Coalesce` ordonează evenimentele din aceeași
  zi după `ID` (GUID v7); `DurataLuni = 0` la intrare face fișa neeligibilă
  pentru totdeauna (terenuri), nediferențiat de o greșeală de culegere.

## Cifrele

| | la deschidere (86, 2026-09-13) | la închidere |
|---|---|---|
| ModelCheck bugetar | 955 / 0 FAIL | 1068 / 0 FAIL (cu `IMO-R0…R9`) |
| ModelCheck privat | 1068 / 0 FAIL | 1184 / 0 FAIL (cu `IMO-R0…R9`, `RECONCILIERE-MF`) |
| `refuzuri.ps1` | 156 / 156 | 229 / 229 (host viu Privat, după fix-urile review-ului) |
| `Motor/*` | — | `AmortizareService.cs` nou (427 linii); `MotorOperare` +8 (trei dispecere), `GardianEditare` +8/−3 (registrul al patrulea, `Originale` internal), `VerificareProfilService` +14/−3 (etichetele politicilor) |
| Migrații | — | `F26Imobilizari` (11 tabele noi, zero atingeri pe cele existente), `F26CodEconomicImobilizare` (trei coloane nule) |
| Import1C integral | — | 2026-09-14 16:39–19:19, raport IDENTIC cu baseline-ul F18 (467/467 linii sortate) |
| Catalog | — | 590 / 598 de poziții |

## Ce rămâne deschis (restanțele deciziei)

Restanțele poartă numele din contract (`F26-rN`).

- **F26-r1** activele din 231 (în curs): PIF care postează 21x = 231.
- **F26-r2** deductibilitatea valorii rămase la ieșire (art. 28 (17)) ca `Fel` nou de regulă pe rândul `Iesire`.
- **F26-r3** legătura CAS ↔ FCL de vânzare ca evidență (tiparul `DviFactura`).
- **F26-r4** degresiva AD2; amortizarea pe unități de producție.
- **F26-r5** transferul ca rând explicit de registru (`Fel = Transfer`, valoare 0).
- **F26-r6** reevaluarea (locul rezervat: `Fel`, coloanele; 21x = 105 prin politică extinsă; 105 → 1175 la ieșire).
- **F26-r7** acțiunea XAF de generare AMO.
- **F26-r8** SAF-T D406 Assets + AssetTransactions din registru.
- **F26-r9** D101 / impozitul pe profit ca proiecție peste `AmortizareDeductibila`.
- **F26-r10** ajustările pentru depreciere (29x), leasingul, obiectele de inventar date în folosință (8035).
- **F26-r11** repartizarea cheltuielii pe mai multe centre de cost cu coeficienți.
- **F26-r12** `RegistruImobilizari` pe `ServerView` (85).
- **F26-r13** migrarea fișelor din 1C (`IntroducereSolduriInitialeMF` → PIF de deschidere cu inițialele): conectorul, nu mecanismul.
- **F26-r14** eligibilitatea metodei fiscale pe categorie (accelerata doar pe echipamente/calculatoare, art. 28 (12)) — azi doar documentată.
- **F26-r15** cele 4 poziții-părinte din catalog ale căror benzi stau pe sub-variante fără cod: fără verificare a duratei fiscale până la o decizie.
- **F26-r16** — închisă la pasul 2b (dimensiunea `CodEconomic` pe fișă).
- **F26-r17** brutul fiscal 0 pe linia PIF (cost integral nerecunoscut fiscal) nu se poate exprima: `ValoareFiscala` 0 = „implicit = `Valoare`" (review).
- **F26-r18** anularea unui eveniment PIF datat în luna unei AMO deja operate e refuzată deși rândul lunar nu s-a calculat pe el (`VerificaFaraFapteUlterioare` pe `Data >=`, operarea pe „luna >") — refuz fals, pe partea sigură (review, `IMO-R9`).
- **F26-r19** lookup-urile XAF ale fișei (`TipMaterial` pe natură) și ale liniei PIF (fișele pe stare și loc) nefiltrate; gardianul refuză oricum (smoke pas 5).
- **F26-r20** `Clasificare` căutabilă în lookup-ul XAF doar pe `Denumire` (coloana `Cautare` există, lookup-ul n-o folosește); `Valoare` a regulii de deductibilitate formatată monetar și la `Procent`.
- **F26-r21** filtrele `FilterRow` pe coloanele cu `Lookup` de enum (`Cauza`) neverificate în browser.
- **F26-r22** două `genereaza` concurente pe aceeași lună creează două drafturi care se blochează reciproc (`AmortizareVie`), fără unicitate pe lună în schemă — identic cu ITV.
