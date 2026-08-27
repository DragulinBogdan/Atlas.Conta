# Pasul 5 — măsurarea de perf a proiecțiilor pe baza de import

**Executată 2026-08-09.** Închide datoria documentată în contractele F3/F4
(§Închidere): D-2a (`CoduriTip` — GetObjectByKey per copil), D-3a
(`DocumenteCuRest` — agregă toate liniile la fiecare încărcare de grilă) și
`PoateGeneraDescarcare` (încarcă entitatea + enumeră liniile la fiecare
`Citeste`). Mandatul: **de măsurat, nu de optimizat orb** — verdictul de mai
jos e pe cifre.

## Metodă

- Baza: `Atlas.Conta.BackOffice.Privat` (clona bazei de import, cea pe care
  rulează WebApi-ul de dev) — **205.168 Documente, 337.152 DocumentDetalii,
  46.063 Imperecheri, 40.536 FCL, 36.698 DSC, 305k/282k rânduri de registre**;
  Postgres în docker (localhost:5444), WebApi Debug pe `https://localhost:5001`.
- HTTP end-to-end cu `curl` (include serializarea + TLS local), 6 rulări per
  endpoint: prima = rece (JIT/cache), raportăm mediana celor 5 calde.
- Parametrii grilei = exact ce trimite clientul (`storeRemote` / DataGrid
  remote): `skip=0&take=20&requireTotalCount=true&sort=[{Data desc}]`;
  pentru panoul de stingeri și `contrapartidaId` (clientul îl trimite mereu).
- Documente-țintă alese worst-case: FCT operat cu 49 linii + copil; FCL-ul de
  smoke cu `GestiuneDescarcareId` setată (singurul pe care
  `PoateGeneraDescarcare` NU scurt-circuitează); FCL de import cu copil DSC;
  contrapartida cu 4.861 de FCT.

## Rezultate (mediană caldă)

| Endpoint | ms | Observații |
|---|---|---|
| `GET /api/proiectii/documente-cu-rest` (grilă: take 20 + count + sort) | **~430** | singurul peste 200ms |
| — filtrat pe contrapartidă (utilizarea reală a panoului) | **~410** | filtrul nu ajută: agregatele rămân whole-table |
| — fără sort, cu count | ~280 | sortul pe uniune costă ~150ms |
| `GET /api/fct/{id}` (Citeste, 49 linii, 1 copil) | ~57 | D-2a inclus |
| `GET /api/fcl/{id}` (smoke — `PoateGeneraDescarcare` drum complet) | ~95 | +~40ms față de scurt-circuit |
| `GET /api/fcl/{id}` (import — scurt-circuit pe gestiune null) | ~54 | cazul majoritar |
| `GET /api/fcl/{id}/rest-nedescarcat` | ~50 | |
| `Lista` FCL / FCT / INC / NIR / DSC (grilă cu join pe agregat) | 135–150 | GROUP BY pe 337k linii, OK |

## Diagnosticul singurului caz lent: `DocumenteCuRest`

Grila plătește **două execuții** ale aceleiași uniuni (`requireTotalCount` →
`count(*)` ~175ms + pagina ~225ms). `EXPLAIN ANALYZE` pe pagina de 20:

- `GroupAggregate` peste **toate** cele 337k `DocumentDetalii` (`Brut`) —
  ~100ms, componenta dominantă;
- `HashAggregate` peste unpivot-ul 2×46k `Imperecheri` (`Asignari`) — ~38ms;
- uniunea celor 5 ramuri de antete (~93k rânduri) — ~48ms;
- restul: join-uri + top-N sort, ieftine. Totul din shared buffers (baza încape
  în RAM); nu lipsește niciun index — costul e STRUCTURAL: agregatele se
  calculează integral la fiecare încărcare, iar filtrul pe contrapartidă se
  aplică doar pe antete, nu intră în agregate.

## Verdict per datorie

- **D-2a (`CoduriTip` per copil)** — NEPROBLEMĂ, închisă. Mulțimea e mărginită
  prin construcție (0–2 copii; memoizare per clasă), iar `Citeste` complet cu
  copii stă la 54–95ms.
- **`PoateGeneraDescarcare`** — NEPROBLEMĂ, închisă. Drumul complet costă
  ~40ms și se plătește DOAR pe FCL operate cu gestiune de descărcare setată
  (pe baza de import: una singură); drafturile și facturile de servicii
  scurt-circuitează, exact cum promite comentariul din `Citeste`.
- **Listele cu join pe agregat** — sănătoase (135–150ms) la 337k linii.
- **D-3a (`DocumenteCuRest`)** — ACCEPTABILĂ AZI, cu curbă de creștere
  reală. ~410ms per deschidere de panou nu blochează release-ul (nu e hot
  path: panoul se deschide la cerere, pe un document), dar e singura proiecție
  al cărei cost crește liniar cu TOTALUL datelor (nu cu pagina): la ~5 ani de
  volum ca 2025 ar ajunge ~2s. Optimizarea țintită, când va fi nevoie, e
  cunoscută și NU cere schemă nouă: (1) împinsă contrapartida în agregate —
  `Brut`/`Asignari` calculate doar pe mulțimea documentelor contrapartidei
  (clientul trimite mereu filtrul; ar tăia agregatele de la whole-table la
  ~mii de rânduri); (2) `requireTotalCount=false` pe panou (paginarea simplă
  nu are nevoie de total) — taie execuția de count (~40%). Ambele aditive,
  în proiecție/client, fără atins motorul.

## Addendum 2026-08-09 (aceeași zi, sesiunea de mărunțiș): găsirea REALĂ era în
`Stingeri`, nu în `Copii`

Sanity-check-ul de după reordonarea `Stingeri` a lovit documentul EXTREM al
bazei de import — un extras de trezorerie cu **335 de stingeri**:
**~11s LA CALD**. Mecanismul era exact cel numit de D-2a (`CoduriTip` =
GetObjectByKey per document, adică interogarea TPT completă per rând), dar pe
mulțimea NEMĂRGINITĂ a panoului de stingeri — presupunerea „mulțime mărginită"
din comentariul funcției nu ține pe documentele de trezorerie ale importului.
Măsurătoarea inițială n-a prins-o fiindcă a exersat `Copii` (0–2 prin
construcție), nu `Stingeri`.

**Fix aplicat** (păstrează designul — ancora pe numele clasei CLR, fără listă
de tipuri înghețată): documentele se materializează POLIMORF într-un singur
query pe bază (`Where(ids.Contains)`) — sub TPT, EF întoarce instanța tipului
derivat corect; aceleași join-uri, o singură dată. Re-măsurat pe același
document: **11,3s → ~0,185s cald (61×)**, date identice (335 rânduri, Total
1.689.058,45, ordinea cronologică nouă).

## Reproducere

Scriptul de măsurare (curl, 6 rulări/endpoint) e trecător (scratchpad);
rețeta: pornește WebApi pe baza Privat, autentifică `Admin`/gol, lovește
endpoint-urile cu parametrii de grilă de mai sus. Pentru SQL:
`ALTER SYSTEM SET log_min_duration_statement=50` + `pg_reload_conf()` (și
RESET la final), apoi `EXPLAIN (ANALYZE, BUFFERS)` pe statement-ul din
`docker logs`.

## Addendum 2026-08-16 — felia 9 (raportarea pe registre): balanță, fișă de cont, registru-jurnal

**Măsurare, nu implementare** — nicio schimbare de cod de producție. Metodă
identică cu cea de mai sus, aceeași bază (`Atlas.Conta.BackOffice.Privat`,
305.059 rânduri `RegistruContabil` — stabil față de măsurătoarea 59), aceiași
parametri de grilă reali (verificați în `Client/src/felii/raportare/*.tsx`),
aceeași disciplină (6 rulări, prima aruncată, mediana celor 5 calde).

Cazuri worst-case alese pe date reale, nu la întâmplare:
- **Contul cel mai traficat**: `4111` „Clienți" — 144.248 atomi/an (108.912 pe
  debit + 35.336 pe credit), de departe primul (locul 2, `371` „Mărfuri", are
  102.606).
- **Contrapartida cu cele mai multe restanțe**: aceeași folosită la măsurătoarea
  59 (`019fa5d1-bb95-…`, 4.861 FCT), regăsită independent ca predator cu cele
  mai multe facturi de intrare — bun reper de comparație mașină-cu-mașină.
- **Luna cea mai încărcată**: martie 2025 (30.798 rânduri) — dar balanța/fișa
  nu variază mult pe lună (costul e dominat de volumul TOTAL al contului/anului,
  nu de lună), deci tabelul de mai jos raportează lună ianuarie (reprezentativă)
  + anul întreg (worst-case real).

### Rezultate (mediană caldă)

| Endpoint | Parametri | ms | Observații |
|---|---|---|---|
| `GET /api/proiectii/balanta` (sintetic) | lună (ian 2025) | **59** | |
| — | an (2025) | **88** | |
| `GET /api/proiectii/balanta` (analitic, cheie dublă) | lună | **84** | |
| — | an | **269** | cel mai scump caz nemăsurat până acum; sub prag |
| `GET /api/proiectii/balanta` + `group=` (sintetic, sumar de grup) | lună | 57 | grupare pe `ContSimbol` = trivială (1:1 cu cheia deja agregată) |
| — | an | 79 | |
| `GET /api/proiectii/balanta` + `group=` (analitic, sumar de grup) | lună | 79 | agregă real: mai multe rânduri `Cont×Repartitor` colapsate pe cont |
| — | an | **202** | worst-case pentru calea de grupare — nimeni n-o măsurase |
| `GET /api/proiectii/fisa-cont` (contul `4111`, 144.248 atomi) | prima pagină, an, cu `requireTotalCount` | **244** | |
| — | pagină adâncă (`skip=100000`) | **286** | |
| — | pagină foarte adâncă (`skip=140000`) | 248 | **cost independent de `skip`** — vezi diagnostic |
| — | prima pagină, fără `requireTotalCount` | 214 | count-ul costă ~30ms |
| `GET /api/proiectii/registru-jurnal` | an, cu `requireTotalCount` | 77 | |
| — | an, fără `requireTotalCount` | 63 | |
| `GET /api/proiectii/documente-cu-rest` (reper, decizia 59) | filtrat contrapartidă (4.861 FCT) | **423** | ~410ms în 59 — mașina de azi e comparabilă, cifrele rămân comparabile |

Singurele peste ~200ms: balanța analitică pe an (269ms, sub prag), balanța
grupată analitic pe an (202ms, sub prag), fișa de cont (214–286ms, sub prag) și
`DocumenteCuRest` (423ms, peste prag — reper, deja diagnosticat în 59).

### Diagnostic: `DocumenteCuRest` — cauza e NESCHIMBATĂ față de decizia 59

`EXPLAIN (ANALYZE, BUFFERS)` pe interogarea reală (capturată din
`docker logs`, parametrizată cu contrapartida de mai sus): pagina costă
**198,8ms** din care dominanta e tot `GroupAggregate` peste **toate** liniile
`DocumentDetalii` (acum ~205k documente / rândurile lor) pentru `Total` —
niciun predicat selectiv nu intră înaintea agregatului, exact ca la 59; plus
count-ul separat (~250ms, măsurat direct din log). Niciun index n-ar reduce
costul ăsta — e structural (agregat whole-table), nu lipsă de index. Verdictul
din 59 rămâne valabil neschimbat.

### Diagnostic: fișa de cont — index-urile EXISTĂ și sunt folosite; costul e `WindowAgg`, nu scanare

`EXPLAIN (ANALYZE, BUFFERS)` pe interogarea SQL brută (contul `4111`, an
întreg): **`RegistruContabil` e accesat prin `Bitmap Index Scan` pe
`IX_RegistruContabil_ContDebitId`/`IX_RegistruContabil_ContCreditId` — NU
`Seq Scan`.** Indexurile simple pe cele două FK-uri deja există și motorul de
interogări le folosește corect. Costul dominant e `WindowAgg` (~58ms din
226ms execuție totală) peste cele 144.241 rânduri unpivotate ale contului —
**structural, prin construcție (R-D6): soldul curent cere suma cumulată peste
tot istoricul contului `<= dataEnd`, indiferent de câte rânduri se afișează.**
Confirmarea empirică: costul e practic CONSTANT indiferent de `skip`
(244ms la `skip=0`, 286ms la `skip=100000`, 248ms la `skip=140000`) — fereastra
se calculează integral înainte de orice `LIMIT/OFFSET`.

**Despre indexul compus `(ContDebitId, Data)`/`(ContCreditId, Data)` întrebat
explicit**: pe testul ăsta NU ar ajuta — filtrul `Data <= dataEnd` nu e
selectiv (aproape toate cele 144k rânduri ale contului `4111` cad deja în anul
cerut; `Bitmap Index Scan` pe `ContDebitId` singur întoarce aproape exact
mulțimea finală). Ar putea ajuta pe un interval mult mai îngust față de
istoricul total al contului (ex. „ultima lună" pe un cont vechi și traficat,
unde filtrul de dată taie mult din rândurile candidate) — de măsurat separat,
la cerință reală, nu preventiv (aceeași disciplină ca 59).

### CONSTATARE CRITICĂ, în afara mandatului de perf — ordinea fișei de cont NU e cea din cod

În timpul diagnosticului de mai sus, SQL-ul real capturat din `docker logs`
pentru pagina de fișă de cont arată:

```sql
... ) AS a
ORDER BY a."Id"
LIMIT $6
```

**Nu** `ORDER BY a."Data", a."Id", a."Sens" DESC`, cum specifică LINQ-ul din
`ContabilProiectii.FisaCont` (`.OrderBy(r => r.Data).ThenBy(r => r.Id)
.ThenByDescending(r => r.Sens)`) și cum cere explicit contractul R-D6
(„Ordinea e FIXĂ cronologic — sortarea din grilă se dezactivează... altfel
soldul curent e o coloană de cifre fără sens"). EF Core pare să fi eliminat
`Data` și `Sens` din `ORDER BY` — plauzibil o optimizare de „redundant
order-by" care tratează `Id`-ul (cheia primară a tipului `FisaContSql`,
înregistrat de `AdHocMapper`) ca fiind suficient pentru determinism, ignorând
că `Data` e criteriul PRINCIPAL de sortare, nu doar un tie-breaker.

**Verificat empiric, nu doar teoretic**: pe contul `4111`/2025, o interogare
SQL directă care compară `ORDER BY Id` cu `ORDER BY Data, Id` găsește **26
rânduri** (din 144.248) unde poziția diferă. Pe pagina curentă (an întreg,
ID-uri de tip UUIDv7 — deci corelate temporal cu inserarea, nu cu `Data`
document) proba vizuală pe primele 50 de rânduri a ieșit întâmplător
cronologică (ordinea de inserare a importului 1C a fost, pentru marea
majoritate a rândurilor, chiar cronologică) — asta ASCUNDE defectul, nu-l
infirmă. `SoldCurent` în sine e calculat corect (fereastra internă folosește
`ORDER BY Data, Id, Sens DESC`, neatinsă), dar **rândurile ies afișate/paginate
în ordinea greșită** oriunde inserarea (deci ID-ul) diverge de `Data`
documentului — exact cazul pe care motorul îl permite explicit: operare
retroactivă (25d), corecții, reimportări.

Nu e „cifră absurdă" pe cazul măsurat (diferența nu s-a văzut pe eșantionul
vizual), dar e o încălcare silențioasă a unui invariant DECLARAT load-bearing
(R-D6) — nu am atins codul (mandatul e strict de măsurare), dar o semnalez
explicit: **de investigat și fixat separat, înainte de a declara felia 9
închisă**, cel mai probabil prin forțarea `ORDER BY` direct în textul SQL brut
(în loc de `.OrderBy()` LINQ compus peste `SqlQuery<T>`, care e vulnerabil la
această optimizare a EF Core).

### Verdict

- **Balanța (sintetică/analitică, cu/fără grupare)** — verde, totul sub 300ms,
  inclusiv worst-case (analitic, an, grupat sau nu: 202–269ms). Calea de
  grupare server-side, nemăsurată până acum, e sănătoasă.
- **Registru-jurnal** — verde, 63–77ms pe an întreg.
- **Fișa de cont** — ACCEPTABILĂ ca perf (214–286ms, sub prag, cost dominat
  structural de `WindowAgg`, indexurile existente sunt corect folosite), DAR
  cu defectul de ordine de mai sus, care e un blocant de CORECTITUDINE, nu de
  performanță — separat de verdictul de perf.
- **`DocumenteCuRest`** — neschimbat față de 59: acceptabil azi, singura
  proiecție cu creștere liniară reală, optimizare cunoscută și amânată.
- **Nimic nu crește liniar în afară de `DocumenteCuRest`** (deja documentat în
  59) — balanța/fișa/jurnalul cresc cu volumul CONTULUI/PERIOADEI cerute, nu cu
  totalul bazei; la 5 ani de volum ca 2025, fișa unui cont foarte traficat ar
  ajunge undeva la ~1–1,2s (extrapolare liniară pe `WindowAgg`, singura
  componentă care scalează cu numărul de rânduri ale contului) — de
  remăsurat, nu de optimizat preventiv.

### Reproducere (addendum)

Contul `4111` = `019fa5d0-cbcd-7461-9982-0809a2075b64`; contrapartida-reper =
`019fa5d1-bb95-7e9c-b168-3169642f4267`. Restul, ca la metoda de mai sus.
`log_min_duration_statement` resetat la final (`ALTER SYSTEM RESET` +
`pg_reload_conf()` — verificat `-1`); WebApi oprit, porturile 5000/5001
verificate libere.

---

## Addendum 3 (2026-08-19) — jurnalele de TVA (felia 11)

Măsurat pe clona de import **după backfill** (`Atlas.Conta.Import1C.Flax`,
**90.732 de rânduri fiscale** pe 61.347 de documente, anul 2025 integral).

**Metodă diferită de a celorlalte addendumuri, și trebuie spus de ce**: baza de
import **n-are tabele de securitate** — o creează Import1C printr-un
`EFCoreObjectSpaceProvider` standalone, fără XAF security — deci nu există
utilizator cu care să se obțină un token, iar calea HTTP folosită peste tot mai
sus e imposibilă acolo. Cifrele de mai jos sunt deci pe **SQL ECHIVALENT**, rulat
în psql (cald, a doua rulare), nu pe SQL-ul emis de EF.

Ce lipsește față de SQL-ul real, și de ce nu schimbă ordinul de mărime: lanțul
`CASE` care traduce `RegimTva` în string, `LEFT JOIN`-ul suplimentar pe frunza
`Parteneri` (as-cast-ul pentru `CodFiscal`), și `ORDER BY`/`LIMIT`-ul pus de
`DataSourceLoader`. Toate se aplică *peste* mulțimea deja agregată — care e de
19–42 mii de rânduri, nu de 90 de mii.

| Proiecție | Perioadă | Rânduri produse | Cald |
|---|---|---|---|
| `JurnalTva` achiziții | an | 19.374 | **14 ms** |
| `JurnalTva` livrări | an | 42.037 | **15 ms** |
| `JurnalTva` livrări | o lună | 3.182 | **4 ms** |
| `DecontTva` | an | 8 | **12 ms** |

**Verdict**: agregarea e integral server-side și ieftină — ordinul de mărime e
al balanței pe o lună, nu al `DocumenteCuRest`. Niciun index adăugat (disciplina
59); indecșii de FK creați automat pe `DocumentId`/`TipTvaId`/`PartenerId` la
crearea tabelei sunt suficienți.

**Ce rămâne de măsurat pe calea reală**: cifrele de mai sus nu acoperă tierul
HTTP. Prima bază care are ȘI volum, ȘI utilizatori (o bază de client, sau clona
de import re-creată prin updater) trebuie remăsurată cu metoda standard — până
atunci, cifrele astea sunt un ordin de mărime, nu un contract.

---

## Addendum 4 (2026-08-24) — D300 (felia 12)

Măsurat pe **`Atlas.Conta.BackOffice.Privat`** — baza cu datele de import 2025
(**90.739 de rânduri în `RegistruTva`**, 205.184 de documente).

**Metodă: calea REALĂ, spre deosebire de addendumul 3.** Acolo cifrele au fost pe
SQL echivalent, fiindcă `Atlas.Conta.Import1C.Flax` n-are tabele de securitate și
deci nu există token. Baza de față le are, așa că D300 s-a măsurat exact cum îl
cere clientul: `GET https://localhost:5001/api/proiectii/d300` cu JWT de `Admin`,
prin ușa securizată (`Secured(typeof(RegistruTva))`), WebApi pornit detașat și
încălzit. Șase rulări per cifră, **prima aruncată** (JIT + primul plan), mediana
celorlalte cinci.

| Cerere | Rânduri de registru citite | Răspuns | Rulări (ms) | **Mediană** |
|---|---|---|---|---|
| `d300` septembrie 2025 | ~7.900 | 55 rd. + 0 nemapate, 13,9 KB | 18, 19, 20, 22, 23 | **20 ms** |
| `d300` martie 2025 | ~10.100 | 55 rd. + 1 nemapat, 14,0 KB | 18, 19, 20, 21, 24 | **20 ms** |
| `d300` **anul 2025 întreg** | 90.739 | 55 rd. + 1 nemapat, 14,1 KB | 24, 25, 25, 25, 26 | **25 ms** |
| `decont-tva` anul 2025 (reper) | 90.739 | 8 grupuri | 23, 23, 24, 26, 28 | **24 ms** |

**Verdict: verde, cu mult sub pragul de ~150 ms (59). Niciun index adăugat** —
disciplina rămâne „cifra decide", iar cifra nu cere nimic.

Ce spune forma cifrelor, dincolo de faptul că sunt mici: **de la o lună la anul
întreg (de 9–11 ori mai multe rânduri de registru) costul crește cu 5 ms**, adică
cu ~25%, nu cu un ordin de mărime. Motivul e structural și merită scris, fiindcă
ține și în viitor: agregarea pe `(Sens, TipTvaId, Regim, Cota)` se face ÎN
BAZĂ, iar mulțimea care iese de acolo e mărginită de NOMENCLATOR (câte tipuri de
TVA × două sensuri × snapshot-uri distincte — 8 grupuri pe tot anul 2025), nu de
volum. Tot ce urmează — așezarea pe rânduri, părinții „din care", oglinzile,
totalurile — lucrează pe zeci de rânduri în memorie, deci e constant. Cei 5 ms de
diferență sunt scanarea, nu proiecția; același profil ca `DecontTva`, care
împarte primul pas cu el (24 ms pe an, la doar 8 grupuri produse — dovada că
prețul e al citirii, nu al formularului).

Consecința practică: **D300 nu are creștere liniară de temut.** O bază cu 5 ani
de volum ca 2025 ar plăti scanarea a ~450 de mii de rânduri fiscale, adică
undeva la 60–80 ms prin extrapolare pe partea care chiar scalează — încă sub
prag, și oricum o interogare pe un an, nu pe cinci: perioada fiscală e o lună sau
un trimestru. Rămâne în afara listei de la 59; singura proiecție cu creștere
liniară reală e tot `DocumenteCuRest`.

### Reproducere (addendum 4)

WebApi pornit detașat cu profilul lui pe `https://localhost:5001`; token prin
`POST /api/Authentication/Authenticate` (`Admin`, parolă goală). Cronometrare cu
`[Diagnostics.Stopwatch]` în jurul lui `Invoke-WebRequest`, deci **latența
completă client→server→client** (TLS, MVC, EF, serializare), nu doar SQL — de
aceea cifrele nu sunt comparabile direct cu cele de psql din addendumul 3, ci cu
cele HTTP din corpul documentului. WebApi oprit la final.

---

## Addendum 5 (2026-08-25) — D394 (felia 14)

Aceeași bază (**`Atlas.Conta.BackOffice.Privat`**, 90.739 de rânduri în
`RegistruTva`), aceeași metodă ca addendumul 4: calea REALĂ prin
`GET https://localhost:5001/api/proiectii/d394` cu JWT de `Admin`, ușa
securizată, WebApi detașat și încălzit; șase rulări, prima aruncată, mediana
celorlalte cinci. Nomenclatorul de parteneri al bazei e NECLASIFICAT (toți tip
2) — nu schimbă costul, dar explică cei ~150/500 de avertismente din răspuns.

| Cerere | Rânduri de registru citite | Răspuns | Rulări (ms) | **Mediană** |
|---|---|---|---|---|
| `d394` septembrie 2025 | ~7.900 | 2.601 rânduri `op1` + 3 rezumate + 151 avertismente, 482 KB | 56, 98, 99, 100, 101 | **99 ms** |
| `d394` **anul 2025 întreg** | 90.739 | 21.936 rânduri `op1` + 500 avertismente, 3,8 MB | 347, 390, 394, 409, 564 | **394 ms** |
| `d300` septembrie 2025 (reper, addendum 4) | ~7.900 | 55 rd., 14 KB | — | 20 ms |

**Verdict: luna — verde (sub 150 ms); anul întreg — peste prag, STRUCTURAL,
nu de optimizat acum.** Diferența față de D300 pe aceeași lună (99 vs 20 ms) nu e
a scanării, ci a FORMEI răspunsului: D300 agregă în bază pe `(Sens × TipTva ×
Regim × Cotă)` și produce 8 grupuri; D394 trebuie să coboare la DOCUMENT (regula
nrFact 1/0 per factură, D4-D3 pasul 3) și la PARTENER, deci mulțimea care iese
din bază e mărginită de numărul de facturi (mii pe lună, ~22.000 rânduri `op1`
pe an), nu de nomenclator, iar serializarea a 3,8 MB de JSON e o parte reală a
celor 394 ms. Cu alte cuvinte, D394 pe un an e o listă de 22.000 de parteneri ×
tip × cotă — lucrul pe care declarația chiar îl cere.

Perioada fiscală a formularului 394 e LUNA (sau trimestrul); anul întreg e o
cerere de control, nu una de depunere. Rămâne în afara listei de la 59 cu
această justificare; dacă o cifră lunară pe o bază mai mare ajunge la prag,
primul candidat e agregatul pe document (un singur `GROUP BY` cu `DocumentId`
în cheie, azi), nu un index.

### Reproducere (addendum 5)

Identic cu addendumul 4 (WebApi detașat, token `Admin`, `Stopwatch` în jurul
lui `Invoke-WebRequest` = latența completă client→server→client). Seed-ul
`MapareD394` trebuie să existe pe bază (`--updateDatabase --forceUpdate
--silent` din Blazor.Server cu `ProfilContabil: Privat`); fără el toate
grupurile cad în `Neincluse` cu `TipTvaNemapat` și cifra măsurată e a altei
proiecții. WebApi oprit la final.

### Re-măsurare la închidere (addendum 5, clona Flax reclasificată)

09/2025: mediană **97–177 ms** pe două serii (zgomot 41–254 ms; d300 pe aceeași lună în aceeași sesiune 21 ms); anul 2025: **598 ms**. Agregatul SQL rulat direct în Postgres: 8 ms (5.445 grupuri, lună) / 26 ms (61.411, an) — query-ul e 5–10 % din total; restul e EF (`Parteneri.Where(idsRep.Contains(...))` cu ~2.400 / ~20.000 GUID-uri în lista IN, etichetele, cele două GroupBy în memorie) și serializarea (451 KB / ~3,5 MB). O cifră de ~1 s văzută o singură dată = cold start (JIT + primul plan), nu regresie. Când cifra o va cere (59): lista IN → join pe agregat sau tabel temporar, nu index.

---

## Addendum 6 (2026-08-27) — SAF-T S, restanța 74-r4 (felia 18, pasul 1, D18-D1)

Baza de import **`Atlas.Conta.Import1C.Flax`** (282.388 de rânduri în
`RegistruStoc`, 2024-12-31 … 2025-12-31), calea REALĂ a uneltei: `Import1C
--saft-s 2025 9` / `2025 12` în Release, timpul proiecției din raportul
`saft-s-<an>-<lună>-raport.txt` (cronometrul din `Saft1C`, adică
`SaftProiectii.SaftStocuri` cap-coadă, fără XML și fără DUK); șase rulări per
lună, prima aruncată, mediana celorlalte cinci. Fișierele XML rezultate (36,2 /
34,4 MiB) diff-uite linie cu linie contra copiilor de la 74.

**Ce s-a schimbat (D18-D1).** `AgregatStoc` era apelat de două ori (deschidere
`Data ≤ start−1`, închidere `Data ≤ end`, ambele filtrate pe `TipStoc`-urile
raportate) și mai exista o a treia scanare pentru `SoldPeTipStocNeraportat`
(`Data ≤ end`, tipurile FĂRĂ cod). Acum e O SINGURĂ interogare pe
`RegistruStoc` cu `Data ≤ end`, grupată pe `(RepartitorId, LotId, TipStoc)`, cu
sume condiționate în bază (`Initial = Σ(Data < start)`, `Rulaj = Σ(Data ≥
start)` pe cantitate și valoare, plus contoarele de rânduri); deschiderea,
închiderea și soldurile neraportate se derivă în memorie (`SoldPeCheie`).
Precedentele: `Saft` L §terți, `ContabilProiectii.Balanta`.

| Lună | Proiecție ÎNAINTE (5 calde, s) | **Mediană** | Proiecție DUPĂ (5 calde, s) | **Mediană** |
|---|---|---|---|---|
| 09/2025 | 4,2 · 3,9 · 4,2 · 3,9 · 3,9 | **3,9 s** | 4,1 · 3,8 · 4,0 · 4,0 · 4,0 | **4,0 s** |
| 12/2025 | 4,0 · 4,1 · 4,1 · 4,2 · 4,1 | **4,1 s** | 4,3 · 4,0 · 4,1 · 4,4 · 4,1 | **4,1 s** |

**Identitate:** XML 09 (791.464 de linii) și 12 (751.933) — câte O linie
diferită, `<SoftwareVersion>1.0.0+<hash git>` (stamp-ul de build, nu date);
rapoartele (secțiuni, S1–S5, Excluse, Neincluse, avertismente) identice minus
linia de timp. D18-V1 în ModelCheck: pe scena D17-V2 agregatul unic == suma
naivă rând cu rând (9 intrări, 0 diferite; `Consum` 1 rând 4/80,00 == avertismentul).

**`EXPLAIN (ANALYZE, BUFFERS)` în Postgres (docker `contapal-postgres-1`),
09/2025, SQL-ul reconstruit fidel din LINQ (cu `GCRecord = 0` al filtrului
XAF), a doua rulare (cald):**

- ÎNAINTE, deschiderea (`Data ≤ 2025-08-31`, `TipStoc IN (1,5)`): `Index Scan`
  pe `IX_RegistruStoc_LotId` (185.968 rânduri trecute, 96.420 eliminate de
  filtru) → `Incremental Sort` → `GroupAggregate` 73.204 grupe; buffers
  170.791 hit; **128 ms**.
- ÎNAINTE, închiderea (`Data ≤ 2025-09-30`): același plan, 210.401 rânduri,
  82.190 grupe; **133 ms**.
- ÎNAINTE, neraportatele (`TipStoc NOT IN (1,5)`): `Parallel Seq Scan` (2
  workers, 1.129 rânduri), buffers 4.928; **9 ms**.
- DUPĂ, trecerea unică (`Data ≤ 2025-09-30`, fără filtru pe tip, 8 agregate
  condiționate): același `Index Scan` pe `IX_RegistruStoc_LotId` (211.530
  rânduri, 70.858 eliminate) → `Incremental Sort` → `GroupAggregate` 83.248
  grupe; buffers 170.964 hit; **155 ms** (152–159 pe trei rulări).

Adică SQL-ul celor trei scanări era **~270 ms** și a devenit **~155 ms**;
diferența (≈0,1 s) e sub zgomotul măsurătorii cap-coadă. Planificatorul NU
face seq-scan pe scanările mari: alege indexul pe `LotId` fiindcă îi dă
ordinea pentru grupare, iar `Data ≤ end` reține 75 % din tabel — un index pe
`(Data)` sau `(TipStoc, Data)` n-ar fi fost ales și n-ar fi schimbat nimic.

**Index: NU.** Condiția din contract („seq-scan dominant ȘI > 1 s") nu e
îndeplinită pe prima parte; a doua parte e adevărată, dar din ALT motiv.

**Unde sunt, de fapt, cele 4 secunde** (cronometru temporar pe secțiunile lui
`SaftStocuri`, 09/2025, două rulări calde; neconsumat în cod):

| Secțiune | ms (cumulat) | Δ | Ce face |
|---|---|---|---|
| 0–5 profil, societate, conturi + balanță, politică, rândurile lunii | 0 → 250–400 | 0,25–0,4 s | `ConturiSiSolduri` ≈ 0,1 s, rândurile lunii (24.510) ≈ 0,1–0,3 s |
| 6 agregatul unic (D18-D1) | → 650–800 | **0,4 s** | 155 ms SQL + materializarea a 83 k `AgregatStocRand` + `SoldPeCheie` |
| 7 documentele mișcărilor | → 2.030–2.160 | **1,4 s** | `ApiProiectii.CoduriTip` pe cele ~9,3 k documente ale lunii: materializează ENTITĂȚILE polimorf (toate join-urile TPT, change tracking) ca să citească clasa CLR — corect pe o pagină de 500 de rânduri (60b), nu pe o lună de import (atribuirea din prima versiune a acestui addendum, „`CoduriTipPeTipuri` fără filtru", era GREȘITĂ pentru §7 — vezi partea a doua) |
| 8 repartitori + parteneri | → 2.140–2.310 | 0,1–0,15 s | două `IN` cu ~sute de GUID-uri |
| 9 loturi + produse | → 2.390–2.590 | 0,25–0,3 s | `IN` cu 16,7 k loturi, apoi 7,5 k produse cu join la cont |
| 10 `PhysicalStock` | → 2.620–2.800 | 0,2 s | 16.723 intrări în memorie |
| 11 `MovementOfGoods` | → 2.820–3.060 | 0,2–0,25 s | potrivire + grupare + unicitatea referințelor |
| 12–13 master files | → 2.980–3.240 | 0,15–0,2 s | produse/UM/taxe |
| 14 cusături, din care `ComponenteS3` | → 3.860–4.060 | **0,8–1,0 s** | măsurat în partea a doua: agregatul pe `RegistruStoc ≤ end` cu join de 4 niveluri (Lot → Produs → TipMaterial → Cont) grupat pe (Simbol, DocumentId) = 0,2–0,3 s (76,8 k / 103,6 k grupe), agregatul GL pe conturile țintă = 0,2 s (44,3 k / 59,6 k), `CoduriTipPeTipuri` pe cele ~78 k / 105 k documente ale istoricului = ~0,4 s |

**Verdict:** D18-D1 e corect și e curat (un singur pass, cifre identice), dar
ținta contractului (< 1 s/lună) nu se atinge prin el — și nici prin index:
scanarea istoricului n-a fost niciodată costul dominant. **Atribuirea din 74-r4 („proiecția S e O(istoric)") era greșită**: cele trei
scanări ale istoricului costau 0,27 s din 4 s. Cauzele reale, măsurate în
partea a doua: rezolvarea POLIMORFĂ a tipului de document (§7 prin entități,
S3 prin listarea per tip) și, mai mărunt, cele două agregate ale lui S3.
Snapshot lunar de solduri: rămâne NEDESCHIS — nu are ce rezolva.

### Partea a doua (aceeași zi) — tipul documentului o singură dată, `ComponenteS3` măsurat

**(a) `CoduriTip` → `CoduriTipPeTipuri`, o dată, partajat.** §7 folosea
`ApiProiectii.CoduriTip` (entități polimorfe, 60b); acum folosește
`CoduriTipPeTipuri` (ancora `TipDocument.ClrType`, doar Guid-uri per tip),
iar dicționarul rezultat — tipul TUTUROR documentelor bazei — se dă mai
departe lui `ComponenteS3`, care îl recalcula. Filtrul pe id-uri ÎN SQL
(`Where(d => cerute.Contains(d.ID))` în `IdsDocumenteDeTip<T>`) a fost
încercat primul, cum cerea planul, și RESPINS pe cifră: Npgsql îl traduce în
`= ANY(@ids)` (un singur parametru array, deci fără problema limitei de
parametri), dar Postgres îl execută ca |ids| sondări de index PER TIP — pe
cele ~9,3 k documente ale lunii (§7) costă ~0,15 s, pe cele ~78 k / 105 k
documente ale istoricului pe care le cere S3 costă **1,4–1,8 s** (măsurat:
proiecția a URCAT la 4,8–5,1 s). Listarea NEFILTRATĂ a tuturor id-urilor
per tip (205.131 documente pe Flax, ~19 interogări TPT pe coloana `ID`) costă
**~0,4 s** și se face o singură dată — de aici partajarea.

| Lună | ÎNAINTE (D1) | DUPĂ (a): 5 calde (s) | **Mediană** |
|---|---|---|---|
| 09/2025 | 4,0 s | 3,0 · 2,9 · 2,9 · 2,9 · 2,3 | **2,9 s** |
| 12/2025 | 4,1 s | 2,7 · 2,8 · 3,0 · 3,0 · 3,1 | **3,0 s** |

Identitate, din nou: XML 09/12 — 1 linie diferită (`SoftwareVersion`),
rapoarte identice, DUK `ok` pe ambele.

**`ApiProiectii.CoduriTip` are ACEEAȘI problemă** pe orice mulțime mare
(materializează entitățile polimorf ca să citească clasa) — e pe hot-path-ul
API-ului (grupul conex, panoul de imperecheri, ~sute de rânduri, unde e
corect și măsurat la 60b). NU s-a atins în pasul ăsta; se raportează.

**(b) `ComponenteS3`, măsurat înainte de a decide** (cronometru temporar,
09 / 12, două rulări calde): agregatul de stoc cu join-ul de 4 niveluri
**0,21–0,23 s / 0,28–0,30 s** (76.789 / 103.579 grupe), agregatul GL
**0,13–0,21 s / 0,16–0,17 s**, `CoduriTipPeTipuri` pe istoric 0,4 s (acum
partajat, deci 0). Mutarea părții „registru per cont" pe agregatul D1 ar
economisi cel mult agregatul de stoc, adică **≤ 0,3 s < pragul de 0,4 s**
fixat pentru încercare — și ar cere oricum o grupare pe `(LotId,
DocumentId)` în SQL (S3 e spartă pe document), adică un al doilea agregat de
cardinalitate comparabilă. **NU s-a făcut**; rămâne restanță cu cifra.

**Unde sunt cele ~2,9 s rămase** (din profilul pe secțiuni, cu §7 ≈ 0,5 s
acum): §6 agregatul unic 0,4 s, §7 tipuri + documente 0,5 s, §8–13
(repartitori, loturi/produse cu `IN` de 16,7 k GUID-uri, stoc fizic,
mișcări, master files) ~1,2 s în felii de 0,1–0,3 s, §14 cusături ~0,5 s.
Nicio felie nu mai domină; **ținta < 1 s/lună NU e atinsă** (2,9–3,0 s), și
nu mai există un singur vinovat de luat — următorul pas real ar fi §8–13
(listele `IN` cu mii de GUID-uri → join pe agregat, ca la 71/addendum 5) și
serializarea a 16,7 k intrări, fiecare ≤ 0,3 s.

### Reproducere (addendum 6)

`cd nou/tools/Import1C; dotnet build -c Release`, apoi de 6 ori per lună
`dotnet run --project . -c Release --no-build -- --saft-s 2025 9` (și `12`),
citind linia `proiecție … s` din `saft-s-2025-09-raport.txt`. EXPLAIN:
`docker exec contapal-postgres-1 psql -U postgres -d Atlas.Conta.Import1C.Flax
-c "EXPLAIN (ANALYZE, BUFFERS) SELECT r.\"RepartitorId\", r.\"LotId\",
r.\"TipStoc\", SUM(CASE WHEN r.\"Data\" < DATE '2025-09-01' THEN
r.\"Cantitate\" ELSE 0.0 END), … FROM \"RegistruStoc\" r WHERE r.\"GCRecord\" =
0 AND r.\"Data\" <= DATE '2025-09-30' GROUP BY 1, 2, 3"`. Identitatea:
copiile XML de la 74 diff-uite cu două `StreamReader`-e linie cu linie.