# CLAUDE.md — Atlas.Conta: contabilitate/gestiune (Delphi + SQL → XAF + React)

> **Constituția: `docs/invarianti.md`** — cei 6 invarianți (2026-08-02), fiecare
> cu clauzele lui de interdicție; orice propunere arhitecturală se testează
> întâi contra lor.
> **Jurnalul integral: `docs/decizii/NNN-*.md`** (un fișier per decizie, index
> în `docs/decizii/README.md`) — textul complet, cu sub-punctele (a)–(k) la
> care trimit codul și docs-urile („decizia 42c" = `docs/decizii/042-*.md`,
> sub-punctul (c)); antetul fiecărui fișier spune dacă decizia e activă,
> amendată sau depășită. Fișierul de față ține, sub ACEEAȘI numerotare și
> aceleași litere, doar regula durabilă — „ce e adevărat acum", nu „cum s-a
> ajuns"; literele marcate „→ jurnal" sunt restanțe/amânări al căror text e
> doar acolo.

## Context general

Rescriem o aplicație de contabilitate/gestiune scrisă în Delphi + SQL Server.
Aplicația veche folosește un model generic de documente configurat prin tabele
(EAV-like); generalizarea nu s-a plătit: ușor de implementat inițial, foarte
greu de întreținut, cu un număr mic și stabil de tipuri de documente.
Direcția de produs e **privat-first** (decizia 35): produsul privat (OMFP 1802)
e sursa de cerințe; profilul bugetar rămâne pachet de seed funcțional.

## Arhitectura veche (legacy, în /legacy și /db)

- Tabelă unică de documente (header) + tabelă unică de detalii, cu `TipDocument`;
  document generic **predator → primitor**, cu tabele de configurare per
  combinație tip × predator × primitor (ce câmpuri se culeg/validează).
- **Clasă/Tip** pe nomenclatorul de produse ≈ planul de conturi operațional
  (Clasa ≈ prima cifră; Tip = detaliere nivel 2–3); maparea pe conturi e
  declarativă, ulterioară.
- **Stocuri** = listă de semn aplicat pe cantitate (+/−) per tip document ×
  tip primitor/predator × filtru.
- Statut: **evidență, niciodată canonic** (decizia 21); istoricul de documente
  rămâne în legacy (decizia 18).

```
/legacy   → surse Delphi (.pas, .dfm) + scripturi SQL vechi
/db       → export schemă + conținutul tabelelor de configurare (SQL Server local,
            Contabilitate_2026); inventarul legacy în db/inventar/
/nou      → soluția nouă: BackOffice (XAF Blazor + Module), WebApi, Client (React),
            tools/ (ModelCheck, Migrare, Import1C, BackfillTva)
/docs     → invarianți, jurnal, design-uri și contracte per felie
```

## Harta surselor de adevăr

| Întrebarea | Unde |
|---|---|
| Ce trebuie să rămână adevărat | `docs/invarianti.md` |
| De ce e așa (text integral, sub-puncte) | `docs/decizii/NNN-*.md` (index: `docs/decizii/README.md`) |
| Istoricul de execuție al planului (feliile, în ordine) | `docs/decizii/istoric-plan-de-lucru.md` |
| Testul bazei (câmpurile `Document`/`DocumentDetaliu`) | `db/inventar/11-testul-bazei.md` |
| TVA structural / descărcarea de gestiune (privat) | `docs/privat/p1-tva-design.md`, `p2-descarcare-design.md` |
| Conectorul 1C și contractul de reconciliere | `docs/import/faza-1c-design.md` |
| Gate-ul XAF; dimensiunile pe frunze | `docs/gate-xaf-contract.md`; `docs/dim/dim-2-inventar.md` |
| Tierul API / clientul React (design) | `docs/api/p5-api-design.md`, `p5-react-design.md` |
| Contractele feliilor pasului 5 (D-urile pin-uite) | `docs/api/p5-*-contract.md`, `p5-felia-jurnale-tva-design.md` |
| Perf pe baza de import | `docs/api/p5-perf-masuratori.md` |
| Ce rămâne de la XAF Blazor pentru React | `docs/api/lista-react.md` |
| Fluxul comenzilor online (bifurcație deschisă) | `docs/architecture-notes-2026-07-28.md` |

## Decizii — regulile durabile (NU redeschide fără motiv nou)

Numerotarea e stabilă; literele corespund sub-punctelor din jurnal (literele
lipsă = istoric fără regulă proprie). „Depășită de N" = regula trăiește acum în
decizia N.

### Modelul (1–22)

1. **Nucleu generic + moștenire, nu tabele separate.** `Document` +
   `DocumentDetaliu` de bază, derivate per tip; motoarele de stoc și contabile
   consumă DOAR baza.
2. **Testul de apartenență a unui câmp**: apare într-o formulă de stoc sau
   regulă contabilă → bază; apare doar pe ecranul unui tip → derivată.
   (Rafinat de 54c: motorul are nevoie de VALOARE, nu de coloană.)
3. **EF Core TPT** pentru header și pentru detaliile derivate (frunzele există
   unde schema diferă; declarația per document = `[TipDetaliu]`, 40a).
4. **Structura devine cod, politica rămâne date**: câmpurile per tip = clase +
   validare declarativă; maparea Clasă/Tip → conturi, definițiile de stoc,
   numerotarea, conexul, scadența, TVA-ul, validările per tip = tabele de
   politică editabile fără release. Politica nu inventează comportament
   (invariant IV).
5. **Backend XAF** (logică, securitate, validare, audit) expus prin Web API;
   **frontend React + DevExtreme** pentru ecranele operaționale.
6. **API-ul NU expune ierarhia polimorfic**: endpoint per tip de document,
   DTO-uri plate prin codegen OpenAPI→TS; moștenirea e detaliu de persistență.
7. **Nav properties în API**: FK-urile explicite, `JsonIgnore` pe navigații.
8. **Metadata-driven UI** doar pe criteriul deciziei 4: build-time = OpenAPI→TS
   + captions; runtime = doar politici-date; layout-ul per tip e cod React
   (rafinat de 42e — serializarea layout-ului a MURIT).
9. **Scope faza 1: contabilitate + gestiune**, plățile/încasările ca documente.
   Salarizare, imobilizări, execuție = module separate, intră prin note
   contabile (NTC, 46b).
10. **Plan de conturi doar sintetic**; analiticele se derivă din dimensiuni la
    raportare, nu se persistă.
11. **Sursa de finanțare = dimensiune explicită** (mecanismul COMPUSA din
    legacy se elimină; denumirile dimensiunilor normalizate).
12. **Migrare green-field la graniță de ciclu**: fără paritate
    document-cu-document; se preiau nomenclatoare, politici (seed),
    solduri/stocuri de deschidere.
13. **Evaluare stoc: identificare specifică pe lot.** `Produs` (catalog) + `Lot`
    (creat de linia de intrare, preț unitar fix, gestiune, dată); ieșirile
    referă lotul; picking auto-FIFO în produs × gestiune, cu override manual.
    O singură metodă de evaluare în motor (CMP parcat ca `PoliticaEvaluare`,
    51d/e).
14. **Registre persistate, append-only.** `Draft → Operat → (Stornat)`; la
    operare motorul scrie tranzacțional stoc + note. Corecție directă/anulare
    DOAR fără dependenți în perioadă deschisă (dependență exactă pe loturi +
    sold intermediar ≥ 0); altfel storno. Perioada închisă = graniță absolută.
    Schema registrelor = COD; regulile de alimentare = DATE.
15. **Dimensiunile** = setul R/M/CF/CE/F/U/P/CC (FK nullable fiecare); regula
    de notă poartă `Comun`/`OverrideDebit`/`OverrideCredit`, rezolvarea
    (coalesce) e generică în motor; linia de document poartă un set parțial,
    rândul de registru unul complet, PER LATURĂ (25a). `DimensiuniObligatorii`
    per cont = validare (33a). Forma de stocare: PLATĂ, pe frunze și registru
    (54c — forma owned a murit). Override-urile pe linie din legacy
    (ContD/ContC, RepD/RepC) NU se preiau.
16. **Repartitori: TPT bază + derivate** doar unde schema diferă și identitatea
    e exclusivă (Partener, Gestiune, Angajat, UnitateInterna, ContPropriu);
    calitățile transversale (Gestionar, Comisie, LocConsum, CentruCost…) =
    flags `Calitati`, nu clase. Furnizor/client = rol dat de poziția pe
    document, o singură clasă `Partener`.
17. **Plăți/încasări = tipuri de document; Imperecherea** (stingerea) = link
    many-to-many plată↔factură cu sume parțiale, NU document (31d).
    **Document conex** = generare automată de document legat, cu link persistat
    (`DocumentSursaId` + `Autogenerat`), mecanism generic (26d).
18. **Migrarea preia nomenclatoare + solduri de deschidere; politicile se
    SEED-uiesc** (definite curat), nu se migrează.
19. **Tipurile de document = ierarhia de clase**: FacturaIntrare, FacturaIesire,
    NIR, BonConsum, NotaTransfer, ListaDiferenteInventar, Decont, Plata,
    Incasare (+ DescarcareGestiune 37; NotaContabila, InchidereTva, Asamblare,
    ReturFurnizor, ReturClient 46). **RaportProductie rămâne REZERVAT**
    (designul BPR se amână până la modulul de rețetar). Proforma exclusă; BF
    (bon fiscal/avans) = variantă de FacturaIntrare, nu tip separat.
20. **Un singur nomenclator de tipuri, și e codul**: nivelul intermediar
    tip × predator × primitor din legacy dispare — derivata își fixează
    direcția; `TipDocument` seed oglindește clasele 1:1 doar ca ancoră FK + UI
    pentru rândurile de politică.
21. **Seed-ul politicilor se face PE FUNCȚIONALITATE, nu prin transcrierea
    config-ului legacy** (nici a lui 1C — 35b). Sursele externe sunt
    evidență/direcție, niciodată canonic; fiecare gaură de profil = decizie
    explicită, nu transcriere. Amânare cu nume: defalcarea multi-sursă pe linie
    (cofinanțarea) se proiectează odată cu SursaFinantare (jurnal 29d/34d).
22. **Testul bazei — închis** (`db/inventar/11-testul-bazei.md`). Baza
    `Document`: Numar, Data, PredatorId, PrimitorId, Stare(+DataOperare),
    DocumentSursaId + Autogenerat, Total. Baza `DocumentDetaliu`: TipMaterialId
    (Clasa se derivă din Tip), LotId?, Cantitate SEMNATĂ, Valoare (valoarea de
    postare), AngajamentId? (+ TipTvaId?/ValoareTva din 36a). Tranșări:
    (a) doar `Valoare` pe bază, intermediarele (preț/cotă/net) per derivată;
    (b) granița dimensiunilor nu e în schema bazei ci în validare + metadata
    (azi: frunze, 54c); (c) AngajamentId FK nullable, modulul angajamente
    separat; (d) contul pe linia Factura* = politică (SursaCont), nu câmp;
    (e) DECONT_* = câmpuri pe FacturaIntrare; scadența/PV = interfețe
    (`IDocumentCuScadenta`, `IDocumentCuPV`); NR_NOTA moare (numărul notei
    aparține registrului).

### Persistență, motor, tipurile de document (23–34)

23. **Persistență + seed.** (a) Schema = migrații EF Core, CANONIC; update-ul de
    schemă XAF e dezactivat; seed-ul prin `--updateDatabase --forceUpdate
    --silent`. (b) `ClasaProdus.Natura` (Stoc/Serviciu/Cheltuiala/Imobilizare/
    Tehnica/Virament): doar Natura=Stoc intră în regulile de stoc. (c)
    NotaTransfer n-are RegulaContare — la plan sintetic transferul nu mișcă
    conturi. `nou/tools/ModelCheck` = suita e2e care rămâne VERDE pe ambele
    profiluri după orice schimbare.
24. **EF Core, nu XPO.** Restul (owned `Dimensiuni` sub XAF, `OwnedObjectBase`,
    `CreateProxy`) e DEPĂȘIT de 54c. NuGet: `nou/nuget.config` mapează
    `Atlas.DXF.*` → feed Atlas.
25. **Motorul de operare** (`Module/Motor/`). (b) Hooks polimorfe pe bază,
    consumate doar de motor: `PregatesteOperare(os)` (derivata materializează
    `Valoare`) și `ValideazaOperare(os, erori)`; lucrează pe FK-uri +
    IObjectSpace, NU pe navigații lazy. (c) Loturile nu se nasc în motor —
    linia le creează la culegere, motorul le FINALIZEAZĂ la operare. (d)
    Gardieni: perioadă lipsă = închisă; sold intermediar = prefix-sum per
    (Lot × Repartitor × TipStoc) ≥ 0 la orice dată; anulare doar fără
    dependenți; storno = rânduri inverse (flag `Storno`) la data stornării;
    copiii operați blochează sursa. (e) `Registru*.DocumentId` null = rând de
    deschidere, fără document. (f) Numărul din `PoliticaNumerotare`, asignat
    abia la MATERIALIZARE (53b). Coalesce-ul dimensiunilor, per latură: linie →
    override(latură) → comun → default polimorf al header-ului (32c) → Material
    din lot (33b). Limitare asumată: fără serializare între operatori
    concurenți (advisory lock per cheie de stoc = aditiv).
26. **NIR + FacturaIntrare + conex.** (a) Recepția CONTEAZĂ pe NIR; FCT
    postează DOAR liniile care nu trec pe NIR + TVA-ul; granița = Natura
    clasei; fără dublă postare. (b) Conturile se rezolvă declarativ: `SursaCont`
    per latură (Explicit/TipMaterial/RepartitorPredator/RepartitorPrimitor);
    `ContImplicit` pe TipMaterial (Cod-ul Tipului E simbol de cont, cu tăierea
    segmentelor terminale) și pe Repartitor = mapările ca DATE. (c) Prioritate:
    TipMaterial exact → `NaturaFiltru` → generică; fără regulă = linia nu
    contează; RegulaStoc pe Clasă bate genericul. (d) Conexul se generează ÎN
    motor, în tranzacția operării sursei (`PoliticaConex.NaturaFiltru`);
    copiii Operați blochează anularea sursei, drafturile autogenerate se șterg
    cu ea. (e) Lotul se naște la culegere pe linia FACTURII; `Lot.LinieIntrareId`
    e coloană FĂRĂ FK (intenționat — ciclu de inserție). (f) Validările
    proprii tipului trăiesc în `ValideazaOperare`; nicio navigație lazy în
    timpul enumerării.
27. **BonConsum.** (a) Două registre simultan: −Magazie pe predator, +Consum pe
    primitor (consumul rămâne pe responsabil). (b) Locul de consum = calitatea
    `LocConsum`; locația lotului = soldul din registru, nu o validare. (d)
    Valoare = preț lot × cantitate. Contarea 6xx = 3xx se DERIVĂ la seed din
    simbol (`SeedContare6xxDin3xx`, excepții per profil).
28. **ListaDiferenteInventar.** (a) `Directie` explicită, materializată în semn
    la operare (UI-ul culege pozitiv); un singur set de reguli de stoc, +1 pe
    predator. `RegulaContare.SemnFiltru` (±1): nepotrivirea scoate regula din
    joc, valoarea se normalizează cu semnul. (d) Plusul creează lot propriu
    (gestiunea = predatorul) și NU poate referi un lot străin; minusul descarcă
    un lot EXISTENT, nu unul născut de același document (63c). Primitorul =
    calitatea `Comisie`. (e) Direcție setată explicit, cantitate ≠ 0, lot per
    linie, plus cu preț de evaluare pozitiv.
29. **Profil contabil = pachet de seed** per bază; motorul e AGNOSTIC la plan
    (niciun simbol hardcodat). (b) Orice cerință de profil = politică, nu clasa
    de document. (c) Profilul privat diferă de CONȚINUT (derivările de seed per
    profil), mecanismele se transferă. Dimensiunile bugetare rămân în model,
    nullable.
30. **FacturaIesire.** (a) La bugetar: pur creanță, fără stoc
    (`PoliticaValidare`); la privat descarcă gestiune prin DSC (37). (b)
    Contare generică: debit `RepartitorPrimitor` (fallback 411), credit
    `TipMaterial` FĂRĂ fallback; venitul = alegerea Tipului (clasa VEN). (c)
    `PoliticaScadenta` se aplică DOAR dacă scadența nu e culeasă. Serie `FCL-`
    server-owned. (e) Seed-ul comite nomenclatoarele înainte de derivări.
31. **Plata / Incasare + Imperechere.** (a) Laturi tipizate (ContPropriu ↔
    Partener/Angajat), liniile = defalcarea sumei cu `Valoare` culeasă; Tipul
    tehnic `TRZ`. `ContImplicit` pe baza `Repartitor`. (c) Contare generică din
    laturi; cont propriu fără cont = eroare clară. (d) **Imperecherea NU e
    document** — link între două documente OPERATE; invarianții în
    `ImperechereService` (Σ ≤ totalul fiecărei părți, contrapartida
    stingătorului pe documentul stins, ambele roluri permise); anulare/storno
    refuzate cât există imperecheri; link-ul se șterge liber. (e) Plata automată
    = `Document.GenereazaSecundar` (din date CULESE), copil al grupului conex,
    cu imperechere automată la operarea ei. (f) Amânate → jurnal; 581 = 64.
32. **Decont.** (a) `ILinieCuPostareExplicita` bate rezolvarea declarativă și e
    nivelul maxim al coalesce-ului; DOAR tipurile care o declară. (b) Predator
    Angajat, primitor intern; fără stoc. (c) Default-ul de dimensiune e
    POLIMORF: `Document.RepartitorImplicitDebit/Credit(os)`. (d) Cantitatea
    pro-formă 0→1.
33. **Validarea transversală.** (a) `Cont.DimensiuniObligatorii` = gardian
    generic pe seturile REZOLVATE, per latură; puntea E: `AngajamentId`
    satisface CodEconomic până la modulul de angajamente. (b) Material din lot.
    (c) `PoliticaValidare` per tip = profilul de validare, aplicat înaintea
    hook-ului tipului. (d) **`Opereaza` = calculează → validează →
    materializează**: niciun rând de registru înainte de trecerea tuturor
    gardienilor.
34. **Migrarea legacy** (`nou/tools/Migrare`; azi prototip de conector, 35a).
    (a) Sursa = deschiderea materializată de trecerea de an legacy (întâi
    trecerea de an, apoi unealta), nu recalcularea. (b) Idempotență prin `MigrareLegatura`; deschiderea se rescrie integral.
    (d) Solduri contra 891 la 31.12; clasa 8 nu; **terții pornesc pe sold, fără
    facturi istorice**. (e) Lot per codmat, cu data reală (FIFO istoric). (f)
    Contractul: soldurile citite ÎNAPOI = sursa; **diferențele sursei se
    RAPORTEAZĂ, nu se ascund**. Clasificarea repartitorilor: semnalul datelor
    bate eticheta legacy. (g) Amânate → jurnal.

### Profil privat, TVA structural, descărcarea de gestiune (35–38)

35. **Pivot privat-first.** (a) Importul din legacy = proiect viitor. (b) 1C =
    evidență, niciodată canonic. (c) SAF-T/e-Factura/D394 = checklist de
    completitudine, NU modele de date. (d) **Bază-per-client**, profil per bază;
    nicio interogare polimorfă pe `Document` în fluxuri calde. (e) Ordinea: P1
    → P2 → polish → pasul 5.
36. **TVA structural.** (a) `TipTva` (cotă × regim + conturi ca date + coduri
    SAF-T/D394); `TipTvaId?`/`ValoareTva` pe bază; **`ValoareTva` culeasă nu se
    suprascrie la operare** (FCT/FCL/DEC); `Total` = BRUT, imperecherea stinge
    brutul. (b) Pasul TVA în motor, condiționat de `PoliticaTva` a tipului; TI =
    4426=4427; dimensiunile rândului TVA fără override de regulă; conexul
    clonează `TipTvaId`, NU ValoareTva (lotul la net). (c) `ContaSeeder` =
    nucleu + pachete de profil; `VerificaProfil`: profilul nu se amestecă pe o
    bază. Codurile SAF-T sunt direcționale; `CategorieD394` a murit (71c).
    (f) Amânate → jurnal (închiderea lunară = 46c; jurnalele = 68).
37. **Descărcarea de gestiune (design).** (a) Tip `DescarcareGestiune` cu
    `LinieSursaId`; laturi gestiune → client, AMBELE dimensiuni pe gestiune;
    Valoare = cost; fără TVA; bugetar inert; reutilizarea BonConsum respinsă
    (+Consum ≠ ieșire din patrimoniu). (b) Spargerea pe loturi la
    GENERARE; gardianul de sold rămâne autoritatea. Generator =
    `DescarcareService` (nu PoliticaConex) prin `GenereazaSecundar` + acțiune
    manuală pe REST; restul rămâne interogabil; acoperirea = Draft+Operat. (d)
    **General! + Specific?**: `ProdusId` obligatoriu pe liniile de stoc; `LotId`
    = pin OPȚIONAL, prioritar, FĂRĂ fallback FIFO pe restul lui; o gestiune per
    factură. (e) Derivarea de vânzare (371→707, 345→701, 381→708) ca date. (f)
    `TipTvaImplicit` = default de CULEGERE, nu de motor. (g) Amânate → jurnal.
38. **DSC executat.** (b) Pin-urile ÎNTÂI, apoi FIFO; generatorul nu aruncă la
    lipsă (backorder); pin fără sold = refuz la operarea FCL. (c) Linie de stoc
    FCL / DSC fără regulă per Tip = refuz; coerența Tip↔Produs/Lot; `LinieSursa`
    = linia facturii-sursă; liniile de tipul derivat. (d) Acțiunea manuală doar
    pe FCL Operat, pe ușa non-secured (58c). (e) Închis de 39–40.

### Atlas.DXF și polish-ul XAF (39–41)

39. **Atlas.DXF în Conta.** (a) Violările de constraint DB → mesaje de domeniu
    prin `ConstraintViolationTranslator` (bibliotecă); template-urile RO
    (`MesajeConstraintRo`) sunt comune ambelor host-uri. Pachetele PINNATE
    (upgrade = bump explicit). (c) Baseline UI = EntityFluent în
    `ContaUiBaseline`, discovery MANUAL.
40. **Polish XAF.** (a) `[TipDetaliu]` per document: New creează derivata,
    coloanele ei; baseline-ul ascunde FK-urile brute. (b) `RuleRequiredField`
    pe NAVIGAȚII (Guid-urile nu pot purta regula); controllerul comite
    culegerea ÎNAINTE de motor. (c) Read-only post-Draft pe toate căile
    liniilor; multi-tab staleness asumat. (d) Registrele `[ForbidCRUD]`;
    **SmartLookup REVERTAT** la lookup standard (53h). (e) Închise de 41.
41. **Genericul în Atlas.DXF.** (a) `ForbidCRUD` = enforcement de fond. (b)
    `HideForeignKeys()`; `Lot.LinieIntrareId` vizibil intenționat. (c)
    `AutoInclude` DOAR pe navigațiile registrului contabil; liniile și regulile
    rămân lazy. (d) Imperechere: New PERMIS, validat la commit (azi 55a); Edit
    blocat; refuz Plata↔Plata; două link-uri în același commit nu se văd. (e)
    Pachete prin `pack-and-push.ps1` (push-ul = utilizatorul); consum = bump.

### Pasul 5 — designurile fixate și reordonarea (42–44)

42. **Design API.** (a) **O singură sursă de reguli**: gardian de Committing
    activ DOAR pe ObjectSpace-uri SECURED; non-secured = ușa de sistem; nimeni
    n-are Write pe registre. (b) **Motorul în OS non-secured PROPRIU, secvență
    nu cuib**: culegerea comisă secured, apoi comanda prin ID; retur/erori ca
    date (`{documentId, stareNoua, conexId?, mesaje[]}` / 422). (c) **Citirea
    = registre + proiecții** (`Module/Proiectii`, IQueryable + DataSourceLoader,
    atomi partajați; orice proiecție care dublează un calcul al motorului are
    check de consistență în ModelCheck); **TS nu calculează niciodată
    sold/rest/total**. (d)
    **Scrierea = agregat per document** (PUT header + linii, reconciliere
    server-side, WriteDto ≠ ReadDto), felii verticale per tip în Module;
    importul = alt apelant al aceluiași Apply, un document per tranzacție. (e)
    Metadata: build-time
    OpenAPI→TS + captions, runtime doar politici, affordances în ReadDto,
    layout = React. (f) Host separat `Atlas.Conta.WebApi`, același Module,
    OData opt-in DOAR nomenclatoare, un singur updater, release pereche per
    client. Concurența multi-operator rămâne parcată (25f).
43. **Design React.** Vocabular de componente compuse ÎN COD, niciodată
    descriptori interpretați. (a) Felii verticale + `Camp*`: metadata leagă
    atributele, codul decide editorul și prezența. (b) Validare structurală din
    OpenAPI + autoritar = motorul (dry-run `valideaza`); zero motor de reguli
    în TS. (c) State: server-read (TanStack Query), formular per felie (agregat
    local, PUT întreg), efemeride; **URL = starea globală**; liniile de draft =
    editor propriu + grid readonly, fără CRUD per linie în grilă. (d) Codegen =
    tipuri, nu clienți (openapi→TS + dump ModelCheck, artefacte comise, drift
    verificat). (e) Same-host, servit de WebApi. (f) Lookup-uri pe OData
    nomenclatoare, mod local/remote explicit.
44. **XAF Blazor = vehiculul de iterație al modelului**; luptele structurale cu
    Blazor NU se hack-uiesc — merg în `lista-react.md`. Conectoarele de import
    nu au nevoie de tierul API (consolă + OS non-secured + motor direct).

### Faza 1C — călirea pe date reale (45–52)

45. **Design 1C.** (a) Sursa = view-urile SkyConta (contract de coloane). Anul
    se importă OPERAT PRIN MOTOR, idempotent prin `MigrareLegatura`, pe bază
    dedicată; harness, nu go-live. Regula: stocul nu se mișcă fără registru de
    stoc; rândurile TVA ale închiderii 1C se SAR (le generează ITV). (e)
    Contractul de reconciliere trăiește în conector (sold per cont / 4423-4424 /
    stoc per produs×gestiune, lunar; toleranță 0,005); rulajele și per-lot NU
    sunt ținte (nereconciliabile structural); diferențele sursei se raportează. (f) `nou/tools/Import1C` (consolă, fără bibliotecă comună cu
    Migrare — deliberat); profilul se completează PE PARCURS, fiecare gaură = decizie explicită.
46. **Tipurile noi.** (a) **Storno = valori NEGATIVE pe corespondența
    ORIGINALĂ**; `RegulaContare.PastreazaSemn`; flag-ul `Storno` = al
    meta-operației. (b) NTC: postare explicită FĂRĂ regulă doar pe tipurile cu
    `IDocumentCuPostareExplicita`. (c) ITV: `InchidereTvaService`, conturi din
    `PoliticaInchidereTva`; idempotent pe închiderea vie, refuz ne-cronologic,
    anti-stale la operare; NU închide perioada, NU atinge 121. (d) ASM: n→m cu
    `DirectieAsamblare`, invariant |Σproduse − Σconsumuri| ≤ 0,005, ZERO
    contare; dezasamblarea = același tip; consumul unui lot produs de același document = refuz. (e) RLF/RDC
    pe lotul ORIGINAL; RDC = UN document (venit + cost pe LotId); `Total`
    VIRTUAL = doar liniile de venit; Capitalizat REFUZAT. (f) → jurnal.
47. **Import1C — nomenclatoare + deschiderea.** (a) Bază dedicată
    `Atlas.Conta.Import1C.Flax`; `--sabotaj` = auto-testul PERMANENT al
    contractului (proba trăiește lângă contract și cere ambele sonde detectate
    — 52f). (b) Nomenclatoarele mari LA
    CERERE; legătura DUPĂ commit; recuperare pe cod. (c) Deschiderea fără
    dimensiuni (fapt al sursei); orice gaură = FAIL. (d) Lot per (document ×
    produs × simbol cont); netarea retururilor-ca-lot conservă EXACT sumele
    grupei. (e) Reconcilierea recitește INTEGRAL din Postgres. (f) Maparea
    conturilor = LISTĂ (48c). Unealta e single-operator pe bază.
48. **Pre-1C-c.** (a) Pin vs solduri netate = supapă de IMPORT, motorul
    neatins. (b) Extras per rând; imperecherile = trecerea 2 (invariant picat
    pe date reale = raport, nu stop); **NotaContabila operată poate stinge**;
    avizele/retail = surogate. (c) **Maparea conturilor: LISTĂ (CSV comentat),
    NU regulă mecanică** + pre-flight. Idempotență per DOCUMENT.
49. **Documentele prin motor.** (a) **Rolul de stingător e POLIMORF**
    (`Document.CapacitateStingere(os)`, plafon per contrapartidă). Rețeta
    **NTC-punte cu WHITELIST** (fără rost declarat = eșec zgomotos, NU
    catch-all). (d) FCL-ul de import poartă DOAR venit, stocul pe DSC pin-uit.
    (e) **Scara
    numerică** (`Comun/Scara.cs`: bani 18,2 / prețuri 18,6 / cantități 18,3) cu
    GARDIAN (decimal nemapat = throw). (f) Review → 50a.
50. **Gate-ul 1C-d.** (a) Ordine CRONOLOGICĂ pe timestamp-ul sursei;
    **identitatea produsului de import = nomenclator 1C × simbol de cont**;
    Draft la reluare = reimport; contract MĂSURAT prin registrul divergențelor.
    (b) **„Acoperit" cere acoperitor**; skip cu motiv OBLIGATORIU.
    Justificările contractului = egalitate cu registrul și categorii mărginite
    de cifra sursei, nu toleranțe. (e)/(f) → 52.
51. **Post-gate.** (a) Structura de documente NU se redeschide. (c)
    **Convenția de rotunjire = dată de profil, ÎNGHEȚATĂ per bază.** (d) CMP
    parcat cu nume `PoliticaEvaluare`; a forța valorile sursei ca „potrivire"
    rămâne interzis. (e) Schița + întrebările → jurnal; modelul actual e apărat
    ca FIFO (OMFP 96(3)).
52. **1C-d-final — FAZA 1C ÎNCHISĂ.** (a) `SetareProfil` (profil + rotunjire)
    scris de seed, ÎNGHEȚAT; `RotunjestePret` rămâne AwayFromZero FIX. (b)
    Tipurile importului sunt în seed-ul explicit (clasa `TER`); zero gaură
    ad-hoc. Deriva de rotunjire = contractul 4, blocant. Rest → jurnal.

### Gate XAF și dimensiunile pe frunze (53–54)

53. **GATE XAF trecut** („un contabil tolerant le operează zilnic", NU
    product-grade). (a) `ProdusId` pe FCT + seam de culegere a loturilor
    (`LoturiCulegereService`, 56a); `Lot` read-only pe FCT. Număr/scadență la
    MATERIALIZARE. (c) Calculul la culegere refolosește același helper; regula
    36a rămâne a OPERĂRII; `Valoare` read-only în UI. Layout-ul = `.Layout(...)`
    în `ContaUiBaseline` (bază-întâi, grupurile derivatei nested în `Antet`).
    (e) AuditTrail reactivat (DIM-3). (f) **Lotul FINALIZAT nu se șterge
    niciodată de curățenia culegerii**; ViewController-ele se REFOLOSESC
    (niciun cache per view). `lista-react.md` = ce rămâne pentru React. (i)
    Rămase → jurnal.
54. **Owned vs relational.** Stocarea dimensiunilor rămâne INLINE. (c) **Owned
    MOARE; dimensiunile = caracteristică de frunză**: FK-uri pe detaliile
    derivatelor (reuniunea per TIP), `DimensiuniCulese()` → value object
    ne-persistat pentru motor; registru și regulă PLATE. (d) **Clasele per
    profil RESPINSE definitiv.** (e) Frunze proprii NIR și trezorerie; clonările
    prin `[TipDetaliu]`. Executat (DIM-1…4): raport Import1C IDENTIC cu
    baseline-ul. Rămase (DIM-4) → istoric.

### Pasul 5 — feliile API + React (55–68)

55. **Spike BTR — regulile tuturor feliilor.** (a) `GardianEditare` pe familia
    secured (înregistrat în AMBELE host-uri): read-only post-Draft pe starea
    ORIGINALĂ, câmpurile server-owned păzite, registrele doar ale motorului.
    (b) Comenzile = `OperareApi` prin ID în OS non-secured, cu **gate de
    autorizare ÎNAINTE**; `Valideaza` = dry-run. Validarea XAF NU rulează pe
    API (Apply rezolvă FK-urile cu mesaje de domeniu; `422 {Erori[]}`).
    Șablonul feliei: DTO + Apply + proiecții în Module (testabile în ModelCheck),
    controllere subțiri; `Lot` read-only în OData. Client: formular
    hand-rolled pe context (RHF respins). (g) Datorii → jurnal.
56. **FCT + NIR prin API.** `LoturiCulegereService` = seam-ul UNIC al loturilor
    (XAF și Apply). `Numar` e cules DOAR pe FCT (numărul furnizorului); pe toate
    celelalte tipuri e server-owned. Pe linii existente, absența unui câmp
    opțional în PUT = golire deliberată (round-trip). TipTva implicit doar pe
    linii NOI; recalcul DOAR pe
    declanșatori; override de ValoareTva doar pe regimuri cu TVA separat,
    nenegativ (fix de fond viitor: `TvaSuprascris`). OData: nomenclatoarele vii
    = CRUD, politicile = ReadOnly. openapi.json OFFLINE, cu drift verificat.
    (e) Nucleu client: cheile OData sunt `Guid` (compară cu `String()`);
    `laSelectie` = notificare, update FUNCȚIONAL; **widget-ul raportează DOAR
    `e.event`** — formularul e sursa de adevăr. (f) Semantica TVA e în
    ModelCheck pe ambele profiluri.
57. **Trezoreria prin API.** (a) `TrezorerieApply<T>` unic; `Numar`
    server-owned; enum-uri pe sârmă ca string, parse pe NUME înainte de
    CreateObject. (c) `DocumenteCuRest` = UNION pe ramuri CONCRETE, `ReturClient`
    EXCLUS deliberat; `Rest == Ramas` verificat. (d) Affordances țin cont de
    imperecheri. `GenereazaPlata` în DTO-urile FCT. (f) `if (e.event)` pe TOATE
    widget-urile; confirmare inline, nu `window.confirm`. Ștergerea amânată e
    invizibilă la Committing (`EsteSters` în gardian). Enum labels prin
    `[XafDisplayName]`, o sursă pentru XAF + React.
58. **FCL + DSC prin API.** FCL nu naște loturi (pinul REFERĂ). DSC = citire +
    comenzi. (c) **ORICE cale UI care apelează un serviciu ce scrie câmpuri
    server-owned rulează pe ușa non-secured.** (d) Plafon de acoperire per
    linie-sursă contra realității MATERIALIZATE. (e) `Lookup.filtru` (format
    DevExtreme → `$filter`, Guid deduse din forma valorii); pinul se stinge la
    schimbarea produsului.
59. **Perf**: totul sub ~150 ms în afară de `DocumenteCuRest` (~410 ms,
    structural); optimizarea documentată se aplică CÂND cifra o cere; niciun
    index preventiv.
60. **Mărunțișuri.** (a) Constraint-urile DB → 422 și pe WebApi; DELETE = ștergere
    AMÂNATĂ. (b) Tipul documentelor se rezolvă POLIMORF într-un singur query
    (`CoduriTip`; per rând = secunde). (d) Check-uri pe Id repetat / linie de
    BAZĂ prin Id.
61. **Restanțe.** (a) `DocumentSursaTip` în ReadDto; toate link-urile prin
    `rutaTip` — tip fără felie = text, nu link mort. (b) Etichetele liniilor
    nesalvate: culese la selecție, per POZIȚIE, mor la orice re-seed; valorile
    rămân ale serverului. (c) `existaInSet` decide felul laturii; `undefined`
    ≠ „nu"; în afara setului ⇒ afișare statică (default-ul care nu minte).
62. **NIR scriere.** `ILinieCareNasteLot` (FCT, NIR, LDI+) — **FCL/DSC NU o
    declară** (`ProdusId` acolo = pin, semantică opusă); `GestiuneLoturiCulese`
    polimorf. Gard: linia cu lot STRĂIN rămâne NEATINSĂ. Valoarea: lot propriu
    = preț cules × cantitate, lot străin = prețul lotului. NIR nu culege TVA;
    `Numar`/`LotId` server-owned; preț POZITIV la naștere; `PoateEdita` =
    funcție de stare. (f) **Un gard care tace devine capcană unde există și
    alternativa corectă** (`Lot` read-only lângă produs); **conexul autogenerat
    NU se șterge**. (g) → jurnal.
63. **LDI + BCS prin API.** `NasteLot` pe contract (LDI: doar Plus), gard
    înaintea celui de lot străin; gestiunea = PREDATORUL. `Directie` string,
    parse pe nume; `LotId` doar pe Minus; valoare SEMNATĂ. Minus pe lot al
    aceluiași document = refuz; coerența Tip↔Produs. Migrațiile se aplică și pe
    bazele de dev suplimentare (altfel TPT dă 500). Rămase → jurnal.
64. **Viramentul 581.** Niciun tip nou: pereche PLT+INC pe ACELEAȘI laturi
    (direcția o poartă tipul); `NaturaClasa.Virament` + Tipul `VIR` și regulile
    lui = DATE per profil. Cuplajul laturi ↔ natură validat în AMBELE sensuri;
    linie VIR fără regulă potrivită = refuz. Perechea prin `GenereazaSecundar`
    (nu PoliticaConex — fără gard de recursie). Motor: imperecherea automată
    doar dacă `CapacitateStingere != null`; `PoateFiStins` = cealaltă jumătate a
    rolului; viramentul suprascrie `RepartitorImplicit*` (ambele laturi = contul
    piciorului). **Un gard care oglindește o potrivire din motor oglindește
    TOATE axele ei** (și `SemnFiltru`). Ștergerea perechii autogenerate rămâne
    PERMISĂ (pierderea e vizibilă pe 581). Constatare deschisă: dimensiunea
    Repartitor pune contrapartida, nu repartitorul contului — decizie proprie.
    (k) Gaura „al doilea picior manual" — închisă de 65b; rămase → jurnal.
65. **Decont + pereche.** DEC: `ILinieCuPretUnitar`, `Cont/Angajament/Repartitor`
    ReadOnly în OData; filtrele lookup-urilor = afordanță, nu validare.
    `LaturaPerecheId`: o singură parte scrisă, suprimarea generării în AMBELE
    sensuri; gardian simetric + **avertisment consultativ, nu refuz** (două
    viramente identice sunt legitime); reciprocitatea refuzată; **o legătură
    contează doar cu capătul Draft/Operat** (stornatul nu e pereche);
    `PerecheActiva` server-computed; latura generată a altui virament nu e
    pereche pentru un al treilea.
66. **Raportarea pe registre.** Atomul = unpivot pe laturi, o singură definiție;
    perioada și dimensiunile sunt PARAMETRI de proiecție, nu filtre de grilă;
    balanța = o agregare cu sume condiționate; **netarea nu e aditivă ⇒ modul se
    CERE**, sumarele de grup doar pe rulaje. Fișa = SQL brut cu fereastră
    (`GCRecord = 0` explicit; două tipuri: query ≠ sârmă); ordinea în forma
    consumată de DataSourceLoader. SQL-ul brut trece printr-un gate fail-closed
    care MĂSOARĂ echivalența cu calea securizată; LEFT JOIN pe nomenclatoare.
    **Check-ul stă pe CALEA REALĂ** (proba securității = HTTP).
67. **Balanța pliată.** **Brutele se cumulează în sus, netarea se face LA
    NOD**; fără `Summary`; Σ rădăcini == Σ balanța plată. Frunzele = `Balanta`;
    pliul în memorie, fără DataSourceLoader; marginile devin rădăcini. Modul
    analitic nu se pliază; `nivelMaxim` taie rânduri, nu sume; ciclul din
    `Cont.Parinte` oprit prin gardă.
68. **Jurnalele de TVA — `RegistruTva`, al treilea registru**, un rând per
    LINIE (liniile fără TVA postat apar legal în jurnal); generarea cere
    `PoliticaTva` pe tip ȘI `TipTva` pe linie; fără rânduri de deschidere;
    reconciliere PER DOCUMENT; snapshot pentru ce intră în calcul, join pentru
    afișare; backfill = unealtă proprie prin ACEEAȘI funcție a motorului;
    decontul se grupează pe `(Sens × TipTva)`; `Storno` în cheie; linia de cost
    a returului nu poartă `TipTva`. Rămase → jurnal.
69. **D300 = proiecție peste `RegistruTva`.** (a) Rândurile formularului =
    nomenclator `RandD300` seed-uit din lege (nucleu, `ForbidCRUD`, seed-ul
    RESCRIE câmpurile ne-cheie), (b) maparea `(TipTva × Sens) → rând` = politică
    `MapareD300`, n rânduri per pereche, ținta doar `Operatiuni`, niciodată un
    rând și un ascendent al lui; nemapatele deliberate = listă cu motiv în
    profil; politica ștearsă logic rămâne ștearsă la re-seed (indexuri unice
    filtrate pe `GCRecord = 0`). (c) Formulele = COD: listă în memorie, un rând
    per poziție, oglinzile = copie, totalurile din nivelul 0, null (nu 0) unde
    coloana nu există. (d) rd. 31 = rd. 30 − nedeductibil (`Capitalizat`
    snapshot × țintele-operanzi ai rd. 30); validările blocante ale formularului
    = AVERTISMENT, nu trunchiere; `max(…,0)` doar pe 36/37/44/45. (e)
    `Nemapate` e parte din contract; TVA pe coloană absentă = avertisment
    (excepție: TI pe livrare, 69-r4). (f) rd. 37/36 == liniile ITV pe scenă;
    pe baza de import diferența = punțile NTC, raportată. (g) Perioada
    obligatorie; `User` = 200 cu rânduri filtrate (403 e al comenzilor). (h)
    **`if (e.event)` e necesar, nu suficient**: widget tastabil legat de store
    asincron cere buffer local. Rămase → jurnal.
70. **Motor/structură post-D300.** (a) **Taxarea inversă are SENS**: sursa =
    `PoliticaTva.Directie` (nu câmp pe `TipTva`, nu hook pe frunză);
    `TvaService.CalculeazaValori` cere direcția EXPLICIT; TI × Colectat ⇒
    `ValoareTva = 0`, niciun rând; TI × Deductibil = autolichidare; ramura TI
    din motor are gard explicit pe `Deductibil`. (b) TVA nenul pe TI × Colectat
    = refuz în motor (`Opereaza` + dry-run) ȘI la PUT; gardul capturează
    înainte de `PregatesteOperare`, citește `TipTvaId` după. (c)
    `RegistruTvaService` neschimbat; excepția din D300 a murit; Import1C
    păstrează `tipTva = null` „ca în sursă". (d) Datele pre-F13 se RAPORTEAZĂ
    (inventar în ModelCheck), nu se migrează. (e) **ModelCheck șterge ca
    host-ul**: interceptorul `UseDeferredDeletion` (suprasarcina pe options) pe
    ambele builder-e; **curățenia de scenă = purjă FIZICĂ (`Purja`)**,
    `os.Delete` doar unde ștergerea logică e obiectul probei; SQL brut care
    citește pune `GCRecord = 0`. (f) **Un singur 400 = `EroriDto`**
    (`InvalidModelStateResponseFactory`, global pe `[ApiController]`; OData
    neatins); GUID malformat pe rută = 404. (g) Gardian de ciclu pe
    `Cont.Parinte` în `GardianEditare` (navigație → FK, limită 64); `Cont` e
    `ReadOnly` pe OData. (i) Rămase → jurnal.
71. **D394 = proiecție peste `RegistruTva`, per partener.** (a) Formularul =
    OPANAF 3769/2015 mod. 2194/2025 (XSD v1.02; fără ordin 2026); tipul de
    operațiune = ENUM (`TipOperatiuneD394`), maparea = POLITICĂ, tipul de
    partener = COD (funcție a nomenclatorului, definită de lege). (b)
    Identitatea fiscală = 4 câmpuri pe `Partener` (`TipPersoana`, `Tara` ISO-2
    default RO, `InregistratTva`, `TvaLaIncasare`), nu satelit (34g deschis doar
    cât cere D394; adresa a venit tot pe `Partener`, 72a); **„înregistrat bate tot"**: `InregistratTva ⇒ 1` indiferent
    de PF/țară, apoi Fizica/RO ⇒ 2, UE ⇒ 3, altfel 4; CUI normalizat (`RO` tăiat
    când înregistrat sau RO). (c) `MapareD394 (TipTva × Sens) → Tip`, unic
    filtrat, `TintaPermisa(tip, sens)` pe AMBELE axe (AI/N niciodată; L/V/LS ⇔
    Livrare, A/C/AS ⇔ Achiziție); nemapatele deliberate = listă cu motiv;
    **`CategorieD394` a murit** (amendează 36d). (d) UN query grupat pe
    (Document, Storno, Partener, Sens, TipTva, Cota), restul în memorie; rândul
    `op1` UNIT pe CUI peste nomenclatoare (cheia XSD unică; tip 1 dacă vreun
    partener e înregistrat); `AI` derivat; cota 0 pe V/LS/AS/N; **`nrFact` pe
    (Document × Storno)** — stornoul e factură proprie; TVA pe V =
    `TvaNedeclarat`; partenerul șters logic se DECLARĂ. (e) Nimic nu se pierde:
    Σ op1 + Σ `Neincluse` == Σ registru per sens; cusătura cu D300 (rd.
    9/24/12.1/13, `Neincluse` == rd. 14/15/29) la cent. (f) Ce cere formularul
    și modelul nu are ⇒ avertisment AGREGAT per cauză (`CodAvertismentD394`),
    nu 0, nu string-uri. (g) `GET api/proiectii/d394`, `User` ⇒ 200 gol; ecran
    `/d394`; bani exacți, rotunjirea e a fișierului. (h) Import1C: sursa ⇒
    preluare, sursa tace ⇒ derivare RAPORTATĂ; **`--reclasifica` = sursă +
    semnalul din registru** (TVA ≠ 0 pe achiziție ⇒ înregistrat; evidența bate
    eticheta, 34f), și ca pas final al importului. (j) Rămase → jurnal.
72. **Partener + ANAF.** (a) **Adresa = câmpuri PLATE pe frunza `Partener`**
    (Strada/Numar/DetaliiAdresa/Localitate/CodPostal/JudetId), `MaxLength` =
    SAF-T și e SINGURA sursă a lungimilor (reflecție, `Lungimi`, în serviciu
    și în Import1C); amendează 71b — satelitul 34g rămâne doar pentru
    IBAN/delegați/contact/adrese multiple. `DataSincronizareAnaf` și
    `InactivFiscal` = server-owned (gardianul le refuză pe secured). (b)
    `Judet` = nomenclator de nucleu, `ForbidCRUD`, seed autoritar din
    `JudeteRo` (ISO 3166-2 + auto + CNP); **județ doar pe `Tara == RO`**, pe
    toate cele trei uși (gardian, serviciu, Import1C); grafiile 1C se
    normalizează în conector. (c) `PlatitorTvaClient` v9 = o clasă în Module,
    fără DI (host-ul dă `HttpClient`): loturi ≤100, 1 apel/s intra-interogare,
    deserializare tolerantă, erori per lot tranzitorie/fatală; `CuiInterogabil`
    = doar cifrele, 2–10, CNP și străinii = necandidați. (d) **Merge: „gol se
    umple, diferit se raportează, canonicul bate"** — axa TVA
    (`InregistratTva`/`TvaLaIncasare`/`InactivFiscal`) e a ANAF-ului
    întotdeauna; adresa/denumirea PER CÂMP; `suprascrie` explicit doar pe
    REST, cu `Modificare(vechi, nou)`; `notFound` = fără timbru. (e) REST =
    comandă pe partener (single + lot ≤500 cu `Sarite`), `ComandaAutorizata<T>`
    generalizează gate-ul documentelor FĂRĂ a-l schimba pe `Document`; domeniu
    ⇒ 422, ANAF tranzitoriu ⇒ 503; V4 măsurat pe HTTP (§Închidere al
    contractului). (f) XAF: acțiunea pe ușa non-secured, fără `suprascrie`. (g)
    Import1C: adresa din 1C DOAR pe bloc gol, județ CNP → denumire → brut în
    `DetaliiAdresa`; `--anaf` peste același serviciu, commit per partener;
    **canonicul ANAF nu e răsturnat de `--reclasifica`** (timbrații sar axa
    TVA). V5: reconciliere IDENTICĂ cu baseline-ul, 8.230/2/0 erori, +190 tip
    1, D394 înainte/după explicat per cauză. (i) Restanțe 72-r1…r10 → jurnal.

## Stare și roadmap

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
  D300 (69), motor/structură post-D300 (70), D394 (71), partener + ANAF (72).

**Următorul pas**: finisajul clientului (listele §Închidere ale contractelor +
`docs/api/lista-react.md`; licența DevExtreme = acțiunea utilizatorului);
feliile de scriere rămase (NTC/ASM/retururi, la cerere); SAF-T peste
`RegistruTva`, pe tiparul D300/D394.

**Amânări și restanțe cu nume** (textul în fișierul deciziei; numele aici ca
să nu se piardă): 21 defalcarea multi-sursă (F) · 31f importul extraselor,
imperecherea pe poziții · 31e `GenereazaChitanta` (fluxul BF) · 34g satelitul partenerilor (IBAN/delegați/contact/adrese multiple; adresa e pe `Partener`, 72a),
valute · 36f TVA la încasare, facturi nesosite 408/4428, rotunjirea per
document×cotă, prorata/ajustări · 37g comenzi, regenerarea DSC la recepția
NIR, multi-gestiune per factură, rezervarea de stoc · 46f imperecherea
returului, toleranța ASM, disciplina de apelant ITV · 51e `PoliticaEvaluare`
(CMP) · 52h consumul ASM pe proveniență, reziduul TRANZIT · 53i culegerea de
produs pe ASM, localizarea shell-ului, perioadele fiscale manuale, `Data` pe
conexe · DIM-4 curatoria grilei registrului, vizibilitatea dimensiunilor per
profil (`SetareProfil`) · 55g JWT secrets la deploy, `$metadata` expune tot
modelul, `Lot.Eticheta` pe OData · 62g/66j finisaj de client și ecrane XAF
(`lista-react.md`) · 63f laturile interne pe `Calitati`, retrofit
`MaterializeazaValori` pe BTR · 64h dimensiunea Repartitor pe rândul de bani
(decizie proprie) · 64k comisionul bancar, valuta · 67e gardian de nomenclator
pentru ciclul din `Cont.Parinte` · 68j smoke vizual, storno/regimuri fără TVA
în backfill · 69-r1 versionarea formularului D300 · 69-r2 rândurile
intracomunitare/agricultori/pro-rata/secțiunile A-B · 69-r3 regularizările pe
cauză juridică · 69-r6 fișierul XML D300 · 70-r1 refuzurile gardianului pe scrierile OData ies
`400 text/plain` (decizie proprie) · 70-r2 smoke XAF al gardianului de ciclu ·
70-r3 poziția „linia N" fără criteriu · 70-r4 `DirectiePentru` per
`ObjectChanged` · 70-r5 mesajele de binding în engleză · 70-r6 `TvaSuprascris` ·
D4-r1 istoricul statutului de TVA (canonicul = registrul ANAF) · D4-r2 adresa
PF fără CNP · D4-r3 `N` + `tip_document` · D4-r4 data primirii facturii ·
D4-r5 op11 / cod NC pe produs · D4-r6 bonurile fiscale (G, I.1) · D4-r7 I.2
facturi/plaje/autofacturi/anulate/simplificate · D4-r8 I.3 · D4-r9 sumele
TVA la încasare (I.4/I.5) · D4-r10 antet/reprezentant/CAEN/I.6/opțiune
(`SetareProfil`) · D4-r11 partenerul cu două coduri (SM + RO) · D4-r12
achizițiile de pe DEC fără furnizor · D4-r13 XML D394 (35c) · D4-r14
`FaraOp11` dispare odată cu r5 · 72-r1 rate-limit ANAF cross-request ·
72-r2 adresa hibridă 1C + ANAF fără proveniență per câmp · 72-r3
`IntrariStricate` = fatală după commit · 72-r4 radierea (`StareInregistrare`)
neconsumată · 72-r5 smoke vizual XAF al acțiunii ANAF · 72-r6
`RegistruContraAnaf` real abia la a doua rulare · 72-r7 ordinea la egalitate
în raportul de reconciliere · 72-r8 `CuiInterogabil("00")` · 72-r9 ecranul
React de partener · 72-r10 `User` ⇒ 404 vs 403 pe comanda ANAF
· C1a fluxul comenzilor
(`docs/architecture-notes-2026-07-28.md`).

## Reguli de lucru pentru Claude Code

- **Decizie nouă** = fișier nou `docs/decizii/NNN-slug.md` (numărul următor;
  antetul cu Data/Stare/Docs + textul integral: context, tranșări, review, ce
  rămâne deschis) + o linie în `docs/decizii/README.md` + rezumatul cu aceeași
  numerotare/litere aici — regula, nu povestea; restanțele = nume în
  „Amânări cu nume", textul doar în fișierul deciziei. Numerele și literele nu
  se renumerotează niciodată; o decizie depășită/amendată își schimbă `Stare:`
  în antet și rezumatul de aici („depășită de N"), textul nu se șterge. Când
  ai nevoie de „decizia N": deschizi UN fișier, nu directorul.
- **Orice propunere arhitecturală se testează întâi contra `docs/invarianti.md`**;
  o felie mare are contract scris în `docs/` (D-uri pin-uite, regulă de oprire,
  review advers la închidere) — precedentele: `docs/api/p5-*-contract.md`.
- **ModelCheck rămâne verde pe AMBELE profiluri** după orice schimbare de
  model/motor/politică; migrațiile EF sunt canonice (23a); `--dump-metadata`
  la orice caption nou; driftul openapi verificat (jurnal 56d).
- Motorul nu cunoaște frunzele: ce are nevoie de la un tip primește prin
  contract (hook polimorf / interfață declarată), niciodată prin `is`/`switch`.
  Niciun simbol de cont hardcodat în motor (29).
- Sursele externe (legacy, 1C) sunt evidență, niciodată canonic (21/35b);
  cazurile speciale descoperite acolo se raportează înainte de a decide unde
  ajung în model. La explorarea legacy: grep selectiv pe tabele/câmpuri, nu
  citit formuri la rând.
- Probele se fac pe CALEA REALĂ (66h): HTTP pentru securitate, browser pentru
  UI; schimbările de motor/registre au ca probă supremă re-rularea integrală
  Import1C cu raport identic cu baseline-ul (precedentul DIM-4) — la felii
  mari, nu la orice commit.
- Rulările lungi = proces detașat + monitor, nu task de fundal al harness-ului
  (jurnal 50d).

## Cunoștințe utilizator (context)

Dezvoltator .NET cu experiență de producție în DevExpress XAF (Blazor Server),
Serenity, React, Flutter, EF, OData. Fluent cu pattern-ul de codegen
C#→TypeScript din Serenity (echivalentul mental al OpenAPI→TS).
Motivația migrării de pe XAF Blazor pe frontend React: limitări structurale
ale ObjectSpace-ului sincron (fără async nativ, dialoguri, extensibilitate
greoaie) — nefixabile la nivel de librărie.
Dezvoltatorul inițial al aplicației legacy.
