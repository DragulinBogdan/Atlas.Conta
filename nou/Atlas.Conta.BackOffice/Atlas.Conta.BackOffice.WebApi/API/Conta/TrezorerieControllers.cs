using Atlas.Conta.BackOffice.Module.Api;
using Atlas.Conta.BackOffice.Module.Api.Trz;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using DevExpress.ExpressApp;
using Microsoft.AspNetCore.Mvc;

namespace Atlas.Conta.BackOffice.WebApi.API.Conta;

// Felia verticală TREZORERIE (contract p5-felia-trz, pasul 3) — transport și
// atât, ca la BTR/FCT: tot ce e regulă (reconcilierea agregatului, scara,
// laturile, numerotarea, gardienii) stă în `TrezorerieApply`/motor, o singură
// sursă pentru toate tierele (42a).
//
// ═══ De ce O SINGURĂ bază, cu două rute (F3-D1) ═══
// `TrezorerieApply` e generic pe `T : DocumentTrezorerie`, iar cele două
// controllere ar fi ieșit IDENTICE caracter cu caracter, cu un singur argument
// de tip diferit. Baza abstractă generică ține transportul o dată; `Plata` și
// `Incasare` sunt clase concrete NE-generice (ASP.NET Core nu descoperă
// controllere generice deschise, dar descoperă derivatele închise ale unei baze
// generice — acțiunile moștenite intră normal în rutare și în ApiExplorer, cu
// `[Route]` propriu per derivată). Baza fiind `abstract` nu e descoperită ea
// însăși, deci nu apare nicio rută „fantomă" în swagger.json.
//
// Diferențele de contract față de FCT sunt în DTO, nu aici: `Numar` NU e în
// WriteDto (PLT/INC au PoliticaNumerotare ⇒ server-owned), `Valoare` E culeasă
// (nu există `PregatesteOperare` pe trezorerie).
public abstract class TrezorerieControllerBase<T> : ContaApiController
    where T : DocumentTrezorerie {

    protected TrezorerieControllerBase(IObjectSpaceFactory secured,
        INonSecuredObjectSpaceFactory nonSecured,
        DevExpress.ExpressApp.Security.ISecurityStrategyBase securitate)
        : base(secured, nonSecured, securitate) { }

    // Prefixul rutei, doar pentru `Location`-ul lui 201 (ruta reală vine din
    // `[Route]`-ul derivatei).
    protected abstract string Ruta { get; }

    // ── Citire ────────────────────────────────────────────────────────────
    // Grila e REMOTE (43c): filtrare/sortare/paginare vin în query string,
    // `DataSourceLoader` le pune peste `IQueryable`, SQL-ul le execută.
    [HttpGet]
    [ProducesResponseType(typeof(PaginaDto<TrezorerieListDto>), StatusCodes.Status200OK)]
    public object Get(DataSourceLoadOptions loadOptions) {
        using var os = Secured(typeof(T));
        return Incarca(TrezorerieApply.Lista<T>(os), loadOptions);
    }

    // `Citeste<T>` întoarce null și când id-ul există dar e de CELĂLALT tip
    // (sub TPT filtrarea e în SQL) — o încasare cerută pe `/api/plt/{id}` dă
    // 404, nu documentul altcuiva.
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TrezorerieReadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetById(Guid id) {
        using var os = Secured(typeof(T));
        var dto = TrezorerieApply.Citeste<T>(os, id);
        return dto == null ? NotFound() : Ok(dto);
    }

    // ── Scriere: agregatul per document (42d) ─────────────────────────────
    [HttpPost]
    [ProducesResponseType(typeof(TrezorerieReadDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult Post([FromBody] TrezorerieWriteDto dto) => Domeniu(() => {
        using var os = Secured(typeof(T));
        var id = TrezorerieApply.Aplica<T>(os, null, dto);
        return Created($"{Ruta}/{id}", TrezorerieApply.Citeste<T>(os, id));
    });

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(TrezorerieReadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Put(Guid id, [FromBody] TrezorerieWriteDto dto) => Domeniu(() => {
        using var os = Secured(typeof(T));
        TrezorerieApply.Aplica<T>(os, id, dto);
        return Ok(TrezorerieApply.Citeste<T>(os, id));
    });

    // Ștergerea unui DRAFT. Ca la FCT: existența se verifică o dată AICI, ca
    // statusul să fie 404 (mesajul „nu există" al Apply-ului ar fi ieșit 422);
    // restul (pre-check-ul de Draft) rămâne în `Apply.Sterge`.
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult Delete(Guid id) => Domeniu(() => {
        using var os = Secured(typeof(T));
        if (os.GetObjectByKey<T>(id) == null)
            return NotFound();
        TrezorerieApply.Sterge<T>(os, id);
        return NoContent();
    });

    // ── Comenzi: OS NON-SECURED, tranzacția integral a motorului (42b) ─────
    // `ComandaAutorizata` decide CINE are voie (404 pe inexistent/invizibil,
    // 403 fără Write) înainte ca ușa non-secured să se deschidă.
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
        using var os = NonSecured(typeof(T));
        return Ok(EroriDto.Din(OperareApi.Valideaza(os, id)));
    }));

    IActionResult Comanda(Guid id, Func<IObjectSpace, OperareRezultat> comanda) =>
        ComandaAutorizata(id, () => Domeniu(() => {
            using var os = NonSecured(typeof(T));
            return Ok(OperareRezultatDto.Din(comanda(os)));
        }));
}

[Route("api/plt")]
public class PlataController : TrezorerieControllerBase<Plata> {
    public PlataController(IObjectSpaceFactory secured, INonSecuredObjectSpaceFactory nonSecured,
        DevExpress.ExpressApp.Security.ISecurityStrategyBase securitate)
        : base(secured, nonSecured, securitate) { }

    protected override string Ruta => "/api/plt";
}

[Route("api/inc")]
public class IncasareController : TrezorerieControllerBase<Incasare> {
    public IncasareController(IObjectSpaceFactory secured, INonSecuredObjectSpaceFactory nonSecured,
        DevExpress.ExpressApp.Security.ISecurityStrategyBase securitate)
        : base(secured, nonSecured, securitate) { }

    protected override string Ruta => "/api/inc";
}
