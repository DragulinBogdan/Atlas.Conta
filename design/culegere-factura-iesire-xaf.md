# FCL — Factura de ieșire, ecranul XAF Blazor (primitive XAF)

Perechea XAF a wireframe-ului React
([culegere-factura-iesire.md](culegere-factura-iesire.md)): **același
document, exprimat în primitivele XAF** — în mare parte starea REALĂ de
după GATE XAF (decizia 53), nu o țintă. Fiecare regiune e adnotată cu
primitiva care o produce; sursele: `ContaUiBaseline.cs`,
`DocumentOperareController.cs`, `FacturaIesireDescarcareController.cs`,
`DefaultTipTvaController.cs`, `DocumentDetaliiEditareController.cs`,
`TipDetaliuViewUpdater` (Module/UI).

Diferența structurală față de React: XAF e **interpretor** — noi declarăm
(layout EntityFluent, atribute, controllere), framework-ul randează. Nu
există „banda de verdict" ca element propriu: verdictul motorului sosește
prin `UserFriendlyException` (alert roșu, buline pe linii — `white-space:
pre-line` în `site.css`) și prin `ShowViewStrategy.ShowMessage` (toast
verde la succes, cu numărul asignat).

## Wireframe — DetailView `FacturaIesire_DetailView`

```
┌────────────┬──────────────────────────────────────────────────────────────────┐
│ NAVIGAȚIE  │ Factura de ieșire · FCL-… — ● Draft                              │
│ (XAF nav)  ├──────────────────────────────────────────────────────────────────┤
│ Documente  │ [💾 Salvează][Salvează și închide]  [Operează][Anulează operarea] │
│  Facturi   │ [Stornează  |06.08.2026|]  [Generează descărcarea ⊘ |06.08.2026|] │
│   ieșire ▸ │  └ ActionContainer-ele toolbar-ului DetailView:                  │
│  NIR       │    Save* = ModificationsController (XAF built-in);               │
│  …         │    Operează/Anulează/Stornează = DocumentOperareController       │
│ Nomencl.   │    (SimpleAction ×2 + ParametrizedAction cu DateTime, Enabled    │
│ Registre   │    ["Stare"] pe Draft/Operat); Generează descărcarea =           │
│            │    FacturaIesireDescarcareController (ParametrizedAction, doar   │
│            │    pe Operat — aici ⊘ pe Draft)                                  │
│            ├──────────────────────────────────────────────────────────────────┤
│            │ ⚠ • Linia de stoc a facturii de ieșire cere produsul — alegeți-l.│
│            │   • Factura de ieșire cu linii de stoc cere gestiunea de descărc.│
│            │  └ OperareException → UserFriendlyException cu buline            │
│            │    (xaf-alert-message, pre-line) — echivalentul benzii de verdict│
│            ├──────────────────────────────────────────────────────────────────┤
│            │ ▾ ANTET            └ .Layout(): Group("Antet") — EntityFluent    │
│            │ ┌─ Document ──────────────────┐ ┌─ Livrare ─────────────────────┐│
│            │ │ Număr        {———}          │ │ Scadență   [ __.__.____ ▦]    ││
│            │ │ Dată         [06.08.2026 ▦] │ │ Gestiune   [▼ Depozit central]││
│            │ │ Predator     [▼ SEDIU     ] │ │ de descărcare                 ││
│            │ │ Primitor     [▼ MEGACORP  ] │ └───────────────────────────────┘│
│            │ └─────────────────────────────┘  └ GrupDocument (ForHierarchy    │
│            │   └ lookup standard (LookupPropertyEditor — SmartLookup          │
│            │     revertat, 40d/53h); Predator/Primitor cu RuleRequiredField   │
│            │     pe NAVIGAȚIE (context Save, mesaje RO — 40b)                 │
│            ├──────────────────────────────────────────────────────────────────┤
│            │ ▾ DETALII          └ Group("GrupDetalii") + colecția agregată    │
│            │ [+ Nou] [🗑 Șterge]     Detalii; [TipDetaliu(typeof(             │
│            │ ┌──────────┬────────┬─────────┬───────┬─────────┬────────┬─────┐│
│            │ │ Produs   │Lot(pin)│Tip(cont)│ Cant. │Preț unit│ Tip TVA│ Val.││
│            │ ├──────────┼────────┼─────────┼───────┼─────────┼────────┼─────┤│
│            │ │ Bec LED  │   —    │ 371 MF  │20,000 │   12,50 │ N21    │250,0││
│            │ │ Transport│        │ 704 VEN │ 1,000 │   80,00 │ N21    │ 80,0││
│            │ └──────────┴────────┴─────────┴───────┴─────────┴────────┴─────┘│
│            │   └ FacturaIesireDetaliu))] → TipDetaliuViewUpdater comută       │
│            │     colecția pe `FacturaIesireDetaliu_ListView` (40a): New       │
│            │     creează DERIVATA, coloanele = schema ei (ordine + FK-uri     │
│            │     ascunse = ContaUiBaseline.FacturaIesire, HideMembers +       │
│            │     .Column(Index)); `Valoare` AllowEdit=false (rezultat, D7);   │
│            │     `Lot` EDITABIL pe FCL (pinul se culege — invers ca pe FCT)   │
│            ├──────────────────────────────────────────────────────────────────┤
│            │ ▾ STARE & TOTALURI └ Group("GrupStare") din layout-ul ierarhiei  │
│            │ Stare {Draft}   Data operare {—}   Document sursă {—}            │
│            │ Autogenerat {nu}                   Total {330,00}                │
│            │   └ Total = proprietate calculată; FĂRĂ notificare live pe       │
│            │     DetailView (restanța din lista React, 53g)                   │
└────────────┴──────────────────────────────────────────────────────────────────┘
```

## Dialogul liniei — popup DetailView `FacturaIesireDetaliu_DetailView`

`[+ Nou]` pe colecția nested deschide dialogul liniei (calea 40a). Înainte
de afișare, `DefaultTipTvaController` (pe `NewObjectViewController
.ObjectCreated` al frame-ului nested) precompletează TipTva din ancora
tipului (`TipDocument.TipTvaImplicit` → N21 privat / CAP21 bugetar);
masterul se citește din `NestedFrame.ViewItem` — back-reference-ul liniei
NU există pre-commit (docs DevExpress 402990/112912).

```
        ┌─ Linie nouă — Factura de ieșire ─────────────────────────┐
        │ Produs        [▼ Bec LED 9W                          🔍] │
        │   └ LookupPropertyEditor pe catalog 312k (remote)        │
        │ Tip (cont)    [▼ 371 — Mărfuri                         ] │
        │   └ RuleRequiredField („Tipul liniei este obligatoriu")  │
        │ Lot (pin)     [▼ L-04412                               ] │
        │   └ opțional; validarea apartenenței/soldului = motorul  │
        │     la operare, NU editorul (lookup-ul XAF nu filtrează  │
        │     pe sold — diferență asumată față de React)           │
        │ Cantitate     [    5,000]   Preț unitar [    12,50]      │
        │ Tip TVA       [▼ N21 21%]   Valoare TVA [    13,13]      │
        │ Valoare       {    62,50}   Descriere   [_____________]  │
        │   └ AllowEdit=false și în dialog (bypass-ul prin         │
        │     row-click e închis de geamănul DetailView — 53/D7)   │
        │                                        [OK]  [Renunță]   │
        └──────────────────────────────────────────────────────────┘
```

Calculul la culegere (`Valoare`/`ValoareTva`/`Total` live) rulează prin
`TvaService.CalculeazaLaCulegere` pe seam-ul de `Committing` (53c) — deci
cifrele se văd **după Save**, nu la tastare. Diferența e structurală
(ObjectSpace sincron, fără notificare de proprietate cross-obiect) și e
exact genul de item care NU se hack-uiește în Blazor, ci moare pe lista
React (44.2).

## Maparea regiune → primitivă

| Regiune | Primitivă XAF | Unde |
|---|---|---|
| Layout antet (Document/Livrare) | EntityFluent `.Layout()` — `Group` nested în `Antet` (Atlas.DXF 26.1.3.9) | `ContaUiBaseline.LayoutDocumente` |
| Etichete RO | `[XafDisplayName]` pe proprietăți (identice ListView/DetailView) | `FacturaIesire.cs` |
| Colecția de linii tipizată | `[TipDetaliu(typeof(FacturaIesireDetaliu))]` + `TipDetaliuViewUpdater` (ModelNodesGeneratorUpdater) | `Module/UI` |
| Ordinea coloanelor, FK ascunse | `.Column(Index)` / `HideMembers` / `HideForeignKeys()` pe ierarhii | `ContaUiBaseline.FacturaIesire` |
| Câmp-rezultat read-only | `IModelCommonMemberViewItem.AllowEdit=false` pe coloană ȘI pe itemul din dialogul liniei | baseline (D7) |
| Default TVA la linie nouă | `NewObjectViewController.ObjectCreated` + `TvaService.AplicaTipTvaImplicit` | `DefaultTipTvaController` |
| Validare de culegere | `RuleRequiredField` (context Save) pe navigații; commit VALIDAT înaintea motorului | 40b + `Executa` |
| Operează / Anulează / Stornează | `SimpleAction` ×2 + `ParametrizedAction(DateTime)` (data stornării — D10), `ConfirmationMessage`, `Enabled["Stare"]` | `DocumentOperareController` |
| Verdictul motorului (erori) | `OperareException` → `UserFriendlyException` cu buline; alert Blazor cu `pre-line` | `Executa` (catch) |
| Verdictul de succes + backorder | `ShowViewStrategy.ShowMessage` (Success, 6s): „Operat. Nr. FCL-…" + `RezumaResturi` | D11 |
| Conexul deschis în editare | `e.ShowViewParameters.CreatedView` (DetailView pe OS nou) — DSC/NIR | pattern 26d/31e |
| Backorder pe FCL operată | `ParametrizedAction(DateTime)` „Generează descărcarea", OS propriu, deleagă la `DescarcareService` | `FacturaIesireDescarcareController` |
| Read-only post-Draft | `View.AllowEdit/New/Delete["Stare"]` pe master + lista nested + DetailView-ul liniei, re-evaluat pe `ObjectSpace.Committed` | `Document(Detalii)EditareController` |
| Lookup-uri | `EditorAliases.LookupPropertyEditor` standard (SmartLookup revertat — 53h) | model |

## Ce NU are ecranul XAF (și de ce rămâne așa)

Diferențele față de wireframe-ul React sunt exact lista React
(`docs/api/lista-react.md`) — se notează, nu se hack-uiesc (regula 44.2):

- **Fără `Verifică` (dry-run)** — validarea motorului rulează doar la
  Operează; dry-run-ul `POST .../valideaza` e al tierului API.
- **Fără calcul la tastare** — TVA/Total se materializează la Save
  (seam de Committing), nu live; `Total` pe DetailView cere refresh.
- **Editor de linie = popup modal**, nu panou lateral cu culegere în serie
  (dialogul XAF se închide la OK; „încă o linie" = New din nou).
- **Lookup-ul de lot nu filtrează pe sold** — apartenența/soldul le spune
  motorul la operare, cu mesajul „întâi transfer (BTR)".
- **Stale multi-tab** — `Committed` nu se propagă între ObjectSpace-uri
  (limitare asumată 40c); fix-ul de fond e gardianul de Committing pe
  server (decizia 42a, la pasul 5).
