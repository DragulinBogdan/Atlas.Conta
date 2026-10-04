using System.Reflection;
using Atlas.Conta.BackOffice.Module.Cub.Citiri;
using DevExpress.ExpressApp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using C = Atlas.Conta.BackOffice.Module.Cub;
using static Atlas.Conta.BackOffice.ModelCheck.SursaProductie;

namespace Atlas.Conta.BackOffice.ModelCheck;

// N-r8: `Transfer` și inversele lui se exclud într-un singur loc, iar consumatorii nu refiltrează.
static class ProbeTransferCititori {
    // `Indiferent`: intrarea nu filtrează felul, domeniul ei e dat de coordonate; N-r8 nu i se aplică.
    public enum Regim { Exclude, Include, Indiferent }

    public sealed record Intrare(string Nume, Regim Regim, Func<IObjectSpace, IQueryable<Guid>> Postari);

    public static readonly Intrare[] Intrari = [
        new("Contabil.Postari", Regim.Exclude, os => Contabil.Postari(os).Select(p => p.ID)),
        new("Contabil.Jurnal", Regim.Exclude, os => Contabil.Jurnal(os, DateOnly.MinValue, DateOnly.MaxValue).Select(p => p.Id)),
        new("Plati.Postari", Regim.Exclude, os => Plati.Postari(os).Select(p => p.Id)),
        new("Loturi.Postari", Regim.Include, os => Loturi.Postari(os).Select(p => p.ID)),
        new("Partide.Postari", Regim.Include, os => Partide.Postari(os).Select(p => p.ID)),
        new("Fiscale.Postari", Regim.Indiferent, os => Fiscale.Postari(os).Select(p => p.ID)),
        new("Imobilizari.PozitiiFaraFisa", Regim.Indiferent, os => Imobilizari.PozitiiFaraFisa(os).Select(p => p.ID)),
    ];

    static readonly (string Fisier, string Membru, string Rol)[] Permise = [
        (Modul + "Cub/Materializare.cs", "Materializare.VerificaPartideFaraDependenti", "scriitorul: retragerea refuză partidele cu dependenți"),
        (Modul + "Cub/Materializare.cs", "Materializare.DesfaceTransfer", "scriitorul: inversa transferului unei legături"),
        (Modul + "Cub/Materializare.cs", "Materializare.Storneaza", "scriitorul: stornoul inversează și transferurile"),
        (Modul + "Cub/Materializare.cs", "Materializare.Anuleaza", "scriitorul: anularea șterge și transferurile"),
        (Modul + "Cub/Materializare.StingereDeschidere.cs", "Materializare.StingeriDeschidere", "scriitorul: stingerea pozițiilor inițiale"),
        (Modul + "Declaratii/DeclarantAsamblare.cs", "DeclarantAsamblare.Declara", "producătorul alege felul tranzacției"),
        (Modul + "Cub/Citiri/Loturi.cs", "Loturi.VerificaRetragere", "retragerea reverifică soldul și pe transferurile proprii"),
        (Modul + "Cub/Citiri/Plati.cs", "Plati.Alocari", "alocarea plăților urmează transferurile pe partida proprie"),
        (Modul + "Cub/Citiri/Imobilizari.cs", "Imobilizari.VerificaAcoperire", "martor: transfer valoric fără suport"),
        (Modul + "Cub/Citiri/Explicatii.cs", "Explicatii.VerificaAcoperire", "martor: ieșirile pe lot din transferuri cer explicație"),
        (Modul + "Cub/Citiri/Invarianti.cs", "Invarianti.VerificaTransferuri", "martor: transferul persistat conservă pe cont (090f)"),
        (Modul + "Saft/SaftProiectii.PeCub.Stocuri.cs", "StocuriPeCub.Referinta", "eticheta mișcării, nu filtru"),
    ];

    public static void VerificaSursa(Action<string, bool> check) {
        var mentiuni = Mentiuni(Arbori());
        var incalcari = Incalcari(mentiuni);
        var nefolosite = Permise.Where(p => !mentiuni.Any(m => m.Fisier == p.Fisier && m.Membru == p.Membru))
            .Select(p => $"{p.Fisier} {p.Membru}").ToList();
        var mutant = Incalcari(Mentiuni(Cu(Modul + "Proiectii/Mutant.cs", "static class Mutant { static IQueryable<Postare> Fara(IObjectSpace os) => "
            + "Loturi.Postari(os).Where(p => p.Tranzactie.Fel != N.FelTranzactie.Transfer); }")));
        Console.WriteLine($"     MĂSURAT (N-r8): {mentiuni.Count} mențiuni `FelTranzactie.Transfer` în {mentiuni.Select(m => (m.Fisier, m.Membru)).Distinct().Count()} "
            + $"membri; încălcări [{string.Join("; ", incalcari)}]; permise nefolosite [{string.Join("; ", nefolosite)}]; mutant [{string.Join("; ", mutant)}].");
        check("N-r8: `FelTranzactie.Transfer` apare în producție numai în scriitor, producător, cititorii comuni numiți și eticheta SAF-T; "
            + "un consumator care refiltrează `Transfer` e detectat", mentiuni.Count > 0 && incalcari.Count == 0 && nefolosite.Count == 0
            && mutant.Count == 1 && mutant[0].Contains("Mutant.Fara"));

        var declarate = Intrari.Select(i => i.Nume).ToHashSet();
        Type[] randuri = [typeof(C.Postare), typeof(PostareJurnal), typeof(PostarePlata)];
        var publice = typeof(Contabil).Assembly.GetTypes().Where(t => t.Namespace == typeof(Contabil).Namespace && t.IsPublic)
            .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly))
            .Where(m => m.ReturnType.IsGenericType && m.ReturnType.GetGenericTypeDefinition() == typeof(IQueryable<>)
                && randuri.Contains(m.ReturnType.GetGenericArguments()[0]))
            .Select(m => $"{m.DeclaringType.Name}.{m.Name}").Distinct().OrderBy(n => n).ToList();
        Console.WriteLine($"     MĂSURAT (N-r8): intrări publice pe rânduri de cub [{string.Join(", ", publice)}]; "
            + $"regim [{string.Join(", ", Intrari.Select(i => $"{i.Nume} {i.Regim}"))}].");
        check("N-r8: fiecare intrare publică din `Cub/Citiri` care întoarce rânduri de cub are regimul față de `Transfer` declarat",
            publice.Count > 0 && declarate.SetEquals(publice));
    }

    public static List<(string Nume, Regim Regim, int Numar)> Numara(IObjectSpace os, IReadOnlyCollection<Guid> postari) =>
        Intrari.Select(i => (i.Nume, i.Regim, i.Postari(os).Count(id => postari.Contains(id)))).ToList();

    static List<(string Fisier, string Membru, int Linie)> Mentiuni(IReadOnlyList<Arbore> arbori) => arbori
        .SelectMany(a => a.Radacina.DescendantNodes().OfType<MemberAccessExpressionSyntax>()
            .Where(m => m.Name.Identifier.ValueText == "Transfer" && UltimulNume(m.Expression) == "FelTranzactie")
            .Select(m => (a.Fisier, Membru(m).Membru, Linie(m.Name.Identifier)))).ToList();

    static List<string> Incalcari(List<(string Fisier, string Membru, int Linie)> mentiuni) => mentiuni
        .Where(m => !Permise.Any(p => p.Fisier == m.Fisier && p.Membru == m.Membru))
        .Select(m => $"{m.Fisier}:{m.Linie} {m.Membru}").ToList();
}
