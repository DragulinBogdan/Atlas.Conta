using DevExpress.ExpressApp;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.Module.Cub.Citiri;

public static class Contabil {
    public static IQueryable<Postare> Postari(IObjectSpace os) {
        var toate = os.GetObjectsQuery<Postare>();
        return toate.Where(p => p.Carte == N.Carte.Contabil
            && p.Gestiune != N.GestiuniVirtuale.Transformare
            && (p.Tranzactie.Fel == N.FelTranzactie.Operare || p.Tranzactie.Fel == N.FelTranzactie.Deschidere
                || (p.Tranzactie.Fel == N.FelTranzactie.Storno && toate.Any(o =>
                    o.ID == p.InversaDinId && o.Spatiu == p.InversaDinSpatiu
                    && o.Tranzactie.Fel != N.FelTranzactie.Transfer))));
    }
}
