# Pasul 5 — Spike 1: fir complet subțire API+React pe NotaTransfer (contract)

Stare: **ÎN LUCRU (2026-08-08)**. Scope confirmat de utilizator: fir complet
subțire (host WebApi + gardian + felie verticală BTR + client React minimal cu
codegen), tip pilot **NotaTransfer** (cel mai simplu — focusul rămâne pe
plumbing, nu pe specificul tipului). Designurile de bază: `p5-api-design.md`
(decizia 42), `p5-react-design.md` (decizia 43) — neatinse; spike-ul le probează.

## Decizii pin-uite (D1–D12)

- **D1. Topologia feliei**: DTO-uri + Apply în `Module/Api/Btr/`, proiecțiile în
  `Module/Proiectii/` (LINQ pur, fără ASP.NET — testabile în ModelCheck);
  endpoint-urile = controllere MVC subțiri în host (`WebApi/API/Conta/`).
  Mutarea într-un assembly `Api` frate = când feliile se înmulțesc (permis de 42d).
- **D2. Contractul comenzilor**: `OperareRezultat { DocumentId, StareNoua,
  ConexId?, Mesaje[] }` construit de adaptorul `OperareApi` (Module) peste
  `MotorOperare` (semnături neschimbate; motorul întoarce entitate, adaptorul
  face date). Erorile gardienilor = `422 { Erori: [] }` — `OperareException.Message`
  spart pe `\n` în controller. **Verificarea empirică §8 „formatul filtrului DX"
  e ÎNCHISĂ din surse**: `UserFriendlyExceptionFilter` = `ContentResult` text
  brut, 400 (403 pe security) — inutilizabil pentru contract; traducem noi.
- **D3. Dry-run `POST .../valideaza`**: `MotorOperare.Valideaza(os, doc)` prin
  extracția fazelor calculează+validează într-un helper privat comun cu
  `Opereaza` (33d le-a ordonat deja); ZERO schimbare de comportament pe
  `Opereaza`; dry-run rulează în OS non-secured propriu, aruncat, fără commit.
- **D4. Gardianul de Committing (42a)**: activ DOAR pe OS-uri secured; refuză
  (a) modificarea/ștergerea `Document`/`DocumentDetaliu` când documentul
  (valoarea ORIGINALĂ a stării) ≠ Draft + orice scriere pe `Stare` din secured,
  (b) orice create/update/delete pe `RegistruStoc`/`RegistruContabil`,
  (c) `Imperechere`: New → `ValideazaCreare`, Edit → refuz, Delete liber —
  logica migrează din `ImperechereController.OnCommitting` (controllerul
  păstrează doar capabilitățile UX). **Seam-ul se alege cu probă pe sursele
  DX**: `XafApplication.ObjectSpaceCreated` NU acoperă OS-urile din
  `IObjectSpaceFactory` scoped (WebApi) — se caută seam care acoperă ambele
  host-uri (candidate: `IObjectSpaceCustomizerService`, înregistrare de
  serviciu în modul); detectarea „secured" verificată pe surse. Căile standalone
  (ModelCheck/Import1C/Migrare pe `EFCoreObjectSpaceProvider`) rămân NEATINSE.
- **D5. `DocumentOperareController` migrează pe 42b**: commit culegere (cu
  validare) → `OperareApi` în OS non-secured (`INonSecuredObjectSpaceFactory`
  din DI) → refresh/redeschidere prin ID. Aceeași cale ca API-ul.
- **D6. Host hardening**: connection string aliniat (5444, postgres/postgres,
  `Atlas.Conta.BackOffice.Privat`); `DisableUpdateSchema = true` pe provider +
  ștergerea blocului DEBUG `UpdateDatabaseAlways` (updater-ul rămâne exclusiv pe
  calea Blazor `--updateDatabase` — 42f); paritate de configurare module cu
  Blazor (AuditTrail EF Core, Dashboards/Notifications/Office/Scheduler/
  StateMachine/ViewVariants/ConditionalAppearance/FileAttachments cu opțiunile
  lor); bootstrap-ul convenției de rotunjire (`Scara`/`SetareProfil`) extras în
  helper partajat în Module și apelat din AMBELE `Program.cs`. FĂRĂ CORS (dev =
  Vite proxy server-side; prod = same-host, 43e).
- **D7. OData opt-in**: DOAR `Gestiune`, `TipMaterial`, `Lot` (nomenclatoarele
  feliei). Nimic din `Document*`/`Registru*` (42f).
- **D8. DTO-urile BTR**: WriteDto = `Data, PredatorId, PrimitorId, NumarPV?,
  DataPV?, Linii[{ Id?, TipMaterialId, LotId, Cantitate }]` (fără `Valoare` —
  server-owned; fără TVA/Angajament — fără semantică pe BTR). ReadDto adaugă
  `Id, Numar, Stare, DataOperare, Total, Autogenerat, DocumentSursaId`,
  `Valoare` + eticheta lotului per linie, și affordances
  `PoateEdita/PoateOpera/PoateAnula/PoateStorna` (din Stare — sursele existente
  ale controller-elor). JSON PascalCase (scaffold-ul setează
  `PropertyNamingPolicy = null` — se păstrează). PUT = agregat, reconciliere
  server-side (upsert linii pe Id, delete pe dispărute), refuz curat pe non-Draft.
- **D9. Proiecția spike**: sold stoc per `Lot × Repartitor × TipStoc`
  (+ etichete produs/gestiune, cantitate și valoare) prin `DataSourceLoader`
  (pachet nou `DevExtreme.AspNet.Data`); test de consistență în ModelCheck:
  proiecția per cheie == `StocService.Sold` (după blocul e2e 3b).
- **D10. Codegen spike**: `openapi.json` luat din host-ul viu (script comis) →
  `openapi-typescript`; captions/enums/DefaultProperty prin REFLECȚIE în
  ModelCheck (`--dump-metadata`, emite `Client/src/generated/metadata.json`).
  Limită documentată: diff-urile xafml NU intră (CaptionHelper cere
  XafApplication — ModelCheck nu are; calea completă amânată). Artefactele
  generate se COMIT; drift verificat de ModelCheck (43d).
- **D11. Clientul**: `nou/Atlas.Conta.Client` (Vite + React + TS, pnpm);
  DevExtreme React (DataGrid + CustomStore remote; SelectBox + ODataStore
  pentru `Lookup`); formular: se evaluează **React Hook Form** la primul
  `DocumentShell`/`CampShell` (criteriul 43 §7: array-ul de linii +
  `useCampMeta` trebuie să rămână naturale), fallback hand-rolled pe context —
  verdictul se raportează la închidere. JWT în sessionStorage + header
  `Authorization`. Ecrane: login, listă BTR (remote), detaliu BTR
  (DocumentShell: header + editor de linie + grid readonly + comenzi pe
  affordances + afișarea `Erori[]`), sold stoc (proiecția).
- **D12. Verificarea empirică rămasă**: atribuirea auditului sub OS non-secured
  (operare prin API cu JWT → rândurile `AuditData` au user?) — se verifică la
  smoke și se raportează. (A doua verificare §8 — formatul filtrului — închisă
  la D2.)

## Pașii (un agent per pas, main verifică independent + commit per pas)

1. **Host hardening + OData opt-in** (D6, D7): WebApi pornește curat pe baza
   dev, warmup OK, Swagger sus, token obtenabil, `$metadata` conține exact cele
   3 nomenclatoare.
2. **Module: gardian + Valideaza + OperareApi + migrarea controller-ului**
   (D2–D5): build verde, ModelCheck verde AMBELE profiluri (căile standalone
   neatinse), Blazor pornește; smoke operare în UI la pasul 5.
3. **Felia BTR + proiecția + ModelCheck** (D1, D8, D9): ModelCheck verde cu
   check-urile noi (apply e2e + consistența proiecției), endpoint-urile răspund
   (smoke cu JWT).
4. **Codegen + clientul React** (D10, D11): `pnpm build` verde, fluxul complet
   în dev (login → listă → draft → PUT → valideaza → opereaza → storno).
5. **Închidere (main)**: smoke e2e în browser, verificarea D12 în `AuditData`,
   review advers dedicat + fix-uri, actualizarea docs/CLAUDE.md.

## Explicit ÎN AFARA spike-ului

Restul tipurilor de document; captions prin CaptionHelper/XafApplication;
assembly `Api` separat; CORS/deploy static (prod same-host vine la felia de
release); concurența multi-operator (parcată, 42 §8); MultiTenancy; importul ca
API; localizarea completă a clientului.
