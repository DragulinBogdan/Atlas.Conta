# TR-D9b-CV — corecții valorice pe lot, fără schimbarea cantității

- Tip: contract
- Data: 2026-10-12
- Trigger: owner, continuarea după originea partidei; inventar și așteptări înainte de implementare
- Decizii: 088 (h, amendată de 103), 090, 091, 103, 104, 110, 111, 118, 119
- Stare: deschis

Contractul fixează ajustarea valorii lotului și compensările necesare fără
rescrierea ieșirilor operate. Pașii 1 și 2 sunt documentari; pasul 3 este
implementarea.
[Inventarul CodeGraph](tr-d9b-corectii-valorice-inventar.md) descrie codul;
[catalogul CVL](scenarii/CVL.md) fixează cifrele. [Producătorul și matricea
compensării](tr-d9b-corectii-valorice-producator-si-compensare.md) detaliază
propunerea revizuită: FCT cu linie explicită de corecție. CVL denumește
mecanismul și catalogul, nu un tip nou de document. Calea executabilă există
din pasul 3; rezultatele scenariilor sunt în secțiunea de verificare a pasului.
Review-ul Claude a confirmat aritmetica și alegerea FCT, dar a respins
forma inițială a compensării. [Revizia R1](tr-d9b-corectii-valorice-review-r1.md)
introduce Operare separată și gard temporal; **punctul 2 nu este acceptat**.
Owner-ul a fixat la 2026-10-13 apartenența compensării: documentul și linia
corecției, nu beneficiarul stornat (R6 din același raport). Regula este
[decizia 119](../decizii/119-compensarea-valorica-apartine-corectiei.md).
Delimitarea pasului 2 este închisă la 2026-10-14, la cererea owner-ului:
forma compensării, predicatele și explicația sunt verificate contra codului
în R8 din raport, iar predicatele sunt fixate în CV-D10. Afirmația de mai
sus despre punctul 2 este istorică.

Regulile economice din 090 (j/k) și 091 (h) sunt ținta deja decisă.
Extinderea FCT și limitele primului pachet de mai jos sunt
**propunerea acestui contract**, de confirmat la review înainte de cod;
nu sunt prezentate ca aprobări anterioare ale owner-ului.

Owner-ul a aprobat perimetrul primului pachet la 2026-10-14, în patru
puncte: corecția taxabilă se refuză; factura mixtă se refuză; lotul
corectabil este numai cel din lanțul FCT → NIR fără diferențe → BCS;
corecția nu se antedatează, iar retragerea sau stornarea ei se refuză cât
există o ieșire activă făcută după ea. Textul lor este în CV-D1, CV-D5 și
CV-D10.

## Scop

Primul pachet: lot recepționat prin FCT → NIR conex, fără diferențe de
recepție, apoi zero, unul sau mai multe BCS, corecție semnată de valoare și
ieșiri ulterioare evaluate din sold. Sunt obligatorii retragerea,
reoperarea, storno, compensarea consumului și granița de lună închisă.
Corecția privește valoarea întregii cantități recepționate, inclusiv partea
consumată, nu o sumă încărcată arbitrar numai pe stocul rămas.

Mecanismul și catalogul de bază sunt comune ambelor profiluri; conturile
diferă prin seed. Cifrele noi sunt RON, fără TVA. Este o specificație de
comportament al produsului, nu o certificare fiscală a documentului.

Verificare contra invarianților: I — fiecare delta are document și
contrapartidă; II — frunza culege, motorul primește operand închis;
III — numai postări noi, inversă și retragere în condițiile admise;
IV — mecanism în cod, conturi în politică; V — constante independente de
surse externe; VI — lotul își păstrează identitatea, ieșirea istorică
rămâne fapt, ieșirea nouă se evaluează din sold prin metoda existentă.

## Ce NU intră

Costul vamal/DVI (pachetul următor), valuta și unitatea de evaluare (două
pachete distincte), ajustările pentru depreciere, imobilizările, TVA-ul
corecției de factură, transportul, schimbarea cantității, importul și
reevaluarea generală a istoricului. NIR cu diferențe nu devine corecție
valorică prin acest contract. `CorectieService` (storno + document
înlocuitor) rămâne distinct de documentul care adaugă o delta valorică.
Exportul SAF-T al corecției rămâne restanță cu refuz explicit (CV-D9).

Lanțurile cu BTR, retururi, ASM, LDI, recepții multiple/deschidere sau
reziduuri valorice preexistente cer matrice proprie înaintea extinderii.
În primul pachet, corecția unui asemenea lot este refuzată explicit;
operațiile deja existente pe aceste tipuri nu se restrâng global.
Anul contabil închis și politica de destinație 1174 din 090 (k) rămân
extensie separată; corecția cu beneficiar într-un an închis este refuzată
în acest pachet, nu trimisă tacit pe cheltuiala curentă.

## D-uri pin-uite

### CV-D1 — ușa documentului și semantica sumei

Propunere revizuită: **FCT nouă**, cu fel explicit al liniei de corecție
valorică. Linia corectată este linia de intrare a lotului
(`Lot.LinieIntrareId`); linia de corecție nu poartă o a doua referință în
primul pachet (R8-3). `LotId` este lotul existent,
`Valoare` este delta semnată în RON, `Cantitate = 0`. O linie pe lot per
document; mai multe loturi se operează atomic. Predătorul este furnizorul
căruia i se modifică datoria; primitorul este locul intern. Lotul are
recepție activă, aceeași gestiune și același furnizor cu sursa declarată.
Politica rezolvă contrapartida, fără conturi hardcodate în motor.

În continuare „CVL” înseamnă această FCT corectivă. Nu se propune un tip
nefiscal separat. Nu se folosește `CorecteazaId` pentru legătura comercială:
FCT inițială rămâne operată, iar delta nu o înlocuiește. Corecția pur valorică
nu naște lot, nu schimbă prețul intrării și nu generează NIR. Felul liniei
nu se deduce din cantitate zero; linia obișnuită păstrează gardul existent.
Felul explicit separă și facturarea lotului recepționat pe aviz, pentru
care 090 (h) cere408=401. Nu se deduce corecția doar din existența lotului.

CVL deschide propria partidă de corecție pe contul de furnizor, pe aceeași
regulă de identitate ca alte documente. Partida FCT nu se rescrie.
Pentru +200: rest FCT −1.000, rest CVL −200; pentru −200: −1.000/+200.
Soldul furnizorului este 1.200, respectiv 800 datorie. Plata/compensarea
acestei partide folosește mecanismul existent, cu gardurile lui.

Cifrele pilotului sunt fără TVA. FCT își păstrează semantica fiscală;
corecția taxabilă se refuză explicit până la matricea fiscală, niciodată
prin omiterea taxei: linia de corecție cu tip de TVA primește
`CV_STRUCTURA`, iar fără tip de TVA nu produce fapte fiscale. Factura mixtă
se refuză la fel: un document cu o linie de corecție are numai linii de
corecție. Liniile de corecție nu intră în documentul conex, deci factura
corectivă nu generează NIR. Valoarea liniei se culege; nu se calculează din
preț × cantitate.
Culegerea XAF, metadata, politica și adaptorul de comandă intră în extensia
FCT; pagina React de detaliu nu se extinde (104d). Proba numerică intră prin
`OperareApi`, nu prin inserări de postări sau printr-un NTC folosit ca
înlocuitor al producătorului lipsă.

Fără `CorecteazaId`, ramura curentă SAF-T ar alege380 pentru +200 și381
pentru −200, nu384. Totuși exportul de factură refuză deja q0 și
contrapartidele pe două conturi; exportul de stoc refuză q0/v≠0.
[Inventarul R1](tr-d9b-corectii-valorice-review-r1.md) enumeră cititorii și
consecințele. Owner-ul a hotărât la 2026-10-13 că reprezentarea acestor
cazuri nu blochează pachetul: rămâne restanță cu refuz explicit (CV-D9).
Cifrele contabile nu certifică exportul. Evenimentul de
factură trebuie să separe perechile comerciale de compensările interne ale
corecției (perechi fără picior de terț): compensarea singură este sărită de
cititorul actual, dar inversa ei intră în grupul liniei la storno CVL.
Separarea se definește și se probează odată cu închiderea restanței CV-r1.

### CV-D2 — distribuția și imutabilitatea

La 100 bucăți/1.000 lei și delta +200:

| Starea la corecție | Corecție stoc | Corecție consum | Sold stoc după | Consum total după |
|---|---:|---:|---:|---:|
| intact, 100 bucăți | +200 | 0 | 100/1.200 | 0 |
| consumate 60, rămase 40 | +80 | +120 | 40/480 | 720 |
| consumate 100 | 0 | +200 | 0/0 | 1.200 |

Cantitatea tuturor postărilor de corecție este zero. BCS-ul de 60 rămâne
600 pe linie și în postările sale; cei 120 sunt postări ale CVL, atribuite
consumului. Recepția rămâne 100/1.000, `Lot.PretUnitar` rămâne 10.
Citirea înaintea datei corecției păstrează toate valorile inițiale.

Pentru cazul parțial, matricea propusă a corecției +200 este exact:

| Pereche | Debit | Credit | Valoare | Atribuirea piciorului intern |
|---|---|---|---:|---|
| 1 | stoc × MAG1 × lot | furnizor × partida CVL | 80 | postarea recepției lotului |
| 2 | consum × LOC × același lot | furnizor × aceeași partidă CVL | 120 | postarea de consum a BCS |

Pentru diminuare se păstrează sensurile debit/credit, cu valori negative
în tranzacția `Operare`: −80/−120 în exemplul parțial. S-D14 din
[TR-D7a](tr-d7a-strangler-contract.md) amendează 090 (f); prima redactare
a contractului CV omitea acest amendament și este corectată aici.
Analiza fiecărui beneficiar se păstrează din faptul atribuit;
pe contrapartidă o rezolvă politica documentului. Contul beneficiarului
este cel al faptului istoric, nu cel din politica BCS de azi.

Lotul epuizat nu primește valoare în MAG1: întreaga corecție merge pe
consumul identificat. Nu este permis un reziduu pe stoc gol sau un cont
de diferență ales numai pentru a echilibra postările.

### CV-D3 — ponderi, rotunjire și corecții succesive

Beneficiarii sunt stocul rămas și cantitățile consumurilor active, fără
dublarea recepției și a capetelor ieșirii. Suma ponderilor este cantitatea
recepționată; storno consum neutralizat nu rămâne pondere de consum.
Mai întâi Hamilton pe cheia completă a beneficiarului (cont, repartitor,
lot/produs, carte, analiză), apoi pe postările atribuite din acea cheie.
Nu se repartizează după valoarea consumului și nici după prețul inițial.

Ordine propusă pentru egalități: data faptului → `ScrisLa` → ID tranzacție
→ ordinal postare → ID postare;
grupurile se ordonează după primul fapt membru. Nu se folosește ordinea
nespecificată a unui query. Ordinea și cotele se persistă în explicație.
În fixture datele beneficiarilor sunt distincte, exceptând proba dedicată
departajării. Stocul rămas folosește faptul de recepție drept ancoră.

O a doua corecție este delta suplimentară; +200 apoi +100 în cazul 40/60
adaugă 80/120 și 40/60. Fiecare are propria proveniență. Ultima ieșire
ia exact soldul valoric rămas, inclusiv cenții. Diminuarea este permisă
până la valoare zero, fără beneficiar cu valoare negativă; delta zero,
precizie invalidă sau depășirea valorii corectabile sunt refuzate.

### CV-D4 — proveniența și compensarea

La alocarea inițială, `Cauza` identifică documentul/linia corecției;
`Atribuit` identifică postarea istorică beneficiară, nu ID-ul lotului sau al facturii.
`InversaDin` păstrează rolul distinct de inversă exactă. Datele persistate
trebuie să reconstruiască, pentru fiecare alocare, sursa, suma inițială,
inversările și compensările deja făcute, deci suma încă activă.
Nu ajunge un flag pe document și nu se deduce activitatea numai din
`Document.Stare`. Propunerea din [matricea pe postări](tr-d9b-corectii-valorice-producator-si-compensare.md)
folosește `Atribuit` pe ambele picioare ale compensării spre rădăcina
alocării, iar `InversaDin` numai pentru inversa exactă.

**Compensarea aparține documentului și liniei care au produs alocarea
compensată** (owner, 2026-10-13). Se scrie la data stornării beneficiarului,
într-o tranzacție `Operare` suplimentară a corecției, cu explicație proprie
și legătură verificabilă către beneficiarul inversat. Stornarea beneficiarului
declanșează mutarea; efectul mutat rămâne al corecției. Inversa compensării
are aceeași cauză, deci stornoul CVL nu conține cauze străine, iar BCS și
inversa lui se închid pe fiecare cont. Nu se adaugă un câmp de compensare în
postare și nu se suprascrie sensul utilizărilor existente ale lui `Atribuit`
la partide. Cele două salturi definesc familia, nu o selecție recursivă nouă.
La storno corecție, compensarea este între postările proprii; la storno
beneficiar se înlocuiește selecția cauzat ∪ atribuit pentru loturi.
Predicatele exacte și izolarea căii partidelor sunt în R1; se refuză
alocarea cu țintă altă alocare.

Storno BCS după corecția +200 din cazul parțial are două componente:
inversa exactă 60/600 a BCS și compensarea valorică +120 pe stoc/−120 pe
consum. Comanda lasă 100/1.200 pe lot, consum zero, datorie 1.200.
Nu inversează datoria CVL. Comanda scrie atomic inversa beneficiarului și
toate compensările: o tranzacție Storno a BCS și câte o `Operare` pentru
fiecare document de corecție cu compensare nenulă (1 + N). Rădăcinile
aceluiași document au perechi distincte în aceeași tranzacție. Compensarea
este Dconsum−120/Cstoc−120, q0, cu cauza CVL și linia ei. Fiecare tranzacție
este balansată. Operarea compensării are explicație proprie a faptului
valoric; nu copiază decizia de ieșire600 și nu reoperează BCS, care ajunge
Stornat. Eșecul după scrierea unei părți dintre compensări anulează tot.

A doua `Operare` pe un document operat contrazicea litera lui 090 (a)
(„EXACT o tranzacție de fel `Operare`”); amendamentul 110 privește altceva.
Decizia 119 amendează explicit 090 (a) și 090 (i).
Lipsa unei constrângeri în schemă sau în cod nu ține locul deciziei.
Apartenența la corecție restrânge găzduirea și explicarea noii `Operare`
la producătorii de corecții; fiecare flux beneficiar viitor (FCL, BTR, ASM)
cere în continuare scenarii proprii pentru identificarea beneficiarului și
destinația compensării.

Dacă CVL a fost stornat înainte, compensarea BCS este zero. Două CVL
active (+200/+100) cer compensare 180; una neutralizată nu participă.
Storno CVL după compensarea BCS inversează atât efectele proprii, cât și
relocarea lor încă activă: net −200 numai pe stoc, fără consum negativ.
Nu se inversează aceeași alocare de două ori (SC-CVL-12).

### CV-D5 — retragere, dependenți și cronologie

Retragerea CVL în perioadă deschisă este permisă numai fără dependenți
valorici, compensări sau stingeri active. Stocul și consumul revin la
valorile dinaintea corecției, iar postările, tranzacția și originea partidei
proprii dispar atomic; lotul și FCT rămân. Reoperarea nu dublează efectul.
Consumul **anterior** corecției este beneficiar, nu dependență care ar
interzice automat retragerea CVL. Refuzul pentru compensări active rămâne
în primul pachet, deși compensarea este faptă proprie a CVL și ar fi ștearsă
odată cu ea: apartenența nu amendează regula. O relaxare cere scenariu
separat pentru dependenți, referințe istorice și perioadele tuturor faptelor
șterse.

Retragerea BCS care are alocări CVL active este refuzată; se folosește
storno cu compensare. Retragerea sursei recepției cu CVL activ este
refuzată chiar dacă nu există ieșire cantitativă. Nu rămâne `Atribuit`
spre o postare ștearsă; inclusiv referințele din istoria neutralizată
trebuie rezolvate explicit înaintea unei retrageri. Postările unei corecții
rămân în cub și după stornarea ei, deci singura rezolvare este refuzul:
beneficiarii unei corecții, chiar stornate, nu mai pot fi retrași, numai
stornați. Recepția e deja oprită de gardul loturilor folosite de alte
documente; BCS primește `CV_DEPENDENTI`.

Stornarea sursei recepției urmează calea oricărui beneficiar: selecția
cauzat ∪ atribuit nu ia alocările de pe lot și inversele lor. Cu valoare de
corecție activă pe stoc, inclusiv cea relocată printr-o compensare, comanda
se refuză cu `CV_DEPENDENTI`: stocul golit nu are unde s-o primi. Cu toate
corecțiile neutralizate, stornoul conține exact inversa recepției, fără
compensare, iar lotul rămâne 0/0. Gardul temporal al familiei se aplică la
fel. SC-CVL-30 fixează cazurile.

Limită propusă pentru primul pachet: retragerea sau stornarea CVL după o
nouă ieșire activă evaluată la costul ajustat este refuzată până la
neutralizarea acelei ieșiri. Nu se scad cei 200 dintr-un stoc care mai
conține doar o parte și nu se recalculează ieșirea nouă. Storno ieșire
ulterioară inversează valoarea ei exactă; nu primește din nou ajustarea
deja inclusă la operare. SC-CVL-13 fixează recuperarea permisă.

Data CVL este cel puțin data ultimei mișcări relevante deja operate a
lotului. Nu se introduce acum corecție antedatată față de o ieșire deja
operată; aceeași zi este permisă, iar serializarea fixează ordinea.
O nouă mișcare antedatată care ar invalida distribuția unei CVL active se
refuză pe lotul respectiv. Acest gard privește loturile corectate, nu
schimbă global regula documentelor retroactive.

Pentru storno CVL **și storno beneficiar**, familia se descoperă fără
filtru de dată, inclusiv compensările și inversele neutralizate. Data
comenzii trebuie să fie cel puțin data maximă a faptelor relevante;
altfel `CV_CRONOLOGIE`, fără efecte. Suma la dată se face numai după gard.
Storno CVL25.01 urmat de storno BCS datat20.01 este refuzat, nu compensat
dintr-o familie tăiată artificial la20.01. SC-CVL-27 fixează și oglinda.

### CV-D6 — perioadă închisă

Operare/retragere/storno în luna închisă: refuz, fără efecte. Corecția
valorică nouă se postează în luna deschisă; poate atribui consumul dintr-o
lună închisă a aceluiași an, fără să-i schimbe postările sau snapshot-ul.
Storno în februarie al CVL din ianuarie este permis în lipsa dependenților:
inversa are data din februarie; situația la 31 ianuarie nu se schimbă.

`Corecteaza` asupra CVL păstrează mecanismul comun: storno în perioada
deschisă + draft nou legat. Noul CVL se operează separat, cu noua delta;
corectarea +200 la +150 lasă efect net +150, nu +350 (SC-CVL-17).
Fără TVA în aceste probe: nu se inventează regularizări fiscale.

### CV-D7 — refuzuri și verificare atomică

Codurile stau în `CoduriRefuz`:
`CV_STRUCTURA`, `CV_LOT_NEELIGIBIL`, `CV_VALOARE_INSUFICIENTA`,
`CV_CRONOLOGIE`, `CV_DEPENDENTI`, `CV_AN_INCHIS`, `CV_PROVENIENTA`.
Mecanismul pur din nucleu nu vede `CoduriRefuz`: emite motivul tipizat
`MotivCorectieValorica`, iar `CoduriRefuz.Refuz` îl traduce la granița
declarantului și a materializării (owner, 2026-10-14).
Se refolosesc codurile existente pentru lot lipsă, delta zero și politică
lipsă când semantica coincide. Refuzurile declarantului se asertă prin
`Materializare.Refuzuri`; gardurile comenzii au propria probă de ușă,
cu familia mesajului dacă refuză înaintea declarantului.

Dry-run: zero efecte persistate, inclusiv fără modificarea liniilor.
Eșec după pregătirea compensării: rollback al întregii comenzi. Serializare
pe aceeași stare ca ieșirea și închiderea; două conexiuni reale verifică
corecție/consum, corecție/storno, corecție/închidere și storno beneficiar
concurent cu storno corecție (SC-CVL-29). Cititorii de produs,
explicația și snapshot-ul se compară cu constantele din catalog.

### CV-D8 — explicația alocării și a compensării

Fiecare `Operare` a corecției poartă explicație proprie; cititorul de audit
o cere deja pe orice `Operare` cu document. Ierarhia deciziilor este închisă
în nucleu (N-D11), deci cele două feluri noi se adaugă acolo. Numele sunt de
lucru.

| Decizie | Câmpuri | Postarea explicată |
|---|---|---|
| `AlocareValorica` | linia corecției; unitatea beneficiarului (lot × cont); referința postării beneficiare; ponderea; valoarea semnată alocată | rădăcina de pe lot cu aceeași linie, unitate și valoare, cu `Atribuit` = beneficiarul; fără postare când valoarea este zero |
| `CompensareValorica` | linia corecției; referința rădăcinii; referința beneficiarului; unitatea sursă și unitatea destinație; valoarea semnată mutată | perechea de compensare, ambele picioare cu `Atribuit` = rădăcina |

Ponderea este stocul rămas pentru alocarea pe stoc și cantitatea postării
de consum pentru alocarea pe consum. Fiecare beneficiar cu pondere pozitivă
are decizie, inclusiv cel căruia repartizarea îi dă zero (R8-4). Deciziile
de alocare stau în ordinea folosită la repartizare (CV-D3); explicația este
sursa ordinii. Inversa beneficiarului nu se copiază în decizia de
compensare: este unică și se recitește prin `InversaDin` (R8-5).

Ipotezele existente ajung. Alocarea reține soldul citit al stocului la data
corecției, perioada deschisă și versiunile de politică; contul contrapartidei
și partida proprie rămân `ContRezolvat` și `PartidaDeschisa`. Compensarea
reține numai perioada deschisă: faptele familiei nu se copiază în explicație,
se recitesc din cub. `Declarant` numește clasa declarantului documentului
care deține tranzacția, cum cere verificarea existentă; mecanismul se citește
din felul deciziilor. Nu se scrie `ValoareIesire` sau `ValoareDeclarata` cu
cantitate zero și nu se schimbă linia BCS.

Invariantul de audit se extinde, fără relaxarea regulilor de azi:

1. Fiecare postare pe lot cu cantitate zero și valoare nenulă dintr-o
   `Operare` este explicată de exact o decizie cu valoare nenulă: alocarea
   explică rădăcina, compensarea explică ambele picioare ale perechii.
   Fiecare decizie cu valoare nenulă are postarea ei. Decizia de alocare cu
   valoare zero nu are postare.
2. Pe linia corecției, suma valorilor alocate este valoarea liniei, iar suma
   ponderilor, toate pozitive, este cantitatea recepționată a lotului.
3. Cotele rezultă din repartizarea ierarhică a valorii liniei pe ponderile
   și ordinea persistate. Grupul fiecărei decizii este poziția postării
   beneficiare referite, recitită din cub (R8-6).
4. La compensare: nicio inversă a rădăcinii nu o precedă; valoarea este suma
   activă a rădăcinii pe poziția sursă; inversa beneficiarului are aceeași
   dată și indică exact beneficiarul.
5. Oglinda inverselor pe lot rămâne neschimbată.

Implementarea (2026-10-14): regula stă în `CorectieValorica.Auditeaza`,
pură, peste faptele loturilor și deciziile explicate; `INV-CUB` o cheamă
din `Explicatii.VerificaAcoperire` și refuză cu `CITIRE_EXPLICATIE_VALORICA`.
Domeniul punctului 1 sunt postările pe lot fără cantitate și cu `Atribuit`,
familia corecției: diferența valorică a NIR-ului (099) scrie pe lot o
postare fără cantitate și fără atribuire, explicată de mecanismul ei, și
rămâne în afara acestui invariant (prima integrală a picat pe SC-NIR, privat).
Pe lângă punctele de mai sus, invariantul refuză postarea valorică fără
valoare, compensarea cu valoare zero, aceeași rădăcină compensată de două
ori și o postare cu mai multe inverse. Atribuirea spre o postare absentă
de pe loturile corectate este proveniență străină.

Forma persistată capătă cele două feluri. Versiunea explicației rămâne 2:
formele existente nu se schimbă, iar un fel necunoscut este deja eroare la
citire. Bazele se recreează, nu se migrează (102b). Linia explicată și
DTO-ul ei capătă listele de alocări și compensări; driftul openapi se
verifică (56d). La storno CVL, cititorul existent compune explicația din
ambele `Operare` ale corecției. SC-CVL-31 fixează proba.

### CV-D9 — exportul SAF-T: restanță cu refuz explicit

Hotărârea owner-ului din 2026-10-13. Pachetul nu definește reprezentarea
SAF-T a corecției. Refuzurile existente rămân, nu se relaxează și nu se
adaugă un cod nou:

| Fișier | Situație | Refuz existent |
|---|---|---|
| D406 lunar, facturi | FCT corectivă operată sau stornată în lună: linie cu cantitate comercială 0 | `SAFT_SURSA_INCOMPLETA` |
| D406 lunar, facturi | aceeași linie cu contrapartide pe stoc și pe consum | `SAFT_PROVENIENTA_AMBIGUA` |
| declarația de stocuri | orice lună cu alocare, compensare sau inversele lor | `SAFT_MISCARE_VALORICA` (SAFT-r2) |

Consecință asumată: luna în care o corecție este operată sau stornată nu se
poate exporta până la închiderea restanței. Luna care conține numai
compensarea se exportă în D406 lunar: compensarea este mișcare internă în
GL, sub jurnalul FCT, și nu devine factură. Nu se introduce cantitatea 1 ca
să treacă cititorul și nu se omit sume. SC-CVL-32 fixează refuzurile, ca
restanța să nu devină pe tăcute export greșit.

### CV-D10 — predicatele fixate la închiderea delimitării

Fixate la 2026-10-14, pe verificarea din R8. Se judecă pe forma faptelor
din cub, fără numele tipurilor de documente. „Activ” înseamnă fără inversă.

**Lotul corectabil.** Poziția de stoc este contul lotului în gestiunea
corecției. În afara familiei valorice, toate postările pe lot sunt:

1. recepția: o singură pereche `Operare` activă, cauzată de linia de intrare
   a lotului, cu debitul pe poziția de stoc, cantitate pozitivă, și cu
   creditul în afara lotului, pe furnizorul corecției;
2. consumuri: perechi `Operare` cu ambele picioare pe lot, creditul pe
   poziția de stoc și debitul pe altă poziție;
3. inversele exacte ale consumurilor.

Orice altă postare pe lot dă `CV_LOT_NEELIGIBIL`: transfer, deschidere,
transformare, pereche cu alt contrapicior, recepție inversată sau cu
diferențe.

**Beneficiarii.** Stocul rămas, ancorat în postarea de debit a recepției,
cu ponderea cantității din poziția de stoc la data corecției; și fiecare
consum activ, prin postarea lui de debit, cu ponderea cantității ei.
Ordinea este cea din CV-D3. Un consum beneficiar datat într-un an anterior
anului corecției dă `CV_AN_INCHIS`; stocul rămas nu (R8-9). Anul calendaristic
ține locul închiderii exercițiului până la operația explicită (CV-r2).

**Rădăcina.** Piciorul de debit al unei perechi `Operare` a corecției:
cantitate zero, `Atribuit` = postarea beneficiară, poziția beneficiarului
(cont, repartitor, lot, produs, carte, analiză), fără contrapondere.
Creditul perechii este partida proprie a corecției pe contul furnizorului
din regula de contare a liniei, cu `Contrapondere.Tert` și fără atribuire.
Ținta este beneficiar, niciodată altă rădăcină.

**Perechea de compensare.** Într-o `Operare` a corecției, datată la
stornarea beneficiarului: debitul pe poziția rădăcinii de consum, creditul
pe poziția de stoc a contrapiciorului beneficiarului, cantitate zero,
aceeași linie, ambele cu `Atribuit` = rădăcina. Valoarea este opusul valorii
rădăcinii active (R8-11). O rădăcină are cel mult o pereche. Rădăcina de
stoc nu are niciodată.

**Selecția la stornare.** Saltul pe `Atribuit` nu urmează postările pe lot
(R8-2). Stornoul beneficiarului conține inversele postărilor lui; stornoul
corecției, inversele tuturor postărilor ei `Operare`.

**Familia și gardul temporal.** La stornarea unui document, pentru fiecare
postare a lui pe lot: rădăcinile atribuite ei, inversele lor, perechile de
compensare atribuite acelor rădăcini și inversele lor. La stornarea unei
corecții se adaugă toate postările ei `Operare`. Familia se citește fără
filtru de dată. Data stornării este cel puțin data maximă a acestor fapte;
altfel `CV_CRONOLOGIE`.

**Cronologia operării.** Data corecției este cel puțin data maximă a
postărilor de pe lot. O operare nouă cu cantitate pe un lot cu rădăcină
activă, datată înaintea acelei rădăcini, dă `CV_CRONOLOGIE`.

**Dependenții**, toți cu `CV_DEPENDENTI`:

| Comandă | Refuz când |
|---|---|
| retragerea oricărui document | o postare a lui este ținta unui `Atribuit` de pe lot, activ sau nu |
| retragerea corecției | documentul are mai mult de o tranzacție `Operare` |
| retragerea sau stornarea corecției | lotul are o ieșire activă scrisă după prima `Operare` a corecției (R8-10) |
| stornarea recepției | lotul are o rădăcină activă |

Stingerile active ale partidei proprii rămân pe gardul existent al
partidelor.

**Valoarea.** După operarea sau stornarea unei corecții, niciun beneficiar
nu are valoare activă negativă: nici soldul valoric al poziției de stoc,
nici valoarea unui consum activ împreună cu rădăcinile lui active. Altfel
`CV_VALOARE_INSUFICIENTA`.

**Structura liniei**, cu `CV_STRUCTURA`: cantitate nenulă pe linia de
corecție, tip de TVA, lot repetat în document, linie obișnuită în același
document. Lotul lipsă și valoarea zero păstrează codurile existente.

## Restanțe

- **CV-r1** — reprezentarea SAF-T a corecției valorice: factura corectivă
  (tipul 380/381/384, cantitatea, distribuția pe conturi), separarea
  perechilor de compensare de liniile comerciale în evenimentul de storno și
  mișcarea de stoc numai valorică, împreună cu SAFT-r2. Până atunci, refuzul
  din CV-D9.

## Legătura cu costul vamal pe lot

După CV verificat, DVI furnizează mecanismului costul capitalizabil și
loturile/alocările explicite; mecanismul nu cunoaște clasa DVI. Matricea
următoare păstrează exemplul +200: intact 200/0, parțial 80/120, epuizat
0/200, cu contrapartida din politica vamală, nu din politica CVL.

Contractul DVI ulterior trebuie să fixeze separat taxa vamală/accizele,
baza TVA și TVA-ul în vamă, alegerea loturilor din facturile legate,
repartizarea unui cost între loturi, dependențele 86-r2 și inversarea
întregului document. O pereche fiscală nu crește costul sau datoria de
achiziție încă o dată. SC-X-13 și 86-r1 rămân deschise aici.
Valuta și unitatea de evaluare nu sunt condiții de intrare în acel pachet.

## Pași

| Pas | Schimbare | Verificare | Risc | Stare |
|---|---|---|---|---|
| 1 | inventar, contract, catalog numeric | surse, decizii, invarianți, docskit | confundarea primitivei cu mecanismul complet | redactat, fără implementare |
| 2 | producător FCT și proveniența compensării | matrice, cititori, explicație și cronologie; SC-CVL-25…32 | fapte omise de cititori, storno antedatat, export incompatibil | închis 2026-10-14: apartenența compensării (R6, decizia 119); explicația și storno recepției (CV-D8, CV-D5); SAF-T restanță cu refuz (CV-D9); predicatele verificate contra codului și fixate (R8, CV-D10) |
| 3 | mecanism pur, extensie FCT, operand, materializare și garduri | nucleu + SC-CVL prin comenzile reale, după fixarea perimetrului | dependenți valorici ignorați de gardul cantitativ | implementat 2026-10-14; scenariile CVL și integrala verzi pe ambele profiluri; resturile sunt în secțiunea de verificare |
| 4 | cititori, snapshot, explicație, XAF și securitate afectată | scenarii, metadata, HTTP/UI unde sunt atinse | afișarea unui cost recalculat ca fapt istoric | de făcut |
| 5 | regresie și review de închidere | integrală pe ambele profiluri, docskit, review advers | extindere tacită la lanțuri neacoperite | de făcut |

Înaintea implementării se fixează un an liber propriu și sufixul bazelor.
Rețeta de fixture este [BCS](scenarii/BCS.md): documente reale,
ObjectSpace nou pentru comenzi, constante independente, curățenie în
`finally`. Nu se scriu direct registre/cub și nu se comută `PosteazaInCub`.

Comenzi de acceptare **după înregistrarea selecției CVL** în harness:

```powershell
pwsh -NoProfile -File nou/tools/ModelCheck/scripts/verifica.ps1 -Suita Nucleu
pwsh -NoProfile -File nou/tools/ModelCheck/scripts/verifica.ps1 -Suita Scenarii -Tip CVL -Profil Ambele -Sufix .CodexCVL
pwsh -NoProfile -File nou/tools/ModelCheck/scripts/verifica.ps1 -Suita Integral -Profil Ambele -Sufix .CodexCVL
docskit gen
docskit check
docskit metrics --check
```

CVL nu este selecție executabilă astăzi. Pregătirea bazelor și coordonarea
rulărilor urmează [rețeta comună](../stare-curenta/dezvoltare-si-validare.md).

## Regula de oprire

Contractul nu autorizează trecerea la cod cu o contradicție numerică
nesoluționată. Se oprește pasul afectat dacă proveniența nu permite
compensarea o singură dată, o perioadă închisă trebuie rescrisă, sau
cantitate zero lasă valoare pe stocul gol. Nu se relaxează conservarea,
nu se schimbă așteptarea pentru a trece implementarea.

Pachetul se închide cu SC-CVL și SC-X-05/06/07 verzi, citiri și inversări
exacte, integrală pe ambele profiluri, garduri pentru cazurile excluse și
review advers închis. Nu închide TR-D9b, SC-X-09/13 sau costul vamal.

## Review advers și ce rămâne deschis

Review-ul pachetului originii rămâne separat. Pentru CV, proveniența și
explicația sunt verificate contra codului și fixate la 2026-10-14 (R8,
CV-D8, CV-D10); exportul q0/multicont este restanță cu refuz (CV-D9), nu
condiție a pachetului. Extensia FCT are recomandare confirmată de review.
Eligibilitatea, fiscalul, factura mixtă, cronologia și refuzul inversării
CVL cu ieșire ulterioară activă sunt refuzuri aprobate de owner la
2026-10-14 (CV-D1, CV-D5, CV-D10). Tranșările de formă din R8 sunt ale
reviziei: owner-ul nu s-a pronunțat separat pe ele și le poate întoarce
până la închiderea pasului 3. Review-ul advers de închidere este la pasul 5.

Nu se atribuie acestei cereri o amânare generală a review-ului advers CV:
owner-ul a cerut pregătirea contractului în timpul review-ului precedent.
Fără implementare în acest pas. Documentele au fost comise la cererea
owner-ului, la 2026-10-13.

## Închidere

Deschis. Redactarea și verificarea documentației nu certifică mecanismul.

### Verificarea pasului 3 — 2026-10-14

Implementat: atribuirea pe mișcare și mecanismul pur în nucleu
(`CorectieValorica`), felul liniei pe FCT cu migrația
`CorectieValoareLot`, faptele lotului în operand, declarația corecției,
compensarea la stornare, gardurile retragerii și ale cronologiei, explicația
persistată a celor două feluri de decizie, oglinda gardurilor în regimul pe
stare. Toate codurile `CV_*` stau în `CoduriRefuz`; nucleul emite
`MotivCorectieValorica` (CV-D7).

Comenzi executate din rădăcină:

- `dotnet test nou/Atlas.Conta.Nucleu/Atlas.Conta.Nucleu.Teste`: 235 de
  teste verzi, dintre care 36 în `CorectieValoricaTeste`.
- `verifica.ps1 -Suita Scenarii -Tip CVL -Profil Privat -Sufix .ClaudeCVL`:
  verde; `run-verificari/20261014-004610-087/rezultat.json`. Baza privată e
  creată și seed-uită de unealtă.
- aceeași comandă cu `-Profil Bugetar`, pe clona
  `Atlas.Conta.BackOffice.ClaudeCVL` adusă la migrația nouă: verde;
  `run-verificari/20261014-004657-481/rezultat.json`.
- Invarianții cubului (`INV-CUB`) trec pe faptele scenei, pe ambele
  profiluri, fără nicio relaxare.
- `verifica.ps1 -Suita Integral -Profil Ambele -Sufix .ClaudeCVL`: verde pe
  ambele profiluri; `run-verificari/20261014-005221-824/rezultat.json`.
  Prima rulare a picat pe SC-REGIM-13: oglinda gardurilor în regimul pe
  stare adaugă o citire. Plafoanele au crescut de la 10 și 12 la 11 și 13
  instrucțiuni SQL, cost confirmat de owner la 2026-10-14 (106f).
- Concurența și partidele (SC-CVL-21…24, 29), adăugate după integrală:
  `-Tip CVL` verde pe privat (`run-verificari/20261014-083922-101`) și pe
  bugetar (`run-verificari/20261014-084012-398`); `-Tip X` verde pe ambele
  profiluri (`run-verificari/20261014-084106-627`), fiindcă serializarea
  probei s-a mutat din `ScenariiConcurenta` în `ScenaDocumente`.
- Mutarea codurilor `CV_*` în `CoduriRefuz`: nucleul 235 de teste verzi;
  `-Tip CVL` verde pe privat (`run-verificari/20261014-085429-523`) și pe
  bugetar (`run-verificari/20261014-085458-555`).
- Invariantul de audit CV-D8: nucleul 247 de teste verzi, cu 12 mutanți ai
  probei pure; `-Suita Integral -Profil Ambele -Sufix .ClaudeCVL` verde
  (`run-verificari/20261014-095623-533`), fiecare ramură `INV-CUB` își
  ucide mutantul, inclusiv cei cinci `VALORICA-*`. Prima integrală
  (`run-verificari/20261014-095008-711`) a picat pe SC-NIR, privat: domeniul
  punctului 1 cuprindea și diferența valorică a NIR-ului, neatribuită.
- SC-CVL-20, cititorii de produs: `-Tip CVL` verde pe privat
  (`run-verificari/20261014-101238-933`) și pe bugetar
  (`run-verificari/20261014-101311-045`).
- SC-CVL-32, refuzurile SAF-T: `-Tip CVL` verde pe privat
  (`run-verificari/20261014-103824-417`) și pe bugetar, unde SAF-T nu se
  aplică (`run-verificari/20261014-103854-125`).
- După SC-CVL-20 și 32: `-Suita Integral -Profil Ambele -Sufix .ClaudeCVL`
  verde, purja fără reziduu (`run-verificari/20261014-103945-779`).

Ce nu e făcut și nu e certificat de aceste rulări:

- părțile lipsă din SC-CVL-18, 19, 28 și 31, enumerate în catalog;
- culegerea XAF, `WriteDto`, linia explicată și DTO-ul ei, driftul openapi:
  pasul 4;
- măsurarea pe scara de volum a interogării noi de pe calea operării
  (`CorectiiValorice.VerificaCronologia`).

### Verificarea pasului documentar — 2026-10-12

Comenzi executate din rădăcină:

- `pwsh -NoProfile -File nou/tools/ModelCheck/scripts/verifica.ps1 -Suita Infrastructura`:
  exit 0, build izolat fără avertismente/erori, probele de excepție și
  proces normal verzi. Manifest local:
  `run-verificari/20261012-184247-020/rezultat.json`; zero baze accesate.
- `docskit gen`: indexurile regenerate.
- `docskit check`: verde; avertismentele preexistente sunt aceleași ca
  înaintea editării (27 trimiteri, 218 restanțe, 3 cod, 4 căi).
- `docskit metrics --check`: forma codului la linia înregistrată.
- `git diff --check`: fără erori pe documentația atinsă.

Nu s-au rulat Nucleu/Scenarii/Integral: pasul nu modifică produsul,
selecția CVL încă nu există, iar review-ul anterior nu este certificat
de proba de infrastructură. Nu s-a modificat baseline-ul de metrici.

Revizia producătorului și a matricei are verificarea documentată în
[analiza punctelor 1 și 2](tr-d9b-corectii-valorice-producator-si-compensare.md#verificarea-redactării--2026-10-12):
Infrastructura exit0 în `run-verificari/20261012-212229-705/rezultat.json`,
docskit și diff verzi, control aritmetic pe16 ordini. Nici această revizie
nu certifică implementarea sau review-ul advers al CV. Rezultatele acestei
prime revizii precedau constatarea incompatibilităților; validarea R1 este
consemnată în [raportul de review](tr-d9b-corectii-valorice-review-r1.md).
