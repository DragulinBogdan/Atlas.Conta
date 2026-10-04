using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Atlas.Conta.BackOffice.ModelCheck;

// Sursa de producție (`Module`, `WebApi`, `Blazor.Server`) ca arbori sintactici, pentru probele de arhitectură.
static class SursaProductie {
    public const string Modul = "Atlas.Conta.BackOffice.Module/";
    public const string WebApi = "Atlas.Conta.BackOffice.WebApi/";

    public sealed record Arbore(string Fisier, SyntaxNode Radacina);

    static IReadOnlyList<Arbore> arbori;

    public static IReadOnlyList<Arbore> Arbori() {
        if (arbori != null) return arbori;
        var radacina = Path.GetFullPath(Path.Combine(MetadataDump.DirectorProiect(), "..", "..", "Atlas.Conta.BackOffice"));
        return arbori = Directory.EnumerateFiles(radacina, "*.cs", SearchOption.AllDirectories)
            .Select(f => Path.GetRelativePath(radacina, f).Replace('\\', '/'))
            .Where(f => !f.Split('/').Any(d => d is "bin" or "obj" or "Migrations"))
            .OrderBy(f => f, StringComparer.Ordinal)
            .Select(f => Parseaza(f, File.ReadAllText(Path.Combine(radacina, f)))).ToList();
    }

    public static Arbore Parseaza(string fisier, string text) => new(fisier, CSharpSyntaxTree.ParseText(text).GetRoot());

    public static IReadOnlyList<Arbore> Cu(string fisier, string text) =>
        [.. Arbori().Where(a => a.Fisier != fisier), Parseaza(fisier, text)];

    public static string Inainte(string fisier, string reper, string adaos) {
        var text = Arbori().Single(a => a.Fisier == fisier).Radacina.ToFullString();
        var i = text.IndexOf(reper, StringComparison.Ordinal);
        return i < 0 ? throw new InvalidOperationException($"Mutant: `{reper}` lipsește din {fisier}.") : text.Insert(i, adaos);
    }

    public static int Linie(SyntaxToken token) => token.GetLocation().GetLineSpan().StartLinePosition.Line + 1;

    public static string UltimulNume(SyntaxNode nod) => nod switch {
        IdentifierNameSyntax i => i.Identifier.ValueText,
        QualifiedNameSyntax q => q.Right.Identifier.ValueText,
        MemberAccessExpressionSyntax m => m.Name.Identifier.ValueText,
        GenericNameSyntax g => g.Identifier.ValueText,
        _ => null,
    };

    public static (string Tip, string Membru) Membru(SyntaxNode nod) {
        var tip = nod.AncestorsAndSelf().OfType<BaseTypeDeclarationSyntax>().FirstOrDefault()?.Identifier.ValueText;
        var membru = nod.AncestorsAndSelf().Select(a => a switch {
            MethodDeclarationSyntax m => m.Identifier.ValueText,
            ConstructorDeclarationSyntax => ".ctor",
            PropertyDeclarationSyntax p => p.Identifier.ValueText,
            IndexerDeclarationSyntax => "this[]",
            OperatorDeclarationSyntax o => "operator " + o.OperatorToken.ValueText,
            ConversionOperatorDeclarationSyntax => "operator conversie",
            EventDeclarationSyntax e => e.Identifier.ValueText,
            BaseFieldDeclarationSyntax f => f.Declaration.Variables[0].Identifier.ValueText,
            _ => null,
        }).FirstOrDefault(m => m != null);
        return (tip, (tip ?? "<global>") + (membru == null ? "" : "." + membru));
    }
}
