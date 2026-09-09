# API și client

**Actualizat: 2026-09-09.** [Index](README.md)

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
| Nomenclatoare cu scriere | Gestiune, TipMaterial, Partener, Produs, Angajat, TipTva, Societate (56, 77h) |
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
diacriticele acoperite de maparea comună C#/SQL/metadata. (77a)

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

## Ecranele disponibile

| Arie | Conținut |
|---|---|
| Documente | Liste și detalii pentru FCT, FCL, NIR, DSC, BTR, BCS, LDI, PLT, INC, DEC, NTC, ASM, RLF și RDC |
| Trezorerie și relații | Stingere manuală în limitele contractelor, vizualizarea relațiilor și comenzile documentului (57d, 76g) |
| TVA lunar | Previzualizare și generare ITV, detaliu și comenzile rezultatului (79e) |
| Contabilitate | Stoc, balanță, balanță pe plan, fișă de cont, registru-jurnal (66, 67) |
| Fiscalitate | Jurnale de cumpărări/vânzări, decont TVA, D300, D394, SAF-T L/S (68, 69g, 71g) |
| Nomenclatoare | Parteneri, produse, societate; sincronizare individuală ANAF (77h) |
| Politici | Implicite TVA, tipuri TVA, implicitele tipurilor de document, mișcări SAF-T, scadențe, numerotare, închidere TVA (81i) |
| Controlul configurației | Verificarea profilului, proveniență și istoric de audit (81g, 81h, 81i) |

DSC și ITV nu au flux generic de creare prin `/nou`; provin din comenzile
specifice. Acoperirea editorilor de politici este mai restrânsă decât
suprafața OData. (58, 79a, 81k)

## Contracte generate

OpenAPI, tipurile TypeScript și metadata de model sunt generate și păstrate
în repository. Clientul nu folosește un client API generic generat pentru
toate operațiile. DTO-urile și atributele serverului rămân sursa contractului. (43d, 56)

Verificarea de drift trebuie să confirme că fișierele generate corespund
sursei. Tipurile TypeScript nu înlocuiesc verificarea serverului, iar
atributele de validare XAF și cele ale contractului HTTP au consumatori
diferiți. (43b, 43d, 77k)
