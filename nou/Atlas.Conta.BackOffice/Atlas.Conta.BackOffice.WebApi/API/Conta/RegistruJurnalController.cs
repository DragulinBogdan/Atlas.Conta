using Atlas.Conta.BackOffice.Module.Api;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Proiectii;
using DevExpress.ExpressApp;
using Microsoft.AspNetCore.Mvc;

namespace Atlas.Conta.BackOffice.WebApi.API.Conta;

// Registrul-jurnal (R-D9): listarea cronologică a notelor așa cum au fost scrise
// — rândurile BRUTE ale registrului, nu atomii (unpivotat, fiecare notă ar apărea
// de două ori). Read-only.
//
// Spre deosebire de balanță și fișă, aici `dataStart`/`dataEnd` sunt filtre
// SIMPLE, deci OPȚIONALE: o listare n-are noțiune de „sold inițial", deci nici
// graniță dinăuntrul unei agregări. Din același motiv sortarea din grilă e
// permisă — n-are sold curent de rupt.
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

        using var os = Secured(typeof(RegistruContabil));
        var rezultat = Incarca(ContabilProiectii.RegistruJurnal(os, dataStart, dataEnd), loadOptions);
        // Aceeași completare ca la fișă, aceeași implementare (R-D8): codul de tip
        // nu e o coloană sub TPT. Vezi limitarea documentată pe `Randuri<T>` pentru
        // modul grupat.
        ContabilProiectii.CompleteazaTipDocument(os, Randuri<JurnalRand>(rezultat));
        return Ok(rezultat);
    }
}
