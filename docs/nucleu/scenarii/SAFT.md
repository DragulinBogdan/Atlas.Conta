# SAF-T — scenarii S1 și matricea de delimitare S2/S3

**2026-09-28 — S1 aprobat (S1-D5 = B', 381 + 384) și implementat pe cub
(`SaftProiectii.SaftPeCub`). Probele S1 sunt în ModelCheck (`--scenarii
SAFT`, plus DEC, ASM și IMO); starea fiecărui rând este în coloana finală.
S2/S3 rămân în delimitare.**
Contract: [SAF-B1…B8](../tr-d8-saft-contract.md). Profil privat dacă nu este
indicat altfel. Simbolurile de cont sunt ale fixture-ului, nu constante în
motor. Datele și cota 21% sunt intrări controlate ale fixture-ului; nu se
deduc din exportul vechi. Codificarea XML a cazurilor încă deschise nu este
declarată aprobată. Numele de probe de mai jos sunt propuse.

**2026-09-29 — S0: artefactele S1 validate cu manifest (SC-SAFT-24/25).
Scena rulează în anul 2040: cota 21% nu există în nomenclatorul ANAF al
anului 2024 (S0-R6); cifrele economice nu se schimbă, iar datele
„ianuarie/februarie” de mai jos sunt ale anului scenei.**

**Review advers S0, reverificat pe `00d8845`: S0-RV1 și S0-RV1.1 închise.**
Proba respinge permutarea surselor GL și DocumentId/Storno falsificate pe
factură. Hărțile originale și santinela 2025-09 trec; fără constatări noi.
XSD/DUK rămân verificate, iar limitele S2/S3 și HTTP sunt neschimbate. S0 aprobat de owner (2026-09-29).
[Review-ul S0](../tr-d8-saft-s0-review-codex.md).

**Review advers Codex, reverificat 2026-09-29 pe `ad193a9`: R1, R2 și
R2.1 închise; probele publice S2 rămân.**
R1: unitatea produsului e imutabilă după operare (S1-R5, alegerea owner-ului
A); contraexemplul 10 H87 → KGM este refuzat, iar exportul rămâne 10 H87.
R2: `Compara` merge în ambele sensuri, pe facturi (tip, net, taxă, brut,
cont, linii, dată) și pe GL per document. Un mutant cu o factură omisă este
prins de comparator. R2.1: excepțiile fixează acum secțiunea, evenimentul
(document, storno) și valorile vechi/noi exacte. Gate-ul cere egalitatea
exactă cu lista declarată, iar mutanții (factură obișnuită omisă, factură
TI21 omisă) sunt respinși amândoi. Diferențele din ianuarie sunt: TI21 brut,
data facturii F și a stornoului, recepția la FCT față de NIR.
[Review-ul S1](../tr-d8-saft-s1-review-codex.md).

**2026-09-29 — S2 propus (contract §S2):** Payments din cub, alocarea la
capătul lunii operării, stornoul care neagă liniile declarate, comutarea L
și `SAFT_ACCES_INCOMPLET`. Rândurile SC-SAFT-26…36 sunt așteptări de
contract. S2-Q1 (stornoul neagă liniile declarate) și S2-Q2 (compensarea
în afara Payments, SAFT-r1) sunt tranșate de owner. Implementate și verificate:
SC-SAFT-05, 06, 13, 26…36; ruta L este comutată pe cub.

| Id | Scenariu și așteptare numerică | Proba propusă | Proveniență | Rezultat / stare |
|---|---|---|---|---|
| SC-SAFT-01 | FCT servicii 100 + 21: GL D 6xx 100, D 4426 21, C 401 121; PurchaseInvoice net 100, taxă 21, brut 121; plata 40 lasă furnizor 81 | SAFT-L-ACHIZITIE | 073, 090, 103; regulă contabilă | Verificat (ScenariiSaft) |
| SC-SAFT-02 | FCL servicii 200 + 42: GL D 411 242, C 7xx 200, C 4427 42; SalesInvoice 200/42/242; INC 100 lasă client 142 | SAFT-L-VANZARE | 073, 090, 103 | Verificat (ScenariiSaft) |
| SC-SAFT-03 | FCT capitalizată 100 + 21: D cost 121 / C 401 121; factura păstrează net 100 și taxă 21, fără dublarea bazei pe cost | SAFT-L-CAPITALIZAT | 103 și catalog FCT | Verificat (ScenariiSaft) |
| SC-SAFT-04 | DVI bază 100, TVA 21: GL D 4426 21/C 446 21; sold și rulaj contabil 21/21, nu 121/121; baza fiscală 100 rămâne în faptele fiscale; zero facturi DVI inventate | SAFT-L-DVI | Contract DVI, D8-B1, T-r11 | Verificat (ScenariiSaft) |
| SC-SAFT-05 | PLT 70, alocare 50: plată totală 70, referință factură numai pentru 50, rest 20; factura 100 rămâne cu 50. Desfacerea alocării restabilește 100/70, fără nouă plată | SAFT-P-ALOCARI | D8-B7; SAF-B3.3; S2-D3 | Verificat (ScenariiSaft) |
| SC-SAFT-06 | PLT 70 în ianuarie; legătură 50 în februarie: la 31 ianuarie alocare 0/rest 70; la 28 februarie alocare 50/rest 20; zero plată suplimentară în februarie | SAFT-P-LIMITA | SAF-B3.3; S2-D3 | Verificat (ScenariiSaft) |
| SC-SAFT-07 | FCT 10 buc × 10 = 100 în ianuarie; NIR egal în februarie: stoc ianuarie 10/100; NIR adaugă 0/0; final februarie 10/100. Codurile Movement rămân de tranșat | SAFT-S-FCT-NIR | 098, contract NIR delta; SAF-B3.5 | Economic specificat; XML deschis |
| SC-SAFT-08 | Deschidere 10/100 în MAG1; BTR 4/40 în MAG2: MAG1 6/60, MAG2 4/40; total 10/100. Inversa în februarie: 10/100 și 0/0; GL rulaj transfer 0 în ambele luni | SAFT-S-TRANSFER | 090, contract BTR; N-r8 | Acceptat; specificat |
| SC-SAFT-09 | ASM același cont: 2 A/100 devin 1 B/100. Movement real −2/−100, +1/+100; GL 0; contraponderi Transformare exportate 0. Inversa reface A 2/100 și B 0/0 | SAFT-S-ASM | ASM-B2…B7 | Acceptat; specificat |
| SC-SAFT-10 | ASM mixt: grup 371 transfer 60/60; grup 301→345 operare 40/40. GL numai D 345 40/C 301 40; comparația veche GL 0, delta explicită 40/40 | SAFT-L-ASM-MIXT | D8-B4, ASM-B7 | Verificat (ScenariiAsm, inclusiv inversa din februarie) |
| SC-SAFT-11 | PIF nominalizează 1.200 deja în contabil: GL suplimentar 0. AMO 100: D cheltuială 100/C amortizare 100. Carte Fiscal și transferurile de fișă nu dublează GL | SAFT-L-IMO | 097 și contract IMO | Verificat PIF, AMO și DEC (ScenariiImo, ScenariiDec); CAS rămâne în catalogul IMO |
| SC-SAFT-12 | Același lot în două gestiuni cu același nume, apoi pe două conturi: poziții distincte, suma 10/100 conservată; chei XML fără coliziuni | SAFT-S-CHEI | SAF-B3.6 | Specificat economic; codificare de pin-uit |
| SC-SAFT-13 | Utilizator ascunde 21 din FCT 100 + 21 sau un rând 401 121: nu primește declarație cu 100 ori 0; refuz înainte de XML, fără scurgere de sume ascunse | SAFT-HTTP-DREPTURI | SAF-D4 A aprobat, 103 | Verificat HTTP (`ProbeHttp/saft-acces.py`, host izolat) |
| SC-SAFT-14 | Bugetar cu documente operate: L și S neaplicabile, fără XML „gol valid”; cont/mapping/reper obligatoriu lipsă pe privat: refuz, nu zero | SAFT-HTTP-REFUZURI | 073/074 și SAF-B6 | Verificat: bugetar neaplicabil, mapare lipsă refuzată |
| SC-SAFT-15 | S1 livrat singur: ruta L rămâne cea existentă, fără noul GL combinat cu Payments vechi; S1+S2: L integral nou; C rămâne vechi până la S3 | SAFT-COMUTARE | R1, SAF-B1 | Verificat: ruta L neschimbată, cusături GL–balanță–facturi, A/B clasificat |
| SC-SAFT-16 | FCT capturată la 23:30 UTC, inversă și corecție în alte două zile UTC: trei timbre proprii; reexportul într-un alt fus nu schimbă SystemEntryDate | SAFT-TIMBRE | R2, S1-D3 | Verificat |
| SC-SAFT-17 | Citire începe cu FCT 100/21; altă sesiune comite FCT 50/10,50 între GL și facturi: primul fișier rămâne integral 121; al doilea are 181,50 | SAFT-REPEATABLE-READ | R3, SAF-B6 | Verificat, inclusiv refuzul ReadCommitted |
| SC-SAFT-18 | Ianuarie depus 100/21; corecție februarie 80/16,80, apoi martie 70/14,70: GL lunar 121, −24,20, −12,10; sold final 84,70; facturi după varianta S1-D5 | SAFT-CORECTII | 088, 103, S1-D5 | Verificat B' pe trei luni, cu reexport stabil octet cu octet |
| SC-SAFT-19 | Două cote: net 100/21 + 50/5,50 → net 150, TVA 26,50, brut 176,50; două linii fiscale, fără multiplicare prin join | SAFT-MULTICOTA | SC-FCT-07, S1-D2/D4 | Verificat |
| SC-SAFT-20 | Două linii 0,01 la 21% → net 0,02, TVA 0,00; separat net 100 cu TVA culeasă 21,01 → brut 121,01 | SAFT-ROTUNJIRE | SC-FCT-08, R6 | Verificat |
| SC-SAFT-21 | TI21 net 100: GL cost 100, furnizor 100, TVA D21/C21; debit=credit=121, factură de plată 100, nu 121 | SAFT-TAXARE-INVERSA | SC-FCT-10, S1-D4 | Verificat; diferența A/B de brut (vechi 121) clasificată |
| SC-SAFT-22 | Serviciu 2 ore × 50: cantitate cub 0, comercială 2; net cub 100/TVA21; factura are 2 și preț 50, nu cantitate 0/1 | SAFT-CANTITATE-COMERCIALA | S1-D4, declarant FCT Netul | Verificat, inclusiv corecția 3 × 40 și refuzul editării liniei operate |
| SC-SAFT-23 | Factură din 8 ianuarie primită/înregistrată la 5 februarie: ianuarie 0; februarie 100/21/121; InvoiceDate/TaxPointDate 8 ianuarie, GLPostingDate 5 februarie | SAFT-FACTURA-INTARZIATA | S1-D3/D5; fixture cu exigibilitate explicită 8 ianuarie | Verificat |
| SC-SAFT-24 | XML L lunile 1–3 (anul 2040): XSD v249 cu namespace-ul `d406` substituit declarat și DUK J2.2.18 pe perioada din antet, zero erori și atenționări; februarie are 381 și 384 cu același `InvoiceNo`; `SalesInvoices` fără intrări scris gol; luna 4 fără rulaj (cub și ruta veche) validă; santinela: ianuarie cu antetul pe 2025-09 trece la fel ca pe 2040; `manifest-d406.json` cu SHA-256 al artefactelor și proveniența fiecărei linii GL și facturi, verificată contra XML-ului și a cubului | SAFT-S0-CERTIFICARE | S0-R1…R8 | Verificat (ScenariiSaft) |
| SC-SAFT-25 | Mutanți: antetul ianuarie pe 2024 → codurile cotei 21% respinse de DUK; tranzacție fără `GLPostingDate` → XSD respinge; namespace `d406t` → XSD derivat și DUK D406 resping; copil obligatoriu fără namespace → XSD respinge; manifest cu linie GL omisă, cheie de postare dublată, surse permutate între RecordID-uri, factură omisă, DocumentId schimbat sau Storno inversat → respins | SAFT-S0-MUTANTI | S0-R3, S0-R4, S0-R8 | Verificat (ScenariiSaft) |
| SC-SAFT-26 | FCT G 100 fără TVA; PLT 50 cu sursa G (nominalizată la operare): o linie D 401 50 cu `SourceDocumentID` G, nu 100 prin citirea și a legăturii. PLT 120 cu sursa G de rest 100: 100 G + 20 rest; factura G rest 0 | SAFT-P-NOMINALIZATA | S2-D3.1; `DeclarantTrezorerie` | Verificat (ScenariiSaft) |
| SC-SAFT-27 | PLT 70 + legătură 50 pe F în ianuarie, storno în februarie: ianuarie 50 F + 20 rest; februarie −50 F și −20 rest, `TransactionID` = tranzacția stornoului; GL februarie D 401 −70 / C 5121 −70. Ramura: legătură în februarie, ștearsă în martie (cu legătura vie în perioadă deschisă, stornoul e refuzat de gardian), apoi storno în martie: ianuarie 70 rest, februarie nicio plată, martie −70 rest | SAFT-P-STORNO | S2-D3 (stornoul neagă E_O) | Verificat (ScenariiSaft) |
| SC-SAFT-28 | FCL V 242 către CA; INC 100 legată de V: linie C 411 100, `SourceDocumentID` V, `CustomerID` CA, `SupplierID` societatea; dispoziția de casă → 01/10, ordinul de plată → 03/42 | SAFT-P-INCASARE | S2-D1/D2; `LaturaContPropriu` | Verificat (ScenariiSaft) |
| SC-SAFT-29 | Virament BANCA→CASA 500: GL D 5311 / C 5121 500, zero plăți. PLT 300 către ANAF pe 4423 (fără `RolTert`): `Neincluse` `PlataFaraContTert`, GL neschimbat. PLT 200 angajatului pe 542: plată 200 cu codul societății pe ambele identificatoare și avertismentul `PlataCatreAngajat` | SAFT-P-EXCLUDERI | S2-D1; paritate cu ruta veche | Verificat (ScenariiSaft) |
| SC-SAFT-30 | Deschidere: partidă inițială furnizor FA 60; PLT 20 stinge partida inițială în ianuarie: linie D 401 20 fără `SourceDocumentID`, avertismentul `PlataPePartidaInitiala`; partida inițială rest 40 | SAFT-P-DESCHIDERE | S2-D3.1; SC-DES-03 | Verificat (ScenariiDeschidere, lunile 1–2) |
| SC-SAFT-31 | PLT 20 avans liber către FA; NTC compensează avansul (stinge partida PLT) în aceeași lună: plata 20 are o linie cu `SourceDocumentID` = numărul NTC; NTC nu devine plată (S2-Q2) | SAFT-P-COMPENSARE | S2-D3.2, S2-D4; SC-DES-11 | Verificat (ScenariiSaft) |
| SC-SAFT-32 | PLT 100 în ianuarie închis, corecție în februarie la 80: februarie are stornoul −100 (liniile ianuarie negate) și plata 80 a documentului nou, cu alocarea lui; ianuarie reexportat identic | SAFT-P-CORECTIE | S2-D3; SC-PLT-06 | Verificat (ScenariiSaft) |
| SC-SAFT-33 | PLT pe două linii 40 + 60 către același furnizor, pe aceeași partidă: o singură linie de plată 100 (partida, nu detaliul); centre de cost diferite pe cele două linii: analiza omisă pe linie, avertisment `PlataAnalizaMixta`, GL cu ambele analize | SAFT-P-MULTILINIE | S2-D2 | Verificat (ScenariiSaft) |
| SC-SAFT-34 | L complet pe lunile scenei, cu Payments nevid: XSD v249 + DUK J2.2.18 fără atenționări (inclusiv 50 + 20 fără referință, R4); manifestul leagă `(TransactionID, LineNumber)` de postările și transferurile sursă; mutanți respinși: linie de plată omisă, sursă permutată, `SourceDocumentID` schimbat; A/B plăți cu diferențele declarate exact (împărțirea 50 + 20, legătura viitoare, `TransactionID`) | SAFT-P-CERTIFICARE | S0-R1…R8, S2-D6 | Verificat (ScenariiSaft) |
| SC-SAFT-35 | Altă sesiune comite o legătură 50 între citirea GL și citirea plăților: fișierul are fie 70 rest, fie 50 + 20, niciodată amestec; refuzul ReadCommitted rămâne | SAFT-P-REPEATABLE-READ | R3, SC-SAFT-17 | Verificat (ScenariiSaft) |
| SC-SAFT-36 | Host viu: Admin complet → 200; fără drept pe `Postare` → 403 `SAFT_ACCES_INCOMPLET`; refuz condițional de rând pe `Postare` sau pe document; membru `Valoare`/`Partener` ascuns; metadate necesare ascunse → 403 înaintea primului byte, pe sumar și pe fișier, fără sume ascunse în corp | SAFT-HTTP-ACCES | SAF-D4, S2-D5 | Verificat HTTP (`ProbeHttp/saft-acces.py`, host izolat); SC-SAFT-36 și prin ScenariiSaft (tabelele citite) |

## SC-SAFT-15…17 — probe structurale cu rezultate măsurabile

SC-SAFT-15 folosește FCT 100/21 și PLT 40: sold furnizor 81, GL total
D/C 161/161, factură 121 și plată 40. În fiecare stare de livrare se verifică
și proveniența cititorilor, deoarece aceleași totaluri nu demonstrează
absența amestecului. S1 singur nu schimbă ruta L; după S1+S2 noul L nu
citește RegistruContabil, RegistruTva sau RegistruStoc pentru măsuri.

SC-SAFT-16 capturează timbrele persistate ale evenimentelor reale și verifică
proiecția datei UTC. Testul de conversie folosește suplimentar valori pure
fixe: 2026-01-10T23:30Z → 10 ianuarie, 2026-02-05T00:15Z → 5 februarie,
2026-02-06T12:00Z → 6 februarie; nu inserează aceste timbre în cub și nu
modifică ceasul sistemului. Originalul păstrează prima dată după corecție.

SC-SAFT-17 stabilește snapshotul prin prima citire SQL, apoi lasă sesiunea
a doua să comită factura 50/10,50. În aceeași citire, GL D/C=121/121,
facturi 100/21/121, furnizor 121. După închiderea citirii, un nou export
are GL D/C=181,50/181,50, facturi net 150/TVA 31,50/brut 181,50 și furnizor
181,50. Fișierul 121 la GL și 181,50 la facturi este defect. Verificarea
include tranzacție ambiantă compatibilă și refuzul uneia ReadCommitted.

## Ciclul contabil/fiscal: cronologie obligatorie pentru S1

Fixture separat pentru fiecare ramură, fără cumularea unor comenzi
incompatibile. Ianuarie = 2026-01, februarie = 2026-02.

1. La 10 ianuarie FCT servicii 100/21: debit contabil 121, credit 121,
   datorie 121, o factură cu 100/21. Aceasta este starea înaintea fiecărei ramuri.
2. Anulare în perioadă deschisă, fără dependenți: postările și factura nu mai
   apar; soldul redevine 0. Nu exportăm o factură storno pentru ștergerea
   unei operări anulate.
3. Storno în ianuarie: rulajele semnate nete și soldul devin 0; evenimentele
   istorice rămân distincte. Numărul/tipul facturilor XML depinde de B3.2.
4. Storno în februarie: ianuarie rămâne D/C 121/121 și sold furnizor 121;
   februarie are inverse semnate −121/−121 și sold final 0. Nu mutăm
   postările din ianuarie când se re-ștampilează reperul fiscal.
5. Corecție după închiderea lui ianuarie: inversă în februarie + înlocuitor
   80/16,80. GL februarie net D/C −24,20/−24,20, sold final furnizor 96,80.
   Reprezentarea facturilor a fost tranșată prin B' (S1-R1): 381 pentru
   inversă și 384 pentru înlocuitor, în februarie; ianuarie nu se rescrie.
6. Repetăm o a doua corecție: evenimentele nu se unesc pe simplul boolean
   Storno. Linia fiscală se leagă de evenimentul propriu, fără multiplicare.

### SC-SAFT-18 — așteptările XML ale celor două variante

Se păstrează XML-ul inițial al lui ianuarie ca artefact de „deja raportat”.
Datele documentului și exigibilității sunt 8 ianuarie; înregistrarea 10
ianuarie, corecțiile 5 februarie și 5 martie. Ramura Draft a corecției cere
422/SAFT_CORECTIE_INCOMPLETA înainte de XML, nu o factură rămasă la zero.

**Tranșat: B' (S1-R1).** Coloana B' este cea verificată; A și B rămân istoric.

| Moment / perioadă cerută | Varianta A | Varianta B | **B' (verificat)** |
|---|---|---|---|
| După prima operare, ianuarie | 1 factură 380: 100/21/121 | Identic | Identic |
| După corecția din februarie, ianuarie | 1 factură 380: 80/16,80/96,80; diferență față de artefact −24,20 brut | Rămâne 100/21/121 | Fișier identic octet cu octet cu artefactul |
| După corecția din februarie, februarie | 0 facturi pentru corecția tehnică; GL net −24,20 | 381 −100/−21/−121 și 380 80/16,80/96,80 | 381 −100/−21/−121 și **384** 80/16,80/96,80; GL net −24,20 |
| După corecția din martie, ianuarie | 1 factură 380: 70/14,70/84,70; diferență față de artefact inițial −36,30 brut | Rămâne 100/21/121 | Rămâne 100/21/121 |
| După corecția din martie, martie | 0 facturi pentru corecția tehnică; GL net −12,10 | 381 −80/−16,80/−96,80 și 380 70/14,70/84,70 | 381 −80/−16,80/−96,80 și **384** 70/14,70/84,70; GL −12,10; furnizor 84,70; februarie reexportat identic |

GL ianuarie rămâne 121/121, februarie −24,20/−24,20, martie
−12,10/−12,10; sold furnizor final 84,70. Identitățile celor cinci evenimente
economice sunt distincte; nicio taxă nu se alătură altei corecții. În A,
proveniența facturii finale arată lanțul și diferențele între secțiuni,
nu rescrie GL pentru a obține egalitate în fiecare lună.

### Vânzări, retururi și anulare — ramuri independente

- FCL servicii 200/42, apoi INC 100: SC-SAFT-02. În fixture separat fără
  încasare, inversa aceleiași luni lasă sold client 0 și GL net 0; inversa
  în februarie lasă ianuarie 242 și februarie −242. Corecția la 150/31,50
  lasă client 181,50, GL februarie net −60,50. Facturile urmează aceeași
  alegere A/B: o versiune 150/31,50 în ianuarie versus 381 −200/−42 și
  380 150/31,50 în februarie.
- Retur comercial RLF de 20/4,20 pe o FCT 100/21: datorie 121 înainte,
  96,80 după; factură RLF 381 −20/−4,20/−24,20 în luna returului în ambele
  variante. Referința indică factura originală, nu un document de corecție
  tehnică. Inversa RLF reface 121; codificarea inversei urmează S1-D5 și
  nu pierde semnul prin alegerea codului 381.
- RDC pentru vânzare 200/42, retur 20/4,20: creanța scade de la 242 la
  217,80. Pentru bunuri, fixture-ul are separat cost original 100 și cost
  returnat 10; revenirea în stoc +10 nu mărește netul facturii de retur.
  DSC inițial și componenta de cost RDC apar în GL, nu ca facturi suplimentare.
- Anularea în perioadă deschisă, fără dependenți sau declarații confirmate,
  elimină evenimentul: FCT/FCL din acea ramură nu mai apar în GL/facturi;
  nu se generează 381 pentru o operare anulată. Proba păstrează separat
  refuzurile existente când anularea nu este admisă.

### SC-SAFT-11 — completarea numerică IMO și DEC în GL

Suport FCT imobilizare 1.200 fără TVA: D imobilizare 1.200/C furnizor
1.200. PIF nu adaugă rulaj contabil. Prima AMO 100 contabil/50 fiscal:
în GL numai D cheltuială 100/C amortizare 100. CAS ulterior, fără alte
operații: D amortizare 100 + D cheltuială casare 1.100 / C imobilizare
1.200; netul contabil al fișei 0. Suma fiscală nu intră în GL. Fixture
separat DEC cost 100/TVA 21 decontat angajatului: D cost 100, D TVA 21,
C 542 121; GL D/C 121/121, zero facturi DEC în SourceDocuments.

### SC-SAFT-19…23 — conservare și surse

În SC-SAFT-19, stornoul semnat este net −150, TVA −26,50, brut −176,50,
cu fiecare taxă pe cota ei. În SC-SAFT-20, exportul nu recalculează taxa
culeasă 21,01 la 21 și nu „repară” taxa rotunjită 0,00 la 0,01.
În SC-SAFT-21, GL deductibil/colectat 21 nu devine factură brut 121;
taxa are maparea taxării inverse, datoria comercială rămâne 100.

SC-SAFT-22 începe cu serviciu 2 ore, net 100/TVA 21. Corecția pe document
nou la 3 ore și net 120/TVA 25,20 păstrează linia originală 2; versiunea nouă
are 3, preț 40. Stornoul original se leagă de cantitatea 2, nu de 3. Modificarea
liniei operate pe ușile publice trebuie refuzată și să lase factura/probele
neschimbate. Lipsa accesului la Quantity/UM refuză exportul, nu produce 0.

SC-SAFT-23 folosește explicit DataPrimire/DataInregistrare 5 februarie și
DataDocument/DataExigibilitate 8 ianuarie. Nu copiază luna D394 ca perioadă
D406. După inversarea din martie, GL februarie rămâne 121, martie −121;
reprezentarea facturii inverse urmează S1-D5, iar datele originale nu se
înlocuiesc cu data inversării.

Toate aceste așteptări sunt pentru review, nu rezultate executate. Forma
XML TI21, 381 și identificatorii se verifică și pe artefactele fixate la S0;
o respingere nu se rezolvă schimbând cifrele economice ale fixture-ului.

## Ciclul plății și al alocării pentru S2

FCT 100 fără TVA în fixture, PLT liberă 70: factura are rest 100, plata
liberă 70, contul furnizorului sold net 30. Legătura 50 lasă factura 50 și
plata liberă 20; soldul net rămâne 30. Desfacerea inversează transferul:
resturile redevin 100 și 70, soldul net tot 30, GL suplimentar 0.
Stornoul plății după desfacere readuce banca și furnizorul la starea de
dinaintea plății; evenimentul de plată inversă este −70. Nu confundăm această
inversă cu inversa alocării. În ramură separată, PLT nominalizată 50 pe
factură exportă 50 o singură dată, nu 100 prin citirea și a legăturii.

S2 adaugă corecția PLT/INC și valuta pe cifrele catalogului de trezorerie;
separă valoarea plății de diferența de curs, înainte de a fixa liniile XML.

## Ciclul stocului pentru S3

Din deschidere 10/100, BCS 3/30 lasă 7/70. Inversa lui în februarie readuce
10/100; corecția cu un BCS nou 2/20 lasă 8/80. Inițialul din februarie este
7/70; mișcarea netă a corecției este +1/+10; final 8/80. Verificăm identic
recompunerea din postări și citirea cu snapshot, inclusiv fără rulaj în lună.

La ASM din SC-SAFT-09, mișcările reale rămân chiar dacă toate postările
sunt Transfer. În SC-SAFT-10 inversa exclude din GL numai partea provenită
din Transfer și păstrează −40/−40 pentru Operare. Proba nu poate fi înlocuită
de excluderea întregii tranzacții/documentului ASM.

S3 trebuie să completeze înainte de cod NIR delta pe fiecare cauză, retururi,
LDI și diferența valorică fără cantitate, inclusiv istoricul ASM cu Δ.
Stornoul inversează Δ istoric, corecția evaluează din nou; nu se recalculează
vechea mișcare folosind prețul curent al lotului. Codurile ANAF pentru aceste
cazuri rămân blocajul explicit SAF-B3.5, nu o presupunere a probei.

S1 are matricea de contract de mai sus, condiționată de alegerea S1-D5.
S2/S3 păstrează starea de delimitare și necesită contractele proprii;
imposibilitatea separării Δ ASM din cub este consemnată în SAF-D3=C.
Nicio secțiune SAF-T nu primește aici starea `verificat`.
