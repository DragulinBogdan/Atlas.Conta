using Atlas.Conta.BackOffice.Module.Api;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Proiectii;
using DevExpress.ExpressApp;
using Microsoft.AspNetCore.Mvc;

namespace Atlas.Conta.BackOffice.WebApi.API.Conta;

[Route("api/proiectii/registru-jurnal")]
public class RegistruJurnalController : ContaApiController {
    public RegistruJurnalController(IObjectSpaceFactory secured, INonSecuredObjectSpaceFactory nonSecured,
        DevExpress.ExpressApp.Security.ISecurityStrategyBase securitate)
        : base(secured, nonSecured, securitate) { }

    [HttpGet]
    [ProducesResponseType(typeof(PaginaDto<JurnalRand>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status400BadRequest)]
    public IActionResult Get(DataSourceLoadOptions loadOptions,
        [FromQuery] DateOnly? dataStart = null, [FromQuery] DateOnly? dataEnd = null) {

        if (dataStart is DateOnly ds && dataEnd is DateOnly de && ds > de)
            return BadRequest(EroriDto.Din(new[] { "„dataStart” nu poate fi după „dataEnd”." }));

        using var os = Secured(typeof(Atlas.Conta.BackOffice.Module.Cub.Postare));
        if (FaraPostariCitibile(os)) return Ok(Incarca(Array.Empty<JurnalRand>().AsQueryable(), loadOptions));
        var rezultat = Incarca(ContabilProiectii.RegistruJurnal(os, dataStart, dataEnd),
            loadOptions, ContabilProiectii.OrdineJurnal());
        ContabilProiectii.CompleteazaTipDocument(os, Randuri<JurnalRand>(rezultat));
        return Ok(rezultat);
    }
}
