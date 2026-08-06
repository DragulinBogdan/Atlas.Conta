# FCL — Factura de ieșire (culegere)

Ruta: `/fcl/:id` (draft nou: `/fcl/nou`). Ancore de model:
`FacturaIesire` / `FacturaIesireDetaliu` (P2, deciziile 30/37/38),
`DescarcareService` (DSC conex), `TvaService` (calcul la culegere, 53c).

**Jobul ecranului**: un operator facturează servicii și marfă în aceeași
factură, cu TVA calculat live și descărcarea de gestiune generată de motor —
fără să atingă vreodată un cont sau un registru.

## Wireframe

```
┌──────────────────────────────────────────────────────────────────────────────┐
│ ◂ Facturi de ieșire   FACTURĂ DE IEȘIRE   ● Draft    [Salvează] [Verifică]   │
│                                                      [Operează]              │
├──────────────────────────────────────────────────────────────────────────────┤
│                    (banda de verdict — goală pe liniște)                     │
├──────────────────────────────────────────────────────────────────────────────┤
│ DOCUMENT                                     LIVRARE                         │
│ Număr           Dată                         Scadență          Gestiune de   │
│ {(la operare)}  [ 06.08.2026]                [ __.__.____]     descărcare    │
│                                              (goală → +30     [▼ Depozit    │
│ Emitent                 Client                din politică)     central    ] │
│ [▼ SEDIU            ]   [▼ MEGACORP SRL   🔍]                                │
│  (intern)                (partener, remote)  Tip TVA implicit al liniilor:   │
│                                              N21 (din TipDocument — 37f)     │
├──────────────────────────────────────────────────────────────────────────────┤
│ LINII                                                        [+ Linie nouă]  │
│ ┌─┬──────────────────┬─────────┬──────────┬────────┬──────────┬───────────┐  │
│ │#│ Produs / Descriere│ Lot(pin)│Tip (cont)│  Cant. │Preț unit.│  Valoare  │  │
│ ├─┼──────────────────┼─────────┼──────────┼────────┼──────────┼───────────┤  │
│ │1│ Bec LED 9W       │    —    │ 371 MF   │ 20,000 │    12,50 │  {250,00} │  │
│ │2│ ▸Bec LED 9W      │ L-04412 │ 371 MF   │  5,000 │    12,50 │   {62,50} │  │
│ │3│ Transport        │         │ 704 VEN  │  1,000 │    80,00 │   {80,00} │  │
│ └─┴──────────────────┴─────────┴──────────┴────────┴──────────┴───────────┘  │
│   (readonly; Enter/dublu-click → editorul; ▸ = linie cu lot pin-uit)         │
├─────────────────────────────────────┬────────────────────────────────────────┤
│                                     │ LINIA 2                    [Șterge]    │
│                                     │ Produs (identitatea liniei de stoc)    │
│                                     │ [▼ Bec LED 9W                      🔍] │
│                                     │ Tip (cont/clasă)  {371 — Mărfuri}      │
│                                     │   (precompletat din produs; coerența   │
│                                     │    Tip↔Produs o validează motorul)     │
│                                     │ Lot — pin opțional (37d)               │
│                                     │ [▼ L-04412 · 12,000 buc în Depozit ]   │
│                                     │   (doar loturile produsului, cu sold   │
│                                     │    în gestiunea de descărcare — sold   │
│                                     │    de la server; gol = auto-FIFO)      │
│                                     │ Cantitate      Preț unitar             │
│                                     │ [     5,000]   [     12,50]            │
│                                     │ Tip TVA        Valoare TVA   Valoare   │
│                                     │ [▼ N21 21%]    [    13,13]   {62,50}   │
│                                     │   (TVA recalculat live; suma tastată   │
│                                     │    se păstrează — 36a)                 │
│                                     │ ▸ Dimensiuni (pliat — după DIM-2)      │
│                                     │            [Gata — Enter] [Renunță]    │
├─────────────────────────────────────┴────────────────────────────────────────┤
│                       Net {392,50}   TVA {82,43}   TOTAL {474,93}            │
└──────────────────────────────────────────────────────────────────────────────┘
```

## Câmpuri

### Antet

| Câmp | Editor | Reguli / sursă |
|---|---|---|
| Număr | {read-only} | Serie fiscală `FCL-`, asignată la operare (53b). |
| Dată | dată | Default azi; gardianul de perioadă refuză perioada închisă. |
| Emitent (Predator) | lookup intern | Repartitor intern — un partener aici = refuz la operare. |
| Client (Primitor) | lookup remote | `Partener` (129k) — search cu debounce, match exact → auto-select. |
| Scadență | dată, opțională | Necules → motorul aplică PoliticaScadenta (+30) la operare (30c). |
| Gestiune de descărcare | lookup local | Obligatorie doar cu linii de stoc (validare motor); una per factură la P2. |

### Linia (editor) — o singură derivată, două identități

- **Linie de stoc** (Natura clasei = Stoc): `Produs` OBLIGATORIU
  (General! — identitatea liniei), `Lot` = pin opțional (Specific? — 37d),
  fără fallback FIFO pe restul pinului. Descărcarea reală o generează
  motorul (DSC conex) la operare.
- **Linie de serviciu**: `Descriere` liberă, Tip din clasa VEN (704/706/708),
  fără Produs/Lot — aceleași câmpuri, editorul nu schimbă forma, doar ce e
  activ (condiționalitate în cod, per felie — decizia 43a).

| Câmp | Editor | Reguli |
|---|---|---|
| Produs | lookup remote (312k) | Obligatoriu pe stoc; precompletează Tipul. |
| Tip (cont/clasă) | lookup remote | Obligatoriu (bază); coerența cu produsul = validare motor. |
| Lot (pin) | lookup filtrat | Doar loturile produsului; afișează soldul în gestiunea de descărcare (server). Fără sold → „întâi transfer (BTR)". |
| Cantitate | numeric 3 zec. | > 0 (validare motor pe toate liniile FCL). |
| Preț unitar | numeric 6 zec. | Prețul de VÂNZARE — decuplat de cost (fluxul-ancoră 37). |
| Tip TVA | select local | Default din `TipTvaImplicit` (N21) la linia nouă. |
| Valoare TVA | numeric | Recalculată la orice schimbare de bază; valoarea tastată se PĂSTREAZĂ (36a). |
| Valoare | {read-only} | Rezultat (`TvaService`): net la regim normal, brut la Capitalizat. |

## Fluxul de culegere (tastatură)

1. `Client` (primul focus pe draft nou) → Enter avansează prin antet.
2. `+ Linie nouă` (`Ctrl+Enter` din grid): focus pe `Produs`; scanare/cod →
   match exact → auto-select → `Cantitate` → `Preț` → `Gata`.
3. Linia se adaugă în grid, editorul se golește pentru următoarea
   (culegere în serie); `Esc` închide editorul.
4. `Salvează` (PUT agregat) → `Verifică` (dry-run, opțional) → `Operează`.

## După operare

```
├──────────────────────────────────────────────────────────────────────────────┤
│ ✓ Operată: FCL-2026-0142 · 4 rânduri de registru · DSC-0089 (draft) deschis  │
│   Rest nedescărcat: Bec LED 9W — 8,000 buc (backorder)     [Vezi DSC-0089]   │
├──────────────────────────────────────────────────────────────────────────────┤
│  ● Operat — document read-only        [Generează descărcarea (rest)] [Stornează]│
```

- Verdictul afișează conexul (DSC draft, deschis în editare) și **restul
  nedescărcat** per linie (`RestNedescarcat` — cusătura fluxului de comenzi,
  38a). Restul NU intră pe DSC — se raportează.
- `Generează descărcarea` (doar pe Operat, cu dată — default azi) reia
  restul după recepția ulterioară (backorder → 37c).
- Anulare/stornare trec prin gardienii de grup (copii operați → refuz).

## Note

- La profil BUGETAR liniile de stoc sunt interzise declarativ
  (PoliticaValidare) — același ecran, verdictul motorului face diferența;
  clientul nu știe profilul.
- Restanța `Lot.DefaultProperty` (40d/43) e a modelului — lookup-ul de pin
  presupune eticheta rezolvată (`Eticheta` există din decizia 53).
