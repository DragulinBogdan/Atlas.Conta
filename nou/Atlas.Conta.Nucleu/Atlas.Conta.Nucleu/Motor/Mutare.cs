namespace Atlas.Conta.Nucleu;

public sealed record Mutare(
    Capat DeLa,
    Capat La,
    Latura Latura,
    decimal Cantitate,
    decimal ValoareValuta,
    decimal Valoare,
    Cauza Cauza) {

    // 090f: transferul conservă pe (Cont, Latura), deci cele două postări stau pe ACEEAȘI latură.
    public static (Postare Iesire, Postare Intrare) Postari(Mutare mutare, DateOnly data) {
        ArgumentNullException.ThrowIfNull(mutare);
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
                mutare.Cauza),
            new Postare(
                mutare.La.Pe(mutare.Latura, data),
                mutare.Cantitate,
                mutare.ValoareValuta,
                mutare.Valoare,
                mutare.Cauza));
    }
}
