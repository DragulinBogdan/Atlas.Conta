namespace Atlas.Conta.BackOffice.Module.Api.Irz;

public sealed class IrzLinieDto {
    public string ContSursaSimbol { get; set; }
    public string ContDestinatieSimbol { get; set; }
    /// <summary>Sursa avea sold creditor, deci se închide prin debitare.</summary>
    public bool SursaPeDebit { get; set; }
    public decimal Valoare { get; set; }
}

/// <summary>Ce s-ar închide pe lună și, dacă nu se poate, motivul (numele membrului <c>MotivNegenerare</c>).</summary>
public sealed class PrevizualizareIrzDto {
    public int An { get; set; }
    public int Luna { get; set; }
    public string Motiv { get; set; }
    public Guid? InchidereVieId { get; set; }
    public List<IrzLinieDto> Linii { get; set; } = new();
}

public sealed class GenerareIrzRequestDto {
    [System.ComponentModel.DataAnnotations.Range(2000, 2100)]
    public int An { get; set; }
    [System.ComponentModel.DataAnnotations.Range(1, 12)]
    public int Luna { get; set; }
    public Guid UnitateId { get; set; }
}

/// <summary>Raportul comenzii: <c>DocumentId</c> null cu <c>Motiv</c> e un răspuns valid.</summary>
public sealed class GenerareIrzRezultatDto {
    public Guid? DocumentId { get; set; }
    public string Motiv { get; set; }
    public Guid? InchidereVieId { get; set; }
    public int NumarLinii { get; set; }
}

public sealed class IrzListDto {
    public Guid Id { get; set; }
    public string Numar { get; set; }
    public DateOnly Data { get; set; }
    public int An { get; set; }
    public int Luna { get; set; }
    public string Stare { get; set; }
    public DateTime? DataOperare { get; set; }
    public string UnitateDenumire { get; set; }
    public int NumarLinii { get; set; }
    public decimal Total { get; set; }
}

public sealed class IrzLinieReadDto {
    public Guid Id { get; set; }
    public string ContDebitSimbol { get; set; }
    public string ContCreditSimbol { get; set; }
    public bool SursaPeDebit { get; set; }
    public decimal Valoare { get; set; }
}

/// <summary>Închiderea citită: <c>Stale</c> e verdictul anti-stale, numai pe Draft.</summary>
public sealed class IrzReadDto {
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
    public bool? Stale { get; set; }
    public bool PoateOpera { get; set; }
    public bool PoateAnula { get; set; }
    public bool PoateStorna { get; set; }
    public bool PoateSterge { get; set; }
    public bool PoateRegenera { get; set; }
    public List<IrzLinieReadDto> Linii { get; set; } = new();
}
