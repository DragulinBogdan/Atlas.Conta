# Pasul 5, felia 11 — jurnalele de TVA — DESIGN

Ultima extensie naturală a raportării, și prima care nu poate fi scrisă peste
registrele existente. Nota din felia 9 spunea că jurnalele de TVA sunt „per
document prin natură … felie proprie, peste altă sursă". Fork-ul a fost tranșat
(2026-08-17): **altă sursă = un registru NOU**, nu documentele.

## De ce registru, și nu o proiecție peste documente

Trei texte, toate mai puternice decât nota de atunci:

- **Invariantul III**: „orice sold, balanță, fișă sau raport e o sumă peste
  registre … niciodată o plimbare peste documente; nicio interogare polimorfă pe
  `Document` în fluxuri calde."
- **Decizia 36f**, care a amânat exact tema asta, cu forma ei deja aleasă:
  „prorata/ajustări/D300/D394/SAF-T ca **proiecții peste registre**".
- **Decizia 14**: schema registrelor (care există, pe ce dimensiuni) e **COD** —
  deci un registru nou e o decizie legitimă, nu o excepție.

Iar registrele de azi nu pot purta jurnalul, din două motive independente:

1. **Rândul contabil de TVA nu știe ce a fost taxat.** `RegistruContabil` poartă
   valoarea TVA și conturile, dar nici **baza impozabilă**, nici **tipul de TVA**.
   Jurnalul le cere pe amândouă, pe fiecare rând.
2. **Jumătate din jurnal nu postează deloc.** Liniile `Scutit`, `Neimpozabil` și
   `Capitalizat` (achiziție fără drept de deducere) nu produc niciun rând
   contabil de TVA — motorul le sare explicit — dar apar legal în jurnalul de
   cumpărări/vânzări și în D300. O proiecție peste rândurile 4426/4427 le-ar
   pierde tăcut, deci ar fi un raport incomplet cu aparență de raport complet.

A treia cale — proiecția peste `DocumentDetaliu` — ar fi fost gratuită, dar ar fi
produs **al doilea adevăr**: jurnalul n-ar mai fi avut nicio legătură structurală
cu soldul lui 4426/4427, iar starea documentelor (Draft/Operat/Stornat) ar fi
trebuit reimplementată în proiecție. Registrul, în schimb, se închide pe cifra
contabilă **prin construcție** (JT-D6).

---

## Deciziile de fixat

### JT-D1 — `RegistruTva`: al treilea registru, un rând per LINIE

Scris de motor, în aceeași tranzacție cu celelalte două, append-only, complet
rezolvat:

```
RegistruTva
  Data           ← doc.Data
  DocumentId     ← NENULL (vezi mai jos)
  DetaliuId      ← NENULL — granularitatea liniei, deci SAF-T-ready
  Sens           ← Achizitie / Livrare, din PoliticaTva.Directie
  PartenerId?    ← repartitorul laturii din PoliticaTva.SursaContrapartida
  TipTvaId       ← identitatea fiscală a liniei
  Regim, Cota    ← SNAPSHOT (vezi JT-D3)
  Baza, Tva      ← cifrele deja rezolvate (vezi JT-D4)
  Storno         ← ca la celelalte registre
```

**Fără rânduri de deschidere.** Celelalte două registre au excepția declarată
`DocumentId = null` (soldurile scrise de migrare — 25e/34d). Aici nu există:
soldul de deschidere al lui 4426/4427 e o poziție contabilă, nu o **operațiune
taxabilă**, iar un jurnal de TVA listează operațiuni. `DocumentId`/`DetaliuId`
sunt deci NENULE, ceea ce face și JT-D6 exact.

Per LINIE, nu per document: SAF-T cere nivelul liniei, motorul lucrează oricum
acolo, iar agregarea la document e gratuită în proiecție. Invers n-ar fi fost.

### JT-D2 — Criteriul de generare: politica tipului × tipul liniei

Un rând se naște când **amândouă** sunt adevărate:

- tipul documentului are un rând `PoliticaTva` — adică profilul declară că *acest
  tip de document e un eveniment de TVA*;
- linia are `TipTvaId`.

Fiecare jumătate face o muncă pe care cealaltă n-o poate face:

- **Politica** e ce ține profilul **bugetar** inert (n-are niciun rând
  `PoliticaTva`, deci n-are jurnale — corect, neplătitor), deși liniile lui
  CHIAR au `TipTva` (CAP21 e implicitul FCT/FCL/DEC acolo). Un criteriu bazat
  doar pe linie ar fi fabricat un jurnal de TVA pentru un neplătitor.
- Tot ea ține în afară **NIR-ul conex**, care clonează `TipTvaId` ca informație
  (26b) dar nu postează TVA, și **închiderea lunară** (ITV — notă contabilă, fără
  politică): închiderea nu e o operațiune taxabilă.
- **Tipul liniei** e ce distinge o linie fiscală de una fără regim declarat.

Consecință asumată, raportată nu inventată: linia **fără** `TipTva` de pe un
document cu politică rămâne în afara jurnalului. E o gaură a datelor, nu a
modelului, și se măsoară (JT-D6, verificarea 3) în loc să fie umplută cu un regim
presupus.

### JT-D3 — Snapshot ce a determinat aritmetica; join ce e etichetă

`Cota` și `Regim` se **copiază pe rând**. `TipTva` e nomenclator editabil: dacă
cineva corectează cota în 2026, jurnalul lui 2025 nu are voie să se schimbe —
exact principiul „rândul de registru se scrie o dată, complet rezolvat" (III).

`Denumirea` partenerului, `CodFiscal`-ul lui și codurile `CodSafTLivrare/
Achizitie` **nu** se snapshot-ează: sunt etichete, rezolvate la citire prin join
(ca `ContSimbol` în balanță). Codurile ANAF sunt, în plus, nomenclator care se
schimbă cu anul de raportare — o declarație se generează cu nomenclatorul în
vigoare atunci, nu cu cel de la momentul operării.

Linia de demarcație, scrisă ca regulă: **snapshot pentru ce a intrat în calcul,
join pentru ce doar se afișează.**

### JT-D4 — `Baza` și `Tva` per regim: patru cazuri, o formulă fiecare

| Regim | Baza | Tva |
|---|---|---|
| `Normal`, `TaxareInversa` | `Valoare` | `ValoareTva` |
| `Scutit`, `Neimpozabil` | `Valoare` | `0` |
| `Capitalizat` | `Valoare / (1 + Cota/100)` | `Valoare − Baza` |

`Capitalizat` e singurul care **calculează**: acolo `Valoare` e BRUTĂ (TVA-ul e
capitalizat în cost — `TvaService`), deci baza se desface înapoi. Rezultatul e
TVA **nedeductibilă**: apare în jurnal și în D300 ca atare, distinsă prin
`Regim`, care e chiar pe rând.

Rotunjirea la bani se face o singură dată, prin `Scara`, ca peste tot.

### JT-D5 — Storno și anulare: aceleași căi, niciun mecanism nou

`AnuleazaOperarea` șterge rândurile documentului — se adaugă și cele de TVA.
`Storneaza` scrie rânduri inverse la data stornării — se adaugă și aici
(`Baza`/`Tva` cu semn schimbat, `Storno = true`). Jurnalul nu filtrează niciodată
`Storno`: registrul e append-only și suma lui algebrică e adevărul (R-D7).

Retururile (RLF/RDC) intră natural: liniile lor sunt negative prin construcție
(46a), deci reduc baza și TVA-ul perioadei fără niciun caz special.

### JT-D6 — Reconcilierea structurală, PER DOCUMENT

Aici se plătește alegerea registrului. Pentru fiecare document prezent în
`RegistruTva`:

```
Σ Tva  (regimuri care postează: Normal, TaxareInversa)
   ==
Σ Valoare  a rândurilor lui din RegistruContabil pe conturile de TVA
```

Per document, nu pe perioadă — și diferența contează: agregatul de perioadă ar
include rândurile închiderii lunare (ITV mișcă 4426/4427 fără să fie operațiune
taxabilă) și ar cere excluderi scrise de mână. Per document, ITV pur și simplu nu
apare în `RegistruTva`, deci nu are ce exclude.

Trei verificări în ModelCheck, ambele profiluri:

1. **Cusătura de mai sus**, pe toate documentele bazei.
2. **Bugetarul rămâne gol** — niciun rând de TVA pe un profil fără `PoliticaTva`,
   deși liniile poartă `TipTva`.
3. **Măsurarea găurii** (JT-D2): numărul liniilor fără `TipTva` de pe documente
   cu politică se raportează, nu se ascunde.

### JT-D7 — Proiecțiile: jurnalul agregă la document, decontul la cotă

- **Jurnal de cumpărări / de vânzări** — un rând per `(Document × TipTva)`:
  data, numărul, tipul documentului, partenerul + `CodFiscal`, cota, regimul,
  baza, TVA. Un jurnal listează **facturi**, nu poziții de factură; registrul
  rămâne per linie pentru cine are nevoie (SAF-T). Un singur ecran
  parametrizat pe `Sens`, ca `PLT`/`INC` (57a).
- **Decont** — un rând per `(Sens × TipTva)`, cu totalurile de bază și TVA.

  **Corectat la implementare**: prima formulare spunea „per `(Sens × Regim ×
  Cota)`" — și ar fi fost greșit. `SDD` (scutit CU drept de deducere) și `SFD`
  (scutit FĂRĂ drept) au același regim și aceeași cotă 0, dar coduri SAF-T
  diferite (310314 vs 310326) și rânduri diferite în D300. Gruparea pe regim×cotă
  le-ar fi fuzionat, adică ar fi produs exact cifra pe care declarația n-o poate
  folosi. **`TipTva` E identitatea de raportare** — el poartă mapările — deci el
  e cheia. Regimul și cota rămân coloane afișate.

Ambele prin `DataSourceLoader`, ca restul listelor.

### JT-D8 — Backfill: registrul se REGENEREAZĂ din documente, nu se scrie de mână

Rândurile există doar pentru documente operate după felie. Pentru bazele vii
(dev, clona de import cu 205k documente) derivarea e aceeași funcție a motorului,
apelată pe documente deja operate — nu o scriere directă în registru (interdicția
din invariantul V). Idempotent: un document care are deja rânduri se sare.

**Tranșat (2026-08-17): comandă de backfill idempotentă**, nu re-rulare de
import. Contractul Import1C rămâne cu cele patru contracte de azi; reconcilierea
JT-D6 se verifică în schimb pe TOATĂ baza după backfill, ceea ce e o probă pe
205k documente reale. Un al cincilea contract vs 1C (jurnalul lunar == cifrele
sursei) rămâne felie proprie, dacă cifrele o cer.

### JT-D9 — Client: trei ecrane, niciun calcul în TS

`Jurnal cumpărări`, `Jurnal vânzări` (același component, `Sens` ca parametru) și
`Decont TVA`. Perioada în URL, ca la raportare. Zero agregare în client (42c):
totalurile de grilă stau pe `Baza`/`Tva`, care **sunt** aditive — spre deosebire
de soldurile balanței.

---

## Ce NU intră, cu motiv

- **Generarea fișierelor** D300/D394/SAF-T (XML, validări ANAF, versionare per an
  fiscal): proiecte izolate ale utilizatorului, declarate ca atare la 35c. Ce
  garantează felia e că **faptele** lor sunt într-un registru, cu codurile ANAF
  legate de nomenclator.
- **TVA la încasare** (regim + 4428 + transfer la imperechere) — amânat la 36f,
  neatins. Când vine, adaugă `DataExigibilitate` pe rând: aditiv.
- **Pro-rata, ajustările, regularizarea de rotunjire per document × cotă** —
  aceeași listă 36f.

## Riscurile pin-uite (ce va ținti review-ul advers)

1. **Documentele care postează TVA fără să fie „operațiune"** — ITV e prins de
   criteriu; există altele? (conexul, secundarul, latura pereche).
2. **`Capitalizat` cu cotă 0** — împărțire la 1, bază = valoare, TVA = 0: corect,
   dar cota 0 pe un regim capitalizat e și configurație plauzibilă.
3. **Partenerul care nu e partener**: pe `Decont`, `SursaContrapartida` e latura
   predator, adică ANGAJATUL. Jurnalul va arăta titularul, nu comerciantul de pe
   bon — onest față de ce știe modelul, dar trebuie spus.
4. **`SursaContrapartida` care nu e o latură** (`Explicit`/`TipMaterial`) ⇒
   partener null. **Tranșat: se raportează, nu se refuză.** Un refuz ar face
   documentul neoperabil pentru o preocupare care e strict de RAPORTARE, iar
   postarea contabilă a aceluiași rând merge perfect (contrapartida se rezolvă
   din contul fallback). Gaura se măsoară alături de cea din JT-D2.
5. **Backfill × idempotență** pe o bază cu 205k documente, întreruptibilă.
6. **Storno peste graniță de perioadă**: rândul invers cade în altă lună decât
   originalul — jurnalul lunii vechi rămâne cum a fost declarat (corect), dar
   cusătura JT-D6 e per document, deci peste ambele luni.
7. **Perf** pe clona de import: jurnalul unui an, cu join pe partener.
