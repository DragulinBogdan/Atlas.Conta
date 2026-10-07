using Atlas.Conta.BackOffice.Module.Api;
using Atlas.Conta.BackOffice.Module.Cub.Citiri;
using DevExpress.ExpressApp;
using Microsoft.AspNetCore.Mvc;

namespace Atlas.Conta.BackOffice.WebApi.API.Conta;

[Route("api/proiectii/explicatii")]
public class ExplicatiiController(IObjectSpaceFactory secured, INonSecuredObjectSpaceFactory nonSecured,
    DevExpress.ExpressApp.Security.ISecurityStrategyBase securitate)
    : ContaApiController(secured, nonSecured, securitate) {
    [HttpGet("{tranzactieId:guid}")]
    [ProducesResponseType(typeof(ExplicatieTranzactieDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status404NotFound)]
    public IActionResult Get(Guid tranzactieId) {
        using var os = Secured(typeof(Atlas.Conta.BackOffice.Module.Cub.Postare));
        if (!Explicatii.Vizibila(os, tranzactieId))
            return Invizibil();
        var lipsuri = CriteriiCitire is { } criterii
            ? AccesComplet.Lipsuri(os, criterii, ExplicatieAcces.Citite)
            : ["securitatea hostului nu expune criteriile de citire"];
        if (lipsuri.Count > 0) {
            const int Afisate = 20;
            return StatusCode(StatusCodes.Status403Forbidden, EroriDto.Din([
                $"{ExplicatieAcces.Refuz}: explicația cere citirea completă, fără restricții, a domeniului ei; "
                + $"restricționate: {string.Join(", ", lipsuri.Take(Afisate))}"
                + (lipsuri.Count > Afisate ? $" și încă {lipsuri.Count - Afisate}." : ".")]));
        }
        return Ok(ExplicatieTranzactieDto.Din(Explicatii.PeTranzactie(os, tranzactieId), v => VersiuniPolitica.Contor(os, v)));
    }
}
