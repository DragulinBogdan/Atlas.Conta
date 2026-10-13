# 111(f) — contractele conceptuale ale operațiilor procedurale

Tip: analiza
Data: 2026-10-11
Alimentează: 111 (f), viitoarea decizie și contractul feliei

Propunere de contracte de operații, cerută de owner după acceptarea
[modelului conceptual](2026-10-11-111f-registre-tipate-reluare.md#continuarea-discuției--acorduri-de-proiectare)
și a [scenariului numeric](2026-10-11-111f-scenariu-numeric.md).
Nu este contractul unei felii de implementare, schemă fizică sau dovadă
de funcționare a codului. O01–O12 sunt identificatori locali ai operațiilor,
nu nume de API, tipuri de document, coduri de refuz sau decizii aprobate.

## Ce este fixat și ce propune documentul

Sunt agreate identitățile stabile, poziția comună propunere–legal anual,
asocierea valorică bugetar–legal, limitele distincte ale planificării,
acoperirea anuală a multianualului, trasabilitatea pe alocări, verificarea
temporală și lipsa rescrierii efectelor operate. Soldurile 110/30/80 din S9
și reprezentarea lor ca proiecții reconstruibile au fost acceptate ulterior.

Documentul propune organizarea comenzilor și a efectelor pentru aceste
reguli. Nu toate operațiile trebuie expuse ca butoane separate. O comandă
poate reuni operații dependente într-o tranzacție; reuniunea nu autorizează
schimbarea pe ascuns a altor documente. Q1–Q4 sunt tranșate de owner
pentru fluxurile și limitele delimitate aici; Q3–Q4 au fost aprobate la
2026-10-12. Contractul fizic și implementarea rămân ulterioare.
Prioritatea rămâne privat-first și TR-D9b; „prima felie” din Q3–Q4
desemnează prima felie procedurală, nu următorul pas al produsului.
Relatarea de domeniu a owner-ului este sursa acestei analize;
nu este făcută aici validarea legislativă a formularelor sau a fluxului.

## Identități, fapte și măsuri

| Element conceptual | Ce identifică | Ce nu trebuie confundat cu el |
|---|---|---|
| Poziție propunere–legal anual | Aceeași poziție din propunere până la angajarea anuală | Starea sau suma aprobării la un moment dat |
| Poziție legal multianual | Angajarea care continuă peste ani | Acoperirea ei dintr-un an |
| Poziție planificată, deschisă sau bugetară | Plafonul corespunzător și proveniența lui | O copie anuală a legalului multianual |
| Asociere bugetar–legal | Acoperirea valorică anuală a unei poziții legale | O simplă compatibilitate de triplet |
| Alocare de obligație | Partea unei poziții de factură/document constatator atribuită legalului | Antetul facturii sau partida contabilă |
| Alocare de ordonanțare | Partea autorizată dintr-o alocare de obligație, cu acoperirea executabilă | O nouă obligație |
| Atribuire a plății | Suma plății repartizată ordonanțării și bugetarului executat | O a doua plată independentă de cub |

Modificarea sumei este fapt asupra identității; nu naște altă identitate
prin înlănțuirea versiunilor. Modificarea sau inversarea referă faptul-sursă
când acesta este obiectul explicit al acțiunii. Nu definim o tabelă generică
de evenimente configurabile: familiile de fapte rămân tipizate, conform 111(f).

Pentru fiecare poziție-sursă, disponibilul se calculează pe TOȚI
consumatorii ei, la data efectului, în istoricul cunoscut la validare:

| Limită | Consum care o limitează |
|---|---|
| Planificat anual | Totalul deschiderilor atribuite poziției |
| Deschis | Totalul bugetarelor atribuite poziției |
| Propunere anuală aprobată | Totalul părților bugetare atribuite aceleiași poziții propuse |
| Bugetar | Totalul rezervărilor anuale pe asocierile sale |
| Credite de angajament | Angajările multianuale atribuite, nu acoperirile lor repetate anual |
| Legal aprobat | Obligațiile constatate cumulate, în cazul obișnuit fără avansuri |
| Alocare de obligație | Plătitul cumulat plus resturile autorizate încă executabile asupra aceleiași alocări |
| Alocare de ordonanțare | Plățile efective atribuite ei |

Rezervarea anuală limitează acoperirea obligațiilor/execuției din anul ei.
La reportare, factura veche consumă drept de plată din acoperirea nouă,
fără a fi încă o dată facturată. Nu adunăm factura și plata ei drept doi
consumatori independenți ai aceleiași acoperiri. Q2 fixează valabilitatea
anuală și emiterea ordonanțării noi; dreptul de execuție nu se deduce din
suma istorică a rezervărilor.

## Reguli comune ale unei comenzi

**Intrări.** Document și poziții-sursă, identitățile referite, suma sau
repartizarea, data efectului, faptul vizat la corecție. Momentul înregistrării
este atribuit de sistem. Implicitul din culegere este o alegere concretă
care se reverifică la operare, nu o relație recalculată ulterior.

**Validare.** Dreptul de a comanda și de a folosi referințele, stările și
perioada, compatibilitatea identităților/clasificării, scara sumelor,
conservarea alocărilor, limitele și dependențele. Datele interne folosite
pentru limite sunt complete, chiar când proiecția utilizatorului este
parțial vizibilă. Mesajul de refuz nu poate dezvălui documente ascunse.
Ordinea precisă a refuzurilor și codurile stabile vor fi fixate în contractul
feliei, fără introducerea unor coduri fictive în această analiză.

**Timp.** Se simulează întregul lot candidat peste faptele acceptate și se
verifică toate momentele de schimbare din intervalele afectate, inclusiv
cele ulterioare datei candidate. În aceeași comandă se judecă rezultatul
atomic; două comenzi distincte trebuie fiecare să lase o stare validă.
Perioada închisă este graniță absolută. Soldul final pozitiv nu compensează
un deficit intermediar. Rezultatul admis persistă valorile hotărâte, nu
instrucțiuni care vor recalcula documentele vechi la următoarea modificare.

Valabilitatea anuală se judecă după data efectului, folosind intervalul
persistat la operare, nu după data curentă a sistemului. Închiderea perioadei
blochează scrierile cu efect în acea perioadă. Înaintea închiderii, o plată
înregistrată târziu pentru anul vechi reverifică și acoperirile și
ordonanțările deja operate în anul nou; nu le ajustează automat (Q2/S9).

**Scriere.** Validarea limitelor și toate efectele obligatorii se execută
în aceeași tranzacție, sub serializarea comună a scrierii. Sunt atomice
faptele procedurale, atribuirile și efectele din cub cerute de document.
O proiecție întârziată nu decide admiterea. Dacă un sumar participă la
validare, trebuie demonstrată echivalența și actualizarea sa atomică.

**Rezultat.** Succesul identifică documentul, efectele și atribuirile
acceptate; refuzul nu lasă efecte de domeniu parțiale. Retrimiterea aceleiași
comenzi nu dublează efectele; mecanismul de identificare/reluare este de
ales la integrarea în ușa de operare. O nouă operare după retragere trebuie
deosebită de repetarea accidentală a celei vechi.

## O01 — aprobă sau modifică limita de planificare

- **Primește:** poziția de planificare, anul/intervalul, clasificarea,
  suma inițială sau variația; fel explicit: planificat anual ori credite
  de angajament. Documentul aprobator este sursa faptului.
- **Verifică:** regulile comune; reducerea nu lasă descoperite deschiderile,
  respectiv angajările multianuale deja aprobate, pe intervalul afectat.
- **Scrie atomic:** aprobarea sau modificarea tipizată, cu proveniență și
  cele două date. Nu creează automat deschideri, bugetare sau legale.
- **Așteptare:** PL27 = 140, deschis = 120; reducerea planificatului la
  119 este refuzată. Credite 160, LM = 150; reducerea creditelor la 149
  este refuzată, indiferent cât s-a plătit deja pe LM.

## O02 — înregistrează și aprobă propunerea anuală

- **Primește:** documentul și poziția propusă, anul, clasificarea și suma.
  Poziția primește identitatea care va fi păstrată la angajarea legală.
- **Verifică:** regulile de culegere și aprobare; după existența unor
  bugetări, reducerea propunerii se verifică împotriva tuturor părților
  bugetare atribuite ei. Un draft nu oferă plafon executabil.
- **Scrie atomic:** faptul propunerii aprobate și identitatea, fără fapt
  de angajare legală și fără execuție. Aprobarea bugetară și angajarea
  legală au documentele/faptele lor, chiar dacă poziția este aceeași.
  În traseul licitației, această operație corespunde lansării; desemnarea
  câștigătorului produce ulterior bugetarul și legalul, conform Q1/S10.
- **Așteptare nouă de proiectare:** propunere 100, bugetări atribuite 80;
  reducerea propunerii la 79 este refuzată. Simpla existență a propunerii
  de 100 nu permite o factură de 1 înaintea angajării legale.

## O03 — aprobă deschiderea și bugetarea anuală

- **Primește:** pentru deschidere, poziția planificată și suma; pentru
  bugetare, poziția deschisă, suma și repartizarea spre pozițiile propuse
  sau spre soldurile multianuale acoperite. Acestea sunt operații distincte,
  descrise împreună fiindcă folosesc aceeași regulă de consum.
- **Verifică:** deschiderile cumulate în planificat, bugetarele cumulate
  în deschis și părțile atribuite propunerii în plafonul ei. Pentru o
  poziție bugetară distribuită între mai multe propuneri, se verifică
  fiecare parte, nu se impută întregul bugetar fiecărei propuneri.
- **Scrie atomic:** aprobarea poziției și atribuirea cu sumă la sursa
  plafonului. Rezervarea pentru legal este O05; dacă aceeași comandă le
  reunește, fiecare efect se scrie o singură dată. Nu produce factură.
- **Așteptare:** PL27 = 140, DB27 = 120, B27 = 100. Un nou bugetar 21
  este refuzat chiar dacă planificatul mai depășește bugetarele cu 40.
  În S5, suplimentarea B28 la 90 cere întâi DB28 = 90.

## O04 — angajează legal

- **Primește:** poziția anuală existentă din propunere sau identitatea
  multianuală, documentul angajării, suma, intervalul și sursele limitelor.
- **Verifică:** pentru anual, plafonul propunerii și acoperirea aprobată;
  pentru multianual, creditele de angajament disponibile și intervalul.
  Multianualul poate avea întregul legal aprobat fără întreaga acoperire
  anuală simultană; factura/plata își verifică acoperirea aplicabilă.
- **Scrie atomic:** faptul angajării legale pe identitatea stabilă și,
  pentru multianual, atribuirea consumului de credite de angajament.
  Dacă rezervarea se face în aceeași comandă, O05 se validează în lot.
  Rezervarea deja acceptată nu se mai scade încă o dată la angajare.
- **Așteptare:** credite 160, angajare LM = 150; rămân 10. În S9,
  acoperirea ulterioară de 30 din B28 nu reexecută această operație.

Pentru traseul anual clarificat prin Q1, desemnarea câștigătorului produce
bugetarul, legalul și rezervarea aferentă. La semnare, o valoare mai mică
produce corecția legalului și rectificarea bugetarului, cu alinierea rezervării.
Sunt fapte distincte ale etapelor, chiar dacă operațiile dependente sunt
reunite într-o comandă atomică. Valoarea inițială a propunerii nu se rescrie.

## O05 — rezervă acoperirea anuală pe asocierea bugetar–legal

- **Primește:** poziția bugetară, identitatea legalului comună cu propunerea, suma,
  anul și data efectului; scop explicit: acoperire curentă ori reacoperire
  a soldului existent. Tripletul coincide conform regulii domeniului,
  dar coincidența singură nu autorizează asocierea.
- **Verifică:** disponibilul bugetar agregat, compatibilitatea și starea
  pozițiilor, plafoanele propunerii/legalului aplicabile scopului; nu
  admite acoperiri simultan executabile care dublează același rest.
  Reacoperirea se verifică pe soldul relevant, nu pe suma istorică a
  rezervărilor din toți anii.
- **Scrie atomic:** faptul rezervării pe asociere, cu sumă, proveniență și
  intervalul de valabilitate în anul bugetarului, conform Q2.
  Asocierea este o identitate urmărită de modificări și consumatori.
  În traseul anual Q1, simpla lansare a propunerii nu produce rezervarea;
  aceasta însoțește bugetarul și legalul la desemnarea câștigătorului.
- **Așteptare:** B27 = 100, rezervări LM = 70 și LA = 30; încă 1 pe LM
  este refuzat. S9 admite B28 = 30 pentru restul de 30 al aceluiași LM,
  fără angajare legală nouă și fără consum nou de credite de angajament.

## O06 — constată obligația și atribuie pozițiile

- **Primește:** factura ori documentul constatator, pozițiile și sumele,
  alocările pe legale și acoperirile anuale de origine. Globalul salarial
  folosește același contract de atribuire, cu documentul său real.
- **Verifică:** totalul alocărilor egal cu suma de repartizat a poziției,
  legalul aprobat, rezervarea relevantă, limita legală cumulată și
  compatibilitatea; apoi toate regulile contabile ale documentului.
- **Scrie atomic:** efectele contabile obligatorii și faptele de atribuire
  care nu se pot deduce din cub. Suma obligației și alocările ei nu devin
  două totaluri autoritative care se întrețin independent; Q3 fixează
  efectul canonic citit și legătura cu alocarea.
- **Așteptare:** F1/1 = 70 se atribuie A1 = 50 și A2 = 20, păstrând
  identitatea poziției comerciale. A1 = 50 și A2 = 21 sunt refuzate.
  După facturare, legal LM = 150 și facturat LM = 140 permit cel mult
  încă 10 pe limita legală, cu verificarea separată a acoperirii anuale.

O factură veche reportată nu trece din nou prin O06. Plata din anul nou
referă alocarea ei originală și acoperirea nouă, conform O07–O08.

## O07 — ordonanțează o parte din obligație

- **Primește:** documentul de ordonanțare, alocarea exactă a obligației,
  suma și acoperirea anuală care va fi executată. În același an aceasta
  poate fi implicită; la reportare este explicit cea a anului nou.
- **Verifică:** restul de ordonanțat pe alocare, toate ordonanțările
  existente asupra ei, legalul comun și acoperirea executabilă a sumei.
  Nu poate ordonanța din nou o parte deja plătită. Pentru restul neplătit
  al unei ordonanțări vechi se emite una nouă în anul următor, conform
  precizării owner-ului din Q2. Admiterea ei verifică expirarea dreptului
  vechi la data noului efect, din intervalul anual persistat.
- **Scrie atomic:** faptul autorizării și atribuirile spre obligație și
  acoperire; la înlocuirea anuală, păstrează și proveniența din alocarea
  vechii ordonanțări. Persistă valabilitatea autorizării, limitată la anul
  acoperirii. Nu constată din nou obligația și nu creează plată în cub.
- **Așteptare:** A2 = 20, deja ordonanțat 15; încă 6 este refuzat, deși
  alte alocări ale facturii au disponibil. În S9, factura 70 are 40
  ordonanțat și plătit în 2027; ordonanțarea nouă 30 pe B28 este admisă.
  Varianta S9 cu ordonanțare veche 70 și plată 40 admite noua ordonanțare
  30 numai după încheierea dreptului de execuție pentru restul vechi.
  Plătit 40 + rest autorizat executabil 30 = obligație 70; totalul istoric
  al autorizărilor 70 + 30 nu reprezintă o obligație de 100.
  Dacă B28 acoperă doar 20 din restul de 30, ordonanțarea de 20 este
  admisă, iar cea de 21 este refuzată; obligația neacoperită rămâne 10.

## O08 — operează plata și execuția ei

- **Primește:** documentul plății, defalcarea contabilă spre partide și
  defalcarea de execuție spre alocările ordonanțărilor/acoperirile anuale.
- **Verifică:** restul fiecărei ordonanțări, disponibilul executabil al
  acoperirii, limitele cumulate ale obligației și legalului, regulile
  contabile și reconcilierea ambelor defalcări inclusiv pe obligația-sursă.
  Data efectului plății trebuie să fie în intervalul autorizării și
  acoperirii; înaintea închiderii, înregistrarea târzie verifică și
  dependenții din anul nou, conform regulilor temporale comune.
  Două linii de câte 6 nu trec separat dacă împreună consumă un rest de 10.
- **Scrie atomic:** postările plății și atribuirile rezolvate ale execuției,
  cu proveniența până la alocarea obligației. Plătitul se citește din
  efectul relevant al cubului; nu se întreține ca sold autoritativ pe legal.
  Alegerea reprezentării din Q3 trebuie să evite dublarea debit/credit și
  numărarea transferurilor de partidă drept plăți noi.
- **Așteptare:** în S3, P2 = 25 stinge F1 = 10 și F2 = 15; varianta
  F1 = 15 și F2 = 10 este refuzată pentru aceeași defalcare de execuție.
  În S9, plata 30 reduce restul facturii la zero și duce B28 la execuție
  30; execuția B27 rămâne 40. Dacă se cere 31, întregul lot este refuzat.
  O plată cu efect în 2028 pe O27/B27 este refuzată prin valabilitate,
  chiar dacă 2027 este încă deschis. Încă 5 cu efect în decembrie 2027,
  înregistrați după O28 = 30, sunt refuzați: 45 + 30 > obligația 70.

Concurența pe S4: două comenzi de câte 8 pe un rest de 10 lasă după
prima 2; a doua este refuzată. Nu se persistă efectul contabil al uneia
fără atribuirea procedurală obligatorie sau invers.

## O09 — modifică valoarea unui fapt procedural

- **Primește:** identitatea poziției/asocierii, documentul modificator,
  variația semnată, data efectului și referința necesară la faptul vizat.
- **Verifică:** limitele amonte și dependenții aval, în toată zona temporală
  afectată. Schimbarea clasificării sau reatribuirea nu se ascunde într-o
  modificare valorică; ar necesita contract separat.
- **Scrie atomic:** faptul modificator cu efectul fixat. Documentele și
  postările dependente rămân neschimbate. Refuzul păstrează situația inițială.
- **Așteptare:** în S7, reducerea retroactivă B cu 30 este refuzată la
  apariția rezervării 80 în intervalul cu plafon 70; reducerea cu 10 este
  admisă, dacă perioada este deschisă. Factura rămâne 80, suplimentarea
  ulterioară rămâne +50, iar disponibilul final pentru rezervări noi este 60.

La semnarea contractului anual din S10, legalul și bugetarul de 80 devin
75 prin corecții de −5, iar rezervarea scade la 75. Faptele corective
referă documentul semnării; propunerea inițială rămâne 100. Orchestrarea
poate reuni rectificările dependente într-un lot atomic, fără a le pierde
identitatea. Dacă există consumatori incompatibili cu noua valoare,
operarea rectificărilor este refuzată; aceștia nu se recalculează automat.

O intrare „plafon nou 120” cere semantica datei și a aprobărilor ulterioare
din Q4. Nu o tratăm ca variație recalculabilă la fiecare modificare a trecutului.

## O10 — retrage sau inversează un fapt procedural

- **Primește:** faptul vizat, documentul/comanda justificativă, felul
  acțiunii și, pentru inversare, data noului efect.
- **Verifică:** starea, perioada, dependenții și limitele temporale după
  acțiune. Retragerea fără dependenți nu înseamnă doar sold final suficient.
- **Scrie atomic:** urma retragerii păstrând conținutul istoric, respectiv
  faptul invers cu proveniență explicită. Retragerea scoate efectul din
  situația curentă inclusiv pentru data lui inițială; inversarea compensează
  din noua dată. Citirea la un moment anterior înregistrării le deosebește.
- **Așteptare:** S8: +20 peste 150, efect 1 martie. După retragerea din
  5 martie, situația zilei de 3 martie cunoscută pe 6 martie este 150;
  după inversarea din 5 martie, aceeași situație este 170. Cunoscută pe
  4 martie, este 170 în ambele ramuri. O rezervare dependentă 160 blochează
  ambele acțiuni cât rămâne în vigoare.

Operația nu amendează singură retragerea actuală din cub. Pentru un document
cu efecte mixte, comanda trebuie să aplice ambele contracte atomic și să
păstreze proveniența cerută. Reconstituirea istorică a efectelor șterse din
cub este problema deschisă din Q3, nu o capacitate presupusă existentă.
Inversarea procedurală nu este automat o rambursare bancară.

## O11 — pregătește acoperirea anului următor

- **Primește:** legalul multianual, anul nou, soldurile la granița anului,
  pozițiile planificat/deschis/bugetar noi și sumele de acoperit.
- **Verifică:** limitele anului nou, soldul legal și atribuirea resturilor;
  exclude dubla execuție a aceluiași rest pe două acoperiri. Nu consumă
  din nou credite de angajament și nu modifică factura originară.
- **Scrie atomic:** pentru fiecare comandă de acoperire, noua rezervare
  și atribuirea reportării necesare păstrării unicității execuției. Nu
  propunem o singură tranzacție pentru toate documentele închiderii anuale.
  Pentru restul unei ordonanțări vechi se emite una nouă prin O07.
  Expirarea dreptului vechi rezultă din intervalul persistat; O11 nu
  scrie un fapt de anulare sau o corecție asupra documentelor vechi.
- **Așteptare:** S9: sold 30, B28 acoperă 30, LM rămâne 70 și creditul
  disponibil rămâne 30. Este admisă și acoperirea parțială B28 = 20:
  permite ordonanțare/plată de cel mult 20, iar 10 rămân neacoperiți.

Pentru LM = 150, facturat = 70, plătit = 40, soldurile de început sunt
110 de executat, din care 30 facturat neplătit și 80 nefacturat. Acoperirea
noului an nu transformă cei 80 în facturi și nici cei 110 în plată autorizată.

## O12 — citește și reconstruiește proiecțiile

- **Primește:** pozițiile/filtrele, data efectului și, pentru o vedere
  istorică, momentul până la care informația este cunoscută.
- **Verifică:** drepturile de citire și disponibilitatea istoricului
  necesar răspunsului. O vedere parțială nu se prezintă ca plafon complet
  pentru operare. Istoricul procedural nu implică automat istoricul cubului.
- **Produce:** disponibilurile și soldurile din sursele canonice. O
  reconstrucție poate înlocui proiecția derivată verificată, nu faptele.
  Publicarea unei proiecții refăcute nu trebuie să amestece versiuni ale
  sursei; mecanismul de actualizare/publicare ține de implementare.
- **Așteptare:** S7 la 25 ianuarie: plafon 100 cu informația de atunci,
  plafon 90 după înregistrarea reducerii admise. S9: 110 = 30 + 80.
  Reconstrucția și citirea directă din fapte dau aceleași valori și
  atribuiri, inclusiv după retragere/inversare. Măsurarea A/B justifică
  materializarea, nu simpla existență a unei interogări mai scurte.

## Puncte rămase înaintea contractului feliei

**Q1 — traseul anual, tranșat de owner.** La lansarea licitației se face
propunerea; la desemnarea câștigătorului se fac bugetarul și legalul, cu
rezervarea aferentă; la semnare, valoarea contractului poate diferi. O
valoare mai mică produce corecția legalului în jos și rectificarea
bugetarului în jos, cu alinierea rezervării. Propunerea, atribuirea și
semnarea sunt fapte distincte, cu documentele, valorile și datele proprii.
Nu impunem egalitatea retrospectivă a sumelor tuturor etapelor.

S10: propunere 100 → bugetar/legal/rezervare 80 la atribuire → toate trei
75 după semnare și rectificări. Diferența de 5 revine disponibilului
deschis pentru alte bugetare; nu rămâne rezervă liberă în bugetarul redus.
Această precizare înlocuiește recomandarea inițială de rezervare separată
înainte de angajare și închide întrebarea asupra momentului din acest flux.

**Q2 — mecanismul anual, tranșat de owner.** Dreptul de execuție are de la
operare valabilitatea anuală persistată. Bugetarul și acoperirea aparțin
anului; autorizarea ordonanțării este limitată la același an. Data efectului
plății trebuie să aparțină intervalului lor. De la 1 ianuarie al anului
următor, dreptul vechi este expirat; istoricul sumelor și plăților rămâne.
Expirarea rezultă din intervalul faptului inițial, fără un proces la miezul
nopții, corecții de închidere sau rescrierea documentelor.

Obligația neplătită și identitatea legalului multianual continuă. Se
constituie acoperirea nouă pe B nou, apoi se emite ordonanțarea nouă.
Acestea sunt operații distincte. Poziția ordonanțării noi referă alocarea
obligației inițiale, acoperirea nouă și alocarea ordonanțării vechi din care
provine suma, când există. Plata consumă autorizarea nouă și execută B nou.
Nu se mai consumă credite de angajament pentru aceeași obligație legală.

Valabilitatea anuală și închiderea perioadei sunt distincte. Prima limitează
data efectului execuției, a doua blochează scrierile în perioada închisă.
Înaintea închiderii, o plată înregistrată târziu cu efect în anul vechi se
verifică și față de acoperirile și ordonanțările anului nou. S9: după 40
plătiți și O28 = 30, încă 5 cu efect în decembrie sunt refuzați, deoarece
45 + 30 > obligația 70; documentele noi nu se reduc automat. După închiderea
perioadei, scrierea cu efect în ea este refuzată indiferent de disponibil.

Este admisă acoperirea parțială: din restul 30, B28 poate acoperi 20,
ordonanțarea nouă poate autoriza 20, iar plata poate executa 20. Ceilalți
10 rămân obligație neplătită, fără drept de plată până la completarea
acoperirii și autorizării. S9 descrie numeric aceste stări și refuzuri.
Q2 este închisă pentru cazul multianual delimitat; excluderea din Q4 rămâne.

**Q3 — proveniența canonică și istoricul citirilor, aprobate.** Investigația din
2026-10-12 este în [analiza Q3–Q4](2026-10-12-111f-q3-q4-provenienta-si-limite.md).
Sursele confirmă cauza pe poziție, cardinalitatea multiplă a postărilor,
transferurile fără plată nouă și ștergerea efectelor de cub la anulare.
Q3-P1…P3 stabilesc selecția explicită a efectului contabil, legături valorice
în afara cubului și scriere/retragere atomică, cu proveniență arhivată.
Q3-P4 delimitează pentru prima felie procedurală citiri integrate la data efectului pe
faptele cunoscute acum și istoric procedural, fără reconstrucția financiară
completă la un moment trecut al cunoașterii după anulări. Owner-ul a aprobat
această limită și forma conceptuală a legăturilor; Q3 este închisă la nivel
de proiectare. Schema fizică rămâne de fixat în contractul feliei.

**Q4 — semantica modificărilor și cazurile neobișnuite.** Variațiile fixe
sunt agreate; rămân plafonul înlocuitor, reatribuirea, schimbarea
clasificării, avansuri, rambursări, curs și baza TVA a limitelor. Globalul
nu rezolvă automat aceste cazuri. Forma documentului salarial și o eventuală
lichidare cu fapt propriu rămân de definit.
Traseul Q1 descrie faptele atribuirii și semnării; eligibilitatea pentru
facturare înaintea semnării și cazul semnării la o valoare mai mare nu
se deduc automat din acest traseu și rămân de delimitat.

[Analiza Q3–Q4](2026-10-12-111f-q3-q4-provenienta-si-limite.md) consemnează
delimitarea aprobată a primei felii procedurale: variații fixe, atribuiri nemodificate pe fapte
operate, traseul anual deja confirmat și flux obișnuit în RON fără TVA
sau operații speciale. Include explicit corecțiile monetare între ani
printre cazurile amânate, distinct de plata soldului din S9. Acestea sunt
limite aprobate ale implementării procedurale inițiale; nu restrâng
contabilitatea existentă. Q4 este închisă prin delimitare. Interpretările
de domeniu ale cazurilor amânate rămân deschise pentru extinderea ulterioară.

**Excludere explicită cerută de owner:** contractele lunare în care luna
a douăsprezecea ajunge să fie plătită în ianuarie rămân neprinse în model,
până la clarificarea interpretării lor. Nu le atribuim tratament prin
analogie cu S9 și nu formulăm acum scenarii sau așteptări pentru ele.

## Legătura cu invarianții și viitoarea felie

111(f) aprobă faptele proprii în afara cubului. Invarianții I și III trebuie
reconciliați explicit pentru documentele exclusiv procedurale și istoricul
lor, înaintea implementării; această analiză nu îi modifică. Fiecare fapt
are document și poziție-sursă; semantica laturilor tipurilor noi trebuie
definită în contractul feliei. II și IV cer contracte tipizate și păstrează
nucleul agnostic la frunze. V păstrează legacy-ul drept evidență. VI nu
este atins: nu propunem schimbări în evaluarea stocului.

Documentele exclusiv procedurale vor necesita o cale explicită de operare
fără postare contabilă fictivă. Acesta este un contract-țintă, nu o afirmație
nouă despre codul curent. O comandă mixtă validează toate efectele înainte
de commit; nucleul contabil nu preia ciclul procedural al angajamentelor.

S1–S10 sunt așteptările numerice de bază; O02 și O06 adaugă așteptări
de refuz explicite pentru proiectare. La implementare, probele vor folosi documente reale,
ușa de operare și cititorii, pe ambele profiluri conform contractului feliei,
prin [verifica.ps1](../../nou/tools/ModelCheck/scripts/verifica.ps1).
Acum s-au modificat numai documente; nu există probe de produs executate,
benchmark, migrare sau review advers care să certifice aceste contracte.

Pasul de proiectare se închide când Q1–Q4 au răspunsuri sau limite explicite
în decizie, sursele fiecărei măsuri sunt numite și fiecare comandă are o
așteptare numerică de admitere/refuz. Implementarea are nevoie apoi de
contractul feliei, inclusiv regula de oprire și review; ordinea TR-D9b nu
se schimbă prin această analiză.
