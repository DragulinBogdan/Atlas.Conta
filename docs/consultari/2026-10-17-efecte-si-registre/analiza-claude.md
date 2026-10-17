# Analiză: registrul contabil, cubul și cele două noțiuni noi (agregare, istoric)

Tip: analiza
Data: 2026-10-17
Stare: răspuns la `sinteza-si-intrebari.md`; a dus la decizia 131 (gard acum, primitiva la primul consumator; proba amânată ca 131-r1)
Autor: Claude (sesiunea de după „rotund”, 130)
Răspuns la: sinteza Codex din 2026-10-17; reia consultarea `docs/consultations/2026-10-05-cub-vs-registre/` pe modelul rotund

## 1. Teza owner-ului, verificată pe cod

**„Registrul contabil este un subset al cubului.”** Adevărat, și e deja
litera lui 090 (a): cubul = registrul contabil (cont × latură × analiză) ∪
stoc (cantitate pe lot) ∪ fiscal (`Carte=Fiscal`, `CodTva`) ∪ partide
(`Unitate`). Registrul contabil al lui 1C (`AccountingRegisters/General`,
Dt/Kt + subconto) e proiecția cubului pe `Carte=Contabil` fără unitate și
cantitate. Nu e o noțiune de adăugat; e una pe care o avem.

**„Adaugă două noțiuni noi: agregare și istoric.”** Una din două e nouă.

- **Agregarea** există, sub alt nume: snapshot-urile de referință
  (`SolduriPerioadaContabil`, `SolduriPerioadaStoc`, `PartideDeschise`,
  `Motor/SolduriService.cs:91-101`) și lema `Sold(≤t) = Sold(≤t0) +
  Sold(t0<d≤t)` (N-D9). Diferența față de 1C e doar CÂND se scrie
  totalul: 1C întreține tabelele de totaluri la fiecare postare (lunar,
  automat), Atlas le scrie la închiderea perioadei. Al doilea read model a
  fost măsurat și respins cu cifre (FZ-r1 închisă, X-D5: balanța din
  snapshot 12,9 ms contra 8,1 ms recitită la 12 luni). Nu e un subiect
  deschis; redevine subiect doar pe un istoric lung (FZ-r4, FZ-r6, după
  PoC).
- **Istoricul** (valoarea valabilă la o dată a unui fapt NEADITIV) e
  noțiunea cu adevărat lipsă. Cubul adună; nu poate spune „de la 1 martie
  durata e 22 de luni”. Azi Atlas o spune în trei feluri, scrise de mână:

| Codificarea de azi | Unde | Ce costă |
|---|---|---|
| Postări cu valoare zero ca ancore de eveniment + coloanele tipate ale liniei | revizuirea PIF: `tools/ModelCheck/ScenariiImo.cs:281-284` cere exact două postări cu `Valoare == 0`; cititorul `Cub/Citiri/Imobilizari.cs:30-88` grupează postările pe (fișă, document, linie), apoi citește `PunereInFunctiuneDetaliu`, `AmortizareLunaraDetaliu`, `IesireImobilizareDetaliu` (liniile 41-56) și aplică semnul stornoului (66) ca să reconstruiască „ultimul eveniment viu” | un registru de informații implementat într-un cititor de cub: trei tabele tipate citite pe id, semantica „retragere” simulată prin inversa de zero |
| Stare mutabilă pe nomenclator, scrisă de motor | `Imobilizare.Stare`, `DataPunereInFunctiune`, `DataIesire`; locul fișei se schimbă FĂRĂ document, „istoricul locului este pe rândurile lunare” (`docs/stare-curenta/imobilizari.md`, 87a/87h) | fără istoric propriu; locul la o dată se deduce din AMO-uri, deci lipsește între două amortizări; abatere de la invariantul I |
| Politici cu valabilitate, fiecare cu codul ei | `TipTva.ValabilDeLa/PanaLa` (`Nomenclatoare/TipTva.cs:27-29`), `PoliticaTvaImplicit.ValabilDeLa` (`Motor/Potrivire.cs:169-184`), `RegulaDeductibilitate.DeLa/PanaLa` (`Politici/Politici.cs:747-749`); `RegulaContare`/`PoliticaAmortizare` n-au istoric, doar contorul `OptimisticLockField` memorat ca `VersiunePolitica` în explicație (D9-r3 deschisă) | trei implementări ale aceleiași întrebări „ce era valabil la data d”; explicația de audit nu poate reda conținutul regulii de atunci |

**„Să evităm să trecem toate tranzacțiile prin regula strictă a cubului.”**
Aici răspunsul e nuanțat. Regula strictă (conservare, forma coordonatelor,
pereche, unitate, perioadă, storno ca inversă) costă doar pentru fapte care
NU sunt mișcări. Pentru mișcări ea e exact ce a eliminat evidențele
paralele: pe Flax, două registre de acumulare pentru aceeași valoare
(contabilul 3xx și stocul) au divergat cu 585.404,66 (090 g, TR-r12), iar
1C ține baza și TVA-ul facturii în `General`, `TVACumparari` și
`Aprovizionari` simultan, cu coerența lăsată algoritmului fiecărui
document. Ce trebuie scos de sub regula cubului nu sunt tranzacțiile, ci
faptele neaditive care azi se deghizează în postări de zero.

## 2. Cele trei familii ale lui 1C, judecate pe catalogul Atlas

| Familia 1C | În Flax | Echivalentul Atlas | Consumatori concreți din `docs/functionalitati.md` | Verdict |
|---|---|---|---|---|
| Registru contabil | 1 (`General`) | proiecția cubului pe `Carte=Contabil` | toate | avem |
| Registre de acumulare | 13 (`Loturi`, `TVACumparari/Vanzari`, `Aprovizionari`, `Vanzari`, `CosturiVanzari`, `Amanunt`, `DecontariCuAngajati`, `ProductiaRealizata`…) | partițiile cubului (lot, partidă, fișă, fiscal) + proiecții (`Cub/Citiri/*`) | niciunul care să nu fie sumă pe cub: angajamentele bugetare (F-BUG-01) sunt conturi din clasa 8; vânzările/costul vânzărilor sunt proiecții pe 7xx/6xx cu lot; rezervările nu sunt în catalog | **nu se adaugă**: ar redeschide 110 (tăierea celor patru registre), fără cerere |
| Registre de informații | 85, din care ~20 de domeniu (`CursValutar`, `PreturiNomenclator`, `ParametriiDeAmortizareMF`, `StareaMFOrganizatii`, `LoculDeAmplasareMF`, `EvenimentMFOrganizatii`, `ConturiDeEvidentaNomenclator`, `Impozite`, `CoteleReglementate…`, `LimitaDeInterzicereModificareDate`, `Rezervari`, `StocMinim`, `Angajatii`…), restul configurare/infrastructură | cele trei codificări de mână din §1 | parametrii și locul fișei (azi), F-VAL-01 (curs valutar: registrul de informații clasic), F-IMO-04 (reevaluare, depreciere: valoare la dată pe fișă), D4-r1 (istoricul statutului de TVA al partenerului), F-SAL-01 (salarii: zeci de fapte datate per angajat), D9-r3 (istoricul politicilor) | **o singură primitivă lipsă**, cu șase consumatori |

Concluzia de structură: din cele „trei planuri” ale sintezei, al treilea
(acumulare) nu are consumator și ar reintroduce ce am tăiat, iar al doilea
(informații) are consumatori și azi e făcut de mână în trei feluri. Nu
„cub ȘI registre specializate”, ci **cub + un fel de fapt datat**.

## 3. Variantele

- **V0, status quo formalizat.** Postarea de zero devine idiom declarat
  („eveniment pe unitate”), schimbarea locului fișei devine document (cum
  cere invariantul I), cititorii per domeniu rămân. Cost mic. Nu rezolvă
  cursul valutar, statutul de TVA, salariile sau istoricul politicilor:
  niciunul n-are unitate pe care să ancoreze un zero.
- **V1, faptul datat ca a doua destinație a operării.** O primitivă:
  `cheie tipată → resurse tipate, valabil de la (dată), cauză (document +
  linie) sau fără document (nomenclator/politică), retras de (stornoul
  cauzei), ScrisLa`; citirea = „valabil la data d” și „istoricul cheii”.
  Fiecare fel de fapt e o clasă/tabelă proprie (structura e cod,
  invariantul IV), cu o bază comună și un cititor generic, ca `Dimensiune`.
  Declarantul întoarce, pe lângă `Declaratie`, lista de fapte; `Materializare`
  le scrie în aceeași comandă. Nucleul pur nu le cunoaște (rămân în
  Module, lângă frunze). Un document poate avea numai fapte (revizuirea):
  amendează 090 (a) „exact o tranzacție `Operare`”, invariantul I („ce nu
  postează nu e operație”) și `Declaratie.cs:118-122` („cel puțin o
  mișcare”).
- **V2, cele trei familii ale lui 1C.** Registre de acumulare proprii pe
  domeniu, cu storno, perioadă și snapshot specializate. Redeschide 110
  fără consumator; dublează infrastructura de corecție. Respins pe
  catalog, nu pe principiu.

**Recomandare: V1, condiționată de probă.** Argumentele contra, luate în
serios: (1) V1 e o a doua „sursă de adevăr” lângă cub, deci regula
„efect primar vs proiecție” din sinteză trebuie să fie gard, nu îndemn:
un fapt intră în registrul de informații DOAR dacă nu e sumă pe cub;
(2) amendamentul invariantului I e real și trebuie scris, nu subînțeles;
(3) stornoul, perioada închisă și dependenții capătă o a doua semantică
(retragere, nu negare), chiar dacă gardurile sunt aceleași; (4) până la
F-VAL-01 și F-SAL-01, singurul câștig măsurabil e pe imobilizări, deci
dacă proba nu arată simplificare ACOLO, V1 se amână, nu se adoptă „pentru
viitor” (generalizare ancorată).

## 4. Proba propusă (Codex, workspace separat)

Cazul: parametrii de amortizare (revizuirea), apoi locul fișei. Criteriile
se fixează ÎNAINTE de cod; proba e comparativă, pe aceleași scenarii.

Ce se construiește:
1. `FaptDatat` (bază) + `ParametriAmortizare` (fapt tipat: fișă → metodă,
   durată, reziduală, metodă fiscală, durată fiscală, categorie, exclusivă)
   + `LocImobilizare` (fișă → loc), cu `ValabilDeLa`, `Cauza`, `RetrasDe`.
2. Declarantul PIF emite fapte pentru intrare/modernizare/revizuire;
   revizuirea NU mai emite postări; `Materializare` le scrie în aceeași
   comandă; stornoul PIF le retrage; `GardianPerioada` judecă `ValabilDeLa`.
3. `Imobilizari.Randuri` și `AmortizareService.Situatie` citesc parametrii
   din fapte (valabil la data), nu din liniile tipate; locul amortizării
   lunii se citește din `LocImobilizare`.

Criterii de succes (toate):
- SC-IMO integral verde pe ambele profiluri, cu SC-IMO-11 rescris:
  revizuirea produce ZERO postări; inversa restabilește durata 12.
- `Cub/Citiri/Imobilizari.cs` pierde cele trei citiri pe tabelele liniilor
  (41-56) și semnul stornoului (66); `AmortizareService.Calcul` nu crește.
- Nucleul (`Atlas.Conta.Nucleu`) nu se schimbă; `Declaratie`/`Motor` rămân
  cum sunt, faptele trec pe lângă, în Module.
- Stornoul, anularea, perioada închisă și dependenții (87e/f/g) folosesc
  gardurile existente plus O coloană (`RetrasDe`) și un filtru pe dată;
  nicio tabelă de snapshot, nicio a doua formă de storno.
- Locul fișei are istoric (fapt per schimbare) cu ≤ 40 de linii noi peste
  primitiva comună, iar „schimbarea fără document” dispare sau devine
  document.
- Bilanț de linii raportat separat pe: primitivă (cost fix), imobilizări
  (câștig sau pierdere), probe.

Criterii de eșec (oricare):
- primitiva cere storno/perioadă/snapshot proprii dincolo de cele de mai sus;
- Nucleul primește un comutator pe felul faptului sau o a doua `Declaratie`;
- cititorii imobilizărilor nu scad, doar se mută;
- apar două surse pentru aceeași valoare (de exemplu durata și pe linie,
  și în fapt, ambele citite).

Regula de oprire: o singură rulare ModelCheck o dată, pe baze cu sufix
propriu (`MODELCHECK_BAZA_SUFIX`), în worktree; buget de o zi de lucru;
raportul conține și argumentele contra, nu numai bilanțul.

## 5. Răspunsuri scurte la cele zece întrebări ale sintezei

1. Cost transversal real azi: imobilizările (parametri și loc). Viitor:
   F-VAL-01, F-IMO-04, D4-r1, F-SAL-01, D9-r3. Niciunul pe acumulare.
2. Postările zero sunt compromis funcțional, dar nu generalizabil: merg
   doar unde există unitate; locul fișei a scăpat deja de sub ele.
3. În cub rămâne tot ce e sumă: valori, cantități, TVA, partide, fișe
   (brut, amortizare, fiscal). Niciun fapt aditiv nu iese din cub.
4. Proiecție derivată: tot ce e azi în `Cub/Citiri/`. Fapt distinct:
   parametri de amortizare, loc, curs, statut TVA, politici cu istoric.
5. Da, dacă faptele sunt a doua ieșire a declarantului și `Materializare`
   le scrie; coordonatorul nu se schimbă per fel de fapt (proba o verifică).
6. „Valabil la d” = ultimul fapt neretras cu `ValabilDeLa ≤ d`, departajat
   pe (`ValabilDeLa`, `ScrisLa`); retragerea e marcaj, nu ștergere; două
   fapte cu aceeași dată și cauze diferite: câștigă ultimul scris, iar
   proba trebuie să-l declare, nu să-l ascundă.
7. Costul ieșirii se decide o dată (decizie în contract) și alimentează
   postările; faptele nu poartă valori care sunt sume pe cub (gardul 4).
8. Comun: tranzacția comenzii, gardianul perioadei, cauza, dependenții.
   Specializat: semantica retragerii (fapt) contra inversei (postare).
9. Amendamente: invariantul I (urma = postare SAU fapt datat cu cauză),
   090 (a) (document cu fapte și fără tranzacție), III neschimbat (faptele
   nu sunt agregare). Garanții păstrate: append-only, perioada, cauza.
10. Măsurabil: criteriile din §4. Adoptare = toate verzi; limitare =
    primitiva ține, dar imobilizările nu scad (atunci rămâne V0 până la
    F-VAL-01); respingere = oricare criteriu de eșec.

## Limite

Analiza e pe codul din main `1fb0405b` și pe metadatele Flax; nu s-a rulat
nimic. Cifrele de perf citate sunt cele din registrul de restanțe (X-D5),
nu remăsurate. Catalogul 1C de registre a fost clasificat după nume, nu
după conținutul XML, cu excepția celor citate de sinteza Codex.
