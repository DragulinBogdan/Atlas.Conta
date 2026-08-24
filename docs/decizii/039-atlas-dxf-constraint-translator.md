# Decizia 39 — Pre-polish

- **Data**: 2026-07-23 (primul commit în jurnal)
- **Stare**: activă
- **Rezumat durabil**: `CLAUDE.md` §39

---

**Pre-polish: referințe `Atlas.DXF.Core` + `Atlas.DXF.Blazor` (26.1.3.5)
și traducerea violărilor de constraint DB promovată în bibliotecă.**
(a) **Mecanism nou în Atlas.DXF.EfCore** (`Database/Exceptions/`, scris în
sesiunea asta, review advers cu probe live pe Postgres):
`ConstraintViolationTranslator` (DbUpdateException → FK/unique/not-null/
check; parse per provider cu SQLSTATE **23001 RESTRICT inclus** — cazul FK
Restrict al DSC-ului; direcția FK din evidența providerului, NU din
`Entries` — EF pune TOT batch-ul acolo, batch-urile mixte delete+insert ar
minți) + rezolvare constraint→tipuri CLR pe IModel relational +
`ConstraintViolationMessages` (template-uri publice suprascriibile,
captions XAF cu fallback CLR). Hook Blazor: `AtlasDxfExceptionService`,
înregistrat de `AddAtlasDxfServices()`/`AddAtlasDxfExceptionHandling()`
(XAF folosește TryAddScoped — verificat decompilat; ordinea față de
AddXaf nu rupe).
(b) **Conta consumă**: Module → +`Atlas.DXF.Core` (venea deja tranzitiv
prin EfCore, acum explicit); Blazor.Server → +`Atlas.DXF.Blazor`,
`.AddAtlasDxf()` în lanțul de module, `AddAtlasDxfServices()` +
template-urile în ROMÂNĂ în `Startup.ConfigureServices`. Închide din 38e
itemul „traducerea FK Restrict în mesaj prietenos". ModelCheck verde pe
ambele profiluri; aplicația pornește curat cu modulul.
(c) **Unelte noi disponibile pentru restul polish-ului 38e**: EntityFluent
(layout-uri ListView/DetailView declarative în C#; FĂRĂ auto-discovery —
`UiBaselineDiscovery.Discover(assembly)` manual în `AddGeneratorUpdaters`),
model extenders (`DisplayDetailType`, `NewObjectTarget`/
`ProcessObjectTarget` — candidat pentru „tipul de detaliu declarat per
document"), `CustomizeListViewController`/`GridAutoResizeController`/
`ViewSummaryController`, `SmartLookupPropertyEditor`, atributele
`ForbidCRUD`/`DeactivateAction`, `NavigationPathAttribute`.
