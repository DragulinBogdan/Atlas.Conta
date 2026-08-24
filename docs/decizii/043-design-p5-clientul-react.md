# Decizia 43 — Design pasul 5 — clientul React

- **Data**: 2026-07-24 (primul commit în jurnal)
- **Stare**: activă
- **Rezumat durabil**: `CLAUDE.md` §43
- **Docs**: docs/api/p5-react-design.md

---

**Design pasul 5 — clientul React — FIXAT**
(`docs/api/p5-react-design.md`, toate cele 6 tranșări confirmate
24.07.2026; închide mandatul §8 din designul API; implementarea urmează).
Regula transversală: vocabular de componente compuse în cod, NICIODATĂ
limbaj de descriptori interpretat (cliff-ul 10–30% al XAF e structural la
orice interpretor propriu). Esența: (a) **felii verticale + vocabular de
editoare** — `DocumentShell` + `CampShell`; metadata leagă *atributele*
câmpului (caption/required/maxLength prin `useCampMeta` din codegen),
codul decide *identitatea* editorului (tag explicit în JSX) și
prezența/ordinea/condiționalitatea; respins `fields=[]` și `<Camp>`
auto-resolve (registry = vocabular înghețat); escape hatch = alt tag în
același JSX. (b) **validare pe două straturi, zero motor de reguli în
TS**: instant/structural din schema OpenAPI + autoritar = motorul, expus
și ca dry-run `POST .../valideaza` (calculează+validează fără
materializare — 33d); respins gemenii TS ai PoliticaValidare („o singură
sursă de reguli"). (c) **state pe trei feluri, fără store global**:
server-read (TanStack Query + CustomStore pe DataSourceLoader), formular
per felie (agregatul WriteDto local, PUT ca întreg), efemeride UI; URL =
starea globală; linii de draft = editor de linie propriu cu vocabularul
`Camp*`, grid readonly — fără CRUD per linie prin componente.
(d) **codegen: tipuri, nu clienți** — dump OpenAPI + dump
captions/enums/DefaultProperty prin ModelCheck (devine verificator +
emitor), artefacte comise în `Client/src/generated/` cu drift verificat
(disciplina migrațiilor); respins orval/NSwag client; `api.ts` de mână
per felie. (e) **topologie same-host**: `nou/Atlas.Conta.Client`
(Vite+React+TS) servit static de WebApi (SPA fallback), dev prin Vite
proxy; CORS moare în producție; release rămâne pereche per client.
(f) **lookup-uri pe OData nomenclatoare** (consumatorul ușii opt-in 42f):
`Lookup` = SelectBox+ODataStore, mod local/remote ca prop explicit,
display din DefaultProperty emis în dump. Amânate: biblioteca de formular
(la primul spike), `Lot.DefaultProperty` (restanța 40d, a modelului).
