# Consultare: nodul de transformare și poziția fără unitate

- Data: 2026-10-05. Stare: **ambele runde rulate (2026-10-05); răspunsurile
  în `raspunsuri/r1-*.md` și `r2-*.md`; decizia e la owner.**
- Rezultat pe scurt: forma Y scrisă ca mișcare obișnuită e respinsă de ambii
  analiști (rulaj dublu, valoare permanentă în nod, nucleul o refuză azi în
  `Conservare.VerificaTransformarile`); regula Q e acceptată de ambii, cu
  schimbări, în TR-D9b. Fișa de fapte avea trei erori, numite în rapoarte:
  „Y trece prin nucleu neschimbată", rulajul V în loc de 2V, „LDI e deja
  forma Y".
- Întrebarea owner-ului: cum simplificăm modelul. Două amendamente:
  1. ASM pe același cont, ca mișcare obișnuită prin nodul `Transformare`,
     cu valoarea și cantitatea împreună (forma Y), în locul contraponderii
     de valoare zero (forma X, implementată azi);
  2. unitatea nulă ca membru al modelului, cu regim declarat pe cont pentru
     poziția fără unitate: închisă, deschisă, suport (regula Q), în locul
     celor trei regimuri de azi (regula P).
- Analiști: Claude Fable 5.1 și GPT-6 Astra, independent, efort ridicat.
- Consultarea nu e sursă de adevăr; aprobă numai owner-ul. Ce se acceptă
  intră ca amendament la contractul TR-D9a sau în contractul TR-D9b.

## Circularitatea

Obiecția owner-ului: catalogul de scenarii și testele sunt date sintetice
scrise pe principiile anterioare, deci nu pot judeca principiile noi.
Exemplu: SC-ASM-04 (301 către 345) își ia justificarea din recensământul
„367 cu mai multe conturi", care măsura contul implicit al tipului de
material, nu contul de pe linia sursei. În sursa Flax, din 10.198 de
asamblări, una singură are două conturi.

Protocolul răspunde în două feluri:

- runda 1 e oarbă: fără cod, fără scenarii, fără să spună ce e implementat;
  judecata vine din regula contabilă și din coerența modelului;
- în runda 2 catalogul e listă de impact, nu oracol. Probele admise: regula
  contabilă, invarianții judecați unul câte unul (principiu sau artefact),
  cazuri numerice construite de analist, practica din sursa reală.

## Protocolul

| Runda | Ce primește analistul | Unde rulează | Fișier |
|---|---|---|---|
| 1, oarbă | numai promptul | director gol, în afara repo-ului | `1-runda-oarba.md` |
| 2, informată | răspunsul lui din runda 1, fișa de fapte, repo în citire | clona `D:\Temp\conta-analiza`, la `a5df5fe` | `2-runda-informata.md`, `fapte.md` |

Fără critică încrucișată; se adaugă numai la cererea owner-ului.

Mecanica e cea din `../2026-10-05-cub-vs-registre/README.md`. Rularea:
`run.sh`, secvențial, cu jurnalul în `raspunsuri/run.log`.

## Răspunsurile

`raspunsuri/r1-astra.md`, `r1-fable.md`, `r2-astra.md`, `r2-fable.md`.

## Ce a hotărât owner-ul (2026-10-05)

Explorarea se închide fără schimbare de model. Rămân forma ASM de azi
(ASM-B2…B7, cu gardul P = C din TR-D9a), cele trei regimuri de azi pentru
poziția fără unitate și cubul fără semn sau rol pe postare. TR-D9a merge cum
e aprobat.

Abandonate, cu motivul:

- **Nodul cu valoare (forma Y).** Liniile sunt deja independente în forma de
  azi; nodul ar cere un gard nou în nucleu și ar ține valoare permanentă pe
  produs pe contul de stoc. Scris ca mișcare obișnuită dă rulaj dublu.
- **Găleata cu regim declarat (regula Q).** Nicio cerere pentru închiderea
  unei găleți; scenariile nu se schimbă sub sămânța propusă; costul e schemă,
  tranziții de închidere și suprapunere cu steagurile de dimensiuni.
- **Semnul sau rolul pe postare.** Rezolva deosebirea dintre nod și găleată,
  care nu mai există fără cele două de mai sus.

## Ce s-a reținut

Restanțele `NG-rN` din `docs/decizii/restante.md` au textul aici.

- **NG-r1. Balanța și fișa filtrate pe gestiune sau pe material nu văd
  transferurile.** `ContabilProiectii.Atomi` citește `Contabil.Postari`, care
  lasă afară tranzacțiile `Transfer` (`Cub/Citiri/Contabil.cs:42`), apoi
  `Balanta` filtrează pe gestiune și pe material
  (`Proiectii/ContabilProiectii.cs:206-208`). După un BTR sau un ASM valoarea
  rămâne la gestiunea sau produsul sursă. Citit din cod de ambii analiști și
  de sesiunea de orchestrare; nerulat. De probat, apoi owner-ul spune care e
  semantica dorită a filtrului pe conturile de stoc.
- **NG-r2. Contul lotului vine din tipul curent de material al produsului**
  (`Motor/Fapte.cs:95-103`). Dacă tipul se schimbă după recepție, soldul se
  caută pe contul nou și consumul poate fi refuzat cu stoc existent. Dedus de
  ambii analiști, nedemonstrat. Candidat pentru TR-D9b.
- **NG-r3. ASM pe același cont.** Direcția owner-ului: asamblarea lucrează pe
  un singur cont, iar trecerea între conturi e a bonului de producție. Nu
  intră cât timp BPR e rezervat (019), fiindcă refuzul n-ar avea ieșire.
  „Același cont" se măsoară pe contul derivat din tip, unde clona de import
  are 367 din 1.228 de documente pe mai multe conturi; pe contul liniei din
  sursă e unul din 10.198. Datele importate nu stabilesc regula.
- **NG-r4. Raportul soldului fără unitate, detaliat pe cont.** Numai citire:
  pozițiile cu unitatea nulă pe conturile care urmăresc loturi, partide sau
  fișe, pe coordonata completă, nu net pe cont. Azi diferența dintre contul de
  stoc și suma loturilor o semnalează doar reconcilierea SAF-T. Candidat
  pentru TR-D9b.

Corecția de recensământ e notată în
`docs/nucleu/tr-d7b-asm-transformare-contract.md`, sub ASM-B1.
