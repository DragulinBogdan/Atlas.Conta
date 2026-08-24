# Pasul 5, felia 12 — D300 (decontul de TVA) ca proiecție peste `RegistruTva` — DESIGN

Felia 11 (decizia 68) a lăsat registrul fiscal gata și un schelet: proiecția
`DecontTva`, grupată pe `(Sens × TipTva)`, cu comentariul din `App.tsx` „decontul
e scheletul D300, nu declarația". Felia de față închide lanțul **TVA structural
(36) → registru (68) → declarație**: aceleași cifre, așezate pe rândurile
formularului 300 în vigoare, cu totalurile și rezultatul lui.

Textul care bate aici e decizia 36f — „D300/D394/SAF-T ca **proiecții peste
registre**" — și 35c: SAF-T/e-Factura/D394 (și D300) sunt **checklist de
completitudine, NU modele de date**. Deci felia produce cifrele decontului și
dovada că nu pierde nimic din registru; **nu** produce fișierul (XML/PDF) și nu
persistă „declarația" ca entitate.

Formularul: **OPANAF 174/2026** (M.Of. 105/09.02.2026), în vigoare de la prima
perioadă fiscală 2026 — cote 21%/11%, 9% doar tranzitoriu (rd. 11), cotele vechi
exclusiv prin rd. 16/33. Structura completă (45 rânduri + sub-rânduri, formulele,
schema XML v12.0.0) e extrasă în `docs/api/d300-structura-2026.md`.

## De ce mapare ca DATE pe `TipTva × Sens`, și nu cod per rând

Decizia 68 a stabilit că **`TipTva` E identitatea de raportare** (SDD și SFD au
același regim și aceeași cotă, dar rânduri diferite în D300). Deci rândul de
decont e o funcție de `(TipTva, Sens)`. Dar nu încape într-o coloană pe `TipTva`
(ca `CodSafTLivrare`/`CodSafTAchizitie`), din două motive:

1. O pereche poate cădea pe **mai multe rânduri**: taxarea inversă internă (TI21,
   achiziție) e în rd. 12.1 (colectată) ȘI în rd. 26.1 (deductibilă).
2. Rândurile formularului se **schimbă cu legea** (2026 a eliminat 13 rânduri și a
   comprimat numerotarea). Un cod liber pe `TipTva` ar fi devenit un string magic
   fără validare; o FK către un nomenclator de rânduri e verificabilă.

Alternativa „switch în cod pe `TipTva.Cod`" cade la invariantul IV (politica nu
inventează comportament, dar nici structura nu îngheață politica): un client cu un
`TipTva` propriu (de ex. „TVA 21% mărfuri accizabile") trebuie să-l poată așeza pe
rând fără release. Deci: **rândurile = nomenclator (seed din lege), maparea =
politică editabilă, formulele = cod**.

## Deciziile de fixat

### D3-D1 — `RandD300`: nomenclatorul rândurilor, seed-uit din OPANAF 174/2026

Clasa `RandD300 : BaseObject` în `BusinessObjects/Nomenclatoare/`:
`Cod` (string, „9", „12.1", „26.1"), `Denumire`, `Sectiune` (enum `SectiuneD300`:
`Colectata = 1, Deductibila = 2, Regularizari = 3`), `Ordine` (int, poziția în
formular), `AreBaza` (bool), `AreTva` (bool), `ParinteId?` (FK către rândul „din
care" — 3.1→3, 5.1→5, 7.1→7, 12.1/12.2→12, 20.1→20, 22.1→22, 26.1/26.2→26,
29.1→29), `Fel` (enum `FelRandD300`: `Operatiuni = 1, Total = 2, Oglinda = 3,
Extern = 4`).

- `Operatiuni`: primește mapări (1–18 fără 19, 24–29, 33).
- `Total`: calculat în cod (19, 30, 31, 35, 36, 37, 40, 43, 44, 45).
- `Oglinda`: copiat din rândul-sursă (20…23, 26, 26.1, 26.2 ← 5…8, 12, 12.1,
  12.2); poartă `OglindaAId?` (FK către sursă).
- `Extern`: fără sursă în registru, valoare din parametri sau 0 (27, 28, 32, 34,
  38, 39, 41, 42).

Seed-ul stă în **nucleu** (`ContaSeeder`, nu în pachetul de profil): formularul e
al legii, nu al profilului. Idempotent pe `Cod`. `Unique(Cod)`. Nomenclatorul e
**read-only în OData** (e lege, nu configurare) și fără CRUD în XAF (`ForbidCRUD`
ca registrele — se schimbă prin seed la o versiune nouă a formularului).

**Versionarea formularului NU intră** (o singură versiune, cea în vigoare); e
restanță cu nume — D3-r1. Consecința asumată: o perioadă din 2025 proiectată pe
formularul 2026 pune 19% pe rd. 16/33 — corect pentru forma 2026, nu pentru
decontul care s-a depus atunci. Ecranul afișează avertismentul când
`dataStart < 2026-01-01`.

### D3-D2 — `MapareD300`: politica `(TipTva × Sens) → RandD300`, n rânduri

Clasa `MapareD300 : BaseObject` în `BusinessObjects/Politici/`: `TipTvaId` (FK),
`Sens` (`SensTva`), `RandId` (FK `RandD300`). Cheie unică pe tripletă. Validare
la seed și în `VerificaProfil`: ținta e `Fel = Operatiuni` (o mapare pe un rând
Total/Oglindă/Extern e refuzată — nu tace). Editabilă în XAF (politică = date,
decizia 4), `ReadOnly` în OData ca toate politicile (56).

Seed **privat** (profilul bugetar n-are `PoliticaTva`, deci `RegistruTva` gol ⇒
nicio mapare; `VerificaProfil` cere 0 rânduri acolo):

| TipTva | Livrare | Achiziție |
|---|---|---|
| N21 | 9 | 24 |
| N11 | 10 | 25 |
| N9 | 11 | — (nemapat, deliberat: formularul n-are rând de achiziție cu 9%; incertitudinea (a) din structură) |
| TI21 | 13 | 12.1 (oglinda 26.1 vine din cod) |
| N19 | 16 | 33 |
| TI19 | 13 | 16 (cote istorice, colectat; deductibilul lui = 33) |
| NED21 | — | 24 (intră în rd. 30, lipsește din rd. 31 — §4.1 din structură) |
| SDD | 14 | 29 |
| SFD | 15 | 29 |
| NIM | — (nemapat: în afara sferei, nu se declară) | 29 |

TI19 pe achiziție are DOUĂ mapări (16 și 33) — e exact cazul „n rânduri". TI21 are
una singură (12.1); rd. 26.1 e oglindă. Rândurile intracomunitare (1–8, 17–18,
20–23, 29.1) rămân fără `TipTva` azi: nu există tipuri pentru AIC/servicii
intracomunitare în seed — se adaugă ca DATE când un client le cere (D3-r2).

### D3-D3 — Proiecția `D300` = listă în memorie, un rând per `RandD300`, formule în cod

`TvaProiectii.D300(os, dataStart, dataEnd, ParametriD300 externi)` →
`RezultatD300 { Randuri: List<D300Rand>, Nemapate: List<D300Nemapat>, Avertismente: string[] }`.

Forma (b) din precedente (`BalantaPlan`): **un formular nu se paginează**, nu
trece prin `DataSourceLoader`. Pașii, în ordine:

1. **Agregatul de registru**: `RegistruTva` filtrat pe `Data ∈ [dataStart, dataEnd]`,
   grupat pe `(Sens, TipTvaId, Regim, Cota)` — Σ`Baza`, Σ`Tva`, `Count` (`Cota` în cheie
   ca snapshot pentru `Nemapate`, la fel ca `DecontTva`; nu schimbă nicio cifră de rând). `Storno` NU se
   filtrează (suma algebrică e adevărul, 68); un storno de N19 ajunge pe rd. 16
   prin `TipTvaId`-ul snapshot — niciun mecanism nou. `Regim` intră în cheie
   fiindcă rd. 31 îl consumă.
2. **Așezarea**: fiecare grup se adună pe TOATE rândurile mapate ale perechii
   `(TipTva, Sens)` (LEFT join către `MapareD300`); grupurile fără mapare ajung în
   `Nemapate`, cu `TipTvaCod/Denumire`, `Sens`, `Baza`, `Tva`, `Randuri`.
   `Baza` se pune doar dacă `AreBaza`, `Tva` doar dacă `AreTva` (rd. 13/14/15/29 nu
   au coloană TVA; TVA-ul unui NIM/SDD e 0 prin construcție, deci nu se pierde —
   verificat în ModelCheck, D3-V3).
3. **Părinții „din care"**: rândul-părinte = mapările lui directe + Σ copii
   (rd. 12 ≥ 12.1 + 12.2 — egal la noi, ≥ dacă apare o mapare directă pe 12).
4. **Oglinzile**: copie din sursă, ambele coloane (`Fel = Oglinda`, `OglindaAId`).
5. **Totalurile**, în cod, exact formulele din structură:
   - rd. 19 = Σ rândurilor `Colectata` de nivel 0 (fără sub-rânduri), per coloană;
   - rd. 30 = Σ 20…28 nivel 0 (29 NU intră — informativ);
   - **rd. 31 = rd. 30.Tva − Σ Tva a grupurilor cu `Regim = Capitalizat` așezate
     în secțiunea Deductibila** (nedeductibilul „nu se preia", §4.1);
   - rd. 35 = 31 + 32 + 33.Tva + 34; rd. 36 = max(35 − 19.Tva, 0); rd. 37 = max(19.Tva − 35, 0);
   - rd. 40 = 37 + 38 + 39; rd. 43 = 36 + 41 + 42; rd. 44 = max(40 − 43, 0); rd. 45 = max(43 − 40, 0).
6. **Externii** (`ParametriD300`, toți `decimal`, default 0): `SoldPlataPrecedent`
   (38), `DiferentePlata` (39), `SoldNegativPrecedent` (41), `DiferenteNegative`
   (42). Restricția încrucișată (38 și 41 nu pot fi ambele > 0) = **400**.
   27/28/32/34 rămân 0 (fără sursă în model: agricultori, restituiri, pro-rata —
   36f); apar în listă cu `Fel = Extern` ca să nu lipsească din formular.

`D300Rand` (sârmă, `sealed`, plat): `Cod`, `Denumire`, `Sectiune` (string),
`Fel` (string), `Nivel` (0/1), `Ordine`, `Baza?`, `Tva?` (null = coloana nu există
pe rând — **nu 0**, ca ecranul să nu mintă), `Randuri` (câte rânduri de registru
au contribuit; 0 pe totaluri/oglinzi), `Surse` (string: codurile `TipTva` care
au alimentat rândul, pentru transparență — „9: N21").

Rotunjirea: cifrele vin rotunjite la bani din registru; totalurile sunt sume
exacte de bani. Nicio rotunjire nouă. Enum-urile pleacă string (57a).

### D3-D4 — Nimic nu se pierde: `Nemapate` e parte din contract, nu log

Suma `Baza`/`Tva` a rândurilor de operațiuni (nivel 0 + nivel 1 fără dublă
numărare, fără oglinzi) **+ Nemapate == Σ registru pe perioadă**, pe ambele
coloane, cu excepția documentată a coloanelor absente (rd. 13/14/15/29: `Tva` e 0
prin construcție pe acele tipuri). Un gard care tace devine capcană (62f): ecranul
afișează panoul „Operațiuni neincluse în decont" cu cifrele lui, nu doar un număr.

### D3-D5 — Cusătura cu registrul contabil: rezultatul D300 == nota ITV a lunii

Pe scena ModelCheck (fără punți NTC, fără solduri anterioare, ITV generat pentru
luna scenei): **rd. 37.Tva == valoarea liniei `4427 = 4423` a ITV** și **rd. 36.Tva
== linia `4424 = 4426`**, citite prin `InchidereTvaService` (conturile ca DATE din
`PoliticaInchidereTva`, niciun simbol hardcodat — 29). Taxarea inversă se anulează
în ambele (4426 = 4427 la ITV; 12.1 vs 26.1 în D300); nedeductibilul lipsește din
ambele (nepostat contabil; exclus din rd. 31). Dacă egalitatea pică, e defect de
fond — nu se „ajustează" cusătura.

Complementul e măsurat, nu ascuns: pe baza cu volum, D300 pe o lună NU va egala
ITV-ul real (punți NTC de import, 1,85 M lei — 68d); smoke-ul raportează
diferența cu explicația, ca la felia 11.

### D3-D6 — API: un endpoint, perioada obligatorie, răspuns întreg

`GET api/proiectii/d300?dataStart=&dataEnd=&soldPlataPrecedent=&diferentePlata=&soldNegativPrecedent=&diferenteNegative=`
→ `200 D300Dto { Randuri[], Nemapate[], Avertismente[] }`, `400 EroriDto` la
perioadă lipsă/inversată sau 38∧41 > 0. `Secured(typeof(RegistruTva))`, `using
var`, materializare cât OS-ul e viu. Fără `DataSourceLoadOptions`. Sub
`ContaApiController`, fișier `D300Controller.cs`. Proiecția + DTO-urile în
`Module/Proiectii/D300Proiectii.cs` (fișier propriu; `TvaProiectii` rămâne al
jurnalelor).

### D3-D7 — Client: `/d300`, formularul în ordinea oficială, externii culeși în bară

Ecran `felii/tva/D300.tsx`: bara = perioadă (`useUrlStare`, implicit luna
curentă) + cele 4 câmpuri externe (numerice, în URL — starea globală e URL-ul,
43c); corpul = tabel (DataGrid local, fără remote ops, `key` pe parametri) cu
secțiuni ca grupuri, rânduri de nivel 1 indentate, totalurile marcate, `Baza`/`Tva`
goale (nu „0,00") unde coloana nu există; sub el panoul „Neincluse în decont"
(D3-D4) și avertismentele (perioadă < 2026, 38/41). Rută + NavLink în `App.tsx`;
etichetele enum din `metadata.json` (`--dump-metadata` obligatoriu — două enum-uri
noi). Fără export, fără XML (35c).

### D3-D8 — Politica se vede: ecranul XAF al mapării + verificarea de profil

`MapareD300` are ListView/DetailView în XAF (lookup pe `TipTva` și `RandD300`,
filtrat pe `Fel = Operatiuni` — afordanță; refuzul rămâne în validare).
`VerificaProfil` (36c) adaugă: privat ⇒ fiecare `TipTva` seed-uit are cel puțin o
mapare SAU e în lista explicită a nemapatelor deliberate (N9-Achiziție,
NED21-Livrare, NIM-Livrare); bugetar ⇒ 0 mapări. Un `TipTva` nou fără mapare
apare în `Nemapate` la prima cifră — vizibil, nu refuzat (raportare ≠ operare).

## Ce NU intră, cu motiv

- **Fișierul D300** (XML pe schema v12.0.0, validare DUKIntegrator, depunere):
  35c — proiect izolat al utilizatorului; felia garantează cifrele și maparea XML
  e documentată în `d300-structura-2026.md` §5 pentru el.
- **Declarația ca entitate** (perioadă, stare, recipisă, istoric — șablonul
  EServices din `1C/04`): nu există cerință; D300 rămâne o citire.
- **Rândurile intracomunitare** (1–8, 17–18, 20–23, 29.1), **agricultori** (27/28),
  **restituiri** (32), **pro-rata / ajustări** (34), **secțiunile A/B** (TVA la
  încasare — 36f): fără sursă în model; rândurile există în nomenclator, cifra e 0.
- **Regularizările pe cauză juridică** (art. 287 vs erori de declarare — rd. 2/4/6/
  8/16/18 vs rândul operațiunii): storno-ul nostru poartă `TipTva`-ul original și
  cade în rândul lui; distincția „eroare de declarare" nu e modelată (D3-r3).
- **Versionarea formularului per an fiscal** (D3-r1).
- **Perioada fiscală trimestrială ca noțiune** (`SetareProfil`): perioada e
  parametru liber; trimestrul = trei luni în bară.

## Riscurile pin-uite (ce va ținti review-ul advers)

1. **Dubla numărare** în rd. 19/30: un grup mapat pe părinte ȘI pe copil, sau o
   oglindă însumată. Totalurile se calculează doar din nivelul 0, oglinzile nu se
   însumează cu sursa lor în același total (sunt în secțiuni diferite — dar 12 e
   în Colectata și 26 în Deductibila, corect).
2. **Rd. 31**: scăderea nedeductibilului trebuie făcută pe `Regim` snapshot al
   grupului, nu pe `TipTva.Regim` de azi; și doar pe grupurile așezate în
   Deductibila.
3. **Semnul stornărilor**: baza și TVA-ul unui storno sunt ambele negative
   (68/46a); un rând de operațiuni poate fi net negativ — nu se trunchiază la 0
   (doar 36/37/44/45 au `max(…, 0)`).
4. **Pierdere tăcută**: o mapare către un rând fără coloana `Tva` cu TVA ≠ 0
   (ex. cineva mapează N21 pe rd. 14) — D3-V3 o prinde; ecranul o arată în
   avertismente („rd. 14 a primit TVA 210,00 pe care nu-l poate purta").
5. **Perioada**: filtrul e pe `Data` a rândului de registru (data stornării
   pentru storno, 25d) — coerent cu jurnalele; fără index (59), cifra decide.
6. **Securitate**: aceeași ușă secured ca jurnalele. **Corectat la implementare**: `User`
   nu primește 403 — ușa securizată filtrează RÂNDURILE, nu cererea (identic la
   `jurnal-tva`/`decont-tva`/`balanta-plan`): 200 cu `Randuri: []`, nomenclatorul
   invizibil, zero scurgere. 403 e al gate-ului de COMANDĂ (55b), nu al citirii.

## Verificări (ModelCheck, ambele profiluri)

- **D3-V1** seed: toate rândurile din structură există, `Ordine` unică, părinții
  și oglinzile rezolvate, formulele de total au operanzii prezenți.
- **D3-V2** (privat) scena JT reutilizată + linii pe fiecare `TipTva`: fiecare
  rând de operațiuni așezat conform tabelului D3-D2 (cifre exacte, inclusiv TI21 în
  12/12.1/26/26.1, N19 în 16, TI19 în 16 și 33, NED21 în 24 și nu în 31).
- **D3-V3** D3-D4: Σ operațiuni + Σ nemapate == Σ registru, ambele coloane; TVA ≠ 0
  pe rând fără coloană ⇒ avertisment prezent.
- **D3-V4** D3-D5: rd. 37/36 == liniile ITV ale lunii scenei (prin
  `InchidereTvaService`, conturi din politică).
- **D3-V5** formulele: 19, 30, 31, 35, 36/37 exclusive, 40, 43, 44/45 exclusive,
  recalculate independent în check; externii intră unde trebuie; 38∧41 ⇒ refuz.
- **D3-V6** storno în perioadă: operare + storno în aceeași lună ⇒ rândul
  operațiunii net 0, `Randuri` = 2 (nu dispare); storno de N19 în 2026 ⇒ rd. 16.
- **D3-V7** (bugetar) `Nemapate` gol, toate rândurile 0, 0 `MapareD300`.
- **HTTP** (calea reală, 66h): Admin 200 cu 55 rânduri; User 200 cu `Randuri: []`;
  perioadă lipsă/inversată, extern negativ, 38∧41 ⇒ 400.

## Regula de oprire

Felia e închisă când: D3-V1…V7 trec pe ambele profiluri (0 FAIL), `--dump-metadata`
și driftul openapi sunt verzi, iar pe clona bazei de import, prin API și în
browser, se poate face drumul: **D300 pe o lună din 2025 → rd. 9/24 egale la cent
cu decontul (`/decont-tva`) pe N21 → panoul „Neincluse" arată exact grupurile fără
mapare → rd. 37 comparat cu ITV-ul lunii, cu diferența explicată prin punțile
NTC**. Perf măsurată (addendum în `p5-perf-masuratori.md`). Decizia 69 scrisă.

Explicit NU în regula de oprire: XML/PDF, versionare de formular, rânduri
intracomunitare, TVA la încasare, D394.
