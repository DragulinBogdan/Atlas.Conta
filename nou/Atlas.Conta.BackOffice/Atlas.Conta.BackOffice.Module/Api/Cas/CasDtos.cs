namespace Atlas.Conta.BackOffice.Module.Api.Cas;

// Ieșirea de imobilizări: se culeg antetul și fișele, liniile le produce serverul (F26-D6/D10).

public sealed class CasWriteDto {
    public DateOnly Data { get; set; }
    // F27-D4: lipsă pe sârmă = data documentului.
    public DateOnly? DataInregistrare { get; set; }
    /// <summary>`Casare` | `Vanzare` | `Lipsa`, pe NUME (57a).</summary>
    public string Cauza { get; set; }
    /// <summary>Locul de pe fișe.</summary>
    public Guid PredatorId { get; set; }
    /// <summary>Unitatea internă care înregistrează ieșirea.</summary>
    public Guid PrimitorId { get; set; }
    /// <summary>Fișele care ies; liniile lor sunt ale serverului.</summary>
    public List<Guid> Fise { get; set; } = new();
}

// Read-only pe sârmă; simbolurile conturilor vin din politică (29), nu din cod.
public sealed class CasLinieReadDto {
    public Guid Id { get; set; }
    public Guid ImobilizareId { get; set; }
    public string NumarInventar { get; set; }
    public string ImobilizareDenumire { get; set; }
    public string Fel { get; set; }
    public decimal Valoare { get; set; }
    public Guid? ContDebitId { get; set; }
    public string ContDebitSimbol { get; set; }
    public Guid? ContCreditId { get; set; }
    public string ContCreditSimbol { get; set; }
}

public sealed class CasReadDto {
    public Guid Id { get; set; }
    public string Numar { get; set; }
    public DateOnly Data { get; set; }
    public DateOnly DataInregistrare { get; set; }
    public string Cauza { get; set; }
    public string Stare { get; set; }
    public DateTime? DataOperare { get; set; }
    public Guid PredatorId { get; set; }
    public string PredatorDenumire { get; set; }
    public Guid PrimitorId { get; set; }
    public string PrimitorDenumire { get; set; }
    /// <summary>Σ `Valoare` a liniilor, calculată pe server (42c).</summary>
    public decimal Total { get; set; }
    /// <summary>Fișele culese, în ordinea numărului de inventar.</summary>
    public List<Guid> Fise { get; set; } = new();
    public List<CasLinieReadDto> Linii { get; set; } = new();
    public bool PoateEdita { get; set; }
    public bool PoateOpera { get; set; }
    public bool PoateAnula { get; set; }
    // F27-D6 — legătura de corecție (null = documentul nu corectează nimic).
    public CorectieDto Corectie { get; set; }
    public bool PoateStorna { get; set; }
    public bool PoateSterge { get; set; }
}

public sealed class CasListDto {
    public Guid Id { get; set; }
    public string Numar { get; set; }
    public DateOnly Data { get; set; }
    public string Cauza { get; set; }
    public string Stare { get; set; }
    public string PredatorDenumire { get; set; }
    public decimal Total { get; set; }
    public int NrFise { get; set; }
}
