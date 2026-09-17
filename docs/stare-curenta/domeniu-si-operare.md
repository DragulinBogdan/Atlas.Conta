# Domeniu și operare

**Actualizat: 2026-09-16.** [Index](README.md)

## Modelul comun

Un document are două laturi cu sens economic, `Predator` și `Primitor`, un
antet și o colecție de linii. Sensul laturilor este stabilit de tipul concret.
`TipDocument` este ancora persistentă a tipului CLR pentru politici și UI;
nu permite inventarea unui tip de document prin configurare. (20, 81e)

Header-ele și liniile folosesc moștenire EF Core TPT. Derivata liniei este
declarată pe document prin `[TipDetaliu]`. Crearea și clonarea liniilor
respectă această declarație. (3, 40a, 54e)

| Element | Contract comun |
|---|---|
| `Document` | Număr, dată, laturi, stare, data operării, sursă, marcaj autogenerat și total derivat (22) |
| `DocumentDetaliu` | Tip material, lot opțional, cantitate semnată, valoare de postare, angajament opțional, tip TVA opțional și valoare TVA (22, 36a) |
| Tipul concret al liniei | Preț, produs cules, direcție, descriere, dimensiuni și alte date specifice (22a, 54c, 81a) |
| `Repartitor` | Identitate comună; derivate pentru partener, gestiune, angajat, unitate internă și cont propriu (16) |
| `Produs` | Identitatea din catalog; nu reprezintă o intrare în stoc (13) |
| `Lot` | Identitatea intrării, produsul, proveniența, data și prețul de evaluare (13, 26e) |

Un câmp intră în baza comună numai dacă are aceeași semantică pentru toate
tipurile care îl folosesc și este necesar direct postării. O valoare necesară
motorului poate fi furnizată printr-un contract; nu cere automat o coloană
pe bază. (2, 54c)

Furnizorul și clientul sunt roluri ale aceluiași `Partener`. Calitățile
transversale ale repartitorilor, precum loc de consum, comisie sau centru
de cost, se exprimă prin `Calitati`. Se folosește o clasă derivată când
identitatea este exclusivă și schema diferă. (16)

## Registre și ciclu de viață

| Registru | Fapt persistat | Deschidere fără document |
|---|---|---|
| `RegistruContabil` | Corespondență debit/credit, valoare și dimensiuni per latură | Da (14, 25e) |
| `RegistruStoc` | Mișcare de cantitate și valoare pe lot, repartitor și tip de stoc | Da (14, 25e) |
| `RegistruTva` | Fapt fiscal per linie, sens, cotă, regim, bază și TVA | Nu (68) |
| `RegistruImobilizari` | Eveniment sau lună per fișă de imobilizare: efecte semnate pe brut, brut fiscal, cumulat contabil/fiscal/deductibil și luni, parametrii de amortizare pe evenimente, locul la data faptului | Nu (87b) |

Registrele sunt scrise prin mecanismele motorului. Nu se editează prin CRUD.
Al patrulea registru este scris de documentul care îl declară prin
`IDocumentCuRegistruPropriu` (materializare, eliminare, storno), pe care
motorul îl cheamă prin interfață în cele trei puncte ale ciclului de viață;
dependențele dintre faptele aceleiași fișe le refuză tipul, nu motorul. (87c)
Un rând operat se citește cu valorile și dimensiunile deja rezolvate.
Excepția scrierii directe pentru migrare este deschiderea contabilă/de stoc,
marcată prin `DocumentId = null`. (14, 25e, 40d)

Stările sunt `Draft`, `Operat` și `Stornat`. Documentul și liniile sale sunt
editabile în Draft. Starea originală din persistență este autoritatea
gardianului de editare; un formular vechi nu redeschide dreptul de scriere. (14, 55a)

### Data documentului și data înregistrării

Documentul poartă două date. `Data` este a documentului fizic: numerotarea,
scadența, cronologia seriilor proprii și identitatea fiscală rămân pe ea.
`DataInregistrare` este data la care documentul intră în evidență. (F27-D4)

- Registrele contabil, de stoc și de imobilizări, precum și lotul născut din
  liniile documentului, se scriu la data înregistrării. `RegistruTva.Data`
  rămâne data faptului fiscal, adică data documentului; perioada în care faptul
  se declară este o coordonată separată pe rândul fiscal, decisă de politică
  atunci când perioada faptului e închisă. Regulile ei sunt în
  [politici și fiscalitate](politici-si-fiscalitate.md). (F27-D4, F27-D5)
- Gardianul de perioadă întreabă despre perioada datei înregistrării, la
  operare și la anulare. Un document cu data fizică într-o perioadă închisă și
  data înregistrării în cea deschisă se operează: documentul întârziat este
  flux normal, nu excepție, și nu atinge soldurile perioadei închise. (F27-D4)
- Ordinea FIFO este ordinea intrării în evidență, fiindcă lotul se naște la
  data înregistrării. Este singura ordine compatibilă cu „sold ≥ 0 la orice
  dată”. (13, F27-D4)
- Data stornării nu poate preceda data înregistrării. Pentru documentele de
  imobilizări stornoul se cere în luna înregistrării. (25d, 87g, F27-D4)
- Data înregistrării nu poate preceda data documentului. Regula este scrisă în
  gardianul de editare, în adaptorul de scriere al API-ului și în motor: căile
  standalone nu trec prin gardianul de Committing. (F27-D4)
- Implicitul este data documentului și se aplică la seam-uri, nu în setter: la
  creare și la schimbarea datei în ecranul XAF cât timp cele două erau egale,
  în adaptorul de scriere când clientul nu trimite câmpul, iar în motor ca
  normalizare pentru orice cale care nu-l culege (Import1C, Migrare, generate).
  Registrele unui document fără câmp cules cad acolo unde cădeau înainte. (F27-D4)
- Documentul conex și cele secundare care copiază data sursei moștenesc și data
  înregistrării ei. Documentele generate (amortizarea lunară, închiderea de TVA,
  descărcarea de gestiune) o primesc egală cu data lor chiar la creare, nu abia
  la operare. (17, F27-D4, F27-r9)

### Operarea

1. Culegerea se salvează prin ușa securizată. (42b)
2. Comanda este autorizată pe tipul și documentul cerut. (55b, 80b)
3. Motorul lucrează cu documentul încărcat prin ID într-un ObjectSpace
   non-secured propriu comenzii. (42b)
4. Calculează valorile prin contractele tipului, aplică evaluarea ieșirilor
   și validează perioada, starea, liniile, politicile, dimensiunile și stocul. (33d, 75a)
5. Materializează numărul și scadența implicite, finalizează loturile și
   creează rândurile de registru și documentele generate. (25c, 25f, 26d)
6. Asignează `Operat` și `DataOperare`, materializează stingerea automată
   cerută de contract și comite operația. (82c)

Validările calculului preced materializarea registrelor. Validările relațiilor
create pot încă refuza operația înainte de commit. La refuz, ObjectSpace-ul
comenzii se abandonează: lipsa commit-ului nu înseamnă că instanțele din
memorie au rămas nemodificate. (33d, 82)

`Valideaza` folosește aceeași cale de calcul și validare ca operarea, fără
commit. Poate modifica obiecte în memorie, deci cere un ObjectSpace propriu,
de unică folosință. `MesajeDupaOperare` este doar informare după commit și
nu trebuie să transforme o operație reușită într-un eșec aparent. (55b, 76c, 82c)

### Anulare și storno

- Anularea operării readuce documentul în Draft și elimină rândurile sale
  de registru. Este permisă numai în perioadă deschisă și fără dependenți. (14, 25d)
- Eliminarea mișcărilor proprii nu poate produce sold intermediar negativ.
  Un lot creat de document nu poate fi deja folosit de alt document. (25d)
- Copiii operați, laturile pereche operate care îl referă și împerecherile
  existente blochează corecțiile conform relațiilor lor. (26d, 31d, 65)
- Drafturile autogenerate ale sursei se elimină la anularea sursei. (26d)
- Stornoul adaugă rânduri inverse la data stornării, cu valori negative pe
  corespondența originală și marcaj `Storno`; nu inversează conturile. (25d, 46a)
- O perioadă absentă se consideră închisă. Administratorul nu ocolește
  granița perioadei fiscale închise. (14, 25d)

### Corecția unui document operat

- Nu există editare în loc a unui document operat. Peste graniță corecția
  este o singură comandă: `Corectează`, pe orice tip. (55a, F27-D6)
- Comanda stornează originalul la data cerută și creează, în aceeași
  tranzacție, un DRAFT nou de același tip concret, cu `CorecteazaId` spre
  original și `MotivCorectie` (`Eroare materială` / `Fapt nou`). Gardienii
  stornării rămân neschimbați: perioada corecției deschisă, data ≥ data
  înregistrării, fără copii operați, fără latură pereche operată, fără
  împerecheri. (F27-D6)
- Legătura este 1:1 și o scrie doar motorul. Invariantul se verifică și la
  fiecare commit securizat: original existent și stornat, de același tip
  concret, motiv prezent, niciun al doilea document spre același original.
- Documentul nou păstrează `Numar` și `Data` ale documentului fizic (seria nu
  se consumă din nou) și primește `DataInregistrare` = data corecției.
- Culegerea se copiază generic, prin metadata EF: toate proprietățile scalare
  și FK-urile mapate ale lanțului TPT, pe antet și pe linii. Nu se copiază
  identitatea (`ID`), câmpurile motorului (`Stare`, `DataOperare`,
  `Autogenerat`, `DocumentSursaId`), datele proprii corecției
  (`DataInregistrare`, `CorecteazaId`, `MotivCorectie`) și câmpurile de
  infrastructură ale lui `BaseObject`.
- Lotul: linia care a NĂSCUT un lot (`Lot.LinieIntrareId == linia`) primește
  pe copie un lot PROPRIU, nou și nefinalizat, pe care motorul îl finalizează
  la operare (preț, dată). Linia care doar CONSUMĂ un lot îl păstrează prin
  `LotId`, copiat ca orice FK. (26e)
- Un original nu se corectează de două ori, iar un document care nu e operat
  nu se corectează deloc.
- Perioada închisă nu se atinge: storno-ul și documentul nou trăiesc în
  fereastra deschisă, iar snapshot-ul perioadei rămâne cel de la închidere.
  Efectul FISCAL al motivului e în `politici-si-fiscalitate.md`.
- Comanda: `Motor/CorectieService.cs`, prin `Api/OperareApi.Corecteaza`;
  ușile sunt `POST api/documente/{id}/corecteaza` și acțiunea XAF
  „Corectează" de pe orice DetailView de document.

### Perioada fiscală ca lanț

- Perioada este o verigă identificată prin an și lună, unică între rândurile
  vii. Anul și luna se culeg la creare și nu se mai schimbă. (F27-D1)
- Starea perioadei — închisă, momentul închiderii curente și momentul primei
  închideri — aparține motorului. Pe calea securizată se refuză orice scriere
  asupra ei, ca la registre. (F27-D1)
- Închiderea unei perioade cere perioada precedentă închisă. O perioadă
  precedentă absentă este închisă prin absență și dă capătul lanțului. (F27-D1)
- Redeschiderea cere perioada următoare deschisă sau absentă și un motiv
  scris: se redeschide numai ultima perioadă închisă. Momentul primei
  închideri nu se șterge la redeschidere. (F27-D1)
- Fiecare închidere și redeschidere scrie un rând în istoricul perioadei, cu
  felul, momentul, utilizatorul și motivul. Istoricul este append-only și
  aparține motorului. (F27-D1)
- Verificarea de închidere întoarce constatări tipizate, cu cheie stabilă,
  fel, severitate, text și obiectul la care se referă. Constatările
  STRUCTURALE stau în cod și nu se configurează: perioadă nedefinită, perioadă
  deja închisă, perioadă precedentă deschisă. Ele sunt întotdeauna blocante, iar
  cât timp una dintre ele stă în picioare constatările de conținut nu se mai
  caută. (F27-D2)
- Constatările de CONȚINUT sunt patru, iar severitatea fiecăreia vine din
  politica de închidere de perioadă, nu din cod: închiderea de TVA lipsă sau
  neoperată pe lună; amortizarea lunară lipsă sau neoperată, când luna are fișe
  de amortizat; fiecare document în lucru cu data înregistrării în perioadă;
  fiecare document operat, scadent și cu rest la sfârșitul perioadei.
  Cheile lor sunt `ITV-LIPSA`, `AMO-LIPSA`, `DRAFT-IN-PERIOADA:{id}` și
  `REST-SCADENT:{id}`; primele două n-au sufix fiindcă amândouă sunt ale
  societății, nu ale unei unități interne. Fiecare familie listează cel mult 200
  de rânduri și rezumă restul. (F27-D2)
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
- Fiecare comandă a motorului rulează într-o tranzacție explicită, deschisă pe
  ObjectSpace-ul ei: operarea, anularea și stornarea prin adaptorul de
  operare, generarea și regenerarea închiderii de TVA și a amortizării,
  închiderea, redeschiderea și reconstrucția perioadei. Gardianul de perioadă
  citește starea lunii blocând rândul în citire, iar comenzile perioadei îl
  blochează în scriere înainte de orice calcul. Cele două comenzi se
  serializează astfel între ele, iar două operări concurente nu se blochează
  una pe alta. (F27-D1)

### Soldurile materializate la închidere

- Snapshot-ul unei perioade există dacă și numai dacă perioada este DE
  REFERINȚĂ: ultima perioadă închisă sau un decembrie închis. Nu este registru
  și nu este urmă — se reconstruiește integral din registre. (F27-D3)
- Cheia snapshot-ului este cheia completă a atomului: cont plus cele opt
  dimensiuni ale laturii pe partea contabilă, lot, repartitor și tip de stoc
  pe partea de stoc. Debitul și creditul se cumulează separat, fiindcă netarea
  nu este aditivă. Orice raport este rollup aditiv peste ea. (F27-D3, 66d)
- Cheile integral zero se omit. Cheia absentă înseamnă zero pentru orice
  consumator. (F27-D3)
- Închiderea scrie snapshot-ul lunii ca sumă între snapshot-ul precedentei și
  rulajele lunii, iar dacă precedenta nu este capăt de an îi șterge
  snapshot-ul. Fără snapshot precedent, luna se calculează prin sumă peste tot
  istoricul. Redeschiderea șterge snapshot-ul lunii și îl reconstruiește pe al
  precedentei, tot prin sumă integrală. Registrele nu se ating. (F27-D3)
- Reconstrucția recalculează integral fiecare perioadă de referință,
  raportează diferențele pe rânduri și pe sume înainte de a rescrie, apoi
  șterge snapshot-urile perioadelor care nu mai sunt referințe. Raportul iese
  și când nu există nicio diferență. (F27-D3, 35b)
- Scrierea și ștergerea snapshot-urilor aparțin motorului: pe calea securizată
  se refuză, ca la registre. Ștergerea lor este fizică, nu amânată. (F27-D3)

### Soldul citit: referință plus rulaje

- Un singur serviciu răspunde „soldul la data d": snapshot-ul ultimei perioade
  de referință care se termină până la d, plus rulajele de după ea. Fără nicio
  perioadă de referință, citirea este integral din registre — de aceea o bază
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
- Soldul de stoc, soldurile pe loturi la o dată, soldul unei chei, alocarea
  FIFO și gardianul de sold negativ pornesc de la aceeași referință. Gardianul
  cumulează de la rândul sintetic încoace: zilele dinaintea lui sunt într-o
  perioadă închisă, unde nicio mișcare nouă nu poate ajunge. Textul refuzului
  nu se schimbă. (F27-D3, 14/25d)
- Soldurile conturilor de TVA ale închiderii lunare vin din aceeași sursă
  cumulată. (F27-D3)
- Excluderea rândurilor unui document (dry-run pe re-operare) și excluderea
  rândurilor eliminate la anulare ating doar rulajele: un document cu rânduri
  într-o perioadă închisă nu se mai poate anula. (F27-D3)
- Cheia cu cantitate ȘI valoare zero lipsește din soldul de stoc și din
  soldurile pe loturi, ca din snapshot: un lot consumat integral nu mai este o
  poziție de stoc și nu mai apare în listă. Cheia cu cantitatea zero și valoare
  nenulă RĂMÂNE — reziduul valoric se vede, nu se ascunde. Motorul nu simte
  diferența: citește soldurile cu valoare implicită zero pe cheia absentă.
  (F27-D3, 74g)

## Contare și dimensiuni

Planul de conturi este sintetic. Analiticele se obțin din dimensiuni în
raportare, fără conturi analitice generate și persistate. (10)

Nomenclatoarele-dimensiune (cod funcțional, cod economic, sursă de
finanțare, proiect, angajament) derivă din baza abstractă `Dimensiune`:
cod, denumire și căutare fără diacritice. Baza e contractul pentru codul
generic (`T : Dimensiune`) și rămâne în afara modelului EF: fiecare
derivată are tabelul și coloana de căutare proprie. `Unitate` nu derivă
încă. (2026-09-13)

`SursaCont` alege cont explicit, contul implicit al tipului material sau
contul implicit al repartitorului de pe una dintre laturi. Motorul nu
conține simboluri de cont specifice profilului. (26b, 29)

Potrivirea politicilor pe o linie este un set de funcții pure pe fapte plate
(`Motor/Potrivire.cs`), consumat de motor, de gardurile documentelor și de
explicație; entitățile se mapează la fapte o singură dată (`Fapte`). La
contare, tipul material exact are prioritate față de filtrul de natură, iar
acesta față de regula generică; `SemnFiltru` elimină regulile incompatibile
înaintea alegerii; câștigătorul e primul de la nivelul cel mai înalt, în
ordinea în care baza întoarce regulile. La stoc, per latură, regulile
specifice pe clasă bat regula generică, iar generica se aplică doar liniilor
cu natura de stoc. Lipsa unei reguli nu produce o notă; tipurile pentru care
tăcerea ar pierde postarea declară pe clasă `[GardContare(natură, nivel
minim, mesaj)]` — aplicat o singură dată în validarea de bază a documentului,
pe toate axele potrivirii (FCL, DSC, RDC cer regulă exactă pe liniile de
stoc; trezoreria cere cel puțin natura pe liniile de virament). Postarea
explicită este permisă numai prin contractele declarate de tip și de linie. (26c, 28a, 32a, 64, 84e–f)

Dimensiunile disponibile sunt repartitor, material, cod funcțional, cod
economic, sursă de finanțare, unitate, proiect și centru de cost. FK-urile
culese sunt pe frunzele liniilor; `DimensiuniCulese()` furnizează motorului
un value object nepersistat. Regulile și registrele păstrează câmpurile plat. (15, 54c)

Pentru fiecare latură, rezolvarea urmează: valoarea liniei → override-ul
laturii din regulă → valorile comune ale regulii → default-ul polimorf al
antetului → materialul din lot. Postarea explicită a liniei are prioritate.
`Cont.DimensiuniObligatorii` se verifică pe rezultatul rezolvat. În validarea
clasificației bugetare, angajamentul poate satisface cerința de cod economic. (25f, 32c, 33a)

## Precizie numerică

Cantitățile folosesc `numeric(18,3)`, sumele `numeric(18,2)`, iar prețurile
`numeric(18,6)`. Convențiile sunt centralizate în `Scara`; o proprietate
decimală fără mapare explicită este refuzată la verificarea modelului. (49e)

Prețul se rotunjește cu `AwayFromZero`. Rotunjirea sumelor respectă profilul
fixat al bazei. Culegerea, motorul și raportarea folosesc aceeași convenție;
clientul nu introduce o rotunjire contabilă independentă. (42c, 51c, 52a)

## Stoc și evaluare

- Doar `ClasaProdus.Natura = Stoc` intră în regulile de stoc. Natura și tipul
  material sunt date distincte de identificarea produsului. (23b)
- Cheia de sold este `(Lot, Repartitor, TipStoc)`. Localizarea curentă a
  lotului se citește din registru. (25d, 27b)
- Soldul cantitativ intermediar trebuie să fie nenegativ la orice dată
  afectată, inclusiv pentru documente introduse retroactiv. (25d)
- Lotul se naște la culegerea liniei de intrare și se finalizează la operare.
  `LoturiCulegereService` este calea comună XAF/Apply. Un lot finalizat nu
  este șters de curățenia culegerii. (25c, 56, 53f)
- `ILinieCareNasteLot` declară nașterea lotului; `ProdusId` pe o linie care
  doar selectează/pin-uiește stoc nu înseamnă naștere de lot. (62)
- Evaluarea este identificare specifică pe lot. Alegerea automată este FIFO
  în produs × gestiune; pin-ul manual are prioritate. Un pin nu completează
  automat lipsa sa cu alte loturi. (13, 37d, 38b)
- La ieșirea care golește cheia de stoc se preia întregul sold valoric rămas.
  Acest calcul aparține motorului. Ieșirea fiscală RLF păstrează valoarea
  documentului furnizorului și nu absoarbe reziduul lotului. (75a)
- O operație retroactivă nu recalculează valorile unor ieșiri deja operate. (75a)

## Tipuri de document

| Cod / tip | Regula specifică |
|---|---|
| FCT — factură de intrare | Numărul furnizorului este cules. Pentru stoc, naște lotul și generează NIR; postează liniile care nu trec pe NIR și TVA-ul propriu. Poate genera o plată draft din datele culese. (26a, 31e, 56) |
| NIR — recepție | Postează recepția. Poate fi manual sau generat din FCT. Lot propriu: preț cules × cantitate; lot străin din factură: prețul lotului. Nu culege TVA. NIR-ul conex nu se șterge independent din client. (26a, 62, 62f) |
| FCL — factură de ieșire | Postează venitul și creanța. În privat poate genera DSC; în bugetar regulile o restrâng la document fără stoc. Numărul fiscal este al serverului. (30a, 30b, 56) |
| DSC — descărcare | Generat de serviciu din FCL, cu `LinieSursaId`, la cost, fără TVA; gestiune → client, cu ambele dimensiuni de repartitor pe gestiune. Clientul oferă citire și comenzi, fără creare manuală. (37a, 37b, 58) |
| BTR — transfer | Mută stocul între gestiuni. Transferul simplu nu postează note contabile în planul sintetic. (23c) |
| BCS — bon de consum | Scade Magazie de la predator și crește Consum la primitor; consumul rămâne pe responsabil. Valoarea se derivă din lot. (27a, 27d) |
| LDI — diferențe inventar | Direcție explicită Plus/Minus, culegere pozitivă, semn la materializare. Plusul naște lot pe gestiunea predatorului; minusul cere lot existent, străin de document. Primitorul are calitatea Comisie. (28a, 28d, 63) |
| PLT / INC — plată / încasare | Valoarea se culege pe linii ca defalcare. Contarea se rezolvă din laturi. PLT: cont propriu → beneficiar; INC: plătitor → cont propriu. Pot exprima și picioarele unui virament intern. (31a, 31c, 64) |
| DEC — decont | Angajat → repartitor intern, fără stoc. Contractul permite cont și repartitor explicite pe linie. Cantitatea pro-formă zero se normalizează la unu. (32a, 32b, 32d) |
| NTC — notă contabilă | Postare explicită. Poate stinge manual pe contrapartidă și sens; nu se înscrie implicit în stingerea automată a sursei. ITV nu este editabil prin această felie. (46b, 79c, 82a) |
| ITV — închidere TVA | Rezultatul serviciului lunar, cu conturi din politică. Nu se culege ca agregat liber și nu închide perioada fiscală. (46c, 79a) |
| DVI — declarație vamală de import | Linii pe detaliul de bază: valoarea în vamă ca bază, taxa declarată (0 = din cotă la operare). Nu postează valoarea și nu mișcă stocul; postează doar taxa din politica TVA (4426 contra contului implicit al predatorului — biroul vamal/comisionarul — sau 4426 = 4427 la amânarea plății). MRN cules, fără numerotare. Legătura n→m cu facturile de import este evidență, doar în Draft, prin agregat. Nu este document stins: taxa se plătește ca orice taxă, fără împerechere. (86a, 86b, 86e, 86g) |
| ASM — asamblare/dezasamblare | Transformare n→m cu linii de produs și consum; fără contare. Diferența valorică absolută trebuie să fie ≤ 0,005. Nu consumă un lot produs de același document. (46d) |
| RLF — retur la furnizor | Folosește lotul original; culegere pozitivă, postare cu semn negativ pe corespondența originală. (46e, 76d) |
| RDC — retur de la client | Un document cu linii de venit și cost pe lotul original. Totalul include doar venitul; linia de cost nu are tip TVA. Rolul unei linii salvate nu se convertește prin editare. (46e, 76d) |
| PIF — punere în funcțiune | Unitate internă → loc; linii per fișă cu `Intrare`, `Modernizare` sau `Revizuire`. Nu postează: scrie evenimentele și parametrii de amortizare în registrul imobilizărilor și materializează starea fișei. `Intrare` cere fișă nouă și parametri completi, cu linie sursă (linia unei FCT operate de clasă F, cu plafonul consumului) sau cu valoare culeasă și inițiale; `Modernizare`/`Revizuire` cer fișă în funcțiune. Refuzat dacă o AMO operată există într-o lună ulterioară. (87e) |
| CAS — ieșire de imobilizare | Loc → unitate internă, cu cauza (casare, vânzare, lipsă). Se culeg doar fișele; liniile le produce serverul din situația la dată și politica tipului material: amortizarea cumulată contra contului imobilizării (omisă la cumulat zero) și valoarea rămasă pe cheltuiala de cedare (omisă la net zero). Operarea recalculează și refuză liniile care nu mai corespund; refuzată dacă o AMO operată acoperă luna ieșirii sau una ulterioară; după operare avertizează dacă luna precedentă n-are amortizare operată. Fișa devine ieșită. (87f) |
| AMO — amortizare lunară | Document generat pe unitate internă și lună, ca ITV. Linie per fișă eligibilă cu trei cifre (contabilă, fiscală, deductibilă); postează doar cifra contabilă (cheltuială = amortizare, din politică) și scrie rândul lunar în registrul imobilizărilor. Nu se culege liber; fără flux `/nou`. (87g) |

Regimurile capitalizate nu sunt acceptate pe retururi. Retururile nu devin
stingători; compensarea lor folosește nota contabilă. (46e, 76g)

`DescarcareService` alocă pin-urile înainte de FIFO și generează acoperirea
disponibilă fără să ascundă restul neacoperit. Acoperirea include documentele
Draft și Operat. Generarea suplimentară este o comandă pe FCL operat;
gardianul de stoc rămâne autoritatea la operarea DSC. (37b, 38b, 38d)

Distribuirea valorii produselor ASM folosește calculul motorului într-un
ObjectSpace separat. Cazurile mixte sau nereprezentabile la scara numerică
se refuză cu explicația diferenței, fără o formulă paralelă în client. (76c)

## Documente generate și viramente

Documentele generate au `DocumentSursaId` și `Autogenerat`. `PoliticaConex`
configurează clonarea filtrată; `GenereazaSecundar` produce documentul
specific tipului. Ele sunt create în tranzacția operării sursei și se
operează separat. (17, 26d, 31e)

Viramentul intern este o pereche PLT/INC pe aceleași laturi, ambele conturi
proprii. Liniile de natură Virament și laturile trebuie să fie coerente în
ambele sensuri. Regula de contare trebuie să se potrivească inclusiv pe semn. (64)

`LaturaPerecheId` se scrie pe o singură parte. Legătura se citește simetric;
reciprocitatea și folosirea unei laturi deja aparținând altei perechi sunt
refuzate. O pereche activă este Draft sau Operat. Legătura existentă suprimă
generarea automată și când este declarată de celălalt picior. (65)

Viramentul nu stinge și nu poate fi stins. Perechea draft autogenerată poate
fi ștearsă; diferența rămâne vizibilă pe contul de tranzit. Avertismentele
despre picioare compatibile sunt consultative: două transferuri identice
pot reprezenta operații distincte. (64, 65)

## Imobilizări

Fișa `Imobilizare` este nomenclator subțire: număr de inventar unic,
denumire, tip material de clasă F (contul imobilizării este contul implicit
al tipului), clasificare opțională din catalog, loc (repartitorul notelor),
centru de cost, responsabil, cod economic (dimensiunea bugetară a
cheltuielii) și starea materializată de motor: nouă, în funcțiune, ieșită,
cu datele punerii în funcțiune și ieșirii. Metoda, durata, valoarea
reziduală, categoria fiscală și valoarea nu sunt pe fișă: sunt fapte datate
în registru, scrise de documente. Parametrii curenți sunt proiecția
ultimului eveniment. (87a)

Gardianul fișei: tipul material este de clasă de imobilizări și se schimbă
doar cât fișa este nouă; locul și codul economic cât este nouă sau în
funcțiune; nimic pe fișa ieșită; ștergerea doar pe fișa nouă, fără rânduri
de registru și fără linii de documente care o poartă, chiar în Draft;
starea și datele le scrie doar motorul. Transferul este schimbarea locului pe fișă, fără
document: următoarea amortizare postează pe noul loc, istoricul locului
este pe rândurile lunare. (87a, 87h)

Situația fișei la o dată este suma coloanelor registrului plus parametrii
ultimului eveniment până la acea dată; fișa, registrul imobilizărilor și
proiecțiile fiscale sunt sume peste registru. Amortizarea fiscală și cea
deductibilă nu postează și nu au document propriu: sunt cifre înghețate pe
rândul lunar, calculate din aceiași parametri datați și din regulile
valabile la data rândului. (87b)

Aritmetica este exclusiv în `AmortizareService`, ca funcție pură aplicată
de trei ori pe lună. Cota liniară este valoarea de amortizat împărțită la
lunile rămase, rotunjită la bani, fixată la ultimul eveniment; suma lunară
este minimul dintre cotă și rest, iar ultima lună absoarbe restul. Baza
„la ultimul eveniment" este situația la sfârșitul lunii evenimentului:
luna evenimentului postează cota veche, parametrii noi curg din luna
următoare, indiferent de zi. Accelerata amortizează 50 % din brut în
primele 12 luni de la punere, apoi liniar; degresiva AD1 aplică coeficientul
pe ani (durata trebuie să fie multiplu de 12) și trece la liniar. Fișa este
eligibilă în luna M dacă a fost pusă în funcțiune înaintea primei zile, nu a
ieșit până la ultima zi, are cel puțin un eveniment de registru până la
ultima zi și mai are rest contabil sau fiscal; linia cu contabil zero și
fiscal pozitiv rămâne fără conturi. (87g)

Lunile pe care le acoperă amortizarea lunii M sunt cele DATORATE minus cele
ACOPERITE: datorate = lunile întregi de la luna de după punerea în funcțiune
până la M inclusiv (luna punerii nu se amortizează), acoperite = suma
coloanei de luni a rândurilor de amortizare până la sfârșitul lui M.
Diferența nulă sau negativă scoate fișa din lună. Diferența mai mare decât
unu este o RECUPERARE: fișa a intrat în evidență după luna punerii în
funcțiune, fiindcă registrul se scrie la data înregistrării. Cota lunii este
atunci suma cotelor celor n luni, calculată iterativ — pragurile
degresivului și ale acceleratului avansează la fiecare pas, iar restul scade
după fiecare — nu cota lunii înmulțită cu n; recuperarea nu depășește restul
de amortizat și nici durata rămasă. Deductibilul se calculează pe SUMA
lunii, cu regulile valabile la sfârșitul ei: plafonul lunar este al lunii de
declarare și se aplică o singură dată pe suma recuperată, nu o dată pe
fiecare lună recuperată. Linia amortizării și rândul de registru poartă
lunile acoperite; ele intră în cheia anti-stale a operării și se inversează
la storno ca oricare altă coloană. Fișa fără niciun eveniment până la
sfârșitul lunii precedente, dar cu rânduri în M, este punerea în funcțiune
înregistrată întârziat: baza și parametrii i se citesc la sfârșitul lui M.
Gardianul lunii precedente lipsă rămâne neatins: se recuperează doar ce
n-a avut cum să fie amortizat, nu ce n-a fost amortizat. (F27-D4)

Generarea lunară urmează ordinea gardienilor: fișă eligibilă fără politică,
amortizare vie în lună, amortizare vie ulterioară, draft anterior, lună
precedentă lipsă, perioadă închisă, nicio fișă. Motivul se raportează la
previzualizare și refuză comanda. Operarea cere data ultimei zile a lunii
și aceeași unitate internă pe ambele laturi, apoi recalculează și refuză
dacă mulțimea liniilor diferă pe fișă, cele trei cifre, conturi, loc, centru
de cost sau cod economic; același criteriu dă `Stale` pe API. Anularea sau
stornarea unei amortizări este refuzată dacă există o amortizare operată
ulterioară sau fapte ulterioare ale fișelor ei (ieșire, modernizare,
revizuire); aceeași regulă refuză anularea sau stornarea unei puneri în
funcțiune, iar anularea sau stornarea unei ieșiri este refuzată cât există
amortizare operată pentru luna ieșirii sau una ulterioară. (87e, 87f, 87g)

Stornoul oricărui document de imobilizări se datează în luna documentului:
rândul invers datat în altă lună ar lăsa situația fișelor falsă între cele
două date. O fișă apare o singură dată pe o punere în funcțiune; duratele
revizuite depășesc lunile deja amortizate. (87e, 87g)

## Împerechere și stingere automată

`Imperechere` este o relație many-to-many între două documente operate, cu
sumă pozitivă și posibil parțială. Link-ul se poate șterge; nu se editează.
Suma trebuie să respecte disponibilul ambelor părți și contrapartida comună. (31d, 41d)

Împerecherea este un fapt **datat** (`Data`): automat, data înregistrării
stingătorului; manual, ziua cerută, implicit azi. Data trebuie să cadă
într-o perioadă deschisă și să nu preceadă data înregistrării niciunuia
dintre documentele legate. (F27-D8)

Ștergerea rămâne liberă cât timp `Data` cade în fereastra deschisă. O
împerechere dintr-o perioadă închisă nu se șterge: se desface printr-un
**rând invers** — aceleași documente, sumă negativă, `Data` în perioadă
deschisă și nu înaintea originalului, `InverseazaId` completat. Legătura
original ↔ invers este 1:1, iar rândul invers îl scrie doar motorul
(`ImperechereService.Desfa`, acțiunea „Desfă împerecherea”, `POST
api/imperecheri/{id}/desfa`). `Asignat` însumează **algebric**, deci
desfacerea eliberează restul pe ambele documente fără a șterge nimic. (F27-D8)

La stornarea unui document împerecheat, împerecherile din fereastra deschisă
se cer șterse ca înainte, iar cele dintr-o perioadă închisă și încă
neinversate primesc rândurile inverse la data stornării, scrise de
`ImperechereService.InverseazaLaStorno` înainte de storno. Anularea rămâne
refuzată la orice împerechere. (F27-D8)

Contractele documentului sunt:

| Contract | Semnificație |
|---|---|
| `CapacitateStingere(os)` | Contrapartidele și plafoanele pe sens pe care documentul le poate stinge (49a, 76f) |
| `PoateFiStins(os)` | Participarea pe rolul de document stins (64) |
| `SensDeStins(os)` | Natura soldului de stins; lipsa declarației nu autorizează ghicirea sensului (76f) |
| `LiniiCreanta(query)` | Liniile care contribuie la totalul creanței/datoriei (46e, 57c) |
| `SursaStingeriiAutomate(os)` | Sursa solicitată pentru stingere automată; implicit `null`, fără efecte secundare (82a) |

Un tip care nu închide nicio datorie o declară prin `PoateFiStins = false`
(DVI): fără declarație, validarea ar accepta tăcut o împerechere când
plafonul stingătorului oferă un singur sens. Totalul folosit la stingere este
Σ(valoare + TVA) pe liniile creanței; un tip cu altă formulă a restului nu
intră pe rolul de document stins. (86g)

Totalul este **fapt scris**, nu agregat la citire: motorul îl calculează din
`LiniiCreanta` și îl pune pe `Document.TotalStingere` la operare, în aceeași
tranzacție cu registrele; îl șterge la anulare; nu îl atinge la storno.
`ImperechereService.Total` îl citește de pe cheie și refuză explicit un
document ieșit din Draft fără total scris. Câmpul este al motorului:
gardianul refuză scrierea lui pe ușa securizată. (F27-D7)

### Partide deschise

`PartidaDeschisa` (`An`, `Luna`, `DocumentId`, `Rest`) este restul de stins al
fiecărui document operat la sfârșitul unei perioade **de referință**, scris de
`SolduriService.MaterializeazaPartide` în tranzacția închiderii, lângă
snapshot-urile de solduri și cu aceeași regulă de referință. Rest =
`TotalStingere` − Σ `Imperechere.Suma` (ambele roluri, algebric, `Data` până la
sfârșitul perioadei); rândurile cu rest zero se omit. Ștearsă la redeschidere,
rescrisă la re-închidere, verificată de `Reconstruieste` (existente /
recalculate / diferite + Δrest). La 31.12 lista este chiar arieratele la nivel
de document. (F27-D7)

`ImperecheriProiectii.DocumenteCuRest(contrapartidă?, sens?, laData?)` pornește
de la ultima perioadă de referință: candidații sunt partidele ei, plus
documentele înregistrate după ea, plus documentele atinse de o împerechere din
fereastra deschisă (o desfacere poate readuce în listă un document stins
integral la închidere). Costul este mărginit de fereastra deschisă plus
numărul partidelor, nu de tot istoricul. `ReturClient` a intrat în uniune
(a șasea ramură): totalul lui este cel filtrat prin `LiniiCreanta`, scris de
motor, deci proiecția nu mai poate diverge de serviciu. Rândurile lui rămân
totuși în afara listei, dar din alt motiv — creanța unui retur este negativă
după operare, iar filtrul `Rest > 0` o taie. (F27-D7)

`ContabilProiectii.SoldParteneri(laData, contId?, repartitorId?, dimensiuni)`
este partea de sold a balanței analitice pe aceeași cheie (cont × repartitor),
citită prin aceiași atomi cumulați; rândurile cu sold net zero se omit. (F27-D7)

Entitățile care își poartă singure invarianții de commit implementează
`IVerificabilLaCommit`; gardianul le cheamă prin interfață înaintea
verificărilor pe tip, inclusiv la ștergere (`DviFactura`: creare/ștergere doar
cât declarația este Draft, factura operată, perechea unică, fără editare). (86f)

Plata stinge datorii, iar încasarea creanțe. PLT→FCL, INC→FCT, PLT→PLT și
INC→INC sunt refuzate. NTC poate avea ambele sensuri; plafonul este netat
pe repartitor × latura liniei. Ambiguitatea de contrapartidă cere alegere
explicită; ambiguitatea de sens se rezolvă prin modelul documentului.
Pe NTC se afișează plafoane grupate, nu un „Rest” derivat din suma liniilor. (41d, 76f, 76g)

Trezoreria solicită stingere automată numai dacă este autogenerată, are
sursă și capacitate de stingere nenulă. Serviciul verifică și rolul sursei.
O NTC cu sursă și capacitate manuală nu participă automat. (82a)

`ImperechereService.CreeazaAutomataLaOperare` folosește: (82b)

```text
disponibil = suma(Valoare + ValoareTva din liniile curente) - Asignat(document)
suma automată = min(disponibil, Ramas(sursa))
```

Doar suma pozitivă intră în validările comune de creare a împerecherii,
cu marcajul autogenerat. Serviciul nu comite. Apelul este după materializarea
registrelor și starea Operat, înainte de commit-ul motorului. (82b, 82c)

## Locurile regulilor în cod

- [Document și contracte](../../nou/Atlas.Conta.BackOffice/Atlas.Conta.BackOffice.Module/BusinessObjects/Documente/Document.cs)
- [Motorul operării](../../nou/Atlas.Conta.BackOffice/Atlas.Conta.BackOffice.Module/Motor/MotorOperare.cs)
- [Serviciul de împerechere](../../nou/Atlas.Conta.BackOffice/Atlas.Conta.BackOffice.Module/Motor/ImperechereService.cs)
- [Documentele de trezorerie](../../nou/Atlas.Conta.BackOffice/Atlas.Conta.BackOffice.Module/BusinessObjects/Documente/Trezorerie.cs)
- [Documentele imobilizărilor](../../nou/Atlas.Conta.BackOffice/Atlas.Conta.BackOffice.Module/BusinessObjects/Documente/Imobilizari.cs)
- [Serviciul de amortizare](../../nou/Atlas.Conta.BackOffice/Atlas.Conta.BackOffice.Module/Motor/AmortizareService.cs)
