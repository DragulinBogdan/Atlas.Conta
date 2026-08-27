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
// Modulul S (stocuri, „la cerere" — felia 17, D17-D4) are aceleași două uși pe
// `…/saft/stocuri` și `…/saft/stocuri/xml`, cu ACELEAȘI gărzi în ACEEAȘI ordine:
// singura diferență e proiecția (`SaftStocuri`) și, în fișier, `HeaderComment`.
// Gărzile trăiesc o singură dată (`Sumar` și `Fisier`, mai jos) — două copii ar
// fi divergat exact acolo unde contează.
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
//    e o declarație falsă, nu o listă goală. Simetric (fixul F7): fără CUI,
//    fișierul nu pleacă deloc — 422, nu un XML anonim cu status 200.
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
    public IActionResult Get([FromQuery] int? an = null, [FromQuery] int? luna = null) =>
        Sumar(an, luna, SaftProiectii.Saft);

    // ═══ Modulul S (la cerere = STOCURI), felia 17 ═══
    // Aceleași două uși, aceeași ordine a gărzilor, alt modul: singura diferență
    // e PROIECȚIA (`SaftStocuri` în loc de `Saft`) și, în fișier,
    // `HeaderComment = C`. Rutele sunt separate, nu un parametru `fel` pe
    // aceeași rută, fiindcă cele două declarații se depun separat (un fișier per
    // lună, per modul — §2 al contractului) și fiindcă un client care cere „S"
    // are altă listă de secțiuni de arătat: contractul e altul, deci și ușa.
    [HttpGet("stocuri")]
    [ProducesResponseType(typeof(SaftSumarDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult GetStocuri([FromQuery] int? an = null, [FromQuery] int? luna = null) =>
        Sumar(an, luna, SaftProiectii.SaftStocuri);

    // Ușa JSON, o singură dată pentru ambele module: perioada → proiecție →
    // `Neaplicabil` → sumar. Diferența dintre L și S intră ca FUNCȚIE de
    // proiecție, nu ca `if` pe un parametru — cele două uși n-au voie să înceapă
    // să difere prin altceva decât modulul.
    IActionResult Sumar(int? an, int? luna, Func<IObjectSpace, int, int, DateOnly?, SaftDto> proiectie) {
        var erori = Perioada(an, luna);
        if (erori.Count > 0)
            return BadRequest(EroriDto.Din(erori));

        // Ușa SECURED, ca la toate proiecțiile de registru: cine n-are voie să
        // citească registrul contabil nu-l citește nici așezat pe formular.
        // Proiecția întoarce liste materializate — nimic deferred după `using`.
        using var os = Secured(typeof(RegistruContabil));
        var dto = proiectie(os, an.Value, luna.Value, null);
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
    public IActionResult Xml([FromQuery] int? an = null, [FromQuery] int? luna = null) =>
        Fisier(an, luna, SaftProiectii.Saft, PrefixL);

    // Fișierul modulului S. ACEEAȘI ordine a gărzilor ca la L (`PoateCiti` ⇒ 403,
    // `Neaplicabil` ⇒ 422, CUI lipsă/invalid ⇒ 422), plus una PROPRIE modulului:
    // stocul fizic gol ⇒ 422. Vezi `Fisier`.
    [HttpGet("stocuri/xml")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult XmlStocuri([FromQuery] int? an = null, [FromQuery] int? luna = null) =>
        Fisier(an, luna, SaftProiectii.SaftStocuri, PrefixS);

    const string PrefixL = "SAF-T", PrefixS = "SAF-T-S";

    // Ușa FIȘIER, o singură dată pentru ambele module: gărzile sunt aceleași și
    // în ACEEAȘI ordine, iar asta e chiar motivul pentru care metoda e una. Două
    // copii ar fi divergat exact acolo unde contează (F7 s-ar fi aplicat doar pe
    // una), și n-ar fi existat niciun loc în care să se citească ordinea.
    IActionResult Fisier(int? an, int? luna,
            Func<IObjectSpace, int, int, DateOnly?, SaftDto> proiectie, string prefixNume) {
        var erori = Perioada(an, luna);
        if (erori.Count > 0)
            return BadRequest(EroriDto.Din(erori));

        using var os = Secured(typeof(RegistruContabil));
        // ÎNAINTE de proiecție: fără drept de citire pe registru, nici măcar nu
        // se calculează declarația — refuzul e al fișierului ca atare, nu al
        // conținutului lui.
        if (!PoateCiti(typeof(RegistruContabil), os))
            return Forbid();

        var dto = proiectie(os, an.Value, luna.Value, null);
        if (dto.Neaplicabil != null)
            return StatusCode(StatusCodes.Status422UnprocessableEntity, EroriDto.DinMesaj(dto.Neaplicabil));
        // ═══ Fixul F7 al review-ului: fără CUI nu pleacă niciun fișier ═══
        // `Header.Company.RegistrationNumber` e identitatea DECLARANTULUI. Fără
        // el (societatea necompletată) sau cu un cod care nu trece cifra de
        // control, fișierul e o declarație anonimă — validatorul o respinge, iar
        // 200 cu un XML nedepozabil e cel mai prost răspuns posibil: pare succes.
        // Refuzul e AL DOMENIULUI pe o cerere bine formată ⇒ 422, și se ia cât
        // timp răspunsul e încă schimbabil (`SaftXml.Scrie` ar arunca oricum pe
        // codul gol, dar abia după ce antetele au plecat, deci pe un 200 cu corp
        // trunchiat). JSON-ul sumar rămâne 200: acolo lipsa e deja numită prin
        // avertismentul `SocietateIncompleta`, iar ecranul exact asta trebuie să
        // arate ÎNAINTE de a încerca descărcarea.
        var codFiscal = dto.Header.RegistrationNumber;
        if (string.IsNullOrWhiteSpace(codFiscal))
            return StatusCode(StatusCodes.Status422UnprocessableEntity, EroriDto.Din([
                "Declarația D406 n-are codul fiscal al societății raportoare — fișierul nu se poate genera "
                + "(completați „Configurare → Societate → Cod fiscal”)."]));
        if (!SaftReguli.CuiValid(codFiscal))
            return StatusCode(StatusCodes.Status422UnprocessableEntity, EroriDto.Din([
                $"Codul fiscal al societății raportoare („{codFiscal}”) nu trece cifra de control a CUI-ului — "
                + "validatorul ANAF ar respinge fișierul, deci nu se generează "
                + "(corectați „Configurare → Societate → Cod fiscal”)."]));
        // ═══ Propriu modulului S: declarația fără stoc fizic nu se poate depune ═══
        // MĂSURAT la pasul 3 cu DUK: pe profilul „la cerere" secțiunea
        // `PhysicalStock` e OBLIGATORIU prezentă (deși XSD-ul o are `minOccurs=0`),
        // iar un tag gol pică schema fiindcă `PhysicalStockEntry` e obligatoriu
        // înăuntru. Nu există deci formă validă a unei declarații S fără nicio
        // intrare de stoc — de aceea refuzul e AICI, ca pe `Neaplicabil` și pe CUI:
        // cerere bine formată, refuz al domeniului ⇒ 422. Sumarul rămâne 200, cu
        // `StocFizic = 0`, care spune exact de ce. Garda oglindește refuzul lui
        // `SaftXml.Scrie` — dar se ia înainte de primul octet, ca răspunsul să nu
        // fie un 200 cu corp trunchiat. Scrisă pe `HeaderComment`, nu pe ruta
        // apelată: condiția e a MODULULUI declarat în fișier, ca și în scriitor.
        if (string.Equals(dto.Header.HeaderComment, SaftProiectii.HeaderCommentStocuri,
                StringComparison.OrdinalIgnoreCase) && dto.StocFizic.Count == 0)
            return StatusCode(StatusCodes.Status422UnprocessableEntity, EroriDto.Din([
                $"Luna {dto.Luna:00}/{dto.An} n-are nicio intrare de stoc fizic, iar declarația D406 „la cerere” "
                + "(stocuri) fără secțiunea `PhysicalStock` nu se poate depune — validatorul ANAF o cere "
                + "prezentă, cu cel puțin o intrare. Verificați soldurile de stoc ale perioadei "
                + "(sumarul declarației arată ce a intrat în calcul)."]));

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
            $"attachment; filename=\"{NumeFisier(dto, prefixNume)}\"";
        // STREAMING pe corpul răspunsului: `SaftXml` scrie cu `XmlWriter` peste
        // `Stream` și are `CloseOutput = false` (fluxul e al apelantului), deci
        // fișierul nu trece niciodată printr-un string sau un `MemoryStream`
        // intermediar. Costul asumat: fără `Content-Length` (răspuns chunked) —
        // bara de progres a browserului nu știe cât mai are, ceea ce e prețul
        // corect pentru a nu ține o lună de registru în RAM de două ori.
        SaftXml.Scrie(dto, Response.Body);
        return new EmptyResult();
    }

    // `SAF-T_{CUI}_{an}-{luna}.xml` (L) / `SAF-T-S_{CUI}_{an}-{luna}.xml` (S) —
    // CUI-ul e cel din antet (`RO` tăiat: e prefixul de TVA, nu parte din cod),
    // fiindcă asta identifică declarantul într-un director cu fișierele mai multor
    // firme; modulul e în prefix fiindcă cele două declarații ale ACELEIAȘI luni
    // ar fi altfel două fișiere cu același nume.
    static string NumeFisier(SaftDto dto, string prefix) {
        var cui = dto.Header?.RegistrationNumber ?? "";
        if (cui.StartsWith("RO", StringComparison.OrdinalIgnoreCase))
            cui = cui[2..];
        cui = new string(cui.Where(char.IsLetterOrDigit).ToArray());
        if (cui.Length == 0)
            cui = "fara-cui";
        return $"{prefix}_{cui}_{dto.An:0000}-{dto.Luna:00}.xml";
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
            erori.Add("„luna” trebuie să fie între 1 și 12 — declarația D406 se depune pe o lună "
                + "(și „L” lunară, și „S” la cerere: un fișier per lună).");
        return erori;
    }
}
