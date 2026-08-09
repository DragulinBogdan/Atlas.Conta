using Atlas.Conta.BackOffice.Module.Api;
using Atlas.Conta.BackOffice.Module.Api.Trz;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using DevExpress.ExpressApp;
using Microsoft.AspNetCore.Mvc;

namespace Atlas.Conta.BackOffice.WebApi.API.Conta;

// STINGEREA prin API (F3-D3) — transport peste `ImperechereApply`. Trei verbe,
// atât: CREEAZĂ, ȘTERGE (liber — 31d), CITEȘTE panoul. Nu există PUT:
// `Imperechere` nu e document (n-are Draft/Operat, deci nici agregat de
// reconciliat, nici comenzi), iar re-validarea sumei la update ar cere
// excluderea propriului rând din plafon (41d).
//
// ═══ De ce NU trece pe ușa non-secured / prin `ComandaAutorizata` ═══
// Scrierea unei imperecheri e o SCRIERE DE UTILIZATOR, nu o comandă de motor:
// n-atinge registre și nu schimbă `Stare`. Deci merge pe ObjectSpace-ul
// SECURED, unde autorizarea o dă securitatea XAF, iar invarianții îi re-verifică
// gardianul de Committing (`GardianEditare.VerificaImperechere` →
// `ValideazaCreare`) — dubla rulare (o dată prin serviciu în Apply, o dată la
// commit) e benignă și documentată în `ImperechereApply`.
// Pe citire, același OS secured filtrează natural documentele invizibile.
[Route("api/imperecheri")]
public class ImperecheriController : ContaApiController {
    public ImperecheriController(IObjectSpaceFactory secured, INonSecuredObjectSpaceFactory nonSecured,
        DevExpress.ExpressApp.Security.ISecurityStrategyBase securitate)
        : base(secured, nonSecured, securitate) { }

    // Refuzurile de invariant (documente neoperate, sensuri identice,
    // contrapartidă lipsă, sumă peste rest) sunt de DOMENIU ⇒ 422 prin
    // `Domeniu`, cu erorile ca listă — inclusiv id-urile inexistente, pe care
    // Apply le traduce în mesaj de domeniu (abaterea documentată de la 42b).
    [HttpPost]
    [ProducesResponseType(typeof(ImperechereReadDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult Post([FromBody] ImperechereWriteDto dto) => Domeniu(() => {
        using var os = Secured(typeof(Imperechere));
        var creata = ImperechereApply.Creeaza(os, dto);
        return Created($"/api/imperecheri/{creata.Id}", creata);
    });

    // 404 pe inexistent, ca la ștergerea drafturilor (FCT/trezorerie):
    // existența se verifică o dată AICI, altfel mesajul „nu există" al
    // Apply-ului ar fi ieșit ca 422. Restul e liber — legătura n-are registre
    // proprii, iar dispariția ei doar eliberează restul celor două documente
    // (și le redeschide anularea/stornarea — `AreImperecheri`).
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult Delete(Guid id) => Domeniu(() => {
        using var os = Secured(typeof(Imperechere));
        if (os.GetObjectByKey<Imperechere>(id) == null)
            return NotFound();
        ImperechereApply.Sterge(os, id);
        return NoContent();
    });

    // Panoul de stingeri al unui document, într-un singur apel (F3-D3):
    // Total/Asignat/Rămas din `ImperechereService` + rândurile cu partea opusă.
    // 404 dacă documentul nu există (Apply întoarce null).
    [HttpGet("{documentId:guid}/stingeri")]
    [ProducesResponseType(typeof(StingeriDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Stingeri(Guid documentId) {
        using var os = Secured(typeof(Document));
        var dto = ImperechereApply.Stingeri(os, documentId);
        return dto == null ? NotFound() : Ok(dto);
    }
}
