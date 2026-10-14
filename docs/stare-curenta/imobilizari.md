# Imobilizări

**Actualizat: 2026-10-15.** [Index](README.md)

Fișa imobilizării, punerea în funcțiune, amortizarea contabilă și fiscală și scoaterea din funcțiune.

## Imobilizări

Fișa `Imobilizare` este nomenclator subțire: număr de inventar unic,
denumire, tip material de clasă F (contul implicit furnizează prima
nominalizare), clasificare opțională din catalog, loc (repartitorul notelor),
centru de cost, responsabil, cod economic (dimensiunea bugetară a
cheltuielii) și starea materializată de motor: nouă, în funcțiune, ieșită,
cu datele punerii în funcțiune și ieșirii. Metoda, durata, valoarea
reziduală, categoria fiscală și valoarea nu sunt pe fișă: sunt fapte datate
în registru, scrise de documente. Parametrii curenți sunt proiecția
ultimului eveniment. (87a)

Gardianul fișei: tipul material este de clasă de imobilizări și se schimbă
doar cât fișa este nouă; locul și codul economic cât este nouă sau în
funcțiune; nimic pe fișa ieșită; ștergerea doar pe fișa nouă, fără postări
care o au ca unitate și fără linii de documente care o poartă, chiar în Draft;
starea și datele le scrie doar motorul. Transferul este schimbarea locului pe fișă, fără
document: următoarea amortizare postează pe noul loc, istoricul locului
este pe rândurile lunare. (87a, 87h)

Situația fișei, AMO/CAS și API Imo citesc aceeași intrare
`Cub/Citiri/Imobilizari`: sume pe fișă și carte, parametri din evenimentele
vii identificate prin cauză. Amortizarea fiscală postează în Carte=Fiscal;
deductibilul și lunile rămân atribute istorice ale liniei AMO. Invarianții
refuză fișa fără cauză, fără suport ori fără originea inversei. (095, 097,
102d)

Conturile nominalizate ale activului și amortizării se citesc pe set din
cub, prin aceeași intrare pentru generatorul AMO, CAS și declarant. Politica
furnizează numai prima nominalizare a fiecărui cont; cheltuielile rămân din
politică. PIF fără linie FCT consumă exclusiv deschidere/NTC operată.
Poziția fără fișă e cheia cont × repartitor × produs × valută × analiză;
partenerul nu intră în ea, deci achizițiile de la furnizori diferiți pe
același loc sunt o singură poziție (C111-Q12). Furnizorul se regăsește prin
referința la suport (`SuportId`), care duce la postarea și la documentul de
achiziție; postările PIF nu-l poartă. Poziția nu poate deveni negativă
(`POZITIE_FARA_FISA_NEGATIVA`),
nici la o dată viitoare, la operare, anulare, storno/corecție sau deschidere.
Protecția include conturile istorice după schimbarea politicii și se
serializează cu PIF. Documentele fără suport/fișă nu iau blocajul IMO.
Stornoul PIF eliberează suportul de la data lui; nu permite anularea
sursei dacă aceasta ar șterge suportul unui interval istoric. (098b/c)

Aritmetica este exclusiv în `AmortizareService`, ca funcție pură aplicată
de trei ori pe lună. Cota liniară este valoarea de amortizat împărțită la
lunile rămase, rotunjită la bani, fixată la ultimul eveniment; suma lunară
este minimul dintre cotă și rest; rotunjirea în jos poate lăsa o lună
suplimentară pentru rest (F26-D7/F27-D4). Baza
„la ultimul eveniment" este situația la sfârșitul lunii evenimentului:
luna evenimentului postează cota veche, parametrii noi curg din luna
următoare, indiferent de zi. Accelerata amortizează 50 % din brut în
primele 12 luni de la punere, apoi liniar; degresiva AD1 aplică coeficientul
pe ani (durata trebuie să fie multiplu de 12) și trece la liniar. Fișa este
eligibilă în luna M dacă a fost pusă în funcțiune înaintea primei zile, nu a
ieșit până la ultima zi, are cel puțin un eveniment de registru până la
ultima zi și mai are rest contabil sau fiscal; linia cu contabil zero și
fiscal pozitiv rămâne fără conturi. (87g)

Lunile pe care le acoperă amortizarea lunii M sunt cele DATORATE minus cele
ACOPERITE: datorate = lunile întregi de la luna de după punerea în funcțiune
până la M inclusiv (luna punerii nu se amortizează), acoperite = suma
coloanei de luni a rândurilor de amortizare până la sfârșitul lui M.
Diferența nulă sau negativă scoate fișa din lună. Diferența mai mare decât
unu este o RECUPERARE: fișa a intrat în evidență după luna punerii în
funcțiune, fiindcă fișa se postează la data înregistrării. Cota lunii este
atunci suma cotelor celor n luni, calculată iterativ — pragurile
degresivului și ale acceleratului avansează la fiecare pas, iar restul scade
după fiecare — nu cota lunii înmulțită cu n; recuperarea nu depășește restul
de amortizat și nici durata rămasă. Deductibilul se calculează pe SUMA
lunii, cu regulile valabile la sfârșitul ei: plafonul lunar este al lunii de
declarare și se aplică o singură dată pe suma recuperată, nu o dată pe
fiecare lună recuperată. Linia amortizării și rândul de registru poartă
lunile acoperite; ele intră în cheia anti-stale a operării și se inversează
la storno ca oricare altă coloană. Fișa fără niciun eveniment până la
sfârșitul lunii precedente, dar cu rânduri în M, este punerea în funcțiune
înregistrată întârziat: baza și parametrii i se citesc la sfârșitul lui M.
Gardianul lunii precedente lipsă rămâne neatins: se recuperează doar ce
n-a avut cum să fie amortizat, nu ce n-a fost amortizat. (F27-D4)

Iterația celor n luni pleacă de la situația fișei la ultimul eveniment, nu de
la situația fiecărei luni recuperate: o fișă reevaluată sau modernizată în
intervalul recuperat primește pe toate cele n luni cota de DUPĂ eveniment,
inclusiv pe lunile dinaintea lui. Aproximare declarată — recuperarea e rară,
iar un eveniment în interiorul ei și mai rar. (review advers F27, L5)

Generarea lunară urmează ordinea gardienilor: fișă eligibilă fără politică,
amortizare vie în lună, amortizare vie ulterioară, draft anterior, lună
precedentă lipsă, perioadă închisă, nicio fișă. Motivul se raportează la
previzualizare și refuză comanda. Operarea cere data ultimei zile a lunii
și aceeași unitate internă pe ambele laturi, apoi recalculează și refuză
dacă mulțimea liniilor diferă pe fișă, cele trei cifre, conturi, loc, centru
de cost sau cod economic; același criteriu dă `Stale` pe API. Anularea sau
stornarea unei amortizări este refuzată dacă există o amortizare operată
ulterioară sau fapte ulterioare ale fișelor ei (ieșire, modernizare,
revizuire); aceeași regulă refuză anularea sau stornarea unei puneri în
funcțiune, iar anularea sau stornarea unei ieșiri este refuzată cât există
amortizare operată pentru luna ieșirii sau una ulterioară. (87e, 87f, 87g)

Stornoul oricărui document de imobilizări se datează în luna documentului:
rândul invers datat în altă lună ar lăsa situația fișelor falsă între cele
două date. O fișă apare o singură dată pe o punere în funcțiune; duratele
revizuite depășesc lunile deja amortizate. (87e, 87g)

## Amortizarea imobilizărilor

AMO este documentul generat lunar pe unitate internă, pe tiparul ITV;
mecanica generării, eligibilitatea fișelor, formula și gardienii sunt
descrise în [imobilizari](imobilizari.md#imobilizări). Aici
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

<!-- gen:decizii -->
## Decizii

Deciziile care numesc pagina în `Arii:` (5; titlurile în [index](../decizii/README.md); generat de `docskit gen`, nu se editează): 87 (amendată de 97), 95, 97 (amendată de 98), 98, 102.
<!-- /gen:decizii -->
