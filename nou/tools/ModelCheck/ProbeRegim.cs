using System.Text.RegularExpressions;
using Atlas.Conta.BackOffice.Module.Api;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Controllers;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.SystemModule;
using DevExpress.Persistent.Base;

namespace Atlas.Conta.BackOffice.ModelCheck;

// Probele structurale ale regimului pe stare (106e).
static class ProbeRegim {
    public static void VerificaActiuni(Action<string, bool> check) {
        var abateri = new List<string>();
        var potrivite = new List<string>();
        foreach (var tip in typeof(RegimDocumentController).Assembly.GetTypes()
                     .Where(t => !t.IsAbstract && typeof(ViewController).IsAssignableFrom(t))) {
            var (view, obiect) = TintaObjectView(tip);
            if (obiect == null || view != typeof(DetailView) || !typeof(Document).IsAssignableFrom(obiect))
                continue;
            if (tip.GetConstructor(Type.EmptyTypes) == null) {
                abateri.Add($"{tip.Name}: fără constructor implicit");
                continue;
            }
            using var controller = (ViewController)Activator.CreateInstance(tip)!;
            foreach (var actiune in controller.Actions.Where(a => a.Category == nameof(PredefinedCategory.RecordEdit))) {
                var comanda = RegimDocumentController.Comanda(actiune);
                if (comanda == null)
                    abateri.Add($"{tip.Name}: {actiune.Id}");
                else
                    potrivite.Add($"{actiune.Id}→{comanda}");
            }
        }
        Console.WriteLine($"     MĂSURAT (106e): comenzi potrivite [{string.Join("; ", potrivite)}]; abateri [{string.Join("; ", abateri)}].");
        using (var stergere = new DeleteObjectsViewController())
            check("106e: ștergerea standard XAF e comanda `Sterge` a regimului",
                RegimDocumentController.Comanda(stergere.DeleteAction) == RegimDocument.Sterge);
        check("106e: fiecare comandă din toolbar-ul DetailView-ului unui `Document` numește prin sufixul ID-ului o comandă a regimului",
            abateri.Count == 0 && potrivite.Count >= 5);
    }

    // Refuzurile pe care regimul nu le promite (106i): valorile și, la stornare și corecție, data cerută.
    static readonly string[] RefuzuriPeValori = ["STOC_INSUFICIENT", "POZITIE_FARA_FISA_NEGATIVA"];
    static readonly string[] RefuzuriPeDataCeruta = ["cu o dată din luna lui", "PARTIDA_CU_DEPENDENTI"];

    public static bool RefuzLaComanda(ComandaDocument comanda, string mesaj) =>
        RefuzuriPeValori.Any(f => mesaj.Contains(f, StringComparison.Ordinal))
        || comanda != ComandaDocument.AnuleazaOperarea
            && RefuzuriPeDataCeruta.Any(f => mesaj.Contains(f, StringComparison.Ordinal));

    static (Type View, Type Obiect) TintaObjectView(Type tip) {
        for (var b = tip; b != null; b = b.BaseType)
            if (b.IsGenericType && b.GetGenericTypeDefinition() == typeof(ObjectViewController<,>))
                return (b.GetGenericArguments()[0], b.GetGenericArguments()[1]);
        return (null, null);
    }

    static readonly Regex AffordancePeStare = new(@"\bPoate\w*\s*=\s*[^,;\n]*\bStareDocument\.");
    static readonly Regex CheieStare = new(@"\[\s*""Stare""\s*\]");
    static readonly Regex StergereInTip = new(@"\bRegimDocument\.Sterge\b|""Sterge""");
    static readonly Regex ComentariuBloc = new(@"/\*.*?\*/", RegexOptions.Singleline);
    static readonly Regex ComentariuLinie = new(@"(?<!:)//.*$", RegexOptions.Multiline);

    public static void VerificaSursa(Action<string, bool> check) {
        var radacina = Path.GetFullPath(Path.Combine(MetadataDump.DirectorProiect(), "..", "..", "Atlas.Conta.BackOffice"));
        var incalcari = new List<string>();
        var fisiere = 0;
        foreach (var fisier in Directory.EnumerateFiles(radacina, "*.cs", SearchOption.AllDirectories)
                     .Select(f => Path.GetRelativePath(radacina, f).Replace('\\', '/'))
                     .Where(f => !f.Split('/').Any(d => d is "bin" or "obj" or "Migrations"))) {
            var text = ComentariuLinie.Replace(ComentariuBloc.Replace(File.ReadAllText(Path.Combine(radacina, fisier)),
                m => new string('\n', m.Value.Count(c => c == '\n'))), "");
            fisiere++;
            var regex = fisier.Contains("/Api/") ? AffordancePeStare : fisier.Contains("/Controllers/") ? CheieStare
                : fisier.Contains("/BusinessObjects/") ? StergereInTip : null;
            if (regex == null)
                continue;
            incalcari.AddRange(regex.Matches(text).Select(m => $"{fisier}:{text[..m.Index].Count(c => c == '\n') + 1} {m.Value.Trim()}"));
        }
        Console.WriteLine($"     MĂSURAT (106c/d/j): {fisiere} fișiere; încălcări [{string.Join("; ", incalcari)}].");
        check("106c/d/j: nicio affordance din `Api/` nu se calculează din `StareDocument`, niciun controller nu poartă cheia „Stare” "
            + "și niciun tip nu decide `Sterge` — regimul e singura sursă",
            fisiere > 100 && incalcari.Count == 0);
    }
}
