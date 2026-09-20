# TR-D6b — Declarația fluxului per tip (pilot BCS, PLT, FCT) — contract

Stare: **ÎN LUCRU (2026-09-20)**, felia 30; branch `tr-d6b-declaratia-fluxului`
(tăiat din `main` după fast-forward-ul feliei 29, `90d7be7`).
Decizia-mamă: 090 (`docs/decizii/090-nucleu-cub-de-postari.md`, regula
durabilă (b), (c), (d), (e), (g), (h), (j), (l), (m)); nucleul: contractul
TR-D6a (`tr-d6a-nucleu-pur-contract.md`, N-D1…N-D12) și restanțele
N-r2…N-r4 (de confirmat sau întors AICI); ordinea și regula de oprire:
`nucleu-transfer.md` §4 („TR-D6b") și §4.1.

**E singurul pas care poate întoarce decizia 090.** Criteriul e scris în
090l: dacă forma declarației nu e mai simplă decât cele 41 de hook-uri pe
care le înlocuiește, sau dacă cele trei frunze NU produc din operand închis
exact postările motorului vechi (plus diferențele DECLARATE), felia se
oprește și decizia se întoarce la 090 — nu se normalizează.

## Scop

Forma care înlocuiește cele 30 de override-uri `PregatesteOperare`/
`ValideazaOperare` și celelalte 11 hook-uri de motor: un declarant per
frunză, în `Atlas.Conta.BackOffice.Module` (primul și singurul consumator al
lui `Atlas.Conta.Nucleu`), care primește un OPERAND ÎNCHIS construit de
adaptorul `Fapte` și DECLARĂ, pur, mișcările documentului cu unitățile
numite/născute, deciziile și ipotezele; nucleul asamblează tranzacția și o
verifică. Pilot: BCS (două mișcări de stoc + sink), PLT (nominalizarea
partidei + latura pereche a viramentului), FCT (recepția, TR-D3).

La acest pas NIMIC nu se persistă și motorul vechi NU se atinge: declarantul
rulează ALĂTURI de `MotorOperare`, pe aceleași documente, iar proba e
egalitatea cu registrele scrise de motorul vechi, transformate în cub cu
maparea fizicii portată în C# (oracolul). XAF și React rămân înghețate (090m);
hook-urile rămân în cod până la TR-D7/D9.

## Testul contra invarianților

- I (orice postare are document): `Cauza` pe fiecare mișcare = `(document,
  linie)`; declarantul nu poate produce postare fără linie.
- II (motorul nu cunoaște frunzele): nucleul primește `Declaratie`; frunza
  își numește declarantul printr-un singur membru polimorf (`Declarant`),
  fără `is`/`switch` în vreun dispecer; operandul e generic, frunza nu-l
  extinde.
- III (starea se citește de pe document → `Σ[Unitate]`): declarantul citește
  starea DOAR din operand (solduri de unitate consemnate ca
  `SoldUnitateCitit`); nu ține nimic ca fapt.
- IV (o singură sursă de reguli): declarantul nu e o a doua sursă — nu e
  apelat de nicio ușă; e probă. Devine sursă la TR-D7, când `PosteazaInCub`
  pe tip îl pune sub `MotorOperare`.
- V (diferențele se raportează): orice diferență față de oracol e ori
  defect, ori normalizare NUMITĂ în acest contract (B-D8); a treia opțiune
  nu există.
- VI (fără interogare per linie): `Fapte.Operand` citește pe seturi (id-uri
  colectate, o interogare per tabelă); proba = `NumaratorSql` din ModelCheck
  pe construirea operandului.

## Deciziile (B-D1…B-D10)

### B-D1 — Așezarea, referința, numele

- `Atlas.Conta.BackOffice.Module.csproj` primește
  `ProjectReference` → `../../Atlas.Conta.Nucleu/Atlas.Conta.Nucleu/Atlas.Conta.Nucleu.csproj`;
  proiectul intră și în `Atlas.Conta.BackOffice.slnx` (build-ul soluției îl
  compilează). WebApi, Blazor.Server, Client, Import1C NU referă nucleul
  direct (090l); ModelCheck îl vede tranzitiv prin Module și îl referă
  DIRECT doar pentru helperul de test (B-D7).
- Codul nou din Module stă în folderul `Declaratii/` (namespace
  `Atlas.Conta.BackOffice.Module.Declaratii`), NU `Nucleu/` (ar face
  `Nucleu.X` ambiguu). Coliziunile de nume cu Module (`Declaratie`,
  `Miscare`, `Sold`, `Unitate`, `Cauza`, `Motor`, `RegimTva`) se rezolvă cu
  alias de namespace (`using N = Atlas.Conta.Nucleu;`) în fișierele care
  ating ambele; nu se redenumește nimic în nucleu.
- `Atlas.Conta.Nucleu` se extinde DOAR dacă pilotul cere un caz de
  `Decizie`/`Ipoteza` sau o primitivă pură absentă (ierarhiile sunt închise:
  `private protected`); orice extindere aduce testul ei în
  `Atlas.Conta.Nucleu.Teste` și rămâne BCL-only (testul de arhitectură).
  `Miscare` NU primește `Atribuit` la acest pas (reevaluarea e TR-D9).

### B-D2 — Forma: `IDeclarant`, `Operand`, `Document.Declarant`

```csharp
public interface IDeclarant {
    N.Declaratie? Declara(Operand operand, N.Rotunjire rotunjire, ICollection<N.Refuz> refuzuri);
}
```

- PURĂ: fără `IObjectSpace`, fără entități, fără ceas; primește operandul
  închis și instanța de rotunjire, întoarce `Declaratie` sau `null` cu
  refuzurile proprii adăugate în listă (toate, nu primul — ca `erori` azi).
  Primitivele nucleului care aruncă `RefuzException` (`Evaluare`,
  `Fifo`) se prind în driver (B-D3), nu în declarant.
- Frunza își numește declarantul printr-un singur membru polimorf pe
  `Document`: `public virtual IDeclarant? Declarant => null;` (null = tipul
  nu declară încă — regimul dual al pilotului; la TR-D7 gardul devine
  DATĂ `PosteazaInCub` pe `TipDocument`, iar `null` devine eroare de
  configurare). Cele trei frunze îl suprascriu într-o linie
  (`=> DeclarantBonConsum.Instanta`). Declaranții sunt clase `sealed`
  fără stare (`Instanta` static readonly) în `Declaratii/`.
- Hook-urile existente NU se șterg și NU se modifică la acest pas
  (motorul vechi le folosește); contractul lor de moarte e TR-D7 (regulile
  de motor) și TR-D9 (stingerea).

`Operand` (record imuabil, DTO, în `Declaratii/Operand.cs`) — tot ce
influențează rezultatul, rezolvat și înghețat (090b):

- **Documentul**: `Id`, `CodTip` (string, ex. `"BCS"`), `TipDocumentId`,
  `Data`, `DataInregistrare`, `Numar?`, `Autogenerat`, `DocumentSursaId?`,
  `Predator`/`Primitor` ca `RepartitorFapt(Id, FelRepartitor, ContImplicitId?,
  Calitati)` cu `FelRepartitor { Partener, Angajat, Gestiune, ContPropriu,
  UnitateInterna }` (numele CLR de azi, ca DATĂ), `Valuta?`, `Curs?`,
  `DataScadenta?` (din `IDocumentCuScadenta`).
- **Liniile** `LinieOperand`: `Id`, `TipMaterialId`, `ClasaId?`, `Natura?`,
  `ContImplicitTipId?`, `LotId?`, `Lot?` = `LotFapt(Id, ProdusId,
  TipMaterialId, ContImplicitId, Data)`, `Cantitate`, `Valoare`, `ValoareTva`
  (culese), `TipTvaId?`, `PretUnitar?` (din `ILinieCuPretUnitar`),
  `ProdusId?` (`ProdusCules()`), `ContDebitId?/ContCreditId?/RepartitorDebitId?/
  RepartitorCreditId?` (din `ILinieCuPostareExplicita`), `Analiza` (cele 6
  dimensiuni din `DimensiuniCulese()`), `AngajamentId?`.
- **Politica** (faptele plate deja existente în `Motor/Potrivire.cs`):
  `ReguliContare`, `ReguliStoc`, `PoliticaTva?` (`Directie`,
  `SursaContrapartida`, `ContrapartidaFallbackId`, `DeclarareIntarziata`),
  `TipuriTva` per id folosit (`Regim`, `Cota`, `ContTvaDeductibilId`,
  `ContTvaColectatId`), `Conturi` per id atins (`Simbol`, `RolTert`),
  `TolerantaTaxa` (B-D6), `Numerotare` NU (numărul e al materializării).
- **Starea citită**: `SolduriLoturi` per lot numit (`N.Sold` la
  `DataInregistrare`, FĂRĂ documentul curent — `StocService.SolduriLaData`),
  `RestPartidaSursa?` (PLT: `ImperechereService.Ramas(DocumentSursaId)`),
  `PerioadaDeschisa` (an, lună), `VersiunePolitica` (data ultimei alinieri a
  seed-ului sau `DataInregistrare`, ca marcaj; valoarea reală e TR-D7).
- **Rotunjirea**: `ConventieBani` (din `Scara.ConventieBani`, înghețată per
  bază — 49e) intră în driver ca `new N.Rotunjire(conventie)`.

Regulă cu dinți: dacă un declarant al pilotului are nevoie de un câmp al
frunzei care NU e pe baza `Document`/`DocumentDetaliu` și nici pe o interfață
declarată (`Interfete.cs`), agentul se OPREȘTE și raportează — nu adaugă
câmpuri de frunză în `Operand` și nu citește entitatea din declarant.

### B-D3 — Adaptorul `Fapte` și driverul

- `Motor/Fapte.cs` (singura ortografie entitate → fapt) primește
  `Operand(IObjectSpace os, Document doc) → Operand`: citește pe seturi,
  o interogare per tabelă (loturi, produse, tipuri de material, conturi,
  tipuri TVA, reguli, politici, solduri), și consemnează ce a citit.
  `Fapte` rămâne `internal`.
- Driverul `Declaratii/Contractare.cs`:
  `Contractare.Contracteaza(IObjectSpace os, Document doc) → N.Contract`:
  `operand = Fapte.Operand(os, doc)`; `rotunjire = new Rotunjire(Scara.ConventieBani)`;
  `declaratie = doc.Declarant.Declara(operand, rotunjire, refuzuri)` (cu
  `RefuzException` prinsă în `refuzuri`); `refuzuri.Count > 0 ⇒ Contract.Refuza`,
  altfel `N.Motor.Opereaza(declaratie, rotunjire)`. `Declarant == null ⇒
  InvalidOperationException` (apelantul nu cheamă driverul pe tipuri fără
  declarant). E forma viitoare a lui `Valideaza` (motorul fără reținere,
  090b); materializarea NU există la acest pas.
- Refuzurile de admisibilitate care sunt azi în `ValideazaOperare` ale
  frunzelor (predatorul e gestiune, primitorul e loc de consum, lotul
  obligatoriu, cantitatea pozitivă, liniile de virament…) se re-exprimă în
  declarant pe operand, cu coduri stabile (`PREDATOR_NEPOTRIVIT`,
  `LOT_LIPSA`, `CANTITATE_NEPOZITIVA`, `VIRAMENT_MIXT`…), FĂRĂ localizare
  (mesajul românesc de azi rămâne al motorului vechi până la TR-D7). Gardienii
  de stare (perioadă închisă, latura pereche deja legată, conexe operate)
  rămân ai motorului vechi la acest pas — sunt admisibilitate de stare, nu
  declarație.

### B-D4 — Declarantul BCS

Per linie (lot obligatoriu, `Cantitate > 0`), O SINGURĂ mișcare:

- `DeLa` = `{ Cont = contul liniei din RegulaContare (SursaCont.TipMaterial ⇒
  ContImplicit al tipului), Gestiune = PredatorId, Produs = lot.ProdusId,
  Unitate = Lot(lot.Id, cont, produs, lot.Data) }`;
- `La` = `{ Cont = contul debit al regulii (Explicit, 6xx derivat),
  Gestiune = PrimitorId (locul de consum — repartitor intern; e „gestiunea
  virtuală Consum" a lui 090g prin FELUL repartitorului, nu prin rând nou de
  nomenclator), Produs = lot.ProdusId, Unitate = același lot }`;
- `Cantitate = q`; `Valoare = Evaluare.Iesire(soldLot, q, rotunjire)` pe
  raportul CURENT al lotului, în secvența liniilor (același lot pe două linii:
  a doua vede soldul după prima) — N-D7/N-r3; `Cauza = (doc, linie)`.
- Decizii: `ContRezolvat(linie, cont, sursa)` ×2, `ValoareIesire(linie, lot,
  q, valoare)`; ipoteze: `SoldUnitateCitit(lot, sold)` per lot,
  `PerioadaDeschisa`, `VersiunePolitica`.
- Refuzuri: predatorul nu e gestiune; primitorul e partener sau fără
  `LocConsum`; linie fără lot; `q ≤ 0`; `STOC_INSUFICIENT` din `Evaluare`.

Diferența declarată față de azi: valoarea ieșirii pe raportul curent contra
`Cantitate × Lot.PretUnitar` cu substituție doar la golire (`StocService.cs:41`).
Pe scenele ModelCheck (lot fără corecție) cifrele coincid; pilotul MĂSOARĂ pe
o scenă cu lot corectat (agentul adaugă una: lot cu două intrări la prețuri
diferite, consum parțial) și consemnează diferența — N-r3 se închide ca
„confirmat, diferența = X pe scena Y".

### B-D5 — Declarantul PLT (și INC, prin aceeași bază)

Per linie de defalcare (`Valoare > 0`), O mișcare:

- **Plata normală** (contrapartida e Partener/Angajat): `La` = `{ Cont =
  ContImplicit al primitorului sau fallback-ul regulii (401), Partener =
  PrimitorId, Unitate = partida nominalizată }`, `DeLa` = `{ Cont =
  ContImplicit al predatorului (5121/5311), Gestiune = PredatorId }`;
  `Valoare = v`.
- **Nominalizarea partidei (TR-D2a)**: dacă `Autogenerat && DocumentSursaId
  != null` (plata născută din FCT), partida = a documentului-sursă pe contul
  liniei: `Unitate.DeschidePartida(cont, partener, DocumentSursaId, dataSursei)`
  — ACEEAȘI funcție deterministă pe care o va folosi și declarantul FCT când
  își deschide partida, deci id-urile coincid fără registru; plafonul =
  `RestPartidaSursa`; suma peste plafon merge pe o partidă NOUĂ deschisă de
  plată (`DeschidePartida(cont, partener, plata.Id, data)`), ca azi excedentul
  rămâne avans. Fără sursă: plata deschide propria partidă (avans dat). FIFO
  automat pe partidele deschise ale `(Cont, Partener)` NU intră în pilot
  (nu există politică azi care să-l ceară; rămâne la TR-D9 cu documentul
  `Împerechere`) — declarat.
- **Viramentul** (ambele laturi `ContPropriu`): `La` = `{ Cont = contul
  tranzit din regula de virament (581, `SursaCont.TipMaterial`), Gestiune =
  contul propriu al documentului (`GetContPropriuId` de azi: pentru `Plata`
  = PredatorId) }`, `DeLa` = `{ Cont = ContImplicit(Predator), Gestiune =
  PredatorId }`. Fără partidă (581 n-are `RolTert`). Latura pereche
  (`Incasare` generată) rămâne un DOCUMENT propriu (trece testul copilului,
  090h) și se generează în continuare de motorul vechi; declarantul PLT nu
  o declară. Refuz `VIRAMENT_MIXT` când liniile nu sunt toate de natură
  `Virament` sau invers.
- Decizii: `ContRezolvat` ×2 per linie, `AlocareFifo(linie, partidaSursa,
  suma)` pentru partea nominalizată, `PartidaDeschisa(linie, partidaNoua)`
  pentru excedent/avans; ipoteze: `SoldUnitateCitit(partidaSursa,
  Sold(rest))`, `PerioadaDeschisa`, `VersiunePolitica`.
- Refuzuri: predatorul nu e cont propriu; primitorul nu e partener/angajat/
  cont propriu; `v ≤ 0`; predator = primitor.

`TotalStingere`, `Imperecheri`, `CapacitateStingere`, `SensDeStins`,
`PoateFiStins`, `SursaStingeriiAutomate` NU se folosesc și NU se ating: sub
TR-D2 stingerea e postarea care numește partida.

### B-D6 — Declarantul FCT

Per linie:

- **Net**: `La` = `{ Cont = contul debit al regulii pe natură (Serviciu/
  Cheltuiala ⇒ 6xx din TipMaterial; Imobilizare ⇒ 2xx, fallback 404 pe
  credit) sau, pentru natura `Stoc` (TR-D3, azi FĂRĂ regulă pe FCT): contul
  implicit al tipului (3xx) cu `Gestiune = PrimitorId`, `Produs`, `Unitate =
  Lot(lot născut de linie)`, `Cantitate = q` }`; `DeLa` = `{ Cont = fallback-ul
  regulii (401; 404 la imobilizări), Partener = PredatorId, Unitate = partida
  401 a facturii = `DeschidePartida(401, PredatorId, fct.Id, DataInregistrare)`,
  Gestiune = `GestiuniVirtuale.Furnizor` DOAR pe mișcările cu cantitate
  (N-D4: capătul virtual `−q` stă pe postarea de terț) }`; `Valoare = net =
  Valoare` culeasă după `Tva.Linie`/regim (Capitalizat: net × (1 + cota)).
  Regula TR-D3 „lotul are deja recepție pe aviz ⇒ 408 = 401" NU intră în
  pilot: liniile FCT își nasc lotul la culegere (`ILinieCareNasteLot`), deci
  recepția e întotdeauna a facturii — declarat, cu restanță la TR-D7.
- **Taxa**: `Tva.PeDocument(linii (id, net, regim, cota), Deductibil,
  rotunjire)` decide taxa per cotă și o repartizează per linie (Hamilton pe
  semn) — N-r4; pe factura primită taxa CULEASĂ (`ValoareTva ≠ 0`,
  `pastreazaTvaCules`) e AUTORITARĂ: se validează cu `Tva.ValideazaData`
  contra celei calculate per cotă cu toleranța `TolerantaTaxa`, niciodată
  recalculată (090j). Valoarea toleranței la pilot: `0,01 × numărul liniilor
  cotei` (deriva maximă explicabilă prin rotunjirea per linie de azi),
  constantă în declarant, MARCATĂ ca rând de politică pentru TR-D7
  (`PoliticaTva.TolerantaTaxa`). Taxa posteză per linie: `La` = `{ Cont =
  ContTvaDeductibil al tipului (4426), CodTva = (tip, Achizitie, Taxa),
  PerioadaDeclarare, Partener = partenerul fiscal }`, `DeLa` = 401/partida
  (ca la net); `TaxareInversa` ⇒ `DeLa` = `{ ContTvaColectat (4427), CodTva
  = (tip, Livrare, Taxa) }` fără partidă; `Capitalizat`/`Scutit`/
  `Neimpozabil` ⇒ fără mișcare de taxă. Postarea de net a liniei poartă
  `CodTva = (tip, Achizitie, Baza)`, `PerioadaDeclarare` și `Partener` fiscal:
  jurnalul TVA e proiecția pe `CodTva` (090a), iar D394 cere partenerul pe
  fapta fiscală. `PerioadaDeclarare` = `RegistruTvaService.PerioadaDeclarare`
  (citită în operand ca fapt: `DeclarareIntarziata` + data).
- Partida: UNA, pe 401, cu datoria integrală (090h); `PartidaDeschisa(linie
  primă, partida)`. Refuzuri: `Numar` gol; predatorul nu e partener;
  primitorul nu e gestiune; `q ≤ 0`; linie de stoc fără lot; produs de alt
  tip decât linia; `TAXA_IN_AFARA_TOLERANTEI`.
- Copiii: NIR-ul conex NU se declară (TR-D3: dispare; la pilot motorul
  vechi îl generează în continuare și oracolul îl ABSOARBE în tranzacția
  FCT, B-D8); plata autogenerată rămâne document propriu.

`GestiuniVirtuale` (`Declaratii/GestiuniVirtuale.cs`): `Furnizor`, `Client`,
`Consum` ca `Guid` DETERMINISTE (SHA-256 din numele calificat, ca
`DeschidePartida`), structurale, fără rând de nomenclator; la TR-D7 devin
rânduri `DinSeed` cu aceleași id-uri dacă rapoartele le cer nume.
`Consum` există pentru simetrie, dar BCS folosește repartitorul real al
locului de consum (B-D4).

### B-D7 — Oracolul: maparea fizicii ca helper de test în ModelCheck

- ModelCheck (`nou/tools/ModelCheck`) primește folderul `Nucleu/` cu:
  `CubDinRegistre.cs` — portul FIDEL al lui
  `run-nucleu/fizica/pas1/02-transform.sql` (§B.3 al raportului fizicii:
  contabil → 2 postări cu `Partener` de pe oricare latură, `Gestiune` pe
  felul repartitorului, `Unitate` = documentul pe conturile cu `RolTert`;
  stoc → 1 postare cu `Cont` din `TipMaterial.ContImplicit`, `Gestiune` =
  repartitor, `Unitate` = lot, măsuri semnate; fiscal → 2 postări per rând,
  rol Bază/Taxă, `PerioadaDeclarare`; `Imperecheri` → tranzacție de
  împerechere cu `−S`/`+S` pe partide) peste rândurile de registru ale unui
  set de documente citite din ObjectSpace; produce `N.Tranzactie` +
  `N.Postare` (partidele re-cheiate prin `Unitate.DeschidePartida(cont,
  partener, documentDeschizator, data)` ca să fie comparabile cu ale
  declarantului).
- `Normalizari.cs` — funcțiile NUMITE ale diferențelor declarate (B-D8),
  fiecare cu numele tranșării: `TrD3AbsoarbeNirConex`, `TrD4UnificaStocCuContabil`,
  `TrD2NominalizeazaPrinImperechere`, `M6PartenerDoarPeTert`, `FiscalFaraTaxaZero`,
  `NrD4CapatVirtualInvizibil`… Aplicate DOAR pe oracol, în ordine fixă.
- `Comparabil.cs` — proiecția comparabilă `PostareComparabila(Cont, Latura,
  Gestiune, Produs, UnitateId, Partener, CodTva, PerioadaDeclarare, Analiza,
  Cantitate, ValoareSemnata, Linie)` și `Compara(asteptat, obtinut) →
  raport` cu diff-ul pe multiset (ce e în plus, ce lipsește), tipărit
  integral la cădere.
- `ProbeNucleu.cs` — `Proba(os, documente operate, nume)`: pentru fiecare
  document cu `Declarant != null`: `Contractare.Contracteaza(os, doc)`
  (contract acceptat cerut), oracolul = `CubDinRegistre` pe document (+ conexul
  lui) normalizat, `Check` pe egalitatea EXACTĂ a multiset-urilor; plus
  `Check` pe `Conservare.Verifica` gol, pe determinism (două contractări ⇒
  contracte egale structural), pe `NumaratorSql` ≤ prag pentru
  `Fapte.Operand` (o interogare per tabelă, nu per linie).
- Probele se leagă în scenele existente ale celor trei tipuri, pe AMBELE
  profiluri, imediat după operarea prin motorul vechi: BCS (`e2e 3c`, felia 6
  Api), PLT/INC (`e2e 3c`, felia Api Trz, felia 7 viramentul), FCT (`e2e 3c
  FCT → NIR`, felia 2 Api, P1 privat cu cele patru regimuri, override-ul de
  TVA). Fiecare probă nouă e `Check("NUC-<TIP>-<n>: …")`. Scenele existente
  NU se modifică în comportament (numărul de `Check`-uri de azi rămâne,
  toate verzi).
- ModelCheck referă `Atlas.Conta.Nucleu` direct (pentru tipurile din helper);
  e unealtă, nu consumator (090l îl permite: „un singur consumator" e despre
  producție).

### B-D8 — Diferențele DECLARATE (lista închisă a normalizărilor)

Oracolul se normalizează cu EXACT aceste transformări; orice reziduu = defect
sau propunere de normalizare nouă raportată main-ului (nu aplicată tăcut):

1. **TR-D3** — registrele NIR-ului conex autogenerat din FCT intră în
   tranzacția FCT-ului: `Cauza.Document = FCT`, `Cauza.Linie` = linia FCT a
   lotului (`Lot.LinieIntrareId` sau perechea 1:1 pe lot); NIR-ul nu mai are
   tranzacție proprie.
2. **TR-D4** — un rând de stoc `(lot, gestiune G, ±q, ±v)` și piciorul contabil
   pe ACELAȘI cont 3xx al aceleiași linii se unifică într-o singură postare
   `(3xx, Latura = semnul, G, produs, lot, |q|, |v|)`; pentru sink-ul BCS,
   rândul de stoc `+q` pe locul de consum și piciorul `D 6xx` al liniei se
   unifică în `D 6xx (loc, +q, lot, v)`.
3. **TR-D2a** — postările de terț ale stingătorului cu `Unitate = documentul
   propriu` plus tranzacția de împerechere `(−S pe stingător, +S pe stins)`
   se rescriu ca postări cu `Unitate = partida stinsă` pentru `S` și
   `Unitate = partida proprie` pentru rest; tranzacția de împerechere dispare.
4. **M6 / design §3** — `Partener` rămâne DOAR pe postările de pe conturi cu
   `RolTert` (din unitate) și pe cele cu `CodTva` (partenerul fiscal); pe
   celelalte se șterge (azi: „contrapartida pe fiecare latură").
5. **Fiscal** — cele două postări fiscale ale unui rând `RegistruTva` se
   rescriu ca atribute `CodTva`/`PerioadaDeclarare`/`Partener` pe postarea
   de net (rol Bază) și pe postarea de taxă (rol Taxă) ale aceleiași linii;
   `Data` fiscală = data documentului NU se compară (e atribut al
   documentului, citit din document, nu coordonată a postării — toate
   postările sunt datate ca tranzacția, `DATA_STRAINA`); rândul de taxă
   `0,00` nu produce postare.
6. **N-D4** — capătul virtual `−q` pe postarea de terț cu
   `Gestiune = Furnizor` NU există în oracol: comparația pe `Cantitate` se
   face DOAR pe postările cu `Unitate.Fel == Lot` (`Spatiu == Stoc`); pe
   restul `Cantitate` se compară ca 0. Confirmarea N-r2 = niciuna dintre
   proiecțiile de citire ale fizicii (portate în `Comparabil` ca filtre:
   fișă de magazie, PhysicalStock, FIFO, terți SAF-T) nu vede capătul
   virtual; dacă una îl vede, agentul raportează și main întoarce N-D4.
7. **N-r3 / N-r4** — pe scenele existente cifrele coincid; pe scenele
   adăugate special (lot corectat; TVA cu rotunjire per linie ≠ per cotă)
   diferența se CONSEMNEAZĂ numeric în contract, nu se normalizează.
8. **Dimensiunile** — `Analiza` ×6 a postării = cele ale laturii din registru
   (coalesce-ul de azi: explicit → linie → override → comun → implicit);
   declarantul reproduce ACELAȘI coalesce prin `DimensiuniResolver` pe fapte
   (funcție pură existentă), nu-l reinventează.

### B-D9 — Ce NU intră (amânări cu nume)

- Materializarea (`Tranzactie`/`Postare` ca entități EF, partiționarea,
  `PosteazaInCub` ca dată, apelul din `MotorOperare`): TR-D7.
- Ștergerea hook-urilor, a `TotalStingere`/`Imperecheri`, a conexului
  FCT→NIR: TR-D7/D9. Regularizarea `408 = 401` (NIR manual pe aviz): TR-D7.
- FIFO automat pe partide, documentul `Împerechere`, desfacerea: TR-D9.
- Numerotarea, scadența, mesajele după operare, copiii (NIR, plata,
  latura pereche): rămân ale motorului vechi; declarația nu produce documente.
- Localizarea refuzurilor și ordinea 401→400→404→403→422 pe uși: neatinse.
- Import1C NU se re-rulează (motorul vechi nu se atinge); proba supremă e a
  lui TR-D7.

### B-D10 — Regula de oprire a agenților

Un agent se oprește și raportează cu dovada, fără să normalizeze, când:
(a) un declarant ar avea nevoie de un câmp de frunză care nu e pe bază sau
pe interfață declarată; (b) o diferență față de oracol nu e în lista B-D8;
(c) o proiecție de citire vede capătul virtual N-D4; (d) forma cere
`is`/`switch` pe tip de document oriunde în afara membrului polimorf
`Declarant`; (e) `Fapte.Operand` nu poate citi ceva pe set (ar cere o
interogare per linie); (f) nucleul ar avea nevoie de un pachet sau de o
referință; (g) un `Check` existent al ModelCheck cade.

## Pașii (un agent per pas; main verifică independent și comite per pas)

1. **Schelet**: B-D1, B-D2, B-D3 — referința, slnx, `Declaratii/` (`IDeclarant`,
   `Operand` + faptele, `Contractare`, `GestiuniVirtuale`), `Fapte.Operand`,
   `Document.Declarant` (fără override-uri încă). Oprire: `dotnet build` pe
   soluția BackOffice verde, 0 avertismente noi; ModelCheck compilează;
   `git diff` pe Blazor.Server/WebApi/Client gol (în afara slnx).
2. **Oracolul**: B-D7 fără probe legate în scene — `CubDinRegistre`,
   `Normalizari`, `Comparabil`, `ProbeNucleu` + o auto-probă pe scena BCS
   bugetară (cubul brut al celor 2 rânduri de stoc + 1 notă = 4 postări cu
   coordonatele din §B.3, tipărite și verificate cu `Check`). Oprire:
   ModelCheck verde pe ambele profiluri, `Check`-urile de azi neschimbate.
3. **BCS**: B-D4 + probele NUC-BCS pe scenele lui (ambele profiluri) + scena
   lotului corectat (N-r3 măsurat). Oprire: egalitate exactă modulo B-D8.
4. **PLT/INC**: B-D5 + probele NUC-PLT (plata normală, plata din FCT cu
   nominalizare, încasarea, viramentul). Oprire: idem.
5. **FCT**: B-D6 + probele NUC-FCT (bugetar, privat cu patru regimuri,
   override-ul de TVA, factura doar de servicii, factura cu imobilizare).
   Oprire: idem; N-r2 confirmat sau raportat.
6. **Review advers** (agent separat, tier-ul main-ului): scenarii concrete —
   același lot pe două linii BCS; plata mai mare decât restul sursei; FCT cu
   două cote și taxa culeasă la limita toleranței; FCT cu linie de stoc și
   linie de serviciu pe același partener (o singură partidă); virament cu
   linie ne-virament; operand cu `Analiza` pe bugetar; determinism cu două
   instanțe de `Rotunjire`; `NumaratorSql` pe FCT cu 50 de linii; forma —
   e mai simplă decât hook-urile pe care le înlocuiește? (verdict explicit).
   Fix-urile le aplică main.
7. **Docs (main)**: `stare-curenta/domeniu-si-operare.md` (secțiunea
   nucleului: declaranții, operandul, ce probează), `dezvoltare-si-validare.md`
   (probele NUC-*, cum se citește un diff), `restante.md` (N-r2/r3/r4
   închise sau amendate + ce scoate review-ul), `istoric-plan-de-lucru.md`,
   `nucleu-transfer.md` Stare, CLAUDE.md §Stare/§Următorul pas (TR-D7),
   `Stare:` a contractului.

## Regula de oprire a feliei

- Forma e scrisă (B-D2…B-D6) și verdictul review-ului pe „mai simplă decât
  hook-urile" e explicit în contract, cu numărătoarea: linii ale celor trei
  declaranți + operand contra liniile hook-urilor de motor ale celor trei
  frunze + serviciile pe care le consumă.
- ModelCheck verde pe AMBELE profiluri, cu toate `Check`-urile de azi și cu
  probele NUC-BCS/PLT/FCT verzi; fiecare diferență față de registre e o
  normalizare din B-D8 sau o cifră consemnată (N-r3, N-r4).
- `dotnet test` pe nucleu verde (152 + ce aduce pilotul), 0 avertismente;
  testul de arhitectură neatins.
- `git diff --stat main -- nou/Atlas.Conta.BackOffice/Atlas.Conta.BackOffice.Blazor.Server
  nou/Atlas.Conta.BackOffice/Atlas.Conta.BackOffice.WebApi nou/Atlas.Conta.Client`
  GOL; XAF și React înghețate (090m); `MotorOperare.cs` și hook-urile
  frunzelor neatinse (diff gol pe ele).
- Review advers aplicat; docs din pasul 7 în commit-ul de închidere;
  `Stare:` ÎNCHISĂ cu data. Decizie proprie NU e necesară dacă forma ține
  (felia execută 090l); dacă forma NU ține, felia se închide ca ÎNTOARSĂ, cu
  o decizie nouă care amendează 090.
