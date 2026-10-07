# TR-D9a, pasul 7 — eliminarea atomică a declarațiilor și lista cubului pe `PostareVizual`

Data: 2026-10-07. Execuție: agent Opus (`d9-pas7`, spec și raport în `run-nucleu/tr-d9a/pas7/`, gitignored);
verificare, documentația și commit-ul per jalon: main. Contractul: D9-D6, D9-D7 (b), D9-D8, D9-D9 (amendat),
D9-D12, amendamentul 3 (D9-A12). Două jaloane, ambele făcute la 2026-10-07.

## Jalonul 7.1 — declarațiile dispar atomic

### Ce a dispărut

- Entitățile `RegistruContabil`, `RegistruStoc`, `RegistruTva` (`BusinessObjects/Registre/Registre.cs`) și
  `RegistruImobilizari` (`RegistruImobilizari.cs`), cu DbSet-urile, aliasul `RegistruContabilEntitate`,
  indecșii lor, maparea și bucla `AutoInclude` pe cele 16 navigații plate. `RandRegistru` rămâne: baza lui
  `DepunereDeclaratie`, `SoldPerioadaContabil`, `SoldPerioadaStoc`, `PartidaDeschisa`, `InchiderePerioada`,
  `MigrareLegatura`.
- Coloanele `Documente.TotalStingere`, `TipuriDocument.PosteazaInCub`, `TipuriDocument.LaturaContPropriu`.
  Niciun enum nu moare: `LaturaDocument` (`RegulaStoc.Latura`, SAF-T plăți), `SensTva`, `FelMiscareImobilizare`,
  `MetodaAmortizare`, `CategorieFiscala`, `TipStoc` au alți consumatori.
- Cele patru cazuri de registru din `GardianEditare.Verifica`; `TotalStingere` din cele două gărzi ale
  documentului și din lista de excludere a corecției.
- Listele XAF vechi: cele trei din `AscundeFkuriBrute`, lista registrului din `Imobilizari`, cele trei noduri
  `ListView` din `Blazor.Server/Model.xafml`.
- `HCategory` din `DbContext` și `AdditionalExportedTypes` (fără consumator; partea de `HCategory` a 106-r5 e
  făcută, StateMachine/ViewVariants rămân).
- Seed: `SeedTipuriDocument` nu mai scrie cele două câmpuri; `RegulaStoc` pierde rândurile ASM, BCS, BTR, NIR
  pe ambele profiluri și RDC, RLF pe privat (bugetar 17 → 3, LDI; privat 19 → 4, LDI și DSC). „Explică” pe un
  tip golit răspunde valid, fără reguli de stoc (`D9-D8-EXPLICA`, pe BCS, ambele profiluri).
- ModelCheck: 155 de purje `GetObjectsQuery<Registru…>` din 59 de fișiere, 5 variabile orfane, montajele
  `tipStoc` nefolosite; lista nominală X-D2 redusă la `Imperechere` (`ProbeCititoriRegistre.Lista.cs`).
- ProbeHttp: rolul `FaraRegistre` din `postare-restrictii.py` și rândul lui din `refuzuri.ps1`.

### Migrațiile

`Migrations/` = `20261007072615_InitialCreate` + snapshot. SQL-ul de mână din lanțul vechi (partițiile, PK
compus, FK-urile per partiție, cei 13 indecși inclusiv ai cititorilor, constrângerile explicației, funcția
`cub_partida_id`) e pliat la sfârșitul lui `Up`; `Down` începe cu `DROP FUNCTION` și `DROP TABLE "Postare"`.
`Tranzactie` cu explicația iese din model cu aceleași nume. `has-pending-model-changes` curat; `STR-SCHEMA`
verde. Comparația structurală cu baza veche: în afara obiectelor scoase diferă numai `DEFAULT false` pe 17
coloane booleene adăugate prin `AddColumn` (EF scrie mereu valoarea). Linia de date din `IntervaleTvaSiAvans`
nu se pliază: baza e greenfield.

### Proba numelor interzise (D9-D7 b)

`ProbeCititoriRegistre.NumeInterzise.cs`, în `--probe-sursa`: 737 de fișiere C# și TypeScript din `nou/`,
zero referințe; zece mutanți (interogare, literal SQL, membru scos, identificator TypeScript cad; comentariu,
`RegistruImobilizariDto`, cod generat, specificator de modul nu cad). Lista excepțiilor rămâne închisă
(fișierul probei, `src/generated/`): componenta paginii de raport s-a redenumit `PaginaRegistruImobilizari`,
fișierul `RegistruImobilizari.tsx` își păstrează numele (D9-D12). Comentariul cu `StocService` din
`Nucleu.Teste/EvaluareTeste.cs` nu e referință de cod.

### Driftul clientului (D9-D12), nominal

`openapi.json` și `api-types.ts`: ies `PosteazaInCub` și `LaturaContPropriu` din cele două scheme
`TipDocument`. `metadata.json`: ies cele patru tipuri de registru, cele două câmpuri de pe `TipDocument` și
`TotalStingere` din 20 de liste de membri. `totalStingere` nu era în `openapi.json`. Clientul nu citea niciun
câmp; `tsc -b` verde; regenerarea e stabilă.

### Cifre

| Ce | Valoare |
|---|---|
| Integrala privat (`Atlas.Conta.ModelCheck.Privat.D9P7`) | 4.784 OK / 0 FAIL (linia de bază 4.784) |
| Integrala bugetar (`Atlas.Conta.BackOffice.D9P7`) | 3.505 OK / 0 FAIL (linia de bază 3.505) |
| Nucleu | 180 / 180 |
| `--probe-sursa` | 11 / 0 |
| `--dump-integritate-tph` | 111 interogări, 0 încălcări |

Pe fiecare profil 3 aserții mor ca formă (`D85-R1` pe `RegistruStoc_ListView`, `F27-R3e`, a doua X-D2) și 3
probe apar (numele interzise, mutanții lor, `D9-D8-EXPLICA`); `D9-P4-ROL-1` trece pe rolurile care citesc
`Document`; D17-V1 pe mișcările cubului (limita consemnată în `limite-curente.md`); `PAR-V16` și `F27-R7a`
pierd clauza `TotalStingere == null`; restul diferențelor sunt texte 1:1.

### Bazele

Integrala și verificarea main-ului au rulat pe baze noi din `InitialCreate` cu sufixul `.D9P7`. Cele trei
baze de dezvoltare (`Atlas.Conta.BackOffice`, `Atlas.Conta.ModelCheck.Privat`, `Atlas.Conta.BackOffice.Privat`)
se recreează de owner (102b); clonele de import și de perf (`.Flax.R3f`, `.D9P5b`, `.D9Vol`) rămân pe schema
veche, Import1C cere `--recreeaza` la pasul 8.

## Jalonul 7.2 — lista de evidență a cubului pe `PostareVizual` (D9-A12)

### Ce s-a făcut

- `Module/Cub/PostareVizual.cs`: entitate de citire fără `BaseObject`, `[NavigationItem("Registre")]`,
  `[ForbidCRUD("ListView", "DetailView")]`, caption „Postări”; cele 45 de proprietăți mapate ale lui `Postare`
  cu aceleași nume și tipuri, plus `TranzactieFel`, `DocumentNumar`, `ContSimbol`, `PartenerCod`, `GestiuneCod`,
  `ProdusCod`, `UnitateCod`, `TipTvaCod`, `CodFunctionalCod`, `CodEconomicCod`, `SursaFinantareCod`,
  `UnitateOrganizatoricaCod`, `ProiectCod`, `CentruCostCod`. Maparea `ToView("PostareVizual")`, `HasKey(ID)`,
  conversiile enum ca pe `Postare`, DbSet `PostariVizuale`. `Postare` pierde `NavigationItem`.
- Migrația `20261007080233_PostareVizual`: `CREATE VIEW` peste `"Postare"` cu `LEFT JOIN` pe `Tranzactie`,
  `Documente`, `Conturi`, `Repartitori` (partener, gestiune, centru de cost), `Produse`, `Loturi` (pe
  `FelUnitate = Stoc`), `Imobilizari` (pe `Fisa`), `TipuriTva`, dimensiunile bugetare; `CASE`-ul gestiunilor
  virtuale generat la rulare din proprietățile statice `Guid` ale `N.GestiuniVirtuale`; `UnitateCod` = cod
  produs + data lotului / cod partener + `UnitateDeschisa` / număr de inventar; fără subinterogare corelată;
  `DROP VIEW` în `Down`. `Up` și `Down` rulate pe ambele baze `.D9P7`.
- `ContaUiBaseline.Postari`: lista navigabilă pe `PostareVizual` cu cele 12 coloane în ordinea D9-A12;
  `Tranzactie_Postari` rămâne pe `Postare`. `Blazor.Server/Model.xafml`: `PostareVizual_ListView` în `ServerView`,
  probat pe modelul real al hostului (`D9-P4-LISTA-1`); `ToSqlQuery` n-a fost necesar.
- Gardul la activare: `Cub/Citiri/Vizibilitate.AccesLista(os, securitate)` = `AccesComplet.Lipsuri(os,
  securitate, [typeof(Postare)])`; `Controllers/PostareVizualController` (`ObjectViewController<ListView,
  PostareVizual>`) pune `Criteria["Acces"] = 1 = 0` și arată fraza porților cu lipsurile.
- Gardianul: `case Cub.PostareVizual` în ramura cubului din `GardianEditare.Verifica`.
- `neexpunere-cub.py` primește `PostareVizual` în `TIPURI`.

### Divergențe față de D9-A12, declarate

- `ValutaCod` lipsește: nu există nomenclator de valută (`Postare.Valuta` e un Guid fără tabelă), deci
  nu are ce îmbina; o coloană mereu nulă ar fi mințit.
- `JOIN "Tranzactie"` e `LEFT JOIN`, ca celelalte, după măsurătoare: cu join intern `COUNT(*)` pe view costa
  2.276 ms față de 502 ms pe `Postare`; `TranzactieId` e NOT NULL cu FK, rândurile sunt aceleași, iar
  planificatorul elimină join-ul (533 ms).
- Eticheta lotului (cod produs + data lotului) nu e unică; lotul n-are cod, iar `Lot.Eticheta` (cu preț) e o
  expresie XAF necopiată în SQL. Consemnat în `limite-curente.md`.
- Proba etichetelor (c) rulează pe FCT (gestiunea virtuală Furnizor, partida pe 401), nu pe LDI: scena
  consumatorilor n-are LDI, iar `CASE`-ul acoperă cele cinci constante prin construcție.
- Rândul view-ului n-are navigație spre tranzacție (referințele EF au fost respinse de owner, amendamentul 3).

### Probe

| Ce | Valoare |
|---|---|
| Integrala privat / bugetar (`.D9P7`) | 4.791 / 3.512 OK, 0 FAIL (7.1: 4.784 / 3.505; +7 `STR-VIZUAL` per profil, 3 texte) |
| `STR-VIZUAL-1…5.3` | paritatea celor 45 de coloane; un rând per postare; etichetele Furnizor și `<partener>/<AAAA-LL-ZZ>`; rolul `Deny` cu criteriu de rând → lipsuri `[Postare]`, listă goală, fraza porților, administratorul fără lipsuri; create/update/delete refuzate la commit, baza neatinsă |
| Nucleu / `--probe-sursa` / TPH / `has-pending-model-changes` | 180/180 / 11/0 / 0 / curat |
| Neexpunerea în artefacte | `PostareVizual` absent din `openapi.json`, `api-types.ts`, `metadata.json`; `src/generated/` neschimbat la regenerare |
| HTTP pe host viu (`run-verificari/d9-pas7-http.ps1`, baza nouă `.Privat.D9P7` din migrații + seed) | `refuzuri.ps1` 318/318 de două ori; `neexpunere-cub.py` 0 FAIL, inclusiv rutele OData și `$metadata` pe `PostareVizual` |
| Browser (`run-verificari/d9-pas7-ui.ps1`: FCT operată cu plata conex, host Blazor pe 5091, `Admin`) | „Registre → Postări”: 6 rânduri, cele 12 coloane, coduri (628/401/4426/5311, MAG1/CASA, `P7-F-…/2026-01-15`), fără creare/ștergere; sortarea pe „Cont” pe server; dublu-clic deschide detaliul în citire |

### Costul pe `Atlas.Conta.ModelCheck.Privat.D9Vol` (5.001.179 de postări, schema veche; view creat și șters prin `psql`)

| Interogarea, proiecția listei | Timp |
|---|---|
| Pagina întâi, `ORDER BY "Data", "ID" LIMIT 50` | 129 ms (referință pe `Postare` brut: 10 ms) |
| Aceeași pagină, `ORDER BY "ContSimbol", "ID" LIMIT 50` | 9.086 ms, scan complet cu spill pe disc |
| `COUNT(*)` pe view | 533 ms (referință pe `Postare` brut: 502 ms) |

Niciun index adăugat (pin 17). Pe containerul de dezvoltare `/dev/shm` are 64 MB: sortarea pe cod cu
paralelismul implicit pică cu `could not resize shared memory segment`; cifrele sunt măsurate serial.
Planurile: `run-nucleu/tr-d9a/pas7/cost-d9vol.md`.

### Bazele după pasul 7

Cele trei baze de dezvoltare au fost șterse de owner și refăcute din `InitialCreate` + `PostareVizual` cu
seed (bugetar și Privat prin updater-ul Blazor; ModelCheck își face baza privată la prima rulare). Clonele
`.D9P7`, `.Privat.D9P7` (HTTP și browser) rămân pentru review; clonele de import și de perf rămân pe schema
veche (Import1C cere `--recreeaza` la pasul 8).
