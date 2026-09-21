using Atlas.Conta.BackOffice.Module.Api;
using Atlas.Conta.BackOffice.Module.Api.Pif;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using DevExpress.ExpressApp;
using Microsoft.AspNetCore.Mvc;

namespace Atlas.Conta.BackOffice.WebApi.API.Conta;

// Punerea în funcțiune (F26-D5/D10). Transport pur: regulile sunt în `PifApply` (42a).
// Derivă direct din `Document`, deci un id de PIF pe ușa NTC/FCT dă 404 fără predicat de felie.
[Route("api/pif")]
public class PifController : ContaApiController {
    public PifController(IObjectSpaceFactory secured, INonSecuredObjectSpaceFactory nonSecured,
        DevExpress.ExpressApp.Security.ISecurityStrategyBase securitate)
        : base(secured, nonSecured, securitate) { }

    [HttpGet]
    [ProducesResponseType(typeof(PaginaDto<PifListDto>), StatusCodes.Status200OK)]
    public object Get(DataSourceLoadOptions loadOptions) {
        using var os = Secured(typeof(PunereInFunctiune));
        return Incarca(PifApply.Lista(os), loadOptions);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PifReadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status404NotFound)]
    public IActionResult GetById(Guid id) {
        var refuz = AutorizeazaCitire<PunereInFunctiune>(id);
        if (refuz != null)
            return refuz;
        using var os = Secured(typeof(PunereInFunctiune));
        var dto = PifApply.Citeste(os, id);
        return dto == null ? Invizibil() : Ok(dto);
    }

    // Dreptul cerut e cel pe FACTURI: panoul arată linii de factură (F26-D5).
    // Ordinea: 400 (cererea) → 403 (dreptul) → 200/422.
    [HttpGet("linii-sursa")]
    [ProducesResponseType(typeof(LiniiSursaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult LiniiSursa([FromQuery] DateOnly? dataStart = null,
            [FromQuery] DateOnly? dataEnd = null, [FromQuery] Guid? partenerId = null,
            [FromQuery] bool toate = false) {
        var erori = new List<string>();
        if (dataStart == null)
            erori.Add("Începutul perioadei (`dataStart`) e obligatoriu.");
        if (dataEnd == null)
            erori.Add("Sfârșitul perioadei (`dataEnd`) e obligatoriu.");
        if (erori.Count > 0)
            return BadRequest(EroriDto.Din(erori));

        using (var osFacturi = Secured(typeof(FacturaIntrare)))
            if (!PoateCiti(typeof(FacturaIntrare), osFacturi))
                return RefuzCitire(typeof(FacturaIntrare));

        return Domeniu(() => {
            using var os = Secured(typeof(FacturaIntrare));
            return Ok(PifApply.LiniiSursa(os, dataStart.Value, dataEnd.Value, partenerId, toate));
        });
    }

    // Scriere: agregatul per document (42d).
    [HttpPost]
    [ProducesResponseType(typeof(PifReadDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult Post([FromBody] PifWriteDto dto) =>
        CreareAutorizata<PunereInFunctiune>(() => Domeniu(() => {
            using var os = Secured(typeof(PunereInFunctiune));
            var id = PifApply.Aplica(os, null, dto);
            return Created($"/api/pif/{id}", PifApply.Citeste(os, id));
        }));

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(PifReadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult Put(Guid id, [FromBody] PifWriteDto dto) =>
        ScriereAutorizata<PunereInFunctiune>(id, () => Domeniu(() => {
            using var os = Secured(typeof(PunereInFunctiune));
            PifApply.Aplica(os, id, dto);
            return Ok(PifApply.Citeste(os, id));
        }));

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult Delete(Guid id) =>
        ScriereAutorizata<PunereInFunctiune>(id, () => Domeniu(() => {
            using var os = Secured(typeof(PunereInFunctiune));
            PifApply.Sterge(os, id);
            return NoContent();
        }), OperatieAcces.Stergere);

    // Comenzi: OS NON-SECURED, tranzacția integral a motorului (42b).
    [HttpPost("{id:guid}/opereaza")]
    [ProducesResponseType(typeof(OperareRezultatDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult Opereaza(Guid id) => Comanda(id, os => OperareApi.Opereaza(os, id));

    [HttpPost("{id:guid}/anuleaza")]
    [ProducesResponseType(typeof(OperareRezultatDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult Anuleaza(Guid id) => Comanda(id, os => OperareApi.AnuleazaOperarea(os, id));

    [HttpPost("{id:guid}/storneaza")]
    [ProducesResponseType(typeof(OperareRezultatDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult Storneaza(Guid id, [FromBody] StornoRequestDto cerere) =>
        Comanda(id, os => OperareApi.Storneaza(os, id, cerere?.Data ?? DateOnly.FromDateTime(DateTime.Today)));

    [HttpPost("{id:guid}/valideaza")]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status404NotFound)]
    public IActionResult Valideaza(Guid id) =>
        ComandaAutorizata<PunereInFunctiune>(id, () => Domeniu(() => {
            using var os = NonSecured(typeof(PunereInFunctiune));
            return Ok(EroriDto.Din(OperareApi.Valideaza(os, id)));
        }));

    IActionResult Comanda(Guid id, Func<IObjectSpace, OperareRezultat> comanda) =>
        ComandaAutorizata<PunereInFunctiune>(id, () => Domeniu(() => {
            using var os = NonSecured(typeof(PunereInFunctiune));
            return Ok(OperareRezultatDto.Din(comanda(os)));
        }));
}
