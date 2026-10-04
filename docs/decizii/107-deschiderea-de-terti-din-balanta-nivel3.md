# 107 — Migrarea deschide terții per partidă din `BalantaNivel3`, prin `Materializare.Deschide`; stingerile anului pe partide inițiale; partener generic de migrare

- Data: 2026-10-01
- Stare: activă, implementată și probată pe Flax 2026-10-01 (ianuarie integral, anul oprit după 8 luni la cererea owner-ului; regula de oprire M1-D10 neatinsă din cauze din afara M1: 107-r3, F26-r13, performanță); rebazată pe main `ac372fc` și reverificată pe ianuarie 2026-10-04 (INV-CUB verde cu invarianții 108, plafonarea verificată pe date); M1-D10 amendat de owner 2026-10-04 la deschidere exactă + ianuarie verde pe mecanism (rularea integrală 12/12 trece în restul 091-r4, după TR-D9); mersă în main prin PR #20 (2026-10-04); review advers Codex închis la RV2 (`docs/import/m1-rv2-review-codex.md`): M1-R1…R4 și M1-R3a (agregarea semnată) corectate și reverificate; prima felie a 091-r4 (M1); amendează 047 (soldul de terț NU e nedefalcat în 1C), 091 (f) (conectorul iese din îngheț pe deschidere și trecerea 2); închide T-r4, TR-r6 și FZ-r10 pe partea de date
- Docs: `docs/import/m1-deschidere-terti-contract.md` (M1-D1…D10, recensământul, execuția); 094 și DES-B1…B4; 090 (d); 092; 096; `docs/import/faza-1c-design.md` §3, §8, §12.2

## Regula durabilă

**Cutoff-ul migrării se bazează pe solduri, nu pe documente. Deschiderea
trece prin comanda motorului, o singură dată per bază, cu terții detaliați
per partidă din sursă; istoricul rămâne în sursă. Transferul datelor fără
motor, cu validare ulterioară, nu se face: ar fi al doilea motor și ar face
sursa canonică pe istoric (12, 18, 21, 35b, invariantul V).**

(a) **Deschiderea = `Materializare.Deschide` (094), scrisă de conector o dată
per bază**, pe prima rulare, înaintea oricărui document, în tranzacție
explicită. La re-rulare nu se rescrie: se verifică împotriva sursei, pe detaliu — fiecare
partidă și fiecare lot, în ambele sensuri, pe identitate și pe măsuri.
Ordinea în conector este loturi → cub → rânduri bloc de registru (regimul
dual, până la TR-D9): comanda refuză un lot cu mișcări în `RegistruStoc`.

(b) **Controalele sunt brute per (cont, latură).** Un cont cu partide de
ambele semne primește două solduri de control contra ancorei; rândurile bloc
ale registrului se scriu la fel, brut, ca invariantul `CITIRE_DESCHIDERE_
INCOMPLETA` să rămână verde fără atingere de `Module`. Contractul 1 al
reconcilierii (sold net per cont) nu se schimbă.

(c) **Partida inițială = poziția `(cont, partener, document de decontare)`
din `BalantaNivel3`**, cu contractul agregat (nu e identitate Atlas),
latura dată de semn, referința un Guid determinist din cheia 1C a
documentului (sau din (cont, partener) când sursa n-are document).
Identitatea partidei e a motorului (`Unitate.DeschidePartidaInitiala`);
legătura `1C:PartidaDeschidere` poartă cheia SURSEI, pe care contractul 5 o
rederivă lună de lună.

(d) **Pozițiile fără partener pe conturi urmărite intră pe un partener
generic de migrare** (`MIGRARE-NEDEFINIT`), creat de conector, nu de seed.
Soldul inițial nu se abate de la sursă; rezolvarea se face după import, prin
documente. Alternativa respinsă (scăderea din control cu diferență
declarată) lăsa soldurile inițiale diferite de sursă fără garanția că
informația mai apare (owner, 2026-10-01).

(e) **Stocul: controlul conturilor de stoc = Σ loturilor scrise în cub.**
Ce nu poate fi lot (grupe produs × depozit total-negative, poziții orfane,
celule cu valoare fără cantitate) rămâne diferență declarată a sursei,
purtată permanent în contractul 1 pe contul de stoc și pe ancoră.

(f) **Stingerile anului pe pozițiile de deschidere se scriu în cub prin
`Materializare.Imperecheaza` pe partidă**, în trecerea 2 a lunii, fără rând
`Imperechere`, cu legătură proprie. Retururile din anul anterior se sting pe
aceeași cale (motivul 46f era al documentelor, nu al partidelor). Sursa care
stinge peste restul poziției (pozițiile în valută, evaluate în lei la cursul
anului anterior și plătite la cursul zilei) se plafonează la rest, iar
excedentul se contorizează (precedentul S-r5 pe documente).

(g) **Data împerecherii este a faptului**: cea mai târzie dintre datele
celor două documente pe perechile de documente, data stingătorului pe
partida inițială. Ziua rulării nu mai e dată de împerechere.

(h) **Contractul 5 al reconcilierii**: restul fiecărei partide inițiale în
cub la fine de lună = soldul poziției în sursă la începutul lunii următoare,
corectat cu mișcarea cerută de sursă și neaplicată pe partidă. Mișcarea
neaplicată e semnată după sensul ei real (debit − credit), cumulează
refuzurile și plafonările și se rederivă din cub la reluare. O explică numai
refuzul nominalizat al motorului; o eroare tehnică a stingerii e eșec al
lunii, niciodată explicație. Partidele partenerului generic și cele pe care
trecerea 2 nu le-a atins deloc (stinse în sursă de un tip din afara trecerii
2: factura care consumă avansul pe 419/409, factura sosită pe 408, nota fără
partener) se declară cu sumă, nu pică; restul e FAIL al lunii.

(i) **Diferențele contractului 1 față de baseline-ul 2026-09-21 se judecă
contra modelului (091 r5), nu se ascund în conector**: rotunjirea TVA pe linie
(±0,03 pe sute de facturi), evaluarea RLF și consecința lor în închiderea de
TVA sunt ale motorului de pe main de după baseline, nu ale migrării, și se
raportează ca atare (107-r3).

(j) **Import1C rulează pe modelul cu stocul citit din cub numai cu
deschiderea în cub.** Pe main, fără (a), orice ieșire dintr-un lot de
deschidere refuză `STOC_INSUFICIENT`: conectorul înghețat de 091 (f) nu mai
putea rula; ieșirea din îngheț pe deschidere e condiția rulării, nu o
extindere.

## Context

Owner-ul a întrebat (2026-10-01) cum se face importul și ce ar însemna
transferul fără motor cu validare ulterioară. Analiza (contractul M1, §1):
un document operat lasă în urmă documentul pe frunza TPH, loturile, cele
patru registre, tranzacția și postările cubului, partidele, împerecherile cu
`Transfer`, proveniența stornoului; a le scrie direct înseamnă un al doilea
motor, iar validarea ulterioară (INV-CUB, `ReconciliereCub`, reconcilierea
1C, DUK) verifică doar consistența internă și egalitatea cu sursa, nu că
postările sunt cele ale motorului (091 c). Owner-ul a confirmat: „este
corect și rămâne așa; cutoff-ul se bazează pe solduri" și a cerut T-r4 și
TR-r6 acum.

Recensământul pe sursă și tranșările (D3 brut, D5 partener generic după
contraargumentul owner-ului, D7 retururi) sunt în contract, §1–2.

## Tranșări la implementare

- Data cubului = 01.01.2025 (perioada fiscală a anului există și e
  deschisă), rândurile bloc rămân datate 31.12.2024 ca până acum.
- Celulele cu valoare fără cantitate (47, 719,94 lei) nu pot fi lot în cub
  (`Deschide` cere cantitate) — rămân în registru și în diferența declarată
  (e).
- Recuperarea partenerului din antetul documentului de decontare (D5) a dat
  zero pe Flax: cele două documente sunt neoperate în 1C, fără partener.
- Refuzurile din prima rulare (305, Σ 1,93 M) erau toate pozițiile în valută
  de pe 401.2 (101 EUR, 1 USD): Atlas n-are nomenclator de valută, pozițiile
  intră în lei; plafonarea (f) le închide, iar nota de curs a sursei din luna
  următoare închide poziția și în sursă.

## Review advers (Codex, 2026-10-04)

Patru observații, toate confirmate și corectate în conector; detaliul,
probele și rularea de închidere sunt în contract, §4. M1-R1: reluarea
verifica numai sumele pe (cont, latură) și numerele, nu detaliul — de aici
precizarea din (a). M1-R2, M1-R3, M1-R4: explicațiile contractului 5 erau
numai în memorie, semnate după deschidere și alimentate și de erori tehnice —
de aici (h) rescris. Triajul partidelor rămase fără explicație a dat două
limite cu nume, 107-r9 și 107-r10. La RV1 a rămas M1-R3a: perechile cu sens
din sursă se agregă semnat, iar netul zero nu e sens necunoscut. RV2 a
închis-o și a închis review-ul, fără observații noi
(`docs/import/m1-rv2-review-codex.md`); limitele închiderii sunt în contract, §4.

## Ce rămâne deschis

- 107-r1 — stingerile prin facturi (avansul pe 419/409 consumat de factură,
  factura sosită pe 408, creditul de client consumat de factura următoare)
  nu intră în trecerea 2: partidele rămân „neatinse", declarate în contractul
  5; intră în handlerele de facturi la felia următoare a migrării.
- 107-r2 — rândurile de terț fără partener din anul importat rămân în punte;
  maparea lor pe partenerul generic ar închide partidele generice (461, 411.8).
- 107-r3 — drift-ul contractului 1 față de baseline (TVA ±0,03 pe linie, RLF,
  ITV): scenarii + decizie pe motor (091 r5), nu normalizare în conector.
  Owner, 2026-10-04: se tratează înaintea TR-D9.
- 107-r4 — valuta: partide inițiale în valută cu curs propriu, la TR-D9.
- 107-r6 — performanța importului: 25 → 50 min/lună față de 8–10 la baseline
  (~5×) la 2026-10-01; după gate-ul TR-D8 (108) ianuarie durează 21:27, tot
  ~2,5× baseline, iar creșterea de la o lună la alta nu e remăsurată. Scara
  sintetică X-D5 nu acoperă calea importului: de profilat înaintea oricărei
  rulări integrale.
- 107-r7 — ordinea stingerilor din aceeași compensare e nedeterministă între
  rulări (ianuarie: 42 apoi 41 partide neexplicate, Σ diferită pe 401); de
  ordonat cronologic, cu dată, în trecerea 2.
- 107-r8 — nota-punte a vânzării cu valoare pe 3 zecimale (iunie, 122,408)
  refuzată de gardianul de scară: rotunjirea la bani în handler, cu
  divergența declarată.
- 107-r9 — documentele care mișcă direct partida inițială la operare (Compensare,
  Operatia transcrise ca notă cu partida nominalizată) nu trec prin trecerea 2;
  diferența lor față de sursă rămâne FAIL în contractul 5, etichetată cu Σ
  mișcată direct.
- 107-r10 — încasările inline ale retailului (`RaportDeVanzariCuAmanunt`,
  copiii `#inc`) nu sunt enumerate de trecerea 2: stingerea lor pe o poziție de
  deschidere nu ajunge pe partida inițială.
- 107-r5 — partenerul generic la go-live: procedura de rezolvare (NTC de
  reclasificare) și excluderea din D394/SAF-T ca decizie de produs.
