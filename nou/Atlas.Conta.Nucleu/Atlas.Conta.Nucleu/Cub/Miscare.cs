namespace Atlas.Conta.Nucleu;

public sealed record Miscare(
    Capat DeLa,
    Capat La,
    decimal Cantitate,
    decimal ValoareValuta,
    decimal Valoare,
    Cauza Cauza) {

    public static (Postare Debit, Postare Credit) Postari(Miscare miscare, DateOnly data, int pereche) {
        ArgumentNullException.ThrowIfNull(miscare);
        if (pereche < 1)
            throw new ArgumentOutOfRangeException(nameof(pereche), pereche, "ordinalul perechii începe de la 1.");
        return (
            new Postare(
                miscare.La.Pe(Latura.Debit, data),
                miscare.Cantitate,
                miscare.ValoareValuta,
                miscare.Valoare,
                miscare.Cauza) { Pereche = pereche },
            new Postare(
                miscare.DeLa.Pe(Latura.Credit, data),
                -miscare.Cantitate,
                miscare.ValoareValuta,
                miscare.Valoare,
                miscare.Cauza) { Pereche = pereche });
    }
}
