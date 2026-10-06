using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Atlas.Conta.BackOffice.Module.Api.Perioade;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Cub.Citiri;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.EFCore;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Atlas.Conta.BackOffice.ModelCheck;

// D9-A1: scara de volum — scena PerfCub privată (m = 12, k = 64) păstrată și multiplicată „în lățime” direct în SQL.
// Raport, fără prag: fiecare treaptă multiplică, face VACUUM (ANALYZE), reface snapshot-urile și măsoară cititorii comuni.
static partial class ScaraVolum {
    const int Istoric = 12, UnitatiIstoric = 16, K = 64, CopiiPeTransa = 25;
    const string PrefixReconstructie = "SCARA-RECON-JSON ";
    static readonly TimeSpan LimitaMasurare = TimeSpan.FromMinutes(10), LimitaReconstructie = TimeSpan.FromMinutes(30);
    static readonly string[] Excluse = ["JTVA", "D300", "D394", "R6", "SAFTL", "SAFTS", "INCH"];
    static readonly string[] TabeleAtinse = ["Repartitori", "Documente", "Loturi", "Tranzactie", "Postare"];
    static readonly string[] TabeleSnapshot = ["SolduriPerioadaContabil", "SolduriPerioadaStoc", "PartideDeschise"];

    public sealed record Proba(string Nume, string Cheie, decimal Scena, decimal Asteptat, decimal Baza, bool Ok);
    public sealed record Reconstructie(double Ms, int Comenzi, int Referinte, long Diferite, long Contabil, long Stoc, long Partide);
    public sealed record Masurare(string Operatie, string Ruta, PerfCub.Masura Masura, Dictionary<string, decimal> Asteptat, bool? ControlOk);

    public sealed class Treapta {
        public int Factor { get; set; }
        public double MsMultiplicare { get; set; }
        public double MsVacuum { get; set; }
        public double MsAnalyzeSnapshot { get; set; }
        public Reconstructie Reconstructie { get; set; }
        public string StareReconstructie { get; set; }
        public Dictionary<string, decimal> Forma { get; set; } = [];
        public List<Proba> Probe { get; set; } = [];
        public List<Masurare> Masurari { get; set; } = [];
        public List<string> Nemasurate { get; set; } = [];
    }

    sealed record Rulare(DateTime Inceput, string Baza, long PostariScena, int OriginiScena, int FactorF, List<int> Factori, double MsScena,
        Dictionary<string, string> Configuratie, string Mediu, string Oprire, List<Treapta> Trepte);

    static readonly Lazy<Dictionary<string, string>> Sectiuni = new(() => {
        using var flux = typeof(ScaraVolum).Assembly.GetManifestResourceStream("ScaraVolum.sql")
            ?? throw new InvalidOperationException("Resursa ScaraVolum.sql lipsește din asamblare.");
        using var cititor = new StreamReader(flux, Encoding.UTF8);
        var sectiuni = new Dictionary<string, StringBuilder>();
        StringBuilder curenta = null;
        for (string linie; (linie = cititor.ReadLine()) != null;) {
            if (linie.StartsWith("-- @@ ", StringComparison.Ordinal)) sectiuni[linie[6..].Trim()] = curenta = new();
            else curenta?.AppendLine(linie);
        }
        return sectiuni.ToDictionary(s => s.Key, s => s.Value.ToString());
    });

    static string Env(string nume) => Environment.GetEnvironmentVariable(nume) is { Length: > 0 } v ? v : null;

    static NpgsqlCommand Comanda(NpgsqlConnection c, string text, params (string Nume, object Valoare)[] parametri) {
        var cmd = new NpgsqlCommand(text, c) { CommandTimeout = 0 };
        foreach (var (nume, valoare) in parametri) cmd.Parameters.AddWithValue(nume, valoare);
        return cmd;
    }

    static void Executa(NpgsqlConnection c, string text, params (string Nume, object Valoare)[] parametri) {
        using var cmd = Comanda(c, text, parametri);
        cmd.ExecuteNonQuery();
    }

    static T Scalar<T>(NpgsqlConnection c, string text) {
        using var cmd = Comanda(c, text);
        return (T)Convert.ChangeType(cmd.ExecuteScalar(), typeof(T), CultureInfo.InvariantCulture);
    }

    static List<Proba> Probe(NpgsqlConnection c, string sectiune, int f, params (string Nume, object Valoare)[] parametri) {
        using var cmd = Comanda(c, Sectiuni.Value[sectiune], parametri);
        using var r = cmd.ExecuteReader();
        var probe = new List<Proba>();
        while (r.Read()) {
            decimal scena = r.GetDecimal(2), baza = r.GetDecimal(3), asteptat = r.GetString(4) == "f" ? f * scena : scena;
            probe.Add(new(r.GetString(0), r.GetString(1), scena, asteptat, baza, asteptat == baza));
        }
        return probe;
    }

    static Dictionary<string, T> Perechi<T>(NpgsqlConnection c, string sectiune, params (string Nume, object Valoare)[] parametri) {
        using var cmd = Comanda(c, Sectiuni.Value[sectiune], parametri);
        using var r = cmd.ExecuteReader();
        var perechi = new Dictionary<string, T>();
        while (r.Read()) perechi[r.GetString(0)] = r.GetFieldValue<T>(1);
        return perechi;
    }

    public static void Ruleaza(string conexiune, Func<IObjectSpace> deschide, Action<string, bool> check,
            Action<IObjectSpace, int, int> inchide, DbContextOptions<BackOfficeEFCoreDbContext> optiuni) {
        var director = Env("SCARA_VOLUM_DIR") ?? Path.Combine(Duk.DirectorTemporar(), $"scara-volum-{DateTime.UtcNow:yyyyMMdd-HHmmss}");
        Directory.CreateDirectory(director);
        var total = Stopwatch.StartNew();
        var buget = TimeSpan.FromMinutes(int.Parse(Env("SCARA_VOLUM_BUGET_MIN") ?? "150", CultureInfo.InvariantCulture));
        var tinta = long.Parse(Env("SCARA_VOLUM_POSTARI") ?? "5000000", CultureInfo.InvariantCulture);
        using var sql = new NpgsqlConnection(conexiune);
        sql.Open();
        var curata = !Scalar<bool>(sql, "select exists (select 1 from pg_namespace where nspname = 'scara')")
            && Scalar<long>(sql, "select count(*) from \"Postare\"") == 0;
        check("SCARA: baza e curată la pornire (fără schema `scara`, fără postări)", curata);
        if (!curata) return;

        PerfCub.Punct punct = null;
        var scena = new PerfCub(deschide, check, true, inchide, 2060 + Istoric / 3, Istoric, UnitatiIstoric, [K],
            p => { punct = p; return []; }, director, optiuni) { Pastrata = true };
        var ceas = Stopwatch.StartNew();
        scena.Ruleaza();
        var msScena = ceas.Elapsed.TotalMilliseconds;
        check("SCARA: scena PerfCub privată m = 12, k = 64 e păstrată, cu punctul ei de măsurare", punct != null
            && Scalar<long>(sql, "select count(*) from \"Postare\"") > 0);
        if (punct == null) return;

        Executa(sql, Sectiuni.Value["fotografie"]);
        var postariScena = Scalar<long>(sql, "select count(*) from scara.postare");
        int originiScena;
        using (var os = deschide()) originiScena = Partide.Origini(os).Count();
        var factorF = (int)((tinta + postariScena - 1) / postariScena);
        var factori = (Env("SCARA_VOLUM_F") ?? "1,10,100,F").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(x => x == "F" ? factorF : int.Parse(x, CultureInfo.InvariantCulture)).Distinct().Order().ToList();
        Console.WriteLine($"     SCARA scena: {postariScena} postări, {originiScena} partide cu origine, {msScena / 1000:0} s; F = {factorF}; trepte [{string.Join(", ", factori)}]");

        var (an, luna) = scena.LunaMasurata;
        DateOnly inceput = new(an, luna, 1), sfarsit = new(an, luna, DateTime.DaysInMonth(an, luna));
        var operatii = PerfCub.Operatii.Where(o => !Excluse.Contains(o.Cod.Split('-')[0])).ToList();
        var rulare = new Rulare(DateTime.UtcNow, sql.Database, postariScena, originiScena, factorF, factori, msScena,
            Perechi<string>(sql, "configuratie"), Env("SCARA_VOLUM_MEDIU"), null, []);
        var facut = 1;
        string oprire = null;
        foreach (var f in factori) {
            var t = new Treapta { Factor = f };
            rulare.Trepte.Add(t);
            ceas.Restart();
            for (var c = facut; c < f; c += CopiiPeTransa) {
                using var tx = sql.BeginTransaction();
                Executa(sql, Sectiuni.Value["copiaza"], ("de_la", c), ("pana_la", Math.Min(f, c + CopiiPeTransa)));
                tx.Commit();
            }
            t.MsMultiplicare = ceas.Elapsed.TotalMilliseconds;
            ceas.Restart();
            foreach (var tabela in TabeleAtinse) Executa(sql, $"VACUUM (ANALYZE) \"{tabela}\"");
            t.MsVacuum = ceas.Elapsed.TotalMilliseconds;
            Console.WriteLine($"     SCARA f={f}: copiile {facut}…{f - 1} în {t.MsMultiplicare / 1000:0.0} s, VACUUM (ANALYZE) {t.MsVacuum / 1000:0.0} s");

            t.Probe.AddRange(Probe(sql, "probe", f));
            if (f > 1) t.Probe.AddRange(Probe(sql, "probe_copie", 1, ("c", f - 1)));
            using (var os = deschide()) {
                var origini = Partide.Origini(os).Count();
                t.Probe.Add(new("partide cu origine recunoscută de `Partide.Origini`", "", originiScena, (decimal)f * originiScena, origini,
                    origini == (long)f * originiScena));
            }
            foreach (var grup in t.Probe.GroupBy(p => p.Nume)) {
                foreach (var p in grup.Where(p => !p.Ok))
                    Console.WriteLine($"     SCARA PROBA f={f} {p.Nume} [{p.Cheie}]: scena {Nr(p.Scena)}, așteptat {Nr(p.Asteptat)}, în bază {Nr(p.Baza)}");
                check($"SCARA f={f}: {grup.Key} ({grup.Count()} chei)", grup.All(p => p.Ok));
            }
            if (t.Probe.Any(p => !p.Ok)) {
                oprire = $"probă de corectitudine picată la f = {f}; treapta nu s-a măsurat";
                break;
            }

            var recon = Reconstruieste(conexiune);
            t.Reconstructie = recon.Rezultat;
            t.StareReconstructie = recon.Stare;
            if (recon.Rezultat == null && recon.Oprita) Console.WriteLine($"     SCARA f={f}: prima reconstrucție — {recon.Stare}; rutele -NI și RECON-N nu se măsoară");
            else check($"SCARA f={f}: prima reconstrucție a snapshot-urilor — {recon.Stare}", recon.Rezultat != null);
            ceas.Restart();
            foreach (var tabela in TabeleSnapshot) Executa(sql, $"ANALYZE \"{tabela}\"");
            t.MsAnalyzeSnapshot = ceas.Elapsed.TotalMilliseconds;
            t.Forma = Perechi<decimal>(sql, "forma", ("inceput", inceput), ("sfarsit", sfarsit));
            Scrie(director, rulare with { Oprire = "în lucru" });

            var directorTreapta = Path.Combine(director, $"f{f:0000}");
            Directory.CreateDirectory(directorTreapta);
            foreach (var op in operatii) {
                if (recon.Rezultat == null && (op.Cod.EndsWith("-NI", StringComparison.Ordinal) || op.Cod == "RECON-N")) {
                    t.Nemasurate.Add($"{op.Cod}: nemăsurat, snapshot nerefăcut");
                    continue;
                }
                if (total.Elapsed > buget) {
                    t.Nemasurate.Add($"{op.Cod}: nemăsurat, bugetul rulării ({buget.TotalMinutes:0} min) depășit");
                    oprire ??= $"bugetul rulării depășit la f = {f}, înaintea operației {op.Cod}";
                    continue;
                }
                var asteptat = scena.AsteptatLaScara(op.Cod, K, f);
                foreach (var m in Masoara(conexiune, punct with { Operatie = op.Cod, Planuri = true }, directorTreapta)) {
                    bool? ok = m.Eroare != null && m.Control.Count == 0 ? null
                        : m.Eroare == null && asteptat.Count == m.Control.Count && asteptat.All(a => m.Control.TryGetValue(a.Key, out var v) && v == a.Value);
                    t.Masurari.Add(new(op.Cod, op.Ruta, m, asteptat, ok));
                    if (ok == false)
                        Console.WriteLine($"     SCARA CONTROL f={f} {op.Cod} {m.Faza}: " + (m.Eroare ?? string.Join(", ", asteptat.Keys.Union(m.Control.Keys)
                            .Where(c => !asteptat.TryGetValue(c, out var a) || !m.Control.TryGetValue(c, out var v) || a != v)
                            .Select(c => $"{c} așteptat {(asteptat.TryGetValue(c, out var a) ? Nr(a) : "—")} citit {(m.Control.TryGetValue(c, out var v) ? Nr(v) : "—")}"))));
                    if (ok == null) Console.WriteLine($"     SCARA f={f} {op.Cod} {m.Faza}: {m.Eroare}");
                    else check($"SCARA f={f}: controlul numeric {op.Cod} {m.Faza} — cifrele cititorului sunt cele ale scenei, cu factorul cheii", ok.Value);
                }
                Scrie(director, rulare with { Oprire = "în lucru" });
            }
            facut = f;
            if (t.Masurari.Any(m => m.ControlOk == false)) oprire ??= $"cifră de control diferită la f = {f}; treptele următoare nu s-au făcut";
            if (oprire != null) break;
        }
        Scrie(director, rulare with { Oprire = oprire });
        Console.WriteLine($"     SCARA tabel: {Path.Combine(director, "scara-volum-privat.md")}; total {total.Elapsed.TotalMinutes:0.0} min"
            + (oprire == null ? "" : "; OPRITĂ: " + oprire));
    }

    /// <summary>Procesul-copil al primei reconstrucții de după multiplicare: drumul produsului, cronometrat fără construcția modelului.</summary>
    public static string ReconstruiesteInProces(string conexiune) {
        using var provider = new EFCoreObjectSpaceProvider<BackOfficeEFCoreDbContext>((b, _) => b.UseNpgsql(conexiune).UseChangeTrackingProxies()
            .UseObjectSpaceLinkProxies().UseLazyLoadingProxies().ConfigureLoggingCacheTime(TimeSpan.Zero));
        using var os = provider.CreateObjectSpace();
        _ = os.GetObjectsQuery<PerioadaFiscala>().Count();
        ReconstructieRezultatDto r = null;
        var ceas = Stopwatch.StartNew();
        var comenzi = CapturaSql.Masoara(() => r = PerioadeApply.Reconstruieste(os));
        return PrefixReconstructie + JsonSerializer.Serialize(new Reconstructie(ceas.Elapsed.TotalMilliseconds, comenzi.Count, r.Referinte.Length,
            r.Referinte.Sum(x => x.ContabilDiferite + x.StocDiferite + x.PartideDiferite), r.Referinte.Sum(x => x.ContabilRecalculate),
            r.Referinte.Sum(x => x.StocRecalculate), r.Referinte.Sum(x => x.PartideRecalculate)));
    }

    static (Reconstructie Rezultat, string Stare, bool Oprita) Reconstruieste(string conexiune) {
        var copil = Copil(conexiune, ["--scara-volum-reconstructie", "privat"], (trecut, _) => trecut > LimitaReconstructie);
        var json = copil.Linii.LastOrDefault(l => l.Text.StartsWith(PrefixReconstructie, StringComparison.Ordinal)).Text;
        if (json == null) return (null, copil.Oprit ? "peste 30 de minute, oprită" : $"proces căzut (exit {copil.Cod})", copil.Oprit);
        var r = JsonSerializer.Deserialize<Reconstructie>(json[PrefixReconstructie.Length..]);
        Console.WriteLine($"     SCARA reconstrucție: {r.Ms / 1000:0.0} s, {r.Comenzi} comenzi SQL, {r.Referinte} referințe, {r.Diferite} rânduri diferite; "
            + $"recalculate {r.Contabil} contabil / {r.Stoc} stoc / {r.Partide} partide");
        return (r, $"{r.Ms / 1000:0.0} s", false);
    }

    [GeneratedRegex(@" rece\): (\d+) ms, (\d+) comenzi / (\d+) ms SQL / (\d+) rânduri citite, (\d+) rânduri livrate, alocați ([\d.,]+) MiB, vârf gestionat ([\d.,]+) MiB")]
    private static partial Regex LinieRece();

    // Pin 13: o execuție de peste 10 minute se oprește, cu interogarea ei de pe server. Faza caldă poartă și cele două treceri de plan.
    static List<PerfCub.Masura> Masoara(string conexiune, PerfCub.Punct p, string director) {
        static (TimeSpan La, string Text) Rece(IReadOnlyList<(TimeSpan La, string Text)> linii) => linii.FirstOrDefault(l => l.Text.Contains(" rece): "));
        var copil = Copil(conexiune, ["--perf-cub-masura", PerfCub.Argument(p), director, "privat"], (trecut, linii) =>
            Rece(linii).Text == null ? trecut > LimitaMasurare : trecut - Rece(linii).La > LimitaMasurare * 3);
        var json = copil.Linii.LastOrDefault(l => l.Text.StartsWith(PerfCub.PrefixJson, StringComparison.Ordinal)).Text;
        if (json != null) return PerfCub.DinJson(json);
        var motiv = copil.Oprit ? "peste 10 minute, oprită" : $"proces căzut (exit {copil.Cod})";
        PerfCub.Masura Goala(string faza) => new(p.Istoric, p.Unitati, p.Operatie, faza, 0, 0, 0, 0, 0, 0, 0, 0, [], 0, 0, 0, 0, 0, motiv);
        if (LinieRece().Match(Rece(copil.Linii).Text ?? "") is not { Success: true } m) return [Goala("rece"), Goala("cald")];
        double Numar(int i) => double.Parse(m.Groups[i].Value.Replace(',', '.'), CultureInfo.InvariantCulture);
        return [new(p.Istoric, p.Unitati, p.Operatie, "rece", Numar(1), (int)Numar(2), Numar(3), (long)Numar(4), (long)(Numar(6) * 1048576),
            (long)(Numar(7) * 1048576), 0, (long)Numar(5), [], 0, 0, 0, 0, 0, "numai din jurnal: " + motiv + " în faza caldă"), Goala("cald")];
    }

    sealed record RezultatCopil(int Cod, bool Oprit, List<(TimeSpan La, string Text)> Linii);

    static RezultatCopil Copil(string conexiune, string[] argumente, Func<TimeSpan, IReadOnlyList<(TimeSpan La, string Text)>, bool> depasit) {
        var aplicatie = "scara-" + Guid.NewGuid().ToString("N")[..12];
        var psi = new ProcessStartInfo(Environment.ProcessPath!) { RedirectStandardOutput = true, UseShellExecute = false };
        foreach (var a in argumente.Prepend(typeof(ScaraVolum).Assembly.Location)) psi.ArgumentList.Add(a);
        psi.Environment["MODELCHECK_CONEXIUNE_EXTRA"] = (Env("MODELCHECK_CONEXIUNE_EXTRA") is { } extra ? extra + ";" : "") + "Application Name=" + aplicatie;
        using var copil = Process.Start(psi)!;
        var linii = new List<(TimeSpan La, string Text)>();
        var ceas = Stopwatch.StartNew();
        var cititor = Task.Run(() => {
            for (string linie; (linie = copil.StandardOutput.ReadLine()) != null;) {
                lock (linii) linii.Add((ceas.Elapsed, linie));
                if (linie.StartsWith("     ", StringComparison.Ordinal) || linie.StartsWith("FAIL", StringComparison.Ordinal)) Console.WriteLine(linie);
            }
        });
        var oprit = false;
        while (!copil.WaitForExit(500)) {
            List<(TimeSpan La, string Text)> copie;
            lock (linii) copie = [.. linii];
            if (!depasit(ceas.Elapsed, copie)) continue;
            oprit = true;
            using (var c = new NpgsqlConnection(conexiune)) {
                c.Open();
                Executa(c, "select pg_terminate_backend(pid) from pg_stat_activity where application_name = @aplicatie", ("aplicatie", aplicatie));
            }
            copil.Kill(true);
            break;
        }
        copil.WaitForExit();
        cititor.Wait();
        lock (linii) return new(copil.ExitCode, oprit, [.. linii]);
    }
}
