# Decizia 23 — Felia 3a

- **Data**: 2026-07-22 (primul commit în jurnal)
- **Stare**: activă
- **Rezumat durabil**: `CLAUDE.md` §23

---

**Felia 3a — executată; deciziile fixate la implementare:**
(a) **Schema = migrații EF Core, canonic.** `BackOfficeDesignTimeDbContextFactory`
în Module (Postgres localhost:5444, oglindește appsettings); update-ul automat
de schemă XAF e DEZACTIVAT (`SchemaUpdateOptions.DisableUpdateSchema`).
Seed-ul (ModuleUpdater) se rulează cu
`dotnet run --project ...Blazor.Server -- --updateDatabase --forceUpdate --silent`;
în DEBUG updater-ul rulează și fără debugger atașat.
(b) **`ClasaProdus.Natura`** (enum `NaturaClasa`: Stoc/Serviciu/Cheltuiala/
Imobilizare/Tehnica) — curățarea din inventar 10 §2: doar Natura=Stoc intră
în regulile de stoc; clasele tehnice (TVA, diferențe) doar la contare.
(c) **NotaTransfer nu are RegulaContare** — la plan sintetic transferul nu
mișcă conturi; mutarea între gestiuni trăiește în registrul de stoc și în
dimensiunea Repartitor (notele 3xx=3xx din legacy = zgomot, nu se preiau).
(d) **Planul de conturi se seed-uiește integral din CPLAN** (1.679 sintetice,
CSV embedded `DatabaseUpdate/SeedData/plan-conturi.csv`, diacritice
normalizate); defalcarea BALANTA → `DimensiuniObligatorii` cu legenda
CPLAN_DEFALCARE (R/M/F/E/B/P; T=Titlu → CodEconomic).
(e) Utilitar `nou/tools/ModelCheck` (consolă): validare model + verificare
migrații/seed contra bazei.
