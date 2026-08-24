# Decizia 68 — Pasul 5, felia 11 — jurnalele de TVA

- **Data**: 2026-08-19 (primul commit în jurnal)
- **Stare**: activă
- **Rezumat durabil**: `CLAUDE.md` §68
- **Docs**: docs/api/p5-felia-jurnale-tva-design.md

---

**Pasul 5, felia 11 — jurnalele de TVA — executată** (design + închidere:
`docs/api/p5-felia-jurnale-tva-design.md`, JT-D1…JT-D9; review advers cu 2
defecte de fond, ambele fixate și văzute picând). Prima felie care adaugă un
REGISTRU. Tranșările:
(a) **Sursa e un registru NOU, `RegistruTva`, nu o proiecție peste
documente** — nota feliei 9 („peste altă sursă") a fost bătută de invariantul
III și de decizia 36f, care ceruse deja „proiecții peste REGISTRE". Motivul
de fond: rândul contabil 4426/4427 n-are baza și n-are tipul de TVA, iar
liniile `Scutit`/`Neimpozabil`/`Capitalizat` **nu postează deloc**, deși apar
legal în jurnal și în D300 — o proiecție peste rândurile de TVA le-ar fi
pierdut tăcut. Un rând per LINIE (SAF-T cere granularitatea); agregarea la
document trăiește în proiecție.
(b) **Criteriul de generare cere AMBELE jumătăți**: `PoliticaTva` pe tip ȘI
`TipTva` pe linie. Politica ține bugetarul inert (deși liniile lui CHIAR au
TipTva — CAP21 e implicitul acolo) și scoate din joc NIR-ul conex și
închiderea lunară; tipul liniei distinge linia fiscală.
(c) **Fără rânduri de deschidere**, spre deosebire de celelalte două registre
(25e/34d): soldul lui 4426 e o poziție contabilă, nu o operațiune taxabilă.
`DocumentId`/`DetaliuId` NENULE ⇒ cusătura e exactă.
(d) **Reconcilierea e PER DOCUMENT**, nu pe perioadă: agregatul de perioadă ar
fi inclus închiderea lunară și ar fi cerut excluderi scrise de mână. Taxarea
inversă se însumează pe RÂND, nu pe latură (o singură operațiune taxabilă,
postată printr-un rând care atinge ambele conturi). **Snapshot pentru ce a
intrat în calcul, join pentru ce doar se afișează** — `Regim`/`Cota` pe rând,
denumirile și codurile SAF-T la citire.
(e) **Backfill = unealtă proprie** (`nou/tools/BackfillTva`), nu flag pe cele
existente; derivă din documentele operate prin ACEEAȘI funcție a motorului.
Proba: **90.732 de rânduri pe 61.347 de documente în 4m13s, 0 divergențe pe
33,2 M lei**; idempotent (a doua rulare, 0 rânduri, 2 secunde).
(f) **Decontul se grupează pe `(Sens × TipTva)`, nu pe regim×cotă** —
corectură făcută înaintea codului: `SDD` și `SFD` au același regim și aceeași
cotă 0, dar coduri SAF-T diferite și rânduri diferite în D300. `TipTva` E
identitatea de raportare.
(g) **Review advers — D1**: linia de COST a lui `ReturClient` primea `TipTva`
de la culegere (implicitul tipului, pus pe orice linie nouă), iar
`PregatesteOperare` îi zeroza doar valoarea. Registrul îi scria rând ⇒ costul
intra în jurnal ca bază impozabilă (retur de 1.000 cu cost 600 → baza 1.600 cu
TVA 210, adică 13,1% în loc de 21%). **Cusătura JT-D6 nu putea să-l vadă**:
contribuția la TVA e exact 0, deci proba se închidea pe un jurnal greșit —
tiparul feliei 9, proba pe altă axă decât greșeala.
(h) **D3**: cheia jurnalului n-avea `Storno`, deci pe „jurnalul pe 2025" —
perioada care cuprinde și operarea, și stornarea — cele două se netau într-un
rând de 0,00 datat în luna operării: factura apărea de zero lei, stornarea
dispărea ca eveniment. Spre deosebire de partener/regim/cotă, `Storno` NU e
determinat de perechea (Document × TipTva).
(i) **D2 — forma contractului**: JT-D6 iterează documentele PREZENTE în
registru, deci complementul era invizibil PRIN CONSTRUCȚIE. Măsurătoare nouă,
pe tip: 23,15 M pe `InchidereTva` (corect prin design), 1,85 M pe
`NotaContabila` (punți de import).
(j) Rămase: smoke-ul VIZUAL în browser (extensie deconectată; probat prin API
pe date reale); ramura de storno a backfill-ului și regimurile
Scutit/Neimpozabil/Capitalizat nu apar în baza de import, deci sunt acoperite
doar de scenă; 2.404 linii fără `TipTva` = gaură a datelor sursei, măsurată.

```
/legacy   → surse Delphi (.pas, .dfm) + scripturi SQL vechi
/db       → se poate export schemă (CREATE) + CONȚINUTUL tabelelor de configurare
        (definiții câmpuri, reguli contabile, definiții stoc, plan Clasă/Tip) din (local)/contabilitate_2026 sql server trusted connection
/nou      → soluția nouă (XAF + React)
```
