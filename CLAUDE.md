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
- **3c. Tipurile de document, în ordinea dependențelor**: NIR + FacturaIntrare
  (conex + creare loturi) → BonConsum → ListaDiferenteInventar (bidirecțional)
  → FacturaIesire → Plata/Incasare + Imperechere → Decont. Per tip: politici
  seed + validare declarativă + test de operare.
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
