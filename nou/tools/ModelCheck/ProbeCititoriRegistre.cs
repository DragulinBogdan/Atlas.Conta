using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Atlas.Conta.BackOffice.ModelCheck.SursaProductie;

namespace Atlas.Conta.BackOffice.ModelCheck;

// X-D2: registrele se ating în producție numai prin utilizările numite în `Permise`.
static partial class ProbeCititoriRegistre {
    public enum Clasa { Mapare, Evidenta, Autorizare, Legatura, Gardian }

    public sealed record Permisa(string Fisier, string Membru, string Registre, Clasa Clasa, string Rol);
    public sealed record Utilizare(string Fisier, string Membru, string Registru, int Linie, string Fel);
    sealed record Sursa(string Tip, string Nume, bool Calificata, string[] Registre);
    sealed record Mutant(string Nume, string Fisier, Func<string> Text, string Asteptat);

    static readonly string[] Registre = ["RegistruContabil", "RegistruStoc", "RegistruTva", "RegistruImobilizari", "Imperechere"];
    static readonly Regex Tabele = new(@"(?<!\w)(RegistruContabil|RegistruStoc|RegistruTva|RegistruImobilizari|Imperecheri)(?!\w)");
    static readonly string[] Cititori = [
        Modul + "Proiectii/", Modul + "Api/", Modul + "Culegere/", Modul + "Saft/", Modul + "Declaratii/", WebApi,
    ];

    public static void VerificaSursa(Action<string, bool> check, bool lista = false) {
        var arbori = Arbori();
        var utilizari = Scaneaza(arbori);
        if (lista)
            foreach (var g in utilizari.GroupBy(u => (u.Fisier, u.Membru, u.Registru)).OrderBy(g => g.Key))
                Console.WriteLine($"{g.Key.Fisier}\t{g.Key.Membru}\t{g.Key.Registru}\t{string.Join(",", g.Select(u => u.Fel).Distinct())}"
                    + $"\t{string.Join(",", g.Select(u => u.Linie).Distinct())}");

        var incalcari = Incalcari(utilizari);
        var chei = utilizari.Select(u => (u.Fisier, u.Membru, u.Registru)).ToHashSet();
        var declarate = Permise.SelectMany(p => p.Registre.Split('|').Select(r => (p.Fisier, p.Membru, Registru: r))).ToList();
        var nefolosite = declarate.Where(c => !chei.Contains(c)).Select(c => $"{c.Fisier} {c.Membru} {c.Registru}").ToList();
        var dubluri = declarate.GroupBy(c => c).Where(g => g.Count() > 1).Select(g => $"{g.Key}").ToList();
        Console.WriteLine($"     MĂSURAT (X-D2): {arbori.Count} fișiere de producție; {chei.Count} utilizări (fișier × membru × registru), "
            + $"{Permise.Length} intrări permise [{PeClase(Permise)}]; încălcări [{string.Join("; ", incalcari)}]; "
            + $"permise nefolosite [{string.Join("; ", nefolosite)}]; dubluri [{string.Join("; ", dubluri)}].");
        check("X-D2: registrele și `Imperechere` se ating în producție numai prin lista nominală (fișier × membru × registru × rol); "
            + "nicio intrare permisă nu rămâne fără utilizare", arbori.Count > 100 && incalcari.Count == 0 && nefolosite.Count == 0 && dubluri.Count == 0);

        var cititori = Permise.Where(p => Cititori.Any(c => p.Fisier.StartsWith(c, StringComparison.Ordinal))).ToList();
        var surseDeSold = cititori.Where(p => p.Clasa is Clasa.Mapare or Clasa.Evidenta or Clasa.Gardian
            || p.Clasa == Clasa.Legatura && p.Registre != "Imperechere").ToList();
        Console.WriteLine($"     MĂSURAT (X-D2): {cititori.Count} intrări în perimetrul cititorilor [{PeClase(cititori)}]; "
            + $"citiri de registru [{string.Join("; ", surseDeSold.Select(p => $"{p.Fisier} {p.Membru}"))}].");
        check("X-D2: în `Proiectii/`, `Api/`, `Culegere/`, `Saft/`, `Declaratii/` și WebApi cele patru registre apar numai ca "
            + "cheie de autorizare și în absorbția ASM-B6 a regimului dual, iar `Imperechere` numai ca legătură sau autorizare",
            cititori.Count > 0 && surseDeSold.Count == 0);

        var mutanti = Mutanti.Select(m => {
            var gasite = Incalcari(Scaneaza(Cu(m.Fisier, m.Text())));
            return (m.Nume, Ok: m.Asteptat == null ? gasite.Count == 0 : gasite.Count == 1 && gasite[0].Contains(m.Asteptat), gasite);
        }).ToList();
        Console.WriteLine("     MĂSURAT (X-D2): mutanți " + string.Join("; ", mutanti.Select(m => $"{m.Nume} → [{string.Join(" | ", m.gasite)}]")) + ".");
        check("X-D2: proba cade pe o citire nouă de registru (interogare, SQL, alias, apel al unui purtător de date de registru, "
            + "membru nou într-un fișier permis) și nu cade pe `nameof`", mutanti.Count == Mutanti.Length && mutanti.All(m => m.Ok));
    }

    static string PeClase(IEnumerable<Permisa> permise) =>
        string.Join(", ", permise.GroupBy(p => p.Clasa).OrderBy(g => g.Key).Select(g => $"{g.Key} {g.Count()}"));

    static List<string> Incalcari(List<Utilizare> utilizari) {
        var permise = Permise.SelectMany(p => p.Registre.Split('|').Select(r => (p.Fisier, p.Membru, r))).ToHashSet();
        return utilizari.Where(u => !permise.Contains((u.Fisier, u.Membru, u.Registru)))
            .GroupBy(u => (u.Fisier, u.Membru, u.Registru)).OrderBy(g => g.Key)
            .Select(g => $"{g.Key.Fisier}:{g.Min(u => u.Linie)} {g.Key.Membru} {g.Key.Registru} ({string.Join(",", g.Select(u => u.Fel).Distinct())})")
            .ToList();
    }

    static List<Utilizare> Scaneaza(IReadOnlyList<Arbore> arbori) {
        var surse = arbori.SelectMany(a => SurseDerivate(a.Radacina)).Concat(Purtatori).GroupBy(s => (s.Tip, s.Nume))
            .Select(g => new Sursa(g.Key.Tip, g.Key.Nume, g.All(s => s.Calificata), [.. g.SelectMany(s => s.Registre).Distinct()]))
            .ToLookup(s => s.Nume);
        var utilizari = new List<Utilizare>();
        foreach (var arbore in arbori) {
            var aliasuri = arbore.Radacina.DescendantNodes().OfType<UsingDirectiveSyntax>()
                .Where(u => u.Alias != null && Registre.Contains(UltimulNume(u.NamespaceOrType)))
                .ToDictionary(u => u.Alias.Name.Identifier.ValueText, u => UltimulNume(u.NamespaceOrType));
            void Adauga(SyntaxNode nod, SyntaxToken token, string registru, string fel) {
                var (tip, membru) = Membru(nod);
                if (tip != registru)
                    utilizari.Add(new(arbore.Fisier, membru, registru, Linie(token), fel));
            }
            foreach (var nume in arbore.Radacina.DescendantNodes().OfType<IdentifierNameSyntax>()) {
                var text = nume.Identifier.ValueText;
                if (nume.Ancestors().Any(a => a is UsingDirectiveSyntax || EsteNameof(a)))
                    continue;
                if (Registre.Contains(text) || aliasuri.ContainsKey(text)) {
                    Adauga(nume, nume.Identifier, aliasuri.GetValueOrDefault(text, text), "tip");
                    continue;
                }
                foreach (var sursa in surse[text]) {
                    var calificator = nume.Parent switch {
                        MemberAccessExpressionSyntax acces when acces.Name == nume => UltimulNume(acces.Expression) ?? "",
                        MemberBindingExpressionSyntax => "",
                        _ => null,
                    };
                    if (calificator == null ? Membru(nume).Tip == sursa.Tip : !sursa.Calificata || calificator == sursa.Tip)
                        foreach (var registru in sursa.Registre)
                            Adauga(nume, nume.Identifier, registru, $"prin {sursa.Tip}.{sursa.Nume}");
                }
            }
            foreach (var token in arbore.Radacina.DescendantTokens().Where(t => t.Kind() is SyntaxKind.StringLiteralToken
                    or SyntaxKind.InterpolatedStringTextToken or SyntaxKind.SingleLineRawStringLiteralToken
                    or SyntaxKind.MultiLineRawStringLiteralToken or SyntaxKind.Utf8StringLiteralToken))
                foreach (Match m in Tabele.Matches(token.ValueText))
                    Adauga(token.Parent, token, m.Value == "Imperecheri" ? "Imperechere" : m.Value, "sql");
        }
        return utilizari;
    }

    // Un membru al cărui tip declarat poartă o colecție de rânduri de registru: apelanții lui citesc registrul.
    static IEnumerable<Sursa> SurseDerivate(SyntaxNode radacina) {
        foreach (var membru in radacina.DescendantNodes().OfType<MemberDeclarationSyntax>()) {
            var (tip, nume, modificatori) = membru switch {
                MethodDeclarationSyntax m => (m.ReturnType, m.Identifier.ValueText, m.Modifiers),
                PropertyDeclarationSyntax p => (p.Type, p.Identifier.ValueText, p.Modifiers),
                FieldDeclarationSyntax f => (f.Declaration.Type, f.Declaration.Variables[0].Identifier.ValueText, f.Modifiers),
                _ => (null, null, default),
            };
            if (tip is null or IdentifierNameSyntax or QualifiedNameSyntax or NullableTypeSyntax { ElementType: IdentifierNameSyntax })
                continue;
            var registre = tip.DescendantNodesAndSelf().OfType<IdentifierNameSyntax>()
                .Select(i => i.Identifier.ValueText).Where(Registre.Contains).Distinct().ToArray();
            var continator = (membru.Parent as BaseTypeDeclarationSyntax)?.Identifier.ValueText;
            if (registre.Length > 0 && continator != null && !Registre.Contains(nume))
                yield return new(continator, nume, modificatori.Any(SyntaxKind.StaticKeyword), registre);
        }
    }

    static bool EsteNameof(SyntaxNode nod) =>
        nod is InvocationExpressionSyntax { Expression: IdentifierNameSyntax { Identifier.ValueText: "nameof" } };

    static readonly Mutant[] Mutanti = [
        new("interogare", Modul + "Proiectii/Mutant.cs",
            () => "static class Mutant { static decimal Rulaj(IObjectSpace os) => os.GetObjectsQuery<RegistruContabil>().Sum(r => r.Valoare); }",
            "Proiectii/Mutant.cs:1 Mutant.Rulaj RegistruContabil (tip)"),
        new("sql", Modul + "Saft/Mutant.cs",
            () => "static class Mutant { const string Sql = \"SELECT SUM(\\\"Cantitate\\\") FROM \\\"RegistruStoc\\\"\"; }",
            "Saft/Mutant.cs:1 Mutant.Sql RegistruStoc (sql)"),
        new("alias", Modul + "Api/Mutant.cs",
            () => "using R = Atlas.Conta.BackOffice.Module.BusinessObjects.RegistruTva;\n"
                + "static class Mutant { static bool Are(IObjectSpace os) => os.GetObjectsQuery<R>().Any(); }",
            "Api/Mutant.cs:2 Mutant.Are RegistruTva (tip)"),
        new("purtător", Modul + "Culegere/Mutant.cs",
            () => "static class Mutant { static decimal Libera(IObjectSpace os, Guid a, Guid b) => Partide.NominalizataLibera(os, a, b); }",
            "Culegere/Mutant.cs:1 Mutant.Libera Imperechere (prin Partide.NominalizataLibera)"),
        new("membru nou în fișier permis", Modul + "Motor/ImperechereService.cs",
            () => Inainte(Modul + "Motor/ImperechereService.cs", "public static decimal Total(IObjectSpace os, Guid documentId)",
                "public static bool AreTva(IObjectSpace os) => os.GetObjectsQuery<RegistruTva>().Any();\n    "),
            "ImperechereService.AreTva RegistruTva (tip)"),
        new("nameof", Modul + "Proiectii/Mutant.cs",
            () => "static class Mutant { const string Nume = nameof(RegistruContabil); }", null),
    ];
}
