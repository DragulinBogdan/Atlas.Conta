using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Cub.Citiri;
using DevExpress.ExpressApp;

namespace Atlas.Conta.BackOffice.Module.Proiectii;

/// <summary>Puntea D300 → D394 pe sens × tip de TVA × cotă (123 c): D300 − doar D300 + doar D394 = declarat + exclus + neinclus.</summary>
public sealed class D394Punte {
    public string SensTva { get; set; }
    public Guid TipTvaId { get; set; }
    public string TipTvaCod { get; set; }
    public string TipTvaDenumire { get; set; }
    public decimal Cota { get; set; }
    public decimal BazaD300 { get; set; }
    public decimal TvaD300 { get; set; }
    public decimal BazaDoarD300 { get; set; }
    public decimal TvaDoarD300 { get; set; }
    public decimal BazaDoarD394 { get; set; }
    public decimal TvaDoarD394 { get; set; }
    public decimal BazaExclusa { get; set; }
    public decimal TvaExclusa { get; set; }
    public decimal BazaNeinclusa { get; set; }
    public decimal TvaNeinclusa { get; set; }
    public decimal BazaDeclarata { get; set; }
    public decimal TvaDeclarata { get; set; }
    /// <summary>D300 − declarat: ce explică celelalte coloane.</summary>
    public decimal BazaDiferenta { get; set; }
    public decimal TvaDiferenta { get; set; }
    /// <summary>Documentele cu perioada D300 diferită de perioada D394, cel mult cinci.</summary>
    public List<string> DocumenteDecalate { get; set; } = [];
    public int NrDocumenteDecalate { get; set; }
}

public static partial class D394Proiectii {
    sealed class Punte {
        public enum Ramura { Declarat, Exclus, Neinclus }

        readonly Dictionary<(SensTva, Guid, decimal), D394Punte> randuri = [];

        D394Punte Rand(SensTva sens, Guid tipTva, decimal cota) {
            if (!randuri.TryGetValue((sens, tipTva, cota), out var r))
                r = randuri[(sens, tipTva, cota)] = new D394Punte { SensTva = sens.ToString(), TipTvaId = tipTva, Cota = cota };
            return r;
        }

        public void Adauga(SensTva sens, Guid tipTva, decimal cota, Ramura ramura, decimal baza, decimal tva) {
            var r = Rand(sens, tipTva, cota);
            switch (ramura) {
                case Ramura.Declarat: r.BazaDeclarata += baza; r.TvaDeclarata += tva; break;
                case Ramura.Exclus: r.BazaExclusa += baza; r.TvaExclusa += tva; break;
                default: r.BazaNeinclusa += baza; r.TvaNeinclusa += tva; break;
            }
        }

        public List<D394Punte> Inchide(IObjectSpace os, DateOnly dataStart, DateOnly dataEnd,
                Dictionary<Guid, (string Cod, string Denumire)> etichete) {
            int p0 = dataStart.Year * 100 + dataStart.Month, p1 = dataEnd.Year * 100 + dataEnd.Month;
            var fapte = Fiscale.Fapte(os)
                .Where(f => (f.PerioadaD300 >= p0 && f.PerioadaD300 <= p1) || (f.PerioadaD394 >= p0 && f.PerioadaD394 <= p1))
                .GroupBy(f => new { f.SensTva, f.TipTvaId, f.Cota, f.PerioadaD300, f.PerioadaD394, f.DocumentFiscalId })
                .Select(g => new {
                    g.Key.SensTva, g.Key.TipTvaId, g.Key.Cota, g.Key.PerioadaD300, g.Key.PerioadaD394, g.Key.DocumentFiscalId,
                    Baza = g.Sum(f => f.Baza), Tva = g.Sum(f => f.Tva),
                })
                .ToList();
            var decalate = new Dictionary<D394Punte, HashSet<Guid>>();
            foreach (var f in fapte) {
                bool inD300 = f.PerioadaD300 >= p0 && f.PerioadaD300 <= p1, inD394 = f.PerioadaD394 >= p0 && f.PerioadaD394 <= p1;
                var r = Rand(f.SensTva, f.TipTvaId, f.Cota);
                if (inD300) { r.BazaD300 += f.Baza; r.TvaD300 += f.Tva; }
                if (inD300 == inD394)
                    continue;
                if (inD300) { r.BazaDoarD300 += f.Baza; r.TvaDoarD300 += f.Tva; }
                else { r.BazaDoarD394 += f.Baza; r.TvaDoarD394 += f.Tva; }
                if (f.Baza != 0m || f.Tva != 0m)
                    (decalate.TryGetValue(r, out var docs) ? docs : decalate[r] = []).Add(f.DocumentFiscalId);
            }
            var ids = decalate.Values.SelectMany(d => d).Distinct().ToList();
            var numere = ids.Count == 0 ? [] : os.GetObjectsQuery<Document>().Where(d => ids.Contains(d.ID))
                .Select(d => new { d.ID, d.Numar, d.Data }).ToList().ToDictionary(d => d.ID);
            var lipsa = randuri.Keys.Select(k => k.Item2).Where(id => !etichete.ContainsKey(id)).Distinct().ToList();
            foreach (var t in os.GetObjectsQuery<TipTva>().Where(t => lipsa.Contains(t.ID)).Select(t => new { t.ID, t.Cod, t.Denumire }).ToList())
                etichete[t.ID] = (t.Cod, t.Denumire);
            foreach (var r in randuri.Values) {
                (r.TipTvaCod, r.TipTvaDenumire) = etichete.TryGetValue(r.TipTvaId, out var e) ? e : (null, null);
                r.BazaDiferenta = r.BazaD300 - r.BazaDeclarata;
                r.TvaDiferenta = r.TvaD300 - r.TvaDeclarata;
                if (decalate.TryGetValue(r, out var docs)) {
                    r.NrDocumenteDecalate = docs.Count;
                    r.DocumenteDecalate = [.. docs.Where(numere.ContainsKey).Select(id => numere[id])
                        .OrderBy(d => d.Data).ThenBy(d => d.Numar, StringComparer.Ordinal).Take(5)
                        .Select(d => $"{d.Numar ?? "fără număr"} din {d.Data:dd.MM.yyyy}")];
                }
            }
            return [.. randuri.Values.OrderBy(r => r.SensTva).ThenBy(r => r.TipTvaCod ?? "", StringComparer.Ordinal).ThenBy(r => r.Cota)];
        }
    }
}
