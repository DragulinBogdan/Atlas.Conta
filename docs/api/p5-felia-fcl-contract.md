# Pasul 5 — Felia 4: FCL + descărcarea de gestiune prin API (contract)

Stare: **EXECUTAT (2026-08-09)** — toți cei 5 pași + review advers cu fix-urile
aplicate; închiderea în §Închidere. A patra felie verticală pe șablonul consolidat
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

## Închidere (2026-08-09)

**Fluxul-ancoră complet, verificat în browser pe baza Privat** (clona de import:
129k parteneri, 312k produse, 66k chei de stoc): FCL nouă — emitent din lookup-ul
`UnitateInterna`, client remote, gestiunea MAGAZIN — cu DOUĂ linii pe același
produs (una cu PIN de lot prin lookup-ul OData filtrat pe produs + `$expand=Produs`,
una FIFO peste sold: 2.000 cerute / 1.391 disponibile) → Creează (TVA N21 aplicat
implicit de server: Total 48.642 brut) → Operează → **FCL-1** (serie fiscală
server-owned) + scadența +30 din politică + **DSC-ul conex în aceeași tranzacție**
→ secțiunea „Descărcare" arată acoperirea la bucată (pin 10/10; FIFO 1.381/2.000,
**rest 619**) → DSC-1 deschis pe `/dsc/{id}` (cost 21.283,56 — decuplat de prețul
de vânzare; „Generat din FCL-1") → operat → note verificate ÎN BAZĂ la cent
(FCL-1: 4111=707 net + 4111=4427 TVA; DSC-1: 607=371; stoc −1.391) → affordances
oneste (PoateAnula/Storna false pe copil operat; PoateGeneraDescarcare true pe
rest) → comanda manuală fără stoc → `DscId null` + restul raportat → **stingerea
din panoul FCL** (candidații filtrați pe rol+contrapartidă: 1.239 INC/PLT din
25.077 documente cu rest; confirmare inline; Asignat 689 / Rest 47.953, candidatul
stins iese din grilă). Recepție reală FCT→NIR (+50 în MAGAZIN) → **acțiunea XAF
„Generează descărcarea" pe Blazor creează draftul** (proba empirică a fixului D1).
ModelCheck final: bugetar verde, privat verde (56 check-uri noi FCL+DSC), ambele
rulate independent de main.

**Review advers — 2 defecte de fond, fixate de main:**
- **D1 (exact riscul pin-uit în F4-D3)**: acțiunea XAF „Generează descărcarea" era
  RUPTĂ de la spike-1 — `Application.CreateObjectSpace` = familia secured, iar
  serviciul scrie `Autogenerat`/`DocumentSursa` (server-owned) ⇒ gardianul refuza
  commit-ul. Fix: migrarea pe secvența `DocumentOperareController` (gate CanWrite →
  `INonSecuredObjectSpaceFactory` din `Application.ServiceProvider` →
  `FacturaIesireApply.GenereazaDescarcare` → DSC-ul deschis prin ID pe OS de View
  propriu). Probat empiric în Blazor pe baza de import.
- **D2**: plafonul de acoperire per linie-sursă nu se valida la operarea DSC —
  dublura de generare concurentă sau DSC-ul manual suprapus materializa cost dublu.
  Fix în `DescarcareGestiune.ValideazaOperare`: Σ(operat pe alte documente) +
  liniile proprii ≤ cantitatea facturată, per linie-sursă. Contra realității
  MATERIALIZATE, nu a drafturilor străine (anti-dublarea drafturilor rămâne a
  generatorului — `RestNedescarcat`): într-o cursă primul operat câștigă, al doilea
  pică zgomotos. Check ModelCheck dedicat (DSC manual 8 peste 13 operați din 20 →
  refuz). Concurența multi-operator pe commit rămâne parcată (42/25f).
- **M1** (fixat): indiciul editorului de linie mințea la golirea override-ului de
  TVA (golirea NU anulează suprascrierea — payload-ul fără ValoareTva = „nu m-am
  pronunțat"); indiciul spune acum adevărul și arată calea (re-atinge un declanșator).

**Constatări minore, documentate, nefixate**: mesajul „nimic de generat + rest" al
comenzii manuale nu se afișează în client după recitire (cosmetic — API-ul întoarce
corect); etichetele liniilor sunt goale în grilă pe documentul NESALVAT (se
populează la prima recitire; pattern moștenit din FCT); **M2** — DELETE pe un draft
FCL ale cărui linii sunt referite de `LinieSursaId` dintr-un DSC manual → 500 brut
(DbUpdateException netradusă în WebApi; cale exotică — traducerea 39a există doar
în Blazor); **M3** — refuzurile „Id duplicat în payload" și „linie de tip bază" nu
au check ModelCheck (căile există, verificate manual la review). Constatare de
mediu: pe baza de import stocul „Sediul central" stă pe `UnitateInterna` (nu pe o
gestiune) — fapt al importului, nu al feliei. Bonus smoke: ListView-ul
`RegistruContabil` în XAF (restanța DIM-4) s-a încărcat normal pe 305k rânduri.

**Datorii rămase (moștenite, re-confirmate)**: perf-ul proiecțiilor pe baza de
import (D-2a/D-3a + `PoateGeneraDescarcare` care încarcă entitatea și enumeră
liniile la fiecare `Citeste`) — de MĂSURAT înainte de release **[MĂSURAT
2026-08-09, decizia 59 — `docs/api/p5-perf-masuratori.md`:
`PoateGeneraDescarcare` +~40ms doar pe drumul complet, neproblemă;
`DocumenteCuRest` ~410ms, acceptabil azi]**; emitentul pe
documentul EXISTENT se afișează static (lookup-ul ar minți pe repartitori din
afara setului `UnitateInterna`); scrierea manuală DSC prin API; RDC în proiecția
de rest. Artefactele de smoke (FCL-1, DSC-1+draft, INC 689, FCT SMOKE-F4-1+NIR)
rămân pe baza de dev Privat, ca la feliile anterioare.
