using Atlas.Conta.BackOffice.Module.BusinessObjects;
using DevExpress.ExpressApp;

namespace Atlas.Conta.BackOffice.Module.Motor;

// Cheia unei poziții de stoc: registrul e sold prin SUM pe această cheie.
public readonly record struct CheieStoc(Guid LotId, Guid RepartitorId, TipStoc TipStoc);

// O mișcare de stoc încă nescrisă (delta) — limbajul verificărilor de sold.
public readonly record struct MiscareStoc(CheieStoc Cheie, DateOnly Data, decimal Cantitate);

public static class StocService {
    public static decimal Sold(IObjectSpace os, CheieStoc cheie, DateOnly? panaLa = null) {
        var rows = os.GetObjectsQuery<RegistruStoc>()
            .Where(r => r.LotId == cheie.LotId && r.RepartitorId == cheie.RepartitorId && r.TipStoc == cheie.TipStoc);
        if (panaLa is { } d)
            rows = rows.Where(r => r.Data <= d);
        return rows.Sum(r => (decimal?)r.Cantitate) ?? 0m;
    }

    // Gardianul din decizia 14: după aplicarea mișcărilor `delta` (și, la
    // anulare, excluderea rândurilor `randuriEliminate`), soldul CUMULAT al
    // fiecărei chei afectate trebuie să rămână ≥ 0 la sfârșitul FIECĂREI zile
    // (granularitatea registrului e ziua — ordinea intra-zi nu e definită).
    // Acoperă și operarea retroactivă: un minus inserat în urmă nu are voie să
    // ducă vreun prefix ulterior sub zero.
    public static void VerificaSoldIntermediar(IObjectSpace os, IReadOnlyCollection<MiscareStoc> delta,
        IReadOnlyCollection<Guid> randuriEliminate = null) {
        var chei = delta.Select(m => m.Cheie).Distinct().ToList();
        var erori = new List<string>();
        foreach (var cheie in chei) {
            var existente = os.GetObjectsQuery<RegistruStoc>()
                .Where(r => r.LotId == cheie.LotId && r.RepartitorId == cheie.RepartitorId && r.TipStoc == cheie.TipStoc)
                .Select(r => new { r.ID, r.Data, r.Cantitate })
                .ToList();
            var miscari = existente
                .Where(r => randuriEliminate == null || !randuriEliminate.Contains(r.ID))
                .Select(r => (r.Data, r.Cantitate))
                .Concat(delta.Where(m => m.Cheie == cheie).Select(m => (m.Data, m.Cantitate)));

            decimal sold = 0;
            foreach (var zi in miscari.GroupBy(m => m.Data).OrderBy(g => g.Key)) {
                sold += zi.Sum(m => m.Cantitate);
                if (sold < 0) {
                    var lot = os.GetObjectByKey<Lot>(cheie.LotId);
                    var rep = os.GetObjectByKey<Repartitor>(cheie.RepartitorId);
                    erori.Add($"Sold negativ ({sold:0.####}) la {zi.Key:yyyy-MM-dd} pe lotul " +
                        $"{lot?.Produs?.Denumire} din {lot?.Data:yyyy-MM-dd} ({rep?.Denumire}, {cheie.TipStoc}).");
                    break;
                }
            }
        }
        if (erori.Count > 0)
            throw new OperareException(string.Join("\n", erori));
    }

    // Picking auto-FIFO TOLERANT (decizia 13, extins la P2 §5): sparge o cerere
    // (produs × gestiune × TipStoc × dată × cantitate) pe loturile disponibile în
    // ordinea vechimii lotului și întoarce alocările + restul neacoperit (spre
    // deosebire de `AlocaFifo`, NU aruncă la insuficient — descărcarea de gestiune
    // lasă restul ca backorder). `dejaAlocat` = alocări per lot deja făcute în
    // aceeași generare, încă necomise (`Sold` nu le vede) — se SCADE din soldul
    // fiecărui lot ca a doua trecere să nu re-aloce ce a prins prima.
    public static (IReadOnlyList<(Guid LotId, decimal Cantitate)> Alocari, decimal Ramas) AlocaFifoTolerant(
        IObjectSpace os, Guid produsId, Guid gestiuneId, TipStoc tipStoc, DateOnly data, decimal cantitate,
        IReadOnlyDictionary<Guid, decimal> dejaAlocat = null) {
        var solduri = os.GetObjectsQuery<RegistruStoc>()
            .Where(r => r.Lot.ProdusId == produsId && r.RepartitorId == gestiuneId
                && r.TipStoc == tipStoc && r.Data <= data)
            .GroupBy(r => r.LotId)
            .Select(g => new { LotId = g.Key, Sold = g.Sum(r => r.Cantitate) })
            .Where(x => x.Sold > 0)
            .ToList();

        var alocari = new List<(Guid, decimal)>();
        var ramas = cantitate;
        foreach (var s in solduri
            .Select(x => (Lot: os.GetObjectByKey<Lot>(x.LotId), x.Sold))
            .OrderBy(x => x.Lot.Data).ThenBy(x => x.Lot.ID)) {
            if (ramas <= 0)
                break;
            var disponibil = s.Sold - (dejaAlocat?.GetValueOrDefault(s.Lot.ID) ?? 0m);
            if (disponibil <= 0)
                continue;
            var alocat = Math.Min(ramas, disponibil);
            alocari.Add((s.Lot.ID, alocat));
            ramas -= alocat;
        }
        return (alocari, ramas);
    }

    // Picking auto-FIFO (decizia 13): sparge o cerere (produs × gestiune ×
    // cantitate) pe loturile disponibile la dată, în ordinea vechimii lotului.
    // Consumat de UI/derivate la culegere; gardianul de sold re-verifică oricum
    // la operare. Override-ul manual = utilizatorul alege alt lot pe linie.
    // Wrapper strict peste `AlocaFifoTolerant`: aruncă dacă rămâne rest.
    public static IReadOnlyList<(Lot Lot, decimal Cantitate)> AlocaFifo(IObjectSpace os,
        Guid produsId, Guid gestiuneId, TipStoc tipStoc, DateOnly data, decimal cantitate) {
        var (alocari, ramas) = AlocaFifoTolerant(os, produsId, gestiuneId, tipStoc, data, cantitate);
        if (ramas > 0)
            throw new OperareException(
                $"Stoc insuficient: lipsesc {ramas:0.####} din {cantitate:0.####} cerute la {data:yyyy-MM-dd}.");
        return alocari.Select(a => (os.GetObjectByKey<Lot>(a.LotId), a.Cantitate)).ToList();
    }
}
