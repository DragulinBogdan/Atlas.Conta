# Documente

**Actualizat: 2026-10-16.** [Index](README.md)

Modelul comun al documentelor, ciclul lor de viață (culegere, operare, anulare, storno, corecție, retragere), data înregistrării și documentele generate.

## Modelul comun

Un document are două laturi cu sens economic, `Predator` și `Primitor`, un
antet și o colecție de linii. Sensul laturilor este stabilit de tipul concret.
`TipDocument` este ancora persistentă a tipului CLR pentru politici și UI;
nu permite inventarea unui tip de document prin configurare. (20, 81e)

`Document`, `DocumentDetaliu` și `Repartitor` folosesc moștenire EF Core TPH:
fiecare ierarhie stă pe tabela rădăcinii (`Documente`, `DocumentDetalii`,
`Repartitori`), iar tipul concret al rândului e discriminatorul `ClrType`,
numele scurt al clasei CLR. Pe document valoarea lui este ancora
`TipDocument.ClrType`. `ClrType` îl scrie doar EF, la creare; e read-only
și poartă caption-ul „Tip”. Discriminatorul este o etichetă citită ca dată,
nu un comutator: motorul nu decide nimic pe valoarea lui. Derivata liniei
este declarată pe document prin `[TipDetaliu]`. Crearea și clonarea liniilor
respectă această declarație. Liniile unui document sunt frunza declarată de
el, un subtip al ei sau baza. (3, 16, 40a, 54e, 89a, 89h)

Proprietățile cu același nume de pe frunze-surori împart aceeași coloană,
numită ca proprietatea, fără prefix de tip. O coloană de frunză se citește
numai pe o mulțime deja restrânsă la tipul frunzei (`GetObjectsQuery<Frunza>`,
`OfType<Frunza>`, liniile unui document al cărui `[TipDetaliu]` e frunza) sau
prin `x is Frunza ? ((Frunza)x).Prop : null`. `as` și cast-ul pe frunză nu
filtrează pe tip: citesc și valoarea fratelui care împarte coloana. SQL-ul
brut pe o coloană de frunză filtrează pe `ClrType`. Coloanele frunzelor sunt
nullable în tabelă; pe rândurile altui tip sunt NULL. (54c, 89b, 89c, 89d)

Tipul documentelor se citește ca dată, dintr-o proiecție `{ID, ClrType}`, prin
`CititorTipDocument`: codul tipului, clasa concretă, `null` pentru un id
inexistent sau invizibil. Un document nu se materializează doar ca să i se
afle clasa. (20, 89f)

Un FK spre un tip ne-rădăcină al unei ierarhii (de exemplu `Lot.GestiuneId`
spre `Gestiune`, `DviFactura.FacturaId` spre `FacturaIntrare`) ține în bază
doar id-ul rădăcinii. Pe ușa securizată, `GardianEditare` verifică, la obiect
nou sau la FK schimbat, că ținta are tipul cerut sau un subtip al lui: altfel
refuză cu 422 („rândul ales e X, nu Y”), iar o țintă invizibilă e refuzată
ca referință invizibilă. FK-urile sunt descoperite din
metadata EF, nu dintr-o listă. Pe ușa de sistem integritatea o probează
ModelCheck. (89e)

| Element | Contract comun |
|---|---|
| `Document` | Număr, dată, laturi, stare, data operării, sursă, marcaj autogenerat și total derivat (22) |
| `DocumentDetaliu` | Tip material, lot opțional, cantitate semnată, valoare de postare, angajament opțional, tip TVA opțional și valoare TVA (22, 36a) |
| Tipul concret al liniei | Preț, produs cules, direcție, descriere, dimensiuni și alte date specifice (22a, 54c, 81a) |
| `Repartitor` | Identitate comună; derivate pentru partener, gestiune, angajat, unitate internă și cont propriu (16) |
| `Produs` | Identitatea din catalog; nu reprezintă o intrare în stoc (13) |
| `Lot` | Identitatea intrării, produsul, proveniența, data și prețul de evaluare (13, 26e) |

Entitățile de domeniu derivă din `EntitateConta` (contractele XAF și cheia
`Guid ID`), nu din `BaseObject`-ul DevExpress; tipurile de securitate rămân
pe el. Mecanismele tehnice vin din familie: `Editabila` (blocare optimistă),
`Nomenclator` (`Editabila` + `Activ`), `Politica` (`Editabila`), `Document`
(`Editabila`, rădăcina TPH) și `RandRegistru` (scris de motor, fără blocare).
Clasificarea claselor pe familii e în fișierul deciziei. `ICuCheie` e cheia
comună a entităților proprii și a celor de securitate. (104e)

Nu există ștergere amânată: modelul nu are `GCRecord` și nici filtru global
de interogare. Draftul se șterge fizic, documentul operat se stornează,
nomenclatorul se inactivează (`Activ`, fără default în schemă), iar
tranzacțiile și postările cubului le șterge doar motorul, la corecția directă. `Cascade` apare numai
pe compoziții, adică pe colecțiile `[Aggregated]` (`Document.Detalii`,
`Dvi.Facturi`). Restul FK-urilor de domeniu sunt `ClientNoAction` în EF și
`NO ACTION` în schemă: refuzul vine mereu din bază, chiar dacă dependentul e
încărcat, și iese 422 de domeniu prin traducătorul de constrângeri, cu cele
două tipuri numite. ModelCheck probează structural regula
(`tools/ModelCheck/ProbeStraturi.cs`), împreună cu coaja comenzii: refuzul
404/403 vine înaintea oricărui context, iar comenzile nu primesc
`IObjectSpace`. (104b, 104f, 104g, 104h)

Un câmp intră în baza comună numai dacă are aceeași semantică pentru toate
tipurile care îl folosesc și este necesar direct postării. O valoare necesară
motorului poate fi furnizată printr-un contract; nu cere automat o coloană
pe bază. (2, 54c)

Furnizorul și clientul sunt roluri ale aceluiași `Partener`. Calitățile
transversale ale repartitorilor, precum loc de consum, comisie sau centru
de cost, se exprimă prin `Calitati`. Se folosește o clasă derivată când
identitatea este exclusivă și schema diferă. (16)

## Registre și ciclu de viață

Faptele operate stau numai în cub (`Tranzactie`, `Postare`); forma lor e la
„Cubul persistat". Tabelele de registru dinaintea cubului nu există în model
(entități, mapare, liste XAF, cazuri de gardian), iar numele lor sunt
interzise în sursă. (D9-D6, D9-D7 b)

Cubul se scrie numai prin motor și nu se editează prin CRUD; o postare
existentă nu se rescrie (`POSTARE_MODIFICATA`, la salvare, pe orice ușă).
Efectele proprii ale unei frunze la comenzile de stare — starea fișei de
imobilizare și refuzurile ei de dependență — trec prin
`IDocumentCuEfecteProprii` (`LaOperare`, `LaAnulare`, `LaStornare`), chemat de
motor prin interfață; dependențele dintre faptele aceleiași fișe le refuză
tipul, nu motorul. (87c, D9-D2, D9-A5, D9-A6)

Un fapt operat se citește cu valorile și coordonatele deja rezolvate.
Excepția scrierii directe pentru migrare este deschiderea contabilă/de stoc,
marcată prin `DocumentId = null`; rândurile ei sunt brute per (cont, latură),
scrise din aceleași controale ca tranzacția `Deschidere` a cubului, pe care
conectorul o scrie prin comanda motorului înaintea lor. (14, 25e, 40d, 107 a/b)

Stările sunt `Draft`, `Operat` și `Stornat`. Documentul și liniile sale sunt
editabile în Draft. Starea originală din persistență este autoritatea
gardianului de editare; un formular vechi nu redeschide dreptul de scriere. (14, 55a)

### Regimul pe stare

Editabilitatea și comenzile disponibile ale unui document au o singură
sursă: `Api/RegimDocument.Calculeaza`, în coaja comenzii. Regimul e onest:
pentru fiecare comandă din vocabular întoarce motivul indisponibilității
sau nimic, cu aceleași predicate ca gardienii motorului. Draft: editabil;
Operează, Validează, Șterge. Operat: needitabil; motivele pentru Anulează,
Stornează și Corectează le compune `Motor/GardieniRetragere.Citeste`, în
ordinea motorului, din predicatele pe care comanda le aruncă. Anularea are
data fixă și își citește toți gardienii: TVA deja declarată, perioada
înregistrării, latura pereche, conexele operate, orice împerechere, loturile
proprii folosite de alte documente, dependenții proprii ai frunzei (PIF,
CAS, AMO), recepția activă, nominalizările active pe partidele și pe suportul
documentului, stingerea unei partide inițiale. Stornarea și corecția își aleg
data la comandă, deci regimul refuză numai ce refuză la orice dată: latura
pereche, conexele, împerecherea vie dintr-o perioadă deschisă (cele din
perioade închise le inversează stornoul), dependenții proprii ai frunzei,
luna închisă a documentului de imobilizări, recepția activă, nominalizările
cu net rămas nenul. Stornat: nimic. Ștergerea se decide numai pe stare
(Draft), fără hook de tip (106j). Tipul contribuie comenzile proprii prin
`Document.ContribuieRegim`, în vocabularul închis
`RegimDocument.ComenziCunoscute`: NTC → Stinge pe Operat; AMO, ITV și IRZ →
Regenerează pe Draft; ASM → Distribuie pe Draft cu linii de consum și de
produs; FCL → Generează descărcarea pe Operat, cu gestiune de descărcare și
rest nedescărcat. Adaptorii (`Api/*Apply`, controllerele XAF) randează
regimul și nu îl re-derivă din `Stare`. Regimul promite starea, dependenții
și perioada; refuzurile care depind de valori (sold, validări), de data
cerută la stornare și `SCRIERE_OCUPATA` apar numai la comandă, iar citirea
regimului nu ia blocajul scrierii. Un document operat fără dependenți se
citește în 10–12 instrucțiuni mărginite pe ID. Proba: catalogul `REGIM`,
conformitatea regim ↔ comandă pe tot catalogul (SC-X-24) și probele
structurale din ModelCheck. (106b, 106c, 106e, 106i, 106k)

### Data documentului și data înregistrării

Documentul poartă două date. `Data` este a documentului fizic: numerotarea,
scadența, cronologia seriilor proprii și identitatea fiscală rămân pe ea.
`DataInregistrare` este data la care documentul intră în evidență. (F27-D4)

- Postările contabile, de stoc și de imobilizări, precum și lotul născut din
  liniile documentului, se scriu la data înregistrării. Faptul fiscal își
  păstrează data documentului; perioada în care faptul
  se declară este o coordonată separată pe rândul fiscal, decisă de politică
  atunci când perioada faptului e închisă. Regulile ei sunt în
  [tva](tva.md#perioada-de-declarare-și-corecțiile). (F27-D4, F27-D5)
- Gardianul de perioadă întreabă despre perioada datei înregistrării, la
  operare și la anulare. Un document cu data fizică într-o perioadă închisă și
  data înregistrării în cea deschisă se operează: documentul întârziat este
  flux normal, nu excepție, și nu atinge soldurile perioadei închise. (F27-D4)
- Ordinea FIFO este ordinea intrării în evidență, fiindcă lotul se naște la
  data înregistrării. Este singura ordine compatibilă cu „sold ≥ 0 la orice
  dată”. (13, F27-D4)
- La aceeași dată de deschidere, FIFO consumă întâi unitatea deschisă de
  documentul creat mai devreme: data, apoi ID-ul documentului deschizător,
  apoi identificatorul unității. Unitatea fără document deschizător (partida
  inițială, lotul fără linie de intrare) vine prima la data ei. La loturi,
  între document și identificator stă poziția liniei de intrare. ID-ul
  documentului e Guid v7 și ține ordinea creării între tranzacții; liniile
  și loturile scrise în aceeași tranzacție n-au ordine în ID, deci
  identificatorul unității rămâne numai ultimul criteriu. Ordonarea loturilor
  e una singură, `Cub.Citiri.Loturi.InOrdineFifo`; a partidelor o face
  nucleul, care primește documentul pe candidat (`Disponibil.Origine`). (114)
- Postarea își poartă ordinalul: poziția ei în tranzacția care a scris-o, de
  la 1, fără goluri (`Postare.Ordinal`, dat de `Randuri.Scrie`). Nu e
  coordonată: nu intră în sold, în grupări sau în identitatea unității. Baza
  nu-i dă valoare implicită, iar lista de evidență îl arată. ID-urile
  postărilor nu poartă ordine. (117)
- Între partidele fără document deschizător, înaintea identificatorului
  departajează ordinalul postării de deschidere (`Disponibil.Deschidere`):
  două partide inițiale născute în aceeași zi se consumă în ordinea în care
  le-a primit deschiderea. Cine deschide trebuie deci să le dea în aceeași
  ordine la fiecare rulare. Loturile inițiale nu sunt acoperite: la aceeași
  dată se departajează pe identificatorul lotului. (116, 117)
- Partida inițială își poartă data la care s-a născut la sursă
  (`PartidaInitiala.Deschisa`); fără ea, data deschiderii. Data nu poate urma
  deschiderii și nu intră în identificatorul partidei. FIFO consumă deci
  partidele inițiale în ordinea vechimii, ca loturile inițiale. (115)
- Data stornării nu poate preceda data înregistrării. Pentru documentele de
  imobilizări stornoul se cere în luna înregistrării. (25d, 87g, F27-D4)
- Data înregistrării nu poate preceda data documentului. Regula este scrisă în
  gardianul de editare, în adaptorul de scriere al API-ului și în motor: căile
  standalone nu trec prin gardianul de Committing. (F27-D4)
- Implicitul este data documentului și se aplică la seam-uri, nu în setter: la
  creare și la schimbarea datei în ecranul XAF cât timp cele două erau egale,
  în adaptorul de scriere când clientul nu trimite câmpul, iar în motor ca
  normalizare pentru orice cale care nu-l culege (Import1C, generate).
  Postările unui document fără câmp cules cad pe data documentului. (F27-D4)
- Documentul conex și cele secundare care copiază data sursei moștenesc și data
  înregistrării ei. Documentele generate (amortizarea lunară, închiderea de TVA,
  închiderea conturilor de rezultat, descărcarea de gestiune) o primesc egală cu data lor chiar la creare, nu abia
  la operare. (17, F27-D4, F27-r9)
- Plata autogenerată din factura de intrare are data ei proprie (ziua plății
  culese), dar data înregistrării nu poate precede intrarea facturii în
  evidență: primește `max(ziua plății, data înregistrării facturii)`. Altfel
  o factură înregistrată mai târziu decât ziua plății ar fi blocat operarea
  plății ei, fiindcă împerecherea automată nu poate fi datată sub înregistrarea
  vreunuia dintre documente. (review advers F27, 3e)

### Culegerea draftului

Culegerea are o singură sursă, `Culegere/CulegereDocument` (L3). Controllerul
XAF (`CulegereDocumentController`, geamănul pe linie) și `Api/*Apply` sunt
adaptori: traduc evenimentele ecranului, respectiv PUT-ul agregatului, în
aceleași apeluri. (104c)

În XAF linia se editează în tabul ei, cu spațiul ei de obiecte; salvarea de
acolo normalizează tot documentul. Reîncărcarea unui obiect nu e culegere:
adaptorul nu recalculează pe evenimentele ei. Când o linie a documentului e
reîncărcată, tabul documentului reîncarcă și liniile surori nemodificate de
el, deci arată taxele și totalul salvate și poate opera fără conflict de
versiune. Tabul liniei care nu a fost deschis din grila documentului nu
declanșează reîncărcarea (109-r4). (109 f)

- Documentul nou primește data de azi, dacă apelantul nu i-a dat-o. Data
  înregistrării urmează data documentului cât timp erau egale. (F27-D4)
- La alegerea produsului, linia primește tipul (cont/clasă) din
  `Produs.TipMaterial`, dacă îl lipsește. Primește și tipul de TVA implicit,
  dacă îl lipsește și dacă formula tipului poartă TVA (`Document.CuTva()`:
  FCT, FCL, DEC, RLF, RDC, DVI). Linia nouă rămasă fără ele le primește la
  salvare. Implicitul se rezolvă pe partener → politică → ancoră, împăcat cu
  cota produsului. (F23-D2, 104c)
- Formula valorii liniei e una singură, pe entitate:
  `Document.CalculeazaValori`, `BazaLinie` și `CalculeazaLinie`. Culegerea o
  cheamă ca previzualizare, operarea ca autoritate; `PregatesteOperare`
  adaugă doar semnul (LDI, ASM, RLF, RDC). Schimbarea unei intrări a bazei
  (`Document.IntrariBaza()`) recalculează valorile și șterge TVA-ul cules pe
  baza veche. Salvarea recalculează păstrând TVA-ul cules nenul. (36a, 48b)
- TVA-ul cules e o intenție explicită a adaptorului. Nu poate fi negativ;
  pe RLF și RDC culegerea e în magnitudine, iar semnul îl pune operarea
  (`SemnulEAlOperarii()`). Se acceptă numai pe regimurile Normal și Taxare
  inversă. Pe livrare, taxarea inversă nu poartă TVA. API-ul verifică regula
  înaintea atribuirii, deci refuzul nu lasă valoarea în context. Salvarea o
  verifică înaintea recalculului, care altfel ar șterge valoarea în tăcere.
  (36a, F13-D1)
- 0 nu e TVA cules. Pe API, un 0 explicit e refuzat când cota tipului de TVA
  dă taxă nenulă; lipsa taxei se alege prin tipul de TVA (scutit,
  neimpozabil). În XAF, TVA-ul adus la 0 revine vizibil la cotă. Câmpul
  `ValoareTva` al DVI e ne-nullable pe sârmă, deci acolo 0 înseamnă necules.
  (48b, 104c)
- La salvare, pe orice ușă, `CulegereDocument.InainteDeSalvare` rulează
  înaintea gardianului. Pentru fiecare document Draft atins face:
  normalizarea tipului (câmpurile celeilalte direcții golite pe LDI și ASM,
  produsul liniei conexe NIR, imputatul NIR, liniile CAS refăcute din fișe la
  data înregistrării, cu refuz pe fișa al cărei tip n-are politică de
  amortizare), precompletările rămase, loturile
  (`LoturiCulegereService`) și valorile. În XAF pasul e
  `CulegereLaCommitXaf`, înregistrat în hostul Blazor înaintea gardianului;
  `Apply` îl cheamă explicit înaintea commit-ului. (104c)
- Validarea culegerii e a gardianului de commit (`GardianEditare`): scara
  coloanei, tipul liniei obligatoriu,
  valoarea în vamă nenegativă și rolul liniei RDC (venit fără lot, marfă cu
  lot), care nu se schimbă. Conservarea, starea, perioada și condițiile
  dependente de date concurente rămân ale motorului, în tranzacția
  comenzii. (42a, 104c)
- `DataPrimire` goală înseamnă data înregistrării. Motorul o rezolvă la
  postare, iar DTO-ul de citire o arată astfel. (F27-D4, 104c)

### Operarea

1. Culegerea se salvează prin ușa securizată. (42b)
2. Coaja comenzii (`Api/ComenziDocument`) primește ID-ul documentului și
   dreptul operatorului (`IDreptComanda`). Verifică dreptul înaintea oricărei
   atingeri a domeniului: documentul invizibil pe ușa cerută e 404
   (`SubiectInvizibil`), fără Write pe instanță e 403 (`RefuzAcces`), iar
   corecția mai cere Create și Write pe tipul concret. `DreptComandaXaf`
   rezolvă documentul prin ușa securizată a utilizatorului autentificat și
   servește XAF-ul și WebApi-ul. (55b, 80b, 104b)
3. Coaja își deschide singură contextul: un ObjectSpace non-secured propriu
   fiecărei comenzi, eliberat după ea. Tranzacția comenzii ține blocarea
   perioadei peste `CommitChanges`. Ușa de sistem
   (`ComenziDocument.Sistem(os)`) e a uneltelor standalone (ModelCheck):
   primește contextul apelantului și nu verifică drepturi. (42b, F27-D1, 104-r2)
4. Verifică starea, data și perioada. Refuză tipul care nu declară sau n-are
   politică pe profil (`TIP_FARA_DECLARATIE`), înaintea oricărei validări a
   frunzei. Pregătește documentul (`PregatesteOperare`) și rulează validările
   care nu țin de postări: TVA cules la taxare inversă, `PoliticaValidare`,
   `ValideazaOperare`. (33d, D9-D2, D9-D5)
5. Contractează declarația frunzei și rulează gardurile de pe postările ei:
   analiza obligatorie, soldul intermediar al loturilor, poziția fără fișă,
   recepțiile conexe. Până aici nimic nu e scris. (D9-D2, D9-D4)
6. Scrie pe document numărul și scadența implicite, finalizează loturile
   născute, aplică efectele proprii ale frunzei și creează documentele
   generate. Asignează `Operat` și `DataOperare`. (25c, 25f, 26d)
7. Materializează contractul în cub, cu explicația lui, și scrie pe liniile
   ieșirilor evaluate din sold valoarea decisă de contract. Materializează
   stingerea automată și comite. (82c, 108c, D9-D3)

Validările și contractul preced orice scriere pe document. Validările relațiilor
create pot încă refuza operația înainte de commit. La refuz, ObjectSpace-ul
comenzii se abandonează: lipsa commit-ului nu înseamnă că instanțele din
memorie au rămas nemodificate. (33d, 82)

`Valideaza` folosește aceeași cale de calcul și validare ca operarea, fără
commit. Poate modifica obiecte în memorie, deci cere un ObjectSpace propriu,
de unică folosință. Întoarce numai refuzurile: valorile lăsate pe liniile din
memorie sunt estimări, nu rezultat (D9-D3). `MesajeDupaOperare` este doar informare după commit și
nu trebuie să transforme o operație reușită într-un eșec aparent. (55b, 76c, 82c)

### Anulare și storno

Stornoul și anularea urmăresc postările existente ale documentului.

- Anularea operării readuce documentul în Draft și șterge tranzacțiile și
  postările lui. Este permisă numai în perioadă deschisă și fără dependenți. (14, 25d)
- Eliminarea mișcărilor proprii nu poate produce sold intermediar negativ.
  Un lot creat de document nu poate fi deja folosit de alt document. (25d)
- Copiii operați, laturile pereche operate care îl referă și împerecherile
  existente blochează corecțiile conform relațiilor lor. (26d, 31d, 65)
- Drafturile autogenerate ale sursei se elimină la anularea sursei. (26d)
- Stornoul adaugă o tranzacție `Storno` la data stornării, cu postările
  originalului la valori negative; nu inversează laturile. (25d, 46a)
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
  și FK-urile mapate ale tipului concret (baza și frunza), pe antet și pe
  linii. Nu se copiază identitatea (`ID`), discriminatorul (`ClrType`, scris
  de EF), câmpurile motorului (`Stare`, `DataOperare`, `Autogenerat`,
  `DocumentSursaId`), datele proprii corecției (`DataInregistrare`,
  `CorecteazaId`, `MotivCorectie`) și câmpurile de infrastructură ale lui
  `BaseObject`. (89f)
- Lotul: linia care a NĂSCUT un lot (`Lot.LinieIntrareId == linia`) primește
  pe copie un lot PROPRIU, nou și nefinalizat, pe care motorul îl finalizează
  la operare (preț, dată). Linia care doar CONSUMĂ un lot îl păstrează prin
  `LotId`, copiat ca orice FK. (26e)
- Un original nu se corectează de două ori, iar un document care nu e operat
  nu se corectează deloc.
- Perioada închisă nu se atinge: storno-ul și documentul nou trăiesc în
  fereastra deschisă, iar snapshot-ul perioadei rămâne cel de la închidere.
  Efectul FISCAL al motivului e în
  [tva](tva.md#perioada-de-declarare-și-corecțiile).
- Comanda: `Motor/CorectieService.cs`, prin `Api/ComenziDocument.Corecteaza`;
  ușile sunt `POST api/documente/{id}/corecteaza` și acțiunea XAF
  „Corectează" de pe orice DetailView de document.

## Tipuri de document

| Cod / tip | Regula specifică |
|---|---|
| FCT — factură de intrare | Numărul furnizorului este cules. Pentru stoc, naște lotul și generează NIR; postează liniile care nu trec pe NIR și TVA-ul propriu. Poate genera o plată draft din datele culese. (26a, 31e, 56) |
| NIR — recepție | Manual: recepție integrală, partidă după `UrmarestePartide`. Conex: delta față de recepția deja postată de FCT; proveniența istorică se păstrează la corecție, inclusiv la delta zero. Cauza diferenței decide contrapartida prin politică; imputarea cere partener. O singură recepție activă cumulativă per FCT, linii-sursă păstrate (zero permis), lotul nu se schimbă. Nu culege TVA. (098, 099, NIR-D1…D6) |
| FCL — factură de ieșire | Postează venitul și creanța. În privat poate genera DSC; în bugetar regulile o restrâng la document fără stoc. Numărul fiscal este al serverului. (30a, 30b, 56) |
| DSC — descărcare | Generat de serviciu din FCL, cu `LinieSursaId`, la cost, fără TVA; gestiune → client, cu ambele dimensiuni de repartitor pe gestiune. Clientul oferă citire și comenzi, fără creare manuală. (37a, 37b, 58) |
| BTR — transfer | Mută stocul între gestiuni. Transferul simplu nu postează note contabile în planul sintetic. (23c) |
| BCS — bon de consum | Scade Magazie (Folosinta pentru OF bugetar, 093) de la predator și crește Consum la primitor; consumul rămâne pe responsabil. Valoarea se derivă din lot. (27a, 27d) |
| LDI — diferențe inventar | Direcție explicită Plus/Minus, culegere pozitivă, semn la materializare. Plusul naște lot pe gestiunea predatorului; minusul cere lot existent, străin de document. Primitorul are calitatea Comisie. Declarant pe cub pentru Magazie/Marfuri/Folosinta: plus contra Inventar, minus contra Consum, contraponderi fără unitate. Folosinta păstrează gestiunea reală pe lanțul FCT/NIR/BTR/BCS/LDI; Custodie explicit neacoperită. (093) (28a, 28d, 63, LDI-B1…B3) |
| PLT / INC — plată / încasare | Valoarea se culege pe linii ca defalcare. Contarea se rezolvă din laturi. PLT: cont propriu → beneficiar; INC: plătitor → cont propriu. Pot exprima și picioarele unui virament intern. (31a, 31c, 64) |
| DEC — decont | Angajat → repartitor intern, fără stoc. Contractul permite cont și repartitor explicite pe linie. Cantitatea pro-formă zero se normalizează la unu. (32a, 32b, 32d) |
| NTC — notă contabilă | Postare explicită. Poate stinge manual pe contrapartidă și sens; nu se înscrie implicit în stingerea automată a sursei. ITV și IRZ nu trec prin ușa NTC (listă, citire, comenzi). (46b, 79c, 82a, E3-D3) |
| ITV — închidere TVA | Rezultatul serviciului lunar, cu conturi din politică. Nu se culege ca agregat liber și nu închide perioada fiscală. (46c, 79a) |
| IRZ — închiderea conturilor de rezultat | Notă generată pe tiparul ITV, datată în ultima zi a lunii, pe o unitate internă; cel mult una vie pe lună. O linie per cheie-sursă cu sold nenul la sfârșitul lunii (contul plus coordonatele soldului), închisă în sensul opus soldului pe destinația rândului de politică. Sursa poartă cheia completă; destinația păstrează repartitorul și coordonatele declarate de rând, restul se pliază; coordonatele fiscale nu trec. Operarea refuză draftul depășit (soldurile sau politica s-au schimbat) și orice închidere când o lună ulterioară e închisă operat. Nu se culege liber; fără flux `/nou`. (E3-D3, E3-D4) |
| DVI — declarație vamală de import | Linii pe detaliul de bază: valoarea în vamă ca bază, taxa declarată (0 = din cotă la operare). Nu postează valoarea și nu mișcă stocul; postează doar taxa din politica TVA (4426 contra contului implicit al predatorului — biroul vamal/comisionarul — sau 4426 = 4427 la amânarea plății). MRN cules, fără numerotare. Legătura n→m cu facturile de import este evidență, doar în Draft, prin agregat. Nu este document stins: taxa se plătește ca orice taxă, fără împerechere. (86a, 86b, 86e, 86g) |
| ASM — asamblare/dezasamblare | Transformare n→m cu linii de produs și consum. Gardul valoric e ΣP = ΣC: produsele culese contra consumului evaluat pe cub (`ASAMBLARE_NEBALANSATA`, cu ambele sume). Grupurile pe cont cu P = C intră în Transfer, restul în Operare, cu contraponderi cantitative Transformare; nu există absorbție. `DistribuieValoarea` aduce produsele la consumul evaluat de declarant pe operandul draftului (D9-D3). Nu consumă un lot produs de același document. (46d, ASM-B2…B7) |
| RLF — retur la furnizor | Folosește lotul original; culegere pozitivă, postare cu semn negativ pe corespondența originală. (46e, 76d) |
| RDC — retur de la client | Un document cu linii de venit și cost pe lotul original. Totalul include doar venitul; linia de cost nu are tip TVA. Rolul unei linii salvate nu se convertește prin editare. (46e, 76d) |
| PIF — punere în funcțiune | Unitate internă → loc; linii per fișă cu `Intrare`, `Modernizare` sau `Revizuire`. Nominalizează valoarea contabilă existentă pe fișă prin Transfer, cu suport obligatoriu și fără modificarea rulajelor generale; baza fiscală se postează distinct (097). Materializează starea fișei (în funcțiune, data punerii în funcțiune); valorile ei se citesc din cub (097). `Intrare` cere fișă nouă și parametri completi, cu linie sursă (linia unei FCT operate de clasă F, cu plafonul consumului) sau cu valoare culeasă și inițiale; `Modernizare`/`Revizuire` cer fișă în funcțiune. Refuzat dacă o AMO operată există într-o lună ulterioară. (87e) |
| CAS — ieșire de imobilizare | Loc → unitate internă, cu cauza (casare, vânzare, lipsă). Se culeg doar fișele; liniile le produce serverul din situația la dată și politica tipului material: amortizarea cumulată contra contului imobilizării (omisă la cumulat zero) și valoarea rămasă pe cheltuiala de cedare (omisă la net zero). Operarea recalculează și refuză liniile care nu mai corespund; refuzată dacă o AMO operată acoperă luna ieșirii sau una ulterioară; după operare avertizează dacă luna precedentă n-are amortizare operată. Fișa devine ieșită. (87f) |
| AMO — amortizare lunară | Document generat pe unitate internă și lună, ca ITV. Linie per fișă eligibilă cu trei cifre (contabilă, fiscală, deductibilă); postează separat contabil și fiscal (cheltuială din politică = amortizare pe contul nominalizat), cu unitate de fișă (095, 097). Nu se culege liber; fără flux `/nou`. (87g) |

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

<!-- gen:decizii -->
## Decizii

Deciziile care numesc pagina în `Arii:` (54; titlurile în [index](../decizii/README.md); generat de `docskit gen`, nu se editează): 2, 3 (amendată de 89), 13, 14, 16 (amendată de 89), 17, 20, 22, 23, 25, 26 (amendată de 112), 27, 28, 30, 31 (amendată de 76, 88), 32, 33, 36 (amendată de 71), 37, 38 (amendată de 81, 84), 40, 42 (amendată de 104, 106), 46 (amendată de 113), 48 (amendată de 76), 54, 55, 56 (amendată de 77, 81), 58, 63, 64 (amendată de 84), 65, 76, 79 (amendată de 80), 80, 81, 82, 86, 87 (amendată de 97), 89, 93, 95, 97 (amendată de 98), 98, 99, 104 (amendată de 106), 106, 108 (amendată de 110), 109, 114 (amendată de 116), 115, 116 (amendată de 117), 117, 124, 129 (propusă).
<!-- /gen:decizii -->
