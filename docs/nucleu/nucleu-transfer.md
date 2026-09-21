# Transferul: ce se mută din implementarea curentă în nucleu și în ce ordine (pasul 3 din §11)

Stare: **PROBAT (2026-09-19), cu review advers aplicat**; TR-D5 (decizia 090),
TR-D6a (felia 29), TR-D6b (felia 30, pilotul BCS/PLT/FCT — forma ține,
`tr-d6b-declaratia-fluxului-contract.md`) și TR-D7a (felia 31, cubul
PERSISTAT pe BCS/FCT/PLT/INC, `tr-d7a-strangler-contract.md`, închisă
2026-09-21) ÎNCHISE; următorul = TR-D7b, tipurile rămase pe cub (agent separat, 6
MAJOR / 9 MEDIU / 5 MINOR; toate acceptate — unde au schimbat o concluzie, e
spus; raportul și SQL-ul lui în `run-nucleu/transfer/review/`). TR-D5 = decizia 090
(2026-09-20, `docs/decizii/090-nucleu-cub-de-postari.md`), care preia §1–§4.1
ca regulă durabilă; propunerea de execuție (§4.1) e ACCEPTATĂ 2026-09-20;
TR-D6a ÎNCHISĂ 2026-09-20 (`tr-d6a-nucleu-pur-contract.md`). Întrebarea din `nucleu-cub-design.md` §11: ce se
transferă din implementarea curentă (documente tipate, politici, gardieni,
ModelCheck, Import1C ca probă supremă), în ce ordine, cu contract și regulă
de oprire — și tranșările de modelare amânate de pașii 1–2: ce e o partidă,
împerecherea, stocul unificat cu 3xx și `TipStoc`, linia FCT contra NIR.
Probele: trei inventare cu `fișier:linie` și SQL pe clona
`Atlas.Conta.Nucleu.Fizica.x1` — `run-nucleu/transfer/01-inventar-implementare.md`
(motor, hook-uri, frunze, politici, gardieni, registre, ModelCheck, Import1C,
uși), `02-partida-imperechere.md` (+ `02-sql/`, `02-out/`),
`03-stoc-sinkuri.md` (+ `03-sql/`, `03-out/`). Nimic de aici nu se
re-derivă; se citează.

## 1. Rezultatul, în cinci propoziții

1. **Partida = unitate nominalizată, deschisă de o postare pe un cont de
   terț**; atributele ei: `Cont`, `Partener`, documentul deschizător (sau
   deschiderea); restul = `Σ[Unitate]`. Un document cu două conturi de terț
   are două partide, fiindcă un singur număr per document nu e sumă pe
   niciun cont (2.036 FCL cu 419, 25 FCT cu 401 + 408/4091 — `02-…` §B4).
2. **Împerecherea = nominalizarea partidei pe postarea de terț a
   stingătorului**, la operare (geamăna FIFO-ului pe lot); împerecherea
   ulterioară și desfacerea = document tipat `Împerechere` cu tranzacție de
   fel `Transfer`. `Imperecheri`, `TotalStingere`, `PartideDeschise` și cele
   patru hook-uri de stingere dispar.
3. **Linia de stoc a facturii de intrare postează ea recepția** dacă nu
   numește un lot deja recepționat (atunci `408 = 401`); NIR-ul autogenerat
   din FCT dispare (clonă 1:1 fără nicio informație proprie — `03-…` §A5,
   §B5). **Postarea de stoc și postarea contabilă pe contul de stoc sunt
   UNA** (1:1 cu valoare egală pe 100 % din perechi — §B2); `TipStoc`
   dispare, distincția devine structurală (cont sau gestiune virtuală).
4. **Tranzacția de fel `Transfer`** (între unități sau gestiuni ale ACELUIAȘI
   cont: împerechere, transfer între gestiuni pe același cont) are
   `Σ per (Cont, Latura) = 0` și e EXCLUSĂ din rapoartele pe cont (balanță,
   fișă, jurnal, GLE) și inclusă în cele pe unitate/gestiune — altfel
   fișa 4111 crește cu 38 % și jurnalul cu 21,7 % (review M4).
5. **Ordinea**: nucleul pur → declarația fluxului per tip (pilot) →
   motorul nou scrie cubul LÂNGĂ registre, per tip de document ca DATĂ
   (strangler incremental, Import1C ca gate) → citirile pe cub → unitățile
   → tăierea. Contractul IM (`docs/api/p5-felia-izolare-motor-contract.md`)
   e DEPĂȘIT de această ordine; ce avea valabil intră în pașii 1–2.

## 2. Tranșările de modelare (TR-D1…D4)

### TR-D1 Partida

Azi partida e DOCUMENTUL: `TotalStingere` = Σ brut al liniilor de creanță,
scris pe ORICE document operat (`MotorOperare.cs:396-397`), împerecherea e
legătură m2m între documente (`Trezorerie.cs:654-676`), restul e
`TotalStingere − Σ Asignat`. Consecințele, cu cifre (`02-…` §B4, §B6):

- 87.478 de „partide” F0 (515,7 M) stau pe documente care nu postează NIMIC
  pe un cont de terț (NotaTransfer 45.542, DescarcareGestiune 36.689…): rest
  fără datornic.
- FCL cu linie de avans pe 419: `TotalStingere` 104,02, dar soldul pe 4111 e
  104,02 D și pe 419 e 87,41 C — două conturi, o singură „partidă”; cifra
  nu e sumă pe niciun cont. 2.036 de asemenea FCL, 25 FCT cu 401 + 408/4091.
- FCT: datoria e ruptă pe două documente (NIR creditează 401 cu netul, FCT
  doar cu TVA-ul), iar împerecherea stă pe FCT ⇒ 14.341 de FCT cu rest
  NEGATIV pe cub (−45,5 M); 1.085 FCT cu taxare inversă n-au NICIO postare de
  terț, deci n-au unde primi cele 1.179 de stingeri (25,6 M).

Cele trei definiții candidate, la 31.12.2025 (`02-sql/08…11`;
`review/02-tautologie-B.sql`):

| definiție | partide cu rest ≠ 0 | împerecheri reprezentabile sub modelul de azi | diferă de celelalte pe |
|---|---|---|---|
| A = documentul | 63.336 | 26.767 (60 %) | documentele cu 2+ conturi de terț (2.061) |
| **B = document × cont de terț** | 66.658 | 26.767 (60 %) | — |
| C = lanțul conex FCT+NIR | 33.032 | 41.838 (94 %) | doar ruptura FCT/NIR |

Review-ul a arătat că „B e singura care se închide pe soldul de terț” era
artefact de `JOIN` (A cu `FULL OUTER JOIN` închide identic, cu aceleași
1.085 de partide inventate pe conturi pe care documentul n-a postat
niciodată, −25.585.375,12): coloana a fost scoasă. Argumentul lui B e cel
structural: restul trebuie să fie sumă pe un cont; C repară artificial
ruptura FCT/NIR, pe care TR-D3 o desființează — după TR-D3, A și B diferă
DOAR pe documentele cu două conturi de terț, unde A nu e sumă.

**Regula**: o partidă se deschide când o postare pe un cont cu `RolTert` nu
numește o partidă existentă; unitatea are identitate proprie și atributele
`(Cont, Partener, document deschizător | deschidere)`; restul e `Σ Valoare`
pe unitate, sensul = semnul restului × rolul contului; un document cu două
conturi de terț are două partide (FCL cu 419: 4111 și 419; regularizarea e o
notă care numește ambele unități). Documentele care nu postează pe terț
n-au partidă. `Document.TotalStingere`, `LiniiCreanta`, `PartideDeschise` și
`ImperecheriProiectii.cs:120-169` (cele șase ramuri de `Sens`/`Tip`) dispar;
„Documente cu rest” = `Σ[Data≤t; Unitate] ≠ 0` (pasul 1 §3) și își schimbă
declarat semantica față de azi.

**Unitatea se numește pe linie; partenerul se scrie din unitate** (review
M6). Nota contabilă nu poate purta partener (`NotaContabila.cs:90-92` cere
repartitori interni): pe Flax 7.824 de picioare de terț n-au partener, din
care 4.068 pe 4111 (12,5 M) și 1.872 din cele 2.068 de note de regularizare
a avansului pe 419 (8,25 M); doar 125 s-ar potrivi euristic
(`review/07-partener.sql`, `08-nota419.sql`). De aceea: orice linie care
postează pe un cont cu unitate (terț → partidă; stoc → lot; imobilizare →
fișă) poartă unitatea ca dată de culegere (`UnitateDebitId`/`UnitateCreditId`
pe liniile cu postare explicită, `LotId`/`PartidaId` pe celelalte), iar pe o
postare care numește o unitate `Partener` se SCRIE din unitate — excepția
declarată la amendamentul 4 al pasului 1 (partenerul din repartitor doar pe
postarea care DESCHIDE). Regula §9 a designului (nota nu atinge o coordonată
cu unitate fără s-o numească) devine gard de operare.

**Deschiderea soldurilor de terți** (review M2): pe Flax soldul real al
conturilor de terț la 31.12.2025 e 14.792.113,93, din care 9.690.314,66
(65,5 %) stă pe deschidere — rânduri fără document și fără repartitor
(`Deschidere.cs:104-115`, un rând bloc per cont contra 891); pe 4111, 87 %
din creanță. O partidă nu se poate deschide fără partener. **Regula**:
deschiderea de terți intră ca tranzacție de deschidere cu o unitate per
`(Cont, Partener)` — sau per factură deschisă, când sursa o dă —, cu
`Partener` pe postare; cerință pentru conectorul 1C (TR-r6), împreună cu
data reală a împerecherii. Fără ea, 65 % din solduri n-ar fi stingibile.

### TR-D2 Împerecherea

Faptele care resping propunerea din `nucleu-fizica.md` §4.4 („două postări
pe același `(Cont, Latura)`” pentru ORICE împerechere): 43.237 din 44.448 de
împerecheri leagă documente pe laturi OPUSE (factura debitează 4111,
încasarea îl creditează — `02-…` §B7.1); toate cele 44.448 au
`Autogenerat = false`, 24.540 de plăți își consumă plafonul integral, 22.045
sting o singură partidă, niciun document nu apare în ambele roluri (§B5);
nominalizarea pe cont nu cade nicăieri în afara taxării inverse (0 stinse
exclusiv pe conturi neatinse de stingător — `review/06b`). Cazul normal nu e
un transfer, e o POSTARE care numește partida.

**Regula**, în două forme ale aceleiași primitive (nominalizarea unității):

- **(a) La operare**: postarea de terț a stingătorului (plata, încasarea,
  factura care regularizează un avans) numește partida pe care o stinge —
  linia o alege; automat: FIFO pe partidele deschise ale aceluiași
  `(Cont, Partener)` când politica o cere (1.795 de împerecheri au un stins
  cu 2+ conturi de terț: contul e al liniei stingătorului, deci alegerea nu
  traversează conturi). Plafonul = restul partidei; suma peste rest deschide
  o partidă nouă pe stingător (plata în avans), cum NIR-ul deschide lot.
  Fără hook-uri: `SensDeStins`, `PoateFiStins`, `CapacitateStingere`,
  `SursaStingeriiAutomate` (`Document.cs:237-271`) devin structurale — un
  document poate fi stins dacă are partidă; 76-r3 se închide.
- **(b) Ulterior și la desfacere**: împerecherea a două partide deja
  deschise (încasare nealocată ↔ factură) și desfacerea unei nominalizări
  (a) sunt un DOCUMENT tipat `Împerechere` (linii = perechi
  `(partida sursă, partida țintă, sumă)`), operat prin motor, cu tranzacție
  de fel `Transfer`: două postări pe același `(Cont, Latura)` a postării
  originale, `−S` pe partida sursă, `+S` pe partida țintă. Document, nu
  legătură (review M4, E7): are gard de perioadă, dată (88j: ≥ înregistrarea
  ambelor partide), storno, `Cauza`; invariantul I și gate-ul „un document =
  o tranzacție” rămân intacte, iar nota `Imperechere` nu mai apare ca a
  doua tranzacție a stingătorului. Stornoul unui document a cărui partidă a
  fost nominalizată de alții se refuză până la desfacere (dependenți, ca
  azi la anulare); stornoul stingătorului inversează și nominalizarea, deci
  partida stinsă se redeschide.

**Fel = Transfer** (review M3, M4): tranzacție cu `Σ per (Cont, Latura) = 0`,
care mută o măsură între unități sau gestiuni ale aceluiași cont. Pe cubul
măsurat, cele 44.448 de împerecheri ar aduce 88.896 de postări, toate cu o
valoare negativă: fișa 4111 de la 144.241 la 199.177 de postări (+38 %),
401 +46 %, jurnalul +21,7 %. De aceea rapoartele pe CONT (balanță, fișă,
registru-jurnal, GLE, `TotalDebit`/`TotalCredit`) le EXCLUD prin fel;
rapoartele pe unitate/gestiune (fișa partidei, documente cu rest, fișa de
magazie, PhysicalStock) le includ. Rulajul brut per partidă nu e sumă
(TR-r8); `Valoare` negativă pe o latură apare doar în tranzacții de fel
`Storno` sau `Transfer`, deci nu se confundă cu stornoul în roșu.

Consecințe: tabela `Imperecheri` și `ImperechereService` (371 linii, din care
`AsignatFataDe` materializează polimorf documentele stinse —
`ImperechereService.cs:327-362`) dispar; regula (c) a gardianului
(`GardianEditare.cs:520-575`) se re-țintește pe documentul `Împerechere`;
invariantul I se amendează (împerecherea DEVINE operație — anunțat în
design §10). Cele 14 cazuri reale cu partener diferit între stins și
stingător sunt transferuri prin notă, cu ambele unități numite. Latura
pereche a viramentului rămâne ce e azi: legătura celor două picioare pe 581,
nu stingere (`Trezorerie.cs:171-197`).

### TR-D3 Linia FCT postează recepția; conexul FCT→NIR dispare

Faptele (`03-…` §A5, §B5; `02-…` §A3; `review/04`): NIR-ul conex e clonă 1:1
la document (17.814 FCT ↔ 17.814 NIR, niciunul manual, toate în aceeași ZI cu
factura) și la linie (34.289 de perechi prin `Lot.LinieIntrareId`, cantitate
și valoare IDENTICE); `NirDetaliu.PretUnitar` e 0 pe toate, prețul trăiește
pe linia FCT și pe lot; SAF-T citește prețul și cantitatea de pe FCT și
contul de pe NIR (`SaftProiectii.cs:1112-1164`). Împărțirea postării pe 401
nu e cod, e ABSENȚA unei reguli de contare pe FCT/Stoc (`Politici.cs:71-72`).

**Regula**: linia de natură `Stoc` a facturii de intrare postează recepția
(`3xx = 401` la net, `+Cantitate` pe lotul născut de linie) și taxa
(`4426 = 401` per linie) DACĂ lotul pe care îl numește n-are deja postare de
recepție; dacă linia numește un lot recepționat pe aviz (`NIR` manual,
`3xx = 408`), postează `408 = 401` (review E1: purtătorul legăturii e
`LotId` al liniei, care există azi; `LinieSursaId` nu e necesar). FCT
deschide o singură partidă pe 401 cu datoria integrală. `PoliticaConex`
pierde singurul rând de seed; mecanismul clonei (`MotorOperare.cs:534-573`)
dispare odată cu el. SAF-T (review E2): sursa `MovementOfGoods` și
`MovementReference` se mută de pe NIR pe FCT — declarat; cheia
`(FCT, 3xx, +1)` a politicii de mișcare e nouă și injectivitatea se
re-probează pe setul de după TR-D3 (gate TR-D7).

**Testul general al documentului-copil**: un document conex/secundar există
DOAR dacă poartă informație proprie (dată, gestiune, loturi alese,
instrument, cont propriu); clona fără informație proprie e postare a
sursei. Trec testul: DSC din FCL (loturi/pin, gestiune, dată, descărcare
parțială — `DescarcareService.cs:22-112`), plata din FCT
(`FacturaIntrare.cs:76-103`, instrument + cont propriu), latura pereche a
viramentului. Nu-l trece: NIR din FCT. `NIR` rămâne tip de document pentru
recepția fără factură (TR-r4 confirmă cerința de produs).

### TR-D4 Stocul: o singură postare, `TipStoc` dispare, sink-urile sunt gestiuni

Faptele (`03-…` §B2, §B3, §B4, §B7; `review/04` §R4.4): pe `DetaliuId`,
98.385 de perechi stoc ↔ contabil 1:1 cu `|Valoare|` EGALĂ pe toate
(diferență 0,00; contul implicit al produsului e pe notă în 100 % din
cazuri), 476 de 1:2 (RLF, a doua notă e TVA-ul), 1.471 de 2:1 (BCS: două
rânduri de stoc, o notă `6xx = 3xx`); fără pereche 181.695 (NotaTransfer
170.054 — contabil „nimic” la plan sintetic —, deschideri 6.947, Asamblare
4.694). `TipStoc` e derivabil din `(Cont, Repartitor)` cu ZERO coliziuni pe
cele 283.498 de rânduri private (Consum stă exclusiv pe `UnitateInterna`
SEDIU); cheia `(TipDocument, Cont, Semn)` a politicii de mișcare SAF-T e
injectivă (0 coliziuni, 39 de chei contra 18). Custodie/Folosință/Gratuit/
ProducțieNeterminată: 0 rânduri, 0 conturi 80xx atinse; bugetarul le
cheiază pe CLASA produsului (`ProfilBugetar.cs:335-336`, `:389`), fără
garanție de cont distinct (review E4).

**Regula**: postarea pe un cont de stoc poartă `Cantitate` și `Unitate` =
lotul; o linie de stoc = o postare. Convenția de semn: cantitatea iese din
gestiunea-sursă (−) și intră în gestiunea-țintă (+), inclusiv când ținta e
virtuală. Transferul între gestiuni pe ACELAȘI cont = tranzacție de fel
`Transfer` (`371 gestiune B +q / 371 gestiune A −q`, exclusă din rapoartele
pe cont); transferul care schimbă contul (Mărfuri → Magazie, `3028 = 371`)
e postare reală; consumul = `6xx (gestiune virtuală Consum, +q, lot) =
3xx (gestiune, −q, lot)` — postarea de cheltuială poartă cantitatea și lotul
(sink-ul din design §3); asamblarea = `345 = 301` cu loturile ambelor
laturi; custodia = cantitate pe 803x cu valoare zero. Distincția fostului
`TipStoc` devine STRUCTURALĂ, nu de date: Magazie/Mărfuri = contul (3xx
contra 371); Consum/Folosință/Gratuit/ProducțieNeterminată = gestiuni
virtuale (coordonata `Gestiune`); Custodie = contul 803x. `PoliticaMiscareSaft`
se re-cheiază pe `(TipDocument, Cont, Semn, Gestiune virtuală?)` și
injectivitatea ei e PROBĂ ModelCheck pe ambele seed-uri, nu restanță.
„Gestiunile de patrimoniu” ale PhysicalStock = gestiunile reale. `Spatiu`
(partiția fizică, FZ) se re-definește: **Stoc = postările cu unitate de tip
lot**, **Contabil = restul**; conservarea VALORII se verifică pe reuniunea
celor două, a cantității doar pe Stoc; FK-ul per partiție pe `Unitate` se
păstrează. Economia: 100.332 de postări (8,63 % din cub; 8,76 % cu sink-ul).

**Ce schimbă în balanță, declarat** (review M3, `review/03-stoc-contabil.sql`):
sub postarea unică, BTR între conturi diferite, ASM și deschiderea de stoc
devin postări pe 3xx pe care registrul contabil de azi NU le are: soldul
3xx se mută cu +585.404,66 (371 +404.030,06; 3028 +134.853,80; 303
+26.236,26; 3024 +17.539,80; 381 +2.744,74), din BTR Mărfuri → Magazie
(69.323,46 pe 3028, 13.874,18 pe 303), ASM (−88.712,43 net de pe 371) și
deschidere; rulajul brut al lui 371 ar crește cu +134,86 M (+148 %) DACĂ
transferurile pe același cont ar intra — nu intră (fel `Transfer`). Δ de
sold e o CONSTATARE: fie registrul de stoc are dreptate și contabilul de azi
e greșit cu 585 k pe 3xx, fie invers; reconcilierea cu 1C (contractul 1) o
tranșează la TR-D7 (TR-r12) — nu se acceptă tăcut. Notele contabile pe
conturi de stoc fără lot (`review/04` §R4.1: 2.997 de rânduri, Σ 260.652,89
— `891 = 371` 1.420 cu Σ 0, `607 = 371` 626, `401 = 371` 354, `3028 = 371`
345, `371 = 401` 180) sunt puntea de import prin 891 și corecții de valoare:
sub §9 devin postări de valoare pe lot (corecția de preț, design §5) sau
divergențe declarate ale conectorului, corespondență cu corespondență
(TR-r2); puntea 891 dispare odată cu deschiderea ca tranzacție.

## 3. Ce se transferă, ce se rescrie, ce dispare

Clasificarea, din `01-inventar-implementare.md` (cifrele de linii sunt ale
lui). Criteriul: **regula** (intrări plate → ieșiri) se transferă;
**mecanismul** formei de azi se rescrie o singură dată pe cub; ce exista
doar din cauza formei dispare.

### 3.1 Se transferă (cod sau date, cu punctele de atingere numite)

| ce | de unde | cum |
|---|---|---|
| Documentele tipate: cele 20 de frunze + liniile lor + culegerea (câmpuri, `DimensiuniCulese`, `ProdusCules`, `ILinieCareNasteLot`, `ILinieCuAtributeLot`, `ILinieCuPretUnitar`, `[GardContare]`) + tipul nou `Împerechere` | §C, §B.1 | neschimbate; liniile cu postare pe cont cu unitate capătă câmpul de unitate (TR-D1); hook-urile de MOTOR (`PregatesteOperare` 13, `ValideazaOperare` 17, `RepartitorImplicit*`, `GenereazaSecundar`, `LiniiCreanta`, `Total`, cele 4 de stingere, `MesajeDupaOperare`) se înlocuiesc cu declarația fluxului pe operand închis (TR-D6b) |
| Politicile ca date: `RegulaContare` (58), `RegulaStoc` (19 → semn + gestiune virtuală opțională; repartitorul rămâne al laturii), `PoliticaTva` (6), `PoliticaTvaImplicit` (11), `PoliticaInchidereTva`, `PoliticaNumerotare` (17), `PoliticaScadenta`, `PoliticaValidare`, `PoliticaAmortizare` (7), `RegulaDeductibilitate` (3), `PoliticaInchidere` (4), `MapareD300` (22), `MapareD394` (13), `PoliticaMiscareSaft` (21 → re-cheiată), `TipTva` (15), `SetareProfil` | §D | tabelele rămân; devin „rezolvarea coordonatelor” (design §3); `PoliticaConex` dispare cu singurul ei rând |
| `Potrivire` (244, pură), `DimensiuniResolver` (26), `Fapte` (69, adaptorul entitate → fapt), `ImpliciteService` (105: implicitul de TVA la culegere + „cine e partenerul documentului”, adaptor de citire) | §A | neschimbate: sunt deja forma nucleului |
| Regulile pure din servicii: `StocService.ValoareGolire`/`VerificaGoliri`, `AmortizareService.CotaLunara`/`Liniara`/`Degresiva`/`Deductibil`/`Situatie`/`LuniDeRecuperat`, `InchidereTvaService.CalculeazaLinii`/`LiniiPotrivescSoldurile`, `TvaService.CalculeazaValori`, `RegistruTvaService.Deriva` (descriere, nu entități), `PerioadaService.Verifica`, `SolduriService.Referinte`/`Referinta`, `DescarcareService` (alocarea cu pin-uri înaintea FIFO) | §A.2 | se mută în nucleu ca funcții pe operand închis; ordinea unică a gardienilor din `InchidereTvaService.Analizeaza` și `AmortizareService.Analizeaza` se păstrează |
| Lanțul de perioade (88): `PerioadeFiscale`, `InchideriPerioada`, acceptarea pe cheie, `GardianPerioada` (`FOR SHARE` devine fapt în operand: „perioada e deschisă și blocată”) | §E.2 | neschimbat ca model; snapshot-urile se înlocuiesc cu `Sold` |
| `GardianEditare` regulile (a), (d)–(o) + invarianții politicilor (F23-D5), pasul 0 (securitatea) | §E.1 | neschimbate; (b) se re-țintește pe `Postare`/`Tranzactie`/`Sold` (scrise doar de motor), (c) pe documentul `Împerechere`; (a) acoperă și `Numar`/`DataScadenta`/`Autogenerat`/`DocumentSursa` ca atribute scrise la operare (TR-r11) |
| `RandDupaCheie`, `TranzactieComanda`, `CititorTipDocument`, `OperareApi` (comanda prin ID, `OperareRezultat`), cele 18 controllere REST + `DocumentOperareController` XAF, `EroriDto` | §I | adaptoare; sârma nu se schimbă cât timp semantica nu se schimbă (partidele — declarat) |
| ModelCheck: harness-ul (`Program.cs` bootstrap, `Purja`, `MetadataDump`, `Duk`, `IntegritateTph`, `ScanareGetObjectByKey`, `NumaratorSql`) + probele de REGULĂ: `Api*` (212), `D3-V*`/`D4-V*` (79), `F21-D*` (46), `F23-V*` (30), `ACC-V*` (22), `COR-V*` (20), `F19-D*` (20), `F24-P*` (10), `R-D*` (10), `AMO-V*` (9), `Seed*` (29), `D15-V*` (29), `PER-*` (23), `DVI-V*` (20), `API-IMO-V*` (23), scenele e2e (≈368, majoritar regulă) | §G | rămân; cifrele de postare din scene se citesc din cub prin aceleași întrebări (Σ pe cont/lot/cod TVA); probă nouă: injectivitatea politicii de mișcare pe ambele seed-uri |
| Import1C: handlerele, `FlaxDb`/`FlaxDocumente`, `Nomenclatoare`, `Divergente`, cele patru contracte de reconciliere (`ReconciliereLuna`) | §H, `03-…` §A1 | se transferă CU puncte de atingere numite (review E5): ~15 citiri de `TipStoc` (`Catalog.cs:23`, `ReconciliereLuna.cs:737` `RegistreComparabile` → gestiuni reale, `Alocare.cs:159-171`, `Deschidere.cs:478-502`, `HandlereStoc.cs`, `HandlerFactura.cs:223`, `HandlereVanzare.cs:778`, `HandlerAsamblare.cs:285`, `Sabotaj.cs`, `Saft1C.cs`); `Deschidere` scrie tranzacția de deschidere cu unitate per partener (TR-D1); cele trei agregate „obținute” (sold per cont × lună, bază/taxă per corespondență de închidere, cantitate/valoare per produs × gestiune × lună) se citesc din cub — sume pe coordonate, forma raportului e aceeași |
| Nomenclatoarele, seed-ul (`ContaSeeder`, profilurile), `VerificareProfilService`, securitatea, catalogul HG 2139 | — | neatinse |

### 3.2 Se rescrie pe nucleu (o singură dată)

| ce | azi | pe nucleu |
|---|---|---|
| `MotorOperare` (814): fazele `CalculeazaSiValideaza` → `Opereaza` (§A.1), stornoul în roșu (`:660-672`), anularea, conexul, secundarul, un singur `CommitChanges` (`:427`) | scrie 3 registre + registrul propriu + `TotalStingere` + `Numar`/`DataScadenta`, în OS | `Motor(operand închis) → Contract { Postari[], Decizii[], Ipoteze[] } \| Refuzuri`; materializarea = adaptor care scrie `Tranzactie` + `Postare` (+ `Sold` la închidere); stornoul = tranzacție distinctă, inversul cauzat ∪ atribuit; anularea = ștergerea tranzacției fără dependenți; commit-ul la apelant (IM-D3, deja pe jumătate) |
| `StocService` (299) + `LoturiCulegereService` (312) + `DescarcareService` (145) | soldul intermediar din `RegistruStoc` + snapshot, FIFO în C# N+1, loturile născute la CULEGERE prin `ModifiedObjects` | unitatea (design §4): soldul lotului = `Σ[Unitate]`, FIFO în SQL pe `(Produs, Gestiune)` (fizica Q09), evaluarea la scriere serializată; lotul se naște la culegere în continuare (UI-ul îl alege), prețul lui e raportul unității |
| `ImperechereService` (371) | plafon per contrapartidă × sens, materializare polimorfă | TR-D2: nominalizare + documentul `Împerechere` |
| `SolduriService` (529): 3 snapshot-uri + `PartideDeschise`, SQL brut | `Sold` unic pe toate coordonatele, per spațiu, la granițe (fizica §2.3), cu proba de egalitate; `Reconstruieste` rămâne unealtă |
| `AmortizareService` (460) + `RegistruImobilizari` + `IDocumentCuRegistruPropriu` (PIF/CAS/AMO scriu ele în registru — `Imobilizari.cs:187/426/616`) | al patrulea registru, trei cifre pe rând | fișa = unitate; AMO postează contabil (Carte=Contabil) și fiscal (Carte=Fiscal) prin motor (design §8); deductibilul = citire; `MaterializeazaRegistrul`/`Elimina`/`Storneaza` dispar |
| `RegistruTvaService` (193) + `TvaService` (171) | rânduri fiscale cu `Baza`/`Tva`, `Data` fizică, perioada rescrisă la corecție | postări în spațiul Fiscal cu `CodTva` compus × rol, taxa per linie prin `Repartizeaza`, perioada = decizie la scriere (pasul 1 §5.9) |
| `PerioadaService.Inchide/Redeschide` (materializarea) | snapshot-uri | `Sold` la graniță + invariantul de egalitate |
| `CorectieService` (160) | storno + copie prin metadata EF + rescrierea perioadei fiscale | storno legat (tranzacție) + document nou; nimic rescris |
| Proiecțiile (`Proiectii/`, `Saft/`, `Api/*Proiectii`) | pe 4 registre + snapshot-uri | forma din pasul 1 §3 (sume pe coordonate + atribute prin `Cauza`), cu excluderea felului `Transfer` pe rapoartele pe cont; 8/13 deja probate identice (fizica §3) |
| ModelCheck probele de FORMĂ: `D17-V*` (58), `D18-V*` (30), `SOL-*` (53), `PAR-V*` (25), `PDT-V*` (19), `DIR-V*` (18), `IMO-V*` parțial (67), `JT-D*` (8), `F28-*` (15) | citesc rânduri de registru | se rescriu pe `Postare` (aceeași întrebare, altă tabelă) sau dispar odată cu forma (JT-D: al treilea registru nu mai există; PDT: perioada nu se mai rescrie) — TR-r9 |
| XAF: cele patru grile pe registre (`UI/ContaUiBaseline.cs:141-175`, `:843-845`, `ServerView`, probele `D85-*`) | `RegistruStoc/Contabil/Tva/Imobilizari` | se re-țintesc pe `Postare` (filtru pe `Spatiu`) la TR-D9 — singura dezghețare a XAF-ului, declarată (review E9) |
| Import1C `Bucla.Opereaza` (`Bucla.cs:903-931`), `Reluare`, `Drafturi.Sterge`, citirile de reconciliere | `MotorOperare.Opereaza(os, doc)` în buclă pe copii, registre re-citite | comanda nouă cu aceeași semnătură logică (document → lanțul de autogenerate, comite singură), contorul `Scara.MidpointBani` expus de nucleu, cele trei agregate din cub |

### 3.3 Dispare

`RegistruContabil`, `RegistruStoc`, `RegistruTva`, `RegistruImobilizari`,
`SolduriPerioadaContabil`, `SolduriPerioadaStoc`, `PartideDeschise`,
`Imperecheri`, `Document.TotalStingere`, `TipStoc`, flag-ul `Storno` pe rând,
`NumarNota`, cele 2 × 8 coloane de dimensiuni per latură, `Lot.PretUnitar` ca
fapt înghețat (devine raportul unității), `PoliticaConex` și mecanismul
clonei, hook-urile de stingere (4) și `IDocumentCuRegistruPropriu` (3
metode), `SensDeStins`, `ImperecheriProiectii` cu cele șase ramuri, puntea
891 a importului, `LoturiCulegereService`-ul XAF în forma pe `ModifiedObjects`
(rămâne decizia per linie).

## 4. Ordinea și regula de oprire (TR-D5…D10)

Fiecare pas e o felie cu contractul ei (D-uri pin-uite, regulă de oprire,
review advers), o sesiune sau mai multe; niciun pas nu începe până ce cel
dinainte nu e ÎNCHIS. XAF și React rămân înghețate (fără funcții noi); ușile
REST/OData și DTO-urile nu se schimbă decât unde semantica o cere (declarat
per pas).

- **TR-D5 Pasul 0 — decizia**: textul de aici + amendamentele la
  `invarianti.md` (I: împerecherea e operație, ca document; III: „starea
  documentului se citește de pe document” → restul e `Σ[Unitate]`, cubul e
  singurul adevăr al agregării; VI: prețul lotului = raportul unității,
  fiecare postare e fapt) intră ca decizie numerotată (următoarea din
  `docs/decizii/`), cu restanțele TR-r în `restante.md`. Contractul IM își
  schimbă starea în „depășit de N”. Oprire: decizia scrisă.
- **TR-D6a Pasul 1 — nucleul pur**: proiect nou fără referință la EF/XAF/HTTP
  (test de arhitectură): `Postare`, `Tranzactie` (fel `Operare | Storno |
  Transfer | Deschidere`), `Contract`, conservarea per spațiu (valoarea pe
  Contabil ∪ Stoc, cantitatea pe Stoc), `Repartizeaza` (Hamilton), unitatea
  (lot = partidă = fișă: o funcție de raport, una de nominalizare FIFO, una
  de deschidere), stornoul = inversul cauzat ∪ atribuit, `Sold` = Σ,
  rotunjirea cu contorul de jumătăți de ban. Din regulile pure ale §3.1
  intră aici DOAR ce cere pilotul (TR-D6b); restul intră cu tipul care le
  consumă, la TR-D7 (rafinare acceptată 2026-09-20, §4.1), cu probele lor
  (`F24-P*`, `AMO-V*`, `R-D*` ca tipar). Oprire: cele 7 invarianți din
  design §10 sunt teste (cu felul `Transfer` în definiția „un document = o
  tranzacție”); zero dependențe; ModelCheck neatins.
- **TR-D6b Pasul 2 — declarația fluxului per tip (pilot)** (review M5):
  forma care înlocuiește cele 30 de override-uri de `PregatesteOperare`/
  `ValideazaOperare` și celelalte 11 hook-uri — documentul tipat DECLARĂ pe
  operand închis: liniile cu măsurile lor, unitățile numite/născute,
  refuzurile proprii, copiii cu informație proprie; motorul rezolvă
  coordonatele din politici și postează. Pilot pe două frunze cu logică
  reală (BCS: două mișcări de stoc + sink; PLT: nominalizare de partidă +
  latura pereche), apoi FCT (recepția, TR-D3) ca a treia. Oprire: forma
  scrisă în contract, cele trei frunze produc din operand închis exact
  postările pe care motorul vechi le scrie azi în registre (probă pe
  scenele ModelCheck ale lor, transformate în cub cu maparea fizicii), plus
  diferențele declarate de TR-D1…D4.
- **TR-D7 Pasul 3 — strangler per tip de document**: motorul nou scrie
  `Tranzactie`/`Postare` în aceeași tranzacție de comandă în care motorul
  vechi scrie registrele; **regimul dual e per `TipDocument`, ca DATĂ**
  (o coloană pe ancora tipului: `PosteazaInCub`), nu per instanță — de aceea
  NIR-ul conex generat de motorul vechi din FCT nu e postat a doua oară:
  `NIR` nu e în mulțimea migrată câtă vreme FCT-ul postează recepția în cub,
  iar cubul și registrele rămân reconciliabile pe Σ per cont (aceeași zi pe
  toate cele 17.814 perechi — `review/04` §R4.3). Fără `is` pe frunză
  (invariantul II). `Postare` e O SINGURĂ entitate EF; partiționarea LIST pe
  `Spatiu` și FK-urile per partiție sunt SQL explicit în migrație
  (`migrationBuilder.Sql`), probate de ModelCheck (partițiile există, FK-urile
  țintesc partiția) — acceptat 2026-09-20, §4.1. Se migrează tip cu tip, gate-ul rulează pe submulțimea
  migrată; Import1C operează prin ambele. Oprire per tip: gate-ul de
  reconciliere al fizicii pe tipul migrat (Σ D = Σ C per tranzacție, Σ per
  cont / lot / cod TVA × perioadă egale cu registrele — cu diferențele
  DECLARATE: partidele, 401 pe FCT în loc de NIR, sink-ul de consum, Δ 3xx
  din TR-D4 tranșată prin contractul 1 — TR-r12); raportul Import1C identic
  pe conținut sortat cu baseline-ul; ModelCheck verde pe ambele profiluri cu
  probele de regulă neatinse; `refuzuri.ps1` 294/294; D406 trece DUK cu o
  sumă negativă într-o tranzacție de fel `Storno`/`Transfer` (review M4);
  injectivitatea politicii de mișcare pe cheile de după TR-D3.
- **TR-D8 Pasul 4 — citirile pe cub**: proiecțiile din pasul 1 §3, `Sold`
  unic + închiderea pe `Sold`, fișa/registrul de imobilizări pe unitate,
  „documente cu rest” și ecranul de stingere pe partidă (citirea per
  partidă intră în lotul de perf, cu indexul `(Unitate, Data)` pe Contabil
  — FZ-r3, review E6), probele de formă rescrise pe `Postare`. Oprire: 13/13
  rapoarte ale fizicii identice sau cu diferența declarată, D406 trece DUK,
  perf ≤ cifrele fizicii pe aceeași bază (A/B) INCLUSIV citirea per
  partidă, Import1C reconciliat din cub.
- **TR-D9 Pasul 5 — unitățile și tăierea**: împerecherea ca nominalizare +
  document `Împerechere` (TR-D2), AMO în două cărți, reevaluarea/corecția de
  preț cu `Atribuit` (design §5; prima instanță reală: DVI pe loturi), apoi
  tăierea: registrele, snapshot-urile, `Imperecheri`, `TotalStingere`,
  `TipStoc`, conexul FCT→NIR, hook-urile de instanță,
  `IDocumentCuRegistruPropriu`, grilele XAF re-țintite; motorul vechi și
  mecanismele din `Motor/*` dispar; lanțul de migrații se resetează
  (greenfield, fără migrare de date: bazele se refac prin seed + Import1C —
  preferința owner-ului, 89h). Oprire: Import1C identic din cub singur;
  ModelCheck cu probele de formă rescrise și cu scenele care acoperă ce
  Import1C nu atinge (review E8: `DVI-V*`, `IMO-V*`, `AMO-V*`, `COR-V*`,
  `F19-D*`, `PAR-V*` rescrise, storno-ul unui document nominalizat);
  `--dump-integritate-tph` zero; `verifica:drift` zero sau driftul declarat
  pe partide.
- **TR-D10 Proba supremă și regula generală de oprire**: la orice pas,
  raportul Import1C pe Flax e IDENTIC pe conținut sortat cu
  `reconciliere-20260918-154628.txt` (baseline-ul feliei 28); orice diferență
  e ori defect, ori consecință declarată a unei TR-D — a treia opțiune nu
  există (invariantul V). Cifrele de perf se compară A/B pe aceeași bază;
  niciun contract de citire nu aduce o interogare per linie.

### 4.1 Propunerea de execuție (2026-09-20, acceptată de owner)

Ordinea TR-D5…D10 rămâne; aici e CUM se execută, ca decizia 090 să o
preia ca regulă, nu ca poveste.

**Mecanica.** O felie = contract în `docs/nucleu/` cu D-uri pin-uite, regulă
de oprire și review advers la închidere; branch per felie, `main` doar cu
felii închise; multiagent-delivery (coordonatorul ține contractul,
verificările și decizia; explorarea și implementarea pe agenți cu spec).
Trei oracole, refolosite, nu inventate: scenele ModelCheck ale tipului
(postările motorului vechi), maparea fizicii registre → cub (`gen-cub.py`
portată în C# ca helper de test — e oracolul pilotului și al
strangler-ului) și raportul Import1C pe Flax identic cu baseline-ul feliei
28. Un singur consumator al nucleului: `Atlas.Conta.BackOffice.Module`;
WebApi și Import1C nu referă `Atlas.Conta.Nucleu` direct.

**Feliile și mărimea lor.** 090 (TR-D5): o sesiune; aduce și FZ-r2
(fișa de la `Sold`: pierderea e +40 % față de F0 cu referință, întrebarea
rămasă e granul lui `Sold`, FZ-r1, gate la TR-D8, nu condiție prealabilă).
Nucleul pur (TR-D6a): 1–2 sesiuni; proiect `nou/Atlas.Conta.Nucleu` fără
niciun pachet în afara BCL + `Atlas.Conta.Nucleu.Teste`; cei 7 invarianți
ca teste pe proprietăți cu generatoare proprii; testul de arhitectură pe
referințele assembly-ului. Pilotul (TR-D6b): 2–3 sesiuni; un declarant per
frunză, în Module, care construiește operandul închis prin adaptorul `Fapte`;
e singurul pas care poate întoarce decizia. Strangler-ul (TR-D7): 6–8
sesiuni; ordinea tipurilor: cele trei ale pilotului, apoi după volumul pe
Flax, tipurile cu conex la urmă. Citirile (TR-D8): 3–4 sesiuni; raport cu
raport, tăiat pe cub când diff-ul din `egalitate.md`, portat în ModelCheck,
e zero; A/B-ul FZ-r1 aici. Unitățile și tăierea (TR-D9): 3–4 sesiuni.
Total ≈ 18–22 de sesiuni cu produsul înghețat.

**Rafinări acceptate față de §3–§4.**
1. Regulile pure intră cu tipul lor: nucleul primește la TR-D6a doar ce
   cere pilotul; fiecare tip aduce regulile lui când migrează la TR-D7
   (altfel pasul 1 transferă ≈2.000 de linii fără consumator și fără probă).
2. `Postare` = o singură entitate EF; partiționarea și FK-urile per
   partiție = SQL explicit în migrație, probate de ModelCheck (nu trei
   entități pe spații cu vedere).
3. Maparea fizicii devine cod de test, nu script.

**Regula înghețului XAF/React** (intră în 090 ca regulă durabilă):
- înghețul e pe FUNCȚII; React e izolat prin DTO-uri, iar din XAF mor doar
  cele patru grile pe registre (re-țintirea lor la TR-D9 e dezghețarea
  declarată);
- o schimbare în XAF sau React e permisă DOAR dacă e consecință declarată a
  unei TR-D în contractul feliei (ecranul de stingere pe partidă și
  „documente cu rest” la TR-D8; culegerea unității pe linie la TR-D9 —
  până atunci nucleul nominalizează implicit FIFO) sau repară un defect
  care blochează un gate (Import1C, ModelCheck, `refuzuri.ps1`);
- orice cerere de produs apărută între timp intră în `restante.md` cu
  decizia ei, nu în felie;
- valul de la sfârșit se ține mic prin DTO-uri stabile pe tot parcursul și
  prin tăierea listei amânate în două: citiri la TR-D8, unități la TR-D9.

## 5. Raportul cu contractul IM și cu invarianții

- **IM** (`p5-felia-izolare-motor-contract.md`, reanalizat 2026-09-18) avea
  aceeași țintă (faza pură, contracte de citire, commit-ul la apelant) pe
  FORMA de azi. TR-D6a/b și TR-D7 o realizează pe forma nouă; ce IM livrase
  deja (precedentele pure, `SolduriService` ca adaptor, `RandDupaCheie`,
  tranzacția la apelant, `CititorTipDocument`) se preia. Pașii IM 0a–0c și
  1–7 NU se mai execută ca felie proprie; IM-r1…r7 se re-evaluează la TR-D5.
- **Invarianți**: I amendat (împerecherea e operație — document `Împerechere`;
  deschiderea rămâne excepția, ca tranzacție de fel `Deschidere` fără
  document, dar cu unitate și partener pe terți); II întărit (motorul nu
  cunoaște frunzele fiindcă nu cunoaște documente, ci operanzi închiși;
  regimul dual e dată pe tip; `RolTert` e atribut de nomenclator al
  contului, nu frunză); III amendat în literă (registre → cub;
  `TotalStingere` nu mai e „stare a documentului”); IV neatins (politicile
  rămân date; două tabele re-cheiate pe coordonate structurale — cont,
  gestiune — nu pe un enum de cod); V neatins (Import1C rămâne proba,
  diferențele se raportează — inclusiv Δ 3xx); VI amendat (prețul lotului =
  raportul unității; valoarea ieșirii rămâne fapt scris la operare, metoda
  rămâne una per bază).

## 6. Ce nu poate proba pasul acesta (și rămâne pe hârtie)

Fără date pe Flax: valuta și partidele în valută, decontul cu angajatul
(0 documente, 0 pe 542), avansul de furnizor (409 nefolosit), compensarea
reală 401 = 4111 pe același partener (0), lanțul avans → factură →
regularizare prin împerechere (0), stornoul unui document împerecheat (0
stornate), corecția legată (0), DVI (0), imobilizările (registru gol),
custodia/folosința/gratuitul/producția neterminată (0 rânduri, 0 politici la
privat), profilul bugetar, recepția pe aviz (0 NIR manuale). Data reală a
împerecherii e artefact de import (toate 2026-09-18): toate cifrele de
partidă sunt „la infinit”.

## 7. Restanțe (TR-r)

- TR-r1 ÎNCHISĂ prin review (N5): sub partida pe 4111 la brut, 118 din
  2.036 de FCL cu avans au o stingere peste rest (nu 1.766); FCT (14.341)
  dispar prin TR-D3.
- TR-r2 notele pe conturi de stoc fără lot (2.997 pe Flax, cinci
  corespondențe): conectorul decide per corespondență — postare de valoare
  pe lot sau divergență declarată; puntea 891 dispare cu deschiderea ca
  tranzacție. Se tranșează la TR-D7 pe contractul 1.
- TR-r3 fizica re-măsurată cu postarea de stoc unificată și cu felul
  `Transfer` exclus: `Spatiu` re-definit, balanțele pe Contabil ∪ Stoc, FK
  per partiție, indexul `(Unitate, Data)`.
- TR-r4 recepția fără factură (NIR pe aviz, 408): cerință de produs de
  confirmat; NIR rămâne tip până atunci; fluxul `408 = 401` prin `LotId`.
- TR-r5 DSC din FCL trece testul documentului-copil azi; se re-judecă dacă
  descărcarea devine clonă fără alegere de loturi.
- TR-r6 conectorul 1C: data reală a împerecherii (artefact) și deschiderea
  de terți per partener / per factură deschisă (65,5 % din soldul de terț
  stă azi pe deschidere fără partener).
- TR-r7 sink-urile bugetare (Gratuit/Folosință/Custodie) ca gestiuni
  virtuale / cont 803x: fără cifre, se probează la primul seed bugetar pe
  cub, cu proba de injectivitate.
- TR-r8 rulajul brut per partidă nu e sumă sub tranzacția de transfer
  (declarat); fișa unei partide se randează ca fereastră, ca fișa contului.
- TR-r9 probele de formă din ModelCheck (≈300): inventar rescrie/șterge la
  TR-D8/D9, nu înainte.
- TR-r10 deschiderea rămâne excepția I ca tranzacție de fel `Deschidere`
  fără document, dar CU unitate (lot / partidă) și partener pe terți;
  Import1C `Deschidere.cs` scrie tranzacția, nu rânduri bloc.
- TR-r11 `Numar`, `DataScadenta`, `Autogenerat`, `DocumentSursa` rămân
  atribute ale documentului scrise la operare (decizii ale contractului, nu
  postări), sub gardianul (a) neschimbat.
- TR-r12 Δ de sold 3xx (+585.404,66 pe Flax) între registrul de stoc și cel
  contabil de azi: cine are dreptate se tranșează prin contractul 1 al
  reconcilierii cu 1C la TR-D7; până atunci e constatare raportată, nu
  consecință acceptată.
