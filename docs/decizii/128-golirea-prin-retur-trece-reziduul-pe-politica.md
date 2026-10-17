# 128 — Golirea lotului prin retur la furnizor trece reziduul pe politica de diferențe

- Data: 2026-10-17
- Stare: activă
- Execuție: firul „rotund”, felia R3 (2026-10-17), branch `rotund-r3`; ModelCheck integral verde pe ambele profiluri; owner-ul a aprobat decizia la 2026-10-17
- Arii: stoc, motor, politici
- Amendează: —
- Închide: T-r2
- Docs: SC-RLF-05, 13, 15, 16 din `docs/nucleu/scenarii/RLF.md`; SC-CIT-75, SC-CIT-98 din `docs/nucleu/scenarii/CITIRI.md`; probele D18-V2 (g) din `VerificaValoareIesire`; `docs/stare-curenta/stoc.md`, `motor.md`; contractul `docs/nucleu/tr-d7b-tipuri-ramase-contract.md`, T-D6

## Regula durabilă

**Returul la furnizor iese la valoarea hârtiei, iar lotul pe care îl golește nu păstrează valoare: reziduul trece pe politica de diferențe.**

(a) **Reziduul golirii.** Când o linie RLF aduce cantitatea lotului din gestiune la zero, valoarea rămasă (soldul lotului plus valoarea hârtiei și cota corecțiilor, pe liniile documentului în ordine) se scoate fără cantitate de pe lot. Reziduul pozitiv e minus neimputabil (`CauzaDiferentei.Neimputabila`), cel negativ e plus (`Plus`). Contul vine din `PoliticaDiferenta` RLF × clasă × cauză; lipsa ei e refuz cu `REZIDUU_POLITICA_LIPSA`. Motorul nu cunoaște conturile (29); seed-ul privat pune 6588 și 7588 pe clasele de stoc.

(b) **Reziduul e explicat.** `ValoareDeclarata` poartă `Reziduu` lângă `Corectie`; explicația persistată, DTO-ul ei și invariantul de acoperire (postarea fără cantitate de pe lot = `Corectie + Reziduu`) îl citesc.

(c) **Politica de diferențe intră în operand numai la cerere.** Declarantul care o folosește implementează `IDeclarantCuDiferente`; operandul o încarcă numai atunci, ca plafonul de interogări al celorlalte tipuri să nu crească.

## Context

T-D6 din contractul TR-D7b a lăsat reziduul pe lot (cantitate 0, valoare ≠ 0), contra 090 (j), până la TR-D9; pe Flax 19–56 de goliri pe lună, ±0,01. T-r2 era `activă` (091).

## Tranșări

- Cauzele existente (`Neimputabila`, `Plus`), nu una nouă: politica nu inventează cauze (099); semnul reziduului alege cauza, ca la NIR.
- Contul din `PoliticaDiferenta`, nu din `RegulaContare`: e același fel de fapt ca diferența de recepție, pe tip × clasă × cauză.
- Câmp propriu `Reziduu`, nu adunat în `Corectie`: cota corecțiilor e a CV (CV-D10), reziduul e alt fapt.

## Ce rămâne deschis

Nimic nou. Importul 1C va întâlni goliri cu reziduu pe RLF; ele trec acum pe 6588/7588 (de raportat la migrare, 091-r4).

## Istoric

- 2026-10-17: scrisă la felia R3 a firului „rotund”; propusă owner-ului.
- 2026-10-17: aprobată de owner; trece în `activă`.
