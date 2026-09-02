using Atlas.Conta.BackOffice.Module.Api;
using Atlas.Conta.BackOffice.Module.Api.Ldi;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using DevExpress.ExpressApp;
using Microsoft.AspNetCore.Mvc;

namespace Atlas.Conta.BackOffice.WebApi.API.Conta;

// Felia LDI (F6): lista de diferențe de inventar — CRUD + comenzi, pe șablonul
// consolidat al feliilor F2–F5. Transport pur: regulile sunt în
// `ListaDiferenteInventarApply` (Module), exersate din ModelCheck pe același cod.
// Culegerea bidirecțională (plus care naște lot / minus care descarcă unul) e
// integral acolo, inclusiv seam-ul `LoturiCulegereService`.
[Route("api/ldi")]
public class LdiController : ContaApiController {
    public LdiController(IObjectSpaceFactory secured, INonSecuredObjectSpaceFactory nonSecured,
        DevExpress.ExpressApp.Security.ISecurityStrategyBase securitate)
        : base(secured, nonSecured, securitate) { }

    // ── Citire ────────────────────────────────────────────────────────────
    [HttpGet]
    [ProducesResponseType(typeof(PaginaDto<LdiListDto>), StatusCodes.Status200OK)]
    public object Get(DataSourceLoadOptions loadOptions) {
        using var os = Secured(typeof(ListaDiferenteInventar));
        return Incarca(ListaDiferenteInventarApply.Lista(os), loadOptions);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(LdiReadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status404NotFound)]
    public IActionResult GetById(Guid id) {
        using var os = Secured(typeof(ListaDiferenteInventar));
        var dto = ListaDiferenteInventarApply.Citeste(os, id);
        return dto == null ? Invizibil() : Ok(dto);
    }

    // ── Scriere: agregatul per document (42d) ─────────────────────────────
    // `Numar` nu apare în WriteDto: LDI are politică de numerotare, deci seria e
    // server-owned (F6-D4). Nici `LotId` pe liniile de plus — lotul se naște la
    // culegere, prin serviciu (F6-D5).
    [HttpPost]
    [ProducesResponseType(typeof(LdiReadDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status403Forbidden)]
    public IActionResult Post([FromBody] LdiWriteDto dto) =>
        CreareAutorizata<ListaDiferenteInventar>(() => Domeniu(() => {
            using var os = Secured(typeof(ListaDiferenteInventar));
            var id = ListaDiferenteInventarApply.Aplica(os, null, dto);
            return Created($"/api/ldi/{id}", ListaDiferenteInventarApply.Citeste(os, id));
        }));

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(LdiReadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status404NotFound)]
    public IActionResult Put(Guid id, [FromBody] LdiWriteDto dto) =>
        ScriereAutorizata<ListaDiferenteInventar>(id, () => Domeniu(() => {
            using var os = Secured(typeof(ListaDiferenteInventar));
            ListaDiferenteInventarApply.Aplica(os, id, dto);
            return Ok(ListaDiferenteInventarApply.Citeste(os, id));
        }));

    // Ștergerea unui DRAFT. Pre-check-ul de domeniu e în `Sterge` (mesaj propriu),
    // gardianul de Committing rămâne plasa; curățenia loturilor născute la
    // culegerea plusurilor merge tot pe acolo.
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status404NotFound)]
    public IActionResult Delete(Guid id) =>
        ScriereAutorizata<ListaDiferenteInventar>(id, () => Domeniu(() => {
            using var os = Secured(typeof(ListaDiferenteInventar));
            ListaDiferenteInventarApply.Sterge(os, id);
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
    public IActionResult Valideaza(Guid id) => ComandaAutorizata<ListaDiferenteInventar>(id, () => Domeniu(() => {
        using var os = NonSecured(typeof(ListaDiferenteInventar));
        return Ok(EroriDto.Din(OperareApi.Valideaza(os, id)));
    }));

    IActionResult Comanda(Guid id, Func<IObjectSpace, OperareRezultat> comanda) =>
        ComandaAutorizata<ListaDiferenteInventar>(id, () => Domeniu(() => {
            using var os = NonSecured(typeof(ListaDiferenteInventar));
            return Ok(OperareRezultatDto.Din(comanda(os)));
        }));
}
