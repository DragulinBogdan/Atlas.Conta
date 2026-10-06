# TR-D9a — pasul 6: tăierea scriitorilor vechi și a regimului dual

- Data: 2026-10-07
- Bază: `tr-d9-taierea` la `29c15a6`; contractul
  [`tr-d9-taierea-contract.md`](tr-d9-taierea-contract.md), D9-D15, rândul
  pasului 6, cu amendamentele 1 și 2.
- Ce conține: ce a ieșit din produs, ce a intrat în locul lui, contabilitatea
  probelor (D9-D7 e), ce rămâne pașilor 6b și 7, ce e al owner-ului.
- Schimbările de comportament ale pasului sunt 1, 2, 4 și 6 din D9-D1. Nimic
  altceva nu și-a schimbat cifra: fiecare aserție dispărută are verdict nominal
  în `run-nucleu/tr-d9a/pas6/lucru.tsv`.

## 1. Produsul

**Coaja comenzii** (`Motor/MotorOperare.cs`), în ordinea D9-D2: gardurile de
stare, dată și perioadă; refuzul tipului care nu declară (`TIP_FARA_DECLARATIE`,
D9-D5); `PregatesteOperare`; validările care nu țin de postări (TVA cules la
taxare inversă, `PoliticaValidare`, `ValideazaOperare`); contractul și
gardurile de pe postările lui (`Materializare.Refuzuri`); scrierile pe
document; `Materializare.Opereaza`, care întoarce contractul; valoarea
liniilor evaluate din sold; împerecherea automată. Au dispărut `PlanOperare`,
`PotrivesteReguliStoc`, pasul de note, pasul TVA, `VerificaDimensiuniObligatorii`
(regula flag-urilor stă acum în `Cub/GardAnaliza.VerificaLatura`), cele trei
scrieri de registru din `Opereaza`, `AnuleazaOperarea` și `Storneaza`,
scrierile `TotalStingere`. `Valideaza` întoarce numai refuzuri.

**Valoarea liniei** (D9-D3 a): `ScrieValorileEvaluate` pune pe fiecare linie cu
decizie `ValoareIesire` suma deciziilor ei, cu semnul cantității liniei. Nu
există `Abs`. Invariantul `CITIRE_EXPLICATIE_LINIE` (`Cub/Citiri/Explicatii`)
cere, pe toată baza, linia = decizia; prima egalitate (postarea normalizată pe
latură = decizia) exista deja (`CITIRE_EXPLICATIE_IESIRE`).

**ASM** (D9-D3): `DeclarantAsamblare.Consum` evaluează consumurile pe soldul
cubului în secvența liniilor și e chemat atât de `Declara`, înaintea gardului
ΣP = ΣC (`ASAMBLARE_NEBALANSATA`, „Consum C=…; produse P=…; diferență
P−C=…"), cât și de `AsamblareApply.DistribuieValoarea`, pe operandul
draftului, într-un ObjectSpace de predicție. Grupurile pe cont cu P_g = C_g
intră în `Transfer`, restul în `Operare`. Au dispărut decizia
`AbsorbtieEvaluare` din nucleu (cu serializarea ei și câmpul `Absorbtii` al
DTO-ului explicației), `ASAMBLARE_DELTA_FARA_ANCORA`, `Fapte.SolduriLoturiRegistru`,
`Operand.SolduriLoturiRegistru`, `IDeclarant.CereSoldRegistruPentruEvaluare` și
invariantul valoric din `Asamblare.ValideazaOperare`.

**Tipul care nu declară** (D9-D5): `IDeclarant.PoliticaCeruta` numește politica
de profil fără de care tipul nu declară: `Contare` pe DSC, RDC, RLF; `Tva` pe
DVI; `InchidereTva` pe ITV (a doua instanță a `DeclarantNotaContabila`).
`Materializare.MotivNuDeclara` o verifică, împreună cu lipsa declarantului
(BPR), înaintea validării frunzei; refuzul e același la dry-run și la operare,
cu și fără număr cules. `PosteazaInCub` nu mai e citit de nimic (coloana și
seed-ul ies la pasul 7); au dispărut `Materializare.AreTranzactii`,
`POSTEAZA_IN_CUB_IREVERSIBIL`, `NIR_REGIM_INACTIV` și refuzul împerecherii
„ambele documente trebuie să posteze în cub".

**Regula de contare fără consumator** (D9-A4): `IDeclarant.ConteazaPrinReguli`
(adevărat pe BCS, DSC, FCL, FCT, LDI, NIR, RDC, RLF, PLT/INC, DEC);
`GardianEditare.VerificaRegulaContare` refuză cu `REGULA_CONTARE_FARA_CONSUMATOR`
crearea sau modificarea unei reguli pe un tip al cărui declarant nu contează
prin reguli ori nu declară, prin `Contractare.DeclarantulTipului`; ștergerea
nu e refuzată.

**Inversa fiscală născută finală** (D9-A5): `CorectieService` citește atribuirea
corecției înaintea stornării și o dă lui `MotorOperare.Storneaza`;
`Materializare.CuInversaFiscala` o pune pe postările fiscale ale inversei la
construcția lor. `ReatribuieInversaFiscala` a dispărut. Proba e în produs:
`BackOfficeEFCoreDbContext.SaveChanges` refuză orice `Postare` în starea
`Modified` (`POSTARE_MODIFICATA`), pe orice ușă. Mutanții ModelCheck care
modificau postări prin ObjectSpace au trecut pe `UPDATE` SQL în tranzacție
anulată (SC-CIT-80); așa s-a și dovedit garda.

**Au dispărut**: `StocService` întreg (cu `CheieStoc`, `MiscareStoc`,
`SoldStoc`, `ValoareGolire`, `VerificaGoliri`, `AlocaFifo*`),
`RegistruTvaService` întreg, `IDocumentCuIesireFiscala` (mecanismul e
`IDeclarant.SursaValoareDeclarata`), acoperirile registru → cub din
`Cub/Citiri/Invarianti` (`CITIRE_ISTORIC_INCOMPLET`,
`CITIRE_ISTORIC_STOC_INCOMPLET`, `CITIRE_DESCHIDERE_INCOMPLETA`, registrul
imobilizărilor), `ImperecheriProiectii.VerificaAcoperire`
(`CITIRE_PARTIDE_POLITICA`, I8, pinul pasului 4), jumătatea de registru a
refuzului „lotul de deschidere are deja mișcări". `IDocumentCuRegistruPropriu`
e `IDocumentCuEfecteProprii` (`LaOperare`, `LaAnulare`, `LaStornare`, cele două
motive): starea fișei și refuzurile ei, fără `RegistruImobilizari`;
`Imobilizari.VerificaAcoperire` e `VerificaProvenienta`.

**Rămân, ca declarații, până la pasul 7**: entitățile și maparea celor patru
registre, `TipDocument.PosteazaInCub`, `TipDocument.LaturaContPropriu`,
`Document.TotalStingere` (cu cazurile gardianului pe ele), listele XAF vechi,
rândurile `RegulaStoc` fără consumator, purjele de registre din ModelCheck.

**Contractul HTTP**: a ieșit `ExplicatieAbsorbtieDto` și câmpul `Absorbtii`
al liniei explicate; `openapi.json` și `api-types.ts` regenerate,
`verifica:drift` zero. Clientul nu citea câmpul.

## 2. ModelCheck

Oracolul a ieșit: `Nucleu/CubDinRegistre`, `Normalizari`, `Comparabil`,
`ReconciliereCub`, `GatePeBaza`, `DiagnosticValoriStoc`, modurile
`--reconciliere-cub` și `--declaratie-pe-baza`, `STR-RECONCILIERE`,
`ProbeNucleu.Proba` și `ProbeCub.ProbaOperare` fără comparația cu oracolul,
`ProbeCub.ProbaTransferPliat`, comutatoarele `Migrat` / `Nemigrat`, mutanții
acoperirii registru → cub și ai politicii de partide. Mutanți noi în `INV-CUB`
(D9-D3): `LINIE-BCS`, `LINIE-BTR`, `LINIE-ASM` (linia cu altă valoare),
`LINIE-SEMN-BCS`, `LINIE-SEMN-ASM` (linia cu semn opus), `IESIRE-SEMN-BTR`,
`IESIRE-SEMN-ASM` (decizia cu semn opus pe un `Transfer`); toți uciși pe
ambele profiluri.

Lista nominală X-D2 (`ProbeCititoriRegistre.Lista.cs`): 40 de intrări —
mapare 6, gardian 1, evidență XAF 2, autorizare 1 (`Imperechere`), legătură
30; clasele scriitor dual și martor au ieșit din enum.

Partea mecanică a curățeniei a făcut-o un agent pe spec
(`run-nucleu/tr-d9a/pas6/spec.md`, `raport.md`); verdictele pe constatările
lui și verificarea sunt ale main-ului (§4).

### Contabilitatea probelor (D9-D7 e)

| | Linia de bază (`29c15a6`, dual) | Pasul 6 | Diferența |
|---|---|---|---|
| ModelCheck privat, linii `OK` | 4.862 | 4.763 | −99 |
| ModelCheck bugetar, linii `OK` | 3.554 | 3.481 | −73 |
| `FAIL` | 0 | 0 | |
| nucleu | 180 | 180 | |
| `--probe-sursa` | 10 | 10 | |
| linii de aserție în ModelCheck (idiomuri) | 2.542 | 2.453 | −89 |

Niciun nume dispărut fără rând în `lucru.tsv` (`scripts/dispar.py`:
neclasificate 0 pe ambele profiluri). Pe verdict, în afara rescrierilor ASM
ale main-ului: oracol 59 șterse și 4 re-țintite; formă 29 șterse, 15 clauze
șterse, 5 re-țintite; repartitor 17 șterse; comportament 34 re-țintite și
43 cu nume schimbat (cifra din nume); regulă 7 re-țintite; montaj 12 mutate
pe cub, 1 re-țintit, 2 fără obiect; `MĂSURAT` 13 mutate pe cub, 2 șterse.
`oprita-cifra`: niciuna. Două probe noi: `D9-A5 (a)` și `D9-A5 (b)`.

Cifrele de după, scrise înaintea codului, au ieșit toate la fel: SC-BCS-15/16,
SC-ASM-17…25, SC-RLF-12/14, SC-RDC-16/19, SC-DSC-09, SC-DVI-12, SC-X-25,
SC-X-26, SC-X-27, cele patru aserții din `tr-d9-pas2-probe.md` §4 și cele
șase din `tr-d9-pas3-retintire.md` §3. Catalogul poartă forma de după tăiere
ca stare curentă.

### Montajele (pasul 3, §4)

`VerificaNucleuBcs`, `VerificaNucleuBtr`, `VerificaNucleuFclDsc` și celelalte
nouă scene care scriau rânduri de registru de mână rămân pe deschiderea
cubului pe care o scriau deja; `VerificaLaturi` primește o deschidere nouă
(`DeschidereScena.Scrie`, 10 / 100). `NUC-BCS-N-R3-1` e pe soldul cubului.
Fără obiect: rândul `RegistruTva` injectat prin SQL în `VerificaD394` (D4-V7:
declarantul nu postează taxă pe taxare inversă × colectat) și ramura cu rând
de registru din `LotCuDeschidere`.

### Cele 17 aserții ale repartitorului (pentru pasul 6b)

Șterse aici, fiindcă registrul pe care îl citeau nu mai există; 8 au geamăn
` [cub]` pe cifră. Lista nominală, cu clauza de repartitor a fiecăreia, e în
`run-nucleu/tr-d9a/pas6/raport.md`, „Cele 17 aserții REP"; se rescriu pe
cititori la 6b, pe convenția D9-A10.

## 3. Limite consemnate

- Ștergerea ambelor postări ale unei linii de notă contabilă lasă tranzacția
  echilibrată și nu mai e văzută de niciun invariant (SC-CIT-34): acoperirea pe
  linie era a registrelor. Intră în decizia 110.
- `POSTARE_MODIFICATA` judecă starea entității, nu un `UPDATE` direct în SQL.
- Refuzul `TIP_FARA_DECLARATIE` nu intră în regimul pe stare al documentului:
  butonul de operare rămâne activ pe un tip inert.
- O cădere în scena F27 lasă în baza privată perioade închise și snapshot-uri
  pe care integrala următoare nu le curăță (PER/SOL rulează înainte, pe
  precondiții globale); curățarea e prin funcțiile celor două scene.

## 4. Verificarea main-ului

Re-rulate independent pe binarul recompilat din arbore: integrala privată
(4.763 / 0), bugetară (3.481 / 0), nucleul (180/180), `--probe-sursa` (10/10),
`pnpm gen:openapi && pnpm gen:types` (driftul = numai `Absorbtii`),
`tsc --noEmit` fără erori. Verdictele pe constatările agentului: testul de
determinism al nucleului cerea 9 cazuri sigilate (corectat la 8, de main);
`NUC-FCT-P4-2` fusese pin-uit greșit „mort cu oracolul" și a revenit re-țintit
pe faptele fiscale ale cubului, cu aceleași cifre; proba regulii de contare
valide (F23-V4) își calcula așteptarea din funcția de sub probă și a primit
fixture fix pe BCS.

## 5. Ce rămâne și cine îl ia

- **6b** (main): repartitorul pe piciorul de terț, gardul strict, cele 17
  aserții pe cititori (D9-A10, 111 h).
- **7**: declarațiile, `InitialCreate`, seed-ul fără rânduri `RegulaStoc`
  moarte, listele XAF vechi, proba numelor interzise, driftul declarat.
- **Owner**: B-r3 se rejudecă aici (D9-D13). Motivul amânării a dispărut
  (planul vechi nu mai postează recepția de două ori), dar NIR-ul conex rămâne
  documentul diferențelor (098, 099), iar o regulă proprie `FCT/Stoc` e
  schimbare de politică, nu a tăierii. Propunere: rămâne activă, cu destinația
  hotărâtă de owner la decizia 110.
