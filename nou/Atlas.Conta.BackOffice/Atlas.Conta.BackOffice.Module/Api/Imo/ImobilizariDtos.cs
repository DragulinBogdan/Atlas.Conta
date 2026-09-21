namespace Atlas.Conta.BackOffice.Module.Api.Imo;

// Cele două proiecții peste `RegistruImobilizari`: fișa și registrul (F26-D10).
// Nomenclatorul `Imobilizare` e pe OData (F2-D4), nu aici.

/// <summary>Oglinda plată a lui `AmortizareService.SituatieImobilizare`; enum-urile ca string (57a).</summary>
public sealed class SituatieImobilizareDto {
    public decimal Valoare { get; set; }
    public decimal ValoareFiscala { get; set; }
    public decimal Amortizare { get; set; }
    public decimal AmortizareFiscala { get; set; }
    public decimal AmortizareDeductibila { get; set; }
    public decimal NetContabil { get; set; }
    public decimal NetFiscal { get; set; }
    public int Luni { get; set; }
    public string Metoda { get; set; }
    public int? DurataLuni { get; set; }
    public decimal? ValoareReziduala { get; set; }
    public string MetodaFiscala { get; set; }
    public int? DurataFiscalaLuni { get; set; }
    public string CategorieFiscala { get; set; }
    public bool? UtilizareExclusiva { get; set; }
    /// <summary>Data ultimului eveniment ≤ dată: baza de la care curge cota.</summary>
    public DateOnly? DataUltimEveniment { get; set; }
}

/// <summary>Un rând de registru; `DocumentTip` e codul ancorei (PIF/CAS/AMO).</summary>
public sealed class RandImobilizareDto {
    public Guid Id { get; set; }
    public DateOnly Data { get; set; }
    public string Fel { get; set; }
    public bool Storno { get; set; }
    public Guid DocumentId { get; set; }
    public string DocumentNumar { get; set; }
    public string DocumentTip { get; set; }
    public decimal Valoare { get; set; }
    public decimal ValoareFiscala { get; set; }
    public decimal Amortizare { get; set; }
    public decimal AmortizareFiscala { get; set; }
    public decimal AmortizareDeductibila { get; set; }
    public int Luni { get; set; }
    public string Metoda { get; set; }
    public int? DurataLuni { get; set; }
    public decimal? ValoareReziduala { get; set; }
    public string MetodaFiscala { get; set; }
    public int? DurataFiscalaLuni { get; set; }
    public string CategorieFiscala { get; set; }
    public bool? UtilizareExclusiva { get; set; }
}

// Banda catalogului e în LUNI (ani × 12), ca duratele culese pe linia de PIF.
public sealed class FisaImobilizareDto {
    public Guid Id { get; set; }
    public string NumarInventar { get; set; }
    public string Denumire { get; set; }
    public string TipMaterialCod { get; set; }
    public string TipMaterialDenumire { get; set; }
    public string ClasificareCod { get; set; }
    public string ClasificareDenumire { get; set; }
    public string LocDenumire { get; set; }
    public string CentruCostDenumire { get; set; }
    public string CodEconomicCod { get; set; }
    public string ResponsabilNume { get; set; }
    public string Stare { get; set; }
    public DateOnly? DataPunereInFunctiune { get; set; }
    public DateOnly? DataIesire { get; set; }
    /// <summary>Banda duratei fiscale; null fără clasificare sau fără bandă (F26-D4).</summary>
    public int? DurataFiscalaMinLuni { get; set; }
    public int? DurataFiscalaMaxLuni { get; set; }
    public DateOnly LaData { get; set; }
    public SituatieImobilizareDto Situatie { get; set; }
    public List<RandImobilizareDto> Randuri { get; set; } = new();
}

// O linie per fișă pusă în funcțiune, la o dată.
public sealed class RandRegistruImobilizariDto {
    public Guid ImobilizareId { get; set; }
    public string NumarInventar { get; set; }
    public string Denumire { get; set; }
    public string TipMaterialCod { get; set; }
    public string LocDenumire { get; set; }
    public string Stare { get; set; }
    public decimal Brut { get; set; }
    public decimal BrutFiscal { get; set; }
    public decimal AmortizareCumulata { get; set; }
    public decimal AmortizareFiscalaCumulata { get; set; }
    public decimal DeductibilCumulat { get; set; }
    public decimal NetContabil { get; set; }
    public decimal NetFiscal { get; set; }
    public int Luni { get; set; }
}

public sealed class RegistruImobilizariDto {
    public DateOnly LaData { get; set; }
    public List<RandRegistruImobilizariDto> Linii { get; set; } = new();
    public decimal TotalBrut { get; set; }
    public decimal TotalBrutFiscal { get; set; }
    public decimal TotalAmortizareCumulata { get; set; }
    public decimal TotalAmortizareFiscalaCumulata { get; set; }
    public decimal TotalDeductibilCumulat { get; set; }
    public decimal TotalNetContabil { get; set; }
    public decimal TotalNetFiscal { get; set; }
}
