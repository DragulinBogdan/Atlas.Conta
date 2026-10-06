using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Motor;
using DevExpress.ExpressApp;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.Module.Cub.Citiri;

public static partial class Imobilizari {
    public static void VerificaAcoperire(IObjectSpace os) {
        var lipsuri = os.GetObjectsQuery<RegistruImobilizari>().Where(r =>
            !os.GetObjectsQuery<Postare>().Any(p => p.FelUnitate == N.FelUnitate.Fisa
                && p.Unitate == r.ImobilizareId && p.DocumentId == r.DocumentId
                && (p.Tranzactie.Fel == N.FelTranzactie.Storno) == r.Storno))
            .Select(r => r.DocumentId).Distinct().Take(10).ToList();
        if (lipsuri.Count > 0)
            throw new OperareException("Registrul imobilizărilor fără fișă pe cub: "
                + string.Join(", ", lipsuri));
        var incomplete = os.GetObjectsQuery<Postare>().Where(p => p.FelUnitate == N.FelUnitate.Fisa
            && (p.DocumentId == null || p.LinieId == null
                || (p.Tranzactie.Fel == N.FelTranzactie.Storno && p.InversaDinId == null)
                || (p.Tranzactie.Fel == N.FelTranzactie.Transfer && p.Valoare != 0m && p.SuportId == null)))
            .Select(p => p.ID).Take(10).ToList();
        if (incomplete.Count > 0)
            throw new OperareException("Fișe pe cub fără cauză, suport sau origine a inversei: " + string.Join(", ", incomplete));
        var registru = os.GetObjectsQuery<RegistruImobilizari>()
            .Select(r => new { r.DocumentId, r.ImobilizareId, r.Data, r.Storno,
                r.Valoare, r.ValoareFiscala, r.Amortizare, r.AmortizareFiscala }).ToList();
        if (registru.Count == 0) return;
        var cub = Randuri(os, registru.Select(r => r.ImobilizareId).Distinct().ToList(), DateOnly.MaxValue)
            .ToDictionary(r => (r.DocumentId, r.Rand.ImobilizareId, r.Rand.Data, r.Rand.Storno), r => r.Rand);
        var diferite = registru.GroupBy(r => (r.DocumentId, r.ImobilizareId, r.Data, r.Storno)).Where(g =>
            !cub.TryGetValue(g.Key, out var c) || c.Valoare != g.Sum(r => r.Valoare)
                || c.ValoareFiscala != g.Sum(r => r.ValoareFiscala) || c.Amortizare != g.Sum(r => r.Amortizare)
                || c.AmortizareFiscala != g.Sum(r => r.AmortizareFiscala)).Select(g => g.Key.DocumentId).Distinct().Take(10).ToList();
        if (diferite.Count > 0)
            throw new OperareException("Registrul imobilizărilor diferă de cub: " + string.Join(", ", diferite));
    }

    /// <summary>Fișa e unitatea a cel puțin unei postări, pe orice fel de tranzacție.</summary>
    public static bool AreMiscari(IObjectSpace os, Guid fisa) =>
        os.GetObjectsQuery<Postare>().Any(p => p.FelUnitate == N.FelUnitate.Fisa && p.Unitate == fisa);

    public sealed record RandCuDocument(RandImobilizare Rand, Guid DocumentId);
    sealed record Atribute(FelMiscareImobilizare Fel, decimal Deductibil, int Luni,
        MetodaAmortizare? Metoda = null, int? Durata = null, decimal? Reziduala = null,
        MetodaAmortizare? MetodaFiscala = null, int? DurataFiscala = null,
        CategorieFiscala? Categorie = null, bool? Exclusiva = null);

    public static List<RandCuDocument> Randuri(IObjectSpace os, IReadOnlyList<Guid> fise, DateOnly panaLa) {
        if (fise.Count == 0) return [];
        var sume = os.GetObjectsQuery<Postare>()
            .Where(p => p.FelUnitate == N.FelUnitate.Fisa && p.Unitate != null && fise.Contains(p.Unitate.Value)
                && p.Data <= panaLa && p.DocumentId != null && p.LinieId != null)
            .GroupBy(p => new { Fisa = p.Unitate.Value, Document = p.DocumentId.Value, Linie = p.LinieId.Value,
                p.Data, Storno = p.Tranzactie.Fel == N.FelTranzactie.Storno, p.Carte, p.Latura })
            .Select(g => new { g.Key, Id = g.Min(p => p.ID.ToString()), Valoare = g.Sum(p => p.Valoare) }).ToList();
        if (sume.Count == 0) return [];
        var ids = sume.Select(r => r.Key.Linie).Distinct().ToList();
        var atribute = new Dictionary<Guid, Atribute>();
        foreach (var p in os.GetObjectsQuery<PunereInFunctiuneDetaliu>().Where(l => ids.Contains(l.ID))
                .Select(l => new { l.ID, l.Fel, l.AmortizareFiscalaInitiala, l.LuniAmortizateInitial,
                    l.Metoda, l.DurataLuni, l.ValoareReziduala, l.MetodaFiscala, l.DurataFiscalaLuni,
                    l.CategorieFiscala, l.UtilizareExclusiva }).ToList())
            atribute[p.ID] = new(p.Fel switch {
                FelLiniePif.Intrare => FelMiscareImobilizare.Intrare,
                FelLiniePif.Modernizare => FelMiscareImobilizare.Modernizare,
                _ => FelMiscareImobilizare.Revizuire,
            }, p.AmortizareFiscalaInitiala, p.LuniAmortizateInitial, p.Metoda, p.DurataLuni,
                p.ValoareReziduala, p.MetodaFiscala, p.DurataFiscalaLuni, p.CategorieFiscala, p.UtilizareExclusiva);
        foreach (var a in os.GetObjectsQuery<AmortizareLunaraDetaliu>().Where(l => ids.Contains(l.ID))
                .Select(l => new { l.ID, l.ValoareDeductibila, l.Luni }).ToList())
            atribute[a.ID] = new(FelMiscareImobilizare.Amortizare, a.ValoareDeductibila, a.Luni);
        foreach (var c in os.GetObjectsQuery<IesireImobilizareDetaliu>().Where(l => ids.Contains(l.ID))
                .Select(l => l.ID).ToList())
            atribute[c] = new(FelMiscareImobilizare.Iesire, 0m, 0);

        var randuri = new List<RandCuDocument>();
        var deductibile = new Dictionary<Guid, decimal>();
        var iesite = new Dictionary<(Guid Fisa, Guid Document), decimal>();
        foreach (var g in sume.GroupBy(p => new { p.Key.Fisa, p.Key.Document, p.Key.Data, p.Key.Storno })
                .OrderBy(g => g.Key.Data).ThenBy(g => g.Key.Storno).ThenBy(g => g.Key.Document)) {
            var linie = g.Min(p => p.Key.Linie);
            if (!atribute.TryGetValue(linie, out var a))
                throw new OperareException($"Fișa {g.Key.Fisa} are postări fără eveniment identificabil ({linie}).");
            var semn = g.Key.Storno ? -1 : 1;
            decimal Suma(N.Carte carte, N.Latura latura) =>
                g.Where(p => p.Key.Carte == carte && p.Key.Latura == latura).Sum(p => p.Valoare);
            var iesire = a.Fel == FelMiscareImobilizare.Iesire;
            var amortizare = a.Fel == FelMiscareImobilizare.Amortizare;
            var brut = amortizare ? 0m : (iesire ? -Suma(N.Carte.Contabil, N.Latura.Credit) : Suma(N.Carte.Contabil, N.Latura.Debit));
            var brutFiscal = amortizare ? 0m : (iesire ? -Suma(N.Carte.Fiscal, N.Latura.Credit) : Suma(N.Carte.Fiscal, N.Latura.Debit));
            var cumulat = iesire ? -Suma(N.Carte.Contabil, N.Latura.Debit) : Suma(N.Carte.Contabil, N.Latura.Credit);
            var cumulatFiscal = iesire ? -Suma(N.Carte.Fiscal, N.Latura.Debit) : Suma(N.Carte.Fiscal, N.Latura.Credit);
            var deductibil = semn * a.Deductibil;
            if (iesire) {
                var cheie = (g.Key.Fisa, g.Key.Document);
                if (!g.Key.Storno) iesite[cheie] = deductibile.GetValueOrDefault(g.Key.Fisa);
                deductibil = -semn * iesite.GetValueOrDefault(cheie);
            }
            deductibile[g.Key.Fisa] = deductibile.GetValueOrDefault(g.Key.Fisa) + deductibil;
            randuri.Add(new(new(Guid.Parse(g.Min(p => p.Id)), g.Key.Fisa, g.Key.Data, a.Fel, g.Key.Storno, linie,
                brut, brutFiscal, cumulat, cumulatFiscal, deductibil, semn * a.Luni,
                a.Metoda, a.Durata, a.Reziduala, a.MetodaFiscala, a.DurataFiscala, a.Categorie, a.Exclusiva),
                g.Key.Document));
        }
        return randuri;
    }
}
