# TR-D8 SAF-B8 — raportul A/B final: ruta pe registre ↔ cubul

**Reverificare Codex pe `ddcac79`, 2026-09-30: B8-RV1.1 / P2 — corectat în `3a4372d`, în așteptarea reverificării.**
Mutanții inițiali de cod și ASM sunt respinși, dar martorul perechii acceptă
FCT 9/90 + NIR 1/10 în loc de FCT 10/100 + NIR fără mișcare. Verificarea
trebuie să lege și distribuția pe documente de postările reale; raportul
arhivat de mai jos necesită reverificarea cu acest martor completat.
[Contraexemplul executat](tr-d8-saft-b8-review-codex.md). **Corectat:** fiecare
document al perechii are mișcarea și GL-ul egale cu postările lui din cub în
lună (NIR fără delta = zero). Contraexemplul, ca mutant „recepția împărțită”,
și varianta lui GL sunt respinse. Raportul de mai jos s-a regenerat:
`run-verificari/20260930-230311-066`.

**Reverificat 2026-09-30 după review-ul Codex (B8-RV1).** Clasificatorul de la
`900cf7b` accepta două diferențe care depășeau martorii: codul FCT 10 → 80 cu
Q/V păstrate și ASM −3,33 → −3,32 când cubul cere −3,34. Martorii sunt acum
strânși (tabelul de mai jos), cu patru mutanți noi, toți respinși. Raportul s-a
regenerat pe starea cu ambele rute (`900cf7b` + `ddcac79` + `3a4372d`, branch
`tr-d8-saft-ab-rv1`, worktree și baze `.ClaudeAB` izolate). Integrala e verde
pe ambele profiluri: `run-verificari/20260930-230311-066`. Rezultatul are
aceleași 111 diferențe cu aceleași valori; diferă numai numerele de document
și sufixele loturilor, generate la rulare. Toate diferențele sunt clasificate
sub martorii strânși. [Review-ul](tr-d8-saft-b8-review-codex.md).

Data: 2026-09-30. Contract: [B8-D2](tr-d8-saft-contract.md#b8--gate-ul-final-saf-t-contract-pentru-aprobare-2026-09-30).
Rulare: `run-verificari/20260930-230311-066` (integrala pe ambele profiluri,
bugetar 3.269 / privat 4.500 OK, zero FAIL), raportul brut în același
director (`saft-ab-final.md`). Rulările anterioare, cu martori mai largi:
`run-verificari/20260930-131021-786` (inițială), `run-verificari/20260930-205445-461` (B8-RV1).

Raportul s-a generat înaintea ștergerii rutei vechi (B8-D1). După ștergere
se regenerează numai pe starea cu ambele rute, într-un worktree la
`tr-d8-saft-ab-rv1`. Cubul rămâne singura sursă, iar
catalogul scris de mână rămâne proba supremă (91).

## Metoda

`ModelCheck/SaftAb.cs` (la `3a4372d`, peste `900cf7b`; scos la pasul 2 odată cu ruta veche) compară semantic cele două exporturi pe aceeași bază
și în aceeași stare, pe fiecare lună a scenei. Scena este la capăt, înaintea
purjei. Cheile folosite sunt:

| Secțiune | Cheie | Valoare comparată |
|---|---|---|
| Conturi | `AccountID` | Opening/Closing D/C |
| Terți | rol × `Id` × `AccountID` | Opening/Closing D/C |
| GL | document × cont × D/C | Σ `Amount` |
| Facturi | document × storno | tip, număr, dată, cont, net/brut, liniile |
| Plăți | document × storno | brut, liniile (referință, cont, D/C, sumă) |
| Mișcări | document × storno × lot × gestiune | coduri, Σ Q / Σ V |
| Stoc | lot × gestiune | `ProductType`, Opening/Closing Q/V |
| Diagnostic | cod | numărul aparițiilor |

`StockAccountNo` și `WarehouseID` sunt forma cheii (S3-D5). Ele se verifică
prin XSD/DUK și prin unicitatea cheilor (S0-R8, S3-RV2.1), nu prin A/B
(SAF-B5).

Fiecare diferență primește o clasă numai dacă martorul ei numeric trece:

| Clasă | Martor |
|---|---|
| SAF-B5 recepția pe documentul-sursă | se mută numai documentul purtător: pe sursă vechiul lipsește, pe conex noul lipsește; **mișcarea și GL-ul noi ale fiecărui document = postările lui din cub în lună (conexul fără delta = zero)**; perechea document ↔ conex (FCT ↔ NIR) e egală cumulat pe scenă, pe cont × D/C și pe lot × gestiune × storno × **cod de mișcare**; excepție numai pentru conexul Draft fără niciun rând de registru |
| SAF-B5 factura sau linia de stoc fără rând în registrul vechi | perechea de mai sus; data = data documentului-rădăcină; net = Σ linii, brut = net + taxă; liniile vechi ⊂ liniile noi, iar diferența de net/brut = liniile în plus |
| SAF-B5 contul istoric | ruta veche are produsul fără cont (`FaraContStoc` sau `ProductType` „0”), iar contul liniei noi este contul postării din cub |
| S1-R4 data facturii | noua `InvoiceDate` = `Data` documentului-rădăcină al lanțului de corecții |
| S1-D5 (B') 381/384 | 380 vechi → 384 nou numai pe un document cu `CorecteazaId` |
| S1-D4 taxarea inversă | brut nou = net, brut vechi − brut nou = taxa, iar toate tipurile TVA ale documentului sunt TI |
| S1-D4 maparea lipsă | `TipTvaFaraCodSaft` pe ruta veche ↔ `SAFT_MAPARE_LIPSA` pe cub, în aceeași lună |
| S2-D2/D3 alocarea plății | brut egal, Σ linii egal, aceleași conturi × D/C; **fiecare țintă și suma ei = împerecherile documentului (în oricare rol) datate ≤ capătul lunii `Operare`**, cu `SourceDocumentID` = numărul țintei; restul, fără referință, = brut − Σ țintelor; stornoul = negatul |
| S3-R1 NIR delta | numai pe conex, cu ambele rute prezente; mișcarea nouă a NIR = postările lui din cub (delta); perechea FCT ↔ NIR egală cumulat pe lot × gestiune × storno × cod |
| S3-R2 Δ ASM | aceleași coduri, cantitate egală, \|ΔV\| ≤ 0,01; **Q/V nou = exact postările cubului ale documentului pe lot × gestiune × storno, în lună**; Σ V pe fiecare tranzacție ASM din cub = 0 |
| 094 jurnalul DESCHIDERE | Σ GL nou = Σ postărilor `Deschidere` din cub pe cont × latură × lună |
| Poziția pe sursă | nou − vechi = Σ pe document (postările cubului − rândurile `RegistruStoc`), Opening și Closing; fiecare document cu diferență este clasificat, este deschiderea (fără document) sau este artefact |
| Artefact de probă | documentul are rânduri de registru și zero postări în cub (`ProbeCub.Nemigrat` în D17-V2); în producție operarea scrie întotdeauna cubul |
| Diagnostic | codul vechi dispare sau apare numai împreună cu clasa care îl explică în aceeași lună |

Mutanții rulează pe prima lună a fiecărei scene și sunt toți respinși:

- brutul unei plăți +1;
- o linie GL +1;
- data unei facturi +1 zi;
- valoarea unei linii de mișcare +1;
- `ClosingValue` al unei poziții +1;
- codul recepției 10 → 80, cu Q/V păstrate (B8-RV1);
- valoarea unei ieșiri ASM +0,02, adică −3,34 → −3,32 față de −3,33 vechi:
  |Δ| rămâne 0,01, dar pleacă de la cub (B8-RV1);
- referința unei linii de plată alocate scoasă, cu sumele păstrate (B8-RV1);
- 1 mutat între două linii ale aceleiași plăți (B8-RV1);
- recepția împărțită: 1 buc mutată de pe FCT pe NIR-ul conex, cu totalul
  perechii păstrat (B8-RV1.1, contraexemplul Codex);
- GL împărțit: 1 mutat de pe FCT pe NIR-ul conex, pe același cont și latură
  (B8-RV1.1).

Un mutant se aplică numai unde luna are ținta lui. Un mutant care face
exportul nou identic cu cel vechi nu are obiect în A/B (A/B vede numai
diferențele). De aceea mutantul de referință se aplică pe o plată cu cel
puțin două linii.

## Defect găsit și corectat: avertismentul de valută

Pe D16-V2, factura FCT-EUR (curs 5) avea avertismentul `FacturaInValuta`
numai pe ruta veche. Cubul nu poartă valuta (B-r6), iar ambele rute declară
RON cu `CurrencyAmount` = `Amount`. Exportul pe cub pierduse însă
avertismentul fără nicio regulă care să justifice pierderea. Conform SAF-B5,
aceasta este un defect, nu o clasă de diferență. `SaftPeCub` citește acum
`FacturaIntrare.Valuta`, pe mulțimea restrânsă la tip (89b), și emite același
avertisment. După corectură, rândul nu mai apare în raport.

## Sumar pe clase

| Secțiune | Clasă | Rânduri |
|---|---|---|
| Diagnostic | S1-D4: maparea TVA lipsă refuză fișierul (avertismentul vechi devenit refuz) | 2 |
| Diagnostic | S1-D5 (B'): 381/384 poartă numărul facturii, fără avertisment | 4 |
| Diagnostic | S1-R: datele din tranzacția cubului, nu din DataOperare | 5 |
| Diagnostic | S2-D2 (SC-SAFT-33) | 1 |
| Diagnostic | S2-D3 (SC-SAFT-30) | 2 |
| Diagnostic | S2-R4: baza fără factură (DVI, decont) în cusături, nu Neincluse | 2 |
| Diagnostic | S3-D1: categoria pe cont; soldul nestoc nu e avertisment (F27-r10) | 3 |
| Diagnostic | SAF-B5: contul istoric al postării; produsul fără cont azi nu mai iese din fișier | 3 |
| Diagnostic | SAF-B5: linia de stoc a facturii are contul recepției în cub | 2 |
| Diagnostic | SAF-B5: produsul facturii emise numai de cub intră în MasterFiles cu avertismentele lui | 2 |
| Facturi | S1-D4 (TI: brutul fără autocolectare) | 2 |
| Facturi | S1-D5 (B') + S1-R4 | 3 |
| Facturi | S1-R4 | 8 |
| Facturi | SAF-B5: factura fără rând propriu în registrul vechi (recepția și baza pe conex); cubul o emite | 1 |
| Facturi | SAF-B5: linia de stoc a facturii are contul recepției în cub; ruta veche o lăsa în Neincluse (LinieFaraContrapartida) | 1 |
| GL | 094/SC-SAFT-45: jurnalul DESCHIDERE din cub; ruta veche nu vedea deschiderea generică | 4 |
| GL | SAF-B5: recepția stă pe documentul-sursă în cub, pe conex în registrul vechi (martor: fiecare document = postările lui din cub, perechea egală cumulat) | 18 |
| Mișcări | S3-R1: NIR delta față de recepția integrală pe NIR în registru (martor: fiecare document = postările lui din cub, perechea egală cumulat pe lot × gestiune × cod) | 2 |
| Mișcări | S3-R2: Δ ASM — Q/V nou = postările cubului pe document × lot × gestiune, \|Δ\| ≤ 0,01 față de registru, ΣP + ΣΔ = ΣC pe tranzacție | 2 |
| Mișcări | SAF-B5: contul istoric al postării față de contul curent al tipului de material (fără cont azi) | 1 |
| Mișcări | SAF-B5: recepția pe documentul-sursă în cub, pe conex în registru (martor: fiecare document = postările lui din cub, perechea egală cumulat pe lot × gestiune × cod) | 16 |
| Plăți | S2-D2/D3: alocarea la capătul lunii operării (martor: referințele și sumele = împerecherile datate ≤ capăt, restul fără referință) | 10 |
| Stoc | poziția pe sursă (cub − registru pe document): 094: deschiderea generică există numai în cub (fără registru) | 6 |
| Stoc | poziția pe sursă (cub − registru pe document): SAF-B5: contul istoric al postării față de contul curent al tipului de material (fără cont azi) | 1 |
| Stoc | poziția pe sursă (cub − registru pe document): SAF-B5: contul istoric al postării față de contul curent al tipului de material (fără cont azi) + artefact de probă: document operat nemigrat, numai în registru (imposibil în producție, ProbeCub.Nemigrat) | 1 |
| Stoc | poziția pe sursă (cub − registru pe document): artefact de probă: document operat nemigrat, numai în registru (imposibil în producție, ProbeCub.Nemigrat) | 1 |
| Stoc | poziția pe sursă (cub − registru pe document): Σ al diferențelor clasificate pe documente | 8 |

Rânduri pe scenă: D16-V2 20, D17-V2 12, E2E-SC-DES 23, E2E-SC-SAFT 28, E2E-SC-SAFTS 28. Toate sunt pe profilul privat. Pe bugetar SAF-T este neaplicabil (TR-r7, SC-SAFT-14/48), deci comparația nu are obiect.

## Rândurile

| Scenă | Profil | Lună | Secțiune | Cheie | Vechi | Nou | Clasă |
|---|---|---|---|---|---|---|---|
| D16-V2 | privat | 2026-08 | GL | FacturaIntrare E2E-SAFT-FCT 401 C | 1273.00 | 1573.00 | SAF-B5: recepția stă pe documentul-sursă în cub, pe conex în registrul vechi (martor: fiecare document = postările lui din cub, perechea egală cumulat) |
| D16-V2 | privat | 2026-08 | GL | FacturaIntrare E2E-SAFT-FCT-EUR 401 C | 142.00 | 242.00 | SAF-B5: recepția stă pe documentul-sursă în cub, pe conex în registrul vechi (martor: fiecare document = postările lui din cub, perechea egală cumulat) |
| D16-V2 | privat | 2026-08 | GL | NIR E2E-SAFT-NIR-A 371 D | 300.00 | absent | SAF-B5: recepția stă pe documentul-sursă în cub, pe conex în registrul vechi (martor: fiecare document = postările lui din cub, perechea egală cumulat) |
| D16-V2 | privat | 2026-08 | GL | NIR E2E-SAFT-NIR-A 401 C | 300.00 | absent | SAF-B5: recepția stă pe documentul-sursă în cub, pe conex în registrul vechi (martor: fiecare document = postările lui din cub, perechea egală cumulat) |
| D16-V2 | privat | 2026-08 | GL | NIR E2E-SAFT-NIR-TI 371 D | 100.00 | absent | SAF-B5: recepția stă pe documentul-sursă în cub, pe conex în registrul vechi (martor: fiecare document = postările lui din cub, perechea egală cumulat) |
| D16-V2 | privat | 2026-08 | GL | NIR E2E-SAFT-NIR-TI 401 C | 100.00 | absent | SAF-B5: recepția stă pe documentul-sursă în cub, pe conex în registrul vechi (martor: fiecare document = postările lui din cub, perechea egală cumulat) |
| D16-V2 | privat | 2026-08 | GL | FacturaIntrare E2E-SAFT-FCT 371 D | absent | 300.00 | SAF-B5: recepția stă pe documentul-sursă în cub, pe conex în registrul vechi (martor: fiecare document = postările lui din cub, perechea egală cumulat) |
| D16-V2 | privat | 2026-08 | GL | FacturaIntrare E2E-SAFT-FCT-EUR 371 D | absent | 100.00 | SAF-B5: recepția stă pe documentul-sursă în cub, pe conex în registrul vechi (martor: fiecare document = postările lui din cub, perechea egală cumulat) |
| D16-V2 | privat | 2026-08 | GL | FacturaIntrare E2E-SAFT-FCT-TI 401 C | absent | 100.00 | SAF-B5: recepția stă pe documentul-sursă în cub, pe conex în registrul vechi (martor: fiecare document = postările lui din cub, perechea egală cumulat) |
| D16-V2 | privat | 2026-08 | GL | FacturaIntrare E2E-SAFT-FCT-TI 371 D | absent | 100.00 | SAF-B5: recepția stă pe documentul-sursă în cub, pe conex în registrul vechi (martor: fiecare document = postările lui din cub, perechea egală cumulat) |
| D16-V2 | privat | 2026-08 | Facturi | FacturaIesire FCL-51 storno | 381 FCL-51 2026-08-25 4111 -400.00/-484.00 [704 1.00×-400.00 310344 -84.00] | 381 FCL-51 2026-08-10 4111 -400.00/-484.00 [704 1.00×-400.00 310344 -84.00] | S1-R4 |
| D16-V2 | privat | 2026-08 | Facturi | FacturaIntrare E2E-SAFT-FCT-EUR | 380 E2E-SAFT-FCT-EUR 2026-08-03 401 100.00/121.00 [628 1.00×100.00 301104 21.00] | 380 E2E-SAFT-FCT-EUR 2026-08-03 401 200.00/242.00 [371 5.00×100.00 301104 21.00; 628 1.00×100.00 301104 21.00] | SAF-B5: linia de stoc a facturii are contul recepției în cub; ruta veche o lăsa în Neincluse (LinieFaraContrapartida) |
| D16-V2 | privat | 2026-08 | Facturi | FacturaIntrare E2E-SAFT-FCT-TI | 380 E2E-SAFT-FCT-TI 2026-08-03 401 100.00/121.00 [371 4.00×100.00 300906 21.00] | 380 E2E-SAFT-FCT-TI 2026-08-03 401 100.00/100.00 [371 4.00×100.00 300906 21.00] | S1-D4 (TI: brutul fără autocolectare) |
| D16-V2 | privat | 2026-08 | Plăți | Plata PLT-54 | 1190.00 [E2E-SAFT-FCT 401 D 1190.00] | 1190.00 [ 401 D 1190.00] | S2-D2/D3: alocarea la capătul lunii operării (martor: referințele și sumele = împerecherile datate ≤ capăt, restul fără referință) |
| D16-V2 | privat | 2026-08 | Diagnostic | avertisment TipTvaFaraCodSaft | 1 | absent | S1-D4: maparea TVA lipsă refuză fișierul (avertismentul vechi devenit refuz) |
| D16-V2 | privat | 2026-08 | Diagnostic | avertisment LinieFaraContrapartida | 1 | absent | SAF-B5: linia de stoc a facturii are contul recepției în cub |
| D16-V2 | privat | 2026-08 | Diagnostic | avertisment NumarFacturaDuplicat | 1 | absent | S1-D5 (B'): 381/384 poartă numărul facturii, fără avertisment |
| D16-V2 | privat | 2026-08 | Diagnostic | neinclus FaraContrapartida | 1 | absent | SAF-B5: linia de stoc a facturii are contul recepției în cub |
| D16-V2 | privat | 2026-08 | Diagnostic | neinclus TipFaraSectiuneFacturi | 1 | absent | S2-R4: baza fără factură (DVI, decont) în cusături, nu Neincluse |
| D16-V2 | privat | 2026-08 | Diagnostic | refuz SAFT_MAPARE_LIPSA | absent | 2 | S1-D4: maparea TVA lipsă refuză fișierul (avertismentul vechi devenit refuz) |
| D17-V2 | privat | 2027-02 | Stoc | lot Marfă fără cont de stoc · 31.12.2026 · 5.0000 #f4bd gest E2E-SAFT-S-MAG1 | 0 30.00/150.00 → 30.00/150.00 | 371 30.00/150.00 → 30.00/150.00 | poziția pe sursă (cub − registru pe document): SAF-B5: contul istoric al postării față de contul curent al tipului de material (fără cont azi) |
| D17-V2 | privat | 2027-02 | Diagnostic | avertisment ProdusFaraContStoc | 1 | absent | SAF-B5: contul istoric al postării; produsul fără cont azi nu mai iese din fișier |
| D17-V2 | privat | 2027-02 | Diagnostic | avertisment DataPostariiInAfaraPerioadei | 1 | absent | S1-R: datele din tranzacția cubului, nu din DataOperare |
| D17-V2 | privat | 2027-03 | Mișcări | NIR NIR-167 lot Marfă SAF-T S A · 03.03.2027 · 30.0000 #07e2 gest E2E-SAFT-S-MAG1 | 10 10.00/300.00 | absent | SAF-B5: recepția pe documentul-sursă în cub, pe conex în registru (martor: fiecare document = postările lui din cub, perechea egală cumulat pe lot × gestiune × cod) |
| D17-V2 | privat | 2027-03 | Mișcări | FacturaIntrare E2E-SAFT-S-FCT lot Marfă SAF-T S A · 03.03.2027 · 30.0000 #07e2 gest E2E-SAFT-S-MAG1 | absent | 10 10.00/300.00 | SAF-B5: recepția pe documentul-sursă în cub, pe conex în registru (martor: fiecare document = postările lui din cub, perechea egală cumulat pe lot × gestiune × cod) |
| D17-V2 | privat | 2027-03 | Mișcări | Asamblare E2E-SAFT-S-ASM lot Marfă fără cont de stoc · 31.12.2026 · 5.0000 #f4bd gest E2E-SAFT-S-MAG1 | absent | 70 -3.00/-15.00 | SAF-B5: contul istoric al postării față de contul curent al tipului de material (fără cont azi) |
| D17-V2 | privat | 2027-03 | Stoc | lot Marfă fără cont de stoc · 31.12.2026 · 5.0000 #f4bd gest E2E-SAFT-S-MAG1 | 0 30.00/150.00 → 24.00/120.00 | 371 30.00/150.00 → 27.00/135.00 | poziția pe sursă (cub − registru pe document): SAF-B5: contul istoric al postării față de contul curent al tipului de material (fără cont azi) + artefact de probă: document operat nemigrat, numai în registru (imposibil în producție, ProbeCub.Nemigrat) |
| D17-V2 | privat | 2027-03 | Stoc | lot Marfă fără cont de stoc · 31.12.2026 · 5.0000 #f4bd gest E2E-SAFT-S-MAG2 | 0 0.00/0.00 → 3.00/15.00 | absent | poziția pe sursă (cub − registru pe document): artefact de probă: document operat nemigrat, numai în registru (imposibil în producție, ProbeCub.Nemigrat) |
| D17-V2 | privat | 2027-03 | Diagnostic | avertisment ProdusFaraContStoc | 1 | absent | SAF-B5: contul istoric al postării; produsul fără cont azi nu mai iese din fișier |
| D17-V2 | privat | 2027-03 | Diagnostic | avertisment SoldPeTipStocNeraportat | 1 | absent | S3-D1: categoria pe cont; soldul nestoc nu e avertisment (F27-r10) |
| D17-V2 | privat | 2027-03 | Diagnostic | avertisment DataPostariiInAfaraPerioadei | 1 | absent | S1-R: datele din tranzacția cubului, nu din DataOperare |
| D17-V2 | privat | 2027-03 | Diagnostic | neinclus FaraContStoc | 1 | absent | SAF-B5: contul istoric al postării; produsul fără cont azi nu mai iese din fișier |
| E2E-SC-DES | privat | 2017-01 | GL | NIR NIR-195 302 D | 50.00 | absent | SAF-B5: recepția stă pe documentul-sursă în cub, pe conex în registrul vechi (martor: fiecare document = postările lui din cub, perechea egală cumulat) |
| E2E-SC-DES | privat | 2017-01 | GL | NIR NIR-195 E2ESCDES C | 50.00 | absent | SAF-B5: recepția stă pe documentul-sursă în cub, pe conex în registrul vechi (martor: fiecare document = postările lui din cub, perechea egală cumulat) |
| E2E-SC-DES | privat | 2017-01 | GL | Deschidere E2ESCDES C | absent | 150.00 | 094/SC-SAFT-45: jurnalul DESCHIDERE din cub; ruta veche nu vedea deschiderea generică |
| E2E-SC-DES | privat | 2017-01 | GL | Deschidere 891 C | absent | 100.00 | 094/SC-SAFT-45: jurnalul DESCHIDERE din cub; ruta veche nu vedea deschiderea generică |
| E2E-SC-DES | privat | 2017-01 | GL | Deschidere 891 D | absent | 150.00 | 094/SC-SAFT-45: jurnalul DESCHIDERE din cub; ruta veche nu vedea deschiderea generică |
| E2E-SC-DES | privat | 2017-01 | GL | Deschidere 302 D | absent | 100.00 | 094/SC-SAFT-45: jurnalul DESCHIDERE din cub; ruta veche nu vedea deschiderea generică |
| E2E-SC-DES | privat | 2017-01 | GL | FacturaIntrare E2E-SC-DES-1 E2ESCDES C | absent | 50.00 | SAF-B5: recepția stă pe documentul-sursă în cub, pe conex în registrul vechi (martor: fiecare document = postările lui din cub, perechea egală cumulat) |
| E2E-SC-DES | privat | 2017-01 | GL | FacturaIntrare E2E-SC-DES-1 302 D | absent | 50.00 | SAF-B5: recepția stă pe documentul-sursă în cub, pe conex în registrul vechi (martor: fiecare document = postările lui din cub, perechea egală cumulat) |
| E2E-SC-DES | privat | 2017-01 | Facturi | FacturaIntrare E2E-SC-DES-1 | absent | 380 E2E-SC-DES-1 2017-01-05 E2ESCDES 50.00/50.00 [302 5.00×50.00 000000 0.00] | SAF-B5: factura fără rând propriu în registrul vechi (recepția și baza pe conex); cubul o emite |
| E2E-SC-DES | privat | 2017-01 | Plăți | Plata PLT-66 | 20.00 [ E2ESCDES D 20.00] | 20.00 [NTC-177 E2ESCDES D 20.00] | S2-D2/D3: alocarea la capătul lunii operării (martor: referințele și sumele = împerecherile datate ≤ capăt, restul fără referință) |
| E2E-SC-DES | privat | 2017-01 | Diagnostic | avertisment FaraCodNc | absent | 1 | SAF-B5: produsul facturii emise numai de cub intră în MasterFiles cu avertismentele lui |
| E2E-SC-DES | privat | 2017-01 | Diagnostic | avertisment FaraUnitateMasura | absent | 1 | SAF-B5: produsul facturii emise numai de cub intră în MasterFiles cu avertismentele lui |
| E2E-SC-DES | privat | 2017-01 | Diagnostic | avertisment PlataPePartidaInitiala | absent | 1 | S2-D3 (SC-SAFT-30) |
| E2E-SC-DES | privat | 2017-02 | Diagnostic | avertisment PlataPePartidaInitiala | absent | 1 | S2-D3 (SC-SAFT-30) |
| E2E-SC-DES | privat | 2017-01 | Mișcări | NIR NIR-195 lot E2E-SC-DES-P2 · 05.01.2017 · 10.0000 #450d gest MAG1 | 10 5.00/50.00 | absent | SAF-B5: recepția pe documentul-sursă în cub, pe conex în registru (martor: fiecare document = postările lui din cub, perechea egală cumulat pe lot × gestiune × cod) |
| E2E-SC-DES | privat | 2017-01 | Mișcări | FacturaIntrare E2E-SC-DES-1 lot E2E-SC-DES-P2 · 05.01.2017 · 10.0000 #450d gest MAG1 | absent | 10 5.00/50.00 | SAF-B5: recepția pe documentul-sursă în cub, pe conex în registru (martor: fiecare document = postările lui din cub, perechea egală cumulat pe lot × gestiune × cod) |
| E2E-SC-DES | privat | 2017-01 | Stoc | lot E2E-SC-DES-1 · 05.01.2017 · 10.0000 #d058 gest MAG1 | 302 0.00/0.00 → 0.00/0.00 | 302 4.00/40.00 → 4.00/40.00 | poziția pe sursă (cub − registru pe document): 094: deschiderea generică există numai în cub (fără registru) |
| E2E-SC-DES | privat | 2017-01 | Stoc | lot E2E-SC-DES-2 · 05.01.2017 · 20.0000 #4749 gest MAG1 | absent | 302 3.00/60.00 → 3.00/60.00 | poziția pe sursă (cub − registru pe document): 094: deschiderea generică există numai în cub (fără registru) |
| E2E-SC-DES | privat | 2017-01 | Stoc | lot E2E-SC-DES-3 · 05.01.2017 · 0.0000 #7d03 gest MAG1 | absent | 302 2.00/0.00 → 2.00/0.00 | poziția pe sursă (cub − registru pe document): 094: deschiderea generică există numai în cub (fără registru) |
| E2E-SC-DES | privat | 2017-01 | Diagnostic | avertisment DataPostariiInAfaraPerioadei | 1 | absent | S1-R: datele din tranzacția cubului, nu din DataOperare |
| E2E-SC-DES | privat | 2017-02 | Stoc | lot E2E-SC-DES-1 · 05.01.2017 · 10.0000 #d058 gest MAG1 | absent | 302 4.00/40.00 → 4.00/40.00 | poziția pe sursă (cub − registru pe document): 094: deschiderea generică există numai în cub (fără registru) |
| E2E-SC-DES | privat | 2017-02 | Stoc | lot E2E-SC-DES-2 · 05.01.2017 · 20.0000 #4749 gest MAG1 | absent | 302 3.00/60.00 → 3.00/60.00 | poziția pe sursă (cub − registru pe document): 094: deschiderea generică există numai în cub (fără registru) |
| E2E-SC-DES | privat | 2017-02 | Stoc | lot E2E-SC-DES-3 · 05.01.2017 · 0.0000 #7d03 gest MAG1 | absent | 302 2.00/0.00 → 2.00/0.00 | poziția pe sursă (cub − registru pe document): 094: deschiderea generică există numai în cub (fără registru) |
| E2E-SC-SAFT | privat | 2040-01 | GL | FacturaIntrare E2E-SC-SAFT-13 401 C | 21.00 | 121.00 | SAF-B5: recepția stă pe documentul-sursă în cub, pe conex în registrul vechi (martor: fiecare document = postările lui din cub, perechea egală cumulat) |
| E2E-SC-SAFT | privat | 2040-01 | GL | NIR NIR-285 371 D | 100.00 | absent | SAF-B5: recepția stă pe documentul-sursă în cub, pe conex în registrul vechi (martor: fiecare document = postările lui din cub, perechea egală cumulat) |
| E2E-SC-SAFT | privat | 2040-01 | GL | NIR NIR-285 401 C | 100.00 | absent | SAF-B5: recepția stă pe documentul-sursă în cub, pe conex în registrul vechi (martor: fiecare document = postările lui din cub, perechea egală cumulat) |
| E2E-SC-SAFT | privat | 2040-01 | GL | FacturaIntrare E2E-SC-SAFT-13 371 D | absent | 100.00 | SAF-B5: recepția stă pe documentul-sursă în cub, pe conex în registrul vechi (martor: fiecare document = postările lui din cub, perechea egală cumulat) |
| E2E-SC-SAFT | privat | 2040-01 | Facturi | FacturaIntrare E2E-SC-SAFT-11 storno | 381 E2E-SC-SAFT-11 2040-01-20 401 -100.00/-121.00 [628 1.00×-100.00 301104 -21.00] | 381 E2E-SC-SAFT-11 2040-01-05 401 -100.00/-121.00 [628 1.00×-100.00 301104 -21.00] | S1-R4 |
| E2E-SC-SAFT | privat | 2040-01 | Facturi | FacturaIntrare E2E-SC-SAFT-6 | 380 E2E-SC-SAFT-6 2040-01-05 401 100.00/121.00 [628 1.00×100.00 300906 21.00] | 380 E2E-SC-SAFT-6 2040-01-05 401 100.00/100.00 [628 1.00×100.00 300906 21.00] | S1-D4 (TI: brutul fără autocolectare) |
| E2E-SC-SAFT | privat | 2040-01 | Facturi | FacturaIntrare E2E-SC-SAFT-8 | 380 E2E-SC-SAFT-8 2040-01-10 401 100.00/121.00 [628 1.00×100.00 301104 21.00] | 380 E2E-SC-SAFT-8 2040-01-08 401 100.00/121.00 [628 1.00×100.00 301104 21.00] | S1-R4 |
| E2E-SC-SAFT | privat | 2040-01 | Plăți | Plata PLT-100 | 120.00 [E2E-SC-SAFT-18 401 D 120.00] | 120.00 [ 401 D 20.00; E2E-SC-SAFT-18 401 D 100.00] | S2-D2/D3: alocarea la capătul lunii operării (martor: referințele și sumele = împerecherile datate ≤ capăt, restul fără referință) |
| E2E-SC-SAFT | privat | 2040-01 | Plăți | Plata PLT-101 | 70.00 [E2E-SC-SAFT-19 401 D 70.00] | 70.00 [ 401 D 20.00; E2E-SC-SAFT-19 401 D 50.00] | S2-D2/D3: alocarea la capătul lunii operării (martor: referințele și sumele = împerecherile datate ≤ capăt, restul fără referință) |
| E2E-SC-SAFT | privat | 2040-01 | Plăți | Plata PLT-106 | 20.00 [ 401 D 20.00] | 20.00 [NTC-239 401 D 20.00] | S2-D2/D3: alocarea la capătul lunii operării (martor: referințele și sumele = împerecherile datate ≤ capăt, restul fără referință) |
| E2E-SC-SAFT | privat | 2040-01 | Plăți | Plata PLT-108 | 100.00 [ 401 D 40.00;  401 D 60.00] | 100.00 [ 401 D 100.00] | S2-D2/D3: alocarea la capătul lunii operării (martor: referințele și sumele = împerecherile datate ≤ capăt, restul fără referință) |
| E2E-SC-SAFT | privat | 2040-01 | Plăți | Plata PLT-96 | 70.00 [E2E-SC-SAFT-15 401 D 70.00] | 70.00 [ 401 D 20.00; E2E-SC-SAFT-15 401 D 50.00] | S2-D2/D3: alocarea la capătul lunii operării (martor: referințele și sumele = împerecherile datate ≤ capăt, restul fără referință) |
| E2E-SC-SAFT | privat | 2040-01 | Plăți | Plata PLT-97 | 60.00 [E2E-SC-SAFT-16 401 D 60.00] | 60.00 [ 401 D 60.00] | S2-D2/D3: alocarea la capătul lunii operării (martor: referințele și sumele = împerecherile datate ≤ capăt, restul fără referință) |
| E2E-SC-SAFT | privat | 2040-01 | Plăți | Plata PLT-98 | 70.00 [E2E-SC-SAFT-16 401 D 70.00] | 70.00 [ 401 D 70.00] | S2-D2/D3: alocarea la capătul lunii operării (martor: referințele și sumele = împerecherile datate ≤ capăt, restul fără referință) |
| E2E-SC-SAFT | privat | 2040-01 | Diagnostic | avertisment NumarFacturaDuplicat | 1 | absent | S1-D5 (B'): 381/384 poartă numărul facturii, fără avertisment |
| E2E-SC-SAFT | privat | 2040-01 | Diagnostic | neinclus TipFaraSectiuneFacturi | 1 | absent | S2-R4: baza fără factură (DVI, decont) în cusături, nu Neincluse |
| E2E-SC-SAFT | privat | 2040-01 | Diagnostic | avertisment PlataAnalizaMixta | absent | 1 | S2-D2 (SC-SAFT-33) |
| E2E-SC-SAFT | privat | 2040-02 | Facturi | FacturaIntrare E2E-SC-SAFT-12 storno | 381 E2E-SC-SAFT-12 2040-02-05 401 -100.00/-121.00 [628 1.00×-100.00 301104 -21.00] | 381 E2E-SC-SAFT-12 2040-01-05 401 -100.00/-121.00 [628 1.00×-100.00 301104 -21.00] | S1-R4 |
| E2E-SC-SAFT | privat | 2040-02 | Facturi | FacturaIntrare E2E-SC-SAFT-7 storno | 381 E2E-SC-SAFT-7 2040-02-05 401 -100.00/-121.00 [628 2.00×-100.00 301104 -21.00] | 381 E2E-SC-SAFT-7 2040-01-05 401 -100.00/-121.00 [628 2.00×-100.00 301104 -21.00] | S1-R4 |
| E2E-SC-SAFT | privat | 2040-02 | Facturi | FacturaIntrare E2E-SC-SAFT-7 | 380 E2E-SC-SAFT-7 2040-02-05 401 120.00/145.20 [628 3.00×120.00 301104 25.20] | 384 E2E-SC-SAFT-7 2040-01-05 401 120.00/145.20 [628 3.00×120.00 301104 25.20] | S1-D5 (B') + S1-R4 |
| E2E-SC-SAFT | privat | 2040-02 | Facturi | FacturaIntrare E2E-SC-SAFT-8 storno | 381 E2E-SC-SAFT-8 2040-02-05 401 -100.00/-121.00 [628 1.00×-100.00 301104 -21.00] | 381 E2E-SC-SAFT-8 2040-01-08 401 -100.00/-121.00 [628 1.00×-100.00 301104 -21.00] | S1-R4 |
| E2E-SC-SAFT | privat | 2040-02 | Facturi | FacturaIntrare E2E-SC-SAFT-8 | 380 E2E-SC-SAFT-8 2040-02-05 401 80.00/96.80 [628 1.00×80.00 301104 16.80] | 384 E2E-SC-SAFT-8 2040-01-08 401 80.00/96.80 [628 1.00×80.00 301104 16.80] | S1-D5 (B') + S1-R4 |
| E2E-SC-SAFT | privat | 2040-02 | Facturi | FacturaIntrare E2E-SC-SAFT-9 | 380 E2E-SC-SAFT-9 2040-02-05 401 100.00/121.00 [628 1.00×100.00 301104 21.00] | 380 E2E-SC-SAFT-9 2040-01-08 401 100.00/121.00 [628 1.00×100.00 301104 21.00] | S1-R4 |
| E2E-SC-SAFT | privat | 2040-02 | Plăți | Plata PLT-101 storno | -70.00 [E2E-SC-SAFT-19 401 D -70.00] | -70.00 [ 401 D -20.00; E2E-SC-SAFT-19 401 D -50.00] | S2-D2/D3: alocarea la capătul lunii operării (martor: referințele și sumele = împerecherile datate ≤ capăt, restul fără referință) |
| E2E-SC-SAFT | privat | 2040-02 | Diagnostic | avertisment NumarFacturaDuplicat | 1 | absent | S1-D5 (B'): 381/384 poartă numărul facturii, fără avertisment |
| E2E-SC-SAFT | privat | 2040-03 | Facturi | FacturaIntrare E2E-SC-SAFT-8 storno | 381 E2E-SC-SAFT-8 2040-03-05 401 -80.00/-96.80 [628 1.00×-80.00 301104 -16.80] | 381 E2E-SC-SAFT-8 2040-01-08 401 -80.00/-96.80 [628 1.00×-80.00 301104 -16.80] | S1-R4 |
| E2E-SC-SAFT | privat | 2040-03 | Facturi | FacturaIntrare E2E-SC-SAFT-8 | 380 E2E-SC-SAFT-8 2040-03-05 401 70.00/84.70 [628 1.00×70.00 301104 14.70] | 384 E2E-SC-SAFT-8 2040-01-08 401 70.00/84.70 [628 1.00×70.00 301104 14.70] | S1-D5 (B') + S1-R4 |
| E2E-SC-SAFT | privat | 2040-03 | Diagnostic | avertisment NumarFacturaDuplicat | 1 | absent | S1-D5 (B'): 381/384 poartă numărul facturii, fără avertisment |
| E2E-SC-SAFTS | privat | 2041-01 | Mișcări | NIR NIR-286 lot E2E-SC-SAFTS-P2 · 05.01.2041 · 10.0000 #d5da gest MAG1 | 10 10.00/100.00 | absent | SAF-B5: recepția pe documentul-sursă în cub, pe conex în registru (martor: fiecare document = postările lui din cub, perechea egală cumulat pe lot × gestiune × cod) |
| E2E-SC-SAFTS | privat | 2041-01 | Mișcări | NIR NIR-287 lot E2E-SC-SAFTS-P4 · 05.01.2041 · 10.0000 #3703 gest MAG1 | 10 2.00/20.00 | absent | SAF-B5: recepția pe documentul-sursă în cub, pe conex în registru (martor: fiecare document = postările lui din cub, perechea egală cumulat pe lot × gestiune × cod) |
| E2E-SC-SAFTS | privat | 2041-01 | Mișcări | NIR NIR-287 lot E2E-SC-SAFTS-P5 · 05.01.2041 · 12.0000 #1d57 gest MAG1 | 10 5.00/60.00 | absent | SAF-B5: recepția pe documentul-sursă în cub, pe conex în registru (martor: fiecare document = postările lui din cub, perechea egală cumulat pe lot × gestiune × cod) |
| E2E-SC-SAFTS | privat | 2041-01 | Mișcări | NIR NIR-288 lot E2E-SC-SAFTS-P7 · 05.01.2041 · 3.3333 #205c gest MAG1 | 10 3.00/10.00 | absent | SAF-B5: recepția pe documentul-sursă în cub, pe conex în registru (martor: fiecare document = postările lui din cub, perechea egală cumulat pe lot × gestiune × cod) |
| E2E-SC-SAFTS | privat | 2041-01 | Mișcări | Asamblare ASM-56 lot E2E-SC-SAFTS-ASM3 · 05.01.2041 · 3.3300 #0f78 gest MAG1 | 20 1.00/3.33 | 20 1.00/3.34 | S3-R2: Δ ASM — Q/V nou = postările cubului pe document × lot × gestiune, \|Δ\| ≤ 0,01 față de registru, ΣP + ΣΔ = ΣC pe tranzacție |
| E2E-SC-SAFTS | privat | 2041-01 | Mișcări | Asamblare ASM-56 lot E2E-SC-SAFTS-P7 · 05.01.2041 · 3.3333 #205c gest MAG1 | 70 -1.00/-3.33 | 70 -1.00/-3.34 | S3-R2: Δ ASM — Q/V nou = postările cubului pe document × lot × gestiune, \|Δ\| ≤ 0,01 față de registru, ΣP + ΣΔ = ΣC pe tranzacție |
| E2E-SC-SAFTS | privat | 2041-01 | Mișcări | NIR NIR-289 lot E2E-SC-SAFTS-P9 · 05.01.2041 · 10.0000 #8a52 gest MAG1 | 10 3.00/30.00 | absent | SAF-B5: recepția pe documentul-sursă în cub, pe conex în registru (martor: fiecare document = postările lui din cub, perechea egală cumulat pe lot × gestiune × cod) |
| E2E-SC-SAFTS | privat | 2041-01 | Mișcări | FacturaIntrare E2E-SC-SAFTS-1 lot E2E-SC-SAFTS-P2 · 05.01.2041 · 10.0000 #d5da gest MAG1 | absent | 10 10.00/100.00 | SAF-B5: recepția pe documentul-sursă în cub, pe conex în registru (martor: fiecare document = postările lui din cub, perechea egală cumulat pe lot × gestiune × cod) |
| E2E-SC-SAFTS | privat | 2041-01 | Mișcări | FacturaIntrare E2E-SC-SAFTS-3 lot E2E-SC-SAFTS-P4 · 05.01.2041 · 10.0000 #3703 gest MAG1 | absent | 10 2.00/20.00 | SAF-B5: recepția pe documentul-sursă în cub, pe conex în registru (martor: fiecare document = postările lui din cub, perechea egală cumulat pe lot × gestiune × cod) |
| E2E-SC-SAFTS | privat | 2041-01 | Mișcări | FacturaIntrare E2E-SC-SAFTS-3 lot E2E-SC-SAFTS-P5 · 05.01.2041 · 12.0000 #1d57 gest MAG1 | absent | 10 5.00/60.00 | SAF-B5: recepția pe documentul-sursă în cub, pe conex în registru (martor: fiecare document = postările lui din cub, perechea egală cumulat pe lot × gestiune × cod) |
| E2E-SC-SAFTS | privat | 2041-01 | Mișcări | FacturaIntrare E2E-SC-SAFTS-6 lot E2E-SC-SAFTS-P7 · 05.01.2041 · 3.3333 #205c gest MAG1 | absent | 10 3.00/10.00 | SAF-B5: recepția pe documentul-sursă în cub, pe conex în registru (martor: fiecare document = postările lui din cub, perechea egală cumulat pe lot × gestiune × cod) |
| E2E-SC-SAFTS | privat | 2041-01 | Mișcări | FacturaIntrare E2E-SC-SAFTS-8 lot E2E-SC-SAFTS-P9 · 05.01.2041 · 10.0000 #8a52 gest MAG1 | absent | 10 3.00/30.00 | SAF-B5: recepția pe documentul-sursă în cub, pe conex în registru (martor: fiecare document = postările lui din cub, perechea egală cumulat pe lot × gestiune × cod) |
| E2E-SC-SAFTS | privat | 2041-01 | Mișcări | FacturaIntrare E2E-SC-SAFTS-10 lot E2E-SC-SAFTS-P11 · 05.01.2041 · 25.0000 #bd1d gest MAG1 | absent | 10 4.00/100.00 | SAF-B5: recepția pe documentul-sursă în cub, pe conex în registru (martor: fiecare document = postările lui din cub, perechea egală cumulat pe lot × gestiune × cod) |
| E2E-SC-SAFTS | privat | 2041-01 | Mișcări | FacturaIntrare E2E-SC-SAFTS-12 lot E2E-SC-SAFTS-P13 · 05.01.2041 · 25.0000 #ca4e gest MAG1 | absent | 10 4.00/100.00 | SAF-B5: recepția pe documentul-sursă în cub, pe conex în registru (martor: fiecare document = postările lui din cub, perechea egală cumulat pe lot × gestiune × cod) |
| E2E-SC-SAFTS | privat | 2041-01 | Stoc | lot E2E-SC-SAFTS-P7 · 05.01.2041 · 3.3333 #205c gest MAG1 | 302 0.00/0.00 → 1.00/3.34 | 302 0.00/0.00 → 1.00/3.33 | poziția pe sursă (cub − registru pe document): Σ al diferențelor clasificate pe documente |
| E2E-SC-SAFTS | privat | 2041-01 | Stoc | lot E2E-SC-SAFTS-ASM3 · 05.01.2041 · 3.3300 #0f78 gest MAG1 | 302 0.00/0.00 → 1.00/3.33 | 302 0.00/0.00 → 1.00/3.34 | poziția pe sursă (cub − registru pe document): Σ al diferențelor clasificate pe documente |
| E2E-SC-SAFTS | privat | 2041-01 | Stoc | lot E2E-SC-SAFTS-P11 · 05.01.2041 · 25.0000 #bd1d gest MAG1 | absent | 302 0.00/0.00 → 4.00/100.00 | poziția pe sursă (cub − registru pe document): Σ al diferențelor clasificate pe documente |
| E2E-SC-SAFTS | privat | 2041-01 | Stoc | lot E2E-SC-SAFTS-P13 · 05.01.2041 · 25.0000 #ca4e gest MAG1 | absent | 302 0.00/0.00 → 4.00/100.00 | poziția pe sursă (cub − registru pe document): Σ al diferențelor clasificate pe documente |
| E2E-SC-SAFTS | privat | 2041-01 | Diagnostic | avertisment SoldPeTipStocNeraportat | 1 | absent | S3-D1: categoria pe cont; soldul nestoc nu e avertisment (F27-r10) |
| E2E-SC-SAFTS | privat | 2041-01 | Diagnostic | avertisment DataPostariiInAfaraPerioadei | 1 | absent | S1-R: datele din tranzacția cubului, nu din DataOperare |
| E2E-SC-SAFTS | privat | 2041-02 | Mișcări | NIR NIR-290 lot E2E-SC-SAFTS-P11 · 05.01.2041 · 25.0000 #bd1d gest MAG1 | 10 3.00/75.00 | 10 -1.00/-25.00 | S3-R1: NIR delta față de recepția integrală pe NIR în registru (martor: fiecare document = postările lui din cub, perechea egală cumulat pe lot × gestiune × cod) |
| E2E-SC-SAFTS | privat | 2041-02 | Mișcări | NIR NIR-291 lot E2E-SC-SAFTS-P13 · 05.01.2041 · 25.0000 #ca4e gest MAG1 | 10 5.00/125.00 | 10 1.00/25.00 | S3-R1: NIR delta față de recepția integrală pe NIR în registru (martor: fiecare document = postările lui din cub, perechea egală cumulat pe lot × gestiune × cod) |
| E2E-SC-SAFTS | privat | 2041-02 | Stoc | lot E2E-SC-SAFTS-P7 · 05.01.2041 · 3.3333 #205c gest MAG1 | 302 1.00/3.34 → 1.00/3.34 | 302 1.00/3.33 → 1.00/3.33 | poziția pe sursă (cub − registru pe document): Σ al diferențelor clasificate pe documente |
| E2E-SC-SAFTS | privat | 2041-02 | Stoc | lot E2E-SC-SAFTS-ASM3 · 05.01.2041 · 3.3300 #0f78 gest MAG1 | 302 1.00/3.33 → 1.00/3.33 | 302 1.00/3.34 → 1.00/3.34 | poziția pe sursă (cub − registru pe document): Σ al diferențelor clasificate pe documente |
| E2E-SC-SAFTS | privat | 2041-02 | Stoc | lot E2E-SC-SAFTS-P11 · 05.01.2041 · 25.0000 #bd1d gest MAG1 | 302 0.00/0.00 → 3.00/75.00 | 302 4.00/100.00 → 3.00/75.00 | poziția pe sursă (cub − registru pe document): Σ al diferențelor clasificate pe documente |
| E2E-SC-SAFTS | privat | 2041-02 | Stoc | lot E2E-SC-SAFTS-P13 · 05.01.2041 · 25.0000 #ca4e gest MAG1 | 302 0.00/0.00 → 5.00/125.00 | 302 4.00/100.00 → 5.00/125.00 | poziția pe sursă (cub − registru pe document): Σ al diferențelor clasificate pe documente |
| E2E-SC-SAFTS | privat | 2041-02 | Diagnostic | avertisment SoldPeTipStocNeraportat | 1 | absent | S3-D1: categoria pe cont; soldul nestoc nu e avertisment (F27-r10) |
| E2E-SC-SAFTS | privat | 2041-02 | Diagnostic | avertisment DataPostariiInAfaraPerioadei | 1 | absent | S1-R: datele din tranzacția cubului, nu din DataOperare |
