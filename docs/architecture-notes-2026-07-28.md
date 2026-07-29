# Sesiune de arhitectură 28.07.2026 — fluxul comenzilor online (rezervare, picking, PO furnizori)

**Status: direcție FIXATĂ în sesiune de explorare.** Nu e design-doc — design-doc-ul
comenzilor (`docs/comenzi/`) urmează și, unde contrazice notele astea, câștigă el.
Ridică amânările din P2 §10 („comenzile", „rezervarea de stoc", „regenerarea la
recepția NIR") și le dă formă; `PoliticaEvaluare` (decizia 51) rămâne parcată.

## Context

Magazin online cu stocul sursă în gestiuni identificate „Online" din Atlas.Conta.
Comanda site-ului se transmite către Atlas; stocul disponibil se rezervă; pozițiile
lipsă se comandă (manual, deocamdată) la furnizori, cu trasabilitate; la acoperirea
comenzii se facturează prin picking — operatorul scanează produsul/unitatea pe care
o are la îndemână. Cerință inițială de CMP (căzută pe parcurs — vezi §9).

## Deciziile sesiunii

1. **Rezervarea = document real care mută stoc într-o gestiune tampon (hard),
   NU alocare soft.** Motivul decisiv: gardianul de sold devine enforcement
   gratuit — soldul rezervat chiar pleacă din gestiunile online, niciun alt flux
   nu-l poate atinge. Cad din asta: feed-ul „prin construcție" (§10), anularea
   prin mecanica existentă, DSC mereu dintr-o singură gestiune (limitarea P2
   37d devine permanentă, nu provizorie). **Respins**: rezervarea soft (entitate
   + „sold − rezervări") — ca să fie enforcing, gardianul de sold ar trebui să
   învețe despre rezervări = mecanism transversal nou în hot-path; fără asta e
   advisory și se descoperă ruptă la picking. Tamponul e sublocație logică ce
   OGLINDEȘTE realitatea (coletul în pregătire e segregarea fizică); la
   inventariere: raftul ↔ gestiunea online, coletele deschise ↔ tamponul.

2. **La intake: transferul de rezervare se generează ȘI se operează automat,
   cu loturi FIFO** — singura regulă cognoscibilă la momentul comenzii.
   Alocarea FIFO e contabilitate internă, nu pretenție fizică; semantica
   rezervării e „produs × cantitate". **Respins pe drum**: rezervare „la nivel
   de produs" ca rând de registru — registrul nu are mișcare fără lot (cheia
   Lot × Repartitor × TipStoc e peste tot: gardian, prefix-sums, harness 1C).

3. **Picking = substituția lotului pe transferul operat.** Scanul poartă
   identitate reală de lot (confirmat: drift FIFO↔fizic „destul de multicel");
   mecanismul NU e primitivă nouă: **anulare directă → schimbă lotul pe linie
   (același produs, aceeași cantitate) → re-operare**, orchestrat ca o acțiune
   per sesiune de picking. Fereastra de substituție E fereastra de anulare
   directă existentă (25d): cât timp DSC-ul nu s-a operat, loturile din tampon
   nu au dependenți. Gardianul de sold la re-operare validează gratuit lotul
   scanat (lot inexistent în gestiunea online = refuz zgomotos = sesizarea
   corectă de LDI). **Ales explicit: adevăr PE DOCUMENT** (linia transferului
   poartă lotul real); perechea de corecție la data curentă (tampon→online
   lot vechi, online→tampon lot nou) = fallback DOAR peste granițe de perioadă
   închisă. Marfa fără etichetă de lot: FIFO la scan — acolo nu e drift, e
   convenția unică posibilă.

4. **Sub FIFO substituția e valoric CORECTĂ, nu doar cantitativ neutră**:
   transferul nu contează (23c), valoarea trăiește în registrul de stoc, iar
   DSC-ul va costa la prețul lotului efectiv livrat. CMP nu e condiție a
   fluxului (vezi §9).

5. **Închiderea: comanda acoperită → FCL generată din comandă (prețurile
   site-ului) → DSC din tampon cu loturile PIN-uite ale propriei rezervări**
   (lanțul linie comandă → linii transfer → loturi; pin-ul P2 §4, first-class).
   NU FIFO pe tampon — tamponul e partajat între comenzi și e fidel pe lot per
   comandă. Pe calea cu comandă, FCL adoptă/operează draftul DSC construit din
   rezervare, nu generează unul nou — inversare LOCALĂ față de P2 (unde DSC e
   copilul FCL prin `GenereazaSecundar`); fluxul manual P2 rămâne neatins în
   paralel. **Respins pe drum** (alternativa „fără tampon": picking-ul
   construiește direct draftul DSC cu loturi reale — elegantă: drift mort la
   rădăcină, 1× volum de registru, zero tipuri noi de stoc): pierdea rezervarea
   hard; sinteza tampon + substituție ia ambele.

6. **Comenzile = derivate `Document` (ComandaClient, ComandaFurnizor) care nu
   postează NIMIC.** Argument decisiv: `LinieSursaId` e FK generic spre
   `DocumentDetaliu` — tot lanțul de acoperire vorbește `DocumentDetaliu`;
   agregat separat ar dubla mașinăria (numerotare, stare, link-uri, gardieni,
   UI). Tipuri fără reguli sunt legale în motor (precedent DSC-la-bugetar).
   Concret: **CDC** — numărul site-ului cules (ca numărul furnizorului pe FCT),
   fără politică de numerotare; poartă cheia externă stabilă de linie și
   versiunea comenzii. **CDF** — emis intern, politică de numerotare proprie.
   **FCT confirmă PO-ul PER LINIE**: `LinieSursaId` aditiv pe detaliul FCT →
   linia CDF (header-ul ar minți la factura care consolidează mai multe PO-uri);
   rest per linie CDF = comandat − confirmat. **CDC↔CDF rămâne SOFT** (PO-ul se
   emite manual ghidat de proiecția de backorder; alocarea la sosire prin
   proiecție) — link hard ar muri la primul PO care agregă resturile mai multor
   comenzi client.

7. **Modificarea comenzii: snapshot complet de la site → diff server-side →
   document de CORECȚIE autogenerat** (copil al comenzii-rădăcină, linii
   semnate; gramatica append-only; forma de scriere e chiar „agregatul per
   document" din 42d — noutatea: ținta operată produce corecție, nu editare).
   Gardianul reconcilierii: reducerea permisă doar până la cantitatea deja
   acoperită (picuită/facturată); sub prag = refuz zgomotos (întâi de-picking
   prin mecanismul §3 sau RDC), site-ul retransmite. Anularea completă = caz
   degenerat (storno cascadă dacă nimic facturat; altfel corecție-minus pe
   rest). „Închide cu rest" (renunțarea la backorder) = corecție-minus — fără
   stare nouă, fără câmp mutabil; starea terminală a comenzii e derivabilă.
   **Respinse**: storno + re-emitere la fiecare update (cascadă de re-legare pe
   transferurile operate); câmp operațional mutabil post-operare (prima
   excepție de la imuabilitate devine canonică prin vechime). **Cerințe spre
   site** (acceptate ca posibile): ID stabil de linie (cazul „același produs pe
   două linii cu prețuri diferite"), versiune monotonă per comandă (webhook-uri
   dezordonate → snapshot stale refuzat). Idempotența intake-ului: legături
   `Site:Comanda` — precedentul Import1C.

8. **Sosirea mărfii: alocarea automată = reconciliere idempotentă permanentă,
   NIR-ul e doar declanșatorul frecvent.** `RezervareService`, un nucleu: per
   produs × gestiune online, resturile nerezervate ale comenzilor în așteptare,
   FIFO pe data comenzii → generează + operează transferuri de rezervare, unul
   per comandă. Două intrări, același cod: la intake (scoped pe comandă) și la
   sosire (scoped pe produsele intrate). Rulează ca SECVENȚĂ, nu cuib (42b),
   tranzacție per comandă, recuperare gratuită prin proiecția de resturi
   (idempotent → re-rularea e mereu sigură). Acoperă toate ușile de intrare
   (NIR, LDI+, RDC, BTR, storno-ul rezervării unei comenzi anulate) fără N
   trigger-e. Serializarea măturării + intake-ul concurent = găleata advisory
   lock 25f (obligatoriu la pasul 5). **Adendum (notiță)**: posibil ecran de
   alocare NIR→comandă (vizibilitate „cine a primit marfa" + realocare
   manuală) — UI peste aceleași primitive de serviciu (realocare = anulare
   rezervare A + alocare B), zero mecanică nouă; amânat.

9. **CMP: argumentul-forță a murit în sesiune.** Motivația inițială („operatorul
   ia ce are la îndemână, deci lotul e ficțiune") cade odată ce scanul poartă
   lotul real — costul per linie e exact, decizia 13 în forma ei cea mai pură.
   `PoliticaEvaluare` (51d/e) rămâne parcată ca opțiune de evaluare (marjă
   netedă per comandă), nu necesitate; fluxul merge integral pe FIFO +
   substituție.

10. **Feed-ul de disponibil = sold per gestiunile flag-uite „Online"** —
    proiecție pe registre, corectă prin construcție (rezervatul e în tampon,
    care nu e flag-uit). Fără logică „sold − rezervări".

11. **Premisă transversală: totul e ADITIV — 51a ține.** Derivate noi (CDC,
    CDF, transferul de rezervare cu `LinieSursaId` pe detaliu), coloane aditive
    (`LinieSursaId` pe detaliile FCT și FCL), servicii, seed (calități/flags
    Online + Tampon pe Gestiune). Zero schimbări de motor în afara
    orchestrărilor de serviciu.

## Secvențierea (inserție în roadmap-ul deciziei 44)

1. **GATE XAF întâi, neschimbat** (FCT+FCL; polish-ul e refolosit de ecranele
   back-office ale comenzilor; CDF se culege manual în XAF din prima zi).
2. **Design-doc comenzi** (`docs/comenzi/`), apoi feliile de model, validate
   headless în ModelCheck pe ambele profiluri (la bugetar tipurile stau inerte),
   cu review advers: **C1a** — CDC + rezervare + picking-substituție +
   închidere (FCL/DSC); **C1b** — CDF + link FCT + măturarea de la sosire +
   proiecțiile (resturi, backorder, feed) în `Module/Proiectii` (42c).
3. **Pasul 5 re-scopat la prima felie**: fluxul online = primul consumator real
   al designurilor 42/43 (neatinse structural) — intake + feed + **ecranul de
   picking ca primul vertical React** (exact tipul de UI pe care regula 44.2 îl
   interzice în Blazor). Aici se plătește advisory lock-ul 25f.

Bifurcație lăsată deschisă: C1a poate trece înaintea gate-ului la presiune de
client; ordinea de mai sus e recomandarea.

## Fire parcate (pentru design-doc)

- Derivata concretă a transferului de rezervare: tip NOU (validări proprii —
  primitor = tamponul, `LinieSursaId` pe detaliu, semantica substituției), nu
  BTR reutilizat — de fixat la design.
- Corecția de comandă: același tip CDC cu `DocumentSursa` setat vs tip separat.
- Ecranul de alocare NIR→comandă (adendumul §8).
- CMP / `PoliticaEvaluare` — parcat, condițiile din 51 neschimbate.
- Marfa sosită înainte de factură (aviz pe lanțul furnizor) — neatinsă în
  sesiune; CDF→NIR fără FCT rămâne de tranșat la design.
- Prioritatea alocării la sosire: FIFO pe data comenzii, deliberat fără
  discreție umană în v1; excepții (comenzi plătite/expres) = extensie de
  politică, dacă apar.
