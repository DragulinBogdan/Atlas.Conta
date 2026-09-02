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
73. **SAF-T (D406 L) = proiecție peste registre + FIȘIER.** (a) `Societate` =
    un rând, nucleu, EDITABIL (nu `SetareProfil`): antetul + identitatea
    raportorului (`CustomerID` ȘI `SupplierID` sunt obligatorii AMBELE pe
    orice linie, latura liberă = raportorul); adresa cu aceleași câmpuri/
    lungimi ca `Partener` (`AdresaSaft.Lungimi`); unicitatea în gardian;
    seed-ul creează gol, nu rescrie. (b) `UnitateMasura` nucleu, `ForbidCRUD`,
    seed UN/ECE; `Produs.CodNc` (8 cifre) + `UnitateMasuraId`; `UM` string
    rămâne; grafia RO → cod fără ghicit (`mc` = metru cub, `ml` nerezolvat).
    (c) `Cont.RolTert` și `Cont.Functie` = DATE per profil; bugetar =
    **neaplicabil** (422). (d) Funcțiile legii = COD (`SaftReguli`): identitatea
    partenerului `00`–`06` (`00` cere CUI VALID), `IdSocietate` ≠
    `RegistrationNumber`, 380/381, tuplele metodei de plată; `NormalizeazaCui`
    taie `RO` repetat. (e) Proiecția: jurnal = `TipDocument`, rând contabil ⇒
    două linii, **partenerul de pe RÂND, rolul al CONTULUI** (64h confirmată);
    `TaxInformation` din `RegistruTva` pe `Detaliu × Storno`; storno = factură
    proprie `381` negativă; liniile de stoc FCT își iau contrapartida din
    NIR-ul conex MATERIALIZAT, niciodată un cont inventat; **nimic nu se
    pierde**: `Neincluse` + avertismente agregate + cusăturile în DTO (partidă
    dublă, TVA cu trei termeni, facturi per sens, solduri, master files).
    (f) `SaftXml` streaming pe XSD-ul oficial; **DUK = oracolul** din
    ModelCheck (`-d` inutilizabil în CLI); măsurat: 7 cifre de cont,
    `ExchangeRate` absent, `0` pe NC, diacritice TREC. (g) JSON = **SUMAR**
    (38,6 MiB/lună a contrazis „nu se paginează"); XML streaming cu
    `AllowSynchronousIO`; **`User` ⇒ 403 pe fișier** (un fișier gol semnat e o
    declarație falsă), 200 gol pe sumar. (h) Client: descărcare prin `fetch` +
    `blob`. (i) Import1C: `Societate` din 1C pe câmp gol, UM/NC, `--saft`;
    V5: DUK `ok` pe lunile reale, reconcilierea neatinsă. (k) Restanțe
    73-r1…r18 → jurnal (73-r1 închisă de 74).
74. **SAF-T S (stocuri) = proiecție peste `RegistruStoc` + FIȘIER `C`.** (a)
    `PoliticaMiscareSaft` = `(TipDocument × TipStoc × Semn?) → CodMiscare? +
    RolTertSaft + Motiv`: codul de mișcare e al TIPULUI × registrului, deci
    politică (4) pe cheia lui `RegulaStoc`; `Semn null` = orice semn; două
    indexuri unice filtrate (`NULL <> NULL`); **cod NULL = excludere
    DELIBERATĂ cu motiv** (`Excluse`) ≠ rând fără politică (`Neincluse`);
    gardian pe ușa comună + seed-ul își validează tabelul; bugetar zero,
    OData ReadOnly (56). (b) Legea = cod (`SaftReguli`): 19 coduri; terții pe
    linia de stoc — Client `(p,"0")`, Furnizor `("0",p)`, intern
    `(soc,soc)` (ALTĂ convenție decât L); `OwnerID` = raportorul,
    `ProductType` = simbolul contului de stoc, `MovementReference` ≤ 35 cu
    discriminant `#n` când `Numar` se repetă (nu e unic per tip). (c)
    Proiecția: politica potrivită pe **semnul REGULII** (`(Storno ? −1 : 1) ×
    sign(Cantitate)` — stornoul păstrează codul original); `PhysicalStock`
    per `(Repartitor × Lot)` pe `TipStoc`-urile cu cod, două agregate
    grupate; `MovementOfGoods` per `(Document × Storno × Cod)` (`/cod` la
    spargere, `/S` pe storno); partenerul de pe laturi sau ale
    `DocumentSursa`; contul liniei = `TipMaterial.ContImplicit`, niciodată
    inventat; `Quantity`/`BookValue` SEMNATE ca în registru;
    `MovementPostingDate` omis în afara perioadei; `AnalysisTypeTable` gol.
    (d) Cusături: S1 (registru) **și S5 (liniile EMISE)** per intrare, S2
    nimic nu se pierde, S3 vs balanță per cont RAPORTATĂ (spartă pe tip de
    document), S4 referințe + unicitatea `MovementReference`. (e) Profilul
    `C` măsurat cu DUK: secțiunile L COMPLET goale (fără totaluri),
    `PhysicalStock` OBLIGATORIU prezent ⇒ lună fără stoc = refuz înainte de
    primul octet / 422, `MovementOfGoods` gol trece. (f) REST
    `saft/stocuri` + `stocuri/xml` cu gărzile o singură dată (proiecția ca
    funcție); `/saft?fel=S`; `--saft-s`; seed pe Flax cere
    `EFCoreProvider=Postgres;`. (g) Deriva per lot a importului (45e) se
    DECLARĂ ca atare (`ReziduValoricFaraCantitate`, `SoldNegativ`), nu se
    ascunde. (h) Restanțe 74-r1…r15 → jurnal (74-r4/r6/r9 închise de 75).
75. **Restanțele grele ale lui S (F18).** (a) **Golirea valorică e a
    MOTORULUI**: ieșirea care golește cheia (Lot × Repartitor × TipStoc)
    preia tot soldul valoric rămas — `StocService.ValoareGolire` (pură) +
    `AplicaValoareIesire` în `MotorOperare`, DUPĂ `PregatesteOperare`,
    ÎNAINTE de `ValideazaOperare` (33d); frunzele rămân previzualizarea
    `preț × cantitate`; Import1C prezice prin ACELAȘI helper. Limitele se
    DECLARĂ, nu se corectează tăcut: retro (golirea se decide la operare pe
    registrul existent — un document retro nu re-decide linii operate) și
    fiscal — **RLF NU absoarbe restul** (`IDocumentCuIesireFiscala`, marker
    pe clasa de document; suma returului = hârtia furnizorului, reziduul
    rămâne pe lot). (b) **Reclasificarea de cont la transfer = MIȘCARE**:
    ASM `#reclas` → BTR → NTC-punte, în ordinea asta (puntea transcrie
    mișcarea; fără ASM operat nu se scrie nimic, rândurile sursei se declară
    nepostate); cheia cerută de sursă dar fără ASM produs se leagă „fără
    document" (ținta `Guid.Empty`); codul S al reclasificării = al ASM-ului
    (74a). (c) **Oracolul golirii** = linia de contract `1'` a
    reconcilierii: `StocService.VerificaGoliri` (pură, verdicte Exacta/
    CuValoare/Fiscala/ReDeschisaRetro/Negolita) pe cifra REGISTRULUI
    (necircular); stornatele pe `DetaliuId` se sar; D4 = defalcare, luna +
    cumulat. (d) Perf S: `AgregatStoc` o trecere, `CoduriTipPeTipuri`
    (ancoră, Guid-uri per tip) partajat; ținta < 1 s neatinsă FĂRĂ vinovat
    dominant — optimizarea următoare doar cu cifră (59). Proba: re-rularea
    integrală Flax pe codul final, contract 12 luni / 0 FAIL, 381/608 dispar
    din divergențe, DUK ok, `ReziduValoricFaraCantitate` 861/1.168 →
    131/132. (e) Restanțe 75-r1…r5 → jurnal.
76. **NTC + ASM + retururi prin API și client** (ultimele patru tipuri fără
    felie de scriere). (a) Trei tracks independente, cu regulă de oprire per
    track; ITV rămâne afară (nu e document cules, e serviciu — 46c); zero
    seturi OData noi. (b) **ASM capătă culegerea de produs** (`ProdusId` +
    `ILinieCareNasteLot` cu `NasteLot => Directie == Produs`,
    `GestiuneLoturiCulese` → **PREDATORUL**) — închide 53i pe ASM. (c)
    **Valoarea produsului ASM se derivă din consum** (închide 75-r1):
    `distribuie-valoarea` cere cifra **MOTORULUI** (`MotorOperare.Valideaza` pe
    un OS de unică folosință, al doilea OS obligatoriu fiindcă `Valideaza`
    SCRIE), nu o formulă geamănă; „calculul n-a rulat" se detectează
    STRUCTURAL, nu din textul erorilor; reziduul se plimbă, iar cazul
    nereprezentabil (75-r4) și cel MIXT se REFUZĂ cu cifra. (d) Semnarea storno
    rămâne a OPERĂRII — RLF/RDC se culeg pozitiv, ecranul explică semnul în loc
    să-l inverseze; linia de cost RDC se persistă cu `TipTvaId = null`; rolul
    liniei RDC e o PREZENȚĂ, iar mutarea lui pe o linie salvată se REFUZĂ (o
    conversie ar rescrie tăcut culegerea). (e) Idempotența: pe draft bate
    `MaterializeazaValori`, la operare bate hook-ul — nu se contrazic fiindcă
    amândouă pleacă din `Abs`; motorul NU de-semnează la anulare. (f)
    **Plafonul de stingere capătă LATURĂ și se NETEAZĂ** (amendează 31d/48b):
    `Document.SensDeStins` = al treilea hook polimorf al rolului (default `null`
    = motorul nu ghicește); plafonul = **`|Σ semnat|` per (repartitor × latura
    liniei)**, cheia cu net 0 nu intră deloc — netarea SUBSUMEAZĂ linia
    negativă. `PLT → FCL` și `INC → FCT` se refuză de acum, DECLARAT. Panourile
    clasice urmează sensul din ReadDto (`SensCandidati`), nu din TS. Refuzul de
    ambiguitate primește ieșire prin MODELARE (`NIR` = `Datorie`), niciodată
    printr-un câmp care lasă apelantul să aleagă jumătatea. (g) `PanouStingeri`
    capătă un al doilea mod (grupat per contrapartidă × sens); pe notă **„Rest"
    dispare** (Σ liniilor nu e o creanță); NTC nu intră în `DocumenteCuRest`,
    retururile nu devin stingători — compensarea trece prin notă (46f rămâne,
    cu cale practică). (h) Bara ASM: cifra e NEUTRĂ, culoarea stă pe verdictul
    dry-run-ului — `Diferenta` minte în ambele sensuri, deci orice culoare pe ea
    reproduce o minciună. (i) Restanțe 76-r1…r6 → jurnal.
77. **Finisajul clientului (F20).** (a) **Căutarea fără diacritice e a BAZEI
    DE DATE, ca o coloană GENERATĂ, nu ca o colație**: `Cautare` =
    `translate(lower(cod || ' ' || denumire), De, La)` STORED (ambele
    IMMUTABLE; `concat` nu e) pe orice nomenclator care declară `ICuCautare`,
    configurată printr-o buclă generică pe model (o coloană pe baza TPT;
    `Simbol` pe `Cont`); colația ICU nedeterministă rupe `LIKE`/`contains`,
    `unaccent` nu poate fi injectat în `$filter`-ul compus de `ODataStore`.
    Tabelul `De`/`La` (`Comun/Cautare.cs`) e UNICA sursă: SQL, C#
    (`Normalizeaza`) și client (prin `metadata.json`), cu oracol SQL == C# pe
    toate rândurile în ModelCheck. (b) **Un singur store OData în client**
    (`nucleu/odata.ts`): `byKey` prin cache-ul TanStack pe `(entitate, id,
    proiecție)` cu `staleTime: Infinity` — proiecția e în cheie, altfel cache-ul
    minte; `cache.clear()` la „Ieșire" (logout-ul e navigare SPA); `Lookup`
    caută default pe `Cautare` și rescrie literalul în `beforeSend` pentru
    `contains`/`startswith`/`endswith`. (c) Precompletarea scrie perechea (id,
    etichetă) printr-un singur verdict „e gol" (`nucleu/etichete.ts`), sursa =
    cache-ul, nu `$expand` imbricat. (d) `ConfirmareInline` + slot în
    `DocumentShell`; **un slot de `ReactNode` nu se compară cu `null`** (`false
    == null` e fals — storno a murit pe 11 ecrane, invizibil pentru `tsc`).
    (e) `Neincluse` pleacă AGREGAT per cauză în sumar (funcție pură pe lista
    plată, care rămâne în fișier); `Suma` = semnată pe S, absolută pe L;
    `ContId` pe S3 ⇒ fișă. (f) Listele legii care sunt COD se PUBLICĂ declarat
    în `metadata.json` (`Nomenclatoare`), nu devin entități. (g) Refuzurile de
    DOMENIU pe `api/odata/*` ies `422 EroriDto` (`RefuzDomeniuOdataFilter`,
    `Order = int.MaxValue`, în WebApi); permisiunea rămâne 404/403 text, tradusă
    doar în client. (h) Șablonul ecranului de nomenclator (`felii/nomenclatoare`):
    scriere prin OData, PATCH = DELTĂ (absența NU e golire, spre deosebire de
    PUT-ul documentelor), lungimile din schemele OData ale `openapi.json`;
    Partener + ANAF, Societate, Produs; `PoliticaMiscareSaft` DOAR citire (56
    nu se redeschide). (i) Licența DevExtreme = `VITE_DEVEXTREME_LICENSE`.
    (k) **Un nomenclator căutabil are cod și denumire**: pe orice `ICuCautare`,
    coloana de cod (`Cod`/`Simbol`, `Cautare.NumeCod`) și `Denumire` sunt NOT
    NULL + CHECK `btrim <> ''` în schemă (ușa de sistem), refuzate cu mesajul
    câmpului de `GardianEditare` (ușa secured, înaintea switch-ului pe tip),
    `[Required]` ⇒ OpenAPI `required[]` ⇒ asterisc/validare în client,
    `[RuleRequiredField]` pentru XAF (Validation nu citește DataAnnotations).
    Migrația nu maschează goluri — o bază cu rânduri goale pică zgomotos.
    (j) Restanțe 77-r1…r8 → jurnal.
78. **Căutarea fără diacritice pe PROIECȚII** (perechea lui 77a — acolo
    coloana generată pe OData, aici ușa `DataSourceLoader`). (a)
    `Cautare.FaraDiacritice` = funcție de query (corp C# null-propagant),
    tradusă de EF pe EXACT fragmentul coloanei generate —
    `Cautare.FragmentSql` e UNICA ortografie a lui `translate(lower(…))`,
    consumată și de `ExpresieSql`, și de `HasTranslation` (cusătura verificată
    în ModelCheck pe `ToQueryString`). (b) `CautareFiltru` = compilator custom
    global (`RegisterBinaryExpressionCompiler`, idempotent per proces,
    înregistrat în Startup-ul WebApi și în ModelCheck): DOAR
    `contains`/`notcontains`/`startswith`/`endswith` pe accessor `string` ⇒
    `FaraDiacritice(coalesce(camp,'')) op literalNormalizat` (literalul în
    C#); orice nerezolvare = compilarea standard; **`=`/`<>` rămân exacte**
    (lista `HeaderFilter` trimite valoarea exactă; egalitatea normalizată ar
    topi valori distincte). Zero schimbări per proiecție sau în client. (c)
    `coalesce` apără sursele în memorie (fișa); pe `notcontains` nulul
    CONTEAZĂ ca „nu conține" — asumat. (e) Perf: nimic preventiv (59) —
    `contains` e ne-btree oricum; la cifră: GIN `pg_trgm` pe `Cautare`
    (77-r7) sau pe expresie (IMMUTABLE, indexabilă fără persistare).
    Măsurat pe HTTP (Privat, 19k FCT): trei grafii ⇒ același total, 56–121 ms.
    Restanța 78-r1 → jurnal (grilele XAF rămân sensibile — asumat, 44/53).
79. **ITV prin API și client — COMANDĂ cu cauză, nu agregat** (76a). (a)
    `InchidereTvaService` întoarce `RezultatInchidere` cu `MotivNegenerare`
    (`ProfilInert`/`InchidereVie`/`FaraSold`/`NeCronologica`; enum-ul stă în
    `BusinessObjects/Comun` — dump-ul de metadata ia doar spațiul ăla);
    `CalculeazaLinii` = SINGURA aritmetică a celor trei linii; `Analizeaza` =
    ordinea gardienilor o singură dată; **raportul (`Previzualizeaza`, nu
    scrie) și comanda (`Incearca`, nu comite) diferă printr-un bit: cronologia
    e motiv la raport, refuz zgomotos la comandă** (46c rămâne); **cronologia
    are AMBELE sensuri și AMBELE uși** (review): un draft neoperat pe o lună
    anterioară blochează generarea (`DraftAnterior`), o închidere OPERATĂ
    ulterioară blochează OPERAREA (gard în `ValideazaOperare` — altfel 4423
    se dubla cu ecranul spunând că e în regulă); perioada fiscală închisă =
    `PerioadaInchisa` la raport / refuz la comandă; solduri `null` (nu 0) pe
    profil inert; unitatea ne-internă = refuz la GENERARE;
    `LiniiPotrivescSoldurile` = SINGURUL criteriu anti-stale (gardian ȘI
    `Stale` din DTO); `Genereaza` = wrapper, Import1C neatins. (b) **Gate-ul comenzii fără
    subiect e pe TIP** (`PoateCrea` = `CanCreate(tip, os)`; `PoateCiti` pe
    previzualizare; `AutorizeazaCitire<T>` pe instanță: invizibil 404,
    vizibil fără drept 403), luat pe ușa securizată ÎNAINTE; **cifrele
    motorului (solduri, `Stale`, liniile) se calculează pe ușa NON-SECURED**
    — pe cea filtrată ar fi o cifră falsă, nu goală (73g). Listele rămân
    securizate. (c) **ITV iese din felia NTC**: `Lista`/`Citeste`/`Candidati`
    filtrează `!(d is InchidereTva)` (tradus pe TPT), PUT/DELETE refuză 422;
    `is` la graniță, în Apply, nu în motor; comenzile NTC pe id ITV NU mai
    sunt permise — 404 pe toată ușa NTC (amendat de 80b, `peUsaAsta`). (d) `genereaza` răspunde **200 și când
    nu generează** (raport: `Motiv` + `InchidereVieId`), 422 doar pe domeniu;
    `regenereaza` = `Incearca(…, inlocuieste: id)` ÎNAINTE de ștergere, o
    singură tranzacție — un refuz lasă draftul intact (review: forma
    „șterge, comite, apoi încearcă" pierdea draftul); cere și `PoateCrea`
    (produce un document nou); unitatea = parametru
    cules, precompletat doar la exact un rând; lista cu ordine implicită
    DECLARATĂ (`Data` desc). (e) Client: previzualizarea lunii pe listă
    (motivul tradus, link către închiderea blocantă), documentul read-only cu
    `Stale` ⇒ atenție, storno cu data implicită = data închiderii (46f);
    `rutaTip('ITV')`. (f) Restanțe 79-r1…r5 → jurnal.
80. **Refuzurile de acces pe toate ușile: 404 / 403 / 422, o singură ordine,
    un singur corp** (închide 77-r8 și familia 70-r1/72-r10/76-r4/76-r5/77k/
    79-r6; amendează 79b). (a) **404** = subiectul cererii e inexistent SAU
    invizibil, DELIBERAT nedistinse (fără oracol de existență), doar pe rutele
    cu subiect; **403** = vizibil (sau întrebarea e pe TIP), dar operația e
    refuzată (Create pe tip, Write/Delete pe instanță, Read pe tip); **422** =
    domeniu, doar pe cereri permise — un 422 nu poate ascunde un refuz de
    permisiune; listele/sumarele rămân 200 filtrat. Ordinea pe sârmă, pe TOATE
    ușile: **401 → 400 → 404 → 403 → 422**. (b) **Ușa de scriere REST are gate
    explicit pe tipul FELIEI**: `CreareAutorizata<T>` (`CanCreate` pe tip,
    înaintea Apply-ului și a oricărei rezolvări de FK), `ScriereAutorizata<T>(id,
    Modificare|Stergere)` — Write și Delete sunt permisiuni DISTINCTE, deci
    operația e PARAMETRU al gate-ului, nu a doua metodă; `ComandaAutorizata<T>`
    cu `T` = tipul feliei, nu `Document` (id de alt tip ⇒ 404); predicatul
    opțional `peUsaAsta` exclude ce TPT găsește dar felia nu servește (NTC ⇒
    ITV e 404 pe toate verbele; amendează 79c). Pe obiecte NOI gate-ul și
    pasul zero cer Create ȘI Write (plasa DevExpress le cere pe amândouă).
    `GET {id}` rămâne fără gate (ușa securizată filtrează). (c) **Gardianul întreabă
    securitatea ÎNAINTEA domeniului**: pasul zero din `GardianEditare.
    OnCommitting` (`CanCreate`/`CanDelete`/`CanWrite` per obiect din
    `ModifiedObjects`) aruncă `RefuzAcces : IUserFriendlySecurityException`
    (NU derivă din `OperareException`), apoi `Verifica`; strategia e injectată
    NULLABLE — fără ea (ModelCheck, ușa de sistem) pasul tace; plasa DevExpress
    din `SaveChanges` rămâne. (d) **Un singur corp**: `EroriDto` pe 403/404 pe
    REST și OData, mesajele cu o singură sursă în Module (`Api/Refuzuri.cs`:
    `Invizibil`, `FaraDrept(operație, tip)`); niciun `Forbid()`/`NotFound()`
    gol; `RefuzOdataFilter` traduce `HttpUserFriendlyException` (pe TIP, înaintea
    interfeței; 404 ⇒ `Invizibil`), `IUserFriendlySecurityException` ⇒ 403,
    `IUserFriendlyException` ⇒ 422. (e) **Cifrele motorului cer dreptul de
    citire pe REGISTRUL din care se însumează**: ITV `GET {id}`/`previzualizare`
    cer și `CanRead(RegistruContabil)`; regula generală pentru orice rută cu
    sumă pe ușa non-secured (73g). (f) **FK invizibil pe ușa securizată = 422 cu
    mesaj onest** printr-un singur helper (`Rezolva.Cere`/`Optional`: „nu există
    sau nu e vizibil(ă) pentru utilizatorul curent"). (g) Rolul `Cititori`
    (`ReadOnlyAllByDefault`) + userul `Cititor`, dev-only — 403-ul pur e
    măsurabil. (h) Clientul: o singură ramură pe `Erori[]` (400/403/404/422 ⇒
    `EroareDomeniu`, `status` informativ, fără ramificare), textele inventate pe
    `/api/odata/` au murit; `dxStore.onAjaxError` rescrie `e.error`. (i)
    **Securitatea se măsoară pe HTTP**, cu script repetabil
    (`nou/tools/ProbeHttp/refuzuri.ps1`, Admin/Cititor/User, PASS/FAIL, fără
    urme); ModelCheck nu capătă strategie de securitate. (j) Restanțe
    80-r1…r5 → jurnal.

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
  pasul zero al gardianului, probele HTTP cu script (80).

**Toate tipurile de document au acum felie prin API și client** — ITV ca
COMANDĂ (79), nu agregat; singurul rămas e BPR (rezervat, 19). Refuzurile de
acces sunt uniforme pe REST și OData și MĂSURATE (80).

**Următorul pas**: 79-r1 (acțiunea XAF de generare) la cerere; 80-r1
(motivul refuzului pe conducta `ODataStore` a clientului) dacă expunerea
crește; `lista-react.md` mai ține doar itemii structurali și 77-r1/r3/r6.
Capcane de probare: `genereaza` SCRIE ori de câte ori luna e liberă (79);
probele de securitate se rulează prin `nou/tools/ProbeHttp/refuzuri.ps1` pe
host viu (Privat, după re-seed pentru `Cititor`), nu se refac de mână.

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
`400 text/plain` (închisă de 77g + 80d) · 70-r2 smoke XAF al gardianului de ciclu ·
70-r3 poziția „linia N" fără criteriu · 70-r4 `DirectiePentru` per
`ObjectChanged` · 70-r5 mesajele de binding în engleză · 70-r6 `TvaSuprascris` ·
D4-r1 istoricul statutului de TVA (canonicul = registrul ANAF) · D4-r2 adresa
PF fără CNP · D4-r3 `N` + `tip_document` · D4-r4 data primirii facturii ·
D4-r5 categoria `op11` (codul NC e pe produs, 73b) · D4-r6 bonurile fiscale
(G, I.1) · D4-r7 I.2 facturi/plaje/autofacturi/anulate/simplificate · D4-r8
I.3 · D4-r9 sumele TVA la încasare (I.4/I.5) · D4-r10 CAEN/I.6/opțiune
(antetul/reprezentantul sunt pe `Societate`, 73a) · D4-r11 partenerul cu două
coduri (SM + RO) · D4-r12 achizițiile de pe DEC fără furnizor · D4-r13 XML
D394 (35c; precedentul `SaftXml`) · 72-r1 rate-limit ANAF cross-request ·
72-r2 adresa hibridă 1C + ANAF fără proveniență per câmp · 72-r3
`IntrariStricate` = fatală după commit · 72-r4 radierea (`StareInregistrare`)
neconsumată · 72-r5 smoke vizual XAF al acțiunii ANAF · 72-r6
`RegistruContraAnaf` real abia la a doua rulare · 72-r7 ordinea la egalitate
în raportul de reconciliere · 72-r8 `CuiInterogabil("00")` · 72-r9 ecranul
React de partener · 72-r10 `User` ⇒ 404 vs 403 pe comanda ANAF (închisă de 80g) · 73-r2 `T`/`R`/nerezidenți ·
73-r3 segmentarea · 73-r4 nomenclatorul NC8 + corecturile `ml`/`pac`/TARIC ·
73-r5 `Produs.UM` string · 73-r6 ecranul React `Societate`, `CodNc`/UM pe
produs · 73-r7 OpANAF 1783/2021 (termene/praguri) · 73-r8 kitul DUK J2.2.8
vs J2.2.15, `-d` inutilizabil · 73-r9 `ContFaraRol` pe facturile de achiziție
Flax (date) · 73-r10 `Neincluse` întreg în sumar · 73-r11 `UnitPrice` la 2
zecimale, cantitatea negativă nemăsurată · 73-r12 64h (`PartenerulRandului`)
· 73-r13 `100030` ≠ `InactivFiscal` · 73-r14 conducătorul rupt la spațiu,
`InregistratTva` derivat · 73-r15 `CuiPrefixDublat` nemăsurat · 73-r16
multi-valută · 73-r17 categoria `op11` · 73-r18 contact/IBAN parteneri ·
73-r19 422 pe CUI gol/invalid nemăsurat pe HTTP
· 74-r1 `Owners`/`8038`, custodie · 74-r2 `ShipTo/From` pe BTR,
`MovementComments` · 74-r3 `InvoiceNo` duplicat în L (date 1C) ·
74-r5 soldul `Consum` neraportat (220 k / 340 k) ·
74-r7 `Neincluse` per
produs în sumar · 74-r8 stornoul S doar pe scenă ·
74-r10 `T`/segmentare pe S · 74-r11 D17-V6 doar privat,
proba ștergerii logice · 74-r12 ecran React `PoliticaMiscareSaft` · 74-r13
DUK J2.2.18 · 74-r14 gardian produs de stoc fără cont · 74-r15 `FaraCodNc`
pe Flax
· 75-r1 ASM UI: derivarea valorii produsului din consum · 75-r2 scalarea
rezoluției de tip (`CoduriTipPeTipuri` liniar cu baza; `ApiProiectii.
CoduriTip` pe hot-path API) · 75-r3 `--continua` fals-roșu pe bază importată
(idempotența doar prin re-rulare integrală) · 75-r4 `PretEvaluare` 6 zecimale
pe cantități mari vs invariantul 46d · 75-r5 stornoul fără timbru propriu în
oracolul golirii
· 76-r1 netarea plafonului e per (repartitor × LATURĂ), nu per CONT (55 chei /
36 note amestecă conturi de clasă 4 pe aceeași latură; expunere reală 3 chei) ·
76-r2 `caStins` se scade din AMBELE sensuri — inatacabil azi, **devine real când
un tip cu partener pe latură capătă capacitate bidirecțională** · 76-r3 perf
`AsignatFataDe` (entități polimorfe, chemat de 2 × nr. contrapartide) · 76-r4
gate-ul comenzilor e pe `Document`, nu pe tipul feliei (422 vs 404 pe aceeași
cauză; închisă de 80b) · 76-r5 `Candidati` sub-raportează pe ușa secured, iar `User` pe ușa de
scriere e refuzat de primul FK invizibil, nu de o permisiune (familia 72-r10;
închisă de 80b/80f) ·
76-r6 patru itemi de client în `lista-react.md`: căutarea sensibilă la
diacritice în TOATE lookup-urile remote (colație `unaccent`/ICU sau coloană
shadow — decizie de bază de date), `Lookup` care refetchează eticheta per
instanță, limita convenției 61b pe valorile din PRECOMPLETARE, `window.confirm`
moștenit pe ștergere (toți patru închiși de 77)
· 77-r1 BTR fără convenția 61b · 77-r2 `Cod`/`Denumire` neobligatorii pe nicio
ușă (închisă de 77k) · 77-r3 editarea `PoliticaMiscareSaft` din React, comanda
ANAF de lot ·
77-r4 `CodFiscal`/`Iban`/`Marca` în afara lui `Cautare` · 77-r5 precompletarea
nu distinge alegerea operatorului; invalidarea nu reîmprospătează
`SelectBox`-urile montate · 77-r6 `displayExpr` de nucleu pentru `TipMaterial`
· 77-r7 `Cautare` fără index (seq scan pe 20 k rânduri; cifra decide; calea:
GIN `pg_trgm`, 78e) · 77-r8
permisiunea pe OData `text/plain` pe server (închisă de 80)
· 78-r1 căutarea din grilele XAF rămâne sensibilă la diacritice (nu trec prin
`DataSourceLoader`; asumat, 44/53)
· 79-r1 acțiunea XAF „Generează închiderea" (aditivă) · 79-r2
`PoliticaInchidereTva` pe OData + ecran React (familia 77-r3) · 79-r3
închiderea perioadei fiscale din client (53i) · 79-r4 mesajul `[Range]` în
engleză pe `genereaza` (70-r5) · 79-r5 storno-ul unei închideri la o dată din
ALTĂ lună ⇒ previzualizarea lunii raportează `FaraSold` (cauza greșită; data
implicită din ecran e cea corectă) · 79-r6 cine are drept de citire pe
`InchidereTva` vede prin previzualizare soldurile de TVA ale societății fără
drept pe `RegistruContabil` (consecința asumată a lui 79b; închisă de 80e) ·
79-r7 „Verifică" activ pe ne-Draft arată refuzul ca eroare (convenția NTC,
transversală)
· 80-r1 pe conducta `ODataStore` a clientului (lookup-uri, `byKey`) refuzul
ajunge ca `statusText`, nu ca mesajul serverului (limita DevExtreme
`errorFromResponse`) · 80-r2 mesajul plasei DevExpress din `SaveChanges`
(permisiuni pe membru) iese 403 `EroriDto` cu text englezesc (70-r5) · 80-r3
`ReportController` (scaffold) păstrează `NotFound()` gol · 80-r4 refuzurile pe
`$expand` OData nemăsurate · 80-r5 captionul din mesajele 403 e numele CLR pe
WebApi (fără model de aplicație XAF în host; `Refuzuri.Caption` are calea) ·
80-r6 `400 "Incorrect body."` englezesc prin filtrul OData (70-r5) · 80-r7
`distribuie-valoarea` (ASM) întoarce sume din prețurile loturilor pe ușa
non-secured fără drept pe `Lot` (familia 79-r6; nu e sumă peste registru)
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
