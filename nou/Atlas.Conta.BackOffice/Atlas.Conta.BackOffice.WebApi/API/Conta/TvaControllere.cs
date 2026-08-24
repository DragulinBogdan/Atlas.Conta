using Atlas.Conta.BackOffice.Module.Api;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Proiectii;
using DevExpress.ExpressApp;
using Microsoft.AspNetCore.Mvc;

namespace Atlas.Conta.BackOffice.WebApi.API.Conta;

// Jurnalul de cumpărări / de vânzări (JT-D7): un rând per (Document × TipTva),
// pe un singur `Sens`. READ-ONLY prin construcție — nu există verb de scriere pe
// registre nicăieri în API (gardianul le refuză oricum, pe orice cale secured).
//
// Un singur controller pentru ambele jurnale, ca `PLT`/`INC` (57a): diferența e
// LATURA, nu raportul.
//
// `dataStart`/`dataEnd` sunt filtre SIMPLE (ca la registrul-jurnal, R-D9), nu
// granițe de agregare: un jurnal n-are noțiune de „sold inițial", deci ambele
// sunt opționale. Din același motiv sortarea din grilă e permisă — n-are sold
// curent de rupt.
[Route("api/proiectii/jurnal-tva")]
public class JurnalTvaController : ContaApiController {
    public JurnalTvaController(IObjectSpaceFactory secured, INonSecuredObjectSpaceFactory nonSecured,
        DevExpress.ExpressApp.Security.ISecurityStrategyBase securitate)
        : base(secured, nonSecured, securitate) { }

    [HttpGet]
    [ProducesResponseType(typeof(PaginaDto<JurnalTvaRand>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status400BadRequest)]
    public IActionResult Get(DataSourceLoadOptions loadOptions,
        [FromQuery] string sens = null,
        [FromQuery] DateOnly? dataStart = null, [FromQuery] DateOnly? dataEnd = null) {

        var erori = new List<string>();
        // `sens` e OBLIGATORIU și se parsează pe NUME, nu pe număr (tiparul
        // `TipInstrument`, F3-D1): contractul de sârmă nu depinde de ordinea
        // membrilor unui enum, iar clientul trimite exact ce citește înapoi în
        // `DecontTvaRand.Sens`. Lipsă sau nume necunoscut ⇒ 400, niciodată un
        // default tăcut: un jurnal fără sens ar amesteca cumpărările cu vânzările
        // și ar arăta perfect plauzibil făcând-o.
        // 400, nu 422: cererea e malformată, n-o refuză domeniul.
        SensTva sens1 = default;
        if (string.IsNullOrWhiteSpace(sens))
            erori.Add("Parametrul „sens” este obligatoriu („Achizitie” sau „Livrare”).");
        else if (!Enum.TryParse(sens, ignoreCase: true, out sens1) || !Enum.IsDefined(sens1))
            erori.Add($"Sensul „{sens}” nu există. Valori acceptate: „Achizitie”, „Livrare”.");
        if (dataStart is DateOnly ds && dataEnd is DateOnly de && ds > de)
            erori.Add("„dataStart” nu poate fi după „dataEnd”.");
        if (erori.Count > 0)
            return BadRequest(EroriDto.Din(erori));

        using var os = Secured(typeof(RegistruTva));
        // Ordinea se declară EXPLICIT și e TOTALĂ: altfel `DataSourceLoader` pune
        // în locul ei ordinea LUI (primul membru numit „Id"), iar `ORDER BY` pe
        // cheie ne-unică sub `LIMIT/OFFSET` n-are ordine garantată — un rând poate
        // apărea pe două pagini sau pe niciuna (`Proiectii/OrdineLista.cs`).
        // Aici e doar un DEFAULT: `sort=` de la client are prioritate.
        var rezultat = Incarca(TvaProiectii.JurnalTva(os, sens1, dataStart, dataEnd),
            loadOptions, TvaProiectii.OrdineJurnalTva());
        // Codul de tip al documentului nu e o coloană sub TPT (R-D8): se completează
        // în memorie, peste pagină, cu ACEEAȘI implementare ca fișa și jurnalul —
        // două copii ar diverge tăcut. Vezi limitarea documentată pe `Randuri<T>`
        // pentru modul grupat.
        ContabilProiectii.CompleteazaTipDocument(os, Randuri<JurnalTvaRand>(rezultat));
        return Ok(rezultat);
    }
}

// Decontul (JT-D7): un rând per (Sens × TipTva), peste o perioadă.
//
// Cheia e `TipTva`, nu (Regim × Cota) — `SDD` și `SFD` au același regim și
// aceeași cotă 0, dar coduri SAF-T diferite și rânduri diferite în D300 (motivul
// complet e scris la `TvaProiectii.DecontTva`).
[Route("api/proiectii/decont-tva")]
public class DecontTvaController : ContaApiController {
    public DecontTvaController(IObjectSpaceFactory secured, INonSecuredObjectSpaceFactory nonSecured,
        DevExpress.ExpressApp.Security.ISecurityStrategyBase securitate)
        : base(secured, nonSecured, securitate) { }

    [HttpGet]
    [ProducesResponseType(typeof(PaginaDto<DecontTvaRand>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(EroriDto), StatusCodes.Status400BadRequest)]
    public IActionResult Get(DataSourceLoadOptions loadOptions,
        [FromQuery] DateOnly? dataStart = null, [FromQuery] DateOnly? dataEnd = null) {

        // Perioada rămâne OPȚIONALĂ, spre deosebire de balanță: acolo `dataStart`
        // definește soldul inițial (fără el raportul n-ar avea sens), aici e un
        // filtru — un decont fără perioadă e pur și simplu decontul de la
        // începutul evidenței, care e o cifră adevărată.
        if (dataStart is DateOnly ds && dataEnd is DateOnly de && ds > de)
            return BadRequest(EroriDto.Din(new[] { "„dataStart” nu poate fi după „dataEnd”." }));

        using var os = Secured(typeof(RegistruTva));
        return Ok(Incarca(TvaProiectii.DecontTva(os, dataStart, dataEnd),
            loadOptions, TvaProiectii.OrdineDecontTva()));
    }
}
