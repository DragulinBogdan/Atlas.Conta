using DevExpress.ExpressApp;

namespace Atlas.Conta.BackOffice.Module.BusinessObjects;

/// <summary>Coerența dintre `MapareD300` și `MapareD394` pe aceeași pereche tip de TVA × sens (123 b, d).</summary>
public static class CoerentaMapariTva {
    const TipOperatiuneD394 L = TipOperatiuneD394.L, LS = TipOperatiuneD394.LS, V = TipOperatiuneD394.V,
        A = TipOperatiuneD394.A, AS = TipOperatiuneD394.AS, C = TipOperatiuneD394.C;
    static readonly TipOperatiuneD394?[] Exclusa = [null];

    /// <summary>Țintele D394 permise pe fiecare rând de operațiuni D300; `null` = exclusă din D394.</summary>
    public static readonly IReadOnlyDictionary<string, TipOperatiuneD394?[]> Matrice = new Dictionary<string, TipOperatiuneD394?[]> {
        ["1"] = Exclusa, ["2"] = Exclusa, ["3"] = Exclusa, ["3.1"] = Exclusa, ["4"] = Exclusa,
        ["5"] = Exclusa, ["5.1"] = Exclusa, ["6"] = Exclusa, ["7"] = [C], ["7.1"] = Exclusa, ["8"] = Exclusa,
        ["9"] = [L, LS], ["10"] = [L, LS], ["11"] = [L, LS],
        ["12"] = [C], ["12.1"] = [C], ["12.2"] = [C],
        ["13"] = [V],
        ["14"] = Exclusa, ["15"] = Exclusa,
        ["16"] = [L, LS, C],
        ["17"] = Exclusa, ["18"] = Exclusa,
        ["24"] = [A, AS], ["25"] = [A, AS],
        ["29"] = [AS, null], ["29.1"] = Exclusa,
        ["33"] = [A, AS, C],
    };

    public static string Eticheta(TipOperatiuneD394? tip) => tip?.ToString() ?? "exclusă";

    /// <summary>Refuzurile perechii, pe starea din `os` (inclusiv obiectele nesalvate); gol = coerentă.</summary>
    public static IReadOnlyList<string> Incoerente(IObjectSpace os, Guid tipTvaId, SensTva sensTva) {
        if (os == null || tipTvaId == Guid.Empty)
            return [];
        var d394 = Vii<MapareD394>(os, m => (m.TipTva?.ID ?? m.TipTvaId) == tipTvaId && m.SensTva == sensTva,
            q => q.Where(m => m.TipTvaId == tipTvaId && m.SensTva == sensTva));
        if (d394.Count != 1)
            return [];
        var tip = d394[0].Tip;
        var tipTva = d394[0].TipTva ?? os.GetObjectByKey<TipTva>(tipTvaId);
        var cod = tipTva?.Cod ?? tipTvaId.ToString();
        if (tipTva is { DeImport: true })
            return tip == null ? [] : [$"Tipul de TVA {cod} e de import: în D394 se mapează numai ca excludere, "
                + "fiindcă declarația vamală nu se declară în 394."];
        var erori = new List<string>();
        var d300 = Vii<MapareD300>(os, m => (m.TipTva?.ID ?? m.TipTvaId) == tipTvaId && m.SensTva == sensTva,
            q => q.Where(m => m.TipTvaId == tipTvaId && m.SensTva == sensTva));
        if (tip != null && d300.Any(MapareD300.EsteExcludere))
            erori.Add($"Perechea {cod}/{sensTva} e exclusă din D300, iar în D394 e {Eticheta(tip)}; "
                + "operațiunea pe care decontul n-o cere se exclude și din D394.");
        var randuri = d300
            .Select(m => m.Rand ?? (m.RandId is Guid randId && randId != Guid.Empty ? os.GetObjectByKey<RandD300>(randId) : null))
            .Where(r => r is { Fel: FelRandD300.Operatiuni })
            .DistinctBy(r => r.ID).OrderBy(r => r.Ordine);
        foreach (var rand in randuri) {
            var permise = Matrice.GetValueOrDefault(rand.Cod) ?? [];
            if (!permise.Contains(tip))
                erori.Add($"Perechea {cod}/{sensTva} e în D394 {Eticheta(tip)}, iar în D300 pe rd. {rand.Cod}; "
                    + $"pe rândul acesta D394 permite: {string.Join(", ", permise.Select(Eticheta))}.");
        }
        return erori;
    }

    static List<T> Vii<T>(IObjectSpace os, Func<T, bool> potrivire, Func<IQueryable<T>, IQueryable<T>> filtru) where T : class =>
        [.. filtru(os.GetObjectsQuery<T>()).ToList()
            .Concat(os.ModifiedObjects.OfType<T>())
            .Distinct()
            .Where(m => potrivire(m) && !os.IsObjectToDelete(m) && !os.IsDeletedObject(m))];
}
