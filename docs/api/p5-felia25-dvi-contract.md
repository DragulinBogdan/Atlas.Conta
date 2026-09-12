# Pasul 5, felia 25 — DVI (declarația vamală de import) ca tip de document (contract)

Data fixării: 2026-09-12. Închide 83-r4 (poarta lăsată deschisă de 83g).
Deciziile DVI-D1…D10 sunt PIN-UITE — agenții de implementare nu le
redeschid; orice nepotrivire cu realitatea codului se RAPORTEAZĂ, nu se
normalizează tăcut. Decizia rezultată se scrie la închidere (086).

## Scop

Factura furnizorului extern se culege azi cu `IMP` (cotă 0, `Neimpozabil`,
nemapat pe D300/D394): linia nu poartă taxă și spune de ce (83g). TVA-ul
datorat în vamă, deductibil pe 4426, nu are unde să intre în evidență, deci
rândurile de import ale decontului (rd. 24/25, respectiv rd. 7/22 la
amânarea plății) rămân goale și SAF-T-ul n-are cum să emită codurile
301204/301205/300604/300605, care sunt ALE DVI-ULUI, nu ale facturii.

Ce au arătat explorările (2026-09-12, două rapoarte read-only):

- **Legacy n-a avut DVI** (10 tipuri în `db/inventar/11-testul-bazei.md`,
  niciunul vamal; `COD_TARIF_VAMAL`/`TVA_AMANAT` = coloane invizibile,
  moarte). Nimic de migrat; felia pornește de la zero.
- **Datele Flax 2025**: 9 FCT extra-UE (MA/GB/NO), în majoritate servicii sau
  colete mici, fără nicio DVI; exportul 1C nu menționează vama. Proba
  funcțională e pe SCENĂ (ModelCheck), nu pe date reale.
- **Nomenclatorul SAF-T** (`anaf/RO_SAFT_SchemaDefCod_16.02.2026.xlsx`, foaia
  „Achizitii ded 100%") are DOUĂ familii de coduri de import: 301204/301205
  (21%/11%, TVA plătită efectiv în vamă → D300 rd. 24/25) și 300604/300605
  (21%/11%, taxare inversă / amânarea plății, art. 326 (4)–(5) → rd. 7, cu
  oglinda 22). Rândurile D300 există în seed (`RanduriD300`: 7, 22, 24, 25).
- **Motorul e deja pregătit**: o linie fără regulă de contare și fără conturi
  explicite NU postează valoarea (`MotorOperare.cs:152-162`), dar primește
  nota de TVA din `PoliticaTva` (debit `TipTva.ContTvaDeductibil`, credit
  contrapartida politicii; la `TaxareInversa` 4426 = 4427) și rândul de
  `RegistruTva` (`RegistruTvaService.Deriva`). DVI se construiește FĂRĂ nicio
  schimbare în `Motor/*`.

Felia livrează: (1) tipul `Dvi` cu liniile pe baza `DocumentDetaliu` și
legătura n→m cu facturile de import, (2) tipurile de TVA de import + politicile
DVI în seed-ul privat, (3) ușa REST + ecranul React, (4) ecranele XAF, (5)
probe ModelCheck pe scenă + oracole HTTP.

## Deciziile

### DVI-D1 — Forma tipului: `Dvi : Document`, linii pe baza `DocumentDetaliu`

`Dvi` derivă direct din `Document` (nu din `NotaContabila`: nota cere conturi
explicite pe fiecare linie, DVI nu postează valoarea). Fără coloane proprii pe
antet: `Numar` = MRN-ul declarației (cules, fără `PoliticaNumerotare` — ca la
FCT), `Data` = data vămuirii, `Predator` = repartitorul CĂRUIA i se datorează
TVA-ul (biroul vamal, cu cont implicit 446, sau comisionarul vamal care a
plătit în vamă, cu 401), `Primitor` = unitatea internă, `Observatii`.
Furnizorul extern NU e latură: vine din facturile legate (D3).

Liniile folosesc `DocumentDetaliu` de bază (precedentul NIR/BCS): `TipMaterial`
= tipul bunurilor vămuite (cheia rămâne NOT NULL pe bază; API-ul îl SUGEREAZĂ
din facturile legate), `TipTva` = tipul de import (D2), `Valoare` = valoarea în
vamă (baza), `ValoareTva` = taxa din declarație (culeasă; dacă e 0 o calculează
`TvaService` la operare, ca la FCT — 48b: un TVA cules nu se suprascrie),
`Cantitate` 0, `Lot` null. `[TipDetaliu(typeof(DocumentDetaliu))]` — dacă
atributul nu acceptă baza, se raportează (nu se inventează o derivată goală).

`TipTva` primește coloana `DeImport` (bool, `[XafDisplayName("De import")]`,
seed `true` pe `IMP` și pe tipurile din D2): singura cale de a spune „tip de
import" fără a hardcoda coduri (29). Migrația e a acestui pas.

### DVI-D2 — Tipurile de TVA și politicile DVI (seed privat; bugetar = doar ancora)

| Cod | Denumire | Cotă | Regim | Cont deductibil | SAF-T achiziție | D300 achiziție |
|---|---|---|---|---|---|---|
| `IMP21` | Import de bunuri 21% — TVA plătită în vamă | 21 | `Normal` | 4426 | 301204 | rd. 24 |
| `IMP11` | Import de bunuri 11% — TVA plătită în vamă | 11 | `Normal` | 4426 | 301205 | rd. 25 |
| `IMPTI21` | Import de bunuri 21% — taxare inversă (art. 326) | 21 | `TaxareInversa` | 4426/4427 | 300604 | rd. 7 |
| `IMPTI11` | Import de bunuri 11% — taxare inversă (art. 326) | 11 | `TaxareInversa` | 4426/4427 | 300605 | rd. 7 |

Codurile de livrare rămân null; `MapareD394` nemapat deliberat (partenerul
extra-UE nu se declară; D394 e pe facturi, nu pe DVI). Rd. 22 e OGLINDA lui 7
(`RandD300.OglindaA`), o pune proiecția — dacă proiecția nu oglindește 7 → 22
automat, se raportează. `IMP` (0%) rămâne neschimbat.

- `TipDocument`: `("DVI", "Declarație vamală de import", nameof(Dvi))` în
  nucleu (`SeedTipuriDocument`, decizia 20) — ancora există pe AMBELE profiluri.
- `PoliticaTva("DVI", Deductibil, RepartitorPredator, fallback "446")` —
  simbolul vine din planul seed-ului privat (dacă planul are analitic 446x
  dedicat TVA-ului în vamă, se folosește ăla; se raportează alegerea).
- `PoliticaTvaImplicit("DVI", orice clasă, "IMP21")`.
- Fără `RegulaContare`, `RegulaStoc`, `PoliticaScadenta`, `PoliticaConex`,
  `PoliticaNumerotare` pe DVI. Bugetar: zero rânduri de politică (81b).

### DVI-D3 — Legătura n→m: `DviFactura`, parte a agregatului, doar în Draft

```csharp
public class DviFactura : BaseObject {          // precedentul Imperechere (Trezorerie.cs:650)
    public virtual Guid DviId { get; set; }      public virtual Dvi Dvi { get; set; }
    public virtual Guid FacturaId { get; set; }  public virtual FacturaIntrare Factura { get; set; }
}
```

Index unic `(DviId, FacturaId)`; FK-uri `Restrict`. O factură poate sta pe mai
multe DVI (transporturi parțiale); o DVI poate avea ZERO facturi (bunuri fără
factură — mostre, e-commerce cu chitanță): legătura e EVIDENȚĂ și explicație,
nu sursa cifrelor (baza și taxa sunt cele declarate în vamă, nu suma
facturilor). Gardianul (`GardianEditare`, pe OS securizate): creare/ștergere
DOAR cât DVI e `Draft`; fără editare (ca `Imperechere`); factura trebuie să fie
`FacturaIntrare` în stare `Operat` la creare. `Dvi.ValideazaOperare` re-verifică
starea facturilor legate la operare (o factură anulată între timp = refuz 422).
După operare legăturile sunt înghețate cu documentul; stornoul NU le clonează
(are `DocumentSursa`). Anularea/stornarea unei FCT legate la o DVI operată NU
se refuză (DVI-r2): cifrele DVI nu derivă din factură; ecranul arată starea
facturii. `DviFactura` NU intră pe OData; se scrie prin `FacturiIds` din
agregatul DVI (PUT), niciodată direct.

### DVI-D4 — Operarea: planul DVI fără nicio schimbare în `Motor/*`

Planul unei DVI: zero mișcări de stoc; zero note pe `Valoare` (fără regulă,
fără conturi explicite); per linie cu `ValoareTva ≠ 0`: `Normal` → 4426 =
contrapartida (contul implicit al Predatorului, altfel fallback-ul politicii);
`TaxareInversa` → 4426 = 4427; rândul de `RegistruTva` (`Achizitie`, partener
= Predator, baza = `Valoare`, tva = `ValoareTva`, cotă/regim snapshot).

`Dvi.ValideazaOperare` (hook-ul polimorf, fără `is`/`switch` în motor):
`Numar` (MRN) nevid; cel puțin o linie; Predator = `Partener` (nu repartitor
intern), Primitor = unitate internă (ca FCT); fiecare linie: `TipTva` cu
`DeImport` și cotă > 0 (refuz explicit pentru `IMP`: „TVA-ul în vamă are
cotă"), `Valoare` > 0; facturile legate `Operat`.

**DVI NU e document stins** (amendat 2026-09-12, la pasul 1, pe cifre:
`ImperechereService.Total` = Σ(`Valoare` + `ValoareTva`) pe toate liniile ar
da rest 1605 la o datorie reală de 210, iar `DocumenteCuRest` e o uniune per
tip concret). TVA-ul în vamă e o TAXĂ și se plătește ca orice taxă
(precedentul ITV/4423): o plată către biroul vamal (partener cu cont implicit
446), fără imperechere — nota 446 = 5121 pe dimensiunea partenerului închide
soldul lui 446 pe vamă. `SensDeStins` rămâne null; `Total (brut)` e ascuns pe
view-urile DVI, iar `Baza` („Valoare în vamă") și `Taxa` („TVA în vamă") sunt
`[NotMapped]` pe DetailView. Restul polimorf pe DVI = DVI-r11.

### DVI-D5 — Ușa API: `api/dvi`, agregat cules ca NTC, plus candidații

| verb | rută | corp / răspuns |
|---|---|---|
| GET | `` | `PaginaDto<DviListDto>` (DataSourceLoader) |
| GET | `{id:guid}` | `DviReadDto` 200 / 404 / 403 |
| GET | `facturi-candidate?dataStart=&dataEnd=&partenerId=&toate=` | `FacturaCandidataDto[]`: FCT `Operat`, implicit doar clasa fiscală `ExtraUe` (`toate=true` = orice FCT operată), cu `TipMaterialSugeratId` (tipul dominant pe liniile facturii) și `Valoare` |
| POST | `` | `DviWriteDto` → `DviReadDto` 201 / 400 / 403 / 422 |
| PUT | `{id:guid}` | `DviWriteDto` → 200 / 404 / 403 / 422 (doar Draft) |
| DELETE | `{id:guid}` | 204 / 422 (doar Draft) |
| POST | `{id:guid}/opereaza` · `anuleaza` · `storneaza` · `valideaza` | ca la NTC (`OperareRezultatDto` / `EroriDto`) |

`DviWriteDto { Data, Numar, PredatorId, PrimitorId, Observatii, Linii[{ Id?,
TipMaterialId, TipTvaId, Valoare, ValoareTva }], FacturiIds[] }`.
`DviReadDto` = antet + `Linii[]` + `Facturi[{ FacturaId, Numar, Data,
PartenerDenumire, Stare, Valoare }]` + `Baza`, `Tva` (totalurile vin de pe
server, 42c). `DviListDto { Id, Numar, Data, Stare, PredatorDenumire, Baza, Tva,
NrFacturi }`. Gate-ul ca la NTC (80): 401 → 400 → 404 → 403 → 422, corp
`EroriDto`; `facturi-candidate` cere dreptul de citire pe `FacturaIntrare`
(nu doar pe `Dvi`). Un id de DVI pe ușa NTC/FCT = 404.

### DVI-D6 — Clientul: `felii/dvi`, două ecrane, `rutaTip` capătă `DVI`

`DviLista.tsx` (perioadă în URL, `storeRemote`), `DviDetaliu.tsx` pe
`DocumentShell`: antet, grila liniilor (TipMaterial lookup, TipTva lookup
filtrat pe `DeImport`, Valoare, ValoareTva; linia nouă primește implicitul
din `api/implicite` și `TipMaterialSugeratId` al ultimei facturi adăugate),
panoul „Facturi de import" (grid + „Adaugă facturi" → popup cu
`facturi-candidate` pe perioada și, opțional, partenerul; buton „toate").
Meniu lângă FCT; rute `/dvi`, `/dvi/:id`; `rutaTip('DVI', id)`. Grila `TipTva`
din `felii/politici` arată coloana nouă `DeImport`. Nimic calculat în TS.

### DVI-D7 — XAF Blazor

`[NavigationItem]` lângă FCT; `ContaUiBaseline`: layout `Dvi` (liniile: coloane
TipMaterial, TipTva, Valoare, ValoareTva; ascunse TipMaterialId/LotId/TipTvaId/
Cantitate/Lot), lista `DviFactura` nested pe DetailView-ul DVI (culegere →
`Client`, 85), `[XafDisplayName]` pe TOȚI membrii noi (84-r5 la atingere).
Operarea prin `DocumentOperareController` generic. `--dump-metadata`.

### DVI-D8 — Probele (ModelCheck pe scenă + HTTP)

Bloc `E2E-DVI` (privat): partener extra-UE + FCT operată cu `IMP`; partener
„Biroul vamal" cu cont implicit 446; DVI Draft cu două linii (`IMP21` 1000/210,
`IMPTI21` 500/105) + legătura → operare. Se verifică: `RegistruStoc` 0 rânduri
noi; `RegistruContabil` EXACT două note (4426 = 446: 210; 4426 = 4427: 105);
`RegistruTva` două rânduri `Achizitie`, partener = vama, baze 1000/500, tva
210/105; D300 pe lună: rd. 24 conține 1000/210, rd. 7 conține 500/105 și rd.
22 oglindește; jurnalul de cumpărări le arată; D394 nu conține DVI; plata de
210 către vamă (PLT pe partenerul cu cont implicit 446, fără imperechere)
închide soldul lui 446 pe vamă, DVI nu apare în `DocumenteCuRest`, iar o
imperechere cu DVI ca document stins e refuzată de domeniu; storno → registre
inverse. Gardian: legătură pe DVI operat = refuz; legătură la FCT Draft = refuz;
aceeași factură de două ori = refuz (unicitate, mesaj de gardian nu `23505`);
linie cu `IMP` (0%) = refuz la operare; DVI fără MRN = refuz; PUT pe DVI operat
= refuz; anularea FCT legate NU e refuzată și `DviReadDto.Facturi[].Stare` o
arată. Bloc `E2E-API-DVI`: `Apply` cap-coadă (creare cu facturi → citire →
candidații exclud factura legată → operare → storno). Bugetar: ancora `DVI`
există, zero politici, `TipTva` fără rânduri `DeImport` (ITV-ul e precedentul).
`refuzuri.ps1`: oracolele DVI pe cei patru utilizatori (User 404/403,
`Cititor` 403 pe scriere, `Configurator` 403 pe scriere, Admin 200/422) +
`facturi-candidate` fără drept pe FCT = 403 + id DVI pe ruta NTC = 404.

### DVI-D9 — Regula de oprire a feliei

- ModelCheck 0 FAIL pe AMBELE profiluri, probele existente neschimbate în text,
  plus blocurile noi; `dotnet ef migrations has-pending-model-changes` curat,
  migrația canonică (23a), `--dump-metadata` la zi.
- `openapi.json`/`api-types.ts` fără drift; `tsc` + `vite build` verzi.
- `refuzuri.ps1` toate verzi (cele 80 existente + cele noi) pe host viu.
- Smoke în browser (React, Privat): FCT extra-UE → DVI cu factura legată →
  operare → jurnal de cumpărări → D300 rd. 24 → plată → rest 0; smoke XAF pe
  hostul Blazor: DetailView DVI cu linii și facturi, operare.
- `Motor/*` NEATINS, cu o singură excepție decisă la pasul 1: dispecerul
  generic `IVerificabilLaCommit` în `GardianEditare.Verifica` (trei linii,
  lângă verificările de interfață existente) — gardianul rulează DOAR pe ușile
  securizate, Import1C intră pe ușa non-secured, deci proba supremă nu e cerută
  de el. Dacă un pas atinge motorul de OPERARE (`MotorOperare`, `StocService`,
  `ImperechereService`, `RegistruTvaService`…), felia se oprește, se
  raportează motivul, iar închiderea cere Import1C integral cu raport IDENTIC
  cu baseline-ul F18 (`reconciliere-20260829-134555.txt`).
- Niciun simbol de cont în cod în afara `DatabaseUpdate/` (grep pe `446`,
  `4426`, `4427` în `Module/**/*.cs`: zero apariții noi).

### DVI-D10 — Ce NU intră (restanțe cu nume, deschise de felie)

- **DVI-r1** taxele vamale și accizele în costul de achiziție (ajustare de cost
  pe lot) — lovește motorul de stoc; decizie proprie, testată contra VI.
- **DVI-r2** anularea/stornarea unei FCT legate la o DVI operată nu se refuză
  (D3); o regulă de dependență ar cere un hook nou pe `Document`.
- **DVI-r3** RLF pe DVI (retur de import / re-export).
- **DVI-r4** amânarea plății în vamă ca politică de partener (azi = alegerea
  `IMPTI*` pe linie).
- **DVI-r5** scadență pe DVI; **DVI-r6** unicitatea MRN-ului.
- **DVI-r7** proba că proiecția SAF-T D406 emite rândul DVI cu 301204: dacă
  proiecția iterează `RegistruTva` fără filtru de tip, intră automat și se
  verifică în `E2E-DVI`; dacă filtrează, se raportează și devine restanță.
- **DVI-r8** `DviFactura` în „Explică"/OData; **DVI-r9** curățenia datelor Flax
  (reMarkable NO marcat `TI19`) — a datelor, nu a feliei.
- **DVI-r10** comisionarul vamal care plătește TVA-ul și îl refacturează:
  factura lui ar purta taxa (4426 = 401) și DVI n-ar mai avea voie s-o
  posteze a doua oară — flux propriu, nedecis (azi Predator = biroul vamal).
- **DVI-r11** DVI ca document stins (rest = Σ `ValoareTva` a liniilor
  `Normal`): cere formula restului polimorfă în `ImperechereService.Total` +
  ramura DVI în uniunea `DocumenteCuRest` (același blocaj ca RDC).

## Testul contra invarianților

- **II** (motorul nu cunoaște frunzele): `Motor/*` neatins; ce cere DVI vine
  prin hook-uri (`ValideazaOperare`, `SensDeStins`) și prin politică.
- **III** (registrele scrise doar de motor): neatins; `DviFactura` nu e registru.
- **IV** (politica = date): tipurile de TVA, contrapartida, implicitul, maparea
  D300 = rânduri de seed; `DeImport` e coloană de nomenclator, nu cod.
- **V/VI**: neatinse (DVI n-are stoc, nici evaluare).
- 29: niciun simbol de cont în cod. 33d: același `Draft → Operat → Stornat`.
  42c: totalurile pe server. 80: aceeași ordine a refuzurilor.

## Pașii (un agent per pas, secvențial; main verifică și comite după fiecare)

1. **Model + seed + gardian + ModelCheck** — `Dvi`, `DviFactura`, `TipTva.DeImport`,
   `DbSet`-uri, migrația (`--context BackOfficeEFCoreDbContext`, niciodată
   `--no-build`), seed-ul D2 (nucleu + privat), gardianul D3, `ValideazaOperare`
   D4, XAF D7 (atribute + `ContaUiBaseline`), blocul `E2E-DVI` + proba bugetar,
   `--dump-metadata`. Verificare: ModelCheck 0 FAIL pe AMBELE profiluri.
   *Executat 2026-09-12 cu două opriri raportate și tranșate*: gardianul fără
   punct de extensie în afara `Motor/` → interfața `IVerificabilLaCommit` pe
   entitate + dispecerul generic (D9); restul de stins greșit (1605 la 210) →
   DVI nu e document stins (D4, DVI-r11). Migrația `20260912193852_F25Dvi`.
2. **API** — `Api/Dvi/` (`DviDtos.cs`, `DviApply.cs`), `DviController`,
   `facturi-candidate`, blocul `E2E-API-DVI`, oracolele în `refuzuri.ps1`,
   `pnpm verifica:drift` (WebApi OPRIT), probele HTTP pe host viu (Privat,
   re-seed pentru `Cititor`/`Configurator`), cifrele în §Închidere.
3. **Client** — `felii/dvi` + rute + meniu + `rutaTip` + coloana `DeImport` în
   grila `TipTva`; `tsc` + `vite build`; smoke în browser pe Privat.
4. **Smoke XAF** pe hostul Blazor (`--urls=https://localhost:5003`, DLL-uri
   blocate cât rulează WebApi) + `--dump-metadata` final.
5. **Review advers** (agent separat, scenarii concrete: PUT cu `FacturiIds` pe
   DVI operat; factura anulată între creare și operare; `User` pe
   `facturi-candidate`; Predator repartitor intern; `N21` pe linia DVI; două
   DVI pe aceeași factură; storno-ul DVI și legăturile; `IMPTI21` și restul de
   stins; DVI pe bugetar; `Configurator` care editează `DeImport`) — fix-urile
   le aplică main-ul; apoi decizia 086 (cu „Regula durabilă"), README-ul
   jurnalului, `restante.md` (83-r4 închisă, DVI-r1…r9), CLAUDE.md §Stare,
   stare-curenta (domeniu-si-operare, politici-si-fiscalitate, api-si-client,
   limite-curente), istoricul, §Închidere aici.

Comenzile de verificare:

```
cd nou/tools/ModelCheck && dotnet run                 # bugetar
cd nou/tools/ModelCheck && dotnet run privat          # privat
dotnet run --project nou/tools/ModelCheck -- --dump-metadata
cd nou/Atlas.Conta.Client && pnpm verifica:drift      # WebApi OPRIT
cd nou/Atlas.Conta.Client && pnpm build
pwsh nou/tools/ProbeHttp/refuzuri.ps1                 # host viu, Privat
```

## Închidere

(se completează la pasul 5)
