# 121 — Ariile stării curente sunt 16, pe responsabilitățile catalogului de funcționalități

- Data: 2026-10-15
- Stare: activă
- Execuție: varianta A din analiza din 2026-10-14, aleasă de owner 2026-10-15; paginile sparte numai prin mutare, `Arii` pe decizii din citări, registrul reclasificat
- Arii: verificare
- Amendează: 118 (l)
- Închide: 121-r1, 121-r2
- Docs: [analiza spargerii ariilor](../analize/2026-10-14-spargerea-ariilor.md); [scop](../scop.md); [catalogul funcționalităților](../functionalitati.md); `docs/docskit.json` (`arii`)

## Regula durabilă

**O arie e o responsabilitate pe care o poate atinge o felie fără s-o atingă pe vecina ei. Lista e cea din `docs/docskit.json`, una pentru stare, registru, decizii și catalog.**

(a) **Ariile.** `documente`, `motor`, `stoc`, `partide`, `imobilizari`, `perioade`, `politici`, `tva`, `saft`, `raportare`, `nomenclatoare`, `api`, `ecrane`, `dezvoltare`, `verificare`, `import`. Fiecare are o pagină `docs/stare-curenta/<arie>.md`. `limite-curente`, `README` și `termeni` rămân transversale (118 l).

(b) **Granița dintre arii.** O regulă de domeniu stă în aria responsabilității ei, nu în cea a ecranului sau a ușii care o arată: regimul pe stare e în `documente`, afișarea lui în `ecrane`; amortizarea e în `imobilizari`; explicația deciziei e în `raportare`. Coloana `Arie` din catalog urmează familia funcționalității.

(c) **`Arii:` pe o decizie = paginile care o citează.** O decizie numește în `Arii:` ariile ale căror pagini îi poartă identificatorul (`(42b)`), plus paginile transversale pe care le numea. O decizie necitată rămâne cu `Arii` gol până o citează o pagină.

(d) **O arie nouă sau o graniță mutată cere decizie** (kit §3.9.3), cu pagina, configul, registrul și catalogul aduse în același PR.

(e) **Limitele au trei locuri.** Ce nu e livrat e un rând `deschisă` în catalog. Ce nu face încă o funcționalitate livrată, în forma văzută de utilizator, stă în `limite-curente`, pe arii. Limita unui mecanism, fără efect direct pentru utilizator, stă la sfârșitul paginii ariei, sub `## Limitele implementării`. Cifrele de mediu, rulările și istoricul nu intră în niciuna: stau în jurnal (kit §3.3.3, §3.3.7).

## Context

Pilotul stratului de scop (kit §3.9) a arătat că perimetrul unei felii (kit §3.5.6) nu semnalează nimic cu 4 arii: contractul TR-D9b-CV le declara pe toate. Analiza din 2026-10-14 a comparat trei variante pe același test. Cu 16 arii, TR-D9b-CV declară 6 (`stoc`, `motor`, `raportare`, `ecrane`, `verificare`, `saft`), iar o atingere în `partide` sau `tva` devine semnal. Abstracția servește funcționalitățile F-CVL-01…06 din etapa curentă și oricare felie ulterioară cu contract (kit §3.9.6).

## Tranșări

Owner-ul a ales varianta A (16 arii) la 2026-10-15. Întrebările 2–4 din analiză au luat propunerile ei, scrise în (b); `limite-curente` rămâne transversală.

Respinse: varianta B (9 arii; TR-D9b-CV ar fi declarat 6 din 9) și varianta C (numai domeniul spart; `politici-si-fiscalitate` rămânea amestecul TVA, SAF-T și configurare).

**Execuția.**
- **Paginile.** Cele 4 pagini vechi au fost sparte numai prin mutare. Secțiunile au trecut întregi, iar sub-secțiunile cu altă arie au urcat un nivel, cu textul heading-ului neschimbat, ca ancorele să rămână valabile. Proba e că liniile nevide ale paginilor vechi și noi sunt egale ca mulțime, în afara titlurilor și a blocurilor generate. Preambulul paginii de dezvoltare a devenit `## Validarea feliilor` în `verificare`.
- **Linkurile.** Cele spre paginile vechi au fost rescrise după ancoră, în tot Markdown-ul din repo. Cele fără ancoră au primit întâi pagina implicită a ariei vechi; 30 au fost apoi îndreptate de mână spre pagina conținutului citat. Cinci, în liniile `Docs:` ale deciziilor 091, 104, 112, 113 și 115, au rămas pe pagina implicită, fiindcă textul citat nu se mai găsește sau ținta nu e sigură. Gardianul `linkuri` probează doar că pagina există, nu că e cea potrivită.
- **`Arii` pe decizii.** 109 decizii au primit `Arii` din citări; 11 necitate au rămas goale.
- **Registrul**, 383 de rânduri:
  - 140 după prefixul familiei (D4-r → `tva`, 73/74-r → `saft`, FZ-r → `dezvoltare`, F26-r → `imobilizari`, F27-r → `perioade` etc.);
  - 74 după decizia-sursă cu o singură arie compatibilă;
  - 45 după decizie și titlu;
  - 85 după cuvintele din titlu;
  - 38 de mână, după titlu;
  - 1 corectat după check.
  Rândurile clasificate de mână sunt primele pentru sondajul owner-ului.

## Ce rămâne deschis

**121-r1 — Rescrierea paginilor pe responsabilități** (închisă prin 121, 2026-10-15). Mutarea a păstrat secțiunile numite după felii („Declarația fluxului per tip (TR-D6b, felia 30)”, „FCL și DSC pe cub (T-D4, felia 32, pasul 2)”) și preambulul de stare al feliilor din `verificare` (`## Validarea feliilor`), contra kit §3.3.1 și §3.3.3. Se rescriu pe subiect, câte o arie o dată, fără mutare de conținut între arii. Triggerul: prima felie care atinge aria.

**121-r2 — Limitele și acoperirea din README față de catalog** (închisă prin 121, 2026-10-15). `limite-curente.md` repetă funcționalități deschise din catalog (salariile, e-Factura, valuta…), iar `stare-curenta/README.md` ține acoperirea funcțională ca proză. Kit §3.3.7 le vrea în catalog. Triggerul: următoarea trecere `/docs-curatenie`.

**121-r3 — Sondajul clasificării registrului.** Owner-ul verifică prin sondaj aria rândurilor, întâi pe cele 38 clasificate de mână și pe cele 85 clasificate după titlu. O arie greșită se corectează pe rând, fără decizie nouă. Triggerul: prima sesiune pe o arie care citește registrul filtrat.

**121-r4 — Mutarea între arii.** Rescrierea 121-r1 a lăsat pe loc pasajele care țin de altă arie (regula ei: fără mutare între arii). Candidații, pagină → arie: din `stoc`, invarianții cubului → `verificare`, citirile contabile ale cubului → `raportare`, urmărirea partidelor pe 461/408 și proba DEC → `partide`, declarantul FCL → `motor`; din `api`, recepția cu diferențe → `stoc`/`politici`, ușile perioadelor → `perioade`; din `tva`, exportul SAF-T → `saft`, raportul „Impact TVA” → `raportare`; din `partide`, `SoldParteneri` → `raportare`, `IVerificabilLaCommit` → `motor`, PIF/AMO/CAS → `imobilizari`; din `documente`, `DescarcareService` → `stoc`; din `ecrane`, ruta rectificativei → `tva`, lista `PostareVizual` → `raportare` (de cântărit); din `raportare`, invarianții explicației → `verificare`, dry-run-ul și `DECLARATIE_INVALIDA` → `motor`; din `dezvoltare`, probele HTTP și purja scenelor → `verificare`; din `verificare`, listarea N-r8 → `raportare`, identitățile cititorilor → `documente`, validarea D406 → `saft`; din `motor`, filtrul de carte al cititorilor → `raportare`, corecția ITV legată → `tva`; din `perioade`, cumulul raportului de stoc → `stoc` (opțional). Se mută câte o arie o dată, cu ancorele rescrise. Triggerul: prima felie care atinge una din cele două arii.

## Istoric

- 2026-10-15: scrisă și executată pe branch-ul docs/arii-16, peste PR #44.
- 2026-10-15: 121-r1 și 121-r2 executate pe branch-ul docs/121-r1-r2. Cele 16 pagini sunt rescrise pe subiect; jurnalul scos din ele (preambulul `## Validarea feliilor`, cifrele de mediu, starea feliilor) a intrat verbatim în `istoric-plan-de-lucru.md`. Regulile care descriau încă registrele, regimul dual sau oracolul ieșite la 110 sunt aduse la forma din cod. `limite-curente` ține numai limitele văzute de utilizator; limitele mecanismelor au trecut la `## Limitele implementării` din pagina ariei, cifrele în jurnal, iar ce nu e livrat a devenit 13 rânduri noi în catalog (12 parcate, F-NOM-01 livrată la linia zero). Două limite contrazise de regula curentă au ieșit: snapshot-ul citit fără filtrul pe rând (citirile securizate folosesc postările autorizate) și soldul pe partener al taxei vamale (86-r13 închisă la 110). Din README au ieșit „Tranziția în curs” și tabelul de acoperire. Intră (e) și 121-r4.
- 2026-10-15: 121-r4, aria `stoc` mutată pe branch-ul tr-d9b-cv-pas4 (TR-D9b-CV pasul 4): invarianții cubului unificați în secțiunea `INV-CUB` din `verificare`, citirile contabile în `raportare`, urmărirea pe 461/408 și proba DEC în `partide`, declarantul FCL în `motor`; ancora din `termeni` rescrisă. Restul candidaților rămâne deschis.
