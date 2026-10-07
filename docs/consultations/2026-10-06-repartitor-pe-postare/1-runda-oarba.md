# Runda 1 (oarbă): cum poartă postarea repartitorul

Ești consultat ca analist independent. Nu ai cod și nu ai acces la proiect.
Judeci din regula contabilă românească (OMFP 1802, planul general de conturi,
declarațiile D300, D394, SAF-T) și din coerența unui model de date. Nu ți se
spune care formă e implementată azi și nici ce preferă echipa.

## Domeniul

Un motor de contabilitate și gestiune. Toate efectele unui document se scriu
într-un singur jurnal de postări, append-only. O postare este un picior: un
cont pe o latură.

- **Măsuri**: cantitatea și valoarea.
- **Mișcarea** este forma obișnuită a unei linii de document: două capete cu
  aceeași valoare. Capătul sursă e pe credit, capătul destinație e pe debit.
- **Transferul** este o mișcare între două capete ale aceluiași cont. Se
  scrie pe aceeași latură, cu valori de semn opus, ca să nu producă rulaj.
  Exemple: transferul între gestiuni proprii, împerecherea unei plăți cu o
  factură.
- **Unitatea** este identitatea urmărită pe un cont: lotul pe conturile de
  stoc (FIFO pe lot), partida pe conturile de terți (factura deschisă, cu
  partenerul în identitate), fișa pe imobilizări.
- **Repartitorul** este noțiunea generică de „cine sau unde". Nomenclatorul
  lui are cinci feluri: partener și angajat (terți externi), gestiune și
  unitate internă (interne), cont propriu (casă sau bancă).
- **Documentul** are două laturi, predătorul și primitorul, fiecare un
  repartitor. Fiecare tip de document declară ce fel admite pe fiecare latură.
  Exemple: factura de intrare e terț către gestiune; bonul de consum e
  gestiune către loc de consum; plata e cont propriu către terț; nota de
  transfer e gestiune către gestiune. O linie poate avea repartitor explicit,
  diferit de cel al antetului.
- **Faptul fiscal** nu are rânduri proprii. E un set de atribute pe postarea
  de bază și pe postarea de taxă ale liniei: tipul de taxă, sensul, perioada,
  datele fiscale. D394 cere partenerul fiecărui fapt.
- **Planul de conturi** poartă politica: ce conturi urmăresc partide, ce
  conturi cer obligatoriu analiză pe repartitor, ce conturi au rol de terț.

Ce se citește din jurnal: balanța și fișa contului; soldul analitic pe cont și
repartitor; stocul pe lot și gestiune; partidele deschise pe partener;
jurnalele de TVA, D300, D394; SAF-T. Soldurile se țin și ca snapshot-uri
cumulate pe cheia de coordonate, pe perioadele de referință.

Invarianți: pe fiecare tranzacție, debitul egalează creditul; pe fiecare
produs, suma cantităților din tot jurnalul e zero, deci capătul din afara
evidenței ține contracantitatea; o cantitate în stocul real cere gestiune,
produs și lot.

## Formele candidate

**Forma 1: două coloane tipate, completate selectiv.** Postarea are
`Partener` și `Gestiune`.

- `Gestiune` pe piciorul intern e gestiunea, unitatea internă sau contul
  propriu. Pe piciorul din afara evidenței e o gestiune virtuală: Furnizor,
  Client, Consum, Inventar, Transformare. Virtualele nu au rând de
  nomenclator.
- `Partener` stă numai pe conturile de terți care urmăresc partide, unde vine
  împreună cu partida, și pe postările cu fapt fiscal, unde e partenerul
  fiscal.
- Urmare: o postare poate avea ambele coloane (4426 debit: gestiunea internă
  și partenerul fiscal; 401 credit la recepție: virtuala Furnizor și
  furnizorul), una singură sau niciuna (401 credit la o factură de servicii,
  pe un cont care nu urmărește partide).
- Varianta 1b: partenerul stă și pe piciorul de terț al conturilor care cer
  analiză pe repartitor, fără partidă.

**Forma 2: ambele coloane pe toate postările documentului.** Fiecare postare
a unui document cu terț poartă și gestiunea internă, și partenerul.

**Forma 3: un singur `Repartitor`, tipat de cont.** O singură coloană.
Contul declară ce fel de repartitor suportă: terț, gestiune, cont propriu sau
niciunul. Postarea poartă repartitorul capătului ei, de felul contului.
Partenerul fiscal trece în blocul de atribute fiscale. Capătul din afara
evidenței e marcat altfel decât printr-o gestiune.

**Forma 4: fluxul și gruparea.** Trei coloane.

- `RepartitorIesire` și `RepartitorIntrare` sunt repartitorii celor două
  capete ale mișcării, scriși pe amândouă postările ei. Sunt atribute de
  eveniment: jurnal, filtre, rulaje, partenerul din D394.
- `RepartitorGrupare` e singura coordonată de sold: repartitorul capătului
  propriu, când contul grupează pe repartitor; altfel nul sau o valoare fixă.
- Semantica și filtrul admis pe fiecare coloană depind de natura contului.
- Virtualele Furnizor și Client dispar: capătul din afara evidenței e chiar
  terțul, cu rând real.

**Forma 5: gruparea și cheia de pereche.** Un singur repartitor de grupare,
ca în forma 3, plus o cheie care leagă cele două postări ale unei mișcări.
Capătul celălalt se citește prin legătură. Transformările și deschiderile nu
au pereche.

## Ce ți se cere

1. Clasează formele după coerență și simplitate. Motivul fiecărei poziții,
   într-o propoziție.
2. Scrie postările, cu toate coloanele de repartitor, în cel puțin trei
   forme la alegere, pentru cazurile de mai jos. La fiecare formă spune ce
   sold nu se închide sau ce informație se pierde.
   - a. Recepție de marfă cu TVA: 371 = 401 și 4426 = 401, apoi plata
     401 = 5121, apoi bon de consum din același lot.
   - b. Transfer între două gestiuni, pe același cont 371.
   - c. Compensare între doi terți: 401 al lui A cu 4111 al lui B.
   - d. Virament intern între două conturi bancare.
   - e. Factură de servicii 628 = 401, pe un cont 401 care nu urmărește
     partide, dar cere analiză pe repartitor.
   - f. Asamblare în aceeași gestiune: se consumă lotul L1, se produce L2.
   - g. Decont de cheltuieli al unui angajat, cu TVA.
   - h. Deschiderea soldurilor, fără document.
3. Partenerul fiscal: coordonată de sold, atribut al faptului fiscal sau
   valoare dedusă din document? Judecă pe D394, pe storno și pe corecția
   care schimbă partenerul.
4. Gestiunile virtuale: sunt un concept necesar? Ce le înlocuiește în
   formele 3, 4 și 5 pentru conservarea cantității? Ce se întâmplă cu Consum,
   Inventar și Transformare, care nu au terț?
5. La forma 4: gruparea se poate deduce la citire din latură, semn și natura
   contului, sau trebuie scrisă? Ce înseamnă intrare și ieșire la storno și
   la transferul cu valoare negativă? Ce se întâmplă când capătul nu e o
   latură a antetului?
6. „Contul declară ce fel de repartitor suportă": e corect la nivel de cont,
   sau trebuie cont și tip de document? Judecă pe conturile cu folosire
   mixtă: 461 și 462, 542, 581, 4091 și 419, 408 și 418, stocurile aflate la
   terți.
7. Agregate și snapshot-uri: cum crește cheia la fiecare formă și ce cost are
   pe un jurnal partiționat de milioane de rânduri.
8. Schimbarea coordonatelor unui jurnal append-only: ce dovezi ceri
   înaintea ei și dacă se poate face aditiv.
9. Un contraexemplu numeric pentru fiecare formă, dacă există.

## Ce livrezi

1. Clasamentul și verdictul, cu motivul principal.
2. Răspunsurile numerotate.
3. Lista faptelor care ți-ar schimba verdictul dacă le-ai afla din cod sau
   din date reale.
4. Încrederea, de la 1 la 5.

Nu fi diplomat. Cel mult 2.400 de cuvinte, în română.
