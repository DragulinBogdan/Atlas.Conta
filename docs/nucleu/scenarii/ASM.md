# ASM — asamblare

2026-09-23; adus la zi 2026-10-07 (TR-D9a, pasul 6: gardul ΣP = ΣC, fără absorbție). **Aprobat, implementat și verificat; review advers aplicat.** Forma exactă a postărilor este
în [contractul transformării](../tr-d7b-asm-transformare-contract.md),
ASM-B2 și ASM-B3; absorbția ASM-B6 și ținta ASM-B7 sunt înlocuite de D9-D3
din [contractul TR-D9a](../tr-d9-taierea-contract.md).
Reguli: 090 (f/g/j), TR-D7b T-D2, amendamentul 091 (a–d).

## Recensământ

`recensamant-asm-ldi-nir.sql`, tranzacție read-only pe clona TrD7b:
1.228 documente, 4.694 linii, toate bidirecționale și balansate valoric.
541 documente au mai multe consumuri, 113 mai multe produse, 367 mai multe
conturi. 1.225 nu conservă cantitatea pe fiecare produs real; 537 nici
cantitatea totală. Zero documente întârziate sau autogenerate.
Conturile observate: 3021, 3028, 303, 371, 381. Nu se deduce politica din ele.

## Scenarii

Rândurile SC-ASM-01…25 sunt implementate pe ambele profiluri unde politica
permite conturile. Conturile numerice sunt exemple private; fixture-ul
bugetar folosește conturile echivalente ale politicii, cu aceleași măsuri.
Proba este `ScenariiAsm`, cu ID-urile de mai jos. SC-ASM-22/24 și contul lipsă
din 13 sunt probate și pe operandul închis (`ProbeAsmOperand`); refuzul atomic
al contului lipsă pe operare este verificat în scena SAF-T S.
SC-ASM-26 e fără obiect după tăiere. Proveniența este regula, dacă nu este indicată
altfel. Contraponderile din Transformare au valoare 0 în toate exemplele.

| ID / probă | Scenariu și așteptare după comandă | Rezultat așteptat | Proveniență |
|---|---|---|---|
| SC-ASM-01 | LA: A 2/100 → LB: B 1/100 pe același cont. Transfer cu cele 4 postări din ASM-B2; LA 0/0, LB 1/100; rulaj contabil 0. | acceptat | regulă; 1.225 transformări de produs observate |
| SC-ASM-02 | LA: A 2/60 și LB: B 3/40 → LC: C 1/100, același cont. 6 postări; LA/LB 0/0, LC 1/100; valoare totală 100. | acceptat | recensământ: 541 cu consumuri multiple |
| SC-ASM-03 | LA: A 1/100 → LB: B 2/60 și LC: C 4/40. 6 postări; LA 0/0, LB 2/60, LC 4/40. | acceptat | recensământ: 113 cu produse multiple |
| SC-ASM-04 | A 2/100 pe 301 → B 1/100 pe 345. Operare, D 345/C 301: 100, 4 postări exacte; cantități reale −2/+1. | acceptat | regulă; 367 cu mai multe conturi |
| SC-ASM-05 | Grup balansat 371: 60→60; grup 301→345: 40. O Transfer de 60 și o Operare de 40; 8 postări totale. | acceptat, ASM-B3 | review |
| SC-ASM-06 | 371 consum 60, produs 100; 301 consum 40. Operare D 371: 100/C 371: 60/C 301: 40; 6 postări, fără Transfer. | acceptat, ASM-B3 | review |
| SC-ASM-07 | Storno SC-ASM-01 în ianuarie: original intact, inverse exacte; LA din nou 2/100, LB 0/0. Repetarea stornoului refuzată fără efecte. | acceptat / refuz stare | regulă |
| SC-ASM-08 | SC-ASM-05 în ianuarie; închidere; storno în februarie. O Storno inversează ambele feluri și toate contraponderile; soldurile la 31.01 rămân cele de după ASM, cele din februarie revin la inițial. | acceptat | regulă |
| SC-ASM-09 | Anulare SC-ASM-05 în luna deschisă: zero tranzacții/postări ASM, sursele restabilite, zero stoc pe loturile produse; reoperare produce exact aceleași măsuri. Existența lotului de culegere pe draft se verifică separat de sold. | acceptat | regulă |
| SC-ASM-10 | După închiderea lui ianuarie, corecție: inversarea 100, draft legat; noul ASM consumă 1/50 din LA și produce B 1/50. După operare: LA 1/50, produsul nou 1/50. | acceptat | regulă |
| SC-ASM-11 | Sursă 3/10: consum 1→3,33, produs 1/3,33; apoi rest 2→6,67, produs 1/6,67. Sursa finală 0/0. Produs evaluat greșit la 6,66 refuzat atomic. | acceptat / ASAMBLARE_NEBALANSATA pe ușa declarației | regulă |
| SC-ASM-12 | Consum 2/100, produs 1/99,99: diferența de 0,01 refuzată; nici loturile, nici cubul nu sunt alterate. | ASAMBLARE_NEBALANSATA pe ușa declarației; refuz valoric pe ușa entității | regulă |
| SC-ASM-13 | Lot absent, cantitate zero, document numai cu consum sau numai cu produs, lot propriu consumat, stoc insuficient, cont absent: refuzuri și zero efecte. Lotul propriu poate identifica produsul când câmpul de culegere lipsește; un produs cules diferit este refuzat. Coduri: LOT_LIPSA, CANTITATE_NEPOZITIVA, ASAMBLARE_STRUCTURA_INVALIDA, STOC_INSUFICIENT, CONT_STOC_LIPSA. | refuzat | regulă / review |
| SC-ASM-14 | ASM produce 1/100, apoi BCS consumă 1/100: storno ASM refuzat pentru dependență. După inversarea BCS se permite inversarea ASM; toate loturile revin exact. | refuz dependență, apoi acceptat | regulă |
| SC-ASM-15 | Data 05.01, înregistrare 05.02, ianuarie închis: mișcarea 2/100→1/100 apare numai în februarie; operarea cu înregistrare în ianuarie este refuzată atomic. | acceptat / refuz perioadă | regulă |
| SC-ASM-16 | Pe toate operările și inversările: 0 fapte TVA, contraponderi fără unitate și valoare 0; SC-X-14 inclus. | acceptat | review |
| SC-ASM-17 | Lot 3/10; trei ASM de câte 1, produse culese 3,33; 3,33; 3,34. ASM-1: C = P = 3,33, Transfer −1/−3,33 și +1/+3,33; sursa 2/6,67. ASM-2: C = 3,34 ≠ P = 3,33 ⇒ `ASAMBLARE_NEBALANSATA` (C 3,34, P 3,33, diferența 0,01), zero efecte; `DistribuieValoarea` pune produsul la 3,34; Transfer −1/−3,34 și +1/+3,34; sursa 1/3,33. ASM-3: C = 3,33 ≠ P = 3,34 ⇒ același refuz; după distribuire produsul e 3,33; Transfer −1/−3,33 și +1/+3,33; sursa 0/0. Produse finale 3,33; 3,34; 3,33. Liniile de consum poartă −3,33; −3,34; −3,33, egale cu postările. | refuz până la redistribuire, apoi acceptat | D9-D3 |
| SC-ASM-18 | Primele două ASM din 17, al doilea redistribuit la 3,34. Storno al doilea: inversează exact −1/−3,34 și +1/+3,34 cu contraponderile; sursa 2/6,67. Corecție în februarie, ianuarie închis: consum 2 (C = 6,67), produs nou cules 1/6,67 ⇒ P = C, Transfer −2/−6,67 și +1/+6,67; sursa 0/0. Fără decizie de absorbție. | acceptat | D9-D3 |
| SC-ASM-19 | Lot 2/10,01; ASM draft consumă 1 și distribuie; BCS intermediar consumă 1. Distribuirea inițială dă produsului valoarea evaluată atunci (5,01 la Away, 5,00 la ToEven); BCS ia aceeași cifră, restul lotului e 5,00, respectiv 5,01. Operarea ASM fără redistribuire: `ASAMBLARE_NEBALANSATA`, zero efecte, BCS neatins. Aceeași comandă reală `DistribuieValoarea`, pe aceleași fapte, pune produsul la rest; operarea trece cu Transfer −1/−rest și +1/+rest; lot 0/0. Predicția nu persistă nimic; a doua distribuire nu schimbă nimic. | refuz atomic, apoi acceptat | D9-D3 |
| SC-ASM-20 | Lot A 3/10 pe un cont de stoc, din care un ASM anterior a scos 1/3,33; lot B 1/10 pe alt cont de stoc. ASM: consum A 1 și B 1; produse culese 1/3,33 pe contul lui A și 1/10 pe contul lui B. C = 3,34 + 10 = 13,34 ≠ P = 13,33 ⇒ `ASAMBLARE_NEBALANSATA` (diferența 0,01), zero efecte. `DistribuieValoarea` repartizează 13,34 proporțional cu valorile culese, ultimul ia restul: 3,33 și 10,01. Grupul lui A are C 3,34 și P 3,33; grupul lui B are C 10 și P 10,01: amândouă nebalansate ⇒ o singură Operare, fără Transfer, cu 8 postări: Credit A 3,34 (−1), Credit B 10 (−1), Debit produs pe contul lui A 3,33 (+1), Debit produs pe contul lui B 10,01 (+1), plus cele 4 contraponderi cu valoare 0. Debit = Credit = 13,34. Loturi după: A 1/3,33, B 0/0, produsele 1/3,33 și 1/10,01. | refuz, apoi acceptat | D9-D3 |
| SC-ASM-21 | Lot 3/10, primul ASM a scos 1/3,33; al doilea consumă 1 și produce 1 cules la 3,33 pe alt cont. C = 3,34 ≠ P = 3,33 ⇒ `ASAMBLARE_NEBALANSATA`. După distribuire produsul e 3,34: Operare cu Credit consum 3,34 (−1) și Debit produs 3,34 (+1) pe celălalt cont, plus două contraponderi 0. | refuz, apoi acceptat | D9-D3 |
| SC-ASM-22 | Grup numai-consum cu C = 9,98; al doilea grup cu C = 1,02 și produse culese 10,99 și 0,01. ΣC = 11 = ΣP ⇒ acceptat fără redistribuire; produsele rămân 10,99 și 0,01. Niciun grup nu e balansat (9,98 contra 0; 1,02 contra 11) ⇒ o singură Operare: Credit 9,98 și 1,02, Debit 10,99 și 0,01. Aserția acumulării Δ dispare; cifrele și acceptarea rămân. | acceptat; probă pe operand închis | D9-D3 |
| SC-ASM-23 | Lot 5/0,02; BCS de 0,5 și de 1 (valoare 0 fiecare, lot 3,5/0,02); ASM consumă 1 din el (C = 0,01) și 1/1 dintr-un lot pe alt cont, cu un produs cules la 1 pe acel cont. ΣC = 1,01 ≠ ΣP = 1 ⇒ `ASAMBLARE_NEBALANSATA`, zero efecte; `ASAMBLARE_DELTA_FARA_ANCORA` nu mai există. După distribuirea lui 1,01 produsul e 1,01; grupul numai-consum (0,01) și grupul cu C 1 și P 1,01 sunt nebalansate ⇒ o singură Operare: Credit 0,01 și 1, Debit 1,01. | refuz, apoi acceptat | D9-D3 |
| SC-ASM-24 | P = 10 contra C = 9,99 sau 9: `ASAMBLARE_NEBALANSATA`, fără altă decizie; evaluarea consumului e disponibilă înaintea gardului de balansare. Separat: produsul cules care se rotunjește la zero e refuzat cu `ASAMBLARE_PRODUS_NEPOZITIV`. | refuz atomic; probă pe operand închis | D9-D3 |
| SC-ASM-25 | Produsul ASM-2 din 17, redistribuit la 3,34. Lotul produsului e 1/3,34; BCS îl golește la 3,34, pe linie și pe postare; lot 0/0. | acceptat | D9-D3 |
| SC-ASM-26 | Stornoul unui original dual. | fără obiect: bazele se recreează (102 b), nu există original dual | D9-D3 |
| SC-X-09 | Privat: surse A 4/200 și B 3/60; ASM consumă A 2/100 și B 3/60, produce marfă 4/160. DSC descarcă produs 1/40 (D 607/C 371). RLF returnează A 1/50 (D 302/C 401 cu −50): A rămâne 1/50, produsul rămâne 3/120, partida RLF +50, postările ASM/DSC intacte. Storno ASM refuzat cât DSC este activ; inversarea RLF și DSC, apoi ASM restabilește A 4/200, B 3/60, produs 0/0 și partida RLF 0. | acceptat / refuz dependență | lanț transversal pe politica privată |

ASM nu deschide partidă; împerecherea nu se aplică. Un ASM valid are minimum
o linie de consum și una de produs, deci cazul simplu are două linii.

## Limite rămase

SC-X-09 acoperă circuitul numeric privat; afirmația despre nepropagarea reevaluării rămâne TR-D9, unde apare
mecanismul Atribuit. Fișa, balanța și snapshot-ul prin cititori comuni:
TR-D8. Concurența și reevaluarea cu compensare: condiții distincte ale
091 (g). Niciun rând de aici nu este declarat verificat prin citirea codului.
Delimitarea contabilă (a)/(h) este aprobată de owner la intrarea în TR-D8:
Operare ASM este raportată în (h), fără contribuție la exit. Un exit 0
certifică domeniul comparabil, nu egalitatea completă cub–registre.
Diagnosticul valoric pe lot rămâne raport, fără efect asupra exit-ului.

## Execuție

Prin `nou/tools/ModelCheck/scripts/verifica.ps1`, numai pe bazele `.CodexBCS`:

- `-Suita Integral -Profil Ambele -Sufix .CodexBCS`: etapa bugetară finală
  are **1.999 OK / 0 FAIL**, exit 0, în
  `run-verificari/20260923-221048-001/integral-bugetar.log`.
  Manifestul acestei încercări are exit 1 deoarece etapa privată a găsit
  proba SAF-T care opera intenționat fără cont; nu este raportat drept succes integral.
- `-Suita Integral -Profil Privat -Sufix .CodexBCS`: după adaptarea probei
  SAF-T la refuzul atomic și repararea curățeniei S-r10, **3.093 OK / 0 FAIL**,
  exit 0; `run-verificari/20260923-221705-012/rezultat.json`.
  Build: zero erori și avertismente.
- `-Suita Nucleu -Sufix .CodexBCS`: **171/171**, zero omise, exit 0;
  `run-verificari/20260923-222003-785/rezultat.json`.
- Rularea filtrată intermediară `-Suita Scenarii -Tip ASM,BCS -Profil Ambele
  -Sufix .CodexBCS` a trecut; gate-urile integrale de mai sus includ și
  ultimele probe de performanță, compatibilitate și SC-X-09.

SC-ASM-01…25 trec în domeniul implementat; SC-X-09 este privat. Probele pure
Δ rulează explicit în ambele convenții de rotunjire. `Fapte.Operand` are
12 interogări pe ObjectSpace nou pentru 2 și 51 de linii (prag 16).
Diagnosticul măsoară exact reziduurile −0,01 și +0,01 și păstrează
proveniența; probele mutante separă istoricul incomplet de o diferență nouă.

Nu există migrație de schemă ori modificări API/UI. BPR rămâne rezervat.

### După review-ul advers

MEDIU-1: diferențele (a) rămân, prin decizia owner-ului, până la TR-D8
(T-r15); proba mixtă le măsoară exact înaintea purjei. MEDIU-2: exit-ul
depinde doar de (a)–(g); diagnosticul numără și postările cubului din afara
domeniului identificat. MINOR-3: primitiva refuză capetele străine formei
ASM-B2. MINOR-4: garda pe cont × produs × cauză și probele de sabotaj
acoperă Transfer, Operare și Storno; contractul nu a fost slăbit.

Prin același wrapper, secvențial, pe `.CodexBCS`:

- `-Suita Nucleu`: **178/178**, zero omise, exit 0;
  `run-verificari/20260923-231828-597/rezultat.json`.
- `-Suita Scenarii -Tip ASM -Profil Ambele`: **164 bugetar / 189 privat OK**,
  zero FAIL, exit 0; `run-verificari/20260923-231934-705/rezultat.json`.
- `-Suita Integral -Profil Ambele`: **2.003 bugetar / 3.097 privat OK**,
  zero FAIL, exit 0 comun; `run-verificari/20260923-232139-948/rezultat.json`.
  Build: zero erori și avertismente. T-r12 închisă; T-r13/T-r14/T-r15 active.

### Delimitarea aprobată la TR-D8 (2026-09-24)

D8-B4 înlocuiește amânarea MEDIU-1/T-r15: Operare ASM este exclusă nominal
numai din (a), iar (h) păstrează numărul documentelor/postărilor și valorile
cub/registre/Δ pe cont, latură și lună. Δ în raport este cub minus registre
pe latura indicată; impactul în sold se citește debit minus credit.
Filtrul de documente se aplică și diagnosticului; Transfer rămâne exclus.
Verificările (b)–(g) și toate scenariile independente ASM rămân active.

`verifica.ps1 -Suita Integral -Profil Ambele -Sufix .CodexBCS`:
**2.616 bugetar / 3.707 privat OK**, zero FAIL, exit 0 comun, build fără
avertismente: `run-verificari/20260924-101646-615/rezultat.json`.
`NUC-ASM-RECONCILIERE` verifică cifrele (h), domeniul comparabil fără
abateri și exit 0, textul excepției, filtrul gol și faptul că o abatere
(a)–(g) continuă să producă exit 1. SC-CIT-04 păstrează cele două postări
contabile de 40 și inversele de −40 în cititorul comun; rapoartele API nu
sunt încă portate. T-r15 este închisă; T-r13/T-r14 rămân active.

Actualizare TR-D8 în lucru (2026-09-25): SC-ASM-17/25 folosesc soldul
cubului pentru C; registrul este separat pentru R. Rezultatele istorice de
mai sus descriu implementarea anterioară. Diagnosticul Δ rămâne activ.

## După tăierea registrelor (TR-D9a, pasul 6, 2026-10-07)

Rândurile 17–26 din tabelul de sus sunt în forma de după tăiere: R a dispărut,
gardul e ΣP = ΣC, grupurile se clasifică pe P_g = C_g, nu există Δ, iar
`DistribuieValoarea` își ia ținta din evaluarea consumurilor făcută de
declarant. Secțiunile „Execuție", „După review-ul advers" și „Delimitarea
aprobată la TR-D8" de mai sus descriu regimul dual și rămân ca istoric;
`NUC-ASM-RECONCILIERE` și diagnosticul Δ au ieșit odată cu oracolul.

Notație: C = consumul evaluat pe soldul cubului, în secvența liniilor (ultima
ieșire ia restul); P = valoarea culeasă a produselor. Într-un `Transfer`
consumul stă pe Debit cu valoare și cantitate negative, produsul pe Debit cu
ele pozitive; într-o `Operare` consumul stă pe Credit, produsul pe Debit.
Fiecare linie are și contraponderea ei pe gestiunea virtuală Transformare,
cu valoare 0 și cantitatea opusă.

Probe: `ScenariiAsm` (SC-ASM-17…21, 23, 25, prin comanda reală
`DistribuieValoarea`: refuz, distribuire, a doua distribuire fără schimbare,
operare) și `ProbeAsmOperand` (SC-ASM-22 și 24, pe operandul închis, în
ambele convenții de rotunjire). Invariantul valorii liniei și mutanții lui
(`LINIE-ASM`, `LINIE-SEMN-ASM`, `IESIRE-SEMN-ASM`) rulează în `INV-CUB`.

## Perechi lângă transformare (D9-F-R1)

`INV-CUB` păstrează drept martori pozitivi transformările și stornourile din catalog.
În tranzacții anulate, mutanții adaugă lângă o transformare o pereche normală
cu valori −1/+1 și cantități 0/0, respectiv −1/+1, pe aceeași cauză.
Cu ordinal perechea trece; cu ambele ordinale șterse rezultatul cerut este
`CITIRE_PERECHE_LIPSA`. Probele rulează separat invariantul perechii, ca un
alt invariant să nu mascheze rezultatul. Verificat pe ambele profiluri: integrala
`run-verificari/20261007-205913-703/`, zero FAIL; vezi pasul 8 §10.
