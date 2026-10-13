# Termeni

**Actualizat: 2026-10-13.** [Index](README.md)

Glosarul numește termenii paginilor de stare curentă și trimite la regula
fiecăruia, fără s-o dubleze. Un termen intră aici la prima lui folosire
într-o pagină de stare curentă, în același commit; definiția e o
propoziție, regula detaliată rămâne în pagina ei. `[de confirmat: …]`
marchează un termen folosit în două sensuri sau fără definiție clară, până
la tranșarea owner-ului; un cuvânt cu mai multe sensuri se califică
(regimul pe stare, rolul de terț), nu primește sinonim (118).

## Termeni de domeniu

### document

Operația economică intră ca document cu două laturi (`Predator`,
`Primitor`), un antet și linii; tipul concret, ancorat unu-la-unu în
`TipDocument`, dă sensul laturilor și declarantul, și nu se inventează
prin configurare. Regula: [domeniu § Modelul comun](domeniu-si-operare.md#modelul-comun) (3, 20, 22, 81e).
Înrudit: linie, latură. În cod: `Document`, `TipDocument`.
În sistemul vechi: tabelă unică de documente + tabelă unică de detalii cu
`TipDocument`, document generic predator → primitor.

### linie

Rândul documentului: tip material, lot, cantitate semnată, valoare de
postare, TVA, cu derivata declarată prin `[TipDetaliu]` și ordinea de
culegere în `Pozitie`. Regula: [domeniu § Modelul comun](domeniu-si-operare.md#modelul-comun) (22, 36a, S-D6).
Înrudit: document, lot. În cod: `DocumentDetaliu`.

### latură

Una din cele două părți ale documentului, `Predator` sau `Primitor`, cu
repartitorul ei și cu partea permisă de contractul laturilor; debitul și
creditul postării nu sunt laturi, ci sensul ei, iar „latura pereche” a
viramentului e documentul pereche (65). Regula:
[domeniu § Laturile ca structură](domeniu-si-operare.md#laturile-documentului-ca-structură-t-d13-felia-32-pasul-2b) (T-D13, 118).
Înrudit: parte, capăt, sens. În cod: `Document.Laturi()`, `LaturaDocument`,
`Declaratii.Latura`; debitul și creditul postării nu poartă numele ăsta
nici ca tip, nici ca membru sau coloană (118-r7, TR-D9b pasul 6).
În sistemul vechi: predator → primitor configurat per tip × predator × primitor.

### sens

Debitul sau creditul postării, coordonată obligatorie a cubului; aceeași
familie numește sensul taxei (`SensTva`) și al stingerii (`SensStingere`)
și nu se confundă cu contarea, care e regula de atribuire a conturilor.
Regula: [domeniu § Nucleul pur](domeniu-si-operare.md#nucleul-pur-și-declarația-fluxului-pilot-bcs-plt-fct) (N-D2, 118).
Înrudit: latură, postare, coordonată. În cod: `Sens` (nucleu, 118-r7),
`SensTva`, `SensStingere`. Membrul care poartă debitul sau creditul se
numește `Sens`, cu coloana `Sens` de pe `Postare` (TR-D9b pasul 6). Numele
necalificat e al debitului/creditului; membrul care poartă un sens calificat
îi ia numele întreg (`SensTva` pe `Postare`), regulă a owner-ului din
2026-10-13. Membrii mai vechi de tip `SensTva` numiți `Sens` (`CodTva`,
mapările D300/D394/SAF-T, proiecțiile fiscale) nu sunt încă aduși la ea.

### parte

Poziția repartitorului față de patrimoniu: `Extern` (partener, angajat),
`Intern` (gestiune, unitate internă), `Propriu` (cont propriu); contractul
laturilor declară părțile permise pe fiecare latură a tipului. Regula:
[domeniu § Laturile ca structură](domeniu-si-operare.md#laturile-documentului-ca-structură-t-d13-felia-32-pasul-2b) (T-D13).
Înrudit: latură, repartitor. În cod: `Parte`.
În sistemul vechi: intern/extern per tip în `GEST_DEFA_DOCUM`.

### repartitor

Ierarhia celor care dau sau primesc: partener, gestiune (internul care
ține stoc), angajat, unitate internă, cont propriu; calitățile (loc de
consum, comisie, centru de cost) sunt atribute, nu clase. Postarea poartă
un singur repartitor de sold, cu felul lui. Regula:
[domeniu § Modelul comun](domeniu-si-operare.md#modelul-comun) (16, 111).
Înrudit: parte, partener, fel de repartitor, loc. În cod: `Repartitor`,
`Calitati`; pe postare, `Repartitor` = `(Id, Fel)`.

### fel de repartitor

Clasa repartitorului de sold al postării: partener, angajat, gestiune,
cont propriu, unitate internă; contul declară felurile admise, cu sau
fără „niciunul”, iar gardul analizei refuză repartitorul de alt fel.
Regula: [domeniu § Contare și dimensiuni](domeniu-si-operare.md#contare-și-dimensiuni) (111, C111-D4).
Înrudit: repartitor, parte, cont. În cod: `FelRepartitor`,
`Cont.FeluriRepartitor`.

### partener

Terțul extern, furnizor și client ca roluri ale aceluiași rând; pe
postare, e repartitorul de sold al capătului de terț și titularul
partidei.
Regula: [domeniu § Declarația fluxului per tip](domeniu-si-operare.md#declarația-fluxului-per-tip-tr-d6b-felia-30) (16, B-D8, 111).
Înrudit: repartitor, partidă, partener fiscal. În cod: `Partener`; pe
postare, `Repartitor` cu felul `Partener`.

### partener fiscal

Partenerul din blocul fiscal al postării cu fapt fiscal: nu poartă
partidă, nu e repartitorul de sold al postării și poate lipsi. Regula:
[domeniu § Nucleul pur](domeniu-si-operare.md#nucleul-pur-și-declarația-fluxului-pilot-bcs-plt-fct) (B-D8 pct. 5, 111, C111-D2).
Înrudit: partener, fapt fiscal, loc. În cod: `PartenerFiscal`.

### loc

Locul intern al postării când nu e repartitorul ei de sold: pe orice
postare cu fapt fiscal („loc fiscal”) și pe capătul intern căruia
titularul unei partide i-a luat locul; nu intră în nicio cheie de sold.
Regula: [domeniu § Nucleul pur](domeniu-si-operare.md#nucleul-pur-și-declarația-fluxului-pilot-bcs-plt-fct) (111, C111-D2, C111-Q14).
Înrudit: repartitor, partener fiscal, fapt fiscal. În cod: `Loc`.

### contrapondere

Marcajul postării care ține contracantitatea în afara evidenței: terț,
consum, inventar sau transformare; postarea nemarcată e în evidență.
Regula: [domeniu § Nucleul pur](domeniu-si-operare.md#nucleul-pur-și-declarația-fluxului-pilot-bcs-plt-fct) (N-D4, 111, C111-D2).
Înrudit: capăt, repartitor, lot. În cod: `Contrapondere`.

### capăt

Una din cele două postări ale unei mișcări, numită după ce poartă (de
terț, propriu, intern) și după unde stă (în evidență sau marcat cu
contrapondere); „picior” e
sinonimul de vorbire al paginilor, iar „proiecție” rămâne al citirilor
(42c). Viramentul intern e perechea PLT/INC cu capetele pe conturi
proprii. Regula:
[domeniu § Declarația fluxului per tip](domeniu-si-operare.md#declarația-fluxului-per-tip-tr-d6b-felia-30) (B-D8, S-D7, T-D13, 64, 118).
Înrudit: latură, postare, contrapondere. În cod: `Capat`.

### unitate de sold

Obiectul nominalizat pe care se ține soldul: lotul, partida și fișa de
imobilizare sub același concept, scurt „unitate” în pagini; FIFO consumă
întâi unitatea numită pe linie. Nu e repartitorul `UnitateInterna`, nici
dimensiunea bugetară `Unitate`, nici unitatea de măsură. Regula:
[domeniu § Nucleul pur](domeniu-si-operare.md#nucleul-pur-și-declarația-fluxului-pilot-bcs-plt-fct) (N-D6, N-D7, 118).
Înrudit: lot, partidă, imobilizare, document deschizător. În cod:
`UnitateSold` (TR-D9b, 118-r8). Membrii existenți numiți `Unitate`, inclusiv
identificatorul persistat pe postare, păstrează numele.

### lot

Identitatea unei intrări în stoc (produs, proveniență, gestiune, dată,
preț de intrare fixat la operare); se naște pe linia de intrare și nu-și
schimbă prețul. Regula: [domeniu § Stoc și evaluare](domeniu-si-operare.md#stoc-și-evaluare) (13, 25c, 26e).
Înrudit: unitate de sold, evaluare FIFO. În cod: `Lot`.

### partidă

Unitatea de terț: cont cu `UrmarestePartide` × partener × document
deschizător, cu identificator determinist; partida inițială vine din
deschidere, iar restul ei e suma netă a postărilor. Regula:
[domeniu § Partide: raport, snapshot și împerechere](domeniu-si-operare.md#partide-raport-snapshot-și-împerechere-101-2026-09-25) (092, 100, 101).
Înrudit: unitate de sold, stingere, partidă deschisă. În cod: `Cub.Citiri.Partide`.

### evaluare FIFO

Ieșirea e identificare specifică pe lot: pinul manual are prioritate,
apoi FIFO în ordinea intrării în evidență; valoarea vine din soldul
cubului în secvența liniilor, ultima bucată ia restul. Regula:
[domeniu § Stoc și evaluare](domeniu-si-operare.md#stoc-și-evaluare) (13, 38b, 090 j, D9-D3, 114).
Înrudit: lot, sold. În cod: `Evaluare.Iesire`.

### sold

Suma postărilor pe orice cheie, citită ca snapshot-ul ultimei perioade de
referință plus rulajele de după ea; proiecțiile (balanță, fișă, stoc,
partide, jurnale) sunt citiri agregate peste cub, iar clientul nu
calculează niciodată sold, rest sau total. Regula:
[domeniu § Soldul citit](domeniu-si-operare.md#soldul-citit-referință-plus-rulaje) (F27-D3, 42c, 66).
Înrudit: snapshot, cub. În cod: `SolduriService`, `Proiectii/`.

### deschidere

Soldul inițial scris ca tranzacție `Deschidere` fără document, detaliat
prin loturi și partide inițiale, o dată per bază; singura scriere directă
a migrării; „soldul inițial” rămâne coloana balanței. Regula:
[domeniu § Ce rămâne al feliilor următoare](domeniu-si-operare.md#ce-rămâne-al-feliilor-următoare) (094, DES-B1…B4, 107, 118).
Înrudit: partidă, tranzacție, document deschizător, partidă deschisă.
În cod: `Materializare.Deschide`.

### document deschizător

Documentul operat care naște o unitate de sold și intră în identitatea și
în ordinea ei FIFO (data deschiderii, documentul deschizător, id);
unitățile din deschidere nu au document deschizător. Regula:
[domeniu § Partide: raport, snapshot și împerechere](domeniu-si-operare.md#partide-raport-snapshot-și-împerechere-101-2026-09-25) (092, 114).
Înrudit: unitate de sold, partidă, lot, deschidere.

### partidă deschisă

Snapshot-ul partidelor cu rest la închiderea perioadei de referință, pe
unitate × cont × partener, scris din cub în tranzacția închiderii; nu e
partida inițială din deschidere. Regula:
[domeniu § Partide deschise](domeniu-si-operare.md#partide-deschise) (101, F27-D7).
Înrudit: partidă, snapshot, deschidere. În cod: `PartidaDeschisa`.

### data înregistrării

Data la care documentul intră în evidență, pe care se scriu postările și
se naște lotul; distinctă de `Data` documentului fizic, nu o poate
preceda și e reperul gardianului de perioadă. Regula:
[domeniu § Data documentului și data înregistrării](domeniu-si-operare.md#data-documentului-și-data-înregistrării) (F27-D4).
Înrudit: perioadă fiscală, perioadă de declarare. În cod: `DataInregistrare`.

### operare

Comanda care calculează, validează și materializează: dreptul, starea și
perioada, contractul declarației, numărul și scadența, loturile, apoi
tranzacțiile în cub, totul într-o singură tranzacție. Regula:
[domeniu § Operarea](domeniu-si-operare.md#operarea) (33d, 42b, D9-D2…D4).
Înrudit: stare, declarant. În cod: `MotorOperare`.

### stare

`Draft` (editabil, se șterge fizic), `Operat` (needitabil, cu postări),
`Stornat`; anularea readuce în Draft și șterge tranzacțiile (perioadă
deschisă, fără dependenți), stornarea adaugă tranzacția `Storno` la data
stornării, cu postările la valori negative, fără schimb de latură. Regula:
[domeniu § Anulare și storno](domeniu-si-operare.md#anulare-și-storno) (14, 25d, 46a, S-D5).
Înrudit: operare, corecție. În cod: `Stare`.

### corecție

Comanda `Corectează`, singura cale peste graniță: stornează originalul la
data cerută și creează în aceeași tranzacție un Draft nou de același tip,
legat 1:1, cu motivul care decide efectul fiscal. Regula:
[domeniu § Corecția unui document operat](domeniu-si-operare.md#corecția-unui-document-operat) (F27-D6, 103c).
Înrudit: stare, retragere, perioadă de declarare. În cod: `CorectieService`.

### retragere

Întoarcerea documentului operat în Draft, în perioadă deschisă și fără
dependenți, cu tranzacțiile lui șterse; numită „corecția directă” în 104f
și în invarianții III. Gardienii retragerii dau, în ordinea motorului,
motivele citibile ale anulării, stornării și corecției. Regula:
[domeniu § Anulare și storno](domeniu-si-operare.md#anulare-și-storno) (104f, 106k, 118).
Înrudit: stare, corecție, gardian. În cod: `Motor/GardieniRetragere`.

### regim pe stare

Singura sursă a editabilității și a comenzilor disponibile: pentru fiecare
comandă din vocabularul închis întoarce motivul indisponibilității sau
nimic, cu predicatele gardienilor motorului. Regula:
[domeniu § Regimul pe stare](domeniu-si-operare.md#regimul-pe-stare-106) (106b…k).
„Regim” se califică întotdeauna: regimul pe stare e al documentului,
regimul de TVA (Normal, TaxareInversa, Capitalizat, Scutit, Neimpozabil)
e al taxei.
Înrudit: stare, gardian, tip de TVA. În cod: `RegimDocument`.

### perioadă fiscală

Veriga (an, lună) a lanțului: închiderea e comandă cu constatări
structurale (în cod, blocante) și de conținut (severitate din politică),
perioada închisă e graniță absolută, cea absentă e închisă, iar cea de
referință (ultima închisă sau un decembrie închis) poartă snapshot.
Regula: [domeniu § Perioada fiscală ca lanț](domeniu-si-operare.md#perioada-fiscală-ca-lanț) (F27-D1…D3).
Înrudit: snapshot, data înregistrării. În cod: `PerioadaFiscala`.

### perioadă de declarare

Coordonata fiscală (lună) în care faptul se declară, distinctă de data
faptului și decisă de politică când perioada faptului e închisă;
depunerea se confirmă pe formular, lună și versiunea exportată. Regula:
[politici § Perioada de declarare și corecțiile](politici-si-fiscalitate.md#perioada-de-declarare-și-corecțiile) (103, 103d, F27-D5).
Înrudit: fapt fiscal, perioadă fiscală. În cod: `PerioadaDeclarare`.

### stingere

Reducerea restului unei partide: automată la operare, prin nominalizarea
partidei sursei pe postarea de terț a stingătorului; manuală prin notă
contabilă (partidă numită sau FIFO); a partidei inițiale prin deschidere.
Regula: [domeniu § Împerechere și stingere automată](domeniu-si-operare.md#împerechere-și-stingere-automată) (82a, 102, 115).
Înrudit: partidă, împerechere. În cod: `Fifo.Nominalizeaza`.

### împerechere

Legătura datată, cu sumă, dintre două documente operate; creată după
operare mută suma între partide printr-o tranzacție `Transfer`, iar
dintr-o perioadă închisă se desface prin rând invers. Regula:
[domeniu § Împerecherea ulterioară operării](domeniu-si-operare.md#împerecherea-ulterioară-operării--tranzacție-transfer) (31d, F27-D8, S-D13).
Înrudit: stingere, tranzacție. În cod: `Imperechere`.

### conex

Documentul generat de operarea sursei, creat în tranzacția ei cu
`DocumentSursaId` și `Autogenerat` și operat separat: NIR din FCT (postează
doar diferența față de recepția facturii), plata din factură, descărcarea
de gestiune DSC din FCL (lotul iese la cost, fără TVA, cu contraponderea
de terț, cu pinurile înaintea FIFO). Regula:
[domeniu § Documente generate și viramente](domeniu-si-operare.md#documente-generate-și-viramente) (17, 26d, 37b, 098, T-D4).
Înrudit: document, evaluare FIFO. În cod: `PoliticaConex`, `DescarcareService`.

### profil

Pachetul de seed al bazei, privat (OMFP 1802, sursa cerințelor) sau
bugetar, fixat per bază împreună cu rotunjirea (scara bani 18,2 / prețuri
18,6 / cantități 18,3, prețul `AwayFromZero`, sumele după convenția
profilului); profilurile diferă prin date și validări, nu prin clase.
Regula: [politici § Configurarea profilului](politici-si-fiscalitate.md#configurarea-profilului) (4, 49e, 51c, 52a).
„Politica de validare” (`PoliticaValidare`, 84e–h) e o politică, nu un
profil; profilul nu e „șablon”, fiindcă e realiniat continuu prin `DinSeed`.
Înrudit: seed, politică. În cod: `SetareProfil`, `Scara`.

### tip material

Clasificarea operațională a produsului, cu cont implicit și clasă (natura
`Stoc` intră în regulile de stoc, clasa F e a imobilizărilor); distinctă
de identitatea produsului. Regula: [domeniu § Stoc și evaluare](domeniu-si-operare.md#stoc-și-evaluare) (23b, 26c).
Înrudit: cont, lot. În cod: `TipMaterial`, `ClasaProdus.Natura`.
În sistemul vechi: Clasă/Tip pe produse ≈ plan de conturi operațional.

### cont

Poziția din planul sintetic, cu analiticele din dimensiuni și cu
atributele care conduc motorul (urmărirea partidelor, rolul comercial
SAF-T, dimensiunile obligatorii, categoria de stoc); niciun simbol de cont
nu stă în cod. Regula: [domeniu § Contare și dimensiuni](domeniu-si-operare.md#contare-și-dimensiuni) (10, 29, 096).
„Rol” se califică: rolul de terț al contului (`RolTert`), rolul liniei,
rolul coloanei, rolul de securitate.
Înrudit: dimensiune, partidă. În cod: `Cont`, `UrmarestePartide`.

### dimensiune

Analiticele laturii, culese pe frunzele liniilor și păstrate plat pe
postare (repartitor, material, cod funcțional, cod economic, sursă de
finanțare, unitate, proiect, centru de cost); rezolvarea urmează linia,
regula, antetul, lotul. Regula: [domeniu § Contare și dimensiuni](domeniu-si-operare.md#contare-și-dimensiuni) (15, 25f, 54c).
Dimensiunea e analitica laturii, culeasă; coordonata e axa postării, pe
care dimensiunile se păstrează plat.
Înrudit: cont, coordonată. În cod: `DimensiuniCulese()`.

### tip de TVA

Cota și regimul unei taxe; implicitul pe linie se rezolvă partener →
politică → ancora tipului de document, împăcat cu produsul și cu clasa
fiscală a partenerului, în intervalul de valabilitate. Regula:
[politici § Implicitele TVA](politici-si-fiscalitate.md#implicitele-tva) (81a, 81b, 103h/i).
Înrudit: taxă nemarcată, fapt fiscal. În cod: `TipTva`.

### taxă nemarcată

Taxa neintrodusă explicit de operator: se decide pe document × cotă din
valorile rotunjite ale liniilor și se repartizează pe linii prin
Hamilton; linia poartă taxa postată, taxa marcată rămâne a operatorului.
Regula: [politici § TVA la operare](politici-si-fiscalitate.md#tva-la-operare) (090j, 109 a/b/g).
Înrudit: linie, tip de TVA. În cod: `TvaCules`.

### fapt fiscal

Postarea care poartă codul de TVA (tip × sens × rol: Bază, Taxă,
Autocolectare), perioada declarării, locul și, când există, partenerul
fiscal; faptele stau numai pe postări, în ambele cărți. Regula:
[politici § TVA la operare](politici-si-fiscalitate.md#tva-la-operare) (103, D9-D2, 111).
Înrudit: tip de TVA, coordonată, loc, partener fiscal. În cod:
`Cub.Citiri.Fiscale`.

### imobilizare

Fișa (număr de inventar, tip material de clasă F, loc, centru de cost,
cod economic) a cărei stare o materializează motorul și ale cărei valori
se citesc din cub; PIF o pune în funcțiune, CAS o scoate, iar AMO, generat
lunar pe unitate internă, îi postează amortizarea (contabilă, fiscală,
deductibilă) din politică și regulile de deductibilitate. Regula:
[domeniu § Imobilizări](domeniu-si-operare.md#imobilizări) (87a, 87d, 87g, 095, 097).
Înrudit: unitate de sold, politică, reevaluare. În cod: `Imobilizare`, `AmortizareService`.

### golire valorică

Eliminarea reziduului valoric rămas pe un lot fără cantitate, prin
corecția de preț atribuită lotului (invarianți VI); nu e reevaluare,
închidere sau regularizare. Regula:
[limite § Domeniu și operare](limite-curente.md#domeniu-și-operare) (75a, D9-D3, 118).
Înrudit: lot, evaluare FIFO. În cod: `Postare.Atribuit`.

### reevaluare

Schimbarea valorii unei imobilizări, cu doar locul rezervat în registrul
imobilizărilor. Regula:
[limite § Domeniu și operare](limite-curente.md#domeniu-și-operare) (87).
Înrudit: imobilizare, golire valorică.

## Termeni tehnici

### cub

Singurul registru al faptelor operate: tranzacții și postări append-only,
complet rezolvate, scrise doar de motorul de operare în tranzacția comenzii,
partiționate pe spațiu (`Contabil`, `Stoc`; funcție de unitate, nu câmp);
migrațiile lui se scriu în SQL, peste `InitialCreate`, și nu transformă
date. Regula: [domeniu § Cubul persistat](domeniu-si-operare.md#cubul-persistat) (S-D1, S-D2, S-r4, D9-D2, 102b).
Înrudit: tranzacție, postare. În cod: `Module/Cub/`, `Spatiu`.

### tranzacție

Gruparea postărilor scrise împreună, de fel `Operare`, `Storno`,
`Transfer` sau `Deschidere`; un document operat are cel mult o `Operare`
și cel mult un `Transfer`, iar prima poartă explicația. Regula:
[domeniu § Felul mixt](domeniu-si-operare.md#felul-mixt-transfer-pe-linia-care-nu-schimbă-contul-t-d2-felia-32) (S-D1, T-D2).
Înrudit: cub, postare. În cod: `Tranzactie`.

### postare

Rândul cubului: coordonate, trei măsuri la scară apărată (`Cantitate`,
`ValoareValuta`, `Valoare`), ordinalul în tranzacție și perechea
mișcării; nu se rescrie, iar valoarea negativă e admisă doar ca linie
„în roșu”, storno sau transfer. Regula:
[domeniu § Entitățile și forma lor fizică](domeniu-si-operare.md#entitățile-și-forma-lor-fizică) (S-D1, S-D14, D9-A2, 117).
Înrudit: cub, coordonată. În cod: `Postare`.

### coordonată

Axa postării, cheia pe care se grupează și se identifică: cont, sens
(obligatoriu), dată, repartitor, contrapondere, produs, unitate, cod de
TVA, perioadă de declarare, partener fiscal, loc, valută, carte
(`Contabil` / `Fiscal`) și șase analize. Regula:
[domeniu § Nucleul pur](domeniu-si-operare.md#nucleul-pur-și-declarația-fluxului-pilot-bcs-plt-fct) (N-D2, N-D3, B-D8, 111, C111-D2).
Înrudit: postare, dimensiune, sens, repartitor, contrapondere. În cod:
`Coordonate`.

### explicație

Deciziile și ipotezele contractului acceptat, persistate pe prima
tranzacție scrisă: de ce a costat ieșirea atât, din ce sold citit, ce
partide a stins, ce rând de politică a decis; nu se rescrie. Regula:
[domeniu § Explicația deciziei](domeniu-si-operare.md#explicația-deciziei-x-d4-2026-10-03) (X-D4, D9-A8).
Endpoint-ul `api/politici/explica` (84e–h) e „explicarea configurației”:
configurația pe o linie ipotetică, fără persistare.
Înrudit: tranzacție, INV-CUB. În cod: `Tranzactie.Explicatie`.

### declarant

Funcția pură a tipului care transformă operandul (DTO-ul închis cu
documentul, liniile, politica și starea citită) în declarație: mișcări
debit ≠ credit, mutări pe același cont, transformări n→m, decizii,
ipoteze; frunza îl numește prin `Document.Declarant()`. Regula:
[domeniu § Declarația fluxului per tip](domeniu-si-operare.md#declarația-fluxului-per-tip-tr-d6b-felia-30) (B-D1…B-D10, D9-D5).
Înrudit: document, contract. În cod: `IDeclarant`, `Operand`.

### contract

Cuvânt folosit doar calificat: contractul feliei (D-urile pin-uite cu
owner-ul, în `docs/`), contractul laturilor (părțile permise pe fiecare
latură a tipului, T-D13) și rezultatul motorului, adică răspunsul
determinist al nucleului la o declarație: acceptat (cu tranzacțiile și
deciziile) sau refuzat, verificat de conservarea structurală (valoare per
carte, cantitate per produs, Σ = 0 la transfer) și materializat în cub la
operare. Regula:
[domeniu § Nucleul pur](domeniu-si-operare.md#nucleul-pur-și-declarația-fluxului-pilot-bcs-plt-fct) (N-D3, N-D4, N-D11, 118).
Înrudit: declarant, felie, latură. În cod: `Contract` e rezultatul
motorului; `Conservare`.

### frunză

Tipul concret al unei ierarhii TPH (`Document`, `DocumentDetaliu`,
`Repartitor`) față de rădăcina comună: o coloană de frunză se citește
doar pe o mulțime restrânsă la tipul ei sau prin `is ? :`, niciodată prin
`as`/cast; discriminatorul `ClrType` e etichetă, nu comutator. Regula:
[domeniu § Modelul comun](domeniu-si-operare.md#modelul-comun) (54c, 89a…h).
Înrudit: document, repartitor. În cod: `ClrType`, `RandDupaCheie`.

### politică

Tabela de date care parametrizează un mecanism din cod (contare, stoc,
TVA, conex, scadență, numerotare, validare, închidere, amortizare,
diferențe, mapări fiscale), editabilă fără release; nu inventează
comportament și nu descrie câmpuri sau ecrane. Regula:
[politici § Configurarea profilului](politici-si-fiscalitate.md#configurarea-profilului) (4, 81e, 84e–f).
Înrudit: seed, profil. În cod: `Politica`, `Motor/Potrivire.cs`.
În sistemul vechi: stocurile = semn aplicat pe cantitate per tip × filtru.

### seed

Inițializarea idempotentă, per profil, a politicilor și nomenclatoarelor
(cataloagele cu `Activ`, care se inactivează, nu se șterg): creează rândul
cu timbrul `DinSeed`, îl aliniază cât poartă timbrul, îl lasă neatins când
e manual și nu îl recreează după un `RefuzSeed`. Regula:
[politici § Proveniență și verificarea configurației](politici-si-fiscalitate.md#proveniență-și-verificarea-configurației) (81d, 83a, 84a, 104f).
Înrudit: profil, politică. În cod: `DinSeed`, `Nomenclator`.

### gardian

Verificarea care refuză înaintea scrierii: gardianul de editare la commit
pe orice ușă securizată, gardianul de perioadă sub blocaj, gardienii de
retragere la anulare și stornare; „gardul” e verificarea pe postările
contractului (analiza obligatorie, nivelul de contare, soldul
intermediar). Regula: [domeniu § Culegerea draftului](domeniu-si-operare.md#culegerea-draftului-104c) (42a, 104c, D9-D4).
Înrudit: ușă, refuz. În cod: `GardianEditare`, `Cub/GardAnaliza`.

### ușă

Calea unei scrieri: ușa securizată (ObjectSpace secured, cu gardian, a
culegerii și a CRUD-ului), ușa de sistem (ObjectSpace non-secured al
motorului și al uneltelor, fără drepturi) și ușa declarației (refuzurile
rezultatului motorului); „pe orice ușă” = XAF, WebApi, Import1C, ModelCheck.
Regula: [domeniu § Operarea](domeniu-si-operare.md#operarea) (42b, 55b, 104b).
Înrudit: gardian, refuz. În cod: `ComenziDocument.Sistem(os)`.

### nucleu

Motorul pur `Atlas.Conta.Nucleu` (BCL, zero pachete, teste de
proprietate): transformă declarația în rezultatul motorului, fără bază,
fără XAF și fără plan de conturi. Regula:
[domeniu § Nucleul pur](domeniu-si-operare.md#nucleul-pur-și-declarația-fluxului-pilot-bcs-plt-fct) (090, N-D1…N-D12).
Înrudit: declarant, contract, motor de operare. În cod:
`Atlas.Conta.Nucleu`, `Motor.Opereaza`.

### motor de operare

Codul din modul care scrie cubul și ia deciziile de domeniu, agnostic la
plan și la frunze: primește comportamentul specific prin declaranți și
politici, nu prin `is`/`switch`, și rulează fiecare comandă într-o
tranzacție explicită care începe cu blocajul scrierii
(`pg_advisory_xact_lock`, cheie constantă), deci comenzile sunt seriale
per bază. Numele `Motor` e purtat și de clasa statică a nucleului, și de
spațiul `Module.Motor` al scriitorului; defectul se rezolvă la atingere
(118). Regula:
[domeniu § Scrierea serială per bază](domeniu-si-operare.md#scrierea-serială-per-bază-x-d6-2026-10-03) (33d, S-D4, X-D6).
Înrudit: cub, nucleu, declarant. În cod: `MotorOperare`, `TranzactieComanda`.

### straturi

Cinci straturi cu dependențe într-un singur sens: L0 nucleul pur, L1
declarațiile, L2 coaja comenzii (primește ID-ul și dreptul operatorului,
refuză 404/403 înaintea domeniului, își deschide propriul context), L3
culegerea, L4 randarea (XAF Blazor culege, React citește; paginile React
de document sunt înghețate). Regula:
[api § Împărțirea responsabilităților](api-si-client.md#împărțirea-responsabilităților) (104a, 104b, 104d).
Înrudit: motor de operare, culegere. În cod: `Api/ComenziDocument`.

### culegere

Stratul L3, singura sursă a impliciților, recalculului și validării pe
Draft; controllerul XAF și `Api/*Apply` sunt adaptori care traduc
ecranul, respectiv PUT-ul agregatului (`WriteDto`), în aceleași apeluri,
iar `InainteDeSalvare` rulează înaintea gardianului. Regula:
[domeniu § Culegerea draftului](domeniu-si-operare.md#culegerea-draftului-104c) (104c, 104d, 109 f).
Înrudit: straturi, gardian. În cod: `CulegereDocument`, `WriteDto`.

### registru

Raportul cu nume legal proiectat din cub: registrul-jurnal, registrul
imobilizărilor; cubul e „cubul”, nu registru, iar tabelele de registru de
dinaintea tăierii nu mai există și numele lor sunt interzise (D9-D7 b).
Regula: [domeniu § Registre și ciclu de viață](domeniu-si-operare.md#registre-și-ciclu-de-viață) (D9-D2, D9-D6, 118).
Înrudit: cub, snapshot, sold.

### snapshot

Soldurile unei perioade de referință, scrise la închidere ca sumă între
snapshot-ul precedentei și rulajele lunii, pe cheia completă; nu e
registru și nu e urmă, se reconstruiește integral din cub, cheile zero se
omit. Regula: [domeniu § Soldurile materializate la închidere](domeniu-si-operare.md#soldurile-materializate-la-închidere) (F27-D3, 101).
Materializarea e scrierea rezultatului motorului în cub la operare
(`Cub/Materializare.cs`); snapshot-ul e soldul materializat la închiderea
perioadei de referință (88).
Înrudit: sold, perioadă fiscală, partidă deschisă. În cod: `SoldPerioadaContabil`.

### refuz

Răspunsul negativ al unei uși, în ordinea 401 → 400 → 404 (inexistent
sau invizibil) → 403 (vizibil fără drept) → 422 (domeniu), cu un singur
corp `EroriDto`; refuzul declarației poartă un cod stabil (`COD: mesaj`),
iar dry-run-ul (`Valideaza`) întoarce aceleași refuzuri fără commit.
Regula: [api § Securitate și răspunsuri](api-si-client.md#securitate-și-răspunsuri) (80a…e, S-D4).
Înrudit: ușă, gardian. În cod: `EroriDto`, `CoduriRefuz`.

### probă

Verificarea reproductibilă pe calea reală: ModelCheck pe bază pentru
model și motor, probe pe sursă fără bază (`--probe-sursa`), HTTP pe host
viu pentru securitate, browser pentru UI; „oracolul” (`NUC-*`) compara
postările cu o a doua sursă, care nu mai există. Regula:
[dezvoltare § Verificări proporționale cu modificarea](dezvoltare-si-validare.md#verificări-proporționale-cu-modificarea) (23, 66, 80i, D9-D7 a).
Înrudit: ModelCheck, scenariu. În cod: `nou/tools/ProbeHttp/`.

### ModelCheck

Unealta care verifică modelul XAF/EF și rulează scenele de domeniu
(fixture-uri create prin comenzi, asertate, purjate) pe PostgreSQL,
scriind în bazele ei; o singură rulare o dată, pe instanța nativă, iar
suita integrală pe ambele profiluri e gate-ul de commit. Regula:
[dezvoltare § Rulare reproductibilă](dezvoltare-si-validare.md#rulare-reproductibilă-scriptsverificaps1) (23, 84, 091 (e)).
Înrudit: probă, INV-CUB. În cod: `nou/tools/ModelCheck`, `MODELCHECK_BAZA_SUFIX`.

### INV-CUB

Invarianții interni ai cubului, rulați după fiecare scenă și înaintea
purjei ei (proveniența inverselor, echilibrul pe tranzacție și carte,
conservarea transferului, taxa postată = taxa liniei, unitățile de
partidă, perechea, ordinalul, explicația), fiecare ramură cu un mutant
într-o tranzacție anulată. Regula:
[domeniu § NIR delta și citirile comune](domeniu-si-operare.md#nir-delta-și-citirile-comune-098099-2026-09-24) (102b, 102d, X-D4);
[dezvoltare](dezvoltare-si-validare.md), antetul paginii.
Înrudit: cub, explicație. În cod: `Cub/Citiri/Invarianti`, `CITIRE_*`.

### scenariu

Rândul catalogului (`SC-<COD>-<NN>`, transversal `SC-X-<NN>`): așteptare
scrisă de mână din regula contabilă, cu cifre, probă ModelCheck și stare;
un tip e „pe cub” numai cu ciclul 1–8 verde pe ambele profiluri, iar
catalogul e proba supremă a motorului. Regula:
[dezvoltare § Verificări proporționale cu modificarea](dezvoltare-si-validare.md#verificări-proporționale-cu-modificarea) (091);
[catalogul](../nucleu/scenarii/README.md).
Înrudit: probă, ModelCheck. În cod: `docs/nucleu/scenarii/<COD>.md`.

### felie

Unitatea de livrare a unui pas mare, cu contract scris în `docs/` (D-uri
pin-uite cu owner-ul, regulă de oprire, review advers la închidere:
lectura independentă Codex cu observații numerotate, corectate și
reverificate înainte de merge), mersă în main prin PR; restanțele ei
(amânări cu nume și stare) intră în backlog, unde o restanță închisă își
schimbă starea, nu dispare. Regula:
[istoricul feliilor](../decizii/istoric-plan-de-lucru.md); [restanțele](../decizii/restante.md) (091 (k)).
Înrudit: contract, gate.

### gate

Suita integrală ModelCheck pe ambele profiluri, gate-ul de commit, și
numai ea: verificarea dreptului înaintea comenzii e refuz, condiția de
închidere a feliei e regula ei de oprire, iar Import1C nu mai e gate din
091. Regula:
[dezvoltare § Contracte generate și build](dezvoltare-si-validare.md#contracte-generate-și-build) (091 (e), 118).
Înrudit: ModelCheck, felie, refuz.

### bază de import

Baza în care Import1C operează prin motor documentele sursei 1C
(`Atlas.Conta.Import1C.Flax`, pe instanța nativă) și clonele ei cu sufix,
sursă de recensământ și de măsurare, nu gate; bazele de dezvoltare se
recreează, nu se repară. Regula:
[dezvoltare § Import operațional 1C](dezvoltare-si-validare.md#import-operațional-1c) (45a, 102b, 107, 116).
Înrudit: ModelCheck, deschidere. În cod: `nou/tools/Import1C`.

### SAF-T

Fișierul D406 (L lunar, S de stoc) proiectat din cub pentru profilul
privat: GL-ul, facturile și plățile din postări și fapte fiscale,
mișcările și pozițiile din postările pe lot, cu mapările de coduri ca
politici versionate; exportul cere citire necondiționată. Regula:
[politici § SAF-T](politici-si-fiscalitate.md#saf-t) (73c, 103, D8-B8, SAF-D4).
Înrudit: sold, fapt fiscal. În cod: `SaftProiectii.SaftPeCub`.

## Cum se citesc identificatorii

| Forma | Ce înseamnă | Exemplu | Unde e textul |
|---|---|---|---|
| `42b` | sub-punctul (b) al regulii durabile a deciziei 42 | `(104c)`, `(090 j)` | `docs/decizii/042-*.md`, §Regula durabilă |
| `42-r1` | restanța 1 a deciziei 42 | `107-r3` | `restante.md` → fișierul deciziei |
| `review advers F27, L4` / `1c` | observație a review-ului advers al feliei 27, cu eticheta ei | `(review advers F27, L3)` | decizia 88; schema etichetelor (L/cifră-literă) nu e descrisă nicăieri |
| `F27-D4`, `F27-r9` | D-ul 4 pin-uit, respectiv restanța 9, din contractul feliei 27 (decizia 88); F13 = 70, F22 = 80, F23 = 81, F26 = 87, F28 = 89 | `F26-r16`, `F28-A…K` (probe TPH) | `docs/api/p5-feliaNN-*-contract.md` |
| `DIM-4`, `C1a`, `D4-r1`, `NG-rN` | pasul 4 al feliei dimensiunilor (54); notă din `architecture-notes-2026-07-28.md`; restanță D394 (71); consultarea nod-și-găleată | `D4-r13` | antetul `restante.md` |
| `TR-D6a` … `TR-D9b` | feliile trecerii pe cub, în ordine: nucleul pur, declarația fluxului, strangler, tipurile rămase, imobilizări/DEC, citirile comune, tăierea, unitățile | `TR-D8` | `docs/nucleu/tr-d*-contract.md`; TR = trecerea pe cub |
| `N-D1`, `B-D1`, `S-D1`, `T-D1`, `X-D1`, `D9-D1` | D-ul pin-uit al contractului: N = TR-D6a, B = TR-D6b, S = TR-D7a, T = TR-D7b, X = gate-ul transversal TR-D8 (108), D9 = TR-D9a (110) | `T-D13`, `X-D6` | contractul feliei, §Deciziile |
| `N-r1`, `S-r1`, `T-r1`, `X-r1`, `D9-r1` | restanța contractului cu același prefix | `S-r10` | contractul feliei, §Restanțe; `restante.md` |
| `TR-r1`, `FZ-r1` | restanțele notelor `nucleu-transfer.md` și `nucleu-fizica.md` (nu ale feliilor TR-D) | `TR-r2` | `docs/nucleu/nucleu-transfer.md` §7, `nucleu-fizica.md` §6 |
| `D9-A1` | amendamentul 1 al contractului TR-D9a | `D9-A10` | `docs/nucleu/tr-d9-taierea-amendament-*.md` |
| `D8-B1`, `R6-B1`, `LDI-B1`, `DES-B1`, `ASM-B2`, `NIR-D1`, `SAF-D1`, `S2-D1`, `M1-D1`, `C111-D1` | pin-ul contractului feliei sau al tipului numit (citiri TR-D8, intervale TVA, LDI, deschidere, ASM, NIR delta, SAF-T și pașii lui, migrarea M1, coordonatele C111) | `D8-B4` | `docs/nucleu/*-contract.md`, `docs/import/m1-*-contract.md`; „-B” și „-D” sunt același lucru, un pin aprobat de owner, numerotat diferit de la TR-D8 |
| `S0` … `S3` | pașii feliei SAF-T din TR-D8 (validator, L, Payments, stocuri) | `S1-R8` | `docs/nucleu/tr-d8-saft-contract.md` |
| `X-RI1`, `D9-F-R1`, `C106-R1`, `C111-R1`, `M1-R1`, `109-R1`, `S1-R1` | observația 1 a review-ului Codex: RI = al implementării (TR-D8), F-R = al închiderii TR-D9a, R = al feliei, deciziei sau pasului numit; `RV1`, `RV2` = rundele de reverificare | `X-RI4`, `M1-R3a` | `docs/nucleu/*-review-codex.md`, `docs/import/m1-review-codex.md` |
| `C111-Q1` | tranșarea 1 a owner-ului pe contractul C111 | `C111-Q8` | `docs/nucleu/c111-coordonate-contract.md`, §C111-Q |
| `IZ-R1` | defectul 1 al rețetei de izolare a mutanților (110) | `IZ-R2` | `docs/decizii/110-*.md` |
| `C102`, `C104`, `C106`, `C111`, `M1` | felii numite după decizia lor (curățenie, straturi, regim pe stare, coordonate) și prima felie de migrare (107) | `C104-R2` | `docs/nucleu/c1NN-*.md`, `docs/import/m1-*.md`; C = feliile de consolidare de după 091 |
| `SC-BCS-01`, `SC-X-24`, `SC-CIT-73` | scenariul 01 al tipului BCS; transversal; al citirilor | `SC-NIR-37` | `docs/nucleu/scenarii/<COD>.md`, `CITIRI.md` |
| `INV-CUB`, `STR-*`, `NUC-*`, `D85-M1`, `104c-S1`, `RAMURA-NUL`, `GARD-ANALIZA`, `X-D5-PLAN`, `REGIM` | probe ModelCheck: invarianții cubului, structurale, ale nucleului, ale deciziei 85, ale culegerii, ramura de nul, gardul analizei, planul perf, catalogul regimului | `STR-SCHEMA-4` | dezvoltare, „Verificări proporționale cu modificarea” |
| `EXPLICATIE-LIPSA`, `PERECHE-RUPTA` | mutantul unei ramuri INV-CUB | `IMO-SUPORT` | dezvoltare, antetul paginii |
| `TIP_FARA_DECLARATIE`, `CITIRE_*`, `SCRIERE_OCUPATA` | cod stabil de refuz (`COD: mesaj`): al declarației, al invarianților, al blocajului | `REGULA_CONTARE_LIPSA` | `CoduriRefuz`; pagina mecanismului |
| `PR #22` | pull request-ul prin care felia a mers în main | `PR #35` | `docs/decizii/istoric-plan-de-lucru.md` |

<!-- gen:decizii -->
## Decizii

Deciziile care numesc pagina în `Arii:` (0; titlurile în [index](../decizii/README.md); generat de `docskit gen`, nu se editează): niciuna.
<!-- /gen:decizii -->
