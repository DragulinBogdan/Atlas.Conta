using System.Security.Cryptography;
using System.Text.Json;
using System.Text.RegularExpressions;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Cub.Citiri;
using Atlas.Conta.BackOffice.Module.Proiectii;
using DevExpress.ExpressApp;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Atlas.Conta.BackOffice.ModelCheck;

static partial class ScaraVolum {
    public static void ComparatieD9b(string conexiune, Func<IObjectSpace> deschide, Action<string, bool> check,
            Action<IObjectSpace, int, int> inchide, DbContextOptions<BackOfficeEFCoreDbContext> optiuni, string director) {
        using var sql = new NpgsqlConnection(conexiune); sql.Open();
        if (sql.Database != "Atlas.Conta.ModelCheck.Privat.CodexD9bB"
            || Scalar<long>(sql, "select count(*) from \"Postare\"") != 0
            || Scalar<bool>(sql, "select exists (select 1 from pg_namespace where nspname = 'scara')"))
            throw new InvalidOperationException("Comparația cere baza nouă .CodexD9bB fără postări sau schema scara.");
        PerfCub.Punct punct = null;
        new PerfCub(deschide, check, true, inchide, 2064, Istoric, UnitatiIstoric, [K],
            p => { punct = p; return []; }, director, optiuni) { Pastrata = true }.Ruleaza();
        if (punct == null) throw new InvalidOperationException("Scena nu a produs punctul de măsurare.");
        File.WriteAllText(Path.Combine(director, "punct.json"), JsonSerializer.Serialize(punct));
        Executa(sql, Sectiuni.Value["fotografie"]);
        PregatesteOriginiD9b(sql);
        var n = Scalar<long>(sql, "select count(*) from scara.postare");
        var maxim = (int)((5000000 + n - 1) / n);
        var facut = 1;
        var comparatii = new List<object>();
        foreach (var f in new[] { 1, 10, 100, maxim }.Distinct().Order()) {
            for (var c = facut; c < f; c += CopiiPeTransa) {
                using var tx = sql.BeginTransaction();
                CopiazaD9b(sql, c, Math.Min(f, c + CopiiPeTransa));
                tx.Commit();
            }
            foreach (var t in TabeleAtinse.Append("OriginiPartide")) Executa(sql, $"VACUUM (ANALYZE) \"{t}\"");
            var probe = Probe(sql, "probe", f);
            if (f > 1) probe.AddRange(Probe(sql, "probe_copie", 1, ("c", f - 1)));
            foreach (var g in probe.GroupBy(x => x.Nume)) check($"D9B-AB f{f}: {g.Key}", g.All(x => x.Ok));
            if (probe.Any(x => !x.Ok)) throw new InvalidOperationException("Copierea nu conservă scena.");
            using (var os = deschide()) OriginiPartide.Verifica(os);
            var dir = Path.Combine(director, $"d9b-f{f}"); Directory.CreateDirectory(dir);
            var amprenta = AmprentaD9b(sql);
            File.WriteAllText(Path.Combine(dir, "manifest.json"), JsonSerializer.Serialize(new {
                Baza = sql.Database, Factor = f, Postari = f * n, Probe = probe, FapteMd5 = amprenta,
                Configuratie = Perechi<string>(sql, "configuratie"),
                Schema = Scalar<string>(sql, "select string_agg(\"MigrationId\", ',' order by \"MigrationId\") from \"__EFMigrationsHistory\""),
                Origini = Scalar<long>(sql, "select count(*) from \"OriginiPartide\""),
                OriginiOcteti = Scalar<long>(sql, "select pg_table_size('\"OriginiPartide\"')"),
                IndecsiOriginiOcteti = Scalar<long>(sql, "select pg_indexes_size('\"OriginiPartide\"')"),
                BazaOcteti = Scalar<long>(sql, "select pg_database_size(current_database())"),
                DllSha256 = HashFisier(typeof(ScaraVolum).Assembly.Location),
                ModuleSha256 = HashFisier(typeof(Partide).Assembly.Location)
            }));
            var masuri = new List<PerfCub.Masura>();
            foreach (var securizat in new[] { false, true })
            foreach (var lectura in new[] { "ORIGINI", "SOLDURI", "REST" })
            foreach (var pagina in new[] { false, true }) {
                var cod = $"D9B-{lectura}-{(pagina ? "P50" : "TOT")}-{(securizat ? "S" : "N")}";
                var pereche = new Dictionary<string, List<PerfCub.Masura>>();
                foreach (var varianta in ((f + (pagina ? 1 : 0)) % 2 == 0 ? new[] { "B", "A" } : new[] { "A", "B" })) {
                    var nume = varianta + "-" + cod;
                    var op = OperatieD9b(nume, lectura, pagina, securizat, varianta == "A");
                    var citiri = PerfCub.MasoaraInProces(conexiune, punct with { Operatie = nume, Planuri = true }, dir, op, 5,
                        comenzi => SalveazaSqlD9b(dir, cod, varianta, comenzi), sistemHost: true);
                    pereche.Add(varianta, citiri); masuri.AddRange(citiri);
                    var martor = JsonSerializer.Serialize(citiri[0].Control);
                    var valid = citiri.Count == 6 && citiri.All(m => m.Eroare == null && JsonSerializer.Serialize(m.Control) == martor)
                        && citiri[^1].PlanuriCerute > 0 && citiri[^1].PlanuriRespinse == 0;
                    check($"D9B-AB f{f} {nume}: șase rezultate identice și planuri valide", valid);
                    File.WriteAllText(Path.Combine(dir, "masuri.json"), JsonSerializer.Serialize(masuri));
                    if (!valid) throw new InvalidOperationException($"Măsurare invalidă: {nume}");
                }
                var egale = JsonSerializer.Serialize(pereche["A"][0].Control) == JsonSerializer.Serialize(pereche["B"][0].Control);
                check($"D9B-AB f{f} {cod}: rezultate A = B", egale);
                var a = pereche["A"].Skip(1).Select(m => m.Ms).Order().ElementAt(2);
                var b = pereche["B"].Skip(1).Select(m => m.Ms).Order().ElementAt(2);
                var prag = f == maxim && lectura == "REST" && !pagina && !securizat ? .8 * a : Math.Max(1.1 * a, a + 10);
                check($"D9B-AB f{f} {cod}: B {b:0.00} ms <= prag {prag:0.00} ms (A {a:0.00})", b <= prag);
                if (lectura == "ORIGINI" && !securizat)
                    check($"D9B-AB f{f} {cod}: B fără scanare Postare", pereche["B"][^1].ScanariPostare == 0);
                comparatii.Add(new { Factor = f, Cod = cod, A = a, B = b, Prag = prag, Egal = egale, Acceptat = b <= prag });
                File.WriteAllText(Path.Combine(director, "comparatie.json"), JsonSerializer.Serialize(comparatii));
            }
            check($"D9B-AB f{f}: fapte neschimbate după citiri", amprenta == AmprentaD9b(sql));
            facut = f;
        }
        new ScenaD9bScriere(deschide, check, director).Ruleaza();
    }

    static string HashFisier(string cale) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(cale)));
    static string AmprentaD9b(NpgsqlConnection sql) => Scalar<string>(sql,
        "select md5(string_agg(md5(row_to_json(p)::text), '' order by p.\"ID\")) from \"Postare\" p");

    static PerfCub.Operatie OperatieD9b(string cod, string lectura, bool pagina, bool securizat, bool a) =>
        new(cod, cod, PerfCub.Recitire, securizat, true, m => {
            PerfCub.Citit Citeste<T>(IQueryable<T> q) {
                var r = (pagina ? q.Take(50) : q).ToArray();
                m.Dupa = d => {
                    var hash = SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(r));
                    for (var i = 0; i < 4; i++) d[$"sha256-{i}"] = BitConverter.ToUInt64(hash, i * 8);
                };
                return new(r.Length, new() { ["randuri"] = r.Length });
            }
            return lectura switch {
                "ORIGINI" => Citeste((a ? D9bCititorA.Origini(m.Os) : Partide.Origini(m.Os)).OrderBy(x => x.UnitateId)),
                "SOLDURI" => Citeste(Partide.Cumulate(m.Os, CitireCumul.Vizibila, m.End).OrderBy(x => x.UnitateId)),
                _ => Citeste((a ? D9bCititorA.PartideCuRest(m.Os, m.End)
                    : ImperecheriProiectii.PartideCuRest(m.Os, laData: m.End)).OrderBy(x => x.UnitateId))
            };
        });

    static void SalveazaSqlD9b(string dir, string cod, string varianta, List<CapturaSql.Comanda> comenzi) {
        File.WriteAllText(Path.Combine(dir, varianta + "-" + cod + "-sql.json"), JsonSerializer.Serialize(comenzi.Select(c => new {
            Sql = c.Text, Parametri = c.Parametri.Select(p => new { p.ParameterName, Tip = p.DbType.ToString(), Valoare = p.Value })
        })));
        if (varianta != "A") return;
        using var captura = JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Referinte", "D9bA", cod + "-sql.json")));
        var sql = captura.RootElement.EnumerateArray().Last().GetProperty("Sql").GetString();
        static string Normal(string text) => Regex.Replace(text, @"\s+", " ").Trim();
        if (Normal(sql) != Normal(comenzi[^1].Text)) throw new InvalidOperationException("SQL A diferit de captura înghețată: " + cod);
    }
}
