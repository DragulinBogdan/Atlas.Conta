# ASM — asamblare

2026-09-23. **Aprobat, implementat și verificat; review advers aplicat.** Delimitarea contabilă (a)/(h) e aprobată numai pentru regimul dual (D8-B4, T-r15 închisă). Forma exactă a postărilor este
în [contractul transformării](../tr-d7b-asm-transformare-contract.md),
ASM-B2…B7; activarea cere verificarea scenariilor.
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
SC-ASM-26 rămâne specificat pentru TR-D9. Proveniența este regula, dacă nu este indicată
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
| SC-ASM-17 | Trei ASM succesive de câte 1 din lot 3/10, produse culese 3,33; 3,33; 3,34: cub 3,33; 3,34; 3,34. Sursa 0/−0,01 în dual, regula țintă 0/0. Produse 3,33; 3,34; 3,34, Δ=0; +0,01; 0. | excepție duală exactă, fără toleranță | ASM-B7, T-r13 |
| SC-ASM-18 | După primele două ASM din 17, storno al doilea inversează exact 3,34, sursa 2/6,67; corecție în februarie după ianuarie închis, consum 2 și produs nou 1/6,67: Δ nou 0 și sursa 0/0. | acceptat | ASM-B6 |
| SC-ASM-19 | Lot 2/10,01, ASM draft consumă 1 și distribuie valoarea; BCS intermediar consumă 1. Fără redistribuire ASM refuză valoric cu stoc suficient; după redistribuire golește exact. Away: 5,01→5,00; ToEven: 5,00→5,01. | refuz atomic, apoi acceptat | proba-capcană dual/TR-D9 |
| SC-ASM-20 | Două grupuri balansate: R/C/P = 3,33/3,34/3,33 și 10/10/10; produse finale 3,34 și 10. O singură Transfer, fără Operare. | acceptat | Δ local |
| SC-ASM-21 | Grup numai-consum R=3,33/C=3,34 și alt cont numai-produs P=3,33: produs final 3,34 în Operare. | acceptat | Δ fără produs local |
| SC-ASM-22 | Grup numai-consum R=10/C=9,98 și grup R=1/C=1,02/P=11 cu produse 10,99 și 0,01: ultimul rămâne 0,01 după −0,02 +0,02; nicio validare intermediară. | acceptat; probă pe operand închis | acumularea Δ |
| SC-ASM-23 | Lot 5/0,02, două BCS de 1, apoi ASM consumă 1: R=0/C=0,01, fără produs pe acel cont; alt grup R=C=P=1. | ASAMBLARE_DELTA_FARA_ANCORA, zero efecte | limitare duală aprobată |
| SC-ASM-24 | Δ negativ face ultimul produs final zero sau negativ, deși ΣP=ΣR inițial; refuz înaintea materializării. | ASAMBLARE_PRODUS_NEPOZITIV, zero efecte | ASM-B6 |
| SC-ASM-25 | Produsul ASM-2 din 17 are 1/3,34 în cub și 1/3,33 în registre; BCS îl golește cu 3,33, rămâne 0/+0,01 în cub. | excepție duală exactă | lanț Δ → consum → golire |
| SC-ASM-26 | Storno după TR-D9 al unui original dual cu valoare ajustată 3,34 inversează 3,34 și toate contraponderile, fără recalcul Δ. | specificat pentru TR-D9 | compatibilitate istorică |
| NUC-ASM-RECONCILIERE | Pe SC-ASM-05: (a)–(g) fără diferențe, exit 0; (h) exact ASM D 345 = 40 față de 0 și C 301 = 40 față de 0 (conturi echivalente bugetare), 1 document și 4 postări Operare, inclusiv 2 contraponderi zero. Raportul declară excepția; orice abatere comparabilă păstrează exit 1, iar filtrul de documente se aplică și în (h). | delimitare duală aprobată, probe independente obligatorii | owner 2026-09-24, D8-B4, T-r15 |
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
