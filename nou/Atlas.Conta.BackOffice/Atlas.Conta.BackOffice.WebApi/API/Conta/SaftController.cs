using Atlas.Conta.BackOffice.Module.Api;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Saft;
using DevExpress.ExpressApp;
using Microsoft.AspNetCore.Mvc;

namespace Atlas.Conta.BackOffice.WebApi.API.Conta;

// SAF-T — declarația D406, modul L (felia 16, D16-D5): proiecția registrelor →
// formularul ANAF, servit în DOUĂ forme ale ACELUIAȘI conținut.
//
//   • `GET api/proiectii/saft?an&luna`      → `SaftSumarDto` (JSON) — ce vede
//     omul: antetul, CÂT are fiecare secțiune, cusăturile, `Neincluse`,
//     avertismentele;
//   • `GET api/proiectii/saft/xml?an&luna`  → fișierul, streaming.
//
// Aceeași proiecție în amândouă: fișierul NU e o a doua sursă de adevăr, e
// aceeași declarație scrisă altfel (D16: „fișierul se generează din proiecție,
// nu invers"). Ecranul poate deci verifica înainte de a descărca.
//
// ═══ Trei contracte moștenite de la D300/D394, unul propriu ═══
//  • fără `loadOptions`: formularul nu se paginează — o lună fără jumătate din
//    tranzacții nu e „pagina 1 dintr-o declarație", e o declarație falsă. Ușa
//    JSON servește însă SUMARUL, nu declarația întreagă (amendamentul pasului 4,
//    MĂSURAT: 38,6 MiB pe o lună reală, din care ecranul nu afișa nicio linie).
//    Nu e o paginare — e altă întrebare, cu răspuns complet: „cât e declarația",
//    nu „primele n rânduri din ea". Declarația întreagă are exact o formă
//    servită: FIȘIERUL;
//  • perioada e OBLIGATORIE și e o LUNĂ (`PeriodStart`/`PeriodEnd` din
//    `SelectionCriteria` sunt luni, nu date libere): `an`+`luna`, nullable, ca
//    „lipsă" să se distingă de `0` pe care binding-ul l-ar livra tăcut;
//  • un singur 400 pe sârmă (70f): `EroriDto`, ca la orice proiecție;
//  • PROPRIU: `User` fără drept de citire pe registrul contabil primește JSON
//    filtrat (200 cu liste goale, ca D394) dar **403 pe XML** — motivul e în
//    `ContaApiController.PoateCiti`: un fișier gol semnat cu CUI-ul societății
//    e o declarație falsă, nu o listă goală.
//
// Bugetar ⇒ 422: proiecția întoarce `Neaplicabil` cu motiv (planul instituțiilor
// publice nu e printre cele 12 `TaxAccountingBasis`), în AMBELE forme. Refuzul e
// al DOMENIULUI pe o cerere bine formată — deci 422, nu 400.
//
// Fără cache, deliberat: declarația e o funcție a registrelor la momentul
// cererii, iar un cache ar servi cifre vechi exact în ziua în care se depune.
// Timpul de răspuns e MĂSURAT (§Închidere al contractului), nu presupus.
[Route("api/proiectii/saft")]
public class SaftController : ContaApiController {
    public SaftController(IObjectSpaceFactory secured, INonSecuredObjectSpaceFactory nonSecured,
        DevExpress.ExpressApp.Security.ISecurityStrategyBase securitate)
        : base(secured, nonSecured, securitate) { }

    // `PeriodYear` are `minInclusive = 2020` în schemă; capătul de sus nu e al
    // schemei, ci al aritmeticii: `new DateOnly(an, …)` pe un an absurd ar ieși
    // 500 în loc de un refuz motivat.
    const int AnMinim = 2020, AnMaxim = 2100;

    [HttpGet]
    [ProducesResponseType(typeof(SaftSumarDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult Get([FromQuery] int? an = null, [FromQuery] int? luna = null) {
        var erori = Perioada(an, luna);
        if (erori.Count > 0)
            return BadRequest(EroriDto.Din(erori));

        // Ușa SECURED, ca la toate proiecțiile de registru: cine n-are voie să
        // citească registrul contabil nu-l citește nici așezat pe formular.
        // Proiecția întoarce liste materializate — nimic deferred după `using`.
        using var os = Secured(typeof(RegistruContabil));
        var dto = SaftProiectii.Saft(os, an.Value, luna.Value);
        if (dto.Neaplicabil != null)
            return StatusCode(StatusCodes.Status422UnprocessableEntity, EroriDto.DinMesaj(dto.Neaplicabil));
        // Aceeași proiecție ca fișierul, apoi `Sumar` — funcție PURĂ pe DTO. Costul
        // rămas e al PROIECȚIEI (2,4–3,4 s pe o lună reală), nu al serializării;
        // ce dispare de pe sârmă sunt cele ~38 MiB de liste pe care ecranul nu le
        // afișa. Când chiar ai nevoie de linii, ai fișierul.
        return Ok(SaftProiectii.Sumar(dto));
    }

    // FĂRĂ `[Produces("application/xml")]`, deliberat: atributul e un filtru care
    // impune lista de content-type-uri TUTUROR rezultatelor acțiunii, inclusiv
    // `BadRequest(EroriDto)` și 422 — iar host-ul n-are formatter XML, deci
    // refuzurile ar fi ieșit 406 în loc de `EroriDto`. Corpul de succes nu trece
    // oricum prin negociere (se scrie direct pe `Response.Body`), deci
    // content-type-ul îl punem cu mâna, acolo unde chiar se aplică.
    [HttpGet("xml")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult Xml([FromQuery] int? an = null, [FromQuery] int? luna = null) {
        var erori = Perioada(an, luna);
        if (erori.Count > 0)
            return BadRequest(EroriDto.Din(erori));

        using var os = Secured(typeof(RegistruContabil));
        // ÎNAINTE de proiecție: fără drept de citire pe registru, nici măcar nu
        // se calculează declarația — refuzul e al fișierului ca atare, nu al
        // conținutului lui.
        if (!PoateCiti(typeof(RegistruContabil), os))
            return Forbid();

        var dto = SaftProiectii.Saft(os, an.Value, luna.Value);
        if (dto.Neaplicabil != null)
            return StatusCode(StatusCodes.Status422UnprocessableEntity, EroriDto.DinMesaj(dto.Neaplicabil));
        // `SaftXml.Scrie` ar arunca oricum — dar după ce am fi trimis deja
        // antetele, adică pe o cerere cu status 200 și corp trunchiat. Refuzul
        // se decide cât timp răspunsul e încă schimbabil.
        if (dto.Header == null)
            return StatusCode(StatusCodes.Status422UnprocessableEntity, EroriDto.Din([
                "Declarația D406 n-are antet — societatea raportoare lipsește "
                + "(completați „Configurare → Societate”)."]));

        // MĂSURAT, nu presupus: Kestrel refuză scrierile SINCRONE pe corpul
        // răspunsului (`AllowSynchronousIO` = false din .NET 3.0), iar `XmlWriter`
        // e un scriitor sincron — prima încercare a ieșit
        // `InvalidOperationException: Synchronous operations are disallowed` din
        // `XmlUtf8RawTextWriter.Close()`. Cele două ieșiri erau: (a) a bufferi
        // fișierul într-un `MemoryStream` — adică a ține o lună de registru încă o
        // dată în RAM, exact ce evită streaming-ul; (b) a face `SaftXml` asincron
        // — o schimbare de contract în Module pentru o constrângere de HOST, iar
        // conectorul Import1C scrie același fișier pe disc, sincron, fără nicio
        // nevoie de `await`.
        // Aleasă: steagul PER CERERE, care e chiar mecanismul prevăzut de ASP.NET
        // pentru serializatoarele sincrone. Nu atinge celelalte endpoint-uri (nu e
        // o opțiune globală de Kestrel) și moare odată cu cererea.
        var controlCorp = HttpContext.Features.Get<Microsoft.AspNetCore.Http.Features.IHttpBodyControlFeature>();
        if (controlCorp != null)
            controlCorp.AllowSynchronousIO = true;

        Response.ContentType = "application/xml; charset=utf-8";
        Response.Headers.ContentDisposition =
            $"attachment; filename=\"{NumeFisier(dto)}\"";
        // STREAMING pe corpul răspunsului: `SaftXml` scrie cu `XmlWriter` peste
        // `Stream` și are `CloseOutput = false` (fluxul e al apelantului), deci
        // fișierul nu trece niciodată printr-un string sau un `MemoryStream`
        // intermediar. Costul asumat: fără `Content-Length` (răspuns chunked) —
        // bara de progres a browserului nu știe cât mai are, ceea ce e prețul
        // corect pentru a nu ține o lună de registru în RAM de două ori.
        SaftXml.Scrie(dto, Response.Body);
        return new EmptyResult();
    }

    // `SAF-T_{CUI}_{an}-{luna}.xml` — CUI-ul e cel din antet (`RO` tăiat: e
    // prefixul de TVA, nu parte din cod), fiindcă asta identifică declarantul
    // într-un director cu fișierele mai multor firme.
    static string NumeFisier(SaftDto dto) {
        var cui = dto.Header?.RegistrationNumber ?? "";
        if (cui.StartsWith("RO", StringComparison.OrdinalIgnoreCase))
            cui = cui[2..];
        cui = new string(cui.Where(char.IsLetterOrDigit).ToArray());
        if (cui.Length == 0)
            cui = "fara-cui";
        return $"SAF-T_{cui}_{dto.An:0000}-{dto.Luna:00}.xml";
    }

    // Perioada, ca listă de refuzuri (aceeași formă în ambele acțiuni — o
    // singură definiție a ce înseamnă „lună validă", ca cele două uși să nu
    // înceapă să difere).
    static List<string> Perioada(int? an, int? luna) {
        var erori = new List<string>();
        if (an == null)
            erori.Add("Parametrul „an” este obligatoriu (anul perioadei de raportare).");
        else if (an < AnMinim || an > AnMaxim)
            erori.Add($"„an” trebuie să fie între {AnMinim} și {AnMaxim} "
                + $"(schema D406 acceptă `PeriodYear` de la {AnMinim}).");
        if (luna == null)
            erori.Add("Parametrul „luna” este obligatoriu (luna perioadei de raportare, 1–12).");
        else if (luna < 1 || luna > 12)
            erori.Add("„luna” trebuie să fie între 1 și 12 — declarația D406 „L” se depune pe o lună.");
        return erori;
    }
}
