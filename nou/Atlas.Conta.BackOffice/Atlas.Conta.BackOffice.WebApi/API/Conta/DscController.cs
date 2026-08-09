using Atlas.Conta.BackOffice.Module.Api;
using Atlas.Conta.BackOffice.Module.Api.Dsc;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using DevExpress.ExpressApp;
using Microsoft.AspNetCore.Mvc;

namespace Atlas.Conta.BackOffice.WebApi.API.Conta;

// Felia DSC (F4-D2): CITIRE + COMENZI, fără agregat de scriere — exact ca NIR-ul
// la felia 2, și din același motiv. Descărcarea se NAȘTE exclusiv prin
// `DescarcareService`: automat, în tranzacția operării facturii, sau prin
// comanda de backorder `POST /api/fcl/{id}/genereaza-descarcare`. Liniile ei
// sunt rezultatul pickingului pe loturi, nu o culegere.
//
// POST/PUT/DELETE (descărcarea culeasă manual, azi acoperită de ecranul XAF) e
// felie separată, pur aditivă: se adaugă aici, fără să mute nimic.
[Route("api/dsc")]
public class DscController : ContaApiController {
    public DscController(IObjectSpaceFactory secured, INonSecuredObjectSpaceFactory nonSecured,
        DevExpress.ExpressApp.Security.ISecurityStrategyBase securitate)
        : base(secured, nonSecured, securitate) { }

    // ── Citire ────────────────────────────────────────────────────────────
    [HttpGet]
    [ProducesResponseType(typeof(PaginaDto<DscListDto>), StatusCodes.Status200OK)]
    public object Get(DataSourceLoadOptions loadOptions) {
        using var os = Secured(typeof(DescarcareGestiune));
        return Incarca(DscApply.Lista(os), loadOptions);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(DscReadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetById(Guid id) {
        using var os = Secured(typeof(DescarcareGestiune));
        var dto = DscApply.Citeste(os, id);
        return dto == null ? NotFound() : Ok(dto);
    }

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
        using var os = NonSecured(typeof(DescarcareGestiune));
        return Ok(EroriDto.Din(OperareApi.Valideaza(os, id)));
    }));

    IActionResult Comanda(Guid id, Func<IObjectSpace, OperareRezultat> comanda) =>
        ComandaAutorizata(id, () => Domeniu(() => {
            using var os = NonSecured(typeof(DescarcareGestiune));
            return Ok(OperareRezultatDto.Din(comanda(os)));
        }));
}
