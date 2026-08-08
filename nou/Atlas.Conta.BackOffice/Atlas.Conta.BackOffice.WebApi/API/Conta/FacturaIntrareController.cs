using Atlas.Conta.BackOffice.Module.Api;
using Atlas.Conta.BackOffice.Module.Api.Fct;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using DevExpress.ExpressApp;
using Microsoft.AspNetCore.Mvc;

namespace Atlas.Conta.BackOffice.WebApi.API.Conta;

// Felia verticală FCT (contract p5-felia-fct, pasul 3) — același șablon ca
// `NotaTransferController`: transport și atât. Tot ce e regulă (culegerea
// loturilor, TVA-ul la culegere, reconcilierea agregatului, gardienii) stă în
// `FacturaIntrareApply`/motor, o singură sursă pentru toate tierele (42a).
//
// Diferența de contract față de BTR e în DTO, nu aici: `Numar` e CULES (FCT
// poartă numărul furnizorului — n-are politică de numerotare), iar ștergerea
// deleagă la `Apply.Sterge` fiindcă are de făcut și curățenia loturilor născute
// la culegere (F2-D1) — pe BTR ștergerea era pură.
[Route("api/fct")]
public class FacturaIntrareController : ContaApiController {
    public FacturaIntrareController(IObjectSpaceFactory secured, INonSecuredObjectSpaceFactory nonSecured,
        DevExpress.ExpressApp.Security.ISecurityStrategyBase securitate)
        : base(secured, nonSecured, securitate) { }

    // ── Citire ────────────────────────────────────────────────────────────
    // Grila e REMOTE (43c): filtrare/sortare/paginare vin în query string,
    // `DataSourceLoader` le pune peste `IQueryable`, SQL-ul le execută.
    [HttpGet]
    [ProducesResponseType(typeof(PaginaDto<FacturaIntrareListDto>), StatusCodes.Status200OK)]
    public object Get(DataSourceLoadOptions loadOptions) {
        using var os = Secured(typeof(FacturaIntrare));
        return Incarca(FacturaIntrareApply.Lista(os), loadOptions);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(FacturaIntrareReadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetById(Guid id) {
        using var os = Secured(typeof(FacturaIntrare));
        var dto = FacturaIntrareApply.Citeste(os, id);
        return dto == null ? NotFound() : Ok(dto);
    }

    // ── Scriere: agregatul per document (42d) ─────────────────────────────
    // `Aplica` face maparea + default-ul de TipTva + calculul de valori +
    // sincronizarea loturilor + commit-ul; endpoint-ul doar traduce rezultatul.
    [HttpPost]
    [ProducesResponseType(typeof(FacturaIntrareReadDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult Post([FromBody] FacturaIntrareWriteDto dto) => Domeniu(() => {
        using var os = Secured(typeof(FacturaIntrare));
        var id = FacturaIntrareApply.Aplica(os, null, dto);
        return Created($"/api/fct/{id}", FacturaIntrareApply.Citeste(os, id));
    });

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(FacturaIntrareReadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Put(Guid id, [FromBody] FacturaIntrareWriteDto dto) => Domeniu(() => {
        using var os = Secured(typeof(FacturaIntrare));
        FacturaIntrareApply.Aplica(os, id, dto);
        return Ok(FacturaIntrareApply.Citeste(os, id));
    });

    // Ștergerea unui DRAFT. Spre deosebire de BTR, corpul NU se scrie aici:
    // `Apply.Sterge` face pre-check-ul de Draft (mesaj de domeniu) ȘI curățenia
    // loturilor născute la culegere — aceeași logică pe care o folosește
    // ModelCheck. 404 doar pentru documentul inexistent din alt tip / alt ID:
    // mesajul „nu există" al Apply-ului ar fi ieșit ca 422, deci existența se
    // verifică o dată, aici, ca statusul să fie cel corect.
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult Delete(Guid id) => Domeniu(() => {
        using var os = Secured(typeof(FacturaIntrare));
        if (os.GetObjectByKey<FacturaIntrare>(id) == null)
            return NotFound();
        FacturaIntrareApply.Sterge(os, id);
        return NoContent();
    });

    // ── Comenzi: OS NON-SECURED, tranzacția integral a motorului (42b) ─────
    // `ComandaAutorizata` decide CINE are voie (404 pe inexistent/invizibil, 403
    // fără Write) înainte ca ușa non-secured să se deschidă.
    // Pe FCT operarea întoarce și `ConexId` — NIR-ul generat în aceeași
    // tranzacție (F2-D3: clientul îl deschide pe `/nir/{id}`).
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

    // Dry-run (D3): „calculează + validează" fără materializare și fără commit —
    // ObjectSpace-ul e propriu și se ARUNCĂ. 200 cu erori nu e un eșec de
    // cerere, e răspunsul întrebării.
    [HttpPost("{id:guid}/valideaza")]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status200OK)]
    public IActionResult Valideaza(Guid id) => ComandaAutorizata(id, () => Domeniu(() => {
        using var os = NonSecured(typeof(FacturaIntrare));
        return Ok(EroriDto.Din(OperareApi.Valideaza(os, id)));
    }));

    IActionResult Comanda(Guid id, Func<IObjectSpace, OperareRezultat> comanda) =>
        ComandaAutorizata(id, () => Domeniu(() => {
            using var os = NonSecured(typeof(FacturaIntrare));
            return Ok(OperareRezultatDto.Din(comanda(os)));
        }));
}
