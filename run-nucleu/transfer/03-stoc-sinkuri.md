# Pas 3 (transferul), explorarea 3: stocul în cub — rândul unificat cu 3xx, `TipStoc`, sink-urile, FCT contra NIR

Agent read-only, 2026-09-19. Cod: `nou/` neatins, niciun build. Cifre: baza
`Atlas.Conta.Nucleu.Fizica.x1` din docker `contapal-postgres-1` (profil
**privat**, `SetariProfil.Profil = 1`), schema `public` = registrele de azi,
`f2."Postare"` = cubul pasului 2. Ajutătoarele mele stau EXCLUSIV în schema
`tr3` (`tr3.stoc`, `tr3.cont` — copii denormalizate cu indexi pe `DetaliuId`);
`public`/`f2`/`cub` neatinse, Flax neatins, `.x10` nefolosit.

SQL-urile: `run-nucleu/transfer/03-sql/*.sql`. Ieșirile brute:
`run-nucleu/transfer/03-out/*.txt`.

Ancore de volum (`03-sql/b0-baza.sql`): `RegistruStoc` 283.498,
`RegistruContabil` 304.382, `Documente` 205.186, `DocumentDetalii` 337.596,
`f2."Postare"` 1.162.622. `GCRecord` e `0` pe TOATE rândurile ambelor registre
(niciun rând șters logic), deci nicio interogare de mai jos nu filtrează pe el.

---

## A. Din cod

### A1. `TipStoc`: enumul și TOATE locurile care îl citesc sau îl scriu

Enumul, `BusinessObjects/Comun/Enums.cs:10-18` — șapte valori:
`Magazie = 1`, `Consum = 2`, `Folosinta = 3`, `Custodie = 4`, `Marfuri = 5`,
`Gratuit = 6`, `ProductieNeterminata = 7`.

**Coloană persistată** (patru tabele):

| Loc | Linie | Rol |
|---|---|---|
| `RegistruStoc.TipStoc` | `BusinessObjects/Registre/Registre.cs:23` | a treia componentă a cheii de sold |
| `SoldPerioadaStoc.TipStoc` | `BusinessObjects/Registre/SolduriPerioada.cs:75` (index unic `:59`, `BackOfficeDbContext.cs:312`) | snapshotul pe aceeași cheie |
| `RegulaStoc.TipStoc` | `BusinessObjects/Politici/Politici.cs:64` | IEȘIREA politicii de stoc |
| `PoliticaMiscareSaft.TipStoc` | `BusinessObjects/Politici/Politici.cs:622` | parte din CHEIA politicii SAF-T (indexi unici `BackOfficeDbContext.cs:530`, `:538`) |

**Cine DECIDE ceva pe el** (nu doar îl transportă):

- `Motor/MotorOperare.cs:471` — cheia mișcării o dă REGULA
  (`new CheieStoc(d.LotId.Value, repartitorId, regula.TipStoc)`); `:355` scrie
  `rand.TipStoc = miscare.Cheie.TipStoc`; `:644`/`:651` copiază `TipStoc` pe
  rândul invers de storno; `:599` reconstruiește cheile la anulare.
- `Motor/StocService.cs:7` `CheieStoc(LotId, RepartitorId, TipStoc)` — cheia
  de sold; `:63`, `:207`, `:228`, `:263` grupează/filtrează pe ea; `:260`,
  `:292` — alocarea FIFO primește `tipStoc` ca parametru.
- `Motor/SolduriService.cs:258-446` — snapshotul și SQL-ul brut de
  materializare grupează pe `("LotId","RepartitorId","TipStoc")`.
- `Motor/DescarcareService.cs:95` — generatorul DSC ia `TipStoc`-ul din prima
  regulă potrivită și îl duce în `StocService.Sold` / `AlocaFifoTolerant`.
- `BusinessObjects/Documente/FacturaIesire.cs:141` — refuzul „lot fără sold în
  gestiunea de descărcare" interoghează cheia cu `potrivit.Reguli[0].TipStoc`.
- `Saft/SaftProiectii.cs:1679-1687` — `Potriveste(tipId, tipStoc, semnRegula)`
  alege codul de mișcare; `raportate` = valorile de `TipStoc` care apar în
  politici CU COD, și ele decid ce intră în `PhysicalStock` (`:1832`, `:2254`,
  `:2527`, `:2618`).
- `Motor/GardianEditare.cs:814-832` — mesajele de refuz pe politica SAF-T.
- `Motor/VerificareProfilService.cs:138`, `:214` — eticheta rândului de politică.
- `Motor/Potrivire.cs:21` — `RegulaStocFapt` îl poartă; `Motor/Fapte.cs:22-24`
  îl proiectează din tabelă.

**Transport / afișare (nicio decizie):** `Proiectii/StocProiectii.cs:25-83`
(enum → string ÎN SQL, prin `CASE`), `Saft/SaftDto.cs:313`, `:464`,
`Saft/SaftProiectii.cs:1927`, `:1942`, `:1967`, `:2103`,
`Api/Politici/ExplicaApply.cs:232`, `:244`, `Api/Politici/ExplicaDtos.cs:64`,
`WebApi/API/Conta/SoldStocController.cs:9`.

**Seed-uri:** `DatabaseUpdate/ContaSeeder.cs:221-233`, `:716`;
`ProfilPrivat.cs:869-954`, `:1017` (tabelul SAF-T + `MagazieSiMarfuri`);
`ProfilBugetar.cs:288-389` (bugetarul e SINGURUL care seed-ează `Gratuit`,
`Folosinta`, `Custodie` — pe NIR și LDI).

**Client React:** `felii/politici/ReguliStoc.tsx:23,33,66`,
`felii/politici/PoliticiMiscareSaft.tsx:33,75`, `felii/stoc/SoldStoc.tsx:7-40`,
`felii/tva/Saft.tsx:742,744,868`, `felii/politici/Explica.tsx:454`,
`generated/api-types.ts:12708` (uniunea de string-uri). Clientul NU decide
nimic pe el: îl afișează prin `labelEnum` și îl trimite ca string.

**Unelte:** `tools/Import1C/Catalog.cs:23`
(`Registru => ClasaCod == "MF" ? Marfuri : Magazie` — REGULA conectorului),
`Alocare.cs:159-171`, `Deschidere.cs:478-502`, `HandlereStoc.cs:115,369,902`,
`HandlerFactura.cs:223`, `HandlereVanzare.cs:778`, `HandlerAsamblare.cs:285`,
`ReconciliereLuna.cs:737` (`RegistreComparabile = [Magazie, Marfuri]` — ce se
compară cu 1C), `Sabotaj.cs:250,313`, `Saft1C.cs:313,327`,
`Migrare/Program.cs:402-423` (maparea legacy `G/OF/MF/MC` → enum).
`tools/ModelCheck/Program.cs`: 197 apariții, toate în fixture-uri și aserturi
pe `CheieStoc`/`RegulaStoc`/politica SAF-T — nicio decizie de producție.

### A2. Politicile de stoc, de contare, de mișcare SAF-T; cum se decide contul 3xx

**`RegulaStoc`** (`Politici.cs:50-66`): cheia `TipDocument × Latura × Clasă?` →
`(TipStoc, Semn)`. Clasa `null` = generic; regula specifică bate genericul
(`Motor/Potrivire.cs`). Pe baza asta, 19 rânduri, toate `DinSeed`
(`03-out/a-politici.txt`):

```
ASM Pred +1 (generic→Magazie, MF→Marfuri)
BTR Pred −1 / Primitor +1 (ambele registre)
BCS Pred −1 (Magazie/Marfuri), Primitor +1 (generic→Consum)
DSC Pred −1   LDI Pred +1   NIR Primitor +1   RDC Primitor −1   RLF Pred +1
```

Semnul rândului de registru vine din regulă, nu din frunză:
`MotorOperare.cs:359` `rand.Valoare = regula.Semn * detaliu.Valoare`, `:471`
`regula.Semn * d.Cantitate`. Repartitorul vine din latura regulii
(`MotorOperare.cs:469`).

**`RegulaContare`** (`Politici.cs:74-111`): cheia
`TipDocument × TipMaterial? × NaturaFiltru? × SemnFiltru?` → conturi D/C prin
`SursaCont` (`Enums.cs:116-121`: `Explicit`, `TipMaterial`,
`RepartitorPredator`, `RepartitorPrimitor`) + trei seturi de dimensiuni. Fără
regulă potrivită, linia nu postează nimic (`MotorOperare.cs:167-169`) — exact
așa se împarte lanțul FCT/NIR fără dublă postare.

**Contul de stoc (371 contra 3xx) se decide din PRODUS, nu din gestiune și nu
dintr-un rând de politică propriu.** Lanțul e
`Lot.ProdusId → Produs.TipMaterialId → TipMaterial.ContImplicitId`, consumat
prin `SursaCont.TipMaterial` (`Nomenclatoare/ClasaTip.cs:52-56`). Regulile de
recepție și de descărcare nu numesc niciun cont de stoc:

```
NIR/Stoc : SursaContDebit = TipMaterial,  SursaContCredit = RepartitorPredator (fallback 401)
DSC, BCS : ContDebit = 6xx explicit per TipMaterial, SursaContCredit = TipMaterial
```

(`ProfilPrivat.cs:979-985`, `:1015-1018`; rândurile reale în
`03-out/b6b7-custodie-economie.txt` §B5.11 — 43 de rânduri de contare pe
tipurile de stoc). Gestiunea nu intră NICIODATĂ în alegerea contului. `TipStoc`
nu intră nici el: `Marfuri` corespunde clasei `MF`, al cărei singur
`TipMaterial` are `ContImplicit = 371`; `Magazie` e restul.

**`PoliticaMiscareSaft`** (`Politici.cs:602-646`): cheia
`TipDocument × TipStoc × Semn?` → `(CodMiscare, RolTert, Motiv)`, cu doi indexi
unici filtrați (`BackOfficeDbContext.cs:530`, `:538`). 21 de rânduri seed pe
baza asta (`03-out/b4-miscaresaft.txt` §B4.1): NIR 10/Furnizor, BTR 80,
BCS −1 → 70 și BCS `Consum +1` → FĂRĂ cod, cu motiv („Consumul pe responsabil
nu e stoc în magazie"), DSC 30/Client, LDI 110 plus / 120 minus,
ASM +1 → 20 și −1 → 70, RLF 50/Furnizor, RDC 40/Client. Potrivirea se face pe
semnul REGULII, `(Storno ? −1 : +1) × sign(Cantitate)`
(`SaftProiectii.cs:1596-1603`).

**„Gestiunile de patrimoniu" ale `PhysicalStock` nu există ca mulțime de
gestiuni.** `SaftProiectii.cs:1687` calculează
`raportate = politici.Where(p => p.Cod != null).Select(p => p.TipStoc)`, iar
`PhysicalStock` declară fiecare cheie `(repartitor × lot)` de pe acele
`TipStoc`-uri (`:1827-1835`). Filtrul e pe REGISTRU, nu pe repartitor. Pe baza
asta mulțimea raportată e `{Magazie, Marfuri}`; `Consum` cade afară fiindcă
rândul lui de politică n-are cod.

### A3. Fluxul unei linii de stoc prin motor

`MotorOperare.CalculeazaSiValideaza` → `PregatesteOperare` al frunzei →
`PotrivesteReguliStoc` (tolerant, `:454-475`) → `StocService.AplicaValoareIesire`
(`StocService.cs:103-125`, regula D18-D2 de golire) → validare → în
`Opereaza`:

| Pas | Linie | Ce scrie |
|---|---|---|
| finalizarea loturilor născute de linii | `MotorOperare.cs:333-350` | `Lot.PretUnitar = Valoare/Cantitate`, `Lot.Data = DataInregistrare` |
| rândul de stoc | `:352-362` | `Data = DataInregistrare`, `TipStoc`/`LotId`/`RepartitorId` din cheia REGULII, `Cantitate = Semn × cant`, `Valoare = Semn × detaliu.Valoare`, `Document`, `Detaliu` |
| rândurile contabile | `:364-374` | pereche `ContDebitId`/`ContCreditId` + `Valoare` + două seturi de 8 dimensiuni, `Document`, `Detaliu` |
| rândurile de TVA | `:376-394` | `Data = doc.Data` (fizică), perioada separat |

**Singura cheie comună între cele două registre e `DetaliuId`** (plus
`DocumentId`). `RegistruStoc` n-are FK spre cont; `RegistruContabil` n-are
`LotId`. Produsul apare pe rândul contabil doar ca DIMENSIUNE
(`DimensiuniDebit_MaterialId` / `DimensiuniCredit_MaterialId`), setată din
`Lot.ProdusId` (`MotorOperare.cs:185-198`) — deci „Material" e derivabil, dar
nu e cheie.

**Când rândul de stoc n-are pereche contabilă** (cifrele în B2):
`NotaTransfer` (nicio `RegulaContare` la plan sintetic — `ProfilPrivat.cs:958`,
`ProfilBugetar.cs:281-284`), `Asamblare` (nicio regulă de contare; documentul
își balansează valoarea intern, `Asamblare.cs:153-159`), și rândurile de
DESCHIDERE scrise de import (`DetaliuId IS NULL`,
`tools/Import1C/Deschidere.cs:478-502`).

**Când un rând contabil pe 3xx n-are rând de stoc:** DA, există — 3.370 de
picioare (B2.9). Aproape toate sunt `NotaContabila` (3.365), plus 4 de
deschidere și UNA de `FacturaIntrare`.

**Reevaluarea / corecția de preț pe lot NU există azi.** Singurul „Reevaluare"
din model e `FelMiscareImobilizare.Reevaluare` (`Enums.cs:555`,
`AmortizareService.cs:68`), pe fișa de imobilizare. DVI (`Documente/Dvi.cs`)
postează DOAR taxa: „`Valoare` = valoarea în vamă, `ValoareTva` = taxa
declarată; nimic nu postează valoarea, doar taxa" (`Dvi.cs:10-12`), nicio
distribuție pe loturi. Prețul lotului se fixează o singură dată, la operarea
documentului care l-a născut (`MotorOperare.cs:344`); singura mișcare de
valoare fără cantitate proprie e regula de golire D18-D2, care mută reziduul
pe contrapartidă, nu pe lot.

### A4. Sink-urile

- **Consum (`TipStoc = 2`)**: singurul sink cu rânduri vii, și numai pe
  **BonConsum** — `ProfilPrivat.cs:1017`
  `SeedReguliStoc(bcs, Primitor, +1, (null, TipStoc.Consum))`. Primitorul e o
  `UnitateInterna` (SEDIU, `Calitati |= LocConsum`, `ContaSeeder.cs:813-829`),
  nu o gestiune. Contul: nota e `6xx = 3xx` derivată din simbolul contului de
  stoc (`SeedContare6xxDin3xx`, cu excepțiile 607/711/608), o SINGURĂ notă
  pentru ambele rânduri de stoc ale liniei. Codul SAF-T: NICIUNUL, cu motiv.
- **DSC → 6xx**: descărcarea NU scrie sink. Un singur rând de stoc (−1 pe
  predator = gestiunea), nota `607 = 371`. Idem RDC și RLF.
- **Folosință (3), Gratuit (6), Custodie (4), Producție neterminată (7)**:
  ZERO rânduri în registru pe baza asta (B6) și zero reguli în seed-ul privat.
  `Gratuit`/`Folosinta`/`Custodie` există doar în seed-ul BUGETAR
  (`ProfilBugetar.cs:335-336`, `:389`, pe clasele `G`/`OF`/`MC`);
  `ProductieNeterminata` nu e seed-at nicăieri.
- **Asamblare (ASM)**: intrare ȘI ieșire în același document, dar printr-o
  SINGURĂ regulă de stoc (`Predator +1`, `ProfilPrivat.cs:1229`) — semnul vine
  din cantitatea semnată de frunză, `Asamblare.cs:56-69` (`Consum` ⇒ cantitate
  negativă evaluată la prețul lotului; `Produs` ⇒ pozitivă la `PretEvaluare`).
  Niciun rând de contare ⇒ zero note.
- **Inventar (LDI)**: o singură regulă `Predator +1`, direcția din semn;
  contarea se desparte prin `SemnFiltru` (minus = `6xx = 3xx`, plus =
  `3xx = 7588` la privat, `3xx = 791` la bugetar).

### A5. FCT contra NIR

**Ce poartă linia FCT** (`FacturaIntrareDetaliu`,
`Documente/FacturaIntrare.cs:165-218`): `PretUnitar` (scara preț 18,6),
`ProdusId`, `DataExpirare`, `LotFabricatie`, `CodCpv`, cele patru dimensiuni
bugetare — plus, din bază, `Cantitate`, `Valoare`, `TipTvaId`, `ValoareTva`,
`LotId`. **Nu are `LinieSursaId`**: coloana e pe `DocumentDetalii` (TPH) dar o
declară `DescarcareGestiuneDetaliu` (`DescarcareGestiune.cs:118-119`, spre
linia de FCL) și linia de PIF (`Imobilizari.cs:99`).

**Ce poartă linia NIR** (`NirDetaliu`, `DocumenteGestiune.cs:137-175`):
`ProdusId`, `PretUnitar`, atributele de lot, dimensiunile — dar pe clona conexă
`PretUnitar` rămâne 0 și e IGNORAT: valoarea vine din prețul lotului
(`NIR.PregatesteOperare`, `DocumenteGestiune.cs:42-50`).

**Cine din cine:** `PoliticaConex` (`Politici.cs:241-256`) cu un singur rând
seed, `FCT → NIR`, `InverseazaLaturi = false`, `NaturaFiltru = Stoc`
(`ProfilPrivat.cs:975-980`). `MotorOperare.GenereazaConex`
(`MotorOperare.cs:534-573`) clonează, la operarea FCT, DOAR liniile de natura
`Stoc`, copiind `TipMaterialId`, `LotId`, `Cantitate`, `Valoare`, `TipTvaId`,
`Angajament` și dimensiunile — dar **NU `ValoareTva`** (`:565-567`: „NIR-ul
duce netul, factura duce rândurile 4426"). Legătura rămasă e la nivel de
DOCUMENT (`Document.DocumentSursaId` + `Autogenerat`), nu de linie; la nivel de
linie urma e `Lot.LinieIntrareId` (lotul născut de linia FCT).

**Ce postează fiecare:** FCT postează liniile NON-stoc (reguli pe
`NaturaFiltru = Serviciu|Cheltuiala|Imobilizare`, credit 401/404) + rândul
`4426` per linie, INCLUSIV pentru liniile de stoc (`MotorOperare.cs:213-219`:
„liniile de stoc ale FCT nu au regulă de contare, dar TVA-ul lor deductibil se
postează pe factură"). NIR postează recepția `3xx = 401` la net și singurul
`+1` din registrul de stoc. FCT de servicii = factură fără nicio linie de
natura `Stoc` ⇒ `GenereazaConex` întoarce `null` (`:539-540`), niciun NIR.

**DVI:** legătura n→m e `DviFactura` (`Dvi.cs:101`), tabelă separată
`DviFacturi`; DVI validează că facturile legate sunt Operate și postează doar
`4426/446`. Nicio atingere de lot.

**SAF-T:** `SaftProiectii.cs:890-949` construiește `receptiePeLinie` prin trei
interogări pe seturi — loturile născute de liniile lunii
(`Lot.LinieIntrareId`), recepțiile lor (`RegistruStoc` cu `Cantitate > 0` și
`!Storno`), rândurile contabile ale liniilor de recepție. `:1112-1118`: pentru
linia de stoc a FCT, `InvoiceLine.AccountID` se ia de pe DEBITUL rândului de
recepție al NIR-ului, iar dimensiunile din ACEL rând — contul nu se inventează
din `TipMaterial.ContImplicit`. `:1120-1130`: fără contrapartidă ȘI fără
recepție (NIR neoperat), linia iese din fișier ca `FaraContrapartida`.
`:1158-1164`: cantitatea și prețul liniei de factură se iau de pe LINIA FCT
(`FacturaIntrareDetaliu.PretUnitar`, cules la `:865-871`), nu de pe NIR —
NIR-ul n-are preț.

---

## B. Cifrele, pe `Fizica.x1`

### B1. `RegistruStoc` pe (tip document, `TipStoc`, semn)

`03-sql/b1-stoc-pe-tip.sql` → `03-out/b1-stoc-pe-tip.txt`. Semn = `sign(Cantitate)`.
Zero rânduri cu `Storno = true` în toată tabela.

| Tip | TipStoc | Semn | Rânduri | Σ cantitate | Σ valoare |
|---|---|---|---|---|---|
| Asamblare | 1 | −1 | 41 | −850,000 | −10.948,15 |
| Asamblare | 1 | +1 | 511 | 2.167,480 | 99.660,58 |
| Asamblare | 5 | −1 | 3.181 | −21.773,174 | −7.507.808,61 |
| Asamblare | 5 | +1 | 961 | 6.784,000 | 7.419.096,18 |
| BonConsum | 1 | −1 | 990 | −17.072,480 | −182.837,04 |
| BonConsum | 2 | +1 | 1.471 | 18.602,480 | 340.618,38 |
| BonConsum | 5 | −1 | 481 | −1.530,000 | −157.781,34 |
| DescarcareGestiune | 5 | −1 | 62.063 | −182.013,000 | −79.199.898,27 |
| (deschidere) | 1 | +1 | 40 | 1.980,000 | 13.568,62 |
| (deschidere) | 5 | 0 | 130 | 0,000 | 6.267,19 |
| (deschidere) | 5 | +1 | 6.777 | 36.567,480 | 9.501.036,21 |
| ListaDiferenteInventar | 5 | +1 | 22 | 395,000 | 773.417,09 |
| NIR | 1 | +1 | 496 | 14.710,000 | 86.294,48 |
| NIR | 5 | +1 | 33.793 | 188.818,694 | 83.507.010,00 |
| NotaTransfer | 1 | −1 | 339 | −4.856,000 | −83.646,38 |
| NotaTransfer | 1 | +1 | 339 | 4.856,000 | 83.646,38 |
| NotaTransfer | 5 | −1 | 84.688 | −265.813,000 | −121.010.658,29 |
| NotaTransfer | 5 | +1 | 84.688 | 265.813,000 | 121.010.658,29 |
| ReturClient | 5 | +1 | 2.010 | 6.552,000 | 3.616.498,74 |
| ReturFurnizor | 1 | −1 | 2 | −6,000 | −657,84 |
| ReturFurnizor | 5 | −1 | 475 | −3.264,000 | −2.614.827,91 |

Trei observații de fapt: (a) `ListaDiferenteInventar` are DOAR plusuri;
(b) `ReturClient` intră pozitiv iar nota lui e negativă (convenția
`PastreazaSemn`); (c) 130 de rânduri de deschidere au cantitate 0 și valoare
6.267,19 lei — reziduu valoric fără cantitate, cunoscut.

### B2. Perechea stoc ↔ contabil

Cheia: `DetaliuId` (singura comună). `03-sql/b2-pereche.sql`,
`b2b-valori.sql`, `b2c-invers.sql`, `b2d-invers-total.sql`.

Acoperirea cheii: 276.551 rânduri de stoc au `DetaliuId`, 6.947 nu (toate
deschideri); 304.318 rânduri contabile au `DetaliuId`, 64 nu (deschiderile
contabile, singurele cu `NumarNota = 'DESCHIDERE'`).

Cardinalitatea pe `DetaliuId` (B2.1):

| rânduri stoc | rânduri contabile | linii |
|---|---|---|
| 0 | 1 | 91.428 |
| 0 | 2 | 56.041 |
| 1 | 0 | 4.694 |
| 1 | 1 | 98.385 |
| 1 | 2 | 476 |
| 2 | 0 | 85.027 |
| 2 | 1 | 1.471 |

**Nu există niciun `1:n` cu n ≥ 3, și nicio linie cu ambele numere > 1.**

**1:1 — valoarea (B2.3): 98.385 perechi, TOATE cu `|stoc.Valoare| =
|contabil.Valoare|`, diferență totală 0,00.** Pe toate cele șase combinații
(DSC/5: 62.063, NIR/5: 33.793, ReturClient/5: 2.010, NIR/1: 496, LDI/5: 22,
ReturFurnizor/5: 1). Zero exemple de valoare diferită — deci nici TVA, nici
rotunjirea nu rup perechea.

**1:2 (B2.8): 476 de linii, toate `ReturFurnizor`** (474 pe TipStoc 5, 2 pe
TipStoc 1); a doua notă e TVA-ul (`371/401 + 4426/401`, sau
`371/401 + 4426/4427` la taxare inversă). Toate 476 au o notă cu `|valoare|`
egală cu a rândului de stoc.

**2:1 (B2.5): 1.471 de linii, toate `BonConsum`.** Suma algebrică a celor două
rânduri de stoc e 0,00 pe fiecare linie (Magazie/Mărfuri iese, Consum intră),
iar nota (`6xx = 3xx`) e egală cu maximul absolut al celor două, pe toate
1.471. Σ notă = 340.618,38.

**Fără pereche (B2.2), 181.695 de rânduri de stoc:**

| Tip | TipStoc | Rânduri | Σ valoare |
|---|---|---|---|
| NotaTransfer | 5 | 169.376 | 0,00 |
| (deschidere) | 5 | 6.907 | 9.507.303,40 |
| Asamblare | 5 | 4.142 | −88.712,43 |
| NotaTransfer | 1 | 678 | 0,00 |
| Asamblare | 1 | 552 | 88.712,43 |
| (deschidere) | 1 | 40 | 13.568,62 |

**Invers — picioare contabile pe 3xx fără rând de stoc (B2.7/B2.9): 3.370 din
103.702.**

| Tip | picioare 3xx | fără rând de stoc |
|---|---|---|
| NotaContabila | 3.365 | **3.365** |
| (deschidere) | 4 | **4** |
| FacturaIntrare | 1 | **1** |
| NIR | 34.289 | 0 |
| DescarcareGestiune | 62.063 | 0 |
| ReturClient | 2.010 | 0 |
| ReturFurnizor | 477 | 0 |
| BonConsum | 1.471 | 0 |
| ListaDiferenteInventar | 22 | 0 |

Corespondențele notelor contabile pe 3xx (B2.11): `891 = 371` 1.420 rânduri
(închiderile de lună, Σ 0,00), `607 = 371` 626, `401 = 371` 354 (Σ 155.340,80),
`3028 = 371` 345 (Σ 85.083,83 — puntea de import), `371 = 401` 180, restul sub 15.

**Surpriză raportată:** o linie de `FacturaIntrare` postează direct
`371 = 401`, 20,00 lei — factura `CCN22146` din 2025-05-29
(`DetaliuId 01a0b4b5-ecbe-7357-994e-34ca8f3a3428`), fără niciun rând de stoc.
Contrazice regula „recepția contează pe NIR": linia are un `TipMaterial` cu
`ContImplicit = 371` dar clasa lui NU e de natura `Stoc` (`TER` / `S371`,
„Punte de stoc 371 (tehnic import)", `03-out/a-politici.txt`), deci n-a trecut
prin conex și și-a găsit regula de contare pe `NaturaFiltru = Serviciu`.

### B3. `TipStoc` contra `(Cont al laturii de valoare, Repartitor)`

`03-sql/b3-tipstoc.sql` → `03-out/b3-tipstoc.txt`. Două definiții ale contului,
fiindcă niciuna nu acoperă singură tot: (a) contul de stoc al PRODUSULUI
(`Lot → Produs → TipMaterial.ContImplicit`, acoperire 100 %); (b) contul
POSTAT, luat de pe rândul contabil cu același `DetaliuId`, pe latura semnului.

**Răspunsul cu cifră: NU există nicio combinație `(Cont, Repartitor)` cu două
valori de `TipStoc`. Zero rânduri, pe AMBELE definiții** (B3.2 și B3.4 întorc
0 rânduri). Deci `TipStoc` E derivabil din `(Cont, Repartitor)` pe toate cele
283.498 de rânduri.

Structura (B3.1), 130 de combinații:

| TipStoc | Conturi | Clasa repartitorului |
|---|---|---|
| 1 Magazie | 3028, 3024, 303, 381, 3021 | `Gestiune` (57 de gestiuni) |
| 2 Consum | 3028 (863), 371 (481), 3024 (64), 303 (33), 381 (30) | `UnitateInterna` SEDIU, EXCLUSIV |
| 5 Marfuri | 371, exclusiv | `Gestiune` (68 de gestiuni) |

Contul singur NU ajunge (371 apare și pe `Marfuri`, și pe `Consum`; 3028 și pe
`Magazie`, și pe `Consum`); repartitorul singur nu ajunge (69 de repartitori
poartă 1 și 5 simultan). Perechea ajunge, fiindcă `Consum` e ancorat pe
singurul repartitor care nu e `Gestiune`. `TipStoc ∈ {3,4,6,7}`: zero rânduri.

### B4. `PoliticaMiscareSaft`: cheia de azi contra `(tip × Cont × semn × rol terț)`

`03-sql/b4-miscaresaft.sql` → `03-out/b4-miscaresaft.txt`. Semnul e cel al
REGULII, `(Storno ? −1 : +1) × sign(Cantitate)`.

**Cheia nouă e injectivă spre codul de mișcare: ZERO coliziuni** (B4.3 întoarce
0 rânduri) — nicio combinație `(tip document, cont, semn)` nu poartă două
coduri distincte, și nici două roluri de terț distincte. Cheia nouă e mai FINĂ
decât cea de azi: 39 de chei distincte pe rândurile reale contra 18 azi (B4.4),
pentru că un `TipStoc` se sparge pe conturile lui (BCS `Magazie −1` devine
patru chei: 3024, 3028, 303, 381).

Cazul care ar fi putut să se rupă nu se rupe: pe BonConsum, `Marfuri −1` și
`Consum +1` folosesc AMÂNDOUĂ contul 371 (481 rânduri fiecare), dar semnul le
separă — `(BCS, 371, −1) → 70`, `(BCS, 371, +1) → fără cod`. Idem
3028/3024/303/381 între `Magazie −1` și `Consum +1`. `NotaTransfer` are același
cont și același cod (80) pe ambele semne, ca azi (politică cu `Semn = null`).

### B5. FCT/NIR

`03-sql/b5-fct-nir.sql`, `b5b-fct-nir.sql` → `03-out/b5-fct-nir.txt`,
`b5b-fct-nir.txt`.

| Cifră | Valoare |
|---|---|
| FCT (documente) | 19.035 |
| NIR (documente) | 17.814 |
| NIR cu `DocumentSursa` = FCT | 17.814 (100 %) |
| NIR manuale (fără sursă) | 0 |
| NIR `Autogenerat` | 17.814 |
| FCT fără niciun NIR | 1.221 |
| max NIR per FCT | **1** |
| FCT cu > 1 NIR | **0** |
| NIR care agregă > 1 FCT | **0** |

**`LinieSursaId` e NULL pe TOATE cele 337.596 de linii ale bazei** (B5.1
întoarce 0 rânduri) — inclusiv pe cele 36.696 de descărcări de gestiune, unde
coloana E declarată. Conectorul Import1C nu o populează. Deci legătura
FCT↔NIR la nivel de linie NU trece prin `LinieSursa` nici măcar teoretic pe
baza asta; trece prin **lot**.

Prin lot (`Lot.LinieIntrareId`): 34.289 de loturi născute de
`FacturaIntrareDetaliu`, 1.472 de `AsamblareDetaliu`, 22 de
`ListaDiferenteInventarDetaliu`; 11.371 de loturi (din 47.154) n-au linie de
intrare — loturile de deschidere ale importului.

**Perechea linie FCT ↔ linie NIR prin lot: 34.289, exact una la una**
(34.289 de linii FCT de natura `Stoc`, toate cu lot; 34.289 de linii NIR, toate
cu lot; zero linii FCT de stoc fără linie NIR pe același lot — B5.7).

| | perechi | egale |
|---|---|---|
| cantitate | 34.289 | **34.289 (100 %)** |
| valoare | 34.289 | **34.289 (100 %)** |
| `PretUnitar` | 34.289 | 23 |

Prețul „diferă" pe 34.266 de perechi dintr-un motiv de structură, nu de date:
**`NirDetaliu.PretUnitar` e 0 pe TOATE cele 34.289 de linii de NIR** (B5.9) —
clona conexă nu-l copiază și `NIR.PregatesteOperare` îl ignoră pe loturile
străine. Cele 23 „egale" sunt liniile FCT care au și ele preț 0 (31 în total).

FCT fără NIR (1.221 de facturi): liniile lor sunt exclusiv de natura
`Serviciu` (1.111 linii, 888 de facturi) și `Cheltuiala` (362 de linii, 351 de
facturi) — **nicio linie de natura `Stoc`** (B5.4). Pe total, liniile FCT se
împart 34.289 stoc (toate cu lot) / 2.302 serviciu / 405 cheltuială (B5.12),
niciuna de natura `Imobilizare`.

Ce postează fiecare (B5.10): FCT — 28.504 rânduri `4426 = 401`, 8.284
`4426 = 4427` (taxare inversă), și 2.678 de note de cheltuială/serviciu pe 30
de corespondențe (624, 628, 613, 667, 6051…); NIR — 34.289 de rânduri
`3xx = 401` (371: 33.793, 3028: 399, 3024: 59, 381: 29, 303: 9).

**DVI: `DviFacturi` are 0 rânduri și nu există niciun document `Dvi` în bază**
(B5.8, B5.2b) — legătura n→m nu se poate proba pe cifre aici.

### B6. Custodie / 803x

Zero. `RegistruStoc` cu `TipStoc = 4` (Custodie): **0 rânduri**; `TipStoc ∈
{3, 6, 7}` (Folosință, Gratuit, Producție neterminată): **0 rânduri**.
`RegistruContabil` pe orice cont `80xx`: **0 rânduri**. Planul de conturi ARE
cele 25 de conturi extrabilanțiere (801…809, inclusiv 8033 „Valori materiale
primite în păstrare sau custodie" și 8035 „Stocuri de natura obiectelor de
inventar date în folosinţă"), dar nicio politică nu le numește.

### B7. Economia reală a unificării

`03-sql/b7b-economie.sql` → `03-out/b7b-economie.txt`. Cubul de azi are
1.162.622 de postări: 697.660 contabile (2 × 304.382 rânduri + 2 × 44.448
împerecheri), 283.498 de stoc (1:1 cu registrul), 181.464 fiscale
(`fizica/pas1/04-reconciliere.sql:55`).

Clasificarea celor 283.498 de rânduri de stoc — criteriul: există pe același
`DetaliuId` un picior de postare pe un cont `3%` cu `|valoare|` egală (în cub
`Latura` e derivată din semn, deci latura nu se cere potrivită):

| Clasă | Rânduri | % din stoc | % din cub |
|---|---|---|---|
| A. deschidere (fără `DetaliuId`) | 6.947 | 2,45 | 0,60 |
| B. nicio notă pe linie | 174.748 | 61,64 | 15,03 |
| C. picior 3xx cu valoare egală — **unificabil** | 101.803 | 35,91 | 8,76 |
| D/E. notă fără picior 3xx, sau fără valoare egală | **0** | 0 | 0 |

Pe tip: DSC 62.063, NIR 34.289, BCS 2.942 (toate trei rândurile liniei),
ReturClient 2.010, ReturFurnizor 477, LDI 22 — unificabile. Fără notă:
NotaTransfer 170.054 (169.376 pe TipStoc 5 + 678 pe TipStoc 1), Asamblare
4.694.

**Contenția (B7.3) taie 1.471:** liniile de BonConsum au DOUĂ rânduri de stoc
dar un singur picior 3xx (celălalt picior al notei e 6xx), deci numai unul din
cele două poate deveni aceeași postare cu 3xx.

**Rezultatul: 100.332 de postări ar dispărea din cub, 8,63 %**; rămân 183.166
de postări de stoc din 283.498. Dacă se numără și rândul de sink al consumului
ca unificat cu piciorul 6xx al aceleiași note (gestiune virtuală `Consum`,
cum cere designul), economia urcă la 101.803, adică **8,76 %**.

Limita superioară derivată în `nucleu-fizica.md` §2.5 („≈107.000, 9 %") e
corectă ca ordin de mărime, dar ușor supraevaluată: numărase 169.376 de rânduri
de NotaTransfer în loc de 170.054 (a omis cele 678 de pe `TipStoc = Magazie`),
iar contenția BCS n-a fost luată în calcul.

---

## C. Ce n-am putut proba

1. **DVI**: zero documente și zero rânduri în `DviFacturi` pe baza asta —
   legătura n→m cu facturile și „DVI pe loturi" rămân pe hârtie.
2. **Custodie, Folosință, Gratuit, Producție neterminată**: zero rânduri de
   registru, zero rânduri de politică la privat. Re-cheierea lor pe `Cont` (și
   separarea custodiei prin cont extrabilanțier) nu se poate măsura; conturile
   80xx există în plan, dar n-au fost atinse niciodată.
3. **Reevaluarea / corecția retroactivă de preț pe lot**: nu există în cod,
   deci nici rânduri. Singurul precedent e reevaluarea de imobilizări.
4. **Storno**: zero rânduri cu `Storno = true` în ambele registre, deci semnul
   regulii din `SaftProiectii.cs:1596-1603` nu e exercitat de date reale — B4
   îl calculează corect, dar pe toate rândurile termenul de storno e `+1`.
5. **`LinieSursaId`**: NULL pe toate liniile bazei, inclusiv pe descărcări —
   contractul „linia DSC acoperă o linie de FCL" e nemăsurabil aici.
6. **Profilul bugetar**: baza e privată. Regulile bugetare care folosesc
   `Gratuit`/`Folosinta`/`Custodie` (`ProfilBugetar.cs:335-336`, `:389`) n-au
   nicio cifră.
7. **`ProductieNeterminata`**: nicio regulă de seed în niciunul dintre cele
   două profiluri; valoarea de enum e moartă.
