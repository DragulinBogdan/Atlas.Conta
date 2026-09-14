using Atlas.Conta.BackOffice.Module.Api;
using Atlas.Conta.BackOffice.Module.Api.Cas;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using DevExpress.ExpressApp;
using Microsoft.AspNetCore.Mvc;

namespace Atlas.Conta.BackOffice.WebApi.API.Conta;

// Ieșirea de imobilizări (F26-D6/D10). Transport pur: regulile sunt în `CasApply` (42a).
// Derivă direct din `Document`, deci un id de CAS pe ușa NTC/FCT dă 404 fără predicat de felie.
[Route("api/cas")]
public class CasController : ContaApiController {
    public CasController(IObjectSpaceFactory secured, INonSecuredObjectSpaceFactory nonSecured,
        DevExpress.ExpressApp.Security.ISecurityStrategyBase securitate)
        : base(secured, nonSecured, securitate) { }

    [HttpGet]
    [ProducesResponseType(typeof(PaginaDto<CasListDto>), StatusCodes.Status200OK)]
    public object Get(DataSourceLoadOptions loadOptions) {
        using var os = Secured(typeof(IesireImobilizare));
        return Incarca(CasApply.Lista(os), loadOptions);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CasReadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status404NotFound)]
    public IActionResult GetById(Guid id) {
        var refuz = AutorizeazaCitire<IesireImobilizare>(id);
        if (refuz != null)
            return refuz;
        using var os = Secured(typeof(IesireImobilizare));
        var dto = CasApply.Citeste(os, id);
        return dto == null ? Invizibil() : Ok(dto);
    }

    // Scriere: antetul și fișele; liniile le produce serverul (F26-D6).
    [HttpPost]
    [ProducesResponseType(typeof(CasReadDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult Post([FromBody] CasWriteDto dto) =>
        CreareAutorizata<IesireImobilizare>(() => Domeniu(() => {
            using var os = Secured(typeof(IesireImobilizare));
            var id = CasApply.Aplica(os, null, dto);
            return Created($"/api/cas/{id}", CasApply.Citeste(os, id));
        }));

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(CasReadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult Put(Guid id, [FromBody] CasWriteDto dto) =>
        ScriereAutorizata<IesireImobilizare>(id, () => Domeniu(() => {
            using var os = Secured(typeof(IesireImobilizare));
            CasApply.Aplica(os, id, dto);
            return Ok(CasApply.Citeste(os, id));
        }));

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult Delete(Guid id) =>
        ScriereAutorizata<IesireImobilizare>(id, () => Domeniu(() => {
            using var os = Secured(typeof(IesireImobilizare));
            CasApply.Sterge(os, id);
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
        ComandaAutorizata<IesireImobilizare>(id, () => Domeniu(() => {
            using var os = NonSecured(typeof(IesireImobilizare));
            return Ok(EroriDto.Din(OperareApi.Valideaza(os, id)));
        }));

    IActionResult Comanda(Guid id, Func<IObjectSpace, OperareRezultat> comanda) =>
        ComandaAutorizata<IesireImobilizare>(id, () => Domeniu(() => {
            using var os = NonSecured(typeof(IesireImobilizare));
            return Ok(OperareRezultatDto.Din(comanda(os)));
        }));
}
