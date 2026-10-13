# 111(f) — scenariu numeric pentru atribuiri, limite și istoric procedural

Tip: analiza
Data: 2026-10-11
Alimentează: 111 (f), viitoarea decizie și contractul feliei

Scenariul pune în cifre [acordurile de proiectare](2026-10-11-111f-registre-tipate-reluare.md#continuarea-discuției--acorduri-de-proiectare).
Este propunere pentru discuție, fără cod sau probă ModelCheck executată.
Numerele sunt așteptări independente de implementare și de registrele legacy.
Descrierile owner-ului sunt sursa cerințelor; exemplul nu este validare legislativă.

Pasul următor de proiectare este descris în
[contractele conceptuale ale operațiilor](2026-10-11-111f-contracte-operatii.md).

Continuarea din 2026-10-12, [Q3–Q4](2026-10-12-111f-q3-q4-provenienta-si-limite.md),
investighează sursele canonice și descrie legăturile cu cubul și limitele
primei felii procedurale, aprobate ulterior de owner în aceeași zi.
Q3-N1…N7 adaugă așteptări de reconciliere, retragere și reoperare;
nu sunt probe executate. Aprobarea nu schimbă prioritatea privat-first
sau ordinea TR-D9b.

## Reguli agreate și convențiile exemplului

Sumele sunt lei întregi, fără diferențe de curs, TVA separat, comisioane sau
avansuri. Obligațiile, ordonanțările și plățile folosesc aceeași bază valorică.
Anii 2027 și 2028 sunt ani ipotetici; perioadele sunt deschise, exceptând
proba explicită de închidere. Fiecare fapt are document și poziție-sursă.

Pentru aceste fluxuri, factura ori documentul salarial constată obligația;
ordonanțarea o autorizează la plată, iar plata consumă ordonanțarea.
O poziție comercială poate avea mai multe alocări de execuție.
Nu generalizăm acest traseu la avansuri sau la toate tipurile de plăți.

La cererea explicită a owner-ului, contractele lunare în care luna a
douăsprezecea este plătită în ianuarie rămân în afara modelului până la
clarificarea interpretării. S9 nu stabilește tratamentul acestui caz.

**Precizare aprobată după prima versiune:** asocierea legalului consumă din
bugetar, iar bugetarul este în limita planificatului anual. Ipoteza inițială
de simplă permisiune a perechii este înlocuită: asocierea are sumă rezervată.
Factura consumă disponibilul de facturat al acestei rezervări; nu mai
scade încă o dată aceeași sumă din disponibilul bugetar pentru angajare.

Controalele se fac pe totalul consumatorilor fiecărei poziții, nu numai pe
documentul curent: suma deschiderilor ≤ planificatul anual; suma bugetarelor
≤ deschiderea din care provin; suma rezervărilor
legalelor ≤ bugetarul; obligația atribuită ≤ rezervarea perechii;
ordonanțatul ≤ obligația; plătitul ≤ ordonanțatul. Plafonul legal, anual sau
multianual, se verifică separat. Aceste relații descriu traseul obișnuit
în același an. S9 precizează reacoperirea unei obligații existente în anul
următor: bugetarul de execuție poate diferi de cel de origine al facturii.
Suma istorică a rezervărilor anuale nu este plafonul legal consumat.

Owner-ul a precizat două limite de planificare: planificat anual pentru
bugetare, prin deschideri, și credite de angajament pentru legalele
multianuale. Acoperirea dintr-un bugetar nou a unui sold existent nu este
o nouă angajare legală. Pentru S1–S5, creditul de angajament disponibil
inițial este 160, LM angajează 150 și lasă 10 pentru alte angajări legale.
În S7, creditul disponibil inițial este 200 pentru L multianual de 200;
această limită nu interferează cu verificările temporale ale exemplului.

Owner-ul a precizat lanțul planificat → deschis → bugetar. Deschiderea
are aceleași proprietăți de consum, reducere și verificare temporală ca
planificatul; nu adăugăm un scenariu separat pentru ea. Respectarea limitei
planificatului de către bugetar rezultă tranzitiv, fără omiterea deschiderii.

„Global” este folosit aici pentru o acoperire salarială limitată valoric,
pe un an; perechea globală bugetar–legal este convenția acestui exemplu,
nu dovada că ambele referințe sunt obligatorii în orice caz global.

## S1 — identități și două feluri de plafon

Tripletul T = (Sursa S1, CF1, CE1) este simbolic. Fiecare identificator de
mai jos numește o poziție stabilă, nu un antet sau o versiune a modificării.

| Poziție | Fel | Interval | Plafon | Triplet |
|---|---|---|---:|---|
| B27 | Bugetar individual | 2027 | 100 | T |
| B28 | Bugetar individual | 2028 | 70 | T |
| LM | Legal multianual individual | 2027–2028 | 150 | T |
| LA | Legal anual individual | 2027 | 30 | T |

Perechi autorizate: (B27, LM), (B27, LA), (B28, LM).
(B28, LA) este refuzată, chiar dacă tripletul coincide.
B27 are două legale posibile; LM are două bugetare, cu intervale diferite.
Culegerea filtrează inclusiv după data aplicabilă, apoi aplică implicitul
numai dacă alegerea rămasă este unică.

| Asociere | Sumă rezervată din bugetar | Anul acoperirii |
|---|---:|---|
| B27 + LM | 70 | 2027 |
| B27 + LA | 30 | 2027 |
| B28 + LM | 70 | 2028 |

Rezervările sunt operate înaintea facturilor anului lor. B27 este consumat
70 + 30 = 100, B28 este consumat cu 70, iar LM are 140 repartizat pe cei
doi ani din plafonul 150. Cei 10 nerepartizați ai lui LM nu autorizează
singuri o factură: trebuie mai întâi acoperire într-un bugetar anual.

Proveniență numerică pentru traseul cerut de owner:

| An | Poziție planificare | Poziție deschidere bugetară | Poziție bugetară atribuită |
|---|---|---|---|
| 2027 | PL27 = 140 | DB27 = 120, provenită din PL27 | B27 = 100, provenită din DB27 |
| 2028 | PL28 = 90 | DB28 = 80, provenită din PL28 | B28 = 70, provenită din DB28 |

Disponibilul planificat pentru alte deschideri este 140 − 120 = 20 în 2027
și 90 − 80 = 10 în 2028. Disponibilul deja deschis pentru alte bugetare
este 120 − 100 = 20 în 2027 și 80 − 70 = 10 în 2028.
Disponibilul fiecărui bugetar pentru asocieri
legale noi este zero după rezervările de mai sus, încă înainte de facturi.
„Deschidere bugetară” nu este tranzacția de deschidere a bazei din cub.
Plafonul multianual 150 nu se deduce din 100 + 70 și nu se resetează anual.

## S2 — două facturi, patru alocări

Toate faptele din S2–S4 au efect în 2027, în ordinea prezentată.
F1 și F2 au fiecare câte o poziție comercială; alocările însumează exact
valoarea poziției lor și au identitate proprie.

| Factură / poziție | Alocare | Pereche | Sumă |
|---|---|---|---:|
| F1 / 1 = 70 | A1 | B27 + LM | 50 |
| F1 / 1 = 70 | A2 | B27 + LA | 20 |
| F2 / 1 = 30 | A3 | B27 + LM | 20 |
| F2 / 1 = 30 | A4 | B27 + LA | 10 |

Obligație constatată: B27 = 100, LM = 70, LA = 30.
Disponibil pentru o nouă obligație pe B27 + LM = 0, pe B27 + LA = 0.
LM are încă 80 nefacturat din plafonul total, dar această diferență nu
este disponibil de facturat pe B27. Bugetarul era deja rezervat integral;
facturarea nu îl consumă a doua oară.
Totalul facturilor este 100, nu 200: vederea bugetară și vederea legală
verifică aceeași valoare, nu se însumează între ele.

## S3 — ordonanțări și plăți parțiale

O1 are două poziții: 40 către A1 și 15 către A2. O2 are două poziții:
10 către A3 și 5 către A4. Fiecare poziție de ordonanțare nominalizează
alocarea facturii și păstrează atribuirea ei.

P1 = 30: execuție 20 către O1/A1 și 10 către O1/A2; stingere contabilă
30 către partida facturii F1. P2 = 25: execuție 10 către O1/A1, 10 către
O2/A3 și 5 către O2/A4; stingere contabilă 10 către F1 și 15 către F2.
Fiecare factură are o singură partidă în acest exemplu.

| Alocare factură | Obligație | Ordonanțat | Plătit | Rămas de ordonanțat | Rămas de plătit din ordonanțat |
|---|---:|---:|---:|---:|---:|
| A1: B27 + LM | 50 | 40 | 30 | 10 | 10 |
| A2: B27 + LA | 20 | 15 | 10 | 5 | 5 |
| A3: B27 + LM | 20 | 10 | 10 | 10 | 0 |
| A4: B27 + LA | 10 | 5 | 5 | 5 | 0 |
| Total | 100 | 70 | 55 | 30 | 15 |

Plătit pe LM = 40, pe LA = 15, pe B27 = 55.
Stins pe F1 = 40, rest 30; stins pe F2 = 15, rest 15.
Restul total al facturilor, 45, include 30 neordonanțat și 15 ordonanțat
neplătit. Nu confundăm restul de stins cu disponibilul pentru plată.

P1 demonstrează cele două defalcări: un rând contabil și două atribuiri de
execuție. Nu le îmbinăm cartezian. Reconcilierea verifică și factura-sursă:
o versiune greșită a lui P2 cu stingere F1 = 15 și F2 = 10 este refuzată,
deși totalul rămâne 25, deoarece execuția atribuie F1 = 10 și F2 = 15.
Nu rezultă de aici că orice alocare procedurală este o partidă distinctă.

Cititorul plătitului trebuie să selecteze o singură contribuție pentru
fiecare sumă: nu adună debitul cu creditul și nu numără transferurile de
partidă drept plăți noi. Alegerea efectului canonic rămâne de proiectat;
nu introducem o a doua sursă autoritativă pentru suma plătită.

## S4 — refuzuri independente și atomicitate

Fiecare rând pornește din S3, cu singura pregătire indicată; rândurile nu
se execută cumulativ. Refuzul păstrează sumele S3 și faptele existente.
Nu sunt alocate încă nume de coduri de refuz; ele vor intra în contract.

| Încercare | Pregătire | Așteptare numerică |
|---|---|---|
| Bugetar nou 21 pe DB27 | Niciuna | Refuz: bugetarele ar totaliza 121 > deschisul 120, deși planificatul este 140 |
| Suplimentare rezervare B27 + LM cu 1 | Niciuna | Refuz: rezervările B27 ar ajunge la 101 > 100 |
| Factură nouă 1 pe B27 + LM | Niciuna | Refuz: obligația pe pereche ar ajunge la 71 > rezervarea 70, deși LM ar fi 71 ≤ 150 |
| Suplimentare rezervare B27 + LA cu 1 | Suplimentare B27 cu 20, în limita DB27 = 120 | Refuz: rezervarea legalului anual ar ajunge la 31 > plafonul LA = 30 |
| Ordonanțare nouă 6 pe A2 | Niciuna | Refuz: A2 ar fi ordonanțat 21 > 20, deși mai există 30 de ordonanțat în total |
| Plată nouă 11 pe O1/A1 | Niciuna | Refuz: A1 ar fi plătit 41 > 40 ordonanțat, deși totalul permite încă 15 |
| Reducere PL27 la 119 | Niciuna | Refuz: deschisul 120 > planificatul 119 |
| Reducere B27 cu 1 | Niciuna | Refuz: rezervările 100 > bugetarul 99; refuzul există și înaintea facturilor |
| Reducere rezervare B27 + LM la 69 | Niciuna | Refuz: obligația pe pereche 70 > rezervarea 69 |
| Reducere O1/A1 de la 40 la 29 | Niciuna | Refuz: plătit 30 > ordonanțat 29 |
| Retragere F1 | Niciuna | Refuz: are ordonanțări și plăți dependente; nu este suficient că restul său este pozitiv |
| Pereche B28 + LA | Niciuna | Refuz de asociere/perioadă, fără efecte |

O comandă cu două atribuiri, una admisibilă și una refuzată, nu persistă
nici efectul contabil, nici pe cel procedural al vreuneia. Controlul se
face pe întregul lot candidat: două atribuiri noi de câte 6 către O1/A1
din aceeași comandă depășesc împreună disponibilul 10 și sunt refuzate.

Concurență: două comenzi distincte cer fiecare 8 din cei 10 rămași pe
O1/A1. Prima admisă duce plătitul A1 la 38 și disponibilul la 2; a doua
este refuzată. Plătitul total este 63, nu 71. Validarea completă și
scrierea sunt în aceeași tranzacție, sub serializarea comună; o proiecție
întârziată sau date filtrate de drepturile cititorului nu pot decide limita.

## S5 — al doilea an al angajamentului legal

Continuăm S3, fără încercările din S4. În 2028, F3 = 70 are o alocare A5
pe B28 + LM; se ordonanțează 60 și se plătesc 50. Aici B28 acoperă numai
factura nouă F3; nu plătim în această ramură resturile din 2027. Sunt suficiente
faptele persistate din 2027; niciun document din 2027 nu se recalculează.

| Poziție / vedere | Obligație | Ordonanțat | Plătit | Plafon minus obligație, nu disponibil de facturat |
|---|---:|---:|---:|---:|
| B27, anul 2027 | 100 | 70 | 55 | 0 |
| B28, anul 2028 | 70 | 60 | 50 | 0 |
| LM, 2027–2028 | 140 | 110 | 90 | 10 |
| LA, anul 2027 | 30 | 20 | 15 | 0 |

Disponibilul de facturat pe fiecare dintre cele trei asocieri este zero.
Pentru a izola limita multianuală, suplimentăm întâi DB28 cu 10, până la
PL28 = 90, apoi B28 cu 20, până la DB28 = 90. Cei 20 din B28 sunt
disponibili pentru rezervare, nu automat pentru facturare.

După precizarea S9, comparația 151 > 150 nu poate fi aplicată mecanic
sumei istorice a rezervărilor anuale. Pentru obligații noi, limita este
neechivocă: o factură suplimentară 11 ar duce obligația LM la 151 > 150
și este refuzată; una de 10 ar duce-o la 150 și trece controlul legal.
Admiterea ei completă cere și rezervarea anuală corespunzătoare. Ordinea
și controalele reacoperirii versus acoperirii de obligații noi trebuie
delimitate în modelul conceptual; nu certificăm aici suplimentarea rezervării.
Plata nu eliberează bugetarul anului executat; noul an își aprobă acoperirea.

## S6 — angajament global fără factură artificială

Caz independent. BG = 120 și LG = 120 sunt poziții globale pentru 2027,
LG fiind legal anual. Perechea poartă tripletul salarial TG și acoperirea
aprobată. Proveniența bugetară de exemplu: PLG = 150 → DBG = 130 → BG = 120;
disponibilul planificat pentru deschideri este 20, iar cel deschis pentru
bugetare este 10. Asocierea BG + LG rezervă 120, deci
disponibilul BG pentru alte legale este zero înaintea documentelor salariale.

Documentul salarial DS1 constată 75, DS2 constată 25; fiecare are o
alocare la BG + LG. Forma documentului și defalcarea obligațiilor salariale
pe beneficiari și conturi nu sunt specificate prin aceste două totaluri.

| Sursă a obligației | Constatare | Ordonanțat | Plătit | Rămas de ordonanțat | Rămas de plătit din ordonanțat |
|---|---:|---:|---:|---:|---:|
| DS1 | 75 | 70 | 50 | 5 | 20 |
| DS2 | 25 | 20 | 15 | 5 | 5 |
| Total | 100 | 90 | 65 | 10 | 25 |

Disponibil pentru obligații noi pe rezervarea BG + LG: 20, deși
disponibilul BG pentru legale noi este zero. O obligație nouă de 21 este
refuzată. Plățile referă alocările ordonanțărilor și ajung la
BG + LG; nu au nevoie de alternativa „angajament sau triplet liber”.
Exemplul demonstrează această posibilitate pentru salarii, nu pentru
comisioane, avansuri și toate celelalte plăți încă nedelimitate.

## S7 — efect retroactiv și informație cunoscută

Caz independent: planificat anual și deschis 200, B = 100 și L = 200 începând cu
10 ianuarie 2027; rezervare B + L de 80 la 18 ianuarie; factură 80 pe
rezervare la 20 ianuarie; suplimentare B cu 50 la 1 februarie.
Fiecare este înregistrată la data efectului. Planificatul, deschisul și L
nu limitează exemplul.
Faptele zilei sunt ordonate în test prin momente de înregistrare distincte.

Pe 5 februarie se încearcă reducerea B cu 30, cu efect din 15 ianuarie.
La final plafonul ar fi 120, însă între 18 ianuarie și 1 februarie ar fi
70 < 80 rezervat, încă înaintea facturii. Refuz: faptele acceptate rămân
B = 100, rezervare 80, factură 80 și suplimentarea B cu 50.

Într-o ramură alternativă se admite reducerea cu 10, cu aceeași dată de
efect și același moment ulterior al înregistrării:

| Data pentru care întrebăm | Informație cunoscută până la | Plafon B | Rezervat | Obligație | Disponibil pentru rezervări noi |
|---|---|---:|---:|---:|---:|
| 25 ianuarie | 25 ianuarie | 100 | 80 | 80 | 20 |
| 25 ianuarie | 5 februarie, după admiterea reducerii | 90 | 80 | 80 | 10 |
| 6 februarie | 6 februarie | 140 | 80 | 80 | 60 |

Aceleași date efective pot avea răspunsuri diferite după momentul
cunoașterii. Suma facturii rămâne 80, iar suplimentarea rămâne +50.
Nu o recalculăm ca să restabilim un presupus plafon absolut de 150.

Control simetric al rezervării: B = 100 din 10 ianuarie, reducere acceptată
cu 30 din 15 ianuarie, suplimentare cu 50 din 1 februarie, fără rezervări
sau facturi inițial; planificat, deschis și legal 200. O rezervare 80 introdusă
pe 5 februarie cu efect din 12 ianuarie este refuzată: ar fi acoperită
la început și în februarie, dar ar încălca bugetarul 70 în interval.

Control simetric al facturii, în altă ramură: planificat și deschis 200, B = 150 și
L = 200; rezervare 100 din 10 ianuarie, reducerea rezervării cu 30 din
15 ianuarie și suplimentarea ei cu 50 din 1 februarie, fără facturi inițial.
O factură 80 introdusă pe 5 februarie cu efect din 12 ianuarie este
refuzată: ar fi acoperită pe 12 ianuarie și în februarie, dar ar încălca
rezervarea 70 între 15 ianuarie și 1 februarie.

Dacă ianuarie este închis înaintea încercării, chiar reducerea de 10
numeric admisibilă este refuzată. Nu folosim înregistrarea din februarie
pentru a ocoli granița perioadei închise.

## S8 — retragere versus inversare și reconstrucție

Caz independent, fără consumatori sau perioade închise: planificat și deschis
200, B = 150;
o suplimentare +20 produce efect și este înregistrată la 1 martie.
Comparăm două ramuri alternative, cu acțiunea înregistrată la 5 martie:

| Acțiune | Efect la 3 martie, cunoscut la 4 martie | Efect la 3 martie, cunoscut la 6 martie | Efect la 6 martie, cunoscut la 6 martie |
|---|---:|---:|---:|
| Retragerea suplimentării | 170 | 150 | 150 |
| Inversare −20 cu efect din 5 martie | 170 | 170 | 150 |

Retragerea păstrează identitatea și conținutul faptului retras plus urma
acțiunii; nu este un simplu flag curent care face imposibil primul răspuns.
Inversarea referă faptul compensat și nu schimbă intervalul anterior.
Dacă o rezervare de 160 către un legal cu plafon suficient ar fi fost
acceptată pe 2 martie, ambele încercări ar fi refuzate, chiar fără factură:
retragerea ar invalida trecutul, inversarea ar lăsa din 5 martie rezervarea
160 peste bugetarul 150.

Reconstituirea proiecției din faptele păstrate trebuie să reproducă exact
toate cele trei coloane din fiecare ramură. Aceasta este cerință pentru
istoricul procedural. Istoricul complet al execuției citite din cub,
inclusiv efecte șterse prin retragerea actuală, nu este încă specificat.

## S9 — restul din 2027 execută bugetarul din 2028

Caz independent, pe precizarea owner-ului. Credite de angajament disponibile
100; LM angajează 70 și lasă 30 pentru alte angajamente multianuale.
În 2027: planificat 70, deschis 70, B27 = 70, rezervat pe LM = 70.
Factura F = 70 nominalizează LM și acoperirea B27. Se ordonanțează și se
plătesc 40; rămân 30 neordonanțat și neplătit. Varianta cu ordonanțare
veche parțial plătită este descrisă separat mai jos.

În 2028: planificat 30, deschis 30, B28 = 30, acoperire pe același LM = 30.
O ordonanțare nouă de 30 nominalizează restul aceleiași alocări din F și
acoperirea B28. Plata de 30 stinge aceeași factură și execută B28.

| Măsură | După plata din 2027 | După plata din 2028 |
|---|---:|---:|
| Angajare legală LM | 70 | 70 |
| Credit de angajament rămas | 30 | 30 |
| Facturat cumulat pe LM | 70 | 70 |
| Plătit cumulat pe LM | 40 | 70 |
| Restul facturii / sold neplătit LM în acest exemplu | 30 | 0 |
| Executat pe B27 | 40 | 40 |
| Executat pe B28 | 0 | 30 |

Nu se creează o factură nouă, nu se modifică FK-ul de origine al facturii
și nu se mai consumă 30 din creditul de angajament. O atribuire de execuție
are nevoie de ambele informații: alocarea-sursă a obligației și acoperirea
anuală executată. Reprezentarea fizică rămâne deschisă.

Acesta este contraexemplul pentru două simplificări din prima versiune:
preluarea necondiționată a bugetarului facturii la plată și limitarea sumei
istorice a rezervărilor anuale la valoarea legalului. B27 = 70 și B28 = 30
au acoperit, la momente diferite, inclusiv același rest de 30; suma 100 nu
înseamnă angajare legală 100 sau execuție 100. În fiecare an, acoperirea
respectă limitele acelui an; execuția cumulată a aceleiași obligații este 70.

Owner-ul a aprobat valabilitatea anuală persistată la operare: dreptul
de execuție pe B27 expiră de la 1 ianuarie 2028, fără rescrierea faptelor
sau corecții generate la trecerea anului. Aceiași 30 nu sunt executabili
simultan pe ambele acoperiri. Noul bugetar
acoperă soldul legalului; când legalul este parțial nefacturat, trebuie
distins soldul legal de simplul rest al facturilor deja primite.

### Varianta cu ordonanțare veche parțial plătită

Ramură independentă a aceluiași caz multianual: în 2027 se ordonanțează
70 și se plătesc 40. Owner-ul a precizat că pentru restul neplătit se
emite o ordonanțare nouă în anul următor. Aceasta autorizează 30 pe B28,
cu trasabilitate la alocarea ordonanțării vechi și la aceeași factură F.

| Măsură | După plata din 2027 | În 2028, înaintea noii ordonanțări | După noua ordonanțare | După plata din 2028 |
|---|---:|---:|---:|---:|
| Valoarea autorizării inițiale păstrate în istoric | 70 | 70 | 70 | 70 |
| Valoarea ordonanțării noi | — | — | 30 | 30 |
| Plătit cumulat pe obligație | 40 | 40 | 40 | 70 |
| Obligație neplătită | 30 | 30 | 30 | 0 |
| Rest executabil prin ordonanțarea veche | 30 | 0 | 0 | 0 |
| Rest executabil prin ordonanțarea nouă | 0 | 0 | 30 | 0 |
| Plătit cumulat + resturi autorizate executabile | 70 | 40 | 70 | 70 |

Controlul pe obligație folosește plătitul cumulat plus resturile autorizate
încă executabile, nu suma istorică 70 + 30 a documentelor emise. Istoricul
ordonanțării vechi și plata ei de 40 rămân; nu se refac factura sau legalul.
Execuția este 40 pe B27 și 30 pe B28, ca în cazul de bază.

O27 păstrează autorizarea de 70, valabilă în 2027. În 2028 cei 30 sunt
rest neplătit cu autorizare expirată; dreptul de plată revine numai prin
acoperirea B28 și noua O28. Expirarea nu cere un fapt suplimentar sau un
proces programat. Regula completă aprobată este Q2 din
[contractele operațiilor](2026-10-11-111f-contracte-operatii.md).

### Acoperire parțială și înregistrare târzie — mecanism aprobat

Ramură independentă: după O27 = 70 și plată 40, B28 acoperă doar 20 din
restul de 30. O28 = 20 este admisă; 10 rămân neacoperiți. După plata de
20 pe O28, plătitul cumulat este 60, obligația neplătită este 10 și restul
autorizat executabil este zero. Plata nu eliberează acoperirea anuală
executată; cei 10 cer completarea acoperirii și o autorizare nouă.

Fiecare încercare de mai jos este independentă; refuzul nu persistă efecte
contabile sau procedurale și nu ajustează automat documentele existente.

| Încercare | Stare de pornire | Așteptare |
|---|---|---|
| Plată 1 cu efect în 2028 pe O27/B27 | O27 = 70, plătit 40; 2027 încă deschis | Refuz prin valabilitatea anuală; plătitul rămâne 40 |
| O28 = 21 | Rest 30, B28 și acoperire 20, nicio O28 | Refuz: 21 > 20 acoperit; obligația rămâne 30 |
| Plată 21 pe O28 | O28 = 20, B28 și acoperire 20, nimic plătit în 2028 | Refuz: 21 > 20 autorizat; plătitul cumulat rămâne 40 |
| Încă 5 plătiți pe O27 cu efect în decembrie 2027, înregistrați după O28 | O27 = 70, plătit 40; B28 și O28 = 30, neplătită; decembrie deschis | Refuz: la data O28 ar rezulta 45 + 30 = 75 > obligația 70; rămân plătit 40 și O28 = 30 |
| Încă 5 plătiți pe O27 cu efect în decembrie 2027, înregistrați în ianuarie | O27 = 70, plătit 40; nicio acoperire/ordonanțare în 2028; decembrie deschis | Admis: plătit 45, rest 25; în 2028 dreptul vechi este expirat, rest executabil 0 |
| Aceeași plată târzie de 5 după închiderea lui decembrie | O27 = 70, plătit 40; nicio acoperire/ordonanțare în 2028 | Refuz de perioadă; plătitul rămâne 40, restul 30 |

Înregistrarea târzie folosește data efectului pentru valabilitate și
reverifică toate acoperirile și autorizările ulterioare afectate. Dacă este
admisă, proiecția soldului la început de an reflectă noile fapte cunoscute;
faptele și valorile documentelor operate nu sunt recalculate. Aceste cazuri
privesc legalul multianual din S9, fără a extinde modelul la contractele
lunare excluse explicit.

### Soldul de început de an — direcție agreată

Owner-ul a precizat că în legacy baza bugetarului era anuală, iar legalul
multianual păstra soldul de început de an. Owner-ul a acceptat pentru noul
model soldul de început de an ca proiecție a faptelor până
la granița anului, cu identitatea LM păstrată. Persistarea lui, dacă este
utilă, cere reconstrucție, proveniență și verificare de echivalență; nu
creează o nouă angajare legală și nu înlocuiește istoricul.

Definițiile următoare au fost acceptate pentru cazul simplificat
fără corecții, avansuri sau curs, pentru legal 150, facturat 70 și plătit 40,
păstrând distinct:

| Măsură la începutul anului nou | Calcul | Rezultat |
|---|---|---:|
| Legal rămas de executat prin plată | 150 − 40 | 110 |
| Din acesta, deja facturat și neplătit | 70 − 40 | 30 |
| Din acesta, încă nefacturat | 150 − 70 | 80 |

Cei 110 nu autorizează automat o plată sau o factură de 110: facturile
noi au limita 80, iar plata cere constatarea obligației, ordonanțare și
acoperirea anuală. Soldul începutului de an nu consumă din nou creditul
de angajament. Pentru citirea la un moment istoric al cunoașterii rămâne
necesar istoricul efectelor relevante; un singur sold suprascris nu îl oferă.

## Propunerea cu identitate comună — sens clarificat de owner

„Id legal anual” numește aceeași poziție care începe ca propunere și devine
legal anual; nu numește o poziție anuală sub legalul multianual. Bugetarul
este în limita propunerii. Identitatea comună nu implică
aceeași stare, aceeași sumă ori aceeași dată a efectului: lansarea,
desemnarea câștigătorului și semnarea trebuie să rămână fapte identificabile,
conform traseului S10 precizat de owner.

Nu introducem o nouă ierarhie anuală sub multianual și nu substituim
fluxului n×m agreat cu bugetarele o relație 1–1. Relația dintre plafonul propunerii,
rezervarea legalului și suma efectiv angajată trebuie formulată fără
dependență circulară și fără a considera o propunere drept legal aprobat.

## S10 — propunere, desemnarea câștigătorului și semnarea contractului anual

Owner-ul a precizat succesiunea: la lansarea licitației se face propunerea;
la desemnarea câștigătorului se fac bugetarul și legalul; la semnare, dacă
valoarea diferă în jos, se corectează legalul și bugetarul. Se păstrează
faptele și valorile etapelor, fără egalizare retrospectivă.

Caz independent, fără facturi sau alți consumatori. Planificat 140,
deschis 120; aceeași poziție propunere–legal anual în toate etapele:

| Etapă | Faptul propunerii inițiale | Bugetar curent | Legal curent | Rezervare curentă | Disponibil deschis pentru alte bugetare |
|---|---:|---:|---:|---:|---:|
| Lansarea licitației | 100 | — | — | — | 120 |
| Desemnarea câștigătorului | 100 | 80 | 80 | 80 | 40 |
| Semnare la 75, după operarea rectificărilor | 100 | 75 | 75 | 75 | 45 |

Semnarea justifică faptele corective de −5 pe legal și bugetar, cu reducerea
rezervării cu 5. Nu se rescriu propunerea 100 sau faptele atribuirii de 80.
Datele efectelor și momentele înregistrării sunt cele ale faptelor respective.
Disponibilul planificat pentru deschideri rămâne 20; nu se reduce automat
deschiderea de 120. În bugetarul redus, disponibilul pentru alte rezervări
este 75 − 75 = 0. Cei 5 au revenit la disponibilul deschis, nu sunt
numărați încă o dată drept rezervă liberă în același bugetar.

Validarea rectificărilor păstrează regulile temporale și ale dependenților.
Înregistrarea unor valori mai mici la semnare nu rescrie obligații deja
operate. Cazul semnării la o valoare mai mare și eligibilitatea facturării
între atribuire și semnare rămân de precizat; exemplul nu le presupune.

## Consecințe și întrebări pentru contract

1. „O poziție bugetară / legală” nu înseamnă „un singur rând de document
   pentru tot istoricul”: identitatea supraviețuiește faptelor modificatoare.
   O referință la faptul inversat nu devine lanțul general al modificărilor.
2. Atribuirea exactă precedă optimizarea citirilor. Locul fizic al perechii,
   legătura cu postarea canonică și o eventuală proiecție rămân deschise;
   faptul plății nu se întreține în două surse autoritative.
3. Rezervarea prin asociere, lanțul planificat → deschis → bugetar și
   execuția pe bugetarul nou a soldului multianual sunt tranșate de owner.
   Identitatea propunere–legal anual, valabilitatea anuală, ordonanțarea
   nouă și acoperirea parțială sunt clarificate. Rămân extinderea măsurilor
   soldului inițial la cazurile speciale, plafonul înlocuitor față de
   modificarea aditivă și schimbarea clasificării.
   Reatribuirea unei obligații operate nu poate fi o simplă editare de FK.
4. Rămân de definit avansuri, rambursări, comisioane, curs, TVA, lichidarea
   dacă are fapt propriu și efectul corecțiilor fiecărei etape. Nu deducem
   toate regulile acestora din inegalitățile cazului obișnuit.
5. Invarianții I și III descriu încă urma operației ca postare în cub și
   retragerea prin ștergerea efectelor. 111(f) aprobă faptele proprii din
   afara cubului; viitoarea decizie trebuie să reconcilieze explicit textul
   pentru documentele exclusiv procedurale și pentru istoricul lor păstrat.
   Nu modificăm acum invarianții și nu extindem implicit istoricul cubului.
6. Invarianții II și IV cer contracte și mecanisme tipizate: fără comutator
   pe tip în nucleu, fără structură configurată generic în tabele. I cere
   și semantica laturilor documentelor noi, încă de definit. III cere citiri
   din fapte rezolvate; V exclude copierea convențiilor legacy ca autoritate.
   VI nu este atins de acest scenariu.
7. Drepturile proiecțiilor și vizibilitatea motivelor de refuz necesită
   contract propriu; verificarea internă a limitei trebuie să vadă toate
   faptele relevante. Echivalența proiecțiilor, costul validării intervalelor,
   spațiul și scrierea se vor măsura A/B pe aceeași bază.

Pasul următor este confirmarea convențiilor care schimbă semantica, apoi
decizia și contractul feliei. Această analiză nu pornește implementarea,
nu schimbă ordinea TR-D9b și nu constituie review advers sau certificare.
