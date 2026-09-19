namespace Atlas.Conta.Nucleu;

public sealed record Sold(decimal Debit, decimal Credit, decimal Cantitate, decimal ValoareValuta) {
    public static readonly Sold Zero = new(0m, 0m, 0m, 0m);

    // `with` ocolește constructorul; scara se apără și în `init` (N-D9).
    readonly decimal debit = LaScara(Debit, Scara.Bani, nameof(Debit));
    readonly decimal credit = LaScara(Credit, Scara.Bani, nameof(Credit));
    readonly decimal cantitate = LaScara(Cantitate, Scara.Cantitate, nameof(Cantitate));
    readonly decimal valoareValuta = LaScara(ValoareValuta, Scara.Bani, nameof(ValoareValuta));

    public decimal Debit { get => debit; init => debit = LaScara(value, Scara.Bani, nameof(Debit)); }
    public decimal Credit { get => credit; init => credit = LaScara(value, Scara.Bani, nameof(Credit)); }
    public decimal Cantitate { get => cantitate; init => cantitate = LaScara(value, Scara.Cantitate, nameof(Cantitate)); }
    public decimal ValoareValuta { get => valoareValuta; init => valoareValuta = LaScara(value, Scara.Bani, nameof(ValoareValuta)); }

    static decimal LaScara(decimal valoare, int scara, string nume) =>
        Scara.EsteLa(valoare, scara)
            ? valoare
            : throw new ArgumentException($"{nume} = {valoare} are mai multe zecimale decât scara {scara}.", nume);

    public decimal Net => Debit - Credit;

    public static Sold operator +(Sold unul, Sold altul) {
        ArgumentNullException.ThrowIfNull(unul);
        ArgumentNullException.ThrowIfNull(altul);
        return new Sold(
            unul.Debit + altul.Debit,
            unul.Credit + altul.Credit,
            unul.Cantitate + altul.Cantitate,
            unul.ValoareValuta + altul.ValoareValuta);
    }

    public static Sold Din(Postare postare) {
        ArgumentNullException.ThrowIfNull(postare);
        // Valoarea e semnată în Storno și Transfer: semnul se păstrează pe latura postării (N-D9).
        var peDebit = postare.Coordonate.Latura == Latura.Debit;
        return new Sold(
            peDebit ? postare.Valoare : 0m,
            peDebit ? 0m : postare.Valoare,
            postare.Cantitate,
            postare.ValoareValuta);
    }
}
