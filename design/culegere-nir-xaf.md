# NIR — ecranul XAF Blazor (primitive XAF)

Perechea XAF a wireframe-ului React ([culegere-nir.md](culegere-nir.md)).
Primitivele comune ale shell-ului (acțiunile de operare, gardienii de
read-only, alertul cu buline / toast-ul de succes) sunt documentate în
[culegere-factura-iesire-xaf.md](culegere-factura-iesire-xaf.md) — aici
doar ce e specific NIR-ului. Surse: `DocumenteGestiune.cs` (clasa `NIR`),
`ContaUiBaseline.DetaliuGeneric`, decizia 26 (conexul), 53i (restanța).

Fapt structural: **NIR nu are detaliu derivat, nici `[TipDetaliu]`** —
liniile sunt baza pură `DocumentDetaliu` (testul bazei §6), afișate prin
ListView-ul GENERIC al colecției (`Document_Detalii_ListView`, unul pentru
toată ierarhia). De aici decurg și limitele de culegere de azi.

## Wireframe — DetailView `NIR_DetailView` (cazul frecvent: conexul din FCT)

```
┌────────────┬──────────────────────────────────────────────────────────────────┐
│ NAVIGAȚIE  │ NIR — ● Draft · Autogenerat ☑                                    │
│  NIR ▸     ├──────────────────────────────────────────────────────────────────┤
│            │ [💾 Salvează][Salvează și închide]  [Operează][Anulează operarea] │
│            │ [Stornează |06.08.2026|]                                         │
│            │  └ DocumentOperareController — comun, nimic specific NIR;        │
│            │    NIR-ul conex s-a deschis SINGUR aici după operarea FCT        │
│            │    (e.ShowViewParameters.CreatedView — 26d)                      │
│            ├──────────────────────────────────────────────────────────────────┤
│            │ ▾ ANTET      └ layout-ul IERARHIEI (ForHierarchy<Document>       │
│            │ ┌─ Document ────────────────────┐    .Layout()) — NIR nu are     │
│            │ │ Număr      {———}              │    grup propriu de layout      │
│            │ │ Dată       [06.08.2026 ▦]     │                                │
│            │ │ Predator   [▼ MEGACORP SRL  ] │  ← furnizorul (Partener)       │
│            │ │ Primitor   [▼ Depozit central]│  ← gestiunea primitoare        │
│            │ └───────────────────────────────┘                                │
│            │   └ RuleRequiredField pe navigații (40b); tipul laturii          │
│            │     (Partener / Gestiune) NU se filtrează în lookup — îl         │
│            │     validează motorul la operare                                 │
│            ├──────────────────────────────────────────────────────────────────┤
│            │ ▾ DETALII — ListView-ul GENERIC (Document_Detalii_ListView)      │
│            │ [+ Nou] [🗑 Șterge]                                              │
│            │ ┌───────────┬──────────┬────────┬──────────┬────────┬──────────┐ │
│            │ │ Tip (cont)│ Lot      │  Cant. │  Valoare │ Tip TVA│ Val. TVA │ │
│            │ ├───────────┼──────────┼────────┼──────────┼────────┼──────────┤ │
│            │ │ 371 MF    │ L-04413  │ 20,000 │   212,00 │   —    │     0,00 │ │
│            │ │ 3028 MAT  │ L-04414  │  4,000 │   180,00 │   —    │     0,00 │ │
│            │ └───────────┴──────────┴────────┴──────────┴────────┴──────────┘ │
│            │   └ coloane = ContaUiBaseline.DetaliuGeneric (.Column(Index):    │
│            │     TipMaterial, Lot, Cantitate, Valoare, TipTva, ValoareTva);   │
│            │     TipTva/ValoareTva EXISTĂ pe bază dar NIR-ul nu poartă TVA    │
│            │     (36b: TVA-ul rămâne pe factură; NIR-ul duce netul) — coloane │
│            │     vizibile-dar-goale, prețul detaliului generic                │
│            ├──────────────────────────────────────────────────────────────────┤
│            │ ▾ STARE & TOTALURI                                               │
│            │ Stare {Draft}  Document sursă {FCT 4471 / 04.08} 🔗  Autogenerat ☑│
│            │ Total {392,00}                                                   │
│            │   └ DocumentSursa = navigația de pe bază — link-ul spre FCT      │
│            │     e lookup-ul read-only al XAF-ului, nu un banner dedicat      │
└────────────┴──────────────────────────────────────────────────────────────────┘
```

## Cele două origini, în termeni XAF

### Conex (FCT → NIR) — fluxul care FUNCȚIONEAZĂ azi cap-coadă

1. Operarea FCT (motorul, în tranzacția sursei) creează draftul NIR:
   clonă header cu laturile inversate + liniile cu Natura=Stoc, cu
   `LotId`/`Valoare` incluse — loturile s-au născut la culegerea FACTURII
   (seam-ul `FacturaIntrareLoturiController`, 53a).
2. `DocumentOperareController` îl deschide imediat în editare
   (`ShowViewParameters`); operatorul verifică și apasă Operează.
3. `NIR.PregatesteOperare`: liniile care referă lot STRĂIN (născut pe altă
   linie — cazul conex) își recalculează `Valoare = Cantitate × PretUnitar`
   din lotul finalizat; liniile cu lot propriu păstrează valoarea culeasă.
4. Gardienii de grup: re-operarea FCT regenerează conexul; anularea FCT
   șterge draftul autogenerat; NIR operat blochează anularea sursei.

### Manual — parțial, restanța 53i e vizibilă exact aici

`[+ Nou]` pe colecție creează un `DocumentDetaliu` de BAZĂ, în dialogul
generic — **fără câmp Produs și fără nașterea lotului**: seam-ul de
Committing care naște/sincronizează lotul din produs există azi DOAR pe
FacturaIntrare (`FacturaIntrareLoturiController`). Pe NIR manual, `Lot` e
un lookup editabil pe nomenclatorul de loturi — operatorul ar trebui să
aleagă un lot deja existent în gestiunea primitoare, ceea ce contrazice
„recepția își naște lotul" (25c). Mecanismul D2 e extensibil (un
`NirLoturiController` geamăn + `ProdusId` pe un viitor detaliu derivat NIR
— care oricum vine la DIM-2, 54e); wiring-ul n-a intrat în felia gate-ului.
**Wireframe-ul React descrie ținta; ecranul XAF de azi acoperă complet
doar fluxul conex.**

## Verdictele motorului (mesajele exacte din `NIR.ValideazaOperare`)

- „Predatorul NIR-ului trebuie să fie un partener (furnizor)."
- „Primitorul NIR-ului trebuie să fie o gestiune."
- „Fiecare linie de NIR referă un lot (recepția e pe lot — decizia 13)."
- „Lotul fiecărei linii aparține gestiunii primitoare."
- „Cantitatea recepționată trebuie să fie pozitivă."

Livrate ca `UserFriendlyException` cu buline (calea comună `Executa`).

## Maparea regiune → primitivă (doar ce diferă de FCL)

| Regiune | Primitivă XAF | Notă |
|---|---|---|
| Colecția de linii | ListView-ul GENERIC `Document_Detalii_ListView` (fără `[TipDetaliu]`) | coloane din `DetaliuGeneric`; comun cu BTR/BCS/PLT/INC/RLF/RDC |
| Dialogul liniei | DetailView-ul generic `DocumentDetaliu_DetailView` | fără Produs, fără atribute de lot (nu există pe bază) |
| Valoarea liniei conexe | `NIR.PregatesteOperare` — recalcul din prețul lotului finalizat | valoarea culeasă rămâne doar pe lot propriu |
| Legătura cu factura | `DocumentSursa` (navigație pe bază) + `Autogenerat` (`ModelDefault AllowEdit=False`) | în `GrupStare` din layout-ul ierarhiei |
| Default TVA | `DefaultTipTvaController` — NO-OP pe NIR | ancora NIR nu are `TipTvaImplicit` |

## Ce NU are ecranul XAF (față de wireframe-ul React)

- **Bannerul de proveniență** („⛓ Generat din FCT…") — informația există
  (DocumentSursa/Autogenerat în GrupStare), dar ca rânduri de câmpuri, nu
  ca element dedicat cu acțiune.
- **Bifa efemeră „Recepționat"** per linie — stare de UI fără câmp
  persistat; n-are purtător natural în ListView-ul XAF (o coloană unbound
  ar fi hacking — merge pe lista React).
- **Culegerea pe produs cu nașterea lotului** (NIR manual) — restanța 53i,
  de rezolvat în model (detaliu derivat NIR la DIM-2) + un controller
  geamăn cu `FacturaIntrareLoturiController`, nu în UI-ul de azi.
- **Atribute lot (Dată expirare / Lot fabricație)** — nu există pe detaliul
  de bază; vin odată cu derivatul NIR (54e).
