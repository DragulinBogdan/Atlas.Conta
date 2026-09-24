# Imobilizări complete pe cub înaintea TR-D8

- Data: 2026-09-24
- Stare: contract de implementare; direcția aprobată prin 095,
  IMO-B2 varianta A aprobată explicit de owner prin 097.
- Surse: 087(a–h), 088, 090(c/f/i/k), 091, 094, 095, 097;
  `nucleu-coordonate-rapoarte.md` §3–5; invarianții I–IV.
- Catalog numeric: `scenarii/IMO.md`. Recensământ Flax: zero PIF/AMO/CAS.

## IMO-B1 — Domeniu și identitate

Se acoperă Intrare, Modernizare și Revizuire pe PIF; AMO contabilă și
fiscală, inclusiv recuperarea lunilor; CAS pentru Casare/Vânzare/Lipsă;
anulare, inversare, dependențe, document întârziat și corecție în limitele
temporale existente. Reevaluarea rezervată, D101 și SAF-T Assets nu sunt
funcții noi ale acestei felii. Registrele rămân în scriere duală până la D9.

Fișa își păstrează identitatea din nomenclator. Cubul persistă explicit
FelUnitate=Fisa: nu se mai deduce Partida pentru orice postare contabilă cu
unitate. Citirea este pe fișă × carte × cont; brutul și amortizarea cumulată
stau pe conturi distincte. Pe fiecare capăt, Cont al unității coincide cu
Cont al postării. Identitatea fișei leagă pozițiile pe 21x și 28x; nu se
folosește cheia document/cont/partener a partidelor. Loturile și partidele
își păstrează forma și identitățile. Cantitate=ValoareValuta=0 la IMO.

## IMO-B2 — PIF și valoarea contabilă preexistentă: A aprobată (097)

087(e) spune că PIF nu postează contabil. FCT de imobilizare postează deja
D 2131 / C 404. PIF trebuie să nominalizeze acea valoare pe fișă, fără a o
dubla. Contractul vechi permite însă și PIF fără linie sursă, cu brut și
amortizare inițială, fără verificarea existenței lor în contabilitate.

Contraexemplu: bază goală, PIF fără sursă cu brut 1.200 și cumulat 200.
Fișa veche arată net 1.000; balanța are 0 pe 2131 și 2813. O simplă mutare
pe fișă lasă +1.200 și −1.200 pe 2131, respectiv +200 și −200 pe 2813,
iar citirea generală rămâne zero. Conservarea singură nu detectează lipsa
suportului contabil.

**A — aprobată: PIF nominalizează valoare contabilă existentă.**

- Intrarea/modernizarea din FCT mută debitul disponibil al contului de
  activ, de pe poziția fără fișă pe fișa aleasă, în tranzacție Transfer.
  Exemplu: FCT 1.200 → D 2131 fără fișă −1.200 și D 2131/fișă +1.200.
  Balanța și rulajele contabile rămân exact cele ale FCT.
- Fără linie FCT, suportul poate fi deschidere sau notă contabilă operată.
  Se verifică disponibilul nenominalizat al contului și dimensiunilor,
  la data înregistrării, separat pentru brut și amortizare inițială.
  Cumulat inițial 200: C 2813 fără fișă −200 și C 2813/fișă +200.
  Lipsa suportului refuză atomic; nu se fabrică un sold negativ anonim.
- Dimensiunile și proveniența suportului sunt preluate din postările
  alocate; consumul a două fișe nu poate depăși suportul comun. Pentru
  FCT se păstrează și plafonul pe linie. Două sesiuni concurente sunt probă.
- Deschiderea generică poate furniza soldurile contabile; nominalizarea
  se face o singură dată. Fișele deja nominalizate nu se nominalizează din nou.
- Suportul FCT/NTC/deschidere consumat capătă dependență: nu poate fi anulat
  ori inversat cât timp PIF-ul aferent rămâne viu. Un PIF inversat eliberează
  exact alocarea sa. Registrele de audit ale alocării nu devin solduri.

Aceasta restrânge intrarea fără suport permisă de 087, cu aprobarea
owner-ului consemnată în 097. În scenariile vechi, fixture-ul primește mai întâi
suport contabil real; nu se ocolește gardianul pentru a păstra probele.

**B — alternativă neadoptată: PIF poate crea și valoare contabilă nouă.**
Intrarea fără suport emite o notă cu contrapartidă explicită din politică,
distinctă de nominalizarea unei achiziții existente. Cere semantica sursei
(aport, producție proprie, donație, deschidere etc.), culegere și politici
suplimentare. Un cont implicit unic ar ascunde natura operației. Aceasta
extinde comportamentul produsului; nu este implementare dedusă din 095.

**Fiscala:** brutul fiscal se naște pe fișă, nu se copiază din soldul
contabil când valorile diferă. Forma aprobată în Carte=Fiscal:
D cont activ/fișă 900 și C același cont/fără fișă 900; pentru amortizarea
inițială, D cont amortizare/fără fișă 150 și C cont amortizare/fișă 150.
Ambele perechi sunt Operare, fără CodTva/PerioadaDeclarare. Conturile vin
din fișă/politica amortizării. Contraponderea fără fișă echilibrează cartea,
nu intră în situația activelor; nu este sold contabil și nu este partidă.
Această formă este inclusă în alegerea A aprobată; aprobarea nu atestă implementarea.

## IMO-B3 — Evenimente și calcul

Intrare și Modernizare nominalizează brutul contabil și postează brutul
fiscal potrivit IMO-B2. Revizuirea fără valoare păstrează o postare zero pe
fișă, cauzată de linia PIF: evenimentul rămâne vizibil chiar fără sumă.
Parametrii (metode, durate, reziduale, categorie, utilizare) se citesc din
liniile evenimentelor vii, prin Cauza. Se păstrează coalesce înapoi și
efectul parametrilor noi din luna următoare evenimentului.

AmortizareService rămâne singura aritmetică: liniară, accelerată, degresivă
AD1, restul de rotunjire, recuperarea lunilor și gardienii anti-stale.
La liniară se păstrează F26-D7/F27-D4: cota rotunjită în jos poate lăsa
o lună suplimentară (100/3 → 33,33 × 3 + 0,01). SC-IMO-09 a fost corectat
înaintea validării finale: varianta inițială schimba neintenționat această
regulă; 097 aprobă nominalizarea, nu o nouă aritmetică.
Operandul citește pe set situația/parametrii/politica, fără SQL pe linie.
Amortizarea lunii produce în fiecare carte D cheltuială/C amortizare,
cu fișa numai pe creditul contului de amortizare. Exemplu: 1.200/12 luni
contabil și 900/18 fiscal → 100 contabil, 50 fiscal; neturi 1.100 și 850.
O linie cu contabil 0 și fiscal 50 produce numai perechea fiscală.

Deductibilul rămâne citire cu politica valabilă pe fișă × lună (090k),
nu o a treia măsură a cubului. Luni se citește din decizia liniei AMO;
recuperarea de două luni poartă 2, inversa −2. Până la existența unui
consumator fiscal care îngheață contractul, suma deductibilă existentă pe
linia AMO rămâne disponibilă ca atribut istoric; nu se declară D101 livrat.

CAS descarcă separat fiecare carte. Pentru brut 1.200, cumulat 100:
D 2813/fișă 100 și C 2131/fișă 100; D cheltuială cedare 1.100 și
C 2131/fișă 1.100. Fiscal: 50 și 850 la brut 900. După CAS, brutul și
cumulatul fișei sunt fiecare zero, nu numai diferența lor. Cazurile complet
amortizate ori cu cumulat zero omit perechea de valoare zero.

Locul și analiza sunt înghețate pe fiecare postare. Schimbarea administrativă
a Loc-ului fișei păstrează regula 087(h): afectează faptele următoare, fără
rescrierea celor existente. Totalul fișei se citește peste locuri; nominalizarea
inițială nu creează rulaje pe cont.

## IMO-B4 — Inversare, corecție și timp

087(g) limitează azi storno PIF/AMO/CAS la luna documentului. 095 devansează
portarea, nu aprobă implicit eliminarea restricției. Contractul o păstrează
și o probează, inclusiv refuzul corecției peste o lună închisă. Relaxarea
ei cere contract separat pentru recalcularea lunilor intermediare și
compensarea efectelor ulterioare. Nu numim acest refuz „caz acoperit prin
corecție”; limita rămâne vizibilă în catalog și în raportul feliei.

În fereastra permisă, storno inversează postările exacte ale ambelor cărți,
inclusiv Transfer-ul PIF și postarea zero a revizuirii. Originea fiecărei
inverse se persistă structural (D8-B3) înaintea folosirii cititorilor.
Evenimentele inversate nu mai furnizează parametri curenți; rămân în istoric.
Originalele sunt imuabile. Dependenții vii blochează anularea/inversarea;
ordinea desfacerii este CAS → AMO ulterioare → evenimentul PIF.

Data contabilă rămâne DataInregistrare; eligibilitatea AMO păstrează data
fizică a punerii. PIF întârziat și recuperarea sunt probate pe ambele date,
cu snapshot-urile perioadelor închise intacte.

## IMO-B5 — Suprafață și activare

- Model: forma fișei în Unitate și persistența FelUnitate; inversa cu
  proveniență; suportul contabil nominalizat și ipotezele sale.
- Declarații PIF/AMO/CAS, operand de fișă și adaptor Fapte pe set.
- Scriere duală în tranzacția comenzii, fără SQL în declarant; gardienii
  fișei și ai suportului protejează același fapt.
- Citire comună de fișă din cub pentru aritmetica AMO/CAS și API Imo;
  portarea lor nu poate aștepta după activarea scriitorilor.
- Politica de amortizare a ambelor profiluri furnizează conturile;
  nicio deducere din prefixe în motor. Politica fiscală nu produce TVA.
- Diagnostic înainte de activare: PIF fără suport, fișe din registre
  fără postări, AMO/CAS fără fișă, storno fără origine. Fără rescriere
  implicită de istoric și fără reunire cub + registre în cititor.
- După probele numerice, perf 2/51, concurență, migrații, metadata/openapi,
  suită integrală pe ambele profiluri și review advers. TR-D8 începe
  numai după acoperirea nominală a producătorilor; TR-D9 taie registrele.

## Implementare și limite tehnice (097)

`DeclarantImobilizari` primește operanzi închiși pentru PIF/AMO/CAS.
`ImobilizariFapte` citește suportul pe set, păstrează dimensiunile și
limitează alocarea la minimul disponibilului de la data cerută până la
ultimele postări existente,
pentru a proteja și nominalizările viitoare. `SuportId/Spatiu` identifică
postarea consumată; `InversaDinId/Spatiu` identifică originalul inversat.
Migrația `FisaSiProvenientaPostarii` adaugă felul explicit al unității,
referințele, constrângerile de formă și indexurile pe părintele partiționat.
Nu reconstruiește stornourile istorice.

Comenzile IMO și anularea/stornarea suportului se serializează printr-un
blocaj tranzacțional comun pe bază, înaintea citirii gardienilor. Motorul
asigură o tranzacție și apelanților direcți; dacă apelantul are deja una,
nu o închide el. Nu este mecanism de serializare generală pentru FIFO sau
pentru închiderea perioadei pe toate ușile.

Proveniența suportului nu are FK restrictiv: după inversarea PIF, suportul
eliberat poate fi anulat fizic în perioada permisă. Postările inverse ale
PIF păstrează referința istorică, însă nu consumă disponibil. Auditul complet
al anulării fizice rămâne de tratat la TR-D9 (091j); nu se pretinde că
referința singură conservă conținutul sursei șterse.

Cititorul comun de fișă agregă în SQL, apoi citește atributele evenimentelor
pe set. Seed-ul refuză istoricul fără fișă/origine/suport și diferențele
valorice dintre cub și registrul dual. Registrul nu completează golurile
cititorului. Cititorii de partide filtrează explicit FelUnitate=Partida și
Carte=Contabil, pentru a nu interpreta o fișă drept partidă.

Validarea și manifestele finale sunt în `scenarii/IMO.md` și în
`run-nucleu/tr-d7c/imo/raport.md`. Portarea rapoartelor generale și a
snapshot-urilor rămâne TR-D8; eliminarea scrierii duale rămâne TR-D9.
