using Atlas.Conta.BackOffice.Module.Api;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Motor;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Security;
using Microsoft.AspNetCore.Mvc;

namespace Atlas.Conta.BackOffice.WebApi.API.Conta;

[Route("api/proiectii/diagnostic-tva")]
public class DiagnosticTvaController(IObjectSpaceFactory secured, INonSecuredObjectSpaceFactory nonSecured,
        ISecurityStrategyBase securitate) : ContaApiController(secured, nonSecured, securitate) {
    [HttpGet]
    [ProducesResponseType(typeof(PaginaDto<RandDiagnosticTva>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status403Forbidden)]
    public IActionResult Get(DataSourceLoadOptions loadOptions, DateOnly? dataStart = null,
            DateOnly? dataEnd = null, Guid? tipTvaId = null, bool includeCompensate = false) {
        if (dataStart > dataEnd)
            return BadRequest(EroriDto.DinMesaj("Începutul perioadei nu poate urma sfârșitului."));
        using var os = Secured(typeof(Document));
        if (!PoateCiti(typeof(Atlas.Conta.BackOffice.Module.Cub.Postare), os))
            return RefuzCitire(typeof(Atlas.Conta.BackOffice.Module.Cub.Postare));
        var randuri = DiagnosticTvaService.Citeste(os, dataStart, dataEnd, tipTvaId, includeCompensate,
            poateCiti: (obj, membru) => PoateCitiMembrul(os, obj, membru));
        return Ok(Incarca(randuri.AsQueryable(), loadOptions,
            new() { Selector = nameof(RandDiagnosticTva.Exigibilitate) },
            new() { Selector = nameof(RandDiagnosticTva.DocumentId) },
            new() { Selector = nameof(RandDiagnosticTva.LinieId) },
            new() { Selector = nameof(RandDiagnosticTva.Cod) }));
    }
}

[Route("api/documente/{id:guid}/recalculeaza-tva")]
public class RecalculTvaController(IObjectSpaceFactory secured, INonSecuredObjectSpaceFactory nonSecured,
        ISecurityStrategyBase securitate) : ContaApiController(secured, nonSecured, securitate) {
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult Post(Guid id, [FromBody] RecalculTvaRequestDto cerere) =>
        ScriereAutorizata<Document>(id, () => Domeniu(() => {
            using var os = Secured(typeof(Document));
            RecalculTvaApply.Aplica(os, id, cerere.Linii ?? []);
            return NoContent();
        }));
}
