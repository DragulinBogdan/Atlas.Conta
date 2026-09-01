namespace Atlas.Conta.BackOffice.Module.Api.Itv;

// Felia ITV (F21): închiderea lunară de TVA. Ultimul tip de document fără ușă
// de client, și deliberat ALTFEL decât toate celelalte (76a, F19-D1): nu e un
// agregat CULES (PUT header + linii), e rezultatul unui SERVICIU
// (`InchidereTvaService`, 46c). De aici forma DTO-urilor:
//
//   * NU există `ItvWriteDto`. Nimic nu se culege pe document — antetul e
//     server-owned în întregime (`Numar` din politică la materializare, `Data` =
//     ultima zi a lunii, laturile = unitatea cerută), iar liniile sunt calculate
//     din soldurile registrului. Un PUT ar fi însemnat că operatorul poate
//     rescrie cifra pe care gardianul anti-stale o verifică la operare.
//   * În locul lui stau o CERERE de comandă (`GenerareItvRequestDto`: an, lună,
//     unitate) și un RAPORT de rezultat (`GenerareItvRezultatDto`).
//   * `PrevizualizareItvDto` e dry-run-ul comenzii: ce s-ar închide, sau de ce
//     nu se poate. Fără el ecranul ar fi trebuit să apese butonul ca să afle.
//
// Cele trei linii au ROLURI, nu doar valori (transfer / de plată / de recuperat),
// iar rolul se citește din CONTURILE POLITICII (decizia 29), niciodată dintr-un
// simbol scris aici: proba unui motor agnostic la plan n-are voie să fie ea
// însăși legată de plan (precedentul D3-V4 din ModelCheck).

// ── Lista lunilor închise ──────────────────────────────────────────────────
//
// `An`/`Luna` se DERIVĂ din `Data` (ultima zi a lunii închise), nu sunt câmpuri:
// modelul nu le are, iar a le persista ar fi creat un al doilea adevăr față de
// data documentului. În SQL ies din `d.Data.Year`/`d.Data.Month`, deci filtrarea
// și sortarea rămân server-side (DataSourceLoader).
public sealed class ItvListDto {
    public Guid Id { get; set; }
    public string Numar { get; set; }
    public DateOnly Data { get; set; }
    public int An { get; set; }
    public int Luna { get; set; }
    public string Stare { get; set; }
    public DateTime? DataOperare { get; set; }
    public string UnitateDenumire { get; set; }
    public decimal Total { get; set; }
}

// ── Linia închiderii ───────────────────────────────────────────────────────
//
// Fără `TipMaterial` (convențional „TRZ", ascuns și în baseline-ul XAF pe notă),
// fără repartitori (serviciul nu-i setează — de aici și dicționarul GOL al lui
// `CapacitateStingere`: ITV nu stinge nimic) și fără cod economic. Ce rămâne e
// exact ce citește un contabil: descrierea, corespondența și suma.
public sealed class ItvLinieReadDto {
    public Guid Id { get; set; }
    public string Descriere { get; set; }
    public Guid? ContDebitId { get; set; }
    public string ContDebitSimbol { get; set; }
    public Guid? ContCreditId { get; set; }
    public string ContCreditSimbol { get; set; }
    public decimal Valoare { get; set; }
}

public sealed class ItvReadDto {
    public Guid Id { get; set; }
    public string Numar { get; set; }
    public DateOnly Data { get; set; }
    public int An { get; set; }
    public int Luna { get; set; }
    public string Stare { get; set; }
    public DateTime? DataOperare { get; set; }
    public Guid UnitateId { get; set; }
    public string UnitateDenumire { get; set; }
    public decimal Total { get; set; }
    public List<ItvLinieReadDto> Linii { get; set; } = new();

    // Rolurile liniilor, identificate prin perechile de conturi ale politicii —
    // ACELEAȘI trei sume pe care le verifică gardianul anti-stale din
    // `InchidereTva.ValideazaOperare`. Zero = linia nu există pe document.
    public decimal Transfer { get; set; }
    public decimal DePlata { get; set; }
    public decimal DeRecuperat { get; set; }

    // Cifra MOTORULUI la `Data` documentului (`InchidereTvaService.Solduri`, ace-
    // eași funcție ca gardianul). Pe un profil fără politică rămân 0 — dar acolo
    // nu există nici documente ITV, deci cazul nu se vede.
    public decimal Sold4426Curent { get; set; }
    public decimal Sold4427Curent { get; set; }

    // Verdictul anti-stale, DOAR pe Draft (`null` pe Operat/Stornat — acolo
    // întrebarea n-are sens: cifra e deja în registru). `true` = soldurile s-au
    // schimbat de la generare, deci operarea VA fi refuzată; ecranul o spune
    // înainte, nu după apăsarea butonului. Criteriul e cel al gardianului, prin
    // `CalculeazaLinii` pe soldurile curente — dacă cele două ar diverge,
    // ecranul ar minți (cusătura e MĂSURATĂ în ModelCheck, F21-D9).
    public bool? Stale { get; set; }

    // Affordances pe RESURSĂ (42e). `PoateAnula`/`PoateStorna` țin cont de
    // imperecheri ca peste tot (57d) — chiar dacă ITV nu poate stinge nimic,
    // afordanța nu se scrie pe presupunerea asta, ci pe aceeași sursă ca
    // gardianul (`ApiProiectii.AreImperecheri`).
    public bool PoateOpera { get; set; }
    public bool PoateAnula { get; set; }
    public bool PoateStorna { get; set; }
    public bool PoateSterge { get; set; }
    public bool PoateRegenera { get; set; }
}

// ── Dry-run-ul comenzii ────────────────────────────────────────────────────
//
// `Motiv` null = luna se poate închide. Altfel numele membrului `MotivNegenerare`
// (string pe sârmă, 57a — eticheta vine din `metadata.json`).
// `Sold4426`/`Sold4427` sunt null DOAR pe `ProfilInert`: fără conturi nu există
// cifră, iar un 0 acolo ar fi spus „n-ai ce închide" în loc de „profilul n-are
// închidere de TVA".
public sealed class PrevizualizareItvDto {
    public int An { get; set; }
    public int Luna { get; set; }
    public string Motiv { get; set; }
    public decimal? Sold4426 { get; set; }
    public decimal? Sold4427 { get; set; }
    public decimal Transfer { get; set; }
    public decimal DePlata { get; set; }
    public decimal DeRecuperat { get; set; }
    // Documentul care blochează (`InchidereVie`: al lunii; `NeCronologica`: cel
    // ulterior), cu eticheta lui — ecranul face link, nu doar afișează un GUID.
    public Guid? InchidereVieId { get; set; }
    public string InchidereVieNumar { get; set; }
    public string InchidereVieStare { get; set; }
    // Simbolurile celor patru conturi ale politicii (79 M6): etichetele
    // ecranului le iau de aici, nu din cod. Null pe profil inert.
    public string SimbolDeductibila { get; set; }
    public string SimbolColectata { get; set; }
    public string SimbolDePlata { get; set; }
    public string SimbolDeRecuperat { get; set; }
}

// ── Comanda ────────────────────────────────────────────────────────────────
//
// Unitatea internă e PARAMETRU CULES (F21-D4): modelul n-are „unitatea internă a
// societății" (`Societate` nu poartă FK spre `UnitateInterna`), iar a ghici-o
// când există mai multe ar fi fost un default care minte. `[Range]` pe lună ⇒
// 400 `EroriDto` din pipeline (70f), înaintea oricărei atingeri de bază.
public sealed class GenerareItvRequestDto {
    // Marginile anului ca pe ruta de previzualizare: fără ele `new DateOnly(0, …)`
    // din serviciu ar fi aruncat un 500 pe o cerere pur și simplu malformată.
    [System.ComponentModel.DataAnnotations.Range(2000, 2100)]
    public int An { get; set; }
    [System.ComponentModel.DataAnnotations.Range(1, 12)]
    public int Luna { get; set; }
    public Guid UnitateId { get; set; }
}

// Rezultatul e un RAPORT, nu un succes/eșec: `DocumentId` null cu `Motiv`
// completat e un răspuns valid și normal (precedentele `DscId = null` la
// backorder, 58, și lotul ANAF, 72e). 422 rămâne pentru refuzurile de DOMENIU
// (cronologie, TRZ lipsă, unitate ne-internă), care sunt cereri greșite, nu
// stări ale lunii.
public sealed class GenerareItvRezultatDto {
    public Guid? DocumentId { get; set; }
    public string Motiv { get; set; }
    public Guid? InchidereVieId { get; set; }
    public decimal? Sold4426 { get; set; }
    public decimal? Sold4427 { get; set; }
    public decimal Transfer { get; set; }
    public decimal DePlata { get; set; }
    public decimal DeRecuperat { get; set; }
}
