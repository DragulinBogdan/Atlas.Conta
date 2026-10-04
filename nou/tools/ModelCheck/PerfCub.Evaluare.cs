using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.Json;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Motor;
using DevExpress.ExpressApp;
using Microsoft.EntityFrameworkCore;
using C = Atlas.Conta.BackOffice.Module.Cub;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.ModelCheck;

sealed partial class PerfCub {
    public sealed record Volum(int Istoric, long Tranzactii, long Postari, long Documente, double MsReconciliere, double MsInvarianti,
        double MsValoriStoc, int Delta, int Reziduuri);

    string Profil => Privat ? "privat" : "bugetar";

    // Așteptările scenei, din regula contabilă a unității și a faptelor „o dată per bază”; nu vin din alt cititor.
    Dictionary<string, decimal> Asteptat(string operatie, int k, bool cuXml) {
        decimal f = Privat ? 181.50m : 150m, c = Privat ? 121m : 100m, lung = Privat ? 1210m : 1000m;
        decimal h = istoric * unitatiIstoric, u = h + k;
        decimal consumLung = istoric == 0 ? 2 : istoric + 1, q = Privat ? 7 : 8;
        decimal odataQ = (Privat ? 21 : 12) + (istoric > 0 ? 6 : 0), odataV = (Privat ? 270 : 130) + (istoric > 0 ? 90 : 0);
        return operatie.Split('-')[0] switch {
            "BAL" => new() {
                ["furnizor.initialC"] = h * (f - 70) + lung, ["furnizor.rulajD"] = k * 70, ["furnizor.rulajC"] = k * f,
                ["furnizor.finalC"] = u * (f - 70) + lung, ["client.rulajD"] = k * c, ["client.finalD"] = u * (c - 60),
            },
            "FISA" => new() { ["debit"] = k * 70, ["credit"] = k * f, ["soldFinal"] = -(u * (f - 70) + lung) },
            "JRN" => new() { ["echilibru"] = 0, ["furnizor.credit"] = k * f, ["client.debit"] = k * c },
            "SPART" => new() { ["furnizor.creditor"] = u * (f - 70) + lung, ["client.debitor"] = u * (c - 60) },
            "PREST" => new() {
                ["furnizor.partide"] = 2 * u + 1, ["furnizor.rest"] = u * (f - 50 + 20) + lung,
                ["client.partide"] = u, ["client.rest"] = u * (c - 60),
            },
            "STOC" => new() {
                ["stoc.cantitate"] = u * q + 1000 - consumLung + odataQ, ["stoc.valoare"] = u * q * 10 + 1000 - consumLung + odataV,
                ["consum.cantitate"] = 2 * u + consumLung + (istoric > 0 ? 1 : 0), ["consum.valoare"] = 20 * u + consumLung + (istoric > 0 ? 10 : 0),
            },
            "FIFO" or "PDISP" => new() { ["refuzuri"] = 0 },
            "RECON" => new() { ["referinte"] = istoric > 0 ? 1 : 0, ["diferite"] = 0 },
            "JTVA" => new() { ["achizitii.baza"] = k * 150, ["achizitii.tva"] = k * 31.50m, ["livrari.baza"] = k * 100, ["livrari.tva"] = k * 21 },
            "D300" => new() {
                ["nemapate"] = 0, ["rd9.baza"] = k * 100, ["rd9.tva"] = k * 21, ["rd24.baza"] = k * 150, ["rd24.tva"] = k * 31.50m,
                ["rd19.tva"] = k * 21, ["rd30.tva"] = k * 31.50m,
            },
            "D394" => new() { ["documente"] = 2 * k, ["baza"] = k * 250, ["neincluse.baza"] = 0 },
            "R6" => new() { ["baza"] = 0, ["taxa"] = 0 },
            "SAFTL" => cuXml ? new() { ["refuzuri"] = 0, ["facturi"] = 2 * k, ["plati"] = 2 * k, ["miscari"] = 0, ["xsd"] = 0 }
                : new() { ["refuzuri"] = 0, ["facturi"] = 2 * k, ["plati"] = 2 * k, ["miscari"] = 0 },
            "SAFTS" => cuXml ? new() { ["refuzuri"] = 0, ["facturi"] = 0, ["plati"] = 0, ["miscari"] = 6 * k + 1, ["xsd"] = 0 }
                : new() { ["refuzuri"] = 0, ["facturi"] = 0, ["plati"] = 0, ["miscari"] = 6 * k + 1 },
            "INCH" => new() { ["snapshot"] = 1 },
            _ => throw new InvalidOperationException(operatie),
        };
    }

    void Controleaza(Operatie op, Masura m, int k) {
        var asteptat = Asteptat(op.Cod, k, m.Control.ContainsKey("xsd"));
        var egale = m.Eroare == null && asteptat.Count == m.Control.Count
            && asteptat.All(a => m.Control.TryGetValue(a.Key, out var v) && v == a.Value);
        if (!egale)
            Console.WriteLine($"     PERFCUB CONTROL {op.Cod} m{istoric} k{k} {m.Faza}: " + (m.Eroare ?? string.Join(", ", asteptat.Keys.Union(m.Control.Keys)
                .Where(c => !asteptat.TryGetValue(c, out var a) || !m.Control.TryGetValue(c, out var v) || a != v)
                .Select(c => $"{c} așteptat {(asteptat.TryGetValue(c, out var a) ? a.ToString(CultureInfo.InvariantCulture) : "—")}"
                    + $" citit {(m.Control.TryGetValue(c, out var v) ? v.ToString(CultureInfo.InvariantCulture) : "—")}"))));
        Verifica("X-D5", $"controlul numeric {op.Cod} m={istoric} k={k} {m.Faza}: cifrele cititorului sunt cele ale scenei", egale);
    }

    // X-D3: reconcilierea integrală, INV-CUB și diagnosticul ASM-B7 pe baza de volum, cu faptele exercitate pe ramuri.
    void Reconciliaza(Guid fisa) {
        using var ctx = new BackOfficeEFCoreDbContext(optiuni);
        var ceas = Stopwatch.StartNew();
        var note = new List<string>();
        var randuri = ReconciliereCub.Ruleaza(ctx, null, note);
        var msReconciliere = ceas.Elapsed.TotalMilliseconds; ceas.Restart();
        string refuz = null;
        try { Comanda(C.Citiri.Invarianti.Verifica); }
        catch (OperareException e) { refuz = e.Message; }
        var msInvarianti = ceas.Elapsed.TotalMilliseconds; ceas.Restart();
        var valori = DiagnosticValoriStoc.Citeste(ctx, DateOnly.MaxValue);
        var msValori = ceas.Elapsed.TotalMilliseconds;
        var reziduuri = valori.Pozitii.Count(p => p.Reziduu);

        var feluri = ctx.Set<C.Tranzactie>().GroupBy(t => t.Fel).Select(g => new { g.Key, Numar = g.LongCount() }).ToDictionary(g => g.Key, g => g.Numar);
        long Fel(N.FelTranzactie fel) => feluri.GetValueOrDefault(fel);
        var postari = ctx.Set<C.Postare>().GroupBy(p => new { p.Spatiu, p.Carte }).Select(g => new { g.Key.Spatiu, g.Key.Carte, Numar = g.LongCount() }).ToList();
        var documente = ctx.Set<Document>().GroupBy(d => new { d.ClrType, d.Stare }).Select(g => new { g.Key.ClrType, g.Key.Stare, Numar = g.LongCount() })
            .ToList().OrderBy(d => d.ClrType).ThenBy(d => d.Stare).ToList();
        var contoare = new (string Ramura, string Fapt, long Numar, bool Cerut)[] {
            ("(a) contabil", "rânduri RegistruContabil cu document", ctx.Set<RegistruContabil>().LongCount(r => r.DocumentId != null), true),
            ("(a) contabil", "postări Carte=Contabil", postari.Where(p => p.Carte == N.Carte.Contabil).Sum(p => p.Numar), true),
            ("(b) stoc", "rânduri RegistruStoc", ctx.Set<RegistruStoc>().LongCount(), true),
            ("(b) stoc", "postări în spațiul Stoc", postari.Where(p => p.Spatiu == N.Spatiu.Stoc).Sum(p => p.Numar), true),
            ("(b) stoc", "postări LDI pe lot", ctx.Set<C.Postare>().LongCount(p => p.Unitate != null && p.Cantitate != 0
                && ctx.Set<ListaDiferenteInventar>().Any(d => d.ID == p.DocumentId)), true),
            ("(c) fiscal", "rânduri RegistruTva", ctx.Set<RegistruTva>().LongCount(), Privat),
            ("(c) fiscal", "postări cu rol fiscal", ctx.Set<C.Postare>().LongCount(p => p.RolTva != null), Privat),
            ("(c) fiscal", "postări Carte=Fiscal", postari.Where(p => p.Carte == N.Carte.Fiscal).Sum(p => p.Numar), true),
            ("(c) fiscal", "legături DVI–factură", ctx.Set<DviFactura>().LongCount(), Privat),
            ("(c) fiscal", "postări RLF", ctx.Set<C.Postare>().LongCount(p => ctx.Set<ReturFurnizor>().Any(d => d.ID == p.DocumentId)), Privat),
            ("(d) balanță", "tranzacții Operare", Fel(N.FelTranzactie.Operare), true),
            ("(d) balanță", "tranzacții Deschidere", Fel(N.FelTranzactie.Deschidere), istoric > 0),
            ("(e) număr", "tranzacții Transfer", Fel(N.FelTranzactie.Transfer), true),
            ("(e) număr", "legături Imperechere", ctx.Set<Imperechere>().LongCount(), true),
            ("(f) partide", "perioade închise", ctx.Set<PerioadaFiscala>().LongCount(p => p.Inchisa && p.An >= An && p.An <= UltimulAn), istoric > 0),
            ("(f) partide", "partide în snapshot", ctx.Set<PartidaDeschisa>().LongCount(), istoric > 0),
            ("(f) partide", "transferuri ale unei note contabile", ctx.Set<C.Tranzactie>().LongCount(t => t.Fel == N.FelTranzactie.Transfer
                && ctx.Set<NotaContabila>().Any(d => d.ID == t.DocumentId)), true),
            ("(g) fiscal storno", "tranzacții Storno", Fel(N.FelTranzactie.Storno), true),
            ("(g) fiscal storno", "postări fiscale în Storno", ctx.Set<C.Postare>().LongCount(p => p.RolTva != null && p.Tranzactie.Fel == N.FelTranzactie.Storno), Privat),
            ("INV-CUB", "postări pe fișa imobilizării", ctx.Set<C.Postare>().LongCount(p => p.Unitate == fisa), true),
            ("INV-CUB", "rânduri RegistruImobilizari", ctx.Set<RegistruImobilizari>().LongCount(), true),
            ("INV-CUB", "tranzacții cu explicație", ctx.Set<C.Tranzactie>().LongCount(t => t.Explicatie != null), true),
        };
        var vide = contoare.Where(c => c.Cerut && c.Numar == 0).ToList();
        VolumFinal = new(istoric, feluri.Values.Sum(), postari.Sum(p => p.Numar), documente.Sum(d => d.Numar),
            msReconciliere, msInvarianti, msValori, randuri.Count, reziduuri);

        var sb = new StringBuilder($"# X-D3 — {Profil}, m = {istoric} luni închise × {unitatiIstoric} unități, luna măsurată cu {trepte[^1]} unități\n\n");
        sb.AppendLine($"Baza: {VolumFinal.Tranzactii} tranzacții, {VolumFinal.Postari} postări, {VolumFinal.Documente} documente.\n");
        sb.AppendLine("## Faptele exercitate pe ramuri\n\n| ramura | fapt | număr | cerut pe profil |\n|---|---|---|---|");
        foreach (var c in contoare) sb.AppendLine($"| {c.Ramura} | {c.Fapt} | {c.Numar} | {(c.Cerut ? "da" : "nu")} |");
        sb.AppendLine("\nTranzacții pe fel: " + string.Join(", ", feluri.OrderBy(f => f.Key).Select(f => $"{f.Key} {f.Value}")) + ".");
        sb.AppendLine("Postări pe spațiu × carte: " + string.Join(", ", postari.OrderBy(p => p.Spatiu).ThenBy(p => p.Carte).Select(p => $"{p.Spatiu}/{p.Carte} {p.Numar}")) + ".");
        sb.AppendLine("Documente pe tip × stare: " + string.Join(", ", documente.Select(d => $"{d.ClrType}/{d.Stare} {d.Numar}")) + ".");
        sb.AppendLine($"\n## (a)–(g): {randuri.Count} rânduri Δ, {msReconciliere:0} ms\n");
        if (randuri.Count > 0) sb.AppendLine("```\n" + ReconciliereCub.Raport(randuri) + "\n```");
        sb.AppendLine("Note și (h):\n");
        foreach (var n in note) sb.AppendLine("- " + n);
        sb.AppendLine($"\n## INV-CUB: {(refuz == null ? "verde" : "REFUZ — " + refuz)}, {msInvarianti:0} ms");
        sb.AppendLine($"\n## ASM-B7 (lot × gestiune × cont): {valori.Pozitii.Count} poziții, {reziduuri} cu cantitate zero și valoare nenulă, {msValori:0} ms\n");
        foreach (var l in valori.Linii()) sb.AppendLine("- " + l);
        File.WriteAllText(Path.Combine(director, $"xd3-{Profil}-m{istoric}.md"), sb.ToString());

        Console.WriteLine($"     MĂSURAT (X-D3 m{istoric}): {VolumFinal.Tranzactii} tranzacții, {VolumFinal.Postari} postări; reconciliere {msReconciliere:0} ms, "
            + $"INV-CUB {msInvarianti:0} ms, ASM-B7 {msValori:0} ms; {randuri.Count} rânduri Δ, {reziduuri} reziduuri.");
        foreach (var n in note) Console.WriteLine("     " + n);
        if (randuri.Count > 0) Console.WriteLine(ReconciliereCub.Raport(randuri));
        Verifica("X-D3", $"m={istoric}: reconcilierea integrală (a)–(g) pe baza de volum — 0 rânduri Δ", randuri.Count == 0);
        Verifica("X-D3", $"m={istoric}: INV-CUB pe baza de volum" + (refuz == null ? "" : " — " + refuz.Split('\n')[0]), refuz == null);
        Verifica("X-D3", $"m={istoric}: fiecare ramură are fapte"
            + (vide.Count == 0 ? "" : " — vide: " + string.Join("; ", vide.Select(v => $"{v.Ramura} {v.Fapt}"))), vide.Count == 0);
        Verifica("X-D3", $"m={istoric}: diagnosticul ASM-B7 rulat pe {valori.Pozitii.Count} poziții, {reziduuri} cu cantitate zero și valoare nenulă", valori.Pozitii.Count > 0);
    }

    // Criteriile din plan amânate nominal prin amendamentul owner-ului (X-RV6), cu restanța fiecăruia.
    static readonly Dictionary<string, string> Amanate = new() { ["PREST-NI"] = "F27-r16" };

    static string Mib(double octeti) => (octeti / 1048576.0).ToString("0.0", CultureInfo.InvariantCulture);

    // Criteriile de formă X-D5 (c) pe matricea operație × rută, pe toate treptele unui profil.
    public static void Evalueaza(IReadOnlyList<PerfCub> scene, Action<string, bool> check, string director, bool privat) {
        var profil = privat ? "privat" : "bugetar";
        var toate = scene.SelectMany(s => s.Masuri).ToList();
        var comenzi = scene.SelectMany(s => s.Comenzi).ToList();
        var istorice = toate.Select(m => m.Istoric).Distinct().Order().ToList();
        var sb = new StringBuilder();
        void Check(string text, bool ok) => check($"X-D5 ({profil}) {text}", ok);

        foreach (var op in Operatii.Where(o => toate.Any(m => m.Operatie == o.Cod))) {
            var ale = toate.Where(m => m.Operatie == op.Cod).ToList();
            Check($"{op.Cod} [{op.Ruta}]: nicio măsurare căzută", ale.All(m => m.Eroare == null));
            foreach (var faza in new[] { "rece", "cald" }) {
                // Reconstrucția lucrează per referință: fără nicio lună închisă (m = 0) nu are ce rescrie.
                var puncte = ale.Where(m => m.Faza == faza && (op.Ruta != Reconstructie || m.Istoric > 0))
                    .OrderBy(m => m.Istoric).ThenBy(m => m.Unitati).ToList();
                // Închiderea elimină referința precedentă numai dacă nu e decembrie: forma ei ține de luna din an, deci se compară în k.
                var constante = op.Ruta == Scriere
                    ? puncte.GroupBy(p => p.Istoric).All(g => g.Select(p => p.Comenzi).Distinct().Count() == 1)
                    : puncte.Select(p => p.Comenzi).Distinct().Count() == 1;
                Check($"{op.Cod} [{op.Ruta}, {faza}]: numărul de comenzi SQL nu depinde de k {(op.Ruta == Scriere ? "" : "și m ")}"
                    + $"([{string.Join(", ", puncte.Select(p => $"m{p.Istoric}k{p.Unitati}:{p.Comenzi}"))}])",
                    puncte.Count > 0 && constante);
            }
            var calde = ale.Where(m => m.Faza == "cald").ToList();
            foreach (var m in istorice) {
                var scara = calde.Where(p => p.Istoric == m).OrderBy(p => p.Unitati).ToList();
                if (scara.Count < 2) continue;
                var perechi = scara.Zip(scara.Skip(1)).ToList();
                bool Liniar(Func<Masura, double> f) => perechi.All(x => f(x.Second) <= 1.25 * f(x.First) * x.Second.Unitati / x.First.Unitati);
                Check($"{op.Cod} [{op.Ruta}, cald, m={m}]: durata și alocările cresc cel mult liniar în k, toleranță 25% pe pas "
                    + $"([{string.Join(", ", scara.Select(p => $"k{p.Unitati}: {p.Ms:0.0} ms / {Mib(p.Alocati)} MiB"))}])",
                    perechi.Count > 0 && Liniar(p => p.Ms) && Liniar(p => p.Alocati));
            }
            var kMaxim = calde.Count == 0 ? 0 : calde.Max(p => p.Unitati);
            var planuri = calde.Where(p => p.Unitati == kMaxim).OrderBy(p => p.Istoric).ToList();
            if (planuri.Count > 0)
                Check($"{op.Cod} [{op.Ruta}, plan la k={kMaxim}]: fiecare citire a operației are planul ei, în ambele treceri "
                    + $"([{string.Join(", ", planuri.Select(p => $"m{p.Istoric}: {p.PlanuriCerute - p.PlanuriRespinse}/{p.PlanuriCerute}"))}])",
                    planuri.All(p => p.PlanuriCerute > 0 && p.PlanuriRespinse == 0));
            var descriere = string.Join(", ", planuri.Select(p => $"m{p.Istoric}: {p.ScanariPostare} scanări / {p.RanduriPostare:0} rânduri / {p.BuffersPostare} buffers / snapshot {p.RanduriSnapshot:0}"));
            var peIndex = string.Join(", ", planuri.Select(p => $"m{p.Istoric}: {p.ScanariIndex} scanări ({p.SecventialeIndex} secvențiale) / {p.RanduriIndex:0} rânduri / {p.BuffersIndex} buffers"));
            if (planuri.Count > 0)
                Console.WriteLine($"     MĂSURAT (X-D5 {profil} {op.Cod} [{op.Ruta}], planul ales la k={kMaxim}): {descriere}");
            if (Amanate.TryGetValue(op.Cod, out var restanta) && planuri.Count > 1)
                Console.WriteLine($"     AMÂNAT (X-D5 {profil} {op.Cod} [{op.Ruta}], plan la k={kMaxim}, {restanta}): {peIndex}");
            else if (op.Ruta is Snapshot or Interval && planuri.Count > 1) {
                // Bufferele nu se compară între trepte: un Index Scan numără fiecare acces la pagină, un Bitmap Heap Scan paginile
                // distincte, iar planificatorul le alternează. Se cer mărginite de rândurile atinse (coborârea în index + pagina rândului).
                var baza = planuri[0];
                Check($"{op.Cod} [{op.Ruta}, plan la k={kMaxim}, fără scanare secvențială]: accesul la `Postare` nu depinde de m — nicio scanare "
                    + $"secvențială rămasă, rânduri atinse în toleranța de 25% față de m={baza.Istoric}, buffers mărginite de rânduri ([{peIndex}])",
                    planuri.All(p => p.PlanuriCerute > 0 && p.PlanuriRespinse == 0
                        && p.SecventialeIndex == 0 && p.RanduriIndex <= 1.25 * baza.RanduriIndex
                        && p.BuffersIndex <= 5 * p.RanduriIndex + 16 * p.ScanariIndex));
            }
            else if (planuri.Count > 0)
                Console.WriteLine($"     MĂSURAT (X-D5 {profil} {op.Cod} [{op.Ruta}], fără scanare secvențială la k={kMaxim}): {peIndex}");
        }

        // Comenzile lunare (AMO, închiderea) nu se repetă în k: forma lor e „nu cresc cu lunile deja închise”, de la a treia încolo.
        foreach (var tip in new[] { "AMO", "INCHIDERE" })
        foreach (var m in istorice) {
            var sir = comenzi.Where(c => c.Tip == tip && c.Istoric == m).Select(c => c.Comenzi).ToList();
            if (sir.Count < 4) continue;
            Check($"comanda {tip} (m={m}): numărul de comenzi SQL nu crește cu lunile deja închise ([{string.Join(", ", sir)}])",
                sir.Skip(2).Distinct().Count() == 1);
        }
        foreach (var tip in comenzi.Select(c => c.Tip).Where(t => t is not ("AMO" or "INCHIDERE")).Distinct().Order()) {
            var grupuri = comenzi.Where(c => c.Tip == tip).GroupBy(c => (c.Istoric, c.Treapta)).OrderBy(g => g.Key).ToList();
            var mediane = grupuri.Select(g => (g.Key, Sql: Mediana(g.Select(c => (double)c.Comenzi)))).ToList();
            Check($"comanda {tip}: numărul tipic de comenzi SQL nu depinde de k și m "
                + $"([{string.Join(", ", mediane.Select(x => $"m{x.Key.Istoric}k{x.Key.Treapta}:{x.Sql:0}"))}])",
                mediane.Select(x => x.Sql).Distinct().Count() == 1);
        }

        sb.AppendLine($"### Cititorii — {profil}\n");
        sb.AppendLine("| operație | rută | m | k | faza | ms | comenzi | ms SQL | rânduri citite | rânduri livrate | alocați MiB | vârf gestionat MiB | scanări Postare | rânduri Postare (plan) | buffers Postare | rânduri snapshot (plan) | server max ms | fără secvențial: scanări (secvențiale) | rânduri | buffers |");
        sb.AppendLine("|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|");
        foreach (var op in Operatii)
        foreach (var m in toate.Where(m => m.Operatie == op.Cod).OrderBy(m => m.Istoric).ThenBy(m => m.Unitati).ThenByDescending(m => m.Faza)) {
            var plan = m.ScanariPostare > 0 || m.MsServerMax > 0;
            sb.AppendLine($"| {op.Cod} | {op.Ruta} | {m.Istoric} | {m.Unitati} | {m.Faza} | {m.Ms.ToString("0.0", CultureInfo.InvariantCulture)} | {m.Comenzi} | "
                + $"{m.MsSql.ToString("0.0", CultureInfo.InvariantCulture)} | {m.Randuri} | {m.Cardinal} | {Mib(m.Alocati)} | {Mib(m.VarfGestionat)} | "
                + (plan ? $"{m.ScanariPostare} | {m.RanduriPostare:0} | {m.BuffersPostare} | {m.RanduriSnapshot:0} | {m.MsServerMax.ToString("0.0", CultureInfo.InvariantCulture)} | "
                    + $"{m.ScanariIndex} ({m.SecventialeIndex}) | {m.RanduriIndex:0} | {m.BuffersIndex} |" : " | | | | | | | |"));
        }
        sb.AppendLine($"\n### Comenzile de scriere — {profil} (durata sub blocajul scrierii)\n");
        sb.AppendLine("| comandă | m | treapta k | n | ms median | ms maxim | comenzi SQL median | comenzi SQL maxim | ms SQL median |");
        sb.AppendLine("|---|---|---|---|---|---|---|---|---|");
        foreach (var g in comenzi.GroupBy(c => (c.Tip, c.Istoric, c.Treapta)).OrderBy(g => g.Key.Tip).ThenBy(g => g.Key.Istoric).ThenBy(g => g.Key.Treapta))
            sb.AppendLine($"| {g.Key.Tip} | {g.Key.Istoric} | {g.Key.Treapta} | {g.Count()} | {Mediana(g.Select(c => c.Ms)).ToString("0.0", CultureInfo.InvariantCulture)} | "
                + $"{g.Max(c => c.Ms).ToString("0.0", CultureInfo.InvariantCulture)} | {Mediana(g.Select(c => (double)c.Comenzi)):0} | {g.Max(c => c.Comenzi)} | "
                + $"{Mediana(g.Select(c => c.MsSql)).ToString("0.0", CultureInfo.InvariantCulture)} |");
        sb.AppendLine($"\n### Baza de volum și X-D3 — {profil}\n");
        sb.AppendLine("| m | tranzacții | postări | documente | reconciliere ms | INV-CUB ms | ASM-B7 ms | rânduri Δ (a)–(g) | reziduuri ASM-B7 | explicația documentului lung | explicații (număr / octeți / maxim) |");
        sb.AppendLine("|---|---|---|---|---|---|---|---|---|---|---|");
        foreach (var s in scene.Where(s => s.VolumFinal != null)) {
            var v = s.VolumFinal; var e = s.Explicatii;
            sb.AppendLine($"| {v.Istoric} | {v.Tranzactii} | {v.Postari} | {v.Documente} | {v.MsReconciliere:0} | {v.MsInvarianti:0} | {v.MsValoriStoc:0} | {v.Delta} | {v.Reziduuri} | "
                + $"{e.Linii} linii / {e.Octeti} octeți | {e.Tranzactii} / {e.Total} / {e.Maxim} |");
        }
        File.WriteAllText(Path.Combine(director, $"perf-cub-{profil}.md"), sb.ToString());
        File.WriteAllText(Path.Combine(director, $"perf-cub-{profil}.json"), JsonSerializer.Serialize(new { Masuri = toate, Comenzi = comenzi }));
        Console.WriteLine($"     PERFCUB tabel: {Path.Combine(director, $"perf-cub-{profil}.md")}");
    }

    static double Mediana(IEnumerable<double> valori) {
        var v = valori.Order().ToList();
        return v.Count == 0 ? 0 : v.Count % 2 == 1 ? v[v.Count / 2] : (v[v.Count / 2 - 1] + v[v.Count / 2]) / 2;
    }
}
