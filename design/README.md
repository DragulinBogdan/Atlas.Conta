# Design — ecranele de culegere (clientul React, pasul 5)

Stare: **schiță de design (wireframe-uri), 2026-08-06.** Ecranele definesc
ținta clientului React (deciziile 42/43) — NU UI-ul XAF Blazor de azi.
Fiecare wireframe e ancorat în modelul real (clasele din
`Module/BusinessObjects/Documente`) și în vocabularul fixat în
`docs/api/p5-react-design.md`: componente compuse în cod, grid de linii
readonly + editor de linie propriu, validarea autoritară = motorul.

| Fișier | Ecran |
|---|---|
| [culegere-factura-iesire.md](culegere-factura-iesire.md) | FCL — factura de ieșire (venit + descărcare de gestiune) |
| [culegere-factura-iesire-xaf.md](culegere-factura-iesire-xaf.md) | FCL — același ecran în primitivele XAF Blazor (starea reală post-GATE) |
| [culegere-nir.md](culegere-nir.md) | NIR — recepția (manual + conexul din FCT) |
| [culegere-nir-xaf.md](culegere-nir-xaf.md) | NIR — același ecran în primitivele XAF (conexul complet; manualul cu restanța 53i) |
| [culegere-asamblare.md](culegere-asamblare.md) | ASM — asamblare/kitting (consum → produs) |
| [culegere-asamblare-xaf.md](culegere-asamblare-xaf.md) | ASM — același ecran în primitivele XAF (detaliu tipizat; fără balanță live) |
| [dashboard.md](dashboard.md) | Dashboard — activitățile curente ale operatorului |

---

## 1. Direcția vizuală

Subiectul e un back-office contabil românesc: operatori care culeg zeci de
documente pe zi, cu cifre care trebuie citite fără efort. Moștenirea vizuală
reală a domeniului e **formularul tipizat** (NIR-ul, factura, bonul de
consum pe hârtie): etichetă mică deasupra valorii, linii subțiri de ghidaj,
rubrici — nu carduri și umbre. Direcția: *registru contabil modern*, nu
„dashboard SaaS".

### Tokens

| Token | Valoare | Rol |
|---|---|---|
| `--hartie` | `#FBFBF9` | fundal (alb cald de hârtie de registru, nu crem) |
| `--tus` | `#1B1D20` | text principal |
| `--tus-slab` | `#5C6066` | etichete, text secundar |
| `--linie` | `#D8D6D0` | hairlines de rubrică |
| `--registru` | `#2B4C7E` | accent — acțiuni, link-uri, focus (albastrul ștampilei) |
| `--operat` | `#2E7D4F` | stare Operat, verdicte pozitive |
| `--draft` | `#B45309` | stare Draft (chihlimbar — „în lucru") |
| `--refuz` | `#B3261E` | erori[], stare Stornat |

### Tipografie

- **UI**: IBM Plex Sans — etichete la 11px/uppercase/tracking larg
  (`--tus-slab`), valori la 14–15px.
- **Cifre**: IBM Plex Mono cu cifre tabulare, pentru ORICE valoare numerică —
  grid, totaluri, dashboard. Ăsta e riscul asumat al direcției: numerele au
  altă voce decât textul, ca într-un registru; coloanele de sume se aliniază
  perfect fără nicio muncă.
- Zecimale bani: mereu 2, aliniate la dreapta. Cantități: 3 (scara 49e).

### Semnătura: **banda de verdict**

Fiecare ecran de document are, sub bara de comenzi, o fâșie persistentă în
care aterizează răspunsul motorului — pentru că în arhitectura asta clientul
nu evaluează reguli, le *simte* prin verdictul serverului (decizia 43b).
Goală pe liniște; la `Verifică`/`Operează` afișează fie erori[] (fond
`--refuz` pal, fiecare eroare clicabilă → focus pe câmpul/linia vinovată),
fie confirmarea cu sumarul postării (fond `--operat` pal: numărul asignat,
rândurile de registru, conexul deschis). E singurul element „tare" vizual;
restul paginii rămâne liniștit.

---

## 2. Anatomia comună — `DocumentShell`

Toate cele trei ecrane de culegere sunt instanțe ale aceleiași anatomii
(decizia 43a); wireframe-urile per ecran arată doar ce diferă.

```
┌──────────────────────────────────────────────────────────────────────────┐
│ ◂ Înapoi   FACTURĂ DE IEȘIRE   ● Draft            [Salvează] [Verifică]  │  bara de comenzi
│                                                   [Operează ▾]           │  (affordances din ReadDto)
├──────────────────────────────────────────────────────────────────────────┤
│ ⚠ 2 erori — motorul refuză operarea:                                     │  BANDA DE VERDICT
│   • Linia 3: linia de stoc cere produsul — alegeți-l.          → linia 3 │  (goală pe liniște)
│   • Factura cu linii de stoc cere gestiunea de descărcare.     → câmp    │
├──────────────────────────────────────────────────────────────────────────┤
│  ANTET — câmpurile headerului, grupate ca în layout-ul tipului           │
├──────────────────────────────────────────────────────────────────────────┤
│  LINII — grid READONLY (afișare); dublu-click / Enter → editorul         │
│  [+ Linie nouă]                                                          │
├────────────────────────────┬─────────────────────────────────────────────┤
│  (gridul de linii)         │  EDITOR DE LINIE (panou lateral)            │
│                            │  vocabularul Camp* — inclusiv lookup grele  │
├────────────────────────────┴─────────────────────────────────────────────┤
│  SUBSOL — totaluri {calculate}: Net / TVA / Total (brut)                 │
└──────────────────────────────────────────────────────────────────────────┘
```

### Reguli transversale (nenegociabile, din deciziile 42/43)

1. **Gridul de linii e readonly.** Culegerea liniei se face în editorul de
   linie (panou lateral), cu vocabularul `Camp*` — gridul doar afișează.
   CRUD per linie NU vorbește cu serverul: agregatul (header + linii) se
   salvează întreg, prin PUT.
2. **TS nu calculează niciodată sold/rest/total autoritar.** Totalurile din
   subsol sunt aritmetica locală a formularului (orientative pe draft);
   verdictul e al motorului — `Verifică` = dry-run `POST .../valideaza`,
   `Operează` = comanda reală. Orice sold de stoc afișat vine de la server.
3. **Câmpurile-rezultat sunt read-only** — `Valoare` se recalculează la
   culegere și motorul o rescrie la operare (GATE XAF D5/D7); `ValoareTva`
   rămâne editabilă (regula 36a: factura bate rotunjirea).
4. **Numărul se asignează la operare** (din PoliticaNumerotare, în faza de
   materializare — 53b). Pe draft, câmpul afișează `(la operare)`.
5. **Lookup-uri** (decizia 43f): mic = local (`TipTva`, `TipDocument`);
   mare = remote cu debounce (`Partener` 129k, `Produs` 312k, `Cont` 1.679).
   Match exact pe DefaultProperty → auto-select.
6. **Stările documentului** conduc tot ecranul: `Draft` (chihlimbar, totul
   editabil) → `Operat` (verde, READ-ONLY integral + acțiunile de după:
   Stornează, Generează descărcarea…) → `Stornat` (roșu pal, read-only).
7. **Dimensiuni**: zonă pliabilă în editorul de linie, populată per tip după
   inventarul DIM-2 (decizia 54e). Wireframe-urile o marchează ca zonă
   rezervată — conținutul exact nu se ghicește.
8. **URL = starea**: `/fcl/:id`, `/nir/:id`, `/asm/:id`, `/` (dashboard).

### Legendă wireframe

```
[Text_______]  input text          [▼ …]      lookup / select
[ 12.10.2026]  dată                {1.234,50} valoare calculată, read-only
(•) (o)        radio               [x] [ ]    checkbox
⊘              dezactivat           ●          bulină de stare
```
