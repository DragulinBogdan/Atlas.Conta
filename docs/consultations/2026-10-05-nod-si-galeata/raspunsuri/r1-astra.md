**Verdictul 1: aleg X pentru transformările care sunt redistribuiri de stoc, deoarece conservă valoarea fără solduri valorice artificiale pe produse; resping însă acceptarea automată a notei 345 = 301 pentru producție. Încredere: 3/5.**

**Verdictul 2: aleg Q, deoarece exprimă explicit aceeași problemă — valoare nenominalizată — printr-o politică verificabilă pe cont; raportul exclusiv de sold net trebuie însă completat. Încredere: 4/5.**

Acestea sunt judecăți asupra modelelor descrise, fără acces la proiect. Presupun inventar permanent.

**1.1. Coerență și simplitate.** X este mai simplă pentru cititorii jurnalului; Y este mai simplă pentru implementarea scrierii. Prefer prima economie: rapoartele și reconcilierea sunt consumatori permanenți ai datelor.

În X, contracantitățile cu valoare zero nu schimbă soldurile valorice nici dacă filtrul este uitat. Tot trebuie identificate explicit pentru rapoartele cantitative. În Y, uitarea filtrului schimbă valoarea atribuită produselor, chiar dacă totalul contului rămâne corect.

Totuși, egalitatea valorilor pe cont nu dovedește că operațiunea este transfer. Două operațiuni economice distincte pot avea accidental aceeași valoare. **Natura documentului trebuie stabilită înaintea grupării și compensării.** Nici X, nici Y nu poate deduce tratamentul contabil numai din egalități numerice.

**1.2. Rulajul și rapoartele.** Debitarea și creditarea aceluiași cont nu sunt incorecte prin ele însele: pot reprezenta mișcări între analitice. Nu rezultă însă că orice trecere printr-un nod tehnic justifică rulaj contabil suplimentar.

Mai întâi, Y are o problemă aritmetică în descriere. Pentru transformarea a 100 lei:

| Mișcare | Debit | Credit |
|---|---:|---:|
| Lot consumat → Transformare | nod: 100 | lot: 100 |
| Transformare → lot produs | lot: 100 | nod: 100 |
| Total pe același cont | **200** | **200** |

Rezultă câte **2V**, nu V, dacă toate postările intră în rulaj. V apare numai după o eliminare sau convenție suplimentară.

Consecințele:

- **Balanța:** soldul final rămâne corect; rulajele cresc cu tranzitul tehnic. Egalitatea balanței nu validează sensul economic.
- **Registrul-jurnal:** apar două autorelații contabile pentru aceeași transformare. Trasabilitatea există, dar totalurile includ mecanica nodului.
- **Fișa contului:** aceeași încărcare; pe produs, includerea nodului poate anula tocmai schimbarea economică urmărită.
- **Rotația:** indicatorii calculați din rulajul brut sunt umflați; cei calculați din costul vânzărilor și stocul mediu corect nu sunt afectați automat.
- **SAF-T Stocuri:** nu este automat greșit din cauza rulajului. Exportul trebuie să reprezinte stocurile și mișcările efective, fără dublarea lor prin contracapetele tehnice. ANAF separă `PhysicalStock` de `MovementOfGoods` și cere detalierea stocurilor pe produs și gestiune. [Ghid ANAF](https://static.anaf.ro/static/10/Anaf/Informatii_R/SAF_T_Ghidul_D406_1712021.pdf)

Nici X nu trebuie să ascundă ieșirile și intrările fizice din SAF-T doar fiindcă rulajul sintetic este zero.

**1.3. Notele directe.** `371 = 301` este justificată pentru materii prime destinate vânzării ca atare; corespondența apare în funcțiunea contului 301. Pentru producția efectivă, traseul uzual este `601 = 301`, respectiv `345 = 711`, la costul corespunzător. [OMFP 1802, funcțiunea conturilor](https://legislatie.just.ro/Public/FormaPrintabila/00000G0DQOAC06J0CD81NUVOYWZUMXCX)

`345 = 301` poate avea sens ca reclasificare documentată a unui produs finit înregistrat greșit la materii prime sau ca revenire asupra unei reclasificări. Nu este formula generală a fabricării unui produs din materiale.

Exemplu: materiale 100 lei, produs obținut 100 lei. Nota directă păstrează stocul total și rezultatul net, dar omite consumul și variația stocurilor aferentă producției. Faptul că manopera suplimentară este zero nu transformă automat producția în reclasificare.

Dezmembrarea pentru recuperări din casare nu este nici ea automat o transformare conservativă a valorii integrale.

**1.4. Trecerea între conturi în Y.** Aleg **refuzul pentru documentul restrâns de transformare**, urmat de documentul potrivit naturii operațiunii:

- producție reală: bon de producție;
- reclasificare reală: document de reclasificare, fără cheltuieli și venituri inventate.

Dacă produsul cere neapărat un singur document Y pentru reclasificări, aleg linia explicită între capetele nodului. Pentru 100 lei din 301 în 371, aceasta este debit pe nodul 371 și credit pe nodul 301. Gardul trebuie atunci să includă și această linie; altfel o respinge.

Linia trebuie justificată și autorizată prin tipul operațiunii, nu generată automat ca să „iasă egalitatea”. Nu repară rulajele suplimentare ale Y. Un cont de tranzit adaugă încă o reconciliere și nu legalizează o producție contabilizată greșit.

**1.5. Contraexemple și limite.**

**X — compensare dependentă de un ban.** Inițial: pe 301 se consumă și se produce câte 100 lei; pe 371, câte 50 lei. X scrie transferuri. O corecție a repartizării produce 99,99 lei pe 301 și 50,01 lei pe 371, consumurile rămânând identice. Totalul rămâne 150 lei, dar ambele grupuri trec la debit/credit obișnuit. O modificare de un ban schimbă masiv rulajele. Nu rupe egalitatea jurnalului; rupe stabilitatea semantică a regulii de grupare.

**Y — valoarea produselor.** Din 2 A, valorând 100 lei, rezultă 1 B, valorând 100 lei. După document, în real: A = 0, B = 100. În nod: A = +100, B = −100. Citirea pe cont și produs, fără filtrul real, arată A = 100, B = 0. Gardul trece perfect.

**Ambele — rotunjire FIFO.** Cost consumat 0,05 lei, repartizat egal către două loturi produse. Rotunjirea independentă la 0,03 + 0,03 creează un ban. Trebuie repartizat determinist 0,03 + 0,02. Y refuză dezechilibrul; X trebuie să-l refuze sau să-l prevină, nu să-l ascundă.

**Ambele — storno după consum.** A de 100 lei devine B de 100 lei; B este apoi consumat integral. Stornarea transformării produce B = −1 bucată/−100 lei și reface A. Invarianții enumerați pot rămâne satisfăcuți! Lipsește controlul disponibilului și al dependențelor. Trebuie corectat lanțul ulterior sau făcută o corecție valorică adecvată, nu un storno fizic imposibil.

Storno trebuie să inverseze loturile și valorile originale, fără reevaluare FIFO curentă. După închiderea lunii, păstrezi istoricul și înregistrezi corecția în perioada permisă ori redeschizi controlat. O lună închisă nu este același lucru cu un exercițiu financiar precedent.

**2.1. Coerență și simplitate.** Q câștigă printr-un singur concept și reguli declarate. P distribuie aceeași problemă în trei mecanisme, dintre care unul depinde de configurări externe recalculate la fiecare scriere. R transformă lipsa informației într-o identitate aparent cunoscută.

Dar „acceptat în jurnal” nu înseamnă „evidență contabilă suficientă”. OMFP cere evidența terților pe persoane; `628 = 401` fără partener nu devine completă prin alegerea regimului deschis. [OMFP 1802, pct. 329](https://legislatie.just.ro/Public/FormaPrintabila/00000G0DQOAC06J0CD81NUVOYWZUMXCX)

**2.2. Audit și reconciliere.**

- **P:** controale neuniforme; lipsa lotului poate rămâne nelimitată. Eliminarea partenerului poate chiar evita gardul destinat terților. Modificarea configurării imobilizărilor poate schimba protecția fără schimbarea contului.
- **Q:** lipsurile sunt vizibile și transferurile explicite păstrează urma. Totuși, soldul net pe cont poate ascunde compensări între produse, gestiuni sau parteneri. Un singur raport este suficient numai dacă permite detalierea, vechimea și identificarea sumelor nealocate.
- **R:** nomenclatoarele par complete, dar identitatea economică lipsește. „Diverși” nu permite confirmarea soldurilor individuale; fișa implicită nu oferă o bază reală pentru amortizare.

La P și Q, istoricul regulilor aplicabile trebuie păstrat. Schimbarea configurației nu trebuie să rescrie verdictul asupra trecutului.

**2.3. Sunt suficiente cele trei regimuri Q?** Da, pentru permisivitatea valorică: refuz, sold liber, sold unilateral. „Suport” trebuie parametrizat prin partea permisă; costul unui activ și amortizarea cumulată au orientări diferite.

Mai sunt necesare reguli independente: cantitate zero în găleată, drepturi, vechime, corecții și închidere. Nu sunt obligatoriu regimuri suplimentare.

Regimul suport fixat prin profil este justificat pentru conturile cu această funcție, dar nu aplicat orbește tuturor conturilor asociate imobilizărilor. Profilul trebuie să distingă costul, amortizarea și ajustările.

Gardul se verifică atomic și cronologic pe cheia poziției, inclusiv asupra soldurilor ulterioare afectate de antedatare. **Data, documentul și linia sunt proveniență, nu componente ale cheii soldului**; altfel alimentarea de ieri nu poate susține nominalizarea de azi.

**2.4. Închiderea unei găleți cu sold.** Refuz trecerea imediată la „închisă”. Clientul trebuie să nominalizeze sau să corecteze justificat soldurile, apoi să închidă cu dată de efect.

Se poate oferi starea operațională „în curs de închidere”: oprește alimentările noi, permite lichidarea controlată. Nu este necesar un al patrulea regim contabil. Nu ștergi soldul și nu îl muți automat într-o unitate fictivă.

Nici soldul agregat zero nu ajunge dacă ascunde poziții nealocate opuse.

**2.5. Lotul implicit și FIFO.** O notă fără cantitate nu creează o intrare fizică. Un lot cu 0 bucăți și 100 lei nu are cost unitar calculabil și nu poate participa normal la FIFO.

Dacă valoarea este un cost suplimentar sau o corecție, trebuie alocată loturilor vizate, inclusiv efectelor ieșirilor deja produse. Dacă lotul implicit este exclus din FIFO și folosit doar pentru sume nealocate, R a recreat găleata Q sub un nume înșelător. Inventarea unei cantități pentru a evita problema falsifică stocul.

**2.6. Nodul Y versus găleata Q.** Deosebirea ține dacă este structurală:

- nodul este un capăt virtual identificat, legat de transformare și controlat pe document, cont și carte;
- găleata este valoare nenominalizată, fără acest rol de contrapartidă tehnică.

`unitate = NULL` singur nu le distinge. Predicatul găleții trebuie să excludă explicit capetele virtuale; utilizatorul nu trebuie să poată evita gardul alegând arbitrar „Transformare”.

Q mai are o contradicție literală: numai nota manuală și deschiderea pot posta în găleată, dar transferul de nominalizare trebuie să o descarce. Excepția trebuie formulată explicit: documentele tipate numesc unitățile economice, însă transferul de nominalizare poate avea găleata drept capăt, iar documentele de stoc pot avea contracapete virtuale.

**2.7. Contraexemple numerice.**

**P:** loturi reale pe 302: 10 bucăți, 100 lei. Nota dată `628 = 302`, 30 lei, fără lot și cantitate, lasă contabil 70 lei și loturile la 100 lei. Lipsa gardului permite divergența. Exemplul testează mecanismul; contul cheltuielii trebuie justificat separat.

**Q:** două poziții fără unitate pe același cont au +100 și −100 lei, pe produse diferite. Raportul „sold găleată pe cont” arată zero și poate permite închiderea, deși ambele poziții sunt nerezolvate. Q rezistă numai cu verificare detaliată, nu exclusiv netă.

Pentru suport: găleată 100 lei, două nominalizări concurente de câte 80 lei. Fiecare trece citind soldul inițial; împreună lasă −60 lei. Regimul declarat nu înlocuiește controlul atomic.

**R:** lot implicit 0 bucăți/100 lei; lot real ulterior 10 bucăți/200 lei. La ieșirea unei bucăți, FIFO nu poate consuma stratul implicit. Dacă îl ignoră, cei 100 lei rămân suspendați; dacă îi distribuie, are nevoie tocmai de o regulă explicită de alocare absentă din R.

**Faptele care mi-ar schimba verdictul:**

- Documentele sunt în realitate producție, reambalare, reclasificare sau recuperări din casare; denumirea „asamblare” nu decide.
- Postările nodului Y sunt excluse structural din proiecția contabilă ori se scriu cu semne diferite de cele descrise: calculul rulajului s-ar schimba.
- X compensează operațiuni independente sau pierde legăturile dintre costuri și loturile rezultate.
- Există costuri suplimentare, pierderi ori producție în curs: conservarea integrală a valorii devine insuficientă.
- R are implicit doar identificatori tehnici marcați ca nealocați, excluși din FIFO și amortizare: atunci se apropie de Q.
- Q are deja reconciliere detaliată, istoric al politicilor și control cronologic atomic: încrederea mea în verdict ar crește.
- Datele reale arată că transferurile, stornările și exporturile necesită excepții numeroase: aș reevalua simplitatea totală, nu numărul primitivelor.