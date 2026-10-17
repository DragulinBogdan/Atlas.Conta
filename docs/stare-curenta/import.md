# Import și migrare

**Actualizat: 2026-10-16.** [Index](README.md)

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
mută o partidă inițială pe care stingătorul a numit-o la operare. Unde
sursa nu numește, conectorul dă cheia de ordonare: partida inițială primește
data documentului 1C ca dată de naștere. (115)

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
venitul; DSC folosește loturile identificate de sursă. (49a, 49d, 75b)

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
să nu depindă de asta (125). Pe ianuarie–martie 2025, pe 5446, importul
costă ~18–21 ms pe document pe ianuarie–iunie 2025, pe 5446 (31:13 pe șase
luni); creșterea rămasă e ~2% pe lună, plus contractul lunar (7 → 27 s).
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

`--inchide-lunile` închide fiecare lună imediat după importul ei, prin
`PerioadaService.Inchide` cu toate constatările curente acceptate (politica de
închidere a bazei de import coboară blocantele de conținut, ca `ItvLipsa` și
`RezNeinchis`, la avertisment — politică, nu ocolire). O lună închisă de 1C
lasă sold pe cheile fine ale lui 6 și 7, fiindcă nota 1C de închidere închide
pe cont: IRZ ar reclasifica între chei, cu netul pe cont egal cu divergența
explicată a importului (`ModelCheck --irz <bază> <an> <lună>`). (E3-D3) E proba supremă a soldurilor materializate: raportul de reconciliere
trebuie să rămână IDENTIC cu baseline-ul rulării fără închideri, adică importul
citește peste snapshot-uri, nu peste registrul integral, și scrie aceleași
cifre. (F27-D1, F27-D3)

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
pe diferențe, doar pe fixture malformat. Import1C nu generează documente de
imobilizări; după orice atingere a motorului de operare, raportul integral
trebuie să rămână identic cu baseline-ul. (87c, 87k)

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
cub pe partidă, în trecerea 2, plafonate la rest; data împerecherii este a
faptului. La reluare deschiderea nu se rescrie: fiecare partidă și fiecare
lot al sursei se compară cu cubul, în ambele sensuri. Contractul 5 al
reconcilierii compară restul fiecărei partide inițiale cu soldul poziției în
sursă la luna următoare, corectat cu mișcarea neaplicată (refuzuri și
plafonări semnate după sensul real, cumulate, rederivate din cub la reluare);
partidele neatinse de trecerea 2 și cele ale partenerului generic se declară
cu sumă. O eroare tehnică a unei stingeri pică luna și nu explică nimic.
`--probe-stingeri` injectează două erori pe prima stingere reală. Contractul
și cifrele:
`docs/import/m1-deschidere-terti-contract.md`. (107)

## Limitele implementării

- Conectorul 1C rulează prin motor numai pe deschidere și pe trecerea 2 a
  lunii: deschiderea trece prin comanda de deschidere, iar stingerile anului
  pe partide inițiale se scriu în cub; fără deschiderea în cub, Import1C nu
  rulează. Stingerile prin facturi rămân în afara trecerii 2 (107-r1), iar
  pozițiile în valută intră în lei, plafonate la rest (107-r4). (107, 107 j)
- Reconcilierea Import1C nu e verde: contractul 1 are conturi fără explicație
  (nota-punte a reclasificării dublează ASM-ul care postează, 110-r2), iar
  contractul 5 are partide inițiale neexplicate. Sensibilitatea contractelor 1
  și 3 n-are probă proprie, iar unealta n-are un mod de reconciliere fără
  scriere. (D9-D11)
- Reproductibilitatea importului e măsurată pe o singură lună, pe chei
  naturale. (116)
- Reluarea parțială cu `--continua` poate afecta interpretarea reconcilierii;
  validarea completă cere o bază și un set de referință controlate, cu starea
  reluării cunoscută. (75-r3)

<!-- gen:decizii -->
## Decizii

Deciziile care numesc pagina în `Arii:` (16; titlurile în [index](../decizii/README.md); generat de `docskit gen`, nu se editează): 34, 45 (amendată de 75), 47 (amendată de 107), 48 (amendată de 76), 49, 50, 51, 75, 87 (amendată de 97), 89, 91 (amendată de 107, 118, 120), 107, 115, 116 (amendată de 117), 125, 130.
<!-- /gen:decizii -->
