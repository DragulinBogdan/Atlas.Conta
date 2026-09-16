namespace Atlas.Conta.BackOffice.Module.Api.Amo;

// Amortizarea lunară: document GENERAT, pe tiparul ITV — fără `WriteDto` (F26-D7/D10).
// Cele trei cifre ale lunii sunt trei coloane ale aceleiași linii; doar contabilul postează.

// Linia care nu postează (contabil 0, fiscal > 0) rămâne FĂRĂ conturi (F26-D7).
public sealed class LinieAmoDto {
    /// <summary>Null pe previzualizare: linia încă nu există ca detaliu.</summary>
    public Guid? Id { get; set; }
    public Guid ImobilizareId { get; set; }
    public string NumarInventar { get; set; }
    public string Denumire { get; set; }
    public Guid TipMaterialId { get; set; }
    public decimal Contabil { get; set; }
    public decimal Fiscal { get; set; }
    public decimal Deductibil { get; set; }
    /// <summary>Lunile acoperite de sumele liniei: > 1 pe recuperarea unei puneri în funcțiune întârziate.</summary>
    public int Luni { get; set; }
    public Guid? ContCheltuialaId { get; set; }
    public string ContCheltuialaSimbol { get; set; }
    public Guid? ContAmortizareId { get; set; }
    public string ContAmortizareSimbol { get; set; }
    public Guid? LocId { get; set; }
    public string LocDenumire { get; set; }
    public Guid? CentruCostId { get; set; }
    public Guid? CodEconomicId { get; set; }
}

// `Motiv` null = luna se poate genera; altfel numele membrului `MotivNegenerare` (57a)
// plus eticheta lui din model.
public sealed class PrevizualizareAmoDto {
    public int An { get; set; }
    public int Luna { get; set; }
    public string Motiv { get; set; }
    public string MotivEticheta { get; set; }
    /// <summary>Documentul care blochează luna (`AmortizareVie`/`NeCronologica`/`DraftAnterior`).</summary>
    public Guid? BlocantId { get; set; }
    public string BlocantNumar { get; set; }
    public string BlocantStare { get; set; }
    /// <summary>Numărul de inventar al fișei vinovate la `FisaFaraPolitica`.</summary>
    public string Detaliu { get; set; }
    public List<LinieAmoDto> Linii { get; set; } = new();
    public decimal TotalContabil { get; set; }
    public decimal TotalFiscal { get; set; }
    public decimal TotalDeductibil { get; set; }
}

public sealed class AmoReadDto {
    public Guid Id { get; set; }
    public string Numar { get; set; }
    public DateOnly Data { get; set; }
    public DateOnly DataInregistrare { get; set; }
    public int An { get; set; }
    public int Luna { get; set; }
    public string Stare { get; set; }
    public DateTime? DataOperare { get; set; }
    public Guid UnitateId { get; set; }
    public string UnitateDenumire { get; set; }
    public List<LinieAmoDto> Linii { get; set; } = new();
    public decimal TotalContabil { get; set; }
    public decimal TotalFiscal { get; set; }
    public decimal TotalDeductibil { get; set; }

    /// <summary>Anti-stale, doar pe Draft: criteriul gardianului (`AmortizareLunara.LiniileCorespund`).</summary>
    public bool? Stale { get; set; }

    public bool PoateOpera { get; set; }
    public bool PoateAnula { get; set; }
    // F27-D6 — legătura de corecție (null = documentul nu corectează nimic).
    public CorectieDto Corectie { get; set; }
    public bool PoateStorna { get; set; }
    public bool PoateSterge { get; set; }
    public bool PoateRegenera { get; set; }
}

public sealed class AmoListDto {
    public Guid Id { get; set; }
    public string Numar { get; set; }
    public DateOnly Data { get; set; }
    public int An { get; set; }
    public int Luna { get; set; }
    public string Stare { get; set; }
    public DateTime? DataOperare { get; set; }
    public string UnitateDenumire { get; set; }
    public int NrLinii { get; set; }
    public decimal TotalContabil { get; set; }
    public decimal TotalFiscal { get; set; }
    public decimal TotalDeductibil { get; set; }
}

// Unitatea internă e parametru CULES (F21-D4); `[Range]` ⇒ 400 `EroriDto` din pipeline (70f).
public sealed class GenerareAmoRequestDto {
    [System.ComponentModel.DataAnnotations.Range(2000, 2100)]
    public int An { get; set; }
    [System.ComponentModel.DataAnnotations.Range(1, 12)]
    public int Luna { get; set; }
    public Guid UnitateId { get; set; }
}

// RAPORT, nu succes/eșec: `DocumentId` null cu `Motiv` completat e un răspuns valid (58, 72e).
public sealed class GenerareAmoRezultatDto {
    public Guid? DocumentId { get; set; }
    public string Motiv { get; set; }
    public string MotivEticheta { get; set; }
    public Guid? BlocantId { get; set; }
    public string Detaliu { get; set; }
}
