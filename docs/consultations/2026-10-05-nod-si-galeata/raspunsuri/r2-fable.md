# Runda 2: raport

Am lucrat numai prin citire, la `a5df5fe`. Prescurtări: `N/`, `M/` ca în fișă; **Y-lit** = Y cum e scrisă în fișă (`Miscare`, tranzacție Operare); **Y-T** = Y scrisă ca `Mutare` (tranzacție Transfer).

## 1. Verdicte

**Amendamentul 1: se acceptă cu schimbări.** Y-lit se respinge, cu contraexemplul din cazul 1: la 100 lei transformați, 371 primește D 200 / C 200, iar balanța filtrată pe material arată 100 lei la stoc zero. Se acceptă Y-T, cu gardul nodului în nucleu și cu nodul marcat ca unitate, nu recunoscut după gestiunea virtuală.

**Amendamentul 2: se acceptă cu schimbări, în TR-D9b.** Regula e bună, dar textul din fișă contrazice codul în două locuri (factura de imobilizări, deschiderea) și lasă patru lucruri nedefinite: felul de unitate al contului, latura semnului, legătura cu steagurile `Repartitor`/`Material`, ieșirea din găleata închisă.

## 2. Sarcinile

### S1. Faptele din runda 1

| # | Întrebarea | Răspuns |
|---|---|---|
| 1 | Transferul cere unitate la ambele capete? | Nu. `Mutare` cere doar același cont (`N/Motor/Mutare.cs:17`), iar unitatea nu e cerută în gestiune virtuală (`N/Conservare.cs:152`). Precedent: PIF mută anonim → fișă pe același cont, prin Transfer (`docs/nucleu/scenarii/IMO.md:15`). |
| 2 | Jurnalul e pe formule compuse? | Da. Rânduri pe postare cu sens (`M/Proiectii/ContabilProiectii.cs:510-528`); contrapartida se calculează ca mulțime (`:486-489`). Argumentul meu M×N contra lui X cade. |
| 3 | Analitice traversate des? | Pe contul liniei din sursă: 1 din 10.198 (fișa §3). Pe contul derivat din tip: 367 din 1.228 (`docs/nucleu/scenarii/ASM.md:12-13`). Vezi S9. |
| 4 | Valorile produse sunt culese? | Da (`M/Declaratii/DeclarantAsamblare.cs:37,59`); derivarea e comanda `DistribuieValoarea` (`docs/nucleu/tr-d9-taierea-contract.md:296-302`). |
| 5 | FIFO rescrie istoria? | Nu (`docs/invarianti.md:143-145`). Gardul pe document nu e fragil. |
| 6 | Citiri de valoare pe produs fără filtru? | Da: balanța și fișa cu `materialId` (`ContabilProiectii.cs:208`, `:470-476`) și snapshotul contabil (`M/Motor/SolduriService.cs:221-231`). SAF-T stocuri nu: citește numai loturi (`M/Saft/SaftProiectii.PeCub.Stocuri.cs:111`). |
| 7 | Document tipat fără fișă pe 21x? | Da: FCT postează anonim, PIF nominalizează (`IMO.md:15-16`). |
| 8 | Solduri mari fără unitate? | Pe stoc da: 3.365 picioare 3xx fără lot (`docs/nucleu/scenarii/NTC.md:77`). Pe 401/4111 nu am cifră. |
| 9 | Nota poate numi gestiune virtuală? | Nu: repartitorul trebuie rezolvat din nomenclator (`M/Declaratii/DeclarantNotaContabila.cs:22-24`), iar gestiunile virtuale nu au rând (`N/Cub/GestiuniVirtuale.cs:7-8`). Nimic nu o interzice explicit. |

### S2. Forma Y pe nucleu

**Nu trece nemodificată.** `VerificaTransformarile` refuză orice postare din gestiunea `Transformare` cu valoare nenulă (`N/Conservare.cs:36`). E singurul obstacol, în ambele scrieri. Restul trece:

- `Miscare.Postari` nu are constrângeri (`N/Cub/Miscare.cs:11-23`).
- Valoarea și cantitatea se conservă pe fiecare linie (`Conservare.cs:71-94`).
- Formele admit capătul virtual fără unitate (`:152`).

**Rămân fără obiect:** `VerificaTransformarile` întreagă (`:26-42`) și primitiva cu validările ei (`N/Motor/Transformare.cs:16-25`). Validările primitivei (lot pe gestiune reală, fără terț, TVA sau valută, cantitate pozitivă) nu le mai apără nimeni în nucleu și trebuie mutate în declarant.

**Ce se pierde și fișa nu spune.** În X, contraponderea are valoare zero, deci egalitatea P = C a documentului e apărată de conservarea generică: Σ pe (cont, latură) la Transfer (`:97-105`), Σ D − Σ C la Operare (`:71-81`). În Y fiecare linie se echilibrează singură și conservarea generică devine vacuă pentru document. Gardul nodului rămâne singura apărare.

**Gardul nou stă în nucleu**, în locul `VerificaTransformarile`: Σ valoare semnată pe postările nodului, pe cont și tranzacție, este zero. Are dublură persistată în `M/Cub/Citiri/Invarianti.cs`. Declarantul păstrează refuzul explicat. Numai în declarant nu ajunge: următorul declarant care folosește nodul poate scurge valoare.

Y-T diferă de Transferul de azi doar prin valoarea de pe contrapondere: postările de pe loturi sunt identice cu cele din `Transformare.cs:27-31`.

### S3. Cazuri proprii

**Cazul 1. LA: A 2/100 → LB: B 1/100, cont 371, gestiune G.**

| Forma | Postări (latură, gestiune, produs, cantitate/valoare) |
|---|---|
| X | D G A −2/−100; D nod A +2/0; D G B +1/+100; D nod B −1/0 (Transfer) |
| Y-lit | C G A −2/100; D nod A +2/100; D G B +1/100; C nod B −1/100 (Operare) |
| Y-T | D G A −2/−100; D nod A +2/+100; D nod B −1/−100; D G B +1/+100 (Transfer) |

- Loturile și soldul 371 sunt identice în toate.
- Rulaj 371: X și Y-T 0 (Transferul e exclus, `M/Cub/Citiri/Contabil.cs:42`); Y-lit D 200 / C 200.
- Nodul în Y ține pe produs +100 la A și −100 la B, pentru totdeauna; pe cont e zero.
- Greșește în tăcere, Y-lit: balanța pe material fără gestiune dă A = 100 și B = 0.
- Aceleași cifre greșite ies azi și în X: Transferul e exclus din cititorul contabil, deci balanța pe material nu vede mutarea. Defectul e existent și independent de amendament.

**Cazul 2. L1: A 3/10,00 și L2: A 3/10,01; consum 1 din L1 și 2 din L2; produse K1, K2, K3 câte o bucată.**

- Evaluarea (`N/Masura/Evaluare.cs:15-17`): 3,33 + 6,67 = 10,00.
- Culese 3 × 3,33 = 9,99: refuz în ambele forme. După distribuire: 3,33; 3,33; 3,34.
- X: L1 −1/−3,33; L2 −2/−6,67; K +3,33; +3,33; +3,34; cinci contraponderi zero.
- Y: aceleași capete reale; nodul primește +10,00 pe A și −10,00 pe K.
- Rest identic: L1 2/6,67; L2 1/3,34.
- Greșește în tăcere: dacă gardul declarantului lipsește, X e refuzat oricum de nucleu (Σ pe (371, D) = −0,01). Y trece și lasă 0,01 în nod; Σ loturi ≠ sold 371, semnalat doar de un raport SAF-T (`Stocuri.cs:452-459`).

**Cazul 3. ASM 2A/100 → 4B/100; DSC vinde 1 B (607 = 371: 25); storno ASM.**

- Inversa duce LB la −1 și e refuzată de gardul cronologic de cantitate pe lot (`M/Cub/Citiri/Loturi.cs:75-104`, chemat din `M/Cub/Materializare.cs:113`). Identic în toate formele: gardul citește numai postări cu lot (`Loturi.cs:77-79`).
- După storno DSC, storno ASM trece. Rulaj: X și Y-T 0; Y-lit D −200 / C −200.
- X și Y-T cer selecția specială a Transferului de stoc (`Materializare.cs:77-80`); Y-lit merge pe calea comună. E singurul avantaj real al lui Y-lit.

**Cazul 4. Ianuarie: 2A/100 → 1B/100, lună închisă. Februarie: storno și ASM nou 1A/50 → 1B′/50.**

- Solduri la 31.01 identice. Final: LA 1/50, LB 0/0, LB′ 1/50.
- Rulaj 371: X și Y-T zero în ambele luni. Y-lit: ianuarie D 200 / C 200, februarie D −100 / C −100, la sold neschimbat.
- X și Y-T nu lasă urmă în jurnal, ca transferul între gestiuni.
- Corecția de preț pe un lot consumat prin ASM nu se propagă în nicio formă; e a TR-D9b (`ASM.md:64`).

### S4. Cititorii

| Cititor | Sub Y-lit | Sub Y-T |
|---|---|---|
| Balanța (`ContabilProiectii.cs:186-228`) | Pe cont: sold corect, rulaj +2V. Pe cont și material: consumatul supraevaluat cu ΣC cumulat, obținutul subevaluat cu ΣP. Corect doar cu gestiune reală. | Nodul e invizibil. |
| Fișa contului (`:439-491`) | Două rânduri pe linie, contrapartidă 371; soldul curent pe material e greșit. | Invizibil. |
| Registrul jurnal (`:510-528`) | Patru rânduri 371 la documentul simplu. | Invizibil. |
| SAF-T GL (`M/Saft/SaftProiectii.PeCub.cs:92`) | Înregistrări 371 = 371; mișcarea de stoc primește `TransactionId` (`Stocuri.cs:367-371`). | Ca azi. |
| Componentele SAF-T (`Stocuri.cs:504-507`) | Se împacă numai cât nodul se închide pe cont. | La fel. |
| Snapshotul contabil (`SolduriService.cs:217-233`) | Rânduri permanente (371, nod, produs). | Invizibil. |
| `PozitiiFaraFisa` (`M/Cub/Citiri/Imobilizari.Conturi.cs:19-21`) | Domeniul include nodul; îl scoate doar filtrul pe conturile fișelor (`M/Motor/ImobilizariFapte.cs:49-50`). | La fel. |
| `VerificaPozitiaFaraFisa` (`M/Cub/Materializare.PozitieFaraFisa.cs:33-42`) | Fără filtru; prinde nodul dacă un cont de stoc devine protejat. | La fel. |
| Partide (`M/Cub/Citiri/Partide.cs:178-204`), loturi (`Loturi.cs:21-23`) | Neatinse: filtrează pe felul unității. | Neatinse. |

`Plati.cs:61` și `ImperecheriProiectii.cs:141` nu le-am verificat.

„Filtrul dispare” e fals pentru Y-lit: reapare ca filtru pe gestiunea virtuală, pe aceleași locuri, și devine portant pe valoare. Azi, uitat, greșește doar numărul de rânduri.

### S5. Invarianții

| Verificare | Verdict |
|---|---|
| Postări lipsă, document și cauză (`Conservare.cs:7-11`, `:44-68`) | principiu |
| Valoarea pe carte (`:71-81`) | principiu; în Y nu mai poartă P = C |
| Cantitatea pe produs (`:83-94`) | principiu al cubului; el cere nodul în ambele forme |
| Transferul pe (cont, latură) și (cont, produs) (`:96-115`) | principiu; în X poartă și P = C pe cont |
| Semnul la deschidere (`:119-128`) | principiu |
| Formele, cu excepția virtuală (`:130-188`) | principiu; nucleul nu cunoaște regimuri, deci P și Q stau în modul |
| `VerificaTransformarile` (`:26-42`) | artefact X, integral |
| Proveniența stornoului (`Invarianti.cs:94-102`) | principiu; filtrul din ea e artefact X |
| Fiscal, taxa liniei (`:79-133`) | principiu |
| Debit = credit pe tranzacție (`:35-38`), transferuri (`:64-74`) | principiu |
| Registru → cub: contabil, deschidere, loturi, imobilizări (`:19-33`, `:42-54`, `Loturi.cs:111-143`, `Imobilizari.cs:9-37`) | martori ai regimului dual; mor la TR-D9a |
| `Partide.VerificaAcoperire`: fel partidă cere unitate și partener (`Partide.cs:200-201`); partener fără partidă (`:203`) | principiu |
| Aceeași, clauza cu părțile documentului (`:203-204`) | artefact P: deduce „documentul tipat” din părți |
| Deducerea conturilor protejate (`PozitieFaraFisa.cs:13-24`) | artefact P; semnul cronologic e principiu |

### S6. Clasificarea scenariilor

| Rând | Verdict |
|---|---|
| SC-ASM-01, 02, 03 | Soldurile vin din regulă și rămân; numărul de postări și felul se rescriu. |
| 04 | Din formă; devine refuz. |
| 05, 06, 21, 22, 23 | Din formă (grupuri pe conturi); devin refuz. |
| 07, 14, 15 | Regulă; rămân. |
| 08, 09 | Regula rămâne; fixture-ul (05) se rescrie pe un cont. |
| 10, 11, 12, 13, 24, 25 | Regulă; rămân. La 13 apare un cod nou pentru conturi diferite. |
| 16 | Zero TVA rămâne; „contraponderi cu valoare 0” se rescrie. |
| 17, 18, 19 | Regulă, cum le rescrie deja TR-D9a; independente de formă. |
| 20 | Din formă; după distribuirea pe document (`tr-d9-taierea-contract.md:321`), ambele conturi ies neînchise. |
| 26, NUC-ASM-RECONCILIERE | Fără obiect după tăiere. |
| SC-X-09 | Lanțul consumă de pe 302 și produce pe 371 (`ASM.md:57`); devine refuz și se rescrie. |
| SC-NTC-01, 15 | Regulă (nu se inventează partidă); rămân sub „deschis”. |
| SC-NTC-14 | Vine din lipsa gardului, nu din regula contabilă; rămâne sub sămânță și primește un geamăn refuzat sub „închis”. |
| SC-IMO-27, 28 | Regulă (semn cronologic); rămân. |
| SC-IMO-31 | Regula rămâne; blocajul global e implementare. |

### S7. Regula Q pe cod

| Regim | Azi |
|---|---|
| Închis | Nu există ca gard de scriere. Apropiat: deschiderea refuză totalul nedetaliat pe stoc și partide (`M/Cub/Materializare.Deschidere.cs:112-116`); nota cu partener nominalizează mereu (`DeclarantNotaContabila.cs:63-84`). |
| Deschis | Implicit: SC-NTC-01, 14, 15. |
| Suport | `VerificaPozitiaFaraFisa`: semn cronologic pe coordonata completă (`PozitieFaraFisa.cs:47-62`), cu latură pe cont (`:11`) și blocaj global (`M/Cub/Materializare.Suport.cs:19`). |

**Ce schimbă sămânța:**

- Pe stoc și terți, la note, nimic.
- Dacă deschiderea ajunge să posteze în găleată pe conturi deschise, se schimbă decizia 094 (refuzul atomic al diferenței). Păstrați 094.
- La imobilizări, protecția trece de la dedusă la declarată. Un cont nou pus ca implicit al unui tip de imobilizare rămâne fără gard dacă nimeni nu-i pune regimul suport: fișe finanțate din nimic, în tăcere. Trebuie gard la editarea configurării și latura semnului declarată.

**Închiderea unei găleți cu sold.** „Refuză numai postările noi” blochează soldul. Exemplu: 401 fără partener, sold creditor 500, regim trecut pe închis. Regularizarea 401 (anonim) D 500 = 401 (furnizor X) C 500 e refuzată, fiindcă piciorul de debit intră în găleată. Trebuie admise inversele de storno și postările care scad soldul spre zero fără să-l treacă.

**Costul blocajului dacă suportul se extinde.**

- Scrierea e deja serială pe bază (`M/Motor/TranzactieComanda.cs:13-14`), deci azi blocajul nu costă nimic în plus. După rafinarea blocajului scrierii (X-r1), blocajul global ar reserializa tot.
- Costul real e citirea: gardul încarcă în memorie tot istoricul fără unitate al conturilor protejate, la fiecare operație (`PozitieFaraFisa.cs:41-43`). Pe 401 sau 371 e liniar în istoric. Nemăsurat (`IMO.md:80`).

**Ciocnirea de nume.**

- `DimensiuneFlags.Unitate` e unitatea organizatorică (`M/BusinessObjects/Comun/Enums.cs:132`, `M/Motor/MotorOperare.cs:526`).
- Mai grav decât numele: steagurile `Repartitor` și `Material` (`MotorOperare.cs:516-519`) spun aproape același lucru ca „închis” pe terți și pe stoc. Q trebuie să spună dacă le implică sau le înlocuiește; altfel regula are două mecanisme.

**Pe cont sau pe cont și fel de unitate.** Pe cont, dar numai dacă felul de unitate al contului e declarat. Azi e dedus din trei locuri: `UrmarestePartide`, tipul de material, politica de amortizare. Regimul fără fel declarat n-are sens: „închis” pe 628 ar refuza tot. Un fel pe cont ajunge [judecată]; deschiderea exclude deja lot pe cont cu partide (`Deschidere.cs:88`).

### S8. Interacțiunea nod și găleată

- **În scriere** discriminatorul ține, dar prin absență: nota nu poate numi gestiunea virtuală și nu pune produs (`DeclarantNotaContabila.cs:56-60`).
- **În citire** nu ține: ambele definiții ale găleții sunt „unitate nulă” (`Imobilizari.Conturi.cs:21`, `PozitieFaraFisa.cs:33-34`). Sub semn, nodul produsului obținut e creditor permanent, deci orice asamblare ar fi refuzată pe un cont de stoc protejat. `GestiuniVirtuale.Este` nu se traduce în SQL, deci ar trebui o expresie-filtru nouă.
- **Alternativa mai sigură [judecată]:** nodul primește o unitate de fel nou, „tranzit”, cu identitate din document și cont. Găleata rămâne „unitate nulă”, fără niciun filtru. Cititorii pe fel o exclud singuri, iar `Conservare.cs:160` dă „același cont” structural. Gardul devine: unitatea de tranzit are sold zero în tranzacția ei.

### S9. Contul lotului

- „Același cont” înseamnă contul implicit curent al tipului de material al produsului (`M/Motor/Fapte.cs:95-103`), nu contul liniei și nu contul pe care stă valoarea lotului.
- **Refuz pe nedrept:** ambalaj (381) și marfă (371) puse într-un set. Recensământul pe această coordonată dă 367 din 1.228 (`recensamant-asm-ldi-nir.sql:6`, `ASM.md:12-15`), iar BPR nu există (`docs/nucleu/tr-d9-inventar.md:42`). Refuzul e fără ieșire.
- **Ocolire:** două tipuri mapate pe același cont trec gardul indiferent de substanță; o producție reală (301 → 345) trece dacă planul le ține pe 371 [judecată].
- **Efect lateral existent:** dacă tipul produsului se schimbă după recepție, soldul se caută pe contul nou (`DeclarantAsamblare.cs:53,62`), iese zero și consumul e refuzat cu stoc insuficient (`Evaluare.cs:9-13`).

### S10. Locul în plan

**Amendamentul 1: în TR-D9a, ca pas propriu 6b, cu commit separat.**

- Pasul 2 n-a început (`docs/instructiuni-proiect/CLAUDE.md:176-178`), deci rândurile ASM se rescriu o singură dată, direct în forma finală. După tăiere s-ar rescrie de două ori.
- Riscul: lista închisă de schimbări (`tr-d9-taierea-contract.md:159-160`) și regula de oprire pe cifre schimbate (`:702-703`).
- Text de schimbat: a cincea schimbare în D9-D1 (`:162-172`); „forma ASM-B2 și regula n→m nu se schimbă” (`:289-291`); rândurile 20-23 (`:321-324`); pasul 6 (`:665`); D9-r1 (`:629-631`); ASM-B2…B4 marcate ca amendate.

**Amendamentul 2: în TR-D9b.** E comportament nou, exact natura feliei „unitățile” (`:29-32`). TR-D9a păstrează gardul de azi (`:183-184`). În TR-D9a ar ieși din perimetru (`:153-157`).

### S11. Fișa de fapte

- „LDI e forma Y deja”: capătul virtual al LDI stă pe contul de cheltuială sau venit (`M/Declaratii/DeclarantDiferenteInventar.cs:83`), nu pe contul de stoc. Precedentul real e PIF, și el susține Transferul.
- Nu spune că nucleul refuză Y azi, nici că P = C iese din conservarea generică.
- Nu dă rulajul sub Y (2V) și nici rulajul sursei.
- §3 numără contul liniei din sursă; gardul va vedea contul tipului. Fișa prezintă 367 ca artefact, deși e măsura pe coordonata gardului.
- „Numai nota și deschiderea postează în găleată” e fals azi (S1.7, S7).
- Omite suprapunerea cu `Repartitor`/`Material`, lipsa BPR, SC-X-09 și distribuirea pe document.

## 3. Ce am schimbat față de runda 1

1. **Storno după consum.** Am scris că nicio formă nu-l oprește. Greșit: gardul există (`Loturi.cs:75-104`).
2. **Corespondența arbitrară în X.** Retrasă: cubul nu ține corespondențe (S1.2).
3. **Pragul lui X.** Rămâne (`DeclarantAsamblare.cs:87-88`), dar dispare și prin simplul refuz al grupului neechilibrat; nu cere Y.
4. **Linia explicită pe nod între conturi.** Retrasă, pe datele sursei; înlocuită cu avertismentul din S9.
5. **„Gard simplu”.** Am subestimat: conservarea generică devine vacuă, deci gardul e portant și trebuie în nucleu.
6. **Implicitul „închis” pe stoc.** Retras, din cauza celor 3.365 de picioare; accept „deschis”, cu raport.

## 4. Schimbări cerute, în ordine

1. Y se scrie ca `Mutare`, nu ca `Miscare`. Dacă owner-ul vrea articolul 371 = 371, ținta e V, nu 2V, și cere filtru pe nod în cititorul contabil.
2. Gardul nodului în `Conservare` și în invarianții cubului; refuzul explicat în declarant.
3. Nodul ca unitate de tranzit, dacă Q vine.
4. Regula scrisă ca „nodul se închide pe fiecare cont”, cu distribuirea pe cont, nu pe document.
5. Recensământul refăcut pe contul derivat, înaintea deciziei; refuzul pe conturi diferite declarat ca restanță până la BPR.
6. Q: felul de unitate și latura declarate pe cont; gard la configurare; ieșirea din găleata închisă; relația cu `Repartitor`/`Material`; nume fără „unitate”.
7. Q: textul corectat. Sursele tipate de suport postează în găleată; deschiderea rămâne detaliată integral.
8. Raport „găleată față de unități” pe cont și control la închiderea perioadei, ca singură plasă sub „deschis”.

Dacă nu vreți gard nou în nucleu, alternativa e X restrâns la Transfer, cu refuzul grupului neechilibrat. Scapă de clasificare și de prag, păstrează primitiva și filtrul, și nu pune valoare în nod.

## 5. De măsurat sau prototipat

| Ce | Zile |
|---|---|
| Recensământul ASM pe contul derivat, cu maparea de tipuri intenționată | 0,5 |
| Rulajul 371.1 din asamblări în sursă față de ΣV, ca să fixați ținta 0 sau V | 0,5 |
| Prototip Y-T în nucleu: gard, teste n→m, storno | 1–1,5 |
| Proba balanței și fișei pe material după ASM și BTR (defectul existent) | 0,5 |
| Numărul de postări fără unitate pe 401, 4111, 3xx și timpul gardului de suport la acel volum | 0,5–1 |
| Schița gardului de configurare și a ieșirii din găleata închisă | 1 |

## 6. Încredere

- **Amendamentul 1:** 4 că Y-lit e greșită; 3 că Y-T bate X restrâns. Mă limitează: n-am rulat nimic, n-am verificat politica de mișcări SAF-T și nu știu rulajul dorit de owner.
- **Amendamentul 2:** 4 pe „cu schimbări, în TR-D9b”; 3 pe forma exactă. Mă limitează: fără cifre pe 401/4111, n-am citit seed-ul steagurilor și doi cititori au rămas neverificați.

Separat de consultare: conectorii claude.ai Gmail, Google Calendar, Google Drive și dxdocs cer autorizare din setările de conectori claude.ai și nu sunt disponibili până atunci; n-au fost necesari. Clona nu are `.codegraph/`, deci am lucrat cu citire și căutare directă. Există CodeGraph v1.6.2; actualizarea se face cu `codegraph upgrade`.
