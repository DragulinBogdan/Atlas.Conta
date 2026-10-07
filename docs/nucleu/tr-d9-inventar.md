# TR-D9a — inventarul pasului 1

- Data: 2026-10-05
- Bază: `tr-d9-taierea` la `88b45a7`; contractul `docs/nucleu/tr-d9-taierea-contract.md`
  (D9-D15, rândul pasului 1).
- Ce conține: partea de produs și de unelte. Referințele ModelCheck, aserție cu
  aserție, stau în anexa `docs/nucleu/tr-d9-inventar-modelcheck.md`.
- Metoda: citire de cod și interogări pe politicile din seed
  (`Atlas.Conta.BackOffice` = bugetar, `Atlas.Conta.ModelCheck.Privat` = privat,
  amândouă la migrația `IndecsiCititoriCub`). Nu s-a rulat nicio scenă: unde
  contractul cere „se măsoară", cifra de aici e dedusă din cod și se probează
  numeric la pasul 2. Fiecare asemenea loc e marcat *din cod*.
- Pasul nu schimbă cod.

## 1. Constatări care cer precizare în contract sau decizia owner-ului

Niciuna nu e oprire înaintea termenului (toate au echivalent exprimabil), dar
contractul le formulează altfel decât arată codul.

| Id | Constatarea | Propunere |
|---|---|---|
| I1 | `IDocumentCuRegistruPropriu` nu poartă numai registrul. `MaterializeazaRegistrul` scrie și **starea fișei** (`Imobilizare.Stare`, `DataPunereInFunctiune`, `DataIesire`); `EliminaRegistrul` / `StorneazaRegistrul` poartă **refuzuri de domeniu** (fapte ulterioare pe fișă, amortizare operată ulterior, stornarea numai în luna documentului) și readuc fișa la starea dinainte; `MotivDependenti` și `MotivPerioadaStornarii` sunt citite de `GardieniRetragere` (106 k). D9-D2 spune „dispare cu cele 11 metode" | interfața se redenumește și rămâne ca hook polimorf al frunzei (operare / anulare / stornare + cele două motive), fără rânduri de registru; numele vechi intră în lista interzisă ca azi. Dispar numai scrierile `RegistruImobilizari`, `RanduriProprii` și `Inverseaza`. Precizare la D9-D2, fără schimbare de comportament |
| I2 | Recepția facturii pe cub nu are gard de analiză. Azi nota 3xx = 401 a liniei de stoc e a NIR-ului conex și e păzită de planul vechi la operarea NIR-ului. Pe cub recepția e postată de FCT (`DeclarantFacturaIntrare.Receptia`, fără regulă de contare), iar NIR-ul conex acoperit postează numai diferența, singura păzită de `ReceptiiConexe.VerificaAnaliza`. Domeniul din D9-D4 („regula de contare, postarea explicită, taxa") n-o cuprinde, deci după tăiere dimensiunile obligatorii ale recepției n-ar mai fi verificate nicăieri | recepția FCT intră în domeniul gardului portat, la operarea facturii. Refuzul se mută de pe NIR-ul conex pe factură: aceeași regulă, alt moment. Se declară în contract (D9-D4) și are scenă bugetară la pasul 2. Cere decizia owner-ului |
| I3 | T-r9 afirmă că `TipDocument.LaturaContPropriu` nu mai e citită. E citită de `Saft/SaftProiectii.PeCub.Plati.cs:27` (clasificarea plății: partener / angajat / virament), adăugat după T-r9 | cititorul se portează pe contractul structural `Document.Laturi()` la pasul 4, înaintea scoaterii coloanei la pasul 7; cifrele SAF-T `Payments` neschimbate. Precizare la D9-D6 |
| I4 | Trei refuzuri de ștergere citesc registrele și nu sunt „martori care mor": lotul cu mișcări (`LoturiLiniiSterse.Curata`), tipul de TVA referit de jurnal (`GardianEditare.VerificaTipTva`), fișa cu rânduri de registru (`Imobilizare.Verifica`). X-D2 le clasează `Martor` | se re-țintesc pe `Postare` la pasul 4 (unitatea = lotul, `TipTvaId`, unitatea = fișa), cu probă pe fiecare. Sunt refuzuri echivalente, în §3 |
| I5 | O `RegulaContare` adăugată de client pe un tip al cărui declarant nu contează prin reguli (ASM, BTR, PIF, AMO, CAS, NTC, ITV, DVI) produce azi notă numai în registru; cubul n-o are. `GardianEditare.VerificaRegulaContare` nu restrânge tipul. După tăiere regula nu mai are niciun efect | nu intră în domeniul gardului (n-are postare). Refuzul regulii la editare ar fi comportament nou, în afara listei închise din D9-D1: restanță nouă **D9-r2**, `după PoC`. Cere confirmarea owner-ului |
| I6 | Tipul inert nu are azi un refuz al lui (§6): e refuzat incidental, cu trei texte diferite, sau, cu număr cules, rămâne Operat fără efect. Schimbarea 4 din D9-D1 se confirmă | refuzul cu un singur cod intră la pasul 6, cu scenariul scris la pasul 2; SC-RLF-12, SC-RDC-16 și SC-DVI-12 se rescriu nominal |
| I8 | Proiecția `TotalStingere` din `ImperecheriProiectii.Antete` nu e moartă de tot: o consumă și `ImperecheriProiectii.VerificaAcoperire` (invariantul `CITIRE_PARTIDE_POLITICA`, chemat din `Invarianti.Verifica`, deci din `INV-CUB`), care compară totalul din antet cu partidele cubului. Fără coloană invariantul rămâne fără termen de comparație | se judecă la pasul 4, sub D9-D10: ori cade cu martorii registru → cub (totalul din antet e el însuși o copie a totalului de pe cub, scrisă la operare), ori se reformulează pe o sursă independentă. Proba SC-CIT-65, SC-CIT-43 și mutantul `POLITICA` îi urmează soarta. Precizare la faptul din contract („proiecția e moartă") |
| I7 | Deschiderea generică (`Materializare.Deschide`) nu scrie și nu verifică `Lot.PretUnitar`: prețul de intrare al lotului de deschidere e ce a pus pe lot cel care îl creează (§5) | se păstrează, cum cere D9-D3; se adaugă un rând de catalog care fixează cifra |

## 2. Politicile pe tip (seed, ambele profiluri)

`stoc` = rânduri `RegulaStoc`; `contare` = rânduri `RegulaContare`; `TVA` =
`PoliticaTva`; `explicit` = tipul e `IDocumentCuPostareExplicita`. Niciun rând
nu e adăugat de client pe cele două baze (`DinSeed` peste tot).

| Tip | Bugetar: cub / stoc / contare / TVA | Privat: cub / stoc / contare / TVA | Explicit | Numerotare lipsă |
|---|---|---|---|---|
| AMO | da / 0 / 0 / – | da / 0 / 0 / – | da | – |
| ASM | da / 2 / 0 / – | da / 2 / 0 / – | – | – |
| BCS | da / 3 / 11 / – | da / 3 / 9 / – | – | – |
| BPR | nu / 0 / 0 / – | nu / 0 / 0 / – | – | ambele |
| BTR | da / 4 / 0 / – | da / 4 / 0 / – | – | – |
| CAS | da / 0 / 0 / – | da / 0 / 0 / – | da | – |
| DEC | da / 0 / 1 / – | da / 0 / 1 / da | – | – |
| DSC | nu / 0 / 0 / – | da / 2 / 9 / – | – | bugetar |
| DVI | nu / 0 / 0 / – | da / 0 / 0 / da | – | ambele (MRN cules) |
| FCL | da / 0 / 1 / – | da / 0 / 10 / da | – | – |
| FCT | da / 0 / 3 / – | da / 0 / 3 / da | – | ambele (numărul furnizorului) |
| INC | da / 0 / 2 / – | da / 0 / 2 / – | – | – |
| ITV | nu / 0 / 0 / – | da / 0 / 0 / – | da (moștenit din NTC) | bugetar |
| LDI | da / 3 / 12 / – | da / 2 / 10 / – | – | – |
| NIR | da / 5 / 1 / – | da / 2 / 1 / – | – | – |
| NTC | da / 0 / 0 / – | da / 0 / 0 / – | da | – |
| PIF | da / 0 / 0 / – | da / 0 / 0 / – | – | – |
| PLT | da / 0 / 2 / – | da / 0 / 2 / – | – | – |
| RDC | nu / 0 / 0 / – | da / 2 / 10 / da | – | bugetar |
| RLF | nu / 0 / 0 / – | da / 2 / 1 / da | – | bugetar |

`PoliticaValidare`: bugetar pe DEC, FCT, PLT (clasificație bugetară) și FCL
(natură interzisă); privat niciuna. `PoliticaConex`: FCT → NIR pe ambele.

## 3. Refuzurile planului vechi, cu verdict (D9-D4)

Planul vechi = `MotorOperare.CalculeazaSiValideaza` de la
`StocService.AplicaValoareIesire` în jos, cele trei scrieri de registru și
ramurile lor din `AnuleazaOperarea` / `Storneaza`, plus cititorii de registru
care refuză în afara motorului. Pe toate tipurile cu `PosteazaInCub` contractul
rulează deja lângă planul vechi, deci un echivalent mai strict există de azi;
verdictul spune dacă scoaterea refuzului vechi **lasă ceva nepăzit**.

| # | Refuzul de azi (locul) | Tipurile pe care poate apărea | Verdict | Echivalentul pe contract |
|---|---|---|---|---|
| 1 | „Linia … intră în regulile de stoc dar nu are lot" (`PotrivesteReguliStoc`, strict) | ASM, BCS, BTR, LDI, NIR; DSC, RDC, RLF pe privat | echivalent | `LOT_LIPSA` în fiecare declarant al acestor tipuri (`DeclarantAsamblare:27`, `BonConsum:31`, `NotaTransfer:32`, `DiferenteInventar:38`, `Nir:29`, `DescarcareGestiune:32`, `ReturClient:27`, `ReturFurnizor:24`); pe FCT linia de stoc fără lot: `DeclarantFacturaIntrare:89` |
| 2 | „Sold negativ … pe lotul …" pe cheia `(Lot, Repartitor, TipStoc)` (`StocService.VerificaSoldIntermediar`), la operare, anulare, storno | niciunul azi: ramura rulează numai cu `!PosteazaInCub`, iar tipurile inerte n-au reguli de stoc | al registrului, moare | soldul cubului e păzit de `Citiri.Loturi.VerificaSoldIntermediar` (`STOC_INSUFICIENT` / `NIR_STOC_INSUFICIENT`) în `Materializare.Opereaza`, `Anuleaza`, `Storneaza` |
| 3 | „Contul debitor / creditor nu se poate rezolva …" (pasul de note) | tipurile cu `RegulaContare` (§2) | echivalent | `REGULA_CONTARE_LIPSA` din `Contari.Rezolva`, chemat de declaranții BCS, DSC, FCL, FCT, LDI, NIR (și diferența), RDC, RLF, PLT/INC; pe DEC `DeclarantDecont:35,43` (`REGULA_CONTARE_LIPSA`, `CONT_EXPLICIT_LIPSA`) |
| 4 | linia fără regulă de contare, sărită tăcut | toate | deja refuz (B-r10) | `REGULA_CONTARE_LIPSA` |
| 5 | „Tipul de TVA al unei linii nu mai există în nomenclator" (pasul TVA și `RegistruTvaService.Deriva`) | tipurile cu `PoliticaTva`: DEC, DVI, FCL, FCT, RDC, RLF (privat) | echivalent | `TIP_TVA_LIPSA` în `DeclarantDecont:25`, `Dvi:28`, `FacturaIesire:83`, `FacturaIntrare:100`, `ReturClient:37`, `ReturFurnizor:32` |
| 6 | „Tipul de TVA … nu are contul de TVA … configurat" | aceleași | echivalent | `CONT_TVA_LIPSA` în `Fiscal.Impozitul:115` și `DeclarantDvi:34` |
| 7 | „Contrapartida rândului de TVA nu se poate rezolva" | aceleași | echivalent | `Fiscal.Impozitul:130` (`REGULA_CONTARE_LIPSA`; `CONT_TVA_LIPSA` la taxare inversă) |
| 8 | dimensiunile obligatorii per cont (`VerificaDimensiuniObligatorii`) | tipurile cu note (§4) | **se portează** pe postări, pasul 2 | gardul din D9-D4, cu domeniul din §4 și cu I2 |
| 9 | `NIR_REGIM_INACTIV` (`NIR.ValideazaOperare`, `DocumenteGestiune.cs:67`) | NIR | moare cu regimul | – |
| 10 | `POSTEAZA_IN_CUB_IREVERSIBIL` (`GardianEditare:1043`) | `TipDocument` | moare cu coloana (D9-D5) | – |
| 11 | „ambele documente trebuie să posteze în cub" (`Materializare.Imperecheaza:128`) | împerecherea | moare cu regimul; refuzul pe efect nul rămâne (108 i) | – |
| 12 | „Tipul … e marcat PosteazaInCub, dar clasa … nu declară" (`Materializare.Contracteaza`) | BPR, dacă ar fi marcat | se înlocuiește cu refuzul tipului care nu declară (D9-D5) | codul unic de la pasul 6 |
| 13 | PIF / CAS / AMO: fapte ulterioare nestornate pe fișă; amortizare operată pentru luna ieșirii sau ulterioară; amortizare ulterioară; stornarea în altă lună decât a documentului (`EliminaRegistrul`, `StorneazaRegistrul`) | PIF, CAS, AMO | **nu sunt ale registrului, rămân** | aceleași funcții (`MotivFapteUlterioare` citește deja `Cub.Citiri.Imobilizari`), pe hook-ul redenumit (I1) |
| 14 | lotul cu mișcări nu se șterge odată cu linia (`LoturiLiniiSterse.Curata`, `LoturiCulegereService:288`) | culegere | echivalent, de re-țintit (pas 4) | `Postare` cu unitatea = lotul |
| 15 | tipul de TVA referit de jurnal nu se șterge (`GardianEditare.VerificaTipTva:1066`) | nomenclator | echivalent, de re-țintit (pas 4) | `Postare.TipTvaId` |
| 16 | fișa cu rânduri de registru nu se șterge (`Imobilizare.Verifica`, `Nomenclatoare/Imobilizari.cs:80`) | nomenclator | echivalent, de re-țintit (pas 4) | `Postare` cu unitatea = fișa |
| 17 | lotul de deschidere cu mișcări în registru (`Materializare.Deschide`, `Materializare.Deschidere.cs:75`) | deschidere | jumătatea de registru moare | jumătatea de pe `Postare`, din aceeași condiție, rămâne |
| 18 | scrierea registrelor pe uși securizate (`GardianEditare.Verifica`, cazurile celor patru registre) | toate ușile | moare cu tipurile, la pasul 7 | cazurile noi pentru `Postare` și `Tranzactie` (D9-D9) |
| 19 | `Document.TotalStingere` scrisă numai de motor (`GardianEditare:400,430`; exclusă din copiere în `CorectieService:32`) | toate | moare cu coloana | – |

Nu sunt ale planului vechi și rămân neatinse: starea și datele documentului,
perioada, blocajele (108 e), TVA cules la taxare inversă, `PoliticaValidare`,
`ValideazaOperare`, `GardContare`, cantitatea pozitivă a liniei care naște lot,
numerotarea, scadența, gardienii retragerii.

Ramura `RegistruTva` din `CorectieService.Corecteaza` (liniile 97–101) nu
refuză nimic: reatribuie perioada inversei fiscale. Geamănul ei pe cub,
`Materializare.ReatribuieInversaFiscala`, e chemat pe linia următoare și
rămâne.

## 4. Domeniul gardului analizei obligatorii, tip cu tip (D9-D4)

Coloana „Note azi" spune ce verifică `VerificaDimensiuniObligatorii` astăzi,
adică ce trebuie să verifice gardul portat ca să fie echivalent.

| Tip | Note azi (planul vechi) | În domeniul gardului portat | Observații |
|---|---|---|---|
| BCS | din reguli, ambele profiluri | da: postările contării liniei | |
| DSC | din reguli, privat | da (privat) | bugetar: inert |
| LDI | din reguli, ambele | da | |
| NIR manual | din regulă, ambele | da | |
| NIR conex | din regulă, ambele | nu prin gardul nou: diferența e păzită de `ReceptiiConexe.VerificaAnaliza`; recepția e a FCT | vezi I2 |
| FCT | din reguli pe servicii și cheltuieli; linia de stoc n-are notă pe FCT; taxa pe privat | da: contarea liniei și taxa; **recepția liniei de stoc numai dacă owner-ul aprobă I2** | |
| FCL | din reguli; taxa pe privat | da | |
| DEC | din regulă, cu contul explicit al liniei; taxa pe privat | da | |
| PLT, INC | din reguli | da | |
| RDC, RLF | din reguli și taxă, privat | da (privat) | bugetar: inerte |
| DVI | numai taxa, privat | da: taxa în `Carte = Contabil`; baza din cartea fiscală nu intră | bugetar: inert |
| NTC | postarea explicită a liniei | da | `DeclarantNotaContabila` pune analiza liniei |
| ITV | postarea explicită (moștenită din NTC), privat | da (privat) | bugetar: refuzat de validarea frunzei |
| AMO, CAS | postarea explicită a liniei | da | **nenumite în contract**: sunt `IDocumentCuPostareExplicita`, deci au note și sunt păzite azi |
| PIF | fără note (fără reguli, fără postare explicită) | nu | azi nepăzit; intră sub D9-r1 alături de BTR și ASM |
| BTR, ASM | fără note | nu | D9-r1, cum spune contractul |
| BPR | fără note | nu | nu declară |

Două abateri față de textul D9-D4, de consemnat în contract:

- AMO și CAS intră în domeniu (contractul le lasă nenumite; „postarea
  explicită" le acoperă, dar tabelul trebuie să le poarte nominal);
- PIF se adaugă la D9-r1: pe cub postează, în registru n-a avut niciodată
  notă.

**Politicile editabile.** O regulă de contare adăugată de client intră singură
în domeniu pe tipurile ai căror declaranți contează prin `Contari.Rezolva`
(BCS, DSC, FCL, FCT, LDI, NIR, RDC, RLF, PLT, INC, DEC): postarea ei există și
e păzită. Pe tipurile fără contare prin reguli — I5.

**Angajamentul.** `LinieOperand.AngajamentId` există deja pe operand, deci
excepția „angajamentul ține loc de cod economic" are de unde să fie citită, pe
linia cauzală a postării. `ReceptiiConexe.VerificaAnaliza` o face deja așa
(`ReceptiiConexe.cs:219`); `Materializare.Deschide` trimite `null`.

**Maparea dimensiunilor** e scrisă de două ori azi, la fel: `Repartitor` =
partenerul postării sau gestiunea ei, `Material` = produsul, restul = analiza
(`ReceptiiConexe.cs:225–230`, `Materializare.Deschidere.cs:127–130`). Gardul
portat o are a treia oară dacă nu se extrage; extragerea e a pasului 2.

## 5. Valoarea liniei (D9-D2, D9-D3)

**Validările de frunză care judecă valoarea finală.** Una singură:
invariantul valoric ASM, `Asamblare.ValideazaOperare` (`Asamblare.cs:171–175`),
care compară Σ produse cu Σ consumuri **după** ce `AplicaValoareIesire` a scris
valoarea de golire pe liniile de consum. După tăiere ar vedea estimarea. Îl
înlocuiește gardul P = C al declarantului (D9-D3); invariantul din frunză se
șterge la pasul 6.

Celelalte comparații de valoare din `ValideazaOperare` (`Decont.cs:62`,
`Dvi.cs:71`, `NotaContabila.cs:108`, `Retururi.cs:208`, `Trezorerie.cs:290`)
judecă valori culese sau calculate de `PregatesteOperare`, nu valori de
golire, și nu se ating. `InchidereTva.ValideazaOperare` compară liniile cu
`InchidereTvaService.Solduri`, care citește cubul.

**Apelanții lui `MotorOperare.Valideaza`.** Doi, în toată soluția:

| Apelant | Ce citește din rezultat | Verdict |
|---|---|---|
| `ComenziDocument.Valideaza` (`Api/ComenziDocument.cs:94`), de pe toate ușile HTTP de validare | lista de refuzuri | rămâne |
| `AsamblareApply.PrezicSumaConsum` (`Api/Asm/AsamblareApply.cs:364`) | valorile lăsate de dry-run pe liniile de consum din ObjectSpace-ul temporar | se mută pe evaluarea consumurilor din declarant (D9-D3), pasul 6 |

XAF și uneltele nu cheamă dry-run-ul.

**Sursa (a), evaluată din sold.** Estimarea de draft e `BazaLinie` →
`Lot.ValoareLaPretulLotului` pe BCS, BTR (`DocumenteGestiune.cs:233, 267`),
consumul ASM (`Asamblare.cs:89`), minusul LDI
(`ListaDiferenteInventar.cs:45`); DSC o calculează direct în
`PregatesteOperare` (`DescarcareGestiune.cs:29–35`), tot din `Lot.PretUnitar`.
Valoarea finală o scrie azi `StocService.AplicaValoareIesire`; decizia
`ValoareIesire` există deja în declaranții BCS, BTR, DSC, LDI și ASM
(`N.Evaluare.Iesire`), deci coaja are de unde s-o ia.

**Sursa (b), declarată la prețul de intrare.** RLF (`Retururi.cs:64`), costul
RDC (`Retururi.cs:154`), linia NIR-ului conex pe lot străin
(`DocumenteGestiune.cs:60–62`). Toate trec prin `Lot.PretUnitar` și ajung
`ValoareDeclarata` în declarant. `AplicaValoareIesire` sare deja RLF
(`IDocumentCuIesireFiscala`).

**Scriitorii `Lot.PretUnitar` în produs.** Unul: `MotorOperare.cs:349`, la
operarea liniei care naște lotul. Rămâne, la punctul 7 al cojii.

**Returul pe un lot de deschidere fără preț de intrare** (*din cod*).
`Materializare.Deschide` primește loturi deja existente; nu le scrie și nu le
verifică `PretUnitar`. Prețul e cel pus de cine a creat lotul: Import1C scrie
Σvaloare / Σcantitate rotunjit la 4 zecimale (`Deschidere.cs:435`). Pe un lot
cu `PretUnitar = 0`, linia de retur iese cu valoarea 0: RLF postează
cantitatea cu valoare zero și decizia `ValoareDeclarata(q, 0)`, fără refuz;
soldul lotului rămâne cu toată valoarea pe o cantitate mai mică. Comportamentul
se păstrează. Catalogul n-are rând pentru el; se adaugă la pasul 2, cu cifra
măsurată.

## 6. Tipul care nu declară sau n-are politică pe profil (D9-D5)

*Din cod.* Astăzi nu există un refuz al tipului inert. Ce se întâmplă:

| Tip, profil | Dry-run (`Valideaza`) | Operare |
|---|---|---|
| ITV, bugetar | refuz din frunză: „Închiderea de TVA cere politica de conturi" | același refuz |
| RDC, RLF, DSC, bugetar | trece (dacă validările de frunză pe linii trec): fără reguli nu există mișcări, note sau taxă, iar `Materializare.Refuzuri` întoarce listă goală pe `!PosteazaInCub` | fără număr cules: refuz „nu are politică de numerotare". Cu număr cules: **trece și rămâne Operat fără niciun efect** |
| DVI, bugetar | numărul (MRN) e cules obligatoriu; frunza cere pe fiecare linie un tip de TVA de import cu cotă. Dacă profilul are un asemenea tip, trece; altfel refuză din frunză | când trece: Operat fără efect. Dacă bugetarul are tip de TVA de import nu s-a verificat aici; se fixează la pasul 2 |
| BPR, ambele profiluri | trece | ca RDC: refuz pe numerotare sau Operat fără efect |

Deci dry-run-ul și operarea nu spun același lucru (numărul se cere abia la
materializare), iar cu număr cules documentul devine Operat fără postări.
Refuzul cu un singur cod, pe toate ușile, e schimbare de comportament:
schimbarea 4 din D9-D1 se confirmă. Scenariul de la pasul 2 fixează numeric
rândurile de mai sus înaintea codului.

Ce e măsurat azi: catalogul probează pe bugetar refuzul **fără număr cules**
la RLF și RDC (SC-RLF-12, SC-RDC-16: textul „politică de numerotare") și
refuzul frunzei la DVI pe linia fără tip de TVA (SC-DVI-12: „TVA de import"),
toate cu `FaraEfecte`. Trei texte diferite, niciunul al tipului inert. Cazul
cu număr cules, care lasă documentul Operat fără postări, nu e măsurat de
nicio scenă; DSC și BPR nu au scenă deloc. Scenele din `Program.cs:7726` și
`7794` probează numai că seed-ul e inert.

Refuzul unic schimbă deci și textul potrivit de cele trei rânduri de catalog;
ele se rescriu nominal la pasul 2.

## 7. `RegulaStoc` după tăiere (D9-D8)

Consumatorii din afara planului vechi:

| Consumator | Tipul ale cărui rânduri le citește | După tăiere |
|---|---|---|
| `DeclarantDiferenteInventar:52` (prin `Operand.ReguliStoc`) | LDI | rămâne: cere exact o regulă Magazie / Marfuri / Folosință |
| `DescarcareService:90,118` | DSC | rămâne: ce linii de factură intră în descărcare |
| `FacturaIesire.ValideazaOperare:134,149` | DSC | rămâne: lotul fixat pe linia facturii |
| `ExplicaApply:68` („Explică") | oricare | rămâne numai pentru tipurile cu rânduri păstrate |
| `GardianEditare.VerificaRegulaStoc`, `VerificareProfilService:106`, pagina client `politici/reguli-stoc`, `TipuriConfigurabile`, OData | tabela | rămân cât rămâne tabela |
| `Fapte.SolduriLoturiRegistru` (absorbția ASM-B6) | ASM | moare (D9-D2) |
| `MotorOperare.PotrivesteReguliStoc`, `StocService.AplicaValoareIesire` | toate | mor |

Rândurile fără consumator după tăiere, care ies din seed pe ambele profiluri:
ASM (2), BCS (3), BTR (4), NIR (5 bugetar, 2 privat), RDC (2, privat), RLF
(2, privat). Rămân rândurile LDI (3 bugetar, 2 privat) și DSC (2, privat).
Tabela nu dispare: LDI și DSC nu-și pot citi structura numai din conturi fără
altă sursă de politică.

Consecință de numit la pasul 7: „Explică" pe BCS, BTR, NIR, ASM, RDC și RLF nu
mai are ce reguli de stoc să arate; răspunsul lui pe aceste tipuri se schimbă
(nu mai potrivește nimic). E urmarea directă a D9-D8, nu o schimbare în plus.

În ModelCheck, 11 aserții depind de rândurile care ies (anexa, §11): 7 numără
sau citesc rânduri de seed pe ASM, RLF și RDC, 3 scriu o regulă de mână pentru
o probă care rămâne validă, una leagă acoperirea `PoliticaMiscareSaft` de
perechile produse de regulile de stoc (`Program.cs:13361`, D17-V1). Niciun
rând de seed de pe BCS, BTR sau NIR nu e numărat de vreo aserție; singurul lor
cititor e montajul de la `Program.cs:16691`, care cade fără rânduri. Schimbarea
de răspuns a lui „Explică" pe tipurile golite nu e probată azi de nimic: proba
ei se scrie la pasul 7, odată cu scoaterea rândurilor.

D17-V1 cere o hotărâre la pasul 7: `PoliticaMiscareSaft` e cheiată pe
`(TipDocument, TipStoc, Semn)`, „exact cheia lui `RegulaStoc`"; fără rândurile
de stoc ale BCS, BTR, NIR, acoperirea ei nu mai are față de ce să fie
verificată. Re-cheierea pe cont rămâne TR-r7; până atunci aserția se
reformulează pe mișcările pe care cubul le produce, nu pe reguli.

`TipStoc` rămâne pe `Cont.CategorieStoc`, pe `PoliticaMiscareSaft` și pe
rândurile `RegulaStoc` păstrate.

## 8. Membrii serviciilor vechi (D9-D2)

| Serviciu | Dispare | Rămâne |
|---|---|---|
| `StocService` | tot ce citește `RegistruStoc`: `MiscariRegistru`, `SolduriLaData`, `AplicaValoareIesire`, `VerificaSoldIntermediar`, `Sold`, `AlocaFifoTolerant`, `AlocaFifo`; tipurile `CheieStoc`, `MiscareStoc`, `SoldStoc` | nimic în produs. `ValoareGolire` și `VerificaGoliri` (cu `RandGolire`, `VerdictGolire`, `FelGolire`) sunt funcții pure, fără apelant în produs; singurii lor apelanți sunt Import1C (`Alocare`, `ReconciliereLuna`) și ModelCheck. Soarta lor e a pasului 5: se șterg odată cu apelanții sau se mută în unealtă. Clasa dispare din `Module/` |
| `RegistruTvaService` | tot: `RandTva`, cele două `Deriva`, `Cifre` (apelanți: planul vechi și BackfillTva); `PerioadaDeclarare` are un singur apelant, scrierea `RegistruTva` din `Opereaza` (`MotorOperare.cs:385`); `PerioadaOriginalului` nu are niciunul | nimic: serviciul dispare la pasul 6. D9-D7 (b) îl dă ca exemplu de „serviciu care rămâne"; exemplul cade, regula nu |
| `AmortizareService` | nimic: nu referă registrele | tot. `Situatie`, chemată din `IesireImobilizare.MaterializeazaRegistrul`, mai are apelanți (`ImobilizariApply`, `ImobilizariFapte`) |
| `InchidereTvaService` | nimic | tot |
| `DescarcareService` | nimic | tot |
| `LoturiCulegereService` | citirea `RegistruStoc` din `LoturiLiniiSterse.Curata` | restul; citirea se re-țintește (I4) |
| `CorectieService` | ramura `RegistruTva` (liniile 97–101); `TotalStingere` din lista de excluderi | restul |
| `Fapte` | `SolduriLoturiRegistru` și apelul lui | restul |
| `Documente/Imobilizari.cs` | scrierile `RegistruImobilizari`, `RanduriProprii`, `Inverseaza` | starea fișei, refuzurile și cele două motive (I1) |

## 9. Lista nominală X-D2 și simbolurile netipizate

Dispoziția celor 75 de intrări, pe clase:

| Clasa | Intrări | Pasul |
|---|---|---|
| scriitor dual | 26 | 6 (nu mai scriu, nu mai citesc); declarațiile lor la 7 |
| martor | 7 | 3 acoperiri registru → cub mor la 6 (`Invarianti.Verifica`, `Imobilizari.VerificaAcoperire`, `Loturi.VerificaAcoperire`); `Materializare.Deschide` pierde jumătatea de registru la 6; 3 se re-țintesc la 4 (I4) |
| evidență XAF | 2 | 7 (lista pe `Postare` apare la 4) |
| autorizare | 4 | 4: cele trei porți `RegistrulCitibil` trec pe `Postare`; `TipuriInsumate` primește `Postare` |
| mapare | 6 | 7 (rămâne `Imperechere`) |
| legătură | 30 | rămân |

Simbolurile pe care lista nominală nu le urmărește (nu sunt tipuri de
registru), în produs:

| Simbol | Locuri | Pasul |
|---|---|---|
| `PosteazaInCub` | `Politici.cs:44`; `Materializare.cs:28, 52, 128–129, 312`; `MotorOperare.cs:127, 610, 660`; `DocumenteGestiune.cs:68`; `GardianEditare.cs:1043`; `ContaSeeder.cs:268, 312, 314` | ramurile la 6, coloana și seed-ul la 7 |
| `Materializare.AreTranzactii` | `Materializare.cs:19`; `GardianEditare.cs:1044`; `ContaSeeder.cs:314` | 6 |
| `TotalStingere` | `Document.cs:163`; `MotorOperare.cs:429, 628`; `GardianEditare.cs:400, 430`; `CorectieService.cs:19, 32`; `Scara.cs:103, 107`; `ImperecheriProiectii.cs:63–97` (6 proiecții), citite de `DocumenteCuRest` (fără efect) și de `VerificaAcoperire` (I8) | proiecția la 4, după verdictul pe I8; scrierile la 6; coloana la 7 |
| `NumarNota` | `Registre.cs:44` (coloana); `MotorOperare.cs:678`; `ContabilProiectii.cs` (9 locuri: câmpul de raport, alimentat deja din numărul documentului); client `FisaCont.tsx:153`, `RegistruJurnal.tsx:45` | coloana la 7. Câmpul de raport `NumarNota` rămâne: e nume de raport (D9-D12), nu de registru; proba numelor interzise n-are voie să-l atingă |
| `LaturaContPropriu` | `Politici.cs:49`; `ContaSeeder.cs:268, 315`; **`SaftProiectii.PeCub.Plati.cs:27, 29`** | cititorul la 4 (I3), coloana la 7 |
| `IDocumentCuRegistruPropriu` | `Interfete.cs:138`; `Imobilizari.cs:14, 378, 570`; `MotorOperare.cs:402, 623, 715`; `GardieniRetragere.cs:14` | redenumire și golire de registru la 6 (I1) |
| `HCategory` | `BackOfficeDbContext.cs:44`, `Module.cs:39` | 7, dacă iese numai din model (106-r5) |

`NumarNota` cere o precizare în D9-D7 (b): numele nu poate intra în lista
interzisă ca identificator întreg, fiindcă e și câmp de DTO păstrat. Contractul
nu-l pune în listă; rămâne așa.

Hosturile: WebApi referă registrele numai în cele trei porți de autorizare
(`AmoController:139`, `ImobilizariController:51`, `ItvController:106`) și în
comentarii (`D300Controller:9`, `D394Controller:9`, `ItvController:48`).
Blazor.Server nu le referă. Registrele nu sunt expuse prin OData.

## 10. Uneltele (D9-D11)

| Unealtă, fișier | Referințe | Verdict |
|---|---|---|
| Import1C `ReconciliereLuna.cs` | `RegistruContabil` 5, `RegistruStoc` 4, `StocService` 10 | se portează pe cititorii cubului (109-r1); partea care folosește `VerificaGoliri` hotărăște soarta oracolului golirii (§8) |
| Import1C `Deschidere.cs` | `RegistruStoc` 5, `RegistruContabil` 3, `NumarNota` 1 | scrierea deschiderii în registre moare; rămâne calea `DeschidereCub` (`Materializare.Deschide`) |
| Import1C `Sabotaj.cs` | `RegistruContabil` 5, `RegistruStoc` 3 | probele de sabotaj pe rânduri de registru rămân fără obiect; se șterg, cu rând la pasul 5 |
| Import1C `Alocare.cs` | `StocService` 5, `RegistruStoc` 1 | predicția valorii de golire din registru; se portează numai cât cere compilarea sau se șterge odată cu `ValoareGolire` |
| Import1C `Reluare.cs` | `RegistruStoc` 2, `RegistruTva` 2, `RegistruContabil` 1 | purjele de reluare trec pe `Tranzactie` / `Postare` |
| Import1C `Saft1C.cs` | `RegistruTva` 2, `RegistruContabil` 1, `RegistruStoc` 1 | de citit la pasul 5 |
| Import1C `Nomenclatoare.cs` | `RegistruTva` 2 | de citit la pasul 5 |
| Import1C `Program.cs`, `Reconciliere.cs`, `Diagnostic.cs` | câte 1–2 | de citit la pasul 5 |
| Migrare `Program.cs` | `RegistruContabil` 4, `RegistruStoc` 3, `NumarNota` 2 | unealta scrie direct în registre; verdictul „se portează sau se șterge" se dă la pasul 5, după citirea ei integrală |
| BackfillTva (`Backfill.cs`, `Reconciliere.cs`, `Program.cs`) | 13 | se șterge (D9-D11) |
| ProbeHttp | 0 | neatinsă |

Uneltele n-au fost citite integral la acest pas; tabelul numără referințele și
dă verdictul pe fișier, nu pe linie. Inventarul pe linie e primul lucru al
pasului 5.

## 11. Clientul (D9-D12)

În afara codului generat, clientul nu citește `posteazaInCub` sau
`totalStingere` și nu referă entitățile de registru. Referințele rămase sunt
nume de raport care se păstrează: pagina `RegistruImobilizari`, coloana
`NumarNota` din `FisaCont` și `RegistruJurnal`, câmpurile de rezumat SAF-T
`ValoareRegistruContabil`, `RegistruStocValoare`, `RegistruStocCantitate`,
`RegistruStocBate` (`Saft.tsx:425, 539–548`) și un comentariu în `D300.tsx:17`.

Câmpurile de rezumat SAF-T poartă nume de registru, dar sunt alimentate din
cub de la SAF-B8; redenumirea lor ar fi drift de contract nedeclarat, deci
rămân. Lista interzisă din D9-D7 (b) nu le atinge (identificator întreg).

`generated/api-types.ts`: 5 linii cu numele vizate; driftul exact se declară
la pasul 7, după regenerare.

Pentru pasul 4 nu rămâne nimic de făcut în client pe câmpurile care ies.

## 12. Restanțele judecate la pasul 1 (D9-D13)

| Id | Constatarea | Verdict |
|---|---|---|
| 63f | cele trei resturi nu privesc registrele. Filtrarea laturilor interne pe `Calitati` e în contractul laturilor (`Latura.Interna.Cu(…)`, T-D13); valoarea BTR la culegere e formula comună `CalculeazaValori` / `BazaLinie` (104 c); afișarea leneșă a lookup-urilor ține de paginile React înghețate (104 d) | se închide fără obiect |
| 64h, 86-r13, F27-r11 | niciun cititor de sold nu mai grupează pe laturile documentului: atomii contabili iau `RepartitorId` din `Postare.Partener` (`ContabilProiectii.cs:190, 460`), iar `SoldParteneri` grupează pe el. Laturile documentului mai apar numai ca etichetă de antet (`ImperecheriProiectii.Antete`), în diagnosticul `Partide.VerificaAcoperire` și în clasificarea plăților SAF-T | se pot închide, cu proba pe `Partener` la închidere |
| T-r9 | coloana are un cititor (I3) | în felie, după portarea cititorului |

## 13. ModelCheck

Inventarul aserțiilor, al ajutoarelor și al scenelor care scriu registre de
mână e în `docs/nucleu/tr-d9-inventar-modelcheck.md`, cu fișierele de lucru în
`run-nucleu/tr-d9a/pas1/` (necomise; `asertii.tsv` e lista pe care o execută
pasul 3).

**Cum a fost făcut și verificat.** Inventarul l-a făcut un agent, pe spec
scris (`run-nucleu/tr-d9a/pas1/spec.md`). Verificarea independentă: mulțimea
referințelor directe re-derivată și comparată (807 linii, egale), numărătorile
de aserții recalculate din sursă, trei unități recitite integral verdict cu
verdict (scena bugetară BCS, felia Api BCS, `VerificaSaftStocuriFixuri`), plus
două căutări țintite după ce i-ar fi putut scăpa: textele refuzurilor planului
vechi și cuvintele-cheie de registru în unitățile închise fără nicio atingere.
Cele trei unități au ieșit corecte. Căutările au găsit două goluri, închise
printr-o a doua trecere a agentului, re-verificată la fel: aserțiile care
ajung la registre prin ajutor și colecție (exemplul: SOL-C0,
`Program.cs:23483`) și aserțiile pe rândurile `RegulaStoc` din seed (D9-D8),
pe care spec-ul inițial nu le ceruse.

**Cifrele de control** (re-derivate de main, `verifica.py` 18/18): 151 de
unități, din care 108 în `Program.cs` (liniile 1…33241, fără găuri); 3.554 de
linii de aserție, din care 2.025 în `Program.cs` (1.755 `Check(`, 270
`CheckRefuza(`); 807 referințe directe în afara oracolului și a listei
nominale, 658 în `Program.cs`; 138 de ajutoare locale.

| Verdict pe aserție | Toate fișierele | `Program.cs` |
|---|---|---|
| `forma` (moare) | 43 | 34 |
| `regula` (se re-țintește) | 319 | 309 |
| `regula-catalog` | 0 | 0 |
| `oracol` (moare cu D9-D7 a) | 52 | 13 |
| `comportament` (schimbările declarate și D9-D8) | 91 | 53 |
| `neclar` (verdictul mai jos) | 34 | 33 |
| neatinse | 3.015 | 1.583 |

`regula-catalog` e zero fiindcă agentul n-a citit catalogul: nicio aserție de
regulă nu e propusă spre ștergere pe motiv de acoperire. A doua trecere, pe
flux de date, a regăsit mecanic 403 din cele 431 de aserții ale primei livrări
(restul nu sunt flux de date) și a adus una nouă, pe cea din exemplu.

**Ce garantează și ce nu.** Referințele directe sunt complete, mecanic.
Verdictele sunt citire de cod, verificate pe eșantion, nu integral. Lista
aserțiilor care depind de registre prin flux de date e un minim: o aserție
scăpată nu se poate pierde tăcut, fiindcă ajutorul sau tipul de care depinde
dispare la pasul 6 sau 7 și compilarea o arată; atunci primește verdict și rând
în inventar, înaintea ștergerii. Riscul care rămâne e verdictul greșit pe o
aserție inventariată; de aceea pasul 3 rulează aserția veche și pe cea
re-țintită pe aceeași scenă, sub regimul dual (D9-D7 d).

**Verdictele main-ului pe clasele `neclar`** (anexa, §8):

| Clasa | Aserții | Verdict |
|---|---|---|
| repartitorul de pe latura notei | 17 | clauza se rescrie pe convenția cubului, declarant cu declarant: `Gestiune` pe piciorul intern, `Partener` pe piciorul de terț. Cifra diferă prin construcție; diferența e declarată (B-D8 pct. 4, 9, 10), deci nu e oprire. Unde cubul nu poartă coordonata (446 fără partener, `Program.cs:22522`), clauza cade ca formă. Aceste rescrieri sunt și proba pe `Partener` cerută de D9-D13 pentru 64h, 86-r13, F27-r11 |
| `StocService.VerificaGoliri` | 7 | mor cu serviciul: funcția n-are apelant în produs (§8). Regula golirii rămâne probată pe cub de catalog, de ținta 0/0 și de invariantul nou din D9-D3. Dacă pasul 5 păstrează funcția în Import1C, probele n-o urmează în ModelCheck |
| alocarea FIFO a `StocService` | 3 | `Program.cs:4607` și `4608` mor cu serviciul; `24002` (DIR-V9, ordinea loturilor după data înregistrării) e regulă și se re-țintește pe ordinea din `Citiri.Loturi.Disponibile` |
| cusătura JT-D6 | 2 | cade ca egalitate între două registre. Pe cub faptul fiscal și postarea de taxă sunt aceeași postare; ce păzea cusătura păzește invariantul „taxa postată = taxa liniei" (109), deja în `INV-CUB`. Diagnosticul de la `Program.cs:10375` cade cu ea |
| `CITIRE_PARTIDE_POLITICA` | 2 și mutantul | după verdictul pe I8, la pasul 4 |
| `Program.cs:21057` (rolul Configurator) | 1 | regulă: lista tipurilor pe care Configuratorul nu le scrie primește `Postare` și `Tranzactie` în locul registrelor, la pasul 4, odată cu D9-D9 |
| `Program.cs:21921` (ServerView pe lista de registru) | 1 | se re-țintește pe lista XAF pe `Postare`, la pasul 4 |
| `Program.cs:27759` (repartitorul rândului de fișă) | 1 | clauza se rescrie pe `Gestiune` a postării fișei; `RandImobilizare` nu primește câmp nou |

**Interpretarea agentului, confirmată.** 75 de aserții de felul „zero rânduri
după refuz, după anulare sau pe documentul fără efect" au verdictul `regula`,
cu ținta „zero postări pe document": atomicitatea refuzului e regulă de domeniu
(33 d), nu formă a registrului.

**Cifre care se schimbă, nu numai sursa lor** (toate sub schimbarea 1 din
D9-D1, de rescris nominal la pasul 2): `Program.cs:16902` (valoarea lăsată de
dry-run pe linie: 10,00 azi, estimarea 10,01 după), `17127` (valoarea de pe
linia de consum ASM după o operare refuzată), `31913` și `31924` (diferența
declarată N-r3: 50 în registru, 75 pe cub).

**Montaje scrise de mână fără geamăn pe cub**: `Program.cs:31846`, `32062`,
`32515`, `33172` (rânduri de registru create direct) și rândul `RegistruTva`
injectat prin SQL la `16611`, cu aserția `16627`, care rămâne fără obiect.
Fiecare scenă primește la pasul 3 un montaj pe cub (deschidere sau operare
reală) sau pierde aserția, cu rând în contabilitatea probelor (D9-D7 e).

## 14. Corecții după măsurătorile pasului 2 (2026-10-05)

Rândurile marcate *din cod* au fost măsurate la pasul 2. Trei diferă de ce
scrie mai sus: DSC pe bugetar e refuzat de frunză, nu rămâne Operat fără
efect (§6); DVI pe bugetar n-are tip de TVA de import, deci e refuzat
întotdeauna de frunză (§6); după stornarea NIR-ului acoperit cu consum
nimic nu e refuzat azi pe lot (098-r3, §12). Tabelul complet, cu probele și
cu cele două constatări despre gardul analizei (repartitorul și materialul):
[`tr-d9-pas2-probe.md`](tr-d9-pas2-probe.md).
