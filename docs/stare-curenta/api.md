# API

**Actualizat: 2026-10-16.** [Index](README.md)

Ușile HTTP: salvarea și comenzile, rutele de perioade, corecție și împerecheri, proiecțiile, securitatea și refuzurile, OData, auditul și contractele generate.

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

Operarea, anularea și stornarea sunt comenzi, nu modificări directe ale
stării prin DTO. Regulile și tranzacția lor sunt descrise în
[documente](documente.md). (42b)

Comenzile care scriu sunt seriale per bază: o comandă trimisă cât timp alta
scrie așteaptă. Dacă așteptarea depășește timpul de comandă al conexiunii,
răspunsul e 422 cu `SCRIERE_OCUPATA`, fără nimic scris; cererea se poate
retrimite. Același refuz îl poate da salvarea unui draft care adaugă linii.
Citirile, dry-run-ul și salvarea fără linii noi nu așteaptă. (X-D6, 108f)

PUT reprezintă starea completă a formularului: liniile sunt reconciliate,
iar câmpurile opționale absente se golesc conform contractului. PATCH OData
este parțial: absența unei proprietăți o păstrează. Enumerările sunt șiruri
și sunt validate înainte de crearea obiectului. Filtrele selectoarelor
ajută utilizatorul; validarea serverului rămâne obligatorie. (56, 57a, 77h)

### Adaptorii de scriere

`Api/*Apply` sunt adaptorii API ai culegerii (L3). Mapează DTO-ul, cheamă
`CulegereDocument` pe fiecare linie și `InainteDeSalvare` înaintea commit-ului.
Nu calculează valori, loturi sau implicite și nu poartă reguli proprii;
refuzurile culegerii vin de la gardianul de commit, ca 422. (104c)

- Pe liniile cu produs (FCT, FCL, NIR, LDI, ASM), `TipMaterialId` e opțional:
  lipsa lui înseamnă tipul produsului, iar lipsa ambelor e refuzată. (104c)
- Linia FCT cu `CorectieValoareLot` poartă `LotId` și `Valoare` (delta
  semnată); produsul, cantitatea, prețul, valoarea TVA, avansul și atributele
  de lot lipsesc. Pe linia obișnuită, `LotId` și `Valoare` lipsesc. Amestecul
  e refuzat de adaptor ca formă a cererii, înaintea oricărei scrieri, ca
  parse-ul enum-urilor (F3-D5); regulile de structură ale corecției rămân ale
  declarantului (`CV_STRUCTURA`): `TipTvaId` pe linia de corecție se salvează
  pe draft, iar operarea refuză corecția taxabilă. Schimbarea felului unei linii existente trece
  prin `CulegereDocument.CorectieComutata`. `ReadDto` expune
  `CorectieValoareLot`. (CV-D1)
- `DataPrimire` absentă rămâne goală și înseamnă data înregistrării; DTO-ul
  de citire o arată astfel. (104c)
- `ValoareTva` prezentă pe linie e TVA-ul cules explicit; un 0 explicit e
  refuzat cu 422 când cota tipului dă taxă (pe DVI, câmpul ne-nullable 0
  înseamnă necules). (104c)
- Refuzul CAS pe fișa fără politică de amortizare e al culegerii (L3);
  `CasApply` îl cheamă înaintea creării documentului, numai pentru ordinea
  mesajului. (104c)
- Fiecare DTO de scriere al unui document poartă `DataInregistrare` ca dată
  opțională: absentă înseamnă „data documentului”, nu „gol”. Adaptorul comun
  (`Api/DocumentApply.AplicaDate`) aplică implicitul și refuză, de domeniu, o
  dată a înregistrării anterioară datei documentului, pe toate ușile de
  scriere. DTO-urile de citire ale documentelor o expun alături de `Data`.
  (F27-D4)

### Affordance-urile de citire

Affordance-urile de pe DTO-urile de citire (`PoateEdita`, `PoateOpera`,
`PoateAnula`, `PoateStorna`, `PoateSterge`, `PoateRegenera`, `PoateDistribui`,
`PoateStinge`, `PoateGeneraDescarcare`) sunt proiecția regimului pe stare
(`RegimDocument`), aceeași sursă ca acțiunile XAF; `Apply` nu le calculează
din `Stare`. Singura excepție declarată e `DscApply.PoateEdita = false`,
fapt al tierului (nu există cale de scriere pe DSC). (42e, 106d)

## Perioadele

Închiderea și redeschiderea perioadei sunt comenzi, pe `api/perioade`.
Subiectul rutelor lunii este luna, nu un identificator: ea se rezolvă pe calea
securizată, deci ordinea refuzurilor rămâne 400 pentru an sau lună în afara
marginilor, 404 pentru lună nedefinită sau invizibilă, 403 pentru lipsa
dreptului cerut și 422 pentru refuzul lanțului. (F27-D1, F27-D2, 80a)

- `GET api/perioade` întoarce lanțul și cere dreptul de citire pe tipul
  perioadei. (F27-D1)
- `GET api/perioade/{an}/{luna}/verificare` întoarce constatările complete —
  cheie, fel, severitate, text, identificatorul și eticheta obiectului. Fiindcă
  verdictul ÎNSUMEAZĂ pe ușa nesecurizată documente, închideri de TVA,
  amortizări, împerecheri și politica severităților, cere dreptul de citire pe
  toate acestea: un raport filtrat n-ar ieși gol, ar ieși FALS. (F27-D2, 80a)
- `GET api/perioade/{an}/{luna}/istoric` întoarce rândurile de istoric ale
  lunii cu felul, momentul, utilizatorul, motivul și cheile acceptate. (F27-D2)
- `POST api/perioade/{an}/{luna}/inchide` primește cheile acceptate în corp —
  corpul e opțional, fiindcă a închide fără constatări e cazul normal — și
  răspunde cu acceptările scrise; un avertisment neacceptat sau orice blocantă
  iese 422 cu lista întreagă, un rând pe linie. (F27-D2)
- `POST api/perioade/{an}/{luna}/redeschide` primește motivul. Motivul absent
  este refuz de domeniu, nu de cerere. (F27-D1)
- `POST api/perioade/reconstruieste` recalculează integral soldurile
  perioadelor de referință și întoarce, per referință, câte rânduri existau,
  câte au ieșit din recalcul, câte diferă și suma absolută a diferențelor pe
  debit, credit, cantitate și valoare. Comanda nu are subiect, deci gate-ul ei
  este pe tip: dreptul de scriere pe perioada fiscală, același drept ca
  închiderea. Ordinea refuzurilor este 401, apoi 403, apoi 422. (F27-D3)
- Exercițiul are rutele lui sub `api/perioade/{an}/exercitiu`, cu subiectul
  decembrie anului: `GET verificare`, `POST inchide` (corp opțional, cheile
  acceptate) și `POST redeschide` (motivul). Ordinea refuzurilor e cea a
  lunii: 400 pe an, 404 pe decembrie nedefinit sau invizibil, 403 pe dreptul
  cerut, 422 pe constatări. Verificarea cere aceleași drepturi de citire ca
  verificarea lunii. `PerioadaDto` poartă `ExercitiuInchis` și
  `ExercitiuInchisLa`. (E3-D7, 80a)

## Închiderea conturilor de rezultat

`api/irz` are ușile ITV: lista filtrată, `GET {id}` cu verdictul anti-stale
pe draft, `GET previzualizare?an=&luna=`, `POST genereaza`,
`POST {id}/regenereaza`, `DELETE {id}` și comenzile documentului. Generarea
cere crearea pe tip, regenerarea și dreptul de scriere pe instanță; citirea
și previzualizarea cer citirea completă pe `Postare`. Generarea care nu
produce document întoarce 200 cu motivul. (E3-D3, F21-D3, F22-D5)

## Corecția

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

## Împerecheri

`POST api/imperecheri` primește opțional `Data` (ziua faptului de stingere;
absent = azi). Crearea trece prin gate-ul de drepturi și vizibilitatea ambelor
documente, apoi prin comanda atomică în ObjectSpace non-secured, la fel ca
desfacerea și ștergerea. (F27-D8, 101)

`POST api/imperecheri/{id}/desfa` cu `{ Data }` scrie rândul invers al unei
împerecheri dintr-o perioadă închisă: 400 pe corp malformat, 404 pe
inexistentă sau invizibilă, 403 fără drept de **scriere** pe instanță
(desfacerea scrie un rând, nu șterge), 422 pe domeniu (deja desfăcută, rând
invers, dată sub cea a împerecherii, perioadă închisă a rândului nou). (F27-D8, 80a)

`DELETE api/imperecheri/{id}` rămâne calea din fereastra deschisă; pe o
împerechere dintr-o perioadă închisă dă 422, cu trimitere la desfacere.
Ștergerea prin API și acțiunea XAF „Șterge împerecherea” folosesc aceeași
comandă atomică: eliberează suma nominalizată în cub și șterg legătura.
Gate-ul rămâne Delete pe instanța vizibilă, înaintea refuzurilor de domeniu.
CRUD-ul generic este refuzat de gardian; în perioadă închisă se folosește
„Desfă împerecherea”. (F27-D8, 101)

Panoul de stingeri poartă pe fiecare rând `Data`, `InverseazaId`, `Desfacuta`
și `PerioadaDeschisa` — verdictul „ce buton are dreptul să apară" e
server-computed, nu dedus în client. (F27-D8)

## Proiecțiile

### Stocul și restul documentelor

`GET api/proiectii/sold-stoc` primește opțional `laData`: fără el întoarce
soldul de azi, cu el soldul la sfârșitul zilei cerute. Parametrul este al
proiecției, ca perioada balanței, nu filtru de grilă; o valoare imposibilă
cade pe 400-ul unic al tierului. Un lot consumat integral, cu cantitate ȘI
valoare zero, nu apare în listă; unul cu cantitatea zero și valoare nenulă
rămâne, ca reziduul valoric să se vadă. (F27-D3)

`GET api/proiectii/documente-cu-rest` primește opțional `laData`: restul se
citește la ziua cerută, pornind de la partidele deschise ale ultimei perioade
de referință. Absent = la zi. Proiecția include și `ReturClient`. (F27-D7)

`documente-cu-rest` acceptă `documentCurentId` și `stinge` pentru candidați
compatibili; `Disponibil` este limita exactă a perechii, iar panourile o
consumă ca atare. (101)

### Partidele

`GET /api/proiectii/partide` și pagina `/partide` expun o partidă pe rând,
cu cont, partener/angajat, document opțional, data nașterii, sens și rest.
Filtrele sunt `laData`, `contrapartidaId`, `sensStingere`; cheia paginării este
unitate × cont × partener. Eticheta documentului nu elimină soldul când
lipsește sau nu este vizibilă. Postările sunt citite prin ObjectSpace secured.
(101)

### Proiecțiile contabile

Proiecțiile contabile (`api/proiectii/balanta`, `balanta-plan`, `fisa-cont`,
`sold-parteneri`) au o singură axă de repartitor, repartitorul de sold al
postării. `repartitorId` îl filtrează, de orice fel: partener, angajat,
gestiune, unitate internă sau cont propriu. Pe `fisa-cont`,
`repartitorNul=true` înseamnă postările fără niciun repartitor și nu se
combină cu `repartitorId`. Serverul nu are parametrul `gestiuneId`; locul
(`Postare.Loc`) și partenerul fiscal nu sunt filtre. Clientul nu trimite
`gestiuneId`, iar ecranele numesc axa „Repartitor”. (S1, C111-Q6)

Pe `fisa-cont`, `faraTert=true` cere postările care nu stau pe un partener
sau pe un angajat: fără repartitor, pe repartitor intern ori pe cont propriu.
E mulțimea rândului „fără terț” din `sold-parteneri` și nu se combină cu
`repartitorNul` sau cu `repartitorId` (400). (C111-Q18)

`GET api/proiectii/fisa-cont/sold`, cu parametrii fișei, fără cei de grilă,
întoarce sinteza perioadei: `SoldInitialDebit`/`Credit`, `RulajDebit`/`Credit`
și `SoldFinalDebit`/`Credit`, soldurile netate pe o singură parte, calculate
din atomii balanței pe filtrele fișei. Refuzurile sunt ale fișei: 400 pe
forma cererii, 404 pe contul inexistent sau invizibil; cine nu vede nicio
postare primește sinteza goală. Clientul o arată într-o bandă deasupra
grilei, deci fișa fără mișcări în perioadă își arată soldul. (C111-Q19)

`registru-jurnal` și `fisa-cont` poartă pe rând contul corespondent
(`ContrapartidaId`, `ContrapartidaSimbol`): contul celeilalte postări a
perechii; rândul fără pereche nu are. Ecranele îl arată în coloana „Cont
corespondent”. (S9, 111 (d))

`GET api/proiectii/sold-parteneri?laData=&contId=&repartitorId=` întoarce
soldurile pe cont la o dată, cu aceleași filtre de dimensiune ca balanța și
cu rândurile de sold net zero omise. Rând propriu au numai repartitorii
externi (partener, angajat); restul contului — sume fără repartitor, pe
repartitor intern sau pe cont propriu — vine într-un singur rând „fără terț”
pe cont, cu `RepartitorId` nul, deci suma rândurilor unui cont e soldul lui
din balanță (C111-Q13). `laData` absent = azi; o dată nevalidă cade pe 400.
Citirea cere dreptul pe registrul contabil, ca balanța. Ecranul
`/sold-parteneri` din client („Solduri pe terți”) ține data în URL și duce
prin dublu-click în fișa contului pe anul datei cerute, filtrată pe rând:
pe repartitorul lui, sau cu `faraTert` pe rândul „fără terț”. (F27-D7,
C111-Q18)

### Fișierele declarațiilor

`GET api/proiectii/d300/xml` și `GET api/proiectii/d394/xml` primesc
parametrii proiecției JSON plus datele de perioadă: D300 `temei` (0 sau 2),
`proRata` (0–100), `depusReprezentant`, `bifaInterne`, `solicitRambursare` și
bifele de taxare inversă internă; D394 `persoaneAfiliate` și `schimbOptiune`.
Parametrii greșiți dau 400; lipsa citirii pe `Postare` sau `Societate` (și,
pe D394, pe `Document` și `PlajaFacturi`) dă 403; refuzurile din
`RefuzuriXml` dau 422 cu `COD: mesaj`, înaintea primului byte. Altfel
răspunsul e `application/xml; charset=utf-8`, cu numele fișierului în
`Content-Disposition`, pe lună amprenta faptelor în `X-Versiune-Exportata`,
iar SHA-256 al fișierului și parametrii lui în `X-Sha256-Fisier` și
`X-Parametri-Fisier`. Confirmarea depunerii
(`POST /api/depuneri-declaratii/{formular}/{an}/{luna}`) le primește înapoi în
`Sha256Fisier` și `ParametriFisier`, numai împreună; parametrii fără forma
exportului dau 400, iar fișierul refăcut diferit dă 422
`DEPUNERE_FISIER_DIFERIT`. `POST …/{luna}/retragere` (`VersiuneExportata`,
`Motiv`) retrage confirmarea activă; versiunea fără confirmare activă dă 422
`DEPUNERE_NECONFIRMATA`, iar D300 cu regularizări sprijinite pe ea dă 422
`DEPUNERE_REGULARIZARI`. (E2-D8, 122 i)

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

### Citirea completă și citirea filtrată a cubului

Rutele care întorc cifre însumate din cub pe ușa de sistem cer citirea
COMPLETĂ pe `Postare`: `itv/{id}`, `itv/previzualizare`, `irz/{id}`,
`irz/previzualizare`, `amo/{id}`, `amo/previzualizare`,
`imobilizari/{id}/fisa`, `imobilizari/registru`,
`perioade/{an}/{luna}/verificare` și `perioade/{an}/exercitiu/verificare`. Un rol fără dreptul pe tip, ori cu un
criteriu de rând sau de membru pe `Postare`, primește 403 înaintea citirii.
Dreptul completează, nu înlocuiește: ruta cere mai întâi dreptul pe subiectul
ei, iar verificarea închiderii pe toate tipurile pe care le însumează.
`Postare` și `Tranzactie` n-au rută OData sau REST și nu intră în contractul
clientului; `$metadata` le descrie ca `EntityType` fără `EntitySet`, ca pe
orice tip neexpus. (F22-D5, 80e, 108 d, D9-D9)

Proiecțiile contabile de pe ușa securizată nu cer citirea completă:
`balanta`, `balanta-plan`, `fisa-cont`, `fisa-cont/sold`, `registru-jurnal`,
`sold-parteneri`, partidele, documentele cu rest și soldul de stoc filtrează
pe rând. Un rol cu criteriu de rând pe `Postare` primește 200 cu cifrele
rândurilor pe care le vede; cine n-are dreptul pe tip și nu vede nicio
postare primește rezultatul gol. Contul corespondent al unei postări ascunse
rămâne nul. (110 (e), C111-Q22)

### Drepturile și referințele

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

### Forma erorilor și sesiunea

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
| Politici cu scriere controlată | MapareD300, MapareD394, PoliticaMiscareSaft, TipDocument, RegulaStoc, RegulaContare, PoliticaConex, PoliticaScadenta, PoliticaValidare, PoliticaTva, PoliticaInchidereTva, PoliticaNumerotare, PoliticaTvaImplicit, PoliticaAmortizare, RegulaDeductibilitate, PoliticaInchidere, PoliticaInchidereConturi (81e, 87i, F27-D2, E3-D4) |
| Nomenclatoare pentru citire | Judet, UnitateMasura, CodEconomic, SursaFinantare, CodFunctional, Proiect, ContPropriu, UnitateInterna, Lot, Cont, Angajament, Repartitor, RandD300, ClasaProdus, ClasificareImobilizari (56, 81e, 87i) |
| Audit pentru citire | AuditDataItemPersistent, AuditEFCoreWeakReference (81e, 81h) |

`TipDocument` permite modificarea implicitului expus; nu permite crearea,
ștergerea sau schimbarea ancorei de cod. Tabelele de bază pentru raportare
nu devin editabile doar pentru că sunt utilizate de o politică editabilă. (81e)

## Recepția cu diferențe (NIR)

API-ul NIR citește proveniența recepției și marcajul liniilor acoperite;
clientul culege cauza și imputatul, cu cantitate zero permisă pe linia
acoperită. Legăturile cu sursa sunt stabilite de server, nu intră în
WriteDto. Liniile-sursă nu se șterg și tipul/lotul lor nu se schimbă.
Cauza implicită la operare este InClarificare pentru minus și Plus pentru
plus; formularul explică aceste implicite. Cantitatea/valoarea constatată
sunt distincte de delta economică postată de declarant. (098, 099)

Politica diferențelor se editează prin OData și ecranul
`/politici/diferente`, sub aceleași drepturi de Configurator ca celelalte
politici. Contul pentru personal este opțional și separat de contul normal. (098, 099)

La schimbarea cauzei NIR din Imputabila, editorul golește imediat imputatul.
API-ul ignoră imputatul din payload pentru celelalte cauze și îl golește
și la delta zero; un imputat ascuns nu rămâne atașat constatării. (098, 099)

## Audit

Auditul EF Core este activ în ambele hosturi. Istoricul identifică obiectul
prin numele CLR complet și identificatorul GUID și include autorul.
Entitățile auditului sunt expuse numai pentru citire. Rolul implicit citește
doar auditul propriu; celelalte drepturi se aplică prin securitatea XAF. (53e, 55e, 81h)

Istoricul de audit și `DinSeed` au scopuri distincte. Curățarea scenariilor
de test nu șterge auditul pentru a ascunde operațiile executate. (81d, 81h)

## Contracte generate

OpenAPI, tipurile TypeScript și metadata de model sunt generate și păstrate
în repository. Clientul nu folosește un client API generic generat pentru
toate operațiile. DTO-urile și atributele serverului rămân sursa contractului. (43d, 56)

Verificarea de drift trebuie să confirme că fișierele generate corespund
sursei. Tipurile TypeScript nu înlocuiesc verificarea serverului, iar
atributele de validare XAF și cele ale contractului HTTP au consumatori
diferiți. (43b, 43d, 77k)

<!-- gen:decizii -->
## Decizii

Deciziile care numesc pagina în `Arii:` (24; titlurile în [index](../decizii/README.md); generat de `docskit gen`, nu se editează): 5, 6, 7, 39, 42 (amendată de 104, 106), 43 (amendată de 104), 53, 55, 56 (amendată de 77, 81), 57, 60, 70, 77, 80, 81, 87 (amendată de 97), 89, 98, 99, 101 (amendată de 102), 104 (amendată de 106), 106, 108 (amendată de 110), 122 (amendată de 123).
<!-- /gen:decizii -->
