# Dashboard — activitățile curente

Ruta: `/` (ecranul de start al operatorului). Sursa TUTUROR cifrelor:
registre + proiecții server-side (decizia 42c — `IQueryable<RandDto>` +
DataSourceLoader); clientul nu calculează niciodată un sold sau un rest.
Fiecare cifră e un link — dashboard-ul e un cuprins de lucru, nu un raport.

**Jobul ecranului**: la deschiderea aplicației, operatorul vede în 5 secunde
*ce are de terminat* (drafturi), *ce curge* (scadențe, backorder) și *unde e
luna* (închiderea de TVA) — și pleacă cu un click în lucrul propriu-zis.

## Wireframe

```
┌──────────────────────────────────────────────────────────────────────────────┐
│ ATLAS CONTA          miercuri, 6 august 2026 · perioada AUGUST 2026 deschisă │
│                                              [+ Document nou ▾]  [Căutare 🔍]│
├──────────────────────────────────────────────────────────────────────────────┤
│ DE TERMINAT — drafturi                                                       │
│ ┌────────────────┬────────────────┬────────────────┬────────────────┐        │
│ │ Facturi ieșire │ NIR            │ Plăți/Încasări │ Alte documente │        │
│ │      3         │      1         │      2         │      1         │        │
│ │ cea mai veche: │ conex FCT 4471 │ extras 04.08   │ ASM 05.08      │        │
│ │ 01.08 (5 zile) │ (azi)          │                │                │        │
│ └────────────────┴────────────────┴────────────────┴────────────────┘        │
│   (bulinele chihlimbar — Draft; click → lista filtrată pe Draft)             │
├───────────────────────────────────────┬──────────────────────────────────────┤
│ SCADENȚE — rest de stins (imperecheri)│ BACKORDER — rest nedescărcat (FCL)   │
│                                       │                                      │
│ De încasat        {  14.320,50}       │ FCL-0142  MEGACORP   Bec LED 9W      │
│  · depășite (3)   {   4.100,00} ⚠     │           rest        8,000 buc      │
│  · sub 7 zile (2) {   6.250,00}       │ FCL-0138  AGRO TOTAL Cablu 3×1,5     │
│ De plătit         {   9.876,00}       │           rest       12,000 buc      │
│  · depășite (1)   {     980,00} ⚠     │                                      │
│                                       │ (după recepție: [Generează           │
│ FCL-0139 MEGACORP  02.08  {4.100,00}  │  descărcarea] direct de aici)        │
│ FCT-4468 AGRO TOT. 05.08  {  980,00}  │                                      │
│ … toate scadențele →                  │ … tot backorder-ul →                 │
├───────────────────────────────────────┼──────────────────────────────────────┤
│ ÎNCHIDEREA LUNII — TVA                │ AZI — operat                         │
│                                       │                                      │
│  Iul  ✓ ITV-0007 operat               │  09:12  FCL-0142  MEGACORP  {474,93} │
│       4423 de plată {  6.812,40}      │  09:12  ↳ DSC-0089 autogenerat       │
│  Aug  ○ în lucru — 26 documente cu    │  08:40  NIR-0231  AGRO TOTAL         │
│       TVA operate luna asta           │  08:15  INC-0087  {1.200,00}         │
│       [Generează închiderea ⊘]        │                                      │
│       (activ după ultima zi a lunii;  │  4 documente · jurnalul zilei →      │
│        cronologic — 46c)              │                                      │
└───────────────────────────────────────┴──────────────────────────────────────┘
```

## Zonele

### 1. De terminat — drafturi

Un card per familie de tipuri (grupare de UI, nu de model), cu numărul de
drafturi și cel mai vechi. Drafturile sunt lucrul neterminat prin definiție
(ciclul Draft→Operat, decizia 14) — e prima zonă pentru că e singura care
depinde 100% de operator. Conexele autogenerate în Draft (NIR din FCT, DSC
din FCL) apar aici cu proveniența afișată — ele sunt exact „de confirmat".

### 2. Scadențe — rest de stins

Sursa: proiecția imperecherilor (unpivot, 42c) — `rest = Total − Σ stins`
calculat pe server. Două agregate (de încasat / de plătit) cu tranșe pe
urgență, apoi primele N documente cu restul cel mai presant. Click →
documentul, unde stingerea se face prin fluxul de imperechere.

### 3. Backorder — rest nedescărcat

Sursa: `RestNedescarcat` per linie FCL (cusătura fluxului de comenzi —
38a). Lista FCL-urilor operate cu rest, cu produsul și cantitatea; acțiunea
`Generează descărcarea` disponibilă direct (deleagă la `DescarcareService`,
cu dată — 38d). Zona apare doar la profil privat cu descărcare activă;
goală = „Toate livrările sunt descărcate." — starea goală e o afirmație,
nu un gol.

### 4. Închiderea lunii — TVA

Starea lunilor: ✓ ITV operat (cu 4423/4424 rezultat), ○ luna curentă în
lucru. `Generează închiderea` respectă gardienii serviciului (cronologic,
idempotent pe închiderea vie, anti-stale — 46c) — butonul e vizibil dar
dezactivat cu motivul afișat, ca operatorul să știe CE așteaptă. La profil
bugetar (fără PoliticaInchidereTva) zona nu se afișează.

### 5. Azi — operat

Jurnalul zilei: documentele operate azi, cronologic, cu autogeneratele
indentate sub sursă (⛓ lanțul conex vizibil). E confirmarea vie că lucrul
făcut chiar s-a postat — perechea zonei 1.

## Reguli

- **Toate cifrele = server** (TanStack Query, invalidare după orice comandă
  de operare); niciun agregat local.
- **Toate cifrele = link** către lista filtrată sau documentul exact
  (URL = starea — `/fcl?stare=Draft`, `/fcl/0142`).
- Perioada curentă (deschisă/închisă) e în header-ul dashboard-ului —
  gardianul de perioadă e prima cauză de refuz nedumerit; o afișăm
  înainte să doară.
- Cardurile fără date își afirmă starea („Nimic de operat", „Toate
  scadențele sunt la zi") — un dashboard gol dimineața e o veste bună și
  trebuie să arate a veste bună.

## Note

- Zona de scadențe presupune proiecția imperecherii din designul API (42c) —
  există ca design, se implementează la pasul 5.
- „26 documente cu TVA operate luna asta" e un count pe registru (ieftin),
  nu o pre-validare a închiderii — verdictul rămâne al
  `InchidereTvaService`.
