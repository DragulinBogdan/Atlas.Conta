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

## Structura workspace

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
4. **Migrarea datelor** → green-field la graniță de ciclu (decizia 12):
   nomenclatoare, politici, solduri de deschidere. Reconciliere: soldurile
   de deschidere în sistemul nou = soldurile de închidere din legacy.
5. **API + React** → abia după validarea pașilor 1–4.

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
  → FacturaIesire (EXECUTAT, decizia 30) → Plata/Incasare + Imperechere →
  Decont. Per tip: politici seed + validare declarativă + test de operare.
- **3d. Validare transversală**: dimensiuni obligatorii per cont, obligativități
  per tip, invariantele imperecherii.

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
