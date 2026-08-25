using Atlas.Conta.BackOffice.Module.Anaf;
using Atlas.Conta.BackOffice.Module.Api;
using Atlas.Conta.BackOffice.Module.Api.Parteneri;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using DevExpress.ExpressApp;
using Microsoft.AspNetCore.Mvc;

namespace Atlas.Conta.BackOffice.WebApi.API.Conta;

// Felia 15 (D15-D4): COMENZI pe partener, nimic altceva.
//
// De ce n-are citire: `Partener` e deja pe OData cu CRUD (`Startup.cs`) —
// nomenclator VIU, întreținut din fluxul operațional (F2-D4). Un `GET
// api/parteneri` ar fi a doua ușă de citire pentru aceleași rânduri, cu altă
// paginare și alt filtru. Aici stă doar ce OData NU poate face: o comandă care
// iese în lume (HTTP către ANAF) și scrie un câmp SERVER-OWNED
// (`DataSincronizareAnaf`, refuzat de `GardianEditare` pe orice ușă secured).
//
// ═══ Cele două uși, ca peste tot (42b / 58c) ═══
// Gate-ul („cine are voie") pe ObjectSpace SECURED, ÎNAINTE — `Autorizeaza<T>`
// din bază, generalizat la felia asta fiindcă întrebarea e aceeași ca pe
// documente, doar tipul diferă. Scrierea pe ușa NON-SECURED, în OS-ul
// serviciului: timbrul e al lui.
//
// ═══ De ce lotul nu e „totul sau nimic" ═══
// Un lot de parteneri e o SELECȚIE de om (sau nomenclatorul filtrat), nu o
// tranzacție: un rând invizibil, o persoană fizică fără CUI sau un commit picat
// nu au voie să anuleze ceilalți 499. Fiecare ies în `Sarite` CU MOTIV, iar
// serviciul comite per partener. Singurul lucru care oprește tot lotul e
// cererea însăși (listă goală / peste plafon) — un 400, adică „reformulează".
[Route("api/parteneri")]
public class ParteneriController : ContaApiController {
    // Plafonul din D15-D4: 500 de parteneri = 5 loturi ANAF = ~5 s de așteptare
    // IMPUSĂ (1 apel/s), fără nimic de câștigat dintr-o paralelizare pe care
    // ANAF o refuză. Peste el, treaba e a Import1C (D15-D6), care rulează
    // detașat, nu a unui request care ar sta agățat în timeout-uri de proxy.
    public const int MaximLot = 500;

    readonly PlatitorTvaClient client;

    public ParteneriController(IObjectSpaceFactory secured, INonSecuredObjectSpaceFactory nonSecured,
        DevExpress.ExpressApp.Security.ISecurityStrategyBase securitate, PlatitorTvaClient client)
        : base(secured, nonSecured, securitate) {
        this.client = client;
    }

    // ── Un partener ───────────────────────────────────────────────────────
    //
    // Statusurile, și de ce sunt exact astea:
    //   404/403 — gate-ul (inexistent sau invizibil / fără drept de scriere);
    //   200     — s-a interogat ANAF, inclusiv când CUI-ul e în `notFound`
    //             (`gasit: false` + avertisment: e un RĂSPUNS, nu un eșec);
    //   422     — refuz de domeniu: partenerul nu e candidat (străin, CNP, fără
    //             CUI) sau lotul ANAF a picat FATAL (4xx/JSON invalid) — a
    //             reîncerca n-ar schimba nimic;
    //   503     — lotul ANAF a picat TRANZITORIU (5xx, timeout, rețea). Nu 422:
    //             cererea e bună, serviciul din amonte nu răspunde ACUM, iar
    //             clientul are voie să reîncerce (riscul 5: două comenzi
    //             concurente înseamnă 2 apeluri/s, peste plafonul ANAF).
    [HttpPost("{id:guid}/sincronizeaza-anaf")]
    [ProducesResponseType(typeof(SincronizareAnafDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> SincronizeazaAnaf(Guid id, [FromQuery] bool suprascrie = false,
            CancellationToken ct = default) {
        var refuz = Autorizeaza<Partener>(id);
        if (refuz != null)
            return refuz;
        return await Domeniu(async () => {
            using var os = NonSecured(typeof(Partener));
            var lot = await SincronizareAnafService.SincronizeazaAsync(os, client, [id], suprascrie, ct);

            // Eroarea de lot ÎNAINTEA rezultatului: pe un singur partener, un lot
            // eșuat înseamnă că partenerul n-a fost atins deloc. El apare și în
            // `Sarite` („lotul ANAF … a eșuat"), dar motivul REAL — și statusul
            // care se cuvine — sunt aici.
            var eroare = lot.Erori.FirstOrDefault();
            if (eroare != null)
                return StatusCode(
                    eroare.Tranzitorie ? StatusCodes.Status503ServiceUnavailable
                                       : StatusCodes.Status422UnprocessableEntity,
                    EroriDto.Din([$"Registrul ANAF nu a răspuns ({(eroare.Tranzitorie
                        ? "eroare tranzitorie, se poate reîncerca" : "eroare fatală")}): {eroare.Mesaj}"]));

            var rezultat = lot.Rezultate.FirstOrDefault();
            if (rezultat != null)
                return Ok(SincronizareAnafDto.Din(rezultat));

            var sarit = lot.Sarite.FirstOrDefault();
            return StatusCode(StatusCodes.Status422UnprocessableEntity, EroriDto.Din([
                sarit != null
                    ? $"Partenerul {sarit.Eticheta ?? sarit.Id.ToString()} nu poate fi sincronizat cu ANAF: {sarit.Motiv}."
                    : "Sincronizarea nu a produs niciun rezultat pentru partenerul cerut."]));
        });
    }

    // ── Lot ───────────────────────────────────────────────────────────────
    //
    // 200 chiar și când NIMIC nu s-a sincronizat: răspunsul e un RAPORT, iar
    // „toți cei 40 selectați sunt persoane fizice" e un raport valid, nu o
    // eroare de cerere. 400 rămâne al cererii care nu se poate executa deloc
    // (listă goală / peste plafon), în forma unică `EroriDto` (70f).
    [HttpPost("sincronizeaza-anaf")]
    [ProducesResponseType(typeof(SincronizareAnafLotDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> SincronizeazaAnafLot([FromBody] SincronizareAnafCerereDto cerere,
            CancellationToken ct = default) {
        var ids = cerere?.Ids;
        if (ids == null || ids.Length == 0)
            return BadRequest(EroriDto.Din([
                "Lista „ids” este obligatorie și trebuie să conțină cel puțin un partener."]));
        if (ids.Length > MaximLot)
            return BadRequest(EroriDto.Din([
                $"Lotul are {ids.Length} parteneri, plafonul este {MaximLot} (ANAF acceptă "
                + $"{PlatitorTvaClient.MaximPerLot} coduri pe secundă). Pentru nomenclatorul întreg "
                + "se folosește conectorul Import1C."]));

        // Gate PER ID, pe ușa secured, înaintea oricărui apel în lume: cel fără
        // drept nu ajunge la ANAF și nu ocupă un loc în lotul de 100.
        var (permise, refuzate) = AutorizeazaLot<Partener>(ids, p => p.Denumire ?? p.Cod);
        var sariteDeGate = refuzate.Select(r => new PartenerSaritDto(r.Id, r.Eticheta, r.Motiv)).ToList();
        if (permise.Count == 0)
            return Ok(new SincronizareAnafLotDto([], [.. sariteDeGate], []));

        return await Domeniu(async () => {
            // UN singur ObjectSpace non-secured pentru tot lotul (contractul):
            // serviciul comite per partener în el, iar un eșec e izolat prin
            // `Rollback` — nu prin OS-uri de unică folosință.
            using var os = NonSecured(typeof(Partener));
            var lot = await SincronizareAnafService.SincronizeazaAsync(
                os, client, permise, cerere.Suprascrie, ct);
            // Erorile de lot rămân în corp (`Erori[]`), NU devin status: într-un
            // lot mixt, 100 de parteneri sincronizați și un lot picat sunt
            // amândouă adevărate, iar un 503 le-ar ascunde pe primele.
            return Ok(SincronizareAnafLotDto.Din(lot, sariteDeGate));
        });
    }
}
