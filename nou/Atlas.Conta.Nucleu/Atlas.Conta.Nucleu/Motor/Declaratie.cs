namespace Atlas.Conta.Nucleu;

public sealed record Declaratie(
    Guid Document,
    DateOnly Data,
    IReadOnlyList<Miscare> Miscari,
    IReadOnlyList<Decizie> Decizii,
    IReadOnlyList<Ipoteza> Ipoteze) {

    // `with` ocolește constructorul; cerința se apără și în `init` (N-D11).
    readonly Guid document = Document;
    readonly IReadOnlyList<Miscare> miscari = Cauzate(Document, Miscari, nameof(Miscari));
    readonly IReadOnlyList<Decizie> decizii = Ceruta(Decizii, nameof(Decizii));
    readonly IReadOnlyList<Ipoteza> ipoteze = Ceruta(Ipoteze, nameof(Ipoteze));

    public Guid Document {
        get => document;
        init {
            document = value;
            Cauzate(value, miscari, nameof(Miscari));
        }
    }

    public IReadOnlyList<Miscare> Miscari {
        get => miscari;
        init => miscari = Cauzate(document, value, nameof(Miscari));
    }

    public IReadOnlyList<Decizie> Decizii {
        get => decizii;
        init => decizii = Ceruta(value, nameof(Decizii));
    }

    public IReadOnlyList<Ipoteza> Ipoteze {
        get => ipoteze;
        init => ipoteze = Ceruta(value, nameof(Ipoteze));
    }

    public bool Equals(Declaratie? alta) =>
        alta is not null
        && Document == alta.Document
        && Data == alta.Data
        && Secvente.Egale(Miscari, alta.Miscari)
        && Secvente.Egale(Decizii, alta.Decizii)
        && Secvente.Egale(Ipoteze, alta.Ipoteze);

    public override int GetHashCode() {
        var cod = new HashCode();
        cod.Add(Document);
        cod.Add(Data);
        Secvente.Adauga(ref cod, Miscari);
        Secvente.Adauga(ref cod, Decizii);
        Secvente.Adauga(ref cod, Ipoteze);
        return cod.ToHashCode();
    }

    static IReadOnlyList<Miscare> Cauzate(Guid document, IReadOnlyList<Miscare> miscari, string nume) {
        ArgumentNullException.ThrowIfNull(miscari, nume);
        if (miscari.Count == 0)
            throw new ArgumentException("declarația se cere cu cel puțin o mișcare.", nume);
        foreach (var miscare in miscari) {
            ArgumentNullException.ThrowIfNull(miscare, nume);
            if (miscare.Cauza.Document != document)
                throw new ArgumentException(
                    $"mișcarea are cauza pe documentul {miscare.Cauza.Document}, nu pe {document}.",
                    nume);
        }
        return miscari;
    }

    static IReadOnlyList<T> Ceruta<T>(IReadOnlyList<T> lista, string nume) {
        ArgumentNullException.ThrowIfNull(lista, nume);
        return lista;
    }
}
