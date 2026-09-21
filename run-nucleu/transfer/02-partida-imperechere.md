# Explorarea 02 (pasul 3, transferul): „ce e o partidă” și împerecherea — cod + cifre

Agent read-only, 2026-09-19. Baza: `Atlas.Conta.Nucleu.Fizica.x1` (clona Flax,
2025 integral, profil privat). SQL-ul în `02-sql/`, ieșirile brute în `02-out/`.
Scriere doar în schema `tr`. **Nu se tranșează nimic aici** — se livrează
semantica de azi (cu `fișier:linie`) și cifrele.

Capcană respectată: toate cele 44.448 `Imperecheri.Data` sunt 2026-09-18
(artefact de import, `02-out/08-b6-definitii.txt` §B6.0), deci nicio cifră de
mai jos nu se sprijină pe data împerecherii. Consecința ei e ea însăși o cifră
(§B6.0b).

---

## A. Semantica de azi, din cod

### A1. Ce e o partidă azi

**`Document.TotalStingere` — fapt SCRIS la operare, brut.**
`Motor/MotorOperare.cs:396-397`:
`doc.TotalStingere = Scara.RotunjesteBani(doc.LiniiCreanta(doc.Detalii.AsQueryable()).Sum(d => d.Valoare + d.ValoareTva))`.
Se scrie pe **orice** document operat, nu doar pe facturi și plăți; devine
`null` la anulare (`MotorOperare.cs:620`) și **rămâne neatins la storno**
(`Storneaza`, `MotorOperare.cs:628-705`, nu-l resetează). Hook-ul polimorf de
filtrare e `Document.LiniiCreanta` (`BusinessObjects/Documente/Document.cs:188`),
suprascris o singură dată azi — `ReturClient.LiniiCreanta`
(`Retururi.cs:190-191`: doar liniile fără lot). Scara îl tratează ca bani
(`Comun/Scara.cs:103-107`).
`ImperechereService.Total` doar îl citește și refuză documentul operat fără el
(`Motor/ImperechereService.cs:30-41`).

**`Imperechere`** (`BusinessObjects/Documente/Trezorerie.cs:654-676`):
`DocumentStingatorId` (tipat `Document`, nu `DocumentTrezorerie` — rolul e
polimorf, 48b), `DocumentId` (documentul stins), `Suma`, `Data` (F27-D8, fapt
datat), `InverseazaId` (rândul invers 1:1), `Autogenerat`. Nu e document (fără
ciclu Draft/Operat) — e o legătură m2m între două documente deja operate
(`Trezorerie.cs:641-652`).

Cine scrie: `ImperechereService.Creeaza` (`:164-182`, fără commit),
`Imperecheaza` (`:62-72`, tranzacție publică), `CreeazaAutomataLaOperare`
(`:144-161`, chemată din `MotorOperare.cs:425`), `CreeazaInvers` (`:89-105`) și
`InverseazaLaStorno` (`:111-129`). Pe ușa securizată gardianul
`GardianEditare.VerificaImperechere` (`Motor/GardianEditare.cs:520-575`) refuză
editarea (`:547`), refuză rândul invers cules manual (`:552-554`), cere data
(`:556-558`) și perioada deschisă (`:560-563`), iar la ștergere refuză o
împerechere din perioadă închisă (`:525-527`), una deja desfăcută (`:531-533`)
și rândul invers însuși (`:534-536`).

**`Asignat` = Σ pe AMBELE roluri, ALGEBRIC** (`ImperechereService.cs:45-48`);
`Ramas = Total − Asignat` (`:50-51`). Geamănul în SQL:
`SolduriService.AsignariPanaLa` (`Motor/SolduriService.cs:132-140`), unpivot pe
ambele laturi, tăiat pe `Imperechere.Data`.

**`AsignatFataDe`** (`ImperechereService.cs:327-362`) — plafonul consumat față
de o contrapartidă × sens: Σ stingerilor către documente pe care apare acea
contrapartidă (un document care NU declară `SensDeStins` se scade din AMBELE
jumătăți, `:350-355`), PLUS tot ce s-a stins pe stingătorul însuși (`:358-360`,
lanțul avans↔regularizare). Materializează polimorf documentele stinse —
singurul loc unde `SensDeStins` se rezolvă per rând, pe o mulțime mărginită
(`:333-339`).

**`SensDeStins`** (hook, `Document.cs:271`, `null` = nedeclarat):
`FacturaIntrare` → Datorie (`FacturaIntrare.cs:23-24`), `NIR` → Datorie
(`DocumenteGestiune.cs:30-31` — recepția contează pe NIR, deci NIR-ul lasă
soldul creditor pe furnizor), `FacturaIesire` → Creanta (`FacturaIesire.cs:20-21`),
`Decont` → Datorie (`Decont.cs:18-19`), `DocumentTrezorerie` → `SensPropriu()`
(`Trezorerie.cs:187`), adică `Plata` → Creanta (`:567`) și `Incasare` → Datorie
(`:593`), `ReturFurnizor` → Creanta (`Retururi.cs:44-45`), `ReturClient` →
Datorie (`Retururi.cs:131-132`; ambele declarate dar azi NEATINSE — după
operare valorile sunt negative, deci `Ramas` e negativ și refuzul de rest cade
primul, `Retururi.cs:35-43`, `:128-130`).

**`PoateFiStins`** (`Document.cs:254`, implicit `true`): `false` pe viramentul
intern (`Trezorerie.cs:197`), pe `Dvi` (`Dvi.cs:18`) și pe cele trei documente
de imobilizări (`Imobilizari.cs:22,364,547`).

**`CapacitateStingere`** (`Document.cs:237`, implicit `null` = tipul nu stinge
nimic): trezoreria — o singură contrapartidă, plafon pe UN sens, opusul
soldului propriu (`Trezorerie.cs:171-177`); nota contabilă — plafon per
(repartitor explicit × latură), pe MIȘCAREA NETĂ, cheia cu net zero nu intră
(`NotaContabila.cs:57-86`).

**`PartidaDeschisa`** (`BusinessObjects/Registre/SolduriPerioada.cs:88-102`) se
materializează prin SQL brut la închiderea perioadei,
`SolduriService.MaterializeazaPartide` (`Motor/SolduriService.cs:107-127`):
`Rest = TotalStingere − COALESCE(Asignat, 0)`, filtru
`Stare = Operat AND TotalStingere IS NOT NULL AND DataInregistrare <= sfârșitul lunii AND Rest <> 0`.
Cheia e `(An, Luna, DocumentId)` — **partida e DOCUMENTUL, iar „restul” e o
cifră de document, nu un sold de cont**. Se scrie doar pe perioadele de
referință (88d), se reconstruiește cu raport de diferențe
(`SolduriService.DiferentePartide`, `:459-492`).

**Proiecția candidaților** `ImperecheriProiectii.DocumenteCuRest`
(`Proiectii/ImperecheriProiectii.cs:110-180`) e o UNIUNE de ȘASE ramuri
tipizate, fiecare cu literalii ei de `Tip` și `Sens` și cu propria alegere de
contrapartidă — adică al doilea loc, după hook-uri, unde „ce e o partidă” e
scris pe tip:

| ramură | linii | Tip | contrapartidă | Sens |
|---|---|---|---|---|
| `FacturaIntrare` | `:118-123` | „FCT” | `PredatorId` (furnizorul) | `Datorie` |
| `FacturaIesire` | `:124-129` | „FCL” | `PrimitorId` (clientul) | `Creanta` |
| `Plata` | `:138-145` | „PLT” | `PrimitorId` (beneficiarul) | `Creanta` |
| `Incasare` | `:146-153` | „INC” | `PredatorId` (plătitorul) | `Datorie` |
| `Decont` | `:154-159` | „DEC” | `PredatorId` (titularul) | `Datorie` |
| `ReturClient` | `:164-169` | „RDC” | `PredatorId` (clientul) | `Datorie` |

`Total` e peste tot `d.TotalStingere ?? 0m`. `Plata`/`Incasare` filtrează
viramentul direct în SQL (`!(d.Predator is ContPropriu && d.Primitor is
ContPropriu)`, `:140`, `:148`). **NIR-ul, `ReturFurnizor` și `NotaContabila` nu
au ramură**, deși NIR-ul e cel care postează datoria pe 401 și își declară
`SensDeStins` — deci un NIR nu apare niciodată ca „document cu rest” în panoul
de stingere, dar are partidă în `PartideDeschise`. Comentariul `:39` recunoaște
că literalul de `Sens` dublează `Document.SensDeStins`.

**Regula de dată (88j)**: `Imperechere.Data` se persistă la creare (automat =
`DataInregistrare` a stingătorului, `ImperechereService.cs:158-160`; manual =
data cerută), ≥ înregistrarea AMBELOR documente (`:197-200`) și în perioadă
deschisă (`:67`, `:80`). Ștergerea directă doar în perioadă deschisă; dintr-o
perioadă închisă se desface prin rând INVERS (sumă negativă, legătură 1:1).
Asignările și partidele însumează algebric.

**Stornoul unui document împerecheat** (`MotorOperare.cs:636` →
`ImperechereService.InverseazaLaStorno`, `:111-129`): legăturile VII (cele
neanulate deja de un invers) dintr-o perioadă DESCHISĂ blochează stornoul cu
refuz („ștergeți-le întâi”); cele din perioadă închisă primesc rând invers la
data stornării. Se aplică identic indiferent de rol — documentul stornat poate
fi stinsul sau stingătorul (`:112-114`, filtru pe ambele coloane). Efectul pe
partide: rândul invers ELIBEREAZĂ restul CELUILALT document (Asignat scade
algebric), iar documentul stornat iese cu totul din `PartideDeschise` (filtrul
cere `Stare = Operat`) deși `TotalStingere` îi rămâne scris și rândurile lui de
registru (originale + inverse) rămân.

### A2. Stingerea din trezorerie

**Ce stinge**: `DocumentTrezorerie` are o singură contrapartidă — latura care
nu e `ContPropriu` (`Trezorerie.cs:134`, `GetContrapartidaId`, abstract; `Plata`
→ Primitor `:562`, `Incasare` → Predator `:589`). Plafonul = `TotalStingere` al
documentului, pe UN sens: `SensPropriu().Opus()` (`Trezorerie.cs:171-177`).
Invariantul de contrapartidă (`ImperechereService.cs:240-245`): contrapartida
stingătorului trebuie să apară pe `PredatorId` sau `PrimitorId` ale documentului
stins. Suma ≤ plafon − `AsignatFataDe` (`:302-307`) ȘI ≤ `Ramas(document stins)`
(`:308-311`) ⇒ **plata parțială e suportată nativ**, fără câmp de stare.

**Liniile** nu participă la alegere: sunt defalcarea sumei
(`Trezorerie.cs:11-14`, `DocumentTrezorerieDetaliu:614-638`); plata autogenerată
dintr-o factură clonează per linie `Valoare + ValoareTva` al liniei sursă
(`FacturaIntrare.cs:98`) — brutul.

**`LaturaPereche`** (`Trezorerie.cs:29-31`) NU e stingere: e legătura celor două
picioare ale viramentului 581. Viramentul nici nu stinge (`CapacitateStingere`
= `null`, `:171-173`), nici nu poate fi stins (`PoateFiStins` = `!EsteVirament`,
`:197`), și e exclus explicit din proiecția candidaților
(`Proiectii/ImperecheriProiectii.cs:130-148`).

**Ce postează** (`MotorOperare.cs:364-374` peste regulile de contare,
`Potrivire.Cont` cu `SursaCont` din `Comun/Enums.cs:116-121`). Pe Flax
(`02-out/12-a-politici.txt` §A.1, §A.4):

| tip | regulă | corespondențe reale |
|---|---|---|
| PLT | debit `RepartitorPrimitor` (401), credit `RepartitorPredator` | `401 = 5121` 2.043 rd / 75.850.253,48; `401 = 5124` 271 / 11.787.268,87; `401 = 5311` 172 / 58.568,11 |
| INC | debit `RepartitorPrimitor`, credit `RepartitorPredator` (4111) | `5121 = 4111` 22.400 / 96.502.595,01; `5311 = 4111` 4.918; `5125 = 4111` 4.003; `5124 = 4111` 60 |

**Avansul pe 419/409**: nu există tip de document de avans. Avansul primit e o
LINIE pe factura de ieșire, al cărei `TipMaterial` are `ContImplicit = 419`
(„Clienţi – creditori”, natură ne-stoc) — regula generică FCL (debit
`RepartitorPrimitor` = 4111, credit `TipMaterial`) postează `4111 = 419`:
2.036 documente, 8.392.226,59 (`§A.4`). Regularizarea o face o notă contabilă
(`419` debit, 1.998 documente, 8.252.374,26). Contul 409 nu e folosit; 4091/4092
apar de 8 ori pe FCT și de 7 ori pe note. **Plata pe cont nu postează niciodată
419/409** — pe Flax plata debitează direct 401, deci avansul de furnizor nu are
partidă separată.

**Decontul cu angajatul (542/4xx)**: regula există (`DEC`: debit din
`TipMaterial`, credit `RepartitorPredator` cu fallback 542, `§A.1`; TVA-ul
`4426 = 542`, `§A.2`), `Decont.RepartitorImplicitCredit = PredatorId`
(`Decont.cs:29`, soldul avansului se ține per angajat). **Pe Flax: 0 documente
`Decont` și 0 picioare pe 542** (`02-out/07-b5-parteneri.txt` §B5.1) — cerința e
în cod, nu în date.

### A3. Conexul FCT→NIR

Legătura e la nivel de **DOCUMENT**: `Document.DocumentSursaId`
(`Document.cs:139-144`), scrisă de `MotorOperare.GenereazaConex`
(`:534-561`: `conex.DocumentSursa = sursa`, `Autogenerat = true`), pe politica
`PoliticiConex` (FCT→NIR, `NaturaFiltru = 1` Stoc, fără inversare de laturi —
`02-out/12-a-politici.txt` §A.3). **`LinieSursaId` NU există pe linia de NIR**:
e declarat doar pe `DescarcareGestiuneDetaliu`
(`DescarcareGestiune.cs:118-119`) și pe linia de PIF (`Imobilizari.cs:99`);
măsurat: 34.289 linii de NIR, 0 cu `LinieSursaId`, și nicio linie din baza
întreagă nu-l are completat (`§A.5`). Trasabilitatea liniei FCT → linia NIR
trece doar prin LOT (linia de factură naște lotul, linia de NIR îl referă —
`NIR.PregatesteOperare`, `DocumenteGestiune.cs:43-51`).

**Împărțirea postării pe 401** nu e cod, e ABSENȚA unei reguli de politică:
„fără regulă potrivită = linia nu contează pe acest tip de document (așa se
împarte lanțul FCT/NIR fără dublă postare)” (`BusinessObjects/Politici/Politici.cs:71-72`,
`:85-86`). Pe Flax:

- **Linia FCT de stoc**: nicio `RegulaContare` pe FCT cu `NaturaFiltru = 1` ⇒
  nu postează nimic.
- **Linia NIR**: `NIR / NaturaFiltru = 1` → debit `TipMaterial`, credit 401 ⇒
  `371 = 401` 33.793 rd / 83.507.010,00 (plus 3028/3024/381/303). **NIR-ul
  creditează 401 cu NETUL.**
- **Linia FCT de servicii** (natura 2/3/4): reguli proprii → credit 401 (natura
  2 și 3) sau 404 (natura 4), debit din `TipMaterial` (624, 628, 613, 6232…).
- **Linia de TVA a FCT**: `PoliticiTva` FCT, `Directie = Deductibil`,
  `SursaContrapartida = RepartitorPredator` cu fallback 401 (`§A.2`) ⇒
  `4426 = 401`, 28.504 rd / 8.836.201,16. **Factura creditează 401 doar cu
  TVA-ul** (plus liniile non-stoc).

**FCT de servicii (fără NIR)**: 1.221 facturi, toate cu postare de terț
(`02-out/02-b2-fct-nir.txt` §B2.4) — își postează integral datoria pe 401/404.
**FCT cu mai multe NIR-uri**: 0 pe Flax (§B2.2, toate 17.814 au exact un NIR;
niciun NIR nu are două FCT). **Storno-ul NIR-ului**: inversează rândurile lui la
data stornării (`MotorOperare.cs:660-672`) și îi anulează partida (iese din
`PartideDeschise` prin `Stare = Stornat`), dar **nu atinge nici `TotalStingere`
al FCT, nici împerecherile FCT-ului** — legătura e la nivel de document și
împerecherea stă pe FCT, nu pe NIR.

### A4. Cazurile partener–partener

**Compensarea**: nu are document propriu — e `NotaContabila` cu rol de stingător
(`NotaContabila.cs:26-30`, decizia 48b). Contrapartidele sunt repartitorii
EXPLICIȚI ai liniilor (`NotaContabilaDetaliu.RepartitorDebitId/CreditId`,
`NotaContabila.cs:110-120`); plafonul e mișcarea NETĂ per (repartitor × latură),
cheia cu net zero nu intră (`:57-86`). O notă `401 = 4111` de 60 pe X dă
`(Datorie 60, Creanta 60)` — exact cele două stingeri legitime (`:51-53`).
Ambiguitatea de grup se refuză, nu se rezolvă tăcut
(`ImperechereService.cs:282-299`); apelantul poate alege explicit contrapartida
(`contrapartidaId`, `:252-257`).

**Nota cu doi parteneri**: permisă structural (fiecare linie poartă propriul
repartitor). Laturile notei trebuie să fie repartitori INTERNI
(`NotaContabila.cs:90-92`) — deci partenerul stă pe contul liniei, nu pe latură.
Dar invariantul de împerechere cere contrapartida pe `PredatorId`/`PrimitorId`
ale documentului STINS (`ImperechereService.cs:240-242`), nu pe liniile lui.

**Transferul între partidele aceluiași partener (avans → factură →
regularizare)**: e prevăzut explicit — un document poate apărea în AMBELE roluri
(`ImperechereService.cs:10-12`), `AsignatFataDe` numără și rolul de stins
(`:358-360`), `Plata ↔ Incasare` e permisă iar `Plata ↔ Plata` / `Incasare ↔
Incasare` refuzate (`:209-212`). Pe Flax **nu se produce niciodată**: niciun
document nu apare simultan ca stingător și ca stins
(`02-out/07-b5-parteneri.txt` §B5.6). Ce postează: nimic nou — împerecherea nu
are registru propriu, e o legătură.

---

## B. Cifrele, pe `Fizica.x1`

### B1. Per tip de document (`02-sql/01-b1-tipuri.sql` → `02-out/01-b1-tipuri.txt`)

205.186 documente operate (toate `Stare = Operat`; 0 stornate, 0 corecții).
**116.320 (56,7 %) postează pe ≥1 cont cu `RolTert <> 0`; 88.866 nu postează
niciun leu pe un cont de terț și au totuși `TotalStingere` scris.**

| ClrType | operate | cu terț | 1 cont | 2 conturi | 3+ | 0 part. | 1 part. | 2+ part. |
|---|---|---|---|---|---|---|---|---|
| NotaTransfer | 45.552 | 0 | 0 | 0 | 0 | 45.552 | 0 | 0 |
| FacturaIesire | 40.535 | 40.535 | 38.499 | 2.036 | 0 | 0 | 40.535 | 0 |
| DescarcareGestiune | 36.696 | 0 | 0 | 0 | 0 | 36.696 | 0 | 0 |
| Incasare | 31.381 | 31.381 | 31.381 | 0 | 0 | 0 | 31.381 | 0 |
| FacturaIntrare | 19.035 | 17.806 | 17.781 | 25 | 0 | 1.229 | 17.806 | 0 |
| NIR | 17.814 | 17.814 | 17.814 | 0 | 0 | 0 | 17.814 | 0 |
| NotaContabila | 7.818 | 4.269 | 2.323 | 1.937 | 9 | 6.816 | 997 | 5 |
| Plata | 2.486 | 2.486 | 2.486 | 0 | 0 | 0 | 2.486 | 0 |
| ReturClient | 1.666 | 1.631 | 1.631 | 0 | 0 | 35 | 1.631 | 0 |
| Asamblare | 1.228 | 0 | — | — | — | 1.228 | 0 | 0 |
| BonConsum | 547 | 0 | — | — | — | 547 | 0 | 0 |
| ReturFurnizor | 398 | 398 | 398 | 0 | 0 | 0 | 398 | 0 |
| ListaDiferenteInventar | 18 | 0 | — | — | — | 18 | 0 | 0 |
| InchidereTva | 12 | 0 | — | — | — | 12 | 0 | 0 |
| **total** | **205.186** | **116.320** | **112.313** | **3.998** | **9** | **92.133** | **113.048** | **5** |

**Un document = un partener, aproape fără excepție**: 5 documente (toate note
contabile) au ≥2 parteneri distincți pe postările de terț. Cele 2 conturi apar
pe FCL (4111 + 419: avansul), pe note (compensări) și pe 25 de FCT (401 + 408/4091).

Conturile de terț efectiv folosite (§B1.3): 4111, 419, 418, 4118 (rol Client);
401, 404, 408, 4091, 4092 (rol Furnizor). Niciun cont de terț nu apare pe
AMBELE laturi ale aceluiași document decât pe 27 de note.

### B2. FCT → NIR (`02-sql/02-b2-fct-nir.sql`, `03-b2b-diferente.sql`)

| cifră | valoare |
|---|---|
| FCT operate | 19.035 |
| FCT cu ≥1 NIR conex | 17.814 (toate cu **exact un** NIR) |
| FCT fără NIR (servicii) | 1.221 |
| NIR operate / cu FCT sursă / fără FCT | 17.814 / 17.814 / **0** |
| FCT cu mai multe NIR-uri | **0** |
| Σ postat pe 401 de FCT (perechile) | 8.949.652,58 |
| Σ postat pe 401 de NIR | 83.593.304,48 |
| Σ `FCT.TotalStingere` (perechile) | 100.417.152,95 |
| Σ (401 FCT + 401 NIR) | 92.542.957,06 |
| FCT cu `TotalStingere` = 401 FCT + 401 NIR **exact** | **16.017 / 17.814** |
| diferite | 1.797, Σ|Δ| = 7.874.195,89 |

**Cele 1.797 sunt EXACT facturile cu taxare inversă** (`Regim = 3`: 1.775
pure + 22 mixte, §B2b.1), iar delta e fix TVA-ul: autolichidarea postează
`4426 = 4427` (8.284 rd / 8.120.612,12), deci taxa nu ajunge niciodată pe 401,
dar `TotalStingere` o include (`Valoare + ValoareTva`). Pe restul (15.990 cu
regim normal + 27 fără rând de TVA) egalitatea e la cent.

Împărțirea e curată: pe 15.619 din 17.814 FCT cu NIR, ce postează factura pe 401
e **exact TVA-ul**; 1.229 postează zero (taxare inversă / fără TVA); 13 postează
brutul. NIR-ul postează baza pe 16.848 din 17.814 (966 diferă, Δ total
−97.247,22 — linii non-stoc și rotunjiri).

FCT fără NIR: 1.221, toate cu postare de terț; `TotalStingere` = Σ terț pe 1.084
(cele 137 care diferă sunt tot taxare inversă: 112 pure + 2 mixte + 23 de regim
normal cu Δ 451.001,93).

### B3. Cele două rupturi ale amendamentului 4 (`02-sql/04-b3-rupturi.sql`, `05-b3b-cele-21.sql`)

**Contul de terț al stinsului ≠ al stingătorului: tabelul cerut e GOL.** La
nivel de (document × cont) nu există nicio împerechere în care ambele documente
au postări de terț pe conturi disjuncte.

Reconcilierea cu proba M5 a review-ului (§B3b.1-2): cei 1.200 se despart în
**1.179 fără nicio postare de terț pe stins** + **21 artefact de selecție** —
`cub."_TertDoc"` alege UN PICIOR (`DISTINCT ON … abs(Valoare) DESC`), nu un
cont; agregate pe cont, cele 21 dispar (20 FCT↔Plata de 923.989,54 și 1
FCL↔NotaContabila de 0,45).

**Cele 1.179 împerecheri fără postare de terț pe documentul stins**: toate pe
`FacturaIntrare`, **1.085 documente distincte**, Σ 25.585.375,12 (spec-ul le
numea „1.179 documente”; sunt 1.179 legături pe 1.085 documente). Toate 1.085 au
NIR, iar NIR-urile lor creditează 401 cu 28.085.433,47 (§B3b.5) — sunt exact
facturile cu taxare inversă, care postează doar `4426 = 4427`. Exemple
(§B3.4): `ACH1006003` (2025-01-03, `TotalStingere` 779,92 — postează 4426 D
124,49 / 4427 C 124,49), `ACZ1000302` (232,79 — 37,19), `ACZ1005882` (15,35 — 2,44).

**Latura** (§B7.1): 43.237 împerecheri leagă documente pe laturi OPUSE (norma),
32 pe aceeași latură (24 FCL↔NotaContabila pe 4111 debit, 58.554,55; 8
FCT↔Plata pe 401 debit, 346.534,47), 1.179 fără latură pe stins. Transformarea
de azi pune AMBELE picioare pe latura stingătorului, deci pe 43.237 din 44.448
piciorul de pe partida stinsă cade pe latura opusă celei pe care documentul a
postat efectiv.

**Partener diferit** între stins și stingător: 1.193, din care 1.179 sunt cele
fără terț; rămân **14 cazuri reale** (13 FCL↔NotaContabila de 44.389,69 și 1
FCT↔NotaContabila de 5.251,00).

### B4. Avansul (`02-sql/06-b4-avans.sql`)

| cont | tip | documente | debit | credit |
|---|---|---|---|---|
| 419 | FacturaIesire | 2.036 | 0 | 8.392.226,59 |
| 419 | NotaContabila | 1.998 | 8.252.374,26 | 1.165,38 |
| 4091 | FacturaIntrare | 7 | 3.249,59 | 0 |
| 4092 | FacturaIntrare | 1 | 8.264,46 | 0 |
| 4091/4092 | NotaContabila | 7 | 8.264,46 | 37.963,85 |

**409 propriu-zis nu e folosit niciodată.** Avansul de client trăiește ca linie
de FCL (§A2).

**Facturi de ieșire împerecheate cu un avans** (stingător = document cu postare
pe 419): **1** (o notă contabilă, 0,45). Adică lanțul avans→regularizare NU
trece prin împerechere azi — trece prin nota contabilă care mută 419 pe 4111 în
CONTABILITATE, fără să atingă partidele.

**Rest brut (F0) contra rest compensat (cub)** pe cele 2.036 FCL cu 419:

| | facturi cu rest ≠ 0 | Σ rest |
|---|---|---|
| F0 (`TotalStingere − Asignat`) | 265 | 241.502,69 |
| cub (Σ pe toate conturile de terț) | 2.035 | 1.763.514,25 |

Exemplul canonic (§B4.2), identic cu cel din `nucleu-fizica.md` §3:
`FLXONL000094802`, `TotalStingere` 104,02, postări `4111 D 104,02`, `419 C 87,41`,
`4427 C 16,61`. F0 vede 104,02 de încasat; cubul vede 16,61 de sold pe partidă.
După încasarea celor 104,02, F0 închide partida, iar cubul rămâne cu −87,41 pe
4111 și +87,41 „nestins” pe 419: **cele două cifre nu măsoară același lucru**.

### B5. Decontul și partener–partener (`02-sql/07-b5-parteneri.sql`)

- **0 documente `Decont`, 0 picioare pe 542.** 122 repartitori `Angajat`, care
  apar în date doar ca PARTENER pe postări de terț (pe 419, ca „clienți
  creditori”) — vezi §B5.3. Deci calea 542/4xx nu e probată pe Flax.
- **Documente cu ≥2 parteneri distincți pe postările de terț: 5**, toate
  `NotaContabila` (§B5.3), toate mărunte: `SED00000651` (401 D pe ACTION S.A.
  −77,39 + 4111 C pe CENTRO C S.P.A. −0,72), `SED00000705` (401 D pe API
  COMPUTERHANDELS −258,73 + 4111 C pe CENTRO C −0,59), `SED00000739` (trei
  parteneri, ≤1,52), `SED00000759` (patru parteneri pe 4111 și 419, ≤42,02),
  `SED00000836` (401 D pe NETOPIA 3.021,97 + 4111 C pe Robert Dinca 60,83).
- **Compensarea prin notă**: 38 note sting 79 partide (54 FCT, 25 FCL), Σ
  74.864,32. **Niciuna nu are și rol Client, și rol Furnizor**, și niciuna n-are
  doi parteneri (§B5.5) — deci compensarea „401 = 4111 pe același partener” nu
  apare în datele de import; cele 38 sunt ajustări unilaterale.
- **Lanțul avans→factură→regularizare: 0 instanțe.** Niciun document nu apare
  în ambele roluri (§B5.6: `Incasare` 22.748 stingători, `Plata` 2.259,
  `NotaContabila` 38 — 0 dintre ele stinse de altcineva).
- Un stingător acoperă 1 partidă în 22.045 de cazuri, 2 în 1.257, 3 în 491,
  până la 335 (`02-out/12-a-politici.txt` §A.7). Plățile/încasările își consumă
  plafonul integral în 24.540 de cazuri și parțial în 467 (§A.8); documentele
  stinse sunt stinse integral în 41.109 de cazuri și parțial în 2.195 (§A.9).
- Toate cele 44.448 împerecheri au `Autogenerat = false` (§A.6): importul 1C le
  scrie direct, calea `CreeazaAutomataLaOperare` nu a produs niciuna.

### B6. Cele trei definiții candidate, la 31.12.2025 (`02-sql/08-b6-definitii.sql`, `10-b6b…`, `11-b6c…`)

Convenție: restul e SEMNAT, în sensul natural al contului (rol Client ⇒
debitor +, rol Furnizor ⇒ creditor +). Împerecherile se aplică indiferent de
`Data` (altfel artefactul le-ar scoate pe toate): efect −Suma pe partida
stinsă, +Suma pe a stingătorului.

| definiție | partide cu rest ≠ 0 | Σ rest debitor | Σ rest creditor | Σ rest |
|---|---|---|---|---|
| A = documentul | 63.336 | 116.832.185,65 | −86.145.011,26 | 30.687.174,39 |
| B = document × cont | 66.658 | 129.329.372,46 | −124.227.573,19 | **5.101.799,27** |
| C = lanțul conex | 33.032 | 75.250.870,77 | −44.563.696,38 | 30.687.174,39 |

**Testul care le separă** (§B6c.3): soldul REAL al conturilor de terț la
31.12.2025 este **5.101.799,27** (401: 4.656.583,59 C; 4111: 649.613,03 D; 419:
−141.017,71; restul sub 65 k). **Numai definiția B se închide pe el.** A și C
pierd exact 25.585.375,12 — efectul celor 1.179 împerecheri al căror document
stins n-are partidă (§B3): jumătatea de pe stingător se postează, cealaltă n-are
unde ateriza. Sub B jumătatea aterizează, dar pe un cont pe care documentul n-a
postat niciodată: **1.085 partide „inventate”, −25.585.375,12** (§B6.4). E
aceeași ruptură văzută din două părți.

**Coincidența cu `PartideDeschise` 12/2025** — dar mai întâi artefactul:
`MaterializeazaPartide` taie asignările pe `Imperechere.Data <= 2025-12-31`, iar
toate sunt 2026-09-18 ⇒ **restul lui F0 la 12/2025 este `TotalStingere` pe toate
cele 201.046 de rânduri, la cent** (§B6b.0: 201.046/201.046, Σ 1.034.831.116,87).
Comparația literală cerută de spec:

| definiție | rânduri F0 | rânduri def. | comune (DocumentId) | rest identic | doar în def. | doar în F0 | Σ|Δrest| |
|---|---|---|---|---|---|---|---|
| A | 201.046 | 63.336 | 63.275 | 39.044 | 61 | 137.771 | 878.891.184,84 |
| B | 201.046 | 66.658 | 64.360 | 39.047 | 61 | 136.686 | 853.305.809,72 |
| C | 201.046 | 33.032 | 37.063* | 24.646 | 51 | 163.983 | 921.696.876,85 |

\* sub C o partidă acoperă două documente (FCT + NIR), de aceea „comune” > „rânduri def.”.

Contra unui F0 **onest** (`TotalStingere − Asignat`, fără tăierea pe dată:
135.401 partide, Σ 699.711.205,35 — §B6b.1/2):

| definiție | comune | rest identic | doar în def. | doar în F0 |
|---|---|---|---|---|
| A | 47.852 | **39.832** | 15.484 | 87.549 |
| B | 48.934 | **39.832** | 15.487 | 86.467 |
| C | 35.283 | 25.597 | 1.831 | 100.118 |

Pe tipuri (§B6b.3), A coincide cu F0 acolo unde documentul postează exact ce
datorează: NIR 17.803/17.803, Incasare 8.810/8.810, Plata 517/517, FacturaIesire
11.663 identice + 308 diferite + 1.771 „doar cub” (reziduurile de avans din §B4).
**Diverge masiv pe FacturaIntrare**: 132 identice, 3.223 cu alt rest, 13.672
„doar cub” — cubul îi dă FCT-ului un rest NEGATIV (§B6b.5: 14.627 partide,
−45.515.656,74), fiindcă plata consumă brutul iar factura n-a postat decât TVA-ul.
**Și divergența cea mai mare e cea structurală**: 87.549 de partide F0 (NotaTransfer
45.542, DescarcareGestiune 36.689, BonConsum 545, InchidereTva 12, note 3.457,
FCT 1.215 …) pe documente care nu postează NIMIC pe un cont de terț —
515.747.871,20 de „rest” care nu e datorie față de nimeni (§B6c.4).

**Dubla partidă a lanțului** (§B6c.5): pentru cele 17.814 perechi, F0 ține
83.593.304,48 pe NIR-uri (niciunul împerecheat) + 25.968.105,96 pe facturi =
109.561.410,44, pentru o datorie pe care cubul o vede la 43.701.583,53 sub C. Sub
C, 13.732 din 17.814 lanțuri sunt STINSE complet (rest 0) în timp ce F0 le vede
cu rest (§B6b.4: 13.721 „cub stins / F0 nu”, 0 invers).

**Faptul, nu recomandarea**: definiția care coincide cel mai des cu F0 pe
DocumentId e **A ≡ B (39.832 partide identice)**; definiția care se închide pe
soldul contului e **B (singura)**; definiția care reproduce datoria economică
față de furnizor e **C** (o partidă per lanț, 13.732 lanțuri stinse), dar ea nu
se închide pe sold și e cea mai departe de F0 în număr de rânduri.

### B7. Împerecherea ca tranzacție (`02-sql/09-b7-imperechere.sql`)

Criteriu de „reprezentabilă fără rupere”: (1) partida stinsă are postare pe
contul de referință al stingătorului, (2) suma cumulată consumată nu depășește
|restul propriu| al NICIUNEIA dintre cele două partide.

| definiție | total | contul OK | **fără rupere** | rupt: cont inexistent | rupt: suma |
|---|---|---|---|---|---|
| A (document) | 44.448 | 43.269 | **26.767 (60 %)** | 1.179 | 16.502 |
| B (document × cont) | 44.448 | 43.269 | **26.767 (60 %)** | 1.179 | 16.502 |
| C (lanț conex) | 44.448 | **44.448** | **41.838 (94 %)** | **0** | 2.610 |

Categoriile de rupere:

1. **Partida stinsă nu există** (1.179 sub A/B, 0 sub C): facturile cu taxare
   inversă, a căror datorie stă pe NIR. Sub C dispare complet.
2. **Suma depășește restul propriu** — sub A (§B7.3): `FacturaIntrare` 14.341
   documente, depășire 42.422.583,22 (plata consumă brutul, factura a postat
   doar TVA-ul); `FacturaIesire` 1.766, 8.234.784,18 (avansul pe 419 scade
   partida sub suma încasată); `NotaContabila` 38, 74.864,32 (`TotalStingere`
   al notei nu are legătură cu ce postează). Sub C rămân 2.533 de lanțuri,
   9.188.808,46 (§B7.4) — aproape numai avansul de client.
3. **Latura** (§B7.1, raportată separat fiindcă e universală, nu excepțională):
   43.237 din 44.448 leagă documente aflate pe laturi OPUSE. Regula „două
   postări pe același (Cont, Latura)” obligă piciorul de pe partida stinsă să
   stea pe latura stingătorului, deci `Valoare` devine semnată pe o latură pe
   care partida n-a postat. Ca sumă per cont se compensează (de aceea gate-ul
   trece), dar orice rulaj brut per partidă încetează să fie o sumă. Doar 32 de
   împerecheri au ambele părți pe aceeași latură.

---

## C. Ce n-am putut proba

1. **Valuta și partidele în valută**: 0 documente cu `Valuta` sau `Curs`
   completate pe Flax; nicio diferență de curs, nicio partidă multi-valută.
2. **Decontul cu angajatul**: 0 documente `Decont`, 0 postări pe 542. Regulile
   de contare și de TVA există în seed, dar lanțul avans→decont→regularizare
   n-are nicio instanță.
3. **Avansul de furnizor (409x)**: 8 documente în total, niciunul într-un lanț
   de stingere; contul 409 nu e folosit deloc.
4. **Compensarea reală partener–partener** (401 = 4111 pe același partener): 0
   instanțe; cele 38 de note care sting sunt unilaterale.
5. **Lanțul avans→factură→regularizare prin împerechere**: 0 instanțe (niciun
   document în ambele roluri).
6. **Stornoul unui document împerecheat și rândul invers**: 0 documente stornate,
   0 rânduri de registru cu `Storno`, 0 împerecheri cu `InverseazaId` sau sumă
   negativă. Efectul stornoului pe partide e citit doar din cod (§A1).
7. **Corecția legată (88h)**: 0 documente cu `CorecteazaId`.
8. **Data reală a împerecherii**: artefact (toate 2026-09-18). Toate cifrele de
   partidă „la o dată” sunt de fapt „la infinit”; `PartideDeschise` 12/2025 e
   inutilizabilă ca referință de comparație (§B6b.0) — de semnalat conectorului
   1C, cum cere deja `nucleu-fizica.md` §5.
9. **DVI și imobilizările**: 0 documente `Dvi`, `RegistruImobilizari` gol — deci
   niciun cont de terț adus de ele (446, furnizori de imobilizări).
10. **`TotalStingere` pe documentele fără linii de creanță**: regula
    `LiniiCreanta` e suprascrisă o singură dată (RDC); nu am putut proba ce ar
    trebui să fie „restul” unui NotaTransfer sau al unei DescarcareGestiune —
    azi e pur și simplu Σ liniilor, 200 M de rest fără datornic.
