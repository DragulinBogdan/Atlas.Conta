# LDI — diferențe de inventar pe cub

2026-09-24. **Aprobat și implementat (093); rezultate în scenarii/LDI.md.**
Contract părinte: `tr-d7b-tipuri-ramase-contract.md`, T-D9 și T-D13.
Nu schimbă evaluarea duală, schema sau API-ul. LDI-B3, aprobat de owner, schimbă
politica de stoc BTR/BCS pentru Folosinta.

## LDI-B1 — Direcția și contarea

Direcția Plus/Minus este fapt cules, separat de semnul cantității persistate.
Declarantul primește direcția și prețul printr-o interfață de linie și operand
închis. Normalizează cantitatea cu direcția înainte de potrivirea contării;
același draft are același rezultat înainte și după PregatesteOperare.
Nu se deduce direcția din valoare ori din prețul încă nefinalizat al lotului.

Minus: `D cheltuială / C stoc`, valoare pozitivă din Evaluare.Iesire,
cantitate −q pe lot și +q pe Consum fără unitate. Conturile și dimensiunile
vin din regula potrivită; soldul se consumă în ordinea (Pozitie, ID), inclusiv
la două linii pe același lot. Plus: `D stoc / C venit`, valoare rotunjită
din q × PretEvaluare, +q pe lot propriu și −q pe contraponderea Inventar,
fără unitate. Nicio partidă și niciun fapt TVA. O singură Operare pe document.

## LDI-B2 — Forma și refuzurile

Cantitatea absolută este strict pozitivă. Plusul cere preț pozitiv (valoarea
poate fi zero după rotunjire), lot născut pe linia lui în gestiunea
inventariată, produs compatibil cu lotul și tipul. Minusul cere lot existent,
străin liniilor proprii; stoc suficient și regulă de contare rezolvată.
Direcția absentă, natura fără stoc, tipul incompatibil sau regula de stoc
neacoperită refuză explicit, fără persistare parțială.

## LDI-B3 — Domeniul stocului

Magazie/Marfuri păstrează gestiunea inventariată. Custodie rămâne explicit
neacoperită (owner, 2026-09-23); Folosinta se tratează acum pe întregul lanț
(owner, 2026-09-24), cu păstrarea gestiunii reale aprobată prin 093.

**Custodie — lacună de politică de tranșat.** Planul bugetar are zece
conturi 803x, dar clasa MC nu are niciun TipMaterial și nicio mapare explicită
la un cont de custodie. Existența familiei 803 nu selectează un cont.
T-D9 cere postare cantitativă cu valoare zero, însă regula LDI/plus generică
scrie valoare pe contul tipului / 791. Nu este stabilit dacă acea notă se
păstrează separat ori nu se emite pentru custodie.

**Folosinta — continuitatea coordonatelor.** Recepția curentă FCT pentru
tipul bugetar 303.02.00 postează în gestiunea reală. Aplicarea literală a
T-D9 numai la LDI ar descărca apoi Folosinta virtuală, lăsând stocul real
intact și un sold negativ virtual. Maparea trebuie tranșată pe întregul
lanț de documente și pe istoricul deja scris, nu introdusă doar în LDI.

Un al doilea contraexemplu: un BTR cu același lot între MAG1 și MAG2 devine
−q/+q pe exact aceeași coordonată dacă ambele gestiuni sunt înlocuite cu
constanta Folosinta. Se pierde cine deține stocul. În plus, seed-ul BTR/BCS
citea Magazie generic, în timp ce NIR/LDI scrie Folosinta pentru OF;
un minus pe LDI este posibil, dar BTR/BCS nu găsesc acel sold în registre.

**Amendament aprobat la 090(g)/T-D9 (093):** pentru obiectele proprii în
folosință, Folosinta păstrează gestiunea reală și unitatea lot. Natura
stocului se identifică prin contul politicii (303.02.00 în seed-ul bugetar),
ca Magazie/Marfuri. Nu se adaugă axă, cont, tabelă sau gestiune virtuală și
nu se reutilizează o axă Analiza cu altă semantică.

Lanț numeric: FCT/NIR în MAG1, 4 bucăți × 25 = 100 pe 303.02.00;
BTR mută 2/50 în MAG2 pe același lot; LDI minus 1/25 în MAG2 scrie
D 603/C 303.02.00: 25; BCS consumă ultima 1/25 din MAG2.
Sold final MAG1 2/50, MAG2 0/0. Plus LDI 1/25 în MAG2 scrie
D 303.02.00/C 791.00.00: 25, pe lot nou; nota și cantitatea se inversează
exact la storno. Maparea Folosinta se aplică politicilor BTR pe ambele
laturi și BCS pe sursă; destinația de consum rămâne comportamentul BCS
curent. FCT are deja coordonata corectă. LDI o păstrează.

Corectură a exemplului inițial: 603.02.00 nu există în planul seed-uit;
politica existentă derivă 603 (prin tăierea segmentelor din simbol).
Proba fixează 603, fără să schimbe politica de contare sau să adauge cont.

Istoric: nicio rescriere a cubului. Recepțiile deja scrise pe gestiunea
reală rămân compatibile, inversările folosesc postările originale. Motorul
de storno al registrelor copiază TipStoc, lotul și repartitorul original,
fără să potrivească din nou politica. SC-LDI-22 probează inversarea după
schimbarea politicii; compatibilitatea cubului singură nu ajunge.

Datele istorice în registre pe altă cheie (Magazie în loc de Folosinta)
se raportează separat pe lot × repartitor × TipStoc și document, fără
mutare automată. Diagnosticul valoric inițial compara numai Magazie/Marfuri;
extinderea lui la Folosinta nu este suficientă dacă însumează cele două
tipuri și ascunde distribuția greșită. Inventarierea cheilor istorice trebuie
păstrată distinctă de comparația valorică. Diagnosticul o listează pentru
loturile cu mișcări Folosinta sau din clase cu politică Folosinta curentă,
inclusiv originalul și inversul cu sold net zero. Reoperarea/corecția folosește
politica curentă; nu va primi sold printr-un fallback la alt TipStoc.

Probele folosesc documente noi. Recensământul anterior schimbării seed-ului
pe cele două baze `.CodexBCS` a găsit zero rânduri de registru pentru clasa
OF (`run-nucleu/tr-d7b/pas5-ldi/istoric-folosinta-inainte.json`). Aceasta
descrie bazele de probă, nu certifică istoricul altor baze.

Contra invarianților I–IV: lanțul rămâne documentat prin operații reale,
tipul stocului rămâne politică, lotul și gestiunea își păstrează semantica,
iar inversările păstrează datele originale. Alegerea unei constante virtuale
unice, fără altă coordonată pentru deținător, nu poate reprezenta transferul
între două gestiuni; alternativa cere un contract distinct pentru acea axă.

Custodie refuză atomic cu cod stabil. T-r6/TR-r7 rămân active pentru
Custodie/Gratuit și pentru injectivitatea completă a politicii SAF-T.
Înaintea activării MC se fixează contul din politică și perechea completă a
postărilor, inclusiv tratamentul notei valorice. Folosinta în gestiune reală
este aprobată de owner și consemnată în 093.

## LDI-B4 — Probe și limite

Catalog numeric independent în `scenarii/LDI.md`: plus, minus, mixt,
golire și lot repetat, storno în lună și peste graniță, anulare/reoperare,
corecție, dependenți, document întârziat, refuzuri, fără TVA și performanța
citirii operandului. Fixture-uri prin documente reale, fără inserții de cub
sau registre, pe ambele profiluri. Scenariile verifică postările și soldurile
directe; cititorii comuni rămân TR-D8. Evaluarea pe propriul sold al cubului
și reziduurile duale rămân T-r13/TR-D9. Import1C nu este gate (091).
