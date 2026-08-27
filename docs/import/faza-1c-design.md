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

Nomenclatoarele: Partenerii → Partener (CUI/RegCom/flags + identitatea fiscală
D394: `PersJurFiz` → `TipPersoana`, `Tara_ID` → `Tari.CodAlfa2` → `Tara`,
`PoliticaTVA`/`DataLuariiInEvidentaTVA` → `InregistratTva`/`TvaLaIncasare`, PF cu
`CNP` ⇒ `CodFiscal = CNP`; unde sursa tace se derivă și se RAPORTEAZĂ — vezi
`ImportLaCerere.AplicaClasificare`; **adresa structurată** din
`flax.InfoRg_InformatiaDeContact` — `Type = N'Adresa'`,
`Gen_TipuriDeInformatiiDeContact in (N'Sediu social partener', N'Punct de lucru
partener')`, un rând per partener cu sediul social înaintea punctului de lucru și
`SimpleKey desc`: `Field1` → `CodPostal`, `Field4` → `Localitate`, `Field6` →
`Strada`, `Field7` → `Numar`, `DetaliiAdresa` = segmentele `bl./sc./et./ap.`
din `Present` (etajul și apartamentul stau DOAR în forma concatenată; fără
ele, `Field8` = blocul), iar județul din `CodJudet` (codul din CNP) cu `Field3`
(denumirea; „Sector n" ⇒ București, prefixul „Jud." tăiat) ca rezervă —
nerezolvat ⇒ denumirea brută intră în `DetaliiAdresa`; se scrie DOAR pe bloc
gol — vezi `ImportLaCerere.AplicaAdresa`, felia 15/D15-D6. Pe axa TVA, un
partener cu timbru `DataSincronizareAnaf` NU mai e atins de clasificarea din
sursă și nici de semnalul din registru: registrul ANAF e canonicul (D4-r1),
contradicțiile se numără — `--anaf` = pasul final opțional, `Anaf1C.cs`),
Depozite → Gestiune,
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
   **Amendament pre-1C-c (§12.1): cantitatea = strictă peste tot; valoarea =
   strictă pe grupele neatinse de netarea deschiderii, iar pe grupele netate
   (set cunoscut din 1C-b) diferența valorică se raportează ca justificată**
   („netare deschidere") — netarea a schimbat prețurile per lot, deci Atlas
   descarcă la alte prețuri decât 1C; diferența e mărginită de valoarea
   redistribuită și se stinge când grupa se golește.
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
  bucla lunii = documente → imperecheri (trecerea 2) → **ITV la fine de lună**
  (relaxarea graniței 1C-c/1C-d, §12.4: fără închiderea de TVA, contractul (1)
  ar pica lunar pe 4426/4427/4423 — Balanța 1C ARE închiderea în ea) →
  reconcilierea lunară de solduri + stoc; găurile de profil descoperite se
  tranșează pe măsură.
- **1C-d. Gate-ul fazei**: rularea integrală a anului, reconcilierea 4423/4424
  pe toate cele 12 luni (forcing function), raportul complet al anului.

## 12. Tranșările pre-1C-c (analiza 25.07.2026 — toate confirmate)

Patru probleme analizate explicit înainte de execuția 1C-c; unde a fost nevoie,
deciziile anterioare s-au relaxat conștient, nu s-au ocolit.

### 12.1 Pin pe lot vs. soldurile netate → supapă de IMPORT, motorul neatins

Problema e a întregii familii de ieșiri pe lot (BTR — cel mai mare tip, BCS,
consum ASM, RLF, DSC pin-uit), nu doar a DSC: netarea din 1C-b a rearanjat
soldurile ȘI prețurile per lot, deci gardianul de sold poate refuza pe orice
lot atins. **Decizia 37d („pinul fără fallback FIFO") rămâne intactă** —
netarea e artefactul importului, consecința se plătește în import:

- Helper unic în Import1C pentru toate ieșirile pe lot: cererea = (lot dorit,
  produs, gestiune, TipStoc, cantitate); ce are pinul se ia de pe pin,
  deficitul se realocă FIFO în interiorul produs×gestiune (grupa în care
  netarea a conservat sumele — acoperirea există prin construcție, mai puțin
  grupele sărite); mapă `dejaAlocat` per document pentru contenția
  intra-document (pattern-ul DescarcareService).
- Realocările se numără și se raportează per lună — diagnostic, nu eșec.
- Consecința pe contract: amendamentul de la §8.3 (valoarea pe grupele netate
  = diferență justificată, purtată înainte lună de lună).

### 12.2 Mecanica per tip rămasă „la implementare" (cele cinci)

- **ExtrasDeCont per rând**: PLT/INC per rând, contul propriu din
  nomenclatoarele importate, direcția din semnul rândului, linia pe TRZ.
- **Stingerile din subconto → Imperechere: DOUĂ treceri per lună** — trecerea
  1 operează documentele cronologic, trecerea 2 creează imperecherile lunii
  (imperecherea nu postează registre, amânarea e gratuită și elimină problema
  de ordine). Ținta legăturii nu există (factură 2024 pe soldul global de
  deschidere — modelul 34d/47c — sau tip neimportat, ex. BonFiscal) → fără
  imperechere, contor + raport. Invariant picat pe date reale → raport, nu
  stop (reconcilierea nu depinde de imperecheri).
- **Compensare → NTC + Imperechere, cu EXTENSIE DE INVARIANT ca decizie de
  produs** (compensarea e stingere reală — 869/an): un `NotaContabila` operat
  poate sta pe rolul de stingere, invariantul de trezorerie (31d) reformulat
  pentru el — contrapartida stinsă (401/411) apare pe liniile lui explicite;
  restul invarianților neschimbați. **Retururile** (totalul negativ — 46f):
  skip + raport la 1C-c; designul compensare/rambursare pe retur la momentul
  lui.
- **Surogat RVA**: FCL pe partenerul generic „consumator final" (rând de
  seed, decizie de profil), linii per (cont venit × cotă) cu Tipul VEN, DSC
  din 607=371 cu pin prin supapa §12.1. **Datorie P1 scoasă la iveală:
  regula „ValoareTva culeasă nenulă NU se suprascrie" (36a, azi doar FCT) se
  uniformizează pe FCT/FCL/DEC** — altfel TVA-ul recalculat diferă prin
  rotunjire de 4427-ul 1C și contractul (2) pică pe bani mărunți; aditivă,
  cu sens de produs (rotunjirea e-Factura, 36f).
- **Avizele + rețeta generală de surogat** (aplicabilă oricărui tip-rătăcit):
  latura de stoc = DSC (sau LDI+) cu loturile 1C ca pin; latura contabilă =
  NTC cu transcrierea exactă a rândurilor 1C (418=707, 4111=418, 4428…).
  Respectă „stocul nu se mișcă fără registru de stoc"; 418/4428 rămân fapte
  ale sursei transcrise, nu mecanisme Atlas (tipul Aviz propriu — amânat §10).

### 12.3 Maparea conturilor (datoria 47f): LISTĂ, nu regulă + pre-flight

Tăierea mecanică a cifrelor terminale e demonstrabil nesigură exact unde e
volumul: `43111` → ar cădea pe 4311 (există, ne-sumator — regula s-ar opri
mulțumită), dar semantic corect e **4316** (CASS) — familia contribuțiilor
salariale nu urmează structura sintetică OMFP. O regulă cu colapsuri semantice
tăcute e mai scumpă decât ~130 de rânduri de dicționar și ar face imposibilă
verificarea „zero colapsuri tăcute" din 47f. Rezolvarea descoperirii-în-mers:

- **Fază pre-flight în Import1C**: înainte de orice document se mătură TOATE
  codurile de cont folosite de mișcările 2025 (NoteContabile/DefalcareNote),
  se trec prin `MapeazaCont`, iar nerezolvabilele + căderile pe sumator se
  emit ca raport UNIC — denumirile din ambele planuri una lângă alta + coloană
  de sugestie mecanică (tăierea de cifre, copilul unic al sumatorului),
  sugestie de triaj, NICIODATĂ aplicată automat. Triajul = o sesiune de
  decizii, nu 130 de descoperiri în timpul rulării.
- Dicționarul crește în **CSV comentat** în tool (forma §8.1) — decizia 21
  aplicată literal.
- **Planul seed aliniat la validatorul D406** (verificat 25.07.2026 contra
  `anaf/plan_conturi_bal_soc_com.md`, extras din DUKIntegrator): CSV-ul
  `plan-conturi-omfp.csv` era TRUNCHIAT la coadă (921 tăiat în mijlocul
  denumirii, fără 922–925/93/931/933) — reparat; restul simbolurilor
  (inclusiv adăugirile 1496, 616–618, 4417, 697) erau deja prezente.

### 12.4 Ordinea și granularitatea rulării

- **Unitatea de idempotență = documentul; unitatea de contract = luna.**
  Legătura `1C:<view>` se scrie **în același commit cu draftul** (documentele
  n-au cod natural de recuperare, spre deosebire de nomenclatoare — 47b).
  Resume oriunde, inclusiv la mijloc de lună: legat + Operat → skip; legat +
  Draft → re-operare (crash-ul dintre creare și operare se vindecă singur).
- **Ordinea**: cronologic pe timestamp-ul 1C complet. Gardianul de sold e pe
  prefix-sum pe ZILE (25d) — ordinea intra-zi nu contează pentru solduri, doar
  pentru existența loturilor (FCT înaintea vânzării care îi pin-uiește lotul);
  excepțiile cad în supapa §12.1 cu raport.
- **Bucla lunii**: documente → imperecheri → ITV → reconciliere → verdict
  (vezi §11/1C-c — ITV intră în buclă).
- **Lună picată: stop dur implicit** (exit ≠ 0 cu raportul complet) + flag
  `--continua` pentru rulările de recoltat găuri; diferențele
  justificate/cunoscute (netarea §12.1, artefactele sursei) se **poartă
  înainte** dintr-o lună în alta de reconciliator — altfel o diferență din
  ianuarie face zgomot în toate lunile următoare.
- **Progres**: contoare per tip la fiecare 1.000 de documente + durata per
  lună; prima rulare reală = ianuarie singur (`--pana-la`), cu măsurătoare —
  abia cifra de acolo decide dacă se discută performanță (§3).

## 13. Felia 15 (D15-D6) — adresele din 1C + registrul ANAF, pe baza de import

Executat 2026-08-25 (V5 al contractului `docs/api/p5-felia-partener-anaf-contract.md`).

**Ce face unealta**: `AplicaAdresa` la materializare și în `--reclasifica`
(DOAR pe bloc gol; județ: cod CNP → denumire, cu grafiile 1C „Sector n" /
„Jud. X"; nerezolvat ⇒ denumirea brută în `DetaliiAdresa`; `bl./sc./et./ap.`
din `Present`; tăiere la `MaxLength` cu contor); `--anaf` (și
`--reclasifica --anaf`) = `Anaf1C.cs` peste `SincronizareAnafService`
(tranșe de 1.000, commit per partener, reluare o dată la eroare tranzitorie,
`suprascrie = false`, raport agregat per cauză + distribuția D394 înainte/după).
**Canonicul bate evidența**: un partener cu `DataSincronizareAnaf` nu mai e
atins pe axa TVA de `AplicaClasificare`, nici de semnalul din registru
(contoarele `PastratiDinAnaf` / `RegistruContraAnaf`).

**Contractul de coloane** (`--cititori`, 53 de cititoare): `AdreseEsantion` /
`AdresaPartener` / `AdreseParteneri` dau același rând pentru același partener.
Constatare pe sursă: `CodJudet` e `nvarchar(10)` (nu int) și e completat pe
57.336 din 148.443 de rânduri; `Field3` are 143.373; etajul/apartamentul
există DOAR în `Present`; `Field4` (localitate) depășește 35 de caractere pe
1.054 de rânduri (SAF-T, tăiere raportată).

**Clona `Atlas.Conta.Import1C.Flax.Api`** (`--reclasifica --anaf`, 4 min 05 s):
- adrese: **20.034 preluate din 20.114 cu bloc gol (99,6 %)**, 4 deja
  completate (V4 al pasului 3), 80 fără adresă în sursă; județ **4.011 din
  codul CNP, 16.015 din denumire, 7 nerezolvate**; 65 câmpuri tăiate.
- ANAF: 8.232 candidați din 20.118 legați (săriți: 8.103 fără cod fiscal,
  3.615 cod fără cifre — PF cu „-", 99 CNP, 60 țară ≠ RO, 9 în afara 2–10
  cifre); **8.230 găsiți, 2 negăsiți, 0 erori de lot**. Modificări 3.364:
  TvaLaIncasare 1.370, DetaliiAdresa 1.013, InregistratTva 444, CodPostal 367,
  Numar 77, InactivFiscal 52, Strada 32, RegistruComert 9. Diferențe
  raportate 25.646: Localitate 7.609, Strada 6.156, Denumire 5.173,
  RegistruComert 4.949, Numar 781, DetaliiAdresa 617, CodPostal 271, Judet 90.
  Avertismente 160: 140 trunchieri Localitate (legitime — ANAF întoarce
  `denumire_Localitate` = „Sat X Com. Y", 36–60 de caractere, peste cei 35 ai
  `City` din SAF-T; câmpul e mapat corect), 17 trunchieri DetaliiAdresa, 2
  negăsiți, 1 fără adresă utilizabilă.
- distribuția D394 a partenerilor: **tip 4 → tip 1: 190** (Juridica · RO ·
  neînregistrat 2.717 → 2.527; înregistrat 5.540 → 5.730); restul neschimbat.
- **D394 09/2025 prin WebApi (Admin), înainte → după**: 2.601 rânduri `op1`
  în ambele; nrCui 1052/1495/1/1 → 1084/1463/1/1. 62 de CUI-uri și-au
  schimbat cartușul, toate explicate de statutul ANAF de azi: **47 rânduri L
  tip 2 → tip 1** (PF cu CUI înregistrate — Σ bază 71.615,49 / TVA
  15.039,31), **13 L + 2 A tip 1 → tip 2** (parteneri marcați PF în 1C,
  neînregistrați azi — Σ 81.360,45 / 17.085,67 și 1.009,05 / 211,91; cele 2
  achiziții ies ca `CombinatieRefuzata` = exact D4-r1, statutul la data
  documentului ≠ statutul de azi), **1 A → AI** (CUI 5310754, TVA la încasare
  din ANAF, 760,33). Σ pe cartușe: tip 1 L 5.126.130,13 → 5.116.385,17, A
  4.045.607,36 → 4.043.837,98, AI 4.248,59 → 5.008,92, C neschimbat
  3.931.447,52; tip 2 L 2.136.904,94 → 2.146.649,90 (+ A 1.009,05); tip 3/4
  neschimbate. Σ totală pe formular identică (nimic pierdut).

**Baza `Atlas.Conta.Import1C.Flax`** (refăcută cu `--recreeaza --cititori`,
binarul feliei, 20:35 → 00:13, CONTRACT ÎNDEPLINIT, 0 FAIL; apoi
`--reclasifica --anaf`, 6 min 42 s):
- adrese la MATERIALIZARE: **20.038 preluate din 20.118 legați (99,6 %)**, 80
  fără adresă în sursă; județ 4.013 din codul CNP, 16.017 din denumire, 7
  nerezolvate; 65 câmpuri tăiate. `--reclasifica` ulterior: 0 preluate,
  20.038 deja completate (neatinse) — idempotența e chiar cifra asta.
- ANAF: 8.232 candidați, **8.230 găsiți, 2 negăsiți, 0 erori**; modificări
  3.365 (TvaLaIncasare 1.371, InregistratTva 444, InactivFiscal 52, restul
  adresă/RegCom), diferențe 25.655, avertismente 160 (aceleași cauze ca pe
  clonă); distribuția D394: **tip 4 → tip 1: 190**.
- **Reconcilierea**: `reconciliere-20260825-203703.txt` vs baseline-ul
  `reconciliere-20260825-095411.txt` (F14/D5 = DIM-4): 444 de linii, 39.321 B
  fiecare; diff pe conținut sortat (fără linia 1, timbrul de timp) = **0
  linii**. Singura permutare în ordine: două categorii cu același număr de
  chei (3) în „justificate, agregat pe categorii" — egalitate la sortarea pe
  contor, fără cheie secundară (raportul nu e determinist pe egalități;
  restanță de cosmetică, nu de cifre). `--anaf` nu atinge registrele
  (scrie doar `Parteneri`), deci raportul nu se regenerează și nu se poate
  mișca.

**Rămase / constatări**: `RegistruContraAnaf` a ieșit 0 pe ambele baze doar
fiindcă în `--reclasifica --anaf` semnalul din registru rulează ÎNAINTEA
timbrului — la a doua rulare `--reclasifica` cifra devine reală (D4-r1);
sortarea pe egalitate în raportul de reconciliere; 7 județe nerezolvate
(denumire liberă) rămân în `DetaliiAdresa`.

## 14. Felia 16 (D16-D6) — societatea, UM/cod NC și fișierul SAF-T, pe baza de import

Executat 2026-08-26 (V5 al contractului `docs/api/p5-felia-saft-contract.md`).
Baza: `Atlas.Conta.Import1C.Flax` — cea de RECONCILIERE, nu clona `.Api` (aceea
e a lui V4). **Niciun document nu s-a re-importat**: toate cele trei flag-uri
noi sunt moduri de NOMENCLATOR, iar `--saft` nici măcar nu deschide sursa 1C.

### Ce face unealta

- **`--societate`** (`Societate1C.cs`): `flax.Organizatii` (denumire, CUI,
  RegCom) + adresa din `InfoRg_InformatiaDeContact` pe `Object_Organizatii_ID`
  (aceeași fereastră ca la partener, cu tipurile „… societate": sediul social
  bate punctul de lucru) + telefonul/e-mailul din același registru + conducătorul
  din `InfoRg_PersoaneResponsabileDinOrganizatia` + contul bancar. Scrie DOAR pe
  câmp gol (72d), adresa ca BLOC; diferențele se raportează cu ambele valori.
  Sursa n-are coloană de TVA (verificat pe `sys.columns`: `Code`, `Description`,
  `DenumireaCompleta`, `CodUnic`, `RegCom`, `CodCAEN` + FK-uri) ⇒ `InregistratTva`
  se DERIVĂ din prefixul `RO` al CUI-ului și se raportează ca derivat. Mai multe
  organizații în sursă ⇒ REFUZ cu lista lor (bug-ul §D.6 al exportului vechi:
  `select … from Organizatii` fără `where`, semnat cu una la întâmplare).
- **`--um-nc`**: `Produs.UnitateMasuraId` prin `UnitatiMasuraRo.Rezolva(UM)` și
  `Produs.CodNc` din `Nomenclator.NIC` (doar dacă are EXACT 8 cifre), pe TOATE
  produsele legate, doar pe câmp gol. Sursa se citește în LOT
  (`NomenclatoareDupaIds`, tranșe de 1.000), commit per 500 de produse — pereche
  exactă cu `ReclasificaToti` și pentru același motiv: produsele aduse înaintea
  feliei 16 sunt deja legate, deci materializarea nu le mai atinge niciodată.
  Rulează și ca pas final al importului normal, lângă reclasificare.
- **`--saft <an> <lună>`** (`Saft1C.cs`): `SaftProiectii.Saft` pe ușa
  non-secured a uneltei → `SaftXml.Scrie` în `saft-<an>-<lună>.xml` → oracolul
  `Duk.Valideaza`, cu raport în `saft-<an>-<lună>-raport.txt` (secțiuni,
  cusăturile cu stare, `Neincluse` grupate per cauză, avertismente per cod,
  verdictul DUK cu erorile grupate pe tipul mesajului, timpi, dimensiune).
  Oracolul e ACELAȘI fișier ca în suită (`<Compile Include="../ModelCheck/Duk.cs">`),
  nu o copie. `Neaplicabil` (bugetar) ⇒ mesaj și cod de ieșire ≠ 0; fișier
  respins de DUK sau oracol lipsă ⇒ tot ≠ 0 (SĂRIT ≠ trecut).
  Artefactele sunt gitignored, ca jurnalele de reconciliere: 70 MiB cu datele
  fiscale reale ale unui client n-au ce căuta în repo.
- **`--saft-s <an> <lună>`** (felia 17, același `Saft1C.cs`): ACEEAȘI comandă pe
  modulul **S** (stocuri, „la cerere") — `SaftProiectii.SaftStocuri` →
  `saft-s-<an>-<lună>.xml` + `-raport.txt` cu contoarele S, cusăturile **S1–S4**
  (S3 per cont, raportată, nu blocantă), `Excluse` deliberat (politica cu cod
  null) separate de `Neincluse`, avertismente, verdictul DUK. Un PARAMETRU
  (`SaftFel`), nu o a doua unealtă: fișierul, oracolul, gruparea erorilor și
  codul de ieșire sunt identice. O lună fără nicio intrare de stoc fizic ⇒
  fișierul NU se scrie (`PhysicalStock` e obligatoriu prezent pe „C") și codul
  de ieșire e ≠ 0.
- **Contorul prefixului RO dublat**: `AplicaClasificare` numără `CodFiscal`-urile
  care încep cu `RORO` (insensibil la caz) și dă exemple. NU rescrie nimic —
  normalizarea e în proiecție (`D394Proiectii.NormalizeazaCui`, o singură sursă).
- **Contractul de coloane** (`--cititori`, acum **55 de cititoare**, 0 eșecuri,
  48.111 rânduri pe 01/2025): `Organizatii` + `Nomenclator` (cu `NIC`), cu
  proba că nomenclatorul e identic pe cele două căi (per id / în lot) și cu
  eșantionul antetului tipărit (adresa societății iese tot din coloane fizice
  numerotate — singura probă onestă e ca omul să vadă că în „județ" scrie un
  județ).

### Seed-ul F16 pe Flax

`Blazor.Server --updateDatabase --forceUpdate --silent` cu
`ConnectionStrings__ConnectionString` pe Flax: **2.163 unități de măsură**, 42
județe, **1 rând `Societate`** (gol), **16 conturi cu `RolTert`**, **644 cu
`Functie`**, iar `LeagaUnitatileProduselor` a legat **22.721 din 22.726 de
produse** (FK-ul de UM era 0 înainte). Documentele și cele trei registre au
rămas **identice la rând**: 205.131 documente, 305.005 rânduri contabile,
282.388 de stoc, 90.732 de TVA, 22.726 produse, 20.119 parteneri, 46.602 loturi
— aceleași cifre înainte și după seed și după toate rulările de mai jos.

### `--societate`

Zece câmpuri umplute dintr-o singură organizație (`00001 FLAX COMPUTERS S.R.L.`):
`Denumire`, `CodFiscal` = `14639030` (prefixul `RO` tăiat de `NormalizeazaCui`),
`InregistratTva = true` **derivat** din prefix, `RegistruComert` = `J15/206/2002`,
`ContactNume`/`ContactPrenume` = `FLORESCU` / `ADRIAN` (conducătorul, rupt la
primul spațiu — convenție, raportată ca atare), `Telefon` = `0372030474`,
`Email` = `conta@flax.ro`, adresa-bloc `CALEA DOMNEASCA 345, TARGOVISTE` cu
județul din DENUMIRE (`DAMBOVITA` ⇒ `RO-DB`), `ContBancar` = `SED000319`
`RO56INGB0000999904841949` (**contul implicit declarat în 1C**, nu „primul în
lei"). 0 diferențe, 0 trunchieri. **A doua rulare: 0 umplute, 8 deja completate,
adresa neatinsă** — idempotența e chiar cifra asta.

### `--um-nc`

22.726 de produse atinse. **UM**: 0 rezolvate în rularea asta / **22.721 deja
legate** (le legase seed-ul, cu ACELAȘI dicționar) / **5 nerezolvate pe 2
grafii — `ml` × 4, `pac` × 1**. Distribuția completă a nomenclatorului:
`buc.` 22.710 ⇒ `H87`, `kg` 4 ⇒ `KGM`, `m` 4 ⇒ `MTR`, `to` 2 ⇒ `TNE`,
`MC` 1 ⇒ `MTQ`; `ml` rămâne deliberat ambiguu (metru liniar SAU mililitru — un
cod ghicit ar trece validatorul și ar fi fals), `pac` e necunoscut.
**Cod NC**: **18.642 preluate** (exact 8 cifre), 4.083 absente în sursă
(`NIC` gol), **1 invalid** — `8536419040`, un cod TARIC de 10 cifre. A doua
rulare: 0 preluate, 18.642 deja completate — idempotent.

### `--saft 2025 9` — **DUK: `ok` (J2.2.8), 0 atenționări**

Fișierul REAL al lunii trece validatorul oficial. E prima oară: la V4, pe clonă,
era respins dintr-o singură cauză (doi parteneri cu prefixul `RO` dublat), pe
care pasul 4b a închis-o în `NormalizeazaCui`.

| | 09/2025 | 12/2025 |
|---|---|---|
| GeneralLedgerAccounts | 99 | 101 |
| Customers / Suppliers | 3.950 / 228 | 4.589 / 263 |
| TaxTable / UOMTable / AnalysisTypeTable | 5 / 1 / 0 | 5 / 1 / 0 |
| Products | 2.325 | 2.136 |
| Journals / Transactions / linii GL | 12 / 13.966 / 52.932 | 11 / 13.664 / 54.092 |
| SalesInvoices (net / brut) | 3.700 — 9.459.762,04 / 10.987.556,17 | 3.655 — 8.810.123,19 / 10.597.933,41 |
| PurchaseInvoices (net / brut) | 1.688 — 4.825.033,98 / 5.795.666,56 | 1.531 — 5.138.063,42 / 6.210.111,52 |
| Payments (Σ) | 3.000 — 17.401.707,14 | 2.836 — 22.128.067,98 |
| `Neincluse` | 100 | 131 |
| **DUK** | **ok (J2.2.8), 0 atenționări** | **ok (J2.2.8), 0 atenționări** |
| proiecție / XML / validator | 4,5 s / 0,4 s / 3,5 s | 4,3 s / 0,4 s / 3,5 s |
| dimensiune | 70,9 MiB (74.375.406 B) | 71,6 MiB (75.045.419 B) |

**Cusăturile, toate șase OK la cent, pe ambele luni.** 09/2025: partidă dublă
80.527.820,95 pe toate trei laturile; TVA 3.238.255,54 (GL) + 0 (capitalizat)
− 32.788,80 (tipuri fără cod SAF-T) = 3.205.466,74 = registrul; bază achiziție
4.614.447,99 + 3.366.855,48 (neincluse) = 7.981.303,47 = registrul; bază livrare
7.266.639,45 + 0 = registrul; `Closing` GLA = balanța; 0 facturi cu partener
nedeclarat. 12/2025 (luna cu închideri): 83.830.122,19; TVA 3.455.615,92 −
3.019,03 = 3.452.596,89; achiziție 5.103.669,14 + 2.822.562,59 = 7.926.231,73;
livrare 8.513.061,91 + 0. Cifrele lui 09/2025 sunt **identice cu V4** pe HTTP
(13.966 / 52.932 / 3.950 / 228 / 3.700 + 1.688 / 3.000 / 100 / 80.527.820,95).

**`Neincluse` 09/2025, per cauză**: 84 × `ContFaraRol [PurchaseInvoices]`
(bază 3.366.855,48 / TVA 707.040,03 — facturi de achiziție pe conturi fără rol
de terț în plan), 5 × `RepartitorNePartener [Suppliers]` și 3 × idem
`[Customers]` (dimensiunea `Repartitor` e „Sediul central", o `UnitateInterna`,
nu un partener), 4 + 4 × `FaraPartener` (rânduri de terți fără nicio latură cu
partener). Cauza dominantă e o gaură de DATE a planului importat, nu a
proiecției: ea se închide completând `Cont.RolTert`, iar cusătura 3a o
contabilizează la cent, deci nu se pierde nimic.

**Avertismentele 09/2025** (7 coduri): 1.641 `PartenerFaraCuiValid` (CUI care nu
trece cifra de control — „CONSUMATOR FINAL" și persoane fizice fără cod; ies cu
prefixul `04`), 473 `TertFaraPartener` (Σ 1.676.955,21), **167 `FaraCodNc`**
(din 2.325 de produse declarate; înainte de `--um-nc` coloana `CodNc` era goală
pe TOATĂ baza, deci avertismentul ar fi ieșit pe toate 2.325 — cifra lui V4 —
iar acum **2.158 de produse și-au primit codul**), 84 `ContFaraRolPeFactura`, 49 `AdresaIncompleta`,
16 `PartenerDublat` (parteneri distincți cu același CUI — „Directia Silvica
Brasov"/„Arges"/„Dambovita" pe `001590120`; se declară o intrare cu solduri
cumulate), 2 `TipTvaFaraCodSaft` (Σ 0,12 — `N19`/`TI19`, cotele istorice de
dinainte de 01.08.2025). **Zero `FaraUnitateMasura`**: toate produsele declarate
au FK de UM.

**`UOMTable` = 1** pe ambele luni, și e corect, nu o regresie: din cele 22.726 de
produse ale nomenclatorului, **16** au altă unitate decât `buc.` (4 `kg`, 4 `m`,
2 `to`, 1 `MC`, 4 `ml`, 1 `pac` — cabluri, șuruburi la kilogram, cherestea,
sorturi de balast), iar niciunul n-a apărut pe facturile lunilor măsurate. Când
apare, tabelul crește singur (secțiunea e derivată din utilizare).

### Reconcilierea, neatinsă

`reconciliere-*.txt`: **18 fișiere, aceleași, nemodificate** —
`reconciliere-20260825-203703.txt` are același SHA-256
(`28F0BD09…A5D62FB1`) și același timestamp (2026-08-26 00:12:46) ca înaintea
feliei. Niciun flag al feliei 16 nu deschide `JurnalContract` și niciunul nu
atinge registrele.

### Rămase / constatări

- `ml` și `pac` cer decizia unui om în nomenclator (puntea nu ghicește);
  `8536419040` e TARIC de 10 cifre, nu NC8 — tot corectură de nomenclator.
- `ContFaraRol` pe 84–111 facturi de achiziție pe lună = conturi fără
  `Cont.RolTert` în planul importat; se închide cu date, nu cu cod.
- Contorul `CuiPrefixDublat` se poate măsura abia la o rulare care reclasifică
  (`--reclasifica`), nu în modurile feliei 16 — nu s-a rulat pe Flax, ca să nu
  se atingă partenerii deja timbrați ANAF.
- Numele conducătorului rupt la primul spațiu (`FLORESCU ADRIAN`) e o convenție
  a conectorului, nu un fapt al sursei: 1C ține numele într-un singur câmp.
