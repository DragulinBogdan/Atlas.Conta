namespace Atlas.Conta.BackOffice.Module.Api.Dvi;

// Felia DVI (decizia 86, DVI-D5): declarația vamală de import — agregat cules ca
// NTC (antet + linii), plus legăturile n→m cu facturile de import și lista
// candidaților de legat.

public sealed class DviWriteDto {
    /// <summary>MRN-ul declarației — cules (DVI n-are politică de numerotare).</summary>
    public string Numar { get; set; }
    public DateOnly Data { get; set; }
    // F27-D4: lipsă pe sârmă = data documentului.
    public DateOnly? DataInregistrare { get; set; }
    public Guid PredatorId { get; set; }
    public Guid PrimitorId { get; set; }
    public List<DviLinieWriteDto> Linii { get; set; } = new();
    /// <summary>
    /// Facturile de import legate. Singura cale de scriere a legăturilor
    /// (DVI-D3): diferența față de starea curentă se creează și se șterge.
    /// </summary>
    public List<Guid> FacturiIds { get; set; } = new();
}

// `Id` null = linie NOUĂ; `Id` cunoscut = actualizare. Liniile existente absente
// din payload se ȘTERG (43c).
public sealed class DviLinieWriteDto {
    public Guid? Id { get; set; }
    public Guid TipMaterialId { get; set; }
    public Guid? TipTvaId { get; set; }
    /// <summary>Valoarea în vamă (baza), culeasă — nu preț × cantitate.</summary>
    public decimal Valoare { get; set; }
    /// <summary>Taxa din declarație; 0 o calculează motorul din cotă la operare.</summary>
    public decimal ValoareTva { get; set; }
}

public sealed class DviReadDto {
    public Guid Id { get; set; }
    public string Numar { get; set; }
    public DateOnly Data { get; set; }
    public DateOnly DataInregistrare { get; set; }
    public string Stare { get; set; }
    public DateTime? DataOperare { get; set; }
    public Guid PredatorId { get; set; }
    public string PredatorDenumire { get; set; }
    public Guid PrimitorId { get; set; }
    public string PrimitorDenumire { get; set; }
    /// <summary>Σ `Valoare` a liniilor — valoarea în vamă.</summary>
    public decimal Baza { get; set; }
    /// <summary>Σ `ValoareTva` a liniilor — taxa în vamă.</summary>
    public decimal Tva { get; set; }
    public List<DviLinieReadDto> Linii { get; set; } = new();
    public List<DviFacturaDto> Facturi { get; set; } = new();
    public bool PoateEdita { get; set; }
    public bool PoateOpera { get; set; }
    public bool PoateAnula { get; set; }
    public bool PoateStorna { get; set; }
}

public sealed class DviLinieReadDto {
    public Guid Id { get; set; }
    public Guid TipMaterialId { get; set; }
    public string TipMaterialCod { get; set; }
    public string TipMaterialDenumire { get; set; }
    public Guid? TipTvaId { get; set; }
    public string TipTvaCod { get; set; }
    public string TipTvaDenumire { get; set; }
    public decimal? TipTvaCota { get; set; }
    public decimal Valoare { get; set; }
    public decimal ValoareTva { get; set; }
}

// Factura legată, cât să fie recunoscută și deschisă. `Stare` e a facturii:
// anularea ei NU se refuză (DVI-r2), deci ecranul o arată.
public sealed class DviFacturaDto {
    public Guid FacturaId { get; set; }
    public string Numar { get; set; }
    public DateOnly Data { get; set; }
    public string PartenerDenumire { get; set; }
    public string Stare { get; set; }
    /// <summary>Σ (`Valoare` + `ValoareTva`) a liniilor, ca `Document.Total`.</summary>
    public decimal Valoare { get; set; }
}

public sealed class DviListDto {
    public Guid Id { get; set; }
    public string Numar { get; set; }
    public DateOnly Data { get; set; }
    public string Stare { get; set; }
    public string PredatorDenumire { get; set; }
    public decimal Baza { get; set; }
    public decimal Tva { get; set; }
    public int NrFacturi { get; set; }
}

// Plicul candidaților: pe o perioadă largă mulțimea facturilor operate e
// nemărginită, iar trunchierea nu se face tăcut (precedentul `MaiSunt` de la
// panoul de compensare NTC).
public sealed class FacturiCandidateDto {
    public List<FacturaCandidataDto> Candidati { get; set; } = new();
    /// <summary>
    /// Interogarea a atins plafonul ÎNAINTEA filtrului de clasă fiscală: mai
    /// există facturi în perioadă, iar operatorul trebuie s-o îngusteze.
    /// </summary>
    public bool MaiSunt { get; set; }
}

// Candidatul de legat: o factură de intrare OPERATĂ din perioada cerută.
public sealed class FacturaCandidataDto {
    public Guid FacturaId { get; set; }
    public string Numar { get; set; }
    public DateOnly Data { get; set; }
    public Guid PartenerId { get; set; }
    public string PartenerDenumire { get; set; }
    /// <summary>Clasa fiscală a furnizorului (`ClasaFiscala.APartenerului`), ca string.</summary>
    public string ClasaFiscala { get; set; }
    /// <summary>Σ (`Valoare` + `ValoareTva`) a liniilor, ca `Document.Total`.</summary>
    public decimal Valoare { get; set; }
    /// <summary>Tipul de material dominant pe liniile facturii (Σ `Valoare` maximă).</summary>
    public Guid? TipMaterialSugeratId { get; set; }
}
