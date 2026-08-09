# Atlas.Conta.Client — SPA React (pasul 5)

Clientul React al tierului API (deciziile 42/43). Felia pilot a spike-ului
(`docs/api/p5-spike1-contract.md`): **NotaTransfer (BTR)** + proiecția
**sold stoc**. A doua felie (`docs/api/p5-felia-fct-contract.md`, F2-D8):
**FacturaIntrare (FCT)** cu editor de linie complet + **NIR** citire/comenzi —
fluxul-ancoră FCT operată → `ConexId` → „Deschide NIR-ul generat" → Operează.
A treia (`docs/api/p5-felia-trz-contract.md`, F3-D7/D8): **trezoreria** —
plăți/încasări culese și operate, **panoul de stingeri** (imperecheri create și
șterse din UI) și **plata automată** a facturii.

## Rulare în dev

```
# 1. host-ul WebApi (Development — Swagger + baza Atlas.Conta.BackOffice.Privat)
dotnet run --project ../Atlas.Conta.BackOffice/Atlas.Conta.BackOffice.WebApi \
           --launch-profile Atlas.Conta.BackOffice.WebApi     # https://localhost:5001

# 2. clientul
pnpm install
pnpm dev            # http://localhost:5173 — /api merge prin proxy la 5001
```

Autentificare: `Admin`, parolă goală. Tokenul stă în `sessionStorage`; 401 pe
orice apel îl șterge și trimite ecranul la `/login` — pe TOATE cele trei
conducte: `http.ts` (fetch-ul feliilor), `dxStore` (grilele remote) și
`ODataStore` (lookup-urile). Ultimele două nu trec prin `http.ts`, iar fără
tratare un 401 arăta acolo ca „nimic găsit" (`nucleu/auth.ts::expiraSesiunea`).

Dev-ul trece prin **proxy-ul Vite** (`vite.config.ts`), nu prin CORS: browserul
vorbește doar cu originul Vite. În producție SPA-ul e servit static de WebApi
(same-host, 43e) — CORS nu există în niciun mediu.

## Codegen: trei artefacte comise, drift verificat (43d)

Canonic e ce e **comis**; uneltele doar reproduc — disciplina migrațiilor EF.

| artefact                     | generator                                  | sursă                     |
|------------------------------|--------------------------------------------|---------------------------|
| `src/generated/openapi.json` | `pnpm gen:openapi`                          | assembly-ul WebApi (OFFLINE) |
| `src/generated/api-types.ts` | `pnpm gen:types` (openapi-typescript)       | `openapi.json` (offline)  |
| `src/generated/metadata.json`| `dotnet run --project ../tools/ModelCheck -- --dump-metadata` | reflecție pe Module |

### Driftul openapi, verificabil fără host viu (F2-D7)

```
pnpm verifica:drift        # regenerează openapi.json + api-types.ts și cere `git diff` gol
```

`gen:openapi` rulează `scripts/gen-openapi.ps1`: build WebApi + `dotnet swagger
tofile` (Swashbuckle CLI, **dotnet local tool** din `nou/dotnet-tools.json` —
`dotnet tool restore` e făcut de script). CLI-ul construiește host-ul exact ca la
pornire, dar nu pornește pipeline-ul HTTP: `IHostedService`-urile nu rulează,
deci **warmup-ul XAF nu pornește și baza nu e atinsă** — comanda merge pe o
mașină fără Postgres. Ieșirea brută trece prin `dump-openapi.mjs`, care e acum
DOAR normalizarea + scrierea (aceeași serializare pentru ambele surse, altfel
„driftul" ar semnala diferența dintre generatoare).

Plasa rămâne: `pnpm gen:openapi:host` citește Swagger-ul din host-ul viu
(`SWAGGER_URL`, implicit `https://localhost:5001`), pentru cazul în care CLI-ul
offline s-ar rupe la o versiune viitoare.

Driftul lui `metadata.json` e verificat separat, de rularea normală a ModelCheck
(vezi mai jos); `git diff -- src/generated/` din `verifica:drift` îl prinde și
pe acela dacă a fost regenerat și necomis.

`metadata.json` = captions (`[XafDisplayName]`, fallback numele; FK-ul `{Nav}Id`
moștenește caption-ul navigației), `DefaultProperty` per tip (display-ul
lookup-urilor) și label-urile de enum. **Rularea normală a ModelCheck compară**
fișierul comis cu dump-ul proaspăt și PICĂ la drift, cu mesajul „rulați
--dump-metadata".

Limită asumată (D10): sursa e reflecția, nu Application Model-ul XAF — un
caption schimbat doar din Model Editor (`.xafml`) nu ajunge aici.
Limită asumată (swagger): Module nu are context nullable activ, deci Swashbuckle
nu emite `required[]` — toate câmpurile ies opționale în TypeScript, iar
obligativitatea se derivă din `nullable` (vezi `nucleu/campMeta.ts`).

Nu se generează **clienți** (43d): `api.ts` per felie, scris de mână peste
tipurile generate. `WriteDto ≠ ReadDto` e vizibil în tipuri.

## Structura

```
src/
  generated/     openapi.json · api-types.ts · metadata.json      (comise)
  nucleu/        auth · http · campMeta · formular · CampShell · zi
                 campuri (CampText/CampData/CampNumar/CampBifa/CampSelectie) · Lookup
                 DocumentShell · PanouErori · dxStore
                 stingeri.ts + PanouStingeri (resursă a DOCUMENTULUI, nu a unei felii)
  felii/
    fct/         api.ts · FctLista · FctDetaliu · FctEditorLinie
    nir/         api.ts · NirLista · NirDetaliu            (citire + comenzi)
    btr/         api.ts · BtrLista · BtrDetaliu · EditorLinie
    trz/         nucleul PARTAJAT al trezoreriei: api.ts (fabrică pe rută) ·
                 TrezorerieLista/Detaliu/EditorLinie · LaturaContrapartida · tipTrz
    plt/ inc/    feliile subțiri: ruta + identitatea (titluri, laturi în JSX)
    stoc/        SoldStoc
  pagini/        Login
  App.tsx        rute: /login · /fct[/nou|/:id] · /nir[/:id] · /plt[/nou|/:id]
                 /inc[/nou|/:id] · /btr[/nou|/:id] · /stoc
```

Ce exersează felia TREZORERIE peste FCT (`felii/trz`, `nucleu/PanouStingeri`):

- **două rute, un nucleu** — oglinda lui F3-D1 de pe server (`TrezorerieApply`
  generic + controllere subțiri): DTO-urile și `spreWrite` sunt în `felii/trz`,
  iar `felii/plt` / `felii/inc` dau ruta, titlurile și **laturile ca JSX** (PLT:
  cont propriu → beneficiar; INC: invers). Identitatea rămâne în cod (43a);
- **`Numar` NU se culege** (PLT/INC au politică de numerotare ⇒ server-owned) și
  **`Valoare` E culeasă** pe linie (nu există `PregatesteOperare` pe trezorerie)
  — exact invers față de FCT;
- **selectorul de fel al contrapartidei** (partener remote / angajat local): două
  lookup-uri comutate în cod, cu felul DEDUS la încărcare printr-o sondă OData
  pe mulțimea îngustă (angajații) și comutator manual ca escape hatch;
- **panoul de stingeri**: Total/Asignat/**Rest** din `StingeriDto` (serverul e
  autorul — TS nu scade nimic), imperecherile cu link spre celălalt document,
  ștergere liberă (31d) și creare din candidații cu rest ai contrapartidei
  (`/api/proiectii/documente-cu-rest`). Rolul (cine stinge pe cine) e declarat de
  felie, nu dedus: trezoreria `stinge`, factura `este-stins`;
- **plata automată pe FCT**: bifă + câmpuri condiționate în cod, iar plata
  generată apare în „Documente generate" ca link `/plt/{id}`.

Trei lucruri pe care le exersează felia FCT peste șablonul BTR, toate vizibile
în `felii/fct/api.ts` și `FctEditorLinie.tsx`:

- **numărul e CULES** (al furnizorului — FCT n-are politică de numerotare);
  serverul îl lasă nullable pe draft și îl cere la operare, clientul îl cere la
  culegere (`obligatoriu` explicit pe câmp — vezi `useCamp`);
- **`TipTvaId` face round-trip, `ValoareTva` NU.** Pe sârmă `ValoareTva`
  înseamnă „override manual" (36a): se trimite doar dacă operatorul a atins
  câmpul în sesiunea curentă de editare, altfel serverul recalculează. `TipTva`
  absent pe o linie EXISTENTĂ e golire deliberată; pe una nouă = implicitul
  tipului de document;
- **Produs → Tip, precompletat din răspunsul OData al selecției** (`laSelectie`
  pe `Lookup`, aplicat prin `seteazaMulte` ca să nu se piardă în două
  actualizări succesive) — doar când Tipul e gol. Lotul rămâne server-owned:
  se naște din `Produs` la culegere, nu apare în WriteDto.

Regula transversală (43a): **vocabular de componente compuse în cod, niciodată
descriptori interpretați.** Metadata leagă *atributele* câmpului
(caption/required/maxLength prin `useCampMeta`); *identitatea* editorului se
scrie explicit în JSX. Nu există nicio hartă „câmp → editor" și niciun
`fields=[]`. Grila DevExtreme rămâne o insulă cumpărată, cu graniță: coloanele
stau în felie și consumă `campMeta` pentru captions.

## Verdict: fără bibliotecă de formular (43 §7)

**Hand-rolled peste context** (`nucleu/formular.tsx`, ~100 de linii), NU React
Hook Form. Motivele, pe criteriul din design:

1. **Valoarea centrală a RHF nu are cui folosi aici.** Validarea autoritară e a
   motorului și vine ca `string[]` neatașat de câmp (43 §2); stratul instant e
   structural, derivat din schema OpenAPI. Nu există resolver, nu există erori
   per câmp de mapat — adică exact ce plătești când iei RHF.
2. **Agregatul e literalmente o valoare.** 43c cere „WriteDto ca o singură
   valoare locală, PUT ca întreg"; `useState<BtrWrite>` + un context care duce
   `(camp) → {valoare, seteaza, meta, eroare}` exprimă asta direct. RHF ar
   introduce un al doilea model al aceleiași stări (registry intern + `watch`),
   pe care l-am sincroniza înapoi la fiecare PUT.
3. **Array-ul de linii nu e un field array.** Liniile se editează într-un editor
   propriu și se afișează într-o grilă readonly (43c) — se împing obiecte
   întregi în `Linii`, nu se leagă inputuri indexate; `useFieldArray` ar fi
   mașinărie pentru o operație de trei rânduri.

Ce ar răsturna verdictul: formulare cu zeci de câmpuri unde re-randarea
controlată devine problemă de performanță, sau apariția unei validări per câmp
as-you-type — ambele explicit în afara direcției de azi.

## Ce NU face clientul

Nu calculează niciodată sold, total sau valoare de domeniu: `Total` e cel din
ReadDto, iar la editare nesalvată se marchează „recalculat la salvare" în loc să
fie însumat în TS. Nu ține store global (URL-ul e starea globală). Nu
interpretează erorile motorului — le afișează.
