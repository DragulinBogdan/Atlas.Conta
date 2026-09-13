# API și client

**Actualizat: 2026-09-13.** [Index](README.md)

## Împărțirea responsabilităților

`Module` deține modelul, regulile de domeniu, DTO-urile și serviciile comune.
WebApi asigură transportul, autentificarea și integrarea HTTP. XAF Blazor
folosește același domeniu. React compune explicit paginile și formularele. (5, 42f, 43)

Contractele documentelor sunt specifice tipului concret. Nu există un
endpoint generic care interpretează o schemă de document primită ca date.
DTO-urile de scriere și de citire sunt distincte; navigațiile EF nu sunt
contractul public. Referințele din scriere sunt identificatori expliciți. (6, 7, 42d)

## Salvare și comenzi

Crearea și salvarea rulează în ObjectSpace securizat. O comandă verifică
accesul asupra documentului înainte să execute motorul într-un ObjectSpace
propriu. Niciun identificator furnizat de client nu autorizează singur
accesul prin contextul nesecurizat. (42b, 55b)

PUT reprezintă starea completă a formularului: liniile sunt reconciliate,
iar câmpurile opționale absente se golesc conform contractului. PATCH OData
este parțial: absența unei proprietăți o păstrează. Enumerările sunt șiruri
și sunt validate înainte de crearea obiectului. Filtrele selectoarelor
ajută utilizatorul; validarea serverului rămâne obligatorie. (56, 57a, 77h)

Operarea, anularea și stornarea sunt comenzi, nu modificări directe ale
stării prin DTO. Regulile și tranzacția lor sunt descrise în
[domeniu și operare](domeniu-si-operare.md). (42b)

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

Crearea cere drepturile de creare și scriere; modificarea cere scriere, iar
ștergerea dreptul aferent. Verificările nu se amână până după execuția
regulilor de domeniu. Un context nesecurizat nu are strategie de securitate
implicită: drepturile se verifică pe calea securizată care îl precedă. (80b, 80c)

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
| Nomenclatoare cu scriere | Gestiune, TipMaterial, Partener, Produs, Angajat, TipTva, Societate (56, 77h, 81e) |
| Politici cu scriere controlată | MapareD300, MapareD394, PoliticaMiscareSaft, TipDocument, RegulaStoc, RegulaContare, PoliticaConex, PoliticaScadenta, PoliticaValidare, PoliticaTva, PoliticaInchidereTva, PoliticaNumerotare, PoliticaTvaImplicit (81e) |
| Nomenclatoare pentru citire | Judet, UnitateMasura, CodEconomic, SursaFinantare, CodFunctional, Proiect, ContPropriu, UnitateInterna, Lot, Cont, Angajament, Repartitor, RandD300, ClasaProdus (56, 81e) |
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

Formularul deține local întregul DTO de scriere. TanStack Query gestionează
starea citită de pe server, iar URL-ul starea navigabilă. Nu se menține un
al doilea magazin global care copiază aceleași documente. (43c)

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
o dată pe rădăcina TPT, pe fiecare derivată a unei baze CLR nemapate.
(77a, 77-r2, 2026-09-13)

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

## Listele XAF Blazor

Modul de acces implicit al ListView-urilor root este `Server`, cu paginare
(fără scroll virtual): o pagină este un query cu join-urile coloanelor de
referință din modelul view-ului plus un COUNT; rândul este entitatea, deci selecția,
detaliul din listă, acțiunile și editarea inline funcționează neschimbate.
`Client` nu este implicit pe niciun ListView root. (85a)

`ServerView` este opt-in per view, doar pe registre append-only citite:
`RegistruStoc`, `RegistruContabil`, `RegistruTva`. Pagina proiectează doar
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

## Ecranele disponibile

| Arie | Conținut |
|---|---|
| Documente | Liste și detalii pentru FCT, FCL, NIR, DSC, BTR, BCS, LDI, PLT, INC, DEC, NTC, ASM, RLF, RDC și DVI |
| Declarații vamale | `api/dvi`: agregat cules (antet, linii, `FacturiIds` ca agregat întreg), `facturi-candidate` cu perioadă obligatorie, filtru implicit pe clasa fiscală extra-UE, plicul `{ Candidati, MaiSunt }` cu plafon 500 decis pe interogare și `TipMaterialSugeratId`; cere și citirea pe FCT. Ecranul: lookup TVA filtrat pe `DeImport`, popup de candidați pe luna declarației, totaluri de pe server (86h, 86i) |
| Trezorerie și relații | Stingere manuală în limitele contractelor, vizualizarea relațiilor și comenzile documentului (57d, 76g) |
| TVA lunar | Previzualizare și generare ITV, detaliu și comenzile rezultatului (79e) |
| Contabilitate | Stoc, balanță, balanță pe plan, fișă de cont, registru-jurnal (66, 67) |
| Fiscalitate | Jurnale de cumpărări/vânzări, decont TVA, D300, D394, SAF-T L/S (68, 69g, 71g) |
| Nomenclatoare | Parteneri, produse, societate; sincronizare individuală ANAF (77h) |
| Politici | Implicite TVA, tipuri TVA, implicitele tipurilor de document, mișcări SAF-T, scadențe, numerotare, închidere TVA, reguli de stoc, reguli de contare (formular popup cu grupuri), politici TVA, conex, validare, mapări D300/D394; „Explică pe acest tip" din fiecare grilă cu tip de document (81i, 84d) |
| Explicarea configurației | `/politici/explica`: starea în URL, un card per mecanism cu câștigătorul, candidații eliminați, proveniența și concluzia serverului (84h) |
| Controlul configurației | Verificarea profilului, proveniență și istoric de audit (81g, 81h, 81i) |

DSC și ITV nu au flux generic de creare prin `/nou`; provin din comenzile
specifice. Grilele de politici pot deschide un formular popup cu grupuri
definite de ecran; rândul nou primește propuneri vizibile pentru câmpurile al
căror gol ar fi refuzat de gardian. (58, 79a, 84d)

## Contracte generate

OpenAPI, tipurile TypeScript și metadata de model sunt generate și păstrate
în repository. Clientul nu folosește un client API generic generat pentru
toate operațiile. DTO-urile și atributele serverului rămân sursa contractului. (43d, 56)

Verificarea de drift trebuie să confirme că fișierele generate corespund
sursei. Tipurile TypeScript nu înlocuiesc verificarea serverului, iar
atributele de validare XAF și cele ale contractului HTTP au consumatori
diferiți. (43b, 43d, 77k)
