# Perioade

**Actualizat: 2026-10-16.** [Index](README.md)

Perioada fiscală ca lanț, închiderea ei cu verificarea și politica constatărilor, exercițiul închis pe decembrie, soldurile materializate la închidere și soldul citit.

## Perioada fiscală ca lanț

- Perioada este o verigă identificată prin an și lună, unică între rândurile
  vii. Anul și luna se culeg la creare și nu se mai schimbă. (F27-D1)
- Starea perioadei — închisă, momentul închiderii curente și momentul primei
  închideri — aparține motorului. Pe calea securizată se refuză orice scriere
  asupra ei, ca la postările cubului. (F27-D1)
- Închiderea unei perioade cere perioada precedentă închisă. O perioadă
  precedentă absentă este închisă prin absență și dă capătul lanțului. (F27-D1)
- Redeschiderea cere perioada următoare deschisă sau absentă și un motiv
  scris: se redeschide numai ultima perioadă închisă. Momentul primei
  închideri nu se șterge la redeschidere. (F27-D1)
- Fiecare închidere și redeschidere scrie un rând în istoricul perioadei, cu
  felul, momentul, utilizatorul și motivul. Istoricul este append-only și
  aparține motorului. (F27-D1)
- Fiecare comandă a motorului rulează într-o tranzacție explicită, deschisă pe
  ObjectSpace-ul ei: operarea, anularea și stornarea prin adaptorul de
  operare, generarea și regenerarea închiderii de TVA și a amortizării,
  închiderea, redeschiderea și reconstrucția perioadei. Gardianul de perioadă
  citește starea lunii blocând rândul în citire, iar comenzile perioadei îl
  blochează în scriere înainte de orice calcul. (F27-D1) Tranzacția comenzii
  începe cu blocajul scrierii, deci comenzile se serializează toate între
  ele; vezi „Scrierea serială per bază”. (X-D6)

## Închiderea de perioadă

### Verificarea de închidere

- Verificarea de închidere întoarce constatări tipizate, cu cheie stabilă,
  fel, severitate, text și obiectul la care se referă. Constatările
  STRUCTURALE stau în cod și nu se configurează: perioadă nedefinită, perioadă
  deja închisă, perioadă precedentă deschisă. Ele sunt întotdeauna blocante, iar
  cât timp una dintre ele stă în picioare constatările de conținut nu se mai
  caută. (F27-D2)
- Constatările de CONȚINUT sunt cinci, iar severitatea fiecăreia vine din
  politica de închidere de perioadă, nu din cod: conturile-sursă lunare ale
  închiderii conturilor de rezultat au sold la sfârșitul lunii; închiderea de
  TVA lipsă sau neoperată pe lună; amortizarea lunară lipsă sau neoperată, când
  luna are fișe de amortizat; fiecare document în lucru cu data înregistrării
  în perioadă; fiecare document operat, scadent și cu rest la sfârșitul
  perioadei. Cheile lor sunt `REZ-NEINCHIS`, `ITV-LIPSA`, `AMO-LIPSA`,
  `DRAFT-IN-PERIOADA:{id}` și `REST-SCADENT:{id}`; primele trei n-au sufix
  fiindcă sunt ale societății, nu ale unei unități interne. (F27-D2, E3-D5)
- `REZ-NEINCHIS` judecă soldul, nu documentul: se emite cât timp vreo cheie a
  conturilor-sursă cu moment `Lunar` are sold la ultima zi a lunii, deci și
  când închiderea e operată, dar un document operat după ea în aceeași lună a
  lăsat sold. Textul spune care din cele trei cazuri e: închiderea lipsește,
  există ca draft sau există operată. (E3-D5)
- Un fapt dă o singură constatare:
  documentul în lucru raportat deja de familia lui — închiderea conturilor de
  rezultat, închiderea de TVA sau amortizarea existente ca draft — nu se mai
  repetă ca `DRAFT-IN-PERIOADA` și
  nu intră nici în numărătoarea acelei familii; dacă felul care l-ar raporta e
  `Ignorat` (deci nu se caută), documentul apare normal ca draft. Fiecare
  familie listează cel mult 200 de rânduri, iar restul intră într-un rând de
  rezumat cu cheia `{FEL}:REZUMAT`, la severitatea familiei, care spune câte
  rânduri nelistate acoperă: acceptarea lui le acceptă pe toate, în bloc.
  (F27-D2)
- Închiderea reia verificarea în aceeași tranzacție: orice blocantă refuză
  oricum, iar orice avertisment a cărui cheie nu a fost acceptată refuză și el.
  Refuzul poartă lista ÎNTREAGĂ, un rând pe linie, ca ecranul s-o arate și
  acceptarea să se dea pe constatări concrete, nu pe un indicator de forțare.
  Cheile acceptate care nu corespund niciunei constatări de acum se ignoră:
  raportul citit de operator e o fotografie, refuzul e al stării de acum.
  Cheile acceptate efectiv se scriu pe rândul de istoric. (F27-D2)
- Documentul în lucru rămas într-o perioadă închisă NU e document mort: se
  operează mai departe, cu o dată de înregistrare ulterioară, deci cade în altă
  perioadă decât cea a documentului fizic. Constatarea o spune. (F27-D2, F27-D4)

### Politica severității constatărilor

Severitatea constatărilor de CONȚINUT ale închiderii de perioadă este dată, nu
cod: un rând per fel în politica de închidere de perioadă, cu trei valori.
`Blocant` refuză închiderea și nu se poate accepta; `Avertisment` o lasă să
treacă numai cu acceptare explicită pe constatarea concretă, scrisă în
istoricul lunii; `Ignorat` nu emite constatarea deloc. Un fel fără rând se
comportă ca avertisment și o SPUNE în textul constatării, ca o configurație pe
jumătate să nu treacă tăcut. (F27-D2)

Cele șase feluri și seed-ul lor (ultimul aparține verificării exercițiului):

| Fel | Privat | Bugetar |
|---|---|---|
| Conturile de rezultat au sold la sfârșitul lunii | Blocant | Blocant |
| Închiderea de TVA lipsește sau nu e operată | Blocant | Ignorat |
| Amortizarea lunară lipsește sau nu e operată | Avertisment | Avertisment |
| Document în lucru cu data înregistrării în perioadă | Avertisment | Avertisment |
| Document operat cu rest scadent în perioadă | Ignorat | Ignorat |
| Conturile închiderii au sold la sfârșitul exercițiului | Blocant | Blocant |

Diferența dintre profiluri este de CONȚINUT, nu de mecanism: bugetarul nu e
plătitor de TVA, deci închiderea de TVA îi este tip inert, iar un blocant pe ea
n-ar fi avut niciodată cum să se stingă. Constatarea de închidere de TVA nu se
emite deloc când profilul n-are politica de conturi completă sau când luna nu
are sold pe cele două conturi de TVA. (F27-D2)

Soldul rămas pe conturile de rezultat e blocant pe ambele profiluri: el ar
strica soldul inițial al anului următor. (E3-D5)

Blocantele STRUCTURALE ale lanțului nu sunt în această politică și nu se
configurează: ele sunt regula lanțului. (F27-D2)

## Exercițiul închis

- Exercițiul este anul calendaristic, iar starea lui stă pe decembrie:
  `ExercitiuInchis` și momentul închiderii, ale motorului, refuzate pe calea
  securizată ca starea lunii. Nu există entitate separată; închiderea și
  redeschiderea exercițiului scriu rânduri în istoricul lui decembrie, cu
  felurile `InchidereExercitiu` și `RedeschidereExercitiu`. (E3-D7)
- Verificarea exercițiului are constatări STRUCTURALE, mereu blocante:
  `DECEMBRIE-NEDEFINIT`, `DECEMBRIE-DESCHIS`, `EXERCITIU-INCHIS` și
  `EXERCITIU-PRECEDENT-DESCHIS:{an}` (decembrie anului precedent e definit și
  are exercițiul deschis; nedefinit înseamnă închis prin absență, ca la
  lanț). Constatarea de CONȚINUT se caută numai fără structurale:
  `EX-SOLD-REZIDUAL`, când un cont-sursă din politica închiderii conturilor,
  cu moment `Lunar` sau `Anual`, are sold la 31.12; textul numește conturile.
  Severitatea ei vine din politica de închidere, ca la lună. (E3-D7)
- Închiderea exercițiului reia verificarea în tranzacție și acceptă
  avertismentele pe cheie, ca închiderea lunii. Blochează decembrie anului
  și pe al anului precedent. Nu postează nimic: rândurile `Anual` ale
  închiderii conturilor de rezultat intră în închiderea lunară a lui
  decembrie, datate 31.12, după rândurile `Lunar`. (E3-D7, E3-D8)
- Cât timp exercițiul e închis, redeschiderea lui decembrie se refuză.
  Redeschiderea exercițiului cere un motiv scris și se poate face numai pe
  ultimul exercițiu închis; decembrie rămâne închis, iar nimic nu se
  stornează. (E3-D7)
- Corecția valorică refuză consumul dintr-un an cu exercițiul închis
  (`CV_AN_INCHIS`); un consum din anul anterior cu exercițiul deschis își
  primește cota. Nucleul primește anul ultimului exercițiu închis ca valoare.
  (E3-D10)

## Soldurile materializate la închidere

- Snapshot-ul unei perioade există dacă și numai dacă perioada este DE
  REFERINȚĂ: ultima perioadă închisă sau un decembrie închis. Nu este registru
  și nu este urmă — se reconstruiește integral din cub. (F27-D3, TR-D8)
- Cheia snapshot-ului este cheia completă a atomului: cont plus cele opt
  dimensiuni ale laturii pe partea contabilă; lot, cont, produs și gestiune
  pe partea de stoc, cu data deschiderii păstrată. Debitul și creditul se cumulează separat, fiindcă netarea
  nu este aditivă. Orice raport este rollup aditiv peste ea. (F27-D3, 66d)
- Cheile integral zero se omit. Cheia absentă înseamnă zero pentru orice
  consumator. (F27-D3)
- Închiderea scrie snapshot-ul lunii ca sumă între snapshot-ul precedentei și
  rulajele lunii, apoi șterge snapshot-urile TUTUROR referințelor de dinaintea
  ei care nu sunt capăt de an, nu doar pe al precedentei definite: cu o lună
  nedefinită între ele (închisă prin absență), ultima referință poate fi mai
  veche de o lună, iar snapshot-ul ei ar fi rămas orfan. Referințele se citesc
  înainte ca luna să fie marcată închisă. Fără snapshot precedent, luna se
  calculează prin sumă peste tot istoricul. Redeschiderea șterge snapshot-ul
  lunii și îl reconstruiește pe al precedentei, tot prin sumă integrală.
  Postările nu se ating. (F27-D3, review advers F27, L1)
- Reconstrucția recalculează integral fiecare perioadă de referință,
  raportează diferențele pe rânduri și pe sume înainte de a rescrie, apoi
  șterge snapshot-urile perioadelor care nu mai sunt referințe. Raportul iese
  și când nu există nicio diferență. Prima ei instrucțiune blochează lanțul
  întreg (`FOR UPDATE` pe perioade), ca la închidere: altfel o închidere care
  comite după citirea referințelor ar fi rămas fără snapshot, iar soldurile ar
  fi pornit tăcut de la zero. (F27-D3, 35b, review advers F27, 1b)
- Scrierea și ștergerea snapshot-urilor aparțin motorului: pe calea securizată
  se refuză, ca la postările cubului. Ștergerea lor este fizică, nu amânată. (F27-D3)

## Soldul citit: referință plus rulaje

- Un singur serviciu răspunde „soldul la data d": snapshot-ul ultimei perioade
  de referință care se termină până la d, plus rulajele de după ea. Fără nicio
  perioadă de referință, citirea este integral din cub — de aceea o bază
  fără închideri dă exact aceleași cifre ca una cu închideri. (F27-D3)
- Balanța cere referinței să se termine cel târziu cu o zi înaintea începutului
  perioadei, ca soldul inițial să rămână separabil de rulaj. Aceeași regulă
  pentru balanța pliată pe planul de conturi, care o consumă. O cheie fără
  niciun rând de snapshot și fără rulaj în fereastră nu apare: rândul ei ar fi
  avut inițial, rulaj și sold zero. (F27-D3)
- Fișa de cont primește soldul de dinaintea perioadei ca un singur rând
  sintetic din snapshot, datat la sfârșitul referinței: fereastra îl cumulează,
  iar afișarea îl exclude, ca pe orice rând anterior perioadei. Filtrele de
  dimensiune se aplică înăuntrul snapshot-ului, deci rândul sintetic poartă
  exact coordonatele filtrului. (F27-D3)
- Raportul de stoc, FIFO și pinurile folosesc `Cub.Citiri.Loturi.Cumulate`:
  referință plus fereastră, pe cheia completă. `CumulPerioade.Citeste` alege
  referința și citește sumele în aceeași instrucțiune SQL pentru contabil,
  stoc și partide. Fereastra de după referință se filtrează pe `Data`,
  comparată cu sfârșitul referinței, deci folosește indexul pe dată; cititorii
  de loturi și de partide citesc numai partiția lor (`Spatiu`). Citirile
  securizate recitesc postările. Evaluarea ieșirii
  transmite o graniță strict anterioară datei documentului exclus; fără
  această garanție, excluderea recitește integral postările. Gardul de sold
  intermediar pornește din cumulul de dinaintea primei date propuse și
  verifică prefixele zilnice de la ea încolo, în cub.
  (F27-D3, X-D5; TR-D8 D8-B1/B5)
- Soldurile conturilor de TVA ale închiderii lunare vin din aceeași sursă
  cumulată. (F27-D3)
- Excluderea istorică generală recitește direct postările; excluderea în
  operare poate folosi snapshot-ul strict anterior documentului. Granița
  contabilă rămâne separată de data finală a raportului. (SC-CIT-73/76/77)
- Scrierea globală de snapshot (materializare, reconstrucție, eliminare)
  refuză un ObjectSpace secured înainte de accesarea datelor. (SC-CIT-78)
  În XAF EF Core verificăm `ISecurityEnabledOption.EnableSecurity` al
  contextului: fabrica nesecurizată poate întoarce tot un
  `SecuredEFCoreObjectSpace`. Verificarea rămâne numai pe scriere, până
  când coaja perioadei își alege singură contextul. (104-r2)
- Citirea cumulată nu întreabă de securitate: apelantul declară cine citește
  (`CitireCumul`). `Integrala` (motorul, ușa de sistem, citirile motorului
  pe ușa non-secured) pornește din snapshot; `Vizibila` citește numai
  postările, fiindcă snapshot-ul nu poartă filtrele de rând ale rolului.
  Citirile cumulate din `Cub/Citiri` (`Loturi.Cumulate`,
  `Partide.Cumulate`, `SolduriService.AtomiCumulati`) cer declarația;
  proiecțiile servite pe ușa securizată (balanța, fișa, soldurile de
  partener, stoc și partide, documentele cu rest, SAF-T) au implicit
  `Vizibila`. Închiderea perioadei citește restanțele integral. (104b,
  SC-CIT-95)
- Cheia cu cantitate ȘI valoare zero lipsește din soldul de stoc și din
  soldurile pe loturi, ca din snapshot: un lot consumat integral nu mai este o
  poziție de stoc și nu mai apare în listă. Cheia cu cantitatea zero și valoare
  nenulă RĂMÂNE — reziduul valoric se vede, nu se ascunde. Motorul nu simte
  diferența: citește soldurile cu valoare implicită zero pe cheia absentă.
  (F27-D3, 74g)

## Limitele implementării

- Balanța, balanța pe plan, fișa de cont, soldul de stoc, soldurile pe
  loturi, soldul unei chei, alocarea FIFO, gardianul de sold negativ și
  soldurile de TVA ale închiderii lunare pornesc de la ultima perioadă de
  referință. Două citiri nu folosesc snapshot: situația imobilizărilor citește
  cubul integral al fișelor cerute (097); împerecherile și restul
  documentelor.
  (F27-D3)
- Cheile cu debitul și creditul cumulate zero la referință nu au rând de
  snapshot. (F27-D3)
- Citirile cumulate iau snapshot-ul prin spațiul de obiecte al apelantului.
  Un rol care ar putea citi perioada fiscală fără să poată citi tabelele de
  solduri ar primi soldul inițial zero, fără avertisment. Rolurile livrate nu
  au această formă: cine citește perioada citește și soldurile, iar un rol
  care nu vede perioadele cade pe citirea integrală a cubului. (F27-D3)
- Integritatea snapshot-ului nu e verificată la citire: un rând șters direct
  din bază dă o balanță tăcut greșită. Singura detecție e reconstrucția la
  cerere, care raportează diferența și repară. (F27-r12)

<!-- gen:decizii -->
## Decizii

Deciziile care numesc pagina în `Arii:` (7; titlurile în [index](../decizii/README.md); generat de `docskit gen`, nu se editează): 1, 35, 66, 74 (amendată de 105), 76, 77, 104 (amendată de 106).
<!-- /gen:decizii -->
