# Pasul 5, felia 22 — refuzurile de acces pe toate ușile: 404 / 403 / 422 (contract)

Data: 2026-09-02. Stare: închisă (decizia 80). Restanța de fond 77-r8 (familia
70-r1 / 72-r10 / 76-r4 / 76-r5 / 77k / 79b / 79-r6). Decizia rezultată: 080.

## Scop

Azi fiecare ușă răspunde altfel la aceeași întrebare („n-ai voie"):

| Ușa | Azi | Problema |
|---|---|---|
| REST, comenzi (`opereaza`/`anuleaza`/`storneaza`/`valideaza`) | `ComandaAutorizata<Document>`: invizibil 404, fără Write 403, corp GOL | gate pe `Document`, nu pe tipul feliei ⇒ un id de NIR pe `api/fct/{id}/opereaza` trece gate-ul și pică 422 din Apply (76-r4) |
| REST, scriere (`POST`/`PUT`/`DELETE` prin `*Apply` pe OS secured) | NICIUN gate; `User` e refuzat de primul FK invizibil („nu există în nomenclatorul…", 422) sau de `GardianEditare` (422); dacă totul e vizibil, `SaveChanges` aruncă `UserFriendlyEFCoreSecurityException` ⇒ filtrul DevExpress ⇒ 403 `text/plain` englezesc | codul e al regulii, nu al dreptului; mesajul spune „nu există" unde adevărul e „nu e vizibil" (76-r5) |
| OData (nomenclatoare) | `DataService.GetObjectByKey` ⇒ `HttpUserFriendlyException(404,"Not Found")`; refuz la commit ⇒ 403; ambele `text/plain` prin `UserFriendlyExceptionFilter`; gardianul (`Committing`) rulează ÎNAINTEA verificării de securitate din `SaveChanges` ⇒ `User` primește 422 înaintea lui 403 (77k) | corpul nu e `EroriDto`; ordinea e a domeniului, nu a dreptului |
| ITV (79b) | `AutorizeazaCitire<T>` pe instanță, `PoateCiti`/`PoateCrea` pe tip; cifrele pe ușa non-secured | cine citește `InchidereTva` vede soldurile societății fără drept pe `RegistruContabil` (79-r6) |
| Client | `http.ts`: 403/404 traduse DOAR pe `/api/odata/`, cu texte inventate în TS; pe REST ⇒ `Error("POST … → 403 Forbidden")` brut; `dxStore.onAjaxError` ignoră 403/404 | operatorul vede statusul, nu motivul |

Mecanica de dedesubt e fixă și măsurată pe surse (DevExpress 26.1.3):
`BaseObjectSpace.CommitChanges` cheamă `OnCommitting` (gardianul nostru)
ÎNAINTE de `DoCommit` → `SaveChanges` → `SecurityStateManager.GetEntriesToSave`
(verificarea Create/Write/Delete per entitate ⇒ `EFCoreSecurityException`,
reîmpachetată de `SecuredEFCoreObjectSpace.DoCommit` ca
`UserFriendlyEFCoreSecurityException : IUserFriendlySecurityException`);
`SecurityQueryCompiler` înfășoară ORICE query (și `Find`) cu predicatul de
securitate ⇒ `GetObjectByKey` pe OS secured întoarce `null` pentru invizibil,
nedistins de inexistent; `UserFriendlyExceptionFilter` face `ContentResult`
(text/plain) cu 403 pe `IUserFriendlySecurityException`, 400 pe restul
`IUserFriendlyException`, statusul propriu pe `HttpUserFriendlyException`.

Felia fixează O SINGURĂ regulă, o aplică pe toate ușile și o MĂSOARĂ pe HTTP
cu trei utilizatori (66h). Zero schimbări de schemă, zero schimbări de motor.

## Deciziile

### F22-D1 — Trei coduri, trei întrebări, o singură ordine

- **404** = „subiectul cererii nu e vizibil pentru tine": inexistent SAU
  invizibil prin securitate, DELIBERAT nedistinse (altfel API-ul e un oracol
  de existență pentru rândurile pe care securitatea le ascunde). Doar pe
  rutele cu subiect (`{id}` în rută).
- **403** = „subiectul e vizibil (sau întrebarea e pe TIP), dar operația
  cerută nu ți-e permisă": Create pe tip, Write/Delete pe instanță, Read pe
  tip (fișiere, cifre ale motorului).
- **422** = refuz de DOMENIU, pe o cerere pe care AI dreptul s-o faci.
  Un 422 nu poate ascunde un refuz de permisiune.
- **200 filtrat** rămâne răspunsul listelor și sumarelor (69g/71g): o listă
  goală e un adevăr, o cifră însumată peste rânduri ascunse nu e (73g/79b).

Ordinea pe sârmă, pe TOATE ușile: **401 → 400 (binding) → 404 → 403 → 422**.
Autorizarea vine ÎNAINTEA domeniului; sintaxa (400) e a pipeline-ului și
rămâne unde e.

### F22-D2 — Ușa de scriere REST capătă gate explicit, pe tipul FELIEI

Închide 76-r4 și 76-r5 pe REST. În fiecare controller de document:

- `POST` (creare) ⇒ `PoateCrea(typeof(T))` pe OS secured, ÎNAINTE de Apply
  ⇒ 403;
- `PUT {id}` ⇒ `Autorizeaza<T>(id, Write)` ⇒ 404 invizibil / 403 fără Write;
- `DELETE {id}` ⇒ `Autorizeaza<T>(id, Delete)` ⇒ 404 / 403 fără **Delete**
  (nu Write — sunt permisiuni distincte în XAF);
- comenzile (`opereaza`/`anuleaza`/`storneaza`/`valideaza`) rămân pe
  `ComandaAutorizata<T>` — cu **`T` = tipul feliei**, nu `Document`: un id de
  alt tip e 404 („nu e vizibil pe ușa asta"), nu 422 din Apply.
- `GET {id}` rămâne pe OS secured fără gate (invizibil ⇒ 404 natural); ITV
  păstrează `AutorizeazaCitire<T>` fiindcă răspunde cu cifrele motorului.
- `ImperecheriController`: `POST` ⇒ `PoateCrea(typeof(Imperechere))`,
  `DELETE` ⇒ `Autorizeaza<Imperechere>(id, Delete)`.
- `ParteneriController` (comanda ANAF single + lot): neschimbat ca formă
  (`ComandaAutorizata<Partener>` / `AutorizeazaLot<Partener>`), doar corpul
  (D4).

Gate-ul de instanță primește OPERAȚIA ca parametru (`Write`/`Delete`), nu se
dublează în două metode.

### F22-D3 — Gardianul întreabă securitatea ÎNAINTEA domeniului

Închide 77k pe OData și e plasa pentru orice cale care scapă lui D2.
`GardianEditare.OnCommitting` primește un pas ZERO: pentru fiecare obiect din
`ModifiedObjects`, întreabă strategia de securitate (`IRequestSecurityStrategy`
prin `IsGrantedExtensions`: nou ⇒ `CanCreate(tip)`, șters ⇒
`CanDelete(obiect)`, modificat ⇒ `CanWrite(obiect)`) și, la primul refuz,
aruncă o excepție care implementează `IUserFriendlySecurityException` cu
mesajul din D4. Abia apoi `Verifica(os)` (domeniul).

- Strategia vine prin **injecție în constructor** (`GardianEditare` e
  înregistrat `Scoped` prin `TryAddEnumerable`, ca customizerii XAF; în ambele
  host-uri `ISecurityStrategyBase` e scoped) — cu parametru NULLABLE: fără
  strategie (ModelCheck, ușa de sistem) pasul zero TACE. Dacă injecția nu
  merge în vreun host, agentul se oprește și raportează (regula de oprire),
  nu ia strategia dintr-un service locator.
- Verificarea DevExpress din `SaveChanges` RĂMÂNE (plasa pe permisiuni de
  membru și pe ce nu vedem noi); doar ordinea față de domeniu se schimbă.
- `Verifica(os)` (statica, folosită de ModelCheck) rămâne NEATINSĂ: pasul
  zero e în `OnCommitting`, nu în `Verifica`.

### F22-D4 — Un singur corp: `EroriDto` pe 403 și 404, pe toate ușile, în română

- REST: `Autorizeaza`/`AutorizeazaCitire`/`PoateCiti`/`PoateCrea` întorc
  `NotFound(EroriDto)` / `StatusCode(403, EroriDto)`, niciodată `Forbid()`
  (corp gol) sau `NotFound()` gol; `[ProducesResponseType(typeof(EroriDto),
  403/404)]` pe acțiuni ⇒ `openapi.json` (adăugiri).
- OData: `RefuzDomeniuOdataFilter` devine **`RefuzOdataFilter`** și traduce și
  refuzurile de acces: `IUserFriendlySecurityException` ⇒ 403 `EroriDto`
  (mesajul nostru, NU textul englezesc DevExpress), `HttpUserFriendlyException`
  ⇒ statusul ei + `EroriDto` (404 „Not Found" ⇒ mesajul nostru de 404; alt
  status ⇒ mesajul ei). Comentariul „ce NU atinge" se rescrie. Filtrul
  DevExpress rămâne înregistrat (rutele XAF care nu sunt ale noastre).
- Mesajele, o singură sursă în Module (`Api/Refuzuri.cs`, statice):
  - 404: `Înregistrarea nu există sau nu e vizibilă pentru utilizatorul curent.`
  - 403 Create: `Nu aveți dreptul de a crea „{caption}”.`
  - 403 Write: `Nu aveți dreptul de a modifica „{caption}”.`
  - 403 Delete: `Nu aveți dreptul de a șterge „{caption}”.`
  - 403 Read (tip): `Nu aveți dreptul de a citi „{caption}”.`
  - `{caption}` = caption-ul XAF al clasei (`CaptionHelper.GetClassCaption`),
    cu fallback pe numele CLR; niciodată numele proxy-ului EF.
- Aceleași mesaje le aruncă gardianul (D3) și le pun controllerele; mesajul
  DevExpress din `SaveChanges` (plasa) trece prin filtru cu textul lui
  (englezesc, 70-r5) — asumat: pe calea normală nu se mai ajunge acolo.

### F22-D5 — Cifrele motorului cer dreptul de citire pe REGISTRUL din care se însumează

Închide 79-r6, aliniat cu fișierul SAF-T (73g). `GET api/itv/{id}` și
`GET api/itv/previzualizare` cer, pe lângă gate-ul de azi, `PoateCiti(typeof(
RegistruContabil))`; lipsa ⇒ 403 cu mesajul de Read pe registru. `genereaza`
și `regenereaza` produc rânduri (draft), nu cifre — rămân pe `PoateCrea`.
Regula generală: o rută care întoarce o SUMĂ peste un registru pe ușa
non-secured cere dreptul de citire pe TIPUL acelui registru.

### F22-D6 — FK invizibil pe ușa securizată = 422 cu mesaj ONEST, printr-un singur helper

Referința pe care apelantul n-o vede nu e subiectul cererii, deci nu e 404;
e o cerere pe care n-o poate formula corect ⇒ 422, dar mesajul spune adevărul:
`„{rol} ({id}) nu există sau nu e vizibil(ă) pentru utilizatorul curent."`.
Cele ~84 de `?? throw new OperareException($"… nu există …")` din
`Module/Api/*Apply.cs` trec printr-un helper unic `Rezolva.Cere<T>(os, id,
rol)` (în `Module/Api`), cu ACEEAȘI frază; comentariile care spuneau „nu
există" se aliniază. Nicio schimbare de comportament în afara textului.

### F22-D7 — Rolul `Cititori` + utilizatorul `Cititor` (dev-only) fac 403-ul măsurabil

Închide 72-r10. În blocul `#if !RELEASE` al `Updater`-ului: rolul `Cititori`
cu Read pe TOATE tipurile de business (`AllowRead` pe `BusinessObjects`,
navigație permisă), fără Create/Write/Delete, și utilizatorul `Cititor`
(parolă goală, ca `Admin`/`User`). Matricea probelor devine: `Admin` (422 pe
domeniu), `Cititor` (403 pe scriere, 200 pe citire), `User` (404 pe
instanțe, 403 pe tip, 200 gol pe liste). Seed-ul e idempotent ca la ceilalți
doi.

### F22-D8 — Clientul arată motivul, nu statusul

- `nucleu/http.ts`: orice 403/404 cu corp `Erori[]` ⇒ `EroareDomeniu`
  (cu proprietatea `status` doar informativ — niciun ecran nu ramifică pe
  ea); ramura specială `/api/odata/` și textele ei inventate MOR (serverul e
  sursa). Un 403/404 FĂRĂ `Erori[]` rămâne eroare tehnică (plasa).
- `nucleu/dxStore.ts` `onAjaxError`: pe 403/404/422 cu `Erori[]` în corp,
  `e.error` devine mesajul (join „\n"), ca grila/lookup-ul să arate motivul.
- Zero motor de reguli în TS (43b): clientul afișează.

### F22-D9 — Securitatea se MĂSOARĂ pe HTTP, printr-un script repetabil

ModelCheck n-are strategie de securitate (OS-uri neautentificate) și NU
capătă una în felia asta; probele lui rămân verzi pe ambele profiluri și
primesc o probă pe helper-ul D6 (fraza) — atât. Proba supremă e pe calea
reală (66h): `nou/tools/ProbeHttp/refuzuri.ps1` (PowerShell 7, `curl`/
`Invoke-WebRequest`, host `https://localhost:5001`, baza Privat) autentifică
`Admin`/`Cititor`/`User`, rulează matricea de mai jos și tipărește tabelul
Markdown al §Închidere (`# | cerere | user | așteptat | primit | corp scurt |
ms`) cu verdict PASS/FAIL per rând și cod de ieșire ≠ 0 la orice FAIL.
Scriptul NU lasă urme: drafturile create pe `Admin` se șterg la final;
capcana 79 (`genereaza` scrie când luna e liberă) se evită probând
`genereaza` DOAR ca `User`/`Cititor`.

Matricea minimă (fiecare rând × cei trei utilizatori unde are sens):

| Ușa | Cereri |
|---|---|
| REST scriere | `POST api/nir` (corp valid), `PUT api/nir/{draft}`, `DELETE api/nir/{draft}`; `POST api/imperecheri`, `DELETE api/imperecheri/{id}` |
| REST comenzi | `POST api/nir/{id}/opereaza` cu id de NIR draft; ACELAȘI id pe `api/fct/{id}/opereaza` (76-r4 ⇒ 404); `valideaza` |
| REST citire | `GET api/nir/{id}` (Admin 200 / User 404 / Cititor 200); `GET api/nir` (200 filtrat) |
| ITV | `GET api/itv/previzualizare` (Cititor 200; un rol cu ITV dar fără registru nu există pe bază — se declară), `GET api/itv/{id}`, `POST genereaza` (Cititor/User 403, fără draft scris) |
| OData | `GET api/odata/Partener({id})` (User 404 `EroriDto`), `POST api/odata/Partener` (Cititor 403 `EroriDto` ÎNAINTEA gardianului: corpul cu `Cod` gol trebuie să dea tot 403, nu 422 — proba lui D3), `PATCH` (Cititor 403, User 404), `DELETE` |
| Fișier | `GET api/saft/xml` (User 403 `EroriDto`) |

### F22-D10 — Regula de oprire

Agentul se oprește și raportează (nu normalizează tăcut) dacă: injecția
strategiei în `GardianEditare` nu funcționează într-un host; `IsGranted` pe OS
secured aruncă sau întoarce altceva decât se așteaptă pentru `Admin`; un
controller nu încape în forma D2 fără a schimba semantica Apply-ului; filtrul
OData nu apucă `HttpUserFriendlyException` înaintea celui DevExpress; o probă
HTTP dă un cod diferit de cel așteptat și cauza nu e a feliei; ModelCheck
pică pe orice profil.

### F22-D11 — Ce NU intră

Ușa XAF Blazor (are propriile mesaje; 78-r1 rămâne); permisiunile pe MEMBRU
(rămân la plasa DevExpress); 401 (neschimbat); listele (200 filtrat rămâne);
roluri de producție (doar cele trei de dev); 400 `[Range]` în engleză
(79-r4/70-r5); refuzurile pe `$expand` OData (nemăsurate, declarate).

## Pașii

1. **Module + ModelCheck** — `Api/Refuzuri.cs` (mesaje + excepția
   `RefuzAcces : Exception, IUserFriendlySecurityException` cu `Operatie`),
   `Api/Rezolva.cs` (D6) + înlocuirea celor ~84 de throw-uri, pasul zero în
   `GardianEditare` (D3), rolul/userul D7; ModelCheck verde pe ambele
   profiluri + proba D6.
2. **WebApi** — gate-urile D2 pe toate controllerele, corpurile D4, D5 pe
   ITV, `RefuzOdataFilter`; build; `pnpm gen:openapi && gen:types`
   (idempotent, doar adăugiri).
3. **Probe HTTP** — re-seed Privat (`Cititor`), scriptul D9, matricea
   rulată pe host viu, tabelul în §Închidere.
4. **Client** — D8, `tsc` + `vite build`, smoke minim în browser (un 403 pe
   REST și unul pe OData arată motivul în `PanouErori`/grilă).
5. **Review advers** cu scenarii concrete (oracol de existență, 422 înaintea
   lui 403 pe vreo cale rămasă, `Admin` refuzat de pasul zero, dublul
   commit al gardianului, `Cititor` cu drept pe ITV dar nu pe registru).
6. **Docs** — decizia 080, README, istoric, CLAUDE.md (§80 + restanțele
   închise: 70-r1, 72-r10, 76-r4, 76-r5, 77-r8, 79-r6; 79b amendat).

## Închidere (2026-09-02, decizia 80)

### Devieri de la contract

- **F22-D2, gate-ul NTC** (review M1): sub TPT `GetObjectByKey<NotaContabila>`
  găsește și `InchidereTva`, deci gate-ul spunea „vizibil" (403/422) acolo
  unde `Citeste` spunea 404. `Autorizeaza<T>` capătă un predicat opțional
  `peUsaAsta`; NTC îl dă (`d is not InchidereTva`) ⇒ 404 pe toate verbele.
  **Amendează 79c**: comenzile NTC pe un id ITV NU mai sunt permise (ITV are
  ușa lui, cu aceleași comenzi).
- **F22-D3/D2, obiectele NOI** (review M2): plasa DevExpress cere pe `Added`
  Create + Write + Read; pasul zero și `PoateCrea` întrebau doar Create. Acum
  `CanCreate(tip) && CanWrite(tip|obiect)`; mesajul rămâne „crea".
- **F22-D4, captionul**: pe WebApi nu există model de aplicație XAF, deci
  `CaptionHelper` întoarce numele complet și `Refuzuri.Caption` cade pe numele
  CLR („InchidereTva", nu „Închidere TVA") — 80-r5.
- **F22-D8, `ODataStore`**: `errorFromResponse` (DevExtreme) păstrează doar
  `statusText` pe ≥ 400 — motivul serverului nu ajunge în `errorHandler`;
  declarat (80-r1), nu cârpit cu texte inventate.
- **F22-D9, proba fișierului SAF-T pe `Cititor`** (review m4): „oricare ≠ 403"
  era vacuă pe 500 ⇒ PASS doar pe 200/422.
- **Terminatorii de linie**: agenții au rescris fișiere CRLF ca LF (comise
  așa în pașii 1/2/4) și au lăsat patru fișiere mixte; normalizate la
  terminatorul original în commit-ul de review. Repo-ul e mixt per fișier
  (`autocrlf=false`) — regula: se păstrează terminatorul fișierului.

### Review advers (Fable, read-only, 40 de probe HTTP proprii)

0 defecte de FOND; 2 MEDII (M1, M2 — fixate, cu probe în script); 5 MINORE:
m1 captionul (80-r5), m2 `400 {"Erori":["Incorrect body."]}` pe OData cu corp
non-JSON — mesajul DevExpress englezesc trecut prin filtru (80-r6, familia
70-r5), m3 rute cu cifre pe ușa non-secured în afara regulii (e):
`distribuie-valoarea` (ASM) întoarce sume din prețurile loturilor unui rol cu
Write pe ASM fără Read pe `Lot` (80-r7, familia 79-r6), m4 (fixată), m5
`EroareDomeniu.status` neconsumat (conform D8). Verificate și trecute: fără
oracol de existență (timpi identici invizibil/inexistent, `$filter`/`$count`/
`$expand` goale), niciun `Domeniu(Secured…)` fără gate, pasul zero nu rulează
pe ușa de sistem, obiect nou+șters = detached, `SelectDataSecurity` cache-uit
per OS, filtrele OData în ordinea 404 → 403 → 422, `ApplicationUser` nu e pe
OData.

### Probele HTTP (F22-D9) — Privat, host viu, `nou/tools/ProbeHttp/refuzuri.ps1`

Rulat de cinci ori (agent ×2, verificare independentă, după fix-urile
review-ului ×2), fără urme; ultima rulare, cu cele 4 probe M1:

| # | cerere | user | așteptat | primit | corp (scurt) | ms | verdict |
|---|---|---|---|---|---|---|---|
| 1 | `POST /api/nir` | Admin | 201 (draftul de lucru) | 201 application/json | {"Id":"{id}","Numar":null,"Data":"2026-09-02","Stare":"Dra… | 788 | **PASS** |
| 2 | `POST /api/nir` | Cititor | 403 + „crea” | 403 application/json | {"Erori":["Nu aveți dreptul de a crea „NIR”."]} | 13 | **PASS** |
| 3 | `POST /api/nir` | User | 403 + „crea” (76-r5: NU 422 „nu există”) | 403 application/json | {"Erori":["Nu aveți dreptul de a crea „NIR”."]} | 14 | **PASS** |
| 4 | `PUT /api/nir/{id}` | Cititor | 403 + „modifica” | 403 application/json | {"Erori":["Nu aveți dreptul de a modifica „NIR”."]} | 129 | **PASS** |
| 5 | `PUT /api/nir/{id}` | User | 404 + „nu există sau nu e vizibil” | 404 application/json | {"Erori":["Înregistrarea nu există sau nu e vizibilă pentru utilizatorul curent."]} | 96 | **PASS** |
| 6 | `DELETE /api/nir/{id}` | Cititor | 403 + „șterge” | 403 application/json | {"Erori":["Nu aveți dreptul de a șterge „NIR”."]} | 61 | **PASS** |
| 7 | `DELETE /api/nir/{id}` | User | 404 + „nu există sau nu e vizibil” | 404 application/json | {"Erori":["Înregistrarea nu există sau nu e vizibilă pentru utilizatorul curent."]} | 43 | **PASS** |
| 8 | `POST /api/nir/{id}/valideaza` | Cititor | 403 + „modifica” | 403 application/json | {"Erori":["Nu aveți dreptul de a modifica „NIR”."]} | 53 | **PASS** |
| 9 | `POST /api/nir/{id}/valideaza` | User | 404 + „nu există sau nu e vizibil” | 404 application/json | {"Erori":["Înregistrarea nu există sau nu e vizibilă pentru utilizatorul curent."]} | 47 | **PASS** |
| 10 | `POST /api/fct/{id}/opereaza` | Admin | 404 + „nu există sau nu e vizibil” (76-r4) | 404 application/json | {"Erori":["Înregistrarea nu există sau nu e vizibilă pentru utilizatorul curent."]} | 108 | **PASS** |
| 11 | `POST /api/nir/{id}/opereaza` | Admin | 404 + „nu există sau nu e vizibil” | 404 application/json | {"Erori":["Înregistrarea nu există sau nu e vizibilă pentru utilizatorul curent."]} | 43 | **PASS** |
| 12 | `GET /api/nir/{id}` | Admin | 200 | 200 application/json | {"Id":"{id}","Numar":null,"Data":"2026-09-02","Stare":"Dra… | 17 | **PASS** |
| 13 | `GET /api/nir/{id}` | Cititor | 200 | 200 application/json | {"Id":"{id}","Numar":null,"Data":"2026-09-02","Stare":"Dra… | 17 | **PASS** |
| 14 | `GET /api/nir/{id}` | User | 404 + „nu există sau nu e vizibil” | 404 application/json | {"Erori":["Înregistrarea nu există sau nu e vizibilă pentru utilizatorul curent."]} | 17 | **PASS** |
| 15 | `GET /api/nir?take=5` | User | 200 + „"data":[]” (200 filtrat) | 200 application/json | {"data":[],"totalCount":-1,"groupCount":-1} | 24 | **PASS** |
| 16 | `GET /api/itv/previzualizare?an=2025&luna=12` | Admin | 200 | 200 application/json | {"An":2025,"Luna":12,"Motiv":"InchidereVie","Sold4426":0,"Sold4427":0,"Transfer":0,"DePlat… | 134 | **PASS** |
| 17 | `GET /api/itv/previzualizare?an=2025&luna=12` | Cititor | 200 (Read pe tot, inclusiv registru) | 200 application/json | {"An":2025,"Luna":12,"Motiv":"InchidereVie","Sold4426":0,"Sold4427":0,"Transfer":0,"DePlat… | 49 | **PASS** |
| 18 | `GET /api/itv/previzualizare?an=2025&luna=12` | User | 403 + „citi” | 403 application/json | {"Erori":["Nu aveți dreptul de a citi „InchidereTva”."]} | 15 | **PASS** |
| 19 | `GET /api/itv/{id}` | Cititor | 200 | 200 application/json | {"Id":"{id}","Numar":"ITV-14","Data":"2026-10-31","An":202… | 91 | **PASS** |
| 20 | `GET /api/itv/{id}` | User | 404 + „nu există sau nu e vizibil” | 404 application/json | {"Erori":["Înregistrarea nu există sau nu e vizibilă pentru utilizatorul curent."]} | 95 | **PASS** |
| 21 | `POST /api/itv/genereaza` | Cititor | 403 + „crea” | 403 application/json | {"Erori":["Nu aveți dreptul de a crea „InchidereTva”."]} | 18 | **PASS** |
| 22 | `POST /api/itv/genereaza` | User | 403 + „crea” | 403 application/json | {"Erori":["Nu aveți dreptul de a crea „InchidereTva”."]} | 14 | **PASS** |
| 23 | `GET /api/odata/Partener({id})` | Cititor | 200 | 200 application/json | {"@odata.context":"https://localhost:5001/api/odata/$metadata#Partener/$entity","ID":"019f… | 32 | **PASS** |
| 24 | `GET /api/odata/Partener({id})` | User | 404 + „nu există sau nu e vizibil” (EroriDto, nu text/plain) | 404 application/json | {"Erori":["Înregistrarea nu există sau nu e vizibilă pentru utilizatorul curent."]} | 36 | **PASS** |
| 25 | `POST /api/odata/Partener` | Cititor | 403 + „crea” (F22-D3: dreptul înaintea domeniului) | 403 application/json | {"Erori":["Nu aveți dreptul de a crea „Partener”."]} | 103 | **PASS** |
| 26 | `POST /api/odata/Partener` | User | 403 + „crea” | 403 application/json | {"Erori":["Nu aveți dreptul de a crea „Partener”."]} | 18 | **PASS** |
| 27 | `POST /api/odata/Partener` | Admin | 422 + „obligatoriu” (domeniul rămâne) | 422 application/json | {"Erori":["„Cod” este obligatoriu pe Partener (nou).","„Denumire” este obligatorie pe Part… | 14 | **PASS** |
| 28 | `PATCH /api/odata/Partener({id})` | Cititor | 403 + „modifica” | 403 application/json | {"Erori":["Nu aveți dreptul de a modifica „Partener”."]} | 22 | **PASS** |
| 29 | `PATCH /api/odata/Partener({id})` | User | 404 + „nu există sau nu e vizibil” | 404 application/json | {"Erori":["Înregistrarea nu există sau nu e vizibilă pentru utilizatorul curent."]} | 37 | **PASS** |
| 30 | `DELETE /api/odata/Partener({id})` | Cititor | 403 + „șterge” | 403 application/json | {"Erori":["Nu aveți dreptul de a șterge „Partener”."]} | 15 | **PASS** |
| 31 | `DELETE /api/odata/Partener({id})` | User | 404 + „nu există sau nu e vizibil” | 404 application/json | {"Erori":["Înregistrarea nu există sau nu e vizibilă pentru utilizatorul curent."]} | 15 | **PASS** |
| 32 | `GET /api/proiectii/saft/xml?an=2025&luna=12` | User | 403 + „citi” | 403 application/json | {"Erori":["Nu aveți dreptul de a citi „RegistruContabil”."]} | 15 | **PASS** |
| 33 | `GET /api/proiectii/saft/xml?an=2025&luna=12` | Cititor | 200 sau 422 (raportat, nu impus) | 422 application/json | {"Erori":["Declarația D406 n-are codul fiscal al societății raportoare — fișierul nu se po… | 3872 | **PASS** |
| 34 | `GET /api/ntc/{id}` | Admin | 404 + „nu există sau nu e vizibil” (M1) | 404 application/json | {"Erori":["Înregistrarea nu există sau nu e vizibilă pentru utilizatorul curent."]} | 16 | **PASS** |
| 35 | `PUT /api/ntc/{id}` | Admin | 404 + „nu există sau nu e vizibil” (M1: NU 422) | 404 application/json | {"Erori":["Înregistrarea nu există sau nu e vizibilă pentru utilizatorul curent."]} | 52 | **PASS** |
| 36 | `PUT /api/ntc/{id}` | Cititor | 404 + „nu există sau nu e vizibil” (M1: NU 403) | 404 application/json | {"Erori":["Înregistrarea nu există sau nu e vizibilă pentru utilizatorul curent."]} | 53 | **PASS** |
| 37 | `POST /api/ntc/{id}/valideaza` | Admin | 404 + „nu există sau nu e vizibil” (M1: NU 200) | 404 application/json | {"Erori":["Înregistrarea nu există sau nu e vizibilă pentru utilizatorul curent."]} | 55 | **PASS** |
| 38 | `POST /api/imperecheri` | Cititor | 403 + „crea” | 403 application/json | {"Erori":["Nu aveți dreptul de a crea „Imperechere”."]} | 14 | **PASS** |
| 39 | `POST /api/imperecheri` | User | 403 + „crea” | 403 application/json | {"Erori":["Nu aveți dreptul de a crea „Imperechere”."]} | 13 | **PASS** |
| 40 | `DELETE /api/imperecheri/{id}` | Admin | 404 + „nu există sau nu e vizibil” | 404 application/json | {"Erori":["Înregistrarea nu există sau nu e vizibilă pentru utilizatorul curent."]} | 149 | **PASS** |
| 41 | `GET /api/nir/{draft}` fără token | (anonim) | 401 | 401 — |  | 7 | **PASS** |
| 42 | `DELETE /api/nir/{id}` | Admin | 204 (curățenie) | 204 — |  | 124 | **PASS** |

Total: 42 probe, 42 PASS, 0 FAIL.

### Smoke în browser (Vite + WebApi pe Privat)

`Cititor`: listele se încarcă (FCT 19.042, Parteneri 20.119, NIR 17.818);
pe un draft NIR — Salvează/Verifică/Operează ⇒ 403 „Nu aveți dreptul de a
modifica „NIR”.", Șterge ⇒ „…de a șterge „NIR”." (Delete distinct de Write
SE VEDE pe ecran), NIR nou ⇒ „…de a crea „NIR”."; Partener: PATCH ⇒ „…de a
modifica „Partener”.", partener nou ⇒ „…de a crea…" (nimic scris în bază);
grila remote pe `fisa-cont` cu cont inexistent ⇒ 404 afișat ca mesajul
serverului, nu JSON brut; SAF-T ca `User` ⇒ „Nu aveți dreptul de a citi
„RegistruContabil”." (F22-D5 pe ecran). Consolă fără erori neprinse, zero
5xx (două 503 raportate de extensie, infirmate cu curl — capcana cunoscută).

**Defect găsit (client, de fond)**: `User` pe `/nir/{id}` al unui document
existent primea de la server 404 `EroriDto` corect, dar ecranul randa un
formular NOU, gol, „nesalvat" — interogarea de citire n-avea ramură de
eroare pe niciunul din cele 16 ecrane de detaliu (`NirDetaliu.tsx:60`;
doar `ItvLista`/`BalantaPlan`/`D300`/`D394`/`Saft` randau `error`). Fix
(pas 4b): mecanica e UNA, în `DocumentShell` (`citire={citit}` ⇒ pe eroare
`PanouErori` cu mesajul serverului și FĂRĂ formular), nu 16 copii.
