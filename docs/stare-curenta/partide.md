# Partide și stingere

**Actualizat: 2026-10-15.** [Index](README.md)

Partidele deschise, stingerea, împerecherea, nominalizarea și decontul.

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
este partea de sold a balanței analitice, citită prin aceiași atomi cumulați.
Rând propriu au numai repartitorii externi (partener, angajat). Restul
contului — sumele fără repartitor, pe repartitor intern sau pe cont propriu —
intră într-un singur rând „fără terț”, cu `RepartitorId` nul, ca raportul să
se închidă cu balanța contului (C111-Q13). `repartitorId` filtrează atomul pe
repartitorul lui de sold, înaintea grupării. Rândurile cu sold net zero se
omit. (F27-D7)

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

## Decontul pe cub și urmărirea partidelor (095, 096, 100)

DEC declară cheltuiala și contrapartida titularului, cu TVA din politica
profilului și fără stoc. Conturile/repartitorii expliciți au prioritate;
contrapartida TVA rămâne cea a politicii. Normal, capitalizat și taxare
inversă sunt probate independent; bugetarul păstrează costul brut fără
fapte fiscale când nu are PoliticaTva.

Fiecare capăt al liniei poartă repartitorul liniei: cel explicit, apoi cel
al regulii, apoi titularul. Partida se deschide numai pentru repartitor
extern pe un cont care urmărește partide. Pe capătul cu fapt fiscal
repartitorul liniei rămâne repartitor de sold, iar partenerul fiscal și locul
stau separat, în `PartenerFiscal` și `Loc` (S10, C111-Q7). Decontul cu fapt
fiscal și cu terț pe linie, altul decât titularul, pe un cont cu partide e
acceptat: partida e a terțului liniei (C111-Q11). Baza cu repartitor intern
pe linie are ca `Loc` locul documentului, nu repartitorul ei (S12). Capătul
de taxă reconstruit pe titular își păstrează partenerul fiscal și locul.

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

## Împerecherea ulterioară operării = tranzacție `Transfer`

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

## Partide: raport, snapshot și împerechere (101, 2026-09-25)

`Cub.Citiri.Partide` este intrarea comună pentru rest, disponibil și raport.
Cheia este unitate × cont × partener, cu identitatea 092(a). Originea se
citește din `OriginiPartide`, scrisă atomic din postările finale eligibile
de materializare (TR-D9b, pasul 4, 2026-10-12). Rândul poartă contul,
titularul și documentul; la deschidere, documentul este nul și sunt păstrate
referința și ordinalul inițial. Nu poartă sold sau dată. Retragerea elimină
originea fără suport propriu, inclusiv când rămân postări străine net zero;
stornoul o păstrează. Reoperarea reface aceeași identitate.

Pe ușa securizată, originea cere suport propriu vizibil prin `EXISTS` pe
chei; soldul rămâne cel al postărilor vizibile. Pe ușa de sistem originea
se citește direct, fără `Postare`. Ușa se recunoaște după starea securității
pe context (`Vizibilitate.Securizat`), nu după tipul ObjectSpace-ului:
ObjectSpace-ul nesecurizat al hosturilor are tipul celui securizat. Proiecția nu are endpoint
OData ori navigație; gardianul refuză CRUD inclusiv administratorului
securizat. `ORIGINE_PARTIDA_INCONSISTENTA` este refuz de materializare,
nu de declarant. [Catalogul D9B](../nucleu/scenarii/D9B.md) fixează probele.
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

<!-- gen:decizii -->
## Decizii

Deciziile care numesc pagina în `Arii:` (15; titlurile în [index](../decizii/README.md); generat de `docskit gen`, nu se editează): 1, 31 (amendată de 76, 88), 41, 46 (amendată de 113), 49, 57, 64 (amendată de 84), 76, 82, 86, 95, 96 (amendată de 100, 102), 100 (amendată de 102), 101 (amendată de 102), 102.
<!-- /gen:decizii -->
