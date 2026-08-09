# Pasul 5 — Felia 3: trezoreria prin API (PLT/INC + imperecheri + plata automată) (contract)

Stare: **ÎN LUCRU (2026-08-09)**. A treia felie verticală pe șablonul BTR/FCT
(`p5-spike1-contract.md`, `p5-felia-fct-contract.md`). Deblochează STINGERILE
în client: plăți/încasări culese și operate, imperecherile create/șterse din
UI, plata automată a facturii. Rămân amânate (31f): transferul între conturi
proprii (pereche 581), importul extraselor, imperecherea pe poziții.

## Decizii pin-uite (F3-D1…D9)

- **F3-D1. Feliile PLT/INC = un singur nucleu** `Module/Api/Trz/`
  (`TrezorerieDtos.cs` + `TrezorerieApply.cs` generic pe
  `T : DocumentTrezorerie`), două rute (`api/plt`, `api/inc`) cu controllere
  subțiri. WriteDto: `Data`, `PredatorId`, `PrimitorId`, `TipInstrument?`
  (STRING pe sârmă — convenția `Stare`; null → `OrdinPlata`, valoare
  necunoscută → refuz de domeniu), `NumarExtras?`, `DataExtras?`, `Linii[{Id?,
  TipMaterialId!, Valoare!, AngajamentId?, CodEconomicId?, SursaFinantareId?,
  CodFunctionalId?, ProiectId?}]`. **FĂRĂ `Numar`** (PLT/INC AU
  PoliticaNumerotare ⇒ server-owned — invers față de FCT; gardianul îl refuză
  oricum). **`Valoare` e CULEASĂ** (nu există PregatesteOperare pe trezorerie —
  invers față de BTR/FCT); `Cantitate`/`LotId`/`TipTvaId`/`ValoareTva` nu au
  semantică și NU intră. Validare de scară pe `Valoare` (Bani) ca la BTR.
- **F3-D2. Affordances ONESTE transversal**: `PoateAnula`/`PoateStorna` țin
  cont de IMPERECHERI (motorul refuză cu link pe oricare rol —
  `VerificaFaraImperecheri`) în ReadDto-urile FCT, NIR, PLT, INC; helper comun
  (`AreImperecheri`). ReadDto-urile de trezorerie expun și
  `Total`/`Asignat`/`Ramas` (numerele stingerii, calculate de server — TS nu
  calculează nimic). BTR rămâne neatins (laturile Gestiune↔Gestiune nu pot
  purta imperecheri).
- **F3-D3. Imperecherile prin API** (`Module/Api/Trz/ImperechereApply.cs` +
  controller `api/imperecheri`): `POST` body
  `{DocumentStingatorId, DocumentId, Suma}` — rezolvă ENTITĂȚILE prin
  `GetObjectByKey` cu mesaje de domeniu (semnătura serviciului cere entități,
  nu ID-uri — abatere documentată de la 42b, tradusă la graniță) și deleagă la
  `ImperechereService.Imperecheaza` (validează + comite; gardianul re-validează
  la Committing — dubla rulare e benignă, documentată). `DELETE /{id}` — liber
  (31d). `GET /{documentId}/stingeri` → `StingeriDto {DocumentId, Total,
  Asignat, Ramas, Imperecheri[{Id, EsteStingator, CelalaltDocumentId,
  CelalaltTip, CelalaltNumar, Suma, Autogenerat}]}` — un singur apel pentru
  panoul de stingeri. Scrierea/citirea în OS SECURED (autorizarea = securitatea
  XAF + gardianul).
- **F3-D4. Proiecția de REST, restrânsă DELIBERAT**:
  `Module/Proiectii/ImperecheriProiectii.cs` — atomii din design (selectorul
  Brut + unpivot-ul Imperecherii pe ambele laturi ca `Concat`) + proiecția
  `DocumenteCuRest` construită ca UNION de proiecții PER TIP CONCRET
  (FCT/FCL/PLT/INC/DEC — `Tip` ca literal string per ramură; sub TPT nu există
  discriminator). **`ReturClient` EXCLUS deliberat** (override-ul
  `LiniiCreanta` face `GROUP BY`-ul universal să divergă tăcut de
  `ImperechereService.Total` — capcana semnalată de explorare); excluderea se
  documentează în cod. Test de consistență în ModelCheck: proiecția per
  document == `ImperechereService.Ramas` pe aceleași id-uri. Filtrarea pe
  contrapartidă (candidații de stins pentru un PLT/INC) = parametru al
  proiecției/endpoint-ului (`GET /api/proiectii/documente-cu-rest`).
- **F3-D5. `GenereazaPlata` intră în DTO-urile FCT** (ridicarea excluderii
  F2): +`GenereazaPlata`, `PlataContPropriuId` (+`PlataContPropriuDenumire` în
  Read), `PlataNumar`, `PlataData`, `PlataTipInstrument` (string) în
  Write+Read; mapare în Apply. `GenereazaChitanta`/`Chitanta*` rămân excluse
  (moarte). Plata autogenerată apare deja în `Copii[]` cu Tip="PLT" — clientul
  o leagă la `/plt/{id}`. Numărul plății autogenerate vine din `PlataNumar`
  (calea motorului, non-secured — legitim; gardianul refuză doar culegerea
  DIRECTĂ de Numar pe tipuri cu politică).
- **F3-D6. OData opt-in nou**: `ContPropriu` **ReadOnly** (conturile proprii =
  politică — ContImplicit/EsteBanca se administrează în back-office),
  `Angajat` CRUD (nomenclator viu). Restul neatins.
- **F3-D7. TRZ = default de CULEGERE în client** (convenție 31a, nu validare):
  editorul de linie PLT/INC precompletează TipMaterial cu Tipul `Cod == "TRZ"`;
  grila NU filtrează lookup-ul pe TRZ (liniile autogenerate poartă Tipul
  facturii-sursă — 302/628 etc.).
- **F3-D8. Clientul**: felii `plt`/`inc` (listă cu Autogenerat; detaliu:
  ContPropriu lookup local, Partener remote/Angajat local — PLT: predator
  ContPropriu, primitor Partener SAU Angajat (selector de fel + lookup;
  condiționalitate în cod — 43a), TipInstrument din enum-ul metadata,
  NumarExtras/DataExtras, editor de linie cu TRZ precompletat + Valoare +
  clasificația pliată; **panoul „Stingeri"** pe detaliul PLT/INC/FCT: Total/
  Asignat/Rest + lista imperecherilor cu link spre celălalt document + ștergere
  + creare din candidații cu rest ai contrapartidei — după `DELETE` link,
  butonul Anulează redevine activ). FCT: secțiunea „Plata automată" (checkbox +
  câmpuri condiționate în cod) + link-ul PLT din Copii.
- **F3-D9. ModelCheck**: bloc „felia Api Trz" (bugetar, după blocul e2e
  trezorerie ~:3708): plată manuală prin Aplica (Numar refuzat în dto? — nu e
  în dto prin construcție; nr. din serie la operare), refuzurile de laturi via
  dry-run, affordances cu imperecheri (PoateAnula false cu link → true după
  DELETE prin ImperechereApply), StingeriDto pe lanțul avans↔regularizare,
  FCT cu GenereazaPlata prin Apply → plata în Copii → operarea plății prin
  OperareApi → imperecherea automată → consistența `DocumenteCuRest` ==
  `Ramas` per id (inclusiv proba că RDC e EXCLUS din proiecție, cu comentariu).

## Pașii

1. **Module A**: TrezorerieDtos/Apply + extinderea DTO-urilor FCT (F3-D5) +
   check-uri ModelCheck pentru ele.
2. **Module B**: ImperechereApply + StingeriDto + proiecția `DocumenteCuRest`
   (atomii 42c) + affordances oneste transversale (F3-D2) + check-urile de
   imperechere/consistență.
3. **Transport** (WebApi): PltController/IncController/ImperecheriController +
   proiecția + OData (F3-D6) + regenerarea openapi/types.
4. **Clientul** (F3-D8) + regenerarea metadata.
5. **Închidere (main)**: smoke browser pe fluxul plată manuală + stingere +
   plata automată; review advers; fix-uri; docs (decizia 57).

## În afara feliei

Transferul 581 (pereche PLT+INC conexă); importul extraselor; imperecherea pe
poziții; NTC prin API (stingătorul-notă rămâne pe calea XAF; `ValideazaCreare`
îl acceptă deja dacă vine); includerea RDC în proiecția de rest (cere soluție
pentru `LiniiCreanta` polimorf în SQL — documentată, nu improvizată).
