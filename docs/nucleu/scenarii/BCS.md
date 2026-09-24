# BCS — bon de consum

**Actualizat: 2026-09-22. Stare: primul lot verificat pe privat și bugetar.**
BCS scrie deja în regim dual. Catalogul nu certifică încă ciclul complet
cerut de 091: cititorii comuni, explicația persistată și compensările după
reevaluare depind de TR-D8/TR-D9; concurența rămâne de probat.

## Regula și convențiile numerice

Surse: decizia **027 (a)–(c)** (consum pe responsabil, contarea 6xx = 3xx),
**090 (a), (c), (g), (i)** (cub, evaluare pe unitate, cantitate și storno),
**088** (data înregistrării, perioada și corecția), **091 (a)–(c), (h)–(j)**.
Formula veche `preț înghețat × cantitate` din 027 (d) nu este oracol pentru
evaluarea nouă pe soldul unității. Acestea sunt așteptări ale politicii
proiectului, nu o certificare a aplicabilității fiscale a unui profil.

Profil privat: credit **302**, debit **602**. Profil bugetar: credit
**302.01.00**, debit **602.01.00**; factura de pregătire are cod economic.
Valorile sunt în lei, fără TVA. Unitatea de măsură este BUC, inclusiv
fracțiunile. Soldul menționat este pe **cont de stoc × MAG1 × lot × carte
Contabil**, la data inclusivă; valoarea se însumează cu semnul laturii.
Nu se însumează cantitățile lotului peste magazie și locul de consum.

Pentru `consum(q, v, L)` sunt exact două postări în spațiul `Stoc`, cartea
`Contabil`, cu documentul și linia drept cauză:

| Cont/latură | Gestiune | Unitate/produs | Cantitate | Valoare |
|---|---|---|---:|---:|
| 602 / Debit (602.01.00 la bugetar) | locul de consum intern al scenei | L / produsul lui L | +q | v |
| 302 / Credit (302.01.00 la bugetar) | MAG1 | L / produsul lui L | −q | v |

Fără partener, valută sau coordonate fiscale pe BCS; `ValoareValuta = 0`.
Stornoul păstrează conturile/laturile/unitățile, inversează **ambele** măsuri
și are data proprie. BCS nu deschide partidă: stingerea nu este aplicabilă.
La BCS fără TVA, trecerea în luna următoare schimbă data postării;
`PerioadaDeclarare` rămâne null, nu se inventează o postare fiscală.

## Fixture și comenzi

`ScenariiBcs` creează date marcate `E2E-SC-BCS`, în ianuarie/februarie 2001,
numai dacă anul este liber pe baza de test. Fiecare caz are loturi proprii.
Stocul se creează prin **FCT operată → NIR conex operat**, fără inserări
manuale în registre/cub și fără comutarea `PosteazaInCub`. FCT este la
05.01, BCS la 10.01 dacă rândul nu precizează altfel. Comenzile
`OperareApi` primesc ID-uri în ObjectSpace-uri noi, ca adaptorul folosit
de API; aceasta nu probează HTTP sau securitatea.

Așteptările de mai jos sunt constante scrise în scenariu. Nici
`CubDinRegistre`, nici `Normalizari` nu participă. Soldurile sunt sume
directe de probă peste cub; nu certifică încă cititorii de producție.
Închiderea trece prin `PerioadaService`, cu avertismentele acceptate
explicit prin helperul existent ModelCheck. Curățenia folosește `finally`
și o purjă pe marcaj, inclusiv perioadele/snapshot-urile fixture-ului.

## Catalog executabil

Proba este ID-ul din prima coloană, în `ScenariiBcs.cs`, inclusă în
`ModelCheck --scenarii BCS`. În acest lot, starea tuturor rândurilor de mai
jos este **verificat pe ambele profiluri** (rularea din secțiunea Validare).

| ID | Comenzi și așteptări în ordine | Rezultat așteptat | Proveniență |
|---|---|---|---|
| SC-BCS-01 | Recepție L: 10 × 10 = 100. Draft BCS 4; dry-run acceptat, fără persistare. Operare: consum(4,40,L), sold 6/60. | acceptat | regulă 027, 090; review |
| SC-BCS-02 | L1: 10/100, L2: 10/150; un BCS cu 4 din L1 și 2 din L2. Exact 4 postări: consum(4,40,L1), consum(2,30,L2). Solduri 6/60 și 8/120. | acceptat | regulă; recensământ: 204 documente cu mai multe linii |
| SC-BCS-02b | L: 10/100; același BCS are două linii, 6 și 4 din L. consum(6,60,L) și consum(4,40,L), sold 0/0. | acceptat | recensământ: 6 documente cu lot repetat |
| SC-BCS-02c | L: 10 × 8 = 80; consum 0,125. consum(0,125;1;L), sold 9,875/79. | acceptat | recensământ: o linie cu cantitate fracționară |
| SC-BCS-03 | După SC-BCS-01: storno la 20.01, consum(−4,−40,L) ca tranzacție Storno; sold 10/100. | acceptat | 090 (i) |
| SC-BCS-04 | L: 10/100; BCS 4 în ianuarie, sold 6/60; închide ianuarie, storno la 05.02. Storno −4/−40; sold la 31.01 rămâne 6/60, la 05.02 devine 10/100. | acceptat | 088, 090 (i) |
| SC-BCS-05 | L: 10/100; BCS 4 → sold 6/60; anulează operarea. Draft, zero tranzacții/postări/registre ale BCS, lotul inițial există, sold 10/100. | acceptat | excepția declarată de anulare, invariantul III |
| SC-BCS-06 | L: 10/100; BCS 4 în ianuarie → 6/60; închide ianuarie. Corectează la 05.02, motiv EroareMateriala: original Stornat, invers −4/−40 și draft nou legat, fără postări proprii; sold curent 10/100. Schimbă cantitatea draftului la 3 și operează: consum(3,30,L). Sold 31.01 = 6/60; 05.02 = 7/70. | acceptat | 088, 090 (i) |
| SC-BCS-07 | Recepție 3 × 0,333333, valoare rotunjită 1 leu. Ieșire 1 → 0,33, rest 2/0,67. Ieșire 2 la 11.01 → 0,67, rest 0/0. | acceptat | 090 (c), ultima ieșire ia restul |
| SC-BCS-08a/b/c | Din L:10/100, BCS cu cantitate 0 / −1 / fără lot. Ușa declarației refuză cu codul stabil; dry-run și operarea refuză cu textul validării vechi. Draft și zero efecte proprii; sold 10/100. | refuzat: `CANTITATE_NEPOZITIVA` / `CANTITATE_NEPOZITIVA` / `LOT_LIPSA` pe declarație; text vechi pe entitate până la TR-D8 | regulă; cazuri absente din recensământ |
| SC-BCS-09 | L:10/100; două linii 6 + 5 din același lot. Dry-run și operarea refuză întregul document, fără consumul primei linii; sold 10/100. | refuzat: stoc insuficient — gardianul REGISTRULUI (`StocService`), fără cod stabil; la TR-D8 devine refuz al cubului | review; lot repetat din recensământ |
| SC-BCS-10 | După SC-BCS-03, repetă storno la 21.01. Rămân exact două tranzacții și sold 10/100. | refuzat: stare neeligibilă | 090 (i) |
| SC-BCS-11 | După închiderea lui ianuarie, anularea BCS operat și operarea altui BCS în ianuarie sunt refuzate; originalul și soldul 6/60 rămân, draftul nou are zero efecte. | refuzat: perioadă închisă | invariantul III, 088 |
| SC-X-02 (pregătire) | FCT cu recepție 10/100 → NIR conex operat. Soldul cubului este 10/100, apoi BCS 4 îl duce la 6/60; recepția nu se dublează. | acceptat | 090 (h), T-D9 |

Refuzurile au două uși. Declarantul (`Declaratii/DeclarantBonConsum.cs`)
refuză cu coduri stabile din `CoduriRefuz`, probate direct prin
`Materializare.Refuzuri` (linia `COD: mesaj`). Ușa entității
(`OperareApi.Valideaza` / `Opereaza`) trece întâi prin `ValideazaOperare` al
clasei și prin gardienii registrelor (`StocService`, perioada, starea), care
refuză cu text, înaintea declarantului; acolo proba verifică familia
mesajului și absența efectelor, nu textul integral. Când validarea veche cade
(TR-D8), asertarea de pe ușa entității trece pe cod, fără să schimbe
așteptarea scenariului. SC-BCS-09 certifică azi gardianul registrului, nu al
cubului.

## Acoperirea rămasă

| ID / cerință | Așteptare / pas | Stare |
|---|---|---|
| SC-BCS-12: fișă, balanță, `Sold` | Pentru SC-BCS-06: ianuarie consum 40 și stoc 60; februarie rulaj net consum −10 și stoc +10; sold final 70. Reconstrucția trebuie să păstreze aceleași valori fără rerularea politicilor. | specificat; cititori comuni și snapshot cub la TR-D8 |
| Explicația evaluării | SC-BCS-07 explică baza 3/1, alocarea 1/0,33 și restul 2/0,67 prin date persistate pe tranzacție. | amânat la TR-D8, 091 (j) |
| SC-X-05/06/07 | Inversare + compensarea exclusiv a reevaluărilor active; cifrele din README. | amânat la TR-D9, 091 (h) |
| SC-X-01 și SC-X-08 | Refuzul stornării sursei cu consum ulterior, distinct de blocarea prin NIR conex. | specificat, de probat independent |
| SC-BCS-13: două sesiuni reale | L:10/100; două comenzi concurente de câte 6. Exact una reușește, cealaltă refuză; sold 4/40, fără dublă cheltuială. | specificat; F27-r8/S-r9 |
| SC-BCS-14: consum vs. închidere | Serializare: fie consumul de 4 intră în luna închisă și snapshot = 6/60, fie închiderea câștigă și consumul refuză, snapshot = 10/100. | specificat; F27-r8/S-r9 |
| Re-intrare / lot cu sold negativ | De completat numeric împreună cu tipul care recepționează/corectează; BCS negativ rămâne refuzat. | specificare restantă |

## Legătura cu probele existente

SC-BCS-15 (implementat și verificat pe ambele profiluri, 2026-09-23): lot 3/10, trei documente
succesive de câte 1. Cub: 3,33 / 3,34 / 3,34; sold final 0/−0,01.
Regula țintă rămâne 0/0, rezultatul dual este excepția exactă T-r13,
ASM-B7; fără toleranță generală. Rândul nu certifică invariantul țintă.

`VerificaNucleuBcs` rămâne în aceeași selecție: `NUC-BCS-*`, inclusiv
`NUC-BCS-N-R3-*` (evaluare 5 × 300/20 = 75, vechiul preț ar da 50),
`NUC-BCS-REFUZURI` (două refuzuri pe două loturi), `STR-ANULARE` și
`STR-NEMIGRAT`. Acestea probează contractul/integrarea duală și regresiile;
egalitatea normalizată nu este acreditată ca scenariu independent.

## Validare

`verifica.ps1 -Suita Scenarii -Tip BCS -Profil Ambele -Sufix .CodexBCS`:
exit 0 pe ambele profiluri, 2026-09-22 (după review: probele de cod pe ușa
declarației pentru SC-BCS-08a/b/c, SC-BCS-09 marcat ca refuz al registrului).
Build fără erori; `ScenariiBcs` 6,8 s bugetar, 7,1 s privat, plus `NUC-BCS`.
Manifest local: `run-verificari/20260922-234431-012/rezultat.json`, commit
de bază `66d33f6` cu modificările locale ale review-ului. Rularea inițială a
lui Codex (înainte de review, aceleași scenarii fără probele de cod):
`run-verificari/20260922-230637-777`, suita integrală
`run-verificari/20260922-230744-727`, ambele exit 0. Directorul `run-*` nu e
versionat.

## Recensământ Flax, numai citire

Rulat 2026-09-22 pe `Atlas.Conta.Import1C.Flax`, cu tranzacție read-only;
interogările reproductibile sunt în [BCS-recensamant.sql](BCS-recensamant.sql).
547 BCS, 1.471 linii; 343 documente cu o linie, 204 cu mai multe (maxim 97);
6 documente repetă un lot. O linie are cantitate fracționară. Sunt 5 tipuri
de material, 22 gestiuni predatoare și un loc de consum. Zero cantități
nule/negative, zero loturi lipsă, zero corecții legate și zero documente cu
data fizică diferită de data înregistrării. Combinațiile observate motivează
SC-BCS-02/02b/02c; cifrele așteptate nu sunt copiate din această bază.
