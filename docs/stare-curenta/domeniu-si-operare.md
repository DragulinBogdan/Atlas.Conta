# Domeniu și operare

**Actualizat: 2026-10-05.** [Index](README.md)

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
Clasificarea celor 49 de clase e în fișierul deciziei. `ICuCheie` e cheia
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
„Cubul persistat". Cele patru tabele de registru vechi nu mai există:
entitățile, maparea, listele XAF și cazurile gardianului au dispărut atomic
la 2026-10-07 (TR-D9a, pasul 7, D9-D6), iar numele lor sunt interzise în
sursă (D9-D7 b). Cubul se scrie numai prin motor și nu se editează prin
CRUD; o postare existentă nu se rescrie (`POSTARE_MODIFICATA`, la salvare, pe
orice ușă). Efectele proprii ale unei frunze la comenzile de stare — starea
fișei de imobilizare și refuzurile ei de dependență — trec prin
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

### Regimul pe stare (106)

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
`RegimDocument.ComenziCunoscute`: NTC → Stinge pe Operat; AMO și ITV →
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
  normalizare pentru orice cale care nu-l culege (Import1C, generate).
  Postările unui document fără câmp cules cad pe data documentului. (F27-D4)
- Documentul conex și cele secundare care copiază data sursei moștenesc și data
  înregistrării ei. Documentele generate (amortizarea lunară, închiderea de TVA,
  descărcarea de gestiune) o primesc egală cu data lor chiar la creare, nu abia
  la operare. (17, F27-D4, F27-r9)
- Plata autogenerată din factura de intrare are data ei proprie (ziua plății
  culese), dar data înregistrării nu poate precede intrarea facturii în
  evidență: primește `max(ziua plății, data înregistrării facturii)`. Altfel
  o factură înregistrată mai târziu decât ziua plății ar fi blocat operarea
  plății ei, fiindcă împerecherea automată nu poate fi datată sub înregistrarea
  vreunuia dintre documente. (review advers F27, 3e)

### Culegerea draftului (104c)

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
  Efectul FISCAL al motivului e în `politici-si-fiscalitate.md`.
- Comanda: `Motor/CorectieService.cs`, prin `Api/ComenziDocument.Corecteaza`;
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
  societății, nu ale unei unități interne. Un fapt dă o singură constatare:
  documentul în lucru raportat deja de familia lui — închiderea de TVA sau
  amortizarea existente ca draft — nu se mai repetă ca `DRAFT-IN-PERIOADA` și
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
- Fiecare comandă a motorului rulează într-o tranzacție explicită, deschisă pe
  ObjectSpace-ul ei: operarea, anularea și stornarea prin adaptorul de
  operare, generarea și regenerarea închiderii de TVA și a amortizării,
  închiderea, redeschiderea și reconstrucția perioadei. Gardianul de perioadă
  citește starea lunii blocând rândul în citire, iar comenzile perioadei îl
  blochează în scriere înainte de orice calcul. (F27-D1) Tranzacția comenzii
  începe cu blocajul scrierii, deci comenzile se serializează toate între
  ele; vezi „Scrierea serială per bază”. (X-D6)

### Soldurile materializate la închidere

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
  se refuză, ca la registre. Ștergerea lor este fizică, nu amânată. (F27-D3)

### Soldul citit: referință plus rulaje

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
  (F27-D3, X-D5)
  (TR-D8 D8-B1/B5)
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
În validarea clasificației bugetare, angajamentul poate satisface cerința de
cod economic. (25f, 32c, 33a)

`Cont.DimensiuniObligatorii` se verifică pe mișcările contabile ale
contractului (`Cub/GardAnaliza`): gardul judecă numai capătul — repartitorul
lui e o singură funcție, `Citiri.Contabil.Repartitor` (partenerul piciorului
de terț sau gestiunea piciorului intern); latura documentului nu se mai
citește, iar `Document.RepartitorImplicitDebit/Credit` nu mai există.
Materialul e produsul; restul e analiza postării; angajamentul liniei ține
loc de cod economic. Mutările (BTR, PIF), transformările (ASM) și cartea
fiscală nu intră. Recepția liniei de stoc a facturii e păzită la operarea
facturii, nu a NIR-ului conex; diferența NIR-ului acoperit rămâne la gardul
recepției conexe. Pe seed niciun document din catalog nu purta repartitorul
numai prin latura documentului, deci gardul strict nu refuză nimic acceptat
înainte; refuză declarantul care uită coordonata (proba directă
`GARD-ANALIZA`). (TR-D9a: D9-D4, D9-A3, D9-A10;
`docs/nucleu/tr-d9-pas6b-repartitor.md`)

## Precizie numerică

Cantitățile folosesc `numeric(18,3)`, sumele `numeric(18,2)`, iar prețurile
`numeric(18,6)`. Convențiile sunt centralizate în `Scara`; o proprietate
decimală fără mapare explicită este refuzată la verificarea modelului. O
valoare care depășește scara coloanei e refuzată de gardianul de commit, pe
orice ușă securizată, cu numele câmpului. Nu ajunge excepție de bază. (49e, 104c)

Prețul se rotunjește cu `AwayFromZero`. Rotunjirea sumelor respectă profilul
fixat al bazei. Culegerea, motorul și raportarea folosesc aceeași convenție;
clientul nu introduce o rotunjire contabilă independentă. (42c, 51c, 52a)

## Stoc și evaluare

- Doar `ClasaProdus.Natura = Stoc` intră în regulile de stoc. Natura și tipul
  material sunt date distincte de identificarea produsului. (23b)
- Cheia soldului este `(Lot, Cont, Produs, Gestiune)`. (25d, 27b, TR-D8)
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
- Ieșirea are una din două surse de valoare, numită de declarant. Evaluată
  din sold (BCS, BTR, DSC, minusul LDI, consumul ASM): contractul o evaluează
  pe soldul cubului, în secvența liniilor, iar ultima bucată ia restul; linia
  primește la operare valoarea decisă, cu semnul cantității ei, deci linia și
  postarea sunt egale. Pe draft și la dry-run linia poartă estimarea
  `cantitate × preț de intrare`. Declarată la prețul de intrare (RLF, costul
  RDC, linia NIR-ului conex pe lot străin): `cantitate × Lot.PretUnitar`; nu
  absoarbe reziduul lotului. `Lot.PretUnitar` e prețul de intrare, scris o
  dată, la operarea liniei care naște lotul. (75a, 090 j, D9-D3)
- O operație retroactivă nu recalculează valorile unor ieșiri deja operate. (75a)

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
| NTC — notă contabilă | Postare explicită. Poate stinge manual pe contrapartidă și sens; nu se înscrie implicit în stingerea automată a sursei. ITV nu este editabil prin această felie. (46b, 79c, 82a) |
| ITV — închidere TVA | Rezultatul serviciului lunar, cu conturi din politică. Nu se culege ca agregat liber și nu închide perioada fiscală. (46c, 79a) |
| DVI — declarație vamală de import | Linii pe detaliul de bază: valoarea în vamă ca bază, taxa declarată (0 = din cotă la operare). Nu postează valoarea și nu mișcă stocul; postează doar taxa din politica TVA (4426 contra contului implicit al predatorului — biroul vamal/comisionarul — sau 4426 = 4427 la amânarea plății). MRN cules, fără numerotare. Legătura n→m cu facturile de import este evidență, doar în Draft, prin agregat. Nu este document stins: taxa se plătește ca orice taxă, fără împerechere. (86a, 86b, 86e, 86g) |
| ASM — asamblare/dezasamblare | Transformare n→m cu linii de produs și consum. Gardul valoric e ΣP = ΣC: produsele culese contra consumului evaluat pe cub (`ASAMBLARE_NEBALANSATA`, cu ambele sume). Grupurile pe cont cu P = C intră în Transfer, restul în Operare, cu contraponderi cantitative Transformare; nu există absorbție. `DistribuieValoarea` aduce produsele la consumul evaluat de declarant pe operandul draftului (D9-D3). Nu consumă un lot produs de același document. (46d, ASM-B2…B7) |
| RLF — retur la furnizor | Folosește lotul original; culegere pozitivă, postare cu semn negativ pe corespondența originală. (46e, 76d) |
| RDC — retur de la client | Un document cu linii de venit și cost pe lotul original. Totalul include doar venitul; linia de cost nu are tip TVA. Rolul unei linii salvate nu se convertește prin editare. (46e, 76d) |
| PIF — punere în funcțiune | Unitate internă → loc; linii per fișă cu `Intrare`, `Modernizare` sau `Revizuire`. Nominalizează valoarea contabilă existentă pe fișă prin Transfer, cu suport obligatoriu și fără modificarea rulajelor generale; baza fiscală se postează distinct (097). Scrie dual registrul imobilizărilor până la TR-D9 și materializează starea fișei. `Intrare` cere fișă nouă și parametri completi, cu linie sursă (linia unei FCT operate de clasă F, cu plafonul consumului) sau cu valoare culeasă și inițiale; `Modernizare`/`Revizuire` cer fișă în funcțiune. Refuzat dacă o AMO operată există într-o lună ulterioară. (87e) |
| CAS — ieșire de imobilizare | Loc → unitate internă, cu cauza (casare, vânzare, lipsă). Se culeg doar fișele; liniile le produce serverul din situația la dată și politica tipului material: amortizarea cumulată contra contului imobilizării (omisă la cumulat zero) și valoarea rămasă pe cheltuiala de cedare (omisă la net zero). Operarea recalculează și refuză liniile care nu mai corespund; refuzată dacă o AMO operată acoperă luna ieșirii sau una ulterioară; după operare avertizează dacă luna precedentă n-are amortizare operată. Fișa devine ieșită. (87f) |
| AMO — amortizare lunară | Document generat pe unitate internă și lună, ca ITV. Linie per fișă eligibilă cu trei cifre (contabilă, fiscală, deductibilă); postează separat contabil și fiscal (cheltuială din politică = amortizare pe contul nominalizat), cu unitate de fișă; păstrează scrierea duală a rândului lunar până la TR-D9 (095, 097). Nu se culege liber; fără flux `/nou`. (87g) |

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
denumire, tip material de clasă F (contul implicit furnizează prima
nominalizare), clasificare opțională din catalog, loc (repartitorul notelor),
centru de cost, responsabil, cod economic (dimensiunea bugetară a
cheltuielii) și starea materializată de motor: nouă, în funcțiune, ieșită,
cu datele punerii în funcțiune și ieșirii. Metoda, durata, valoarea
reziduală, categoria fiscală și valoarea nu sunt pe fișă: sunt fapte datate
în registru, scrise de documente. Parametrii curenți sunt proiecția
ultimului eveniment. (87a)

Gardianul fișei: tipul material este de clasă de imobilizări și se schimbă
doar cât fișa este nouă; locul și codul economic cât este nouă sau în
funcțiune; nimic pe fișa ieșită; ștergerea doar pe fișa nouă, fără postări
care o au ca unitate și fără linii de documente care o poartă, chiar în Draft;
starea și datele le scrie doar motorul. Transferul este schimbarea locului pe fișă, fără
document: următoarea amortizare postează pe noul loc, istoricul locului
este pe rândurile lunare. (87a, 87h)

Situația fișei, AMO/CAS și API Imo citesc aceeași intrare
`Cub/Citiri/Imobilizari`: sume pe fișă și carte, parametri din evenimentele
vii identificate prin cauză. Amortizarea fiscală postează în Carte=Fiscal;
deductibilul și lunile rămân atribute istorice ale liniei AMO. Invarianții
refuză fișa fără cauză, fără suport ori fără originea inversei. (095, 097,
102d)

Conturile nominalizate ale activului și amortizării se citesc pe set din
cub, prin aceeași intrare pentru generatorul AMO, CAS și declarant. Politica
furnizează numai prima nominalizare a fiecărui cont; cheltuielile rămân din
politică. PIF fără linie FCT consumă exclusiv deschidere/NTC operată.
Poziția anonimă contabilă pe cont × dimensiuni nu poate deveni negativă,
nici la o dată viitoare, la operare, anulare, storno/corecție sau deschidere.
Protecția include conturile istorice după schimbarea politicii și se
serializează cu PIF. Documentele fără suport/fișă nu iau blocajul IMO.
Stornoul PIF eliberează suportul de la data lui; nu permite anularea
sursei dacă aceasta ar șterge suportul unui interval istoric. (098b/c)

Aritmetica este exclusiv în `AmortizareService`, ca funcție pură aplicată
de trei ori pe lună. Cota liniară este valoarea de amortizat împărțită la
lunile rămase, rotunjită la bani, fixată la ultimul eveniment; suma lunară
este minimul dintre cotă și rest; rotunjirea în jos poate lăsa o lună
suplimentară pentru rest (F26-D7/F27-D4). Baza
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
funcțiune, fiindcă fișa se postează la data înregistrării. Cota lunii este
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

Iterația celor n luni pleacă de la situația fișei la ultimul eveniment, nu de
la situația fiecărei luni recuperate: o fișă reevaluată sau modernizată în
intervalul recuperat primește pe toate cele n luni cota de DUPĂ eveniment,
inclusiv pe lunile dinaintea lui. Aproximare declarată — recuperarea e rară,
iar un eveniment în interiorul ei și mai rar. (review advers F27, L5)

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

Desfacerea este acceptată și pe o împerechere din fereastra **deschisă**, unde
ștergerea ar fi fost suficientă: alegerea este deliberată — rândul invers e
mereu legitim și algebric corect, iar un refuz ar fi cerut apelantului să
cunoască granița perioadei înaintea comenzii. Rezultatul rămâne două rânduri
care se anulează, în loc de niciunul. (review advers F27, L4)

Perechea original ↔ invers nu se mai poate desface prin ștergere: ștergerea
originalului unei împerecheri desfăcute și ștergerea unui rând invers sunt
refuzate explicit de gardian, cu textul lor. Altfel jumătatea rămasă ar fi
înviat cu semnul ei, mutând tăcut restul ambelor documente. (review advers
F27, L3)

Crearea manuală din XAF este o **comandă**, nu culegere: acțiunea
„Împerechează” de pe lista de împerecheri ia parametrii într-un dialog și
rulează `ImperechereService.Imperecheaza` pe ObjectSpace non-secured, după
gate-ul de creare pe tip. `New` este retras, iar ecranele împerecherii sunt
read-only. Motivul e cursa: gardianul de Committing citește perioada în
autocommit, deci între validarea lui și `SaveChanges` o închidere se poate
strecura, iar comanda ia rândul perioadei sub `FOR UPDATE` în tranzacția ei.
Ștergerea din fereastra deschisă rămâne pe ușa securizată. (review advers
F27, 1c)

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
plafonul stingătorului oferă un singur sens. Un tip cu altă formulă a
restului nu intră pe rolul de document stins. (86g)

Totalul de stins vine din cub: suma netelor pe unitățile de partidă proprii ale
documentului, în sensul lui de stins (`SensDeStins`). Pentru Datorie se adună
netele creditoare, pentru Creanță netele debitoare; fără sens declarat, Σ |net|
(`Cub.Citiri.Partide.Total`). Restul (`Partide.Ramas`) folosește aceeași
selecție pe soldurile curente ale partidelor proprii, iar asignatul este total −
rest. Pe o factură cu avans, datoria 401 și creanța 4091 rămân partide
distincte (SC-NIR-37). Rămân în afara lui: taxa autolichidată
(`TaxareInversa`, 4426 = 4427, SC-FCT-10), contul explicit fără partide
(SC-DEC-10) și creanța avansului de pe aceeași factură (SC-NIR-30/avans).
Plata autogenerată a FCT preia pe linie valoarea datorată terțului
(`TvaService.DatoratTertului`, SC-FCT-10). (102)

Totalul se citește din cub: `ImperechereService.Total` și coloana „Total" din
`DocumenteCuRest` folosesc aceeași formulă. Documentul nu mai are coloană de
total stins: a dispărut la pasul 7 al TR-D9a. (F27-D7, 102, D9-D10)

### Partide deschise

`PartidaDeschisa` este snapshot-ul pe unitate × cont × partener, scris din
cub în tranzacția închiderii. `DocumenteCuRest` citește resturile din aceeași
intrare comună; nu scade din nou sumele legăturilor. Regulile complete sunt
în §„Partide: raport, snapshot și împerechere” de mai jos. (101, F27-D7 amendat)

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

## Nucleul pur și declarația fluxului (pilot BCS, PLT, FCT)

`nou/Atlas.Conta.Nucleu` ține tipurile cubului și regulile pure ale deciziei
90, fără nicio referință la EF/XAF/HTTP; singurul lui consumator e
`Atlas.Conta.BackOffice.Module` (folderul `Declaratii/`, TR-D6b). Motorul de
azi (`Motor/MotorOperare`) rămâne singurul care SCRIE: declaranții rulează
alături de el, ca probă în ModelCheck, și nu persistă nimic până la TR-D7.
Ce ține nucleul (contractul `docs/nucleu/tr-d6a-nucleu-pur-contract.md`):

- **Cubul**: `Coordonate` (cont, latură OBLIGATORIE, dată, partener,
  gestiune, produs, unitate, cod TVA compus, perioadă de declarare, valută,
  carte, analiză ×6), `Postare` cu trei măsuri la scară apărată în
  constructor și în `with`, `Tranzactie` cu felul `Operare | Storno |
  Transfer | Deschidere`; spațiul (`Stoc` = unitate de tip lot, restul
  `Contabil`) e funcție, nu câmp. Jurnalul TVA e proiecție pe `CodTva`, nu
  spațiu separat. (N-D2)
- **Conservarea**, structurală: valoarea per `Carte`, cantitatea per produs
  peste TOATE postările (capătul virtual al cantității stă pe postarea de
  terț, cu gestiune virtuală `GestiuniVirtuale.Furnizor/Client/Consum` —
  constante deterministe ale nucleului, CONFIRMAT de pilotul FCT: invizibil
  proiecțiilor pe gestiune reală și pe unitate-lot, N-r2), transferul cu
  Σ = 0 per (cont, latură), valoarea negativă doar în `Storno`/`Transfer`,
  formele (cantitate ⇒ gestiune, produs și unitate — cu excepția gestiunii
  VIRTUALE, unde cantitatea nu cere unitate, fiindcă pe un profil fără
  `RolTert` capătul virtual n-are partidă;
  lot ⇒ produs și gestiune; partidă ⇒ partener; unitatea pe contul, produsul
  și partenerul postării; postările datate ca tranzacția; deschiderea fără
  document, scutită de Σ). (N-D3, N-D4)
- **Perechea** (`Postare.Pereche`, D9-A2): ordinalul mișcării sau mutării în
  tranzacția ei, dat de `Motor` (mișcările 1…m în `Operare`, mutările 1…k în
  `Transfer`); transformările și deschiderea n-au ordinal. `Conservare`
  (`PERECHE_INVALIDA`) cere pe fiecare ordinal nenul exact două postări cu
  aceeași cauză și cantități opuse: pe laturi opuse cu aceeași valoare
  (`Operare`), pe aceeași latură cu valori opuse (`Transfer`); stornoul
  acceptă ambele forme. Nu e coordonată: nu intră în chei de sold și
  snapshot-uri; cititorii n-o folosesc încă (D9-r4).
- **Unitatea** (lot = partidă = fișă): raportul = cost / curs / valoare
  rămasă ca citire; partida deschisă de un document are id determinist din
  (document, cont, partener), conform 092; identitățile istorice sunt păstrate
  la citire și stingere, fără rescrierea postărilor. **FIFO**: unitatea numită
  pe linie se consumă întâi,
  fără cădere pe FIFO, apoi (data deschiderii, id), tolerant cu rest
  întors. **Evaluarea ieșirii** pe raportul CURENT al unității, ultima
  ieșire ia restul ⇒ cantitate zero ⇒ valoare zero; față de motorul de azi
  (preț înghețat pe lot, substituit doar la golire) diferența e declarată
  și măsurată: în teste (378 din 491 goliri lasă valoare pe cantitate zero
  sub regula veche) și pe pilotul BCS (lot cu 20 buc / 300 lei și preț
  înghețat 10: consumul de 5 buc dă 50 azi și 75 în nucleu, Δ = +25;
  documentele de azi nu pot produce două prețuri pe același lot — Δ apare
  doar din deschideri/import). (N-D6, N-D7, N-r3)
- **Repartizarea** = Hamilton ierarhic, Σ = total exact, singura primitivă
  de distribuție. **TVA**: taxa se decide și se rotunjește pe document ×
  cotă, se postează per linie prin Hamilton peste |net|, pe fiecare semn
  separat; taxa dată pe facturile primite se validează cu toleranță, nu se
  recalculează. Aceeași taxă stă pe linia documentului și pe postare: 3 × 0,01
  la 21 % ⇒ 0,01 pe o singură linie, peste tot (109). Toleranța de pilot e `0,01 × liniile cu TVA` (constantă în
  adaptor) și REFUZĂ o taxă culeasă cu abatere mai mare (o factură a scenei
  P1 cu abatere 0,10 e refuzată, unde motorul vechi operează) — devine rând
  de politică la TR-D7. (N-D8, 090j, N-r4)
- **Sold** = Σ pe orice cheie, pe tranzacții, cu `Transfer` exclus ca
  parametru al citirii; snapshot-ul e lema `Sold(≤t) = Sold(≤t0) +
  Sold(t0<d≤t)`. **Stornoul** = inversul cauzat ∪ atribuit, fără schimb de
  latură, tranzacție distinctă; contrapartida unei postări atribuite
  inversate rămâne nedefinită până la TR-D9b (N-r5). (N-D9, N-D10)
- **Motorul**: `Declaratie` (mișcări pe coordonate rezolvate + decizii +
  ipoteze) → exact o tranzacție `Operare` (sau `Transfer` din mutări pe
  același cont) → `Contract` acceptat/refuzat, determinist, cu contorul de
  jumătăți de ban al instanței `Rotunjire` primite. `Decizie`/`Ipoteza` sunt
  ierarhii închise cu cazurile pilotului. (N-D11)

### Declarația fluxului per tip (TR-D6b, felia 30)

Forma care înlocuiește hook-urile de motor ale frunzelor (contractul
`docs/nucleu/tr-d6b-declaratia-fluxului-contract.md`, B-D1…B-D10):

- **Operandul închis** (`Declaratii/Operand.cs`, DTO): documentul (laturile
  ca `RepartitorFapt` cu felul din discriminatorul `ClrType`), liniile cu
  lotul, prețul, produsul și dimensiunile culese, politica (regulile de
  contare/stoc, politica de TVA, tipurile de TVA cu conturile lor, conturile
  atinse cu `UrmarestePartide`), starea citită (soldurile loturilor la data
  înregistrării fără documentul curent, restul partidei sursei, perioada
  de declarare, perioada deschisă, toleranța taxei). Faptele de politică
  poartă contorul rândului (`OptimisticLockField`, D9-A8).
  Îl construiește `Motor/Fapte.Operand(os, doc)` PE SETURI (o interogare
  per tabelă, probat `≤ 16` cu `NumaratorSql`). Câmpurile de frunză fără
  interfață declarată (`Valuta`/`Curs` pe FCT) NU intră.
- **Declarantul** (`IDeclarant.Declara(Operand, Rotunjire, refuzuri) →
  Declaratie?`): pur, fără `IObjectSpace`, fără entități; frunza îl numește
  printr-o singură METODĂ polimorfă `Document.Declarant()` (`null` = tipul nu
  declară încă; o proprietate ar intra în metadata clientului). Driverul
  `Declaratii/Contractare.Contracteaza(os, doc)` = operand → declarant →
  `Motor.Opereaza`; `RefuzException` a primitivelor devine refuz.
  Rezolvările comune (`Contari`): contul fiecărei laturi prin
  `Potrivire.Contare/Cont` și coalesce-ul dimensiunilor prin
  `DimensiuniResolver` — aceleași funcții pure ca motorul vechi.
- **Regula coordonatelor** (B-D8 pct. 9, 10; pct. 4 amendat de D9-A10):
  capătul intern poartă `Gestiune` = repartitorul intern al documentului;
  capătul de terț poartă `Partener` + partida pe un cont cu `UrmarestePartide`
  și `Partener` singur pe un cont care cere `Repartitor`
  (`ContFapt.CereRepartitor`), și când contul nu urmărește partide. Un singur
  helper, `Declaratii/Terti.Capat`, pune ambele pe capătul de terț pe care i-l
  dă apelantul; capătul cu gestiune reală nu e de terț, capătul din cartea
  fiscală nu primește partener. Trezoreria alege piciorul de terț structural
  (predatorul dă pe credit, primitorul primește pe debit), nu după lipsa
  gestiunii; piciorul propriu fără gestiune rămâne vizibil gardului.
  Terțul capătului e latura externă a documentului; la DEC și NTC e
  repartitorul liniei, iar DEC, NTC, DSC și diferența NIR îl pun pe capătul
  extern necondiționat (T-D13 g). Convenția pozițională a notei vechi
  (debit ← predator, credit ← primitor) nu se reproduce. Coordonata nu
  deschide partidă și nu schimbă unitatea. Proba: SC-PLT-08 pe ambele
  profiluri; pe seed, 462.01.09 (bugetar) e singurul cont fără partide care
  cere repartitor și e atins de catalog.
- **BCS** (`DeclarantBonConsum`): o mișcare per linie — lotul iese de pe
  contul creditor al regulii din gestiunea predatoare și intră pe contul de
  cheltuială al locului de consum (repartitorul real, cu lotul ca unitate
  re-cheiată pe contul postării), evaluat pe raportul curent în secvența
  liniilor.
- **PLT/INC** (`DeclarantTrezorerie`, O clasă pentru ambele: diferența e în
  regula de contare și în contul cu `UrmarestePartide`): o mișcare per linie de
  defalcare; plata născută din factură numește partida sursei prin
  `Fifo.Nominalizeaza` cât ține restul ei, excedentul pe partida proprie
  ca a doua mișcare (linia se sparge, inclusiv piciorul de bani); fără sursă,
  partida proprie (avans); viramentul cu ambele capete pe contul propriu al
  documentului. Laturile sunt ale contractului clasei (T-D13: PLT
  `Propriu → Extern | Propriu`, INC `Extern | Propriu → Propriu`), verificat
  înaintea declarantului; declarantul refuză doar laturile identice.
- **FCT** (`DeclarantFacturaIntrare`): linia de stoc e RECEPȚIA facturii
  (TR-D3: `3xx` din contul implicit al tipului, gestiunea primitoare, lotul
  născut de linie, `+q`; capătul virtual `Furnizor` cu `−q` pe postarea de
  terț, N-D4); netul celorlalte naturi pe regula lor (imobilizări → 404);
  taxa se DECIDE per document × cotă — culeasă = autoritară cu toleranță,
  nedată = Hamilton per linie, scrisă pe linie la pregătire și postată de
  acolo (109); faptul fiscal e atribut al postării interne
  (`CodTva` tip × sens × rol, perioada declarării, partenerul fiscal);
  `Normal` 4426 = 401, `TaxareInversa` 4426 = 4427 (4427 fără `CodTva`: azi
  nu există fapt fiscal colectat pe TI), `Capitalizat` = bază + taxă pe
  ACELAȘI cont de cost (jurnalul rămâne proiecție), `Scutit`/`Neimpozabil`
  doar bază; o singură partidă per cont de terț. NIR-ul conex postează numai
  diferența față de recepția deja reprezentată în cubul FCT (098, 099). Plata
  autogenerată are document și postări proprii.

## Cubul persistat

RDC folosește `DeclarantReturClient`, activat numai pe profilul privat
(T-D8, pasul 4). Linia fără lot inversează venitul și TVA-ul, deschizând
partida proprie negativă; linia cu lot readuce cantitatea și valoarea
pe lotul original, fără fapt fiscal. NTC poate compensa partida returului
cu cea a facturii; nominalizarea directă pe factura originală rămâne TR-D9b.
Catalogul `docs/nucleu/scenarii/RDC.md` acoperă și stocul returnat deja
consumat, anularea/reoperarea și corecția mixtă peste luna închisă.

RLF folosește `DeclarantReturFurnizor`, tot numai pe privat (T-D6/T-D8).
Pe lotul original scade cantitatea și valoarea notei fiscale, fără
preluarea soldului valoric la golire. Partida proprie401 are rest pozitiv;
compensarea cu factura folosește NTC. Catalogul `scenarii/RLF.md` probează
și reziduul−0,01 pe lot gol, reintrarea prin RDC și descărcarea ulterioară
la soldul cubului. Eliminarea reziduului prin reevaluare rămâne T-r2/TR-D9b.

DVI folosește `DeclarantDvi`, activat numai pe privat. Taxa este contabilă;
baza vamală este o pereche debit/credit pe contul deductibil în
`Carte=Fiscal`, fără unități. Numai debitul fiscal poartă cod, rol Bază,
partener și perioadă; creditul echilibrează fără fapt fiscal. Baza 0,01
rămâne în cub și când taxa este zero. Anularea, stornoul și corecția folosesc
mecanismele comune, inclusiv refuzul inversării cu o partidă consumată prin
NTC. Contract: `docs/nucleu/tr-d7b-dvi-baza-fiscala-contract.md`; probe:
`SC-DVI-01…20`, `SC-X-14`. Conturile se rezolvă din politică, inclusiv ancora
fiscală; modelul pur și schema nu se schimbă.

Citirile contabile cer `Carte=Contabil`, jurnalul TVA citește ambele cărți
și numai faptele cu cod, adunând valorile semnate. SC-DVI-16 detectează lipsa
filtrului de carte inclusiv pe soldul cont × partener.

NTC și ITV folosesc același `DeclarantNotaContabila` (T-D3, pasul 3).
Nota păstrează conturile explicite și valoarea semnată; cantitatea este 0.
Pe un cont cu `UrmarestePartide`, partenerul explicit al liniei nominalizează FIFO
partidele aceluiași cont și partener, în sensul stingerii și până la rest;
excedentul deschide partida proprie. Soldurile pentru această nominalizare
se citesc din cub la data înregistrării, ca fapte în operand. Fără partener,
nu se inventează partidă; pe 3xx fără lot postarea rămâne doar contabilă.
Împerecherea ulterioară fără partidă proprie nu mai transferă încă o dată
valoarea deja nominalizată. Sursele cu nominalizări active sunt protejate
la anulare/storno, inclusiv în intervalul dintre nominalizare și inversarea
ei ulterioară: un dependent stornat pe 20 nu permite stornarea sursei pe 10.

ITV postează explicit liniile generate din `SolduriService`, fără fapte
fiscale noi. Corecția legată verifică soldurile la `DataInregistrare`, unde
stornoul a redeschis sumele; închiderea obișnuită folosește `Data`.
Validarea și citirea `Stale` folosesc aceeași regulă. Cataloagele și
validarea independentă: `docs/nucleu/scenarii/NTC.md`, `ITV.md`.

Motorul scrie `Tranzactie`/`Postare` în tranzacția comenzii, pentru orice tip
care declară; e singurul scriitor al faptelor. Contractul formei:
`docs/nucleu/tr-d7a-strangler-contract.md` (S-D1…S-D16); tăierea registrelor:
`docs/nucleu/tr-d9-taierea-contract.md` (D9-D2).

### Entitățile și forma lor fizică

`Module/Cub/` ține cele două entități, POCO EF în afara `EntitateConta`:
cubul e append-only, deci fără `OptimisticLockField` și fără filtru global de
interogare. Proprietățile sunt `virtual` și colecția e
`ObservableCollection`, cât timp hosturile folosesc proxy-uri de change
tracking. Niciuna nu intră în metadata clientului. (S-D1) În XAF apar numai
ca evidență read-only (lista `Postare` de sub „Registre”). Pe orice ușă
securizată gardianul le refuză crearea, modificarea și ștergerea, inclusiv
administratorului: le scrie numai motorul, pe ușa de sistem. Refuzurile de
ștergere care se sprijineau pe registre citesc cubul: lotul propriu al unei
linii șterse rămâne cât e unitatea unei postări, tipul de TVA purtat de o
postare nu se șterge, fișa care e unitatea unei postări nu se șterge.
(D9-D9, D9-A6; 2026-10-06)

| Entitate | Coloane |
|---|---|
| `Tranzactie` | `ID`, `DocumentId`, `Fel` (`Operare`, `Storno`, `Transfer`, `Deschidere`), `Data`, `ScrisLa` (UTC) |
| `Postare` | `Spatiu`, `TranzactieId`, `DocumentId`, `LinieId`, `Data`, `Cont`, `Latura`, `Partener`, `Gestiune`, `Produs`, `Unitate`, `UnitateDeschisa`, codul de TVA în trei coloane (`TipTvaId`, `SensTva`, `RolTva`), `PerioadaDeclarare`, `Valuta`, `Carte`, cele șase dimensiuni, `Atribuit`, `Pereche`, `Cantitate`, `ValoareValuta`, `Valoare` |

Enumurile nucleului se mapează pe `smallint` cu valorile lor numerice; scara
măsurilor rămâne a gardianului `Scara` (bani 18,2, cantități 18,3).

Tabela `Postare` e partiționată LIST pe `Spatiu`, cu exact două partiții —
`Postare_Contabil` (1) și `Postare_Stoc` (2) — și cheie primară `(Spatiu, ID)`.
Cheia compusă și partiționarea trăiesc DOAR în bază: modelul EF declară cheia
`ID`, fiindcă XAF EF Core nu suportă chei compuse. FK-urile stau pe partiții,
nu pe părinte: `Tranzactie`, `Documente`, `Conturi`, `Repartitori` (partener)
și `Produse` pe amândouă, `Loturi` (unitate) doar pe Stoc. Nu există FK pe
`Gestiune`, fiindcă gestiunile virtuale sunt id-uri fără rând (S-r3), și nici
pe `Unitate` pe partiția Contabil, fiindcă id-ul partidei e hash determinist,
nu id de rând. (S-D2)

`UnitateDeschisa` e data deschiderii unității, decisă de declarant la postare:
cu ea, cititorul rândurilor reconstruiește unitatea fără niciun lookup.
Reconstrucția merge pe FEL — `Partida` ia partenerul rândului și n-are produs,
`Lot` ia produsul și n-are partener — iar scrierea refuză un rând din care
unitatea nu s-ar reconstrui exact. (`Module/Cub/Randuri.cs`)

Orice migrație care atinge `Postare` sau `Tranzactie` se scrie în SQL, nu se
lasă generată: snapshot-ul EF poartă cheia simplă și FK-ul pe părinte, baza are
cheia compusă și FK-urile per partiție. (S-r4)

### Tipul care nu declară

Orice tip operabil declară. Tipul fără declarant (BPR) sau fără politica de
profil pe care declarantul lui o cere (`IDeclarant.PoliticaCeruta`: regulile
de contare pentru DSC, RDC și RLF, politica de TVA pentru DVI, politica
închiderii de TVA pentru ITV — absente pe bugetar) e refuzat la dry-run și la
operare cu `TIP_FARA_DECLARATIE`, înaintea validării frunzei, cu sau fără
număr cules. Tipul de document nu mai poartă un comutator de postare în cub:
orice tip operabil postează, prin declarant. (D9-D5, D9-D6)

O regulă de contare pe un tip al cărui declarant nu contează prin reguli
(`IDeclarant.ConteazaPrinReguli` fals: ASM, BTR, PIF, AMO, CAS, NTC, ITV, DVI)
sau care nu declară e refuzată la editare, pe ușa gardianului, cu
`REGULA_CONTARE_FARA_CONSUMATOR`: politica nu descrie comportament pe care
motorul nu-l are. (4, D9-A4)

### Materializarea, stornoul, anularea

`Module/Cub/Materializare.cs` rulează din `MotorOperare`, deci pe toate ușile
(UI, WebApi, Import1C):

- **Operarea** — după scrierile pe document, înainte de commit: contractul declarantului devine cel mult o tranzacție `Operare` și
  cel mult una `Transfer` (cel puțin una din ele; felul mixt de mai jos, T-D2),
  datate cu `DataInregistrare`, o postare per postare a contractului. Refuzul
  declarației E refuzul operației: o singură eroare cu toate refuzurile, iar
  tranzacția de comandă se anulează integral — nimic în cub, nimic pe document.
  Dry-run-ul arată aceleași refuzuri, în forma `EroriDto` de azi. (S-D4)
- **Stornoul** — o singură tranzacție de fel `Storno`: inversul exact al
  postărilor `Operare` ale documentului, al transferului lui de STOC (unități
  de fel `Lot`; transferurile pe partidă sunt ale împerecherii și le inversează
  rândul ei invers, S-D13) și al celor `Atribuit` spre ele, datat
  la data stornării, cu `PerioadaDeclarare` re-ștampilată la perioada
  stornării. La corecția cu motivul `EroareMateriala`, postările fiscale ale
  inversei se nasc cu atribuirea corecției — perioada originalului sau, dacă e
  depusă, regularizarea — și cu marcajul de inversă tehnică; nicio postare
  existentă nu se rescrie. (S-D5, D9-A5) Ordinalul perechii se păstrează pe
  prima sursă (după identificatorul tranzacției) și se decalează pe celelalte
  cu maximul surselor dinaintea lor, ca perechile să rămână distincte în
  unica tranzacție de storno (D9-A2, `Storno.Inverseaza` pe `(Sursa, Postare)`).
- **Anularea operării** șterge fizic tranzacțiile `Operare` și `Transfer` ale
  documentului și postările lor; e gardată
  de „fără împerecheri", deci orice `Transfer` al documentului e al lui. (S-D5, T-D2)

### Felul mixt: `Transfer` pe linia care nu schimbă contul (T-D2, felia 32)

Declarația are `Miscari` (debit ≠ credit, devin `Operare`) și
`Mutari` (același cont, aceeași latură, −/+ între două capete, devin
`Transfer`). O linie de stoc al cărei cont nu se schimbă între laturi e o
mutare; una care schimbă contul e o mișcare. Un document are astfel cel mult o
`Operare` și cel mult un `Transfer`, cel puțin una — amendament de literă al
lui 090 (a), T-r1. Fiecare capăt real al unui `Transfer` de stoc poartă `Gestiune`
și `Unitate` (lotul); contraponderea ASM de mai jos nu are unitate.
Rapoartele pe cont exclud Transfer (Σ per (cont, latură) = 0),
cele pe gestiune și pe lot îl includ.

- **BTR** (`DeclarantNotaTransfer`): lotul își schimbă gestiunea și contul lui
  rămâne — pe modelul
  de azi BTR produce NUMAI `Transfer` (T-D2.1: pe Flax 85.027 linii, toate cu
  același lot pe ambele capete, zero rânduri contabile). Contul e al lotului
  (`TipMaterial.ContImplicitId`), fără regulă de contare; refuzuri:
  `GESTIUNI_IDENTICE`, `CONT_STOC_LIPSA`, plus cele de gestiune, lot și
  cantitate. Valoarea = `Evaluare.Iesire` pe raportul curent al lotului în
  gestiunea predatoare, în secvența liniilor.
- **Diferența declarată T-D2.2**: motorul vechi scrie `round(cantitate ×
  Lot.PretUnitar)` cu prețul înghețat la nașterea lotului; cubul evaluează pe
  raportul curent (090 (j), N-r3). Pe Flax: 535 din 45.552 BTR (1,17 %), 560
  linii din 85.027, toate ieșiri PARȚIALE (cele 70.841 de goliri sunt egale),
  Σ semnată +0,92 lei și Σ absolută 28,60 lei pe 121.094.304,67 lei mutați;
  553 de linii sunt reziduul propriei goliri a motorului vechi mutat pe
  destinație, 7 sunt loturi intrate la altă valoare decât prețul lor (extrem
  `BTR-9039`: preț 0 contra 11 lei/buc). E repartizare între gestiuni, nu
  conservare: literele (a)–(g) ale reconcilierii n-o văd; apare doar la citirile
  pe cub per gestiune × lot (TR-D8). Nu se normalizează.
- Ramura `Operare` a formei mixte e probată prin proprietăți în nucleu; pe
  scenă o probează ASM (pasul 5).

### Transformarea n→m (ASM-B2…B7, 2026-09-23)

`Declaratie.Transformari` conține linii cu rol consum/produs, capăt real,
cantitate, valoare și cauză. Primitiva pură adaugă o contrapondere pe
`GestiuniVirtuale.Transformare`: același cont/produs/analiză/cauză,
cantitate opusă, valoare zero, fără unitate. Conservările sunt neschimbate.
Stornoul selectează tranzacția Transfer de stoc întreagă, inclusiv aceste
postări din partiția Contabil, și verifică conservarea înaintea scrierii.

Declarantul citește rolul și prețul prin `ILinieCuTransformare`. P = R se
verifică și pe operandul închis, fără pregătirea valorilor entității.
Grupurile sunt clasificate o singură dată după P/R; contribuțiile Δ se
acumulează pe produsul țintă înaintea gardului de pozitivitate. Prețul cules,
valoarea entității și prețul lotului rămân ale registrelor. Stornoul inversează
ajustarea istorică exact; corecția calculează din nou. Contractul detaliat:
[transformarea ASM](../nucleu/tr-d7b-asm-transformare-contract.md).

### FCL și DSC pe cub (T-D4, felia 32, pasul 2)

- **FCL** (`DeclarantFacturaIesire`): FCT în oglindă — per linie venitul pe
  regula de vânzare a Tipului (creditul = internul, cu gestiunea emitentului;
  debitul 4111 = terțul, fără gestiune, cu partidă), taxa colectată per linie
  prin `Fiscal.Impozitul` pe direcția `Colectat` (creditul pe 4427, debitul pe
  contrapartidă; gestiunea internă = latura opusă contrapartidei politicii,
  `Fiscal.GestiuneaInterna`), taxa culeasă autoritară, taxarea inversă pe
  livrare fără postare de taxă. Zero postări de stoc: linia de natură `Stoc` e
  linie de venit, fără lot și fără produs. Partidă pe fiecare cont cu `UrmarestePartide`
  al liniilor (S-D16): FCL cu regularizare de avans (`4111 = 419`) deschide
  DOUĂ partide, iar soldul partidei de creanță e netul ei (debitul de −100 al
  liniei de avans intră pe 4111). Valorile negative (prețuri negative) se
  declară semnate, pe aceeași latură. Refuzuri: laturile prin contractul
  clasei (T-D13), fără linii, cantitate ≤ 0, `PRODUS_ALT_TIP`, regulă lipsă, TVA.
- **DSC** (`DeclarantDescarcareGestiune`): o mișcare per linie — lotul iese
  din gestiunea predatoare pe contul de stoc al regulii (`6xx = 3xx` per
  Tip), evaluat cu `Evaluare.Iesire` pe raportul curent în secvența liniilor
  (ca BCS), iar costul intră cu `+q` pe gestiunea virtuală `Client`, fără
  unitate (090 (g), prima folosire a constantei) și cu `Partener` = primitorul
  când acesta e de parte externă (T-D13 (g); fără partidă — 607 n-are rol de
  terț). Aceeași formă cu sau fără factura-sursă. Lotul e ales de
  `DescarcareService` (pin-uri, apoi FIFO), declarantul nu realocă.
- **Transferul INC → FCL** e activ prin `Materializare.Imperecheaza` fără
  nicio schimbare: referința e a stingătorului (4111), plafonul e restul FCL
  pe 4111; partida 419 nu primește transfer.
- **Diferența declarată T-D4.2** (aceeași clasă ca T-D2.2, N-r3): pe Flax 842
  din 36.696 DSC (2,29 %), 919 linii din 62.063, toate ieșiri PARȚIALE (cele
  47.505 goliri sunt egale), Σ semnată +31,01 lei, |Δ| max 16,50 lei (lot cu
  preț înghețat 0), 909 din 919 linii ≤ 6 bani, pe 79.199.898,27 lei mutați.
  Nu se normalizează (T-r7).

### Laturile documentului ca structură (T-D13, felia 32, pasul 2b)

Modelul e cel din legacy (`GEST_DEFA_DOCUM`: predator → primitor pe
intern/extern per tip), pus în cod, nu în tabelă:

- **`Parte`** (`Declaratii/Laturi.cs`) e partea repartitorului față de
  patrimoniu, derivată o singură dată din `FelRepartitor` (= `ClrType`, 89b)
  prin `Laturi.ParteA`: `Extern` (Partener, Angajat), `Intern` (Gestiune,
  UnitateInterna), `Propriu` (ContPropriu). `RepartitorFapt.Parte` o expune;
  nu ajunge în nucleu, pe `Capat` sau în cub (gestiunea virtuală rămâne
  semnalul de extern pe postare) și n-are coloană.
- **Contractul** e METODA polimorfă `Document.Laturi()` (abstractă: fiecare
  tip o declară, compilatorul o cere) → `ContractLaturi(Latura Predator,
  Latura Primitor)`, unde `Latura` = părțile permise + calitatea cerută
  (`LocConsum` pe primitorul BCS, `Comisie` pe al LDI) + felul exact DOAR
  unde structura îl cere: laturile care poartă stoc sunt `Gestiune`, fiindcă
  `Lot.Gestiune` e tipat `Gestiune`. Fără tabelă de politică, fără `switch`.
- **Un singur loc de verificare**, `Laturi.Verifica`, pe ambele uși:
  `Contractare` o cheamă ÎNAINTEA declarantului (refuzul laturii e singurul
  refuz al declarației), iar `Document.ValideazaOperare` o cheamă pe faptele
  citite din bază (`Fapte.Laturile`), cu aceeași linie de mesaj
  (`Contractare.Mesaj`: `COD: text`). Codurile sunt `PREDATOR_NEPOTRIVIT` /
  `PRIMITOR_NEPOTRIVIT`; declaranții și clasele nu mai au verificări proprii
  de latură (`is Gestiune` tăiat din cele 19 clase); `CONT_PROPRIU_LIPSA` și
  `LATURA_CONT_PROPRIU_NEPOTRIVITA` au dispărut (de neatins sub contract).
- **Contractele declarate** (predator → primitor): NIR, FCT, RDC
  `Extern → Gestiune`; FCL, RLF `Gestiune/Intern → Extern` (FCL emitentul e
  `Intern`, RLF `Gestiune`); BTR, ASM `Gestiune → Gestiune`; BCS `Gestiune →
  Intern + LocConsum`; LDI `Gestiune → Intern + Comisie`; DSC `Gestiune →
  Extern | Intern`; NTC, ITV `Intern | Propriu` pe ambele; DEC, DVI
  `Extern → Intern`; PLT `Propriu → Extern | Propriu`; INC `Extern | Propriu
  → Propriu`; PIF, CAS, AMO, BPR `Intern → Intern` (AMO cere în plus
  identitatea, în clasa ei). Față de validările vechi, granularitatea e a
  părții: pe laturile fără stoc `UnitateInterna` și `Gestiune` sunt
  interschimbabile (DVI, PIF, CAS, DEC), `Partener` și `Angajat` la fel pe
  cele externe (FCT, NIR, FCL, DEC), iar `Angajat` nu mai e admis pe laturile
  interne (BCS/LDI cu calitate, NTC) — pe Flax nicio latură nu poartă
  `Angajat`.
- **Terțul pe capătul extern**: DSC pune `Partener` = primitorul pe capătul
  607 (T-D13 (g)); oracolul are normalizarea `TrD13TertulPeCapatulExtern`
  (după M6, numărată în `Normalizari.Contoare`, nu avertisment).
- **UI înghețat** (090 (m)): filtrarea lookup-urilor Predator/Primitor pe
  partea permisă e a dezghețului (`lista-react.md`).

`DocumentDetaliu.Pozitie` e ordinea de culegere a liniei. Se atribuie o
singură dată, la salvarea unei linii noi, în `SaveChanges`-ul contextului — un
singur loc pentru UI, WebApi, Import1C și conexul clonat — ca max-ul liniilor
documentului plus unu. Ambele motoare citesc liniile `OrderBy(Pozitie)`, apoi
`ThenBy(ID)`: cele cinci enumerări ale motorului vechi (TVA culeasă, notele,
TVA-ul, loturile născute, potrivirea regulilor de stoc) trec printr-un singur
helper, iar conexul clonat primește liniile sursei în aceeași ordine. (S-D6)

### Decontul pe cub și urmărirea partidelor (095, 096, 100)

DEC declară cheltuiala și contrapartida titularului, cu TVA din politica
profilului și fără stoc. Conturile/repartitorii expliciți au prioritate;
contrapartida TVA rămâne cea a politicii. Normal, capitalizat și taxare
inversă sunt probate independent; bugetarul păstrează costul brut fără
fapte fiscale când nu are PoliticaTva.

`Cont.UrmarestePartide` conduce deschiderea, nominalizarea și selecția
partidelor. `RolTert` rămâne clasificarea comercială SAF-T. Seed-ul activează
urmărirea pe 542 privat și 542.01.00/542.02.00 bugetar, păstrând acoperirea
comercială existentă. Din 2026-09-25 sunt urmărite și conturile bugetare
401.01.00, 404.01.00 și 411.01.01 DinSeed, fără schimbarea RolTert (100).
Conturile devenite manuale sunt respectate la re-seed. Invarianții refuză
postările fără identitate completă de partidă și indică
postarea/documentul/contul (`CITIRE_PARTIDE_INCOMPLETE`).
Partida decontului este a titularului Angajat;
Customers/Suppliers nu îl preiau numai pentru că are avans. Migrația
inițializează noul atribut pe conturile cu rol comercial, inclusiv manuale;
postările istorice rămân neschimbate.

Catalog: [DEC](../nucleu/scenarii/DEC.md), inclusiv PLT 150 → DEC 100 →
INC 50, storno/corecție după închidere și refuzuri atomice. PIF/AMO/CAS au declarant și
cititor comun pe cub, conform [contractului complet](../nucleu/tr-d7c-imobilizari-contract.md).

### Împerecherea ulterioară operării = tranzacție `Transfer`

O împerechere creată DUPĂ operare, desfacerea ei și rândul invers scris la
storno produc pe stingător o tranzacție de fel `Transfer`: suma se mută de pe
partida proprie a stingătorului pe partida stinsului, ieșire și intrare pe
ACELAȘI cont și aceeași latură, deci Σ = 0 per cont × latură. Împerecherea
  automată la operare nu produce transfer — ea E nominalizarea din `Operare`. (S-D13)

- **Contul comun** e contul partidei de referință a stingătorului: postarea lui
  cu unitate de fel `Partida` și valoare absolută maximă. **Partenerul** e al
  partidei stinsului.
- **Plafonul** e restul partidei stinsului pe acel cont: valoarea absolută a
  netului tranzacției `Operare` a stinsului, cu conexul lui autogenerat
  absorbit (TR-D3) și cu transferurile deja primite de partidele lui. Plafon
  zero ⇒ nu se mută nimic. Sold al partidei proprii sub plafon ⇒ refuz
  `PARTIDA_PROPRIE_INSUFICIENTA`. Rândul invers desface exact cât a mutat
  perechea lui, nu restul de azi al partidei.
- **Data** tranzacției e `Imperechere.Data` a rândului, original sau invers —
  reperul pe care se taie partidele, deja garantat de gardieni ca fiind
  în perioadă deschisă și nu înaintea datelor de înregistrare.
- Se scrie doar când ambele documente au tranzacție `Operare` în cub. Pe un
  cont fără `UrmarestePartide` nu există partide, deci nu există ce muta.

Din 2026-09-25, `Cub.Citiri.Partide` este intrarea comună pentru soldurile
pe unitate/cont/partener, cu transferurile și inversele lor incluse.
Selecția FIFO folosește această intrare și include partidele inițiale.
Raportul general și snapshot-ul de partide folosesc aceeași intrare (101).

Ștergerea unei împerecheri este comandă atomică: eliberează în cub suma
nominalizată și șterge linkul. CRUD-ul direct este refuzat; API și XAF
verifică dreptul Delete înaintea comenzii. Pentru împerecherea automată,
desfacerea mută suma de pe factura stinsă pe partida proprie a plății;
postările originale rămân. Transferul păstrează atribuirea către
nominalizarea originală, astfel încât stornarea ulterioară să îi compenseze
efectul. Stornarea directă a plății automate inversează nominalizarea o
singură dată. Gardul de dependențe include inversele încă necomise din
aceeași comandă și verifică soldurile intermediare pe dată.

### Explicația deciziei (X-D4, 2026-10-03)

Contractul acceptat își persistă deciziile și ipotezele: de ce a costat
ieșirea atât, din ce sold, ce partide a stins.

- **Unde**: `Tranzactie.Explicatie` (`jsonb`), pe prima tranzacție scrisă a
  contractului — `Operare` când există, altfel `Transfer` (BTR, ASM fără
  schimb de cont). A doua tranzacție a aceluiași contract o referă prin
  `ExplicatieDinId`. Se scrie în același `INSERT` cu rândul și nu se mai
  modifică. `Storno`, `Deschidere`, transferul împerecherii, desfacerea și
  stingerea de deschidere nu au explicație; baza refuză prin
  `CK_Tranzactie_Explicatie` și `CK_Tranzactie_ExplicatieDin`.
- **Ce**: `Cub.Explicatie` — versiunea schemei (`v` = 2), numele
  declarantului, `JumatatiDeBan`, deciziile și ipotezele contractului în
  ordinea declarației, cu linia și unitatea lor. Un cititor refuză o versiune,
  o decizie sau o ipoteză pe care nu o cunoaște; versiunea 1 nu are cititor.
- **Regula care a decis** (D9-A8): pentru fiecare rând de politică din care
  declarantul a luat un fapt folosit la o postare, ipoteza
  `VersiunePolitica(Fel, Rand, Versiune)` — clasa rândului, identificatorul
  și contorul lui la operare, una pe rând, fără dubluri
  (`Declaratii/PoliticiConsumate`). Se rețin: regula de contare câștigătoare
  a fiecărei linii (numită și pe `ContRezolvat.Regula`), regula de stoc a
  LDI, politica de TVA când declarația are fapt fiscal sau contrapartida
  recepției vine din ea, politica de diferență a NIR-ului conex și politica
  de amortizare a fișelor atinse. Nu se rețin nomenclatoarele care decid
  (tipul de material, tipul de TVA, steagurile contului) și nici conținutul
  regulii la operare (D9-r3). Un declarant care nu consumă reguli (ASM, BTR,
  NTC) nu scrie ipoteza. Lista nominală, declarant cu declarant:
  `docs/nucleu/tr-d9-pas7c-versiunea-politicii.md`.
- **Ieșirea pe lot** are exact o decizie de valoare: `ValoareIesire` când
  valoarea vine din soldul citit (BCS, DSC, BTR, LDI minus, consumul ASM),
  `ValoareDeclarata` când o dă altă sursă — linia documentului (RLF și RDC, la
  valoarea culeasă) sau recepția facturii (NIR-minus). Sursele sunt
  `Declaratii.SurseValoare`. Decizia poartă cantitatea și valoarea pozitive;
  postarea-sursă a unui transfer le are negative. Mecanismul e al
  declarantului, nu al explicației: `IDeclarant.SursaValoareDeclarata` e
  `null` pentru declaranții care evaluează din sold și numește sursa pentru
  cei trei care declară valoarea (NIR, RLF, RDC). Niciun declarant nu le
  amestecă.
- **Soldul citit** (`SoldUnitateCitit`) e soldul net al unității la data
  documentului, fără documentul curent, o dată per unitate. Soldul dinaintea
  fiecărei ieșiri următoare de pe aceeași unitate se derivă din deciziile
  anterioare (`Explicatie.IesiriEvaluate`).
- **Stingerea FIFO** are câte o `AlocareFifo` per linie și partidă stinsă,
  iar partida stinsă are soldul ei citit între ipoteze; partida proprie a
  documentului are `PartidaDeschisa`.
- **Stornoul** se explică prin original: `Cub.Citiri.Explicatii.PeTranzactie`
  urmează `InversaDin` și întoarce explicațiile purtătorilor de origine.
  **Anularea operării** șterge explicația odată cu tranzacția.
- **Invariantul** (`Explicatii.VerificaAcoperire`, în `INV-CUB`):
  `CITIRE_EXPLICATIE_LIPSA` (o tranzacție `Operare` sau o ieșire pe lot fără
  explicație), `…_REFERINTA` (referință spre o tranzacție fără explicație sau
  a altui document), `…_STORNO` (inversa pe lot nu oglindește originalul),
  `…_IESIRE` (ieșirile postate și deciziile de valoare nu corespund una la
  una pe linie, unitate, cantitate și valoare), `…_EVALUARE` (valoarea unei
  `ValoareIesire` nu rezultă din soldul persistat, cu rotunjirea bazei),
  `…_STINGERE` (alocare fără postare, postare pe partida altui document
  fără alocare, partidă stinsă fără sold citit sau cu alocări peste soldul
  citit), `…_MECANISM` (explicația numește alt declarant decât cel al
  documentului, poartă o valoare declarată la un declarant care evaluează
  din sold, sau o valoare evaluată ori altă sursă la unul care declară).
  Declarantul se ia din document, nu din explicație.
- **Transferul persistat conservă pe cont** (`Invarianti.VerificaTransferuri`,
  `CITIRE_TRANSFER_NECONSERVAT`, în `INV-CUB`): în orice tranzacție `Transfer`
  valoarea se anulează pe (cont, latură) și cantitatea pe (cont, produs). E
  regula de la contractare (090f), reverificată pe ce s-a scris. Acoperă
  contul capătului de destinație, pe care nici acoperirea cantitativă a
  stocului, nici reconcilierea (a) nu îl văd.
- **Perechea persistată e cea de la contractare** (`Invarianti.VerificaPerechi`,
  în `INV-CUB`): `CITIRE_PERECHE_INVALIDA` — pe (tranzacție, ordinal) nu stau
  exact două postări cu același document, aceeași linie, cantități opuse și
  valorile potrivite laturilor felului; `CITIRE_PERECHE_LIPSA` — postare fără
  ordinal în afara deschiderii și a liniilor transformării. (D9-A2)

Dry-run-ul nu persistă nimic. O declarație pe care nucleul nu o poate
construi (`ArgumentException`) sau un declarant care nu întoarce nici
declarație, nici refuz dau refuzul `DECLARATIE_INVALIDA`, pe dry-run și pe
operare, în aceeași formă ca orice refuz al declarației (S-r11).

### Scrierea serială per bază (X-D6, 2026-10-03)

- **O singură comandă scrie la un moment dat.** Tranzacția unei comenzi se
  deschide prin `TranzactieComanda.Incepe`, care ia ca primă instrucțiune
  blocajul scrierii — un `pg_advisory_xact_lock` cu cheie constantă
  (`TranzactieComanda.BlocajScriere`), ținut până la commit sau rollback.
  Blocajul protejează citirea care decide valoarea, nu doar scrierea: soldul
  lotului, restul partidei, starea perioadei și planul registrelor se citesc
  după el, pe starea lăsată de comanda precedentă.
- **Cine îl ia**: operarea, anularea, stornoul și corecția; împerecherea,
  desfacerea și ștergerea ei; închiderea, redeschiderea și reconstrucția
  perioadei; generarea și regenerarea AMO și ITV; confirmarea depunerii;
  deschiderea și stingerea ei, care rulează în tranzacția apelantului și iau
  blocajul la intrare (`Materializare.CereScriere`); salvarea care adaugă
  detalii fără poziție. Lista e nominală și probată pe sursă.
- **Cine nu îl ia**: citirile, dry-run-ul (`Valideaza`, `Refuzuri`) și
  salvarea unui draft fără detalii noi.
- **Ordinea blocajelor** e „scrierea, apoi restul”: perioada (F27-D1),
  documentele stingerii, suportul și fișele IMO (97001), sursa recepției
  (97002) și depunerile rămân și se iau după blocajul scrierii, deci nu pot
  forma cicluri între comenzi.
- **Așteptarea** durează cât comanda din față. Dacă depășește timpul de
  comandă al conexiunii, a doua comandă primește refuzul `SCRIERE_OCUPATA`
  (422), fără nimic scris, și se poate relua.
- **`Pozitie` pe detalii** (S-r9): salvarea care adaugă detalii fără poziție
  deschide tranzacția ei, ia blocajul scrierii și abia apoi citește maximul
  pe document; două sesiuni care adaugă pe același document primesc poziții
  distincte, în ordinea intrării.
- **Rezultatele seriale probate pe două conexiuni** (SC-X-15…SC-X-23, ambele
  profiluri): două consumuri peste lot — un succes și `STOC_INSUFICIENT`;
  două consumuri care încap pe 3/1,00 — 0,33 și 0,34, cu soldurile citite
  3/1,00 și 2/0,67 în explicațiile persistate; consum contra stornoul
  recepției — un succes și un refuz, în ambele ordini; două stingeri peste
  restul aceleiași partide — un succes și un refuz; aceeași pereche — după
  sume; operare, apoi închidere — ambele trec și snapshot-ul poartă
  operarea; închidere, apoi operare — operarea e refuzată.

### Gardurile declaranților, ca dată sau ca regulă

- Valoarea negativă e admisă în `Operare`: e reprezentarea „în roșu” a liniei
  culese, retur sau discount pe același document. `SEMN_NEGATIV` rămâne doar pe
  `Deschidere`. Un lot născut de o linie „în roșu” se evaluează negativ, ca în
  motorul vechi (S-r7). (S-D14)
- `PoliticaTva.TolerantaTaxa` e opțională: `null` înseamnă că taxa culeasă
  rămâne autoritară, fără validare — valoarea de seed a profilului privat. O
  valoare dată refuză `TVA_IN_AFARA_TOLERANTEI` peste `toleranță × liniile
  cotei`. (S-D15)
- O linie cu două conturi cu `UrmarestePartide` numește partidă pe AMBELE capete,
  fiecare pe contul lui. (S-D16)
- Tipul de document nu mai declară latura contului propriu: declarantul de
  trezorerie o alege structural, piciorul propriu e cel al cărui cont vine de
  la repartitorul intern, iar la virament ambele picioare sunt ale contului
  propriu numit de latura care nu e tranzit. (S-D7, D9-A10, D9-D6)
- Linia fără regulă de contare e refuzată (`REGULA_CONTARE_LIPSA`) acolo unde
  motorul vechi o sare tăcut: valoarea ei ar dispărea din contare.

### Ce rămâne al feliilor următoare

Deschiderea generică este disponibilă prin `Cub.Materializare.Deschide`
în tranzacția explicită a apelantului, fără commit propriu. Soldurile de
control includ ancora aleasă de apelant; loturile și partidele înlocuiesc
rândul bloc pe Carte/Cont/Latura. Totalurile diferite sunt refuzate înaintea
creării entităților de cub. Partidele cer cont cu UrmarestePartide, partener și
referință stabilă; nu există document fictiv. Indexul unic filtrat permite
o singură Deschidere în bază. Stingerea unei partide inițiale verifică
restul întregii unități, contul, partenerul și perioada sub blocare comună
cu storno/anulare. Transferul contribuie la restul de domeniu al plății,
la lista de documente cu rest și la snapshot-ul partidelor, cu aceeași tăiere pe dată;
anularea este refuzată, iar stornoul nu poate preceda stingerea și reface
partida fără a rescrie deschiderea. Detalierea stocului derivă din politică;
loturile deja folosite și deschiderea datată după istorie sunt refuzate.
Intrarea păstrează analiza/valuta și verifică dimensiunile obligatorii;
limitele intrării și ale stingerii sunt în DES-B4. (094, DES-B1…B4)

Citirile — sold, proiecții, fișe, SAF-T, D394, D406 — sunt pe cub. Notele pe
conturi de stoc fără lot (TR-r2) rămân delimitate prin contractele tipurilor.
Deschiderea generică scrie cubul, iar loturile ei se consumă ca oricare altele.

## Locurile regulilor în cod

- [Document și contracte](../../nou/Atlas.Conta.BackOffice/Atlas.Conta.BackOffice.Module/BusinessObjects/Documente/Document.cs)
- [Motorul operării](../../nou/Atlas.Conta.BackOffice/Atlas.Conta.BackOffice.Module/Motor/MotorOperare.cs)
- [Culegerea draftului (L3)](../../nou/Atlas.Conta.BackOffice/Atlas.Conta.BackOffice.Module/Culegere/)
- [Nucleul pur: conservarea](../../nou/Atlas.Conta.Nucleu/Atlas.Conta.Nucleu/Conservare.cs)
- [Nucleul pur: motorul pe declarație](../../nou/Atlas.Conta.Nucleu/Atlas.Conta.Nucleu/Motor/Motor.cs)
- [Declarația fluxului: operandul, driverul, declaranții BCS/PLT/FCT](../../nou/Atlas.Conta.BackOffice/Atlas.Conta.BackOffice.Module/Declaratii/)
- [Adaptorul operandului închis (`Fapte.Operand`)](../../nou/Atlas.Conta.BackOffice/Atlas.Conta.BackOffice.Module/Motor/Fapte.cs)
- [Oracolul pilotului și gate-ul de reconciliere al cubului](../../nou/tools/ModelCheck/Nucleu/)
- [Cubul persistat: entitățile, materializarea, cititorul rândurilor, transferurile](../../nou/Atlas.Conta.BackOffice/Atlas.Conta.BackOffice.Module/Cub/)
- [Serviciul de împerechere](../../nou/Atlas.Conta.BackOffice/Atlas.Conta.BackOffice.Module/Motor/ImperechereService.cs)
- [Documentele de trezorerie](../../nou/Atlas.Conta.BackOffice/Atlas.Conta.BackOffice.Module/BusinessObjects/Documente/Trezorerie.cs)
- [Documentele imobilizărilor](../../nou/Atlas.Conta.BackOffice/Atlas.Conta.BackOffice.Module/BusinessObjects/Documente/Imobilizari.cs)
- [Serviciul de amortizare](../../nou/Atlas.Conta.BackOffice/Atlas.Conta.BackOffice.Module/Motor/AmortizareService.cs)

### NIR delta și citirile comune (098/099, 2026-09-24)

[Contractul NIR](../nucleu/tr-d8-nir-delta-contract.md) fixează sursa ca
tranzacție și linii istorice, un singur cumul activ sub blocare comună și
politica `tip × cauză × clasă`. `PoliticaDiferenta` este configurabilă,
cu proveniență de seed; nomenclatorul privat existent furnizează conturile 32x.
Bugetarul folosește maparea aprobată 473.01.09/408.00.00/461.01.09,
428.01.02 pentru angajați și 35x pentru PeDrum. Lipsa politicii sau a
analizelor obligatorii este refuz atomic. Stocul istoric, datoria și TVA-ul
facturii nu se rescriu; inversarea utilizează delta existentă.

`Citiri/Receptii` este intrarea comună pentru proveniența recepțiilor conexe.
Pentru NIR acoperit, cubul păstrează recepția pe factură și gardul ei de stoc:
după stornarea sau anularea NIR-ului acoperit cu consum, operațiile următoare
pe lot se judecă numai pe soldul cubului (098-r3, SC-X-27).
Refuzul retragerii este verificat înaintea modificării tracker-ului, iar
cel al operării înaintea numerotării.

Scriitorul stornoului garantează proveniența inversei. `Citiri/Invarianti`
verifică în ModelCheck (`INV-CUB`), pe faptele fiecărei scene a catalogului,
înaintea purjei ei, invarianții pe care scrierea nu-i garantează prin
construcție (schema garantează deja, prin `CK_Postare_FelUnitate`, unitatea și
nașterea partidei): proveniența
(`CITIRE_PROVENIENTA_LIPSA`), echilibrul pe tranzacție și carte, conservarea
transferului, taxa postată = taxa liniei, unitățile de partidă, proveniența
fișelor și explicația, inclusiv valoarea liniei = decizia ieșirii, cu semn
(`CITIRE_EXPLICATIE_LINIE`, D9-D3). Hosturile nu
scanează istoria la pornire; o bază care nu corespunde codului se recreează
(102b, 102d).
`Postari` compune interogarea fără diagnostic global per apel;
inversele Operare/Deschidere se clasifică prin ID și spațiul originii.
Balanța, fișa, jurnalul și soldul pe partener sunt comutate pe cub împreună
cu snapshot-ul contabil. Snapshot-ul include separat gestiunea și partenerul,
se scrie numai din cub și se reconstruiește din postări. Citirile securizate folosesc
postările autorizate, nu snapshot-ul global, pentru a păstra permisiunile
pe rând și membru. Fișa afișează toate conturile corespondente, fără să
inventeze o pereche; jurnalul are identitatea postare × spațiu.

Cititorul de lot folosește cheia lot/cont/produs/gestiune, cu Transfer și
inversele lui. BCS/BTR/DSC/LDI/ASM evaluează ieșirile din soldul cubului.
FIFO și pinurile DSC
folosesc data înregistrării și coordonatele complete. Raportul de stoc
arată contul, gestiunea și costul unitar din sold; etichetele lipsă nu
elimină sumele. Snapshot-ul de stoc se scrie numai din cub pe aceeași cheie,
cu data deschiderii; citirile nesecurizate folosesc snapshot + fereastră,
inclusiv evaluarea ieșirii cu graniță strict anterioară documentului. Reconstrucția detectează și data alterată, pe lângă
diferențele de chei și măsuri. Soldurile 0/0 se omit, cele 0/valoare nenulă
rămân. Gestiunile virtuale nu intră în disponibilul real. TR-D8 rămâne în
lucru pentru fiscal/SAF-T și verificările transversale.
`Citiri/Transformare` oferă un singur predicat
pentru contraponderea virtuală ASM, utilizat și de probe/diagnostic.
DEC are probă numerică independentă pentru inversare și corecție peste
închidere: partidă −100 în ianuarie, zero după inversă, noua partidă −80.

Factura de avans 4091 privat / 409.01.01 bugetar nu produce recepție.
Seed-ul bugetar clasifică 409.01.01 fără stoc (clasa S), conform 099(d);
postările istorice nu se reclasifică prin această corecție de seed.

UrmarestePartide este activ și pe 461 privat, respectiv
408.00.00/461.01.09/428.01.02 bugetar. Efectul aparține contului, nu NIR-ului:
și FCT bugetar pe 408 deschide partida furnizorului (SC-NIR-36/FCT), iar
INC/PLT pe 461 folosesc urmărirea partidelor prin mecanismul comun.
SursaReceptieiId este proveniența unică, scrisă la generarea conexului;
corecția o păstrează independent de Autogenerat. Recepția-sursă se citește
o singură dată pe comandă, sub blocarea sursei la operare. Validarea
analizelor curente privește capătul diferenței; capătul stocului păstrează
analiza istorică. Imputatul fără cauză Imputabila sau fără deltă se golește.

### Partide: raport, snapshot și împerechere (101, 2026-09-25)

`Cub.Citiri.Partide` este intrarea comună pentru rest, disponibil și raport.
Cheia este unitate × cont × partener; originea este identitatea 092(a).
Deschiderile au document nul.
Raportul general expune restul absolut și sensul; nu deduce un „total al
partidei” din rulajele documentului. Candidații păstrează etichetele tipurilor
eligibile, dar sumele și limita perechii se citesc din cub.

Snapshot-ul de partide păstrează cheia completă, debitul, creditul, data
nașterii și documentul opțional. Conține numai solduri nete nenule;
combinarea cu fereastra următoare garantează restul, nu rulajul istoric al
unei partide închise și redeschise. Citirea securizată agregă postările
permise și ocolește snapshot-ul global. Reconstrucția raportează diferențele
înaintea înlocuirii, inclusiv detalierea fără document. Invarianții refuză
documentele stingibile ale căror conturi de contrapartidă nu urmăresc
partide (`CITIRE_PARTIDE_POLITICA`).

Împerecherea manuală cere efect integral pe un cont și partener comun,
fără plafonare tăcută sau alegerea celei mai mari postări. Nominalizarea
existentă se poate asocia documentar fără un nou transfer. Legătura poartă
identitatea transferului creat. Desfacerea are două căi: transferul exact al
legăturii sau, la stingerea automată, desfacerea nominalizării într-o partidă
proprie a stingătorului. Asocierea manuală fără transfer nu inversează operarea.
La stingerea automată, suma legăturii se confirmă din nominalizarea cubului.
Lipsa efectului, insuficiența și ambiguitatea se refuză înaintea creării
legăturii. La fel la desfacere: nominalizarea automată care nu-și mai găsește
efectul pe partida stinsului dă `IMPERECHERE_FARA_EFECT`, iar legătura rămâne
și rândul invers nu se scrie fără transferul lui (102-r4, X-D7 f).
CRUD-ul direct de creare/ștergere este refuzat; se folosesc comenzile.

Sursa nominalizării automate se citește tot din cub, inclusiv recepția
facturii înaintea NIR-ului. Disponibilul este limitat de fiecare dată
ulterior scrisă: un sold eliberat în viitor nu finanțează o stingere
retroactivă. La re-declarare se exclude efectul documentului curent.
Calculul este unic (`Cub.Citiri.Partide.Evolutie`/`DisponibilTemporal`):
zilele anterioare datei se lipesc de ea, mișcările aceleiași zile se
compensează. Îl folosesc PLT automată, FIFO-ul NTC, transferul manual,
stingerea partidei inițiale, nominalizarea liberă și verificarea
dependenților (C-D5, 101-r1). Ținta comenzii de împerechere este orice
unitate proprie a stinsului, inclusiv cea deschisă prin Transfer la
desfacerea nominalizării, aceeași mulțime din care panoul oferă candidații.

În invarianți, totalul de decontare al antetului este doar martor de acoperire:
valoarea partidelor Operare trebuie să-l acopere integral, în modul. Costul
vânzării/returului nu cere partidă; o politică manuală care pierde partida
comercială este refuzată. RDC cu partidă proprie creditoare apare ca datorie,
chiar dacă totalul documentului este negativ. Soldul citit pentru explicația
nominalizării este separat de limita disponibilă pe cont peste datele viitoare.
