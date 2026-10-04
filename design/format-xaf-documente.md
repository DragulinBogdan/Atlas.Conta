# Formatul unitar al ecranului de document în XAF Blazor

Stare: **2026-10-01** (axa 2 făcută, 106h); prima scriere 2026-09-30, după
sesiunea de arhitectură cu owner-ul; regula durabilă e decizia 106 (`docs/decizii/106-regim-pe-stare.md`), acoperirea
curentă în `docs/stare-curenta/`. Fișierele `culegere-*-xaf.md` de aici
(2026-08-06) descriu starea de la GATE XAF, ecran cu ecran; acesta descrie
ce e comun tuturor ecranelor de document și din ce primitive e făcut.

## Cele trei axe

Orice ecran de document e instanța aceleiași anatomii, declarată pe trei axe.
Tipul contribuie doar ce are în plus; nimic nu se rescrie per tip.

| Axa | Ce declară | Primitiva | Unde | Stare |
|---|---|---|---|---|
| 1. Antet | grupurile Antet / Detalii / Stare / Corecție ale ierarhiei; grupul propriu al tipului, nested în Antet | Atlas.DXF `ForHierarchy<Document>().Layout()` + `For<T>().Layout()` (layout autoritar, merge pe ierarhie) | `UI/ContaUiBaseline.LayoutDocumente` | făcută |
| 2. Linii | vocabularul rolurilor liniei o dată pe ierarhie; tipul spune ce roluri poartă; rezultatele motorului `ReadOnly` pe grilă și pe dialog | `[TipDetaliu]` + `TipDetaliuViewUpdater`; Atlas.DXF `ForHierarchy<DocumentDetaliu>().Columns(sloturi)` + `For<T>().Columns(...).ReadOnly(...)` (26.1.4.10) | `UI/ContaUiBaseline.LiniiPeRoluri` + `<Tip>` | făcută (106h) |
| 3. Regim pe stare | ce se editează și ce comenzi sunt disponibile în starea curentă, cu motiv | `Api/RegimDocument` (L2) randat de `RegimDocumentController` (XAF) și `Api/*Apply` (React) | `Api/RegimDocument.cs`, `Controllers/RegimDocumentController.cs` | făcută (106) |

## Axa 3, regimul pe stare

Regimul e o valoare: `{ Stare, Editabil, Comenzi: comandă → motiv sau null }`.
Baza dă regula ierarhiei, tipul contribuie prin `Document.ContribuieRegim`.

```
Draft    editabil   Opereaza ✓  Valideaza ✓  Sterge ✓   AnuleazaOperarea ✗  Storneaza ✗  Corecteaza ✗
Operat   read-only  Opereaza ✗  Valideaza ✗  Sterge ✗   AnuleazaOperarea ?  Storneaza ?  Corecteaza ?
Stornat  read-only  toate ✗ „Documentul e stornat.”
```

`?` = disponibilă dacă gardienii tac: Anulează cere perioada înregistrării
deschisă și niciun dependent (latură pereche operată, conex operat,
împerechere); Stornează și Corectează cer doar lipsa dependenților. Motivele
sunt textele gardienilor, nu reformulări.

Comenzile proprii ale tipurilor, în același vocabular:

| Tip | Comandă | Condiția |
|---|---|---|
| FCL | GenereazaDescarcarea | Operat, gestiune de descărcare aleasă, rest nedescărcat > 0 |
| AMO, ITV | Regenereaza | Draft |
| ASM | Distribuie | Draft, cel puțin o linie de consum și una de produs |
| NTC | Stinge | Operat |

Convenția XAF: ID-ul acțiunii se termină cu numele comenzii
(`Document.Opereaza`, `FacturaIesire.GenereazaDescarcarea`). Gardianul
potrivește sufixul cu vocabularul și pune `Enabled["Regim"]` și tooltip-ul cu
motivul. Ștergerea standard XAF (`Delete`) e legată explicit de `Sterge`.
O comandă nouă intră întâi în `RegimDocument.ComenziCunoscute`,
apoi în hook-ul tipului, apoi în acțiune; ModelCheck refuză o acțiune
`RecordEdit` fără comandă în vocabular.

## Axa 2, liniile pe roluri

O grilă de linii nu se mai scrie pe indici. Ierarhia `DocumentDetaliu` declară
o dată **vocabularul rolurilor**, în ordinea de culegere; fiecare tip spune
doar ce roluri poartă (umple un slot cu membrii lui) și ce nu poartă (`Drop`).
Un rol al bazei pe care tipul nu-l poartă se ascunde pe grila lui; un membru
fără rol vine la coadă, în ordinea generată, ca o proprietate nouă să nu
dispară. Excepțiile pe view (o legendă, o coloană ascunsă pe o singură grilă,
grila DVI) rămân view-scoped și câștigă în fața rolurilor.

| Rol (slot) | Baza `DocumentDetaliu` | Cine îl umple altfel |
|---|---|---|
| Directie | — | LDI/ASM `Directie`; PIF/CAS `Fel` |
| Identitate | `TipMaterial` | FCT/NIR/FCL/LDI/ASM `Produs, TipMaterial`; DEC `TipMaterial, Descriere`; DSC `LinieSursa, TipMaterial`; NTC `Descriere` + perechea de conturi și repartitori (linia E postarea); PIF `Imobilizare, TipMaterial`; CAS/AMO `Imobilizare` |
| Provenienta | — | PIF `LinieSursa` |
| Unitate | `Lot` | DEC/NTC/TRZ/PIF/CAS/AMO: `Drop` |
| Cantitate | `Cantitate` | NTC/TRZ/PIF/CAS/AMO: `Drop` |
| Pret | — | FCT/NIR/FCL/DEC `PretUnitar`; LDI/ASM `PretEvaluare` |
| Tva | `TipTva, ValoareTva` | NIR/LDI/DSC/NTC/ASM/PIF/CAS/AMO: `Drop` |
| Valori | `Valoare` | PIF `+ ValoareFiscala`; AMO `+ ValoareFiscala, ValoareDeductibila` |
| AtributeLot | — | FCT/NIR/LDI/ASM `DataExpirare, LotFabricatie` |
| Conturi | — | DEC/CAS conturi + repartitori (+ `CodEconomic`); AMO conturi, `RepartitorDebit`, `CentruCost`, `CodEconomic` |
| Parametri | — | FCT `CodCpv`; FCL `Descriere`; PIF parametrii amortizării; AMO `Luni` |

**Rezultatul motorului e blocat pe ambele suprafețe, printr-o singură
declarație.** `Valoare` e `ReadOnly` acolo unde `BazaLinie` o calculează și
`Valoare` nu e intrare (FCT, NIR, FCL, LDI, DEC, DSC, ASM); `Lot` e `ReadOnly`
acolo unde îl naște mecanismul necondiționat (FCT, NIR). Pe DEC și ASM
blocajul e nou (înainte `Valoare` era editabilă și rescrisă la operare fără
mesaj); pe FCL blocajul ajunge acum și pe dialogul liniei. NTC și DVI culeg
`Valoare`, deci n-o blochează; TRZ n-are `BazaLinie`.

**Ce s-a normalizat față de grilele pe indici** (aceeași formă pe toate
tipurile): `Valoare` după coloanele de TVA pe grila generică; `Produs`
înaintea lui `TipMaterial` pe LDI/ASM și `TipMaterial` înaintea lui `Lot` pe
FCL; `Fel` primul pe PIF/CAS; conturile după `Valoare` pe CAS; linia de
trezorerie primește forma bazei fără lot și cantitate (înainte n-avea
baseline, dimensiunile veneau primele).

**Limită numită (106-r6).** BCS, BTR și RLF calculează `Valoare` (FIFO;
RDC o culege), dar stau pe grila generică `Document_Detalii_ListView` și pe dialogul
`DocumentDetaliu_DetailView`, pe care DVI îl folosește cu `Valoare` culeasă;
blocajul lor cere detaliu propriu, nu un `AllowEdit` pe view.

**Proba.** ModelCheck, pe modelul real (`ProbeLinii`, 106h-1…3): fiecare grilă
tipizată începe cu coloanele rolurilor în ordinea vocabularului și ascunde
rolurile nepurtate; fiecare membru `ReadOnly` e blocat pe grilă ȘI pe dialog;
pe fiecare tip cu detaliu propriu care calculează `Valoare`, `Valoare` e
`ReadOnly`.

**Primitiva** e în Atlas.DXF (`Views/docs/COLUMN-SLOTS.md`): `Columns(c =>
c.Slot(id, membri…) / Drop(id))` pe `For<T>()` și `ForHierarchy<T>()`, compus
de `ColumnsResolver` (baza deține ordinea, cea mai derivată declarație
deține conținutul slotului); `ReadOnly(membri…)` pune `AllowEdit=false` pe
coloana ListView-ului și pe itemul DetailView-ului, pe tip și derivate.
Lookup-urile nu sunt atinse.

## Ce folosim și ce nu

- **DevExpress nativ**: Conditional Appearance (culoarea stării, câmpurile
  condiționate de rolul liniei la ASM/LDI) și Validation (`RuleRequiredField`
  pe navigații). StateMachine nu: tranziția e comandă a motorului cu
  registre, nu schimbare de câmp. `DxTreeListEditor` e în pachetul de bază
  și merge pe date plate (Key/ParentKey), doar Client/Queryable; locul lui
  e planul de conturi și D300 (106-r3), nu documentele. `HCategory` nu:
  moștenește `BaseObject` (contra 104e).
- **Atlas.DXF**: layout autoritar, coloane pe sloturi (roluri) cu `ReadOnly`
  pe ambele suprafețe (axa 2, 106h), `RequiredNav`, `ServerMode`, navigație,
  dialoguri.
- **Extern (Llamachant, Reactive.XAF)**: nu. Regulile per tip sunt structură,
  deci cod (invariantul IV); regulile „data driven” ar muta structura în
  bază. Reactive.XAF rămâne sursă de idei (ModelViewInheritance ≈ merge-ul
  pe ierarhie din Atlas.DXF).
