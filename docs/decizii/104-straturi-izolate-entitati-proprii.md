# 104 — Straturi izolate, culegerea în XAF, entități proprii fără ștergere amânată

Data: 2026-09-26
Stare: activă; amendează 042(b)(e), 043 (aria React) și 083(a)(j) (mecanismul, prin (i)); depășește constatarea 60a și F13-D2 (070) privind ștergerea amânată
Docs: acest fișier (regula + pașii feliei C104); `docs/stare-curenta/` se actualizează la fiecare pas, odată cu codul

## Regula durabilă

**(a) Cinci straturi, cu dependențe într-un singur sens.** L0 Nucleu (pur,
BCL). L1 Declarații (pure peste `Operand`, snapshot-ul înghețat). L2 Coaja
comenzii (citește `Operand`, scrie cubul și registrele, blocaje, perioadă).
L3 Culegere (draft, implicite, recalcul, validare de domeniu). L4 Randare
(XAF Blazor, React). Un strat nu cunoaște stratul de deasupra lui. Motorul
(L0–L2) nu cunoaște securitatea XAF și nici ștergerea logică.

**(b) Coaja comenzii își creează singură contextul.** Apelantul dă ID-ul
documentului și identitatea operatorului. Coaja își alege singură contextul
de scriere și verifică explicit dreptul de comandă. Ce are voie să facă cineva
nu se mai deduce din fabrica prin care s-a născut ObjectSpace-ul primit.
„Secvență, nu cuib” (42b) rămâne: culegerea se comite înainte, comanda e
tranzacția motorului. Ținta completă (coaja fără `IObjectSpace`, hook-urile
cu `os` de pe entitate mutate în declarant sau în coajă) se execută la TR-D9,
când drumul registrelor dispare.

**(c) L3 e singura sursă de precompletare, recalcul și validare de domeniu.**
Controllerele XAF și `Api/*Apply` sunt adaptori peste aceleași servicii.
ModelCheck, importul și React sunt alți apelanți ai lui `Apply` (42d).
Un comportament de culegere scris într-un singur adaptor e defect.

**(d) Culegerea documentelor se face în XAF; React face citiri și proiecții.**
React acoperă jurnale, declarații, fișe, reconcilieri, adică ecrane al căror
contract e ieșirea motorului. Paginile React de detaliu pentru documente sunt
înghețate: nu mai primesc câmpuri noi, pot rămâne în urmă față de model și
nu blochează nicio felie. `WriteDto`/`Apply` rămân, fiindcă sunt L3.
Amendează 42(e) („layout = React”) și aria din 43.

**(e) Entitățile de domeniu au o bază proprie.** `EntitateConta` (abstractă)
poartă doar contractele XAF (`IXafEntityObject`, `IObjectSpaceLink`) și
cheia `Guid ID`. Mecanismele tehnice se aleg pe familia de entități, nu vin
din bază:
- `Editabila`: blocare optimistă;
- `Nomenclator`: `Editabila` + `Activ`;
- `Politica`: `Editabila`;
- `Document`: `Editabila`, rădăcina TPH;
- `RandRegistru`: scris doar de motor, fără blocare, fără ștergere.

Cubul rămâne POCO. Tipurile de securitate (utilizator, roluri, login)
rămân pe `BaseObject`-ul DevExpress.

**(f) Fără ștergere amânată.** `UseDeferredDeletion` nu se mai apelează pe
model, deci coloana `GCRecord` nu există. Draftul se șterge fizic, documentul
operat se stornează, nomenclatorul se inactivează (`Activ`), registrele nu se
șterg. „Șters” nu mai e o stare a nicio entități de domeniu. Un tip care ar
cere păstrarea rândului după ștergere cere o stare de domeniu explicită, nu
reintroducerea `GCRecord`.

**(g) FK: `Cascade` numai în agregat.** O cheie străină are `Cascade` doar
dacă navigația e compoziție, adică are colecția inversă `[Aggregated]`
(liniile documentului, legăturile DVI ↔ factură). În rest, ștergerea
părintelui referit se refuză în bază: EF `ClientNoAction`, iar în schemă
`NO ACTION`. Nu se folosește `Restrict` în EF, fiindcă, atunci când
dependentul e deja încărcat în același ObjectSpace, EF aruncă singur o
excepție netradusă, înaintea bazei. Refuzul iese ca 422 de domeniu pe toate
ușile (80), prin traducătorul de constrângeri (39a), cu tipurile părintelui
și dependentului numite.

**(h) Proba.** Catalogul de scenarii și ModelCheck, verzi pe ambele
profiluri. ModelCheck probează structural că:
- modelul EF nu are proprietatea `GCRecord` pe niciun tip de domeniu;
- `Cascade` apare doar pe compoziții;
- `RandRegistru` nu are câmp de blocare optimistă;
- fiecare tip configurabil are index unic, adică cheia refuzului din (i).

Refuzul FK se probează în ModelCheck pe ambele căi EF (dependent încărcat
sau nu), prin HTTP (`refuzuri.ps1`) și în browser pe XAF.

**(i) Rândul de seed șters rămâne șters, prin refuz explicit.** Ștergerea
unui rând `ICuProvenienta` pe ușa securizată lasă un `RefuzSeed`: tipul și
cheia rândului. Cheia sunt valorile coloanelor indexurilor unice (83b),
serializate. Seed-ul (`ContaSeeder.Aliniaza`) și golurile de mapare citesc
refuzul, nu rândul șters, deci 83(a)(j) rămân adevărate fără `GCRecord`.
Rândul revine dacă se șterge refuzul (rolul `Configurator`). Ușa de sistem
nu lasă refuzuri.

## Context

Discuția cu owner-ul, 2026-09-26. Sistemul e privit pe trei responsabilități:
UI (layout, validare, precompletare), orchestratorul de persistare și motorul
(proiecția faptelor în dimensiuni, reguli de compunere). Codul le are, dar:

- **Între orchestrator și motor**, granița e curată doar în `Contractare`
  (`Fapte.Operand` → `IDeclarant.Declara` pur → `N.Motor.Opereaza`). Restul
  motorului (`MotorOperare`, `SolduriService`, `PerioadaService`,
  `Cub/Materializare`) lucrează peste `IObjectSpace` și îl ocolește prin
  `((EFCoreObjectSpace)os).DbContext` în ~18 fișiere: tranzacție,
  `pg_advisory_xact_lock`, `ChangeTracker`, `Reload`.
- **Securitatea se scurge.** Contractul de apelant al `OperareApi` cere un OS
  non-secured, iar `Cub/Citiri/CumulPerioade` se comportă diferit după
  `os is ISecuredObjectSpace`.
- **Ștergerea amânată se scurge.** `"GCRecord" = 0` e scris explicit în SQL-ul
  brut al motorului. Registrele primesc la `INSERT` `GCRecord` și
  `OptimisticLockField`. ~15 indexuri unice sunt filtrate pe `GCRecord`.
  `D394Proiectii` citește `GCRecord != 0` ca „partener șters”. Capcana
  `IsDeletedObject` e descrisă în `GardianEditare`.
- **Entitatea e tipul comun al tuturor straturilor**: atribute de UI, entitate
  EF și participant la motor prin `PregatesteOperare(os)`,
  `ValideazaOperare(os, …)`, `GenereazaSecundar(os)`.
- **Două UI-uri pe o culegere nepartajată.** XAF lucrează direct pe entitate,
  cu precompletare și recalcul în controllere (`DocumentDefaultsController`,
  `RecalculValoriCulegereController`, `DefaultTipTvaController`). React
  lucrează pe DTO-uri scrise de mână per tip. Exemplul măsurat: două câmpuri
  noi pe FCT (103) au atins entitatea, migrația, baseline-ul XAF, trei DTO-uri,
  `Apply`, `openapi.json`, `api-types.ts`, `api.ts` și pagina TSX. La fel, de
  6 ori, pentru FCT, FCL, DVI, RDC, RLF și DEC. ModelCheck probează prin `Apply`
  (`TrezorerieApply.Aplica` ×53, `FacturaIntrareApply.Aplica` ×25), deci
  `Apply` e deja stratul de culegere independent de UI; XAF îl ocolește.

Tranșarea UI (varianta A din trei):
- (A) culegerea în XAF, React pe citiri;
- (B) React ca randor generic din metadate: reconstruiește XAF în React, prea devreme;
- (C) status quo: nu reduce nimic.

Owner-ul a ales A. Speranța declarată e ca XAF Blazor să câștige cu entități
de UI bine structurate, fără să plătim acum costul optimizării UX. Proba
motorului e catalogul, nu ecranul.

### Verificarea în sursele DevExpress 26.1.4

- `modelBuilder.UseDeferredDeletion(this)`
  (`EFCoreDeferredDeletionExtension.cs`, `EFCoreDeferredDeletionRegistration`)
  pune marcajul `IDeferredDeletion` pe model, mapează `GCRecord` și pune
  filtrul global. Pe `BaseObject` (EF), `GCRecord` e `[NotMapped]` implicit.
- Host-ul adaugă mereu interceptorul (`ObjectSpaceProviderBuilderExtensions.cs:73, :110`),
  dar acesta nu face nimic fără marcajul de model
  (`EFCoreDeferredDeletionInterceptor.IsRegistered`). Oprirea globală =
  scoaterea apelului de pe `ModelBuilder`.
- Oprirea pe tip: `[DisableDeferredDeletion]` (`Inherited = true`, citit cu
  moștenire) sau `modelBuilder.DisableDeferredDeletion<T>()`. Amândouă ignoră
  coloana și scot filtrul.
- Blocarea optimistă e simetrică: `UseOptimisticLock` mapează orice
  `IOptimisticLock`, cu excepția tipurilor cu `[OptimisticLockIgnore]`.
- Nimic din XAF nu depinde de clasa `BaseObject`: `EFCoreObjectSpace.IsDeletionDeferredType`
  și `SecurityStateManager` întreabă de interfață, iar marcajul
  `IEFCoreBaseObject` nu e citit nicăieri.

Precedentul owner-ului: `D:\Dev\github.db\AtlasTI\ATS.Atlas.Abstraction`
(`XafBaseAbstractObject` → `XafKeyedObject<TKey>` → `XafSoftBaseObject<TKey>`
opțional → `XafBaseNomenclator`). Ideea preluată: mecanismele tehnice se aleg
pe familie de entități. Evaluatorul de proprietăți calculate de acolo e o
unealtă de L3/L4 și nu intră în motor (42c).

De ce acum: costul se plătește deja la fiecare felie, iar bazele se
recreează (102b), deci schimbarea de schemă nu are cost de migrare de date.

## Pașii feliei C104 (branch `c104-straturi`)

Fiecare pas se comite separat, cu `stare-curenta` actualizată în același
commit, cu ModelCheck verde pe ambele profiluri și cu catalogul verde.

**Pasul 1 — entitățile proprii și ștergerea fizică (e, f, g, h).**
- Clasifici cele 49 de clase derivate din `BaseObject` pe familiile din (e);
  clasificarea intră în tabelul de mai jos, în acest fișier, înaintea codului.
- Introduci baza proprie, scoți `modelBuilder.UseDeferredDeletion(this)` din
  `BackOfficeDbContext` și `UseDeferredDeletion()` din harness-urile ModelCheck
  (`Program.cs`, `Purja.cs`).
- Scoți filtrele `GCRecord` din indexuri și din SQL-ul brut, precum și
  `OptimisticLockField` din `INSERT`-urile registrelor.
- `D394Proiectii` citește `!Activ`.
- Auditezi cele 48 de FK `Cascade` după regula (g).
- Refuzul FK devine 422 pe XAF și pe API.
- Regenerezi migrația, recreezi bazele (rețeta 102), rulezi `--dump-metadata`
  și verifici driftul openapi.
- Adaugi probele structurale din (h).

Executat 2026-09-26. Oprirea (seed-ul cerea păstrarea rândului șters) a fost
tranșată de owner prin (i), iar forma FK prin (g) (`ClientNoAction`). Proba:
ModelCheck verde pe ambele profiluri, pe baze recreate (bugetar 3214, privat
4244 de verificări), `tools/ProbeHttp/stergere-fizica.py` pe hostul WebApi
(422 la FK, 200 + `RefuzSeed` pentru `Configurator`) și ștergerea refuzată
în XAF Blazor, cu mesajul tradus. Clonele de dezvoltare rămase pe 5444
(C102R1, ClaudeF103, FiscalCub, SnapshotStoc, CodexC102Review) nu mai
corespund codului și se recreează la nevoie (102b).

Review-ul Codex al pasului 1 (`comunicari/2026-09-26-1518`), corectat
înaintea pasului 2: R1 — cheia refuzului serializa zecimalul cu scara
reprezentării (`21.0000` din bază ≠ `21` din seed), deci maparea SAF-T
ștearsă reapărea; serializarea e acum fără scară (F24-V3, cheie zecimală).
R2 — indexul `(Categorie, DeLa)` al regulii de deductibilitate refuza un
plafon și un procent din aceeași zi, admise de 087(g); cheia e
`(Categorie, Fel, DeLa)`, ca selecția motorului (IMO-V58, migrația
`C104CheieDeductibilitate`). R3 — `stergere-fizica.py` ștergea toate
refuzurile D394 în `finally`; șterge acum doar refuzul creat, după ID.

**Pasul 2 — coaja comenzii (b), partea de azi.** `OperareApi` (sau
succesorul lui) nu mai primește `IObjectSpace` de la apelant. Primește o
fabrică de context și identitatea, și verifică explicit dreptul de comandă.
Se adaptează `DocumentOperareController`, controllerele WebApi și ModelCheck.
`CumulPerioade` nu mai întreabă de `ISecuredObjectSpace`. Interiorul lui
`MotorOperare` nu se atinge (TR-D9). Identitatea vine din contextul
autentificat, iar dreptul se verifică pe documentul și operația cerute; un
ID dat de apelant nu e singur autorizare (review Codex, pas 1).

Executat 2026-09-27. `OperareApi` a devenit coaja `Api/ComenziDocument`:
primește fabrica non-secured și un `IDreptComanda`, verifică dreptul
înaintea oricărei atingeri a domeniului (`SubiectInvizibil` = 404,
`RefuzAcces` = 403) și își deschide singură un context per comandă.
`DreptComandaXaf` e dreptul comun XAF și WebApi: rezolvă documentul pe
ușa securizată a utilizatorului autentificat, cu tipul ușii și restricția
ei (NTC), cere Write pe instanță și, la corecție, Create și Write pe tipul
concret; refuzul numește clasa reală a documentului. Gate-urile
duplicate din `DocumentOperareController` și `ContaApiController`/
`CorectieController` au ieșit; controllerele sunt adaptori de o linie
(`ComandaDocument<T>`). `CumulPerioade` nu mai întreabă de securitate:
apelantul declară `CitireCumul` — motorul `Integrala` (snapshot +
fereastră), proiecțiile servite pe ușa securizată `Vizibila` implicit
(numai postări). Tranșări ale pasului:
- ModelCheck păstrează contextul propriu prin ușa de sistem explicită
  `ComenziDocument.Sistem(os)` (fără drept XAF); separarea completă a
  contextului și în ModelCheck cade la 104-r2, împreună cu coaja fără
  `IObjectSpace`.
- Scrierile globale de snapshot (`SolduriService.CereNesecurizat`) încă
  refuză un context securizat după opțiunea EF; verificarea iese când
  închiderea perioadei trece prin propria coajă (104-r2).
- Balanța, fișa, soldurile și SAF-T citesc pe ușa securizată doar postări,
  ca înainte; probele de snapshot o cer explicit `Integrala` (SC-CIT-11…14,
  75, F27-R9), iar SC-CIT-95 și F27-R9 probează că citirea vizibilă nu
  atinge snapshot-ul.

Proba: ModelCheck verde pe ambele profiluri (bugetar 3219, privat 4250 de
verificări, `run-verificari/20260927-172209-149`), cu probele structurale
104b (refuzul înaintea contextului, un context per comandă, niciun
`IObjectSpace` în semnăturile comenzilor); HTTP
`tools/ProbeHttp/comenzi-coaja.py` 28/28 pe hostul WebApi privat; în XAF
Blazor, operarea unui draft fără linii dă refuzul de domeniu, iar `Cititor`
primește „Nu aveți dreptul de a modifica „NIR””. Driftul openapi: zero.
Matricea generală `refuzuri.ps1` nu rulează pe bazele recreate la C102
(nu are subiecte); limita e cea din `stare-curenta/dezvoltare-si-validare.md` (104-r5).

**Pasul 3 — culegerea unică (c).**
- Inventariezi căile de precompletare și recalcul din XAF față de cele din
  `Apply`/`ImpliciteApply`.
- Mute logica în servicii L3; controllerele XAF devin adaptori.
- O diferență de comportament (nu doar de formă) între două căi se raportează
  înainte de unificare.
- „Validarea de domeniu” din (c) e validarea culegerii (draftul). Conservarea,
  starea, perioada și condițiile dependente de date concurente rămân validate
  de L0–L2, în tranzacția comenzii; formularea lui (c) se precizează la acest
  pas (review Codex, pas 1).

Inventarul (2026-09-27). Validarea culegerii era deja una singură:
gardianul de Committing pe ușa securizată (`GardianEditare`,
`IVerificabilLaCommit`, 42a). Făceau excepție câteva reguli scrise numai în
`Apply`: TVA-ul manual (negativ, regim Normal/Taxare inversă), garda de
scară (copiată în 14 fișiere), rolul liniei RDC și produsul liniei NIR pe
lot străin. Precompletarea și recalculul difereau de comportament:
- TipTva implicit: XAF îl punea la crearea liniei, fără produsul ei (deci
  fără cota lui), pe orice tip; `Apply` îl punea la salvare, cu produsul, pe
  cinci tipuri.
- Valorile la culegere: XAF le calcula numai pe FCT/FCL; `Apply` le calcula
  pe FCT/FCL/DEC/RDC/RLF/BCS/NIR/LDI/ASM, cu formule copiate din
  `PregatesteOperare`.
- TipMaterial din produs: XAF îl completa pe FCT/FCL; API-ul îl cerea pe
  sârmă, iar React îl completa în TS.
- `DataPrimire`: XAF o lăsa goală; `Apply` o scria din data înregistrării
  și o lăsa veche la mutarea acesteia.
- CAS: `Apply` genera liniile cu `Data`, dar operarea le verifica cu
  `DataInregistrare` (defect). XAF nu le genera deloc.
- PIF `ValoareFiscala` și DEC `Cantitate` 0 → 1 se completau la culegere
  numai pe o cale.

Tranșările owner-ului (2026-09-27):
- TipTva implicit se aplică la alegerea produsului și, dacă linia nouă e
  încă fără tip, la salvare. Se aplică numai pe tipurile a căror formulă
  de valori poartă TVA (`Document.CuTva()`: FCT, FCL, DEC, RLF, RDC, DVI).
  Valoarea o decid datele: partenerul, politica și ancora. Nu `PoliticaTva`,
  fiindcă bugetarul n-o are, dar are `CAP21` pe ancoră.
- `DataPrimire` goală înseamnă „data înregistrării”. Nu se materializează
  la culegere.
- `TipMaterialId` devine opțional pe liniile cu produs. Serverul îl ia din
  `Produs.TipMaterial`; lipsa lui, după precompletare, e refuz de domeniu.
- TVA-ul manual e o intenție explicită a adaptorului (XAF: editarea
  câmpului; API: câmpul prezent). Recalculul șterge override-ul numai la
  schimbarea bazei sau a tipului de TVA. Regulile lui trec în gardianul de
  commit.

Precizarea lui (c): validarea culegerii e gardianul de commit, pe orice
ușă securizată. Pre-verificările din `Apply` rămân numai pentru ordinea
mesajului (Draft înaintea mapării, enum-ul înaintea `CreateObject`); nu
poartă reguli proprii. Formula valorii liniei e una, pe entitate, lângă
`PregatesteOperare`. Culegerea o cheamă pentru previzualizare, iar operarea
o cheamă ca autoritate și îi adaugă semnul.

Executat 2026-09-27.
- L3 e `Module/Culegere/`: `CulegereDocument` (precompletare, recalcul,
  `InainteDeSalvare`) și `NormalizariTip` (câmpurile direcției opuse la
  LDI și ASM, NIR, liniile CAS refăcute la data înregistrării — repară
  defectul CAS).
- Formula valorii e pe entitate: `Document.CalculeazaValori`, `BazaLinie`,
  `CalculeazaLinie`, `IntrariBaza()`, `CuTva()`, `SemnulEAlOperarii()`.
- În XAF, `CulegereDocumentController` și geamănul pe linie înlocuiesc cele
  patru controllere vechi. `CulegereLaCommitXaf` e înregistrat în hostul
  Blazor înaintea gardianului.
- Cele 15 `Apply`-uri mapează și cheamă L3. Garda de scară e generică în
  gardian, citită din modelul EF. Regulile TVA-ului cules (negativ, regim,
  taxare inversă pe livrare, scară) sunt în L3 și se verifică înaintea
  atribuirii și înaintea recalculului, care altfel le-ar șterge în tăcere.
  Rolul liniei RDC, valoarea în vamă și tipul liniei obligatoriu sunt în
  gardian.
- Tranșări ale execuției:
  - Împerecherea își păstrează garda explicită de scară, fiindcă e comandă,
    nu culegere: serviciul ar rotunji suma în tăcere.
  - DVI calculează din cotă taxa lăsată la 0 încă de la culegere, ca
    operarea (48b).
  - BTR și BCS au valoarea la culegere.
  - În ecranul XAF, o valoare culeasă de bază (RDC venit, DVI) cu prea multe
    zecimale se rotunjește la recalculul interactiv, înaintea gardianului.
    Editorul e deja limitat la scara coloanei, deci cazul rămâne limită
    asumată.

Proba:
- ModelCheck verde pe ambele profiluri (bugetar 3220, privat 4255 de
  verificări, `run-verificari/20260927-191138-576`), cu `104c-S1`, `E1`,
  `E2`, `V1` și `V2`.
- Driftul openapi e numai `TipMaterialId?: string | null` pe cele 5 DTO-uri
  de linie; `tsc` trece.
- În XAF Blazor, pe o FCT nouă: data de azi, produsul completează tipul și
  N9, iar 2 × 50 dă 100 + 9 înaintea salvării. Salvarea naște lotul și lasă
  `DataPrimire` goală. Un TVA manual pe o linie scutită e refuzat cu
  mesajul de domeniu.

**Pasul 4 — aria React (d).** Actualizezi `stare-curenta/api-si-client.md`,
`docs/api/lista-react.md` și principiile din `CLAUDE.md` (straturile,
culegerea în XAF). Paginile de detaliu pentru documente se marchează înghețate.

Executat 2026-09-27, numai documentație: `stare-curenta` (împărțirea
responsabilităților, ecranele, limitele, indexul), `lista-react.md` (premisa
restrânsă la citiri; itemii de culegere marcați), principiile din `CLAUDE.md`,
antetele 042/043 și README-ul clientului. 77-r1 și 77-r6 (selectoarele
paginilor React de culegere) sunt depășite de (d); soarta paginilor rămâne
104-r4.

**Pasul 5 — review advers și închidere.** Review advers pe pașii 1–3,
corecturile pe felie, apoi închiderea restanței 104-r1.

**Regula de oprire.** Te oprești și raportezi în trei situații:
- un tip de domeniu cere păstrarea rândului după ștergere;
- o cascadă auditată nu se încadrează clar ca agregat sau referință;
- pasul 2 cere schimbări în interiorul `MotorOperare`.

Nicio cale de ocolire nu devine canonică fără decizie. O singură rulare grea
o dată (ModelCheck, gate).

### Clasificarea entităților (se completează la pasul 1)

| Clasa | Familia | Observații |
|---|---|---|
| `ApplicationUserLoginInfo` | securitate | rămâne pe `BaseObject`-ul DX |
| `Document` | Document | rădăcina TPH; frunzele moștenesc |
| `DocumentDetaliu` | Editabila | compoziția documentului (`Cascade`) |
| `DviFactura` | Editabila | compoziția DVI (`Cascade` pe `Dvi`); spre factură refuz din bază |
| `Lot` | Editabila | născut de linia de intrare (L3), șters fizic cu ea; fără `Activ` |
| `Imperechere` | Editabila | legătura culeasă și ștearsă liber (31d); inversul din perioada închisă e al motorului |
| `PerioadaFiscala` | Editabila | veriga lanțului; starea o scrie motorul |
| `Societate` | Editabila | un singur rând de configurare |
| `Repartitor` (+ 5 frunze TPH) | Nomenclator | are deja `Activ`; D394 citește `!Activ` |
| `Dimensiune` (+ 6 derivate) | Nomenclator | bază CLR nemapată; `Activ` pe fiecare tabel |
| `Unitate` | Nomenclator | |
| `Imobilizare` | Nomenclator | fișa; ieșirea din gestiune e document (IMO), nu `Activ` |
| `Produs` | Nomenclator | |
| `Cont` | Nomenclator | `ICuProvenienta` |
| `ClasaProdus`, `TipMaterial` | Nomenclator | `ICuProvenienta` |
| `TipTva` | Nomenclator | are deja `Activ`; `ICuProvenienta` |
| `Judet`, `UnitateMasura`, `RandD300` | Nomenclator | de lege, seed-uite; `Activ` = abrogat |
| `TipDocument` | Politica | ancora politicilor și a motorului |
| `SetareProfil` | Politica | un rând, înghețat (51c) |
| `RegulaStoc`, `RegulaContare`, `PoliticaDiferenta` | Politica | |
| `PoliticaConex`, `PoliticaScadenta`, `PoliticaValidare`, `PoliticaTva`, `PoliticaInchidereTva`, `PoliticaNumerotare`, `PoliticaInchidere` | Politica | |
| `MapareD300`, `MapareD394`, `MapareTvaSaft`, `PoliticaMiscareSaft`, `PoliticaTvaImplicit` | Politica | |
| `PoliticaAmortizare`, `RegulaDeductibilitate` | Politica | |
| `RegistruStoc`, `RegistruContabil`, `RegistruTva`, `RegistruImobilizari` | RandRegistru | motorul le șterge fizic la corecția directă (33d) |
| `SoldPerioadaContabil`, `SoldPerioadaStoc`, `PartidaDeschisa` | RandRegistru | proiecții rescrise la închidere (ștergere fizică deja azi) |
| `InchiderePerioada` | RandRegistru | istoricul lanțului |
| `DepunereDeclaratie` | RandRegistru | scris doar de comanda de confirmare |
| `MigrareLegatura` | RandRegistru | infrastructura migrării |

## Ce rămâne deschis

- Coaja completă fără `IObjectSpace` și mutarea hook-urilor cu `os` de pe
  entitate se fac la TR-D9 (104-r2).
- Filtrarea pe `Activ` în lookup-urile documentelor noi e o regulă de L3/L4
  (104-r3). Motorul ignoră `Activ`: documentele istorice își păstrează
  referințele.
- Paginile React de culegere: le scoatem sau le generăm din metadate după PoC
  (104-r4).
- Matricea generală `tools/ProbeHttp/refuzuri.ps1` nu rulează pe bazele
  recreate la C102: cere subiecți existenți (închidere de TVA, împerechere,
  candidat DVI cu furnizor NeinregistratRo, lanț de perioade). Își creează
  singură subiecții la pornire și îi șterge la final, ca
  `comenzi-coaja.py`, înaintea review-ului advers din pasul 5: pasul 3 mută
  culegerea în L3 și atinge ușile de scriere, pe care matricea le
  regresează (104-r5, owner 2026-09-27). Închisă 2026-09-27: fixture-ul
  (furnizor NeinregistratRo, FCT operată cu plata conex și împerecherea, ITV
  draft, angajat) pe prima lună deschisă, desfăcut în `finally`; 294/294 PASS
  de două ori pe baza Privat din seed. Proba de plafon DVI (`MaiSunt=true` pe
  19k facturi) a trecut în ModelCheck, care forțează plafonul; urma
  `RefuzSeed` a rândurilor de politică de probă e declarată în antetul
  matricei.
