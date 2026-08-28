# Pasul 5, felia 18 — restanțele grele ale lui S: 74-r4 (perf O(istoric)), 74-r6 (deriva per lot), 74-r9 (reclasificarea la transfer) — CONTRACT

Data: 2026-08-27. Stare: în lucru. Decizia rezultată: 75 (la închidere).
Precedente: `p5-felia-saft-s-contract.md` (F17, decizia 74), `p5-perf-masuratori.md`
(59), `docs/import/faza-1c-design.md` (contractul de reconciliere, 45e/50b).

## Ce a arătat explorarea (fapte, nu design)

**74-r4.** `SaftProiectii.AgregatStoc` (`Module/Saft/SaftProiectii.cs:2556`) e
SINGURUL consumator care scanează `RegistruStoc` ÎNTREG, și o face de TREI ori
per apel: deschidere (`Data <= dataStart−1`), închidere (`Data <= dataEnd`) și
`SoldPeTipStocNeraportat` (`Data <= dataEnd`, tipurile fără cod) — toate
traduse în SQL (GroupBy + Sum), fără niciun index pe `Data` (doar cele 4 FK
din `InitialCreate`). Precedentul „un singur pass cu sume condiționate"
există deja: `SaftProiectii.Saft` (L, terți, :348-357) și
`ContabilProiectii.Balanta` (:302-339, `SUM(CASE WHEN Data < dataStart)`).
Nu există niciun snapshot de solduri; celelalte consumatoare de sold
(`StocService.Sold`, `VerificaSoldIntermediar`, `AlocaFifoTolerant`,
`StocProiectii.SoldStoc`) sunt mărginite la o cheie/produs sau fac o singură
trecere. Cifrele azi (Flax, 282 k rânduri): proiecție 3,4–5,6 s/lună.

**74-r6.** Deriva „0 bucăți, X lei" NU e a importului, e a MOTORULUI: toate
ieșirile fac `Valoare = RotunjesteBani(Cantitate × Lot.PretUnitar)`
(`DocumenteGestiune.cs:178-183, 213-217`; importul la fel, HandlereStoc/
HandlerAsamblare/HandlereRetur/Vanzare1C), iar prețul lotului e rotunjit
(motor 18,6; `Deschidere.cs:450` la 4 zecimale). Un lot de 3 buc. × 10,005
(30,02) descărcat bucată cu bucată = 3 × 10,01 = 30,03 ⇒ −0,01 pe lot cu
cantitate 0. Niciun mecanism „ultima ieșire preia restul" nu există în
motor. Pe Flax: `ReziduValoricFaraCantitate` 861/1.168 loturi (Σ ≈ 6,35 k),
`SoldNegativ` 555/735. 1C poartă aceleași reziduuri (371 reconciliază la
cent azi) ⇒ orice corecție mută 371/381/3028 ↔ 607/608/… față de 1C cu
exact Σ reziduurilor absorbite.

**74-r9.** Cele două NTC pe 381 (80,00 la 2025-03-10, 4.720,00 la 2025-08-19
= 4.800) sunt punți `"BTR: reclasificare de cont pe transfer"`
(`HandlereStoc.cs` ~281-294, `Punte.cs:187-238`): 1C mută marfa 381→371 la
transfer fără să-i schimbe identitatea de lot; Atlas nu poate (contul
lotului = `TipMaterial.ContImplicit`, identitatea produsului de import =
nomenclator × cont, 50a), deci puntea transcrie CONTABIL o mișcare pe care
stocul n-a făcut-o; DSC-ul descarcă apoi lotul rămas pe 381 ⇒ 381 creditat de
două ori (loturile coincid, verificat în `RegistruStoc`). Generalizare:
**toate cele 556 reclasificări/an** sunt cauza lui S3 pe 371 (185.267,81 în
09, 239.136,20 în 12) — balanța urmează 1C, registrul de stoc nu.

## Deciziile de fixat

### D18-D1 — `AgregatStoc` = O SINGURĂ trecere, index doar dacă cifra o cere

- Un singur query pe `RegistruStoc` cu `Data <= dataEnd`, grupat pe
  `(RepartitorId, LotId, TipStoc)`, cu `Initial = Σ(Data < dataStart)`,
  `Rulaj = Σ(Data >= dataStart)` pe cantitate și valoare; deschiderea/
  închiderea/`SoldPeTipStocNeraportat` se derivă ÎN MEMORIE din acest
  rezultat (≤ ~17 k chei × tipuri). Rândurile lunii (`MovementOfGoods`)
  rămân query-ul lor.
- Nicio schimbare de cifre: secțiunile S ale fișierelor Flax 09/12 2025 sunt
  IDENTICE cu cele de la 74 (probă: diff pe XML fără `HeaderComment`/
  timestamp, sau SHA pe secțiuni).
- Măsurare (metodologia 59: 6 rulări, mediana celor 5 calde) ÎNAINTE și DUPĂ,
  pe Flax, prin `Import1C --saft-s` (timpul proiecției e în raport) și prin
  `EXPLAIN (ANALYZE)` pe query-ul EF (docker psql). Index: DOAR dacă
  `EXPLAIN` arată seq-scan dominant ȘI cifra rămâne > ~1 s/lună; candidatul
  `(Data)` sau `(TipStoc, Data)` — cu cifra înainte/după în
  `p5-perf-masuratori.md` (addendum S). Snapshot lunar de solduri = NU în
  această felie (se deschide ca restanță doar dacă ținta nu se atinge).
- Ținta: proiecția S < 1 s/lună pe Flax (cald). Dacă nu se atinge fără
  index ⇒ index; dacă nu se atinge nici cu index ⇒ se raportează, nu se
  forțează.
- **Amendament măsurat (pasul 1, 2026-08-27)**: atribuirea din 74-r4 era
  GREȘITĂ — cele trei scanări costau 128 + 133 + 9 ms (index scan pe
  `IX_LotId`, nu seq-scan) din ~4 s; trecerea unică (155 ms) lasă cifra
  neschimbată (3,9→4,0 s / 4,1→4,1 s) și e păstrată pentru curățenie, fără
  index (`Data <= end` selectează 75 % din tabel). Costul real: §7
  `CoduriTipPeTipuri` → `IdsDocumenteDeTip<T>` listează TOATE documentele
  bazei pentru fiecare din ~19 tipuri TPT și filtrează în memorie (~1,4 s,
  O(documente)); §14 `ComponenteS3` (~0,9 s, join pe 4 niveluri grupat pe
  `(Simbol, DocumentId)`); restul ~1,2 s în felii de 100–300 ms. D1 se
  extinde: (a) filtrul pe id-uri (sau pe perioadă, dacă e echivalent —
  verificat în cod) în SQL la `IdsDocumenteDeTip`, aceleași cifre; (b)
  `ComponenteS3` pe agregatul unic, doar dacă e mărginit, identic la cifre
  și ≥ 0,4 s câștig — altfel restanță cu cifra. `ApiProiectii.CoduriTip`
  (același tipar pe API) se RAPORTEAZĂ, nu se atinge în felia asta.

### D18-D2 — Motor: ieșirea care GOLEȘTE cheia preia valoarea rămasă

- Regulă generică în motor (amendează 13/27d/62 „lot străin = prețul
  lotului"): pentru o linie de IEȘIRE pe cheia `(Lot × Repartitor ×
  TipStoc)` (semnul din regula de stoc = −1), dacă soldul cantitativ al
  cheii DUPĂ linie, la data documentului, e 0, atunci `Valoare` = soldul
  VALORIC al cheii înainte de linie (nu `preț × cantitate`). Altfel
  `RotunjesteBani(cantitate × preț)` ca azi.
- Un singur helper, în `StocService` (`ValoareIesire(os, cheie, data,
  cantitate, pretUnitar, excludeDocumentId, acumulatDinAcelasiDocument)`),
  apelat din `PregatesteOperare` al tuturor frunzelor cu ieșiri de stoc:
  BCS, DSC, LDI (Minus), RLF, BTR (latura −; valoarea liniei e comună
  ambelor laturi ⇒ restul se MUTĂ pe destinație), ASM (consumuri). Intrările
  (NIR, LDI Plus, RDC, ASM produse) NEATINSE. Liniile aceluiași document pe
  aceeași cheie se acumulează în ordinea liniilor (a doua linie vede soldul
  după prima). Soldul „la dată" = prefix-sum ≤ data documentului, aceeași
  convenție ca gardianul 25d; rândurile deja scrise ale ACELUIAȘI document
  (re-operare) nu se numără de două ori.
- **Amendament (review advers F1/F10, 2026-08-28)**: golirea e decisă **LA
  MOMENTUL OPERĂRII, pe rândurile care EXISTĂ atunci în registru** cu `Data ≤`
  data documentului — NU „convenția gardianului 25d" (gardianul e pe ziua
  întreagă, valoarea e pe registrul văzut). Invariantul „cheie golită ⇒
  0/0,00" NU ține la operare retro / re-ordonare intra-zi: o linie decisă
  „nu golește" nu e re-decisă de un document retro operat ulterior (probat:
  lot 2 × 20,01, BCS 10.05 −1 ⇒ 10,01 și rest 1/10,00, BCS retro 05.05 −1
  operat după ⇒ prefix-ul ≤ 05.05 nu vede rândul din 10.05 ⇒ 10,01 ⇒ cheia
  0/−0,01). Reziduul retro se DECLARĂ (S: `ReziduValoricFaraCantitate`; D4:
  „golită la operare, re-deschisă retro", avertisment cu măsurarea rândului
  retro), nu se corectează tăcut. Helper-ul e în MOTOR (42a), nu în
  `PregatesteOperare` al frunzelor. ModelCheck D18-V2 (r) ține proba ca FAPT
  documentat, nu ca invariant.
- **Amendament (review F5)**: RLF NU absoarbe restul — suma returului e a
  facturii/notei de credit a furnizorului (`q × preț`), nu a lotului. Contract
  pe tipul de document, fără migrație: `IDocumentCuIesireFiscala` (marker în
  `Comun/Interfete.cs`, declarat de `ReturFurnizor`); `AplicaValoareIesire`
  sare documentele care îl declară; `Alocare.Aloca(absoarbeLaGolire: false)`
  face același lucru în Import1C. Reziduul rămâne pe lot (S), contorizat de
  D4 ca „golită fiscal".
- Storno: rândurile inverse copiază valorile (neschimbat). Anularea:
  neschimbată. Dry-run (`Valideaza`) = același calcul (calculează →
  validează → materializează, 33d).
- Cine mai calculează `Valoare` la culegere (helper-ul 53c pentru UI,
  `DescarcareService` la generare 37b, `LoturiCulegereService`) rămâne pe
  `preț × cantitate`: e o PREVIZUALIZARE; adevărul se materializează la
  operare (36a analog). Afișarea nu minte: ReadDto al liniei arată valoarea
  materializată.
- Consecință contabilă: la golirea unui lot, contul de stoc ajunge EXACT la 0
  pe acel lot; diferența (cenți) merge în contul de cheltuială/venit al
  ieșirii. Aceasta e regula OMFP 1802 (identificare specifică: costul
  lotului consumat = valoarea lui integrală).

### D18-D3 — Import1C: reclasificarea de cont la transfer = MIȘCARE de stoc

- Rândul 1C `TransferDeMarfuri` cu `simbolDebit ≠ simbolCredit` NU mai
  produce (BTR + punte) ci: (1) **ASM** (dezasamblare/asamblare, tipul
  existent 46d: n→m, |Σ| ≤ 0,005, ZERO contare) în gestiunea SURSEI, care
  consumă lotul de pe `produs@contVechi` și naște lotul pe `produs@contNou`
  cu aceeași cantitate și valoare (valoarea consumului = D18-D2, deci lotul
  vechi ajunge exact la 0 dacă e golit); (2) **NTC-punte** `contNou =
  contVechi` la valoarea ASM-ului (rămâne, categoria de azi, dar acum are
  MIȘCAREA în spate); (3) **BTR** al lotului NOU spre gestiunea destinație
  dacă gestiunile diferă (dacă rândul 1C e doar reclasificare în aceeași
  gestiune, fără BTR). Cheile de idempotență: `h.Id + "#reclas"` pentru ASM,
  `#punte` rămâne, BTR pe cheia plată de azi.
- Fapte (explorare 2): azi reclasificarea scrie un ALIAS
  (`MigrareLegaturi.Tabela = '1C:LotAlias'`, cheia cu simbolul NOU →
  ACELAȘI lot, `Catalog.cs:57-71`), iar `MiscareStoc1C.Rezolva`
  (`HandlereStoc.cs:102-118`) ia contul din `lot.Simbol` CANONIC — de aceea
  DSC descarcă 381 pentru un rând 1C pe 371 (`Vanzare1C.cs:210-216` o spune
  explicit). Pe Flax: 225 punți din `TransferDeMarfuri` (3028→371 205,
  303→371 12, 371→3028 3, 371→381 2, 381→371 1, 371↔3021 2), 650 aliasuri
  pe 643 loturi (7 reclasificate de două ori), 30 din ele cu RLF ulterior.
  `HandlerAsamblare.cs:217-226` are un GARD care refuză „asamblarea care
  reclasifică" — se înlocuiește deliberat cu calea de mai jos; tiparul
  `h.Id + "#btr"` pentru transferul produselor în altă gestiune există
  deja acolo (:127-134) și se refolosește.
- Legătura: cheia `1C:Lot` cu simbolul NOU (azi alias spre lotul vechi)
  devine LOTUL NOU (canonic, `Simbol = contNou`); cheia cu simbolul vechi
  rămâne pe lotul vechi (restul, la reclasificare parțială). Rezoluția
  ulterioară e cea de azi (cheia exactă după contul rândului 1C); prefixul
  `document × nomenclator` devine ambiguu pentru aceste perechi și cade în
  supapa FIFO (48a) — se numără (`LoturiRezolvatePePrefix` scade,
  `LoturiNerezolvate` se raportează cu cifra). `1C:LotAlias` moare pentru
  reclasificările de transfer (rămâne doar dacă alt flux îl scrie — de
  verificat, nu presupus). `Produs@contNou` prin `AsiguraProdus` (50a).
- Reclasificarea de mai multe ori a aceluiași lot 1C = lanț (lotul nou
  devine sursa următoarei); RLF pe contul vechi după reclasificare totală =
  sold 0 pe lotul vechi ⇒ backorder, raportat (nu se inventează).
- Ținta măsurabilă: S3 pe 371/381 în `--saft-s 2025 9/12` = DOAR deschidere
  (4.852,19) + FCT (−20,00) — componenta NTC ⇒ 0 (sau explicată la cent de
  punți care NU sunt reclasificări de transfer, listate). 381 nu mai e
  creditor.
- Reconcilierea (45e/50b): contractul 1 (sold per cont) NEATINS de D3 (puntea
  ține conturile ca în 1C; Δ pe 371 e deja explicat de registrul
  divergențelor); contractul 3 e pe `(nomenclator 1C × gestiune)` —
  `NomenclatorDinCheie` taie simbolul (`ReconciliereLuna.cs:558-588`) ⇒
  gemenii se însumează și D3 NU îl atinge (0 chei nejustificate rămân 0).
  Efectul măsurabil al D3 e în S3 (registrul de stoc vs balanță per cont).

### D18-D4 — Reconcilierea primește categoria „reziduu per lot absorbit"

- D2 mută cenții de pe conturile de stoc pe cheltuială/venit față de 1C.
  Contractul 1 primește o categorie JUSTIFICATĂ, cu cifră EXACTĂ calculată
  din registru, nu toleranță (50b): per lună și per cont de stoc, Σ
  `(Valoare − RotunjesteBani(Cantitate × PretUnitar))` pe liniile care au
  golit o cheie; contrapartida pe contul de cheltuială/venit al aceleiași
  linii. Diferența sursei rămasă după categorie trebuie să fie 0,00 (sau
  cea de dinainte, pe alte categorii).
- Raportul de reconciliere NU mai e byte-identic cu baseline-ul
  `reconciliere-20260825-203703.txt`; proba devine: **diff explicat integral
  pe categorii** (D4 + efectele D3), cu fiecare linie schimbată atribuită;
  liniile neatinse de D2/D3 rămân identice.

## Ce NU intră, cu motiv

- Snapshot/tabel de solduri lunare (74-r4 varianta grea) — doar dacă D1 nu
  atinge ținta; 59 interzice optimizarea orb.
- CMP / `PoliticaEvaluare` (51d) — neatinsă.
- `Produs.ContImplicit` mutabil / reclasificarea ca tip de document nou —
  respinsă: 50a fixează identitatea, ASM există exact pentru re-identificarea
  stocului fără contare.
- 74-r5 (`Consum` neraportat), 74-r7, 74-r8, 74-r10…r15 — neatinse.

## Riscurile pin-uite (ținta review-ului advers)

1. D1: sumele condiționate pe `decimal` în Postgres — identitate la miime cu
   cele două query-uri (probă: secțiuni identice pe Flax).
2. D2: cheia golită la data D dar cu rânduri ULTERIOARE (document retro):
   soldul „la dată" e prefix-sum ≤ D; rândurile după D nu se văd — rezultat
   corect pentru registrul la D; gardianul 25d păzește negativul.
   **Amendat (review F1)**: cazul invers — documentul RETRO (Data < D, operat
   DUPĂ linia din D) — lasă reziduu pe cheie, fiindcă nici linia din D (nu
   vedea retro-ul), nici retro-ul (nu vede rândul din D) nu golesc; regula nu
   re-decide linii deja operate. Reziduul e vizibil în S și clasificat de
   oracolul D4 „re-deschisă retro" (măsurat: există un rând cu `Data ≤` și
   `DataOperare >`), nu ascuns.
3. D2: două linii ale aceluiași document pe aceeași cheie; DSC spart pe
   loturi de generator (37b) apoi recalculat la operare; BTR care golește
   sursa — restul ajunge pe destinație (probă S1 pe ambele gestiuni).
4. D2: re-operarea (Draft → Operat după anulare) nu dublează soldul.
5. D2: importul ține `valoareAtlas` proprie pentru punți/divergențe
   (HandlereStoc etc.) — trebuie citită DIN registru după operare (sau prin
   același helper), altfel puntea/divergența e cu cenții vechi.
6. D3: reclasificare parțială / lot 1C reclasificat de mai multe ori /
   reclasificare urmată de retur la furnizor pe lotul vechi (RLF pe lotul
   ORIGINAL, 46e) — care lot e „original" după re-țintire.
7. D3: ASM cere `|Σproduse − Σconsumuri| ≤ 0,005` — cu D2 consumul preia
   restul, produsul primește aceeași valoare ⇒ exact 0.
8. D3: numărul de documente crește cu până la 556 ASM + BTR-uri — timpul
   importului, `MovementReference` (S4 unicitate), codurile S ale ASM (20/70
   — reclasificarea apare în S ca asamblare; se raportează dacă DUK sau
   sensul o contrazic).
9. D4: categoria cu cifră exactă — dacă reziduul nu se închide la 0,00 pe o
   lună, e semnal de altă cauză, nu se lărgește pragul.
10. Perf: D2 adaugă un `Sold` per linie de ieșire la operare — pe import
    (24 k linii/lună) trebuie măsurat (timpul total al rulării vs 1h40).

## Verificări (ModelCheck, ambele profiluri; Flax la final)

- **D18-V1** (D1): pe scena D17-V2 secțiunile S identice înainte/după; timpul
  proiecției pe Flax înainte/după (6 rulări), `EXPLAIN` atașat în
  `p5-perf-masuratori.md`.
- **D18-V2** (D2, ambele profiluri): lot 3 × 10,005 descărcat 1+1+1 ⇒ ultima
  ieșire 10,00 și lot exact 0/0,00; două linii pe aceeași cheie într-un
  document; BTR care golește sursa ⇒ destinația poartă restul; DSC pe FCL
  care golește; LDI Minus; RLF; ASM consum; ieșire care NU golește ⇒
  neschimbat; storno al liniei absorbante ⇒ rânduri inverse identice;
  anulare + re-operare; dry-run == operare; valoarea materializată în
  ReadDto ≠ previzualizarea (documentat). `ReziduValoricFaraCantitate` = 0
  pe scena D17-V2 după D2 (avertismentul rămâne în cod pentru date vechi).
- **D18-V3** (D3, scenă Import1C sintetică dacă există harness; altfel pe
  Flax): reclasificare totală, parțială, dublă, urmată de vânzare și de
  retur; idempotență la re-rulare (`--continua`).
- **D18-V4** (Flax, o singură re-rulare integrală, detașată + monitor):
  raportul de reconciliere diff-uit față de baseline pe categorii (D4);
  `--saft-s 2025 9` și `2025 12` DUK `ok`, S1–S5, S3 pe 371/381 fără
  componenta NTC de reclasificare, 381 necreditor, `ReziduValoricFaraCantitate`
  și `SoldNegativ` ⇒ 0 (sau explicate: loturi ale DESCHIDERII care nu s-au
  golit în an), timpul proiecției, timpul importului.
- ModelCheck verde pe ambele profiluri; `verifica:drift` idempotent;
  `--dump-metadata` dacă apare vreun caption.

## Regula de oprire

Agentul se oprește și raportează (nu normalizează tăcut) dacă:
- D1 schimbă o cifră din secțiunile S pe scenă sau pe Flax;
- D2 cere un câmp nou pe registru/lot/frunză, sau atinge intrările;
- ASM refuză reclasificarea pe un invariant al lui (46d) — se raportează
  cazul, nu se relaxează invariantul;
- reziduul D4 nu se închide la 0,00 pe o lună;
- ModelCheck pică pe un profil; DUK refuză fișierul S după D3;
- rularea Import1C depășește 2× timpul de azi.

## Pașii (un agent per pas, verificare independentă + commit după fiecare)

1. **D1 perf**: o singură trecere, măsurători înainte/după, `EXPLAIN`, index
   doar dacă cifra o cere, addendum în `p5-perf-masuratori.md`, D18-V1.
2. **D2 motor**: helper + toate frunzele de ieșire, D18-V2 pe ambele
   profiluri; Import1C: `valoareAtlas` din registru/helper (riscul 5), fără
   rulare pe Flax încă.
3. **D3 + D4 Import1C**: reclasificarea ca ASM (+BTR) + re-țintirea
   legăturii, categoria D4 în reconciliere, D18-V3.
4. **Re-rularea Flax** (detașată, monitor): D18-V4, cifrele în §Închidere,
   `--saft-s` 09/12 cu DUK.
5. **Închidere**: review advers pe riscurile 1–10, fix-uri printr-un agent de
   implementare, decizia 75 (jurnal + README + CLAUDE.md §75 + roadmap +
   restanțe; 74-r4/r6/r9 închise), istoricul planului.

## Închidere

(se completează la pasul 5)

**Pasul 1 (D18-D1, 2026-08-27).** `AgregatStoc` = o singură interogare
(Repartitor × Lot × TipStoc, `Initial`/`Rulaj` condiționate pe dată),
deschiderea/închiderea/`SoldPeTipStocNeraportat` derivate în memorie
(`SoldPeCheie`). Flax 09/12 2025: XML identic cu 74 (1 linie diferită din
791.464 / 751.933 — `SoftwareVersion`, stamp-ul de build), rapoarte identice.
Proiecție mediană caldă: 09 3,9 → 4,0 s, 12 4,1 → 4,1 s — **ținta < 1 s NU se
atinge**, fiindcă SQL-ul celor trei scanări era ~270 ms (acum 155 ms), nu
costul; `EXPLAIN` = index scan pe `IX_LotId`, fără seq-scan dominant ⇒
**fără index**. Profilul pe secțiuni (addendum 6 în `p5-perf-masuratori.md`):
`CoduriTipPeTipuri` listează TOATE documentele bazei per tip, fără filtru pe
id-uri (≈1,4 s, de două ori), `ComponenteS3` ≈0,8 s — cauzele reale, în afara
lui D1, de decis explicit. D18-V1 verde (scena D17-V2 == suma naivă);
ModelCheck verde (bugetar 764, privat 594).

**Pasul 1, partea a doua.** (a) §7 pe `CoduriTipPeTipuri` (ancoră, doar
Guid-uri) o singură dată, dicționarul partajat cu `ComponenteS3`; filtrul
pe id-uri în SQL măsurat și RESPINS (`= ANY` = |ids| sondări per tip: 0,15 s
pe luna de 9 k, 1,4–1,8 s pe istoricul de 78 k / 105 k cerut de S3); listarea
nefiltrată 0,4 s o dată. Proiecție mediană caldă: 09 **4,0 → 2,9 s**, 12
**4,1 → 3,0 s**; XML/rapoarte identice, DUK ok. (b) `ComponenteS3` măsurat:
agregatul cu join 0,2–0,3 s, GL 0,15–0,2 s ⇒ câștigul posibil ≤ 0,3 s <
0,4 s — NU s-a făcut, restanță. `ApiProiectii.CoduriTip` are aceeași
problemă pe mulțimi mari (hot-path API, neatins — raportat). **Ținta < 1 s
NU e atinsă**: 2,9–3,0 s, fără un vinovat dominant (§8–13 ~1,2 s în felii de
0,1–0,3 s).

**Pasul 2 (D18-D2, 2026-08-27).** Regula stă în MOTOR, nu în frunze:
`StocService.ValoareGolire(soldInainte, cantitate)` (funcția PURĂ, o singură
sursă), `StocService.SolduriLaData` (o interogare grupată per document,
prefix-sum ≤ `Data`, rândurile documentului excluse) și
`StocService.AplicaValoareIesire` (`Motor/StocService.cs`), chemat din
`MotorOperare.CalculeazaSiValideaza` DUPĂ `PregatesteOperare` și ÎNAINTE de
`ValideazaOperare` (`Motor/MotorOperare.cs`, potrivirea regulilor extrasă în
`PotrivesteReguliStoc(strict)` — tolerantă înainte de validare, strictă la
mișcări). Motivul locului: cheia (`TipStoc`, latura) și semnul sunt ale
REGULII de stoc — frunza nu le cunoaște și n-are voie să le re-potrivească
(42a); valoarea rămâne a liniei (`Valoare = −rest × Semn`, deci convenția de
semn a fiecărei frunze se păstrează: BCS/BTR/DSC pozitiv, LDI−/RLF/ASM
negativ). Frunzele (`preț × cantitate`) rămân previzualizarea implicită —
doar comentate. Niciun câmp nou, intrările neatinse (mișcare pozitivă ⇒ regula
nu se aplică). **ASM**: consumul preia restul înainte de validare, produsele
rămân la valoarea CULEASĂ ⇒ invariantul 46d SEMNALEAZĂ cu cenți când operatorul
a evaluat produsul la `preț × cantitate` pe un lot golit (probat: refuz la
10,01 vs 10,00, trece la 10,00) — nu se ajustează nicio latură tăcut; decizia
de a deriva automat valoarea produsului rămâne a închiderii. **RLF**: cenții
cad pe 401 (n-are cont de cheltuială) — consecință acceptată, comentată în
`Retururi.cs`. **Riscul 5 (Import1C, cod, nerulat)**: `AlocareIesire.Aloca`
întoarce acum `(LotId, Cantitate, Valoare)`, valoarea prezisă prin ACEEAȘI
`ValoareGolire` peste soldul la dată pe ambele axe (`Disponibil` citește și
`Valoare`), cu acumularea per document în `AlocatInDocument` (înlocuiește
`Dictionary<Guid, decimal>` la cei 6 apelanți); `valoareAtlas`/`cost`/
`valoareConsum` din HandlereStoc (BTR ×2, BCS, LDI−), HandlerAsamblare,
HandlereRetur (RLF), Vanzare1C (DSC) consumă valoarea alocării — colateral: BTR
și LDI− declarau în punte suma NEROTUNJITĂ, acum rotunjită per linie ca
motorul. RDC (HandlereRetur:577) e intrare, neatins. ASM-ul importului
(riscul 7) trece exact: `Scaleaza` distribuie valoarea REALĂ a consumului.
**D18-V2** (`ModelCheck`, `VerificaValoareIesire`, ambele profiluri; lot
3 × 30,02 ⇒ preț 10,006667, `preț × 3` = 30,02 deci reziduul e al ACUMULĂRII):
(a) 1+1+1 pe trei BCS ⇒ 10,01 / 10,01 / **10,00**, lot 0/0,00, consumul +10,00,
nota 6xx=3xx 10,00, dry-run 10,00 == operare, ReadDto 10,00, anulare ⇒ 1/10,00
și re-operare 10,00 cu exact 2 rânduri, storno ⇒ inverse identice; (b) 1+1+1
într-UN document ⇒ 10,01 / 10,01 / 10,00; (c) BTR pe rest ⇒ sursă −1/−10,00,
destinație +1/+10,00; (d) LDI− ⇒ −10,00; (e) ieșire care nu golește ⇒ 10,01
neschimbat; privat: (f) FCL ⇒ DSC generat cu 10,01 (37b), operat 10,00,
ReadDto 10,00; (g) RLF −10,00, 3xx=401 −10,00; (h) ASM refuz la 10,01, trece
la 10,00, kit 10,00; nicio cheie golită cu valoare ≠ 0. Scena D17-V2/V6 are
prețuri exacte ⇒ nicio cifră veche schimbată (F6 injectează reziduul direct
în registru, rămâne). ModelCheck verde (bugetar 778, privat 613).

**Pasul 3 (D18-D3 + D18-D4, 2026-08-27).** Doar Import1C (Module neatins);
detaliul mecanicii în `docs/import/faza-1c-design.md` §15.

*D3 — reclasificarea ca mișcare* (`HandlereStoc.cs`, `HandlerTransfer`):
rândul `TransferDeMarfuri` cu `contDebit ≠ contCredit` produce, în aceeași
unitate și în ordinea asta, ASM `#reclas` în gestiunea sursei (consum = lotul
de pe `produs@contVechi` prin `Alocare.Aloca`, produs = lot NOU pe
`produs@contNou`, `PretEvaluare = Σ consum / cantitate` ⇒ invariantul 46d la
zero), NTC-punte `#punte` (categoria de azi, la valoarea ASM-ului), BTR pe
lotul nou doar când gestiunile diferă (rezolvat la materializare pe cheia lui
1C, după ASM — tiparul `TransferaProduse`). Cheia `1C:Lot` cu simbolul nou =
lotul nou (`LeagaLotNou`), cea cu simbolul vechi rămâne pe lotul vechi;
`1C:LotAlias` nu se mai scrie la transfer (rămâne DOAR al returului de la
client — `HandlerReturClient`; varianta `Catalog.LeagaAliasLot(cheie, lot)`
cu ObjectSpace propriu ștearsă). Decizia de mișcare se ia contra simbolului
CANONIC al lotului (`tip.Simbol`), puntea contra formei 1C. Gardul reluării:
cheile derivate din sursă (`Cere`) prin `Reluare1C.UnitatePartiala`; BTR-ul
refuză curat dacă ASM-ul nu e operat. Gardul din `HandlerAsamblare` (asamblare
1C care reclasifică) rămâne, cu comentariul re-țintit.

*D4 — reziduul absorbit* (`ReconciliereLuna.ReziduuAbsorbit`): DEFALCARE, nu
categorie aditivă — cifra e deja în explicația contractului 1 (handlerele
declară în punte valoarea prezisă de `Aloca`, restul e „Evaluare"), o a doua
explicație ar fi dublat-o. Din registru: linii de ieșire ne-storno cu
`Valoare − RotunjesteBani(Cantitate × PretUnitar) ≠ 0`, golirea verificată cu
convenția EXACTĂ a motorului (rândurile cu `Data <` + cele din aceeași zi ale
documentelor cu `DataOperare ≤`; prefix-sum-ul pe ziua întreagă dădea 3/9/14
fals-pozitive BTR pe clonă — chei golite și re-încărcate în aceeași zi),
contul de stoc din `Lot.Produs.TipMaterial.ContImplicit`, contrapartida din
`RegistruContabil.DetaliuId` al aceleiași linii; BTR (fără rând contabil) se
numără separat. Blocul `[1] D18-D4` din raportul integral: per cont, reziduul
și `Δ fără reziduu`.

*D18-V3 pe clona `Atlas.Conta.Import1C.Flax.Api`* (`--recreeaza --pana-la 3`,
baza `Flax` neatinsă; seed-ul profilului îl face Import1C singur): **contract
îndeplinit, 0 eșecuri**, 49.498 documente, 4.328 copii, 1.102 realocări;
**30 min 31 s** (ian 8:47 / feb 9:35 / mar 11:09; anul întreg pe codul de
dinainte 1h40 ⇒ ~25 min pe 3 luni: +20 %, sub regula 2×).
Reclasificări (cumulat ian/feb/mar): 59/117/170 rânduri de punte, din care
**59/117/169 ca MIȘCARE** (169 loturi noi, 17 chei discriminate — același lot
1C reclasificat a doua oară pe același cont, 0 linii noi netransferate, 0
„deja pe contul nou", 0 fără Tip), 13/33/48 documente în aceeași gestiune
(fără BTR), **1 cu lotul la valoare 0** (03/2025, `BEDB…717E`, 1 buc / 0,00
lei: ASM cere preț pozitiv — invariantul NU s-a relaxat, rândul rămâne pe
calea veche BTR + punte la 0, contor `ReclasificariValoareZero` + avert;
descoperit ca EȘEC în prima rulare, fix-ul e chiar gardul ăsta). Loturile
vechi consumate: 169, 45 cu rest (reclasificare parțială), **0 cu rezidu
valoric la cantitate 0**; loturile noi: 4 golite până în martie, 2 cu DSC, 0
cu RLF. Aliasuri rămase: 70, toate ale returului de la client. Lanțul
(371→3028→371 în aceeași zi, 06.03: `SED00056123-R` naște lotul pe 3028,
`SED00056180-R` îl consumă și naște din nou pe 371) merge prin cheia exactă.
**Cazul 10.03 (74-r9)**: ASM `SED00056436-R` consumă lotul `SED000097087/381`
−4 / −80,00 (preț 20,00) și naște `SED000097087/371` +4 / 80,00; BTR-8676 îl
mută SED000073 → SED000001; DSC `FLXONL000098622-D` din aceeași zi îl descarcă
pe lotul NOU (−4 / −80,00) și postează **607 = 371** 80,00 (nu 608 = 381);
puntea `SED00056436-P` 371 = 381 80,00; **381 nu mai e creditat de două ori**:
contractul 1 nu mai are Δ pe 381/608 (baseline 03: 381 −80 / 608 +80),
soldul 381 = 2.900,00 debitor = balanța 1C. Contractul 3: 0 chei
nejustificate în toate cele 3 luni (gemenii pe aceeași cheie 1C).
D4 (din registru, cumulat): 371 **32,07 / 30,25 / 20,09**, 607 −32,07 /
−30,25 / −20,08, 3024 0,03 / 6024 −0,03 pe toate, martie și 3028 −0,02 / 6028
0,02 / 401 −0,01; 0 contrapartide ambigue; liniile BTR fără contrapartidă
93/210/319. `Δ fără reziduu` vs baseline-ul de dinainte de D2/D3
(`reconciliere-20260825-203703.txt`): 607 ian −466,94 vs −466,93, feb −335,47
vs −335,44, mar −586,16 vs −585,65 (= −665,65 + 80 mutat de pe 608) —
diferențele de cenți sunt efectele lui D3 pe evaluare (prețul lotului nou =
Σ consum / cantitate, DSC-urile ulterioare pe el), deja în categoria
„Evaluare"; 371 ian 496,79 vs 486,93 (+9,86 = 3028 → 371 mutat de punțile
reclasificării: 3028 de la 9,85 la −0,01).
`--saft-s 2025 3` pe clonă: **DUK ok (J2.2.8), 0 atenționări**, 37,1 MiB,
proiecție 1,9 s; S1/S5/S2/S4 verzi, `MovementTypeTable` 8 coduri (a apărut
20/70 — reclasificarea iese în S ca asamblare, riscul 8: DUK n-o contrazice;
sensul rămâne de decis la închidere); **S3 pe 371 spartă**: NTC −58.663,63 =
punți de reclasificare **−25.748,59 (== ASM #reclas pe 371, la cent)** + RLF
fără acoperire −27.884,34 + `Operatia` (note care mută stoc, 95 rânduri)
−3.533,78 + FCT −736,08 + DSC nerezolvabil −761,29 + BCS 0,45; restul S3 371
= deschidere 4.852,19 ⇒ componenta „reclasificare" e **0**; **381: stoc
2.900 = balanță 2.900, diferență 0** (era creditor); 3028: 0,46 = alte note.
`ReziduValoricFaraCantitate` 129 / 6.265,25 la 31.03 — TOATE pe loturi ale
deschiderii (`SED000004`, `SERSER001`…, preț rotunjit la 4 zecimale la
netare, 47d); 0 pe loturile golite de D2 (verificat: nicio cheie golită în
ian–mar cu valoare ≠ 0 în afara deschiderii — `SoldNegativ` nu apare).
`--sabotaj --pana-la 1` pe clonă (după importul complet, 38 s): **ambele
probe detectate** (contabilă de contractul 1, de stoc de contractul 3, exit
1 = succesul auto-testului); colateral, gardul de golire al lui D4 a prins
rândul de stoc sabotat ca „reziduu pe cheie NEGOLITĂ — altă cauză" (1 linie),
adică exact ce trebuie să facă. Contorul `Loturi (cumulat)` (pin-uri pe
prefix / nerezolvate ⇒ supapa 48a) a intrat în raportul lunii abia după
run-ul integral (0 / 15 pe rularea de sabotaj, care replanifică doar
unitățile fără document) — cifra pe anul întreg vine din pasul 4.

*Abateri și restanțe (pas 3)*: (a) `--continua` pe o bază deja importată dă
**fals-roșu** pe contractele 1/3 și RE-PLANIFICĂ cu efect real: unitățile
fără document legat (BCS/RLF „fără acoperire", RLF blocate de punte) se
replanifică contra registrului complet al lunii (nu al celui de la momentul
cronologic al primei rulări; `Disponibil ≤ dată` vede și documentele aceleiași
zile importate DUPĂ), pot găsi acoperire și se OPEREAZĂ (măsurat: BCS-161 din
01.02, 607 = 371 777,66), iar `RegistruDivergente.Persista` purjează rândurile
vechi ale sursei la primul rând nou (cheia 9424D067… × 941B… 1 buc / 304,67
și-a pierdut justificarea „măsurat"; 401 −65.222 neexplicat) — limitare a
MODULUI (comentariul „de ce nu există purjează la orice replanificare" din
`Divergente.cs` o descrie), nu a pasului 3: idempotența NU s-a probat prin
`--continua`, rămâne restanță (proba validă = re-rulare integrală, cum s-a
făcut). (b) O unitate care a cerut `#reclas` (din sursă) dar n-a produs ASM
(nicio linie acoperită / valoare 0) rămâne „parțială" la rulările următoare
dacă BTR-ul ei s-a operat (refuz cu motiv, `--deblocheaza`) — limitare
moștenită de la `HandlerAsamblare` (`#btr`). (c) Lotul cu valoare 0 nu se
poate re-identifica prin ASM (preț pozitiv, 46d) — rămâne pe contul vechi,
contorizat. (d) `PretEvaluare` la 6 zecimale: pe cantități ≥ ~10.000 `round(q
× preț)` poate diferi cu un ban de Σ consum ⇒ invariantul ar refuza
(nemăsurat pe Flax, 0 eșecuri pe 3 luni; același risc în `HandlerAsamblare`).
(e) Reclasificări din alte tipuri 1C decât `TransferDeMarfuri`: 0 (gardul
`HandlerAsamblare` n-a lovit nimic). (f) Codul S al reclasificării (20/70) —
de decis la închidere (riscul 8).

**Review advers (pasul 5, 2026-08-28) — F1–F10 → fix / restanță.**

- **F1 (D2, invariantul nu ține la retro) + F10 (docs)** → DOCS + PROBĂ:
  D18-D2 și riscul 2 amendate mai sus (golirea e decisă la operare pe registrul
  existent; retro/re-ordonarea intra-zi lasă reziduu declarat); comentariul
  `StocService.AplicaValoareIesire` spune limita; ModelCheck D18-V2 (r)
  reproduce proba review-ului (lot 2 × 20,01, BCS 10.05 apoi BCS retro 05.05 ⇒
  ambele `round(1 × preț)`, cheia 0/−0,01) ca FAPT, plus S 06/2026 pe privat
  (intrarea 0→0 @ −0,01, `ReziduValoricFaraCantitate` o numără).
- **F2 (D4 = oracol, blocant)** → FIX: logica pură extrasă în MOTOR —
  `StocService.VerificaGoliri(rânduriCheie, prețLot, din?, panăLa?)` peste
  `RandGolire` (DocumentId, DetaliuId, Data, Operat, Cantitate, Valoare, Storno,
  IesireFiscala) cu verdicte `Exacta / CuValoare / Fiscala / ReDeschisaRetro /
  Negolita`: pe rândurile VĂZUTE la operare (`Data ≤` + `Operat ≤`, deschiderea
  inclusă) cheia golită cantitativ trebuie să aibă Σ valoare 0,00 — cifra e a
  REGISTRULUI, nu a `ValoareGolire` (necircular). `ReconciliereLuna.
  ReziduuAbsorbit` o consumă: `CuValoare` ⇒ linie FAIL a contractului (cheia,
  lotul, Σ) + linia de contract `1'. D18-D4 oracolul golirii`; `ReDeschisaRetro`
  ⇒ avertisment doar dacă există EFECTIV un rând cu `Data ≤` și `DataOperare >`
  (măsurat, nu presupus); `Negolita` cu reziduu ≠ 0 ⇒ avertismentul de azi
  („altă cauză"). Probată în ModelCheck D18-V2 (o) cu rânduri sintetice
  (golire corectă ⇒ `Exacta`; valoare dublată ⇒ `CuValoare` Σ −10,00; retro ⇒
  `ReDeschisaRetro`; stornat ⇒ 0 verdicte; fiscal ⇒ `Fiscala`; fereastra
  lunii) și pe scena reală (r).
- **F3 (D4 storno)** → FIX: în aceeași funcție, perechea original + invers pe
  `DetaliuId` se sare (`stornate`), deci originalul unei linii stornate nu mai
  intră în categorie; probat (o).
- **F4 (D3 ordinea)** → FIX: `HandlerTransfer.Importa` = ASM → BTR → punte;
  puntea condiționată de ASM Operat (ca BTR-ul); dacă ASM-ul pică, puntea NU
  se scrie și rândurile de reclasificare ale sursei se DECLARĂ nepostate
  (`plan.ReclasRanduri`, categoria „BTR: reclasificarea (ASM #reclas) n-a
  ajuns operată — puntea nu se scrie, rândul sursei rămâne nepostat") ⇒
  contractul 1 explicat, nu roșu mut; unitatea se replanifică integral la
  rularea următoare (#reclas rămâne nelegat).
- **F5 (RLF, decizie)** → FIX: `IDocumentCuIesireFiscala` (marker pe
  DOCUMENT — RLF folosește detaliul de bază, deci un marker pe linie ar fi
  cerut frunză + migrație), declarat de `ReturFurnizor`; `AplicaValoareIesire`
  sare documentele care îl declară; `Alocare.Aloca(absoarbeLaGolire: false)`
  din `HandlerReturFurnizor` prezice aceeași cifră. D18-V2 (g) rescris: RLF
  1 buc pe restul 1/10,00 ⇒ linia −10,01, 3xx = 401 −10,01, lotul 0/−0,01,
  `ValoareTva = −round(10,01 × cota)` (baza TVA = valoarea liniei — F5
  original închis). D4: `Fiscala` contorizată („goliri fiscale RLF, reziduul
  rămas pe lot Σ"), nu FAIL.
- **F6 (ASM UI: valoarea produsului cu cenți)** → RESTANȚĂ 75-r: operatorul
  care evaluează produsul la `preț × cantitate` pe un lot golit primește refuz
  pe invariantul 46d cu cenți (probat D18-V2 (h)); derivarea automată a
  valorii produsului din consum rămâne de proiectat (review: „UI-ul cere
  operatorului să ghicească restul").
- **F7 (D4 perf O(istoric))** → FIX: `ReziduuAbsorbit` citește DOAR ieșirile
  LUNII (`Data ∈ [Prima, Ultima]`) și istoricul loturilor lor (chunk-uri de
  500); cumulatul per cont trăiește în `Stare.ReziduuAbsorbitCumulat`
  (raportul `[1] D18-D4` arată luna + cumulatul, `Δ fără reziduu` pe cumulat).
- **F8 (`CoduriTipPeTipuri` scalare)** → RESTANȚĂ 75-r: listarea tuturor
  documentelor per tip (0,4 s pe 205 k) crește liniar cu baza; alternativa
  (join pe ancora `TipDocument` sau discriminator materializat) se măsoară
  când cifra o cere (59).
- **F9 (D3, `#reclas` cerut fără ASM)** → FIX: `BuclaImport.LeagaFaraDocument`
  scrie legătura cu ținta `Guid.Empty` (convenția „țintă goală" a registrului
  divergențelor/midpoint); `EsteCunoscut` ⇒ true, `Tinta` ⇒ null, `Executa`
  ⇒ skip cu motiv, `--deblocheaza` o șterge ca orfană, idempotența din
  Program.cs n-o numără. `HandlerTransfer` o cheamă când `Cere` a cerut
  `#reclas` dar planul n-a produs ASM (valoare 0 / fără Tip / deja pe cont /
  nicio linie acoperită) — după BTR și punte, ca decizie, nu promisiune.
- Verificări: ModelCheck AMBELE profiluri (cifrele în raportul agentului);
  Import1C build Debug verde; nicio schimbare de API (marker-ul e pe clasa de
  document, nu pe DTO) ⇒ openapi neatins. **Rămâne de probat pe Flax**
  (re-rularea finală, pasul 4): oracolul `1'. D18-D4` verde pe 12 luni, cifra
  „goliri fiscale RLF", `ReDeschisaRetro` (aștept 0 — importul e cronologic),
  ordinea ASM → BTR → punte pe cele 169+ reclasificări, `#reclas` legat fără
  document pe cazul valoare 0 (03/2025).

**Pasul 4 (re-rularea integrală Flax, 2026-08-28 → 29) — proba finală, pe
codul cu fix-urile review-ului.**

*Incident de execuție, consemnat*: prima rulare integrală (28.08 00:00–02:05,
exit 0, contract îndeplinit 12 luni) a rulat pe binarul Release construit la
23:59 — codul PASULUI 3, FĂRĂ fix-urile F2–F9 (agentul de fix a editat
sursele la 00:24–00:33, DUPĂ lansare; sesiunea a murit la 01:16 pe o pană de
net, fără commit). Raportul ei are formatul vechi al blocului D4 (fără
oracolul `1'`), deci NU e proba contractului; artefactele stau în
`run-f18\pas3-binar\` ca dovadă separată că pașii 1–3 țin pe anul întreg.
Proba finală = rularea de mai jos, pe binarul reconstruit din working
tree-ul cu fix-uri.

*Rularea* (`--recreeaza --cititori` + `--reclasifica`, detașat): 28.08
23:32:48 → 29.08 01:25:38, import 1h52 (deschidere 0:52, documente 1:50:56),
exit 0 + 0, **CONTRACT ÎNDEPLINIT, 0 FAIL**, 932 avertismente; reclasificarea
finală 25 s / 23 avertismente / 0 eșecuri.

- **Oracolul `1'. D18-D4` (F2): VERDE pe toate cele 12 luni, 0 goliri cu
  valoare rămasă** (8.734–11.318 goliri verificate/lună pe 8.174–10.524 chei
  exacte). Goliri fiscale RLF (F5): 19/18/26/35/53/56/40/23/27/36/43/24 pe
  luni (Σ 400), TOATE cu reziduul rămas pe lot Σ 0,00 (valoarea fiscală a
  coincis cu restul). `ReDeschisaRetro`: 0 în 11 luni și **1 în aprilie**
  (aștept era 0): lot `01a04a20…` × Marfuri, document din 10.04 cu 1 rând
  retro, valoare la dată 0,01 — limita F1 pe date reale (sursa are un
  document cu `Data` anterioară operat mai târziu chiar și în ordonarea
  cronologică pe timestamp), declarată cu măsurarea rândului retro, nu
  ascunsă.
- **Reclasificarea (D3), cumulat pe an**: 556 rânduri de punte, din care
  **552 ca MIȘCARE** (552 loturi noi, 81 chei discriminate, 0 linii noi
  netransferate, 0 „deja pe contul nou", 0 fără Tip), 2 cu lotul la valoare 0
  (calea veche, contorizate), 169 documente în aceeași gestiune (fără BTR),
  0 rânduri nerezolvate. Ordinea ASM → BTR → punte (F4) a ținut peste tot
  (0 apariții ale categoriei „puntea nu se scrie"). **F9 a lovit exact o
  dată**: `1 × TransferDeMarfuri: reclasificarea cerută de sursă n-a produs
  ASM … — cheie decisă fără document` (cazul valoare-0 din 03/2025).
- **D4 la 31.12** (luna + cumulat): 371 cumulat 40,18 / 607 −40,18, 3024
  0,04 / 6024 −0,04, 3028 0,01 / 6028 −0,01; **`Δ fără reziduu` 371 =
  136.746,56, identic la cent cu rularea pe binarul pas-3**; reziduul pe 401
  al rulării pas-3 (−0,03) a DISPĂRUT — efectul F5 (RLF nu mai absoarbe,
  cenții rămân pe lot).
- **Diff-ul raportului vs baseline-ul F17** (`reconciliere-20260825-203703`
  → `reconciliere-20260828-233406`): 530 de linii schimbate, TOATE atribuite
  — conturile 371/607/3028/6028/3024/6024/303 (mutările punților de
  reclasificare + D4), blocul `[1] D18-D4` nou, contoarele categoriilor din
  `[3]` (mai puține chei de justificat: 575 vs baseline, multe rezolvate
  structural de mișcare); **381/608 dispar INTEGRAL din registrul
  divergențelor** (74-r9: ±80 și ±4.800 pe toate lunile erau dubla creditare;
  acum 0 apariții), 6588 (0,01 × 7 luni) dispare, 6584 scade 0,02 → 0,01.
  Nicio linie schimbată în afara acestor cauze.
- **SAF-T S (09 și 12/2025)**: **DUK ok (J2.2.8), 0 atenționări** pe ambele;
  S3 = 2 conturi diferite (371, 3028 — perechea ASM-stoc / NTC-balanță);
  **punțile de reclasificare == ASM `#reclas` per cont LA CENT** pe ambele
  orizonturi, verificat SQL pe toate cele 5 conturi atinse (09: 3028
  52.493,59−514,03 = 51.979,56, 371 net −57.305,20, 303 10.125,64, 381
  −4.800,00, 3021 0; 12: 371 −88.712,43, 381 −2.620,00 …) ⇒ componenta
  „reclasificare" a lui S3 e exact 0; **381 nu mai apare deloc în S3**
  (baseline: creditor). `ReziduValoricFaraCantitate`: **131 (09) / 132 (12)
  chei, Σ 6.231,84/6.231,85 — de la 861/1.168 în F17**: D2 a stins deriva
  per lot a ieșirilor; rămân loturile deschiderii + limitele declarate
  (F1 retro, F5 fiscal); `SoldNegativ` 4 chei Σ −0,04 (deschidere, 45e).
- Riscul 8 (codul S al reclasificării = 20/70, „iese ca asamblare"):
  DECIS — rămâne 20/70. Codul de mișcare e al TIPULUI × registrului (74a),
  iar mișcarea reclasificării E un ASM; un cod distinct ar cere politică
  per-document, contra lui 74a. DUK nu o contrazice.
