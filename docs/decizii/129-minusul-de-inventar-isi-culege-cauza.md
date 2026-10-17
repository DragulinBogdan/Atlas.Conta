# 129 — Minusul de inventar își culege cauza: cheltuiala rămâne pe tip, imputarea merge pe politica de diferențe

- Data: 2026-10-17
- Stare: activă
- Execuție: firul „rotund”, felia R4 (2026-10-17), branch `rotund-r4`; răspunsurile owner-ului la 2026-10-17; ModelCheck integral verde pe ambele profiluri; owner-ul a aprobat decizia la 2026-10-17
- Arii: stoc, politici, documente
- Amendează: —
- Închide: 099-r1
- Docs: SC-LDI-23, SC-LDI-24 din `docs/nucleu/scenarii/LDI.md`; `docs/stare-curenta/stoc.md`, `limite-curente.md`

## Regula durabilă

**Linia de minus a listei de inventar spune de ce lipsește marfa; cauza alege contul numai unde regula de contare nu o poate face.**

(a) **Cauza pe linie.** `ListaDiferenteInventarDetaliu` culege `CauzaDiferentei` și imputatul (`PartenerDiferentaId`), pe coloanele partajate cu NIR. Minusul fără cauză e neimputabil; pe minus sunt permise Perisabilitate, Neimputabilă și Imputabilă, pe plus numai Plus. Culegerea golește cauza pe plus și imputatul în afara imputabilei; refuzurile sunt `INVENTAR_CAUZA` și `INVENTAR_POLITICA`.

(b) **Cheltuiala și plusul rămân pe regula de contare.** Perisabilitatea și neimputabila se contează pe cheltuiala derivată din tipul liniei (6xx din 3xx), plusul pe venitul regulii (7588 / 791), ca înainte.

(c) **Imputarea merge pe politica de diferențe.** Imputabila ia contul din `PoliticaDiferenta` LDI × clasă × Imputabilă (privat 461; bugetar 461.01.09, iar 428.01.02 pe angajat), cu partidă proprie pe imputat; contul trebuie să urmărească partide. Imputatul e partener sau angajat.

(d) **Custodia pe bugetar e limită declarată.** Inventarul refuză Custodie (`INVENTAR_STOC_NEACOPERIT`), iar clasa MC n-are tipuri de material; T-r6 trece „după PoC”.

## Context

099-r1 era `activă` (091): lista de inventar nu culegea cauza, deci minusul nu putea fi imputat. Owner-ul a tranșat la 2026-10-17: minusul fără cauză e neimputabil, conturile ca la NIR, plusul neschimbat, custodia „după PoC”.

## Tranșări

- Abatere de la „conturile ca la NIR” pe cheltuială, cu motiv: maparea NIR e pe clasă (de ex. OF → 603.00.00 pe bugetar), iar LDI conta pe tip (303.02.00 → 603.02.00), la fel ca BCS pe lanțul Folosință (093). Mutarea pe clasă ar fi schimbat contul și ar fi rupt lanțul. Pe imputare maparea NIR se aplică întocmai. Răspunsul inițial al owner-ului („conturile minusului = maparea NIR pe ambele profiluri”, „plusul mutat în politica de diferențe”) a fost depășit de această tranșare; owner-ul a confirmat abaterea la 2026-10-17.
- Plusul nu trece pe politica de diferențe: aceleași conturi stau deja în regula de contare, iar mutarea n-ar adăuga nimic.

## Ce rămâne deschis

099-r2 (efectele fiscale pe cauză) rămâne „după PoC”. T-r6 trece „după PoC” prin (d).

## Istoric

- 2026-10-17: scrisă la felia R4 a firului „rotund”; propusă owner-ului.
- 2026-10-17: owner-ul confirmă abaterea de la răspunsul inițial (cheltuiala pe tip, plusul pe regula de contare).
- 2026-10-17: aprobată de owner; trece în `activă`.
