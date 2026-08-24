using Atlas.Conta.BackOffice.Module.Api;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Proiectii;
using DevExpress.ExpressApp;
using Microsoft.AspNetCore.Mvc;

namespace Atlas.Conta.BackOffice.WebApi.API.Conta;

// Decontul de TVA — formularul 300 (D3-D6): proiecția `RegistruTva` → cele 55 de
// poziții ale formularului. READ-ONLY prin construcție, ca toate proiecțiile de
// registru — nu există verb de scriere pe registre nicăieri în API.
//
// Două diferențe de contract față de `DecontTvaController`, ambele consecințe
// ale faptului că răspunsul e un FORMULAR, nu o listă:
//
//  • **Fără `loadOptions`** (BP-D3, aceeași lecție ca balanța pliată): un
//    formular nu se paginează. Rd. 19 fără rd. 9 nu e „pagina 1 dintr-un
//    decont", e un decont fals; iar sortarea din grilă ar strica ordinea
//    oficială, singura în care formularul se citește. Se întoarce tabloul
//    întreg — mărginit de LEGE la 55 de rânduri, nu de `LIMIT/OFFSET`.
//  • **Perioada e OBLIGATORIE**, spre deosebire de decontul-schelet unde e un
//    simplu filtru. Aici perioada E declarația („perioada de raportare" din
//    antetul formularului): rd. 37 „taxa de plată în perioada de raportare"
//    n-are înțeles fără ea, iar un default tăcut (luna curentă) ar produce un
//    decont plauzibil pentru altă lună decât cea cerută.
[Route("api/proiectii/d300")]
public class D300Controller : ContaApiController {
    public D300Controller(IObjectSpaceFactory secured, INonSecuredObjectSpaceFactory nonSecured,
        DevExpress.ExpressApp.Security.ISecurityStrategyBase securitate)
        : base(secured, nonSecured, securitate) { }

    [HttpGet]
    [ProducesResponseType(typeof(D300Dto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status400BadRequest)]
    public IActionResult Get(
        [FromQuery] DateOnly? dataStart = null, [FromQuery] DateOnly? dataEnd = null,
        [FromQuery] decimal soldPlataPrecedent = 0m, [FromQuery] decimal diferentePlata = 0m,
        [FromQuery] decimal soldNegativPrecedent = 0m, [FromQuery] decimal diferenteNegative = 0m) {

        // 400, nu 422: cererea e malformată (parametri lipsă sau incoerenți), n-o
        // refuză domeniul. Datele sunt nullable EXACT ca să se poată distinge
        // „lipsă" de `default(DateOnly)` (0001-01-01), pe care model binding-ul
        // l-ar fi livrat tăcut pe un parametru ne-nullable.
        var erori = new List<string>();
        if (dataStart == null)
            erori.Add("Parametrul „dataStart” este obligatoriu (începutul perioadei de raportare).");
        if (dataEnd == null)
            erori.Add("Parametrul „dataEnd” este obligatoriu (sfârșitul perioadei de raportare).");
        if (dataStart is DateOnly ds && dataEnd is DateOnly de && ds > de)
            erori.Add("„dataStart” nu poate fi după „dataEnd”.");
        // Cei patru externi sunt SOLDURI și DIFERENȚE de plată — mărimi care în
        // formular au sens doar pozitiv (sensul îl dă rândul pe care stau: 38/39
        // de plată, 41/42 negative). O valoare negativă ar inversa tăcut sensul
        // rezultatului la rd. 44/45, adică ar produce un decont care arată corect
        // și spune invers.
        foreach (var (nume, valoare) in new[] {
            ("soldPlataPrecedent", soldPlataPrecedent), ("diferentePlata", diferentePlata),
            ("soldNegativPrecedent", soldNegativPrecedent), ("diferenteNegative", diferenteNegative)
        })
            if (valoare < 0m)
                erori.Add($"„{nume}” nu poate fi negativ — sensul sumei îl dă rândul pe care se înscrie.");
        // Restricția încrucișată a formularului (§2.3): rd. 38 = 0 dacă rd. 41 > 0
        // și invers. Cele două sunt jumătățile aceleiași bucle între deconturi
        // (rd. 44 → rd. 38, rd. 45 → rd. 41), iar perechea 44/45 e mutual
        // exclusivă — deci un decont precedent nu poate lăsa ambele solduri.
        // Refuz, nu normalizare tăcută: care dintre ele e greșită e o întrebare
        // pentru contabil, nu pentru proiecție.
        if (soldPlataPrecedent > 0m && soldNegativPrecedent > 0m)
            erori.Add("„soldPlataPrecedent” (rd. 38) și „soldNegativPrecedent” (rd. 41) nu pot fi ambele "
                + "pozitive: decontul precedent lasă fie sold de plată (rd. 44), fie sold negativ (rd. 45).");
        if (erori.Count > 0)
            return BadRequest(EroriDto.Din(erori));

        // Aceeași ușă securizată ca jurnalele și decontul-schelet: cine n-are voie
        // să citească registrul fiscal nu are voie să-l citească nici agregat pe
        // formular. Proiecția întoarce deja liste materializate, deci nu rămâne
        // nimic deferred după `using` (lecția `Incarca`).
        using var os = Secured(typeof(RegistruTva));
        return Ok(D300Proiectii.D300(os, dataStart.Value, dataEnd.Value,
            new ParametriD300(soldPlataPrecedent, diferentePlata,
                soldNegativPrecedent, diferenteNegative)));
    }
}
