# C111 — coordonatele postării: contract

**Data:** 2026-10-10. **Stare:** pin-uit cu owner-ul pe C111-Q1, Q2 și Q4
(2026-10-10); pașii 0 și 1 sunt aprobați. Pasul 2 așteaptă review-ul advers
Codex pe contract și confirmarea lui C111-Q3.
**Bază:** main `1cc9d93`. **Decizia:**
[111](../decizii/111-repartitor-unic-si-registre-tipate.md), aprobată
2026-10-10. **Măsurătorile:** [pasul 0](tr-d9b-pas0-masuratori.md).

Felia implementează 111 (a)–(e) și (i): un singur repartitor de sold pe
postare, partenerul fiscal în blocul fiscal, marcajul contraponderii în locul
gestiunilor virtuale. E o reformare: nicio sumă nu se schimbă. TR-D9b
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
stingere, custodia, furnizorul bonului la decont, locul intern al faptului
fiscal (111-r1). Lista completă e în §„Ce NU intră".

## C111-D2 — Forma postării

**Nucleu.** `Coordonate` și `Capat` pierd `Partener` și `Gestiune` și primesc:

| Câmp | Tip | Regula |
|---|---|---|
| `Repartitor` | `Repartitor?` = `(Guid Id, FelRepartitor Fel)` | coordonata de sold; nul = niciunul |
| `Contrapondere` | `Contrapondere?` = terț, consum, inventar, transformare | nul = postare în evidență |
| `PartenerFiscal` | `Guid?` | numai pe postarea cu fapt fiscal |

`GestiuniVirtuale` dispare. Forma e cea a prototipului `8c36600`, adusă peste
main. `FelRepartitor` e unul singur, în nucleu; cel din
`Declaratii/Operand.cs` iese.

**Cub.** `Postare` pierde `Partener` și `Gestiune` și primește `Repartitor`
(`uuid`, nulabil, cheie străină pe `Repartitori`, pe ambele partiții),
`Contrapondere` (`smallint`, nulabil) și `PartenerFiscal` (`uuid`, nulabil,
cheie străină pe `Repartitori`). Felul nu e coloană (111 (a)). Indecșii
parțiali de pe `Partener` trec pe `Repartitor`.

**Invarianții**, în `Conservare` și în `INV-CUB`:

1. cantitatea nemarcată cere lot și repartitor intern;
2. lotul cere repartitor intern; partida cere repartitor extern, egal cu
   titularul ei;
3. `PartenerFiscal` apare numai cu fapt fiscal;
4. contraponderea transformării n-are repartitor, unitate, valoare sau fapt
   fiscal;
5. inversa fiscală poartă același `PartenerFiscal` ca originalul.

Nicio formă refuzată azi nu devine acceptată.

## C111-D3 — Felul repartitorului

Nucleul primește felul ca valoare în operand. Sursa e una:
`Motor/Fapte.Repartitori`. Postările recitite din cub pentru nucleu (storno,
corecții) își iau felul din același cititor, prin îmbinare pe `Repartitori`.
Părțile rămân cele din `Laturi.ParteA`: extern (partener, angajat), intern
(gestiune, unitate internă), propriu (cont propriu).

## C111-D4 — Contul declară felurile admise

`Cont.FeluriRepartitor`, mulțime peste `FelRepartitor` plus `Niciunul`,
înlocuiește flag-ul `Repartitor` din `Cont.DimensiuniObligatorii`.
`DimensiuneFlags` rămâne pentru celelalte dimensiuni.

| Declarația contului | Gardul |
|---|---|
| mulțime vidă | nu judecă (C111-Q1 = lume deschisă) |
| mulțime fără `Niciunul` | cere repartitor, de unul din feluri |
| mulțime cu `Niciunul` | admite lipsa; dacă există, de unul din feluri |

Gardul rămâne `GardAnaliza`, pe capetele mișcărilor din cartea contabilă, și
scutește postările marcate cu contrapondere. `ContFapt.CereRepartitor` se
derivă din mulțime. Seed-ul bugetar declară cele 20 de conturi: 552.00.00 =
cont propriu (hotărât); 462.01.09, 437 și 803 se hotărăsc la pasul 3
(111-r2); restul după tabelul din pasul 0 §5. Seed-ul privat nu declară
nimic, ca azi.

## C111-D5 — Ce repartitor poartă fiecare picior

Regula e una: piciorul poartă repartitorul al cărui sold îl mișcă. Când apare
terțul pe picior nu se schimbă față de D9-A10; se schimbă numai unde stă.

| Piciorul | Azi | După |
|---|---|---|
| intern, fără fapt fiscal (stoc, cheltuială, venit, fișă) | `Gestiune` | `Repartitor` = același |
| intern, cu fapt fiscal (3xx, 6xx, 7xx, 4426, 4427) | `Gestiune` + `Partener` | `Repartitor` = repartitorul intern; `PartenerFiscal` = partenerul |
| de terț, cu partidă sau cu cont care cere repartitor | `Partener` | `Repartitor` = terțul capătului |
| de terț, cu partidă și fapt fiscal (419, 408, 4091) | `Partener` + `Gestiune` | `Repartitor` = titularul; `PartenerFiscal` = partenerul; locul intern nu se scrie |
| de trezorerie, propriu | `Gestiune` = contul propriu | `Repartitor` = contul propriu |
| contracantitatea recepției și a returului la furnizor | virtuala `Furnizor` | `Contrapondere` = terț |
| contracantitatea descărcării de gestiune și a returului de la client | virtuala `Client` | `Contrapondere` = terț |
| plusul și minusul de inventar | virtuala `Inventar` / `Consum` | `Contrapondere` = inventar / consum |
| contraponderea asamblării | virtuala `Transformare` | `Contrapondere` = transformare, fără repartitor |

Pe piciorul marcat, terțul rămâne `Repartitor` unde e azi `Partener`
(diferența de la NIR, DSC cu primitor extern, returul de la client).

`PartenerFiscal` are un singur scriitor, `Fiscal.CuFapt`, din latura numită
de `PoliticaTva.SursaContrapartida`. La decont rămâne angajatul.

## C111-D6 — Schimbările de comportament, nominal

Orice schimbare în afara listei oprește pasul (D9).

| Id | Schimbarea | Unde se vede |
|---|---|---|
| S1 | analitica pe repartitor a piciorului intern cu fapt fiscal arată repartitorul intern, nu partenerul fiscal | balanța și fișa pe repartitor pentru 3xx, 6xx, 7xx, 4426, 4427 |
| S2 | piciorul de terț cu partidă și fapt fiscal nu mai poartă locul intern | postările pe 419, 408, 4091 (111-r1) |
| S3 | `Furnizor` și `Client` devin „terț” | lista postărilor, scenele ModelCheck |
| S4 | refuzuri noi ale nucleului: partener fiscal fără fapt; cantitate nemarcată fără lot | numai pe un declarant greșit |
| S5 | lista postărilor arată `Repartitor`, `Partener fiscal`, `Contrapondere` în locul celor două coloane | `PostareVizual` |
| S6 | gardul refuză un repartitor de alt fel decât cele admise de cont | conturile care declară (bugetar) |
| S7 | piciorul contului de tranzit al viramentului nu mai poartă contul propriu | analitica lui 581 |
| S8 | DSC cu primitor intern marchează „consum”, nu „terț” | lista postărilor |
| S9 | fișa contului și registrul jurnal citesc contrapartida prin pereche | cont unic pe rândurile cu pereche; închide D9-r4 |

## C111-D7 — Schema, migrațiile, bazele

Fără migrare de date: lanțul se comprimă din nou în `InitialCreate`, iar
bazele de dezvoltare, clonele și bazele de import se recreează (102 (b);
C111-Q3, de confirmat înaintea pasului 2). `SolduriPerioada` pierde `GestiuneId` pe atomul contabil;
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
| cele 152 de postări pe 419, 408, 4091 | S2; 111-r1 |
| 552.00.00 și 462.01.09 pe bugetar | 552 hotărât; 462.01.09 la pasul 3 (111-r2) |
| criteriul de formă al partidelor (F27-r16, F28-r1) | TR-D9b |

## C111-D9 — Pașii, verificarea, regula de oprire

| Pas | Ce | Verde când |
|---|---|---|
| 0 | linia de bază pe main: import Flax ianuarie, amprentele (`--amprente`), raportul SAF-T S, integrala ModelCheck pe ambele profiluri | făcut 2026-10-10: [`c111-pas0-linia-de-baza.md`](c111-pas0-linia-de-baza.md) |
| 1 | poarta unică, pe forma de azi: tot ce citește `Partener` sau `Gestiune` din cub trece prin cititori numiți, iar scenele ModelCheck prin ei | integrala cu aceleași numărători ca pasul 0 (3.578 / 4.859 OK), nicio schimbare de schemă |
| 2 | forma nouă, atomic: nucleul, schema, scrierea, declaranții, cititorii din poartă, snapshot-ul, `PostareVizual`; bazele recreate | suita nucleului; catalogul verde pe ambele profiluri, cu așteptările schimbate numai pe S1–S5; `INV-CUB` |
| 3 | `Cont.FeluriRepartitor`, gardul, seed-ul bugetar | ModelCheck pe ambele profiluri; `--dump-metadata`; S6 probat cu scenariu |
| 4 | S7 și S8 | scenariu propriu pe fiecare |
| 5 | S9: contrapartida prin pereche | fișa contului pe HTTP; cifra de timp față de pasul 0, pe aceeași bază |
| 6 | uneltele: Import1C, `ScaraVolum.sql`, PerfCub | import pe ianuarie: regula de oprire 1; `INV-CUB` verde |
| 7 | contractul HTTP, clientul, `stare-curenta`, închiderea | drift zero; `refuzuri.ps1`; review advers |

**Regula de oprire.**

1. Pe import, amprentele (balanța, stocul, partidele, D300, D394), raportul
   de reconciliere și raportul SAF-T S sunt identice cu pasul 0. În catalog,
   nicio așteptare de sumă nu se schimbă. O diferență oprește pasul. SAF-T L
   nu se generează pe ianuarie 2025, deci îl țin numai scenele din catalog.
2. O schimbare de comportament care nu e în D6 oprește pasul și cere
   amendament aprobat.
3. Un declarant care nu se poate scrie pe regula din D5 fără o coordonată în
   plus oprește pasul 2: cazul se raportează, nu se ocolește.
4. O singură rulare grea o dată; importul și integrala nu rulează concurent.
5. Felia se închide cu review advers pe diff-ul întreg.

Costul scrierii cu două chei străine noi pe partiții se măsoară la pasul 6,
cu PerfCub pe baze recreate din același seed. Compararea e între baze
diferite, ca la TR-D9a, și se numește ca abatere.

## C111-Q — tranșările owner-ului (2026-10-10)

| Id | Întrebarea | Tranșarea |
|---|---|---|
| C111-Q1 | Contul care nu declară feluri: gardul nu judecă sau admite numai „niciunul”? | **lume deschisă**: gardul nu judecă; lumea închisă rămâne C111-r2 |
| C111-Q2 | Contrapartida prin pereche (S9, D9-r4) intră în C111? | **da**, ca pas separat (pasul 5) |
| C111-Q3 | Recomprimare în `InitialCreate` cu baze recreate, sau migrație cu transformarea datelor? | propus: recomprimare (102 (b)); **neconfirmat**, se cere înaintea pasului 2 |
| C111-Q4 | S7 (581 fără repartitor) și S8 (DSC intern = consum) intră? | **amândouă**, fiecare cu scenariu propriu (pasul 4) |

Pornirea: pașii 0 și 1 acum; review-ul advers Codex pe contract se închide
înaintea pasului 2.

## Amendamentul 1 (propus 2026-10-10, neaprobat)

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
- 111-r1: locul intern al faptului fiscal pe piciorul de terț cu partidă.
- 111 (f): registrele tipate; decizie proprie, cu ALOP.
- TR-D9b: originea scrisă a partidei (F27-r16, F28-r1), valuta, reevaluarea,
  `PoliticaEvaluare`, hook-urile de stingere, custodia.
- D9-r1: gardul analizei pe mutări și transformări.
