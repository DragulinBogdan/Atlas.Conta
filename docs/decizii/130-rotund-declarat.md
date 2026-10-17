# 130 — „Rotund” e declarat: PoC-ul se oprește, migrarea devine etapa următoare, iar restanțele se rejudecă pe modelul rotund

- Data: 2026-10-17
- Stare: propusă
- Execuție: felia de declarare a firului „rotund” (2026-10-17), branch `rotund-declarare`, după feliile R0–R7 (PR #62–#71); owner-ul a cerut declararea la 2026-10-17
- Arii: verificare, dezvoltare, import
- Amendează: —
- Docs: `docs/scop.md` §Etapa curentă; `docs/decizii/restante.md`; CLAUDE.md §Stare

## Regula durabilă

**„Rotund” e declarat la 2026-10-17. PoC-ul s-a oprit: ce urmează e migrarea, cu decizie proprie, iar orice altă cerință pornește din registru, rejudecată pe modelul rotund.**

(a) **Condițiile.** Cele șapte condiții din 091 (g) și 120 (b) sunt îndeplinite: (1) și (3)–(6) prin 126 (a), (2) prin 126 (h) și (i), (7) prin 126 (j). Registrul nu mai are nicio restanță `activă`. Regulile feliilor care au dus acolo sunt 125–129.

(b) **Etapa următoare e migrarea** (091 f): conectorul 1C, deschiderea din balanță, împerecherile și reconcilierea, cu contract scris și decizie proprie. Un caz din date care contrazice catalogul devine scenariu nou plus decizie, nu normalizare (091-r5). Restanțele `migrare` din registru sunt intrarea ei.

(c) **Starea `activă` capătă obiect nou.** După „rotund”, `activă` înseamnă că restanța blochează închiderea etapei deschise; o etapă își declară restanțele active în contractul ei. Restanțele `după PoC` nu blochează nimic. Una intră într-o etapă numai prin contractul acelei etape, rejudecată pe modelul rotund (091 k).

(d) **Lanțul de migrații crește din migrația de bază.** O schimbare de schemă de după „rotund” adaugă o migrație incrementală peste `20261017101337_InitialCreate` (120 a). Lanțul nu se mai recomprimă: prima bază care trebuie păstrată vine din migrare, iar de atunci lanțul e istoric (120 d).

## Context

Firul „rotund” a pornit la 2026-10-17 din inventarul 126: 31 de restanțe `activă` și condițiile (2) și (7) neîndeplinite. Feliile R1–R7 au închis restanțele (127, 128, 129 și 126 d–g), au probat ciclul și lanțurile transversale (126 h, i) și au recomprimat migrațiile (126 j). Owner-ul a hotărât ca migrarea să vină după „rotund” (091 f).

## Tranșări

- Declararea are decizie proprie, nu o literă în 126: 126 e drumul (inventarul, retriajul, semantica cititorilor), iar declararea schimbă regula de lucru de după, adică (b)–(d).
- (c), nu scoaterea stării `activă`: registrul păstrează vocabularul (118), iar o etapă tot are nevoie de restanțe care o blochează.
- Probele în browser rămase (coloana `Consum`, culegerea cauzei pe LDI) nu țin de condițiile lui „rotund”; sunt numite în 130-r1.

## Ce rămâne deschis

- 130-r1: proba pe calea reală, în browser, a coloanei `Consum` din raportul de stoc (126 d) și a culegerii Cauză/Imputat pe o linie LDI reală (129 a). Baza hostului Privat are numai seed.

## Istoric

- 2026-10-17: scrisă la cererea owner-ului, după aprobarea deciziilor 125, 127 și 128; propusă owner-ului.
