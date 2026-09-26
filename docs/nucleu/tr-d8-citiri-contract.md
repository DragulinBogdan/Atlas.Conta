# TR-D8 — Citiri comune și comutarea consumatorilor

- Data: 2026-09-24
- Stare: în lucru; producătorii DEC și IMO implementați (095–097),
  balanță, fișă, jurnal, sold parteneri și snapshot contabil portate;
  stocul și partidele, inclusiv snapshot-urile, portate. D8-B4/B6/B7 aprobate;
  D8-B8 este aprobată și implementată prin 103, în corectură după review;
  restul SAF-T și gate-urile transversale rămân nefinalizate.
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

**Amendat de 102 (C102, 2026-09-25):** refuzul istoricului fără unități la
activare a ieșit; unitățile complete și totalul de decontare sunt invarianți
verificați de ModelCheck (`INV-CUB`) pe baza rezultată.

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

**Amendat de 102 (C102, 2026-09-25):** legăturile istorice fără efect nu mai
există, deci diagnosticul lor și `StingeriDto.Avertismente` au ieșit;
desfacerea are două căi (transferul exact, nominalizarea automată). D-2 și
D-3 din review-ul advers sunt corectate în C102 (SC-CIT-66/67), iar
disponibilul temporal are un singur calcul.

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

## D8-B8 — Citirea fiscală: contract aprobat, 2026-09-25

**Aprobată de owner: F1=A, F2=A, F3=A (103).** Completează D8-B1 și
amendează 088(g/h) și DVI-B3. Felia fiscală este implementată și verificată;
[review-ul propriu](tr-d8-review-codex.md) consemnează probele și limitele.

### a. Intrarea greenfield și informația istorică

`Cub.Citiri.Fiscale` pornește din postările cu fapt TVA, din ambele cărți.
Cheia atomului este `(Spatiu, PostareId)`; cheia faptului economic este
`(TranzactieId, LinieId, TipTvaId, Sens, PartenerId)`. Păstrăm rolul fiscal,
proveniența inversei/corecției și identitatea documentului fiscal. Cartea și
contul rămân proveniență; nu separă baza DVI de taxa aceleiași linii.
Sumele sunt `Valoare`, cu semnul stocat, **nu Debit minus Credit**. Nu
contopim originalul, inversa tehnică și o factură distinctă de corecție.

Proiecția grupează explicit pe regim/cotă și atribuirile temporale istorice;
nu folosește `First`/`Min` ca să ascundă două calificări incompatibile.
La scriere înghețăm pe faptul fiscal `Regim`, `Cota` și calificarea de import.
Inversa le copiază. Tipul TVA curent oferă etichete, nu recalculează istoria.
Alternativa cu versiuni imuabile de nomenclator cere încă o entitate și un
join; recomand valorile istorice pe fapt pentru modelul actual, ca la IMO.
Codul SAF-T este o **mapare versionată la margine**, pe calificarea istorică
și versiunea exportului; nu confundăm schimbarea cotei cu schimbarea
nomenclatorului de raportare. Mapările absente se raportează.

Separăm data documentului, faptul generator/exigibilitatea când diferă,
data primirii la achiziție și data înregistrării. Nu deducem automat toate
aceste date din `Postare.Data`. Atribuirile D300/D394 sunt câmpuri tipizate,
fixate la operare după regula aprobată, nu un registru paralel și nici EAV.
Cititorul comun expune faptele; fiecare declarație își selectează atribuirea.
Jurnalul trebuie să arate explicit reperul temporal ales.

Nu introducem snapshot fiscal: declarațiile citesc intervale de fapte,
nu solduri cumulative. Dacă apare ulterior un sold fiscal materializat,
folosește `CumulPerioade`, fără a patra implementare.

### b. Transferul și blocajele concrete

| Suprafață | Păstrăm | Rescriem înaintea comutării |
|---|---|---|
| TVA/jurnale | agregare SQL, etichete prin LEFT JOIN, sume stocate/manuale | sursa comună, cheia completă, separarea evenimentelor și a perioadelor; numărul de fapte nu devine număr de postări |
| D300 | rânduri, formule, mapări și avertismente verificabile | sursa, regularizările și dublarea prin oglindă la TI; indicatorul generic „rectificativă” nu certifică o declarație depusă |
| D394 | secțiuni, tipuri de operații, XML și validările existente | perioada primirii/emiterii, numărul documentelor fiscale, versiunea corectată fără numărarea inverselor tehnice |
| SAF-T fiscal | schema și validările, nomenclatorul local pin-uit | TaxInformation din fapte, codificare pe secțiune și cotă istorică; restul SourceDocuments rămâne felie distinctă |
| Închiderea TVA | soldurile conturilor TVA deja citite din cub | consumatorii fiscali auxiliari; soldul contabil nu devine suma celor două laturi de autolichidare |
| Storno/corecție | inversa append-only și conservarea contabilă | atribuirea pe declarație, citirea perioadei originalului din cub, identitatea facturii vs inversa tehnică |

Blocante la intrare: `Postare` nu păstrează azi Regim/Cota; un singur
`PerioadaDeclarare` este insuficient pentru cazurile de mai jos; contrapartea
4427 a TI nu are fapt fiscal (B-r4). DVI și TVA capitalizat au deja baza
distinctă, nu o reconstruim din taxe. Aceste blocaje inițiale sunt rezolvate
prin 103. CHECK-ul bazei refuză calificarea/reperele obligatorii absente;
invarianții dintre postări rămân probe ModelCheck. Nu se scanează istoricul
la activare și nu se completează din nomenclatorul de azi (102).
Probele din registre rămân etalon independent numai în regimul dual.

### c. Cercetare și alegeri închise

Alternativele de mai jos păstrează formularea prezentată owner-ului; A este
aprobată la toate cele trei puncte, prin 103.

Sursele au fost confruntate cu codul la 2026-09-25:

- Codul fiscal, art. 280–282/291: distinge faptul generator de exigibilitate
  și stabilește reguli pentru cota aplicabilă. PDF-ul legii inițiale este
  reper pentru structură, nu dovadă a cotelor curente; cotele istorice ale
  exemplelor sunt confruntate cu nomenclatorul local.
  [Legea 227/2015](https://static.anaf.ro/static/10/Anaf/legislatie/L_227_2015.pdf).
- D300: instrucțiunile din OPANAF 174/2026 prevăd regularizări pentru
  perioade anterioare, inclusiv la rd. 16. Nu justifică o rectificativă D300
  automată pentru orice corecție contabilă.
  [Ordinul și instrucțiunile](https://static.anaf.ro/static/10/Anaf/legislatie/OPANAF_174_2026.pdf).
- D394: anexa 2, §1(b), include facturile primite în perioada raportată
  indiferent de exigibilitate; §3 prevede înlocuirea declarației pentru
  omisiuni/erori. [OPANAF 2194/2025](https://static.anaf.ro/static/10/Anaf/legislatie/OPANAF_2194_2025.pdf).
- Procedura erorilor materiale D300 exclude erorile de înregistrare a TVA
  în evidența contabilă. Enum-ul nostru `EroareMateriala` nu poate desemna
  automat acea procedură. [OPANAF 3604/2015, art. 3, forma consolidată](https://legislatie.just.ro/Public/DetaliiDocument/173910).
- Autolichidarea produce la beneficiar taxă colectată și deductibilă;
  furnizorul nu facturează TVA. [Normele, pct. 109](https://static.anaf.ro/static/10/Anaf/legislatie/HG_1_2016_norme%20CF.pdf).
- `anaf/RO_SAFT_SchemaDefCod_16.02.2026.xlsx`: „Legenda coduri taxa TVA”,
  B11:B19 separă achizițiile după deductibilitate; „TVA_NoteContabile”,
  D17:S17 dă codul 380006 pentru autocolectare 21%, **numai GLA**.
  Nu folosim acest cod automat pentru o factură TI. „TAX-IMP - Impozite”,
  B11:D11 distinge factura de import de taxele vamale. Aceste diferențe
  cer mapare pe secțiune, nu un singur cod universal.

**F1 — perioade:** aprobăm atribuiri distincte D300/D394 și data primirii
explicită la achiziții? **Recomand A: da.** Exemplu: factură din ianuarie,
primită în februarie, introdusă în februarie, ianuarie încă deschis; D394
este februarie, chiar dacă regula actuală alege ianuarie. Data primirii se
propune la culegere din data înregistrării, poate fi corectată înainte de
operare și se îngheață. **B:** păstrăm 088 și refuzăm/raportăm cazul ca
neacoperit; comutarea fiscală completă rămâne blocată.

**F2 — corecții:** aprobăm separarea erorii de evidență de factura nouă de
corecție și de eroarea materială a formularului? **Recomand A:** pentru
eroarea contabilă simplă 100/21 → 80/16,80, descoperită după declarare,
D300 păstrează trecutul și duce Δ −20/−4,20 la regularizarea curentă,
iar D394 înlocuiește perioada inițială cu 80/16,80 și o singură factură.
Factura distinctă de reducere −20/−4,20 emisă/primită ulterior aparține
perioadei sale; inversa tehnică nu inventează o factură. Cazurile speciale
de ajustare, inspecție sau procedură ANAF nu se deduc din acest exemplu.
Închiderea internă și depunerea declarației sunt repere distincte; propunerea
include confirmarea explicită a depunerii pe formular/perioadă, cu versiunea
exportată și momentul confirmării. Este reper de raportare, fără sume
fiscale paralele. `InchisaPrimaOara` nu dovedește depunerea. **B:** păstrăm regula
088(h), dar nu certificăm declarațiile afectate; necesită delimitare explicită.

**F3 — taxare inversă:** aprobăm completarea B-r4 cu faptul distinct
`Autocolectare`, atașat contrapărții 4427, cu `Sens=Achizitie`?
**Recomand A:** pe o linie avem o Bază 100, o Taxă 21 și o Autocolectare 21;
nu dublăm baza și nu transformăm achiziția într-o factură de vânzare.
Amendăm unicitatea DVI-B3 la cel mult o postare per rol, cu rolul suplimentar
permis numai la autolichidare. D300 citește cele două obligații și elimină
vechea oglindire care le-ar dubla; D394 numără o achiziție. **B:** derivăm
autocolectarea o singură dată în intrarea comună, din regimul istoric,
fără fapt distinct; este mai puțin cod de scriere, dar nu probează separat
existența coordonatei fiscale pe contrapartea contabilă.

### d. Probe și regula de oprire

Completarea review-ului R1–R5: exportul DTO lunar poartă amprenta faptelor
din citirea comună, calculată în același snapshot cu raportul. Confirmarea
o verifică sub blocaj exclusiv (`DEPUNERE_VERSIUNE_DEPASITA`). Nu se păstrează
o etichetă liberă; nu se pretinde validarea XML-ului ANAF ori a parametrilor
externi prin amprenta faptelor. `DeclarareIntarziata` este eliminată, fără
parametru inert în politica TVA. SC-CIT-88/89 probează versiunea și CHECK-ul.

SC-CIT-79…87 în [catalog](scenarii/CITIRI.md) au fost specificate înaintea
codului și probate prin `ScenariiFiscale`, HTTP și browser. Catalogul
precizează domeniul profilurilor și variația temporală a fixture-ului 80.

Închiderea feliei fiscale cere: producători cu atribute istorice, intrarea
comună și consumatorii TVA/D300/D394/TaxInformation portați împreună;
scenarii independente verzi, integral ambele profiluri, HTTP cu refuz pe
rând și pe `Valoare`/taxă, inclusiv după închidere; arhitectură fără citiri
`RegistruTva` de producție în domeniul portat; A/B pe aceeași bază pentru
jurnal și D394 anual, plus măsurarea părții fiscale SAF-T. Lipsa unui
reper temporal sau a unei mapări produce diagnostic, nu zero plauzibil.

Rămân nominal pentru restul TR-D8: SAF-T SourceDocuments integral,
reconcilierea și auditul transversal, perf-ul exportului complet, review-ul
final și T-r11 pe întregul inventar. Nu declarăm prin această felie acoperite
TVA la încasare, pro-rata/deduceri parțiale sau noi regimuri necontractate.

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
precum și raportul de stoc, sunt portate împreună. Snapshot-ul de stoc
folosește acum lot/cont/produs/gestiune și data deschiderii din cub;
scrierea incrementală și reconstrucția au aceeași sursă. Raportul, FIFO și
pinurile folosesc snapshot + fereastră în ObjectSpace nesecurizat.
Citirile securizate și gardul zilnic recitesc postările. Excluderea în
operare poate folosi numai referința strict anterioară datei documentului;
excluderea istorică generală recitește postările. Tiparul comun
`CumulPerioade.Citeste` selectează referința și citește sumele în aceeași
instrucțiune SQL pentru contabil/stoc/partide, păstrând granița contabilă.
Scrierile globale de snapshot refuză un ObjectSpace secured înainte de
accesarea datelor. Probe suplimentare: SC-CIT-76…78. Registrul necesar regimului dual se citește separat,
fără snapshot din cub. Probe: SC-CIT-69…75.

Restul regulii de oprire D8-B5 rămâne deschis: fiscal/TVA istoric,
SAF-T integral (numai GLA/Customers/Suppliers folosesc acum atomi din cub),
audit/arhitectură/perf și review final.

Validare după portarea operațională de stoc: **2.899 bugetar / 3.989 privat OK**,
zero FAIL, `run-verificari/20260925-031339-966/rezultat.json`.
Review-ul propriu și limitele care împiedică închiderea întregii etape:
[tr-d8-review-codex.md](tr-d8-review-codex.md).
