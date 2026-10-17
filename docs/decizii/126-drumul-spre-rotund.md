# 126 — Drumul spre „rotund”: ce rămâne din 091 (g), restanțele închise sau retrase din lista activă și semantica cititorilor de stoc

- Data: 2026-10-17
- Stare: propusă
- Execuție: firul „rotund”, felia R0 (2026-10-17), branch `rotund-r0`; inventarul făcut de trei agenți doar-citire și verificat de main pe cod; owner-ul a tranșat retriajul și (d)–(f) la 2026-10-17
- Arii: verificare, stoc, motor, imobilizari, partide, tva, perioade
- Amendează: —
- Închide: 091-r2, 76-r2, N-r7, TR-r7, TR-r10, TR-r11, 095-r1, X-r3, NG-r1, T-r5, 86-r11, 091-r6, S-r4
- Docs: `docs/decizii/restante.md`; `docs/nucleu/scenarii/IMO.md`; testul `FifoTeste.OrdineaGuidEsteOrdineaUuidDinPostgres`; SC-CIT-117, SC-CIT-118 din `docs/nucleu/scenarii/CITIRI.md`; `docs/stare-curenta/stoc.md`, `raportare.md`

## Regula durabilă

**„Rotund” se declară după condițiile (2) și (7) din 091 (g), plus restanțele rămase `activă`; restul registrului nu-l mai blochează.**

(a) **Condițiile (1) și (3)–(6) se îndeplinesc fără felie nouă:** (1) toate cele 15 tipuri declară prin `Document.Declarant()`, iar tipul fără declarant e refuzat cu `TIP_FARA_DECLARATIE`; (3) prin 119; (4) prin X-D6; (5) prin 091-r3 și N-r8; (6) prin 110. Condiția (2), ciclul obligatoriu în 8 rânduri pe BCS, FCT, PLT, INC, BTR, FCL și DSC plus lanțurile transversale (091-r6), se probează prin (h) și (i); condiția (7), recomprimarea migrațiilor (120 b), se face ultima, prin (j).

(b) **Restanțe închise ca făcute sau ca regulă:** 091-r2 (recensămintele sunt în `docs/nucleu/scenarii/recensamant-*.sql`, cifrele în fișierele tipurilor); 76-r2 (`AsignatFataDe` citește un singur sens din cub); N-r7 (premisa e falsă: `Guid.CompareTo` dă ordinea octeților RFC, adică `ORDER BY uuid`, probat de `OrdineaGuidEsteOrdineaUuidDinPostgres`); TR-r7 (Folosință prin 093, injectivitatea SAF-T prin D17-D1 și 105 h, Custodie în T-r6, Gratuit fără date pe Flax); TR-r10 (SC-DES verzi pe ambele profiluri, restul SC-X-11 în 091-r6); TR-r11 (atributele documentului scrise la operare sunt regula, nu restanță); 095-r1 (SC-IMO verzi pe ambele profiluri, corecturile 098b/c comise în a82a2416).

(c) **Trecute în „după PoC”** (nu blochează „rotund”): 75-r4, 76-r1, 76-r3, F28-r1, F27-r16, 103-r2, 103-r3, 097-r2, TR-r8.

(d) **Raportul de stoc arată și consumul pe lot.** Poziția lăsată de consum pe contul de cheltuială, la locul de consum, cu lotul ca unitate (X-r3), apare în raportul de stoc și în soldul pe lot, marcată ca poziție de consum, nu ca stoc disponibil.

(e) **Vederile pe repartitor sau material urmează valoarea la destinație.** Atomii balanței și ai snapshot-ului contabil includ `Transfer` și inversele lui (BTR, ASM pe același cont), prin cititorul comun `Contabil.Transferuri`; balanța analitică, balanța, fișa și soldul pe partener filtrate pe repartitor sau material arată deci valoarea mutată. Transferul conservă pe cont și se scrie ca debite de semn opus (090 f), deci balanța sintetică și rulajele pe cont rămân neschimbate. Listările (jurnalul, fișa nefiltrată) rămân fără transferuri; fișa filtrată pe repartitor sau material le listează (NG-r1).

(f) **Lotul nu se naște cu valoare negativă.** O linie care naște lot cu cantitate pozitivă și valoare negativă e refuzată cu cod stabil; returul sau reducerea pe același document se fac prin RLF sau prin corecția valorică (119) (S-r7).

(g) **Declarația vamală rămâne evidență fiscală.** Datoria din vamă e sold de cont: DVI creditează contul implicit al predatorului (446 pe vamă, fără partide); partida apare numai pe un comisionar vamal cu 401, unde contul urmărește partide, și se stinge prin notă sau plată pe FIFO (SC-DVI-17), fără împerechere pe DVI (DVI-D4). Capitalizarea taxei vamale pe lot (86-r1), refuzul anulării facturii legate de o DVI operată (86-r2) și lanțul SC-X-13 trec „după PoC” și ies din condiția (2) a lui „rotund”.

(h) **Rândul 8 al ciclului se probează la sursă.** Orice sold citit după un pas al unei scene (`SoldLot`, `SoldPartida`, `Sold` la BCS) e comparat cu cititorii de producție (`Loturi.Cumulate`, `Partide.Cumulate`, citire integrală și vizibilă) și cu fișa și balanța pe cheia unității la acea dată. Cititorii nu se probează separat, pe scene paralele, ci pe pașii ciclului fiecărui tip.

(i) **Condiția (2) din 091 (g) e îndeplinită.** Ciclul obligatoriu în 8 rânduri e probat pe BCS, FCT, PLT, INC, BTR, FCL și DSC, pe profilurile unde tipul există, iar lanțurile transversale SC-X-01, 03, 08, 10 și 11 sunt realizate (SC-X-13 e după PoC, prin (g)). Stornoul se judecă pe sold (025d), nu pe dependență: SC-X-08 probează ambele ramuri. Dry-run-ul refuză la fel ca operarea pe toate refuzurile de operare din scene.

(j) **Condiția (7) e îndeplinită.** Lanțul de 15 migrații e recomprimat în `20261017101337_InitialCreate`, cu SQL-ul nedeclarat de model în `Migrations/Sql` (120 e) și proba de schemă identică (120 c). S-r4 se închide ca regulă: divergența cheii `Postare` (EF `ID`, baza `(Spatiu, ID)`) e declarată și probată de `STR-SCHEMA`, iar migrațiile pe `Postare` se scriu în SQL; recomprimarea a scos singura abatere (`RenameColumn` din `SensPePostare`).

## Context

Firul de performanță a importului s-a închis la 2026-10-16 (125), iar owner-ul a hotărât ca migrarea să rămână după „rotund” (091 f). Lista `activă` avea 31 de rânduri; o parte erau stări vechi (corecturi comise de atunci), regule de design trecute ca restanțe sau performanță pe care 091 (k) o trece explicit „după PoC”.

## Tranșări

- Retriajul celor 9, owner, 2026-10-17: toate în „după PoC”. Familia hook-urilor de stingere (90 e) rămâne numită; sumele lor vin deja din cub.
- T-r5, owner: „sold de cont, fără partidă” (DVI-D4). Codul o făcea deja pe vamă: 446 nu urmărește partide; numirea pe 401 e regula contului, nu a DVI.
- 86-r1, owner: „după PoC”; 86-r2 și SC-X-13 urmează, fiindcă refuzul anulării devine necesar abia când DVI atinge loturile.
- X-r3, owner: „și consumul”, nu numai conturile de stoc; alternativa unității nule pe capătul de consum ar fi pierdut urma lotului pe cheltuială.
- NG-r1, owner: filtrul include `Transfer`; alternativa (balanța strict contabilă, vederea pe gestiune doar din mișcările de gestiune) a fost respinsă.
- S-r7, owner: refuz; pe Flax sunt 325 de linii negative, care devin întrebări de migrare.

## Ce rămâne deschis

Nimic. Restanțele `activă` de la inventar s-au închis în feliile R1–R7 (127, 128, 129 și literele (d)–(j)); ce a rămas din registru e `după PoC` sau `migrare`. Declararea e decizia 130.

## Istoric

- 2026-10-17: curățată la declararea lui „rotund” (130): (a) și (i) fără stările tranzitorii, „Ce rămâne deschis” golit.
- 2026-10-17: scrisă la felia R0 a firului „rotund”; propusă owner-ului.
- 2026-10-17: (j) adăugată la felia R7; S-r4 închisă; toate condițiile din 091 (g) și 120 (b) îndeplinite.
- 2026-10-17: (i) adăugată la feliile R6b–R6c; 091-r6 închisă. Owner: SC-X-08 pe sold, SC-X-03 fără RDC.
- 2026-10-17: (h) adăugată la felia R6a: 667 de verificări pe pas (286 bugetar, 381 privat), verzi.
- 2026-10-17: (g) adăugată la felia R5, după răspunsurile owner-ului pe DVI.
- 2026-10-17: (d) și (e) implementate la felia R2. (e) precizată: transferurile intră în atomii balanței și în snapshot, deci și în balanța analitică și în soldul pe partener; o primă variantă care le citea din cub, fără snapshot, a picat scara PerfCub (X-D5, citirea depindea de istoric). Transferul nu schimbă rulajele sintetice, fiindcă e scris ca debite de semn opus pe același cont. Probat de SC-CIT-118.
