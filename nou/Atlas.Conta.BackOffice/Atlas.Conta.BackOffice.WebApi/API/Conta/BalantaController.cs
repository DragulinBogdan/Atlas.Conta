using Atlas.Conta.BackOffice.Module.Api;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Proiectii;
using DevExpress.ExpressApp;
using Microsoft.AspNetCore.Mvc;

namespace Atlas.Conta.BackOffice.WebApi.API.Conta;

// Balanța de verificare (R-D2/R-D3/R-D4, decizia 42c): agregarea registrului
// contabil append-only. READ-ONLY prin construcție — nu există verb de scriere pe
// registre nicăieri în API (gardianul le refuză oricum, pe orice cale secured).
//
// Perioada, modul analitic și cele 8 dimensiuni sunt PARAMETRI AI PROIECȚIEI, nu
// filtre `DataSourceLoader` (R-D2): `dataStart` decide ce înseamnă soldul inițial
// — e o graniță dinăuntrul agregării —, iar filtrele de dimensiune se aplică pe
// atomi, ÎNAINTE de `GROUP BY`, când coloana încă există. `loadOptions` rămâne
// deasupra pentru ce e legitim al lui: sortare, paginare, filtrare pe coloanele
// de ieșire (simbol, denumire), grupare. Cele două nu se ating — ASP.NET leagă
// `loadOptions` prin binder-ul DevExtreme și restul ca simpli parametri de query
// (tiparul lui `DocumenteCuRestController`).
[Route("api/proiectii/balanta")]
public class BalantaController : ContaApiController {
    public BalantaController(IObjectSpaceFactory secured, INonSecuredObjectSpaceFactory nonSecured,
        DevExpress.ExpressApp.Security.ISecurityStrategyBase securitate)
        : base(secured, nonSecured, securitate) { }

    [HttpGet]
    [ProducesResponseType(typeof(PaginaDto<BalantaRand>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status400BadRequest)]
    public IActionResult Get(DataSourceLoadOptions loadOptions,
        [FromQuery] DateOnly? dataStart = null, [FromQuery] DateOnly? dataEnd = null,
        [FromQuery] bool analitic = false,
        [FromQuery] Guid? repartitorId = null, [FromQuery] Guid? materialId = null,
        [FromQuery] Guid? codFunctionalId = null, [FromQuery] Guid? codEconomicId = null,
        [FromQuery] Guid? sursaFinantareId = null, [FromQuery] Guid? unitateId = null,
        [FromQuery] Guid? proiectId = null, [FromQuery] Guid? centruCostId = null) {

        // Perioada e OBLIGATORIE și nu are default rezonabil: fără `dataStart`
        // noțiunea „sold inițial" nu există, iar un default tăcut (azi, anul
        // curent) ar produce o balanță plauzibilă și greșită. Parametrii sunt
        // declarați nullable EXACT ca să se poată distinge „lipsă" de
        // `default(DateOnly)` (0001-01-01), pe care model binding-ul l-ar fi
        // livrat tăcut pe un parametru ne-nullable.
        // 400, nu 422: cererea e malformată (îi lipsesc parametri), n-o refuză
        // domeniul.
        var erori = new List<string>();
        if (dataStart == null)
            erori.Add("Parametrul „dataStart” este obligatoriu (definește soldul inițial).");
        if (dataEnd == null)
            erori.Add("Parametrul „dataEnd” este obligatoriu (definește sfârșitul perioadei).");
        if (dataStart is DateOnly ds && dataEnd is DateOnly de && ds > de)
            erori.Add("„dataStart” nu poate fi după „dataEnd”.");
        if (erori.Count > 0)
            return BadRequest(EroriDto.Din(erori));

        using var os = Secured(typeof(RegistruContabil));
        // Ordinea se declară EXPLICIT și e TOTALĂ (review advers D2). Fără ea,
        // `DataSourceLoader` pune ordinea LUI — `ContId` —, care e cheie unică în
        // modul sintetic dar REPETATĂ în cel analitic (cheia de grupare e
        // `Cont × Repartitor`): `ORDER BY` ne-unic sub `LIMIT/OFFSET` n-are ordine
        // garantată, deci un rând poate apărea pe două pagini sau pe niciuna.
        // Depinde de MOD, ca și cheia de grupare a proiecției.
        return Ok(Incarca(ContabilProiectii.Balanta(os, dataStart.Value, dataEnd.Value, analitic,
            repartitorId, materialId, codFunctionalId, codEconomicId,
            sursaFinantareId, unitateId, proiectId, centruCostId),
            loadOptions, ContabilProiectii.OrdineBalanta(analitic)));
    }
}
