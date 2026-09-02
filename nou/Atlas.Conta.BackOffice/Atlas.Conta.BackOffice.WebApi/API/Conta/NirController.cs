using Atlas.Conta.BackOffice.Module.Api;
using Atlas.Conta.BackOffice.Module.Api.Nir;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using DevExpress.ExpressApp;
using Microsoft.AspNetCore.Mvc;

namespace Atlas.Conta.BackOffice.WebApi.API.Conta;

// Felia NIR: CITIRE + COMENZI (F2-D3) + SCRIERE (F5-D8). Cele două fluxuri ale
// recepției intră pe aceleași rute:
//   * CONEX — FCT operată → `ConexId` → `/nir/{id}` → Operează: liniile vin din
//     clona pe care motorul o generează în tranzacția operării facturii, iar
//     PUT-ul le poate corecta (recepția PARȚIALĂ: cantitatea primită scade,
//     valoarea o urmează din prețul lotului);
//   * MANUAL — marfa intră pe aviz/bon: POST/PUT culeg documentul, iar loturile
//     se nasc pe propriile linii (`LoturiCulegereService`, apelat din Apply —
//     pe tierul ăsta nu rulează niciun ViewController).
// Scrierea s-a ADĂUGAT fără să mute nimic, exact cum anunța excluderea F2-D3.
//
// Comenzile sunt identice cu ale oricărei felii: `OperareApi` e agnostic de tip,
// iar gate-ul de autorizare vine din `ContaApiController`.
[Route("api/nir")]
public class NirController : ContaApiController {
    public NirController(IObjectSpaceFactory secured, INonSecuredObjectSpaceFactory nonSecured,
        DevExpress.ExpressApp.Security.ISecurityStrategyBase securitate)
        : base(secured, nonSecured, securitate) { }

    // ── Citire ────────────────────────────────────────────────────────────
    [HttpGet]
    [ProducesResponseType(typeof(PaginaDto<NirListDto>), StatusCodes.Status200OK)]
    public object Get(DataSourceLoadOptions loadOptions) {
        using var os = Secured(typeof(NIR));
        return Incarca(NirApply.Lista(os), loadOptions);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(NirReadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status404NotFound)]
    public IActionResult GetById(Guid id) {
        using var os = Secured(typeof(NIR));
        var dto = NirApply.Citeste(os, id);
        return dto == null ? Invizibil() : Ok(dto);
    }

    // ── Scriere: agregatul per document (42d) ─────────────────────────────
    // `Numar` nu apare în WriteDto: NIR are politică de numerotare, deci seria e
    // server-owned (F5-D8) — invers față de FCT, unde numărul e al furnizorului.
    [HttpPost]
    [ProducesResponseType(typeof(NirReadDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status403Forbidden)]
    public IActionResult Post([FromBody] NirWriteDto dto) =>
        CreareAutorizata<NIR>(() => Domeniu(() => {
            using var os = Secured(typeof(NIR));
            var id = NirApply.Aplica(os, null, dto);
            return Created($"/api/nir/{id}", NirApply.Citeste(os, id));
        }));

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(NirReadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status404NotFound)]
    public IActionResult Put(Guid id, [FromBody] NirWriteDto dto) =>
        ScriereAutorizata<NIR>(id, () => Domeniu(() => {
            using var os = Secured(typeof(NIR));
            NirApply.Aplica(os, id, dto);
            return Ok(NirApply.Citeste(os, id));
        }));

    // Ștergerea unui DRAFT. Pre-check-ul de domeniu e în `Sterge` (mesaj propriu),
    // gardianul de Committing rămâne plasa; curățenia loturilor născute la
    // culegere merge tot pe acolo.
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status404NotFound)]
    public IActionResult Delete(Guid id) =>
        ScriereAutorizata<NIR>(id, () => Domeniu(() => {
            using var os = Secured(typeof(NIR));
            NirApply.Sterge(os, id);
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
    public IActionResult Valideaza(Guid id) => ComandaAutorizata<NIR>(id, () => Domeniu(() => {
        using var os = NonSecured(typeof(NIR));
        return Ok(EroriDto.Din(OperareApi.Valideaza(os, id)));
    }));

    IActionResult Comanda(Guid id, Func<IObjectSpace, OperareRezultat> comanda) =>
        ComandaAutorizata<NIR>(id, () => Domeniu(() => {
            using var os = NonSecured(typeof(NIR));
            return Ok(OperareRezultatDto.Din(comanda(os)));
        }));
}
