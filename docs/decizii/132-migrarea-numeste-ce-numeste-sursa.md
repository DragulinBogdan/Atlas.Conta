# 132 — Migrarea din 1C: conectorul numește ce numește sursa, iar diferența unei partide se atribuie pe document

- Data: 2026-10-17
- Stare: propusă
- Execuție: etapa E4, pasul 1 (2026-10-17), branch `e4-pas1-ianuarie`; ianuarie 2025 cu bază recreată: contractele 1–5 îndeplinite
- Arii: import, partide
- Amendează: —
- Închide: 110-r2, 107-r9, 107-r10, 109-r3, TR-r2
- Docs: `docs/import/e4-migrarea-contract.md` (pasul 1); `docs/stare-curenta/import.md`

## Regula durabilă

**Conectorul transcrie o singură dată ce mișcă sursa, numește partida pe care o numește sursa și stinge tot ce stinge sursa. O diferență a contractului 5 se împarte pe documentele sursei, iar fiecare parte are un nume sau e eșec.**

(a) **Ce postează documentul nu mai transcrie puntea.** Reclasificarea de cont la transfer devine ASM, iar ASM-ul o postează pe cont. Puntea NTC o preia numai pe rândurile pe care ASM-ul nu le mișcă (lot cu valoare 0, cont nou fără tip, lot deja pe contul nou). Dacă ASM-ul nu ajunge operat, rândurile se declară nepostate.

(b) **Slotul „Documente” al unei linii de notă numește partida.** Documentul din slot numește partida documentului sau partida inițială a lui (115). Slotul prezent și gol numește poziția fără document a (cont, partener), adică partida inițială cu referința din (cont, partener). Un document numit care n-are poziție la graniță și nici document în Atlas lasă linia pe FIFO și se numără (E4-r1).

(c) **Trecerea 2 enumeră orice document care stinge în sursă.** Printre ele intră și încasările pe facturi nominale ale raportului de vânzări cu amănuntul (copiii `#inc`). Cheia stingătorului e cea sub care trecerea 1 a scris încasarea.

(d) **Împerecherea pe documente se plafonează la restul ambelor documente**, ca pe partida inițială (107 h). Excedentul se numără și se raportează. Un document fără partidă pe latură se raportează separat de unul cu restul consumat.

(e) **Contractul 5 atribuie pe document.** Pentru o partidă rămasă fără explicație, conectorul compară, document cu document, mișcarea sursei pe poziție (slotul „Documente” și partenerul, de la graniță la capătul lunii) cu mișcarea Atlas pe partidă (postările de operare și transfer, legate de documentul-sursă). O parte e numită când:
- e o stingere refuzată sau plafonată de trecerea 2;
- e un rând al unui document tipizat, ținut în Atlas pe partida documentului;
- e o notă Atlas pe FIFO, acolo unde sursa numește un document fără poziție (E4-r1).

O parte fără nume, sau o atribuire a cărei sumă nu închide Δ-ul partidei, e eșec.

## Context

Etapa E4 a pornit cu măsurătoarea de plecare (contractul E4): pe ianuarie 2025 contractul 1 pica pe 371/3028/303, cu suma zero, iar contractul 5 pe 5 partide. Ancheta pe sursă a dat trei cauze în conector, nu în motor:
- puntea reclasificării era din vremea în care ASM-ul nu posta contabil (110-r2);
- încasările inline ale retailului lipseau din trecerea 2 (107-r10);
- slotul gol al subconto-ului nu numea poziția fără document (107-r9).

Partida a cincea (401, −60) e o notă 1C care creditează două facturi ALSO din 2024 cu sold zero la graniță: Atlas nu are partida lor și aplică FIFO pe avansul partenerului.

## Tranșări

- (b) pe FIFO, nu partidă nouă: o linie de notă care ar naște partida unui document fără poziție cere o regulă de motor (o latură care deschide partidă proprie fără FIFO). Regula de oprire 1 a contractului E4 o trimite la owner; cazul e de 60 de lei pe ianuarie și rămâne E4-r1.
- (d) plafonare, nu refuz: refuzul lăsa factura cu rest și plata fără împerechere, pentru diferențe de 0,29–73 de lei între totalul 1C și totalul Atlas al facturii (diferența stă pe puntea NTC). Plafonarea stinge factura și lasă excedentul pe plată, cu sumă raportată.
- (e) atribuire, nu acumulator pe terț: o primă variantă compara Σ Δ pe (cont, partener) cu suma liniilor rămase pe FIFO. A căzut la primul caz cu două documente pe aceeași poziție (o operație și un retur la furnizor pe slotul gol al partenerului FF91). Atribuirea pe document numește fiecare parte.

## Ce rămâne deschis

- E4-r1: o linie de notă pe un cont urmărit nu poate deschide partida unui document fără poziție. Rămâne pe FIFO, iar contractul 5 o numește.
- Categoria „neatinse de trecerea 2” (128 de partide pe ianuarie, Σ cub −258.298,19) rămâne declarare globală. La pasul 3 se judecă dacă trece pe atribuirea pe document (107-r1).

## Istoric

- 2026-10-17: scrisă la pasul 1 al etapei E4; propusă owner-ului.
