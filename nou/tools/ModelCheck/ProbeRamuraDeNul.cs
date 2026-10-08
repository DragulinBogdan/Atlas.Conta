using System.Diagnostics;
using System.Reflection;
using System.Text.RegularExpressions;
using Atlas.Conta.BackOffice.Module.Cub;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Atlas.Conta.BackOffice.ModelCheck;

/// <summary>
/// Egalitățile cu ramură de nul între două surse (`a = b OR (a IS NULL AND b IS NULL)`),
/// în tot SQL-ul emis de EF Core în proces, de la <see cref="Porneste"/> până la <see cref="Verifica"/>.
/// </summary>
sealed class ProbeRamuraDeNul : IObserver<DiagnosticListener>, IObserver<KeyValuePair<string, object>> {
    public sealed record Gasire(string Cheie, string Tabele, string Scena, string Sql);

    const string Martor = "RAMURA-NUL martor";

    // Cheia = apelantul din produs (sau din unealtă, când interogarea se execută acolo) + coloanele comparate în comandă.
    // Valoarea = egalitatea simplă pe cheia selectivă care corelează aceleași surse; admiterea ține cât timp ea e în comandă.
    static readonly Dictionary<string, string> Admise = new(StringComparer.Ordinal) {
        ["Explicatii.VerificaAcoperire: DocumentId"] = "\"ID\" = \"ExplicatieDinId\"",
        ["Explicatii.VerificaAcoperire: Unitate"] = "\"ID\" = \"InversaDinId\"",
        ["Invarianti.VerificaFiscal: CotaTva, DeImport, DocumentFiscalId, RegimTva, RolTva, SensTva"] = "\"ID\" = \"InversaDinId\"",
        ["Invarianti.VerificaPerechi: Cantitate0, CentruCost, CodEconomic, CodFunctional, DocumentId, LinieId, Produs, Proiect, SursaFinantare, UnitateOrganizatorica"]
            = "\"TranzactieId\" = \"TranzactieId\"",
        ["AcoperireInvarianti.PerecheLangaTransformare: DocumentId, LinieId"] = "\"TranzactieId\" = \"TranzactieId\"",
    };

    const string Operand = @"(?:[^()\n]|\((?:[^()\n]|\([^()\n]*\))*\))+?";
    static readonly Regex Ramura = new($@"\bOR \((?<s>{Operand}) IS NULL AND (?<d>{Operand}) IS NULL\)",
        RegexOptions.NonBacktracking | RegexOptions.CultureInvariant);
    static readonly Regex Alias = new(@"(?<![\w""])([A-Za-z_]\w*)\.""", RegexOptions.CultureInvariant);

    static readonly ProbeRamuraDeNul Instanta = new();
    readonly Dictionary<string, Gasire> gasiri = new(StringComparer.Ordinal);
    readonly List<IDisposable> abonari = [];
    int comenzi, martori;

    public static void Porneste() {
        lock (Instanta.abonari)
            if (Instanta.abonari.Count == 0)
                Instanta.abonari.Add(DiagnosticListener.AllListeners.Subscribe(Instanta));
    }

    /// <summary>Predicatele cu ramură de nul între două surse diferite, cu aliasurile scoase.</summary>
    public static IEnumerable<string> Gaseste(string sql) {
        if (!sql.Contains(" IS NULL AND ", StringComparison.Ordinal))
            yield break;
        foreach (Match m in Ramura.Matches(sql)) {
            string s = m.Groups["s"].Value, d = m.Groups["d"].Value;
            var stanga = Alias.Matches(s).Select(a => a.Groups[1].Value).ToHashSet(StringComparer.Ordinal);
            var dreapta = Alias.Matches(d).Select(a => a.Groups[1].Value).ToHashSet(StringComparer.Ordinal);
            if (stanga.Count == 0 || dreapta.Count == 0 || stanga.Overlaps(dreapta))
                continue;
            string a = Alias.Replace(s, "").Replace("\"", ""), b = Alias.Replace(d, "").Replace("\"", "");
            yield return a == b ? a : $"{a} = {b}";
        }
    }

    public static void Verifica(Suita s) {
        using (var os = s.Deschide()) {
            var postari = os.GetObjectsQuery<Postare>();
            _ = (from a in postari.Where(p => p.ID == Guid.Empty)
                 join b in postari on new { a.Unitate, a.Partener } equals new { b.Unitate, b.Partener }
                 select b.ID).TagWith(Martor).ToList();
        }
        Gasire[] gasite; int vazute, martori;
        lock (Instanta.gasiri) {
            gasite = [.. Instanta.gasiri.Values.OrderBy(g => g.Cheie, StringComparer.Ordinal)];
            vazute = Instanta.comenzi; martori = Instanta.martori;
        }
        Console.WriteLine($"     RAMURA-NUL: {vazute} comenzi SQL citite; {gasite.Length} forme cu ramură de nul între două surse.");
        s.Check("RAMURA-NUL: detectorul prinde îmbinarea martor pe două chei nulabile ale lui `Postare`", martori == 2);
        var neadmise = gasite.Where(g => !Admise.TryGetValue(g.Cheie, out var cheieSelectiva)
            || !Alias.Replace(g.Sql, "\"").Contains(cheieSelectiva, StringComparison.Ordinal)).ToArray();
        var director = Path.Combine(Path.GetTempPath(), "ramura-nul");
        if (neadmise.Length != 0)
            Directory.CreateDirectory(director);
        foreach (var (g, i) in neadmise.Select((g, i) => (g, i + 1))) {
            File.WriteAllLines(Path.Combine(director, $"{i:00}.sql"), [$"-- {g.Cheie}", $"-- {g.Scena}", g.Sql]);
            Console.WriteLine($"     RAMURA-NUL {i:00} {g.Cheie} [{g.Tabele}] în {g.Scena}");
        }
        if (neadmise.Length != 0)
            Console.WriteLine($"     RAMURA-NUL: SQL-ul fiecărei forme în {director}");
        s.Check("RAMURA-NUL: nicio îmbinare cu ramură de nul în SQL-ul rulării, în afara celor admise nominal", neadmise.Length == 0);
        if (s.FiltruScenarii != null)
            return;
        var moarte = Admise.Keys.Where(k => gasite.All(g => g.Cheie != k)).ToArray();
        foreach (var k in moarte)
            Console.WriteLine($"     RAMURA-NUL admisă fără obiect: {k}");
        s.Check("RAMURA-NUL: fiecare formă admisă nominal apare în rulare", moarte.Length == 0);
    }

    public void OnNext(DiagnosticListener listener) {
        if (listener.Name == DbLoggerCategory.Name)
            lock (abonari) abonari.Add(listener.Subscribe(this));
    }

    public void OnNext(KeyValuePair<string, object> eveniment) {
        if (eveniment.Key != RelationalEventId.CommandExecuting.Name || eveniment.Value is not CommandEventData date)
            return;
        var sql = date.Command.CommandText;
        var predicate = Gaseste(sql).Distinct(StringComparer.Ordinal).ToArray();
        lock (gasiri) {
            comenzi++;
            if (predicate.Length == 0)
                return;
            if (sql.Contains(Martor, StringComparison.Ordinal)) {
                martori += predicate.Length;
                return;
            }
            var (apelant, scena) = Apelanti();
            var cheie = $"{apelant}: {string.Join(", ", predicate.Order(StringComparer.Ordinal))}";
            gasiri.TryAdd(cheie, new(cheie, string.Join(", ", CapturaSql.Tabele([sql]).Order(StringComparer.Ordinal)), scena, sql));
        }
    }

    public void OnCompleted() { }
    public void OnError(Exception error) { }

    static (string Apelant, string Scena) Apelanti() {
        string apelant = null, scena = null;
        foreach (var cadru in new StackTrace(false).GetFrames()) {
            var metoda = cadru.GetMethod();
            var tip = metoda?.DeclaringType;
            if (tip == null || tip == typeof(ProbeRamuraDeNul) || tip == typeof(CapturaSql))
                continue;
            var unealta = tip.Assembly == typeof(ProbeRamuraDeNul).Assembly;
            if (!unealta && tip.Assembly != typeof(Postare).Assembly)
                continue;
            apelant ??= Nume(metoda);
            if (unealta) { scena = Nume(metoda); break; }
        }
        return (apelant ?? "?", scena ?? "?");
    }

    static string Nume(MethodBase metoda) {
        var tip = metoda.DeclaringType;
        var nume = metoda.Name;
        for (; tip.DeclaringType != null && tip.Name.Contains('<'); tip = tip.DeclaringType)
            if (nume == "MoveNext" || nume.StartsWith("<>", StringComparison.Ordinal))
                nume = tip.Name;
        if (nume.IndexOf('<') is >= 0 and var i && nume.IndexOf('>', i) is > 0 and var j && j > i + 1)
            nume = nume[(i + 1)..j];
        return $"{tip.Name}.{nume}";
    }
}
