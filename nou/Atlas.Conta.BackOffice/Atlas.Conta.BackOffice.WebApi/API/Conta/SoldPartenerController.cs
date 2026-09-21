using Atlas.Conta.BackOffice.Module.Api;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Proiectii;
using DevExpress.ExpressApp;
using Microsoft.AspNetCore.Mvc;

namespace Atlas.Conta.BackOffice.WebApi.API.Conta;

// Soldurile pe (cont × repartitor) la o dată (F27-D7) — „cât are de dat / de
// luat fiecare, azi". READ-ONLY, ca balanța, și pe aceiași atomi cumulați: un
// rând de aici e rândul analitic al balanței, redus la partea de sold.
//
// `laData` e parametru al PROIECȚIEI (granița cumulului), nu filtru
// `DataSourceLoader`; `contId`/`repartitorId` și celelalte dimensiuni se aplică
// pe atomi, ÎNAINTE de `GROUP BY`. `loadOptions` rămâne deasupra pentru sortare,
// paginare și filtrare pe coloanele de ieșire — tiparul `BalantaController`.
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
        [FromQuery] Guid? centruCostId = null) {

        // Spre deosebire de balanță, data are un default onest: „soldul la zi".
        // `default(DateOnly)` venit de la model binding ar fi însă 0001-01-01,
        // adică un raport gol care arată ca o bază goală — 400, ca la balanță.
        if (laData is DateOnly d && d == default)
            return BadRequest(EroriDto.DinMesaj(
                "Parametrul „laData” nu e o dată validă (soldul se citește la o zi)."));

        using var os = Secured(typeof(RegistruContabil));
        return Ok(Incarca(ContabilProiectii.SoldParteneri(os,
            laData ?? DateOnly.FromDateTime(DateTime.Today), contId, repartitorId,
            materialId, codFunctionalId, codEconomicId, sursaFinantareId,
            unitateId, proiectId, centruCostId),
            loadOptions, ContabilProiectii.OrdineSoldParteneri()));
    }
}
