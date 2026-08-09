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

## Reproducere

Scriptul de măsurare (curl, 6 rulări/endpoint) e trecător (scratchpad);
rețeta: pornește WebApi pe baza Privat, autentifică `Admin`/gol, lovește
endpoint-urile cu parametrii de grilă de mai sus. Pentru SQL:
`ALTER SYSTEM SET log_min_duration_statement=50` + `pg_reload_conf()` (și
RESET la final), apoi `EXPLAIN (ANALYZE, BUFFERS)` pe statement-ul din
`docker logs`.
