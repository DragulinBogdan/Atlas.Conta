using Atlas.Conta.BackOffice.Module.Api;
using Atlas.Conta.BackOffice.Module.Api.Trz;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using DevExpress.ExpressApp;
using Microsoft.AspNetCore.Mvc;

namespace Atlas.Conta.BackOffice.WebApi.API.Conta;

// Transport peste ImperechereApply. Desfacerea și ștergerea au gate pe
// instanța vizibilă, apoi comandă non-secured: scriu compensarea în cub.
// Citirea panoului și rezolvarea documentelor pentru creare rămân secured.
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
    // `Domeniu`, cu erorile ca listă. Referințele inexistente/invizibile sunt
    // refuzate cu 404 înaintea comenzii non-secured (101).
    [HttpPost]
    [ProducesResponseType(typeof(ImperechereReadDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status403Forbidden)]
    public IActionResult Post([FromBody] ImperechereWriteDto dto) =>
        CreareAutorizata<Imperechere>(() => Domeniu(() => {
            using var os = Secured(typeof(Imperechere));
            if (os.GetObjectByKey<Document>(dto.DocumentStingatorId) == null
                || os.GetObjectByKey<Document>(dto.DocumentId) == null) return Invizibil();
            using var motor = NonSecured(typeof(Imperechere));
            var creata = ImperechereApply.Creeaza(motor, dto);
            return Created($"/api/imperecheri/{creata.Id}", creata);
        }));

    // Gate-ul de ȘTERGERE pe instanță (F22-D2), care înlocuiește verificarea de
    // existență scrisă cu mâna: 404 pe inexistent SAU invizibil (altfel mesajul
    // „nu există" al Apply-ului ar fi ieșit 422), 403 fără drept de **Delete** —
    // permisiune distinctă de Write în XAF. Comanda eliberează suma în cub
    // și șterge legătura în aceeași tranzacție, numai în perioada deschisă.
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult Delete(Guid id) =>
        ScriereAutorizata<Imperechere>(id, () => Domeniu(() => {
            using var os = NonSecured(typeof(Imperechere));
            ImperechereApply.Sterge(os, id);
            return NoContent();
        }), OperatieAcces.Stergere);

    // F27-D8: DESFACEREA unei imperecheri dintr-o perioadă închisă — rând
    // INVERS, nu ștergere. `DELETE` rămâne calea din fereastra deschisă (unde
    // gardianul îl lasă); aici e singura cale dincolo de graniță.
    //
    // Ordinea de pe sârmă (F22-D1): 400 (data malformată) → 404 (inexistentă sau
    // invizibilă) → 403 (Write pe instanță — desfacerea SCRIE, nu șterge) → 422
    // (domeniul: deja desfăcută, rând invers, dată sub cea a imperecherii,
    // perioadă închisă a rândului nou). Comanda rulează pe ușa non-secured:
    // rândul invers e al motorului, iar gardianul îl refuză pe cea securizată.
    [HttpPost("{id:guid}/desfa")]
    [ProducesResponseType(typeof(ImperechereReadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult Desfa(Guid id, [FromBody] DesfaImperechereRequestDto cerere) {
        var refuz = Autorizeaza<Imperechere>(id);
        if (refuz != null)
            return refuz;
        return Domeniu(() => {
            using var os = NonSecured(typeof(Imperechere));
            return Ok(ImperechereApply.Desfa(os, id, cerere));
        });
    }

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
