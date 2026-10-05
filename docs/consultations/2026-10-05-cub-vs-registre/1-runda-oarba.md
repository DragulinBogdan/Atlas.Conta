# Runda 1 (oarbă): forma registrului unui sistem de contabilitate și gestiune

Ești consultat ca arhitect independent. Nu ai acces la cod și nu ți se spune
ce a ales echipa. Ți se cere o judecată din principii, pe cazuri concrete.
În runda a doua vei primi codul și măsurătorile și îți vei verifica singur
afirmațiile.

## Domeniul

Aplicație de contabilitate și gestiune pentru România, două profiluri pe
același mecanism: privat (OMFP 1802) și instituții publice (bugetar). Circa
20 de tipuri de document, stabile: facturi de intrare și ieșire, recepții
(NIR), bonuri de consum, transferuri, asamblare, retururi, plăți și încasări,
deconturi, note contabile, închidere de TVA, imobilizări (punere în
funcțiune, amortizare, casare), declarație vamală, inventar.

Constrângeri comune oricărei opțiuni:

- documentul e tipat (o clasă per tip); ciclul e `Draft → Operat → Stornat`;
- ce s-a operat nu se rescrie; corecția e storno plus document nou;
- perioada închisă e graniță absolută;
- politica e date editabile fără release: conturile, regulile de stoc, TVA,
  numerotarea; structura e cod;
- stocul se evaluează FIFO pe lot;
- terții se urmăresc pe partide (documentul deschis și stingerile lui);
- TVA cu perioadă de declarare distinctă de data documentului, D300, D394;
- SAF-T D406: jurnal, mișcări de stoc, terți, taxe per linie;
- închidere lunară, solduri la dată, balanță, fișe de cont;
- o bază PostgreSQL per client, .NET cu EF Core;
- pentru runda aceasta presupune sute de mii de documente pe an la clientul
  mare; volumul măsurat vine în runda a doua.

## Cele două forme

**Forma R, registre specializate.** Patru tabele persistate:

- registrul contabil: un rând e o notă cu cont debitor, cont creditor și
  valoare, cu dimensiunile analitice ținute separat pe fiecare latură
  (partener, material, clasificații bugetare, proiect, centru de cost);
- registrul de stoc: un rând e o mișcare pe lot și gestiune, cu cantitate și
  valoare semnate și cu un tip de stoc;
- registrul de TVA: un rând per linie de document și tip de taxă, cu perioada
  de declarare, sensul, regimul, cota, baza și taxa;
- registrul de imobilizări: un rând per eveniment al fișei.

Lângă ele: solduri de perioadă materializate, restul de stins ținut pe
antetul documentului plus o tabelă de împerecheri document cu document.
Motorul construiește la operare un plan: mișcările de stoc din tabela de
reguli de stoc, notele din tabela de reguli de contare, pasul de TVA. Tipul
de document contribuie prin metode polimorfe suprascrise. Stocul și
contabilitatea lui sunt rânduri separate, în registre separate, scrise de
același plan. Stornoul e un rând invers marcat.

**Forma C, cub de postări.** O singură tabelă:

- o postare are coordonate (cont, latură, dată, partener, gestiune, produs,
  unitate nominalizată, cod și perioadă de TVA, valută, carte contabilă sau
  fiscală, șase dimensiuni analitice), trei măsuri aditive (cantitate,
  valoare în valută, valoare) și proveniența (document, linie);
- o tranzacție e un set balansat de postări, cu fel: operare, storno,
  transfer, deschidere;
- lotul, partida și fișa de imobilizare sunt același concept, „unitatea":
  soldul, restul, costul și valoarea rămasă sunt sume pe unitate;
- stocul și contabilitatea lui sunt aceeași postare;
- TVA-ul e postare în cartea fiscală.

Motorul e o funcție fără acces la bază: primește documentul împreună cu
politicile rezolvate și soldurile unităților atinse și întoarce postările,
deciziile luate și ipotezele citite, sau refuzuri. Fiecare tip de document
are un „declarant" care îi descrie fluxul. Soldurile se materializează la
granițele de perioadă.

## Opțiunile

- **A.** Numai forma C. Orice citire e o sumă pe cub sau pe soldurile lui.
- **B.** Forma C e autoritatea, forma R e proiecție materializată din ea.
  Documentul scrie numai cubul. Registrele se derivă, se pot reconstrui
  integral și sunt calea principală de citire a rapoartelor, deci și stratul
  de cache. Alegi tu dacă derivarea e sincronă, asincronă sau la cerere.
- **C.** Numai forma R. Motorul scrie registrele direct.
- **D.** Ambele forme scrise independent de același document, în aceeași
  tranzacție. E termen de referință, nu neapărat candidat.

## Criteriile

1. Coerența modelului: ce inconsecvențe devin imposibile și care rămân
   posibile.
2. Simplitatea implementării și a întreținerii.
3. Simplitatea cu care se reprezintă comportamentul unui tip de document:
   unde stă cunoașterea, cât e cod și cât e date, cât vede un om care
   citește un singur tip.
4. Portița: cazurile neacoperite de reguli, sau prea scumpe de acoperit,
   trebuie să se poată rezolva prin note contabile punctuale.
5. Loc pentru optimizări de viteză făcute mai târziu, fără schimbarea
   modelului.
6. Explicabilitatea unei cifre la audit.
7. Reversibilitatea: ce nu se mai poate reface ieftin după ce una dintre
   forme e ștearsă.

## Cazurile de probă

Exprimă fiecare caz în fiecare opțiune. Spune ce se scrie, unde, ce garantează
consecvența și ce se rupe.

1. **Factură de intrare cu marfă**, recepționată în gestiune, cu TVA
   deductibil: stoc, datorie către furnizor, taxă.
2. **Recepție ulterioară** pe o factură deja înregistrată, cu diferență de
   cantitate sau preț față de recepția inițială. Diferența se postează cu
   cauza ei păstrată.
3. **Asamblare**: consumă componente din loturi FIFO și produce produse al
   căror cost este costul consumului. Valoarea se conservă.
4. **Notă contabilă manuală** în trei variante: între două conturi fără
   analitic; pe un cont de terț urmărit pe partide; pe un cont de stoc.
   Spune ce preț are portița în fiecare opțiune și ce poate rămâne
   inconsecvent după ea.
5. **Storno** al unui document operat într-o lună închisă, care are
   dependenți: stingeri și consumuri din loturile lui.
6. **Citiri grele**: fișa integrală a unui cont de terț, balanța la dată,
   stocul la dată pe gestiune, produs și lot, jurnalul de TVA al lunii,
   SAF-T. Spune unde stau indexii, soldurile materializate și cache-ul și ce
   optimizări rămân deschise.
7. **Un caz neacoperit**: bunuri ale terților ținute în custodie, cu evidență
   cantitativă în afara bilanțului. Compară acoperirea prin regulă cu
   acoperirea prin notă punctuală.
8. **O extensie viitoare**: ciclul bugetar ALOP la instituții publice
   (angajare, lichidare, ordonanțare, plată). Creditele bugetare se consumă
   pe clasificația bugetară, fiecare fază consumă din precedenta, iar
   evidența stă pe conturi în afara bilanțului (8060, 8066, 8067). Modulul
   nu e scris și va fi construit pe paradigma aleasă aici. Spune ce structură
   nouă cere fiecare opțiune.

## Ce livrezi

1. Tabelul cazuri × opțiuni, cu schița pe scurt în fiecare celulă.
2. Clasamentul opțiunilor pe fiecare criteriu, cu motivul.
3. Cel mai puternic argument PENTRU fiecare opțiune, inclusiv pentru cele pe
   care le respingi.
4. Recomandarea ta și condițiile în care ține.
5. Lista faptelor care ți-ar schimba verdictul, ca întrebări verificabile în
   cod sau prin măsurătoare. Ele devin lista ta de verificat în runda a doua.
6. Ce lipsește sau e greșit pus în întrebare, inclusiv o opțiune nelistată.

Reguli: marchează fiecare afirmație ca `[principiu]` sau `[presupunere]`. Nu
fi diplomat. Dacă două opțiuni sunt echivalente pe un criteriu, spune-o. Cel
mult 2.500 de cuvinte, în română.
