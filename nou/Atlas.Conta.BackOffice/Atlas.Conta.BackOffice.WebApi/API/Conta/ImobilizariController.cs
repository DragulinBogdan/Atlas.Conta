using Atlas.Conta.BackOffice.Module.Api;
using Atlas.Conta.BackOffice.Module.Api.Imo;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using DevExpress.ExpressApp;
using Microsoft.AspNetCore.Mvc;

namespace Atlas.Conta.BackOffice.WebApi.API.Conta;

// Proiecțiile fișei de imobilizare (F26-D10). Transport pur: regulile sunt în `ImobilizariApply`.
// Nomenclatorul `Imobilizare` n-are ușă REST — CRUD-ul lui e pe `api/odata/Imobilizare` (F2-D4),
// iar regulile de editare ale fișei sunt ale gardianului, pe ușa comună.
[Route("api/imobilizari")]
public class ImobilizariController : ContaApiController {
    public ImobilizariController(IObjectSpaceFactory secured, INonSecuredObjectSpaceFactory nonSecured,
        DevExpress.ExpressApp.Security.ISecurityStrategyBase securitate)
        : base(secured, nonSecured, securitate) { }

    // F22-D5: cifrele se fac non-secured, dar cer și dreptul pe registru — după 404-ul instanței (80a).
    [HttpGet("{id:guid}/fisa")]
    [ProducesResponseType(typeof(FisaImobilizareDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status404NotFound)]
    public IActionResult Fisa(Guid id, [FromQuery] DateOnly? laData = null) {
        var refuz = AutorizeazaCitire<Imobilizare>(id) ?? RegistrulCitibil();
        if (refuz != null)
            return refuz;
        using var os = NonSecured(typeof(Imobilizare));
        var dto = ImobilizariApply.Fisa(os, id, laData ?? DateOnly.FromDateTime(DateTime.Today));
        return dto == null ? Invizibil() : Ok(dto);
    }

    // Raport întreg, ca D300: fără `loadOptions` — totalurile sunt ale registrului, nu ale unei pagini (42c).
    [HttpGet("registru")]
    [ProducesResponseType(typeof(RegistruImobilizariDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status403Forbidden)]
    public IActionResult Registru([FromQuery] DateOnly? laData = null) {
        using (var osSecured = Secured(typeof(Imobilizare))) {
            if (!PoateCiti(typeof(Imobilizare), osSecured))
                return RefuzCitire(typeof(Imobilizare));
        }
        var refuzRegistru = RegistrulCitibil();
        if (refuzRegistru != null)
            return refuzRegistru;
        return Domeniu(() => {
            using var os = NonSecured(typeof(Imobilizare));
            return Ok(ImobilizariApply.Registru(os, laData ?? DateOnly.FromDateTime(DateTime.Today)));
        });
    }

    IActionResult RegistrulCitibil() {
        using var os = Secured(typeof(RegistruImobilizari));
        return PoateCiti(typeof(RegistruImobilizari), os)
            ? null
            : RefuzCitire(typeof(RegistruImobilizari));
    }
}
