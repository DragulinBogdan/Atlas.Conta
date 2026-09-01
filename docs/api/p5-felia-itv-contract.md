# Pasul 5, felia 21 — ITV (închiderea de TVA) prin API și client (contract)

Data fixării: 2026-09-01. Scheletul scurt al feliei 20
(`p5-felia-finisaj-client-contract.md`) — felia nu schimbă schema, nu atinge
registrele și nu redeschide motorul; adaugă o UȘĂ pentru un serviciu care există
(`InchidereTvaService`, 46c). Deciziile de mai jos (F21-D1…F21-D12) sunt
PIN-UITE — agenții de implementare nu le redeschid; orice nepotrivire cu
realitatea codului se RAPORTEAZĂ, nu se normalizează tăcut.

## Scop

ITV e ultimul tip de document fără felie de client, și e deliberat altfel
(76a, F19-D1): nu e un agregat CULES (PUT header + linii), e rezultatul unui
serviciu — o COMANDĂ cu parametri (an, lună, unitatea internă) și un ecran de
rezultat. Azi singurul apelant viu e consola (Import1C); un contabil nu poate
genera o închidere nici din React, nici din XAF, iar documentele ITV existente
se văd (și se pot rescrie) accidental prin felia NTC, fiindcă TPT le întoarce ca
note contabile.

Felia livrează: (1) o ușă REST pentru generare/regenerare + comenzile standard,
(2) o proiecție de rezultat care spune CE s-a închis și DE CE nu s-a generat,
(3) ecranul React (lista lunilor închise + generarea + documentul), (4) ieșirea
ITV din felia NTC, (5) probe ModelCheck pe ușă + probe HTTP pe securitate.

## Deciziile

### F21-D1 — Scope: comandă + ecran de rezultat; zero schimbări de schemă

Nicio migrație, niciun set OData nou (`UnitateInterna` e deja pe OData
ReadOnly, `PoliticaInchidereTva` NU intră — F21-D12). Motorul de operare
(`MotorOperare`), gardianul anti-stale din `InchidereTva.ValideazaOperare` și
`RegistruContabil` rămân neatinse. Rutele, toate sub `[Route("api/itv")]`,
în `ItvController : ContaApiController`:

| verb | rută | corp / răspuns |
|---|---|---|
| GET | `` | `PaginaDto<ItvListDto>` (DataSourceLoader, doar `InchidereTva`) |
| GET | `{id:guid}` | `ItvReadDto` 200 / 404 / 403 |
| GET | `previzualizare?an=&luna=` | `PrevizualizareItvDto` 200 / 400 / 403 |
| POST | `genereaza` | `GenerareItvRequestDto` → `GenerareItvRezultatDto` 200 / 400 / 403 / 422 |
| POST | `{id:guid}/regenereaza` | `GenerareItvRezultatDto` 200 / 404 / 403 / 422 |
| DELETE | `{id:guid}` | 204 / 422 (doar Draft; ștergere AMÂNATĂ, 60a) |
| POST | `{id:guid}/opereaza` · `anuleaza` · `storneaza` · `valideaza` | ca la NTC (`OperareRezultatDto` / `EroriDto`) |

Acțiunea XAF de generare rămâne aditivă (comentariul din `InchidereTva.cs`,
44) — NU intră (restanță cu nume, F21-D12).

### F21-D2 — Serviciul capătă REZULTAT cu cauză; `Genereaza` rămâne pentru apelanții vechi

`InchidereTvaService.Genereaza` colapsează azi trei cauze de `null` (profil
inert / închidere vie / solduri zero) pe care ecranul trebuie să le distingă.
Se adaugă, în `Module/Motor/InchidereTvaService.cs`:

```csharp
public enum MotivNegenerare { ProfilInert, InchidereVie, FaraSold, NeCronologica }
  // cu [XafDisplayName] pe fiecare membru — etichetele vin în metadata.json (57f)
public sealed record RezultatInchidere(
    InchidereTva Document,           // null când Motiv != null
    MotivNegenerare? Motiv,
    decimal? Sold4426, decimal? Sold4427,   // null pe ProfilInert (nu există conturi)
    Guid? InchidereVieId,            // pe InchidereVie/NeCronologica: documentul care blochează
    LiniiInchidere Linii);           // ce s-ar genera / s-a generat
public readonly record struct LiniiInchidere(decimal Transfer, decimal DePlata, decimal DeRecuperat);

public static LiniiInchidere CalculeazaLinii(decimal sold4426, decimal sold4427);   // PURĂ
public static RezultatInchidere Incearca(IObjectSpace os, int an, int luna, Guid unitateId);
public static RezultatInchidere Previzualizeaza(IObjectSpace os, int an, int luna);  // NU scrie
public static InchidereTva Genereaza(IObjectSpace os, int an, int luna, Guid unitateId)
    => Incearca(os, an, luna, unitateId).Document;   // Import1C și probele vechi neatinse
```

Reguli: (a) `CalculeazaLinii` e SINGURA aritmetică a celor trei linii
(transfer = min, de plată = excedent colectată, de recuperat = excedent
deductibilă; rotunjire `Scara.RotunjesteBani`) — generatorul o consumă, deci
previzualizarea nu e o copie a calculului, e același calcul (42c; proba de
egalitate rămâne obligatorie, F21-D9). (b) `Incearca` păstrează EXACT ordinea
gardienilor de azi: profil → închidere vie → cronologie → solduri → TRZ;
cronologia rămâne `throw OperareException` (46c: refuz zgomotos, nu raport) —
`NeCronologica` ca MOTIV apare doar în `Previzualizeaza`, care e raport, nu
comandă. (c) `Incearca` NU comite (contractul de apelant din antetul
serviciului). (d) `Solduri` devine `public` (Apply-ul o consumă pentru
`Stale`). (e) Tipul TRZ lipsă rămâne `OperareException`. (f) `unitateId` care
nu e `UnitateInterna` ⇒ `OperareException` la `Incearca` („laturile închiderii
sunt interne" — oglinda gardului din `NotaContabila.ValideazaOperare`, adusă
la GENERARE, nu abia la operare).

### F21-D3 — Gate-ul: generarea se autorizează pe TIP, cifrele motorului nu se filtrează tăcut

Comanda de generare n-are subiect (produce documentul), deci
`ComandaAutorizata<T>(id)` nu se aplică (76-r4). Se adaugă în
`ContaApiController`, lângă `PoateCiti`:

```csharp
protected bool PoateCrea(Type tip, IObjectSpace os) =>
    securitate is IRequestSecurityStrategy cerinte && cerinte.CanCreate(tip, os);
// IsGrantedExtensions.CanCreate(this IRequestSecurityStrategy, Type, IObjectSpace) — DevExpress 26.1.3
```

Ușile, pe rute:

- `POST genereaza`: `PoateCrea(typeof(InchidereTva), osSecured)` ⇒ altfel 403;
  apoi `Incearca` + commit condiționat pe ușa NON-SECURED (58c: precedentul
  `genereaza-descarcare`; draftul e scris de un serviciu, nu cules).
- `GET previzualizare`: `PoateCiti(typeof(InchidereTva), osSecured)` ⇒ altfel
  403; calculul pe ușa NON-SECURED. Motiv: previzualizarea e dry-run-ul unei
  comenzi, nu o listă — soldurile calculate pe rânduri filtrate de permisiuni
  ar fi o cifră FALSĂ prezentată ca „nu e nimic de închis" (argumentul 73g
  pentru fișierul SAF-T; precedentul `Valideaza` = dry-run pe non-secured
  după gate).
- `GET {id}`: gate pe instanță pe ușa SECURED (`GetObjectByKey<InchidereTva>`
  null ⇒ 404; `CanRead` fals ⇒ 403), apoi DTO-ul pe ușa NON-SECURED — DTO-ul
  poartă `Sold4426Curent`/`Sold4427Curent`/`Stale`, cifre ale motorului, cu
  același argument. Se adaugă `AutorizeazaCitire<T>(Guid id)` în bază (același
  tipar ca `Autorizeaza<T>`, cu `CanRead` în loc de `CanWrite`).
- `GET` lista: ușa SECURED, ca orice listă (lista goală pentru `User` e un
  răspuns adevărat).
- `regenereaza` / `opereaza` / `anuleaza` / `storneaza` / `valideaza`:
  `ComandaAutorizata(id)` (404/403), apoi ușa NON-SECURED — identic NTC.
- `DELETE {id}`: ușa SECURED prin `InchidereTvaApply.Sterge` (draft; gardianul
  permite ștergerea unui Draft) — identic NTC.

### F21-D4 — Unitatea internă e PARAMETRU cules, nu dedusă

Modelul n-are „unitatea internă a societății" (`Societate` nu poartă FK spre
`UnitateInterna`; Import1C dă `Catalog.SediuId`). `GenerareItvRequestDto {
int An; [Range(1,12)] int Luna; Guid UnitateId }`, toate obligatorii; luna
invalidă ⇒ 400 `EroriDto` (70f). Ecranul culege unitatea prin `Lookup` remote
pe setul OData `UnitateInterna` (există, ReadOnly) și o PRECOMPLETEAZĂ când
setul are exact un rând (un default care nu minte: cu două unități nu alege).
`regenereaza` refolosește `PredatorId` al draftului — nu cere unitatea din nou.

### F21-D5 — ITV iese din felia NTC

Sub TPT `GetObjectsQuery<NotaContabila>()` întoarce și `InchidereTva`, deci azi
un ITV apare în `GET api/ntc`, se deschide la `/ntc/{id}` și un Draft se poate
rescrie prin `PUT api/ntc/{id}` (reconcilierea acceptă orice linii; gardianul
anti-stale l-ar prinde abia la operare). Se închide în `NotaContabilaApply`:

- `Lista` și `Citeste`: `.Where(d => !(d is InchidereTva))` (EF traduce `is`
  pe TPT prin frunză) — `GET api/ntc/{id}` pe un ITV ⇒ 404.
- `Aplica` (PUT pe id existent) și `Sterge`: dacă obiectul încărcat e
  `InchidereTva` ⇒ `OperareException` „Documentul … e o închidere de TVA — se
  gestionează din ecranul ei (/itv)." ⇒ 422. (`is` aici e la graniță, în
  Apply, nu în motor — regula „motorul nu cunoaște frunzele" nu e atinsă.)
- Comenzile `opereaza/anuleaza/storneaza` ale NTC pe un id ITV rămân
  PERMISE: sunt `OperareApi` pe `Document`, agnostic la tip, și produc același
  rezultat ca pe ruta ITV. Nu se dublează gardul acolo.

Probă ModelCheck obligatorie pe fiecare din cele patru uși.

### F21-D6 — DTO-urile de citire

`Module/Api/Itv/InchidereTvaDtos.cs`:

```csharp
public sealed class ItvListDto { Guid Id; string Numar; DateOnly Data; int An; int Luna;
    string Stare; DateTime? DataOperare; string UnitateDenumire; decimal Total; }
public sealed class ItvLinieReadDto { Guid Id; string Descriere; Guid ContDebitId; string ContDebitSimbol;
    Guid ContCreditId; string ContCreditSimbol; decimal Valoare; }
public sealed class ItvReadDto {
    Guid Id; string Numar; DateOnly Data; int An; int Luna; string Stare; DateTime? DataOperare;
    Guid UnitateId; string UnitateDenumire; decimal Total;
    List<ItvLinieReadDto> Linii;
    // rolurile liniilor, identificate prin CONTURILE POLITICII (29), niciodată prin simbol:
    decimal Transfer; decimal DePlata; decimal DeRecuperat;
    // cifra motorului la Data documentului (InchidereTvaService.Solduri) + verdictul anti-stale
    decimal Sold4426Curent; decimal Sold4427Curent;
    bool? Stale;           // doar pe Draft: CalculeazaLinii(solduri curente) != (Transfer, DePlata, DeRecuperat); null altfel
    bool PoateOpera; bool PoateAnula; bool PoateStorna; bool PoateSterge; bool PoateRegenera;
}
public sealed class PrevizualizareItvDto { int An; int Luna; string Motiv /*null = se poate genera*/;
    decimal? Sold4426; decimal? Sold4427; decimal Transfer; decimal DePlata; decimal DeRecuperat;
    Guid? InchidereVieId; string InchidereVieNumar; string InchidereVieStare; }
public sealed class GenerareItvRequestDto { int An; [Range(1,12)] int Luna; Guid UnitateId; }
public sealed class GenerareItvRezultatDto { Guid? DocumentId; string Motiv; Guid? InchidereVieId;
    decimal? Sold4426; decimal? Sold4427; decimal Transfer; decimal DePlata; decimal DeRecuperat; }
```

Affordances: `PoateOpera = PoateSterge = PoateRegenera = Draft`;
`PoateAnula = PoateStorna = Operat && !AreImperecheri` (oglinda 57d, chiar dacă
ITV nu poate stinge — capacitatea lui e dicționar gol). `Motiv` pe sârmă =
numele membrului enum (string, 57f). `Stale` refolosește EXACT criteriul
gardianului din `InchidereTva.ValideazaOperare` (aceleași perechi de conturi,
aceeași `Solduri` la `Data`) — dacă cele două diverg, ecranul minte;
ModelCheck le pune față în față (F21-D9).

### F21-D7 — Comenzile de generare, regenerare și storno

- `InchidereTvaApply.Genereaza(osNonSecured, cerere)`: `Incearca` →
  `CommitChanges` DOAR când `Document != null` → `GenerareItvRezultatDto`. 200
  și când `Motiv != null` — e un RAPORT, ca `DscId = null` la backorder (58) și
  ca lotul ANAF (72e); 422 doar pe `OperareException` (cronologie, TRZ,
  unitate ne-internă).
- `InchidereTvaApply.Regenereaza(osNonSecured, id)`: doar pe Draft (altfel 422);
  șterge draftul cu liniile lui (același mecanism ca `Sterge`) și COMITE, apoi
  `Incearca(an, luna, PredatorId)` + commit condiționat, în ACELAȘI OS. Două
  commit-uri secvențiale, nu o tranzacție: ștergerea amânată pune `GCRecord`, iar
  filtrul global al ObjectSpace-ului ascunde draftul vechi la a doua interogare
  a idempotenței; cazul „ștergerea a reușit, generarea a dat `FaraSold`" e
  legitim (luna nu mai are ce închide) și iese ca raport cu `DocumentId = null`.
  Fereastra dintre commit-uri e limitarea asumată 25f (fără operatori
  concurenți).
- Storno: ruta standard; ecranul propune ca dată IMPLICITĂ `Data` documentului
  (ultima zi a lunii), cu indiciul „stornarea la chiar data închiderii
  permite regenerarea lunii" (disciplina 46f, singura ei consecință pentru
  operator). Data rămâne editabilă.
- Anularea operării: standard (25d — doar fără dependenți; ITV n-are).

### F21-D8 — Clientul: `felii/itv`, două ecrane, `rutaTip` capătă `ITV`

- `api.ts` de mână cu tipurile generate (43d): `citeste`, `previzualizare(an,
  luna)`, `genereaza(cerere)`, `regenereaza(id)`, `sterge`, `valideaza`,
  `opereaza`, `anuleaza`, `storneaza`, `storeLista`.
- `ItvLista.tsx` (`/itv`): (1) bara de generare — an + lună prin `<select>`
  nativ (precedentul SAF-T: listă închisă, fără cerere per tastă), unitatea prin
  `Lookup` pe `UnitateInterna` (F21-D4), starea în URL (`?an=&luna=`, 43c);
  (2) sub bară, PREVIZUALIZAREA lunii alese (`useQuery` pe `previzualizare`):
  soldurile 4426/4427, cele trei linii care s-ar genera, sau motivul tradus prin
  `labelEnum('MotivNegenerare', …)` cu link către închiderea vie când există
  (`InchidereVieId`); butonul „Generează" e activ doar când `Motiv == null`;
  la 200 cu `DocumentId` navighează la `/itv/{id}`; refuzul (422) se arată
  doar cât e al cererii de pe ecran (tiparul SAF-T „cerere === cale"); (3)
  grila `DataGrid` remote pe `storeLista` (Numar, Data, Stare, Unitate, Total;
  `HeaderFilter` cu `Search`, 78), dublu-click ⇒ `/itv/{id}`. Clientul NU
  calculează nimic — nici sumele, nici verdictul (42c).
- `ItvDetaliu.tsx` (`/itv/:id`) pe `DocumentShell` (fără formular — antetul e
  read-only: Numar, Data, luna, unitatea, starea): tabelul liniilor (Descriere,
  cont debit, cont credit, valoare), panoul „Soldurile de TVA la data
  închiderii" (4426/4427 curente + transfer/de plată/de recuperat din DTO),
  `PanouErori fel="atentie"` când `Stale === true` („Soldurile s-au schimbat de
  la generare — regenerați închiderea"); comenzile: Operează, Anulează
  operarea, Stornează (`cereData` cu `implicit = Data`, F21-D7), Regenerează
  (`ConfirmareInline`), Șterge (`ConfirmareInline`), Înapoi la listă. Toate
  prin `e.event`/`ConfirmareInline`, fără `window.confirm` (57f/77d).
- `nucleu/stingeri.ts`: `case 'ITV': return \`/itv/${id}\`` — rândurile ITV din
  fișă și registrul-jurnal devin link (61a: felia există acum). Comentariul
  care numea ITV ca exemplu de „tip fără felie" se actualizează.
- `App.tsx`: rutele `/itv` și `/itv/:id`; meniul, grupul „TVA și declarații",
  itemul „Închidere TVA" înaintea lui „Decont TVA".
- Captions prin `campMeta('InchidereTva', …)` (moștenește membrii
  `Document`/`NotaContabila` din dump) și `labelEnum('StareDocument', …)`;
  `MotivNegenerare` intră în `metadata.json` prin `--dump-metadata` — dump-ul
  ia DOAR tipurile din spațiul `…Module.BusinessObjects` (`MetadataDump.
  EsteRelevant`), deci enum-ul stă în `BusinessObjects/Comun/Enums.cs`, lângă
  celelalte enum-uri de sârmă, nu în `Motor/` (constatat la pasul 1; `FelGolire`,
  `FelIdSaft`, `ProfilContabil` rămân în afara dump-ului, deliberat).

### F21-D9 — ModelCheck: probe de UȘĂ, pe scena proprie

Semantica de motor e deja acoperită (`E2E-ITV`, lunile 9/10 2026). Blocul nou
`E2E-API-ITV`, pe profilul PRIVAT, pe luni proprii (11/12 2026, cu precondiție
MĂSURATĂ: solduri 0 și nicio închidere vie ulterioară; dacă lunile sunt
ocupate, agentul se oprește și raportează), curățenie prin `Purja` (70e).
Probele minime:

1. `Previzualizeaza` pe lună fără TVA ⇒ `FaraSold`, solduri 0/0; după FCT+FCL
   în lună ⇒ `Motiv == null`, `Linii == CalculeazaLinii(solduri)`.
2. `InchidereTvaApply.Genereaza` ⇒ `DocumentId != null`, liniile draftului ==
   `Linii` ale previzualizării (cusătura previzualizare ↔ generare; 42c).
3. A doua generare ⇒ `InchidereVie` cu `InchidereVieId` = draftul; `Incearca`
   pe luna anterioară ⇒ `OperareException`; `Previzualizeaza` pe luna anterioară
   ⇒ `NeCronologica` cu `InchidereVieId` (raport vs comandă, F21-D2b).
4. `Citeste` pe draft: `Stale == false`, `Transfer/DePlata/DeRecuperat` ==
   liniile; după încă o FCT în lună ⇒ `Stale == true` ȘI `OperareApi.Valideaza`
   dă exact eroarea gardianului anti-stale (DTO-ul și gardianul spun același
   lucru); `Regenereaza` ⇒ draft NOU (`DocumentId` diferit), `Stale == false`,
   draftul vechi șters logic (`GCRecord`).
5. `OperareApi.Opereaza` ⇒ Operat, `Numar` ITV-; `Citeste` ⇒ `Stale == null`,
   `PoateStorna`; storno la `Data` ⇒ `Previzualizeaza` din nou `Motiv == null`.
6. F21-D5: `NotaContabilaApply.Lista` nu conține id-ul ITV, `Citeste` ⇒ null,
   `Aplica` pe id ITV ⇒ `OperareException`, `Sterge` ⇒ `OperareException`;
   `InchidereTvaApply.Lista` conține DOAR ITV (niciun NTC de scenă).
7. `Incearca` cu `unitateId` = un `Partener` ⇒ `OperareException` (F21-D2f).
8. Bugetar: `Previzualizeaza` ⇒ `ProfilInert` cu solduri null;
   `InchidereTvaApply.Lista` goală; nimic scris.
9. Curățenia: purjă fizică, soldurile 4426/4427 la 31.12 revin la 0.

`--dump-metadata` obligatoriu (enum nou); driftul openapi verificat.

### F21-D10 — Securitatea se MĂSOARĂ pe HTTP (66h), în §Închidere

Pe Privat, cu `Admin` și `User` (fără permisiuni): `previzualizare` Admin 200 /
User 403; `genereaza` Admin 200 cu `DocumentId`, a doua oară 200 cu
`Motiv = InchidereVie`, luna anterioară unei închideri vii 422, `Luna = 13`
400 `EroriDto`; User `genereaza` 403 (înainte de orice scriere — se verifică că
nu apare draft); `GET api/itv` User 200 gol; `GET api/itv/{id}` User 403 sau
404 (se notează care — familia 72-r10); `GET api/ntc/{id}` pe ITV Admin 404;
`PUT api/ntc/{id}` pe ITV Admin 422; `opereaza` Admin 200. Cifrele se scriu
în §Închidere.

### F21-D11 — Regula de oprire

Un agent se oprește și raportează (nu normalizează) când: `CanCreate` nu are
semnătura pin-uită; `is InchidereTva` nu se traduce în EF pe query-ul NTC;
filtrul global al ștergerii amânate NU ascunde draftul șters la a doua
interogare din `Regenereaza` (atunci ștergerea fizică sau al doilea OS devin
decizie a main-ului); lunile 11/12 2026 sunt ocupate în ModelCheck; enum-ul nu
apare în `metadata.json`; setul OData `UnitateInterna` nu răspunde la `Lookup`.

### F21-D12 — Ce NU intră (restanțe cu nume, deschise de felie)

- **79-r1** acțiunea XAF „Generează închiderea" (aditivă; 44/53).
- **79-r2** `PoliticaInchidereTva` pe OData ReadOnly + ecran React (familia
  77-r3 — politicile din React).
- **79-r3** închiderea perioadei fiscale din client (rămâne XAF/seed, 53i);
  ITV nu o atinge (46c).
- 36f (TVA la încasare, 4428, prorata) rămân amânate; cusătura D300 rd. 36/37
  (69f) rămâne cum e; Import1C neatins (wrapper-ul `Genereaza`).

## Pașii (un agent per pas, secvențial; main verifică și comite după fiecare)

1. **Module + ModelCheck** — `InchidereTvaService` (F21-D2), `Api/Itv/`
   (`InchidereTvaDtos.cs`, `InchidereTvaApply.cs`: `Lista`, `Citeste`,
   `Previzualizeaza`, `Genereaza`, `Regenereaza`, `Sterge`), F21-D5 în
   `NotaContabilaApply`, blocul `E2E-API-ITV` + probele bugetar (F21-D9),
   `--dump-metadata`. Verificare: ModelCheck 0 FAIL pe AMBELE profiluri.
2. **WebApi** — `PoateCrea`/`AutorizeazaCitire` în `ContaApiController`,
   `ItvController` (F21-D1/D3), `pnpm verifica:drift` (WebApi OPRIT) apoi
   probele HTTP F21-D10 pe host viu (Privat), cifrele în §Închidere.
3. **Client** — `felii/itv` + rute + meniu + `rutaTip` (F21-D8); `tsc` + `vite
   build`; smoke în browser pe Privat: previzualizare → generare → detaliu →
   operare → storno la data implicită → regenerare; link din fișa de cont.
4. **Review advers** (agent separat, scenarii concrete: gate-ul pe tip ocolit
   prin NTC, `Stale` care minte, regenerarea care lasă două drafturi, `User` pe
   `previzualizare`, luna cu închidere stornată, unitatea ne-internă) —
   fix-urile le aplică main-ul; apoi decizia 079 + README + CLAUDE.md §79 +
   istoric + §Închidere aici.

Comenzile de verificare:

```
cd nou/tools/ModelCheck && dotnet run                 # bugetar
cd nou/tools/ModelCheck && dotnet run privat          # privat
dotnet run --project nou/tools/ModelCheck -- --dump-metadata
cd nou/Atlas.Conta.Client && pnpm verifica:drift      # WebApi OPRIT
cd nou/Atlas.Conta.Client && pnpm build
```

Niciodată `--no-build` la `dotnet ef`; nu e pas de model, deci nicio migrație
— `dotnet ef migrations has-pending-model-changes` trebuie să rămână curat.

## Închidere (2026-09-01, decizia 79)

Trei commit-uri (`5fbd23e` Module + ModelCheck, `59625d6` WebApi + probe HTTP,
`ac77fdc` client + smoke) + review-ul advers (fix-uri + 23 de probe noi) și
docs. ModelCheck final: privat **944 OK / 0 FAIL**, bugetar **900 OK / 0
FAIL** (re-rulate independent de main după fiecare pas); `has-pending-model-changes` curat; codegen doar
adăugiri, idempotent; `metadata.json` cu `MotivNegenerare`.

### Devieri de la contract (toate raportate, nu normalizate)

- F21-D8 se înșela: dump-ul de metadata ia DOAR spațiul `BusinessObjects`
  (`MetadataDump.EsteRelevant`), deci `MotivNegenerare` stă în
  `BusinessObjects/Comun/Enums.cs`, nu în `Motor/`.
- Lunile 11/12 2026 SUNT folosite de scenele `E2E-ASM`/`E2E-RET`, care
  rulează după: blocul `E2E-API-ITV` stă imediat după `E2E-ITV`, cu
  precondiție măsurată al cărei nume spune ce rupe o reordonare.
- Gardul de unitate ne-internă stă ÎNAINTEA gardienilor de stare (e
  verificare de argument): altfel pe o lună fără sold cererea greșită ar fi
  ieșit `FaraSold`.
- `[Range(2000, 2100)]` pe `An` în cerere (lipsea; `An = 0` dădea 500 din
  `DateOnly`). Pe `previzualizare` validarea e explicită, cu parametri
  nullable (forma SAF-T/D300): „lipsă" ≠ `0`, toate erorile deodată.
- `Lookup` nu se poate folosi în afara unui formular (`useCamp`): unitatea
  se culege prin `SelectBox` pe setul OData citit întreg (F21-D4 cere
  NUMĂRUL rândurilor), tiparul fișei de cont.
- `GET api/ntc/{id}/candidati` pe un id ITV răspundea 200 (inert): închis
  minimal, `Candidati` ⇒ null ⇒ 404, cu probă anti-vacuă.
- **Din review (decizia 79, §Review advers)**: F21-D7 „două commit-uri" a
  MURIT — `Regenereaza` = `Incearca(…, inlocuieste: id)` înaintea ștergerii,
  o singură tranzacție (F2); cronologia capătă sensul invers la generare
  (`DraftAnterior`) și gard la OPERARE (F1); `PerioadaInchisa` (M2);
  `LiniiPotrivescSoldurile` = criteriul unic anti-stale, `Stale = null` pe
  politică incompletă (M4); `regenereaza` cere și `PoateCrea` (M5);
  simbolurile conturilor în previzualizare vin din politică (M6);
  `previzualizare` în `Domeniu` (M7); Import1C cere SEDIU/COMISIE pe
  `UnitateInterna` (M9).

### Probele HTTP (F21-D10) — Privat, host viu, `Admin` + `User`

Baza avea `ITV-1`…`ITV-12` (2025, Operat). Luna probei L = 09/2026 (solduri
357 / 8.463; 08/2026 are și ea TVA, deci proba cronologică nu e vacuă).

| # | cerere | user | cod | corp (scurt) | ms |
|---|---|---|---|---|---|
| P1 | `GET api/itv/previzualizare?an=2026&luna=9` | Admin | 200 | `Motiv:null, Sold4426:357, Sold4427:8463, Transfer:357, DePlata:8106, DeRecuperat:0` | 52 |
| P2 | idem | User | 403 | | 151 |
| P3 | `POST genereaza {Luna:13}` | Admin | 400 | `Erori:["Luna: The field Luna must be between 1 and 12."]` (engleză — 79-r4) | 16 |
| P4 | `previzualizare?an=1999` | Admin | 400 | `„an” trebuie să fie între 2000 și 2100.` | 6 |
| P5 | `previzualizare?luna=13` | Admin | 400 | `„luna” trebuie să fie între 1 și 12 — …` | 6 |
| P6 | `POST genereaza` cu `UnitateId` = Partener | Admin | 422 | `Laturile închiderii de TVA sunt unitatea internă … „Mikro Atlas - FURNIZOR” nu e o unitate internă.` | 50 |
| P7 | `POST genereaza {2026,9,SEDIU}` | User | 403 | | 15 |
| P8 | control după P7: `GET api/itv` | Admin | 200 | `totalCount: 12` — niciun draft scris | 162 |
| P9 | `GET api/itv` | User | 200 | `data:[], totalCount:0` | 35 |
| P10 | `POST genereaza {2026,9,SEDIU}` | Admin | 200 | `DocumentId:…, Motiv:null, Transfer:357, DePlata:8106` | 900 (rece) |
| P11 | idem, a doua oară | Admin | 200 | `DocumentId:null, Motiv:"InchidereVie", InchidereVieId:…` | 39 |
| P12 | `GET api/itv/{id}` | Admin | 200 | Draft, `Numar:null`, 2 linii, `Stale:false`, `PoateOpera/Sterge/Regenera:true` | 202 |
| P13 | `GET api/itv/{id}` | User | 404 | invizibil, nu interzis (familia 72-r10) | 110 |
| P14 | `GET api/ntc/{id}` pe id ITV | Admin | 404 | | 27 |
| P15 | `GET api/ntc/{id}/candidati` pe id ITV | Admin | 404 | | 120 |
| P16 | `PUT api/ntc/{id}` pe id ITV | Admin | 422 | `Documentul (30.09.2026) e o închidere de TVA — se gestionează din ecranul ei (/itv).` | 73 |
| P17 | `POST {id}/valideaza` | Admin | 200 | `Erori:[]` | 1188 (rece) |
| P18 | `POST {id}/regenereaza` | Admin | 200 | `DocumentId` NOU | 255 |
| P19 | `GET api/itv/{idVechi}` | Admin | 404 | draftul vechi șters logic | 45 |
| P20 | `GET api/itv/{idNou}` | Admin | 200 | aceleași cifre, `Stale:false` | 80 |
| P21 | `POST {idNou}/opereaza` | Admin | 200 | `StareNoua:"Operat"` | 502 |
| P22 | `GET api/itv/{idNou}` | Admin | 200 | `Numar:"ITV-13"`, `Sold*Curent:0`, `Stale:null`, `PoateAnula/PoateStorna:true` | 85 |
| P23 | `POST genereaza {2026,8,SEDIU}` | Admin | 422 | `Există o închidere de TVA vie pentru o lună ulterioară lui 08/2026 — închiderile se generează cronologic.` | 42 |
| P24 | `previzualizare?an=2026&luna=8` | Admin | 200 | `Motiv:"NeCronologica", InchidereVieNumar:"ITV-13", InchidereVieStare:"Operat"` | 42 |
| P25 | `DELETE api/itv/{idNou}` (Operat) | Admin | 422 | `Închiderea ITV-13 nu mai e Draft (starea „Operat”) — nu se șterge.` | 102 |
| P26 | `DELETE api/itv/{idNou}` | User | 404 | | 222 |
| P27 | `POST {idNou}/storneaza {"Data":"2026-09-30"}` | Admin | 200 | `StareNoua:"Stornat"` | 717 |
| P28 | `GET api/itv/{idNou}` | Admin | 200 | Stornat, liniile neatinse | 80 |
| P29 | `previzualizare?an=2026&luna=9` după storno | Admin | 200 | `Motiv:null` — luna redevine închiderabilă | 39 |
| P30 | `previzualizare?an=2026&luna=8` după storno | Admin | 200 | `Motiv:null` | 38 |
| P31 | `DELETE api/itv/{id}` pe un Draft | Admin | 204 | | 161 |

Timpi la cald (mediana din 5): lista 112 ms, previzualizare 38 ms, citire
80 ms, generare 69 ms — sub pragul 59.

Capcană de probare: `genereaza` SCRIE ori de câte ori luna e liberă — un
„retry de măsurare" pe altă lună a creat un draft (șters cu `DELETE` ⇒ 204).
Baza Privat de dev a rămas cu `ITV-13` (09/2026) și `ITV-14` (10/2026, din
smoke) STORNATE la chiar data lor — soldurile 4426/4427 neatinse.

### Smoke în browser (Privat, Admin) — 11/11

Meniu → `/itv` (13 rânduri) → 10/2026: previzualizare 357/8.463 + linii,
„Generează" inactiv fără unitate, `SelectBox` gol (două unități ⇒ fără
precompletare) → 03/2026: „Luna n-are sold de TVA de închis" → generare ⇒
`/itv/{id}` Draft, 2 linii, fără atenție → Verifică ⇒ „trece toți gardienii"
→ Regenerează (confirmare) ⇒ id nou → Operează ⇒ `ITV-14`, solduri curente
0/0 → listă 09/2026: „Există o închidere pentru o lună ulterioară" cu link
`ITV-14` → Stornează cu data precompletată 31.10.2026 ⇒ Stornat → `/jurnal`
pe 31.10.2026: `ITV-14` e link → `/itv?an=2026&luna=10` reîncărcat păstrează
luna. Consolă fără erori, zero `[campMeta]`, toate cererile 200.

### Restanțe cu nume, deschise de felie

79-r1 acțiunea XAF de generare · 79-r2 `PoliticaInchidereTva` pe OData +
ecran · 79-r3 închiderea perioadei fiscale din client · 79-r4 mesajul
`[Range]` în engleză (70-r5) · 79-r5 `Stale` mai strict decât gardianul
(doar pe linii editate de mână — cale închisă de F21-D5). Textul integral
în `docs/decizii/079-p5-felia21-itv.md`.
