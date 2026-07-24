# CLAUDE.md — Migrare aplicație contabilitate (Delphi + SQL → XAF + React)

## Context general

Rescriem o aplicație de contabilitate/gestiune scrisă în Delphi + SQL Server.
Aplicația veche folosește un model generic de documente configurat prin tabele
(EAV-like). Generalizarea nu s-a plătit: ușor de implementat inițial, foarte
greu de întreținut, cu un număr mic și stabil de tipuri de documente.

**Tipurile de documente reale (acoperă >90% din nevoi, stabile de ani de zile):**
Factură, NIR, Bon de consum, Notă de transfer, Listă diferențe inventar,
Raport de producție.

## Arhitectura veche (legacy, în /legacy și /db)

- Tabelă unică de documente (header) + tabelă unică de detalii, cu `TipDocument`.
- Document generic: **predator** și **primitor**; tipul de document definește
  ce tip de predator/primitor permite (intern/extern/oricare).
- Pentru fiecare combinație permisă tip document × tip predator × tip primitor
  există o tabelă de configurare care descrie ce câmpuri se culeg și se
  validează în header/detalii.
- Peste acestea: reguli contabile și definiții de stocuri.
- **Clasă/Tip** pe nomenclatorul de produse și pe detalii: echivalent operațional
  al planului de conturi (Clasa ≈ prima cifră: imobilizare/materiale/cheltuială;
  Tip = detaliere până la nivel 2–3). Separă operaționalul de contabilitate;
  maparea pe conturi se face ulterior, declarativ.
- **Stocuri**: definite ca listă de semn aplicat pe cantitate (+/−) per tip
  document, detaliat pe tip primitor/predator, cu filtru.
  Ex: `Stoc produse = sum(cantitate × semn_linie)` pentru NIR, Bon consum etc.

## Decizii arhitecturale luate (NU redeschide fără motiv nou)

1. **Nucleu generic + moștenire, nu tabele complet separate.**
   Clase de bază `Document` (header) și `DocumentDetaliu`, cu clase derivate
   per tip de document. Motoarele de stoc și contabile consumă DOAR clasa de bază.

2. **Testul de apartenență a unui câmp** (aplicat obiectiv la fiecare câmp):
   - Apare într-o formulă de stoc sau regulă contabilă → **clasa de bază**.
   - Apare doar pe ecranul unui tip de document → **clasa derivată**.

3. **Mapare EF Core: TPT (table-per-type)** pentru header. Pentru detalii:
   de verificat la inventar dacă derivatele sunt necesare deloc (diferențele
   pot fi doar de obligativitate, nu de schemă → bază pură + validare).

4. **Structura devine cod, politica rămâne date:**
   - Devin cod (clase derivate + validare declarativă): definițiile de câmpuri
     per combinație tip/predator/primitor din tabelele de configurare.
   - Rămân date (tabele de politică editabile fără release): maparea contabilă
     Clasă/Tip → conturi, definițiile de stoc (semn × tip document × filtru).

5. **Backend: XAF** (business logic, securitate, validare, audit, rapoarte),
   expus prin XAF Web API Service / OData.
   **Frontend: React + DevExtreme** pentru ecranele operaționale.

6. **API-ul NU expune ierarhia polimorfic.** Endpoint-uri per tip de document
   (`/api/odata/NIR`, `/api/odata/BonConsum`, …) → DTO-uri TypeScript plate
   prin codegen OpenAPI→TypeScript. Opțional un endpoint read-only pe bază
   pentru registrul general de documente. Moștenirea e detaliu de persistență.

7. **Nav properties în Web API**: se expun cheile străine (FK) explicit și se
   aplică `JsonIgnore` pe proprietățile de navigație (pattern deja validat
   în producție).

8. **Metadata-driven UI** (pipeline în două straturi, din arhitectura XAF/React):
   build-time OpenAPI→TS pentru tipare; runtime endpoint-uri custom de metadata
   serializate din XAF Application Model (captions, editor types, lookups,
   coloane ListView, layout DetailView, validări simple). Server-authoritative:
   metadata se filtrează prin securitate PE SERVER înainte de serializare.

9. **Scope faza 1: contabilitate + gestiune.** Plățile/încasările intră acum,
   ca tipuri de document (fără reguli de stoc, doar reguli contabile).
   Salarizarea, imobilizările și execuția rămân module separate; intră în
   contabilitate prin import de note contabile.

10. **Plan de conturi doar sintetic.** Analiticele se derivă din dimensiuni
    la raportare/balanță (ex: `401.01.<cod_fiscal>`); nu se persistă în plan.

11. **Dimensiuni pe note: mecanismul COMPUSA se elimină.** Sursa de finanțare
    devine dimensiune explicită (nu se mai extrage din codul de sector al
    clasificației funcționale); denumirile dimensiunilor se normalizează.

12. **Migrare green-field la graniță de ciclu.** Nu se cere paritate
    document-cu-document cu legacy; migrarea se scrie specific (nomenclatoare,
    politici, solduri/stocuri de deschidere). Forma exactă a reconcilierii —
    de detaliat.

13. **Evaluare stoc: identificare specifică pe lot** (mecanism deliberat în
    legacy prin codmat unic per intrare, păstrat și făcut first-class).
    `Produs` (catalog, fost sumator) + `Lot` (fost codmat: creat de linia de
    intrare, preț unitar fix, gestiune, dată). Liniile de ieșire referă lotul;
    picking auto-FIFO în cadrul produs×gestiune, cu override manual.
    O singură metodă de evaluare în motor — fără CMP/FIFO paralel.

14. **Registre persistate, append-only.** Documentul are ciclu de viață
    `Draft → Operat → (Stornat)`; la operare motorul generează rândurile de
    registru (stoc + note) tranzacțional. Corecție directă/anulare permisă
    DOAR fără dependenți în perioada deschisă (dependență exactă pe loturi +
    verificare sold intermediar ≥ 0 pe loturile afectate); altfel storno.
    Perioada închisă = graniță absolută. Îngustare a deciziei 4: schema
    registrelor (care există, pe ce dimensiuni) = COD; regulile de alimentare
    (tip document × semn × filtru) = DATE.

15. **Dimensiuni: owned type `Dimensiuni`** (proprietate nullable + FK real
    per dimensiune: Repartitor, Material, CodFunctional, CodEconomic,
    SursaFinantare, Unitate, Proiect, CentruCost). Regula de notă poartă
    `Comun` / `OverrideDebit` / `OverrideCredit`; rezolvarea (coalesce) e o
    funcție generică în motor. Linia de document poartă `Dimensiuni` parțial;
    rândul de registru/notă poartă unul complet rezolvat. Flag-urile de
    defalcare din plan (R/M/E/B/F/P) devin date de validare: dimensiuni
    obligatorii per cont. Override-urile de pe linia legacy (ContD/ContC,
    RepD/RepC în GEST_ITEMSI — hack rapid, confirmat) NU se preiau; dacă
    apare nevoie reală, adăugarea de override-uri pe linie e pur aditivă.

16. **Repartitori: TPT bază + derivate, cu regula:** moștenire doar unde
    schema diferă și identitatea e exclusivă; calitățile transversale
    (centru de cost, delegat, cursant etc.) = roluri/flags, nu clase derivate.
    Furnizor/client = rol contextual dat de poziția pe document, o singură
    clasă `Partener`. Dublare acceptată pentru cazuri exotice.

17. **Plăți/încasări = tipuri de document** (înlocuiesc registrul separat
    bregistru/breg_p din legacy). **Imperecherea** (stingerea facturilor) =
    entitate many-to-many plată↔factură cu sume parțiale (în legacy: m2m
    gest_docum↔bregistru). **Document conex**: generare automată de document
    legat cu link persistat (ex. NIR din Factură, deschis automat în editare)
    — mecanism generic, de proiectat.

18. **Migrarea preia nomenclatoare + solduri de deschidere.** Regulile de
    politică se SEED-uiesc (definite curat în sistemul nou), nu se migrează.
    Istoricul de documente rămâne doar în legacy.

19. **Lista țintă de tipuri de documente (10 derivate):** FacturaIntrare,
    FacturaIesire (clase separate, nu comasate), NIR, BonConsum, NotaTransfer,
    RaportProductie (n materii prime → m produse; loturile produse se
    evaluează prin alocarea valorii consumurilor — istoric Otelinox/PPUP:
    preț produs manual din rețetar cu chei de distribuție; designul BPR se
    AMÂNĂ până la modulul de rețetar, clasa rămâne rezervată în ierarhie),
    ListaDiferenteInventar, Decont, Plata, Incasare.
    Proforma (FPR) exclusă. BF (bon fiscal / avans) = variantă de
    FacturaIntrare (același flux, conex NIR), NU tip separat (confirmat).

20. **Un singur nomenclator de tipuri, și e codul.** Nivelul GEST_TIP_DOCUM
    devine ierarhia de clase; nivelul intermediar GEST_DEFA_DOCUM (combinații
    tip × predator × primitor) dispare — fiecare clasă derivată își fixează
    semantica de direcție, iar datele vii confirmă (un singur combo activ per
    tip). Config-ul per-combo din legacy se separă: structură → clasă;
    politici (numerotare, document conex țintă, tipuri de stoc pe laturi) →
    rânduri seed în tabele de politică, cheiate pe un `TipDocument` seed care
    oglindește clasele 1:1 (doar ca ancoră FK + UI).

21. **Seed-ul politicilor se face PE FUNCȚIONALITATE, nu prin transcrierea
    config-ului legacy.** Configurarea legacy a fost făcută fără verificări;
    servește ca direcție/evidență, NU canonic. Inventarul documentează ce face
    sistemul + zgomotul cunoscut; politicile noi se definesc curat per cerință.
    Context: FCT vin parțial/complet dintr-un sistem extern (Tethys) —
    FacturaIntrare are nevoie de cale de import; tabelele `*_procent` au fost
    hack pentru lipsa sursei de finanțare (defalcare procentuală de la
    angajament în jos) — nevoia reală (cofinanțare multi-sursă pe linie) se
    rezolvă în modelul nou prin mecanism propriu de defalcare, de proiectat
    odată cu dimensiunea SursaFinantare (decizia 11/15).

22. **Testul bazei (pasul 2) — închis; rezultatul canonic în
    `db/inventar/11-testul-bazei.md`.** Baza `Document`: Numar, Data,
    PredatorId, PrimitorId, Stare(+DataOperare), DocumentSursaId+Autogenerat,
    Total calculat. Baza `DocumentDetaliu`: TipMaterialId (Clasa se derivă din
    Tip — un singur FK), LotId?, Cantitate (SEMNATĂ — direcția LDI devine
    `Directie` pe derivată, materializată în semn), Valoare, AngajamentId?,
    Dimensiuni (owned, integral, tot nullable). Tranșări: (a) lanțul de valori
    se taie la capăt — doar `Valoare` (valoarea de postare în registre,
    unifică RECEPTIE_TVA/LIVRARE) e în bază, intermediarele
    (pret/cotă/TVA/valoare fără TVA) sunt per derivată; TVA separat nu e în
    bază — adăugare ulterioară aditivă. (b) granița dimensiunilor nu e în
    schemă: owned type integral pe bază, ce diferă per tip = validare
    declarativă + metadata UI. (c) AngajamentId = FK nullable pe bază;
    modulul angajamente (head+detaliu+self-ref, tipuri legal/buget
    anual/multianual) se proiectează separat. (d) contul pe linia Factura* =
    politică per tip partener și/sau Clasă-Tip; câmp explicit doar ca fallback
    ulterior, aditiv. (e) DECONT_* = câmpuri persistate pe FacturaIntrare;
    DataScadenta și NumarPV/DataPV = interfețe (`IDocumentCuScadenta`,
    `IDocumentCuPV`). NR_NOTA moare de pe document (numărul notei aparține
    registrului contabil).

23. **Felia 3a — executată; deciziile fixate la implementare:**
    (a) **Schema = migrații EF Core, canonic.** `BackOfficeDesignTimeDbContextFactory`
    în Module (Postgres localhost:5444, oglindește appsettings); update-ul automat
    de schemă XAF e DEZACTIVAT (`SchemaUpdateOptions.DisableUpdateSchema`).
    Seed-ul (ModuleUpdater) se rulează cu
    `dotnet run --project ...Blazor.Server -- --updateDatabase --forceUpdate --silent`;
    în DEBUG updater-ul rulează și fără debugger atașat.
    (b) **`ClasaProdus.Natura`** (enum `NaturaClasa`: Stoc/Serviciu/Cheltuiala/
    Imobilizare/Tehnica) — curățarea din inventar 10 §2: doar Natura=Stoc intră
    în regulile de stoc; clasele tehnice (TVA, diferențe) doar la contare.
    (c) **NotaTransfer nu are RegulaContare** — la plan sintetic transferul nu
    mișcă conturi; mutarea între gestiuni trăiește în registrul de stoc și în
    dimensiunea Repartitor (notele 3xx=3xx din legacy = zgomot, nu se preiau).
    (d) **Planul de conturi se seed-uiește integral din CPLAN** (1.679 sintetice,
    CSV embedded `DatabaseUpdate/SeedData/plan-conturi.csv`, diacritice
    normalizate); defalcarea BALANTA → `DimensiuniObligatorii` cu legenda
    CPLAN_DEFALCARE (R/M/F/E/B/P; T=Titlu → CodEconomic).
    (e) Utilitar `nou/tools/ModelCheck` (consolă): validare model + verificare
    migrații/seed contra bazei.

24. **Owned `Dimensiuni` sub XAF/EF Core — limitarea e gestionabilă, rămânem
    pe EF Core (nu XPO).** Docs XAF declară owned types „nesuportate", dar
    verificat empiric (26.1.3, UI Blazor + surse DevExpress): TypesInfo,
    startup, securitate, ListView/DetailView funcționează; singura rupere
    reală era la creare — DbContext-ul XAF cere notificări complete
    (`ChangingAndChangedNotificationsWithOriginalValues` + proxies), iar
    instanța `new Dimensiuni()` e POCO fără `INotifyPropertyChanging`.
    Fix (promovat în Atlas.DXF.EfCore 26.1.3.2, pachet referit din Module):
    `Dimensiuni : OwnedObjectBase` (baza implementează
    `INotifyPropertyChanging/Changed` + helper `SetPropertyValue`; aici rămân
    doar perechile backing-field + proprietate virtuală), iar maparea folosește
    `OwnsOneRequired` (OwnsOne + navigație required). Owner-ii noi creați în
    cod trebuie să fie proxy (`CreateProxy`, cum face XAF/ObjectSpace implicit).
    Rețeta completă: xaf-kb `recipes/atlas-dxf/efcore-owned-types.md`.
    NuGet: `nou/nuget.config` mapează Atlas.DXF.* → feed Atlas; EF Core /
    Npgsql / System.Security.Cryptography.Xml au trecut pe versiuni flotante
    `10.0.*` (cerință de compatibilitate cu pachetul).
    Round-trip insert/update/materializare all-null verificat în ModelCheck;
    fără drift de migrații. Proprietățile Dimensiuni apar în UI XAF ca
    read-only ToString (inofensiv); editarea lor în back-office (când va fi
    nevoie, la 3c/3d — RegulaContare) se rezolvă aditiv: wrappers delegați
    `[NotMapped]` sau ecran React. Tierul Web API pentru React (pasul 5)
    rămâne de validat pe owned la momentul lui — DTO-urile plate (decizia
    6/7) fac oricum flattening explicit.

25. **Felia 3b — executată; motorul de operare în `Module/Motor/`
    (`MotorOperare`, `StocService`, `DimensiuniResolver`, `GardianPerioada`),
    validat end-to-end pe NotaTransfer în ModelCheck (28 verificări, pe
    `EFCoreObjectSpaceProvider` standalone — docs 113709). Tranșările:**
    (a) **RegistruContabil poartă dimensiuni PER LATURĂ**:
    `DimensiuniDebit`/`DimensiuniCredit` (două owned) înlocuiesc
    `RepartitorDebit/Credit` + setul unic — regula are Override per latură,
    deci rezultatul rezolvat diferă (echivalentul tripletelor dim_d/dim_c din
    CNOTE). Coalesce per latură: linie → override(latură) → comun → default
    header (debit←Predator, credit←Primitor — 00 §5).
    (b) **Hooks polimorfe pe bază**, consumate doar de motor:
    `PregatesteOperare(os)` (derivata materializează `Valoare` — BTR/BCS:
    preț lot × cantitate) și `ValideazaOperare(os, erori)`. Lucrează pe
    FK-uri + IObjectSpace, NU pe navigații (contextul apelant nu garantează
    lazy loading); motorul preîncarcă Clasa/Natura per Tip la fel.
    (c) **Loturile nu se nasc în motor** (baza nu are ProdusId — testul
    bazei): linia de intrare creează Lotul la culegere (`Lot.LinieIntrare`),
    motorul îl FINALIZEAZĂ la operare (PretUnitar = Valoare/Cantitate, Data).
    Se exersează la 3c/NIR.
    (d) **Gardieni**: perioadă lipsă = închisă; sold intermediar = prefix-sum
    pe zile per cheie (Lot × Repartitor × TipStoc) ≥ 0 — acoperă și operarea
    retroactivă; anularea (corecția directă) simulează eliminarea rândurilor
    proprii + cere ca loturile create să nu fie atinse de alte documente;
    storno = rânduri inverse (flag `Storno`) la data stornării, cu aceeași
    verificare din acea dată încolo; grup conex: gardian conservator (refuz
    cât timp există copii `DocumentSursaId` operați) până la mecanismul
    complet din 3c.
    (e) **`RegistruStoc/Contabil.DocumentId` e nullable**: null = rând de
    sold de deschidere scris de migrare (decizia 12), fără document sursă.
    (f) Numerotarea se asignă la operare din `PoliticaNumerotare` (seed BTR).
    UI: `DocumentOperareController` (Operează/Anulează/Stornează) doar
    deleagă la motor — aceeași cale o va folosi tierul Web API (pasul 5).
    Limitare asumată, documentată în cod: verificarea de sold și commit-ul nu
    sunt serializate între utilizatori concurenți (back-office cu operator
    unic); la nevoie: advisory lock Postgres per cheie de stoc, aditiv.

26. **Felia 3c-1 — NIR + FacturaIntrare + mecanismul conex — executată;
    validată e2e în ModelCheck (FCT cu linie stoc + linie serviciu → NIR conex
    → operare → gardieni grup → storno lanț). Tranșările:**
    (a) **Întrebarea 00 §13.1 închisă:** recepția CONTEAZĂ pe NIR (3xx =
    furnizor, valoarea cu TVA capitalizat); FacturaIntrare postează DOAR
    liniile care nu trec pe NIR (servicii/cheltuieli/imobilizări). Granița e
    Natura clasei — aceeași care filtrează conexul; fără dublă postare.
    (b) **Conturile regulii de contare se rezolvă declarativ** (testul bazei
    §7.2): enum `SursaCont` per latură (Explicit / TipMaterial /
    PartenerPredator / PartenerPrimitor), contul explicit al regulii = valoare
    directă sau fallback. `TipMaterial.ContImplicit` (FK nou) = maparea
    Clasă/Tip → cont ca DATE (decizia 4), seed-uită din simbol (Cod-ul Tipului
    E simbol de cont — potrivire exactă, apoi tăierea segmentelor terminale:
    302.02.00.2 → 302.02.00). `Partener.ContImplicit` particularizează
    creditorul (404 la furnizorii de imobilizări), fallback 401.01.00.
    (c) **Prioritate la potrivirea regulilor**: RegulaContare — TipMaterial
    exact → `NaturaFiltru` (câmp nou) → generică; fără regulă = linia nu
    contează pe acel tip de document. RegulaStoc — regula specifică pe Clasă
    bate genericul (Clasa=null = orice Natura=Stoc) per latură. Seed NIR:
    +1 primitor, generic→Magazie, G/OF/MF/MC→registrele proprii.
    (d) **PoliticaConex.NaturaFiltru înlocuiește lista m2m de tipuri permise**
    (filtrul de conținut e funcțional natura liniei — decizia 21). Generarea
    conexului trăiește ÎN motor, în tranzacția operării sursei: draft
    `Autogenerat` cu `DocumentSursa`, clonă header (`InverseazaLaturi`) +
    liniile eligibile (lot/dimensiuni/valoare incluse); fără linii eligibile
    ⇒ nu se generează. `Opereaza` întoarce conexul; controller-ul îl deschide
    în editare. Grup conex la anulare/storno: copiii Operați → refuz
    (conservator, există din 3b); copiii DRAFT autogenerați se ȘTERG odată cu
    anularea sursei (artefact al operării; re-operarea regenerează).
    (e) **Lotul se naște la culegere pe linia FACTURII** pentru lanțul conex
    (echivalentul legacy: GEST_GNMCL.id_document_intrare = FCT):
    `DocumentDetaliu.CreeazaLot(os, produs, gestiune)`; NIR-ul conex preia
    LotId, NIR-ul manual își creează loturile pe propriile linii; motorul
    finalizează lotul la operarea documentului-mamă și copiază atributele
    culese (`ILinieCuAtributeLot`: DataExpirare/LotFabricatie).
    `Lot.LinieIntrareId` = coloană FĂRĂ constrângere FK (intenționat): FK pe
    ambele sensuri linie↔lot = ciclu de inserție pe care EF nu-l sparge, iar
    ObjectSpace-ul XAF comite totul într-un singur SaveChanges.
    (f) Validările proprii tipului în `ValideazaOperare` — FCT: numărul
    furnizorului obligatoriu (FCT NU are politică de numerotare), clasificație
    bugetară per linie (angajament SAU cod economic), laturi Partener→Gestiune,
    cantități pozitive, linia de stoc cu lot creat; NIR: laturi
    Partener→Gestiune, lot per linie, lotul în gestiunea primitoare.
    Reconfirmat 25b în hooks: interogările NU ating navigații lazy în timpul
    enumerării (proiecție cu Select înainte de materializare).
    (g) Seed self-healing: regulile de contare cu sursă Explicit și fără cont
    debitor (imposibil de rezolvat — reziduu de evoluție de schemă) se șterg
    și se recreează la updater.

27. **Felia 3c-2 — BonConsum — executată; validată e2e în ModelCheck (sold
    deschidere → operare → gardieni → anulare directă → storno). Tranșările:**
    (a) **Consumul alimentează DOUĂ registre simultan** (inventar 03, defa 65):
    RegulaStoc −1 Magazie pe predator + +1 Consum pe primitor (generice,
    Natura=Stoc) — consumul nu „dispare", rămâne pe responsabilul locului de
    consum (obiecte în folosință / responsabilități).
    (b) **Locul de consum = calitatea transversală `LocConsum`** (decizia 16),
    nu clasă derivată: ValideazaOperare cere predator Gestiune și primitor
    intern purtător al flag-ului; seed-ul pune flag-ul pe SEDIU (cu `|=`,
    idempotent). Lotul NU se leagă de gestiunea predatoare prin validare —
    locația curentă e soldul din registru, gardianul de sold refuză consumul
    de unde lotul nu există (așa rămâne permis consumul loturilor transferate).
    (c) **Contarea 6xx = 3xx per Clasă/Tip se DERIVĂ la seed** (echivalentul
    curat al celor 18 modele legacy): rând RegulaContare per TipMaterial cu
    Natura=Stoc și simbol 3xx — debit Explicit = simbolul cu prima cifră 3→6
    (301→601, 302.01→602.01, 303.01→603) cu aceeași tăiere de segmente ca la
    ContImplicit; credit = SursaCont.TipMaterial (contul de stoc al Tipului).
    Seed incremental: tipurile fără rând primesc regulă la fiecare updater;
    tipurile non-3xx (532/409 — bonuri valorice) nu contează pe BCS — rând
    manual dacă apare nevoia reală (decizia 21).
    (d) Valoare = preț lot × cantitate în PregatesteOperare (ca BTR — prețul
    nu se culege); BCS e frunză în graful de dependențe ⇒ corecția directă
    aproape întotdeauna permisă (03), reconfirmată de gardienii generici.

28. **Felia 3c-3 — ListaDiferenteInventar — executată; validată e2e în
    ModelCheck (sold deschidere → LDI cu minus + plus pe aceeași listă →
    gardieni → anulare directă → storno). Tranșările:**
    (a) **Direcția explicită se materializează în semn la operare**
    (`Directie` pe detaliu — testul bazei §4/22): UI-ul culege cantitatea
    pozitivă, `PregatesteOperare` semnează Cantitate ȘI Valoare (minus =
    cantitate negativă × preț lot; plus = cantitate × `PretEvaluare` cules) —
    idempotent (Abs înainte de semnare). Un SINGUR set de reguli de stoc:
    +1 pe predator (gestiunea inventariată), cantitatea semnată dă direcția
    (exact mecanismul legacy defa 270, cu direcția explicită în loc de
    convenție); tip stoc per clasă ca la NIR (generic→Magazie, OF→Folosință,
    MC→Custodie).
    (b) **`RegulaContare.SemnFiltru` (±1/null)** — extensia aditivă care
    desparte contarea pe direcție (inventar 05: LDI e singurul tip din set
    unde semnul chiar diferențiază). Nepotrivirea de semn scoate regula din
    joc la TOATE nivelurile de specificitate (plusul sare peste regula exactă
    de minus și cade pe genericul de plus); valoarea postată se normalizează
    cu semnul filtrului (`(SemnFiltru ?? +1) × Valoare`) — nota minusului e
    pozitivă deși linia poartă valoare negativă.
    (c) **Seed contare LDI**: minus = 6xx = 3xx per TipMaterial cu
    SemnFiltru=-1 — aceeași derivare din simbol ca la BCS, extrasă în
    helper-ul comun `SeedContare6xxDin3xx` (incremental la updater); plus =
    un rând generic Natura=Stoc, SemnFiltru=+1, debit din contul Tipului,
    credit explicit **791.00.00** (CPLAN nu are 758 — 791 „Venituri din
    valorificarea unor bunuri ale statului" e echivalentul din planul
    instituțiilor publice).
    (d) **Plusul creează lot** la culegere pe propria linie (`CreeazaLot`,
    gestiunea = predatorul), cu `ILinieCuAtributeLot` (LotFabricatie/
    DataExpirare pe poziție — inventar 05); motorul îl finalizează la operare
    (PretUnitar = PretEvaluare). Minusul descarcă un lot EXISTENT (validare:
    lotul nu e creat de linia proprie; refolosirea lotului altcuiva pe plus =
    refuz). **Primitorul = comisia de inventariere** — calitatea transversală
    `Comisie` (decizia 16), seed repartitor `COMISIE` (UnitateInterna, `|=`).
    (e) Validări proprii: predator Gestiune, primitor intern cu Comisie,
    direcție setată explicit (enum-ul nu are default valid — protecție la
    linii culese fără direcție), cantitate ≠ 0, lot per linie, plus cu preț
    de evaluare pozitiv și lot în gestiunea inventariată.

29. **Profil contabil: privat-first ca direcție de produs, bugetar ca profil
    de validare.** Motorul e agnostic la plan (niciun simbol hardcodat —
    totul prin RegulaContare/SursaCont/ContImplicit pe FK-uri); ce e specific
    planului trăiește în seed. Se introduce conceptul de **profil contabil**
    = pachet de seed (plan CSV + Clasă/Tip + politici + validări specifice):
    CPLAN devine *profilul bugetar*, primul din două — nu „planul aplicației".
    Tranșări: (a) profilul bugetar rămâne vehiculul de lucru până trece
    pasul 4 — reconcilierea cu legacy se poate face DOAR pe planul în care
    trăiesc soldurile; (b) singura scurgere bugetară din cod — validarea FCT
    „angajament SAU cod economic" — se mută din clasa de document spre
    politică/profil la 3d (validarea declarativă); (c) profilul privat
    (OMFP 1802) se scrie când apare clientul privat: al doilea CSV + propriul
    set de politici (diferă de CONȚINUT, nu doar simboluri: plus inventar
    3xx=6xx/345=711 la privat vs 3xx=791 la bugetari; mărfuri 371→607, nu
    671 — derivările de seed sunt per-profil, mecanismele — Cod Tip = simbol,
    tăierea segmentelor — se transferă); (d) dimensiunile bugetare din owned
    (CodFunctional/CodEconomic/SursaFinantare) rămân în model — nullable,
    inofensive la privat; ALOP/angajamente/execuție = modulele „bugetar
    peste" (deciziile 9, 22c), stratificarea există deja.

30. **Felia 3c-4 — FacturaIesire — executată; validată e2e în ModelCheck
    (laturi → refuz stoc → operare cu scadență default → debit particularizat
    → anulare directă → storno). Tranșările:**
    (a) **FCL = pur creanță (411 = 7xx), NICIO regulă de stoc** (inventar 07:
    în acest profil facturarea nu descarcă gestiune). Liniile cu Natura=Stoc
    se REFUZĂ la validare — vânzarea de bunuri din stoc = document/reguli
    proprii când apare nevoia; ca la FCT (29b), validarea hardcodată migrează
    spre declarativ la 3d.
    (b) **Contarea = un singur rând generic**: debit `SursaCont.PartenerPrimitor`
    (ContImplicit al clientului — ex. 461 debitori; fallback 411.01.01),
    credit `SursaCont.TipMaterial` FĂRĂ fallback (Tip fără cont = eroare clară
    la operare). Contul de venit „ales pe linie" din legacy devine alegerea
    Tipului: clasă nouă **VEN** (Natura=Serviciu) cu tipuri din planul bugetar
    — 751.01.00 (prestări servicii), 750.02.00 (chirii), 751.04.00 (diverse);
    la privat ar fi 704/706/708 — seed per profil (decizia 29).
    (c) **`PoliticaScadenta` (tabelă nouă: TipDocumentId + ZileDefault)** —
    formula de header legacy `DATA_SCADENTA = data+30` devine politică-date;
    motorul o aplică generic pe `IDocumentCuScadenta` la operare, DOAR dacă
    scadența nu a fost culeasă. FCT nu are rând seed (scadența furnizorului
    se culege) — neatins.
    (d) **Numerotare proprie `FCL-`** (serie fiscală — invers față de FCT).
    TVA rămâne în `Valoare` (o singură valoare de postare — testul bazei 22a);
    defalcarea 4427 TVA colectată = adăugare ulterioară aditivă (deployment
    neplătitor, cota default 0). Dimensiunile urmează convenția 00 §5
    nemodificată (debit←emitent, credit←client). Fără cerință de clasificație
    bugetară (veniturile sunt exceptate de la angajament — 00 §10).
    (e) **Fix seed fresh-install**: derivările care interoghează baza
    (ContImplicit din simbol, 6xx=3xx) nu vedeau nomenclatoarele create în
    același run — `Seed()` comite nomenclatoarele înainte de derivări.

31. **Felia 3c-5 — Plata/Incasare + Imperechere — executată; validată e2e în
    ModelCheck (FCT cu DECONT_* → plată autogenerată + imperechere automată →
    gardieni → încasare manuală cu imperechere manuală → avans 542 → storno).
    Tranșările:**
    (a) **Laturile tipizate, liniile = defalcarea sumei** (echivalentul BREG_P):
    Plata = ContPropriu → Partener/Angajat, Incasare invers; linia poartă
    `Valoare` culeasă direct + `Dimensiuni`, fără stoc/lot/cantitate. Tipul
    liniei la culegere manuală = tehnicul `TRZ` (Clasa TRZ, Natura=Tehnica,
    fără ContImplicit); liniile autogenerate păstrează Tipul liniei sursă.
    (b) **`ContImplicit` urcă pe baza `Repartitor`** (testul apartenenței:
    intră în rezolvarea declarativă pentru ORICE latură) — partener 401/404/411,
    ContPropriu 5xx/770, Angajat 542 (avans); `SursaCont.Partener*` redenumit
    `Repartitor*` (valorile int neschimbate); migrarea mută datele existente.
    (c) **Contare: un rând generic per tip, ambele conturi din laturi** —
    PLT: debit=RepartitorPrimitor (fallback 401.01.00), credit=
    RepartitorPredator FĂRĂ fallback (cont propriu fără cont = eroare clară);
    INC: oglindit (fallback 411.01.01 pe credit). Conturi proprii seed
    (legacy `casierie`): CASA→531.01.01, TREZ→770.00.00 (bancă).
    (d) **Imperecherea NU e document** — link între două documente OPERATE;
    invarianții trăiesc în `ImperechereService` (motor, aceeași cale pentru
    UI/Web API): ambele Operat, suma>0, Σ imperecheri ≤ totalul fiecărei
    părți, contrapartida trezoreriei (latura non-ContPropriu) apare pe
    documentul stins — acoperă și lanțul avans↔decont↔regularizare (un doc
    poate sta pe AMBELE roluri; Asignat numără ambele coloane). `ramas` =
    calcul. Gardian nou în motor: anulare/storno refuzate cât există
    imperecheri (link-ul se șterge liber, fără registre proprii).
    (e) **Plata automată (00 §7) = hook `Document.GenereazaSecundar`** — spre
    deosebire de conexul-clonă din PoliticaConex, secundarul se construiește
    din date CULESE pe derivată: FCT cu `GenereazaPlata` → draft `Plata`
    autogenerat (header din DECONT_*, liniile clonează defalcarea facturii:
    valoare+dimensiuni+angajament); motorul îl marchează
    Autogenerat+DocumentSursa → copil normal al grupului conex (șters la
    anularea sursei, blochează anularea cât e operat). La operarea plății
    autogenerate motorul creează imperecherea automată pe restul stingibil.
    `Opereaza` întoarce `conex ?? secundar`. GenereazaChitanta rămâne
    neactivat (nu are cont propriu cules — se tratează la fluxul BF, aditiv).
    (f) Amânate, documentate: transferul între conturi proprii (pereche
    PLT+INC conexă, 581 — 09 §4), importul extraselor de trezorerie
    (xml_trezor — la API, ca importul FCT), imperecherea pe poziții
    (GEST_DEFALCARE_DECONTARI, 00 §13.3 — nivelul de document ajunge),
    obligativitatea clasificației bugetare pe liniile de plată (validarea
    declarativă 3d, ca 29b).

32. **Felia 3c-6 — Decont — executată; validată e2e în ModelCheck (avans →
    decont cu postare explicită pe linie → imperecherea lanțului
    avans↔decont↔regularizare → gardieni → anulare → storno). Tranșările:**
    (a) **Postarea explicită pe linie = contract de interfață
    `ILinieCuPostareExplicita`** (ContDebit/ContCredit/RepartitorDebit/
    RepartitorCredit, toate opționale) — trăsătura PROPRIE a tipului (inventar
    06, nuanța deciziei 15): motorul o consultă înaintea rezolvării declarative
    (contul liniei bate SursaCont) și ca nivel maxim al coalesce-ului de
    dimensiuni (repartitorul per latură), dar NUMAI tipurile care declară
    interfața o au — nu devine mecanism generic (baza DocumentDetaliu nu
    poartă câmpurile; azi doar DecontDetaliu).
    (b) **Laturi: predator = Angajat (titular), primitor = unitate internă**
    (UnitateInterna/Gestiune); fără reguli de stoc. Contare: un rând generic —
    debit `SursaCont.TipMaterial` FĂRĂ fallback (Tip fără cont + linie fără
    explicit = eroare clară la operare), credit `RepartitorPredator` cu
    fallback 542.01.00 (acoperă angajatul fără ContImplicit).
    (c) **Convenția 00 §5 devine default POLIMORF**:
    `Document.RepartitorImplicitDebit()/Credit()` (debit←Predator,
    credit←Primitor) — ultimul nivel de coalesce în motor; Decont mută
    creditul pe TITULAR (soldul avansurilor 542 se ține per angajat, nu per
    primitorul justificării).
    (d) Cantitatea e pro-formă (legacy BUC/1): `PregatesteOperare` o
    normalizează 0→1 și materializează Valoare = PretUnitar × Cantitate ×
    (1+TVA). Clasificația bugetară per linie ca la FCT — hardcodată până la
    validarea declarativă (3d, ca 29b). Lanțul avans→decont→regularizare =
    imperecheri (31d), verificat e2e cu avansul stând pe ambele roluri.
    Fără migrație: schema DecontDetaliu există din 3a.

33. **Felia 3d — validarea transversală — executată; validată e2e în ModelCheck
    (177 verificări: refuz per cont pe 404/BFEPR, puntea angajamentului,
    politici per tip pe FCT/DEC/PLT/FCL, venituri/trezorerie cu E). Tranșările:**
    (a) **Dimensiunile obligatorii per cont = gardian generic în motor**
    (decizia 15 închisă): flag-urile `Cont.DimensiuniObligatorii` se verifică
    pe seturile REZOLVATE ale rândului contabil, per latură (debitul contra
    DimensiuniDebit, creditul contra DimensiuniCredit); toate lipsurile se
    raportează împreună, cu simbolul contului și linia. **Puntea E**: până la
    modulul de angajamente, `AngajamentId` pe linie satisface cerința de
    CodEconomic (clasificația trăiește în angajament; când modulul apare,
    rezolvarea va materializa CodEconomic din angajament și puntea moare).
    (b) **Dimensiunea Material se rezolvă implicit din lot** (`Lot.Produs` →
    MaterialId, ambele laturi, ultimul nivel de coalesce alături de
    repartitorul implicit polimorf) — închide nota „de confirmat la motor";
    în CPLAN-ul bugetar niciun cont nu cere azi M, dar mecanismul e viu.
    (c) **`PoliticaValidare` (tabelă nouă): obligativitățile per tip ca profil
    de validare** (decizia 29) — `CereClasificatieBugetara` (angajament SAU cod
    economic per linie) și `NaturaInterzisa`. Seed bugetar: FCT/DEC/PLT cer
    clasificație (29b/32d închise; 31f închis — INC nu primește rând: veniturile
    n-au angajamente, iar defalcarea E a conturilor de trezorerie cere oricum
    codul economic la nivel de cont); FCL interzice natura Stoc (30a închisă).
    Hardcode-urile echivalente din FacturaIntrare/Decont/FacturaIesire au fost
    șterse. Motorul aplică politica generic, înaintea hook-ului
    `ValideazaOperare` al tipului, cu erorile cumulate în aceeași listă.
    (d) **`Opereaza` restructurat pe calculează-validează-materializează**:
    rândurile contabile se calculează (regulă, conturi, dimensiuni) și se
    validează ÎNAINTE de orice `CreateObject` de registru (ca mișcările de
    stoc din 3b) — un refuz al oricărui gardian (sold, cont nerezolvabil,
    dimensiuni lipsă) nu lasă rânduri-fantomă în ObjectSpace-ul apelantului;
    finalizarea loturilor s-a mutat tot înaintea materializării.
    (e) **Invarianții imperecherii erau deja transversali** (31d, în
    `ImperechereService` + gardianul de anulare/storno din motor) — nimic nou;
    re-confirmați e2e. Consecință de date asumată: activarea flag-urilor CPLAN
    cere acum cod economic pe veniturile facturate (751/750 — E), plusul de
    inventar (791 — E) și liniile de trezorerie (531/542/770 — E) — cerințe
    reale ale profilului bugetar, editabile ca date. Felia 3d ÎNCHISĂ.

34. **Pasul 4 — migrarea datelor — executat; unealta `nou/tools/Migrare`
    (consolă SQL Server → Postgres prin `EFCoreObjectSpaceProvider`, ca
    ModelCheck), validată pe Contabilitate_2026. Tranșările:**
    (a) **Sursa = deschiderea materializată de trecerea de an legacy** în baza
    anului nou (`solduri_repartitori` + LDI-ul administrativ de 31.12 scris de
    `spPreiaStocuriInitiale` + `gest_gnmcl`), NU recalcularea închiderii:
    legacy își închide anul cu procedurile proprii, migrarea preia rezultatul.
    La go-live: întâi trecerea de an legacy, apoi unealta.
    (b) **Idempotență prin `MigrareLegatura`** (tabelă nouă: Tabela +
    CheieLegacy → TintaId, index unic) pentru sursele cheiate pe id
    (REPARTITORI, CASIERIE, GEST_SUMATOR, GEST_GNMCL, OI_*); nomenclatoarele
    cu cod natural (CodFunctional/CodEconomic, perioade) fac upsert pe cod.
    Rândurile de deschidere (DocumentId=null — decizia 25e, exclusiv
    proprietatea migrării) se rescriu integral la fiecare rulare.
    (c) **Clasificarea repartitorilor** (inventar 10 §1 → decizia 16):
    purtătorii de stoc de deschidere FORȚAȚI `Gestiune` (lotul cere gestiune —
    semnalul datelor bate eticheta legacy), apoi Gestiuni(7/18)→Gestiune,
    Salariați(3)→Angajat, Unități/Departament(11/19)→UnitateInterna, restul
    clasificărilor de identitate→Partener; neclasificați: GESTINT=1→
    UnitateInterna, altfel Partener. Gestionar/Comisie/Departament/CentruCost/
    LocConsum → `Calitati` (cu `|=`). ContImplicit din REPARTITORI.CONT cu
    tăierea segmentelor; `casierie` → ContPropriu de sine stătător (CRSP_LEI →
    ContImplicit, denumirea-IBAN → Iban).
    (d) **Solduri contabile = rânduri RegistruContabil contra 891.01.00**
    („Bilanț de deschidere", există în CPLAN) la 31.12.(an−1), dimensiunile
    (R/CF/CE/U/P) pe latura contului, analiticele legacy tăiate la plan.
    Clasa 8 (17,2M — evidența angajamentelor/creditelor bugetare) NU se
    migrează — vine cu modulul angajamente (deciziile 9/22c). SursaFinantare
    rămâne goală (mecanismul de defalcare — decizia 21). **Terții pornesc pe
    sold per partener, FĂRĂ facturi/imperecheri istorice** — exact modelul
    legacy (GEST_DECONTARI începe gol la an nou); plățile pe facturi vechi
    sting soldul de deschidere fără imperechere.
    (e) **Stoc = Lot per codmat + rând RegistruStoc per linie LDI** (tip stoc
    din clasa Tipului — maparea NIR/28a); produsul din sumator (surogat per
    codmat unde lipsește), migrate DOAR cele referite de deschidere (inventar
    10 §3). PretUnitar din linia LDI, atributele din gnmcl; Lot.Data =
    data_cod reală (ordinea FIFO istorică se păstrează).
    (f) **Reconcilierea** (contractul pasului 4): soldurile citite ÎNAPOI din
    Postgres = soldurile legacy per cont (0 diferențe; 891 se închide la 0;
    stocul nou = LDI-ul legacy per cont); nepotrivirea stoc↔contabilitate a
    LEGACY-ului (303.x fără detaliu de stoc în sursă ~2,06M; combustibil) se
    RAPORTEAZĂ, nu se ascunde — diferențele sunt ale sursei și se tranșează la
    go-live (inventariere sau acceptare explicită).
    (g) Amânate, documentate: adrese/conturi bancare/delegați (satelit,
    aditiv), JUDETE/LOCALITATI/valute, importul FCT (Tethys) și extrasele de
    trezorerie (la pasul 5, ca API).

35. **Pivot privat-first: privatul devine sursa principală de cerințe;
    legacy-ul pierde statutul canonic.** Pasul 4 a încasat valoarea ancorei
    legacy (reconcilierea a validat modelul pe date reale); direcția se ia de
    acum din produsul privat — decizia 29 promovată din „direcție" în mod de
    lucru. Tranșări:
    (a) **Importul din legacy = proiect separat, viitor**, tratat ca import
    din orice sursă; unealta Migrare îngheață ca prim prototip de conector;
    profilul bugetar rămâne pachet de seed funcțional (dovada agnosticismului
    motorului); modulele bugetare (ALOP/angajamente/execuție) se reiau după ce
    aplicația privată e rotundă — structura documentelor se așteaptă să ducă
    greul și acolo.
    (b) **1C primește statutul deciziei 21** (evidență/direcție, niciodată
    canonic): MD-uri + view-uri peste structura generică
    (_DocumentXXX/_CatalogXXX) + bază istorică consistentă (ideal un an fiscal
    complet, cu închiderile de TVA) = ținta de reconciliere a profilului
    privat și primul conector al proiectului de import.
    (c) **SAF-T (D406) + e-Factura (UBL) + D394 = checklist orizontal de
    completitudine** a modelului (atribute parteneri/conturi/tipuri de TVA),
    NU modele de date; librăria e-Factura (API+UBL) și exportul SAF-T din 1C
    există ca proiecte izolate ale utilizatorului — se integrează la momentul
    lor.
    (d) **Tenancy: bază-per-client** (profil contabil per bază) — rezolvă
    scalarea produsului și limitarea 25f fără schemă nouă. Disciplina de hot
    path rămâne: raportarea trăiește pe registre; nicio interogare polimorfă
    pe `Document` în fluxuri calde.
    (e) Ordinea fazei private: **P1** = profil privat + TVA structural
    (nomenclator TipTva mapat pe codurile SAF-T/D394, defalcare 4426/4427);
    **P2** = FacturaIesire completă cu descărcare de gestiune; apoi polish XAF
    pe modelul stabilizat; pasul 5 (API+React) neschimbat.

36. **Felia P1 — profil privat + TVA structural — executată** (designul fixat
    `docs/privat/p1-tva-design.md`); validată e2e în ModelCheck pe AMBELE
    profiluri. Tranșările de implementare:
    (a) **`TipTva` (nomenclator: cotă × regim + conturile 4426/4427/4428 ca
    date + mapări SAF-T/D394) și `TipTvaId?`+`ValoareTva` pe baza
    `DocumentDetaliu`**; `CotaTva` ștearsă de pe cele trei derivate (migrația
    `TvaStructural`). Calculul = helper comun `TvaService.CalculeazaValori`
    (formula §3: Capitalizat → brut în Valoare; Normal/TI → net + ValoareTva;
    Scutit/Neimpozabil/null → net), apelat din `PregatesteOperare` al
    FCT/FCL/DEC; pe FCT `ValoareTva` nenulă culeasă NU se suprascrie (factura
    furnizorului bate rotunjirea). `Document.Total` explicit BRUT
    (Σ Valoare+ValoareTva) — `ImperechereService.Total` și plata autogenerată
    sting brutul (liniile plății clonează Valoare+ValoareTva; TipTva rămâne
    null pe plată).
    (b) **Pasul TVA în motor** (faza „calculează" din `Opereaza`, înaintea
    gardianului de dimensiuni): per linie cu ValoareTva≠0 și regim care
    postează, condiționat de rândul `PoliticaTva` al tipului (Directie +
    SursaContrapartida + fallback — tabelă nouă, simetrică PoliticaScadenta/
    PoliticaValidare); TaxareInversa = 4426=4427 indiferent de direcție;
    dimensiunile rândului TVA: linie → default polimorf header + Material din
    lot, FĂRĂ override-uri de regulă. Rândurile TVA intră în aceeași listă de
    note → `VerificaDimensiuniObligatorii`, materializarea și storno le
    acoperă natural. Conexul clonează `TipTvaId` ca informație, NU ValoareTva
    (TVA-ul se postează pe documentul sursă — NIR-ul duce netul; evaluarea
    stocului la privat = net, lotul primește preț net).
    (c) **`ContaSeeder` spart în nucleu + pachete de profil**: nucleul neutru
    ține enum `ProfilContabil`, ancorele TipDocument, perioadele, repartitorii
    minimali și MECANISMELE (ContDinSimbol/tăierea segmentelor,
    SeedContImplicitTipMaterial, SeedContare6xxDin3xx cu parametru de
    excepții, gardianul `VerificaProfil` — ancora planului: 891.01.00 bugetar
    vs 4426 privat, profilul nu se amestecă pe aceeași bază); `ProfilBugetar`
    = conținutul de azi mutat + TipTva Capitalizat (21/19-istoric/11/0, fără
    conturi de TVA, fără PoliticaTva — zero schimbare de comportament);
    `ProfilPrivat` = plan OMFP 1802 (CSV `plan-conturi-omfp.csv`, format
    Account,ParentAccount,Denumire; Sumator = are copii; DimensiuniObligatorii
    pornesc GOALE), Clasă/Tip pe simboluri OMFP (301/302/303/345/371/381 stoc,
    6xx servicii, 704/706/707/708 venituri, TRZ/T tehnice), derivările proprii
    (excepțiile 371→607, 345→711, 381→608; plus inventar = **7588**, nu 791),
    conturi proprii CASA→5311 / BANCA→5121, fallback-uri 401/404/4111/542,
    PoliticaValidare doar FCL⊘Stoc (fără clasificație bugetară). `Updater`
    citește `ProfilContabil` din appsettings (default Bugetar, per bază — 35d).
    (d) **Seed TipTva privat cu codurile SAF-T reale** (extrase din
    RO_SAFT_SchemaDefCod 16.02.2026, direcționale livrare/achiziție):
    N21 310344/301104, N11 310351/301105, N9 310310/301102 (tranzitoriu, expiră
    31.07.2026 — marcat în denumire), TI21 310312/300906, NED21=Capitalizat
    −/351104, SDD 310314, SFD 310326, NIM 310324; 4428 legat ca REZERVAT pe
    rândurile Normal/TI. `CategorieD394` rămâne null — categoria e direcțională
    la nivel de operațiune, se fixează la proiecția D394 (checklist 35c).
    (e) **ModelCheck parametrizat pe profil**: implicit = suita bugetară pe
    baza aplicației (verde integral; singura schimbare în scenarii: culegerea
    CotaTva → TipTva, rândul istoric CAP19 păstrează valorile-ancoră);
    `dotnet run privat` = bază DEDICATĂ `Atlas.Conta.ModelCheck.Privat` pe
    care unealta o migrează și o seed-uiește singură (ContaSeeder direct,
    aceeași cale ca updater-ul) + blocul e2e privat: FCT stoc+serviciu (NIR
    net, rând 4426 și pentru linia de stoc FĂRĂ regulă principală, 401 brut,
    imperecherea plății automate pe brut), FCL cu 4427, DEC cu 4426=542 pe
    titular, taxare inversă, capitalizat, ValoareTva manuală păstrată, storno
    cu rândurile TVA inverse. Idempotent la rulări repetate.
    (f) Amânate, documentate (design §8): TVA la încasare (regim + 4428 +
    transfer la imperechere), facturi nesosite (408/4428), regularizarea de
    rotunjire per document×cotă (e-Factura), prorata/ajustări/D300/D394/SAF-T
    ca proiecții peste registre, închiderea lunară de TVA (4423/4424).

37. **Design P2 — descărcarea de gestiune la FacturaIesire — FIXAT**
    (`docs/privat/p2-descarcare-design.md`, toate cele 6 tranșări confirmate
    23.07.2026; implementarea urmează). Fluxul-ancoră acoperit (nu copiat —
    decizia 21): magazin online — FCL dictată de site (preț decuplat de cost,
    marjă posibil negativă), poziții fără stoc la facturare (venit acum, cost
    la disponibilitate), identificare cu prioritate pe lot. Tranșările:
    (a) **Tip nou `DescarcareGestiune` (DSC, al 11-lea derivat)**; detaliu
    derivat cu `LinieSursaId?` (FK real spre DocumentDetaliu — acoperirea per
    linie FCL). Laturi gestiune→client, AMBELE dimensiuni pe gestiune
    (`RepartitorImplicitCredit()`→Predator, ca Decont 32c). Valoare = cost
    (preț lot × cantitate, pattern BTR/BCS); fără TVA pe DSC (4427 rămâne
    integral pe FCL). Stoc −1 predator (Magazie/MF→Marfuri); contare
    `SeedContare6xxDin3xx` cu excepțiile profilului (607=371, 711=345).
    Reutilizarea BonConsum respinsă (+Consum pe primitor ≠ ieșire din
    patrimoniu). Ancora TipDocument DSC în nucleu; la bugetar fără politici —
    tip inert.
    (b) **Spargerea pe loturi la GENERARE** (draftul concret e condiția
    override-ului manual — decizia 13; operarea nu creează linii nicăieri în
    motor); `AlocaFifo` variantă TOLERANTĂ (alocă disponibilul, întoarce
    restul); gardianul de sold rămâne autoritatea la operare (alocarea
    învechită refuză zgomotos). TipStoc-ul căutării se citește din RegulaStoc
    al DSC-ului (fără hardcode).
    (c) **Generator = `DescarcareService` în motor, NU clona PoliticaConex**
    (valorile diferă, liniile se sparg 1→N, predatorul se înlocuiește):
    declanșat prin `GenereazaSecundar` pe FCL (precedent 31e) + acțiune
    „Generează descărcarea" pe FCL Operat pentru REST (backorder, după
    recepția FCT→NIR; data se culege, default azi). Fără tabelă
    PoliticaDescarcare (nimic de configurat; aditivă la nevoie). Restul
    neacoperit NU intră pe DSC — se raportează și rămâne interogabil per
    linie (cusătura fluxului de comenzi, pasul 5). Acoperirea = Σ linii DSC
    Draft+Operat pe LinieSursaId (draftul contează — anti-dublare; Stornat
    nu). Gardienii de grup existenți acoperă tot (draft șters la anulare,
    operat blochează).
    (d) **Culegerea FCL — General! + Specific?**: `ProdusId?` pe derivată,
    OBLIGATORIU prin validare pe liniile de stoc (identitatea liniei =
    produsul; schema rămâne nullable — aceeași derivată poartă serviciile);
    `LotId` de bază = rafinarea specifică OPȚIONALĂ, validată ca aparținând
    produsului, prioritară la picking și FĂRĂ fallback FIFO pe restul ei
    (pinul e intenția magazinului; deblocarea = scoaterea pinului).
    `GestiuneDescarcareId?` pe header — o gestiune per factură la P2; lot
    explicit fără sold în ea = refuz (întâi BTR).
    (e) **Derivarea de VÂNZARE pe FCL** (regula generică ar posta 4111=371):
    rând RegulaContare per TipMaterial de stoc — debit RepartitorPrimitor
    (fallback 4111), credit Explicit din mapă 371→707, 345→701, 381→708,
    fallback 708; incremental la updater, editabil ca date. Seed-ul privat
    ȘTERGE rândul NaturaInterzisa=Stoc de pe FCL (pas explicit — există în
    bazele P1); la bugetar RĂMÂNE (30a) — diferența de profil e în date,
    hook-ul no-op natural.
    (f) **Datoriile P1 intră în felie**: `TipTvaImplicitId?` pe TipDocument
    (default de CULEGERE, nu de motor — un rând PoliticaTva doar-pentru-
    default ar activa pasul TVA; seed N21 privat / CAP21 bugetar pe
    FCT/FCL/DEC) + verificarea culegerii TipTva/ValoareTva și a noilor câmpuri
    în UI XAF (smoke test manual în contractul feliei).
    (g) Amânate, documentate (design §10): comenzile (sales order/PO) și
    importul FCL din site (pasul 5), regenerarea automată la recepția NIR,
    multi-gestiune per factură, amănuntul la preț de vânzare (371/378/4428 —
    non-goal: evaluarea rămâne identificare specifică la cost net), rezervarea
    de stoc (gardianul de sold ajunge single-operator).

38. **Felia P2 — DescarcareGestiune — executată; validată e2e în ModelCheck
    (privat 69 verificări, idempotent la rulări repetate; bugetar 180) +
    review advers dedicat cu fix-urile aplicate. Tranșările de implementare
    peste designul fixat (37):**
    (a) **`AlocaFifoTolerant` = nucleul pickingului** (alocă disponibilul,
    întoarce restul); `AlocaFifo` devine wrapper care aruncă la rest —
    comportament identic pentru apelanții existenți. `Genereaza` și
    `RestNedescarcat` sunt PUBLICE pe `DescarcareService` — puncte de intrare
    ale motorului (controller azi, Web API la pasul 5); `RestNedescarcat` e
    cusătura interogabilă a fluxului de comenzi (design §2.2).
    (b) **Contenția intra-draft — rafinare de design**: două linii FCL pot
    concura pe același lot într-o generare, iar `Sold()` nu vede alocările
    necomise ⇒ generatorul procesează ÎNTÂI liniile pin-uite (identificarea
    specifică bate FIFO), apoi produs+FIFO, cu mapă `dejaAlocat` per lot
    scăzută din solduri. Generatorul NU aruncă niciodată la lipsă de stoc
    (restul = backorder); refuzul „lot explicit fără sold în gestiune — întâi
    BTR" trăiește în `FacturaIesire.ValideazaOperare`, înainte de
    materializare (33d — hook-ul `GenereazaSecundar` rulează după ea).
    (c) **Review-ul advers a întărit integritatea (validări, nu design):**
    linie de stoc FCL / linie DSC fără regulă de contare per-Tip = refuz
    explicit (30b — un Tip creat între updater-e nu mai postează
    greșit/silențios prin genericul FCL, nici nu mai mișcă stoc fără notă pe
    DSC); coerența Tip-linie ↔ Produs/Lot validată pe ambele tipuri;
    `LinieSursa` validată ca linie a facturii-sursă a documentului, iar
    acoperirea din `RestNedescarcat` filtrată pe `DocumentSursa == fcl` (un
    DSC străin nu poate otrăvi backorder-ul altei facturi); liniile FCL/DSC
    trebuie să fie de tipul derivat (linia de bază ar ocoli General!+
    Specific?); gardian NOU generic de editare (`DocumentEditareController`):
    documentele ne-Draft sunt read-only la nivel de DetailView — câmpurile
    FCL sunt load-bearing și după operare.
    (d) **UI**: acțiunea „Generează descărcarea" = ParametrizedAction cu dată
    (default azi), doar pe FCL Operat, deleagă la serviciu în ObjectSpace
    propriu și deschide draftul în editare; restul nedescărcat se raportează
    în mesaj pe AMBELE căi (operare + acțiune). `DefaultTipTvaController`
    generic pe `NewObjectViewController.ObjectCreated` (masterul din
    `NestedFrame.ViewItem` — sub EF Core back-reference-ul liniei NU e
    inițializat pre-commit, docs DevExpress 402990/112912); seed
    `TipTvaImplicit` N21/CAP21 setat doar unde e null.
    (e) **Smoke UI XAF (37f) — parțial, cu constatări**: aplicația pornește
    curat cu modelul P2; DSC apare ca al 11-lea tip creabil; câmpul
    GestiuneDescarcare + acțiunea (data precompletată azi, disabled pe Draft)
    și coloanele TipTva/ValoareTva vizibile pe FCL. Constatări PRE-EXISTENTE,
    rămase la faza polish XAF: butonul New al colecției Detalii creează tipul
    de BAZĂ (culegerea Descriere/PretUnitar/ProdusId și smoke-ul end-to-end
    al default-ului TVA blocate până la ListView-uri tipizate per derivată;
    validarea (c) refuză între timp liniile generice la operare); commit-ul
    master-ului la New pe linie cu laturile necompletate → FK violation brut
    (lipsesc validările de culegere pe laturi). Tot la polish, din review:
    traducerea FK Restrict (ștergerea unei linii referite de un DSC) în mesaj
    prietenos; mecanismul generic „tipul de detaliu declarat per document".

39. **Pre-polish: referințe `Atlas.DXF.Core` + `Atlas.DXF.Blazor` (26.1.3.5)
    și traducerea violărilor de constraint DB promovată în bibliotecă.**
    (a) **Mecanism nou în Atlas.DXF.EfCore** (`Database/Exceptions/`, scris în
    sesiunea asta, review advers cu probe live pe Postgres):
    `ConstraintViolationTranslator` (DbUpdateException → FK/unique/not-null/
    check; parse per provider cu SQLSTATE **23001 RESTRICT inclus** — cazul FK
    Restrict al DSC-ului; direcția FK din evidența providerului, NU din
    `Entries` — EF pune TOT batch-ul acolo, batch-urile mixte delete+insert ar
    minți) + rezolvare constraint→tipuri CLR pe IModel relational +
    `ConstraintViolationMessages` (template-uri publice suprascriibile,
    captions XAF cu fallback CLR). Hook Blazor: `AtlasDxfExceptionService`,
    înregistrat de `AddAtlasDxfServices()`/`AddAtlasDxfExceptionHandling()`
    (XAF folosește TryAddScoped — verificat decompilat; ordinea față de
    AddXaf nu rupe).
    (b) **Conta consumă**: Module → +`Atlas.DXF.Core` (venea deja tranzitiv
    prin EfCore, acum explicit); Blazor.Server → +`Atlas.DXF.Blazor`,
    `.AddAtlasDxf()` în lanțul de module, `AddAtlasDxfServices()` +
    template-urile în ROMÂNĂ în `Startup.ConfigureServices`. Închide din 38e
    itemul „traducerea FK Restrict în mesaj prietenos". ModelCheck verde pe
    ambele profiluri; aplicația pornește curat cu modulul.
    (c) **Unelte noi disponibile pentru restul polish-ului 38e**: EntityFluent
    (layout-uri ListView/DetailView declarative în C#; FĂRĂ auto-discovery —
    `UiBaselineDiscovery.Discover(assembly)` manual în `AddGeneratorUpdaters`),
    model extenders (`DisplayDetailType`, `NewObjectTarget`/
    `ProcessObjectTarget` — candidat pentru „tipul de detaliu declarat per
    document"), `CustomizeListViewController`/`GridAutoResizeController`/
    `ViewSummaryController`, `SmartLookupPropertyEditor`, atributele
    `ForbidCRUD`/`DeactivateAction`, `NavigationPathAttribute`.

40. **Felia polish XAF — executată; smoke UI real (browser) + review advers cu
    fix-urile aplicate. Backlog-ul 38e închis.** Tranșările:
    (a) **Mecanismul „tipul de detaliu declarat per document"**:
    `[TipDetaliu(typeof(...))]` pe cele 5 derivate cu detaliu propriu
    (FCT/FCL/LDI/DEC/DSC) + `TipDetaliuViewUpdater`
    (ModelNodesGeneratorUpdater<ModelDetailViewItemsNodesGenerator>) comută
    `IModelMemberViewItem.View` al colecției `Detalii` pe ListView-ul
    derivatei — New creează derivata, coloanele arată schema ei. Extenderii
    Atlas.DXF (`NewObjectTarget` etc.) NU rezolvau asta (controlează doar
    popup-vs-tab) — mecanism propriu în `Module/UI/`. Capcană fixată: la
    generare `View` NU e null (XAF leagă implicit view-ul bazei
    `Document_Detalii_ListView`) — se suprascrie DOAR default-ul tipat pe
    bază; alegerile explicite din Model Editor rămân. Baseline EntityFluent
    (`ContaUiBaseline`, discovery manual în `AddGeneratorUpdaters`): ordinea
    coloanelor per tip, FK-urile brute ascunse (HideMembers = toate view-urile
    tipului, inclusiv export), TipTva/ValoareTva ascunse pe LDI/DSC. Smoke UI:
    New tipizat + dialogul liniei cu câmpurile derivatei + `TipTvaImplicit`
    precompletat (CAP21) — inclusiv smoke-ul TVA din 37f/38e.
    (b) **Validare de culegere**: `RuleRequiredField` (context Save) pe
    NAVIGAȚIILE `Document.Predator/Primitor` și `DocumentDetaliu.TipMaterial`
    (FK-urile Guid nu pot purta regula — Guid.Empty nu e null), mesaje RO;
    New pe linie cu laturile goale → refuz prietenos în locul FK violation
    (calea `CommitMasterObject`, exact bug-ul 38e). Review advers: motorul
    rulează în ObjectSpace-ul View-ului ⇒ commit-ul operării trece prin
    aceleași reguli, iar validarea (în Committing) pica DUPĂ materializare →
    „operare-fantomă" în OS-ul viu, comisă de un Save ulterior fără motor.
    Fix: `DocumentOperareController.Executa` comite culegerea (cu validare)
    ÎNAINTE de motor; + refuz în `ValideazaOperare` de bază pe linii cu
    `TipMaterialId` gol (înaintea materializării — 33d). NOTĂ pas 5: obiectele
    motorului trec validarea prin fixup/lazy-load pe entități Added
    (nedocumentat ca garanție EF) — de re-verificat la tierul Web API.
    (c) **Read-only post-Draft pe toate căile liniilor**:
    `DocumentDetaliiEditareController` (ListView nested: AllowEdit/New/Delete
    când masterul nu e Draft) + geamănul pe DetailView-ul LINIEI (bypass prin
    row-click, găsit de review); re-evaluare pe `ObjectSpace.Committed`.
    Limitare asumată (ca la gardianul de header): stale multi-tab — Committed
    nu se propagă între ObjectSpace-uri; fix-ul de fond ar fi gardian în
    Committing pe server, la nevoie.
    (d) **Registre + lookup-uri**: `[ForbidCRUD("ListView","DetailView")]` pe
    RegistruStoc/Contabil (ascunde acțiunile) + `RegistruReadOnlyController`
    de FOND (AllowEdit/New/Delete=false pe orice view — appearance-ul nu se
    evaluează pe lista goală și nu oprea Save-ul prin dialogul „modificări
    nesalvate"); verificat în UI. `SmartLookupPropertyEditor`
    (`AtlasEditorAliases`, opt-in per proprietate) pe 13 navigații mari:
    conturile (plan 1.679) de pe TipMaterial/Repartitor/TipTva/RegulaContare/
    DecontDetaliu, `DocumentDetaliu.TipMaterial`, `FacturaIesireDetaliu.Produs`,
    laturile `Predator/Primitor`; match exact pe DefaultProperty → auto-select,
    altfel popup pre-filtrat. `Lot` sărit — nu are DefaultProperty (de decis).
    La orice problemă de componentă: revert per proprietate la lookup-ul
    standard (scoți `[EditorAlias]`), repararea se face în Atlas.DXF.
    (e) **Semnalate, netranșate**: `Imperechere` creabilă din UI prin New
    generic — ocolește invarianții `ImperechereService` (31d); de tranșat
    (probabil ForbidCRUD + acțiune dedicată, cel târziu la pasul 5). Drafturile
    vechi cu linii de tip BAZĂ se afișează acum prin ListView-ul tipizat
    (cosmetic; operarea oricum le refuză — 38c). FK-urile brute rămân vizibile
    pe tipurile ne-anotate (NIR/BTR/BCS/PLT/INC). `Dimensiuni` se afișează ca
    „Castle.Proxies.DimensiuniProxy" pe registre (ToString de adăugat).

41. **Felia „restanțele 40e → Atlas.DXF 26.1.3.6" — executată; genericul
    promovat în bibliotecă, consumul în Conta; smoke UI browser + review advers
    cu fix-urile aplicate. Restanțele 40e închise. Tranșările:**
    (a) **Enforcement de fond `ForbidCRUD` în bibliotecă**:
    `ForbidCrudCapabilitiesController` (Atlas.DXF.Core, înregistrat cu
    `[IncludeController]` pe modulul Blazor — pattern-ul cross-assembly
    existent) taie `AllowEdit/New/Delete` pe view-urile din Context-ul
    atributului; potrivirea oglindește algoritmul DevExpress AppearanceContext
    (tokenizare `,`/`;`, „Any" INVERSEAZĂ — fidel sursei, inert pentru uzul
    ForbidCRUD). `RegistruReadOnlyController` din Conta a murit. Limitare
    documentată: consumator non-Blazor al Core primește doar hiding-ul.
    (b) **`HideForeignKeys()` pe EntityFluent + HierarchyFluent** (convenția:
    scalar `{Nav}Id` Guid/int/long cu navigație pereche; orfanii rămân —
    `Lot.LinieIntrareId` intenționat vizibil). În Conta
    (`ContaUiBaseline.AscundeFkuriBrute`): ierarhiile Document/DocumentDetaliu
    + FK-urile proprii per tip (inclusiv header-ele descoperite
    `FacturaIntrare.PlataContPropriuId`, `FacturaIesire.GestiuneDescarcareId`).
    Nuanțe: descoperirea Hierarchy vede DOAR membrii bazei (derivatele cer
    For<T>); expansiunea owned generează scalari cu path nested
    (`DimensiuniDebit.RepartitorId`) — ascunși explicit, HideForeignKeys nu-i
    vede.
    (c) **Afișarea owned-urilor**: `OwnedObjectBase.ToString()` în bibliotecă
    (numele tipului real, de-proxificat — walk pe BaseType cât
    `Assembly.IsDynamic`); `Dimensiuni.ToString()` în Conta = enumerarea
    compactă a dimensiunilor setate (prefixe R/M/CF/CE/F/U/P/CC pe `Cod`),
    navigația citită DOAR la FK non-null. `[ExpandObjectMembers(InDetailView)]`
    pe `RegistruContabil.DimensiuniDebit/Credit` FUNCȚIONEAZĂ pe owned
    (verificat în browser — membrii apar inline, read-only; motivul pentru
    care nu se auto-expanda: sub EF Core `IsAggregated` = doar atributul
    `[Aggregated]`, nu ownership-ul). Review advers → **AutoInclude pe
    navigațiile interne ale owned-urilor registrului**
    (`ConfigureDimensiuniEager`): PreFetch-ul XAF nu coboară în owned ⇒ lazy
    per instanță (N+1 pe pagină) + lazy-load pe OS disposed la render târziu;
    DOAR registrul — liniile de document și regulile de contare rămân lazy
    (hot-path-ul motorului nu afișează etichete).
    (d) **Imperechere — 40e tranșat ALTFEL decât schița**: nu ForbidCRUD +
    acțiune dedicată (popup non-persistent = mașinărie grea), ci New prin UI
    PERMIS dar validat la commit: `ImperechereService.ValideazaCreare` (extras
    din `Creeaza`, aceleași verificări + null-guard pe navigații) apelat de
    `ImperechereController` în `ObjectSpace.Committing`; Edit blocat
    (AllowEdit=false + refuz pe modificate la commit — re-validarea sumei ar
    cere excluderea propriului rând); Delete liber (31d); `Autogenerat`
    read-only. Review advers → invariant NOU în serviciu: refuz
    Plata↔Plata / Incasare↔Incasare (sensul opus e obligatoriu;
    Plata↔Incasare rămâne — avans↔regularizare); re-evaluarea capabilităților
    și pe `Committed` (după Save view-ul rămânea editabil). Limitare asumată,
    comentată: două link-uri noi în același commit nu se văd reciproc la
    Σ≤rest (operator unic, ca 25f).
    (e) Proces: pachetele ies prin `build\pack-and-push.ps1` (bump automat din
    tag-uri git; push-ul îl rulează utilizatorul — clasificatorul de permisiuni
    blochează push-ul extern); Conta referă flotant `26.1.3.*` ⇒ consumul =
    restore. Testul ToString din bibliotecă simulează proxy-ul cu
    Reflection.Emit (Proxies tranzitiv e 8.x vs EF 10 — pariu de versiune
    refuzat); calea reală EF e acoperită de ModelCheck + smoke UI în Conta.

42. **Design pasul 5 — tierul API + React — FIXAT**
    (`docs/api/p5-api-design.md`, toate cele 6 tranșări confirmate
    24.07.2026; implementarea urmează). Rafinează deciziile 5–8 pe modelul
    stabilizat. Esența: (a) **o singură sursă de reguli** — gardian generic
    de Committing în Module, activ DOAR pe ObjectSpace-uri secured (non-Draft
    read-only, registre read-only pentru toți, Imperechere prin
    ValideazaCreare); distincția secured/non-secured înlocuiește orice token
    (non-secured = ușa de sistem); în securitate nimeni nu are Write pe
    registre. Închide restanțele 40c/41a/41d. (b) **Motorul rulează în
    ObjectSpace non-secured PROPRIU, secvență nu cuib**: faza 1 = culegerea
    comisă prin secured OS; faza 2 = comanda prin ID, tranzacția integral a
    motorului; puntea = ID în ambele sensuri, retur/erori ca date
    (`{documentId, stareNoua, conexId?, mesaje[]}` / 422+erori[]);
    DocumentOperareController migrează pe același pattern. (c) **Citirea =
    registre + proiecții**: `Module/Proiectii` cu `IQueryable<RandDto>` +
    DataSourceLoader (DevExtreme.AspNet.Data); atomii partajați Brut +
    unpivot-ul Imperecherii (LINQ simplu, fără LINQKit; join pe agregate, nu
    subquery corelat); test de consistență proiecție==StocService/
    ImperechereService în ModelCheck; Dimensiuni aplatizate în Select (owned
    nu traversează sârma — închide 24 pe citire). TS nu calculează niciodată
    sold/rest/total. (d) **Scrierea = agregat per document** (PUT header+
    linii, reconciliere server-side, WriteDto≠ReadDto vizibil în tipuri),
    organizată în **felii verticale per tip** (Dtos+Apply+Endpoints, în
    Module/assembly Api testabil în ModelCheck); importul (site/Tethys) =
    alt apelant al aceluiași apply, un document per tranzacție. (e)
    **Metadata pe criteriul deciziei 4**: build-time = OpenAPI→TS + captions
    prin CaptionHelper (fluxul de localizare XAF); runtime = doar
    politici-date; pe resursă = affordances în ReadDto; serializarea
    layout-ului MOARE (layout per tip = componentă React). (f) **Host
    separat** `Atlas.Conta.WebApi` din scaffold-ul DevExpress (fără
    multitenancy — MT rămâne opțiune aditivă), același Module, OData opt-in
    DOAR nomenclatoare, JWT + user store comun, updater unul singur, release
    ca pereche per client. Datorie parcată împreună: concurență
    multi-operator + acoperirea rest per linie (seam: advisory lock 25f).
    De verificat la primul spike: atribuirea auditului sub OS non-secured,
    formatul UserFriendlyExceptionFilter.

43. **Design pasul 5 — clientul React — FIXAT**
    (`docs/api/p5-react-design.md`, toate cele 6 tranșări confirmate
    24.07.2026; închide mandatul §8 din designul API; implementarea urmează).
    Regula transversală: vocabular de componente compuse în cod, NICIODATĂ
    limbaj de descriptori interpretat (cliff-ul 10–30% al XAF e structural la
    orice interpretor propriu). Esența: (a) **felii verticale + vocabular de
    editoare** — `DocumentShell` + `CampShell`; metadata leagă *atributele*
    câmpului (caption/required/maxLength prin `useCampMeta` din codegen),
    codul decide *identitatea* editorului (tag explicit în JSX) și
    prezența/ordinea/condiționalitatea; respins `fields=[]` și `<Camp>`
    auto-resolve (registry = vocabular înghețat); escape hatch = alt tag în
    același JSX. (b) **validare pe două straturi, zero motor de reguli în
    TS**: instant/structural din schema OpenAPI + autoritar = motorul, expus
    și ca dry-run `POST .../valideaza` (calculează+validează fără
    materializare — 33d); respins gemenii TS ai PoliticaValidare („o singură
    sursă de reguli"). (c) **state pe trei feluri, fără store global**:
    server-read (TanStack Query + CustomStore pe DataSourceLoader), formular
    per felie (agregatul WriteDto local, PUT ca întreg), efemeride UI; URL =
    starea globală; linii de draft = editor de linie propriu cu vocabularul
    `Camp*`, grid readonly — fără CRUD per linie prin componente.
    (d) **codegen: tipuri, nu clienți** — dump OpenAPI + dump
    captions/enums/DefaultProperty prin ModelCheck (devine verificator +
    emitor), artefacte comise în `Client/src/generated/` cu drift verificat
    (disciplina migrațiilor); respins orval/NSwag client; `api.ts` de mână
    per felie. (e) **topologie same-host**: `nou/Atlas.Conta.Client`
    (Vite+React+TS) servit static de WebApi (SPA fallback), dev prin Vite
    proxy; CORS moare în producție; release rămâne pereche per client.
    (f) **lookup-uri pe OData nomenclatoare** (consumatorul ușii opt-in 42f):
    `Lookup` = SelectBox+ODataStore, mod local/remote ca prop explicit,
    display din DefaultProperty emis în dump. Amânate: biblioteca de formular
    (la primul spike), `Lot.DefaultProperty` (restanța 40d, a modelului).

```
/legacy   → surse Delphi (.pas, .dfm) + scripturi SQL vechi
/db       → se poate export schemă (CREATE) + CONȚINUTUL tabelelor de configurare
            (definiții câmpuri, reguli contabile, definiții stoc, plan Clasă/Tip) din (local)/contabilitate_2026 sql server trusted connection
/nou      → soluția nouă (XAF + React)
```

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

## Reguli de lucru pentru Claude Code

- NU genera modelul nou înainte de finalizarea și validarea inventarului (pasul 1).
- La explorarea surselor Delphi: navighează selectiv (grep pe numele tabelelor
  de configurare și ale câmpurilor), nu citi formuri la rând.
- Orice caz special descoperit în legacy (semantici suprapuse, reguli ascunse
  în cod) se raportează înainte de a decide unde ajunge în modelul nou.
- Deciziile noi importante se adaugă în acest fișier, în secțiunea de decizii.

## Cunoștințe utilizator (context)

Dezvoltator .NET cu experiență de producție în DevExpress XAF (Blazor Server),
Serenity, React, Flutter, EF, OData. Fluent cu pattern-ul de codegen
C#→TypeScript din Serenity (echivalentul mental al OpenAPI→TS).
Motivația migrării de pe XAF Blazor pe frontend React: limitări structurale
ale ObjectSpace-ului sincron (fără async nativ, dialoguri, extensibilitate
greoaie) — nefixabile la nivel de librărie.
Dezvoltatorul initial al aplicatiei legacy
