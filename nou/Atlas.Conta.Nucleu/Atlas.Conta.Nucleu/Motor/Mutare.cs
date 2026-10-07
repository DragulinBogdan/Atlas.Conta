namespace Atlas.Conta.Nucleu;

public sealed record Mutare(
    Capat DeLa,
    Capat La,
    Latura Latura,
    decimal Cantitate,
    decimal ValoareValuta,
    decimal Valoare,
    Cauza Cauza) {

    public ReferintaPostare? Suport { get; init; }

    // 090f: transferul conservă pe (Cont, Latura), deci cele două postări stau pe ACEEAȘI latură.
    public static (Postare Iesire, Postare Intrare) Postari(Mutare mutare, DateOnly data, int pereche) {
        ArgumentNullException.ThrowIfNull(mutare);
        if (pereche < 1)
            throw new ArgumentOutOfRangeException(nameof(pereche), pereche, "ordinalul perechii începe de la 1.");
        if (mutare.DeLa.Cont != mutare.La.Cont)
            throw new ArgumentException(
                $"transferul mută între contul {mutare.DeLa.Cont} și contul {mutare.La.Cont}.",
                nameof(mutare));
        return (
            new Postare(
                mutare.DeLa.Pe(mutare.Latura, data),
                -mutare.Cantitate,
                -mutare.ValoareValuta,
                -mutare.Valoare,
                mutare.Cauza) { Suport = mutare.Suport, Pereche = pereche },
            new Postare(
                mutare.La.Pe(mutare.Latura, data),
                mutare.Cantitate,
                mutare.ValoareValuta,
                mutare.Valoare,
                mutare.Cauza) { Suport = mutare.Suport, Pereche = pereche });
    }
}
