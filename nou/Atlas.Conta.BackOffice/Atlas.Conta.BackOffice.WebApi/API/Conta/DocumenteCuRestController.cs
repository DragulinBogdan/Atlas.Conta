using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Proiectii;
using DevExpress.ExpressApp;
using Microsoft.AspNetCore.Mvc;

namespace Atlas.Conta.BackOffice.WebApi.API.Conta;

// Proiecția de REST (F3-D4, decizia 42c): documentele OPERATE care mai au ceva
// de stins — candidații panoului de stingeri. READ-ONLY prin construcție.
//
// `contrapartidaId` e parametru al PROIECȚIEI, nu un filtru DataSourceLoader:
// contrapartida e o latură DIFERITĂ per tip (furnizorul e predator pe FCT,
// clientul e primitor pe FCL…), deci filtrul trăiește ÎN uniunea de ramuri, pe
// coloana deja normalizată. `loadOptions` rămâne peste el, ca la orice grilă
// remote — cele două nu se ating (ASP.NET leagă `loadOptions` din query string
// prin binder-ul DevExtreme și `contrapartidaId` ca simplu parametru).
//
// `ReturClient` lipsește DELIBERAT din proiecție (override-ul `LiniiCreanta` ar
// diverge tăcut de `ImperechereService.Total`) — motivul complet e în antetul
// lui `ImperecheriProiectii`.
[Route("api/proiectii/documente-cu-rest")]
public class DocumenteCuRestController : ContaApiController {
    public DocumenteCuRestController(IObjectSpaceFactory secured, INonSecuredObjectSpaceFactory nonSecured,
        DevExpress.ExpressApp.Security.ISecurityStrategyBase securitate)
        : base(secured, nonSecured, securitate) { }

    [HttpGet]
    [ProducesResponseType(typeof(PaginaDto<DocumentCuRestRand>), StatusCodes.Status200OK)]
    public object Get(DataSourceLoadOptions loadOptions, [FromQuery] Guid? contrapartidaId = null) {
        using var os = Secured(typeof(Document));
        return Incarca(ImperecheriProiectii.DocumenteCuRest(os, contrapartidaId), loadOptions);
    }
}
