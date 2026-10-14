# Import și migrare

**Actualizat: 2026-10-15.** [Index](README.md)

Importul din 1C și reconcilierea cu sistemul vechi.

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
mai mută o partidă inițială pe care stingătorul a numit-o la operare. Unde
sursa nu numește, conectorul dă cheia de ordonare: partida inițială primește
data documentului 1C ca dată de naștere. Pe ianuarie 2025: 546 de laturi cu
partida numită, niciuna absentă la data notei; 677 de date distincte pe cele
3.967 de partide inițiale. (115)

Partidele inițiale intră în deschidere ordonate pe cheia sursei (simbol,
partener, document 1C), iar la aceeași dată de naștere FIFO le consumă în
această ordine. Două importuri ale aceluiași binar pe ianuarie 2025 dau
același raport de reconciliere, aceleași legături și aceleași partide pe chei
naturale (18.770 de tranzacții, 81.774 de postări, 10 verificări picate în
ambele). (116; 2026-10-09)

Nomenclatoarele sunt create la nevoie. Identitatea materialului importat
ține cont de catalog și cont; lotul, de document × produs × cont. Mapările
de cont sunt explicite și se verifică înainte de import. (47d, 48c, 50a)

NTC este punte numai pentru cazurile permise explicit, nu fallback universal
pentru documente nerecunoscute. Transformările și transferurile sunt
clasificate în ASM, BTR sau NTC după faptul economic. FCL importată postează
venitul; DSC folosește loturile identificate de sursă. (49a, 49d, 75b)

Fără connection string dat, Import1C scrie în `Atlas.Conta.Import1C.Flax`
pe instanța nativă (5446). Pe orice conexiune care nu spune altfel pornește
pregătirea automată a comenzilor (`Max Auto Prepare=300`,
`Auto Prepare Min Usages=2`): pe ianuarie, 11:29 → 9:55 pe același binar.

`--pana-la-ziua Z` taie ultima lună a ferestrei (`--pana-la`) la ziua Z,
pentru rulări scurte de măsurare: sursa se citește numai până la acea zi,
stingerile lunii se aplică, iar închiderea de TVA și contractul lunar nu
rulează, fiindcă se compară cu sfârșitul de lună al sursei. Raportul spune că
luna e tăiată, iar verdictul e „contract lunar nerulat”, nu „îndeplinit”. Nu
se combină cu `--inchide-lunile`. Ianuarie până la ziua 5: 822 de documente,
72 s cu tot cu deschidere.

`--inchide-lunile` închide fiecare lună imediat după importul ei, prin
`PerioadaService.Inchide` cu toate constatările curente acceptate (politica de
închidere a bazei de import coboară `ItvLipsa` la avertisment — politică, nu
ocolire). E proba supremă a soldurilor materializate: raportul de reconciliere
trebuie să rămână IDENTIC cu baseline-ul rulării fără închideri, adică importul
citește peste snapshot-uri, nu peste registrul integral, și scrie aceleași
cifre. Verificat 2026-09-17 pe anul 2025: 12/12 luni închise, 0 constatări per
lună, raport identic cu `reconciliere-20260914-164035.txt`, `Reconstruieste`
0 diferențe pe contabil, stoc și partide. (F27-D1, F27-D3)

Verificarea istorică a trecerii pe TPH a cerut ca importul integral cu
`--recreeaza --cititori --inchide-lunile` să dea un raport IDENTIC pe
conținut sortat cu baseline-ul curent
(`nou/tools/Import1C/reconciliere-20260917-121343.txt`), iar
`Reconstruieste` 0 diferențe. După ea se rulează pe baza de import SQL-ul
din `--dump-integritate-tph`: zero rânduri pe fiecare interogare. Verificat
pe TPH la 2026-09-18: raportul `reconciliere-20260918-154628.txt` e identic pe
conținut sortat cu baseline-ul; 12/12 luni închise fără constatări;
`Reconstruieste` a dat 0 diferențe; integritatea TPH a dat 0 încălcări în 103
interogări. 15 dintre ele sunt vacue pe import (tipuri și legături pe care
importul nu le produce), iar pe acelea le acoperă ModelCheck. (89g, 89h)

**Din 2026-09-22 (091) proba supremă nu mai e importul, ci catalogul de
scenarii** (`docs/nucleu/scenarii/README.md`): așteptări scrise de mână,
ciclul complet per tip, lanțurile transversale, pe ambele profiluri; clona
Flax rămâne sursă de întrebări (recensământ, 091-r2), Import1C e felia de
migrare (091-r4), după „rotund" (091 (g)). Paragraful de mai jos e
istoricul probei până la felia 32, pasul 2b.

Pe cubul persistat proba supremă avea aceeași formă: importul integral cu
tipurile migrate marcate `PosteazaInCub` trebuie să dea exit 0, ZERO refuzuri
ale declarației, raport identic pe conținut sortat cu baseline-ul, 12/12 luni
închise, `Reconstruieste` 0 diferențe, `--dump-integritate-tph` 0 încălcări și
`--reconciliere-cub` 0 rânduri Δ pe toate literele; `refuzuri.ps1` se reface pe
clona privată a noului import. Măsurat la 2026-09-21: Import1C integral pe Flax (`--recreeaza --cititori --inchide-lunile`, 2026-09-21): exit 0, 1 h 57 min (3 h 21 min la felia 28), raportul `nou/tools/Import1C/reconciliere-20260921-035646.txt` IDENTIC pe conținut sortat cu baseline-ul feliei 28, ZERO refuzuri ale declarației, 12/12 luni închise cu 0 constatări, `--reconciliere-cub` 0 rânduri Δ pe (a)–(g) — (f) vacuă: cele 9 conturi cu rol de terț sunt atinse și de tipuri nemigrate —, integritatea TPH 0 încălcări în 107 interogări, cubul cu 70.373 tranzacții / 252.092 postări / 16.924 transferuri (PLT → FCT; INC → FCL fără transfer, FCL fiind nemigrat), `refuzuri.ps1` 294/294 PASS pe `Atlas.Conta.BackOffice.Privat` refăcută din import cu perioadele redeschise. (S-D10)

## Reconciliere și migrare legacy

Reconcilierea recitește PostgreSQL după operare. Compară pe luni conturile,
TVA-ul, creanțele/datoriile și stocul pe produs × gestiune. Toleranța numerică
este 0,005; o diferență neexplicată este eșec, nu motiv pentru ajustarea
ascunsă a valorii postate. Explicațiile trebuie sprijinite de documentele
sursei. (45e, 47e, 51d)

Rulajele pe lot nu sunt țintă când identitatea lotului nu este comparabilă
structural. Evaluarea exactă, excepția returului fiscal și efectele
retroactivității se verifică separat. (45e, 47a, 75c)

Import1C citește cubul prin `Cub/Citiri` și nu mai referă registrele
(TR-D9a pasul 5, 2026-10-06). Contractul 1 citește `Contabil.Postari`, intrarea
balanței. Contractul 2 reface perechea debit / credit a unei linii ITV din
cele două postări ale ei; o linie de altă formă e eșec, nu se sare. Contractul
3 citește `Loturi`; la deschidere, celulele sursei cu valoare fără cantitate,
care nu intră în cub, sunt o categorie de justificare numită, pe exact
cheile măsurate. Predicția de valoare a alocării cheamă `Nucleu.Evaluare.Iesire`
pe soldul din cub, pe orice ieșire evaluată din sold. `--sabotaj`, oracolul
golirii din raport și scrierea deschiderii în registre au ieșit. `Migrare` și
`BackfillTva` sunt șterse. (D9-D11; `docs/import/faza-1c-design.md` §16)

Formula amortizării se reconciliază cu cifrele postate în 1C prin blocul
`RECONCILIERE-MF` din ModelCheck, condiționat de fixture-ul gitignored
`1C/mf/` (parametrii datați și rândurile lunare per activ): recalculul cu
`AmortizareService.CotaLunara` se compară lună cu lună, potrivirile și
diferențele se raportează cu activ, lună, așteptat și postat; blocul nu pică
pe diferențe, doar pe fixture malformat. Import1C nu generează documente de
imobilizări; după orice atingere a motorului de operare, raportul integral
trebuie să rămână identic cu baseline-ul. (87c, 87k)

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

<!-- gen:decizii -->
## Decizii

Deciziile care numesc pagina în `Arii:` (14; titlurile în [index](../decizii/README.md); generat de `docskit gen`, nu se editează): 34, 45 (amendată de 75), 47 (amendată de 107), 48 (amendată de 76), 49, 50, 51, 75, 87 (amendată de 97), 89, 91 (amendată de 107, 118, 120), 107, 115, 116 (amendată de 117).
<!-- /gen:decizii -->
