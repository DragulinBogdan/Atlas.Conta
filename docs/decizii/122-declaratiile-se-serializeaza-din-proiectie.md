# 122 — Declarațiile D300 și D394 se serializează din proiecția lor, în lei, cu refuz înaintea fișierului; corecția valorică intră în SAF-T fără cantitate inventată

- Data: 2026-10-15
- Stare: propusă
- Execuție: etapa E2, contractul `docs/api/e2-depunere-xml-contract.md`, pașii 0–5 (2026-10-15). Owner-ul a hotărât antetul pe societate (E2-D4), categoria de bunuri pe produs (E2-D6), plajele ca nomenclator (E2-D7) și forma SAF-T a corecției (E2-D10: factura după regula comună, linia cu cantitate 0 și 000/000000, mișcarea 90/60). La discuția deciziei (2026-10-15), owner-ul a precizat (b), (c), (f) și (h) și a adăugat (i), implementat în aceeași zi pe branch-ul etapei. Restul D-urilor se confirmă la review-ul PR-ului
- Arii: tva, saft, nomenclatoare, api, ecrane
- Închide: 69-r6, D4-r13, D4-r5, CV-r1
- Docs: contractul `docs/api/e2-depunere-xml-contract.md`; catalogul `docs/nucleu/scenarii/DECL.md` și SC-CVL-32 din `docs/nucleu/scenarii/CVL.md`; cercetarea formularelor `docs/api/d300-structura-2026.md`, `docs/api/d394-structura-2026.md`

## Regula durabilă

**Fișierul unei declarații e proiecția ei pe formular, rotunjită după lege; ce proiecția nu poate pune corect în formular oprește fișierul, nu ajunge în el.**

(a) **Sursa.** XML-ul D300 și D394 serializează proiecția existentă, pe același snapshot, plus antetul societății și datele de perioadă ale cererii. Nu recitește cubul. Aceeași perioadă, cu aceleași date și același antet, dă același fișier, byte cu byte.

(b) **Rotunjirea.** Sumele sunt lei întregi: sub 50 de bani se neglijează, de la 50 de bani se întregește. Se rotunjește cea mai fină unitate emisă (rândul de operațiuni D300, rândul `op1` D394), iar totalurile, oglinzile și rezumatele se calculează din valorile rotunjite, cu aceleași formule ca proiecția în bani. Defalcarea pe bunuri a unui rând își împarte rândul rotunjit prin regula de repartizare a nucleului (Hamilton la scara leului), separat pe semne: suma lor e rândul și niciun bun nu își schimbă semnul. Diferența dintre rotunjirea pe partener din D394 și rotunjirea o singură dată a totalului, ca în D300, se arată pe tip × cotă, ca informare, nu ca refuz.

(c) **Refuzul.** Perioada în afara formularului suportat sau a periodicității societății, antetul cu câmpuri obligatorii lipsă sau peste lungimea schemei, sumele fără rând în formular, avertismentele care ar fi erori la validator, facturile emise în afara plajelor și facturile emise ascunse utilizatorului pe obiect dau 422 cu cod `DECLARATIE_*`, înaintea primului byte. Lipsa citirii pe un tip citit de formular (`Postare` și `Societate`; pe D394 și `Document` și `PlajaFacturi`) dă 403, după ordinea din 80. Nu se completează valori de rezervă. Proiecția JSON poartă aceleași refuzuri.

(d) **Versiunea formularului e cod.** Registrul formularelor (versiune, spațiu de nume, data de la care se aplică) și harta rând → element sunt cod, fiindcă structura e a legii. O versiune nouă e un rând nou în registru.

(e) **Datele de identificare sunt ale clientului.** Antetul stabil stă pe societate; plajele de facturi sunt un nomenclator; categoria de bunuri cu taxare inversă e pe produs. Datele de perioadă (temei, rambursare, bife, persoane afiliate) sunt parametri ai cererii. Nimic din acestea nu se deduce din conturi, tipuri de material sau coduri NC.

(f) **Factura emisă** pentru I.2 e documentul cu număr propriu și fapte de livrare în perioadă. Numărul ei se leagă de plajă prin seria normalizată și cifrele finale.

(g) **Corecția valorică în SAF-T.** Factura care o poartă urmează regula comună a tipului (380, 381 pe storno sau pe total negativ). Linia de corecție are cantitatea 0, prețul 0 și informația fiscală nefiscală, câte o linie pe fiecare cont de contrapartidă. Pe declarația de stocuri, linia numai valorică a corecției primește codul politicii de mișcare pe cauza `CorectieValorica` (90 pe plus, 60 pe minus, consumul exclus). Orice altă linie numai valorică rămâne refuz. Cantitatea fictivă e exclusă.

(h) **Oracolul extern.** Fiecare formă de fișier din catalogul de scenarii se probează pe XSD-ul oficial și pe DUKIntegrator cu validatorul oficial, pe kit pin-uit prin SHA-256, fără erori și fără atenționări. În produs, atenționările formularului nu opresc fișierul; se arată lângă buton.

(i) **Fișierul confirmat.** Exportul XML își dă amprenta (SHA-256) și parametrii din care se reface: datele de perioadă și antetul societății de la export. Confirmarea care le primește reface fișierul din faptele cubului și din acești parametri și păstrează amprenta și parametrii numai când coincid; altfel refuză. Fără fișier, când declarația s-a depus din alt program, confirmarea rămâne pe amprenta faptelor. O versiune de fapte are cel mult o confirmare activă: reconfirmarea ei cu alt fișier sau fără fișier o înlocuiește, iar cea veche rămâne în istoric, retrasă. O confirmare se retrage cu motiv, fără ștergere; pe D300, retragerea e refuzată cât luna nu mai are altă confirmare activă și are fapte atribuite ca regularizare fiindcă era depusă.

## Context

T2 cere ca decontul, D394 și SAF-T să treacă validarea ANAF pe lunile de probă. Până la E2, D300 și D394 existau numai ca proiecții JSON (69, 71), iar SAF-T refuza luna cu corecție valorică (CV-D9). Cercetarea formularelor era făcută din surse primare; lipseau fișierele, datele de antet și reprezentarea corecției.

## Tranșări

- Antetul pe societate, nu parametri la fiecare export: e identitatea clientului, culeasă o dată în XAF (owner).
- Plaja ca nomenclator cu istoric, nu câmpuri pe politica de numerotare: o serie poate avea mai multe decizii de alocare (owner).
- Factura cu corecția rămâne pe tipul ei comun; validatorul primește și 384, dar exportul nu schimbă tipul după conținutul liniilor (owner, după cercetarea 3a).
- 90/60 în loc de 100/101: „diferențele de preț” sunt ale prețului standard, pe care modelul FIFO nu-l folosește (owner).
- Lista CAEN nu se verifică înaintea fișierului; o verifică validatorul ANAF (E2-r6).
- Factura emisă se definește prin numărul propriu, nu prin excluderea stornoului. Stornoul e factură cu număr din plajă în practică; modelul nu i-l dă încă (E2-r4), iar regula nu se schimbă când îl primește (owner, la discuția deciziei).
- Bunurile unui rând își împart rândul rotunjit prin Hamilton, regula de repartizare a nucleului, în locul restului pus pe bunul cel mai mare. Regula veche putea întoarce semnul: cinci bunuri de 0,60 pe un rând de 3 lei ieșeau −1, 1, 1, 1, 1 (SC-DECL-12) (owner).
- Diferența de rotunjire dintre D394 și D300 se arată, nu se ascunde și nu blochează: validatorul nu compară cele două declarații, iar diferențele se raportează (principiul 21) (owner).
- Confirmarea reface fișierul în loc să creadă amprenta trimisă de client: amprenta fără refacere ar certifica orice șir (owner a cerut amprenta și parametrii în E2, nu ca restanță).
- Antetul intră în parametrii fișierului, nu se recitește la confirmare: datele societății se pot schimba după depunere, iar fișierul depus trebuie să rămână reproductibil. Faptele rămân verificate prin amprentă. Jurnalul exporturilor, cu fișierul arhivat pe server, a fost respins: tabel nou și scriere la fiecare descărcare (owner).
- Confirmarea greșită se corectează prin retragere cu motiv sau prin înlocuire, nu prin ștergere: rândul e evidență, ca orice registru. Garda pe D300 urmează regula corecției directe fără dependenți (14, 33d): regularizările s-au decis pe confirmare, deci se corectează întâi ele (owner, după semnalarea că regula „o confirmare, fără revenire” bloca tratarea erorilor).

## Review

Review advers la pasul 5 (agent nou, numai citire): 2 MAJOR, 4 MEDIU, 4 MINOR, toate tratate în contract, secțiunea „Review advers”. MAJOR: stornoul comercial era numărat ca factură emisă în I.2; datele invizibile utilizatorului (documente, plaje, societate) se pierdeau tăcut sau dădeau 422 în loc de 403.

## Ce rămâne deschis

E2-r1…E2-r6 în `restante.md`: amprenta și confirmarea pe trimestru, decontul trimestrial cumulat pe două luni, secțiunile D394 fără sursă, facturile anulate și autofacturile, persoana fizică fără CNP și achizițiile N, lista CAEN. 69-r1 (formularele anterioare) și SAFT-r2 (celelalte linii numai valorice) rămân.

## Istoric

- 2026-10-15: scrisă la închiderea etapei E2.
- 2026-10-15: discuția cu owner-ul: (b) Hamilton pe bunuri și diferențele D394–D300 afișate, (c) 403 pe tip și 422 pe obiect, (f) pe numărul propriu, (h) delimitată la probă, (i) adăugat; SC-DECL-12…14.
- 2026-10-15: (i) precizat: antetul în parametrii fișierului, înlocuirea confirmării aceleiași versiuni și retragerea cu motiv (gardă pe regularizările D300); SC-DECL-15, SC-CIT-84.
