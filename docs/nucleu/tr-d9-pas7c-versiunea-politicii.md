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

1. operarea reține regula câștigătoare și contorul ei: ipotezele `VersiunePolitica` ale BCS-ului sunt exact
   regulile de contare câștigătoare, cu `OptimisticLockField` al rândului; fiecare `ContRezolvat` al liniilor
   numește regula;
2. după editarea regulii prin ușa gardianului (`GardianEditare.Verifica` înaintea commit-ului), contorul
   rândului crește; o operare nouă reține contorul nou; explicația veche rămâne neschimbată (JSON identic) și
   e arătată ca „schimbată", cea nouă ca neschimbată;
3. o regulă a clientului ștearsă apare ca „schimbată", fără contor curent;
4. tipul fără reguli (NTC) dă lista vidă;
5. forma persistată e versiunea 2; versiunea 1 e refuzată de cititor.

Constatare de harness: providerul ModelCheck nu avea `EFCoreOptimisticLockInterceptor` (lista „ce NU s-a adus
din AddEFCore"); fără el contorul nu crește în harness, deși crește pe hosturi. Interceptorul intră în
providerul principal al suitei, ca proba 2 să măsoare ce face hostul.

## 4. Rezultate

(se completează după execuție)
