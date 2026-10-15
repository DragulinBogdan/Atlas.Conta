using Atlas.Conta.BackOffice.Module.Api;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Motor;
using Atlas.Conta.BackOffice.Module.Proiectii;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Security;
using Microsoft.AspNetCore.Mvc;

namespace Atlas.Conta.BackOffice.WebApi.API.Conta;

public sealed class ConfirmaDepunereDto {
    public string VersiuneExportata { get; set; }
    /// <summary>Antetul `X-Sha256-Fisier` al exportului XML; lipsește când declarația s-a depus din alt program.</summary>
    public string Sha256Fisier { get; set; }
    /// <summary>Antetul `X-Parametri-Fisier` al aceluiași export.</summary>
    public string ParametriFisier { get; set; }
}

public sealed class DepunereDeclaratieDto {
    public string VersiuneExportata { get; set; }
    public DateTime? ConfirmataLa { get; set; }
    public string ConfirmataDe { get; set; }
    public string Sha256Fisier { get; set; }
    public string ParametriFisier { get; set; }
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
                Sha256Fisier = d.Sha256Fisier, ParametriFisier = d.ParametriFisier,
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
        var sha = string.IsNullOrWhiteSpace(cerere.Sha256Fisier) ? null : cerere.Sha256Fisier.Trim().ToLowerInvariant();
        var parametri = string.IsNullOrWhiteSpace(cerere.ParametriFisier) ? null : cerere.ParametriFisier.Trim();
        if ((sha == null) != (parametri == null))
            return BadRequest(EroriDto.Din(["Amprenta fișierului și parametrii lui se trimit împreună."]));
        if (parametri != null && !FisiereDeclaratii.Valizi(tip, parametri))
            return BadRequest(EroriDto.Din(["Parametrii fișierului nu au forma exportului."]));
        Guid? id;
        using (var os = Secured(typeof(PerioadaFiscala)))
            id = os.GetObjectsQuery<PerioadaFiscala>()
                .Where(p => p.An == an && p.Luna == luna).Select(p => (Guid?)p.ID).SingleOrDefault();
        if (id == null) return Invizibil();
        var refuz = Autorizeaza<PerioadaFiscala>(id.Value, OperatieAcces.Modificare);
        if (refuz != null) return refuz;
        if (sha != null && Fisierul(tip, an, luna, sha, parametri) is { } refuzFisier)
            return refuzFisier;
        return Domeniu(() => {
            using var os = NonSecured(typeof(DepunereDeclaratie));
            return Ok(FiscalitateService.ConfirmaDepunerea(os, tip, an, luna,
                cerere.VersiuneExportata.Trim(), utilizator, sha, parametri));
        });
    }

    // Fișierul confirmat se reface pe ușa exportului, cu citirile ei (122 i).
    IActionResult Fisierul(FormularFiscal formular, int an, int luna, string sha, string parametri) {
        using var os = Secured(typeof(Atlas.Conta.BackOffice.Module.Cub.Postare));
        foreach (var tip in FisiereDeclaratii.Citite(formular))
            if (!PoateCiti(tip, os))
                return RefuzCitire(tip);
        var refuzuri = FisiereDeclaratii.Verifica(os, formular, an, luna, sha, parametri);
        return refuzuri.Count == 0 ? null : StatusCode(StatusCodes.Status422UnprocessableEntity, EroriDto.Din(refuzuri));
    }
}
