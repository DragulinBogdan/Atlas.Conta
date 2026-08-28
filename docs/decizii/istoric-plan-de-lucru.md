# Istoricul de execuție al planului de lucru (snapshot 2026-08-24)

> Extras verbatim din `CLAUDE.md` la 2026-08-24 (commit `f18c24c`). Starea
> curentă compactă e în `CLAUDE.md` §„Stare și roadmap"; deciziile referite
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
   tabelele de politică.
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

