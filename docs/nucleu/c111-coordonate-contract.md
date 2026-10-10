# C111 — coordonatele postării: contract

**Data:** 2026-10-10. **Stare:** pin-uit cu owner-ul pe C111-Q1…Q16
(2026-10-10). Pasul 0 e făcut. Review-ul advers Codex al contractului
(C111-R1…R16) e reconciliat mai jos, în trei runde; amendamentul 1 e retras. Pașii 1 și 1b
sunt făcuți. Codex a închis review-ul de contract (2026-10-10,
`comunicari/2026-10-10-1732-…-c111-contract-review-inchis.md`). Pasul 2 e
implementat și verificat pe catalog; C111-Q12…Q14 au apărut la implementare.
**Bază:** main `1cc9d93`. **Decizia:**
[111](../decizii/111-repartitor-unic-si-registre-tipate.md), aprobată
2026-10-10. **Măsurătorile:** [pasul 0](tr-d9b-pas0-masuratori.md).

Felia implementează 111 (a)–(e) și (i): un singur repartitor de sold pe
postare, partenerul și locul fiscal în blocul fiscal, marcajul contraponderii
în locul gestiunilor virtuale. E o reformare: nicio sumă nu se schimbă. TR-D9b
(unitățile) vine după, pe forma nouă.

## Fapte măsurate (main `1cc9d93`)

| Fapt | Cifra | Sursa |
|---|---|---|
| Postări cu ambele coloane, pe Flax ianuarie | 22.389 din 82.154, niciuna neexplicată | pasul 0 §1 |
| Postări fiscale al căror partener e latura numită de politica de TVA | 15.206 din 15.206 | pasul 0 §2 |
| D394 și D300 cu partenerul mutat în blocul fiscal | identice octet cu octet | pasul 0 §2 |
| Prototipul nucleului (`proto/tr-d9b-pas0-nucleu`, `8c36600`) | 195 / 195, +229 / −106 linii, 15 fișiere | pasul 0 §3 |
| Referințe la `Partener` / `Gestiune` ca nume de coordonată | modul 43 de fișiere, ModelCheck 59, Import1C 9 | `grep`, 2026-10-10 |
| Gestiuni virtuale citite ca valoare distinctă în produs | numai `Transformare` și `Inventar` (4 locuri) | `grep`, 2026-10-10 |
| Felul repartitorului ca fapt al operandului | există: `Motor/Fapte.cs:356`, din `ClrType`, citit ca dată | cod |
| Conturi private cu flag `Repartitor` | zero: planul OMFP pornește fără defalcare | `ProfilPrivat.cs:233` |
| Conturi bugetare cu flag `Repartitor` | 20 | pasul 0 §5 |

`Furnizor`, `Client` și `Consum` sunt scrise de declaranți și comparate numai
de scenele ModelCheck. SAF-T citește postările cu lot, pe gestiune reală, iar
`PoliticaMiscareSaft` e cheiată pe tip de document, tip de stoc și semn.

## C111-D1 — Perimetrul

Intră: forma coordonatelor în nucleu și în cub, scrierea (materializarea,
deschiderea, stornoul), declaranții, cititorii din `Cub/Citiri`,
`Proiectii`, `Saft`, `Motor`, snapshot-ul de solduri, `PostareVizual`,
declarația felurilor pe cont cu gardul ei, ModelCheck, Import1C,
`ScaraVolum.sql`, contractul HTTP al citirilor atinse.

Nu intră: (f), originea partidei, valuta, reevaluarea, hook-urile de
stingere, custodia, furnizorul bonului la decont. Lista completă e în §„Ce NU
intră".

## C111-D2 — Forma postării

**Nucleu.** `Coordonate` și `Capat` pierd `Partener` și `Gestiune` și primesc:

| Câmp | Tip | Regula |
|---|---|---|
| `Repartitor` | `Repartitor?` = `(Guid Id, FelRepartitor Fel)` | coordonata de sold; nul = niciunul |
| `Contrapondere` | `Contrapondere?` = terț, consum, inventar, transformare | nul = postare în evidență |
| `PartenerFiscal` | `Guid?` | numai pe postarea cu fapt fiscal |
| `Loc` | `Guid?` | locul intern al postării când nu e repartitorul ei de sold: pe orice postare cu fapt fiscal (latura internă a documentului) și pe piciorul intern căruia titularul unei partide i-a luat locul (C111-Q8, Q10, Q14) |

`GestiuniVirtuale` dispare. Forma e cea a prototipului `8c36600`, adusă peste
main. `FelRepartitor` e unul singur, în nucleu; cel din
`Declaratii/Operand.cs` iese.

**Cub.** `Postare` pierde `Partener` și `Gestiune` și primește `Repartitor`
(`uuid`, nulabil, cheie străină pe `Repartitori`, pe ambele partiții),
`Contrapondere` (`smallint`, nulabil), `PartenerFiscal` și `Loc`
(`uuid`, nulabile, chei străine pe `Repartitori`). Felul nu e coloană
(111 (a)). Indecșii
parțiali de pe `Partener` trec pe `Repartitor`.

**Invarianții.** 1–4 stau în `Conservare` și în `INV-CUB`. 5 ține prin
construcție la storno, care copiază coordonatele originalului, și se verifică
în `INV-CUB` (`Citiri/Invarianti.cs`) pe identitatea originalului
(`InversaDin`, id și spațiu), nu pe ordinalul perechii: `Conservare`
vede numai tranzacția curentă, iar stornoul peste mai multe tranzacții mută
ordinalele.

1. cantitatea nemarcată cere lot și repartitor intern;
2. lotul cere repartitor intern; partida cere repartitor extern, egal cu
   titularul ei;
3. `PartenerFiscal` apare numai cu fapt fiscal; `Loc`, numai cu fapt fiscal
   sau pe o postare cu partidă; faptul fiscal cere `Loc`, iar
   `PartenerFiscal` poate lipsi, ca azi;
4. contraponderea transformării n-are repartitor, unitate, valoare sau fapt
   fiscal; în `INV-CUB` forma se cere pe orice postare marcată, cu sau fără
   pereche (C111-P2-R2);
5. inversa fiscală poartă același `PartenerFiscal` și același `Loc` ca
   originalul.

Nicio formă refuzată azi nu devine acceptată, cu o excepție numită
(C111-Q11): decontul cu fapt fiscal și cu terț pe linie, altul decât
titularul, pe un cont care urmărește partide. Azi îl refuză nucleul
(`UNITATE_NEPOTRIVITA`), fiindcă partenerul fiscal e scris peste titularul
partidei; după S10 cele două stau separat și forma trece. A doua excepție e
la S14.

## C111-D3 — Felul repartitorului

Nucleul primește felul ca valoare în operand. Sursa e una:
`Motor/Fapte.Repartitori`. Părțile rămân cele din `Laturi.ParteA`: extern
(partener, angajat), intern (gestiune, unitate internă), propriu (cont
propriu).

Postările recitite din cub pentru nucleu își iau felul din același cititor.
Căile care recitesc:

- stornoul, împerecherea și desfacerea transferului (`Cub/Materializare.cs`);
- stingerea deschiderii (`Materializare.StingereDeschidere.cs`);
- poziția fără fișă (`Materializare.PozitieFaraFisa.cs`);
- verificarea retragerii de lot (`Citiri/Loturi.cs`);
- recepția istorică a NIR-ului (`ReceptiiConexe.cs`);
- capetele construite direct din proiecția cubului (`Motor/ImobilizariFapte.cs`).

`Randuri.Citeste` primește felurile ca argument. Ele se citesc în lot, pentru
reuniunea repartitorilor postărilor reconstruite, nu câte o interogare pe
postare. Felul lipsă sau necunoscut e refuz explicit: nu se deduce din cont și
nu se face nul.

## C111-D4 — Contul declară felurile admise

`Cont.FeluriRepartitor`, mulțime peste `FelRepartitor` plus `Niciunul`,
înlocuiește flag-ul `Repartitor` din `Cont.DimensiuniObligatorii`.
`DimensiuneFlags` rămâne pentru celelalte dimensiuni.

| Declarația contului | Gardul |
|---|---|
| mulțime vidă | nu judecă repartitorul (C111-Q1 = lume deschisă) |
| mulțime fără `Niciunul` | cere repartitor, de unul din feluri |
| mulțime cu `Niciunul` | admite lipsa; dacă există, de unul din feluri |

Gardul rămâne `GardAnaliza`. Regulile lui:

- aceeași regulă de prezență și de fel se aplică pe capetele mișcărilor din
  cartea contabilă și pe postările de deschidere
  (`Materializare.Deschidere.cs`);
- conturile păzite sunt cele care declară feluri sau alte dimensiuni, fiecare
  familie independent de cealaltă;
- pe postarea marcată cu contrapondere, lipsa repartitorului nu e refuz în
  gardul mișcărilor, ca azi, când gestiunea virtuală ține loc de repartitor;
  felul unui repartitor prezent se judecă, iar celelalte dimensiuni se judecă
  integral;
- gardul NIR (`ReceptiiConexe.VerificaAnaliza`) rămâne cum e: pe
  contraponderea de inventar cere terțul real când contul cere repartitor;
- mutările și transformările rămân în afara gardului (D9-r1).

`ContFapt.CereRepartitor` se derivă din mulțime. Seed-ul bugetar declară cele
20 de conturi: 552.00.00 = cont propriu (hotărât); 462.01.09 = partener și
angajat, 437.01.00, 437.02.00 și 803.00.01 = partener sau niciunul
(C111-Q15, 111-r2); restul după tabelul din pasul 0 §5. Seed-ul privat nu
declară nimic, ca azi.

Probe la pasul 3: 552 la deschidere cu repartitor absent, extern și cont
propriu; un cont cu mulțime vidă; SC-PLT-09 și SC-INC-09 pe gardul nou.

## C111-D5 — Ce repartitor poartă fiecare picior

Regula e una: piciorul poartă repartitorul al cărui sold îl mișcă. Proveniența
repartitorului de sold e a declarantului (latura, linia, regula de contare);
proveniența partenerului fiscal e a politicii de TVA. Niciuna nu se deduce din
clasa contului sau din `CereRepartitor`. Când apare terțul pe picior nu se
schimbă față de azi; se schimbă numai unde stă.

| Piciorul | Azi | După |
|---|---|---|
| intern, fără fapt fiscal (stoc, cheltuială, venit, fișă) | `Gestiune` | `Repartitor` = același |
| intern, cu fapt fiscal (3xx, 6xx, 7xx, 4426, 4427) | `Gestiune` + `Partener` | `Repartitor` = repartitorul intern; `PartenerFiscal` = partenerul; `Loc` = locul |
| cu fapt fiscal, fără repartitor intern (autocolectarea la taxare inversă) | `Partener` | `Repartitor` nul; `PartenerFiscal` = partenerul; `Loc` = locul |
| de terț, cu partidă sau cu cont care cere repartitor | `Partener` | `Repartitor` = terțul capătului |
| de terț, nominalizat pe linie (DEC, NTC), cu sau fără partidă ori obligație pe cont | `Partener` | `Repartitor` = terțul liniei |
| intern, cu partidă și fapt fiscal (419, 408, 4091) | `Partener` + `Gestiune` | `Repartitor` = titularul; `PartenerFiscal` = partenerul; `Loc` = locul |
| intern, cu partidă, fără fapt fiscal (aceleași conturi, pe o linie fără tip de TVA) | `Partener` + `Gestiune` | `Repartitor` = titularul; `Loc` = locul (C111-Q14) |
| cu fapt fiscal, al decontului | `Partener` = partenerul fiscal, scris peste repartitorul liniei | `Repartitor` = repartitorul liniei; `PartenerFiscal` = partenerul (S10); `Loc` = locul |
| de trezorerie, propriu, numit de regulă prin latură | `Gestiune` = contul propriu | `Repartitor` = contul propriu |
| de trezorerie, propriu, cu ambele conturi numite explicit | niciuna | `Repartitor` nul, ca azi (SC-PLT-09, SC-INC-09) |
| contracantitatea recepției și a returului la furnizor | virtuala `Furnizor` | `Contrapondere` = terț |
| contracantitatea descărcării de gestiune și a returului de la client | virtuala `Client` | `Contrapondere` = terț |
| plusul și minusul de inventar | virtuala `Inventar` / `Consum` | `Contrapondere` = inventar / consum |
| contraponderea asamblării | virtuala `Transformare` | `Contrapondere` = transformare, fără repartitor |

Pe piciorul marcat, terțul rămâne `Repartitor` unde e azi `Partener`
(diferența de la NIR, DSC cu primitor extern, returul de la client).

`PartenerFiscal` are un singur scriitor, `Fiscal.CuFapt`. `Loc` are doi:
`Fiscal.CuFapt`, pe faptul fiscal, și `Terti.Capat`, când pune partida peste
un capăt care avea repartitor intern (C111-Q14). În al doilea caz locul e
repartitorul intern pe care capătul îl avea; pe faptul fiscal rămâne cel
scris de `CuFapt`. Locul nu intră în balanță, în fișă sau în snapshot.

- **Partenerul fiscal** e latura numită de `PoliticaTva.SursaContrapartida`,
  și nimic nu-l mai scrie după `CuFapt`. O sursă care nu numește o latură
  (`Explicit`, `TipMaterial`) îl lasă nul, iar D394 raportează faptul între
  neincluse. Azi regula ține numai pe capătul neatins de `Terti.Capat`: pe
  capătul cu partidă acesta scrie terțul documentului peste partener, iar
  pe contul care cere repartitor completează partenerul nul (S13).
- **Locul fiscal** e latura internă a documentului, citită din faptele
  tipate ale laturilor (`RepartitorFapt.Parte`), nu din politică (C111-Q10).
  Cele șase tipuri cu fapt fiscal au, prin `Document.Laturi()`, exact o
  latură internă: FCT, DEC, RDC și DVI pe primitor, FCL și RLF pe predator.
  `Fiscal.GestiuneaInterna` iese. Un document cu fapt fiscal fără exact o
  latură internă rămâne fără loc și e refuzat de invariantul 3.
- Locul e al documentului, nu al piciorului: baza și taxa aceluiași document
  poartă același loc, și când repartitorul de sold al bazei e altul
  (decontul cu repartitor intern pe linie, SC-C111-23).
- `Loc` se scrie pe orice postare cu fapt fiscal, și când e egal cu
  `Repartitor`, ca niciun cititor să nu aleagă între două coloane.
- Piciorul de taxă al direcției are ca repartitor de sold locul fiscal.
  Capătul de taxă pe care decontul îl reconstruiește își păstrează amândouă
  atributele.
- `INV-CUB` verifică felul locului: `Loc` e repartitor intern.

## C111-D6 — Schimbările de comportament, nominal

Orice schimbare în afara listei oprește pasul (D9).

| Id | Schimbarea | Unde se vede |
|---|---|---|
| S1 | o singură axă analitică, repartitorul de sold, în locul celor două de azi (`Partener` și `Gestiune`); detaliile sub tabel | balanța, fișa, soldul pe parteneri, snapshot-ul, filtrele HTTP |
| S2 | piciorul intern cu partidă nu mai apare la filtrul pe locul intern, cu sau fără fapt fiscal; locul rămâne pe postare, ca `Loc` (C111-Q14) | balanța și fișa filtrate pe repartitor, pentru 419, 408, 4091 |
| S3 | `Furnizor` și `Client` devin „terț” | lista postărilor, cititorii de contrapondere, scenele ModelCheck |
| S4 | refuzuri noi ale nucleului: partener fiscal fără fapt; loc fără fapt fiscal și fără partidă; fapt fiscal fără loc; cantitate nemarcată fără lot | numai pe un declarant greșit sau pe un tip cu fapt fiscal fără exact o latură internă |
| S5 | lista postărilor arată `Repartitor`, `Partener fiscal`, `Loc`, `Contrapondere` în locul celor două coloane | `PostareVizual` |
| S6 | gardul refuză un repartitor de alt fel decât cele admise de cont | conturile care declară (bugetar) |
| S7 | piciorul contului de tranzit al viramentului nu mai poartă contul propriu | analitica lui 581 |
| S8 | DSC cu primitor intern marchează „consum”, nu „terț” | lista postărilor |
| S9 | fișa contului și registrul jurnal citesc contrapartida prin pereche | cont unic pe rândurile cu pereche; închide D9-r4 |
| S10 | la decont, repartitorul liniei nu mai e suprascris de partenerul fiscal pe piciorul cu fapt fiscal; forma cu terț pe linie, altul decât titularul, pe cont cu partide nu mai e refuzată (C111-Q11) | analitica piciorului de cheltuială al decontului; acceptarea formei. Atingerea prin culegere e citită în `Api/Dec/DecontApply.cs:143`; de operat la pasul 2 |
| S11 | în SAF-T, terțul comercial al postării pe un cont cu rol e repartitorul ei extern, fără rezervă pe partenerul fiscal (C111-Q9); detaliile sub D10 | registrul, facturile, Customers și Suppliers, pe postarea fără partidă de pe un cont cu rol comercial, fie că acel cont urmărește partide, fie că nu |
| S12 | locul fiscal și repartitorul piciorului de taxă se iau din latura internă tipată a documentului, nu din politica de TVA (C111-Q10) | la decont, baza cu repartitor intern pe linie are alt loc decât repartitorul ei; în afara seed-ului, taxa pe FCL sau RLF cu sursă `Explicit` nu mai poartă terțul ca loc |
| S13 | partenerul fiscal e cel numit de politica de TVA și pe capătul cu partidă ori pe contul care cere repartitor; azi `Terti.Capat` scrie acolo terțul documentului (C111-R16) | citirea fiscală și D394, numai când politica nu numește latura terțului: cu sursă `Explicit` sau `TipMaterial` baza trece la „fără partener”, cu latura internă la „contrapartida nu e partener”. Pe cele șase politici din seed latura numită e chiar terțul documentului, deci nu se schimbă nimic |
| S14 | poziția fără fișă a imobilizărilor nu mai are partenerul în cheie: e cont, repartitor, produs, valută, analiză (C111-Q12); detaliile sub tabel | gardul `POZITIE_FARA_FISA_NEGATIVA`, suportul și disponibilul de nominalizare ale PIF; postările PIF nu mai poartă furnizorul suportului |

**S6, în detaliu** (pasul 3). Înainte, contul cu flag era mulțumit de orice
repartitor, deci și de locul notei, care e intern. Pe bugetar, nota contabilă
pe un cont de terți (401.01.00, 411.01.01 și celelalte care declară numai
feluri externe) fără partener pe linie e acum refuzată. Scenele care foloseau
un asemenea cont drept contrapartidă generică trec, pe bugetar, pe 542.01.00,
cont cu partide care nu declară feluri; pe privat rămân neschimbate.

**S14, în detaliu** (C111-Q12; găsită la implementarea pasului 2). Azi cheia
poziției include partenerul postării. Pe piciorul intern el există numai
când achiziția are fapt fiscal, iar PIF îl copiază de pe suport pe postările
lui. Pe forma nouă PIF nu-l poate purta: n-are fapt fiscal (invariantul 3).
Urmările:

- achizițiile de la furnizori diferiți, pe același loc, intră în aceeași
  poziție;
- o notă fără partener, refuzată azi fiindcă grupul ei fără partener iese
  negativ, trece cât timp totalul locului rămâne pozitiv. E a doua formă
  refuzată azi care devine acceptată, pe lângă cea din D2;
- furnizorul se regăsește prin referința la suport (`SuportId`), care duce
  la postarea și la documentul de achiziție, cu sau fără TVA.

**S1, în detaliu** (C111-Q6):

- piciorul intern apare pe repartitorul lui intern, cu sau fără fapt fiscal,
  și nu mai apare la „fără repartitor”;
- partenerul care era numai fiscal dispare de pe axă;
- contul propriu apare ca repartitor;
- `repartitorId` filtrează repartitorul de sold, de orice fel; `gestiuneId`
  iese din balanță, fișă și soldul pe parteneri; `repartitorNul` înseamnă fără
  niciun repartitor;
- „Sold parteneri” arată pe rând propriu numai repartitorii externi (partener,
  angajat). Restul contului, adică sumele fără repartitor, pe repartitor
  intern sau pe cont propriu, intră într-un singur rând „fără terț”, ca
  raportul să se închidă cu balanța contului (C111-Q13);
- snapshot-ul are aceeași axă, iar citirea dinamică și cea din snapshot dau
  aceleași cifre.

Proba de referință: SC-CIT-27 dă azi 230 fără repartitor, 11 pe furnizor și
241 în total; după, 230 pe `Loc`, 11 pe furnizor și 241 în total. Semantica
HTTP e cea de aici de la pasul 2; clientul se aduce la zi la pasul 7.

## C111-D7 — Schema, migrațiile, bazele

Fără migrare de date: lanțul se comprimă din nou în `InitialCreate`, iar
bazele de dezvoltare, clonele și bazele de import se recreează (102 (b);
C111-Q3, confirmată). `SolduriPerioada` pierde `GestiuneId` pe atomul contabil;
snapshot-ul de stoc își păstrează gestiunea lotului. `PostareVizual` se
rescrie pe coloanele noi (amendamentul 3 (g) al TR-D9a).

## C111-D8 — Constatările strânse în 111: dispoziția

| Constatarea | Dispoziția |
|---|---|
| 3xx la recepție poartă partenerul fiscal | rezolvată de D5; S1 |
| viramentul pune aceeași bancă pe ambele picioare | S7 |
| DSC pune virtuala Client și la primitor intern | S8 |
| LDI pierde comisia | nu e coordonată; nu intră |
| BCS pune lotul și locul pe 6xx | păstrat; invariantul 2 îl acoperă |
| la decont partenerul fiscal e angajatul; furnizorul bonului nu există | nu intră (C111-r1) |
| invariantul inversei fiscale nu verifică partenerul | intră: invariantul 5 |
| custodia | nu intră; TR-D9b, identitatea lotului |
| cele 152 de postări pe 419, 408, 4091 | S2; locul rămâne în `Loc` (C111-Q8) |
| 552.00.00 și 462.01.09 pe bugetar | 552 hotărât; 462.01.09, 437 și 803 hotărâte la pasul 3 (C111-Q15) |
| criteriul de formă al partidelor (F27-r16, F28-r1) | TR-D9b |

## C111-D9 — Pașii, verificarea, regula de oprire

| Pas | Ce | Verde când |
|---|---|---|
| 0 | linia de bază pe main: import Flax ianuarie, amprentele (`--amprente`), raportul SAF-T S, integrala ModelCheck pe ambele profiluri | făcut 2026-10-10: [`c111-pas0-linia-de-baza.md`](c111-pas0-linia-de-baza.md) |
| 1 | poarta, pe forma de azi: în afara `Cub/Citiri`, a scriitorului (`Cub/Randuri.cs`) și a entității, nimeni nu numește `Postare.Partener` sau `Postare.Gestiune`; cititorii expun identitățile din D10; o probă pe sursă păzește regula | făcut 2026-10-10: [`c111-pas1-poarta.md`](c111-pas1-poarta.md) |
| 1b | așteptările înaintea implementării: fișierul de catalog cu formele din D5 și secțiunea analitică a amprentelor | făcut 2026-10-10: [`c111-pas1b-asteptari.md`](c111-pas1b-asteptari.md) |
| 2 | forma nouă, atomic: nucleul, schema, scrierea, declaranții, cititorii din poartă, atomii și snapshot-ul, `PostareVizual`, mutanții și scrierile brute din ModelCheck; bazele recreate | implementat și verificat pe catalog 2026-10-10: [`c111-pas2-forma-noua.md`](c111-pas2-forma-noua.md); lista probată în browser; rămân bazele de dezvoltare |
| 3 | `Cont.FeluriRepartitor`, gardul, seed-ul bugetar | făcut 2026-10-10: [`c111-pas3-feluri.md`](c111-pas3-feluri.md) |
| 4 | S7 și S8 | scenariu propriu pe fiecare |
| 5 | S9: contrapartida prin pereche | fișa contului pe HTTP; cifra de timp față de pasul 0, pe aceeași bază |
| 6 | uneltele: Import1C, `ScaraVolum.sql`, PerfCub | import pe ianuarie: regulile de oprire 1 și 2; `INV-CUB` verde |
| 7 | contractul HTTP, clientul, `stare-curenta`, închiderea | drift zero; `refuzuri.ps1`; review advers |

**Scrierile brute și mutanții** (pasul 2). Probele care scriu sau strică
postări fără scriitorul produsului se mută semantic, nu mecanic:
`ScenariiConsumatori.cs:97`, `AcoperireInvarianti.Perechi.cs:58` și `:83`,
`AcoperireInvarianti.cs:108`, `ScenariiStocCub.cs:82`,
`ScenariiPartideCub.cs:421`, `ScenariiBtr.cs:34`, plus capătul fiscal
construit direct în `ScenariiDvi.cs:236`, care primește loc fiscal. Fiecare mutant rămâne
probă de domeniu, cu codul lui de refuz (`CITIRE_PARTIDE_INCOMPLETE` și
celelalte); așteptarea nu se lărgește la „orice refuz”. `ScaraVolum.sql`
scrie postări cu listă explicită de coloane și e indisponibil între pasul 2 și
pasul 6.

**Regula de oprire.**

1. Pe import, amprentele (balanța, stocul, partidele, D300, D394), raportul
   de reconciliere și raportul SAF-T S sunt identice cu pasul 0. În catalog,
   nicio așteptare de sumă pe cont nu se schimbă. O diferență oprește pasul.
   SAF-T L nu se generează pe ianuarie 2025, deci îl țin numai scenele din
   catalog.
2. Amprenta analitică (cont × repartitor × lună, cont × partener fiscal ×
   lună, cont × loc fiscal × lună, cont × contrapondere × lună) se calculează pe baza de referință prin
   derivarea din D5 și pe forma nouă direct. Diferențele admise sunt numai
   S7, S8, S10, S12 și S13, numite rând cu rând, și locul fiscal al postărilor
   care azi n-au gestiune (`c111-pas1b-asteptari.md`). Derivarea e valabilă
   numai pe o bază fără decont cu fapt fiscal: pe forma de azi repartitorul
   liniei de decont nu se poate reconstitui din postare. Baza de referință
   n-are niciun decont; decontul îl țin scenele numerice din catalog.
3. Fiecare formă din D5 are în catalog o așteptare cu repartitorul concret,
   partenerul și locul fiscal, contraponderea, unitatea și perechea, plus soldurile
   filtrate pe identități distincte, dinamic și din snapshot. Formele cerute:
   DEC simplu, DEC cu TVA, DEC cu taxare inversă, DEC cu repartitor intern pe
   linie diferit de locul documentului, DEC cu terț pe linie cu și fără
   partidă, FCL și RLF cu politica de TVA pe sursă `Explicit`, FCL cu baza
   pe cont cu partidă sub cele trei orientări ale politicii de TVA, postarea
   fără partidă pe cont cu rol comercial în SAF-T, NTC cu terț fără partidă,
   trezoreria cu conturi explicite, diferențele NIR, IMO cu suport istoric pe
   alt loc decât laturile documentului, deschiderea, stornoul peste mai multe
   tranzacții cu perechi locale suprapuse. Stornoul cu doi originali cu
   parteneri fiscali diferiți, cerut aici la început, nu e produs de
   declaranții de azi și nu e probat ca formă (SC-C111-20, C111-P2);
   invariantul 5 îl țin mutanții `FISCAL-INVERSA-PARTENER` și
   `FISCAL-INVERSA-LOC`.
4. Martorul: un repartitor intern A schimbat în B pe o notă fără unitate și
   fără fapt fiscal e prins de amprenta analitică și de catalog.
5. O schimbare de comportament care nu e în D6 oprește pasul și cere
   amendament aprobat.
6. Un declarant care nu se poate scrie pe regula din D5 fără o coordonată în
   plus oprește pasul 2: cazul se raportează, nu se ocolește.
7. O singură rulare grea o dată; importul și integrala nu rulează concurent.
8. Felia se închide cu review advers pe diff-ul întreg.

Costul scrierii cu două chei străine noi pe partiții se măsoară la pasul 6,
cu PerfCub pe baze recreate din același seed. Compararea e între baze
diferite, ca la TR-D9a, și se numește ca abatere.

## C111-D10 — Identitățile cititorilor (poarta)

Clasificarea e pe expresie și pe domeniul ei, nu pe fișier.

| Identitatea | Domeniul | Azi | După |
|---|---|---|---|
| titularul partidei | postarea cu `FelUnitate.Partida` | `Partener` | `Repartitor` |
| terțul piciorului | postarea contabilă a unui document, cu sau fără partidă (`Citiri/Plati`; SAF-T: Payments, numai evenimente fără fapt fiscal) | `Partener` | `Repartitor`, când e extern |
| partenerul fiscal | postarea cu fapt fiscal | `Partener` | `PartenerFiscal` |
| locul fiscal | postarea cu fapt fiscal | `Gestiune` | `Loc` |
| terțul comercial | postarea pe un cont cu `RolTert` (SAF-T: registrul și facturile) | `Partener` | `Repartitor`, când e extern; fără rezervă pe `PartenerFiscal` (S11) |
| cheia soldului de terț | atomul contabil pe un cont cu `RolTert` (SAF-T: Customers și Suppliers) | `Partener`, ca repartitor al atomului | `Repartitor` (S1): aceeași axă ca terțul comercial |
| locul lotului | postarea cu lot | `Gestiune` | `Repartitor` |
| repartitorul intern fără lot | locul fișei, poziția fără fișă | `Gestiune` | `Repartitor` |
| contraponderea | postarea cu gestiune virtuală | `Gestiune` | `Contrapondere` |
| axele analitice | atomul contabil | `Partener` și `Gestiune`, separat | `Repartitor` (S1) |

`Contabil.Jurnal` expune identitățile separat, nu un singur `Partener`.
Nomenclatorul SAF-T reunește identitățile pe care le caută consumatorii lui:
terțul comercial al registrului și al facturilor, repartitorul atomilor de pe
conturile cu rol, centrul de cost și terțul plăților; e o reuniune, scrisă ca
atare. Partenerul fiscal nu intră: după S11 nicio secțiune nu-l caută în
nomenclator, iar maparea fiscală se cheiază pe calificarea de TVA (C111-P2).
SAF-T citește și atomii
(`SaftProiectii.PeCub.cs:186`), deci se reverifică la pasul 2, nu numai la
pasul 1. Probele pasului 1: terț fără partidă, picior intern cu fapt fiscal,
cont cu rol comercial.

**S11, în detaliu** (C111-Q9, C111-R9). Registrul, facturile și soldurile
Customers și Suppliers citesc aceeași axă, repartitorul de sold. Postarea cu
partidă are ca repartitor titularul, deci pe ea nu se schimbă nimic. Regula
privește postarea fără partidă de pe un cont cu rol comercial. Că acel cont
urmărește partide nu garantează partida pe fiecare postare: decontul o
deschide numai pentru repartitor extern (`DeclarantDecont.cs:79`), iar nota
poate posta pe repartitorul ei intern. Baza de referință are 28.339 de
postări cu partidă pe conturi cu rol comercial și 559 fără, toate din note
contabile, pe unitate internă, fără partener și fără fapt fiscal.

- postarea pe repartitor intern, fără partener (cele 559): în registru
  rămâne avertismentul `TertFaraPartener`; soldul ei rămâne în `Neincluse`,
  dar cauza trece de la `FaraPartener` la `RepartitorNePartener`, cu
  repartitorul numit (urmarea lui S1);
- piciorul de decont cu terț T pe linie, altul decât titularul E, iese pe T
  în registru și în sold; azi poartă E, scris peste;
- piciorul intern cu fapt fiscal nu mai are terț comercial: în registru iese
  cu avertismentul `TertFaraPartener`, iar soldul lui cade în `Neincluse`, cu
  `RepartitorNePartener`; azi amândouă ies pe partenerul fiscal.

Payments rămân pe `Citiri/Plati`, pe terțul piciorului extern, și refuză ca
azi evenimentele cu fapt fiscal. Probele pasului 2: SC-C111-26 și SC-C111-27.

Scenele ModelCheck care compară rânduri întregi (`ScenaDocumente.cs:373`,
`:414`) rămân pe rândul complet la pasul 1 și își rescriu așteptările la
pasul 2, după regula de oprire 3.

## C111-Q — tranșările owner-ului (2026-10-10)

| Id | Întrebarea | Tranșarea |
|---|---|---|
| C111-Q1 | Contul care nu declară feluri: gardul nu judecă sau admite numai „niciunul”? | **lume deschisă**: gardul nu judecă; lumea închisă rămâne C111-r2 |
| C111-Q2 | Contrapartida prin pereche (S9, D9-r4) intră în C111? | **da**, ca pas separat (pasul 5) |
| C111-Q3 | Recomprimare în `InitialCreate` cu baze recreate, sau migrație cu transformarea datelor? | **recomprimare**, cu baze recreate |
| C111-Q4 | S7 (581 fără repartitor) și S8 (DSC intern = consum) intră? | **amândouă**, fiecare cu scenariu propriu (pasul 4) |
| C111-Q5 | Amendamentul 1 după review-ul Codex? | **retras**; pasul 1 e poarta prin cititori compozabili, pasul 2 rămâne atomic |
| C111-Q6 | Filtrele HTTP după S1? | **un singur filtru**: `repartitorId` pe repartitorul de sold; `gestiuneId` iese; „Sold parteneri” numai terți |
| C111-Q7 | La decont, când linia numește alt terț decât partenerul fiscal? | **amândoi, separat** (S10) |
| C111-Q8 | Locul intern al faptului fiscal se scrie pe postare? (reconfirmare: premisa „nu-l citește nimeni” era falsă, filtrul `gestiuneId` îl citește) | **da**, `Loc` pe orice postare cu fapt fiscal; nu intră în filtrul balanței; amendează 111 (b), închide 111-r1 |
| C111-Q9 | În SAF-T, cine e terțul comercial când postarea n-are repartitor extern? | **numai repartitorul extern**, fără rezervă pe partenerul fiscal; registrul, facturile și soldurile citesc aceeași axă (S11) |
| C111-Q10 | De unde se ia locul fiscal: din politica de TVA sau din laturile documentului? | **din laturile tipate ale documentului** (S12); `Fiscal.GestiuneaInterna` iese |
| C111-Q11 | Decontul cu TVA și cu terț pe linie, pe cont cu partide, refuzat azi, trece după S10? | **trece**; excepția e numită în D2 |
| C111-Q12 | Poziția fără fișă păstrează partenerul în cheie, citit prin suport, sau îl pierde? | **îl pierde** (S14); furnizorul rămâne recuperabil prin suport și prin documentul de achiziție |
| C111-Q13 | „Sold parteneri” numai cu terți lasă pe dinafară sumele fără terț; raportul se mai închide cu balanța? | **un rând „fără terț” pe cont**, cu tot ce nu e pe un repartitor extern; precizează C111-Q6 |
| C111-Q14 | Piciorul intern cu partidă și fără fapt fiscal își pierde locul (găsit la pasul 2, pe regularizarea de avans fără TVA)? | **nu**: `LocFiscal` devine `Loc` și se scrie oriunde titularul unei partide scoate locul din repartitor, cu sau fără fapt fiscal; semnalul se păstrează cât timp nu atinge cardinalitatea soldului. Amendează 111 (b) |

A doua serie (2026-10-10, seara):

| Id | Întrebarea | Tranșarea |
|---|---|---|
| C111-Q15 | 111-r2: felurile pe conturile bugetare fără dovadă în legacy? | 462.01.09 = **partener și angajat**; 437.01.00 și 437.02.00 = **partener sau niciunul**; 803.00.01 = **partener sau niciunul** |
| C111-Q16 | Când se recreează bazele de dezvoltare? | **după pasul 3**, o singură dată, pe schema cu `FeluriRepartitor` |

## Review-ul advers Codex al contractului (2026-10-10)

Mesajele: `comunicari/2026-10-10-1034-…-c111-contract-review.md` (R1–R7) și
`…-1105-…-c111-contract-amendament-1-review.md` (R8–R12). Fiecare constatare
a fost verificată în cod înaintea dispoziției.

| Id | Constatarea | Dispoziția |
|---|---|---|
| R1 | S1 omite postările interne fără fapt fiscal și semantica filtrelor | acceptată: S1 rescrisă, C111-Q6 |
| R2 | D5 nu acoperă terțul nominalizat fără partidă și faptul fiscal fără repartitor intern | acceptată: trei rânduri noi în D5; S10 |
| R3 | piciorul propriu cu conturi explicite are legitim repartitor nul | acceptată: rând în D5 |
| R4 | D4 omite deschiderea și selecția conturilor păzite | acceptată: D4 |
| R5 | scutirea contraponderilor e prea largă; NIR are gard propriu | acceptată: D4, scutirea e numai de prezență, gardul NIR rămâne |
| R6 | recitirile care cer felul nu sunt inventariate | acceptată: D3 |
| R7 | oracolul agregat nu vede un repartitor intern greșit | acceptată: regulile de oprire 2–4, pasul 1b |
| R8 | „cititorul numit nu se traduce în SQL” e fals | acceptată: amendamentul 1 retras |
| R9 | clasele nu sunt exhaustive și nu se atribuie pe fișier | acceptată: D10 |
| R10 | scrierea dublă nu acoperă istoricul | fără obiect: amendamentul 1 retras |
| R11 | scrieri în afara `Randuri`; mutanții se mută semantic | acceptată pentru pasul 2: D9 |
| R12 | starea intermediară 2c nedefinită; momentele S2 și S3 | fără obiect pentru 2c; S2 și S3 precizate în D6 |

Invariantul 5 se verifică pe `InversaDin`, cum cere răspunsul la întrebarea
despre storno (D2).

### Runda a doua (2026-10-10)

Mesajul: `comunicari/2026-10-10-1459-…-c111-reconciliere-si-loc-fiscal-review.md`.
Codex închide R1–R8 și R11, lasă R10 și R12 fără obiect, ține R9 deschisă și
adaugă R13–R15. Fiecare contraexemplu a fost recitit în cod; niciunul n-a
fost operat.

| Id | Constatarea | Dispoziția |
|---|---|---|
| R9 | terțul comercial nu e neutru după S10; soldurile Customers și Suppliers au altă sursă decât registrul | acceptată: C111-Q9, S11; D10 separă terțul comercial, cheia soldului și Payments |
| R13 | la decont, locul din antet poate diferi de gestiunea nenulă a postării, când linia numește alt repartitor intern | acceptată: locul e al documentului (111 (b)); diferența e S12, cu scena SC-C111-23 |
| R14 | `GestiuneaInterna` nu garantează un loc intern pe orice politică admisă | acceptată: C111-Q10, locul din laturile tipate; SC-C111-25 |
| R15 | S10 face acceptată o formă refuzată azi | acceptată: C111-Q11, excepția din D2; SC-C111-04 și SC-C111-24 |
| R6, completare | stornoul lipsea din lista recitirilor | adăugat în D3 |
| R5, completare | 111 (c) spune „scutește postările marcate”, mai larg decât D4 | 111 (c) precizată: scutirea e numai de prezență |
| R7, completare | derivarea amprentei analitice nu acoperă decontul fiscal | domeniul e restrâns în regula de oprire 2; baza de referință n-are decont, numărat |

### Runda a treia (2026-10-10)

Mesajul: `comunicari/2026-10-10-1719-…-c111-reconciliere-2-review.md`. Codex
închide R9 și R13–R15 ca direcție și adaugă R16. Lanțul lui e recitit în
cod (`Partide.cs:56`–`:58`, `DeclarantFacturaIesire.cs:55`); neoperat.

| Id | Constatarea | Dispoziția |
|---|---|---|
| R16 | partenerul fiscal scris de `CuFapt` e rescris azi de `Terti.Capat` pe capătul cu partidă; „ca azi” nu ține acolo | acceptată: ținta rămâne 111 (b), efectul e numit S13, cu scena SC-C111-28 |
| R9, completare | neutralitatea lui S11 pe seed nu decurge din flag-ul contului | acceptată: domeniul lui S11 e postarea fără partidă; recensământul bazei de referință e în D10 |

## Review-ul Codex al pasului 2 (2026-10-10)

Mesajul: `comunicari/2026-10-10-2038-…-c111-pas2-review.md`. Codex găsește
C111-Q12, Q13 și Q14 coerente cu ținta amendată și deschide două probleme de
validare. Fiecare constatare a fost recitită în cod; corecturile sunt
verificate pe integrală (`run-verificari/20261010-210057-804/`).

| Id | Constatarea | Dispoziția |
|---|---|---|
| C111-P2-R1 | `CK_Postare_BlocFiscal` admite `Loc` pe o postare fără fapt fiscal și fără unitate: comparația `"FelUnitate" = 2` dă nul, iar un `CHECK` respinge numai fals | acceptată: comparația e totală (`IS NOT DISTINCT FROM 2`); mutantul `COORD-LOC-FARA-UNITATE`, lângă cel pe lot; forma veche, reprodusă pe o tabelă temporară, primește rândul, cea nouă îl refuză |
| C111-P2-R2 | invariantul 4 nu se verifică în `INV-CUB` pe postările cu pereche: o notă obișnuită cu ambele postări marcate „transformare” trece și iese întreagă din balanță și din fișă | acceptată: `VerificaCoordonate` cere forma contraponderii pe orice postare marcată; mutantul `COORD-TRANSFORMARE-PE-PERECHE`; filtrul din `Citiri/Transformare` rămâne pe marcaj |
| D10 | nomenclatorul SAF-T nu reunește partenerul fiscal, cum spunea textul | textul D10 adus la cod: niciun consumator al nomenclatorului nu caută partenerul fiscal (S11) |
| Q13, completare | rândul fără terț nu era probat cu sume fără niciun repartitor | SC-C111-10 primește o plată cu ambele conturi explicite: 40 pe cont propriu și 7 fără repartitor dau un rând de 47, dinamic și după închidere |
| SC-C111-20 | forma cu parteneri fiscali diferiți e o limită a producătorilor de azi, nu a stornoului | regula de oprire 3 și starea scenei spun ce e probat; forma rămâne neprobată |

Întrebările 1–3 și 5 din predare au rămas fără constatare. La 7 (o inversă
care iese cu alt partener sau alt loc fără să treacă prin copiere) Codex nu
numește nicio cale.

## Amendamentul 1 (propus 2026-10-10; retras 2026-10-10, C111-Q5)

Premisa de mai jos e greșită (C111-R8): cititorii compozabili există deja.
Textul rămâne ca istoric.

**Constatarea.** Pasul 1, „poarta unică pe forma de azi”, nu se poate face
cum e scris. Modulul are, în afara declaranților, circa 130 de rânduri care
citesc `Partener` sau `Gestiune` de pe postare, iar o mare parte sunt
interogări EF, cu grupări, filtre și îmbinări pe coloană
(`Citiri/Partide.cs`, `Motor/Fapte.cs`, `Citiri/Loturi.cs`,
`ImperecheriProiectii.cs`, `ContabilProiectii.cs`). Un cititor numit în C#
nu se traduce în SQL, iar două proprietăți nu se pot mapa pe aceeași coloană.
Poarta ar acoperi numai citirile din memorie.

Referințele se împart în cinci clase, fiecare cu altă țintă:

| Clasa | Azi | După | Unde |
|---|---|---|---|
| A, titularul partidei | `Partener` pe postarea cu partidă | `Repartitor` | `Citiri/Partide`, `Plati`, `Transferuri`, `Fapte`, `ImperechereService`, `ImperecheriProiectii`, `Explicatii` |
| B, partenerul fiscal | `Partener` pe postarea cu fapt fiscal | `PartenerFiscal` | `Citiri/Fiscale`, `Invarianti`, `D394Proiectii`, `DiagnosticTvaService` |
| C, gestiunea lotului | `Gestiune` pe postarea cu lot | `Repartitor` | `Citiri/Loturi`, `SaftProiectii.PeCub.Stocuri`, `ImobilizariFapte` |
| D, capătul din afara evidenței | `Gestiune` = virtuală | `Contrapondere` | `Invarianti`, `Citiri/Transformare`, `ReceptiiConexe` |
| E, atomul de sold | `Partener` ca repartitor, `Gestiune` separat | `Repartitor` | `ContabilProiectii.Atomi`, `SolduriService` (S1) |

`Contabil.Jurnal` proiectează `Partener` pentru A și B deodată; consumatorii
lui din SAF-T se clasifică pe rând.

**Propunerea.** Pașii 1 și 2 se înlocuiesc cu o secvență în care coloanele noi
stau întâi lângă cele vechi:

| Pas | Ce | Verde când |
|---|---|---|
| 1 | coloanele noi, scrise lângă cele vechi: `Repartitor`, `Contrapondere`, `PartenerFiscal` pe `Postare`, completate la scriere dintr-o singură funcție de derivare; migrație obișnuită; `INV-CUB` verifică potrivirea vechi ↔ nou | integrala cu numărătorile pasului 0 plus proba nouă; niciun cititor mutat |
| 2a | cititorii claselor A, B, C și D trec pe coloanele noi, câte o clasă pe commit | integrala neschimbată după fiecare clasă |
| 2b | clasa E: atomul de sold pe `Repartitor`; `SolduriPerioada` fără `GestiuneId` | S1, singura schimbare de așteptări din pas |
| 2c | nucleul și declaranții scriu forma nouă direct (D2, D5); invarianții 1–5; funcția de derivare iese | suita nucleului; integrala; S2–S4 |
| 2d | `Partener`, `Gestiune` și `GestiuniVirtuale` ies; `PostareVizual` (S5); lanțul recomprimat în `InitialCreate`, bazele recreate | integrala pe baze recreate; proba numelor interzise |

Pașii 3–7 rămân. Regula de oprire nu se schimbă.

**De ce.** Clasificarea fiecărei referințe se verifică pe rând, sub un oracol
verde, în loc să se verifice toate deodată la sfârșitul unui pas atomic. S1
ajunge singură într-un commit. Costă o migrație temporară și un pas de
scriere dublă, care dispar la 2d.

**Ce se pierde.** Pasul 1 schimbă schema, deci nu mai e „fără schimbare de
schemă”; rămâne fără schimbare de comportament.

## Ce NU intră (amânări cu nume)

- C111-r1: furnizorul bonului la decont, ca atribut pe linie; după PoC.
- C111-r2: lumea închisă a felurilor pe planul privat (C111-Q1); după PoC.
- 111 (f): registrele tipate; decizie proprie, cu ALOP.
- TR-D9b: originea scrisă a partidei (F27-r16, F28-r1), valuta, reevaluarea,
  `PoliticaEvaluare`, hook-urile de stingere, custodia.
- D9-r1: gardul analizei pe mutări și transformări.
