using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Atlas.Conta.BackOffice.Module.Api;
using Atlas.Conta.BackOffice.Module.Api.Perioade;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Cub.Citiri;
using Atlas.Conta.BackOffice.Module.Motor;
using Atlas.Conta.BackOffice.Module.Proiectii;
using Atlas.Conta.BackOffice.Module.Saft;
using DevExpress.EntityFrameworkCore.Security;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.EFCore;
using DevExpress.ExpressApp.Security;
using DevExpress.Persistent.BaseImpl.EF.PermissionPolicy;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Conta.BackOffice.ModelCheck;

sealed partial class PerfCub {
    public const string Snapshot = "snapshot", Interval = "interval", Recitire = "recitire", Reconstructie = "reconstrucție", Scriere = "scriere";

    public sealed record Punct(int Istoric, int An, int Luna, int Unitati, string Operatie, bool Planuri, bool Privat, string Marcaj,
        Guid BcsDraft, Guid NtcDraft, Guid Furnizor = default, Guid Client = default, Guid ContFurnizor = default, Guid ContClient = default,
        Guid Loc = default);

    public sealed record Masura(int Istoric, int Unitati, string Operatie, string Faza, double Ms, int Comenzi, double MsSql, long Randuri,
        long Alocati, long VarfGestionat, long VarfSetLucru, long Cardinal, Dictionary<string, decimal> Control,
        int ScanariPostare, double RanduriPostare, long BuffersPostare, double RanduriSnapshot, double MsServerMax, string Eroare,
        int ScanariIndex = 0, double RanduriIndex = 0, long BuffersIndex = 0, int SecventialeIndex = 0,
        int PlanuriCerute = 0, int PlanuriRespinse = 0);

    public sealed record DurataComanda(int Istoric, int Treapta, string Tip, double Ms, int Comenzi, double MsSql);

    public sealed record Citit(long Cardinal, Dictionary<string, decimal> Control);

    public sealed class Mediu(IObjectSpace os, Punct p, ISecurityStrategyBase securitate, string director, string faza) {
        public IObjectSpace Os => os;
        public Punct P => p;
        public ISecurityStrategyBase Securitate => securitate;
        public DateOnly Start => new(p.An, p.Luna, 1);
        public DateOnly End => new(p.An, p.Luna, DateTime.DaysInMonth(p.An, p.Luna));
        public string Fisier(string extensie) => Path.Combine(director, $"perf-cub-{p.Operatie}-m{p.Istoric}-k{p.Unitati}-{faza}.{extensie}");
        public Action<Dictionary<string, decimal>> Dupa { get; set; }
    }

    public sealed record Operatie(string Cod, string Nume, string Ruta, bool Securizat, bool NumaiPrivat, Func<Mediu, Citit> Ruleaza);

    static IEnumerable<Operatie> Cumulata(string cod, string nume, Func<Mediu, CitireCumul, Citit> f) => [
        new(cod + "-SV", nume, Recitire, true, false, m => f(m, CitireCumul.Vizibila)),
        new(cod + "-NV", nume, Recitire, false, false, m => f(m, CitireCumul.Vizibila)),
        new(cod + "-NI", nume, Snapshot, false, false, m => f(m, CitireCumul.Integrala))];

    static IEnumerable<Operatie> PeInterval(string cod, string nume, bool numaiPrivat, Func<Mediu, Citit> f, string ruta = Interval) => [
        new(cod + "-S", nume, ruta, true, numaiPrivat, f), new(cod + "-N", nume, ruta, false, numaiPrivat, f)];

    public static readonly Operatie[] Operatii = [
        .. Cumulata("BAL", "balanța analitică a lunii", (m, c) => {
            var r = ContabilProiectii.Balanta(m.Os, m.Start, m.End, analitic: true, citire: c).ToList();
            var f = r.Where(x => x.ContId == m.P.ContFurnizor && x.RepartitorId == m.P.Furnizor).ToList();
            var cl = r.Where(x => x.ContId == m.P.ContClient && x.RepartitorId == m.P.Client).ToList();
            return new(r.Count, new() {
                ["furnizor.initialC"] = f.Sum(x => x.SoldInitialCredit - x.SoldInitialDebit), ["furnizor.rulajD"] = f.Sum(x => x.RulajDebit),
                ["furnizor.rulajC"] = f.Sum(x => x.RulajCredit), ["furnizor.finalC"] = f.Sum(x => x.SoldFinalCredit - x.SoldFinalDebit),
                ["client.rulajD"] = cl.Sum(x => x.RulajDebit), ["client.finalD"] = cl.Sum(x => x.SoldFinalDebit - x.SoldFinalCredit),
            });
        }),
        .. Cumulata("FISA", "fișa contului de furnizori pe partener, luna", (m, c) => {
            var r = ContabilProiectii.FisaCont(m.Os, m.P.ContFurnizor, m.Start, m.End, repartitorId: m.P.Furnizor, citire: c).ToList();
            return new(r.Count, new() {
                ["debit"] = r.Sum(x => x.Debit), ["credit"] = r.Sum(x => x.Credit), ["soldFinal"] = r.Count == 0 ? 0 : r[^1].SoldCurent,
            });
        }),
        .. PeInterval("JRN", "registrul-jurnal al lunii", false, m => {
            var r = ContabilProiectii.RegistruJurnal(m.Os, m.Start, m.End).ToList();
            return new(r.Count, new() {
                ["echilibru"] = r.Sum(x => x.Debit - x.Credit),
                ["furnizor.credit"] = r.Where(x => x.ContId == m.P.ContFurnizor).Sum(x => x.Credit),
                ["client.debit"] = r.Where(x => x.ContId == m.P.ContClient).Sum(x => x.Debit),
            });
        }),
        .. Cumulata("SPART", "soldul partenerilor la sfârșitul lunii", (m, c) => {
            var r = ContabilProiectii.SoldParteneri(m.Os, m.End, citire: c).ToList();
            return new(r.Count, new() {
                ["furnizor.creditor"] = r.Where(x => x.ContId == m.P.ContFurnizor && x.RepartitorId == m.P.Furnizor).Sum(x => x.SoldCreditor - x.SoldDebitor),
                ["client.debitor"] = r.Where(x => x.ContId == m.P.ContClient && x.RepartitorId == m.P.Client).Sum(x => x.SoldDebitor - x.SoldCreditor),
            });
        }),
        .. Cumulata("PREST", "partidele cu rest la sfârșitul lunii", (m, c) => {
            var r = ImperecheriProiectii.PartideCuRest(m.Os, laData: m.End, citire: c).ToList();
            var f = r.Where(x => x.ContrapartidaId == m.P.Furnizor).ToList();
            var cl = r.Where(x => x.ContrapartidaId == m.P.Client).ToList();
            return new(r.Count, new() {
                ["furnizor.partide"] = f.Count, ["furnizor.rest"] = f.Sum(x => x.Rest),
                ["client.partide"] = cl.Count, ["client.rest"] = cl.Sum(x => x.Rest),
            });
        }),
        .. Cumulata("STOC", "raportul de stoc la sfârșitul lunii", (m, c) => {
            var r = StocProiectii.SoldStoc(m.Os, m.End, citire: c).ToList();
            var stoc = r.Where(x => x.RepartitorId != m.P.Loc).ToList();
            var consum = r.Where(x => x.RepartitorId == m.P.Loc).ToList();
            return new(r.Count, new() {
                ["stoc.cantitate"] = stoc.Sum(x => x.Cantitate), ["stoc.valoare"] = stoc.Sum(x => x.Valoare),
                ["consum.cantitate"] = consum.Sum(x => x.Cantitate), ["consum.valoare"] = consum.Sum(x => x.Valoare),
            });
        }),
        new("FIFO-N", "dry-run-ul unui consum nou pe lotul lung (disponibilul FIFO)", Snapshot, false, false, m => {
            var refuzuri = ComenziDocument.Sistem(m.Os).Valideaza(m.P.BcsDraft);
            foreach (var r in refuzuri) Console.WriteLine("     PERFCUB REFUZ " + r);
            return new(1, new() { ["refuzuri"] = refuzuri.Count });
        }),
        new("PDISP-N", "dry-run-ul unei note stingătoare (PartideDisponibile)", Recitire, false, false, m => {
            var refuzuri = ComenziDocument.Sistem(m.Os).Valideaza(m.P.NtcDraft);
            foreach (var r in refuzuri) Console.WriteLine("     PERFCUB REFUZ " + r);
            return new(1, new() { ["refuzuri"] = refuzuri.Count });
        }),
        new("RECON-N", "reconstrucția snapshot-urilor", Reconstructie, false, false, m => {
            var r = PerioadeApply.Reconstruieste(m.Os);
            return new(r.Referinte.Length, new() {
                ["referinte"] = r.Referinte.Length,
                ["diferite"] = r.Referinte.Sum(x => x.ContabilDiferite + x.StocDiferite + x.PartideDiferite),
            });
        }),
        .. PeInterval("JTVA", "jurnalele de TVA ale lunii", true, m => {
            var a = TvaProiectii.JurnalTva(m.Os, SensTva.Achizitie, m.Start, m.End).ToList();
            var l = TvaProiectii.JurnalTva(m.Os, SensTva.Livrare, m.Start, m.End).ToList();
            return new(a.Count + l.Count, new() {
                ["achizitii.baza"] = a.Sum(x => x.Baza), ["achizitii.tva"] = a.Sum(x => x.Tva),
                ["livrari.baza"] = l.Sum(x => x.Baza), ["livrari.tva"] = l.Sum(x => x.Tva),
            });
        }),
        .. PeInterval("D300", "D300 pe lună", true, m => {
            var d = D300Proiectii.D300(m.Os, m.Start, m.End, null);
            D300Rand Rand(string cod) => d.Randuri.Single(r => r.Cod == cod);
            return new(d.Randuri.Count, new() {
                ["nemapate"] = d.Nemapate.Count,
                ["rd9.baza"] = Rand("9").Baza ?? 0, ["rd9.tva"] = Rand("9").Tva ?? 0,
                ["rd24.baza"] = Rand("24").Baza ?? 0, ["rd24.tva"] = Rand("24").Tva ?? 0,
                ["rd19.tva"] = Rand("19").Tva ?? 0, ["rd30.tva"] = Rand("30").Tva ?? 0,
            });
        }),
        .. PeInterval("D394", "D394 pe lună", true, m => {
            var d = D394Proiectii.D394(m.Os, m.Start, m.End);
            return new(d.Operatiuni.Count + d.Neincluse.Count, new() {
                ["documente"] = d.Operatiuni.Sum(x => x.Documente), ["baza"] = d.Operatiuni.Sum(x => x.Baza),
                ["neincluse.baza"] = d.Neincluse.Sum(x => x.Baza),
            });
        }),
        .. PeInterval("R6", "raportul de impact TVA pe lună", true, m => {
            var cerinte = m.Securitate as IRequestSecurityStrategy;
            var r = DiagnosticTvaService.Citeste(m.Os, m.Start, m.End,
                poateCiti: cerinte == null ? null : (obiect, membru) => cerinte.CanRead(m.Os, obiect, membru));
            return new(r.Count, new() { ["baza"] = r.Sum(x => x.Baza), ["taxa"] = r.Sum(x => x.Taxa) });
        }),
        .. PeInterval("SAFTL", "SAF-T L", true, m => Saft(m, true), Snapshot),
        .. PeInterval("SAFTS", "SAF-T S", true, m => Saft(m, false), Snapshot),
        new("INCH-N", "închiderea lunii măsurate", Scriere, false, false, m => {
            var severitate = m.Os.FirstOrDefault<PoliticaInchidere>(p => p.Fel == FelConstatareInchidere.ItvLipsa);
            var initiala = severitate?.Severitate;
            if (severitate != null) { severitate.Severitate = SeveritateConstatare.Avertisment; m.Os.CommitChanges(); }
            try {
                PerioadaService.Inchide(m.Os, m.P.An, m.P.Luna,
                    PerioadaService.Verifica(m.Os, m.P.An, m.P.Luna).Select(c => c.Cheie).ToArray(), null, "PerfCub");
            }
            finally {
                if (severitate != null) { severitate.Severitate = initiala.Value; m.Os.CommitChanges(); }
            }
            return new(1, new() { ["snapshot"] = SolduriService.AreSnapshot(m.Os, m.P.An, m.P.Luna) ? 1 : 0 });
        }),
    ];

    static Citit Saft(Mediu m, bool l) {
        var data = new DateOnly(m.P.An, m.P.Luna, 28);
        var dto = l ? SaftProiectii.SaftPeCub(m.Os, m.P.An, m.P.Luna, data) : SaftProiectii.SaftStocuriPeCub(m.Os, m.P.An, m.P.Luna, data);
        foreach (var r in dto.Refuzuri) Console.WriteLine($"     PERFCUB REFUZ {r.Cod}: {r.Mesaj}");
        var control = new Dictionary<string, decimal> {
            ["refuzuri"] = dto.Refuzuri.Count, ["facturi"] = dto.FacturiEmise.Count + dto.FacturiPrimite.Count,
            ["plati"] = dto.Plati.Count, ["miscari"] = dto.MiscariStoc.Count,
        };
        if (m.P.Planuri && dto.Refuzuri.Count == 0)
            m.Dupa = c => {
                var cale = m.Fisier("xml");
                using (var fs = File.Create(cale)) SaftXml.Scrie(dto, fs);
                c["xsd"] = XsdD406.Valideaza(cale).Count;
            };
        return new(dto.Jurnale.Sum(j => j.Tranzactii.Count) + dto.MiscariStoc.Count + dto.StocFizic.Count, control);
    }

    // Procesul-copil: aceeași operație de două ori — prima e rece (proces nou), a doua caldă; pe ușa securizată, logonul precede măsurarea.
    public static List<Masura> MasoaraInProces(string conexiune, Punct punct, string director) {
        var op = Operatii.Single(o => o.Cod == punct.Operatie);
        Action<DbContextOptionsBuilder> configureaza = b => b.UseNpgsql(conexiune).UseChangeTrackingProxies()
            .UseObjectSpaceLinkProxies().UseLazyLoadingProxies().ConfigureLoggingCacheTime(TimeSpan.Zero);
        ISecurityStrategyBase securitate = null;
        IDisposable furnizor;
        Func<IObjectSpace> deschide;
        IObjectSpace logon = null;
        if (op.Securizat) {
            var autentificare = new AuthenticationStandard(typeof(ApplicationUser), typeof(AuthenticationStandardLogonParameters));
            var strategie = new SecurityStrategyComplex(typeof(ApplicationUser), typeof(PermissionPolicyRole), autentificare);
            var p = new SecuredEFCoreObjectSpaceProvider<BackOfficeEFCoreDbContext>(strategie, (b, _) => configureaza(b));
            autentificare.SetLogonParameters(new AuthenticationStandardLogonParameters(Utilizator, ""));
            logon = p.CreateNonsecuredObjectSpace();
            strategie.Logon(logon);
            securitate = strategie; furnizor = p; deschide = p.CreateObjectSpace;
        }
        else {
            var p = new EFCoreObjectSpaceProvider<BackOfficeEFCoreDbContext>((b, _) => configureaza(b));
            furnizor = p; deschide = p.CreateObjectSpace;
        }
        try {
            return [Masoara(op, deschide, securitate, punct, "rece", false, director), Masoara(op, deschide, securitate, punct, "cald", punct.Planuri, director)];
        }
        finally { logon?.Dispose(); furnizor.Dispose(); }
    }

    static Masura Masoara(Operatie op, Func<IObjectSpace> deschide, ISecurityStrategyBase securitate, Punct punct, string faza, bool cuPlanuri, string director) {
        using var os = deschide();
        var mediu = new Mediu(os, punct, securitate, director, faza);
        var alocatInainte = GC.GetTotalAllocatedBytes(true);
        long varf = 0;
        using var opreste = new CancellationTokenSource();
        var esantionare = Task.Run(() => {
            while (!opreste.IsCancellationRequested) {
                varf = Math.Max(varf, GC.GetTotalMemory(false));
                Thread.Sleep(2);
            }
        });
        Citit citit = null;
        string eroare = null;
        var ceas = Stopwatch.StartNew();
        var comenzi = CapturaSql.Masoara(() => {
            try { citit = op.Ruleaza(mediu); }
            catch (Exception e) { eroare = e.GetType().Name + ": " + e.Message.Split('\n')[0]; }
        });
        var ms = ceas.Elapsed.TotalMilliseconds;
        opreste.Cancel(); esantionare.Wait();
        var alocati = GC.GetTotalAllocatedBytes(true) - alocatInainte;
        if (citit != null) mediu.Dupa?.Invoke(citit.Control);
        using var proces = Process.GetCurrentProcess();
        if (op.Cod == "INCH-N" && eroare == null) {
            using var osRedeschide = deschide();
            PerioadaService.Redeschide(osRedeschide, punct.An, punct.Luna, "PerfCub", null, "PerfCub");
        }
        var gol = new StatisticaPlan(0, 0, 0, 0, 0, 0, 0, 0);
        var plan = cuPlanuri && eroare == null ? Planuri(os, comenzi, mediu.Fisier("planuri.txt"), false) : gol;
        var index = cuPlanuri && eroare == null ? Planuri(os, comenzi, mediu.Fisier("planuri-index.txt"), true) : gol;
        var m = CuPlanuri(new Masura(punct.Istoric, punct.Unitati, op.Cod, faza, ms, comenzi.Count, comenzi.Sum(c => c.Durata.TotalMilliseconds),
            comenzi.Sum(c => c.Randuri), alocati, varf, proces.PeakWorkingSet64, citit?.Cardinal ?? 0, citit?.Control ?? [],
            0, 0, 0, 0, 0, eroare), plan, index);
        Console.WriteLine($"     MĂSURAT (perfcub {op.Cod} m{punct.Istoric} k{punct.Unitati} {faza}): {m.Ms:0} ms, {m.Comenzi} comenzi / {m.MsSql:0} ms SQL / "
            + $"{m.Randuri} rânduri citite, {m.Cardinal} rânduri livrate, alocați {m.Alocati / 1048576.0:0.0} MiB, vârf gestionat {m.VarfGestionat / 1048576.0:0.0} MiB"
            + (cuPlanuri ? $"; plan: {m.ScanariPostare} scanări pe Postare, {m.RanduriPostare:0} rânduri, {m.BuffersPostare} buffers, snapshot {m.RanduriSnapshot:0}"
                + $"; fără scanare secvențială: {m.ScanariIndex} scanări ({m.SecventialeIndex} secvențiale), {m.RanduriIndex:0} rânduri, {m.BuffersIndex} buffers" : "")
            + (m.Eroare == null ? "" : "; EROARE " + m.Eroare));
        return m;
    }

    // Un plan respins invalidează măsurarea: statisticile lui lipsesc, nu sunt zero.
    static Masura CuPlanuri(Masura m, StatisticaPlan plan, StatisticaPlan index) => m with {
        ScanariPostare = plan.Scanari, RanduriPostare = plan.Randuri, BuffersPostare = plan.Buffers,
        RanduriSnapshot = plan.RanduriSnapshot, MsServerMax = plan.MsServerMax,
        ScanariIndex = index.Scanari, RanduriIndex = index.Randuri, BuffersIndex = index.Buffers, SecventialeIndex = index.Secventiale,
        PlanuriCerute = plan.Cerute + index.Cerute, PlanuriRespinse = plan.Respinse + index.Respinse,
        Eroare = m.Eroare ?? (plan.Respinse + index.Respinse == 0 ? null
            : $"EXPLAIN respins pe {plan.Respinse + index.Respinse} din {plan.Cerute + index.Cerute} planuri cerute"),
    };

    public const string PrefixJson = "PERFCUB-JSON ";
    public static string Json(IReadOnlyList<Masura> masuri) => PrefixJson + JsonSerializer.Serialize(masuri);
    public static List<Masura> DinJson(string linie) => JsonSerializer.Deserialize<List<Masura>>(linie[PrefixJson.Length..]);
    public static string Argument(Punct p) => Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(p)));
    public static Punct DinArgument(string a) => JsonSerializer.Deserialize<Punct>(Encoding.UTF8.GetString(Convert.FromBase64String(a)));

    sealed record StatisticaPlan(int Scanari, double Randuri, long Buffers, double RanduriSnapshot, double MsServerMax, int Secventiale,
        int Cerute, int Respinse);

    // EXPLAIN (ANALYZE, BUFFERS) pe fiecare citire a operației, cu parametrii ei; proba din plan numără nodurile de scanare pe
    // partițiile lui `Postare`: rândurile atinse (livrate + respinse de filtru) × bucle și bufferele lor. A doua trecere
    // interzice scanarea secvențială: arată calea de acces pe care planificatorul o are când luna e o fracțiune mică din tabelă.
    static StatisticaPlan Planuri(IObjectSpace os, List<CapturaSql.Comanda> comenzi, string fisier, bool faraSecvential) {
        var conexiune = ((EFCoreObjectSpace)os).DbContext.Database.GetDbConnection();
        if (conexiune.State != System.Data.ConnectionState.Open) conexiune.Open();
        using (var regim = conexiune.CreateCommand()) {
            regim.CommandText = "SET enable_seqscan = " + (faraSecvential ? "off" : "on");
            regim.ExecuteNonQuery();
        }
        var text = new StringBuilder();
        var brute = new List<string>();
        int scanari = 0, secventiale = 0, cerute = 0, respinse = 0; double randuri = 0, snapshot = 0, serverMaxim = 0; long buffers = 0;
        foreach (var c in comenzi.Where(c => c.Text.TrimStart().StartsWith("SELECT", StringComparison.OrdinalIgnoreCase)
                     || c.Text.TrimStart().StartsWith("WITH", StringComparison.OrdinalIgnoreCase))) {
            if (c.Text.Contains("FOR UPDATE", StringComparison.OrdinalIgnoreCase) || c.Text.Contains("pg_advisory", StringComparison.OrdinalIgnoreCase)) continue;
            cerute++;
            using var cmd = conexiune.CreateCommand();
            cmd.CommandText = "EXPLAIN (ANALYZE, BUFFERS, FORMAT JSON) " + c.Text;
            foreach (var p in c.Parametri) cmd.Parameters.Add(p is ICloneable cl ? (System.Data.Common.DbParameter)cl.Clone() : p);
            string json;
            try { json = (string)cmd.ExecuteScalar(); }
            catch (Exception e) { respinse++; text.AppendLine($"-- EXPLAIN respins: {e.Message.Split('\n')[0]}\n{c.Text}\n"); continue; }
            brute.Add(json);
            using var doc = JsonDocument.Parse(json);
            var radacina = doc.RootElement[0];
            var ms = radacina.GetProperty("Execution Time").GetDouble();
            serverMaxim = Math.Max(serverMaxim, ms);
            var noduri = new List<string>();
            void Nod(JsonElement n, int nivel) {
                var tip = n.GetProperty("Node Type").GetString();
                double Numar(string nume) => n.TryGetProperty(nume, out var v) ? v.GetDouble() : 0;
                if (n.TryGetProperty("Relation Name", out var rel)) {
                    var bucle = Numar("Actual Loops");
                    var atinse = (Numar("Actual Rows") + Numar("Rows Removed by Filter") + Numar("Rows Removed by Index Recheck")) * bucle;
                    var b = (long)(Numar("Shared Hit Blocks") + Numar("Shared Read Blocks"));
                    var nume = rel.GetString();
                    if (bucle > 0 && nume.StartsWith("Postare", StringComparison.Ordinal)) {
                        scanari++; randuri += atinse; buffers += b;
                        if (tip == "Seq Scan") secventiale++;
                    }
                    if (bucle > 0 && (nume.StartsWith("SolduriPerioada", StringComparison.Ordinal) || nume == "PartideDeschise")) snapshot += atinse;
                    noduri.Add($"{new string(' ', nivel * 2)}{tip} {nume}"
                        + (n.TryGetProperty("Index Name", out var ix) ? $" [{ix.GetString()}]" : "")
                        + $": rânduri {Numar("Actual Rows"):0.##} × {bucle:0}, respinse {Numar("Rows Removed by Filter"):0.##}, buffers {b}");
                }
                else noduri.Add($"{new string(' ', nivel * 2)}{tip}"
                    + (n.TryGetProperty("Index Name", out var ix) ? $" [{ix.GetString()}]" : "")
                    + $": rânduri {Numar("Actual Rows"):0.##} × {Numar("Actual Loops"):0}");
                if (n.TryGetProperty("Plans", out var copii))
                    foreach (var copil in copii.EnumerateArray()) Nod(copil, nivel + 1);
            }
            Nod(radacina.GetProperty("Plan"), 0);
            text.AppendLine($"-- în operație {c.Durata.TotalMilliseconds:0.0} ms, pe server {ms:0.000} ms, {c.Randuri} rânduri citite");
            text.AppendLine(c.Text);
            foreach (var n in noduri) text.AppendLine("   " + n);
            text.AppendLine();
        }
        File.WriteAllText(fisier, text.ToString());
        File.WriteAllText(Path.ChangeExtension(fisier, ".json"), "[" + string.Join(",\n", brute) + "]");
        using (var regim = conexiune.CreateCommand()) {
            regim.CommandText = "SET enable_seqscan = on";
            regim.ExecuteNonQuery();
        }
        return new(scanari, randuri, buffers, snapshot, serverMaxim, secventiale, cerute, respinse);
    }
}
