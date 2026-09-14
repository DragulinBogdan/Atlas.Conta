# Pasul 5, felia 26 — Imobilizări și amortizare, contabil și fiscal (contract)

Data fixării: 2026-09-14 (după două runde de întrebări cu owner-ul, 12 + 9,
toate tranșate). Decizia rezultată se scrie la închidere (087).
Deciziile F26-D1…D16 sunt PIN-UITE — agenții de implementare nu le redeschid; orice nepotrivire cu realitatea codului se RAPORTEAZĂ, nu se
normalizează tăcut.

## Scop

Imobilizările sunt modul ABSENT: `NaturaClasa.Imobilizare` e doar eticheta la
achiziție (FCT cu linie de clasă F postează contul implicit al tipului
material contra 404, nu naște lot, nu trece pe NIR — `LoturiCulegereService.cs:147`,
`ProfilPrivat.cs:975-987`), iar decizia 9 le-a lăsat „modul separat, prin
note". Felia livrează evidența imobilizărilor pe tiparele existente: fișa ca
nomenclator subțire, mișcările și parametrii ca evenimente operate prin motor,
amortizarea lunară ca document GENERAT (tiparul ITV) cu TREI cifre pe lună
(contabilă, fiscală, deductibilă), toate faptele per fișă într-un singur
registru append-only, conturile și regulile de deductibilitate exclusiv din
politică, versionate în timp.

Ce au arătat explorările (2026-09-14, patru rapoarte read-only):

- **Motorul n-are nimic pentru imobilizări** (grep `Imobiliz`/`Amortiz` în
  `Module/**`: doar enum-ul și rândurile de seed). Ambele planuri au tot ce cer
  OMFP 1802 / OMFP 1917: 20x/21x/23x, 280x/281x, 6811, 6583, 7583, 105, 8035;
  bugetar 681.01.00, 280.xx.00/281.xx.00, 691.00.00.
- **ITV este o `NotaContabila`** cu liniile calculate de `InchidereTvaService`
  din registre, operată prin `MotorOperare` obișnuit; conturile din
  `PoliticaInchidereTva` (4 FK-uri); „luna liberă" = nicio ITV vie
  (Draft/Operat) în lună; raport (`Previzualizeaza`) și comandă (`Incearca`)
  diferă printr-un bit; anti-stale la operare = același calcul.
- **Registrele sunt trei**, materializate în `MotorOperare.Opereaza`, șterse în
  `AnuleazaOperarea`, inversate în `Storneaza`. Un al patrulea cere dispecer
  în cele trei puncte (D3).
- **1C (Flax)**: 127 fișe, 119 cu amortizare per activ. Cardul poartă doar
  identitatea; parametrii sunt rânduri de registru DATATE, scrise de
  documente (`ParametriiDeAmortizareMF`) — forma pe care o adoptăm (D2).
  `PunereaInEvidentaMF` NU postează. Formula, verificată LA BAN pe toate
  activele integral amortizate: `cota = ROUND(valoare_de_amortizat /
  luni_rămase, 2)` FIXATĂ la ultimul eveniment, `suma = MIN(cota, rest)`,
  ultima lună absoarbe restul (lună în plus: Citan 33 × 665,17 + 0,07; lună
  scurtă: Zebra 35 × 95,98 + 95,81). Prima amortizare = luna DE DUPĂ punerea
  în funcțiune (51/51). Modernizarea/reevaluarea: `valoare_nouă = brut −
  amortizat + increment`, `luni_rămase = durata − consumate`. Casarea: 281x =
  21x pe cumulat, restul pe 658.3; luna ieșirii neamortizată. Toate liniare;
  niciun obiect fiscal; `TransferMF` 0 rânduri.
- **Catalogul HG 2139/2004** (`anaf/catalog-mf.csv`, 598 rânduri: cod |
  denumire | bandă de ani, grupele fără bandă) = sursa duratei normale de
  funcționare, obligatorie fiscal (Cod fiscal art. 28).
- **SAF-T D406 Assets** (anual): derivabil integral din registrul D2.

## Deciziile

### F26-D1 — Fișa = nomenclator subțire `Imobilizare`; parametrii sunt fapte datate în registru

```csharp
public class Imobilizare : BaseObject, ICuCautare {
    NumarInventar (Cod, unic), Denumire, Cautare;
    TipMaterialId / TipMaterial            // clasă F; contul de imobilizare = TipMaterial.ContImplicit (26b)
    ClasificareId / Clasificare            // ClasificareImobilizari (catalog HG 2139), opțional; dă banda duratei fiscale
    LocId / Loc : Repartitor               // locul: gestiune / unitate internă / angajat — dimensiunea Repartitor a notelor
    CentruCostId / CentruCost : Repartitor? // opțional, calitatea CentruCost
    ResponsabilId / Responsabil : Angajat? // evidență
    Stare : StareImobilizare               // Noua | InFunctiune | Iesita — materializată de hook-ul D3
    DataPunereInFunctiune, DataIesire : DateOnly?   // materializate de PIF / CAS
}
```

Ce NU e pe fișă, deliberat: metoda, durata, valoarea reziduală, categoria
fiscală, valoarea, furnizorul, data achiziției — toate sunt FAPTE cu dată,
scrise de documente în registru (D2), ca în 1C. Cardul are identitate +
starea materializată; „parametrii curenți" = proiecție (ultimul eveniment),
expusă pe DTO și `[NotMapped]` pe DetailView-ul XAF. Astfel revizuirea
duratei, schimbarea categoriei fiscale sau a metodei nu sunt edit-uri de
nomenclator care rup reproductibilitatea calculului, ci evenimente (D5).

Editarea fișei (gardian): `NumarInventar`, `Denumire`, `Responsabil`,
`CentruCost`, `Clasificare` oricând; `TipMaterial` doar în `Noua`; `Loc` în
`Noua` și `InFunctiune` (transferul, D8); nimic în `Iesita`. Ștergere doar în
`Noua`, fără rânduri de registru.

**Contabil vs fiscal = doi parametri și două cifre, un singur registru, nu
două registre.** Amortizarea fiscală nu postează și n-are document propriu;
ar fi „registru fără document" (I). E însă FAPT lunar înghețat (nu recalcul):
rândul lunar poartă cifra fiscală și cea deductibilă lângă cea contabilă,
calculate de aceeași funcție din aceiași parametri datați și din regulile
valabile la acea dată (D4). Regulile viitoare (plafon schimbat prin lege în
timpul vieții activului) schimbă doar lunile viitoare, prin rând nou de
politică cu `DeLa`; nimic din trecut nu se rescrie.

### F26-D2 — Un singur registru: `RegistruImobilizari`, append-only, evenimente + luni

```csharp
public class RegistruImobilizari : BaseObject {
    Data : DateOnly;  ImobilizareId;  Fel : FelMiscareImobilizare;   // Intrare | Modernizare | Revizuire | Amortizare | Iesire | Reevaluare (rezervat)
    // efecte valorice, semnate; toate 18,2 (Luni: int)
    Valoare                 // brut contabil: Intrare +, Modernizare +, Iesire −brut
    ValoareFiscala          // brut fiscal (poate diverge: reevaluare nerecunoscută, costuri nedeductibile); Intrare +, Modernizare +, Iesire −
    Amortizare              // cumulat contabil: Amortizare +, Iesire −cumulat, Intrare +inițială (deschidere)
    AmortizareFiscala       // idem fiscal
    AmortizareDeductibila   // idem deductibil (rezultatul regulilor D4 la data rândului)
    Luni                    // luni amortizate: Amortizare 1, Intrare = LuniAmortizateInitial, altfel 0
    // parametri — doar pe evenimente (Intrare, Revizuire; Modernizare/Reevaluare îi pot repeta), null pe Amortizare/Iesire
    Metoda : MetodaAmortizare?;  DurataLuni : int?;  ValoareReziduala : decimal?
    MetodaFiscala : MetodaAmortizare?;  DurataFiscalaLuni : int?
    CategorieFiscala : CategorieFiscala?;  UtilizareExclusiva : bool?
    RepartitorId (locul la data faptului);  DocumentId;  DetaliuId;  Storno
}
```

Situația la o dată = sumele coloanelor valorice + parametrii ultimului
eveniment ≤ dată (coalesce înapoi pe evenimente, pentru că un `Modernizare`
poate lăsa parametrii null = „neschimbați"). Totul e reproductibil din
registru: cifrele ȘI intrările calculului. Fișa (raport), registrul
imobilizărilor (raport), D101 (restanță) și SAF-T Assets (restanță) = sume
peste el. Scris DOAR de motor prin D3; protejat de gardian ca celelalte
registre.

De ce un singur registru și nu două (mișcări / amortizări): aceeași cheie
(fișă × dată), același ciclu de viață (anulare șterge, storno inversează),
aceeași citire (situația la o dată e o singură sumă); separarea ar dubla
protecția, hook-ul și proiecțiile fără să câștige nimic. De ce nu o proiecție
peste `RegistruContabil` ⋈ frunze: PIF/Revizuire nu postează, deci valoarea de
intrare, parametrii și lunile n-ar avea niciun registru (III). Amortizarea
lunară apare și în `RegistruContabil` (nota 68x = 28x cu `DetaliuId`): două
registre, două fapte (precedentul recepției), atribuirea per fișă e a lui
`RegistruImobilizari`.

### F26-D3 — Contractul motorului: `IDocumentCuRegistruPropriu`, un singur hook, trei puncte

```csharp
public interface IDocumentCuRegistruPropriu {              // BusinessObjects/Comun/Interfete.cs
    void MaterializeazaRegistrul(IObjectSpace os);          // Opereaza: după registrele nucleului, înainte de conex/secundar
    void EliminaRegistrul(IObjectSpace os);                 // AnuleazaOperarea: lângă ștergerea celor trei registre
    void StorneazaRegistrul(IObjectSpace os, DateOnly data);// Storneaza: lângă rândurile inverse
}
```

`MotorOperare` primește EXACT trei dispecere `if (doc is IDocumentCuRegistruPropriu r) …`
(≤ 15 linii, interfață, nu tip — 25b/32a); `GardianEditare` primește
`RegistruImobilizari` în lista registrelor protejate (≤ 5 linii, precedentul
86f). Nucleul `Document`/`DocumentDetaliu` NEATINS. Dependențele (AMO
ulterioară operată, ieșire operată) le refuză FRUNZA în
`EliminaRegistrul`/`StorneazaRegistrul` cu `OperareException`. Fiindcă
`MotorOperare` e atins, închiderea cere Import1C integral cu raport IDENTIC cu
baseline-ul F18 (Import1C nu generează PIF/CAS/AMO; diferența așteptată: zero).

### F26-D4 — Politica: conturi, reguli de deductibilitate versionate, catalogul

**`PoliticaAmortizare`** (precedentul `PoliticaInchidereTva`), cheiată pe tipul
material de clasă F, per profil:

```csharp
TipMaterialId (unic) → ContAmortizareId (28x), ContCheltuialaAmortizareId (6811 / 681.01.00),
                        ContCheltuialaCedareId (6583 / 691.00.00), DinSeed
```

Seed privat (`TipMaterial` de clasă F EXTINS de la 2 la 8; codul = simbolul
contului, 26b): 205→2805, 208→2808, 212→2812, 2131/2132/2133→2813, 214→2814,
toate cu 6811/6583; 211 (terenuri) FĂRĂ rând. Seed bugetar: `205.00.00`,
`208.01.00`, `208.02.00`, `214.00.00` + `213.01.00`, `213.03.00` (adăugate) →
280.05.00 / 280.08.00 / 281.03.00 / 281.04.00, cheltuială 681.01.00, cedare
691.00.00; `231.00.00`, `682.01.09` fără rând. Simbolurile se confirmă la
pasul 1 din CSV-urile planurilor; se raportează alegerea.

**`RegulaDeductibilitate`** — cadrul pentru evoluția legii, ca DATE cu
valabilitate în timp (IV: rândul parametrizează un mecanism din cod; un fel
nou de regulă = cod nou):

```csharp
public class RegulaDeductibilitate : BaseObject, ICuProvenienta {
    Categorie : CategorieFiscala          // Standard | VehiculPersoaneMax9Locuri | SediuSocialInLocuinta | … (enum, extins la nevoie)
    DoarNeexclusiv : bool                 // true = se aplică doar când activul NU e utilizat exclusiv în scopul activității
    Fel : FelDeductibilitate              // PlafonLunar | Procent
    Valoare : decimal                     // lei/lună, respectiv procent (0 = nedeductibil)
    DeLa : DateOnly;  PanaLa : DateOnly?  // valabilitatea legală
    Temei : string                        // „Cod fiscal art. 28 (14)"
    DinSeed
}
```

Mecanismul (`AmortizareService.Deductibil`): din regulile valabile la data
rândului lunar, potrivite pe `(Categorie, DoarNeexclusiv ⇒ !UtilizareExclusiva)`,
se aplică în ordine fixă `PlafonLunar` apoi `Procent`; per `(Categorie, Fel)`
câștigă rândul cu `DeLa` maxim ≤ data rândului lunar (`PanaLa` doar când legea
are capăt explicit); fără regulă → deductibil = fiscal; `DurataFiscalaLuni = 0`
→ fiscal 0. Seed privat (rândurile
de pornire; textul legii pe 2026 îl confirmă owner-ul la pasul 1 —):

| Categorie | DoarNeexclusiv | Fel | Valoare | DeLa | Temei |
|---|---|---|---|---|---|
| VehiculPersoaneMax9Locuri | true | PlafonLunar | 1 500 | 2012-02-01 | art. 28 (14); excepțiile (utilizare exclusivă: taxi, școală de șoferi, închiriere, agenți de vânzări, intervenție…) = `UtilizareExclusiva` pe fișă |
| SediuSocialInLocuinta | true | Procent | 0 | 2024-01-01 | art. 28 (4) — Legea 296/2023 |
| SediuSocialInLocuinta | true | Procent | 50 | 2026-01-01 | modificările 2026 (temeiul exact = text de seed, îl completează owner-ul la revizia seed-ului) |

Bugetar: zero rânduri (fără impozit pe profit). Regulile sunt configurabile
(`TipuriConfigurabile` primește `PoliticaAmortizare` și `RegulaDeductibilitate`;
grila comună `GrilaPolitica`, 84c), aliniate de seed doar pe `DinSeed` (84a).
O regulă modificată/ adăugată NU rescrie lunile deja postate (fapt).

**`ClasificareImobilizari`** — nomenclator de seed din catalog (nucleu, ambele
profiluri; CSV copiat în `DatabaseUpdate/SeedData/catalog-mf.csv`): `Cod`
(„2.2.9."), `Denumire`, `DurataMinAni`/`DurataMaxAni` (null pe grupe),
`Grupa` (prefixul), `Cautare`; `Dimensiune`-derived (cod + denumire +
căutare). La PIF/Revizuire: dacă fișa are clasificare, `DurataFiscalaLuni` în
afara benzii `[min×12, max×12]` = refuz 422 (durata normală e obligatorie
fiscal); fără clasificare = fără verificare. Implicitul duratei fiscale la
culegere = `min × 12` (ușa `api/implicite` pe ancora PIF nu se atinge:
implicitul e al DTO-ului de fișă, `GET api/imobilizari/{id}` întoarce banda).
Perechea de conturi NU e în catalog (spre deosebire de 1C): tipul material îl
alege utilizatorul.

Tot din seed: ancorele `PIF`/`CAS`/`AMO` în nucleu; `PoliticaNumerotare` per
profil; `TipMaterial` `7583` (natura Serviciu) pentru linia FCL a vânzării,
`791.00.00` pe bugetar. Fără `RegulaContare`/`RegulaStoc`/`PoliticaTva` pe
PIF/CAS/AMO. Niciun simbol de cont în cod în afara `DatabaseUpdate/`.
„Explică" (84) nu acoperă PIF/CAS/AMO (n-au `RegulaContare`, ca ITV).

### F26-D5 — `PIF`: evenimentele care nu postează — intrare, modernizare, revizuire

`PunereInFunctiune : Document, IDocumentCuRegistruPropriu`, `[TipDetaliu(typeof(PunereInFunctiuneDetaliu))]`.
`Predator` = unitatea internă, `Primitor` = locul — un PIF per loc; fișele de
pe linii au `Loc = Primitor`. `Numar` din `PoliticaNumerotare`.

```csharp
public class PunereInFunctiuneDetaliu : DocumentDetaliu {
    ImobilizareId;  Fel : FelLiniePif           // Intrare | Modernizare | Revizuire
    LinieSursaId : DocumentDetaliu?              // linia FCT de clasă F (evidență + prefill), Intrare/Modernizare
    // Valoare (bază): valoarea intrată / a modernizării; 0 la Revizuire. TipMaterial (bază) = al fișei.
    ValoareFiscala : decimal                     // implicit = Valoare; culeasă doar când diferă (cost nedeductibil)
    AmortizareInitiala, AmortizareFiscalaInitiala, LuniAmortizateInitial   // doar Intrare fără linie sursă (deschidere/migrare)
    // parametrii (D2) — obligatorii la Intrare și Revizuire, null la Modernizare (= neschimbați):
    Metoda?, DurataLuni?, ValoareReziduala?, MetodaFiscala?, DurataFiscalaLuni?, CategorieFiscala?, UtilizareExclusiva?
}
```

Reguli: `Intrare` cere fișă `Noua` și parametri completi; `Modernizare` și
`Revizuire` cer `InFunctiune`; `Valoare > 0` la Intrare/Modernizare, `= 0` la
Revizuire; `TipMaterial` = al fișei; `DurataLuni ≥ 0`, `ValoareReziduala < brut`;
banda catalogului (D4). Cu linie sursă: linia e a unei FCT `Operat`, de clasă
F, iar Σ `Valoare` a liniilor PIF nestornate pe aceeași linie sursă ≤ `Valoare`
a liniei (o linie cu cantitate 3 hrănește 3 fișe; „nu se culege dublu" =
prefill + plafon). Fără linie sursă: valoarea e culeasă (deschidere, producție
proprie, migrare 1C) și inițialele sunt permise. Cronologie: refuz dacă există
AMO `Operat` pentru o lună > luna PIF-ului.

Postare: NICIUNA în v1 (valoarea e pe 21x din FCT; ca BTR). Activele din 231 =
restanță F26-r1. Registru: `Intrare` / `Modernizare` / `Revizuire` cu efectele
și parametrii de mai sus, `Repartitor = Primitor`. Fișa: `InFunctiune`,
`DataPunereInFunctiune` (la Intrare). Anulare/storno: refuzate de frunză dacă
fișa are rânduri ulterioare nestornate; la Intrare readuc fișa în `Noua`.

### F26-D6 — `CAS`: ieșirea (casare / vânzare / lipsă), două note per fișă din politică

`IesireImobilizare : Document, IDocumentCuPostareExplicita, IDocumentCuRegistruPropriu`,
`Cauza : CauzaIesire` (Casare | Vanzare | Lipsa) pe antet; `Predator` = locul,
`Primitor` = unitatea internă.

`IesireImobilizareDetaliu : DocumentDetaliu, ILinieCuPostareExplicita { ImobilizareId; Fel : FelLinieIesire (AmortizareCumulata | ValoareRamasa); ContDebitId, ContCreditId, RepartitorDebitId, RepartitorCreditId }`

Culegerea = lista fișelor; serviciul (`AmortizareService.LiniiIesire`) produce
per fișă linia `AmortizareCumulata` (debit `ContAmortizare`, credit contul
implicit al tipului material) și linia `ValoareRamasa` (debit
`ContCheltuialaCedare`, credit contul implicit; omisă la net 0), `Repartitor`
= locul. `ValideazaOperare` recalculează din registru la `Data` și REFUZĂ dacă
liniile nu mai corespund (anti-stale, ca ITV). Cronologie: refuz dacă există
AMO `Operat` cu luna ≥ luna ieșirii (luna ieșirii nu se amortizează);
`MesajeDupaOperare` avertizează dacă luna precedentă n-are AMO operată.
Registru: un rând `Iesire` per fișă (−brut, −brut fiscal, −cumulat contabil /
fiscal / deductibil, `Luni` 0). Fișa: `Iesita`. Deductibilitatea valorii
rămase la ieșire (art. 28 (17): la vânzare integral, la casare condiționat) =
restanță F26-r2, cu locul pregătit (regula ar fi un `Fel` nou pe
`RegulaDeductibilitate`, aplicat rândului `Iesire`).

Vânzarea: FCL separată (linie pe tipul material `7583`), NELEGATĂ în v1
(restanță F26-r3, tiparul `DviFactura`).

### F26-D7 — `AMO`: amortizarea lunară generată; `AmortizareService` = singura aritmetică, trei cifre

`AmortizareLunara : Document, IDocumentCuPostareExplicita, IDocumentCuRegistruPropriu`;
`Predator = Primitor` = unitatea internă (parametru); `Data` = ultima zi a
lunii; `Autogenerat`; `PoateFiStins = false`. Derivă din `Document`, nu din
`NotaContabila` (nu apare pe ușa NTC, fără filtru `is`).

`AmortizareLunaraDetaliu : DocumentDetaliu, ILinieCuPostareExplicita { ImobilizareId; ValoareFiscala; ValoareDeductibila; conturile și repartitorii expliciți; DimensiuniCulese() ⇒ Repartitor = Loc, CentruCost }` — `Valoare` (bază) = amortizarea contabilă postată.

`Motor/AmortizareService.cs` (static, insulă — II):

- `Situatie(os, imobilizareId, laData)` — sumele și parametrii D2.
- `CotaLunara(valoareDeAmortizat, luniRamase, metoda, luniDeLaPunere, brut)` —
  funcție PURĂ, singura aritmetică (42c), aplicată de trei ori pe lună:
  contabil `(brut − reziduală − cumulat)` la ultimul eveniment / `(DurataLuni −
  luni)`, fiscal `(brutFiscal − cumulatFiscal) / (DurataFiscalaLuni − luni)`,
  apoi deductibil = `Deductibil(fiscal, reguli la dată)`.
  - `Liniara`: `cota = RotunjesteBani(valoareDeAmortizat / luniRamase)`, citită
    LA ULTIMUL EVENIMENT (Intrare/Modernizare/Revizuire/Reevaluare) și FIXĂ
    până la următorul; `suma = min(cota, restul curent)`; `luniRamase = 0` cu
    rest > 0 (rotunjire în jos) → lună suplimentară cu restul; rest 0 = nimic
    (fișa rămâne `InFunctiune`, integral amortizată). „Repornirea" duratei
    (1C, invertorul) = `Revizuire` cu durata nouă — durata e fapt datat.
  - `Accelerata`: primele 12 luni de la punerea în funcțiune `RotunjesteBani(brut
    × 50 % / 12)`; apoi liniar pe restul / lunile rămase.
  - `Degresiva` (AD1; AD2 = restanță F26-r4): k = 1,5 (2–5 ani), 2,0 (5–10),
    2,5 (> 10); pe fiecare an `anual = rest_la_început_de_an × rata_liniară ×
    k`, trecere la liniar din anul în care `anual ≤ rest / ani_rămași`; lunar
    = anual / 12, ultima lună a anului absoarbe restul anului.
  - Rotunjirea = `Scara.RotunjesteBani` (convenția profilului, 51c; privat
    `AwayFromZero` = half-up-ul observat în 1C).
- `Analizeaza(os, an, luna)` — ordinea gardienilor (79a): fișă eligibilă fără
  `PoliticaAmortizare` → `FisaFaraPolitica` (numele); AMO vie în lună →
  `AmortizareVie`; AMO vie ulterioară → `NeCronologica`; draft anterior
  neoperat → `DraftAnterior`; luna precedentă fără AMO operată deși avea fișe
  eligibile → `LunaLipsa` (cronologie STRICTĂ); perioada fiscală închisă →
  `PerioadaInchisa`; nicio fișă eligibilă → `FaraFise`. Motiv la raport, refuz
  la comandă; `MotivNegenerare` primește membrii noi.
- Fișă eligibilă în luna M: `InFunctiune`, `DataPunereInFunctiune` < prima zi a
  lui M, rest contabil > 0 SAU rest fiscal > 0 (una din cele două poate
  termina mai devreme; linia postează 0 contabil dacă doar fiscalul mai
  curge —), fără `Iesire` ≤ ultima zi a lui M.
- `Incearca` / `Previzualizeaza` / `Genereaza` / `regenereaza` — ca ITV (79a/d).
- `AmortizareLunara.ValideazaOperare`: recalculează și refuză dacă mulțimea
  (fișă, contabil, fiscal, deductibil, conturi, loc) diferă; refuz dacă există
  AMO operată ulterioară sau lipsește cea precedentă. `PregatesteOperare` nu
  recalculează.
- Postarea: per linie debit `ContCheltuialaAmortizare` = credit
  `ContAmortizare` pe `Valoare` (contabil); fiscalul și deductibilul NU
  postează (linia cu contabil 0 și fiscal > 0 rămâne pe document, fără notă).
  Registru: rând `Amortizare` cu cele trei cifre, `Luni` 1, `Repartitor` =
  loc. Storno AMO = rânduri inverse în ambele registre, în lună liberă, refuzat
  dacă există AMO ulterioară operată; perioada închisă = graniță și la storno.
- `genereaza` SCRIE ori de câte ori luna e liberă (capcana 79).

### F26-D8 — Transferul = schimbarea locului pe fișă, administrativ, fără document

`Imobilizare.Loc` e editabil în `InFunctiune`; următoarea AMO postează pe noul
loc; istoricul = rândurile lunare (`RepartitorId`). Nu mișcă valoare, nu
postează, nu schimbă nimic fiscal. Urmă explicită (`Fel = Transfer`, valoare
0) = restanță F26-r5, dacă auditul o cere.

### F26-D9 — Reevaluarea = restanță F26-r6, cu locul rezervat

`Fel = Reevaluare` în enum; `Valoare` ± și `ValoareFiscala` 0 (nerecunoscută
fiscal) sunt deja coloane; contarea 21x = 105 prin `PoliticaAmortizare` extinsă.
Nu intră (aritmetică proprie, 105 → 1175 la ieșire).

### F26-D10 — Ușile API

| ușă | formă |
|---|---|
| `api/imobilizari` | nomenclator: `GET`, `GET {id}` (+ parametrii curenți, banda catalogului), `POST`, `PUT {id}`, `DELETE {id}` (`Noua`), `GET {id}/fisa` (situația + rândurile), `GET registru?laData=` (toate fișele: brut / cumulat / net contabil și fiscal / deductibil cumulat). `fisa`/`registru` = sume pe ușa non-secured → cer și citire pe `RegistruImobilizari` (F22-D5) |
| `api/clasificari` | doar citire (nomenclator de seed), căutare |
| `api/pif` | agregat cules: CRUD (Draft) + comenzi; `GET linii-sursa?dataStart=&dataEnd=&partenerId=&toate=` (liniile FCT operate de clasă F cu restul neconsumat; plic `{ Candidati, MaiSunt }`, plafon 500). `PifWriteDto { Data, PredatorId, PrimitorId, Linii[{ Id?, ImobilizareId, Fel, LinieSursaId?, Valoare, ValoareFiscala?, inițialele, parametrii }] }` |
| `api/cas` | `CasWriteDto { Data, Cauza, PredatorId, PrimitorId, Fise[ImobilizareId] }`; liniile le produce serverul; `PUT` le re-produce |
| `api/amo` | tiparul ITV: `GET` (`Data desc`), `GET {id}`, `GET previzualizare?an=&luna=` (`Motiv?`, `AmortizareVieId?`, `Linii[{ fișă, contabil, fiscal, deductibil, conturi, loc }]`, totalurile), `POST genereaza { An, Luna, UnitateId }`, `POST {id}/regenereaza`, `DELETE {id}`, comenzile; fără `WriteDto` |
| `api/politici` | `PoliticaAmortizare`, `RegulaDeductibilitate` prin OData + gardian (81), ca celelalte politici |

Gate-urile 401 → 400 → 404 → 403 → 422, `EroriDto` (80); `CreareAutorizata` pe
`genereaza`; id PIF/CAS/AMO pe ușa NTC = 404 (TPT); totalurile pe server
(42c); `Rezolva.Cere` înaintea oricărui `CreateObject`. Securitatea nu cere
seed per tip (`ReadOnlyAllByDefault`).

### F26-D11 — Clientul React

`felii/imobilizari` (`ListaNomenclator`, detaliu cu panoul „Fișa": parametrii
curenți + situația + grila registrului, `RegistruImobilizari.tsx` la dată),
`felii/pif`, `felii/cas` pe `DocumentShell` (PIF: linii cu lookup de fișă,
popup „Linii de factură" cu prefill, secțiunea de parametri pe linie cu banda
catalogului; CAS: doar fișele, liniile read-only), `felii/amo` pe tiparul ITV
(previzualizarea lunii cu cele trei coloane, generează/regenerează, storno).
Rute `/imobilizari[/nou|/:id|/registru]`, `/pif[/nou|/:id]`, `/cas[/nou|/:id]`,
`/amo[/:id]`; meniul „Imobilizări"; `rutaTip` capătă `PIF`/`CAS`/`AMO`; grila
politicilor primește cele două politici. Nimic calculat în TS.

### F26-D12 — XAF Blazor: doar cât cere ModelCheck

`Imobilizare` și `ClasificareImobilizari` cu `[NavigationItem]`,
`[DefaultProperty]`; documentele moștenesc navigația; `ContaUiBaseline`:
layout-uri pentru cele trei tipuri și fișă (grile `Client` pe culegere;
liniile CAS/AMO read-only), `RegistruImobilizari` `Server`; `[XafDisplayName]`
pe toți membrii noi; `--dump-metadata`. Fără acțiune XAF de generare AMO
(restanță F26-r7). Operarea prin `DocumentOperareController`.

### F26-D13 — Probele

Bloc `E2E-IMO`, pe AMBELE profiluri (conturile citite din politică; marcaj
`E2E-IMO`, purjă încrucișată cu `E2E-API-IMO`, precondiția lunilor libere):

- Scenă: unitate internă, gestiune „PROBĂ F26", furnizor; FCT cu linie de
  clasă F (`214`, 3 600) operată → contul implicit = 404, fără lot/NIR.
- Fișă `Noua` → PIF `Intrare` cu linie sursă (prefill 3 600; a doua fișă pe
  aceeași linie cu 1 = refuz), liniară 36 / fiscal liniară 36, `Standard` →
  zero note, rând `Intrare` cu parametri, `InFunctiune`; previzualizarea
  lunii PIF = `FaraFise`.
- AMO M+1..M+3: notă 6811 = 2814 de 100,00 pe gestiune, rând `Amortizare`
  (100 / 100 / 100). Modernizare 1 650 în M+3 → AMO M+4 = 150,00. Revizuire în
  M+4 (durata 48) → AMO M+5 = `round(rest / 44)`. Storno AMO M+5 în lună liberă
  → rânduri inverse în ambele registre; anulare AMO M+3 cu M+4 operată =
  refuz; lună închisă = motiv/refuz; M+7 cu M+6 lipsă = `LunaLipsa`; fișă fără
  politică = `FisaFaraPolitica`.
- Fiscal/deductibil: fișă „autoturism" 90 000 / 60 luni, categoria
  `VehiculPersoaneMax9Locuri`, neexclusiv → contabil 1 500,00 / fiscal 1 500,00 /
  deductibil 1 500,00; 120 000 / 60 → 2 000 / 2 000 / **1 500**; aceeași cu
  `UtilizareExclusiva` → 2 000 / 2 000 / 2 000; fișă cu durata fiscală 24 și
  contabilă 36 → după luna 24 linia postează contabil > 0, fiscal 0; fișă cu
  reziduală 600 pe 3 600 / 36 → contabil 83,33, fiscal 100,00; regulă cu
  `DeLa` în mijlocul vieții → luna dinainte / de după diferă; durata fiscală
  în afara benzii catalogului = refuz; `SediuSocialInLocuinta` neexclusiv
  → deductibil 0 (2024) și 50 % (2026).
- CAS `Casare` în M+6 → două note (2814 = 214 cumulat; 6583 = 214 rest), rând
  `Iesire` cu toate cifrele negative, `Iesita`; AMO M+7 n-o mai conține; CAS cu
  AMO a lunii operată = refuz; PUT pe PIF operat = refuz; ștergerea fișei
  `InFunctiune` = refuz; `Loc` schimbat → următoarea AMO pe noul loc.
- Probe PURE pe `CotaLunara`: Citan, Zebra, CENTRU IT după eveniment,
  invertorul după modernizare cu durata repornită (prin `Revizuire`);
  accelerata; degresiva AD1 pe 60 de luni (cifrele fixate de mână la pasul 2).

Bloc `E2E-API-IMO`: `Apply` cap-coadă pe ușile noi (fișă → PIF cu
`linii-sursa` → AMO previzualizare/genereaza/regenereaza → Revizuire → CAS), cu
refuzurile 422 și `Stale`.

Bloc `RECONCILIERE-MF` (privat): DOAR dacă există `1C/mf/esantion-amortizare.csv`
(extras din Flax cu interogarea din raportul explorării, ≥ 30 de active,
obligatoriu cele integral amortizate, cele 3 reevaluate și cel modernizat;
folderul gitignored): per activ `(CodInventar, ValoareDeAmortizat, LuniRamase,
luna, SumaPostata)`; recalculăm cu `CotaLunara` și comparăm luna cu luna;
potrivirile și DIFERENȚELE se raportează, nu se ascund (21, 35b); blocul nu
pică pe diferențe, doar pe fixture malformat. Tabelul intră în decizia 087.

`refuzuri.ps1`: oracolele ușilor noi pe cei patru utilizatori (User 404/403;
`Cititor`/`Configurator` 403 pe scriere și comenzi, `Configurator` 200 pe cele
două politici; Admin 200/201/422), `fisa`/`registru`/`previzualizare` fără
drept pe `RegistruImobilizari` = 403, id PIF pe ușa NTC = 404, `genereaza`
doar ca `Cititor`/`User`/`Configurator`.

Comenzile de verificare:

```
cd nou/tools/ModelCheck && dotnet run                 # bugetar
cd nou/tools/ModelCheck && dotnet run privat          # privat
dotnet run --project nou/tools/ModelCheck -- --dump-metadata
cd nou/Atlas.Conta.BackOffice/Atlas.Conta.BackOffice.Module && dotnet ef migrations has-pending-model-changes --context BackOfficeEFCoreDbContext
cd nou/Atlas.Conta.Client && pnpm verifica:drift      # WebApi OPRIT
cd nou/Atlas.Conta.Client && pnpm build
pwsh nou/tools/ProbeHttp/refuzuri.ps1                 # host viu, Privat
nou/tools/Import1C — rulare integrală, proces detașat + monitor (50d), raport identic cu baseline-ul F18
```

### F26-D14 — Regula de oprire a feliei

- ModelCheck 0 FAIL pe AMBELE profiluri, probele existente neschimbate în text
  (doar numere noi dacă seed-ul mută cifre); migrația canonică (`--context
  BackOfficeEFCoreDbContext`, niciodată `--no-build`), `has-pending-model-changes`
  curat, `metadata.json` la zi.
- `openapi.json`/`api-types.ts` fără drift; `tsc` + `vite build` verzi;
  `refuzuri.ps1` verde pe host viu.
- Smoke în browser (React, Privat) pe scenariul `E2E-IMO`; smoke XAF pe hostul
  Blazor (fișa, PIF, AMO generată din React și operată din XAF, CAS).
- **Atingerile PERMISE în `Motor/*`, fixate aici**: (a) interfața
  `IDocumentCuRegistruPropriu` + trei dispecere în `MotorOperare` (≤ 15 linii);
  (b) `RegistruImobilizari` în registrele protejate de `GardianEditare` (≤ 5
  linii); (c) fișierul NOU `Motor/AmortizareService.cs`. Orice altă linie în
  `MotorOperare`, `StocService`, `ImperechereService`, `RegistruTvaService`,
  `DimensiuniResolver`, `Potrivire`, `LoturiCulegereService` sau în
  `Document`/`DocumentDetaliu` = felia se OPREȘTE și se arată alternativa.
- Închiderea cere Import1C integral cu raport IDENTIC cu baseline-ul F18.
- Niciun simbol de cont și nicio cifră de lege (1 500, 50 %) în cod în afara
  `DatabaseUpdate/` și a probelor. Cod slim; capcanele devin probe.

### F26-D15 — Ce NU intră (restanțe cu nume)

- **F26-r1** activele din 231 (în curs): PIF care postează 21x = 231.
- **F26-r2** deductibilitatea valorii rămase la ieșire (art. 28 (17)) ca `Fel` nou de regulă pe rândul `Iesire`.
- **F26-r3** legătura CAS ↔ FCL de vânzare ca evidență.
- **F26-r4** degresiva AD2; amortizarea pe unități de producție.
- **F26-r5** transferul ca rând explicit de registru.
- **F26-r6** reevaluarea (D9).
- **F26-r7** acțiunea XAF de generare AMO.
- **F26-r8** SAF-T D406 Assets + AssetTransactions din registru.
- **F26-r9** D101 / impozitul pe profit ca proiecție peste `AmortizareDeductibila`.
- **F26-r10** ajustările pentru depreciere (29x), leasingul, obiectele de inventar date în folosință (8035).
- **F26-r11** repartizarea cheltuielii pe mai multe centre de cost cu coeficienți.
- **F26-r12** `RegistruImobilizari` pe `ServerView` (85).
- **F26-r13** migrarea fișelor din 1C (`IntroducereSolduriInitialeMF` → PIF de deschidere cu inițialele): conectorul, nu mecanismul.
- **F26-r14** reguli de eligibilitate a metodei fiscale pe categorie (accelerata doar pe echipamente/calculatoare, art. 28 (12)) — mecanism nou dacă se cere refuz, azi doar documentat.
- **F26-r15** cele 4 poziții-părinte din catalog ale căror benzi stau pe sub-variante fără cod (`2.1.6.1.1.`, `2.1.6.1.2.`, `2.1.17.4.`, `2.1.17.5.`): fără verificare a duratei fiscale până la o decizie (cod derivat sau banda unită pe părinte).
- **F26-r16** clasificația bugetară a cheltuielii cu amortizarea pe profilul bugetar: contul de cheltuială are defalcarea `E` în planul instituției, iar linia AMO n-are de unde lua Codul economic (nici fișa, nici politica, nici linia) — decizia owner-ului între dimensiune pe fișă/politică/linie și defalcare `S` în seed; până atunci AMO se generează pe bugetar, dar nu se operează (proba `IMO-V31c`, pasul 2).

### F26-D16 — Cadrul pentru evoluția legii (rezumat, ca regulă durabilă)

Legea se exprimă în DATE cu valabilitate (`DeLa`/`PanaLa`, `Temei`) pe rânduri
de politică aliniate de seed; un fel nou de regulă = mecanism nou în cod, mic
și numit; cifrele deja postate sunt fapte și nu se recalculează; parametrii
activului sunt fapte datate în registru, scrise de documente, niciodată
edit-uri de nomenclator. Testul unei cerințe fiscale noi: „încape într-un rând
nou cu `DeLa`?" — dacă da, e seed; dacă nu, e felie.

## Testul contra invarianților

- **I**: PIF, CAS, AMO au ambele laturi (unitatea internă ↔ locul) și lasă urmă
  în registre; fișa nu e document; transferul nu e document (nu lasă urmă);
  revizuirea parametrilor E document (lasă urmă).
- **II**: contracte declarate (`IDocumentCuRegistruPropriu`,
  `IDocumentCuPostareExplicita`, `ILinieCuPostareExplicita`, `DimensiuniCulese`),
  niciun `is`/`switch` pe tip; mecanismul în `AmortizareService` (insulă);
  nimic pe baza `Document`/`DocumentDetaliu`.
- **III**: un registru append-only, scris doar de motor, complet rezolvat
  (cifre + parametri + loc); fișa, registrul, D101, SAF-T = sume peste el;
  corecția = anulare / storno; perioada închisă = graniță.
- **IV**: structura (fișa, registrul, tipurile, enum-urile, felurile de regulă)
  = cod; conturile, plafoanele, procentele, datele legii, catalogul = date;
  nicio expresie interpretabilă; `TipDocument` = ancoră.
- **V**: 1C și catalogul = evidență; formula e definită de noi, confirmată pe
  cifrele lor; diferențele reconcilierii se raportează.
- **VI**: neatins (fără lot, fără evaluare de stoc).
- 29, 33d, 42c, 80, 84a: ca la felia 25.

## Pașii (un agent per pas, secvențial; main verifică independent și comite după fiecare)

1. **Model + registru + hook + politici + catalog + seed + gardian + migrație + XAF baseline** —
   `Imobilizare`, `ClasificareImobilizari` (+ CSV în `SeedData/`),
   `RegistruImobilizari`, `PoliticaAmortizare`, `RegulaDeductibilitate`, cele
   trei tipuri cu detaliile lor, enum-urile, `IDocumentCuRegistruPropriu` +
   dispecerele, gardianul (registrul; regulile fișei; `TipuriConfigurabile`),
   `DbSet`-uri, migrația `F26Imobilizari`, seed-ul D4 pe ambele profiluri,
   `ContaUiBaseline`, `[XafDisplayName]`, `--dump-metadata`; hook-urile de
   registru ale PIF/CAS scrise, AMO schelet. Verificare: ModelCheck 0 FAIL × 2,
   `has-pending-model-changes` curat. Regula de oprire: D14.
   *Executat 2026-09-14, fără opriri; devierile raportate și acceptate*:
   catalogul seed-uiește 590 de poziții din 598 (8 sub-variante „a)/b)" fără
   cod propriu sunt sărite, părinții lor rămân fără bandă — F26-r15);
   `ClasificareImobilizari` nu e `ICuProvenienta` (ar fi intrat forțat în
   `TipuriConfigurabile`), seed prin upsert pe cod ca `RandD300`; pe bugetar
   perechile de amortizare sunt FRUNZELE `280.08.01`/`280.08.09`/`281.03.01`/
   `281.03.03` (nodurile `.00` din D4 nu există); `VerificareProfilService`
   primește etichetele celor două politici (vocabularul raportului de profil,
   obligatoriu pentru `TipuriConfigurabile`, nu motor); `Cautare.NumeCod` cade și
   pe `NumarInventar`; inițialele sunt REFUZATE pe intrarea cu linie sursă;
   `ValoareReziduala` opțională; deductibilul inițial = fiscalul inițial;
   scena probelor e în 2027 (lunile 2026 sunt ocupate de suită), cu perioade
   create și purjate de bloc; `RegulaDeductibilitate` fără `PanaLa` ⇒ D7 alege
   per `(Categorie, Fel)` rândul cu `DeLa` maxim ≤ data rândului lunar.
   `PoateFiStins = false` declarat și pe PIF/CAS (86g, proba `IMO-V29`);
   `GardianEditare.Originale` devine `internal` pentru gardianul fișei (fără
   duplicare). Migrația `20260914113154_F26Imobilizari` (11 tabele noi, zero
   atingeri pe cele existente); ModelCheck bugetar 991/0, privat 1106/0
   (re-rulate independent după curățenia de cod slim).
2. **`AmortizareService` + hook-urile de operare + `E2E-IMO` + probele pure + `RECONCILIERE-MF`** —
   `CotaLunara`, `Deductibil`, `Situatie`, `Analizeaza`/`Incearca`/
   `Previzualizeaza`/`Genereaza`, `LiniiIesire`, `ValideazaOperare` pe cele trei
   tipuri, `MotivNegenerare` extins; fixture-ul din Flax (agent read-only,
   SELECT only) în `1C/mf/`; blocul integral pe ambele profiluri. Apoi
   **Import1C integral** (detașat + monitor) cu diff sortat contra
   baseline-ului — după pasul 2 `Motor/*` nu se mai atinge.
   *Executat 2026-09-14, două opriri raportate (nu blocante), devierile
   acceptate de main*: (1) **baza „la ultimul eveniment" = situația la SFÂRȘITUL
   lunii evenimentului**, iar restul curent și lunile se citesc la sfârșitul
   lunii precedente — luna evenimentului postează încă cota veche, parametrii
   noi curg din luna următoare indiferent de zi; e formula OBSERVATĂ în Flax
   (invertor modernizat pe 21.09.2023 → 1 314,55 din 10.2023; CENTRU IT
   reevaluat pe 01.07.2024 → 1 255,50 din 08.2024), raportul `1C/07` §5.4
   spunea imprecis „în 2023-09"; cifrele D13 rămân identice (150,00; 4 800/44).
   (2) Eligibilitatea fișei e după DATE (`DataPunereInFunctiune` < prima zi,
   `DataIesire` > ultima zi), nu după `Stare`: o fișă ieșită într-o lună
   ulterioară e legitimă pe AMO-ul lunii curente; verificarea `InFunctiune` din
   `AmortizareLunara.ValideazaOperare` a fost înlocuită de compararea mulțimii
   de linii. (3) Linia cu contabil 0 și fiscal > 0 rămâne FĂRĂ conturi (motorul
   sare linia explicită fără conturi; cu conturi ar posta o notă de 0), locul
   rămâne obligatoriu; conturile se cer doar la `Valoare ≠ 0`. (4) `Degresiva`
   cere durata multiplu de 12 (graficul e pe ani), refuz la PIF pe ambele
   metode. (5) `Previzualizeaza` are un al patrulea parametru opțional
   `inlocuieste` — gardianul de operare se exclude pe sine. (6) Linia AMO n-are
   `Descriere` (ar fi coloană nouă): FK-ul `Imobilizare` identifică fișa.
   (7) **Oprire consemnată, deschisă pentru owner (F26-r16)**: pe bugetar nota de
   amortizare NU se poate posta — planul instituției dă contului de cheltuială
   cu amortizarea defalcarea `E` (`plan-conturi.csv`), deci motorul cere Cod
   economic, iar nici fișa, nici politica, nici linia nu-l poartă (conturile
   CAS au `S`, de aceea pasul 1 n-a lovit-o); sursa lui e decizie de model
   (coloană pe fișă/politică/linie sau defalcare `S` în seed), nu cârpeală de
   generator — proba `IMO-V31c` o ține la vedere, lanțul lunar (AMO operate,
   storno, CAS după AMO) rulează pe privat, generarea/cele trei cifre/probele
   pure rulează pe ambele profiluri. Probele: `IMO-V30…V57` (+13 bugetar, +33
   privat); `RECONCILIERE-MF` pe 118 active / 2 559 rânduri lunare din Flax:
   2 502 potriviri exacte + 44 rânduri „ultima lună" (99,49 %), diferențele
   rămase sunt 13 rânduri din 05.2023 postate printr-un document MANUAL de recuperare care
   cumulează mai multe luni (raportate cu activ/lună/așteptat/postat; tabelul
   intră în decizia 087). ModelCheck: bugetar 1004/0, privat 1139/0;
   `has-pending-model-changes`: niciuna.
3. **API** — `Api/Imobilizari/`, `Api/Pif/`, `Api/Cas/`, `Api/Amo/`,
   controllerele, `api/clasificari`, `linii-sursa`, `E2E-API-IMO`, oracolele,
   `verifica:drift` (WebApi OPRIT), probele HTTP pe host viu.
4. **Client** — feliile + rute + meniu + `rutaTip` + politicile în grilă; `tsc`
   + `vite build`; smoke în browser pe Privat.
5. **Smoke XAF** + `--dump-metadata` final.
6. **Review advers** (agent separat: PIF pe fișă `Iesita`; două `Intrare` pe
   aceeași fișă în același commit; plafonul liniei sursă cu PIF stornat;
   `Revizuire` cu durata sub lunile deja amortizate; AMO generată, `Revizuire`
   operată, AMO operată fără regenerare; storno AMO cu CAS ulterioară; CAS în
   luna unei AMO operate; regulă de deductibilitate ștearsă de `Configurator`
   după previzualizare; `DeLa` viitor; categorie fără regulă; `UtilizareExclusiva`
   schimbată prin `Revizuire` în mijlocul lunii; reziduală ≥ brut; net negativ
   prin storno parțial; `User` pe `fisa`/`registru`; bugetar cu politica
   bugetară; `ToEven` pe profil ipotetic) — fix-urile le aplică main-ul; apoi
   decizia 087 (Regula durabilă a–k), README, `restante.md` (F26-r1…r14),
   CLAUDE.md §Stare, `docs/stare-curenta` (domeniu-si-operare: registrul al
   patrulea, tipurile, contractul D3; politici-si-fiscalitate: cele două
   politici, catalogul, cadrul D16; api-si-client; limite-curente), istoricul,
   §Închidere aici.
