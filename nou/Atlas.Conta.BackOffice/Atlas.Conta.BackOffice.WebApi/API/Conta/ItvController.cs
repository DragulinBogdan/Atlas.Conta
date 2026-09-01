using Atlas.Conta.BackOffice.Module.Api;
using Atlas.Conta.BackOffice.Module.Api.Itv;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Proiectii;
using DevExpress.ExpressApp;
using Microsoft.AspNetCore.Mvc;

namespace Atlas.Conta.BackOffice.WebApi.API.Conta;

// Felia ITV (F21): închiderea lunară de TVA. Transport pur, ca toate
// controllerele de felie — regulile sunt în `InchidereTvaApply` și în
// `InchidereTvaService` (Module), exersate din ModelCheck pe același cod.
//
// ═══ Ce e ALTFEL față de celelalte felii (F21-D1) ═══
// ITV nu e un agregat CULES: nu există `POST`/`PUT` cu un `WriteDto`, fiindcă
// nu se culege nimic pe document. Antetul e server-owned integral (`Numar` din
// politică la materializare, `Data` = ultima zi a lunii, laturile = unitatea
// cerută), iar liniile sunt CALCULATE din soldurile registrului. În locul
// scrierii stau trei comenzi proprii:
//
//   * `previzualizare` — dry-run-ul: ce s-ar închide, sau de ce nu se poate;
//   * `genereaza`      — comanda, cu parametri (an, lună, unitate internă);
//   * `{id}/regenereaza` — „modificarea" unui draft: se șterge și se reface.
//
// Restul (`opereaza`/`anuleaza`/`storneaza`/`valideaza`/`DELETE`) e identic
// `NtcController`, fiindcă acolo întrebarea e a DOCUMENTULUI, nu a tipului.
//
// ═══ Cele două uși, și de ce citirea are DOUĂ gate-uri diferite (F21-D3) ═══
// Trei rute răspund cu cifre ale MOTORULUI (soldurile 4426/4427 la data
// închiderii, verdictul anti-stale, cele trei linii care s-ar genera), deci
// calculul lor rulează pe ușa NON-SECURED: pe ușa securizată soldurile s-ar fi
// însumat peste rândurile de registru pe care permisiunile le ascund, adică o
// cifră FALSĂ prezentată ca „nu e nimic de închis" (argumentul 73g pentru
// fișierul SAF-T — filtrarea tăcută produce un răspuns plauzibil și greșit, nu
// unul gol). Verdictul de ACCES se ia în schimb înainte, pe ușa securizată, și
// diferă după cum întrebarea are sau nu subiect:
//
//   * `GET {id}`         — instanță ⇒ `AutorizeazaCitire<InchidereTva>` (404/403);
//   * `GET previzualizare` — n-are subiect (e o LUNĂ) ⇒ `PoateCiti` pe TIP (403);
//   * `POST genereaza`   — n-are subiect (îl PRODUCE) ⇒ `PoateCrea` pe TIP (403),
//     asimetria numită de 76-r4: `ComandaAutorizata(id)` n-are ce rezolva;
//   * `GET` lista        — ușa securizată filtrează rândurile, deci lista goală
//     pentru cine n-are drepturi e un răspuns adevărat (69g/71g), fără gate.
[Route("api/itv")]
public class ItvController : ContaApiController {
    public ItvController(IObjectSpaceFactory secured, INonSecuredObjectSpaceFactory nonSecured,
        DevExpress.ExpressApp.Security.ISecurityStrategyBase securitate)
        : base(secured, nonSecured, securitate) { }

    // Anul, mărginit ca la SAF-T: cererea cu un an absurd e MALFORMATĂ, n-o
    // refuză domeniul. Marginea de jos e mai jos decât acolo (2020) fiindcă aici
    // nu e o schemă de declarație care s-o impună, ci doar plauzibilitatea.
    const int AnMinim = 2000, AnMaxim = 2100;

    // ── Citire ────────────────────────────────────────────────────────────

    // Ordinea implicită e DECLARATĂ, spre deosebire de listele celorlalte felii:
    // rândurile sunt LUNI, iar `Id`-ul pe care biblioteca l-ar alege singură
    // (`Proiectii/OrdineLista.cs`) e ordinea de INSERARE — după o regenerare,
    // draftul lunii vechi are cel mai nou Id și ar sări în capul listei. Cea mai
    // recentă lună închisă întâi; `Id` rămâne tiebreak, ca `LIMIT/OFFSET`-ul să
    // aibă ordine totală. Sortarea cerută de grilă rămâne primară.
    [HttpGet]
    [ProducesResponseType(typeof(PaginaDto<ItvListDto>), StatusCodes.Status200OK)]
    public object Get(DataSourceLoadOptions loadOptions) {
        using var os = Secured(typeof(InchidereTva));
        return Incarca(InchidereTvaApply.Lista(os), loadOptions,
            OrdineLista.Descrescator("Data"), OrdineLista.Descrescator("Id"));
    }

    // Gate pe INSTANȚĂ pe ușa securizată (invizibil ⇒ 404, fără drept de citire
    // ⇒ 403), apoi DTO-ul pe ușa non-secured: `Sold4426Curent`/`Sold4427Curent`/
    // `Stale` sunt cifre ale motorului. `null` de la `Citeste` ⇒ 404 și pentru un
    // id care există dar nu e o închidere de TVA — dar acolo gate-ul a răspuns
    // deja 404, fiindcă `GetObjectByKey<InchidereTva>` nu-l vede.
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ItvReadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetById(Guid id) {
        var refuz = AutorizeazaCitire<InchidereTva>(id);
        if (refuz != null)
            return refuz;
        using var os = NonSecured(typeof(InchidereTva));
        var dto = InchidereTvaApply.Citeste(os, id);
        return dto == null ? NotFound() : Ok(dto);
    }

    // Dry-run-ul comenzii. NU scrie nimic — de aceea e GET, și de aceea gate-ul e
    // de CITIRE, nu de creare: „ce s-ar închide în luna asta" e o întrebare, nu o
    // intenție.
    //
    // Perioada e OBLIGATORIE și e o LUNĂ; parametrii sunt nullable ca „lipsă" să
    // se distingă de `0`, pe care model binding-ul l-ar livra tăcut (precedentul
    // D300/SAF-T). Validarea e explicită, nu prin `[Range]` pe un DTO de query:
    // aceeași formă ca toate celelalte rute cu `an`+`luna`, și un singur 400 pe
    // sârmă (70f) — `EroriDto`, cu TOATE erorile deodată, nu prima.
    [HttpGet("previzualizare")]
    [ProducesResponseType(typeof(PrevizualizareItvDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IActionResult Previzualizare([FromQuery] int? an = null, [FromQuery] int? luna = null) {
        var erori = Perioada(an, luna);
        if (erori.Count > 0)
            return BadRequest(EroriDto.Din(erori));

        using (var osSecured = Secured(typeof(InchidereTva))) {
            if (!PoateCiti(typeof(InchidereTva), osSecured))
                return Forbid();
        }
        // `Domeniu` și aici (review 79 M7): ancora `TipDocument` lipsă din seed
        // aruncă `OperareException`, care fără traducere ar ieși 400 text/plain
        // prin filtrul DevExpress — în afara contractului „un singur 400" (70f).
        return Domeniu(() => {
            using var os = NonSecured(typeof(InchidereTva));
            return Ok(InchidereTvaApply.Previzualizeaza(os, an.Value, luna.Value));
        });
    }

    // ── Comenzile proprii feliei ──────────────────────────────────────────

    // Generarea. 200 și când `Motiv != null`: „luna e deja închisă" / „n-are ce
    // închide" e un RAPORT adevărat, nu o eroare (precedentele `DscId = null` la
    // backorder, 58, și lotul ANAF, 72e). 422 rămâne al refuzurilor de DOMENIU pe
    // care le aruncă serviciul — cronologia (46c), `TRZ` lipsă, unitatea
    // ne-internă; 400 al lunii în afara 1–12, din `[Range]`-ul cererii, prin
    // `InvalidModelStateResponseFactory` (70f), înaintea oricărei atingeri de bază.
    //
    // Gate-ul de CREARE, pe tip, ÎNAINTE de orice: draftul se scrie pe ușa
    // non-secured (58c — serviciul completează câmpuri server-owned), deci fără
    // el orice utilizator autentificat ar fi generat închideri. `Apply`-ul comite
    // singur, condiționat pe existența draftului.
    [HttpPost("genereaza")]
    [ProducesResponseType(typeof(GenerareItvRezultatDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult Genereaza([FromBody] GenerareItvRequestDto cerere) {
        using (var osSecured = Secured(typeof(InchidereTva))) {
            if (!PoateCrea(typeof(InchidereTva), osSecured))
                return Forbid();
        }
        return Domeniu(() => {
            using var os = NonSecured(typeof(InchidereTva));
            return Ok(InchidereTvaApply.Genereaza(os, cerere));
        });
    }

    // Regenerarea unui DRAFT: are subiect, deci gate-ul de instanță e cel al
    // comenzilor (`ComandaAutorizata` ⇒ 404 invizibil / 403 fără Write) — dar
    // PRODUCE și un document nou, cu alt Id, deci cere și dreptul de CREARE pe
    // tip, ca `genereaza` (review 79 M5: altfel dreptul de scriere pe documente
    // ar fi ocolit `PoateCrea`). Unitatea nu se cere din nou — `Apply`
    // refolosește `PredatorId` al draftului (F21-D4). Pe un document care nu mai
    // e Draft ⇒ 422.
    [HttpPost("{id:guid}/regenereaza")]
    [ProducesResponseType(typeof(GenerareItvRezultatDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult Regenereaza(Guid id) => ComandaAutorizata(id, () => {
        using (var osSecured = Secured(typeof(InchidereTva))) {
            if (!PoateCrea(typeof(InchidereTva), osSecured))
                return Forbid();
        }
        return Domeniu(() => {
            using var os = NonSecured(typeof(InchidereTva));
            return Ok(InchidereTvaApply.Regenereaza(os, id));
        });
    });

    // Ștergerea unui DRAFT, pe ușa SECURED ca la NTC: pre-check-ul de domeniu e
    // în `Sterge` (mesaj propriu), gardianul de Committing rămâne plasa.
    // `Apply`-ul comite singur.
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult Delete(Guid id) => ComandaAutorizata(id, () => Domeniu(() => {
        using var os = Secured(typeof(InchidereTva));
        InchidereTvaApply.Sterge(os, id);
        return NoContent();
    }));

    // ── Comenzi: OS NON-SECURED, tranzacția integral a motorului (42b) ─────
    // Identic `NtcController`: `OperareApi` lucrează pe `Document`, agnostic la
    // tip. Singura diferență e `typeof` din fabrica de ObjectSpace.
    [HttpPost("{id:guid}/opereaza")]
    [ProducesResponseType(typeof(OperareRezultatDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult Opereaza(Guid id) => Comanda(id, os => OperareApi.Opereaza(os, id));

    [HttpPost("{id:guid}/anuleaza")]
    [ProducesResponseType(typeof(OperareRezultatDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult Anuleaza(Guid id) => Comanda(id, os => OperareApi.AnuleazaOperarea(os, id));

    // Data stornării se CULEGE (GATE XAF D10). Ecranul propune ca implicit `Data`
    // documentului — stornarea la chiar data închiderii lasă luna regenerabilă
    // (F21-D7) — dar alegerea rămâne a operatorului, deci serverul nu o impune.
    [HttpPost("{id:guid}/storneaza")]
    [ProducesResponseType(typeof(OperareRezultatDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult Storneaza(Guid id, [FromBody] StornoRequestDto cerere) =>
        Comanda(id, os => OperareApi.Storneaza(os, id, cerere?.Data ?? DateOnly.FromDateTime(DateTime.Today)));

    [HttpPost("{id:guid}/valideaza")]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Valideaza(Guid id) => ComandaAutorizata(id, () => Domeniu(() => {
        using var os = NonSecured(typeof(InchidereTva));
        return Ok(EroriDto.Din(OperareApi.Valideaza(os, id)));
    }));

    IActionResult Comanda(Guid id, Func<IObjectSpace, OperareRezultat> comanda) =>
        ComandaAutorizata(id, () => Domeniu(() => {
            using var os = NonSecured(typeof(InchidereTva));
            return Ok(OperareRezultatDto.Din(comanda(os)));
        }));

    // Toate erorile deodată, ca la SAF-T/D300: cine cere o lună greșită vrea să
    // afle tot ce e greșit, nu prima problemă.
    static List<string> Perioada(int? an, int? luna) {
        var erori = new List<string>();
        if (an == null)
            erori.Add("Parametrul „an” este obligatoriu (anul lunii de închis).");
        else if (an < AnMinim || an > AnMaxim)
            erori.Add($"„an” trebuie să fie între {AnMinim} și {AnMaxim}.");
        if (luna == null)
            erori.Add("Parametrul „luna” este obligatoriu (luna de închis, 1–12).");
        else if (luna < 1 || luna > 12)
            erori.Add("„luna” trebuie să fie între 1 și 12 — închiderea de TVA se face pe o lună.");
        return erori;
    }
}
