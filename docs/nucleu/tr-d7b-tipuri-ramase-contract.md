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
| V (diferențele se raportează) | T-D3 (NTC fără unitate), T-D6 (RLF la valoare fiscală), T-D7 (deschiderea fără partener), T-D10 (Δ 3xx) sunt consecințe DECLARATE cu cifră, fiecare cu litera ei în gate; Import1C identic pe conținut sortat |
| VI (fără interogare per linie) | `Fapte.Operand` rămâne ≤ 16 interogări pe toate tipurile; DSC citește loturile liniilor pe set (`SolduriLoturi` există); deschiderea scrie pe set |

## Deciziile (T-D1…T-D12)

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

Evaluarea: ieșirea din lotul sursă e `N.Evaluare.Iesire` pe raportul curent
(090 (j), ca BCS); intrarea pe lotul țintă e la aceeași valoare. Pe același
cont, unitatea rămâne ACELAȘI lot cu altă `Gestiune` dacă motorul vechi ține
lotul; dacă motorul vechi naște lot nou pe destinație, unitatea țintă e lotul
nou — pasul 1 CONSTATĂ pe `RegistruStoc` (cheia de stoc a destinației) și
declarantul urmează registrul; forma se pin-uiește ca amendament (T-D2.1)
înainte de gate.

Aceeași regulă pentru ASM: consumul (−q pe lotul consumat) și produsul (+q pe
lotul născut de linia `Produs`, la `PretEvaluare`) sunt `Transfer` dacă
ambele loturi sunt pe același cont, altfel `Operare`. Invariantul ASM
(Σ produse = Σ consumuri, toleranță 0,005) e conservarea valorii pe spațiul
Stoc — nu se re-implementează în declarant; diferența sub toleranță, dacă
există, se postează ca reziduu DOAR dacă o politică o cere (090 (j)), altfel
refuz `ASAMBLARE_NEBALANSATA`.

### T-D3 — NTC: postare explicită CA ATARE; unitatea pe linie e a lui TR-D9, iar lipsa ei e DECLARATĂ, nu refuzată

Nota contabilă postează exact perechea de conturi a liniei, cu valoarea ca
atare (negativă inclusiv, S-D14), fără normalizare de semn, cu dimensiunile
liniei; linia cu `ContDebit = ContCredit` (compensările 1C prin 891,
`411.1 = 411.1`) e permisă — e vehiculul stingerii până la TR-D9.

Pe un cont cu `RolTert`: `Partener` = repartitorul laturii dacă e de fel
`Partener`, altfel `null`; `Unitate = null` — nota NU deschide și NU numește
partidă. E abaterea declarată de la 090 (c) („nota nu atinge o coordonată cu
unitate fără s-o numească") pe durata dintre TR-D7b și TR-D9, când culegerea
unității pe linie e dezghețul declarat în 090 (m). Consecințe declarate:
(1) litera (f) a reconcilierii EXCLUDE, cu numele lor, partidele
`(Cont, Partener)` atinse de o postare fără unitate (numărul și Σ raportate
per cont — pe Flax 7.824 picioare, 4.068 pe 4111); (2) un NTC stingător
(`CapacitateStingere`) care ajunge în `Materializare.Imperecheaza` fără
partidă proprie e `Sarit` (ca plafonul 0), contorizat; (3) regularizarea
avansului pe 419 rămâne, în cub, fără nominalizare până la TR-D9.

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

Grupurile literei (a)/(b)/(c): `{FCL ∪ DSC secundar al FCL}` și `{DSC fără
sursă}`; `Incomplete` raportează FCL cu DSC neoperat. Transferul FCL ↔ INC
prin `Materializare.Imperecheaza` se ACTIVEAZĂ cu pasul 2 (până acum ieșea
devreme, FCL nemigrat): plafonul e restul partidei FCL pe 4111 (regula
MAJOR-A a feliei 31), `Imperecheri.Data` ca dată; cifrele (transferuri
scrise, plafonate, sărite) intră în raportul pasului.

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

### T-D7 — Deschiderea ca tranzacție `Deschidere`, scrisă de `Cub.Materializare`, 1:1 cu rândurile bloc, fără partener cât timp sursa nu-l dă (TR-r10, TR-r6)

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

Unitatea per `(Cont, Partener)` pe terți intră DOAR când sursa o dă (090 (d),
TR-D1): 1C nu poartă defalcarea la 01.01 (`Deschidere.cs:43-50`, două rânduri
cont × partener în `BalantaNivel1`), deci pe Flax deschiderea de terți rămâne
FĂRĂ partener și fără partidă, iar 65,5 % din soldul de terț rămâne nestingibil
în cub, cum e și în registre azi. Consecință declarată: litera (f) exclude cu
numele conturile cu `RolTert` care au postare de deschidere fără partener
(pe Flax toate), până la TR-r6 (date noi din 1C). `Deschide` acceptă, când
apelantul îl dă, partenerul per rând și deschide partida (`DeschidePartida`
cu `documentDeschizator = Guid.Empty`, data deschiderii) — probat pe scenă
(`STR-DESCHIDERE-PARTENER`), nefolosit de conector.

Gate: grup nou `{Deschidere}` (rândurile cu `DocumentId = null`) la literele
(a) și (b); litera (e) exclude tranzacția `Deschidere`; `--declaratie-pe-baza`
nu o vede (n-are document). ModelCheck: `STR-DESCHIDERE` (Σ per cont = rânduri
bloc, Σ per lot = rânduri de stoc, balanță pe fiecare `Carte`, `Fel =
Deschidere` unic) pe ambele profiluri; scenele care azi scriu rânduri de
deschidere direct trec prin `Deschide`.

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
`RolTert` la ultima perioadă închisă, cu două excluderi declarate și
raportate cu cifrele lor: partidele `(Cont, Partener)` atinse de o postare
fără unitate (T-D3) și conturile cu deschidere fără partener (T-D7). Restul
(pe Flax: partidele deschise de FCT/FCL/RDC/RLF/DVI/NIR în 2025 pe conturi
pe care nicio notă nu le atinge) trebuie să dea 0 Δ față de
`PartideDeschise.Rest`. Dacă mulțimea rămasă e vidă pe Flax, se spune, cu
motivul; nu se declară „(f) verde".

### T-D12 — Regula de oprire a agenților (S-D12 + doi termeni)

S-D12 (a)–(j) rămân literă cu literă. În plus: (k) agentul NU lansează
subagenți (un nivel; raportul unui subagent lansat de un agent numit se
pierde — dovedit 2026-09-21); (l) raportul final se scrie ȘI în
`run-nucleu/tr-d7b/<pas>/raport.md` (director gitignorat) înainte de
livrare — fișierul e canalul de rezervă, cu `path:line` și cifre, fără
substituenți.

## Ce NU intră (amânări cu nume)

- Culegerea unității pe linie (NTC, regularizarea avansului), împerecherea ca document, partida ca rând, tăierea registrelor/conexului/`DescarcareService`: TR-D9.
- Citirile pe cub (balanță, fișe, „documente cu rest" cu semantica 090 (d), soldurile ITV din cub, contractul 1 al reconcilierii pe balanța 1C): TR-D8.
- „DVI pe loturi" (reevaluarea cu `Atribuit`), producția reală `345 = 711`, reziduul RLF → 658/758, `PoliticaMiscareSaft` re-cheiată: TR-D9.
- Deschiderea de terți per partener / per factură: cerință de DATE din 1C (TR-r6), nu de cod; `Deschide` o acceptă, conectorul n-o dă.
- B-r3, B-r4, B-r5, B-r8/S-r3, TR-r4, TR-r5: rămân deschise cu motivele lor.
- XAF și React: ÎNGHEȚATE (090 (m)); singura atingere permisă = `Client/src/generated/*` dacă metadata se schimbă (nu ar trebui: nicio proprietate nouă).

## Pașii (un agent per pas; main verifică independent și comite per pas)

0. **Contractul** (main): fișierul de față; `.gitignore` cu `/agenti-msg/`; commit.
1. **BTR pe cub + felul mixt** (T-D2): forma mișcării cu fel în nucleu (proprietate: o declarație cu mișcări `Transfer` produce Σ per (Cont, Latura) = 0 pe ele; una cu ambele feluri produce două tranzacții balansate), `Materializare` împarte pe set, `Storneaza`/`Anuleaza` acoperă ambele, `DeclarantNotaTransfer`, override, seed `BTR`, T-D2.1 constatat pe `RegistruStoc` și pin-uit în contract, litera (e) amendată, oracolul: rândurile BTR pe același cont pliate ca `Transfer`; probe `STR-BTR-ACELASI-CONT`, `STR-BTR-CONT-DIFERIT`, `STR-BTR-MIXT`, `STR-BTR-STORNO`; ModelCheck verde pe ambele profiluri; `--declaratie-pe-baza <clonă> BTR` = 100 % egal sau fiecare diferență declarată aici. Oprire: (c), (d), (e), (g), (i).
2. **FCL ∪ DSC pe cub** (T-D4): `DeclarantFacturaIesire`, `DeclarantDescarcareGestiune`, gestiunea virtuală `Client` folosită, două partide pe FCL cu avans, transferul FCL ↔ INC activ, grupurile în (a)/(b)/(c) + `Incomplete`, probe `STR-FCL-*`, `STR-DSC-*`, `STR-FCL-AVANS-DOUA-PARTIDE`, `STR-FCL-INC-TRANSFER`; toate probele `FCL*`/`DSC*` existente materializează; bugetar fără DSC; gate pe clonă FCL și DSC (cu cifrele transferurilor). Oprire: (c), (d), (g).
3. **NTC + ITV pe cub** (T-D3): `DeclarantNotaContabila`, override pe `NotaContabila` (ITV îl moștenește), gardul `Conservare.cs:133` verificat/relaxat cu proprietate, contorul postărilor fără unitate per cont și al celor pe 3xx fără lot per corespondență în gate, `Imperecheaza` sărit fără partidă proprie, litera (f) cu excluderea T-D3, probe `STR-NTC-EXPLICIT`, `STR-NTC-TERT-FARA-UNITATE`, `STR-NTC-STOC-FARA-LOT`, `STR-NTC-COMPENSARE`, `STR-ITV`; familia `F21-D*` materializează; gate pe clonă NTC și ITV cu cele cinci corespondențe la cifrele din B2.7. Oprire: (c), (d), (g) — un refuz pe o notă pe care motorul vechi o operează e oprire, nu normalizare.
4. **RDC + RLF + DVI pe cub** (T-D6, T-D8): trei declaranți, seed, probe `STR-RDC-VENIT-COST`, `STR-RDC-PARTIDA-NEGATIVA`, `STR-RLF-FISCAL-REZIDUU`, `STR-DVI`; `DVI-V*` și contractele API `ReturClientApply`/`ReturFurnizorApply` materializează; gate pe clonă RDC și RLF (DVI n-are documente — se spune). Oprire: (c), (d), (g).
5. **ASM + LDI + NIR pe cub** (T-D2 pe ASM, T-D5, T-D9): `DeclarantAsamblare` (fel mixt refolosit), `DeclarantListaDiferente` + constantele `Folosinta`/`Gratuit`/`Custodie` + maparea pură `TipStoc →`, `DeclarantNir` + excluderea conexului prin `PoliticaConex` (probată: FCT cu NIR conex operat ⇒ o singură recepție în cub, `STR-NIR-CONEX-EXCLUS`), probe `STR-ASM-ACELASI-CONT`, `STR-ASM-PRODUCTIE`, `STR-LDI-PLUS-MINUS`, `STR-LDI-BUGETAR-SINK`, `STR-NIR-MANUAL`; gate pe clonă ASM, LDI, NIR (NIR: 0 documente manuale — se spune; cele 17.814 conexe verificate NEmaterializate). Oprire: (c), (d), (g); TR-r7 fără cont 803x ⇒ constatare, nu inventare.
6. **Deschiderea + literele (h) și (f) finale** (T-D7, T-D10, T-D11): `Materializare.Deschide`, `Randuri.Citeste` fără aruncare, `Import1C/Deschidere.cs` o cheamă, grupul `{Deschidere}` în (a)/(b), (e) exclude `Deschidere`, litera (h) cu descompunerea, (f) cu cele două excluderi numite, probe `STR-DESCHIDERE`, `STR-DESCHIDERE-PARTENER`; `--reconciliere-cub` pe bazele ModelCheck 0 Δ pe (a)–(g) cu excluderile listate. Oprire: (g); dacă `Deschide` cere un fapt pe care scenele nu-l au, se raportează.
7. **Proba supremă** (main lansează procesul detașat; agentul pregătește `run-nucleu/tr-d7b/import/run.ps1` din rețeta feliei 31 cu baseline-ul `reconciliere-20260921-035646.txt`: Import1C integral `--recreeaza --cititori --inchide-lunile` → `--reclasifica` → diff sortat → `--reconciliere-cub` (a)–(h) → `--dump-integritate-tph` → `privat.ps1` → `refuzuri.ps1`; sumar cu cifrele: tranzacții/postări/transferuri per fel, refuzuri 0, excluderile (f) cu numele, (h) descompus). Oprire: (h) din S-D12.
8. **Review advers** (agent separat, read-only, tier-ul main-ului; zone: documentul cu două tranzacții la storno cross-perioadă și la anulare, conexul exclus la storno/anulare (NIR conex stornat când FCT e stornat), DSC fără sursă, transferul FCL ↔ INC pe FCL cu avans (care partidă e „de referință"), NTC negativ pe cont de terț și compensarea `411 = 411`, reziduul RLF pe lot golit apoi re-intrat de RDC, deschiderea rescrisă la `--recreeaza` cu perioade închise, (f) care „trece" pentru că mulțimea e vidă, seed-ul care întoarce `PosteazaInCub` pe o bază unde owner-ul l-a oprit). Fix-urile le aplică main-ul.
9. **Docs și închidere** (main): `stare-curenta/domeniu-si-operare.md` (regimul dual complet, felul mixt, excluderea conexului, deschiderea, abaterile declarate), `dezvoltare-si-validare.md` (literele (a)–(h), excluderile, rețeta), `restante.md` (TR-r2/r7/r10/r12 amendate sau închise cu constatarea; T-r*), `istoric-plan-de-lucru.md` (felia 32), `nucleu-transfer.md` Stare, CLAUDE.md §Stare/§Următorul pas (TR-D8), memorie.

## Regula de oprire a feliei

- `PosteazaInCub = true` în seed pentru toate cele 15 tipuri pe profilul unde tipul există (privat: toate; bugetar: cele ne-inerte), pe ambele profiluri; niciun `is`/`switch` pe frunză; `Operand.cs`/`Fapte.cs` ≤ 16 interogări (proba existentă).
- ModelCheck verde pe AMBELE profiluri cu toate `Check`-urile de azi + probele `STR-*` noi; nucleu `dotnet test` verde, 0 avertismente, testul de arhitectură neatins.
- `--declaratie-pe-baza` pe clona Flax pentru BTR, FCL, DSC, NTC, RDC, ASM, RLF, LDI, ITV = 100 % egal sau fiecare diferență declarată în T-D2…T-D10 cu cifra ei; NIR și DVI: 0 documente, spus explicit, oracolul = scenele.
- Import1C integral pe Flax: exit 0, raport IDENTIC pe conținut sortat cu `reconciliere-20260921-035646.txt`, ZERO refuzuri ale declarației, 12/12 luni închise cu 0 constatări; `--reconciliere-cub` 0 rânduri Δ pe (a)–(g) cu excluderile T-D3/T-D7 listate nominal și cu cifra lor; (h) raportat și descompus, cifrele consemnate în contract; NIR conex: 0 tranzacții; `--dump-integritate-tph` 0; `refuzuri.ps1` toate PASS pe Privat refăcută.
- Diff-ul pe Blazor.Server/WebApi/Client gol în afara fișierelor generate; `MotorOperare.cs` atins doar pentru excluderea conexului (T-D5), dacă nu încape în `Materializare`.
- Review advers aplicat; docs din pasul 9 în commit-ul de închidere; `Stare: ÎNCHISĂ` cu data. Decizie proprie NU e necesară (felia execută 090 (l)); amendamentele de literă (T-D2 la 090 (a), T-D3 la 090 (c), T-D6 la 090 (j)) se consemnează aici și în `restante.md`.

## Restanțe noi (T-r*)

| Id | Conținut | Stare |
|---|---|---|
| T-r1 | 090 (a) „EXACT o tranzacție `Operare`" devine „cel mult una `Operare` și cel mult una `Transfer`, cel puțin una" pentru tipurile cu linii care nu schimbă contul (BTR, ASM); litera (e) amendată; textul deciziei 090 nu se rescrie, amendamentul e aici | deschisă |
| T-r2 | reziduul valoric lăsat de RLF pe lotul golit (valoare fiscală ≠ raportul lotului) contrazice 090 (j); se rezolvă la TR-D9 prin re-evaluarea unității cu reziduul spre 658/758 din politică | deschisă |
| T-r3 | postările NTC pe conturi cu `RolTert` fără unitate (7.824 pe Flax) și pe 3xx fără lot (3.365, TR-r2): declarate până la TR-D9; litera (f) le exclude nominal | deschisă |
| T-r4 | deschiderea de terți fără partener (65,5 % din sold): `Deschide` acceptă partenerul, conectorul nu-l are (TR-r6); (f) exclude conturile atinse | deschisă |
| T-r5 | DVI deschide partidă pe 446 prin S-D16 deși `PoateFiStins = false`; hook-ul e al registrelor până la TR-D9 | deschisă |
| T-r6 | `Custodie` pe bugetar: cont 803x sau gestiune virtuală, după cum are planul bugetar contul — constatat la pasul 5 | deschisă |
