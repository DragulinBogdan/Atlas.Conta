# GATE XAF — contractul feliei (decizia 44.2)

**Obiectiv:** polish pe DOUĂ ecrane — FacturaIntrare (FCT) și FacturaIesire (FCL)
— până la pragul: **„un contabil tolerant le operează zilnic"**. Explicit NU
product-grade. Gate trecut = „avem pe ce construi" (pasul 5 se planifică pe
modelul călit).

**Regula de oprire (cu dinți):** orice luptă STRUCTURALĂ cu Blazor (async,
dialoguri, refresh de UI, feedback) NU se hack-uiește — se notează în
`docs/api/lista-react.md` și moare acolo. Cosmetica dincolo de prag nu intră.
Polish-ul e refolosibil de ecranele back-office ale comenzilor
(`docs/architecture-notes-2026-07-28.md` §Secvențiere) — investiția nu e
consumabilă.

## Starea de la care pornim (explorare 29.07.2026, două rapoarte)

Trei goluri de flux + defecte reale găsite pe drum:

1. **GOL 1 (blocant absolut):** linia de stoc FCT nu-și poate crea Lotul prin
   UI — `FacturaIntrareDetaliu` nu are `ProdusId`, `CreeazaLot` are zero
   apelanți UI. Validarea „alegeți produsul" (`FacturaIntrare.cs:97-98`) e
   neîndeplinibilă; ocolirea manuală (Lot din Nomenclatoare) produce divergență
   tăcută TVA↔cost (două prețuri independente; `DocumenteGestiune.cs:15-19`
   ia prețul TASTAT pe lotul străin).
2. **GOL 2:** `Lot` e singurul tip țintă de lookup fără `[XafDefaultProperty]`
   → „Castle.Proxies.LotProxy" pe FCT/FCL/DSC/LDI/ASM (restanța 40d).
3. **GOL 3:** operatorul culege financiar orb — `Valoare`/`ValoareTva`/`Total`
   rămân 0 până la operare (`TvaService.CalculeazaValori` apelat doar din
   `PregatesteOperare`); nu poate confrunta totalul cu hârtia înainte de a
   scrie registrele. `ViewSummaryController` (Atlas.DXF) nefolosit.
4. Defecte reale: `Stare`/`DataOperare`/`Autogenerat`/`DocumentSursa`
   EDITABILE pe Draft (operator poate seta Operat fără registre!); `Numar`
   editabil pe FCL ocolește seria fiscală; numărul + incrementul politicii se
   consumă ÎNAINTEA gardienilor (`MotorOperare.cs:45` vs `:70`/`:217` — refuz
   ⇒ gol în serie comis de un Save ulterior); `Data` pornește pe 0001-01-01
   (zero hooks de inițializare în model); câmpuri moarte
   `GenereazaChitanta`/`ChitantaNumar`/`ChitantaData`; storno hardcodat azi;
   zero feedback la succes; erori multiple = paragraf continuu (`\n` nerandat).
5. Header-ele FCT/FCL 100% auto-generate (~20 câmpuri plate); NIR (conexul!)
   fără baseline de coloane; captions fără diacritice, shell EN.
6. Starea reală vs CLAUDE.md: SmartLookup REVERTAT la lookup standard (commit
   `98ce1d0`; decizia 40d descrie starea veche; comentariile din cod stale);
   Atlas.DXF pinnat `26.1.3.7` (nu flotant, contra 41e); FK-urile brute pe
   NIR/BTR/BCS/PLT/INC sunt de fapt REZOLVATE de
   `ForHierarchy<Document>().HideForeignKeys()` (nota 40e inexactă).
7. Baza de dev UI e profil Bugetar; fluxul FCL→DSC (privat) nu e exersabil pe
   ea. Bazele private există doar în unelte (ModelCheck.Privat, Import1C.Flax).

## Decizii pin-uite (înainte de delegare)

- **D1. `ProdusId` pe `FacturaIntrareDetaliu` (derivată), NU pe bază** —
  consecvent cu testul bazei (22/25c: baza nu are ProdusId) și cu precedentul
  FCL (37d). NIR manual / LDI+ / ASM rămân în afara feliei (backlog explicit).
- **D2. Nașterea lotului la culegere = controller pe `Committing`** al
  ObjectSpace-ului view-ului FCT: pentru liniile cu `Natura == Stoc` și
  `Produs` ales — `CreeazaLot(produs, Primitor-ca-Gestiune)`; sincronizează
  (Produs/gestiune schimbate → update pe lotul propriu), curăță (linie/document
  Draft șters → lotul cu `LinieIntrareId == linia.ID` se șterge; FK-ul e fără
  constrângere — 26e, deci curățenia e a noastră). Primitor lipsă/ne-Gestiune
  la commit → skip grațios (se naște la commit-ul următor; operarea comite
  întâi culegerea — seam-ul acoperă și butonul direct). Lotul pe linia FCT
  devine READ-ONLY în UI (alegerea unui lot străin = exact bypass-ul divergent
  — se închide). Coerența Tip-linie ↔ Tip-produs se validează și pe FCT
  (oglinda 38c de pe FCL).
- **D3. `TipMaterial` se precompletează din `Produs.TipMaterial`** la alegerea
  produsului (FCT + FCL), doar dacă e gol — friction-ul principal al culegerii.
- **D4. Identitatea `Lot`: `[XafDefaultProperty]` pe o proprietate calculată
  `Eticheta`** (`Produs.Denumire × Data × PretUnitar`, `[NotMapped]`); dacă
  server-mode-ul lookup-ului refuză membrul nemapat → fallback `ToString()`
  override + coloane explicite, iar constatarea merge pe lista React.
  Opțional (attempt, nu promisiune): lookup-ul pinului FCL filtrat pe produsul
  liniei (`DataSourceCriteria`); dacă se bate cu nested/EF → doar lista React.
- **D5. Recalcul la culegere prin ACELAȘI helper `TvaService`** (o singură
  sursă a formulei): controller pe `ObjectChanged` (FCT+FCL linii) la
  schimbarea `Cantitate`/`PretUnitar`/`TipTva` → `Valoare`+`ValoareTva`;
  editarea manuală a `ValoareTva` NU se suprascrie decât dacă se schimbă baza
  (comportament documentat; motorul păstrează 36a neatins). `Total` vizibil pe
  DetailView înainte de operare; dacă refresh-ul live al header-ului se bate
  structural cu Blazor → sumar de grid (footer) + lista React. `Total` NU se
  adaugă în ListView-urile root (NotMapped ⇒ N+1 pe Detalii — disciplina de
  hot-path, 35d).
- **D6. `AsignaNumar` se mută în faza de MATERIALIZARE** (după TOȚI gardienii)
  — alinierea cu propriul principiu 33d; un refuz nu mai lasă număr consumat /
  `UrmatorulNumar` incrementat în OS-ul viu. Verificare ModelCheck: refuz ⇒
  numărul politicii neatins.
- **D7. `Numar` read-only DATA-DRIVEN**: tipul are rând `PoliticaNumerotare`
  ⇒ câmpul nu se culege (seria fiscală neocolibilă); FCT (fără politică) îl
  culege obligatoriu, ca azi.
- **D8. `Stare`, `DataOperare`, `Autogenerat`, `DocumentSursa` = read-only în
  UI întotdeauna** (sunt ale motorului).
- **D9. `Data` = azi la creare** (generic, toate documentele; primul hook de
  inițializare din model — controller pe `ObjectCreated`, precedentul
  `DefaultTipTvaController`).
- **D10. Stornează devine `ParametrizedAction(DateTime)`** cu default azi
  (precedentul „Generează descărcarea"); motorul primea deja data.
- **D11. Feedback la succes**: toast la operare („Operat. Nr. …" / + mesajul
  de backorder existent pe FCL). Erorile multi-linie: încercare minimă de
  randare lizibilă; dacă e luptă structurală → separator „ • " + lista React.
- **D12. Layout DetailView FCT/FCL prin EntityFluent** (39c), cu grupuri
  logice și captions RO cu diacritice pe cele două ecrane (+ liniile lor).
  Câmpurile moarte `GenereazaChitanta`/`ChitantaNumar`/`ChitantaData` și
  `TethysId` se ASCUND (rămân în schemă; reactivarea la fluxul BF e aditivă);
  `Valuta`/`Curs` în grup secundar. NIR + ListView-ul generic de detaliu
  primesc ordine de coloane (beneficiază BTR/BCS/PLT/INC gratuit). Appearance
  pe `Stare` (Draft/Operat/Stornat distinse vizual). Localizarea COMPLETĂ
  (shell, dialoguri framework) = în afara feliei → lista React.
- **D13. Smoke pe profil PRIVAT, pe CLONA bazei de import** (cerută de owner):
  `createdb -T "Atlas.Conta.Import1C.Flax" "Atlas.Conta.BackOffice.Privat"` —
  date reale la volum real (187k documente, nomenclatoarele importate, plan
  complet). ORIGINALUL nu se atinge (e harness-ul de reconciliere 1C și
  baseline-ul conectorului). Profil de lansare separat: `appsettings.Privat.json`
  (env `Privat`): `ProfilContabil=Privat` + connection string pe clonă;
  default-ul (Bugetar) neatins. Precondiții pe clonă la pasul 4: migrația
  nouă aplicată manual (aplicația nu face schema update — 23a), userii XAF îi
  creează updater-ul aplicației la prima pornire (Flax e seed-uit prin
  ContaSeeder, fără useri), perioadele 2026 se adaugă din UI (seed-ul le sare
  la count>0). `VerificaProfil`/`SetareProfil` se potrivesc natural
  (Privat + AwayFromZero, `ConventieRotunjire` absent = convenția bazei).
- **D14. SmartLookup rămâne revertat** (memoria: repararea se face în
  Atlas.DXF, separat). Lookup standard = acceptabil pentru „tolerant";
  comentariile stale din cod se corectează. CLAUDE.md se corectează la
  închidere (40d/40e/41e — starea reală).
- **D15. `lista-react.md` se DESCHIDE la felia asta** (`docs/api/`), seed-uită
  cu itemii deja cunoscuți; orice item nou se adaugă cu context (ce s-a
  încercat, de ce e structural).

## În afara feliei (explicit)

NIR manual / LDI+ / ASM cu culegere de produs (mecanismul D2 e extensibil,
wiring-ul nu intră); localizarea shell-ului; repararea SmartLookup (Atlas.DXF);
ecrane pentru RLF/RDC/NTC/ITV/PLT/INC; Imperechere UI dincolo de 41d;
multi-tab staleness (40c — limitare asumată); perioade fiscale ≠ 2026 (seed
manual la nevoie); mina `IChatClient` (`Startup.cs:35-47`) — doar dacă mușcă
în smoke.

## Pașii (un agent per pas, secvențial; commit per pas după verificarea main-ului)

1. **Model + motor + migrație**: D1 (ProdusId + migrația `FacturaIntrareProdus`),
   D4 (Eticheta), coerența Tip↔Produs pe FCT, D6 (AsignaNumar), seam-ul de
   recalcul în TvaService (metodă publică refolosibilă). Verificare: build +
   ModelCheck AMBELE profiluri + zero drift migrații + check nou „refuzul nu
   consumă număr".
2. **Controllere**: D2 (loturi la culegere), D3+D5 (recalcul + default
   TipMaterial), D7 (Numar data-driven), D8 parțial (unde e nevoie de
   controller), D9 (Data=azi), D10 (storno cu dată), D11 (toast + erori).
   Verificare: build + review de cod al main-ului (smoke-ul real la pasul 4).
3. **Baseline + layout**: D12 integral (layouts, captions, ascunderi, ordine
   NIR/generic, appearance Stare, Total pe DetailView, ViewSummary pe grile cu
   fallback), coloana Produs pe linia FCT, Lot read-only pe FCT, comentariile
   stale. Verificare: build + aplicația pornește curat.
4. **Mediu privat + SMOKE UI REAL (browser, făcut de MAIN)** — checklist-ul de
   mai jos; fix-uri mici pe loc; luptele structurale → lista React.
5. **Review advers dedicat** (agent separat, scenarii concrete de exploatare pe
   cele două ecrane + controllerele noi) → fix-urile le aplică main-ul →
   decizia nouă în CLAUDE.md (+ corecțiile 40d/40e/41e) → commit final.

## Criteriile de trecere a gate-ului (checklist-ul smoke-ului, pasul 4)

**FCT:** culegere integrală din UI — furnizor/gestiune, număr furnizor
obligatoriu, linie de STOC (Produs → TipMaterial precompletat → lot născut
automat, nevizibil ca fricțiune), linie de serviciu, TVA default N21 +
override manual de ValoareTva respectat, `Valoare`/`Total` vizibile și egale
cu „hârtia" ÎNAINTE de operare; operare → NIR conex tipizat se deschide și se
operează la rândul lui; `GenereazaPlata` → plata autogenerată; refuz de
validare LIZIBIL (mesaje distinguibile); post-operare totul read-only.

**FCL:** culegere — client, gestiune de descărcare, linie cu Produs (+pin de
lot LIZIBIL, opțional), serie `FCL-` asignată la operare și neocolibilă,
totaluri vizibile; operare → DSC generat și deschis, backorder-ul raportat;
storno cu dată culeasă.

**Transversal:** niciun „Castle.Proxies.*", niciun FK brut, niciun câmp mort
pe cele două ecrane + NIR-ul conex; `Data` precompletată; refuzul unui gardian
nu lasă urme (număr/serie); ce NU trece = enumerat în lista React, nu
hack-uit.
