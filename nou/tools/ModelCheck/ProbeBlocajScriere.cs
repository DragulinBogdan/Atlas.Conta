using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Atlas.Conta.BackOffice.ModelCheck.SursaProductie;

namespace Atlas.Conta.BackOffice.ModelCheck;

// X-D6 (c): blocajul scrierii se ia într-un singur loc, numai la intrarea comenzilor numite; nicio citire nu îl ia.
static class ProbeBlocajScriere {
    const string Tranzactie = Modul + "Motor/TranzactieComanda.cs";

    static readonly (string Fisier, string Membru, string Rol)[] Intrari = [
        (Modul + "Api/ComenziDocument.cs", "ComenziDocument.Opereaza", "comanda de operare"),
        (Modul + "Api/ComenziDocument.cs", "ComenziDocument.AnuleazaOperarea", "comanda de anulare"),
        (Modul + "Api/ComenziDocument.cs", "ComenziDocument.Storneaza", "comanda de storno"),
        (Modul + "Motor/CorectieService.cs", "CorectieService.Corecteaza", "comanda de corecție"),
        (Modul + "Motor/MotorOperare.cs", "MotorOperare.Opereaza", "motorul chemat fără tranzacție de comandă"),
        (Modul + "Motor/MotorOperare.cs", "MotorOperare.AnuleazaOperarea", "motorul chemat fără tranzacție de comandă"),
        (Modul + "Motor/MotorOperare.cs", "MotorOperare.Storneaza", "motorul chemat fără tranzacție de comandă"),
        (Modul + "Motor/ImperechereService.cs", "ImperechereService.Imperecheaza", "împerecherea"),
        (Modul + "Motor/ImperechereService.cs", "ImperechereService.Desfa", "desfacerea împerecherii"),
        (Modul + "Motor/ImperechereService.cs", "ImperechereService.Sterge", "ștergerea împerecherii"),
        (Modul + "Motor/PerioadaService.cs", "PerioadaService.Inchide", "închiderea perioadei"),
        (Modul + "Motor/PerioadaService.cs", "PerioadaService.Redeschide", "redeschiderea perioadei"),
        (Modul + "Api/Perioade/PerioadeApply.cs", "PerioadeApply.Reconstruieste", "reconstrucția snapshot-urilor"),
        (Modul + "Api/Amo/AmoApply.cs", "AmoApply.Genereaza", "generarea amortizării lunare"),
        (Modul + "Api/Amo/AmoApply.cs", "AmoApply.Regenereaza", "regenerarea amortizării lunare"),
        (Modul + "Api/Itv/InchidereTvaApply.cs", "InchidereTvaApply.Genereaza", "generarea închiderii de TVA"),
        (Modul + "Api/Itv/InchidereTvaApply.cs", "InchidereTvaApply.Regenereaza", "regenerarea închiderii de TVA"),
        (Modul + "Motor/FiscalitateService.cs", "FiscalitateService.ConfirmaDepunerea", "confirmarea depunerii"),
        (Modul + "Cub/Materializare.Deschidere.cs", "Materializare.CereScriere", "deschiderea și stingerea ei, în tranzacția apelantului"),
        (Modul + "BusinessObjects/BackOfficeDbContext.cs", "BackOfficeEFCoreDbContext.SaveChanges", "poziția detaliilor noi (S-r9)"),
        (Modul + "BusinessObjects/BackOfficeDbContext.cs", "BackOfficeEFCoreDbContext.SaveChangesAsync", "poziția detaliilor noi (S-r9)"),
    ];

    static readonly (string Fisier, string Membru, string Rol)[] TranzactiiProprii = [
        (Modul + "Cub/Citiri/Fiscale.cs", "Fiscale.DeschideCitirea", "tranzacția de citire a unei declarații, RepeatableRead"),
    ];

    public static void VerificaSursa(Action<string, bool> check) {
        var arbori = Arbori();
        var intrari = Mentiuni(arbori);
        var straine = Incalcari(intrari, Intrari);
        var nefolosite = Intrari.Where(p => !intrari.Any(m => m.Fisier == p.Fisier && m.Membru == p.Membru))
            .Select(p => $"{p.Fisier} {p.Membru}").ToList();
        var mutant = Incalcari(Mentiuni(Cu(Modul + "Proiectii/Mutant.cs",
            "static class Mutant { static int Citeste(IObjectSpace os) { using var tx = TranzactieComanda.Incepe(os); return 0; } }")), Intrari);
        Console.WriteLine($"     MĂSURAT (X-D6): {intrari.Count} intrări sub blocajul scrierii în {intrari.Select(m => (m.Fisier, m.Membru)).Distinct().Count()} membri; "
            + $"în afara listei [{string.Join("; ", straine)}]; nefolosite [{string.Join("; ", nefolosite)}]; mutant [{string.Join("; ", mutant)}].");
        check("X-D6: blocajul scrierii se ia numai la intrarea comenzilor numite; o citire care deschide tranzacția de comandă e detectată",
            intrari.Count > 0 && straine.Count == 0 && nefolosite.Count == 0 && mutant.Count == 1 && mutant[0].Contains("Mutant.Citeste"));

        var tranzactii = Deschideri(arbori);
        var proprii = Incalcari(tranzactii.Where(t => t.Fisier != Tranzactie).ToList(), TranzactiiProprii);
        var literal = arbori.Where(a => a.Fisier != Tranzactie && a.Radacina.DescendantTokens()
                .Any(t => t.IsKind(SyntaxKind.StringLiteralToken) && t.ValueText.Contains("pg_advisory_xact_lock(97000", StringComparison.Ordinal)))
            .Select(a => a.Fisier).ToList();
        var mutantTranzactie = Incalcari(Deschideri(Cu(Modul + "Api/Mutant.cs",
            "static class Mutant { static void Scrie(DbContext db) { using var tx = db.Database.BeginTransaction(); } }"))
            .Where(t => t.Fisier != Tranzactie).ToList(), TranzactiiProprii);
        Console.WriteLine($"     MĂSURAT (X-D6): {tranzactii.Count} deschideri de tranzacție în producție; în afara `TranzactieComanda` și a citirii declarate "
            + $"[{string.Join("; ", proprii)}]; cheia blocajului în alt fișier [{string.Join("; ", literal)}]; mutant [{string.Join("; ", mutantTranzactie)}].");
        check("X-D6: producția deschide tranzacții numai prin `TranzactieComanda` și prin citirea declarată; cheia blocajului stă într-un singur loc",
            tranzactii.Any(t => t.Fisier == Tranzactie) && proprii.Count == 0 && literal.Count == 0
            && TranzactiiProprii.All(p => tranzactii.Any(t => t.Fisier == p.Fisier && t.Membru == p.Membru))
            && mutantTranzactie.Count == 1 && mutantTranzactie[0].Contains("Mutant.Scrie"));
    }

    static List<(string Fisier, string Membru, int Linie)> Mentiuni(IReadOnlyList<Arbore> arbori) => arbori
        .SelectMany(a => a.Radacina.DescendantNodes().OfType<MemberAccessExpressionSyntax>()
            .Where(m => m.Name.Identifier.ValueText is "Incepe" or "Asigura" or "AsiguraAsync" or "BlocajScriere"
                && UltimulNume(m.Expression) == "TranzactieComanda")
            .Select(m => (a.Fisier, Membru(m).Membru, Linie(m.Name.Identifier)))).ToList();

    static List<(string Fisier, string Membru, int Linie)> Deschideri(IReadOnlyList<Arbore> arbori) => arbori
        .SelectMany(a => a.Radacina.DescendantNodes().OfType<MemberAccessExpressionSyntax>()
            .Where(m => m.Name.Identifier.ValueText is "BeginTransaction" or "BeginTransactionAsync")
            .Select(m => (a.Fisier, Membru(m).Membru, Linie(m.Name.Identifier)))).ToList();

    static List<string> Incalcari(List<(string Fisier, string Membru, int Linie)> mentiuni,
            (string Fisier, string Membru, string Rol)[] permise) => mentiuni
        .Where(m => !permise.Any(p => p.Fisier == m.Fisier && p.Membru == m.Membru))
        .Select(m => $"{m.Fisier}:{m.Linie} {m.Membru}").ToList();
}
