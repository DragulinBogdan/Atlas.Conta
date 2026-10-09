# Limite curente

**Actualizat: 2026-10-09.** [Index](README.md)

Această pagină delimitează implementarea disponibilă. Elementele de aici nu
sunt angajamente de livrare și nu descriu o ordine de implementare.

## Domeniu și operare

- Analiza obligatorie per cont: repartitorul e al capătului (D9-A10);
  partenerul stă pe piciorul de terț numai când contul cere repartitor sau
  urmărește partide — pe un cont fără flag și fără partide (446 privat)
  partenerul rămâne absent, iar partenerul pe conturile de cheltuieli și
  venituri e al deciziei 111. Pe piciorul intern cu fapt fiscal `Partener` e
  partenerul fiscal (B-D8 pct. 5), deci `Contabil.Repartitor` îl întoarce pe
  el înaintea gestiunii; separarea e a deciziei 111. Pe trezorerie, când
  regula de contare numește ambele conturi explicit, piciorul propriu n-are
  gestiune (B-D8 pct. 9 o ia din `SursaCont`); dacă contul lui cere
  repartitor documentul e refuzat (SC-PLT-09), nu reparat cu terțul.
  Cerința de material citește numai produsul lotului, nu și materialul fix al
  unei reguli de contare. BTR, ASM și PIF nu sunt păzite (D9-r1).
  (`docs/nucleu/tr-d9-pas6b-repartitor.md`; `tr-d9-pas2-probe.md`, G2)
- PIF/AMO/CAS scriu cubul, iar situația fișei se citește din el (095, 097).
  Storno
  rămâne limitat la luna documentului (087g), inclusiv în corecție.
  Nominalizarea și inversarea suportului folosesc un blocaj tranzacțional
  comun pe bază; corectitudinea concurentă este probată, debitul concurent
  pe volum mare nu este măsurat. Perf 2/51 privește operandul, nu întregul
  flux de validare al frunzelor. Referințele la suport sunt păstrate în
  istoricul inversat chiar dacă suportul eliberat este ulterior anulat;
  auditul durabil după anularea fizică rămâne în delimitarea TR-D9 (091j).

- Deschiderea generică (094) este o comandă de motor și scrie cubul; cititorii
  și evaluarea iau soldurile de acolo (X-D2). Intrarea păstrează analiza și
  valuta, dar stingerea unei partide inițiale în valută este refuzată explicit
  până la TR-D9b;
  soldurile nedetaliate nu se împart pe mai multe analize în aceeași cheie
  de control. Ștergerea concurentă a nomenclatoarelor referite nu este
  serializată de comandă (review Deschidere, MINOR-3).
  Stingerea prin motor a unei partide inițiale în lei și
  inversarea ei prin storno PLT sunt probate; nu există încă o ușă UI/HTTP
  pentru această comandă. Conectorul 1C a ieșit din îngheț numai pe deschidere
  și pe trecerea 2 a lunii (107): deschiderea trece prin această comandă, iar
  stingerile anului pe partide inițiale se scriu în cub; fără deschiderea în
  cub Import1C nu mai rulează pe modelul cu stocul citit din cub (107 j).
  Stingerile prin facturi rămân în afara trecerii 2 (107-r1), pozițiile în
  valută intră în lei și se plafonează la rest (107-r4).
- Comenzile care scriu sunt seriale per bază (X-D6): doi operatori nu scriu
  simultan, al doilea așteaptă comanda primului, oricât de străine ar fi
  documentele lor. Așteaptă și salvarea unui draft care adaugă detalii fără
  poziție. Pe scara sintetică (X-D5) o comandă durează 19–39 ms la orice
  istoric, o unitate de 11–12 comenzi sub 300 ms, un consum de 64 de linii
  110 ms și o închidere de lună 79 ms; debitul cu mai mulți operatori nu e
  măsurat. Rafinarea blocajului pe gestiune și partener e restanța X-r1. Peste timpul
  de comandă al conexiunii (30 s implicit), așteptarea iese ca refuz
  `SCRIERE_OCUPATA`; pragul nu e configurat separat de timpul comenzilor.
- Reluarea idempotentă generală a comenzilor nu este acoperită. (42f)
- Blocajul scrierii îl ia `TranzactieComanda`. O unealtă care își deschide
  singură tranzacția (`Database.BeginTransaction`) și cheamă motorul nu îl
  are; proba pe sursă refuză asta în `Module`, WebApi și Blazor, nu și în
  uneltele standalone. Deschiderea și stingerea ei îl iau oricum, la intrare.
- Baza nu verifică tipul unui rând: discriminatorul `ClrType` al documentelor
  nu are FK spre `TipDocument.ClrType`, iar un FK spre o frunză (de exemplu
  `Lot.GestiuneId`) acceptă în schemă id-ul oricărui repartitor. Pe ușa
  securizată tipul îl verifică gardianul; pe ușa de sistem (Import1C, seed)
  nimic nu-l verifică la scriere, iar abaterile le găsesc doar
  probele ModelCheck și SQL-ul din `--dump-integritate-tph` rulat după
  import. (89a, 89e)
- Închiderea unei perioade și operarea unui document în ea sunt serializate
  pe toate căile care deschid tranzacția prin `TranzactieComanda`: adaptorul
  de operare, comenzile de generare, și motorul chemat direct de uneltele
  standalone, care își asigură tranzacția dacă apelantul nu are una (097).
  (F27-D1, X-D6)
- Balanța, balanța pe plan, fișa de cont, soldul de stoc, soldurile pe loturi,
  soldul unei chei, alocarea FIFO, gardianul de sold negativ și soldurile de
  TVA ale închiderii lunare pornesc de la ultima perioadă de referință. Trei
  citiri nu folosesc snapshot: situația imobilizărilor citește cubul integral
  al fișelor cerute (097); oracolul golirilor, care
  citește rânduri concrete, nu solduri; împerecherile și restul documentelor,
  până la felia care le datează. (F27-D3)
- Inițialul de stoc și cel de cont ale SAF-T, ca și soldurile terților, pornesc
  din snapshot, sub accesul complet verificat înaintea proiecției (105,
  SAFT-r4 închisă, X-D5). Rapoartele API cumulate (balanță, fișă, sold
  parteneri, partide cu rest, stoc) citesc `Vizibila` și recitesc tot
  istoricul la fiecare cerere: pe scara sintetică, 7.827 de rânduri din
  `Postare` pentru balanța de după 12 luni, față de 1.926 din snapshot. La acel
  volum recitirea e mai rapidă (8 ms față de 13 ms), deci nu există un al
  doilea read model. (104b, FZ-r1 închisă, X-D5)
- Raportul de stoc listează și capătul de consum al bonului: postarea de debit
  a BCS poartă lotul ca unitate, pe contul de cheltuială și la locul de
  consum, iar `Loturi.Postari` o ia ca poziție. Rândurile acestea cresc cu tot
  ce s-a consumat, și în raport, și în snapshot-ul de stoc. (X-D5; X-r3)
- `PartideCuRest` caută documentul deschizător al fiecărei partide parcurgând
  toate postările de partidă (`Partide.Origini`), și când soldurile vin din
  snapshot. E singurul criteriu de formă picat al scării transversale:
  rândurile atinse cresc cu istoricul. Amânat prin amendamentul owner-ului
  (2026-10-04), odată cu F27-r16, și re-amânat cu cifră la TR-D9a
  (2026-10-06): parcurgerea e liniară, 2,7 s de SQL pentru 488.733 de
  partide cu rest la 5 milioane de postări. Rămâne până la decizia 111.
  (F27-r16, X-D5, D9-D10 (b))
- O egalitate între două surse pe coloane nulabile iese din EF cu ramură de
  nul, iar Postgres nu o poate folosi drept cheie de hash sau merge.
  `RAMURA-NUL` o prinde în tot SQL-ul pe care EF îl emite într-o rulare
  ModelCheck, deci numai pe citirile pe care o scenă le execută: un cititor
  neatins de nicio scenă nu e văzut. Proba citește forma SQL-ului, nu planul:
  cele cinci forme admise nominal sunt admise pe cheia selectivă de lângă ele,
  nu pe o cifră la volum. Un SQL scris de mână cu `IS NOT DISTINCT FROM` nu e
  căutat. `DocumenteCuRest` are forma corectată, dar nu are cifră la volum.
  (`docs/stare-curenta/dezvoltare-si-validare.md`, „Îmbinările pe chei
  nulabile ale cubului")
- Scara transversală este sintetică și mică: 7.951 de postări la 12 luni. Nu
  are prag absolut. Proba din plan se evaluează cu scanarea secvențială
  interzisă, fiindcă la acest volum planificatorul o alege legitim; planul
  ales e raportat alături. Pragul pe volum real rămâne al migrării. (X-D5,
  FZ-r3)
- Scara de volum (5 milioane de postări) e tot sintetică: scena scării
  transversale multiplicată în lățime, citită de utilizatorul administrativ,
  pe Postgres neconfigurat. Nu măsoară scrierea, închiderea de lună,
  cititorii fiscali și SAF-T, nici filtrele de rând ale unui rol restrâns.
  (D9-A1)
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
- Golirea lotului se decide la momentul operării. Un document retroactiv nu
  reevaluează ieșirile deja operate și poate lăsa reziduu valoric pe un lot
  fără cantitate; eliminarea lui e reevaluare (TR-D9b). O asamblare culeasă
  înaintea unei alte ieșiri din același lot poate fi refuzată la operare cu
  `ASAMBLARE_NEBALANSATA`, până la redistribuire. (D9-D3)
- Valoarea de pe linia unui draft cu ieșire evaluată din sold e estimarea
  `cantitate × preț de intrare`, nu promisiune; dry-run-ul nu întoarce
  valoarea decisă. Ea se vede pe linie după operare și în explicația
  tranzacției. (D9-D3, D9-D12)
- Folosința păstrează gestiunea reală pe lanțul FCT/NIR/BTR/BCS/LDI.
  Custodie rămâne refuzată la LDI; Gratuit și injectivitatea SAF-T rămân
  TR-r7. (093, LDI-B3)
- Nu există un al doilea scriitor cu care cubul să fie comparat: corectitudinea
  valorică o poartă catalogul de scenarii și invarianții interni ai cubului
  (echilibrul, conservarea transferului, explicația, taxa și valoarea liniei).
  Conservarea singură nu dovedește cifra. (D9-D7 a)
- Recepția unui NIR nu declanșează automat completarea DSC pentru facturile
  cu acoperire parțială. Există comanda de generare suplimentară pe FCL. (37g, 38d)
- NIR manual postează pe cub la net, fără fapt fiscal; nu acoperă avizul pe
  408 sau factura ulterioară pe un lot recepționat anterior (TR-r4/B-r5).
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
  alocărilor filtrează postările contabile pe `Unitate`, prin indexul
  `(Unitate, Data)` (X-D5).
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
  duplicate (SC-SAFT-12 parțial).
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
- `sold-parteneri` și balanța analitică grupează pe `AtomContabil.RepartitorId`
  = `Postare.Partener`: pe contul de terț e partenerul capătului (partida sau
  flag-ul `Repartitor`, D9-A10), pe piciorul intern cu fapt fiscal e
  partenerul fiscal, pe restul e nul; gestiunea e axă separată (`GestiuneId`).
  Creanța per partener se citește din partidele deschise
  (`documente-cu-rest`); unificarea repartitorului e a deciziei 111. (F27-D7, D9-A10)
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
  purtător nou, nedeclarat, nu e văzut.
- Lista de evidență a cubului (`PostareVizual`): eticheta lotului e codul
  produsului și data lotului, deci două loturi ale aceluiași produs din
  aceeași zi au aceeași etichetă; nu există cod de valută (`Postare.Valuta`
  n-are nomenclator, view-ul nu-l etichetează); rândul n-are navigație spre
  tranzacție (view fără referințe EF, respinse de owner); sortarea pe o
  coloană de cod scanează tot cubul (9,1 s la 5 milioane de postări, fără
  index), iar pe containerul Postgres de dezvoltare, cu `/dev/shm` de 64 MB,
  sortarea paralelă pică (`could not resize shared memory segment`); costul
  e raportat fără prag în `docs/nucleu/tr-d9-pas7-declaratii.md`. (D9-A12;
  2026-10-07)
- Cu pregătirea automată a comenzilor, citirile de sold pe o listă de loturi
  (`Fapte.SolduriLoturi`, `Loturi.VerificaSoldIntermediar`) primesc un plan
  generic de cinci ori mai lent (0,45 → 2,2 ms pe apel): cu limitele de dată
  necunoscute, planificatorul alege indexul pe dată în locul celui pe
  `(Unitate, Data)` și filtrează lotul după citire. Coborârea filtrului de
  loturi pe rând nu schimbă planul. Import1C câștigă totuși cu ea pornită;
  hosturile nu o au. (2026-10-09)
- Ordinea FIFO a partidelor inițiale născute în aceeași zi e ordinea din
  deschidere, purtată de ID-ul postării, nu de o coloană a ei. (116-r1)
- Loturile inițiale cu același produs, gestiune, cont și dată se departajează
  în FIFO pe identificatorul lotului, dat de cel care le creează. (116-r2)
- Reproductibilitatea Import1C e măsurată numai pe ianuarie 2025 (două
  rulări identice pe chei naturale); lunile următoare nu sunt măsurate.
  (116; 2026-10-09)
- Nominalizarea partidei pe linia de notă nu e în culegere: câmpurile sunt
  ascunse în XAF și lipsesc din API. (115-r1)
- Cheia de pereche (`Postare.Pereche`) nu acoperă transformările (consumul și
  produsul sunt corespondență de grup) și nu e citită de nimeni: fișa
  contului întoarce contrapartida nulă când tranzacția are mai multe conturi
  pe sensul opus (D9-r4). Stornoul cu mai multe surse decalează ordinalele
  surselor după prima, deci ordinalul stornoului nu e literal al
  originalului. (D9-A2; 2026-10-07)
- Import1C rulează pe cititorii cubului, dar reconcilierea lui nu e verde.
  Pe ianuarie 2025 (2026-10-07): 15.232 de documente, zero eșecuri de import,
  `INV-CUB` verde; contractul 1 are trei conturi fără explicație (371, 3028,
  303: nota-punte a reclasificării dublează ASM-ul care postează acum,
  110-r2), iar contractul 5 are 5 partide inițiale neexplicate, neatribuite.
  Sensibilitatea contractelor 1 și 3 nu mai are probă proprie (`--sabotaj` a
  ieșit), iar unealta n-are un mod de reconciliere fără scriere.
  (D9-D11; `docs/nucleu/tr-d9-pas8-inchiderea.md` §6)
- Refuzul de acces pe cifrele din cub e fraza generică de citire pe `Postare`:
  nu spune dacă lipsește dreptul pe tip sau dacă rolul are un criteriu de rând
  ori de membru. (D9-D9)
- Refuzurile de ștergere pentru lot, tip de TVA și fișă citesc cubul, dar
  textele lor numesc încă registrul. (D9-A6)
- O postare existentă nu se rescrie: garda e la salvare (`POSTARE_MODIFICATA`),
  pe starea `Modified` a entității, deci nu vede o actualizare făcută direct în
  SQL. Ștergerea postărilor la anulare rămâne permisă motorului. (D9-A5)
- Refuzul tipului fără politică pe profil (`TIP_FARA_DECLARATIE`) apare la
  dry-run și la operare, nu în regimul pe stare al documentului: butonul de
  operare rămâne activ pe un tip inert. (D9-D5)
- Produsul nu are o listare a postărilor pe lot sau pe partidă. Regula
  listării N-r8 e fixată pe intrările comune și probată pe ele; singura
  listare pe lot din producție este mișcarea de stoc SAF-T, care poartă
  eticheta felului. O fișă a lotului sau a partidei se construiește peste
  `Loturi.Postari`, respectiv `Partide.Postari`, fără filtru pe fel.
- Probele de concurență (X-D6) pun cele două comenzi în coada blocajului
  ținut de scenă și le lasă să ruleze în ordinea cozii. Dovedesc rezultatul
  serial și așteptarea pe conexiuni distincte; nu măsoară debitul. Pe ușa
  HTTP și pe contextul securizat e probat numai refuzul `SCRIERE_OCUPATA`
  și ce nu ia blocajul (`scriere-ocupata.py`), nu matricea rezultatelor
  seriale.
- `SCRIERE_OCUPATA` iese 422, ca orice refuz de domeniu. Un client nu îl
  deosebește de un refuz definitiv decât după cod. (108)
- Explicația deciziei (X-D4) există numai pe tranzacțiile scrise de un
  contract. Împerecherea, desfacerea, stingerea de deschidere și deschiderea
  nu au decizii de explicat și nu au explicație. Un document operat înaintea
  migrației n-ar avea explicație, iar invariantul l-ar refuza: baza se
  recreează (102b).
- Refuzul `EXPLICATIE_ACCES_INCOMPLET` se decide pe tip, nu pe tranzacția
  cerută: un criteriu de rând pe `Postare` sau pe o frunză de document refuză
  explicația oricărei tranzacții, inclusiv a uneia pe care criteriul n-o
  atinge. E prețul regulii „fără proiecție parțială” (SAF-D4).
- `DECLARATIE_INVALIDA` (S-r11) e probată pe coaja comenzii, cu o cantitate
  în afara scării ținută în memorie. Un declanșator persistabil pe ușa HTTP
  nu a fost construit; răspunsul ușii e cel al oricărui refuz de declarație
  (422 la operare, listă la dry-run), neprobat separat pentru acest cod.
- Explicația reține care rând de politică a decis și dacă s-a schimbat de la
  operare, nu și cum arăta atunci; nomenclatoarele care decid contarea
  (contul tipului de material, conturile tipului de TVA, steagurile contului)
  nu se rețin (D9-r3). Contorul e `OptimisticLockField`: nu-l mișcă o scriere
  SQL directă sau restaurarea bazei, iar un rând șters și recreat apare ca
  dispărut. Politica de amortizare se reține pe orice fișă atinsă care are
  rând, fără să spună care cont a venit din ea și care din cub. (D9-A8;
  2026-10-07)
- Explicația repetă câte două `ContRezolvat` și unitatea întreagă pe fiecare
  linie. Măsurat în X-D5: 907 octeți pe linie, 58.066 pentru un consum de 64
  de linii.
- Invariantul explicației nu recitește istoricul: verifică soldul citit al
  unei partide stinse numai ca prezență și ca plafon al alocărilor. Un sold
  citit alterat în sus trece. La fel, o valoare declarată e comparată cu
  postarea ei și cu sursa permisă declarantului, nu recalculată din linia
  sau recepția de origine. (X-RI1)
- Matricea `refuzuri.ps1` probează ușa explicației numai cu cei patru
  utilizatori ai ei: 200 și 404. 403 `EXPLICATIE_ACCES_INCOMPLET` cere un
  rol cu citire restricționată și rămâne al probei `explicatii.py`, care
  lasă un bon stornat pe baza ei. (X-D8)
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
- Regimul pe stare nu promite refuzurile care depind de valori (sold de
  stoc, poziție fără fișă) și, la stornare și corecție, pe cele care depind
  de data cerută: perioada ei, luna la imobilizări cât luna documentului e
  deschisă, nominalizările stinse ulterior. La stornarea unui document cu
  legături vii în perioade închise, regimul nu numără nominalizarea
  partenerului pe aceeași partidă când ea nu vine din transferul legăturii,
  inclusiv la legătura manuală care acoperă exact o nominalizare făcută la
  operare; refuzul `PARTIDA_CU_DEPENDENTI` rămas după desfacerea legăturilor
  apare la comandă. (106i, 106k, 106-r7)
- Salariile, execuția bugetară completă, producția pe rețete, împărțirea pe
  cofinanțări și contabilitatea multivalutară nu sunt module complete în
  produsul curent. Importul 1C nu echivalează cu un import bancar
  operațional general. (9, 21, 31f)

## Fiscalitate și nomenclatoare

- `SDD`/`SFD` nu au cod SAF-T de achiziție; `N9` poartă codul de livrare al
  rândului 10.1 în timp ce maparea D300 îl pune pe rândul 11. (84-r1, 84-r2)
- Acoperirea politicii de mișcări SAF-T (D17-V1) se probează într-o singură
  direcție: fiecare pereche tip × categorie pe care cubul o produce în scena
  SAF-T are politică, iar scena atinge cele opt tipuri care mișcă stocul
  (FCT în locul NIR: pe cub recepția mișcă pe factură, NIR-ul conex egal nu
  mișcă). Direcția inversă, „politica n-are perechi fără mișcare”, nu se mai
  măsoară de la tăierea regulilor de stoc fără consumator: perechile pe
  Magazie și NIR/Marfuri rămân în politică fără scenă care să le producă.
  Limită consemnată pentru decizia 110. (TR-D9a pasul 7, D9-D8, 2026-10-07)

- R6 verifică intervale configurate, nu toate condițiile legale ale unei cote.
  Proveniența fiscală a ajustărilor RDC/RLF/reduceri rămâne neacoperită:
  diagnosticul nu deduce factura fiscală din lot. Nu ține consumul/restul
  avansului și nu refuză depășirea lui; referințele parțiale repetate sunt
  permise. Nu repară automat taxele sau calificările istorice. (103-r2)
- Pe FCT/FCL, baza negativă și taxa culeasă pozitivă sunt încă admise în
  L3; abaterea este diagnosticată aritmetic. Un refuz pentru semne
  incompatibile și înlocuirea lui `ILinieCuAvans` ca proxy pentru linia de
  factură rămân de tratat la următoarea atingere a validării. (103-r3)
- Raportul R6, măsurat pe scara sintetică la 128 de documente fiscale în
  lună: 33 ms pe ușa nesecurizată, 104 ms și 24,9 MiB alocați pe cea
  securizată (verificarea de permisiune per obiect), 17 comenzi SQL,
  independent de istoric. Accesul leneș la `Tranzactie` a ieșit. Rămân
  nemăsurate rezolvarea tipului și a politicii per draft, citirea fără
  perioadă și deschiderea lookup-ului de avans. (103h, X-D5)
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
- Rotunjirea TVA pe document × cotă acoperă taxa nemarcată (109). Taxa
  culeasă rămâne suma liniilor, fără regularizare pe grup.
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
