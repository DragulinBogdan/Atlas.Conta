# Limite curente

**Actualizat: 2026-09-16.** [Index](README.md)

Această pagină delimitează implementarea disponibilă. Elementele de aici nu
sunt angajamente de livrare și nu descriu o ordine de implementare.

## Domeniu și operare

- Serializarea operațiilor concurente și reluarea idempotentă generală a
  comenzilor nu sunt acoperite complet. Validarea într-o singură operație
  nu dovedește protecția față de două comenzi simultane. (25f, 42f)
- Închiderea unei perioade și operarea unui document în ea sunt serializate
  numai pe căile care trec prin adaptorul de operare și prin comenzile de
  generare ale închiderii de TVA și ale amortizării. Uneltele standalone —
  ModelCheck, Import1C, Migrare — cheamă motorul direct, fără tranzacția
  comenzii, deliberat: acolo nu există concurență. (F27-D1)
- Balanța, balanța pe plan, fișa de cont, soldul de stoc, soldurile pe loturi,
  soldul unei chei, alocarea FIFO, gardianul de sold negativ și soldurile de
  TVA ale închiderii lunare pornesc de la ultima perioadă de referință. Trei
  citiri rămân pe registrul integral: situația imobilizărilor, fiindcă al
  patrulea registru nu are snapshot în felia aceasta; oracolul golirilor, care
  citește rânduri concrete, nu solduri; împerecherile și restul documentelor,
  până la felia care le datează. (F27-D3)
- Inițialul de stoc al SAF-T rămâne pe registrul integral: agregatul lui
  raportează și NUMĂRUL de rânduri de registru pe tipurile de stoc
  nedeclarate, iar dintr-un snapshot numărul nu se mai poate afla. Inițialul
  de CONT al SAF-T trece prin balanță, deci pornește de la referință.
  (F27-D3, F27-r10)
- Rândurile integral nule dispar din rapoarte după prima închidere. O cheie cu
  debitul și creditul cumulate zero la referință nu are rând de snapshot, deci
  un cont sau un cont cu repartitor fără nicio mișcare în perioada cerută nu
  mai apare deloc în balanță, în loc să apară cu patru zerouri. Măsurat pe baza
  de import: 1.743 rânduri din 72.910 pe balanța analitică a lunii decembrie,
  toate cu inițial, rulaj și sold zero. Cifrele rândurilor rămase nu se
  schimbă, iar declarațiile oricum nu raportează soldul zero. (F27-D3)
- Citirile cumulate iau snapshot-ul prin spațiul de obiecte al apelantului. Un
  rol care ar putea citi perioada fiscală fără să poată citi tabelele de
  solduri ar primi soldul inițial zero, fără avertisment. Rolurile livrate nu
  au această formă: cine citește perioada citește și soldurile, iar un rol care
  nu vede perioadele cade pe citirea din registrul integral. (F27-D3)
- Snapshot-urile de referință se citesc fără filtrarea de securitate pe rând.
  Fișa de cont își păstrează gate-ul strict (echivalența celor două căi,
  numărată pe TOT istoricul contului, nu doar pe fereastra de după referință),
  deci un utilizator restrâns pe rânduri primește 403 ca înainte; balanța și
  soldul de stoc trec prin LINQ, unde snapshot-ul e o entitate ca oricare
  alta, iar o restricție pe rând pusă pe registru NU se propagă asupra lui.
  (F27-D3, 66)
- Constatările de închidere acoperă doar blocantele structurale ale lanțului.
  Constatările de conținut și severitatea lor ca politică nu sunt acoperite,
  iar corpul cu chei acceptate este primit și ignorat. (F27-D2)
- Închiderea și redeschiderea perioadei au ecran în XAF Blazor, nu în
  clientul React. (F27-D2)
- Două împerecheri noi, încă necomise în același context, au o limită de
  vizibilitate în calculele bazate pe interogarea bazei. Plafoanele nu trebuie
  prezentate ca protecție completă pentru orice lot de scrieri concurente. (41d)
- Capacitatea NTC este netată pe contrapartidă și latură, nu pe cont.
  Calculul restului scade conservator legăturile existente în ambele roluri. (76-r1, 76-r2)
- Ieșirea fiscală RLF poate lăsa un reziduu valoric pe cheia de stoc golită.
  Documentele retroactive nu reevaluează ieșirile deja operate. (75a)
- Unele distribuiri ASM nu sunt reprezentabile exact la precizia prețului.
  Cazurile refuzate nu sunt corectate prin prețuri sau valori forțate. (75-r4, 76c)
- Recepția unui NIR nu declanșează automat completarea DSC pentru facturile
  cu acoperire parțială. Există comanda de generare suplimentară pe FCL. (37g, 38d)
- Fluxurile de rezervare, comenzi de vânzare și distribuire a aceleiași
  facturi din mai multe gestiuni nu sunt acoperite complet. (37g, C1a)
- Retururile nu au un flux general propriu de compensare; se folosește NTC. (46f, 76g)
- Declarația vamală nu este document stins: taxa în vamă se plătește printr-o
  plată către biroul vamal, fără împerechere și fără rest pe document; soldul
  pe partener al contului de taxă nu se citește pe dimensiune (latura de terț
  a plăților poartă contul propriu). Taxele vamale și accizele nu intră în
  costul de achiziție; comisionarul care refacturează taxa nu are flux propriu.
  Anularea unei facturi legate la o declarație operată nu se refuză; starea
  facturii se arată. (86-r1, 86-r2, 86-r10, 86-r11, 86-r13)
- Imobilizările: activele în curs (231) nu au punere în funcțiune care
  postează; reevaluarea are doar locul rezervat în registru; transferul nu
  lasă rând explicit; degresiva AD2 și amortizarea pe unități de producție,
  ajustările pentru depreciere, leasingul și obiectele de inventar în
  folosință nu sunt acoperite; cheltuiala nu se repartizează pe mai multe
  centre de cost; legătura ieșirii cu factura de vânzare nu este evidență;
  deductibilitatea valorii rămase la ieșire nu are regulă; eligibilitatea
  metodei fiscale pe categorie e doar documentată; patru poziții-părinte din
  catalog nu au bandă de durată; fișele din 1C nu se migrează încă; nu
  există acțiune XAF de generare a amortizării. Brutul fiscal zero pe linia
  de punere în funcțiune nu se poate exprima; anularea unui eveniment din
  luna unei amortizări deja operate e refuzată deși luna nu depinde de el;
  două generări simultane ale aceleiași luni lasă două drafturi care se
  blochează reciproc. Lookup-urile XAF ale fișei și ale liniei PIF nu sunt
  filtrate pe natură, stare și loc; clasificarea se caută doar pe denumire;
  mesajele gardienilor scriu numele membrilor enum. (87, F26-r1…r22)
- Salariile, execuția bugetară completă, producția pe rețete, împărțirea pe
  cofinanțări și contabilitatea multivalutară nu sunt module complete în
  produsul curent. Importul 1C nu echivalează cu un import bancar
  operațional general. (9, 21, 31f)

## Fiscalitate și nomenclatoare

- `SDD`/`SFD` nu au cod SAF-T de achiziție; `N9` poartă codul de livrare al
  rândului 10.1 în timp ce maparea D300 îl pune pe rândul 11. (84-r1, 84-r2)

- D300 și D394 sunt proiecții pentru cazurile implementate, nu acoperirea
  integrală a formularelor. Nu există export XML D300/D394. (69-r6, D4-r13)
- Prorata, ajustările, cazurile fiscale speciale și toate secțiunile
  informative D394 nu sunt implementate integral. Datele neacoperite rămân
  explicite în avertismente și în sumele neincluse. (36f, 69-r2, D4-r7)
- Marcajele ANAF pentru TVA la încasare nu constituie implementarea întregului
  mecanism contabil. Fluxurile de exigibilitate amânată și conturile
  intermediare aferente nu sunt acoperite complet. (36f, D4-r9)
- Rotunjirea TVA pe grup fiscal document × cotă nu este un mecanism general.
  Registrul păstrează valorile fiscale, dar nu un snapshot complet al
  identității și clasificării istorice a partenerului. (36f, D4-r1)
- Implicitele TVA pentru cumpărări extra-UE și anumite cumpărări de la
  neînregistrați nu au politici distincte în seed-ul privat. Fallback-ul la
  ancora documentului poate cere alegere explicită la culegere. (81-r1)
- SAF-T acoperă profilurile L și S implementate și testate. Nu acoperă toate
  variantele de contribuabil, multivaluta, proprietarii terți ai stocului și
  toate mișcările logistice. Validarea XML nu dovedește completitudinea
  datelor economice. (73-r2, 73-r16, 74-r1)
- Nu există catalog NC complet și nici conversie automată sigură pentru
  orice text UM legacy. Precizia câmpurilor de preț exportate trebuie
  distinsă de precizia internă a prețului și de cea a cantității. (73-r4, 73-r5, 73-r11)
- Nu există integrare operațională e-Factura.

## ANAF și configurare

- Limitarea ritmului ANAF este pe cerere; nu există coordonare globală între
  cereri simultane. (72-r1)
- Proveniența adresei nu este păstrată separat pentru fiecare componentă.
  Completarea golurilor poate produce o adresă cu surse mixte. (72-r2)
- Nu toate informațiile ANAF despre înregistrare/radiere sunt consumate în
  domeniu. Fluxul individual poate păstra o salvare efectuată înaintea unei
  erori ulterioare; mesajul transportului nu este dovada unui rollback global. (72-r3, 72-r4)
- Sincronizarea ANAF în lot este disponibilă prin API, fără ecran React
  dedicat pentru întregul flux. (77-r3)
- În formularul regulii de contare, dimensiunile `Unitate*` sunt doar de
  citit: setul OData `Unitate` nu are controller. Întoarcerea din panoul
  „Explică" în grilă nu focalizează rândul. (84-r4)
- Captions: `SursaCont` și mai mulți membri ai politicilor nu au
  `[XafDisplayName]`; ecranele poartă caption-ul în cod. (84-r5)
- Selectoarele XAF de tip TVA nu filtrează încă `Activ`; unele grile de
  politici afișează FK-uri brute și nu au configurarea vizuală completă. (81k, 81-r7)
- Explicația e a configurației pe o linie ipotetică, nu planul unui
  document; gate-ul ei de citire e pe tip, nu pe obiect, iar codul de tip se
  rezolvă înaintea gate-ului (400 pe cod inexistent pentru orice rol). Nu
  există import/export general al configurației. Raportul de profil nu are
  categoria „rând de seed lipsă"; recrearea unui rând de seed șters rămâne
  manuală. Marcajul istoric nu distinge toate intervențiile manuale
  anterioare, iar alinierea atinge acum și `Cont.DimensiuniObligatorii` pe
  rândurile marcate. (81k, 81-r3, 83-r1, 84-r3, 84-r6, 84-r8)
- Rolul `Configurator` e al release-ului: permisiunile lui se reaplică la
  fiecare seed, iar pe RELEASE nu există un utilizator cu acest rol până nu îl
  atribuie administratorul; navigația XAF nu e configurată. (83-r5, 84-r7)
- Administrarea perioadelor fiscale nu are un flux React complet. Conturile
  sunt expuse prin OData pentru citire, nu prin editor contabil general. (53i, 79-r3, 70g)

## Client, API și livrare

- Cheia cache-ului `byKey` combină proiecțiile de selecție și expandare într-o
  formă care nu distinge toate combinațiile posibile. Nu se presupune că
  orice proiecție nouă este sigură fără verificarea cheii. (77b)
- Invalidarea cache-ului nu garantează reîncărcarea imediată a tuturor
  widgeturilor deja montate. Selectoarele pot necesita reîmprospătare. (77-r5)
- La unele citiri OData, transportul DevExtreme reduce eroarea la status și
  pierde mesajele `Erori`. Unele mesaje de transport rămân în engleză. (80-r1, 70-r5, 81-r9)
- Refuzurile asupra navigațiilor OData expandate nu au acoperire completă
  prin probe HTTP pe roluri restrânse. (80-r4)
- Calculul ASM `distribuie-valoarea` citește prețuri prin context nesecurizat
  fără un drept separat de citire pe Lot. Autorizarea documentului nu
  echivalează cu această permisiune asupra nomenclatorului. (80-r7)
- Controllerul de rapoarte rămas din scaffold poate întoarce 404 cu corp gol,
  în afara formei comune `Erori`. (80-r3)
- Metadata OData descrie modelul expus și nu este filtrată ca o listă de
  înregistrări după permisiunile utilizatorului. (55g)
- În XAF Blazor niciun view nu folosește `InstantFeedback`/
  `InstantFeedbackView`; activarea cere o măsurătoare peste prag și probe în
  browser. (85-r2)
- Proprietățile nemapate ale documentelor (`Total`, valorile de
  livrare/recepție) sunt disponibile în DetailView, nu în liste; o altă
  proprietate nemapată care ar ajunge într-o listă este refuzată de ModelCheck.
  Mărimea paginii grilelor nu este calibrată; `RegulaStoc_ListView` poartă un
  override `Server` redundant cu opțiunea aplicației. (85-r3, 85-r4, 85-r5)
- În modul `Server`, referințele coloanelor ascunse rămân în interogarea
  paginii (FCT: 33 de join-uri); scoaterea lor din modelul view-ului nu e
  făcută. Gruparea încarcă primele rânduri ale fiecărui grup, iar `Refresh`
  execută pagina de două ori. (85-r6, 85-r7, 85-r8)
- Un layout salvat de utilizator poate ascunde toate coloanele unui view; pe
  `ServerView` celulele rămân goale până la Refresh după re-bifarea
  coloanelor. Grila Detalii a unei facturi importate poate arăta coloana
  `Produs` goală (neverificat dacă e de date sau de afișare). (85-r9, 85-r10)
- Găzduirea clientului și API-ului pe aceeași origine este contractul de
  livrare. Hostul are `UseStaticFiles`, dar proiectul WebApi nu include
  copierea automată a build-ului React și fallback-ul rutelor SPA.
  Publicarea cere și configurarea JWT pentru mediul țintă. (43e, 55g)
- Reluarea parțială a importului cu `--continua` poate afecta interpretarea
  reconcilierii. Validarea completă cere o bază și un set de referință
  controlate, cu starea reluării cunoscută. (75-r3)
