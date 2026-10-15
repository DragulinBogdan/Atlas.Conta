using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Cub.Citiri;
using DevExpress.ExpressApp;

namespace Atlas.Conta.BackOffice.Module.Proiectii;

/// <summary>Defalcarea unei operațiuni V/C pe categoria de bunuri (`op11`, E2-D6).</summary>
public sealed class D394Bun {
    public int CodPR { get; set; }
    public int NrFact { get; set; }
    public decimal Baza { get; set; }
    public decimal? Tva { get; set; }
}

/// <summary>Cât diferă suma rândurilor rotunjite pe partener de totalul rotunjit o dată, cum îl rotunjește D300.</summary>
public sealed class D394DiferentaRotunjire {
    public string Tip { get; set; }
    public int Cota { get; set; }
    public int Randuri { get; set; }
    public decimal Baza { get; set; }
    public decimal Tva { get; set; }
}

public static partial class D394Proiectii {
    sealed record LinieVc(TipOperatiuneD394 Tip, decimal Baza);

    /// <summary>Bunurile pe fiecare rând V/C și liniile V/C fără categorie (E2-D6).</summary>
    static (Dictionary<Rand, List<D394Bun>> PeRand, List<LinieVc> FaraCategorie) Bunuri(IObjectSpace os,
            DateOnly dataStart, DateOnly dataEnd, Dictionary<(Guid, SensTva), TipOperatiuneD394> mapari,
            Dictionary<Guid, InfoPartener> parteneri, HashSet<(Guid, bool, string, TipOperatiuneD394, int)> castigatoare,
            Dictionary<(string, TipOperatiuneD394, SensTva, int), Rand> randuri) {
        var vc = mapari.Where(m => m.Value is TipOperatiuneD394.V or TipOperatiuneD394.C).Select(m => m.Key).ToHashSet();
        var peRand = new Dictionary<Rand, List<D394Bun>>();
        var faraCategorie = new List<LinieVc>();
        if (vc.Count == 0)
            return (peRand, faraCategorie);
        var linii = Fiscale.IntreLuni(Fiscale.Fapte(os), dataStart, dataEnd, FormularFiscal.D394)
            .Select(r => new {
                r.DetaliuId, r.DocumentFiscalId, Storno = r.Storno && !r.InversaTehnica, r.PartenerId,
                r.TipTvaId, r.SensTva, r.Cota, r.Baza, r.Tva,
            })
            .ToList()
            .Where(r => vc.Contains((r.TipTvaId, r.SensTva)) && (r.Baza != 0m || r.Tva != 0m))
            .ToList();
        var categorii = Categorii(os, linii.Select(l => l.DetaliuId).Distinct().ToList());

        var grupuri = new Dictionary<Rand, List<(Guid Doc, bool Storno, int Cod, decimal Baza, decimal Tva)>>();
        foreach (var l in linii) {
            var tip = mapari[(l.TipTvaId, l.SensTva)];
            if (!categorii.TryGetValue(l.DetaliuId, out var cod)) {
                faraCategorie.Add(new LinieVc(tip, l.Baza));
                continue;
            }
            if (l.PartenerId is not Guid pid || !parteneri.TryGetValue(pid, out var partener))
                continue;
            if (!randuri.TryGetValue((partener.CheieCui, tip, l.SensTva, CotaDeclarata(tip, l.Cota).Cota), out var rand))
                continue;
            if (!grupuri.TryGetValue(rand, out var lista))
                grupuri[rand] = lista = [];
            lista.Add((l.DocumentFiscalId, l.Storno, cod, l.Baza, l.Tva));
        }
        foreach (var (rand, lista) in grupuri) {
            var bunuri = lista.GroupBy(x => x.Cod).OrderBy(g => g.Key)
                .ToDictionary(g => g.Key, g => new D394Bun {
                    CodPR = g.Key, Baza = g.Sum(x => x.Baza), Tva = rand.Tip == TipOperatiuneD394.C ? g.Sum(x => x.Tva) : null,
                });
            foreach (var factura in lista.GroupBy(x => (x.Doc, x.Storno))) {
                if (!castigatoare.Contains((factura.Key.Doc, factura.Key.Storno, rand.CheieCui, rand.Tip, rand.Cota)))
                    continue;
                var castigator = factura.GroupBy(x => x.Cod)
                    .OrderByDescending(g => Math.Abs(rand.Tip == TipOperatiuneD394.C ? g.Sum(x => x.Tva) : g.Sum(x => x.Baza)))
                    .ThenBy(g => g.Key).First().Key;
                bunuri[castigator].NrFact++;
            }
            peRand[rand] = bunuri.Values.ToList();
        }
        return (peRand, faraCategorie);
    }

    static Dictionary<Guid, int> Categorii(IObjectSpace os, List<Guid> ids) {
        var rezultat = new Dictionary<Guid, int>();
        var peLot = os.GetObjectsQuery<DocumentDetaliu>().Where(d => ids.Contains(d.ID) && d.LotId != null)
            .Select(d => new { d.ID, d.Lot.Produs.CategorieTaxareInversa }).ToList();
        var peFct = os.GetObjectsQuery<FacturaIntrareDetaliu>().Where(d => ids.Contains(d.ID))
            .Select(d => new { d.ID, d.Produs.CategorieTaxareInversa }).ToList();
        var peFcl = os.GetObjectsQuery<FacturaIesireDetaliu>().Where(d => ids.Contains(d.ID))
            .Select(d => new { d.ID, d.Produs.CategorieTaxareInversa }).ToList();
        foreach (var x in peLot.Concat(peFct).Concat(peFcl))
            if (x.CategorieTaxareInversa is { } c)
                rezultat.TryAdd(x.ID, (int)c);
        return rezultat;
    }

    /// <summary>Lei întregi pe rând; bunurile rândului își împart rândul rotunjit (122 b).</summary>
    static void Rotunjeste(IEnumerable<Rand> randuri, Dictionary<Rand, List<D394Bun>> bunuri) {
        foreach (var rand in randuri) {
            rand.TvaInainteDeRotunjire = rand.Tva;
            rand.Baza = RotunjireDeclaratie.Lei(rand.Baza);
            rand.Tva = RotunjireDeclaratie.Lei(rand.Tva);
            if (!bunuri.TryGetValue(rand, out var lista) || lista.Count == 0)
                continue;
            var baze = RotunjireDeclaratie.Lei(rand.Baza, [.. lista.Select(b => b.Baza)]);
            var tva = RotunjireDeclaratie.Lei(rand.Tva, [.. lista.Select(b => b.Tva ?? 0m)]);
            for (var i = 0; i < lista.Count; i++) {
                lista[i].Baza = baze[i];
                lista[i].Tva = lista[i].Tva == null ? null : tva[i];
            }
        }
    }

    /// <summary>Rotunjirea pe partener față de rotunjirea totalului, pe tip × cotă (122 b).</summary>
    static List<D394DiferentaRotunjire> DiferenteRotunjire(IEnumerable<Rand> randuri) =>
        [.. randuri.GroupBy(r => (r.Tip, r.Cota)).OrderBy(g => g.Key.Tip).ThenBy(g => g.Key.Cota)
            .Select(g => new D394DiferentaRotunjire {
                Tip = g.Key.Tip.ToString(), Cota = g.Key.Cota, Randuri = g.Count(),
                Baza = g.Sum(r => RotunjireDeclaratie.Lei(r.Baza)) - RotunjireDeclaratie.Lei(g.Sum(r => r.Baza)),
                Tva = AreTva(g.Key.Tip)
                    ? g.Sum(r => RotunjireDeclaratie.Lei(r.Tva)) - RotunjireDeclaratie.Lei(g.Sum(r => r.Tva)) : 0m,
            })
            .Where(d => d.Baza != 0m || d.Tva != 0m)];
}
