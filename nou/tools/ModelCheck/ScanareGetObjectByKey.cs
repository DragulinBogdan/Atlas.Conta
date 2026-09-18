using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Atlas.Conta.BackOffice.ModelCheck;

// Plasa F28-N (89): `GetObjectByKey` cu tip explicit (generic sau `typeof`) pe o frunză a unei ierarhii EF
// sau pe un parametru generic — cu prefetch-ul XAF, o cheie urmărită ca alt tip al ierarhiei iese cast greșit.
static class ScanareGetObjectByKey {
    static readonly Regex Apel = new(@"\bGetObjectByKey\s*(?:<\s*(?<tip>[\w.]+)\s*>|\(\s*typeof\s*\(\s*(?<tip>[\w.]+)\s*\))",
        RegexOptions.Compiled);
    static readonly Regex ComentariuBloc = new(@"/\*.*?\*/", RegexOptions.Compiled | RegexOptions.Singleline);
    static readonly Regex ComentariuLinie = new(@"(?<!:)//.*$", RegexOptions.Compiled | RegexOptions.Multiline);

    public record Rezultat(int Fisiere, int Apeluri, List<string> Incalcari);

    public static Rezultat Scaneaza(IModel model, string radacina) {
        if (!Directory.Exists(radacina))
            return new Rezultat(0, 0, [$"lipsește sursa {radacina}"]);
        var entitati = model.GetEntityTypes().GroupBy(e => e.ClrType.Name)
            .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.Ordinal);
        var fisiere = Directory.EnumerateFiles(radacina, "*.cs", SearchOption.AllDirectories)
            .Where(f => !f.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                .Any(d => d is "bin" or "obj" or "Migrations"))
            .ToList();
        var apeluri = 0;
        var incalcari = new List<string>();
        foreach (var fisier in fisiere) {
            var text = ComentariuLinie.Replace(
                ComentariuBloc.Replace(File.ReadAllText(fisier), m => new string('\n', m.Value.Count(c => c == '\n'))), "");
            foreach (Match m in Apel.Matches(text)) {
                apeluri++;
                var nume = m.Groups["tip"].Value.Split('.')[^1];
                var motiv = !entitati.TryGetValue(nume, out var tipuri)
                    ? "parametru generic sau tip nemapat"
                    : tipuri.FirstOrDefault(t => t.BaseType != null) is { } frunza
                        ? $"frunză a ierarhiei {frunza.GetRootType().ClrType.Name}"
                        : null;
                if (motiv != null)
                    incalcari.Add($"{Path.GetRelativePath(radacina, fisier)}:{text[..m.Index].Count(c => c == '\n') + 1} "
                        + $"{m.Value} ({motiv})");
            }
        }
        return new Rezultat(fisiere.Count, apeluri, incalcari);
    }
}
