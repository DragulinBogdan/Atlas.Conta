# 108 — Gate-ul transversal TR-D8: registrele nu se mai citesc, scrierea e serială per bază, explicația deciziei e persistată

Data: 2026-10-04
Stare: activă; concretizează 090 (i)(j) și 091 (g)(4)(5); extinde 101 pe ramura negativă a desfacerii (102-r4)
Docs: `docs/nucleu/tr-d8-transversal-contract.md` (X-D1…X-D8, X-Q1…X-Q4, X-RV1…X-RV7, „Execuție” pașii 1–6); `docs/nucleu/scenarii/CITIRI.md`; `docs/api/p5-perf-masuratori.md` („TR-D8 gate-ul transversal”)

**Starea verificării, 2026-10-04:** pașii 1–6 din X-D8 sunt implementați și
verificați pe `tr-d8-transversal`. Review-ul advers al contractului este
închis (X-RV1…X-RV7). [Review-ul advers al implementării](../nucleu/tr-d8-transversal-implementare-review-codex.md)
la `2b57aa5` a adus patru observații P2, X-RI1…X-RI4. Toate sunt corectate
(contractul, „Corecturile review-ului implementării”); integrala după
corecturi: 3.394 bugetar / 4.604 privat OK. Codex a reverificat independent
la `7223abc`, cu aceleași rezultate pe ambele profiluri, și a închis
X-RI1…X-RI4. Review-ul feliei este închis; branch-ul nu este mers.

## Regula durabilă

**(a) Registrele se ating în producție numai prin lista nominală (X-D2).**
`RegistruContabil`, `RegistruStoc`, `RegistruTva`, `RegistruImobilizari` și
`Imperechere` nu sunt sursă de sold, rulaj sau rest pentru niciun cititor de
producție. O intrare permisă numește utilizarea: fișierul, membrul,
registrul și rolul. Apartenența la un serviciu deja permis nu absolvă un
membru nou. Clasele: scriitor al regimului dual, martor și diagnostic,
evidență XAF, plus maparea, autorizarea și legătura (`Imperechere`).
Scriitorii duali și evidența XAF cad la TR-D9.

**(b) Regimul `Transfer` se declară pe intrarea comună, o singură dată
(N-r8).** `Contabil` și `Plati` exclud `Transfer` și inversa lui; `Loturi` și
`Partide` le includ; `Fiscale` și `Imobilizari.PozitiiFaraFisa` au domeniul
dat de coordonate. Niciun consumator nu refiltrează felul. Jurnalul și fișa
de cont nu listează transferul; o listare pe unitate îl arată cu felul din
`Tranzactie.Fel`. Contraponderile Transformare nu ajung prin nicio intrare
comună (T-r14).

**(c) Explicația unui contract se persistă o singură dată (X-D4, X-Q1 = A).**
Stă pe prima tranzacție efectiv produsă de contract, în `Tranzactie.Explicatie`
(jsonb versionat, append-only, scris odată cu rândul). A doua tranzacție a
aceleiași declarații o referă prin `ExplicatieDinId`. Stornoul nu are
explicație proprie: a lui este a originalului. Împerecherea, desfacerea,
stingerea de deschidere și deschiderea nu au decizii de explicat. O ieșire a
cărei valoare nu vine din sold poartă `ValoareDeclarata`, cu sursa numită.
Mecanismul e al declarantului, nu al explicației: declarantul spune dacă
evaluează din sold sau declară valoarea și din ce sursă, iar invariantul îl
ia din document. O partidă stinsă FIFO își are soldul citit între ipoteze.
Dry-run-ul nu persistă nimic. Acoperirea explicației este invariant
`INV-CUB`, cu mutant pe fiecare ramură.

**(d) Explicația se citește numai cu acces complet (X-RV2).** Tranzacția
invizibilă dă 404, ca una inexistentă. Pe tranzacția vizibilă, orice criteriu
de rând sau de membru pe tipurile citite dă 403 `EXPLICATIE_ACCES_INCOMPLET`
înaintea citirii, fără nicio valoare. Nu există proiecție parțială.
Mecanismul este `Api.AccesComplet`, comun cu SAF-T (105 b).

**(e) Scrierea în cub este serială per bază (X-D6, nivelul 3).** O comandă
care scrie ia la intrare un singur blocaj, cu cheie constantă, ținut până la
sfârșitul tranzacției. Blocajul protejează citirea care decide valoarea, nu
doar scrierea: se ia înaintea planului registrelor, a contractului și a
oricărui alt blocaj. Ordinea e „scrierea, apoi restul”, deci nu există
cicluri. Citirile și dry-run-ul nu îl iau niciodată. Salvarea care adaugă
detalii fără poziție îl ia. Cheia stă într-un singur loc
(`TranzactieComanda`), iar producția nu deschide tranzacții pe alături.

**(f) Așteptarea peste timpul comenzii este refuz, nu eroare.**
`SCRIERE_OCUPATA` iese 422 pe ușa HTTP, fără nimic scris, și cererea se poate
retrimite.

**(g) Acoperirea cubului se probează pe fapte, în ModelCheck, înaintea
purjei (X-D7 a, 102d).** Contabilul, stocul cantitativ, deschiderea,
partidele, fișele și explicațiile au fiecare o verificare în
`Invarianti.Verifica`. Stocul se compară pe grup și proveniență, nu pe rând:
recepția conexă aparține în registre NIR-ului și în cub facturii. Lipsa este
`CITIRE_ISTORIC_STOC_INCOMPLET`. Un transfer persistat conservă valoarea pe
(cont, latură) și cantitatea pe (cont, produs), ca la contractare (090f);
altfel `CITIRE_TRANSFER_NECONSERVAT`. Nicio verificare nu rulează la pornirea
hosturilor.

**(h) `PosteazaInCub` nu se stinge (X-Q4 = A).** După prima tranzacție în cub
a unui tip, gardianul refuză trecerea pe `false`
(`POSTEAZA_IN_CUB_IREVERSIBIL`), iar seed-ul nu o aliniază.

**(i) Împerecherea și desfacerea fără efect pe partidă sunt refuz (X-Q3 =
A).** `IMPERECHERE_FARA_EFECT` se aruncă și pe suma negativă a desfacerii
automate (101, 102-r4).

**(j) Reconcilierea (a)–(g) are toleranță zero pe o bază întreagă (X-D3).**
Cubul și registrele sunt scrise de aceleași comenzi, deci un rând Δ este
defect, nu declarație. Litera (h) și diagnosticul ASM-B7 se raportează cu
proveniență; nimic nu se declară N-r3 automat. Întrebarea „cine are
dreptate” pe datele 1C rămâne a migrării (091-r4).

**(k) Perf se judecă pe formă, nu pe prag absolut (X-Q2 = A).** Criteriile:
comenzi SQL constante în volum și istoric; pentru rutele care pornesc din
snapshot sau citesc un interval, accesul la `Postare` independent de istoric,
probat din plan fără scanare secvențială; liniaritate la cald; control
numeric cu oracol independent de cititori. Un criteriu picat lasă gate-ul
deschis. Amânarea este amendament explicit al owner-ului, cu cititorul,
criteriul, cifra și restanța numite. O materializare nouă cere cifră
măsurată (090 i). Scara se măsoară în container. Absența probei nu e probă:
un plan respins invalidează măsurarea, iar controlul numeric compară
rezultatul cititorului măsurat, nu al altuia.

## Context

Consumatorii din inventarul TR-D8 erau portați nominal, raport cu raport.
Lipsea ce niciun raport nu poartă singur: dovada că producția nu mai citește
registrele, reconcilierea pe o bază întreagă, explicația persistată (S-r2),
concurența pe două conexiuni reale, costul cititorilor pe scară și probele de
acoperire ale regimului dual. Contractul le-a fixat ca X-D2…X-D7, cu
integrala și review-ul ca X-D8. Derivările, cifrele și probele rămân în
contract; decizia adună regula.

## Tranșările owner-ului

- X-Q1 = A: explicația ca `jsonb` pe `Tranzactie`, cu cititor și `GET`.
- X-Q2 = A: scara sintetică `PerfCub`, în container, cu criterii de formă.
- X-Q3 = A: refuz pe ramura negativă a desfacerii automate.
- X-Q4 = A: `PosteazaInCub` ireversibil.
- Nivelul 3 al blocajului (2026-10-01): serial per bază. Variantele mai fine
  (chei pe unitate; gestiune + partener) nu se implementează acum.
- Modelul perioadei rămâne cel din 088 (2026-10-01): închiderea
  materializează, redeschiderea nu reevaluează ieșirile operate.
- Numărul deciziei este 108 (2026-10-03): 106 și 107 sunt luate de
  `c106-regim-stare` și `m1-deschidere-terti`, nemerse.
- Amendamentul pe `PartideCuRest` (2026-10-04): criteriul „accesul la
  `Postare` nu depinde de istoric” se amână la TR-D9, odată cu F27-r16.
  Cifrele: 2.242 / 3.366 / 4.504 rânduri pe privat și 1.579 / 2.418 / 3.282
  pe bugetar, la 0 / 6 / 12 luni închise. Scara îl raportează `AMÂNAT`.
- Metoda probei din plan (2026-10-04): rândurile atinse pe `Postare` se
  numără cu `enable_seqscan = off`; bufferele se cer mărginite de rânduri,
  nu egale între trepte.

## Ce a livrat felia

| Pas | Litera | Commit | Integrala (bugetar / privat) |
|---|---|---|---|
| 1 | X-D2: proba registrelor, lista nominală, N-r8 pe cititori | `9185a32` | 3.277 / 4.485 |
| 2 | X-D4: explicația persistată, cititor, `GET`, S-r11 | `e29aa2c` | 3.309 / 4.518 |
| 3 | X-D6: blocajul scrierii, probele pe două conexiuni, S-r9 | `9b5fa62` | 3.361 / 4.570 |
| 4 | X-D7: acoperirea stocului, regimul ireversibil, restanțele | `87052f7` | 3.387 / 4.597 |
| 5 | X-D5 + X-D3: scara, reconcilierea pe baza de volum | `06cd4d4`, `ff08a8c` | 3.387 / 4.597 |
| 6 | X-D8: integrala, probele HTTP, driftul, închiderea | `2b57aa5` | 3.387 / 4.597 |
| review | X-RI1…X-RI4: corecturile review-ului implementării | `791e41c` | 3.394 / 4.604 |

Probele pasului 6, pe o bază privată recreată din seed
(`run-verificari/x6-http/`): matricea `refuzuri.ps1` 300/300 de două ori, cu
aceleași rezultate și zero reziduu, acum cu ușa explicației;
`scriere-ocupata.py` 7/7; `comenzi-coaja.py` 28/28; `explicatii.py` 8/8.
Integrala la `ff08a8c`: `run-verificari/20261004-081014-672/`. Nucleu
180/180, `--probe-sursa` verde, OpenAPI și tipurile TS fără drift.

Scara transversală: 1.488 OK pe privat și 970 pe bugetar, cu un singur
criteriu picat pe fiecare, cel amânat
(`run-verificari/perf-cub-20261004-002621/`).

## Constatări care au amendat contractul

- Lista nominală a cerut trei clase în plus față de contract: maparea,
  autorizarea și legătura `Imperechere`.
- Rapoartele API cumulate citesc `Vizibila`, deci recitesc postările (104b).
  Din snapshot citesc motorul și SAF-T. Matricea de perf are de aceea o rută
  în plus, *interval*.
- Prima rulare a scării a picat proba din plan pe aproape toți cititorii.
  Cauzele au fost de formă și s-au corectat în felie: fereastra nesargabilă
  din `CumulPerioade`, lipsa filtrului `Spatiu`, gardul de sold intermediar
  care citea tot istoricul lotului, încărcarea leneșă din raportul de impact
  și cinci indecși lipsă.
- Reconcilierea (f) nu cunoștea stornoul și partidele de deschidere. Defectul
  era al diagnosticului.
- Trei probe vechi presupuneau două comenzi simultan în scriere și s-au
  rescris pe modelul serial (SC-DES-05, SC-DES-21, SC-IMO-32).

## Interpretări declarate

- N-r8 cerea „fișa lotului și a partidei cu eticheta felului”. Produsul nu
  are o asemenea listare, deci regula e fixată și probată pe intrările
  comune `Loturi.Postari` și `Partide.Postari`. O fișă viitoare se
  construiește peste ele.
- Cheia acoperirii cantitative a stocului nu conține contul: document × lot ×
  gestiune × storno × semn. Contul recepției conexe diferă legitim între
  registru și cub.
- `SCRIERE_OCUPATA` iese 422, ca orice `OperareException`. Regula refuzurilor
  (80) nu are un cod separat pentru „ocupat, reîncearcă”.
- Invariantul explicației nu recitește istoricul. Un sold citit alterat în
  sus și o valoare declarată diferită de linia sau recepția ei de origine,
  dar egală cu postarea, rămân nedetectate.

## Review

Review-ul advers al contractului: X-RV1…X-RV7, plus reverificările 1.1, 1.2
și 5.1, toate închise înaintea implementării
(`docs/nucleu/tr-d8-transversal-review-codex.md`).

Review-ul advers al implementării, la `2b57aa5`
(`docs/nucleu/tr-d8-transversal-implementare-review-codex.md`): patru
observații P2 despre verificările care certifică felia. Niciuna nu a cerut
schimbarea unei cifre postate și niciuna nu redeschide tranșările
owner-ului.

| Obs. | Ce lipsea | Corecția |
|---|---|---|
| X-RI1 | invariantul explicației accepta ieșirile evaluate redenumite „declarate” și stingerea FIFO fără soldul citit | mecanismul ținut de declarant (`IDeclarant.SursaValoareDeclarata`), `CITIRE_EXPLICATIE_MECANISM`; soldul citit al partidei stinse obligatoriu și plafon al alocărilor |
| X-RI2 | contul capătului de destinație al unui BTR nu era păzit: cheia cantitativă nu are cont, iar reconcilierea (a) exclude transferurile | `Invarianti.VerificaTransferuri`, `CITIRE_TRANSFER_NECONSERVAT` |
| X-RI3 | un `EXPLAIN` respins lăsa statistici zero, acceptate de criteriul din plan | planul respins e eroarea măsurării; fiecare citire cere plan; proba `X-D5-PLAN` |
| X-RI4 | controlul numeric al operației D300 citea `DecontTva` | controlul pe rândurile 9, 24, 19 și 30 din rezultatul D300 |

Nu s-a confirmat o cale reală de ocolire a blocajului scrierii și nici un
cititor de registre ratat de proba sintactică. Reverificarea Codex la
`7223abc` închide X-RI1…X-RI4: integrala proprie 3.394 / 4.604 OK, zero
FAIL, `run-verificari/20261004-102241-857/`, inclusiv mutanții noi și
`X-D5-PLAN`. Scara completă predată a fost inspectată, nu rerulată;
detaliile și limitele sunt în review-ul implementării.

## Restanțe

Închise în felie: S-r2, S-r9, S-r11, F27-r8, T-r11, T-r14, N-r8, 097-r1,
102-r4, SAFT-r4, FZ-r1.

Noi:

- **X-r1** — rafinarea blocajului scrierii. Două fețe: granularitatea pe
  gestiune + partener și scoaterea salvării de draft de sub blocajul
  comenzilor. Criteriul de deschidere: așteptare măsurată peste un prag pe
  care owner-ul îl fixează atunci. Pragul de așteptare nu e separat azi de
  timpul comenzii.
- **X-r2** — subiectul permisiunii care păzește cifrele citite din cub. Azi
  este dreptul de citire pe tipul registrului (`RegistrulCitibil`); tipurile
  de registru dispar la TR-D9.
- **X-r3** — raportul de stoc listează capătul de consum al BCS: postarea de
  debit poartă lotul ca unitate, pe contul de cheltuială și la locul de
  consum, iar `Loturi.Postari` o ia ca poziție. Întrebare de semantică pentru
  owner.

Rămân deschise, cu starea din `restante.md`: T-r13 (evaluările noi ale
reziduului dual, TR-D9), FZ-r3 (pragul pe volum real, migrare), F27-r16
(forma `DocumenteCuRest`, cu criteriul de perf amânat), 102-r5 (numai
măsurat), 097-r3.

## Ce rămâne deschis

TR-D9: tăierea scriitorilor vechi și a regimului dual, N-r5 și `Atribuit`,
TR-r9. Migrarea: contractul de reconciliere 1C și bazele de volum reale.
După PoC: ecranele explicației și B8-r*.
