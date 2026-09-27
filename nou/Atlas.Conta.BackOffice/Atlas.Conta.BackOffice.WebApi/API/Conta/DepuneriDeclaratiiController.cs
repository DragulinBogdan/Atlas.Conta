using Atlas.Conta.BackOffice.Module.Api;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Motor;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Security;
using Microsoft.AspNetCore.Mvc;

namespace Atlas.Conta.BackOffice.WebApi.API.Conta;

public sealed class ConfirmaDepunereDto {
    public string VersiuneExportata { get; set; }
}

public sealed class DepunereDeclaratieDto {
    public string VersiuneExportata { get; set; }
    public DateTime? ConfirmataLa { get; set; }
    public string ConfirmataDe { get; set; }
    public bool PoateConfirma { get; set; }
}

[Route("api/depuneri-declaratii")]
public class DepuneriDeclaratiiController(IObjectSpaceFactory secured,
    INonSecuredObjectSpaceFactory nonSecured, ISecurityStrategyBase securitate)
    : ContaApiController(secured, nonSecured, securitate) {
    readonly string utilizator = securitate.UserName;

    [HttpGet("{formular}/{an:int}/{luna:int}")]
    [ProducesResponseType(typeof(DepunereDeclaratieDto), StatusCodes.Status200OK)]
    public IActionResult Citeste(string formular, int an, int luna) {
        if (!Enum.TryParse<FormularFiscal>(formular, out var tip) || !Enum.IsDefined(tip)
                || an < 2000 || an > 2100 || luna < 1 || luna > 12)
            return BadRequest(EroriDto.Din(["Formularul și perioada sunt obligatorii."]));
        using var os = Secured(typeof(PerioadaFiscala));
        var id = os.GetObjectsQuery<PerioadaFiscala>().Where(p => p.An == an && p.Luna == luna)
            .Select(p => (Guid?)p.ID).SingleOrDefault();
        if (id == null) return Invizibil();
        var perioada = an * 100 + luna;
        var dto = os.GetObjectsQuery<DepunereDeclaratie>().Where(d => d.Formular == tip && d.Perioada == perioada)
            .OrderByDescending(d => d.ConfirmataLa).Select(d => new DepunereDeclaratieDto {
                VersiuneExportata = d.VersiuneExportata, ConfirmataLa = d.ConfirmataLa, ConfirmataDe = d.ConfirmataDe,
            }).FirstOrDefault() ?? new();
        dto.PoateConfirma = Autorizeaza<PerioadaFiscala>(id.Value, OperatieAcces.Modificare) == null;
        return Ok(dto);
    }

    [HttpPost("{formular}/{an:int}/{luna:int}")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult Confirma(string formular, int an, int luna, [FromBody] ConfirmaDepunereDto cerere) {
        if (!Enum.TryParse<FormularFiscal>(formular, out var tip) || !Enum.IsDefined(tip)
                || an < 2000 || an > 2100 || luna < 1 || luna > 12
                || string.IsNullOrWhiteSpace(cerere?.VersiuneExportata))
            return BadRequest(EroriDto.Din(["Formularul, perioada și versiunea exportată sunt obligatorii."]));
        Guid? id;
        using (var os = Secured(typeof(PerioadaFiscala)))
            id = os.GetObjectsQuery<PerioadaFiscala>()
                .Where(p => p.An == an && p.Luna == luna).Select(p => (Guid?)p.ID).SingleOrDefault();
        if (id == null) return Invizibil();
        var refuz = Autorizeaza<PerioadaFiscala>(id.Value, OperatieAcces.Modificare);
        if (refuz != null) return refuz;
        return Domeniu(() => {
            using var os = NonSecured(typeof(DepunereDeclaratie));
            return Ok(FiscalitateService.ConfirmaDepunerea(os, tip, an, luna,
                cerere.VersiuneExportata.Trim(), utilizator));
        });
    }
}
