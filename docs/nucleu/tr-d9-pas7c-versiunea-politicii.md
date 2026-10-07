# TR-D9a, pasul 7c — versiunea politicii în explicație: regula care a decis (D9-A8)

Data: 2026-10-07. Execuție: main, direct. Contractul: amendamentul 1, D9-A8; D9-D15 rândul 7c; D9-D1
schimbarea 7; regula de oprire 11. Nivelul ales de owner: 1 (regula care a decis și contorul ei; nu conținutul
ei la operare, nu nomenclatoarele care decid — D9-r3).

## 1. Lista nominală a faptelor de politică, scrisă înaintea codului

Domeniul: tabelele `Politica` consumate prin operand (`Declaratii/Operand.cs`, `Motor/Fapte.cs`,
`Cub/ReceptiiConexe.Completeaza`, `Motor/ImobilizariFapte.Completeaza`). Verdictul pe fiecare fapt: **reținut**
(declarantul a luat din rând un fapt folosit la o postare) sau **nu decide postări**. Contorul: `Editabila.
OptimisticLockField`, incrementat la fiecare salvare a rândului de `EFCoreOptimisticLockInterceptor`
(înregistrat de `AddSecuredEFCore` pe ambele hosturi; ModelCheck îl primește în providerul lui, vezi §3).

| Declarant | Rândul de politică | Faptul luat | Verdict |
|---|---|---|---|
| BCS, DSC, PLT/INC (`DeclarantTrezorerie`), NIR, LDI, FCT, FCL, RDC, RLF, DEC | `RegulaContare`, câștigătoarea liniei (`Potrivire.Contare` prin `Contari.Rezolva`) | contul debitor, contul creditor, dimensiunile comune și pe laturi | reținut, o dată per regulă distinctă; `ContRezolvat.Regula` o numește pe fiecare cont |
| LDI | `RegulaStoc`, singura potrivită (`Potrivire.Stoc`) | latura, semnul și felul stocului pe gestiunea inventariată | reținut |
| FCT, FCL, RDC, RLF, DEC, DVI | `PoliticaTva` a tipului | direcția (sensul fiscal al faptelor), sursa și contul de rezervă al contrapartidei terțului (recepția facturii, impozitul), toleranța taxei | reținut când declarația are cel puțin un fapt fiscal sau contrapartida vine din ea; altfel nu |
| NIR conex cu diferență | `PoliticaDiferenta` (tip × cauză × clasă) | contul diferenței și contul personal | reținut |
| PIF, AMO, CAS (`DeclarantImobilizari`) | `PoliticaAmortizare` a tipului fișei (`ImobilizariFapte`) | conturile fișei: amortizare (când cubul nu-l are încă), cheltuială cu amortizarea, cheltuială cu cedarea | reținut pe fiecare fișă atinsă care are rând; proveniența per cont (cub sau politică) nu se urmărește — limită consemnată |
| toate | `TipDocument` (e `Politica`) | codul și identitatea tipului | nu decide postări: identitate, nu regulă |
| FCT, FCL, RDC, RLF, DEC, DVI | `TipTva` | cota, regimul, conturile de TVA | nu intră: nomenclator; rezultatul e în `CodTva` al postării (D9-A8, „ce nu intră") |
| ASM, BTR | — (contul din `TipMaterial`, nomenclator) | — | fără ipoteză |
| NTC, inclusiv nota închiderii de TVA | `PoliticaInchidereTva` | conturile închiderii | nu decide postările declarației: le consumă `InchidereTvaService.Genereaza` la generare, devin linii culese cu conturi explicite |
| NIR conex | `PoliticaConex` | tipul țintă, inversarea laturilor, filtrul naturii | nu decide postări: `MotorOperare.GenereazaConex` decide existența documentului conex, nu postările lui |
| PIF, AMO | `RegulaDeductibilitate` | limita fiscală | nu prin operand: `AmortizareService` o consumă la generare, valoarea fiscală e culeasă pe linie |
| toate | convenția de rotunjire (`SetareProfil`, `Scara.ConventieBani`) | rotunjirea banilor | nu se reține: dată înghețată a bazei (51c), nu rând editabil |
| culegere | `PoliticaTvaImplicit`, `PoliticaScadenta`, `PoliticaNumerotare`, `PoliticaValidare` | implicitele și validările culegerii | nu decid postări (L3) |
| citiri | `MapareD300`, `MapareD394`, `PoliticaMiscareSaft`, `MapareTvaSaft`, `PoliticaInchidere` | declarațiile, SAF-T, închiderea perioadei | nu decid postări |
| seed | `RefuzSeed` | — | nu |

Un declarant care nu consumă niciun rând reținut nu scrie ipoteza (ASM, BTR, NTC); declarația goală
(`PermiteDeclaratieFaraMiscari`) rămâne numai cu perioada deschisă.

## 2. Forma

- **Nucleu.** `VersiunePolitica(string Fel, Guid Rand, int Versiune)`: felul politicii (numele clasei rândului),
  identificatorul rândului, contorul lui la operare; una pe rând consumat, fără duplicate. Textul fix
  `("seed", data)` dispare. `ContRezolvat(Linie, Cont, Sursa, Guid? Regula)`: identificatorul regulii de contare
  când contul vine dintr-o regulă.
- **Operand.** Faptele poartă contorul: `RegulaContareFapt.Versiune`, `RegulaStocFapt.Versiune`,
  `PoliticaTvaFapt.Id/Versiune`, `PoliticaDiferentaFapt.Id/Versiune`, `FisaFapt.Politica` (ipoteza rândului de
  amortizare sau null). `Operand.VersiunePolitica` dispare.
- **Declaranți.** `PoliticiConsumate` (`Declaratii/PoliticiConsumate.cs`): ipoteza unui fapt și adunarea fără
  dubluri, în ordinea primei consumări; `Contari.Decide` scrie cele două `ContRezolvat` cu regula și o reține.
  Fiecare declarant adună ce consumă și pune ipotezele în locul în care punea textul fix.
- **Explicația.** Versiunea 2: `VersiunePolitica` cu `fel`, `rand`, `versiune`; `ContRezolvat` cu `regula` când
  există. Fără cititor pentru versiunea 1 (bazele sunt recreate la pasul 7; cele de dezvoltare la 7b).
- **API.** `ExplicatieContractDto` pierde `Politica` și `PoliticaValabilaDeLa`; primește `Politici`:
  `{ Fel, RandId, Versiune, VersiuneCurenta, Schimbata }` — `Schimbata` = contorul curent diferă sau rândul a
  dispărut (`VersiuneCurenta` null). `ExplicatieContDto` primește `RegulaId`. Contorul curent se citește prin
  `Cub/Citiri/VersiuniPolitica.Contoare(os, …)`: tipul rândului după numele clasei de `Politica`,
  `GetObjectByKey`, `OptimisticLockField`.
- **Client.** Tipurile generate se regenerează; nicio pagină React nu citește câmpurile scoase (104d).

## 3. Probele (D9-A8)

Scena SC-CIT-111 (în `ScenariiExplicatii`, ambele profiluri):

1. operarea reține regula câștigătoare și contorul ei: singura ipoteză `VersiunePolitica` a BCS-ului e regula de
   contare câștigătoare, cu `OptimisticLockField` al rândului; ambele `ContRezolvat` ale liniei o numesc; DTO-ul
   o arată neschimbată;
2. editarea regulii prin ușa gardianului (`GardianEditare.Verifica` înaintea commit-ului) crește contorul cu 1
   și timbrează rândul ca al clientului; o operare nouă reține contorul nou; explicația veche rămâne neschimbată
   (JSON identic) și e arătată „schimbată", cea nouă nu;
3. regula clientului ștearsă apare ca „schimbată", fără contor curent;
4. tipul fără reguli (BTR) nu scrie ipoteza: lista vidă, conturile fără regulă;
5. forma persistată e versiunea 2; versiunea 1 e refuzată de cititor.

Pe host viu (`nou/tools/ProbeHttp/explicatii.py`): aceeași regulă editată de două ori pe ușa OData
(`PATCH api/odata/RegulaContare(id)`, dus-întors) are contorul curent +2, iar explicația bonului operat înainte o
arată „schimbată".

Abaterea de la schița de mai sus, declarată: proba nu creează o regulă a clientului pe lângă cea din seed.
Regula BCS din seed e deja pe `TipMaterial` exact; o copie ar fi dublură (indexul unic
`IX_ReguliContare_TipDocumentId_TipMaterialId_NaturaFiltru_Semn~`) și n-ar câștiga (primul din listă ia nivelul).
Proba editează și șterge chiar rândul din seed prin ușa gardianului — care îl face al clientului — și îl reface
identic într-un `finally`, pe ușa de sistem. Câmpul editat e `PastreazaSemn`, pe care nu-l citește niciun
declarant (constatarea 3 de mai jos), deci postările nu se schimbă.

## 4. Rezultate

| Probă | Rezultat |
|---|---|
| Nucleu | 190/190 |
| `--probe-sursa` | 11/11 |
| `--dump-metadata` | fără diferență (DTO-urile nu sunt în metadată) |
| `gen:openapi` + `gen:types` | diferența declarată: `ExplicatieContractDto` pierde `Politica` și `PoliticaValabilaDeLa`, primește `Politici`; `ExplicatieContDto.RegulaId`; schema nouă `ExplicatiePoliticaDto`. A doua regenerare e stabilă; `tsc -b` verde |
| Integrala privat | 4.803 OK / 0 FAIL (`.P7b`) |
| Integrala bugetar | 3.524 OK / 0 FAIL (`.P7b`) |
| Diferența față de 7b (`compara.py`) | numai adaos: +8 pe fiecare profil (aserțiile SC-CIT-111); zero dispărute |
| A/B pe aceeași bază | fără interceptorul de blocare în providerul harness-ului, aserțiile 2 pică (contorul nu crește); cu el trec |
| HTTP pe host viu (`run-verificari/d9-pas8-http.ps1`, baza nouă `.Privat.D9P8`) | `refuzuri.ps1` 318/318 de două ori la rând; `neexpunere-cub.py` 0 FAIL; `explicatii.py` 9 PASS, cu SC-CIT-111 |

Log-urile: `run-nucleu/tr-d9a/pas7b/7c-*`, `run-verificari/d9-pas8-http/` (gitignored).

## 5. Constatări

1. **Contorul și ușile de scriere** (condiția de oprire din D9-A8). Contorul se citește în proiecția faptelor
   (`OptimisticLockField` în `Select`) și crește pe ușile hosturilor: `AddSecuredEFCore` înregistrează
   `EFCoreOptimisticLockInterceptor` pe context, deci pe XAF Blazor, pe OData/REST și pe updater-ul de seed.
   Providerele standalone nu-l aveau: ModelCheck (lista „ce nu s-a adus din `AddEFCore`") și Import1C, care
   rulează `ContaSeeder.Seed`. Ambele îl primesc acum explicit. Import1C rămâne probat numai prin compilare.
   Nu e oprire: nicio ușă a produsului nu ocolește contorul.
2. **Ce nu vede contorul.** O scriere SQL directă pe rândul de politică și restaurarea unei baze nu-l ating.
   Un rând șters și recreat are alt identificator, deci apare ca „dispărut". Limită consemnată.
3. **`RegulaContare.PastreazaSemn` nu mai are consumator.** După tăiere, câmpul ajunge în `RegulaContareFapt`
   și în „Explică", dar niciun declarant nu-l citește (RDC și RLF culeg semnul pe linie). E politică editabilă
   fără efect, de felul celei refuzate de D9-A4 la nivel de regulă. Nedecis: se scoate sau se leagă; se trece
   în decizia 110 și în restanțe.
4. **Proveniența conturilor fișei.** `FisaFapt` ia contul de amortizare din cub când fișa îl are deja și din
   `PoliticaAmortizare` altfel; ipoteza reține rândul politicii pe orice fișă atinsă care îl are, fără să spună
   care cont a venit de unde. Reține mai mult decât a decis, niciodată mai puțin.
5. **Contrapartida recepției facturii** vine din `PoliticaTva` și se reține și pe factura fără fapt fiscal;
   când politica nu dă cont, o dă regula de contare de rezervă, numită pe `ContRezolvat`.
