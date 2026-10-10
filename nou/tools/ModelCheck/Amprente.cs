using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Proiectii;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.EFCore;
using Microsoft.EntityFrameworkCore;
using C = Atlas.Conta.BackOffice.Module.Cub;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.ModelCheck;

/// <summary>
/// `ModelCheck --amprente &lt;bază&gt; &lt;an&gt; &lt;lună&gt; &lt;director&gt;`: sumele unei baze pe chei
/// naturale, comparabile între două baze cu identificatori diferiți (C111-D9).
/// </summary>
static class Amprente {
    public static void Scrie(string connectionString, int an, int luna, string director) {
        Directory.CreateDirectory(director);
        using var provider = new EFCoreObjectSpaceProvider<BackOfficeEFCoreDbContext>((b, _) => b.UseNpgsql(connectionString)
            .UseChangeTrackingProxies().UseObjectSpaceLinkProxies().UseLazyLoadingProxies());
        using var os = provider.CreateObjectSpace();
        var deLa = new DateOnly(an, luna, 1);
        var panaLa = deLa.AddMonths(1).AddDays(-1);

        var rezumat = new List<string> {
            Sectiune(director, "balanta.txt", Balanta(os)),
            Sectiune(director, "stoc.txt", Stoc(os)),
            Sectiune(director, "partide.txt", Partide(os)),
            Sectiune(director, "d300.json", D300(os, deLa, panaLa)),
            Sectiune(director, "d394.json", D394(os, deLa, panaLa)),
            Sectiune(director, "analitic-repartitor.txt", AnaliticRepartitor(os)),
            Sectiune(director, "analitic-fiscal.txt", AnaliticFiscal(os)),
            Sectiune(director, "analitic-contrapondere.txt", AnaliticContrapondere(os)),
        };
        File.WriteAllLines(Path.Combine(director, "amprente.txt"), rezumat, new UTF8Encoding(false));
        foreach (var rand in rezumat)
            Console.WriteLine(rand);
    }

    static string Sectiune(string director, string fisier, IReadOnlyList<string> randuri) {
        var text = string.Join('\n', randuri) + "\n";
        File.WriteAllText(Path.Combine(director, fisier), text, new UTF8Encoding(false));
        return $"{fisier}|{randuri.Count}|{Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text)))}";
    }

    static List<string> Balanta(IObjectSpace os) {
        var conturi = os.GetObjectsQuery<Cont>().Select(c => new { c.ID, c.Simbol }).ToDictionary(c => c.ID, c => c.Simbol);
        return C.Citiri.Contabil.Postari(os)
            .GroupBy(p => new { p.Cont, p.Data.Year, p.Data.Month, p.Latura })
            .Select(g => new { g.Key.Cont, g.Key.Year, g.Key.Month, g.Key.Latura, Valoare = g.Sum(p => p.Valoare), Numar = g.Count() })
            .ToList()
            .GroupBy(r => (Simbol: conturi[r.Cont], r.Year, r.Month))
            .OrderBy(g => g.Key.Simbol, StringComparer.Ordinal).ThenBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
            .Select(g => string.Join('|', g.Key.Simbol, $"{g.Key.Year}-{g.Key.Month:00}",
                Bani(g.Where(r => r.Latura == N.Latura.Debit).Sum(r => r.Valoare)),
                Bani(g.Where(r => r.Latura == N.Latura.Credit).Sum(r => r.Valoare)),
                g.Sum(r => r.Numar)))
            .ToList();
    }

    static List<string> Stoc(IObjectSpace os) {
        var conturi = os.GetObjectsQuery<Cont>().Select(c => new { c.ID, c.Simbol }).ToDictionary(c => c.ID, c => c.Simbol);
        var produse = os.GetObjectsQuery<Produs>().Select(p => new { p.ID, p.Cod }).ToDictionary(p => p.ID, p => p.Cod);
        var gestiuni = os.GetObjectsQuery<Repartitor>().Select(r => new { r.ID, r.Cod }).ToDictionary(r => r.ID, r => r.Cod);
        return os.GetObjectsQuery<C.Postare>()
            .Where(p => p.Spatiu == N.Spatiu.Stoc)
            .GroupBy(p => new { p.Cont, p.Produs, p.Gestiune, p.Data.Year, p.Data.Month })
            .Select(g => new {
                g.Key.Cont, g.Key.Produs, g.Key.Gestiune, g.Key.Year, g.Key.Month,
                Cantitate = g.Sum(p => p.Cantitate),
                Valoare = g.Sum(p => p.Latura == N.Latura.Debit ? p.Valoare : -p.Valoare),
                Numar = g.Count(),
            })
            .ToList()
            .Select(r => string.Join('|', conturi[r.Cont], Cod(produse, r.Produs), Cod(gestiuni, r.Gestiune),
                $"{r.Year}-{r.Month:00}", r.Cantitate.ToString("0.000", CultureInfo.InvariantCulture), Bani(r.Valoare), r.Numar))
            .OrderBy(r => r, StringComparer.Ordinal)
            .ToList();
    }

    static List<string> Partide(IObjectSpace os) {
        var conturi = os.GetObjectsQuery<Cont>().Select(c => new { c.ID, c.Simbol }).ToDictionary(c => c.ID, c => c.Simbol);
        var parteneri = os.GetObjectsQuery<Repartitor>().Select(r => new { r.ID, r.Cod }).ToDictionary(r => r.ID, r => r.Cod);
        return os.GetObjectsQuery<C.Postare>()
            .Where(p => p.FelUnitate == N.FelUnitate.Partida)
            .GroupBy(p => new { p.Cont, p.Partener, p.Unitate, p.UnitateDeschisa })
            .Select(g => new {
                g.Key.Cont, g.Key.Partener, g.Key.UnitateDeschisa,
                Sold = g.Sum(p => p.Latura == N.Latura.Debit ? p.Valoare : -p.Valoare),
                Numar = g.Count(),
            })
            .ToList()
            .Select(r => string.Join('|', conturi[r.Cont], Cod(parteneri, r.Partener),
                r.UnitateDeschisa?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? "-", Bani(r.Sold), r.Numar))
            .OrderBy(r => r, StringComparer.Ordinal)
            .ToList();
    }

    // C111-D9, regula de oprire 2: pe forma de azi repartitorul de sold se derivă după C111-D5.
    static List<string> AnaliticRepartitor(IObjectSpace os) {
        var conturi = os.GetObjectsQuery<Cont>().Select(c => new { c.ID, c.Simbol }).ToDictionary(c => c.ID, c => c.Simbol);
        var repartitori = os.GetObjectsQuery<Repartitor>().Select(r => new { r.ID, r.Cod }).ToDictionary(r => r.ID, r => r.Cod);
        var virtuale = N.GestiuniVirtuale.Toate.ToArray();
        return C.Citiri.Contabil.Postari(os)
            .Select(p => new {
                p.Cont, p.Data.Year, p.Data.Month, p.Latura, p.Valoare,
                Repartitor = p.FelUnitate == N.FelUnitate.Partida ? p.Partener
                    : p.Gestiune != null && !virtuale.Contains(p.Gestiune.Value) ? p.Gestiune
                    : p.TipTvaId != null ? (Guid?)null
                    : p.Partener,
            })
            .GroupBy(p => new { p.Cont, p.Repartitor, p.Year, p.Month, p.Latura })
            .Select(g => new { g.Key.Cont, g.Key.Repartitor, g.Key.Year, g.Key.Month, g.Key.Latura, Valoare = g.Sum(p => p.Valoare), Numar = g.Count() })
            .ToList()
            .GroupBy(r => (Simbol: conturi[r.Cont], Repartitor: Cod(repartitori, r.Repartitor), r.Year, r.Month))
            .Select(g => string.Join('|', g.Key.Simbol, g.Key.Repartitor, $"{g.Key.Year}-{g.Key.Month:00}",
                Bani(g.Where(r => r.Latura == N.Latura.Debit).Sum(r => r.Valoare)),
                Bani(g.Where(r => r.Latura == N.Latura.Credit).Sum(r => r.Valoare)),
                g.Sum(r => r.Numar)))
            .OrderBy(r => r, StringComparer.Ordinal)
            .ToList();
    }

    static List<string> AnaliticFiscal(IObjectSpace os) {
        var conturi = os.GetObjectsQuery<Cont>().Select(c => new { c.ID, c.Simbol }).ToDictionary(c => c.ID, c => c.Simbol);
        var repartitori = os.GetObjectsQuery<Repartitor>().Select(r => new { r.ID, r.Cod }).ToDictionary(r => r.ID, r => r.Cod);
        var virtuale = N.GestiuniVirtuale.Toate.ToArray();
        return os.GetObjectsQuery<C.Postare>()
            .Where(p => p.TipTvaId != null)
            .Select(p => new {
                p.Cont, p.Data.Year, p.Data.Month, p.RolTva, p.Latura, p.Valoare, PartenerFiscal = p.Partener,
                LocFiscal = p.Gestiune != null && !virtuale.Contains(p.Gestiune.Value) ? p.Gestiune : null,
            })
            .GroupBy(p => new { p.Cont, p.PartenerFiscal, p.LocFiscal, p.RolTva, p.Year, p.Month })
            .Select(g => new {
                g.Key.Cont, g.Key.PartenerFiscal, g.Key.LocFiscal, g.Key.RolTva, g.Key.Year, g.Key.Month,
                Valoare = g.Sum(p => p.Latura == N.Latura.Debit ? p.Valoare : -p.Valoare), Numar = g.Count(),
            })
            .ToList()
            .Select(r => string.Join('|', conturi[r.Cont], Cod(repartitori, r.PartenerFiscal), Cod(repartitori, r.LocFiscal),
                r.RolTva, $"{r.Year}-{r.Month:00}", Bani(r.Valoare), r.Numar))
            .OrderBy(r => r, StringComparer.Ordinal)
            .ToList();
    }

    static List<string> AnaliticContrapondere(IObjectSpace os) {
        var conturi = os.GetObjectsQuery<Cont>().Select(c => new { c.ID, c.Simbol }).ToDictionary(c => c.ID, c => c.Simbol);
        var virtuale = N.GestiuniVirtuale.Toate.ToArray();
        string Marcaj(Guid gestiune) =>
            gestiune == N.GestiuniVirtuale.Furnizor || gestiune == N.GestiuniVirtuale.Client ? "Tert"
            : gestiune == N.GestiuniVirtuale.Consum ? "Consum"
            : gestiune == N.GestiuniVirtuale.Inventar ? "Inventar"
            : "Transformare";
        return os.GetObjectsQuery<C.Postare>()
            .Where(p => p.Gestiune != null && virtuale.Contains(p.Gestiune.Value))
            .GroupBy(p => new { p.Cont, p.Gestiune, p.Data.Year, p.Data.Month })
            .Select(g => new {
                g.Key.Cont, g.Key.Gestiune, g.Key.Year, g.Key.Month, Cantitate = g.Sum(p => p.Cantitate),
                Valoare = g.Sum(p => p.Latura == N.Latura.Debit ? p.Valoare : -p.Valoare), Numar = g.Count(),
            })
            .ToList()
            .GroupBy(r => (Simbol: conturi[r.Cont], Marcaj: Marcaj(r.Gestiune.Value), r.Year, r.Month))
            .Select(g => string.Join('|', g.Key.Simbol, g.Key.Marcaj, $"{g.Key.Year}-{g.Key.Month:00}",
                g.Sum(r => r.Cantitate).ToString("0.000", CultureInfo.InvariantCulture), Bani(g.Sum(r => r.Valoare)), g.Sum(r => r.Numar)))
            .OrderBy(r => r, StringComparer.Ordinal)
            .ToList();
    }

    static List<string> D300(IObjectSpace os, DateOnly deLa, DateOnly panaLa) {
        var d300 = D300Proiectii.D300(os, deLa, panaLa, null);
        return [
            .. Linii("rand", d300.Randuri),
            .. Linii("nemapat", d300.Nemapate.Select(n => new { n.Sens, n.TipTvaCod, n.Regim, n.Cota, n.Baza, n.Tva, n.Randuri })),
            .. Linii("avertisment", d300.Avertismente),
        ];
    }

    static List<string> D394(IObjectSpace os, DateOnly deLa, DateOnly panaLa) {
        var d394 = D394Proiectii.D394(os, deLa, panaLa);
        return [
            .. Linii("operatiune", d394.Operatiuni),
            .. Linii("rezumat", d394.Rezumat),
            .. Linii("cota", d394.RezumatCote),
            .. Linii("neinclus", d394.Neincluse.Select(n =>
                new { n.Cauza, n.Sens, n.TipTvaCod, n.Cota, n.RepartitorDenumire, n.Baza, n.Tva, n.Randuri })),
            .. Linii("avertisment", d394.Avertismente.Select(a => new { a.Cod, a.Numar, a.Suma })),
            .. Linii("cui", new[] { d394.NrCui1, d394.NrCui2, d394.NrCui3, d394.NrCui4 }),
        ];
    }

    static IEnumerable<string> Linii<T>(string fel, IEnumerable<T> elemente) =>
        elemente.Select(e => $"{fel}|{JsonSerializer.Serialize(e)}").OrderBy(r => r, StringComparer.Ordinal);

    static string Cod(IReadOnlyDictionary<Guid, string> coduri, Guid? id) =>
        id is Guid cheie ? coduri.GetValueOrDefault(cheie) ?? "?" : "-";

    static string Bani(decimal valoare) => valoare.ToString("0.00", CultureInfo.InvariantCulture);
}
