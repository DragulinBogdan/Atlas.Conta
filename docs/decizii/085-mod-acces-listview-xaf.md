# 85. Modul de acces al ListView-urilor XAF Blazor (EF Core, PostgreSQL): `Server` implicit cu paginare, `ServerView` opt-in pe registre, `Client` explicit pe grilele de culegere, IF/IFV doar cu prag măsurat

- **Data**: 2026-09-11
- **Stare**: activă, ÎNCHISĂ 2026-09-12 (tranșează riscul D4 din `docs/gate-xaf-contract.md` prin (g); modul `ServerView` pe `RegistruStoc` și `Server` pe `RegulaStoc`, puse ad-hoc la GATE XAF, capătă regulă; 85-r1 închisă la închidere — cele trei registre pe `ServerView`)
- **Docs**: `nou/Atlas.Conta.BackOffice/Atlas.Conta.BackOffice.Blazor.Server/Model.xafml` (`Options`, registrele), `nou/.../Module/UI/ContaUiBaseline.cs` (grilele de culegere), `nou/.../Module/BusinessObjects/Nomenclatoare/ProdusLot.cs` (`Lot.Eticheta`, `Lot.ExpresieEticheta`), `nou/.../Blazor.Server/Controllers/PartenerSincronizareAnafController.cs`, `nou/tools/ModelCheck/Program.cs` (D85-M0/M1/M2/R1/R2/R3), `nou/tools/ModelCheck/ModelAplicatie.cs`, `NumaratorSql.cs`, `docs/api/p5-perf-masuratori.md` (§Decizia 85 — cifrele de pagină)

## Regula durabilă

**Modul de acces al listelor XAF Blazor.** (a) Implicitul aplicației e
`Server` cu paginare: `Options.DataAccessMode = Server`,
`VirtualScrollingEnabled = False` în `Blazor.Server/Model.xafml`; `Client`
nu mai e implicit pe niciun ListView root (Client încarcă tot setul la
fiecare deschidere, scroll-ul virtual virtualizează doar randarea; volumele:
205 k documente, 19 k FCT, 41 k FCL, 47 k loturi, registre de 91 k–305 k
rânduri). (b) `ServerView` e opt-in per view, doar pe registre append-only
citite, cu precondiții probate (`D85-M2`): toate coloanele vizibile mapate
sau `[Calculated]`, orice coloană vizibilă de tip referință are
`DefaultProperty` pe clasa țintă, mapat/`[Calculated]` (altfel grila arată
GUID-ul), niciun controller care face cast pe selecție, nicio regulă
Appearance pe membru nevizibil (pe rândul de view `EFCoreDataViewRecord`
aruncă `IndexOutOfRangeException`). DetailView-ul rândului SE deschide din
`ServerView` (prin cheie); ce e inactiv e acțiunea `OpenObject` pe
referințe. Azi: `RegistruStoc`, `RegistruContabil`, `RegistruTva` (85-r1
închisă pe cifre: pagina `RegistruTva` 966 → 0,1 ms). (c) ListView-urile de
CLASĂ folosite ca grile nested de culegere (cele opt tipizate puse de
`TipDetaliuViewUpdater`) se declară `Client` explicit în `ContaUiBaseline`:
`Options.DataAccessMode` se aplică oricărui `<Class>_ListView`
(`ModelViewLogic.Get_DataAccessMode`), pe când `<Class>_<Member>_ListView`
primește Client hardcodat la generare
(`ModelNestedListViewNodesGenerator.cs:59`); culegerea nu se face niciodată
peste o colecție server: colecția Server nu vede liniile NESALVATE ale
masterului (`D85-R3`: Client 2 / Server 0), grupare + adăugare aruncă în
`ServerModeSourceAdderRemover`, `IsModified`/clona pe colecție modificată
aruncă, fără Batch Edit. (d) `InstantFeedback`/`InstantFeedbackView` NU sunt implicite; opt-in
per view cu prag măsurat — p95 al unei pagini > ~300 ms după indexare — și
probă în browser pe selecție, grupare, refresh după commit (DbContext nou
per ciclu de fetch, `EFCoreInstantFeedbackSource.cs:84-116`; selecția e
`ObjectRecord`, entitățile trăiesc într-un DbContext de fundal dispus; fără
editare inline; marshaling-ul spre circuitul Blazor invizibil în sursa XAF;
securitatea NU e ocolită). (e) `DataView` e exclus: pe EF Core face un singur
query eager pe tot setul, doar proiectat (`EFCoreObjectSpace.LoadExpressions`).
(f) Controllerele rezolvă selecția prin `ObjectSpace.GetObject`, nu prin cast
pe `SelectedObjects`/`CurrentObject`; primul aliniat:
`PartenerSincronizareAnafController`. (g) O proprietate afișată într-un
ListView sau folosită ca `DefaultProperty` e mapată sau `[Calculated]`
(expresie de criterii peste membri persistenți, tradusă în SQL);
`Lot.Eticheta` = `[Calculated(Lot.ExpresieEticheta)]` (`Iif`/`Concat`/
`ToStr`/`Round(PretUnitar, 4)` — `FormatString` și `PadLeft` nu se traduc;
prețul are 4 zecimale FIXE, `DateOnly` default e `-infinity` în Npgsql), FĂRĂ
`[SearchMemberOptions(Exclude)]`: membrul aliased trece de
`FullTextSearchEngine.IsMemberPersistent` și de `LookupPropertyEditor`, deci
căutarea pe lookup-ul de lot e vie — închide riscul D4 din
`docs/gate-xaf-contract.md` (`SearchMode = None` pe DisplayMember nemapat,
`LookupPropertyEditor.cs:624-627`); `ApiProiectii.EtichetaLot` deleagă la
`Lot`; oglinda React (`nucleu/lot.ts`, OData, 55g) rămâne o compunere
separată cu format `ro-RO` — aceeași semantică, nu același literal. (h)
Probe: ModelCheck pe modelul REAL al hostului Blazor (`ModelAplicatie`,
`D85-M0`) cu numărătoare de comenzi SQL (`NumaratorSql`): `D85-M1` (modurile
per view), `D85-M2` (precondițiile lui (b) pe orice ServerView/IFV), `D85-R1`
(pagină ServerView pe `Lot`/`RegistruStoc` cu `Eticheta` calculată ≡ C#,
inclusiv rotunjirea jumătății), `D85-R2` (o pagină Server = query + COUNT,
fără interogare per rând), `D85-R3` (culegerea nested Client vede liniile
nesalvate); pe calea reală: deschiderea FCT și a registrelor,
sort/filtru/grupare, DetailView din listă, adăugare de linie pe document nou
cu grupare, acțiunea ANAF pe parteneri, căutarea pe lookup-ul de lot; cifrele
în `p5-perf-masuratori.md` §Decizia 85. (i) Lookup-urile nu sunt atinse:
`LookupPropertyEditor` forțează `Queryable` (`LookupPropertyEditor.cs:619-620`).
Restanțele 85-r1…r10 → `restante.md`.

## Context

La pornire, `Model.xafml` avea `Client` (implicitul XAF) cu
`VirtualScrollingEnabled = True` pe `Options`, iar din GATE XAF (53) două
override-uri ad-hoc, fără decizie: `RegistruStoc_ListView` pe `ServerView`
(fix-ul N+1 pe eticheta de lot, 53f) și `RegulaStoc_ListView` pe `Server`.
Volumele bazei de import (`docs/api/p5-perf-masuratori.md`, §Metodă):
205.168 documente, 337.152 linii, 19.042 FCT, 40.555 FCL, 305.039 /
282 k / 90.740 rânduri în `RegistruContabil` / `RegistruStoc` /
`RegistruTva` — un ListView `Client` pe `Document` materializează tot setul în
DbContext-ul view-ului la fiecare deschidere, iar scroll-ul virtual nu
schimbă nimic din asta: virtualizează randarea, nu încărcarea.

Întrebarea utilizatorului a fost în două trepte: `Server` sau `ServerView` ca
implicit, apoi dacă `InstantFeedback`/`InstantFeedbackView` merită ca
implicit sau ca opt-in. Răspunsul s-a dat pe sursă, nu pe documentația de
marketing.

## Ce s-a citit în sursă

DevExpress 26.1.4 (`D:\dev\SrcLib\DevExpress26.1.4`), `ExpressApp.EFCore` +
`ExpressApp.Blazor`:

| Mod | Clasa de colecție | Sursa |
|---|---|---|
| `Client` | `EFCoreCollection` | `EFCoreObjectSpace.cs:560-572` |
| `Server` | `EFCoreServerCollection` | `EFCoreObjectSpace.cs:667-679` |
| `DataView` | `EFCoreDataView` | `EFCoreObjectSpace.cs:238-250` |
| `ServerView` | `EFCoreServerModeView` | `EFCoreObjectSpace.cs:1286-1291` |
| `InstantFeedback` | `EFCoreInstantFeedbackSource` | `EFCoreObjectSpace.cs:680-692` |
| `InstantFeedbackView` | `EFCoreInstantFeedbackView` | `EFCoreObjectSpace.cs:1292-1297` |

- **`Server`**: entitățile paginii sunt TRACKED (niciun `AsNoTracking` în
  `ExpressApp.EFCore`), `.Include` per coloană de referință afișată
  (`EFCoreObjectSpace.cs:611-618`), `CanFilterByNonPersistentMembers = false`
  (`:574-576`), COUNT-ul e query separat (`:410-419`). O pagină = un SELECT cu
  join-urile coloanelor + un COUNT; rândul e entitatea, deci selecția,
  DetailView-ul din listă și editarea inline funcționează ca pe `Client`.
- **`ServerView`**: SELECT-ul proiectează doar `DisplayableProperties`
  (`EFCoreServerModeView.cs:134-140, 274-307`); agregatele și join-urile pe
  expresii de view aruncă `NotSupportedException` (`:197-202`); rândul e
  `EFCoreDataViewRecord`, nu entitatea — un membru cerut care nu e în
  proiecție aruncă `IndexOutOfRangeException`.
- **`InstantFeedbackView`** refolosește `EFCoreServerModeView`
  (`EFCoreInstantFeedbackView.cs:124-128`); **`InstantFeedback`** deschide un
  DbContext nou per ciclu de fetch (`EFCoreInstantFeedbackSource.cs:84-116`),
  prin provider-ul securizat când aplicația e securizată — securitatea rămâne.
- **`VirtualScrollingEnabled`** e pe `IModelOptionsBlazor` și
  `IModelListViewBlazor` (`Blazor SystemModule\Module.cs:367, 397, 522-527`)
  și privește doar randarea grilei.
- **`PropertyCollectionSource`** (nested) oglindește toate modurile
  (`PropertyCollectionSource.cs:113-140`), dar nodul
  `<Class>_<Member>_ListView` primește `Client` hardcodat la generare
  (`ModelNestedListViewNodesGenerator.cs:59`); `Options.DataAccessMode`
  ajunge doar la `<Class>_ListView` (`ModelViewLogic.Get_DataAccessMode`).
- **Docs oficiale**: „List View Data Access Modes" (matricea: rândul e
  `ObjectRecord` pe ServerView/IF/IFV; editarea in-place doar pe
  Client/Queryable/Server; `Options` nu atinge nested-urile autogenerate),
  „Server, ServerView, InstantFeedback…" (118450), `DxGridListEditor` (fără
  Batch Edit pe Server; `Queryable` nu e suportat de editorul de grilă).

## Tranșări

**(a) `Server`, nu `ServerView`, ca implicit.** `Server` păstrează rândul ca
entitate: selecția e obiectul, DetailView-ul din listă, acțiunile pe
selecție, editarea inline și regulile Appearance pe orice membru merg
neschimbate — deci o schimbare GLOBALĂ a implicitului nu rupe niciun ecran.
Costul față de `ServerView` e `.Include` per coloană de referință afișată și
tracking-ul paginii (20–50 de entități), nu al setului. `ServerView` cere
disciplină per view (precondițiile din (b)) și se plătește doar acolo unde
lățimea rândului sau join-urile fac diferența: registrele, citite și late.
Paginarea, nu scroll-ul virtual: o pagină e deterministă (un SELECT cu
OFFSET/LIMIT + COUNT), selectorul de pagină e explicit, iar opțiunea „toate
rândurile" din selector se ascunde singură pe sursele server.

**(b) `ServerView` opt-in, pe registre — toate trei, pe cifre.**
`RegistruStoc` stătea deja pe `ServerView` (eticheta de lot pe fiecare rând,
53f). `RegistruContabil` și `RegistruTva` au fost măsurate întâi pe `Server`
(85-r1), iar cifra a decis: pe `RegistruTva` (90.740) pagina costa 966 ms —
EXPLAIN ANALYZE 805 ms, INNER JOIN pe derived-table-ul TPT `DocumentDetalii`
(9 LEFT JOIN) → Hash Join cu Seq Scan pe 337 k linii pentru 20 de rânduri,
pentru că în modul `Server` `.Include` se aplică și coloanelor cu
`Index = -1` (coloana rămâne în modelul view-ului; 85-r6); pe `ServerView`
0,1 ms (8 coloane, 0 JOIN). Pe `RegistruContabil` (305.039, 16 dimensiuni
`AutoInclude`, 41c) SELECT-ul scade de la 342 coloane / 66 JOIN la 25 / 20,
gruparea de la ≈ 500 la ≈ 170 ms. Tabelul complet: `p5-perf-masuratori.md`
§Decizia 85. Rândul de `ServerView` e un `EFCoreDataViewRecord`, dar
DetailView-ul lui se deschide normal (dublu-click → `RegistruTva_DetailView/
{id}`, probat); „`OpenObject` inactiv" din documentația DevExpress e acțiunea
`OpenObjectController` pe referințe. Precondițiile devin probă (`D85-M2`),
nu recomandare: un membru nemapat într-o coloană sau un `DefaultProperty`
nemapat pe o referință afișată înseamnă rânduri goale sau excepție la
randare; o referință FĂRĂ `DefaultProperty` pe clasa țintă arată GUID-ul
(văzut pe coloanele `Document`/`Detaliu` din `RegistruStoc`/`RegistruContabil`
— se scot din view, ca pe `RegistruTva`); un cast pe selecție înseamnă
`InvalidCastException` la prima acțiune.

**(c) Nested-urile de culegere rămân `Client`, declarat.** Cele opt ListView-uri
tipizate pe care `TipDetaliuViewUpdater` le pune pe colecția `Detalii` sunt
noduri `<Clasă>_ListView`, deci ar fi moștenit implicitul `Server`. Premisa
inițială („master nesalvat") era greșită — pe calea reală XAF Blazor
salvează antetul ÎNAINTE de prima linie nested (`New` pe grila Detalii a
unui document nou face INSERT pe antet întâi, probat); motivele reale: o
colecție server nu vede liniile NESALVATE ale masterului (`D85-R3`: aceeași
scenă, Client 2 rânduri / Server 0), gruparea cu adăugare aruncă în
`ServerModeSourceAdderRemover`, `IsModified`/clona pe o colecție modificată
aruncă, iar Batch Edit nu e suportat. `ContaUiBaseline` le declară `Client`
explicit; `D85-R3` și proba în browser (factură nouă: linia apare imediat,
grupare pe Tip + încă o linie fără excepție) o acoperă.

**(d) IF/IFV — opt-in cu prag, nu implicit.** Câștigul lor e UI-ul care nu
așteaptă query-ul; prețul e un DbContext nou per fetch, selecția ca
`ObjectRecord` (entitatea e într-un context de fundal deja dispus — orice
controller care o atinge trebuie s-o reîncarce), fără editare inline, și un
marshaling spre circuitul Blazor pe care sursa XAF nu-l arată. Pe o pagină
sub ~300 ms nu există ce ascunde. Pragul e p95 al unei pagini după
indexare; declanșarea cere proba în browser pe selecție, grupare și refresh
după commit (85-r2).

**(e) `DataView` exclus.** Pe EF Core, `EFCoreDataView` face un singur query
eager pe tot setul (`EFCoreObjectSpace.LoadExpressions`) — e `Client`
proiectat, nu server-mode.

**(f) Selecția prin `GetObject`.** Cu `Server` rândul e entitatea, deci
cast-ul merge azi; cu `ServerView`/IF/IFV e `ObjectRecord` și cast-ul
tace (`OfType` întoarce gol). `ObjectSpace.GetObject(rând)` rezolvă corect
în toate modurile, deci controllerele o folosesc indiferent de modul curent al
view-ului — un view care trece pe `ServerView` nu mai poate rupe un controller.

**(g) `[Calculated]` pe `Lot.Eticheta`.** `[NotMapped]` însemna: coloană
goală pe `ServerView`, `SearchMode = None` pe lookup-ul de lot (căutarea
moartă — pe colecții mari lookup-ul PORNEȘTE gol, deci căutarea era singura
cale), excludere din full-text (53f). Expresia (`Lot.ExpresieEticheta`):

```
Iif(Data = #0001-01-01# And PretUnitar = 0,
    Concat(IsNull(Produs.Denumire,'(produs nedefinit)'), ' (în culegere)'),
    Concat(IsNull(Produs.Denumire,'(produs nedefinit)'), ' · ',
           Iif(GetDay(Data)<10,'0',''), ToStr(GetDay(Data)), '.',
           Iif(GetMonth(Data)<10,'0',''), ToStr(GetMonth(Data)), '.',
           ToStr(GetYear(Data)), ' · ', ToStr(Round(PretUnitar,4))))
```

se traduce pe Postgres în `CASE WHEN l."Data" = DATE '-infinity' AND
l."PretUnitar" = 0.0 THEN … ELSE … date_part(...) … round(l."PretUnitar",
4)::text END`: proiectabilă, căutabilă, sortabilă (`ORDER BY CASE …
date_part`). Tranșări: `FormatString` nu e în converter, `PadLeft` se
traduce într-o supraîncărcare inexistentă („No method PadLeft compatible",
măsurat) → `Iif`; prețul are 4 zecimale FIXE (`0.0000`, invariant; înainte
`0.####`) ca SQL ≡ C# literal (`D85-R1`: trei rânduri identice, inclusiv
rotunjirea jumătății 12.34565 → 12.3457); `DateOnly` default e scris/citit de
Npgsql ca `-infinity` (un rând scris brut cu `'0001-01-01'` n-ar intra în
ramura „în culegere"). `[SearchMemberOptions(Exclude)]` a fost scos: membrul
aliased trece de `FullTextSearchEngine.IsMemberPersistent` și de
`LookupPropertyEditor` (`SearchMode` nu mai e `None`) — probat în browser:
„Katun" filtrează, COUNT + pagină ≈ 30 ms per tastă. `ApiProiectii.EtichetaLot`
deleagă la `Lot` (o compunere pe server); oglinda din clientul React
(`nucleu/lot.ts`, pe OData — 55g) rămâne o compunere SEPARATĂ cu format
`ro-RO`: aceeași semantică, nu același literal. Regula devine generală și e
a probei: ce apare într-o listă sau e `DefaultProperty` e mapat sau calculat
(`D85-M2`, `D85-R1`); `Document.Total` rămâne pe DetailView (Index = −1 pe
root, 40d).

## Inventarul nostru

- Nemapate care ating liste: `Document.Total` (deja `Index = -1` pe root,
  40d), `FacturaIesire.ValoareLivrare` / `FacturaIntrare.ValoareReceptie`
  (ascunse), `Lot.Eticheta` (`DefaultProperty` nemapat — (g)).
- `AutoInclude` doar pe cele 16 dimensiuni ale `RegistruContabil` (41c);
  `RegistruStoc`, `RegistruTva`, `Lot.Produs` lazy deliberat.
- SmartLookup revertat la lookup-ul standard (53h) — lookup-urile stau pe
  `Queryable` (i).
- Un singur cast pe selecție: `PartenerSincronizareAnafController.cs:46` (f).
- Override-uri în `Model.xafml` la închidere: `RegistruStoc_ListView`,
  `RegistruContabil_ListView`, `RegistruTva_ListView` = `ServerView`;
  `RegulaStoc_ListView = Server` (redundant cu `Options`, 85-r5).

## Închidere

| Pas | Livrat | Verdict |
|---|---|---|
| Cod | `Options.DataAccessMode = Server`, `VirtualScrollingEnabled = False`; cele trei registre pe `ServerView`; cele opt grile de culegere `Client` în `ContaUiBaseline`; `Lot.Eticheta` `[Calculated(ExpresieEticheta)]` fără `SearchMemberOptions`; `PartenerSincronizareAnafController` pe `GetObject` | nicio migrație (`has-pending-model-changes`: none) |
| ModelCheck | `ModelAplicatie` (modelul REAL al hostului Blazor: `Startup` din Blazor.Server, calea `--updateDatabase`, fără circuit) + `NumaratorSql` (`DbCommandInterceptor`); probele D85-M0 (modelul se construiește), M1, M2, R1 ×2, R2, R3, „scena nu lasă urme" | privat 1016 → 1024 OK, bugetar 946 → 954 OK, 0 FAIL |
| Cifre de pagină | Postgres `log_min_duration_statement = 0` (exec + plan per comandă) + `performance.now()` în browser, pe Privat | tabelul în `p5-perf-masuratori.md` §Decizia 85; `RegistruTva` refresh 1,4 s → 0,3 s, sort 823 → 125 ms, grupare 691 → 148 ms; `RegistruContabil` refresh 367 → 337, sort 295 → 218, grupare 1237 → 501 ms |
| Calea reală | FCT: deschidere, sort, filtru, grupare, pagina 2, DetailView ≈ 1 s (o pagină = COUNT + 1 SELECT); nested Client pe factură existentă și nouă (linia apare imediat, grupare pe Tip + încă o linie); `RegistruStoc` ServerView sort/filtru/pagina 2/selecție; Partener + ANAF (2 selectați → „2 găsiți… 3 diferențe raportate"); Loturi (eticheta cu valori, sort pe expresie, căutare „Toner Katun" 234 rânduri, lookup viu); consolă + log fără `IndexOutOfRange`/`NotSupported`/`InvalidOperation`/`ObjectDisposed` | toate PASS (capturile în scratchpad-ul sesiunii, nu în repo) |
| Docs | decizia, README, restanțe, istoric, gate D4, stare-curenta, perf | acest fișier |

Devieri față de textul inițial: premisa „master nesalvat" a lui (c)
corectată (antetul se salvează înaintea primei linii; motivul e
vizibilitatea liniilor nesalvate + gruparea/`IsModified`); volumetria „40 k
FCT" corectată (FCT 19.042, FCL 40.555); prețul din etichetă cu 4 zecimale
fixe, nu `0.####`; 85-r1 închisă în aceeași felie, pe cifre. Rularea finală
(după coloanele GUID scoase și M2 întărită, cu cele trei registre pe
`ServerView`): privat 1024 OK / bugetar 954 OK, 0 FAIL.

Review advers (main, pe diff-ul integral): (1) `Culegere` din fluent e aplicat
DIRECT pe nod (`UiBaselineUpdater`), nu `SetIfDefault`, deci bate `Options` —
altfel (c) ar fi fost decorativ; M1 o probează. (2) `ViewSelectedObjects` face
`GetObject` pe `IObjectRecord` (`ViewController.cs:89-104`) — singurul cast pe
selecție din BackOffice era cel din controllerul ANAF. (3) `[Calculated]` nu
schimbă modelul EF (nicio migrație), iar `PadLeft`/`FormatString` nu se
traduc — de aceea `Iif` pe zi/lună; jumătatea se rotunjește la fel în PG și C#
(AwayFromZero). (4) În modul Server `.Include` prinde și coloanele cu
`Index = -1`: nu e regresie a deciziei, e cauza cifrei pe `RegistruTva` și
motivul pentru care registrele merg pe `ServerView` (85-r6 pentru restul).
(5) ModelCheck compilează Blazor.Server: capcana DLL-urilor blocate e în
`dezvoltare-si-validare.md` și în CLAUDE.md. (6) Oglinda React a etichetei nu
e aliniată la literal (format `ro-RO`) — declarat în (g), nu ascuns. Verdict:
fără defecte de fond; nimic de refăcut.

## Ce rămâne deschis

- **85-r1** `RegistruContabil` / `RegistruTva` pe `ServerView` — ÎNCHISĂ pe
  cifrele din §Închidere / `p5-perf-masuratori.md`.
- **85-r2** `InstantFeedback` / `InstantFeedbackView` — doar pe view-ul care
  trece pragul (d), cu proba în browser.
- **85-r3** Alte proprietăți nemapate care ajung vreodată în liste (`Total`
  pe DetailView rămâne); regula (g) le refuză din ModelCheck, nu din review.
- **85-r4** `PageSize` implicit pe grile — cifra optimă se decide pe probă.
- **85-r5** `RegulaStoc_ListView` are override `Server` redundant cu
  `Options`; se curăță la atingere.
- **85-r6** În modul `Server`, navigațiile coloanelor ASCUNSE cu `Index = -1`
  intră tot în `.Include` (FCT: 129 coloane / 33 JOIN; după salvarea unei
  linii reîncărcarea ei costă 150–260 ms, 28 JOIN). Curatoria: referințele
  care nu se afișează ies din MODELUL view-ului (`HideMembers` /
  `VisibleInListView(false)`), nu doar din index; de măsurat pe listele grele.
- **85-r7** Gruparea pe `Server`/`ServerView` încarcă primele rânduri ale
  FIECĂRUI grup (chei + entități per grup; FCT grupat pe Primitor = 17 grupuri
  × 2 comenzi) — „≤ 3 comenzi pe pagină" ține doar negrupat.
- **85-r8** `Refresh` execută pagina de două ori pe `Server` (o dată pe
  `ServerView`).
- **85-r9** Layout-ul salvat al utilizatorului (`ModelDifferenceAspects`)
  poate ascunde toate coloanele unui view (văzut pe Admin / `RegistruStoc`,
  diff din 2026-07-29), iar pe `ServerView` celulele rămân goale până la
  Refresh după re-bifarea coloanelor din Column Chooser — de tratat la
  curatoria grilelor (DIM-4).
- **85-r10** Coloana `Produs` goală în grila Detalii a FCT `WIS26511` — de
  verificat dacă e de date sau de afișare.
