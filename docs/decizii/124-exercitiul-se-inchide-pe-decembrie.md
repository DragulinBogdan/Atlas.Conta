# 124 — Rezultatul se închide lunar printr-un document generat, exercițiul închis e starea lui decembrie, iar balanța are graniță de an fără tranzacție de deschidere

- Data: 2026-10-16
- Stare: propusă
- Execuție: etapa E3, contractul `docs/nucleu/e3-inchiderea-exercitiului-contract.md`, pașii 0–5 (2026-10-16), PR #54–#57 și PR-ul pasului 5. Owner-ul a tranșat la 2026-10-16: 121 pe privat trece în anul nou (E3-D2), închiderea lunară prin document generat (E3-D3), lanțul strict (E3-D6), rândurile anuale datate 31.12 (E3-D7), execuția bugetară în etapă, cu seed-ul ei de pornire (E3-D9), și cheia închiderii = atomul complet al soldului
- Arii: perioade, politici, documente, motor, stoc, raportare, api, ecrane, verificare
- Amendează: —
- Închide: F27-r4, CV-r2
- Docs: contractul `docs/nucleu/e3-inchiderea-exercitiului-contract.md`; catalogul `docs/nucleu/scenarii/EX.md` (SC-EX-00…07, SC-EX-10, SC-EX-20…26), SC-CIT-114 din `docs/nucleu/scenarii/CITIRI.md`, SC-X-12 din `docs/nucleu/scenarii/X.md`

## Regula durabilă

**Conturile de rezultat se închid prin documente generate din politică, exercițiul închis e o stare a lanțului de perioade, iar anul nou începe din soldurile de la 31.12, fără nicio postare de deschidere.**

(a) **Închiderea lunară e un document.** `InchidereRezultat` e o notă contabilă generată, previzualizată și regenerată pe tiparul închiderii de TVA. Există cel mult un document viu pe lună. E datat în ultima zi a lunii și se generează în ordinea lunilor. Liniile vin din soldurile de la sfârșitul lunii, câte una pe fiecare cheie cu sold. Cheia e atomul complet al soldului: contul plus coordonatele laturii (repartitor, material, coduri funcțional și economic, sursă de finanțare, unitate, proiect, centru de cost). Operarea refuză un draft care nu mai închide exact soldurile de acum.

(b) **Politica alege conturile, nu comportamentul.** `PoliticaInchidereConturi` dă sursa (prinde descendenții), destinația, momentul (`Lunar` sau `Anual`), ordinea și coordonatele păstrate pe destinație. Restul coordonatelor se pliază, iar coordonatele fiscale nu trec niciodată pe destinație. Rândurile `Anual` se aplică numai în decembrie, după cele `Lunar`, datate 31.12. Un rând nou e numai seed: motorul nu cunoaște simboluri de cont și nici profilul.

(c) **Luna nu se închide cu rezultatul deschis.** `REZ-NEINCHIS` se emite cât timp vreo cheie a conturilor pe care le închide luna are sold la ultima zi a ei. În decembrie judecă și rândurile `Anual`. Severitatea vine din politica de închidere (seed: blocant pe ambele profiluri).

(d) **Exercițiul închis e starea lui decembrie.** `ExercitiuInchis` și momentul lui sunt ale motorului, ca starea lunii. Închiderea și redeschiderea scriu în istoricul lui decembrie. Exercițiul se închide numai cu decembrie închis, cu exercițiul precedent închis sau nedefinit, și fără sold pe conturile din politică la 31.12 (`EX-SOLD-REZIDUAL`). Se redeschide numai ultimul exercițiu închis, cu motiv scris. Cât timp e închis, decembrie lui nu se redeschide. O perioadă nouă nu se creează înaintea unei perioade închise.

(e) **Anul nou nu postează.** Soldul la 1 ianuarie e soldul la 31.12 pe aceeași cheie. Închiderea exercițiului nu scrie nicio tranzacție, iar `Deschidere` rămâne a migrării. Pe privat, 121 trece în anul nou; repartizarea profitului e notă manuală.

(f) **Balanța are graniță de an calendaristic.** `SoldInceputAn` e soldul net al cheii la 1 ianuarie, iar `Initial` sunt sumele precedente: soldul inițial al anului plus rulajele de la 1 ianuarie până înaintea perioadei. Soldurile inițial și final rămân neschimbate, iar granița nu depinde de închiderea exercițiului.

(g) **Corecția valorică judecă exercițiul închis, nu anul calendaristic.** Consumul beneficiar dintr-un an până la ultimul exercițiu închis inclusiv dă `CV_AN_INCHIS`. Nucleul primește anul ultimului exercițiu închis ca valoare și nu cunoaște perioadele.

(h) **Ușile.** Comenzile exercițiului stau pe `api/perioade/{an}/exercitiu`, cu decembrie ca subiect. Închiderea conturilor de rezultat are ușile ITV pe `api/irz`. În XAF, comenzile exercițiului sunt pe decembrie, iar generarea pe lista închiderilor. Rutele care arată cifre sau constatări calculate pe ușa de sistem cer citirea a tot ce însumează (80e).

## Context

Până la E3, 6 și 7 cumulau de la începutul bazei, 121 primea rezultatul doar prin notă manuală, iar coloana `Initial` a balanței cuprindea tot istoricul. Perioada fiscală avea lanțul, istoricul și snapshot-ul capătului de an (088), dar nu exercițiul închis (F27-r4). Corecția valorică judeca anul închis după calendar (CV-r2). Legacy-ul bugetar închidea anul prin „nota de deschidere an” (121 și 489 în 117, cu dimensiunile păstrate); E3 preia regula, nu procedura.

## Tranșări

- Cheia închiderii e atomul complet, deși 1C închide pe cont (owner). Pe importul din ianuarie 2025, IRZ reclasifică între cheile aceluiași cont, iar netul pe cont e divergența explicată a importului. Alternativa respinsă, cheia pe cont, ar fi lăsat reziduuri analitice pe 6 și 7.
- Exercițiul pe decembrie, nu entitate proprie: lanțul, blocajul și istoricul există deja pe perioadă.
- Rândurile anuale în IRZ-ul lui decembrie, datate 31.12: redeschiderea exercițiului nu are nimic de stornat.
- Balanța pe calendar, nu pe exercițiul închis: o bază fără închideri dă aceleași cifre ca una cu închideri.
- Comenzile exercițiului și IRZ se fac din XAF; React primește doar coloana „Exercițiu” (104d).

## Review

Review advers la pasul 5 (agent nou, numai citire): niciun MAJOR, 3 MEDIU, 6 MINOR, tratate în contract, secțiunea „Review advers”. MEDIU: perioada nouă putea intra în anul unui exercițiu închis, iar `CV_AN_INCHIS` și cheia cu material nu erau probate pe calea reală. Proba din browser de la pasul 4 a găsit, separat, grila liniilor IRZ în afara modului de culegere 85c.

## Ce rămâne deschis

E3-r1…E3-r6 în `restante.md`: repartizarea profitului ca document generat, perioada de ajustare, exercițiul diferit de anul calendaristic, ordinea închiderilor la anulare și storno, data de înregistrare a IRZ, proba IRZ pe un decembrie de import.

## Istoric

- 2026-10-16: scrisă la închiderea etapei E3; propusă owner-ului.
