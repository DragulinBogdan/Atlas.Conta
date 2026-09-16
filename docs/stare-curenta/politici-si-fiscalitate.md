# Politici și fiscalitate

**Actualizat: 2026-09-15.** [Index](README.md)

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
celor 19 tipuri cu proveniență, consumată și de raportul de profil și de
gate-urile de citire; permisiunile rolului se reaplică la fiecare seed. Toate
tabelele de politici au editor React; `Cont` rămâne doar citire pe OData. (81e, 81k, 84c, 84d)

| Configurare | Condiții impuse |
|---|---|
| Tip TVA | Cotă între 0 și 100; referințele pentru implicite trebuie să indice tipuri active (81e) |
| Regulă de contare / politică TVA | Sursa de cont explicită cere cont explicit (81e) |
| Regulă de contare | Filtrul de semn este `-1`, `1` sau absent; un filtru de tip material exclude completarea filtrului de natură (81e) |
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

Unicitățile politicilor se aplică rândurilor active, cu tratarea explicită a
axelor opționale. `Repartitor.Cod` nu este cheie unică globală. (81c, 81-r4)

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
import, 300604/300605, rd. 7 cu oglinda 22 pusă de proiecție); declarația
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
mai atinge; utilizatorul nu îl poate activa la creare; rândul șters logic
rămâne șters și se raportează; reseed-ul nu reactivează timbrul. (81d, 83a, 84a)

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
deși regula s-a potrivit (gardul de nivel minim al tipului, natura interzisă
de profilul de validare). Dreptul de citire se cere pe toate tipurile
configurabile și pe nomenclatoarele citite, înaintea calculului pe contextul
nesecurizat; o referință inexistentă sau invizibilă e refuzată înaintea
calculului. Panoul `/politici/explica` afișează răspunsul fără niciun calcul
în client și se deschide precompletat din grilele de politici. (84e–h)

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

Registrul TVA se materializează pe linii când există politica și tipul TVA
necesare, inclusiv pentru linii fiscale fără sumă TVA contabilizată. Cota,
regimul și valorile fiscale sunt fixate în registru. Etichetele care sunt
citite prin referințe nu constituie snapshot complet al nomenclatoarelor.
Gruparea ține separat sensul, tipul TVA și marcajul storno. Backfill-ul
folosește același generator ca operarea. (68)

### Perioada de declarare și rectificativa

Rândul fiscal poartă două coordonate de timp: `Data` este a faptului fiscal
(data documentului, iar pe rândul invers data stornării), `PerioadaAn` și
`PerioadaLuna` sunt perioada în care faptul se DECLARĂ. Jurnalele, decontul,
D300, D394 și SAF-T filtrează pe perioada de declarare. (F27-D5)

Regula de completare este una singură, la scrierea rândului: dacă perioada
datei faptului este deschisă, perioada de declarare este a ei. Dacă este
închisă sau nedefinită — nedefinită înseamnă închisă, ca la gardian — decide
politica tipului, prin câmpul `DeclarareIntarziata` de pe `PoliticaTva`:
`PerioadaInregistrarii` sau `PerioadaFaptului`. Seed-ul privat o pune pe
direcție: deductibilul (FCT, DEC, RLF, DVI) declară în perioada înregistrării,
colectatul (FCL, RDC) în perioada faptului. Profilul poate alege altfel;
motorul nu știe de ce. Profilul bugetar nu are rânduri `PoliticaTva`, deci
câmpul este inert acolo. (F27-D5, 29, 35d)

Rândul invers al unui storno se declară în perioada stornării, deschisă prin
gardian. Perioada de declarare este snapshot pe rând, ca regimul și cota: o
politică schimbată ulterior nu rescrie rândurile deja scrise. (JT-D5, JT-D3)

Rectificativa nu este un marcaj cules, ci un derivat din două momente:
conținutul de rectificativă al unei perioade este mulțimea rândurilor
declarate în ea și scrise (`ScrisLa`) după `InchisaPrimaOara` a ei. O perioadă
niciodată închisă nu are reper, deci nu are rectificativă. Redeschiderea nu
stinge `InchisaPrimaOara`, deci reperul rămâne cel al primei declarații.
D300 și D394 pe exact o lună calendaristică raportează `Rectificativa` și
diferențele față de declarat, pe cheia decontului. (F27-D5, F27-D1)

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
lui, câștigă per categorie și fel rândul cu `DeLa` maxim; plafonul se aplică
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

Raportarea folosește registrele. Debitarea și creditarea sunt proiectate pe
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

D300 proiectează registrul TVA prin `MapareD300` către rândurile de operații.
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
amânarea plății; SAF-T D406 emite codurile de import din același registru,
fără filtru de tip. (86c, 86k)

D394 grupează document × storno × partener × sens × tip TVA × cotă.
Clasificarea partenerului folosește aceeași funcție fiscală ca implicitele.
CUI-ul este normalizat pentru agregare; partenerii multipli cu același CUI
pot forma aceeași poziție. Înregistrarea TVA are prioritate în grup. (71b, 71d, 81b)

`MapareD394` permite operațiile de vânzare L/V/LS și de cumpărare A/C/AS pe
sensul corespunzător. AI este derivată și nu se configurează ca mapare.
Numărul de facturi distinge documentul și storno-ul. Totalurile pe sens se
reconciliază cu registrul prin operațiile incluse și `Neincluse`. Ștergerea
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
în proiecțiile fiscale. (73b, 73c)

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

În L, partenerul unei note rezultă din rolul contului și dimensiunile
materializate. TVA este asociată pe linie și storno. Retururile/stornările
folosesc tipul și semnul fiscal corespunzător. Pentru facturile de stoc,
legătura contabilă poate proveni din NIR-ul materializat. Societatea apare
în rolurile de client/furnizor cerute de reprezentarea celeilalte laturi. (73e)

Datele neincluse rămân explicite și participă la reconcilierea notelor,
facturilor, TVA, soldurilor și nomenclatoarelor. Lipsa unui câmp necesar nu
este mascată printr-o valoare inventată. (73e)

În S, politica mișcării se aplică pe tip document × tip stoc × semn.
Absența politicii diferă de excluderea deliberată prin cod absent.
Storno-ul păstrează codul mișcării inițiale și inversează valorile.
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
