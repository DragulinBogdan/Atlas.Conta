# Limite curente

**Actualizat: 2026-10-03.** [Index](README.md)

Această pagină delimitează implementarea disponibilă. Elementele de aici nu
sunt angajamente de livrare și nu descriu o ordine de implementare.

## Domeniu și operare

- PIF/AMO/CAS scriu cubul, iar situația fișei se citește din el (095, 097).
  Storno
  rămâne limitat la luna documentului (087g), inclusiv în corecție.
  Nominalizarea și inversarea suportului folosesc un blocaj tranzacțional
  comun pe bază; corectitudinea concurentă este probată, debitul concurent
  pe volum mare nu este măsurat. Perf 2/51 privește operandul, nu întregul
  flux de validare al frunzelor. Referințele la suport sunt păstrate în
  istoricul inversat chiar dacă suportul eliberat este ulterior anulat;
  auditul durabil după anularea fizică rămâne în delimitarea TR-D9 (091j).

- Deschiderea generică (094) este o comandă de motor și scrie numai cubul.
  Registrele regimului dual nu primesc aceste solduri; cititorii și
  evaluarea le iau din cub (X-D2). Intrarea păstrează analiza și valuta, dar stingerea
  unei partide inițiale în valută este refuzată explicit până la TR-D9;
  soldurile nedetaliate nu se împart pe mai multe analize în aceeași cheie
  de control. Ștergerea concurentă a nomenclatoarelor referite nu este
  serializată de comandă (review Deschidere, MINOR-3).
  Stingerea prin motor a unei partide inițiale în lei și
  inversarea ei prin storno PLT sunt probate; nu există încă o ușă UI/HTTP
  pentru această comandă. Conectorul 1C rămâne înghețat (091-r4).
- Serializarea operațiilor concurente și reluarea idempotentă generală a
  comenzilor nu sunt acoperite complet. Validarea într-o singură operație
  nu dovedește protecția față de două comenzi simultane. (25f, 42f)
- Baza nu verifică tipul unui rând: discriminatorul `ClrType` al documentelor
  nu are FK spre `TipDocument.ClrType`, iar un FK spre o frunză (de exemplu
  `Lot.GestiuneId`) acceptă în schemă id-ul oricărui repartitor. Pe ușa
  securizată tipul îl verifică gardianul; pe ușa de sistem (Import1C, seed,
  Migrare) nimic nu-l verifică la scriere, iar abaterile le găsesc doar
  probele ModelCheck și SQL-ul din `--dump-integritate-tph` rulat după
  import. (89a, 89e)
- Închiderea unei perioade și operarea unui document în ea sunt serializate
  numai pe căile care trec prin adaptorul de operare și prin comenzile de
  generare ale închiderii de TVA și ale amortizării. Uneltele standalone —
  ModelCheck, Import1C, Migrare — pot chema motorul direct; acesta asigură
  acum o tranzacție dacă apelantul nu are deja una (097). Aceasta nu adaugă
  implicit blocajul de închidere al adaptorului API pe toate ușile. (F27-D1)
- Balanța, balanța pe plan, fișa de cont, soldul de stoc, soldurile pe loturi,
  soldul unei chei, alocarea FIFO, gardianul de sold negativ și soldurile de
  TVA ale închiderii lunare pornesc de la ultima perioadă de referință. Trei
  citiri nu folosesc snapshot: situația imobilizărilor citește cubul integral
  al fișelor cerute (097); oracolul golirilor, care
  citește rânduri concrete, nu solduri; împerecherile și restul documentelor,
  până la felia care le datează. (F27-D3)
- Inițialul de stoc al SAF-T citește snapshot-ul de loturi al cubului
  (105; F27-r10 închisă). Inițialul de CONT al SAF-T recitește postările pe
  spațiul securizat, nu pornește din snapshot. (SAFT-r4, X-D5)
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
  nu vede perioadele cade pe citirea integrală a cubului. (F27-D3)
- Integritatea snapshot-ului nu e verificată la citire. Un rând de snapshot
  șters direct din bază dă o balanță tăcut greșită (inițialul scade), fără
  niciun semnal; singura detecție e reconstrucția la cerere, care raportează
  diferența și repară. F27-r12 propune memorarea numărului de rânduri și a
  sumelor scrise la închidere, verificate ca o constatare de închidere, și
  refuzul unei referințe goale cu număr memorat pozitiv. (review advers F27, 9)
- Compatibilizarea vechilor repere `RegistruTva.ScrisLa` nu este implementată:
  citirea fiscală folosește cubul greenfield și confirmarea explicită a
  depunerii; bazele de dezvoltare se recreează conform 102. (103)
- Snapshot-urile de referință se citesc fără filtrarea de securitate pe rând.
  Fișa de cont își păstrează gate-ul strict (echivalența celor două căi,
  numărată pe TOT istoricul contului, nu doar pe fereastra de după referință),
  deci un utilizator restrâns pe rânduri primește 403 ca înainte; balanța și
  soldul de stoc trec prin LINQ, unde snapshot-ul e o entitate ca oricare
  alta, iar o restricție pe rând pusă pe registru NU se propagă asupra lui.
  (F27-D3, 66)
- Reconcilierea cu sursa externă 1C NU e constatare de închidere: documentele
  neimportate într-o perioadă nu opresc și nu semnalează închiderea ei. E
  treaba conectorului, nu a mecanismului. (F27-D2, F27-r6)
- Constatarea de rest scadent citește doar documentele cu scadență culeasă sau
  implicită — facturile de intrare și de ieșire. Un document stins fără
  scadență (plată, încasare, decont, retur) rămâne în afara ei chiar cu rest.
  (F27-D2)
- Constatările de închidere listează cel mult 200 de rânduri per fel, iar
  restul intră într-un rând de rezumat cu cheie proprie, la severitatea
  familiei, care spune câte rânduri acoperă: acceptarea lui e o acceptare ÎN
  BLOC a celor nelistate, nu una pe constatări individuale. Pe o lună cu peste
  200 de rânduri de același fel operatorul vede deci cifra, nu lista.
  (F27-D2)
- Constatările de conținut se caută doar când niciun blocant STRUCTURAL nu
  stă în picioare: pe o lună cu precedenta deschisă ecranul arată doar
  blocanta lanțului, nu și ce ar mai fi de rezolvat în ea. (F27-D2)
- Dialogul de închidere din XAF Blazor arată constatările ca listă imbricată cu
  bifă. Bifa pe o constatare blocantă e dezactivată prin regulă de aspect;
  autoritatea rămâne serviciul, care refuză blocantele indiferent de ce
  s-a trimis. (F27-D2)
- Două împerecheri noi, încă necomise în același context, au o limită de
  vizibilitate în calculele bazate pe interogarea bazei. Plafoanele nu trebuie
  prezentate ca protecție completă pentru orice lot de scrieri concurente. (41d)
- Capacitatea NTC este netată pe contrapartidă și latură, nu pe cont.
  Calculul restului scade conservator legăturile existente în ambele roluri. (76-r1, 76-r2)
- Ieșirea fiscală RLF poate lăsa un reziduu valoric pe cheia de stoc golită.
  Documentele retroactive nu reevaluează ieșirile deja operate. (75a)
- Unele distribuiri ASM nu sunt reprezentabile exact la precizia prețului.
  Cazurile refuzate nu sunt corectate prin prețuri sau valori forțate. (75-r4, 76c)
- Regimul dual evaluează încă ieșirile cubului pe soldul registrelor: trei
  BCS/ASM de câte 1 din 3/10 lasă în cub 0/−0,01; un produs ASM ajustat prin
  Δ poate păstra 0/+0,01 după consum. Sunt probe exacte ale limitei curente,
  fără toleranță; ținta rămâne 0/0. T-r13 cere atât evaluarea pe propriul sold
  complet, cât și tratarea explicită a istoricului divergent. (ASM-B7)
- Diagnosticul valoric citește întreg istoricul semnat pe lot/gestiune/cont,
  cu proveniență. Compară stocul Magazie/Marfuri/Folosinta; alte tipuri de registru și
  postările cubului identificate în afara domeniului sunt numărate separat.
  Postările fără corespondent rămân istoric incomplet, nu sunt excluse.
  Contul registrelor este rezolvat din nomenclatorul curent,
  fără a pretinde un snapshot istoric. Diferențele complete rămân neexplicate
  până la certificarea cauzei; lipsa unei corespondențe se raportează separat.
  Nu deduce automat N-r3 din tipul documentului. (ASM-B7, T-r13)
- Folosința păstrează gestiunea reală pe lanțul FCT/NIR/BTR/BCS/LDI.
  Istoricul pe alt TipStoc nu se mută și nu alimentează noua cheie prin
  fallback. Diagnosticul listează separat lotul, gestiunea, TipStoc,
  documentul și stornoul pentru loturile cu mișcări Folosinta ori din clase
  acoperite de o politică Folosinta curentă; aceasta este evidență pentru
  verificare, nu o reclasificare a istoricului. Custodie rămâne refuzată la
  LDI; Gratuit și injectivitatea SAF-T rămân TR-r7. (093, LDI-B3)
- Reconcilierea contabilă (a) exclude nominal Operare ASM în regimul dual;
  (h) păstrează numărul și diferențele vizibile, fără efect asupra exit-ului.
  Pentru ASM, comparația cu registrele nu mai detectează erori valorice:
  probele numerice independente sunt obligatorii, conservarea singură nu
  ajunge. Cititorii contabili includ efectul ASM. Diagnosticul valoric pe
  lot rămâne raport. (D8-B4 aprobat, T-D10, T-r15)
- Recepția unui NIR nu declanșează automat completarea DSC pentru facturile
  cu acoperire parțială. Există comanda de generare suplimentară pe FCL. (37g, 38d)
- NIR manual postează pe cub la net, fără fapt fiscal; nu acoperă avizul pe
  408 sau factura ulterioară pe un lot recepționat anterior (TR-r4/B-r5).
  NIR conex sursei migrate rămâne necesar registrelor până la TR-D9 (B-r3).
  Excluderea se aplică numai cu Autogenerat, sursă și PoliticaConex potrivită,
  cu sursa PosteazaInCub; simpla legătură cu o sursă nu este suficientă.
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
- D300/D394 au atribuiri distincte. Confirmarea depunerii din aplicație
  păstrează formularul/perioada, amprenta faptelor exportate și momentul confirmării;
  nu transmite declarația la ANAF și nu verifică recipisa. Indicatorul
  D394 de rectificativă este disponibil pentru exact o lună calendaristică;
  D300 folosește regularizări. `TaxInformation` SAF-T citește faptele pe luna
  exportului contabil. Ruta publică L citește cubul (S1, S2); validarea
  XSD/DUK (S0) certifică GL-ul, facturile și plățile lunilor scenei.
  Compensarea prin notă contabilă nu apare în Payments (SAFT-r1). Valuta și
  TVA la încasare pe plăți nu există în cub, iar exportul le refuză. Factura
  în valută se declară în RON, cu avertismentul `FacturaInValuta` (B-r6). Citirea
  alocărilor filtrează postările contabile pe `Unitate`, fără index dedicat.
  Pe scara sintetică (SAF-B8: k ∈ {1, 4, 16, 64} unități/lună × m ∈ {0, 6, 12}
  luni de istoric, proces rece și cald) L emite 43 de comenzi SQL și S 26,
  constant. Istoricul nu se citește din `Postare`. Durata și alocările cresc
  liniar, iar execuția maximă pe server la k = 64 este 5,2 ms. Planul pe volum
  real rămâne nemăsurat, fiindcă nu există bază de volum după C102 (FZ-r3).
  După o inserare masivă, până la autoanalyze, citirea faptelor fiscale poate
  degenera în Nested Loop (SAFT-r5). Pragul de ~43 ms văzut pe mașina de
  dezvoltare aparține proxy-ului de porturi Docker Desktop, nu produsului.
  Lotul pe două conturi de stoc nu are azi producător; dacă apare, poziția
  îl separă prin `ProductType`, iar garda de injectivitate refuză cheile
  duplicate (SC-SAFT-12 parțial). Balanța SAF-T recitește postările (SAFT-r4).
  Validatorul se rulează numai pe lună întreagă (antetul cu o singură lună).
  (103, D8-B8, S1-R8, S0-R4, S2)
- Corecția unui document operat: motivul decide efectul fiscal, nu contarea.
  Reclasificarea pe 1174 a erorilor semnificative din exerciții anterioare
  rămâne notă contabilă manuală — motorul nu judecă semnificația. (F27-r1)
- Corecția moștenește integral gardienii stornării: un document cu împerecheri
  în fereastra DESCHISĂ cere ștergerea lor înainte (cele dintr-o perioadă
  închisă se inversează singure la storno), iar un document cu conex operat sau
  cu latură pereche operată e refuzat exact ca la storno. Comanda nu relaxează
  nimic. (F27-D6, F27-D8)
- Partidele deschise nu poartă scadența și nu se grupează pe vechime:
  scadențarul și aging-ul sunt aceeași listă cu scadența alături, felie de
  raportare proprie. (F27-D7, F27-r2)
- Soldul partenerului nu apare în lookup-urile de partener din culegere:
  există ca ecran și ca rută, nu ca o coloană `Sold` pe `Partener`. (F27-r7)
- `sold-parteneri` grupează pe dimensiunea **Repartitor**, care urmează
  laturile documentului (debit←predator, credit←primitor — 00 §5), NU contul
  de terț: pe o factură de client atomul de debit al lui 4111 poartă
  emitentul, iar clientul apare pe atomul de credit al venitului (măsurat pe
  baza Privat: 103.301 din 108.912 rânduri de 4111 au „Sediul central" pe
  debit). Ecranul dă deci soldul pe cheia contabilă așa cum e ea, nu creanța
  per partener; creanța per partener se citește din partidele deschise
  (`documente-cu-rest`). Dimensionarea contului de terț pe partener e decizie
  separată, nu a acestei felii. (F27-D7)
- `ReturClient` intră în proiecția de rest, dar rândurile lui nu apar:
  creanța unui retur e negativă după operare (venit stornat), iar filtrul
  `Rest > 0` o taie. Împerecherea unui retur rămâne pe calea directă
  (serviciu / XAF). (F27-D7)
- `Imperechere` rămâne legătura explicită: panoul stingerilor îi afișează
  rândurile, iar restul, totalul și candidații vin din `Cub.Citiri.Partide`.
  Singurul calcul care citește sumele legăturilor este
  `Partide.NominalizataLibera`, pe o pereche de documente (101, X-D2).
- Proba X-D2 este sintactică. Un rezultat netipizat derivat dintr-un registru
  se urmărește la apelanți numai dacă membrul e declarat în `Purtatori`; un
  purtător nou, nedeclarat, nu e văzut. `StocService.Sold`, `AlocaFifoTolerant`
  și `AlocaFifo` nu mai au apelant de producție și rămân în `Module` numai ca
  oracol al probelor, până la TR-D9.
- Cifrele citite din cub sunt păzite de dreptul de citire pe tipul registrului
  (`RegistruContabil` pentru închiderea de TVA, `RegistruImobilizari` pentru
  fișă și amortizare). Subiectul permisiunii după tăierea registrelor nu este
  ales. (F22-D5, X-D2; TR-D9)
- `documente-cu-rest` rămâne proiecția scumpă, iar partidele nu schimbă asta:
  pe baza de import costă 181 ms filtrat pe o contrapartidă (171 ms cu lanțul
  desfăcut — diferență în zgomot), 423 ms nefiltrat pe grilă și 220 ms pe
  forma nefiltrată fără paginare. Cauza e cea de la 59 — uniunea tuturor
  documentelor operate ale celor opt tipuri, cu filtrul abia în `WHERE`-ul
  exterior — plus legăturile ca tabele derivate: agregatul `Imperecheri` intră
  prin `Nested Loop Left Join` cu două `Seq Scan` și 2,18 M de rânduri respinse.
  Corelarea legăturii ELIMINĂ `Seq Scan`-urile și duce panoul filtrat la 82 ms,
  dar mută costul pe calea nefiltrată neplafonată (220 ms → 1,02 s), pe care o
  consumă constatarea de rest scadent la închidere — deci nu e fixul; cifrele
  și variantele respinse sunt în `docs/api/p5-perf-masuratori.md` §Felia 27.
  Niciun index nu lipsește. (F27-r16)
- Ținta de 150 ms a lui `documente-cu-rest` a fost calibrată pe baza `Privat`
  (147 ms la pasul 6, 131 ms la re-măsurare); cele 181 ms sunt de pe baza de
  import, alt set de date. Pe baza pe care a fost pusă, ținta nu e încălcată.
- Fișa de cont și balanța analitică coboară cu închiderile (187 → 122 ms,
  respectiv 254 → 210 ms, A/B pe aceeași bază), dar rămân peste ținta de
  100 ms. La fișă costul nu mai e proiecția — 7 ms —, ci cadrul unei cereri
  (58 de instrucțiuni de securitate per ObjectSpace, hidratare, serializare),
  deci ținta se ratează din afara feliei (F27-r14). La balanța analitică e
  cardinalitatea cheii `Cont×Repartitor` (71.167 de grupe); ridicarea lui
  `work_mem` NU ajută, o înrăutățește (136 ms la 4 MB cu agregare paralelă și
  sortare pe disc, 183 ms la 64 MB cu hash aggregate secvențial) — ținta e de
  re-calibrat (F27-r15). (F27-D3)
- Balanța analitică pe decembrie are 71.167 de rânduri cu snapshot și 72.910
  fără: cheile cu debit ȘI credit cumulat zero nu se scriu în snapshot.
  (F27-D3)
- Coloana „Dată” a împerecherii apare în panoul de stingeri și în lista XAF,
  dar nu există listă proprie de împerecheri în clientul React: desfacerea se
  face din panoul documentului. (F27-D8)
- Legătura de corecție nu apare în coloanele listelor de documente și nu se
  poate filtra pe ea: se vede pe ecranul documentului, ca bandă, și în grupul
  „Corecție" al DetailView-ului XAF. (F27-D6)
- Data înregistrării: documentele generate
  (AMO, ITV, DSC și NIR autogenerat) moștenesc data înregistrării sursei sau
  ultima zi a lunii; editabilitatea ei pe ele nu e decisă. Plata autogenerată
  din factura de intrare face excepție: data ei este cea culeasă, iar data
  înregistrării i se normalizează la ea, nu la a facturii. Coloana „Data
  înregistrării” lipsește din coloanele listelor de documente ale clientului
  React și din DTO-urile lor de listă. (F27-D4, F27-D5, F27-r9)
- Salariile, execuția bugetară completă, producția pe rețete, împărțirea pe
  cofinanțări și contabilitatea multivalutară nu sunt module complete în
  produsul curent. Importul 1C nu echivalează cu un import bancar
  operațional general. (9, 21, 31f)

## Fiscalitate și nomenclatoare

- `SDD`/`SFD` nu au cod SAF-T de achiziție; `N9` poartă codul de livrare al
  rândului 10.1 în timp ce maparea D300 îl pune pe rândul 11. (84-r1, 84-r2)

- R6 verifică intervale configurate, nu toate condițiile legale ale unei cote.
  Proveniența fiscală a ajustărilor RDC/RLF/reduceri rămâne neacoperită:
  diagnosticul nu deduce factura fiscală din lot. Nu ține consumul/restul
  avansului și nu refuză depășirea lui; referințele parțiale repetate sunt
  permise. Nu repară automat taxele sau calificările istorice. (103-r2)
- Pe FCT/FCL, baza negativă și taxa culeasă pozitivă sunt încă admise în
  L3; abaterea este diagnosticată aritmetic. Un refuz pentru semne
  incompatibile și înlocuirea lui `ILinieCuAvans` ca proxy pentru linia de
  factură rămân de tratat la următoarea atingere a validării. (103-r3)
- Raportul R6 are costuri de măsurat la gate-ul transversal de performanță:
  acces leneș la `Tranzactie` și verificări de securitate per postare,
  rezolvarea tipului/politicii per draft, citire fără limită temporală când
  perioada lipsește și self-join pe postările fiscale la deschiderea
  lookup-ului de avans. Nu există încă un buget de cost probat. (103h; gate-ul transversal TR-D8, 090/091)
- Implicitele Privat `FCT|RLF/UE → TI21` și `DVI → IMP21` sunt nedatate.
  Înainte de 2025-08-01, tipurile nu sunt eligibile; rămâne alegerea
  explicită cu explicație conform R6-B5. Adăugarea rândurilor istorice
  datate în seed cere alegerea owner-ului. (103h/i, R6-B5)
- D300 și D394 sunt proiecții pentru cazurile implementate, nu acoperirea
  integrală a formularelor. Nu există export XML D300/D394. (69-r6, D4-r13)
- Prorata, ajustările, cazurile fiscale speciale și toate secțiunile
  informative D394 nu sunt implementate integral. Datele neacoperite rămân
  explicite în avertismente și în sumele neincluse. (36f, 69-r2, D4-r7)
- Marcajele ANAF pentru TVA la încasare nu constituie implementarea întregului
  mecanism contabil. Fluxurile de exigibilitate amânată și conturile
  intermediare aferente nu sunt acoperite complet. (36f, D4-r9)
- Rotunjirea TVA pe grup fiscal document × cotă nu este un mecanism general.
  Cubul păstrează valorile și calificarea fiscală, dar nu un snapshot complet al
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

- Paginile React de detaliu ale documentelor sunt înghețate: câmpurile
  adăugate modelului după 2026-09-27 nu apar acolo, iar convențiile de
  selector rămase neadoptate (BTR, `TipMaterial`) nu se mai aplică. Culegerea
  curentă e în XAF Blazor. Scoaterea sau generarea lor din metadate se decide
  după PoC. (104d, 104-r4)
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
  paginii (FCT: 33 de join-uri măsurate pe maparea TPT; pe TPH join-urile
  moștenirii au dispărut, iar navigațiile coloanelor ascunse rămân — 89);
  scoaterea lor din modelul view-ului nu e făcută. Gruparea încarcă primele rânduri ale fiecărui grup, iar `Refresh`
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
