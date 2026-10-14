namespace Atlas.Conta.Nucleu;

public sealed record Miscare(
    Capat DeLa,
    Capat La,
    decimal Cantitate,
    decimal ValoareValuta,
    decimal Valoare,
    Cauza Cauza) {

    /// <summary>Postarea căreia i se atribuie capătul <see cref="La"/>.</summary>
    public Guid? AtribuitLa { get; init; }

    /// <summary>Postarea căreia i se atribuie capătul <see cref="DeLa"/>.</summary>
    public Guid? AtribuitDeLa { get; init; }

    public static (Postare Debit, Postare Credit) Postari(Miscare miscare, DateOnly data, int pereche) {
        ArgumentNullException.ThrowIfNull(miscare);
        if (pereche < 1)
            throw new ArgumentOutOfRangeException(nameof(pereche), pereche, "ordinalul perechii începe de la 1.");
        return (
            new Postare(
                miscare.La.Pe(Sens.Debit, data),
                miscare.Cantitate,
                miscare.ValoareValuta,
                miscare.Valoare,
                miscare.Cauza,
                miscare.AtribuitLa) { Pereche = pereche },
            new Postare(
                miscare.DeLa.Pe(Sens.Credit, data),
                -miscare.Cantitate,
                miscare.ValoareValuta,
                miscare.Valoare,
                miscare.Cauza,
                miscare.AtribuitDeLa) { Pereche = pereche });
    }
}
