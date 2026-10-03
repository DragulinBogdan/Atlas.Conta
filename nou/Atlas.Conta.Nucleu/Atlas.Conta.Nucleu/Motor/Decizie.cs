namespace Atlas.Conta.Nucleu;

public abstract record Decizie {
    // Ierarhie închisă (N-D11): singurul constructor e vizibil doar în assembly.
    private protected Decizie() { }
}

public sealed record AlocareFifo(Guid Linie, Unitate Unitate, decimal Masura) : Decizie;

public sealed record ValoareIesire(Guid Linie, Unitate Unitate, decimal Cantitate, decimal Valoare) : Decizie;

/// <summary>Ieșirea a cărei valoare nu vine din soldul unității: o dă sursa numită.</summary>
public sealed record ValoareDeclarata(Guid Linie, Unitate Unitate, decimal Cantitate, decimal Valoare, string Sursa) : Decizie;

public sealed record PartidaDeschisa(Guid Linie, Unitate Unitate) : Decizie;

public sealed record ContRezolvat(Guid Linie, Guid Cont, string Sursa) : Decizie;

public sealed record AbsorbtieEvaluare(Guid Document, Guid Linie, Guid ContSursa,
    decimal R, decimal C, decimal P, decimal Delta) : Decizie;
