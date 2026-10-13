namespace Atlas.Conta.Nucleu;

public sealed record Coordonate {
    public required Guid Cont { get; init; }
    public required Sens Sens { get; init; }
    public required DateOnly Data { get; init; }
    public Repartitor? Repartitor { get; init; }
    public Contrapondere? Contrapondere { get; init; }
    public Guid? Produs { get; init; }
    public UnitateSold? Unitate { get; init; }
    public CodTva? CodTva { get; init; }
    public int? PerioadaDeclarare { get; init; }
    public ReperFiscal? ReperFiscal { get; init; }
    public Guid? PartenerFiscal { get; init; }
    public Guid? Loc { get; init; }
    public Guid? Valuta { get; init; }
    public Carte Carte { get; init; } = Carte.Contabil;
    public Analiza Analiza { get; init; } = Analiza.Fara;
}
