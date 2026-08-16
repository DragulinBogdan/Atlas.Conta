using Atlas.Conta.BackOffice.Module.Api;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Proiectii;
using DevExpress.ExpressApp;
using Microsoft.AspNetCore.Mvc;

namespace Atlas.Conta.BackOffice.WebApi.API.Conta;

// Fișa de cont (R-D6): rândurile unui cont, cronologic, cu SOLDUL CURENT cumulat
// până la fiecare rând inclusiv. Read-only, ca toată suprafața de raportare.
//
// Perioada și dimensiunile sunt PARAMETRI AI PROIECȚIEI (R-D2), ca la balanță:
// `dataStart` nu taie rezultatul, ci decide de unde începe AFIȘAREA unui sold
// care s-a cumulat din tot ce e `<= dataEnd`.
[Route("api/proiectii/fisa-cont")]
public class FisaContController : ContaApiController {
    public FisaContController(IObjectSpaceFactory secured, INonSecuredObjectSpaceFactory nonSecured,
        DevExpress.ExpressApp.Security.ISecurityStrategyBase securitate)
        : base(secured, nonSecured, securitate) { }

    [HttpGet]
    [ProducesResponseType(typeof(PaginaDto<FisaContRand>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status400BadRequest)]
    public IActionResult Get(DataSourceLoadOptions loadOptions,
        [FromQuery] Guid? contId = null,
        [FromQuery] DateOnly? dataStart = null, [FromQuery] DateOnly? dataEnd = null,
        [FromQuery] Guid? repartitorId = null, [FromQuery] Guid? materialId = null,
        [FromQuery] Guid? codFunctionalId = null, [FromQuery] Guid? codEconomicId = null,
        [FromQuery] Guid? sursaFinantareId = null, [FromQuery] Guid? unitateId = null,
        [FromQuery] Guid? proiectId = null, [FromQuery] Guid? centruCostId = null) {

        // Ca la balanță: 400 (cererea e malformată — îi lipsesc parametri), nu 422
        // (n-o refuză domeniul). Nullable EXACT ca să se distingă „lipsă" de
        // `default(DateOnly)`/`Guid.Empty`, pe care model binding-ul le-ar livra
        // tăcut pe parametri ne-nullable — o fișă pe `Guid.Empty` ar ieși goală și
        // plauzibilă, adică minciuna cea mai scumpă.
        var erori = new List<string>();
        if (contId == null || contId == Guid.Empty)
            erori.Add("Parametrul „contId” este obligatoriu (fișa e a unui cont anume).");
        if (dataStart == null)
            erori.Add("Parametrul „dataStart” este obligatoriu (definește soldul inițial).");
        if (dataEnd == null)
            erori.Add("Parametrul „dataEnd” este obligatoriu (definește sfârșitul perioadei).");
        if (dataStart is DateOnly ds && dataEnd is DateOnly de && ds > de)
            erori.Add("„dataStart” nu poate fi după „dataEnd”.");
        if (erori.Count > 0)
            return BadRequest(EroriDto.Din(erori));

        // ═══ Sortarea și gruparea se REFUZĂ pe server, nu doar în grilă (R-D6) ═══
        // Soldul curent are sens DOAR în ordinea în care a fost cumulat
        // (`Data, Id, Sens DESC`). Reordonat după alt criteriu — sau spart pe
        // grupuri — devine o coloană de cifre fără sens, afișată cu aceeași
        // autoritate ca restul. Ecranul dezactivează sortarea, dar un client poate
        // trimite `sort=` oricum (curl, deep-link vechi, o grilă viitoare), deci
        // refuzul trăiește AICI. Tăcut, nu 400: cererea rămâne valabilă, doar că
        // ordinea nu e negociabilă.
        loadOptions.Sort = null;
        loadOptions.Group = null;
        // …iar refuzul NU e suficient: golit, `Sort` îl face pe `DataSourceLoader`
        // să-și inventeze ordinea LUI (`Id`-ul singur — adică ordinea de INSERARE),
        // cu un `OrderBy` care ȘTERGE ordinea proiecției în EF Core. De asta ordinea
        // fișei se declară EXPLICIT mai jos, prin `OrdineFisa()`: e singura care
        // ajunge la Postgres sub `LIMIT/OFFSET`, deci singura pe care paginarea o
        // taie. Mecanica, cu sursele citate: `Proiectii/OrdineLista.cs`.
        // FILTRAREA rămâne permisă, deliberat: `SoldCurent` e proprietate a
        // REGISTRULUI (soldul contului la acel rând), nu a vederii — rămâne
        // adevărat pe orice submulțime afișată. Cine ar „repara" asta filtrând
        // înaintea ferestrei ar obține alt raport: soldul cumulat DOAR peste
        // rândurile filtrate, care nu mai e soldul contului.
        // (Dimensiunile, în schimb, sunt parametri de proiecție tocmai fiindcă
        // ele TREBUIE aplicate înaintea ferestrei — vezi R-D2.)

        using var os = Secured(typeof(RegistruContabil));
        var rezultat = Incarca(ContabilProiectii.FisaCont(os, contId.Value,
            dataStart.Value, dataEnd.Value,
            repartitorId, materialId, codFunctionalId, codEconomicId,
            sursaFinantareId, unitateId, proiectId, centruCostId),
            loadOptions, ContabilProiectii.OrdineFisa());
        // Codul de tip al documentului-sursă, peste pagina DEJA materializată
        // (R-D8): un singur query polimorf pe mulțime, nu `GetObjectByKey` în
        // buclă. Consecință: `DocumentTip` nu e o coloană, deci filtrarea/sortarea
        // de grilă pe el nu funcționează — clientul rutează prin `rutaTip` (61a).
        ContabilProiectii.CompleteazaTipDocument(os, Randuri<FisaContRand>(rezultat));
        return Ok(rezultat);
    }
}
