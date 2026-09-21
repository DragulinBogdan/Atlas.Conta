using Atlas.Conta.BackOffice.Module.Api;
using Atlas.Conta.BackOffice.Module.Api.Implicite;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using DevExpress.ExpressApp;
using Microsoft.AspNetCore.Mvc;

namespace Atlas.Conta.BackOffice.WebApi.API.Conta;

// Felia 23 (F23-D6) — IMPLICITELE DE CULEGERE: serverul spune, clientul arată.
//
// Ruta nu aparține niciunei felii de document: aceeași întrebare o pun toate
// cele cinci editoare de linie cu TVA (FCT, FCL, DEC, RLF, RDC), iar răspunsul
// e al aceleiași funcții pe care o cheamă și PUT-ul lor
// (`ImpliciteService.TipTva`, prin wrapper-ul din `TvaService`). Cele două nu
// pot diverge fiindcă sunt o singură funcție — de-aia endpoint-ul e o CITIRE
// subțire peste ea, nu o a doua rezolvare scrisă pentru client.
//
// ═══ Ușa: SECURIZATĂ ═══
// Implicitul se propune peste nomenclatoarele pe care utilizatorul le VEDE.
// Nicio cifră de registru nu intră în răspuns, deci regula 80e („o sumă peste un
// registru cere dreptul de citire pe registru") nu se aplică aici: partenerul
// invizibil produce „fără partener", nu o cifră filtrată prezentată ca adevăr.
// Ramura aia e chiar apărarea contra oracolului de existență (80a): serviciul
// dă ACELAȘI text pentru partener lipsă, inexistent și invizibil, deci `User`
// nu poate afla, întrebând implicite, ce parteneri există.
//
// ═══ Fără gate pe TIP (F23-D6) ═══
// Cine poate culege o linie poate întreba implicitul ei. Un gate de citire pe
// `TipTva` ar fi fost redundant (ușa securizată filtrează deja: cui nu-i e
// vizibil tipul propus primește id-ul fără etichetă) și ar fi transformat o
// afordanță într-un refuz — pe o rută care nu dezvăluie nimic ce ușa securizată
// n-ar dezvălui oricum.
//
// ═══ 400 vs 404, și de ce ancora se caută pe ușa NON-SECURED ═══
// `tipDocument` necunoscut e 400, nu 404: parametrul e al CERERII (un cod de
// vocabular), nu subiectul ei — ruta n-are `{id}`, deci n-are subiect a cărui
// vizibilitate s-o ascundă. Ordinea de pe sârmă (80a) rămâne 401 → 400 → …, iar
// corpul e `EroriDto` ca peste tot (80d).
//
// Consecința măsurată pe host (probă F23, `User`): dacă ancora s-ar căuta pe ușa
// SECURIZATĂ, un utilizator care nu vede `TipDocument` ar primi „tip de document
// necunoscut" — adică un 400 care MINTE despre cauză (codul lui exista) și care
// transformă o lipsă de drepturi într-o eroare de cerere. De aceea maparea
// COD → id se face non-secured: e vocabularul de rutare al clientului, publicat
// deja în `metadata.json`, iar clientul chiar l-a trimis în cerere — nu e nimic
// de ascuns acolo.
//
// REZOLVAREA rămâne pe ușa securizată, și asta e esențial: PUT-ul celor cinci
// felii aplică ACELAȘI serviciu pe OS-ul lor securizat (`TvaService.
// AplicaTipTvaImplicit`). Dacă endpoint-ul ar rezolva non-secured, cele două ar
// DIVERGE — ecranul ar precompleta un tip pe care salvarea nu-l scrie. Prețul,
// declarat: un utilizator fără drept de citire pe `TipTva`/`TipDocument`/
// politici primește 200 cu `Sursa = Niciuna` și niciun tip, nu implicitul
// generic — un răspuns GOL, adevărat, nu unul plauzibil și fals (73g).
[Route("api/implicite")]
public class ImpliciteController : ContaApiController {
    public ImpliciteController(IObjectSpaceFactory secured, INonSecuredObjectSpaceFactory nonSecured,
        DevExpress.ExpressApp.Security.ISecurityStrategyBase securitate)
        : base(secured, nonSecured, securitate) { }

    /// <summary>
    /// Tipul de TVA propus pentru o linie NOUĂ, cu sursa și motivul.
    /// </summary>
    /// <param name="tipDocument">Codul ancorei (`TipDocument.Cod`): FCT, FCL, DEC, RLF, RDC…</param>
    /// <param name="partenerId">Partenerul documentului — el poartă REGIMUL.</param>
    /// <param name="produsId">Produsul liniei — el poartă COTA.</param>
    /// <param name="data">Data DOCUMENTULUI (nu ziua de azi): rândurile de politică au valabilitate.</param>
    [HttpGet("tip-tva")]
    [ProducesResponseType(typeof(RezultatImplicitDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status400BadRequest)]
    public IActionResult TipTva([FromQuery] string tipDocument, [FromQuery] Guid? partenerId = null,
            [FromQuery] Guid? produsId = null, [FromQuery] DateOnly? data = null) {
        // `Domeniu` acoperă și aici commit-ul care nu există și excepțiile de
        // rezolvare: ruta nu scrie nimic, dar un profil fără ancoră nu trebuie să
        // iasă 500 brut sau 400 text/plain prin filtrul DevExpress (70f).
        return Domeniu(() => {
            Guid? id;
            using (var vocabular = NonSecured(typeof(TipDocument)))
                id = ImpliciteApply.IdTipDocument(vocabular, tipDocument);
            if (id == null)
                // Mesaj de CERERE, în română, nu `ModelState`: binding-ul a
                // reușit (e un string valid), iar ce lipsește e vocabularul.
                // Fiindcă lookup-ul de mai sus e non-secured, 400 înseamnă AICI
                // un singur lucru — codul nu există în ancoră —, niciodată „nu
                // ți-e vizibil".
                return BadRequest(EroriDto.DinMesaj(
                    $"Tip de document necunoscut: „{tipDocument}”. Se așteaptă codul ancorei "
                    + "(FCT, FCL, DEC, RLF, RDC…)."));
            // Data lipsă = AZI. Nu e o valoare implicită de comoditate: linia se
            // culege acum, iar rezolvarea are nevoie de o dată ca să compare
            // `ValabilDeLa` — un `default(DateOnly)` (anul 1) ar fi ales tăcut
            // rândul „dintotdeauna" chiar și acolo unde profilul a fost schimbat.
            var laData = data ?? DateOnly.FromDateTime(DateTime.Today);
            using var os = Secured(typeof(TipTva));
            return Ok(ImpliciteApply.TipTva(os, id.Value, partenerId, produsId, laData));
        });
    }
}
