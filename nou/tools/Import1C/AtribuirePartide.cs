using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Cub;
using DevExpress.ExpressApp;
using N = Atlas.Conta.Nucleu;

namespace Import1C;

// Contractul 5, pe document (132 e): diferența unei partide inițiale se împarte pe documentele sursei care o mișcă;
// fiecare parte primește numele felului ei sau rămâne eșec.
static class AtribuirePartide {
    const decimal Eps = 0.005m;
    static readonly HashSet<string> Note = new(StringComparer.Ordinal) { "Operatia", "Compensare" };
    static readonly HashSet<string> Stingatori = new(StringComparer.Ordinal) {
        "ExtrasDeCont", "Plata", "Incasare", "IncasareCard", "Compensare", "RaportDeVanzariCuAmanunt",
    };

    public const string DocumentPropriu = "rândul sursei e pe poziție, Atlas îl ține pe partida documentului (107-r1)";
    public const string FifoNumire = "notă Atlas pe FIFO, sursa numește un document fără poziție (E4-r1)";
    public const string StingereNeaplicata = "stingere refuzată sau plafonată de trecerea 2 (107h)";

    public sealed record Parte(string Recorder, string View, decimal Sursa, decimal Atlas, string Categorie) {
        public decimal Delta => Atlas - Sursa;
    }

    public sealed record Rezultat(IReadOnlyList<Parte> Parti, bool Inchisa) {
        public bool Explicata => Inchisa && Parti.All(p => p.Categorie != null);
    }

    public static Rezultat Atribuie(IObjectSpace os, BuclaImport bucla, Guid partida, string cheie,
            decimal delta, DateOnly de, DateOnly pana, bool areRefuzSauPlafon) {
        var parti = cheie.Split('|');
        var (simbol, partenerHex, referinta) = (parti[0], parti[1], parti[2]);
        var docHex = referinta == Deschidere.CheieFaraDocument ? null : referinta.Split('/')[1];
        var sursa = new Dictionary<string, decimal>(StringComparer.Ordinal);
        foreach (var m in bucla.Flax.MiscariPePozitie(docHex, partenerHex, de.ToDateTime(TimeOnly.MinValue),
                     pana.AddDays(1).ToDateTime(TimeOnly.MinValue))) {
            var cont = m.Correspond == Subconto.Debit ? m.ContDebit : m.ContCredit;
            if (bucla.Catalog.Mapeaza(cont) != simbol)
                continue;
            var semnat = m.Correspond == Subconto.Debit ? m.Suma : -m.Suma;
            sursa[m.Recorder] = sursa.GetValueOrDefault(m.Recorder) + semnat;
        }

        var miscari = os.GetObjectsQuery<Postare>()
            .Where(p => p.Unitate == partida && p.Carte == N.Carte.Contabil
                && p.Tranzactie.Fel != N.FelTranzactie.Deschidere && p.Data <= pana)
            .Select(p => new { p.DocumentId, Semnat = p.Sens == N.Sens.Debit ? p.Valoare : -p.Valoare })
            .ToList();
        var documente = miscari.Select(m => m.DocumentId).OfType<Guid>().Distinct().ToList();
        var legaturi = os.GetObjectsQuery<MigrareLegatura>()
            .Where(l => documente.Contains(l.TintaId))
            .Select(l => new { l.TintaId, l.Tabela, l.CheieLegacy })
            .ToList()
            .GroupBy(l => l.TintaId)
            .ToDictionary(g => g.Key, g => g.First());
        var atlas = new Dictionary<string, decimal>(StringComparer.Ordinal);
        var vederi = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var m in miscari) {
            var recorder = "?";
            if (m.DocumentId is Guid id && legaturi.TryGetValue(id, out var l)) {
                recorder = l.CheieLegacy.Split('#', '@')[0];
                vederi[recorder] = l.Tabela.StartsWith("1C:") ? l.Tabela[3..] : l.Tabela;
            }
            atlas[recorder] = atlas.GetValueOrDefault(recorder) + m.Semnat;
        }
        foreach (var r in sursa.Keys.Where(r => !vederi.ContainsKey(r)))
            vederi[r] = bucla.Flax.VedereDocument(r);

        var rezultat = new List<Parte>();
        foreach (var r in sursa.Keys.Union(atlas.Keys)) {
            var s = sursa.GetValueOrDefault(r);
            var a = atlas.GetValueOrDefault(r);
            if (Math.Abs(a - s) < Eps)
                continue;
            var view = vederi.GetValueOrDefault(r) ?? "?";
            rezultat.Add(new Parte(r, view, s, a, Categorie(view, s, a, areRefuzSauPlafon)));
        }
        return new Rezultat(rezultat, Math.Abs(rezultat.Sum(p => p.Delta) - delta) < Eps);
    }

    static string Categorie(string view, decimal sursa, decimal atlas, bool areRefuzSauPlafon) {
        if (Stingatori.Contains(view) && areRefuzSauPlafon)
            return StingereNeaplicata;
        if (Math.Abs(atlas) < Eps && !Stingatori.Contains(view) && !Note.Contains(view))
            return DocumentPropriu;
        if (Math.Abs(sursa) < Eps && Note.Contains(view))
            return FifoNumire;
        return null;
    }
}
