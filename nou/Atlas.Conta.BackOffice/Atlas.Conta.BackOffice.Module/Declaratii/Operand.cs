#nullable enable
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Motor;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.Module.Declaratii;

/// <summary>Felul repartitorului ca DATĂ: numele CLR al clasei lui (89).</summary>
public enum FelRepartitor {
    Partener = 1,
    Angajat = 2,
    Gestiune = 3,
    ContPropriu = 4,
    UnitateInterna = 5,
}

/// <summary>`Fel` null = repartitorul lipsește de pe document sau are o clasă necunoscută.</summary>
public sealed record RepartitorFapt(Guid Id, FelRepartitor? Fel, Guid? ContImplicitId, CalitateRepartitor Calitati) {
    /// <summary>Partea față de patrimoniu (T-D13): derivată o singură dată din `Fel`, prin <see cref="Laturi.ParteA"/>.</summary>
    public Parte? Parte => Laturi.ParteA(Fel);
}

public sealed record ContFapt(Guid Id, string? Simbol, bool UrmarestePartide, bool CereRepartitor);

/// <summary><paramref name="Origine"/> = documentul deschizător al partidei; gol la partida fără document.</summary>
public sealed record SoldPartidaFapt(N.Unitate Unitate, N.Sold Sold, decimal Disponibil, Guid Origine);

public readonly record struct CheieLotFapt(Guid Lot, Guid Cont, Guid Produs, Guid Gestiune);

public sealed record LotFapt(
    Guid Id,
    Guid ProdusId,
    Guid? TipMaterialId,
    Guid? ContImplicitId,
    DateOnly Data,
    decimal PretUnitar) {
    public Guid? LinieIntrareId { get; init; }
    public Guid? GestiuneId { get; init; }
}

public sealed record TransformareFapt(N.RolTransformare? Rol, decimal? PretProdus);

public sealed record DiferentaInventarFapt(DirectieDiferenta Directie, decimal? PretEvaluare);

public sealed record PoliticaTvaFapt(
    DirectieTva Directie,
    SursaCont SursaContrapartida,
    Guid? ContrapartidaFallbackId) {
    public Guid Id { get; init; }
    public int Versiune { get; init; }
}

public sealed record DocumentFapt(
    Guid Id,
    string? CodTip,
    Guid TipDocumentId,
    DateOnly Data,
    DateOnly DataInregistrare,
    string? Numar,
    bool Autogenerat,
    Guid? DocumentSursaId,
    RepartitorFapt Predator,
    RepartitorFapt Primitor,
    string? Valuta,
    decimal? Curs,
    DateOnly? DataScadenta);

public sealed record LinieOperand(
    Guid Id,
    Guid TipMaterialId,
    Guid? ClasaId,
    NaturaClasa? Natura,
    Guid? ContImplicitTipId,
    Guid? LotId,
    LotFapt? Lot,
    decimal Cantitate,
    decimal Valoare,
    decimal ValoareTva,
    Guid? TipTvaId,
    decimal? PretUnitar,
    Guid? ProdusId,
    Guid? TipProdusCulesId,
    Guid? ContDebitId,
    Guid? ContCreditId,
    Guid? RepartitorDebitId,
    Guid? RepartitorCreditId,
    N.Analiza Analiza,
    Guid? AngajamentId) {

    public TransformareFapt? Transformare { get; init; }
    public DiferentaInventarFapt? DiferentaInventar { get; init; }
    public ImobilizareCuleasa? Imobilizare { get; init; }

    /// <summary>Forma pe care o consumă `Potrivire` — aceeași ortografie ca `Fapte.Linie`.</summary>
    public LinieFapt Fapt => new(TipMaterialId, ClasaId, Natura, Math.Sign(Cantitate), LotId, ContImplicitTipId);
}

/// <summary>
/// Tot ce influențează rezultatul declarației, rezolvat și înghețat (090b):
/// declarantul nu mai citește nimic din afara lui.
/// </summary>
public sealed record Operand(
    DocumentFapt Document,
    IReadOnlyList<LinieOperand> Linii,
    IReadOnlyList<RegulaContareFapt> ReguliContare,
    IReadOnlyList<RegulaStocFapt> ReguliStoc,
    PoliticaTvaFapt? PoliticaTva,
    IReadOnlyDictionary<Guid, TipTvaFapt> TipuriTva,
    IReadOnlyDictionary<Guid, ContFapt> Conturi,
    IReadOnlyDictionary<CheieLotFapt, N.Sold> SolduriLoturi,
    decimal? RestPartidaSursa,
    IReadOnlyList<(Guid Cont, decimal Sold)> PartideSursa,
    DateOnly? DataInregistrareSursa,
    int? PerioadaDeclarare,
    decimal? TolerantaTaxa,
    N.PerioadaDeschisa PerioadaDeschisa) {

    public ReceptieSursaFapt? ReceptieSursa { get; init; }
    public N.ReperFiscal? ReperFiscal { get; init; }
    public IReadOnlyDictionary<Guid, RepartitorFapt> Repartitori { get; init; } = new Dictionary<Guid, RepartitorFapt>();
    public IReadOnlyList<SoldPartidaFapt> PartideDisponibile { get; init; } = [];
    public IReadOnlyList<N.Unitate> UnitatiSursa { get; init; } = [];
    /// <summary>Minimul disponibil pe cont peste datele deja scrise, separat de soldul citit.</summary>
    public IReadOnlyDictionary<Guid, decimal> DisponibilPartideSursa { get; init; } = new Dictionary<Guid, decimal>();
    public IReadOnlyDictionary<Guid, FisaFapt> Fise { get; init; } = new Dictionary<Guid, FisaFapt>();
    public IReadOnlyList<SuportFapt> Suporturi { get; init; } = [];
    public IReadOnlyList<DisponibilFapt> DisponibilNominalizare { get; init; } = [];

    /// <summary>Forma pe care o consumă `Potrivire.Cont`.</summary>
    public LaturiFapt Laturi => new(Document.Predator.ContImplicitId, Document.Primitor.ContImplicitId);
}
