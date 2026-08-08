# Pasul 5 — API + React: designul tierului Web API (design)

Stare: **FIXAT (24.07.2026) — sesiune de explorare arhitecturală; toate cele 6
tranșări confirmate; implementarea urmează (decizia 42 în CLAUDE.md).**
*Anotare 24.07.2026: mandatul din §8 „sesiune separată — structura aplicației
React" e închis: `p5-react-design.md` (decizia 43).*
Contextul: deciziile vechi 5–8 (XAF Web API, endpoint per tip, DTO plate,
metadata-driven UI) au fost formulate pre-registre și pre-EntityFluent —
sesiunea le-a rafinat pe modelul stabilizat post-P2/polish (40/41). Scopul: o
singură sursă de reguli, motorul izolat tranzacțional, suprafață de citire pe
registre, scriere pe agregat, metadata pe criteriul „structură=cod,
politică=date".

## 1. Fapte pe care stă designul (cod la zi)

- **Motorul e UI-agnostic**: clase statice pe `IObjectSpace` + entități
  (`MotorOperare.Opereaza` — `Motor/MotorOperare.cs:21`; `DescarcareService`,
  `ImperechereService`, `StocService`). `DocumentOperareController` e wrapper
  subțire și comite culegerea ÎNAINTE de motor (40b).
- **Invarianții de editare trăiesc azi exclusiv în controllere Blazor**:
  read-only post-Draft (40c), `ForbidCRUD` pe registre (41a — „consumator
  non-Blazor primește doar hiding-ul"), validarea imperecherii la New (41d).
  O a doua ușă (Web API) i-ar ocoli complet.
- **Motorul modifică el însuși documente non-Draft** în tranzacția proprie:
  `Storneaza` (Operat→Stornat, `MotorOperare.cs:448,491`), `AnuleazaOperarea`
  (Operat→Draft, :438), cu `os.CommitChanges()` propriu (:276/440/492).
- **`INonSecuredObjectSpaceFactory` + `IObjectSpaceFactory` sunt scoped,
  frați în DI**, în orice host `AddXaf` (surse DX:
  `DevExpress.ExpressApp\Services\Core\StartupExtensions.cs:77-79`).
- **`AddXafWebApi` = XafApplication headless**: modulele se înregistrează ca
  în Blazor, dar fără Frames/Views — controllerele UI din Module nu se
  activează. `UserFriendlyExceptionFilter` e în pipeline
  (`DevExpress.ExpressApp.WebApi\Services\StartupExtensions.cs:82`); OData
  generic e **opt-in per tip** (`GenericControllerFeatureProvider`);
  `api/Localization` există pe `ICaptionHelperProvider` (per-item).
- **Precedent non-UI**: ModelCheck bootstrapează `EFCoreObjectSpaceProvider`
  standalone (`tools/ModelCheck/Program.cs:69-81`).

## 2. Tranșarea 1 — o singură sursă de reguli: gardianul de Committing

- Gardian generic în **Module** (abonare la crearea ObjectSpace-urilor, nu
  ViewController — în host-ul WebApi nu există view-uri), activ **doar pe
  ObjectSpace-uri secured**: document non-Draft = read-only, registrele =
  read-only pentru oricine, `Imperechere` doar prin
  `ImperechereService.ValideazaCreare`.
- **Distincția secured/non-secured înlocuiește orice token de exceptare**:
  non-secured = ușa de sistem (motor, seed, Migrare, ModelCheck) —
  utilizatorii nu primesc niciodată una. Respins: gardian „deștept" per-câmp
  care știe tranzițiile legitime — mașinărie care se decalibrează la fiecare
  felie.
- Defense in depth în securitatea XAF: **nimeni nu are Write/Create pe
  `RegistruStoc`/`RegistruContabil`** — motorul nici nu trece prin ea.
- Închide trei restanțe: fix-ul de fond 40c (stale multi-tab pe comenzi),
  enforcement non-Blazor 41a, invarianții imperecherii la ușa a doua 41d.

## 3. Tranșarea 2 — motorul în tranzacția lui: secvență, nu cuib

- **Respins** sandwich-ul `[secured begin → (motor: OS propriu, commit) →
  secured commit]`: (a) citire stale — OS-ul motorului nu vede culegerea
  necomisă a părintelui fără cross-boundary transaction; (b) fereastră ruptă
  — motor comis + părinte picat nu se mai desface.
- **Forma fixată**: faza 1 = culegerea prin secured OS (validare + gardian),
  commit; faza 2 = comanda primește **ID**, motorul rulează în
  **ObjectSpace non-secured propriu** (`INonSecuredObjectSpaceFactory` din
  DI), încarcă proaspăt, un singur `CommitChanges`. Legitimitatea graniței:
  „Draft salvat + operare refuzată" e stare de domeniu validă, nu stare ruptă
  — nu e nevoie de tranzacție distribuită.
- **Puntea e ID-ul în ambele sensuri**: retur = date pure
  `{documentId, stareNoua, conexId?, mesaje[]}` (mesajele includ restul
  nedescărcat — 38d); erorile gardienilor = date (`422 + erori[]`), excepția
  se traduce O DATĂ la graniță (filtrul UserFriendly există în scaffold; de
  verificat formatul listă vs string concatenat).
- **Ambele uși identice**: în WebApi fazele sunt natural request-uri separate
  (`PUT` draft, apoi `POST .../opereaza`); `DocumentOperareController`
  (Blazor) migrează pe același pattern — commit culegere → motor în OS
  non-secured → `Refresh`/reload prin ID (conexul se redeschide prin ID).
- **De verificat empiric la primul spike**: atribuirea utilizatorului în
  audit (`WithAuditedDbContext`) sub OS non-secured — ipoteza: da, identitatea
  vine din scope-ul request-ului (`UserNameProvider` scoped,
  `StartupExtensions.cs:74`), nu din secured-ness.

## 4. Tranșarea 3 — citirea: registre + proiecții

- **Regula de aur: orice număr cu sens de domeniu vine dintr-o proiecție pe
  server. TypeScript-ul nu calculează niciodată sold, rest, total.**
- `Module/Proiectii/`: funcții `IObjectSpace → IQueryable<RandDto>` (rânduri
  plate: sold pe gestiune, fișă de cont, rest de plată, disponibil per lot).
  Endpoint-ul minimal API aplică `DataSourceLoader`
  (**DevExtreme.AspNet.Data**) pe queryable — filter/sort/page/group
  server-side cu vocabularul DTO-ului, nu schema entităților.
- **Semantică comună fără expression-tree machinery** — atomii se partajează
  cu LINQ simplu: (a) selectorul **Brut** `d => d.Valoare + d.ValoareTva` ca
  `Expression` static dat direct lui `Queryable.Sum`; (b) **unpivot-ul
  Imperecherii** `IQueryable<{DocId, Suma}>` (Concat pe ambele laturi) —
  sursa comună pentru `Asignat` scalar și proiecțiile de rest. Forma
  proiecțiilor = **join pe agregate**, nu subquery corelat (evită LINQKit și
  SQL prost deopotrivă). La Sold nu există ce partaja: semnul e în date
  (28a), formula e `Sum(Cantitate)`.
- **Testul de consistență în ModelCheck** = arbitrul anti-drift: proiecția
  agregată per cheie == `StocService.Sold` / `ImperechereService.Ramas` pe
  aceeași cheie. Rulează azi, fără host.
- `Dimensiuni` se aplatizează explicit în `Select` — entitatea/owned-ul nu
  traversează niciodată sârma; închide restanța deciziei 24 („owned la Web
  API — de validat") pe partea de citire, prin construcție.

## 5. Tranșarea 4 — scrierea: agregat per document, felii verticale

- **Draftul se salvează ca agregat**: `GET`/`PUT` per tip pe headerul + toate
  liniile (DTO plate, FK explicite — decizia 7 neatinsă); serverul
  reconciliază în secured OS (upsert linii după ID, delete pe dispărute), un
  commit. Respins CRUD per linie: unitatea de consistență e documentul;
  starea intermediară incompletă trăiește în state-ul React, necomisă.
- **`WriteDto` ≠ `ReadDto`, vizibil în tipuri**: câmpurile stăpânite de server
  (Stare, Autogenerat, Valoare materializată, LotId finalizat) nu există în
  WriteDto — nu „ignore" în mapper.
- **Efectele de culegere rămân server-side, deterministe**: `CreeazaLot` pe
  liniile de stoc fără lot la apply; default-ul TipTva = metadata spre React
  (precompletare) + fallback pe server la apply.
- **Felia verticală per tip = unitatea de organizare a API-ului**:
  `<Tip>Dtos.cs` + `<Tip>Apply.cs` + `<Tip>Endpoints.cs` (comenzi incluse).
  Trăiește în Module (sau assembly `Api` frate referit de host și ModelCheck)
  — apply-ul e testabil e2e în ModelCheck fără HTTP; host-ul = doar wiring.
  Partajat în afara feliilor: motorul, proiecțiile, gardianul, helper-ul
  generic de reconciliere linii.
- **Importul = alt apelant al aceluiași apply** (o singură ușă de scriere per
  tip): endpoint-ul de import traduce formatul sursei (site, Tethys) în
  WriteDto și-l împinge prin apply, **un document per tranzacție**; „bulk" =
  buclă de documente izolate cu raport per document (297 comise + 3 respinse
  cu erori, nu tranzacție mega-payload).
- **Concurență draft**: doar refuzul PUT pe non-Draft (îl dă gardianul).
  Datoria dublă „multi-operator + acoperirea rest per linie" se parchează
  **împreună** (aceeași carte contabilă implicită pe linii); seam pregătit:
  advisory lock Postgres per cheie (25f) — nimic din design nu le blochează.

## 6. Tranșarea 5 — metadata pe criteriul deciziei 4

Criteriul de scope: *schimbarea cere release?* → cod, călătorește
**build-time**; e date? → **runtime**; depinde de stare/utilizator? → **pe
resursă**. Decizia 8 veche se sparge în:

- **Build-time (codegen)**: tipurile OpenAPI→TS + validările simple GRATIS
  din schemă (required/maxLength/nullable din `RuleRequiredField` etc.) +
  **captions/enums exportate prin fluxul de localizare XAF**
  (`CaptionHelper`/`ICaptionHelperProvider` — pas de codegen sau endpoint
  bulk mic; endpoint-urile per-item `api/Localization` rămân escape hatch).
  Override client-side pe captions = hack asumat, cu prag declarat de
  re-evaluare (traduceri sau alt mecanism dacă devine persistent).
- **Runtime (politici-date, editabile fără release)**: `TipTvaImplicit`,
  `DimensiuniObligatorii` per cont, `PoliticaValidare`, nomenclatorul TipTva.
  Lookups nu-s metadata: GET-uri normale pe proiecții de nomenclatoare.
- **Pe resursă (affordances)**: `poateOpera`/`poateStorna`/`poateEdita` în
  ReadDto, calculate din Stare + gardieni + permisiuni — React vede
  consecințele securității, nu modelul ei.
- **Moare serializarea layout-ului** (DetailView, coloane, editor types):
  layout-ul per tip = componentă React în felia tipului. Un renderer generic
  de Application Model în TS ar re-implementa XAF-ul în React — exact
  platforma de ale cărei limitări fugim. Modelul rămâne UNUL (C#); Blazor și
  React au fiecare baseline-ul lor declarativ de prezentare.
- Plusul la model: disciplina forțează semanticile UI-ish rămase prin
  controllere/diffs să-și declare natura (atribut sau tabelă de politică) —
  metadata ca test de puritate, cum a fost testul bazei pentru câmpuri.
- Frâna asumată: costul vizibil al canalului runtime e sănătos — anti-EAV.

## 7. Tranșarea 6 — topologia: host separat

- **`Atlas.Conta.WebApi` = proces separat**, generat cu **scaffold-ul
  DevExpress** (fără multitenancy) și customizat pe loc — nu reinventăm
  roata; Blazor.Server rămâne back-office neatins; ambele peste același
  Module.
- **OData generic opt-in DOAR pe nomenclatoare** — documentele și registrele
  NU se înregistrează (prima decizie a sesiunii, exprimată ca listă de
  tipuri, nu disciplină). Disciplina de la drop-in: lista de tipuri din
  scaffold se taie la nomenclatoare din prima zi.
- **Auth**: JWT pe API (scaffold standard), cookie pe Blazor, același user
  store (aceeași bază, aceiași ApplicationUser + roluri); CORS doar pe API.
- **Updater unul singur** (calea existentă `--updateDatabase` din Blazor sau
  migrator dedicat); host-ul API nu updatează niciodată schema.
- **Release ca pereche per client** (schema comună = cuplaj de versiune —
  regulă de livrare, nu infrastructură). Tenancy bază-per-client (35d) merge
  ca deployment-per-client; **MultiTenancy = opțiune aditivă parcată**
  (pachetul `DevExpress.ExpressApp.MultiTenancy.WebApi.EFCore` pin-uit deja
  în `Directory.Packages.props:40`; modulul MT se montează în ambele tipuri
  de host când/dacă apare nevoia de single-deployment).

## 8. Amânate / de verificat (documentate, nu uitate)

- **Verificări empirice la primul spike**: atribuirea auditului sub OS
  non-secured (§3); formatul `UserFriendlyExceptionFilter` (`erori[]` vs
  string concatenat).
  *Anotare 2026-08-08 (spike 1 — `p5-spike1-contract.md`): AMBELE ÎNCHISE.
  Auditul sub OS non-secured atribuie corect utilizatorul din scope-ul
  request-ului (verificat pe baza vie). Filtrul DX = text brut 400/403 —
  traducerea `422 {Erori[]}` se face în controllerele noastre, înaintea lui.
  Constatare nouă de fond: validarea XAF (RuleRequiredField) NU rulează pe
  acest tier (PersistenceValidationController e per-View) — apply-ul fiecărei
  felii rezolvă FK-urile cu mesaje de domeniu, gardianul de Committing e plasa.
  Amendament la §2/§3 din review-ul advers al spike-ului: distincția
  secured/non-secured răspunde la „cum scrie motorul", NU la „cine are voie să-l
  cheme" — comenzile poartă gate de autorizare (CanWrite pe documentul rezolvat
  prin OS secured) înaintea ușii non-secured, în ambele host-uri.*
- **Datoria dublă parcată împreună**: concurență multi-operator + acoperirea
  rest per linie (advisory lock per cheie — 25f; optimistic concurrency pe
  draft aditiv).
- **MultiTenancy single-deployment** (pachetul există, nimic nu-l blochează).
- **Registrul general read-only pe bază** (decizia 6, opțional) — doar dacă
  apare nevoia; nicio interogare polimorfă pe Document în fluxuri calde
  (35d).
- **Sesiune separată**: structura aplicației React (state management,
  pipeline-ul de codegen OpenAPI→TS + captions, componente per felie).
