using Atlas.Conta.BackOffice.Module.Api;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Proiectii;
using DevExpress.ExpressApp;
using Microsoft.AspNetCore.Mvc;

namespace Atlas.Conta.BackOffice.WebApi.API.Conta;

[Route("api/proiectii/fisa-cont")]
public class FisaContController : ContaApiController {
    public FisaContController(IObjectSpaceFactory secured, INonSecuredObjectSpaceFactory nonSecured,
        DevExpress.ExpressApp.Security.ISecurityStrategyBase securitate)
        : base(secured, nonSecured, securitate) { }

    [HttpGet]
    [ProducesResponseType(typeof(PaginaDto<FisaContRand>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status403Forbidden)]
    public IActionResult Get(DataSourceLoadOptions loadOptions,
        [FromQuery] Guid? contId = null,
        [FromQuery] DateOnly? dataStart = null, [FromQuery] DateOnly? dataEnd = null,
        [FromQuery] Guid? repartitorId = null, [FromQuery] Guid? materialId = null,
        [FromQuery] Guid? codFunctionalId = null, [FromQuery] Guid? codEconomicId = null,
        [FromQuery] Guid? sursaFinantareId = null, [FromQuery] Guid? unitateId = null,
        [FromQuery] Guid? proiectId = null, [FromQuery] Guid? centruCostId = null,
        [FromQuery] bool repartitorNul = false, [FromQuery] bool faraTert = false) {

        var erori = Erori(contId, dataStart, dataEnd, repartitorId, repartitorNul, faraTert);
        if (erori.Count > 0)
            return BadRequest(EroriDto.Din(erori));

        loadOptions.Sort = null;
        loadOptions.Group = null;

        using var os = Secured(typeof(Atlas.Conta.BackOffice.Module.Cub.Postare));

        if (os.GetObjectByKey<Cont>(contId.Value) == null)
            return Invizibil();
        if (FaraPostariCitibile(os)) return Ok(Incarca(Array.Empty<FisaContRand>().AsQueryable(), loadOptions));
        var rezultat = Incarca(ContabilProiectii.FisaCont(os, contId.Value,
            dataStart.Value, dataEnd.Value,
            repartitorId, materialId, codFunctionalId, codEconomicId,
            sursaFinantareId, unitateId, proiectId, centruCostId, repartitorNul, faraTert: faraTert),
            loadOptions, ContabilProiectii.OrdineFisa());
        ContabilProiectii.CompleteazaTipDocument(os, Randuri<FisaContRand>(rezultat));
        return Ok(rezultat);
    }

    [HttpGet("sold")]
    [ProducesResponseType(typeof(FisaContSold), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status403Forbidden)]
    public IActionResult Sold(
        [FromQuery] Guid? contId = null,
        [FromQuery] DateOnly? dataStart = null, [FromQuery] DateOnly? dataEnd = null,
        [FromQuery] Guid? repartitorId = null, [FromQuery] Guid? materialId = null,
        [FromQuery] Guid? codFunctionalId = null, [FromQuery] Guid? codEconomicId = null,
        [FromQuery] Guid? sursaFinantareId = null, [FromQuery] Guid? unitateId = null,
        [FromQuery] Guid? proiectId = null, [FromQuery] Guid? centruCostId = null,
        [FromQuery] bool repartitorNul = false, [FromQuery] bool faraTert = false) {

        var erori = Erori(contId, dataStart, dataEnd, repartitorId, repartitorNul, faraTert);
        if (erori.Count > 0)
            return BadRequest(EroriDto.Din(erori));

        using var os = Secured(typeof(Atlas.Conta.BackOffice.Module.Cub.Postare));

        if (os.GetObjectByKey<Cont>(contId.Value) == null)
            return Invizibil();
        if (FaraPostariCitibile(os)) return Ok(new FisaContSold());
        return Ok(ContabilProiectii.FisaSold(os, contId.Value, dataStart.Value, dataEnd.Value,
            repartitorId, materialId, codFunctionalId, codEconomicId,
            sursaFinantareId, unitateId, proiectId, centruCostId, repartitorNul, faraTert: faraTert));
    }

    static List<string> Erori(Guid? contId, DateOnly? dataStart, DateOnly? dataEnd, Guid? repartitorId,
            bool repartitorNul, bool faraTert) {
        var erori = new List<string>();
        if (contId == null || contId == Guid.Empty)
            erori.Add("Parametrul „contId” este obligatoriu (fișa e a unui cont anume).");
        if (dataStart == null)
            erori.Add("Parametrul „dataStart” este obligatoriu (definește soldul inițial).");
        if (dataEnd == null)
            erori.Add("Parametrul „dataEnd” este obligatoriu (definește sfârșitul perioadei).");
        if (dataStart is DateOnly ds && dataEnd is DateOnly de && ds > de)
            erori.Add("„dataStart” nu poate fi după „dataEnd”.");
        if (repartitorNul && repartitorId != null)
            erori.Add("„repartitorNul” și „repartitorId” se exclud reciproc "
                + "(unul cere rândurile FĂRĂ repartitor, celălalt pe cele ale unui repartitor anume).");
        if (faraTert && (repartitorNul || repartitorId != null))
            erori.Add("„faraTert” nu se combină cu „repartitorNul” sau cu „repartitorId” "
                + "(cere tot ce nu stă pe un partener sau pe un angajat).");
        return erori;
    }
}
