# Decizia 74 — Pasul 5, felia 17 — SAF-T D406 **S (stocuri)** peste `RegistruStoc`

- **Data**: 2026-08-27
- **Stare**: activă (închide 73-r1; amendează 73f/g cu faptele profilului `C`)
- **Rezumat durabil**: `CLAUDE.md` §74
- **Docs**: docs/api/p5-felia-saft-s-contract.md (explorarea XSD/nomenclator/
  ghid/validator, D17-D1…D4 + amendamentele pașilor 1 și 3, riscurile 1–10,
  V1–V5, §Închidere), docs/import/faza-1c-design.md (`--saft-s`),
  anaf/SAF_T_Ghidul_D406_v2.0_dec2021.pdf (gitignored, referință)

---

**Pasul 5, felia 17 — SAF-T S — executată** (contract:
`docs/api/p5-felia-saft-s-contract.md`; 4 pași + review advers cu fix-uri, un
agent per pas, verificare independentă + commit după fiecare: `3f1f519`
contract + model, `ee6bb6a` proiecție, `195838f` XML + DUK, `4722055` REST +
client + Import1C, fix-urile F1–F8 odată cu decizia). A doua declarație cu
FIȘIER, pe infrastructura feliei 16: același `SaftDto`, același scriitor,
același oracol (DUK), aceleași uși REST — S = un DTO cu `HeaderComment = C` și
secțiunile lui, nu o a doua unealtă.

## Context

S (stocuri, „la cerere") e ALTĂ declarație decât L: tipul vine EXCLUSIV din
`HeaderComment = C` (validatorul: `AUDIT_FILE_TYPE.ON_DEMAND`), secțiunile
așteptate sunt `GeneralLedgerAccounts, TaxTable, UOMTable, AnalysisTypeTable,
MovementTypeTable, Products, PhysicalStock, Owners, MovementOfGoods`, iar
`Customers/Suppliers/GLE/facturi/plăți` = tag-uri COMPLET goale. Termen ≥ 30
zile de la solicitare, un fișier per lună. Nomenclatorul mișcărilor = 19
coduri (10 achiziție … 180 alte tranzacții), același pentru `MovementType` și
`MovementSubType`; valoare din afara listei = respingere integrală. DUK nu
verifică NICIO aritmetică pe stoc (Opening + intrări − ieșiri = Closing nu e a
lui) — cusăturile sunt ale noastre.

## Tranșări

(a) **`PoliticaMiscareSaft` = `(TipDocument × TipStoc × Semn?) → CodMiscare?
+ RolTertSaft + Motiv`.** Codul de mișcare e o funcție a TIPULUI de document
și a registrului atins, nu a produsului — deci politică (decizia 4), pe cheia
lui `RegulaStoc`. `Semn` nullabil (`null` = orice semn; LDI/ASM diferă pe
direcție); două indexuri unice filtrate (tripleta pe `GCRecord = 0` + perechea
pe `Semn IS NULL`, fiindcă în Postgres `NULL <> NULL`). **Cod NULL = excludere
DELIBERATĂ cu motiv** (`Excluse`, agregat per politică) ≠ rând fără politică
(`Neincluse/FaraCodMiscare`). Gardian pe ușa comună (cod din nomenclator,
`IsNullOrWhiteSpace`, motiv la cod null, semn ∈ {−1, null, +1}); seed-ul își
validează tabelul. Seed privat 21 rânduri (NIR 10/Furnizor, BTR 80, BCS −70 /
+Consum EXCLUS, LDI 110/120, DSC 30/Client, ASM 20/70, RLF 50/Furnizor, RDC
40/Client), probat că acoperă FIECARE pereche (tip × registru) scrisă de
`RegulaStoc` (17/17); bugetar zero (S e `Neaplicabil`, ca L). OData ReadOnly
(56).

(b) **Funcțiile legii = cod (`SaftReguli`)**: `CoduriMiscare` (19),
`TertiLinieStoc` — Client ⇒ `(partener, "0")`, Furnizor ⇒ `("0", partener)`,
intern/lipsă ⇒ `(societate, societate)` (convenție DIFERITĂ de L, unde latura
liberă = raportorul; validatorul refuză ambele `0`); `OwnerIdRaportor` =
`IdSocietate`; `ProductType` = simbolul contului de stoc (text liber ≤ 18,
nevalidat ca `AccountID`); `StockCharacteristic ("0","0")`;
`MovementReference` ≤ 35 cu discriminant și trunchiere raportată.

(c) **Proiecția `SaftStocuri(os, an, luna)`** = același `SaftDto`, cu
`HeaderComment = C`. Politica se potrivește pe **semnul REGULII**
(`(Storno ? −1 : 1) × sign(Cantitate)`): stornoul păstrează codul operației
originale, cantitatea rămâne cea din registru. `PhysicalStock` per `(Repartitor
× Lot)` pe `TipStoc`-urile cu politică cu cod (lotul = prețul unitar
aplicabil, ghid p. 36), `Opening/Closing` prin DOUĂ agregate grupate,
`StockAccountNo` = `Lot.Id` doar la > 1 lot per (gestiune × produs), `OwnerID`
= raportorul, `Owners` GOL (proprii). `MovementOfGoods` per `(Document × Storno
× Cod)`: un document cu două coduri se SPARGE (`/cod`), stornoul e mișcare
proprie (`/S`); partenerul de pe laturile documentului sau ale `DocumentSursa`
(conexele); contul liniei = `TipMaterial.ContImplicit` (lipsă ⇒
`Neincluse/FaraContStoc`, niciun cont inventat); `Quantity`/`BookValue`
SEMNATE ca în registru (DUK acceptă și |q|; semnul registrului e sursa);
`MovementPostingDate` omis în afara perioadei; rolul grupului = cel
ne-`Niciunul` unic, altfel `RolTertMixt`; `AnalysisTypeTable` GOL pe S.
Soldurile pe `TipStoc` neraportate ⇒ `SoldPeTipStocNeraportat` (n-au
document, nu pot fi `Neincluse`). Cod necunoscut ⇒
`Neincluse/CodMiscareNecunoscut`, niciodată autodeclarat.

(d) **Cusăturile**: S1 stoc fizic vs REGISTRU per intrare; **S5** aceeași
egalitate pe LINIILE EMISE (S1 singură verifică trei query-uri ale aceleiași
surse, nu fișierul — review); S2 nimic nu se pierde (mișcări + `Excluse` +
`Neincluse` == Σ registru pe TOATE `TipStoc`); S3 stoc fizic vs balanță PER
CONT, **raportată, nu blocantă**, spartă pe tip de document (pe Flax: 371 =
NTC + deschidere + FCT −20, restul exact 0); S4 referințe (produse, coduri,
identități, **unicitatea `MovementReference`**).

(e) **Fișierul**: `SaftXml` scrie cele trei secțiuni pe ordinea XSD-ului;
pe `C` secțiunile L ies FĂRĂ totaluri (`CuTotaluri`); `PhysicalStock` e
OBLIGATORIU prezent pe `C` (XSD `minOccurs=0`, profilul o cere) ⇒ lună fără
stoc fizic = refuz ÎNAINTE de primul octet, 422 pe REST; `MovementOfGoods`
gol pe `C` trece (măsurat). DUK `ok` pe scenă și pe lunile reale.

(f) **REST + client + Import1C**: `GET api/proiectii/saft/stocuri` (sumar,
`User` ⇒ 200 gol) și `stocuri/xml` (gărzile O SINGURĂ dată, proiecția ca
funcție: 400 → 403 → 422 `Neaplicabil` → 422 CUI → 422 stoc gol → streaming;
`SAF-T-S_{CUI}_{an}-{luna}.xml`); `/saft?fel=S` (URL = stare, S5 lângă S1,
S3 per cont cu componente, `Excluse`, 422 inline, starea descărcării legată
de cererea ei); `--saft-s <an> <luna>` (`SaftFel`, capitolele raportului
comune, fișier nescris + cod ≠ 0 la refuz). Seed-ul pe bazele Flax cere
`EFCoreProvider=Postgres;` în connection string.

## Review advers (fapte, cu cifre)

- `MovementReference` nu era unic: `Numar` nu e unic per tip (187 perechi pe
  Flax; 24 coliziuni în 01/2025, 1 în 12/2025 — două DSC către doi clienți
  sub o singură referință; DUK nu prinde). Fix: discriminant `#n` + S4.
  Colateral L: `InvoiceNo` duplicat 2/5.185 ⇒ `NumarFacturaDuplicat`
  (numărul rămâne cel real, 74-r3).
- S1 nu vedea fișierul ⇒ S5. Codul politicii nescos din nomenclator în
  proiecție ⇒ F3. `MovementPostingDate` = ora importului ⇒ F4.
- „0 bucăți, X lei" (861/1.168 intrări, Σ ≈ 6,35 k) și soldurile negative de
  cenți (555/735) = deriva per lot a IMPORTULUI (45e), nu gardianul 25d; se
  declară ca atare, cu avertismente separate.
- Ce NU se reproduce: semne opuse 0; `Custodie` 0; repartitorul pe
  `Magazie`/`Marfuri` = `Gestiune` 100%; `Data` rând ≠ `Data` document
  0/275.441; storno 0 pe Flax (probat doar pe scenă).

## Restanțe cu nume

- **74-r1** `Owners` cu terți (`8038`), custodie/consignație — `TipStoc.Custodie`
  fără regulă, 0 rânduri azi.
- **74-r2** `ShipTo/ShipFrom` pe BTR (gestiunile pe transfer), `LocationID`,
  `MovementPostingTime`, `MovementComments` (nicio linie de stoc n-are
  `Descriere`).
- **74-r3** `InvoiceNo` duplicat în L (2/5.185 pe 12/2025): numărul real al
  sursei 1C; doar avertisment — decizia de date e a importului.
- **74-r4** perf O(istoric): `AgregatStoc` scanează întreg `RegistruStoc` de
  două ori + `SoldPeTipStocNeraportat` cumulativ (282 k rânduri azi, 3,4–5,6
  s/lună); snapshot de solduri lunare când cifra o cere (59).
- **74-r5** `SoldPeTipStocNeraportat` pe `Consum` = 220.093,08 (09) /
  340.618,05 (12) — cifră mare, doar avertisment.
- **74-r6** deriva per lot a importului publicată per lot
  (`ReziduValoricFaraCantitate`, `SoldNegativ`) — curățare la import
  (rotunjirea pe ultimul lot), nu în proiecție.
- **74-r7** `Neincluse` întreg în sumar; pe S cheia `FaraContStoc` e
  PRODUSUL (până la 7.466 intrări) — 73-r10 mai expus.
- **74-r8** stornoul (semn, `/S`, S1 peste lună) măsurat doar pe scenă — Flax
  n-are rânduri de storno.
- **74-r9** `381` balanță creditoare (−4.520) din un NTC care dublează DSC-ul
  — constatare Import1C (date), fișierul S e cel corect.
- **74-r10** `T`/`NT` pentru S, segmentarea (73-r2/r3 se extind pe S).
- **74-r11** D17-V6 doar pe privat; proba „politică ștearsă de utilizator ⇒
  nu se recreează la re-seed" (tiparul 69b) neprobată.
- **74-r12** ecran React pentru `PoliticaMiscareSaft` (azi doar XAF; OData
  ReadOnly).
- **74-r13** kitul DUK J2.2.18 (73-r8) — S neverificat pe el.
- **74-r14** produsele fără cont de stoc pe date REALE = 0 structural pe
  Flax; pe o bază culeasă manual `ProductType "0"` + `FaraContStoc` sunt
  singurele semnale — un gardian de nomenclator (produs de stoc fără cont)
  ar fi mai devreme.
- **74-r15** `FaraCodNc` 579/647 pe Flax (73-r4 nomenclatorul NC8 rămâne).
