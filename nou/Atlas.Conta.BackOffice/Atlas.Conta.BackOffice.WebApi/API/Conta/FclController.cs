using Atlas.Conta.BackOffice.Module.Api;
using Atlas.Conta.BackOffice.Module.Api.Fcl;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using DevExpress.ExpressApp;
using Microsoft.AspNetCore.Mvc;

namespace Atlas.Conta.BackOffice.WebApi.API.Conta;

// Felia verticală FCL (contract p5-felia-fcl, pasul 3) — același șablon ca
// `FacturaIntrareController`: transport și atât. Regulile (reconcilierea
// agregatului, TVA-ul la culegere, pinul de lot, generatorul de descărcare)
// stau în `FacturaIesireApply`/`DescarcareService`/motor — o singură sursă
// pentru toate tierele (42a).
//
// Diferențele de contract față de FCT sunt în DTO și în cele două endpoint-uri
// de backorder de mai jos: `Numar` NU se culege (FCL are politică de numerotare
// — serie fiscală, server-owned), iar `Sterge` n-are curățenie de loturi (FCL
// referă loturi, nu le naște).
[Route("api/fcl")]
public class FclController : ContaApiController {
    public FclController(IObjectSpaceFactory secured, INonSecuredObjectSpaceFactory nonSecured,
        DevExpress.ExpressApp.Security.ISecurityStrategyBase securitate)
        : base(secured, nonSecured, securitate) { }

    // ── Citire ────────────────────────────────────────────────────────────
    // Grila e REMOTE (43c): filtrare/sortare/paginare vin în query string,
    // `DataSourceLoader` le pune peste `IQueryable`, SQL-ul le execută.
    [HttpGet]
    [ProducesResponseType(typeof(PaginaDto<FacturaIesireListDto>), StatusCodes.Status200OK)]
    public object Get(DataSourceLoadOptions loadOptions) {
        using var os = Secured(typeof(FacturaIesire));
        return Incarca(FacturaIesireApply.Lista(os), loadOptions);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(FacturaIesireReadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status404NotFound)]
    public IActionResult GetById(Guid id) {
        using var os = Secured(typeof(FacturaIesire));
        var dto = FacturaIesireApply.Citeste(os, id);
        return dto == null ? Invizibil() : Ok(dto);
    }

    // ── Scriere: agregatul per document (42d) ─────────────────────────────
    [HttpPost]
    [ProducesResponseType(typeof(FacturaIesireReadDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status403Forbidden)]
    public IActionResult Post([FromBody] FacturaIesireWriteDto dto) =>
        CreareAutorizata<FacturaIesire>(() => Domeniu(() => {
            using var os = Secured(typeof(FacturaIesire));
            var id = FacturaIesireApply.Aplica(os, null, dto);
            return Created($"/api/fcl/{id}", FacturaIesireApply.Citeste(os, id));
        }));

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(FacturaIesireReadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status403Forbidden)]
    public IActionResult Put(Guid id, [FromBody] FacturaIesireWriteDto dto) =>
        ScriereAutorizata<FacturaIesire>(id, () => Domeniu(() => {
            using var os = Secured(typeof(FacturaIesire));
            FacturaIesireApply.Aplica(os, id, dto);
            return Ok(FacturaIesireApply.Citeste(os, id));
        }));

    // Ștergerea unui DRAFT: pre-check-ul de Draft e în `Apply.Sterge` (mesaj de
    // domeniu ⇒ 422). Existența se verifică o dată aici, ca documentul
    // inexistent să iasă 404, nu 422 (ca la FCT).
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status403Forbidden)]
    public IActionResult Delete(Guid id) =>
        ScriereAutorizata<FacturaIesire>(id, () => Domeniu(() => {
            using var os = Secured(typeof(FacturaIesire));
            FacturaIesireApply.Sterge(os, id);
            return NoContent();
        }), OperatieAcces.Stergere);

    // ── Comenzi: OS NON-SECURED, tranzacția integral a motorului (42b) ─────
    // `ComandaAutorizata` decide CINE are voie (404 pe inexistent/invizibil, 403
    // fără Write) înainte ca ușa non-secured să se deschidă.
    // Pe FCL operarea întoarce și `ConexId` — descărcarea de gestiune generată
    // în aceeași tranzacție (`GenereazaSecundar`), pe care clientul o deschide
    // pe `/dsc/{id}`.
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

    // Dry-run (D3): „calculează + validează" fără materializare și fără commit.
    [HttpPost("{id:guid}/valideaza")]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status404NotFound)]
    public IActionResult Valideaza(Guid id) => ComandaAutorizata<FacturaIesire>(id, () => Domeniu(() => {
        using var os = NonSecured(typeof(FacturaIesire));
        return Ok(EroriDto.Din(OperareApi.Valideaza(os, id)));
    }));

    // ── Backorder (F4-D3/D4) ──────────────────────────────────────────────
    //
    // Generarea MANUALĂ a descărcării pentru restul neacoperit — geamănul
    // acțiunii XAF „Generează descărcarea". Ușa e NON-SECURED, ca la orice
    // comandă: `DescarcareService` scrie `Autogenerat` și `DocumentSursa`,
    // câmpuri SERVER-OWNED pe care `GardianEditare` le refuză pe orice cale
    // secured. Autorizarea („cine comandă") rămâne a controllerului, prin
    // `ComandaAutorizata`, ÎNAINTEA ușii.
    // `DscId` null în răspuns nu e eroare: backorder-ul rămas e starea normală
    // a unei comenzi nelivrate încă.
    [HttpPost("{id:guid}/genereaza-descarcare")]
    [ProducesResponseType(typeof(GenerareDescarcareRezultatDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status404NotFound)]
    public IActionResult GenereazaDescarcare(Guid id, [FromBody] GenerareDescarcareRequestDto cerere) =>
        ComandaAutorizata<FacturaIesire>(id, () => Domeniu(() => {
            using var os = NonSecured(typeof(FacturaIesire));
            var data = cerere?.Data ?? DateOnly.FromDateTime(DateTime.Today);
            return Ok(FacturaIesireApply.GenereazaDescarcare(os, id, data));
        }));

    // Acoperirea per linie de stoc — CITIRE, deci ușa secured. Documentul
    // inexistent se verifică aici (ca la Delete), ca să iasă 404 în loc de
    // mesajul de domeniu al Apply-ului (422).
    [HttpGet("{id:guid}/rest-nedescarcat")]
    [ProducesResponseType(typeof(IReadOnlyList<RestNedescarcatRandDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult RestNedescarcat(Guid id) => Domeniu(() => {
        using var os = Secured(typeof(FacturaIesire));
        if (os.GetObjectByKey<FacturaIesire>(id) == null)
            return Invizibil();
        return Ok(FacturaIesireApply.RestNedescarcat(os, id));
    });

    IActionResult Comanda(Guid id, Func<IObjectSpace, OperareRezultat> comanda) =>
        ComandaAutorizata<FacturaIesire>(id, () => Domeniu(() => {
            using var os = NonSecured(typeof(FacturaIesire));
            return Ok(OperareRezultatDto.Din(comanda(os)));
        }));
}
