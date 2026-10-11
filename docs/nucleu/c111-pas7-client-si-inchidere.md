# C111, pasul 7 — clientul, contractul HTTP și închiderea

**Data:** 2026-10-11. **Stare:** implementat și verificat pe catalog, pe HTTP
și în browser; review-ul advers pe diff-ul întreg al feliei a sosit:
C111-F-R1 e corectată, C111-F-R2 e la owner. Contractul: [C111](c111-coordonate-contract.md), D6, D9 și
C111-Q18…Q21; decizia 111.

Urmele, neversionate: `run-verificari/c111-pas7-http/` (rețeta
`c111-pas7-http.ps1`, proba `c111-pas7-date.py`, măsurătorile
`c111-pas7-*.py`), `run-verificari/20261011-122558-351/` (integrala).

## Ce s-a schimbat

**Clientul React, adus la forma postării.**

- `gestiuneId` a ieșit din starea de URL a rapoartelor; serverul nu-l mai avea
  de la pasul 2.
- „Sold parteneri” se numește „Solduri pe terți”: rând propriu au partenerii
  și angajații, restul contului stă în rândul „fără terț”, iar explicația
  ecranului spune asta.
- Balanța analitică și fișa numesc axa „Repartitor”; filtrul `repartitorNul`
  se numește „fără repartitor”.
- Registrul jurnal are coloana „Cont corespondent”.

**Fișa fără terț (C111-Q18).** `GET api/proiectii/fisa-cont` primește
`faraTert=true`: postările contului care nu stau pe un partener sau pe un
angajat, adică exact mulțimea rândului „fără terț” din soldurile pe terți.
Nu se combină cu `repartitorNul` sau cu `repartitorId` (400). Dublu-click-ul
pe rândul „fără terț” o deschide; înainte deschidea `repartitorNul`, care
arăta numai postările fără niciun repartitor.

**Sinteza fișei (C111-Q19).** `GET api/proiectii/fisa-cont/sold`, cu
parametrii fișei, întoarce soldul inițial, rulajele și soldul final ale
perioadei, din atomii balanței, pe aceleași filtre. Clientul le arată într-o
bandă deasupra grilei. O fișă fără mișcări în perioadă își arată astfel
soldul: rândul 302 al probei, cu mișcări numai în 2012, are fișa pe 2026
goală și sold inițial și final 125,00 D.

**Indexul perechii (C111-Q20).** `IX_Postare_Tranzactie_Pereche`, pe
`(TranzactieId, Pereche)`, înlocuiește `IX_Postare_TranzactieId` în
`InitialCreate`. Căutarea perechii prin indexul vechi citea toată tranzacția
pentru fiecare rând; tranzacția de deschidere a importului are 10.828 de
postări.

**Registrul jurnal, cu îmbinarea perechii în SQL (C111-Q20).** `Pereche`
rămâne nulabilă: nul înseamnă „fără pereche”. Din LINQ, egalitatea între două
coloane nulabile iese cu ramura „amândouă nule” sau, ocolită cu `??`, ca
expresie pe care indexul n-o poate folosi. Registrul jurnal compune acum
SQL-ul ca fișa: sursele din LINQ, îmbinarea perechii scrisă de mână, cu
egalitate simplă.

**Fișa fără terț, la volum.** Mulțimea repartitorilor externi e un CTE citit
de două ori (soldul inițial și rândurile). Materializat, scotea soldul
inițial din planul paralel și îi strica estimarea; e declarat
`NOT MATERIALIZED`.

## Măsurătorile

Toate pe calea produsului (`ModelCheck --perf-fisa`, citire integrală, cinci
rulări la cald, mediana), pe aceeași bază, înainte și după schimbarea stării.

Baza de import, `Atlas.Conta.Import1C.Flax.C111P6`, ianuarie 2025:

| Cititor | Rânduri | Indexul vechi | Indexul nou |
|---|---|---|---|
| fișa 371, contul întreg | 14.824 | 11.333 ms | 105–186 ms |
| fișa 371, pe repartitor | 3.081 | 6.023 ms | 58–70 ms |
| fișa 371, fără terț | 14.824 | 11.271 ms | 160–185 ms |
| registrul jurnal, îmbinarea din LINQ | 59.776 | 21,2 s (20,7 s în `EXPLAIN`) | 23,8 s, numai în `EXPLAIN` |
| registrul jurnal, îmbinarea în SQL | 59.776 | 5.044 ms | 246–293 ms |

Registrul jurnal livrează aceleași 59.776 de rânduri, din care 48.830 cu
cont corespondent, în toate stările. Pe celelalte conturi măsurate, cu
indexul nou: 401 între 46 și 88 ms, 5121 între 61 și 79 ms.

Baza de volum, `Atlas.Conta.ModelCheck.Privat.C111Vol` (5 milioane de
postări), ianuarie 2065, cu indexul nou:

| Cititor | Rânduri | Timp |
|---|---|---|
| fișa 401, contul întreg | 201.280 | 3.003 ms |
| fișa 401, pe repartitor | 320 | 158 ms (136 ms la pasul 6, prin PerfCub) |
| fișa 302, contul întreg | 161.653 | 3.114 ms |
| fișa 302, fără terț, CTE materializat | 161.653 | 8.131 ms |
| fișa 302, fără terț, CTE nematerializat | 161.653 | 3.330 ms |
| registrul jurnal | 888.148 | 6,6–8,0 s (6,9 s la pasul 6, prin PerfCub) |

La volum registrul jurnal nu s-a schimbat: tranzacțiile scenei sunt mici, iar
costul e căutarea perechii pe fiecare rând, nu tranzacția mare. Câștigul
indexului e pe tranzacțiile cu multe postări.

Scara PerfCub restrânsă la jurnal și fișă (`perf-cub.ps1 -Operatii JRN,FISA`,
baze noi `.C111Perf`), lângă rularea de la pasul 6: aceleași verificări (373
pe privat, 367 pe bugetar, zero FAIL), aceleași rânduri citite și livrate, iar
timpul SQL la treapta de sus, la cald, e în zgomot: registrul jurnal pe ruta
nesecurizată 6,9 → 6,2 ms pe privat și 5,5 → 5,1 ms pe bugetar, fișa pe
partener 6,8 → 6,8 ms și 6,7 → 5,9 ms. Cele două rulări sunt pe baze create
separat din aceeași scenă.

Varianta `Pereche` nenulă cu 0 pentru „fără pereche” a fost simulată pe baza
de import, într-o tranzacție anulată: cu excluderea lui 0 pe partea perechii,
202 ms; cu excluderea pe rândul curent, 129 s; fără excludere, 117 milioane
de rânduri. Nu s-a adoptat.

## Verificarea

| Probă | Rezultat |
|---|---|
| Integrala ModelCheck, bugetar | 3.673 OK, 0 FAIL |
| Integrala ModelCheck, privat | 5.075 OK, 0 FAIL |
| `refuzuri.ps1` pe bază privată nouă, de două ori | 327 / 327 PASS, baza neschimbată |
| `neexpunere-cub.py`, `explicatii.py` | verzi |
| Proba de date pe HTTP (`c111-pas7-date.py`) | 22 / 22 |
| PerfCub restrâns la jurnal și fișă | 373 privat / 367 bugetar OK, 0 FAIL |
| OpenAPI și tipurile clientului | regenerate, idempotente; clientul compilează |
| Browser | soldurile pe terți, fișa fără terț cu banda ei, fișa pe partener, registrul jurnal |

Scenele noi din catalog: SC-C111-31 (fișa fără terț) și SC-C111-32 (sinteza
fișei, dinamic și din snapshot). Matricea are nouă rânduri noi pe fișă și pe
sinteza ei.

Proba de date creează pe hostul privat o factură cu recepție și TVA, o notă
și o plată, peste documentele din 2012 lăsate de `explicatii.py`. Fiecare
rând din soldurile pe terți (opt, din care șase „fără terț”) e egal cu
ultimul sold curent al fișei lui și cu soldul final al sintezei, pe perioada
largă și pe anul curent; registrul jurnal are cont corespondent pe toate cele
20 de rânduri, câte postări cu pereche sunt în bază.

Prima rulare a matricei de pe binarul final a căzut înaintea primei probe:
hostul n-a putut deschide în 15 s o conexiune nouă spre Postgres-ul din
container, la încărcarea rolurilor. Rețeta folosea `localhost`; pe
`127.0.0.1` rularea următoare, de la zero, a trecut de două ori.

## Bazele

- Bazele de dezvoltare fără sufix (5444 și 5446) sunt recreate din
  `InitialCreate`; cea privată a ModelCheck se reface la prima rulare.
- Bazele de lucru `.C111P2` sunt recreate cu indexul nou.
- Pe baza de import `.C111P6` și pe cea de volum `.C111Vol` indexul a fost
  schimbat prin SQL, fără recreare (abatere numită de la 102 (b), C111-Q20).
- Bazele de referință `…C111P0` rămân pe schema veche.

## Review-ul advers al închiderii

Codex a citit diff-ul întreg (`main...c840765`) și a rulat integrala pe baze
proprii, cu aceleași cifre. Dispozițiile sunt în contract, sub „Review-ul
advers Codex al închiderii”.

- **C111-F-R1, corectată.** De la pasul 3, terțul documentului nu mai ajungea
  pe piciorul unui cont care admite lipsa repartitorului (437.01.00,
  437.02.00, 803.00.01): condiția „pune terțul” fusese strânsă la „contul nu
  admite lipsa”. Acum terțul se pune pe orice cont care declară feluri, ca
  pe `main` pe conturile cu flag-ul R. Proba e SC-C111-33; condiția veche,
  repusă, o aprinde. Integrala după corecție: 3.678 OK pe bugetar, 5.080 pe
  privat, zero FAIL (`run-verificari/20261011-135559-972/`). Matricea HTTP și
  proba din browser sunt cele de mai sus, de pe arborele dinaintea corecției.
- **C111-F-R2, la owner.** Sinteza fișei trece prin ușa securizată, ca fișa
  și balanța: un rol cu citire restrânsă pe `Postare` primește suma
  rândurilor lui, fără refuz. 110 (e) cere, la literă, citirea completă.

## Ce rămâne

- Tranșarea C111-F-R2 și reverificarea Codex a corecției; decizia 111 trece
  la „implementată” după ele.
- La volum, contrapartida din registrul jurnal costă tot circa 4 µs pe rând.
