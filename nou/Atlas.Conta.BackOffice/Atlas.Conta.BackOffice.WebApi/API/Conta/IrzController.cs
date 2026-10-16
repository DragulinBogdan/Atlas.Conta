using Atlas.Conta.BackOffice.Module.Api;
using Atlas.Conta.BackOffice.Module.Api.Irz;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Proiectii;
using DevExpress.ExpressApp;
using Microsoft.AspNetCore.Mvc;

namespace Atlas.Conta.BackOffice.WebApi.API.Conta;

// Închiderea conturilor de rezultat (E3-D3), pe ușile ITV (F21-D3, F22-D5): cifrele motorului
// pe ușa non-secured după gate-ul securizat; generarea cere crearea pe tip.
[Route("api/irz")]
public class IrzController : ContaApiController {
    const int AnMinim = 2000, AnMaxim = 2100;

    public IrzController(IObjectSpaceFactory secured, INonSecuredObjectSpaceFactory nonSecured,
        DevExpress.ExpressApp.Security.ISecurityStrategyBase securitate)
        : base(secured, nonSecured, securitate) { }

    [HttpGet]
    [ProducesResponseType(typeof(PaginaDto<IrzListDto>), StatusCodes.Status200OK)]
    public object Get(DataSourceLoadOptions loadOptions) {
        using var os = Secured(typeof(InchidereRezultat));
        return Incarca(InchidereRezultatApply.Lista(os), loadOptions,
            OrdineLista.Descrescator("Data"), OrdineLista.Descrescator("Id"));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(IrzReadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status404NotFound)]
    public IActionResult GetById(Guid id) {
        var refuz = AutorizeazaCitire<InchidereRezultat>(id) ?? PostariCitibile();
        if (refuz != null)
            return refuz;
        using var os = NonSecured(typeof(InchidereRezultat));
        var dto = InchidereRezultatApply.Citeste(os, id);
        return dto == null ? Invizibil() : Ok(dto);
    }

    [HttpGet("previzualizare")]
    [ProducesResponseType(typeof(PrevizualizareIrzDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status403Forbidden)]
    public IActionResult Previzualizare([FromQuery] int? an = null, [FromQuery] int? luna = null) {
        var erori = Perioada(an, luna);
        if (erori.Count > 0)
            return BadRequest(EroriDto.Din(erori));
        using (var osSecured = Secured(typeof(InchidereRezultat)))
            if (!PoateCiti(typeof(InchidereRezultat), osSecured))
                return RefuzCitire(typeof(InchidereRezultat));
        return PostariCitibile() ?? Domeniu(() => {
            using var os = NonSecured(typeof(InchidereRezultat));
            return Ok(InchidereRezultatApply.Previzualizeaza(os, an.Value, luna.Value));
        });
    }

    [HttpPost("genereaza")]
    [ProducesResponseType(typeof(GenerareIrzRezultatDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult Genereaza([FromBody] GenerareIrzRequestDto cerere) =>
        CreareAutorizata<InchidereRezultat>(() => Domeniu(() => {
            using var os = NonSecured(typeof(InchidereRezultat));
            return Ok(InchidereRezultatApply.Genereaza(os, cerere));
        }));

    [HttpPost("{id:guid}/regenereaza")]
    [ProducesResponseType(typeof(GenerareIrzRezultatDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult Regenereaza(Guid id) =>
        ComandaAutorizata<InchidereRezultat>(id, () => CreareAutorizata<InchidereRezultat>(() => Domeniu(() => {
            using var os = NonSecured(typeof(InchidereRezultat));
            return Ok(InchidereRezultatApply.Regenereaza(os, id));
        })));

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult Delete(Guid id) =>
        ScriereAutorizata<InchidereRezultat>(id, () => Domeniu(() => {
            using var os = Secured(typeof(InchidereRezultat));
            InchidereRezultatApply.Sterge(os, id);
            return NoContent();
        }), OperatieAcces.Stergere);

    [HttpPost("{id:guid}/opereaza")]
    [ProducesResponseType(typeof(OperareRezultatDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult Opereaza(Guid id) => Comanda(c => c.Opereaza(id));

    [HttpPost("{id:guid}/anuleaza")]
    [ProducesResponseType(typeof(OperareRezultatDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult Anuleaza(Guid id) => Comanda(c => c.AnuleazaOperarea(id));

    [HttpPost("{id:guid}/storneaza")]
    [ProducesResponseType(typeof(OperareRezultatDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult Storneaza(Guid id, [FromBody] StornoRequestDto cerere) =>
        Comanda(c => c.Storneaza(id, cerere?.Data ?? DateOnly.FromDateTime(DateTime.Today)));

    [HttpPost("{id:guid}/valideaza")]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status404NotFound)]
    public IActionResult Valideaza(Guid id) => ComandaDocument<InchidereRezultat>(c => EroriDto.Din(c.Valideaza(id)));

    IActionResult Comanda(Func<ComenziDocument, OperareRezultat> comanda) =>
        ComandaDocument<InchidereRezultat>(c => OperareRezultatDto.Din(comanda(c)));

    static List<string> Perioada(int? an, int? luna) {
        var erori = new List<string>();
        if (an == null)
            erori.Add("Parametrul „an” este obligatoriu (anul lunii de închis).");
        else if (an < AnMinim || an > AnMaxim)
            erori.Add($"„an” trebuie să fie între {AnMinim} și {AnMaxim}.");
        if (luna == null)
            erori.Add("Parametrul „luna” este obligatoriu (luna de închis, 1–12).");
        else if (luna < 1 || luna > 12)
            erori.Add("„luna” trebuie să fie între 1 și 12 — închiderea conturilor de rezultat se face pe o lună.");
        return erori;
    }
}
