using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Cub.Citiri;
using Atlas.Conta.BackOffice.Module.Proiectii;
using DevExpress.ExpressApp;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Atlas.Conta.BackOffice.ModelCheck;

static partial class ScaraVolum {
    public static void ReferintaD9b(string conexiune, Func<IObjectSpace> deschide, Action<string, bool> check,
            Action<IObjectSpace, int, int> inchide, DbContextOptions<BackOfficeEFCoreDbContext> optiuni, string director) {
        using var sql = new NpgsqlConnection(conexiune); sql.Open();
        if (!sql.Database.EndsWith(".CodexD9bA", StringComparison.Ordinal)
            || Scalar<long>(sql, "select count(*) from \"Postare\"") != 0
            || Scalar<bool>(sql, "select exists (select 1 from pg_namespace where nspname = 'scara')"))
            throw new InvalidOperationException("Referința cere baza dedicată .CodexD9bA, fără postări și fără schema scara.");
        PerfCub.Punct punct = null;
        var scena = new PerfCub(deschide, check, true, inchide, 2064, Istoric, UnitatiIstoric, [K],
            p => { punct = p; return []; }, director, optiuni) { Pastrata = true };
        scena.Ruleaza();
        if (punct == null) throw new InvalidOperationException("Scena nu a produs punctul de măsurare.");
        File.WriteAllText(Path.Combine(director, "punct.json"), JsonSerializer.Serialize(punct));
        Executa(sql, Sectiuni.Value["fotografie"]);
        var n = Scalar<long>(sql, "select count(*) from scara.postare");
        var maxim = (int)((5000000 + n - 1) / n);
        var facut = 1;
        foreach (var f in new[] { 1, 10, 100, maxim }.Distinct().Order()) {
            for (var c = facut; c < f; c += CopiiPeTransa) {
                using var tx = sql.BeginTransaction();
                Executa(sql, Sectiuni.Value["copiaza"], ("de_la", c), ("pana_la", Math.Min(f, c + CopiiPeTransa)));
                tx.Commit();
            }
            foreach (var t in TabeleAtinse) Executa(sql, $"VACUUM (ANALYZE) \"{t}\"");
            var probe = Probe(sql, "probe", f);
            if (f > 1) probe.AddRange(Probe(sql, "probe_copie", 1, ("c", f - 1)));
            foreach (var g in probe.GroupBy(x => x.Nume)) check($"D9B-A f{f}: {g.Key}", g.All(x => x.Ok));
            if (probe.Any(x => !x.Ok)) throw new InvalidOperationException("Copierea nu conservă scena.");
            var dir = Path.Combine(director, $"d9b-f{f}"); Directory.CreateDirectory(dir);
            File.WriteAllText(Path.Combine(dir, "manifest.json"), JsonSerializer.Serialize(new {
                Baza = sql.Database, Factor = f, Postari = f * n, Probe = probe,
                Configuratie = Perechi<string>(sql, "configuratie"),
                Schema = Scalar<string>(sql, "select string_agg(\"MigrationId\", ',' order by \"MigrationId\") from \"__EFMigrationsHistory\""),
                FapteMd5 = Scalar<string>(sql, "select md5(string_agg(md5(row_to_json(p)::text), '' order by p.\"ID\")) from \"Postare\" p"),
                BazaOcteti = Scalar<long>(sql, "select pg_database_size(current_database())"),
                DllSha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(typeof(ScaraVolum).Assembly.Location)))
            }));
            var masuri = new List<PerfCub.Masura>();
            foreach (var securizat in new[] { false, true })
            foreach (var lectura in new[] { "ORIGINI", "SOLDURI", "REST" })
            foreach (var pagina in new[] { false, true }) {
                var cod = $"D9B-{lectura}-{(pagina ? "P50" : "TOT")}-{(securizat ? "S" : "N")}";
                var op = new PerfCub.Operatie(cod, cod, PerfCub.Recitire, securizat, true, m => {
                    PerfCub.Citit Citeste<T>(IQueryable<T> q) {
                        var r = (pagina ? q.Take(50) : q).ToArray();
                        var control = new Dictionary<string, decimal> { ["randuri"] = r.Length };
                        m.Dupa = d => {
                            var hash = SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(r));
                            for (var i = 0; i < 4; i++) d[$"sha256-{i}"] = BitConverter.ToUInt64(hash, i * 8);
                        };
                        return new(r.Length, control);
                    }
                    return lectura switch {
                        "ORIGINI" => Citeste(Partide.Origini(m.Os).OrderBy(x => x.UnitateId)),
                        "SOLDURI" => Citeste(Partide.Cumulate(m.Os, CitireCumul.Vizibila, m.End).OrderBy(x => x.UnitateId)),
                        _ => Citeste(ImperecheriProiectii.PartideCuRest(m.Os, laData: m.End).OrderBy(x => x.UnitateId))
                    };
                });
                var citiri = PerfCub.MasoaraInProces(conexiune, punct with { Operatie = cod, Planuri = true }, dir, op, 5,
                    comenzi => File.WriteAllText(Path.Combine(dir, cod + "-sql.json"), JsonSerializer.Serialize(comenzi.Select(c => new {
                        Sql = c.Text, Parametri = c.Parametri.Select(p => new { p.ParameterName, Tip = p.DbType.ToString(), Valoare = p.Value })
                    }))));
                masuri.AddRange(citiri);
                var martor = JsonSerializer.Serialize(citiri[0].Control);
                check($"D9B-A f{f} {cod}: șase rezultate identice, planuri obținute", citiri.Count == 6
                    && citiri.All(m => m.Eroare == null && JsonSerializer.Serialize(m.Control) == martor)
                    && citiri[^1].PlanuriCerute > 0 && citiri[^1].PlanuriRespinse == 0);
                File.WriteAllText(Path.Combine(dir, "masuri.json"), JsonSerializer.Serialize(masuri));
                if (citiri.Any(m => m.Eroare != null)) throw new InvalidOperationException($"Măsurare invalidă: {cod}");
            }
            facut = f;
        }
        new ScenaD9bScriere(deschide, check, director).Ruleaza();
    }
}
