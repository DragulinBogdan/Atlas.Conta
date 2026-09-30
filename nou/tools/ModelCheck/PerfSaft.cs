using System.Diagnostics;
using System.Text;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Saft;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.EFCore;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Atlas.Conta.BackOffice.ModelCheck;

// SAF-B8 D3: scara sintetică a exporturilor L și S — k unități în luna măsurată, m luni închise de istoric.
sealed class PerfSaft(Func<IObjectSpace> deschide, Action<string, bool> check, Action<IObjectSpace, int, int> inchide,
        int an, int istoric, int unitatiIstoric, int[] trepte, string director, bool planuri)
    : ScenaDocumente(deschide, check, true, inchide, "PERF" + istoric, an) {

    public sealed record Masura(int Istoric, int Unitati, string Modul, string Faza, double Ms, int Comenzi, double MsSql,
        long Randuri, long Alocati, long VarfGestionat, long OctetiXml, double MsXml, double MsDuk,
        int Tranzactii, int Facturi, int Plati, int Miscari, int Pozitii, int Refuzuri, long RanduriPostare, long RanduriSnapshot, double MsServerMax = 0);

    static string Tabela(CapturaSql.Comanda c) => CapturaSql.Tabele([c.Text]).FirstOrDefault() ?? "";

    public List<Masura> Masuri { get; } = [];
    protected override int UltimulAn => An + 1;

    protected override void Executa() {
        var inainte = CuSpatiu(os => os.GetObjectsQuery<Societate>().Select(x => new {
            x.CodFiscal, x.InregistratTva, x.Tara, x.Denumire, x.ContactNume, x.ContactPrenume, x.Telefon, x.ContBancarId,
        }).First());
        var ibanInainte = CuSpatiu(os => os.GetObjectsQuery<ContPropriu>().Single(c => c.Cod == "BANCA").Iban);
        Comanda(os => {
            var soc = os.GetObjectsQuery<Societate>().First();
            var banca = os.GetObjectsQuery<ContPropriu>().Single(c => c.Cod == "BANCA");
            banca.Iban = "RO49AAAA1B31007593840000";
            soc.CodFiscal = "12345674"; soc.InregistratTva = true; soc.Tara = "RO";
            soc.Denumire = "Atlas Probă SAF-T SRL"; soc.ContactNume = "Popescu"; soc.ContactPrenume = "Ion";
            soc.Telefon = "0264000000"; soc.ContBancarId = banca.ID;
            foreach (var (a, l) in Enumerable.Range(3, 10).Select(l => (An, l)).Concat(Enumerable.Range(1, 12).Select(l => (An + 1, l)))) {
                var p = os.CreateObject<PerioadaFiscala>(); p.An = a; p.Luna = l;
            }
            os.CommitChanges();
        });
        try { Scena(); }
        finally {
            Comanda(os => {
                var soc = os.GetObjectsQuery<Societate>().First();
                soc.CodFiscal = inainte.CodFiscal; soc.InregistratTva = inainte.InregistratTva; soc.Tara = inainte.Tara;
                soc.Denumire = inainte.Denumire; soc.ContactNume = inainte.ContactNume; soc.ContactPrenume = inainte.ContactPrenume;
                soc.Telefon = inainte.Telefon; soc.ContBancarId = inainte.ContBancarId;
                os.GetObjectsQuery<ContPropriu>().Single(c => c.Cod == "BANCA").Iban = ibanInainte;
                os.CommitChanges();
            });
        }
    }

    void Scena() {
        var ceas = Stopwatch.StartNew();
        var (anM, lunaM) = istoric == 12 ? (An + 1, 1) : (An, istoric + 1);
        // Lotul „lung”, consumat în fiecare lună: istoricul lui crește cu m, poziția rămâne una.
        var inceput = istoric > 0 ? new DateOnly(An, 1, 1) : new DateOnly(anM, lunaM, 1);
        var lung = Factura(inceput, new LinieFctScena(1000, 1, "N21"));
        Dateaza(lung.Id, inceput);
        Opereaza(Opereaza(lung.Id).ConexId.Value);
        void ConsumLung(DateOnly d) { var b = Consum(lung.Linii[0].Lot.Value, 1); Dateaza(b, d); Opereaza(b); }
        for (var luna = 1; luna <= istoric; luna++) {
            for (var u = 0; u < unitatiIstoric; u++) Unitate(new DateOnly(An, luna, 1 + u % 27));
            ConsumLung(new DateOnly(An, luna, 28));
            Inchide(An, luna);
        }
        ConsumLung(new DateOnly(anM, lunaM, 28));
        Console.WriteLine($"     PERF istoric {istoric} luni × {unitatiIstoric} unități: {ceas.Elapsed.TotalSeconds:0} s");
        var facute = 0;
        foreach (var k in trepte) {
            for (; facute < k; facute++) Unitate(new DateOnly(anM, lunaM, 1 + facute % 27));
            foreach (var modul in new[] { "L", "S" })
                foreach (var faza in new[] { "rece", "cald" })
                    Masuri.Add(Masoara(anM, lunaM, k, modul, faza, planuri && k == trepte[^1] && faza == "cald"));
        }
    }

    void Dateaza(Guid doc, DateOnly d) => Comanda(os => {
        var x = os.GetObjectByKey<Document>(doc); x.Data = d; x.DataInregistrare = d; os.CommitChanges();
    });

    Guid Fcl(DateOnly d, decimal baza) => CuSpatiu(os => {
        var x = os.CreateObject<FacturaIesire>(); x.Data = d; x.DataInregistrare = d; x.PredatorId = Loc; x.PrimitorId = Client;
        var l = os.CreateObject<FacturaIesireDetaliu>(); l.Document = x; l.Pozitie = 1;
        l.TipMaterialId = Tip(os, "704"); l.Cantitate = 1; l.PretUnitar = baza;
        l.TipTvaId = os.GetObjectsQuery<TipTva>().Single(t => t.Cod == "N21").ID;
        os.CommitChanges(); return x.ID;
    });

    // O unitate: FCT (stoc + serviciu) cu NIR conex, PLT parțial legată, BCS, BTR, DSC, FCL cu INC legată.
    void Unitate(DateOnly d) {
        var f = Factura(d, new LinieFctScena(10, 10, "N21"), new LinieFctScena(1, 50, "N21", Stoc: false));
        Dateaza(f.Id, d);
        var nir = Opereaza(f.Id).ConexId.Value;
        Opereaza(nir);
        var lot = f.Linii[0];
        var plt = Trezorerie(false, 70).Id; Dateaza(plt, d); Opereaza(plt);
        Imperecheaza(plt, f.Id, 50, d);
        var bcs = Consum(lot.Lot.Value, 2); Dateaza(bcs, d); Opereaza(bcs);
        var btr = Iesire(true, (lot, 1)).Id; Dateaza(btr, d); Opereaza(btr);
        var dsc = Iesire(false, (lot, 1)).Id; Dateaza(dsc, d); Opereaza(dsc);
        var fcl = Fcl(d, 100); Opereaza(fcl);
        var inc = Trezorerie(true, 60).Id; Dateaza(inc, d); Opereaza(inc);
        Imperecheaza(inc, fcl, 60, d);
    }

    Masura Masoara(int anM, int lunaM, int k, string modul, string faza, bool cuPlanuri) {
        if (faza == "rece") { NpgsqlConnection.ClearAllPools(); GC.Collect(); GC.WaitForPendingFinalizers(); }
        using var os = Deschide();
        var alocatInainte = GC.GetTotalAllocatedBytes(true);
        long varf = 0;
        using var opreste = new CancellationTokenSource();
        var esantionare = Task.Run(() => {
            while (!opreste.IsCancellationRequested) {
                varf = Math.Max(varf, GC.GetTotalMemory(false));
                Thread.Sleep(2);
            }
        });
        SaftDto dto = null;
        var ceas = Stopwatch.StartNew();
        var comenzi = CapturaSql.Masoara(() => dto = modul == "L"
            ? SaftProiectii.SaftPeCub(os, anM, lunaM, new DateOnly(anM, lunaM, 28))
            : SaftProiectii.SaftStocuriPeCub(os, anM, lunaM, new DateOnly(anM, lunaM, 28)));
        var ms = ceas.Elapsed.TotalMilliseconds;
        foreach (var r in dto.Refuzuri) Console.WriteLine($"     PERF REFUZ {r.Cod}: {r.Mesaj}");
        var cale = Path.Combine(director, $"perf-{modul}-m{istoric}-k{k}-{faza}.xml");
        ceas.Restart();
        if (dto.Refuzuri.Count == 0) using (var fs = File.Create(cale)) SaftXml.Scrie(dto, fs);
        var msXml = ceas.Elapsed.TotalMilliseconds;
        opreste.Cancel(); esantionare.Wait();
        var alocati = GC.GetTotalAllocatedBytes(true) - alocatInainte;
        double msDuk = 0, serverMax = 0;
        if (cuPlanuri && dto.Refuzuri.Count == 0) {
            ceas.Restart();
            var duk = Duk.Valideaza(cale);
            msDuk = ceas.Elapsed.TotalMilliseconds;
            Console.WriteLine($"     PERF DUK {modul} m{istoric} k{k}: {(duk.Valid ? "ok" : "RESPINS")} în {msDuk:0} ms");
            Verifica("SAF-B8-D3", $"XML {modul} la m={istoric}, k={k} validat de DUK", duk.Valid && duk.Avertismente.Count == 0);
            serverMax = Planuri(os, comenzi, $"perf-planuri-{modul}-m{istoric}-k{k}.txt");
        }
        var m = new Masura(istoric, k, modul, faza, ms, comenzi.Count, comenzi.Sum(c => c.Durata.TotalMilliseconds),
            comenzi.Sum(c => c.Randuri), alocati, varf, File.Exists(cale) ? new FileInfo(cale).Length : 0, msXml, msDuk,
            dto.Jurnale.Sum(j => j.Tranzactii.Count), dto.FacturiEmise.Count + dto.FacturiPrimite.Count, dto.Plati.Count,
            dto.MiscariStoc.Count, dto.StocFizic.Count, dto.Refuzuri.Count,
            comenzi.Where(c => Tabela(c) == "Postare").Sum(c => c.Randuri),
            comenzi.Where(c => Tabela(c).StartsWith("SolduriPerioada")).Sum(c => c.Randuri), serverMax);
        Console.WriteLine($"     MĂSURAT (perf {modul} m{istoric} k{k} {faza}): {m.Ms:0} ms, {m.Comenzi} comenzi / {m.MsSql:0} ms SQL / "
            + $"{m.Randuri} rânduri, alocați {m.Alocati / 1048576.0:0.0} MiB, vârf gestionat {m.VarfGestionat / 1048576.0:0.0} MiB, "
            + $"XML {m.OctetiXml / 1024.0:0} KiB în {m.MsXml:0} ms; {m.Tranzactii} tranzacții, {m.Facturi} facturi, {m.Plati} plăți, "
            + $"{m.Miscari} mișcări, {m.Pozitii} poziții; rânduri din Postare {m.RanduriPostare}, din snapshot {m.RanduriSnapshot}");
        return m;
    }

    // Criteriile B8-D3 peste toate punctele (k, m); tabelul intră în p5-perf-masuratori.md.
    public static void Evalueaza(List<Masura> toate, Action<string, bool> check, string director) {
        var cald = toate.Where(m => m.Faza == "cald").ToList();
        foreach (var modul in new[] { "L", "S" }) {
            var puncte = cald.Where(m => m.Modul == modul).ToList();
            check($"SAF-B8-D3 ({modul}): numărul de comenzi SQL nu depinde de k și m "
                + $"([{string.Join(", ", puncte.Select(p => $"m{p.Istoric}k{p.Unitati}:{p.Comenzi}"))}])",
                puncte.Max(p => p.Comenzi) - puncte.Min(p => p.Comenzi) <= 2);
            var scara = puncte.Where(p => p.Istoric == 0).OrderBy(p => p.Unitati).ToList();
            var perechi = scara.Zip(scara.Skip(1)).ToList();
            bool Liniar(Func<Masura, double> f, double toleranta) =>
                perechi.All(x => f(x.Second) <= toleranta * f(x.First) * x.Second.Unitati / x.First.Unitati);
            check($"SAF-B8-D3 ({modul}): timpul client (fără SQL) crește cel mult liniar în k (toleranță 25% pe pas), "
                + "alocările cel mult n log n (toleranță 50% pe pas) "
                + $"([{string.Join(", ", scara.Select(p => $"k{p.Unitati}:{p.Ms - p.MsSql:0} ms/{p.Alocati / 1048576.0:0.0} MiB"))}])",
                perechi.Count > 0 && Liniar(p => p.Ms - p.MsSql, 1.25) && Liniar(p => p.Alocati, 1.5));
            var maxim = scara.Count > 0 ? scara.Where(p => p.MsServerMax > 0).DefaultIfEmpty(scara[^1]).Last() : null;
            check($"SAF-B8-D3 ({modul}): la k maxim nicio interogare nu depășește 100 ms pe server (EXPLAIN ANALYZE; "
                + $"maxim {maxim?.MsServerMax:0.0} ms); pragul de transport al tablourilor mari se raportează separat",
                maxim is { MsServerMax: > 0 and <= 100 });
            check($"SAF-B8-D3 ({modul}): exportul complet la k maxim nu are refuzuri", scara.Count > 0 && scara[^1].Refuzuri == 0);
            var istoric = puncte.Where(p => p.Unitati == puncte.Where(q => q.Istoric > 0).Select(q => q.Unitati).DefaultIfEmpty(-1).First())
                .OrderBy(p => p.Istoric).ToList();
            if (modul == "L" && istoric.Count > 1)
                check("SAF-B8-D3 (L): rândurile citite din `Postare` nu cresc cu m (toleranță 25%) "
                    + $"([{string.Join(", ", istoric.Select(p => $"m{p.Istoric}: Postare {p.RanduriPostare}, snapshot {p.RanduriSnapshot}"))}])",
                    istoric.Skip(1).All(p => p.RanduriPostare <= 1.25 * istoric[0].RanduriPostare));
            if (modul == "S" && istoric.Count > 1) {
                var baza = istoric[0];
                check("SAF-B8-D3 (S): Opening citește snapshot-ul — rândurile citite din `Postare` nu cresc cu m (toleranță 25%), "
                    + "iar cele din snapshot urmează pozițiile deschise "
                    + $"([{string.Join(", ", istoric.Select(p => $"m{p.Istoric}: Postare {p.RanduriPostare}, snapshot {p.RanduriSnapshot}, poziții {p.Pozitii}"))}])",
                    istoric.Skip(1).All(p => p.RanduriPostare <= 1.25 * baza.RanduriPostare && p.RanduriSnapshot <= 2 * p.Pozitii));
            }
        }
        var sb = new StringBuilder("| m | k | modul | faza | ms | comenzi | ms SQL | rânduri | alocați MiB | vârf MiB | XML KiB | ms XML | ms DUK | tranzacții | facturi | plăți | mișcări | poziții | rânduri Postare | rânduri snapshot |\n"
            + "|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|\n");
        foreach (var m in toate.OrderBy(m => m.Istoric).ThenBy(m => m.Unitati).ThenBy(m => m.Modul).ThenByDescending(m => m.Faza))
            sb.Append($"| {m.Istoric} | {m.Unitati} | {m.Modul} | {m.Faza} | {m.Ms:0} | {m.Comenzi} | {m.MsSql:0} | {m.Randuri} | "
                + $"{m.Alocati / 1048576.0:0.0} | {m.VarfGestionat / 1048576.0:0.0} | {m.OctetiXml / 1024.0:0} | {m.MsXml:0} | {m.MsDuk:0} | "
                + $"{m.Tranzactii} | {m.Facturi} | {m.Plati} | {m.Miscari} | {m.Pozitii} | {m.RanduriPostare} | {m.RanduriSnapshot} |\n");
        File.WriteAllText(Path.Combine(director, "perf-saft.md"), sb.ToString());
        Console.WriteLine($"     PERF tabel: {Path.Combine(director, "perf-saft.md")}");
    }

    // EXPLAIN (ANALYZE, BUFFERS) pe fiecare comandă a exportului, cu parametrii ei; cele mai scumpe primele.
    double Planuri(IObjectSpace os, List<CapturaSql.Comanda> comenzi, string fisier) {
        var ctx = ((EFCoreObjectSpace)os).DbContext;
        var conexiune = ctx.Database.GetDbConnection();
        if (conexiune.State != System.Data.ConnectionState.Open) conexiune.Open();
        var planuri = new List<(double Ms, string Text)>();
        double serverMaxim = 0;
        foreach (var c in comenzi.Where(c => c.Text.TrimStart().StartsWith("SELECT", StringComparison.OrdinalIgnoreCase)
                     || c.Text.TrimStart().StartsWith("WITH", StringComparison.OrdinalIgnoreCase))) {
            using var cmd = conexiune.CreateCommand();
            cmd.CommandText = "EXPLAIN (ANALYZE, BUFFERS) " + c.Text;
            foreach (var p in c.Parametri) cmd.Parameters.Add(p is ICloneable cl ? (System.Data.Common.DbParameter)cl.Clone() : p);
            var text = new StringBuilder();
            using (var r = cmd.ExecuteReader()) while (r.Read()) text.AppendLine(r.GetString(0));
            var plan = text.ToString();
            var linie = plan.Split('\n').FirstOrDefault(l => l.StartsWith("Execution Time:"));
            var ms = linie == null ? 0 : double.Parse(linie.Split(':')[1].Replace("ms", "").Trim(), System.Globalization.CultureInfo.InvariantCulture);
            serverMaxim = Math.Max(serverMaxim, ms);
            planuri.Add((c.Durata.TotalMilliseconds, $"-- în export {c.Durata.TotalMilliseconds:0.0} ms, EXPLAIN {ms:0.000} ms, {c.Randuri} rânduri\n{c.Text}\n{plan}"));
        }
        File.WriteAllText(Path.Combine(director, fisier), string.Join("\n\n", planuri.OrderByDescending(p => p.Ms).Select(p => p.Text)));
        Console.WriteLine($"     MĂSURAT (perf planuri {fisier}): {planuri.Count} interogări, cele mai scumpe în export "
            + string.Join(", ", planuri.OrderByDescending(p => p.Ms).Take(3).Select(p => $"{p.Ms:0.0} ms"))
            + $"; execuția maximă pe server {serverMaxim:0.0} ms");
        return serverMaxim;
    }
}
