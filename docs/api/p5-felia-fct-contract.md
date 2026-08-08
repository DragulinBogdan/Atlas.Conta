# Pasul 5 — Felia 2: FacturaIntrare + conex-NIR prin API (contract)

Stare: **EXECUTAT (2026-08-08)** — toți cei 5 pași + review advers cu fix-urile
aplicate; închiderea în §Închidere. A doua felie verticală pe șablonul BTR
(`p5-spike1-contract.md` — șablonul + datoriile lui). Exersează exact ce BTR nu
a atins: **lotul născut la culegere, TVA la culegere, dimensiunile pe frunză,
mecanismul conex, numărul cules (nu din serie)**. Scope confirmat de utilizator:
NIR = citire + comenzi (POST/PUT pe NIR + `ProdusId` pe `NirDetaliu` = felie
separată, aditivă); GenereazaPlata/Plata* NU intră în WriteDto (la felia
trezoreriei).

## Decizii pin-uite (F2-D1…D9)

- **F2-D1. Extracția `SincronizeazaLoturi(IObjectSpace, FacturaIntrare)`** din
  `FacturaIntrareLoturiController.OnCommitting` (verificat: corpul folosește
  DOAR `IObjectSpace` + documentul; `CancelEventArgs` e ignorat) — O SINGURĂ
  logică de naștere/sincronizare/curățenie a loturilor, apelată din (a)
  controllerul XAF (comportament identic) și (b) `FacturaIntrareApply` înainte
  de commit. Fără ea felia API e non-funcțională: pe tierul API nu rulează
  niciun ViewController (nici loturile, nici `DefaultTipTva`, nici
  `RecalculValoriCulegere`, nici `DocumentDefaults`). Echivalentele refolosite
  direct în Apply: `TvaService.AplicaTipTvaImplicit` (linii noi fără TipTva),
  `TvaService.CalculeazaLaCulegere` (Valoare/ValoareTva la culegere — GATE 53c).
  `Sterge(os, id)` în Apply apelează curățenia loturilor orfane (echivalentul
  `DocumenteLoturiCuratenieController`).
- **F2-D2. FCT WriteDto**: header `Numar!` (FCT poartă numărul FURNIZORULUI —
  fără PoliticaNumerotare; diferență de contract față de BTR, confirmată în 3
  locuri), `Data`, `PredatorId` (Partener), `PrimitorId` (Gestiune),
  `DataScadenta?`, `NumarPV?`, `DataPV?`, `CodCpv?`, `Valuta?`, `Curs?`.
  EXCLUSE: `GenereazaPlata`/`Plata*` (felia trezoreriei), `Chitanta*`/`TethysId`
  (moarte/import). Linia: `Id?`, `TipMaterialId!`, `ProdusId?`, `Cantitate`,
  `PretUnitar`, `TipTvaId?`, `ValoareTva?` (null = calculul standard; valoare =
  override-ul operatorului, aplicat DUPĂ `CalculeazaLaCulegere` — oglinda
  fluxului UI), `DataExpirare?`, `LotFabricatie?`, `CodCpv?`, `AngajamentId?`,
  dimensiunile frunzei `CodEconomicId?`/`SursaFinantareId?`/`CodFunctionalId?`/
  `ProiectId?`. FĂRĂ `LotId` (server-owned pe FCT — 53a) și FĂRĂ `Valoare`.
  Precompletarea Tipului din Produs = UX de client (OData Produs expune
  `TipMaterialId`), NU magie în Apply — `TipMaterialId` rămâne obligatoriu.
- **F2-D3. NIR API = `Citeste`/`Lista`/comenzi** (opereaza/anuleaza/storneaza/
  valideaza prin gate-ul F1). Fluxul-ancoră: FCT operat → `ConexId` → clientul
  deschide `/nir/{id}` → Operează. Fără POST/PUT/DELETE pe NIR în felia asta.
- **F2-D4. OData opt-in extins**: `Partener`, `Produs` (CRUD — nomenclatoare
  vii), `TipTva`, `CodEconomic`, `SursaFinantare`, `CodFunctional`, `Proiect`
  (toate **ReadOnly** — politici/nomenclatoare administrate în back-office;
  clientul doar citește). `Lot` rămâne ReadOnly (spike M1).
- **F2-D5. ReadDto FCT** = oglinda Write + server-owned (`Stare`, `DataOperare`,
  `Total` brut, `Autogenerat`, `DocumentSursaId`, affordances) + per linie
  `LotId`/`LotEticheta` (inclusiv starea „(în culegere)"), `Valoare`,
  `ValoareTva`, denumirile (TipMaterial, Produs, TipTva cu Cota) și Cod-urile
  dimensiunilor + **`Copii[] {Id, Tip, Numar, Stare, Autogenerat}`** (query pe
  `DocumentSursaId`) — link-ul UI spre NIR-ul generat. NIR ReadDto analog
  (frunza NIR are aceleași 4 dimensiuni).
- **F2-D6. Blocul ModelCheck nou (bugetar, după blocul e2e FCT existent)**
  oglindește 3c prin Apply: lotul se NAȘTE din `ProdusId` la `Aplica` (nu
  `CreeazaLot` manual — proba diferenței față de blocurile vechi), TVA
  materializat la culegere, override `ValoareTva` păstrat, reconcilierea
  sincronizează lotul (schimb de produs), ștergerea liniei/documentului curăță
  lotul în culegere, opereaza → NIR conex cu dimensiunile prin contract →
  opereaza NIR → registre, gardienii de grup, `Citeste.Copii`.
- **F2-D7. Driftul openapi (datoria M2 a spike-ului)**: se încearcă generarea
  FĂRĂ host viu (`swagger tofile` pe assembly-ul WebApi) + script de drift în
  client (`pnpm verifica:drift` — regenerare + diff contra comisului). Regulă
  de oprire: dacă generarea offline pică pe warmup-ul XAF, rămâne dump-ul
  manual + datoria re-documentată.
- **F2-D8. Clientul**: felia `fct` (listă + detaliu cu editor de linie: Produs
  remote cu precompletarea Tipului, TipMaterial/TipTva local, dimensiunile într-o
  secțiune „Clasificație bugetară" opțională; valorile Valoare/ValoareTva/Total
  apar DUPĂ Salvează — calcul de domeniu, serverul e autorul) + felia `nir`
  (listă + detaliu read-only cu comenzi); după operarea FCT, rezultatul arată
  link „Deschide NIR". Polish din datoriile spike-ului: `CampData` la data
  stornării (moare `window.prompt`), 401 din store-uri → logout+login.
- **F2-D9. Captions**: `[XafDisplayName]` pe `NotaTransfer.NumarPV/DataPV`
  (restanța smoke-ului spike) + regenerarea `metadata.json`.

## Pașii

1. **Extracția loturilor** (Module, behavior-neutral): `SincronizeazaLoturi` +
   curățenia ca serviciu; controllerele devin adaptori; ModelCheck + build verzi
   fără nicio schimbare de aserții.
2. **Apply FCT + NIR citire + ModelCheck** (Module): F2-D2/D3/D5/D6/D9.
3. **Transport + OData + drift openapi** (WebApi): controllere FCT/NIR pe
   șablonul BTR (gate F1 moștenit din `ContaApiController`), F2-D4, F2-D7.
4. **Clientul** (F2-D8) + regenerarea artefactelor.
5. **Închidere (main)**: smoke browser pe fluxul FCT→NIR complet (baza Privat),
   review advers, fix-uri, docs (decizia 56).

## În afara feliei

POST/PUT/DELETE pe NIR + `ProdusId` pe `NirDetaliu`; GenereazaPlata prin API
(felia trezoreriei: PLT/INC + imperecheri); proiecții noi de raportare; restul
datoriilor de client din spike (licența DevExtreme — acțiunea utilizatorului).

## Închidere (2026-08-08)

**Fluxul-ancoră complet, verificat în browser pe baza Privat**: FCT nouă
(SMOKE-F2-1, furnizor remote din 129k, gestiune, linie de stoc cu produs din
312k) → salvare (Total 60,50 — net 50 + N21 aplicat implicit de server) →
dry-run verde → operare → panoul „Documente generate" cu link → NIR conex
(Total 50 = NETUL, 36b) → **NIR-17815 operat** (numărul din serie la operare)
→ lotul finalizat la NET (12,50) cu +4/50 în gestiune → anularea lanțului →
ștergere. ModelCheck: bugetar + privat verzi (inclusiv blocul privat NOU de
semantică TVA pe N21).

**Datoria M2 a spike-ului ÎNCHISĂ**: `openapi.json` se generează OFFLINE
(`swagger tofile`, fără host/Postgres — HostedService-urile nu pornesc; probat
byte-identic cu dump-ul din host viu); `pnpm gen:openapi` + `verifica:drift`.

**Constatări de fond fixate pe parcurs** (nu erau în contract):
- Recalculul TVA la culegere e CONDIȚIONAT de declanșatorii din UI (baza/
  TipTva schimbate) — altfel override-ul murea la orice PUT de header.
- `NirApply.Citeste` citește liniile pe BAZA detaliului cu frunza prin
  as-cast (TPT LEFT JOIN) — NIR-urile istorice (linii pre-DIM-2) nu mai ies
  cu `Linii: []`.
- Trei bug-uri de nucleu client, găsite la smoke și fixate: cheile OData sunt
  OBIECTE `Guid` (comparațiile normalizate); `laSelectie` = notificare +
  update funcțional în felie; `onValueChanged` propagă DOAR schimbările cu
  `e.event` (schimbarea programatică re-raporta prin closure vechi și ștergea
  câmpurile abia scrise).

**Review advers — niciun defect de FOND; fixate**: override TVA refuzat pe
regimuri fără TVA separat + negativ (D1/D7); re-parentarea... nu — Tipul mutat
pe ne-stoc rupe referința la lotul finalizat (D3); lotul finalizat fără urme
moare la Sterge (D4); affordances oneste — `PoateAnula/Storna` încorporează
copiii operați, NIR `PoateEdita=false` (D5).

**Datorii documentate**: override explicit 0 moare la operare (condiția 36a
`!= 0`) și renunțarea la override cere declanșator — fix-ul de fond = flag
`TvaSuprascris` pe frunză, aditiv, dacă nevoia devine reală (D2/D6, comentate
în DTO); `Partener.ContImplicit` scriibil prin OData (politică de contare —
aceeași putere ca back-office-ul; member-permission dacă se cere, D8);
overload-urile OData cu `tenantName` nu trec prin customizer (irelevant fără
MT); linia de tip BAZĂ pe drafturi vechi e invizibilă în `Linii` dar
contribuie la `Total` și moare la prima reconciliere (deliberat, comentat).
