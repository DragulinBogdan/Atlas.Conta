using Atlas.Conta.BackOffice.Module.Api;
using Atlas.Conta.BackOffice.Module.Api.Perioade;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Atlas.Conta.BackOffice.WebApi.API.Conta;

// Lanțul perioadelor și comenzile lui (F27-D1/D2). Subiectul rutelor nu e un
// `{id}`, ci luna: id-ul se rezolvă pe ușa SECURIZATĂ, ca 404-ul să însemne tot
// „nu există SAU nu-ți e vizibil", iar comanda să treacă prin același gate de
// instanță ca oriunde (F22-D1). Verdictul și comanda rulează pe ușa
// non-secured: `PerioadaService` citește lanțul întreg, iar un lanț filtrat ar
// da un răspuns FALS, nu unul gol (73g/80e).
[Route("api/perioade")]
public class PerioadeController : ContaApiController {
    const int AnMinim = 2000, AnMaxim = 2100;

    readonly ISecurityStrategyBase securitate;

    public PerioadeController(IObjectSpaceFactory secured, INonSecuredObjectSpaceFactory nonSecured,
            ISecurityStrategyBase securitate) : base(secured, nonSecured, securitate) =>
        this.securitate = securitate;

    [HttpGet]
    [ProducesResponseType(typeof(PerioadaDto[]), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status403Forbidden)]
    public IActionResult Get() {
        using var os = Secured(typeof(PerioadaFiscala));
        return PoateCiti(typeof(PerioadaFiscala), os)
            ? Ok(PerioadeApply.Lant(os))
            : RefuzCitire(typeof(PerioadaFiscala));
    }

    [HttpGet("{an:int}/{luna:int}/verificare")]
    [ProducesResponseType(typeof(ConstatareInchidereDto[]), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status404NotFound)]
    public IActionResult Verificare(int an, int luna) {
        var erori = Perioada(an, luna);
        if (erori.Count > 0)
            return BadRequest(EroriDto.Din(erori));
        var refuz = Subiect(an, luna, OperatieAcces.Citire);
        if (refuz != null)
            return refuz;
        // Verdictul ÎNSUMEAZĂ pe ușa non-secured documente, închideri de TVA,
        // amortizări, împerecheri și politica severităților: 80e cere dreptul de
        // citire pe tot ce însumează, altfel „nicio constatare" ar fi un răspuns
        // plauzibil și FALS pentru cine nu vede rândurile.
        refuz = CititeIntegral();
        if (refuz != null)
            return refuz;
        return Domeniu(() => {
            using var os = NonSecured(typeof(PerioadaFiscala));
            return Ok(PerioadeApply.Verifica(os, an, luna));
        });
    }

    /// <summary>Istoricul comenzilor unei luni: închideri, redeschideri, motive, acceptări.</summary>
    [HttpGet("{an:int}/{luna:int}/istoric")]
    [ProducesResponseType(typeof(IstoricPerioadaDto[]), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status404NotFound)]
    public IActionResult Istoric(int an, int luna) {
        var erori = Perioada(an, luna);
        if (erori.Count > 0)
            return BadRequest(EroriDto.Din(erori));
        var refuz = Subiect(an, luna, OperatieAcces.Citire);
        if (refuz != null)
            return refuz;
        using (var os = Secured(typeof(InchiderePerioada)))
            if (!PoateCiti(typeof(InchiderePerioada), os))
                return RefuzCitire(typeof(InchiderePerioada));
        return Domeniu(() => {
            using var os = NonSecured(typeof(PerioadaFiscala));
            return Ok(PerioadeApply.Istoric(os, an, luna));
        });
    }

    // Corpul e OPȚIONAL: a închide fără constatări de acceptat e cazul normal,
    // iar un 400 pe corp lipsă ar fi un refuz de sintaxă pe o cerere completă.
    [HttpPost("{an:int}/{luna:int}/inchide")]
    [ProducesResponseType(typeof(InchiderePerioadaRezultatDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult Inchide(int an, int luna,
            [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] InchidePerioadaRequestDto cerere) =>
        Comanda(an, luna, os => PerioadeApply.Inchide(os, an, luna, cerere, UserId(), securitate?.UserName));

    [HttpPost("{an:int}/{luna:int}/redeschide")]
    [ProducesResponseType(typeof(InchiderePerioadaRezultatDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult Redeschide(int an, int luna, [FromBody] RedeschidePerioadaRequestDto cerere) =>
        Comanda(an, luna, os => PerioadeApply.Redeschide(os, an, luna, cerere, UserId(), securitate?.UserName));

    // Reconstrucția n-are lună ca subiect: recalculează integral TOATE perioadele
    // de referință și raportează diferențele înainte de rescriere (F27-D3, 35b).
    // Gate-ul e pe TIP, `Write` pe `PerioadaFiscala` — același drept ca
    // `inchide`, fiindcă pe tierul REST nu există o noțiune de „Administrator"
    // separată de permisiunile XAF.
    [HttpPost("reconstruieste")]
    [ProducesResponseType(typeof(ReconstructieRezultatDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult Reconstruieste() {
        using (var os = Secured(typeof(PerioadaFiscala)))
            if (!PoateScrie(typeof(PerioadaFiscala), os))
                return RefuzScriere(typeof(PerioadaFiscala));
        return Domeniu(() => {
            using var os = NonSecured(typeof(PerioadaFiscala));
            return Ok(PerioadeApply.Reconstruieste(os));
        });
    }

    // Tipurile pe care verificarea le ÎNSUMEAZĂ pe ușa non-secured (80e).
    static readonly Type[] TipuriInsumate = [
        typeof(Document), typeof(InchidereTva), typeof(AmortizareLunara),
        typeof(Imperechere), typeof(PoliticaInchidere),
    ];

    IActionResult CititeIntegral() {
        using var os = Secured(typeof(Document));
        foreach (var tip in TipuriInsumate)
            if (!PoateCiti(tip, os))
                return RefuzCitire(tip);
        return null;
    }

    IActionResult Comanda(int an, int luna, Func<IObjectSpace, InchiderePerioadaRezultatDto> comanda) {
        var erori = Perioada(an, luna);
        if (erori.Count > 0)
            return BadRequest(EroriDto.Din(erori));
        var refuz = Subiect(an, luna, OperatieAcces.Modificare);
        if (refuz != null)
            return refuz;
        return Domeniu(() => {
            using var os = NonSecured(typeof(PerioadaFiscala));
            return Ok(comanda(os));
        });
    }

    // 404 pe luna nedefinită SAU invizibilă, apoi 403 pe dreptul cerut: ordinea
    // lui F22-D1 pe o rută al cărei subiect e luna, nu un `{id}`.
    IActionResult Subiect(int an, int luna, OperatieAcces operatie) {
        Guid id;
        using (var os = Secured(typeof(PerioadaFiscala))) {
            var gasit = os.GetObjectsQuery<PerioadaFiscala>()
                .Where(p => p.An == an && p.Luna == luna)
                .Select(p => (Guid?)p.ID)
                .FirstOrDefault();
            if (gasit == null)
                return Invizibil();
            id = gasit.Value;
        }
        return Autorizeaza<PerioadaFiscala>(id, operatie);
    }

    // `ISecurityStrategyBase.UserId` e `object` (cheia utilizatorului poate fi de orice tip).
    Guid? UserId() => securitate?.UserId as Guid?;

    static List<string> Perioada(int an, int luna) {
        var erori = new List<string>();
        if (an < AnMinim || an > AnMaxim)
            erori.Add($"„an” trebuie să fie între {AnMinim} și {AnMaxim}.");
        if (luna < 1 || luna > 12)
            erori.Add("„luna” trebuie să fie între 1 și 12 — perioada fiscală e o lună.");
        return erori;
    }
}
