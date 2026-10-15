# 122 — Declarațiile D300 și D394 se serializează din proiecția lor, în lei, cu refuz înaintea fișierului; corecția valorică intră în SAF-T fără cantitate inventată

- Data: 2026-10-15
- Stare: propusă
- Execuție: etapa E2, contractul `docs/api/e2-depunere-xml-contract.md`, pașii 0–5 (2026-10-15). Owner-ul a hotărât antetul pe societate (E2-D4), categoria de bunuri pe produs (E2-D6), plajele ca nomenclator (E2-D7) și forma SAF-T a corecției (E2-D10: factura după regula comună, linia cu cantitate 0 și 000/000000, mișcarea 90/60). Restul D-urilor se confirmă la review-ul PR-ului
- Arii: tva, saft, nomenclatoare, api, ecrane
- Închide: 69-r6, D4-r13, D4-r5, CV-r1
- Docs: contractul `docs/api/e2-depunere-xml-contract.md`; catalogul `docs/nucleu/scenarii/DECL.md` și SC-CVL-32 din `docs/nucleu/scenarii/CVL.md`; cercetarea formularelor `docs/api/d300-structura-2026.md`, `docs/api/d394-structura-2026.md`

## Regula durabilă

**Fișierul unei declarații e proiecția ei pe formular, rotunjită după lege; ce proiecția nu poate pune corect în formular oprește fișierul, nu ajunge în el.**

(a) **Sursa.** XML-ul D300 și D394 serializează proiecția existentă, pe același snapshot, plus antetul societății și datele de perioadă ale cererii. Nu recitește cubul. Aceeași perioadă, cu aceleași date și același antet, dă același fișier, byte cu byte.

(b) **Rotunjirea.** Sumele sunt lei întregi: sub 50 de bani se neglijează, de la 50 de bani se întregește. Se rotunjește cea mai fină unitate emisă (rândul de operațiuni D300, rândul `op1` D394), iar totalurile, oglinzile și rezumatele se calculează din valorile rotunjite, cu aceleași formule ca proiecția în bani. Defalcarea pe bunuri a unui rând își păstrează suma prin restul pus pe bunul cel mai mare.

(c) **Refuzul.** Perioada în afara formularului suportat sau a periodicității societății, antetul cu câmpuri obligatorii lipsă sau peste lungimea schemei, sumele fără rând în formular, avertismentele care ar fi erori la validator, facturile emise în afara plajelor și datele invizibile utilizatorului dau 422 cu cod `DECLARATIE_*`, înaintea primului byte. Nu se completează valori de rezervă. Proiecția JSON poartă aceleași refuzuri.

(d) **Versiunea formularului e cod.** Registrul formularelor (versiune, spațiu de nume, data de la care se aplică) și harta rând → element sunt cod, fiindcă structura e a legii. O versiune nouă e un rând nou în registru.

(e) **Datele de identificare sunt ale clientului.** Antetul stabil stă pe societate; plajele de facturi sunt un nomenclator; categoria de bunuri cu taxare inversă e pe produs. Datele de perioadă (temei, rambursare, bife, persoane afiliate) sunt parametri ai cererii. Nimic din acestea nu se deduce din conturi, tipuri de material sau coduri NC.

(f) **Factura emisă** pentru I.2 e documentul cu fapte de livrare în perioadă, fără storno. Numărul ei se leagă de plajă prin seria normalizată și cifrele finale.

(g) **Corecția valorică în SAF-T.** Factura care o poartă urmează regula comună a tipului (380, 381 pe storno sau pe total negativ). Linia de corecție are cantitatea 0, prețul 0 și informația fiscală nefiscală, câte o linie pe fiecare cont de contrapartidă. Pe declarația de stocuri, linia numai valorică a corecției primește codul politicii de mișcare pe cauza `CorectieValorica` (90 pe plus, 60 pe minus, consumul exclus). Orice altă linie numai valorică rămâne refuz. Cantitatea fictivă e exclusă.

(h) **Oracolul extern.** Fiecare formă de fișier se probează pe XSD-ul oficial și pe DUKIntegrator cu validatorul oficial, pe kit pin-uit prin SHA-256, fără erori și fără atenționări.

## Context

T2 cere ca decontul, D394 și SAF-T să treacă validarea ANAF pe lunile de probă. Până la E2, D300 și D394 existau numai ca proiecții JSON (69, 71), iar SAF-T refuza luna cu corecție valorică (CV-D9). Cercetarea formularelor era făcută din surse primare; lipseau fișierele, datele de antet și reprezentarea corecției.

## Tranșări

- Antetul pe societate, nu parametri la fiecare export: e identitatea clientului, culeasă o dată în XAF (owner).
- Plaja ca nomenclator cu istoric, nu câmpuri pe politica de numerotare: o serie poate avea mai multe decizii de alocare (owner).
- Factura cu corecția rămâne pe tipul ei comun; validatorul primește și 384, dar exportul nu schimbă tipul după conținutul liniilor (owner, după cercetarea 3a).
- 90/60 în loc de 100/101: „diferențele de preț” sunt ale prețului standard, pe care modelul FIFO nu-l folosește (owner).
- Lista CAEN nu se verifică înaintea fișierului; o verifică validatorul ANAF (E2-r6).

## Review

Review advers la pasul 5 (agent nou, numai citire): 2 MAJOR, 4 MEDIU, 4 MINOR, toate tratate în contract, secțiunea „Review advers”. MAJOR: stornoul comercial era numărat ca factură emisă în I.2; datele invizibile utilizatorului (documente, plaje, societate) se pierdeau tăcut sau dădeau 422 în loc de 403.

## Ce rămâne deschis

E2-r1…E2-r6 în `restante.md`: amprenta și confirmarea pe trimestru, decontul trimestrial cumulat pe două luni, secțiunile D394 fără sursă, facturile anulate și autofacturile, persoana fizică fără CNP și achizițiile N, lista CAEN. 69-r1 (formularele anterioare) și SAFT-r2 (celelalte linii numai valorice) rămân.

## Istoric

- 2026-10-15: scrisă la închiderea etapei E2.
