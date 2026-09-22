# TR-D7b — Strangler-ul per tip, felia 32: tipurile rămase pe cub și deschiderea ca tranzacție

- **Data**: 2026-09-21
- **Stare**: în lucru (branch `tr-d7b-tipuri-ramase`, tăiat din `main` = 48bf1c7; un commit per pas)
- **Docs**: decizia 090 §Regula durabilă (a), (c), (d), (f), (g), (j), (l), (m); `docs/nucleu/tr-d7a-strangler-contract.md` (fundația: S-D1…S-D16, amendamentele (1)–(12), „Închidere"); `docs/nucleu/nucleu-transfer.md` (TR-D1 deschiderea terților :115-123, TR-D4 postarea unică :234-240 și „Ce schimbă în balanță, declarat" :253-269, notele pe stoc fără lot :262-269); `docs/decizii/restante.md` (TR-r2, TR-r4, TR-r5, TR-r6, TR-r7, TR-r10, TR-r12, B-r3, B-r4, B-r5, B-r8, N-r8, S-r1…S-r11); recensământul documentelor pe Flax `run-nucleu/transfer/03-out/b5-fct-nir.txt:23-38`; hărțile explorării în `agenti-msg/` (untracked).
- **Felii TR-D7**: felia 31 (TR-D7a) = fundația persistată + BCS, FCT, PLT, INC. Felia 32 (TR-D7b) = TOATE tipurile rămase + deschiderea. După ea, TR-D7 e închis și urmează TR-D8 (citirile pe cub).

## Scop

Fiecare tip de document care mai postează doar în registre primește declarant
și `PosteazaInCub = true` pe profilurile unde există, în ordinea volumului pe
Flax, refolosind fundația feliei 31 neschimbată: entitățile, migrația,
materializarea în tranzacția comenzii, stornoul, anularea, `Pozitie`,
transferul împerecherii, cele două unelte de gate. Deschiderea (soldurile
inițiale ale importului) devine tranzacție de fel `Deschidere` (TR-r10).
Citirile rămân pe registre (TR-D8). La închidere, `--reconciliere-cub` nu mai
are litera (f) vacuă din cauza tipurilor nemigrate, iar Δ de sold 3xx
(TR-r12) e MĂSURAT și descompus pe tipurile care îl produc, nu ascuns.

## Recensământul și ordinea (owner, 2026-09-21: DSC și LDI intră; ordinea e a volumului)

Documente operate pe Flax (`b5-fct-nir.txt`, B5.2b), tipurile nemigrate:

| Ordine | Tip | ClrType | Documente | Pasul |
|---|---|---|---|---|
| 1 | BTR | `NotaTransfer` | 45.552 | 1 |
| 2 | FCL | `FacturaIesire` | 40.535 | 2 (grup cu DSC) |
| 3 | DSC | `DescarcareGestiune` | 36.696 | 2 |
| 4 | NIR | `NIR` | 17.814 (toate conexe autogenerate, 0 manuale) | 5 |
| 5 | NTC | `NotaContabila` | 7.818 | 3 |
| 6 | RDC | `ReturClient` | 1.666 | 4 |
| 7 | ASM | `Asamblare` | 1.228 | 5 |
| 8 | RLF | `ReturFurnizor` | 398 | 4 |
| 9 | LDI | `ListaDiferenteInventar` | 18 | 5 |
| 10 | ITV | `InchidereTva` | 12 | 3 (moștenește declarantul NTC) |
| 11 | DVI | `Dvi` | 0 (fără sursă 1C) | 4 |
| — | deschidere | fără tip (`DocumentId = null`) | 1 tranzacție per bază | 6 |

Grupurile se țin împreună pentru că gate-ul le cere ca grup (litera (a)):
FCL cu DSC-ul lui secundar; NIR intră după FCT-ul care îi postează deja
recepția. Pașii sunt ordonați după volumul tipului cel mai mare din pas.
Lista din CLAUDE.md §Următorul pas (fără DSC și LDI, altă ordine) e
DEPĂȘITĂ de tabelul de aici.

## Testul contra invarianților

| Invariant | Cum îl respectă felia |
|---|---|
| I (registre append-only) | neschimbat față de felia 31: `Operare`, `Storno` ca a doua tranzacție, anularea ștergere simetrică; `Transfer` pe BTR/ASM e append-only ca orice tranzacție; `Deschidere` se scrie o dată, la import, și se șterge doar cu `--recreeaza` (împreună cu rândurile bloc de azi) |
| II (motorul nu cunoaște frunzele) | fiecare tip își aduce declarantul prin `Declarant()`; felul mixt `Operare`/`Transfer` (T-D2) se decide pe CONTUL liniei, nu pe tip; excluderea conexului (T-D5) se decide pe rândul `PoliticaConex`, nu pe `is NIR`; ITV moștenește declarantul NTC prin clasă, fără `is` |
| III (structura = cod, politica = date) | nicio coloană de politică nouă; `PosteazaInCub` se extinde în seed; gestiunile virtuale noi (T-D9) sunt constante ale nucleului, ca cele trei existente (B-r8 rămâne deschisă); contrapartida plusului LDI și derivările 6xx vin din `RegulaContare` |
| IV (o singură sursă de reguli) | refuzul declarației = refuzul operației pe toate cele 11 tipuri, în același OS, în aceeași tranzacție; deschiderea scrie tranzacția prin `Cub.Materializare`, nu prin rânduri directe |
| V (diferențele se raportează) | T-D3 (A) (NTC fără partener), T-D6 (RLF la valoare fiscală), T-D10 (Δ 3xx) sunt consecințe DECLARATE cu cifră, fiecare cu litera ei în gate; scăparea conectorului la deschidere (T-D7) se CORECTEAZĂ, nu se declară; Import1C identic pe conținut sortat |
| VI (fără interogare per linie) | `Fapte.Operand` rămâne ≤ 16 interogări pe toate tipurile; DSC citește loturile liniilor pe set (`SolduriLoturi` există); deschiderea scrie pe set |

## Deciziile (T-D1…T-D13)

### T-D1 — Fundația se refolosește, nu se re-taie

Entitățile, migrația `CubDePostari`, `Materializare.Opereaza/Refuzuri/Storneaza/Anuleaza/Imperecheaza`,
`Transferuri.Muta`, `Fapte.Operand`, `Contari.Rezolva`, `Partide.*`,
`Fiscal.*`, oracolul `CubDinRegistre` + `Normalizari`, `GatePeBaza`,
`ReconciliereCub`, `ProbeCub` — toate se EXTIND, nu se rescriu. Un tip nou
atinge lista din harta fundației (`agenti-msg/explore-fundatie-out.md`
§„Ce atinge adăugarea unui tip nou pe cub", 16 puncte): declarantul,
override-ul pe frunză, codurile de refuz, operandul (doar dacă lipsește un
fapt), seed-ul, oracolul/normalizările, gate-ul, grupul în reconciliere,
probele, docs. `Operand.cs` + `Fapte.cs` se ating o singură dată pe felie
dacă se poate (faptele noi ale tuturor tipurilor la pasul 1, cu probe), nu
pas cu pas.

### T-D2 — Felul tranzacției pe linia de stoc: `Transfer` când contul nu se schimbă, `Operare` când se schimbă (090 (f), (g); TR-D4)

Pentru o linie care mută un lot între gestiuni (BTR) sau valoare între loturi
(ASM): dacă contul de stoc al laturii sursă = contul laturii țintă, postările
liniei intră într-o tranzacție de fel `Transfer` (`Σ per (Cont, Latura) = 0`,
exclusă din rapoartele pe cont); dacă diferă (Mărfuri → Magazie, `3028 = 371`;
`345 = 301`), postările intră în tranzacția de fel `Operare`. Un document poate
avea astfel CEL MULT o tranzacție `Operare` și CEL MULT o tranzacție
`Transfer`, cel puțin una — amendament de literă al lui 090(a) („EXACT o
tranzacție `Operare`") consemnat aici și în `restante.md` (T-r1); litera (e)
a reconcilierii se amendează la fel: document operat al unui tip migrat ⇔
exact o tranzacție `Operare` SAU exact o tranzacție `Transfer` SAU câte una
din fiecare, nimic altceva. Stornoul inversează ambele (fel `Storno`, N-r8);
anularea le șterge pe ambele. Mecanica e în `Materializare` (o singură
împărțire pe set după `Fel` al mișcării declarate), nu în declarant:
declarantul marchează fiecare `N.Miscare` cu felul cerut (nucleul primește
`FelTranzactie` pe mișcare sau `N.Declaratie` cu două liste — pasul 1 alege
forma cu proprietate în `Atlas.Conta.Nucleu.Teste`).

Semnalul de gestiune NU se pierde: fiecare capăt al unui `Transfer` de stoc
poartă obligatoriu `Gestiune` și `Unitate` (lotul); felul schimbă doar
cititorul — rapoartele pe cont îl exclud (Σ pe (Cont, Latura) = 0), cele pe
gestiune și pe unitate îl includ (090 (f)). Proba `STR-BTR-ACELASI-CONT`
verifică stocul per gestiune și per lot, nu doar balanța.

Evaluarea: ieșirea din lotul sursă e `N.Evaluare.Iesire` pe raportul curent
(090 (j), ca BCS); intrarea pe lotul țintă e la aceeași valoare. Pe același
cont, unitatea rămâne ACELAȘI lot cu altă `Gestiune` dacă motorul vechi ține
lotul; dacă motorul vechi naște lot nou pe destinație, unitatea țintă e lotul
nou — pasul 1 CONSTATĂ pe `RegistruStoc` (cheia de stoc a destinației) și
declarantul urmează registrul; forma se pin-uiește ca amendament (T-D2.1)
înainte de gate.

**T-D2.1 (pin-uit 2026-09-21, măsurat pe clona `Atlas.Conta.Import1C.Flax.TrD7b`
înaintea pasului 1)**: cheia de stoc e `(Lot, Repartitor, TipStoc)`
(`StocService.cs:7`), deci lotul își schimbă gestiunea, nu se naște lot nou;
pe cele 45.552 BTR operate: 85.027 linii, 170.054 rânduri `RegistruStoc`,
EXACT 2 per linie, 0 linii cu loturi diferite între laturi, 0 cu `TipStoc`
diferit, 0 rânduri contabile, 0 loturi fără cont implicit pe `TipMaterial`,
0 linii cu predator = primitor. Contul postării de stoc e al lotului pe
ambele capete (regula oracolului, `CubDinRegistre.DeStoc`), deci **pe modelul
de azi BTR produce NUMAI `Transfer`**: unitatea țintă e ACELAȘI lot cu altă
`Gestiune`, la valoarea ieșirii din sursă. Reclasificarea de cont din 1C nu
trece prin BTR, ci prin ASM `#reclas` + BTR pe lotul nou
(`Import1C/HandlereStoc.cs:147-174`). Ramura `Operare` a formei mixte rămâne
generică (nucleu + `Materializare`), probată prin proprietăți în nucleu la
pasul 1 și pe scenă de ASM la pasul 5; probele `STR-BTR-CONT-DIFERIT` și
`STR-BTR-MIXT` sunt proprietățile nucleului, nu scene BTR. Forma în nucleu:
`N.Declaratie` cu a patra listă `Mutari`, `N.Contract.Tranzactii` (1–2, ordinea
`Operare`, `Transfer`), fără `Fel` pe `Miscare`; stornoul = O SINGURĂ
tranzacție `Storno` peste postările ambelor (N-r8), anularea le șterge pe
ambele; stornoul selectează transferul de STOC al documentului (unități de fel
`Lot`) — transferurile pe partidă rămân ale împerecherii (S-D13).

**T-D2.2 (declarată 2026-09-21, gate-ul BTR pe clona TrD7b: 45.552 documente,
45.017 egale, 0 refuzate, 0 excepții, 535 diferite, un singur fel)**: valoarea
liniei care NU golește cheia de stoc diferă între motorul vechi
(`round(cantitate × Lot.PretUnitar)`, `NotaTransfer.PregatesteOperare`, preț
înghețat la nașterea lotului) și declarant (`Evaluare.Iesire` pe raportul
curent al cheii `(Lot, Predator, TipStoc)`, 090 (j)) — aceeași abatere ca N-r3,
măsurată pe BTR: 560 de linii din 85.027, toate ieșiri parțiale (cele 70.841
de linii care golesc cheia sunt egale prin construcție), Δ fără semn constant
(+0,01 pe 295, −0,01 pe 234, |Δ| > 0,02 pe 9), Σ semnată +0,92 lei și Σ
absolută 28,60 lei pe 121.094.304,67 lei mutați; 553 de linii sunt reziduul
propriei goliri a motorului vechi (D18-D2) mutat pe destinație, 7 sunt loturi
intrate în gestiune la altă valoare decât prețul lor înghețat (extrem
`BTR-9039`: `PretUnitar = 0` contra 11,00 lei/buc). Abaterea e de REPARTIZARE
între gestiuni, nu de conservare: ambele capete poartă aceeași valoare, Σ per
(cont, latură) = 0, literele (a)–(g) rămân 0 Δ; devine vizibilă abia la
citirile pe cub per gestiune × lot (TR-D8). Se DECLARĂ, nu se normalizează
(B-D8 rămâne închisă); gate-ul BTR se citește „100 % egal în afara celor 535".
Descompunerea integrală: `run-nucleu/tr-d7b/pas1/raport.md` §B–§F.

Constatări ale pasului 1 care precizează pin-urile: plierea în oracol cere ca
NICIO linie a grupului (document, lot) să n-aibă rând contabil — altfel BCS (două
rânduri de stoc pe același lot, cu picior `6xx`) s-ar plia greșit
(`CubDinRegistre.EMutare`); litera (b) a reconcilierii citește `Operare` ⊕
transferul de stoc; normalizările TR-D4 și TR-D2a primesc gard pe fel
(`Normalizari.EDeImperechere`: transferul cu unități `Lot` nu e împerechere);
scena SAF-T cu lot fără cont de stoc rulează pe tip nemigrat (refuzul
`CONT_STOC_LIPSA` e corect și simetric cu oracolul; pe Flax 0 loturi fără cont).

Aceeași regulă pentru ASM: consumul (−q pe lotul consumat) și produsul (+q pe
lotul născut de linia `Produs`, la `PretEvaluare`) sunt `Transfer` dacă
ambele loturi sunt pe același cont, altfel `Operare`. Invariantul ASM
(Σ produse = Σ consumuri, toleranță 0,005) e conservarea valorii pe spațiul
Stoc — nu se re-implementează în declarant; diferența sub toleranță, dacă
există, se postează ca reziduu DOAR dacă o politică o cere (090 (j)), altfel
refuz `ASAMBLARE_NEBALANSATA`.

### T-D3 — NTC: postare explicită CA ATARE; partenerul vine pe linie din sursă, partida se nominalizează FIFO; fără partener, postarea e DECLARATĂ, nu refuzată (owner, 2026-09-21: varianta B cu A ca fallback)

Nota contabilă postează exact perechea de conturi a liniei, cu valoarea ca
atare (negativă inclusiv, S-D14), fără normalizare de semn, cu dimensiunile
liniei; linia cu `ContDebit = ContCredit` (compensările 1C prin 891,
`411.1 = 411.1`) e permisă — e vehiculul stingerii până la TR-D9.

Contextul: laturile documentului sunt obligatoriu interne
(`NotaContabila.cs:90`), dar LINIA poartă `RepartitorDebit`/`RepartitorCredit`
(`NotaContabilaDetaliu`, `ILinieCuPostareExplicita`), care pot fi parteneri;
Import1C le lasă goale (`NoteComune.Materializeaza`, SEDIU pe laturi, fără
repartitor pe linie în afara compensărilor), deși rândul 1C poartă partenerul
ca subconto pe 401/411/419. De aici cele 7.824 de picioare fără partener pe
Flax (4.068 pe 4111, 1.872 regularizări de avans pe 419).

(B) Conectorul pune partenerul din subconto-ul rândului 1C pe repartitorul
laturii liniei (modelul îl acceptă deja; `NoteComune` primește callback-ul
`repartitori` pentru toate notele, nu doar compensări). Pe un cont cu
`RolTert` cu partener pe latură, declarantul NOMINALIZEAZĂ FIFO pe partidele
deschise ale aceluiași `(Cont, Partener)` la `DataInregistrare` (090 (e):
„automat, FIFO pe partidele deschise … când politica o cere" — aici politica
e regula tipului NTC, declarată, nu rând de tabelă), cu plafonul = restul
partidei; suma peste rest deschide partidă proprie pe notă. Regularizarea
avansului pe 419 numește astfel ambele unități (4111 și 419 ale partenerului).

(A, fallback declarat) Pe un cont cu `RolTert` FĂRĂ partener pe latură:
`Partener = null`, `Unitate = null` — nota NU deschide și NU numește partidă.
E abaterea declarată de la 090 (c) până la TR-D9 (culegerea unității pe linie,
090 (m)). Consecințe: (1) litera (f) EXCLUDE nominal partidele
`(Cont, Partener)` atinse de o postare fără unitate, cu numărul și Σ per cont;
(2) un NTC stingător fără partidă proprie e `Sarit` în `Imperecheaza`,
contorizat. Pasul 3 MĂSOARĂ cât din cele 7.824 primește partener prin (B) și
cât rămâne pe (A); gate-ul arată că raportul Import1C rămâne identic
(capacitatea de stingere a notelor se schimbă în registre — `Imperecheri`
create de conector pot crește; cifra se consemnează).

Pe un cont de stoc (3xx) fără lot (TR-r2, 3.365 picioare pe Flax): postarea
e DOAR de valoare, `Cantitate = 0`, `Unitate = null`, deci `Spatiu = Contabil`
(090 (g): Stoc = postările cu unitate de tip lot). Gate-ul o numără și o
însumează per corespondență `(ContDebit, ContCredit)` — cele cinci de pe Flax
(`891 = 371`, `607 = 371`, `401 = 371`, `3028 = 371`, `371 = 401`) trebuie să
iasă cu cifrele din `b2c-invers.txt:71-94`; alegerea „postare pe lot sau
divergență a conectorului" per corespondență rămâne a lui TR-r2 la TR-D9,
când linia poate numi lotul. Gardul `Conservare.cs:133` se verifică pe caz:
dacă respinge postarea fără unitate pe un cont de stoc în spațiul Contabil,
se relaxează cu proprietate (spațiul e al unității, nu al contului), nu se
ocolește.

ITV moștenește `Declarant()` de la `NotaContabila` prin clasă: liniile lui
(`4427 = 4426`, `4423`/`4424`) sunt postări explicite fără terț și fără stoc.
Soldurile din care ITV își calculează liniile vin din `SolduriService`
(registre) până la TR-D8 — divergență de sursă declarată, fără efect asupra
postării.

### T-D4 — FCL ∪ DSC: venitul pe FCL, costul și stocul pe DSC, un singur grup în gate

FCL: `4111 = 70x` la net per linie (regula per `TipMaterial`, generic 708),
`4111 = 4427` per linie prin `Fiscal.Impozitul` cu direcția `Colectat`, taxa
culeasă autoritară (`pastreazaTvaCules`), taxarea inversă pe livrare ⇒ fără
postare fiscală (ca motorul vechi, `MotorOperare.cs:252-253`); partidă pe
FIECARE cont cu `RolTert` al liniilor (S-D16): 4111 și, pe liniile de avans,
419 — FCL cu avans are DOUĂ partide (090 (d), 2.036 pe Flax); zero postări de
stoc. Bugetar: `PoliticaValidare(FCL, NaturaInterzisa = Stoc)` rămâne, fără
DSC.

DSC: `6xx = 3xx` per `TipMaterial` cu excepțiile profilului, `−q` pe lotul
numit de linie (`LotId` e ales de `DescarcareService`: pin-uri, apoi FIFO —
declarantul NU realocă), evaluat cu `N.Evaluare.Iesire` pe raportul curent în
secvența liniilor (ca BCS), capătul 6xx cu `+q` pe gestiunea virtuală
`Client` (090 (g); prima folosire a constantei). DSC fără `DocumentSursa`
(din `HandlerAvizIesire`) e un document de sine stătător, fără partidă.

Grupurile literei (a)/(b)/(c) NU se schimbă (amendat la pasul 2, 2026-09-22):
grupul e per COD de tip migrat ∪ conexele prin `PoliticiConex`, iar DSC nu e
conex — FCL și DSC sunt fiecare grupul lui, cu tranzacția lui în cub, deci
literele se închid singure; `Incomplete` rămâne pe `PoliticiConex` (un FCL cu
DSC draft nu e Δ). Transferul FCL ↔ INC prin `Materializare.Imperecheaza` se
ACTIVEAZĂ cu pasul 2 (până acum ieșea devreme, FCL nemigrat), fără nicio
atingere: `Transferuri.Cea` ia referința de pe STINGĂTOR (INC: 4111), iar
plafonul e `Net` pe 4111 al FCL (operare + transferurile primite) — pe FCL cu
avans partida 419 nu intră în plafon și nu primește transfer. Sub S-D16
partida de creanță ține și debitul liniei de avans (`4111 = 419`, −100):
soldul partidei 4111 e netul ei (probat `STR-FCL-AVANS-DOUA-PARTIDE`: 142,
nu 242), deci împerecherea e plafonată la 142. Cifrele transferurilor
MATERIALIZATE INC → FCL ies la proba supremă (pasul 7): gate-ul e read-only.

**T-D4.1 — normalizarea oracolului pentru capătul de cost al DSC** (pasul 2):
registrul DSC are un singur rând de stoc per linie (−q pe 371, predator) și
rândul `607 = 371` cu AMBELE dimensiuni pe gestiunea predatoare (32c), pe când
în cub capătul 607 e pe gestiunea virtuală `Client` (proiectată ca lipsă,
N-D4). Regula, generală, în `Normalizari.TrD41CostulIesiriiEAlTertului` după
`TrD4`: piciorul contabil FĂRĂ stoc al unei linii care are DOAR ieșiri de stoc,
fără unitate și fără partener, cu gestiunea egală cu a piciorului de stoc,
își pierde gestiunea în oracol. BCS (rând `+q` de `Consum`), FCT (recepție =
intrare), BTR (fără picior contabil), PLT/INC (fără stoc) nu sunt atinse —
`NUC-BCS`, `NUC-FCT*`, `NUC-BTR-*` verzi, `gate-fct` identic cu felia 31.

**T-D4.2 — diferența declarată pe DSC** (măsurată pe clona TrD7b, 2026-09-22):
aceeași clasă ca T-D2.2 pe BTR (N-r3): `DescarcareGestiune.PregatesteOperare`
scrie `round(q × Lot.PretUnitar)` cu prețul înghețat, cubul evaluează pe
raportul curent al cheii `(Lot, Predator, TipStoc)` (090 (j)). Gate: 842 din
36.696 DSC (2,29 %), 0 refuzate; în modelul SQL 919 linii din 62.063, toate
ieșiri PARȚIALE (cele 47.505 goliri sunt egale), Σ semnată +31,01 lei, |Δ| max
16,50 lei (lot cu `PretUnitar = 0`), 909 din 919 linii ≤ 6 bani, pe
79.199.898,27 lei mutați; cauze: raport curent ≠ `PretUnitar` 487 linii,
derivă de rotunjire ≤ 2 bani 419, registru care nu e `round(q × PretUnitar)`
13. Nu se normalizează; T-r7 se extinde la DSC.

**T-D4.3 — citirea soldului „fără documentul curent" pe o lună închisă**
(pasul 2, fix în `Motor/StocService.cs:61`): `SolduriService.MiscariCumulate`
ia snapshot-ul perioadei de referință când sfârșitul ei ≤ data cerută, iar
`faraDocumentId` filtrează doar rândurile de după referință — un document
datat în ULTIMA zi a unei luni închise își citea propria ieșire în snapshot
(2 DSC din 31.12.2025 refuzate `STOC_INSUFICIENT` la gate). Regula: când se
citește fără un document, referința se termină STRICT înaintea datei
(`granita = data − 1 zi`; gard pe `DateOnly.MinValue`). Neutru în operarea vie
(perioada e deschisă); `gate-dsc` după fix: 0 refuzate. Capcana consemnată la
felia 31 în `dezvoltare-si-validare.md` dispare.

### T-D5 — NIR: doar recepția fără factură; clona conexă a unui tip migrat NU se materializează

`Materializare.Opereaza/Refuzuri/Storneaza/Anuleaza` ies devreme pe un
document generat prin `PoliticaConex` dintr-o sursă cu `PosteazaInCub` —
condiția e `doc.Autogenerat && doc.DocumentSursaId != null` și existența
rândului `PoliticaConex(tipul sursei → tipul documentului)`, citit din
politică (o interogare pe set, în `GasesteTipDocument` sau alături), fără
`is`. Recepția e deja a FCT (TR-D3); NIR-ul conex rămâne în registre până la
TR-D9 (B-r3 rămâne deschisă cu același motiv). Grupul `{FCT ∪ NIR conex}` al
gate-ului nu se schimbă.

NIR manual (`Autogenerat = false`): `3xx = 401` la net (rândul `NIR/Stoc`),
`+q` pe lotul născut de linie (`ILinieCareNasteLot`), capătul de stoc pe
gestiunea Primitor, contrapartida pe gestiunea virtuală `Furnizor` (ca
`Receptia` din FCT), partidă pe 401 (S-D16), fără postare fiscală. Pe Flax nu
există niciun NIR manual ⇒ oracolul lui sunt EXCLUSIV scenele ModelCheck (cele
12 probe `NIR*` existente materializează; una nouă `STR-NIR-MANUAL`). TR-r4
(408 pe aviz) și B-r5 rămân deschise: fără oracol, nu se implementează.

### T-D6 — RLF: ieșirea la valoarea FISCALĂ, reziduul pe lot DECLARAT (abatere de la 090 (j) până la TR-D9)

RLF postează `3xx = 401` cu `−V` la valoarea hârtiei furnizorului (preț ×
cantitate, `IDocumentCuIesireFiscala`), `4426 = 401` cu `−TVA`, `−q` pe lotul
numit, partidă pe 401 — identic cu registrele. Când linia golește lotul,
reziduul valoric rămâne pe unitate (cantitate 0, valoare ≠ 0), contra regulii
090 (j) „cantitate zero ⇒ valoare zero". E consecința declarată a
transferului regulii pure „cu tipul ei" (090 (l)): re-evaluarea unității cu
reziduul spre 658/758 prin politică e a lui TR-D9, când `Lot.PretUnitar`
dispare și unitatea se citește din cub — restanță nouă T-r2. Gate-ul (b)
compară cantitatea (egală) și raportează per lot reziduul lăsat de RLF (pe
Flax 19–56 goliri/lună, ±0,01). `N.Evaluare.Iesire` NU se atinge; declarantul
RLF nu-l cheamă.

### T-D7 — Deschiderea ca tranzacție `Deschidere`, scrisă de `Cub.Materializare`; pe terți, partidă per (Cont, Partener, document 1C) din `BalantaNivel3`; stingerile 2025 pe ele prin aceeași cheie (TR-r10, TR-r6 închise pe date)

`Cub/Materializare.Deschide(os, data, randuriContabil, randuriStoc)` (în
Module, nu în Import1C) scrie EXACT o tranzacție de fel `Deschidere` per bază
(`DocumentId = null`, `Data` = data deschiderii), cu postările pe care oracolul
`CubDinRegistre` le derivă din rândurile cu `DocumentId = null`: per rând
contabil o postare Debit și una Credit (contul și ancora 891, `Valoare`,
fără dimensiuni, `Partener = null`, `Unitate = null`), per rând de stoc o
postare pe partiția Stoc cu `Unitate = lotul`, `Cantitate`, `Valoare`,
`Gestiune`, contul din simbolul lotului. `Import1C/Deschidere.cs` cheamă
`Deschide` DUPĂ ce scrie rândurile bloc (regimul dual) și o șterge la
`--recreeaza` odată cu ele. `Cub/Randuri.Citeste` încetează să arunce pe
`DocumentId == null`; `Conservare.VerificaSemnul` ține deja `Deschidere` la
valori nenegative — loturile sunt netate în interiorul grupei produs × depozit
înainte (fapt al conectorului, neschimbat).

**Sursa DĂ defalcarea (constatare 2026-09-21, corectează `Deschidere.cs:43-50`
și decizia 047).** Conturile de terț au TREI subconto în 1C, deci nu apar în
`BalantaNivel1` (conturi cu un subconto), ci în `BalantaNivel3`, pe care
importul o citește doar pentru stoc. La 01.01.2025, `BalantaNivel3` are pe
401.1 1.235 poziții nenule (159 parteneri), 401.2 105 (17), 411.1 2.490,
419.1 118, 409.1 7, cu subconto `Partener × Contract × Document de decontare`
(factura de achiziție / vânzarea / returul), iar Σ per cont = `Balanta` la
ban (401.1 −5.106.443,33; 411.1 4.281.860,21; 419.1 −227.565,80). E exact
cazul „per factură deschisă, când sursa o dă" din 090 (d). Scăparea era a
CONECTORULUI (nivelul citit), nu a view-urilor; comentariul din
`Deschidere.cs` și textul 047 se corectează la pasul 6, `SolduriPartener`
(pe `BalantaNivel1`) se înlocuiește.

Regula: pe conturile cu `RolTert`, deschiderea din cub scrie o postare per
poziție `(Cont, Partener, document 1C)` cu `Partener` pe postare și
`Unitate` = partida deschisă prin `DeschidePartida(cont, partener,
documentDeschizator, data)`, unde `documentDeschizator` e un Guid DETERMINIST
derivat din referința 1C `(Valoare3_Type, Valoare3_Id)` (aceeași cheie pe
care conectorul o folosește la `bucla.Tinta(TintaTip, TintaId)`), înregistrat
în `MigrareLegatura` ca orice țintă; pozițiile care se netează la zero pe
`(partener, document)` se sar. Rândurile bloc din registre rămân NESCHIMBATE
(un rând per cont contra 891, fără dimensiuni) — raportul Import1C rămâne
identic, iar `PartideDeschise`/`SolduriService` nu se ating.

**Stingerile din 2025 pe documente din 2024 sunt acoperite de aceeași cheie
(owner, 2026-09-21: arieratele la nivel de document).** Azi conectorul le sare
cu motivul „ținta e dinaintea ferestrei de import (sold de deschidere)": pe
Flax 15.679 pe facturi de achiziție, 15.149 pe vânzări, 1.628 pe retururi de
la client, 649 pe retururi la furnizor (`import.log` al feliei 31). La pasul
6, `bucla.Tinta(TintaTip, TintaId)` întoarce partida de deschidere când ținta
e o poziție din `BalantaNivel3`, iar stingerea se scrie DOAR în cub prin
`Materializare.Imperecheaza(stingator, partidaId, suma, data)` (varianta pe
partidă fără document; aceeași funcție pură `Transferuri.Muta`, plafon =
restul partidei de deschidere, peste rest = `Sarit` contorizat), fără rând în
`Imperecheri` și fără `TotalStingere` (registrele n-au partida). Retururile
din 2024 rămân sărite (total negativ, 46f). Contoarele conectorului se mută
din „sărite" în „stinse pe partidă de deschidere", cu Σ.

Gate: grup nou `{Deschidere}` (postările tranzacției `Deschidere`) la
literele (a) și (b) contra rândurilor cu `DocumentId = null` (Σ per cont
egală, deși cubul e defalcat pe partener × document); litera (e) exclude
`Deschidere`; `--declaratie-pe-baza` n-o vede (n-are document); (f) INCLUDE
partidele de deschidere (`Operare ⊕ Transfer ⊕ Deschidere`), comparate nu cu
`PartideDeschise` (care nu le are), ci cu poziția `BalantaNivel3` la data
respectivă MINUS stingerile scrise — check-ul stă în Import1C (`Deschidere`),
nu în ModelCheck. ModelCheck: `STR-DESCHIDERE` (Σ per cont = rânduri bloc, Σ
per lot = rânduri de stoc, balanță pe fiecare `Carte`, `Fel = Deschidere`
unic), `STR-DESCHIDERE-PARTIDA` (partidă per (cont, partener, document),
stinsă ulterior de o PLT ⇒ `Transfer`), pe ambele profiluri; scenele care azi
scriu rânduri de deschidere direct trec prin `Deschide`.

### T-D8 — Retururile și DVI

RDC: linia fără lot = venit `4111 = 70x` cu `−V` (`PastreazaSemn`), `4111 =
4427` cu `−TVA` (direcția `Colectat`); linia cu lot = cost `6xx = 3xx` cu
`−V` și `+q` pe lotul ORIGINAL (intrare la valoarea liniei, nu `Evaluare.
Iesire`), capătul 6xx cu `−q` pe gestiunea virtuală `Client`, fără fapt fiscal
(`TipTvaId = null`, `Retururi.cs:171`). Partida: RDC deschide partidă proprie
pe 4111 cu rest negativ (sensul = semnul restului × rolul contului, 090 (d));
nominalizarea partidei FCL originale e a lui TR-D9 (compensarea trece prin
notă, 46f) — declarat. Operandul primește totalul FILTRAT prin `LiniiCreanta`
doar dacă declarantul are nevoie de el (azi nu: postează per linie).

RLF: T-D6.

DVI: fără net (zero `RegulaContare`), postare fiscală per linie `4426 = 446`
(sau `= 401` prin `SursaCont.RepartitorPredator`) cu direcția `Deductibil`,
`4426 = 4427` pe tipurile `TaxareInversa` (B-r4 rămâne deschisă: fără fapt
colectat); dacă 446 are `RolTert`, DVI deschide partidă (S-D16) deși
`PoateFiStins = false` — hook-ul rămâne al registrelor până la TR-D9,
declarat. Zero documente pe Flax ⇒ oracolul e exclusiv `DVI-V*` (14 probe) +
`STR-DVI`. „DVI pe loturi" (reevaluarea cu `Atribuit`) NU intră (TR-D9).

### T-D9 — LDI și sink-urile bugetare (TR-r7 intră cu LDI)

LDI: linia `Minus` = `6xx = 3xx` cu `−q` pe lotul numit, `Evaluare.Iesire`,
capătul 6xx pe gestiunea virtuală `Consum` (090 (g)); linia `Plus` = `3xx =
7588` (bugetar `791`) cu `+q` pe lotul născut de linie la `PretEvaluare`,
contrapartida din rândul `LDI/plus` (`SursaCont.Explicit`). Bugetar: rândurile
`RegulaStoc` cu `TipStoc = Folosinta` (clasa `OF`) și `Custodie` (clasa `MC`)
devin, în cub: `Folosinta` ⇒ gestiune virtuală nouă `GestiuniVirtuale.
Folosinta` (constantă a nucleului, ca `Furnizor/Client/Consum`), `Custodie`
⇒ postare pe contul 803x al politicii cu `Valoare = 0` și `Cantitate = q`
DACĂ planul bugetar are contul; dacă nu-l are, `Custodie` intră ca gestiune
virtuală `Custodie` pe același cont, declarat, iar TR-r7 rămâne deschisă cu
constatarea. `Gratuit` primește constanta, nefolosită de niciun seed (0
rânduri pe Flax). Injectivitatea `PoliticaMiscareSaft` NU intră (TR-D8/D9).
Maparea `TipStoc → (gestiune virtuală | cont)` e o funcție pură în
`Declaratii/` citită din `RegulaStocFapt.TipStoc` (dată), fără `is`.

### T-D10 — Δ de sold 3xx (TR-r12) devine litera (h) a reconcilierii: măsurat, descompus, niciodată absorbit

`--reconciliere-cub` capătă litera (h): per cont de stoc (3xx) și lună, Σ
`Valoare` din cub (`Operare` pe ambele spații; `Transfer` exclus, ca la
balanță) − Σ `RegistruContabil` `!Storno`, descompusă pe grupul-sursă (BTR
care schimbă contul, ASM, `Deschidere`, NTC fără lot). Pe Flax, cifrele de
referință sunt cele din `nucleu-transfer.md:253-262` și `b2c-invers.txt:77-82`
(+585.404,66 total: 371 +404.030,06, 3028 +134.853,80, 303 +26.236,26, 3024
+17.539,80, 381 +2.744,74; BTR Mărfuri → Magazie 69.323,46 pe 3028 + 13.874,18
pe 303; ASM −88.712,43 pe 371; deschiderea 9.502.451,21 pe 371…). Litera (h)
NU intră în criteriul „0 rânduri Δ" — e raport; literele (a)–(g) rămân la
toleranță 0 cu excluderile declarate în T-D3/T-D7. Cine are dreptate (cubul
sau registrul contabil) se tranșează prin contractul 1 al reconcilierii 1C la
TR-D8, când balanța se citește din cub și se compară cu balanța 1C
(`[1] sold per cont OMFP` din raportul Import1C); raportul Import1C NU se
modifică în felia asta (rămâne identic cu baseline-ul).

### T-D11 — Litera (f) după felie: non-vacuă, cu excluderi NUMITE

După ce toate tipurile sunt migrate, (f) se evaluează pe fiecare cont cu
`RolTert` la ultima perioadă închisă, cu o singură excludere declarată și
raportată cu cifrele ei: partidele `(Cont, Partener)` atinse de o postare
fără unitate (T-D3 (A)). Partidele de deschidere n-au corespondent în
`PartideDeschise` și se verifică în conector (T-D7), nu aici. Restul (pe
Flax: partidele deschise de FCT/FCL/RDC/RLF/DVI/NIR în 2025 pe conturi pe
care nicio notă fără partener nu le atinge) trebuie să dea 0 Δ față de
`PartideDeschise.Rest`. Dacă mulțimea rămasă e vidă pe Flax, se spune, cu
motivul; nu se declară „(f) verde".

### T-D12 — Regula de oprire a agenților (S-D12 + doi termeni)

S-D12 (a)–(j) rămân literă cu literă. În plus: (k) agentul NU lansează
subagenți (un nivel; raportul unui subagent lansat de un agent numit se
pierde — dovedit 2026-09-21); (l) raportul final se scrie ȘI în
`run-nucleu/tr-d7b/<pas>/raport.md` (director gitignorat) înainte de
livrare — fișierul e canalul de rezervă, cu `path:line` și cifre, fără
substituenți.

### T-D13 — Laturile documentului ca structură: `Parte` (intern/extern) pe repartitor și felul permis per latură pe tip (owner, 2026-09-22)

Modelul mental e cel din legacy: `GEST_DEFA_DOCUM` fixa per tip combinația
predator → primitor pe intern/extern (FCT extern → intern, FCL intern →
extern, BCS/BTR/LDI intern → intern, DEC extern → intern; `db/inventar/`).
Azi regula e re-derivată în șase declaranți (helpere private în
`DeclarantTrezorerie`, `Calitati.LocConsum` în BCS, felul exact pe latură în
FCT/FCL/BTR/DSC) și a doua oară în validările claselor de document
(`is Gestiune`, 14 locuri) — contra „o singură sursă de reguli" (42a).
Nu e restanță: e pasul 2b, înaintea NTC, primul tip cu repartitor pe linie.

(a) `RepartitorFapt` primește `Parte`: `Extern` (Partener, Angajat),
`Intern` (Gestiune, UnitateInterna), `Propriu` (ContPropriu); derivată o
singură dată, în `Fapte`, din `Fel`. `Fel` rămâne pentru ce cere felul exact
(contul contrapartidei prin `RolTert`/`ContImplicit`).
(b) `Document` declară contractul laturilor prin METODĂ polimorfă, ca
`Declarant()`: `Laturi()` → per latură partea permisă + calitatea cerută
(`LocConsum` pe primitorul BCS). Structură = cod (decizia 4): fără tabelă de
politică, fără `switch` pe tip.
(c) Un singur loc de refuz: `Contractare` verifică laturile ÎNAINTEA
declarantului, cu codurile existente `PREDATOR_NEPOTRIVIT`/
`PRIMITOR_NEPOTRIVIT`; declaranții își pierd verificările proprii.
Validarea claselor de document (ușa de Committing, 42a) apelează ACEEAȘI
metodă, nu o a doua listă; `is Gestiune` se taie la atingere.
(d) Nucleul și cubul NEATINSE: gestiunea virtuală rămâne semnalul de extern
pe postare; `Parte` nu ajunge pe `Capat` și nu are coloană. Nicio migrație.
(e) XAF și React rămân înghețate (090 (m)): filtrarea lookup-urilor
Predator/Primitor pe partea permisă e consumatorul natural al lui (b) și
intră la dezgheț, consemnată în `lista-react.md`, nu acum.
(f) Probe: `STR-LATURI-CONTRACT` — fiecare tip de document declară
`Laturi()` și fiecare document de probă existent (`STR-*`, `F*-D*`,
`DVI-V*`, scenele SAF-T) îl respectă; `STR-LATURI-REFUZ` — un document cu
latura de partea greșită e refuzat pe ambele uși (comandă și Committing) cu
același cod. Gate-urile pe clonă ale pașilor 1–2 (BTR, FCL, DSC, FCT, BCS,
PLT, INC) rămân IDENTICE cu cifrele lor: 0 refuzuri noi.
(g) Terțul nominalizat pe capătul extern (owner, 2026-09-22): DSC pune
`Partener = Primitor` pe capătul 607 când primitorul e `Extern` (FCT o face
deja prin partidă; BCS n-are capăt virtual). Oracolul primește normalizarea
`TrD13TertulPeCapatulExtern` (rândul vechi 607 nu poartă partener), cu contor
în gate. Citirea „net livrat / net primit per (Partener, Produs) pe gestiunile
virtuale" e a lui TR-D8.
Oprire: un document de pe clona Flax refuzat de contractul laturilor e
oprire, nu lărgire tăcută — se raportează tipul, felul real al laturii și
numărul.

**Amendamente la implementare (pasul 2b, 2026-09-22):**

- **(a')** `Parte` e derivată o singură dată, dar ca proprietate calculată pe
  `RepartitorFapt` (`Laturi.ParteA(Fel)`), nu ca al doilea câmp umplut în
  `Fapte`: un fapt nu poate purta `Fel` și `Parte` contradictorii.
- **(b')** `Latura` = parte permisă + calitate cerută + **felul exact, doar
  unde structura îl cere**: `Lot.Gestiune` e tipat `Gestiune`, deci laturile
  care poartă stoc (BCS/BTR/ASM/DSC/RLF/LDI predator, FCT/NIR/RDC primitor,
  BTR/ASM primitor) sunt `Latura.Gestiune` — altfel o unitate internă ar
  trece contractul și ar cădea mai jos, pe nașterea lotului (constatat pe
  proba `Api ASM: predator ne-Gestiune`). Pe laturile fără stoc rămâne
  granularitatea părții: `UnitateInterna`/`Gestiune` interschimbabile (DVI,
  PIF, CAS, DEC primitor), `Partener`/`Angajat` la fel pe cele externe;
  `Angajat` nu mai e admis pe laturile interne (BCS/LDI primitor, NTC/ITV) —
  vechile validări îl admiteau; pe Flax nu apare (recensământul de mai jos).
- **(c')** `CONT_PROPRIU_LIPSA` și `LATURA_CONT_PROPRIU_NEPOTRIVITA` (B-r2)
  sunt de neatins sub contractele `Plata`/`Incasare` și au fost scoase;
  `DocumentFapt.LaturaContPropriu` la fel. Coloana `TipDocument.LaturaContPropriu`
  și rândul ei de seed rămân până la prima migrație (T-r9; (d) cere nicio
  migrație acum). `STR-LATURA` asertează acum `PREDATOR_NEPOTRIVIT` ca singur
  refuz (contul propriu pe primitorul plății e legal: viramentul).
  `Contractare.Mesaj` (`COD: mesaj [linie]`) e formatul unic al ambelor uși;
  `Materializare.Refuzuri` îl refolosește.
- **(f')** Proba „fiecare document de probă existent îl respectă" e ușa
  entității însăși: fiecare scenă operează prin `ValideazaOperare`, iar
  scenele își purjează documentele, deci o verificare finală pe bază ar fi
  vacuă (măsurat: 0 documente operate la finalul rulării). Pe date reale,
  recensământul laturilor pe clona `Atlas.Conta.Import1C.Flax.TrD7b`
  (toate documentele, toate operate), fiecare pereche admisă de contract:

  | Tip | Predator → Primitor (fel real, calități) | Documente |
  |---|---|---|
  | ASM | Gestiune → Gestiune | 1.228 |
  | BCS | Gestiune → UnitateInterna (LocConsum) | 547 |
  | DSC | Gestiune → Partener | 36.696 |
  | FCL | UnitateInterna (LocConsum) → Partener | 40.535 |
  | FCT | Partener → Gestiune | 19.035 |
  | INC | Partener → ContPropriu | 31.381 |
  | ITV | UnitateInterna → UnitateInterna | 12 |
  | LDI | Gestiune → UnitateInterna (Comisie) | 18 |
  | NIR | Partener → Gestiune | 17.814 |
  | NTC | UnitateInterna → UnitateInterna | 7.818 |
  | BTR | Gestiune → Gestiune | 45.552 |
  | PLT | ContPropriu → Partener | 2.486 |
  | RDC | Partener → Gestiune | 1.666 |
  | RLF | Gestiune → Partener | 398 |

  Nicio latură cu `Angajat`; DEC, DVI, PIF, CAS, AMO, BPR n-au documente pe
  Flax. Gate-urile pașilor 1–2 re-rulate cu contractul activ sunt proba pe
  cele 7 tipuri migrate (cifrele la pasul 2b).
- **(g')** Normalizarea `TrD13TertulPeCapatulExtern` rulează DUPĂ M6 (care
  șterge partenerul de pe capetele fără partidă), pe piciorul contabil al
  unei linii care doar iese (aceeași identificare ca T-D4.1, `IesirilePure`),
  cu terțul din `Context.TertPrimitor` (document → primitor de parte externă,
  prin ACEEAȘI derivare `Laturi.ParteA`). E numărată în `Normalizari.Contoare`
  (gate: `Normalizari.Contoare ×n`), nu în avertismente — avertismentele pică
  probele `STR-OPERARE`.

## Ce NU intră (amânări cu nume)

- Culegerea unității pe linie (NTC, regularizarea avansului), împerecherea ca document, partida ca rând, tăierea registrelor/conexului/`DescarcareService`: TR-D9.
- Citirile pe cub (balanță, fișe, „documente cu rest" cu semantica 090 (d), soldurile ITV din cub, contractul 1 al reconcilierii pe balanța 1C): TR-D8.
- „DVI pe loturi" (reevaluarea cu `Atribuit`), producția reală `345 = 711`, reziduul RLF → 658/758, `PoliticaMiscareSaft` re-cheiată: TR-D9.
- Partidele de deschidere în REGISTRE (rânduri bloc cu repartitor, `PartideDeschise` din deschidere): registrele mor la TR-D9; cubul le are din T-D7.
- B-r3, B-r4, B-r5, B-r8/S-r3, TR-r4, TR-r5: rămân deschise cu motivele lor.
- XAF și React: ÎNGHEȚATE (090 (m)); singura atingere permisă = `Client/src/generated/*` dacă metadata se schimbă (nu ar trebui: nicio proprietate nouă).

## Pașii (un agent per pas; main verifică independent și comite per pas)

0. **Contractul** (main): fișierul de față; `.gitignore` cu `/agenti-msg/`; commit.
1. **BTR pe cub + felul mixt** (T-D2): forma mișcării cu fel în nucleu (proprietate: o declarație cu mișcări `Transfer` produce Σ per (Cont, Latura) = 0 pe ele; una cu ambele feluri produce două tranzacții balansate), `Materializare` împarte pe set, `Storneaza`/`Anuleaza` acoperă ambele, `DeclarantNotaTransfer`, override, seed `BTR`, T-D2.1 constatat pe `RegistruStoc` și pin-uit în contract, litera (e) amendată, oracolul: rândurile BTR pe același cont pliate ca `Transfer`; probe `STR-BTR-ACELASI-CONT`, `STR-BTR-CONT-DIFERIT`, `STR-BTR-MIXT`, `STR-BTR-STORNO`; ModelCheck verde pe ambele profiluri; `--declaratie-pe-baza <clonă> BTR` = 100 % egal sau fiecare diferență declarată aici. Oprire: (c), (d), (e), (g), (i). **ÎNCHIS 2026-09-21** (agent F32-P1 + verificarea main-ului): nucleu 165 teste, 0 avertismente; ModelCheck 1698 OK privat / 1434 OK bugetar, 0 FAIL; gate BTR 45.017/45.552 egale + 535 declarate (T-D2.2); `--reconciliere-cub` 0 Δ pe ambele baze; diff gol pe Blazor.Server/WebApi/Client; `STR-BTR-CONT-DIFERIT`/`STR-BTR-MIXT` = proprietățile nucleului (T-D2.1).
2. **FCL ∪ DSC pe cub** (T-D4): `DeclarantFacturaIesire`, `DeclarantDescarcareGestiune`, gestiunea virtuală `Client` folosită, două partide pe FCL cu avans, transferul FCL ↔ INC activ, grupurile în (a)/(b)/(c) + `Incomplete`, probe `STR-FCL-*`, `STR-DSC-*`, `STR-FCL-AVANS-DOUA-PARTIDE`, `STR-FCL-INC-TRANSFER`; toate probele `FCL*`/`DSC*` existente materializează; bugetar fără DSC; gate pe clonă FCL și DSC (cu cifrele transferurilor). Oprire: (c), (d), (g). **ÎNCHIS 2026-09-22** (agent F32-P2 + verificarea main-ului): ModelCheck 1765 OK privat / 1452 OK bugetar, 0 FAIL; nucleu 165; gate FCL 40.535/40.535 egale, 0 refuzate; gate DSC 35.854/36.696 egale, 0 refuzate, 842 declarate (T-D4.2); gate FCT 19.022 + 13 declarate, identic cu felia 31 (`Fiscal.Impozitul` pe direcție neutru); `--reconciliere-cub` 0 Δ pe ambele baze (privat: BCS, BTR, DSC, FCL, FCT, INC, PLT; bugetar fără DSC); diff gol pe Blazor.Server/WebApi/Client/Nucleu; T-D4.1 (normalizare), T-D4.3 (fix `StocService.SolduriLaData`) și amendamentul grupurilor consemnate în T-D4.
2b. **Laturile ca structură** (T-D13; înaintea pasului 3, fiindcă NTC e primul tip cu repartitor pe linie și primul consumator al lui `Parte`): `Parte` pe `RepartitorFapt` derivată în `Fapte`, `Document.Laturi()` pe TOATE tipurile de document (și cele fără `PosteazaInCub`), `Contractare` verifică înaintea declarantului, validarea claselor refolosește metoda, declaranții pierd helperele proprii, `is Gestiune` tăiat în clasele atinse, DSC cu `Partener` pe capătul 607 + normalizarea în oracol cu contor, probe `STR-LATURI-CONTRACT`, `STR-LATURI-REFUZ`; nucleu neatins; ModelCheck verde pe ambele profiluri; gate-urile pașilor 1–2 re-rulate pe clonă cu cifre identice; diff gol pe Nucleu/Blazor.Server/WebApi/Client. Oprire: (c), (d), (g) + oprirea din T-D13. **ÎNCHIS 2026-09-22** (main, direct): `Declaratii/Laturi.cs` (`Parte`, `Latura`, `ContractLaturi`, `Laturi.Verifica` pe ambele uși), `Document.Laturi()` abstract cu 19 override-uri, `Contractare` verifică înaintea declarantului, cele 6 declaranți fără verificări de latură, `is Gestiune` tăiat din toate clasele, DSC cu `Partener` pe 607, normalizarea `TrD13TertulPeCapatulExtern` + `Normalizari.Contoare`; amendamentele (a')–(g') mai sus. ModelCheck 1771 OK privat / 1458 OK bugetar, 0 FAIL (20/20 tipuri cu contract; `STR-LATURI-*`, `STR-LATURA` amendată; probele ASM/RDC/PLT-Api pe cod); `--reconciliere-cub` 0 Δ pe ambele baze; gate pe clona TrD7b (3 h 04, `run-nucleu/tr-d7b/pas2b/`) IDENTIC cu pașii 1–2: BTR 45.017/45.552 + 535 declarate, FCL 40.535/40.535, DSC 35.854/36.696 + 842 declarate, FCT 19.022 + 13 declarate, BCS 547/547, PLT 2.486/2.486, INC 31.381/31.381, 0 refuzate pe toate; `Normalizari.Contoare ×62.063` TR-D13 pe DSC (= toate liniile lui); diff gol pe Nucleu/Blazor.Server/WebApi/Client.
3. **NTC + ITV pe cub** (T-D3): `DeclarantNotaContabila`, override pe `NotaContabila` (ITV îl moștenește), nominalizarea FIFO pe `(Cont, Partener)` cu `N.Fifo.Nominalizeaza` (B) și fallback-ul fără unitate (A), conectorul pune partenerul din subconto pe repartitorul liniei (`NoteComune` + `Punte`), gardul `Conservare.cs:133` verificat/relaxat cu proprietate, contoarele în gate (picioare cu partener nominalizate / fără partener per cont; 3xx fără lot per corespondență), `Imperecheaza` sărit fără partidă proprie, litera (f) cu excluderea (A), probe `STR-NTC-EXPLICIT`, `STR-NTC-PARTENER-FIFO`, `STR-NTC-AVANS-DOUA-UNITATI`, `STR-NTC-TERT-FARA-UNITATE`, `STR-NTC-STOC-FARA-LOT`, `STR-NTC-COMPENSARE`, `STR-ITV`; familia `F21-D*` materializează; gate pe clonă NTC și ITV cu cele cinci corespondențe la cifrele din B2.7 și cu măsura (B)/(A) pe cele 7.824; Import1C pe o lună (ianuarie) pe clonă ca să se vadă efectul partenerului pe `Imperecheri`. Oprire: (c), (d), (g) — un refuz pe o notă pe care motorul vechi o operează e oprire, nu normalizare.
4. **RDC + RLF + DVI pe cub** (T-D6, T-D8): trei declaranți, seed, probe `STR-RDC-VENIT-COST`, `STR-RDC-PARTIDA-NEGATIVA`, `STR-RLF-FISCAL-REZIDUU`, `STR-DVI`; `DVI-V*` și contractele API `ReturClientApply`/`ReturFurnizorApply` materializează; gate pe clonă RDC și RLF (DVI n-are documente — se spune). Oprire: (c), (d), (g).
5. **ASM + LDI + NIR pe cub** (T-D2 pe ASM, T-D5, T-D9): `DeclarantAsamblare` (fel mixt refolosit), `DeclarantListaDiferente` + constantele `Folosinta`/`Gratuit`/`Custodie` + maparea pură `TipStoc →`, `DeclarantNir` + excluderea conexului prin `PoliticaConex` (probată: FCT cu NIR conex operat ⇒ o singură recepție în cub, `STR-NIR-CONEX-EXCLUS`), probe `STR-ASM-ACELASI-CONT`, `STR-ASM-PRODUCTIE`, `STR-LDI-PLUS-MINUS`, `STR-LDI-BUGETAR-SINK`, `STR-NIR-MANUAL`; gate pe clonă ASM, LDI, NIR (NIR: 0 documente manuale — se spune; cele 17.814 conexe verificate NEmaterializate). Oprire: (c), (d), (g); TR-r7 fără cont 803x ⇒ constatare, nu inventare.
6. **Deschiderea + literele (h) și (f) finale** (T-D7, T-D10, T-D11): `Materializare.Deschide` (contabil bloc + stoc pe lot + terți pe partidă per (cont, partener, document 1C)), `Materializare.Imperecheaza` pe partidă fără document, `Randuri.Citeste` fără aruncare, `Import1C/Deschidere.cs` citește terții din `BalantaNivel3` și înregistrează partidele ca ținte în `MigrareLegatura`, `Imperecheri1C` rezolvă țintele din 2024 pe partide și le stinge în cub (contoarele mutate din „sărite"), comentariul din `Deschidere.cs:43-50` și decizia 047 corectate (`Stare: amendată de TR-D7b`), `SolduriPartener` înlocuit, grupul `{Deschidere}` în (a)/(b), (e) exclude `Deschidere`, litera (h) cu descompunerea, (f) cu excluderea (A), probe `STR-DESCHIDERE`, `STR-DESCHIDERE-PARTIDA`; `--reconciliere-cub` pe bazele ModelCheck 0 Δ pe (a)–(g). Oprire: (g); dacă poziția `BalantaNivel3` nu se leagă determinist de referința folosită de `Imperecheri1C` (tipuri de document diferite), se raportează cu cifrele, nu se ghicește.
7. **Proba supremă** (main lansează procesul detașat; agentul pregătește `run-nucleu/tr-d7b/import/run.ps1` din rețeta feliei 31 cu baseline-ul `reconciliere-20260921-035646.txt`: Import1C integral `--recreeaza --cititori --inchide-lunile` → `--reclasifica` → diff sortat → `--reconciliere-cub` (a)–(h) → `--dump-integritate-tph` → `privat.ps1` → `refuzuri.ps1`; sumar cu cifrele: tranzacții/postări/transferuri per fel, refuzuri 0, excluderile (f) cu numele, (h) descompus). Oprire: (h) din S-D12.
8. **Review advers** (agent separat, read-only, tier-ul main-ului; zone: documentul cu două tranzacții la storno cross-perioadă și la anulare, conexul exclus la storno/anulare (NIR conex stornat când FCT e stornat), DSC fără sursă, transferul FCL ↔ INC pe FCL cu avans (care partidă e „de referință"), NTC negativ pe cont de terț și compensarea `411 = 411`, reziduul RLF pe lot golit apoi re-intrat de RDC, deschiderea rescrisă la `--recreeaza` cu perioade închise, stingerea pe partida de deschidere peste rest sau pe partener greșit (cheia 1C), nota cu partener care nominalizează FIFO partida altui document decât cel din 1C, (f) care „trece" pentru că mulțimea e vidă, seed-ul care întoarce `PosteazaInCub` pe o bază unde owner-ul l-a oprit). Fix-urile le aplică main-ul.
9. **Docs și închidere** (main): `stare-curenta/domeniu-si-operare.md` (regimul dual complet, felul mixt, excluderea conexului, deschiderea, abaterile declarate), `dezvoltare-si-validare.md` (literele (a)–(h), excluderile, rețeta), `restante.md` (TR-r2/r7/r10/r12 amendate sau închise cu constatarea; T-r*), `istoric-plan-de-lucru.md` (felia 32), `nucleu-transfer.md` Stare, CLAUDE.md §Stare/§Următorul pas (TR-D8), memorie.

## Regula de oprire a feliei

- `PosteazaInCub = true` în seed pentru toate cele 15 tipuri pe profilul unde tipul există (privat: toate; bugetar: cele ne-inerte), pe ambele profiluri; niciun `is`/`switch` pe frunză; `Operand.cs`/`Fapte.cs` ≤ 16 interogări (proba existentă).
- ModelCheck verde pe AMBELE profiluri cu toate `Check`-urile de azi + probele `STR-*` noi; nucleu `dotnet test` verde, 0 avertismente, testul de arhitectură neatins.
- `--declaratie-pe-baza` pe clona Flax pentru BTR, FCL, DSC, NTC, RDC, ASM, RLF, LDI, ITV = 100 % egal sau fiecare diferență declarată în T-D2…T-D10 cu cifra ei; NIR și DVI: 0 documente, spus explicit, oracolul = scenele.
- Import1C integral pe Flax: exit 0, raport IDENTIC pe conținut sortat cu `reconciliere-20260921-035646.txt`, ZERO refuzuri ale declarației, 12/12 luni închise cu 0 constatări; `--reconciliere-cub` 0 rânduri Δ pe (a)–(g) cu excluderea T-D3 (A) listată nominal și cu cifra ei; (h) raportat și descompus; stingerile pe partidele de deschidere cu cifrele (create / plafonate / sărite) și check-ul Σ per (cont, partener) = `BalantaNivel3` − stingeri; cifrele consemnate în contract; NIR conex: 0 tranzacții; `--dump-integritate-tph` 0; `refuzuri.ps1` toate PASS pe Privat refăcută.
- Contractul laturilor (T-D13) declarat pe toate tipurile de document, refuzul laturii într-un singur loc, `STR-LATURI-*` verzi, `Parte` absentă din Nucleu și din cub.
- Diff-ul pe Blazor.Server/WebApi/Client gol în afara fișierelor generate; `MotorOperare.cs` atins doar pentru excluderea conexului (T-D5), dacă nu încape în `Materializare`.
- Review advers aplicat; docs din pasul 9 în commit-ul de închidere; `Stare: ÎNCHISĂ` cu data. Decizie proprie NU e necesară (felia execută 090 (l)); amendamentele de literă (T-D2 la 090 (a), T-D3 la 090 (c), T-D6 la 090 (j)) se consemnează aici și în `restante.md`.

## Restanțe noi (T-r*)

| Id | Conținut | Stare |
|---|---|---|
| T-r1 | 090 (a) „EXACT o tranzacție `Operare`" devine „cel mult una `Operare` și cel mult una `Transfer`, cel puțin una" pentru tipurile cu linii care nu schimbă contul (BTR, ASM); litera (e) amendată; textul deciziei 090 nu se rescrie, amendamentul e aici | deschisă |
| T-r2 | reziduul valoric lăsat de RLF pe lotul golit (valoare fiscală ≠ raportul lotului) contrazice 090 (j); se rezolvă la TR-D9 prin re-evaluarea unității cu reziduul spre 658/758 din politică | deschisă |
| T-r3 | postările NTC pe conturi cu `RolTert` care rămân FĂRĂ partener după (B) (măsurat la pasul 3 din cele 7.824) și pe 3xx fără lot (3.365, TR-r2): declarate până la TR-D9; litera (f) le exclude nominal | deschisă |
| T-r4 | `Deschidere.cs:43-50` și decizia 047 afirmă că 1C nu defalcă soldul de terț pe partener la 01.01 — FALS: defalcarea (partener × contract × document) e în `BalantaNivel3`; se corectează la pasul 6, TR-r6 (partea de deschidere) și TR-r10 se închid | deschisă |
| T-r5 | DVI deschide partidă pe 446 prin S-D16 deși `PoateFiStins = false`; hook-ul e al registrelor până la TR-D9 | deschisă |
| T-r6 | `Custodie` pe bugetar: cont 803x sau gestiune virtuală, după cum are planul bugetar contul — constatat la pasul 5 | deschisă |
