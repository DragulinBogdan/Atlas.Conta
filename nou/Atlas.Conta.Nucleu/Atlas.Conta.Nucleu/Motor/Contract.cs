namespace Atlas.Conta.Nucleu;

public sealed record Contract {
    Contract() { }

    public Tranzactie? Tranzactie { get; private init; }

    public IReadOnlyList<Decizie> Decizii { get; private init; } = [];

    public IReadOnlyList<Ipoteza> Ipoteze { get; private init; } = [];

    public int JumatatiDeBan { get; private init; }

    public IReadOnlyList<Refuz> Refuzuri { get; private init; } = [];

    public bool EsteAcceptat => Refuzuri.Count == 0;

    public static Contract Accepta(
        Tranzactie tranzactie,
        IReadOnlyList<Decizie> decizii,
        IReadOnlyList<Ipoteza> ipoteze,
        int jumatatiDeBan) {
        ArgumentNullException.ThrowIfNull(tranzactie);
        ArgumentNullException.ThrowIfNull(decizii);
        ArgumentNullException.ThrowIfNull(ipoteze);
        return new Contract {
            Tranzactie = tranzactie,
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
        // Tranzacția lipsește: un contract refuzat nu lasă nimic de materializat (N-D11).
        return new Contract {
            Tranzactie = null,
            Decizii = decizii.ToArray(),
            Ipoteze = ipoteze.ToArray(),
            JumatatiDeBan = jumatatiDeBan,
            Refuzuri = refuzuri.ToArray(),
        };
    }

    public bool Equals(Contract? altul) =>
        altul is not null
        && Tranzactie == altul.Tranzactie
        && JumatatiDeBan == altul.JumatatiDeBan
        && Secvente.Egale(Decizii, altul.Decizii)
        && Secvente.Egale(Ipoteze, altul.Ipoteze)
        && Secvente.Egale(Refuzuri, altul.Refuzuri);

    public override int GetHashCode() {
        var cod = new HashCode();
        cod.Add(Tranzactie);
        cod.Add(JumatatiDeBan);
        Secvente.Adauga(ref cod, Decizii);
        Secvente.Adauga(ref cod, Ipoteze);
        Secvente.Adauga(ref cod, Refuzuri);
        return cod.ToHashCode();
    }
}
