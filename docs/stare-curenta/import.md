# Import și migrare

**Actualizat: 2026-10-18.** [Index](README.md)

Importul din 1C, opțiunile rulării, reconcilierea cu sursa și deschiderea importată.

## Import operațional 1C

Importul lucrează într-o bază dedicată, cu mapări explicite și verificare
prealabilă. Documentele sunt operate prin motor în ordinea timestamp-ului
sursei; nu se copiază registre pentru a evita regulile domeniului.
Tranzacția operațională este per document. (45a, 47a, 50a)

`MigrareLegatura` asigură legătura și idempotenta importului. Recuperarea
după un commit fără legătură și reluarea drafturilor folosesc identitatea
stabilă de import. Configurația legacy nu este importată ca limbaj de
politici, iar instrumentele de migrare nu impun o bibliotecă de domeniu
comună cu aplicația veche. (45f, 47b, 50a)

Conectorul păstrează nominalizarea sursei. La loturi, linia poartă lotul
numit de 1C. La partide, rândul de compensare sau de operație care are un
document în subconto-ul „Documente” numește pe linia de notă partida acelui
document, fie ea deschisă de un document importat sau inițială; trecerea 2 nu
mută o partidă inițială pe care stingătorul a numit-o la operare. Slotul
„Documente” prezent și gol numește poziția fără document a (cont, partener).
Un document numit fără poziție la graniță și fără document în Atlas lasă
linia pe FIFO și se numără (E4-r1). Unde sursa nu numește, conectorul dă
cheia de ordonare: partida inițială primește data documentului 1C ca dată de
naștere. (115, 132 b)

Partidele inițiale intră în deschidere ordonate pe cheia sursei (simbol,
partener, document 1C), iar la aceeași dată de naștere FIFO le consumă în
această ordine. Două importuri ale aceluiași binar dau același raport de
reconciliere, aceleași legături și aceleași partide pe chei naturale. (116)

Nomenclatoarele sunt create la nevoie. Identitatea materialului importat
ține cont de catalog și cont; lotul, de document × produs × cont. Mapările
de cont sunt explicite și se verifică înainte de import. (47d, 48c, 50a)

NTC este punte numai pentru cazurile permise explicit, nu fallback universal
pentru documente nerecunoscute. Transformările și transferurile sunt
clasificate în ASM, BTR sau NTC după faptul economic. FCL importată postează
venitul; DSC folosește loturile identificate de sursă. Puntea reclasificării
de cont la transfer preia numai rândurile pe care ASM-ul nu le postează.
Linia notei-punte se rotunjește la scara de bani, iar ce taie rotunjirea e
divergență numită; o linie rotunjită la zero se sare. (49a, 49d, 75b,
132 a, 134 d)

Proba supremă a motorului e catalogul de scenarii, nu importul
(`docs/nucleu/scenarii/README.md`): așteptări scrise de mână, ciclul complet
per tip, lanțurile transversale, pe ambele profiluri. Clona Flax rămâne sursă
de întrebări (recensământ, 091-r2); Import1C e felia de migrare (091-r4),
după „rotund" (091 (g)). (091)

### Opțiunile rulării

Fără connection string dat, Import1C scrie în `Atlas.Conta.Import1C.Flax`
pe instanța nativă (5446). Pe orice conexiune care nu spune altfel pornește
pregătirea automată a comenzilor (`Max Auto Prepare=300`,
`Auto Prepare Min Usages=2`). Cu ea, o citire cu parametri de care depinde
selectivitatea poate trece pe planul generic; citirile pe loturi sunt scrise
să nu depindă de asta (125). Pe anul 2025, pe 5446, cu `--inchide-lunile`,
importul durează 1:23:10 pentru 187.033 de documente: o lună între 5:45 și
7:41, contractul lunar 5–8 s. Sursa 1C n-are indecși pe `DefalcareNote` și
`NoteContabile`, așa că reconcilierea o citește pe lot, nu pe poziție (134).
Conexiunile importului nu se resetează la întoarcerea în pool
(`No Reset On Close=true`, pe orice conexiune): importul nu lasă stare de
sesiune (fără `SET`, advisory lock de sesiune sau tabele temporare), iar
resetarea era jumătate din instrucțiunile trimise pe document. (125d)

`--pana-la-ziua Z` taie ultima lună a ferestrei (`--pana-la`) la ziua Z,
pentru rulări scurte de măsurare: sursa se citește numai până la acea zi,
stingerile lunii se aplică, iar închiderea de TVA și contractul lunar nu
rulează, fiindcă se compară cu sfârșitul de lună al sursei. Raportul spune că
luna e tăiată, iar verdictul e „contract lunar nerulat”, nu „îndeplinit”. Nu
se combină cu `--inchide-lunile`.

`--inchide-lunile` închide fiecare lună fără eșec imediat după importul
ei, prin `PerioadaService.Inchide` cu constatările curente acceptate pe
cheie (politica de închidere a bazei de import coboară blocantele de
conținut la avertisment — politică, nu ocolire). Rezultatul lunii îl
închide IRZ-ul motorului, generat după ITV; rândurile închiderii 1C dintre
121 și 6x/7x nu se transcriu, iar diferența pe perechea de conturi, ținută
într-o singură orientare, e divergența „închiderea rezultatului”. Când
fereastra ajunge la decembrie și toate lunile anului sunt închise, importul
închide și exercițiul prin `PerioadaService.InchideExercitiul`, cu
constatările acceptate pe cheie; o constatare blocantă pică rularea.
Soldurile de la 01.01 ale anului următor sunt cele de la 31.12 și se
compară pe cont cu `Balanta` 1C (contractul 1 al lui decembrie), pe stoc cu
`BalantaNivel3` (contractul 3), pe partide cu `BalantaNivel1` (contractul
5). (134 a, b) E și proba soldurilor materializate: raportul de
reconciliere trebuie să rămână identic cu cel al rulării fără închideri,
adică importul citește peste snapshot-uri, nu peste registrul integral, și
scrie aceleași cifre. (F27-D1, F27-D3)

După un import, SQL-ul din `--dump-integritate-tph` se rulează pe baza de
import: zero rânduri pe fiecare interogare. Interogările pe tipuri și legături
pe care importul nu le produce sunt vacue pe import; pe acelea le acoperă
ModelCheck. (89g, 89h)

## Reconciliere și migrare legacy

Reconcilierea recitește PostgreSQL după operare. Compară pe luni conturile,
TVA-ul, creanțele/datoriile și stocul pe produs × gestiune. Toleranța numerică
este 0,005; o diferență neexplicată este eșec, nu motiv pentru ajustarea
ascunsă a valorii postate. Explicațiile trebuie sprijinite de documentele
sursei. (45e, 47e, 51d)

Rulajele pe lot nu sunt țintă când identitatea lotului nu este comparabilă
structural. Evaluarea exactă, excepția returului fiscal și efectele
retroactivității se verifică separat. (45e, 47a, 75c)

### Contractele reconcilierii pe cub

Import1C citește cubul prin `Cub/Citiri` și nu referă registrele. Contractul
1 citește `Contabil.Postari`, intrarea balanței. Contractul 2 reface perechea
debit / credit a unei linii ITV din cele două postări ale ei; o linie de altă
formă e eșec, nu se sare. Contractul 3 citește `Loturi`; la deschidere,
celulele sursei cu valoare fără cantitate, care nu intră în cub, sunt o
categorie de justificare numită, pe exact cheile măsurate. Predicția de
valoare a alocării cheamă `Nucleu.Evaluare.Iesire` pe soldul din cub, pe
orice ieșire evaluată din sold. (D9-D11; `docs/import/faza-1c-design.md` §16)

### Amortizarea față de 1C

Formula amortizării se reconciliază cu cifrele postate în 1C prin blocul
`RECONCILIERE-MF` din ModelCheck, condiționat de fixture-ul gitignored
`1C/mf/` (parametrii datați și rândurile lunare per activ): recalculul cu
`AmortizareService.CotaLunara` se compară lună cu lună, potrivirile și
diferențele se raportează cu activ, lună, așteptat și postat; blocul nu pică
pe diferențe, doar pe fixture malformat. După orice atingere a motorului de
operare, raportul integral trebuie să rămână identic cu baseline-ul. (87c, 87k)

### Imobilizările migrate

Import1C face recensământul MF la graniță înaintea deschiderii cubului. Un MF
cu parametri de amortizare și sold pe un singur cont de activ devine fișă în
PIF-ul de deschidere: valoarea = soldul pe activ, amortizarea = soldul pe
contul 28x principal, lunile rămase = restul în cote fixe 1C, rotunjit la
întreg. Perechea 1C (activ, amortizare) care diferă de politica tipului
primește un tip geamăn. Soldurile fără fișă se raportează pe motiv, iar
controlul pe cont e Balanța = fișe + fără fișă + fără subconto MF. Soldul
fără subconto MF de semn opus naturii contului e Δ declarat la deschidere.
(133 a, b)

Pe conturile de imobilizări (20x, 21x, 28x), soldul de control al
deschiderii stă pe SEDIU, repartitorul pe care postează notele; gardianul
poziției fără fișă grupează pe repartitor. (134 c)

În lunile importate, PIF-ul 1C devine PIF Atlas la data sursei, iar
vânzarea și casarea unui MF cu fișă devin CAS, cu cauza lor; CAS-ul acoperă
rândurile cu subconto MF cu fișă, restul se transcriu. Amortizarea lunii o generează motorul
(AMO), înaintea ITV-ului; rândurile 68x = 28x ale închiderii 1C pe MF cu
fișă nu se transcriu. Diferențele față de 1C sunt divergențe numite: cota
Atlas față de cota 1C și ieșirea fișei. (133 e–g, 134 g)

### Deschiderea importată

Prototipul legacy migrează nomenclatoare și solduri de deschidere la granița
aleasă. Istoricul rămâne în sursă. Deschiderile contabile folosesc convenția
de cont de deschidere, iar soldurile terților nu sunt transformate în facturi
inventate. Legăturile de migrare fac reluarea identificabilă și idempotentă. (34a, 34b, 34d)

Import1C scrie deschiderea prin `Materializare.Deschide`, o dată per bază:
controale brute per (cont, latură), loturile ca detaliu al conturilor de stoc,
terții ca partide inițiale `(cont, partener, document de decontare)` din
`BalantaNivel3`, pozițiile fără partener pe partenerul generic de migrare
`MIGRARE-NEDEFINIT`. Stingerile anului pe pozițiile de deschidere se scriu în
cub pe partidă, în trecerea 2, plafonate la rest, în ordinea datei
stingătorului; data împerecherii este a faptului. (134 e) Trecerea 2 enumeră și încasările pe facturi ale raportului de
vânzări cu amănuntul (copiii `#inc`). Împerecherea pe documente se
plafonează la restul ambelor documente, cu excedentul raportat. (132 c, d) La reluare deschiderea nu se rescrie: fiecare partidă și fiecare
lot al sursei se compară cu cubul, în ambele sensuri. Contractul 5 al
reconcilierii compară restul fiecărei partide inițiale cu soldul poziției în
sursă la luna următoare, corectat cu mișcarea neaplicată (refuzuri și
plafonări semnate după sensul real, cumulate, rederivate din cub la reluare);
partidele partenerului generic se declară cu sumă. O partidă rămasă fără
explicație sau neatinsă de trecerea 2 se atribuie pe documentele sursei:
mișcarea pe poziție din 1C față de mișcarea Atlas pe partidă, document cu
document. Fiecare parte are o categorie (stingere refuzată sau plafonată,
inclusiv netarea 1C a unei poziții de sens opus în plată; rând ținut pe
partida documentului; notă cu latură numită lăsată pe FIFO; notă peste
restul partidei numite, cu excedentul pe FIFO; notă care mărește partida
numită în sensul ei, ținută pe partida notei), iar suma părților închide
Δ; altfel partida pică. O neatinsă fără nume pe document rămâne declarată
cu sumă. O partidă are refuz când trecerea 2 a refuzat cel puțin o
stingere pe ea. (132 e, 133 c, h, 134 f, h) Reziduul golirii lotului la
retur, pe contul politicii de diferențe, se declară divergență după
operare (133 d). O eroare tehnică a unei stingeri pică luna și nu explică nimic.
`--probe-stingeri` injectează două erori pe prima stingere reală. Contractul
și cifrele:
`docs/import/m1-deschidere-terti-contract.md`. (107)

## Limitele implementării

- Conectorul 1C rulează prin motor numai pe deschidere și pe trecerea 2 a
  lunii: deschiderea trece prin comanda de deschidere, iar stingerile anului
  pe partide inițiale se scriu în cub; fără deschiderea în cub, Import1C nu
  rulează. Stingerile prin facturi rămân în afara trecerii 2; contractul 5
  le atribuie pe document sau le declară cu sumă (134 h). Pozițiile în valută
  intră în lei, plafonate la rest (107-r4). (107, 107 j)
- Reconcilierea Import1C e verde pe anul 2025, cu lunile și exercițiul
  închise; ianuarie 2026 și granița intră la pasul 4 al etapei E4
  (`docs/import/e4-migrarea-contract.md`).
  Sensibilitatea contractelor 1 și 3 n-are probă proprie, iar unealta n-are
  un mod de reconciliere fără scriere. (D9-D11, 132–134)
- O linie de notă care numește un document fără poziție la graniță rămâne
  pe FIFO, iar contractul 5 o numește (E4-r1).
- Reproductibilitatea importului e măsurată pe ianuarie–martie 2025, pe chei
  naturale (116, 134 e).
- Reevaluarea MF din 31.12.2025 intră ca notă pe poziția fără fișă: Atlas
  n-are document de reevaluare pe fișă, iar fișa rămâne la valoarea
  dinainte (134).
- Reluarea parțială cu `--continua` poate afecta interpretarea reconcilierii;
  validarea completă cere o bază și un set de referință controlate, cu starea
  reluării cunoscută. (75-r3)

<!-- gen:decizii -->
## Decizii

Deciziile care numesc pagina în `Arii:` (19; titlurile în [index](../decizii/README.md); generat de `docskit gen`, nu se editează): 34, 45 (amendată de 75), 47 (amendată de 107), 48 (amendată de 76), 49, 50, 51, 75, 87 (amendată de 97), 89, 91 (amendată de 107, 118, 120), 107, 115, 116 (amendată de 117), 125, 130, 132 (propusă), 133 (propusă), 134 (propusă).
<!-- /gen:decizii -->
