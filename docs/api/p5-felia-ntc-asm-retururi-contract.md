# Pasul 5, felia 19 — NTC + ASM + retururi (RLF/RDC) prin API și client (contract)

Data fixării: 2026-08-29. Șablonul consolidat al feliilor F2–F8 (contractele
`p5-felia-fct-contract.md`, `p5-felia-nir-contract.md`,
`p5-felia-ldi-bcs-contract.md`, `p5-felia-dec-pereche-contract.md`).
Deciziile de mai jos (F19-D1…F19-D15) sunt PIN-UITE — agenții de implementare
nu le redeschid; orice nepotrivire cu realitatea codului se RAPORTEAZĂ, nu se
normalizează tăcut.

## Scop

Ultimele patru tipuri de document fără felie de scriere (roadmap: „feliile de
scriere rămase — NTC/ASM/retururi"):

- **NotaContabila (NTC)** — scriere + citire + comenzi + felie client. Include
  ROLUL DE STINGĂTOR (48b: compensarea — 869 documente/an în sursa 1C), adică
  panoul de stingeri pe un document cu contrapartide MULTIPLE.
- **Asamblare (ASM)** — idem, plus pasul de MODEL care închide 53i pe ASM
  (culegerea produsului care naște lotul), plus mecanismul care închide
  **75-r1** (derivarea valorii produsului din consum).
- **ReturFurnizor (RLF) + ReturClient (RDC)** — idem, cu TVA culeasă și cu
  semnarea storno lăsată OPERĂRII.

Motorul e deja probat pe toate patru: `nou/tools/ModelCheck/Program.cs` are
blocuri e2e pentru NTC (bugetar + privat), ASM, RLF și RDC, cu refuzurile lor.
Felia asta NU redeschide semantica de motor — pune ușa de API și ecranul peste
scene existente. Singurele atingeri de model sunt cele din F19-D3.

## Deciziile

### F19-D1 — Scope: NTC + ASM + RLF + RDC; ITV exclus

`InchidereTva` (ITV) rămâne în afara feliei: nu e document CULES, e rezultatul
unui serviciu (`InchidereTvaService`, 46c) — felia lui e o COMANDĂ cu
parametri (luna) și un ecran de rezultat, nu un agregat PUT header + linii.
Are contract propriu, la cerere. `RaportProductie` (BPR) rămâne REZERVAT
(decizia 19). Nimic din rețetar/alocare automată nu intră (46f).

### F19-D2 — Trei tracks independente, în ordinea NTC → ASM → RLF/RDC

Cele patru tipuri nu se ating între ele: politici disjuncte, DTO-uri disjuncte,
felii de client disjuncte. Ordinea e a riscului crescător de MODEL: NTC nu cere
nicio schimbare de model (F19-D5), ASM cere pasul indivizibil F19-D3, iar
retururile cer lanțul de TVA la culegere. **Regula de oprire**: fiecare track se
închide de sine — dacă unul se blochează, feliile închise rămân livrate și restul
devine restanță cu nume, nu se livrează pe jumătate.

### F19-D3 — Pasul de model ASM, indivizibil (închide 53i pe ASM)

Azi `Asamblare.ValideazaOperare` spune „Linia de produs își creează lotul la
culegere (alegeți produsul)", dar `AsamblareDetaliu` **nu are `ProdusId`** și nu
declară `ILinieCareNasteLot` — mesajul e neîndeplinibil din orice UI. Exact
situația LDI dinainte de F6-D2, și se rezolvă identic, în trei piese
indivizibile:

1. `AsamblareDetaliu` capătă `ProdusId` (Guid?) + navigația `Produs` și declară
   `ILinieCareNasteLot` (păstrând `ILinieCuAtributeLot`).
2. `NasteLot => Directie == DirectieAsamblare.Produs` — consumul descarcă un lot
   EXISTENT; un produs rămas cules pe o linie de consum ar naște lot-artefact pe
   draft (F6-D3).
3. `Asamblare.GestiuneLoturiCulese` → **PREDATORUL**. Default-ul hook-ului e
   primitorul; ASM validează `lot.GestiuneId == PredatorId` pe liniile de produs,
   iar laturile POT diferi („de regulă aceeași", nu obligatoriu) ⇒ fără override
   lotul se naște în gestiunea greșită și operarea refuză.

Migrația `AsmCulegereLot`, strict aditivă. Captions RO (`[XafDisplayName]`) pe
câmpurile ASM care ies azi brute în dump: `Directie`, `PretEvaluare`,
`DataExpirare`, `LotFabricatie`, + `Produs` nou.

Interdicția din `Interfete.cs` („nu se declară pe ieșiri") NU e atinsă: linia de
produs a ASM e o INTRARE de stoc, ca plusul de inventar.

### F19-D4 — ASM: valoarea produsului derivată din consum (închide 75-r1)

**Problema** (75-r1, F6 al review-ului advers F18): din F18-D2 consumul care
GOLEȘTE cheia preia tot soldul valoric rămas. Operatorul care evaluează produsul
la `preț lot × cantitate` primește refuz pe invariantul 46d cu un rest de cenți,
fără nicio cale de a nimeri cifra — ecranul devine capcană.

**Mecanismul**: comandă server-side pe documentul Draft —
`POST api/asm/{id}/distribuie-valoarea` (ușa non-secured, gate de autorizare
înainte, șablonul `OperareApi` 55b) — care rescrie `PretEvaluare` pe liniile de
produs astfel încât `Σ Valoare(produse) == Σ |Valoare(consumuri)|` EXACT.

Reguli pin-uite:

- **Predicția consumului trece prin ACELEAȘI funcții pure ale motorului**
  (`StocService.SolduriLaData` + `StocService.ValoareGolire`), niciun al doilea
  adevăr: comanda calculează exact ce va scrie `AplicaValoareIesire` la operare.
  ModelCheck probează egalitatea predicție == cifra operării pe scena cu lot golit
  cu rezidu (scena D18-V2: lot 3 × 30,02 ⇒ 10,006667).
- **Cheia de repartizare** pe mai multe linii de produs: proporțional cu valoarea
  CULEASĂ a fiecăreia (`RotunjesteBani(q × PretEvaluare)`); dacă niciuna n-are
  valoare culeasă, proporțional cu cantitatea. O singură linie de produs = tot
  consumul, fără repartizare.
- **Reziduul de ban** se plimbă pe linii: după `PretEvaluare = Round(țintă/q, 6)`
  (scara prețurilor, 49e) se recalculează valoarea realizată și diferența se
  aplică pe linia unde un pas de `1e-6` o poate absorbi.
- **Limita 75-r4 se DECLARĂ, nu se ascunde**: dacă niciun pas reprezentabil nu
  poate stinge reziduul (cantități mari — grila valorilor devine mai groasă decât
  banul), comanda REFUZĂ cu 422 și cu cifra reziduului, în loc să lase un ASM pe
  care operarea îl va refuza oricum. Zero apariții pe Flax.
- **Predicția e a MOMENTULUI**: dacă registrul se schimbă între distribuire și
  operare, invariantul 46d refuză cu ambele sume — comportamentul de azi, dar
  acum cu o cale de reparare (re-distribuie). Se documentează, nu se blochează.
- Idempotentă: a doua rulare pe același document dă aceleași cifre.

### F19-D5 — NTC și retururile: ZERO schimbări de model

`NotaContabilaDetaliu` are deja tot ce cere culegerea (Descriere, cele patru
FK-uri ale postării explicite, `CodEconomic`); RLF/RDC folosesc detaliul de BAZĂ,
fără câmp propriu. `Cont`, `Repartitor` (baza TPT), `CodEconomic`, `Lot`,
`Produs`, `TipMaterial`, `TipTva`, `Gestiune`, `Partener`, `UnitateInterna` sunt
DEJA expuse pe OData (opt-in-ul feliilor DEC/D300/SAF-T) ⇒ **felia asta nu adaugă
niciun set OData nou**. Orice nevoie descoperită de un set nou = raport, nu
adăugare tăcută.

### F19-D6 — `Numar` server-owned pe toate patru

NTC (ambele profiluri), ASM/RLF/RDC (profilul privat) au `PoliticaNumerotare` cu
seriile `NTC-`/`ASM-`/`RLF-`/`RDC-` ⇒ `Numar` NU intră în niciun WriteDto
(contractul BTR/NIR/FCL/TRZ/LDI/BCS; invers doar față de FCT, unde numărul e al
furnizorului). Consumat abia la materializare (53b) — ModelCheck probează că un
refuz nu consumă seria, pe fiecare tip.

### F19-D7 — TVA: doar pe retururi

- **NTC / ASM**: fără `PoliticaTva` în niciun profil ⇒ `TipTvaId`/`ValoareTva` NU
  intră în DTO-uri (precedentul F5-D5/F6-D5: cifră moartă). Baseline-ul XAF le
  ascunde deja pe ambele frunze — API-ul spune același lucru.
- **RLF / RDC** (privat): au `PoliticaTva` (RLF Deductibil/contrapartidă pe
  primitor; RDC Colectat/contrapartidă pe predator) și `TipTvaImplicit = N21`.
  `TipTvaId` + `ValoareTva` intră în DTO; implicitul se aplică DOAR pe linii NOI,
  recalculul DOAR pe declanșatori, override de `ValoareTva` doar pe regimurile cu
  TVA separat și nenegativ (56). Regimul `Capitalizat` e refuzat de tip — refuzul
  rămâne al motorului (dry-run), clientul nu-l duplică.
- **Linia de COST a RDC (cu lot) nu poartă TVA**: Apply o persistă cu
  `TipTvaId = null` și `ValoareTva = 0`, oglinda exactă a lui `PregatesteOperare`
  (fix-ul feliei 11). „Inert devine adevărat, nu doar afirmat" (F6-D3): golirea se
  PERSISTĂ, altfel `RegistruTva` ar scrie rândul la backfill.

### F19-D8 — Valoarea la culegere: `MaterializeazaValori` geamănă, per tip

Toate patru materializează la culegere formula GEAMĂNĂ a hook-ului
`PregatesteOperare` (precedentul F5-D6/F6-D6), rulată DUPĂ `Sincronizeaza`, pe
`OfType<frunză>()` unde există frunză, sărind `IsObjectToDelete`:

- **NTC**: nimic de materializat — `Valoare` e CULEASĂ direct (nu există lanț de
  valori). Negativul e PERMIS (note storno); zero e refuzat de tip.
- **ASM**: SEMNATĂ, ca LDI — Consum: `−RotunjesteBani(|q| × lot.PretUnitar)`
  (fără lot ⇒ 0); Produs: `+RotunjesteBani(|q| × (PretEvaluare ?? 0))`.
  `Cantitate` rămâne cum a fost culeasă (pozitivă) până la operare — semnarea
  cantității e a operării (28a).
  **Consecință folosită ca afordanță**: `Total`-ul draftului ASM e diferența
  invariantului — 0 ⇔ documentul e echilibrat (vezi F19-D9).
- **RLF**: POZITIVĂ pe draft — `RotunjesteBani(|q| × lot.PretUnitar)` + TVA prin
  `TvaService.CalculeazaLaCulegere` cu `pastreazaTvaCules`. Semnarea storno e a
  OPERĂRII (`PregatesteOperare` neatins): pe draft operatorul vede cifra de pe
  nota de credit a furnizorului, iar documentul operat o arată negativă.
  Marker-ul `IDocumentCuIesireFiscala` rămâne singurul adevăr al golirii —
  culegerea NU prezice și NU absoarbe rest (F18/F5).
- **RDC**: linia de VENIT — `Valoare` culeasă (venitul stornat) + TVA calculată,
  pozitive pe draft; linia de COST — `RotunjesteBani(|q| × lot.PretUnitar)`,
  pozitivă, `TipTva`/`ValoareTva` golite (F19-D7).

### F19-D9 — ReadDto și affordances

- Șablonul NIR: header plat cu denumirile laturilor, linii cu etichete per FK,
  `Stare`/`Directie` ca STRING pe sârmă (enum → nume; parse pe NUME la graniță,
  înaintea oricărui `CreateObject`).
- **Citirea per tip**: NTC și ASM au frunză ⇒ citire pe BAZA detaliului cu frunza
  prin `as`-cast (TPT LEFT JOIN, nullable explicit) + refuzul „linie de tip vechi"
  în reconciliere (există documente istorice de import). RLF/RDC nu au frunză ⇒
  citire directă pe `DocumentDetaliu` (șablonul BTR/BCS).
- Affordances: `PoateEdita`/`PoateOpera` din Draft; `PoateAnula`/`PoateStorna` =
  Operat + `!AreImperecheri` (predicatul e al BAZEI). Fără `Copii[]` — niciunul
  din cele patru nu generează conex sau secundar.
- **ASM**: ReadDto poartă `SumaConsum`, `SumaProdus` și `Diferenta`, calculate
  SERVER-SIDE. Clientul afișează invariantul fără să facă aritmetică (42c: „TS nu
  calculează niciodată"), și tot de acolo decide dacă butonul „Distribuie valoarea
  consumului" are rost.
- **RDC**: ReadDto poartă `Total` = DOAR liniile de venit (oglinda `Total` virtual
  + `LiniiCreanta`) și, separat, `TotalCost` — ca operatorul să nu citească „121"
  acolo unde documentul valorează „−121 creanță + −30 cost".
- `LotEticheta` prin `ApiProiectii.EtichetaLot` pe ASM/RLF/RDC.
- Cantitățile și valorile unui document OPERAT ies SEMNATE (fapta operării);
  documentul e oricum read-only.

### F19-D10 — NTC ca stingător: candidații se cer, nu se ghicesc (48b)

`NotaContabila.CapacitateStingere` există și e probată în motor: plafon per
contrapartidă = Σ valorilor absolute pe care repartitorul apare, NUMĂRATĂ PER
LATURĂ. Ce lipsește e ușa de citire: `DocumenteCuRest` filtrează pe **o singură**
contrapartidă (parametru `contrapartidaId`), fiindcă toate tipurile din uniune au
contrapartida pe LATURĂ. Nota o are pe LINII, și pot fi mai multe.

Pin: endpoint dedicat **`GET api/ntc/{id}/candidati`** care rezolvă
`CapacitateStingere` server-side și întoarce, per contrapartidă: denumirea,
capacitatea, cât e deja asignat și rândurile `DocumenteCuRest` filtrate pe ea
(refolosind proiecția existentă, fără a o rescrie). Clientul refolosește
`nucleu/PanouStingeri`, grupat pe contrapartidă.

**NTC NU se adaugă în `DocumenteCuRest`**: nota nu are semantică de „rest" — Σ
liniilor ei nu e o creanță și nici o datorie, e o postare. Excludere deliberată,
documentată acolo unde e documentată și cea a lui RDC.

### F19-D11 — Retururile NU devin stingători (46f rămâne, cu motiv)

`ReturClient` rămâne EXCLUS din `DocumenteCuRest` (motivul e deja scris în
`ImperecheriProiectii.cs`: `LiniiCreanta` e al TIPULUI, nu al coloanei — un
`GROUP BY` universal ar diverge tăcut de `ImperechereService.Total`). `RLF` NU se
adaugă nici el, prin simetrie: totalul lui e brutul negativ, iar un panou de rest
cu cifre negative ar minți despre ce se poate stinge.

Calea de lucru pentru compensarea unui retur cu factura originală rămâne **nota
contabilă** — exact ce livrează track-ul NTC al feliei (48b). 46f rămâne restanță
cu nume, acum cu motivul scris și cu o cale practică deschisă. Includerea RDC cere
`LiniiCreanta` polimorf în SQL: decizie proprie, nu improvizată aici.

### F19-D12 — Clientul: patru felii, convențiile cimentate

- **`ntc`** — editor de linie: `Descriere` + `ContDebit`/`ContCredit` (`Lookup` pe
  `Cont`, remote) + `RepartitorDebit`/`RepartitorCredit` (`Lookup` pe
  `Repartitor` — BAZA TPT, deliberat: postarea explicită acceptă orice repartitor)
  + `CodEconomic` + `Valoare` (negativ permis). Laturile = `Lookup` pe
  `UnitateInterna`, nefiltrat (F6-D8: autoritatea e motorul). Panoul de stingeri
  (F19-D10).
  **Obligatoriu din F19-D16** (altfel cazul ambiguu e fundătură pentru operator):
  panoul are un rând per (contrapartidă × SENS) — afișează `Sens` ca atare, cu
  plafonul, `Asignat`, `Disponibil` și candidații AI JUMĂTĂȚII — iar stingerea
  trimite `ContrapartidaId` EXPLICIT. Fără el, un document care poartă două
  contrapartide ale notei primește refuz de ambiguitate, corect dar orb: operatorul
  vede rândul sub un grup și serverul îi cere să spună sub care.
- **`asm`** — clona feliei LDI: comutator `CampSelectie` pe `DirectieAsamblare`;
  Produs ⇒ `Produs` (remote, `$expand=TipMaterial`, precompletare Tip) +
  `PretEvaluare` + `DataExpirare`/`LotFabricatie`; Consum ⇒ `Lot` (remote,
  `$expand=Produs`, sortare Data) + precompletare Tip din lot. Comutarea GOLEȘTE
  client-side câmpurile celeilalte direcții (comparație PRE-update) — oglinda
  golirii din Apply. Bara documentului: `Diferenta` din ReadDto + butonul
  „Distribuie valoarea consumului" (activ pe Draft cu cel puțin o linie de fiecare
  rol), confirmare inline, nu `window.confirm`.
- **`rlf`** — editor: `TipMaterial` + `Lot` (remote, `$expand=Produs`) +
  `Cantitate` + `TipTva` + `ValoareTva`. Laturi: predator `Gestiune`, primitor
  `Partener`.
- **`rdc`** — editor cu ROLUL liniei ca comutator explicit („Venit" / „Marfă
  returnată"); pe sârmă rolul rămâne ce e azi în model — prezența `LotId` —
  traducerea se face în editor, nu în model. Venit ⇒ `TipMaterial` (venit) +
  `Valoare` + `TipTva`/`ValoareTva`; Marfă ⇒ `TipMaterial` + `Lot` + `Cantitate`,
  fără TVA. Laturi: predator `Partener`, primitor `Gestiune`. Antetul arată `Total`
  (venit) și `TotalCost` separat.
- Înregistrare: rute `/ntc`, `/asm`, `/rlf`, `/rdc` (+ `/nou`, `/:id`), `NavLink`,
  `rutaTip` (+ 'NTC', 'ASM', 'RLF', 'RDC').
- Convențiile cimentate se respectă integral: garda `if (e.event)`, `laSelectie` =
  notificare + update FUNCȚIONAL, `String()` pe Guid-uri, formularul = sursa de
  adevăr, etichete nesalvate per POZIȚIE (61b), zero calcul de domeniu în TS.

### F19-D13 — Oglinda XAF (lecția F5/F6: gardul care tace devine capcană)

Declararea `ILinieCareNasteLot` pe ASM face ecranul XAF de asamblare cale VIE de
culegere (`DocumenteLoturiCulegereController` e generic pe `Document`). Oglinda în
`ContaUiBaseline`: coloana `Produs` intră în layout-ul frunzei ASM; `[Appearance]`
pe frunză comută editabilitatea pe direcție (Produs: `Lot` read-only; Consum:
`Produs`/`PretEvaluare`/atributele read-only). Ecranele XAF rămân funcționale, NU
product-grade (62g); smoke minimal la închidere.

### F19-D14 — ModelCheck: pe ce profil rulează fiecare bloc

Politicile decid, nu preferința: **NTC** are `PoliticaNumerotare` în AMBELE
profiluri (nota e neutră: fără stoc, fără contare, fără TVA) ⇒ blocul lui rulează
pe suita bugetară ȘI pe cea privată, ca blocul de motor existent. **ASM / RLF /
RDC** au politici DOAR în profilul privat ⇒ blocurile lor rulează pe suita PRIVATĂ
(pe bugetar ar fi cifre moarte).

Marcaje `E2E-API-NTC`, `E2E-API-ASM`, `E2E-API-RLF`, `E2E-API-RDC`; șablonul
feliei 5 (fixture + `DryRun` cu OS propriu + `CheckRefuza` + curățenie finală cu
assert). Ancorele obligatorii:

- **NTC**: Apply → Citeste/Lista; `Valoare` culeasă (inclusiv NEGATIVĂ)
  supraviețuiește round-trip-ului; refuzuri (latură `Partener`, cont lipsă,
  valoare 0, linie de tip BAZĂ) fără rânduri-fantomă; seria `NTC-` neconsumată la
  refuz; operare → note pe postarea EXPLICITĂ, fără nicio `RegulaContare`;
  `candidati` = plafon per contrapartidă identic cu `CapacitateStingere` (cusătură
  măsurată, nu afirmată) și rândurile filtrate == `DocumenteCuRest` pe acea
  contrapartidă; stingere prin panou → `Rest` scade; anulare refuzată cât există
  imperecheri; storno.
- **ASM**: **testul-ancoră** — linia de produs culeasă prin Apply (`ProdusId`)
  naște lotul pe linia proprie în gestiunea PREDATORULUI; PUT repetat NU naște al
  doilea lot; comutarea Produs→Consum prin PUT șterge lotul propriu nefinalizat și
  golește câmpurile produsului; consumul cu lot pinuit rămâne NEATINS (gardul de
  lot străin); `Valoare` semnată la culegere ⇒ `Total` draft = diferența
  invariantului; **`distribuie-valoarea` pe scena cu lot golit cu rezidu**:
  predicția == cifra pe care o scrie operarea, invariantul trece exact, a doua
  rulare e idempotentă, iar cazul nereprezentabil (75-r4, cantitate mare) dă 422 cu
  cifra; refuzurile existente ale tipului (laturi non-Gestiune, consum din lot
  frate, preț de evaluare ≤ 0, coerența Tip↔lot) prin `CheckRefuza`; operare →
  semnarea cantităților + lot finalizat cu `PretUnitar = PretEvaluare` + atribute
  copiate; anulare; storno.
- **RLF**: Apply → Citeste/Lista cu valori POZITIVE pe draft; TVA la culegere ==
  ce calculează `PregatesteOperare` înainte de semnare; operare → −q pe stoc, note
  `3xx = 401` cu −V și `4426 = 401` cu −TVA; **lotul golit rămâne cu rezidu pe el,
  401 nu-l primește** (`IDocumentCuIesireFiscala`, proba F18/F5 replicată pe calea
  API); refuzuri (laturi inversate, linie fără lot, regim Capitalizat, lot propriu)
  fără rânduri-fantomă; re-operare după anulare NU dublează semnul (idempotența
  `Abs`, probată prin Apply, nu doar prin motor); anulare; storno.
- **RDC**: Apply → Citeste/Lista; rolul liniei = `LotId`, iar un PUT care scoate
  `LotId` de pe o linie de cost e REFUZAT explicit (nu convertit tăcut în linie de
  venit); linia de cost persistată cu `TipTvaId = null` (probat în bază, nu doar în
  ReadDto); `Total` == doar liniile de venit; operare → +q pe stoc pe lotul
  original, note `4111 = 70x` / `4111 = 4427` / `607 = 371`, toate negative;
  refuzurile existente prin `CheckRefuza`; anulare; storno.

Verde pe AMBELE profiluri la fiecare pas (privatul prin `dotnet run privat`).

### F19-D16 — Plafonul de stingere capătă LATURĂ (amendament la 31d/48b)

**Descoperit de felie, la pasul 2, MĂSURAT** (`ModelCheck/Program.cs:16544-16550`):
`CapacitateStingere` întoarce `Dictionary<Guid, decimal>` — contrapartidă →
plafon, **fără latură** — iar `NotaContabila` adună în aceeași cheie repartitorul
de pe debit cu cel de pe credit. `ValideazaCreare` consumă din cheia aceea fără
să deosebească laturile. Consecința: o notă de compensare `401 = 4111` de 60 lei
pe partenerul X are plafon 120 și îl poate consuma INTEGRAL pe două încasări —
documente de ACEEAȘI natură. O compensare de 60 stinge 120.

Tavanul de 120 e CORECT (intenția scrisă în `NotaContabila.cs`: 60 pe datorie +
60 pe creanță = exact cele două stingeri legitime). Ce lipsește e regula că
fiecare jumătate se consumă pe latura ei.

> **CORECTAT de review-ul advers (F1), 2026-08-29 — propoziția de mai sus e
> FALSĂ pe jumătate.** Tavanul e corect doar când semnele nu se anulează.
> `CapacitateStingere` sumează `Σ |Valoare|` per (repartitor × sens), iar
> valoarea negativă doar RĂSTOARNĂ sensul — deci o pereche `+v` / `−v` pe
> ACEEAȘI latură a aceluiași repartitor produce `(Datorie v, Creanta v)` deși
> mișcarea NETĂ pe partener e ZERO. Adică exact „o compensare de 60 stinge 120",
> reprodus pe axa semnelor în loc de axa laturilor: prima versiune a lui
> F19-D16 a MUTAT defectul, nu l-a închis.
>
> Măsurat pe Flax 2025: din 1.478 chei (repartitor × latură) ale notelor, **908
> au brut > |net|** — 10.517.794,82 lei de capacitate fără nicio mișcare în
> spate — din care **899 au net exact 0,00**. Exemplu: `SED00000793` are două
> linii `4091 = 891` de `+1.311.593,79` și `−1.311.593,79` pe același
> repartitor ⇒ plafon `Datorie 2.623.187,58`, iar în bază există 841 de FCT ale
> acelui furnizor cu rest. Panoul le-ar oferi cu buton „Stinge", pentru o notă
> care n-a mișcat nimic.
>
> **Formula corectă: `|Σ semnat|` per (repartitor × latură)**, cu semnul netului
> care alege sensul — nu `Σ |v|` cu răsturnare per linie. Netarea subsumează
> tratarea liniei negative: nu mai e un caz special, e o consecință.

**Lecția de metodă, scrisă fiindcă a costat**: gardul de ne-regresie al primei
versiuni („toate verificările existente rămân verzi") a fost respectat literal și
a fost totuși insuficient — verde însemna „nimic din ce e ACOPERIT nu s-a rupt",
nu „comportamentul e identic". F2 al review-ului a găsit că trezoreria ÎȘI
SCHIMBĂ comportamentul (`PLT → FCL` și `INC → FCT` primesc acum 422), iar
ModelCheck n-a semnalat fiindcă n-avea niciun check pe acele perechi. Un
invariant de ne-regresie se măsoară pe SPAȚIUL de cazuri, nu pe suita existentă.

Defectul e PREEXISTENT — atins azi prin XAF și prin Import1C (compensarea din
1C, 869/an), nu îl introduce felia. Se repară AICI fiindcă felia e cea care pune
panoul de compensare în mâna unui operator: un ecran de compensare peste o gaură
contabilă cunoscută nu se livrează.

Pin-uri:

- **Plafonul devine per (contrapartidă × SENS)**, iar consumul alege sensul după
  natura documentului stins. Generalizarea e naturală și acoperă tot ce există
  azi: plata stinge datorii (sens debitor), încasarea stinge creanțe (sens
  creditor), nota le poate face pe amândouă — repartitorul de pe debitul liniei
  intră pe un sens, cel de pe credit pe celălalt.
- **Mecanismul rămâne CONTRACT, nu `is`/`switch`** (invariantul II): sensul
  acceptat de documentul stins se declară polimorf, ca `CapacitateStingere` și
  `PoateFiStins`. Agentul alege forma exactă a hook-ului și o RAPORTEAZĂ; ce nu
  are voie e să pună cunoașterea tipurilor în motor.
- **Invariantul de ne-regresie, cel mai tare gard**: pentru documentele cu o
  SINGURĂ contrapartidă (toată trezoreria + DEC) comportamentul trebuie să rămână
  IDENTIC — toate verificările existente de imperechere rămân verzi, neatinse și
  nerescrise. Dacă o verificare existentă trebuie schimbată ca să treacă, e semn
  că fixul a schimbat altceva decât trebuia: se RAPORTEAZĂ, nu se ajustează
  testul.
- **Proba proprie**: nota de 60 pe X nu mai poate stinge 120 de aceeași natură;
  poate încă stinge 60 de datorie + 60 de creanță (cazul legitim rămâne).
- **A doua axă**, citită din cod la pasul 2 și NEMĂSURATĂ atunci:
  `ValideazaCreare` alege singur contrapartida (primul key care se potrivește cu
  o latură a documentului stins), iar cheile se umplu debit-întâi ⇒ un document
  care poartă DOUĂ dintre contrapartidele notei pe cele două laturi ale lui ar fi
  taxat pe plafonul altui grup decât cel sub care panoul l-a afișat. Se MĂSOARĂ
  întâi (check dedicat); dacă se confirmă, fixul intră în același pas — selecția
  contrapartidei devine explicită (cerută de apelant sau dedusă fără ambiguitate),
  nu „primul key".
- Panoul `candidati` urmează plafonul nou, tot prin partajarea funcțiilor
  motorului (F19-D10) — nicio formulă rescrisă în felia de citire.

**Adăugat după review (F19-D16 revizuită)**, pin-uri noi:

- **`PLT → FCL` și `INC → FCT` se REFUZĂ de acum, DECLARAT.** Verdictul e corect
  contabil (a credita 401 nu stinge o factură de furnizor; a debita 4111 nu
  stinge una de client) — vechea permisivitate era o gaură din aceeași familie.
  Nu mai pretindem că trezoreria e neatinsă: pe Flax cade exact **1 imperechere
  / 700,00 lei** din 46.056. Se adaugă acoperire pe SPAȚIUL de perechi
  (stingător × stins), nu doar pe cazurile fericite.
- **Panourile CLASICE urmează și ele sensul** (F3 al review-ului): azi
  trezoreria/FCT/FCL/DEC filtrează candidații pe TIP, ceea ce n-are legătură cu
  sensul, iar `DocumenteCuRestController` nici nu expune parametrul `sens` pe
  care proiecția l-a primit. Măsurat: **87 din 353 de contrapartide au documente
  pe AMBELE sensuri** (≈25 %) ⇒ un panou de Încasare oferă zeci de FCT cu buton
  „Stinge" care duc garantat la 422. Sensul cerut vine din ReadDto-ul
  documentului (server-computed), NU dintr-o deducție în TS.
- **Refuzul de ambiguitate primește ieșire** (F4): azi, un document stins care nu
  declară sens, în fața unui stingător cu ambele jumătăți, primește un refuz pe
  care apelantul nu-l poate rezolva pe NICIO cale. Ieșirea preferată e
  MODELAREA, nu o portiță: tipurile care chiar pot fi stinse își declară sensul
  (**`NIR` = `Datorie`** — prin 26a recepția contează pe NIR, deci NIR-ul lasă
  401 creditor față de furnizorul care e chiar predatorul lui). Un câmp `Sens` în
  DTO care lasă apelantul să aleagă jumătatea unui document fără natură declarată
  ar fi exact arbitrarul pe care refuzul există ca să-l oprească.

### F19-D15 — Ce NU intră în felie

- ITV (F19-D1).
- Generarea automată a liniilor de cost RDC din pin-urile liniilor de venit
  (amânată explicit în FAZA 1C §7; importul aduce ambele feluri direct).
- Alocarea ASM pe rețetar (BPR, decizia 19).
- Retururile ca stingători (F19-D11).
- Filtrarea laturilor interne pe `Calitati` (limitarea F6-D8, neschimbată).

## Pașii de implementare (un agent per pas, secvențial)

1. **Pas 1 — Modelul ASM** (Module): F19-D3 + oglinda XAF F19-D13 + captions +
   migrația `AsmCulegereLot` + `--dump-metadata`. Verificare: build,
   `database update` (inclusiv bazele de dev suplimentare — capcana F6), ModelCheck
   AMBELE profiluri verde, inclusiv blocul e2e ASM existent neatins.
2. **Pas 2 — API NTC** (Module `Api\Ntc\` + WebApi): Dtos + Apply + controller +
   `candidati` (F19-D10), blocurile `E2E-API-NTC` pe ambele profiluri, openapi
   regenerat + drift verde.
3. **Pas 3 — API ASM** (Module `Api\Asm\` + WebApi): Dtos + Apply + controller +
   comanda `distribuie-valoarea` (F19-D4), blocurile `E2E-API-ASM` (privat),
   openapi + drift.
4. **Pas 4 — API RLF + RDC** (Module `Api\Rlf\`, `Api\Rdc\` + WebApi): Dtos +
   Apply + controllere, blocurile `E2E-API-RLF` / `E2E-API-RDC` (privat), openapi +
   drift.
4bis. **Pas 4bis — Plafonul de stingere cu latură** (Module Motor + Api\Ntc):
   F19-D16. Întâi MĂSURAREA celei de-a doua axe (check dedicat), apoi fixul
   plafonului per (contrapartidă × sens) + hook-ul polimorf + probele. Toate
   verificările existente de imperechere rămân verzi și NERESCRISE.
5. **Pas 5 — Clientul NTC + ASM** (Atlas.Conta.Client): feliile `ntc` (cu panoul de
   stingeri) + `asm` (comutator + distribuire), rute/meniu/`rutaTip`, `gen:types`,
   build client verde.
6. **Pas 6 — Clientul RLF + RDC**: feliile `rlf` + `rdc`, rute/meniu/`rutaTip`,
   `gen:types`, build client verde.
7. **Review advers dedicat** (agent separat, scenarii concrete de exploatare);
   fix-urile le aplică main-ul.
7bis. **Gate de închidere — Import1C pe Flax** (obligatoriu din cauza lui
   F19-D16). Regula proiectului: schimbările de motor/registre au ca probă supremă
   re-rularea integrală Import1C cu raport comparat cu baseline-ul (precedentul
   DIM-4/F18). Aici ținta e precisă: sensul poate refuza acum imperecheri pe care
   sursa le implica — **869 compensări/an**. Prin 48b astea cad în RAPORTUL de
   imperecheri, nu ca stop, deci criteriul nu e „zero diferențe", ci **fiecare
   diferență explicată per cauză**. Se rulează pe binarul construit din codul
   FINAL (lecția F18: prima rulare a mers pe binar pre-fix), detașat, cu monitor.
   Sondă ieftină acceptabilă ÎNAINTE, ca semnal timpuriu: câte dintre
   imperecherile existente în Flax ar viola regula nouă de sens — minute, nu ore.
8. **Smoke browser** pe perechea WebApi + client (baza Privat), fluxurile-ancoră:
   NTC cules → operat → note pe postarea explicită → compensare prin panou; ASM cu
   lot golit → „Distribuie valoarea consumului" → operat → lot nou finalizat; RLF
   cules → operat → valori negative + rezidu rămas pe lot; RDC cu ambele feluri de
   linii → operat → `Total` = doar venitul.

Comenzile de verificare (main le re-rulează independent la fiecare pas):

```
cd nou/Atlas.Conta.BackOffice/Atlas.Conta.BackOffice.Module
dotnet ef migrations add AsmCulegereLot --context BackOfficeEFCoreDbContext   # doar pasul 1
dotnet ef database update --context BackOfficeEFCoreDbContext                  # înainte de ModelCheck bugetar
dotnet ef database update --context BackOfficeEFCoreDbContext --connection "...Database=Atlas.Conta.BackOffice.Privat"
cd nou/tools/ModelCheck && dotnet run                    # bugetar (pe baza aplicației)
cd nou/tools/ModelCheck && dotnet run privat             # baza dedicată
dotnet run --project nou/tools/ModelCheck -- --dump-metadata                   # la orice caption nou
cd nou/Atlas.Conta.Client && pnpm verifica:drift                               # pasul 2+
cd nou/Atlas.Conta.Client && pnpm build                                        # pașii 5-6
```

Niciodată `--no-build` la `dotnet ef` (incident cunoscut). `verifica:drift` cere
WebApi OPRIT (memoria „verifica-drift-webapi-oprit").

## Riscurile pin-uite (review-ul advers le țintește)

1. **NTC e cea mai puternică ușă de scriere din sistem**:
   `IDocumentCuPostareExplicita` face ca o linie cu postare COMPLETĂ să posteze în
   absența oricărei reguli. Gate-ul de autorizare (55b), `GardianEditare` și
   `Cont.DimensiuniObligatorii` sunt tot ce stă între API și note arbitrare —
   verifică fiecare, pe HTTP (66h), inclusiv că `User` nu poate scrie NTC.
2. **NTC `candidati`**: plafonul per contrapartidă e numărat PER LATURĂ (401=4111
   pe același partener dă capacitate pe ambele) — panoul poate propune o stingere
   pe care `ImperechereService` o refuză? Măsoară cusătura, nu o afirma.
3. **ASM `NasteLot == false`** pe liniile de consum: poziția față de self-healing și
   de gardul de lot străin; comutarea de direcție cu lot propriu FINALIZAT (operare
   → anulare → comutare) — ce rămâne în urmă (precedentul M3 al F6).
   Tot aici: **coerența Tip↔Produs la NAȘTERE** pe linia de produs. LDI a primit
   un gard explicit la review-ul lui (F6-F2); ASM o acoperă azi doar TRANZITIV,
   prin lotul deja născut (`tipPerLot` în `ValideazaOperare`) — deci lotul se naște
   întâi și abia operarea refuză. Măsoară dacă asta lasă vreo urmă (lot orfan pe
   draft) și dacă gardul explicit al LDI-ului lipsește cu adevărat.
4. **`distribuie-valoarea`**: predicția vs realitatea operării (registrul se
   schimbă între timp; alt document golește lotul primul); reziduul plimbat pe
   linii; cantități mari (75-r4); documentul cu ZERO linii de consum sau zero linii
   de produs; consumuri fără lot; re-rularea după o operare eșuată.
5. **RDC: rolul liniei e o PREZENȚĂ, nu un enum** — orice cale prin care un PUT
   mută o linie dintr-un rol în celălalt (adaugă/scoate `LotId`) trebuie să fie
   refuz explicit sau conversie COMPLETĂ (inclusiv TVA și natura Tipului), nu
   jumătate.
6. **Idempotența semnării** pe RLF/RDC prin calea API: culegere → operare →
   anulare → PUT → re-operare; `Abs`-urile din `PregatesteOperare` țin, dar
   `MaterializeazaValori` scrie și el peste — cine bate pe cine.
7. **Seria consumată / rânduri-fantomă la refuz** (33d) pe toate căile noi.
8. **RLF și golirea fiscală**: calea API nu are voie să introducă un al doilea
   adevăr despre valoarea ieșirii (F18/F5) — culegerea prezice `q × preț`, motorul
   nu absoarbe restul, iar reziduul rămâne raportat în SAF-T S.
9. **Liniile istorice de import** (NTC și ASM au documente de import în bazele
   reale): citire, reconciliere, self-healing — o linie veche nu se strică la PUT.

## Anexa — șablonul concret (măsurat, nu presupus)

Forma de mai jos e ce EXISTĂ azi în felia DEC (cea mai recentă) și în BCS; agenții
o replică, nu o reinventează. Diferă doar mapările proprii tipului.

- **Apply** (`Module\Api\<Tip>\<Tip>Apply.cs`, static, zero ASP.NET, cu
  comentariul „CONTRACT DE APELANT"): `Aplica(IObjectSpace os, Guid? id, WriteDto)
  : Guid`, `Sterge(os, id)`, `Citeste(os, id) : ReadDto`, `Lista(os) :
  IQueryable<ListDto>`. Rulează în OS-ul SECURED al apelantului și comite;
  gardianul de Committing rămâne ultima autoritate; pre-check de Draft ca refuzul
  să fie de domeniu. Precedent: `Api\Dec\DecontApply.cs`.
- **DTO-uri** (`<Tip>Dtos.cs`): Write (header + linii), Read (header + linii cu
  etichete + affordances), List.
- **Controller** (`WebApi\API\Conta\<Tip>Controller.cs`, `[Route("api/<cod>")]`,
  extinde `ContaApiController`): `GET /`, `GET /{id}`, `POST /`, `PUT /{id}`,
  `DELETE /{id}` prin `Domeniu(...)` pe OS secured; `POST /{id}/opereaza`,
  `/anuleaza`, `/storneaza`, `/valideaza` prin `Comanda(...)` pe OS non-secured.
  Comenzile proprii feliei (`/{id}/distribuie-valoarea` pe ASM,
  `GET /{id}/candidati` pe NTC) intră pe aceleași uși: comanda pe non-secured,
  citirea pe secured. Precedent: `DecontController.cs` (99 linii).
- **ModelCheck**: totul în `nou/tools/ModelCheck/Program.cs` (un singur fișier,
  ~16 k linii) — `const string MarcajApiXxx = "E2E-API-XXX"`, `CurataApiXxx(os)`
  cu purjă FIZICĂ (`Purja`), seed de scenă cu prefix, `DryRunXxx(id)` pe OS
  propriu, `SerieXxx()` care citește `UrmatorulNumar`, apoi
  Apply → Citeste → Opereaza → Anuleaza → Storneaza → Sterge, curățenie finală cu
  assert. Precedente: blocul DEC (`Program.cs:7049-7421`), blocul BCS
  (`6316-6539`).
- **Client**: `src/felii/<cod>/` cu `api.ts` (tipuri din codegen +
  `ia/posteaza/pune/sterge` din `nucleu/http.ts`, `storeRemote` din
  `nucleu/dxStore.ts`), `<Cod>EditorLinie.tsx`, `<Cod>Detaliu.tsx` (pe
  `nucleu/DocumentShell.tsx`), `<Cod>Lista.tsx`. Înregistrare în `App.tsx`:
  import, ruta statică ÎNAINTEA celei parametrice (`/x`, `/x/nou`, `/x/:id`),
  `NavLink`. **`rutaTip()` trăiește în `nucleu/stingeri.ts:45-59`** — azi NTC,
  ASM și RDC cad pe `default: return null` (text, nu link mort); fiecare track își
  adaugă codul acolo când felia lui devine reală.
- **Comutatorul de direcție**: `felii/ldi/LdiEditorLinie.tsx` — câmpurile
  celeilalte direcții se golesc client-side ȘI nu se randează deloc; validarea
  structurală e condiționată de direcție (`cerut('ProdusId', plus)`).
- **`Lookup` pe `Cont`** există deja (`felii/dec/DecEditorLinie.tsx:142,151`) —
  NTC îl refolosește, nu îl rescrie.
- **`ApiProiectii`** (119 linii) oferă `EtichetaLot`, `Copii`, `CoduriTip`,
  `CodTip`, `AreImperecheri`. Niciunul din cele patru tipuri nu are încă proiecție
  proprie.
- **`PoliticaMiscareSaft`** are deja rânduri pentru ASM (4), RLF (2, `Furnizor`) și
  RDC (2, `Client`); NTC n-are, și nici nu-i trebuie (fără stoc). Felia NU atinge
  politicile SAF-T.

## Închidere

### Gate 7bis — Import1C pe Flax: TRECUT (2026-08-29)

Re-rulare integrală `--recreeaza` pe binarul construit din codul FINAL (13:44 →
15:54, 2h10, exit 0, stderr gol): **CONTRACT ÎNDEPLINIT**, 0 contracte picate
(4 contracte × 12 luni), 932 avertismente, 0 eșecuri pe fiecare lună.

Comparat cu proba FINALĂ a feliei F18 (`nou/tools/Import1C/run-f18/import.log`,
28.08 23:32 → 29.08 01:25 — *gitignored, de aceea invizibilă la o căutare sub
`docs/`; primul baseline folosit a fost o rulare din 25/26.08, dinaintea
fix-urilor F18, iar cele trei „delte" pe care le producea au dispărut la
compararea corectă*):

| control | F18 final | F19 |
|---|---|---|
| REZULTAT | CONTRACT ÎNDEPLINIT, 932 avertismente | **identic** |
| Documente 2025 | 187.372 / 190 sărite / 17.814 copii / 0 eșecuri / 3.597 realocări | **identic, cifră cu cifră** |
| Punți NTC, închiderea, CMP, perechi în sursă (52.039) | — | **identice** |
| Imperecheri create | 46.056 | **44.448** |

Tot ce nu ține de imperecheri e IDENTIC — inclusiv cele 932 de avertismente, care
sunt gardul cel mai sensibil: dacă felia ar fi atins ceva colateral, cifra s-ar fi
mișcat.

**Diferența de 1.608 imperecheri, atribuită integral și verificată ca CORECȚIE**:

- Conservarea închide pe ambele rulări: `46.056 + 5.983 = 44.448 + 7.591 = 52.039`
  perechi în sursă. Nimic pierdut tăcut — imperecherile s-au mutat din „create" în
  „sărite", cu categoria scrisă.
- Cauza dominantă e **netarea (F1)**, nu regula de sens: NTC ca stingător scade de
  la 1.687 la 79. Din 1.478 chei de plafon, **899 (867 note) aveau net EXACT 0** —
  10.499.837,16 lei de plafon fantomă; doar 9 chei (7 note) sunt „micșorate, nu
  anulate", și acelea nu stingeau nimic nici înainte.
- Notele care și-au pierdut capacitatea sunt contabil VACUE: `±v` pe același cont,
  același partener, aceeași latură, contra hub-ului tehnic `891`. Compensarea reală
  (parteneri diferiți) își păstrează plafonul și continuă să stingă. Prin 45e,
  divergențele sursei se RAPORTEAZĂ — nu se forțează o capacitate care nu există.
- Axa sensului, cum prezisese review-ul: `PLT → FCL` **0 rămase** (era 1, 700,00
  lei); `INC → FCT` 0.
- Depășiri de plafon (`asignat > plafon`): **0 chei**, de la 135 / 59.853,65 lei pe
  formula nenetată.
- Categoria „ținta nu poartă contrapartida pe latură" crește cu 1.974 — aceeași
  cauză: nota fără capacitate cade mai devreme, în altă categorie. Nu e o a treia
  cauză.

### Restanțe cu nume, deschise de felie

- **F19-r1 — netarea e per (repartitor × LATURA liniei), nu per CONT.** Măsurat pe
  Flax: 55 de chei / 36 de note amestecă pe aceeași latură conturi diferite
  (`4111+473`, `401+404`, `421+423`, `4111+419`, `461+473`, `401+473`) — 394.551,89
  brut → 25.383,45 net, 52 din 55 anulate oricum. Toate din clasa 4. Expunerea reală
  e de 3 chei; rafinarea la cheia cu cont se face cu cifră, nu preventiv (59).
- **F19-r2** — `AsignatFataDe`: `caStins` se scade din AMBELE sensuri. Inatacabil azi
  (singurul tip cu două jumătăți e NTC, ale cărui laturi sunt unități interne, deci
  nu poate fi stins). **Devine real în clipa în care un tip cu partener pe latură
  capătă capacitate bidirecțională** — condiția e scrisă ca să fie recunoscută.
- **F19-r3** — perf: `AsignatFataDe` materializează entități polimorfe (TPT), iar
  `Candidati` îl cheamă de `2 × nr. contrapartide` ori. Nemăsurat; se atinge doar cu
  cifră (59).
- **F19-r4** — gate-ul comenzilor e pe `Document`, nu pe tipul feliei
  (`ComandaAutorizata<Document>`): rutele nu sunt tipizate, iar `422` vs `404` diferă
  pe aceeași cauză. Preexistent, identic în toate feliile.
- **F19-r5** — `Candidati` pe ușa SECURED sub-raportează tăcut (un utilizator fără
  drept de citire pe liniile notei vede panoul gol, nu un refuz).
- **F19-r6** — `User` primit de ușa de scriere e refuzat de primul FK invizibil
  („nu există în nomenclatorul de repartitori"), nu de o verificare de permisiune:
  închis în fapt, dar mesajul spune „nu există" unde adevărul e „n-ai voie să vezi".
  Aceeași familie cu 72-r10, acum pe calea de scriere.

### Pasul 4bis — plafonul cu latură (F19-D16)

**A doua axă: MĂSURATĂ și CONFIRMATĂ.** Ipoteza citită din cod la pasul 2 e
reală. Scenă (bloc NTC, ModelCheck): o notă cu două linii de debit — casa 60,00
și partenerul X 5,00 — plus încasarea `incX1` (predator X, primitor casa), care
poartă AMÂNDOUĂ contrapartidele. Regula veche („primul key din `capacitati` care
se potrivește cu o latură", chei umplute debit-întâi, în ordinea liniilor) alege
CASA, cu plafon 60,00, deși `DocumenteCuRest` pune încasarea EXCLUSIV sub
partenerul X, cu disponibil 5,00: o stingere de 50,00 trecea pe plafonul altui
grup decât cel afișat de panou. Configurația e realizabilă fiindcă
`CapacitateStingere` ia ORICE repartitor de pe linie, nu doar parteneri.
Fixul a intrat în același pas: `ValideazaCreare` primește un
`contrapartidaId` OPȚIONAL (alegerea explicită a apelantului — panoul știe sub
ce grup a afișat candidatul), iar deducția refuză AMBIGUITATEA în loc s-o
rezolve tăcut.

**Forma hook-ului.** Al treilea hook polimorf al rolului, lângă
`CapacitateStingere` și `PoateFiStins`:
`Document.SensDeStins(os) → SensStingere?` — ce fel de sold poartă documentul pe
contul contrapartidei, deci din ce jumătate de plafon se stinge. `null` =
tipul nu declară; motorul atunci NU ghicește: dacă stingătorul oferă exact un
sens față de contrapartida aleasă comportamentul e identic cu cel dinainte
(toată trezoreria), dacă oferă două se refuză. Plafonul rămâne cheiat pe
contrapartidă (`IReadOnlyDictionary<Guid, PlafonStingere>`), cu valoarea
defalcată pe sens — nu pe cheie compusă: apelanții care întreabă doar „apare
contrapartida asta?" rămân neatinși. `AsignatFataDe` capătă parametrul `sens`;
un document stins care nu declară se scade din AMBELE sensuri (conservator).
`DocumentCuRestRand.Sens` = literal per ramură a uniunii, ca `Tip`, cu check de
consistență contra hook-ului pe fiecare rând (42c).

Sensurile declarate: FCT/DEC/INC/RDC = `Datorie` (sold creditor pe
contrapartidă, se stinge debitând); FCL/PLT/RLF = `Creanta`. Trezoreria le
declară o singură dată (`SensPropriu`), iar plafonul ei e pe sensul OPUS —
plata debitează 401 (stinge datorii) dar rămâne ea însăși un avans = creanță.

Panoul `candidati` are acum un rând per (contrapartidă × SENS), cu candidații
filtrați pe amândouă: altfel ar promite capacitate dublă și ar propune facturi
de client sub jumătatea de datorie.

**Ne-regresie**: nicio verificare existentă de imperechere n-a fost rescrisă
(motor, trezorerie, E2E-CMP, virament, decont). ModelCheck 857 bugetar / 757
privat, 0 FAIL; drift exit 0.
