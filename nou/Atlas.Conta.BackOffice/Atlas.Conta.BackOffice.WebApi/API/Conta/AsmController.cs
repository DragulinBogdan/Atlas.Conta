using Atlas.Conta.BackOffice.Module.Api;
using Atlas.Conta.BackOffice.Module.Api.Asm;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using DevExpress.ExpressApp;
using Microsoft.AspNetCore.Mvc;

namespace Atlas.Conta.BackOffice.WebApi.API.Conta;

// Felia ASM (F19, track 2): asamblarea/kitting-ul — CRUD + comenzi + comanda
// proprie `distribuie-valoarea`. Transport pur: regulile sunt în
// `AsamblareApply` (Module), exersate din ModelCheck pe același cod. Culegerea
// bidirecțională (produs care naște lot / consum care descarcă unul) e integral
// acolo, inclusiv seam-ul `LoturiCulegereService`.
[Route("api/asm")]
public class AsmController : ContaApiController {
    public AsmController(IObjectSpaceFactory secured, INonSecuredObjectSpaceFactory nonSecured,
        DevExpress.ExpressApp.Security.ISecurityStrategyBase securitate)
        : base(secured, nonSecured, securitate) { }

    // ── Citire ────────────────────────────────────────────────────────────
    [HttpGet]
    [ProducesResponseType(typeof(PaginaDto<AsmListDto>), StatusCodes.Status200OK)]
    public object Get(DataSourceLoadOptions loadOptions) {
        using var os = Secured(typeof(Asamblare));
        return Incarca(AsamblareApply.Lista(os), loadOptions);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AsmReadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetById(Guid id) {
        using var os = Secured(typeof(Asamblare));
        var dto = AsamblareApply.Citeste(os, id);
        return dto == null ? NotFound() : Ok(dto);
    }

    // ── Scriere: agregatul per document (42d) ─────────────────────────────
    // `Numar` nu apare în WriteDto: ASM are politică de numerotare („ASM-") în
    // profilul privat, deci seria e server-owned și se consumă la operare
    // (F19-D6). Nici `LotId` pe liniile de produs — lotul se naște la culegere,
    // prin serviciu (F19-D3). Nici TVA (F19-D7: ASM n-are `PoliticaTva`).
    [HttpPost]
    [ProducesResponseType(typeof(AsmReadDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult Post([FromBody] AsmWriteDto dto) => Domeniu(() => {
        using var os = Secured(typeof(Asamblare));
        var id = AsamblareApply.Aplica(os, null, dto);
        return Created($"/api/asm/{id}", AsamblareApply.Citeste(os, id));
    });

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(AsmReadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult Put(Guid id, [FromBody] AsmWriteDto dto) => Domeniu(() => {
        using var os = Secured(typeof(Asamblare));
        AsamblareApply.Aplica(os, id, dto);
        return Ok(AsamblareApply.Citeste(os, id));
    });

    // Ștergerea unui DRAFT. Pre-check-ul de domeniu e în `Sterge` (mesaj propriu),
    // gardianul de Committing rămâne plasa.
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult Delete(Guid id) => Domeniu(() => {
        using var os = Secured(typeof(Asamblare));
        if (os.GetObjectByKey<Asamblare>(id) == null)
            return NotFound();
        AsamblareApply.Sterge(os, id);
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
        using var os = NonSecured(typeof(Asamblare));
        return Ok(EroriDto.Din(OperareApi.Valideaza(os, id)));
    }));

    // ── Comanda proprie feliei: distribuirea valorii consumului (F19-D4) ───
    // Închide 75-r1. Rescrie `PretEvaluare` pe liniile de produs ca invariantul
    // 46d să treacă EXACT, prezicând valoarea consumului prin dry-run-ul
    // motorului (deci și regula golirii D18-D2).
    //
    // Ușa: NON-SECURED, cu gate-ul de autorizare ÎNAINTE (55b) — comanda scrie
    // prin serviciu, nu prin culegere (58c). DOUĂ ObjectSpace-uri non-secured,
    // deliberat: unul al comenzii (scrie și comite), altul de UNICĂ FOLOSINȚĂ
    // pentru dry-run — `MotorOperare.Valideaza` SEMNEAZĂ cantitățile pe linii,
    // deci nu are voie să atingă ObjectSpace-ul care comite.
    //
    // Răspunsul se RECITEȘTE pe ușa securizată: comanda a lucrat pe ușa de
    // sistem, dar ce vede clientul rămâne ce are voie să vadă.
    [HttpPost("{id:guid}/distribuie-valoarea")]
    [ProducesResponseType(typeof(AsmDistribuireDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult DistribuieValoarea(Guid id) => ComandaAutorizata(id, () => Domeniu(() => {
        AsmDistribuireDto rezultat;
        using (var os = NonSecured(typeof(Asamblare)))
            rezultat = AsamblareApply.DistribuieValoarea(os, () => NonSecured(typeof(Asamblare)), id);
        using var osCitire = Secured(typeof(Asamblare));
        rezultat.Document = AsamblareApply.Citeste(osCitire, id);
        return Ok(rezultat);
    }));

    IActionResult Comanda(Guid id, Func<IObjectSpace, OperareRezultat> comanda) =>
        ComandaAutorizata(id, () => Domeniu(() => {
            using var os = NonSecured(typeof(Asamblare));
            return Ok(OperareRezultatDto.Din(comanda(os)));
        }));
}
