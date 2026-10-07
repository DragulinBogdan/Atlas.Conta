using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Atlas.Conta.BackOffice.ModelCheck;

// D9-D7 (b): nicio referință de cod din `nou/` (C# și TypeScript, fără comentarii) la numele registrelor tăiate.
static partial class ProbeCititoriRegistre {
    static readonly Regex Interzise = new(@"(?<![\w$])(RegistruContabil|RegistruStoc|RegistruTva|RegistruImobilizari|PosteazaInCub"
        + @"|TotalStingere|IDocumentCuRegistruPropriu|StocService|CubDinRegistre)(?![\w$])");
    static readonly string[] DirectoareOmise = ["bin", "obj", "node_modules", ".git", "dist"];
    const string FisierulProbei = "tools/ModelCheck/ProbeCititoriRegistre.NumeInterzise.cs";
    const string CodGenerat = "Atlas.Conta.Client/src/generated/";

    sealed record MutantNume(string Nume, string Fisier, string Text, int Asteptate);

    static readonly MutantNume[] MutantiNume = [
        new("interogare C#", "Atlas.Conta.BackOffice/Atlas.Conta.BackOffice.Module/Proiectii/Mutant.cs",
            "static class Mutant { static int N(IObjectSpace os) => os.GetObjectsQuery<RegistruTva>().Count(); }", 1),
        new("SQL în literal", "tools/ModelCheck/Mutant.cs",
            "static class Mutant { const string Sql = \"SELECT 1 FROM \\\"RegistruStoc\\\"\"; }", 1),
        new("membru scos", "Atlas.Conta.BackOffice/Atlas.Conta.BackOffice.WebApi/Mutant.cs",
            "static class Mutant { static bool B(TipDocument t) => t.PosteazaInCub; }", 1),
        new("TypeScript", "Atlas.Conta.Client/src/Mutant.ts",
            "export const total = (d: { TotalStingere?: number }) => d.TotalStingere ?? 0;", 2),
        new("comentariu C#", "Atlas.Conta.BackOffice/Atlas.Conta.BackOffice.Module/Mutant.cs",
            "// StocService a dispărut\n/* CubDinRegistre */ static class Mutant { }", 0),
        new("comentariu TypeScript", "Atlas.Conta.Client/src/Mutant.ts", "// RegistruContabil\nexport const x = 1; /* RegistruTva */", 0),
        new("nume de raport păstrat", "Atlas.Conta.BackOffice/Atlas.Conta.BackOffice.Module/Api/Imo/Mutant.cs",
            "sealed class Mutant { public RegistruImobilizariDto Dto { get; set; } }", 0),
        new("cod generat", CodGenerat + "Mutant.ts", "export type T = { PosteazaInCub?: boolean };", 0),
        new("specificator de modul", "Atlas.Conta.Client/src/Mutant.tsx",
            "import { Pagina } from './felii/imobilizari/RegistruImobilizari';\nconst x = import('./RegistruTva');", 0),
        new("identificator TypeScript", "Atlas.Conta.Client/src/Mutant.tsx",
            "export function RegistruImobilizari() { return null; }", 1),
    ];

    static void VerificaNumeInterzise(Action<string, bool> check) {
        var radacina = Path.GetFullPath(Path.Combine(MetadataDump.DirectorProiect(), "..", ".."));
        var fisiere = Fisiere(radacina, radacina).ToList();
        var gasite = fisiere.SelectMany(f => Referinte(f, File.ReadAllText(Path.Combine(radacina, f)))).ToList();
        Console.WriteLine($"     MĂSURAT (D9-D7b): {fisiere.Count} fișiere C# și TypeScript în `nou/`; "
            + $"referințe [{string.Join("; ", gasite)}].");
        check("D9-D7b: niciun nume al registrelor tăiate, al membrilor scoși sau al serviciilor vechi nu apare ca referință "
            + "de cod în `nou/` (identificator întreg, C# și TypeScript); excepții: fișierul probei și codul generat",
            fisiere.Count > 500 && gasite.Count == 0);

        var mutanti = MutantiNume.Select(m => (m.Nume, Gasite: Referinte(m.Fisier, m.Text).ToList(), m.Asteptate)).ToList();
        Console.WriteLine("     MĂSURAT (D9-D7b): mutanți " + string.Join("; ", mutanti.Select(m => $"{m.Nume} → [{string.Join(" | ", m.Gasite)}]")) + ".");
        check("D9-D7b: proba cade pe interogare, literal SQL, membru scos și identificatori TypeScript, nu cade pe comentarii, "
            + "pe `RegistruImobilizariDto`, pe codul generat și pe specificatorii de modul", mutanti.All(m => m.Gasite.Count == m.Asteptate));
    }

    static IEnumerable<string> Fisiere(string radacina, string director) {
        foreach (var sub in Directory.EnumerateDirectories(director))
            if (!DirectoareOmise.Contains(Path.GetFileName(sub)))
                foreach (var f in Fisiere(radacina, sub))
                    yield return f;
        foreach (var f in Directory.EnumerateFiles(director))
            if (Path.GetExtension(f) is ".cs" or ".ts" or ".tsx")
                yield return Path.GetRelativePath(radacina, f).Replace('\\', '/');
    }

    static IEnumerable<string> Referinte(string fisier, string text) {
        if (fisier == FisierulProbei || fisier.StartsWith(CodGenerat, StringComparison.Ordinal))
            yield break;
        var bucati = fisier.EndsWith(".cs", StringComparison.Ordinal) ? CodCSharp(text) : CodTypeScript(text);
        foreach (var (linie, cod) in bucati)
            foreach (Match m in Interzise.Matches(cod))
                yield return $"{fisier}:{linie} {m.Value}";
    }

    static IEnumerable<(int Linie, string Cod)> CodCSharp(string text) =>
        CSharpSyntaxTree.ParseText(text).GetRoot().DescendantTokens()
            .Where(t => t.IsKind(SyntaxKind.IdentifierToken) || t.IsKind(SyntaxKind.StringLiteralToken)
                || t.IsKind(SyntaxKind.InterpolatedStringTextToken) || t.IsKind(SyntaxKind.SingleLineRawStringLiteralToken)
                || t.IsKind(SyntaxKind.MultiLineRawStringLiteralToken) || t.IsKind(SyntaxKind.Utf8StringLiteralToken))
            .Select(t => (t.GetLocation().GetLineSpan().StartLinePosition.Line + 1, t.ValueText ?? t.Text));

    // D9-D12: specificatorul de modul numește fișierul paginii păstrate, nu un simbol.
    static readonly Regex ComentariuTs = new(@"/\*.*?\*/|//[^\n]*|(?<=\bfrom\s*|\bimport\s*\(\s*)(['""])[^'""\n]*\1",
        RegexOptions.Singleline);

    static IEnumerable<(int Linie, string Cod)> CodTypeScript(string text) {
        var fara = ComentariuTs.Replace(text, m => new string('\n', m.Value.Count(c => c == '\n')));
        return fara.Split('\n').Select((l, i) => (i + 1, l));
    }
}
