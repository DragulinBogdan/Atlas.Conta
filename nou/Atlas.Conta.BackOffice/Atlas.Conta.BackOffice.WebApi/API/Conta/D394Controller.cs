using Atlas.Conta.BackOffice.Module.Api;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Proiectii;
using DevExpress.ExpressApp;
using Microsoft.AspNetCore.Mvc;

namespace Atlas.Conta.BackOffice.WebApi.API.Conta;

// Declarația informativă 394 (felia 14, D4-D6): proiecția `RegistruTva` →
// listele și rezumatele formularului, PER PARTENER. READ-ONLY prin construcție,
// ca toate proiecțiile de registru.
//
// Același contract ca D300, pentru aceleași motive — răspunsul e o DECLARAȚIE,
// nu o listă:
//  • fără `loadOptions`: `op1` nu se paginează (o pagină din lista partenerilor
//    nu e „o declarație parțială", e una falsă), rezumatele C–H sunt calculate
//    peste întregul ei, iar `nrCui1..4` n-au sens pe o felie;
//  • perioada e OBLIGATORIE: perioada E declarația; un default tăcut ar produce
//    o listă plauzibilă pentru altă lună decât cea cerută.
// Un singur 400 pe sârmă (70f): parametrii lipsă/incoerenți ies aici ca
// `EroriDto`, exact în forma în care `InvalidModelStateResponseFactory` scoate
// și data malformată — clientul are UN panou pentru amândouă.
[Route("api/proiectii/d394")]
public class D394Controller : ContaApiController {
    public D394Controller(IObjectSpaceFactory secured, INonSecuredObjectSpaceFactory nonSecured,
        DevExpress.ExpressApp.Security.ISecurityStrategyBase securitate)
        : base(secured, nonSecured, securitate) { }

    [HttpGet]
    [ProducesResponseType(typeof(D394Dto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status400BadRequest)]
    public IActionResult Get([FromQuery] DateOnly? dataStart = null, [FromQuery] DateOnly? dataEnd = null) {
        // Nullable EXACT ca să se distingă „lipsă" de `default(DateOnly)`
        // (0001-01-01), pe care binding-ul l-ar fi livrat tăcut (lecția D300).
        var erori = new List<string>();
        if (dataStart == null)
            erori.Add("Parametrul „dataStart” este obligatoriu (începutul perioadei de raportare).");
        if (dataEnd == null)
            erori.Add("Parametrul „dataEnd” este obligatoriu (sfârșitul perioadei de raportare).");
        if (dataStart is DateOnly ds && dataEnd is DateOnly de && ds > de)
            erori.Add("„dataStart” nu poate fi după „dataEnd”.");
        if (erori.Count > 0)
            return BadRequest(EroriDto.Din(erori));

        // Aceeași ușă securizată ca D300 și jurnalele: cine n-are voie să
        // citească registrul fiscal nu-l citește nici așezat pe parteneri (`User`
        // ⇒ 200 cu liste goale — ușa filtrează rândurile, 69g). Proiecția
        // întoarce liste materializate: nimic deferred după `using`.
        using var os = Secured(typeof(RegistruTva));
        return Ok(D394Proiectii.D394(os, dataStart.Value, dataEnd.Value));
    }
}
