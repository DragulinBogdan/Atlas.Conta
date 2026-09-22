namespace Atlas.Conta.Nucleu;

public sealed record Declaratie(
    Guid Document,
    DateOnly Data,
    IReadOnlyList<Miscare> Miscari,
    IReadOnlyList<Mutare> Mutari,
    IReadOnlyList<Decizie> Decizii,
    IReadOnlyList<Ipoteza> Ipoteze) {

    public Declaratie(
            Guid document,
            DateOnly data,
            IReadOnlyList<Miscare> miscari,
            IReadOnlyList<Decizie> decizii,
            IReadOnlyList<Ipoteza> ipoteze)
        : this(document, data, miscari, [], decizii, ipoteze) { }

    // `with` ocolește constructorul; cerința se apără și în `init` (N-D11).
    readonly Guid document = Document;
    readonly IReadOnlyList<Miscare> miscari = Cauzate(Document, Miscari, m => m.Cauza, nameof(Miscari));
    readonly IReadOnlyList<Mutare> mutari = CelPutinUna(
        Miscari.Count, Cauzate(Document, Mutari, m => m.Cauza, nameof(Mutari)));
    readonly IReadOnlyList<Decizie> decizii = Ceruta(Decizii, nameof(Decizii));
    readonly IReadOnlyList<Ipoteza> ipoteze = Ceruta(Ipoteze, nameof(Ipoteze));

    public Guid Document {
        get => document;
        init {
            document = value;
            Verifica(value, miscari, m => m.Cauza, nameof(Miscari));
            Verifica(value, mutari, m => m.Cauza, nameof(Mutari));
        }
    }

    public IReadOnlyList<Miscare> Miscari {
        get => miscari;
        init {
            var noi = Cauzate(document, value, m => m.Cauza, nameof(Miscari));
            CereCelPutinUna(noi.Count + mutari.Count);
            miscari = noi;
        }
    }

    public IReadOnlyList<Mutare> Mutari {
        get => mutari;
        init => mutari = CelPutinUna(
            miscari.Count, Cauzate(document, value, m => m.Cauza, nameof(Mutari)));
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
        && Secvente.Egale(Mutari, alta.Mutari)
        && Secvente.Egale(Decizii, alta.Decizii)
        && Secvente.Egale(Ipoteze, alta.Ipoteze);

    public override int GetHashCode() {
        var cod = new HashCode();
        cod.Add(Document);
        cod.Add(Data);
        Secvente.Adauga(ref cod, Miscari);
        Secvente.Adauga(ref cod, Mutari);
        Secvente.Adauga(ref cod, Decizii);
        Secvente.Adauga(ref cod, Ipoteze);
        return cod.ToHashCode();
    }

    static IReadOnlyList<T> Cauzate<T>(
            Guid document, IReadOnlyList<T> elemente, Func<T, Cauza> cauza, string nume) {
        Verifica(document, elemente, cauza, nume);
        return [.. elemente];
    }

    static void Verifica<T>(Guid document, IReadOnlyList<T> elemente, Func<T, Cauza> cauza, string nume) {
        ArgumentNullException.ThrowIfNull(elemente, nume);
        foreach (var element in elemente) {
            ArgumentNullException.ThrowIfNull(element, nume);
            if (cauza(element).Document != document)
                throw new ArgumentException(
                    $"{nume}: cauza e pe documentul {cauza(element).Document}, nu pe {document}.",
                    nume);
        }
    }

    // T-D2: declarația e numai `Miscari` (`Operare`), numai `Mutari` (`Transfer`) sau ambele.
    static IReadOnlyList<Mutare> CelPutinUna(int cateMiscari, IReadOnlyList<Mutare> mutari) {
        CereCelPutinUna(cateMiscari + mutari.Count);
        return mutari;
    }

    static void CereCelPutinUna(int cate) {
        if (cate == 0)
            throw new ArgumentException(
                "declarația se cere cu cel puțin o mișcare sau o mutare.", nameof(Miscari));
    }

    static IReadOnlyList<T> Ceruta<T>(IReadOnlyList<T> lista, string nume) {
        ArgumentNullException.ThrowIfNull(lista, nume);
        return [.. lista];
    }
}
