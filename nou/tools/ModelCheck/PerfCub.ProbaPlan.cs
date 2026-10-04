using DevExpress.ExpressApp;

namespace Atlas.Conta.BackOffice.ModelCheck;

sealed partial class PerfCub {
    // X-RI3: un EXPLAIN respins invalidează măsurarea; un zero legitim de accesări rămâne probă.
    public static void ProbaPlanRespins(IObjectSpace os, bool privat, Action<string, bool> check) {
        var director = Path.Combine(Path.GetTempPath(), "perf-cub-proba-plan-" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(director);
        try {
            StatisticaPlan Plan(string sql) => Planuri(os, [new CapturaSql.Comanda(sql, [])], Path.Combine(director, "plan.txt"), true);
            var respins = Plan("SELECT 1 / 0 FROM \"Postare\" RIGHT JOIN (VALUES (1)) v(x) ON true");
            var legitim = Plan("SELECT count(*) FROM \"Postare\" WHERE \"ID\" = '00000000-0000-0000-0000-000000000000'");
            var absent = new StatisticaPlan(0, 0, 0, 0, 0, 0, 0, 0);
            check("X-D5-PLAN: planul respins e numărat, cel obținut pe zero rânduri nu ("
                + $"respins {respins.Respinse}/{respins.Cerute}, legitim {legitim.Respinse}/{legitim.Cerute}, {legitim.Randuri:0} rânduri)",
                respins is { Cerute: 1, Respinse: 1 } && legitim is { Cerute: 1, Respinse: 0, Randuri: 0 });

            const string Criteriu = "accesul la `Postare` nu depinde de m";
            (bool Cazuta, bool Plan) Evaluat(StatisticaPlan plan) {
                var scene = new List<PerfCub>();
                foreach (var m in new[] { 0, 6, 12 }) {
                    var scena = new PerfCub(() => os, (_, _) => { }, privat, (_, _, _) => { }, 2011, m, 16, [1, 4], null, director, null);
                    foreach (var k in new[] { 1, 4 })
                    foreach (var faza in new[] { "rece", "cald" })
                        scena.Masuri.Add(CuPlanuri(new Masura(m, k, "BAL-NI", faza, 1, 1, 1, 1, 1, 1, 1, 1, [], 0, 0, 0, 0, 0, null), plan, plan));
                    scene.Add(scena);
                }
                var verdicte = new List<(string Text, bool Ok)>();
                var iesire = Console.Out;
                Console.SetOut(TextWriter.Null);
                try { Evalueaza(scene, (text, ok) => verdicte.Add((text, ok)), director, privat); }
                finally { Console.SetOut(iesire); }
                return (verdicte.Single(v => v.Text.Contains("nicio măsurare căzută")).Ok,
                    verdicte.Single(v => v.Text.Contains(Criteriu)).Ok);
            }
            check("X-D5-PLAN: evaluatorul refuză planul respins și planul absent, și acceptă zeroul legitim",
                Evaluat(respins) == (false, false) && Evaluat(absent) == (true, false) && Evaluat(legitim) == (true, true));
        }
        finally { Directory.Delete(director, true); }
    }
}
