namespace Atlas.Conta.Nucleu;

public sealed record Contract {
    Contract() { }

    /// <summary>T-D2: 1 sau 2 tranzacții, în ordinea <c>Operare</c>, <c>Transfer</c>.</summary>
    public IReadOnlyList<Tranzactie> Tranzactii { get; private init; } = [];

    public IReadOnlyList<Decizie> Decizii { get; private init; } = [];

    public IReadOnlyList<Ipoteza> Ipoteze { get; private init; } = [];

    public int JumatatiDeBan { get; private init; }

    public IReadOnlyList<Refuz> Refuzuri { get; private init; } = [];

    public bool EsteAcceptat => Refuzuri.Count == 0;

    public static Contract Accepta(
        IReadOnlyList<Tranzactie> tranzactii,
        IReadOnlyList<Decizie> decizii,
        IReadOnlyList<Ipoteza> ipoteze,
        int jumatatiDeBan) {
        ArgumentNullException.ThrowIfNull(tranzactii);
        ArgumentNullException.ThrowIfNull(decizii);
        ArgumentNullException.ThrowIfNull(ipoteze);
        if (tranzactii.Count == 0)
            throw new ArgumentException(
                "contractul acceptat se cere cu cel puțin o tranzacție.", nameof(tranzactii));
        return new Contract {
            Tranzactii = tranzactii.ToArray(),
            Decizii = decizii.ToArray(),
            Ipoteze = ipoteze.ToArray(),
            JumatatiDeBan = jumatatiDeBan,
            Refuzuri = [],
        };
    }

    public static Contract Refuza(
        IReadOnlyList<Refuz> refuzuri,
        IReadOnlyList<Decizie> decizii,
        IReadOnlyList<Ipoteza> ipoteze,
        int jumatatiDeBan) {
        ArgumentNullException.ThrowIfNull(refuzuri);
        ArgumentNullException.ThrowIfNull(decizii);
        ArgumentNullException.ThrowIfNull(ipoteze);
        if (refuzuri.Count == 0)
            throw new ArgumentException("contractul refuzat se cere cu cel puțin un refuz.", nameof(refuzuri));
        // Tranzacțiile lipsesc: un contract refuzat nu lasă nimic de materializat (N-D11).
        return new Contract {
            Tranzactii = [],
            Decizii = decizii.ToArray(),
            Ipoteze = ipoteze.ToArray(),
            JumatatiDeBan = jumatatiDeBan,
            Refuzuri = refuzuri.ToArray(),
        };
    }

    public bool Equals(Contract? altul) =>
        altul is not null
        && JumatatiDeBan == altul.JumatatiDeBan
        && Secvente.Egale(Tranzactii, altul.Tranzactii)
        && Secvente.Egale(Decizii, altul.Decizii)
        && Secvente.Egale(Ipoteze, altul.Ipoteze)
        && Secvente.Egale(Refuzuri, altul.Refuzuri);

    public override int GetHashCode() {
        var cod = new HashCode();
        cod.Add(JumatatiDeBan);
        Secvente.Adauga(ref cod, Tranzactii);
        Secvente.Adauga(ref cod, Decizii);
        Secvente.Adauga(ref cod, Ipoteze);
        Secvente.Adauga(ref cod, Refuzuri);
        return cod.ToHashCode();
    }
}
