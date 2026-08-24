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
    protected IActionResult ComandaAutorizata(Guid documentId, Func<IActionResult> comanda) {
        using (var os = Secured(typeof(Document))) {
            var doc = os.GetObjectByKey<Document>(documentId);
            if (doc == null)
                return NotFound();
            if (securitate is not IRequestSecurityStrategy cerinte
                    || !cerinte.CanWrite(os, doc))
                return Forbid();
        }
        return comanda();
    }

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
        catch (OperareException ex) {
            return StatusCode(StatusCodes.Status422UnprocessableEntity, EroriDto.DinMesaj(ex.Message));
        }
        catch (Exception ex) {
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
                throw;
            return StatusCode(StatusCodes.Status422UnprocessableEntity,
                EroriDto.DinMesaj(Atlas.DXF.EfCore.Database.Exceptions.ConstraintViolationMessages.Format(violare)));
        }
    }
}
