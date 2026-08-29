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

## Închise de felia 20 (decizia 77)

Itemii de mai jos au fost rezolvați și textul lor original trăiește în
`docs/decizii/077-p5-felia20-finisaj-client.md` + contractul feliei:

- `window.confirm` la ștergerea draftului (F5, F19) → `ConfirmareInline` + slot
  în `DocumentShell`; zero `window.confirm` în cod.
- Ecranul de partener (72-r9), `Societate` (73-r6), `CodNc` + UM pe produs
  (73-r6) → `felii/nomenclatoare/`. `PoliticaMiscareSaft` (74-r12) → grilă de
  citire; editarea din React = 77-r3.
- `/saft` `Neincluse` plafonat la 200 (73-r10/74-r7) → agregat per cauză pe
  server, exemple ≤ 20; S3 → link la fișa contului (`ContId`).
- Căutarea sensibilă la diacritice (F19) → coloana generată `Cautare` (77a);
  rămân în afara ei `CodFiscal`/`Iban`/`Marca` (77-r4).
- `Lookup` care refetchează eticheta (F19) → `nucleu/odata.ts`, `byKey` prin
  cache pe `(entitate, id, proiecție)`; a închis de la sine și placeholder-ul pe
  linia existentă (restanța F6 din LDI+BCS).
- Etichetele la precompletare (F19) → `nucleu/etichete.ts`; BTR rămâne 77-r1.
- Smoke-ul vizual al jurnalelor de TVA și al decontului (datoria F12) — văzute.

## Rămase (nestructurale)

- **BTR n-a adoptat convenția 61b** (etichete per poziție, precompletare) —
  77-r1, plumbing de felie.
- **`displayExpr` de nucleu pentru `TipMaterial`** (ASM arată cod + denumire,
  BCS doar denumire) — 77-r6.
- **Cele 8 dimensiuni n-au UI** pe balanță/fișă (pass-through din URL).
- **`AngajamentId` fără lookup** (tabela e goală).
- **`HeaderFilter` trunchiat la 100 de valori** (cauza e server-side).
