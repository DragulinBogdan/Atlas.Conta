# Pasul 5, felia 20 — finisajul clientului (contract)

Data fixării: 2026-08-29. Input: `docs/api/lista-react.md` (integral) + itemii de
client rămași deschiși în §Închidere ale contractelor F1–F19. Deciziile de mai jos
(F20-D1…F20-D10) sunt PIN-UITE — agenții de implementare nu le redeschid; orice
nepotrivire cu realitatea codului se RAPORTEAZĂ, nu se normalizează tăcut.

## Scop

Toate tipurile de document au felie de scriere (76). Ce a rămas în client sunt
**limitele mecanismelor comune**, care reapar pe orice ecran următor, plus
**ecranele de nomenclator** cerute de feliile 15–17 și care n-au niciun precedent
în client (nu există niciun ecran de nomenclator; `http.ts` n-are `PATCH`).

Inventarul de pornire (măsurat pe cod la fixare):

| item | sursa | stare azi |
|---|---|---|
| căutarea în lookup-uri sensibilă la diacritice | 76-r6 | colație `en_US.utf8` pe toate bazele, fără `UseCollation`, fără extensii, fără coloană normalizată; clientul nu compune `$filter` (îl face `ODataStore`) |
| `Lookup` refetchează eticheta per instanță (`byKey`) | 76-r6 | `ODataStore` nativ per instanță, zero cache; `useSonda` (`nucleu/sonda.ts:31-41`) e deja șablonul „TanStack pe `(entitate, id)`" |
| etichetele 61b lipsesc la PRECOMPLETARE | 76-r6 | 10 editoare, precompletarea din lot scrie doar id-ul (`AsmEditorLinie.tsx:210-221` ș.a.) |
| `window.confirm` pe ștergerea draftului | F5, 76-r6 | 11 apariții identice în `stergeDocumentul()`; nu există componentă de confirmare, doar clasa `.cerere-data` și 5 blocuri JSX de mână |
| `Neincluse` plafonat la 200 în client | 73-r10 / 74-r7 | serverul trimite lista întreagă (`SaftProiectii.cs:1533`, `:2377`), agregarea per cauză există DEJA ca formă pe avertismente (`:1525`, `:2370`) |
| S3 fără drill-down pe cont | 74 | `SaftDiferentaCont.Cont` = SIMBOL, `FisaCont` cere GUID |
| ecranul de partener (ANAF) | 72-r9 | lipsește; DTO-urile comenzii sunt deja în `api-types.ts` |
| ecranul `Societate` | 73-r6 | lipsește; `BazeContabile` e `static readonly`, invizibilă pe orice ușă |
| `CodNc` + UM pe produs | 73-r6 | lipsește ecranul de produs; regula XAF de 8 cifre NU rulează pe API |
| ecranul `PoliticaMiscareSaft` | 74-r12 | lipsește; OData ReadOnly (56); `CoduriMiscare` invizibilă pe orice ușă |
| refuzurile gardianului pe OData ies `400 text/plain` | 70-r1 | `http.ts` caută `Erori[]` ⇒ orice refuz de nomenclator ar fi „eroare tehnică" |
| cheia de licență DevExtreme | spike 1 | zero apariții în `src/` |
| smoke vizual `/jurnal-*`, `/decont-tva` | F12 | niciodată văzute în browser |
| lookup pe linia EXISTENTĂ arată placeholder până la deschidere | F6 (ne-listat) | moștenit din BTR; se măsoară după F20-D2 |

## Deciziile

### F20-D1 — Căutarea fără diacritice = coloană GENERATĂ `Cautare`, nu colație

Trei căi posibile, tranșate:

- **Colație ICU nedeterministă** pe coloanele de denumire — respinsă: `contains`
  al OData se traduce prin `LIKE`/`strpos`, al căror suport pe colații
  nedeterministe e recent și parțial, iar colația schimbă și semantica
  egalității/indexurilor unice existente (`Cod`, `Simbol`). Prea lat pentru ce se
  cere.
- **`unaccent` + index de expresie** — respinsă: filtrul îl compune `ODataStore`
  din client și EF îl traduce mecanic; nimeni nu poate injecta `unaccent(...)` în
  jurul coloanei fără să intercepteze query-ul OData.
- **Coloană generată STORED `Cautare`** — ALEASĂ. Postgres 18 pe toate bazele.
  `Cautare = translate(lower(coalesce(Cod,'') || ' ' || coalesce(Denumire,'')),
  <de>, <la>)` — `translate` e IMMUTABLE (spre deosebire de `unaccent`), deci
  poate sta într-o coloană generată. O singură coloană acoperă căutarea pe cod ȘI
  pe denumire (operatorul tastează oricare).

Mecanica:

1. **Tabelul de normalizare = o singură sursă**, `Comun/Cautare.cs`:
   `Cautare.De` / `Cautare.La` (perechi de caractere: `ăâîșşțţ` + variantele cu
   virgulă/sedilă + majusculele NU (lower-ul le-a rezolvat) + un set restrâns de
   diacritice latine uzuale în denumiri de parteneri străini — `éèêëáàäöüçñ`), și
   funcția C# `Cautare.Normalizeaza(string)` = aceeași transformare în memorie.
   Tabelul se PUBLICĂ în `metadata.json` (F20-D6) ca TS-ul să nu-l copieze.
2. **Interfața `ICuCautare`** (`Cautare` string, get-only, `[NotMapped]` NU —
   e coloană mapată, read-only prin `HasComputedColumnSql(..., stored: true)` +
   `ValueGeneratedOnAddOrUpdate`). Declarată pe: `Repartitor` (baza TPT — o
   coloană pe tabelul de bază acoperă Partener/Gestiune/Angajat/UnitateInterna/
   ContPropriu), `Cont` (`Simbol` joacă rolul lui `Cod`), `TipMaterial`,
   `ClasaProdus`, `Produs`, `TipTva`, `Judet`, `UnitateMasura`, `CodEconomic`,
   `CodFunctional`, `SursaFinantare`, `Proiect`, `Angajament`, `TipDocument`.
   Configurarea e GENERICĂ în `OnModelCreating` (o buclă pe entitățile care
   implementează interfața, cu numele coloanei de cod luat din entitate —
   `Simbol` pe `Cont`), nu 14 blocuri copiate. `Lot` NU intră (fără denumire;
   eticheta e `[NotMapped]`).
3. Migrația `CautareFaraDiacritice` (aditivă; coloanele generate se populează
   singure). Se aplică pe TOATE bazele de dev (bugetar, Privat, ModelCheck.Privat,
   Flax.Api) — capcana din F6.
4. **ModelCheck**: pe fiecare entitate `ICuCautare`, pentru toate rândurile,
   `Cautare` (citită din SQL) == `Cautare.Normalizeaza(Cod + ' ' + Denumire)`
   (calculată în C#) — oracolul că SQL-ul și C#-ul spun același lucru. Plus
   proba pe HTTP: `GET api/odata/Cont?$filter=contains(Cautare,'clienti')`
   găsește `419`.
5. **Clientul** (F20-D2): `Lookup` caută pe `Cautare` DEFAULT când entitatea o
   are (prezența se citește din `metadata.json`), iar textul tastat se
   normalizează cu ACELAȘI tabel înainte de a pleca; `cauta` explicit rămâne
   override. `displayExpr` neschimbat.

`Cautare` e vizibilă pe OData (e coloană) — e read-only prin EF (orice PATCH pe
ea e ignorat de EF; gardianul n-are ce refuza). NU se ascunde din `$metadata`:
55g rămâne deschisă cum era.

### F20-D2 — `nucleu/odata.ts`: un singur store OData al clientului, cu `byKey` prin cache

Azi cablajul `ODataStore` e duplicat deliberat în `Lookup.tsx:66-79` și
`raportare/comune.tsx:57-71` cu clauza „a treia utilizare justifică extragerea".
A treia utilizare a venit (nomenclatoarele, F20-D8). Se extrage `nucleu/odata.ts`:

- `storeOData(entitate, optiuni)` = o SUBCLASĂ de `ODataStore` (nu `CustomStore`
  — păstrăm traducerea filtrelor DevExtreme → `$filter` a bibliotecii) care:
  (a) pune `Authorization` + tratarea 401 (o singură dată, nu de trei ori);
  (b) suprascrie **`byKey(id)`** ca să treacă prin `queryClient.fetchQuery` cu
  cheia `['nomenclator', entitate, String(id)]`, `staleTime: Infinity` — o
  rezolvare per valoare, partajată între toate widget-urile și toate ecranele
  din sesiune; (c) în `beforeSend`, pentru `$filter`-ul de căutare pe `Cautare`,
  normalizează literalul (F20-D1.5) — punctul de decuplare e că `searchExpr` e
  al nostru, deci știm exact ce literal rescriem; dacă rescrierea prin
  `beforeSend` se dovedește fragilă (agentul o probează pe cererea reală),
  alternativa pin-uită e `CustomStore` cu `load` propriu — dar NU ambele.
- `useNomenclator<T>(entitate, id)` = hook peste aceeași cheie de cache (pentru
  etichete în afara unui `Lookup`); `citesteNomenclator(entitate, id)` =
  varianta imperativă (`fetchQuery`), pentru precompletare (F20-D3).
- `useSonda` rămâne (semantica „e în SET" ≠ „citește"), dar se mută în același
  fișier.
- `Lookup` și `useSursaConturi` consumă `storeOData`; `dxStore.ts` (grilele pe
  REST) NU e atins — e altă conductă (aspnet-data), nu OData.

Măsura de închidere: deschiderea ecranului NTC face **1** `GET UnitateInterna(<guid>)`
per guid distinct (azi 8); navigarea între două documente cu aceeași latură face 0.

### F20-D3 — Precompletarea scrie perechea (id, etichetă) prin cache, nu prin `$expand` imbricat

Cele două jumătăți din raport: (1) sursa etichetei, (2) scrierea atomică. Pin:

- Sursa = `citesteNomenclator('TipMaterial', tipId)` din F20-D2, NU `$expand`
  imbricat pe `Lot` (nomenclator mare, mod remote — costul ar cădea pe fiecare
  tastare în lookup-ul de lot).
- Un helper de nucleu, `nucleu/etichete.ts`: `precompleteazaTip(...)` — sau
  forma generală pe care o cere codul — aplică guard-ul „doar dacă e gol" pe
  AMBELE părți deodată (azi `setLinie` are update funcțional pe `prev`, iar
  `setEtichete` citește `linie` din closure — cele două pot diverge; unificarea
  lor e conținutul acestei decizii).
- Se aplică în TOATE precompletările din lot: ASM, RLF, RDC, BCS, BTR, LDI (și
  oriunde mai găsește agentul același șablon). `EticheteCulese` rămâne per
  editor (tipurile diferă); mecanismul per poziție (61b) nu se schimbă.

### F20-D4 — `ConfirmareInline` în nucleu + slot în `DocumentShell`; `window.confirm` dispare

Componenta `nucleu/ConfirmareInline.tsx` cu props
`{ intrebare: ReactNode; verb: string; ocupat?: boolean; onConfirma; onRenunta }`,
extrasă din șablonul canonic (`PanouStingeri.tsx:286-297`). `DocumentShell`
capătă un slot `confirmare?: ReactNode` randat în același loc ca `cereData`
(o singură cerere în așteptare la un moment dat — dacă operatorul deschide
alta, prima se închide). Cele 11 `window.confirm` din `stergeDocumentul()` trec pe
el; cele 5 blocuri JSX de mână (`PanouStingeri` ×2, distribuirea ASM, `FclDetaliu`
DateBox) trec pe componentă DOAR dacă nu au conținut de formular propriu
(cele cu `NumberBox`/`DateBox` rămân cum sunt — nu se generalizează un slot de
formular pentru două cazuri).

### F20-D5 — `Neincluse` agregat per cauză PE SERVER; lista întreagă rămâne în `SaftDto`

Închide 73-r10 + 74-r7 cu forma deja scrisă pentru avertismente:

- `SaftDto.Neincluse` (fișierul/proiecția completă) rămâne lista PLATĂ — nimic
  nu se pierde (73e).
- `SaftSumarDto.Neincluse` devine `NeinclusAgregat[]`:
  `{ Cauza, Numar, Randuri, Suma (L: Σ|Baza| ; S: Σ Valoare), Cantitate (S),
  Exemple: SaftNeinclus[] ≤ 20 }` — exemplele sunt PRIMELE după ordinea de azi
  a listei (determinist). Cusătură nouă în ModelCheck: Σ `Numar` == `Neincluse.Count`
  al DTO-ului complet, pe L și pe S.
- `SaftDiferentaCont` capătă `ContId` (GUID-ul contului), ca S3 să poată trimite
  la fișă.
- Clientul (`Saft.tsx`): tabelul plat + plafonul 200 mor; un rând per cauză cu
  `<details>` pe exemple (șablonul `Avertismente`), textul „primele N din M";
  în S3 simbolul contului devine link `urlCu('/fisa-cont', { contId, … })` pe
  perioada lunii.

### F20-D6 — `metadata.json` capătă secțiunea `Nomenclatoare`: listele legii care sunt COD

`BazeContabile` (Societate) și `CoduriMiscare` (SaftReguli) sunt liste `static
readonly` pe tipuri C# — „lege în cod" (decizia 73d/74b) — invizibile pe OData,
metadata și REST. NU devin entități (ar contrazice motivația scrisă în
`SaftReguli.cs:311-316`). `MetadataDump` capătă a treia secțiune, `Nomenclatoare`,
declarată EXPLICIT (o listă de `(nume, valori)` în `MetadataDump`, nu reflecție pe
statice — prezența în dump e o decizie per listă, ca `[TipDetaliu]`):

- `BazeContabile: [{ Cod, Descriere }]` (descrierile din comentariul clasei
  devin date — un `IReadOnlyList<(string Cod, string Descriere)>`);
- `CoduriMiscare: [{ Cod, Denumire }]`;
- `Cautare: { De, La }` (F20-D1.1).

Clientul: `campMeta.ts` capătă `nomenclator(nume)`. Driftul e prins de gardul
existent al ModelCheck.

### F20-D7 — 70-r1 se închide: refuzurile de domeniu pe OData ies `422 EroriDto`

Blocantul latent al oricărui ecran de nomenclator: `GardianEditare` aruncă
`UserFriendlyException`, care pe rutele OData iese `400 text/plain`. Se
aliniază cu regula 70f („un singur 400 = `EroriDto`") extinsă: pe rutele
`api/odata/*`, `UserFriendlyException` ⇒ `422 { Erori: [mesaj] }` JSON,
`ConstraintViolation` idem (60a). Filtrul e al WebApi (nu al XAF), instalat pe
pipeline-ul OData; `$metadata` și query-urile neatinse. Proba pe HTTP:
`PATCH api/odata/Partener({id})` cu `DataSincronizareAnaf` ⇒ 422 JSON cu textul
de azi; `POST` al doilea `Societate` ⇒ 422. Clientul: `http.ts` capătă
`modifica` (PATCH, `application/json`, `Prefer: return=representation` opțional)
și tratează 422 exact ca pe REST.

### F20-D8 — Ecranele de nomenclator: Partener, Societate, Produs; `PoliticaMiscareSaft` DOAR citire

Primul șablon de ecran de nomenclator în client — `felii/nomenclatoare/`:

- **Șablonul**: listă `DataGrid remoteOperations` pe OData (prin `storeOData`
  din F20-D2 — o singură conductă) + detaliu = `Formular` + `Camp*`/`Lookup` pe
  entitatea OData; scrierea prin `POST`/`PATCH` OData (F20-D7); rutele
  `/<nume>`, `/<nume>/nou`, `/<nume>/:id` (ca la documente, 43c); URL = starea
  filtrelor. Captions din `metadata.json` (`campMeta(TIP, membru)` fără schemă
  OpenAPI — OData nu e în `openapi.json`; `required`/`maxLength` vin din
  `metadata.json` DOAR dacă dump-ul le are — agentul verifică și, dacă lipsesc,
  `MaxLength` intră în dump (`Lungimi` există deja pe server, 72a) în loc să fie
  hardcodat în TS).
- **Partener** (72-r9): grupul de adresă cu `Lookup` pe `Judet` activ DOAR pe
  `Tara == RO` (afordanță; gardianul refuză altfel — 422 prin F20-D7);
  `DataSincronizareAnaf`/`InactivFiscal` readonly; butonul „Sincronizează din
  ANAF" = `POST api/parteneri/{id}/sincronizeaza-anaf` cu rezultatul ca listă
  (`Modificari` vechi/nou, `Diferente` cules/ANAF, `Avertismente`; `Camp` se
  traduce prin captions — regula din `ParteneriDtos.cs:14-16`), `suprascrie` ca
  opțiune explicită confirmată cu `ConfirmareInline` (F20-D4); 503 ⇒ „ANAF n-a
  răspuns, reia" + buton de reluare. Comanda de LOT nu intră (nu există listă cu
  selecție multiplă în client; restanță).
- **Societate** (73-r6): un singur ecran `/societate` (fără listă, fără `/nou`):
  citește `GET api/odata/Societate?$top=1`; dacă nu există rând ⇒ formular gol
  cu `POST`, altfel `PATCH`. `BazaContabila` = `CampOptiuni` din
  `nomenclator('BazeContabile')`; `ContBancar` = `Lookup` pe `ContPropriu` cu
  `filtru=['EsteBanca','=',true]`; `RaporteazaCnp` = `CampBifa` cu textul de
  confidențialitate lângă.
- **Produs** (73-r6): listă + detaliu cu `Cod`, `Denumire`, `TipMaterial`
  (lookup), `UM` (text, 73-r5 rămâne), `UnitateMasura` (lookup REMOTE, afișare
  `Cod — Denumire`, căutare pe `Cautare`), `CodNc` cu validare de formă
  `^\d{8}$` în client (regula XAF nu rulează pe API — 55b; gardianul NU capătă
  regula, e format, nu fond; DUK rămâne oracolul).
- **`PoliticaMiscareSaft`** (74-r12): regula 56 („politicile = ReadOnly pe
  OData") NU se redeschide — ecranul `/politici/miscare-saft` e o GRILĂ DE
  CITIRE (`TipDocument × TipStoc × Semn → cod + denumirea codului din
  `nomenclator('CoduriMiscare')` + rol + motiv`), cu textul „se editează în
  BackOffice". Editarea politicilor din React = decizie separată, cu nume
  (F20-r).

Meniul: grupul „Nomenclatoare" (Parteneri, Produse, Societate) și „Politici"
(Mișcări SAF-T) în `Cadru`.

### F20-D9 — Licența DevExtreme = acțiunea utilizatorului, cablajul e al feliei

`main.tsx`: `config({ licenseKey: import.meta.env.VITE_DEVEXTREME_LICENSE ?? '' })`
înainte de orice widget; `.env.local` gitignored; `.env.example` documentează
cheia. Fără cheie build-ul merge (watermark-ul de trial e tolerat pe dev).

### F20-D10 — Regula de oprire + ce NU intră

Pașii se închid de sine, în ordinea de mai jos; un pas blocat devine restanță cu
nume, restul se livrează. NU intră: comanda ANAF de lot, editarea politicilor
din React, satelitul partenerilor (34g), nomenclatorul NC8 (73-r4), `Produs.UM`
string (73-r5), `HeaderFilter` la 100 (server), cele 8 dimensiuni fără UI,
`AngajamentId` fără lookup (tabela e goală).

## Pașii

1. **Server A** — F20-D1 (`Cautare.cs`, `ICuCautare`, `OnModelCreating` generic,
   migrația, ModelCheck oracol + HTTP) + F20-D6 (`Nomenclatoare` în dump) +
   F20-D7 (422 pe OData). ModelCheck verde ambele profiluri; migrația aplicată
   pe toate bazele de dev; `--dump-metadata`.
2. **Server B** — F20-D5 server (`NeinclusAgregat`, `ContId`, cusătura Σ în
   ModelCheck, `openapi.json` regenerat).
3. **Client nucleu** — F20-D2, F20-D3, F20-D4, F20-D9. `pnpm build`; măsura
   NTC (1 cerere per guid).
4. **Client SAF-T** — F20-D5 client.
5. **Client nomenclatoare** — F20-D8 (Partener, Societate, Produs, politica).
6. **Smoke pe calea reală** (browser + WebApi pe Privat): fiecare ecran nou,
   ștergerea unui draft prin `ConfirmareInline`, căutarea „clienti" ⇒ 419,
   precompletarea ASM cu eticheta pe grilă, `/jurnal-cumparari` /
   `/jurnal-vanzari` / `/decont-tva` văzute (datoria F12), lookup-ul pe linia
   existentă (placeholder-ul din F6 — se măsoară și se închide sau se declară).
7. **Review advers** + decizia 77 (jurnal + CLAUDE.md + roadmap + lista-react
   curățată de itemii închiși).

Comenzile de verificare (identice cu F19 §Închidere; `verifica:drift` cere
WebApi OPRIT; niciodată `--no-build` la `dotnet ef`):

```
cd nou/Atlas.Conta.BackOffice/Atlas.Conta.BackOffice.Module
dotnet ef migrations add CautareFaraDiacritice --context BackOfficeEFCoreDbContext
dotnet ef database update --context BackOfficeEFCoreDbContext
dotnet ef database update --context BackOfficeEFCoreDbContext --connection "...Database=Atlas.Conta.BackOffice.Privat"
dotnet ef database update --context BackOfficeEFCoreDbContext --connection "...Database=Atlas.Conta.Import1C.Flax.Api"
cd nou/tools/ModelCheck && dotnet run            # bugetar
cd nou/tools/ModelCheck && dotnet run privat     # privat (migrează singur)
dotnet run --project nou/tools/ModelCheck -- --dump-metadata
cd nou/Atlas.Conta.Client && pnpm verifica:drift && pnpm build
```

## Închidere

(se completează la pasul 7)
