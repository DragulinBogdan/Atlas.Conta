using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Proiectii;
using DevExpress.ExpressApp;
using Microsoft.AspNetCore.Mvc;

namespace Atlas.Conta.BackOffice.WebApi.API.Conta;

// Proiecția de raportare (D9, decizia 42c): soldul de stoc per
// `Lot × Repartitor × TipStoc`, direct din registrul append-only. READ-ONLY prin
// construcție — nu există verb de scriere pe registre nicăieri în API (gardianul
// le refuză oricum, pe orice cale secured).
[Route("api/proiectii/sold-stoc")]
public class SoldStocController : ContaApiController {
    public SoldStocController(IObjectSpaceFactory secured, INonSecuredObjectSpaceFactory nonSecured,
        DevExpress.ExpressApp.Security.ISecurityStrategyBase securitate)
        : base(secured, nonSecured, securitate) { }

    [HttpGet]
    [ProducesResponseType(typeof(PaginaDto<SoldStocRand>), StatusCodes.Status200OK)]
    // `laData` e PARAMETRU AL PROIECȚIEI, ca perioada balanței: fără el soldul e
    // cel de azi (comportamentul de dinaintea feliei 27), cu el e soldul la
    // sfârșitul zilei cerute. O valoare imposibilă cade pe 400-ul unic al
    // tierului (`[ApiController]` + `InvalidModelStateResponseFactory`), în forma
    // `EroriDto`, ca orice eșec de model binding.
    public object Get(DataSourceLoadOptions loadOptions, [FromQuery] DateOnly? laData = null) {
        using var os = Secured(typeof(RegistruStoc));
        return Incarca(StocProiectii.SoldStoc(os, laData), loadOptions);
    }
}
