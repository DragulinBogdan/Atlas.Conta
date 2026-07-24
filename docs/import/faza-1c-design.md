# FAZA 1C — conector de import + reconciliere pe an fiscal complet (design)

> **Statut**: FIXAT — toate tranșările confirmate 25.07.2026. Implementarea urmează
> (feliile în §11). Contextul de roadmap: decizia 44 (pasul 5 amânat; faza 1C =
> călirea profilului privat pe date reale). Filosofia: **1C e evidență și direcție,
> niciodată canonic** (deciziile 21/35b) — politicile se definesc curat pe măsură ce
> importul scoate găurile.
>
> Inventarul complet al surselor 1C: `1c/01-1c-structura-si-acces.md` (gitignored,
> conține date sensibile). Precedentele de unealtă: `nou/tools/Migrare` (conector),
> `nou/tools/ModelCheck` (arbitrul modelului).

## 1. Fapte pe care stă designul (verificate pe surse, 25.07.2026)

### Sursa

- Baza 1C `flax` pe SQL Server `(local)`, trusted connection; view-urile lizibile
  generate de SkyContaSource există în **`EServicesFlx`, schema `[flax]`** —
  262 de view-uri, verificate azi pe date (`NoteContabile`, `DefalcareNote`,
  `PlanConturi`, `Balanta`/`BalantaNivel1..3`, `TVACumparari/Vanzari`, documentele
  cu secțiunile lor tabulare, `FullTables`).
- **2025 = anul fiscal complet țintă**: ~399.500 rânduri în registrul contabil,
  documente de închidere de lună pe toate cele 12 luni (`InchidereLunaDeExercitiu`
  SED…01–12). 2026 e parțial (până în iunie).
- **Închiderea lunară 1C postează**: 4427=4426 (transferul deductibilei),
  4427=4423 (TVA de plată), 681=281 (amortizare), 6xx/7xx=121.25 (închiderea
  P&L). Plata TVA-ului: 4423=512.1 prin extras.
- **Politica de evidență** (`InfoRg_PoliticaDeEvidentaPentruContabilitate`):
  **FIFO**, plătitor de TVA lunar, **TVA la EMITERE** (nu la încasare).
- **Loturile sunt vii în `flax`** — nu prin registrul de acumulare `Loturi`
  (0 rânduri), ci ca **subconto pe conturile de stoc**: tripletul
  (Nomenclator, Lot = documentul de achiziție, Depozit); ~24.000 loturi
  distincte/an. `BalantaNivel3` dă stocul per lot cu **cantitate + valoare**
  (deschiderea 01.01.2025: 11.599 poziții, 9,5M lei pe 371).
- Plan de conturi cu **coduri punctate** (442.6 ≡ 4426, 121.25, 401.1…),
  652 conturi; **o singură organizație** (FLAX).
- Cote de TVA folosite în 2025: TVA19, TVA9, TaxareInversa, Neimpozabile,
  ScutiteTVAFara (+ rânduri fără cotă) — **seed istoric de 19% necesar** în
  TipTva (N19/TI19…), pe lângă rândurile 21% din P1.
- **Retururile 1C sunt documente cu sume NEGATIVE** pe corespondențele
  originale (RDC: 411=707, 607=371, 411=4427 toate cu minus; RLF: 371=401,
  4426=401 cu minus) și **creează loturi negative** în stoc (returul devine
  „lot" separat cu sold negativ, lotul original rămâne intact).
- **Asamblarea postează exclusiv 371.1=371.1** (7M lei/an) — la plan sintetic
  e zgomot (raționamentul deciziei 23c); efectul real e pe loturi (n componente
  → produs). Dezasamblarea e inversul (1→n).
- Mixul de documente 2025 (documente, prin registrul contabil):

  | 1C | Docs 2025 | Țintă Atlas |
  |---|---:|---|
  | TransferDeMarfuri | 45.871 | BTR |
  | VanzareMarfuriSiServicii (toate seriile) | ~39.800 | FCL + DSC |
  | AprovizionareMarfuriSiServicii | 19.120 (34.471 linii marfă) | FCT + NIR conex |
  | ExtrasDeCont | 2.666 (54.366 rânduri) | PLT/INC per rând |
  | ReturDeLaClient / ReturLaFurnizor | ~1.670 / 569 | RDC / RLF (tipuri noi) |
  | Operatia (notă manuală) | 874 | NTC (tip nou) |
  | Compensare | 869 | NTC + Imperechere |
  | RaportDeVanzariCuAmanunt | 809 | surogat FCL+DSC |
  | Asamblare / Dezasamblare | 672 / 191 | ASM (tip nou) |
  | BonDeConsum | 542 | BCS |
  | Plata/Incasare/Ordine/Dispoziții/Card | ~1.000 | PLT/INC |
  | AvizDeIesire / AvizDeIntrare | ~360 / mici | surogat (vezi §4) |
  | MarireStoc / DiminuareStoc | mici | LDI ± |
  | Salarii (sume totale) | 12 | NTC |
  | InchidereLunaDeExercitiu | 12 | NTC (fără rândurile de TVA — §6) |
  | BonFiscal | — | nu postează (agregat în RVA) — nu se importă |
  | IntroducereaSoldurilor | — | deschiderea (nu ca document) |

- **Capcane de sursă** (toate verificate): rânduri cu anul nedeplasat rezidual
  în `Balanta` (perioade 3999), `nchar` cu spații (LTRIM/RTRIM peste tot),
  split per valută în balanțe (agregare la extracție), view-urile cu câmpuri
  polimorfe au ~30 de join-uri — **filtrare pe `Period` obligatorie**, tratare
  ca hartă nu ca hot path.

### Uneltele existente (rapoarte scout 25.07.2026)

- **Migrare** (`nou/tools/Migrare`, 3 fișiere): `EFCoreObjectSpaceProvider`
  standalone fără XAF Application; `MigrareLegatura` + `Legaturi()/Leaga()`
  (idempotență pe chei externe); `TrimCont` (tăierea analiticelor la planul
  sintetic); reconcilierea = ultimul bloc al aceluiași proces (recitire din
  Postgres + diff cu toleranță 0.005 + raportare fără ascundere). **NU apelează
  motorul** — deschiderea se scrie direct (`DocumentId=null`, decizia 25e).
- **ModelCheck** (`nou/tools/ModelCheck`): dovedește că **MotorOperare rulează
  complet pe provider standalone** (toate scenariile e2e operează documente prin
  motor exact pe infrastructura pe care o va folosi conectorul); pe profil privat
  își migrează + seed-uiește singur baza dedicată (`ContaSeeder.Seed` direct —
  aceeași cale ca updater-ul). Proiecțiile din decizia 42c NU există încă —
  nu anticipăm nimic din pasul 5.

## 2. Tranșarea 1 — sursa: view-urile SkyConta, nu tabelele fizice

Interfața de citire a conectorului = **view-urile `[flax]` din `EServicesFlx`**.
Conectorul nu depinde de codul SkyContaSource, doar de contractul view-urilor
(nume + coloane); regenerarea lor e responsabilitatea pipeline-ului existent
(EServices, rețeta 8.A din inventar). `OneCvProvider` / conectorul generic fără
dump rămâne al proiectului de import viitor (decizia 35a) — nu-l validăm acum.

Precondiții și igienă de extracție (în conector, documentate în cod):

- **Log curat la generarea view-urilor** (capcana mapării poziționale §4.3 din
  inventar) — precondiție de rulare, verificată o dată per regenerare.
- LTRIM/RTRIM pe orice `nchar`; corecția reziduală de an (perioade > 3000);
  agregarea rândurilor per valută la balanțe; sume `numeric` citite ca decimal.
- Extracțiile mari (NoteContabile, DefalcareNote) — întotdeauna filtrate pe
  `Period`, citite secvențial pe luni.

## 3. Tranșarea 2 — ce se importă: anul 2025, operat prin motor

Scopul e **reconcilierea ca harness de validare** a modelului și profilului
privat — nu go-live-ul (deciziile 12/18 neatinse: la un go-live real istoricul
rămâne în sursă; aici istoricul E materialul de test).

- **Deschiderea la 01.01.2025**: solduri per cont din `Balanta` (contra
  891.01.00, pattern-ul 34d — 891 există și în OMFP), dimensiunile disponibile
  pe latura contului; stocul din `BalantaNivel3`: un `Lot` Atlas per poziție
  (produs × doc achiziție × depozit) cu PretUnitar = valoare/cantitate + rând
  `RegistruStoc` de deschidere (`DocumentId=null`). **Pozițiile negative de lot**
  (artefactul returului-ca-lot) se **netează per produs×gestiune** și se
  raportează — gardianul de sold Atlas cere ≥0 per lot.
- **Documentele 2025 în ordine cronologică, prin `MotorOperare`** — maparea în
  §4. Operarea prin motor (nu copierea în registre) e esența fazei: fiecare
  document real exersează gardienii, rezolvarea declarativă, TVA-ul structural.
- **Idempotență prin `MigrareLegatura`** per document/nomenclator
  (`Tabela="1C:…"`, cheia = `_IDRRef` hex); re-rularea sare ce există — și e
  mecanismul de **resume** pentru rulările lungi.
- **Bază dedicată de import** (`Atlas.Conta.Import1C.Flax`), profil **Privat**,
  migrată + seed-uită de unealtă (calea ModelCheck.Privat / ContaSeeder direct).
  De unică folosință — se poate arunca și reconstrui.
- Scara: ~130.000 documente/an prin motor — rulare lungă asumată (consolă,
  progres pe luni, resume prin legături). Fără optimizări premature; dacă doare,
  se măsoară întâi.

## 4. Maparea tipurilor de documente

Regula transversală: **stocul nu se mișcă fără registru de stoc** — orice tip 1C
care mișcă 3xx se mapează pe un tip Atlas cu reguli de stoc (existent sau nou);
doar tipurile pur contabile trec pe NotaContabila.

| 1C | Atlas | Mecanism |
|---|---|---|
| AprovizionareMarfuriSiServicii | **FCT** | Linii marfă + servicii pe aceeași factură (26a); lotul se naște pe linia FCT (26e), legat prin MigrareLegatura de perechea 1C (doc achiziție × produs) pentru referirile ulterioare din subconto; NIR-ul conex îl generează motorul. TipTva din CotaTVA + TaxareInversa; DECONT_*/scadența din header. |
| VanzareMarfuriSiServicii | **FCL + DSC** | FCL cu liniile de venit (ContVenituri per linie → Tipul VEN corespunzător); descărcarea NU se re-pichează FIFO: liniile DSC se construiesc din rândurile 607=371 ale 1C cu **lotul explicit ca pin** (mecanismul 37d există) — valoarea o calculează Atlas din prețul lotului, egalitatea cu 1C decurge din aceleași loturi × cantități. |
| TransferDeMarfuri | **BTR** | ±1 pe lot, gestiuni din subconto Depozite. |
| BonDeConsum | **BCS** | Direct. |
| MarireStoc / DiminuareStoc | **LDI** ± | Direcția explicită (28a). |
| Plata/Incasare, OrdinDe*, Dispoziții, Card, ExtrasDeCont | **PLT / INC** | Extrasul: un PLT/INC per rând; conturile proprii din Casierii/ConturiBancare. Stingerile din subconto Documente → **Imperechere** prin ImperechereService. |
| Compensare | **NTC + Imperechere** | Nota 401=411 ca NTC; linkurile ca imperecheri. |
| ReturLaFurnizor / ReturDeLaClient | **RLF / RDC** (tipuri noi, §7) | |
| Asamblare / Dezasamblare | **ASM** (tip nou, §7) | n→m; valorile per linie din 1C. |
| RaportDeVanzariCuAmanunt | surogat **FCL + DSC** | FCL pe partener generic „consumator final" (venit + 4427 agregat), DSC din rândurile 607=371 (loturi explicite). |
| AvizDeIesire / AvizDeIntrare | surogat | Descărcarea/încărcarea de stoc pe **DSC**/tip de stoc corespunzător cu loturile 1C + **NTC** pentru 418/4428; detaliat la implementare. Aviz ca tip propriu = amânat (§10). |
| Operatia, Salarii, CasareMF/amortizare, închiderea 121 | **NTC** | Postare explicită per linie; dimensiunile disponibile din subconto. |
| InchidereLunaDeExercitiu | **NTC parțial** | Rândurile 681=281 și 6xx/7xx=121 se importă ca NTC; **rândurile 4427=4426 și 4427=4423/4424 se SAR** — le generează Atlas (§6), reconcilierea le compară. |
| BonFiscal, jurnale, registre derivate | — | Nu postează / derivate — nu se importă. |

Nomenclatoarele: Partenerii → Partener (CUI/RegCom/flags), Depozite → Gestiune,
Casierii + ConturiBancare (proprii) → ContPropriu, PersoaneFizice → Angajat,
Nomenclator → Produs (loturile la import, per §3), planul de conturi NU se
importă (planul Atlas = OMFP din seed; maparea codurilor în §8), Clasă/Tip:
liniile 1C poartă ContEvidenta/ContVenituri — Tipul Atlas se alege prin maparea
cont → TipMaterial (mecanismul „Cod Tip = simbol de cont", 26b); găurile de
nomenclator (TipMaterial lipsă pentru un cont folosit) = semnal de completare a
profilului, nu blocaj mut.

## 5. Tranșarea 3 — tip nou `NotaContabila` (NTC)

Mecanismul de **import de note contabile** anticipat de decizia 9 (salarii,
imobilizări intră în contabilitate prin note) — faza 1C doar îl forțează acum.

- Derivat nou (al 12-lea): header standard (laturile = unitatea internă pe
  ambele părți sau necompletate semantic — de fixat în felie), detaliu propriu cu
  **`ILinieCuPostareExplicita`** (contract existent, 32a): ContDebit/ContCredit
  obligatorii pe linie, RepartitorDebit/Credit opționali, `Valoare` culeasă,
  `Dimensiuni` pe linie.
- **Fără reguli de stoc, fără reguli de contare** — postarea explicită a liniei
  bate rezolvarea declarativă (mecanismul 32a în motor există deja); gardienii
  generici rămân activi (perioadă, dimensiuni obligatorii per cont).
- E și tip de CULEGERE manuală (nota contabilă manuală = cerință reală — 874
  „Operatia"/an în flax), nu doar ușă de import.

## 6. Tranșarea 4 — închiderea lunară de TVA: `InchidereTva` (ITV) + serviciu

Iese din amânarea 36f. Precedentul de formă: **DescarcareService** (37b —
liniile se nasc la GENERARE, draftul concret e condiția override-ului).

- **`InchidereTvaService.Genereaza(os, perioada)`** (motor): calculează
  soldurile 4426/4427 din `RegistruContabil` pe luna dată → **draft `InchidereTva`**
  cu linii cu postare explicită: 4427=4426 pe minim, excedentul de 4427 → 4423
  (TVA de plată), excedentul de 4426 → 4424 (TVA de recuperat). Operarea =
  motor standard (nimic special în `Opereaza`); anulare/storno/regenerare = 
  gardienii existenți.
- **Conturile = politică-date per profil**: rând nou de politică
  (`PoliticaInchidereTva`: TipDocumentId ITV + ConturiDeductibila/Colectata/
  DePlata/DeRecuperat) — seed privat 4426/4427/4423/4424; bugetar fără rând
  (tip inert, ca DSC la bugetar).
- Închiderea **nu închide perioada** (GardianPerioada rămâne mecanism separat)
  și **nu atinge 121** — închiderea de exercițiu nu intră în scope (§10).
- Amânatele 36f rămân amânate: TVA la încasare (flax e pe TVA la emitere —
  verificat), 4428/facturi nesosite, prorata, D300/D394 ca proiecții.

## 7. Tranșarea 5 — tipuri noi de produs: ASM, RLF, RDC

**`Asamblare` (ASM)** — tip separat; **BPR rămâne rezervat** (decizia 19
neatinsă: producția reală cu rețetar/chei de distribuție e alt subiect; forțarea
semanticii ar polua clasa rezervată).

- n consumuri → m produse pe Natura=Stoc; detaliu cu **`Directie`**
  (Consum/Produs — precedentul LDI 28a).
- Consumurile descarcă loturi EXISTENTE (valoare = preț lot × cantitate,
  pattern BCS); liniile de produs **creează lot** (CreeazaLot) cu valoare
  EXPLICITĂ per linie (`PretEvaluare`, ca LDI-plus).
- Invariantul alocării, validat la operare: **Σ valori produse = Σ valori
  consumuri**. Alocarea AUTOMATĂ (rețetar, chei) rămâne amânată — valoarea se
  culege / vine din import.
- Contare: politică-date; seed-ul flax/privat NU pune reguli pe marfă→marfă
  (371=371 = zgomot la sintetic — raționamentul 23c); producția reală (345=711)
  primește reguli când apare cerința.

**`ReturFurnizor` (RLF)** / **`ReturClient` (RDC)** — cerință de PRODUS
(fluxul de magazin), nu doar de import.

- **RLF**: laturi Gestiune→Partener; liniile referă **lotul original**;
  stoc −1; valoare din prețul lotului; contare = stornarea achiziției
  (401=3xx) + stornarea TVA deductibile pe linie.
- **RDC**: laturi Partener→Gestiune; stocul **REVINE pe lotul original**
  (+1 pe lot existent — mecanic deja posibil, BTR o face); venit storno +
  TVA colectată stornată + costul revine (3xx=607, inversul DSC).
- Detaliile fine se fixează în felia de implementare, cu spike: reprezentarea
  storno (negativ pe aceeași latură vs pozitiv pe latura inversă — aliniată cu
  convenția existentă a motorului, 25d) și spargerea venit/cost la RDC (un
  document vs pereche ca FCL+DSC). Designul fazei fixează: sunt **tipuri de
  sine stătătoare** cu semantica economică de mai sus.

### Rezoluția spike-ului storno (felia 1C-a, 25.07.2026)

**Reprezentarea = valori NEGATIVE pe corespondența ORIGINALĂ** (minus pe
aceeași latură), fără flag `Storno` pe rânduri:

- E deja convenția motorului: `Storneaza` (25d) păstrează ContDebit/ContCredit
  și neagă Valoarea — alinierea cerută = aceeași reprezentare pe RLF/RDC. E și
  reprezentarea 1C (rulajele devin reconciliabile ulterior, §10, fără
  conversie), iar jurnalele de TVA / D300 / D394 ca proiecții (35c) culeg
  natural minusul din același jurnal.
- Mecanica: liniile se CULEG pozitive; `PregatesteOperare` semnează negativ
  Cantitate, Valoare, ValoareTva (idempotent prin Abs — precedent LDI 28a;
  direcția e fixă per tip, nu enum). **Stoc: zero schimbări de motor** —
  RLF: RegulaStoc Semn=+1 pe Predator (gestiune) → −q pe lotul original;
  RDC: Semn=−1 pe Primitor (gestiune) → +q (revine pe lot). **Pasul TVA:
  zero schimbări** — ValoareTva negativă postează minusul pe corespondența
  direcției: RLF (Deductibil, contrapartida RepartitorPrimitor→401) →
  4426=401 −TVA; RDC (Colectat, contrapartida RepartitorPredator→4111) →
  4111=4427 −TVA. Exact rândurile 1C.
- **Singura extensie de motor: `RegulaContare.PastreazaSemn`** (bool, aditiv):
  valoarea liniei se postează CU semnul ei (fără normalizarea `SemnFiltru`),
  pe corespondența ORIGINALĂ a achiziției/vânzării. Seed RLF: per-TipMaterial
  debit=TipMaterial (3xx), credit=RepartitorPrimitor (fallback 401) →
  3xx=401 −V. Seed RDC: venit debit=RepartitorPredator (fallback 4111),
  credit=TipMaterial (70x) → 4111=70x −V; cost = derivarea 607=371 cu
  excepțiile profilului (ca DSC) → 607=371 −cost.
- Rândurile returului NU poartă `Storno` — flag-ul rămâne al meta-operației
  `Storneaza` (stornarea unui retur = rânduri pozitive cu Storno=true,
  consistent); proveniența = TipDocument.

**Spargerea venit/cost la RDC: UN singur document, linii pe două roluri**
(venit: Tip VEN, preț de vânzare cules, TipTva/ValoareTva, fără lot/stoc;
cost-stoc: Tip marfă, LotId original, cantitate, Valoare = −cost din prețul
lotului) — NU pereche FCL+DSC:

- Perechea FCL+DSC există pentru decuplarea temporală (backorder), pe care
  returul n-o are — marfa sosește CU documentul; 1C = un document (import
  1:1: rândurile 411=707 → linii de venit, 607=371 cu lot → linii de cost);
  precedentul de formă e ASM (aceeași felie: linii cu rol + invariant).
- O linie nu poate purta două valori de postare diferite (testul bazei 22a)
  ⇒ două linii; motorul multi-regulă per linie se respinge (ar rupe modelul
  de specificitate al potrivirii). RegulaStoc generică Natura=Stoc atinge
  doar liniile de stoc (VEN e Serviciu) — separarea rolurilor e naturală.
- `Total` pe RDC se overrideaza = Σ liniilor de venit (brutul care ajustează
  creanța); generarea liniilor de cost din liniile de venit pin-uite pe lot
  poate fi acțiune/serviciu (precedent DescarcareService) — detaliu de
  implementare.
- Semnalat pentru 1C-c, netranșat: imperecherea returului (RLF/RDC nu sunt
  DocumentTrezorerie; compensarea cu factura originală / rambursarea — se
  tranșează unde apare Compensarea, maparea §4).

## 8. Tranșarea 6 — contractul de reconciliere

Trăiește în **conector** (precedentul 34f — Migrare); ModelCheck rămâne arbitrul
modelului pe date sintetice, neatins.

Numerele care trebuie să bată (după fiecare lună importată + la final):

1. **Sold per cont sintetic OMFP per lună** = `Balanta` 1C normalizată
   (maparea cod punctat → OMFP prin dicționarul conectorului — CSV în tool;
   agregare valute; corecție an; analiticele tăiate cu `TrimCont`).
   **Rulajele = doar informativ**: reprezentarea storno 1C (minus pe aceeași
   latură) face rulajele structural nereconciliabile fără aliniere de convenție.
2. **4423/4424 per lună**: rândurile generate de `InchidereTva` = sumele 1C
   din închiderea lunii — **forcing function-ul TVA-ului structural P1**.
3. **Stoc per produs×gestiune la fine de lună** (cantitate + valoare) =
   `BalantaNivel3` agregat pe conturile de stoc. **Per lot NU e țintă** —
   loturile negative 1C (retururi) fac per-lot zgomot permanent.
4. **891.01.00 se închide la 0** după deschidere (34f).

Toleranță 0.005 per cont/poziție; diferențele SURSEI se raportează explicit,
nu se ascund (34f); contractul picat = cod de ieșire ≠ 0 cu raportul complet.

## 9. Tranșarea 7 — conectorul: `nou/tools/Import1C`

- **Consolă separată, ca Migrare** (Program.cs pe faze + cititor propriu
  `FlaxDb` — SqlClient raw pe view-uri, DTO-uri record); referă Module.
- Refolosește **pattern-urile** Migrare (provider standalone, MigrareLegatura +
  Legaturi/Leaga, TrimCont, structura de faze, blocul de reconciliere adaptat)
  prin copiere/adaptare — **fără bibliotecă comună prematură** (Migrare a
  înghețat, 35a; factorizarea se face când al treilea conector o cere).
- **Diferența de fond față de Migrare: apelează motorul.** Documentele se
  operează prin `MotorOperare` (precedentul ModelCheck pe provider standalone);
  ObjectSpace per document/grup mic; ordine cronologică.
- Profilul privat se completează PE PARCURS: seed TipTva istoric 19%, Clasă/Tip
  pentru marfă, partener generic retail, PoliticaInchidereTva, politicile
  tipurilor noi — fiecare gaură descoperită = decizie explicită documentată
  (21/35b), nu transcriere din 1C.

## 10. Amânate, documentate

- **TVA la încasare, 408/4428 facturi nesosite, prorata, regularizarea de
  rotunjire, D300/D394/SAF-T ca proiecții** — rămân amânatele 36f/35c.
- **Închiderea de exercițiu (121) ca mecanism Atlas** — notele 1C se importă;
  mecanismul propriu se proiectează când devine cerință de produs.
- **Aviz de însoțire ca tip propriu** — surogatul §4 acoperă importul; tipul
  se proiectează la cerință reală.
- **Conectorul 1C generic** (OneCvProvider, fără dump, multi-configurație) —
  proiectul de import (35a); Import1C e al doilea prototip care îl informează.
- **Rulajele în contract** — după alinierea reprezentării storno, dacă va fi
  nevoie.
- Multi-organizație, multi-valută, salarizare/MF ca module.

## 11. Contractul feliilor (execuție, o sesiune per felie)

- **1C-a. Tipurile noi de model**: NTC, ITV + `InchidereTvaService` +
  `PoliticaInchidereTva`, ASM, RLF/RDC (cu spike-ul de reprezentare storno);
  politici seed per profil; scenarii e2e sintetice în ModelCheck per tip
  (disciplina feliilor 3c).
- **1C-b. Scheletul Import1C + nomenclatoare + deschiderea**: FlaxDb,
  MigrareLegatura pe surse 1C, deschiderea 01.01.2025 (solduri + loturi din
  BalantaNivel3, netarea negativului raportată), reconcilierea deschiderii
  (891→0, stoc per produs×gestiune = sursa).
- **1C-c. Documentele prin motor**: maparea §4 în ordine cronologică, pe luni;
  reconcilierea lunară de solduri + stoc; găurile de profil descoperite se
  tranșează pe măsură.
- **1C-d. Închiderea de TVA + contractul final**: ITV generat per lună,
  reconcilierea 4423/4424 (forcing function), raportul complet al anului —
  gate-ul fazei.
