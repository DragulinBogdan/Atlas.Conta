# TR-D9b-CV — producătorul și matricea compensării

- Tip: analiza
- Data: 2026-10-12
- Alimentează: [contractul TR-D9b-CV](tr-d9b-corectii-valorice-contract.md)

Propunere documentară pentru punctele 1 și 2 ale review-ului: documentul
real și forma exactă a postărilor. Nu este implementare, aprobare de schemă
sau rezultat ModelCheck. Limitele primului pachet, cronologia și fiscalul
cer review separat înainte de cod. Așteptările economice rămân cele din
090 (j/k), amendate pentru storno de 091 (h).

Revizie după review-ul Claude: forma inițială k1/k2 în Storno este
retrasă. Aritmetica era corectă, dar contrazicea proveniența persistată și
cititorul contabil. [Raportul de review și inventarul cititorilor](tr-d9b-corectii-valorice-review-r1.md)
fixează varianta recomandată și blocajele rămase. Punctul 2 **nu este încă
gata de acceptare**; controlul aritmetic nu certifică integrarea.

A doua revizie (2026-10-13, decizia owner-ului): compensarea aparține
corecției A, nu beneficiarului B. Tabelele de mai jos poartă cauza A/lA pe
k1/k2 și pe inversele lor; comparația variantelor este în R6 din raport.

## 1. Producătorul recomandat: FCT cu linie explicită de corecție

Corecția comercială primită de la furnizor se culege într-o **FCT nouă**,
cu număr, dată, furnizor și scadență proprii. Pe frunza liniei FCT se propun
un fel explicit `CorectieValoareLot` și referința la linia FCT de recepție
corectată. Numele câmpurilor sunt de lucru, nu API/schema aprobată.
`LotId` identifică lotul existent; `Valoare` este delta semnată;
`Cantitate = 0`. Nu se deduce felul liniei din cantitate zero și nu se
simulează o cantitate de 1 pentru a trece validarea curentă.

FCT inițială rămâne operată. Referința comercială spre ea NU este
`CorecteazaId`: acesta descrie documentul înlocuit după storno, conform
088 (h), amendată de 103 (b/c). Comanda `Corecteaza` asupra FCT corective rămâne disponibilă prin
mecanismul comun, cu propriile garduri; nu este intrarea unei delta comerciale.

| Variantă | Constatare din surse și concluzie |
|---|---|
| FCT, recomandată | Are deja furnizor, numerotare, scadență, partidă și fapte fiscale. Lipsesc felul liniei, referința sursei, pregătirea valorii fără preț × cantitate și declararea ajustării. |
| Tip nou CVL nefiscal | Ar introduce încă un producător al aceleiași datorii. Înregistrarea ulterioară și a FCT ar dubla efectul dacă nu se construiește o reconciliere suplimentară. Nu este recomandat pentru această corecție comercială. |
| NTC | Perechile contabile nu înlocuiesc identificarea recepției, distribuția pe lot/consum și proveniența compensării. |
| NIR | Diferența de recepție corectează constatarea cantitativă/valorică și contrapartida diferenței; nu este documentul comercial de modificare a datoriei. |
| FCT care facturează avizul | 090 (h) dă deja lotului recepționat sensul 408 = 401. Referința la lot nu identifică o corecție valorică; felul explicit al liniei separă cele două operații. Contractul CV nu declară implementat întregul flux pe aviz. |
| `CorectieService` | Inversează și creează document înlocuitor. Nu exprimă o delta nouă peste o recepție rămasă operată. |

CVL rămâne abrevierea mecanismului și prefixul stabil `SC-CVL-*` din
catalog, **nu un tip nou de document** în propunerea revizuită. În exemple,
„CVL +200” înseamnă noua FCT cu linie de corecție +200.

FCT corectivă pur valorică nu naște lot, nu modifică prețul de intrare și
nu generează NIR. Reoperarea nu produce încă o recepție. Linia obișnuită de
recepție păstrează gardul cantității pozitive; cantitatea zero devine
permisă numai pentru felul explicit de corecție. Integrarea trebuie să
acopere și culegerea/prepararea, nu numai `DeclarantFacturaIntrare`:
`FacturaIntrareDetaliu` participă astăzi la nașterea lotului și la calculul
prin `PretUnitar × Cantitate`.

FCT corectivă își deschide propria partidă, pe contul de furnizor rezolvat
prin politică. FCT inițială rămâne la −1.000; corecția +200 adaugă o partidă
−200, iar corecția −200 una +200. Nu se postează aceeași datorie și printr-un
CVL auxiliar. Scenariile de stingere și compensare rămân SC-CVL-24.

Exemplele acestei felii sunt fără TVA. Alegerea FCT **nu suprimă fiscalul**:
pentru o linie taxabilă trebuie specificate baza, taxa și costul
capitalizabil înainte de a o accepta. Propunerea pilotului este refuzul
explicit al corecției taxabile neacoperite; nu omiterea faptelor TVA.
FCT mixtă (recepție + corecție) cere, de asemenea, delimitare înainte de cod.
Nu se restrâng facturile obișnuite. Aceste două limite rămân la punctul 3
al review-ului, împreună cu eligibilitatea lotului și cronologia.

## 2. Convențiile matricei

Exemplu privat: `F` = FCT inițială, `B` = BCS, `A` = FCT corectivă;
`lF/lB/lA` sunt liniile lor. Recepția FCT → NIR conex fără diferențe lasă
100 bucăți/1.000 lei; B consumă 60/600; A adaugă 200. NIR nu adaugă
încă o pereche de recepție în acest caz. Conturile sunt constante ale
exemplului, nu literali de introdus în motor.

`S` = 302 × MAG1 × lot L; `C` = 602 × LOC × lot L;
`T_F/T_A` = 401 × furnizor × partida F/A. Capetele interne poartă același
produs, carte și analizele beneficiarului istoric; unitatea lotului are
contul capătului respectiv. Contrapartida externă păstrează forma curentă
de terț și contragreutatea cantitativă; nu este beneficiar al distribuției.
Toate valorile sunt RON; măsura în valută este zero în acest fixture.

ID-urile `f1`, `b1`, `a1` etc. sunt etichete ale postărilor persistate.
În `InversaDin`, eticheta înseamnă referința completă `(Spațiu, ID)`.
În `Atribuit`, eticheta înseamnă ID-ul postării din spațiul Stoc; nu ID de
document sau lot. `—` înseamnă null. Perechile sunt locale tranzacției;
numerele din tabele sunt **etichete logice**, nu ordinale persistate de
asertat literal. `Storno.Inverseaza` remapează pe surse ordonate după Guid;
se asertează apartenența celor două picioare, unicitatea și conservarea,
nu valorile 1/2/3. Ordinalele postărilor se verifică separat, fără goluri.

### Recepție și consum: fapte care nu se rescriu

| ID | Tranzacție/document | Cauza | Pereche | Sens/poziție | Cantitate | Valoare | Atribuit | InversaDin |
|---|---|---|---:|---|---:|---:|---|---|
| f1 | Operare F | F/lF | 1 | D S | +100 | +1.000 | — | — |
| f2 | Operare F | F/lF | 1 | C T_F | −100 | +1.000 | — | — |
| b1 | Operare B | B/lB | 1 | D C | +60 | +600 | — | — |
| b2 | Operare B | B/lB | 1 | C S | −60 | +600 | — | — |

Stoc 40/400, consum 600, datorie 1.000. Compensarea nu recalculează aceste
patru postări și nu schimbă valoarea 600 de pe BCS.

### Corecție A: rădăcinile alocărilor

| ID | Tranzacție/document | Cauza | Pereche | Sens/poziție | Cantitate | Valoare | Atribuit | InversaDin |
|---|---|---|---:|---|---:|---:|---|---|
| a1 | Operare A | A/lA | 1 | D S | 0 | +80 | f1 | — |
| a2 | Operare A | A/lA | 1 | C T_A | 0 | +80 | — | — |
| a3 | Operare A | A/lA | 2 | D C | 0 | +120 | b1 | — |
| a4 | Operare A | A/lA | 2 | C T_A | 0 | +120 | — | — |

Stoc 40/480, consum 720, datorie 1.200. Rădăcinile interne sunt a1/a3;
perechile lor păstrează contrapartida. `Atribuit` pe terț rămâne null.
Pe a2/a4, `Contrapondere = Tert`, inclusiv la cantitate zero; se păstrează
forma capătului de terț al FCT. Pe a1/a3 și pe compensare este null.
În cazul intact rămâne doar perechea de stoc, cu 200; în cazul epuizat
doar perechea de consum, cu 200. Nu se scriu perechi de valoare zero.

### Ordinea I: storno B, apoi storno A

La storno B, aceeași comandă atomică scrie la aceeași dată tranzacția
Storno B (s1/s2) și câte o tranzacție `Operare` pe fiecare document de
corecție cu compensare nenulă: aici una, o a doua Operare a lui A (k1/k2).
Operare descrie faptul valoric suplimentar; B ajunge Stornat la commit,
A rămâne Operat.

| ID | Tranzacție | Cauza | Pereche | Sens/poziție | Cantitate | Valoare | Atribuit | InversaDin |
|---|---|---|---:|---|---:|---:|---|---|
| s1 | Storno B | B/lB | 1 | D C | −60 | −600 | — | b1 |
| s2 | Storno B | B/lB | 1 | C S | +60 | −600 | — | b2 |
| k1 | Operare A, compensare | A/lA | 1 | C S | 0 | −120 | a3 | — |
| k2 | Operare A, compensare | A/lA | 1 | D C | 0 | −120 | a3 | — |

Rezultat: stoc 100/1.200, consum 0, datorie 1.200. Perechea k1/k2 mută
ajustarea activă a consumului pe stoc. Are sensuri opuse și aceeași valoare
negativă, formă în roșu admisă în Operare. Nu este `Transfer`: schimbă
soldul conturilor 302/602. Are explicație proprie a compensării, fără
copierea deciziei de ieșire 60/600 și fără reevaluarea liniei BCS.
Nu inversează a3/a4 și nu atinge datoria față de furnizor.

Cei 120 provin din A; k1/k2 schimbă repartizarea acelui efect între consum
și stoc, iar stornarea lui B doar declanșează mutarea. Postările cu cauza B
(b1/b2, s1/s2) se închid la zero pe fiecare cont. Postările cu cauza A
poartă integral cei 200: 80 + 120 pe stoc, 0 pe consum, 200 datorie.
Într-o `Operare`, cauza este documentul tranzacției
(`Conservare.VerificaDocumentul`), deci k1/k2 nu pot sta în tranzacția lui B
cu altă cauză.

La storno A după această compensare, tranzacția **Storno A** conține:

| ID | Cauza | Pereche | Sens/poziție | Cantitate | Valoare | Atribuit | InversaDin |
|---|---|---:|---|---:|---:|---|---|
| i1 | A/lA | 1 | D S | 0 | −80 | f1 | a1 |
| i2 | A/lA | 1 | C T_A | 0 | −80 | — | a2 |
| i3 | A/lA | 2 | D C | 0 | −120 | b1 | a3 |
| i4 | A/lA | 2 | C T_A | 0 | −120 | — | a4 |
| j1 | A/lA | 3 | C S | 0 | +120 | a3 | k1 |
| j2 | A/lA | 3 | D C | 0 | +120 | a3 | k2 |

Rezultat: stoc 100/1.000, consum 0, datorie 1.000. Inversarea a3 este
compensată pe consum de inversarea k2; efectul total al comenzii este
−200 stoc/−200 datorie, fără consum negativ.

**Stornoul corecției nu are cauze străine.** k1/k2 sunt postări proprii ale
lui A, deci selecția postărilor cauzate le cuprinde direct, iar j1/j2 au
cauza și proprietarul A. `Atribuit = a3` rămâne legătura spre rădăcină:
un document cu mai multe rădăcini are perechi de compensare distincte, iar
activitatea se judecă pe fiecare. Familia valorică a corecției se
neutralizează integral la storno A.

### Ordinea II: storno A, apoi storno B

Storno A include numai i1…i4: stoc 40/400, consum 600, datorie 1.000.
Storno B include numai s1/s2: stoc 100/1.000, consum 0, datorie 1.000.
Alocarea a3 are inversa i3, deci suma activă este zero. Nu se emit k1/k2
cu 120 și nici o pereche artificială cu zero.

### Ordinea III: două corecții independente

A = +200 (stoc80/consum120), A′ = +100 (stoc40/consum60):

| Comandă | Efect asupra stocului / consumului / datoriei | Stare după: stoc; consum; datorie |
|---|---|---|
| A, apoi A′, după B | +120 / +180 / +300 | 40/520; 780; 1.300 |
| Storno B | +600/+180 pe stoc; −600/−180 consum; 0 datorie | 100/1.300; 0; 1.300 |
| Storno A | inversa celor patru postări A + inversa perechii proprii de compensare; net −200/0/−200 | 100/1.100; 0; 1.100 |
| Storno A′ | analog, numai familia A′; net −100/0/−100 | 100/1.000; 0; 1.000 |

Compensarea de 180 se scrie în **două tranzacții `Operare`**, 120 pe A și
60 pe A′, fiecare cu rădăcina ei: comanda storno B scrie 1 + 2 tranzacții.
Două rădăcini ale aceluiași document ar fi perechi distincte în aceeași
tranzacție. Nu se pierde proveniența printr-o singură pereche agregată.
Dacă A se stornează înainte de B, compensarea B este numai 60;
stocul ajunge la 100/1.100. Inversarea A′ înainte de A după compensare
lasă intermediar 100/1.200, apoi același final 100/1.000.

Toate cele șase permutări ale comenzilor de storno, pornind de la
40/520, consum780, datorie1.300, au următoarele stări. În fiecare celulă:
`cantitate/valoare stoc; consum; datorie`.

| Ordinea stornărilor | După prima | După a doua | După a treia |
|---|---|---|---|
| B → A → A′ | 100/1.300;0;1.300 | 100/1.100;0;1.100 | 100/1.000;0;1.000 |
| B → A′ → A | 100/1.300;0;1.300 | 100/1.200;0;1.200 | 100/1.000;0;1.000 |
| A → B → A′ | 40/440;660;1.100 | 100/1.100;0;1.100 | 100/1.000;0;1.000 |
| A → A′ → B | 40/440;660;1.100 | 40/400;600;1.000 | 100/1.000;0;1.000 |
| A′ → B → A | 40/480;720;1.200 | 100/1.200;0;1.200 | 100/1.000;0;1.000 |
| A′ → A → B | 40/480;720;1.200 | 40/400;600;1.000 | 100/1.000;0;1.000 |

### Storno recepției după corecție

Lot intact, A = +200: a1 este D S +200 cu `Atribuit = f1`, a2 este
C T_A +200. Cu A activă, storno F se refuză cu `CV_DEPENDENTI`: stocul
golit nu are unde primi cei 200. După storno A (i1/i2), storno F conține:

| ID | Cauza | Pereche | Sens/poziție | Cantitate | Valoare | Atribuit | InversaDin |
|---|---|---:|---|---:|---:|---|---|
| r1 | F/lF | 1 | D S | −100 | −1.000 | — | f1 |
| r2 | F/lF | 1 | C T_F | +100 | −1.000 | — | f2 |

Rezultat: stoc 0/0, datorie 0. a1 și i1 au amândouă `Atribuit = f1`, dar
nu intră în stornoul lui F: selecția de azi le-ar lua fără perechile lor și,
din citirea codului, ar pica pe `PerecheInvalida`; cazul nu a fost rulat.
Calea CV le exclude pentru orice beneficiar, nu numai pentru consum. În cazul parțial, după storno A și storno B, storno
F este același r1/r2. Dacă B a fost stornat cu compensare și A e încă
activă, cei 200 sunt pe stoc (80 + 120 relocați) și storno F se refuză.

### Diminuarea păstrează sensurile, cu valori negative

S-D14 din [TR-D7a](tr-d7a-strangler-contract.md) amendează regula veche
090 (f): `Operare` admite valori negative. Pentru A = −200, a1/a2 au −80,
a3/a4 au −120, **cu aceleași sensuri D/C**. Stoc 40/320, consum 480,
datorie 800. Storno B păstrează s1/s2 exact −600; k1 este C S +120,
k2 este D C +120. Rezultat 100/800, consum 0, datorie 800.

Storno A ulterior inversează a1…a4 și k1/k2: efect net +200 pe stoc,
0 pe consum, +200 datorie; final 100/1.000. Dacă A este inversată înainte
de B, compensarea B este zero și finalul este identic. Nu se folosesc
`Abs`, `Max(0, ...)` sau inversarea sensurilor pentru a ascunde semnul.

## 3. Reconstrucția efectului activ din fapte

Propunerea nu adaugă câmp de compensare în postare și nu folosește
`Suport`, rezervat relației cu fișa. Folosește roluri distincte:

1. Alocarea r: `Atribuit = postarea beneficiară istorică`, fără `InversaDin`.
2. Compensarea: ambele picioare au `Atribuit = r` și cauza lui r
   (documentul și linia corecției), fără `InversaDin`.
3. Inversa exactă: păstrează `Atribuit` și cauza sursei;
   `InversaDin = (Spațiu, ID sursă)`.

Rezultă cel mult două salturi în **definiția familiei**: compensare →
alocare → beneficiar. La storno A, k1/k2 sunt între postările proprii;
saltul pe `Atribuit` le regăsește, fără dublare. La storno B se
înlocuiește selecția cauzat ∪ atribuit pentru
loturi cu inversă proprie + compensare activă; altfel ar lua și a3/i3.
Nu este o extindere mecanică a selecției și nu schimbă atribuirea de partidă.
Nu cere registru auxiliar sau flag mutabil „compensat” (111f).

Pentru rădăcina r, familia cuprinde r, inversa sa, cele două picioare ale
compensării atribuite r și inversele lor. Se deduplică după `(Spațiu, ID)`:
compensarea este și cauzată de A, și atribuită rădăcinii, deci intră prin
ambele selecții. Familia nu include contrapartida de furnizor și nici alte
alocări ale aceleiași FCT. Sub aceeași serializare ca operarea/închiderea,
familia se descoperă **fără tăietură de dată**, inclusiv inversele și
compensările neutralizate. Dacă data comenzii precedă vreun fapt relevant,
se refuză cu `CV_CRONOLOGIE`. Abia după gard se calculează suma la data
comenzii; nu se ascund faptele viitoare pentru a construi altă istorie.

Măsura semnată de sold este `m(p) = Valoare` pentru Debit și `−Valoare`
pentru Credit. Efectul activ într-o poziție k este suma m(p) a familiei
**pe poziția k**, cu cheia completă cont/repartitor/lot/produs/carte/analiză
din contract; data, sensul și proveniența nu separă pozițiile acestei sume.

Pentru a3: înainte de compensare, C=120/S=0; după k1/k2,
C=0/S=120; după i3+j1/j2, C=0/S=0. Suma globală a perechii k1/k2 este
zero, dar aceasta nu înseamnă „nicio compensare”. Pentru −200, aceleași
poziții sunt −120/0, apoi 0/−120, apoi 0/0. Activitatea se deduce din
postări și inversele persistate, nu numai din starea documentului.

La storno B se compensează suma încă aflată pe beneficiarul b1, separat
pe rădăcini. La storno A se inversează postările proprii din `Operare`:
rădăcinile și compensările lor. Nu se inversează o inversă, iar la storno B
nu se selectează postările atribuite lui b1 (a3, i3). Perechile selectate
se remapează fără coliziuni de ordinal. Sumele și legăturile se includ în
operandul închis și în explicație, apoi comanda scrie atomic.

Această regulă NU rezolvă implicit ieșirile noi la preț ajustat, retururile
sau lanțurile BTR/ASM. Gardurile propuse pentru ele rămân în CV-D5 și la
review-ul perimetrului; nu se extinde algoritmul prin recursie arbitrară.

## 4. Obligații de implementare și probe care pot infirma propunerea

Conservarea existentă poate verifica perechile din matrice; ea nu verifică
singură semantica atribuirii. Implementarea trebuie să adauge verificarea
țintei existente și eligibile, a lotului/produsului/cărții, a coordonatelor
beneficiare, a ordinii faptelor, a absenței ciclurilor/autoreferinței și a
unicității inversei. O pereche balansată pe un alt lot este tot greșită.

SC-CVL-25/26 completează catalogul. În proba pură, următoarele mutații
trebuie detectate; fixture-ul de produs continuă să folosească documente
reale, fără inserare directă de postări:

| Mutație / eroare de selecție | Martor obligatoriu |
|---|---|
| Inversarea a3 la storno B fără contrapartida ei | refuz de conservare; nu se slăbește gardul pentru a trece |
| Omiterea k1 sau k2 | pereche incompletă / conservare încălcată |
| Dublarea k1/k2, chiar cu ordinale noi | refuz de proveniență sau sumă activă depășită; conservarea singură ar trece |
| Suma globală zero a k1/k2 tratată ca inactivitate | storno A ar lăsa stoc1.120/consum−120; martorul cere1.000/0 |
| Compensare scrisă cu cauza B, în tranzacția lui B | B Stornat rămâne cu 302 +120/602 −120; martorul cere suma zero pe fiecare cont a postărilor cu cauza B |
| Folosirea a3 deja inversate în ordinea II | compensare greșită120; martorul cere0 |
| Compensările A/A′ într-o singură tranzacție | refuz `CauzaStraina`; martor intermediar1.100/0 după storno numai al lui A |
| Două rădăcini ale aceluiași document agregate într-o pereche | activitatea pe rădăcină nu se mai reconstituie; martorul cere perechi distincte |
| Scrierea numai a unora dintre cele N compensări | rollback integral; B rămâne Operat, zero postări noi |
| Aplicarea `Abs` corecției negative | storno B trebuie să lase800, nu1.040 |
| Inversarea ambelor picioare cu aceeași referință `InversaDin` | refuz; fiecare inversă indică exact propria sursă |

Citirile la data anterioară rămân cele inițiale. După închiderea lui
ianuarie, perechile din storno datat februarie se scriu numai în februarie;
nu se mută data rădăcinii și nu se reface snapshot-ul ianuarie prin
reevaluare. SC-CVL-15…17 rămân martorii numerici.

## Starea review-ului

Punctele 1 și 2 au acum o propunere concretă și matrice verificabilă.
Review-ul confirmă producătorul FCT și aritmetica, dar nu acceptă punctul 2.
Revizia propune Operare separată pentru compensare și gard temporal;
predicatul, explicația și inventarul consecințelor sunt în raportul R1.
Apartenența compensării la corecție este fixată de owner (R6). Storno
recepției, explicația și exportul SAF-T lăsat pe refuz sunt în contract
(CV-D5, CV-D8, CV-D9).
Punctul 3 (perimetru, TVA, facturi
mixte, cronologie, ieșiri ulterioare) a fost aprobat de owner la 2026-10-14,
ca refuzuri. Predicatele și explicația sunt închise în aceeași zi prin R8 din
raport și CV-D10 din contract; referința la linia FCT de recepție din
secțiunea 1 este înlocuită de linia de intrare a lotului (R8-3).

## Verificarea redactării — 2026-10-12

Control inițial, numai în memorie, asupra perechilor și soldurilor intermediare:
16 ordini de storno (cele două pentru +200, două pentru −200, șase pentru
+200/+100 și șase pentru +200/−100). Toate perechile sunt balansate și
toate finalurile revin la100/1.000, consum0, datorie1.000. Acest calcul
independent nu execută motorul și nu certifică selecția/proveniența din cod.
Lipsa artefactului primei rulări a fost constatată în R1; revizia păstrează
simulatorul și rezultatul reproductibil indicate în raportul de review.

`verifica.ps1 -Suita Infrastructura`: exit0, build izolat fără avertismente
sau erori, probele de excepție și proces normal verzi. Manifest:
`run-verificari/20261012-212229-705/rezultat.json`; zero baze accesate.
`docskit gen`, `docskit check`, `docskit metrics --check` și
`git diff --check`: verzi; avertismentele de documentație rămân27 trimiteri,
218 restanțe,3 cod,4 căi. Baseline-ul nu a fost schimbat.

Nucleu/Scenarii/Integral nu au fost rulate pentru această schimbare
documentară. SC-CVL-01…26 rămân specificate, neimplementate și nerulate.
