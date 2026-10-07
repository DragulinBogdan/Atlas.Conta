namespace Atlas.Conta.Nucleu;

public abstract record Ipoteza {
    // Ierarhie închisă (N-D11): singurul constructor e vizibil doar în assembly.
    private protected Ipoteza() { }
}

public sealed record SoldUnitateCitit(Unitate Unitate, Sold Sold) : Ipoteza;

public sealed record PerioadaDeschisa(int An, int Luna) : Ipoteza;

/// <summary>Rândul de politică consumat la o postare: felul (clasa rândului), identificatorul și contorul lui la operare (D9-A8).</summary>
public sealed record VersiunePolitica(string Fel, Guid Rand, int Versiune) : Ipoteza;
