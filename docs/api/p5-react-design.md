# Pasul 5 — API + React: designul clientului React (design)

Stare: **FIXAT (24.07.2026) — sesiune de explorare arhitecturală; toate cele 6
tranșări confirmate; implementarea urmează.**
Contextul: acoperă mandatul din `p5-api-design.md` §8 („sesiune separată:
structura aplicației React — state management, pipeline-ul de codegen
OpenAPI→TS + captions, componente per felie"). La momentul sesiunii clientul
nu există deloc (niciun `package.json` în repo); scaffold-ul
`Atlas.Conta.BackOffice.WebApi` e în soluție, nemodificat.

Diagnosticul-ancoră al sesiunii (motivația migrării de pe XAF Blazor,
re-formulată structural): XAF e un **interpretor** — framework-ul randează,
customizarea trece prin override-uri; cât ești în vocabularul lui e magie, în
momentul în care ieși schimbi *mediul* de expresie (declarativ → hacking).
Cliff-ul de 10–30% e structural la orice interpretor propriu, nu un defect
DevExpress. Toate tranșările de mai jos aplică aceeași regulă: vocabular de
componente compuse în cod, nu limbaj de descriptori interpretat.

## 1. Tranșarea 1 — structura: felii verticale + vocabular de editoare

- **Clientul oglindește feliile verticale ale API-ului** (feature folder per
  tip de document). Nucleul partajat: tipurile generate, layer subțire de
  data access, `DocumentShell` (zona de header, grid-ul de linii, bara de
  comenzi condusă de affordances, afișarea `erori[]`), vocabularul de
  componente de câmp.
- **Granița generic/specific: metadata leagă *atributele* unui câmp,
  niciodată *identitatea* editorului.** Identitatea = cod, mereu explicită în
  JSX (`<LookupPartener camp="predatorId" />`, `<CampSuma camp="valoare" />`
  — ~100 de decizii triviale pe 11 tipuri); caption/required/maxLength =
  derivate din codegen printr-un hook (`useCampMeta` pe contextul WriteDto al
  feliei); prezența/ordinea/condiționalitatea câmpurilor = cod, per felie.
- **`CampShell`**: editorii tipizați sunt fețe subțiri peste un shell comun —
  label din caption, marker required, slot de eroare, mesaje șablonate per
  *fel* de regulă cu caption interpolat („«{caption}» este obligatoriu") —
  mesaje uniforme fără stringuri per câmp; un editor nou = doar controlul de
  input.
- **Respins `fields=[]`** (formular condus de descriptori): condiționalitatea
  („PretEvaluare doar când Directie=Plus") ar cere un limbaj de condiții —
  Appearance rules reinventate în TS; prețul Blazorului plătit treptat.
  **Respins și `<Camp>` care își alege singur editorul**: re-introduce
  registrul central (echivalentul AtlasEditorAliases) — fix locul unde
  vocabularul îngheață. Escape hatch prin construcție: o „expresie nouă" = alt
  tag în același JSX, același mediu, zero cliff.
- Nuanță: regula se aplică la ce *construim*, nu la ce *cumpărăm* — grid-ul
  DevExtreme e interpretor cu `columns=[...]`, acceptat ca insulă cu graniță
  (descriptorii de coloane stau în felie și pot consuma `useCampMeta` pentru
  captions).

## 2. Tranșarea 2 — validarea: două straturi, zero motor de reguli în TS

- **Strat 1, instant/structural**: required/maxLength/nullable din schema
  OpenAPI, randate de `CampShell` — nu pot drifta, schema *e* contractul.
- **Strat 2, autoritar**: motorul însuși, expus și ca **dry-run
  `POST .../valideaza`** — rulează fazele calculează+validează fără
  materializare (33d le-a separat deja) și întoarce aceiași `erori[]` ca
  operarea; butonul „Verifică" pe draft sau automat după PUT.
- **Respins: gemeni TS ai PoliticaValidare** — încălcarea tranșării 1 din
  designul API („o singură sursă de reguli") pe ușa din dos; dimensiunile
  obligatorii se verifică pe conturi *rezolvate*, iar rezolvarea (SursaCont,
  fallback-uri, priorități) e miezul motorului — geamănul regulii „TS nu
  calculează niciodată sold". Politica editată fără release se reflectă în
  client fără schimbare de cod, pentru că clientul nu o citește — o *simte*
  prin verdictul serverului.
- Prețul asumat: feedback-ul de politică e la salvare/verificare, nu
  as-you-type — UX corect pentru back-office contabil. Escape per felie: dacă
  o regulă anume cere feedback instant, un check explicit local (cod, trei
  rânduri), nu mecanism.
- Canalul runtime de date servește default-uri (TipTvaImplicit) și
  nomenclatoare, nu reguli de evaluat.

## 3. Tranșarea 3 — state: trei feluri, fără store global

- **(1) Stare de server, citită**: cache per cheie de query (TanStack Query
  sau echivalent) pentru ReadDto-uri și lookups; `CustomStore` DevExtreme
  legat la endpoint-urile DataSourceLoader pentru grid-urile de raportare;
  după comenzi (opereaza etc.) — invalidare + recitire; clientul nu „ține"
  niciodată un sold.
- **(2) Stare de formular, per felie**: agregatul WriteDto (header + array-ul
  de linii) ca o singură valoare locală, necomisă; PUT ca întreg.
- **(3) Efemeride de UI**: component state, mor cu componenta.
- **URL-ul = starea globală** (`/fcl/:id`, `/fise-cont?cont=…`): deep-linking
  și refresh gratis, fără store de sincronizat.
- **Grid-ul are două roluri fundamental diferite**: citire = remote,
  CustomStore, server-side totul; linii de draft = **editor de linie propriu
  cu vocabularul `Camp*`** (inclusiv lookup-urile grele — 1.679 conturi),
  grid-ul doar afișează (readonly). Grid-ul de linii NU vorbește cu serverul
  — CRUD-ul per linie a fost respins în designul API, componentele nu-l
  reintroduc.

## 4. Tranșarea 4 — codegen: tipuri, nu clienți

- **Pipeline = două dump-uri + o generare, artefacte comise, drift verificat**
  (disciplina migrațiilor EF: canonic e ce e comis, unealta verifică):
  (a) dump `openapi.json` din host (Swagger e în scaffold); (b) **dump
  captions/enums/DefaultProperty prin ModelCheck** — boot headless al
  modelului există deja acolo; ModelCheck devine verificator + emitor
  (confirmat explicit), emite JSON `{tip.membru → caption, enum → label}`
  filtrat la tipurile expuse; (c) generare types-only (`openapi-typescript`
  sau echivalent) + generator mic propriu pentru `captions.ts`.
- Artefactele generate trăiesc în `Client/src/generated/`; ModelCheck compară
  dump-ul proaspăt cu cel comis; generarea TS din dump = script determinist
  în `package.json`.
- **Respins: clienți generați** (orval, NSwag client, hooks generate) —
  interpretor cumpărat *fără graniță bună*: dictează forma hook-urilor, nu
  ajută CustomStore, lupți cu generatorul în loc să scrii fetch-ul.
  Suprafața API per felie e mică și deliberată — `api.ts` de mână per felie,
  cu tipurile generate; `WriteDto ≠ ReadDto` vizibil în numele tipurilor
  (compilatorul oprește mâna care le-ar încurca).

## 5. Tranșarea 5 — topologia: same-host, SPA servit de WebApi

- **`nou/Atlas.Conta.Client`** (Vite + React + TS), proiect frate cu
  soluția; în producție = fișiere statice servite de `Atlas.Conta.WebApi`
  (same-origin, SPA fallback); dev = Vite dev server cu proxy către WebApi.
- Release-ul e deja „pereche per client" (42f) — SPA-ul e cuplat de versiunea
  API prin tipurile generate; same-host face perechea fizică: Blazor
  back-office + WebApi-cu-SPA, două procese. **CORS moare în producție**
  (mențiunea „CORS doar pe API" din 42f = artefact al momentului, rafinată
  aici); JWT neschimbat (same-origin nu împinge spre cookies).
- Limita asumată: nu există „release doar de UI" — la deployment-per-client
  cu pereche versionată, distincția oricum nu se exploatează; un artefact per
  proces e simplificare. Separarea (origin separat/CDN, UI central
  multi-client) = aditivă, parcată împreună cu MT single-deployment.

## 6. Tranșarea 6 — lookup-uri pe OData nomenclatoare

- **Lookup-urile sunt consumatorul ușii OData opt-in pe nomenclatoare**
  (42f, decisă atunci fără consumator numit): componenta `Lookup` din
  vocabular = SelectBox + `ODataStore` nativ DevExtreme (`$filter`/`$top`/
  `$select`). Respins: proiecții custom per nomenclator doar pentru lookup —
  reconstruire manuală a drop-in-ului deja acceptat. Securitatea nu se
  diluează (OData XAF trece prin securitatea aplicației).
- **Mic vs mare = prop explicit per instanță** (`mod="local"|"remote"`), nu
  mecanism: TipTva/TipDocument = fetch integral + filtrare locală;
  conturi/parteneri = remote search cu debounce.
- **Display-ul din dump-ul de captions**: `DefaultProperty` per tip e în
  modelul XAF, se emite alături de captions. Comportamentul SmartLookup
  („match exact → auto-select") = UX al componentei, scris o dată.
- Restanță re-suprafațată: **`Lot` nu are `DefaultProperty`** (40d — sărit și
  la SmartLookup în Blazor); revine identic la pin-ul de lot pe FCL. E a
  modelului, nu a clientului — tranșarea ei rezolvă ambele UI-uri deodată.

## 7. Amânate / de verificat (documentate, nu uitate)

- **Endpoint-ul dry-run `POST .../valideaza`** = extensie a feliilor din
  designul API (comandă nouă lângă `opereaza`) — de adăugat la implementarea
  feliilor.
- **Biblioteca de formular** (React Hook Form vs hand-rolled peste context)
  — nedecisă; se alege la primul spike al `DocumentShell`/`CampShell`, cu
  criteriul: array-ul de linii + `useCampMeta` trebuie să rămână naturale.
- **`Lot.DefaultProperty`/ToString** — de tranșat în model (restanța 40d).
- Detalii standard neatinse deliberat (nu structurează): routing library,
  stocarea/refresh-ul JWT în SPA, theming DevExtreme, structura exactă a
  folderului de felie.
