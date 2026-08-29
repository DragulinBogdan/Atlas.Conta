using Atlas.Conta.BackOffice.Module.Api;
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
    public object Get(DataSourceLoadOptions loadOptions, [FromQuery] Guid? contrapartidaId = null,
        [FromQuery] string sens = null) {
        // `sens` = A DOUA jumătate a filtrului de candidați (F19-D16, review F3).
        // Fără el panourile CLASICE (trezorerie/FCT/FCL/DEC) filtrau DOAR pe TIP,
        // ceea ce n-are legătură cu sensul: măsurat pe baza Privat, 87 din 353 de
        // contrapartide au documente pe AMBELE sensuri, deci panoul unei Încasări
        // oferea zeci de facturi de furnizor cu buton „Stinge" care duc garantat
        // la 422. Valoarea NU se deduce în TS: vine server-computed din
        // `StingeriDto.SensCandidati` al documentului curent (42c).
        //
        // Enum pe sârmă ca STRING, parsat pe NUME înainte de proiecție (57a):
        // o valoare necunoscută e eroare de client, nu filtru tăcut ignorat.
        SensStingere? sensCerut = null;
        if (!string.IsNullOrWhiteSpace(sens)) {
            // 400, nu 422: cererea e malformată, n-o refuză domeniul (ca la balanță).
            if (!Enum.TryParse<SensStingere>(sens, ignoreCase: false, out var parsat))
                return BadRequest(EroriDto.DinMesaj(
                    $"Sensul stingerii („{sens}”) nu e cunoscut: "
                    + $"{nameof(SensStingere.Datorie)} sau {nameof(SensStingere.Creanta)}."));
            sensCerut = parsat;
        }
        using var os = Secured(typeof(Document));
        return Incarca(ImperecheriProiectii.DocumenteCuRest(os, contrapartidaId, sensCerut), loadOptions);
    }
}
