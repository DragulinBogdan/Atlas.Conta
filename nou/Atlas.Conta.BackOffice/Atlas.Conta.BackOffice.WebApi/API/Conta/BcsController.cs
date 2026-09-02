using Atlas.Conta.BackOffice.Module.Api;
using Atlas.Conta.BackOffice.Module.Api.Bcs;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using DevExpress.ExpressApp;
using Microsoft.AspNetCore.Mvc;

namespace Atlas.Conta.BackOffice.WebApi.API.Conta;

// Felia BCS (F6): consumul cules manual — CRUD + comenzi, pe șablonul consolidat
// al feliilor F2–F5. Transport pur: regulile sunt în `BonConsumApply` (Module),
// exersate din ModelCheck pe același cod.
//
// Comenzile sunt identice cu ale oricărei felii: `OperareApi` e agnostic de tip,
// iar gate-ul de autorizare vine din `ContaApiController`.
[Route("api/bcs")]
public class BcsController : ContaApiController {
    public BcsController(IObjectSpaceFactory secured, INonSecuredObjectSpaceFactory nonSecured,
        DevExpress.ExpressApp.Security.ISecurityStrategyBase securitate)
        : base(secured, nonSecured, securitate) { }

    // ── Citire ────────────────────────────────────────────────────────────
    [HttpGet]
    [ProducesResponseType(typeof(PaginaDto<BcsListDto>), StatusCodes.Status200OK)]
    public object Get(DataSourceLoadOptions loadOptions) {
        using var os = Secured(typeof(BonConsum));
        return Incarca(BonConsumApply.Lista(os), loadOptions);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(BcsReadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status404NotFound)]
    public IActionResult GetById(Guid id) {
        using var os = Secured(typeof(BonConsum));
        var dto = BonConsumApply.Citeste(os, id);
        return dto == null ? Invizibil() : Ok(dto);
    }

    // ── Scriere: agregatul per document (42d) ─────────────────────────────
    // `Numar` nu apare în WriteDto: BCS are politică de numerotare, deci seria e
    // server-owned (F6-D4) — ca BTR/NIR/FCL, invers față de FCT.
    [HttpPost]
    [ProducesResponseType(typeof(BcsReadDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status403Forbidden)]
    public IActionResult Post([FromBody] BcsWriteDto dto) =>
        CreareAutorizata<BonConsum>(() => Domeniu(() => {
            using var os = Secured(typeof(BonConsum));
            var id = BonConsumApply.Aplica(os, null, dto);
            return Created($"/api/bcs/{id}", BonConsumApply.Citeste(os, id));
        }));

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(BcsReadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status404NotFound)]
    public IActionResult Put(Guid id, [FromBody] BcsWriteDto dto) =>
        ScriereAutorizata<BonConsum>(id, () => Domeniu(() => {
            using var os = Secured(typeof(BonConsum));
            BonConsumApply.Aplica(os, id, dto);
            return Ok(BonConsumApply.Citeste(os, id));
        }));

    // Ștergerea unui DRAFT. Pre-check-ul de domeniu e în `Sterge` (mesaj propriu),
    // gardianul de Committing rămâne plasa.
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status404NotFound)]
    public IActionResult Delete(Guid id) =>
        ScriereAutorizata<BonConsum>(id, () => Domeniu(() => {
            using var os = Secured(typeof(BonConsum));
            BonConsumApply.Sterge(os, id);
            return NoContent();
        }), OperatieAcces.Stergere);

    // ── Comenzi: OS NON-SECURED, tranzacția integral a motorului (42b) ─────
    [HttpPost("{id:guid}/opereaza")]
    [ProducesResponseType(typeof(OperareRezultatDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status404NotFound)]
    public IActionResult Opereaza(Guid id) => Comanda(id, os => OperareApi.Opereaza(os, id));

    [HttpPost("{id:guid}/anuleaza")]
    [ProducesResponseType(typeof(OperareRezultatDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status404NotFound)]
    public IActionResult Anuleaza(Guid id) => Comanda(id, os => OperareApi.AnuleazaOperarea(os, id));

    [HttpPost("{id:guid}/storneaza")]
    [ProducesResponseType(typeof(OperareRezultatDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status404NotFound)]
    public IActionResult Storneaza(Guid id, [FromBody] StornoRequestDto cerere) =>
        Comanda(id, os => OperareApi.Storneaza(os, id, cerere?.Data ?? DateOnly.FromDateTime(DateTime.Today)));

    [HttpPost("{id:guid}/valideaza")]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status404NotFound)]
    public IActionResult Valideaza(Guid id) => ComandaAutorizata<BonConsum>(id, () => Domeniu(() => {
        using var os = NonSecured(typeof(BonConsum));
        return Ok(EroriDto.Din(OperareApi.Valideaza(os, id)));
    }));

    IActionResult Comanda(Guid id, Func<IObjectSpace, OperareRezultat> comanda) =>
        ComandaAutorizata<BonConsum>(id, () => Domeniu(() => {
            using var os = NonSecured(typeof(BonConsum));
            return Ok(OperareRezultatDto.Din(comanda(os)));
        }));
}
