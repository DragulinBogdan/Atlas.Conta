using Atlas.Conta.BackOffice.Module.Api;
using Atlas.Conta.BackOffice.Module.Api.Btr;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using DevExpress.ExpressApp;
using Microsoft.AspNetCore.Mvc;

namespace Atlas.Conta.BackOffice.WebApi.API.Conta;

// Felia verticală BTR (D1/D8) — endpoint-uri PER TIP DE DOCUMENT (decizia 6:
// API-ul nu expune ierarhia polimorf). Controllerul e subțire prin construcție:
// citirea deleagă la proiecțiile din `NotaTransferApply`, scrierea la
// reconcilierea agregatului, comenzile la `OperareApi`. Fiecare linie de mai jos
// e transport sau traducere de erori.
[Route("api/btr")]
public class NotaTransferController : ContaApiController {
    public NotaTransferController(IObjectSpaceFactory secured, INonSecuredObjectSpaceFactory nonSecured)
        : base(secured, nonSecured) { }

    // ── Citire ────────────────────────────────────────────────────────────
    // Grila e REMOTE (43c): filtrarea/sortarea/paginarea vin în query string,
    // `DataSourceLoader` le pune peste `IQueryable` și SQL-ul le execută
    // server-side. Nimic nu se materializează în afara paginii cerute.
    [HttpGet]
    [ProducesResponseType(typeof(PaginaDto<NotaTransferListDto>), StatusCodes.Status200OK)]
    public object Get(DataSourceLoadOptions loadOptions) {
        using var os = Secured(typeof(NotaTransfer));
        return Incarca(NotaTransferApply.Lista(os), loadOptions);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(NotaTransferReadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetById(Guid id) {
        using var os = Secured(typeof(NotaTransfer));
        var dto = NotaTransferApply.Citeste(os, id);
        return dto == null ? NotFound() : Ok(dto);
    }

    // ── Scriere: agregatul per document (42d) ─────────────────────────────
    [HttpPost]
    [ProducesResponseType(typeof(NotaTransferReadDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult Post([FromBody] NotaTransferWriteDto dto) => Domeniu(() => {
        using var os = Secured(typeof(NotaTransfer));
        var id = NotaTransferApply.Aplica(os, null, dto);
        return Created($"/api/btr/{id}", NotaTransferApply.Citeste(os, id));
    });

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(NotaTransferReadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult Put(Guid id, [FromBody] NotaTransferWriteDto dto) => Domeniu(() => {
        using var os = Secured(typeof(NotaTransfer));
        NotaTransferApply.Aplica(os, id, dto);
        return Ok(NotaTransferApply.Citeste(os, id));
    });

    // Ștergerea unui DRAFT. Nu există pre-check aici: gardianul de Committing
    // refuză ștergerea oricărui document ne-Draft (D4, aceeași regulă pe toate
    // tierele), iar refuzul lui iese pe aceeași cale 422.
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult Delete(Guid id) => Domeniu(() => {
        using var os = Secured(typeof(NotaTransfer));
        var doc = os.GetObjectByKey<NotaTransfer>(id);
        if (doc == null)
            return NotFound();
        os.Delete(doc.Detalii.ToList());
        os.Delete(doc);
        os.CommitChanges();
        return NoContent();
    });

    // ── Comenzi: OS NON-SECURED, tranzacția integral a motorului (42b) ─────
    [HttpPost("{id:guid}/opereaza")]
    [ProducesResponseType(typeof(OperareRezultatDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult Opereaza(Guid id) => Comanda(os => OperareApi.Opereaza(os, id));

    [HttpPost("{id:guid}/anuleaza")]
    [ProducesResponseType(typeof(OperareRezultatDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult Anuleaza(Guid id) => Comanda(os => OperareApi.AnuleazaOperarea(os, id));

    [HttpPost("{id:guid}/storneaza")]
    [ProducesResponseType(typeof(OperareRezultatDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult Storneaza(Guid id, [FromBody] StornoRequestDto cerere) =>
        Comanda(os => OperareApi.Storneaza(os, id, cerere?.Data ?? DateOnly.FromDateTime(DateTime.Today)));

    // Dry-run (D3): fazele „calculează + validează" ale operării, fără
    // materializare și fără commit — ObjectSpace-ul e PROPRIU și se ARUNCĂ
    // (`Valideaza` scrie pe linii prin `PregatesteOperare`, dar nimic nu se
    // comite). 200 cu listă goală = documentul trece toți gardienii; 200 cu
    // erori NU e un eșec de cerere, e răspunsul întrebării.
    [HttpPost("{id:guid}/valideaza")]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status200OK)]
    public IActionResult Valideaza(Guid id) => Domeniu(() => {
        using var os = NonSecured(typeof(NotaTransfer));
        return Ok(EroriDto.Din(OperareApi.Valideaza(os, id)));
    });

    IActionResult Comanda(Func<IObjectSpace, OperareRezultat> comanda) => Domeniu(() => {
        using var os = NonSecured(typeof(NotaTransfer));
        return Ok(OperareRezultatDto.Din(comanda(os)));
    });
}
