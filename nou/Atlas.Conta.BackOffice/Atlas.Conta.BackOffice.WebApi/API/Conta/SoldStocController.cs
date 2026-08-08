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
    public SoldStocController(IObjectSpaceFactory secured, INonSecuredObjectSpaceFactory nonSecured)
        : base(secured, nonSecured) { }

    [HttpGet]
    [ProducesResponseType(typeof(PaginaDto<SoldStocRand>), StatusCodes.Status200OK)]
    public object Get(DataSourceLoadOptions loadOptions) {
        using var os = Secured(typeof(RegistruStoc));
        return Incarca(StocProiectii.SoldStoc(os), loadOptions);
    }
}
