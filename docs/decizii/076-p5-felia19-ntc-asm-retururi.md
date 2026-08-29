# 76. Pasul 5, felia 19 — NTC + ASM + retururi (RLF/RDC) prin API și client

- **Data**: 2026-08-29
- **Stare**: activă (amendează 31d/48b prin (f); închide 53i pe ASM, 75-r1, 46f parțial)
- **Docs**: `docs/api/p5-felia-ntc-asm-retururi-contract.md` (contractul,
  F19-D1…F19-D16 + §Anexa + §Închidere), `docs/api/lista-react.md`

## Context

Ultimele patru tipuri de document fără felie de scriere. Motorul le avea deja
probate e2e în ModelCheck (NTC pe ambele profiluri, ASM/RLF/RDC pe privat), cu
refuzurile lor — felia pune ușa de API și ecranul peste scene existente.

Premisa de intrare, scrisă în contract: „felia NU redeschide semantica de motor".
S-a dovedit falsă de două ori, în ambele cazuri fiindcă o cerință de UI a atins
un mecanism vechi: o dată intenționat (ASM n-avea cum să culeagă produsul), o
dată prin descoperire (plafonul de stingere al notei era greșit). Ambele sunt în
decizie, nu ascunse în implementare.

## Tranșările

### (a) Scope și ordine

NTC + ASM + RLF + RDC, în trei track-uri independente (politici, DTO-uri și felii
de client disjuncte), cu **regulă de oprire per track**: dacă unul se blochează,
celelalte rămân livrate și restul devine restanță cu nume. ITV rămâne în afară —
nu e document CULES, e rezultatul unui serviciu (46c), deci felia lui e o
COMANDĂ cu parametri, nu un agregat PUT header + linii.

Zero seturi OData noi: tot ce cere culegerea (`Cont`, `Repartitor`,
`CodEconomic`, `Lot`, `Produs`, `TipMaterial`, `TipTva`, `Gestiune`, `Partener`,
`UnitateInterna`) era deja expus de feliile DEC/D300/SAF-T.

### (b) ASM capătă culegerea de produs — închide 53i pe ASM

`Asamblare.ValideazaOperare` cerea „Linia de produs își creează lotul la culegere
(alegeți produsul)", dar `AsamblareDetaliu` **nu avea `ProdusId`** și nu declara
`ILinieCareNasteLot`: mesaj neîndeplinibil din orice UI. Exact situația LDI de
dinainte de F6-D2, rezolvată identic, în trei piese indivizibile — `ProdusId` +
navigația, `NasteLot => Directie == Produs`, și `GestiuneLoturiCulese` →
**PREDATORUL** (validarea cere `lot.GestiuneId == PredatorId`, iar laturile POT
diferi; default-ul hook-ului e primitorul).

`Document.cs` anticipase locul: comentariul hook-ului spunea deja că „produsele
Asamblării (46d) vor face override când intră în scopul culegerii (restanța
53i); hook-ul e ancora pentru ele". Pasul n-a inventat un mecanism, a ocupat un
loc care aștepta.

### (c) Valoarea produsului ASM se derivă din consum — închide 75-r1

Din F18-D2, consumul care GOLEȘTE cheia preia tot soldul valoric rămas. Operatorul
care evaluează produsul la `preț lot × cantitate` primea refuz pe invariantul 46d
cu un rest de cenți, **fără nicio cale de a nimeri cifra** — ecranul era o capcană.

`POST api/asm/{id}/distribuie-valoarea` rescrie `PretEvaluare` pe liniile de
produs astfel încât `Σ produse == Σ |consumuri|` exact. Regula de fond:
**predicția nu e o formulă geamănă, e cifra MOTORULUI** — comanda rulează
`MotorOperare.Valideaza` pe un ObjectSpace de unică folosință, care execută
fazele de calcul ale operării și se oprește înainte de materializare, apoi
citește valorile de pe liniile de consum. Al doilea OS e obligatoriu: `Valideaza`
SCRIE pe linii (semnează cantitățile).

Detaliul care contează: erorile dry-run-ului se ignoră deliberat (inclusiv chiar
invariantul reparat), dar „calculul n-a rulat deloc" se detectează **STRUCTURAL**,
nu din textul erorilor — un consum rămas pozitiv înseamnă că `PregatesteOperare`
n-a apucat să semneze. Textul unui mesaj nu devine API.

Reziduul de ban se plimbă pe linii, în ordinea cantității crescătoare (grila cea
mai fină); cazul nereprezentabil (75-r4, cantități mari) **REFUZĂ cu 422 și cu
cifra**, în loc să lase un ASM pe care operarea îl va refuza oricum. Cazul MIXT
(o linie de produs evaluată, alta nu) se refuză explicit: o pondere 0 ar da preț
0, adică același refuz mutat cu un pas mai încolo.

### (d) Semnarea storno rămâne a OPERĂRII; culegerea e pozitivă

RLF/RDC se culeg POZITIV pe draft — cifra de pe nota de credit — iar documentul
operat le arată negative. Ecranul nu inversează nimic: afișează ce întoarce
serverul în ambele stări și **spune de ce**, o dată, în vocabularul stării
curente. Linia de COST a RDC se persistă cu `TipTvaId = null` și `ValoareTva = 0`
(oglinda `PregatesteOperare`): „inert devine adevărat, nu doar afirmat" — altfel
`RegistruTva` ar scrie rândul la backfill.

Rolul liniei RDC e o PREZENȚĂ (`LotId`), nu un enum. Un PUT care mută linia
dintr-un rol în altul se **REFUZĂ**, nu se convertește: o conversie „completă" ar
rescrie tăcut TVA-ul, natura Tipului, valoarea și cantitatea pro-formă — adică
tocmai culegerea operatorului. Calea legitimă exista deja în agregat (linia lipsă
din payload se șterge, rolul dorit se culege ca linie nouă), iar clientul o face
vizibilă: comutatorul e activ pe linia NOUĂ și devine fapt static pe cea salvată.

Capcana găsită prin gândire, nu prin testare: **o linie RDC blank arată pe sârmă
exact ca una de venit** (n-are lot). Deci blank ⇒ rol `null`, cerut structural;
altfel o linie nouă de venit reintrată în editare și-ar pierde rolul.

### (e) Idempotența semnării: pe draft bate Apply, la operare bate hook-ul

Motorul **nu de-semnează la anulare**, deci ReadDto-ul de după anulare — singurul
lucru pe care clientul îl are — e semnat. `MaterializeazaValori` îl normalizează
la forma de culegere. Alternativele erau ambele mai proaste: refuz (documentul
anulat devenea needitabil) sau ignorare (cantitate −4 lângă valoare +40 pe ecran).
Cele două căi nu se contrazic fiindcă amândouă pleacă din `Abs`.

### (f) Plafonul de stingere capătă LATURĂ și se NETEAZĂ (amendează 31d/48b)

Descoperit de felie, măsurat, reparat în două trepte — a doua fiindcă prima era
greșită.

**Defectul inițial**: `CapacitateStingere` întorcea `Dictionary<Guid, decimal>` —
contrapartidă → plafon, fără latură — iar `NotaContabila` aduna în aceeași cheie
repartitorul de pe debit cu cel de pe credit. `ValideazaCreare` consuma fără să
deosebească. Consecință măsurată: o notă `401 = 4111` de 60 lei pe X are plafon
120 și îl poate consuma INTEGRAL pe două încasări — documente de aceeași natură.
**O compensare de 60 stingea 120.** Preexistent (atins prin XAF și Import1C), dar
felia e cea care pune panoul de compensare în mâna unui operator.

**Prima reparație** a introdus sensul: plafon per (contrapartidă × SENS),
`Document.SensDeStins(os) → SensStingere?` ca al treilea hook polimorf al rolului,
lângă `CapacitateStingere` și `PoateFiStins`. Default `null` = nu declar, iar
motorul NU ghicește: un sens ⇒ identic cu înainte, două ⇒ refuz explicit. Plafonul
a rămas cheiat pe `Guid`, cu valoarea defalcată (`PlafonStingere`), tocmai ca să
nu rupă `ContainsKey(cp)` din verificări existente. Trezoreria declară o singură
dată (`SensPropriu`), iar plafonul ei e pe sensul OPUS: plata debitează 401 (stinge
datorii) dar rămâne ea însăși avans = creanță.

**Ce a găsit review-ul advers**: premisa scrisă („tavanul de 120 e corect, lipsea
doar regula laturii") era falsă pe jumătate. Formula suma `Σ |Valoare|` cu
răsturnare de sens per linie, deci o pereche `+v` / `−v` pe ACEEAȘI latură a
aceluiași repartitor producea `(Datorie v, Creanta v)` cu mișcare netă ZERO —
același defect, mutat de pe axa laturilor pe axa semnelor.

**Formula finală**: `net = Σ Valoare` (semnat) per (repartitor × latura liniei);
cheia cu net 0 **nu intră deloc**; altfel `|net|` pe sensul `net > 0 ? latură :
opusul ei`. Netarea SUBSUMEAZĂ tratarea liniei negative — nu mai e caz special, e
consecință.

**Consecințe declarate, nu ascunse**:
- `PLT → FCL` și `INC → FCT` se refuză de acum. Verdictul e corect contabil (a
  credita 401 nu stinge o factură de furnizor); vechea permisivitate era o gaură
  din aceeași familie. Pe Flax cade exact 1 imperechere / 700,00 lei din 46.056.
- Panourile CLASICE urmează și ele sensul: `DocumenteCuRest` primise parametrul,
  dar controllerul nu-l expunea, iar feliile filtrau pe TIP — axă fără legătură.
  Măsurat: **87 din 353 de contrapartide (≈25 %) au documente pe ambele sensuri**,
  deci un panou de Încasare oferea zeci de facturi cu refuz garantat. Sensul vine
  din ReadDto (`StingeriDto.SensCandidati`), nu dintr-o deducție în TS.
- Refuzul de ambiguitate a primit ieșire prin MODELARE, nu prin portiță: `NIR`
  declară `SensDeStins = Datorie` (prin 26a recepția contează pe NIR, deci lasă
  401 creditor față de furnizorul care e chiar predatorul lui). Un câmp `Sens` în
  DTO, care ar fi lăsat apelantul să aleagă jumătatea unui document fără natură
  declarată, a fost respins explicit: e exact arbitrarul pe care refuzul îl oprește.

### (g) `PanouStingeri` capătă un al doilea mod, nu un geamăn

Nota are contrapartide pe LINII, câte vrea, iar plafonul are și sens — deci „ce
pot stinge cu nota asta" are n răspunsuri. Endpoint dedicat
`GET api/ntc/{id}/candidati` rezolvă `CapacitateStingere` server-side și întoarce
un rând per (contrapartidă × sens), refolosind proiecția existentă. Panoul primește
un al doilea mod (`grupuri` absent ⇒ modul clasic, byte-identic pentru trezorerie/
FCT/FCL/DEC), fiindcă zona de legături, grila și confirmarea inline sunt aceleași —
doar zona de candidați diferă.

Două consecințe de vocabular: **„Rest"-ul global dispare pe notă** (Σ liniilor nu
e o creanță, plafonul e per jumătate), iar suma propusă e `min(disponibil-ul
grupului, restul candidatului)` — exact cele două plafoane pe care le verifică
`ValideazaCreare`, nu `Ramas`-ul notei.

**NTC nu se adaugă în `DocumenteCuRest`** (n-are semantică de „rest") și
**retururile nu devin stingători** (RDC ar cere `LiniiCreanta` polimorf în SQL —
al doilea adevăr față de `ImperechereService`). Calea de lucru pentru compensarea
unui retur rămâne nota contabilă, adică exact ce livrează felia: 46f rămâne
restanță, dar cu motivul scris și cu o cale practică deschisă.

### (h) Bara ASM: cifra nu dictează, verdictul dictează

După o distribuire reușită, `Diferenta` din ReadDto rămâne ≠ 0 — fiindcă
`MaterializeazaValori` rescrie consumurile la `preț × cantitate`, nu la cifra
golirii. Constatarea care a decis forma fixului: **`Diferenta` minte în AMBELE
sensuri** — verde `0,00` pe un document pe care motorul îl refuză (capcana), roșu
`−0,01` pe unul corect. Orice culoare legată de ea reproduce una dintre minciuni.

Deci cifrele rămân dar devin NEUTRE, iar culoarea trece pe verdictul dry-run-ului
(43b: autoritar = motorul), cerut automat pe Draft sub prefixul de cache existent,
cu explicația PERMANENTĂ, nu tranzitorie. Zero cifre de domeniu în TS, zero câmpuri
noi cerute serverului.

## Review advers

Patru constatări de fond, toate măsurate pe date reale (46.056 imperecheri Flax,
probe HTTP, check-uri noi în ModelCheck): F1 plafonul nenetat, F2 ne-regresia
trezoreriei contrazisă, F3 panourile clasice pe sensul greșit, F4 refuzul de
ambiguitate nerezolvabil. Toate reparate. Trei minore (bara ASM roșie, gardul
Tip↔Produs pe ASM, `Candidati` pe Draft) reparate; cinci observații → restanțe.

**Lecția de metodă, care a costat**: gardul de ne-regresie al primei versiuni
(„toate verificările existente rămân verzi") a fost respectat LITERAL — 1369
inserții, 0 ștergeri în `Program.cs` — și a fost totuși insuficient. Verde însemna
„nimic din ce e ACOPERIT nu s-a rupt", nu „comportamentul e identic": ModelCheck
n-avea niciun check pe perechile `PLT↔FCL` / `INC↔FCT`, care își schimbaseră
comportamentul. **Un invariant de ne-regresie se măsoară pe SPAȚIUL de cazuri, nu
pe suita existentă.**

A doua lecție, din același loc: contractul a cerut ca fiecare cusătură să fie
MĂSURATĂ, nu afirmată (riscul 2: „poate panoul să propună o stingere pe care
serviciul o refuză?"). Răspunsul pe axa întrebată a fost „nu, prin construcție" —
dar măsurătoarea a dat peste faptul că plafonul ÎNSUȘI era prea permisiv. Un check
care doar confirmă ce speri nu vede nimic.

## Proba

- ModelCheck verde pe AMBELE profiluri la fiecare pas: **786 → 858+ bugetar**,
  **623 → 870 privat**, 0 FAIL. `pnpm build` verde, `verifica:drift` exit 0.
- **Gate Import1C pe Flax** (`--recreeaza`, binar din codul FINAL, 2h10, exit 0):
  CONTRACT ÎNDEPLINIT, 0 contracte picate (4 × 12 luni), 932 avertismente.
  Comparat cu proba FINALĂ a lui F18: documente `187.372 / 190 / 17.814 / 0 /
  3.597` **identice cifră cu cifră**, punți, închidere, CMP și perechi în sursă
  (52.039) identice, **inclusiv cele 932 de avertismente** — gardul cel mai
  sensibil. Singura diferență e pe axa schimbată: imperecheri 46.056 → 44.448,
  atribuite integral (899 chei cu net EXACT 0, 10.499.837,16 lei plafon fantomă;
  `PLT→FCL` 0 rămase; depășiri de plafon 0, de la 135 / 59.853,65). Conservarea
  închide pe ambele rulări: `46.056 + 5.983 = 44.448 + 7.591 = 52.039`.
- **Smoke browser**, toate cele 6 fluxuri verzi, **zero erori de consolă**: NTC
  operat cu postarea explicită (`6588=462 −50`, `401=4111 +600`, fără nicio regulă
  de contare); panoul de compensare cu două jumătăți independente și `Confirmă`
  dezactivat pe jumătatea epuizată; ASM pe scena cu reziduu (bara arăta `8334.56`,
  motorul cita `8334.55`, comanda a prezis `8334.55`, preț `4.167,275`, lotul
  consumat golit exact la `0.000 / 0.00`, 0 rânduri contabile); RLF cu lotul rămas
  la `0.000 / −0,01` și 401 primind doar cifra hârtiei; RDC cu `Total −2420,00` și
  `TotalCost −1514,08` separat, linia de cost cu `TipTvaId = NULL` în bază; iar pe
  fixul F3, Încasarea oferă 29 de candidați toți FCL (cele 29 de INC absente),
  Plata 841 toți FCT (cele 38 de PLT absente), cu „Rest" încă în antet pe modul
  clasic.

**Incident de metodă, consemnat**: primul baseline al gate-ului a fost o rulare
din 25/26.08 — dinaintea fix-urilor F18 — fiindcă proba finală a feliei
precedente stă în `nou/tools/Import1C/run-f18/`, director **gitignored**, invizibil
la o căutare sub `docs/`. Producea trei „delte" (55 documente, 312 punți) care au
dispărut integral la compararea corectă. E a doua oară în două felii când
artefactul probei supreme e greu de regăsit.

## Ce rămâne deschis (restanțele 76-r1…r6)

- **76-r1** — netarea e per (repartitor × LATURA liniei), nu per CONT. Măsurat pe
  Flax: 55 de chei / 36 de note amestecă pe aceeași latură conturi diferite
  (`4111+473`, `401+404`, `421+423`, `4111+419`, `461+473`, `401+473`) —
  394.551,89 brut → 25.383,45 net, 52 din 55 anulate oricum, toate din clasa 4.
  Expunerea reală: 3 chei. Rafinarea la cheia cu cont se face cu cifră (59).
- **76-r2** — `AsignatFataDe`: `caStins` se scade din AMBELE sensuri. Inatacabil
  azi (singurul tip cu două jumătăți e NTC, ale cărui laturi sunt unități interne,
  deci nu poate fi stins). **Devine real în clipa în care un tip cu partener pe
  latură capătă capacitate bidirecțională** — condiția e scrisă ca să fie
  recunoscută.
- **76-r3** — perf: `AsignatFataDe` materializează entități polimorfe (TPT), iar
  `Candidati` îl cheamă de `2 × nr. contrapartide` ori. Nemăsurat (59).
- **76-r4** — gate-ul comenzilor e pe `Document`, nu pe tipul feliei
  (`ComandaAutorizata<Document>`): rutele nu sunt tipizate, `422` vs `404` diferă
  pe aceeași cauză. Preexistent, identic în toate feliile.
- **76-r5** — `Candidati` pe ușa SECURED sub-raportează tăcut; și `User` pe ușa de
  scriere e refuzat de primul FK invizibil („nu există în nomenclatorul de
  repartitori"), nu de o verificare de permisiune — închis în fapt, dar mesajul
  spune „nu există" unde adevărul e „n-ai voie să vezi" (familia 72-r10, acum pe
  calea de scriere).
- **76-r6** — patru itemi de client în `lista-react.md`: căutarea sensibilă la
  diacritice în TOATE lookup-urile remote (decizie de bază de date — colație
  `unaccent`/ICU sau coloană shadow), `Lookup` care refetchează eticheta per
  instanță și per re-render, limita convenției 61b pe valorile venite din
  PRECOMPLETARE, `window.confirm` moștenit pe ștergerea draftului.
- Neatinse deliberat: ITV, generarea liniilor de cost RDC din pin-uri, rețetarul
  (BPR), retururile ca stingători, filtrarea laturilor interne pe `Calitati`.
