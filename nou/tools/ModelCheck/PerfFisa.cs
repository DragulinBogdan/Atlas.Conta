using System.Diagnostics;
using System.Text;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Proiectii;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.EFCore;
using Microsoft.EntityFrameworkCore;
using C = Atlas.Conta.BackOffice.Module.Cub;

namespace Atlas.Conta.BackOffice.ModelCheck;

/// <summary>
/// `ModelCheck --perf-fisa &lt;bază&gt; &lt;simbol&gt; &lt;an&gt; &lt;lună&gt; &lt;director&gt;`: timpul fișei contului pe o bază
/// numită, pe contul întreg, pe repartitorul lui cel mai încărcat și fără terț, și al registrului jurnal al lunii, cu SQL-ul produsului scris în director (C111 S9).
/// </summary>
static class PerfFisa {
    const int Rulari = 5;

    public static void Masoara(string connectionString, string simbol, int an, int luna, string director) {
        Directory.CreateDirectory(director);
        using var provider = new EFCoreObjectSpaceProvider<BackOfficeEFCoreDbContext>((b, _) => b.UseNpgsql(connectionString)
            .UseChangeTrackingProxies().UseObjectSpaceLinkProxies().UseLazyLoadingProxies());
        using var os = provider.CreateObjectSpace();
        var cont = os.GetObjectsQuery<Cont>().Where(c => c.Simbol == simbol).Select(c => c.ID).Single();
        var deLa = new DateOnly(an, luna, 1);
        var panaLa = deLa.AddMonths(1).AddDays(-1);
        var repartitor = C.Citiri.Contabil.Analitice(os)
            .Where(p => p.Cont == cont && p.Data >= deLa && p.Data <= panaLa && p.Repartitor != null)
            .GroupBy(p => p.Repartitor).OrderByDescending(g => g.Count()).Select(g => g.Key).First();

        Forma(os, "cont-intreg", director, () => ContabilProiectii.FisaCont(os, cont, deLa, panaLa), CuPereche);
        Forma(os, "cont-repartitor", director, () => ContabilProiectii.FisaCont(os, cont, deLa, panaLa, repartitorId: repartitor), CuPereche);
        Forma(os, "cont-fara-tert", director, () => ContabilProiectii.FisaCont(os, cont, deLa, panaLa, faraTert: true), CuPereche);
        Forma(os, "jurnal", director, () => ContabilProiectii.RegistruJurnal(os, deLa, panaLa), r => r.ContrapartidaId != null);
    }

    static bool CuPereche(FisaContRand rand) => rand.ContrapartidaId != null;

    static void Forma<T>(IObjectSpace os, string nume, string director, Func<IQueryable<T>> interogare, Func<T, bool> cuPereche) {
        File.WriteAllText(Path.Combine(director, nume + ".sql"), interogare().ToQueryString(), new UTF8Encoding(false));
        var randuri = interogare().ToList();
        var timpi = new List<double>(Rulari);
        for (var i = 0; i < Rulari; i++) {
            ((EFCoreObjectSpace)os).DbContext.ChangeTracker.Clear();
            var ceas = Stopwatch.StartNew();
            interogare().ToList();
            timpi.Add(ceas.Elapsed.TotalMilliseconds);
        }
        timpi.Sort();
        Console.WriteLine($"{nume}|randuri={randuri.Count}|cu contrapartida={randuri.Count(cuPereche)}"
            + $"|mediana_ms={timpi[Rulari / 2]:0}|min_ms={timpi[0]:0}|max_ms={timpi[^1]:0}");
    }
}
