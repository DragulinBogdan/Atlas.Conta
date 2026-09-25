using Atlas.Conta.BackOffice.Module.Api;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Proiectii;
using DevExpress.ExpressApp;
using Microsoft.AspNetCore.Mvc;

namespace Atlas.Conta.BackOffice.WebApi.API.Conta;

[Route("api/proiectii/sold-parteneri")]
public class SoldPartenerController : ContaApiController {
    public SoldPartenerController(IObjectSpaceFactory secured, INonSecuredObjectSpaceFactory nonSecured,
        DevExpress.ExpressApp.Security.ISecurityStrategyBase securitate)
        : base(secured, nonSecured, securitate) { }

    [HttpGet]
    [ProducesResponseType(typeof(PaginaDto<SoldPartenerRand>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status400BadRequest)]
    public IActionResult Get(DataSourceLoadOptions loadOptions,
        [FromQuery] DateOnly? laData = null,
        [FromQuery] Guid? contId = null, [FromQuery] Guid? repartitorId = null,
        [FromQuery] Guid? materialId = null, [FromQuery] Guid? codFunctionalId = null,
        [FromQuery] Guid? codEconomicId = null, [FromQuery] Guid? sursaFinantareId = null,
        [FromQuery] Guid? unitateId = null, [FromQuery] Guid? proiectId = null,
        [FromQuery] Guid? centruCostId = null, [FromQuery] Guid? gestiuneId = null) {

        if (laData is DateOnly d && d == default)
            return BadRequest(EroriDto.DinMesaj(
                "Parametrul „laData” nu e o dată validă (soldul se citește la o zi)."));

        using var os = Secured(typeof(Atlas.Conta.BackOffice.Module.Cub.Postare));
        if (FaraPostariCitibile(os)) return Ok(Incarca(Array.Empty<SoldPartenerRand>().AsQueryable(), loadOptions));
        return Ok(Incarca(ContabilProiectii.SoldParteneri(os,
            laData ?? DateOnly.FromDateTime(DateTime.Today), contId, repartitorId,
            materialId, codFunctionalId, codEconomicId, sursaFinantareId,
            unitateId, proiectId, centruCostId, gestiuneId),
            loadOptions, ContabilProiectii.OrdineSoldParteneri()));
    }
}
