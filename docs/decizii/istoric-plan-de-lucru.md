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

- **Decizia 091 — scenarii în loc de import** (2026-09-22, branch
  `tr-d7b-tipuri-ramase`, după pasul 2b al feliei 32): owner-ul, după
  consultarea cu Codex (`docs/consultari/codex-2026-09-22-cub.md`), oprește
  Import1C ca gate; proba supremă devine catalogul `docs/nucleu/scenarii/`
  (așteptări de mână, ciclul complet per tip, 13 lanțuri transversale);
  „rotund" primește definiție (091 (g)); stornoul după reevaluare = inversare
  + compensare cu proveniență (091 (h)); triajul celor 248 de restanțe
  deschise: 34 activă, 182 după PoC, 14 migrare, 16 cade la TR-D9, 2
  depășite; pașii 3–5 ai feliei 32 continuă pe scenarii, pasul 6 devine
  `Deschidere` generic, pasul 7 se taie. Următorul pas: 091-r1
  (`--scenarii <TIP>`), fișierele tipurilor deja pe cub, apoi pasul 3 NTC +
  ITV pe scenarii.

- **TR-D7b, pasul 4 — DVI și SC-X-14** (2026-09-23): implementat
  contractul aprobat DVI-B1…B7. Baza vamală devine pereche echilibrată în
  Carte=Fiscal, taxa rămâne contabilă; fără schemă nouă sau cont tehnic.
  `DeImport` intră în operandul închis; activare numai pe privat.
  20 de scenarii DVI, inclusiv baza 0,01/taxa 0, 401 cu dependent NTC,
  storno/corecție și înregistrarea întârziată. SC-X-14 verifică fiscalitatea
  pe matricile numerice ale catalogului, cu martori de duplicare/lipsă.
  `ReconciliereCub.Contabile` filtrează cartea contabilă; adaptorul DVI
  reconstruiește perechea fiscală și gestiunea taxei. Gate integral
  1.829 bugetar / 2.897 privat, zero FAIL; nucleu 165/165. Manifestele și
  acoperirea sunt în `docs/nucleu/scenarii/DVI.md`. T-r10 închisă; T-r11,
  B-r4, T-r5/86-r11, 86-r2 și reevaluarea pe loturi rămân declarate.
  Următorul pas: ASM → LDI → NIR, cu scenarii înainte de cod.

- **TR-D8 pregătire — corecturi citiri/DEC și NIR delta** (2026-09-24):
  proveniența stornoului este obligatorie la citire, cu completare numai a
  perechilor istorice univoce; contraponderea virtuală are un predicat comun.
  DEC probează numeric inversarea/corecția partidei 542. NIR aplică 098/099:
  diferență față de recepția istorică a facturii, cauză și politică pe clasă,
  imputat extern/angajat, proveniență persistentă inclusiv la delta zero,
  unicitate concurentă a cumulului și gardian de stoc pe cub. Maparea
  bugetară NIR-D4 a fost aprobată explicit de owner. API/clientul expun
  cauza, imputatul și politicile; reconcilierea fizică delimitează în (h)
  întreg grupul cu delta nenulă. Contractul și probele finale sunt în
  `docs/nucleu/tr-d8-nir-delta-contract.md` și `docs/nucleu/scenarii/NIR.md`.
  Review-ul advers a fost amânat de owner până după NIR; predare fără
  commit. Cititorii generali rămân de portat la TR-D8; intervalul dintre
  inversarea și operarea cumulului corectat poate lăsa registrele vechi
  negative și este raportat ca grup incomplet.

- **Corecturi review NIR 1945** (2026-09-24): proveniență completată în
  migrație, fără fallback runtime; citirea sursei o singură dată pe comandă,
  imputat golit când nu produce efect, analiza istorică păstrată. Probe noi
  SC-NIR-32…36; Integral 2.836/3.917 OK, selectiv NIR 227/224 OK, zero FAIL,
  client/drift/HTTP/browser trecute. Limita gărzii registrelor include și
  storno/anularea simplă după consum, 098-r3 cade la TR-D9. Predare fără
  commit pentru reverificarea independentă.

- **TR-D8 — snapshot de stoc pe cub** (2026-09-25): cheia devine
  lot/cont/produs/gestiune, cu data deschiderii păstrată. Materializarea
  incrementală, reconstrucția și cititorul comun folosesc cubul; registrul
  necesar regimului dual rămâne separat. SC-CIT-69…75 probează recepția
  fără NIR operat, deschiderea, transferul/consumul, storno peste închidere,
  FIFO, alterarea snapshot-ului, reziduul valoric și securitatea HTTP.
  ModelCheck integral 3.166 bugetar / 4.166 privat OK, zero FAIL; HTTP 15/15;
  client și regenerarea artefactelor verificate. Schema a fost probată pe
  baze noi, fără recrearea bazelor principale (102b). Review propriu în
  `docs/nucleu/tr-d8-review-codex.md`; fiscal/SAF-T și verificările
  transversale rămân deschise. Predare fără commit și fără mesaj intermediar
  către Claude, conform instrucțiunii owner-ului.

- **TR-D8 — corecturi snapshot C1–C5** (2026-09-25, peste `07c79e0`):
  proiecție comună de lot și helper pentru referință + fereastră, într-o
  singură instrucțiune SQL. Evaluarea ieșirilor consumă snapshot-ul cu
  graniță strict anterioară. Scrierile globale refuză securitatea activă;
  HTTP a corectat presupunerea că interfața `ISecuredObjectSpace` ar fi
  suficientă pentru a o identifica în DevExpress 26.1. Migrația a fost
  aplicată pe cele trei baze principale, fără recreare; bazele reziduale
  au fost numai inventariate. SC-CIT-76…78 adăugate, 69…75 păstrate.
  Integral final **3.178/4.178 OK**, zero FAIL,
  `run-verificari/20260925-213600-946`; HTTP **15/15**, curățare verificată;
  client și regenerări stabile. Eșecurile intermediare, hash-urile,
  comenzile și limitele sunt în `docs/nucleu/tr-d8-review-codex.md`.
  Predare fără commit. D8-B8 și SC-CIT-79…87 pregătesc fiscal/TVA, cu
  surse, alternative și trei alegeri pentru owner; rămân propuneri,
  fără modificări fiscale de cod și fără mesaj intermediar către Claude.

- **TR-D8 — felia fiscală 103** (2026-09-25/26, peste `07c79e0`): owner-ul
  aprobă F1=A/F2=A/F3=A. Faptele fiscale păstrează calificarea istorică și
  atribuiri distincte D300/D394; corecția tehnică folosește Δ curent D300 și
  înlocuirea facturii D394. Depunerea se confirmă explicit; anularea faptelor
  declarate și depunerea corecțiilor incomplete se refuză. Autocolectare
  distinctă la TI, mapare SAF-T versionată pe secțiune. Cititorii comuni,
  API și client sunt portați împreună. Integral **3.182/4.214**, Nucleu
  **180/180**, HTTP securizat/browser și artefacte stabile; A/B documentat
  inclusiv regresia de latență a jurnalului/D394. `103-r1` și `B-r4` închise;
  restul SAF-T, reconcilierea/auditul și scalarea rămân TR-D8. Fără commit.

- **Fiscal 103 — review R1–R5** (2026-09-26, același diff peste `07c79e0`):
  amprentă a faptelor exportate și refuz atomic al versiunii depășite,
  snapshot comun raport/amprentă, eliminarea politicii `DeclarareIntarziata`,
  CHECK pentru calificarea fiscală completă, contract și stare aliniate.
  SC-CIT-88/89, integral **3.207/4.237 OK**, zero FAIL,
  `run-verificari/20260926-111832-851`; HTTP și browser cu exportul vechi
  refuzat și noul export confirmat, metadata/OpenAPI/types stabile,
  client final build exit 0. Migrația `FiscalComplet` aplicată și verificată
  pe cele trei baze principale fără recreare. R6 rămâne specificație:
  103(h), SC-CIT-90…94, `tr-d8-tva-intervale-contract.md`, fără cod;
  referința la avans cu mai multe cote este delimitată pentru review.
  Comenzile și limitele sunt în `tr-d8-review-codex.md`. Fără commit.

- **2026-09-28 — R6 implementat (103h/i), review advers închis.** Intervale
  TVA, marcaj explicit `TvaCules`, recalcul L3/XAF/API, referință la linia
  avansului, avertismente pure și raport secured de impact. Migrație canonică,
  un singur FK TPH, CHECK-uri, seed pe ambele profiluri. Integrala ambelor
  profiluri exit 0 (`20260927-235325-207`); scenarii FISCALE după corecturile
  de review, HTTP R6 și 294/294 refuzuri, browser XAF/React, build client și
  generare repetată fără drift. Review-ul propriu a corectat materializarea
  faptelor cu cotă ascunsă prin permisiuni; proba este în `tva-intervale.py`.
  Corecturile M1–M3 și m2 sunt verificate: FISCALE ambele profiluri
  (`20260928-005014-921`), HTTP și browser XAF; commit aprobat de owner.
  Proveniența ajustărilor rămâne 103-r2; validarea semnului taxei, 103-r3.
  Bazele de verificare sunt izolate.

- **2026-10-04 — TR-D8 gate-ul transversal (decizia 108), pașii 1–6 pe
  `tr-d8-transversal`.** Proba registrelor cu lista nominală și regimul
  `Transfer` pe intrările comune (X-D2); explicația deciziei persistată pe
  tranzacție, cu cititor, `GET` și acces complet (X-D4); scrierea serială
  per bază, probată pe două conexiuni (X-D6); acoperirea cantitativă a
  stocului, `PosteazaInCub` ireversibil, refuzul desfacerii fără efect
  (X-D7); scara `PerfCub` în container și reconcilierea (a)–(g) cu zero Δ
  pe baza de volum (X-D5, X-D3), cu un criteriu amânat de owner
  (`PartideCuRest`, F27-r16). Integrala **3.387 bugetar / 4.597 privat OK**,
  nucleu 180/180, matricea de refuzuri 300/300 de două ori,
  `SCRIERE_OCUPATA` pe ușa HTTP, drift zero. Închise: S-r2, S-r9, S-r11,
  F27-r8, T-r11, T-r14, N-r8, 097-r1, 102-r4, SAFT-r4, FZ-r1. Noi: X-r1,
  X-r2, X-r3. Review-ul implementării (Codex, la `2b57aa5`): patru
  observații P2 pe verificările feliei, X-RI1…X-RI4, corectate — mecanismul
  explicației ținut de declarant, conservarea pe cont a transferului
  persistat, planul respins care invalidează măsurarea, controlul D300 pe
  rezultatul D300; integrala **3.394 / 4.604 OK**, scara 1.526 / 996 OK.
  Reverificarea Codex la `7223abc` închide X-RI1…X-RI4: integrala proprie
  3.394 / 4.604 OK, zero FAIL, inclusiv mutanții și `X-D5-PLAN`;
  `run-verificari/20261004-102241-857/`. Mers în main prin PR #15.

- **2026-10-04 — M1, deschiderea de terți din `BalantaNivel3` (decizia 107),
  prima felie a 091-r4, pe `m1-deschidere-terti`.** Import1C scrie deschiderea
  prin `Materializare.Deschide`: controale brute per (cont, latură), loturile
  ca detaliu, terții ca partide inițiale, pozițiile fără partener pe
  `MIGRARE-NEDEFINIT`. Stingerile anului pe pozițiile de deschidere intră în
  cub pe partidă, în trecerea 2, plafonate la rest. Contractul 5 al
  reconcilierii compară restul partidei cu soldul poziției în sursă.
  Implementată 2026-10-01 (anul oprit după 8 luni la cererea owner-ului),
  rebazată pe main `ac372fc` și reverificată pe ianuarie: 0 eșecuri de
  import, INV-CUB verde cu invarianții 108, deschiderea fără diferențe de
  detaliu, reluarea cu jurnal identic. Contractele 1 și 2 pică pe aceleași 5
  conturi, la ban (107-r3). Contractul 5: 1.474 partide stinse integral, 34
  explicate de refuzuri, 30 de plafonare, 130 neatinse de trecerea 2, 10 FAIL
  (107-r9). M1-D10 amendat de owner la deschidere exactă + ianuarie verde pe
  mecanism; 12/12 trece în restul 091-r4, după TR-D9. Închise: T-r4, TR-r6,
  FZ-r10 pe date. Noi: 107-r1…r10. Review-ul advers (Codex): M1-R1…R4 și
  M1-R3a corectate, închise la RV2 pe `111b89a`; importul proaspăt nu a fost
  repetat cu binarul final. `run-verificari/m1-ian-r1r4/`. Mers în main prin
  PR #20.

- **2026-10-04 — 107-r3, drift-ul contractului 1 față de baseline (decizia
  109), pe `107-r3-drift-tva`.** Cauza nu era motorul: R6 (103i) a înlocuit
  proxy-ul „taxă nenulă = culeasă” cu marcajul `TvaCules`, iar conectorul
  scria taxa sursei fără marcaj, deci toate cele 7.242 de linii cu taxă din
  ianuarie erau recalculate la cotă. Conectorul marchează acum taxa sursei
  (`Catalog.TaxaDinSursa`). Pe drum a ieșit o diferență de fond: pe
  documentele nemarcate linia purta taxa rotunjită pe linie, iar cubul taxa pe
  document × cotă (481 din 4.926 de documente pe Flax, cel mult 0,02). Taxa
  nemarcată se decide acum pe document × cotă în formula entității și se scrie
  pe linii, la culegere și la operare; declarația postează taxa liniei;
  INV-CUB refuză divergența (`CITIRE_TAXA_DIFERITA_DE_LINIE`). Scenarii noi:
  SC-FCL-11, SC-FCL-12, SC-FCT-11, roșii pe motorul vechi. ModelCheck integral
  **3.459 / 4.705 OK**, zero FAIL. Ianuarie pe Flax, numai cu conectorul
  corectat (`.Flax.R3`): contractele 1 și 2 verzi la ban, egale cu baseline-ul
  în afara Δ-ului declarat al stocului (107 e); împerecherile refuzate „peste
  totalul documentului” 212 → 46. Importul proaspăt cu binarul final
  (`.Flax.R3f`): aceleași contracte 1–4, INV-CUB verde cu invariantul nou.
  Contractul 5 a dat 12, 10 și 8 partide fără explicație la trei importuri
  proaspete (107-r7, 107-r9). Proba din browser (109-r2) a găsit un conflict
  de versiune la operarea din tabul documentului după salvarea unei linii în
  tabul ei: reîncărcarea liniei era tratată ca editare. Corectat: adaptorul
  XAF nu recalculează la reîncărcare și reîncarcă liniile surori (109 f), iar
  formula scrie fiecare câmp o singură dată (109 g). După corecție: 2,11 /
  2,10 / 4,20, total 48,47 pe ecran, pe totalul de stins, în cub și în
  registre. Închise: 107-r3, 109-r2. Noi: 109-r1, 109-r3, 109-r4. Decizia e
  aprobată de owner (2026-10-04); review-ul advers e nefăcut.
  `run-verificari/r3-ian/`, `r3-ian-final/`, `r3-motor/`.

- **2026-10-05 — review-ul advers Codex al deciziei 109, o observație
  (109-R1), corectată de Codex și reverificată independent.** Poziția liniei
  noi se atribuie la salvare, deci la culegere linia nouă era prima în
  repartizare și ultima după salvare: cu o linie marcată, draftul salvat arăta
  36,46, iar operarea 36,45. Liniile fără poziție vin acum după cele
  numerotate, în ordinea colecției documentului, aceeași la calcul și la
  atribuirea pozițiilor (`BackOfficeEFCoreDbContext.InOrdineaPozitiilor`).
  SC-FCL-13, SC-FCL-14 (0/1/2 linii salvate, fără și cu taxă culeasă), roșii
  pe ordonarea veche. ModelCheck integral **3.459 / 4.771 OK**, zero FAIL, pe
  binar recompilat. Proba pe HTTP și cea din browser (a treia linie adăugată
  din ecran, FCL-6 și FCL-7) trec; ianuarie pe Flax păstrează contractele 1–4.
  `docs/nucleu/109-review-codex.md`, `run-verificari/r3-ui/r1-*`, `r3-ian-r1/`.

- **2026-10-05 — TR-D9a, pașii 0–2: contractul tăierii, inventarul și
  probele dinaintea ei.** Contract aprobat de owner cu review Codex închis
  (`docs/nucleu/tr-d9-taierea-contract.md`), inventarul nominal
  (`tr-d9-inventar.md`), amendamentul 1 (D9-A1…A9). Pasul 2, sub regim dual:
  gardul analizei obligatorii portat pe mișcările contractului
  (`Cub/GardAnaliza`), cu domeniul structural (mișcările din cartea
  contabilă) și cu recepția facturii păzită la operarea facturii (schimbarea
  5). Echivalența cu gardul vechi e probată prin trei integrale bugetare: cu
  amândouă, numai cu cel nou, cu niciunul. Scenarii noi, cu cifra de azi
  măsurată și cu cea de după tăiere scrisă înaintea codului: SC-BCS-16,
  SC-FCT-12…14, SC-RLF-14, SC-RDC-19, SC-DSC-09, SC-DES-22, SC-X-25…27;
  SC-ASM-17…26 rescrise nominal. Măsurătorile au corectat trei rânduri ale
  inventarului și au arătat că limita 098-r3 nu refuză nimic azi. Două
  constatări lăsate owner-ului: cerința de repartitor nu refuză niciun
  document (G1) și materialul din regulă nu ajunge pe postare (G2). ModelCheck
  integral **3.527 / 4.835 OK**, zero FAIL; nucleu 180/180.
  `docs/nucleu/tr-d9-pas2-probe.md`, `run-nucleu/tr-d9a/pas2/`.

- **2026-10-06 — TR-D9a, pasul 3: aserțiile de regulă pe cititorii cubului.**
  338 de aserții ModelCheck care păzeau o cifră citită din registre: 313
  re-țintite pe `Cub/Citiri` cu același nume și aceleași literale, după ce
  forma veche și cea nouă au rulat verzi pe aceeași scenă; una ștearsă cu
  rând de catalog (SC-DEC-16); 17 din clasa repartitorului amânate la 6b;
  șase rămase pe forma veche, fiindcă cubul dă altă cifră, toate diferențe
  declarate (D9-D3, SC-NIR-09, o scenă de oracol). Verificarea main-ului a
  întors zece ștergeri ale căror perechi nu purtau valoarea. Numai
  `nou/tools/ModelCheck/`, niciun cititor nou în produs. ModelCheck integral
  **3.531 / 4.839 OK**, zero FAIL.
  `docs/nucleu/tr-d9-pas3-retintire.md`, `run-nucleu/tr-d9a/pas3/`.

- **2026-10-06 — TR-D9a, pasul 4: consumatorii din produs pe cub.** Sub
  regim dual, tot ce în produs mai citea registrele în afara motorului vechi
  trece pe cub. Cele trei porți `RegistrulCitibil` și verificarea închiderii
  cer citirea completă pe `Postare` (schimbarea 3 din D9-D1): 403 înaintea
  citirii pentru rolul cu criteriu de rând sau de membru. Gardianul refuză
  scrierea unei `Postare` și a unei `Tranzactie` pe ușile securizate. Lista
  XAF `Postare`, `ServerView`, fără editare. Cele trei refuzuri de ștergere
  (lot, tip de TVA, fișă) și latura contului propriu din plățile SAF-T citesc
  cubul, respectiv `Document.Laturi()`; `DocumenteCuRest` nu mai proiectează
  totalul din antet. Rolurile din seed nu s-au schimbat: drepturile pe cub
  vin din politica rolului. Lista nominală X-D2 scade de la 75 la 69 de
  intrări. Proba pe host viu a corectat un pin: `$metadata` descrie orice tip
  al contextului ca `EntityType`, deci neexpunerea se probează pe
  `EntitySet` și pe rute. Limite noi: lista arată identificatori bruți.
  ModelCheck integral **3.552 / 4.860 OK**, zero FAIL; `refuzuri.ps1` de două
  ori la rând 324 / 324, cu 24 de rânduri noi pe dreptul unic; neexpunerea
  0 FAIL; lista, detaliul postării și al tranzacției văzute în browser.
  `run-nucleu/tr-d9a/pas4/`, `run-verificari/d9-pas4-http/`.

- **2026-10-06 — TR-D9a, pasul 5: uneltele.** Import1C citește cubul prin
  `Cub/Citiri` și nu mai referă registrele, `StocService` sau planul vechi.
  Contractul 2 al reconcilierii își păstrează sensul: perechea debit / credit
  a liniei ITV se reface din cele două postări ale ei. Predicția de valoare a
  alocării cheamă evaluarea nucleului. Reconcilierea deschiderii justifică pe
  chei celulele cu valoare fără cantitate, care nu intră în cub. Au ieșit
  `--sabotaj`, oracolul golirii din raport, scrierea deschiderii în registre
  și purja defensivă. `Migrare` și `BackfillTva` sunt șterse. Totul e probat
  numai prin compilare: unealta nu se rulează între pașii 5 și 6. ModelCheck
  integral neschimbat, **3.552 / 4.860 OK**.
  `docs/import/faza-1c-design.md` §16, `run-nucleu/tr-d9a/pas5/`.

- **2026-10-06 — TR-D9a, review-ul advers Codex al lotului 3–5, închis.**
  Două constatări în probele pasului 3, corectate și reverificate. D9-L35-R1:
  două aserții își pierduseră clauza pe nota persistată, fiindcă perechea
  numită în inventar recalcula declarația; clauzele sunt readuse pe cub.
  D9-L35-R2: `CubScena.Nota` nu compara tranzacția; acum demonstrează perechea
  numai pe aceeași tranzacție și linie, cu un singur debit și un singur credit
  la valoarea aceea. `Aloca` din Import1C refuză lotul cu sold pe două conturi
  ale aceleiași categorii. Integrala rulată independent de Codex: 3.552 /
  4.860 OK. `docs/nucleu/tr-d9-pas3-retintire.md` §6.

- **2026-10-06 — TR-D9a, pasul 5b: probele dinaintea tăierii.** Fără cod de
  produs. Ultima reconciliere registre ↔ cub e arhivată pe `.Flax.R3f`
  (precizarea owner-ului: `.Flax.M1s` e dinaintea deciziei 109): șase
  rânduri Δ, toate explicate, nicio clasă nouă. `PerfCub` în regim dual dă
  termenul A al comparației de după tăiere, cu două mostre pentru zgomot:
  1.541 privat / 1.011 bugetar OK. Scara de volum, mod nou `ModelCheck
  --scara-volum`, duce scena la 5.001.179 de postări: cititorii cresc liniar
  și rămân sub două secunde, ruta securizată nu costă, iar partidele cu rest
  și snapshot-ul de partide cresc pătratic, din forma unei îmbinări pe chei
  nulabile (8,9 s cu egalitate simplă, față de peste 10 minute).
  D9-D10 (b) rămâne al owner-ului, cu cifra. ModelCheck integral neschimbat,
  **3.552 / 4.860 OK**. `docs/nucleu/tr-d9-pas5b-probe.md`.

- **2026-10-06 — TR-D9a, pasul 5c: îmbinarea partidelor pe chei nulabile.**
  Aprobat de owner pe cifra pasului 5b. Cititorii de partide îmbină
  soldurile cu documentul deschizător într-un singur loc
  (`Partide.CuOrigine`), cu o parte a cheii ne-nulă, deci prin egalitate
  simplă; originea de document se recunoaște numai după identitate. Aceeași
  formă era în cinci locuri: snapshot-ul de partide, `PartideCuRest`,
  `Partide.Proprii`, restul sursei din `Fapte` și `DocumenteCuRest`, ultimul
  găsit de proba nouă. Pe scara de volum, la 651.644 de partide: partidele
  cu rest trec de la peste 10 minute la 6 s, reconstrucția snapshot-urilor
  de la „oprită la 30 de minute" la 48 s. Fără schimbare de schemă. Proba
  nouă `SC-CIT-110` verifică SQL-ul generat. D9-D10 (b) e re-amânat explicit,
  cu cifra. ModelCheck integral **3.554 / 4.862 OK**.
  `docs/nucleu/tr-d9-pas5c-imbinare-partide.md`.

- **2026-10-07 — TR-D9a, pasul 6: tăierea.** Motorul nu mai scrie, nu mai
  citește și nu mai ramifică pe registre și pe regim. Valoarea liniei vine din
  decizia contractului; ASM cere P = C; tipul inert și regula de contare fără
  consumator sunt refuzate; inversa fiscală se naște finală. Oracolul și
  martorii au ieșit din ModelCheck, cu verdict nominal pe fiecare aserție.
  ModelCheck integral **3.481 / 4.763 OK** (linia duală 3.554 / 4.862).
  `docs/nucleu/tr-d9-pas6-taierea.md`.

- **2026-10-07 — TR-D9a, pasul 6b: repartitorul pe capătul de terț.**
  Partenerul stă pe piciorul de terț al conturilor care cer repartitor, și
  fără partide; gardul analizei judecă numai postarea (D9-A10). Review-ul
  Codex al lotului 6 + 6b: D9-6B-R1 corectată (piciorul de terț al
  trezoreriei ales structural), închis la `ea5d0bc`. **3.505 / 4.784 OK**.
  `docs/nucleu/tr-d9-pas6b-repartitor.md`.

- **2026-10-07 — TR-D9a, pasul 6c: spargerea `Program.cs` din ModelCheck.**
  32.499 de linii în `Suita` + `Suita/*.cs`, cu secvența OK/FAIL identică pe
  ambele profiluri, după repararea a două probe nedeterministe.
  `docs/nucleu/tr-d9-pas6c-spargere-modelcheck.md`.

- **2026-10-07 — TR-D9a, pasul 7: declarațiile dispar atomic.** Entitățile
  de registru, cele trei coloane, maparea, listele XAF vechi și cazurile
  gardianului au ieșit; lanțul de migrații e `InitialCreate`; proba numelor
  interzise e activă. Lista „Registre → Postări" stă pe view-ul
  `PostareVizual` (D9-A12). **3.512 / 4.791 OK**.
  `docs/nucleu/tr-d9-pas7-declaratii.md`.

- **2026-10-07 — TR-D9a, pasul 7b: cheia de pereche pe postare.**
  `Postare.Pereche` e ordinalul mișcării sau mutării, dat de nucleu;
  invariantul e în `Conservare` și în `INV-CUB`, cu doi mutanți. Stornoul
  peste mai multe tranzacții sursă decalează ordinalele. Coloana e în
  `InitialCreate`, bazele sunt recreate. Nucleu 190/190, **3.516 / 4.795 OK**.
  `docs/nucleu/tr-d9-pas7b-pereche.md`.

- **2026-10-07 — TR-D9a, pasul 7c: regula care a decis, în explicație.**
  Explicația reține rândurile de politică consumate, cu contorul lor, în
  locul textului fix; formatul 2; API-ul arată dacă rândul s-a schimbat de la
  operare. Lista nominală a faptelor de politică e scrisă înaintea codului.
  SC-CIT-111 în ModelCheck și pe HTTP. **3.524 / 4.803 OK**.
  `docs/nucleu/tr-d9-pas7c-versiunea-politicii.md`.

- **2026-10-07 — TR-D9a, pasul 8: închiderea; decizia 110 scrisă.** Pe baze
  din `InitialCreate`: integrala **3.524 / 4.803 OK**, nucleu 190/190,
  `refuzuri.ps1` 318/318 de două ori, TPH și drift zero. `PerfCub` după
  tăiere: 1.010 / 1.540 OK; comenzile de document fac cu 6 până la 16 comenzi
  SQL mai puțin și durează cu 11–24 % mai puțin, iar niciuna nu face mai multe. Operarea și lista
  postărilor probate în browser. Import1C pe ianuarie rulează pe cub, cu
  reconcilierea neverde: contractul 1 arată dublarea reclasificării de către
  nota-punte. Verdictul restanțelor e scris. Decizia 110 așteaptă aprobarea
  owner-ului, iar review-ul advers Codex al închiderii e cerut: felia nu e
  închisă până atunci. `docs/nucleu/tr-d9-pas8-inchiderea.md`.

- **2026-10-08 — TR-D9b, pasul 0: măsurătorile dinaintea deciziei 111.**
  Cele șase puncte din §„Înaintea deciziei", fără decizie și fără cod de
  produs. Importul Flax pe ianuarie și scena de volum refăcute pe schema de
  azi; `ScaraVolum.sql` adus la zi. Prototipul nucleului cu repartitor unic:
  195 / 195. D394 identic cu partenerul în blocul fiscal. Constatările noi
  sunt linii în 111. `docs/nucleu/tr-d9b-pas0-masuratori.md`.

- **2026-10-10 — Decizia 111 aprobată; contractul C111 propus.** Owner-ul a
  aprobat (a)–(f) și (i) în forma precizată după pasul 0. Coordonatele se
  implementează în felia C111, înaintea TR-D9b; locul intern al faptului
  fiscal nu se persistă (111-r1); 552.00.00 declară cont propriu; 110-r3 e
  închisă. Contractul e pin-uit pe Q1, Q2 și Q4; pașii 0–1 sunt aprobați,
  pasul 2 așteaptă review-ul Codex. `docs/nucleu/c111-coordonate-contract.md`.

- **2026-10-10 — C111, pasul 0: linia de bază.** Integrala **3.578 / 4.859
  OK**. Import Flax pe ianuarie în 7 min 53 s, 81.774 de postări, 10
  verificări picate. Comanda `ModelCheck --amprente` scrie sumele pe chei
  naturale; două importuri dau amprente identice, iar trei postări modificate
  de mână sunt prinse pe toate secțiunile. SAF-T L nu se generează pe
  ianuarie 2025. `docs/nucleu/c111-pas0-linia-de-baza.md`.

- **2026-10-10 — C111: review-ul advers Codex al contractului, reconciliat.**
  C111-R1…R12 verificate în cod și acceptate. Amendamentul 1 (coloane
  paralele) e retras: premisa lui era greșită. S1 e rescrisă ca o singură axă
  analitică, cu filtrele HTTP fixate; D5 are patru forme noi; S10 e nouă;
  regula de oprire cere amprenta analitică și așteptări pe fiecare formă.
  `docs/nucleu/c111-coordonate-contract.md`.

- **2026-10-10 — C111, pasul 1: poarta coordonatelor.** `Postare.Partener` și
  `Postare.Gestiune` poartă diagnosticul `C111`, eroare de compilare în modul
  în afara `Cub/Citiri`, a scriitorului și a listei de coloane; 14 rânduri din
  9 fișiere au trecut pe cititori numiți. Integrala **3.579 / 4.860 OK** (+1
  proba nouă), amprentele bazei de import identice cu pasul 0, drift zero.
  `docs/nucleu/c111-pas1-poarta.md`.

- **2026-10-10 — C111, pasul 1b: așteptările dinaintea formei noi.**
  `--amprente` are trei secțiuni analitice (repartitor de sold, partener și
  loc fiscal, contrapondere), calculate pe forma de azi prin derivarea din D5.
  Martorul A → B: cele cinci secțiuni vechi nu văd un repartitor intern
  schimbat, secțiunea analitică îl vede. Catalogul `scenarii/C111.md` are 22
  de rânduri specificate. `docs/nucleu/c111-pas1b-asteptari.md`.

- **2026-10-10 — C111: runda a doua a review-ului Codex, reconciliată.**
  Codex a închis R1–R8 și R11, a ținut R9 deschisă și a adăugat R13–R15.
  Owner-ul a hotărât C111-Q9…Q11: terțul comercial SAF-T e numai repartitorul
  extern (S11), locul fiscal vine din laturile tipate ale documentului (S12),
  decontul cu terț pe linie și partidă trece după S10. Contractul, 111 (b) și
  (c) și catalogul (27 de rânduri) sunt aduse la zi. Niciun cod schimbat.

- **2026-10-10 — C111: runda a treia a review-ului Codex, reconciliată.**
  Codex a închis R9 și R13–R15 ca direcție și a adăugat R16: azi `Terti.Capat`
  rescrie partenerul fiscal pe capătul cu partidă. Ținta rămâne 111 (b);
  efectul e numit S13, cu scena SC-C111-28. Domeniul lui S11 e precizat pe
  recensământul bazei de referință: 559 de postări fără partidă pe conturi cu
  rol comercial, toate din note pe unitate internă. Niciun cod schimbat.

- **2026-10-10 — C111: review-ul de contract închis de Codex.** R16 închisă,
  nicio constatare de contract deschisă. Închiderea nu certifică
  implementarea. Pasul 2 pornește.

- **2026-10-10 — C111, pasul 2: forma nouă a postării.** Nucleul, cubul,
  declaranții, cititorii, snapshot-ul, lista de postări și HTTP-ul sunt pe
  `Repartitor`, `Contrapondere`, `PartenerFiscal` și `Loc`. Lanțul de migrații
  e din nou o singură migrație. Integrala: 3.625 / 5.029 OK, zero FAIL, pe
  baze create din ea; nucleul 199. La implementare au apărut C111-Q12 (S14,
  poziția fără fișă), Q13 (rândul fără terț) și Q14 (`Loc` și fără fapt
  fiscal; 111 (b) amendată). `docs/nucleu/c111-pas2-forma-noua.md`.

- **2026-10-10 — C111, pașii 3–5: felurile pe cont, S7 și S8, perechea.**
  `Cont.FeluriRepartitor` înlocuiește flag-ul R; gardul judecă prezența și
  felul; seed-ul bugetar declară cele 20 de conturi (C111-Q15). Tranzitul
  viramentului rămâne fără repartitor, iar consumul intern la DSC nu mai scrie
  contrapondere de client. Fișa contului și registrul jurnal citesc
  contrapartida din pereche (D9-r4 închisă). Integrala după pasul 5: 3.660 /
  5.062 OK. `docs/nucleu/c111-pas3-feluri.md`, `c111-pas4-s7-s8.md`,
  `c111-pas5-pereche.md`.

- **2026-10-11 — C111, pasul 6: uneltele.** Import1C pe forma nouă, cu
  amprentele liniei de bază identice pe ianuarie 2025; `ScaraVolum.sql` și
  PerfCub pe forma nouă; costul scrierii măsurat pe aceeași bază, cu și fără
  cele două chei străine (C111-Q17); scara de 5 milioane de postări. Scara
  transversală a prins citirea perechii fără margine în registrul jurnal.
  `docs/nucleu/c111-pas6-import.md`, `c111-pas6-scriere-si-volum.md`.

- **2026-10-11 — C111, pasul 7: clientul și închiderea.** Clientul e pe o
  singură axă de repartitor; fișa are filtrul `faraTert` (C111-Q18) și banda
  de sinteză (C111-Q19); indexul `(TranzactieId, Pereche)` înlocuiește indexul
  pe tranzacție, iar registrul jurnal îmbină perechea în SQL (C111-Q20). Pe
  import registrul jurnal scade de la 21 s la 0,25 s; la 5 milioane rămâne la
  6,6–8,0 s. `docs/nucleu/c111-pas7-client-si-inchidere.md`.

- **2026-10-11 — C111 închisă: review-ul advers Codex al închiderii.**
  C111-F-R1: de la pasul 3, terțul documentului nu mai ajungea pe piciorul
  conturilor „partener sau niciunul”; corectat (`ContFapt.DeclaraRepartitor`,
  scena SC-C111-33). C111-F-R2: sinteza fișei însumează pe ușa securizată ce
  vede rolul; owner-ul a precizat 110 (e) (C111-Q22), fără schimbare de cod.
  Codex a reverificat și a închis amândouă. Integrala finală: 3.678 / 5.080
  OK, zero FAIL. Decizia 111 (a)–(e) și (i) e implementată; urmează TR-D9b.

- **2026-10-11 — C111 mersă în main; guvernanța documentației unită cu ea.**
  Felia C111 a intrat în main prin PR #36. Branch-ul `docs-guvernanta`
  (decizia 118) a unit main-ul: decizia 111 are antetul kitului, cu
  `Închide: 110-r3, D9-r4, 111-r1, 111-r2`; rândurile aduse de felie în
  registru au arie; fișierele feliei din `docs/nucleu` au `Tip`. Baza de
  import pe forma nouă e `.Flax.C111P6` (ianuarie 2025), scena de volum e
  `.C111Vol`. 118-r7 (`Latura` → `Sens`) trece la TR-D9b.

## Starea din `CLAUDE.md` la 2026-10-10 (mutată prin 118 j)

> Textul secțiunii §Stare așa cum era înainte ca `docs/stare.md` să fie generat; din acest moment §Stare e un paragraf, iar rezumatele stau aici și în `stare.md`.

**Produs (înghețat pe funcții, 90m).** Toate tipurile de document au felie
prin API și client; singurul rămas e BPR (rezervat, 19). Refuzurile de acces
sunt uniforme pe REST și OData și măsurate (80). Listele XAF Blazor pe
`Server` / `ServerView` / `Client` după 85. Imobilizările au modul propriu cu
registru append-only (87). Perioada e lanț, închiderea e comandă, soldurile se
materializează doar pe perioadele de referință și se citesc prin
`Motor/SolduriService` (88). Cele trei ierarhii sunt TPH cu discriminatorul
mapat `ClrType` (89).

**Nucleul (90, amendat de 91).** `nou/Atlas.Conta.Nucleu` e motorul pur (BCL,
zero pachete, teste de proprietate). Declarația fluxului stă în
`Module/Declaratii/` (`IDeclarant` numit prin `Document.Declarant()`, driverul
`Contractare`, laturile ca structură prin `Document.Laturi()`). Cubul e
persistat în `Module/Cub/` (`Tranzactie` / `Postare`, POCO, tabelă
partiționată pe `Spatiu`, migrații scrise în SQL) și e singurul efect contabil
al operării (110): registrele, regimul dual și oracolul lor au ieșit la TR-D9a.
Declară: BCS, FCT, PLT, INC, BTR, FCL, NTC, ASM, LDI, NIR, DEC și PIF/AMO/CAS pe
ambele profiluri, plus DSC, ITV, RDC, RLF și DVI numai pe privat; un tip fără
declarant sau fără politica cerută pe profil e refuzat (`TIP_FARA_DECLARATIE`);
LDI acoperă Magazie/Marfuri și lanțul Folosință în gestiune reală (093),
cu Custodie explicit neacoperită. NIR conex postează diferența față de
recepția istorică a facturii, cu proveniență păstrată la corecție (098, 099). Deschiderea generică detaliază soldul inițial
prin loturi și partide, fără dublare, cu refuz atomic al diferențelor (094).
Fișa imobilizării este citită din cub de AMO/CAS și API Imo (097).
Citirile contabile/stoc/partide, fiscale și SAF-T și snapshot-urile lor
sunt pe cub (103, 105). Scrierea în cub e serială per bază (108), postarea
poartă ordinalul perechii ei, iar explicația persistată pe tranzacție reține
regulile de politică consumate, cu contorul lor (110). Corectitudinea o poartă
catalogul și invarianții interni ai cubului (`INV-CUB`).

**Proba supremă (91, 2026-09-22)** e catalogul de scenarii
`docs/nucleu/scenarii/`: așteptări scrise de mână din regula contabilă, ciclul
complet per tip, lanțuri transversale, pe ambele profiluri. Import1C e unealtă
de migrare, înghețată, la sfârșit (091-r4); clona Flax e sursă de întrebări
prin recensământ, nu gate. „Rotund" (regula de oprire a PoC-ului) e 091 (g). Restanțele au
patru stări (091 (k)); lista `activă` din `restante.md` e singurul backlog al
PoC-ului.

Cronologia, cifrele și contractele feliilor: `docs/decizii/istoric-plan-de-lucru.md`,
`docs/nucleu/*-contract.md`. Un rezumat de felie nu se mai adaugă aici (91l).

**Următorul pas**: felia C104 e închisă (104-r1) și mersă în main (PR #8).
R6 este implementat și verificat (103h/i, 2026-09-28), cu review advers închis:
`docs/nucleu/tr-d8-tva-intervale-contract.md`. Proveniența fiscală a ajustărilor
rămâne delimitată în 103-r2. SAF-T L și S sunt pe cub, cu ruta pe registre
scoasă; review-ul B8 e închis (RV1.2 corectat și reverificat, decizia 105).
TR-D8 este închis: gate-ul transversal (decizia 108, contractul
`docs/nucleu/tr-d8-transversal-contract.md`, pașii 1–6) e mers în main prin
PR #15 (2026-10-04), cu review-ul implementării închis (X-RI1…X-RI4).
Felia C106 (decizia 106: regimul pe stare, liniile pe roluri, ștergerea pe
stare) e mersă în main prin PR #17 (2026-10-04), cu 106-r1 închisă; review-ul
advers al feliei e închis (C106-R1…R5, corectat prin 106 (k)). Felia M1 (decizia 107: Import1C
deschide terții per partidă din `BalantaNivel3`, stingerile anului pe partide
inițiale) e mersă în main prin PR #20 (2026-10-04), cu M1-D10 amendat și
review-ul advers închis (M1-R1…R4, M1-R3a). 107-r3 e tratată prin decizia 109
(taxa nemarcată decisă pe document × cotă și scrisă pe linie; conectorul
marchează taxa sursei; reîncărcarea nu e culegere), aprobată de owner
2026-10-04, mersă în main prin PR #21 (2026-10-05), cu review-ul advers
Codex închis (109-R1 corectată și reverificată,
`docs/nucleu/109-review-codex.md`).
TR-D9 e împărțit în TR-D9a (tăierea) și TR-D9b (unitățile). TR-D9a e închisă
(2026-10-07): contractul `docs/nucleu/tr-d9-taierea-contract.md` cu
amendamentele 1–3, pașii în `docs/nucleu/tr-d9-pas*.md`, închiderea în
`tr-d9-pas8-inchiderea.md`. Decizia 110 e aprobată de owner; review-ul advers
Codex al închiderii e închis (D9-F-R1…R4, IZ-R1, IZ-R2), iar proba PerfCub pe
clone diferite e acceptată de owner ca abatere de la „aceeași bază”.
Mersă în main prin PR #22 (2026-10-07). Bazele de dezvoltare sunt recreate din
`InitialCreate`. Pasul 0 al TR-D9b e măsurat (2026-10-08,
`docs/nucleu/tr-d9b-pas0-masuratori.md`); baza de import la zi e
`.Flax.D9bP0`, scena de volum e `.D9bVol`. Urmează hotărârea owner-ului pe
decizia 111 (propusă, neaprobată), apoi contractul TR-D9b.
Felia fiscală
103 este implementată și verificată; snapshot-ul de stoc este pe cub;
contractul și probele sunt în `docs/nucleu/tr-d8-citiri-contract.md` și
`docs/nucleu/scenarii/CITIRI.md`.
Felia de curățenie C102 este închisă (2026-09-25, mersă în main prin PR #8):
compatibilitatea cu bazele de dezvoltare a ieșit, invarianții cubului rulează
în ModelCheck (`INV-CUB`), migrațiile s-au comprimat în `InitialCreate`, bazele
s-au recreat, 101-r1 e închisă; rămân 102-r4/r5. TR-D8 — portarea consumatorilor pe intrările comune ale
cubului, după DEC și PIF/AMO/CAS (095, 097), cu scenarii numerice înaintea
implementării. Review-urile adverse LDI, NIR, Deschidere, DEC și IMO sunt făcute
(2026-09-24); corecturile se aplică pe felii peste `94ddfa8` (098).
Urmărirea partidelor este separată de rolul comercial SAF-T (096). Transformarea ASM și absorbția
temporară Δ în regimul dual sunt aprobate și implementate:
`docs/nucleu/tr-d7b-asm-transformare-contract.md` (ASM-B2…B7).
Review-ul ASM este aplicat; delimitarea contabilă (a)/(h) este aprobată
de owner numai pentru regimul dual (T-r15, D8-B4).
RDC, RLF și DVI au cataloage independente
în `docs/nucleu/scenarii/`; DVI păstrează baza distinctă în Carte=Fiscal
conform `docs/nucleu/tr-d7b-dvi-baza-fiscala-contract.md`.
Identitatea partidei include partenerul (092);
probele și limitele pasului 3 sunt în `docs/nucleu/scenarii/NTC.md` și `ITV.md`.
Primele loturi
independente pentru tipurile deja pe cub sunt verificate; ciclurile complete
rămân deschise conform fișierelor tipurilor. Contractul feliei:
`docs/nucleu/tr-d7b-tipuri-ramase-contract.md`, amendamentul 091 sub „Pașii”.

## Starea din `CLAUDE.md` la 2026-10-14 (mutată prin 118 j)

Felia curentă: adoptarea kitului de guvernanță (decizia 118, activă din
2026-10-10). Unealta `docskit` (118-r2) are `gen`, `check` și `metrics`
adoptate, cu linia zero a codului în `docs/docskit.baseline.json`; `build`
e încă deschis; 118-r1 (curățenia antetelor normalizate)
așteaptă `/docs-curatenie` (118-r4). Pe motor, felia C111 e închisă și
mersă în main (PR #36, 2026-10-11): decizia 111 (a)–(e) și (i) e
implementată, postarea are un singur repartitor de sold. Contractul
[TR-D9b](../nucleu/tr-d9b-contract.md), felia unităților, este redactat pe
forma nouă a postării; primul pachet este originea partidei. Redenumirea
tipurilor din pasul 2 este implementată, cu validarea în
[raportul pasului](../nucleu/tr-d9b-pas2-redenumiri.md). Persistența originii
este implementată și verificată în [pasul 4](../nucleu/tr-d9b-pas4-origini.md).
[Pasul 3](../nucleu/tr-d9b-pas3-referinta.md)
are catalogul A verificat pe ambele profiluri și referința privată măsurată
până la 5.001.179 postări. [Pasul 5](../nucleu/tr-d9b-pas5-comparatie.md)
are comparația A/B verde, review-ul independent Codex și reverificarea
Claude, care a corectat recunoașterea ușii de sistem; pachetul e comis la
cererea owner-ului (2026-10-13). Pachetul următor, corecțiile valorice pe
lot, are [contractul TR-D9b-CV](../nucleu/tr-d9b-corectii-valorice-contract.md)
și decizia 119; delimitarea pasului 2 e închisă și perimetrul aprobat de
owner (2026-10-14), implementarea (pasul 3) e în lucru. [Pasul 6](../nucleu/tr-d9b-pas6-sens.md)
al contractului TR-D9b este făcut: membrii care poartă debitul sau creditul
și coloana de pe `Postare` se numesc `Sens`, printr-o migrație incrementală
(120). [Pasul 7](../nucleu/tr-d9b-pas7-senstva.md) aduce membrii de tip
`SensTva` la numele întreg, cu trei coloane de mapări și șase câmpuri de
API, iar [pasul 8](../nucleu/tr-d9b-pas8-sensstingere.md) face la fel
pentru `SensStingere`, cu parametrii de interogare. Pachetul originii și
pașii 6–8 sunt merse în main prin PR #38 (2026-10-13). Unde suntem, cu cifre, e
generat în [docs/stare.md](../stare.md); cronologia feliilor e în
`docs/decizii/istoric-plan-de-lucru.md`.

## Starea din `CLAUDE.md` la 2026-10-15 (mutată prin 118 j)

Pe guvernanță, stratul de scop ([scop](../scop.md),
[catalogul funcționalităților](../functionalitati.md); PR #44) și cele 16
arii ale stării curente (decizia 121, PR #45) sunt în main din 2026-10-15;
paginile sunt rescrise pe subiect, iar limitele aduse la catalog (121-r1,
121-r2, 121 e). Rămân 121-r3 (sondajul owner-ului pe aria rândurilor din
registru) și 121-r4 (mutarea între arii a pasajelor lăsate pe loc, la prima
felie care atinge aria). Tot pe guvernanță
rămân 118-r1 la `/docs-curatenie` și `build` din 118-r2. Pe motor, felia
deschisă e [TR-D9b-CV](../nucleu/tr-d9b-corectii-valorice-contract.md)
(decizia 119), adică etapa E1 (F-CVL-01…06): pasul 3, mecanismul pe FCT, e
în main (PR #41); pasul 4 (explicația în DTO, culegerea XAF și API,
cititorii) e implementat pe branch-ul `tr-d9b-cv-pas4`; urmează pasul 5
(regresie și review advers). Unde suntem,
cu cifre: [docs/stare.md](../stare.md); cronologia:
`docs/decizii/istoric-plan-de-lucru.md`.

## Textul scos din stare-curenta la 2026-10-15 (mutat prin 121-r1 și 121-r2)

Pasajele scoase verbatim din paginile de stare curentă când au fost rescrise pe
subiect (121-r1) și când limitele și acoperirea au fost aduse la catalog
(121-r2): jurnal de execuție, cifre de mediu, stare a feliilor, reguli ieșite.
Forma curentă a regulilor e în `docs/stare-curenta/`.

### README.md — Tranziția în curs și Acoperirea funcțională

#### Tranziția în curs

**Implementat:** cubul e singurul scriitor și singura sursă contabilă;
registrele, regimul dual și oracolul lor au ieșit (110). Citirile contabile,
de stoc, de partide, fiscale și SAF-T sunt pe cub (103, 105, 108). Catalogul
de scenarii e proba supremă a motorului (091).

**În lucru:** felia C111 schimbă coordonatele postării (111), înaintea
TR-D9b, felia unităților. Pasul curent e în `CLAUDE.md` §Stare; domeniul și
oprirea feliei, în contractul ei; acoperirea, în catalog.

#### Acoperirea funcțională

Tabelul rezumă ariile; rând cu rând, cu ținta, etapa și scenariile, acoperirea
stă în [catalogul funcționalităților](../functionalitati.md).

| Arie | Disponibil în implementare |
|---|---|
| Documente | FCT, FCL, NIR, DSC, BTR, BCS, LDI, PLT, INC, DEC, NTC, ASM, RLF, RDC, DVI, PIF, CAS |
| Închidere TVA | Previzualizare, generare, regenerare și comenzi asupra rezultatului ITV |
| Imobilizări | Fișa cu situația la dată și registrul, punerea în funcțiune (intrare, modernizare, revizuire), ieșirea, amortizarea lunară generată cu cifra contabilă, fiscală și deductibilă, politica de amortizare, regulile de deductibilitate și catalogul HG 2139/2004 |
| Raportare | Sold stoc, balanță, balanță pe plan, fișă de cont, registru-jurnal, jurnale TVA, decont TVA, D300 și D394 |
| Fișiere fiscale | SAF-T L și S, cu sumar, avertismente, reconciliere și XML |
| Nomenclatoare în React | Parteneri, produse, societate și sincronizarea individuală a partenerului cu ANAF |
| Configurare în React | Implicite TVA, tipuri TVA, implicitul tipului de document, mișcări SAF-T, scadențe, numerotare, închidere TVA, politici de amortizare, reguli de deductibilitate, verificarea profilului și istoric de audit |
| Import | Import1C prin motor, legături de migrare și reconciliere; conectorul legacy este prototip |

Existența unei rute nu înseamnă CRUD complet: DSC se generează din factură,
iar ITV și AMO din comanda lunară. Particularitățile fiecărui tip sunt descrise în
[documente](../stare-curenta/documente.md). Acoperirea formularelor fiscale
și modulele absente sunt precizate în [limitele curente](../stare-curenta/limite-curente.md).


### limite-curente.md — pagina întreagă

#### Domeniu și operare

- Corecția valorii lotului (TR-D9b-CV, pasul 3): acoperă numai lotul
  recepționat prin FCT cu NIR conex fără diferențe și consumat prin bonuri.
  Lotul cu transfer, retur, asamblare, inventar sau deschidere e refuzat.
  Corecția cu TVA și factura mixtă sunt refuzate. Consumul dintr-un an
  anterior anului corecției e refuzat; produsul nu are închidere de an
  (CV-r2).
  Corecția cu o ieșire ulterioară activă nu se retrage și nu se stornează
  până la neutralizarea ieșirii. Linia de corecție se creează numai prin
  model: culegerea XAF și `WriteDto` nu o expun încă. Linia explicată și
  DTO-ul ei nu poartă încă deciziile valorice. Exportul SAF-T al lunii cu o corecție rămâne pe
  refuz (CV-r1). (`docs/nucleu/tr-d9b-corectii-valorice-contract.md`)
- Felurile de repartitor (C111-D4): numai seed-ul bugetar declară, pe cele
  20 de conturi cu defalcarea R; 14 dintre ele n-au mișcare în legacy 2026,
  deci felul lor e din natura contului, nu din date. Pe 461.01.09 angajatul
  nu e admis (confirmat, C111-Q21). Planul privat nu declară nimic, iar contul
  nedeclarat nu judecă repartitorul (lumea deschisă, C111-r2). Terțul stă pe
  capătul de terț numai când contul declară feluri de repartitor
  sau urmărește partide — pe un cont nedeclarat și fără partide (446 privat)
  rămâne absent. Pe contul care admite lipsa (437.01.00, 437.02.00,
  803.00.01) terțul cunoscut de document stă pe capăt; lipsa e admisă numai
  când documentul n-are terț pentru el (C111-F-R1). Pe conturile de cheltuieli și venituri terțul nu e axă de
  sold: apare numai ca `PartenerFiscal`, pe postarea cu fapt fiscal. Pe
  trezorerie, când regula de contare numește ambele conturi explicit,
  capătul propriu n-are repartitor; dacă contul lui cere repartitor
  documentul e refuzat (SC-PLT-09), nu reparat cu terțul.
  Cerința de material citește numai produsul lotului, nu și materialul fix al
  unei reguli de contare. BTR, ASM și PIF nu sunt păzite (D9-r1).
  (`docs/nucleu/c111-coordonate-contract.md`, D4; `tr-d9-pas2-probe.md`, G2)
- La decont, partenerul fiscal e latura numită de politica de TVA, adică
  titularul; furnizorul de pe bon nu există pe postare. (C111-r1)
- Partenerul fiscal lipsește când politica de TVA nu numește o latură (sursă
  `Explicit` sau `TipMaterial`): baza intră în D394 la „fără partener”. Pe
  cele șase politici din seed latura numită e terțul documentului. (S13)
- Invariantul coordonatelor: ramurile „partener fiscal fără fapt”, „loc fără
  temei” și „fapt fără loc” ale lui `VerificaCoordonate` nu se pot aprinde
  cât există `CK_Postare_BlocFiscal`; le ține constrângerea, nu invariantul.
  Stornoul cu doi originali cu parteneri fiscali diferiți nu e produs de
  declaranții de azi: stornoul inversează și postările atribuite altui
  document, dar singurul scriitor al atribuirii (împerecherea) produce
  transferuri de partide fără fapt fiscal. Forma nu e probată; invariantul 5
  îl țin mutanții `FISCAL-INVERSA-*`. Invariantul 5 compară partenerul fiscal
  și locul inversei numai pe postările cu fapt fiscal: locul unei inverse pe
  partidă fără fapt fiscal (C111-Q14) e păstrat de copierea stornoului, nu
  verificat de `INV-CUB` (reverificarea Codex a pasului 2, 2026-10-11). Felul
  necunoscut al unui repartitor e probat numai la recitire.
- PIF/AMO/CAS scriu cubul, iar situația fișei se citește din el (095, 097).
  Storno
  rămâne limitat la luna documentului (087g), inclusiv în corecție.
  Nominalizarea și inversarea suportului folosesc un blocaj tranzacțional
  comun pe bază; corectitudinea concurentă este probată, debitul concurent
  pe volum mare nu este măsurat. Perf 2/51 privește operandul, nu întregul
  flux de validare al frunzelor. Referințele la suport sunt păstrate în
  istoricul inversat chiar dacă suportul eliberat este ulterior anulat;
  auditul durabil după anularea fizică rămâne în delimitarea TR-D9 (091j).

- Deschiderea generică (094) este o comandă de motor și scrie cubul; cititorii
  și evaluarea iau soldurile de acolo (X-D2). Intrarea păstrează analiza și
  valuta, dar stingerea unei partide inițiale în valută este refuzată explicit
  până la TR-D9b;
  soldurile nedetaliate nu se împart pe mai multe analize în aceeași cheie
  de control. Ștergerea concurentă a nomenclatoarelor referite nu este
  serializată de comandă (review Deschidere, MINOR-3).
  Stingerea prin motor a unei partide inițiale în lei și
  inversarea ei prin storno PLT sunt probate; nu există încă o ușă UI/HTTP
  pentru această comandă. Conectorul 1C a ieșit din îngheț numai pe deschidere
  și pe trecerea 2 a lunii (107): deschiderea trece prin această comandă, iar
  stingerile anului pe partide inițiale se scriu în cub; fără deschiderea în
  cub Import1C nu mai rulează pe modelul cu stocul citit din cub (107 j).
  Stingerile prin facturi rămân în afara trecerii 2 (107-r1), pozițiile în
  valută intră în lei și se plafonează la rest (107-r4).
- Comenzile care scriu sunt seriale per bază (X-D6): doi operatori nu scriu
  simultan, al doilea așteaptă comanda primului, oricât de străine ar fi
  documentele lor. Așteaptă și salvarea unui draft care adaugă detalii fără
  poziție. Pe scara sintetică (X-D5) o comandă durează 19–39 ms la orice
  istoric, o unitate de 11–12 comenzi sub 300 ms, un consum de 64 de linii
  110 ms și o închidere de lună 79 ms; debitul cu mai mulți operatori nu e
  măsurat. Rafinarea blocajului pe gestiune și partener e restanța X-r1. Peste timpul
  de comandă al conexiunii (30 s implicit), așteptarea iese ca refuz
  `SCRIERE_OCUPATA`; pragul nu e configurat separat de timpul comenzilor.
- Reluarea idempotentă generală a comenzilor nu este acoperită. (42f)
- Blocajul scrierii îl ia `TranzactieComanda`. O unealtă care își deschide
  singură tranzacția (`Database.BeginTransaction`) și cheamă motorul nu îl
  are; proba pe sursă refuză asta în `Module`, WebApi și Blazor, nu și în
  uneltele standalone. Deschiderea și stingerea ei îl iau oricum, la intrare.
- Baza nu verifică tipul unui rând: discriminatorul `ClrType` al documentelor
  nu are FK spre `TipDocument.ClrType`, iar un FK spre o frunză (de exemplu
  `Lot.GestiuneId`) acceptă în schemă id-ul oricărui repartitor. Pe ușa
  securizată tipul îl verifică gardianul; pe ușa de sistem (Import1C, seed)
  nimic nu-l verifică la scriere, iar abaterile le găsesc doar
  probele ModelCheck și SQL-ul din `--dump-integritate-tph` rulat după
  import. (89a, 89e)
- Închiderea unei perioade și operarea unui document în ea sunt serializate
  pe toate căile care deschid tranzacția prin `TranzactieComanda`: adaptorul
  de operare, comenzile de generare, și motorul chemat direct de uneltele
  standalone, care își asigură tranzacția dacă apelantul nu are una (097).
  (F27-D1, X-D6)
- Balanța, balanța pe plan, fișa de cont, soldul de stoc, soldurile pe loturi,
  soldul unei chei, alocarea FIFO, gardianul de sold negativ și soldurile de
  TVA ale închiderii lunare pornesc de la ultima perioadă de referință. Trei
  citiri nu folosesc snapshot: situația imobilizărilor citește cubul integral
  al fișelor cerute (097); oracolul golirilor, care
  citește rânduri concrete, nu solduri; împerecherile și restul documentelor,
  până la felia care le datează. (F27-D3)
- Inițialul de stoc și cel de cont ale SAF-T, ca și soldurile terților, pornesc
  din snapshot, sub accesul complet verificat înaintea proiecției (105,
  SAFT-r4 închisă, X-D5). Rapoartele API cumulate (balanță, fișă, sold
  parteneri, partide cu rest, stoc) citesc `Vizibila` și recitesc tot
  istoricul la fiecare cerere: pe scara sintetică, 7.827 de rânduri din
  `Postare` pentru balanța de după 12 luni, față de 1.926 din snapshot. La acel
  volum recitirea e mai rapidă (8 ms față de 13 ms), deci nu există un al
  doilea read model. (104b, FZ-r1 închisă, X-D5)
- Raportul de stoc listează și capătul de consum al bonului: postarea de debit
  a BCS poartă lotul ca unitate, pe contul de cheltuială și la locul de
  consum, iar `Loturi.Postari` o ia ca poziție. Rândurile acestea cresc cu tot
  ce s-a consumat, și în raport, și în snapshot-ul de stoc. (X-D5; X-r3)
- `PartideCuRest` caută documentul deschizător al fiecărei partide parcurgând
  toate postările de partidă (`Partide.Origini`), și când soldurile vin din
  snapshot. E singurul criteriu de formă picat al scării transversale:
  rândurile atinse cresc cu istoricul. Amânat prin amendamentul owner-ului
  (2026-10-04), odată cu F27-r16, și re-amânat cu cifră la TR-D9a
  (2026-10-06): parcurgerea e liniară, 2,7 s de SQL pentru 488.733 de
  partide cu rest la 5 milioane de postări. Rămâne până la TR-D9b.
  (F27-r16, X-D5, D9-D10 (b))
- O egalitate între două surse pe coloane nulabile iese din EF cu ramură de
  nul, iar Postgres nu o poate folosi drept cheie de hash sau merge.
  `RAMURA-NUL` o prinde în tot SQL-ul pe care EF îl emite într-o rulare
  ModelCheck, deci numai pe citirile pe care o scenă le execută: un cititor
  neatins de nicio scenă nu e văzut. Proba citește forma SQL-ului, nu planul:
  cele cinci forme admise nominal sunt admise pe cheia selectivă de lângă ele,
  nu pe o cifră la volum. Un SQL scris de mână cu `IS NOT DISTINCT FROM` nu e
  căutat. `DocumenteCuRest` are forma corectată, dar nu are cifră la volum.
  (`docs/stare-curenta/dezvoltare.md`, „Îmbinările pe chei
  nulabile ale cubului")
- Scara transversală este sintetică și mică: 7.951 de postări la 12 luni. Nu
  are prag absolut. Proba din plan se evaluează cu scanarea secvențială
  interzisă, fiindcă la acest volum planificatorul o alege legitim; planul
  ales e raportat alături. Pragul pe volum real rămâne al migrării. (X-D5,
  FZ-r3)
- Scara de volum (5 milioane de postări) e tot sintetică: scena scării
  transversale multiplicată în lățime, citită de utilizatorul administrativ,
  pe Postgres neconfigurat. Nu măsoară scrierea, închiderea de lună,
  cititorii fiscali și SAF-T, nici filtrele de rând ale unui rol restrâns.
  (D9-A1)
- Rândurile integral nule dispar din rapoarte după prima închidere. O cheie cu
  debitul și creditul cumulate zero la referință nu are rând de snapshot, deci
  un cont sau un cont cu repartitor fără nicio mișcare în perioada cerută nu
  mai apare deloc în balanță, în loc să apară cu patru zerouri. Măsurat pe baza
  de import: 1.743 rânduri din 72.910 pe balanța analitică a lunii decembrie,
  toate cu inițial, rulaj și sold zero. Cifrele rândurilor rămase nu se
  schimbă, iar declarațiile oricum nu raportează soldul zero. (F27-D3)
- Citirile cumulate iau snapshot-ul prin spațiul de obiecte al apelantului. Un
  rol care ar putea citi perioada fiscală fără să poată citi tabelele de
  solduri ar primi soldul inițial zero, fără avertisment. Rolurile livrate nu
  au această formă: cine citește perioada citește și soldurile, iar un rol care
  nu vede perioadele cade pe citirea integrală a cubului. (F27-D3)
- Integritatea snapshot-ului nu e verificată la citire. Un rând de snapshot
  șters direct din bază dă o balanță tăcut greșită (inițialul scade), fără
  niciun semnal; singura detecție e reconstrucția la cerere, care raportează
  diferența și repară. F27-r12 propune memorarea numărului de rânduri și a
  sumelor scrise la închidere, verificate ca o constatare de închidere, și
  refuzul unei referințe goale cu număr memorat pozitiv. (review advers F27, 9)
- Compatibilizarea vechilor repere `RegistruTva.ScrisLa` nu este implementată:
  citirea fiscală folosește cubul greenfield și confirmarea explicită a
  depunerii; bazele de dezvoltare se recreează conform 102. (103)
- Snapshot-urile de referință se citesc fără filtrarea de securitate pe rând.
  Fișa de cont își păstrează gate-ul strict (echivalența celor două căi,
  numărată pe TOT istoricul contului, nu doar pe fereastra de după referință),
  deci un utilizator restrâns pe rânduri primește 403 ca înainte; balanța și
  soldul de stoc trec prin LINQ, unde snapshot-ul e o entitate ca oricare
  alta, iar o restricție pe rând pusă pe registru NU se propagă asupra lui.
  (F27-D3, 66)
- Reconcilierea cu sursa externă 1C NU e constatare de închidere: documentele
  neimportate într-o perioadă nu opresc și nu semnalează închiderea ei. E
  treaba conectorului, nu a mecanismului. (F27-D2, F27-r6)
- Constatarea de rest scadent citește doar documentele cu scadență culeasă sau
  implicită — facturile de intrare și de ieșire. Un document stins fără
  scadență (plată, încasare, decont, retur) rămâne în afara ei chiar cu rest.
  (F27-D2)
- Constatările de închidere listează cel mult 200 de rânduri per fel, iar
  restul intră într-un rând de rezumat cu cheie proprie, la severitatea
  familiei, care spune câte rânduri acoperă: acceptarea lui e o acceptare ÎN
  BLOC a celor nelistate, nu una pe constatări individuale. Pe o lună cu peste
  200 de rânduri de același fel operatorul vede deci cifra, nu lista.
  (F27-D2)
- Constatările de conținut se caută doar când niciun blocant STRUCTURAL nu
  stă în picioare: pe o lună cu precedenta deschisă ecranul arată doar
  blocanta lanțului, nu și ce ar mai fi de rezolvat în ea. (F27-D2)
- Dialogul de închidere din XAF Blazor arată constatările ca listă imbricată cu
  bifă. Bifa pe o constatare blocantă e dezactivată prin regulă de aspect;
  autoritatea rămâne serviciul, care refuză blocantele indiferent de ce
  s-a trimis. (F27-D2)
- Două împerecheri noi, încă necomise în același context, au o limită de
  vizibilitate în calculele bazate pe interogarea bazei. Plafoanele nu trebuie
  prezentate ca protecție completă pentru orice lot de scrieri concurente. (41d)
- Capacitatea NTC este netată pe contrapartidă și latură, nu pe cont.
  Calculul restului scade conservator legăturile existente în ambele roluri. (76-r1, 76-r2)
- Ieșirea fiscală RLF poate lăsa un reziduu valoric pe cheia de stoc golită.
  Documentele retroactive nu reevaluează ieșirile deja operate. (75a)
- Unele distribuiri ASM nu sunt reprezentabile exact la precizia prețului.
  Cazurile refuzate nu sunt corectate prin prețuri sau valori forțate. (75-r4, 76c)
- Golirea lotului se decide la momentul operării. Un document retroactiv nu
  reevaluează ieșirile deja operate și poate lăsa reziduu valoric pe un lot
  fără cantitate; eliminarea lui e reevaluare (TR-D9b). O asamblare culeasă
  înaintea unei alte ieșiri din același lot poate fi refuzată la operare cu
  `ASAMBLARE_NEBALANSATA`, până la redistribuire. (D9-D3)
- Valoarea de pe linia unui draft cu ieșire evaluată din sold e estimarea
  `cantitate × preț de intrare`, nu promisiune; dry-run-ul nu întoarce
  valoarea decisă. Ea se vede pe linie după operare și în explicația
  tranzacției. (D9-D3, D9-D12)
- Folosința păstrează gestiunea reală pe lanțul FCT/NIR/BTR/BCS/LDI.
  Custodie rămâne refuzată la LDI; Gratuit și injectivitatea SAF-T rămân
  TR-r7. (093, LDI-B3)
- Nu există un al doilea scriitor cu care cubul să fie comparat: corectitudinea
  valorică o poartă catalogul de scenarii și invarianții interni ai cubului
  (echilibrul, conservarea transferului, explicația, taxa și valoarea liniei).
  Conservarea singură nu dovedește cifra. (D9-D7 a)
- Recepția unui NIR nu declanșează automat completarea DSC pentru facturile
  cu acoperire parțială. Există comanda de generare suplimentară pe FCL. (37g, 38d)
- NIR manual postează pe cub la net, fără fapt fiscal; nu acoperă avizul pe
  408 sau factura ulterioară pe un lot recepționat anterior (TR-r4/B-r5).
- Fluxurile de rezervare, comenzi de vânzare și distribuire a aceleiași
  facturi din mai multe gestiuni nu sunt acoperite complet. (37g, C1a)
- Retururile nu au un flux general propriu de compensare; se folosește NTC. (46f, 76g)
- Declarația vamală nu este document stins: taxa în vamă se plătește printr-o
  plată către biroul vamal, fără împerechere și fără rest pe document; soldul
  pe partener al contului de taxă nu se citește pe dimensiune (latura de terț
  a plăților poartă contul propriu). Taxele vamale și accizele nu intră în
  costul de achiziție; comisionarul care refacturează taxa nu are flux propriu.
  Anularea unei facturi legate la o declarație operată nu se refuză; starea
  facturii se arată. (86-r1, 86-r2, 86-r10, 86-r11, 86-r13)
- Imobilizările: activele în curs (231) nu au punere în funcțiune care
  postează; reevaluarea are doar locul rezervat în registru; transferul nu
  lasă rând explicit; degresiva AD2 și amortizarea pe unități de producție,
  ajustările pentru depreciere, leasingul și obiectele de inventar în
  folosință nu sunt acoperite; cheltuiala nu se repartizează pe mai multe
  centre de cost; legătura ieșirii cu factura de vânzare nu este evidență;
  deductibilitatea valorii rămase la ieșire nu are regulă; eligibilitatea
  metodei fiscale pe categorie e doar documentată; patru poziții-părinte din
  catalog nu au bandă de durată; fișele din 1C nu se migrează încă; nu
  există acțiune XAF de generare a amortizării. Brutul fiscal zero pe linia
  de punere în funcțiune nu se poate exprima; anularea unui eveniment din
  luna unei amortizări deja operate e refuzată deși luna nu depinde de el;
  două generări simultane ale aceleiași luni lasă două drafturi care se
  blochează reciproc. Lookup-urile XAF ale fișei și ale liniei PIF nu sunt
  filtrate pe natură, stare și loc; clasificarea se caută doar pe denumire;
  mesajele gardienilor scriu numele membrilor enum. (87, F26-r1…r22)
- D300/D394 au atribuiri distincte. Confirmarea depunerii din aplicație
  păstrează formularul/perioada, amprenta faptelor exportate și momentul confirmării;
  nu transmite declarația la ANAF și nu verifică recipisa. Indicatorul
  D394 de rectificativă este disponibil pentru exact o lună calendaristică;
  D300 folosește regularizări. `TaxInformation` SAF-T citește faptele pe luna
  exportului contabil. Ruta publică L citește cubul (S1, S2); validarea
  XSD/DUK (S0) certifică GL-ul, facturile și plățile lunilor scenei.
  Compensarea prin notă contabilă nu apare în Payments (SAFT-r1). Valuta și
  TVA la încasare pe plăți nu există în cub, iar exportul le refuză. Factura
  în valută se declară în RON, cu avertismentul `FacturaInValuta` (B-r6). Citirea
  alocărilor filtrează postările contabile pe `Unitate`, prin indexul
  `(Unitate, Data)` (X-D5).
  Pe scara sintetică (SAF-B8: k ∈ {1, 4, 16, 64} unități/lună × m ∈ {0, 6, 12}
  luni de istoric, proces rece și cald) L emite 43 de comenzi SQL și S 26,
  constant. Istoricul nu se citește din `Postare`. Durata și alocările cresc
  liniar, iar execuția maximă pe server la k = 64 este 5,2 ms. Planul pe volum
  real rămâne nemăsurat, fiindcă nu există bază de volum după C102 (FZ-r3).
  După o inserare masivă, până la autoanalyze, citirea faptelor fiscale poate
  degenera în Nested Loop (SAFT-r5). Pragul de ~43 ms văzut pe mașina de
  dezvoltare aparține proxy-ului de porturi Docker Desktop, nu produsului.
  Lotul pe două conturi de stoc nu are azi producător; dacă apare, poziția
  îl separă prin `ProductType`, iar garda de injectivitate refuză cheile
  duplicate (SC-SAFT-12 parțial).
  Validatorul se rulează numai pe lună întreagă (antetul cu o singură lună).
  (103, D8-B8, S1-R8, S0-R4, S2)
- Corecția unui document operat: motivul decide efectul fiscal, nu contarea.
  Reclasificarea pe 1174 a erorilor semnificative din exerciții anterioare
  rămâne notă contabilă manuală — motorul nu judecă semnificația. (F27-r1)
- Corecția moștenește integral gardienii stornării: un document cu împerecheri
  în fereastra DESCHISĂ cere ștergerea lor înainte (cele dintr-o perioadă
  închisă se inversează singure la storno), iar un document cu conex operat sau
  cu latură pereche operată e refuzat exact ca la storno. Comanda nu relaxează
  nimic. (F27-D6, F27-D8)
- Partidele deschise nu poartă scadența și nu se grupează pe vechime:
  scadențarul și aging-ul sunt aceeași listă cu scadența alături, felie de
  raportare proprie. (F27-D7, F27-r2)
- Soldul partenerului nu apare în lookup-urile de partener din culegere:
  există ca ecran și ca rută, nu ca o coloană `Sold` pe `Partener`. (F27-r7)
- `sold-parteneri` nu desface rândul „fără terț”: sumele pe repartitor
  intern, pe cont propriu și fără repartitor ale unui cont vin împreună;
  defalcarea lor e în balanța analitică, iar postările lor în fișa cu
  `faraTert`. Rândul e probat cu sume pe
  repartitor intern și pe cont propriu, nu și cu sume fără niciun repartitor.
  Locul (`Postare.Loc`) și partenerul fiscal nu sunt axe de sold și nu au
  filtru pe nicio proiecție. (C111-Q13, C111-Q8)
- `ReturClient` intră în proiecția de rest, dar rândurile lui nu apar:
  creanța unui retur e negativă după operare (venit stornat), iar filtrul
  `Rest > 0` o taie. Împerecherea unui retur rămâne pe calea directă
  (serviciu / XAF). (F27-D7)
- `Imperechere` rămâne legătura explicită: panoul stingerilor îi afișează
  rândurile, iar restul, totalul și candidații vin din `Cub.Citiri.Partide`.
  Singurul calcul care citește sumele legăturilor este
  `Partide.NominalizataLibera`, pe o pereche de documente (101, X-D2).
- Proba X-D2 este sintactică. Un rezultat netipizat derivat dintr-un registru
  se urmărește la apelanți numai dacă membrul e declarat în `Purtatori`; un
  purtător nou, nedeclarat, nu e văzut.
- Lista de evidență a cubului (`PostareVizual`): eticheta lotului e codul
  produsului și data lotului, deci două loturi ale aceluiași produs din
  aceeași zi au aceeași etichetă; nu există cod de valută (`Postare.Valuta`
  n-are nomenclator, view-ul nu-l etichetează); rândul n-are navigație spre
  tranzacție (view fără referințe EF, respinse de owner); sortarea pe o
  coloană de cod scanează tot cubul (9,1 s la 5 milioane de postări, fără
  index), iar pe containerul Postgres de dezvoltare, cu `/dev/shm` de 64 MB,
  sortarea paralelă pică (`could not resize shared memory segment`); costul
  e raportat fără prag în `docs/nucleu/tr-d9-pas7-declaratii.md`. Coloanele
  repartitor, partener fiscal, loc și contrapondere sunt probate în
  ModelCheck (`STR-VIZUAL-*`), nu și în browser; contraponderea se afișează
  cu numele enumului, fără diacritice (`Tert`). (D9-A12, S5; 2026-10-10)
- Cu pregătirea automată a comenzilor, citirile de sold pe o listă de loturi
  (`Fapte.SolduriLoturi`, `Loturi.VerificaSoldIntermediar`) primesc un plan
  generic de cinci ori mai lent (0,45 → 2,2 ms pe apel): cu limitele de dată
  necunoscute, planificatorul alege indexul pe dată în locul celui pe
  `(Unitate, Data)` și filtrează lotul după citire. Coborârea filtrului de
  loturi pe rând nu schimbă planul. Import1C câștigă totuși cu ea pornită;
  hosturile nu o au. (2026-10-09)
- Loturile inițiale cu același produs, gestiune, cont și dată se departajează
  în FIFO pe identificatorul lotului, dat de cel care le creează. (116-r2)
- Reproductibilitatea Import1C e măsurată numai pe ianuarie 2025 (două
  rulări identice pe chei naturale); lunile următoare nu sunt măsurate.
  (116; 2026-10-09)
- Nominalizarea partidei pe linia de notă nu e în culegere: câmpurile sunt
  ascunse în XAF și lipsesc din API. (115-r1)
- Cheia de pereche (`Postare.Pereche`) nu acoperă transformările (consumul și
  produsul sunt corespondență de grup): rândul fără pereche n-are
  contrapartidă în fișa contului și în registrul jurnal. Stornoul cu mai multe surse decalează ordinalele
  surselor după prima, deci ordinalul stornoului nu e literal al
  originalului. (D9-A2; 2026-10-07)
- Contrapartida costă în registrul jurnal circa 4 µs pe rând la volum: luna
  de 888 de mii de rânduri pe baza de 5 milioane de postări se citește în
  4,8 s, față de 1,3 s fără contrapartidă. Planul caută perechea fiecărui
  rând prin indexul pe `(TranzactieId, Pereche)`; indexul a scos costul
  tranzacțiilor mari (deschiderea importului), nu și căutarea pe rând. O
  îmbinare prin hash pe postările intervalului n-a fost încercată.
  (`docs/nucleu/c111-pas6-scriere-si-volum.md`,
  `docs/nucleu/c111-pas7-client-si-inchidere.md`, 2026-10-11)
- Fișa contului și registrul jurnal compun SQL-ul de mână în jurul surselor
  din LINQ, fiindcă egalitatea pe `Pereche`, nulabilă, nu iese din LINQ fără
  ramură de nul. `Pereche` nulă înseamnă „fără pereche”; valoarea 0 nu e
  folosită. (C111-Q20)
- Proiecțiile contabile trec prin ușa securizată: un rol cu citire restrânsă
  pe `Postare` (criteriu de rând) primește balanța, fișa cu sinteza ei,
  registrul jurnal și soldurile pe terți calculate numai din rândurile pe
  care le vede, fără refuz și fără semn că cifra e parțială. Dreptul unic cu
  403 e numai pe rutele ușii de sistem. Niciun rol din seed nu are criteriu
  de rând pe `Postare`. (110 (e) precizată, C111-Q22)
- La volum, fișa unui cont întreg cu 160–200 de mii de rânduri în lună se
  citește integral în circa 3 s.
  (`docs/nucleu/c111-pas7-client-si-inchidere.md`)
- Import1C a rulat pe forma postării din C111 pe ianuarie 2025, cu amprentele
  liniei de bază identice (`docs/nucleu/c111-pas6-import.md`). Bazele de
  import și clonele de măsurare de pe 5444 sunt pe schema veche și cer
  `--recreeaza`. Pe 5446 nu mai există bază de import sau de volum: cele ale
  C111 (`Atlas.Conta.Import1C.Flax.C111P6`,
  `Atlas.Conta.ModelCheck.Privat.C111Vol`) erau pe schema de dinaintea
  TR-D9b și au fost șterse la 2026-10-14, cu bazele de referință `.C111P0`;
  o măsurare nouă le reface. Pe ele indexul perechii fusese schimbat prin
  SQL, fără recreare (abatere numită de la 102b, C111-Q20). Cifrele de mai
  jos sunt de dinaintea ei. (C111-D9, pașii 6 și 7)
- Import1C rulează pe cititorii cubului, dar reconcilierea lui nu e verde.
  Pe ianuarie 2025 (2026-10-07): 15.232 de documente, zero eșecuri de import,
  `INV-CUB` verde; contractul 1 are trei conturi fără explicație (371, 3028,
  303: nota-punte a reclasificării dublează ASM-ul care postează acum,
  110-r2), iar contractul 5 are 5 partide inițiale neexplicate, neatribuite.
  Sensibilitatea contractelor 1 și 3 nu mai are probă proprie (`--sabotaj` a
  ieșit), iar unealta n-are un mod de reconciliere fără scriere.
  (D9-D11; `docs/nucleu/tr-d9-pas8-inchiderea.md` §6)
- Refuzul de acces pe cifrele din cub e fraza generică de citire pe `Postare`:
  nu spune dacă lipsește dreptul pe tip sau dacă rolul are un criteriu de rând
  ori de membru. (D9-D9)
- Refuzurile de ștergere pentru lot, tip de TVA și fișă citesc cubul, dar
  textele lor numesc încă registrul. (D9-A6)
- O postare existentă nu se rescrie: garda e la salvare (`POSTARE_MODIFICATA`),
  pe starea `Modified` a entității, deci nu vede o actualizare făcută direct în
  SQL. Ștergerea postărilor la anulare rămâne permisă motorului. (D9-A5)
- Refuzul tipului fără politică pe profil (`TIP_FARA_DECLARATIE`) apare la
  dry-run și la operare, nu în regimul pe stare al documentului: butonul de
  operare rămâne activ pe un tip inert. (D9-D5)
- Produsul nu are o listare a postărilor pe lot sau pe partidă. Regula
  listării N-r8 e fixată pe intrările comune și probată pe ele; singura
  listare pe lot din producție este mișcarea de stoc SAF-T, care poartă
  eticheta felului. O fișă a lotului sau a partidei se construiește peste
  `Loturi.Postari`, respectiv `Partide.Postari`, fără filtru pe fel.
- Probele de concurență (X-D6) pun cele două comenzi în coada blocajului
  ținut de scenă și le lasă să ruleze în ordinea cozii. Dovedesc rezultatul
  serial și așteptarea pe conexiuni distincte; nu măsoară debitul. Pe ușa
  HTTP și pe contextul securizat e probat numai refuzul `SCRIERE_OCUPATA`
  și ce nu ia blocajul (`scriere-ocupata.py`), nu matricea rezultatelor
  seriale.
- `SCRIERE_OCUPATA` iese 422, ca orice refuz de domeniu. Un client nu îl
  deosebește de un refuz definitiv decât după cod. (108)
- Explicația deciziei (X-D4) există numai pe tranzacțiile scrise de un
  contract. Împerecherea, desfacerea, stingerea de deschidere și deschiderea
  nu au decizii de explicat și nu au explicație. Un document operat înaintea
  migrației n-ar avea explicație, iar invariantul l-ar refuza: baza se
  recreează (102b).
- Refuzul `EXPLICATIE_ACCES_INCOMPLET` se decide pe tip, nu pe tranzacția
  cerută: un criteriu de rând pe `Postare` sau pe o frunză de document refuză
  explicația oricărei tranzacții, inclusiv a uneia pe care criteriul n-o
  atinge. E prețul regulii „fără proiecție parțială” (SAF-D4).
- `DECLARATIE_INVALIDA` (S-r11) e probată pe coaja comenzii, cu o cantitate
  în afara scării ținută în memorie. Un declanșator persistabil pe ușa HTTP
  nu a fost construit; răspunsul ușii e cel al oricărui refuz de declarație
  (422 la operare, listă la dry-run), neprobat separat pentru acest cod.
- Explicația reține care rând de politică a decis și dacă s-a schimbat de la
  operare, nu și cum arăta atunci; nomenclatoarele care decid contarea
  (contul tipului de material, conturile tipului de TVA, steagurile contului)
  nu se rețin (D9-r3). Contorul e `OptimisticLockField`: nu-l mișcă o scriere
  SQL directă sau restaurarea bazei, iar un rând șters și recreat apare ca
  dispărut. Politica de amortizare se reține pe orice fișă atinsă care are
  rând, fără să spună care cont a venit din ea și care din cub. (D9-A8;
  2026-10-07)
- Explicația repetă câte două `ContRezolvat` și unitatea întreagă pe fiecare
  linie. Măsurat în X-D5: 907 octeți pe linie, 58.066 pentru un consum de 64
  de linii.
- Invariantul explicației nu recitește istoricul: verifică soldul citit al
  unei partide stinse numai ca prezență și ca plafon al alocărilor. Un sold
  citit alterat în sus trece. La fel, o valoare declarată e comparată cu
  postarea ei și cu sursa permisă declarantului, nu recalculată din linia
  sau recepția de origine. (X-RI1)
- Matricea `refuzuri.ps1` probează ușa explicației numai cu cei patru
  utilizatori ai ei: 200 și 404. 403 `EXPLICATIE_ACCES_INCOMPLET` cere un
  rol cu citire restricționată și rămâne al probei `explicatii.py`, care
  lasă un bon stornat pe baza ei. (X-D8)
- `documente-cu-rest` rămâne proiecția scumpă, iar partidele nu schimbă asta:
  pe baza de import costă 181 ms filtrat pe o contrapartidă (171 ms cu lanțul
  desfăcut — diferență în zgomot), 423 ms nefiltrat pe grilă și 220 ms pe
  forma nefiltrată fără paginare. Cauza e cea de la 59 — uniunea tuturor
  documentelor operate ale celor opt tipuri, cu filtrul abia în `WHERE`-ul
  exterior — plus legăturile ca tabele derivate: agregatul `Imperecheri` intră
  prin `Nested Loop Left Join` cu două `Seq Scan` și 2,18 M de rânduri respinse.
  Corelarea legăturii ELIMINĂ `Seq Scan`-urile și duce panoul filtrat la 82 ms,
  dar mută costul pe calea nefiltrată neplafonată (220 ms → 1,02 s), pe care o
  consumă constatarea de rest scadent la închidere — deci nu e fixul; cifrele
  și variantele respinse sunt în `docs/api/p5-perf-masuratori.md` §Felia 27.
  Niciun index nu lipsește. (F27-r16)
- Ținta de 150 ms a lui `documente-cu-rest` a fost calibrată pe baza `Privat`
  (147 ms la pasul 6, 131 ms la re-măsurare); cele 181 ms sunt de pe baza de
  import, alt set de date. Pe baza pe care a fost pusă, ținta nu e încălcată.
- Fișa de cont și balanța analitică coboară cu închiderile (187 → 122 ms,
  respectiv 254 → 210 ms, A/B pe aceeași bază), dar rămân peste ținta de
  100 ms. La fișă costul nu mai e proiecția — 7 ms —, ci cadrul unei cereri
  (58 de instrucțiuni de securitate per ObjectSpace, hidratare, serializare),
  deci ținta se ratează din afara feliei (F27-r14). La balanța analitică e
  cardinalitatea cheii `Cont×Repartitor` (71.167 de grupe); ridicarea lui
  `work_mem` NU ajută, o înrăutățește (136 ms la 4 MB cu agregare paralelă și
  sortare pe disc, 183 ms la 64 MB cu hash aggregate secvențial) — ținta e de
  re-calibrat (F27-r15). (F27-D3)
- Balanța analitică pe decembrie are 71.167 de rânduri cu snapshot și 72.910
  fără: cheile cu debit ȘI credit cumulat zero nu se scriu în snapshot.
  (F27-D3)
- Coloana „Dată” a împerecherii apare în panoul de stingeri și în lista XAF,
  dar nu există listă proprie de împerecheri în clientul React: desfacerea se
  face din panoul documentului. (F27-D8)
- Legătura de corecție nu apare în coloanele listelor de documente și nu se
  poate filtra pe ea: se vede pe ecranul documentului, ca bandă, și în grupul
  „Corecție" al DetailView-ului XAF. (F27-D6)
- Data înregistrării: documentele generate
  (AMO, ITV, DSC și NIR autogenerat) moștenesc data înregistrării sursei sau
  ultima zi a lunii; editabilitatea ei pe ele nu e decisă. Plata autogenerată
  din factura de intrare face excepție: data ei este cea culeasă, iar data
  înregistrării i se normalizează la ea, nu la a facturii. Coloana „Data
  înregistrării” lipsește din coloanele listelor de documente ale clientului
  React și din DTO-urile lor de listă. (F27-D4, F27-D5, F27-r9)
- Regimul pe stare nu promite refuzurile care depind de valori (sold de
  stoc, poziție fără fișă) și, la stornare și corecție, pe cele care depind
  de data cerută: perioada ei, luna la imobilizări cât luna documentului e
  deschisă, nominalizările stinse ulterior. La stornarea unui document cu
  legături vii în perioade închise, regimul nu numără nominalizarea
  partenerului pe aceeași partidă când ea nu vine din transferul legăturii,
  inclusiv la legătura manuală care acoperă exact o nominalizare făcută la
  operare; refuzul `PARTIDA_CU_DEPENDENTI` rămas după desfacerea legăturilor
  apare la comandă. (106i, 106k, 106-r7)
- Salariile, execuția bugetară completă, producția pe rețete, împărțirea pe
  cofinanțări și contabilitatea multivalutară nu sunt module complete în
  produsul curent. Importul 1C nu echivalează cu un import bancar
  operațional general. (9, 21, 31f)

#### Fiscalitate și nomenclatoare

- `SDD`/`SFD` nu au cod SAF-T de achiziție; `N9` poartă codul de livrare al
  rândului 10.1 în timp ce maparea D300 îl pune pe rândul 11. (84-r1, 84-r2)
- Acoperirea politicii de mișcări SAF-T (D17-V1) se probează într-o singură
  direcție: fiecare pereche tip × categorie pe care cubul o produce în scena
  SAF-T are politică, iar scena atinge cele opt tipuri care mișcă stocul
  (FCT în locul NIR: pe cub recepția mișcă pe factură, NIR-ul conex egal nu
  mișcă). Direcția inversă, „politica n-are perechi fără mișcare”, nu se mai
  măsoară de la tăierea regulilor de stoc fără consumator: perechile pe
  Magazie și NIR/Marfuri rămân în politică fără scenă care să le producă.
  Limită consemnată pentru decizia 110. (TR-D9a pasul 7, D9-D8, 2026-10-07)

- R6 verifică intervale configurate, nu toate condițiile legale ale unei cote.
  Proveniența fiscală a ajustărilor RDC/RLF/reduceri rămâne neacoperită:
  diagnosticul nu deduce factura fiscală din lot. Nu ține consumul/restul
  avansului și nu refuză depășirea lui; referințele parțiale repetate sunt
  permise. Nu repară automat taxele sau calificările istorice. (103-r2)
- Pe FCT/FCL, baza negativă și taxa culeasă pozitivă sunt încă admise în
  L3; abaterea este diagnosticată aritmetic. Un refuz pentru semne
  incompatibile și înlocuirea lui `ILinieCuAvans` ca proxy pentru linia de
  factură rămân de tratat la următoarea atingere a validării. (103-r3)
- Raportul R6, măsurat pe scara sintetică la 128 de documente fiscale în
  lună: 33 ms pe ușa nesecurizată, 104 ms și 24,9 MiB alocați pe cea
  securizată (verificarea de permisiune per obiect), 17 comenzi SQL,
  independent de istoric. Accesul leneș la `Tranzactie` a ieșit. Rămân
  nemăsurate rezolvarea tipului și a politicii per draft, citirea fără
  perioadă și deschiderea lookup-ului de avans. (103h, X-D5)
- Implicitele Privat `FCT|RLF/UE → TI21` și `DVI → IMP21` sunt nedatate.
  Înainte de 2025-08-01, tipurile nu sunt eligibile; rămâne alegerea
  explicită cu explicație conform R6-B5. Adăugarea rândurilor istorice
  datate în seed cere alegerea owner-ului. (103h/i, R6-B5)
- D300 și D394 sunt proiecții pentru cazurile implementate, nu acoperirea
  integrală a formularelor. Nu există export XML D300/D394. (69-r6, D4-r13)
- Prorata, ajustările, cazurile fiscale speciale și toate secțiunile
  informative D394 nu sunt implementate integral. Datele neacoperite rămân
  explicite în avertismente și în sumele neincluse. (36f, 69-r2, D4-r7)
- Marcajele ANAF pentru TVA la încasare nu constituie implementarea întregului
  mecanism contabil. Fluxurile de exigibilitate amânată și conturile
  intermediare aferente nu sunt acoperite complet. (36f, D4-r9)
- Rotunjirea TVA pe document × cotă acoperă taxa nemarcată (109). Taxa
  culeasă rămâne suma liniilor, fără regularizare pe grup.
  Cubul păstrează valorile și calificarea fiscală, dar nu un snapshot complet al
  identității și clasificării istorice a partenerului. (36f, D4-r1)
- Implicitele TVA pentru cumpărări extra-UE și anumite cumpărări de la
  neînregistrați nu au politici distincte în seed-ul privat. Fallback-ul la
  ancora documentului poate cere alegere explicită la culegere. (81-r1)
- SAF-T acoperă profilurile L și S implementate și testate. Nu acoperă toate
  variantele de contribuabil, multivaluta, proprietarii terți ai stocului și
  toate mișcările logistice. Validarea XML nu dovedește completitudinea
  datelor economice. (73-r2, 73-r16, 74-r1)
- Nu există catalog NC complet și nici conversie automată sigură pentru
  orice text UM legacy. Precizia câmpurilor de preț exportate trebuie
  distinsă de precizia internă a prețului și de cea a cantității. (73-r4, 73-r5, 73-r11)
- Nu există integrare operațională e-Factura.

#### ANAF și configurare

- Limitarea ritmului ANAF este pe cerere; nu există coordonare globală între
  cereri simultane. (72-r1)
- Proveniența adresei nu este păstrată separat pentru fiecare componentă.
  Completarea golurilor poate produce o adresă cu surse mixte. (72-r2)
- Nu toate informațiile ANAF despre înregistrare/radiere sunt consumate în
  domeniu. Fluxul individual poate păstra o salvare efectuată înaintea unei
  erori ulterioare; mesajul transportului nu este dovada unui rollback global. (72-r3, 72-r4)
- Sincronizarea ANAF în lot este disponibilă prin API, fără ecran React
  dedicat pentru întregul flux. (77-r3)
- În formularul regulii de contare, dimensiunile `Unitate*` sunt doar de
  citit: setul OData `Unitate` nu are controller. Întoarcerea din panoul
  „Explică" în grilă nu focalizează rândul. (84-r4)
- Captions: `SursaCont` și mai mulți membri ai politicilor nu au
  `[XafDisplayName]`; ecranele poartă caption-ul în cod. (84-r5)
- Selectoarele XAF de tip TVA nu filtrează încă `Activ`; unele grile de
  politici afișează FK-uri brute și nu au configurarea vizuală completă. (81k, 81-r7)
- Explicația e a configurației pe o linie ipotetică, nu planul unui
  document; gate-ul ei de citire e pe tip, nu pe obiect, iar codul de tip se
  rezolvă înaintea gate-ului (400 pe cod inexistent pentru orice rol). Nu
  există import/export general al configurației. Raportul de profil nu are
  categoria „rând de seed lipsă"; recrearea unui rând de seed șters rămâne
  manuală. Marcajul istoric nu distinge toate intervențiile manuale
  anterioare, iar alinierea atinge acum și `Cont.DimensiuniObligatorii` pe
  rândurile marcate. (81k, 81-r3, 83-r1, 84-r3, 84-r6, 84-r8)
- Rolul `Configurator` e al release-ului: permisiunile lui se reaplică la
  fiecare seed, iar pe RELEASE nu există un utilizator cu acest rol până nu îl
  atribuie administratorul; navigația XAF nu e configurată. (83-r5, 84-r7)
- Administrarea perioadelor fiscale nu are un flux React complet. Conturile
  sunt expuse prin OData pentru citire, nu prin editor contabil general. (53i, 79-r3, 70g)

#### Client, API și livrare

- Un deep-link vechi cu `gestiuneId` deschide raportul nefiltrat, fără
  refuz: nici clientul, nici serverul nu mai cunosc parametrul. Legătura
  „Înapoi la balanță” din fișă apare și când fișa e deschisă din soldurile pe
  terți. (C111 pasul 7)
- Paginile React de detaliu ale documentelor sunt înghețate: câmpurile
  adăugate modelului după 2026-09-27 nu apar acolo, iar convențiile de
  selector rămase neadoptate (BTR, `TipMaterial`) nu se mai aplică. Culegerea
  curentă e în XAF Blazor. Scoaterea sau generarea lor din metadate se decide
  după PoC. (104d, 104-r4)
- Cheia cache-ului `byKey` combină proiecțiile de selecție și expandare într-o
  formă care nu distinge toate combinațiile posibile. Nu se presupune că
  orice proiecție nouă este sigură fără verificarea cheii. (77b)
- Invalidarea cache-ului nu garantează reîncărcarea imediată a tuturor
  widgeturilor deja montate. Selectoarele pot necesita reîmprospătare. (77-r5)
- La unele citiri OData, transportul DevExtreme reduce eroarea la status și
  pierde mesajele `Erori`. Unele mesaje de transport rămân în engleză. (80-r1, 70-r5, 81-r9)
- Refuzurile asupra navigațiilor OData expandate nu au acoperire completă
  prin probe HTTP pe roluri restrânse. (80-r4)
- Calculul ASM `distribuie-valoarea` citește prețuri prin context nesecurizat
  fără un drept separat de citire pe Lot. Autorizarea documentului nu
  echivalează cu această permisiune asupra nomenclatorului. (80-r7)
- Controllerul de rapoarte rămas din scaffold poate întoarce 404 cu corp gol,
  în afara formei comune `Erori`. (80-r3)
- Metadata OData descrie modelul expus și nu este filtrată ca o listă de
  înregistrări după permisiunile utilizatorului. (55g)
- În XAF Blazor niciun view nu folosește `InstantFeedback`/
  `InstantFeedbackView`; activarea cere o măsurătoare peste prag și probe în
  browser. (85-r2)
- Proprietățile nemapate ale documentelor (`Total`, valorile de
  livrare/recepție) sunt disponibile în DetailView, nu în liste; o altă
  proprietate nemapată care ar ajunge într-o listă este refuzată de ModelCheck.
  Mărimea paginii grilelor nu este calibrată; `RegulaStoc_ListView` poartă un
  override `Server` redundant cu opțiunea aplicației. (85-r3, 85-r4, 85-r5)
- În modul `Server`, referințele coloanelor ascunse rămân în interogarea
  paginii (FCT: 33 de join-uri măsurate pe maparea TPT; pe TPH join-urile
  moștenirii au dispărut, iar navigațiile coloanelor ascunse rămân — 89);
  scoaterea lor din modelul view-ului nu e făcută. Gruparea încarcă primele rânduri ale fiecărui grup, iar `Refresh`
  execută pagina de două ori. (85-r6, 85-r7, 85-r8)
- Un layout salvat de utilizator poate ascunde toate coloanele unui view; pe
  `ServerView` celulele rămân goale până la Refresh după re-bifarea
  coloanelor. Grila Detalii a unei facturi importate poate arăta coloana
  `Produs` goală (neverificat dacă e de date sau de afișare). (85-r9, 85-r10)
- Găzduirea clientului și API-ului pe aceeași origine este contractul de
  livrare. Hostul are `UseStaticFiles`, dar proiectul WebApi nu include
  copierea automată a build-ului React și fallback-ul rutelor SPA.
  Publicarea cere și configurarea JWT pentru mediul țintă. (43e, 55g)
- Reluarea parțială a importului cu `--continua` poate afecta interpretarea
  reconcilierii. Validarea completă cere o bază și un set de referință
  controlate, cu starea reluării cunoscută. (75-r3)



### verificare.md — Validarea feliilor

TR-D9b, pasul 2: tipurile CLR sunt `UnitateSold` și `Sens`; proprietățile,
coloanele și forma JSON a explicației își păstrează numele. Nucleul a
trecut 199/199 la 2026-10-12; integrala are **3.678 bugetar / 5.080 privat
OK**, zero FAIL, `run-verificari/20261012-115040-608/`. Condițiile rulării
offline sunt în [raportul pasului](../nucleu/tr-d9b-pas2-redenumiri.md).

TR-D9b, pasul 3: [catalogul D9B](../nucleu/scenarii/D9B.md) verifică
comportamentul anterior separat de proiecția persistată introdusă în pasul 4.
Regresia `D9B,CITIRI,DESCHIDERE,PLT,NTC` are **800 bugetar / 927 privat OK**,
zero FAIL, cu `INV-CUB` pe 13 scene/profil și zero reziduu,
`run-verificari/20261012-125206-497/`. Referința A acoperă 48 combinații de
citire până la 5.001.179 postări și scrierea cu 2/51 partide pe baza dedicată
privată `.CodexD9bA`. Reproducere prin `verifica.ps1 -Suita D9bReferinta
-Profil Privat -Sufix .CodexD9bA`; numai comenzile, pe drafturile păstrate,
prin `-Suita D9bScriere`. Cifrele, artefactele SQL și limita copiilor
sintetice sunt în [raport](../nucleu/tr-d9b-pas3-referinta.md).

TR-D9b, pasul 4: originea partidei este persistată în `OriginiPartide`,
cu cititor direct, vizibilitate pe suportul propriu și refuz CRUD pe ușa
securizată. Migrația este `20261012114235_OriginiPartide`; bazele de probă
au sufix `.CodexD9bOrigine`. Proba HTTP, prin `verifica.ps1 -Suita D9bHttp
-Profil Privat -Sufix .CodexD9bOrigine`, pornește hostul izolat pe5093,
baza `Atlas.Conta.Http.Privat.CodexD9bOrigine` pe5446, seed privat și
utilizatorii reali. Rulează matricea `refuzuri.ps1 -Baza` completă și
`origini-partide.py`. Integrala are **3.756 bugetar / 5.158 privat OK**,
zero FAIL (`run-verificari/20261012-150800-674/`); HTTP **327/327** plus
SC-D9B-10 verde (`run-verificari/20261012-150627-604/`), nucleu **199/199**.
Origini reziduale zero, mutanți și `RAMURA-NUL` verzi. Condițiile sunt în
[raportul pasului 4](../nucleu/tr-d9b-pas4-origini.md).

TR-D9b, pasul 5: **48/48 perechi A/B egale**, toate pragurile verzi pe
aceeași bază `.CodexD9bB`, până la 5.001.179 postări. Seria de sistem
rulează pe ușa nesecurizată a hosturilor; restul integral scade acolo de la
11.952,96 la 3.664,04 ms. Scrierea cu 51 partide este 63,5928 ms față de
limita 78,2313 ms, fără rezervă dovedită: măsurarea nu desparte variantele.
Reproducere prin `verifica.ps1` cu suitele `D9bComparatie`, `D9bScriere`,
`D9bRaport`, secvențial, pe o bază B nouă. După corecția ușii de sistem din
reverificare, integrala are **3.759 bugetar / 5.161 privat OK**, zero FAIL
(`run-verificari/20261012-201158-086/`): origine fără suport, egalitate A/B
sub rol restrictiv cu martor vizibil, originea directă pe ușa de sistem a
hostului, `RAMURA-NUL` și curățenie verzi; HTTP **327/327** plus SC-D9B-10
(`run-verificari/20261012-205322-837/`).
Protocolul, artefactele și limitele sunt în
[raportul pasului 5](../nucleu/tr-d9b-pas5-comparatie.md).

TR-D9b, pasul 6 (2026-10-13): membrii care poartă debitul sau creditul și
coloana de pe `Postare` se numesc `Sens`; migrația este
`20261013073914_SensPePostare`, incrementală (120). Bazele de dinaintea ei
o primesc prin `database update` sau se recreează; cele opt capturi SQL ale
cititorului A poartă numele nou al coloanei. Nucleu **199/199**; integrala
**3.759 bugetar / 5.161 privat OK**, zero FAIL
(`run-verificari/20261013-104100-831/`); metadata și OpenAPI fără drift.
Comparația D9b, pe o bază `.CodexD9bB` nouă: **48/48 perechi A = B**, 46/48
praguri (`run-verificari/20261013-104824-959/`). Cele două picate sunt
SOLDURI integral la factorul 100, unde A și B rulează același SQL; pragul
compara acolo un cititor cu el însuși. De la aceeași dată SOLDURI nu mai
are prag (D9B-D7, owner): harness-ul măsoară perechea și compară
rezultatele, iar pragurile rămân pe cele 32 de perechi ORIGINI și REST, pe
care rularea de mai sus e verde. Comparația nu a fost rerulată după
schimbarea harness-ului. Detaliile sunt în
[raportul pasului 6](../nucleu/tr-d9b-pas6-sens.md).

TR-D9b, pasul 7 (2026-10-13): membrii de tip `SensTva` numiți `Sens` se
numesc `SensTva`, cu coloanele de pe `MapariD300`, `MapariD394` și
`MapariTvaSaft` (migrația `20261013082803_SensTvaPeMapari`) și cu șase
câmpuri de API ale proiecțiilor de TVA; clientul generat și paginile React
care le citesc sunt aduse la zi. Nucleu **199/199**; integrala **3.759
bugetar / 5.161 privat OK**, zero FAIL
(`run-verificari/20261013-112954-429/`); HTTP **327/327** plus SC-D9B-10 pe
host izolat (`run-verificari/20261013-113445-406/`); `pnpm build` verde.
Detaliile și limitele sunt în
[raportul pasului 7](../nucleu/tr-d9b-pas7-senstva.md).

TR-D9b, pasul 8 (2026-10-13): membrii de tip `SensStingere` numiți `Sens`
sau `Latura` se numesc `SensStingere`, cu trei câmpuri de API; parametrii
de interogare sunt `sensStingere` pe partide și documente cu rest și
`sensTva` pe jurnalul de TVA. Fără migrație. Integrala **3.759 bugetar /
5.161 privat OK**, zero FAIL (`run-verificari/20261013-115203-880/`); HTTP
**327/327** plus SC-D9B-10 (`run-verificari/20261013-115710-093/`); nouă
probe de legare a parametrilor pe host viu, toate verzi; `pnpm build`
verde. Detaliile sunt în
[raportul pasului 8](../nucleu/tr-d9b-pas8-sensstingere.md).

Validarea curentă (C111, corecția C111-F-R1 a review-ului de închidere,
2026-10-11): integrala **3.678 bugetar / 5.080 privat OK**, zero FAIL, pe
baze create din `20261010151835_InitialCreate`; `--scenarii C111` verde pe
ambele profiluri. Urma: `run-verificari/20261011-135559-972/`. De la pasul 7,
pe arborele dinaintea corecției (3.673 / 5.075,
`run-verificari/20261011-122558-351/`): `refuzuri.ps1` 327/327 de două ori pe
bază privată nouă, drift OpenAPI zero, clientul compilează. Matricea n-a fost
rerulată după corecție: corecția schimbă numai conturile care declară feluri,
iar baza privată a matricei nu declară niciunul. Nucleul n-a fost atins de la
pasul 6 (199/199 acolo). Raportul pasului:
`docs/nucleu/c111-pas7-client-si-inchidere.md`.

TR-D9a, pasul 6 (tăierea, 2026-10-07): motorul nu mai scrie, nu mai citește și
nu mai ramifică pe registre și pe regim; oracolul registre → cub a ieșit din
ModelCheck (`CubDinRegistre`, `Normalizari`, `Comparabil`, `ReconciliereCub`,
`GatePeBaza`, `DiagnosticValoriStoc`, modurile `--reconciliere-cub` și
`--declaratie-pe-baza`, `STR-RECONCILIERE`, probele `NUC-ORACOL-*`,
`STR-NEMIGRAT`, `STR-TRANSFER-3`), odată cu acoperirile registru → cub din
`INV-CUB` și cu mutanții lor. Fiecare aserție care citea un registru are
verdict nominal (`run-nucleu/tr-d9a/pas6/lucru.tsv`): formă, oracol, regulă
re-țintită, comportament declarat (D9-D1: 1, 2, 4, 6), repartitor (17, de
rescris la pasul 6b) sau montaj mutat pe cub. Validare: integrala **3.481
bugetar / 4.763 privat OK**, zero FAIL (linia de bază duală 3.554 / 4.862);
nucleu 180/180; `--probe-sursa` 10/10; `verifica:drift` zero după regenerare
(a ieșit `ExplicatieAbsorbtieDto`). Un reziduu lăsat de o cădere în scena F27
nu e curățat de integrala următoare (PER/SOL rulează înainte și pică pe
precondiții globale); se purjează prin funcțiile celor două scene.

TR-D9a, pasul 2 (2026-10-05): gardul analizei obligatorii pe mișcările
contractului are proba lui directă, `GARD-ANALIZA` (`ProbeGardAnaliza`, pe
funcția pură, fără bază), și proba prin comandă SC-FCT-12…14. Scena
`ScenariiTaiere` (codul `X`) fixează numeric tipul care nu declară (SC-X-25),
regula de contare fără consumator (SC-X-26) și lotul după stornarea recepției
(SC-X-27). Scenele BCS, ASM, RLF, RDC și DESCHIDERE asertează și valoarea
LINIILOR, nu numai postările. O rulare-diagnostic care dezactivează un gard și
cade lasă documente în baza bugetară; scena e2e își curăță singură reziduul la
următoarea integrală, dar `--scenarii` nu, deci după o cădere se rulează întâi
integrala.

TR-D8 în lucru peste `c10d0fe`: rapoartele contabile, snapshot-ul contabil,
evaluarea operațională pe lot, pin/FIFO DSC, raportul de stoc și
raportul/snapshot-ul/citirile operaționale ale partidelor folosesc
intrările comune ale cubului. C102 (102) scoate codul de compatibilitate cu
bazele de dezvoltare: hosturile nu scanează istoria la pornire, iar
invarianții cubului rulează în ModelCheck (`INV-CUB`, `Cub.Citiri.Invarianti`).

`INV-CUB` (`tools/ModelCheck/AcoperireInvarianti.cs`) rulează în
`ScenaDocumente.Ruleaza` după scenă și înaintea purjei ei, pe faptele
produse, inclusiv stornourile. Fiecare ramură fără probă proprie are un
mutant într-o tranzacție anulată, rulat o dată per profil pe prima scenă cu
faptele potrivite: `DESCHIDERE-EGALA` (registrul istoric egal cu deschiderea
cubului trece), `DESCHIDERE`, `PARTIDE` (partidă fără repartitor), `POLITICA`
(totalul de decontare), `IMO-FISA`, `IMO-CAUZA`, `IMO-ORIGINE` (inversa unei
fișe fără `InversaDin`), `IMO-SUPORT` (transfer valoric pe fișă fără suport),
`IMO-REGISTRU` și cei șapte ai explicației (X-D4): `EXPLICATIE-LIPSA` (explicația ștearsă),
`-REFERINTA` (referința mutată pe alt document), `-STORNO` (cantitatea
inversei pe lot schimbată), `-IESIRE` (valoarea unei `ValoareIesire` +0,01),
`-DECLARATA` (`ValoareDeclarata` scoasă), `-EVALUARE` (soldul citit +1.000),
`-STINGERE` (`AlocareFifo` scoase). Review-ul implementării (X-RI1, X-RI2) a
adăugat cinci: `EXPLICATIE-MECANISM` (ieșirile evaluate redenumite
`ValoareDeclarata`, cu aceleași cifre și fără soldurile citite),
`-DECLARANT` (aceeași substituție, cu numele declarantului schimbat într-unul
care declară valori), `-SOLD-FIFO` (soldul citit al partidei stinse scos),
`-SOLD-FIFO-MIC` (soldul citit pus sub suma alocată) și `TRANSFER-CONT`
(numai contul capătului de destinație al unui transfer pe lot). Providerul
suitei și cel al Import1C poartă `EFCoreOptimisticLockInterceptor`, ca
hosturile (`AddSecuredEFCore`): contorul rândului de politică crește la
salvare și în harness, altfel SC-CIT-111 n-ar măsura ce face hostul
(D9-A8). Pasul 7b
(D9-A2) a adăugat `PERECHE-RUPTA` (o postare a perechii fără ordinal) și
`PERECHE-LIPSA` (ambele postări fără ordinal). `PERECHE-DATA` (o postare a
perechii la altă dată) ține marginea de interval a citirii contrapartidei
(C111 pasul 6). Corecția D9-F-R1 adaugă
mutanți separați pentru ordinal, cauză, valoare, valută, cantitate și latură,
apoi pierderea ambelor ordinale ale unei mutări valorice/cantitative lângă
o transformare. Aceștia apelează direct invariantul perechii, în tranzacții
anulate, ca alt invariant să nu mascheze rezultatul. SC-CIT-111 folosește
un tip de material și o regulă proprii; purja elimină numai regulile și
refuzurile de seed ale acelui tip. SC-CIT-111-IZOLARE verifică reluarea după
editare/ștergere, cu reguli BCS străine și refuzuri preexistente păstrate.
SC-X-26 urmează aceeași regulă: regula ei stă pe un tip de material propriu,
iar refuzul de seed lăsat de ștergerea prin gardian e curățat pe identitate.
Curățenia unei scene nu selectează după `DinSeed` (`ScenaDocumente.CurataPolitica`).
Ținta fiecărui mutant al perechii e aleasă determinist, după `ID`. Fiecare
disjuncție a predicatului perechii are mutantul ei (documentul, deschiderea cu
ordinal, latura și sumele transferului, pe lângă cele de mai sus). Izolarea
celor cinci din urmă e probată și invers: cu disjuncția scoasă din predicat
supraviețuiește numai mutantul ei
(`nou/tools/ModelCheck/scripts/izolare-mutanti.ps1`, pasul 8 §11). Rețeta
ține blocajul verificărilor cât timp sursa invariantului e modificată.
Forma coordonatelor (C111-D2) are doisprezece mutanți. Cinci apelează direct
`VerificaCoordonate` și cer `CITIRE_COORDONATE_INVALIDE`:
`COORD-CANTITATE-NEMARCATA` (cantitate fără lot și fără contrapondere),
`COORD-LOT-NEINTERN` (lot pe repartitor extern), `COORD-PARTIDA-NEEXTERN`
(partidă pe repartitor intern), `COORD-LOC-NEINTERN` (loc pe repartitor
extern) și `COORD-TRANSFORMARE-PE-PERECHE` (perechea unei postări obișnuite,
marcată transformare, care altfel ar ieși întreagă din evidență). Patru sunt
opriți de schemă înaintea invariantului și cer `CK_Postare_BlocFiscal`:
`COORD-PARTENER-FARA-FAPT`, `COORD-LOC-FARA-FAPT` (loc pe lot),
`COORD-LOC-FARA-UNITATE` (loc pe o postare fără unitate) și
`COORD-FAPT-FARA-LOC`; ramurile corespunzătoare ale invariantului nu se pot
aprinde cât există constrângerea. `COORD-LOC-PE-PARTIDA` e martorul: locul pe
o postare cu partidă și fără fapt fiscal trece (C111-Q14).
`FISCAL-INVERSA-PARTENER` și `FISCAL-INVERSA-LOC` schimbă partenerul fiscal,
respectiv locul inversei și cer `CITIRE_FISCAL_INVERSA`. Mutanții sunt uciși
pe ambele profiluri.
Ramurile
acoperirii registru ↔ cub, echilibrului și provenienței au probele SC-CIT-23,
SC-CIT-34 și SC-CIT-10. Acoperirea cantitativă a stocului
(`Loturi.VerificaAcoperire`, `CITIRE_ISTORIC_STOC_INCOMPLET`, X-D7 a) rulează
prima dintre acoperiri și are probele SC-CIT-103…105. Mutantul
`EXPLICATIE-STORNO` aplică aceeași abatere și rândului de registru, ca să
ajungă la ramura explicației. La final, profilul cere cel puțin o scenă verificată și,
pe integrală, toate ramurile ucise, iar purja nu lasă postări. La prima
rulare, proba a găsit trei totaluri de stins care nu corespundeau cubului:
taxarea inversă (SC-FCT-10), contul explicit fără partide (SC-DEC-10) și
factura cu avans (SC-NIR-30/avans). Totalul se scrie acum din cub (102).
Integrala după corectură: **3.142 bugetar / 4.141 privat OK**, zero FAIL,
build fără avertismente, `run-verificari/20260925-163947-708/`.

Validarea curentă (C102 cu review-ul Codex R1/R2): **3.146 bugetar / 4.145
privat OK**, zero FAIL, exit 0, build fără avertismente,
`run-verificari/20260925-174625-732/`. La închiderea C102: 3.083/4.079,
`run-verificari/c102/final-bugetar.log` și `final-privat.log`; cifra scăzuse
față de 3.107/4.103 pentru că probele compatibilității scoase au ieșit
(SC-CIT-08/24, SC-NTC-20, avertismentele seed-ului, backfill-urile).
Clientul compilează; metadata/OpenAPI/types sunt stabile la regenerare.
Decizia 101 este implementată; 101-r1, redeschisă de review-ul advers, e închisă în C102 (D-2/D-3, SC-CIT-66…68). Anterior: SC-CIT-49…65,
HTTP raport/panou cu securitate pe rând și membru peste închidere/reconstrucție,
comandă de împerechere și refuz atomic, browser raport și candidat 100/40/60.
Logurile `trd8-101-http-report2`, `trd8-101-http-payment2`,
`trd8-101-client-final`, `trd8-101-drift-final` sunt în `run-verificari/`.
Decizia 100 este implementată: SC-CIT-41…48 și lanțurile comerciale sunt
verzi pe ambele profiluri. SC-CIT-46/48 trec și prin HTTP și prin acțiunea
XAF, cu verificarea numerică a cubului; fixture-urile sunt curățate.
`nou/tools/ProbeHttp/partide-cub.py` oferă proba HTTP și opțiunea
`--prin-xaf`, care așteaptă cel mult cinci minute acțiunea din browser.
HTTP SC-CIT-25 a trecut de două ori, inclusiv după închidere și reconstrucție;
raportul de stoc este verificat prin HTTP și browser (3/10 pe contul 371).
Matricea generală `refuzuri.ps1` își creează singură subiecții (104-r5);
plafonul candidaților DVI se probează în ModelCheck.
Catalogul: [CITIRI.md](../nucleu/scenarii/CITIRI.md).
Review-ul propriu și limitele: [tr-d8-review-codex.md](../nucleu/tr-d8-review-codex.md).
Proba HTTP durabilă `nou/tools/ProbeHttp/citiri-cub.py` verifică accesul
pe rând și membru înainte/după închidere și reconstrucție, pe host privat
cu baza izolată `.CodexBCS`. Tokenurile rămân în memorie; fixture-ul și
rolurile temporare se curăță în finally.

Accesul la cub (091-r3): codul de producție (`Module`, `WebApi`,
`Blazor.Server`) obține rânduri `Postare`/`Tranzactie` numai prin
`Cub/Citiri`. Excepțiile numite sunt scriitorul (`Cub/Materializare*`),
faptul sursei recepției conexe (`Cub/ReceptiiConexe.cs`) și maparea EF
(`BackOfficeDbContext`). Consumatorii compun peste intrările comune
(filtre, proiecții, eticheta `Storno`), fără să refacă domeniul. Proba
`091-r3` (`tools/ModelCheck/ProbeCititoriCub.cs`) scanează sursa înaintea
bazei, deci rulează și fără bază.

Consumatorii citesc din `Cub/Citiri` identități numite (C111-D10): titularul
partidei (`Partide.Titular`, `Partide.Nete`, `Partide.MiscariPeTitulari`,
`Partide.UnitatiAle`), terțul comercial (`PostareJurnal.TertComercial`),
locul lotului (`Loturi.PostariCitite`), repartitorul și suportul poziției
fără fișă (`Imobilizari.PozitiiCitite`) și axa analitică
(`Contabil.Analitice`). Regula identităților e în
[domeniu-si-operare](../stare-curenta/documente.md), §„Identitățile cititorilor”.

La fel de devreme rulează probele regimului pe stare
(`tools/ModelCheck/ProbeRegim.cs`, 106e): prin reflecție, fiecare comandă
`RecordEdit` a unui controller pe `DetailView` de `Document` numește prin
sufixul ID-ului o comandă din `RegimDocument.ComenziCunoscute`; prin scanare
sintactică, nicio affordance `Poate*` din `Module/Api/` nu se calculează din
`StareDocument`, niciun controller nu poartă cheia „Stare” și niciun tip din
`BusinessObjects/` nu decide `Sterge` (106j). Catalogul
`REGIM` (`ScenariiRegim.cs`, anul 2013) rulează cu `--scenarii REGIM`.
Conformitatea regim ↔ comandă (SC-X-24, 106k) rulează în fiecare scenă:
`ScenaDocumente.Anuleaza`, `Storneaza` și `Corecteaza` citesc regimul
înaintea comenzii și numără abaterile în ambele sensuri. Un refuz lăsat
comenzii se dovedește pe starea documentului (`ProbeRegim.Clasifica`): pe
valori, pe data cerută ori pe limita 106-r7, al cărei număr îl declară scena
(`LimiteRegim`). O scenă care probează
concurența sau prima instrucțiune SQL a comenzii cheamă `ComenziDocument`
direct, nu helperul. Un gardian nou de dependență pe calea anulării sau a
stornării intră ca predicat `Motiv*` în `Motor/GardieniRetragere`; altfel
SC-X-24 cade la prima scenă care îl atinge. Probele
axei 2 (`tools/ModelCheck/ProbeLinii.cs`, 106h-1…3) rulează pe modelul XAF
real, în rularea integrală, cu registrul baseline-ului construit pe loc:
fiecare grilă tipizată de linii începe cu coloanele rolurilor în ordinea
vocabularului și ascunde rolurile nepurtate; fiecare membru `ReadOnly` e
blocat pe grilă și pe dialogul liniei; pe fiecare tip cu detaliu propriu care
calculează `Valoare` (`BazaLinie` suprascris, `Valoare` nu e intrare),
`Valoare` e `ReadOnly`. Caută rădăcinile de interogare,
`ModifiedObjects.OfType`, `CreateObject`, `Set<>`, `typeof` pe
`GetObjects`, SQL pe tabelele cubului, navigarea `Tranzactie.Postari` și
`DbContext.Postari`. Pică și pe o excepție care nu mai are acces. Scanarea e
sintactică: tipul calificat cu `N.`/`Nucleu.` e al nucleului, restul contează
ca rând al cubului. ModelCheck e probă independentă și citește cubul direct
(D8-B1). Excluderea `Transfer` și a inversei lui stă numai în
`Citiri.Contabil.Postari`, după originea `InversaDin` (N-r8). `Loturi` și
`Partide` includ transferul prin contract.

`Transfer` pe cititorii comuni (N-r8, gate-ul transversal TR-D8 X-D2): proba
`N-r8` (`tools/ModelCheck/ProbeTransferCititori.cs`) cere ca fiecare intrare
publică din `Cub/Citiri` care întoarce rânduri de cub să aibă regimul
declarat. `Contabil.Postari`, `Contabil.Jurnal` și `Plati.Postari` exclud
`Transfer` și inversele lui. `Loturi.Postari` și `Partide.Postari` le includ.
`Fiscale.Postari` și `Imobilizari.PozitiiFaraFisa` nu filtrează felul:
domeniul lor e dat de coordonate, iar regula nu li se aplică.
`FelTranzactie.Transfer` apare în producție numai în doisprezece membri
numiți: scriitorul (cinci membri din `Cub/Materializare*`), producătorul
`DeclarantAsamblare`, doi cititori comuni (`Loturi.VerificaRetragere`,
`Plati.Alocari`), trei martori (`Imobilizari.VerificaAcoperire`,
`Explicatii.VerificaAcoperire`, `Invarianti.VerificaTransferuri`) și eticheta
mișcării din SAF-T. Un consumator
care refiltrează `Transfer` e detectat (mutant). SC-CIT-100…102 numără, pe
fiecare intrare, rândurile `Transfer` și inversele lor din BTR stornat, din
împerecherea desfăcută și din ASM-ul mixt stornat.

Regula listării (N-r8, X-D7 d): jurnalul și fișa de cont nu listează
`Transfer` și nici inversa lui, fiindcă pornesc din `Contabil.Postari`.
O listare pe unitate (lot, partidă) pornește din `Loturi.Postari` sau
`Partide.Postari`, care le întorc pe amândouă, și arată felul rândului din
`Tranzactie.Fel`; inversa unui transfer apare ca `Storno`. Listarea nu
refiltrează felul (mutantul probei `N-r8`). SC-CIT-107 o probează pe
`ContabilProiectii.RegistruJurnal`, pe `FisaCont` și pe cele două intrări pe
unitate. Contraponderile Transformare (T-r14) nu ajung prin nicio intrare
comună: `Contabil`, `Partide` și `Imobilizari.PozitiiFaraFisa` aplică
`Transformare.FaraContrapondere`, `Loturi` cere unitate, `Fiscale` cere tip de
TVA, iar `Plati` pornește din `Contabil`. SC-CIT-106 numără zero pe toate
șapte, SC-CIT-15 și SC-SAFT-10 pe rapoarte, iar SC-ASM-16 probează că
diagnosticul `Comparabil` le exclude cu numărul raportat.

Accesul la legătură (X-D2, redusă la pasul 7 al TR-D9a): `Imperechere` se
atinge în producție numai prin lista nominală din
`tools/ModelCheck/ProbeCititoriRegistre.Lista.cs`. O intrare numește
fișierul, membrul și rolul. Un membru nou într-un fișier deja permis rămâne
încălcare. Proba `X-D2` (`ProbeCititoriRegistre.cs`) citește arborele
sintactic al sursei (`SursaProductie.cs`) și numără trei feluri de utilizare:
mențiunea tipului, inclusiv prin alias `using`; numele tabelei într-un
literal; apelul unui purtător. Purtător este orice membru al cărui tip
declarat conține o colecție de rânduri de legătură, plus lista declarată
`Purtatori` pentru rezultatele netipizate (`Partide.NominalizataLibera`).
Proba numelor interzise (D9-D7 b, `ProbeCititoriRegistre.NumeInterzise.cs`,
în `--probe-sursa`): în sursa C# și TypeScript din `nou/` nicio referință
de cod, pe identificator întreg, la cele patru entități de registru, la
membrii `PosteazaInCub` și `TotalStingere`, la `IDocumentCuRegistruPropriu`,
`StocService` și `CubDinRegistre`; excepțiile sunt fișierul probei și
`src/generated/`. Comentariile, literalii de cale (specificatorii de modul
TypeScript) și numele de raport păstrate de D9-D12 nu sunt referințe de cod;
zece mutanți fixează fiecare caz. (2026-10-07)
`nameof` și definiția tipului nu contează. Proba pică și pe o intrare rămasă
fără utilizare, iar mutanții îi probează detecția. În `Motor/`, `Cub/`,
`Declaratii/`, `Proiectii/`, `Api/`, `Culegere/`, `Saft/` și WebApi cele patru
registre nu mai apar deloc.

Lista nominală (40 de intrări; TR-D9a pasul 6, 2026-10-07):

| Clasa | Membrii | Rolul |
|---|---|---|
| maparea EF (6) | `BackOfficeEFCoreDbContext`: `OnModelCreating` și cele cinci `DbSet` | definiția și maparea; cele patru registre dispar la pasul 7 |
| gardian (1) | `GardianEditare.Verifica` | refuză scrierea celor patru registre pe ușile securizate (14); cade cu entitățile |
| evidență XAF (2) | `ContaUiBaseline`: `AscundeFkuriBrute`, `Imobilizari` | listele vechi ale registrelor, goale pentru documentele noi; dispar la pasul 7 |
| autorizare (1) | `PerioadeController.TipuriInsumate` | `Imperechere` între tipurile însumate de verificarea închiderii (80e); cifrele din cub le păzește citirea completă pe `Postare` (D9-D9) |
| legătură (30) | `ImperechereService`, `GardianEditare`, `MotorOperare.MotivImperecheri`, `Partide.NominalizataLibera`, `Materializare.Imperecheaza`, `ImperechereApply`, `ImperechereController`, `ImperecheriController` | `Imperechere` este legătura explicită, nu registru; rămâne |

Nu mai există scriitor dual și nici martor: clasele au ieșit din enum odată
cu codul lor (D9-D15, pasul 6); maparea, gardianul și evidența XAF au ieșit
la pasul 7, odată cu activarea probei numelor interzise (D9-D7 b).
Prima rulare a găsit un singur defect: supraîncărcarea
`TvaProiectii.IntreLuni(IQueryable<RegistruTva>)`, fără apelant de producție, a
ieșit din `Proiectii/`; oracolul pe registrul fiscal stă acum în ModelCheck.
`ImperecheriProiectii.Asignari` și martorul `RegistruTva` din `TvaProiectii`,
numite în contract, nu mai există.

Blocajul scrierii (X-D6): proba `X-D6` (`ProbeBlocajScriere.cs`) ține lista
nominală a membrilor care intră sub blocaj (21 de intrări: comenzile, cele
două uși din tranzacția apelantului și salvarea detaliilor noi). Un membru
nou care cheamă `TranzactieComanda.Incepe`/`Asigura` pică proba până e
adăugat, cu rolul lui; la fel o deschidere de tranzacție în afara
`TranzactieComanda` și a citirii declarate (`Fiscale.DeschideCitirea`) sau
cheia blocajului scrisă în alt fișier. Scena `ScenariiConcurenta`
(`--scenarii X`) probează pe captura SQL că fiecare fel de comandă începe cu
blocajul și că citirile nu îl iau (SC-X-22), apoi matricea rezultatelor
seriale pe două conexiuni (SC-X-15…SC-X-21, SC-X-23). O scenă care ține
`TranzactieComanda.Incepe` pe un ObjectSpace și cheamă pe același fir o
comandă sau o salvare de detalii noi pe altul se blochează singură până la
`SCRIERE_OCUPATA`.

`ModelCheck --probe-sursa [--lista]` rulează numai probele pe sursă (104c-S1,
091-r3, X-D2, N-r8, X-D6, 106c/d), fără bază; `--lista` tipărește utilizările reale,
din care se actualizează lista nominală.

Validarea pasului 1 al gate-ului transversal (X-D2, 2026-10-03): **3.277
bugetar / 4.485 privat OK**, zero FAIL, exit 0,
`run-verificari/20261003-173905-898/`, pe clonele `.ClaudeX1`. Clona bugetară
a cerut aplicarea a două migrații: baza-sursă `Atlas.Conta.BackOffice` este în
urma codului cu `IntervaleTvaSiAvans` și `S3CategorieStoc`.

Pasul 2 al gate-ului transversal (X-D4, 2026-10-03). Migrația
`ExplicatieTranzactie` e scrisă în SQL (S-r4): două coloane, două CHECK-uri,
FK-ul și indexul lui pe `Tranzactie`; relația e și în modelul EF, ca
inserarea și ștergerea să respecte ordinea. `Explicatii.VerificaAcoperire`
intră în lista N-r8 ca martor. Proba HTTP `nou/tools/ProbeHttp/explicatii.py`
rulează pe o clonă de unică folosință a unei baze private cu seed: bonul
rămâne stornat, deci baza nu se refolosește (rețeta:
`run-verificari/x2-expl-http.ps1`). Validare: nucleu **180/180**; integrala
**3.309 bugetar / 4.518 privat OK**, zero FAIL, exit 0, build fără
avertismente, 53 de scene per profil sub `INV-CUB`,
`run-verificari/20261003-182901-833/`, pe clonele `.ClaudeX2`; SC-CIT-99 8/8
PASS, `run-verificari/x2-expl-http/proba.log`; OpenAPI și tipurile TS
regenerate, cu schimbări numai aditive în `api-types.ts`.

Pasul 3 al gate-ului transversal (X-D6, 2026-10-03). Fără migrație și fără
schimbare de contract HTTP. Trei probe existente presupuneau două comenzi
simultan în secțiunea de scriere și s-au rescris pe modelul serial:
SC-DES-05 (a doua deschidere așteaptă și e refuzată de domeniu; indexul unic
se probează direct în bază), SC-DES-21 (așteptarea e pe blocajul scrierii)
și SC-IMO-32 (blocajul IMO străin se ține fără blocajul scrierii).
Validare: integrala **3.361 bugetar / 4.570 privat OK**, zero FAIL, exit 0,
54 de scene per profil sub `INV-CUB`, `run-verificari/20261003-211029-736/`, pe clonele
`.ClaudeX2`; `--probe-sursa` verde.

Pasul 4 al gate-ului transversal (X-D7, 2026-10-03). Fără migrație și fără
schimbare de contract HTTP. `ContaSeeder.SeedTipuriDocument` este public, ca
proba seed-ului să-l poată chema nesalvat. Validare: integrala **3.387
bugetar / 4.597 privat OK**, zero FAIL, exit 0, 54 de scene per profil sub
`INV-CUB`, `run-verificari/20261003-214312-683/`, pe clonele `.ClaudeX2`;
`--probe-sursa` verde.

Pasul 5 al gate-ului transversal (X-D5 + X-D3, 2026-10-04): vezi „Scara
transversală de perf”. Migrația `IndecsiCititoriCub` adaugă cinci indecși.

Pasul 6 al gate-ului transversal (X-D8, 2026-10-04). Fără schimbare de
producție. Validare: integrala la `ff08a8c` **3.387 bugetar / 4.597 privat
OK**, zero FAIL, exit 0, `run-verificari/20261004-081014-672/`, pe clonele
`.ClaudeX2`; nucleu **180/180**; `--probe-sursa` verde; `verifica:drift`
exit 0; probele HTTP în `run-verificari/x6-http/` (matricea 300/300 de două
ori, `scriere-ocupata.py` 7/7, `comenzi-coaja.py` 28/28, `explicatii.py`
8/8). Regula feliei: decizia 108.

Review-ul implementării (Codex, 2026-10-04) a adus patru observații,
X-RI1…X-RI4, toate corectate: mecanismul explicației ținut de declarant și
soldul citit al partidei stinse, conservarea pe cont a transferului
persistat, planul respins care invalidează măsurarea, controlul D300 pe
rezultatul D300. Validare după corecturi: integrala **3.394 bugetar / 4.604
privat OK**, zero FAIL, `run-verificari/20261004-092403-010/`;
`--probe-sursa` verde; scara completă
`run-verificari/perf-cub-20261004-093104/`, 1.526 / 996 OK, zero FAIL.
Reverificarea Codex la `7223abc` a închis X-RI1…X-RI4, cu integrala proprie
3.394 / 4.604 OK (`run-verificari/20261004-102241-857/`). TR-D8 este închis
(decizia 108) și mers în main prin PR #15.

Cititorii TVA/D300/D394/TaxInformation sunt portați prin 103. Snapshot-ul de stoc folosește cubul.
Nucleu: **180/180**, zero omise, exit 0:
`run-verificari/20260924-124628-047/rezultat.json`.
Comenzile, încercările intermediare și limitele sunt în
[DESCHIDERE.md](../nucleu/scenarii/DESCHIDERE.md), NIR în
[NIR.md](../nucleu/scenarii/NIR.md); lanțul Folosinta în
[LDI.md](../nucleu/scenarii/LDI.md); review-ul ASM în
[ASM.md](../nucleu/scenarii/ASM.md). S-r10 este reparată: purja SAF-T
șterge întâi regulile `DinSeed` atașate tipurilor temporare ale scenei.

### verificare.md — Rulare reproductibilă (`scripts/verifica.ps1`)

(paragraful întreg; ieșit: fraza „Validat la 2026-09-23: …”)

`-Suita Infrastructura` probează acest comportament fără acces la baze:
compilează o consolă izolată, verifică moștenirea modului, provoacă o
excepție necapturată și apoi pornește cu succes un al doilea proces.
Doar procesele acestei probe au termen de 30 s; wrapperul nu introduce un
timeout general pentru ModelCheck. Validat la 2026-09-23:
`run-verificari/20260923-224135-701/rezultat.json`, exit 0; căderea produce
`0xe0434352` / `-532462766`, stack trace prezent, procesul următor exit 0,
fără intervenție. Compilarea probei: zero avertismente și erori.

(paragraful întreg; ieșit: „primele … verificate (2026-09-23)”)

Refuzurile se probează pe două uși: codul stabil (`CoduriRefuz`, linia
`COD: mesaj`) pe ușa declarației (`Materializare.Refuzuri`); pe ușa entității
(`ComenziDocument.Valideaza` / `Opereaza`) validarea veche a clasei și gardienii
registrelor refuză cu text ÎNAINTEA declarantului, deci acolo se asertează
familia mesajului și absența efectelor, iar un refuz al registrului se
marchează ca atare în fișierul tipului (până la TR-D8). Cataloagele
[BCS, FCT, PLT/INC, BTR și FCL/DSC](../nucleu/scenarii/README.md) au primele
loturi independente verificate (2026-09-23), cu fixture prin documentele
reale și comenzile `ComenziDocument`; probele `NUC-*` rămân regresie. Sumele directe ale scenariului
peste cub nu înlocuiesc verificarea cititorilor comuni și a `Sold` la TR-D8.

### verificare.md — Scara transversală de perf (`scripts/perf-cub.ps1`)

(primul paragraf; ieșit: exemplul căderii, rulările și cifrele lor)

Integrala nu rulează scala: după o schimbare de regulă, scena ei se verifică
separat (la TR-D9a pasul 8 a căzut pe un ASM cu P ≠ C rămas din regimul dual).
Ultima rulare completă, pe forma postării din C111 și pe instanța nativă
(2026-10-11): `run-verificari/c111-pas6/perf-head2/`, 1.580 privat / 1.046
bugetar OK, zero FAIL, 33 de minute. Prima rulare de după C111 a picat pe
registrul jurnal, care citea toate postările ca să găsească perechea
(`docs/nucleu/c111-pas6-scriere-si-volum.md`). Rularea de după tăiere:
`run-verificari/perf-cub-20261007-145947/`, 1.540 / 1.010 OK. Față de regimul dual, comenzile de
document fac cu 6 până la 16 comenzi SQL mai puțin
(`docs/nucleu/tr-d9-pas8-inchiderea.md` §3).

(ieșit: durata scării complete)

construcția scenei, deci apar și într-o rulare restrânsă la un singur cititor. Scara completă durează circa 35 de minute și
este o rulare grea: nu se suprapune cu alta pe aceleași baze. O rulare

### verificare.md — Scara de volum (`scripts/scara-volum.ps1`)

(ieșit: duratele, data rezultatului, exemplul re-măsurării)

cu planurile. O măsurare se oprește la 10 minute, reconstrucția la 30. E
raport, fără prag. Rețeta rulează pe instanța nativă (5446), pe o clonă cu
sufix (`-PregatesteBaze` o face din baza privată de dezvoltare),
care rămâne bază de citire: liniile de document, registrele și împerecherile
nu se multiplică. Rularea completă durează circa 95 de minute și e rulare
grea; numai treapta finală (`-Factori F`) durează circa 14 minute. Pe forma
din C111, nativ, treptele ×1, ×10 și ×629 au durat 24 de minute (2026-10-11,
`docs/nucleu/c111-pas6-scriere-si-volum.md`). Rezultatul din 2026-10-06 și limitele lui:
`docs/nucleu/tr-d9-pas5b-probe.md` §3. (D9-A1) Scriptul numește
explicit coloanele fiecărei tabele clonate; proba `SCARA-COLOANE`, care
rulează în orice ModelCheck, le compară cu coloanele scrise ale bazei, ca o
coloană nouă să pice acolo, nu la următoarea rulare a scării (C111 pasul 6).

Un singur cititor se re-măsoară pe baza scării fără a o reface: procesul-copil
`--perf-cub-masura` sau `--scara-volum-reconstructie`, cu
`MODELCHECK_BAZA_SUFIX` pe sufixul bazei, pe aceeași instanță ca rețeta.
Așa s-a măsurat corectura îmbinării partidelor
(`docs/nucleu/tr-d9-pas5c-imbinare-partide.md` §2).

### verificare.md — Verificări proporționale cu modificarea (paragrafele de după tabel)

(ieșit: raportarea ieșirii uneltelor de reconciliere)

Nu există al doilea scriitor cu care cubul să fie comparat (TR-D9a, pasul 6).
Uneltele de reconciliere registre ↔ cub (`--declaratie-pe-baza`,
`--reconciliere-cub`, diagnosticul valoric pe lot) au ieșit odată cu
registrele; ultima lor rulare e arhivată în `docs/nucleu/tr-d9-pas5b-probe.md`.

(paragraful capcanelor, întreg; ieșit: blocajul „definitiv” înaintea reparării S-r10, artefactul felia 31 dispărut la T-D4.3)

Capcane măsurate ale acestor probe: o SINGURĂ rulare ModelCheck o dată — două
concurente crapă în purje și lasă reziduu (`TipuriMaterial` cu codul
`E2E-SAFT-S-TIP` plus `RegulaContare` `DinSeed` pe care seeder-ul i-o
re-atașează la fiecare rulare), care blochează definitiv rulările următoare pe
acea bază până e șters manual (S-r10). Interogările pe catalogul Postgres cer
cast explicit: `partattrs` e `int2vector` indexat de la 0, `conkey` e `int2[]`
de la 1, iar `partstrat` e `"char"` și cere `::text` înainte de concatenare,
altfel interogarea pică și oprește rularea. O clonă a bazei de import poartă
valoarea DEFAULT a unei coloane noi, nu valoarea de seed (`TolerantaTaxa` 0
contra `null`): se aliniază înainte de gate, altfel refuzurile sunt ale bazei,
nu ale codului. Gate-ul re-contractează documente pe o bază cu perioadele deja
închise; artefactul „document datat exact la sfârșitul perioadei de referință
refuzat `STOC_INSUFICIENT`" (felia 31) a dispărut la T-D4.3: citirea fără
documentul curent ia referința strict înaintea datei. Oracolul citește registrele ORDONAT pe
(document, poziția liniei, linie, id), altfel ordinea heap-ului schimbă
nominalizarea între rulări și aceeași probă alternează OK/FAIL.

### verificare.md — Documentația

(ieșit: „nu mai scrie indexul” → „nu scrie indexul”)

rulare să scrie zero. `python -X utf8 nou/tools/docs/normalizeaza-antete.py --scrie` rămâne
doar pentru normalizarea antetelor vechi; nu mai scrie indexul. (118j)

(ieșit: starea gardienilor la 2026-10-11)

ca text). Un gardian intră la `eroare` când are zero
constatări; la 2026-10-11 sunt la `eroare` `linkuri`, `generat`, `arii`,
`actualizat`, `orfane`, `termeni`, `tabele`, restul la `avertisment` cu
cifrele în 118-r1. `check`

(ieșit: versiunea uneltei și cifrele liniei zero)

gardianul `cod`. Linia zero e `docs/docskit.baseline.json` și ține, de la
docskit 0.7.0, numărul de linii al fiecărui fișier peste plafon; rescrisă la
2026-10-11: 11 fișiere C# și 6 TypeScript peste plafon, 44 și 5 comentarii
narative.


### motor.md — Contare și dimensiuni

Nomenclatoarele-dimensiune (cod funcțional, cod economic, sursă de
finanțare, proiect, angajament) derivă din baza abstractă `Dimensiune`:
cod, denumire și căutare fără diacritice. Baza e contractul pentru codul
generic (`T : Dimensiune`) și rămâne în afara modelului EF: fiecare
derivată are tabelul și coloana de căutare proprie. `Unitate` nu derivă
încă. (2026-09-13)

### motor.md — Nucleul pur și declarația fluxului (pilot BCS, PLT, FCT)

`nou/Atlas.Conta.Nucleu` ține tipurile cubului și regulile pure ale deciziei
90, fără nicio referință la EF/XAF/HTTP; singurul lui consumator e
`Atlas.Conta.BackOffice.Module` (folderul `Declaratii/`, TR-D6b). Motorul de
azi (`Motor/MotorOperare`) rămâne singurul care SCRIE: declaranții rulează
alături de el, ca probă în ModelCheck, și nu persistă nimic până la TR-D7.
Ce ține nucleul (contractul `docs/nucleu/tr-d6a-nucleu-pur-contract.md`):

(bulletul Perechea; ieșit: „Fără margine, registrul jurnal al unei luni citea toate postările bazei (C111 pasul 6).”)

- **Perechea** (`Postare.Pereche`, D9-A2): ordinalul mișcării sau mutării în
  tranzacția ei, dat de `Motor` (mișcările 1…m în `Operare`, mutările 1…k în
  `Transfer`); transformările și deschiderea n-au ordinal. `Conservare`
  (`PERECHE_INVALIDA`) cere pe fiecare ordinal nenul exact două postări cu
  aceeași cauză și cantități opuse: pe laturi opuse cu aceeași valoare
  (`Operare`), pe aceeași latură cu valori opuse (`Transfer`); stornoul
  acceptă ambele forme. Nu e coordonată: nu intră în chei de sold și
  snapshot-uri. O citesc fișa contului și registrul jurnal: contrapartida
  unui rând e contul celeilalte postări a perechii, iar rândul fără pereche
  (deschiderea) n-are contrapartidă (111 (d), C111 S9). Cealaltă postare se
  caută pe (tranzacție, ordinal) prin `Contabil.Perechi`, mărginită de
  intervalul cititorului: vizibilitatea și data sunt ale perechii, deci
  filtrul pe felul tranzacției nu se repetă. Fără margine, registrul jurnal
  al unei luni citea toate postările bazei (C111 pasul 6).

(bulletul Unitatea; ieșit: comparația cu motorul de azi și cifrele ei)

- **Unitatea** (lot = partidă = fișă): raportul = cost / curs / valoare
  rămasă ca citire; partida deschisă de un document are id determinist din
  (document, cont, partener), conform 092; identitățile istorice sunt păstrate
  la citire și stingere, fără rescrierea postărilor. **FIFO**: unitatea numită
  pe linie se consumă întâi,
  fără cădere pe FIFO, apoi (data deschiderii, documentul deschizător, id;
  114), tolerant cu rest
  întors. **Evaluarea ieșirii** pe raportul CURENT al unității, ultima
  ieșire ia restul ⇒ cantitate zero ⇒ valoare zero; față de motorul de azi
  (preț înghețat pe lot, substituit doar la golire) diferența e declarată
  și măsurată: în teste (378 din 491 goliri lasă valoare pe cantitate zero
  sub regula veche) și pe pilotul BCS (lot cu 20 buc / 300 lei și preț
  înghețat 10: consumul de 5 buc dă 50 azi și 75 în nucleu, Δ = +25;
  documentele de azi nu pot produce două prețuri pe același lot — Δ apare
  doar din deschideri/import). (N-D6, N-D7, N-r3)

(bulletul Repartizarea/TVA; ieșit: toleranța de pilot, înlocuită de politica S-D15)

- **Repartizarea** = Hamilton ierarhic, Σ = total exact, singura primitivă
  de distribuție. **TVA**: taxa se decide și se rotunjește pe document ×
  cotă, se postează per linie prin Hamilton peste |net|, pe fiecare semn
  separat; taxa dată pe facturile primite se validează cu toleranță, nu se
  recalculează. Aceeași taxă stă pe linia documentului și pe postare: 3 × 0,01
  la 21 % ⇒ 0,01 pe o singură linie, peste tot (109). Toleranța de pilot e `0,01 × liniile cu TVA` (constantă în
  adaptor) și REFUZĂ o taxă culeasă cu abatere mai mare (o factură a scenei
  P1 cu abatere 0,10 e refuzată, unde motorul vechi operează) — devine rând
  de politică la TR-D7. (N-D8, 090j, N-r4)

### motor.md — Cubul persistat (paragrafele RDC și RLF; ieșit: „pasul 4”, „rămâne TR-D9b”, „rămâne T-r2/TR-D9b”)

RDC folosește `DeclarantReturClient`, activat numai pe profilul privat
(T-D8, pasul 4). Linia fără lot inversează venitul și TVA-ul, deschizând
partida proprie negativă; linia cu lot readuce cantitatea și valoarea
pe lotul original, fără fapt fiscal. NTC poate compensa partida returului
cu cea a facturii; nominalizarea directă pe factura originală rămâne TR-D9b.
Catalogul `docs/nucleu/scenarii/RDC.md` acoperă și stocul returnat deja
consumat, anularea/reoperarea și corecția mixtă peste luna închisă.

RLF folosește `DeclarantReturFurnizor`, tot numai pe privat (T-D6/T-D8).
Pe lotul original scade cantitatea și valoarea notei fiscale, fără
preluarea soldului valoric la golire. Partida proprie401 are rest pozitiv;
compensarea cu factura folosește NTC. Catalogul `scenarii/RLF.md` probează
și reziduul−0,01 pe lot gol, reintrarea prin RDC și descărcarea ulterioară
la soldul cubului. Eliminarea reziduului prin reevaluare rămâne T-r2/TR-D9b.

(paragraful NTC/ITV; ieșit: „pasul 3”)

NTC și ITV folosesc același `DeclarantNotaContabila` (T-D3, pasul 3).

### motor.md — Entitățile și forma lor fizică (ieșit: data și „se sprijineau pe registre”)

`Module/Cub/` ține cele două entități, POCO EF în afara `EntitateConta`:
cubul e append-only, deci fără `OptimisticLockField` și fără filtru global de
interogare. Proprietățile sunt `virtual` și colecția e
`ObservableCollection`, cât timp hosturile folosesc proxy-uri de change
tracking. Niciuna nu intră în metadata clientului. (S-D1) În XAF apar numai
ca evidență read-only (lista `Postare` de sub „Registre”). Pe orice ușă
securizată gardianul le refuză crearea, modificarea și ștergerea, inclusiv
administratorului: le scrie numai motorul, pe ușa de sistem. Refuzurile de
ștergere care se sprijineau pe registre citesc cubul: lotul propriu al unei
linii șterse rămâne cât e unitatea unei postări, tipul de TVA purtat de o
postare nu se șterge, fișa care e unitatea unei postări nu se șterge.
(D9-D9, D9-A6; 2026-10-06)

### motor.md — Tipul care nu declară (ieșit: „nu mai poartă”)

Orice tip operabil declară. Tipul fără declarant (BPR) sau fără politica de
profil pe care declarantul lui o cere (`IDeclarant.PoliticaCeruta`: regulile
de contare pentru DSC, RDC și RLF, politica de TVA pentru DVI, politica
închiderii de TVA pentru ITV — absente pe bugetar) e refuzat la dry-run și la
operare cu `TIP_FARA_DECLARATIE`, înaintea validării frunzei, cu sau fără
număr cules. Tipul de document nu mai poartă un comutator de postare în cub:
orice tip operabil postează, prin declarant. (D9-D5, D9-D6)

### motor.md — Felul mixt: `Transfer` pe linia care nu schimbă contul (T-D2, felia 32)

- **BTR** (`DeclarantNotaTransfer`): lotul își schimbă gestiunea și contul lui
  rămâne — pe modelul
  de azi BTR produce NUMAI `Transfer` (T-D2.1: pe Flax 85.027 linii, toate cu
  același lot pe ambele capete, zero rânduri contabile). Contul e al lotului
  (`TipMaterial.ContImplicitId`), fără regulă de contare; refuzuri:
  `GESTIUNI_IDENTICE`, `CONT_STOC_LIPSA`, plus cele de gestiune, lot și
  cantitate. Valoarea = `Evaluare.Iesire` pe raportul curent al lotului în
  gestiunea predatoare, în secvența liniilor.
- **Diferența declarată T-D2.2**: motorul vechi scrie `round(cantitate ×
  Lot.PretUnitar)` cu prețul înghețat la nașterea lotului; cubul evaluează pe
  raportul curent (090 (j), N-r3). Pe Flax: 535 din 45.552 BTR (1,17 %), 560
  linii din 85.027, toate ieșiri PARȚIALE (cele 70.841 de goliri sunt egale),
  Σ semnată +0,92 lei și Σ absolută 28,60 lei pe 121.094.304,67 lei mutați;
  553 de linii sunt reziduul propriei goliri a motorului vechi mutat pe
  destinație, 7 sunt loturi intrate la altă valoare decât prețul lor (extrem
  `BTR-9039`: preț 0 contra 11 lei/buc). E repartizare între gestiuni, nu
  conservare: literele (a)–(g) ale reconcilierii n-o văd; apare doar la citirile
  pe cub per gestiune × lot (TR-D8). Nu se normalizează.
- Ramura `Operare` a formei mixte e probată prin proprietăți în nucleu; pe
  scenă o probează ASM (pasul 5).

### motor.md — Laturile documentului ca structură (T-D13, felia 32, pasul 2b)

- **Un singur loc de verificare**, `Laturi.Verifica`, pe ambele uși:
  `Contractare` o cheamă ÎNAINTEA declarantului (refuzul laturii e singurul
  refuz al declarației), iar `Document.ValideazaOperare` o cheamă pe faptele
  citite din bază (`Fapte.Laturile`), cu aceeași linie de mesaj
  (`Contractare.Mesaj`: `COD: text`). Codurile sunt `PREDATOR_NEPOTRIVIT` /
  `PRIMITOR_NEPOTRIVIT`; declaranții și clasele nu mai au verificări proprii
  de latură (`is Gestiune` tăiat din cele 19 clase); `CONT_PROPRIU_LIPSA` și
  `LATURA_CONT_PROPRIU_NEPOTRIVITA` au dispărut (de neatins sub contract).
- **Contractele declarate** (predator → primitor): NIR, FCT, RDC
  `Extern → Gestiune`; FCL, RLF `Gestiune/Intern → Extern` (FCL emitentul e
  `Intern`, RLF `Gestiune`); BTR, ASM `Gestiune → Gestiune`; BCS `Gestiune →
  Intern + LocConsum`; LDI `Gestiune → Intern + Comisie`; DSC `Gestiune →
  Extern | Intern`; NTC, ITV `Intern | Propriu` pe ambele; DEC, DVI
  `Extern → Intern`; PLT `Propriu → Extern | Propriu`; INC `Extern | Propriu
  → Propriu`; PIF, CAS, AMO, BPR `Intern → Intern` (AMO cere în plus
  identitatea, în clasa ei). Față de validările vechi, granularitatea e a
  părții: pe laturile fără stoc `UnitateInterna` și `Gestiune` sunt
  interschimbabile (DVI, PIF, CAS, DEC), `Partener` și `Angajat` la fel pe
  cele externe (FCT, NIR, FCL, DEC), iar `Angajat` nu mai e admis pe laturile
  interne (BCS/LDI cu calitate, NTC) — pe Flax nicio latură nu poartă
  `Angajat`.

### motor.md — Scrierea serială per bază (X-D6, 2026-10-03) (heading-ul vechi)

### Scrierea serială per bază (X-D6, 2026-10-03)

### motor.md — Gardurile declaranților, ca dată sau ca regulă (ieșit: „nu mai declară”, comparația cu motorul vechi)

- Tipul de document nu mai declară latura contului propriu: declarantul de
  trezorerie o alege structural, piciorul propriu e cel al cărui cont vine de
  la repartitorul intern, iar la virament capătul de tranzit (581) rămâne fără
  repartitor, deci soldul lui se închide pe o singură cheie.
  (S-D7, D9-A10, D9-D6; C111 S7)
- Linia fără regulă de contare e refuzată (`REGULA_CONTARE_LIPSA`) acolo unde
  motorul vechi o sare tăcut: valoarea ei ar dispărea din contare.

### motor.md — Ce rămâne al feliilor următoare (ultimul paragraf)

Citirile — sold, proiecții, fișe, SAF-T, D394, D406 — sunt pe cub. Notele pe
conturi de stoc fără lot (TR-r2) rămân delimitate prin contractele tipurilor.
Deschiderea generică scrie cubul, iar loturile ei se consumă ca oricare altele.

### motor.md — Locurile regulilor în cod (eticheta veche a linkului)

- [Oracolul pilotului și gate-ul de reconciliere al cubului](../../nou/tools/ModelCheck/Nucleu/)

### dezvoltare.md — Model și persistență (ieșit: „pasul4”, trimiterea la dovezile rulărilor)

Lanțul are migrația de bază `20261010151835_InitialCreate`
(C111-D7; 102e, D9-D6) și `20261012114235_OriginiPartide` (TR-D9b pasul4),
care adaugă proiecția provenienței fără backfill. Migrația de bază este
Dovezile rulărilor și inventarul bazelor sunt în review și în
`docs/decizii/istoric-plan-de-lucru.md`.

(serverele Postgres; ieșit: latențele, măsurătorile din 2026-10-09 și 2026-10-11, datele)

Rulările grele stau pe instanța nativă fiindcă un dus-întors din gazdă spre
container trece prin proxy-ul Docker Desktop (0,27–0,5 ms față de 0,06–0,2
ms), iar ele fac sute de mii de dus-întorsuri. Măsurat la 2026-10-09, câte o
rulare, același binar: importul pe ianuarie 21:00 pe container și 11:29 pe
nativ; ModelCheck integral 133 s bugetar / 231 s privat pe container și
100 s / 180 s pe nativ, cu aceleași verificări. Pe mașina de dezvoltare
instanța nativă e serviciul Windows `PostgreSQL`, cu pornire automată
(2026-10-09). Ordonarea textelor e cea a Windows (`en-US`), nu glibc
(`en_US.utf8`) ca în container; integrala a dat același rezultat pe amândouă.
Citirile mari merg invers: pe baza de volum, balanța, stocul și partidele cu
rest au aceleași planuri și aceleași rânduri atinse ca în container și durează
de 1,4–2,1 ori mai mult pe nativ (2026-10-11,
`docs/nucleu/c111-pas6-scriere-si-volum.md`). O cifră a scării de volum se
compară numai cu una de pe aceeași instanță.

(gazda 127.0.0.1; ieșit: timpii observați și data)

Șabloanele de conexiune ale uneltelor numesc gazda `127.0.0.1`, nu
`localhost`. După o trezire din hibernare, Npgsql 10 pe `Host=localhost` a
rămas în negocierea GSS până la expirarea conexiunii (15 s), deși serverul
răspundea, iar psql se conecta; cu `127.0.0.1` sau cu
`GSS Encryption Mode=Disable` conexiunea se deschidea în 50 ms. ModelCheck
bugetar raportează atunci „Baza nu există încă — doar validare de model” și
iese cu cod 2, deși baza există. Șirurile hosturilor (`appsettings*.json`,
șirul de design din `BackOfficeDbContext`) numesc aceeași gazdă, pe 5444.
(2026-10-09)

(bazele de dezvoltare și probele HTTP; ieșit: recrearea din 2026-09-25, rulările măsurate)

Recrearea din 2026-09-25 (C102) a șters toate bazele Atlas.Conta de pe
5444 (clonele de import, review, perf `Nucleu.Fizica.x1/x10`, CodexBCS,
ClaudeRev) și a refăcut cele trei de mai sus. Pe baza Privat din seed,
`partide-cub.py` își creează singur fixture-ul. (89g, 102b)
Comenzile de document au proba HTTP proprie, cu fixture creat și șters de ea:
`nou/tools/ProbeHttp/comenzi-coaja.py` (404/403/422 pe cele cinci comenzi,
ușa altui tip, fără scriere la refuz). (104b)
Matricea generală `refuzuri.ps1` rulează pe baza Privat din seed cu fixture
propriu pe prima lună deschisă a lanțului: un furnizor NeinregistratRo, o FCT
operată cu plata conex operată și împerecherea lor, un ITV draft pe aceeași
lună și un angajat, desfăcute în ordine inversă în `finally`. Rămân auditul
și cele două `RefuzSeed` ale rândurilor de politică de probă (104i, chei fixe:
a doua rulare nu adaugă nimic). Plafonul de 500 al candidaților DVI e probat
în ModelCheck, nu pe HTTP. Măsurat 2026-09-27 pe `c104-straturi`: 294/294
PASS de două ori consecutiv, baza identică înainte și după. (104-r5)
La 2026-10-11, cu rândurile fișei fără terț și ale sintezei ei: 327/327 de
două ori. Matricea cere bază nouă: pe una pe care a rulat `explicatii.py`
pică pe starea lăsată de acesta (documente din 2012, politici de probă), nu
pe produs. (C111 pasul 7)
Matricea conține și ușa explicației (X-D4): tranzacția facturii fixture-ului
se află din jurnal, `Admin`, `Cititor` și `Configurator` primesc 200, iar
`User` 404, la fel ca pe un id inexistent. 403
`EXPLICATIE_ACCES_INCOMPLET` cere un rol cu citire restricționată și rămâne
al probei `explicatii.py`. Măsurat 2026-10-04 pe `tr-d8-transversal`, pe o
bază privată recreată din seed: 300/300 PASS de două ori consecutiv. (X-D8)
Blocajul scrierii are proba HTTP proprie,
`nou/tools/ProbeHttp/scriere-ocupata.py`: ține
`pg_advisory_xact_lock(97000)` pe o a doua conexiune și cere, sub blocaj,
200 imediat pe citire, pe dry-run și pe salvarea fără detalii noi, 422
`SCRIERE_OCUPATA` fără nimic scris pe operare și pe salvarea cu o linie
nouă, apoi 200 pe aceeași operare după eliberare. Fixture-ul e al probei și
se desface în `finally`. Durează cât două timpuri de comandă (un minut).
Rețeta care recreează baza din seed, pornește hostul pe 5089 și rulează
matricea de două ori, `scriere-ocupata.py`, `comenzi-coaja.py` și
`explicatii.py` (ultima, fiindcă lasă un bon stornat):
`run-verificari/x6-http.ps1`. (X-D6, X-D8)
Desfacerea facturii se înscrie imediat după crearea ei și redescoperă din
ID-ul FCT plata conex și împerecherile; după `finally`, matricea verifică pe
API absența identităților fixture-ului (cod 3 la rezidu). `-CadeDupa <punct>`
injectează o cădere după o mutație a fixture-ului, iar
`refuzuri-caderi.ps1` le parcurge pe toate și cere cod 2 și zero rezidu la
fiecare. (104-r5, review C104 R2)

### dezvoltare.md — Contracte generate și build (ieșit: dovada pe `InitialCreate`, data observației)

Schema bazei nu contează (a trecut pe o bază rămasă la `InitialCreate`).
Fără serverul hosturilor, `ConnectionStrings__ConnectionString` din mediu
îndreaptă procesul spre altă bază (observat 2026-10-13). Metadata este derivată din model

(`--scenarii`; ieșit: fraza despre tipurile rulate selectiv și durată, „pasul 6c, 2026-10-07”)

probele de model și fără celelalte scene: toate tipurile de pe cub în
rulate selectiv pe privat; durata depinde de grupul ales. Codurile sunt ale catalogului (`docs/nucleu/scenarii/`,
citite din bază le ordonează pe o cheie a scenei, nu pe ordinea bazei; purja
de scenă prinde codurile pe prefixul marcajului. (091 (e), 091-r1; TR-D9a
pasul 6c, 2026-10-07)

### import.md — Import operațional 1C (nominalizarea sursei; ieșit: cifrele pe ianuarie 2025 și „nu mai mută”)

Conectorul păstrează nominalizarea sursei. La loturi, linia poartă lotul
numit de 1C. La partide, rândul de compensare sau de operație care are un
document în subconto-ul „Documente” numește pe linia de notă partida acelui
document, fie ea deschisă de un document importat sau inițială; trecerea 2 nu
mai mută o partidă inițială pe care stingătorul a numit-o la operare. Unde
sursa nu numește, conectorul dă cheia de ordonare: partida inițială primește
data documentului 1C ca dată de naștere. Pe ianuarie 2025: 546 de laturi cu
partida numită, niciuna absentă la data notei; 677 de date distincte pe cele
3.967 de partide inițiale. (115)

(ordinea partidelor inițiale; ieșit: cifrele celor două importuri și data)

Partidele inițiale intră în deschidere ordonate pe cheia sursei (simbol,
partener, document 1C), iar la aceeași dată de naștere FIFO le consumă în
această ordine. Două importuri ale aceluiași binar pe ianuarie 2025 dau
același raport de reconciliere, aceleași legături și aceleași partide pe chei
naturale (18.770 de tranzacții, 81.774 de postări, 10 verificări picate în
ambele). (116; 2026-10-09)

(pregătirea automată; ieșit: timpii pe ianuarie)

Fără connection string dat, Import1C scrie în `Atlas.Conta.Import1C.Flax`
pe instanța nativă (5446). Pe orice conexiune care nu spune altfel pornește
pregătirea automată a comenzilor (`Max Auto Prepare=300`,
`Auto Prepare Min Usages=2`): pe ianuarie, 11:29 → 9:55 pe același binar.

(`--pana-la-ziua`; ieșit: cifrele rulării scurte)

se combină cu `--inchide-lunile`. Ianuarie până la ziua 5: 822 de documente,
72 s cu tot cu deschidere.

(`--inchide-lunile`; ieșit: verificarea din 2026-09-17)

ocolire). E proba supremă a soldurilor materializate: raportul de reconciliere
trebuie să rămână IDENTIC cu baseline-ul rulării fără închideri, adică importul
citește peste snapshot-uri, nu peste registrul integral, și scrie aceleași
cifre. Verificat 2026-09-17 pe anul 2025: 12/12 luni închise, 0 constatări per
lună, raport identic cu `reconciliere-20260914-164035.txt`, `Reconstruieste`
0 diferențe pe contabil, stoc și partide. (F27-D1, F27-D3)

(verificarea istorică a trecerii pe TPH, întreagă)

Verificarea istorică a trecerii pe TPH a cerut ca importul integral cu
`--recreeaza --cititori --inchide-lunile` să dea un raport IDENTIC pe
conținut sortat cu baseline-ul curent
(`nou/tools/Import1C/reconciliere-20260917-121343.txt`), iar
`Reconstruieste` 0 diferențe. După ea se rulează pe baza de import SQL-ul
din `--dump-integritate-tph`: zero rânduri pe fiecare interogare. Verificat
pe TPH la 2026-09-18: raportul `reconciliere-20260918-154628.txt` e identic pe
conținut sortat cu baseline-ul; 12/12 luni închise fără constatări;
`Reconstruieste` a dat 0 diferențe; integritatea TPH a dat 0 încălcări în 103
interogări. 15 dintre ele sunt vacue pe import (tipuri și legături pe care
importul nu le produce), iar pe acelea le acoperă ModelCheck. (89g, 89h)

(proba supremă după 091, întreagă, și istoricul probei pe cub)

**Din 2026-09-22 (091) proba supremă nu mai e importul, ci catalogul de
scenarii** (`docs/nucleu/scenarii/README.md`): așteptări scrise de mână,
ciclul complet per tip, lanțurile transversale, pe ambele profiluri; clona
Flax rămâne sursă de întrebări (recensământ, 091-r2), Import1C e felia de
migrare (091-r4), după „rotund" (091 (g)). Paragraful de mai jos e
istoricul probei până la felia 32, pasul 2b.

Pe cubul persistat proba supremă avea aceeași formă: importul integral cu
tipurile migrate marcate `PosteazaInCub` trebuie să dea exit 0, ZERO refuzuri
ale declarației, raport identic pe conținut sortat cu baseline-ul, 12/12 luni
închise, `Reconstruieste` 0 diferențe, `--dump-integritate-tph` 0 încălcări și
`--reconciliere-cub` 0 rânduri Δ pe toate literele; `refuzuri.ps1` se reface pe
clona privată a noului import. Măsurat la 2026-09-21: Import1C integral pe Flax (`--recreeaza --cititori --inchide-lunile`, 2026-09-21): exit 0, 1 h 57 min (3 h 21 min la felia 28), raportul `nou/tools/Import1C/reconciliere-20260921-035646.txt` IDENTIC pe conținut sortat cu baseline-ul feliei 28, ZERO refuzuri ale declarației, 12/12 luni închise cu 0 constatări, `--reconciliere-cub` 0 rânduri Δ pe (a)–(g) — (f) vacuă: cele 9 conturi cu rol de terț sunt atinse și de tipuri nemigrate —, integritatea TPH 0 încălcări în 107 interogări, cubul cu 70.373 tranzacții / 252.092 postări / 16.924 transferuri (PLT → FCT; INC → FCL fără transfer, FCL fiind nemigrat), `refuzuri.ps1` 294/294 PASS pe `Atlas.Conta.BackOffice.Privat` refăcută din import cu perioadele redeschise. (S-D10)

### import.md — Reconciliere și migrare legacy (contractele de citire; ieșit: „nu mai referă”, „pasul 5, 2026-10-06”, uneltele ieșite)

Import1C citește cubul prin `Cub/Citiri` și nu mai referă registrele
(TR-D9a pasul 5, 2026-10-06). Contractul 1 citește `Contabil.Postari`, intrarea
balanței. Contractul 2 reface perechea debit / credit a unei linii ITV din
cele două postări ale ei; o linie de altă formă e eșec, nu se sare. Contractul
3 citește `Loturi`; la deschidere, celulele sursei cu valoare fără cantitate,
care nu intră în cub, sunt o categorie de justificare numită, pe exact
cheile măsurate. Predicția de valoare a alocării cheamă `Nucleu.Evaluare.Iesire`
pe soldul din cub, pe orice ieșire evaluată din sold. `--sabotaj`, oracolul
golirii din raport și scrierea deschiderii în registre au ieșit. `Migrare` și
`BackfillTva` sunt șterse. (D9-D11; `docs/import/faza-1c-design.md` §16)

### verificare.md — Verificări proporționale cu modificarea (corectură pe cod)

```text
| Declarant, operand, `Fapte.Operand`, oracolul pilotului | Scenariile independente ale tipului, apoi ModelCheck pe AMBELE profiluri; `NUC-*` păstrează comparația normalizată ca regresie, conservarea, determinismul și `≤ 16` interogări per operand.
```

### verificare.md — Verificări proporționale cu modificarea (corectură pe cod)

```text
| Materializare, declarant al unui tip migrat, împerecherea ca `Transfer` | ModelCheck pe ambele profiluri: probele `STR-*` pe scenele BCS, Trezorerie și FCT — operare, roundtrip, storno, anulare, refuz, configurație, poziție, transfer, latură, corecție, reconciliere — cu comutarea locală a regimului (`ProbeCub.Migrat`/`Nemigrat`/`CuToleranta`, cu restaurare) și purja rândurilor de cub ale documentelor scenei (S-D8) |
```

### verificare.md — Capcanele rulării (corectură pe cod)

- O clonă a bazei de import poartă valoarea DEFAULT a unei coloane noi, nu
  valoarea de seed (`TolerantaTaxa` 0 contra `null`): se aliniază înainte de
  gate, altfel refuzurile sunt ale bazei, nu ale codului.
- Gate-ul re-contractează documente pe o bază cu perioadele deja închise;
  citirea fără documentul curent ia referința strict înaintea datei, deci un
  document datat exact la sfârșitul perioadei de referință nu e refuzat
  `STOC_INSUFICIENT`. (T-D4.3)
- Oracolul citește registrele ORDONAT pe (document, poziția liniei, linie,
  id), altfel ordinea heap-ului schimbă nominalizarea între rulări și aceeași
  probă alternează OK/FAIL.

### verificare.md — Catalogul de scenarii (corectură pe cod)

Refuzurile se probează pe două uși: codul stabil (`CoduriRefuz`, linia
`COD: mesaj`) pe ușa declarației (`Materializare.Refuzuri`); pe ușa entității
(`ComenziDocument.Valideaza` / `Opereaza`) validarea veche a clasei și gardienii
registrelor refuză cu text ÎNAINTEA declarantului, deci acolo se asertează
familia mesajului și absența efectelor, iar un refuz al registrului se
marchează ca atare în fișierul tipului (până la TR-D8).

Cataloagele [BCS, FCT, PLT/INC, BTR și FCL/DSC](../nucleu/scenarii/README.md)
au loturi independente, cu fixture prin documentele reale și comenzile
`ComenziDocument`; probele `NUC-*` rămân regresie. Sumele directe ale
scenariului peste cub nu înlocuiesc verificarea cititorilor comuni și a
`Sold` la TR-D8.

### verificare.md — Catalogul de scenarii (corectură pe cod)

separarea, SC-NTC-20 compatibilitatea cu identitatea istorică. SC-NTC-22

### verificare.md — Invarianții cubului (`INV-CUB`) (corectură pe cod)

rulat o dată per profil pe prima scenă cu faptele potrivite:
`DESCHIDERE-EGALA` (registrul istoric egal cu deschiderea cubului trece),
`DESCHIDERE`, `PARTIDE` (partidă fără repartitor), `POLITICA` (totalul de
decontare), `IMO-FISA`, `IMO-CAUZA`, `IMO-ORIGINE` (inversa unei fișe fără
`InversaDin`), `IMO-SUPORT` (transfer valoric pe fișă fără suport),
`IMO-REGISTRU` și cei șapte ai explicației (X-D4): `EXPLICATIE-LIPSA`

### verificare.md — Invarianții cubului (`INV-CUB`) (corectură pe cod)

(soldul citit +1.000), `-STINGERE` (`AlocareFifo` scoase). Mutantul
`EXPLICATIE-STORNO` aplică aceeași abatere și rândului de registru, ca să
ajungă la ramura explicației.

### verificare.md — Invarianții cubului (`INV-CUB`) (corectură pe cod)

Ramurile acoperirii registru ↔ cub, echilibrului și provenienței au probele
SC-CIT-23, SC-CIT-34 și SC-CIT-10. Acoperirea

### verificare.md — Declaranții pilotului (corectură pe cod)

Declaranții pilotului (BCS, PLT/INC, FCT) se probează în ModelCheck, pe
ambele profiluri, prin `ProbeNucleu.Proba` (`nou/tools/ModelCheck/Nucleu/`):
după operarea prin motorul vechi, documentul se contractează prin
`Contractare.Contracteaza` și postările lui se compară EXACT (multiset) cu
registrele scrise de motorul vechi, transformate în cub prin `CubDinRegistre`
(portul mapării fizicii) și normalizate DOAR prin funcțiile numite ale
diferențelor declarate (`Normalizari`, contractul TR-D6b B-D8); orice reziduu
al unei normalizări e avertisment tipărit și pică proba. Scenele proprii
(`VerificaNucleuBcs`, `VerificaNucleuTrezorerie`) își purjează documentele
(altfel `PAR-V*` văd partide în plus). Cifrele consemnate, nu normalizate:
`NUC-BCS-N-R3-*` (Δ = +25 pe lotul corectat) și `NUC-FCT-N-R4-*` (Δ = 0,01
pe taxa per document). (TR-D6b)

### motor.md — Nucleul pur (corectură pe cod)

  latură, tranzacție distinctă; contrapartida unei postări atribuite
  inversate rămâne nedefinită în cod până la TR-D9b (N-r5). Regula e 119,
  neimplementată: stornoul beneficiarului conține numai inversa lui, iar
  corecția își scrie compensarea într-o `Operare` proprie. (N-D9, N-D10)

### motor.md — Ordinea liniilor (corectură pe cod)

documentului plus unu. Ambele motoare citesc liniile `OrderBy(Pozitie)`, apoi
`ThenBy(ID)`: cele cinci enumerări ale motorului vechi (TVA culeasă, notele,
TVA-ul, loturile născute, potrivirea regulilor de stoc) trec printr-un singur
helper, iar conexul clonat primește liniile sursei în aceeași ordine. (S-D6)

### motor.md — Scrierea serială per bază (corectură pe cod)

  lotului, restul partidei, starea perioadei și planul registrelor se citesc
  după el,

### motor.md — Gardurile declaranților, ca dată sau ca regulă (corectură pe cod)

- Linia fără regulă de contare e refuzată (`REGULA_CONTARE_LIPSA`), fiindcă
  valoarea ei ar dispărea din contare. (112)

### perioade.md — Perioada fiscală ca lanț (corectură pe cod)

  asupra ei, ca la registre. (F27-D1)

### perioade.md — Soldurile materializate la închidere (corectură pe cod)

  se refuză, ca la registre. Ștergerea lor

### dezvoltare.md — Migrațiile (corectură pe cod)

Lanțul pornește din migrația de bază `20261010151835_InitialCreate`
(C111-D7; 102e, D9-D6); `20261012114235_OriginiPartide` adaugă proiecția
provenienței fără backfill (TR-D9b). Migrația de bază este


### documente.md — Modelul comun

Clasificarea celor 49 de clase e în fișierul deciziei.

### documente.md — Registre și ciclu de viață

Cele patru tabele de registru vechi nu mai există:
entitățile, maparea, listele XAF și cazurile gardianului au dispărut atomic
la 2026-10-07 (TR-D9a, pasul 7, D9-D6), iar numele lor sunt interzise în
sursă (D9-D7 b).

### documente.md — Tipuri de document

ITV nu este editabil prin această felie. (46b, 79c, 82a)

### api.md — Salvare și comenzi

Comenzile care scriu sunt seriale per bază (X-D6): o comandă trimisă cât
timp alta scrie așteaptă. [...]
Măsurat pe host viu, cu blocajul ținut de altă conexiune: 422 după 30 s,
timpul de comandă implicit. (108f)

dată a înregistrării anterioară datei documentului — pe toate cele
cincisprezece uși de scriere.

Schimbare de comportament: un lot consumat
integral, cu cantitate ȘI valoare zero, nu mai apare în listă; unul cu
cantitatea zero și valoare nenulă rămâne, ca reziduul valoric să se vadă.
(F27-D3)

Proiecția include acum și `ReturClient`.
(F27-D7)

Serverul nu are parametrul `gestiuneId`; locul
(`Postare.Loc`) și partenerul fiscal nu sunt filtre. Clientul nu-l mai
trimite, iar ecranele numesc axa „Repartitor”. (C111 pasul 7)

### api.md — Securitate și răspunsuri

Niciun tip de registru nu mai e cerut, deci separarea contabil / imobilizări
a dispărut.

(F22-D5, 80e, 108 d, D9-D9; 2026-10-06)

(110 (e) precizată, C111-Q22; 2026-10-11)

### api.md — Contracte generate

Din 2026-09-25, ștergerea împerecherii prin API și acțiunea XAF
„Șterge împerecherea” folosesc aceeași comandă atomică: [...]

### api.md — Diferențele NIR (098/099, 2026-09-24)

(heading-ul; conținutul a rămas în pagină sub „Recepția cu diferențe (NIR)”)

### api.md — Raportul partidelor pe cub (101, 2026-09-25)

(heading-ul; conținutul a rămas în pagină sub „Proiecțiile” → „Partidele” și „Împerecheri”)

### tva.md — TVA la operare

Pe cele șase politici de TVA din seed
latura numită e terțul documentului.

[domeniu-si-operare](../stare-curenta/documente.md), „Regula coordonatelor”.

(linkul indica `documente.md`; „Regula coordonatelor” stă în `motor.md`, linkul a fost îndreptat acolo)

### tva.md — Citirea fiscală și confirmarea depunerii (103)

(heading-ul; conținutul a rămas în pagină, comasat cu paragraful de depunere din „Perioada de declarare și corecțiile” sub „Confirmarea depunerii”, plus „Accesul la citirea fiscală” și „Exportul SAF-T (D406)”)

### partide.md — Împerechere și stingere automată

La stornarea unui document împerecheat, împerecherile din fereastra deschisă
se cer șterse ca înainte, [...]

Totalul se citește din cub: `ImperechereService.Total` și coloana „Total" din
`DocumenteCuRest` folosesc aceeași formulă. Documentul nu mai are coloană de
total stins: a dispărut la pasul 7 al TR-D9a. (F27-D7, 102, D9-D10)

### partide.md — Decontul pe cub și urmărirea partidelor (095, 096, 100)

Seed-ul activează
urmărirea pe 542 privat și 542.01.00/542.02.00 bugetar, păstrând acoperirea
comercială existentă. Din 2026-09-25 sunt urmărite și conturile bugetare
401.01.00, 404.01.00 și 411.01.01 DinSeed, fără schimbarea RolTert (100).

Migrația
inițializează noul atribut pe conturile cu rol comercial, inclusiv manuale;
postările istorice rămân neschimbate.

### partide.md — Împerecherea ulterioară operării = tranzacție `Transfer`

Din 2026-09-25, `Cub.Citiri.Partide` este intrarea comună pentru soldurile
pe unitate/cont/partener, cu transferurile și inversele lor incluse.

### partide.md — Partide: raport, snapshot și împerechere (101, 2026-09-25)

Originea se
citește din `OriginiPartide`, scrisă atomic din postările finale eligibile
de materializare (TR-D9b, pasul 4, 2026-10-12).

### ecrane.md — Formulare React

Conversia citire → scriere a fiecărei felii o poartă explicit: un
câmp lipsă de acolo s-ar fi rescris tăcut la fiecare re-salvare. Coloana din
listele de documente rămâne de adăugat. (F27-D4)

### ecrane.md — Regimul pe stare în XAF Blazor

în Blazor tooltip-ul se randează și pe acțiunea dezactivată (probat în
browser, 2026-10-04).

nomenclatorul deschis din
navigație sau din lookup are ștergerea disponibilă (probat 2026-10-04). (106k)
Controllerele de comenzi nu mai poartă `Enabled["Stare"]`;

### ecrane.md — Listele XAF Blazor

`ServerView` este opt-in per view, doar pe registre append-only citite:
azi numai lista de evidență a cubului, `PostareVizual_ListView` de sub
„Registre” (D9-A12).

`Postare` nu mai are intrare de navigare; [...] Listele
vechi de registre au dispărut odată cu entitățile lor (2026-10-07, D9-D6).
(D9-D9, D9-A12)

### stoc.md — Transformarea n→m (ASM-B2…B7, 2026-09-23)

(heading-ul; conținutul a rămas în pagină sub „Transformarea n→m”)

### stoc.md — FCL și DSC pe cub (T-D4, felia 32, pasul 2)

- **Transferul INC → FCL** e activ prin `Materializare.Imperecheaza` fără
  nicio schimbare: referința e a stingătorului (4111), plafonul e restul FCL
  pe 4111; partida 419 nu primește transfer.
- **Diferența declarată T-D4.2** (aceeași clasă ca T-D2.2, N-r3): pe Flax 842
  din 36.696 DSC (2,29 %), 919 linii din 62.063, toate ieșiri PARȚIALE (cele
  47.505 goliri sunt egale), Σ semnată +31,01 lei, |Δ| max 16,50 lei (lot cu
  preț înghețat 0), 909 din 919 linii ≤ 6 bani, pe 79.199.898,27 lei mutați.
  Nu se normalizează (T-r7).

### stoc.md — Corecția valorii lotului (TR-D9b-CV, 119)

- La stornare, saltul pe `Atribuit` nu mai urmează postările pe lot; pe
  partide rămâne cel din 090 (i). (119 c)

### stoc.md — NIR delta și citirile comune (098/099, 2026-09-24)

`PoliticaDiferenta` este configurabilă,
cu proveniență de seed; nomenclatorul privat existent furnizează conturile 32x.

Postările marcate cu contrapondere n-au lot, deci nu intră în
disponibilul real. TR-D8 rămâne în
lucru pentru fiscal/SAF-T și verificările transversale.

Seed-ul bugetar clasifică 409.01.01 fără stoc (clasa S), conform 099(d);
postările istorice nu se reclasifică prin această corecție de seed.

### imobilizari.md — Amortizarea imobilizărilor

AMO este documentul generat lunar pe unitate internă, pe tiparul ITV;
mecanica generării, eligibilitatea fișelor, formula și gardienii sunt
descrise în [imobilizari](../stare-curenta/imobilizari.md#imobilizări). Aici
stau datele care o parametrizează. (87d, 87g)

(trimiterea internă a devenit inutilă: pagina are acum secțiunile „Amortizarea lunară” și „Datele amortizării”)

### saft.md — SAF-T

Restul `SourceDocuments` și sursele contabile/stoc ale exportului complet
rămân inventariate separat în TR-D8; această felie nu certifică migrarea sau
securitatea întregului SAF-T. (103, D8-B8)

**L pe cub (TR-D8 S1 + S2).**

Reverificarea Codex din 2026-09-29 închide R1 la nivelul probei de domeniu:
unitatea definită e protejată de regula produsului de mai sus.

**Ruta pe registre este scoasă (SAF-B8, 2026-09-30).** `SaftProiectii.Saft`
și `SaftStocuri` nu mai există. Diferențele lor față de cub sunt clasificate
o singură dată, cu martor numeric, în [raportul A/B final](../nucleu/tr-d8-saft-ab.md);
probele D16/D17 rulează pe cub.

**S pe cub (TR-D8 S3).**

### raportare.md — Explicația deciziei (X-D4, 2026-10-03)

(heading-ul; conținutul a rămas în pagină sub „Explicația deciziei” și „Invarianții explicației și ai tranzacției”)

### raportare.md — Identitățile cititorilor (C111-D10)

Regula terțului comercial în SAF-T (S11) și cea a partenerului fiscal în
D394 (S13) sunt în [politici-si-fiscalitate](../stare-curenta/politici.md),
§„SAF-T” și §„D300 și D394”.

(trimiterea indica `politici.md`; secțiunile stau în `saft.md` și `tva.md`, linkurile au fost îndreptate)

### politici.md — Configurarea profilului

`Politici.TipuriConfigurabile` — lista explicită a
celor 22 de tipuri cu proveniență,

### politici.md — Proveniență și verificarea configurației

Așa e pe mapările de TVA de la
TR-D9b pasul 7, unde `Sens` a devenit `SensTva`.

Raportul de profil nu mai are
categoria „referință spre un rând șters”:

### documente.md — Data documentului și data înregistrării / Corecția unui document operat

Regulile ei sunt în
  [politici](../stare-curenta/politici.md). (F27-D4, F27-D5)

Efectul FISCAL al motivului e în `politici.md`.

(trimiterile indicau `politici.md`; perioada de declarare și efectul fiscal al corecției stau în `tva.md` § Perioada de declarare și corecțiile, linkurile au fost îndreptate)
