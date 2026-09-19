namespace Atlas.Conta.Nucleu;

public sealed record Capat {
    public required Guid Cont { get; init; }
    public Guid? Partener { get; init; }
    public Guid? Gestiune { get; init; }
    public Guid? Produs { get; init; }
    public Unitate? Unitate { get; init; }
    public CodTva? CodTva { get; init; }
    public int? PerioadaDeclarare { get; init; }
    public Guid? Valuta { get; init; }
    public Carte Carte { get; init; } = Carte.Contabil;
    public Analiza Analiza { get; init; } = Analiza.Fara;

    public Coordonate Pe(Latura latura, DateOnly data) => new() {
        Cont = Cont,
        Latura = latura,
        Data = data,
        Partener = Partener,
        Gestiune = Gestiune,
        Produs = Produs,
        Unitate = Unitate,
        CodTva = CodTva,
        PerioadaDeclarare = PerioadaDeclarare,
        Valuta = Valuta,
        Carte = Carte,
        Analiza = Analiza,
    };
}
