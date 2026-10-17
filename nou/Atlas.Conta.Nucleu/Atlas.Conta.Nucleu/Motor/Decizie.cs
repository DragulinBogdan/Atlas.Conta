namespace Atlas.Conta.Nucleu;

public abstract record Decizie {
    // Ierarhie închisă (N-D11): singurul constructor e vizibil doar în assembly.
    private protected Decizie() { }
}

public sealed record AlocareFifo(Guid Linie, UnitateSold Unitate, decimal Masura) : Decizie;

public sealed record ValoareIesire(Guid Linie, UnitateSold Unitate, decimal Cantitate, decimal Valoare) : Decizie;

/// <summary>Ieșirea a cărei valoare nu vine din soldul unității: o dă sursa numită; <paramref name="Corectie"/> e cota
/// corecțiilor de valoare ale unității, scoasă fără cantitate lângă ieșire (CV-D10); <paramref name="Reziduu"/> e valoarea
/// rămasă pe unitate la golirea ei, scoasă fără cantitate spre politica de diferențe (T-r2).</summary>
public sealed record ValoareDeclarata(Guid Linie, UnitateSold Unitate, decimal Cantitate, decimal Valoare, string Sursa,
    decimal Corectie = 0m, decimal Reziduu = 0m) : Decizie;

public sealed record PartidaDeschisa(Guid Linie, UnitateSold Unitate) : Decizie;

/// <summary>Cota unui beneficiar din valoarea liniei de corecție; fără postare când <paramref name="Valoare"/> e zero (CV-D8).</summary>
public sealed record AlocareValorica(Guid Linie, UnitateSold Unitate, ReferintaPostare Beneficiar, decimal Pondere, decimal Valoare) : Decizie;

/// <summary>Mutarea alocării active a rădăcinii de pe consumul inversat pe stoc (CV-D8).</summary>
public sealed record CompensareValorica(
    Guid Linie, ReferintaPostare Radacina, ReferintaPostare Beneficiar, UnitateSold Sursa, UnitateSold Destinatie, decimal Valoare) : Decizie;

/// <summary><paramref name="Regula"/>: regula de contare care a dat contul, când vine dintr-una (D9-A8).</summary>
public sealed record ContRezolvat(Guid Linie, Guid Cont, string Sursa, Guid? Regula = null) : Decizie;
