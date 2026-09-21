# Istoricul de execuție al planului de lucru (snapshot 2026-08-24)

> Extras verbatim din `CLAUDE.md` la 2026-08-24 (commit `f18c24c`); secțiunea
> finală (cronologia compactă) e mutată din `CLAUDE.md` la 2026-09-11. Starea
> curentă și „următorul pas" stau în `CLAUDE.md` §„Stare"; deciziile referite
> („decizia N") sunt în fișierele `NNN-*.md` din acest director.

## Plan de lucru (ordine obligatorie)

1. **Inventar legacy** → produce specificația de migrare: per tip de document,
   câmpurile reale, validările, regulile de stoc și contabile. Surse: tabelele
   de configurare din Sql Server (local) : db : Contabilitate_2026 + codul Delphi care le interpretează. 
   ATENȚIE la reguli hardcodate în Delphi pe lângă tabelele de config și la câmpuri
   refolosite cu semantici diferite per tip — se documentează explicit.
2. **Testul bazei** → pe inventarul real, lista finală de câmpuri pentru
   `Document` / `DocumentDetaliu` de bază (criteriul de la decizia 2).
3. **Modelul nou** → clase XAF (bază + 6 derivate, TPT), validare declarativă,
   tabelele de politică. *(Notă 2026-09-18: maparea TPT e depășită — din
   felia 28 cele trei ierarhii sunt TPH cu discriminatorul `ClrType`, decizia 89;
   textul de aici e istoric.)*
4. **Migrarea datelor** (EXECUTAT, decizia 34) → green-field la graniță de
   ciclu (decizia 12): nomenclatoare, politici, solduri de deschidere.
   Reconciliere: soldurile de deschidere în sistemul nou = soldurile de
   închidere din legacy — verificată de unealta `nou/tools/Migrare`.
5. **API + React** → abia după validarea pașilor 1–4. Designul tierului API:
   FIXAT (decizia 42, `docs/api/p5-api-design.md`); partea React = sesiune
   de design separată.

### Execuția pasului 3 — felii (o sesiune per felie; scheletul claselor există
în `/nou`, compilează, modelul EF validează — vezi decizia 22)

- **3a. Persistență + nomenclatoare + seed**: migrații EF + `Updater`;
  seed `TipDocument` (ancorele 1:1), Clasă/Tip curățat (clase tehnice separate
  de stoc), plan de conturi sintetic, repartitori/gestiuni minimali;
  politicile (RegulaStoc/RegulaContare) DOAR pentru NotaTransfer.
- **3b. Motorul de operare** (zonă sensibilă → sesiune dedicată): Draft→Operat→
  Stornat generic pe clasa de bază — scriere registre tranzacțional, creare
  loturi, picking auto-FIFO, rezolvare dimensiuni (coalesce), gardieni
  (perioadă, dependență pe loturi, sold intermediar ≥ 0). Vertical slice de
  validare: **NotaTransfer end-to-end** (un registru, două rânduri ±).
  Context la pornire (fixat pre-3b, vezi decizia 24): owned `Dimensiuni` e
  funcțional sub XAF — coalesce-ul poate conta pe instanțe non-null
  (`OwnsOneRequired`); orice owner instanțiat în cod în afara ObjectSpace
  (motor, teste) se creează cu `ctx.CreateProxy<T>()`, NU cu `new` (altfel EF
  aruncă la atașare). Modulul referă `Atlas.DXF.EfCore` (≥26.1.3.2): folosește
  `EnsureObject`/`CreateObject(id, initializer)` din ObjectSpaceExtensions
  pentru seed/upsert în loc de boilerplate propriu. `tools/ModelCheck` conține
  round-trip-ul owned (insert/update/all-null) — rămâne verde după orice
  schimbare de model; rețeta completă: xaf-kb
  `recipes/atlas-dxf/efcore-owned-types.md`.
- **3c. Tipurile de document, în ordinea dependențelor**: NIR + FacturaIntrare
  (conex + creare loturi — EXECUTAT, decizia 26) → BonConsum (EXECUTAT,
  decizia 27) → ListaDiferenteInventar (bidirecțional — EXECUTAT, decizia 28)
  → FacturaIesire (EXECUTAT, decizia 30) → Plata/Incasare + Imperechere
  (EXECUTAT, decizia 31) → Decont (EXECUTAT, decizia 32 — felia 3c ÎNCHISĂ;
  BPR rămâne rezervat, decizia 19). Per tip: politici seed + validare
  declarativă + test de operare.
- **3d. Validare transversală** (EXECUTAT, decizia 33 — pasul 3 ÎNCHIS, mai
  puțin BPR rezervat): dimensiuni obligatorii per cont, obligativități per tip
  (PoliticaValidare), invariantele imperecherii (erau în 31d).

### Faza privat — P-felii (decizia 35; fără ancoră legacy)

- **P1. Profil privat + TVA structural** (EXECUTAT, decizia 36; design în
  `docs/privat/p1-tva-design.md`): nomenclator `TipTva` (cotă × regim,
  conturile de TVA ca date, mapări SAF-T/D394), `TipTvaId` + `ValoareTva` pe
  baza `DocumentDetaliu`, `PoliticaTva` per tip de document, profilul contabil
  ca selecție de seed per bază + plan OMFP 1802; ModelCheck pe profilul privat.
- **P2. FacturaIesire completă la privat** (EXECUTAT, decizia 38; design în
  `docs/privat/p2-descarcare-design.md`, decizia 37): tip nou
  `DescarcareGestiune` generat pe loturi din FCL (`DescarcareService`,
  spargere la generare, acțiune manuală pe rest/backorder), General!+Specific?
  la culegere (ProdusId obligatoriu pe stoc, LotId pin opțional), derivarea
  de vânzare 371→707/345→701, + datoriile P1 (TipTvaImplicit, smoke UI XAF).
- Apoi: polish XAF pe modelul stabilizat (backlog-ul concret în decizia 38e);
  pasul 5 (API+React) neschimbat.

### FAZA 1C — călirea pe date reale (deciziile 44/45; înaintea pasului 5)

Design FIXAT în `docs/import/faza-1c-design.md`; ordinea feliilor (o sesiune
per felie):

- **1C-a. Tipurile noi de model** (EXECUTAT, decizia 46): NTC (NotaContabila),
  ITV (InchidereTva + `InchidereTvaService` + `PoliticaInchidereTva`), ASM
  (Asamblare n→m), RLF/RDC (retururi; spike-ul storno rezolvat — negativ pe
  corespondența originală, `RegulaContare.PastreazaSemn`); politici seed per
  profil; e2e ModelCheck per tip, ambele profiluri, + review advers.
- **1C-b. Scheletul `nou/tools/Import1C` + nomenclatoare + deschiderea**
  (EXECUTAT, decizia 47): FlaxDb + baza dedicată profil Privat, nomenclatoare
  mici integral / mari la cerere, deschiderea 01.01.2025 (solduri contra 891 +
  loturi din BalantaNivel3 cu netarea retururilor-ca-lot), reconcilierea
  deschiderii verde (55 simboluri, 8.283 chei stoc, 0 nejustificate) +
  auto-testul --sabotaj.
- **1C-c. Documentele 2025 prin motor** (EXECUTAT, decizia 49): toate cele
  23+2 tipuri Recorder cu handler, bucla lunii completă (documente →
  imperecheri → ITV → reconciliere), ianuarie verde pe contractele (1)+(2),
  contractul (3) cu familiile diagnosticate; fix-ul de model „scara
  numerică"; review advers cu D1/D2/D5 aplicate.
- **1C-d. Gate-ul fazei** (EXECUTAT, decizia 50): lotul de robustețe 49f în
  5 pași (cronologia sursei, identitatea produs×cont — amendament 47d,
  D3/D4/persistarea realocărilor, contractul măsurat, calea tipizată) +
  gate-ul G1/G2 (porțile vacue, „Acoperit cere acoperitor", registrul
  divergențelor cu egalitate per cont, porțile de proveniență); anul 2025
  integral prin motor: contractele (1)+(2) verzi 12/12 — 4423/4424 la cent
  (forcing function trecut), (3) verde 11/12 cu 1 cheie reziduală (−0,69,
  diferență de model documentată). Review advers: 6 defecte de fond =
  lotul pre-1C-d-final (decizia 50e), nefixate încă.
- **1C-d-final** (EXECUTAT, decizia 52): lotul 50e (D1–D6, cu D1/D5
  tranșate pe structura reală și D5-parțial respins pe măsurătoare) +
  hunk-ul BTR-cost validat și completat (decăderea la ieșiri) + G3 +
  convenția de rotunjire ca dată de profil (51c, `SetareProfil`) + două
  defecte de măsurătoare găsite și închise de re-validare; anul 2025 =
  CONTRACT ÎNDEPLINIT pe 4 contracte × 12 luni, 0 chei nejustificate.
  FAZA 1C ÎNCHISĂ.
- **GATE XAF** (EXECUTAT, decizia 53; contract: `docs/gate-xaf-contract.md`):
  polish FCT+FCL până la pragul „un contabil tolerant le operează zilnic" —
  `ProdusId` + nașterea lotului la culegere (golul de flux al FCT era de model),
  numărul consumat abia la materializare, calculul TVA la culegere, layout +
  captions RO, câmpurile motorului read-only; smoke UI real pe clona bazei de
  import + review advers cu 8 defecte fixate (cel critic: ștergerea loturilor
  istorice). Gate TRECUT. Lista React deschisă (`docs/api/lista-react.md`).
### Feliile DIM — dimensiunile pe frunze (decizia 54; înaintea pasului 5)

- **DIM-1. Contractul** (EXECUTAT, 2026-08-07; zero schimbare de schemă):
  perechea de contract pe `DocumentDetaliu` — `DimensiuniCulese()` (citire:
  copie detașată ca value object, interimar din owned) + `PreiaDimensiuni()`
  (scriere: folosită DOAR de clonări) — cu helper-ele `Dimensiuni.Copie()/
  CopiazaDin()` (doar FK-uri scalare, navigațiile nu se ating). Pe contract au
  trecut: coalesce-ul notelor + rândurile TVA + gardianul de clasificație
  bugetară din `ValideazaDeclarativ` (MotorOperare), clona conexă
  (`GenereazaConex`), plata autogenerată (FacturaIntrare) și descărcarea
  (DescarcareService) — trucul `d.Dimensiuni = Rezolva(s.Dimensiuni)` a murit.
  Owned-ul mai e atins doar de mapare (DbContext) și de culegere
  (UI/Import1C/ModelCheck) — exact ce mută DIM-2. ModelCheck verde ambele
  profiluri. Lucrat în modul „main-ul spune, utilizatorul implementează,
  main-ul verifică" — mod de re-apropriere, continuă la DIM-2.
- **DIM-2. Frunzele + migrația** (EXECUTAT, 2026-08-07; inventarul pe probe:
  `docs/dim/dim-2-inventar.md`, cele 3 întrebări tranșate de utilizator).
  Faptele care au dictat forma: Import1C nu atinge owned-ul DELOC (anul 2025
  a trecut fără nicio dimensiune pe linie — postarea explicită NTC + Material
  din lot + default header); UI-ul actual nu culege nicio dimensiune (owned
  read-only) — DIM-2 face culegerea POSIBILĂ prima dată; R/M nu se culeg pe
  linie nicăieri, U/CC n-au nicio probă (rămân doar în value object +
  registru). Rezultatul: FCT + cine primește clona ei (NIR, trezoreria) = 4
  FK-uri (E/F/CF/P); FCL/DSC/LDI/DEC/NTC = doar E; BTR/BCS/ASM/RLF/RDC =
  nimic. Frunze NOI: `NirDetaliu` (culegibilă și manual — Î3) și
  `DocumentTrezorerieDetaliu` UNIC pe PLT+INC (Î1; obligativitatea = politică
  per profil, la privat opționale — Î2/54d). Clonările instanțiază frunza din
  `[TipDetaliu]` (GenereazaConex generic prin atribut — declarația UI 40a
  devine sursa unică; plata autogenerată direct tipizat). Migrația
  `DimensiuniPeFrunze`: gard SQL zgomotos anti-pierdere (refuz pe orice
  valoare fără destinație — R/M/U/CC oriunde, F/CF/P în afara FCT, E în afara
  celor 6 frunze) → coloane noi → UPDATE de mutare → abia apoi DROP
  `Dimensiuni_*` de pe bază; Down cu copiere inversă. Owned-ul a dispărut de
  pe `DocumentDetaliu` (proprietate + mapare); baza întoarce value object gol
  / no-op. Import1C/Migrare: ZERO schimbări (doar recompilare). ModelCheck:
  setterii pe FK-urile frunzelor, liniile PLT/INC pe frunza nouă, asserțiile
  clonelor prin contract; VERDE ambele profiluri, migrația aplicată curat pe
  baza aplicației. Registrul/RegulaContare rămân owned până la DIM-3; smoke
  UI la DIM-4.
- **DIM-3. Registrul + regula, plate** (EXECUTAT, 2026-08-07):
  `RegistruContabil` = 2×8 perechi FK+navigație plate (`[Column]` conservă
  numele coloanelor owned — `DimensiuniDebit_*`/`DimensiuniCredit_*`) + puntea
  spre value object (`DimensiuniDebit()/Credit()` la citire — storno,
  `AplicaDimensiuniDebit/Credit()` la materializare); `RegulaContare` = 3×8
  plate + metodele `DimensiuniComun()/OverrideDebit()/OverrideCredit()` —
  editarea politicilor devine XAF-nativă (FK-uri normale, HideForeignKeys
  declarat). `Dimensiuni` = POCO pur (8 scalari nullable): OwnedObjectBase,
  navigațiile, ToString-ul cu etichete și regula CreateProxy au MURIT — ultima
  referință `Atlas.DXF.EfCore.Owned` a ieșit din Conta. DbContext: maparea
  owned + ConfigureDimensiuni(Eager) șterse; motivul 41c păstrat ca AutoInclude
  pe cele 16 navigații ale registrului (alias `RegistruContabilEntitate` —
  DbSet-ul omonim umbrea tipul în nameof). **Migrația `DimensiuniPlate` e
  GOALĂ** — maparea plată produce schemă relațională identică byte-cu-byte
  (proba directă a lui 54c „schemă identică, migrație zero"); există doar
  pentru snapshot. ModelCheck: round-trip-ul owned (decizia 24) devine garda
  mapării plate (o nepotrivire de nume de coloană pică acolo); asserțiile pe
  registru prin metodele value object; VERDE ambele profiluri. **AuditTrail
  REACTIVAT** (53e închis: owned-ul era singurul blocaj) — `AddAuditTrailEFCore`
  + `WithAuditedDbContext` (pattern-ul WebApi, care îl avea deja); updater-ul
  rulează curat prin contextul auditat; verificarea pe fluxul UI la DIM-4.
- **DIM-4. UI + re-validarea totală** (EXECUTAT MINIMAL, 2026-08-07; smoke-ul
  complet se reia în faza următoare — decizia utilizatorului, „oricum avem
  modificări de integrat"). **Testul suprem TRECUT**: re-rularea integrală
  Import1C (`--recreeaza`, anul 2025 prin motor pe codul DIM-1..3) —
  CONTRACT ÎNDEPLINIT, 02:33:51 total, iar raportul integral de reconciliere
  e **IDENTIC BYTE-CU-BYTE** cu baseline-ul pre-DIM (28.07, decizia 52), mai
  puțin antetul cu timestamp: anul real se reproduce exact — dovada finală a
  echivalenței refactorului. Smoke UI minimal (browser, pe clona de import
  migrată — gardul migrației validat pe cele 187k documente reale): aplicația
  pornește cu modelul DIM + AuditTrail; FCT list/detail funcționale, layout-ul
  GATE intact; linia FCT afișează cele 4 FK-uri noi cu captions RO, FK-urile
  brute ascunse, read-only-ul post-Draft (40c) acoperă și câmpurile noi;
  salvare auditată prin UI (create+delete nomenclator) fără NRE. **Rămase la
  reluarea smoke-ului**: ListView-ul RegistruContabil nu s-a încărcat în ~60s
  (rulat concurent cu importul — posibil doar contenție, posibil modul de
  acces pe ~600k rânduri; de diagnosticat); curatoria celor 16 coloane de
  navigații plate din grila registrului; layout-ul fin per tip + vizibilitatea
  per profil (`SetareProfil`). Feliile DIM-1…DIM-4 ÎNCHISE.

### Pasul 5 — API + React (deciziile 42/43; în curs)

- **Spike 1 — fir complet subțire pe BTR** (EXECUTAT, decizia 55; contract:
  `docs/api/p5-spike1-contract.md`): host WebApi hardening + gardianul de
  Committing + OperareApi/dry-run + felia verticală BTR (Apply/proiecții/
  endpoint-uri) + codegen + clientul React minimal; smoke browser end-to-end,
  review advers cu fix-urile aplicate (gate-ul de autorizare pe comenzi!),
  ambele verificări empirice 42 §8 închise.
- **Felia 2 — FCT + conex-NIR** (EXECUTATĂ, decizia 56; contract:
  `docs/api/p5-felia-fct-contract.md`): extracția LoturiCulegereService,
  Apply-ul FCT cu culegerea integrală (lot din ProdusId, TVA condiționat,
  override validat pe regim), NIR citire+comenzi, OData extins, driftul
  openapi offline (datoria M2 închisă), feliile client fct/nir + 3 bug-uri de
  nucleu fixate (Guid-uri OData, laSelectie funcțional, doar e.event).
- **Felia 3 — trezoreria** (EXECUTATĂ, decizia 57; contract:
  `docs/api/p5-felia-trz-contract.md`): PLT/INC generic, imperecheri + proiecția
  de rest, plata automată prin API, panoul de stingeri în client.
- **Felia 4 — FCL + descărcarea de gestiune** (EXECUTATĂ, decizia 58; contract:
  `docs/api/p5-felia-fcl-contract.md`): FacturaIesireApply (Numar server-owned,
  fără loturi), DSC citire+comenzi, generarea manuală pe ușa non-secured +
  rest-nedescarcat, feliile client fcl/dsc cu pinul de lot; review advers: fixul
  acțiunii XAF (ruptă sub gardian de la spike-1) + plafonul de acoperire per
  linie-sursă.
- **Măsurarea de perf a proiecțiilor pe baza de import** (EXECUTATĂ, decizia
  59; raport: `docs/api/p5-perf-masuratori.md`): datoria D-2a/D-3a +
  `PoateGeneraDescarcare` închisă ca măsurătoare — totul sub 150ms în afară de
  `DocumenteCuRest` (~410ms, acceptabil azi; optimizarea țintită documentată,
  se aplică când cifra o cere).
- **Felia 5 — NIR scriere** (EXECUTATĂ, decizia 62; contract:
  `docs/api/p5-felia-nir-contract.md`): `ProdusId`+`PretUnitar` pe `NirDetaliu`,
  contractul `ILinieCareNasteLot` + `LoturiCulegereService` generalizat cu gardul
  de lot străin, `NirApply.Aplica/Sterge` + POST/PUT/DELETE, felia client
  editabilă; review advers: capcana lotului străin în XAF + ștergerea conexului.
- **Felia 6 — LDI + BCS** (EXECUTATĂ, decizia 63; contract:
  `docs/api/p5-felia-ldi-bcs-contract.md`): modelul LDI+ (ProdusId +
  ILinieCareNasteLot + gardul NasteLot + gestiunea = predatorul — închide
  53i), API pe șabloanele BTR/NIR cu valoarea la culegere, feliile client
  bcs/ldi cu comutator de direcție; review advers: gardul lotului-frate +
  coerența Tip↔Produs.
- **Felia 7 — viramentul intern (transferul 581)** (EXECUTATĂ, decizia 64;
  contract: `docs/api/p5-felia-vir-contract.md`): perechea PLT+INC pe aceleași
  laturi, `NaturaClasa.Virament` + Clasa/Tipul `VIR` cu 581 ca dată de profil,
  `GenereazaSecundar` pe `DocumentTrezorerie`, două fix-uri de fond în motor
  (imperecherea automată doar dacă documentul stinge; dimensiunea Repartitor pe
  contul propriu al piciorului), `EsteVirament` în ReadDto + al treilea fel de
  contrapartidă în client. Închide amânarea 31f.
- **Felia 8 — Decont (DEC) + legătura explicită de pereche** (EXECUTATĂ,
  decizia 65; contract: `docs/api/p5-felia-dec-pereche-contract.md`): felia DEC
  completă (aderarea `ILinieCuPretUnitar`, postarea explicită prin API și
  client, `Cont`/`Angajament`/`Repartitor` în OData) — ridică excluderea F6-D12
  — plus `LaturaPerecheId` pe `DocumentTrezorerie`, care închide gaura 64k.
- **Felia 9 — raportarea pe registre** (EXECUTATĂ, decizia 66; contract:
  `docs/api/p5-felia-raportare-contract.md`): balanță de verificare (sintetică
  și analitică pe repartitor), fișă de cont cu sold curent prin window function
  — primul SQL brut din repo — și registru-jurnal, peste un atom de unpivot
  partajat; `urlStare` în nucleu (prima folosire reală a lui „URL = starea
  globală"). Prima felie care nu adaugă un tip de document, ci **suprafața de
  citire** cu care se verifică tot ce s-a construit înainte.
- **Felia 10 — balanța pliată pe planul de conturi** (EXECUTATĂ, decizia 67;
  contract: `docs/api/p5-felia-balanta-plan-contract.md`): rollup cu netare
  refăcută la fiecare nod peste `Balanta`, `nivelMaxim`, ecran de arbore în
  client. Închide R-D5.
- **Felia 11 — jurnalele de TVA** (EXECUTATĂ, decizia 68; design + închidere:
  `docs/api/p5-felia-jurnale-tva-design.md`): `RegistruTva` (al treilea registru)
  + derivarea în motor + unealta de backfill + jurnalul de cumpărări/vânzări +
  scheletul D300, cu reconciliere per document probată pe 61.347 de documente.
  Fork-ul tranșat: sursa e
  un **registru NOU, `RegistruTva`**, nu o proiecție peste documente —
  invariantul III („orice raport e o sumă peste registre") și decizia 36f
  („D300/D394/SAF-T ca proiecții peste REGISTRE") bat nota din felia 9, care
  anticipa „altă sursă". Registrele de azi n-au faptele fiscale: rândul
  4426/4427 poartă doar TVA-ul, nu baza și nu tipul, iar liniile
  scutite/neimpozabile/capitalizate NU postează deloc, deși apar legal în
  jurnal și în D300. Scop confirmat: registrul + jurnalul de cumpărări +
  jurnalul de vânzări + agregarea per cotă (scheletul D300), cu codurile
  SAF-T/D394 ca atribute pe rând; generarea fișierelor rămâne proiect separat
  (35c).
- **Felia 12 — D300** (EXECUTATĂ, decizia 69; design + închidere:
  `docs/api/p5-felia-d300-design.md`, formularul în
  `docs/api/d300-structura-2026.md`): nomenclatorul rândurilor OPANAF 174/2026
  (55 poziții, seed în nucleu) + maparea `(TipTva × Sens) → rând` ca politică
  privată + proiecția în memorie cu formulele în cod + `/d300`. Review advers
  cu 8 constatări, 6 fixate (V_6 la storno de nedeductibil, scăderea rd. 31 pe
  operanzi, dubla numărare părinte+copil, ștergerea logică vs re-seed, seed-ul
  care nu corecta, afordanțele). Smoke pe importul 2025: rd. 9/24 = decont la
  cent, rd. 37 vs ITV explicat integral prin punțile NTC. Perf 25 ms/an.
- **Felia 13 — motor/structură post-D300** (EXECUTATĂ, decizia 70; contract +
  închidere: `docs/api/p5-felia-motor-structura-contract.md`): 69-r4 taxarea
  inversă cu SENS (`PoliticaTva.Directie` în `TvaService`, gard în motor + la
  PUT, excepția D300 moartă), 69-r7 ModelCheck cu interceptorul de ștergere
  amânată + `Purja` (curățenie fizică; F5/57f pe mecanismul real; scurgerea
  DIM-3 oprită), 69-r5 un singur 400 = `EroriDto`, 67e gardian de ciclu pe
  `Cont.Parinte`. Review advers 0 fond / 3 medii fixate / 4 minore. D5: Import1C
  re-rulat integral (1:47) — CONTRACT ÎNDEPLINIT, raport identic pe conținut cu
  baseline-ul DIM-4 (doar ordinea unor linii de justificare diferă).
- **Felia 14 — D394** (EXECUTATĂ, decizia 71; design + închidere:
  `docs/api/p5-felia-d394-design.md`, formularul în
  `docs/api/d394-structura-2026.md`): identitatea fiscală pe `Partener`
  (`TipPersoana`/`Tara`/`InregistratTva`/`TvaLaIncasare`) + `MapareD394 (TipTva
  × Sens) → Tip` ca politică privată (13 mapări + 7 nemapate deliberate;
  `CategorieD394` scoasă) + proiecția `op1`/rezumate/H/`Neincluse`/avertismente
  per cauză peste `RegistruTva` + `GET api/proiectii/d394` + `/d394` + Import1C
  cu identitatea fiscală din `flax.Partenerii` și `--reclasifica` (semnalul din
  registru). Commit-uri: 8038d18 (pas 1, model), b10249c (pas 4a, Import1C),
  2cea578 (pas 2, proiecția), 321b740 (pas 3, API + client, perf addendum 5),
  0d58f1f, 64ec5bc (fixurile review-ului advers: 8 constatări, toate fixate —
  „înregistrat bate tot", unirea pe CUI peste nomenclatoare, `TintaPermisa(tip,
  sens)`, partenerul șters declarat, avertismente agregate). Cusătura cu
  registrul și cu D300 la cent pe scenă și pe septembrie 2025. Perf 99 ms/lună,
  394 ms/an. Închidere: D5 (Import1C `--recreeaza`,
  01:47:31) cu raport IDENTIC cu baseline-ul DIM-4 (445 linii, diff sortat 0;
  +23 avertismente = clasificarea partenerilor); `--reclasifica` 20.118 parteneri,
  35 înregistrați din registru; smoke 09/2025 pe clona Flax: cusătura cu D300 la
  cent, `CombinatieRefuzata` 0, fix la smoke pentru CUI „-" ⇒ null. Decizia 71.
- **Pasul 5, felia 15 — partener + ANAF — executată** (2026-08-25/26; contract
  `docs/api/p5-felia-partener-anaf-contract.md`): adresa plată pe `Partener`
  (SAF-T) + nomenclatorul `Judet` + migrația `AddAdresaPartener`; clientul
  `PlatitorTva` v9 + `SincronizareAnafService` (merge „gol se umple, diferit se
  raportează, canonicul bate") + acțiunea XAF; REST `POST
  api/parteneri/{id}/sincronizeaza-anaf` (+ lot) cu `ComandaAutorizata<T>`;
  Import1C cu adresele din 1C pe bloc gol și `--anaf`. Commit-uri: 7d80bc9
  (contract), c1d527c (pas 1, model), 56f57f3 (pas 2, serviciu), ab89dfb (pas
  3, REST), ab614af (HideForeignKeys), 9528811 (pas 4, Import1C), de5c9e4
  (fix-urile review-ului advers: F1 județ pe străin în Import1C, F2 `InactivFiscal`
  server-owned, F3 V4 măsurat; R6/R8). Închidere: `--recreeaza` pe Flax (3h38)
  cu reconciliere IDENTICĂ cu baseline-ul (diff sortat 0), 20.038/20.118
  adrese preluate; `--anaf` 8.230 găsiți / 2 negăsiți / 0 erori, +190
  parteneri tip 4 → 1; D394 09/2025 înainte/după pe clona Flax.Api: 62 CUI
  mutate, toate = statutul ANAF de azi, Σ formular identică. Decizia 72.
- **Pasul 5, felia 16 — SAF-T (D406 L) — executată** (2026-08-26; contract
  `docs/api/p5-felia-saft-contract.md`, structura oficială
  `docs/api/d406-structura-2026.md`): `Societate` (un rând, editabil),
  `UnitateMasura` (UN/ECE, 2.163), `Produs.CodNc` + FK UM, `Cont.RolTert` +
  `Cont.Functie` seed-uite la privat, migrația `F16SaftModel`; `SaftReguli`
  (identitatea `00`–`06`, 380/381, metoda de plată), `SaftProiectii` →
  `SaftDto` cu cusături + `Neincluse` + avertismente agregate, `SaftXml`
  streaming, `Duk.cs` = validatorul oficial ca oracol în ModelCheck; REST
  `GET api/proiectii/saft` (sumar) + `saft/xml` (403 pe `User`), ecranul
  `/saft`; Import1C `--societate`/`--um-nc`/`--saft`. Commit-uri: 82aeec8
  (contract + structură), c104e05 (pas 1, model), 482f119 (pas 2, proiecție),
  010909c (pas 3, XML + DUK), 1ad6f1e (pas 4, REST + client), 5c1fc05 (pas
  4b, sumar JSON + CUI), 55c71b6 (pas 5, Import1C). Închidere: DUK J2.2.8
  `ok` pe fișierele REALE 09 și 12/2025 de pe Flax (70,9 / 71,6 MiB), 6/6
  cusături la cent (partidă dublă 80.527.820,95), reconcilierea neatinsă;
  JSON-ul întreg de 38,6 MiB/lună ⇒ sumar. Decizia 73.
- **Pasul 5, felia 17 — SAF-T S (stocuri) — executată** (contract:
  `docs/api/p5-felia-saft-s-contract.md`; 4 pași + review advers cu fix-uri
  F1–F8, un agent per pas, verificare independentă + commit după fiecare):
  3f1f519 (contract + pas 1, `PoliticaMiscareSaft`), ee6bb6a (pas 2,
  `SaftStocuri`), 195838f (pas 3, XML + DUK), 4722055 (pas 4, REST + client
  + Import1C), fix-urile odată cu decizia. Închidere: DUK J2.2.8 `ok` pe
  fișierele REALE 09 și 12/2025 (36,2 / 34,3 MiB; 16.723 / 16.444 intrări de
  stoc fizic, 9.341 / 8.523 mișcări), S1/S5 0 diferite, S2 la cent, S4 0
  referințe duplicate după fix, 0 `Neincluse`, reconcilierea neatinsă;
  review-ul a găsit `MovementReference` neunic (reprodus pe fișierul
  livrat), S1 oarbă la fișier (⇒ S5), codul politicii nere-verificat.
  Decizia 74.
- **Pasul 5, felia 18 — restanțele grele ale lui S (74-r4/r6/r9) —
  executată** (contract: `docs/api/p5-felia-restante-s-contract.md`; 4 pași +
  review advers F1–F10 cu fix-uri, un agent per pas): bc8f8ea/ba233f6 (pas 1,
  `AgregatStoc` + `CoduriTipPeTipuri`, 4,0→2,9 s, ținta < 1 s neatinsă fără
  vinovat dominant), 7436988 (pas 2, D18-D2 motor: golirea preia valoarea
  rămasă), 06f9fc4 (pas 3, D18-D3/D4 Import1C: reclasificarea ca mișcare,
  D18-V3 pe clonă), fix-urile + proba finală odată cu decizia. Închidere:
  re-rularea integrală Flax pe codul final (1h52, contract îndeplinit 12
  luni, 0 FAIL), oracolul golirii verde 12/12, 381/608 dispar din registrul
  divergențelor, `--saft-s` 09/12 DUK ok, punțile == ASM `#reclas` la cent,
  `ReziduValoricFaraCantitate` 861/1.168 → 131/132. Incident: prima rulare
  a mers pe binarul pre-fix (pană de net, sesiune moartă) — refăcută.
  Decizia 75.
- Restul: finisaj de client (listele §Închidere ale contractelor +
  `docs/api/lista-react.md`; licența DevExtreme = acțiunea utilizatorului) și
  feliile de scriere rămase (NTC/ASM/retururi, la cerere).
  Alternativă rămasă: felia C1a a comenzilor
  (`docs/architecture-notes-2026-07-28.md` — bifurcație deschisă, la presiune
  de client).

- **Pasul 5, felia 19 — NTC + ASM + retururi (RLF/RDC) — executată**
  (contract: `docs/api/p5-felia-ntc-asm-retururi-contract.md`; 6 pași de
  implementare + 2 de fix + review advers + gate Import1C + smoke, un agent per
  pas): modelul ASM (`ProdusId` + `ILinieCareNasteLot`, gestiunea = predatorul —
  închide 53i pe ASM), API NTC + `candidati`, API ASM +
  `distribuie-valoarea` (închide 75-r1: predicția e cifra MOTORULUI, prin
  `MotorOperare.Valideaza` pe un OS de unică folosință), API RLF + RDC, client
  pentru toate patru. Pe parcurs a ieșit un defect de MOTOR preexistent —
  plafonul de stingere al notei se număra per latură dar se consuma fără latură
  (o compensare de 60 stingea 120) — reparat în două trepte: sensul
  (`Document.SensDeStins`, al treilea hook al rolului) și, după ce review-ul
  advers a arătat că premisa mea era greșită, **netarea** (`|Σ semnat|` per
  repartitor × latură, în loc de `Σ|v|`). Review advers: 4 constatări de fond, 3
  minore, 5 observații. Închidere: ModelCheck 786→858+ bugetar / 623→870 privat
  0 FAIL, build client verde, drift exit 0; gate Import1C pe Flax (2h10, binar
  din codul final) CONTRACT ÎNDEPLINIT cu TOT identic cu proba finală F18 în
  afara imperecherilor (46.056 → 44.448, atribuite integral: 899 chei cu net
  EXACT 0, 10,5 mil. lei plafon fantomă; conservarea închide pe ambele rulări la
  52.039); smoke browser 6/6 fluxuri, zero erori de consolă. Lecția consemnată:
  „toate verificările existente rămân verzi" NU e un invariant de ne-regresie —
  verde înseamnă „nimic din ce e acoperit nu s-a rupt". Decizia 76.

- **Felia 20 — finisajul clientului** (2026-08-29, contract
  `docs/api/p5-felia-finisaj-client-contract.md`, F20-D1…D10): lista
  `lista-react.md` + itemii de client rămași din §Închidere ale F1–F19, în 7
  pași cu 6 agenți. Căutarea fără diacritice tranșată la nivel de BAZĂ:
  coloană generată STORED `Cautare` (translate+lower, IMMUTABLE) pe 14
  nomenclatoare prin `ICuCautare` + buclă generică în `OnModelCreating`, cu
  oracol SQL==C# pe toate rândurile; `nucleu/odata.ts` = singurul store OData
  al clientului, `byKey` prin TanStack pe `(entitate, id, proiecție)`;
  precompletarea scrie (id, etichetă) prin cache; `ConfirmareInline` — zero
  `window.confirm`; `Neincluse` agregat per cauză pe server + `ContId` pe S3;
  `metadata.json` capătă `Nomenclatoare`; 70-r1 închisă (422 `EroriDto` pe
  OData); primele ecrane de nomenclator (Partener + ANAF, Societate, Produs,
  politica SAF-T citire). Review advers: 2 de fond (storno mort pe 11 ecrane
  printr-un `== null` pe `ReactNode`; cache-ul supraviețuia „Ieșire"), 3 medii,
  6 cosmetice — F1–F9 fixate. Smoke 7/7 + F6 și datoria F12 închise de la
  sine. Decizia 77.
- **2026-08-30 — 77-r2 (decizia 77k)**: `Cod`/`Simbol` + `Denumire`
  obligatorii pe toate cele 14 nomenclatoare `ICuCautare` — NOT NULL + CHECK
  `btrim <> ''` prin bucla generică a coloanei `Cautare` (migrația
  `CodDenumireObligatorii`, 0 rânduri goale pe bazele de dev în afara
  partenerului-probă), gardian cu mesajul câmpului pe ușa secured,
  `[Required]` ⇒ OpenAPI ⇒ asterisc în client, `[RuleRequiredField]` pentru
  XAF; probe ModelCheck pe trei uși (0 FAIL ambele profiluri) + HTTP 422 pe
  `api/odata/Partener`.
- **2026-09-01 — P5-F21, ITV prin API și client (decizia 79)**: ultimul tip
  fără felie capătă ușa lui ca COMANDĂ, nu agregat — `InchidereTvaService`
  întoarce rezultat cu cauză (`MotivNegenerare`; `Genereaza` rămâne wrapper
  pentru Import1C), `CalculeazaLinii` = singura aritmetică, `Previzualizeaza`
  = raport (cronologia ca motiv) vs `Incearca` = comandă (refuz zgomotos);
  `api/itv` cu gate pe TIP (`PoateCrea`/`PoateCiti`) și `AutorizeazaCitire`
  pe instanță, cifrele motorului (solduri, `Stale`) pe ușa non-secured după
  verdictul de acces; ITV iese din felia NTC (patru uși); ecranul de listă cu
  previzualizarea lunii + generare, ecranul documentului cu regenerare și
  storno la data închiderii; review advers: 2 defecte de fond (cronologia
  doar la generare și doar într-un sens ⇒ 4423 dublat; regenerarea ștergea
  înainte de a verifica ⇒ draft pierdut) + 9 medii, fixate cu probe;
  ModelCheck final privat 944 / bugetar 900, 0 FAIL; 31 de probe HTTP +
  smoke 11/11 pe Privat. Decizia 79.

- **2026-09-02 — P5-F22, refuzurile de acces pe toate ușile (decizia 80)**:
  restanța de fond 77-r8 și familia ei (70-r1/72-r10/76-r4/76-r5/77k/79-r6)
  închise printr-o singură regulă: 404 = inexistent SAU invizibil (nedistins,
  fără oracol de existență), 403 = vizibil / pe tip dar operația refuzată,
  422 = domeniu doar pe cereri permise; ordinea 401→400→404→403→422 pe REST și
  OData. Module: `Refuzuri` (mesaje unice, `RefuzAcces :
  IUserFriendlySecurityException`), `Rezolva.Cere` (88 de throw-uri „nu
  există" ⇒ fraza onestă „nu există sau nu e vizibil"), pasul zero al
  `GardianEditare` (securitatea înaintea domeniului — inversarea lui 77k),
  rolul `Cititori`/userul `Cititor` dev-only. WebApi: gate explicit pe ușa de
  scriere pe tipul feliei (`CreareAutorizata`/`ScriereAutorizata(id, op)`,
  Create+Write pe nou, Delete distinct), `ComandaAutorizata<T>` pe tipul
  feliei + `peUsaAsta`, corpuri `EroriDto` pe 403/404, `RefuzOdataFilter`
  (404 → 403 → 422), ITV cere Read pe `RegistruContabil`. Client: o singură
  ramură pe `Erori[]`, `dxStore.onAjaxError`. Probe: script repetabil
  `nou/tools/ProbeHttp/refuzuri.ps1` — 42 de probe, 42 PASS pe Privat
  (Admin/Cititor/User), fără urme; review advers 0 fond / 2 medii (M1 ușa NTC
  pe id ITV, M2 Create fără Write) fixate cu probe; ModelCheck final privat
  947 / bugetar 903, 0 FAIL. Capcană: agenții au rescris fișiere CRLF ca LF —
  regula e terminatorul fișierului. Decizia 80.
- **2026-09-02 — 79-r1, acțiunea XAF „Generează închiderea"** (făcută
  direct, un singur pas): `Module/Controllers/InchidereTvaGenerareController.cs`
  — `PopupWindowShowAction` pe lista ITV cu dialog pe obiect non-persistent
  (an/lună/unitate; lookup-ul prin `PopulateAdditionalObjectSpaces` local
  dialogului; tipul exportat în `Module.cs`), gate-ul de pe API (Create ȘI
  Write pe tip, `Refuzuri.FaraDrept`), comanda prin `InchidereTvaApply.Genereaza`
  pe ușa non-secured, motivul negenerării ca toast cu `[XafDisplayName]`,
  draftul deschis în tab nou (`TargetWindow.NewWindow` — MDI; `Default` și
  `Current` probate și respinse pe sursa `BlazorMdiShowViewStrategy`). Smoke
  în browser 7/7 (Admin + Cititor), fără urme; capcană: `EditMask "0"` pe
  `int` afișează `0`, masca e `"d"`. Textul în decizia 079 (restanța 79-r1).
- **2026-09-02 → 2026-09-09 — Pasul 5, felia 23: implicitele de culegere +
  întreținerea politicilor** (contract
  `docs/api/p5-felia23-implicite-politici-contract.md`, decizia 81; șase
  pași, un agent per pas, verificare independentă): 1a Module
  (`ImpliciteService`, `ClasaFiscala`, `PoliticaTvaImplicit`, `TipTva.Activ`,
  `DinSeed` + 15 indexuri unice, ramurile gardianului, `VerificareProfilService`),
  1b ModelCheck (F23-V1…V6), 2 WebApi (OData deschis pe politici, `api/implicite`,
  `api/politici/verificare`; fix de fond `GetObjectByKey<Partener>` sub TPT),
  3 probe HTTP (blocul „politici", 80/80), 4 client (`GrilaPolitica`, 7 ecrane,
  precompletarea), 5 review advers (tabelul scenariilor în 081; proba nouă
  „rândul șters logic nu e dublu"), 6 docs. Între ele, decizia 82 (stingerea
  automată prin contract) și guvernanța documentației (`docs/stare-curenta`,
  regula codului slim).
- **2026-09-10 → 2026-09-11 — Pasul 5, felia 24: politicile — seed-ul aliniază,
  `Configurator`, cele șapte ecrane, `Potrivire` și „Explică"** (contract
  `docs/api/p5-felia24-politici-explica-contract.md`, decizia 84; două
  track-uri în paralel pe worktree-uri și baze ModelCheck cu sufix, un agent
  per pas, verificare independentă, commit per pas): 1 seed (`Aliniaza`,
  `RaportSeed`, `IMP`/`NIM`, F24-V1…V8), 2 rol (`TipuriConfigurabile`,
  `SeedRolConfigurator`, al patrulea oracol — 100/100), 4 `Motor/Potrivire.cs`
  + `Fapte` (oglinzile moarte, F24-P1…P8, smoke Import1C 3 luni), 5
  `GET api/politici/explica` (F24-E1…E7, 111/111), 3 cele șapte ecrane
  (`GrilaPolitica.formular`, 81-r8, smoke în browser), 6 panoul „Explică"
  (19 cazuri), 7 review advers (două defecte de fond fixate: enum 0 pe OData,
  concluzia contra gardurilor frunzelor ⇒ `[GardContare]` + `Rezerve`; trei
  medii: DEC postare explicită, curățenia `PoliticaValidare`, ștergerea
  `TipTva` referit) + docs + Import1C integral identic cu baseline-ul F18.

## Cronologia compactă a feliilor (la 2026-09-11)

Executate, în ordine (contractele/design-urile per felie în `docs/`; istoricul
detaliat în jurnal):

- **Pasul 1–2** — inventar legacy + testul bazei (`db/inventar/`).
- **Pasul 3** — modelul: 3a persistență/seed (23), 3b motorul (25), 3c tipurile
  (26–32), 3d validarea transversală (33). BPR rămâne rezervat.
- **Pasul 4** — migrarea legacy (34); azi prototip de conector (35a).
- **Faza privat** — P1 TVA structural (36), P2 descărcarea de gestiune (37/38);
  Atlas.DXF + polish XAF (39–41).
- **Design pasul 5** — API (42), React (43); amânat după 1C (44).
- **Faza 1C** — tipurile noi (46), Import1C (47–50), 1C-d-final (52): anul 2025
  prin motor cu contract îndeplinit 4 × 12 luni. ÎNCHISĂ.
- **GATE XAF** (53) — trecut. **DIM-1…4** (54) — owned-ul a murit; raport de
  reconciliere identic byte-cu-byte.
- **Pasul 5** — spike BTR (55), FCT+NIR (56), trezorerie (57), FCL+DSC (58),
  perf (59), mărunțișuri (60–61), NIR scriere (62), LDI+BCS (63), virament (64),
  DEC + pereche (65), raportare (66), balanța pliată (67), jurnale TVA (68),
  D300 (69), motor/structură post-D300 (70), D394 (71), partener + ANAF (72),
  SAF-T D406 L (73), SAF-T S stocuri (74), restanțele grele ale lui S —
  golirea valorică în motor + reclasificarea ca mișcare (75), NTC + ASM +
  retururi prin API și client + plafonul de stingere cu latură și netat (76),
  finisajul clientului — căutarea fără diacritice, cache-ul de nomenclator,
  confirmările, `Neincluse` agregat, primele ecrane de nomenclator (77),
  căutarea fără diacritice pe proiecții — filtrele grilelor prin
  `DataSourceLoader`, prin același normalizator (78), ITV prin API și client
  — comandă cu cauză + ecran de rezultat (79), refuzurile de acces pe toate
  ușile — 404/403/422 cu o singură ordine și un singur corp, gate pe scriere,
  pasul zero al gardianului, probele HTTP cu script (80), implicitele de
  culegere + întreținerea politicilor — regimul e al partenerului, cota e a
  produsului; politicile pe OData cu invarianții în gardian, unicitatea în
  schemă, `DinSeed`, raportul de profil, grila comună (81); stingerea
  automată prin contract (82); seed-ul și proveniența — re-seed pe `DinSeed`,
  golurile privat cu poarta spre DVI, rolul `Configurator` (83); felia 24 —
  seed-ul aliniază, `Configurator` seed-uit, cele șapte ecrane de politică,
  `Motor/Potrivire.cs` consumat de motor și de explicație, gardul de nivel
  minim ca atribut pe clasă, `GET api/politici/explica` + panoul „Explică",
  enum-urile fără membru și ștergerea `TipTva` referit refuzate (84).
- **Decizia 85** (2026-09-11) — modul de acces al ListView-urilor XAF Blazor:
  `Server` cu paginare ca implicit, `ServerView` opt-in pe registre,
  `Client` explicit pe grilele nested de culegere, IF/IFV cu prag măsurat,
  selecția prin `GetObject`, `Lot.Eticheta` `[Calculated]` (tranșează D4 din
  gate-ul XAF).
- **Felia 25** (2026-09-12/13, decizia 86) — DVI ca tip de document: TVA-ul în
  vamă prin `PoliticaTva` fără schimbare în motorul de operare, tipurile de
  import ca date (`DeImport`), D300 rd. 24/25 și 7/22, codurile SAF-T de
  import, legătura n→m cu facturile (`DviFactura` + `IVerificabilLaCommit`),
  DVI nu e document stins, `api/dvi` + `felii/dvi`, smoke React și XAF.
  Închide 83-r4.
- **Felia 26** (2026-09-14/15, decizia 87) — imobilizări și amortizare,
  contabil și fiscal: fișa ca nomenclator subțire cu parametrii ca fapte
  datate, registrul al patrulea `RegistruImobilizari` scris prin
  `IDocumentCuRegistruPropriu` (trei dispecere în motor), PIF (intrare,
  modernizare, revizuire; nu postează), CAS (două note per fișă din
  politică, liniile produse de server), AMO generată lunar pe tiparul ITV
  cu trei cifre din `Motor/AmortizareService.cs` (cota fixată la ultimul
  eveniment, baza la sfârșitul lunii evenimentului — formula confirmată pe
  118 active din Flax, 99,49 %), `PoliticaAmortizare` + `RegulaDeductibilitate`
  versionată + catalogul HG 2139/2004 ca date, `CodEconomic` ca dimensiune
  pe fișă (F26-r16), nomenclatoarele pe OData, `api/pif|cas|amo` +
  `felii/imobilizari|pif|cas|amo`, smoke React și XAF, Import1C integral cu
  raport identic; defectul de cronologie a lunilor (anularea AMO sub CAS
  operată) găsit la smoke-ul XAF și fixat. Concretizează 9.
- **Felia 27** (2026-09-16/17, decizia 88, contract
  `docs/api/p5-felia27-perioade-solduri-contract.md`, F27-D1…D10) — perioada
  ca lanț, închiderea ca comandă, soldurile materializate. Nouă pași, un agent
  per pas, commit per pas: **0** spike-ul cursei și al costului snapshot-ului
  (F1 probat pe calea reală XAF; `SUM` integral 300 ms azi / ~1,5 s la 5 ani ⇒
  două amendamente: tranzacția e a comenzii, snapshot doar pe perioadele de
  REFERINȚĂ); **1** lanțul și comenzile `inchide`/`redeschide` cu istoric
  append-only și gardianul pe `Inchisa`; **2a** tranzacția comenzii +
  `FOR SHARE`/`FOR UPDATE` + cele două tabele de snapshot, **2b**
  `SolduriService` la citire cu toți consumatorii mutați (o oprire: inițialul
  de stoc al SAF-T rămâne pe registrul integral, F27-r10); **3**
  `Document.DataInregistrare` cu implicitul la trei seam-uri, registrele și
  lotul la data înregistrării; **4a** `PerioadaDeclarare` + `ScrisLa` pe
  `RegistruTva` cu rectificativa ca derivat (o oprire: perioada fiscală E luna,
  deci o probă pe fereastră de zile nu mai e exprimabilă), **4b** recuperarea
  amortizării întârziate cu `Luni` pe linie și pe registru; **5** corecția ca
  storno legat + document nou cu motiv, cu culegerea copiată generic prin
  metadata EF; **6** `TotalStingere` la operare, `PartidaDeschisa` la
  închidere, împerecherea datată cu desfacere prin rând invers,
  `DocumenteCuRest` rescris, `sold-parteneri` (constatarea de produs: dimensiunea
  `Repartitor` urmează laturile, nu contul de terț — F27-r11); **7**
  `PoliticaInchidere` (al 20-lea tip configurabil), cele patru constatări de
  conținut, acceptarea conștientă, dialogul XAF și `/perioade` în React (o
  oprire: severitatea se coboară în POLITICĂ pe durata închiderilor de scenă,
  calea operatorului real); **8a** probele supreme, integritatea soldurilor și
  perf-ul, **8b** două runde de review advers (pașii 0–6: 0 MAJOR, 3 MEDIU,
  5 MINOR, 7 observații; pasul 7: rezumatul plafonat ca acceptare în bloc și
  „un fapt, o constatare"), **8c** decizia 88 și restanțele.
  **Ce a ieșit la 8a**: Import1C integral pe Flax CU lunile închise pe parcurs
  (2026-09-17, 12:12 → 14:27, exit 0; 12/12 luni, 0 constatări, 0,7 → 5,2 s per
  lună) ⇒ importul citește peste snapshot-uri, iar raportul de reconciliere e
  IDENTIC pe conținut cu baseline-ul feliei 26 — proba supremă a feliei;
  reconstrucția soldurilor cu **0 diferențe** pe toate trei materializările
  (184.780 contabil / 7.914 stoc / 201.046 partide). A/B-ul pe ACEEAȘI bază
  (lanțul desfăcut prin 11 redeschideri, apoi re-închis cronologic cu aceleași
  cifre la rând) a INFIRMAT bănuiala de regresie a partidelor: fișa `4111`
  187 → 122 ms, balanța analitică 254 → 210 ms, soldul de stoc 153 → 50 ms,
  operarea unei FCT cu 49 de linii 411 → 394 ms, iar `documente-cu-rest`
  171 → 181 ms, adică neschimbat cu și fără partide. Fixul de formă al
  proiecției (corelarea legăturii pe fereastră) a fost MĂSURAT ȘI RESPINS
  motivat, nu omis: duce panoul filtrat la 82 ms, dar calea neplafonată a
  constatării de rest scadent de la 220 ms la 1,02 s — nu se plătește, deci
  felia rămâne pe forma existentă (F27-r16). Au rămas neatinse două ținte,
  NEoptimizate conform regulii de oprire (F27-r14 cadrul cererii, F27-r15
  cardinalitatea balanței analitice). Închidere: ModelCheck bugetar 1278/0, privat 1434/0;
  `refuzuri.ps1` 285/285. Amendează 31d; închide 79-r3.
- **Felia 28** (2026-09-18, decizia 89, contract
  `docs/api/p5-felia28-tph-contract.md`, F28-D1…D8) — TPH cu discriminatorul
  mapat `ClrType` pe `Document`, `DocumentDetaliu` și `Repartitor`, tipul ca
  dată. Cinci pași, un agent per pas: **0** spike-ul de mapare (coloanele
  partajate fără prefix, 37 / 45 / 28 coloane; gardianul coliziunilor pe
  tipul de STOCARE — `Directie` și `Fel` sunt enumuri diferite peste
  `integer`; FK-ul discriminator → `TipDocument.ClrType` a căzut pe calea XAF
  și fallback-ul declarat s-a ratificat; indexul compus
  `(ClrType, DocumentId)` respins pe EXPLAIN A/B; cele 45 de migrații șterse,
  un singur `InitialCreate`; Import1C pe luna 01/2025 identic cu baseline-ul);
  **1** bazele recreate și probele dependente de TPT rescrise pe intenție,
  F28-A/B/C/F/G (defect latent scos la iveală: D4-V2 și D16-V2 își luau
  premisa dintr-un draft orfan istoric); **2** `CititorTipDocument` cu
  consumatorii mutați (`CoduriTipPeTipuri` dispare, 75-r2 închisă),
  `GardianEditare` regula (o) pe FK-urile spre frunze (9, descoperite din
  metadata), F28-D/E; **3** probele supreme: Import1C integral pe `3c4193f`
  (15:45 → 19:06, exit 0, 3 h 21 min cu o pauză de ~1 h 20 min pusă pe
  presiunea de memorie a mașinii; ritmul pe lună identic cu F27), raportul
  `reconciliere-20260918-154628.txt` IDENTIC pe conținut sortat cu
  baseline-ul, 12/12 luni închise fără constatări (0,7 → 5,4 s),
  `Reconstruieste` 0 diferențe (10,4 s), integritatea TPH pe Flax 0 încălcări
  în 103 interogări (14,2 M rânduri), drift și `pnpm build` verzi,
  `refuzuri.ps1` 294/294. Perf A/B TPT → TPH pe aceeași bază de conținut:
  fișa `4111` −57 %, operarea FCT −48 %, `CoduriTip` ≈ 10× pe HTTP,
  `RegistruTva` Server ≈ 60× (42 → 4 JOIN-uri), restul în zgomot; singura
  regresie, D406 S la rece +0,2 s, e cost per proces (F28-r5); **4**
  review advers (0 MAJOR, 2 MEDIU, 2 MINOR, 3 observații: contractul greșea —
  `as`/cast pe frunză NU filtrează pe tip, doar `is ? :` emite `CASE`; ușa
  de sistem probată prin F28-H/I/J din `IntegritateTph.cs`; F28-K pe
  ierarhia utilizatorilor XAF), apoi defectul PREEXISTENT scos de o
  investigație pornită din review (`GetObjectByKey<Frunza>` pe un id urmărit
  ca altă frunză: 500 sau obiectul greșit, fiindcă identity map-ul EF e per
  rădăcină și prefetch-ul XAF nu verifică tipul) ⇒ `RandDupaCheie` + fraza
  unică `RandDeAltTip`, F28-L/M/N, 9 probe noi în `refuzuri.ps1`; docs.
  Închidere: ModelCheck bugetar 1294/0, privat 1450/0. Amendează 3, 16 și
  IM-D10; închide 75-r2.
- **Nucleul cub** (2026-09-19/20, decizia 90, `docs/nucleu/`) — sesiune de
  arhitectură fără constrângerile proiectului, convergentă pe un singur cub
  de postări în locul celor patru registre (`nucleu-cub-design.md`), apoi
  trei pași probați, câte unul per sesiune, fiecare cu review advers de
  agent separat: **1** coordonatele contra rapoartelor reale
  (`nucleu-coordonate-rapoarte.md`, patru inventare cu `fișier:linie`;
  patru amendamente structurale: `Latura` D/C, stornoul ca tranzacție
  distinctă, taxa per linie, partenerul pe postarea de terț); **2** fizica
  măsurată pe clonele `Atlas.Conta.Nucleu.Fizica.x1/.x10`
  (`nucleu-fizica.md`, FZ-D1…D9: LIST pe `Spatiu` ales pe structură, 9 FK
  în loc de 24, scrierea 1,7 contra 5,3 ms, 8/13 rapoarte identice, `Sold`
  4–9× cub contra cub dar 2,1× snapshot-ul; FZ-r2 măsurată: fișa 348 contra
  248 ms); **3** transferul (`nucleu-transfer.md`, TR-D1…D4 pe cifre:
  partida = unitate pe cont de terț, împerecherea = nominalizare +
  `Împerechere` cu `Transfer`, FCT postează recepția, o singură postare de
  stoc; §3 transferă / rescrie / dispare; §4 ordinea TR-D5…D10; §4.1
  propunerea de execuție acceptată de owner 2026-09-20), `nucleu-bilant.md`
  ca sinteză. **Decizia 90** scrisă 2026-09-20 (pasul 0, TR-D5): regula
  durabilă (a)–(m), invarianții I/III/VI amendați, contractul IM depășit,
  IM-r re-evaluate, TR-r/FZ-r în restanțe. Fără cod. Următorul pas: TR-D6a
  (nucleul pur, `nou/Atlas.Conta.Nucleu`), cu contract propriu în
  `docs/nucleu/`.
- **Felia 29 — TR-D6a, nucleul pur** (2026-09-20, branch
  `tr-d6a-nucleu-pur`, contractul `docs/nucleu/tr-d6a-nucleu-pur-contract.md`
  cu N-D1…N-D12; `main` adus fast-forward la 090 înainte). Patru pași cu un
  agent per pas și verificare independentă: **1** scheletul BCL-only
  (xunit.v3), cubul, `Scara`/`Rotunjire` cu instanță și contor,
  `Conservare` C1–C6, testul de arhitectură; **2** `Repartizare` (Hamilton
  ierarhic) și `Tva` (per document × cotă, Hamilton per linie, toleranță);
  **3** `Unitate` (partida cu id determinist), `Fifo` (pin-uri întâi),
  `Evaluare` (raportul curent, ultima ia restul), `Sold`/`Cub`, `Storno`,
  invarianții 3–5 cu contra-proba prețului înghețat; **4** `Declaratie`/
  `Motor`/`Contract`, `Decizie`/`Ipoteza` închise, invarianții 2 și 6.
  Amendamente ale main-ului la pași: `Sold` cu gardian de scară, cauza
  străină permisă în storno doar pe postarea cu `Atribuit`. Review advers
  (agent separat): 1 MAJOR (semnul taxei per linie pe grupuri cu semne
  mixte — Hamilton pe fiecare semn), 2 MEDII (unitatea pe contul postării;
  postările datate ca tranzacția), minore (fără aliasing al listelor, pin
  ≤ 0 refuzat, tranzacția fără postări, C1 per `Carte`, cantitate ⇒
  produs) — toate aplicate. Închidere: `dotnet test` 152/152, 0
  avertismente; diff pe BackOffice/tools/Client gol; ModelCheck nerulat
  (nimic atins din ce probează). N-r1…N-r9 în restanțe; invariantul 7 e al
  lui TR-D7/D10. Următorul pas: TR-D6b (declarația fluxului BCS/PLT/FCT).
- **Felia 30 — TR-D6b, declarația fluxului per tip (pilot BCS, PLT, FCT)**
  (2026-09-20, branch `tr-d6b-declaratia-fluxului` tăiat din `main` după
  fast-forward-ul feliei 29; contractul
  `docs/nucleu/tr-d6b-declaratia-fluxului-contract.md` cu B-D1…B-D10).
  Cinci pași cu un agent per pas și verificare independentă (ModelCheck pe
  ambele profiluri după fiecare): **1** scheletul — Module referă nucleul,
  `Declaratii/` (`IDeclarant`, `Operand` + faptele, `Contractare`),
  `Fapte.Operand` pe seturi, `Document.Declarant()` (metodă: o proprietate
  intra în metadata clientului); **2** oracolul în ModelCheck —
  `CubDinRegistre` (portul mapării fizicii), `Normalizari` (lista închisă
  B-D8), `Comparabil`, `ProbeNucleu`, auto-probele NUC-ORACOL; **3** BCS
  (N-r3 măsurat: Δ = +25 pe lotul corectat); **4** PLT/INC într-o singură
  clasă, nominalizarea partidei sursei prin `Fifo`, splitul liniei, scena
  privată cu partide; **5** FCT — recepția TR-D3, taxa per document × cotă
  (N-r4 măsurat: Δ = 0,01), capitalizatul ca bază + taxă, `GestiuniVirtuale`
  mutate în nucleu cu C5 amendat (N-r2 confirmat). Constatările pilotului
  au amendat contractul: partida doar pe cont cu `RolTert` (bugetarul n-are
  niciunul), „repartitorul pe piciorul propriu" (azi nota pune pe fiecare
  picior repartitorul contrapartidei), linia nominalizată parțial sparge și
  piciorul de bani. Egalitate EXACTĂ cu registrele normalizate pe toate
  documentele celor trei tipuri, pe ambele profiluri. Review advers (agent
  separat, tier-ul main-ului): 1 MAJOR (nominalizarea PLT afirma restul
  DOCUMENTULUI ca sold al partidei de pe contul liniei — pe o factură cu
  404 + 401 nominaliza peste ce ține partida; oracolul avea același unghi
  mort) — operandul poartă soldul sursei per cont, plafon = min(rest,
  sold), oracolul plafonează la fel; 4 MEDII (taxa culeasă autoritară per
  linie, nu per cotă; `Sold.Din` adună cantitatea doar pe spațiul Stoc —
  capătul virtual conservă, nu se citește; ordinea liniilor `OrderBy(ID)`;
  toleranța per cotă) și 7 minore, toate aplicate; verdictul 090l: forma
  mai simplă în citire per tip, ≈2× mai lungă, echivalentă azi — condiția
  (helperii `Fiscal`/`Partide` înainte de al patrulea declarant) îndeplinită
  în felie. Închidere: ModelCheck privat 1573/0, bugetar 1389/0; nucleu
  156/156; diff pe WebApi/Blazor.Server/Client gol; `MotorOperare` și
  hook-urile neatinse. B-r1…B-r11 în restanțe. Următorul pas: TR-D7 (strangler per tip: `PosteazaInCub`,
  entitatea `Postare`, materializarea).
- **Felia 31 — TR-D7a, cubul persistat și strangler-ul primelor patru tipuri**
  (2026-09-20/21, branch `tr-d7-strangler` tăiat din `main` după felia 30;
  contractul `docs/nucleu/tr-d7a-strangler-contract.md` cu S-D1…S-D16).
  Șapte pași cu un agent per pas și verificare independentă a main-ului:
  **1** entitățile `Postare`/`Tranzactie` (POCO fără `BaseObject`: convențiile
  XAF se aplică pe interfețe, deci nu cer excludere), migrația cu
  partiționarea LIST pe `Spatiu` și FK-urile per partiție scrise în SQL, cele
  patru coloane de politică, probele `STR-SCHEMA-*`/`STR-POZITIE`;
  **2** materializarea în tranzacția de comandă (refuzul declarației = refuzul
  operației), cititorul rândurilor, stornoul ca a doua tranzacție cu perioada
  fiscală re-ștampilată, anularea ca ștergere, `Pozitie` citită de ambele
  motoare; **3** cele două unelte de gate și măsurătoarea read-only pe o clonă
  a bazei Flax (53.449 de documente, 23:32 min): 25.488 neegale, cu PATRU
  cauze, toate ale formei — pe Flax niciun document de trezorerie n-are sursă,
  stingerea trăiește doar în `Imperecheri`, create DUPĂ operare; B-r1 măsurată
  (275 refuzuri la 0,01/linie, maxim 55,87), B-r10 infirmată pe date, B-r7
  confirmată pe FCT, B-r5 căutată în motor și negăsită; **4** BCS și FCT pe
  cub (valoarea negativă admisă în `Operare` ca linie „în roșu”, toleranța
  taxei opțională, partidă pe fiecare cont cu `RolTert`, FCT-urile fără rânduri
  proprii ca excepție declarată a oracolului); **5** PLT/INC cu împerecherea
  ulterioară ca tranzacție `Transfer` pe partide, cu două amendamente ale
  ORACOLULUI (rândul invers citit algebric, spargerea piciorului de bani doar
  pe împerecherile de la operare); **6** proba supremă pe Import1C integral;
  **7** review advers (agent separat, read-only): 3 MAJOR — plafonul unei
  împerecheri e RESTUL partidei stinsului cu conexul autogenerat absorbit și
  transferurile deja primite (cele 14.498 „trunchiate” și 1.168 „sărite” erau
  trunchierea REFERINȚEI, nu fapt de date: după fix 709 plafonate legitim și 1
  sărită), `Transfer.Data` = `Imperechere.Data` (`max(DataInregistrare)`
  rescria perioade închise la desfacere și storno), corecția cu
  `EroareMateriala` re-ștampilează perioada și pe postările de storno — plus 6
  MEDII și minorele, toate aplicate sau declarate ca regulă. Închidere:
  ModelCheck privat 1674/0, bugetar 1410/0, nucleu 159/159,
  `--reconciliere-cub` 0 Δ pe (a)…(g) pe ambele profiluri; gate-ul pe clona
  Flax BCS 544 egale + 3 explicate, FCT 19.022 egale + 13 excepție declarată,
  PLT 2.486/2.486 și INC 31.381/31.381; drift zero; diff gol pe
  Blazor.Server/WebApi/Client. Import1C integral: Import1C integral pe Flax (`--recreeaza --cititori --inchide-lunile`, 2026-09-21): exit 0, 1 h 57 min (3 h 21 min la felia 28), raportul `nou/tools/Import1C/reconciliere-20260921-035646.txt` IDENTIC pe conținut sortat cu baseline-ul feliei 28, ZERO refuzuri ale declarației, 12/12 luni închise cu 0 constatări, `--reconciliere-cub` 0 rânduri Δ pe (a)–(g) — (f) vacuă: cele 9 conturi cu rol de terț sunt atinse și de tipuri nemigrate —, integritatea TPH 0 încălcări în 107 interogări, cubul cu 70.373 tranzacții / 252.092 postări / 16.924 transferuri (PLT → FCT; INC → FCL fără transfer, FCL fiind nemigrat), `refuzuri.ps1` 294/294 PASS pe `Atlas.Conta.BackOffice.Privat` refăcută din import cu perioadele redeschise. B-r1, B-r2, B-r7,
  B-r9, B-r10 și B-r11 închise; S-r1…S-r10 în restanțe. Următorul pas: TR-D7b
  (tipurile rămase pe cub, în ordinea volumului pe Flax).
