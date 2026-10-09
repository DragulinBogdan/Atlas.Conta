# 114 — Departajarea FIFO: data, documentul deschizător, apoi identificatorul

- Data: 2026-10-09
- Stare: **activă, amendată de 116** ((a): între unitățile fără document departajează postarea de deschidere; (d): postările unei tranzacții au ID-urile în ordinea ei); regula din motor hotărâtă de owner 2026-10-09. Precizează 013 și N-D7 (090 (e)); tratează parțial 107-r11. 114-r1 e închisă prin 116.
- Docs: `docs/stare-curenta/domeniu-si-operare.md`; catalogul `docs/nucleu/scenarii/NTC.md` (SC-NTC-09); `docs/nucleu/tr-d9b-pas0-masuratori.md` §7.

## Regula durabilă

**La aceeași dată de deschidere, FIFO consumă întâi unitatea deschisă de documentul creat mai devreme.**

(a) **Ordinea FIFO** a unităților este: data deschiderii, apoi ID-ul documentului deschizător, apoi identificatorul unității. Unitatea fără document deschizător (partida inițială, lotul fără linie de intrare) vine prima la data ei.

(b) **Partide.** Documentul deschizător e cel a cărui identitate (092) dă identificatorul partidei. Nucleul îl primește pe candidat (`Disponibil.Origine`), nu pe unitate: identitatea unității nu se schimbă.

(c) **Loturi.** Documentul deschizător e cel al liniei de intrare (`Lot.LinieIntrareId`); între document și identificator stă poziția liniei. Ordonarea e una singură, `Cub.Citiri.Loturi.InOrdineFifo`, folosită de descărcarea de gestiune și de cititorul loturilor disponibile; conectorul Import1C ordonează pe aceleași chei.

(d) **ID-ul documentului e Guid v7**, deci ordinea lui e ordinea creării. Ține între tranzacții. Rândurile scrise în aceeași tranzacție n-au ordine în ID, de aceea ID-ul liniei, al postării sau al lotului nu departajează decât ca ultim criteriu.

(e) **Pinul rămâne înaintea FIFO** (N-D7): unitatea numită se consumă întâi și nu cade pe FIFO când nu ajunge.

## Context

Restanța 107-r11: același binar Import1C, pe aceeași sursă, dădea alt număr de tranzacții, de postări și de partide inițiale la fiecare rulare, deci contractul 5 nu putea atribui o diferență unei schimbări de cod.

Cauza, urmărită pe un caz (bazele `.Flax.ProfA` și `.ProfE`, același binar de pe `3a19ab6`): FIFO-ul pe partide departaja partidele deschise în aceeași zi după identificatorul partidei (`Fapte.PartideDisponibile`, `ThenBy(p => p.Unitate.Id)`). Identificatorul e hash peste document, cont și partener (092), iar ID-urile se generează din nou la fiecare import. Un furnizor are opt facturi din 08.01.2025; compensarea `SED00000030` a consumat 333,85 lei din `ITD00170487`, `170488` și `170537` într-o rulare și din `ITD00170454` în cealaltă. Plata `SED00000099-48`, care vine cu sumele exacte din 1C, a fost apoi refuzată pe facturile deja atinse: trei refuzuri într-o rulare, unul în cealaltă.

Candidatul notat la pasul 0 al TR-D9b (`OrderBy(d => d.ID)`) nu e cauza: e în `Materializare.StingereDeschidere.cs` și ordonează numai blocarea rândurilor. Interogările sursei 1C sunt ordonate.

Măsurat pe ianuarie 2025:

- 1.126 de grupe cont × partener × zi au mai mult de o partidă deschisă de documente (3.187 de partide);
- din 16.597 de documente, niciunul nu împarte milisecunda de creare cu altul;
- din 5.094 de perechi de linii vecine create în aceeași milisecundă, 2.559 au ID-urile inversate față de poziție;
- 127 din 322 de perechi de loturi cu același produs, gestiune și zi sunt create în aceeași milisecundă;
- cele 3.967 de partide inițiale au toate data 01.01.2025, sunt scrise într-o singură tranzacție, iar 3.354 stau în grupe cu egalitate.

## Tranșări

1. **ID-ul documentului deschizător** (aleasă, owner). E stabil față de ce are baza și nu cere schemă.
2. **Numărul documentului.** Nealeasă: nu e unic între tipuri și nu spune ordinea intrării în evidență.
3. **Documentul pe unitate, nu pe candidat.** Respinsă: `Unitate` e valoare cu egalitate pe toate câmpurile și e coordonată a postării; un câmp nou i-ar schimba identitatea în tot motorul.
4. **Momentul scrierii tranzacției deschizătoare.** Nealeasă: tranzacția de deschidere scrie toate partidele inițiale odată, fără ordinal pe rând, deci nu le-ar departaja nici ea.

## Probe

- Nucleul: 191 de teste; `LaAceeasiDataOrigineaBateId`, iar proprietatea FIFO generează și origini.
- ModelCheck integral: 3.556 bugetar / 4.837 privat OK, zero FAIL (`run-verificari/20261009-102009-746/`). SC-NTC-09 fixa vechea departajare și a fost adusă la regulă.
- Două importuri pe ianuarie cu același binar (`.Flax.ProfF1`, `.ProfF2`): împerecherile între documente sunt aceleași (2.132, zero chei diferite; înainte 13 chei diferite). Conturile pe zi și stocul sunt identice pe chei naturale, ca înainte.

## Ce rămâne deschis

- **114-r1** — unitățile fără document deschizător se departajează tot pe identificator. La import, identificatorul partidei inițiale se schimbă la fiecare rulare, fiindcă include ID-urile de cont și de partener. Între cele două importuri de probă au rămas 45 de stingeri pe partide inițiale diferite și 24 de linii diferite în raportul de reconciliere, toate în contractul 5. Căi: păstrarea nominalizării din 1C pe compensări și operații (1C numește factura în subconto; vezi 107-r9) sau o cheie stabilă a partidei inițiale dată de conector. 107-r11 rămâne deschisă până atunci. Ambele căi sunt luate prin 115 (2026-10-09), care restrânge restanța la două partide inițiale ale aceluiași partener, pe același cont, născute în aceeași zi și nenumite de sursă. Restul e închis prin 116 (2026-10-09).
