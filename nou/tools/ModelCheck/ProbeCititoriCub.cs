using System.Text.RegularExpressions;

namespace Atlas.Conta.BackOffice.ModelCheck;

// 091-r3: codul de producție ajunge la rândurile cubului numai prin lista de mai jos.
static class ProbeCititoriCub {
    static readonly (string Cale, string Rol)[] Permise = [
        ("Atlas.Conta.BackOffice.Module/Cub/Citiri/", "cititorii comuni"),
        ("Atlas.Conta.BackOffice.Module/Cub/Materializare", "scriitorul cubului"),
        ("Atlas.Conta.BackOffice.Module/Cub/ReceptiiConexe.cs", "faptul sursei, fixat la materializarea NIR conex"),
        ("Atlas.Conta.BackOffice.Module/BusinessObjects/BackOfficeDbContext.cs", "maparea EF"),
    ];

    const string Rand = @"(?<q>(?:[\w:]+\.)*)(?:Postare|Tranzactie)";
    static readonly Regex[] Acces = [
        new($@"\b(?:GetObjectsQuery|GetObjects|GetObjectByKey|FindObject|FirstOrDefault|CreateObject|OfType|Cast|Set|DbSet|Entity)\s*<\s*{Rand}\s*>"),
        new($@"\b(?:GetObjectsQuery|GetObjects|GetObjectByKey|FindObject|FirstOrDefault|CreateObject)\s*\(\s*typeof\s*\(\s*{Rand}\s*\)"),
        new(@"\\?""{1,2}(?:Postare|Tranzactie)\\?""{1,2}"),
        new(@"\bTranzactie\s*\??\.\s*Postari\b"),
        new(@"\b(?:DbContext|ctx|db)\s*\)?\s*\.\s*(?:Postari|Tranzactii)\b"),
    ];
    static readonly Regex ComentariuBloc = new(@"/\*.*?\*/", RegexOptions.Singleline);
    static readonly Regex ComentariuLinie = new(@"(?<!:)//.*$", RegexOptions.Multiline);

    public static void VerificaSursa(Action<string, bool> check) {
        var radacina = Path.GetFullPath(Path.Combine(MetadataDump.DirectorProiect(), "..", "..", "Atlas.Conta.BackOffice"));
        var fisiere = Directory.EnumerateFiles(radacina, "*.cs", SearchOption.AllDirectories)
            .Select(f => Path.GetRelativePath(radacina, f).Replace('\\', '/'))
            .Where(f => !f.Split('/').Any(d => d is "bin" or "obj" or "Migrations"))
            .ToList();
        var incalcari = new List<string>();
        var folosite = new HashSet<string>();
        foreach (var fisier in fisiere) {
            var text = ComentariuLinie.Replace(ComentariuBloc.Replace(File.ReadAllText(Path.Combine(radacina, fisier)),
                m => new string('\n', m.Value.Count(c => c == '\n'))), "");
            var accese = Acces.SelectMany(r => r.Matches(text))
                .Where(m => m.Groups["q"].Value.TrimEnd('.').Split('.')[^1] is not ("N" or "Nucleu"))
                .ToList();
            if (accese.Count == 0) continue;
            if (Permise.FirstOrDefault(p => fisier.StartsWith(p.Cale, StringComparison.Ordinal)) is { Cale: { } cale }) {
                folosite.Add(cale);
                continue;
            }
            incalcari.AddRange(accese.Select(m => $"{fisier}:{text[..m.Index].Count(c => c == '\n') + 1} {m.Value}"));
        }
        var nefolosite = Permise.Select(p => p.Cale).Where(c => !folosite.Contains(c)).ToList();
        Console.WriteLine($"     MĂSURAT (091-r3): {fisiere.Count} fișiere de producție; încălcări [{string.Join("; ", incalcari)}]; "
            + $"permise nefolosite [{string.Join("; ", nefolosite)}].");
        check("091-r3: `Postare`/`Tranzactie` se citesc în producție doar prin `Cub/Citiri`; excepțiile numite sunt "
            + string.Join(", ", Permise.Skip(1).Select(p => p.Rol)),
            fisiere.Count > 100 && incalcari.Count == 0 && nefolosite.Count == 0);
    }
}
