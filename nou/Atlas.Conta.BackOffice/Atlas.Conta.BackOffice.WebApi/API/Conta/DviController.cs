using Atlas.Conta.BackOffice.Module.Api;
using Atlas.Conta.BackOffice.Module.Api.Dvi;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using DevExpress.ExpressApp;
using Microsoft.AspNetCore.Mvc;

namespace Atlas.Conta.BackOffice.WebApi.API.Conta;

// Felia DVI (decizia 86, DVI-D5): declarația vamală de import. Transport pur —
// regulile sunt în `DviApply`/motor/gardian, o singură sursă pentru toate
// tierele (42a). `Dvi` derivă direct din `Document`, deci un id de DVI cerut pe
// ușa NTC/FCT nu trece gate-ul lor (TPT): 404, fără predicat de felie.
[Route("api/dvi")]
public class DviController : ContaApiController {
    public DviController(IObjectSpaceFactory secured, INonSecuredObjectSpaceFactory nonSecured,
        DevExpress.ExpressApp.Security.ISecurityStrategyBase securitate)
        : base(secured, nonSecured, securitate) { }

    // ── Citire ────────────────────────────────────────────────────────────
    [HttpGet]
    [ProducesResponseType(typeof(PaginaDto<DviListDto>), StatusCodes.Status200OK)]
    public object Get(DataSourceLoadOptions loadOptions) {
        using var os = Secured(typeof(Dvi));
        return Incarca(DviApply.Lista(os), loadOptions);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(DviReadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status404NotFound)]
    public IActionResult GetById(Guid id) {
        var refuz = AutorizeazaCitire<Dvi>(id);
        if (refuz != null)
            return refuz;
        using var os = Secured(typeof(Dvi));
        var dto = DviApply.Citeste(os, id);
        return dto == null ? Invizibil() : Ok(dto);
    }

    // Candidații de legat (DVI-D5). DOUĂ drepturi, ca la ITV (F22-D5): declarația
    // din rută (dacă vine) ȘI tipul din care se citesc rândurile — panoul arată
    // facturi, nu declarații, iar o listă filtrată de securitate ar propune
    // legături pe care operatorul nu le poate face.
    [HttpGet("facturi-candidate")]
    [ProducesResponseType(typeof(FacturiCandidateDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult FacturiCandidate([FromQuery] DateOnly? dataStart = null,
            [FromQuery] DateOnly? dataEnd = null, [FromQuery] Guid? partenerId = null,
            [FromQuery] bool toate = false, [FromQuery] Guid? dviId = null) {
        var erori = new List<string>();
        if (dataStart == null)
            erori.Add("Începutul perioadei (`dataStart`) e obligatoriu.");
        if (dataEnd == null)
            erori.Add("Sfârșitul perioadei (`dataEnd`) e obligatoriu.");
        if (erori.Count > 0)
            return BadRequest(EroriDto.Din(erori));

        if (dviId is Guid id) {
            var refuz = AutorizeazaCitire<Dvi>(id);
            if (refuz != null)
                return refuz;
        }
        using (var osFacturi = Secured(typeof(FacturaIntrare)))
            if (!PoateCiti(typeof(FacturaIntrare), osFacturi))
                return RefuzCitire(typeof(FacturaIntrare));

        return Domeniu(() => {
            using var os = Secured(typeof(FacturaIntrare));
            return Ok(DviApply.FacturiCandidate(os, dataStart.Value, dataEnd.Value,
                partenerId, toate, dviId));
        });
    }

    // ── Scriere: agregatul per document (42d) ─────────────────────────────
    [HttpPost]
    [ProducesResponseType(typeof(DviReadDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult Post([FromBody] DviWriteDto dto) =>
        CreareAutorizata<Dvi>(() => Domeniu(() => {
            using var os = Secured(typeof(Dvi));
            var id = DviApply.Aplica(os, null, dto);
            return Created($"/api/dvi/{id}", DviApply.Citeste(os, id));
        }));

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(DviReadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult Put(Guid id, [FromBody] DviWriteDto dto) =>
        ScriereAutorizata<Dvi>(id, () => Domeniu(() => {
            using var os = Secured(typeof(Dvi));
            DviApply.Aplica(os, id, dto);
            return Ok(DviApply.Citeste(os, id));
        }));

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult Delete(Guid id) =>
        ScriereAutorizata<Dvi>(id, () => Domeniu(() => {
            using var os = Secured(typeof(Dvi));
            DviApply.Sterge(os, id);
            return NoContent();
        }), OperatieAcces.Stergere);

    // ── Comenzi: OS NON-SECURED, tranzacția integral a motorului (42b) ─────
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
    public IActionResult Valideaza(Guid id) => ComandaAutorizata<Dvi>(id, () => Domeniu(() => {
        using var os = NonSecured(typeof(Dvi));
        return Ok(EroriDto.Din(OperareApi.Valideaza(os, id)));
    }));

    IActionResult Comanda(Guid id, Func<IObjectSpace, OperareRezultat> comanda) =>
        ComandaAutorizata<Dvi>(id, () => Domeniu(() => {
            using var os = NonSecured(typeof(Dvi));
            return Ok(OperareRezultatDto.Din(comanda(os)));
        }));
}
