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

## Decizia 85 — paginile listelor XAF Blazor

**Executată 2026-09-12.** Baza Privat (clona bazei de import); Postgres cu
`log_min_duration_statement = 0` (exec + plan per comandă), browser cu
`performance.now()`. O pagină = 20 de rânduri; „cmd/pag" = comenzi SQL pe
pagină (COUNT + chei + SELECT). Verdictul: cele trei registre pe `ServerView`
(85-r1 închisă); documentele rămân pe `Server`.

| View × mod | pagina 1 (ms) | salt de pagină | sort `Data` | grupare | cmd/pag | SELECT |
|---|---|---|---|---|---|---|
| `RegistruTva` Server (90.740) | COUNT 5 + 966 | chei 15 + 221 | 604 | 6 + 2 × (5–12 + 230–250) | 2–3 | 156 col, 35 JOIN |
| `RegistruTva` ServerView | COUNT 4 + 0,1 | chei 16 + 0,05 | 20 | 5 + 2 × (4–10 + 0,1) | 2–3 | 8 col, 0 JOIN |
| `RegistruContabil` Server (305.039) | COUNT 37–54 + 12–48 exec / 21–42 plan | chei 41 + 51 | 69 | ≈ 500 (14 grupuri) | 2–3 | 342 col, 66 JOIN |
| `RegistruContabil` ServerView | COUNT 19–29 + 0,6–2 exec / 4–22 plan | chei 5 + 26 | 38 | ≈ 170 | 2–3 | 25 col, 20 JOIN |
| `RegistruStoc` ServerView (282 k) | COUNT 32 + 15 | chei 52 + 34 | 88 | — | 2–3 | 11 col, 6 JOIN |
| FCT Server (19.042; FCL are 40.555) | COUNT 15–135 + 16 | chei 16 + 21 | 44 | 59 + 2 cmd/grup | 2–3 | 129 col, 33 JOIN |

Browser (refresh / sort / grupare, Server → ServerView): `RegistruTva`
1,4 s → 0,3 s / 823 → 125 ms / 691 → 148 ms; `RegistruContabil` 367 → 337 /
295 → 218 / 1237 → 501 ms.

**Cauza pe `RegistruTva` Server** (EXPLAIN ANALYZE 805 ms): INNER JOIN pe
derived-table-ul TPT `DocumentDetalii` (9 LEFT JOIN) → Hash Join cu Seq Scan
pe 337 k linii pentru 20 de rânduri, pentru că în modul `Server` `.Include`
se aplică și coloanelor cu `Index = -1` (coloana rămâne în modelul
view-ului) — restanța 85-r6. Gruparea încarcă primele rânduri ale fiecărui
grup (85-r7); `Refresh` execută pagina de două ori pe `Server` (85-r8).
Lookup-ul de lot cu `Eticheta` calculată: COUNT + pagină ≈ 30 ms per tastă.

---

## Felia 27 (2026-09-17) — perioadele închise, soldurile materializate și forma lui `DocumenteCuRest`

**Măsurare; singura schimbare de cod încercată a fost măsurată și RESPINSĂ.**
Metoda de mai sus, neschimbată: HTTP end-to-end cu `curl`, WebApi Debug,
`Admin`, 6 rulări per endpoint, prima aruncată, mediana celor 5 calde,
parametrii de grilă ai clientului (`skip=0&take=20&requireTotalCount=true`,
plus `sort=[{Data desc}]` pe grilele de documente).

**A/B PE ACEEAȘI BAZĂ.** Singura comparație care atribuie o diferență
închiderilor e cea făcută pe ACELAȘI set de date: două baze diferă din motive
proprii. Deci `Atlas.Conta.Import1C.Flax` (importul integral al lui 2025,
205.186 documente operate) măsurată de două ori —

- **A, cu 11 luni închise**: 01–11/2025 închise, 12/2025 deschisă ⇒ referința e
  11/2025 (171.396 rânduri contabile, 7.790 de stoc, 184.458 partide);
- **B, fără nicio închidere**: lanțul desfăcut prin 11 redeschideri succesive,
  de la cea mai nouă (fiecare rematerializează referința nouă prin `SUM`
  integral) ⇒ zero referințe, zero snapshot-uri, zero partide. După
  măsurătoare lanțul s-a RE-ÎNCHIS în ordine cronologică, cu aceleași cifre la
  rând (171.396 / 7.790 / 184.458) și `Reconstruieste` 0 diferențe.

Contul reper `4111`, contrapartida reper cea de la 59/66 („NOD", 4.861 FCT),
documentul reper de operare FCT-ul cu 49 de linii `FA/EU-2500084872` (copie
Draft datată 15.12.2025, operată și anulată la fiecare rulare).

### Ce aduc perioadele închise (cod neschimbat)

| Probă | B: fără închideri | A: 11 luni închise | Δ | Țintă |
|---|---|---|---|---|
| fișa contului `4111`, decembrie | 187 ms | **122 ms** | −35% | < 100 ms, ratată |
| balanța ANALITICĂ, decembrie | 254 ms | **210 ms** | −17% | < 100 ms, ratată |
| `documente-cu-rest`, contrapartida-reper | 171 ms | 181 ms | zgomot | < 150 ms (vezi mai jos) |
| `sold-parteneri` la 31.12 | 290 ms | 219 ms | −24% | informativ |
| `sold-stoc` la zi | 153 ms | 50 ms | −67% | informativ |
| balanța sintetică, decembrie | 83 ms | 58 ms | −30% | informativ |
| operarea unui FCT cu 49 de linii | 411 ms | 394 ms | −4% | nu mai lentă, atinsă |

Comenzile lanțului: `Reconstruieste` (recalcul integral + rescriere, o
referință) **6,9–9,4 s**; o închidere **1,0 s** (ianuarie) → **5,2 s**
(noiembrie), crescător cu volumul cumulat; o redeschidere **0,0 s** (ianuarie,
fără referință nouă de scris) → **6,1 s** (noiembrie).

**Ca CONTEXT, altă bază** (`Atlas.Conta.BackOffice.Privat`, clona de dev, zero
închideri, aceeași zi și aceeași mașină): fișa 190 ms, balanța analitică
266 ms, `documente-cu-rest` 131 ms, `sold-parteneri` 298 ms, `sold-stoc`
152 ms, balanța sintetică 82 ms, operarea 415 ms. Cifrele NU se compară cu cele
de mai sus rând cu rând.

### Forma lui `DocumenteCuRest`: patru variante măsurate, niciuna adoptată

Diagnosticul: planul lega agregatul `Imperecheri` — un tabel DERIVAT
(`Asignari(...).GroupBy(...)`) — prin `Nested Loop Left Join`, cu
`Rows Removed by Join Filter: 2.184.985` și două `Seq Scan` peste cele 44.448
de rânduri, IDENTIC cu și fără referință. `PartideDeschise` nu e problema: e
deja atinsă prin index, per rând exterior. Deci nu partidele feliei 27 o fac
lentă, ci mulțimea de candidați de la 59 plus forma legăturii.

Măsurat pe aceeași bază, în starea A (11 luni închise). **Coloana a treia e
cea care decide**: forma NEFILTRATĂ și FĂRĂ `LIMIT`, adică exact ce consumă
`PerioadaService.RestScadent` (`DocumenteCuRest(os, laData: ultimaZi).ToList()`)
la închiderea unei perioade.

| variantă | grilă filtrată | grilă nefiltrată | **nefiltrat, FĂRĂ `LIMIT`** | `Imperecheri` în plan |
|---|---|---|---|---|
| **V0** — legături ca tabele derivate (rămâne) | 181 ms | 423 ms | **220–232 ms** | 2 × `Seq Scan`, nested loop, 2,18 M rânduri respinse |
| V1 — AMBELE legături corelate | 200 ms | 2.902 ms | — | blocul `Imperecheri` intră de 3 ori, re-evaluat per rând |
| V2 — doar fereastra corelată (măsurată, RESPINSĂ) | **82 ms** | 473 ms | **1.024–1.061 ms** | 8 × `Index Scan`, ZERO `Seq Scan` |
| V2′ — `Asignari` ca două sume corelate | 87 ms | 539 ms | **1.230–1.245 ms** | 16 × `Index Scan`, ZERO `Seq Scan` |

**Verdict: V0 rămâne; fixul a fost MĂSURAT și RESPINS, nu neîncercat.** V2 e
tentantă — panoul filtrat 181 → 82 ms (51 ms în starea fără închideri), pagina
88,7 → 11,7 ms SQL, `EXPLAIN` 194 → 18,8 ms, `Seq Scan`-urile dispar complet —
dar mută costul pe calea NEPLAFONATĂ: 220 ms → 1,02 s, adică 4,6×, pe forma pe
care o consumă constatarea de rest scadent. O sută de milisecunde câștigate pe
un panou nu se plătesc cu 0,8 s adăugate unei comenzi de închidere. Restanța,
cu cifrele de mai sus: **F27-r16** — candidații de la 59 și forma legăturilor
se rezolvă ÎMPREUNĂ, nu separat.

**Prețul, spus întreg**: V2 câștigă real pe panoul filtrat (−100 ms) și
elimină `Seq Scan`-urile, dar plătește 0,8 s în plus pe forma pe care
`RestScadent` o materializează integral, iar costul ăla crește cu baza. Azi
familia e `Ignorat` în seed pe ambele profiluri, deci nici nu se caută — dar
seed-ul e o valoare de politică, nu o garanție de formă: un client care ridică
severitatea plătește diferența. Rămâne deschisă opțiunea unei forme DEPENDENTE
de `contrapartidaId` (corelată când vine filtru, derivată altfel), cu probele
ei, dacă panoul devine vreodată gâtul real — dar ea pune o ramură de plan în
hot-path și nu se decide fără cerință.

**Lecție de metodă, scrisă ca să nu se repete**: o măsurătoare pe grilă
PAGINATĂ ascunde costul căii care consumă tot. Pe `take=20`, V2 arăta ca o
pierdere de 12% pe cazul nefiltrat (423 → 473 ms), fiindcă `LIMIT 20` oprește
execuția devreme; fără `LIMIT`, aceeași variantă costă de 4,6× mai mult.
Orice proiecție care are ȘI consumatori care o materializează integral se
măsoară pe ambele forme.

**Rezultate negative, păstrate fiindcă valorează cât cele pozitive**:
- EF Core **nu emite `LATERAL`** pentru forma corelată, ci subinterogări
  scalare corelate. Corelând AMBELE legături (V1), blocul `Imperecheri` (un
  `UNION ALL` peste cele două laturi) ajunge de trei ori în interogare și se
  re-evaluează per rând — 2,9 s pe cazul nefiltrat.
- Desfacerea lui `Asignari` în două sume corelate (V2′) se traduce curat, dar e
  mai slabă decât V2 pe toate coloanele: `dinFereastra` e folosit în patru
  locuri, deci tabela e atinsă de 16 ori în loc de 8.
- **Niciun index nu lipsește.** `IX_Imperecheri_DocumentId` și
  `IX_Imperecheri_DocumentStingatorId` există amândouă și sunt folosite de
  formele corelate (zero `Seq Scan` în planurile V2/V2′); nicio migrație nu ar
  schimba ceva. Nimeni să nu reia drumul ăsta.

**Despre ținta de „< 150 ms"**: ea a fost calibrată pe `Atlas.Conta.BackOffice.Privat`,
unde V0 măsura 147 ms la pasul 6 (și 131 ms azi, la re-măsurare). Cele 181 ms
sunt de pe `Atlas.Conta.Import1C.Flax` — altă bază, alt set de date. Pe baza pe
care a fost pusă, ținta NU e încălcată.

### Cele două ținte ratate care rămân, și de ce nu se ating azi

**Fișa de cont: calea de date 7 ms, end-to-end 122 ms.** Interogarea proiecției
costă **7 ms**: snapshot-ul taie fereastra la decembrie, deci `WindowAgg`
rulează peste 12.953 atomi, nu peste cei 144.248 ai contului (la 66 pagina
costa 244 ms). Restul e CADRU: **58 de instrucțiuni SQL per cerere**
(bootstrap-ul de securitate XAF per ObjectSpace, fiecare sub 0,03 ms),
hidratarea documentelor paginii (15 ms), `count` (4 ms), serializarea. Ținta se
ratează din AFARA feliei 27 (F27-r14).

**Balanța analitică: cardinalitatea cheii, și NU se rezolvă din `work_mem`.**
~161 ms execuție, din care agregarea în **71.167 de grupe `Cont×Repartitor`**.
Snapshot-ul E folosit și reduce intrarea de 3,5× (171.396 rânduri de snapshot +
DOAR decembrie din `RegistruContabil`, prin `Index Scan using
IX_RegistruContabil_Data`, în loc de ~608k rânduri unpivotate ale anului).
`Sort Method: external merge Disk: 3648–4952 kB` la `work_mem` implicit de 4 MB
arată ca un buton de deployment — **măsurat, nu e**:

| `work_mem` pe sesiune | mediana a 4 rulări calde (SQL pur) | plan |
|---|---|---|
| 4 MB (implicit) | **136 ms** | `Partial GroupAggregate` paralel (2 lucrători) + `external merge` pe disc |
| 8 MB | 155 ms | `HashAggregate`, un singur proces |
| 16 MB | 162 ms | `HashAggregate`, un singur proces |
| 64 MB | 183 ms | `HashAggregate` cu `Memory Usage: 96273kB`, un singur proces |

Cu mai multă memorie planificatorul RENUNȚĂ la paralelism în favoarea unui hash
aggregate secvențial și iese mai prost: sortarea pe disc nu e gâtul, ci
producerea celor 71.167 de rânduri grupate. Ținta „< 100 ms" fusese pusă pe
cifra „lună = 84 ms" a addendumului 66, care e IANUARIE — o lună fără sold
inițial, deci cu puține grupe; comparabilul real al lui decembrie e „an =
269 ms" (F27-r15).

Observație colaterală: cu snapshot balanța analitică pe decembrie are 71.167 de
rânduri, fără snapshot 72.910 — diferența sunt cheile cu debit ȘI credit
cumulat zero, pe care snapshot-ul nu le scrie.

### Reproducere (felia 27)

`Atlas.Conta.Import1C.Flax` nu mai e oarbă la HTTP (addendumul 3): rulează
`Atlas.Conta.BackOffice.Blazor.Server --updateDatabase --forceUpdate --silent`
cu `ConnectionStrings__ConnectionString` și `ProfilContabil` în MEDIU (pe linia
de comandă NU ajung în `IConfiguration`), care creează rolurile și cei patru
utilizatori dev; apoi WebApi pe aceeași bază, cu portul impus prin
`-- --urls=…` (`dotnet run` ia altfel URL-urile din `launchSettings`).
Contul `4111` = `01a0aea3-9ec7-7577-9dd6-ffbaeb2382a7`, contrapartida-reper =
`01a0aea4-c8d4-7e06-8664-9b508af4ced8` (pe `Privat`:
`019fa5d0-cbcd-7461-9982-0809a2075b64`, respectiv
`019fa5d1-bb95-7e9c-b168-3169642f4267`). Desfacerea și re-închiderea lanțului,
ca și operarea, se fac în proces, pe un ObjectSpace standalone ca al lui
ModelCheck (`PerioadaService.Redeschide`/`Inchide` în buclă; copie Draft prin
metadata EF, `MotorOperare.Opereaza`, `AnuleazaOperarea`, ștergere — 0 reziduu
viu). Forma „fără `LIMIT`" se măsoară cu `EXPLAIN (ANALYZE, BUFFERS)` pe SQL-ul
real al variantei, cu predicatul de contrapartidă scos și fără `LIMIT`.
`log_min_duration_statement` resetat la final (verificat `-1`); hosturile
oprite.

---

## Felia 28 (2026-09-18) — TPT → TPH, A/B pe aceeași bază de conținut

**Măsurare, decizia 89, regula de oprire F28-D8.** Metoda felia 27,
neschimbată: HTTP end-to-end cu `curl`, WebApi Debug, `Admin`, 6 rulări per
endpoint, prima aruncată, mediana celor 5 calde, cu parametrii de grilă ai
clientului (`skip=0&take=20&requireTotalCount=true`, plus `sort=[{Data desc}]`
pe grilele de documente). Cifrele marcate „în proces” sunt măsurate în
proces, ca operarea la F27. Zgomotul observat e ±5–10 %.

**A/B PE ACEEAȘI BAZĂ DE CONȚINUT, pe două mapări.** Schema nu se poate
schimba pe loc, fiindcă lanțul de migrații s-a resetat. A și B sunt deci două
baze cu ACELAȘI conținut, măsurate pe aceeași mașină, una după alta:

- **A, TPT**: clona înghețată `Atlas.Conta.Import1C.Flax.TPT`, servită de
  codul `ba20faa` (închiderea feliei 27);
- **B, TPH**: `Atlas.Conta.Import1C.Flax.TPH`, importul integral al feliei 28,
  servit de HEAD `14bc649`;
- aceeași stare a lanțului pe ambele: 01–11/2025 închise, 12/2025 deschisă,
  referința 11/2025 = 171.396 / 7.790 / 184.458;
- `VACUUM ANALYZE` pe ambele baze și câte două treceri pe fiecare parte, cu
  hostul repornit între ele.

Reperele sunt cele de la felia 27: contul `4111`, contrapartida-reper și
FCT-ul cu 49 de linii (copie Draft operată și anulată la fiecare rulare). În
plus, trei cifre atribuite TPT: `CoduriTip` pe extrasul cu 335 de stingeri,
`RegistruTva` în modul `Server` și SAF-T D406 S.

| Probă | A: TPT (trecerea 1 / 2) | B: TPH (trecerea 1 / 2) | Δ | Verdict |
|---|---|---|---|---|
| fișa contului `4111`, decembrie | 144 / 126 ms | **61 / 55 ms** | −57 % | mai bun |
| balanța analitică, decembrie | 211 / 206 ms | 201 / 192 ms | −5 % | zgomot |
| `sold-parteneri` la 31.12 | 212 / 218 ms | 220 / 206 ms | — | zgomot |
| `sold-stoc` la zi | 52 / 54 ms | 57 / 50 ms | — | zgomot |
| balanța sintetică, decembrie | 55 / 53 ms | 48 / 44 ms | −15 % | mai bun |
| operarea unui FCT cu 49 de linii (în proces) | 404 / 424 ms | **230 / 199 ms** | −48 % | mai bun |
| `documente-cu-rest`, contrapartida-reper | 176 / 188 ms | 169 / 158 ms | −8 % | zgomot / ușor mai bun |
| `CoduriTip`, extrasul cu 335 de stingeri, HTTP | 214 / 247 ms | **23 / 23 ms** | ≈ 10× | mai bun |
| `CoduriTip`, în proces | 35 ms | **1,6 ms** | ≈ 20× | mai bun |
| `RegistruTva` `Server`, EF în proces | 875 ms | **14,5 ms** | ≈ 60× | mai bun |
| `RegistruTva` `Server`, EXPLAIN ANALYZE | 1.099 / 1.703 ms | **12 ms** | ≈ 100× | mai bun |
| D406 S 09/2025, în proces | 2,5 s | 2,5 s | — | zgomot |
| D406 S 12/2025, în proces, la RECE | 2,7 s | 3,0 s (intercalat 2,7 → 2,9) | +0,2 s (+7 %) | mai prost DOAR la rece (F28-r5) |
| D406 S prin HTTP, cald | 09: 1224–1298 ms; 12: 1232–1274 ms | 09: 1081–1198 ms; 12: 1243–1277 ms | — | zgomot |

Ca CONTEXT, nu ca termen de comparație, cifrele anterioare pe alte baze:
- fișa 122 ms, balanța analitică 210 ms, operarea 394 ms (F27, A);
- `CoduriTip` 185 ms și D406 2,9–3,0 s (contractul F28);
- `RegistruTva` Server 805 ms (85, Privat).

### EXPLAIN pe cifrele atribuite TPT

- **`RegistruTva` `Server`**:
  - A: 42 de JOIN-uri, Hash Join pe cele 338.594 de rânduri din
    `DocumentDetalii`; planificare 15–18 ms, execuție 1,1–1,7 s.
  - B: 4 JOIN-uri, top-N heapsort și Nested Loop pe PK; planificare 2 ms,
    execuție 12 ms.
- **`CoduriTip`**:
  - A: materializare polimorfă, `= ANY` cu 77 de JOIN-uri; bind 41 +
    execuție 25 ms, seq scan pe toate frunzele.
  - B: `SELECT "ID","ClrType" … = ANY`, Index Scan pe PK; 0,46 + 0,2 ms.
  - Pe toată cererea, SQL-ul scade de la 127 + 30 ms la 2,6 + 1,1 ms.
- **D406**:
  - În A, rezoluția tipului costa 20 de listări per tip, adică 60 de
    instrucțiuni și 166,8 ms.
  - SQL-ul total al proiecției scade de la 675 la 525 ms (12/2025) și de la
    637 la 480 ms (09/2025).
  - S3 cu `= ANY` pe istoric: 78.287 de id-uri costă 16,3 + 41,6 ms, 105.530
    de id-uri 20 + 55 ms. Costul de 1,4–1,8 s din addendumul 6 NU revine.

### Verdict

Nicio cifră caldă nu e mai proastă peste zgomot. Câștigurile se concentrează
exact pe cele trei cifre atribuite TPT și pe căile care citeau pe bază:
- fișa de cont −57 %;
- operarea −48 %;
- `CoduriTip` ≈ 10× pe HTTP;
- `RegistruTva` Server ≈ 60×.

Singura cifră mai proastă e D406 S pe 12/2025 la RECE, +0,2 s. Cauza e
demonstrată: un cost unic per proces.
- În ACELAȘI proces, prima chemare costă 2509/2552 ms în A și 2621/2707 ms
  în B.
- Medianele chemărilor calde sunt 1553/1706 ms în A și 1621/1659 ms în B,
  adică egale; SQL-ul e mai mic în B.
- Cauza probabilă e JIT-ul sau compilarea EF a formei noi de interogare, dar
  nu e izolată prin profil. Conform F28-D8 cifra nu blochează felia și rămâne
  restanță cu nume: F28-r5.
- Ieșirea D406 e identică între A și B după normalizarea GUID-urilor.

### Reproducere (felia 28)

- A: clona înghețată `Atlas.Conta.Import1C.Flax.TPT`, servită de codul
  `ba20faa`.
- B: `Atlas.Conta.Import1C.Flax.TPH`, servită de `14bc649`.
- Ambele au fost aduse la aceeași stare a lanțului și au primit
  `VACUUM ANALYZE`. Pornirea hosturilor și reperele urmează §Reproducere
  (felia 27); id-urile reper se rezolvă pe fiecare bază, fiindcă bazele
  recreate nu păstrează GUID-urile.
- Artefactele pasului 3 sunt în `run-f28/pas3/`, necomise.

## TR-D8 SAF-B8 (2026-09-30) — SAF-T L și S pe cub, scara sintetică

**Reprodus de Codex pe `37ac61c`, 2026-09-30: B8-RV3 închis.**
`run-verificari/perf-saft-20260930-222637`, baza proprie `.CodexSaftS3R`:
48 de măsurători (matricea completă, L/S, rece/cald), criteriile trecute,
43/26 comenzi SQL constante, șase seturi de planuri și șase XML-uri k=64
acceptate de DUK fără atenționări. Saltul maxim la cald: L ×1,43,
S ×1,58 (din tabelul rotunjit). Condițiile B8-RV3-P și amendamentul
B8-RV3-A sunt păstrate; aceasta nu certifică transportul prin proxy-ul
Windows și nu închide SAFT-r4/r5. [Reverificarea](../nucleu/tr-d8-saft-b8-review-codex.md).

Contract: [B8-D3, B8-RV3-P și B8-RV3-A](../nucleu/tr-d8-saft-contract.md). Rularea
care închide B8-D3 este `run-verificari/perf-saft-20260930-213218`: XML-uri,
planuri `EXPLAIN (ANALYZE, BUFFERS)` la k = 64 pe fiecare m, `perf.log`,
`duk.log`, `rulare.json` (commit, hash DLL). Baza este
`Atlas.Conta.ModelCheck.Privat.ClaudeS3`. Comanda este
`nou/tools/ModelCheck/scripts/perf-saft-container.ps1`: `ModelCheck --perf-saft`
rulează într-un container `dotnet/aspnet:10.0`, în rețeaua containerului
Postgres, apoi `--perf-saft-duk` rulează pe Windows.

**De ce în container.** Postgres-ul de dezvoltare (5444) stă în spatele
proxy-ului de porturi Docker Desktop (`wslrelay` / `com.docker.backend`). Pe
această cale, o cerere parametrizată de ~5–40 KB plătește ~43 ms. Măsurat pe
`unnest($1::uuid[])`:

- 200 de UUID-uri: 0,5 ms;
- 256–2.000: 44 ms;
- 3.000–4.000: 5–9 ms.

În rețeaua containerului, aceeași interogare durează 1–3,7 ms pe toată scara
(`run-verificari/saft-b8-rv3-transport/`). Pragul ține de mașina de
dezvoltare, nu de export, de Npgsql sau de Postgres. Rularea din 30.09 16:28
îl atribuise greșit buclei locale Windows.

**Scena.** O unitate cuprinde:

- FCT (stoc 10 × 10 + serviciu 50, N21) cu NIR conex;
- PLT 70, legată 50 de FCT;
- BCS 2, BTR 1, DSC 1 și ASM 1 din lotul FCT;
- FCL 100 cu INC 60 legată.

Luna măsurată are k ∈ {1, 4, 16, 64} unități. Istoricul are m ∈ {0, 6, 12}
luni închise × 16 unități, plus un lot „lung” consumat în fiecare lună. Totul
se face prin comenzi reale; scena se purjează la final. După fiecare treaptă
și înaintea măsurării rulează `ANALYZE` (B8-RV3-A; fără el, scena m = 0 se
măsura pe statistici de tabelă goală, vezi mai jos).

**Fazele.** Fiecare punct (k, m, modul) rulează într-un proces-copil nou.
„Rece” este prima rulare, cu pool gol, JIT rece și modelul EF construit
atunci. „Cald” este a doua rulare, în același proces. Captura SQL a
procesului nou cere `LoggingCacheTime = 0`: EF ține în cache 1 s starea
„diagnostic activ”, iar fără setare exportul rece nu emite evenimente.

**Măsurile:**

- `ms` = exportul complet, cu SQL (proiecția, fără XML); `ms SQL` = Σ duratelor
  comenzilor;
- rândurile citite din `DataReaderClosing`;
- octeții alocați (`GC.GetTotalAllocatedBytes`);
- vârful gestionat, eșantionat la 2 ms;
- vârful setului de lucru al procesului (`PeakWorkingSet64`);
- XML-ul scris pe disc, validat XSD în proces; DUK separat, la k = 64.

| m | k | modul | faza | ms | comenzi | ms SQL | rânduri | alocați MiB | vârf gestionat MiB | vârf set lucru MiB | XML KiB | ms XML | tranzacții | facturi | plăți | mișcări | poziții | rânduri Postare | rânduri snapshot | server max ms |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| 0 | 1 | L | rece | 613 | 43 | 38 | 962 | 24.8 | 31.7 | 197.8 | 44 | 15 | 8 | 3 | 2 | 0 | 0 | 68 | 0 |  |
| 0 | 1 | L | cald | 42 | 43 | 21 | 962 | 2.4 | 29.1 | 202.8 | 44 | 12 | 8 | 3 | 2 | 0 | 0 | 68 | 0 |  |
| 0 | 4 | L | rece | 603 | 43 | 37 | 1172 | 25.1 | 31.6 | 197.8 | 127 | 43 | 26 | 9 | 8 | 0 | 0 | 191 | 0 |  |
| 0 | 4 | L | cald | 40 | 43 | 21 | 1172 | 2.4 | 29.8 | 202.8 | 127 | 10 | 26 | 9 | 8 | 0 | 0 | 191 | 0 |  |
| 0 | 16 | L | rece | 636 | 43 | 43 | 2012 | 26.7 | 31.7 | 198.0 | 459 | 34 | 98 | 33 | 32 | 0 | 0 | 683 | 0 |  |
| 0 | 16 | L | cald | 53 | 43 | 24 | 2012 | 4.0 | 33.9 | 206.8 | 459 | 24 | 98 | 33 | 32 | 0 | 0 | 683 | 0 |  |
| 0 | 64 | L | rece | 707 | 43 | 50 | 5372 | 34.3 | 32.9 | 201.9 | 1787 | 94 | 386 | 129 | 128 | 0 | 0 | 2651 | 0 |  |
| 0 | 64 | L | cald | 73 | 43 | 31 | 5372 | 11.6 | 37.1 | 213.4 | 1787 | 122 | 386 | 129 | 128 | 0 | 0 | 2651 | 0 | 4.4 |
| 0 | 1 | S | rece | 506 | 26 | 28 | 1496 | 22.1 | 31.4 | 196.6 | 21 | 11 | 0 | 0 | 0 | 8 | 4 | 39 | 1 |  |
| 0 | 1 | S | cald | 21 | 26 | 9 | 1496 | 1.9 | 25.7 | 200.9 | 21 | 3 | 0 | 0 | 0 | 8 | 4 | 39 | 1 |  |
| 0 | 4 | S | rece | 526 | 26 | 29 | 1616 | 22.4 | 31.6 | 196.7 | 54 | 14 | 0 | 0 | 0 | 26 | 13 | 90 | 1 |  |
| 0 | 4 | S | cald | 23 | 26 | 10 | 1616 | 2.1 | 26.2 | 201.0 | 54 | 5 | 0 | 0 | 0 | 26 | 13 | 90 | 1 |  |
| 0 | 16 | S | rece | 556 | 26 | 30 | 2096 | 23.2 | 31.5 | 197.1 | 185 | 17 | 0 | 0 | 0 | 98 | 49 | 294 | 1 |  |
| 0 | 16 | S | cald | 27 | 26 | 11 | 2096 | 2.9 | 28.0 | 201.4 | 185 | 11 | 0 | 0 | 0 | 98 | 49 | 294 | 1 |  |
| 0 | 64 | S | rece | 568 | 26 | 35 | 4016 | 26.4 | 31.8 | 196.7 | 710 | 41 | 0 | 0 | 0 | 386 | 193 | 1110 | 1 |  |
| 0 | 64 | S | cald | 42 | 26 | 16 | 4016 | 6.2 | 35.9 | 207.5 | 710 | 34 | 0 | 0 | 0 | 386 | 193 | 1110 | 1 | 1.4 |
| 6 | 1 | L | rece | 719 | 43 | 49 | 952 | 24.7 | 31.8 | 197.6 | 37 | 14 | 7 | 2 | 2 | 0 | 0 | 63 | 0 |  |
| 6 | 1 | L | cald | 43 | 43 | 25 | 952 | 2.0 | 28.7 | 202.3 | 37 | 5 | 7 | 2 | 2 | 0 | 0 | 63 | 0 |  |
| 6 | 4 | L | rece | 625 | 43 | 43 | 1162 | 25.1 | 31.8 | 197.3 | 120 | 18 | 25 | 8 | 8 | 0 | 0 | 186 | 0 |  |
| 6 | 4 | L | cald | 46 | 43 | 26 | 1162 | 2.4 | 29.8 | 202.2 | 120 | 9 | 25 | 8 | 8 | 0 | 0 | 186 | 0 |  |
| 6 | 16 | L | rece | 615 | 43 | 47 | 2002 | 26.8 | 31.6 | 197.3 | 453 | 36 | 97 | 32 | 32 | 0 | 0 | 678 | 0 |  |
| 6 | 16 | L | cald | 52 | 43 | 28 | 2002 | 4.0 | 33.8 | 206.1 | 453 | 25 | 97 | 32 | 32 | 0 | 0 | 678 | 0 |  |
| 6 | 64 | L | rece | 656 | 43 | 60 | 5362 | 34.3 | 32.8 | 201.7 | 1781 | 100 | 385 | 128 | 128 | 0 | 0 | 2646 | 0 |  |
| 6 | 64 | L | cald | 74 | 43 | 36 | 5362 | 11.5 | 37.0 | 213.1 | 1781 | 115 | 385 | 128 | 128 | 0 | 0 | 2646 | 0 | 4.5 |
| 6 | 1 | S | rece | 541 | 26 | 34 | 2066 | 23.0 | 31.8 | 197.0 | 360 | 25 | 0 | 0 | 0 | 7 | 292 | 35 | 387 |  |
| 6 | 1 | S | cald | 28 | 26 | 13 | 2066 | 2.9 | 28.3 | 201.3 | 360 | 19 | 0 | 0 | 0 | 7 | 292 | 35 | 387 |  |
| 6 | 4 | S | rece | 534 | 26 | 33 | 2186 | 23.2 | 31.7 | 196.9 | 393 | 27 | 0 | 0 | 0 | 25 | 301 | 86 | 387 |  |
| 6 | 4 | S | cald | 37 | 26 | 17 | 2186 | 3.3 | 29.0 | 201.4 | 393 | 28 | 0 | 0 | 0 | 25 | 301 | 86 | 387 |  |
| 6 | 16 | S | rece | 571 | 26 | 36 | 2666 | 24.2 | 31.4 | 196.9 | 525 | 38 | 0 | 0 | 0 | 97 | 337 | 290 | 387 |  |
| 6 | 16 | S | cald | 37 | 26 | 16 | 2666 | 4.0 | 30.9 | 202.8 | 525 | 32 | 0 | 0 | 0 | 97 | 337 | 290 | 387 |  |
| 6 | 64 | S | rece | 550 | 26 | 36 | 4586 | 27.4 | 31.4 | 196.5 | 1051 | 57 | 0 | 0 | 0 | 385 | 481 | 1106 | 387 |  |
| 6 | 64 | S | cald | 62 | 26 | 19 | 4586 | 7.2 | 37.4 | 208.8 | 1051 | 57 | 0 | 0 | 0 | 385 | 481 | 1106 | 387 | 3.7 |
| 12 | 1 | L | rece | 649 | 43 | 48 | 952 | 24.9 | 31.8 | 197.7 | 37 | 13 | 7 | 2 | 2 | 0 | 0 | 63 | 0 |  |
| 12 | 1 | L | cald | 44 | 43 | 26 | 952 | 2.0 | 28.8 | 202.4 | 37 | 5 | 7 | 2 | 2 | 0 | 0 | 63 | 0 |  |
| 12 | 4 | L | rece | 643 | 43 | 48 | 1162 | 25.1 | 31.8 | 197.5 | 121 | 24 | 25 | 8 | 8 | 0 | 0 | 186 | 0 |  |
| 12 | 4 | L | cald | 51 | 43 | 30 | 1162 | 2.4 | 29.7 | 202.3 | 121 | 13 | 25 | 8 | 8 | 0 | 0 | 186 | 0 |  |
| 12 | 16 | L | rece | 640 | 43 | 50 | 2002 | 26.8 | 31.5 | 197.6 | 453 | 31 | 97 | 32 | 32 | 0 | 0 | 678 | 0 |  |
| 12 | 16 | L | cald | 60 | 43 | 32 | 2002 | 4.0 | 33.8 | 206.6 | 453 | 32 | 97 | 32 | 32 | 0 | 0 | 678 | 0 |  |
| 12 | 64 | L | rece | 664 | 43 | 60 | 5362 | 34.2 | 32.7 | 201.9 | 1783 | 103 | 385 | 128 | 128 | 0 | 0 | 2646 | 0 |  |
| 12 | 64 | L | cald | 81 | 43 | 40 | 5362 | 11.5 | 36.9 | 213.6 | 1783 | 99 | 385 | 128 | 128 | 0 | 0 | 2646 | 0 | 5.2 |
| 12 | 1 | S | rece | 558 | 26 | 35 | 2642 | 24.2 | 32.0 | 197.0 | 703 | 43 | 0 | 0 | 0 | 7 | 580 | 35 | 771 |  |
| 12 | 1 | S | cald | 37 | 26 | 16 | 2642 | 3.9 | 31.0 | 205.5 | 703 | 93 | 0 | 0 | 0 | 7 | 580 | 35 | 771 |  |
| 12 | 4 | S | rece | 618 | 26 | 39 | 2762 | 24.4 | 31.7 | 196.8 | 736 | 70 | 0 | 0 | 0 | 25 | 589 | 86 | 771 |  |
| 12 | 4 | S | cald | 36 | 26 | 16 | 2762 | 4.1 | 31.5 | 203.1 | 736 | 38 | 0 | 0 | 0 | 25 | 589 | 86 | 771 |  |
| 12 | 16 | S | rece | 585 | 26 | 41 | 3242 | 25.1 | 32.1 | 197.2 | 867 | 49 | 0 | 0 | 0 | 97 | 625 | 290 | 771 |  |
| 12 | 16 | S | cald | 39 | 26 | 17 | 3242 | 4.9 | 33.5 | 205.3 | 867 | 39 | 0 | 0 | 0 | 97 | 625 | 290 | 771 |  |
| 12 | 64 | S | rece | 577 | 26 | 46 | 5162 | 28.7 | 31.5 | 197.4 | 1394 | 96 | 0 | 0 | 0 | 385 | 769 | 1106 | 771 |  |
| 12 | 64 | S | cald | 80 | 26 | 27 | 5162 | 8.4 | 37.5 | 209.9 | 1394 | 79 | 0 | 0 | 0 | 385 | 769 | 1106 | 771 | 4.9 |

**Criteriile B8-D3 aprobate, toate OK:**

- **Comenzi SQL constante:** 43 pe L, 26 pe S, în toate cele 12 puncte, rece
  și cald.
- **Opening S nu citește istoricul.** Rândurile din `Postare` la k fix nu
  cresc cu m (k = 64: 1.110 / 1.106 / 1.106). Snapshot-ul urmează pozițiile
  deschise (1 / 387 / 771 rânduri la 193 / 481 / 769 de poziții).
- **Liniaritate** f(4k) ≤ 1,25 × 4 × f(k), pe durata totală și pe alocări.
  Pasul cel mai mare, cald:
  - L m = 0, k 16 → 64: 53 → 73 ms, 4,0 → 11,6 MiB (×2,9);
  - S m = 12, k 16 → 64: 39 → 80 ms (×2,1).

  Rece, durata e dominată de pornire (~0,5–0,7 s: modelul EF, JIT) și
  crește sub 20% pe toată scara.
- **k = 64 exportat integral pe fiecare m:** fără refuzuri, XSD valid, DUK
  J2.2.18 fără atenționări pe cele 6 fișiere (~2,2 s fiecare).
- **Suplimentar:** execuția maximă pe server la k = 64 este 5,2 ms (L) și
  4,9 ms (S).

**Statisticile vechi (SAFT-r5).** Prima rulare pe aceleași condiții, dar
fără `ANALYZE` (`run-verificari/perf-saft-20260930-212405`), a picat într-un
singur punct: S cald, m = 0, k 16 → 64, 46 → 236 ms. Scena m = 0 se măsoară
la câteva secunde după purjă și reumplere, înaintea autoanalyze. Postgres
estimează `Tranzactie` la 1 rând (real: 642). Citirea faptelor fiscale
(`Cub/Citiri/Fiscale.Fapte`) devine un Nested Loop cu 642 de scanări pe
interval: 108.189 de buffere, 60,5 ms în loc de 1,4 ms. Riscul e real după
orice inserare masivă. Unealta de migrare/import va rula `ANALYZE`
(SAFT-r5).

**Balanța scanează istoricul pe server (SAFT-r4, neschimbat).** Soldurile de
cont ale ambelor fișiere vin din `ContabilProiectii.Balanta` pe
ObjectSpace-ul securizat, care recitește postările: un rând întors pe cont,
dar scanare pe `Postare_Contabil` + `Postare_Stoc`. Cu accesul complet
verificat (SAF-D4), balanța poate porni din snapshot. Rămâne la gate-ul
transversal.

**Istoric.** Rularea `perf-saft-20260930-162802` (review Codex B8-RV3) nu
acoperea matricea aprobată și trecea criteriile doar după precizări făcute
după măsurare: timpul fără SQL și alocări cu 50%. Precizările s-au retras
(B8-RV3-P). Pragul ei de ~43 ms era al proxy-ului, iar faza ei „rece”
rula în același proces.

Pragul absolut și planul pe volum real rămân la gate-ul transversal. Baza de
volum reală nu există după C102, deci FZ-r3 rămâne activă.
