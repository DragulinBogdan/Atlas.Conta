# Ecrane

**Actualizat: 2026-10-16.** [Index](README.md)

Formularele React (selectoare, căutare, cache, ecranele fiscale) și ecranele XAF Blazor (regim, liste, grile de linii), plus harta ecranelor disponibile.

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

Formularul deține local întregul DTO de scriere. TanStack Query gestionează
starea citită de pe server, iar URL-ul starea navigabilă. Nu se menține un
al doilea magazin global care copiază aceleași documente. (43c)

Formularele de culegere au „Data înregistrării” lângă „Dată”. Câmpul gol nu
se trimite, deci serverul aplică implicitul; ecranele documentelor generate o
arată doar. Conversia citire → scriere a fiecărei felii o poartă explicit: un
câmp lipsă de acolo s-ar rescrie tăcut la fiecare re-salvare. Listele de
documente nu au coloana „Data înregistrării”. (F27-D4)

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

## Ecranele fiscale

Proiecțiile fiscale filtrează pe perioada de DECLARARE, nu pe data faptului:
`jurnal-tva`, `decont-tva`, `d300`, `d394` și SAF-T. Perioada de declarare
are granularitate de LUNĂ: `dataStart`/`dataEnd` se citesc ca luni, deci o
fereastră sub-lunară întoarce luna întreagă. Rândul de jurnal poartă ambele
coordonate — data faptului și perioada de declarare. (F27-D5)

Jurnalele de TVA au coloana „Perioada” lângă „Data”, iar textul explicativ
spune că însumarea e pe perioada de declarare. D300 și D394 arată banda
„RECTIFICATIVĂ — diferențe față de declarat” cu agregatul, doar când serverul
o raportează. (F27-D5)

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

D300 și D394 au butonul „Descarcă XML”, cu bifele datelor de perioadă
(D300: temeiul, rambursarea, cele patru bife de taxare inversă internă; D394:
persoanele afiliate, schimbarea opțiunii), ținute în URL. Butonul stă inactiv
cât `RefuzuriXml` are cauze, iar cauzele apar într-un panou; refuzul cererii
(403, 422) apare în același loc. Pe lună, după descărcarea XML, confirmarea
depunerii trimite amprenta fișierului și parametrii lui, iar ecranul arată
începutul amprentei; fără XML, confirmarea folosește versiunea exportului
JSON. Confirmarea activă se retrage cu motiv, din același panou. Pe
trimestru nu există amprentă (E2-r1). D394 arată lângă buton tabelul
„Rotunjirea pe partener”, pe tip × cotă, când diferența nu e zero, iar sub
listă panoul „Excluse din declarație” (motivul fiecărei perechi excluse) și
„Puntea D300 → D394”, numai rândurile cu diferență, cu documentele decalate.
Grila `MapariD394` primește motivul excluderii și tipul gol. (122 b, 122 i, 123) Câmpurile antetului se culeg în XAF,
pe societate, iar plajele de facturi și categoria de bunuri pe nomenclatoarele
lor. (E2-D8)

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
în Blazor tooltip-ul se randează și pe acțiunea dezactivată. (106d, 106e)

Pe lista de documente, `RegimListaController` aplică pe selecție componenta
ieftină a ștergerii (`RegimDocument.MotivStergere`, fără interogări): o
selecție care conține un document ne-Draft are ștergerea indisponibilă în
întregime, cu motivul în tooltip. (106j)

Regimul se re-evaluează la activare, la schimbarea obiectului curent, la
`Committed` și la `Reloaded` (comanda comite în alt context și controllerul
de operare face `Refresh`). La dezactivare, gardianul își retrage cheia
„Regim” și tooltip-urile de pe acțiunile atinse, fiindcă un frame își
refolosește controllerele și acțiunile între view-uri. În configurația
curentă fiecare view rădăcină are tabul și frame-ul lui; în browser, după un
document operat, nomenclatorul deschis din navigație sau din lookup are
ștergerea disponibilă. (106k)

Controllerele de comenzi nu poartă `Enabled["Stare"]`; ModelCheck probează
structural că fiecare comandă din toolbar-ul DetailView-ului unui `Document`
numește o comandă a regimului. (106d, 106e)

## Listele XAF Blazor

### Modul de acces

Modul de acces implicit al ListView-urilor root este `Server`, cu paginare
(fără scroll virtual): o pagină este un query cu join-urile coloanelor de
referință din modelul view-ului plus un COUNT; rândul este entitatea, deci selecția,
detaliul din listă, acțiunile și editarea inline funcționează neschimbate.
`Client` nu este implicit pe niciun ListView root. (85a)

`ServerView` este opt-in per view, doar pe registre append-only citite;
pagina proiectează doar coloanele vizibile, iar detaliul rândului se deschide
normal. Precondițiile sunt verificate de ModelCheck: toate coloanele vizibile
sunt mapate sau `[Calculated]`, orice coloană de referință are
`DefaultProperty` pe clasa țintă, mapat sau calculat, niciun controller nu
face cast pe selecție și nicio regulă Appearance nu atinge un membru
nevizibil. `InstantFeedback` și `InstantFeedbackView` nu sunt implicite; se
activează pe un singur view, după o măsurătoare de pagină peste prag și probe
în browser. `DataView` nu se folosește pe EF Core. (85b, 85d, 85e)

Grilele nested de culegere a liniilor (ListView-urile tipizate puse pe
`Detalii`) sunt declarate `Client` explicit în `ContaUiBaseline`: o colecție
server nu vede liniile nesalvate ale documentului și nu acceptă adăugare
sub grupare. Regula e a fiecărui `[TipDetaliu]`, nu a unei liste: și
grilele pe care operatorul nu le culege (AMO, închiderea conturilor de
rezultat) sunt `Client`, `ReadOnly`. Pe o colecție server, grila validează
coloanele pe tipul de bază al colecției și cade la primul membru al
derivatei. Lookup-urile rămân pe modul forțat de editor. (85c, 85i, F26-D12)

### Lista de evidență a cubului

Singurul view `ServerView` este lista de evidență a cubului,
`PostareVizual_ListView` de sub „Registre”. `Cub.PostareVizual` e o entitate
de citire mapată `ToView("PostareVizual")`, fără `BaseObject`, cu cheia `ID`,
toate coloanele postării și codurile nomenclatoarelor alături (`ContSimbol`,
`RepartitorCod`, `PartenerFiscalCod`, `LocCod`, `ProdusCod`, `UnitateCod`,
`DocumentNumar`, `TipTvaCod`, dimensiunile bugetare, `TranzactieFel`);
view-ul SQL e creat în `InitialCreate`, odată cu tabela, cu `LEFT JOIN` pe
nomenclatoare — `Repartitori` de trei ori, pentru repartitor, partener fiscal
și loc — și o expresie pentru unitate (partida = codul repartitorului + data
deschiderii; lotul = codul produsului + data lotului; fișa = numărul de
inventar). (D9-A12)

Contraponderea e coloana postării, afișată cu numele enumului (`Tert`,
`Consum`, `Inventar`, `Transformare`). Coloanele vizibile, în ordine:
tranzacție, document, data, cont, latură, repartitor, partener fiscal, loc,
contrapondere, produs, unitate, cantitate, valoare, valoare în valută; fără
identificatori bruți, fără editare, fără totaluri; detaliul rândului se
deschide în citire. (S5, C111-Q14)

La activarea listei, `PostareVizualController` cere
`Cub.Citiri.Vizibilitate.AccesLista` (lipsurile de citire completă pe
`Postare`, prin `AccesComplet.Lipsuri`): cu lipsuri, criteriul listei e fals
și se arată fraza porților; nu există drept propriu pe `PostareVizual`.
Gardianul refuză crearea, modificarea și ștergerea unei `PostareVizual` pe
orice ușă securizată, ca pe `Postare`; tipul nu e expus pe OData, în OpenAPI
și în metadata clientului (`neexpunere-cub.py`). (108 d, D9-D9, D9-A12)

`Postare` nu are intrare de navigare; lista imbricată a tranzacției
(`Tranzactie_Postari`) rămâne pe `Postare`, `Client`, fără editare, cu
aceleași patru coordonate (repartitor, partener fiscal, loc, contrapondere),
ca identificatori. Listele de registre dinaintea cubului nu există. (D9-D6,
D9-D9, D9-A12)

### Selecția și proprietățile afișate

Controllerele rezolvă selecția prin `ObjectSpace.GetObject`, nu prin cast
pe obiectele selectate. O proprietate afișată într-o listă sau folosită ca
`DefaultProperty` este mapată sau `[Calculated]` cu o expresie de criterii
tradusă în SQL. `Lot.Eticheta` este calculată (produs · zz.ll.aaaa · preț
cu 4 zecimale, „(în culegere)" pe lotul fără dată și preț), deci
proiectabilă, sortabilă și căutabilă în lookup-ul de lot; eticheta afișată
de clientul React pe OData este o compunere separată, cu aceeași semantică.
(85f, 85g)

### Coloana „Tip”

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

Coloanele grilelor de linii nu se declară pe indici, ci pe roluri.
`ContaUiBaseline.LiniiPeRoluri` declară o dată, pe ierarhia `DocumentDetaliu`,
vocabularul în ordinea de culegere: Directie, Identitate, Provenienta,
Unitate, Cantitate, Pret, Tva, Valori, AtributeLot, Conturi, Parametri.
Fiecare tip de linie umple sloturile pe care le poartă cu membrii lui și lasă
goale (`Drop`) pe cele pe care nu le poartă; un rol al bazei nepurtat se
ascunde pe grila tipului, iar un membru fără rol vine la coadă, în ordinea
generată. (106h)

Rezultatele motorului sunt `ReadOnly` printr-o singură declarație,
care blochează coloana grilei și itemul dialogului liniei deopotrivă:
`Valoare` pe NIR, FCL, LDI, DEC, DSC și ASM; `Lot` pe NIR. NTC și
DVI culeg `Valoare`. Pe FCT felul liniei decide: linia obișnuită are `Lot` și
`Valoare` blocate, iar linia de corecție a valorii lotului (prima coloană,
„Corecție de valoare a lotului”) le culege și blochează produsul, cantitatea,
prețul, valoarea TVA, avansul și atributele de lot (`[Appearance]` pe
`FacturaIntrareDetaliu`). Tipul de TVA rămâne editabil: corecția declarată
taxabilă se refuză la operare cu `CV_STRUCTURA`. Lookup-ul lotului oferă loturile recepționate în
gestiunea facturii pe o factură operată a aceluiași furnizor
(`Culegere/LoturiCorectabile`). Comutarea felului golește linia, iar tipul se
completează din produsul lotului la salvare. (CV-D1) Excepțiile pe view (legenda `Valoare în vamă`, o coloană
ascunsă pe o singură grilă, grila DVI) rămân view-scoped și câștigă în fața
rolurilor. Lookup-urile nu sunt atinse. Primitiva e Atlas.DXF 26.1.4.10
(`Columns`/`Slot`/`Drop`, `ReadOnly`; `Views/docs/COLUMN-SLOTS.md`). (106h)

Limita: BCS, BTR și RLF (RDC culege `Valoare`) stau pe grila generică și pe
dialogul comun cu DVI, deci `Valoare` nu e blocată acolo (106-r6). Tabloul
rolurilor per tip: `design/format-xaf-documente.md`, axa 2. (106h)

## Ecranele disponibile

| Arie | Conținut |
|---|---|
| Documente | Liste pentru FCT, FCL, NIR, DSC, BTR, BCS, LDI, PLT, INC, DEC, NTC, ASM, RLF, RDC, DVI, PIF, CAS și AMO; paginile lor de detaliu și de culegere sunt înghețate, culegerea curentă e în XAF Blazor (104d) |
| Imobilizări | Fișa ca ecran de nomenclator pe OData, cu panoul „Fișa" (situația la data din URL, parametrii curenți, rândurile registrului) din `GET api/imobilizari/{id}/fisa?laData=`; registrul imobilizărilor la `/imobilizari/registru` din `GET api/imobilizari/registru?laData=`, totaluri de pe server; ambele cer citirea completă pe `Postare` ([api](api.md#citirea-completă-și-citirea-filtrată-a-cubului)). `api/pif`: agregat cules cu lookup de fișă filtrat pe locul primitorului și pe stare, dialogul liniilor de factură de clasă F din `linii-sursa` (plic `{ Candidati, MaiSunt }`, plafon 500, prefill cu restul), parametrii pre-completați pe revizuire din fișă. `api/cas`: antet plus fișele de pe locul predatorului; liniile produse de server. `api/amo`: previzualizare pe an, lună și unitate cu motiv, blocant și cele trei cifre, generare, regenerare cu confirmare, storno (87i, 87j) |
| Declarații vamale | `api/dvi`: agregat cules (antet, linii, `FacturiIds` ca agregat întreg), `facturi-candidate` cu perioadă obligatorie, filtru implicit pe clasa fiscală extra-UE, plicul `{ Candidati, MaiSunt }` cu plafon 500 decis pe interogare și `TipMaterialSugeratId`; cere și citirea pe FCT. Ecranul: lookup TVA filtrat pe `DeImport`, popup de candidați pe luna declarației, totaluri de pe server (86h, 86i) |
| Trezorerie și relații | Stingere manuală în limitele contractelor, vizualizarea relațiilor și comenzile documentului (57d, 76g) |
| TVA lunar | Previzualizare și generare ITV, detaliu și comenzile rezultatului (79e) |
| Închiderea anului | În XAF Blazor: „Generează închiderea” pe lista închiderilor conturilor de rezultat (an, lună, unitate; luna implicită urmează ultimei închideri vii), detaliul cu liniile needitabile; pe lista perioadelor, „Închide exercițiul” și „Redeschide exercițiul” pe decembrie, cu dialogul constatărilor, respectiv al motivului; „Redeschide perioada” e inactiv pe decembrie cât exercițiul e închis. În React, coloana „Exercițiu” pe `/perioade` (E3-D3, E3-D7) |
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

## Limitele implementării

- Cheia cache-ului `byKey` combină proiecțiile de selecție și expandare
  într-o formă care nu distinge toate combinațiile; o proiecție nouă nu e
  sigură fără verificarea cheii. (77b)
- Niciun view XAF Blazor nu folosește `InstantFeedback`/`InstantFeedbackView`;
  activarea cere o măsurătoare peste prag și probe în browser. (85-r2)
- O proprietate nemapată care ar ajunge într-o listă XAF e refuzată de
  ModelCheck. Mărimea paginii grilelor nu e calibrată; `RegulaStoc_ListView`
  poartă un override `Server` redundant cu opțiunea aplicației. (85-r4, 85-r5)
- În modul `Server`, navigațiile coloanelor ascunse rămân în interogarea
  paginii; scoaterea lor din modelul view-ului nu e făcută. Gruparea încarcă
  primele rânduri ale fiecărui grup, iar `Refresh` execută pagina de două
  ori. (85-r6, 85-r7, 85-r8, 89)
- Dialogul de închidere arată constatările ca listă imbricată cu bifă; bifa
  pe o constatare blocantă e dezactivată prin regulă de aspect, iar
  autoritatea rămâne serviciul, care refuză blocantele indiferent de ce s-a
  trimis. (F27-D2)

<!-- gen:decizii -->
## Decizii

Deciziile care numesc pagina în `Arii:` (27; titlurile în [index](../decizii/README.md); generat de `docskit gen`, nu se editează): 8, 42 (amendată de 104, 106), 43 (amendată de 104), 56 (amendată de 77, 81), 57, 58, 66, 67, 68, 69, 71 (amendată de 72), 76, 77, 78, 79 (amendată de 80), 80, 81, 84, 85, 86, 87 (amendată de 97), 89, 104 (amendată de 106), 106, 122 (amendată de 123), 123, 124.
<!-- /gen:decizii -->
