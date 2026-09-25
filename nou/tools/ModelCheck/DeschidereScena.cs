using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Motor;
using DevExpress.ExpressApp;
using C = Atlas.Conta.BackOffice.Module.Cub;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.ModelCheck;

static class DeschidereScena {
    public static void Scrie(IObjectSpace os, Lot lot, decimal cantitate, decimal valoare) =>
        Scrie(os, lot.Data, (lot, lot.GestiuneId, cantitate, valoare));

    public static void Scrie(IObjectSpace os, DateOnly data,
            params (Lot Lot, Guid Gestiune, decimal Cantitate, decimal Valoare)[] pozitii) {
        os.CommitChanges();
        var loturi = pozitii.Select(p => new C.LotInitial(p.Lot.Produs.TipMaterial.ContImplicitId!.Value,
            p.Lot.ID, p.Gestiune, p.Cantitate, p.Valoare)).ToArray();
        var solduri = loturi.GroupBy(l => l.Cont)
            .Select(g => new C.SoldInitial(g.Key, N.Latura.Debit, g.Sum(l => l.Valoare), true)).ToList();
        var ancora = os.GetObjectsQuery<Cont>().First(c => c.Simbol.StartsWith("891")).ID;
        solduri.Add(new(ancora, N.Latura.Credit, loturi.Sum(l => l.Valoare)));
        using var tx = TranzactieComanda.Incepe(os);
        C.Materializare.Deschide(os, data, solduri, loturi, []);
        os.CommitChanges(); tx.Commit();
    }

    public static void Curata(IObjectSpace os, Purja pj, IEnumerable<Guid> loturi) {
        var idsLot = loturi.ToArray();
        var tx = os.GetObjectsQuery<C.Postare>().Where(p => p.DocumentId == null
            && p.Unitate != null && idsLot.Contains(p.Unitate.Value)).Select(p => p.TranzactieId).Distinct().ToList();
        // Nu purjăm o deschidere care conține unități din afara scenei.
        if (os.GetObjectsQuery<C.Postare>().Any(p => tx.Contains(p.TranzactieId)
                && p.Unitate != null && !idsLot.Contains(p.Unitate.Value)))
            throw new InvalidOperationException("Deschiderea de test conține unități străine scenei.");
        pj.AdaugaCheie<C.Postare>(os.GetObjectsQuery<C.Postare>().Where(p => tx.Contains(p.TranzactieId)).Select(p => p.ID).ToList());
        pj.AdaugaCheie<C.Tranzactie>(tx);
    }
}
