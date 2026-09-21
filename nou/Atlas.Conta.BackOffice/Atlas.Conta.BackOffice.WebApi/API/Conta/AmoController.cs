using Atlas.Conta.BackOffice.Module.Api;
using Atlas.Conta.BackOffice.Module.Api.Amo;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Proiectii;
using DevExpress.ExpressApp;
using Microsoft.AspNetCore.Mvc;

namespace Atlas.Conta.BackOffice.WebApi.API.Conta;

// Amortizarea lunară (F26-D7/D10), oglinda lui `ItvController`: document GENERAT, fără `WriteDto`.
// Derivă din `Document`, nu din `NotaContabila`, deci un id de AMO pe ușa NTC dă 404 fără predicat.
// F22-D5: rutele cu cifre de registru cer și dreptul pe el; `genereaza`/`regenereaza` cer CREARE.
[Route("api/amo")]
public class AmoController : ContaApiController {
    public AmoController(IObjectSpaceFactory secured, INonSecuredObjectSpaceFactory nonSecured,
        DevExpress.ExpressApp.Security.ISecurityStrategyBase securitate)
        : base(secured, nonSecured, securitate) { }

    const int AnMinim = 2000, AnMaxim = 2100;

    // Ordine implicită DECLARATĂ, ca la ITV: fără ea `Id`-ul (ordinea de inserare) ar pune
    // draftul regenerat în capul listei.
    [HttpGet]
    [ProducesResponseType(typeof(PaginaDto<AmoListDto>), StatusCodes.Status200OK)]
    public object Get(DataSourceLoadOptions loadOptions) {
        using var os = Secured(typeof(AmortizareLunara));
        return Incarca(AmoApply.Lista(os), loadOptions,
            OrdineLista.Descrescator("Data"), OrdineLista.Descrescator("Id"));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AmoReadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status404NotFound)]
    public IActionResult GetById(Guid id) {
        var refuz = AutorizeazaCitire<AmortizareLunara>(id) ?? RegistrulCitibil();
        if (refuz != null)
            return refuz;
        using var os = NonSecured(typeof(AmortizareLunara));
        var dto = AmoApply.Citeste(os, id);
        return dto == null ? Invizibil() : Ok(dto);
    }

    // Parametrii sunt nullable ca „lipsă" să se distingă de `0`, pe care binding-ul
    // l-ar livra tăcut; un singur 400, cu toate erorile deodată (70f).
    [HttpGet("previzualizare")]
    [ProducesResponseType(typeof(PrevizualizareAmoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult Previzualizare([FromQuery] int? an = null, [FromQuery] int? luna = null) {
        var erori = Perioada(an, luna);
        if (erori.Count > 0)
            return BadRequest(EroriDto.Din(erori));

        using (var osSecured = Secured(typeof(AmortizareLunara))) {
            if (!PoateCiti(typeof(AmortizareLunara), osSecured))
                return RefuzCitire(typeof(AmortizareLunara));
        }
        var refuzRegistru = RegistrulCitibil();
        if (refuzRegistru != null)
            return refuzRegistru;
        return Domeniu(() => {
            using var os = NonSecured(typeof(AmortizareLunara));
            return Ok(AmoApply.Previzualizeaza(os, an.Value, luna.Value));
        });
    }

    // 200 și când `Motiv != null`: luna ocupată e un RAPORT, nu o eroare.
    // 79: comanda scrie ori de câte ori luna e liberă, deci gate-ul de CREARE vine înaintea ei.
    [HttpPost("genereaza")]
    [ProducesResponseType(typeof(GenerareAmoRezultatDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult Genereaza([FromBody] GenerareAmoRequestDto cerere) =>
        CreareAutorizata<AmortizareLunara>(() => Domeniu(() => {
            using var os = NonSecured(typeof(AmortizareLunara));
            return Ok(AmoApply.Genereaza(os, cerere));
        }));

    // Produce un document cu alt Id, deci cere AMBELE: gate de instanță și de creare pe tip (79).
    [HttpPost("{id:guid}/regenereaza")]
    [ProducesResponseType(typeof(GenerareAmoRezultatDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult Regenereaza(Guid id) =>
        ComandaAutorizata<AmortizareLunara>(id, () => CreareAutorizata<AmortizareLunara>(() => Domeniu(() => {
            using var os = NonSecured(typeof(AmortizareLunara));
            return Ok(AmoApply.Regenereaza(os, id));
        })));

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult Delete(Guid id) =>
        ScriereAutorizata<AmortizareLunara>(id, () => Domeniu(() => {
            using var os = Secured(typeof(AmortizareLunara));
            AmoApply.Sterge(os, id);
            return NoContent();
        }), OperatieAcces.Stergere);

    // Comenzi: OS NON-SECURED, tranzacția integral a motorului (42b).
    [HttpPost("{id:guid}/opereaza")]
    [ProducesResponseType(typeof(OperareRezultatDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult Opereaza(Guid id) => Comanda(id, os => OperareApi.Opereaza(os, id));

    [HttpPost("{id:guid}/anuleaza")]
    [ProducesResponseType(typeof(OperareRezultatDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult Anuleaza(Guid id) => Comanda(id, os => OperareApi.AnuleazaOperarea(os, id));

    [HttpPost("{id:guid}/storneaza")]
    [ProducesResponseType(typeof(OperareRezultatDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult Storneaza(Guid id, [FromBody] StornoRequestDto cerere) =>
        Comanda(id, os => OperareApi.Storneaza(os, id, cerere?.Data ?? DateOnly.FromDateTime(DateTime.Today)));

    [HttpPost("{id:guid}/valideaza")]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status404NotFound)]
    public IActionResult Valideaza(Guid id) =>
        ComandaAutorizata<AmortizareLunara>(id, () => Domeniu(() => {
            using var os = NonSecured(typeof(AmortizareLunara));
            return Ok(EroriDto.Din(OperareApi.Valideaza(os, id)));
        }));

    IActionResult Comanda(Guid id, Func<IObjectSpace, OperareRezultat> comanda) =>
        ComandaAutorizata<AmortizareLunara>(id, () => Domeniu(() => {
            using var os = NonSecured(typeof(AmortizareLunara));
            return Ok(OperareRezultatDto.Din(comanda(os)));
        }));

    IActionResult RegistrulCitibil() {
        using var os = Secured(typeof(RegistruImobilizari));
        return PoateCiti(typeof(RegistruImobilizari), os)
            ? null
            : RefuzCitire(typeof(RegistruImobilizari));
    }

    static List<string> Perioada(int? an, int? luna) {
        var erori = new List<string>();
        if (an == null)
            erori.Add("Parametrul „an” este obligatoriu (anul lunii de amortizat).");
        else if (an < AnMinim || an > AnMaxim)
            erori.Add($"„an” trebuie să fie între {AnMinim} și {AnMaxim}.");
        if (luna == null)
            erori.Add("Parametrul „luna” este obligatoriu (luna de amortizat, 1–12).");
        else if (luna < 1 || luna > 12)
            erori.Add("„luna” trebuie să fie între 1 și 12 — amortizarea se calculează pe o lună.");
        return erori;
    }
}
