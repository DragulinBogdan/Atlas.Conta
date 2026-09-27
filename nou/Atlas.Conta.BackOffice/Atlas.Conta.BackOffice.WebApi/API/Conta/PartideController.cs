using Atlas.Conta.BackOffice.Module.Api;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Proiectii;
using DevExpress.ExpressApp;
using Microsoft.AspNetCore.Mvc;

namespace Atlas.Conta.BackOffice.WebApi.API.Conta;

[Route("api/proiectii/partide")]
public class PartideController(IObjectSpaceFactory secured, INonSecuredObjectSpaceFactory nonSecured,
    DevExpress.ExpressApp.Security.ISecurityStrategyBase securitate)
    : ContaApiController(secured, nonSecured, securitate) {
    [HttpGet]
    [ProducesResponseType(typeof(PaginaDto<PartidaCuRestRand>), StatusCodes.Status200OK)]
    public IActionResult Get(DataSourceLoadOptions loadOptions, [FromQuery] Guid? contrapartidaId = null,
            [FromQuery] string sens = null, [FromQuery] DateOnly? laData = null) {
        SensStingere? ales = null;
        if (!string.IsNullOrEmpty(sens)) {
            if (sens != nameof(SensStingere.Datorie) && sens != nameof(SensStingere.Creanta))
                return BadRequest(EroriDto.DinMesaj("Sensul trebuie să fie Datorie sau Creanta."));
            ales = Enum.Parse<SensStingere>(sens);
        }
        using var os = Secured(typeof(Atlas.Conta.BackOffice.Module.Cub.Postare));
        if (FaraPostariCitibile(os)) return Ok(Incarca(Array.Empty<PartidaCuRestRand>().AsQueryable(), loadOptions));
        var rezultat = Incarca(ImperecheriProiectii.PartideCuRest(os, contrapartidaId, ales, laData), loadOptions,
            [OrdineLista.Crescator(nameof(PartidaCuRestRand.UnitateId)),
             OrdineLista.Crescator(nameof(PartidaCuRestRand.ContId)),
             OrdineLista.Crescator(nameof(PartidaCuRestRand.ContrapartidaId))]);
        ContabilProiectii.CompleteazaTipDocument(os, Randuri<PartidaCuRestRand>(rezultat));
        return Ok(rezultat);
    }
}
