using Atlas.Conta.BackOffice.Module.Api;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Motor;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Security;
using Microsoft.AspNetCore.Mvc;

namespace Atlas.Conta.BackOffice.WebApi.API.Conta;

// Corecția unui document operat (F27-D6), pe ORICE tip: comanda e a BAZEI, ca
// `api/perioade`, nu a unei felii — ruta e `api/documente/{id}/corecteaza` și
// documentul se rezolvă POLIMORF (`Document`), exact cum îl încarcă motorul.
//
// Ordinea de pe sârmă (F22-D1): 400 (motivul necunoscut — enum pe NUME, 57a) →
// 404 (documentul inexistent SAU invizibil pe ușa securizată) → 403 (Write pe
// instanță, plus Create ȘI Write pe TIPUL CONCRET: comanda PRODUCE un document
// nou, ca `regenereaza`) → 422 (domeniul: original neoperat, deja corectat,
// perioadă închisă, dependenți — refuzurile motorului).
[Route("api/documente")]
public class CorectieController : ContaApiController {
    public CorectieController(IObjectSpaceFactory secured, INonSecuredObjectSpaceFactory nonSecured,
        ISecurityStrategyBase securitate) : base(secured, nonSecured, securitate) { }

    [HttpPost("{id:guid}/corecteaza")]
    [ProducesResponseType(typeof(CorectieRezultatDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult Corecteaza(Guid id, [FromBody] CorecteazaRequestDto cerere) {
        var erori = new List<string>();
        var motiv = Motiv(cerere?.Motiv, erori);
        if (erori.Count > 0)
            return BadRequest(EroriDto.Din(erori));

        var refuz = Autorizeaza<Document>(id) ?? RefuzPeTipulConcret(id);
        if (refuz != null)
            return refuz;

        var data = cerere?.Data is DateOnly d && d != default ? d : DateOnly.FromDateTime(DateTime.Today);
        return Domeniu(() => {
            using var os = NonSecured(typeof(Document));
            return Ok(CorectieRezultatDto.Din(OperareApi.Corecteaza(os, id, data, motiv.Value)));
        });
    }

    // Parse pe NUME, la graniță, cu valorile valide enumerate — 400, nu 422:
    // cererea e MALFORMATĂ (un membru de enum care nu există), nu refuzată de
    // domeniu. Aceeași frază ca `ApiEnum.Membru`, alt status.
    static MotivCorectie? Motiv(string valoare, ICollection<string> erori) {
        if (string.IsNullOrWhiteSpace(valoare)) {
            erori.Add("„Motiv” nu e cules — valorile acceptate: "
                + string.Join(", ", Enum.GetNames<MotivCorectie>()) + ".");
            return null;
        }
        var cerut = valoare.Trim();
        foreach (var nume in Enum.GetNames<MotivCorectie>())
            if (string.Equals(nume, cerut, StringComparison.OrdinalIgnoreCase))
                return Enum.Parse<MotivCorectie>(nume);
        erori.Add($"Motivul corecției „{valoare}” nu există — valorile acceptate: "
            + string.Join(", ", Enum.GetNames<MotivCorectie>()) + ".");
        return null;
    }

    // Gate-ul de CREARE pe tipul CONCRET al documentului (80b: Create ȘI Write).
    // `Autorizeaza<Document>` a răspuns deja la „e vizibil?" și „am voie să
    // scriu pe el?"; aici se pune întrebarea documentului NOU, pe care comanda
    // îl produce. Tipul nu poate veni din rută — se citește de pe instanță.
    IActionResult RefuzPeTipulConcret(Guid id) {
        using var os = Secured(typeof(Document));
        var doc = os.GetObjectByKey<Document>(id);
        if (doc == null)
            return Invizibil();
        var tip = MotorOperare.ClasaReala(doc);
        return PoateCrea(tip, os) ? null : RefuzCreare(tip);
    }
}
