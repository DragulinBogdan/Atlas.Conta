using Atlas.Conta.BackOffice.Module.Api;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Motor;
using Atlas.Conta.BackOffice.Module.Proiectii;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Security;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Data.ResponseModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Atlas.Conta.BackOffice.WebApi.API.Conta;

// Baza controllerelor de felie (spike D1): DOAR transport — cele două fabrici de
// ObjectSpace și traducerea erorilor de domeniu. Nicio regulă de business aici;
// regulile sunt în Module (Apply/motor/gardian), o singură sursă pentru toate
// tierele (42a).
//
// ═══ Cele două uși (42b) ═══
//   `Secured`     — CULEGEREA: OS din `IObjectSpaceFactory` (scoped), trece prin
//                   securitate ȘI prin `GardianEditare` (D4). Aici se scriu
//                   drafturile.
//   `NonSecured`  — COMANDA: OS din `INonSecuredObjectSpaceFactory`, propriu și
//                   aruncat după comandă; motorul își ține acolo tranzacția
//                   (registre + `Stare` = exact ce refuză gardianul din secured).
// Secvență, nu cuib: culegerea se comite întâi, comanda pornește de la ID.
//
// ═══ De ce traducem NOI erorile (D2) ═══
// `OperareException : UserFriendlyException`, iar filtrul DevExpress
// (`UserFriendlyExceptionFilter`) l-ar transforma într-un `ContentResult` text
// brut cu status 400 — inutilizabil pentru un contract tipat. Prinderea de mai
// jos e ÎNAINTEA filtrului (try/catch în acțiune), deci contractul rămâne al
// nostru: **422** + `EroriDto { Erori: [] }`, cu mesajul cumulat spart pe „\n".
// 422 (nu 400) fiindcă cererea e sintactic validă — o refuză DOMENIUL.
//
// ═══ Ordinea de pe sârmă (felia 22, F22-D1) ═══
//   401 → 400 (binding) → **404** → **403** → 422
// Autorizarea vine ÎNAINTEA domeniului, pe toate ușile: un 422 n-are voie să
// ascundă un refuz de permisiune, iar un 404 n-are voie să distingă „nu există"
// de „nu ți-e vizibil" (altfel API-ul e un oracol de existență peste rândurile
// pe care securitatea le ascunde). Sintaxa (400) rămâne a pipeline-ului.
// Cele trei întrebări, și unde se pun:
//   * 404 — „subiectul cererii îmi e vizibil?" ⇒ `Autorizeaza<T>` /
//     `AutorizeazaCitire<T>`, pe rutele cu `{id}`;
//   * 403 — „am voie operația cerută?" ⇒ același gate pe instanță (Write/
//     Delete/Read) sau `PoateCrea`/`PoateCiti` + `RefuzCreare`/`RefuzCitire` pe
//     TIP, unde întrebarea n-are subiect;
//   * 422 — refuz de DOMENIU, pe o cerere pe care AI dreptul s-o faci.
// Corpul e ACELAȘI peste tot: `EroriDto`, cu frazele din `Api/Refuzuri.cs`
// (Module) — o singură sursă pentru controller, gardian și filtrul OData.
// `Forbid()` și `NotFound()` cu corp GOL nu se mai folosesc nicăieri: un corp
// gol îl obligă pe client să inventeze textul (exact defectul închis de F22-D4).
//
// ═══ Validarea XAF pe acest tier: NU rulează (necunoscuta notată în 40b) ═══
// Probă pe surse (26.1.3): `PersistenceValidationController : ViewController`
// (DevExpress.ExpressApp.Validation\PersistenceValidationController.cs:298) se
// abonează la `ObjectSpace.Committing` în `OnActivated` (:410-414), adică o dată
// per VIEW activat. Tierul Web API n-are Frame-uri și n-are View-uri, deci
// ObjectSpace-urile din `IObjectSpaceFactory` NU au abonatul — regulile
// `RuleRequiredField` de pe `Document.Predator/Primitor` și
// `DocumentDetaliu.TipMaterial` (GATE XAF D2) sunt INERTE aici. Corolarul
// empiric, din smoke: singurele refuzuri primite au fost ale NOASTRE
// (rezolvarea din `Aplica` + gardianul de Committing), niciodată mesajele
// regulilor XAF.
// Consecință de proiectare, deja respectată: `NotaTransferApply.Aplica` rezolvă
// fiecare FK prin `GetObjectByKey` și refuză explicit cu mesaj de domeniu — nu
// se bazează pe validarea de culegere. Orice felie nouă trebuie să facă la fel:
// pe acest tier, regulile care contează sunt cele din Module (Apply, gardian,
// motor), nu cele din pipeline-ul UI.
[ApiController]
[Authorize]
// Un singur 400 pe sârmă (F13-D3): `[ApiController]` poate răspunde 400 pe
// ORICE acțiune, fără ca acțiunea s-o declare — eșecul de model binding e al
// pipeline-ului, nu al codului feliei. `InvalidModelStateResponseFactory`
// (`Startup.cs`) îl aduce la forma `EroriDto`, iar declarația de aici o pune în
// `openapi.json` pentru toate acțiunile deodată. Cele per acțiune (proiecțiile,
// care întorc 400 de domeniu prin `BadRequest(EroriDto.Din(...))`) rămân:
// Swashbuckle deduplică pe (status × tip), iar tipul e același.
[ProducesResponseType(typeof(EroriDto), StatusCodes.Status400BadRequest)]
public abstract class ContaApiController : ControllerBase {
    // Plicul răspunsului `DataSourceLoader` — există DOAR ca să apară în
    // `swagger.json`, pentru codegen-ul OpenAPI→TS (D10): acțiunile întorc
    // `object`, iar Swashbuckle n-are ce infera. Numele câmpurilor sunt cele ale
    // bibliotecii (lowercase — `LoadResult`), fiindcă asta consumă `CustomStore`.
    // ATENȚIE: forma asta e cea cu `requireTotalCount`/`group`/`summary`; fără
    // ele, `DataSourceLoader` întoarce direct tabloul de rânduri.
    public sealed class PaginaDto<T> {
        public T[] data { get; set; }
        public int totalCount { get; set; }
        public int groupCount { get; set; }
        public object[] summary { get; set; }
    }

    readonly IObjectSpaceFactory securedFactory;
    readonly INonSecuredObjectSpaceFactory nonSecuredFactory;
    readonly ISecurityStrategyBase securitate;

    protected ContaApiController(IObjectSpaceFactory securedFactory,
        INonSecuredObjectSpaceFactory nonSecuredFactory, ISecurityStrategyBase securitate) {
        this.securedFactory = securedFactory;
        this.nonSecuredFactory = nonSecuredFactory;
        this.securitate = securitate;
    }

    protected IObjectSpace Secured(Type objectType) => securedFactory.CreateObjectSpace(objectType);

    protected IObjectSpace NonSecured(Type objectType) =>
        nonSecuredFactory.CreateNonSecuredObjectSpace(objectType);

    // ═══ Gate-ul de autorizare al COMENZILOR (review advers F1) ═══
    // Ușa non-secured răspunde la „CUM scrie motorul" (42b), nu la „CINE are voie
    // să-l cheme": fără gate, orice utilizator autentificat — inclusiv unul fără
    // niciun drept pe documente — ar opera/storna/anula prin simpla cunoaștere a
    // unui GUID. Autorizarea se decide AICI, prin securitatea XAF, pe documentul
    // rezolvat printr-un ObjectSpace SECURED: inexistent SAU invizibil pentru
    // user → 404 (fără sondare de existență); vizibil dar fără Write → 403;
    // abia apoi comanda primește ușa non-secured.
    //
    // GENERIC de la felia 15 (D15-D4): comanda de sincronizare ANAF e pe
    // `Partener`, nu pe un document, dar întrebarea („există pentru mine? am voie
    // să scriu?") și răspunsurile (404/403) sunt aceleași. Tipul e singurul lucru
    // care se schimbă, deci intră ca parametru de tip — nu ca a doua copie a
    // gate-ului.
    //
    // `T` = tipul FELIEI, nu `Document` (F22-D2, închide 76-r4): supraîncărcarea
    // fără parametru de tip a MURIT. Cât gate-ul întreba pe `Document`, un id de
    // NIR trecea autorizarea pe `api/fct/{id}/opereaza` și pica abia în Apply, cu
    // 422 — adică un refuz de domeniu pentru o rută pe care documentul nici nu e
    // vizibil. Acum `GetObjectByKey<FacturaIntrare>` nu-l vede și răspunsul e
    // 404: „nu e vizibil PE UȘA ASTA".
    protected IActionResult ComandaAutorizata<T>(Guid id, Func<IActionResult> comanda,
            OperatieAcces operatie = OperatieAcces.Modificare, Func<T, bool> peUsaAsta = null) where T : class {
        var refuz = Autorizeaza<T>(id, operatie, peUsaAsta);
        return refuz ?? comanda();
    }

    // Ușa de SCRIERE a agregatului (F22-D2, închide 76-r5): `PUT {id}` cere
    // `Modificare`, `DELETE {id}` cere `Stergere`. Până acum n-avea NICIUN gate:
    // `User` era refuzat de primul FK invizibil rezolvat de Apply („nu există în
    // nomenclatorul…", 422) sau de `GardianEditare` — cu alte cuvinte codul era
    // al regulii, nu al dreptului, iar mesajul spunea „nu există" unde adevărul
    // era „nu e vizibil". Gate-ul se ia ÎNAINTE de Apply, deci înaintea oricărei
    // rezolvări de FK.
    //
    // Același gate ca `ComandaAutorizata`, sub alt nume și nu ca a doua copie:
    // cele două uși ale lui 42b — comanda motorului (non-secured) și scrierea
    // culegerii (secured) — rămân distincte la citire, iar verdictul rămâne
    // scris o singură dată, în `Autorizeaza<T>`.
    protected IActionResult ScriereAutorizata<T>(Guid id, Func<IActionResult> actiune,
            OperatieAcces operatie = OperatieAcces.Modificare, Func<T, bool> peUsaAsta = null) where T : class =>
        ComandaAutorizata<T>(id, actiune, operatie, peUsaAsta);

    // Perechea fără SUBIECT a lui `ComandaAutorizata` (F22-D2): crearea. Nu există
    // instanță de rezolvat — deci nici 404 — iar întrebarea se pune pe TIP.
    // Gate-ul se ia pe un OS SECURED, ÎNAINTE de orice atingere a bazei prin
    // Apply, ca refuzul să nu depindă de ordinea în care Apply-ul rezolvă FK-urile.
    //
    // Create ȘI Write (review 80 M2): plasa DevExpress din `SaveChanges` cere pe
    // un obiect `Added` toate trei — Create, Write, Read
    // (`SecurityStateManager.CheckIsGrantedToSave`, :94-96 + :197-198). Un rol cu
    // Create fără Write ar fi trecut gate-ul și ar fi picat abia în plasă, cu
    // textul ei englezesc (80-r2) — pe o cale NORMALĂ, nu excepțională. Mesajul
    // rămâne „crea": dreptul care lipsește e al creării COMPLETE (`PoateCrea`).
    protected IActionResult CreareAutorizata<T>(Func<IActionResult> comanda) where T : class {
        using (var os = Secured(typeof(T)))
            if (!PoateCrea(typeof(T), os))
                return RefuzCreare(typeof(T));
        return comanda();
    }

    // Gate-ul de INSTANȚĂ, ca funcție: `null` = are voie, altfel refuzul gata
    // format. Separat de `ComandaAutorizata` fiindcă îl cheamă și PUT/DELETE
    // direct, și fiindcă LOTUL (D15-D4) are nevoie de verdict per id — acolo un
    // refuz nu e răspunsul cererii, ci un rând în `Sarite`.
    //
    // OPERAȚIA e parametru, nu a doua metodă (F22-D2): Write și Delete sunt
    // permisiuni DISTINCTE în XAF, iar a le fi întrebat pe amândouă cu `CanWrite`
    // ar fi însemnat că cine poate modifica poate și șterge. `Citire` intră pe
    // aceeași ușă (vezi `AutorizeazaCitire<T>`); `Creare` n-are instanță, deci
    // n-are ce căuta aici — o cerere de genul ăsta e un bug de apelant, nu un
    // refuz de utilizator.
    //
    // `peUsaAsta` (review 80 M1): sub TPT, `GetObjectByKey<T>` găsește și
    // derivatele lui `T` — un id de `InchidereTva` e „vizibil" ca `NotaContabila`,
    // deși felia NTC îl EXCLUDE la citire (79c). Fără predicat, aceeași ușă ar
    // spune 404 pe GET și 403/422 pe PUT, adică două adevăruri. Predicatul e
    // al feliei (ea știe ce servește), iar un obiect care nu-l trece e, pentru
    // ușa asta, invizibil ⇒ 404, aceeași frază.
    protected IActionResult Autorizeaza<T>(Guid id,
            OperatieAcces operatie = OperatieAcces.Modificare, Func<T, bool> peUsaAsta = null) where T : class {
        if (operatie == OperatieAcces.Creare)
            throw new ArgumentException(
                "Crearea n-are subiect — se întreabă pe TIP (`CreareAutorizata`/`PoateCrea`).",
                nameof(operatie));

        using var os = Secured(typeof(T));
        var obiect = os.GetObjectByKey<T>(id);
        // Inexistent SAU invizibil SAU în afara ușii, aceeași frază: 404 nu
        // distinge, deliberat.
        if (obiect == null || (peUsaAsta != null && !peUsaAsta(obiect)))
            return Invizibil();
        // Cast explicit la `object`: supraîncărcările care contează sunt cele pe
        // INSTANȚĂ (`CanWrite(IObjectSpace, object targetObject)` etc.). Fără
        // cast, un `T` care s-ar nimeri `Type` ar aluneca pe supraîncărcarea de
        // TIP (`CanWrite(Type, IObjectSpace, …)`), adică altă întrebare.
        var tinta = (object)obiect;
        var permis = securitate is IRequestSecurityStrategy cerinte && operatie switch {
            OperatieAcces.Stergere => cerinte.CanDelete(os, tinta),
            OperatieAcces.Citire => cerinte.CanRead(os, tinta),
            _ => cerinte.CanWrite(os, tinta)
        };
        return permis ? null : RefuzAccesRezultat(operatie, typeof(T));
    }

    // ═══ Gate-ul de CITIRE pe INSTANȚĂ (felia 21, F21-D3) ═══
    // Aceeași ușă ca `Autorizeaza<T>`, cu `CanRead`: inexistent SAU invizibil ⇒
    // 404 (fără sondare de existență), vizibil dar fără drept de citire ⇒ 403,
    // `null` = are voie.
    //
    // DE CE există, când o citire obișnuită n-are nevoie de el: `GET api/itv/{id}`
    // răspunde cu cifre ale MOTORULUI (soldurile 4426/4427 la data închiderii +
    // verdictul anti-stale), calculate pe ușa NON-SECURED. Pe ușa securizată
    // soldurile s-ar fi însumat peste rândurile de registru pe care permisiunile
    // le ascund — adică o cifră FALSĂ prezentată ca adevărul motorului (argumentul
    // 73g pentru fișierul SAF-T: filtrarea tăcută produce un răspuns plauzibil și
    // greșit, nu unul gol). Deci verdictul de acces se ia AICI, pe instanță,
    // înainte ca ușa non-secured să se deschidă.
    //
    // Listele nu-l cer și nu-l primesc: acolo ușa securizată filtrează rândurile,
    // iar o listă goală pentru cine n-are drepturi e un răspuns adevărat (69g/71g).
    protected IActionResult AutorizeazaCitire<T>(Guid id) where T : class =>
        Autorizeaza<T>(id, OperatieAcces.Citire);

    // Gate-ul de LOT (D15-D4): fiecare id trece SEPARAT, iar cel refuzat iese cu
    // motiv, nu aruncă tot lotul. Un 403 pe 500 de parteneri fiindcă unul singur
    // e invizibil ar fi „totul sau nimic" acolo unde utilizatorul a cerut „ce se
    // poate" — și ar ascunde exact ce s-ar fi putut face.
    //
    // `eticheta` se citește CÂT E OS-UL VIU: după `using` obiectul e detașat, iar
    // mesajul din UI n-are ce face cu un GUID.
    protected (List<Guid> Permise, List<IdRefuzat> Refuzate) AutorizeazaLot<T>(
            IEnumerable<Guid> ids, Func<T, string> eticheta) where T : class {
        var permise = new List<Guid>();
        var refuzate = new List<IdRefuzat>();
        using var os = Secured(typeof(T));
        var cerinte = securitate as IRequestSecurityStrategy;
        foreach (var id in (ids ?? []).Distinct()) {
            var obiect = os.GetObjectByKey<T>(id);
            // Inexistent SAU invizibil pentru user — aceeași frază, deliberat:
            // altfel lotul ar fi un oracol de existență pentru rândurile pe care
            // securitatea le ascunde (motivul lui 404 din gate-ul simplu).
            // Frazele sunt cele din `Refuzuri` (F22-D4): un lot n-are voie să
            // spună altfel decât ruta cu un singur id ce spune același refuz.
            if (obiect == null)
                refuzate.Add(new IdRefuzat(id, null, Refuzuri.Invizibil));
            else if (cerinte == null || !cerinte.CanWrite(os, (object)obiect))
                refuzate.Add(new IdRefuzat(id, eticheta?.Invoke(obiect),
                    Refuzuri.FaraDrept(OperatieAcces.Modificare, typeof(T))));
            else
                permise.Add(id);
        }
        return (permise, refuzate);
    }

    protected sealed record IdRefuzat(Guid Id, string Eticheta, string Motiv);

    // ═══ Gate-ul de CITIRE, la nivel de TIP (felia 16, D16-D5) ═══
    // Proiecțiile de registru n-au nevoie de el: ușa securizată FILTREAZĂ rândurile,
    // deci cine n-are drept vede o listă goală și primește 200 (69g/71g) — o listă
    // goală e un răspuns adevărat.
    //
    // Fișierul D406 nu e o listă: e o DECLARAȚIE semnată cu CUI-ul societății.
    // Filtrarea tăcută ar produce acolo un fișier valid sintactic, cu antetul
    // real și zero tranzacții — adică o declarație FALSĂ, nu un răspuns gol. De
    // aceea generarea lui cere dreptul de citire pe TIPUL registrului, verificat
    // înainte de a scrie primul octet, iar lipsa lui e 403.
    //
    // `CanRead(Type, IObjectSpace)` (`IsGrantedExtensions`, DevExpress 26.1.3) =
    // `PermissionRequest(os, tip, SecurityOperations.Read)` — permisiunea pe TIP,
    // nu pe instanță: exact întrebarea „are voie omul ăsta să citească registrul?".
    // Gate-ul de comandă (`Autorizeaza<T>`) rămâne NESCHIMBAT: acolo întrebarea e
    // pe o instanță și are alt răspuns (404 pentru invizibil).
    protected bool PoateCiti(Type tip, IObjectSpace os) =>
        securitate is IRequestSecurityStrategy cerinte && cerinte.CanRead(tip, os);

    // ═══ Gate-ul de CREARE, la nivel de TIP (felia 21, F21-D3) ═══
    // Comanda de GENERARE n-are subiect: ea PRODUCE documentul, deci nu există
    // instanță pe care `ComandaAutorizata<T>(id)` s-o rezolve (76-r4 numește
    // aceeași asimetrie). Întrebarea corectă e „are voie omul ăsta să creeze un
    // document de tipul ăsta?", iar securitatea XAF o răspunde direct:
    // `CanCreate(Type, IObjectSpace)` = `PermissionRequest(os, tip,
    // SecurityOperations.Create)` (`IsGrantedExtensions`, DevExpress 26.1.3).
    //
    // Fără el, ușa non-secured pe care generatorul își scrie draftul (58c: scrie
    // câmpuri server-owned, deci nu poate rula securizat) ar fi însemnat că orice
    // utilizator autentificat — inclusiv unul fără niciun drept pe documente —
    // creează închideri de TVA. Gate-ul se ia ÎNAINTE, pe un OS securizat, ca la
    // toate comenzile (55b).
    protected bool PoateCrea(Type tip, IObjectSpace os) =>
        securitate is IRequestSecurityStrategy cerinte
            && cerinte.CanCreate(tip, os) && cerinte.CanWrite(tip, os);

    // ═══ 404-ul, cu MOTIV (F22-D4) ═══
    // `NotFound()` gol obligă clientul să inventeze textul — exact ce făcea
    // `nucleu/http.ts` pe rutele OData, cu fraze scrise în TS care nu aveau de
    // unde ști ce s-a întâmplat. Fraza e una singură, în Module, și NU distinge
    // „nu există" de „nu-ți e vizibil": vezi `Refuzuri.Invizibil`.
    //
    // O folosesc și rutele de citire fără gate (`GET {id}` unde `Citeste`
    // întoarce `null` pentru un rând filtrat de securitate): răspunsul lor e
    // deja 404 din același motiv, deci merită același corp.
    protected IActionResult Invizibil() => NotFound(EroriDto.DinMesaj(Refuzuri.Invizibil));

    // ═══ Refuzurile pe TIP, ca rezultat (F22-D4) ═══
    // `PoateCiti`/`PoateCrea` întorc `bool` fiindcă unele rute au de făcut ceva
    // ÎNTRE verdict și răspuns (SAF-T calculează un sumar filtrat în loc de
    // fișier). Când verdictul ESTE răspunsul, forma lui e una singură: 403 cu
    // `EroriDto` și fraza din Module — niciodată `Forbid()`, care produce un corp
    // GOL (și, pe scheme de autentificare cu challenge, chiar alt status).
    protected IActionResult RefuzCitire(Type tip) =>
        RefuzAccesRezultat(OperatieAcces.Citire, tip);

    protected IActionResult RefuzCreare(Type tip) =>
        RefuzAccesRezultat(OperatieAcces.Creare, tip);

    IActionResult RefuzAccesRezultat(OperatieAcces operatie, Type tip) =>
        StatusCode(StatusCodes.Status403Forbidden,
            EroriDto.DinMesaj(Refuzuri.FaraDrept(operatie, tip)));

    // Încărcarea unei proiecții prin `DataSourceLoader`, cu MATERIALIZARE
    // explicită înainte de întoarcere.
    //
    // De ce nu se poate `return DataSourceLoader.Load(query, loadOptions)` direct
    // (probă live, nu teorie): rezultatul poartă `data` ca `IEnumerable`
    // DEFERRED — chiar `IQueryable`-ul nostru. Serializarea JSON se întâmplă însă
    // DUPĂ ce acțiunea s-a întors, deci după `using`-ul ObjectSpace-ului: prima
    // încercare a dat `ObjectDisposedException: Cannot access a disposed context
    // instance` din `SecurityQueryCompiler`, în mijlocul serializării. În
    // șabloanele DevExtreme problema nu apare fiindcă `DbContext`-ul e injectat
    // scoped (trăiește până la finalul cererii); aici ObjectSpace-ul e AL NOSTRU
    // și trebuie eliberat deterministic — deci enumerăm cât e viu.
    //
    // `ordineImplicita` (opțional): ordinea proiecției, în forma pe care
    // `DataSourceLoader` o consumă. NU e un lux de stil — biblioteca își inventează
    // singură o ordine (`Id`-ul, adică ordinea de INSERARE) când cererea n-are
    // `sort=`, iar în EF Core `OrderBy`-ul ei ȘTERGE ordinea scrisă în proiecție.
    // Demonstrația mecanică, cu sursele citate: `Proiectii/OrdineLista.cs`.
    // Proiecțiile a căror ordine e semnificativă (fișa de cont, jurnalul) o
    // declară aici; restul listelor n-au ordine documentată, deci n-o pasează.
    protected static object Incarca<T>(IQueryable<T> sursa, DataSourceLoadOptions loadOptions,
        params SortingInfo[] ordineImplicita) {
        // Plafon pe pagină (review advers, minor 1): fără `take`, DataSourceLoader
        // ar materializa TOATE rândurile (pe clona de import: sute de mii).
        const int TakeImplicit = 100, TakeMaxim = 500;
        if (loadOptions.Take <= 0)
            loadOptions.Take = TakeImplicit;
        else if (loadOptions.Take > TakeMaxim)
            loadOptions.Take = TakeMaxim;
        OrdineLista.AplicaOrdineImplicita(loadOptions, ordineImplicita);
        var rezultat = DataSourceLoader.Load(sursa, loadOptions);
        // Cu `requireTotalCount`/`group`/`totalSummary` întoarce `LoadResult`;
        // altfel, direct secvența. Grupurile („Group.items") sunt construite deja
        // în memorie de bibliotecă — se materializează nivelul deferred, cel de sus.
        if (rezultat is LoadResult incarcare) {
            incarcare.data = Materializeaza(incarcare.data);
            return incarcare;
        }
        return Materializeaza(rezultat as System.Collections.IEnumerable);
    }

    static System.Collections.IEnumerable Materializeaza(System.Collections.IEnumerable sursa) =>
        sursa == null ? Array.Empty<object>() : sursa.Cast<object>().ToList();

    // Rândurile paginii deja materializate de `Incarca` — pentru completările care
    // NU pot veni din SQL și se fac în memorie, peste pagină (azi: codul de tip al
    // documentului, R-D8/60b). Instanțele sunt cele care se serializează, deci
    // mutarea lor aici se vede în răspuns.
    //
    // Limitare asumată, nu ascunsă: în modul GRUPAT, `LoadResult.data` conține
    // obiecte `Group`, nu rânduri, deci `OfType<T>` întoarce gol și completarea nu
    // se aplică. Fișa forțează `Group = null` (R-D6), iar jurnalul grupat rămâne
    // fără codul de tip (link-ul se pierde, cifrele nu) — coborârea recursivă în
    // grupuri se adaugă dacă apare cerința.
    protected static IEnumerable<T> Randuri<T>(object incarcare) {
        var sursa = incarcare is LoadResult rezultat
            ? rezultat.data
            : incarcare as System.Collections.IEnumerable;
        return sursa == null ? Enumerable.Empty<T>() : sursa.Cast<object>().OfType<T>();
    }

    // Învelișul unic al acțiunilor: orice `OperareException` (pre-check de felie,
    // gardian de Committing, gardian de motor) devine 422 cu erorile ca listă.
    protected IActionResult Domeniu(Func<IActionResult> actiune) {
        try {
            return actiune();
        }
        catch (Exception ex) {
            // `throw;` rămâne AICI, în catch, ca stiva să nu se piardă: helper-ul
            // doar traduce, iar `null` înseamnă „nu e o eroare de domeniu".
            var refuz = RefuzDeDomeniu(ex);
            if (refuz == null)
                throw;
            return refuz;
        }
    }

    // Aceeași traducere pe calea ASYNC (felia 15: comanda ANAF face I/O de
    // rețea, deci acțiunea e `async Task<IActionResult>`). O a doua copie a
    // catch-urilor ar fi fost exact locul unde contractul de 422 ar începe să
    // difere între felii — de-aia traducerea e una singură, mai jos.
    protected async Task<IActionResult> Domeniu(Func<Task<IActionResult>> actiune) {
        try {
            return await actiune();
        }
        catch (Exception ex) {
            var refuz = RefuzDeDomeniu(ex);
            if (refuz == null)
                throw;
            return refuz;
        }
    }

    // `null` = nu e nici refuz de acces, nici eroare de domeniu (apelantul o lasă
    // să treacă mai departe, cu `throw;` din catch-ul lui).
    //
    // Ordinea e cea de pe sârmă (F22-D1): DREPTUL înaintea domeniului. Cele două
    // ramuri de 403 sunt PLASA — pe calea normală refuzul de acces s-a dat deja
    // în gate, înainte ca Apply-ul să atingă baza. Ele prind ce trece pe lângă:
    //   * `RefuzAcces` — pasul zero al lui `GardianEditare` (F22-D3), ridicat la
    //     `Committing`, adică din interiorul lui `CommitChanges`, deci în plin
    //     `Domeniu`;
    //   * orice alt `IUserFriendlySecurityException` — `UserFriendlyEFCoreSecurityException`,
    //     pe care `SecuredEFCoreObjectSpace.DoCommit` o împachetează din
    //     verificarea de permisiuni a lui `SaveChanges` (inclusiv permisiunile pe
    //     MEMBRU, pe care noi nu le întrebăm — F22-D11). Mesajul ei e al
    //     DevExpress și e în engleză (70-r5), asumat: pe calea normală nu se mai
    //     ajunge aici.
    // De ce nu se pot amesteca cu 422: `RefuzAcces` NU derivă din
    // `OperareException` (e `Exception, IUserFriendlySecurityException`, iar
    // `OperareException : UserFriendlyException`), tocmai ca un refuz de drept să
    // nu poată fi înghițit de un acumulator de erori de domeniu.
    IActionResult RefuzDeDomeniu(Exception ex) {
        if (ex is IUserFriendlySecurityException)
            return StatusCode(StatusCodes.Status403Forbidden, EroriDto.DinMesaj(ex.Message));
        if (ex is OperareException)
            return StatusCode(StatusCodes.Status422UnprocessableEntity, EroriDto.DinMesaj(ex.Message));
        // Violările de constraint DB (F4-M2): pe rutele fără pre-check propriu
        // (ex. DELETE pe un draft FCL ale cărui linii sunt referite de un DSC
        // manual prin `LinieSursaId` — FK Restrict) commit-ul iese ca
        // DbUpdateException, care fără traducere ajungea 500 brut. Aceeași
        // traducere ca în Blazor (39a, `AtlasDxfExceptionService`), același
        // contract ca orice refuz de domeniu: 422 + EroriDto. Template-urile
        // RO se aplică la pornire (`MesajeConstraintRo.Aplica`); modelul EF se
        // deduce din `Entries`-urile excepției — captions XAF cu fallback CLR.
        var violare = Atlas.DXF.EfCore.Database.Exceptions.ConstraintViolationTranslator.TryTranslate(ex);
        if (violare == null)
            return null;
        return StatusCode(StatusCodes.Status422UnprocessableEntity,
            EroriDto.DinMesaj(Atlas.DXF.EfCore.Database.Exceptions.ConstraintViolationMessages.Format(violare)));
    }
}
