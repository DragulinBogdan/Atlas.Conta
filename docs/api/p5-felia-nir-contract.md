# Pasul 5 — Felia 5: NIR scriere (recepția culeasă manual) — contract

Stare: **PROPUS** (2026-08-11) — deciziile pin-uite mai jos așteaptă confirmarea;
implementarea urmează pe șablonul consolidat (`p5-spike1-contract.md`,
`p5-felia-fct-contract.md`, `p5-felia-trz-contract.md`,
`p5-felia-fcl-contract.md`).

A cincea felie verticală. Ridică excluderea declarată explicit la felia 2
(F2-D3, comentată în `NirDtos.cs`/`NirController.cs`: „POST/PUT/DELETE pe NIR +
`ProdusId` pe `NirDetaliu` sunt felie separată, pur aditivă") și închide
restanța de model din decizia 53i („culegerea de produs pe NIR manual").

Fluxul care se deblochează: **recepția fără factură** — marfa intră pe aviz /
pe bon de la furnizor, NIR-ul se culege manual, loturile se nasc pe liniile lui,
factura vine ulterior. Azi calea asta nu există nicăieri: nici în XAF (aceeași
gaură de model pe care GATE-ul a închis-o pe FCT — decizia 53a), nici prin API.

## Premise verificate pe cod (explorare main, nu presupuneri)

- **`NIR` are `PoliticaNumerotare` („NIR-")** — seed în ambele profiluri
  (`ProfilBugetar.cs:293`, `ProfilPrivat.cs:401`) ⇒ `Numar` e SERVER-OWNED,
  nu intră în WriteDto (contractul FCL/BTR/TRZ, invers față de FCT).
- **`NIR` NU are `PoliticaTva`** (niciun rând în cele două profiluri) ⇒ pasul
  TVA al motorului nu se declanșează pe NIR: TVA-ul se postează pe factură,
  NIR-ul duce netul (decizia 36b).
- **Clona conexă NU copiază `ProdusId`** (`MotorOperare.GenereazaConex` clonează
  `TipMaterialId/LotId/Cantitate/Valoare/TipTvaId/AngajamentId` + dimensiunile
  prin contract) — deci linia de NIR conex referă un lot STRĂIN (născut pe linia
  facturii, `Lot.LinieIntrareId` = linia FCT) și are `ProdusId` gol.
- **`NIR.PregatesteOperare`** recalculează `Valoare` DOAR pe liniile cu lot
  străin (`lot.LinieIntrareId != d.ID` ⇒ `Cantitate × lot.PretUnitar`); liniile
  cu lot propriu păstrează `Valoare` culeasă — adică 0 azi, fiindcă nimic nu o
  culege.
- **`NIR.ValideazaOperare`** cere deja: predator Partener, primitor Gestiune,
  lot per linie, lotul în gestiunea primitoare, cantitate pozitivă.
- **`LoturiCulegereService`** e tipat pe `FacturaIntrare`/`FacturaIntrareDetaliu`
  (inclusiv `LoturiLiniiSterse.Ids` și `CurataOrfane`).
- **`FacturaIesireDetaliu` are și el `ProdusId`** — dar cu semantică OPUSĂ
  (pin spre un lot EXISTENT, 37d), niciodată naștere de lot.
- **`NirApply.Citeste`** citește liniile pe BAZA detaliului cu frunza prin
  `as`-cast (istoricul pre-DIM-2 poartă linii de tip bază) — se păstrează.

## Decizii pin-uite (F5-D1…D9)

- **F5-D1. Model: `NirDetaliu` capătă `ProdusId?`+`Produs`, `PretUnitar` și
  atributele de lot** (`DataExpirare`/`LotFabricatie` prin `ILinieCuAtributeLot`,
  ca FCT/LDI/ASM). Migrație `NirCulegereLot`, pur aditivă (coloane noi nullable
  + `PretUnitar` numeric(18,6) default 0 — scara `Scara.Pret`, 49e).
  Justificarea e testul apartenenței (decizia 2/54c): produsul și prețul de
  recepție sunt caracteristică de FRUNZĂ (baza nu poartă `ProdusId` — decizia
  25c), iar `Valoare` rămâne REZULTAT, nu câmp cules (GATE 53c).
- **F5-D2. Interfața `ILinieCareNasteLot`** (`Guid? ProdusId` + `Produs Produs`),
  implementată de `FacturaIntrareDetaliu` și `NirDetaliu`.
  **NU** de `FacturaIesireDetaliu`/`DescarcareGestiuneDetaliu`: acolo `ProdusId`
  e PIN spre un lot existent (37d) — dacă ar intra în mecanism, fiecare culegere
  de FCL ar naște loturi fantomă. Numele spune intenția, nu forma.
  Gestiunea lotului nou = hook polimorf `Document.GestiuneLoturiCulese(os)`,
  default `Primitor as Gestiune` (adevărat pe FCT ȘI pe NIR); LDI+/ASM vor face
  override pe Predator când intră în scop (restanța 53i) — hook-ul e ancora,
  nu se scrie nimic speculativ pentru ele acum.
- **F5-D3. `LoturiCulegereService` generalizat** pe `(Document,
  ILinieCareNasteLot)` — o singură logică de naștere/sincronizare/curățenie
  pentru toate tipurile care nasc loturi la culegere; `LoturiLiniiSterse.Ids` și
  `CurataOrfane` trec pe `DocumentDetaliu` (strict mai larg, `Curata` filtrează
  oricum pe `LinieIntrareId`). Toate fix-urile de review ale feliilor 2 și GATE
  se păstrează VERBATIM ca semantică: self-healing pe lot FINALIZAT
  (`ProdusId` null + lot finalizat ⇒ linia își ia produsul de la lot, NU se
  șterge lotul istoric — review GATE D1), ruperea referinței când Tipul trece pe
  ne-stoc, skip grațios fără gestiune, loturile nesalvate din
  `os.ModifiedObjects`, curățenia orfanilor fără urme.
  **Gard NOU, obligatoriu (riscul propriu al feliei):** linia care referă un lot
  STRĂIN (`LotId != null` și niciun lot cu `LinieIntrareId == linia.ID`) rămâne
  NEATINSĂ — cazul clonei conexe. Fără el, un PUT pe NIR-ul conex cu `ProdusId`
  completat ar naște un AL DOILEA lot pentru marfa deja recepționată, iar
  gardianul de sold nu are cum să prindă asta (lotul nou pornește de la zero).
- **F5-D4. `LotId` rămâne SERVER-OWNED pe NIR** (ca pe FCT): nu intră în
  WriteDto. Clona conexă își păstrează lotul străin (PUT-ul nu-l atinge), liniile
  culese manual îl primesc de la serviciu, la commit. Consecință de UI, asumată:
  pe o linie cu lot străin `ProdusId` rămâne gol, iar clientul afișează produsul
  LOTULUI în loc să ofere alegerea — recepția conexă nu-și alege marfa, o
  moștenește de pe factură.
- **F5-D5. NIR nu culege TVA.** `TipTvaId`/`ValoareTva` NU intră în WriteDto și
  `AplicaTipTvaImplicit` nu se apelează: fără `PoliticaTva` pe NIR pasul TVA al
  motorului nu se declanșează, iar un TVA cules aici ar fi cifră moartă în cel
  mai bun caz și dublă postare când sosește factura. Clona conexă își păstrează
  `TipTvaId` informativ (36b) — PUT-ul nu-l atinge; ReadDto continuă să-l arate.
- **F5-D6. Valoarea liniei: două cazuri, o formulă fiecare, aceeași la culegere
  și la operare.**
  (a) linie cu lot PROPRIU (recepție manuală): `Valoare = PretUnitar × Cantitate`
  — pusă de `Aplica` la culegere (GATE 53c: operatorul confruntă hârtia înainte
  de operare) ȘI de `NIR.PregatesteOperare` la operare (extensie NOUĂ a hook-ului:
  azi ramura lipsește, deci un NIR manual s-ar opera cu valoare 0);
  (b) linie cu lot STRĂIN (clona conexă): `Valoare = Cantitate × lot.PretUnitar`
  — comportamentul existent al hook-ului, oglindit și în `Aplica` (recepția
  parțială e legitimă: operatorul scade cantitatea primită, iar valoarea trebuie
  să-l urmeze pe ecran, nu abia după operare).
  `PretUnitar` cules pe o linie cu lot străin e IGNORAT (prețul e al lotului) —
  clientul nici nu-l oferă.
- **F5-D7. Validări de operare** — se ADAUGĂ la `NIR.ValideazaOperare`:
  coerența Tip-linie ↔ Produs (oglinda 38c/53f: produsul aparține altui Tip ⇒
  refuz), și linia de natură Stoc fără produs ȘI fără lot ⇒ mesajul de domeniu
  „alegeți produsul" în locul actualului „referă un lot" (care nu spune
  operatorului ce să facă). NU se adaugă refuzul „linia trebuie să fie de tipul
  derivat" (regula FCT/FCL): NIR-urile istorice și cele importate poartă linii de
  tip BAZĂ, iar `Citeste` le arată deliberat — refuzul le-ar face ne-anulabile.
- **F5-D8. `NirApply.Aplica`/`Sterge` + endpoint-uri + felia client.**
  WriteDto: header `{Data, PredatorId!, PrimitorId!}` (fără `Numar` — F5-D1
  premise), linii `{Id?, TipMaterialId!, ProdusId?, Cantitate, PretUnitar,
  AngajamentId?, DataExpirare?, LotFabricatie?, CodEconomicId?,
  SursaFinantareId?, CodFunctionalId?, ProiectId?}`. Reconciliere server-side ca
  la FCT (upsert pe Id, delete pe dispărute, refuz pe Id repetat / Id străin /
  linie de tip vechi), garduri de scară, rezolvarea FK prin navigație cu mesaje
  de domeniu, `LoturiCulegereService.Sincronizeaza` înainte de commit,
  `CurataOrfane` la `Sterge`. `POST/PUT/DELETE /api/nir` în `NirController`.
  ReadDto se extinde cu `ProdusId/ProdusCod/ProdusDenumire`, `PretUnitar`,
  `DataExpirare`, `LotFabricatie` și cu `LotStrain` (bool: `LotId != null &&`
  lotul nu e al liniei) — clientul nu re-derivă proveniența lotului dintr-o
  euristică. Felia client `nir` devine editabilă pe șablonul FCT (`Formular` +
  editor de linie cu lookup de Produs pe OData, dimensiunile, atributele de lot),
  cu „NIR nou" în listă și liniile de lot străin cu produs/preț read-only.
- **F5-D9. Paritate XAF minimală, un singur mecanism** (lecția 58c: orice cale
  UI care nu trece prin același seam divergează tăcut):
  `FacturaIntrareLoturiController` devine `DocumenteLoturiCulegereController`
  țintit pe `Document` — serviciul e natural no-op pe tipurile fără linii
  `ILinieCareNasteLot`. NU intră în felie: layout, captions RO, polish de ecran
  pentru NIR (GATE-ul a fost explicit FCT+FCL — decizia 44.2); ecranul XAF de NIR
  rămâne funcțional, nu product-grade. Dacă `RecalculValoriCulegereController` e
  FCT-only, NIR-ul în XAF nu recalculează `Valoare` la culegere — se documentează
  ca limitare (calea API o face, `PregatesteOperare` o rescrie la operare),
  NU se extinde controllerul „pentru simetrie".

## Riscuri identificate (de ținut sub ochi la implementare și review)

1. **Al doilea lot pe NIR-ul conex** — gardul F5-D3. Cel mai scump defect posibil
   al feliei: marfă recepționată de două ori, invizibilă pentru gardianul de sold.
2. **Regresie pe FCT** din generalizarea serviciului — `LoturiCulegereService` e
   cod pe care GATE-ul și review-ul feliei 2 l-au întărit cu 4 fix-uri de fond;
   generalizarea e o mutare de TIP, nu de semantică. Verificarea: blocurile
   ModelCheck existente pe FCT rămân verzi FĂRĂ nicio ajustare.
3. **Ștergerea de loturi istorice** — aceeași clasă cu review-ul GATE D1, acum pe
   coloane noi: pe NIR-urile importate `ProdusId` e null iar lotul e finalizat.
   Ramura de self-healing trebuie să acopere identic ambele tipuri.
4. **`Valoare` zero la operare** pe NIR-urile manuale culese în XAF (F5-D9).

## Contractul de execuție

Regula de oprire, scrisă: **un NIR se culege manual din client, cu loturi născute
pe liniile lui, se operează, iar registrele de stoc și contabile ies identice cu
ale unui NIR conex echivalent** (aceeași marfă, aceeași gestiune, aceeași
valoare). Plus: FCT + conexul ei rămân neschimbate — ModelCheck verde pe ambele
profiluri fără nicio ajustare a blocurilor existente.

Verificări obligatorii la închidere:
- ModelCheck ambele profiluri (bloc NOU pentru NIR: naștere de lot din `ProdusId`
  la `Aplica`, refuzurile F5-D7, PUT pe NIR conex care NU naște al doilea lot,
  recepție parțială cu valoare recalculată din lot);
- migrația aplicată pe baza de dev ȘI pe clona bazei de import (gardul pe date
  reale: 34.289 linii FCT + NIR-urile importate);
- smoke browser end-to-end: NIR nou → linii cu produse → Save → Operează →
  registrele corecte; apoi FCT operată → conex → PUT cu cantitate redusă →
  Operează;
- drift openapi + dump metadata verificate;
- review advers dedicat, cu accent pe riscurile 1–3.
