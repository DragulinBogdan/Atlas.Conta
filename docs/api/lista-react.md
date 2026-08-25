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
- **`Total` pe DetailView nu se reîmprospătează după salvarea unei linii**
  (smoke pas 4): rămâne 0 până la o re-citire a documentului (după operare apare
  corect: 1.210). Cauza e structurală — proprietatea `[NotMapped]` nu notifică,
  iar XAF nu re-evaluează editorul la commit-ul colecției nested; „fix"-ul în
  Blazor ar fi un refresh manual de ViewItem la fiecare commit de linie (fragil,
  și tot nu acoperă editarea inline). În React: totalul e stare derivată din
  agregatul de formular, recalculată la fiecare schimbare (43c). Workaround
  acceptat pentru gate: valorile per linie (Valoare / Valoare TVA) SE văd live,
  iar totalul e corect imediat după operare.
- **AuditTrail EF Core e incompatibil cu owned types** (smoke pas 4, dezactivat
  în `Startup.cs`): `AuditTrailService.GetKeyAsObject` citește PK-ul prin
  reflecție CLR, iar owned entity types au PK shadow ⇒ NRE la orice SaveChanges
  care atinge o linie de document (surse 26.1.3, AuditTrailService.cs:505-517;
  apelul precede filtrarea pe tip, deci nu e configurabil). Dacă apare cerință
  reală de audit: fie ticket la DevExpress, fie audit propriu pe registre
  (append-only, deja istoricul contabil), fie la pasul 5 în tierul API.

## Adăugate la felia 5 (NIR scriere)

- **`window.confirm` la ștergerea unui draft** (`FctDetaliu`/`NirDetaliu`):
  inconsecvent cu confirmarea inline aleasă la felia 3 pentru panoul de stingeri
  (57f). Dincolo de inconsecvență, dialogul nativ blochează întregul renderer —
  s-a văzut la smoke-ul feliei 5, unde tab-ul a rămas nefolosibil. În React
  soluția e aceeași ca la stingeri: confirmare inline în shell, nu dialog de
  browser.

## Adăugate la felia 15 (partener + ANAF)

- **Ecranul de partener în React** (72-r9): prima felie de nomenclator din
  client. Ce trebuie să aibă, din ce a fixat felia: grupul de adresă cu lookup
  pe `Judet` (OData, `ForbidCRUD`) activ DOAR când `Tara == RO` (gardianul
  refuză altfel — afordanță, nu validare, 65); `DataSincronizareAnaf` și
  `InactivFiscal` afișate readonly (server-owned); butonul „Sincronizează din
  ANAF" = `POST api/parteneri/{id}/sincronizeaza-anaf` cu rezultatul ca listă
  (`Modificari` cu vechi/nou, `Diferente`, `Avertismente`) și `suprascrie` ca
  opțiune explicită, confirmată inline (57f); 503 = „ANAF n-a răspuns, reia".
- **Acțiunea XAF e sincronă și blochează ~5 s la 500 selectați** (72-r5): în
  React comanda de lot e async natural, cu progres per tranșă.
