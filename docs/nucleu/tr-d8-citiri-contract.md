# TR-D8 — Citiri comune și comutarea consumatorilor

- Data: 2026-09-24
- Stare: în lucru; producătorii DEC și IMO implementați (095–097),
  intrarea contabilă comună probată; D8-B4 aprobată de owner. Rapoartele
  generale și snapshot-urile nu sunt încă portate.
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
