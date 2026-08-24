# Decizia 42 — Design pasul 5 — tierul API + React

- **Data**: 2026-07-24 (primul commit în jurnal)
- **Stare**: activă
- **Rezumat durabil**: `CLAUDE.md` §42
- **Docs**: docs/api/p5-api-design.md

---

**Design pasul 5 — tierul API + React — FIXAT**
(`docs/api/p5-api-design.md`, toate cele 6 tranșări confirmate
24.07.2026; implementarea urmează). Rafinează deciziile 5–8 pe modelul
stabilizat. Esența: (a) **o singură sursă de reguli** — gardian generic
de Committing în Module, activ DOAR pe ObjectSpace-uri secured (non-Draft
read-only, registre read-only pentru toți, Imperechere prin
ValideazaCreare); distincția secured/non-secured înlocuiește orice token
(non-secured = ușa de sistem); în securitate nimeni nu are Write pe
registre. Închide restanțele 40c/41a/41d. (b) **Motorul rulează în
ObjectSpace non-secured PROPRIU, secvență nu cuib**: faza 1 = culegerea
comisă prin secured OS; faza 2 = comanda prin ID, tranzacția integral a
motorului; puntea = ID în ambele sensuri, retur/erori ca date
(`{documentId, stareNoua, conexId?, mesaje[]}` / 422+erori[]);
DocumentOperareController migrează pe același pattern. (c) **Citirea =
registre + proiecții**: `Module/Proiectii` cu `IQueryable<RandDto>` +
DataSourceLoader (DevExtreme.AspNet.Data); atomii partajați Brut +
unpivot-ul Imperecherii (LINQ simplu, fără LINQKit; join pe agregate, nu
subquery corelat); test de consistență proiecție==StocService/
ImperechereService în ModelCheck; Dimensiuni aplatizate în Select (owned
nu traversează sârma — închide 24 pe citire). TS nu calculează niciodată
sold/rest/total. (d) **Scrierea = agregat per document** (PUT header+
linii, reconciliere server-side, WriteDto≠ReadDto vizibil în tipuri),
organizată în **felii verticale per tip** (Dtos+Apply+Endpoints, în
Module/assembly Api testabil în ModelCheck); importul (site/Tethys) =
alt apelant al aceluiași apply, un document per tranzacție. (e)
**Metadata pe criteriul deciziei 4**: build-time = OpenAPI→TS + captions
prin CaptionHelper (fluxul de localizare XAF); runtime = doar
politici-date; pe resursă = affordances în ReadDto; serializarea
layout-ului MOARE (layout per tip = componentă React). (f) **Host
separat** `Atlas.Conta.WebApi` din scaffold-ul DevExpress (fără
multitenancy — MT rămâne opțiune aditivă), același Module, OData opt-in
DOAR nomenclatoare, JWT + user store comun, updater unul singur, release
ca pereche per client. Datorie parcată împreună: concurență
multi-operator + acoperirea rest per linie (seam: advisory lock 25f).
De verificat la primul spike: atribuirea auditului sub OS non-secured,
formatul UserFriendlyExceptionFilter.
