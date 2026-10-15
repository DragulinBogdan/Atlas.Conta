# CVL — corecția valorii lotului fără schimbarea cantității

- Tip: catalog
- Actualizat: 2026-10-15

Catalogul fixează așteptările înainte de implementare, conform
[contractului TR-D9b-CV](../tr-d9b-corectii-valorice-contract.md).
Starea execuției este în secțiunea de validare. `CVL`
este abrevierea mecanismului, cu producător propus **FCT cu linie explicită
de corecție**, nu tip nou de document. Proba este `ScenariiCvl`, prin
comenzile documentului. [Matricea pe postări](../tr-d9b-corectii-valorice-producator-si-compensare.md)
fixează cauzele, atribuirile și inversele așteptate.
Un refuz așteptat este rezultat al scenariului, nu stare de implementare.

## Convenții și fixture

RON, fără TVA, BUC. Privat: stoc 302, consum 602, furnizor 401. Bugetar:
302.01.00, 602.01.00, 401.01.00, cu analizele cerute de profil. Conturile
sunt constante ale probelor și date ale politicii, nu literali în motor.
`q/v` = cantitate/valoare în MAG1 pe cheia de stoc; `C` = valoarea netă
pe consum la LOC; `D` = datoria totală față de furnizor, pozitivă.
Valoarea de pe consum nu se adună cu stocul pentru evaluarea ieșirii.

Fixture standard independent per rând: FCT la 05.01 cu 100 × 10 = 1.000,
NIR conex operat, BCS la 10.01 dacă este cerut, CVL la 15.01.
Storno la 20.01, ieșire ulterioară la 18.01; scenariile cu altă ordine
precizează datele. Anul se rezervă după verificarea disponibilității la
implementare; nu se reutilizează anul pachetului D9B. Date pe marcaj,
comenzi în ObjectSpace-uri noi, curățenie în `finally`.

Fiecare scenariu verifică postările exacte, cauzele, atribuirile,
inversările, perechile, cantitate zero la corecție, absența faptelor TVA,
explicația și cititorii de produs. Amprenta originalelor include liniile
și postările. Dinaintea corecției până după ea, FCT/BCS și prețul de
intrare rămân neschimbate. Nu se folosesc registre vechi ca oracol.

## Operare și evaluare

| ID | Pași și așteptări numerice | Rezultat |
|---|---|---|
| SC-CVL-01 | Lot intact 100/1.000; dry-run +200: zero efecte. Operare +200: 100/1.200, C=0, D=1.200. BCS nou 25: valoare 300, rest 75/900; apoi 75: valoare 900, rest 0/0. | acceptat |
| SC-CVL-02 | BCS 60/600; CVL +200: delta stoc80/consum120, sold40/480, C=720, D=1.200. BCS vechi rămâne600. BCS nou10:120, rest30/360; ultimul30:360, rest0/0, C=1.200. | acceptat |
| SC-CVL-03 | BCS100/1.000; CVL+200: stoc0/0, delta consum200, C=1.200, D=1.200; nicio valoare nouă pe MAG1. | acceptat |
| SC-CVL-04 | Trei fixture-uri, CVL−200: intact100/800,C0; parțial după60:40/320,C480; epuizat:0/0,C800. D=800 în toate. BCS vechi rămâne600 sau1.000; diminuarea păstrează D/C cu valori negative (S-D14). Pe cazul parțial, storno BCS compensează−120 spre stoc și lasă100/800,C0,D800; storno CVL după aceea lasă100/1.000,C0,D1.000. Ordinea inversă are compensare0 și același final. | acceptat |
| SC-CVL-05 | Lot100/1.000, consum60; CVL−1.000:40/0,C0,D0. Ieșire40: valoare0, sold0/0. Separat CVL−1.000,01: refuz, sold40/400,C600,D1.000 neschimbate. | acceptat / refuz valoric |
| SC-CVL-06 | Lot3/1; BCS1 la10.01 =0,33, BCS1 la11.01 =0,34, rest1/0,33. CVL+0,02: Hamilton pe grupuri dă stoc0,01/consum0,01; în consum primul BCS ia0,01, al doilea0. Rest1/0,34, C0,68, D1,02. Ultima ieșire=0,34, rest0/0. La egalitate de dată se verifică aceeași ordine prin ordinale persistate, cu ID-uri în ordine opusă. | acceptat |
| SC-CVL-07 | Lot100/1.000, BCS20 la10.01 și40 la11.01; CVL+200: stoc80, atribuiri consum40/80, sold40/480,C720,D1.200. Separat două loturi intacte100/1.000 și10/150, CVL pe linii +200/−30:100/1.200 și10/120, D1.320. O linie invalidă anulează întreaga comandă. | acceptat / atomicitate |

## Retragere, storno și ordinea compensării

| ID | Pași și așteptări numerice | Rezultat |
|---|---|---|
| SC-CVL-08 | Caz parțial, CVL+200, fără mișcare ulterioară; retragere CVL:40/400,C600,D1.000, CVL Draft, zero efecte/origine proprie. Reoperare:40/480,C720,D1.200 o singură dată. | acceptat |
| SC-CVL-09 | Intact/parțial/epuizat, CVL+200 apoi storno CVL, fără dependenți:100/1.000,C0;40/400,C600;0/0,C1.000. D1.000 în toate; inversele exacte−200, respectiv−80/−120; originalele rămân. Storno repetat: refuz, fără efecte. | acceptat / refuz stare |
| SC-CVL-10 / SC-X-05 | BCS60/600 → CVL+200 → storno BCS: inversa60/600 și compensarea120 spre stoc;100/1.200,C0,D1.200. Partida CVL rămâne−200. | acceptat |
| SC-CVL-11 / SC-X-06 | BCS60/600 → CVL+200 → storno CVL → storno BCS: inversa BCS600, compensare0;100/1.000,C0,D1.000. | acceptat |
| SC-CVL-12 / SC-X-07 | BCS60/600 → CVL+200 → CVL+100:40/520,C780,D1.300. Storno BCS:600+180 spre stoc;100/1.300,C0. Storno CVL+200 după compensare:100/1.100,C0,D1.100; storno CVL+100:100/1.000,C0,D1.000. Separat, dacă +200 e stornat înaintea BCS, BCS primește doar600+60, final100/1.100,C0,D1.100. | acceptat, fără dublă compensare |
| SC-CVL-13 | BCS60/600 → CVL+200 → BCS nou20/240:20/240,C960,D1.200. Retragere/storno CVL: refuz dependință, totul neschimbat. Storno BCS nou: exact20/240, compensare0, rest40/480,C720. Apoi storno CVL:40/400,C600,D1.000. | refuz urmat de recuperare |
| SC-CVL-14 | Caz parțial cu CVL activ: retragere BCS refuzată, fără modificări. Lot intact cu CVL activ: retragere/storno sursă refuzate din dependența valorică; NIR conex este retras înaintea acestei probe ca să nu mascheze gardul. Nicio referință CVL nu devine orfană. | refuz dependință |

## Perioade și corecția documentului

| ID | Pași și așteptări numerice | Rezultat |
|---|---|---|
| SC-CVL-15 | BCS60 în ianuarie, închide luna:31.01=40/400,C600,D1.000. CVL+200 la05.02:40/480,C720,D1.200; rulaj februarie stoc+80/consum+120/datorie+200. 31.01 și snapshot-ul rămân intacte. | acceptat |
| SC-CVL-16 | BCS60 și CVL+200 în ianuarie, închide luna:31.01=40/480,C720,D1.200. Retragere CVL sau storno datat în ianuarie: refuz. Storno CVL05.02:40/400,C600,D1.000; lectura31.01 rămâne40/480,C720,D1.200. Separat storno BCS05.02 cu CVL activ:100/1.200,C0,D1.200, ianuarie neschimbat. | refuz / acceptat în luna deschisă |
| SC-CVL-17 | Cazul ianuarie din16; `Corecteaza` CVL la05.02: inversă+draft legat, situație40/400,C600,D1.000. Draft modificat la+150 și operat:40/460,C690,D1.150; februarie net stoc−20/consum−30/datorie−50. Ianuarie rămâne40/480,C720,D1.200. | acceptat |

## Refuzuri, citire și concurență

| ID | Pași și așteptări numerice | Rezultat |
|---|---|---|
| SC-CVL-18 | Lot lipsă, fără recepție activă, lot repetat, furnizor/gestiune greșit, cantitate introdusă nenulă, delta0, precizie peste2 zecimale, politică lipsă, atribuire străină/lipsă: refuz pe ușa potrivită; zero postări/tranzacții/origini/compensări noi. | refuz structură/proveniență |
| SC-CVL-19 | Lot cu BTR/retur/ASM/LDI sau reziduu preexistent: refuz eligibilitate. CVL înainte de recepție ori antedatată față de BCS operat: refuz cronologie. Ieșire nouă antedatată înainte de CVL activ: refuz. Consum beneficiar dintr-un an anterior anului corecției: refuz distinct de cazul lunii închise; stocul rămas dintr-o recepție a anului anterior se corectează. | refuz de perimetru |
| SC-CVL-20 | Pe02 înainte/după CVL: fișa și balanța arată stoc400→480, consum600→720, datorie1.000→1.200; explicația arată ponderi40/60 și cote80/120. Citirea istorică și cea cu snapshot coincid cu suma faptelor; reconstrucția snapshot-ului nu reexecută documentele. | acceptat |
| SC-CVL-21 | Lot intact100/1.000; două sesiuni CVL+200 / BCS60. Dacă CVL câștigă: BCS720, compensare atribuită BCS0. Dacă BCS câștigă: BCS600, ajustare separată120. În ambele:40/480,C720,D1.200. Se probează ambele ordini, în aceeași zi. | serializare |
| SC-CVL-22 | Lot100/1.000, BCS60; CVL+200 / storno BCS concurente: CVL întâi → compensare120; storno întâi → CVL200 pe stoc intact. Ambele lasă100/1.200,C0,D1.200, fără efect dublu. | serializare |
| SC-CVL-23 | CVL+200 / închidere ianuarie: dacă CVL intră prima, snapshot40/480,C720,D1.200; dacă închiderea câștigă, CVL refuzată și snapshot40/400,C600,D1.000. Eșec injectat după pregătirea compensării din10: BCS încă Operat,40/480,C720,D1.200, zero efecte noi. | serializare / rollback |
| SC-CVL-24 | CVL+200, stingere nominalizată50: rest CVL−150, FCT−1.000. Storno CVL refuzat până la neutralizarea stingerii; după aceasta rest−200 și storno CVL→0. Separat CVL−200: FCT−1.000/CVL+200; compensare NTC200→FCT−800/CVL0. | partide și dependenți |

## Producător și proveniență — propunerea revizuită

| ID | Pași și așteptări numerice | Rezultat |
|---|---|---|
| SC-CVL-25 | FCT nouă cu fel explicit de corecție pe lotul existent (linia corectată este linia de intrare a lotului), q0/v+200: în cazul parțial40/480,C720,D1.200. FCT inițială rămâne Operat, propria partidă−1.000; FCT corectivă are−200; `CorecteazaId` nu înlocuiește legătura comercială. Zero loturi și NIR noi; prețul intrării10. Retragere/reoperare fără dependenți nu dublează recepția sau datoria. Linia obișnuită FCT cu q0 rămâne refuzată; linia corectivă cu q nenulă, cu tip de TVA sau alături de o linie obișnuită este refuzată. Factura corectivă nu generează NIR conex. | acceptat / refuz structură |
| SC-CVL-26 | Matricea revizuită a1…a4, s1/s2, k1/k2, i1…i4, j1/j2 se asertă în10/11/12 și04, cu **apartenența la perechi, fără ordinalele numerice din tabel**. K este o a doua Operare a CVL, D602−120/C302−120, cu cauza CVL și linia ei; J este inversa ei exactă în Storno CVL, fără cauze străine. a2/a4 au Contrapondere.Tert chiar cu q0. Două corecții au compensările120/60 în tranzacții distincte, câte una pe fiecare CVL; inversarea numai+200 lasă100/1.100,C0,D1.100. Proba pură respinge proveniență străină, cicluri, compensare/inversă duplicată și alocare cu țintă alocare; nu o reclasifică drept compensare. | acceptat / refuz proveniență |
| SC-CVL-27 | A15.01, storno A25.01, apoi storno B datat20.01: CV_CRONOLOGIE; la22.01 rămâne40/480,C720,D1.200, după25.01 rămâne40/400,C600,D1.000. Separat storno B14.01 după A15.01: același refuz. Oglindă: storno B25.01, apoi storno A20.01: refuz, la22.01 rămâne40/480,C720, după25.01 rămâne100/1.200,C0. În aceeași zi25.01, A inversată apoi B: acceptat cu compensare0, final100/1.000,C0,D1.000. La refuz, zero postări/tranzacții/explicații noi și stările documentelor neschimbate. | refuz cronologie / acceptat serializat |
| SC-CVL-28 | După storno B: tranzacția Storno B și o Operare suplimentară pe CVL, B Stornat, CVL Operat, q100/v1.200,C0,D1.200 prin Loturi, Contabil și snapshot. Postările cu cauza B se închid la zero pe fiecare cont; cele cu cauza CVL poartă integral200 pe stoc. K are explicație proprie pe CVL (rădăcină, beneficiar inversat, sumă120), fără ieșire fictivăq0; toți invarianții cubului persistat trec. Cu două CVL active, comanda scrie 1 + 2 tranzacții. După storno A: J oglindește K ne-Storno; q100/v1.000,C0,D1.000; nicio postare cu cauză străină. Fișa și jurnalul arată K și J sub numărul CVL. Retragerea CVL cu compensare activă: refuz, fără efecte. Exportul SAF-T rămâne pe refuz (SC-CVL-32). | țintă de integrare |
| SC-CVL-29 | Storno B / storno A concurente, două conexiuni reale, aceeași zi: B întâi → compensare120 pe A, apoi storno A inversează și K; A întâi → compensare0. Ambele ordini lasă100/1.000,C0,D1.000, fără compensare orfană sau dublă. Separat, două CVL active și eșec injectat după scrierea primei compensări: rollback integral, B încă Operat,40/520,C780,D1.300, zero tranzacții noi pe B, A sau A′. | serializare / rollback |
| SC-CVL-30 | Storno recepției, cu NIR conex retras înainte. Lot intact, CVL+200 activ (100/1.200,D1.200): storno FCT → `CV_DEPENDENTI`, fără efecte. Storno CVL la20.01 (100/1.000,D1.000), apoi storno FCT la21.01: acceptat; Storno FCT conține exact cele două inverse ale recepției, fără alocare și fără inversa ei; lot0/0,D0, partidele FCT și CVL0. Parțial: BCS60, CVL+200, storno CVL (40/400,C600), storno BCS (100/1.000,C0), storno FCT:0/0,C0,D0. Parțial cu CVL activ după storno BCS compensat (100/1.200,C0): storno FCT → `CV_DEPENDENTI`, cu200 activi pe stoc (80 + 120 relocați). Storno CVL la25.01, apoi storno FCT datat20.01: `CV_CRONOLOGIE`. După CVL stornat: retragerea FCT refuzată de gardul loturilor folosite, retragerea BCS refuzată cu `CV_DEPENDENTI`, storno BCS acceptat cu compensare0. | refuz dependință / acceptat fără compensare |
| SC-CVL-31 | Explicația (CV-D8). Parțial, CVL+200: Operare CVL are două `AlocareValorica` în ordinea repartizării — stoc, pondere40, valoare80, spre recepție; consum, pondere60, valoare120, spre postarea BCS; Σ ponderi100, Σ valori200 = valoarea liniei; sold citit stoc40/400. După storno BCS: a doua Operare CVL are explicație proprie cu o `CompensareValorica` (rădăcină, beneficiar,602→302,120), fără ieșire cuq0. Storno CVL expune explicațiile ambelor Operare. Pe fixture-ul din SC-CVL-06, al doilea bon are decizie cu pondere1 și valoare0, fără postare. DTO-ul HTTP poartă listele de alocări și compensări. Proba pură a invariantului respinge: postare valorică fără decizie, decizie fără postare, Σ valori diferită de linie, cotă care nu rezultă din ponderi, beneficiar sau rădăcină greșită, compensare după inversa rădăcinii. | acceptat / refuz audit |
| SC-CVL-32 | SAF-T pe refuz (CV-D9). Parțial, CVL+200 la15.01, storno BCS la05.02, storno CVL la05.03. D406 ianuarie: `SAFT_PROVENIENTA_AMBIGUA` pe linia CVL (contrapartide302 și602); pe lotul intact: `SAFT_SURSA_INCOMPLETA` (cantitate comercială0). D406 februarie, numai compensarea: fără refuz pe documentele scenariului; compensarea apare în GL sub jurnalul FCT și lipsește din PurchaseInvoices. D406 martie: refuz pe evenimentul de storno CVL. Declarația de stocuri, în fiecare dintre cele trei luni: `SAFT_MISCARE_VALORICA`. La refuz nu se emite fișier; nicio linie cu cantitate1 inventată. | refuz explicit (restanța CV-r1) |

SC-CVL-12/26 se parametrizează cu toate cele șase ordini ale stornărilor
B/A/A′ din anexă. Varianta cu semne opuse (+200/−100) pornește de la
40/440,C660,D1.100; storno B compensează120 și−60 în tranzacții distincte,
rezultând100/1.100,C0,D1.100. Storno +200 lasă100/900,C0,D900;
storno −100 revine la100/1.000,C0,D1.000. Celelalte ordini păstrează
separarea rădăcinilor și același final; corecțiile opuse nu se anulează
prin agregare înainte de stabilirea provenienței.

## Validare și review

`-Suita Scenarii -Tip CVL` rulează pe ambele profiluri din 2026-10-14
(pasul 3 al contractului). Comenzile, regula de oprire și rezultatele sunt
în contract.

| Stare | Scenarii |
|---|---|
| rulate | SC-CVL-01…32 |

SC-CVL-19 rulează pe bază (pasul 5): transferul, asamblarea (lotul consumat
și lotul produs), lipsa la inventar și cronologia pe ambele profiluri;
descărcarea de gestiune, returul la furnizor, returul de la client și NIR-ul
cu diferență de cantitate pe privat, unde există politicile lor. Vânzarea
către terț, recepția stornată și anul anterior sunt probate în nucleu.
Reziduul preexistent nu are formă proprie: NIR-ul cu diferență și reziduul
lăsat de retur la golire (SC-RLF-05) sunt postări pe lot în afara
predicatului, deci același refuz. Pe lotul corectat activ, returul la
furnizor (100 bucăți) și cel de la client dau `CV_DEPENDENTI` și lasă
100/1.200; după stornarea corecției, returul a 20 trece la prețul lotului:
80/800.

SC-CVL-18 și 25 rulează și pe ușile culegerii (pasul 4): linia culeasă prin
`FacturaIntrareApply` și prin evenimentele ecranului XAF (`CulegereDocument`),
operată, dă aceleași cifre ca linia scrisă direct; comutarea felului golește
sau reface linia; lotul propriu nefinalizat al liniei comutate dispare; API-ul
refuză amestecul de câmpuri ale celor două feluri, iar gardianul de scară,
valoarea cu trei zecimale. Furnizorul sau gestiunea alt decât ale recepției dau
`CV_LOT_NEELIGIBIL`. Politica lipsă trece prin același `Contari.Rezolva` ca
recepția, probat de SC-FCT-16 pe operand; pe bază nu se scoate o regulă din
seed. Descărcarea de gestiune există numai pe privat.

SC-CVL-20 citește cazul parțial cu ianuarie închis și corecția în februarie:
balanța și fișa pe stoc, consum și datorie, cu citirea din mișcări
(`CitireCumul.Vizibila`) și cu cea pornită din snapshot (`Integrala`), egale
cu suma faptelor. Datoria se citește pe un partener propriu cazului. Pe fișa
furnizorului corecția are câte un credit pe alocare (80 și 120), nu un
singur rând de 200. Reconstrucția snapshot-ului găsește zero diferențe și
nu scrie tranzacții.

SC-CVL-28 citește pe fixture-ul SAF-T (corecția în ianuarie, storno bon la
05.02, storno corecție la 05.03, februarie închisă): jurnalul pune K și J sub
numărul corecției, nu al bonului; fișa stocului pe februarie are inversa
bonului sub numărul lui și K sub numărul corecției, cu sold 1.200; stocul
lotului la 28.02 e 100/1.200 din mișcări, din snapshot și în rândul persistat
al lunii. Bonul stornat își păstrează valoarea 600 pe linie.

SC-CVL-32 exportă ianuarie, februarie și martie pe privat; pe bugetar SAF-T
nu se aplică, iar proba asertă că nu există proiecție. Factura corecției
refuzate rămâne în proiecție cu antetul, fără nicio linie, și nu se scrie
niciun fișier. Februarie, cu numai compensarea, nu are refuz pe documentele
cazului; luna are însă refuzuri pe celelalte corecții ale scenei.

Concurența (SC-CVL-21, 22, 23, 29) rulează pe două conexiuni reale, în
ambele ordini: scena ține blocajul scrierii, iar comenzile intră în coada
lui (tiparul SC-X-15). În SC-CVL-23, cazul în care închiderea câștigă
folosește februarie, fiindcă ianuarie se închide în cazul celălalt.
Snapshot-ul de stoc se citește pe lot; cel contabil nu poartă lotul și
partida, deci se compară cu suma faptelor pe repartitorii scenei, iar C și D
se citesc din cub la sfârșitul lunii. Eșecul injectat vine din bază, printr-un
trigger temporar pe inserarea tranzacției de compensare, fără cârlig în cod.

Proba pură `CorectieValoricaTeste` acoperă matricea, cele 16 ordini de
stornare, cazurile temporale, storno recepției și refuzurile, fără bază.
Tot ea respinge cei 12 mutanți ai invariantului de audit din SC-CVL-26 și 31,
fiecare pe motivul lui. Pe bază, `INV-CUB` ucide cinci mutanți
`VALORICA-*`: decizie lipsă, ponderi schimbate, valoarea liniei, valoarea
compensării și atribuirea străină. [Review-ul R1](../tr-d9b-corectii-valorice-review-r1.md)
confirmă cifrele și FCT; review-ul originii
partidei nu certifică acest catalog. SC-CVL-29…32 vin din a doua rundă
(2026-10-13): apartenența compensării (decizia 119), storno recepției, explicația și
exportul SAF-T lăsat pe refuz. Delimitarea este închisă la 2026-10-14 (R8,
CV-D10), cu perimetrul aprobat de owner. Flax nu este oracolul cifrelor.
