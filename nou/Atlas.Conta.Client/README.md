# Atlas.Conta.Client — SPA React (pasul 5, spike 1)

Clientul React al tierului API (deciziile 42/43; contractul spike-ului:
`docs/api/p5-spike1-contract.md`, pasul 4 = D10 + D11). Felia pilot:
**NotaTransfer (BTR)** + proiecția **sold stoc**.

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
orice apel îl șterge și trimite ecranul la `/login`.

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
  nucleu/        auth · http · campMeta · formular · CampShell
                 campuri (CampText/CampData/CampNumar) · Lookup
                 DocumentShell · PanouErori · dxStore
  felii/
    btr/         api.ts · BtrLista · BtrDetaliu · EditorLinie
    stoc/        SoldStoc
  pagini/        Login
  App.tsx        rute: /login · /btr · /btr/nou · /btr/:id · /stoc
```

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
