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

    // Gate-ul de CREARE pe TIP, ca la orice `POST` de felie (F22-D2): fără el,
    // `User` era refuzat abia de primul document invizibil rezolvat de Apply, cu
    // 422 și cu fraza domeniului — cod al regulii, unde adevărul e al dreptului.
    //
    // După gate, refuzurile de invariant (documente neoperate, sensuri identice,
    // contrapartidă lipsă, sumă peste rest) rămân de DOMENIU ⇒ 422 prin
    // `Domeniu`, cu erorile ca listă — inclusiv id-urile inexistente, pe care
    // Apply le traduce în mesaj de domeniu (abaterea documentată de la 42b).
    [HttpPost]
    [ProducesResponseType(typeof(ImperechereReadDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status403Forbidden)]
    public IActionResult Post([FromBody] ImperechereWriteDto dto) =>
        CreareAutorizata<Imperechere>(() => Domeniu(() => {
            using var os = Secured(typeof(Imperechere));
            var creata = ImperechereApply.Creeaza(os, dto);
            return Created($"/api/imperecheri/{creata.Id}", creata);
        }));

    // Gate-ul de ȘTERGERE pe instanță (F22-D2), care înlocuiește verificarea de
    // existență scrisă cu mâna: 404 pe inexistent SAU invizibil (altfel mesajul
    // „nu există" al Apply-ului ar fi ieșit 422), 403 fără drept de **Delete** —
    // permisiune distinctă de Write în XAF. Restul e liber: legătura n-are
    // registre proprii, iar dispariția ei doar eliberează restul celor două
    // documente (și le redeschide anularea/stornarea — `AreImperecheri`).
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult Delete(Guid id) =>
        ScriereAutorizata<Imperechere>(id, () => Domeniu(() => {
            using var os = Secured(typeof(Imperechere));
            ImperechereApply.Sterge(os, id);
            return NoContent();
        }), OperatieAcces.Stergere);

    // Panoul de stingeri al unui document, într-un singur apel (F3-D3):
    // Total/Asignat/Rămas din `ImperechereService` + rândurile cu partea opusă.
    // 404 dacă documentul nu există (Apply întoarce null).
    [HttpGet("{documentId:guid}/stingeri")]
    [ProducesResponseType(typeof(StingeriDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status404NotFound)]
    public IActionResult Stingeri(Guid documentId) {
        using var os = Secured(typeof(Document));
        var dto = ImperechereApply.Stingeri(os, documentId);
        return dto == null ? Invizibil() : Ok(dto);
    }
}
