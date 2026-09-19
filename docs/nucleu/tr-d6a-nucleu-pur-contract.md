# TR-D6a — Nucleul pur (contract)

Stare: **ÎNCHISĂ (2026-09-20)**, felia 29; branch `tr-d6a-nucleu-pur`;
`dotnet test` 152/152, review advers aplicat (1 MAJOR, 2 MEDII, 7 MINORE).
Amendamentele față de textul de mai jos, toate consemnate în `restante.md`
(N-r1…N-r9) și în cod: C1 se verifică per `Carte`; C5 cere și unitatea pe
contul postării, `Cantitate ≠ 0 ⇒ Produs ≠ null`, postările datate ca
tranzacția (`DATA_STRAINA`), tranzacția cu cel puțin o postare
(`POSTARI_LIPSA`); în `Storno` cauza altui document e permisă DOAR pe
postarea cu `Atribuit` (090i); `Sold` apără scara ca `Postare`; TVA per
linie se repartizează pe fiecare semn al taxei separat; cazurile `Decizie`
poartă `Linie`; `Motor.Transfera` ia `Mutare`; un pin FIFO cu măsură ≤ 0 e
eroare de apelant; listele primite se copiază (fără aliasing).
Decizia-mamă: 090 (`docs/decizii/090-nucleu-cub-de-postari.md`, regula
durabilă (a)–(l)); designul: `nucleu-cub-design.md` §2–§7, §10; ordinea și
regula de oprire: `nucleu-transfer.md` §4 („TR-D6a") și §4.1.

## Scop

Proiectul `nou/Atlas.Conta.Nucleu` (BCL, zero pachete, zero referințe la
EF/XAF/HTTP) cu tipurile cubului și regulile pure cerute de pilotul TR-D6b
(BCS, PLT, FCT), plus `Atlas.Conta.Nucleu.Teste` cu invarianții designului
§10 ca teste pe proprietăți cu generatoare proprii și testul de arhitectură.
Nimic din `Atlas.Conta.BackOffice`, `tools/` sau `Client` nu se atinge:
nucleul nu are încă niciun consumator (primul e `Module`, la TR-D6b).

## Testul contra invarianților

- I (orice postare are document): `Postare.Cauza` e obligatorie; `Deschidere`
  rămâne excepția declarată (TR-r10), verificată la nivel de fel de tranzacție.
- II (motorul nu cunoaște frunzele): nucleul nu cunoaște niciun tip de
  document; primește `Declaratie` (mișcări pe coordonate rezolvate) și
  întoarce `Contract`. Fără `is`/`switch` pe tipuri de document, fiindcă nu
  există.
- III (starea se citește de pe document → `Σ[Unitate]`): `Cub.Sold` e
  singura agregare; nicio structură a nucleului nu ține sold, rest sau total
  ca fapt.
- IV, V, VI: neatinse la acest pas (nucleul nu scrie și nu citește nicio
  bază; proba supremă Import1C e a lui TR-D7/D10).

## Deciziile (N-D1…N-D12)

### N-D1 — Așezarea și dependențele

- `nou/Atlas.Conta.Nucleu/` = folder de soluție, pe tiparul `BackOffice`:
  `Atlas.Conta.Nucleu.slnx`, `Directory.Packages.props` (CPM, pin-uri doar
  pentru teste), `Atlas.Conta.Nucleu/Atlas.Conta.Nucleu.csproj` (net10.0,
  `Nullable=enable`, `TreatWarningsAsErrors=true`, `ImplicitUsings=enable`,
  `Deterministic` implicit = true; FĂRĂ `PackageReference`/`ProjectReference`),
  `Atlas.Conta.Nucleu.Teste/Atlas.Conta.Nucleu.Teste.csproj` (xunit.v3 3.2.2 +
  `Microsoft.NET.Test.Sdk` 18.x + `xunit.runner.visualstudio` 3.x; fără
  FsCheck/CsCheck — generatoarele sunt proprii, §4.1).
- Fără `Directory.Packages.props` sau `Directory.Build.props` la `nou/`:
  ar activa CPM pe `tools/*` și le-ar rupe restore-ul.
- Testul de arhitectură (N-D12) e o probă, nu o convenție.
- Namespace-ul rădăcină: `Atlas.Conta.Nucleu`. Limba identificatorilor:
  română, ca în Module.

### N-D2 — Cubul: `Coordonate`, `Postare`, `Tranzactie`

- `Coordonate` (record imuabil): `Guid Cont`, `Latura Latura` (`Debit |
  Credit`, OBLIGATORIE pe orice postare — jurnalul TVA e proiecție pe
  `CodTva`, nu spațiu separat; fizica pas1 „Spatiu=3" era maparea registrului
  de azi), `DateOnly Data`, `Guid? Partener`, `Guid? Gestiune`, `Guid? Produs`,
  `Unitate? Unitate` (N-D6), `CodTva? CodTva` = `(Guid TipTva, SensTva Sens,
  RolTva Rol)`, `int? PerioadaDeclarare` (AAAALL), `Guid? Valuta`, `Carte
  Carte` (`Contabil | Fiscal`), `Analiza Analiza` (record cu 6 `Guid?`:
  CodFunctional, CodEconomic, SursaFinantare, UnitateOrganizatorica, Proiect,
  CentruCost).
- `Postare` (record imuabil): `Coordonate`, `decimal Cantitate` (scara 3),
  `decimal ValoareValuta` (2), `decimal Valoare` (2), `Cauza Cauza` =
  `(Guid Document, Guid? Linie)`, `Guid? Atribuit` (id-ul unei postări
  EXISTENTE, dat de operand). Constructorul REFUZĂ o măsură în afara scării
  (`ArgumentException`): în cub nu intră niciodată o a treia zecimală la bani.
  Nucleul NU generează id-uri de postare: identitatea o dă adaptorul la
  materializare (determinism, 090b).
- `Tranzactie` (record imuabil): `FelTranzactie Fel` (`Operare | Storno |
  Transfer | Deschidere`), `DateOnly Data`, `Guid? Document` (null doar la
  `Deschidere`), `IReadOnlyList<Postare> Postari`. `ScrisLa` e al
  materializării, nu al nucleului.
- `Spatiu(Postare)` e FUNCȚIE, nu câmp: `Unitate?.Fel == Lot ? Stoc :
  Contabil` (090g). Nicio structură nu îl stochează.

### N-D3 — Conservarea (structurală, în cod)

`Conservare.Verifica(Tranzactie) → IReadOnlyList<Refuz>` (listă goală =
balansată). Regulile, toate structurale, fără nicio politică:

- C1 valoare: `Σ (Latura == Debit ? +Valoare : −Valoare) = 0` peste TOATE
  postările (Contabil ∪ Stoc), la scara 2.
- C2 cantitate: `Σ Cantitate = 0` per `Produs`, peste postările cu
  `Cantitate ≠ 0` (N-D4 spune unde stă capătul virtual).
- C3 `Transfer`: în plus, `Σ Valoare = 0` per `(Cont, Latura)` și
  `Σ Cantitate = 0` per `(Cont, Produs)` (090f).
- C4 semn: în `Operare` și `Deschidere` orice `Valoare ≥ 0`; `Valoare < 0`
  e permisă DOAR în `Storno` și `Transfer` (090f). `Cantitate` e semnată
  întotdeauna.
- C5 forme: `Cantitate ≠ 0 ⇒ Gestiune ≠ null`; `Unitate.Fel == Lot ⇒ Produs
  ≠ null ∧ Gestiune ≠ null`; `Unitate.Fel == Partida ⇒ Partener ≠ null`
  (090d: fără partener nu există partidă); `Cantitate ≠ 0 ∧ Unitate == null
  ⇒ refuz` (nota contabilă nu postează cantitate fără unitate, 090c).
- C6 `Deschidere`: scutită de C1 și C2 (egalitatea bilanțului de deschidere
  e a comenzii de deschidere, nu a fiecărei tranzacții; TR-r10), supusă C4–C5.
- `Document` obligatoriu pe `Operare | Storno | Transfer`; toate postările
  unei tranzacții au `Cauza.Document == Tranzactie.Document`.

### N-D4 — Capătul virtual al cantității (de CONFIRMAT la TR-D6b)

Cantitatea intră/iese ÎNTOTDEAUNA dintr-o gestiune, reală sau virtuală
(090g). La recepție (`3xx = 401`), `+q` stă pe postarea de stoc (lot,
gestiunea reală), iar `−q` stă pe postarea de terț (`401`, unitatea =
partida) cu `Gestiune = Furnizor` (virtuală); simetric la vânzare (`Client`).
Consecințe: C2 se verifică peste toate postările, nu doar pe `Spatiu = Stoc`
(amendament de literă la 090g „a cantității doar pe Stoc": spiritul — cu
sink-uri — se ține, litera se lărgește); fișa de magazie filtrează pe
gestiuni reale și nu vede capătul virtual; partiția fizică rămâne pe lot
(TR-r3 o re-măsoară). Dacă pilotul FCT arată că forma asta strică un raport
al fizicii (13/13), TR-D6b întoarce N-D4, nu o normalizează.

### N-D5 — Rotunjirea și scările

- `Scara` (static): `Precizie = 18`, `Bani = 2`, `Pret = 6`, `Cantitate = 3`,
  `Procent = 4`; `Scara.EsteLa(valoare, scara)`.
- `Rotunjire` (clasă cu instanță, NU static): construită cu
  `MidpointRounding conventie` (dată de profil, înghețată per bază — 49e,
  51c; nucleul o PRIMEȘTE, nu o fixează); `Bani(v)`, `Pret(v)` (mereu
  `AwayFromZero`, ca azi), `Cantitate(v)`; contorul `JumatatiDeBan`
  (incrementat când `|v| % 0.01 == 0.005`) e al INSTANȚEI, iar `Contract`
  îl expune — nu mai există static de proces. `Module/Scara` rămâne
  neatins până la TR-D9 (consumatorul mapează).

### N-D6 — Unitatea (lot = partidă = fișă)

- `Unitate` (record): `Guid Id`, `FelUnitate Fel` (`Lot | Partida | Fisa`),
  `Guid Cont`, `Guid? Partener` (obligatoriu la `Partida`), `Guid? Produs`
  (obligatoriu la `Lot`), `DateOnly Deschisa`.
- `SoldUnitate` = `Sold` (N-D9) al unității; `Unitate.Raport(sold)`: `Lot` →
  `Valoare / Cantitate` (cost), `Partida` → `Valoare / ValoareValuta` (curs;
  `null` fără valută), `Fisa` → `Valoare` (rămasă). Raportul e CITIRE, nu
  fapt; `Lot.PretUnitar` de azi devine acest raport (090a).
- Deschiderea: `Unitate.DeschidePartida(cont, partener, documentDeschizator,
  data)` cu `Id` DETERMINIST din `(documentDeschizator, cont)` (SHA-256 →
  Guid): același document deschide aceeași partidă la orice re-rulare, fără
  id furnizat (090d: „identitate proprie", atributele `(Cont, Partener,
  document deschizător)`). Loturile și fișele se nasc la culegere cu id dat.

### N-D7 — Nominalizarea FIFO și evaluarea ieșirii

- `Fifo.Nominalizeaza(decimal cerere, IReadOnlyList<Disponibil> candidati,
  IReadOnlyList<Pin> pinuri) → Nominalizare(alocari, ramas)` —
  `Disponibil = (Unitate, decimal Masura)`, candidații ordonați de nucleu
  pe `(Deschisa, Id)`; pin-urile (unitatea numită pe linie, 090c) se
  consumă ÎNTÂI, fără cădere pe FIFO când pin-ul n-ajunge
  (`DescarcareService.cs:89-105`), restul cererii pe FIFO; plafonul e
  disponibilul; `ramas > 0` se ÎNTOARCE (tolerant), apelantul decide: refuz
  (stoc) sau partidă nouă pe stingător (090e). Aceeași primitivă pentru lot
  (măsura = cantitate) și partidă (măsura = restul în valoare).
- `Evaluare.Iesire(SoldUnitate inainte, decimal cantitate, Rotunjire r) →
  decimal`: `cantitate == inainte.Cantitate ? inainte.Valoare :
  r.Bani(cantitate × inainte.Valoare / inainte.Cantitate)` — pe raportul
  CURENT, în secvență, ultima ia restul (090j). Precondiții ca refuz de
  domeniu: `0 < cantitate ≤ inainte.Cantitate`, `inainte.Cantitate > 0`.
  Diferență DECLARATĂ față de azi: motorul vechi evaluează cu
  `Lot.PretUnitar` înghețat și substituie doar la golire
  (`StocService.cs:41`); pilotul BCS va arăta diferența pe loturile cu
  corecție, ca ieșire a lui TR-D6b, nu ca defect al nucleului.

### N-D8 — `Repartizeaza` (Hamilton, ierarhic)

- `Repartizare.Hamilton(decimal total, IReadOnlyList<decimal> ponderi, int
  scara) → decimal[]`: cote la scara dată, `Σ = total` exact, fiecare cotă
  la cel mult o unitate de scară de proporția exactă; restul de unități se
  dă cotelor cu fracția cea mai mare, la egalitate primei în ordine.
  `ponderi ≥ 0`, `Σ ponderi > 0` (altfel `ArgumentException` — eroare de
  apelant, nu refuz de domeniu); `total` de orice semn (cotele iau semnul
  totalului).
- `Repartizare.Hamilton(total, IReadOnlyList<IReadOnlyList<decimal>>
  grupuri, scara)`: întâi între grupuri (pe Σ ponderi per grup), apoi în
  interiorul fiecărui grup — exact per coordonată, apoi în interior (090j).
- Singura primitivă de distribuție a nucleului; înlocuiește „ultima ia
  restul" din `AsamblareApply.cs:384-391` când ASM migrează (TR-D7).

### N-D9 — `Cub.Sold` (citirea = adunare)

- `Sold` (record): `Debit`, `Credit`, `Cantitate`, `ValoareValuta`; `Net =
  Debit − Credit`; `operator +`. Restul partidei = `Net` cu sensul dat de
  rolul contului (citire a consumatorului).
- `Cub.Sold<TCheie>(IEnumerable<Tranzactie>, Func<Postare, TCheie> cheie,
  Func<Postare, bool>? filtru, DateOnly? panaLa, bool includeTransfer)` →
  `IReadOnlyDictionary<TCheie, Sold>`. Intrarea e tranzacții, nu postări:
  excluderea felului `Transfer` din rapoartele pe cont (090f) e un parametru,
  nu un flag pe postare. `Storno` se adună ca orice tranzacție.
- Invariantul snapshot-ului ca lemă: `Sold(≤ t) = Sold(≤ t0) + Sold(t0 < d
  ≤ t)` pentru orice `t0 < t`.

### N-D10 — Stornoul

`Storno.Inverseaza(IEnumerable<Postare> cauzateSiAtribuite, Guid document,
DateOnly data) → Tranzactie(Fel = Storno)`: fiecare postare cu `Cantitate`,
`ValoareValuta`, `Valoare` negate, aceleași `Coordonate` cu `Data = data`,
aceeași `Cauza` și același `Atribuit` — FĂRĂ schimb de latură (ca azi,
`MotorOperare.cs:660-672`). Selecția „cauzat ∪ atribuit" (postările cu
`Cauza.Document == document` ∪ cele cu `Atribuit` în id-urile primelor) e a
apelantului, care are id-urile; nucleul oferă `Storno.Selecteaza(postari cu
id, document)` peste perechi `(Guid Id, Postare)`. Data ≥ data
tranzacției stornate e gard de admisibilitate (perioadă), nu al nucleului.

### N-D11 — `Declaratie`, `Motor`, `Contract`

- `Miscare` (record): `Capat DeLa`, `Capat La`, `Cantitate`, `ValoareValuta`,
  `Valoare`, `Cauza`; `Capat` = `Coordonate` fără `Latura` și fără `Data`.
  Motorul o traduce în DOUĂ postări: `La` cu `Debit` și `+Cantitate`, `DeLa`
  cu `Credit` și `−Cantitate` (același `Valoare`, `ValoareValuta`). O
  tranzacție construită numai din mișcări e balansată prin construcție;
  `Conservare.Verifica` rămâne proba independentă.
- `Declaratie` (record): `Guid Document`, `DateOnly Data`,
  `IReadOnlyList<Miscare> Miscari`, `IReadOnlyList<Decizie> Decizii`,
  `IReadOnlyList<Ipoteza> Ipoteze` — declarantul (TR-D6b) pune aici ce a
  decis cu primitivele N-D7/N-D8 și ce stare a citit.
- `Motor.Opereaza(Declaratie, Rotunjire) → Contract`: asamblează EXACT o
  `Tranzactie` de fel `Operare`, rulează `Conservare.Verifica`, întoarce
  `Contract { Tranzactie, Decizii, Ipoteze, JumatatiDeBan }` sau
  `Contract.Refuzat(Refuzuri)`. Funcție pură: aceeași declarație ⇒ același
  contract (egalitate structurală).
- `Decizie` și `Ipoteza` sunt ierarhii de record-uri închise (nu perechi
  cheie–text): la D6a doar cazurile cerute de pilot — `AlocareFifo(unitate,
  masura)`, `ValoareIesire(unitate, cantitate, valoare)`,
  `PartidaDeschisa(unitate)`, `ContRezolvat(linie, cont, sursa)`;
  `SoldUnitateCitit(unitate, sold)`, `PerioadaDeschisa(an, luna)`,
  `VersiunePolitica(nume, valabilDeLa)`. TR-D6b adaugă, nu redefinește.
- `Refuz` (record): `string Cod`, `string Mesaj`, `Guid? Linie` — fără
  localizare, fără HTTP; codurile sunt stabile (`CONSERVARE_VALOARE`,
  `CONSERVARE_CANTITATE`, `SEMN_NEGATIV`, `UNITATE_LIPSA`, `STOC_INSUFICIENT`…).

### N-D12 — Invarianții ca teste și testul de arhitectură

Testele pe proprietăți rulează pe generatoare proprii (`Gen`: `Random` cu
sămânță fixă + N ≥ 500 cazuri per proprietate; la cădere raportul tipărește
sămânța și cazul). Mapa invariant → test (design §10):

1. **Compilează fără EF/XAF/HTTP**: `Arhitectura`: fiecare
   `GetReferencedAssemblies()` al assembly-ului `Atlas.Conta.Nucleu` are
   numele `System.*`, `netstandard` sau `mscorlib`; `.csproj`-ul nu conține
   `PackageReference`/`ProjectReference` (citit ca XML).
2. **Un document operat = exact o tranzacție balansată per spațiu**:
   `Motor.Opereaza` pe declarații generate ⇒ o singură `Tranzactie` de fel
   `Operare` cu `Verifica` gol; orice perturbare a unei singure postări
   (valoare, cantitate, latură) ⇒ `Verifica` ne-gol; `Transfer` generat cu
   `Σ per (Cont, Latura) = 0` trece, cu o postare mutată pe alt cont pică.
3. **Storno = inversul exact al cauzat ∪ atribuit**: `Sold` pe ORICE
   proiecție de coordonate după `T ∪ Storno(T)` = `Sold` dinainte de `T`;
   postările atribuite (reevaluare simulată) intră în selecție; `Storno` ∘
   `Storno` = originalul (mai puțin data).
4. **Cantitate zero pe unitate ⇒ valoare zero**: secvențe generate de
   intrări (cantitate, valoare la scară) și ieșiri prin `Evaluare.Iesire`
   pe raportul curent; ori de câte ori `Σ Cantitate = 0`, `Σ Valoare = 0`;
   contra-proba: evaluarea cu preț înghețat (regula veche) ÎNCALCĂ
   proprietatea pe același generator (documentează diferența N-D7).
5. **Snapshot = Σ la graniță**: lema din N-D9 pe tranzacții generate cu
   date aleatoare și `t0 < t` aleatoare, inclusiv `Transfer` cu ambele
   valori ale lui `includeTransfer`.
6. **O schimbare de politică nu modifică postări din trecut**: (a) `Motor`
   e determinist (două rulări ⇒ contracte egale structural); (b) `Postare`,
   `Tranzactie`, `Coordonate`, `Contract` n-au niciun setter public și
   niciun membru mutabil (reflecție); (c) `Declaratie` nu are nicio cale
   spre politică — politica intră doar ca `Ipoteza.VersiunePolitica`, iar
   două declarații identice cu ipoteze diferite dau aceeași tranzacție.
   Restul invariantului (istoricul persistat nu se re-proiectează) e al
   materializării, TR-D7.
7. **Motorul reproduce baseline-ul Import1C**: NU e testabil în nucleul pur
   — e prin definiție proba supremă TR-D7/D10 pe o bază reală. Se declară
   aici ca AMÂNARE cu nume (nu se simulează cu un test de formă).

Plus proprietăți ale primitivelor: `Repartizare` (`Σ = total`, fiecare cotă
la ≤ o unitate de proporție, ierarhic ⇒ Σ per grup = Hamilton pe grupuri),
`Fifo` (Σ alocări + ramas = cerere; pin-urile întâi; nicio alocare peste
disponibil; ordinea `(Deschisa, Id)`), `Rotunjire` (contorul numără exact
jumătățile; `Bani` respectă convenția primită; `Pret` nu o urmează),
`Tva` (`Σ taxă per linie = taxa documentului per cotă`, exact).

## TVA la D6a (pilotul FCT)

`Tva` (static, pe enumuri PROPRII `RegimTva { Normal, TaxareInversa,
Capitalizat, Scutit, Neimpozabil }`, `DirectieTva { Deductibil, Colectat }`
— nucleul nu poate referi Module; duplicarea e declarată și moare la TR-D9):
- `Tva.Linie(net, regim, cota, directie) → (Valoare, Taxa)` NErotunjit —
  portul ramurilor din `TvaService.cs:54-74`.
- `Tva.PeDocument(linii (id, net, regim, cota), directie, Rotunjire) →
  (taxa per cotă, taxa per linie)`: taxa se DECIDE și se rotunjește pe
  document × cotă, apoi se POSTEAZĂ per linie prin `Repartizare.Hamilton`
  peste bazele liniilor (090j). Diferență declarată față de azi (rotunjire
  per linie, `TvaService.cs:79-80`): pilotul FCT o măsoară.
- `Tva.ValideazaData(taxaData, taxaCalculata, toleranta) → Refuz?`: pe
  facturile primite taxa e dată, validată cu toleranță din politică,
  niciodată recalculată (090j). Azi nu există toleranță numerică
  (`pastreazaTvaCules` păstrează orice cifră ≠ 0): toleranța devine
  parametru al operandului; valoarea ei e politică (TR-D6b).

## Pașii (un agent per pas; main verifică independent și comite)

1. **Schelet + cub + conservare**: N-D1, N-D2, N-D3 (+ N-D4 ca regulă C2),
   `Scara`/`Rotunjire` (N-D5), `Refuz`, generatoarele `Gen`, testul de
   arhitectură (12.1), proprietățile conservării pe tranzacții construite
   din mișcări (parte din 12.2) și `Rotunjire`. Oprire: `dotnet test` verde;
   `Atlas.Conta.Nucleu.csproj` fără nicio referință.
2. **Repartizare + TVA**: N-D8, secțiunea TVA; proprietățile lor.
3. **Unitate + FIFO + evaluare + Sold + Storno**: N-D6, N-D7, N-D9, N-D10;
   invarianții 3, 4, 5 ca proprietăți, cu contra-proba prețului înghețat.
4. **Motor + Contract**: N-D11; invarianții 2 (integral) și 6; `Decizie`/
   `Ipoteza` închise.
5. **Review advers** (agent separat, tier-ul main-ului): scenarii concrete —
   scară scăpată prin `Sold` (adunare la scară mai mare), Hamilton cu
   `total < 0` sau ponderi egale, FIFO cu pin peste disponibil, `Evaluare`
   cu rest sub un ban, `Storno` cu `Atribuit` spre postare din altă
   tranzacție, `Transfer` cu C3 satisfăcută dar C1 nu, `Deschidere` cu
   document; onestitate > liniște, fără cosmetice. Fix-urile le aplică main.
6. **Docs (main)**: `stare-curenta/dezvoltare-si-validare.md` (proiectul,
   comanda `dotnet test`, verificarea proporțională „Nucleu → `dotnet
   test` + arhitectură"), `domeniu-si-operare.md` (secțiune „Nucleul pur",
   fără implementare în motor încă), `restante.md` (invariantul 7 amânat +
   ce scoate review-ul), `istoric-plan-de-lucru.md`, `docs/nucleu/*.md`
   `Stare:`, CLAUDE.md §Stare/§Următorul pas (TR-D6b).

Regula de oprire pentru agenți: dacă o decizie de mai sus nu se poate
implementa ca atare (contradicție internă, imposibil la scară, BCL
insuficient), agentul se oprește și raportează cu dovada — NU normalizează
tăcut și NU adaugă pachete.

## Ce NU intră (amânări cu nume)

- Regulile pure ale celorlalte tipuri (`AmortizareService.*`,
  `InchidereTvaService.*`, `PerioadaService.Verifica`, `SolduriService.*`,
  `Potrivire`, `DimensiuniResolver`): intră cu tipul lor la TR-D7 (090l);
  `Potrivire` rămâne în Module ca să nu atingă `F24-P*` din ModelCheck.
- Declaranții BCS/PLT/FCT, adaptorul `Fapte`, referința Module → Nucleu:
  TR-D6b.
- Entitatea EF `Postare`, partiționarea, `Sold` persistat: TR-D7/D8.
- Reevaluarea cu `Atribuit` (distribuția peste postările unității) și
  documentul `Împerechere`: TR-D9; nucleul are DOAR primitivele
  (`Repartizare`, `Atribuit` pe postare, `Transfer` în conservare).
- Invariantul 7 (Import1C): TR-D7/D10 — restanță cu nume.
- Toleranța TVA ca valoare de politică; `CodTva` versionat ca rând de
  politică: TR-D6b.

## Regula de oprire a feliei

- `dotnet build` + `dotnet test` verzi pe `nou/Atlas.Conta.Nucleu/`, cu
  testul de arhitectură și invarianții 1–6 ca proprietăți (N ≥ 500 cazuri
  fiecare), invariantul 7 declarat amânat.
- `git diff --stat main -- nou/Atlas.Conta.BackOffice nou/tools
  nou/Atlas.Conta.Client` GOL (ModelCheck, Module, WebApi, Client neatinse);
  ModelCheck nu se re-rulează fiindcă nu s-a atins nimic din ce probează.
- Review advers aplicat; docs din pasul 6 în același commit de închidere;
  `Stare:` a contractului trecută pe ÎNCHISĂ cu data și numărul deciziei
  (decizia proprie NU e necesară: felia execută 090l; amendamentele de
  literă — N-D4, N-D7, TVA per document × cotă — se consemnează în
  `restante.md` ca puncte de confirmat la TR-D6b, nu ca decizie nouă).
