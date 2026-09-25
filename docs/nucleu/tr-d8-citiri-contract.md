# TR-D8 — Citiri comune și comutarea consumatorilor

- Data: 2026-09-24
- Stare: în lucru; producătorii DEC și IMO implementați (095–097),
  balanță, fișă, jurnal, sold parteneri și snapshot contabil portate;
  D8-B4 aprobată de owner. Stocul este în curs, restul inventarului rămâne
  explicit nefinalizat. D8-B6 A este aprobată (100); implementarea și probele sunt în curs.
- Surse: 090(f/k), 091(g/i/j), 094, invarianții II/III/VI,
  [inventarul](tr-d8-citiri-inventar.md), N-r8, T-r11…T-r15, S-r2, 091-r3.

## D8-B1 — Domeniul cititorilor

O singură intrare `Module/Cub/Citiri` definește domeniul fiecărei citiri.
Consumatorii nu recompun filtrele. Agregarea și filtrarea se execută în
bază; atributele documentelor și etichetele se alătură după agregare.

- Contabil: Carte=Contabil, reuniunea partițiilor, fără Transfer și
  inversele lui, fără contraponderile structurale Transformare. Include
  Deschidere fără document, fără dublarea detaliilor cu soldul de control.
- Loturi: Carte=Contabil, lot/cont/produs/gestiune; include Transfer și
  inversele sale. Gestiunile virtuale nu devin stoc disponibil real.
- Partide: Carte=Contabil, cont/partener/unitate; include Deschidere,
  Transfer și inverse. Unitatea fără document este o poziție validă.
  Eligibilitatea contului folosește UrmarestePartide (096), nu RolTert.
  Activarea cere diagnostic pentru postările istorice fără unitate;
  simpla migrare a atributului nu le reconstruiește.
- Fiscal: faptele Bază/Taxă, cod TVA și perioadă de declarare; include
  cartea fiscală DVI, exclude contraponderea fără fapt. Păstrează
  regimul/cota istorice și atribuirea temporală, fără recalcul din politica
  curentă. Schema necesară se fixează după verificarea versiunilor TVA.

Cititorul contabil servește simultan sold, rulaje, fișă și jurnal; testarea
doar a soldului este insuficientă pentru DVI și Transfer (T-r11, N-r8).
Testul de arhitectură 091-r3 separă cititorii de producție de adaptoarele
de scriere și de probele independente, cu lista explicită a excepțiilor.

## D8-B2 — Producătorii compleți înaintea comutării (095)

**Alegerea owner-ului, 2026-09-24:** DEC → contractul complet al imobilizărilor → PIF/AMO/CAS pe cub → TR-D8. Include fișa ca unitate, parametrii istorici și amortizarea fiscală. Variantele A/B de mai jos sunt propuneri depășite, păstrate ca istoric; nu se implementează etapa intermediară A. Tăierea registrelor rămâne TR-D9.

Inventarul inițial a identificat dependențele de mai jos; starea este
actualizată după implementarea 097:

| Producător | Starea verificată | Ce pierde o citire exclusiv din cub |
|---|---|---|
| DEC | lipsa inițială este rezolvată: declarant contabil/fiscal și partide pe titular (096), SC-DEC-01…17 | istoria operată înainte de activare cere diagnostic (096-r1) |
| CAS | declarant și fișă în ambele cărți, descărcare brut/cumulat, SC-IMO | istoricul fără fișă/origine se refuză la activare |
| AMO | postări contabile/fiscale și cititor de fișă comun, SC-IMO | istoricul fără fișă/origine se refuză la activare |
| PIF | nominalizare cu suport, fiscal separat, istoric de parametri; API Imo și AMO/CAS citesc cubul (097) | nominalizarea istorică fără suport nu este inventată |

Contraexemplul inițial, rezolvat prin 095–097: notă AMO D cheltuială/C amortizare = 100.
Registrul contabil o include, cubul nu are postările. Comutarea generală
ar raporta zero în loc de 100 pe ambele rulaje, fără vreo eroare tehnică.
DEC justifică 100 cu TVA 21: lipsesc inclusiv baza 100 și taxa 21 din
raportul fiscal alimentat exclusiv de cub.

**A — recomandat: devansăm numai producătorii necesari citirilor generale.**
DEC primește declarație contabilă/fiscală, partide și catalog propriu.
CAS/AMO primesc postările contabile existente, cu scenarii independente
care exercită lanțul real PIF → AMO → CAS, inclusiv storno/corecție.
Migrarea completă a fișei ca unitate, amortizarea în Carte=Fiscal,
parametrii fișei și eliminarea registrului de imobilizări rămân TR-D9,
cu contractul lor. Nu pretindem că registrul imobilizărilor este portat.
Postările contabile intermediare trebuie păstrate/reluate explicit la
trecerea la unități de fișă; nu se dublează în TR-D9.

**B — păstrăm producătorii IMO integral la TR-D9.** Construim și probăm
cititorii comuni, mutăm numai consumatorii pentru care acoperirea este
completă, iar comutarea generală a balanței/jurnalului/SAF-T așteaptă
TR-D9. TR-D8 rămâne explicit incomplet; nu livrăm un raport general care
omite documente și nu unim cubul cu registrele pentru a ascunde lipsurile.

Ambele variante cer DEC; catalogul tipurilor rămase l-a omis, deși are
document și API funcționale. Varianta A schimbă frontiera dintre etape,
nu formulele contabile existente. Decizia se consemnează aici înaintea
implementării acestei extinderi.

## D8-B3 — Proveniența inversării

**Amendat de 102 (2026-09-25):** fără completarea provenienței prin migrație
și fără refuz la activarea hosturilor; bazele de dezvoltare se recreează,
iar invarianții rămân probe ModelCheck.

Storno ASM poate reuni Operare și Transfer. Filtrarea exclusiv după
`Tranzactie.Fel` nu distinge postările. Implementat împreună cu 097:
fiecare postare inversă persistă identitatea postării originale, împreună
cu spațiul ei pentru cheia partiționată; scriitorul stabilește referința
în momentul inversării. `Atribuit` își păstrează semantica separată.
Cititorul clasifică inversa după originea verificabilă, nu după egalitatea
sumelor sau a coordonatelor. Corecția și stornoul peste perioadă folosesc
aceeași regulă.

Migrația nu ghicește proveniența stornourilor istorice mixte. Înaintea
activării citirilor există recensământ: originea unică poate fi atribuită
determinist, cazurile ambigue sunt raportate și blochează activarea până
la tratarea explicită. Nu se recreează baze ale utilizatorului implicit.

Corectura MEDIU-4: diagnosticul de activare verifică întregul domeniu
contabil și refuză cu numărul inverselor fără origine existentă. Migrarea
completează doar perechi univoce în ambele sensuri:
aceeași cauză și toate coordonatele, măsuri inverse, original anterior
stornoului și nefolosit de altă inversă. Perioada declarării poate fi
redatată; prezența ei se păstrează. Ambiguitatea nu se rezolvă prin ordinea
ID-urilor. Proba de migrare modifică temporar proveniența unui BTR real,
într-o tranzacție anulată la final; verifică refuzul, completarea și
idempotenta, apoi un caz ambiguu rămas blocat.

Review advers amânat de owner până la sincronizare, după NIR; verificările
automate continuă pe fiecare felie.

Migrația `20260924130000_OriginiStornoUnivoce` și verificarea inițială
per citire au fost probate prin SC-CIT-07/08: integral 2.723/3.814 OK,
`run-verificari/20260924-160912-899/rezultat.json`.
Separarea de mai jos păstrează scenariile, mutând refuzul la activare.
Predicatul complet al contraponderii este unic în `Cub.Citiri.Transformare`,
folosit în citirea contabilă, Comparabil și DiagnosticValoriStoc. Verificarea
formei la scriere rămâne independentă, în Conservare.

**Pin de performanță implementat înaintea portării consumatorilor:**
`Citiri.Contabil.NumaraFaraProvenienta` numără problemele pe același domeniu
ca diagnosticul explicit `VerificaProvenienta`. Seed-ul păstrează numărul
în `RaportSeed.PostariFaraProvenienta` și tipărește avertismentul dacă este
nenul; proveniența incompletă nu oprește alinierea politicilor sau pornirea
hostului. Diagnosticul explicit refuză originea lipsă, inexistentă, din alt
spațiu sau tot de fel Storno. Completarea deterministă rămâne în migrație.
`Citiri.Contabil.Postari` doar compune interogarea. Inversa contabilă este
selectată numai când originea identificată are fel Operare/Deschidere.
`SC-CIT-07/08` probează diagnosticul și migrarea; `SC-CIT-09/10` probează
numărul comenzilor SQL, refuzul diagnosticului explicit și avertismentul
seed-ului, fără repararea provenienței. Comparația A/B
folosește aceeași bază: diagnostic + citire = 2 comenzi, citire = 1,
compunere = 0. Aceasta este o măsurare de comenzi, nu de latență pe volum.

Refuzul se leagă de activarea primului raport portat (Atomi/Balanța), în
felia care îl comută; nu de seed. Atunci se verifică toate căile de pornire,
inclusiv hosturile fără seed, acoperirea producătorilor și proveniența
snapshot-urilor. Până acolo rămân diagnosticul explicit și probele sale.
Precondiția diagnosticului nu devine o nouă verificare per raport și nu este
înlocuită de cache static per proces. Rapoartele generale rămân necomutate
în această primă felie.

## D8-B4 — Reconcilierea ASM la intrare

**Aprobat de owner, 2026-09-24, numai pentru regimul dual (T-r15).**
Operare ASM se
exclude nominal din comparația contabilă cu registrele vechi (a), iar
(h) raportează separat numărul documentelor/postărilor și diferențele
semnate pe cont. Celelalte tipuri rămân în (a). Probele independente de
transformare, conservare și postare D produs/C materiale rămân obligatorii.
Δ de evaluare pe lot/gestiune/cont nu dispare prin această delimitare.

Implicație: (a) nu mai poate detecta o eroare valorică ASM. Conservarea
nu înlocuiește proba numerică (D 41/C 41 este echilibrat, dar greșit când
scena cere 40). (h) păstrează diferențele vizibile, fără a le include în
exit-ul de neconcordanțe (a)–(g); rezultatul nu se prezintă drept egalitate
completă cub–registre. Cititorii contabili includ integral efectul economic
ASM. Excepția privește numai reconcilierea temporară, nu rapoartele.

Implementat și verificat: `NUC-ASM-RECONCILIERE`, integral ambele profiluri,
2.616/3.707 OK, exit 0, `run-verificari/20260924-101646-615/rezultat.json`.
T-r15 se închide; T-r13 (evaluare/istoric) și T-r14 (consumatori) rămân
separate. (h) compară valorile semnate pe fiecare latură; variația soldului
este Δ debit minus Δ credit. Aceasta livrează delimitarea ASM, nu declară
finalizat întreg diagnosticul transversal (h) din T-D10.

## D8-B5 — Ordinea execuției și regula de oprire

1. DEC → contract IMO → PIF/AMO/CAS complete pe cub (095), inclusiv
   cartea fiscală și istoricul fișei; cataloagele numerice înaintea codului.
2. Delimitarea ASM, proveniența inversării și intrările comune, cu probe
   DVI/ASM/BTR; apoi balanță, fișă, jurnal, partide.
3. Stoc: rapoarte și cititorul operațional comun. Deschidere → BCS/FCL
   trebuie să consume lotul inițial real; doar schimbarea raportului nu
   închide felia. Diferențele duale istorice se raportează (T-r13).
4. TVA, D300/D394, SAF-T, snapshot-uri și închiderea/reconstrucția,
   în ordinea dependențelor din inventar, inclusiv citirile imobilizărilor.
   Eliminarea scrierii registrelor rămâne TR-D9.
5. Auditul deciziei (S-r2), perf A/B pe aceeași bază, arhitectură,
   integral ambele profiluri, review advers și docs.

TR-D8 se închide numai cu lista nominală a consumatorilor portați,
acoperirea producătorilor dovedită și niciun raport declarat complet
alimentat dintr-un cub incomplet. Nicio tăiere de scriitor vechi aici;
eliminarea regimului dual rămâne TR-D9. Import1C rămâne înghețat.

## D8-B6 — Acoperirea partidelor bugetare (A aprobată de owner, 2026-09-25)

Recensământul seed-ului din 2026-09-25: `401.01.00`, `404.01.00` și
`411.01.01`, folosite de politicile FCT/NIR, respectiv FCL/PLT/INC, au
`UrmarestePartide=false`. Rolul SAF-T este Niciunul. 096 a păstrat numai
urmărirea comercială deja existentă și a adăugat 542; 098 a adăugat
conturile diferențelor de recepție. Niciuna nu activează cele trei conturi.

Contraexemplu: FCT 100 și PLT 40 împerecheată lasă sold creditor 60 pe
401.01.00. Raportul vechi arată rest 60, dar cubul nu are nicio unitate
de partidă pentru această factură. Portarea cititorului ar pierde-o.

**A aprobată (decizia 100):** activăm `UrmarestePartide` în politica bugetară pentru
`401.01.00`, `404.01.00`, `411.01.01`, pe conturile DinSeed. Păstrăm
`RolTert` neschimbat. Documentele noi nominalizează unitățile; lanțurile
FCT/PLT, FCL/INC, NTC și deschiderea se probează numeric pe ambele profiluri.
Diagnosticul de activare refuză istoricul fără unități; seed-ul nu îl
reconstruiește și nu se deduc stingeri istorice din simplul sold pe cont.
Conturile manuale și alte conturi neactivate nu sunt schimbate implicit.
O politică de facturare/decontare care le folosește trebuie raportată ca
neacoperită înaintea comutării raportului, nu transformată în rest zero.

**Alternativa B:** păstrăm politica actuală și amânăm comutarea raportului
de partide pentru profilul bugetar. TR-D8 rămâne incomplet pentru această
suprafață; nu mascăm lipsa cu o reuniune cub + registre.

Owner-ul a aprobat A; decizia 100 amendează acoperirea din 096(b).
Implementarea și probele rămân condiții de închidere, distincte de aprobare.

Implementat și verificat la 2026-09-25: seed DinSeed, cititor comun de
partide și gard de activare, SC-CIT-41…48 și cataloagele comerciale pe
ambele profiluri. Integral **3.036 bugetar / 4.026 privat OK**, zero FAIL,
`run-verificari/20260925-073729-162/rezultat.json`; HTTP și XAF verzi pentru
ștergerea atomică a împerecherii. 100-r1 este închisă. Raportul și
snapshot-ul de partide au fost portate ulterior în felia D8-B7 de mai jos.

## D8-B7 — Împerecheri fără efect pe partidă (A aprobată de owner)

**A aprobată la 2026-09-25; decizia 101. Implementată și verificată.**
Portarea raportului scoate la vedere două contraexemple din probele vechi:

- PLT 70 către partener, NTC 581 = 531 cu repartitor contul propriu,
  împerechere 50: registrul legăturilor dă rest 20, cubul păstrează partida
  plății la 70. Nota nu are partidă pe contul și partenerul plății.
- INC 100 pe contul de client și NTC debit furnizor 60 pentru același
  partener: legătura manuală dă rest 40, dar conturile diferă, deci nu
  există Transfer conservativ pe contul comun; cubul păstrează 100.

În comportamentul anterior, `Transferuri.Muta` întorcea `Sarit`, iar materializarea permitea
persistarea legăturii. S-D13 permitea această delimitare a regimului dual;
ea nu mai poate fundamenta o promisiune de stingere în raportul pe cub.

**A aprobată:** comanda nouă de împerechere este acceptată numai dacă
produce efectul cerut pe partidă sau dacă dovedește nominalizarea deja
scrisă, fără dublare (SC-NTC-17). Lipsa contului/partenerului comun,
insuficiența ori ambiguitatea țintei se refuză atomic, iar panourile oferă
numai candidați compatibili. Stingerea între conturi diferite cere o notă
cu postările corespunzătoare, nu un Transfer care ar altera soldul pe cont.
Legăturile istorice fără efect sunt diagnosticate separat, fără rescriere
și fără scăderea lor din raportul partidelor. Citirile operaționale de
disponibil se aliniază cu efectul din cub în aceeași felie.

**Alternativa B:** păstrăm temporar comanda veche și panourile ei explicit
în regim dual. Raportul nou și snapshot-ul rămân strict pe cub, însă
comutarea consumatorilor operaționali și închiderea TR-D8 rămân amânate.
Diferența 20/70 sau 40/100 este vizibilă; nu introducem fallback pe registre.

Felia include raportul `/partide`, snapshot-ul și reconstrucția pe cheia
unitate × cont × partener, deschiderile fără document, citirile operaționale
și nominalizarea automată din cub. Proveniența transferului permite
desfacerea exactă; disponibilul păstrează și efectele datelor ulterioare.
SC-CIT-49…65 și integralele: **3.107 bugetar / 4.103 privat OK**, zero FAIL,
exit 0. HTTP verifică permisiunile, refuzul atomic, închiderea/reconstrucția;
browserul verifică raportul și panoul/candidatul 100 / 40 / 60.
Logurile și limitele sunt în [review-ul propriu](tr-d8-review-codex.md).
101-r1 este închisă; aceasta nu închide întregul TR-D8.

## Starea execuției — 2026-09-25

Portate: intrarea contabilă, Atomi/Balanță/Plan/SoldParteneri, fișa cu
sold curent calculat înaintea filtrului grilei, jurnalul pe identitatea
postării, API/client, snapshot-ul contabil și reconstrucția sa. Activarea
în ambele hosturi verifică proveniența, acoperirea pe linie/cont/latură,
conservarea pe tranzacție/carte și proveniența snapshot-ului. Seed-ul
continuă să raporteze fără a bloca alinierea politicilor.

Validarea integrală a acestei felii: `run-verificari/20260925-022045-998/rezultat.json`,
ambele profiluri, înaintea modificărilor operaționale de stoc. Rularea nu
certifică modificările ulterioare. Probele HTTP contabil și browser au
exercitat permisiunile pe rând/membru și conturile corespondente multiple;
proba durabilă HTTP după închiderea perioadei este implementată în
`nou/tools/ProbeHttp/citiri-cub.py` și trece inclusiv după reconstrucție.

Implementat și verificat: soldurile pentru evaluarea BCS/BTR/DSC/LDI/ASM vin din intrarea
comună de lot, cu cheia completă și gardian pe fiecare zi. R din ASM rămâne
citire explicită a registrului; C vine din cub. FIFO și pinurile DSC,
precum și raportul de stoc, sunt portate împreună. Snapshot-ul de stoc nu
este încă portat și nu alimentează cititorul de lot.

Restul regulii de oprire D8-B5 rămâne deschis: fiscal/TVA istoric,
SAF-T integral (numai GLA/Customers/Suppliers folosesc acum atomi din cub),
snapshot stoc, audit/arhitectură/perf și review final.

Validare după portarea operațională de stoc: **2.899 bugetar / 3.989 privat OK**,
zero FAIL, `run-verificari/20260925-031339-966/rezultat.json`.
Review-ul propriu și limitele care împiedică închiderea întregii etape:
[tr-d8-review-codex.md](tr-d8-review-codex.md).
