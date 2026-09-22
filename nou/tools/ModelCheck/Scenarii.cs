namespace Atlas.Conta.BackOffice.ModelCheck;

/// <summary>O scenă a catalogului (091): numele funcției care o rulează și tipurile pe care le probează.</summary>
sealed record Scena(string Nume, string[] Tipuri, Action Ruleaza);

/// <summary>Filtrul `--scenarii &lt;TIP&gt;[,&lt;TIP&gt;…]` (091-r1).</summary>
static class Scenarii {
    public const string Optiune = "--scenarii";

    /// <summary>Codurile catalogului, în ordinea din <c>docs/nucleu/scenarii/README.md</c>.</summary>
    public static readonly string[] Coduri = [
        "BCS", "FCT", "PLT", "INC", "BTR", "FCL", "DSC", "NTC", "ITV", "RDC", "RLF", "DVI", "ASM", "LDI",
        "NIR", "DESCHIDERE", "IMO", "X",
    ];

    public static string Folosire =>
        $"Folosire: ModelCheck {Optiune} <TIP>[,<TIP>…] [privat]  — TIP ∈ {string.Join(", ", Coduri)}";

    /// <summary>
    /// Null dacă opțiunea lipsește (suita integrală); altfel mulțimea codurilor cerute.
    /// <paramref name="eroare"/> e nevidă când opțiunea e prezentă dar greșită.
    /// </summary>
    public static HashSet<string> Filtru(string[] args, out string eroare) {
        eroare = null;
        var index = Array.FindIndex(args, a => a.Equals(Optiune, StringComparison.OrdinalIgnoreCase));
        if (index < 0)
            return null;
        if (args.Length <= index + 1 || args[index + 1].StartsWith('-')) {
            eroare = Folosire;
            return null;
        }
        var cerute = args[index + 1].Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(c => c.ToUpperInvariant()).ToHashSet(StringComparer.Ordinal);
        var necunoscute = cerute.Where(c => !Coduri.Contains(c)).ToList();
        if (cerute.Count == 0 || necunoscute.Count > 0) {
            eroare = (necunoscute.Count > 0 ? $"Cod necunoscut: {string.Join(", ", necunoscute)}. " : "") + Folosire;
            return null;
        }
        return cerute;
    }

    /// <summary>Scenele selectate de filtru, în ordinea suitei; filtrul null le ia pe toate.</summary>
    public static List<Scena> Selecteaza(IEnumerable<Scena> scene, IReadOnlySet<string> filtru) =>
        scene.Where(s => filtru == null || s.Tipuri.Any(filtru.Contains)).ToList();
}
