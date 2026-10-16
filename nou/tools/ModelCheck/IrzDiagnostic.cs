using Atlas.Conta.BackOffice.Module.Api.Irz;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Motor;
using DevExpress.ExpressApp.EFCore;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Conta.BackOffice.ModelCheck;

/// <summary>`ModelCheck --irz <bază> <an> <lună>`: ce ar închide IRZ pe lună și constatarea REZ-NEINCHIS, fără scrieri (E3).</summary>
static class IrzDiagnostic {
    public static int Scrie(string connectionString, int an, int luna) {
        using var provider = new EFCoreObjectSpaceProvider<BackOfficeEFCoreDbContext>((b, _) => b.UseNpgsql(connectionString)
            .UseChangeTrackingProxies().UseObjectSpaceLinkProxies().UseLazyLoadingProxies());
        using var os = provider.CreateObjectSpace();
        var r = InchidereRezultatApply.Previzualizeaza(os, an, luna);
        Console.WriteLine($"IRZ {luna:00}/{an}: motiv {r.Motiv ?? "(se generează)"}; {r.Linii.Count} linii, {r.Linii.Sum(l => l.Valoare):0.00} lei.");
        foreach (var g in r.Linii.GroupBy(l => (l.ContSursaSimbol, l.ContDestinatieSimbol)).OrderBy(g => g.Key))
            Console.WriteLine($"  {g.Key.ContSursaSimbol} → {g.Key.ContDestinatieSimbol}: {g.Count()} chei, "
                + $"debit {g.Where(l => l.SursaPeDebit).Sum(l => l.Valoare):0.00} / credit {g.Where(l => !l.SursaPeDebit).Sum(l => l.Valoare):0.00}");
        var rez = PerioadaService.Verifica(os, an, luna).Where(c => c.Fel == nameof(FelConstatareInchidere.RezNeinchis)).ToList();
        Console.WriteLine(rez.Count == 0 ? "REZ-NEINCHIS: absentă." : $"REZ-NEINCHIS: {rez[0].Severitate} — {rez[0].Text}");
        return r.Linii.Count == 0 ? 0 : 1;
    }
}
