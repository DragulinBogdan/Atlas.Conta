using Atlas.Conta.BackOffice.Module.Api;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Proiectii;
using DevExpress.ExpressApp;
using Microsoft.AspNetCore.Mvc;

namespace Atlas.Conta.BackOffice.WebApi.API.Conta;

// Decontul de TVA — formularul 300 (D3-D6): proiecția faptelor fiscale din cub, întreagă, fără paginare;
// perioada e obligatorie, fiindcă perioada e declarația.
[Route("api/proiectii/d300")]
public class D300Controller : ContaApiController {
    public D300Controller(IObjectSpaceFactory secured, INonSecuredObjectSpaceFactory nonSecured,
        DevExpress.ExpressApp.Security.ISecurityStrategyBase securitate)
        : base(secured, nonSecured, securitate) { }

    [HttpGet]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(D300Dto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status400BadRequest)]
    public IActionResult Get(
        [FromQuery] DateOnly? dataStart = null, [FromQuery] DateOnly? dataEnd = null,
        [FromQuery] decimal soldPlataPrecedent = 0m, [FromQuery] decimal diferentePlata = 0m,
        [FromQuery] decimal soldNegativPrecedent = 0m, [FromQuery] decimal diferenteNegative = 0m) {
        var externi = new ParametriD300(soldPlataPrecedent, diferentePlata, soldNegativPrecedent, diferenteNegative);
        if (Valideaza(dataStart, dataEnd, externi) is { Count: > 0 } erori)
            return BadRequest(EroriDto.Din(erori));
        using var os = Secured(typeof(Atlas.Conta.BackOffice.Module.Cub.Postare));
        if (!PoateCiti(typeof(Atlas.Conta.BackOffice.Module.Cub.Postare), os))
            return RefuzCitire(typeof(Atlas.Conta.BackOffice.Module.Cub.Postare));
        return Ok(D300Proiectii.D300(os, dataStart.Value, dataEnd.Value, externi));
    }

    /// <summary>Fișierul D300 pe schema v12; 422 cu refuzurile din `D300Dto.RefuzuriXml` (E2-D8).</summary>
    [HttpGet("xml")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult Xml(
        [FromQuery] DateOnly? dataStart = null, [FromQuery] DateOnly? dataEnd = null,
        [FromQuery] decimal soldPlataPrecedent = 0m, [FromQuery] decimal diferentePlata = 0m,
        [FromQuery] decimal soldNegativPrecedent = 0m, [FromQuery] decimal diferenteNegative = 0m,
        [FromQuery] int temei = 0, [FromQuery] bool depusReprezentant = false, [FromQuery] bool? bifaInterne = null,
        [FromQuery] decimal proRata = 0m, [FromQuery] bool solicitRambursare = false,
        [FromQuery] bool cereale = false, [FromQuery] bool mobile = false,
        [FromQuery] bool dispozitive = false, [FromQuery] bool console = false) {
        var externi = new ParametriD300(soldPlataPrecedent, diferentePlata, soldNegativPrecedent, diferenteNegative);
        var erori = Valideaza(dataStart, dataEnd, externi);
        if (temei is not (0 or 2))
            erori.Add("„temei” este 0 (fără temei) sau 2 (art. 105 alin. (6) lit. b) din Codul de procedură fiscală).");
        if (proRata is < 0m or > 100m)
            erori.Add("„proRata” este între 0 și 100.");
        if (erori.Count > 0)
            return BadRequest(EroriDto.Din(erori));
        using var os = Secured(typeof(Atlas.Conta.BackOffice.Module.Cub.Postare));
        foreach (var tip in FisiereDeclaratii.Citite(FormularFiscal.D300))
            if (!PoateCiti(tip, os))
                return RefuzCitire(tip);
        return Declaratie(D300Xml.Genereaza(os, dataStart.Value, dataEnd.Value, externi,
            new ParametriXmlD300(temei, depusReprezentant, bifaInterne, proRata, solicitRambursare,
                cereale, mobile, dispozitive, console)));
    }

    // Externii sunt solduri și diferențe: sensul îl dă rândul, iar rd. 38 și rd. 41 se exclud reciproc (§2.3).
    static List<string> Valideaza(DateOnly? dataStart, DateOnly? dataEnd, ParametriD300 externi) {
        var erori = new List<string>();
        if (dataStart == null)
            erori.Add("Parametrul „dataStart” este obligatoriu (începutul perioadei de raportare).");
        if (dataEnd == null)
            erori.Add("Parametrul „dataEnd” este obligatoriu (sfârșitul perioadei de raportare).");
        if (dataStart is DateOnly ds && dataEnd is DateOnly de && ds > de)
            erori.Add("„dataStart” nu poate fi după „dataEnd”.");
        foreach (var (nume, valoare) in new[] {
            ("soldPlataPrecedent", externi.SoldPlataPrecedent), ("diferentePlata", externi.DiferentePlata),
            ("soldNegativPrecedent", externi.SoldNegativPrecedent), ("diferenteNegative", externi.DiferenteNegative)
        })
            if (valoare < 0m)
                erori.Add($"„{nume}” nu poate fi negativ — sensul sumei îl dă rândul pe care se înscrie.");
        if (externi.SoldPlataPrecedent > 0m && externi.SoldNegativPrecedent > 0m)
            erori.Add("„soldPlataPrecedent” (rd. 38) și „soldNegativPrecedent” (rd. 41) nu pot fi ambele "
                + "pozitive: decontul precedent lasă fie sold de plată (rd. 44), fie sold negativ (rd. 45).");
        return erori;
    }
}
