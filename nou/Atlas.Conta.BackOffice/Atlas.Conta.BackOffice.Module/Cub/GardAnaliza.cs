#nullable enable
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Motor;
using DevExpress.ExpressApp;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.Module.Cub;

/// <summary>
/// Analiza obligatorie per cont (15) pe mișcările contabile ale contractului.
/// Mutările și transformările nu intră (D9-D4, D9-r1).
/// </summary>
public static class GardAnaliza {
    public readonly record struct ContFapt(string Simbol, DimensiuneFlags Flags,
        FeluriRepartitor Feluri = FeluriRepartitor.Nedeclarat);
    public readonly record struct LinieFapt(string? Denumire, Guid? Angajament);

    static FeluriRepartitor Fel(N.FelRepartitor fel) => fel switch {
        N.FelRepartitor.Partener => FeluriRepartitor.Partener,
        N.FelRepartitor.Angajat => FeluriRepartitor.Angajat,
        N.FelRepartitor.Gestiune => FeluriRepartitor.Gestiune,
        N.FelRepartitor.ContPropriu => FeluriRepartitor.ContPropriu,
        N.FelRepartitor.UnitateInterna => FeluriRepartitor.UnitateInterna,
        _ => throw new ArgumentOutOfRangeException(nameof(fel), fel, null),
    };

    // C111-D4: contraponderea e scutită numai de prezență; felul unui repartitor prezent se judecă.
    static string? LipsaRepartitor(FeluriRepartitor feluri, N.Repartitor? repartitor, bool contrapondere) {
        if (feluri == FeluriRepartitor.Nedeclarat)
            return null;
        if (repartitor is not { } r)
            return contrapondere || feluri.HasFlag(FeluriRepartitor.Niciunul) ? null : "Repartitor";
        if (feluri.HasFlag(Fel(r.Fel)))
            return null;
        var admise = Enum.GetValues<N.FelRepartitor>().Where(f => feluri.HasFlag(Fel(f)));
        return $"Repartitor de felul {string.Join(" sau ", admise)} (primit: {r.Fel})";
    }

    /// <summary>Forma pe care o judecă gardul: materialul e produsul.</summary>
    public static Dimensiuni Dimensiuni(Guid? produs, N.Analiza analiza) => new() {
        MaterialId = produs,
        CodFunctionalId = analiza.CodFunctional,
        CodEconomicId = analiza.CodEconomic,
        SursaFinantareId = analiza.SursaFinantare,
        UnitateId = analiza.UnitateOrganizatorica,
        ProiectId = analiza.Proiect,
        CentruCostId = analiza.CentruCost,
    };

    /// <summary>Lipsurile pe capetele mișcărilor din <c>Carte = Contabil</c>; gardul judecă numai capătul.</summary>
    public static IReadOnlyList<string> Lipsuri(
            IEnumerable<N.Miscare> miscari,
            IReadOnlyDictionary<Guid, ContFapt> conturi,
            IReadOnlyDictionary<Guid, LinieFapt> linii) {
        ArgumentNullException.ThrowIfNull(miscari);
        ArgumentNullException.ThrowIfNull(conturi);
        ArgumentNullException.ThrowIfNull(linii);
        var lipsuri = new List<string>();
        foreach (var miscare in miscari) {
            var linie = miscare.Cauza.Linie is Guid id ? linii.GetValueOrDefault(id) : default;
            Latura(miscare.La, "debit");
            Latura(miscare.DeLa, "credit");

            void Latura(N.Capat capat, string latura) {
                if (capat.Carte != N.Carte.Contabil || !conturi.TryGetValue(capat.Cont, out var cont))
                    return;
                VerificaLatura(cont, capat.Repartitor, capat.Contrapondere is not null,
                    Dimensiuni(capat.Produs, capat.Analiza),
                    linie.Angajament, latura, linie.Denumire, lipsuri);
            }
        }
        return [.. lipsuri.Distinct()];
    }

    // 15: angajamentul liniei ține loc de cod economic.
    internal static void VerificaLatura(ContFapt cont, N.Repartitor? repartitor, bool contrapondere, Dimensiuni dims,
        Guid? angajamentId, string latura, string? denumireLinie, ICollection<string> lipsuri) {
        var (simbol, flags, feluri) = cont;
        if (flags == DimensiuneFlags.Niciuna && feluri == FeluriRepartitor.Nedeclarat)
            return;
        var lipsa = new List<string>();
        if (LipsaRepartitor(feluri, repartitor, contrapondere) is { } lipsaRepartitor)
            lipsa.Add(lipsaRepartitor);
        if (flags.HasFlag(DimensiuneFlags.Material) && dims.MaterialId == null)
            lipsa.Add("Material");
        if (flags.HasFlag(DimensiuneFlags.CodFunctional) && dims.CodFunctionalId == null)
            lipsa.Add("Cod funcțional");
        if (flags.HasFlag(DimensiuneFlags.CodEconomic) && dims.CodEconomicId == null && angajamentId == null)
            lipsa.Add("Cod economic");
        if (flags.HasFlag(DimensiuneFlags.SursaFinantare) && dims.SursaFinantareId == null)
            lipsa.Add("Sursă de finanțare");
        if (flags.HasFlag(DimensiuneFlags.Unitate) && dims.UnitateId == null)
            lipsa.Add("Unitate");
        if (flags.HasFlag(DimensiuneFlags.Proiect) && dims.ProiectId == null)
            lipsa.Add("Proiect");
        if (flags.HasFlag(DimensiuneFlags.CentruCost) && dims.CentruCostId == null)
            lipsa.Add("Centru de cost");
        if (lipsa.Count > 0)
            lipsuri.Add($"Contul {simbol} ({latura}, linia cu {denumireLinie}) cere: {string.Join(", ", lipsa)}.");
    }

    /// <summary>Refuză atomic documentul ale cărui mișcări lasă o analiză obligatorie necompletată.</summary>
    public static void Verifica(IObjectSpace os, Document doc, IReadOnlyList<N.Miscare> miscari) {
        ArgumentNullException.ThrowIfNull(os);
        ArgumentNullException.ThrowIfNull(doc);
        ArgumentNullException.ThrowIfNull(miscari);
        if (miscari.Count == 0)
            return;
        var ids = miscari.SelectMany(m => new[] { m.DeLa.Cont, m.La.Cont }).Distinct().ToList();
        var conturi = os.GetObjectsQuery<Cont>()
            .Where(c => ids.Contains(c.ID) && (c.DimensiuniObligatorii != DimensiuneFlags.Niciuna
                || c.FeluriRepartitor != FeluriRepartitor.Nedeclarat))
            .Select(c => new { c.ID, c.Simbol, c.DimensiuniObligatorii, c.FeluriRepartitor })
            .ToDictionary(c => c.ID, c => new ContFapt(c.Simbol, c.DimensiuniObligatorii, c.FeluriRepartitor));
        if (conturi.Count == 0)
            return;
        var denumiri = Fapte.ClaseTip(os, doc.Detalii.Select(d => d.TipMaterialId));
        var linii = doc.Detalii.ToDictionary(d => d.ID,
            d => new LinieFapt(denumiri.GetValueOrDefault(d.TipMaterialId).Denumire, d.AngajamentId));
        var lipsuri = Lipsuri(miscari, conturi, linii);
        if (lipsuri.Count > 0)
            throw new OperareException(string.Join("\n", lipsuri));
    }
}
