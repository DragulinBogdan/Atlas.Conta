namespace Atlas.Conta.BackOffice.Module.DatabaseUpdate;

// Auditul re-seed-ului (83d): ușa de sistem nu trece prin AuditTrail, deci
// consola e singura urmă a corecțiilor. Contoarele per tabel și lista
// corecțiilor sunt parte din contract — ModelCheck le citește de aici.
public sealed class RaportSeed {
    public sealed record Corectie(string Tip, string Cheie, string Camp, string Vechi, string Nou);

    public sealed class Contor {
        public int Create;
        public int Corectate;
        public int Manuale;
        public int Sterse;
        public int CheiDiferite;
    }

    readonly Dictionary<string, Contor> peTip = new(StringComparer.Ordinal);
    readonly List<Corectie> corectii = [];

    public IReadOnlyList<Corectie> Corectii => corectii;

    /// <summary>Contoarele tabelului, zero pe un tip pe care seed-ul nu l-a atins.</summary>
    public Contor Pentru(string tip) => peTip.TryGetValue(tip, out var c) ? c : new Contor();

    public int TotalCreate => peTip.Values.Sum(c => c.Create);
    public int TotalCorectate => peTip.Values.Sum(c => c.Corectate);

    internal Contor Contoare(string tip) {
        if (!peTip.TryGetValue(tip, out var c))
            peTip[tip] = c = new Contor();
        return c;
    }

    internal void Corectat(string tip, string cheie, string camp, string vechi, string nou) {
        corectii.Add(new Corectie(tip, cheie, camp, vechi, nou));
        Console.WriteLine($"  {tip} / {cheie} / {camp}: {vechi} → {nou}");
    }

    public void Tipareste() {
        foreach (var (tip, c) in peTip.OrderBy(x => x.Key, StringComparer.Ordinal))
            Console.WriteLine($"Seed {tip}: {c.Create} create, {c.Corectate} corectate, "
                + $"{c.Manuale} manuale pe cheie de seed, {c.Sterse} șterse"
                + (c.CheiDiferite > 0 ? $", {c.CheiDiferite} pe cheie diferită" : "") + ".");
        Console.WriteLine($"Seed TOTAL: {TotalCreate} create, "
            + $"{TotalCorectate} corectate, "
            + $"{peTip.Values.Sum(c => c.Manuale)} manuale pe cheie de seed, "
            + $"{peTip.Values.Sum(c => c.Sterse)} șterse.");
    }
}
