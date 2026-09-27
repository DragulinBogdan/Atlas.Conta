# NIR — diferența față de recepția postată de FCT

Data: 2026-09-24. Stare: implementat și verificat pe ambele profiluri; regulile
098(a), 099(a–h) sunt aprobate. **NIR-D4, inclusiv 35x și contul distinct
pentru personal, aprobat de owner la 2026-09-24.**
Review-ul advers 1945 este aplicat; în așteptarea reverificării independente înainte de commit.
Surse: invarianții II/III/IV/VI, T-D5 amendat, 092, 096, comunicările
2026-09-24-1350 și 1428.

## NIR-D1 — Sursa și proveniența

FCT declară structural că acoperă recepția conexă. Politica alege generarea
și filtrul liniilor, dar un rând PoliticaConex nu poate inventa absorbția.
La operarea recepției, dovada este recepția deja postată de FCT în cub,
cu lotul, gestiunea, contul, analiza, cantitatea și valoarea ei istorică.
PosteazaInCub și politica actuală nu sunt dovada faptului trecut.

Conexul păstrează identitatea sursei și a liniilor acoperite; corecția le
preia chiar dacă Autogenerat devine false. Aceste legături sunt stabilite
de server, nu editabile prin DTO. Tranzacția-sursă și identitățile liniilor folosite la
comparație se fixează la operare, inclusiv când delta este zero; ele
identifică setul postărilor imuabile, inclusiv separarea bază/TVA capitalizat. Astfel,
zero tranzacții economice nu înseamnă lipsa provenienței absorbției.
Migrația DiferenteReceptie completează SursaReceptieiId din DocumentSursaId
numai pentru NIR autogenerat cu sursă FacturaIntrare. SursaReceptieiId este
apoi singura definiție în cod; nu există fallback la Autogenerat.
Istoria fără legătură determinabilă se diagnostichează; nu se deduce
absorbția din politica de azi. NIR manual păstrează recepția integrală.

## NIR-D2 — Delta și limitele de culegere

O singură recepție activă, cumulativă, per FCT. Un alt candidat activ se
refuză atomic, inclusiv la operare concurentă. Sosirea ulterioară trece
prin corecție: inversarea deltei vechi, apoi diferența noului cumul față
de aceeași sursă. Anularea permite reoperarea, fără dublarea recepției FCT.

Liniile sursei rămân prezente și unice. Schimbarea lotului, produsului,
gestiunii sau duplicarea liniei-sursă se refuză; factura este locul
corectării recepției inițiale. Cantitatea zero este admisă numai pe linia
acoperită, cu cauza minusului; cantitatea negativă se refuză. Liniile
adăugate au lot propriu, cantitate pozitivă și cauza Plus. NIR manual
păstrează cantitate strict pozitivă. O singură cauză și un singur partener
de imputare pe linie; nu se împarte aceeași lipsă prin duplicarea liniei.

Pentru linia-sursă: Qdelta = Qconstatat − Qsursă; Vdelta = Vconstatat −
Vsursă. Evaluarea folosește valoarea/cantitatea recepției din cub, inclusiv
TVA capitalizat în stoc; Qconstatat = Qsursă păstrează exact Vsursă, iar
Qconstatat = 0 dă zero. Rotunjirea este a profilului. NIR nu schimbă
prețul lotului. Pentru linia adăugată: prețul cules și rotunjirea normală
a recepției. Cauza implicită este InClarificare la minus, Plus la plus;
o cauză incompatibilă cu sensul se refuză.

Minusul scoate cantitatea și valoarea din lotul sursei și debitează
contrapartida cauzei; plusul adaugă stoc și creditează contrapartida.
Capătul fără stoc păstrează conservarea cantitativă prin gestiunea
structurală Inventar, fără lot real disponibil. Partida se nominalizează
după UrmarestePartide, pe partenerul potrivit: imputat la Imputabila,
furnizor la Plus. Cauza, partenerul și conturile efective rămân istoric.
NIR nu postează TVA; efectele fiscale ale cauzelor rămân 099-r2.

Exemplu privat, fără TVA: FCT 4/100 → constatat 3/75. Delta este C 302 =
D 473: 25, cantitate −1 pe lot; rezultatul grupului: stoc 75, clarificare
25, datorie 401 de 100. FCT 4/100 + lot nou 1/10: stoc 110, 401 de 100,
408 de 10. Nu se rescrie partida facturii ca să coincidă cu registrul vechi.

## NIR-D3 — Politica și profilul privat aprobat

CauzaDiferentei este enum de domeniu, cu set permis declarat pe tip;
NIR admite cele șase cauze din 099. Politica are cheia tip document ×
cauză × clasa materialului. Conturile sunt date; mecanismul nu cunoaște
simboluri și profiluri. Imputabila cere partener și cont cu
UrmarestePartide; RolTert comercial nu se schimbă artificial.

Privat, aprobat prin 099(e): InClarificare → 473; Plus → 408;
Imputabila → 461; Perisabilitate/Neimputabila → contul 6xx al clasei;
PeDrum: 301→321, 302→322, 303→323, 371→327, 381→328.
Conturile 32x există în nomenclatorul privat; seed-ul le folosește. 461 primește urmărire
de partide; partenerul poate fi și angajat, conform alegerii 099.

## NIR-D4 — Maparea bugetară aprobată pentru 098-r1

Analiticele următoare există deja în `SeedData/plan-conturi.csv`; lipsește
politica nouă, nu nomenclatorul lor. Alegerea aprobată:

| Cauză / categorie | Cont |
|---|---|
| InClarificare | 473.01.09 |
| Plus | 408.00.00 |
| Imputabila, partener extern | 461.01.09 |
| Imputabila, angajat | 428.01.02 |
| Perisabilitate / Neimputabila | 601.00.00, analiticul 602.xx.00 corespunzător clasei, 603.00.00, 607.00.00 sau 608.00.00 |
| PeDrum, materii prime / consumabile | 351.01.00 |
| PeDrum, obiecte de inventar | 351.02.00 |
| PeDrum, mărfuri | 357.00.00 |
| PeDrum, ambalaje | 358.00.00 |

Cheia politicii rămâne cea aprobată. Imputabila are două conturi în același
rând: cont normal și cont pentru personal; alegerea se face după felul
partenerului. Privat ambele pot indica 461, păstrând alegerea 099(e).
Conturile 461.01.09 și 428.01.02 urmăresc partide fără rol comercial;
Plus folosește furnizorul și urmărirea pe 408.00.00. Analizele cerute de
conturile bugetare se validează; lipsa lor nu este completată artificial.

Surse pentru simboluri: [planul public, anexa OMFP 2021/2013](https://legislatie.just.ro/Public/DetaliiDocument/269572),
[analiticele 461 și 473 din OMFP 1917](https://legislatie.just.ro/Public/DetaliiDocumentAfis/211576).
Pentru PeDrum, aceasta este **propunerea de mapare**, nu afirmația că
planul impune automat aceeași corespondență cu 32x privat. Funcțiunea
351 permite evidența achizițiilor rămase la terți și revenirea în gestiune;
vezi [OMFP 1917, cap. VII, contul 351, pagina PDF 289](https://ditlgiurgiu.ro/Portals/0/Legislatie/Ord.1917.2005.pdf?ver=2018-12-26-121547-243#page=289)
(copie publicată de DITL, consolidare 2021). Nu se adaugă conturi 32x
private în planul bugetar. Alegerea 35x și separarea imputării pe personal
sunt confirmate de owner pentru implementarea politicii bugetare.

## NIR-D5 — Inversare, gardieni și regim dual

Storno/anulare se decid pe postările proprii existente și proveniența
persistată; schimbarea politicii conexului, conturilor sau PosteazaInCub
nu poate suprima inversarea. Storno păstrează identitatea originalului și
inversează măsurile, fără recontare. Corecția nouă calculează noua deltă
cu politica valabilă la noua operare. Dependențele pe lot/partidă și
perioada închisă rămân gardieni; dry-run și operare dau același refuz.

Registrele vechi rămân până la TR-D9. Reconcilierea separă numai grupurile
FCT + recepție + corecții care au deltă; raportează diferența contabilă
completă în (h), inclusiv 401. Nu exclude global FCT/NIR din (a). Celelalte
grupuri rămân comparabile; (f), originea inverselor și probele numerice pe
cub rămân obligatorii. Absorbția din diagnosticul stocului și normalizările
reconcilierii se citesc din aceeași proveniență istorică.

Implementarea păstrează contul și analiza istorică pe capătul stocului.
Doar contrapartida este validată față de cerințele curente ale contului.
Pe contrapartidă, analiza culeasă se completează din cea istorică și se
validează față de cerințele contului; inversarea nu revalidează politica
curentă. Delta zero este o acceptare explicită a adaptorului, fără tranzacție
economică persistată; nu relaxează conservările nucleului.

Pentru NIR acoperit, storno/anularea simplă după consum, precum și intervalul
până la operarea cumulului corectat, pot lăsa registrul lotului negativ.
Grupul fără cumul activ se raportează incomplet. Cubul păstrează recepția
FCT și gardianul său de stoc; cât registrul este negativ, operațiile pe
același lot care trec prin gardianul registrelor pot fi refuzate. Owner-ul
acceptă limita temporară până la TR-D9; nu se adaugă adaptări pentru
compatibilitatea regimului dual. Cititorii generali trec pe cub la TR-D8,
iar garda registrelor dispare la TR-D9 (098-r3).

Seed-ul creează politici pentru clasele de stoc existente: MP/M/OI/MF/AMB
privat, M/OI/OF/MF/D/L/PS/MED/MS/DEZ bugetar. Clasa M folosește 602 privat,
602.01.00 bugetar; cheia aprobată nu distinge subtipurile din aceeași clasă.
Bugetarul nu are încă clase MP/AMB, deci maparea aprobată 601/608/358 nu
inventează clase noi. Clasele fără politică (inclusiv PF privat) primesc
refuz explicit la delta nenulă; recepția fără diferențe nu cere politică.

## NIR-D6 — Probe și suprafața implementării

SC-NIR-09/10 se rescriu după regula faptului postat. Se adaugă înaintea
codului matricea de delta: parțial 75/25/100, plus 110/100/10, mixt,
zero, toate cauzele, imputări 10+15 la doi parteneri, lot schimbat refuzat,
linie-sursă ștearsă/duplicată refuzată, două recepții active/concurente,
corecție cumulativă PeDrum și corecție peste închidere.
Lanț obligatoriu: 4/100 → 3/75 → schimbare de politică → storno →
corecție 2/50. Originalul și inversa folosesc aceleași conturi vechi;
noua deltă folosește politica nouă. Se probează anularea și cazul delta
zero sub schimbarea politicii, factura de avans și TVA capitalizat.

Suprafață: frunzele NIR/linii, contractul structural al sursei, generarea
conexului și corecția, politica/seed/migrația EF, operand/declarant,
Materializare și gardienii, cititorul provenienței, reconciliere și
DiagnosticValoriStoc, API/DTO/metadata/client pentru cauza și imputatul.
Recensământul 098-r2 este numai citire pe clona Flax, compară liniile și
loturile sursei/conexului; nu este gate și nu rescrie importul.

Oprire: toate probele numerice și integral ambele profiluri verzi,
metadata/OpenAPI aliniate, documentație actualizată, fără commit până la
review-ul advers de după NIR. Nu închide 099-r1/r2/r3, avizul pe 408,
TR-D8 general sau TR-D9.

Proba avansului folosește 4091 privat / 409.01.01 bugetar și cere D avans
100/C furnizor 100, fără lot, cantitate sau NIR. Seed-ul bugetar corectează
clasa 409.01.01 din D (combustibil) în S (fără stoc), conform 099(d).
Nu reclasifică postările existente.

Corecturile review-ului 1945: citirea recepției este un fapt reutilizat pe
durata comenzii și eliberat la ieșire; citirea pentru operare are loc după
blocarea sursei. Editarea exclusiv cantitativă nu citește sursa în gardianul
de editare. Imputatul este golit pe server când cauza nu este Imputabila
sau când delta este zero; clientul îl golește la schimbarea cauzei.
SC-NIR-32…36 probează migrarea/corecția, costul citirii, analiza istorică,
imputatul inert și partida FCT bugetare pe 408.00.00.
