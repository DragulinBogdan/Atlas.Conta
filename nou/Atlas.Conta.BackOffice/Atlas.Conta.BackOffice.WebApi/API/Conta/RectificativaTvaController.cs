using Atlas.Conta.BackOffice.Module.Api;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Proiectii;
using DevExpress.ExpressApp;
using Microsoft.AspNetCore.Mvc;

namespace Atlas.Conta.BackOffice.WebApi.API.Conta;

// Conținutul de rectificativă al unei perioade (F27-D5): rândurile fiscale
// declarate în ea și scrise DUPĂ ce a fost închisă prima oară. Proiecție, ca
// jurnalul — read-only, ușă securizată, fără `loadOptions` (răspunsul e un
// raport pe o lună, nu o grilă care se paginează).
//
// Subiectul e LUNA, ca la `api/perioade`, dar ca parametri de interogare:
// ecranele fiscale duc deja perioada în URL prin `dataStart`/`dataEnd`, iar
// ruta asta e chemată alături de ele, cu aceeași lună.
//
// Gate-ul e DUBLU fiindcă răspunsul amestecă două tipuri: perioada (existența și
// reperul ei) și registrul fiscal (cifrele). Ordinea e cea din decizia 80:
// 400 pe margini, 404 pe luna nedefinită SAU invizibilă, 403 pe registrul fără
// drept de citire.
[Route("api/proiectii/rectificativa-tva")]
public class RectificativaTvaController : ContaApiController {
    const int AnMinim = 2000, AnMaxim = 2100;

    public RectificativaTvaController(IObjectSpaceFactory secured,
        INonSecuredObjectSpaceFactory nonSecured,
        DevExpress.ExpressApp.Security.ISecurityStrategyBase securitate)
        : base(secured, nonSecured, securitate) { }

    [HttpGet]
    [ProducesResponseType(typeof(RectificativaTva), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status404NotFound)]
    public IActionResult Get([FromQuery] int? an = null, [FromQuery] int? luna = null) {
        var erori = new List<string>();
        if (an == null)
            erori.Add("Parametrul „an” este obligatoriu (anul perioadei de raportare).");
        else if (an < AnMinim || an > AnMaxim)
            erori.Add($"„an” trebuie să fie între {AnMinim} și {AnMaxim}.");
        if (luna == null)
            erori.Add("Parametrul „luna” este obligatoriu (luna perioadei de raportare).");
        else if (luna < 1 || luna > 12)
            erori.Add("„luna” trebuie să fie între 1 și 12 — perioada fiscală e o lună.");
        if (erori.Count > 0)
            return BadRequest(EroriDto.Din(erori));

        using (var osPerioada = Secured(typeof(PerioadaFiscala)))
            if (!osPerioada.GetObjectsQuery<PerioadaFiscala>().Any(p => p.An == an && p.Luna == luna))
                return Invizibil();

        using var os = Secured(typeof(RegistruTva));
        return PoateCiti(typeof(RegistruTva), os)
            ? Ok(TvaProiectii.Rectificativa(os, an.Value, luna.Value))
            : RefuzCitire(typeof(RegistruTva));
    }
}
