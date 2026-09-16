namespace Atlas.Conta.BackOffice.Module.Api.Pif;

// Punerea în funcțiune: agregat cules, cu parametrii de amortizare pe linie (F26-D5/D10).

public sealed class PifWriteDto {
    public DateOnly Data { get; set; }
    // F27-D4: lipsă pe sârmă = data documentului.
    public DateOnly? DataInregistrare { get; set; }
    /// <summary>Unitatea internă care pune în funcțiune.</summary>
    public Guid PredatorId { get; set; }
    /// <summary>Locul: gestiunea / unitatea / angajatul pe care stau fișele liniilor.</summary>
    public Guid PrimitorId { get; set; }
    public List<PifLinieWriteDto> Linii { get; set; } = new();
}

// `Id` null = linie NOUĂ; liniile existente absente din payload se ȘTERG (43c).
public sealed class PifLinieWriteDto {
    public Guid? Id { get; set; }
    public Guid ImobilizareId { get; set; }
    /// <summary>`Intrare` | `Modernizare` | `Revizuire`, pe NUME (57a).</summary>
    public string Fel { get; set; }
    /// <summary>Linia de factură de intrare care hrănește fișa (F26-D5).</summary>
    public Guid? LinieSursaId { get; set; }
    public decimal Valoare { get; set; }
    /// <summary>Brutul fiscal; null (sau 0) îl egalează `PregatesteOperare` cu `Valoare`.</summary>
    public decimal? ValoareFiscala { get; set; }
    public decimal AmortizareInitiala { get; set; }
    public decimal AmortizareFiscalaInitiala { get; set; }
    public int LuniAmortizateInitial { get; set; }
    public string Metoda { get; set; }
    public int? DurataLuni { get; set; }
    public decimal? ValoareReziduala { get; set; }
    public string MetodaFiscala { get; set; }
    public int? DurataFiscalaLuni { get; set; }
    public string CategorieFiscala { get; set; }
    public bool? UtilizareExclusiva { get; set; }
}

public sealed class PifLinieReadDto {
    public Guid Id { get; set; }
    public Guid ImobilizareId { get; set; }
    public string NumarInventar { get; set; }
    public string ImobilizareDenumire { get; set; }
    public string Fel { get; set; }
    public Guid? LinieSursaId { get; set; }
    public string LinieSursaNumar { get; set; }
    public Guid TipMaterialId { get; set; }
    public string TipMaterialCod { get; set; }
    public decimal Valoare { get; set; }
    public decimal ValoareFiscala { get; set; }
    public decimal AmortizareInitiala { get; set; }
    public decimal AmortizareFiscalaInitiala { get; set; }
    public int LuniAmortizateInitial { get; set; }
    public string Metoda { get; set; }
    public int? DurataLuni { get; set; }
    public decimal? ValoareReziduala { get; set; }
    public string MetodaFiscala { get; set; }
    public int? DurataFiscalaLuni { get; set; }
    public string CategorieFiscala { get; set; }
    public bool? UtilizareExclusiva { get; set; }
}

public sealed class PifReadDto {
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
    /// <summary>Σ `Valoare` a liniilor, calculată pe server (42c).</summary>
    public decimal Total { get; set; }
    public List<PifLinieReadDto> Linii { get; set; } = new();
    public bool PoateEdita { get; set; }
    public bool PoateOpera { get; set; }
    public bool PoateAnula { get; set; }
    public bool PoateStorna { get; set; }
    public bool PoateSterge { get; set; }
}

public sealed class PifListDto {
    public Guid Id { get; set; }
    public string Numar { get; set; }
    public DateOnly Data { get; set; }
    public string Stare { get; set; }
    public string PredatorDenumire { get; set; }
    public string PrimitorDenumire { get; set; }
    public decimal Total { get; set; }
    public int NrLinii { get; set; }
}

public sealed class LiniiSursaDto {
    public List<LinieSursaCandidataDto> Candidati { get; set; } = new();
    /// <summary>Plafonul de pagină a fost atins: mai există linii de clasă F în perioadă.</summary>
    public bool MaiSunt { get; set; }
}

public sealed class LinieSursaCandidataDto {
    public Guid LinieId { get; set; }
    public Guid DocumentId { get; set; }
    public string Numar { get; set; }
    public DateOnly Data { get; set; }
    public Guid PartenerId { get; set; }
    public string PartenerDenumire { get; set; }
    public string TipMaterialCod { get; set; }
    public string TipMaterialDenumire { get; set; }
    /// <summary>Denumirea produsului cules pe linia de factură; null fără produs.</summary>
    public string Descriere { get; set; }
    public decimal Cantitate { get; set; }
    public decimal Valoare { get; set; }
    /// <summary>Σ `Valoare` a liniilor PIF nestornate legate de linia asta.</summary>
    public decimal Consumat { get; set; }
    public decimal Rest { get; set; }
}
