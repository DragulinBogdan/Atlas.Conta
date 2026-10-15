# TVA și declarații

**Actualizat: 2026-10-15.** [Index](README.md)

Implicitele TVA, taxa la operare și faptul fiscal din cub, intervalele și diagnosticul, perioada de declarare și corecțiile, confirmarea depunerii, închiderea lunară, D300 și D394, accesul la citirea fiscală și exportul SAF-T.

Aceste reguli descriu comportamentul implementat. Acoperirea fiscală este
delimitată în [limite curente](limite-curente.md).

## Implicitele TVA

Implicitul ajută la culegerea unei linii noi. Nu rescrie documentele deja
culese și nu anulează ștergerea intenționată a unei valori dintr-o linie
existentă. Implicitele de sesiune aparțin clientului; cele de domeniu sunt
rezolvate pe server. (56, 81a)

`ImpliciteService` primește tipul documentului, partenerul, produsul și data.
Rezolvarea are doi pași:

1. Candidatul de regim `R` se caută în ordinea: implicitul partenerului,
   politica TVA implicită aplicabilă, implicitul tipului de document.
2. Candidatul produsului `P` se folosește dacă are același regim cu `R`.
   Dacă regimurile diferă se păstrează `R`; fără `R` se poate folosi `P`;
   fără ambii candidați rezultatul este absent. (81a, 81b)

Politica este selectată după tip document, clasă fiscală și dată: clasa
exactă are prioritate față de clasa absentă, apoi câștigă cea mai recentă
dată de început care nu depășește data documentului. Data absentă este cea
mai veche. Un candidat inactiv este omis, cu motiv; dacă politica selectată
indică un tip inactiv, nu se caută a doua politică din clasament. (81b)

Clasele fiscale sunt `InregistratRo`, `NeinregistratRo`, `Ue` și `ExtraUe`.
Clasificarea este comună cu raportarea: înregistrarea TVA din România are
prioritate, apoi sunt evaluate persoana fizică și țara. (71b, 81b)

`GET /api/implicite/tip-tva` întoarce valoarea, sursa și motivul rezolvării.
Datele partenerului și produsului sunt citite securizat. Un obiect invizibil
nu devine sursă de implicit și nu este deosebit public de unul inexistent.
Un cod de tip document necunoscut este cerere invalidă. (80a, 81f, 81-r6)

Dezactivarea unui tip TVA folosit ca implicit este refuzată cu lista
referințelor. Tipurile inactive rămân valabile pentru istoricul documentelor;
selectoarele React pentru culegere nouă oferă tipurile active. Filtrarea
echivalentă a selectoarelor XAF este o limită curentă. (81c, 81e, 81-r7)

### Seed-ul TVA

Seed-ul privat configurează FCL/RDC pentru UE și extra-UE cu SDD, iar
FCT/RLF pentru UE cu TI21. Cazurile fără politică folosesc regulile de
fallback de mai sus. Aceste valori descriu seed-ul, nu o acoperire completă
a tuturor situațiilor fiscale. Profilul bugetar nu primește aceste rânduri. (81b, 81-r1)

Seed-ul privat acoperă și achiziția de la un partener neînregistrat din
România (`FCT`/`RLF` → `NIM`, fără fapt de TVA pe linie) și achiziția din
afara UE (`FCT`/`RLF` → `IMP`, tip propriu cu cotă 0 sub regimul neimpozabil,
fără cod SAF-T — codurile de import sunt ale DVI-ului — și nemapat deliberat
pe D300 și D394). (83f–g, 84b)

Tipurile de import sunt marcate prin `TipTva.DeImport`, nu prin coduri în cod.
Seed-ul privat are `IMP21`/`IMP11` (regim normal, coduri SAF-T de achiziție
301204/301205, D300 rd. 24/25) și `IMPTI21`/`IMPTI11` (taxare inversă la
import, 300604/300605, rd. 7/22 din Autocolectare/Taxă); declarația
vamală are politica TVA deductibilă cu contrapartida pe predator (fallback
446), implicitul generic `IMP21` și nicio mapare D394. Bugetar are doar
ancora tipului. (86c)

## TVA la operare

`TipTva` descrie cota și regimul; politicile stabilesc sensul și conturile.
TVA cules explicit este păstrat la operare și validat, nu înlocuit automat
cu rezultatul unei recalculări. Recalcularea din culegere și validarea la
operare au responsabilități distincte. Totalul brut participă la stingere. (36a, 36b, 56)

Taxarea inversă pe sens deductibil generează autolichidarea. Pe sens
colectat, TVA trebuie să fie zero și nu se generează notă TVA; o valoare
nenulă este refuzată înainte de materializare, inclusiv la salvarea REST. (70a, 70b)

Linia declarației vamale poartă doar tipuri `DeImport` cu cotă (`IMP` cu cotă
0 este refuzat la operare); taxa culeasă se păstrează, iar zero se completează
din cotă la operare. (86d)

### Repartizarea taxei pe linii

Taxa nemarcată se decide pe document × cotă, din valorile rotunjite ale
liniilor fiscale, și se repartizează pe linii (Hamilton peste |valoare|, pe
fiecare semn, în ordinea operării). Formula e una singură, pe entitate:
culegerea o aplică la fiecare recalcul și la salvare, operarea la pregătire.
Linia poartă deci taxa pe care o postează cubul și registrele, iar totalul
documentului este totalul partidei, pe draft și după operare. Declarația
postează taxa liniei, fără să o decidă din nou; taxa marcată rămâne a
operatorului și intră neschimbată. Un tip fără politică de TVA păstrează taxa
calculată pe linie. INV-CUB refuză taxa postată separat care diferă de taxa
liniei (`CITIRE_TAXA_DIFERITA_DE_LINIE`); taxa capitalizată stă în valoarea
liniei și nu intră în comparație. Formula scrie fiecare câmp o singură dată:
renormalizarea unui document neschimbat nu modifică nicio linie.
Liniile noi fără poziție vin după cele numerotate, în aceeași ordine folosită
de salvare pentru atribuirea pozițiilor. Calculul nu atribuie poziții;
acestea se scriu în continuare sub blocajul salvării.
(090j, 109 a/b/g; SC-FCL-11…14, SC-FCT-11)

### Taxa culeasă (`TvaCules`)

`DocumentDetaliu.TvaCules` distinge taxa nenulă introdusă explicit de taxa
calculată. L3 îl aprinde la culegere și îl stinge la schimbarea bazei/tipului,
zero în ecran sau recalcul explicit pe selecție. Zero explicit prin Apply
păstrează refuzul C104. `ValoareTva = null` prin WriteDto, cu baza
neschimbată, păstrează taxa și marcajul existente. Acțiunea XAF de recalcul
este disponibilă numai în lista de linii inclusă în documentul cu TVA,
cu selecție explicită de linii draft. Corecția copiază taxa și marcajul, conexul niciuna;
RDC/RLF păstrează marcajul la schimbarea semnului. CHECK-ul exclude marcajul
pe zero. Operarea păstrează numai taxa marcată; cea automată urmează cota
curentă și repartizarea pe document. Toleranța configurată rămâne refuz.
O cale care nu culege prin L3 și aduce taxa unui document real (conectorul
1C) o marchează la scriere; nemarcată, taxa se recalculează. (103i, 109 c)

## Faptul fiscal în cub

Cubul păstrează pe fiecare fapt fiscal regimul, cota, importul, valorile cu
semn, identitatea documentului fiscal și reperele temporale. Intrarea comună
`Cub.Citiri.Fiscale` citește ambele cărți; contul și cartea păstrează
proveniența, fără să despartă baza fiscală DVI de taxa contabilă. Un fapt
are cel mult o Bază și o Taxă; achiziția cu taxare inversă adaugă o
Autocolectare pe contrapartea colectată, cu același sens Achiziție.
Nomenclatorul curent oferă etichete, fără să recalculeze cota istorică. (103)

Faptul fiscal poartă pe postare partenerul fiscal și locul, separat de
repartitorul de sold. `PartenerFiscal` e latura numită de
`PoliticaTva.SursaContrapartida`: predatorul sau primitorul documentului. O
sursă care nu numește o latură (`Explicit`, `TipMaterial`) îl lasă nul, și
nimic nu-l completează după aceea, nici pe capătul cu partidă, nici pe contul
care cere repartitor (S13, C111-R16). Pe politicile de TVA din seed latura
numită e terțul documentului. `Loc` nu vine din politică: e singura latură
internă a documentului, citită din laturile lui tipate (C111-Q10). Baza și
taxa aceluiași document poartă același loc, iar capătul de taxă are locul ca
repartitor de sold. Cine scrie fiecare coordonată: [motor](motor.md),
„Regula coordonatelor”. (S13, C111-R16, C111-Q10)

Faptele fiscale stau numai pe postări. Nu există snapshot fiscal cumulativ și
nici compatibilizare a istoricului de dezvoltare. (102, 103, D9-D2)

`CK_Postare_FiscalComplet` impune la scriere calificările și reperele
obligatorii când `TipTvaId` este prezent, inclusiv primirea la achiziții.
Nu există scanare de istoric la pornire. Invarianții între roluri și
proveniența inversei rămân verificați în ModelCheck. (103)

## Intervalele tipului de TVA și diagnosticul

`TipTva.ValabilDeLa` / `ValabilPanaLa` sunt opționale și incluzive;
intervalul inversat se refuză. Implicitul din culegere folosește
exigibilitatea (fallback data documentului), exclude candidații din afara
intervalului cu motiv în `Explica` și păstrează filtrele de acces/Activ.
Intervalele seed sunt cele din R6-B8, aliniate numai pe `DinSeed`. (103h/i)

Raportul „Impact TVA” (`/api/proiectii/diagnostic-tva`) citește faptele din
cub și drafturile din agregat, prin ObjectSpace secured. Filtrează pe
exigibilitate și tip, arată calificarea înghețată/actuală și avertizează la
abatere mai mare de 0,01 lei față de repartizarea nucleului. Nu reconstruiește
taxa capitalizată și nu dublează autocolectarea. Lipsa mapării SAF-T este
explicită numai când SAF-T se aplică; diagnosticul și proiecția SAF-T
folosesc același criteriu, care exclude profilul bugetar.
Inversele legate compensează originalul inclusiv între luni;
avertismentele compensate apar numai la cerere. Operarea emite mesaje
minimale pe poziția liniei proprii, fără GUID; raportul nu dezvăluie sursa
inaccesibilă. (103h)

### Regularizarea avansului

`TipMaterial.RegularizareAvans` și baza negativă identifică regularizarea
FCT/FCL. `LinieAvansId` nominalizează linia pozitivă din altă factură,
operată, marcată, a aceluiași partener/sens. Sursa oferă calificarea și
reperul pentru diagnostic, fără să schimbe perioada sau taxa regularizării.
Sursa invalidă, lipsă ori compensată produce avertisment; FK-ul nu se
redirecționează la corecție. Retururile și reducerile fără sursă fiscală
verificabilă primesc avertisment de proveniență în locul verdictului de
interval. Contract: `docs/nucleu/tr-d8-tva-intervale-contract.md`. (103h/i)

## Perioada de declarare și corecțiile

Data documentului, exigibilitatea, primirea la achiziție și înregistrarea
sunt distincte. Primirea se propune din înregistrare, rămâne editabilă în
draft și se îngheață la operare. D394 folosește primirea/emisia; D300,
perioada înregistrării la achiziție și cea a exigibilității la livrare.
O livrare omisă dintr-o perioadă D300 deja declarată intră în regularizarea
curentă. Nu există parametru `DeclarareIntarziata` în politica TVA. (103)

DTO/Apply pentru FCT/FCL/DEC/DVI/RDC/RLF transmit exigibilitatea și, la
achiziții, data primirii. Clientul propune primirea din înregistrare, fără
să suprascrie o dată introdusă explicit. Jurnalul filtrează perioada D300
și afișează separat perioada D394 și reperele istorice. (103)

La eroarea de evidență 100/21 → 80/16,80, dacă D300 inițial a fost depus,
inversa tehnică și versiunea corectată produc Δ −20/−4,20 în regularizarea
curentă, păstrând trecutul declarat. D394 înlocuiește perioada inițială cu
80/16,80 și o singură factură. Inversa tehnică nu este o factură distinctă;
un partener vechi cu net zero nu contribuie la numărătoare. În lipsa unei
depuneri D300, corecția rămâne în atribuirea inițială. Corecția TVA
capitalizată nu inventează o deducere. (103c)

Factura distinctă de reducere, inclusiv returul comercial, are propriile
date și propriile atribuiri. Draftul `FaptNou` propune data corecției ca
dată a documentului, exigibilității și primirii, unde se aplică. Codul enum
`EroareMateriala` se păstrează, dar eticheta este „Eroare de evidență”; el nu
implementează procedura ANAF pentru eroarea materială a formularului.
Reclasificarea semnificativă pe 1174 rămâne notă manuală. (103, F27-r1)

Anularea operării unui fapt inclus într-o declarație confirmată este
refuzată și după redeschiderea contabilă. Corecția tehnică începută trebuie
încheiată: draftul nu poate fi șters, iar depunerea perioadei afectate este
refuzată cât timp versiunea înlocuitoare nu este operată. (103d)

## Confirmarea depunerii

Depunerea se confirmă explicit pe formular, an, lună și versiune exportată.
Versiunea emisă în DTO-ul D300/D394 (`VersiuneExportata`) este SHA-256 peste
faptele perioadei din citirea comună, formular și perioadă. Raportul și
amprenta folosesc același snapshot tranzacțional. Confirmarea recalculează
amprenta sub blocaj exclusiv; o versiune depășită (faptele s-au schimbat
între export și confirmare) primește `DEPUNERE_VERSIUNE_DEPASITA`. UI
descarcă exportul JSON cu amprentă și păstrează versiunea acelui export
pentru confirmare, fără etichetă liberă. Aceasta verifică faptele cubului, nu
parametrii manuali sau fișierul XML ANAF: JSON-ul nu este XML ANAF, iar
parametrii externi D300 nu sunt certificați de amprenta faptelor. (103d)

`DepunereDeclaratie` păstrează momentul și utilizatorul, fără sume fiscale
paralele; nu se editează prin CRUD generic. Aceeași confirmare este
idempotentă. Confirmarea și scrierile fiscale se serializează tranzacțional.
Închiderea sau redeschiderea contabilă nu confirmă și nu șterge depunerea.
D394 compară `ScrisLa` cu ultima confirmare; D300 folosește regularizări și
nu expune automat o rectificativă. Confirmarea din aplicație este evidența
depunerii efectuate de utilizator, nu transmitere către ANAF. (103d)

`GET/POST /api/depuneri-declaratii/{formular}/{an}/{luna}` citește sau
confirmă depunerea. POST cere versiunea exportată, drept de scriere și
perioadă vizibilă; refuzurile au 400/403/404/422. Aceeași versiune este
idempotentă. OData expune istoricul numai pentru citire. D300/D394 oferă
confirmarea pentru o singură lună, cu versiunea și momentul afișate. (103d)

## Închiderea lunară TVA

ITV este document specializat generat de serviciul lunar. Politica are patru
conturi; închiderea nu transferă rezultatul în 121 și nu închide perioada. (46c)

Pentru o lună există cel mult o închidere vie. Generarea este idempotentă:
soldul nul nu cere document. Profilul fără conturi este inert. Cronologia
ține cont de drafturile anterioare, documentele ulterioare operate și
perioadele închise. Comparația care detectează un draft depășit este comună
cu interfața. (46c, 79a)

Previzualizarea este fără scrieri și explică refuzul prin motive precum
`ProfilInert`, `InchidereVie`, `FaraSold`, `NeCronologica`, `DraftAnterior`
sau `PerioadaInchisa`. Generarea poate răspunde cu succes fără document,
însoțită de motiv. Regenerarea verifică toate condițiile și drepturile,
inclusiv dreptul de creare, înainte să înlocuiască draftul într-o tranzacție. (79a, 79d)

## D300 și D394

D300 proiectează faptele fiscale din cub prin `MapareD300` către rândurile de operații.
Structura `RandD300` este furnizată de seed și nu se editează prin API;
formulele sunt cod. O mapare poate contribui la mai multe rânduri, fără
dublare pe lanțul strămoș–descendent. (69a, 69b, 69c)

Totalurile și rândurile calculate respectă semantica explicită a fiecărui
rând. Rândul 31 scade nedeductibilul din rândul 30 folosind operanzii
înregistrați. Limitarea la zero se aplică numai rândurilor care o cer.
Valorile nemapate sunt raportate cu motiv, iar avertismentele nu trunchiază
sumele. Coloanele neaplicabile sunt absente, nu zerouri fabricate. (69c, 69d, 69e)

Importurile intră în decont din declarația vamală, nu din factura furnizorului
extern: rd. 24/25 pentru taxa plătită în vamă, rd. 7 (și oglinda 22) pentru
amânarea plății; SAF-T D406 emite codurile de import din aceleași fapte,
fără filtru de tip. (86c, 86k)

D394 grupează document fiscal × storno comercial × partener × sens × tip TVA × calificare istorică.
Partenerul e partenerul fiscal al faptului (`Postare.PartenerFiscal`, prin
`Cub.Citiri.Fiscale`); repartitorul de sold al postării nu intră în grupare.
Faptul fără partener fiscal intră în `Neincluse` cu `FaraPartener`; cel al
cărui partener fiscal nu e de fel partener (angajatul decontului, o latură
internă), cu `RepartitorNePartener`. (S13)

Clasificarea partenerului folosește aceeași funcție fiscală ca implicitele.
CUI-ul este normalizat pentru agregare; partenerii multipli cu același CUI
pot forma aceeași poziție. Înregistrarea TVA are prioritate în grup. (71b, 71d, 81b)

`MapareD394` permite operațiile de vânzare L/V/LS și de cumpărare A/C/AS pe
sensul corespunzător. AI este derivată și nu se configurează ca mapare.
Numărul de facturi distinge documentul fiscal și storno-ul comercial, fără
inverse tehnice. Totalurile pe sens se reconciliază cu faptele cubului prin
operațiile incluse și `Neincluse`. Ștergerea
logică a partenerului nu elimină faptele fiscale. Secțiunile neacoperite
produc limite și avertismente, nu date presupuse. (71c, 71d, 71e)

## Fișierul D300

`D300Xml.Genereaza` serializează decontul pe schema v12
(`mfp:anaf:dgti:d300:declaratie:v12`), pentru perioade de la 2026-01. Nu
recitește cubul: proiecția D300 rulează în modul `lei`, care rotunjește
fiecare rând de operațiuni la leu (sub 50 de bani se neglijează), separat bază
și TVA, înaintea însumării pe rândurile „din care” și a formulelor; oglinzile
și totalurile ies din aceleași formule. Rândul tipărit trece în atributul
schemei după harta fixă din cod (rd. 17 → `R64`, rd. 19 → `R17`, rd. 24 →
`R22`…). `totalPlata_A` e suma atributelor R. `nr_evid` e numărul de
evidență al plății: obligația (301 lunar, 302 trimestrial), perioada, scadența
la 25 a lunii următoare și cifra de control. `bifa_interne` se propune 1 când
rândurile intracomunitare (1–8, 20–23, 29.1) sunt nule. Antetul vine din
societate; datele de perioadă (temeiul, rambursarea, pro-rata, bifele de
taxare inversă) sunt parametri cu implicitele formularului. Aceeași perioadă,
cu aceleași date și același antet, dă același fișier. (E2-D1…D4)

Fișierul lipsește și `D300Dto.RefuzuriXml` numește cauza când: perioada nu e
luna sau trimestrul întreg al periodicității societății ori e dinaintea
formularului (`DECLARATIE_PERIOADA_NESUPORTATA`); antetul are câmpuri
obligatorii lipsă, cu lista lor (`DECLARATIE_ANTET_INCOMPLET`); un tip de TVA
cu sume nenule nu are rând, un rând primește o coloană pe care n-o are sau
nomenclatorul n-are un rând al hărții (`DECLARATIE_DATE_NEINCLUSE`). Proiecția
JSON și fișierul poartă aceleași refuzuri. (E2-D5)

## Fișierul D394

`D394Xml.Genereaza` serializează declarația pe schema v5
(`mfp:anaf:dgti:d394:declaratie:v5`), pentru operațiunile de la 2025-08-01,
din proiecția D394 în modul `lei`: fiecare rând `op1` se rotunjește la leu,
iar rezumatele, `nrCui*` și `totalPlata_A` (numărul partenerilor plus bazele
L, A și AI din `rezumat2`) ies din rândurile rotunjite. Operațiunile V și C
poartă defalcarea `op11` pe categoria de bunuri a produsului liniei: factura
cu mai multe categorii numără 1 la categoria cu baza (V) sau TVA-ul (C) cel
mai mare, iar restul de rotunjire al bazelor merge pe cea mai mare.
`rezumat1` agregă categoriile în `detaliu`, cerealele pe bunul 21. Încasările
AMEF, facturile simplificate și TVA-ul la încasare ies cu zero. (E2-D3, E2-D6)

`serieFacturi` declară plajele de facturi active cu decizia până la sfârșitul
perioadei (tip 1) și, pe fiecare, primul și ultimul număr emis în perioadă
(tip 2). Factura emisă e documentul fiscal cu fapte de livrare în perioadă,
fără storno; numărul ei se leagă de plajă prin seria normalizată (fără spații,
separatori finali și diferențe de majuscule) și cifrele finale. `nrFacturi`
e numărul facturilor emise. O factură emisă invizibilă utilizatorului oprește
fișierul. (E2-D7, 122 f)

Fișierul lipsește și `D394Dto.RefuzuriXml` numește cauza pentru perioadă,
antet (telefonul, reprezentantul, întocmitorul și calitatea lui), sume
neincluse, avertismentele care ar fi erori la validator (`Tip1FaraCui`,
`PfFaraCnp`, `TvaPeTipFaraColoana`, `CotaNeintreaga`, `FaraOp11`,
`CombinatieRefuzata`, judecate pe sumele dinaintea rotunjirii), orice rând
neinclus (chiar cu net zero), o factură emisă în afara oricărei plaje active
sau mai mult de cinci cote. Telefonul are cel mult 15 caractere, iar banca din
D300 cel mult 50. (E2-D5, E2-D7)

## Accesul la citirea fiscală

`/politici/tva-saft` editează mapările versionate și calificarea istorică.
Metadata include enumurile proprietăților persistente, inclusiv cele din
nucleu. Rapoartele fiscale cer citire pe `Postare`; lipsa dreptului pe
întregul tip este 403, iar filtrarea pe obiect/membru se aplică sursei comune.
Această validare fiscală nu certifică toate câmpurile SourceDocuments SAF-T. (103)

## Exportul SAF-T (D406)

D406 L (`GET api/proiectii/saft` și `…/saft/xml`) și S (`…/saft/stocuri` și
`…/saft/stocuri/xml`) citesc cubul (`SaftProiectii.SaftPeCub`,
`SaftStocuriPeCub`). Excepție de la filtrarea rapoartelor fiscale: exportul
cere citire necondiționată. Orice criteriu de rând sau de membru pe tabelele
citite de export (`SaftAcces.Citite` pe L, `SaftAcces.CititeStocuri` pe S, cu
toate tipurile mapate în ele) dă 403
`SAFT_ACCES_INCOMPLET` pe ambele uși, înaintea proiecției. Corpul numește
tipurile și membrii restricționați, fără rânduri ori sume: întâi tipurile
citite de secțiune, fără frunzele TPH ale unei baze deja restricționate. Criteriile se
citesc din `ISelectDataSecurity`, aceeași sursă din care EF Core filtrează.
`CanRead(Type)` nu ajunge, fiindcă răspunde `true` sub restricții
condiționale. (SAF-D4, S2-D5, S3-D6)

Refuzurile proiecției (`SaftDto.Refuzuri`) apar în sumar
(`Refuzuri`, 200; ecranul le arată înaintea descărcării și ține butonul
XML inactiv) și dau 422 pe fișier, înaintea primului byte. Fișa contului
deschisă din S3 revine la SAF-T (`inapoi`, numai rută internă). (SAF-D4, S2-D5, S3-D6)

Cusăturile ecranului L compară declarația cu cubul: rulajul balanței,
faptele fiscale și baza lor pe sens. Cele ale ecranului S compară fiecare
poziție cu postările pe lot ale lunii și stocul pe cont cu soldul contabil;
diferența pe cont se sparge în „(sold inițial)” (din snapshot) și
componentele lunii pe tipul documentului, fără a citi istoricul (B8-Q3).
(SAF-D4, S2-D5, S3-D6)

Soldurile de cont și ale terților din L și S pornesc din snapshot-ul contabil,
deci accesul complet cerut include perioadele fiscale și soldurile de perioadă.
(SAFT-r4, X-D5)

## Limitele implementării

- Reperele vechi `RegistruTva.ScrisLa` nu au compatibilizare: citirea fiscală
  folosește cubul și confirmarea explicită a depunerii; bazele de dezvoltare
  se recreează. (103, 102)

<!-- gen:decizii -->
## Decizii

Deciziile care numesc pagina în `Arii:` (16; titlurile în [index](../decizii/README.md); generat de `docskit gen`, nu se editează): 25, 36 (amendată de 71), 46 (amendată de 113), 56 (amendată de 77, 81), 69, 70, 71 (amendată de 72), 79 (amendată de 80), 80, 81, 84, 86, 90 (amendată de 91, 92, 93, 95, 96, 110, 119), 102, 103, 122 (propusă).
<!-- /gen:decizii -->
