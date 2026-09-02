using Atlas.Conta.BackOffice.Module.Api;
using Atlas.Conta.BackOffice.Module.Api.Ntc;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using DevExpress.ExpressApp;
using Microsoft.AspNetCore.Mvc;

namespace Atlas.Conta.BackOffice.WebApi.API.Conta;

// Felia NTC (F19): nota contabilă — CRUD + comenzi + panoul de compensare, pe
// șablonul consolidat al feliilor F2–F8. Transport pur: regulile sunt în
// `NotaContabilaApply` (Module), exersate din ModelCheck pe același cod.
//
// ═══ Nota e cea mai puternică ușă de scriere din sistem (riscul 1) ═══
// `IDocumentCuPostareExplicita` face ca o linie cu postare COMPLETĂ să posteze
// în ABSENȚA oricărei reguli de contare — adică prin ruta asta se poate scrie
// orice corespondență contabilă. Ce stă între ea și note arbitrare, în ordine:
//   1. `[Authorize]` + securitatea XAF pe ușa SECURED (`Domeniu` + `Secured`):
//      culegerea trece prin `IObjectSpaceFactory`, deci CREATE/WRITE pe
//      `NotaContabila`/`NotaContabilaDetaliu` cer permisiune de TIP. Rolul
//      `Default` (userul „User") nu primește niciun drept pe documente ⇒ nota îi
//      e invizibilă la citire și nescriibilă la commit.
//   2. `ComandaAutorizata` (55b) pe comenzi: documentul se rezolvă printr-un OS
//      SECURED — invizibil ⇒ 404, vizibil fără Write ⇒ 403 — ÎNAINTE ca ușa
//      non-secured a motorului să se deschidă.
//   3. `GardianEditare` la Committing: `Stare`/`Numar`/`DataOperare`/
//      `Autogenerat`/`DocumentSursaId` rămân server-owned, registrele nu se
//      scriu direct, iar un document care nu mai e Draft nu se mai modifică.
//   4. `Cont.DimensiuniObligatorii` la OPERARE (33a): postarea explicită NU
//      scutește linia de defalcarea cerută de contul ales, pe fiecare latură.
// Ce NU stă între ele: nicio regulă de „corespondență permisă" — un cont poate
// fi pus în corespondență cu oricare altul. E trăsătura tipului (48b/46b), nu o
// scăpare a feliei: nota contabilă manuală asta ESTE.
[Route("api/ntc")]
public class NtcController : ContaApiController {
    public NtcController(IObjectSpaceFactory secured, INonSecuredObjectSpaceFactory nonSecured,
        DevExpress.ExpressApp.Security.ISecurityStrategyBase securitate)
        : base(secured, nonSecured, securitate) { }

    // ── Citire ────────────────────────────────────────────────────────────
    [HttpGet]
    [ProducesResponseType(typeof(PaginaDto<NtcListDto>), StatusCodes.Status200OK)]
    public object Get(DataSourceLoadOptions loadOptions) {
        using var os = Secured(typeof(NotaContabila));
        return Incarca(NotaContabilaApply.Lista(os), loadOptions);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(NtcReadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status404NotFound)]
    public IActionResult GetById(Guid id) {
        using var os = Secured(typeof(NotaContabila));
        var dto = NotaContabilaApply.Citeste(os, id);
        return dto == null ? Invizibil() : Ok(dto);
    }

    // Candidații de compensare, grupați pe CONTRAPARTIDĂ (F19-D10). CITIRE, deci
    // ușa SECURED: rândurile invizibile pentru utilizator cad natural din
    // proiecție. Nota NU apare printre candidații ei (nu e în `DocumenteCuRest`).
    [HttpGet("{id:guid}/candidati")]
    [ProducesResponseType(typeof(NtcCandidatiDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status404NotFound)]
    public IActionResult Candidati(Guid id) {
        using var os = Secured(typeof(NotaContabila));
        var dto = NotaContabilaApply.Candidati(os, id);
        return dto == null ? Invizibil() : Ok(dto);
    }

    // ── Scriere: agregatul per document (42d) ─────────────────────────────
    // `Numar` nu apare în WriteDto: NTC are politică de numerotare („NTC-") în
    // AMBELE profiluri, deci seria e server-owned și se consumă la operare
    // (F19-D6).
    [HttpPost]
    [ProducesResponseType(typeof(NtcReadDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status403Forbidden)]
    public IActionResult Post([FromBody] NtcWriteDto dto) =>
        CreareAutorizata<NotaContabila>(() => Domeniu(() => {
            using var os = Secured(typeof(NotaContabila));
            var id = NotaContabilaApply.Aplica(os, null, dto);
            return Created($"/api/ntc/{id}", NotaContabilaApply.Citeste(os, id));
        }));

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(NtcReadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status404NotFound)]
    public IActionResult Put(Guid id, [FromBody] NtcWriteDto dto) =>
        ScriereAutorizata<NotaContabila>(id, () => Domeniu(() => {
            using var os = Secured(typeof(NotaContabila));
            NotaContabilaApply.Aplica(os, id, dto);
            return Ok(NotaContabilaApply.Citeste(os, id));
        }));

    // Ștergerea unui DRAFT. Pre-check-ul de domeniu e în `Sterge` (mesaj propriu),
    // gardianul de Committing rămâne plasa.
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status404NotFound)]
    public IActionResult Delete(Guid id) =>
        ScriereAutorizata<NotaContabila>(id, () => Domeniu(() => {
            using var os = Secured(typeof(NotaContabila));
            NotaContabilaApply.Sterge(os, id);
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
    public IActionResult Valideaza(Guid id) => ComandaAutorizata<NotaContabila>(id, () => Domeniu(() => {
        using var os = NonSecured(typeof(NotaContabila));
        return Ok(EroriDto.Din(OperareApi.Valideaza(os, id)));
    }));

    IActionResult Comanda(Guid id, Func<IObjectSpace, OperareRezultat> comanda) =>
        ComandaAutorizata<NotaContabila>(id, () => Domeniu(() => {
            using var os = NonSecured(typeof(NotaContabila));
            return Ok(OperareRezultatDto.Din(comanda(os)));
        }));
}
