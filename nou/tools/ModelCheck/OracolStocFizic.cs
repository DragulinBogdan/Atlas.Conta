using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Saft;
using DevExpress.ExpressApp;
using C = Atlas.Conta.BackOffice.Module.Cub;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.ModelCheck;

// D18-V1: PhysicalStock recalculat naiv din postările cubului. Domeniul (conturi, chei) vine din postări și din
// `Cont.CategorieStoc` urcată pe părinți, niciodată din fișierul verificat (B8-RV2); cheia e așteptată dacă are
// sold la un capăt sau postări în lună (S3-D4).
static class OracolStocFizic {
    public sealed record Rezultat(int Postari, int Conturi, int Intrari, int Diferite, int LipsaDinFisier, int StraineInFisier,
        IReadOnlyList<string> ConturiLipsa) {
        public bool Ok => Postari > 0 && Intrari > 0 && Diferite == 0 && LipsaDinFisier == 0 && StraineInFisier == 0;
        public override string ToString() => $"{Postari} postări pe lot ≤ capăt pe {Conturi} conturi raportabile; {Intrari} intrări "
            + $"de stoc fizic, {Diferite} diferite, {LipsaDinFisier} chei așteptate lipsă din fișier, {StraineInFisier} chei din fișier "
            + $"neașteptate de oracol, conturi lipsă: [{string.Join(", ", ConturiLipsa)}]";
    }

    public static Rezultat Compara(IObjectSpace os, SaftDto saft, DateOnly pStart, DateOnly pEnd) {
        var conturi = os.GetObjectsQuery<Cont>().Select(c => new { c.ID, c.ParinteId, c.CategorieStoc, c.Simbol }).ToList()
            .ToDictionary(c => c.ID);
        bool Raportabil(Guid cont) {
            for (var (c, pas) = ((Guid?)cont, 0); c is Guid id && pas < 64 && conturi.TryGetValue(id, out var info); c = info.ParinteId, pas++)
                if (info.CategorieStoc is TipStoc t) return t is TipStoc.Magazie or TipStoc.Marfuri;
            return false;
        }
        var postari = os.GetObjectsQuery<C.Postare>()
            .Where(p => p.Unitate != null && p.Gestiune != null && p.Carte == N.Carte.Contabil && p.Data <= pEnd)
            .Select(p => new { Lot = p.Unitate.Value, Gestiune = p.Gestiune.Value, p.Cont, p.Data, p.Cantitate,
                Deschidere = p.Tranzactie.Fel == N.FelTranzactie.Deschidere,
                Valoare = p.Latura == N.Latura.Debit ? p.Valoare : -p.Valoare })
            .ToList()
            .Where(p => Raportabil(p.Cont)).ToList();
        var deschidere = postari.Where(p => p.Data < pStart || p.Deschidere)
            .GroupBy(p => (p.Gestiune, p.Lot, p.Cont)).ToDictionary(g => g.Key, g => (g.Sum(p => p.Cantitate), g.Sum(p => p.Valoare)));
        var inchidere = postari
            .GroupBy(p => (p.Gestiune, p.Lot, p.Cont)).ToDictionary(g => g.Key, g => (g.Sum(p => p.Cantitate), g.Sum(p => p.Valoare)));
        var asteptate = inchidere.Keys
            .Where(k => deschidere.GetValueOrDefault(k) != (0m, 0m) || inchidere.GetValueOrDefault(k) != (0m, 0m))
            .Concat(postari.Where(p => p.Data >= pStart && !p.Deschidere).Select(p => (p.Gestiune, p.Lot, p.Cont)))
            .ToHashSet();
        var fisier = saft.StocFizic.Select(e => (e.RepartitorId, e.LotId, e.ContId)).ToList();
        var cheiFisier = fisier.ToHashSet();
        var diferite = saft.StocFizic.Count(e =>
            (e.OpeningQuantity, e.OpeningValue) != deschidere.GetValueOrDefault((e.RepartitorId, e.LotId, e.ContId))
            || (e.ClosingQuantity, e.ClosingValue) != inchidere.GetValueOrDefault((e.RepartitorId, e.LotId, e.ContId)));
        var lipsa = asteptate.Where(k => !cheiFisier.Contains(k)).ToList();
        return new Rezultat(postari.Count, asteptate.Select(k => k.Cont).Distinct().Count(), fisier.Count, diferite + (fisier.Count - cheiFisier.Count),
            lipsa.Count, fisier.Count(k => !asteptate.Contains(k)),
            lipsa.Select(k => k.Cont).Distinct().Where(c => !fisier.Any(f => f.ContId == c))
                .Select(c => conturi.TryGetValue(c, out var x) ? x.Simbol : c.ToString()).Order().ToList());
    }
}
