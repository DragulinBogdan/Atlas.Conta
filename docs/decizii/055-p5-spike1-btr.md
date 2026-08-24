# Decizia 55 — Pasul 5, spike 1 — fir complet subțire API+React pe NotaTransfer

- **Data**: 2026-08-08 (primul commit în jurnal)
- **Stare**: activă
- **Rezumat durabil**: `CLAUDE.md` §55
- **Docs**: docs/api/p5-spike1-contract.md

---

**Pasul 5, spike 1 — fir complet subțire API+React pe NotaTransfer —
executat** (contract + închidere: `docs/api/p5-spike1-contract.md`,
D1–D12; smoke browser end-to-end pe baza Privat + review advers cu toate
fix-urile aplicate). Designurile 42/43 au trecut proba; tranșările care
rămân adevărate pentru TOATE feliile următoare:
(a) **Gardianul de Committing (42a) există și e seam-ul corect**:
`GardianEditare : IObjectSpaceCustomizer` (scoped, înregistrat în AMBELE
host-uri prin `AddContaGardianEditare` — modulele XAF nu pot înregistra
servicii; `ObjectSpaceCreated` ar fi acoperit doar Blazor). Acoperă exact
familia secured (`IObjectSpaceFactory` + View-urile Blazor); non-secured/
updating/standalone rămân ușa de sistem. Reguli: documente read-only
post-Draft pe starea ORIGINALĂ (EF OriginalValues), câmpurile server-owned
păzite integral (Stare, DataOperare, Autogenerat, DocumentSursaId; Numar la
tipurile cu PoliticaNumerotare — `AsignaNumar` onorează numărul
pre-completat, legitim doar la re-operare), liniile nu se re-parentează,
registrele exclusiv ale motorului, Imperecherea prin `ValideazaCreare`
(logica din ImperechereController migrată; controllerul = doar UX).
(b) **Comenzile = OperareApi prin ID** (`OperareRezultat{DocumentId,
StareNoua, ConexId?, Mesaje[]}`), rulate în OS NON-secured propriu, cu
**gate de autorizare ÎNAINTE** (review advers F1: secured/non-secured
răspunde la „cum scrie motorul", nu la „cine comandă" — documentul se
rezolvă prin OS secured, 404/403/`CanWrite`, apoi ușa);
`DocumentOperareController` migrat pe aceeași secvență (commit culegere →
comandă → `Refresh` prin ID). `MotorOperare.Valideaza` = dry-run (fazele
calculează+validează extrase în `CalculeazaSiValideaza`; `Opereaza`
neschimbat comportamental — ModelCheck a rămas verde fără nicio ajustare).
(c) **Validarea XAF NU rulează pe tierul API** (PersistenceValidation e
per-View; probă pe surse + empiric — nota 40b închisă): apply-ul fiecărei
felii rezolvă FK-urile prin GetObjectByKey cu mesaje de domeniu; erori =
`422 {Erori[]}` traduse în controller (filtrul DX = text brut, inutilizabil).
(d) **Felia BTR = șablonul**: DTO+Apply+proiecții în Module (testabile în
ModelCheck — +23 verificări, inclusiv consistența proiecției ==
`StocService.Sold` și „dry-run-ul nu materializează nimic"), controllere
subțiri în host, DataSourceLoader (DevExtreme.AspNet.Data; binder copiat —
nu e în pachet; LoadResult se MATERIALIZEAZĂ înainte de dispose-ul OS-ului;
plafon take 100/500). OData opt-in doar Gestiune/TipMaterial/Lot, cu
**Lot read-only** (`ConfigureController().ReadOnly()` — PretUnitar/Data/
Gestiune sunt load-bearing pentru evaluare).
(e) **Audit sub OS non-secured: atribuie corect utilizatorul** din scope-ul
request-ului (verificare empirică 42 §8 închisă, pe baza vie).
(f) **Clientul `nou/Atlas.Conta.Client`** (Vite+React+TS, pnpm; devextreme
26.1.4): vocabular CampShell/editori/Lookup(ODataStore, mod local/remote
explicit)/DocumentShell pe affordances; **formular hand-rolled pe context —
verdictul D11** (RHF respins: erorile autoritare sunt `Erori[]` neatașate
de câmp, agregatul e o valoare unică, liniile nu-s field array); codegen =
openapi→TS + `ModelCheck --dump-metadata` (reflecție: captions RO/enums/
DefaultProperty; FK moștenește caption-ul navigației; drift verificat la
rularea normală). Host hardening: bootstrap `ScaraBootstrap` partajat,
`DisableUpdateSchema` + fără updater în WebApi, paritate module,
`PermissionsReloadMode.NoCache` aliniat.
(g) Datorii documentate în contract §Închidere: driftul openapi.json
neverificat (doar metadata e sub ModelCheck), finisajul clientului (linii
nesalvate, licența DevExtreme, 401 în grile), JWT secrets la deploy,
`$metadata` expune forma întregului model, `Lot.Eticheta [NotMapped]` nu
trece prin OData ($expand=Produs ca workaround — restanța 40d revine pe
sârmă).
