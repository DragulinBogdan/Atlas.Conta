namespace Atlas.Conta.Nucleu;

public abstract record Ipoteza {
    // Ierarhie închisă (N-D11): singurul constructor e vizibil doar în assembly.
    private protected Ipoteza() { }
}

public sealed record SoldUnitateCitit(Unitate Unitate, Sold Sold) : Ipoteza;

public sealed record PerioadaDeschisa(int An, int Luna) : Ipoteza;

public sealed record VersiunePolitica(string Nume, DateOnly ValabilDeLa) : Ipoteza;
