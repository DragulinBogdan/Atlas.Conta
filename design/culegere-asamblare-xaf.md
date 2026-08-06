# ASM — Asamblare, ecranul XAF Blazor (primitive XAF)

Perechea XAF a wireframe-ului React
([culegere-asamblare.md](culegere-asamblare.md)). Primitivele comune ale
shell-ului sunt în [culegere-factura-iesire-xaf.md](culegere-factura-iesire-xaf.md);
aici doar specificul ASM. Surse: `Asamblare.cs` (`[TipDetaliu
(typeof(AsamblareDetaliu))]`, `PregatesteOperare`, `ValideazaOperare`),
`ContaUiBaseline.Asamblare` (coloanele tipizate).

Fapt structural: spre deosebire de NIR, **ASM are detaliu derivat propriu**
(`AsamblareDetaliu`: Directie, PretEvaluare, DataExpirare, LotFabricatie) și
ListView tipizat comutat de `TipDetaliuViewUpdater` — culegerea rolului e
completă în XAF. Ce lipsește (nașterea lotului pe linia de produs, balanța
live) e aceeași familie de limite ca pe NIR/FCL.

## Wireframe — DetailView `Asamblare_DetailView`

```
┌────────────┬──────────────────────────────────────────────────────────────────┐
│ NAVIGAȚIE  │ Asamblare — ● Draft                                              │
│  Asamblări▸├──────────────────────────────────────────────────────────────────┤
│            │ [💾 Salvează][Salvează și închide]  [Operează][Anulează operarea] │
│            │ [Stornează |06.08.2026|]                                         │
│            │  └ DocumentOperareController — comun; fără acțiune specifică ASM │
│            ├──────────────────────────────────────────────────────────────────┤
│            │ ⚠ • Valoarea produsă (590,00) trebuie să fie egală cu valoarea   │
│            │     consumată (585,00) — asamblarea nu creează și nu distruge    │
│            │     valoare.                                                     │
│            │  └ invariantul se AFLĂ abia la Operează (alertul cu buline) —    │
│            │    nu există balanță live (vezi „Ce NU are")                     │
│            ├──────────────────────────────────────────────────────────────────┤
│            │ ▾ ANTET      └ layout-ul IERARHIEI — ASM nu are grup propriu     │
│            │ ┌─ Document ────────────────────┐                                │
│            │ │ Număr      {———}              │                                │
│            │ │ Dată       [06.08.2026 ▦]     │                                │
│            │ │ Predator   [▼ Depozit central]│  ← gestiunea în care se lucrează│
│            │ │ Primitor   [▼ Depozit central]│  ← de regulă ACEEAȘI gestiune  │
│            │ └───────────────────────────────┘                                │
│            │   └ DOUĂ lookup-uri pe laturi (modelul de bază) — „o singură     │
│            │     Gestiune de lucru" din wireframe-ul React e o alegere de     │
│            │     prezentare a clientului, nu a modelului                      │
│            ├──────────────────────────────────────────────────────────────────┤
│            │ ▾ DETALII — ListView TIPIZAT `AsamblareDetaliu_ListView`         │
│            │ [+ Nou] [🗑 Șterge]                                              │
│            │ ┌────────┬──────────┬─────────┬───────┬──────────┬────────┬─────┐│
│            │ │Direcție│Tip (cont)│ Lot     │ Cant. │Preț eval.│ Valoare│ Exp.││
│            │ ├────────┼──────────┼─────────┼───────┼──────────┼────────┼─────┤│
│            │ │ Consum │ 3023 AMB │ L-03101 │50,000 │     —    │  60,00 │  —  ││
│            │ │ Consum │ 371 MF   │ L-04412 │50,000 │     —    │ 530,00 │  —  ││
│            │ │ Produs │ 371 MF   │ {(?)}   │50,000 │   11,80  │ 590,00 │ ... ││
│            │ └────────┴──────────┴─────────┴───────┴──────────┴────────┴─────┘│
│            │   └ coloane = ContaUiBaseline.Asamblare (.Column(Index)):        │
│            │     Directie(0), TipMaterial(1), Lot(2), Cantitate(3),           │
│            │     PretEvaluare(4), Valoare(5), DataExpirare(6),                │
│            │     LotFabricatie(7); TipTva/ValoareTva ASCUNSE (Index=-1 —      │
│            │     ASM nu poartă TVA); un singur grid — zonele Consum/Produs    │
│            │     din React sunt aici o coloană de rol, cu sortare pe ea       │
│            ├──────────────────────────────────────────────────────────────────┤
│            │ ▾ STARE & TOTALURI                                               │
│            │ Stare {Draft}   Data operare {—}   Total {1.180,00}              │
│            │   └ Total-ul bazei = Σ Valoare (aici suma AMBELOR roluri —       │
│            │     cifră fără sens de gestiune pe ASM; pe Operat, după semnare, │
│            │     consumurile negative o aduc la 0 la echilibru)               │
└────────────┴──────────────────────────────────────────────────────────────────┘
```

## Dialogul liniei — `AsamblareDetaliu_DetailView`

```
        ┌─ Linie nouă — Asamblare ─────────────────────────────────┐
        │ Direcție      [▼ Consum ▾]   ← enum DirectieAsamblare;   │
        │   └ editorul standard de enum; FĂRĂ default valid (46d — │
        │     linia fără rol pică la operare, nu la culegere)      │
        │ Tip (cont)    [▼ 371 — Mărfuri                         ] │
        │   └ RuleRequiredField (baza)                             │
        │ Lot           [▼ L-04412                               ] │
        │   └ pe CONSUM: lookup standard pe nomenclatorul de       │
        │     loturi — nefiltrat pe gestiune/sold (motorul refuză  │
        │     la operare); pe PRODUS: ar trebui să se NASCĂ din    │
        │     produs — restanța 53i, ca pe NIR manual              │
        │ Cantitate     [   50,000]  (pozitivă; semnul îl pune     │
        │                             motorul la operare — 28a)    │
        │ Preț evaluare [   11,80 ]  (doar pe Produs; pe Consum    │
        │                             se ignoră — valoarea vine    │
        │                             din prețul lotului)          │
        │ Dată expirare [ __.__.____ ▦]  Lot fabricație [________] │
        │   └ ILinieCuAtributeLot — copiate pe lotul produs de     │
        │     motor la finalizare                                  │
        │ Valoare       {   590,00}  ← rescrisă de PregatesteOperare│
        │                                        [OK]  [Renunță]   │
        └──────────────────────────────────────────────────────────┘
```

Dialogul e UNUL singur pentru ambele roluri — XAF nu condiționează
vizibilitatea câmpurilor pe valoarea altui câmp fără Conditional
Appearance; câmpurile irelevante rolului rămân vizibile și se ignoră
(prețul detaliului unic). Un rând de Appearance („PretEvaluare enabled
doar pe Produs") ar fi pasul următor natural DACĂ se cere — e exact genul
de regulă care în React e un ternar în JSX.

## Verdictele motorului (mesaje exacte din `Asamblare.ValideazaOperare`)

- „Predatorul asamblării este gestiunea în care se lucrează." /
  „Primitorul asamblării este o gestiune (de regulă aceeași cu predatorul)."
- „Fiecare linie de asamblare poartă rolul ei (consum sau produs)."
- „Cantitatea liniei de asamblare nu poate fi zero."
- „Linia de produs își creează lotul la culegere (alegeți produsul)." /
  „Linia de consum descarcă un lot existent."
- „Lotul unei linii de produs se naște pe linia însăși, nu se refolosește."
- „Lotul produsului aparține gestiunii în care se asamblează."
- „Linia de produs cere preț de evaluare pozitiv (valoarea nu se derivă
  din rețetar)."
- „Linia de consum descarcă un lot existent, nu unul creat de acest
  document (lanțul de kitting = documente separate, operate în ordine)."
- „Lotul liniei aparține unui produs cu alt Tip decât Tipul liniei."
- „Valoarea produsă (X) trebuie să fie egală cu valoarea consumată (Y) —
  asamblarea nu creează și nu distruge valoare." (toleranță 0,005)

## Maparea regiune → primitivă (doar ce diferă de FCL)

| Regiune | Primitivă XAF | Notă |
|---|---|---|
| Colecția tipizată | `[TipDetaliu(typeof(AsamblareDetaliu))]` + `TipDetaliuViewUpdater` | New creează derivata cu Directie/PretEvaluare/atribute |
| Rolul liniei | enum `DirectieAsamblare` — editor standard de enum, coloana 0 | fără default valid, deliberat (46d) |
| Coloane / TVA ascuns | `ContaUiBaseline.Asamblare` — `.Column(Index)`, TipTva/ValoareTva la `Index=-1` | ASM nu poartă TVA |
| Semnarea & valorile | `Asamblare.PregatesteOperare` (consum: −cant × preț lot; produs: +cant × PretEvaluare; `Scara.RotunjesteBani`) | idempotent prin Abs |
| Invariantul valoric | `ValideazaOperare` — refuz cu ambele sume în mesaj | doar la Operează |
| Default TVA | `DefaultTipTvaController` — no-op (ancora ASM fără `TipTvaImplicit`) | |

## Ce NU are ecranul XAF (față de wireframe-ul React)

- **Balanța live din subsol** (Σ consum vs Σ produs, Δ, „Egalizează pe
  ultimul produs") — un agregat cross-linii recalculat la tastare cere
  notificări pe care ObjectSpace-ul sincron nu le dă (aceeași familie cu
  `Total` fără refresh — lista React 53g). În XAF invariantul se află la
  Operează, din mesajul motorului, care conține ambele sume — suficient
  pentru „contabil tolerant" (44.2), nehack-uit deliberat.
- **Zonele separate Consum / Produs** — un singur grid cu coloana de rol;
  gruparea vizuală pe zone e prezentare de client React.
- **Nașterea lotului pe linia de produs** — restanța 53i (ca NIR manual):
  mecanismul D2 (`FacturaIntrareLoturiController`) e extensibil la ASM,
  wiring-ul nu există azi; validarea motorului ghidează dar culegerea din
  UI nu poate crea lotul.
- **Câmpuri condiționate de rol** (PretEvaluare doar pe Produs) — posibil
  cu Conditional Appearance dacă se cere; azi câmpurile stau vizibile și
  cele irelevante se ignoră.
