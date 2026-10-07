# API și client

**Actualizat: 2026-10-04.** [Index](README.md)

## Împărțirea responsabilităților

`Module` deține modelul, regulile de domeniu, DTO-urile și serviciile comune.
WebApi asigură transportul, autentificarea și integrarea HTTP. XAF Blazor
folosește același domeniu. (5, 42f, 43)

Straturile au dependențe într-un singur sens: L0 Nucleu, L1 Declarații, L2
coaja comenzii, L3 culegerea (`Module/Culegere/`), L4 randarea (XAF Blazor,
React). Un strat nu cunoaște stratul de deasupra lui. (104a)

Culegerea documentelor se face în XAF Blazor. React acoperă citirile și
proiecțiile: jurnale, declarații, fișe, reconcilieri, rapoarte, consolele
comenzilor și editorii de politici și nomenclatoare. Paginile React de detaliu
ale documentelor sunt înghețate: nu primesc câmpuri noi, pot rămâne în urmă
față de model și nu blochează nicio felie. Un câmp nou pe un document intră în
entitate, în L3 și în `WriteDto`/`ReadDto`, nu în pagina React. `WriteDto` și
`Apply` rămân contractul de scriere pentru ModelCheck, import și orice alt
apelant. (104d, 104-r4)

Contractele documentelor sunt specifice tipului concret. Nu există un
endpoint generic care interpretează o schemă de document primită ca date.
DTO-urile de scriere și de citire sunt distincte; navigațiile EF nu sunt
contractul public. Referințele din scriere sunt identificatori expliciți. (6, 7, 42d)

## Salvare și comenzi

Crearea și salvarea rulează în ObjectSpace securizat. Comenzile de document
(operare, anulare, storno, corecție, validare) trec prin coaja
`ComenziDocument`, construită de `ContaApiController.ComandaDocument<T>` cu
dreptul `DreptComandaXaf` pe ușa `T` (și restricția ei, ex. NTC fără
închideri de TVA). Coaja verifică dreptul înaintea domeniului și își deschide
singură contextul motorului; `SubiectInvizibil` iese 404, `RefuzAcces` 403,
`OperareException` 422. Niciun identificator furnizat de client nu
autorizează singur accesul prin contextul nesecurizat. (42b, 55b, 80, 104b)

Comenzile care scriu sunt seriale per bază (X-D6): o comandă trimisă cât
timp alta scrie așteaptă. Dacă așteptarea depășește timpul de comandă al
conexiunii, răspunsul e 422 cu `SCRIERE_OCUPATA`, fără nimic scris; cererea
se poate retrimite. Același refuz îl poate da salvarea unui draft care
adaugă linii. Citirile, dry-run-ul și salvarea fără linii noi nu așteaptă.
Măsurat pe host viu, cu blocajul ținut de altă conexiune: 422 după 30 s,
timpul de comandă implicit. (108f)

PUT reprezintă starea completă a formularului: liniile sunt reconciliate,
iar câmpurile opționale absente se golesc conform contractului. PATCH OData
este parțial: absența unei proprietăți o păstrează. Enumerările sunt șiruri
și sunt validate înainte de crearea obiectului. Filtrele selectoarelor
ajută utilizatorul; validarea serverului rămâne obligatorie. (56, 57a, 77h)

Operarea, anularea și stornarea sunt comenzi, nu modificări directe ale
stării prin DTO. Regulile și tranzacția lor sunt descrise în
[domeniu și operare](domeniu-si-operare.md). (42b)

Affordance-urile de pe DTO-urile de citire (`PoateEdita`, `PoateOpera`,
`PoateAnula`, `PoateStorna`, `PoateSterge`, `PoateRegenera`, `PoateDistribui`,
`PoateStinge`, `PoateGeneraDescarcare`) sunt proiecția regimului pe stare
(`RegimDocument`), aceeași sursă ca acțiunile XAF; `Apply` nu le calculează
din `Stare`. Singura excepție declarată e `DscApply.PoateEdita = false`,
fapt al tierului (nu există cale de scriere pe DSC). (42e, 106d)

Fiecare DTO de scriere al unui document poartă `DataInregistrare` ca dată
opțională: absentă înseamnă „data documentului”, nu „gol”. Adaptorul comun
(`Api/DocumentApply.AplicaDate`) aplică implicitul și refuză, de domeniu, o
dată a înregistrării anterioară datei documentului — pe toate cele
cincisprezece uși de scriere. DTO-urile de citire ale documentelor o expun
alături de `Data`. (F27-D4)

`Api/*Apply` sunt adaptorii API ai culegerii (L3). Mapează DTO-ul, cheamă
`CulegereDocument` pe fiecare linie și `InainteDeSalvare` înaintea commit-ului.
Nu calculează valori, loturi sau implicite și nu poartă reguli proprii;
refuzurile culegerii vin de la gardianul de commit, ca 422. Pe liniile cu
produs (FCT, FCL, NIR, LDI, ASM), `TipMaterialId` e opțional: lipsa lui
înseamnă tipul produsului, iar lipsa ambelor e refuzată. `DataPrimire`
absentă rămâne goală și înseamnă data înregistrării; DTO-ul de citire o
arată astfel. `ValoareTva` prezentă pe linie e TVA-ul cules explicit; un 0
explicit e refuzat cu 422 când cota tipului dă taxă (pe DVI, câmpul
ne-nullable 0 înseamnă necules). Refuzul CAS pe fișa fără politică de
amortizare e al culegerii (L3); `CasApply` îl cheamă înaintea creării
documentului, numai pentru ordinea mesajului. (104c)

Închiderea și redeschiderea perioadei sunt tot comenzi, pe `api/perioade`:
`GET api/perioade` întoarce lanțul și cere dreptul de citire pe tipul
perioadei; `GET api/perioade/{an}/{luna}/verificare` întoarce constatările
complete — cheie, fel, severitate, text, identificatorul și eticheta obiectului
— și, fiindcă verdictul ÎNSUMEAZĂ pe ușa nesecurizată documente, închideri de
TVA, amortizări, împerecheri și politica severităților, cere dreptul de citire
pe toate acestea: un raport filtrat n-ar ieși gol, ar ieși FALS;
`GET api/perioade/{an}/{luna}/istoric` întoarce rândurile de istoric ale lunii
cu felul, momentul, utilizatorul, motivul și cheile acceptate;
`POST api/perioade/{an}/{luna}/inchide` primește cheile acceptate în corp
— corpul e opțional, fiindcă a închide fără constatări e cazul normal — și
răspunde cu acceptările scrise, iar un avertisment neacceptat sau orice blocantă
iese 422 cu lista întreagă, un rând pe linie;
`POST api/perioade/{an}/{luna}/redeschide` primește motivul. Subiectul acestor rute
este luna, nu un identificator: ea se rezolvă pe calea securizată, deci
ordinea refuzurilor rămâne 400 pentru an sau lună în afara marginilor, 404
pentru lună nedefinită sau invizibilă, 403 pentru lipsa dreptului cerut și
422 pentru refuzul lanțului. Motivul absent la redeschidere este refuz de
domeniu, nu de cerere. (F27-D1, F27-D2, 80a)

`POST api/documente/{id}/corecteaza` corectează un document operat, pe orice
tip: ruta e a BAZEI, iar documentul se rezolvă polimorf. Corpul poartă `Data`
(a corecției) și `Motiv` — numele membrului de enum, ca string pe sârmă.
Răspunsul dă originalul, documentul nou, starea originalului și `TipCod`,
codul tipului cu care clientul știe pe ce ecran să navigheze. Ordinea
refuzurilor este 400 pentru motiv necunoscut sau absent, cu valorile acceptate
enumerate, 404 pentru document inexistent sau invizibil, 403 pentru lipsa
dreptului de scriere pe instanță sau a dreptului de creare pe tipul concret —
comanda produce un document nou — și 422 pentru refuzurile domeniului.
DTO-urile de citire ale documentelor poartă `Corectie` (originalul, eticheta
lui și motivul) sau `null`. În client, comanda și banda „Corectează pe …" sunt
o singură componentă a nucleului, montată de shell pe toate ecranele de
document. (F27-D6, 80a)

`POST api/perioade/reconstruieste` recalculează integral soldurile
perioadelor de referință și întoarce, per referință, câte rânduri existau,
câte au ieșit din recalcul, câte diferă și suma absolută a diferențelor pe
debit, credit, cantitate și valoare. Comanda nu are subiect, deci gate-ul ei
este pe tip: dreptul de scriere pe perioada fiscală, același drept ca
închiderea. Ordinea refuzurilor este 401, apoi 403, apoi 422. (F27-D3)

`GET api/proiectii/sold-stoc` primește opțional `laData`: fără el întoarce
soldul de azi, cu el soldul la sfârșitul zilei cerute. Parametrul este al
proiecției, ca perioada balanței, nu filtru de grilă; o valoare imposibilă
cade pe 400-ul unic al tierului. Schimbare de comportament: un lot consumat
integral, cu cantitate ȘI valoare zero, nu mai apare în listă; unul cu
cantitatea zero și valoare nenulă rămâne, ca reziduul valoric să se vadă.
(F27-D3)

`POST api/imperecheri` primește opțional `Data` (ziua faptului de stingere;
absent = azi). `POST api/imperecheri/{id}/desfa` cu `{ Data }` scrie rândul
invers al unei împerecheri dintr-o perioadă închisă: 400 pe corp malformat,
404 pe inexistentă sau invizibilă, 403 fără drept de **scriere** pe instanță
(desfacerea scrie un rând, nu șterge), 422 pe domeniu (deja desfăcută, rând
invers, dată sub cea a împerecherii, perioadă închisă a rândului nou).
`DELETE api/imperecheri/{id}` rămâne calea din fereastra deschisă; pe o
împerechere dintr-o perioadă închisă dă 422, cu trimitere la desfacere.
Panoul de stingeri poartă pe fiecare rând `Data`, `InverseazaId`, `Desfacuta`
și `PerioadaDeschisa` — verdictul „ce buton are dreptul să apară" e
server-computed, nu dedus în client. (F27-D8, 80a)

`GET api/proiectii/documente-cu-rest` primește opțional `laData`: restul se
citește la ziua cerută, pornind de la partidele deschise ale ultimei perioade
de referință. Absent = la zi. Proiecția include acum și `ReturClient`.
(F27-D7)

`GET api/proiectii/sold-parteneri?laData=&contId=&repartitorId=` întoarce
soldurile pe (cont × repartitor) la o dată, cu aceleași filtre de dimensiune
ca balanța și cu rândurile de sold net zero omise. `laData` absent = azi; o
dată nevalidă cade pe 400. Citirea cere dreptul pe registrul contabil, ca
balanța. Ecranul `/sold-parteneri` din client ține data în URL și duce prin
dublu-click în fișa contului. Repartitorul e dimensiunea laturii, nu partenerul
contului de terț — ecranul o spune explicit. (F27-D7)

## Securitate și răspunsuri

Ordinea gărzilor este autentificare, forma cererii, vizibilitatea obiectului,
permisiunea operației, apoi validarea de domeniu. (80a)

| Status | Semnificație |
|---|---|
| 401 | Sesiune absentă sau nevalidă (80a) |
| 400 | Cerere/valoare de transport nevalidă (70f, 80a) |
| 404 | Obiect inexistent, invizibil sau de alt tip decât cel cerut (80a, 80b) |
| 403 | Operație interzisă asupra unui obiect vizibil ori asupra tipului (80a) |
| 422 | Refuz de domeniu sau constrângere de persistență tradusă (60a, 80a) |

O rută care cere GUID poate răspunde 404 pentru text care nu respectă ruta.
Listele securizate pot răspunde 200 cu rezultate filtrate. Rapoartele care
cer cifre complete verifică separat accesul necesar și refuză cererea dacă
filtrarea ar produce un rezultat incomplet prezentat ca total. (70f, 80a, 80e)

Rutele care întorc cifre însumate din cub pe ușa de sistem cer citirea
COMPLETĂ pe `Postare`: `itv/{id}`, `itv/previzualizare`, `amo/{id}`,
`amo/previzualizare`, `imobilizari/{id}/fisa`, `imobilizari/registru` și
`perioade/{an}/{luna}/verificare`. Un rol fără dreptul pe tip, ori cu un
criteriu de rând sau de membru pe `Postare`, primește 403 înaintea citirii.
Niciun tip de registru nu mai e cerut, deci separarea contabil / imobilizări
a dispărut. Dreptul completează, nu înlocuiește: ruta cere mai întâi dreptul
pe subiectul ei, iar verificarea închiderii pe toate tipurile pe care le
însumează. `Postare` și `Tranzactie` n-au rută OData sau REST și nu intră în
contractul clientului; `$metadata` le descrie ca `EntityType` fără
`EntitySet`, ca pe orice tip neexpus. (F22-D5, 80e, 108 d, D9-D9; 2026-10-06)

Crearea cere drepturile de creare și scriere; modificarea cere scriere, iar
ștergerea dreptul aferent. Verificările nu se amână până după execuția
regulilor de domeniu. Un context nesecurizat nu are strategie de securitate
implicită: drepturile se verifică pe calea securizată care îl precedă. (80b, 80c)

O referință din corpul cererii (un FK cules) se rezolvă pe rădăcina
ierarhiei tipului cerut, iar tipul se verifică după. O referință spre un rând
de alt tip al aceleiași ierarhii (de exemplu, un partener ales ca gestiune de
descărcare) e refuzată cu 422 și o frază unică: „rândul ales (id) e X, nu Y”.
O referință inexistentă sau invizibilă e refuzată tot cu 422 („nu există sau
nu e vizibil(ă)”). Răspunsul e determinist: nu depinde de ce a încărcat deja
cererea. Același refuz îl dă gardianul de commit pe OData și în XAF. (89e, 89i)

Erorile de aplicație REST/OData au forma `{"Erori":[...]}`. Erorile de
model binding rămân erori de cerere. Constrângerile cunoscute sunt traduse
în mesaje de domeniu, fără detalii interne ale bazei de date. (39a, 60a, 80d)

Ambele hosturi folosesc reîncărcarea permisiunilor `NoCache`. Tokenul JWT
este păstrat în `sessionStorage`; un 401 curăță sesiunea și trimite la
autentificare pe transporturile REST, DevExtreme și OData. (55f)

## Suprafața OData

Expunerea este explicită. Accesul efectiv depinde de permisiuni și de gărzile
fiecărei entități. (42f, 56)

| Grup | Entități |
|---|---|
| Nomenclatoare cu scriere | Gestiune, TipMaterial, Partener, Produs, Angajat, TipTva, Societate, Imobilizare (regulile fișei la gardianul de commit; starea și datele le scrie doar motorul) (56, 77h, 81e, 87i) |
| Politici cu scriere controlată | MapareD300, MapareD394, PoliticaMiscareSaft, TipDocument, RegulaStoc, RegulaContare, PoliticaConex, PoliticaScadenta, PoliticaValidare, PoliticaTva, PoliticaInchidereTva, PoliticaNumerotare, PoliticaTvaImplicit, PoliticaAmortizare, RegulaDeductibilitate (81e, 87i) |
| Nomenclatoare pentru citire | Judet, UnitateMasura, CodEconomic, SursaFinantare, CodFunctional, Proiect, ContPropriu, UnitateInterna, Lot, Cont, Angajament, Repartitor, RandD300, ClasaProdus, ClasificareImobilizari (56, 81e, 87i) |
| Audit pentru citire | AuditDataItemPersistent, AuditEFCoreWeakReference (81e, 81h) |

`TipDocument` permite modificarea implicitului expus; nu permite crearea,
ștergerea sau schimbarea ancorei de cod. Tabelele de bază pentru raportare
nu devin editabile doar pentru că sunt utilizate de o politică editabilă. (81e)

## Audit

Auditul EF Core este activ în ambele hosturi. Istoricul identifică obiectul
prin numele CLR complet și identificatorul GUID și include autorul.
Entitățile auditului sunt expuse numai pentru citire. Rolul implicit citește
doar auditul propriu; celelalte drepturi se aplică prin securitatea XAF. (53e, 55e, 81h)

Istoricul de audit și `DinSeed` au scopuri distincte. Curățarea scenariilor
de test nu șterge auditul pentru a ascunde operațiile executate. (81d, 81h)

## Formulare React

Ecranele sunt compuse în JSX cu controale concrete. Metadata furnizează
denumiri, tipuri și constrângeri comune; nu este un descriptor executabil de
formular. Coloanele specifice aparțin paginii respective. (8, 42e, 43a)

Regulile de formular de mai jos rămân valabile pentru editorii vii
(politici, nomenclatoare, consolele comenzilor) și descriu paginile de
detaliu ale documentelor așa cum au fost înghețate; acestea nu se extind.
(104d)

Listele de documente sunt compuse din `ListaDocumente` și `GrilaDocumente`
(`nucleu/`): grilă remote cu filtre, sortare și paginare pe server; click-ul
selectează rândul, dublu-click-ul deschide documentul. Pagina dă titlul,
crearea (lipsește la DSC), perioada opțională din URL și coloanele. Consolele
ITV și AMO folosesc aceeași grilă sub previzualizare. (43a, 43c)

Proiecțiile fiscale filtrează pe perioada de DECLARARE, nu pe data faptului:
`jurnal-tva`, `decont-tva`, `d300`, `d394` și SAF-T. Perioada de declarare
are granularitate de LUNĂ: `dataStart`/`dataEnd` se citesc ca luni, deci o
fereastră sub-lunară întoarce luna întreagă. Rândul de jurnal poartă ambele
coordonate — data faptului și perioada de declarare. (F27-D5)
`GET api/proiectii/rectificativa-tva?an=&luna=` întoarce conținutul de
rectificativă al unei perioade: rândurile declarate în ea și scrise după prima
ei închidere, plus agregatul lor pe cheia decontului. Gate-ul este dublu:
existența perioadei se rezolvă pe ușa securizată (404 pentru lună nedefinită
sau invizibilă), iar cifrele cer dreptul de citire pe registrul fiscal (403);
marginile lipsă sau în afara intervalului sunt 400. D300 și D394 poartă
`Rectificativa` și `DiferenteDeclarat`, completate doar când perioada cerută
acoperă exact o lună calendaristică. Toate trei poartă și `PerioadaDeschisa`:
pe o lună redeschisă după prima declarare, conținutul e deja calculat, dar
devine rectificativă abia la re-închidere, iar banda din client o spune.
(F27-D5, 80a, review advers F27, 2')

Formularul deține local întregul DTO de scriere. TanStack Query gestionează
starea citită de pe server, iar URL-ul starea navigabilă. Nu se menține un
al doilea magazin global care copiază aceleași documente. (43c)

Jurnalele de TVA au coloana „Perioada” lângă „Data”, iar textul explicativ
spune că însumarea e pe perioada de declarare. D300 și D394 arată banda
„RECTIFICATIVĂ — diferențe față de declarat” cu agregatul, doar când serverul
o raportează. (F27-D5)

Formularele de culegere au „Data înregistrării” lângă „Dată”. Câmpul gol nu
se trimite, deci serverul aplică implicitul; ecranele documentelor generate o
arată doar. Conversia citire → scriere a fiecărei felii o poartă explicit: un
câmp lipsă de acolo s-ar fi rescris tăcut la fiecare re-salvare. Coloana din
listele de documente rămâne de adăugat. (F27-D4)

Liniile se editează într-un editor separat, apoi se afișează în grilă.
Totalurile, resturile și disponibilitatea comenzilor sunt calculate de
server. Pagina folosește aceste rezultate și nu reproduce contabilitatea
în JavaScript. (42c, 43c)

Evenimentele de inițializare ale widgeturilor nu trebuie să suprascrie
valorile formularului. Modificările utilizatorului sunt recunoscute prin
evenimentul real al controlului. Selectoarele asincrone păstrează temporar
valoarea până la încărcarea elementului selectat. (56e, 57f, 69h)

O citire eșuată afișează eroarea; nu produce un formular gol editabil.
Confirmările sunt inline. Verificarea conținutului unei confirmări ține
cont de `false` și `null`, nu doar de existența unui obiect React.
Comenzile dezactivează acțiunile incompatibile cât timp sunt în curs și
respectă disponibilitatea întoarsă de server. (42e, 57f, 77d)

## Selectoare, căutare și cache

Căutarea uzuală folosește o coloană calculată și stocată în PostgreSQL,
formată din cod/simbol și denumire normalizate la litere mici, fără
diacriticele acoperite de maparea comună C#/SQL/metadata. Coloana și
regulile „ne-gol" stau în tabelul entității EF care declară proprietatea:
o dată pe tabela rădăcinii unei ierarhii TPH (`Repartitori`: `Cautare`,
`CK_Repartitori_Cod_negol`), pe fiecare derivată a unei baze CLR nemapate.
(77a, 77-r2, 89d)

Filtrele text `contains`, `notcontains`, `startswith` și `endswith` sunt
normalizate pe calea DataSourceLoader. Egalitatea și inegalitatea rămân
exacte. Pentru `notcontains`, valoarea nulă este tratată ca text gol.
Transportul OData normalizează literalii filtrelor înainte de trimitere.
Comportamentul nu extinde automat căutarea la CUI, IBAN sau marcă și nu
schimbă căutarea XAF. (77b, 78b, 78c)

Citirea OData `byKey` este partajată prin cache-ul de query, cu identitatea
entității și proiecția cerută. Intrările nu expiră automat; modificările
cunoscute invalidează datele, iar logout-ul golește cache-ul. Limitele
cheilor de proiecție și ale reîmprospătării widgeturilor sunt enumerate
separat în [limite curente](limite-curente.md). (77b)

Grilele de politici citesc prin OData și scriu prin transportul HTTP comun,
pentru a afișa mesajele `Erori`, proveniența și accesul la istoric.
Transportul OData standard al DevExtreme nu păstrează în toate cazurile
aceleași mesaje detaliate la citire. (81i, 80-r1)

## Regimul pe stare în XAF Blazor

Un singur gardian (`Controllers/RegimDocumentController`, cu gemenii lui pe
grila nested a liniilor și pe dialogul liniei) aplică regimul pe stare
(106): `View.AllowEdit/AllowNew/AllowDelete` cu cheia „Regim” și
`Action.Enabled` pe fiecare acțiune al cărei ID se termină cu numele unei
comenzi din vocabular (`Document.Opereaza`, `FacturaIesire.GenereazaDescarcarea`).
Ștergerea standard XAF (`Delete`) e comanda `Sterge` a regimului pe
DetailView-ul documentului. Motivul indisponibilității ajunge în tooltip-ul
acțiunii, iar tooltip-ul propriu revine când comanda redevine disponibilă;
în Blazor tooltip-ul se randează și pe acțiunea dezactivată (probat în
browser, 2026-10-04). Pe lista de documente, `RegimListaController` aplică
pe selecție componenta ieftină a ștergerii (`RegimDocument.MotivStergere`,
fără interogări): o selecție care conține un document ne-Draft are
ștergerea indisponibilă în întregime, cu motivul în tooltip. (106j) Regimul se re-evaluează la
activare, la schimbarea obiectului curent, la `Committed` și la `Reloaded`
(comanda comite în alt context și controllerul de operare face `Refresh`).
La dezactivare, gardianul își retrage cheia „Regim” și tooltip-urile de pe
acțiunile atinse, fiindcă un frame își refolosește controllerele și acțiunile
între view-uri. În configurația curentă fiecare view rădăcină are tabul și
frame-ul lui; în browser, după un document operat, nomenclatorul deschis din
navigație sau din lookup are ștergerea disponibilă (probat 2026-10-04). (106k)
Controllerele de comenzi nu mai poartă `Enabled["Stare"]`; ModelCheck
probează structural că fiecare comandă din toolbar-ul DetailView-ului unui
`Document` numește o comandă a regimului. (106d, 106e)

## Listele XAF Blazor

Modul de acces implicit al ListView-urilor root este `Server`, cu paginare
(fără scroll virtual): o pagină este un query cu join-urile coloanelor de
referință din modelul view-ului plus un COUNT; rândul este entitatea, deci selecția,
detaliul din listă, acțiunile și editarea inline funcționează neschimbate.
`Client` nu este implicit pe niciun ListView root. (85a)

`ServerView` este opt-in per view, doar pe registre append-only citite:
azi numai `Postare`. Lista `Postare` de sub „Registre” e evidența brută a
cubului: fără editare și fără totaluri, cu tranzacția, felul, documentul,
contul, latura, partenerul, gestiunea, unitatea și cele trei măsuri; din rând
se deschide detaliul postării, iar detaliul tranzacției își arată postările,
tot fără editare. Listele vechi de registre au dispărut odată cu entitățile
lor (2026-10-07, D9-D6). (D9-D9) Pagina proiectează doar
coloanele vizibile; detaliul rândului se deschide normal. Precondițiile sunt
verificate de ModelCheck: toate coloanele vizibile sunt mapate sau
`[Calculated]`, orice coloană de referință are `DefaultProperty` pe clasa
țintă, mapat sau calculat, niciun controller nu face cast pe selecție și
nicio regulă Appearance nu atinge un membru nevizibil. `InstantFeedback` și
`InstantFeedbackView` nu sunt implicite; se activează pe un singur view,
după o măsurătoare de pagină peste prag și probe în browser. `DataView` nu
se folosește pe EF Core. (85b, 85d, 85e)

Grilele nested de culegere a liniilor (ListView-urile tipizate puse pe
`Detalii`) sunt declarate `Client` explicit în `ContaUiBaseline`: o colecție
server nu vede liniile nesalvate ale documentului și nu acceptă adăugare
sub grupare. Lookup-urile rămân pe modul forțat de editor. (85c, 85i)

Controllerele rezolvă selecția prin `ObjectSpace.GetObject`, nu prin cast
pe obiectele selectate. O proprietate afișată într-o listă sau folosită ca
`DefaultProperty` este mapată sau `[Calculated]` cu o expresie de criterii
tradusă în SQL. `Lot.Eticheta` este calculată (produs · zz.ll.aaaa · preț
cu 4 zecimale, „(în culegere)" pe lotul fără dată și preț), deci
proiectabilă, sortabilă și căutabilă în lookup-ul de lot; eticheta afișată
de clientul React pe OData este o compunere separată, cu aceeași semantică.
(85f, 85g)

Tipul concret al unui document, al unei linii sau al unui repartitor este
membrul mapat `ClrType` („Tip”), read-only. Ca orice coloană mapată, se
poate afișa, sorta și filtra pe `Server` și `ServerView`, fără join de
moștenire. În XAF Blazor „Tip” apare doar pe listele care amestecă tipuri:
`Document_ListView`, `DocumentTrezorerie_ListView` (plăți și încasări) și
`Repartitor_ListView` (a doua coloană), lookup-ul
`Repartitor_LookupListView` (Predator/Primitor, repartitorii postării
explicite: coloanele Denumire și Tip) și `DocumentDetaliu_LookupListView`
(liniile-sursă, generat de XAF). Lipsește din listele și lookup-urile
frunzelor, unde e constant, din grilele de linii și din orice DetailView.
Mecanismul: `[VisibleInListView(false), VisibleInDetailView(false)]` pe
proprietate (acoperă toate derivatele, inclusiv grupul-mătură al
layout-ului autoritar), iar coloanele de pe cele trei liste ale bazelor se
declară în `ContaUiBaseline.ColoanaTip`. (89a)

## Grilele de linii pe roluri în XAF Blazor

Coloanele grilelor de linii nu se declară pe indici, ci pe roluri (106h).
`ContaUiBaseline.LiniiPeRoluri` declară o dată, pe ierarhia `DocumentDetaliu`,
vocabularul în ordinea de culegere: Directie, Identitate, Provenienta,
Unitate, Cantitate, Pret, Tva, Valori, AtributeLot, Conturi, Parametri.
Fiecare tip de linie umple sloturile pe care le poartă cu membrii lui și lasă
goale (`Drop`) pe cele pe care nu le poartă; un rol al bazei nepurtat se
ascunde pe grila tipului, iar un membru fără rol vine la coadă, în ordinea
generată. Rezultatele motorului sunt `ReadOnly` printr-o singură declarație,
care blochează coloana grilei și itemul dialogului liniei deopotrivă:
`Valoare` pe FCT, NIR, FCL, LDI, DEC, DSC și ASM; `Lot` pe FCT și NIR. NTC și
DVI culeg `Valoare`. Excepțiile pe view (legenda `Valoare în vamă`, o coloană
ascunsă pe o singură grilă, grila DVI) rămân view-scoped și câștigă în fața
rolurilor. Lookup-urile nu sunt atinse. Primitiva e Atlas.DXF 26.1.4.10
(`Columns`/`Slot`/`Drop`, `ReadOnly`; `Views/docs/COLUMN-SLOTS.md`).
Limita: BCS, BTR și RLF (RDC culege `Valoare`) stau pe grila generică și pe dialogul comun cu
DVI, deci `Valoare` nu e blocată acolo (106-r6). Tabloul rolurilor per tip:
`design/format-xaf-documente.md`, axa 2. (106h)

## Ecranele disponibile

| Arie | Conținut |
|---|---|
| Documente | Liste pentru FCT, FCL, NIR, DSC, BTR, BCS, LDI, PLT, INC, DEC, NTC, ASM, RLF, RDC, DVI, PIF, CAS și AMO; paginile lor de detaliu și de culegere sunt înghețate, culegerea curentă e în XAF Blazor (104d) |
| Imobilizări | Fișa ca ecran de nomenclator pe OData, cu panoul „Fișa" (situația la data din URL, parametrii curenți, rândurile registrului) din `GET api/imobilizari/{id}/fisa?laData=`; registrul imobilizărilor la `/imobilizari/registru` din `GET api/imobilizari/registru?laData=`, totaluri de pe server; ambele cer și citirea pe `RegistruImobilizari`. `api/pif`: agregat cules cu lookup de fișă filtrat pe locul primitorului și pe stare, dialogul liniilor de factură de clasă F din `linii-sursa` (plic `{ Candidati, MaiSunt }`, plafon 500, prefill cu restul), parametrii pre-completați pe revizuire din fișă. `api/cas`: antet plus fișele de pe locul predatorului; liniile produse de server. `api/amo`: previzualizare pe an, lună și unitate cu motiv, blocant și cele trei cifre, generare, regenerare cu confirmare, storno (87i, 87j) |
| Declarații vamale | `api/dvi`: agregat cules (antet, linii, `FacturiIds` ca agregat întreg), `facturi-candidate` cu perioadă obligatorie, filtru implicit pe clasa fiscală extra-UE, plicul `{ Candidati, MaiSunt }` cu plafon 500 decis pe interogare și `TipMaterialSugeratId`; cere și citirea pe FCT. Ecranul: lookup TVA filtrat pe `DeImport`, popup de candidați pe luna declarației, totaluri de pe server (86h, 86i) |
| Trezorerie și relații | Stingere manuală în limitele contractelor, vizualizarea relațiilor și comenzile documentului (57d, 76g) |
| TVA lunar | Previzualizare și generare ITV, detaliu și comenzile rezultatului (79e) |
| Contabilitate | Stoc, balanță, balanță pe plan, fișă de cont, registru-jurnal (66, 67) |
| Perioade fiscale | `/perioade`: consolă, nu listă — luna în URL, verificarea cu constatările grupate pe severitate și bifă pe fiecare avertisment, închiderea care trimite cheile bifate, redeschiderea cu motiv, istoricul cu acceptările și lanțul întreg (F27-D2) |
| Fiscalitate | Jurnale de cumpărări/vânzări, decont TVA, D300, D394, SAF-T L/S (68, 69g, 71g) |
| Nomenclatoare | Parteneri, produse, societate; sincronizare individuală ANAF (77h) |
| Politici | Implicite TVA, tipuri TVA, implicitele tipurilor de document, mișcări SAF-T, scadențe, numerotare, închidere TVA, reguli de stoc, reguli de contare (formular popup cu grupuri), politici TVA, conex, validare, mapări D300/D394, politici de amortizare, reguli de deductibilitate, închidere de perioadă; „Explică pe acest tip" din fiecare grilă cu tip de document (81i, 84d, 87j) |
| Explicarea configurației | `/politici/explica`: starea în URL, un card per mecanism cu câștigătorul, candidații eliminați, proveniența și concluzia serverului (84h) |
| Controlul configurației | Verificarea profilului, proveniență și istoric de audit (81g, 81h, 81i) |

DSC, ITV și AMO nu au flux generic de creare prin `/nou`; provin din
comenzile specifice. Rutele `/nou` ale întregului client poartă o cheie de
montare proprie: la tranziția directă de la un detaliu la `/nou`, formularul
se remontează și nu păstrează starea documentului anterior. Grilele de
politici pot deschide un formular popup cu grupuri definite de ecran; rândul
nou primește propuneri vizibile pentru câmpurile al căror gol ar fi refuzat
de gardian. (58, 79a, 84d, 87g, 87j)

## Contracte generate

Din 2026-09-25, ștergerea împerecherii prin API și acțiunea XAF
„Șterge împerecherea” folosesc aceeași comandă atomică: eliberează suma
nominalizată în cub și șterg legătura. Gate-ul rămâne Delete pe instanța
vizibilă, înaintea refuzurilor de domeniu. CRUD-ul generic este refuzat
de gardian; în perioadă închisă se folosește „Desfă împerecherea”.

OpenAPI, tipurile TypeScript și metadata de model sunt generate și păstrate
în repository. Clientul nu folosește un client API generic generat pentru
toate operațiile. DTO-urile și atributele serverului rămân sursa contractului. (43d, 56)

Verificarea de drift trebuie să confirme că fișierele generate corespund
sursei. Tipurile TypeScript nu înlocuiesc verificarea serverului, iar
atributele de validare XAF și cele ale contractului HTTP au consumatori
diferiți. (43b, 43d, 77k)

### Diferențele NIR (098/099, 2026-09-24)

API-ul NIR citește proveniența recepției și marcajul liniilor acoperite;
clientul culege cauza și imputatul, cu cantitate zero permisă pe linia
acoperită. Legăturile cu sursa sunt stabilite de server, nu intră în
WriteDto. Liniile-sursă nu se șterg și tipul/lotul lor nu se schimbă.
Cauza implicită la operare este InClarificare pentru minus și Plus pentru
plus; formularul explică aceste implicite. Cantitatea/valoarea constatată
sunt distincte de delta economică postată de declarant.

Politica diferențelor se editează prin OData și ecranul
`/politici/diferente`, sub aceleași drepturi de Configurator ca celelalte
politici. Contul pentru personal este opțional și separat de contul normal.

La schimbarea cauzei NIR din Imputabila, editorul golește imediat imputatul.
API-ul ignoră imputatul din payload pentru celelalte cauze și îl golește
și la delta zero; un imputat ascuns nu rămâne atașat constatării.

### Raportul partidelor pe cub (101, 2026-09-25)

`GET /api/proiectii/partide` și pagina `/partide` expun o partidă pe rând,
cu cont, partener/angajat, document opțional, data nașterii, sens și rest.
Filtrele sunt `laData`, `contrapartidaId`, `sens`; cheia paginării este
unitate × cont × partener. Eticheta documentului nu elimină soldul când
lipsește sau nu este vizibilă. Postările sunt citite prin ObjectSpace secured.

`documente-cu-rest` acceptă `documentCurentId` și `stinge` pentru candidați
compatibili; `Disponibil` este limita exactă a perechii, iar panourile o
consumă ca atare. Crearea trece
prin gate-ul de drepturi și vizibilitatea ambelor documente, apoi prin
comanda atomică în ObjectSpace non-secured, la fel ca desfacerea și ștergerea.

## Citirea fiscală și confirmarea depunerii (103)

DTO/Apply pentru FCT/FCL/DEC/DVI/RDC/RLF transmit exigibilitatea și, la
achiziții, data primirii. Clientul propune primirea din înregistrare, fără
să suprascrie o dată introdusă explicit. Jurnalul filtrează perioada D300
și afișează separat perioada D394 și reperele istorice.

`GET/POST /api/depuneri-declaratii/{formular}/{an}/{luna}` citește sau
confirmă depunerea. POST cere versiunea exportată, drept de scriere și
perioadă vizibilă; refuzurile au 400/403/404/422. Aceeași versiune este
idempotentă. OData expune istoricul numai pentru citire. D300/D394 oferă
confirmarea pentru o singură lună, cu versiunea și momentul afișate.
DTO-urile D300/D394 emit `VersiuneExportata`, amprentă a faptelor lunii.
UI descarcă exportul JSON și reține acea versiune pentru confirmare;
nu acceptă etichetă liberă. POST refuză `DEPUNERE_VERSIUNE_DEPASITA`
dacă s-au schimbat faptele între export și confirmare. JSON-ul nu este
XML ANAF; parametrii externi D300 nu sunt certificați de amprenta faptelor.

`/politici/tva-saft` editează mapările versionate și calificarea istorică.
Metadata include enumurile proprietăților persistente, inclusiv cele din
nucleu. Rapoartele fiscale cer citire pe `Postare`; lipsa dreptului pe
întregul tip este 403, iar filtrarea pe obiect/membru se aplică sursei comune.
Această validare fiscală nu certifică toate câmpurile SourceDocuments SAF-T.

D406 L (`GET api/proiectii/saft` și `…/saft/xml`) și S (`…/saft/stocuri` și
`…/saft/stocuri/xml`) citesc cubul (`SaftProiectii.SaftPeCub`,
`SaftStocuriPeCub`). Excepție de la filtrarea de mai sus: exportul
cere citire necondiționată. Orice criteriu de rând sau de membru pe tabelele
citite de export (`SaftAcces.Citite` pe L, `SaftAcces.CititeStocuri` pe S, cu
toate tipurile mapate în ele) dă 403
`SAFT_ACCES_INCOMPLET` pe ambele uși, înaintea proiecției. Corpul numește
tipurile și membrii restricționați, fără rânduri ori sume: întâi tipurile
citite de secțiune, fără frunzele TPH ale unei baze deja restricționate. Criteriile se
citesc din `ISelectDataSecurity`, aceeași sursă din care EF Core filtrează.
`CanRead(Type)` nu ajunge, fiindcă răspunde `true` sub restricții
condiționale. Refuzurile proiecției (`SaftDto.Refuzuri`) apar în sumar
(`Refuzuri`, 200; ecranul le arată înaintea descărcării și ține butonul
XML inactiv) și dau 422 pe fișier, înaintea primului byte. Fișa contului
deschisă din S3 revine la SAF-T (`inapoi`, numai rută internă). Cusăturile ecranului L compară declarația
cu cubul: rulajul balanței, faptele fiscale și baza lor pe sens. Cele ale
ecranului S compară fiecare poziție cu postările pe lot ale lunii și stocul
pe cont cu soldul contabil; diferența pe cont se sparge în „(sold inițial)”
(din snapshot) și componentele lunii pe tipul documentului, fără a citi
istoricul (B8-Q3). (SAF-D4, S2-D5, S3-D6)
Soldurile de cont și ale terților din L și S pornesc din snapshot-ul contabil,
deci accesul complet cerut include perioadele fiscale și soldurile de perioadă.
(SAFT-r4, X-D5)

## Explicația deciziei (X-D4)

`GET api/proiectii/explicatii/{tranzactieId}` întoarce `ExplicatieTranzactieDto`:
felul tranzacției, documentul și `Origini` — explicația proprie, cea referită
sau, la storno, ale originalelor; lista e goală pe împerechere, desfacere și
deschidere. Fiecare origine are purtătorul, declarantul, versiunea, perioada,
politica și liniile cu ieșirile (unitate, cantitate, valoare, soldul dinainte
sau sursa valorii declarate), stingerile (partidă, măsură, sold citit),
conturile rezolvate și partidele deschise. Nu există pagină
React (104d).

Tranzacția pe care contextul securizat nu o vede dă 404, ca una inexistentă.
Explicația dezvăluie solduri întregi ale unităților și valori ale altor linii,
deci cere citire necondiționată pe `ExplicatieAcces.Citite` (`Tranzactie`,
`Postare`, `Document`, `DocumentDetaliu`, `Lot`, `Produs`, `Cont`,
`Repartitor`, cu toate tipurile mapate în aceleași tabele). Orice criteriu de
rând sau de membru dă 403 `EXPLICATIE_ACCES_INCOMPLET` înaintea citirii, cu
tipurile și membrii restricționați în corp și fără nicio valoare; nu există
proiecție parțială. Mecanismul e `Api.AccesComplet`, comun cu D406
(`SaftAcces` îl apelează cu listele lui).
