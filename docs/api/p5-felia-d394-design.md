# Pasul 5, felia 14 — D394 (declarația informativă) ca proiecție peste `RegistruTva` — DESIGN

Felia 12 (decizia 69) a așezat cifrele registrului fiscal pe rândurile D300.
Felia de față face același lucru pentru **formularul 394** — aceleași cifre,
agregate **per partener** (tip de partener × CUI × tip de operațiune × cotă), cu
rezumatele lui și regula proprie de numărare a facturilor. Textul care bate:
36f („D394 ca proiecție peste registre") și 35c (D394 = checklist de
completitudine, NU model de date). Deci felia produce **listele și rezumatele
declarației și dovada că nu pierde nimic din registru**; nu produce XML-ul și nu
persistă declarația.

Formularul: **OPANAF 3769/2015, modificat prin OPANAF 2194/2025** (M.Of.
852/17.09.2025; cotele 21/11 adăugate, cotele istorice păstrate; XSD
`d394_20250917.xml`, v1.02). Structura completă, câmp cu câmp, regulile de
agregare și validările sunt în `docs/api/d394-structura-2026.md` (sursele
primare în `anaf/d394/`, gitignored).

## Ce a arătat explorarea (fapte, nu design)

- `RegistruTva` poartă deja **`PartenerId` snapshot** (nullable, tipizat
  `Repartitor`), `DocumentId`, `DetaliuId`, `Sens`, `TipTvaId`, `Regim`, `Cota`,
  `Baza`, `Tva`, `Storno` — agregarea per partener e un join direct, nu o
  plimbare peste documente (invariant III). `PartenerId = null` când
  `SursaContrapartida` e Explicit/TipMaterial; pe `Decont` „partenerul" e
  Angajatul.
- `Partener` are **doar `CodFiscal` + `RegistruComert`**. Nu există tip de
  persoană, țară, județ, adresă, statut de plătitor de TVA, TVA la încasare,
  afiliere. Import1C pune CUI real în `CodFiscal`.
- `TipTva.CategorieD394` există ca **coloană null în tot seed-ul** (36d: „se
  fixează la proiecția D394"). Nu există marcaj de bon fiscal / autofactură /
  factură simplificată / anulată; nu există `Serie` ca coloană; nu există
  nomenclator de țară/județ; nu există cod NC / categorie de bunuri cu taxare
  inversă pe produs.
- Scena ModelCheck a D300 creează parteneri FĂRĂ `CodFiscal`.

Concluzia de model din structură (§6): **sumele sunt o proiecție pură peste
`RegistruTva`; identitatea rândului cere clasificarea partenerului** (tip 1–4),
regimul furnizorului (AI) și — pentru detaliile V/C — categoria de bunuri.
Felia tranșează primele două ca DATE pe `Partener` (satelitul 34g se deschide
DOAR cât cere D394) și lasă a treia cu nume.

## De ce tipul de operațiune e politică, iar tipul de partener e cod

Tipul de operațiune (L/A/V/C…) e — ca rândul D300 — o funcție de
`(TipTva × Sens)`: N21 pe livrare = L, pe achiziție = A; TI21 pe livrare = V,
pe achiziție = C. Aceeași logică de la decizia 69b: nu coloană pe `TipTva`
(perechea e direcțională — 36d), ci **politică `(TipTva × Sens) → tip`**, ca un
client cu un `TipTva` propriu să-l așeze fără release (invariant IV). Lista
tipurilor de operațiune e enum în cod (`Int_tipOpSType` e stabilă din 2016;
formularul o schimbă rar și structural — nu e nomenclator ca rândurile D300).

Tipul de partener (1–4) NU e politică: e o **funcție a nomenclatorului**
(stabilit în RO / în UE / în afara UE; înregistrat în scopuri de TVA; persoană
fizică) — definiția e a legii și nu variază per client. Deci: date pe
`Partener`, derivarea în cod. `CategorieD394` de pe `TipTva` **moare** (coloană
nefolosită, înlocuită de politică; se scoate din model — nu rămâne un string
magic null).

## Deciziile de fixat

### D4-D1 — Identitatea fiscală a partenerului = 4 câmpuri pe `Partener`, nu satelit

Pe `Partener` (nu pe baza `Repartitor` — calitățile fiscale sunt ale
partenerului, nu ale gestiunii/angajatului):

| Câmp | Tip | Default | Rol |
|---|---|---|---|
| `TipPersoana` | enum `TipPersoana { Juridica = 1, Fizica = 2 }` | Juridica | PF ⇒ `tip_partener = 2`; CUI-ul e CNP |
| `Tara` | string(2), cod ISO 3166-1 alpha-2 | `"RO"` | stabilit în RO / UE / non-UE |
| `InregistratTva` | bool | false | înregistrat în scopuri de TVA în RO |
| `TvaLaIncasare` | bool | false | furnizor în sistemul TVA la încasare ⇒ A devine AI |

Derivarea `tip_partener`, în cod (`D394Proiectii.TipPartener(p)`):
`Fizica ⇒ 2`; `Tara == "RO" ∧ InregistratTva ⇒ 1`; `Tara == "RO" ⇒ 2`;
`Tara ∈ UE ⇒ 3`; altfel `4`. Lista statelor membre = constantă în cod
(`Comun/TariUe.cs`, 27 coduri) — e lege, nu politică.

`cuiP` = `CodFiscal` normalizat: trim, majuscule, **prefixul `RO` tăiat** (doar
pentru `Tara == "RO"`; codurile străine rămân întregi). Tip 1 cu `CodFiscal` gol
= rândul se emite, cu AVERTISMENT (ANAF îl respinge; noi nu ascundem cifra).

**Istoricul înregistrării în scopuri de TVA** (un partener devine/încetează
plătitor în cursul anului) NU intră: statutul e cel de azi, restanță cu nume
(D4-r1). Adresa structurată a PF fără CNP (`taraP…detP`) NU intră (D4-r2) —
rândul PF fără CNP iese cu avertisment.

Editabile în XAF (baseline `ContaUiBaseline`, grup „Fiscal" pe `Partener`) și pe
OData (nomenclator viu = CRUD, 56). **Import1C**: catalogul 1C `Partenerii`
(`_Reference35`, `1C/01-1c-structura-si-acces.md` §tabel) poartă `PersJurFiz`,
`Tara`, `CNP`, `Nerezident`, `Intracomunitar`, `PoliticaTVA`,
`DataLuariiInEvidentaTVA`, `NuIncludeInDec394` — se PREIAU (`PersJurFiz` ⇒
`TipPersoana`; `Tara` ⇒ ISO-2; `PoliticaTVA`/`DataLuariiInEvidentaTVA` ⇒
`InregistratTva` (+ `TvaLaIncasare` dacă politica o spune); PF cu `CNP` ⇒
`CodFiscal = CNP`), prin view-ul `flax.Partenerii` extins cu coloanele astea
(contract de coloane, 45a). Unde sursa tace se DERIVĂ (prefix `RO` ⇒
`InregistratTva`; 13 cifre ⇒ `Fizica`) și derivarea se RAPORTEAZĂ (35b:
evidență, nu canonic). `NuIncludeInDec394` e fapt al sursei: se raportează
(câți parteneri îl au), nu se modelează. Raportul de reconciliere trebuie să
rămână identic cu baseline-ul — nomenclatorul se îmbogățește, cifrele nu se
mișcă.

### D4-D2 — `MapareD394`: politica `(TipTva × Sens) → TipOperatiuneD394`

Enum în cod `TipOperatiuneD394 { L = 1, A, AI, LS, AS, V, C, N }` (`[XafDisplayName]`
cu denumirile din formular; pe sârmă string — 57a).

Clasa `MapareD394 : BaseObject` în `BusinessObjects/Politici/`: `TipTvaId` (FK,
required), `Sens` (`SensTva`), `Tip` (`TipOperatiuneD394`). **O singură mapare
per pereche** (unic filtrat pe `GCRecord = 0`, ca la D300 — 69b) — un grup nu
poate fi pe două tipuri. Ținta permisă din politică: doar `L, A, V, C, LS, AS`;
**`AI` și `N` nu se mapează** — `AI` se DERIVĂ în cod (`A` × partener cu
`TvaLaIncasare`), iar `N` (achiziții de la neînregistrați) n-are sursă azi
(liniile fără TVA de la PF nu ajung în `RegistruTva`; D4-r3). Regulă XAF +
gardian la seed pentru ținta interzisă.

Seed **privat** (bugetar: 0 mapări, `VerificaProfil` o cere):

| TipTva | Livrare | Achiziție |
|---|---|---|
| N21, N11, N9, N19 | L | A |
| TI21, TI19 | V | C |
| NED21 | — (nemapat deliberat: nedeductibilul nu se livrează) | A (achiziție taxabilă, se declară cu TVA-ul facturat) |
| SDD, SFD, NIM | — | — (nemapate deliberat: scutite/neimpozabile nu se declară în 394) |

Cota declarată = `int(Cota)` din snapshot-ul registrului pentru L/A/AI/C; **0
pentru V/LS/AS/N** (regula formularului — `facturiV` există doar la `cota = 0`);
un `Cota` ne-întreg ⇒ avertisment + rândul cu cota trunchiată (nu se pierde).

`VerificaProfil` (36c): privat ⇒ fiecare `TipTva` seed-uit are mapare pe cel
puțin un sens SAU e în lista explicită a nemapatelor deliberate; ținta nu e
AI/N; bugetar ⇒ 0 mapări. Un `TipTva` nou fără mapare apare în `Neincluse` cu
cifrele lui, nu e refuzat (raportare ≠ operare).

### D4-D3 — Proiecția: agregat per document, apoi per rând `op1`, formulele în cod

`D394Proiectii.D394(os, dataStart, dataEnd)` →
`D394Dto { Operatiuni: List<D394Operatiune>, Rezumat: List<D394Rezumat>, RezumatCote: List<D394RezumatCota>, Neincluse: List<D394Neinclus>, Avertismente: string[] }`.
Un formular nu se paginează (fără `DataSourceLoader`). Pașii:

1. **Agregatul de registru**, SQL/LINQ, pe `Data ∈ [dataStart, dataEnd]`, grupat
   pe `(DocumentId, PartenerId, Sens, TipTvaId, Cota)` — Σ`Baza`, Σ`Tva`,
   `Count`. `Storno` NU se filtrează (68): stornoul poartă `TipTva`-ul și cota
   ORIGINALĂ, cu sume negative — exact cerința formularului (§5.3). Perioada e pe
   `Data` rândului de registru, ca la D300 și jurnale (data stornării pentru
   storno — 25d); „data primirii" pentru facturile sosite târziu nu e modelată
   (D4-r4).
2. **Clasificarea**: `PartenerId` ⇒ `Partener` (join pe frunză, nu pe
   `Repartitor`): `tip_partener` (D4-D1), `cuiP`; `(TipTvaId, Sens)` ⇒ tip
   (D4-D2), cu `A → AI` dacă `TvaLaIncasare`. Grupurile fără `PartenerId`, cu
   repartitor care nu e `Partener` (Angajatul de pe DEC), sau cu pereche nemapată
   ⇒ **`Neincluse`** cu cauza (`FaraPartener` / `RepartitorNePartener` /
   `TipTvaNemapat`), `TipTvaCod`, `Sens`, `Baza`, `Tva`, `Randuri`.
3. **Numărul de facturi, per document** (§5.2): un document se numără **1 pe
   cota cu TVA-ul (absolut) maxim și 0 pe celelalte**; la egalitate, cota mai
   mare. Un document storno se numără ca factură (1) pe aceeași regulă. Documentul
   cu linii pe două tipuri (L și V pe aceeași factură) se numără separat per tip.
4. **`op1`**: grupare pe `(tip_partener, cuiP, tip, cota)` — Σ baze, Σ TVA, Σ
   nrFact, `Denumire` partener, `Randuri`, `Documente` (count distinct). V n-are
   coloană TVA în XSD: TVA ≠ 0 pe V ⇒ **avertisment cu suma**, nu înghițire
   (excepția 69-r4 s-a închis prin 70a — TI × Colectat are `ValoareTva = 0` din
   motor; pe date pre-F13 avertismentul e adevărul, 70d).
5. **`rezumat1`** per `(tip_partener, cota)`: `facturi*/baza*/tva*` pe fiecare
   tip, cu regulile de prezență din §4.2 (null unde XSD-ul cere absent — nu 0);
   `nrCui1..4` = parteneri distincți per tip. **`rezumat2`** per cotă: sumele de
   control ale coloanelor pe care le avem (L/A/AI/V/C); restul null.
6. Rotunjirea: proiecția livrează **bani exacți** (sumele registrului); rotunjirea
   la leu întreg e a fișierului XML (35c), nu a proiecției — altfel cusătura cu
   D300 și cu registrul n-ar mai fi la cent.

DTO-urile plate, `sealed`, enum-uri string; `Cota` int; `Baza`/`Tva` `decimal?`
(null = coloana nu există pe acel tip, ca la D300 — niciodată 0 în loc de null).

### D4-D4 — Nimic nu se pierde: `Neincluse` + cusătura cu D300

- **D4-V-registru**: Σ `Operatiuni.Baza/Tva` + Σ `Neincluse` == Σ `RegistruTva`
  pe perioadă, per sens, ambele coloane (TVA-ul de pe V intră în avertisment ȘI în
  cusătură — nu dispare).
- **D4-V-D300**: pe scena ModelCheck, pentru perechile mapate în ambele
  declarații: Σ L la cota 21 == rd. 9 D300 (N21/Livrare), Σ A + AI la cota 21 ==
  rd. 24, Σ C la 21 == rd. 12.1, Σ V == rd. 13 (baza), pe aceeași perioadă —
  două proiecții peste același registru, aceleași cifre. Diferența legitimă:
  D394 exclude SDD/SFD/NIM (rd. 14/15/29 D300) — verificată ca egalitate cu
  `Neincluse`, nu ignorată.
- Ecranul arată panoul „Neincluse în declarație" cu cifrele lui (62f), și
  avertismentele nominal (partener tip 1 fără CUI, PF fără CNP, V cu TVA,
  cotă ne-întreagă, **V/C fără detaliul op11** — vezi mai jos).

### D4-D5 — Ce cere formularul și modelul nu are: se RAPORTEAZĂ, nu se inventează

Fiecare cartuș/atribut fără sursă în model iese din proiecție cu null și e
listat în `Avertismente` DOAR când perioada are cifre care l-ar cere:
- **`op11`/`detaliu`** (categoria de bunuri + cod NC pentru V/C): obligatoriu în
  XSD când există V sau C ⇒ avertisment „V/C fără categorie de bunuri (D4-r5)" cu
  suma; rândul `op1` rămâne.
- **`N`** (achiziții de la neînregistrați cu `tip_document`) — D4-r3.
- **Bonurile fiscale** (cartușul G, `op2`, I.1) — D4-r6; **autofacturi,
  facturi anulate, simplificate, plaje `serieFacturi`, `facturi`** (I.2) —
  D4-r7; **I.3 bifele soldului negativ** — D4-r8; **I.4/I.5 TVA la încasare
  (sume)** — D4-r9 (flag-ul furnizorului intră, sumele plăților nu); **I.6, I.7
  CAEN, antetul, reprezentantul, întocmitorul, opțiunea** — D4-r10 (date de
  profil, `SetareProfil`); **XML-ul** — 35c.

Toate cu nume în decizie; nimic „de completat manual" nu e tăcut.

### D4-D6 — API: un endpoint, perioada obligatorie, răspuns întreg

`GET api/proiectii/d394?dataStart=&dataEnd=` → `200 D394Dto`, `400 EroriDto`
(perioadă lipsă/inversată — convenția 70f). `Secured(typeof(RegistruTva))`,
`using var`, materializare cât OS-ul e viu; `User` ⇒ 200 cu liste goale (ușa
filtrează rândurile — 69g). `D394Controller.cs` sub `ContaApiController`;
proiecția + DTO-urile în `Module/Proiectii/D394Proiectii.cs`.

### D4-D7 — Client: `/d394`

Ecran `felii/tva/D394.tsx`: bara = perioadă (`useUrlStare`, implicit luna
curentă); corpul = (1) grila `op1` grupată pe tip de partener (C/D/E/F), coloane
CUI · Denumire · Tip · Cotă · Facturi · Bază · TVA, sortabilă, filtrabilă local;
(2) rezumatele C–F ca tabele mici per cotă; (3) panoul „Neincluse"; (4)
avertismentele. `key` pe parametri, fără remote ops, fără export. Rută + NavLink
în `App.tsx`; `--dump-metadata` (două enum-uri noi). Câmpurile noi ale
partenerului apar în ecranul de nomenclator al partenerului acolo unde există
(XAF sigur; React dacă felia partenerului există — altfel `lista-react.md`).

### D4-D8 — Politica se vede: XAF + `VerificaProfil`

`MapareD394` are ListView/DetailView (lookup `TipTva`); `Partener` primește grupul
„Fiscal". `VerificaProfil` extins (D4-D2). `CategorieD394` se scoate din `TipTva`
(migrație aditivă-negativă: drop coloană; nimeni n-o citește — verificat prin
grep în explorare).

## Ce NU intră, cu motiv

- **Fișierul XML** (35c) — cifrele + maparea documentată în structură §4/§7.
- **Declarația ca entitate** (perioadă/stare/recipisă) — fără cerință.
- **Istoricul statutului de TVA al partenerului** (D4-r1), **adresa PF** (D4-r2),
  **N** (D4-r3), **data primirii** (D4-r4), **op11/NC** (D4-r5), **bonuri
  fiscale** (D4-r6), **I.2 facturi/plaje/autofacturi/anulate** (D4-r7), **I.3**
  (D4-r8), **sumele TVA la încasare** (D4-r9), **antet/profil/CAEN/I.6** (D4-r10).
- **Nomenclator de țări/județe**: `Tara` e cod ISO-2 liber cu validare de
  format (regex `^[A-Z]{2}$`) — un nomenclator vine când adresa vine (34g).

## Riscurile pin-uite (ținta review-ului advers)

1. **nrFact**: regula 1/0 per document pe cota cu TVA maxim — nu `count distinct`
   per rând; egalitatea ⇒ cota mai mare; storno = 1; document cu L și V = 1 + 1.
2. **CUI**: prefixul RO tăiat doar pe RO; codurile străine intacte; CUI gol pe
   tip 1 = avertisment, nu pierdere; același partener cu două forme de scriere
   (`RO123`/`123`) se unește pe cheia normalizată — dacă doi parteneri distincți
   au același CUI, rândul e unul singur (formularul cere unicitate pe cuiP) și
   avertismentul spune care s-au unit.
3. **Tip partener 3 vs 4**: lista UE completă (27, fără UK); `Tara` goală ⇒ RO
   (default) — nu 4.
4. **V fără coloană de TVA**: TVA ≠ 0 pe V (date pre-F13) ⇒ avertisment + în
   cusătură; nu se trunchiază.
5. **Cusătura cu D300**: aceeași perioadă, același registru — egalitate la cent
   pe perechile mapate în ambele; diferența = exact `Neincluse` (scutitele).
6. **`RegistruTva.Partener` e `Repartitor`**: join-ul se face pe tabela `Partener`
   (TPT), nu cast pe navigație lazy; Angajatul de pe DEC ⇒ `Neincluse`.
7. **Securitate**: aceeași ușă secured ca D300; `User` ⇒ 200 gol.
8. **Import1C**: raport de reconciliere IDENTIC cu baseline-ul după îmbogățirea
   partenerilor (D5 — precedentul DIM-4/F13).

## Verificări (ModelCheck, ambele profiluri)

- **D4-V1** seed: mapările din tabelul D4-D2, ținte permise, `VerificaProfil`
  verde; bugetar 0 mapări; `CategorieD394` absentă din model.
- **D4-V2** (privat) scenă proprie `E2E-D394`, parteneri cu `CodFiscal`
  (`RO12345678`, `12345678`, CNP, `DE…`, `US…`), cele 4 tipuri de partener +
  PF; FCT/FCL/RLF/RDC/DEC pe N21/N11/TI21/NED21/SDD; verificate: tip_partener,
  cuiP normalizat, tip (L/A/AI/V/C), cota (0 pe V), sume exacte.
- **D4-V3** nrFact: factură cu N21+N11 ⇒ 1 pe 21, 0 pe 11; egalitate ⇒ cota
  mai mare; storno ⇒ 1 cu sume negative; L+V pe aceeași factură ⇒ 1+1.
- **D4-V4** cusătura cu registrul (D4-D4), ambele coloane, per sens; DEC ⇒
  `RepartitorNePartener`; SDD ⇒ `TipTvaNemapat`.
- **D4-V5** cusătura cu D300 pe scenă (rd. 9/24/12.1/13).
- **D4-V6** rezumate: `rezumat1` recalculat independent din `op1`; null unde
  XSD-ul cere absent; `nrCui1..4`.
- **D4-V7** avertismente: tip 1 fără CUI; V cu TVA (pe rând SQL pre-F13
  injectat); V/C fără op11; PF fără CNP; cotă ne-întreagă.
- **HTTP** (66h): Admin 200; User 200 gol; perioadă lipsă/inversată ⇒ 400
  `EroriDto`.

## Regula de oprire

Felia e închisă când: D4-V1…V7 trec pe ambele profiluri (0 FAIL); migrațiile
aplicate pe toate bazele de dev; `--dump-metadata` și driftul openapi verzi;
Import1C re-rulat integral cu raport de reconciliere **identic** cu baseline-ul
(D5) și partenerii clasificați (raportul spune câți au fost derivați); pe clona
bazei de import, prin API și în browser: **D394 pe septembrie 2025 → Σ L la 21%
egal la cent cu rd. 9 D300 al lunii, Σ A(+AI) la 21% cu rd. 24, C cu rd. 12.1;
partenerii cu CUI real vizibili pe cartușul C; `Neincluse` explicat integral
(scutite + fără partener + DEC)**; perf măsurată (addendum). Decizia 71 scrisă
(jurnal + README + CLAUDE.md §71 + istoric), restanțele D4-r1…r10 cu nume.

Explicit NU în regula de oprire: XML, bonuri fiscale, op11, TVA la încasare,
adrese, SAF-T.
