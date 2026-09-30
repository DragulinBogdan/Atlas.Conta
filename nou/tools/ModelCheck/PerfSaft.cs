using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Saft;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.EFCore;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Conta.BackOffice.ModelCheck;

// SAF-B8 D3: scara sintetică a exporturilor L și S — k unități în luna măsurată, m luni închise de istoric;
// fiecare punct (k, m, modul) se măsoară într-un proces nou: prima rulare e „rece”, a doua „cald”.
sealed class PerfSaft(Func<IObjectSpace> deschide, Action<string, bool> check, Action<IObjectSpace, int, int> inchide,
        int an, int istoric, int unitatiIstoric, int[] trepte, Func<PerfSaft.Punct, IReadOnlyList<PerfSaft.Masura>> masoaraProces)
    : ScenaDocumente(deschide, check, true, inchide, "PERF" + istoric, an) {

    public sealed record Punct(int Istoric, int An, int Luna, int Unitati, string Modul, bool Planuri);

    public sealed record Masura(int Istoric, int Unitati, string Modul, string Faza, double Ms, int Comenzi, double MsSql,
        long Randuri, long Alocati, long VarfGestionat, long VarfSetLucru, long OctetiXml, double MsXml, bool XsdValid,
        int Tranzactii, int Facturi, int Plati, int Miscari, int Pozitii, int Refuzuri, long RanduriPostare, long RanduriSnapshot,
        double MsServerMax = 0);

    static string Tabela(CapturaSql.Comanda c) => CapturaSql.Tabele([c.Text]).FirstOrDefault() ?? "";

    public List<Masura> Masuri { get; } = [];
    protected override int UltimulAn => An + 1;
    int numarProdus;

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
            Comanda(os => ((EFCoreObjectSpace)os).DbContext.Database.ExecuteSqlRaw("ANALYZE"));
            foreach (var modul in new[] { "L", "S" })
                Masuri.AddRange(masoaraProces(new Punct(istoric, anM, lunaM, k, modul, k == trepte[^1])));
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

    Guid Asm(DateOnly d, LinieScena consum) => CuSpatiu(os => {
        var x = os.CreateObject<Asamblare>(); x.Data = d; x.DataInregistrare = d; x.PredatorId = Magazie; x.PrimitorId = Magazie;
        var c = os.CreateObject<AsamblareDetaliu>(); c.Document = x; c.Pozitie = 1; c.Directie = DirectieAsamblare.Consum;
        c.LotId = consum.Lot; c.Cantitate = 1; c.TipMaterialId = os.GetObjectByKey<Produs>(consum.Produs!.Value).TipMaterialId!.Value;
        var l = os.CreateObject<AsamblareDetaliu>(); l.Document = x; l.Pozitie = 2; l.Directie = DirectieAsamblare.Produs;
        l.TipMaterialId = Tip(os, Stoc); l.Cantitate = 1; l.PretEvaluare = 10;
        var p = os.CreateObject<Produs>(); p.Cod = Marcaj + "-ASM" + ++numarProdus; p.Denumire = p.Cod; p.UM = "BUC";
        p.TipMaterialId = l.TipMaterialId; l.ProdusId = p.ID;
        l.CreeazaLot(os, p, os.GetObjectByKey<Gestiune>(Magazie));
        os.CommitChanges(); return x.ID;
    });

    // O unitate: FCT (stoc + serviciu) cu NIR conex, PLT parțial legată, BCS, BTR, DSC, ASM, FCL cu INC legată.
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
        Opereaza(Asm(d, lot));
        var fcl = Fcl(d, 100); Opereaza(fcl);
        var inc = Trezorerie(true, 60).Id; Dateaza(inc, d); Opereaza(inc);
        Imperecheaza(inc, fcl, 60, d);
    }

    // Procesul-copil: aceeași măsurare de două ori — prima e rece (proces nou, pool gol), a doua caldă.
    public static List<Masura> MasoaraInProces(Func<IObjectSpace> deschide, Punct punct, string director, Action<string, bool> check) =>
        [Masoara(deschide, punct, "rece", false, director, check), Masoara(deschide, punct, "cald", punct.Planuri, director, check)];

    static Masura Masoara(Func<IObjectSpace> deschide, Punct punct, string faza, bool cuPlanuri, string director, Action<string, bool> check) {
        var (istoric, anM, lunaM, k, modul) = (punct.Istoric, punct.An, punct.Luna, punct.Unitati, punct.Modul);
        using var os = deschide();
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
        using var proces = Process.GetCurrentProcess();
        var setLucru = proces.PeakWorkingSet64;
        var xsd = dto.Refuzuri.Count == 0 && XsdD406.Valideaza(cale).Count == 0;
        double serverMax = 0;
        if (cuPlanuri && dto.Refuzuri.Count == 0)
            serverMax = Planuri(os, comenzi, director, $"perf-planuri-{modul}-m{istoric}-k{k}.txt");
        var m = new Masura(istoric, k, modul, faza, ms, comenzi.Count, comenzi.Sum(c => c.Durata.TotalMilliseconds),
            comenzi.Sum(c => c.Randuri), alocati, varf, setLucru, File.Exists(cale) ? new FileInfo(cale).Length : 0, msXml, xsd,
            dto.Jurnale.Sum(j => j.Tranzactii.Count), dto.FacturiEmise.Count + dto.FacturiPrimite.Count, dto.Plati.Count,
            dto.MiscariStoc.Count, dto.StocFizic.Count, dto.Refuzuri.Count,
            comenzi.Where(c => Tabela(c) == "Postare").Sum(c => c.Randuri),
            comenzi.Where(c => Tabela(c).StartsWith("SolduriPerioada")).Sum(c => c.Randuri), serverMax);
        Console.WriteLine($"     MĂSURAT (perf {modul} m{istoric} k{k} {faza}): {m.Ms:0} ms, {m.Comenzi} comenzi / {m.MsSql:0} ms SQL / "
            + $"{m.Randuri} rânduri, alocați {m.Alocati / 1048576.0:0.0} MiB, vârf gestionat {m.VarfGestionat / 1048576.0:0.0} MiB, "
            + $"vârf set de lucru {m.VarfSetLucru / 1048576.0:0.0} MiB, XML {m.OctetiXml / 1024.0:0} KiB în {m.MsXml:0} ms "
            + $"(XSD {(m.XsdValid ? "valid" : "INVALID")}); {m.Tranzactii} tranzacții, {m.Facturi} facturi, {m.Plati} plăți, "
            + $"{m.Miscari} mișcări, {m.Pozitii} poziții; rânduri din Postare {m.RanduriPostare}, din snapshot {m.RanduriSnapshot}");
        return m;
    }

    public const string PrefixJson = "PERF-JSON ";
    public static string Json(IReadOnlyList<Masura> masuri) => PrefixJson + JsonSerializer.Serialize(masuri);
    public static List<Masura> DinJson(string linie) => JsonSerializer.Deserialize<List<Masura>>(linie[PrefixJson.Length..]);

    // Criteriile B8-D3, așa cum au fost aprobate, pe matricea k × m; tabelul intră în p5-perf-masuratori.md.
    public static void Evalueaza(List<Masura> toate, Action<string, bool> check, string director) {
        foreach (var modul in new[] { "L", "S" })
        foreach (var faza in new[] { "rece", "cald" }) {
            var puncte = toate.Where(m => m.Modul == modul && m.Faza == faza).ToList();
            check($"SAF-B8-D3 ({modul}, {faza}): numărul de comenzi SQL nu depinde de k și m "
                + $"([{string.Join(", ", puncte.Select(p => $"m{p.Istoric}k{p.Unitati}:{p.Comenzi}"))}])",
                puncte.Count > 0 && puncte.Select(p => p.Comenzi).Distinct().Count() == 1);
            foreach (var m in puncte.Select(p => p.Istoric).Distinct().Order()) {
                var scara = puncte.Where(p => p.Istoric == m).OrderBy(p => p.Unitati).ToList();
                var perechi = scara.Zip(scara.Skip(1)).ToList();
                bool Liniar(Func<Masura, double> f) =>
                    perechi.All(x => f(x.Second) <= 1.25 * f(x.First) * x.Second.Unitati / x.First.Unitati);
                check($"SAF-B8-D3 ({modul}, {faza}, m={m}): durata și alocările cresc cel mult liniar în k, toleranță 25% pe pas "
                    + $"([{string.Join(", ", scara.Select(p => $"k{p.Unitati}: {p.Ms:0} ms / {p.Alocati / 1048576.0:0.0} MiB"))}])",
                    perechi.Count == 3 && Liniar(p => p.Ms) && Liniar(p => p.Alocati));
                var maxim = scara[^1];
                check($"SAF-B8-D3 ({modul}, {faza}, m={m}): scena k={maxim.Unitati} se exportă integral — fără refuzuri, XML scris, XSD valid",
                    maxim.Refuzuri == 0 && maxim.OctetiXml > 0 && maxim.XsdValid);
                if (faza == "cald")
                    check($"SAF-B8-D3 ({modul}, m={m}, suplimentar): la k={maxim.Unitati} nicio interogare nu depășește 100 ms pe server "
                        + $"(EXPLAIN ANALYZE; maxim {maxim.MsServerMax:0.0} ms)", maxim.MsServerMax is > 0 and <= 100);
            }
            if (modul == "S")
                foreach (var k in puncte.Select(p => p.Unitati).Distinct().Order()) {
                    var istoric = puncte.Where(p => p.Unitati == k).OrderBy(p => p.Istoric).ToList();
                    var baza = istoric[0];
                    check($"SAF-B8-D3 (S, {faza}, k={k}): Opening citește snapshot-ul — rândurile din `Postare` nu cresc cu m "
                        + "(toleranță 25%), iar cele din snapshot urmează pozițiile deschise "
                        + $"([{string.Join(", ", istoric.Select(p => $"m{p.Istoric}: Postare {p.RanduriPostare}, snapshot {p.RanduriSnapshot}, poziții {p.Pozitii}"))}])",
                        istoric.Count == 3 && istoric.Skip(1).All(p => p.RanduriPostare <= 1.25 * baza.RanduriPostare && p.RanduriSnapshot <= 2 * p.Pozitii));
                }
        }
        var sb = new StringBuilder("| m | k | modul | faza | ms | comenzi | ms SQL | rânduri | alocați MiB | vârf gestionat MiB | vârf set lucru MiB | XML KiB | ms XML | tranzacții | facturi | plăți | mișcări | poziții | rânduri Postare | rânduri snapshot | server max ms |\n"
            + "|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|\n");
        foreach (var m in toate.OrderBy(m => m.Istoric).ThenBy(m => m.Modul).ThenBy(m => m.Unitati).ThenByDescending(m => m.Faza))
            sb.Append($"| {m.Istoric} | {m.Unitati} | {m.Modul} | {m.Faza} | {m.Ms:0} | {m.Comenzi} | {m.MsSql:0} | {m.Randuri} | "
                + $"{m.Alocati / 1048576.0:0.0} | {m.VarfGestionat / 1048576.0:0.0} | {m.VarfSetLucru / 1048576.0:0.0} | {m.OctetiXml / 1024.0:0} | {m.MsXml:0} | "
                + $"{m.Tranzactii} | {m.Facturi} | {m.Plati} | {m.Miscari} | {m.Pozitii} | {m.RanduriPostare} | {m.RanduriSnapshot} | "
                + $"{(m.MsServerMax > 0 ? m.MsServerMax.ToString("0.0") : "")} |\n");
        File.WriteAllText(Path.Combine(director, "perf-saft.md"), sb.ToString());
        Console.WriteLine($"     PERF tabel: {Path.Combine(director, "perf-saft.md")}");
    }

    // DUK pe XML-urile calde de la k maxim, separat de export (B8-D3): kitul are JRE numai pentru Windows.
    public static void ValideazaDuk(string director, Action<string, bool> check) {
        foreach (var cale in Directory.EnumerateFiles(director, "perf-*-k64-cald.xml").Order()) {
            var ceas = Stopwatch.StartNew();
            var duk = Duk.Valideaza(cale);
            Console.WriteLine($"     MĂSURAT (perf DUK {Path.GetFileName(cale)}): {(duk.Valid ? "ok" : "RESPINS")} în {ceas.Elapsed.TotalMilliseconds:0} ms");
            check($"SAF-B8-D3: {Path.GetFileName(cale)} validat de DUK, fără atenționări", duk.Valid && duk.Avertismente.Count == 0);
        }
    }

    // EXPLAIN (ANALYZE, BUFFERS) pe fiecare comandă a exportului, cu parametrii ei; cele mai scumpe primele.
    static double Planuri(IObjectSpace os, List<CapturaSql.Comanda> comenzi, string director, string fisier) {
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
