# Decizia 73 — Pasul 5, felia 16 — SAF-T (D406 L) ca proiecție peste registre

- **Data**: 2026-08-26
- **Stare**: activă (închide D4-r5 parțial — cod NC pe produs, D4-r10 — antetul
  societății, D4-r14; deschide prima infrastructură de fișier XML din repo —
  69-r6/D4-r13 rămân, dar au acum un precedent)
- **Rezumat durabil**: `CLAUDE.md` §73
- **Docs**: docs/api/p5-felia-saft-contract.md (D16-D1…D6 + amendamentele
  pașilor 2 și 4, riscurile 1–12, V1–V5), docs/api/d406-structura-2026.md
  (structura oficială, ancora 16.02.2026, regulile reale ale validatorului),
  docs/import/faza-1c-design.md §14 (cifrele V5)

---

**Pasul 5, felia 16 — SAF-T — executată** (contract:
`docs/api/p5-felia-saft-contract.md`; 5 pași + 4b + închidere, un agent per
pas, verificare independentă + commit după fiecare: `82aeec8` deschidere,
`c104e05` model + seed, `482f119` reguli + proiecție, `010909c` XML + DUK,
`1ad6f1e` REST + client, `5c1fc05` sumar JSON + CUI, `55c71b6` Import1C).
Prima declarație cu FIȘIER: D300/D394 se opreau la proiecție (35c), aici
fișierul e livrabilul, iar validatorul oficial ANAF (DUKIntegrator, offline)
e oracolul din suită. Tăietura: DOAR declarația L (lunară/trimestrială
corelată cu decontul) — S (stocuri, la cerere) și A (active, anual) sunt
ALTE declarații, cu alte reguli, nu secțiuni opționale ale lunarului.

## Context

SAF-T era numit din 36f/35c ca „proiecție peste registre" și amânat pentru
că îi lipseau patru lucruri de model, nu de proiecție: societatea raportoare
(`CustomerID` ȘI `SupplierID` sunt obligatorii AMBELE pe fiecare linie de
jurnal/plată, latura liberă = raportorul), codul NC și unitatea de măsură
codificată pe produs (`Product` le cere obligatoriu), tipul fiscal al facturii
(380/381). Restul fundației exista deja și se transferă întreg: `RegistruTva`
per LINIE cu `DetaliuId` („fiindcă asta cere SAF-T", 68), partenerul per
latură pe rândul contabil (54c), `AtomContabil`/`Balanta` (66), `CoduriTip`
(60b), `Judet` ISO 3166-2 și adresa la lungimile `AddressStructure` (72),
codurile SAF-T direcționale pe `TipTva` (36a), storno în negru (46a) = exact
convenția GL.31/32.

Explorarea (două rapoarte read-only) a extras structura oficială din xlsx-ul
ANAF 16.02.2026 și din BYTECODE-ul validatorului (nu există XSD în kit; cel
oficial v2.4.7 s-a găsit în pasul 3 la `D:\Dev\Work\EServices\EServices.Web`),
inclusiv regulile pe care doar validatorul le impune: `Name` interzis în
`CustomerInfo`, tuplele `PaymentMethod ↔ PaymentMechanism`, `Region` doar RO,
`Amount == CurrencyAmount × ExchangeRate`, `TotalSegmentsInsequence` cu `s`
mic, și — important — **nicio regulă `Total == Σ linii`**: totalurile și
semantica fiscală sunt ale noastre (1C exporta TVA fictiv și trecea).

## Tranșări

(a) **`Societate` = un rând, nucleu, EDITABIL** (nu `SetareProfil`, care
rămâne profil + rotunjire, 52a): antetul e DATE ale clientului. Câmpuri:
denumire, `CodFiscal` normalizat, `InregistratTva`, `RegistruComert`, adresa
cu ACELEAȘI 6 câmpuri/lungimi ca `Partener` (`AdresaSaft.Lungimi` = sursa
unică, prin reflecție; probată egalitatea), contact, `ContBancarId` →
`ContPropriu`, `BazaContabila` din cele 12 `TaxAccountingBasis` (listă pe tip,
verificată în gardian), `RaporteazaCnp` (default false: PF ⇒ `04`+cod intern —
decizia de confidențialitate a legacy-ului, ca politică a bazei). Unicitatea
nu se poate exprima ca index ⇒ `GardianEditare.VerificaSocietate` pe ușa
comună (și OData). Seed-ul CREEAZĂ rândul gol, nu-l rescrie niciodată.

(b) **`UnitateMasura` = nomenclator de nucleu, `ForbidCRUD`, seed autoritar
UN/ECE Rec 20/21** (2.163 coduri distincte generate din xlsx — `B30` e dublat în
foaie); `Produs.UnitateMasuraId?` + `Produs.CodNc` (8 cifre, gard și pe OData);
`Produs.UM` string RĂMÂNE (73-r5). `UnitatiMasuraRo.Rezolva` = grafia RO →
cod, cu coliziunea `mc` (microgram UN/ECE vs metru cub RO) tranșată pentru
GRAFIE, `ml` DELIBERAT nerezolvat (metri liniari vs mililitru — un cod ghicit
ar trece validatorul și ar fi fals). Nomenclatorul NC8 (9.984 coduri anuale) nu
se seed-uiește — validarea e a DUK (73-r4).

(c) **Rolul de terț al contului = DATE** (`Cont.RolTert` seed privat pe prefix:
411/413/418/419 client, 401/403/404/405/408/409 furnizor; 461/462 și grupele =
niciunul); **`Cont.Functie` seed-uit la privat din anexa OMFP 1802** (coloana
`Functie` D/C/B în `plan-conturi-omfp.csv`, 644 conturi, 0 necunoscute;
rectificativele 609 = C, 709 = D) — până aici era GOL și `AccountType` ar fi
ieșit `Bifunctional` peste tot. Bugetar: nimic — SAF-T îi e **neaplicabil**
(planul instituțiilor publice nu e printre cele 12 baze), 422 pe REST.

(d) **Funcțiile legii = COD** (`SaftReguli`, pure, probate): `IdPartener`
(`00`+CUI dacă înregistrat sau RO cu CUI VALID — altfel `01` n-ar fi accesibil
străinilor înregistrați; `03`+CNP doar cu `RaporteazaCnp`; `01/05` UE, `02/06`
non-UE; `04`+cod intern filtrat `[A-Za-z0-9]`), `IdSocietate` (`00`+CUI) ≠
`RegistrationNumber`-ul societății (`RO`+CUI) — două reguli, două clase în
validator; `InvoiceType` 380/381 (storno sau retur), `MetodaPlata`
(`DispozitieCasa`/`Chitanta` ⇒ 01/10, `OrdinPlata` ⇒ 03/42, `Cec` ⇒ 03/20),
`SimbolSaft` (fără puncte, netăiat — 7 cifre acceptate, măsurat), `TipCont`,
`CuiValid`/`CnpValid`. `NormalizeazaCui` (D394, o singură sursă) taie prefixul
`RO` REPETAT și insensibil la caz — `RORo1853162` a fost singura cauză de
respingere DUK pe fișierul real.

(e) **Proiecția `SaftProiectii.Saft(os, an, luna)` → `SaftDto`** (arbore
plat, sealed, enum-uri string), query-uri pe seturi, dicționare LEFT JOIN,
partenerul șters logic se declară. **Jurnal = `TipDocument`**, tranzacție =
document, rând contabil ⇒ DOUĂ `TransactionLine` (debit, credit). **Partenerul
se citește de pe RÂND, nu de pe latură** (64h confirmată: convenția
`RepartitorImplicit*` pune pe latură contrapartida — pe `628 = 401` furnizorul
e pe DEBIT); rolul e al CONTULUI (`RolTert` decide `Customer` vs `Supplier`),
partenerul = cel de pe rând (`PartenerulRandului`, un singur loc) ⇒
`Customers/Suppliers` NU ies din `Balanta(analitic)`, ci dintr-un agregat
propriu pe conturile cu rol. `TaxInformation` pe rândul al cărui cont e contul
de TVA al `TipTva`-ului rândului fiscal (`DetaliuId × Storno`): `300` + codul
direcțional + cota + `Tva`; 4428 (neexigibil) ⇒ `000/000000` (nu e taxă
exigibilă); ITV ⇒ `380200` (prin TIP, nu simbol); tip fără cod ⇒ `000000` +
avertisment; orice altceva `000/000000`. Facturi: FCL+RDC vânzări, FCT+RLF
cumpărări, **stornoul = factură proprie `381` cu valori negative** (cheia
`Document × Storno`, ca la D394), `DebitCreditIndicator` fix pe sens, semnul
pe sume; **liniile de stoc ale FCT** n-au rând contabil propriu (recepția
contează pe NIR, 26a) ⇒ contrapartida se ia din realitatea MATERIALIZATĂ a
conexului (`Lot.LinieIntrareId` → `RegistruStoc` → rândul contabil al NIR-ului
→ contul de debit), fără recepție ⇒ `Neincluse/FaraContrapartida` — niciun
cont inventat. Plăți: PLT+INC cu contrapartidă `Partener`/`Angajat`,
viramentele doar în GL, `SourceDocumentID` din imperecherea UNICĂ. Products =
produsele de pe facturile lunii (`CodNc` null ⇒ `0` + `FaraCodNc`; fără UM ⇒
`H87` + avertisment), `ValuationMethod = FIFO` (51e). **Nimic nu se pierde**:
`Neincluse` cu cauză + avertismente AGREGATE per cod (`CodAvertismentSaft`,
13 coduri) + **cusăturile în DTO** (`SaftRezumat`): partidă dublă == registru;
TVA cu TREI termeni (GL + capitalizat + fără cod SAF-T == `RegistruTva.Tva`);
facturi per sens (linii + neincluse == registru); solduri (GLA == `Balanta`);
master files (fiecare ID de pe facturi/plăți e declarat). `City` gol ⇒
`Nespecificat` + avertisment per partener; doi parteneri cu același
identificator ⇒ o intrare cu solduri CUMULATE + `PartenerDublat`.

(f) **Fișierul: `SaftXml.Scrie(dto, stream)`**, `XmlWriter` streaming, UTF-8
fără BOM, namespace de producție ca default, ordinea și numele din XSD-ul
oficial (`TaxAmount` = `AmountStructure`, `PaymentMechanism` în
`PaymentSettlement`, `TaxInformationTotals`, IBAN XOR număr de cont),
`xs:choice`-urile respectate, opționalele null omise, secțiunile S/A ca tag
gol, textul NEdespiacritizat, fără `ExchangeRate` pe RON (măsurat: trece),
`UnitPrice` la 2 zecimale (schema). **`Duk.cs` = oracolul** (în ModelCheck,
legat și în Import1C): `DUKIntegrator_AnLunaUI.jar -v D406 <xml> !<err> $
an= luna=`, `Valid ⇔ .err.txt == ok`, atenționările din `.wrn.txt`, kit/java
lipsă ⇒ SĂRIT zgomotos; **`-d` NU e utilizabil în CLI** (agață procesul), deci
auto-update-ul rămâne posibil la orice rulare (73-r8). Măsurat: `AccountID`
de 7 cifre, `1/1` segmente, `ProductCommodityCode = 0`, diacriticele și
`ONGE` TREC; `04` cu cratimă e RESPINS; NC-ul e validat contra NC8 al anului.

(g) **REST**: `GET api/proiectii/saft?an&luna` ⇒ **SUMAR** (`SaftSumarDto`:
antet, 13 contoare, cusături, `Neincluse`, avertismente) — JSON-ul întreg era
**38,6 MiB pe lună** pe Flax, „formularul nu se paginează" (69/71) e
contrazis de date: listele trăiesc DOAR în fișier și în `Saft()` pentru
ModelCheck; `GET saft/xml` ⇒ streaming pe `Response.Body` cu
`AllowSynchronousIO` per cerere (Kestrel refuză scrierile sincrone; bufferarea
ar ține luna de două ori în RAM), `Content-Disposition
SAF-T_{CUI}_{an}-{luna}.xml`, chunked. **`User` fără Read pe
`RegistruContabil` ⇒ 403 pe fișier** (un fișier gol semnat cu CUI-ul e o
declarație falsă, nu o listă goală) prin `PoateCiti(tip)` = `CanRead` pe TIP,
aditiv în `ContaApiController`; JSON ⇒ 200 gol ca D394; bugetar ⇒ 422;
an < 2020 / lună ∉ 1..12 / lipsă ⇒ 400 `EroriDto`; `[Produces("application/
xml")]` NU (ar impune content-type-ul și pe `EroriDto` ⇒ 406). `JsonApi` în
Module = opțiunile STJ ale host-ului (o sursă) + probă de serializare în
ModelCheck (coliziunea `PartenerID`/`PartenerId` dăduse 500 până la V4).

(h) **Client `/saft`**: perioada în URL, sumarul, cusăturile cu stare,
`Neincluse` (plafon 200), avertismente pliate, **„Descarcă XML" prin `fetch` +
`blob`** (JWT în antet — o navigare nu poartă antete; token în URL exclus),
numele din `Content-Disposition`. Ecranul `Societate` = XAF (73-r6).

(i) **Import1C**: `Societate1C` din `flax.Organizatii` + adresă (`SelectAdresaPe`
generalizat: o singură definiție a ordinii „sediu bate punct de lucru") +
contul bancar IMPLICIT din 1C + conducătorul, DOAR pe câmp gol (72d), contoare;
`InregistratTva` DERIVAT din prefixul RO (catalogul 1C n-are coloana) și
raportat; `AplicaUmSiCodNc` (`Rezolva(UM)`, `NIC` doar pe 8 cifre) la
materializare și `--um-nc`; contorul `CuiPrefixDublat`; **`--saft <an>
<luna>`** = proiecție + XML + DUK + raport per cauză, fără să deschidă sursa.
V5 pe baza Flax de reconciliere (seed F16 rulat; registrele identice la rând,
`reconciliere-*.txt` SHA neschimbat): `--societate` 10 câmpuri; `--um-nc`
18.642 `CodNc` preluate / 5 UM nerezolvate (`ml`×4, `pac`×1) / 1 TARIC de 10
cifre; **`--saft 2025 9` și `2025 12`: DUK `ok`, 0 atenționări pe fișierele
REALE** (70,9 / 71,6 MiB; 13.966 / 13.664 tranzacții, 3.950 / 4.589 clienți,
3.700 / 3.655 facturi emise, 1.688 / 1.531 primite, 3.000 / 2.836 plăți),
cusăturile 6/6 la cent (partidă dublă 80.527.820,95 / 83.830.122,19),
`FaraCodNc` 2.325 ⇒ 167, proiecție 4,5 s / XML 0,4 s / DUK 3,5 s. V4 pe clona
Flax.Api: matricea 200/403/422/400 completă, JSON 2,4–3,4 s, XML 3,4–6,4 s,
12 luni în 28,3 s, smoke vizual `/saft`.

(j) **Review advers** (read-only, pe `bed6417..55c71b6`, cu query-uri pe
Flax 09/2025): **0 blocante, 3 `[lege]`, 12 `[fond]`, 4 `[cosmetic]`** —
fișierul real trecea DUK cu cifre GREȘITE în locuri pe care validatorul nu le
vede: **L1** achizițiile intracomunitare (TI) cu TOATE liniile pe stoc
lipseau din `PurchaseInvoices` — singurele rânduri ale FCT sunt `4426 = 4427`,
401-ul e pe NIR-ul conex (26a); cele 84 de `ContFaraRol` de pe Flax (bază
3,37 M) erau EXACT astea (dovadă: 308 rânduri 4426/4427, Σ 707.040,03 = TVA-ul
din `Neincluse`) ⇒ gaură de PROIECȚIE, nu de seed (73-r9 se închide aici);
**L2** regimul Capitalizat scotea `InvoiceLineAmount`/`NetTotal` BRUTE
(`Valoare` = net × (1+cota) în motor) lângă `TaxBase` net; **L3** jumătatea de
storno purta `InvoiceDate`/`TransactionDate` = data documentului, nu data
stornării (motorul scrie rândurile inverse la `dataStorno`). Fond: **F1**
plățile stornate ieșeau pozitive, de două ori (fără spargere pe `Storno`);
**F3** cusăturile 1 și 4 erau VIDE (Σ debit − credit = 0 prin partidă dublă;
D și C din același acumulator); **F4** lipsea cusătura terților (Customers +
Neincluse == closing pe conturile cu rol); **F5** faptele fiscale ale DEC/NTC
dispăreau fără urmă (nici facturi, nici `Neincluse`); **F6** terț referit cu
`AccountID` gol ⇒ XML invalid; **F7** fișier fără CUI ieșea 200 („declarația
falsă" pe care 403-ul o refuză pe `User`); **F8** `SeedRolTert` rescria și
conturile adăugate manual; **F2** „partenerul de pe rând" cu partener pe
AMBELE laturi și AMBELE conturi cu rol = decizie nepin-uită (pin: fiecare
latură își ia partenerul PROPRIU, rolul contului ei decide); **F9**
`SeedPlanConturi` fără early-return re-creează conturile șterse logic (regulă
asumată: planul CSV e al legii); **F10** Grecia `01GR` vs `01EL` (de măsurat);
**F11** `Neincluse` plafonat în client (73-r10); **F12** unicitatea
`Societate` = race asumat. Răspunsuri „nu se reproduce", cu dovezi: criteriul
de perioadă e ACELAȘI câmp (`Data` a rândului) în GL/TVA/balanță/facturi/plăți;
rândul TVA e PER LINIE (8.001/8.215 rânduri 4426/4427 cu fapt, Σ exact
3.205.466,74; 214 = ITV); identitatea partenerului pe toate cazurile; PATCH
pe `Societate` deny-by-default pentru `User`. **Fix-urile (pas 6a, aplicate
de main prin agent, verificate independent)**: L1 (contul cu rol se caută și
pe conexele facturii), L2, L3, F1, F3, F4, F5 (cauza `TipFaraSectiuneFacturi`),
F6 (⇒ `Neincluse`, gard în scriitor), F7 (422 pe XML fără CUI), F8 (doar
conturile din CSV), F2 pin-uit cu probă, C1/C2/C4; F9/F12 = reguli asumate,
scrise; F10 măsurat cu DUK: `01EL`/`01GR` și `Country EL`/`GR` TREC toate ⇒
pin ISO `GR` (`SaftReguli.CodTaraSaft`, identificator + adresă). Pe Flax
09/2025 după fix-uri: `Neincluse` 100 ⇒ 16, facturi primite 1.688 ⇒ 1.772,
cusăturile noi (per cont 99/0, terți) la cent, DUK `ok`. ModelCheck 752/530.

(k) **Restanțe cu nume** (textul aici, numele în CLAUDE.md):
- **73-r1** declarația S (MovementOfGoods / PhysicalStock / Owners /
  MovementTypeTable) peste `RegistruStoc` — cere `MovementType` per
  tip × `TipStoc` ca politică și `OwnerID`; A (Assets) = modul separat (9).
- **73-r2** trimestrial `T` (neînregistrați), rectificativă `R`, nerezidenți
  `NL/NT` — `HeaderComment` e mereu `L`.
- **73-r3** segmentarea: pragul nu e în nicio sursă; un singur segment; luna
  Flax = 71 MiB trece.
- **73-r4** nomenclatorul NC8 (anual, 9.984) + lookup pe `Produs.CodNc`;
  corecturile de nomenclator de pe Flax: `ml`/`pac` (om), TARIC `8536419040`.
- **73-r5** `Produs.UM` string rămâne lângă FK (tech-debt marcat; se elimină
  când toate produsele au FK).
- **73-r6** ecranul React `Societate` + `CodNc`/UM pe ecranul de produs
  (`lista-react.md`).
- **73-r7** OpANAF 1783/2021 neprocurat: termene, praguri pe categorii,
  perioada de grație — nu afectează structura.
- **73-r8** kitul DUK local J2.2.8 vs publicat J2.2.15; `-d` inutilizabil ⇒
  auto-update-ul e posibil la rulare; ancora se notează, actualizarea e a
  utilizatorului.
- **73-r9** `ContFaraRol` pe 84 (09) / 111 (12) facturi de achiziție Flax —
  gaură de DATE (conturi fără `RolTert` în planul importat), contabilizată la
  cent în cusătura 3a; de închis prin seed/mapare (vezi review).
- **73-r10** `Neincluse` întreg în sumar (plafon 200 în client); agregarea per
  cauză e candidatul următor.
- **73-r11** `UnitPrice` la 2 zecimale (schema) vs prețul la 6 în model;
  riscul 10 (cantitate negativă pe storno) nemăsurat contra DUK.
- **73-r12** 64h rămâne: dimensiunea `Repartitor` poartă contrapartida;
  `PartenerulRandului` e singurul loc de atins dacă modelul se schimbă.
- **73-r13** `TaxRegistration.TaxType 100030` (cod anulat ex officio) ≠
  `InactivFiscal`; D4-r1/72-r4 neconsumate.
- **73-r14** conducătorul din 1C rupt la primul spațiu (convenție a
  conectorului); `InregistratTva` al societății derivat, nu citit.
- **73-r15** `CuiPrefixDublat` nemăsurat (rulează doar la reclasificare);
  curățarea `CodFiscal` în Import1C.
- **73-r16** multi-valută: totul RON, FCT cu `Valuta ≠ RON` ⇒ avertisment
  (34g).
- **73-r17** D4-r5 rămâne pentru CATEGORIA de bunuri/structura `op11` (codul
  NC e închis); `FaraOp11` doar pe liniile fără `CodNc`.
- **73-r18** contactul/IBAN-ul partenerilor (34g) — opționale în
  `CompanyStructure`, neemise.
- **73-r19** 422 pe fișier cu CUI gol/invalid probat la scriitor, nu pe HTTP;
  `--cititori` (C4: exact o organizație cu `CodUnic`) nemăsurat pe Flax.
