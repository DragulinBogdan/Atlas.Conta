# M1 — Deschiderea de terți din `BalantaNivel3` și stingerile 2025 pe partide inițiale (contract)

- Data: 2026-10-01
- Stare: PIN-UIT de owner 2026-10-01 (M1-D3 brut per latură, M1-D5 partener
  generic de migrare, M1-D7 retururile 2024 pe partida inițială); implementarea
  urmează pe branch `m1-deschidere-terti`.
- Surse: 091 (f) și 091-r4 (felia de migrare); 094 și DES-B1…B4
  (`docs/nucleu/tr-d7b-deschidere-contract.md`); T-D7 din
  `docs/nucleu/tr-d7b-tipuri-ramase-contract.md` (forma inițială, amendată de
  094); 090 (d); 092; 096; restanțele T-r4, TR-r6, FZ-r10;
  `docs/import/faza-1c-design.md` §3, §8, §12.2.
- Ce deschide: 091-r4 spune „conectorul repornit pe modelul final, deschiderea
  de terți din `BalantaNivel3` (T-r4), stingerile 2024". Owner-ul (2026-10-01)
  a confirmat că cutoff-ul se bazează pe solduri și a cerut abordarea T-r4 și
  TR-r6 acum. Felia scoate conectorul din îngheț DOAR pe deschidere și pe
  trecerea 2 a lunii (stingerile); handlerele și motorul nu se ating.

## 1. Recensământ pe sursă (Flax, `BalantaNivel3`, `Period = 2025-01-01`)

Toate cifrele sunt citite pe 2026-10-01 din `EServicesFlx`, cu `SoldIni <> 0`
și `Cont not like '3%'`. Σ per cont = `Balanta` la ban pe toate cele 15
conturi (verificat).

| Cont 1C | Poziții | Parteneri | Documente | Fără partener | Sold (net) | Σ pe latura inversă | Parteneri cu ambele semne |
|---|---:|---:|---:|---:|---:|---:|---:|
| 401.1 | 1.235 | 78 | 1.215 | 0 | −5.106.443,33 | +798.051,17 | 31 |
| 401.2 | 105 | 12 | 104 | 1 | −427.594,48 | +8.620,70 | 6 |
| 404.1 | 2 | 2 | 2 | 0 | −23.667,64 | 0 | 0 |
| 408 | 3 | 2 | 3 | 0 | −65.180,24 | 0 | 0 |
| 409.1 | 7 | 5 | 5 | 0 | +55.923,17 | −12.451,99 | 2 |
| 411.1 | 2.490 | 1.061 | 2.442 | 7 | +4.281.860,21 | −710.219,18 | 463 |
| 411.2 | 2 | 2 | 2 | 0 | +20.275,78 | −597,84 | 0 |
| 411.8 | 5 | 2 | 3 | 1 | +9.644,59 | −127,73 | 1 |
| 418 | 36 | 15 | 36 | 0 | +39.137,36 | −247,27 | 5 |
| 419.1 | 118 | 102 | 102 | 1 | −227.565,80 | +59.071,10 | 15 |
| 461 | 8 | 6 | 3 | 1 | +85.527,48 | 0 | 0 |
| 473.1 | 86 | 55 | 7 | 26 | +4.365,97 | −62.603,05 | 6 |
| 442.7 / 442.8 / 511.2 | 4 / 27 / 1 | — | — | toate | 0 / 2.920,20 / 442,00 | — | — |

Subconto: `Valoare1` = partener, `Valoare2` = contract, `Valoare3` = documentul
de decontare. Tipurile documentului de decontare (`Valoare3_Type`):
Vânzare 2.343, Aprovizionare 1.238, ReturDeLaClient 221 (Σ −146.911,71 pe
411.1), referință goală `0x00000000` 154, ReturLaFurnizor 84 (Σ +530.975,30
pe 401.1), AvizDeIeșire 36 (418), Depozite 27 (442.8), IntroducereaSoldurilor
14, Plată 4, AvizDeIntrare 3, Încasare 1. 33 de poziții `(cont, partener,
document)` apar sub mai multe contracte; 8 documente apar pe mai multe conturi
și 8 la mai mulți parteneri (identitatea Atlas cont/partener/referință le ține
separate).

**Cât din deschidere se stinge în 2025** (aceeași cheie, `Rulaj` cumulat
2025 și `SoldIni` la 2026-01-01):

| Cont | Poziții | Stinse integral | Parțial | Neatinse | Semn inversat | Sold la 01.01.2026 |
|---|---:|---:|---:|---:|---:|---:|
| 401.1 | 1.235 | 1.214 | 3 | 18 | 0 | −78.737,56 |
| 401.2 | 105 | 101 | 0 | 4 | 0 | +746,47 |
| 411.1 | 2.490 | 1.753 | 18 | 721 | 3 | +579.997,80 |
| 419.1 | 118 | 113 | 2 | 3 | 1 | +43,11 |
| 404.1 / 408 / 411.2 / 511.2 | toate | toate | 0 | 0 | 0 | 0 |
| 461 | 8 | 5 | 0 | 3 | 0 | +192,00 |

Stoc la aceeași dată: `Balanta` 3xx = 9.516.019,83 pe 6 conturi; poziții
orfane 4 (408,37); grupe produs × depozit total-negative 124 (−5.260,56).

Azi conectorul: scrie deschiderea de terți ca un rând bloc per cont contra 891
(`Deschidere.Contabile`, fără dimensiuni) și sare în trecerea 2 toate
stingerile a căror țintă e dinaintea ferestrei (pe Flax 15.679 pe achiziții,
15.149 pe vânzări, 1.628 RDC, 649 RLF, `import.log` al feliei 31). Nu cheamă
`Materializare.Deschide`; cubul de pe Flax nu are tranzacție `Deschidere`.
`Imperecheri1C.Creeaza` nu transmite data ⇒ `Imperechere.Data` = ziua rulării
(44.448 rânduri pe 2026-09-18).

## 2. D-urile

### M1-D1 — Scop și graniță

Prima felie a 091-r4, pe branch `m1-deschidere-terti`. Se ating:
`Import1C/Deschidere.cs`, `Program.cs` (faza de deschidere și verificarea ei),
`Imperecheri.cs`, `Reconciliere.cs`/`ReconciliereLuna.cs` (contractele noi),
`FlaxDb.cs` (cititorul pozițiilor de terț). Nu se ating handlerele, `Bucla`,
`Module` (cu excepția din M1-D2 dacă se dovedește necesară), motorul,
nucleul. Codul înghețat pe care felia nu-l atinge rămâne înghețat (091 f).

### M1-D2 — Deschiderea trece prin `Materializare.Deschide`, o dată per bază

Pe bază goală (prima rulare sau `--recreeaza`), după nomenclatoare și
înaintea oricărui document: o singură tranzacție explicită
(`TranzactieComanda.Incepe`), `Materializare.Deschide(os, 2025-01-01, solduri,
loturi, partide)`. Ordinea în conector: loturile se nasc întâi (upsert prin
legături, ca azi), apoi `Deschide`, abia apoi rândurile bloc din
`RegistruContabil`/`RegistruStoc` cu `DocumentId = null` (regimul dual ține
până la TR-D9): `Deschide` refuză un lot care are deja rânduri în
`RegistruStoc`, iar rândurile bloc sunt geamănul de registru al aceleiași
deschideri. Dacă ordinea nu e suficientă (de măsurat), excepția
`DocumentId == null` din gardul „lotul are deja mișcări" e singura atingere
de `Module` admisă, cu proba ei în `DESCHIDERE.md`.

La re-rulare pe bază cu deschidere: `Deschide` NU se mai cheamă (unică per
bază, refuzată după istoric); conectorul VERIFICĂ tranzacția existentă contra
sursei (M1-D9 i) și raportează. Rândurile bloc se rescriu ca azi (idempotent
pe conținut).

### M1-D3 — Soldurile de control sunt BRUTE per (cont, latură) (owner, 2026-10-01)

Cheia de control a lui `Deschide` e (Carte, Cont, Latură); un cont cu partide
de ambele semne (411.1: 463 de parteneri, −710.219,18 pe Credit) cere DOUĂ
solduri de control, D 4.992.079,39 și C 710.219,18, fiecare contra ancorei.
Rândurile bloc de registru se scriu la fel, brut per latură (două rânduri
pentru 411 în loc de unul net), ca `Invarianti.Verifica`
(`CITIRE_DESCHIDERE_INCOMPLETA`, care compară D și C per cont între rândurile
fără document și tranzacția `Deschidere`) să rămână verde fără schimbare de
`Module`. Contractul 1 al reconcilierii (sold NET per cont) nu se schimbă.
Alternativa respinsă: net pe registre și `Invarianti.Verifica` mutat pe net
(ar atinge `Module` pentru o nevoie a conectorului).

### M1-D4 — Partidele inițiale: poziție `(cont, partener, document de decontare)`

Pe conturile OMFP cu `UrmarestePartide` (privat: prefixele 401, 403, 404,
405, 408, 409, 411, 413, 418, 419, plus 461 și 542), din `BalantaNivel3` la
01.01.2025:

- cheia = (cont mapat prin `mapari-conturi.csv`, `Valoare1_Id`,
  `Valoare3_Type`, `Valoare3_Id`); contractul (`Valoare2`) se AGREGĂ (33 de
  poziții multi-contract devin una); semnul sumei agregate dă latura, valoarea
  absolută dă suma; agregatele nule se sar;
- `Referinta` = Guid determinist (hash stabil al perechii
  `Valoare3_Type`/`Valoare3_Id`, forma fixată în cod într-un singur loc);
  referința goală `0x00000000` (154 de poziții) primește Guid determinist din
  (cont, partener, „fără document"); `IntroducereaSoldurilor` (14) e
  referință ca oricare;
- partenerul prin `ImportLaCerere.AsiguraPartener` (la cerere, ca la
  documente), inclusiv pe 461 (`Fapte.Repartitori` cere `Parte.Extern`);
- id-ul partidei e `Unitate.DeschidePartidaInitiala(cont, partener,
  referinta, data).Id`, determinist; se înregistrează totuși în
  `MigrareLegatura` sub tabela `1C:PartidaDeschidere` cu cheia
  `<Valoare3_Type>/<Valoare3_Id>` (sau `<cont>/<partener>/fara-document`),
  ca trecerea 2 să rezolve țintele fără să recalculeze și ca `--deblocheaza`
  să le vadă.

### M1-D5 — Pozițiile fără partener pe conturi urmărite: partener generic de migrare (owner, 2026-10-01)

11 poziții pe 5 conturi urmărite (401.2 +778,10; 411.1 șapte poziții cu
Σ −119,00, dintre care +45.349,75 și −45.147,00 aproape se anulează;
411.8 +9.644,59; 419.1 −453,42; 461 +37.132,63). `Valoare1_Type` e
partener, dar id-ul e gol: sursa însăși a pierdut partenerul, cel mai
probabil din perioade anterioare sistemului 1C. Două poziții pe 411.1 au
document de decontare (vânzări din 2018 și 2024), restul au și referința
goală.

Regula: soldul inițial NU se abate de la sursă. Pozițiile intră ca partide
inițiale pe un **partener generic de migrare**, creat de conector (nu de
seed: e nevoia migrării, nu a profilului), cod fix `MIGRARE-NEDEFINIT`,
denumire explicită, `NuIncludeInDec394`, legat `1C:PartenerMigrare`.
Referința = documentul de decontare când există, altfel Guid determinist
din (cont, generic, „fără document"); poziția cu document își ia partenerul
din antetul documentului dacă antetul îl are (recuperare, nu ghicire;
contorizată). Nu se creează categorie de diferență: contractul 1 rămâne
exact, iar soldul stă vizibil pe un partener numit, de rezolvat după import
prin documente (NTC de reclasificare generic → real, sau stingere când
informația apare). Alternativa respinsă (scăderea din control + diferență
declarată) lăsa soldurile inițiale diferite de sursă fără garanția că
informația mai apare vreodată.

Consecință declarată, nu ascunsă: în sursă 461 (37.132,63) și 411.8
(9.644,59) se sting integral în 2025 prin rânduri care sunt la rândul lor
fără partener; în conector rândurile acelea intră azi în punte (contrapartidă
nerezolvată), nu devin documente cu partener, deci partidele de pe partenerul
generic rămân DESCHISE la 12/2025 în Atlas, cu 0 în sursă. Contractul 5
(M1-D9 ii) tratează partidele partenerului generic separat: Δ-ul lor față de
sursă se declară „stingere fără partener în sursă", cu Σ, nu FAIL. Maparea
rândurilor de terț fără partener din 2025 pe același partener generic (care
ar închide partidele) atinge handlerele și rămâne în afara M1, ca restanță
numită la închidere. Partenerul generic apare în SAF-T Customers/Suppliers
fără CUI (precedentul „CONSUMATOR FINAL", acceptat de DUK cu avertisment).

### M1-D6 — Stocul: controlul = Σ loturilor scrise, nu `Balanta`

`Deschide` cere pe conturile de stoc detaliere exactă. Controlul pe 3xx =
Σ loturilor după netarea de azi (grupele total-negative sărite, orfanele
excluse); diferența față de `Balanta` (408,37 + 5.260,56 la 01.01.2025) e
deja raportată de conector și devine Δ justificat al contractului 1 pe
conturile de stoc, categoria „deschidere fără detaliu de lot". Ancora: proba
„ancora reproduce exact sursa" devine „ancora = sursa ± diferențele
declarate (M1-D5, M1-D6)", cu cifrele în raport.

### M1-D7 — Stingerile 2025 pe partide inițiale, în trecerea 2 (retururile: owner, 2026-10-01)

În `Imperecheri1C`, când ținta nu e document importat: se caută partida
inițială pe (contul postării stingătorului pe terț, partener, referința 1C a
țintei). Găsită ⇒ `Materializare.Imperecheaza(os, stingător, partidaId, suma,
data)` în tranzacție explicită, legătură `1C:StingereDeschidere` cu cheia
`<view>/<cheieStingător>-><referință>`, FĂRĂ rând `Imperechere` (registrele
n-au partida; urma e transferul din cub, DES-B4). Negăsită ⇒ motivul de azi.

- Plafonul e restul partidei; peste rest sau cu semn inversat (3 pe 411.1,
  1 pe 419.1) ⇒ sărit, contorizat cu Σ, nu eroare.
- Stingerea parțială și cea în mai multe tranșe sunt acoperite de mecanism
  (SC-DES-03/12).
- **Retururile 2024 (RDC 221, RLF 84) se sting pe aceeași cale.** Motivul
  46f („totalul negativ") ținea de `Imperechere` pe documente; partida
  inițială n-are document și n-are total: RDC stă pe 411 Credit, iar
  rambursarea (PLT pe 411 Debit) mută exact latura opusă. Contor separat.
- Stingător NTC (compensare): partida proprie se caută pe postările
  explicite ale notei cu partener; lipsa ei ⇒ refuz contorizat, măsurat la
  prima rulare, nu tranșat în avans.

### M1-D8 — Data împerecherii (TR-r6, partea de date)

`Imperecheri1C.Creeaza` transmite `data = max(stingător.DataInregistrare,
țintă.DataInregistrare)` (regula serviciului: data nu precede niciunul dintre
documente; avansul plătit înaintea facturii se stinge la data facturii).
Pe partida inițială data = `stingător.DataInregistrare`. Efect așteptat pe
Flax: `PartideDeschise` la 12/2025 nu mai arată 24.937 facturi integral
neîncasate cu împerechere. FZ-r10 (împerecherea ca tranzacție pe date reale)
se închide pe partea de date; mecanismul rămâne cel de azi.

### M1-D9 — Verificarea (contract de reconciliere, extins în conector)

- (i) Deschidere: per (cont, latură) Σ postărilor `Deschidere` din cub =
  rândurile bloc = `Balanta` ± diferențele declarate (M1-D5/D6); numărul
  partidelor = pozițiile agregate din `BalantaNivel3`; `Invarianti.Verifica`
  rulat pe bază după deschidere.
- (ii) Lunar, contract nou 5: pentru fiecare partidă inițială, restul în cub
  la fine de lună = `SoldIni` al lunii următoare din `BalantaNivel3` pe
  aceeași cheie + Σ stingerilor sărite pe ea (contorizate); toleranță 0,005;
  o diferență neexplicată = FAIL al lunii. E oracolul ieftin și independent
  al feliei: sursa dă restul per poziție lună de lună.
- (iii) Contractele 1–4 neschimbate; raportul integral identic în afara
  categoriilor noi (diff pe conținut sortat contra baseline-ului
  `reconciliere-20260921-035646.txt`).
- (iv) `ModelCheck --reconciliere-cub` cu grupul {Deschidere} rămâne
  diagnostic (091 c), nu gate.

### M1-D10 — Regula de oprire

Rulare integrală `--recreeaza --cititori` pe Flax cu exit 0, contractele 1–5
verzi 12/12 luni, (i) verde, `Invarianti.Verifica` fără refuz; prima rulare
reală pe `--pana-la 1` cu măsurătoare înainte de anul întreg (o singură
rulare grea o dată). ModelCheck neatins dacă `Module` nu se schimbă; verde
pe ambele profiluri altfel. Review advers la închidere. La închidere: decizie
nouă `docs/decizii/107-*.md` (regula durabilă a deschiderii de terți în
migrare), `stare-curenta` actualizată, T-r4 închisă, TR-r6 închisă pe date,
FZ-r10 închisă pe date; 091-r4 rămâne deschisă pentru restul (reconcilierea
ca raport de diferențe pe modelul rotund).

## 3. Ce NU intră

- Conturile neurmărite pe partide cu trei subconto (473, 442.x, 511.2):
  rămân sold nedetaliat, ca azi.
- Valuta (0 pe Flax), stingerile între documente importate (neschimbate),
  handlerele, cititorii TR-D8, închiderea de perioadă.
- Orice corecție de motor descoperită pe date: devine scenariu nou plus
  decizie (091-r5), nu se repară în conector.
