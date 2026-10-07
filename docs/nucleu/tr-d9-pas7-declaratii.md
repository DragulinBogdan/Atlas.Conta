# TR-D9a, pasul 7 — eliminarea atomică a declarațiilor și lista cubului pe `PostareVizual`

Data: 2026-10-07. Execuție: agent Opus (`d9-pas7`, spec și raport în `run-nucleu/tr-d9a/pas7/`, gitignored);
verificare, documentația și commit-ul per jalon: main. Contractul: D9-D6, D9-D7 (b), D9-D8, D9-D9 (amendat),
D9-D12, amendamentul 3 (D9-A12). Două jaloane; 7.2 e în lucru.

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
