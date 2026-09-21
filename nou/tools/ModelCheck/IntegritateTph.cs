using System.Reflection;
using System.Text;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Motor;
using Atlas.Conta.BackOffice.Module.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Atlas.Conta.BackOffice.ModelCheck;

// Integritatea tipului sub TPH (89), din metadata EF: ModelCheck o rulează (F28-H/I/J), `--dump-integritate-tph`
// o scrie ca fișier pentru bazele umplute pe ușa de sistem. Fiecare rând: încălcări (trebuie 0) + rânduri verificate.
sealed record ProbaTph(string Familie, string Eticheta, string Din, string Filtru, string Incalcare) {
    public string Sql => $"SELECT '{Text(Familie)}' AS familie, '{Text(Eticheta)}' AS eticheta, "
        + $"count(*) FILTER (WHERE {Incalcare}) AS incalcari, count(*) AS verificate FROM {Din} WHERE {Filtru}";

    static string Text(string s) => s.Replace("'", "''");
}

sealed record RezultatTph(string Familie, string Eticheta, long Incalcari, long Verificate);

static class IntegritateTph {
    public const string Linii = "linii";
    public const string Fk = "fk";
    public const string Coloane = "coloane";

    public static IReadOnlyList<ProbaTph> Probe(DbContext ctx) {
        var model = ctx.GetService<IDesignTimeModel>().Model;
        return [.. ProbeLinii(model), .. ProbeFk(ctx.Model, model), .. ProbeColoane(model)];
    }

    public static string Script(IReadOnlyList<ProbaTph> probe) {
        var sb = new StringBuilder()
            .AppendLine("-- Integritatea tipului sub TPH (89), generat de `ModelCheck --dump-integritate-tph`.")
            .AppendLine("-- Fiecare rând: `incalcari` trebuie să fie 0; `verificate` = rândurile acoperite.")
            .AppendLine($"-- {probe.Count} interogări: {string.Join(", ", probe.GroupBy(p => p.Familie).Select(g => $"{g.Key} {g.Count()}"))}.")
            .AppendLine("SELECT familie, eticheta, incalcari, verificate FROM (");
        for (var i = 0; i < probe.Count; i++)
            sb.Append(i == 0 ? "      " : "UNION ALL ").AppendLine($"SELECT {i} AS ord, x.* FROM ({probe[i].Sql}) x");
        return sb.AppendLine(") t ORDER BY ord;").ToString();
    }

    public static List<RezultatTph> Ruleaza(DbContext ctx, IReadOnlyList<ProbaTph> probe) {
        var conexiune = ctx.Database.GetDbConnection();
        var deschisa = conexiune.State == System.Data.ConnectionState.Open;
        if (!deschisa)
            conexiune.Open();
        try {
            using var comanda = conexiune.CreateCommand();
            comanda.CommandText = Script(probe);
            using var cititor = comanda.ExecuteReader();
            var rezultat = new List<RezultatTph>();
            while (cititor.Read())
                rezultat.Add(new(cititor.GetString(0), cititor.GetString(1), cititor.GetInt64(2), cititor.GetInt64(3)));
            return rezultat;
        }
        finally {
            if (!deschisa)
                conexiune.Close();
        }
    }

    // F28-H: liniile unui document sunt frunza declarată prin `[TipDetaliu]` (cu subtipurile) sau baza.
    static IEnumerable<ProbaTph> ProbeLinii(IModel model) {
        var documente = model.FindEntityType(typeof(Document))!;
        var detalii = model.FindEntityType(typeof(DocumentDetaliu))!;
        var fk = detalii.FindNavigation(nameof(DocumentDetaliu.Document))!.ForeignKey;
        var din = $"{Tabela(detalii)} l JOIN {Tabela(documente)} d ON d.{Coloana(documente, fk.PrincipalKey.Properties[0])} "
            + $"= l.{Coloana(detalii, fk.Properties[0])}";
        foreach (var tip in documente.GetDerivedTypesInclusive().Where(t => !t.ClrType.IsAbstract)) {
            var declarat = tip.ClrType.GetCustomAttribute<TipDetaliuAttribute>(inherit: false)?.TipDetaliu;
            var permise = (declarat == null ? [] : Discriminatori(model.FindEntityType(declarat)
                    ?? throw new InvalidOperationException($"[TipDetaliu({declarat.Name})] pe {tip.ClrType.Name} nu e în modelul EF.")))
                .Append(Discriminator(detalii)).Distinct().ToList();
            yield return new(Linii, $"{tip.ClrType.Name} → [{string.Join(", ", permise)}]", din,
                $"d.{Disc(documente)} = {Lit(Discriminator(tip))}", $"l.{Disc(detalii)} NOT IN ({Lista(permise)})");
        }
    }

    // F28-I: ținta unui FK spre frunză are discriminatorul frunzei sau al unui subtip (regula (o), fără gardian).
    static IEnumerable<ProbaTph> ProbeFk(IModel runtime, IModel model) {
        foreach (var tipRuntime in runtime.GetEntityTypes())
            foreach (var fkRuntime in GardianEditare.FkSpreFrunze(tipRuntime).Where(f => f.DeclaringEntityType == tipRuntime)) {
                var dependent = model.FindEntityType(tipRuntime.Name)!;
                var principal = model.FindEntityType(fkRuntime.PrincipalEntityType.Name)!;
                var radacina = principal.GetRootType();
                var coloana = Coloana(dependent, dependent.FindProperty(fkRuntime.Properties[0].Name)!);
                var filtru = $"dep.{coloana} IS NOT NULL";
                if (dependent.FindDiscriminatorProperty() != null && dependent.BaseType != null)
                    filtru += $" AND dep.{Disc(dependent)} IN ({Lista(Discriminatori(dependent))})";
                yield return new(Fk, $"{dependent.ClrType.Name}.{fkRuntime.Properties[0].Name} → {principal.ClrType.Name}",
                    $"{Tabela(dependent)} dep JOIN {Tabela(radacina)} p ON p.{Coloana(radacina, radacina.FindPrimaryKey()!.Properties[0])} "
                    + $"= dep.{coloana}",
                    filtru, $"p.{Disc(radacina)} NOT IN ({Lista(Discriminatori(principal))})");
            }
    }

    // F28-J: o coloană declarată pe tipuri derivate e NULL pe rândurile oricărui alt tip al ierarhiei.
    static IEnumerable<ProbaTph> ProbeColoane(IModel model) {
        foreach (var radacina in model.GetEntityTypes().Where(e => e.BaseType == null
                     && e.FindDiscriminatorProperty() != null && e.GetDerivedTypes().Any())) {
            var peColoana = radacina.GetDerivedTypes()
                .SelectMany(t => t.GetDeclaredProperties().Select(p => (Tip: t, Proprietate: p)))
                .Where(x => x.Proprietate.GetComputedColumnSql() == null)
                .GroupBy(x => x.Proprietate.GetColumnName(), StringComparer.Ordinal);
            foreach (var g in peColoana.OrderBy(g => g.Key, StringComparer.Ordinal)) {
                var permise = g.SelectMany(x => Discriminatori(x.Tip)).Distinct().ToList();
                yield return new(Coloane, $"{radacina.GetTableName()}.{g.Key} ← [{string.Join(", ", g.Select(x => x.Tip.ClrType.Name).Distinct())}]",
                    Tabela(radacina), $"{Disc(radacina)} NOT IN ({Lista(permise)})", $"{Id(g.Key)} IS NOT NULL");
            }
        }
    }

    static string Discriminator(IEntityType tip) => (string)tip.GetDiscriminatorValue()!;

    static List<string> Discriminatori(IEntityType tip) =>
        tip.GetDerivedTypesInclusive().Select(t => t.GetDiscriminatorValue() as string).OfType<string>().ToList();

    static string Tabela(IEntityType tip) {
        var radacina = tip.GetRootType();
        var nume = radacina.GetTableName() ?? throw new InvalidOperationException($"{radacina.Name} n-are tabelă.");
        return radacina.GetSchema() is { } schema ? $"{Id(schema)}.{Id(nume)}" : Id(nume);
    }

    static string Coloana(IEntityType tip, IProperty proprietate) {
        var radacina = tip.GetRootType();
        return Id(proprietate.GetColumnName(StoreObjectIdentifier.Table(radacina.GetTableName()!, radacina.GetSchema()))
            ?? throw new InvalidOperationException($"{tip.Name}.{proprietate.Name} n-are coloană."));
    }

    static string Disc(IEntityType tip) => Coloana(tip, tip.FindDiscriminatorProperty()!);

    static string Id(string nume) => $"\"{nume.Replace("\"", "\"\"")}\"";

    static string Lit(string valoare) => $"'{valoare.Replace("'", "''")}'";

    static string Lista(IEnumerable<string> valori) => string.Join(", ", valori.Select(Lit));
}
