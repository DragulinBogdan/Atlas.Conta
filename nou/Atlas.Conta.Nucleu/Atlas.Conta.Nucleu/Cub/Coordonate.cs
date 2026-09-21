namespace Atlas.Conta.Nucleu;

public sealed record Coordonate {
    public required Guid Cont { get; init; }
    public required Latura Latura { get; init; }
    public required DateOnly Data { get; init; }
    public Guid? Partener { get; init; }
    public Guid? Gestiune { get; init; }
    public Guid? Produs { get; init; }
    public Unitate? Unitate { get; init; }
    public CodTva? CodTva { get; init; }
    public int? PerioadaDeclarare { get; init; }
    public Guid? Valuta { get; init; }
    public Carte Carte { get; init; } = Carte.Contabil;
    public Analiza Analiza { get; init; } = Analiza.Fara;
}
