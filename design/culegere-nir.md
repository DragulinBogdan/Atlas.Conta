# NIR — Nota de intrare-recepție (culegere)

Ruta: `/nir/:id`. Ancore de model: `NIR` (`DocumenteGestiune.cs`), detaliul
de BAZĂ azi; deciziile 26 (conexul din FCT, nașterea lotului), 54e (NIR
primește detaliu derivat propriu la DIM-2), 53i (culegerea de produs pe NIR
manual = restanță deschisă — **acest wireframe definește ținta**).

**Jobul ecranului**: două origini, un singur ecran —
1. **NIR conex** (majoritar): generat de motor la operarea FCT, cu liniile
   de stoc clonate și loturile deja născute pe factura-mamă. Operatorul
   CONFIRMĂ recepția fizică, nu re-culege.
2. **NIR manual**: recepție fără factură în sistem — liniile își nasc
   loturile la culegere (25c/26e), pe produs.

## Wireframe — NIR conex (cazul frecvent)

```
┌──────────────────────────────────────────────────────────────────────────────┐
│ ◂ NIR-uri   NOTĂ DE INTRARE-RECEPȚIE   ● Draft · autogenerat                 │
│                                                    [Salvează] [Operează]     │
├──────────────────────────────────────────────────────────────────────────────┤
│                    (banda de verdict — goală pe liniște)                     │
├──────────────────────────────────────────────────────────────────────────────┤
│ ⛓ Generat din FCT nr. 4471 / 04.08.2026 — MEGACORP SRL       [Deschide FCT]  │
│   (liniile de stoc ale facturii, cu loturile născute acolo; re-operarea      │
│    facturii îl regenerează — 26d)                                            │
├──────────────────────────────────────────────────────────────────────────────┤
│ DOCUMENT                                                                     │
│ Număr           Dată recepție    Furnizor              Gestiune primitoare   │
│ {(la operare)}  [ 06.08.2026]    {MEGACORP SRL}        [▼ Depozit central ]  │
│                                   (din factură)                              │
├──────────────────────────────────────────────────────────────────────────────┤
│ LINII — preluate din factură (valorile NET — TVA rămâne pe FCT, 36b)         │
│ ┌─┬───────────────────┬──────────┬──────────┬────────┬───────────┬────────┐  │
│ │#│ Produs (lot)      │ Lot      │Tip (cont)│  Cant. │  Valoare  │ Recepț.│  │
│ ├─┼───────────────────┼──────────┼──────────┼────────┼───────────┼────────┤  │
│ │1│ Bec LED 9W        │ {L-04413}│ 371 MF   │ 20,000 │  {212,00} │  [x]   │  │
│ │2│ Cablu 3×1,5 (50m) │ {L-04414}│ 3028 MAT │  4,000 │  {180,00} │  [x]   │  │
│ └─┴───────────────────┴──────────┴──────────┴────────┴───────────┴────────┘  │
│   (lot/valoare read-only — s-au cules pe factură; „Recepț." = bifă de        │
│    control fizic, efemeridă de UI, nu se persistă)                           │
├──────────────────────────────────────────────────────────────────────────────┤
│                                          Total recepție (net)  {392,00}      │
└──────────────────────────────────────────────────────────────────────────────┘
```

Diferență cantitativă la recepție (marfă lipsă/deteriorată): NU se editează
NIR-ul conex — corecția aparține facturii (anulare → corectare → re-operare
regenerează conexul) sau, post-operare, diferenței de inventar. Ecranul
oferă doar scurtătura `[Deschide FCT]`.

## Wireframe — NIR manual (editorul de linie activ)

```
┌──────────────────────────────────────────────────────────────────────────────┐
│ ◂ NIR-uri   NOTĂ DE INTRARE-RECEPȚIE   ● Draft     [Salvează] [Verifică]     │
│                                                    [Operează]                │
├──────────────────────────────────────────────────────────────────────────────┤
│ DOCUMENT                                                                     │
│ Număr           Dată recepție    Furnizor (predator)   Gestiune primitoare   │
│ {(la operare)}  [ 06.08.2026]    [▼ AGRO TOTAL SRL 🔍] [▼ Magazie materiale] │
├──────────────────────────────────────────────────────────────────────────────┤
│ LINII                                                        [+ Linie nouă]  │
│ ┌─┬───────────────────┬──────────┬──────────┬────────┬───────────┐           │
│ │#│ Produs            │ Lot      │Tip (cont)│  Cant. │  Valoare  │           │
│ ├─┼───────────────────┼──────────┼──────────┼────────┼───────────┤           │
│ │1│ Sămânță lucernă   │ {(nou)}  │ 3028 MAT │ 10,000 │  {450,00} │           │
│ └─┴───────────────────┴──────────┴──────────┴────────┴───────────┘           │
├─────────────────────────────────────┬────────────────────────────────────────┤
│                                     │ LINIE NOUĂ                 [Șterge]    │
│                                     │ Produs — naște lotul (25c)             │
│                                     │ [▼ Sămânță lucernă                 🔍] │
│                                     │   (produs nou? → [+ Produs] inline)    │
│                                     │ Tip (cont/clasă)  {3028 — Materiale}   │
│                                     │   (din produs)                         │
│                                     │ Lot {(se naște pe linie — gestiunea    │
│                                     │       primitoare a headerului)}        │
│                                     │ Cantitate      Valoare (cost recepție) │
│                                     │ [    10,000]   [    450,00]            │
│                                     │ Atribute lot (opțional)                │
│                                     │ Dată expirare   Lot fabricație         │
│                                     │ [ __.__.____]   [___________]          │
│                                     │ ▸ Dimensiuni (pliat — după DIM-2)      │
│                                     │            [Gata — Enter] [Renunță]    │
├─────────────────────────────────────┴────────────────────────────────────────┤
│                                          Total recepție  {450,00}            │
└──────────────────────────────────────────────────────────────────────────────┘
```

## Câmpuri

| Câmp | Editor | Reguli / sursă |
|---|---|---|
| Număr | {read-only} | La operare, din politică (dacă tipul are rând de numerotare). |
| Dată recepție | dată | Gardian de perioadă la operare. |
| Furnizor (Predator) | lookup remote | Trebuie `Partener` (validare motor: Partener→Gestiune). |
| Gestiune primitoare (Primitor) | lookup local | Trebuie `Gestiune`; loturile liniilor trebuie să-i aparțină. |
| Produs (linie) | lookup remote | Naște lotul la culegere pe linia proprie — ținta 53i; NIR conex: read-only, lotul e al facturii. |
| Tip (linie) | lookup remote | Precompletat din produs; obligatoriu. |
| Cantitate | numeric 3 zec. | > 0. |
| Valoare | numeric (manual) / {read-only} (conex) | Cost de recepție NET; motorul finalizează lotul la operare: PretUnitar = Valoare/Cantitate. |
| Dată expirare, Lot fabricație | opționale | `ILinieCuAtributeLot` — copiate pe lot de motor la finalizare; intră pe detaliul derivat NIR (54e). |

## Verdicte specifice (motorul)

- „Fiecare linie de recepție cere lot" / „lotul aparține gestiunii
  primitoare" (26f).
- Recepția CONTEAZĂ pe NIR (3xx = furnizor la bugetar cu TVA capitalizat;
  net la privat) — operatorul nu vede conturi, doar sumarul postării în
  banda de verdict.

## Note

- Schimbarea gestiunii primitoare pe un NIR manual cu linii culese mută
  loturile născute (sincronizare la salvare — seam-ul de culegere, ca
  `FacturaIntrareLoturiController` pe FCT, 53a).
- Bifa „Recepționat" e deliberat efemeră: controlul fizic e al operatorului,
  nu al modelului — nu inventăm câmp persistat fără cerință (decizia 21).
