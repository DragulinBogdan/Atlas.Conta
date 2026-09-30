# Formatul unitar al ecranului de document în XAF Blazor

Stare: **2026-09-30**, după sesiunea de arhitectură cu owner-ul; regula
durabilă e decizia 106 (`docs/decizii/106-regim-pe-stare.md`), acoperirea
curentă în `docs/stare-curenta/`. Fișierele `culegere-*-xaf.md` de aici
(2026-08-06) descriu starea de la GATE XAF, ecran cu ecran; acesta descrie
ce e comun tuturor ecranelor de document și din ce primitive e făcut.

## Cele trei axe

Orice ecran de document e instanța aceleiași anatomii, declarată pe trei axe.
Tipul contribuie doar ce are în plus; nimic nu se rescrie per tip.

| Axa | Ce declară | Primitiva | Unde | Stare |
|---|---|---|---|---|
| 1. Antet | grupurile Antet / Detalii / Stare / Corecție ale ierarhiei; grupul propriu al tipului, nested în Antet | Atlas.DXF `ForHierarchy<Document>().Layout()` + `For<T>().Layout()` (layout autoritar, merge pe ierarhie) | `UI/ContaUiBaseline.LayoutDocumente` | făcută |
| 2. Linii | grila tipizată a liniilor și dialogul liniei; câmpurile-rezultat read-only | `[TipDetaliu]` + `TipDetaliuViewUpdater`; `.Column(Index)` și `AllowEdit=false` pe coloană și pe item | `UI/ContaUiBaseline.<Tip>` | pe indici, per tip (106-r4) |
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
motivul. O comandă nouă intră întâi în `RegimDocument.ComenziCunoscute`,
apoi în hook-ul tipului, apoi în acțiune; ModelCheck refuză o acțiune
`RecordEdit` fără comandă în vocabular.

## Ce folosim și ce nu

- **DevExpress nativ**: Conditional Appearance (culoarea stării, câmpurile
  condiționate de rolul liniei la ASM/LDI) și Validation (`RuleRequiredField`
  pe navigații). StateMachine nu: tranziția e comandă a motorului cu
  registre, nu schimbare de câmp. `DxTreeListEditor` e în pachetul de bază
  și merge pe date plate (Key/ParentKey), doar Client/Queryable; locul lui
  e planul de conturi și D300 (106-r3), nu documentele. `HCategory` nu:
  moștenește `BaseObject` (contra 104e).
- **Atlas.DXF**: layout autoritar, coloane, `RequiredNav`, `ServerMode`,
  navigație, dialoguri. Candidatul de primitivă nouă e axa 2 (106-r4): grilă
  de linii declarată pe roluri, cu blocajele câmpurilor-rezultat puse o
  singură dată.
- **Extern (Llamachant, Reactive.XAF)**: nu. Regulile per tip sunt structură,
  deci cod (invariantul IV); regulile „data driven” ar muta structura în
  bază. Reactive.XAF rămâne sursă de idei (ModelViewInheritance ≈ merge-ul
  pe ierarhie din Atlas.DXF).
