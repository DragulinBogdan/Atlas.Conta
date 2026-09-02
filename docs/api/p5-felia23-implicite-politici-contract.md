# Pasul 5, felia 23 — implicitele de culegere și întreținerea politicilor (contract)

Data: 2026-09-02. Stare: deschisă. Pleacă din discuția de arhitectură
„extindere în cod vs. în date; valorile implicite" (2026-09-02) și din
restanțele 77-r3 (editarea politicilor din React), 79-r2 (`PoliticaInchidereTva`
pe OData + ecran), 74-r12 (ecran `PoliticaMiscareSaft`). Decizia rezultată: 081
(se scrie la închidere).

## Scop

Două lucruri care azi lipsesc, legate prin aceeași ușă:

| Zona | Azi | Problema |
|---|---|---|
| Implicitele de culegere | UN singur mecanism pe o singură axă: `TipDocument.TipTvaImplicit`, aplicat la culegere pe ambele uși (`TvaService.AplicaTipTvaImplicit`; XAF `DefaultTipTvaController`, API în cele 5 Apply-uri FCT/FCL/DEC/RLF/RDC) | implicitul care depinde de COMBINAȚIA tip × partener × produs nu există: o livrare intracomunitară primește N21, un produs cu cotă redusă primește N21, un partener cu regim propriu primește N21 — operatorul corectează de mână, de fiecare dată |
| Politicile pe OData | `ReadOnly` pe TOATE (`WebApi/Startup.cs:95-100`: „ar fi politică editată pe ușa din dos, în afara oricărei validări de profil") | argumentul a expirat la 77g/77k/80c: gardianul stă pe ușa COMUNĂ. Ce rămâne adevărat e că regulile XAF Validation nu rulează pe API (55b) — deci fiecare politică deschisă cere invarianții ei în `GardianEditare` |
| Unicitatea politicilor | nouă politici per tip de document (`PoliticaTva`, `Conex`, `Scadenta`, `Validare`, `Numerotare`, `InchidereTva`, `RegulaStoc`, `RegulaContare`, `TipDocument`) FĂRĂ index unic; motorul le citește cu `FirstOrDefault` (`MotorOperare.cs:226/400/458/808/825`, `InchidereTvaService.cs:181`, `TvaService.cs:116`) | un al doilea rând pe același tip, creabil azi din XAF, face motorul NEDETERMINIST, tăcut; nici codurile nomenclatoarelor (`TipTva.Cod`, `TipDocument.Cod`, `Cont.Simbol`, `ClasaProdus.Cod`, `TipMaterial.Cod`) nu sunt unice, deși seed-ul le tratează ca chei și `Cautare.cs:15` afirmă că indexurile există |
| Proveniența rândurilor | patru semantici de re-seed (rescrie tot / creează gol / insert dacă lipsește / insert + respectă ștergerea logică), NICIUN marcaj seed vs. manual | un rând seed-uit și editat de client rămâne divergent pentru totdeauna, invizibil; un ecran de editare amplifică exact asta |
| Gardianul pe politici | din 12 politici, DOAR `PoliticaMiscareSaft` are ramură în `Verifica`; `MapareD300`/`D394` au regulile ca atribute XAF (nu rulează pe API); `RegulaContare` are `SursaCont.Explicit ⇒ ContId` reparat post-factum de seed (`StergeReguliContareStricate`) | pe OData un rând invalid trece |
| Auditul pe OData | `AddAuditTrailEFCore` + `WithAuditedDbContext` pe providerul securizat al WebApi (prin construcție acoperă și OData); probat empiric DOAR pe calea motorului (spike 1) | nemăsurat pe scrierile OData; `AuditDataItemPersistent` nu e expus, deci niciun ecran nu poate arăta „cine și când" |

Mecanica de dedesubt, măsurată pe surse:

- `TipTva` = cotă × regim (`Cota`, `Regim: RegimTva`) — cele două jumătăți ale
  unui implicit de TVA vin din SURSE diferite: regimul e al partenerului
  (înregistrat / RO neînregistrat / UE / extra-UE), cota e a produsului, tipul
  de document dă doar sensul și fallback-ul.
- Clasa fiscală a partenerului EXISTĂ ca funcție a legii:
  `D394Proiectii.TipPartener(tipPersoana, tara, inregistratTva)` ⇒ 1..4
  („înregistrat bate tot", 71b) peste `TariUe.Coduri`. Se refolosește, nu se
  reinventează.
- Postgres pe host-ul de dev e 18.x ⇒ indexurile unice pe chei cu coloane
  nullable se scriu cu `NULLS NOT DISTINCT` (EF/Npgsql: `AreNullsDistinct(false)`),
  un singur index în loc de perechea din 74a.
- `GardianEditare.Verifica` e `public static`, chemat direct de ModelCheck pe OS
  fără securitate — regulile noi de politică se probează acolo, fără host.
- Clientul are deja șablonul de nomenclator (`felii/nomenclatoare`, 77h):
  `ShellNomenclator`, `ListaNomenclator` peste `storeOData`, scrierea prin
  `api.ts` (`creeazaRand`/`modificaRand`/`stergeRand` ⇒ `http.ts` ⇒ mesajele
  `EroriDto`), PATCH = DELTĂ.

Felia adaugă UN serviciu de implicite cu o singură sursă, DESCHIDE politicile pe
OData cu invarianții în gardian, pune unicitatea în schemă, marchează
proveniența, și dă clientului o grilă comună de politică plus raportul de
verificare a profilului. Zero schimbări de motor (potrivirea regulilor nu se
atinge); schimbări de schemă ADITIVE plus indexuri.

## Deciziile

### F23-D1 — Trei feluri de implicite, o singură sursă, aplicate la CULEGERE

- **Implicit de SUBIECT** = câmp pe nomenclator, ca `ContImplicit` (precedent 26b/31a):
  `Partener.TipTvaImplicitId?` (regimul propriu al partenerului — override
  explicit) și `Produs.TipTvaImplicitId?` (cota proprie a produsului).
- **Implicit de POLITICĂ** = tabel tipizat cu cheie de potrivire și prioritate
  DECLARATĂ, pe modelul lui `RegulaContare`: `PoliticaTvaImplicit` (D2).
- **Implicit de SESIUNE** (data de azi, ultima gestiune) rămâne al clientului
  (URL / stocare locală, 43c) — NU intră în felie.
- Aplicarea rămâne la CULEGERE, nu în motor (datoria P1, 38d): o singură dată,
  pe linia NOUĂ fără `TipTva` în payload; pe linia existentă absența e golire
  deliberată (56, round-trip). Nimic din 56 nu se redeschide.
- O SINGURĂ sursă în Module: `Motor/ImpliciteService` (static, pe FK-uri +
  `IObjectSpace`, 25b). Trei apelanți: controllerul XAF
  (`DefaultTipTvaController`), cele cinci Apply-uri (prin
  `TvaService.AplicaTipTvaImplicit`, care devine un wrapper peste serviciu —
  semnătura apelanților nu se schimbă) și endpoint-ul de citire (D6).

### F23-D2 — `PoliticaTvaImplicit`: regimul e al partenerului, cota e a produsului

Clasa `ClasaFiscalaPartener` = enum în `BusinessObjects/Comun` (intră în
`metadata.json`): `InregistratRo = 1`, `NeinregistratRo = 2`, `Ue = 3`,
`ExtraUe = 4` — exact valorile lui `D394Proiectii.TipPartener`, care începe să
ÎNTOARCĂ enum-ul (D394 face cast la `int` acolo unde scrie cifra; funcția se
mută în `Comun/ClasaFiscala.cs` ca `ClasaFiscala.APartenerului(tipPersoana,
tara, inregistratTva)`, D394 o cheamă de acolo; zero schimbare de cifră în
D394, probată de check-urile existente).

Tabelul `PoliticaTvaImplicit` (`Politici/Politici.cs`, `[NavigationItem("Politici")]`):

| Câmp | Tip | Semantică |
|---|---|---|
| `TipDocumentId` | Guid, FK | tipul de document (obligatoriu) |
| `ClasaFiscala` | `ClasaFiscalaPartener?` | `null` = orice clasă (generic) |
| `ValabilDeLa` | `DateOnly?` | `null` = dintotdeauna; altfel rândul se aplică documentelor cu `Data >= ValabilDeLa` (precedentul real: cotele din 1 august 2025) |
| `TipTvaId` | Guid, FK | ținta (obligatorie) |
| `DinSeed` | bool | D4 |

Index unic `(TipDocumentId, ClasaFiscala, ValabilDeLa)` filtrat `"GCRecord" = 0`,
cu `NULLS NOT DISTINCT`.

Rezolvarea (`ImpliciteService.TipTva(os, tipDocumentId, partenerId?, produsId?,
data)` ⇒ `RezultatImplicit { TipTvaId?, Sursa: SursaImplicit, Motiv }`,
`SursaImplicit` enum în `Comun`: `Partener`, `Produs`, `Politica`, `Ancora`,
`Niciuna`):

1. **R** (purtătorul de REGIM) = `Partener.TipTvaImplicit` (dacă partenerul e
   dat și îl are) → altfel rândul `PoliticaTvaImplicit` cel mai SPECIFIC:
   candidații = rândurile tipului cu `ValabilDeLa == null || ValabilDeLa <= data`;
   clasa exactă bate `null`; la egalitate de clasă bate `ValabilDeLa` cel mai
   recent → altfel `TipDocument.TipTvaImplicit` (ancora) → altfel `null`.
2. **P** (purtătorul de COTĂ) = `Produs.TipTvaImplicit`, doar dacă linia are produs.
3. **Rezultatul** = `P` dacă `P != null && R != null && P.Regim == R.Regim`
   (același regim ⇒ produsul își impune cota: pâinea rămâne 11% de la orice
   furnizor înregistrat; pe bugetar `CAP11` bate `CAP21` la fel); altfel `R ?? P`.
   Regimul partenerului bate cota produsului când regimurile DIFERĂ: o livrare
   intracomunitară e scutită indiferent de produs.
4. Clasa fiscală se calculează din `Partener` cu funcția legii; un partener
   INVIZIBIL pe ușa securizată se tratează ca „fără partener" (rezolvarea cade
   pe generic), cu `Motiv` care spune asta — nu 422 (implicitul e afordanță, nu
   validare).
5. Partenerul documentului se ia FĂRĂ `is`/`switch` pe frunze: dintre
   `PredatorId`/`PrimitorId`, cel care e `Partener` (`os.GetObjectByKey<Partener>`);
   dacă amândoi (nu există azi), predatorul. Un tip fără partener pe laturi
   (DEC: angajat) cade natural pe generic.
6. Un `TipTva` INACTIV (D3) nu e ales niciodată de rezolvare: treapta care ar
   întoarce un inactiv se SARE, cu `Motiv`, iar raportul de profil (D8) îl
   listează ca defect de configurare.

Seed privat, DOAR ce e sigur în lege (rândurile nesigure se DECLARĂ, nu se
inventează): `FCL × Ue → SDD`, `FCL × ExtraUe → SDD`, `RDC × Ue → SDD`,
`RDC × ExtraUe → SDD`, `FCT × Ue → TI21`, `RLF × Ue → TI21`. Fără rând pentru
achiziția extra-UE (importul cu TVA în vamă e un document propriu — familia
36f) și pentru achiziția de la neînregistrat RO (linia n-are fapt de TVA;
rămâne pe ancora N21 până la o decizie proprie — restanță). Bugetar: zero
rânduri (totul e capitalizat, ancora ajunge). `--forceUpdate` idempotent pe
cheia indexului.

### F23-D3 — Unicitatea și viața nomenclatoarelor de politică intră în SCHEMĂ

- `TipTva.Activ` (bool, default `true`): lookup-urile de culegere ale
  clientului filtrează `Activ eq true`; XAF: `[ListViewFilter]`-ul rămâne
  restanță (44). Un tip inactiv pe o linie EXISTENTĂ e istorie legitimă și
  nu se refuză nicăieri. Seed-ul marchează `N19`/`TI19`/`CAP19` inactive
  (`Activ = false` DOAR când `DinSeed`, D4).
- Indexuri unice filtrate `"GCRecord" = 0` (precedentul 74a), `NULLS NOT
  DISTINCT` unde cheia are nullable:
  `PoliticaTva(TipDocumentId)`, `PoliticaConex(TipDocumentSursaId)`,
  `PoliticaScadenta(TipDocumentId)`, `PoliticaValidare(TipDocumentId)`,
  `PoliticaNumerotare(TipDocumentId)`, `PoliticaInchidereTva(TipDocumentId)`,
  `RegulaStoc(TipDocumentId, Latura, ClasaId)`,
  `RegulaContare(TipDocumentId, TipMaterialId, NaturaFiltru, SemnFiltru)`,
  `TipDocument(Cod)`, `TipDocument(ClrType)`, `TipTva(Cod)`, `Cont(Simbol)`,
  `ClasaProdus(Cod)`, `TipMaterial(Cod)`, `PoliticaTvaImplicit` (D2).
  `Repartitor.Cod` NU intră (spațiul de coduri e partajat pe TPT între
  parteneri, gestiuni, angajați; bazele de import pot avea coliziuni legitime —
  restanță cu nume).
- Migrația nu maschează dubluri (77k): ÎNAINTE de a scrie migrația, agentul
  rulează interogarea de dubluri pe fiecare cheie, pe bazele de dev
  (`Atlas.Conta.BackOffice.Privat`, baza bugetară, `Atlas.Conta.ModelCheck.Privat`,
  Flax dacă e sus) și RAPORTEAZĂ; un dublu real e regulă de oprire, nu
  `DISTINCT ON` în migrație.
- Comentariul din `Comun/Cautare.cs:15` devine adevărat; ModelCheck citește
  indexurile din `IDesignTimeModel` și le compară literal (precedentul D17-V1).

### F23-D4 — Proveniența: `DinSeed`, scris de seed, șters de gardian

- Interfața `ICuProvenienta { bool DinSeed { get; set; } }` pe cele 12 clase de
  politică, pe `PoliticaTvaImplicit`, `TipTva`, `Cont`, `ClasaProdus`,
  `TipMaterial`. NU pe repartitori (structura clientului) și NU pe `Societate`.
- Seed-ul pune `DinSeed = true` pe rândurile pe care le CREEAZĂ și pe cele pe
  care le GĂSEȘTE pe cheia lui (o linie în ramura „există ⇒ continue" a fiecărei
  funcții de seed — mecanic, fără schimbarea semanticii de re-seed). Limita
  declarată: un rând editat manual ÎNAINTE de această migrație va fi marcat
  seed la prima trecere (nedetectabil; se declară în decizie).
- `GardianEditare` (ușa securizată): la orice scriere pe un obiect
  `ICuProvenienta` care NU e nou, `DinSeed` devine `false` (gardianul îl
  SCRIE, ca un câmp server-owned inversat); pe obiect nou, `DinSeed = true`
  primit de la client se refuză cu mesaj („proveniența o scrie seed-ul"). Pe
  ușa de sistem (seed, ModelCheck fără gardian) nimic nu se întâmplă.
- Semantica de re-seed NU se schimbă în felia asta („insert dacă lipsește"
  rămâne). Ce cumpără flag-ul acum: vizibilitatea (grila arată „seed / manual"),
  raportul D8, și fundația pentru „seed-ul corectează DOAR rândurile
  `DinSeed`" — restanță cu nume, cu textul aici.
- OData: `DinSeed` e citibil, iar scrierea lui e refuzată de gardian (mai sus).
  OpenAPI îl expune ca membru obișnuit; clientul îl arată, nu îl editează.

### F23-D5 — Ușa OData se DESCHIDE pe politici, cu invarianții în gardian

Trec de la `ReadOnly` la CRUD: cele 12 politici (`TipDocument` inclusiv, dar
cu `Cod`/`ClrType` server-owned — gardianul refuză schimbarea lor și crearea
de rânduri noi: ancora e codul, 20), `PoliticaTvaImplicit`, `TipTva`.
`ClasaProdus` se expune `ReadOnly` (lookup). Rămân `ReadOnly`, declarat:
`Cont` (planul de conturi rămâne în XAF; ciclul `Parinte`, 70g), `ContPropriu`,
`UnitateInterna`, `RandD300`, `Judet`, `UnitateMasura`, `Lot`, clasificația
bugetară, `Repartitor` (baza).

Ramuri noi în `GardianEditare.Verifica` (regula rămâne și ca atribut XAF unde
există — 77k: două jumătăți de prezentare, o regulă de fond):

| Tip | Invariant |
|---|---|
| orice `ICuProvenienta` | D4 |
| `TipDocument` | pe existent `Cod`/`ClrType` neschimbate; rând nou refuzat; `TipTvaImplicit` (dacă e dat) activ |
| `TipTva` | `Cota ∈ [0, 100]`; `Activ = false` pe un tip referit de ancoră/`PoliticaTvaImplicit`/`Partener`/`Produs` ca implicit = refuz cu lista referințelor (un implicit spre inactiv ar fi sărit tăcut) |
| `PoliticaTvaImplicit` | `TipTva` activ; cheia (D2) — unicitatea o dă indexul, gardianul dă MESAJUL înaintea constraint-ului (60a rămâne plasă) |
| `PoliticaTva` | `SursaContrapartida = Explicit ⇒ ContrapartidaFallbackId != null` |
| `RegulaContare` | `SursaContDebit/Credit = Explicit ⇒ ContDebitId/CreditId != null`; `SemnFiltru ∈ {−1, null, +1}`; `TipMaterialId != null ⇒ NaturaFiltru == null` (cele două trepte sunt alternative, 26c) |
| `RegulaStoc` | `Semn ∈ {−1, +1}` |
| `PoliticaNumerotare` | `Serie` și `Format` nevide; `UrmatorulNumar ≥ 1` |
| `PoliticaScadenta` | `ZileDefault ≥ 0` |
| `PoliticaInchidereTva` | cele patru conturi ori TOATE, ori niciunul (serviciul cere setul complet) |
| `MapareD300` | `RandEsteDeOperatiuni` + `RandFaraAscendentMapat` (mutate din `RuleFromBoolProperty` într-o funcție statică chemată din ambele: atributul rămâne, gardianul o cheamă) |
| `MapareD394` | `TintaPermisa(Tip, Sens)` (funcția statică există) |
| `PoliticaMiscareSaft` | ramura existentă |

Ștergerea logică a unei politici rămâne liberă (seed-urile noi o citesc ca
„decizie a utilizatorului", 3.1 din raport). Refuzurile ies `422 EroriDto`
prin `RefuzOdataFilter` (80d), permisiunea 403/404 înaintea lor (80a).

### F23-D6 — Endpoint-ul de implicite: serverul spune, clientul arată

`GET api/implicite/tip-tva?tipDocument=FCT&partenerId=…&produsId=…&data=…`
(`ImpliciteController`, ușa SECURIZATĂ — implicitul se calculează peste
nomenclatoare vizibile; nicio cifră de registru, deci nu intră în 80e).
Răspuns 200 `RezultatImplicitDto { TipTvaId?, TipTvaCod, TipTvaDenumire,
Cota?, Sursa (string, pe nume), Motiv }`; `tipDocument` necunoscut ⇒ 400
(`EroriDto`, binding). Fără gate pe TIP: cine poate culege o linie poate
întreba implicitul ei; `User` primește generic (partenerul invizibil ⇒ „fără
partener", D2.4).

Client: în editorul de linie al celor cinci felii, pe linie NOUĂ cu `TipTvaId`
gol, se cere implicitul la deschidere și la schimbarea produsului
(declanșatori, nu la fiecare tastă — 56), și se PRECOMPLETEAZĂ perechea
(id, etichetă) prin convenția 77c (`precompleteaza*`: niciodată peste alegerea
operatorului). Indiciul de sub câmp spune sursa („implicit din politica
tipului × clasa fiscală", „cota produsului", „ancora tipului"). Serverul aplică
ACELAȘI serviciu la PUT pe liniile care ajung goale — cele două nu pot diverge
fiindcă sunt o singură funcție. Zero calcul în TS (43b).

### F23-D7 — Grila comună de politică în client

`felii/politici/GrilaPolitica.tsx`: `DataGrid` DevExtreme cu editare pe rând
(`mode="row"`, `allowAdding/Updating/Deleting` din prop), peste un
`CustomStore` care CITEȘTE prin `storeOData(entitate)` (paginare, sortare,
`$expand` pentru etichete) și SCRIE prin `creeazaRand`/`modificaRand`/
`stergeRand` din `nomenclatoare/api.ts` — adică prin `http.ts`, ca mesajul
`EroriDto` să ajungă în grilă (pe conducta `ODataStore` s-ar pierde, 80-r1).
Refuzul = promisiune respinsă cu `Error(erori.join('\n'))` ⇒ rândul rămâne în
editare cu mesajul serverului. Lookup-urile de FK = coloane `Lookup` cu
`storeOData` pe entitatea țintă, `displayExpr` cod + denumire; enum-urile prin
`labelEnum`; `DinSeed` = coloană read-only „seed / manual".

Coloanele sunt COD, per politică, într-un fișier scurt (43a; precedentul
`PoliticiMiscareSaft.tsx`): ecranele feliei — `PoliticaTvaImplicit` (nou),
`TipTva` (cu `Activ`), `TipDocument` (doar `TipTvaImplicit` editabil),
`PoliticaMiscareSaft` (devine editabilă — închide 77-r3 pe itemul ăsta),
`PoliticaScadenta`, `PoliticaNumerotare`, `PoliticaInchidereTva` (închide
79-r2). `RegulaContare`/`RegulaStoc`/`MapareD300`/`MapareD394`/`PoliticaTva`/
`Conex`/`Validare` rămân pentru felia următoare, pe același șablon, împreună
cu unealta „Explică" (declarate în §Ce NU intră). Pe `PartenerDetaliu` și
`ProdusDetaliu`: un `Lookup` `TipTvaImplicitId` (filtrat `Activ`). Meniul:
grupul „Politici" existent.

### F23-D8 — Raportul de verificare a profilului

`Motor/VerificareProfilService.Raporteaza(os)` ⇒ listă de
`ConstatareProfil { Tabel, Cheie (etichetă lizibilă), Fel: FelConstatare,
Mesaj }`, `FelConstatare` enum în `Comun` (`RandManual`, `ReferintaStearsa`,
`TipTvaInactivReferit`, `PoliticaLipsa`, `MapareLipsa`). Conținut:
(a) rândurile `ICuProvenienta` cu `DinSeed = false` (create sau editate de
client), (b) FK-uri de politică spre rânduri șterse logic (`IgnoreQueryFilters`),
(c) `TipTva` inactiv referit ca implicit, (d) tip de document cu `PoliticaTva`
dar fără ancoră `TipTvaImplicit`, (e) golurile deja verificate de seed
(`VerificaD300`/`VerificaD394`, cu categoria „șters de utilizator" respectată)
— raportate, nu aruncate. Ruta `GET api/politici/verificare` (secured, `Read`
pe tipurile citite; `User` ⇒ 200 gol, 69g), ecranul `/politici/verificare`
cu grupare pe `Fel`. Seed-ul rămâne cel care ARUNCĂ; raportul e cel care
ARATĂ.

### F23-D9 — Auditul se MĂSOARĂ pe OData și devine istoric per rând

`AuditDataItemPersistent` și `AuditEFCoreWeakReference` se expun `ReadOnly`
pe OData (securitatea XAF filtrează: rolul `Default` vede doar rândurile
proprii — `Updater.cs:142-153`). Proba HTTP: `PATCH api/odata/TipTva({id})`
ca `Admin` ⇒ 204, apoi `GET api/odata/AuditDataItemPersistent?$filter=…` cu
un rând nou pentru acel obiect (`OperationType`, `PropertyName`, `UserName =
Admin`); fără urme = PATCH-ul revine la valoarea inițială în `finally`. În
grilă: panoul „Istoric" pe rândul selectat (ultimele n modificări: când,
cine, ce câmp, din ce în ce), citit din același set. Dacă forma
`AuditedObject` (referința slabă) nu se poate filtra prin `$filter` pe OData,
panoul devine restanță cu cauza măsurată — proba HTTP rămâne obligatorie.

### F23-D10 — Securitatea se măsoară pe HTTP, în scriptul existent

`nou/tools/ProbeHttp/refuzuri.ps1` capătă blocul „politici" (fără urme, a doua
rulare identică):

| Cerere | Admin | Cititor | User |
|---|---|---|---|
| `PATCH api/odata/PoliticaScadenta({id})` (`ZileDefault` +0, revenit) | 204 | 403 `EroriDto` „modifica" | 404 `EroriDto` |
| `POST api/odata/PoliticaTvaImplicit` cu `TipTva` INACTIV | 422 „inactiv" | 403 (înaintea gardianului — 80c) | 403 |
| `POST api/odata/PoliticaTvaImplicit` DUBLU pe cheie | 422 cu mesajul gardianului (nu textul constraint-ului) | — | — |
| `PATCH api/odata/TipTva({N21})` `Activ=false` | 422 cu lista referințelor | — | — |
| `PATCH api/odata/TipDocument({FCT})` `Cod` | 422 „server-owned" | — | — |
| `PATCH …` cu `DinSeed=true` pe rând manual | 422 | — | — |
| `GET api/implicite/tip-tva?tipDocument=FCL&partenerId={UE}` | 200 SDD, `Sursa=Politica` | 200 | 200 generic (`Motiv` cu partener invizibil) |
| `GET api/politici/verificare` | 200 cu rânduri | 200 | 200 gol |
| audit (D9) | 204 + rând nou | — | — |

### F23-D11 — Regula de oprire

Agentul se oprește și raportează (nu normalizează tăcut) dacă: interogarea de
dubluri (D3) găsește vreun dublu pe vreo bază de dev; `AreNullsDistinct(false)`
nu produce `NULLS NOT DISTINCT` în migrație (versiunea Npgsql/EF); mutarea
lui `TipPartener` schimbă vreo cifră în check-urile D394 existente; gardianul
nu poate scrie `DinSeed` fără ca EF să raporteze obiectul ca modificat de
două ori (dublu commit); `AuditDataItemPersistent` nu se expune sau nu se
filtrează prin OData; DevExtreme `DataGrid` nu ține rândul în editare la
respingerea promisiunii; o probă HTTP dă alt cod și cauza nu e a feliei;
ModelCheck pică pe orice profil; driftul openapi/metadata nu se închide cu
regenerarea.

### F23-D12 — Ce NU intră

Implicitele pentru laturi (care gestiune / care cont propriu — implicit de
sesiune sau de subiect, felia următoare); publicarea felului laturilor în
metadata; „Explică" (rezolvarea motorului pe un context) și ecranele
`RegulaContare`/`RegulaStoc`/`MapareD300`/`MapareD394`/`PoliticaTva`/`Conex`/
`Validare`; schimbarea semanticii de re-seed pe `DinSeed`; export/import de
politici între baze; rolul „Configurator" (Write pe politici fără Write pe
documente); `Repartitor.Cod` unic; achiziția extra-UE și de la neînregistrat
ca rânduri de politică; XAF: filtrul `Activ` pe lookup-urile de `TipTva`,
baseline-ul UI pe cele 8 politici fără `HideForeignKeys`; `Cont` scriibil pe
OData.

## Pașii

1. **Module + ModelCheck** — `ClasaFiscalaPartener` + mutarea funcției legii
   (D2), `PoliticaTvaImplicit`, `TipTva.Activ`, `Partener/Produs.TipTvaImplicit`,
   `ICuProvenienta` + `DinSeed` (D4), indexurile (D3, cu interogarea de dubluri
   ÎNAINTE), migrația (`--context` obligatoriu), seed-ul (rândurile D2, marcajele
   D4, inactivele D3), `ImpliciteService` + wrapper-ul din `TvaService`,
   ramurile gardianului (D5), `VerificareProfilService` (D8); ModelCheck: probe
   pe rezolvare (ordinea, regula „același regim", `ValabilDeLa`, inactivul
   sărit, partenerul lipsă), pe indexuri (din `IDesignTimeModel`), pe gardian
   (`Verifica` direct: fiecare rând din tabelul D5), pe `DinSeed` (seed-ul
   marchează, gardianul demarchează — probat cu OS securizat? NU: gardianul
   fără strategie tot rulează `Verifica`; proba stă pe `Verifica` + ștergerea
   flag-ului acolo), pe D394 neschimbat; `--dump-metadata`; verde pe AMBELE
   profiluri.
2. **WebApi** — deschiderea OData (D5), `ClasaProdus`/audit `ReadOnly`,
   `ImpliciteController` (D6), `api/politici/verificare` (D8); Apply-urile
   neschimbate la semnătură; `pnpm gen:openapi && gen:types`, drift închis.
3. **Probe HTTP** — blocul „politici" în `refuzuri.ps1` (D10 + D9), rulat pe
   host viu (Privat, re-seed pentru `Cititor` și pentru rândurile D2), tabelul
   în §Închidere.
4. **Client** — `GrilaPolitica` + cele 7 ecrane (D7), lookup-urile pe
   partener/produs, precompletarea implicitului în cele 5 editoare de linie
   (D6), ecranul de verificare (D8), panoul „Istoric" (D9); `pnpm build`
   verde; smoke în browser (Admin: editează o politică și vede refuzul
   gardianului în rând; FCL nouă pe partener UE ⇒ SDD precompletat cu indiciul
   sursei; Cititor: grila fără butoane de editare și 403 la o încercare).
5. **Review advers** cu scenarii concrete: implicitul suprascrie o alegere
   (client sau PUT); `Partener.TipTvaImplicit` inactiv; două rânduri de
   politică cu `ValabilDeLa` egal; `DinSeed` reînviat printr-un PATCH care nu
   schimbă nimic; gardianul cheamă `IgnoreQueryFilters` și „vede" un rând
   șters ca dublu; `TipDocument` nou creat prin OData; auditul atribuit altui
   utilizator; `User` obține prin `implicite` informația că un partener
   invizibil există (oracol de existență — `Motiv` trebuie să fie același
   pentru inexistent și invizibil); `Cititor` pe `verificare` vede
   `RandManual` ale societății (asumat: e Read).
6. **Docs** — decizia 081 (Context / Decizia (a)–(l) / Review / Ce rămâne
   deschis), README-ul deciziilor, istoricul, CLAUDE.md (§81, roadmap,
   „Următorul pas" = felia 24: Explică + restul politicilor; restanțele
   închise 77-r3 parțial, 79-r2, 74-r12; cele noi cu nume), §Închidere aici.
