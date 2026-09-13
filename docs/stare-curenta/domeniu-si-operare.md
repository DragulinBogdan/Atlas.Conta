# Domeniu și operare

**Actualizat: 2026-09-13.** [Index](README.md)

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

Registrele sunt scrise prin mecanismele motorului. Nu se editează prin CRUD.
Un rând operat se citește cu valorile și dimensiunile deja rezolvate.
Excepția scrierii directe pentru migrare este deschiderea contabilă/de stoc,
marcată prin `DocumentId = null`. (14, 25e, 40d)

Stările sunt `Draft`, `Operat` și `Stornat`. Documentul și liniile sale sunt
editabile în Draft. Starea originală din persistență este autoritatea
gardianului de editare; un formular vechi nu redeschide dreptul de scriere. (14, 55a)

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

## Împerechere și stingere automată

`Imperechere` este o relație many-to-many între două documente operate, cu
sumă pozitivă și posibil parțială. Link-ul se poate șterge; nu se editează.
Suma trebuie să respecte disponibilul ambelor părți și contrapartida comună. (31d, 41d)

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
