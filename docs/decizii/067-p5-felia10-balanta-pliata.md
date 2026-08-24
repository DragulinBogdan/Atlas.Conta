# Decizia 67 — Pasul 5, felia 10 — balanța pliată pe planul de conturi

- **Data**: 2026-08-17 (primul commit în jurnal)
- **Stare**: activă
- **Rezumat durabil**: `CLAUDE.md` §67
- **Docs**: docs/api/p5-felia-balanta-plan-contract.md

---

**Pasul 5, felia 10 — balanța pliată pe planul de conturi — executată**
(contract + închidere: `docs/api/p5-felia-balanta-plan-contract.md`,
BP-D1…BP-D5; smoke browser pe clona de import). Închide singurul item lăsat
deschis cu nume de felia 9 (R-D5). Zero migrații, motorul neatins.
Tranșările:
(a) **Se cumulează BRUTELE în sus, se netează LA NOD** — niciodată invers.
De asta „balanța pe clase" nu se putea obține grupând balanța plată,
oricâte `GroupItem`: soldul unei clase nu e o sumă de rânduri afișate. Pe
date reale, clasa 4 iese `C 10.100.771,84`, în timp ce însumarea coloanelor
de sold ale grupelor ei ar afișa `D 4.623.420,74 / C 14.724.192,58` — două
cifre, niciuna adevărată la acel nivel. Corolar impus, nu afirmat: ecranul
**n-are `Summary`** (un total peste rândurile afișate ar aduna părinții cu
copiii, adică ar număra fiecare frunză de câte ori are strămoși); cifra de
control `Σ rădăcini == Σ balanța plată` se verifică în ModelCheck.
(b) **Frunzele sunt chiar `Balanta`**, nu o a doua agregare (regula care
ține un singur `AtomContabil`); pliul e în MEMORIE — planul e mărginit prin
construcție, iar recursivitatea în SQL ar cere CTE scris de mână pentru un
rezultat oricum ne-paginabil. Cele două margini — părinte invizibil, cont
fără etichetă — devin **rădăcini**, nu rânduri pierdute (lecția D4 din
felia 9), sub invariantul „fiecare frunză contribuie la exact o rădăcină".
(c) **Fără `DataSourceLoader`** (singura proiecție a pasului 5 fără el): un
arbore nu se paginează — `LIMIT/OFFSET` peste mulțimea pliată taie exact
strămoșii, iar un nod fără ei e rând orfan. Diferența e de natură, nu de
comoditate: dincolo rezultatul e o listă, aici un graf.
(d) **Modul analitic nu se pliază** (cheia `Cont × Repartitor` e o a doua
ierarhie: „clasa 4 pe furnizorul X" nu e un nod al planului); dimensiunile
rămân filtre pre-agregare. `nivelMaxim` taie RÂNDURI, nu sume, cu
`AreCopii` recalculat peste mulțimea păstrată și refuz 400 sub 1 (un raport
golit tăcut arată exact ca o bază goală).
(e) **Ciclul din `Cont.Parinte` e oprit prin gardă de vizitare** —
navigația e editabilă din UI, iar fără ea verificarea n-ar pica, ar atârna;
proba e chiar terminarea apelului. Ne-blocant rămas: ciclul e oprit, nu
RAPORTAT — un gardian de nomenclator ar fi fixul de fond, aditiv.
(f) Smoke-ul a scos un defect de client în afara mandatului lui:
`autoExpandAll` se aplică LA MONTARE, iar arborele se monta pe tabloul gol
(datele vin din `useQuery`) ⇒ bifa „Extinde tot" nu făcea nimic, tăcut;
randarea e gate-uită pe încărcare. Perf: 58–75 ms pe clona de import, sub
balanța plată (nu plătește `requireTotalCount`); niciun index adăugat.
