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
    public readonly record struct ContFapt(string Simbol, DimensiuneFlags Flags);
    public readonly record struct LinieFapt(string? Denumire, Guid? Angajament);

    /// <summary>Forma pe care o judecă gardul: repartitorul e partenerul sau gestiunea, materialul e produsul.</summary>
    public static Dimensiuni Dimensiuni(Guid? repartitor, Guid? produs, N.Analiza analiza) => new() {
        RepartitorId = repartitor,
        MaterialId = produs,
        CodFunctionalId = analiza.CodFunctional,
        CodEconomicId = analiza.CodEconomic,
        SursaFinantareId = analiza.SursaFinantare,
        UnitateId = analiza.UnitateOrganizatorica,
        ProiectId = analiza.Proiect,
        CentruCostId = analiza.CentruCost,
    };

    /// <summary>
    /// Lipsurile pe capetele mișcărilor din <c>Carte = Contabil</c>. Repartitorul unei laturi
    /// e partenerul sau gestiunea capătului, apoi cel al documentului, dacă e dat.
    /// </summary>
    public static IReadOnlyList<string> Lipsuri(
            IEnumerable<N.Miscare> miscari,
            IReadOnlyDictionary<Guid, ContFapt> conturi,
            IReadOnlyDictionary<Guid, LinieFapt> linii,
            Guid? repartitorDebit = null,
            Guid? repartitorCredit = null) {
        ArgumentNullException.ThrowIfNull(miscari);
        ArgumentNullException.ThrowIfNull(conturi);
        ArgumentNullException.ThrowIfNull(linii);
        var lipsuri = new List<string>();
        foreach (var miscare in miscari) {
            var linie = miscare.Cauza.Linie is Guid id ? linii.GetValueOrDefault(id) : default;
            Latura(miscare.La, "debit", repartitorDebit);
            Latura(miscare.DeLa, "credit", repartitorCredit);

            void Latura(N.Capat capat, string latura, Guid? alDocumentului) {
                if (capat.Carte != N.Carte.Contabil || !conturi.TryGetValue(capat.Cont, out var cont))
                    return;
                VerificaLatura(cont.Simbol, cont.Flags,
                    Dimensiuni(capat.Partener ?? capat.Gestiune ?? alDocumentului, capat.Produs, capat.Analiza),
                    linie.Angajament, latura, linie.Denumire, lipsuri);
            }
        }
        return [.. lipsuri.Distinct()];
    }

    // 15: angajamentul liniei ține loc de cod economic.
    internal static void VerificaLatura(string simbol, DimensiuneFlags flags, Dimensiuni dims,
        Guid? angajamentId, string latura, string denumireLinie, ICollection<string> lipsuri) {
        if (flags == DimensiuneFlags.Niciuna)
            return;
        var lipsa = new List<string>();
        if (flags.HasFlag(DimensiuneFlags.Repartitor) && dims.RepartitorId == null)
            lipsa.Add("Repartitor");
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
            .Where(c => ids.Contains(c.ID) && c.DimensiuniObligatorii != DimensiuneFlags.Niciuna)
            .Select(c => new { c.ID, c.Simbol, c.DimensiuniObligatorii })
            .ToDictionary(c => c.ID, c => new ContFapt(c.Simbol, c.DimensiuniObligatorii));
        if (conturi.Count == 0)
            return;
        var denumiri = Fapte.ClaseTip(os, doc.Detalii.Select(d => d.TipMaterialId));
        var linii = doc.Detalii.ToDictionary(d => d.ID,
            d => new LinieFapt(denumiri.GetValueOrDefault(d.TipMaterialId).Denumire, d.AngajamentId));
        // D9-D4: repartitorul cade pe latura documentului, ca pe nota planului vechi.
        var lipsuri = Lipsuri(miscari, conturi, linii,
            doc.RepartitorImplicitDebit(os), doc.RepartitorImplicitCredit(os));
        if (lipsuri.Count > 0)
            throw new OperareException(string.Join("\n", lipsuri));
    }
}
