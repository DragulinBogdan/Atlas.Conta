# Politici și fiscalitate

**Actualizat: 2026-10-08.** [Index](README.md)

Aceste reguli descriu comportamentul implementat. Acoperirea fiscală este
delimitată în [limite curente](limite-curente.md).

## Configurarea profilului

Profilul și rotunjirea sunt fixe per bază de date. Politicile sunt date care
parametrizează mecanisme scrise în cod; nu introduc tipuri de documente,
câmpuri, formule contabile executabile sau ecrane dinamice. (4, 51c, 52a)

`TipDocument` este ancora dintre codul documentului și clasa CLR, în relație
unu-la-unu. Crearea și ștergerea sunt refuzate, iar `Cod` și `ClrType` sunt
controlate de server. Implicitul TVA al ancorei este editabil. (20, 81e)

Politicile sunt editabile prin OData în limita permisiunilor și a gărzilor
de domeniu. Rolul `Configurator` (seed-uit, și pe RELEASE) citește tot și
scrie doar tipurile din `Politici.TipuriConfigurabile` — lista explicită a
celor 22 de tipuri cu proveniență, consumată și de raportul de profil și de
gate-urile de citire; permisiunile rolului se reaplică la fiecare seed. Toate
tabelele de politici au editor React; `Cont` rămâne doar citire pe OData. (81e, 81k, 84c, 84d)

| Configurare | Condiții impuse |
|---|---|
| Tip TVA | Cotă între 0 și 100; referințele pentru implicite trebuie să indice tipuri active (81e) |
| Regulă de contare / politică TVA | Sursa de cont explicită cere cont explicit (81e) |
| Regulă de contare | Filtrul de semn este `-1`, `1` sau absent; un filtru de tip material exclude completarea filtrului de natură (81e). Filtrul doar potrivește: regula nu poartă și nu schimbă semnul valorii, care e al liniei (113) |
| Regulă de stoc | Semnul este `-1` sau `1` (81e) |
| Orice politică sau nomenclator cu proveniență | Fiecare câmp enum poartă un membru definit; enum-urile fără membru 0 nu acceptă valoarea implicită scrisă prin omisiune (84i) |
| Tip TVA | Ștergerea unui tip referit (implicite, ancoră, parteneri, produse, linii de document, registrul TVA) se refuză, ca dezactivarea (84j) |
| Numerotare | Serie nevidă, următorul număr cel puțin 1; formatul opțional folosește argumentele număr și serie (81e) |
| Scadență | Număr de zile nenegativ (81e) |
| Închidere TVA | Cele patru conturi sunt toate completate sau toate absente (81e) |
| Mapare D300 | Destinații de operații; fără mapare simultană la un rând și un strămoș al său (69b, 81e) |
| Mapare D394 | Operația și sensul trebuie să formeze o combinație permisă (71c, 81e) |
| Politică de amortizare | Un rând per tip material de clasă de imobilizări (natura verificată la commit): contul de amortizare, cheltuiala cu amortizarea și cheltuiala la cedare; fișa fără politică blochează generarea lunară și ieșirea (87d) |
| Regulă de deductibilitate | Categoria fiscală, felul (plafon lunar sau procent), valoarea, valabilitatea de la o dată cu temei; aplicată doar utilizării neexclusive când e marcată așa (87d) |
| Închidere de perioadă | Un singur rând per fel de constatare; gardianul refuză al doilea înaintea indexului unic (F27-D2) |

Unicitățile politicilor se aplică rândurilor active, cu tratarea explicită a
axelor opționale. `Repartitor.Cod` nu este cheie unică globală, deși toate
familiile stau pe aceeași tabelă: datele importate au coduri legitim
repetate între familii. (81c, 81-r4, F28-r2)

## Implicitele TVA

Implicitul ajută la culegerea unei linii noi. Nu rescrie documentele deja
culese și nu anulează ștergerea intenționată a unei valori dintr-o linie
existentă. Implicitele de sesiune aparțin clientului; cele de domeniu sunt
rezolvate pe server. (56, 81a)

`ImpliciteService` primește tipul documentului, partenerul, produsul și data.
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

Seed-ul privat configurează FCL/RDC pentru UE și extra-UE cu SDD, iar
FCT/RLF pentru UE cu TI21. Cazurile fără politică folosesc regulile de
fallback de mai sus. Aceste valori descriu seed-ul, nu o acoperire completă
a tuturor situațiilor fiscale. Profilul bugetar nu primește aceste rânduri. (81b, 81-r1)

`GET /api/implicite/tip-tva` întoarce valoarea, sursa și motivul rezolvării.
Datele partenerului și produsului sunt citite securizat. Un obiect invizibil
nu devine sursă de implicit și nu este deosebit public de unul inexistent.
Un cod de tip document necunoscut este cerere invalidă. (80a, 81f, 81-r6)

Dezactivarea unui tip TVA folosit ca implicit este refuzată cu lista
referințelor. Tipurile inactive rămân valabile pentru istoricul documentelor;
selectoarele React pentru culegere nouă oferă tipurile active. Filtrarea
echivalentă a selectoarelor XAF este o limită curentă. (81c, 81e, 81-r7)

## Proveniență și verificarea configurației

`DinSeed` marchează proveniența pe entitățile care implementează
`ICuProvenienta`. Seed-ul îl setează la creare și ALINIAZĂ la fiecare trecere
rândurile care îl poartă (timbrul e proprietate); o modificare efectivă prin
ObjectSpace securizat îl stinge, după care rândul e al clientului și nu se
mai atinge; utilizatorul nu îl poate activa la creare; reseed-ul nu
reactivează timbrul. (81d, 83a, 84a)

Rândul `ICuProvenienta` șters pe ușa securizată se șterge fizic și lasă un
`RefuzSeed` (tip + cheia din indexurile unice, serializată; zecimalele fără
scară, deci `21.0000` citit din bază și `21` din seed dau aceeași cheie). Seed-ul nu
recreează un rând refuzat și îl numără la „șterse”; golurile de mapare
D300/D394 citesc tot refuzurile. Refuzul se șterge de rolul `Configurator`,
iar rândul revine la următorul re-seed. Raportul de profil nu mai are
categoria „referință spre un rând șters”: FK-ul refuză ștergerea rândului
referit (valoarea 2 a lui `FelConstatare` e retrasă). Avertismentul D394
pentru partenerul scos din uz e `PartenerInactiv`. (83a, 83j, 104f, 104i)

Marcajul nu este permisiune, jurnal de audit sau criteriu pentru corecții
automate. Rândurile istorice marcate prin backfill nu permit reconstituirea
modificărilor manuale anterioare. Un PATCH fără modificări efective poate
răspunde cu succes fără a constitui o scriere. (81d, 81-r3, 81-r5)

Verificarea profilului citește configurația completă după verificarea
dreptului de citire asupra tuturor tipurilor implicate, inclusiv partener și
produs. Lipsa unui drept produce 403, nu un raport aparent complet din date
filtrate. Raportul semnalează referințe șterse, implicite inactive, ancore
lipsă, goluri de mapare și rânduri manuale. Listele de rânduri manuale sunt
limitate la 200 per tabel. Verificarea nu repară datele. (81g)

## Explicarea configurației

`GET api/politici/explica` explică CONFIGURAȚIA pe o linie ipotetică (tip de
document, tip de material, semn, dată, laturi, partener, produs), fără
document: pentru fiecare mecanism — contare, stoc per latură, TVA, conex,
implicitul de TVA, validare, scadență, numerotare — rândul câștigător,
nivelul potrivirii, candidații eliminați cu motivul lor, proveniența
fiecărui rând și o concluzie formată pe server. Potrivirea e aceeași funcție
pură pe care o consumă motorul (`Motor/Potrivire.cs`). Blocul de contare
declară postarea explicită (NTC, DEC) și rezervele — ce ar refuza operarea
peste potrivire: gardul de nivel minim al tipului, natura interzisă de
profilul de validare și, pe un tip care contează prin reguli, linia fără nicio
regulă (`REGULA_CONTARE_LIPSA`, 112e). Dreptul de citire se cere pe toate tipurile
configurabile și pe nomenclatoarele citite, înaintea calculului pe contextul
nesecurizat; o referință inexistentă sau invizibilă e refuzată înaintea
calculului. Panoul `/politici/explica` afișează răspunsul fără niciun calcul
în client și se deschide precompletat din grilele de politici. (84e–h)

## TVA la operare

`TipTva` descrie cota și regimul; politicile stabilesc sensul și conturile.
TVA cules explicit este păstrat la operare și validat, nu înlocuit automat
cu rezultatul unei recalculări. Recalcularea din culegere și validarea la
operare au responsabilități distincte. Totalul brut participă la stingere. (36a, 36b, 56)

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

Taxarea inversă pe sens deductibil generează autolichidarea. Pe sens
colectat, TVA trebuie să fie zero și nu se generează notă TVA; o valoare
nenulă este refuzată înainte de materializare, inclusiv la salvarea REST. (70a, 70b)

Linia declarației vamale poartă doar tipuri `DeImport` cu cotă (`IMP` cu cotă
0 este refuzat la operare); taxa culeasă se păstrează, iar zero se completează
din cotă la operare. (86d)

Cubul păstrează pe fiecare fapt fiscal regimul, cota, importul, valorile cu
semn, identitatea documentului fiscal și reperele temporale. Intrarea comună
`Cub.Citiri.Fiscale` citește ambele cărți; contul și cartea păstrează
proveniența, fără să despartă baza fiscală DVI de taxa contabilă. Un fapt
are cel mult o Bază și o Taxă; achiziția cu taxare inversă adaugă o
Autocolectare pe contrapartea colectată, cu același sens Achiziție.
Nomenclatorul curent oferă etichete, fără să recalculeze cota istorică. (103)

Faptele fiscale stau numai pe postări. Nu există snapshot fiscal cumulativ și
nici compatibilizare a istoricului de dezvoltare. (102, 103, D9-D2)

### Intervale și diagnostic TVA

`TipTva.ValabilDeLa` / `ValabilPanaLa` sunt opționale și incluzive;
intervalul inversat se refuză. Implicitul din culegere folosește
exigibilitatea (fallback data documentului), exclude candidații din afara
intervalului cu motiv în `Explica` și păstrează filtrele de acces/Activ.
Intervalele seed sunt cele din R6-B8, aliniate numai pe `DinSeed`. (103h/i)

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

`TipMaterial.RegularizareAvans` și baza negativă identifică regularizarea
FCT/FCL. `LinieAvansId` nominalizează linia pozitivă din altă factură,
operată, marcată, a aceluiași partener/sens. Sursa oferă calificarea și
reperul pentru diagnostic, fără să schimbe perioada sau taxa regularizării.
Sursa invalidă, lipsă ori compensată produce avertisment; FK-ul nu se
redirecționează la corecție. Retururile și reducerile fără sursă fiscală
verificabilă primesc avertisment de proveniență în locul verdictului de
interval. Contract: `docs/nucleu/tr-d8-tva-intervale-contract.md`. (103h/i)

### Perioada de declarare și corecțiile

Data documentului, exigibilitatea, primirea la achiziție și înregistrarea
sunt distincte. Primirea se propune din înregistrare, rămâne editabilă în
draft și se îngheață la operare. D394 folosește primirea/emisia; D300,
perioada înregistrării la achiziție și cea a exigibilității la livrare.
O livrare omisă dintr-o perioadă D300 deja declarată intră în regularizarea
curentă. Nu există parametru `DeclarareIntarziata` în politica TVA. (103)

Depunerea se confirmă explicit pe formular, an, lună și versiune exportată.
Versiunea emisă în DTO-ul D300/D394 este SHA-256 peste faptele perioadei din
citirea comună, formular și perioadă. Raportul și amprenta folosesc același
snapshot tranzacțional. Confirmarea recalculează amprenta sub blocaj exclusiv;
o versiune depășită primește `DEPUNERE_VERSIUNE_DEPASITA`. UI exportă JSON-ul
cu amprentă și păstrează versiunea acelui export pentru confirmare, fără
etichetă liberă. Aceasta verifică faptele cubului, nu parametrii manuali sau
fișierul XML ANAF.
`DepunereDeclaratie` păstrează momentul și utilizatorul, fără sume fiscale
paralele; nu se editează prin CRUD generic. Aceeași confirmare este
idempotentă. Confirmarea și scrierile fiscale se serializează tranzacțional.
Închiderea sau redeschiderea contabilă nu confirmă și nu șterge depunerea.
D394 compară `ScrisLa` cu ultima confirmare; D300 folosește regularizări și
nu expune automat o rectificativă. Confirmarea din aplicație este evidența
depunerii efectuate de utilizator, nu transmitere către ANAF. (103d)

`CK_Postare_FiscalComplet` impune la scriere calificările și reperele
obligatorii când `TipTvaId` este prezent, inclusiv primirea la achiziții.
Nu există scanare de istoric la pornire. Invarianții între roluri și
proveniența inversei rămân verificați în ModelCheck.

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
refuzată cât timp versiunea înlocuitoare nu este operată.

## Închiderea de perioadă

Severitatea constatărilor de CONȚINUT ale închiderii de perioadă este dată, nu
cod: un rând per fel în politica de închidere de perioadă, cu trei valori.
`Blocant` refuză închiderea și nu se poate accepta; `Avertisment` o lasă să
treacă numai cu acceptare explicită pe constatarea concretă, scrisă în
istoricul lunii; `Ignorat` nu emite constatarea deloc. Un fel fără rând se
comportă ca avertisment și o SPUNE în textul constatării, ca o configurație pe
jumătate să nu treacă tăcut. (F27-D2)

Cele patru feluri și seed-ul lor:

| Fel | Privat | Bugetar |
|---|---|---|
| Închiderea de TVA lipsește sau nu e operată | Blocant | Ignorat |
| Amortizarea lunară lipsește sau nu e operată | Avertisment | Avertisment |
| Document în lucru cu data înregistrării în perioadă | Avertisment | Avertisment |
| Document operat cu rest scadent în perioadă | Ignorat | Ignorat |

Diferența dintre profiluri este de CONȚINUT, nu de mecanism: bugetarul nu e
plătitor de TVA, deci închiderea de TVA îi este tip inert, iar un blocant pe ea
n-ar fi avut niciodată cum să se stingă. Constatarea de închidere de TVA nu se
emite deloc când profilul n-are politica de conturi completă sau când luna nu
are sold pe cele două conturi de TVA. (F27-D2)

Blocantele STRUCTURALE ale lanțului nu sunt în această politică și nu se
configurează: ele sunt regula lanțului. (F27-D2)

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

## Amortizarea imobilizărilor

AMO este documentul generat lunar pe unitate internă, pe tiparul ITV;
mecanica generării, eligibilitatea fișelor, formula și gardienii sunt
descrise în [domeniu și operare](domeniu-si-operare.md#imobilizări). Aici
stau datele care o parametrizează. (87d, 87g)

`PoliticaAmortizare` dă, per tip material de clasă F, contul de amortizare,
contul cheltuielii cu amortizarea și contul cheltuielii la cedare. Seed-ul
privat acoperă tipurile 205, 208, 212, 2131–2133 și 214 cu perechile lor
28x și 6811/6583; terenurile nu au rând. Seed-ul bugetar folosește frunzele
280.08.01, 280.08.09, 281.03.01, 281.03.03 cu 681.01.00 și 691.00.00.
Niciun simbol de cont nu stă în cod. (87d)

`RegulaDeductibilitate` exprimă legea ca date cu valabilitate: categorie
fiscală, marcaj „doar neexclusiv", fel (plafon lunar sau procent), valoare,
`DeLa`/`PanaLa`, temei. La fiecare rând lunar, din regulile valabile la data
lui, câștigă per categorie și fel rândul cu `DeLa` maxim (aceasta e și cheia
unică: categorie, fel, `DeLa`); plafonul se aplică
înaintea procentului; fără regulă, deductibilul este egal cu fiscalul.
Seed-ul privat: plafon 1 500 lei/lună pe vehiculele de persoane cu cel mult
9 locuri neexclusive (din 2012-02-01), sediul social în locuință 0 % din
2024-01-01 și 50 % din 2026-01-01. Bugetarul nu are rânduri. O regulă nouă
sau modificată nu rescrie lunile deja postate. (87d)

O cerință fiscală nouă care încape într-un rând nou cu `DeLa` este seed; un
fel nou de regulă este mecanism nou în cod, mic și numit. (87d)

`ClasificareImobilizari` este catalogul HG 2139/2004, seed-uit din CSV
(590 de poziții cu cod; sub-variantele fără cod sunt sărite). Banda de ani a
poziției dă intervalul duratei fiscale: la punere în funcțiune și la
revizuire, o durată fiscală în afara benzii se refuză când fișa are
clasificare; fără clasificare nu se verifică. Perechea de conturi nu este în
catalog: tipul material îl alege utilizatorul. (87d)

## Proiecțiile contabile

Citirile contabile portate folosesc cubul, cu `Carte=Contabil`. Debitarea și creditarea sunt proiectate pe
laturi; perioada și dimensiunile sunt parametri ai raportului, diferiți de
filtrele de afișare ale grilei. (42c, 66)

Balanța plată agregă mișcările și calculează soldul conform modului cerut.
Soldurile nete nu sunt aditive; totalurile nu se obțin însumând orbește
solduri deja netate. Balanța pe plan însumează valorile brute către părinți,
apoi netează fiecare nod. Totalul rădăcinilor corespunde balanței plate;
adâncimea limitează afișarea. Dimensiunile analitice nu formează alt arbore. (66, 67)

Fișa de cont calculează soldul progresiv în SQL. Căile SQL aplică explicit
ștergerea logică și verificarea echivalenței de securitate; dacă verificarea
nu poate garanta accesul necesar, raportul este refuzat. Etichetele folosesc
join-uri care păstrează faptele contabile când un nomenclator lipsește. (66)

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

## Societate, nomenclatoare și ANAF

`Societate` este configurație singleton editabilă. Seed-ul creează forma
goală și nu suprascrie datele completate. Adresa este structurată, ca la
partener; județul se aplică adreselor din România. Județele și unitățile de
măsură standard sunt nomenclatoare de bază, expuse pentru citire. (72b, 73a, 73b)

Produsul poate avea cod NC de opt cifre și unitate standard. Referința la
unitatea standard coexistă cu textul UM legacy. Normalizarea este explicită
și nu ghicește corespondențe. Rolul și funcția contului sunt date utilizate
în proiecțiile fiscale. (73b, 73c) Unitatea produsului (standard și text) se
poate completa oricând, dar nu se mai schimbă după ce produsul are postări
în cub sau apare pe o linie operată: cantitățile istorice au sensul unității
de la operare. Pentru altă unitate se creează alt produs. (S1-R5)

Sincronizarea ANAF folosește clientul PlatitorTva v9 injectat de host.
Candidații au CUI normalizabil la 2–10 cifre; persoanele fizice/CNP și
partenerii străini nu intră în acest flux. (72c)

Datele canonice privind înregistrarea TVA sunt actualizate din ANAF.
Denumirea și componentele adresei completează golurile; diferențele față de
valori deja culese sunt prezentate. Suprascrierea cere opțiune explicită și
păstrează valorile vechi/noi în jurnal. Un rezultat negăsit nu primește
ștampilă de sincronizare. Importul nu suprascrie clasificarea fiscală
canonică a unui partener deja sincronizat. (72d, 72g)

Endpoint-ul de lot acceptă până la 500 de identificatori și raportează și
pozițiile omise. Clientul extern împarte cererile în loturi de cel mult 100,
cu limitare de ritm în interiorul cererii. Erorile tranzitorii sunt 503,
iar refuzurile de domeniu 422. (72c, 72e)

## SAF-T

SAF-T L și S folosesc profilul privat; profilul bugetar este neaplicabil și
este refuzat cu 422. Exportul este lunar. Sumarul JSON expune agregate,
avertismente și reconcilieri; XML-ul este scris prin streaming. (73c, 73g)

`TaxInformation` citește `Cub.Citiri.Fiscale`. `MapareTvaSaft` selectează
codurile după versiunea exportului, secțiune, tip, regim, cotă istorică,
import, sens și rol. Lipsa mapării produce diagnostic; autocolectarea are
cod distinct numai în GeneralLedger. Politica are editor React și proveniență.
Restul `SourceDocuments` și sursele contabile/stoc ale exportului complet
rămân inventariate separat în TR-D8; această felie nu certifică migrarea sau
securitatea întregului SAF-T. (103, D8-B8)

În L, partenerul unei note rezultă din rolul contului și dimensiunile
materializate. TVA este asociată pe linie și storno. Retururile/stornările
folosesc tipul și semnul fiscal corespunzător. Pentru facturile de stoc,
legătura contabilă poate proveni din NIR-ul materializat. Societatea apare
în rolurile de client/furnizor cerute de reprezentarea celeilalte laturi. (73e)

**L pe cub (TR-D8 S1 + S2).** `SaftProiectii.SaftPeCub` produce
nomenclatoarele, GL-ul, facturile și plățile din `Citiri.Contabil.Jurnal`,
`Citiri.Fiscale` și `Citiri.Plati`, fără registre. Ruta publică L îl
folosește (R1).
Tranzacția GL este tranzacția cubului, inclusiv deschiderea din lună.
Linia GL este postarea, iar totalul D/C este rulajul balanței.
`TransactionDate` și `GLPostingDate` sunt data contabilă; `SystemEntryDate`
este data UTC a `ScrisLa`. Factura este evenimentul cubului
(`Operare`/`Storno`) și poartă `TransactionID`-ul tranzacției GL. Tipul:
storno 381, reemisă după corecție 384, total comercial negativ 381, altfel
380. Luna închisă se reexportă identic după o corecție ulterioară (B').
Brutul vine din postările de terț, iar netul, taxa și autocolectarea din
faptele fiscale ale liniei. Brutul trebuie să fie Σ(net + taxă −
autocolectare); cantitatea comercială vine din linia operată.
Ambiguitatea, sursa lipsă, maparea lipsă și corecția cu înlocuitor Draft
sunt refuzuri (`SaftDto.Refuzuri`), iar XML-ul nu se scrie. Citirea cere
RepeatableRead și refuză o tranzacție ambiantă mai slabă. Regulile complete:
[S1-R](../nucleu/tr-d8-saft-contract.md#s1-r--review-și-tranșări-2026-09-28).

Plata este evenimentul cubului (`Operare`/`Storno`) al unui document de
trezorerie, cu `TransactionID`-ul tranzacției GL. Liniile ei sunt postările
de pe latura contrapartidei (opusă laturii pe care `Document.Laturi()` admite
numai contul propriu), grupate
pe cont, partener și țintă. Ținta se află la capătul lunii operării: partida
străină nominalizată la operare, perechile `Transfer` de pe partida proprie
datate până atunci (legătură, desfacere, notă care stinge avansul) și restul.
`SourceDocumentID` este numărul documentului-origine al țintei. Restul și
partida inițială nu au referință. Stornoul neagă liniile operării. Legătura
de după luna plății nu apare în nicio declarație. Viramentul intern rămâne
numai în GL. Plata către partener fără cont cu `RolTert` intră în
`Neincluse`. Plata către angajat poartă codul societății. Valuta și faptele
fiscale pe plată refuză exportul. Compensarea prin notă nu este plată
(SAFT-r1). Regulile:
[S2](../nucleu/tr-d8-saft-contract.md#s2--payments-și-comutarea-l-contract-pentru-aprobare).

Reverificarea Codex din 2026-09-29 închide R1 la nivelul probei de domeniu:
unitatea definită e protejată de regula produsului de mai sus.
Stabilitatea octet cu octet a reexportului e probată cu metadatele și data generării fixate;
redenumirile din nomenclator (descrieri) rămân etichete curente.
[Review S1](../nucleu/tr-d8-saft-s1-review-codex.md).

Factura în valută se declară în RON, cu avertismentul `FacturaInValuta`
(cubul nu poartă valuta, B-r6). `ConturiDiferite` numără conturile al căror
Closing − Opening din GeneralLedgerAccounts diferă de rulajul net al liniilor
GL emise; `Neincluse` pe terți poartă numele repartitorului.

**Ruta pe registre este scoasă (SAF-B8, 2026-09-30).** `SaftProiectii.Saft`
și `SaftStocuri` nu mai există. Diferențele lor față de cub sunt clasificate
o singură dată, cu martor numeric, în [raportul A/B final](../nucleu/tr-d8-saft-ab.md);
probele D16/D17 rulează pe cub.

Datele neincluse rămân explicite și participă la reconcilierea notelor,
facturilor, TVA, soldurilor și nomenclatoarelor. Lipsa unui câmp necesar nu
este mascată printr-o valoare inventată. (73e)

**S pe cub (TR-D8 S3).** `SaftProiectii.SaftStocuriPeCub` produce
PhysicalStock și MovementOfGoods din postările pe lot (`Citiri.Loturi`), fără
registre. Categoria stocului vine din contul istoric al postării
(`Cont.CategorieStoc`, moștenită de la părinte; privat: 30/34/38 Magazie,
37 Mărfuri, 6 și 711 Consum). Magazie/Mărfuri au poziții. Consumul nu e stoc,
iar liniile lui cer politică, de regulă excludere. Folosință, Custodie,
Gratuit și producția în curs sunt neacoperite. Contul fără categorie pe tot
lanțul refuză fișierul. Mișcarea este evenimentul cubului
(`Operare`/`Transfer`/`Storno`) × cod. Linia agregă postările pe
(linie, lot, cont, gestiune). Codul vine din politica pe tip × categorie ×
semn, cu semnul originii pe storno. NIR delta este 10 cu cantitatea semnată
(S3-R1). ASM raportează producția la valoarea finală a cubului (ΣC, S3-R2).
Linia numai valorică este refuz (SAFT-r2). Poziția este lot × cont × produs
× gestiune; `Opening` include deschiderea din lună. Cheile: WarehouseID =
codul gestiunii, `StockAccountNo` = lotul, `MovementReference` lizibilă sau
rezerva `{TranzactieId:N}{cod}`. Pe o categorie raportabilă, politica nu poate
exclude. Regulile:
[S3](../nucleu/tr-d8-saft-contract.md#s3--movementofgoods-physicalstock-și-comutarea-c-contract-pentru-aprobare).
Stocul fizic se grupează pe repartitor și lot, pentru tipurile eligibile;
mișcările pe document × storno × cod, cu cantități și valori semnate. (74a, 74c)

Identificatorii emiși trebuie să fie unici și să respecte lungimile
acceptate. Data postării nu se emite când este în afara perioadei acceptate.
Profilul XML de stoc păstrează secțiunile L goale și cere secțiunea de stoc
fizic; o reprezentare invalidă este refuzată înainte de scriere. (74b, 74c, 74e)

Reconcilierile S urmăresc registrul față de ceea ce se emite, completitudinea,
soldurile pe cont și integritatea referințelor. Diferențele sunt raportate.
Validarea DUK verifică fișierul în profilurile testate; nu extinde acoperirea
funcțională declarată în [limitele curente](limite-curente.md). (74d, 74e)

O secțiune `GeneralLedgerEntries`, `SalesInvoices`, `PurchaseInvoices` sau
`Payments` fără intrări se scrie goală, fără `NumberOfEntries` și totaluri,
pe ambele module: validatorul respinge totalurile zero, iar schema cere
secțiunea prezentă. (74e, S0-R5)
