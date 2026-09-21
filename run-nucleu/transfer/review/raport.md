# Review advers: `docs/nucleu/nucleu-transfer.md` (pasul 3 al designului nucleului)

Agent read-only, 2026-09-19. Documentul review-uit NU a fost modificat. Cifrele
proprii sunt rulate pe `Atlas.Conta.Nucleu.Fizica.x1` (docker
`contapal-postgres-1`), SQL-ul în `run-nucleu/transfer/review/*.sql`; fără DDL
pe `public`/`f2`/`cub`/`tr`, fără scriere, Flax neatins. Trimiterile la cod sunt
pe `p5-f28-tph`.

## Rezumat

**6 MAJOR, 9 MEDIU, 5 MINOR.**

Cele șase MAJOR nu resping direcția; resping trei probe care susțin decizii,
o gaură de model și două locuri unde ordinea ascunde muncă:

1. testul care alege partida (`B se închide pe soldul real`) e un artefact de
   `JOIN`, nu o proprietate a cheii — `A` cu același `JOIN` dă aceeași cifră;
2. „soldul real al conturilor de terț” din test nu e soldul acelor conturi:
   65,5 % din el stă pe rânduri de deschidere fără document și fără partener,
   pe care cheia TR-D1 nu le poate exprima;
3. TR-D4 nu scoate doar 8,63 % din cub — schimbă soldul ȘI rulajele conturilor
   de stoc (371: rulaj debitor +148 %), iar diferența nu e în lista declarată a
   gate-ului TR-D7;
4. tranzacția de transfer a împerecherii intră în TOATE rapoartele contabile
   (fișa 4111 +38 % rânduri, 44.448 de înregistrări de jurnal noi, fiecare cu o
   sumă negativă) — azi împerecherea n-are nicio urmă contabilă;
5. TR-D7 nu e o felie: conține forma per-tip care înlocuiește cele 30 de
   override-uri de hook-uri (formă pe care documentul o amână la „§4 pasul 2”,
   iar §4 n-o definește) și un dual-write al cărui mecanism nu e scris;
6. nota contabilă — vehiculul desemnat al transferului între partide și al
   compensării — nu poate purta coordonata `Partener`, deci nu poate forma
   cheia TR-D1: 7.812 picioare de terț de notă (peste 26 M) n-au partener azi,
   iar structura le interzice să aibă.

Ce am verificat și **ține**: perechea 1:1 stoc ↔ contabil (98.385, zero
diferențe de valoare, zero cazuri în care contul produsului lipsește din notă);
FCT și NIR-ul ei sunt în aceeași ZI pe toate cele 17.814 perechi, deci
diferența declarată „în aceeași lună” e exactă; nominalizarea pe cont nu cade
niciodată pe Flax în afara cazului de taxare inversă pe care TR-D3 îl
desființează (0 împerecheri în care stinsul postează exclusiv pe conturi pe
care stingătorul nu le atinge).

---

## MAJOR

### M1. Testul care alege partida măsoară tipul de `JOIN`, nu definiția partidei

**Unde**: §1 punctul 1 („E singura definiție care se închide pe soldul real al
conturilor de terț (5.101.799,27; `A` și `C` pierd 25,6 M)”) și tabelul din §2
TR-D1, coloana „se închide pe soldul de terț”.

**Scenariul**: Σ resturilor peste ORICE definiție care partiționează postările
de terț este, prin construcție, Σ postărilor de terț — plus efectul net al
împerecherilor. Împerecherea mută `−S` / `+S`, deci efectul net e zero ori de
câte ori ambele picioare aterizează. Singura variabilă e ce se întâmplă cu
piciorul orfan (cele 1.179 de împerecheri al căror document stins n-are nicio
postare de terț). În `02-sql/08-b6-definitii.sql`, `PartidaA` le pierde fiindcă
folosește `LEFT JOIN ImpEfectDoc` (`:57`), iar `PartidaB` le păstrează fiindcă
folosește `FULL OUTER JOIN` (`:80-84`) și inventează rândul lipsă.

**Proba** (`review/02-tautologie-B.sql`, §R2.2): am recalculat `A` cu același
`FULL OUTER JOIN`, inventând partida pe DOCUMENT în loc de pe (document, cont):

| variantă | Σ rest | partide ≠ 0 |
|---|---|---|
| A cu `LEFT JOIN` (ca în raport) | 30.687.174,39 | 63.336 |
| **A cu `FULL OUTER JOIN`** | **5.101.799,27** | 64.421 |
| din care partide A inventate | −25.585.375,12 | 1.085 |
| B (din raport) | 5.101.799,27 | 66.658 |
| din care partide B inventate | −25.585.375,12 | 1.085 |

`A` închide pe aceeași cifră, cu aceleași 1.085 de partide inventate și aceeași
sumă. Aritmetica o confirmă independent: 5.101.799,27 + 25.585.375,12 =
30.687.174,39 — diferența dintre coloane e EXACT masa de împerechere
neaterizată, nimic altceva.

**Ce ar trebui schimbat**: scoate „se închide pe soldul real” din tabel ca
discriminant, sau redenumește coloana „piciorul orfan de împerechere
aterizează”. Argumentul real al lui `B` e cel din propoziția următoare a §2 și
el rezistă: pe cele 2.036 de FCL cu 419 și pe cele 25 de FCT cu 401 + 408/4091,
un singur număr per document nu e sumă pe niciun cont. Declară și că închiderea
lui `B` se cumpără azi cu 1.085 de partide inventate pe conturi pe care
documentul n-a postat niciodată (−25.585.375,12) — și că TR-D3 le desființează,
moment în care `A` și `B` închid la fel și testul nu mai separă nimic.

### M2. „Soldul real al conturilor de terț” exclude deschiderea, care e 65,5 % din el — și pe care cheia TR-D1 n-o poate exprima

**Unde**: §1 punctul 1 și §2 TR-D1 („cheia ei e `(Cauza.document, Cont,
Partener)`”), plus TR-r10.

**Scenariul**: o bază nouă se construiește prin seed + Import1C (89h). Import1C
scrie deschiderea ca rânduri de registru cu `DocumentId = null`, un rând bloc
per cont sintetic, contra ancorei 891 (`tools/Import1C/Deschidere.cs:104-115`).
Rândurile n-au nici document, nici repartitor. Sub TR-D1 nu există partidă
pentru ele; sub TR-r10 rămân o tranzacție fără `DocumentId`. Deci pentru fiecare
client vechi și fiecare furnizor vechi, soldul reportat nu e nominalizabil, nu e
stingibil și nu apare în „documente cu rest”.

**Proba** (`review/02-tautologie-B.sql`, §R2.1 și `review/01-tert-sold.sql`), la
31.12.2025, în aceeași convenție naturală ca `tr."TertLa1231"`:

| cont | sold total | cu document | pe deschidere |
|---|---|---|---|
| 401 | 10.190.621,40 | 4.656.583,59 | **5.534.037,81** |
| 4111 | 4.951.749,02 | 649.613,03 | **4.302.135,99** |
| 419 | −368.583,51 | −141.017,71 | −227.565,80 |
| 408 | 310,00 | −64.870,24 | 65.180,24 |
| **total terț** | **14.792.113,93** | **5.101.799,27** | **9.690.314,66** |

`tr."TertLa1231"` filtrează `DocumentId IS NOT NULL`
(`02-sql/08-b6-definitii.sql:35`), deci cifra 5.101.799,27 e Σ postărilor de
terț CU document, nu soldul conturilor. Pe 401, 54 % din datorie e deschidere;
pe 4111, 87 % din creanță. Verificat și în structură (`review/05-891.sql` §R6.2):
cele 9 rânduri de deschidere pe conturi de terț au ZERO repartitori distincți —
nici măcar coordonata `Partener` nu există pe ele.

**Ce ar trebui schimbat**: una din două, scrisă explicit. Fie deschiderea
soldurilor de terț devine documente per partener (per factură deschisă) —
schimbare de contract pentru conectorul 1C, cu cerere către el ca TR-r6 — fie
documentul declară că soldurile de terț reportate n-au partidă, și spune ce
arată ecranul de stingere și „documente cu rest” pentru ele. Până atunci
propoziția „singura definiție care se închide pe soldul real al conturilor de
terț” trebuie corectată: se închide pe 34,5 % din el.

### M3. TR-D4 schimbă soldul și rulajele conturilor de stoc; diferența nu e în lista declarată a gate-ului

**Unde**: §2 TR-D4 („Economia: 100.332 de postări, 8,63 %”; „Transferul între
gestiuni e tranzacție `371 (gestiune B) = 371 (gestiune A)`”; „asamblarea =
`345 = 301`”) și §4 TR-D7, lista diferențelor declarate („partidele, 401 postat
de FCT în loc de NIR în aceeași lună, sink-ul de consum”).

**Scenariul**: azi `NotaTransfer` scrie 170.054 de rânduri de stoc și ZERO
rânduri contabile (`ProfilPrivat.cs:958`: la plan sintetic transferul nu mișcă
conturi), iar `Asamblare` scrie 4.694 de rânduri de stoc și zero note. Sub
TR-D4 fiecare rând de stoc E o postare contabilă pe contul produsului. Balanța
de verificare și registrul-jurnal își schimbă cifrele.

**Proba** (`review/03-stoc-contabil.sql`): rulajele și soldurile conturilor de
stoc, azi (din `RegistruContabil`) contra celor aduse de postarea unificată
(din `RegistruStoc`, contul = `Lot → Produs → TipMaterial.ContImplicit`):

| cont | rulaj D azi | rulaj D nou | Δ rulaj D | Δ sold |
|---|---|---|---|---|
| 371 | 91.134.063,77 | 225.991.765,04 | **+134.857.701,27** | +404.030,06 |
| 3028 | 144.520,48 | 350.817,99 | +206.297,51 | +134.853,80 |
| 303 | 29.252,11 | 69.362,55 | +40.110,44 | +26.236,26 |
| 3024 | 17.539,80 | 35.079,60 | +17.539,80 | +17.539,80 |
| 381 | 7.544,74 | 10.729,48 | +3.184,74 | +2.744,74 |
| **Δ sold total 3xx** | | | | **+585.404,66** |

Sursa (§R3.2): `NotaTransfer` aduce ±121.010.658,29 pe 371 (plus 69.323,46 pe
3028, 13.874,18 pe 303), `Asamblare` mută net −88.712,43 de pe 371 pe
3028/303/381, deschiderea de stoc 9.507.303,40 pe 371. Rulajul debitor al lui
371 crește cu 148 %; soldul contabil al lui 3028 cu 3.824 %. În plus intră în
registrul-jurnal și în SAF-T GLE 45.552 de tranzacții BTR și 1.228 ASM care azi
nu există contabil.

Diferența de SOLD (585.404,66) e cea care sparge gate-ul: contractul 1 al
reconcilierii Import1C cere ca fiecare cont cu Δ ≠ 0 să fie explicat EXACT de
registrul divergențelor, „oricât de mic” (`ReconciliereLuna.cs`, §H al
inventarului).

**Ce ar trebui schimbat**: adaugă în TR-D4 și în lista TR-D7 diferența, cu
cifrele: „postarea unică de stoc aduce pe conturile 3xx rulaje de
+134,86 M (371) și un sold cu +585.404,66 față de registrul contabil de azi;
sursele sunt BTR (azi necontat), ASM (azi necontat) și deschiderea”. Și spune
care parte e adevărul — dacă registrul de stoc are dreptate, atunci registrul
contabil de azi e greșit cu 585 k pe 3xx și asta e o constatare de raportat, nu
o consecință de acceptat tăcut.

### M4. Tranzacția de transfer intră în toate rapoartele contabile; azi împerecherea n-are urmă contabilă

**Unde**: §2 TR-D2 (b) („tranzacție proprie, fel `Transfer`, datată … cu două
postări pe același `(Cont, Latura)`: `−S` pe partida lui, `+S` pe partida
stinsă”) și TR-r8, care declară DOAR că rulajul brut per partidă nu mai e sumă.

**Scenariul**: cubul măsurat de `nucleu-fizica.md` are deja forma asta.
Consecințele nu sunt doar pe rulajul per partidă: postările intră în fișa
contului, în registrul-jurnal, în GLE și în orice fereastră cumulativă.

**Proba** (interogări pe `f2."Postare"` × `cub."Tranzactie"`):

| ce | cifră |
|---|---|
| tranzacții de împerechere (`Fel = 4`) | 44.448 |
| postările lor | 88.896 |
| dintre care cu `Valoare` NEGATIVĂ | 44.448 |
| postări pe 4111: azi → cu transfer | 144.241 → 199.177 (**+38 %**) |
| postări pe 401: azi → cu transfer | 73.243 → 107.201 (**+46 %**) |
| tranzacții de document în cub | 205.173 (deci **+21,7 %** înregistrări de jurnal) |

În plus, în cubul măsurat fiecare tranzacție de transfer poartă `DocumentId`-ul
STINGĂTORULUI (44.448 din 44.448, zero pe stins) — adică documentul stingător
capătă o A DOUA tranzacție, posibil într-o perioadă ULTERIOARĂ (regula de dată
88j). Documentul nu spune asta nicăieri, iar gate-ul „orice document operat are
exact o tranzacție” (design §10) ar avea nevoie de a treia excepție, după
storno.

Riscul necontrolat: `03-…` §C.4 spune că pe Flax sunt ZERO rânduri cu
`Storno = true`, deci sumele negative în GLE n-au fost niciodată exercitate.
TR-D8 pune „D406 trece DUK” în regula de oprire fără să fi probat că o
`DebitAmount` negativă trece.

**Ce ar trebui schimbat**: declară în TR-D2 volumul și rapoartele atinse (fișa,
jurnalul, GLE), fixează explicit `Cauza` tranzacției de transfer (stingătorul,
sau documentul care o comandă — o notă), extinde gate-ul design §10 și pune
proba DUK pe o sumă negativă în TR-D7, nu în TR-D8.

### M5. TR-D7 nu e o felie: conține forma per-tip nedefinită și un dual-write fără mecanism

**Unde**: §3.1, rândul documentelor tipate („hook-urile de MOTOR … se
înlocuiesc cu declarația fluxului pe operand închis (§4 pasul 2)”) și §4 TR-D7.

**Scenariul**: §4 pasul 2 nu definește forma promisă. Ca să scrie cubul,
motorul nou trebuie să reproducă, pentru toate cele 20 de frunze deodată, cele
13 override-uri de `PregatesteOperare` și 17 de `ValideazaOperare` plus
celelalte 11 hook-uri (§B al inventarului) — logică reală, nu formalități: NIR
recalculează valoarea din prețul lotului (`DocumenteGestiune.cs:43-51`),
`ReturClient` semnează negativ și șterge `TipTvaId` (`Retururi.cs:134-181`),
`Dvi` validează facturile legate (`Dvi.cs:51-96`). În aceeași felie intră
TR-D1…D4, dual-write-ul și gate-ul Import1C. Regula de oprire e binară și
evaluabilă abia la capăt.

Dual-write-ul are o gaură de mecanism, nu de efort: motorul VECHI generează
NIR-ul conex ca document real, iar `Bucla.Opereaza` (`Bucla.cs:903-931`) cheamă
`MotorOperare.Opereaza` în buclă peste lanțul de autogenerate, maximum 5 pași.
Motorul NOU trebuie să posteze recepția pe FCT și să NU o posteze a doua oară
când îi vine NIR-ul din același lanț. Ca să facă asta trebuie să recunoască un
document generat de conex — după tip sau după `Autogenerat`/`DocumentSursa`,
adică exact `is`/`switch`-ul pe care invariantul II îl interzice motorului.

**Ce ar trebui schimbat**: (a) scoate „forma declarației fluxului per tip”
într-un pas propriu, între TR-D6 și TR-D7, cu contract și regulă de oprire
proprie (o frunză pilot, de exemplu NIR sau BCS, cu probele ei); (b) fă
dual-write-ul incremental — per tip de document sau per lună, cu gate-ul de
reconciliere rulat pe submulțimea deja migrată, nu pe tot; (c) scrie regula
care decide ce documente postează motorul nou în regimul dual, și arată de ce
nu e `is` pe frunză.

### M6. Nota contabilă nu poate purta `Partener`, deci nu poate forma cheia partidei — și e chiar vehiculul desemnat al transferului

**Unde**: §2 TR-D1 („cheia ei e `(Cauza.document, Cont, Partener)`”; „regularizarea
prin notă e transfer între ele — §9 al designului obligă nota să numească
unitatea”) și §2 TR-D2 („cele 14 cazuri reale cu partener diferit … permise
doar prin notă, cu ambele unități numite”).

**Scenariul**: regularizarea avansului. FCL deschide două partide (4111 și 419);
încasarea o stinge pe prima; nota `419 = 4111` trebuie să stingă partida de pe
419 și să numească partida de pe 4111 a facturii finale. Dar
`NotaContabilaDetaliu` poartă `RepartitorDebitId`/`RepartitorCreditId` care
TREBUIE să fie repartitori INTERNI (`NotaContabila.cs:90-92`) — nu există loc pe
linia de notă pentru un partener. Regula cubului (amendamentul 4 din pasul 1,
„partenerul = repartitorul de tip `Partener`/`Angajat` de pe oricare latură”) nu
găsește nimic.

**Proba** (`review/07-partener.sql`): din 222.826 de picioare de terț, 7.824
(3,51 %) n-au partener — dar sunt concentrate exact pe note și pe deschidere, și
poartă sume mari:

| tip | cont | picioare | fără partener | Σ fără partener |
|---|---|---|---|---|
| NotaContabila | 4111 | 5.737 | 4.068 | 12.496.630,25 |
| NotaContabila | 419 | 2.260 | 1.872 | 8.254.637,73 |
| NotaContabila | 418 | 948 | 931 | 3.171.549,25 |
| NotaContabila | 401 | 4.398 | 886 | 1.324.076,27 |
| NotaContabila | 404 / 408 / 4091 | 63 | 55 | 1.446.639,91 |
| (deschidere) | 401 / 4111 / 419 / 418 / 4118 | 7 | 7 | 10.112.521,55 |

Și, pe cazul concret al regularizării (`review/08-nota419.sql`):
**2.068 de note debitează 419 cu 8.252.374,26; 1.862 (90 %) fără partener; doar
125 se potrivesc exact pe (partener, sumă) cu o partidă de 419 a unei FCL.**
Nici nominalizarea manuală nu are de unde porni.

**Ce ar trebui schimbat**: una din două, scrisă. Fie linia de notă capătă un
câmp de UNITATE (partida nominalizată) și `Partener` se SCRIE din unitate, nu
se rezolvă din politică — atunci amendamentul 4 din pasul 1 are nevoie de
excepția „pe o postare care nominalizează, partenerul vine din unitate”. Fie
cheia partidei renunță la `Partener` și îl păstrează ca atribut al unității.
Oricare din ele trebuie decisă înainte de TR-D7, fiindcă modelarea TR-D1 intră
în pasul 2.

---

## MEDIU

### E1. TR-D3, așa cum e scris, face fluxul 408 (marfă înainte de factură) neimplementabil

§2 TR-D3 enunță necondiționat „linia de natură `Stoc` a facturii de intrare
postează recepția (`3xx = 401`)”, iar TR-r4 păstrează NIR-ul pentru recepția pe
aviz (`3xx = 408`). O FCT care urmează unui NIR pe aviz ar posta recepția a
doua oară în loc de `408 = 401`. Documentul folosește faptul că `LinieSursaId` e
NULL pe toate cele 337.596 de linii (`03-…` §B5.1) ca dovadă că NIR-ul e clonă
pură — dar exact acea legătură e ce cere fluxul 408. **Schimbare**: condiționează
regula pe „linia nu acoperă o recepție anterioară” și numește purtătorul
legăturii; altfel TR-r4 nu e o restanță, e un blocaj.

### E2. SAF-T `MovementOfGoods` după TR-D3: sursa mișcării și `MovementReference` se mută pe FCT, nedeclarat

Azi codul de mișcare și referința vin de pe NIR (`SaftProiectii.cs:1679-1687`,
`:1827-1835`). După TR-D3 vin de pe factură, deci referința de mișcare a
fiecărei achiziții din D406 se schimbă. În plus, injectivitatea măsurată a
cheii noi `(TipDocument, Cont, Semn)` — 39 de chei, 0 coliziuni (`03-…` §B4.3)
— a fost calculată pe rândurile de AZI, unde NIR-ul postează; cheia
`(FCT, 371, +1)` nu există în măsurătoare și nici în seed. TR-D3 nu menționează
deloc SAF-T. **Schimbare**: declară mutarea referinței și re-verifică
injectivitatea pe setul de chei de după TR-D3.

### E3. Notele pe conturi de stoc fără lot (TR-r2): descriere inexactă și coliziune cu contractul 1 al reconcilierii

`review/04-note-lot-fct-nir.sql` §R4.1/R4.2: 2.997 de rânduri de notă (Σ
260.652,89) + 4 de deschidere + 1 linie de FCT. Grupele: `891 = 371` 1.420
rânduri (Σ 0,00, jumătate cu valoare negativă), `607 = 371` 626, `401 = 371`
354 (155.340,80), `3028 = 371` 345 (85.083,83), `371 = 401` 180. Raportul
`03-…` §B2.11 le numește „închiderile de lună”; `review/05-891.sql` §R5.1 arată
că sunt împrăștiate pe toate cele douăsprezece luni (aprilie 505, decembrie
314, februarie 10) — e puntea de import prin ancora 891, aceeași care poartă
întreaga deschidere (`891 = 401` 5.534.037,81, `4111 = 891` 4.237.078,05).
TR-r2 zice că Import1C le raportează ca divergență, dar contractul 1 al
reconcilierii pică pe orice Δ neexplicat. **Schimbare**: spune, pe cele patru
corespondențe, care devine postare cu lot, care devine divergență declarată și
ce înlocuiește puntea 891.

### E4. Derivabilitatea lui `TipStoc` e o proprietate a datelor private, nu a modelului

`03-…` §B3 probează 0 coliziuni pe profilul privat, unde se folosesc 3 din 7
valori. Pe bugetar distincția e cheiată pe CLASA produsului
(`ProfilBugetar.cs:335-336` NIR: `G`→Gratuit, `OF`→Folosință, `MF`→Mărfuri,
`MC`→Custodie; `:389` LDI), iar clasele `G`, `MF` și `MC` n-au NICIUN
`TipMaterial` în acel seed — deci nimic nu împiedică primul tip adăugat pe clasa
`G` să arate spre un cont pe care un material de Magazie îl folosește deja
(clasele `OI`/`OF` sunt separate azi doar din noroc de nomenclator: 303.01.00
contra 303.02.00). În momentul ăla `(TipDocument, Cont, Semn)` poartă două
coduri de mișcare. §2 TR-D4 enunță „`TipStoc` dispare” ca decis, iar TR-r7
acoperă doar sink-urile. **Schimbare**: fă distincția structurală (cont distinct
sau gestiune virtuală per fost-`TipStoc`) și pune injectivitatea ca probă
ModelCheck pe seed, pe AMBELE profiluri — nu ca restanță.

### E5. §3.1 spune că Import1C se transferă „neschimbat”; inventarul pe care îl citează spune altceva

`TipStoc` e citit în ~15 locuri din Import1C (`03-…` §A1): `Catalog.cs:23`
(regula conectorului), `ReconciliereLuna.cs:737`
(`RegistreComparabile = [Magazie, Marfuri]`), `Alocare.cs:159-171`,
`Deschidere.cs:478-502`, `HandlereStoc.cs:115,369,902`, `HandlerFactura.cs:223`,
`HandlereVanzare.cs:778`, `HandlerAsamblare.cs:285`, `Sabotaj.cs:250,313`,
`Saft1C.cs:313,327`. `RegistreComparabile` e chiar filtrul care ține sink-ul de
Consum în afara contractului 3 de reconciliere; după TR-D4 el trebuie să devină
„gestiuni reale”. **Schimbare**: mută Import1C din „se transferă neschimbat” în
„se transferă cu un punct de atingere numit”, și spune-l pe cel din contractul
de reconciliere — filtrul probei supreme se schimbă odată cu modelul.

### E6. Citirea per partidă e singura cale caldă nemăsurată și ajunge pe calea de culegere

Sub TR-D1 restul devine `Σ[Unitate]` pe cub. FZ-r3 spune că indexul
`(Unitate, Data)` n-a fost probat („nu e în lot, neindexat pe speculație”) și
că nicio interogare a lotului fizicii nu atinge o partidă. Ecranul de stingere
are nevoie de restul fiecărei partide candidate la culegere, iar SAF-T Payments
cere `SourceDocumentID` per linie. Regula de oprire a lui TR-D8 („perf ≤
cifrele fizicii”) nu acoperă o interogare care nu e în lot, iar TR-D10 promite
„niciun contract de citire nu aduce o interogare per linie”. **Schimbare**:
adaugă interogarea per partidă în lotul de perf și în oprirea lui TR-D8.

### E7. Desfacerea unei nominalizări făcute LA OPERARE nu are regulă

TR-D2 desființează cele patru hook-uri, tabela `Imperecheri` și
`ImperechereService`, și re-țintește regula (c) a gardianului
(`GardianEditare.cs:520-575`) pe „tranzacția de transfer”. Nominalizarea din
cazul (a) nu e o tranzacție de transfer, deci rămâne nepăzită. Azi realocarea e
operație curentă: 22.045 de stingători acoperă o partidă, 1.257 acoperă două,
491 trei, până la 335 (`02-…` §B5), și se repară ștergând legătura. Mâine cere
ori storno-ul plății, ori un transfer pe care documentul nu-l numește. La fel,
stornoul documentului STINS lasă postarea care l-a nominalizat arătând spre o
partidă inversată — fără regulă. **Schimbare**: scrie explicit că desfacerea
unei nominalizări (a) e o tranzacție de transfer (b), ce perioadă poartă, și ce
face gardianul la stornoul unui document nominalizat.

### E8. TR-D9 introduce trei mecanisme pe care regula lui de oprire nu le poate proba

Pasul 4 aduce împerecherea ca nominalizare + transfer, AMO în două cărți și
reevaluarea cu `Atribuit` („prima instanță reală: DVI pe loturi”), iar oprirea
lui e „Import1C identic din cub singur”. Pe Flax: 0 documente `Dvi` și 0 rânduri
în `DviFacturi` (`03-…` §B5.8), `RegistruImobilizari` gol, 0 documente stornate,
0 împerecheri cu rând invers (`02-…` §C.6, §C.9). Proba supremă nu atinge
niciunul dintre cele trei. **Schimbare**: numește în oprirea lui TR-D9 probele
care le acoperă efectiv (scene ModelCheck `DVI-V*`, `IMO-V*`, `AMO-V*`,
`COR-V*`), nu doar Import1C.

### E9. XAF-ul îngheațat are patru grile legate direct de registre, care mor la TR-D9

`UI/ContaUiBaseline.cs:141-175` și `:843-845` declară ListView-uri pe
`RegistruStoc`, `RegistruContabil`, `RegistruTva` și `RegistruImobilizari` (mod
`ServerView`, decizia 85; probele `D85-*`). §3.3 taie registrele fără să le
menționeze, iar §4 spune doar „XAF rămâne înghețat”. Ori se re-țintesc pe
`Postare` (deci XAF se dezgheață controlat la pasul 4), ori dispar cu tot cu
probele D85. **Schimbare**: decide-o în pasul care taie, nu după.

---

## MINOR

### N1. Cifra 87.549 e lipită de o defalcare care sumează 87.478

§2 TR-D1: „87.549 de «partide» F0 (515,7 M) stau pe documente care nu postează
NIMIC pe un cont de terț (NotaTransfer 45.542, DescarcareGestiune 36.689…)”.
Defalcarea din `02-out/11-b6c-reconciliere.txt` §B6c.4 sumează 87.478 de partide
și exact 515.747.871,20. 87.549 e altă mărime (`02-…` §B6b: partide F0 fără
partidă de cub NENULĂ, deci plus 71 de documente care postează pe terț cu rest
zero). Folosește 87.478 cu suma, sau spune ce sunt cele 71.

### N2. `ImpliciteService` nu apare nicăieri în clasificarea §3

105 linii (`Motor/ImpliciteService.cs:16`): implicitul de TVA la culegere și
„cine e partenerul documentului”. A doua responsabilitate alimentează exact
coordonata `Partener`. Nu e nici în 3.1, nici în 3.2, nici în 3.3.

### N3. Notația sink-ului pune minusul pe latura care primește

§2 TR-D4: „consumul = `6xx (gestiune virtuală Consum, −lot) = 3xx (gestiune,
lot)`”. Cantitatea iese din gestiune și intră în Consum; scrie convenția de semn
explicit, altfel conservarea cantității pe spațiul Stoc se citește invers.

### N4. „`RegulaStoc` (19 → semn + gestiune)” e imprecis

Azi repartitorul vine din LATURA documentului, nu din regulă
(`MotorOperare.cs:469`); doar regula de sink are nevoie să numească o gestiune.
Formularea corectă: „semn + gestiune virtuală opțională”.

### N5. TR-r1 se poate închide acum, cu o cifră mai mică decât cea presupusă

TR-r1 cere re-numărarea celor 1.766 de FCL cu avans, cu partida pe 4111 la brut.
Măsurat (`review/06b-nominalizare.sql` §R7.2): din cele 2.036 de FCL cu două
partide (Σ 4111 = 10.155.740,84; Σ 419 = −8.392.226,59; Σ stins = 9.914.238,15),
**118** au o stingere care depășește partida de pe 4111. Restanța e mult mai
mică decât pare.

---

## Ce am verificat și ține (ca să nu se re-probeze)

- **Perechea stoc ↔ contabil** (`review/04-…` §R4.4): 98.385 de perechi 1:1,
  **0** cu `|Valoare|` diferită și **0** în care contul implicit al produsului
  lipsește de pe nota pereche. Premisa lui TR-D4 („postarea de stoc și postarea
  contabilă pe contul de stoc sunt UNA”) e corectă pe perechile care există.
  Economia 100.332 = 98.385 + 1.471 (contenția BCS) + 476 (RLF 1:2) și
  100.332 / 1.162.622 = 8,63 % — aritmetica se închide.
- **FCT și NIR în aceeași lună** (`review/04-…` §R4.3): toate cele 17.814
  perechi au aceeași `DataInregistrare` la ZI (0 zile diferite, 0 luni fizice
  diferite). Diferența declarată de TR-D7 („401 postat de FCT în loc de NIR în
  aceeași lună”) e exactă, nu aproximativă.
- **Nominalizarea pe cont** (`review/06b-…` §R7.1): din 44.448 de împerecheri,
  43.269 au un cont de terț comun între stins și stingător, **0** au un stins
  care postează exclusiv pe conturi neatinse de stingător, 1.179 au un stins
  fără nicio postare de terț (cazul de taxare inversă pe care TR-D3 îl
  desființează). Regula TR-D2 (a) nu cade pe datele reale. De notat însă: 1.795
  de împerecheri au un stins cu 2+ conturi de terț, deci linia stingătorului
  chiar trebuie să aleagă — automatismul FIFO „pe partidele deschise ale
  aceluiași `(Cont, Partener)`” nu poate traversa conturile.
