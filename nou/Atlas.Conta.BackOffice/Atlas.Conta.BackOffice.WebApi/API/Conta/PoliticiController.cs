using Atlas.Conta.BackOffice.Module.Api;
using Atlas.Conta.BackOffice.Module.Api.Politici;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using DevExpress.ExpressApp;
using Microsoft.AspNetCore.Mvc;

namespace Atlas.Conta.BackOffice.WebApi.API.Conta;

// Felia 23 (F23-D8) — RAPORTUL DE VERIFICARE A PROFILULUI.
//
// De la felia 23 politicile se editează din client (F23-D5) și poartă un timbru
// de proveniență (`DinSeed`, F23-D4). Raportul e perechea acelei deschideri:
// fără el, divergențele dintre profilul livrat și baza clientului rămân
// invizibile până când iese o declarație greșită.
//
// Scrierea celor 12 politici NU trece pe aici — ea merge prin OData
// (`api/odata/*`), cu invarianții în `GardianEditare` pe ușa comună. Ruta asta e
// singura din felie care are nevoie de un controller: e o PROIECȚIE peste mai
// multe tabele deodată, nu CRUD-ul unei entități.
//
// ═══ Ușa: gate de CITIRE pe tipuri, apoi calculul NON-SECURED ═══
// (F23-D8, amendat după măsurătoarea pasului 2 — familia 73g/80e.)
//
// Forma dintâi a rutei citea raportul pe ușa SECURIZATĂ, pe argumentul „lista
// goală e un răspuns adevărat" (69g/71g). Măsurat pe host, argumentul nu ține
// AICI: un `User` care nu vede niciun rând din `TipTva`/`MapareD300` primea 20
// de constatări care AFIRMAU că tipuri de TVA existente „nu există în bază".
// Raportul nu e o listă de rânduri, e un VERDICT peste starea profilului —
// filtrarea tăcută nu-l face gol, îl face FALS, exact defectul pe care 73g îl
// descrie pentru fișierul SAF-T (un răspuns plauzibil și greșit, nu unul gol).
//
// Deci: dreptul de citire se cere pe TOATE tipurile pe care raportul le citește
// (`PoliticiApply.TipuriCitite` — descoperite prin reflecție din
// `ICuProvenienta`, plus `Partener`/`Produs`), pe ușa securizată, ÎNAINTE; primul
// refuz iese 403 `EroriDto` cu fraza „citi" a tipului lipsă. Abia apoi calculul
// rulează NON-SECURED, ca cifra să fie a BAZEI.
// Regula generalizată a lui 80e: o rută care întoarce un VERDICT calculat pe ușa
// non-secured cere dreptul de citire pe tot ce însumează.
[Route("api/politici")]
public class PoliticiController : ContaApiController {
    public PoliticiController(IObjectSpaceFactory secured, INonSecuredObjectSpaceFactory nonSecured,
        DevExpress.ExpressApp.Security.ISecurityStrategyBase securitate)
        : base(secured, nonSecured, securitate) { }

    /// <summary>
    /// Ce s-a abătut de la profilul livrat: rânduri manuale, referințe spre
    /// rânduri șterse, tipuri de TVA inactive încă referite ca implicit, tipuri
    /// cu politică de TVA dar fără ancoră, goluri de mapare D300/D394.
    /// </summary>
    [HttpGet("verificare")]
    [ProducesResponseType(typeof(ConstatareProfilDto[]), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult Verificare() {
        // Gate-ul, pe ușa securizată: un singur ObjectSpace pentru toate
        // întrebările — `CanRead(Type, IObjectSpace)` e o permisiune pe TIP, nu o
        // interogare, deci nu atinge baza per tip.
        using (var securizat = Secured(typeof(TipDocument)))
            foreach (var tip in PoliticiApply.TipuriCitite)
                if (!PoateCiti(tip, securizat))
                    return RefuzCitire(tip);

        // Nu e paginat prin `DataSourceLoader`: raportul e MĂRGINIT prin
        // construcție (plafon per tabel în serviciu) și se citește întreg — o
        // constatare fără vecinele ei nu spune mare lucru. Gruparea pe `Fel` e a
        // ecranului.
        //
        // `Domeniu` traduce refuzurile de domeniu pe care le pot ridica funcțiile
        // seed-ului chemate pentru golurile de mapare (aceeași disciplină ca la
        // `previzualizare` din ITV): 422 `EroriDto`, nu 400 text/plain prin
        // filtrul DevExpress (70f).
        return Domeniu(() => {
            using var os = NonSecured(typeof(TipDocument));
            return Ok(PoliticiApply.Verificare(os));
        });
    }
}
