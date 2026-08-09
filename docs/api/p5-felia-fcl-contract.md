# Pasul 5 — Felia 4: FCL + descărcarea de gestiune prin API (contract)

Stare: **ÎN LUCRU (2026-08-09)**. A patra felie verticală pe șablonul consolidat
(`p5-spike1-contract.md`, `p5-felia-fct-contract.md`, `p5-felia-trz-contract.md`).
Completează fluxul de VÂNZARE în client: factura de ieșire culeasă și operată,
descărcarea de gestiune generată (automat la operare + manual pe backorder),
DSC-urile citite și operate, stingerea cu încasările (panoul există din F3).

Premise verificate pe cod (explorare + spot-check main):
- FCL e COMPLET wired în XAF (culegere cu TVA, validări, acțiunea „Generează
  descărcarea") — spre deosebire de FCT la felia 2, nu e nevoie de nicio
  extracție de serviciu; `FacturaIesireApply` urmează direct șablonul FCT.
- `DescarcareService.Genereaza`/`RestNedescarcat` sunt deja PUBLICE (proiectate
  ca puncte de intrare la P2, design §2.2) — felia doar le expune.
- FCL e DEJA în proiecția de rest `DocumenteCuRest` (ramura a 2-a din UNION) —
  nimic de adăugat; clientul o consumă.
- `PoliticaTva` FCL există în seed-ul privat (4111 = 4427 per linie,
  `ProfilPrivat.SeedPoliticiTva`) — nimic de adăugat.
- Dimensiunea-frunză pe `FacturaIesireDetaliu`/`DescarcareGestiuneDetaliu` =
  DOAR `CodEconomicId` (DIM-2).
- `Genereaza` setează `Autogenerat = true` + `DocumentSursa`
  (`DescarcareService.cs:142-143`) — câmpuri SERVER-OWNED păzite de
  `GardianEditare` ⇒ comanda de generare trebuie să meargă pe ușa NON-secured.

## Decizii pin-uite (F4-D1…D10)

- **F4-D1. `FacturaIesireApply` pe șablonul FCT** (`Module/Api/Fcl/` —
  `FacturaIesireDtos.cs` + `FacturaIesireApply.cs`), cu diferențele de
  contract: **FĂRĂ `Numar` în WriteDto** (FCL ARE PoliticaNumerotare „FCL-",
  serie fiscală ⇒ server-owned, ca BTR/TRZ — invers față de FCT). Header:
  `Data`, `PredatorId` (emitent), `PrimitorId` (client), `DataScadenta?`
  (opțional — politica +30 la operare dacă necules), `GestiuneDescarcareId?`.
  Linii: `{Id?, TipMaterialId!, ProdusId?, LotId?, Descriere?, Cantitate,
  PretUnitar, TipTvaId?, ValoareTva?, CodEconomicId?}`. Regulile TVA identice
  cu F2 (o singură sursă): `AplicaTipTvaImplicit` doar pe linii NOI fără
  TipTva în payload; `CalculeazaLaCulegere` CONDIȚIONAT de declanșatori
  (`noua || bazaNoua != bazaVeche || TipTvaId schimbat`); override `ValoareTva`
  acceptat doar pe regimuri cu TVA separat și non-negativ; `Valoare`
  server-owned. Gard de scară pe Cantitate/PretUnitar/ValoareTva ca la FCT.
  **FĂRĂ `LoturiCulegereService`** — FCL nu naște loturi (pinul REFERĂ loturi
  existente); `Sterge` = pre-check Draft + delete agregat, fără CurataOrfane.
- **F4-D2. DSC prin API = citire + comenzi** (șablonul NIR — `Module/Api/Dsc/`,
  `DscDtos.cs` + `DscApply.cs` doar `Citeste`/`Lista`): DSC-ul se NAȘTE exclusiv
  prin `DescarcareService` (hook la operarea FCL + comanda manuală F4-D3);
  POST/PUT/DELETE manual = felie viitoare (ecranul XAF acoperă azi cazul
  manual). ReadDto linii: `{Id, TipMaterialId+Denumire, LinieSursaId?, LotId,
  EtichetaLot, Cantitate, Valoare (cost), CodEconomicId}`; header cu
  `DocumentSursaId` (+ link client spre `/fcl/{id}`), `Autogenerat`,
  affordances. Comenzile `opereaza/anuleaza/storneaza/valideaza` prin
  `ComandaAutorizata` + `OperareApi` (generice, nimic nou în motor).
- **F4-D3. Generarea manuală = comandă pe ruta FCL, ușa NON-secured**:
  `POST /api/fcl/{id}/genereaza-descarcare` body `{Data}` →
  `ComandaAutorizata` (gate pe OS secured: 404/403/`CanWrite`) → OS
  NON-secured → punct de intrare NOU în Module
  `FacturaIesireApply.GenereazaDescarcare(os, id, data)` (verifică Operat +
  refuz de domeniu altfel; deleagă la `DescarcareService.Genereaza`, comite,
  întoarce `{DscId?, Resturi[]}` — `DscId` null = nimic de generat).
  Ușa secured NU e o opțiune: serviciul scrie `Autogenerat`/`DocumentSursa`
  (server-owned, gardianul le-ar refuza). **De verificat la smoke**: acțiunea
  XAF UI („Generează descărcarea", OS propriu din `Application.CreateObjectSpace`
  — familia secured) mai trece de gardian post-spike-1? Dacă nu → migrare pe
  pattern-ul `DocumentOperareController` (fix colateral, în felie).
- **F4-D4. Restul nedescărcat expus + affordance onestă**:
  `GET /api/fcl/{id}/rest-nedescarcat` → rânduri per linie de stoc
  `{LinieId, ProdusId, ProdusDenumire, LotId?, Cantitate, Acoperit, Rest}`
  (proiecția `RestNedescarcat` existentă, tradusă în DTO). Pe FCL ReadDto:
  `PoateGeneraDescarcare` (Operat && `GestiuneDescarcareId` setată && Σ rest
  > 0 — calculată server-side, clientul nu re-derivă); `Copii[]` include
  DSC-urile (generic, `ApiProiectii.Copii` — perf O(n) rămâne datoria D-2a).
- **F4-D5. OData: NIMIC nou** — toate lookup-urile clientului există deja
  (Partener/Produs/TipMaterial/Gestiune CRUD; TipTva/CodEconomic/Lot ReadOnly).
- **F4-D6. Pinul de lot în client**: `Lookup` pe OData `Lot` filtrat pe
  `ProdusId` (activ doar cu produs ales — condiționalitate în cod, 43a), cu
  `$expand=Produs` și eticheta compusă client-side (oglinda
  `ApiProiectii.EtichetaLot`; `Eticheta` e `[NotMapped]` și nu trece prin
  OData — restanța 40d rămâne pe sârmă, workaround documentat). Validarea de
  sold („întâi BTR") rămâne EXCLUSIV a motorului — clientul o vede prin
  dry-run/422.
- **F4-D7. Clientul, felia `fcl`**: listă (Stare, client, Total, scadență);
  detaliu pe șablonul FCT — header (Partener remote, GestiuneDescarcare
  local, DataScadenta), editor de linie (Produs remote cu `laSelectie` →
  TipMaterial precompletat — update funcțional; pin Lot F4-D6; TipTva local;
  CodEconomic; Cantitate/PretUnitar; override ValoareTva cu `tvaAtins` local
  ca la FCT); secțiunea „Descărcare" pe documentul Operat: tabelul restului
  nedescărcat + butonul „Generează descărcarea" cu dată (default azi, activ
  din `PoateGeneraDescarcare`) + DSC-urile din `Copii[]` cu link;
  `PanouStingeri` (rol 'este-stins', `tipuriCandidate` = PLT/INC). `rutaTip`
  extins cu `FCL → /fcl`, `DSC → /dsc`.
- **F4-D8. Clientul, felia `dsc`**: listă + detaliu READ-ONLY (grilă de linii
  cu eticheta lotului și costul; fără editor de linie) + comenzile din
  affordances + link „Generat din" spre `/fcl/{id}` prin `rutaTip` (nu
  hardcodat — închide forma D-6b pentru DSC).
- **F4-D9. ModelCheck: bloc „Api FCL + DSC" în suita PRIVAT** (descărcarea și
  TVA-ul colectat trăiesc la privat; la bugetar FCL⊘Stoc prin PoliticaValidare
  și DSC e tip inert — blocul nu are sens acolo). Scenariul, cu curățenie pe
  marcaj propriu: sold de stoc pregătit (lot + registru, pattern-ul P2) → FCL
  prin `Aplica` (linie stoc cu pin + linie stoc FIFO + linie serviciu; TVA
  N21) → semantica TVA (retenție override / cedare la schimbarea bazei) →
  dry-run → `Opereaza` → DSC autogenerat în `Copii[]`, liniile lui corecte
  (LinieSursa, lot pin respectat, cost = preț lot × cantitate, CodEconomic
  clonat) → operarea DSC-ului prin comenzi → backorder: FCL cu cantitate >
  sold → rest raportat de `RestNedescarcat` → `GenereazaDescarcare` manuală
  după suplimentarea stocului → affordances oneste (PoateAnula ține cont de
  DSC operat; PoateGeneraDescarcare) → consistența rest == proiecție →
  anulare/storno pe lanț. AMBELE suite rămân verzi.
- **F4-D10. În afara feliei**: scrierea manuală DSC prin API; RDC în proiecția
  de rest (nerezolvat, documentat în F3); regenerarea automată a descărcării la
  recepția NIR (37g); transferul 581; comenzile/importul FCL din site;
  măsurarea perf a proiecțiilor pe baza de import (datoria D-2a/D-3a, înainte
  de release).

## Pașii

1. **Module A**: `Fcl/FacturaIesireDtos.cs` + `FacturaIesireApply.cs`
   (F4-D1) + blocul ModelCheck Api FCL (partea de culegere/TVA/operare din
   F4-D9). Suitele bugetar + privat verzi.
2. **Module B**: `Dsc/DscDtos.cs` + `DscApply.cs` (F4-D2) +
   `GenereazaDescarcare` + `RestNedescarcat` DTO + affordance (F4-D3/D4) +
   restul blocului ModelCheck (descărcare/backorder/consistență). Suitele verzi.
3. **Transport** (WebApi): `FclController` + `DscController` (rute, comenzi,
   genereaza-descarcare, rest-nedescarcat) + regenerarea openapi/types + drift.
4. **Clientul** (F4-D6/D7/D8) + regenerarea metadata.
5. **Închidere (main)**: smoke browser pe fluxul vânzare (FCL nouă → linii cu
   pin → operare → DSC în Copii → operarea DSC → stingere cu INC) + verificarea
   acțiunii XAF „Generează descărcarea" contra gardianului (F4-D3); review
   advers; fix-uri; docs (decizia 58).

## Închidere

(de completat la final)
