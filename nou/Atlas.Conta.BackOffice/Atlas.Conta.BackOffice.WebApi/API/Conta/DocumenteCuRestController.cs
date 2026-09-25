using Atlas.Conta.BackOffice.Module.Api;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Proiectii;
using DevExpress.ExpressApp;
using Microsoft.AspNetCore.Mvc;

namespace Atlas.Conta.BackOffice.WebApi.API.Conta;

[Route("api/proiectii/documente-cu-rest")]
public class DocumenteCuRestController : ContaApiController {
    public DocumenteCuRestController(IObjectSpaceFactory secured, INonSecuredObjectSpaceFactory nonSecured,
        DevExpress.ExpressApp.Security.ISecurityStrategyBase securitate)
        : base(secured, nonSecured, securitate) { }

    [HttpGet]
    [ProducesResponseType(typeof(PaginaDto<DocumentCuRestRand>), StatusCodes.Status200OK)]
    public object Get(DataSourceLoadOptions loadOptions, [FromQuery] Guid? contrapartidaId = null,
        [FromQuery] string sens = null, [FromQuery] DateOnly? laData = null,
        [FromQuery] Guid? documentCurentId = null, [FromQuery] bool stinge = true) {
        SensStingere? sensCerut = null;
        if (!string.IsNullOrWhiteSpace(sens)) {
            if (sens != nameof(SensStingere.Datorie) && sens != nameof(SensStingere.Creanta))
                return BadRequest(EroriDto.DinMesaj(
                    $"Sensul stingerii („{sens}”) nu e cunoscut: "
                    + $"{nameof(SensStingere.Datorie)} sau {nameof(SensStingere.Creanta)}."));
            sensCerut = Enum.Parse<SensStingere>(sens);
        }
        using var os = Secured(typeof(Atlas.Conta.BackOffice.Module.Cub.Postare));
        if (FaraPostariCitibile(os)) return Incarca(Array.Empty<DocumentCuRestRand>().AsQueryable(), loadOptions);
        return Incarca(ImperecheriProiectii.DocumenteCuRest(os, contrapartidaId, sensCerut, laData, documentCurentId, stinge),
            loadOptions);
    }
}
