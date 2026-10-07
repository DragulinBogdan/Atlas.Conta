# Runda 2: verificarea pe cod (`a5df5fe`)

**Verdict: rămâne A. TR-D9a se amendează în cinci puncte, nu se oprește.** B nu se construiește, iar C nu mai merită întoarcerea. Trei dintre argumentele mele pentru A din runda 1 au ieșit mai slabe pe cod decât pe principii, iar o parte din ce credita bilanțul cubului nu s-a întâmplat. Numai citire: nimic rulat.

Prescurtări: `M/` = `nou/Atlas.Conta.BackOffice/Atlas.Conta.BackOffice.Module/`, `N/` = `nou/Atlas.Conta.Nucleu/Atlas.Conta.Nucleu/`, `dn/` = `docs/nucleu/`, `perf` = `docs/api/p5-perf-masuratori.md`, `X` = `dn/tr-d8-transversal-contract.md`, `K` = `dn/tr-d9-taierea-contract.md`, `inv` = `dn/tr-d9-inventar.md`.

## 1. Verificarea listei din runda 1

| # | Întrebarea | Răspunsul din cod |
|---|---|---|
| 1 | Cheie de corespondență? | **Nu.** Declarația e pe perechi (`N/Cub/Miscare.cs:11-23`), dar motorul le aplatizează (`N/Motor/Motor.cs:10-14`) și `Postare` nu are cheie de pereche (`M/Cub/Postare.cs:8-65`). Contul corespondent din fișă e euristic, la nivel de tranzacție, și iese NULL când sunt mai multe conturi (`M/Proiectii/ContabilProiectii.cs:486-489`). |
| 2 | Proiecția citește în afara cubului? | Fără obiect: proiecția cub→registre nu există. Există numai direcția inversă, ca oracol de test (`K:126-128`). |
| 3 | Ramificări, coordonate nule, CHECK-uri | 47 de apariții `FelUnitate.X` în 27 de fișiere din `M/Cub`, `Declaratii`, `Proiectii`, `Motor` (numărat azi), plus nucleul (`N/Conservare.cs:165-186`). 33 din 45 de coloane sunt nulabile și sunt 2 CHECK-uri (`M/Migrations/20260925110419_InitialCreate.cs:3760-3769`). Completitudinea fiscală e probă de ModelCheck, „negarantată de scriere” (`M/Cub/Citiri/Invarianti.cs:10, 104-113`). |
| 4 | p95 pe volum real | **Nu există.** Scara are 7.951 de postări (`perf:1023-1024`); „nu există bază de volum reală” (`X:201`); tabelele au sub 300 de pagini (`X:729`). |
| 5 | Lipsește stratul de vederi? | Nu lipsește. Toate interogările directe pe `Postare` din produs stau în `M/Cub/` (grep azi); `Contabil.Postari` filtrează central felurile de tranzacție (`M/Cub/Citiri/Contabil.cs:36-46`). |
| 6 | Notă pe 401 fără unitate, pe 371 fără lot | **Trec amândouă**, ca scenarii acceptate: `dn/scenarii/NTC.md:28, 41, 42`; codul la `M/Declaratii/DeclarantNotaContabila.cs:63`. Nu am găsit un invariant „sold cont = Σ unități” în `Invarianti.cs:12-59`. |
| 7 | Storno cu dependenți | Refuz. Sunt patru garduri separate, nu un mecanism (`M/Cub/Materializare.cs:72-76`, `M/Motor/MotorOperare.cs:649`). |
| 8 | Două bonuri concurente | Serializate printr-un blocaj global per bază (`X:267-279`). „Nu există blocaj pe lot și nici pe partidă” (`X:262`). |
| 9 | Taxa din cartea fiscală contra 4426 | Altfel decât presupuneam: faptul fiscal e chiar postarea contabilă, calificată (`M/Cub/Postare.cs:34-48`); cusătura cade prin construcție (`inv:425`). Notele manuale pe 4426 nu poartă coordonate de TVA (`NTC.md:14-15`), deci rulaj 4426 ≠ jurnal rămâne posibil. |
| 10 | Datele de producție | Nu există producție pe forma nouă; tăierea e fără migrare (`K:45`). |
| 11 | Divergențe reale | +585.404,66 pe 3xx (`dn/nucleu-bilant.md:51`). Întrebarea „cine are dreptate” e încă deschisă, mutată la migrare (`docs/decizii/restante.md:251`). |
| 12 | Linii | Nucleu 74 de linii de motor; declaranți 2.418; `M/Cub` 2.788 (dosar §4). Diferența din 09-19: +90.491/−9.719 în produs, +27.000 în unelte (`git diff --shortstat 9b040b4 a5df5fe`). |
| 13 | Antedatarea | Gardul verifică fiecare prefix zilnic (`M/Cub/Citiri/Loturi.cs:75-104`). Costul nu se reevaluează: „retro-ul lasă reziduu” (`K:336-338`). |
| 14 | Unitate-părinte | Nu există (`N/Cub/Unitate.cs:5-11`). Există `Suport`, o referință între postări (`N/Cub/Postare.cs:11`). |

## 2. Predicțiile bilanțului contra rezultat

| Rând | Verdict | Dovada |
|---|---|---|
| Corectitudinea modelului | **confirmat parțial** | Partida e unitate cu partener (`N/Cub/Unitate.cs:27-28`); stocul și 3xx sunt aceeași postare (`M/Declaratii/DeclarantFacturaIntrare.cs:140-146`). Nota reintroduce diferența cont ≠ Σ unități (§6). |
| Un mecanism în loc de patru | **confirmat parțial** | Storno unic (`N/Cub/Storno.cs:8-31`). Rămân trei tabele de snapshot (`M/BusinessObjects/Registre/SolduriPerioada.cs:7, 56, 81`), nu un `Sold`. 13 indecși, nu 7 (`InitialCreate.cs:3797-3807`, `20261003203845_IndecsiCititoriCub.cs:15-20`). FK-uri 5 și 6 pe partiție (`InitialCreate.cs:3773-3796`). |
| Izolarea motorului | **infirmat ca formulare** | Vezi mai jos. |
| Scrierea 1,7 ms | **neverificabil; indiciu contrar** | FCT: 78 de comenzi SQL, 38,9 ms; BCS: 48 și 25,3 ms (`perf:1117-1122`), în regim dual. Nu există cifră a registrelor pe aceeași bază. |
| Citirile lunii | **confirmat ca formă** | Jurnal, TVA, D300, D394: rânduri atinse constante în istoric (`perf:1053, 1067-1072`). |
| Citirile integrale, pierdere | **neverificabil** | Fără volum. |
| Balanța pe snapshot | **agravat la volum mic** | Snapshot 12,9 ms contra recitire 8,1 (`X:755-757`). Rapoartele API nici nu-l folosesc: citesc `Vizibila` și recitesc postările (`X:714-716`). |
| Disc 1,5–1,8× | **neverificabil** | Nemăsurat pe schema reală, care are 45 de coloane față de prototip. |
| Coordonate noi | **neexercitat** | Valuta și `Atribuit` ca mecanism sunt în TR-D9b (`K:153-154`). `Carte` e folosită la DVI și AMO. |
| Semantică vizibilă | **confirmat** | Lista crește: patru schimbări în tăiere (`K:159-172`), plus I2, I5, I6 (`inv:23-27`). |
| Disciplina `Fel = Transfer` | **rezolvată mai bine decât prevăzut** | Filtru central, nu disciplină per raport (`Contabil.cs:36-46`). A apărut o a doua: contraponderea de transformare (`M/Cub/Citiri/Transformare.cs:7-14`). |
| Volumul rescrierii | **depășit mult** | Prevăzut ≈3.450 + 6.384 de linii; diferența reală e de ordinul 90 de mii. |
| Ce nu s-a putut proba | **parțial închis** | Storno, imobilizări, DVI și DEC au cataloage (`dn/scenarii/`). Valuta și reevaluarea nu. |
| Produsul înghețat | **confirmat** | `docs/instructiuni-proiect/CLAUDE.md:113`. |

**Forma declarației fluxului.** Criteriul bilanțului era: dacă nu iese mai simplă decât cele 41 de hook-uri, „a mutat cuplajul, nu l-a desființat” (`nucleu-bilant.md:253-256`). Rezultatul e la mijloc, mai aproape de „mutat”.

- **Ce s-a obținut.** `IDeclarant.Declara(Operand) → Declaratie` e o funcție pură, fără `IObjectSpace` (`M/Declaratii/IDeclarant.cs:11-17`). Un tip se citește într-un fișier de 65–194 de linii, iar ipotezele și deciziile se persistă ca explicație (`M/Cub/Tranzactie.cs:18-22`). Câștigul e real pentru testare și audit.
- **Hook-urile nu au dispărut**, cum promitea `nucleu-bilant.md:95`. Azi sunt 14 override-uri `PregatesteOperare`, 21 `ValideazaOperare`, 8 `SensDeStins` (grep în `M/BusinessObjects`), plus 37 `Declarant()` și `Laturi()`. După tăiere rămân `PregatesteOperare` și `ValideazaOperare` (`K:179-181`), hook-urile de stingere (`K:97-99, 616`) și interfața registrului propriu, redenumită (`inv:22`).
- **Operandul e reuniunea nevoilor tuturor tipurilor**: 24 de membri, dintre care `Transformare`, `DiferentaInventar`, `Imobilizare`, `ReceptieSursa`, `Fise`, `Suporturi` servesc câte un singur tip (`M/Declaratii/Operand.cs:64-126`). Cuplajul tip↔motor s-a mutat în `M/Motor/Fapte.cs` (423 de linii).
- **Motorul nu decide nimic.** Convertește perechile în postări și verifică conservarea (`N/Motor/Motor.cs:4-32`). FIFO și costul le cheamă declaranții (`M/Declaratii/DeclarantAsamblare.cs:66`, `DeclarantNotaContabila.cs:72`). Regula stă în declaranți, în `Materializare` (692 de linii în cinci fișiere) și în `ReceptiiConexe` (235).
- **Declarația e pe perechi debit–credit.** Stilul funcțional n-a cerut cubul: aceeași declarație putea alimenta registrele. Observația din runda 1 §6 se confirmă.

## 3. Cazurile de probă pe cod real

| Caz | Registre | Cub | Unde se greșește în tăcere |
|---|---|---|---|
| **1. FCT** | Plan în trei pași: note pe regulă (`MotorOperare.cs:152-213`), TVA (`:222-281`), `RegistruTva` (`:289`). Stocul nu postează pe FCT, ci pe NIR-ul conex (`:133-134`). Cititorul trebuie să știe `RegulaContare`, `PoliticaTva`, `PoliticaConex`, `RegulaStoc` și patru hook-uri. | Un fișier de 194 de linii; linia de stoc e recepția. Concepte: `Capat`, `Miscare`, unitate, gestiune virtuală, partidă, `Fiscal`. Caz special: capitalizatul despărțit în două mișcări (`DeclarantFacturaIntrare.cs:176-193`). | Cub: contul furnizorului la recepție vine din politica de TVA sau din prima regulă de serviciu ori cheltuială (`:107-128`), o politică împrumutată. Recepția nu are gard de analiză (`inv:23`, I2). Registre: stoc ≠ 3xx. |
| **2. NIR cu diferență** | Clonă conexă cu `PreiaLinieConexa`; cumulul stă pe NIR. | Delta față de recepția facturii (`M/Declaratii/DeclarantNir.Diferenta.cs`, 97 de linii) plus `ReceptiiConexe.cs` (235). Cauza nu e coordonată: e politică, text în decizie (`:61-66, :90`). | Cub: comparația cere regrupare pe „capul” recepției (`Loturi.cs:128-137`). E cel mai greu caz de citit pe ambele căi. |
| **3. ASM** | Două reguli de stoc, validare valorică în frunză (`M/BusinessObjects/Documente/Asamblare.cs:93`). | Primitivă nouă în nucleu (`N/Motor/Transformare.cs:13-33`), conservare dedicată (`N/Conservare.cs:26-42`), postări-contrapondere filtrate în fiecare citire contabilă. 124 de linii. | Azi declarantul balansează pe soldul **registrului** (`DeclarantAsamblare.cs:9, 63-71, 78-84`). Tăierea schimbă gardul la P = C (`K:286-311`): comportament nou, nerulat încă. |
| **4. NTC** | Postare explicită, opt-in pe tipul documentului (`MotorOperare.cs:160-171`); un rând pe linie. | 87 de linii, FIFO automat pe partide (`DeclarantNotaContabila.cs:63-84`). | Vezi §6. |
| **5. Storno** | Trei bucle și registrul propriu (`MotorOperare.cs:663-716`). | `Storno.Inverseaza` plus selecția cauzat ∪ atribuit (`Materializare.cs:77-115`). | Cub: stornoul fiscal e **rescris** după scriere (`Materializare.cs:213-222`, chemat din `M/Motor/CorectieService.cs:95-103`). Defectul 4 din bilanț §2.1 nu e reparat. |
| **7. Custodie** | Neacoperită. | Refuzată (`dn/scenarii/LDI.md:38`). Nota nu postează cantitate (`NTC.md:14`). | Niciuna nu acoperă cazul. Predicția mea „un rând de politică” e neprobată. |

**Sinteză.** Pe cub un tip se citește într-un singur fișier, dar cere vocabularul nucleului: 6–8 concepte. Pe registre cere patru tabele de politică și hook-urile, cu vocabularul contabilului. Raportul cod/politică s-a mutat spre cod: recepția FCT, ASM, NTC, PIF, AMO, CAS, ITV, DVI și BTR nu mai contează prin reguli. O regulă adăugată de client pe aceste tipuri nu mai are niciun efect după tăiere (`inv:26`, I5). Asta contrazice principiul proiectului „politica rămâne date”.

## 4. Opțiunea B, concret

**Constatarea de bază: B există deja, în măsura în care e util.**

- Stratul logic e `M/Cub/Citiri/`: `Contabil`, `Loturi`, `Partide`, `Fiscale`, `Imobilizari`.
- Stratul materializat sunt cele trei snapshot-uri lunare, derivate la închidere, cu reconstrucție măsurată la 119 ms (`perf:1066`).

Ca registre derivate drept cale principală ar însemna:

1. **Jurnal și fișă, pe postare.** Forma pe perechi nu se poate deriva determinist, fiindcă lipsește cheia (§1.1).
2. **Stoc**: lot × gestiune × zi.
3. **Partide**: unitate × document × zi, cu originea.
4. **Fiscal**: un rând pe linie.
5. **Fișa mijlocului fix.**

Regim: derivare la închidere și citire vie pe luna deschisă. Derivarea sincronă s-ar adăuga la 48–111 comenzi SQL pe document, sub blocajul global (`perf:1117-1122`, `X:267-275`).

Trei fapte sunt contra:

- Rapoartele API ocolesc azi cache-ul existent din cauza securității pe rând (`X:714-716`). Un registru derivat ar avea aceeași problemă.
- La volumul măsurat, cache-ul e mai lent decât recitirea (`X:755-757`).
- Cititorii pe interval sunt deja independenți de istoric (`perf:1053, 1067-1072`).

Singurul cititor care pică criteriul de formă e `PartideCuRest`: 2.242 / 3.366 / 4.504 rânduri atinse, din cauza căutării documentului deschizător (`X:796-802`). Remediul e un **satelit al unității** (unitate → document deschizător, dată, fel), nu un registru. Același satelit îl cere și ALOP (§8).

**Ce s-ar păstra din ce șterge TR-D9a:** nimic ca entitate. `RegistruContabil` pe perechi nu e derivabil, iar `RegistruStoc` are cheia `TipStoc` × repartitor. B ar fi cod nou integral, 2–4 săptămâni cu probele [judecată].

## 5. Opțiunea C, concret

**Costul întoarcerii** [judecată]: 4–6 săptămâni.

- Toate citirile, SAF-T și snapshot-urile sunt pe cub, iar ruta SAF-T pe registre e scoasă (`docs/instructiuni-proiect/CLAUDE.md:136-137, 156-157`).
- Catalogul de scenarii e scris pe semantica cubului: partide ca unități, recepția pe FCT, delta pe NIR.
- S-ar arunca nucleul, declaranții și `M/Cub`: 9.257 de linii (dosar §4).

**Defectele din bilanț §2.1, rămânând pe registre:**

1. *Partida ca document*: reparabilă numai cu un al cincilea registru, de partide cu identitate proprie. Asta ar însemna jumătate din cub. E singurul defect care cerea cu adevărat forma nouă.
2. *Stoc ↔ contabil nereconciliat*: detectabil ieftin, printr-o probă Σ `RegistruStoc.Valoare` contra soldului 3xx. Legătura pe linie există pe ambele registre (`M/BusinessObjects/Registre/Registre.cs:36, 158`). Bilanțul însuși spune că proba nu exista (`nucleu-bilant.md:51-52`). Nu devine imposibil structural, dar nici în cub nu mai e (§6).
3. *Partener = repartitorul laturii*: reparabil cu o coloană de partener pe latură, scrisă de plan.
4. *Rânduri rescrise*: nereparat nici în cub. Remediul e același în ambele forme: rând nou, nu rescriere.

„Taxa per linie imposibilă azi” e slab: `RegistruTva` e deja pe linie (`Registre.cs:197-199`). Trei din patru defecte nu cereau cubul. Asta nu justifică întoarcerea, fiindcă prețul e deja plătit.

## 6. Portița

**Regula din design nu e regula din cod.** Designul spune: nota „nu atinge o coordonată cu unitate fără să numească unitatea” (`dn/nucleu-cub-design.md:177-179`). Codul admite poziția fără unitate, în trei regimuri diferite:

- **Partidă**: fără partener explicit, nota postează pe 401 fără partidă (`DeclarantNotaContabila.cs:63`; `NTC.md:28, 42`). O probă de ModelCheck o tolerează numai dacă nici laturile documentului nu sunt terți (`M/Cub/Citiri/Partide.cs:193-204`).
- **Lot**: 628 = 302 fără lot e acceptat (`NTC.md:41`), fără niciun gard.
- **Fișă**: poziția fără fișă e admisă, dar n-are voie să devină negativă (`M/Cub/Materializare.PozitieFaraFisa.cs:33-63`).

Relaxarea e corectă față de date: 7.816 picioare pe conturi de terț fără repartitor și 3.365 pe 3xx fără lot (`NTC.md:73-77`). Dosarul și textul sarcinii o prezintă totuși drept „regula de azi”.

| Varianta | A, azi | C |
|---|---|---|
| (a) fără analitic | Două postări, fără unitate. | Un rând. Efect identic. |
| (b) 401 cu partener | FIFO **automat** pe partidele partenerului sau partidă proprie (`DeclarantNotaContabila.cs:65-83`). Operatorul nu alege partida; împerecherea ulterioară poate cădea pe `IMPERECHERE_AMBIGUA` (`M/Cub/Transferuri.cs:73-74`). | Notă plus împerechere separată, cu hook (`M/BusinessObjects/Documente/NotaContabila.cs:64`). |
| (c) 371 cu lot | **Imposibil prin notă**: capătul nu primește lot (`DeclarantNotaContabila.cs:56-60`). Merge fără lot, deci sold 3xx ≠ Σ loturi. Reevaluarea e în TR-D9b (`K:153`). | Identic: nota mișcă 371, registrul de stoc nu. |

În B, proiecția ar trebui să ducă poziția fără unitate într-un rând „fără lot” ori „fără partidă”; altfel pierde total.

**Judecata.** Avantajul lui A pe portiță, criteriul pe care am respins C în runda 1, e azi realizat numai pe (b). Pe (a) și (c) cele două forme se comportă la fel. Diferența care rămâne e de detectabilitate: în cub restul e un filtru `Unitate IS NULL` pe același tabel, în registre e o reconciliere între tabele. Regula trebuie rescrisă cum e: „soldul contului = Σ unități + poziția fără unitate”, cu raport și cu gard de semn și pe 3xx.

## 7. Viteza

Nu există nicio cifră după implementare care să compare cubul cu registrele. „A/B”-ul din X-D5 compară cubul cu snapshot contra cubului recitit (`X:246, 755`). Prototipul de volum avea rânduri de 182 B și 7 indecși (`nucleu-bilant.md:16, 154`); schema reală are 45 de coloane și 13 indecși. Cifrele din `nucleu-fizica.md` nu mai descriu sistemul.

- **A — rămân deschise fără schimbare de model:** numărul de comenzi SQL pe document (48–111; cauza probabilă e coaja ORM, nu forma [judecată]); sub-partiționarea pe an; snapshot-ul cu cheie de vizibilitate; satelitul de unitate; rafinarea blocajului global (`restante.md:375`). **Se închid:** indexarea pe perechi și integritatea prin schemă pe coordonate (rămâne în cod).
- **B:** aceleași, plus tabele înguste pe raport. Se închide scrierea simplă.
- **C:** se închide tot ce depinde de partenerul pe postare: fișa × partener 719 → 33 ms în prototip (`nucleu-bilant.md:119`).

Blocajul global costă 19–39 ms pe comandă (`X:771-772`), deci plafonul e de ordinul 25–50 de comenzi pe secundă pe bază. Limita e acceptabilă pentru o bază per client, dar ține de regula de concurență aleasă, nu de model.

## 8. ALOP

Din legacy am citit numai structura, nu logica: 30 de fișiere, 12.175 de linii `.pas`. Tabelele sunt `alop_angajamente` și `alop_ordonantare`, fiecare cu `_defalcare`, plus `alop_ordonantare_lichidare` și `alop_dispozitie`. Câmpurile includ `RAMAS_DE_ANGAJAT`, `TOTAL_ANGAJATE`, `ordonantat`, `SUMA_PLATA` și clasificația (grep în `legacy/Buget/Alop*.dfm`). Fluxul: credit → angajament defalcat pe clasificație → ordonanțare, cu lichidare → plată, fiecare fază cu rest față de precedenta. În noul cod există `Angajament` ca dimensiune (`M/BusinessObjects/Nomenclatoare/DimensiuniBugetare.cs:46`) și `AngajamentId` pe linie (`M/BusinessObjects/Documente/Document.cs:448`).

**În A.** „Nicio tabelă nouă” ține probabil; „prin declarație” nu ține. Cere:

1. Un fel nou de unitate. CHECK-ul are felurile bătute în cuie (`InitialCreate.cs:3760-3764`), iar unitatea se reconstruiește numai din cont, partener și produs (`M/Cub/Randuri.cs:100-113`). Un angajament are identitate pe clasificație, deci cere **satelit de unitate**.
2. Partidă simplă în clasa 8. Conservarea cere debit = credit pe carte (`N/Conservare.cs:71-81`), deci fie un cont tehnic de contrapondere, fie o carte „Buget” cu regulă proprie.
3. Legătura fază → fază. `Unitate` nu are părinte; `Mutare` cere același cont (`N/Motor/Mutare.cs:17-20`). Se poate cu `Suport`, ca la imobilizări.
4. Un gard de depășire scris de mână. Fiecare fel de unitate are azi gardul lui (`Loturi.cs:75`, `Materializare.cs:188`, `PozitieFaraFisa.cs:33`); acesta ar fi al patrulea.

**În B:** A plus un registru de execuție bugetară (clasificație × fază × lună). E singurul loc unde un registru derivat ar avea sens propriu, fiindcă raportul e legal și are formă fixă.

**În C:** registru nou cu solduri, storno și împerecheri între faze. Precedentul de cost e registrul propriu al imobilizărilor, 11 metode (`K:206`).

## 9. Ce omite sau înclină dosarul

- Spune „regula de azi” a portiței fără să spună că trei scenarii acceptate o încalcă.
- Nu spune că termenul vechi de comparație a pierdut deja acoperire: ASM pe conturi diferite e „cub 40, registre 0” (`X:783`).
- Nu spune că TR-D9a schimbă comportament: patru schimbări declarate (`K:159-172`), plus I2, I5, I6.
- §6 numește „perf A/B” o măsurătoare care nu compară căile. Recunoaște doar pe jumătate.
- Omite blocajul global al scrierii și rescrierea postărilor fiscale la storno.
- Omite mărimea reală a rescrierii față de predicție.
- Omite că +585 k pe 3xx rămâne netranșat, iar martorul intern dispare la tăiere.
- §4 dă linii pe zone, dar nu ModelCheck: 46.887 de linii, cu oracolul de 3.293 (`K:119-128`).

## 10. Livrabile

### 10.1 Verdictul pe cele șapte criterii

| Criteriu | Runda 1 | Runda 2 | Dovada |
|---|---|---|---|
| Coerență | A > B > C > D | **A > C, cu marjă mai mică** | Stoc = 3xx pe document e structural. Prin notă nu e (§6). |
| Simplitate | A > C > B | **A ≈ C** | Un fișier pe tip, dar două primitive noi, contraponderi, 47 de ramificări și hook-uri rămase (§2, §3). |
| Reprezentarea tipului | A = B > C | **A > C la citire, C > A la configurare** | I5: regulile clientului mor pe opt tipuri. |
| Portița | A > B > C | **A ≥ C** | Diferență reală numai pe (b). |
| Optimizări | A = B > C | **A > C, B nejustificat** | §4, §7. |
| Audit | A > B > C | **A > C, întărit** | Explicația persistată (`Tranzactie.cs:18-22`), `InversaDin`, cauza pe linie. |
| Reversibilitate | B > A > D > C | **A slabă** | Fără cheie de pereche, registrele nu se refac din cub; tăierea e fără migrare. |

### 10.2 Ce mi-am schimbat și de ce

- **„A face imposibil rest ≠ cont și stoc ≠ cont”: fals pe cod.** Corect e că le face detectabile într-un singur tabel.
- **„Depășirea e sold negativ, deci refuz”, ca mecanism generic: fals.** Fiecare fel de unitate are gard scris separat.
- **ALOP „prin declarație”: nu.** Cere fel de unitate, satelit, regulă de carte și gard.
- **Condiția 1 din runda 1, cheia de corespondență, nu e îndeplinită.** O mențin drept cerință.
- **Recomandarea mea „registre ca vederi”: există deja** ca strat de citire. Materializarea selectivă a fost măsurată și respinsă pe cifră. Renunț la ea.
- **Nu mi-am schimbat alegerea A.** Am căutat dovada că declarația e mai grea decât hook-urile, sau că citirile pică pe formă. Am găsit doar `PartideCuRest`, care e reparabil.

### 10.3 TR-D9a: se amendează

1. **Cheie de pereche pe `Postare`**: un ordinal al mișcării în tranzacție, scris în `Motor.Opereaza`. Acum costă cel mult o zi, fiindcă schema se resetează oricum la pasul 7 (`K:666`). După primele date reale devine migrare.
2. **Regula portiței, rescrisă cum e**, în decizia 110 și în `invarianti.md`: poziția fără unitate e rest legitim; raport „sold cont = Σ unități + fără unitate”; gard de semn pe 3xx fără lot, după modelul fișelor.
3. **`ReatribuieInversaFiscala`**: fie se înlocuiește cu rând nou, fie afirmația „niciun fapt nu se atinge” iese din documente și rescrierea se declară excepție.
4. **I2 și I5 se decid înaintea pasului 2**, cum cere deja inventarul (`inv:23, 26`). La I5 recomand refuzul regulii la editare. Altfel politica editabilă minte în tăcere.
5. **Pasul 6 nu pornește înaintea măsurătorii de volum de mai jos.**

### 10.4 De măsurat sau prototipat înainte

| Ce | Zile |
|---|---|
| Scara `PerfCub` la 5–10 milioane de postări pe schema reală: fișa, balanța, partidele cu rest, operarea FCT și BCS, mărimea tabelei și a indecșilor | 2–3 |
| O ultimă reconciliere registre ↔ cub pe clona Flax, sub regimul dual, arhivată; ultimul moment pentru +585 k cu martor intern | 1–2 |
| Schiță ALOP pe hârtie cu satelit de unitate și carte „Buget”, verificată contra `Conservare` și `Randuri`, înainte ca TR-D9b să fixeze forma unității | 2 |
| Cheia de pereche, cu jurnalul în perechi probat pe FCT cu TVA și pe o notă N:M | 1 |

### 10.5 Încrederea: 3 din 5

Ce o limitează:

- Nu am rulat nimic.
- Nu există nicio cifră de volum pe schema reală.
- Nu am citit `Fapte.cs`, `GardianEditare.cs`, SAF-T și clientul.
- Din catalog am citit integral numai NTC.
- ALOP l-am văzut la nivel de tabele și câmpuri, nu de logică.
- Costurile în săptămâni sunt estimări, nu măsurători.
