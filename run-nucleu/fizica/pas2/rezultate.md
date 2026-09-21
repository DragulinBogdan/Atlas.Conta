# Măsurătorile pasului 2 — cifrele

Bazele: `Atlas.Conta.Nucleu.Fizica.x1` (1.162.622 postări) și `.x10`
(11.562.545). Disciplina FZ-D8: `EXPLAIN (ANALYZE, BUFFERS)`, 6 rulări, prima
aruncată, **mediana celor 5 calde** (min–max în paranteză). Ordinea: toate
interogările pe o formă, apoi forma următoare. Niciun parametru de Postgres n-a
fost atins:

```
shared_buffers 160 MB   work_mem 4 MB   maintenance_work_mem 64 MB
max_parallel_workers_per_gather 2   jit on   effective_cache_size 5 GB
PostgreSQL 18.4 (Debian), docker contapal-postgres-1
```

Singura pregătire în plus față de pasul 1: `VACUUM (ANALYZE)` pe AMBELE părți
(registrele din `public` și cele trei forme), ca `Index Only Scan` să fie posibil
pentru toată lumea, nu doar pentru cub.

EXPLAIN-urile integrale ale rulării a treia: `pas2/explain/qNN-<formă>-<scară>.txt`.
Textele: `pas2/cub/qNN.sql` (cu `{T}`), `f0/qNN.sql`, plus `f0/q02b.sql`,
`f0/q04b.sql`, `f0/q12b.sql` scrise pentru pasul ăsta.

## ×1 — mediana ms (min–max)

| Q | F0 | F1 | F2 | F3 | F2+S | F3+S |
|---|---|---|---|---|---|---|
| Q01 balanță sintetică | 58,7 (57–67) | 56,5 (52–67) | **45,7** (46–46) | 53,2 (53–54) | 51,8 (51–54) | 52,9 (52–54) |
| Q02 balanță × partener | **105,9** (104–109) | 172,7 (170–175) | 141,4 (141–143) | 151,6 (149–167) | 157,1 (155–162) | 155,8 (154–181) |
| Q02b (F0, regula cubului) | 219,9 (213–228) | — | — | — | — | — |
| Q03 fișa 4111 | 206,4 (201–207) | **187,7** (185–192) | 200,9 (192–203) | 201,8 (194–229) | — | — |
| Q03c fișă + contrapartidă | — | 509,0 (501–518) | **472,0** (456–478) | 543,1 (516–558) | — | — |
| Q04 fișă × partener | 171,8 (168–185) | 8,8 (8,6–9,7) | **6,1** (5,8–6,7) | 6,8 (6,3–7,3) | — | — |
| Q04b (F0, regula cubului) | 42,0 (41–44) | — | — | — | — | — |
| Q05 sold × partener | **6,2** (5,9–6,3) | 82,7 (76–85) | 70,8 (66–79) | 67,6 (65–71) | 36,1 (33–37) | 35,9 (34–37) |
| Q06 partide cu rest | **76,3** (74–82) | 154,7 (148–191) | 154,7 (141–170) | 89,2 (87–92) | 85,5 (84–92) | 89,1 (87–92) |
| Q07 jurnal TVA | 9,7 (9,3–12,3) | 6,6 (5,7–8,4) | **5,8** (5,7–6,5) | 7,7 (7,2–8,1) | — | — |
| Q08 D394 | 7,9 (7,7–9,5) | **6,7** (6,5–7,9) | 6,9 (6,9–7,0) | 8,5 (8,1–10,6) | — | — |
| Q09 FIFO pe gestiune | **0,3** (0,3–0,4) | 33,4 (31–34) | 15,4 (15–16) | 17,2 (16–18) | — | — |
| Q09b FIFO produs × gestiune | — | 0,3 (0,3–0,4) | **0,2** (0,2–0,2) | 0,2 (0,2–0,3) | — | — |
| Q10 PhysicalStock | 170,2 (162–172) | 218,1 (213–244) | **138,5** (132–150) | 150,7 (146–158) | — | — |
| Q11 terți SAF-T | 84,5 (80–86) | 75,0 (72–76) | **66,0** (63–67) | 70,4 (70–75) | 68,2 (67–77) | 71,7 (67–76) |
| Q12 GLE (rândurile lunii) | **3,1** (2,9–3,8) | 59,2 (55–65) | 76,9 (76–78) | 80,2 (79–84) | — | — |
| Q12b TaxInformation | **5,3** (4,7–6,2) | 115,2 (111–118) | 20,2 (19–21) | 23,3 (23–24) | — | — |
| Q13 citirea pentru storno | **0,1** | 0,1 | 0,1 | 0,3 | — | — |

## ×10 — mediana ms (min–max)

| Q | F0 | F1 | F2 | F3 | F2+S | F3+S |
|---|---|---|---|---|---|---|
| Q01 | 617,8 (607–657) | 523,2 (516–562) | 467,6 (463–488) | 549,6 (536–560) | 112,9 (109–120) | **87,9** (87–95) |
| Q02 | 1.068 (959–1.554) | 1.358 (1.224–1.478) | 977 (956–1.036) | 1.132 (1.109–1.153) | 304 (295–311) | **272** (267–274) |
| Q02b | 1.696 (1.595–1.704) | — | — | — | — | — |
| Q03 | **1.017** (1.011–1.073) | 1.119 (1.110–1.162) | 1.714 (1.659–1.764) | 1.196 (1.190–1.221) | — | — |
| Q03c | — | **1.784** (1.755–1.829) | 2.228 (2.189–2.273) | 1.981 (1.968–2.022) | — | — |
| Q04 | 718,8 (715–754) | 72,7 (70–76) | 32,5 (31–34) | **31,3** (30–34) | — | — |
| Q04b | 262,3 (254–272) | — | — | — | — | — |
| Q05 | **6,2**° (6,0–6,7) | 412,9 (411–476) | 336,2 (323–354) | 357,5 (357–362) | 54,6 (52–58) | 53,5 (49–59) |
| Q06 | **71,8**° (71–79) | 733,2 (722–752) | 636,3 (585–676) | 624,7 (607–636) | 63,2 (62–64) | 62,5 (60–63) |
| Q07 | 23,2 (22–24) | **5,8** (5,6–6,1) | 6,4 (5,9–6,6) | 6,9 (6,5–7,8) | — | — |
| Q08 | 18,1 (18–20) | **7,5** (6,8–10,0) | 7,6 (6,8–8,4) | 7,7 (7,7–8,9) | — | — |
| Q09 | **1,0**† (1,0–1,2) | 285,3 (283–297) | 178,6 (174–182) | 184,0 (182–185) | — | — |
| Q09b | — | 1,7 (1,7–2,0) | **0,9** (0,8–1,1) | 1,0 (0,9–1,2) | — | — |
| Q10 | 697,6 (673–716) | 1.466 (1.408–1.566) | 625,5 (606–641) | **570,6** (559–585) | — | — |
| Q11 | 600,2 (594–635) | 968,0 (954–1.007) | 964,5 (941–1.064) | 1.181 (1.170–1.231) | 104,6 (101–106) | **102,0** (102–106) |
| Q12 | **3,1**‡ (3,0–4,4) | 278,8 (267–303) | 276,0 (268–292) | 266,1 (264–269) | — | — |
| Q12b | **16,7** (16–17) | 530,2 (522–552) | 139,3 (138–143) | 206,2 (196–211) | — | — |
| Q13 | **0,3** (0,3) | 0,5 (0,5–0,6) | 0,6 (0,6–0,7) | 1,1 (1,0–1,2) | — | — |

° **F0 nu scalează la Q05 și Q06 prin CONSTRUCȚIE.** Pasul 1 a replicat ×10
doar cele trei registre; `SolduriPerioadaContabil`, `SolduriPerioadaStoc`,
`PartideDeschise`, `Imperecheri` și `Documente` au rămas la ×1 (FZ-D6: „doar
perioadele de referință sunt materializate"). Q05 și Q06 citesc EXCLUSIV din
ele, deci cifra lor de la ×10 e cifra de la ×1. Comparația corectă pentru ele e
coloana **+S**, care e echivalentul de cub al aceluiași snapshot: 54,6 ms și
63,2 ms, adică același ordin de mărime ca F0, pe date de zece ori mai multe.

† Q09 pe cub nu e aceeași întrebare ca `f0/q09.sql`: spec-ul cere forma pe
gestiune (13.101 loturi), F0-ul real filtrează produs × gestiune × TipStoc (30
de loturi). Perechea comparabilă e **Q09b**: F0 1,0 ms ↔ F2 0,9 ms.

‡ Q12 pe F0 NU face join-urile: `SaftProiectii` citește rândurile plate și
asamblează în memorie, cu încă două interogări (antetele documentelor și codul
de tip). Interogarea pe cub le face în SQL (`cub."Tranzactie"` + `Documente`).
Cifra de 3,1 ms e o citire plată, nu raportul.

## `shared hit/read` (prima rulare caldă)

Tabelele integrale: `pas2/out/tabel-x1.md` și `tabel-x10.md`. Faptul de reținut:
la ×10 **nicio formă nu mai încape în `shared_buffers`** (160 MB), deci
interogările largi citesc de pe disc — Q01 face 139.000 de citiri pe F0,
154.000 pe F2, dar doar **19.700 pe F2+S**. Snapshot-ul nu câștigă din CPU, ci
din pagini neatinse.

## Mărimile

| Formă | ×1 tabelă | ×1 indexi | ×1 total | ×10 tabelă | ×10 indexi | ×10 total |
|---|---|---|---|---|---|---|
| F0 (3 registre + 3 snapshot-uri) | 187 MB | 293 MB | **480 MB** | 1.152 MB | 953 MB | **2.105 MB**° |
| F1 (plat, 18 indexi EF-style) | 202 MB | 199 MB | 401 MB | 2.006 MB | 1.752 MB | 3.758 MB |
| F2 (LIST pe Spațiu) | 202 MB | 240 MB | 442 MB | 2.008 MB | 2.132 MB | 4.140 MB |
| F3 (+ RANGE pe an) | 202 MB | 242 MB | 444 MB | 2.008 MB | 2.644 MB | 4.652 MB |
| `cub."Tranzactie"` + `CodTva` | 18 MB | 16 MB | 34 MB | 182 MB | 100 MB | 283 MB |
| `cub."Sold"` | 52 MB | 57 MB | 109 MB | 577 MB | 656 MB | 1.233 MB |

° la ×10, `SolduriPerioada*` și `PartideDeschise` au rămas nereplicate (140 MB
din cele 2.105).

Partițiile F2 la ×1 / ×10: Contabil 121 / 1.208 MB tabelă, Stoc 54 / 527,
Fiscal 27 / 273. Lățimea medie a rândului de cub: **182 B**, a rândului de
`RegistruContabil`: 189 B — dar un rând de registru devine DOUĂ postări.

**Cubul complet costă ~2× discul lui F0.** La ×10, F2 + `Tranzactie` + o
singură graniță de `Sold` ≈ 4.550 MB, față de ~2.250 MB pentru un F0 replicat
integral. Sursa: 11,56 M postări contra 6,72 M rânduri de registru (contabilul
și fiscalul se dublează), plus tabela de tranzacții.

## Snapshot-ul `cub."Sold"`

Σ pe TOATE coordonatele, per spațiu, la 31 decembrie.

| | ×1 (2 granițe) | ×10 (10 granițe) |
|---|---|---|
| construcție | **1,7 s** | **50 s** |
| rânduri | 483.005 | 5.365.168 |
| mărime | 109 MB | 1.233 MB |
| `Sold` la 31.12.2025 == Σ directă din f2 | **0 abateri** | **0 abateri** |

La 31.12.2025, pe Contabil, snapshot-ul cubului are **321.199 de rânduri** față
de cele 184.780 ale lui `SolduriPerioadaContabil`, iar pe Stoc **110.786** față
de 7.914. Cubul păstrează TOATE coordonatele, inclusiv `Unitate` (partida), deci
un singur `Sold` înlocuiește toate trei snapshot-urile de azi ȘI
`PartideDeschise` — de aceea Q06+S citește 1.981 de pagini în loc de 139.591.

**Avertisment declarat:** la ×10 numărul de rânduri al lui `Sold` e constant pe
granițe (≈543.000 la fiecare), pentru că replicarea a refolosit ACELEAȘI ID-uri
de coordonate (FZ-D6). Într-un istoric real de zece ani, coordonatele se
înmulțesc; cifra de 1.233 MB e o LIMITĂ INFERIOARĂ, nu o prognoză.

## Trei surprize

1. **Cei doi indexi cei mai largi nu sunt atinși niciodată la ×10.** C1
   (`Cont, Data` cu `INCLUDE`) și S1 (`Gestiune, Unitate, Data` cu `INCLUDE`)
   însumează **1.384 MB** pe f2+f3 la ×10 și nu apar în niciun `EXPLAIN` al
   interogărilor pentru care au fost proiectate. Detaliu și cifrele de după
   scoaterea lor: `pas2/indexi-f2.md`.
2. **`Sold` cumpără mai mult decât balanțele.** Q06 (partide) trece de la 636 ms
   la 63 ms și Q11 (terți SAF-T) de la 965 ms la 102 ms — rapoarte care azi NU
   folosesc niciun snapshot pe F0 (`SaftProiectii` scanează registrul brut).
   Câștigul e de 6–10× și crește cu istoricul.
3. **Partiționarea pe an (F3) nu se plătește la interogările lunii, dar salvează
   fișa.** Q12/Q12b, care citesc o lună, sunt egale sau mai lente pe F3 decât pe
   F2 (266 ↔ 276 ms, 206 ↔ 139 ms: indexul pe `Data` al lui F2 taie deja luna,
   iar F3 mai adaugă `Append` peste 12 sub-partiții). În schimb Q03 (fișa, care
   citește TOT istoricul contului pentru soldul cumulativ) e 1.196 ms pe F3 față
   de 1.714 ms pe F2, iar F3 costă 512 MB de index în plus la ×10.

## Starea în care rămân bazele

- `…Fizica.x1`: setul de 6 indexi per uz INTACT (starea în care s-au măsurat
  cifrele de ×1); `cub."Sold"` la 2024-12-31 și 2025-12-31; niciun FK.
- `…Fizica.x10`: setul FINAL de 4 (C1 și S1 scoase de iterația declarată);
  indexii de identitate ai lui f2/f3, lipsă din pasul 1, refăcuți
  (`pas2/fix-identitate-x10.sql`); `cub."Sold"` la 2016…2025; niciun FK.
- Schema `public` (F0) nu a fost modificată pe nicio bază. Nimic în Flax,
  nimic în `nou/`, niciun commit.

## Defectul de pregătire găsit și reparat

La ×10, f2 și f3 au fost măsurate PRIMA DATĂ fără indexii lor de identitate
(`DocumentId`, `LinieId`, `TranzactieId`). Cauza e în pasul 1: `06a` i-a șters
și `06c` i-a recreat din `pg_get_indexdef`, care pentru un index pe o tabelă
PARTIȚIONATĂ întoarce `CREATE INDEX … ON ONLY …` — se recreează doar învelișul
părintelui, fără indexii de partiție. Cele două scări nu erau măsurate pe
același set. Efectul, măsurat: Q13 (citirea unui document pentru storno) pe F2
la ×10 **196,6 ms** în loc de **0,6 ms** — `Parallel Seq Scan` peste toate cele
trei partiții. Reparat, formele de cub re-rulate integral, restul cifrelor
neschimbat peste pragul de zgomot. Ieșirea defectă e păstrată ca probă în
`pas2/out/raw-x10-fara-identitate/`. **Aceeași cauză afectează și cifrele de
mărime ale lui f2/f3 din `pas1/out/marimi-x10.txt`** (448 / 710 MB de indexi
raportați acolo nu includeau identitatea) — de semnalat coordonatorului.

## FZ-r2 — fișa contului de la `Sold` (măsurată 2026-09-19, după închiderea pasului 2)

Întrebarea: fișa 4111 pe 2025 era singura interogare unde F2 pierdea clar față
de F0 la ×10 (citește TOT istoricul contului pentru soldul cumulativ). Remediul
propus: soldul inițial = Σ(D−C) din `cub."Sold"` la granița anterioară
(2024-12-31, toate coordonatele contului) + fereastra DOAR pe postările din 2025.
Texte: `pas2/cub/q03s.sql`, `q03cs.sql` (cu contrapartidă); pentru F0 s-a scris
și ramura lui de referință (`f0/q03r.sql` = `ContabilProiectii.cs:801-864`,
F27-D3: `Data > sfârșitul referinței` + rândul sintetic din
`SolduriPerioadaContabil`). Bazele au O SINGURĂ referință F0 (12/2025), deci
q03r citește rândul sintetic din 12/2025: **costul e cel real al ramurii,
valoarea e deliberat greșită**; egalitatea nu se verifică pe q03r. Harness:
`pas2/run-fzr2/`, ieșiri brute `pas2/out/raw-fzr2/`, tabelul integral
`pas2/out/raw-fzr2/tabel.md`, planurile în `pas2/explain/fzr2-*.txt`. Aceeași
disciplină FZ-D8 (6 rulări, prima aruncată, mediana celor 5), toate formele
rulate una după alta pe aceeași mașină, în aceeași stare a bazelor (starea
declarată mai sus: ×1 cu setul de 6, ×10 cu setul final de 4).

### Mediane ms (min–max)

| Scară | Q | F0 | F0 cu referință | F2 | F2+S | F3 | F3+S |
|---|---|---|---|---|---|---|---|
| ×1 | Q03 | 210,9 (204–219) | 216,1 (205–238) | 203,6 (196–233) | 213,0 (207–218) | 201,5 (199–216) | 224,9 (222–229) |
| ×1 | Q03c | — | — | 474,5 (465–483) | 488,2 (484–506) | 556,8 (536–578) | 589,9 (585–593) |
| ×10 | Q03 | 1.080 (1.022–1.161) | **248,5** (232–252) | 1.726 (1.718–1.780) | **348,3** (341–356) | 1.227 (1.213–1.239) | **327,8** (326–333) |
| ×10 | Q03c | — | — | 2.255 (2.243–2.280) | **940,3** (932–945) | 2.013 (2.004–2.068) | 1.308 (1.289–1.322) |

Egalitate (probă directă, nu pe q03r): soldul final al fișei e IDENTIC pe F0,
F2 și F2+S — 4.951.749,02 la ×1, 10.798.266,29 la ×10; rândurile din 2025:
144.239 la ×1 pe ambele forme; la ×10 F0 are 144.226 și cubul **199.175**,
fiindcă replicarea pasului 1 a mutat cu un an în urmă și împerecherile datate
2026 (54.936 postări `Fel = 4` în 2025) — cubul citește cu 38 % mai multe
rânduri decât F0 pe aceeași fereastră, deci cifra lui e conservatoare.

### Ce spun cifrele

1. **Remediul funcționează și scoate fișa din categoria „se rupe la ×10”**:
   F2 1.726 → F2+S 348 ms (5×), F3 1.227 → 328; cu contrapartidă 2.255 → 940.
   Costul nu mai depinde de lungimea istoricului: la ×1 (istoric = un an,
   nimic înaintea graniței) +S e egal cu forma fără snapshot (213 ↔ 204).
2. **F0 are același remediu, deja scris în cod, și rămâne mai rapid**: ramura
   de referință a lui `FisaCont` face 248 ms la ×10 contra 348 pe F2+S. Diferența
   are două surse măsurate: (a) granul lui `Sold` — pe 4111 la 2024-12-31 sunt
   **126.263 rânduri** (toate coordonatele: partener × unitate × …), citite
   `Index Only Scan` în 23 ms, contra **1.591 rânduri** în
   `SolduriPerioadaContabil` (cheiat pe repartitor), 1,2 ms — cifră directă
   pentru FZ-r1; (b) cubul are cu 38 % mai multe rânduri pe fereastră
   (împerecherile replicate) și `ix_f2c_data` citește toate cele 697.532 de
   postări contabile din 2025 ca să păstreze 199.175 (107 ms din 348), pe când
   F0 intră pe cei doi indexi de cont.
3. **Indexul `(Cont, Data)` NU e remediul**, nici pe fereastra de un an:
   recreat temporar pe f2 (512 MB la ×10, 51 MB la ×1), planificatorul l-a
   ignorat și cu `ix_f2c_data` scos (`DROP INDEX` în tranzacție cu `ROLLBACK`:
   alege `Parallel Seq Scan`, 393 ms); forțat prin `enable_seqscan = off` face
   293 ms contra 348 — 16 % pentru 512 MB, pe contul care e 29 % din postări
   (rândurile lui sunt pe fiecare pagină a anului). Concluzia iterației
   declarate a pasului 2 (C1 scos) rămâne.
4. **Partiționarea pe an nu mai e necesară pentru fișă**: F3+S 328 ↔ F2+S 348,
   zgomot; motivul de redeschidere al lui FZ-r6 (fișa integrală) dispare.

### Incident de stare, reparat

Scriptul fazei cu index (`run-fzr2/idx.sh`) a presupus că `(Cont, Data)`
lipsește pe AMBELE baze; pe ×1 exista (setul de 6 e starea declarată a lui
×1), `CREATE` a picat și `DROP`-ul de la final l-a scos. Refăcut imediat cu
aceeași definiție + `ANALYZE` (`ix_f2c_cont_data` prezent, 7 indexi pe
`f2."Postare_Contabil"` la ×1); cifrele ×1-idx din `tabel.md` sunt deci
măsurate pe același set ca ×1 și nu aduc nimic. Starea finală a bazelor e
cea declarată în „Starea în care rămân bazele”.
