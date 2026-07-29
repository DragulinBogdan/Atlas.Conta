# Lista React — luptele structurale cu XAF Blazor

Artefactul cerut de decizia 44.2: orice luptă STRUCTURALĂ cu Blazor (async,
dialoguri, refresh, feedback) NU se hack-uiește în XAF — se notează aici și se
rezolvă în clientul React (pasul 5, designul `p5-react-design.md`). Fiecare
item: ce s-a lovit, de ce e structural, cum arată rezolvarea în React.

Itemii se adaugă cu context (ce s-a încercat, de ce nu se fixează în Blazor).
Deschisă la felia GATE XAF (29.07.2026), seed-uită cu ce era deja cunoscut.

## Structurale (motivația migrării — CLAUDE.md §Cunoștințe utilizator)

- **ObjectSpace sincron, fără async nativ** — orice operație lungă blochează
  circuitul Blazor Server; în React: TanStack Query + endpoint-urile motorului
  (42b), UI-ul rămâne viu.
- **Dialoguri**: XAF Blazor nu are un flux natural de dialog compus
  (culegere-în-pași, confirmări cu conținut dinamic); în React: componente de
  dialog proprii per felie (43a).
- **Feedback de progres** (operare lungă, import): fără streaming de stare în
  XAF; în React: stare de mutație + retur `{documentId, stareNoua, mesaje[]}`
  (42b).

## Din felii anterioare

- **Multi-tab staleness pe read-only post-Draft** (40c, limitare asumată):
  `ObjectSpace.Committed` nu se propagă între tab-uri/OS-uri; fix-ul de fond =
  gardian generic de Committing pe server, care în designul 42a e exact
  distincția secured/non-secured a pasului 5.
- **SmartLookupPropertyEditor revertat** (commit `98ce1d0`, memoria
  „smartlookup-fallback-standard"): match-exact-pe-tastare pe nomenclatoare
  mari (plan 1.679, TipMaterial) nu e viabil azi în componenta Atlas.DXF;
  repararea se face în Atlas.DXF separat; în React: `Lookup` pe OData cu
  DefaultProperty emis în dump (43f) acoperă nevoia nativ.
- **Localizarea shell-ului** (butoane, dialoguri framework, navigație — EN):
  localizarea completă XAF e efort disproporționat pentru două ecrane;
  în React: totul e al nostru, captions din metadata (43a/42e).

## Adăugate la GATE XAF

- **Footer de sumar pe grilele de linii** (pas 3, DROP documentat):
  `IModelColumn.Summary` e citit doar de grila WinForms (docs DevExpress);
  în Blazor sumarul cere ViewController pe `DxGridListEditor.GridSummary`
  (`ViewSummaryController` din Atlas.DXF e acțiune interactivă, gated pe
  extender din assembly-ul Blazor). Nevoia gate-ului e acoperită de `Total`
  pe DetailView; în React: footer de agregate nativ în DataGrid (43c).
- **Total pe ListView-urile root** (pas 3): `Total` e `[NotMapped]` peste
  `Detalii` ⇒ N+1 per rând; ascuns din listele FCT/FCL prin baseline (celelalte
  10 tipuri îl mai au — o linie per tip la nevoie). Soluția corectă = coloană
  calculată server-side (proiecție), adică exact modelul de citire al pasului 5
  (42c); nu se cârpește în XAF.
