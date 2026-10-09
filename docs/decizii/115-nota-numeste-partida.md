# 115 — Linia de notă poate numi partida pe care o stinge; partida inițială își poartă data de naștere

- Data: 2026-10-09
- Stare: **activă**; hotărâtă de owner 2026-10-09 (nominalizarea din sursă se păstrează; fără ea, conectorul dă o cheie stabilă de ordonare). Precizează 090 (e) și 107; tratează 107-r9 pe compensări și operații; restrânge 114-r1.
- Docs: `docs/stare-curenta/domeniu-si-operare.md`, `dezvoltare-si-validare.md`; catalogul `docs/nucleu/scenarii/NTC.md` (SC-NTC-23…26) și `CITIRI.md` (SC-CIT-112).

## Regula durabilă

**O linie de notă contabilă poate numi, pe fiecare latură, partida pe care o stinge. Fără nume, alege FIFO, iar ordinea FIFO a partidelor inițiale e data la care s-au născut la sursă.**

(a) **Partida numită.** `NotaContabilaDetaliu.PartidaDebitId` și `PartidaCreditId` poartă identificatorul unității. Latura cu partidă numită stinge numai acea partidă, în sensul stingerii și până la restul ei. Motorul o citește prin `ILinieCuPartidaNumita`, nu prin tipul liniei.

(b) **Lipsa nu cade pe FIFO.** Ce nu acoperă partida numită deschide partida proprie a notei, ca pinul de lot (37d).

(c) **Refuzul.** Partida numită trebuie să existe la data notei pe contul și partenerul laturii; altfel `PARTIDA_NUMITA_INVALIDA`, fără efecte.

(d) **Partida inițială își poartă data de naștere.** `PartidaInitiala.Deschisa` e data la care partida s-a născut la sursă; fără ea, data deschiderii. Nu poate urma deschiderii. Identificatorul partidei nu depinde de ea. FIFO o folosește ca primă cheie (114 (a)), deci partidele inițiale se consumă în ordinea vechimii.

(e) **Contractul conectorului.** Conectorul de import numește partida când sursa o numește; când n-o numește, dă cheia de ordonare prin (d). Import1C numește partida din subconto-ul „Documente” al rândului, pe compensări și pe operații, și dă partidei inițiale data documentului 1C.

(f) **Câmpurile nu sunt în culegere.** Sunt ascunse în XAF și lipsesc din `WriteDto`; le scrie conectorul. Expunerea lor în culegere e o decizie separată.

## Context

După 114, două importuri ale aceluiași binar dădeau aceleași împerecheri între documente, dar 45 de stingeri pe partide inițiale diferite: partidele inițiale n-au document deschizător, erau toate datate 01.01.2025 și se departajau pe un identificator care se schimbă la fiecare import (114-r1).

1C numește documentul stins în subconto-ul rândului. Compensarea `SED00000030` numește cinci facturi, printre care 305,46 lei pe `ITD00170454`. Conectorul o transcria ca notă contabilă fără această informație, iar motorul alegea FIFO (107-r9). Stingerile exacte venite apoi din 1C erau refuzate pe partidele deja atinse.

## Tranșări

1. **Nominalizarea pe linia de notă** (aleasă, owner). Păstrează ce spune sursa și folosește primitiva existentă: un singur candidat, restul pe partida proprie.
2. **Cheia de ordonare ca dată de naștere a partidei** (aleasă ca rezervă). E aceeași formă ca la loturile inițiale, care își poartă deja data reală, și nu cere schemă. Nu departajează două partide inițiale ale aceluiași partener, pe același cont, născute în aceeași zi.
3. **O cheie de ordonare persistată pe postare.** Respinsă acum: ar fi o coloană nouă pe `Postare`, iar coordonatele postării nu se ating cât 111 e propusă.
4. **Trecerea compensărilor prin trecerea 2.** Nealeasă: nota mișcă partida la operare, deci stingerea ulterioară n-ar avea ce muta.

## Probe

- ModelCheck integral: 3.571 bugetar / 4.852 privat OK, zero FAIL (`run-verificari/20261009-114027-243/`), cu SC-NTC-23…26 și SC-CIT-112.
- Import1C pe ianuarie 2025, față de starea de după 114:

| | după 114 | cu 115 |
|---|---|---|
| Verificări picate | 15 | 10 |
| Laturi de notă cu partida numită de sursă | 0 | 546 |
| Partide inițiale cu refuzuri | 276–279 | 16 |
| Stingeri pe partide inițiale, chei diferite între două rulări | 45 | 0 |
| Linii diferite în raportul de reconciliere între două rulări | 24 | 4 |
| Tranzacții și postări, între două rulări | diferite | aceleași (18.770 / 81.774) |

Numai cu (a)–(c), fără (d), rămâneau 11 linii diferite în raport.

## Ce rămâne deschis

- **114-r1** rămâne, restrânsă: două partide inițiale ale aceluiași partener, pe același cont, născute în aceeași zi se departajează tot pe identificator. Pe ianuarie 2025 a rămas un caz: 60 lei care cad pe una sau pe alta dintre două facturi ale aceluiași furnizor. Închiderea lui cere o cheie de ordonare persistată (tranșarea 3).
- **107-r9** rămâne pe rândurile fără document numit în subconto.
- **115-r1** — nominalizarea partidei nu e în culegere (XAF, API).
