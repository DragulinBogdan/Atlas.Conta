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
    public const string FifoExcedent = "notă Atlas peste restul partidei numite, excedentul pe FIFO (N-D7)";
    public const string NotaPeProprie = "notă care mărește partida numită în sensul ei, Atlas o ține pe partida notei (115)";

    public sealed record Parte(string Recorder, string View, decimal Sursa, decimal Atlas, string Categorie) {
        public decimal Delta => Atlas - Sursa;
    }

    public sealed record Rezultat(IReadOnlyList<Parte> Parti, bool Inchisa) {
        public bool Explicata => Inchisa && Parti.All(p => p.Categorie != null);
    }

    public sealed record Cerere(Guid Partida, string Cheie, decimal Delta, bool AreRefuzSauPlafon, int SemnInitial);

    public static List<Rezultat> Atribuie(IObjectSpace os, BuclaImport bucla, IReadOnlyList<Cerere> cereri,
            DateOnly de, DateOnly pana) {
        var pozitii = cereri.Select(c => Pozitie(c.Cheie)).ToList();
        var miscariSursa = bucla.Flax.MiscariPePozitii(pozitii.Select(p => (p.Doc, p.Partener)).Distinct().ToList(),
            de.ToDateTime(TimeOnly.MinValue), pana.AddDays(1).ToDateTime(TimeOnly.MinValue));

        var partide = cereri.Select(c => c.Partida).Distinct().ToList();
        var miscariAtlas = os.GetObjectsQuery<Postare>()
            .Where(p => p.Unitate != null && partide.Contains(p.Unitate.Value) && p.Carte == N.Carte.Contabil
                && p.Tranzactie.Fel != N.FelTranzactie.Deschidere && p.Data <= pana)
            .Select(p => new { Partida = p.Unitate.Value, p.DocumentId, Semnat = p.Sens == N.Sens.Debit ? p.Valoare : -p.Valoare })
            .ToList()
            .ToLookup(m => m.Partida);
        var documente = miscariAtlas.SelectMany(g => g).Select(m => m.DocumentId).OfType<Guid>().Distinct().ToList();
        var legaturi = os.GetObjectsQuery<MigrareLegatura>()
            .Where(l => documente.Contains(l.TintaId))
            .Select(l => new { l.TintaId, l.Tabela, l.CheieLegacy })
            .ToList()
            .GroupBy(l => l.TintaId)
            .ToDictionary(g => g.Key, g => g.MinBy(l => l.CheieLegacy, StringComparer.Ordinal)!);

        var vederi = new Dictionary<string, string>(StringComparer.Ordinal);
        var sumeSursa = new List<Dictionary<string, decimal>>();
        var sumeAtlas = new List<Dictionary<string, decimal>>();
        for (var k = 0; k < cereri.Count; k++) {
            var sursa = new Dictionary<string, decimal>(StringComparer.Ordinal);
            foreach (var m in miscariSursa[(pozitii[k].Doc, pozitii[k].Partener)]) {
                var cont = m.Correspond == Subconto.Debit ? m.ContDebit : m.ContCredit;
                if (bucla.Catalog.Mapeaza(cont) != pozitii[k].Simbol)
                    continue;
                sursa[m.Recorder] = sursa.GetValueOrDefault(m.Recorder) + (m.Correspond == Subconto.Debit ? m.Suma : -m.Suma);
            }
            var atlas = new Dictionary<string, decimal>(StringComparer.Ordinal);
            foreach (var m in miscariAtlas[cereri[k].Partida]) {
                var recorder = "?";
                if (m.DocumentId is Guid id && legaturi.TryGetValue(id, out var l)) {
                    recorder = l.CheieLegacy.Split('#', '@')[0];
                    vederi[recorder] = l.Tabela.StartsWith("1C:") ? l.Tabela[3..] : l.Tabela;
                }
                atlas[recorder] = atlas.GetValueOrDefault(recorder) + m.Semnat;
            }
            sumeSursa.Add(sursa);
            sumeAtlas.Add(atlas);
        }
        var necunoscute = sumeSursa.SelectMany(s => s.Keys).Where(r => !vederi.ContainsKey(r)).Distinct().ToList();
        foreach (var (r, v) in bucla.Flax.VederiDocumente(necunoscute))
            vederi[r] = v;

        var rezultate = new List<Rezultat>(cereri.Count);
        for (var k = 0; k < cereri.Count; k++) {
            var (sursa, atlas, c) = (sumeSursa[k], sumeAtlas[k], cereri[k]);
            var parti = new List<Parte>();
            foreach (var r in sursa.Keys.Union(atlas.Keys).Order(StringComparer.Ordinal)) {
                var s = sursa.GetValueOrDefault(r);
                var a = atlas.GetValueOrDefault(r);
                if (Math.Abs(a - s) < Eps)
                    continue;
                var view = vederi.GetValueOrDefault(r) ?? "?";
                parti.Add(new Parte(r, view, s, a, Categorie(r, view, s, a, c.AreRefuzSauPlafon, c.SemnInitial)));
            }
            rezultate.Add(new Rezultat(parti, Math.Abs(parti.Sum(p => p.Delta) - c.Delta) < Eps));
        }
        return rezultate;
    }

    static (string Simbol, string Partener, string Doc) Pozitie(string cheie) {
        var parti = cheie.Split('|');
        var doc = parti[2] == Deschidere.CheieFaraDocument ? null : parti[2].Split('/')[1].ToUpperInvariant();
        return (parti[0], parti[1].ToUpperInvariant(), doc);
    }

    static string Categorie(string recorder, string view, decimal sursa, decimal atlas, bool areRefuzSauPlafon,
            int semnInitial) {
        var nota = Note.Contains(view);
        if (nota && (Math.Abs(sursa) < Eps || NoteComune.PeFifo.Contains(recorder)))
            return FifoNumire;
        if (nota && Math.Abs(atlas) < Eps && Math.Sign(sursa) == semnInitial)
            return NotaPeProprie;
        if (Stingatori.Contains(view) && areRefuzSauPlafon)
            return StingereNeaplicata;
        if (Math.Abs(atlas) < Eps && !Stingatori.Contains(view) && !nota)
            return DocumentPropriu;
        if (nota && Math.Sign(atlas) == Math.Sign(sursa) && Math.Abs(atlas) < Math.Abs(sursa))
            return FifoExcedent;
        return null;
    }
}
