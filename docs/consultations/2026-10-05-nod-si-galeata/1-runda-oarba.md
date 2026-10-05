# Runda 1 (oarbă): transformarea de stoc și poziția fără unitate

Ești consultat ca analist independent. Nu ai cod și nu ai acces la proiect.
Judeci din regula contabilă românească (OMFP 1802, planul general de conturi)
și din coerența unui model de date. Nu ți se spune care formă e implementată
azi și nici ce preferă echipa.

## Domeniul

Un motor de contabilitate și gestiune. Toate efectele unui document se scriu
într-un singur jurnal de postări. O postare are coordonate și măsuri.

- Coordonate: cartea (contabilă sau fiscală), contul, latura (debit sau
  credit), partenerul, gestiunea, produsul, unitatea, dimensiunile analitice,
  data, documentul și linia care au cauzat-o.
- Măsuri: cantitatea și valoarea.
- **Unitatea** este identitatea urmărită pe un cont: lotul pe conturile de
  stoc (evaluare FIFO pe lot), partida pe conturile de terți (factura deschisă,
  cu partenerul în identitate), fișa pe conturile de imobilizări.
- **Mișcarea** este forma obișnuită a unei linii de document: două capete,
  aceeași valoare, debit pe capătul de destinație cu cantitatea pozitivă,
  credit pe capătul sursă cu cantitatea negativă.
- **Transferul** este o mișcare între două capete ale aceluiași cont. Se scrie
  pe aceeași latură cu valori de semn opus, deci nu produce rulaj pe cont.
  Exemple: transferul între gestiuni proprii, împerecherea unei plăți cu o
  factură.
- **Gestiunile virtuale** sunt capete din afara stocului real: Furnizor,
  Client, Consum, Inventar, Transformare. Nu au rând în nomenclator. O postare
  într-o gestiune virtuală nu cere unitate.

Invarianții jurnalului:

1. pe fiecare tranzacție și carte, valoarea pe debit egalează valoarea pe
   credit;
2. pe fiecare produs, suma cantităților din tot jurnalul este zero; capetele
   virtuale țin contracantitatea;
3. o cantitate într-o gestiune reală cere gestiune, produs și unitate;
4. un transfer conservă valoarea pe cont și latură.

Exemple de mișcări existente. Recepția: Furnizor către lot. Bonul de consum:
lot către Consum, cu contul de cheltuială luat din politică. Diferențele de
inventar: minusul merge din lot către Consum, plusul vine din Inventar către
un lot nou, contul capătului virtual vine din politică, liniile nu se
compensează între ele.

## Întrebarea 1: asamblarea și dezmembrarea

Documentul consumă M loturi și produce N loturi în aceeași gestiune. Între
cantități nu există proporție: 2 bucăți de A pot deveni 1 bucată de B.
Valoarea totală consumată egalează valoarea totală produsă. Consumul se
evaluează FIFO pe lot. Există două forme candidate.

**Forma X.** Valoarea trece direct de la loturile consumate la loturile
produse. Liniile se grupează pe contul lotului. Un grup în care valoarea
consumată egalează valoarea produsă se scrie ca transfer, fără rulaj pe cont.
Celelalte grupuri se scriu ca debit și credit obișnuite între conturi.
Cantitatea nu poate trece de la lot la lot, fiindcă produsele diferă. De
aceea fiecare linie primește o contrapostare numai de cantitate în gestiunea
virtuală Transformare, cu valoarea zero. Citirile contabile exclud
contrapostările printr-un filtru. Forma are o primitivă proprie în motor.
Un document pe conturi diferite (consum de pe 301, produs pe 345) este
acceptat și dă direct 345 = 301.

**Forma Y.** Fiecare linie este o mișcare obișnuită între lot și gestiunea
virtuală Transformare, pe contul lotului, cu valoarea și cantitatea
împreună. Consumul merge din lot către nod, producția din nod către lot.
Liniile nu sunt legate între ele. Gardul: pe document și pe cont, valoarea
intrată în nod egalează valoarea ieșită din nod. Consecințe: contul de stoc
primește rulaj debitor și creditor egal cu valoarea transformată; nodul ține
pe produs un sold de valoare care se anulează pe cont; citirea valorii pe
cont și produs trebuie să aleagă gestiunile reale. Forma nu are primitivă
proprie: este aceeași cu diferențele de inventar. Un document pe conturi
diferite este refuzat de gard. Trecerea între conturi se face prin alt tip
de document, bonul de producție, unde contul nodului vine din politică
(de exemplu 6xx și 711).

Cere răspuns la:

1. Care formă este mai coerentă și mai simplă, și de ce.
2. Este corect contabil rulajul debitor și creditor pe același cont de stoc
   la o transformare? Ce distorsionează: balanța de verificare, registrul
   jurnal, fișa contului, indicatorii de rotație, declarația SAF-T de
   stocuri?
3. Este corectă nota 345 = 301 sau 371 = 301 scrisă direct, fără conturi de
   cheltuieli și venituri? În ce situații da și în ce situații nu?
4. Pentru trecerea între conturi în forma Y: refuz și bon de producție, cont
   de tranzit sau o linie explicită de trecere între capetele nodului?
5. Un contraexemplu numeric construit de tine care rupe fiecare formă, dacă
   există: rotunjiri la FIFO, storno, corecție după închiderea lunii,
   consumul produsului înaintea stornării asamblării.

## Întrebarea 2: poziția fără unitate

Pe un cont care urmărește unități poate sosi o postare fără unitate. Cazuri
reale: nota contabilă manuală 628 = 401 fără partener; nota 628 = 302 fără
lot și fără cantitate; valoarea pusă pe un cont de imobilizări înainte să
existe fișa, din care punerea în funcțiune nominalizează apoi fișele.
Există trei reguli candidate.

**Regula P.** Trei regimuri, câte unul pe felul unității. La partide,
postarea fără unitate este tolerată, cu excepția cazului în care postarea
are partener sau documentul are un terț extern. La loturi nu există niciun
gard. La fișe există un gard de semn: soldul poziției fără fișă nu poate
trece de partea opusă, evaluat cronologic pe coordonata completă, cu blocaj
serial la scriere; conturile protejate se deduc la fiecare postare din
configurarea tipurilor de imobilizări.

**Regula Q.** Unitatea nulă este membru legitim al modelului. Fiecare cont
care urmărește unități are o „găleată", poziția cu unitatea nulă, cu un
regim declarat pe cont:

- închisă: postarea fără unitate este refuzată;
- deschisă: postarea este acceptată, soldul găleții este liber;
- suport: postarea este acceptată, soldul găleții nu trece de partea opusă.

Reguli însoțitoare: numai nota manuală și deschiderea au voie să posteze în
găleată; documentele tipate numesc unitatea întotdeauna; valoarea trece din
găleată pe o unitate numită numai prin transfer explicit; o unitate numită
se echilibrează pe ea însăși; există un singur raport de reconciliere, soldul
găleții pe fiecare cont. Regimul suport este fixat de profil pe conturile de
imobilizări. Regimurile închisă și deschisă sunt editabile de client pe
conturile de terți și de stoc.

**Regula R.** Unitatea nu este niciodată nulă. Nomenclatoarele au poziții
implicite obligatorii: un partener „Diverși", un lot implicit pe produs și
gestiune, o fișă implicită. Nota fără unitate cade pe poziția implicită.

Cere răspuns la:

1. Care regulă este mai coerentă și mai simplă, și de ce.
2. Ce pierde fiecare la audit și la reconciliere.
3. La regula Q: cele trei regimuri sunt suficiente? Lipsește vreunul? Este
   corect ca regimul suport să nu fie editabil?
4. La regula Q: ce se întâmplă când clientul închide o găleată care are sold?
5. La regula R: ce înseamnă un lot implicit pentru evaluarea FIFO și pentru
   cantitate, când nota nu are cantitate?
6. Interacțiunea cu întrebarea 1: în forma Y postările nodului stau pe un
   cont de stoc, fără lot, într-o gestiune virtuală. Cum se deosebesc de
   găleată și dacă deosebirea ține.
7. Un contraexemplu numeric construit de tine pentru fiecare regulă.

## Ce livrezi

1. Verdictul pe întrebarea 1 și pe întrebarea 2, fiecare cu motivul
   principal într-o propoziție.
2. Răspunsurile numerotate de mai sus.
3. Lista faptelor care ți-ar schimba verdictul dacă le-ai afla din cod sau
   din date reale.
4. Încrederea, de la 1 la 5, pe fiecare verdict.

Nu fi diplomat. Cel mult 2.200 de cuvinte, în română.
