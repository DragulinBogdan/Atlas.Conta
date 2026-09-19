# Explorare 02: semantica registrelor pentru maparea registre → cub (agent read-only, 2026-09-19)

Raport primit de coordonator; salvat ca atare, cu `fișier:linie`.

## 1. Repartitor și derivatele

`BusinessObjects/Nomenclatoare/Repartitori.cs:15-192`, decizia
`docs/decizii/016-repartitori-tpt-si-calitati.md:8-12`: bază TPH cu `Cod`,
`Denumire`, `Calitati` (flags), `ContImplicitId`. Regula: „moștenire doar
unde schema diferă și identitatea e exclusivă; calitățile transversale =
flags, nu clase". Derivate: `Partener:55` (fiscalitate + adresă;
furnizor/client = rol dat de poziția pe document, `:54`), `Angajat:178`
(Marca), `Gestiune:182` (gol), `UnitateInterna:185` (gol), `ContPropriu:189`
(Iban, EsteBanca).

`UnitateInterna` = **loc/sink de justificare, nu gestiune**: seed-ul creează
SEDIU cu `Calitati |= LocConsum` și COMISIE cu `|= Comisie`
(`DatabaseUpdate/ContaSeeder.cs:813-829`). E primitorul consumului (BCS), al
decontului (`Documente/Decont.cs:52`), al DVI (`Dvi.cs:58`), predator/primitor
la PIF/CAS/AMO (`Imobilizari.cs:32,368,551`), argument la închiderea TVA
(`Motor/InchidereTvaService.cs:95`). Calitatea (LocConsum/CentruCost) e pe
flag, clasa doar separă identitatea.

## 2. RegistruContabil

`BusinessObjects/Registre/Registre.cs:43-161`: pereche
`ContDebitId`/`ContCreditId` + `Valoare`, cu două seturi de 8 dimensiuni
plate (`DimensiuniDebit_*`/`DimensiuniCredit_*`, `:61-125`).

- **Storno = aceleași conturi, valoare negativă** („în roșu"):
  `Motor/MotorOperare.cs:660-671` copiază conturile, `NumarNota` și ambele
  seturi de dimensiuni și pune `invers.Valoare = -r.Valoare`, `Storno = true`,
  `Data = dataStorno`. Identic la stoc (`:648-659`, `-Cantitate`/`-Valoare`)
  și la TVA (`:681-699`, `-Baza`/`-Tva`, identitatea fiscală copiată).
- `DocumentId IS NULL` = rândurile de deschidere scrise de import/migrare
  (`Registre.cs:156`). Pe Flax: 64 de rânduri, toate la 2024-12-31,
  singurele cu `NumarNota` nenul (`'DESCHIDERE'`; restul 304.318 sunt NULL).
  Se scriu contra unui cont-ancoră: `tools/Import1C/Deschidere.cs:104-115` —
  sold > 0 ⇒ `Debit = contul, Credit = ancora`, altfel invers,
  `Valoare = ABS(SoldIni)`. `RegistruStoc` are 6.947 rânduri de deschidere,
  tot la 2024-12-31.

## 3. RegistruStoc

`Registre.cs:21-39`: `Cantitate`/`Valoare` poartă semnul inclus („sold prin
SUM", `:29`). Semnul vine din politică: `RegulaStoc.Semn` pe cheia
(TipDocument × `Latura` × `TipStoc` × Clasă) —
`BusinessObjects/Politici/Politici.cs:52-66`. Motorul scrie
`rand.Valoare = regula.Semn * detaliu.Valoare` (`MotorOperare.cs:359`) și alege
repartitorul din latura regulii (`MotorOperare.cs:469`).

- `ReguliStoc` pe Flax (19 rânduri): NIR Primitor +1; DSC/BCS Predator −1;
  BTR ambele laturi (−1 predator, +1 primitor); RDC Primitor −1; RLF/LDI/ASM
  Predator +1; **BCS are și Latura=Primitor cu TipStoc=2 (Consum), Semn=+1**.
- **Sink-ul de consum există, dar numai pe BonConsum**: `RegistruStoc` ×
  repartitor pe Flax dă exact trei combinații — `Gestiune`/TipStoc=1 (2.758
  rânduri), `Gestiune`/TipStoc=5 (279.269), `UnitateInterna`/TipStoc=2
  (1.471, toate pozitive, `Valoare` +340.618,38, toate din BonConsum). DSC,
  RDC, RLF nu scriu rând pe sink. TipStoc 5 (Mărfuri) și 1 (Magazie) = gestiune
  reală; 2 (Consum) exclusiv al sink-ului.

## 4. RegistruTva

`Registre.cs:203-256`: `Sens` (1 Achiziție / 2 Livrare, `Comun/Enums.cs:108-111`),
`Regim` (`Enums.cs:59-72`: 1 Normal, 2 Capitalizat, 3 TaxareInversa, 4 Scutit,
5 Neimpozabil), `Cota`, `Baza`, `Tva` — toate snapshot. `Data` = **`doc.Data`**
(data documentului fizic), nu `DataInregistrare` (`MotorOperare.cs:379`);
perioada de declarare se calculează separat (`:380-382`).

- TaxareInversa: pe achiziție se postează **4426 = 4427** pe valoarea taxei,
  sold zero; pe livrare nicio taxă și niciun rând (`Motor/TvaService.cs:24-26,59-68`).
  Registrul are pe achiziția cu taxare inversă UN rând cu `Baza` și `Tva`
  nenule: 8.380 rânduri pe Flax, toate `Sens=1`, bază 40,19 M, TVA 8,01 M.
- Storno: `Baza` și `Tva` se inversează, identitatea fiscală se copiază,
  `Data`/perioada trec pe luna stornării (`MotorOperare.cs:683-698`).
  Jurnalul nu filtrează niciodată `Storno` (`Registre.cs:252-255`).
- `PartenerId` e tipat `Repartitor` (pe Decont latura e angajatul,
  `Registre.cs:227-236`), nullable. Pe Flax toate cele 90.732 de rânduri (cu
  cele 4.096 GC?) trimit spre un `Partener`, niciunul NULL, niciun `Angajat`.

## 5. Partide și împerecheri

`Imperechere` (`BusinessObjects/Documente/Trezorerie.cs:654-680`):
`DocumentStingatorId` = plata/încasarea/nota, `DocumentId` = documentul stins
(factura). Pe Flax: Incasare→FacturaIesire 27.444, Plata→FacturaIntrare
16.925, NotaContabila→facturi 79.

- `Suma` pozitivă pe toate cele 44.448 de rânduri. Negativul e rezervat
  rândului invers care desface o împerechere dintr-o perioadă închisă, legat
  1:1 prin `InverseazaId` (`Trezorerie.cs:672-676`; `Motor/ImperechereService.cs:98-100`).
- Un stingător poate acoperi mai multe partide: 22.045 sting un document,
  1.257 două, 491 trei, până la 10+.
- `Document.TotalStingere` e fapt SCRIS la operare:
  `Scara.RotunjesteBani(doc.LiniiCreanta(...).Sum(d => d.Valoare + d.ValoareTva))`
  (`Motor/MotorOperare.cs:396-397`), brut, prin hook polimorf.
  `ImperechereService.Total` doar îl citește (`Motor/ImperechereService.cs:30-41`).
- `Asignat` = `SUM(Suma)` pe AMBELE roluri, algebric (`ImperechereService.cs:45-48`);
  `Ramas = Total − Asignat` (`:50-51`).
- `PartidaDeschisa(An, Luna, DocumentId, Rest)` (`Registre/SolduriPerioada.cs:88-102`)
  se materializează prin SQL brut la închidere: `Rest = TotalStingere −
  COALESCE(Asignat, 0)`, cu filtrul `Stare = Operat AND TotalStingere IS NOT
  NULL AND DataInregistrare <= sfârșitul lunii AND Rest <> 0`
  (`Motor/SolduriService.cs:168-181`); `Asignat` = unpivot pe ambele laturi
  tăiat pe `Imperechere.Data` (`:186-196`).
- **Capcană: nu doar facturile și plățile deschid partidă.** `TotalStingere`
  se scrie pe ORICE document operat, deci pe 12/2025 Flax are partide pentru
  NotaTransfer (45.542), DescarcareGestiune (36.689), NIR (17.803), BonConsum
  (545), ListaDiferenteInventar, InchidereTva. `Rest` e pozitiv peste tot,
  negativ doar pe ReturClient (1.631) și ReturFurnizor (398). O plată
  neîmperecheată își deschide partidă proprie: Incasare 31.381, Plata 2.486.

## 6. Conturi

`BusinessObjects/Nomenclatoare/PlanConturi.cs:11-41`. `Functie` = FCTCONT
legacy D/C/B (`:23`); `Sumator` = însumare vs soldare pe părinți (`:25`);
`RolTert` = `RolTertCont` (`Comun/Enums.cs:192-197`): `0 Niciunul`, `1 Client`,
`2 Furnizor` — politică de seed per profil, sursa listelor Customers/Suppliers
din SAF-T. Pe Flax, 16 conturi (`Simbol|RolTert|Functie`):

```
401|2|C   403|2|C   405|2|C   404|2|C   408|2|C
409|2|D   4091|2|D  4092|2|D  4093|2|D  4094|2|D
411|1|D   4111|1|D  4118|1|D  418|1|D   413|1|D   419|1|C
```

Rolul nu urmează funcția: 409x sunt furnizori cu funcție D, 419 e client cu
funcție C.

## 7. Documente: Data vs DataInregistrare, Stare, conex

`BusinessObjects/Documente/Document.cs:89-163`. `Data` = documentul fizic;
`DataInregistrare` = intrarea în evidență, „pe care se scriu registrele, lotul
și gardianul de perioadă" (`:97-100`).

| Registru | Câmpul sursă | Linia |
|---|---|---|
| `RegistruStoc.Data` | `doc.DataInregistrare` | `MotorOperare.cs:354` |
| `RegistruContabil.Data` | `doc.DataInregistrare` | `MotorOperare.cs:366` |
| `RegistruTva.Data` | `doc.Data` (fizică) | `MotorOperare.cs:379` |

`RegistruTva` mai are `PerioadaAn`/`PerioadaLuna` din
`RegistruTvaService.PerioadaDeclarare(os, doc, doc.Data, doc.DataInregistrare,
regula)` (`MotorOperare.cs:380-382`). `Lot.Data` ia tot `DataInregistrare` (`:345`).

`Stare` = `StareDocument`: `Draft = 0, Operat = 1, Stornat = 2`
(`Comun/Enums.cs:5`). Filtrele pe registre folosesc `Operat`; documentul
stornat își păstrează rândurile plus inversele, suma algebrică e adevărul.

`DocumentSursaId` (`Documente/Document.cs:139-144`) = conexul sursă→generat la
nivel de DOCUMENT (FacturaIntrare → NIR, cu `Autogenerat` pe generat). Conexul
NIR↔FCT se ține doar aici, nu pe linie. `LinieSursaId` NU e pe
`DocumentDetaliu`: e pe `DescarcareGestiuneDetaliu`
(`Documente/DescarcareGestiune.cs:118-119`, spre linia de FacturaIesire
acoperită) și pe linia de PIF (`Documente/Imobilizari.cs:99`). `Cauza` nu
există pe `Document`; enumul `CauzaIesire` (1 Casare, 2 Vânzare, 3 Lipsă,
`Enums.cs:593-597`) stă pe frunza de casare (`Imobilizari.cs:361`).

## 8. Rând de stoc → contul 3xx

Nu există FK spre cont pe `RegistruStoc`. Prin `DetaliuId` join-ul spre
`RegistruContabil` întoarce perechea D/C a notei, cu contul de stoc pe latura
care diferă de la tip la tip (NIR debit 371/3028/3024/381/303 contra 401;
BonConsum debit 6028/607/… contra credit 3028/371/…; DescarcareGestiune
607/371). **NotaTransfer nu are niciun rând contabil pe Flax** (84.688 rânduri
de stoc pe fiecare latură), iar cele 6.947 rânduri de deschidere n-au
`DetaliuId` — join-ul le pierde tăcut.

Calea sigură: `TipMaterial.ContImplicit` (`Nomenclatoare/ClasaTip.cs:52-56`),
pe lanțul `RegistruStoc.LotId → Lot.ProdusId → Produs.TipMaterialId →
TipMaterial.ContImplicitId`. Pe Flax acoperirea e totală, zero rânduri fără
cont: Mărfuri→371 (279.750), Alte materiale consumabile→3028 (3.265), Piese
de schimb→3024 (187), Ambalaje→381 (163), Obiecte de inventar→303 (129),
Materiale auxiliare→3021 (4).
