# 111(f) — reluarea discuției despre fapte tipizate și proiecții persistate

Tip: analiza
Data: 2026-10-11
Alimentează: 111 (f), viitoarea decizie de implementare și legătura cu TR-D9b

Owner-ul a acceptat direcția propusă în discuția cu Codex și a cerut
păstrarea contextului înainte de restartarea CLI-ului. Următorul pas este
discuția de proiectare, cu pro și contra; nu implementarea automată.
Această analiză păstrează acordul de principiu, dovezile și întrebările
rămase. Nu înlocuiește decizia sau contractul viitoarei felii.

Discuția a continuat după reluare: precizările și acordurile sunt în
§„Continuarea discuției”, iar [scenariul numeric](2026-10-11-111f-scenariu-numeric.md)
le verifică pe hârtie. Secțiunile inițiale păstrează contextul de la salvare;
întrebările lor nu sunt toate încă deschise.

## Punctul de reluare

La salvare, HEAD este `c9f45d5`. C111 este închisă și mersă în main prin
PR #36; 111 (a)–(e) și (i) sunt implementate. C111-F-R1 și R2 sunt închise:
terțul contului opțional este păstrat, iar accesul proiecțiilor securizate
este precizat prin Q22. Nu trebuie reluat review-ul acestei felii.

[111 (f)](../decizii/111-repartitor-unic-si-registre-tipate.md) rămâne
criteriul aprobat pentru faptele tipizate din afara cubului, fără
implementare. TR-D9b rămâne felia unităților de sold. Direcția acceptată
aici nu schimbă implicit ordinea feliilor sau scopul TR-D9b.

Regulile de documentare au fost actualizate între analiza inițială și
această salvare: intrarea este [CLAUDE.md](../../CLAUDE.md), împreună cu
[AGENTS.md](../../AGENTS.md), decizia 118 și kitul de guvernanță indicat
acolo. Această notă aparține jurnalului de analiză, nu stării implementate.

## Ce a cerut și ce a acceptat owner-ul

Cererea: evaluarea câștigului real al lui 111(f), a costului de performanță
și a utilității unor informații suplimentare persistate, chiar redundante,
pentru simplificarea interogărilor și proiecțiilor. Analiza trebuie să
alimenteze viitoarea implementare, după o discuție comună.

Owner-ul a spus că este de acord cu propunerile analizei. Direcția reținută:

- Faptele independente, cu ciclu de viață propriu, au un depozit tipizat:
  aprobări, modificări, rectificări; efectele contabile rămân în cub.
- Executatul și disponibilul sunt proiecții peste faptele relevante;
  nu devin valori întreținute independent în două surse autoritative.
- Identitățile și atributele stabile cunoscute la declarare pot fi
  persistate pentru a evita reconstrucția costisitoare la citire.
- O proiecție materializată este permisă dacă este reconstructibilă,
  verificabilă și justificată de măsurători.
- Se pornește cu un caz concret, nu cu un mecanism generic configurat prin
  tabele: angajament, modificare, plată atribuită, controlul disponibilului.
- Efectele obligatorii se validează și se scriu în aceeași tranzacție;
  nucleul contabil nu preia ciclul procedural al angajamentelor.

Formulare de lucru acceptată ca direcție, de formalizat în decizie:

> Fiecare fapt are o singură sursă autoritativă. Identitățile și atributele
> stabile cunoscute la declarare pot fi persistate pentru citire eficientă.
> Rezultatele derivate pot fi materializate ca proiecții reconstruibile,
> cu proveniență și verificare de echivalență.

## Plata cu defalcare — informația nouă a owner-ului

În aplicația veche plata avea un antet și o defalcare pe execuție:
**o linie corespundea unei poziții din angajament**. Prin detaliile plății
se obținea relația n×m: o plată putea acoperi mai multe poziții, iar o
poziție putea fi executată prin mai multe plăți. Aceasta este descrierea
owner-ului, nu o constatare nouă din analiza surselor legacy.

Owner-ul permite păstrarea acestei reguli sau alegerea uneia mai bune pe
termen lung. Nu a cerut compatibilitate cu orice detaliu al implementării
vechi.

Recomandarea Codex la salvare: **păstrăm această regulă drept punct de
plecare**. Detaliul cu identitate, sumă și referință la poziție reprezintă
explicit asocierea. Nu este necesară încă o relație n×m generică deasupra
aceleiași defalcări. Pentru împărțirea unei sume între două poziții se
culeg două detalii; plata bancară poate rămâne una, la nivelul antetului.

Exemplu de discutat: P1 are A=60 și B=40; P2 are A=25. P1 totalizează 100,
P2 totalizează 25, iar execuția este A=85 și B=40. Sunt trei detalii, fără
ca un detaliu să trebuiască să numească simultan A și B.

Consecințe propuse, încă de formalizat:

- O poziție are identitate stabilă peste rectificări. Trebuie stabilit
  separat dacă plata fixează și versiunea aprobării folosite la validare.
- Asocierea nu implică o singură postare per detaliu: declararea poate
  produce mai multe postări. Cititorul execuției selectează efectul
  relevant, fără a dubla suma prin debit și credit sau prin transferuri.
- Referința la poziție poate ajunge direct pe postarea relevantă dacă
  atribuirea rămâne unică. Ea nu intră automat în toate cheile balanței.
- Granularitatea stingerii facturilor este altă întrebare: o linie de
  execuție poate necesita alocări către mai multe partide. Nu echivalăm
  poziția de angajament cu factura sau cu partida.
- Trebuie delimitate plățile fără angajament, comisioanele, avansurile,
  rambursările și diferențele de curs înainte de a impune obligativitatea.

## Baza verificată și reperele de performanță

Analiza inițială a fost făcută pe `75f908f`, după închiderea C111. Nu a
inclus benchmark nou sau implementare experimentală.

- `Angajament` este stub de nomenclator: `DimensiuniBugetare.cs:42–46`.
  `AngajamentId` ajunge în operandul liniei, dar nu există pe `Postare`.
  Atribuirea execuției nu are încă o referință directă în cub.
- `Contabil.Postari` consultă tranzacția și, pentru storno, originalul;
  `Partide.OriginiDocument` recunoaște originea prin postări. Acestea sunt
  candidați concreți pentru simplificare, nu dovada că orice join e scump.
- `MotorOperare.PregatesteSiValideaza` cere acum declarantul cubului;
  un document exclusiv procedural va necesita o adaptare explicită a
  orchestrării. Există deja tranzacția și blocajul comun al scrierii.

Sursele de cod citite:
[dimensiuni și angajament](../../nou/Atlas.Conta.BackOffice/Atlas.Conta.BackOffice.Module/BusinessObjects/Nomenclatoare/DimensiuniBugetare.cs),
[postare](../../nou/Atlas.Conta.BackOffice/Atlas.Conta.BackOffice.Module/Cub/Postare.cs),
[cititor contabil](../../nou/Atlas.Conta.BackOffice/Atlas.Conta.BackOffice.Module/Cub/Citiri/Contabil.cs),
[partide](../../nou/Atlas.Conta.BackOffice/Atlas.Conta.BackOffice.Module/Cub/Citiri/Partide.cs),
[motor de operare](../../nou/Atlas.Conta.BackOffice/Atlas.Conta.BackOffice.Module/Motor/MotorOperare.cs),
[tranzacția comenzii](../../nou/Atlas.Conta.BackOffice/Atlas.Conta.BackOffice.Module/Motor/TranzactieComanda.cs).

| Dovadă existentă | Rezultat și limita lui |
|---|---|
| Originea partidei într-un tabel temporar cu un rând per partidă | 2,9 s → 0,8 s pe aceeași bază; prototipul este în [pasul 5c](../nucleu/tr-d9-pas5c-imbinare-partide.md), nu implementare curentă |
| Cele două FK-uri suplimentare din C111 | Împreună circa 2,3 µs/postare în scena măsurată; efectul pe comandă a fost în variația rulărilor; [pasul 6](../nucleu/c111-pas6-scriere-si-volum.md) |
| Fișa 371 după corectarea formei și indexului | 11,3 s → 0,1–0,2 s pe baza măsurată; fără copierea contului corespondent; [pasul 7](../nucleu/c111-pas7-client-si-inchidere.md) |

Nu există încă o cifră pentru costul lui 111(f). Costul depinde de
validările noi, numărul efectelor scrise, indecși și proiecții întreținute.
Serializarea scrierii face important timpul de validare sub blocaj:
citiri pe lot și restrânse la pozițiile atinse, nu scanarea istoricului la
fiecare linie.

## Candidații de persistență și limitele lor

1. **Originea unității de sold:** candidat cu dovadă de câștig; se
   coordonează cu TR-D9b, nu se implementează separat și concurent.
2. **Poziția de angajament atribuită:** prioritară pentru execuție; devine
   fapt de atribuire la scriere, fără deducere ulterioară din documente.
3. **Clasificarea efectului și proveniența inversei:** candidat pentru
   prototip; trebuie păstrată semantica transferului și securitatea.
4. **Contul corespondent:** numai proiecție de citire conform 111(d).
   O pereche ascunsă utilizatorului nu trebuie dezvăluită prin copia ei.
5. **Felul repartitorului pe postare:** nepropus acum; ar amenda 111(a)
   și cere câștig măsurat, plus regulă clară pentru consistență.
6. **Sold/executat curent:** sumar separat, reconstructibil; nu valoare
   independentă pe fiecare postare și nu sursă întârziată de validare.

„Nu se balansează” nu înseamnă „nu se însumează”: o aprobare și modificările
ei pot avea măsuri aditive fără debit/credit. Nici câte un tip de document,
nici câte o etapă procedurală nu impun automat câte o tabelă.

## Întrebările de reluat împreună

1. Confirmarea formei antet + detalii de execuție, o poziție pe detaliu;
   relația acesteia cu alocările pe facturi/partide și cu plata bancară.
2. Definirea lui „executat”: plată, obligație recunoscută, alte etape;
   ce efecte se selectează din cub și cum tratăm transferurile și storno.
3. Identitatea poziției și versiunile aprobării; limite anuale/multianuale,
   datele efectelor și comportamentul rectificărilor retroactive.
4. Verificarea transversală pe intervalele afectate, nu numai soldul final;
   verificarea plății și a reducerii angajamentului, în ambele direcții.
5. Unde persistăm atribuirea și ce cer citirile: coloană pe postare,
   structură separată sau combinație justificată de cardinalitate.
6. Drepturile noilor proiecții și ale validării. Regimul parțial acceptat
   pentru rapoarte prin Q22 nu justifică validarea unei limite pe date
   incomplete.
7. Terminologia: 111(f) spune „registre tipate”, iar 118 restrânge termenul
   „registru” la rapoarte cu nume legal. Păstrăm referința istorică și
   alegem termenul nou cu owner-ul, fără redenumiri premature în cod.
8. Felia minimă, apoi contractul și măsurarea A/B pe aceeași bază:
   operare/retragere/storno, citiri selective și rapoarte complete,
   reconstrucție, spațiu și costul scrierii. Nu există buget numeric aprobat.

La reluare se discută aceste puncte și alternativele; acordul de principiu
nu se transformă automat în schema finală. Nu sunt procese de verificare
în curs lansate pentru această analiză și nu există modificări de produs
de continuat. Salvarea de față adaugă numai documentația discuției și
indexarea ei prin docskit.

## Continuarea discuției — acorduri de proiectare

Owner-ul a precizat că plata legacy avea două defalcări: contabilă,
cu legătura spre facturi, și de execuție, pe poziție de angajament sau pe
Sursă + CF + CE. Sursa era reprezentată prin reutilizarea codului de sector;
această convenție nu se preia. Factura purta referința la angajament pe
poziție. Relațiile amestecau identificatori de antet și de detaliu.
Acestea sunt informațiile owner-ului, nu un recensământ nou al legacy-ului.

Direcțiile următoare au fost acceptate în conversație, pentru formalizare
ulterioară în decizie și în contractul feliei:

- Execuția bugetară verifică robustețea modelului comun; direcția rămâne
  privat-first, fără schimbarea ordinii TR-D9b.
- Plata păstrează două defalcări distincte; nu se presupune corespondență
  rând cu rând. Trasabilitatea valorică folosește identități de poziții și,
  când o poziție se distribuie, alocări explicite cu sumă.
- Atribuirea nominalizează poziția bugetară și poziția legală. Asocierea
  lor se validează explicit; același triplet nu dovedește compatibilitatea.
  Cazul 1–1 este optimizare a culegerii: selecția uneia o completează pe
  cealaltă când alegerea este unică; operarea reverifică perechea.
- Precizare ulterioară a owner-ului: asocierea legalului consumă din
  bugetar, iar bugetarul este în limita planificatului anual. Asocierea
  poartă deci suma rezervată, nu doar permisiunea de a folosi perechea.
  Factura consumă acoperirea rezervată, fără o a doua scădere din bugetar.
  Owner-ul a completat lanțul: planificat → deschis → bugetar. Deschiderea
  are aceleași proprietăți de consum și control temporal; nu cere un caz
  separat în analiza de față. Fiecare treaptă consumă din precedenta.
- Natura bugetară / legală anuală / legală multianuală se separă de
  acoperirea individuală / globală. Angajamentul global poartă clasificarea
  și limitele, nu este un recipient nelimitat. Obligativitatea celor două
  referințe pentru toate fluxurile rămâne de precizat.
- Ordonanțarea consumă alocarea concretă a poziției facturii; plata consumă
  alocarea ordonanțării. Precizare la trecerea dintre ani: pentru soldul
  unui legal multianual se face un bugetar în anul nou, pe care îl execută
  plata; bugetarul de origine al facturii nu se preia automat la plată.
  Pentru salarii, sursa obligației este documentul
  corespunzător, fără factură artificială. Forma acelui document rămâne deschisă.
- Facturatul / obligația constatată, ordonanțatul și plătitul sunt măsuri
  distincte. Aceeași sumă se verifică față de limitele aplicabile, fără a
  fi numărată de două ori în execuție. Reducerea și retragerea verifică și
  dependențele din aval, nu doar creșterile verifică disponibilul din amonte.
- Verificarea temporală acoperă toate momentele relevante din intervalul
  afectat, pe pozițiile atinse. Data efectului și momentul înregistrării
  sunt distincte; o suplimentare ulterioară nu acoperă un deficit anterior.
- Poziția are identitate stabilă; aprobările și modificările sunt fapte
  persistate. Reverificarea trecutului nu rescrie efectele deja operate.
  Efectele aditive sunt fixe; „+20” și „noul plafon este 120” nu sunt
  interschimbabile la introducerea unor modificări retroactive.
- Istoricul procedural păstrează și retragerile. Retragerea unui fapt și
  inversarea lui la o dată ulterioară au efecte temporale diferite. Această
  direcție nu schimbă implicit retragerea din cub și nu promite istoricul
  complet al cubului la orice moment de înregistrare.
- Limitele planificării sunt distincte: planificatul anual limitează
  bugetarele prin deschideri; creditele de angajament limitează angajamentele
  multianuale. Reacoperirea anuală a aceluiași sold nu creează încă o
  obligație legală și nu consumă încă o dată creditul de angajament.

Owner-ul a clarificat „propunerea = id legal anual”: este aceeași poziție
care începe ca propunere și devine legal anual, iar bugetarul este în limita
ei. Nu desemnează o poziție anuală de acoperire sub legalul multianual.
Identitatea comună simplifică trasabilitatea; suma propusă, aprobarea
bugetară și angajarea legală rămân fapte distincte, de formalizat în
modelul conceptual. Această identitate nu impune relații 1–1 cu bugetarele.

Precizare despre legacy, dată de owner: baza bugetarului era anuală și la
nivelul legalului multianual se păstra soldul de început de an. Legacy-ul
omitea propunerea separată și avea direct bugetar în legal. Acestea sunt
informații de domeniu și de proveniență, nu prescripția de a copia stocarea
veche; nu a fost făcută o verificare legislativă în această analiză.

Owner-ul a autorizat pasul următor: un scenariu numeric complet, înaintea
schemei fizice. [Scenariul](2026-10-11-111f-scenariu-numeric.md) separă aceste
acorduri de convențiile noi ale exemplului. Nu este probă executată și nu
certifică implementarea lui 111(f).

Ulterior, owner-ul a acceptat distincția soldurilor de început de an
(legal 150, facturat 70, plătit 40: rămas de executat 110 = facturat
neplătit 30 + nefacturat 80) și proiecția lor reconstructibilă. A acceptat
modelul conceptual cu identități pentru poziții, asocieri valorice și
alocări de obligație/execuție și a cerut descrierea contractului fiecărei
operații. Rezultatul este în
[contractele conceptuale](2026-10-11-111f-contracte-operatii.md), cu intrări,
validări, efecte atomice și așteptări numerice. Organizarea detaliată a
comenzilor este descrisă conceptual, nu deja implementată. Q1–Q4 sunt
tranșate prin precizările și aprobarea de mai jos; limitele lor sunt
centralizate acolo; identitatea propunerii și măsurile soldului simplificat
nu mai sunt întrebări deschise.

Precizarea finală a owner-ului asupra contractelor anuale: propunerea se
face la lansarea licitației; bugetarul și legalul se fac la desemnarea
câștigătorului; semnarea poate avea o valoare diferită, iar o scădere cere
corecția legalului și rectificarea bugetarului în jos. Direcția păstrează
faptele distincte și trasabilitatea lor, nu egalitatea retrospectivă a
valorilor. S10 exprimă numeric propunere 100 → atribuire 80 → semnare 75;
rezervarea se aliniază rectificărilor. Interpretarea intermediară ca
egalitate obligatorie între toate valorile etapelor nu este direcția reținută.

Pentru ordonanțarea rămasă neplătită la trecerea dintre ani, owner-ul a
precizat că se emite una nouă. Varianta S9 păstrează ordonanțarea veche și
plățile ei în istoric, iar restul este autorizat prin documentul nou pe
bugetarul anului nou, fără dublarea dreptului de execuție.

Owner-ul a aprobat apoi mecanismul din Q2: dreptul de execuție al
acoperirii și ordonanțării are valabilitate anuală persistată la operare.
Expiră la trecerea anului fără corecții automate sau modificarea faptelor
vechi. Acoperirea nouă și ordonanțarea nouă sunt operații distincte.
Acoperirea parțială este admisă: 20 din restul de 30 permit autorizare și
plată de 20; ceilalți 10 rămân obligație neacoperită.

Valabilitatea se verifică la data efectului, separat de închiderea
perioadei. O plată înregistrată târziu pentru anul vechi verifică și
acoperirile și ordonanțările anului nou, fără a le ajusta automat; după
închidere, perioada refuză scrierea. S9 include așteptările numerice.
Q2 este închisă pentru fluxul multianual delimitat, la nivel de proiectare.

Owner-ul a exclus explicit din model contractele lunare în care luna a
douăsprezecea ajunge să fie plătită în ianuarie, până la clarificarea
interpretării lor. Nu deducem tratamentul acestor cazuri din S9 și nu le
modelăm acum.

La 2026-10-12, owner-ul a cerut investigația Q3, urmată de Q4. Rezultatul
este [analiza provenienței și limitelor](2026-10-12-111f-q3-q4-provenienta-si-limite.md),
pe sursele de la `96ccc5a`, consultate prin CodeGraph. Propunerile sunt:
legături valorice între alocări și efectele canonice din cub, integrare
atomică și proveniență păstrată la retragere; prima felie ar oferi istoricul
procedural și situația financiară la data efectului pe faptele cunoscute
acum, fără reconstrucție financiară completă după momentul cunoașterii.
Q4 delimitează prima implementare procedurală. Owner-ul a aprobat apoi
Q3–Q4 și a cerut păstrarea ritmului pe privat. Aprobarea închide întrebările
la nivel de proiectare pentru domeniul delimitat; nu pornește implementarea
procedurală și nu schimbă ordinea feliilor. TR-D9b rămâne următorul pas,
iar implementarea scenariilor bugetare nu devine condiție a livrării private.
Recomandarea de execuție este consemnată la finalul analizei Q3–Q4.
Nu s-au modificat codul sau invarianții.

Owner-ul a acceptat apoi revenirea la TR-D9b. Punctul de pornire și primul
pas propus sunt în [reluarea pe privat](2026-10-12-tr-d9b-reluare-privat.md):
originea explicită a partidei, cu soldul citit în continuare din cub.
Există măsurătorile TR-D9b și direcția aprobată, dar contractul propriu
trebuie încă scris; nota delimitează pregătirea lui, fără implementare.
