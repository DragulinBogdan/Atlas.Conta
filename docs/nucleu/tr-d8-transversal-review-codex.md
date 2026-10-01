# TR-D8 transversal — review advers al contractului

Data: 2026-10-01. Contract revizuit: `26df646`, branch `tr-d8-transversal`,
bază `02f7788`. Stare: **șapte observații deschise; contractul cere corectare
înaintea implementării**. Aprobarea D-urilor și X-Q-urilor rămâne la owner.

Review static al [contractului](tr-d8-transversal-contract.md), confruntat
cu deciziile și sursele curente. Contraexemplele de concurență de mai jos
sunt intercalări deduse din cod, nu rulări pe două conexiuni deja executate.
Nu sunt constatări despre o implementare nouă: felia este încă propusă.

## X-RV1 / P1 — Blocajul trebuie să protejeze citirea care decide valoarea

Loc: X-D6(b), liniile 197–203.

„Înaintea verificărilor de sold” permite blocarea prea târziu.
`Materializare.Opereaza` construiește contractul la linia 27 și verifică
soldul intermediar la 31. `Contractare.Contracteaza` citește operandul și
execută declarantul; `DeclarantBonConsum` evaluează ieșirea din acel sold.
Separat, `MotorOperare.Opereaza:306` calculează planul registrelor înainte
de apelul materializării de la 428.

Contraexemplu: lot cu Q=3, V=1,00; două BCS consumă câte o bucată. Dacă
ambele contracte sunt construite înainte de blocare, ambele aleg 0,33.
Un blocaj pus numai înainte de `VerificaSoldIntermediar` serializează
scrierea și ambele verificări cantitative trec, dar rămâne Q=1, V=0,34.
Execuția serială corectă evaluează a doua ieșire din Q=2, V=0,67: 0,34;
rămâne Q=1, V=0,33. Conservarea și lipsa stocului negativ nu disting cazurile.
Regula este explicită în `Nucleu/Masura/Evaluare.cs:15–17`.

Corecție cerută: fixați limita tranzacțională **înaintea citirilor folosite
la selecție/evaluare**, inclusiv planul regimului dual, sau recitiți și
reconstruiți toate rezultatele dependente după obținerea blocajelor.
Un contract calculat înainte de așteptare nu devine valid doar printr-un
nou test de disponibil. Precizați cum se descoperă și revalidează setul
unităților FIFO când concurentul schimbă disponibilul.

Probă de închidere: cazul pozitiv 3/1,00 cu două consumuri de câte 1,
ambele acceptate, valori 0,33 și 0,34 în ordinea serializată, rest 1/0,33;
explicațiile persistate trebuie să dovedească cele două solduri diferite.
Păstrați și cazurile negative peste disponibil și consum–retragere.

Ordinea pe Guid a unităților rezolvă numai ordinea acelei categorii de
blocaje. Contractul trebuie să inventarieze ordinea comună cu documentele,
perioada, 97001, 97002 și deschiderea. De exemplu,
`ImperechereService.Imperecheaza:22–24` ia documentele înaintea perioadei;
`Materializare.StingereDeschidere.cs:20–34` adaugă blocajul tranzacției de
deschidere. Testați căile mixte reale. Nu afirm un deadlock demonstrat între
aceste căi; testul cu două liste de unități inversate nu certifică singur
interacțiunea lor.

## X-RV2 / P1 — Securitatea explicației nu se reduce la accesul la tranzacție

Loc: X-D4(a), (c), (e), liniile 107–120 și 129–132.

Se copiază întregul contract în fiecare tranzacție și se expun deciziile,
soldurile citite și absorbțiile. Proba „rândul refuzat nu divulgă valoarea”
nu definește ce se întâmplă când tranzacția este accesibilă, dar o altă
linie, un membru valoric sau o parte din istoricul unității este restricționată.

Contraexemplu: utilizatorul vede BCS-ul curent, dar nu toate intrările pe
lot. `SoldUnitateCitit.Sold` dezvăluie soldul integral citit de motor.
Chiar filtrarea explicației după postările vizibile ale BCS-ului lasă
această valoare să treacă. Analog, accesul la un `Transfer` nu autorizează
automat toate deciziile copiate din contractul cu `Operare` + `Transfer`.
Permisiunea pe `Postare.Valoare` nu filtrează automat numerele din jsonb.

Corecție cerută: definiți explicit domeniul de autorizare al explicației,
inclusiv faptele istorice/aggregate și membrii valorici. O variantă simplă
este refuzul citirii explicației dacă nu se poate demonstra accesul complet
la datele pe care le dezvăluie; o proiecție parțială cere reguli proprii,
nu doar filtrarea tranzacției. Forma jsonb versus tabelă nu rezolvă această
problemă. Precedentul local pentru acces complet, inclusiv membri, este
`SaftController.AccesIncomplet:226–237`.

Probă de închidere: HTTP pe host viu, cu tranzacție vizibilă și separat
(1) membru valoric refuzat, (2) altă linie refuzată, (3) istoric parțial al
unității. Răspunsul nu conține nici soldul ascuns, nici o valoare derivată
care îl reconstituie. Includeți și storno → explicația originii.

## X-RV3 / P2 — Acoperirea stocului pe rând respinge recepțiile conexe valide

Loc: X-D7(a), liniile 211–216.

Egalitatea propusă între fiecare rând `RegistruStoc` și o postare de lot
nu este contractul recepției conexe. Pentru FCT 10/100 → NIR constatat
10/100, registrul recepției este al NIR-ului, iar cubul are recepția pe FCT
și zero tranzacții economice NIR. Pentru FCT 10/100 → constatat 9/90,
NIR-ul are cumulul recepționat în registru și delta −1/−10 în cub.
Acestea sunt cazuri aprobate prin NIR-D1/D2 și 098/099, nu istoric incomplet.

Surse: [contractul NIR](tr-d8-nir-delta-contract.md), liniile 14–23,
45–63 și 125–128; `ModelCheck/Nucleu/ReconciliereCub.cs:27–59` folosește
deja grupul identificat prin proveniența istorică.

Corecție cerută: definiți corespondența cantitativă prin eveniment/grup
și proveniență, cu lot, gestiune, cont, sens și origine de storno. Nu
introduceți o excepție generică „NIR ignorat” și nu comparați doar totalul
global. Un `Any` pe aceeași cantitate poate reutiliza aceeași postare pentru
mai multe rânduri și rata lipsa unui capăt BTR.

Probe de închidere: recepție conexă cu delta zero, minus, plus și storno;
fiecare trece nemodificată. Ștergerea/mutarea unui efect obligatoriu sau a
provenienței, respectiv lipsa unui capăt BTR, este detectată. Așteptările
cantitative rămân independente de interogarea invariantului.

## X-RV4 / P2 — Un singur câștigător și bariera propusă nu sunt oracole generale

Loc: X-D6(a), liniile 188–196; X-D8, liniile 261–262.

Operare → commit → închidere poate accepta legitim ambele comenzi;
închidere → operare trebuie să refuze operarea. Două inserări de detalii
sub același document pot primi poziții distincte și pot reuși ambele.
Regula globală „exact una trece” cere refuzuri artificiale pentru rezultate
seriale valide. La împerechere, rezultatul depinde de sumele și capacitatea
fixture-ului, nu numai de identitatea perechii.

În plus, o barieră bilaterală „a verificat, înainte să scrie” se poate
bloca tocmai după implementarea corectă: prima sesiune ține blocajul și
așteaptă bariera; a doua așteaptă blocajul și nu poate ajunge la barieră.

Corecție cerută: matrice cu rezultatele seriale permise pentru fiecare
scenariu și ambele ordini relevante. Pentru cazul peste disponibil cereți
un succes și un refuz; pentru cazurile compatibile, două succese cu starea
numerică a execuției seriale. Clarificați și formularea „două documente noi
pe același document-părinte” pentru S-r9: este vorba de pozițiile detaliilor.
Sincronizarea testului trebuie să permită primei sesiuni să facă commit
când a doua a intrat în așteptarea blocajului, cu timeout și rollback
controlate; nu să ceară ambelor să treacă simultan secțiunea protejată.

## X-RV5 / P2 — Invariantul auditului amestecă ieșirea fizică și evaluarea FIFO

Loc: X-D4(d), liniile 122–128.

Stornoul unei intrări are cantitate negativă pe lot, dar contractul cere
și că orice ieșire are propria `ValoareIesire`, și că storno nu are explicație
proprie. RLF este alt contraexemplu: ieșirea păstrează valoarea fiscală,
nu evaluarea FIFO a soldului. `DeclarantReturFurnizor.cs:57–59` spune și
execută explicit această regulă; [SC-RLF-05](scenarii/RLF.md) păstrează
reziduul valoric declarat. Actuala declarație RLF nu produce
`ValoareIesire`/`SoldUnitateCitit` pentru acest calcul.

Corecție cerută: invariant pe mecanismul explicat, cu domeniu și semn
explicite. Ieșirea evaluată din sold trebuie legată de decizie și de soldul
citit; RLF/NIR-delta trebuie să explice sursa valorii lor fără a schimba
evaluarea aprobată; storno verifică inversa și explicația originii.
Precizați cum se atribuie deciziile unei postări când lista întregului
contract este duplicată pe mai multe tranzacții, ca suma auditului să nu
numere aceeași decizie de mai multe ori.

Probe de închidere: BCS, RLF la golire cu reziduu, NIR-minus și storno de
intrare, plus document cu `Operare` și `Transfer`. Auditul trece pentru
semantica existentă și cade dacă explicația este eliminată sau alterată.

## X-RV6 / P2 — Criteriul picat poate fi declarat și apoi gate-ul închis

Loc: X-D5(c), liniile 158–162; X-D8, liniile 264–265.

„Se corectează sau intră în limite-curente cu cifra” permite închiderea
cu N+1 sau recitire integrală demonstrată. Contrazice X-D1: toate literele
verzi și nicio închidere prin text. Simpla publicare a măsurătorii nu face
criteriul verde. X-Q2(B), care permite închiderea TR-D8 fără cifra de perf,
este tot o schimbare de gate, nu o realizare a lui X-D1 în forma curentă.

Corecție cerută: criteriu picat = X-D5/TR-D8 deschis. Dacă owner-ul acceptă
o amânare, aceasta amendează explicit perimetrul și numește cititorul,
criteriul, cifra și restanța; nu se aplică automat prin adăugare în docs.
Pragurile absolute pe baza reală pot rămâne la migrare fără această supapă.

## X-RV7 / P2 — Rândurile returnate și liniaritatea în k pot ascunde istoricul

Loc: X-D5(b)–(e), liniile 149–172.

Un SQL `SUM` peste întregul istoric returnează un rând și execută o singură
comandă indiferent de m. La m fix, timpul dominat de istoric poate satisface
ușor f(4k) ≤ 5f(k). Toate trei pot fi verzi deși scanarea istoriei rămâne.
Contractul cere planuri, dar nu fixează ce probă din plan decide criteriul;
„rânduri citite” trebuie separat de rândurile livrate clientului.

Există și fals negativ: reconstrucția snapshot-urilor, inclusă în lista
măsurată, citește legitim istoria pe care o reconstruiește. Citirea curentă
din snapshot, reconstruirea și fallback-ul pentru acces parțial/excluderea
unui document nu au același criteriu. În plus, datele de istoric pot lăsa
mai multe poziții deschise: trebuie controlată cardinalitatea rezultatului
când se compară m=0/6/12.

Corecție cerută: matrice operație × rută × criteriu, separând reconstrucția
de consumul unui snapshot valid. La k fix comparați planurile pentru m=0,
6,12 și accesul efectiv la `Postare`: intervalele/partițiile accesate,
nodurile de scanare, `actual rows × loops`, rândurile filtrate și buffers,
nu doar rândurile de ieșire. Fixați mulțimea pozițiilor relevante pentru
comparație sau raportați separat creșterea justificată a rezultatului.

Adăugați controlul numeric al rezultatelor înainte/după optimizare pe
aceeași stare și oracle independente pentru fixture. X-D3 verifică faptele
persistate; nu detectează un cititor optimizat care întoarce zero sau omite
soldul inițial. Proba adversă trebuie să respingă atât acel cititor rapid,
cât și `SUM`-ul istoric mascat de un singur rând returnat.

## Răspunsuri la punctele cerute și delimitări

1. Perimetrul nominal acoperă marile restanțe citate. Mai trebuie pin-uite
   acoperirea nevacuă și excepțiile: fixture-ul X-D5 nu numește DVI,
   PIF/AMO/CAS, deschiderea și transferul explicit NTC/împerechere. Listați
   pentru fiecare ramură a reconcilierii/INV-CUB faptele și proba care o
   exercită; dacă sunt alte scene, referiți-le explicit. 102(d) cere fapte
   și mutant pe ramură, nu verde pe mulțime vidă. În X-D2 lista permisă
   trebuie să numească utilizarea permisă, nu să absolve întregul serviciu
   de culegere doar prin includerea lui în categoria „scriitori”.
2. X-Q1: recomand **A, jsonb versionat**, după X-RV2/X-RV5. Nu am găsit
   un motiv care să impună tabela copil pentru cerința curentă. Persistarea
   atomică și append-only sunt potrivite, dar nu stabilesc autorizarea și
   nici semantica auditului.
3. X-D6: contraexemplul de evaluare și refuzurile false sunt X-RV1/X-RV4.
   091 spune „direcția (de confirmat la implementare)”, nu că ordinea
   tuturor blocajelor este deja decisă. Este nevoie de demonstrația căilor
   mixte; nu certific absența deadlock-ului din lectura statică.
4. X-Q2: recomand **A**, cu X-RV6/X-RV7 corectate. Scara sintetică poate
   verifica forma costului; nu devine cifră de producție pe volum real.
5. X-D3: **zero Δ în domeniul comparabil rămâne corect**. Nu propun
   toleranță nouă. „Aceleași comenzi” nu înseamnă aceeași reprezentare în
   cele două modele. Păstrați nominal delimitările T-D3/T-D7, ASM și mai
   ales FCT/NIR-delta din 099. `ReconciliereCub.Raport:541` exclude deja
   ASM Operare și grupurile FCT/NIR cu deltă din (a), raportându-le în (h).
   Grupurile incomplete sunt raportate separat (`:53–66`), iar (f) la m=0
   este explicit vacuă (`:161`). Artefactul trebuie să arate și aceste
   contoare/note, nu numai exit 0; o ramură vacuă nu este acoperită.
6. X-Q3: recomand **A**, refuzul fără efect, cu mutantul cerut. X-Q4:
   recomand **A**, gardianul ireversibil după primele tranzacții, exercitat
   și pe seed. Acestea sunt recomandări de review, nu aprobarea owner-ului.

La linia 22 trebuie eliminată formularea „diagnostice ... la pornire”:
102(d) interzice scanarea istoriei la pornirea hosturilor. X-D7 poate rămâne
probă ModelCheck pe fapte, înainte de purjă, fără reintroducerea mecanismului
eliminat. `Imobilizari.VerificaAcoperire` există efectiv în sursa curentă;
nu contest premisa X-D7(e).

## Verificări și limite

Comenzi: `git status --short`, `git rev-parse --short HEAD`,
`git log -1 --oneline`; interogări `codegraph explore` pentru Materializare,
Contractare, Fapte, Decizie/Ipoteza, ReconciliereCub, ReceptiiConexe,
MotorOperare, ImperechereService, Evaluare.Iesire, DeclarantReturFurnizor,
Imobilizari.VerificaAcoperire și accesul securizat; citiri țintite prin
`Get-Content`/`Select-String`; `git diff --check` după scrierea review-ului.

Au fost consultate CLAUDE.md, invarianții, contractele TR-D8 citiri,
NIR-delta, TR-D7b, review-ul TR-D8, deciziile relevante 090/091/102,
restanțele și catalogul RLF. Nu am schimbat codul sau contractul propus,
nu am executat ModelCheck/verifica.ps1, teste HTTP, perf sau concurență și
nu am atins baze de date. Observațiile de contract trebuie transformate
în probe executabile în implementare; acest review nu certifică gate-ul.
