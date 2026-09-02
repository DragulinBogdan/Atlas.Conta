# 80. Refuzurile de acces pe toate ușile: 404 / 403 / 422 — o singură ordine, un singur corp

- **Data**: 2026-09-02
- **Stare**: activă (închide 77-r8 și familia ei: 70-r1, 72-r10, 76-r4, 76-r5, 77k, 79-r6; amendează 79b)
- **Docs**: `docs/api/p5-felia-refuzuri-acces-contract.md` (F22-D1…D11 + §Închidere cu matricea HTTP), `nou/.../Module/Api/Refuzuri.cs`, `nou/.../Module/Api/Rezolva.cs`, `nou/.../Module/Motor/GardianEditare.cs` (pasul zero), `nou/.../WebApi/API/Conta/ContaApiController.cs` (gate-urile), `nou/.../WebApi/API/Odata/RefuzOdataFilter.cs`, `nou/Atlas.Conta.Client/src/nucleu/http.ts`, `nou/tools/ProbeHttp/refuzuri.ps1`

## Context

Aceeași întrebare — „n-ai voie" — primea alt răspuns pe fiecare ușă, iar
restanțele se adunaseră sub un singur nume (77-r8) fără o regulă care să le
închidă pe toate:

- **REST, comenzi** (`opereaza`/`anuleaza`/…): gate-ul de instanță era pe
  `Document`, nu pe tipul feliei, deci un id de NIR trimis pe
  `api/fct/{id}/opereaza` trecea gate-ul și pica 422 din Apply (76-r4);
  corpurile lui 403/404 erau goale.
- **REST, scriere** (`POST`/`PUT`/`DELETE` prin `*Apply` pe OS secured):
  NICIUN gate. Un utilizator fără drepturi era refuzat de primul FK invizibil
  („nu există în nomenclatorul de repartitori", 422) sau de gardianul de
  domeniu (422) — codul era al regulii, nu al dreptului, iar mesajul afirma
  „nu există" acolo unde codul nu putea ști decât „nu e vizibil" (76-r5).
- **OData**: `DataService.GetObjectByKey` ⇒ `HttpUserFriendlyException(404,
  "Not Found")`, refuzul la commit ⇒ 403, ambele `text/plain` prin
  `UserFriendlyExceptionFilter`; iar gardianul nostru (`Committing`) rula
  ÎNAINTEA verificării de securitate din `SaveChanges`, deci `User` primea 422
  „Codul e obligatoriu" înainte de orice 403 (77k).
- **ITV** (79b): cine avea drept de citire pe `InchidereTva` vedea prin
  previzualizare soldurile de TVA ale societății fără drept pe
  `RegistruContabil` (79-r6).
- **Client**: `http.ts` traducea 403/404 doar pe `/api/odata/`, cu două texte
  inventate în TS (unul mințea: spunea „scriere" și la refuz de citire); pe
  REST operatorul vedea `POST … → 403 Forbidden`.
- **Măsurabilitate**: 403-ul „pur" (vizibil, dar fără drept de scriere) nu
  putea fi probat pe nicio bază — nu exista un rol cu Read fără Write
  (72-r10).

Mecanica de dedesubt, măsurată pe sursele DevExpress 26.1.3 (nu presupusă):
`BaseObjectSpace.CommitChanges` → `OnCommitting` (gardianul) → `DoCommit` →
`SaveChanges` → `SecurityStateManager.GetEntriesToSave` (verificarea
Create/Write/Delete per entitate, `EFCoreSecurityException`, reîmpachetată de
`SecuredEFCoreObjectSpace.DoCommit` ca `UserFriendlyEFCoreSecurityException :
IUserFriendlySecurityException`). `SecurityQueryCompiler` înfășoară ORICE
query, inclusiv `Find`, cu predicatul de securitate ⇒ pe OS secured
`GetObjectByKey` întoarce `null` și pentru invizibil, nedistins de inexistent.
`UserFriendlyExceptionFilter` construiește `ContentResult` (text/plain)
necondiționat: 403 pe `IUserFriendlySecurityException`, 400 pe restul, statusul
propriu pe `HttpUserFriendlyException` — niciun drum spre JSON.

## Decizia

**(a) Trei coduri, trei întrebări, o singură ordine.** 404 = „subiectul
cererii nu e vizibil pentru tine" — inexistent SAU invizibil, DELIBERAT
nedistinse (altfel API-ul e un oracol de existență pentru rândurile pe care
securitatea le ascunde); doar pe rutele cu subiect. 403 = „subiectul e vizibil
(sau întrebarea e pe TIP), dar operația nu ți-e permisă" — Create pe tip,
Write/Delete pe instanță, Read pe tip. 422 = refuz de DOMENIU pe o cerere pe
care AI dreptul s-o faci; **un 422 nu poate ascunde un refuz de permisiune**.
200 filtrat rămâne răspunsul listelor și sumarelor (69g/71g). Ordinea pe
sârmă, pe toate ușile: **401 → 400 (binding) → 404 → 403 → 422** — autorizarea
înaintea domeniului; sintaxa e a pipeline-ului.

**(b) Ușa de scriere REST capătă gate explicit, pe tipul FELIEI.** În fiecare
controller de document: `POST` ⇒ `CreareAutorizata<T>` (`CanCreate` pe tip,
înaintea Apply-ului, deci înaintea oricărei rezolvări de FK); `PUT {id}` ⇒
`ScriereAutorizata<T>(id, Modificare)`; `DELETE {id}` ⇒ `…(id, Stergere)` —
Write și Delete sunt permisiuni DISTINCTE în XAF, deci operația e PARAMETRU al
gate-ului (`Autorizeaza<T>(id, OperatieAcces)`), nu o a doua metodă; comenzile
rămân pe `ComandaAutorizata<T>` cu **`T` = tipul feliei**, nu `Document` — un
id de alt tip e 404 (închide 76-r4). Gate-ul primește și un predicat opțional
`peUsaAsta` pentru ceea ce TPT găsește dar felia exclude: NTC dă `d is not
InchidereTva` ⇒ un id ITV e 404 pe TOATE verbele ușii NTC (review M1;
**amendează 79c** — comenzile NTC pe id ITV nu mai sunt permise, ITV are ușa
lui cu aceleași comenzi). `GET {id}` rămâne fără gate (ușa
securizată filtrează; 404 natural), cu excepția ITV. `ImperecheriController`
intră pe aceeași formă; `Parteneri` (comanda ANAF single + lot) își păstrează
forma, doar corpul se schimbă.

**(c) Gardianul întreabă securitatea ÎNAINTEA domeniului.** `GardianEditare`
capătă pasul ZERO în `OnCommitting`: pentru fiecare obiect din
`ModifiedObjects`, nou ⇒ `CanCreate(tip dezproxat) && CanWrite(obiect)` (plasa
DevExpress cere pe `Added` Create + Write + Read — review M2; la fel
`PoateCrea` = `CanCreate && CanWrite` pe tip), șters ⇒ `CanDelete(obiect)`,
altfel `CanWrite(obiect)` (`IsGrantedExtensions`); primul refuz ⇒
`RefuzAcces : Exception, IUserFriendlySecurityException` (marker gol al
DevExpress; NU derivă din `OperareException`, ca un refuz de drept să nu fie
înghițit de vreun acumulator de erori de domeniu). Abia apoi `Verifica(os)`.
Strategia vine prin injecție în constructor, NULLABLE (DI-ul Microsoft onorează
parametrul opțional — `CallSiteFactory` cade pe `ParameterDefaultValue`; probat
într-un test izolat, nu presupus): fără strategie (ModelCheck, ușa de sistem)
pasul tace — acolo nu există utilizator, deci nici întrebare de drept.
Utilizatorul administrativ trece întreg pasul (`IsAdministratorPermission` ⇒
`RoleType.AllowAllWithoutPermissions`, `PermissionRequestProcessor.cs:671`).
Verificarea DevExpress din `SaveChanges` RĂMÂNE ca plasă (permisiuni pe
membru, ce nu vedem noi); se schimbă doar ordinea față de domeniu. `Verifica`
statică rămâne neatinsă (ModelCheck o cheamă direct). Închide 77k.

**(d) Un singur corp: `EroriDto` pe 403 și 404, pe toate ușile, în română.**
Mesajele au o singură sursă, în Module (`Api/Refuzuri.cs`): `Invizibil`
(„Înregistrarea nu există sau nu e vizibilă pentru utilizatorul curent.") și
`FaraDrept(operație, tip)` („Nu aveți dreptul de a crea/modifica/șterge/citi
„{caption}”." — captionul XAF al clasei, tipul dezproxat, fallback numele
CLR). REST: niciun `Forbid()`/`NotFound()` cu corp gol în felie;
`RefuzDeDomeniu` traduce `IUserFriendlySecurityException` (al nostru și plasa
DevExpress) ⇒ 403 înaintea lui 422; `[ProducesResponseType(typeof(EroriDto),
403/404)]` ⇒ `openapi.json`. OData: `RefuzDomeniuOdataFilter` devine
`RefuzOdataFilter` și traduce și accesul: `HttpUserFriendlyException` ⇒
statusul ei (404 ⇒ `Invizibil`, altfel mesajul ei — testată pe TIP, înaintea
interfeței, altfel ar fi căzut în 422), `IUserFriendlySecurityException` ⇒
403, `IUserFriendlyException` ⇒ 422, constraint ⇒ 422; ordinea filtrelor
(`Order = int.MaxValue`, cel mai din interior) verificată pe surse; filtrul
DevExpress rămâne înregistrat pentru rutele XAF care nu sunt ale noastre.
Închide 70-r1 (restul lui, după 77g).

**(e) Cifrele motorului cer dreptul de citire pe REGISTRUL din care se
însumează.** `GET api/itv/{id}` și `previzualizare` cer, pe lângă gate-ul de
la 79b, `CanRead(RegistruContabil)` pe tip; lipsa ⇒ 403 „citi „Registru
contabil”". `genereaza`/`regenereaza` produc rânduri, nu cifre — rămân pe
`CanCreate`. Regula generală: o rută care întoarce o sumă peste un registru pe
ușa non-secured cere dreptul de citire pe TIPUL acelui registru (aliniat cu
fișierul SAF-T, 73g). Închide 79-r6, amendează 79b.

**(f) FK invizibil pe ușa securizată = 422 cu mesaj ONEST, printr-un singur
helper.** Referința pe care apelantul n-o vede nu e subiectul cererii, deci nu
e 404; e o cerere pe care n-o poate formula corect — refuz de domeniu, dar
fraza spune adevărul: `„{rol} ({id}) nu există sau nu e vizibil(ă) pentru
utilizatorul curent."` (`Rezolva.Cere<T>`/`Optional<T>`). 88 de throw-uri din
`*Apply`/`OperareApi` — patru texte divergente pentru aceeași necunoaștere —
au devenit un singur helper. Închide 76-r5 (cu (b), care pune dreptul înaintea
FK-ului).

**(g) Rolul `Cititori` + utilizatorul `Cititor` (dev-only) fac 403-ul
măsurabil.** `PermissionPolicy = ReadOnlyAllByDefault` — la sursă
(`PermissionsContainer.IsOperationAllowByPolicy`) exact Read + Navigate, fără
Write/Create/Delete pe niciun tip, inclusiv pe cele de mâine (o listă
enumerată ar fi trebuit ținută la zi). Matricea probelor: `Admin` (422 pe
domeniu), `Cititor` (403 pe scriere, 200 pe citire), `User` (404 pe instanțe,
403 pe tip, 200 gol pe liste). Închide 72-r10.

**(h) Clientul arată motivul, nu statusul.** `http.ts`: o singură ramură
pentru 400/403/404/422 cu corp `Erori[]` ⇒ `EroareDomeniu` (cu `status`
informativ — niciun ecran nu ramifică pe el, 43b); ramura `/api/odata/` cu
textele inventate a murit; 403/404 fără `Erori[]` rămân eroare tehnică
(plasa). `dxStore.onAjaxError` rescrie `e.error` cu mesajul serverului
(verificat pe sursa `devextreme-aspnet-data`: `error = e.error` după handler;
fără rescriere grila arăta JSON-ul brut, fiindcă `Erori` e array, nu string).
Limită DECLARATĂ (80-r1): pe conducta `ODataStore` (lookup-uri, `byKey`)
motivul nu ajunge în `errorHandler` — `errorFromResponse` păstrează doar
`statusText` pe ≥ 400; nu se cârpește cu texte inventate.

**(i) Securitatea se MĂSOARĂ pe HTTP, printr-un script repetabil.**
ModelCheck n-are strategie de securitate și nu capătă una; probele lui rămân
verzi + una pe fraza (f). `nou/tools/ProbeHttp/refuzuri.ps1` autentifică cei
trei utilizatori, rulează matricea (REST scriere/comenzi/citire, ITV, OData,
fișier, imperecheri) și tipărește tabelul §Închidere cu PASS/FAIL, cod de
ieșire ≠ 0 la orice FAIL, fără urme (drafturile de probă se șterg;
`genereaza` se probează doar ca `Cititor`/`User` — capcana 79).

**(j) Ce NU intră**: ușa XAF Blazor (78-r1 rămâne), permisiunile pe membru
(plasa DevExpress), 401, listele (200 filtrat), roluri de producție, 400
`[Range]` în engleză (79-r4/70-r5), refuzurile pe `$expand` OData (nemăsurate).

## Review advers și probe

Tiparul în 4 pași + review advers (Fable, read-only, 40 de probe HTTP
proprii): **0 defecte de fond, 2 medii, 5 minore**. M1 — ușa NTC se
contrazicea pe un id ITV (GET 404, PUT/DELETE 422 cu numărul închiderii,
`valideaza` 200; `Cititor` 403): fixat prin `peUsaAsta` (b). M2 — pasul zero
și `PoateCrea` întrebau doar Create pe obiecte noi, plasa DevExpress cere și
Write: fixat (c). Minore: captionul CLR (80-r5), `400 "Incorrect body."`
englezesc prin filtrul OData (80-r6), `distribuie-valoarea` întoarce sume din
prețurile loturilor pe ușa non-secured unui rol cu Write pe ASM fără Read pe
`Lot` (80-r7), proba SAF-T vacuă pe 500 (fixată în script), `status`
neconsumat (conform (h)). Verificate și trecute: fără oracol de existență
(timpi identici invizibil/inexistent; `$filter`/`$count`/`$expand` goale),
niciun `Domeniu(Secured…)` fără gate, pasul zero nu rulează pe ușa de sistem
(`NonSecuredObjectSpaceFactory` cheamă altă interfață), obiect nou+șters =
detached, permisiunile cache-uite per OS, filtrele OData 404 → 403 → 422,
`ApplicationUser`/`PermissionPolicyRole` nu sunt pe OData.

Smoke în browser (`Cititor`/`User`): refuzurile REST și OData apar cu
mesajul serverului în `PanouErori`, Delete distinct de Write, grila remote
arată motivul, SAF-T ca `User` spune „citi „RegistruContabil”"; **defect de
fond în client**: pe un document invizibil (404) cele 16 ecrane de detaliu
randau un formular NOU, gol — interogarea de citire n-avea ramură de eroare;
fixat o singură dată, în `DocumentShell` (`citire={citit}` ⇒ `PanouErori`
cu mesajul serverului, fără formular), pasul 4b.

Probe: `refuzuri.ps1` — **42 de probe, 42 PASS** (după fix-uri; 38 înaintea
celor 4 de M1), rulat de cinci ori fără urme, tabelul în §Închidere al
contractului; ModelCheck după fix-uri: vezi istoricul. Capcană de execuție:
agenții au rescris fișiere CRLF ca LF (comise așa în pașii 1/2/4) și au lăsat
patru fișiere mixte — normalizate în commit-ul de review; repo-ul e mixt per
fișier (`autocrlf=false`), regula e „păstrezi terminatorul fișierului".

## Ce rămâne deschis

- **80-r1** pe conducta `ODataStore` a clientului (lookup-uri, `byKey`)
  refuzul de acces ajunge ca `statusText`, nu ca mesajul serverului — limita
  DevExtreme `errorFromResponse`; expunere mică (rutele de citire), declarată.
- **80-r2** mesajul plasei DevExpress din `SaveChanges` (permisiuni pe
  membru) iese 403 `EroriDto` cu textul ei englezesc (70-r5).
- **80-r3** `ReportController.cs:83` (scaffold DevExpress) păstrează
  `NotFound()` gol — nu e subiect de entitate, în afara lui (b).
- **80-r4** refuzurile pe `$expand` OData (entitate legată invizibilă) —
  nemăsurate.
- **80-r6** `HttpUserFriendlyException(400)` de pe OData (corp non-JSON:
  „Incorrect body.") iese `400 EroriDto` cu textul DevExpress englezesc —
  clientul îl arată ca refuz motivat (familia 70-r5/79-r4).
- **80-r7** rute cu cifre pe ușa non-secured în afara regulii (e):
  `POST api/asm/{id}/distribuie-valoarea` întoarce `SumaConsum`/`SumaProdus`/
  `ReziduuPlimbat` din prețurile loturilor unui rol cu Write pe `Asamblare`
  fără Read pe `Lot`/`RegistruStoc`; `genereaza-descarcare` și mesajele
  dry-run-ului sunt în aceeași familie (79-r6). Nu e sumă peste registru —
  declarată, nu normalizată.
- **80-r5** captionul din mesajele 403 e numele CLR pe WebApi („InchidereTva”,
  nu „Închidere TVA”): host-ul n-are model de aplicație XAF, deci
  `CaptionHelper` întoarce numele complet și `Refuzuri.Caption` cade pe
  fallback; calea (captions din `metadata.json`/xafml) e aditivă.
