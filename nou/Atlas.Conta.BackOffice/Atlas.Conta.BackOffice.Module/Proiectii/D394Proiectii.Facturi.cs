using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Cub.Citiri;
using DevExpress.ExpressApp;

namespace Atlas.Conta.BackOffice.Module.Proiectii;

/// <summary>Rândul `serieFacturi`: 1 = plaja alocată, 2 = facturile emise din ea în perioadă (E2-D7).</summary>
public sealed class D394SerieFacturi {
    public int Tip { get; set; }
    public string Serie { get; set; }
    public long NrI { get; set; }
    public long NrF { get; set; }
}

public static partial class D394Proiectii {
    /// <summary>Plajele active, facturile emise din ele și facturile emise în afara oricărei plaje (E2-D7).</summary>
    static void Facturi(IObjectSpace os, D394Dto rezultat, DateOnly dataStart, DateOnly dataEnd) {
        var emise = Fiscale.IntreLuni(Fiscale.Fapte(os), dataStart, dataEnd, FormularFiscal.D394)
            .Where(r => r.SensTva == SensTva.Livrare && !r.Storno)
            .Select(r => r.DocumentFiscalId).Distinct().ToList();
        var numere = os.GetObjectsQuery<Document>().Where(d => emise.Contains(d.ID))
            .Select(d => new { d.ID, d.Numar }).ToList();
        if (numere.Count != emise.Count)
            rezultat.RefuzuriXml.Add(new RefuzDeclaratie {
                Cod = CoduriDeclaratie.DateNeincluse,
                Mesaj = $"{emise.Count - numere.Count} facturi emise în perioadă nu sunt vizibile utilizatorului; plaja nu se poate declara.",
            });
        var plaje = os.GetObjectsQuery<PlajaFacturi>().Where(p => p.Activ && p.DataDecizie <= dataEnd)
            .OrderBy(p => p.Serie).ThenBy(p => p.NumarInitial).ToList();
        rezultat.NrFacturi = numere.Count;
        var peplaja = plaje.ToDictionary(p => p.ID, _ => new List<long>());
        foreach (var f in numere.OrderBy(f => f.Numar, StringComparer.Ordinal)) {
            var (serie, numar) = Descompune(f.Numar);
            var plaja = numar is long n
                ? plaje.FirstOrDefault(p => PlajaFacturi.SerieNormalizata(p.Serie) == serie && p.NumarInitial <= n && n <= p.NumarFinal)
                : null;
            if (plaja == null)
                rezultat.RefuzuriXml.Add(new RefuzDeclaratie {
                    Cod = CoduriDeclaratie.DateNeincluse,
                    Mesaj = $"Factura emisă {f.Numar ?? "fără număr"} nu cade în nicio plajă de facturi activă.",
                });
            else
                peplaja[plaja.ID].Add(numar.Value);
        }
        foreach (var p in plaje) {
            rezultat.SeriiFacturi.Add(new D394SerieFacturi { Tip = 1, Serie = Curat(p.Serie), NrI = p.NumarInitial, NrF = p.NumarFinal });
            if (peplaja[p.ID] is { Count: > 0 } folosite)
                rezultat.SeriiFacturi.Add(new D394SerieFacturi { Tip = 2, Serie = Curat(p.Serie), NrI = folosite.Min(), NrF = folosite.Max() });
        }
    }

    /// <summary>Seria e prefixul numărului, numărul e cifrele lui finale.</summary>
    static (string Serie, long? Numar) Descompune(string numar) {
        if (string.IsNullOrWhiteSpace(numar))
            return (null, null);
        var t = numar.Trim();
        var i = t.Length;
        while (i > 0 && char.IsAsciiDigit(t[i - 1]))
            i--;
        return (PlajaFacturi.SerieNormalizata(t[..i]), i < t.Length && t.Length - i <= 18 ? long.Parse(t[i..]) : null);
    }

    static string Curat(string text) => string.IsNullOrWhiteSpace(text) ? null : text.Trim();
}
