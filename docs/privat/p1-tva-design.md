# P1 — Profil privat + TVA structural (design)

Stare: **FIXAT — toate cele 5 tranșări din §9 confirmate (22.07.2026);
document de intrare pentru sesiunea de implementare P1.** Contextul: decizia
35 (pivot privat-first). Zonă sensibilă
(atinge baza `DocumentDetaliu` și motorul) → design înainte de schemă, conform
regulii de cercetare.

## 1. Fapte pe care stă designul

### În cod (verificate la zi)

- `DocumentDetaliu` (bază): `TipMaterialId`, `LotId?`, `Cantitate`, `Valoare`,
  `AngajamentId?`, `Dimensiuni` — fără nimic de TVA (decizia 22a: „adăugare
  ulterioară aditivă" — adăugarea e acum).
- `CotaTva decimal` există DOAR pe `FacturaIntrareDetaliu`,
  `FacturaIesireDetaliu`, `DecontDetaliu`; toate trei materializează
  `Valoare = net × (1 + CotaTva/100)` în `RecalculeazaValoare()` — adică
  **TVA capitalizat în valoarea de postare** (corect pentru bugetar
  neplătitor, incorect pentru privat plătitor).
- Pipeline-ul `MotorOperare.Opereaza` e calculează-validează-materializează
  (decizia 33d); regula de contare se alege per linie (TipMaterial exact →
  NaturaFiltru → generică), fără regulă = linia nu postează pe acel tip.
- Nu există niciun mecanism de profil în seed: `ContaSeeder.Seed()` e
  monolitic bugetar (CPLAN + politici).

### Fiscal (confirmate pe surse actuale, iul. 2026)

- Cote TVA în vigoare (Legea 141/2025, de la 01.08.2025): **21%** standard,
  **11%** redusă; **9%** tranzitoriu doar locuințe, până la 31.07.2026; 0/scutit.
- Nomenclatorul de coduri de taxă SAF-T (D406) e **direcțional și granular pe
  deductibilitate**: serii distincte pentru livrări (ex. 3103nn) și pentru
  achiziții pe grade de deductibilitate (301nnn–309nnn); ANAF a publicat
  coduri noi pentru 21%/11%. ⇒ un rând de nomenclator TVA are nevoie de mapări
  separate pe vânzare/cumpărare.
- D394 agregă pe tip de operațiune × cotă (L/A/AI/V/C/N…) ⇒ categoria D394 e
  atribut de nomenclator, nu calcul.

## 2. Nomenclatorul `TipTva` (tabelă nouă — date, nu cod)

Regimul fiscal e nomenclator, nu procent. Câmpuri:

| Câmp | Tip | Rol |
|---|---|---|
| `Cod`, `Denumire` | string | identitate + UI |
| `Cota` | decimal | 21 / 11 / 9 / 0 |
| `Regim` | enum `RegimTva` | vezi mai jos |
| `ContTvaDeductibilId?` | FK Cont | 4426 (date, per profil) |
| `ContTvaColectatId?` | FK Cont | 4427 |
| `ContTvaNeexigibilId?` | FK Cont | 4428 — REZERVAT (TVA la încasare / facturi nesosite; mecanismul e amânat) |
| `CodSafTLivrare?`, `CodSafTAchizitie?` | string | maparea D406 (direcțională) |
| `CategorieD394?` | string | agregarea D394 |

`enum RegimTva`: `Normal` (deductibil/colectat după direcția documentului),
`Capitalizat` (TVA intră în `Valoare`, nu se postează separat — comportamentul
de azi, profilul bugetar neplătitor), `TaxareInversa` (4426 = 4427,
autolichidare), `Scutit`, `Neimpozabil`. `NeexigibilLaIncasare` NU intră în
enum la P1 — se adaugă aditiv odată cu mecanismul (vezi §8).

Seed privat: Normal 21 / Normal 11 / Normal 9 (tranzitoriu) / TaxareInversa 21
/ Scutit cu drept / Scutit fără drept (=Capitalizat cu cotă 0? nu — scutit
fără drept la achiziție înseamnă TVA-ul furnizorului capitalizat; rândul de
achiziție nedeductibilă = `Capitalizat 21`) / Neimpozabil. Seed bugetar:
un singur rând `Capitalizat 21` (+ `Capitalizat 0`) — comportament identic cu
azi. Codurile SAF-T/D394 se completează la seed din nomenclatoarele ANAF
(checklist-ul orizontal — decizia 35c).

## 3. Baza `DocumentDetaliu`: `TipTvaId?` + `ValoareTva`

**De confirmat (tranșarea principală).** Testul de apartenență (decizia 2):
postarea 4426/4427 e regulă contabilă generică → câmpurile intră în **bază**:

- `TipTvaId Guid?` + nav — null = linie fără semantică de TVA (BTR, BCS, LDI,
  NIR, PLT/INC — neschimbate).
- `ValoareTva decimal` (default 0) — TVA-ul de postat separat. `Valoare`
  rămâne CE A FOST MEREU: valoarea de postare a rândului principal (net la
  regim deductibil, brut la Capitalizat). Semantica „o singură valoare de
  postare" (22a) nu se schimbă — se adaugă a doua valoare de postare, cu
  destinație fixă (conturile de TVA).
- `CotaTva` de pe cele trei derivate **se șterge** (redundant cu
  `TipTva.Cota`); alternativa respinsă: interfață `ILinieCuTva` — TVA-ul nu e
  trăsătură exotică a unui tip (ca postarea explicită), e mecanism generic de
  motor pe familia facturilor, iar SAF-T îl cere pe orice linie sursă.

Calculul devine helper comun apelat din `PregatesteOperare` al derivatelor
(`RecalculeazaValoare` unificat):

```
net = PretUnitar × |Cantitate|
Capitalizat:            Valoare = net × (1 + Cota/100); ValoareTva = 0
Normal / TaxareInversa: Valoare = net;                  ValoareTva = net × Cota/100
Scutit / Neimpozabil:   Valoare = net;                  ValoareTva = 0
TipTva null:            Valoare = net;                  ValoareTva = 0
```

Pe FCT `ValoareTva` rămâne **editabilă** după calcul (factura furnizorului
bate rotunjirea noastră); pe FCL se calculează. `Document.Total` devine
explicit BRUT: `Σ (Valoare + ValoareTva)` — imperecherea și plata autogenerată
sting brutul (azi Total e deja brut prin capitalizare, deci invarianții
`ImperechereService` nu se schimbă).

## 4. Postarea TVA: `PoliticaTva` (tabelă nouă) + pas generic în motor

**De ce nu prin `RegulaContare`:** pe FCT liniile de stoc NU au regulă de
contare (postează pe NIR — decizia 26a), dar TVA-ul lor deductibil se postează
pe FACTURĂ (4426 = 401 pentru toată factura; NIR-ul duce netul 3xx = 401).
Postarea TVA trebuie deci să fie independentă de potrivirea regulii
principale. Simetric cu `PoliticaScadenta`/`PoliticaValidare`:

`PoliticaTva`: `TipDocumentId` + `DirectieTva` (enum: `Deductibil`/`Colectat`)
+ `SursaContrapartida` (`SursaCont`: RepartitorPredator/RepartitorPrimitor) +
`ContrapartidaFallbackId?` (FK Cont).

Pas nou în faza „calculează" a `Opereaza` (după rândurile principale, înainte
de validare/materializare): pentru fiecare linie cu `ValoareTva ≠ 0` și regim
care postează:

- `Deductibil` (FCT, DEC): debit = `TipTva.ContTvaDeductibil`, credit =
  contrapartida rezolvată (FCT: RepartitorPredator, fallback 401; DEC:
  RepartitorPredator, fallback 542).
- `Colectat` (FCL): debit = contrapartida (RepartitorPrimitor, fallback 411),
  credit = `TipTva.ContTvaColectat`.
- `TaxareInversa`: indiferent de direcție, un rând 4426 = 4427 pe valoarea TVA
  (autolichidare, sold zero).

Rândul TVA e per linie (păstrează `DetaliuId` pe registru, ca tot restul);
dimensiunile se rezolvă cu ACELAȘI coalesce ca rândul principal (fără
override-uri de regulă — sursele: linie → default polimorf header). Fără rând
`PoliticaTva` pe tip = niciun rând TVA (profilul bugetar nu primește rânduri —
zero schimbare de comportament). Verificarea `DimensiuniObligatorii` (33a) se
aplică natural și rândurilor TVA — nimic special.

## 5. Profilul contabil devine selecție de seed (decizia 29 materializată)

- Setare `ProfilContabil` în appsettings (`Bugetar` | `Privat`), citită de
  `Updater`/`ContaSeeder` — **profil per bază**, aliniat cu bază-per-client
  (decizia 35d). Fără profil în date, fără mixaj la runtime.
- `ContaSeeder` se sparge: nucleu neutru (ancorele `TipDocument`, mecanismele
  de derivare Cod-Tip=simbol + tăierea segmentelor, `SeedContare6xxDin3xx`) +
  pachet per profil (plan CSV + Clasă/Tip + politici + TipTva + repartitori
  minimali). Pachetul bugetar = conținutul de azi, mutat, nemodificat.
- Pachetul privat: `plan-conturi-omfp.csv` (OMFP 1802, sintetice grad 1–3,
  aceeași structură pipe-delimited; sursa oficială se fixează la implementare),
  Clasă/Tip privat minimal (301/302/303/345/371/381 pe stoc, SERV, VEN cu
  704/706/707/708, tehnicele TRZ/TVA), derivări per profil: 6xx=3xx (există),
  **371→607, 345→711** (decizia 29c — pregătesc P2), plus inventar 3xx=7588
  la privat (nu 791).
- `DimensiuniObligatorii` la privat: pornesc goale (nu există CPLAN_DEFALCARE);
  editabile ca date când apare nevoia (Gestiune/CentruCost).

## 6. Efecte pe tipurile existente (sub profil privat)

- **FCT**: liniile de stoc → NIR conex postează netul (3xx = 401); factura
  postează serviciile/cheltuielile net + rândurile 4426 per linie (inclusiv
  ale liniilor de stoc). Totalul pe 401 = brut. ✔ fără dublă postare.
- **FCL**: 411 = 7xx net + 411 = 4427 per linie. Rămâne fără reguli de stoc
  până la P2.
- **DEC**: net pe cheltuială + 4426 = 542.
- **PLT/INC/BTR/BCS/LDI/NIR manual**: `TipTvaId` null → identic cu azi.
- **Profil bugetar**: singurele TipTva sunt `Capitalizat` → calculul dă exact
  valorile de azi; fără `PoliticaTva` → niciun rând nou. ModelCheck-ul bugetar
  existent trebuie să rămână verde DOAR cu înlocuirea culegerii `CotaTva` →
  `TipTva` în scenariile lui.

## 7. Validare (contractul feliei)

- ModelCheck se parametrizează pe profil (bază separată pentru privat, ca
  profil-per-bază) și primește bloc e2e nou: FCT privat cu linie stoc + linie
  serviciu (verifică: NIR net, 4426 pe factură, 401 brut, imperechere pe
  brut), FCL cu 4427, DEC cu 4426, taxare inversă, capitalizat (nedeductibil),
  storno cu rânduri TVA inverse.
- Scenariile bugetare existente (177 verificări) rămân verzi.

## 8. Amânate, documentate (aditive peste P1)

- **TVA la încasare** (regim + 4428 + transfer la imperechere) — nomenclatorul
  rezervă contul, mecanismul se scrie când apare cerința.
- **Facturi nesosite / NIR fără factură la privat** (408/4428).
- **Regularizarea de rotunjire per document per cotă** (e-Factura calculează
  pe total; per linie + ValoareTva editabilă acoperă culegerea; rândul de
  regularizare se adaugă când FCL emite e-Factura).
- **Prorata, ajustări de TVA, decont D300/D394/SAF-T ca proiecții** — module
  de raportare peste registre, după P2.
- **Închiderea de TVA lunară (4423/4424)** — document/procedură de închidere,
  se proiectează odată cu închiderea de perioadă.

## 9. Tranșări deschise (de confirmat înainte de implementare)

1. `TipTvaId` + `ValoareTva` pe **bază** (nu interfață) și ștergerea `CotaTva`
   de pe cele trei derivate — recomandat: da.
2. `PoliticaTva` ca tabelă nouă per tip de document (nu extensie
   `RegulaContare`) — recomandat: da.
3. Profil = setare appsettings per bază; `ContaSeeder` spart în nucleu +
   pachete — recomandat: da.
4. `Document.Total` explicit brut (`Σ Valoare + ValoareTva`) — invarianții
   imperecherii neschimbați — recomandat: da.
5. Seed-ul cotelor: 21/11/9-tranzitoriu/0 + TaxareInversa + Capitalizat +
   Scutit/Neimpozabil; 9% se marchează cu valabilitate (expiră 31.07.2026) —
   detaliu de date, nu de schemă.
