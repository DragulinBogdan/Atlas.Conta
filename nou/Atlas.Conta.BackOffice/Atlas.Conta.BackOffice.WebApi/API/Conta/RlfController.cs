using Atlas.Conta.BackOffice.Module.Api;
using Atlas.Conta.BackOffice.Module.Api.Rlf;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using DevExpress.ExpressApp;
using Microsoft.AspNetCore.Mvc;

namespace Atlas.Conta.BackOffice.WebApi.API.Conta;

// Felia RLF (F19, track 3): returul la furnizor — CRUD + comenzi, pe șablonul
// consolidat al feliilor F2–F8. Transport pur: regulile sunt în
// `ReturFurnizorApply` (Module), exersate din ModelCheck pe același cod.
//
// Nicio comandă proprie: retururile NU sunt stingători (F19-D11), deci n-au
// panou de stingeri, iar valoarea ieșirii rămâne integral a motorului (F18/F5).
[Route("api/rlf")]
public class RlfController : ContaApiController {
    public RlfController(IObjectSpaceFactory secured, INonSecuredObjectSpaceFactory nonSecured,
        DevExpress.ExpressApp.Security.ISecurityStrategyBase securitate)
        : base(secured, nonSecured, securitate) { }

    // ── Citire ────────────────────────────────────────────────────────────
    [HttpGet]
    [ProducesResponseType(typeof(PaginaDto<RlfListDto>), StatusCodes.Status200OK)]
    public object Get(DataSourceLoadOptions loadOptions) {
        using var os = Secured(typeof(ReturFurnizor));
        return Incarca(ReturFurnizorApply.Lista(os), loadOptions);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(RlfReadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetById(Guid id) {
        using var os = Secured(typeof(ReturFurnizor));
        var dto = ReturFurnizorApply.Citeste(os, id);
        return dto == null ? NotFound() : Ok(dto);
    }

    // ── Scriere: agregatul per document (42d) ─────────────────────────────
    // `Numar` nu apare în WriteDto: RLF are politică de numerotare („RLF-") în
    // profilul privat, deci seria e server-owned și se consumă la operare
    // (F19-D6). Nici `Valoare` — o materializează `Aplica` din prețul lotului.
    [HttpPost]
    [ProducesResponseType(typeof(RlfReadDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult Post([FromBody] RlfWriteDto dto) => Domeniu(() => {
        using var os = Secured(typeof(ReturFurnizor));
        var id = ReturFurnizorApply.Aplica(os, null, dto);
        return Created($"/api/rlf/{id}", ReturFurnizorApply.Citeste(os, id));
    });

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(RlfReadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult Put(Guid id, [FromBody] RlfWriteDto dto) => Domeniu(() => {
        using var os = Secured(typeof(ReturFurnizor));
        ReturFurnizorApply.Aplica(os, id, dto);
        return Ok(ReturFurnizorApply.Citeste(os, id));
    });

    // Ștergerea unui DRAFT. Pre-check-ul de domeniu e în `Sterge` (mesaj propriu),
    // gardianul de Committing rămâne plasa.
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult Delete(Guid id) => Domeniu(() => {
        using var os = Secured(typeof(ReturFurnizor));
        if (os.GetObjectByKey<ReturFurnizor>(id) == null)
            return NotFound();
        ReturFurnizorApply.Sterge(os, id);
        return NoContent();
    });

    // ── Comenzi: OS NON-SECURED, tranzacția integral a motorului (42b) ─────
    [HttpPost("{id:guid}/opereaza")]
    [ProducesResponseType(typeof(OperareRezultatDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult Opereaza(Guid id) => Comanda(id, os => OperareApi.Opereaza(os, id));

    [HttpPost("{id:guid}/anuleaza")]
    [ProducesResponseType(typeof(OperareRezultatDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult Anuleaza(Guid id) => Comanda(id, os => OperareApi.AnuleazaOperarea(os, id));

    [HttpPost("{id:guid}/storneaza")]
    [ProducesResponseType(typeof(OperareRezultatDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult Storneaza(Guid id, [FromBody] StornoRequestDto cerere) =>
        Comanda(id, os => OperareApi.Storneaza(os, id, cerere?.Data ?? DateOnly.FromDateTime(DateTime.Today)));

    [HttpPost("{id:guid}/valideaza")]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status200OK)]
    public IActionResult Valideaza(Guid id) => ComandaAutorizata(id, () => Domeniu(() => {
        using var os = NonSecured(typeof(ReturFurnizor));
        return Ok(EroriDto.Din(OperareApi.Valideaza(os, id)));
    }));

    IActionResult Comanda(Guid id, Func<IObjectSpace, OperareRezultat> comanda) =>
        ComandaAutorizata(id, () => Domeniu(() => {
            using var os = NonSecured(typeof(ReturFurnizor));
            return Ok(OperareRezultatDto.Din(comanda(os)));
        }));
}
