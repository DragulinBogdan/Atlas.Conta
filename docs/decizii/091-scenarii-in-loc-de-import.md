# 91. Proba supremă devine catalogul de scenarii — Import1C se îngheață ca unealtă de migrare, nu ca gate; clona Flax rămâne sursă de întrebări; „rotund" primește definiție ca regulă de oprire a PoC-ului; stornoul după reevaluare = inversarea originalului + compensarea efectelor rămase active; proiecțiile persistate doar reconstruibile din postări; triajul restanțelor în patru stări

- **Data**: 2026-09-22
- **Stare**: activă (amendează 090 la TR-D10, la „Import1C ca probă supremă" și la pașii TR-D7b 3–7; depășește N-r1; închide ca gate `--declaratie-pe-baza` și `--reconciliere-cub`, care rămân unelte de diagnostic; dă stare fiecărei restanțe deschise)
- **Docs**: `docs/consultari/codex-2026-09-22-cub.md` (consultarea care a declanșat decizia), `docs/nucleu/scenarii/README.md` (catalogul: forma, cazurile obligatorii, lanțurile transversale), `docs/nucleu/tr-d7b-tipuri-ramase-contract.md` (amendamentul 091 pe pași și pe regula de oprire), `docs/decizii/restante.md` (triajul)

## Regula durabilă

**Motorul se probează pe scenarii scrise de mână din regula contabilă, nu
prin egalitate cu motorul vechi pe o bază importată. Importul din 1C e
migrare, se face la sfârșit, pe un model rotund, și diferențele de atunci
se judecă contra modelului, nu invers.**

(a) **Proba supremă = catalogul de scenarii.** Un scenariu construiește
documentul (sau lanțul de documente) prin ușa de comandă pe o bază de
profil, pe AMBELE profiluri unde tipul există, și asertează postările din cub
și citirile care rezultă. Așteptarea e scrisă de mână, din OMFP 1802 și din
regula de politică, cu cifre. TR-D10 din 090 („raportul Import1C pe Flax
IDENTIC") nu mai e regulă de oprire a niciunei felii; granița „motorul pe
faptele unei baze reale reproduce baseline-ul" din `nucleu-cub-design.md`
§10 cade; N-r1 e depășită.

(b) **Catalogul** stă în `docs/nucleu/scenarii/`: `README.md` (forma,
cazurile-limită obligatorii, lanțurile transversale) și un fișier per tip de
document. Un rând = `SC-<TIP>-<NN>` (transversalele `SC-X-<NN>`), scenariul,
așteptarea cu cifre, proba (numele check-ului ModelCheck sau al testului de
nucleu), starea. Fiecare tip acoperă ciclul complet: operare, storno în
aceeași perioadă și peste graniță de perioadă, anulare, corecție în perioadă
închisă (storno legat + document nou), stingere sau împerechere unde tipul
deschide partidă, și citirile rezultate (sold pe unitate, fișă de cont,
balanță la graniță). Un tip fără toate rândurile ciclului nu e „pe cub".

(c) **Independența de motorul vechi.** Așteptarea unui scenariu nu se derivă
rulând registrele sau oracolul din ModelCheck (`CubDinRegistre`,
`Normalizari`). Oracolul normalizat a ajuns să semene cu motorul nou prin
patru normalizări succesive (TR-D4, TR-D2a, T-D4.1, T-D13) și a declarat
diferențe în care cubul e cel corect (BTR 535, DSC 842, FCT 13): nu mai e
oracol, e detector de regresie contra unei referințe pe care o corectăm din
mers. `--declaratie-pe-baza` și `--reconciliere-cub` rămân unelte de
diagnostic pentru felia de migrare; nu mai sunt gate al niciunui pas.

(d) **Clona Flax = sursă de întrebări, nu gate.** Înaintea scenariilor unui
tip se face un recensământ prin interogări pe clonă (precedentul: laturile,
T-D13), o dată, cu cifre; combinațiile găsite intră în catalog ca scenarii,
cu proveniența notată. Nicio rulare Import1C, integrală sau pe lună, nu mai
e probă de felie.

(e) **Rularea.** Scenariile rulează prin ModelCheck cu filtru pe tip
(`--scenarii <TIP>`, 091-r1), într-un minut, pe o bază de profil; suita
integrală pe ambele profiluri rămâne gate-ul de commit. Testele de
proprietate din nucleu rămân probele mecanismelor (conservare, FIFO,
Hamilton, TVA); scenariile sunt probele fluxului, prin ușa de comandă.

(f) **Migrarea la sfârșit.** Conectorul (`Import1C`, `Deschidere.cs` din
`BalantaNivel3`, `Imperecheri1C`, `MigrareLegatura`, reconcilierea 1C) e o
felie proprie, cu decizie proprie, DUPĂ „rotund". Codul rămâne în repo și
compilează, nu se extinde. Un caz apărut la migrare care contrazice
catalogul devine scenariu nou plus decizie, nu normalizare (091-r5).
`Materializare.Deschide`, tranzacția de fel `Deschidere`, e a motorului
(orice client are solduri de deschidere) și intră în catalog acum, generic,
fără partea 1C.

(g) **„Rotund" = regula de oprire a PoC-ului**, în șase condiții:
(1) toate cele 15 tipuri cu `PosteazaInCub` pe profilul unde există;
(2) catalogul verde pe ambele profiluri, ciclul complet per tip plus
lanțurile transversale; (3) stornoul după reevaluare definit și probat
conform (h); (4) concurența probată cu două sesiuni reale pe același lot, pe
aceeași partidă și operare simultană cu închiderea perioadei (F27-r8,
S-r9); (5) cititori comuni pe cub (rulaje contabile, mișcări de gestiune,
solduri de partidă și de lot), cu accesul la `Postare` în afara lor refuzat
de un test de arhitectură (091-r3) și cu `Transfer` plus stornourile lui
excluse acolo, nu în fiecare raport (N-r8); (6) citirile de bază din cub
(balanță, fișă de cont, sold pe unitate, jurnal) și registrele vechi tăiate
(TR-D9), fără migrare. Până la (6) nu se declară „rotund".

(h) **Stornoul după reevaluare** (N-r5) are două componente semantice
executate într-o singură comandă atomică: inversarea originalului (exact
postările tranzacției originale, fără politica de azi, fără re-evaluare) și
compensarea efectelor ulterioare rămase active (regulă de domeniu: stocul
înapoi pe lot, cursul pe 665/765, imobilizarea pe fișă). Compensarea cere
proveniență persistată: postarea pe care a acționat corecția, suma alocată,
ce s-a compensat deja. Scenariile obligatorii, scrise ÎNAINTEA codului:
corecție → storno consum; corecție → storno corecție → storno consum (fără a
doua compensare); două corecții → storno consum. Se implementează la TR-D9
odată cu `Atribuit`.

(i) **Proiecțiile persistate** sunt legitime doar dacă se pot șterge și
reconstrui exact din postări fără a rerula documentele prin politici; fiecare
vine cu `Reconstruieste` și cu proba de egalitate; `Sold` e precedentul.
Materializarea suplimentară se face doar pe cifră măsurată (TR-D8), nu
anticipat. „Citirea doar adună" se citește pe domeniul măsurii: cantitatea
per produs × unitate de măsură, valoarea per valută, cărțile separat;
contractul cititorului poartă domeniul.

(j) **Explicația deciziei** se persistă compact pe tranzacție: versiunea
politicii, alocările pe unitate cu cantitate și valoare, baza evaluării,
distribuția rotunjirii (S-r2, activă la TR-D8). Scenariile o asertează:
„de ce acest lot", nu doar „cât".

(k) **Triajul restanțelor.** Fiecare restanță deschisă din `restante.md`
poartă una din stările: `activă` (blochează „rotund"), `după PoC` (cerință
de produs, ecran, host sau perf; se rejudecă pe modelul rotund), `migrare`
(conectorul 1C, Flax, date), `cade la TR-D9` (mecanismul dispare cu
registrele; dacă nevoia rămâne, se rejudecă pe cub), `depășită de 091` (era
a gate-ului). O restanță nouă intră direct cu una din stări. Lista `activă`
trebuie să încapă pe un ecran; când nu mai încape, se face triaj, nu se
lungește.

(l) **Documentația stării.** CLAUDE.md §Stare spune ce e adevărat AZI, în
cel mult un paragraf pe arie; cronologia și cifrele feliilor stau doar în
`istoric-plan-de-lucru.md` și în fișierele deciziilor. Un rezumat de felie nu
se mai adaugă în CLAUDE.md.

## Context

Owner-ul, 2026-09-22, după consultarea cu Codex
(`docs/consultari/codex-2026-09-22-cub.md`), a numit patru lucruri care îl
țin în loc: datoriile care se strâng pe parcurs și îndepărtează PoC-ul;
decizia de a rula importul din 1C ca validare, care dă validare dar
limitează modelul la ce există în Flax; nevoia ca motorul să ajungă într-un
punct „rotund"; numărul de decizii și micro-decizii care proiectează o
imagine tot mai divergentă de ce există. Cererea: nu mai facem import din
1C; stabilim scenarii pentru documentele acoperite, cu cazuri-limită, și
rulăm motorul peste ele; migrarea rămâne la sfârșit, când deciziile se iau
pe un model rotund.

Cifrele care susțin diagnosticul, citite din repo la 2026-09-22:

| Semnal | Cifră |
|---|---|
| Restanțe deschise în `restante.md` înaintea triajului | 230 din 263, pe 27 de prefixe |
| O rulare de gate `--declaratie-pe-baza` pe clonă | 3 h 04 (pasul 2b) |
| Import1C integral pe Flax | 1 h 57 min |
| Normalizări ale oracolului acumulate | TR-D4, TR-D2a, T-D4.1, T-D13 |
| Diferențe „declarate" în care cubul e mai corect decât registrele | BTR 535, DSC 842, FCT 13 |
| Tipuri absente din Flax | DVI, PIF, CAS, AMO, NIR manual |
| Probe existente | nucleu 165 teste de proprietate; ModelCheck 1771 privat / 1458 bugetar |

Ce dădea importul și cum se acoperă fără el: volumul (70 k tranzacții) se
produce sintetic din `Gen.cs` cu sămânță fixă când e nevoie de perf;
combinatorica reală (două partide pe o FCL cu avans, stingeri din 2025 pe
documente din 2024, ieșiri parțiale cu rest) se culege prin recensământ pe
clonă, o dată per tip, și devine scenarii — litera (d).

## Punctele consultării și poziția luată

1. **Stornoul după reevaluare.** Codex: inversarea izolată a postărilor
   atribuite nu descrie complet operația; separă inversarea originalului de
   compensarea consecințelor, cu proveniență explicită, și probează ordinea
   operațiilor. Acceptat integral, litera (h). Adăugat: reevaluarea nu e
   încă implementată, deci scenariul e specificația, scris înainte de cod;
   e exact tipul de caz pe care Flax nu îl exercită.
2. **`Transfer` ca filtru fragil.** Acceptat diagnosticul; remediul e proba
   de arhitectură (accesul la `Postare` doar prin cititorii comuni) și
   derivarea excluderii stornoului de transfer prin `StornoAl`, nu un flag
   nou — litera (g)(5). Întrebarea owner-ului „proiecțiile devin registre
   derivate?" primește răspunsul lui Codex ca literă (i): da, dacă se
   reconstruiesc din postări fără a rerula politicile.
3. **Concurența.** Condiție de corectitudine pentru FIFO, nu doar
   nefuncțional; nu blochează pașii TR-D7b, dar e condiție a lui „rotund",
   litera (g)(4). Direcția (de confirmat la implementare): blocaj pesimist
   per unitate în tranzacția de comandă, nu Serializable cu reluare,
   fiindcă ObjectSpace-ul sincron nu reia elegant o tranzacție întreagă.
4. **„Citirea doar adună" prea larg.** Parțial: e formulare, nu defect de
   model; litera (i), a doua propoziție.
5. **Postările nu explică decizia.** Acceptat; mutat mai devreme decât
   S-r2 o punea, litera (j), fiindcă scenariile au nevoie de explicație ca
   să aserteze „de ce". Ștergerea fizică la anulare rămâne excepție
   declarată, de tranșat la TR-D9.
6. **Flax nu acoperă modelul.** Acceptat integral; e chiar teza deciziei.

Obiecția de fond a lui Codex, că motorul pur nu depinde de cub și că mai
multe registre produse atomic nu sunt mai multe surse de adevăr, e corectă
intelectual și NU se redeschide: felia 32 e pe această direcție, iar
redeschiderea e chiar divergența de care se teme owner-ul. Cubul își
dovedește valoarea la citiri (TR-D8); dacă acolo aproape fiecare raport cere
reconstruirea unui registru complet, întrebarea se pune atunci, cu cifre.

## Ce se schimbă concret

- **090**: `Stare` devine „activă, amendată de 091"; TR-D10 și „Import1C ca
  probă supremă" din titlu și din §Ordinea nu mai sunt regulă de oprire.
  Textul nu se rescrie.
- **TR-D7b** (`tr-d7b-tipuri-ramase-contract.md`): pașii 3, 4, 5 se fac cu
  scenariile tipului în locul gate-ului pe clonă (recensământul pe clonă
  rămâne, ca întrebare); pasul 6 se reduce la `Materializare.Deschide`
  generic cu scenariile lui, partea 1C (`BalantaNivel3`, `MigrareLegatura`,
  `Imperecheri1C`, T-r4) trece la felia de migrare; pasul 7 (proba supremă)
  se taie; regula de oprire a feliei se rescrie pe catalog. Amendamentul e
  scris în contract, sub pași, fără renumerotare.
- **`nucleu-cub-design.md` §10/§11**: invariantul 7 al designului cade;
  notă de amendament, textul rămâne.
- **`stare-curenta/dezvoltare-si-validare.md`**: rândul „Tip trecut pe
  `PosteazaInCub`" și paragraful probei supreme trimit la catalog; uneltele
  de gate devin diagnostic.
- **CLAUDE.md**: §Stare rescris ca stare, nu cronologie (litera (l));
  regula de lucru „proba supremă = Import1C" înlocuită cu catalogul.
- **`restante.md`**: triajul (litera (k)); 091-r1…r5.
- **`docs/nucleu/scenarii/README.md`**: forma catalogului, cazurile-limită
  obligatorii, lanțurile transversale, starea per tip.

## Raportul cu invarianții

I, II, IV, VI neatinși. III („registrele sunt singurul adevăr al
agregării") e deja amendat de 090 spre cub; litera (i) îl întărește:
o proiecție persistată nu e adevăr, e reconstrucție probată. V („sursele
externe sunt evidență, niciodată canonic") e chiar litera (f): 1C a fost,
prin gate, canonic de facto pentru forma motorului; decizia îl readuce la
evidență.

## Ce rămâne deschis (restanțele deciziei)

- **091-r1** — filtrul `--scenarii <TIP>` în ModelCheck (rulare pe un tip
  într-un minut, pe o bază de profil); azi scenele rulează doar în suita
  integrală. Activă, primul lucru din pasul 3.
- **091-r2** — recensământul pe clona Flax per tip rămas (NTC, ITV, RDC,
  RLF, ASM, LDI, NIR), o dată, înaintea scenariilor lui; cifrele în fișierul
  tipului din catalog. Activă, per pas.
- **091-r3** — testul de arhitectură care refuză accesul la `Postare` în
  afara cititorilor comuni (`Module/Cub/Citiri`). Activă, TR-D8.
- **091-r4** — felia de migrare, cu decizie proprie, după „rotund":
  conectorul 1C repornit pe modelul final, deschiderea de terți din
  `BalantaNivel3` (T-r4), stingerile 2024, reconcilierea 1C ca raport de
  diferențe. Migrare.
- **091-r5** — un caz apărut la migrare care contrazice catalogul devine
  scenariu nou plus decizie; oracolul normalizat nu se mai extinde.
  Migrare.
