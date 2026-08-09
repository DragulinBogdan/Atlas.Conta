# Pasul 5 — Felia 3: trezoreria prin API (PLT/INC + imperecheri + plata automată) (contract)

Stare: **EXECUTAT (2026-08-09)** — toți cei 5 pași + review advers cu fix-urile
aplicate; închiderea în §Închidere. A treia felie verticală pe șablonul BTR/FCT
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

## Închidere (2026-08-09)

**Fluxul-ancoră complet, verificat în browser pe baza Privat**: FCT nouă cu
secțiunea „Plată automată" (bifă + cont propriu Casa + instrument) → operare →
NIR + **plata autogenerată** în „Documente generate" → link `/plt/{id}` →
plata se deschide COMPLETĂ (laturi, felul beneficiarului dedus prin sonda OData,
linia 628 clonată) → operare (număr `OP-F3-1` din culegere, nu din serie) →
**panoul de stingeri** arată imperecherea automată (Total 242 / Asignat 242 /
Rest 0), stingerea legată de factură cu link. Plată MANUALĂ separată: candidații
de stins filtrați corect pe rol (doar FCT/FCL/DEC pentru trezorerie).
ModelCheck: bugetar 334 / privat 191, verzi.

**Bug de fond găsit la smoke, fixat**: TOATE widget-urile DevExtreme propagau
și schimbarea PROGRAMATICĂ a valorii (seed-ul agregatului), iar un `seteaza`
din closure-ul vechi al contextului ștergea câmpurile abia scrise — plata
autogenerată se deschidea cu laturile/liniile „dispărute". Garda `if (e.event)`
(regula F2 de pe `Lookup`) generalizată pe tot vocabularul de câmpuri; regula
canonică a clientului: formularul e sursa de adevăr, widget-ul raportează
exclusiv acțiunile omului.

**Review advers — niciun defect de fond; fix-urile aplicate**:
- D-6a: candidații de stingere filtrați pe rol compatibil (`tipuriCandidate`
  per felie — filtru pe DataSourceLoader → SQL; refuzul se evită, nu se provoacă);
- `window.confirm` (blocant — exact tipul de dialog care a înghețat renderer-ul
  la smoke) înlocuit cu confirmare INLINE în panou;
- D-7/D-7b: `[XafDisplayName]` pe `TipInstrumentPlata` (label frumos în XAF ȘI
  React, o singură sursă) + dump-ul enum în ordinea de DECLARAȚIE (default-ul
  primul, nu alfabetic);
- D-5a: gardian ModelCheck pe membrii enum (CASE-ul cu fallback ar fi mapat
  tăcut un membru nou);
- D-1a: `EsteSters` prin API public `IObjectSpace.IsObjectToDelete` (fără cast
  la tipul concret).
- **Fix colateral de FOND** (defect pre-existent din spike, scos de smoke):
  sub ștergerea amânată, `EFCoreObjectSpace.IsDeletedObject` e fals la
  `Committing` (GCRecord-ul îl pune interceptorul abia la SavingChanges) — DELETE
  de imperechere era judecat ca EDITARE și refuzat pe orice cale secured,
  inclusiv UI-ul XAF. Reparat cu `EsteSters` în cele 3 ramuri ale gardianului.

**Datorii documentate (minore, nefixate)**: D-2a/D-3a — perf pe baza de import
(`CoduriTip` face GetObjectByKey per copil; `DocumenteCuRest` agregă TOATE
liniile la fiecare încărcare de grilă — de MĂSURAT înainte de release, nu de
optimizat orb); D-5b — editarea manuală a plății autogenerate Draft se pierde
la anularea facturii-sursă (de afișat un indiciu); D-6b — link-ul „Generat din"
hardcodat `/fct/` (corect azi, de generalizat la transferul 581 cu
`DocumentSursaTip`); ordinea `Stingeri` pe Id (arbitrară Guid).
