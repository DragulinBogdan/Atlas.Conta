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
