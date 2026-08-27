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
