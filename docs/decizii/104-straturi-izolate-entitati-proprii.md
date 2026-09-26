# 104 — Straturi izolate, culegerea în XAF, entități proprii fără ștergere amânată

Data: 2026-09-26
Stare: activă; amendează 042(b)(e) și 043 (aria React); depășește constatarea 60a și F13-D2 (070) privind ștergerea amânată
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
dacă navigația e compoziție (liniile documentului, rândurile proprii ale unei
politici); în rest are `Restrict`. Refuzul unui FK la ștergere iese ca 422 de
domeniu pe toate ușile (80), cu obiectul care ține referința numit.

**(h) Proba.** Catalogul de scenarii și ModelCheck, verzi pe ambele
profiluri. ModelCheck probează structural că:
- modelul EF nu are proprietatea `GCRecord` pe niciun tip de domeniu;
- `Cascade` apare doar pe compoziții;
- `RandRegistru` nu are câmp de blocare optimistă.

Refuzul FK se probează prin HTTP (`refuzuri.ps1`) și în browser pe XAF.

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

**Pasul 2 — coaja comenzii (b), partea de azi.** `OperareApi` (sau
succesorul lui) nu mai primește `IObjectSpace` de la apelant. Primește o
fabrică de context și identitatea, și verifică explicit dreptul de comandă.
Se adaptează `DocumentOperareController`, controllerele WebApi și ModelCheck.
`CumulPerioade` nu mai întreabă de `ISecuredObjectSpace`. Interiorul lui
`MotorOperare` nu se atinge (TR-D9).

**Pasul 3 — culegerea unică (c).**
- Inventariezi căile de precompletare și recalcul din XAF față de cele din
  `Apply`/`ImpliciteApply`.
- Mute logica în servicii L3; controllerele XAF devin adaptori.
- O diferență de comportament (nu doar de formă) între două căi se raportează
  înainte de unificare.

**Pasul 4 — aria React (d).** Actualizezi `stare-curenta/api-si-client.md`,
`docs/api/lista-react.md` și principiile din `CLAUDE.md` (straturile,
culegerea în XAF). Paginile de detaliu pentru documente se marchează înghețate.

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

## Ce rămâne deschis

- Coaja completă fără `IObjectSpace` și mutarea hook-urilor cu `os` de pe
  entitate se fac la TR-D9 (104-r2).
- Filtrarea pe `Activ` în lookup-urile documentelor noi e o regulă de L3/L4
  (104-r3). Motorul ignoră `Activ`: documentele istorice își păstrează
  referințele.
- Paginile React de culegere: le scoatem sau le generăm din metadate după PoC
  (104-r4).
