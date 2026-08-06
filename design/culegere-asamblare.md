# ASM — Asamblare / kitting (culegere)

Ruta: `/asm/:id`. Ancore de model: `Asamblare` / `AsamblareDetaliu`
(1C-a, decizia 46d): n consumuri → m produse, într-o gestiune; direcția
explicită pe linie se materializează în semn la operare (mecanica LDI 28a);
invariantul valoric Σ produse = Σ consumuri (toleranță 0,005); FĂRĂ contare
la marfă→marfă. Dezasamblarea = același tip, alt rol pe linii.

**Jobul ecranului**: operatorul spune *ce se desface* și *ce rezultă*, cu
valoarea alocată manual pe produse (rețetarul/BPR e alt subiect, rezervat —
decizia 19). Ecranul face vizibilă singura regulă care contează: valoarea
nu se creează și nu se distruge, doar se mută între loturi.

## Wireframe

```
┌──────────────────────────────────────────────────────────────────────────────┐
│ ◂ Asamblări   ASAMBLARE   ● Draft                [Salvează] [Verifică]       │
│                                                  [Operează]                  │
├──────────────────────────────────────────────────────────────────────────────┤
│                    (banda de verdict — goală pe liniște)                     │
├──────────────────────────────────────────────────────────────────────────────┤
│ DOCUMENT                                                                     │
│ Număr           Dată             Gestiune de lucru                           │
│ {(la operare)}  [ 06.08.2026]    [▼ Depozit central          ]               │
│                                   (predator = primitor; loturile consumate   │
│                                    și cele produse trăiesc aici)             │
├──────────────────────────────────────────────────────────────────────────────┤
│ CONSUM — loturi existente, descărcate la prețul lor          [+ Consum]      │
│ ┌─┬───────────────────┬──────────┬──────────┬────────┬──────────┬─────────┐  │
│ │#│ Produs            │ Lot      │Tip (cont)│  Cant. │Preț lot  │ Valoare │  │
│ ├─┼───────────────────┼──────────┼──────────┼────────┼──────────┼─────────┤  │
│ │1│ Cutie carton M    │ L-03101  │ 3023 AMB │ 50,000 │  {1,20}  │ {60,00} │  │
│ │2│ Bec LED 9W        │ L-04412  │ 371 MF   │ 50,000 │ {10,60}  │{530,00} │  │
│ └─┴───────────────────┴──────────┴──────────┴────────┴──────────┴─────────┘  │
├──────────────────────────────────────────────────────────────────────────────┤
│ PRODUS — loturi noi, cu valoarea alocată explicit            [+ Produs]      │
│ ┌─┬───────────────────┬──────────┬──────────┬────────┬──────────┬─────────┐  │
│ │#│ Produs            │ Lot      │Tip (cont)│  Cant. │Preț eval.│ Valoare │  │
│ ├─┼───────────────────┼──────────┼──────────┼────────┼──────────┼─────────┤  │
│ │3│ Set cadou LED ×1  │ {(nou)}  │ 371 MF   │ 50,000 │ [ 11,80] │{590,00} │  │
│ └─┴───────────────────┴──────────┴──────────┴────────┴──────────┴─────────┘  │
├─────────────────────────────────────┬────────────────────────────────────────┤
│                                     │ LINIE DE CONSUM (#2)       [Șterge]    │
│                                     │ Rol   (•) Consum   (o) Produs          │
│                                     │ Produs                                 │
│                                     │ [▼ Bec LED 9W                      🔍] │
│                                     │ Lot — existent, cu sold în gestiune    │
│                                     │ [▼ L-04412 · 62,000 buc · 10,60/buc ]  │
│                                     │   (sold de la server; lotul născut de  │
│                                     │    ACEST document = refuz — kitting-ul │
│                                     │    în lanț = documente separate)       │
│                                     │ Cantitate       Valoare                │
│                                     │ [    50,000]    {530,00}               │
│                                     │   (preț lot × cant. — read-only)       │
│                                     │            [Gata — Enter] [Renunță]    │
│                                     ├────────────────────────────────────────┤
│                                     │ (pe rol Produs, în plus:)              │
│                                     │ Preț de evaluare  [    11,80]          │
│                                     │ Dată expirare     [ __.__.____]        │
│                                     │ Lot fabricație    [___________]        │
├─────────────────────────────────────┴────────────────────────────────────────┤
│ BALANȚA ASAMBLĂRII (orientativă — verdictul e al motorului)                  │
│   Consum {590,00}  ─────────────▶  Produs {590,00}     Δ {0,00} ✓            │
└──────────────────────────────────────────────────────────────────────────────┘
```

## Câmpuri

### Antet

| Câmp | Editor | Reguli |
|---|---|---|
| Număr | {read-only} | La operare, din politică. |
| Dată | dată | Gardian de perioadă; soldul consumului se verifică la această dată. |
| Gestiune de lucru | lookup local | Un singur câmp în UI → Predator ȘI Primitor (modelul permite gestiuni diferite, dar cazul curent e una; escape: „Gestiuni diferite" sub un disclosure). |

### Linia — rolul întâi

`Rol` (Consum/Produs) e PRIMUL câmp al editorului: schimbă ce urmează.
Fără default valid (enum-ul refuză linia fără rol — 46d); gridul le
grupează pe zone, nu pe coloană de rol.

| Câmp | Consum | Produs |
|---|---|---|
| Produs | lookup remote — filtrează loturile | lookup remote — identitatea lotului NOU |
| Lot | existent, cu sold în gestiune la dată; afișează sold + preț unitar (server) | {(nou)} — se naște pe linie la culegere (`CreeazaLot`), în gestiunea de lucru |
| Tip (cont) | {din produs} | {din produs} |
| Cantitate | numeric 3 zec., pozitivă (semnul îl pune motorul: consum −) | pozitivă (produs +) |
| Preț | {preț lot} — read-only | `Preț de evaluare` [obligatoriu, > 0] — valoarea NU se derivă din rețetar |
| Valoare | {preț lot × cant.} | {preț eval. × cant.} |
| Atribute lot | — | Dată expirare / Lot fabricație, opționale (copiate de motor la finalizare) |

## Semnătura locală: balanța din subsol

Subsolul nu e un total — e **invariantul făcut vizibil**: Σ consum vs
Σ produs, cu Δ live și prag 0,005. Verde la echilibru, `--refuz` la
dezechilibru, cu scurtătura `[Egalizează pe ultimul produs]` care ajustează
prețul de evaluare al ultimei linii de produs ca Δ→0 (aritmetică locală de
formular, pe valori deja afișate — autoritatea rămâne a motorului la
operare). E gestul care economisește cele mai multe tastări pe kitting-ul
1→1 cu ambalaj.

## Verdicte specifice (motorul — 46d)

- Rol lipsă / cantitate zero / lot lipsă pe consum / produs fără preț de
  evaluare pozitiv.
- „Linia de consum descarcă un lot existent, nu unul creat de acest
  document" — lanțul de kitting = documente separate, operate în ordine.
- Coerența Tip-linie ↔ produsul lotului; lotul produsului aparține
  gestiunii de lucru.
- |Σ produse − Σ consumuri| ≤ 0,005 — mesajul motorului arată ambele sume.
- Gardianul de sold refuză consumul unde lotul nu are sold la dată.

## Note

- Dezasamblare: același ecran — 1 linie de consum, n linii de produs;
  nu există flag, rolurile spun tot.
- FĂRĂ sumar de TVA în subsol: ASM nu are semantică de TVA (coloanele
  TipTva/ValoareTva ascunse — baseline 40a); balanța ține locul totalului.
