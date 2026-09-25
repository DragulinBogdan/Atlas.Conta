using DevExpress.ExpressApp;
using Atlas.Conta.BackOffice.Module.Motor;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.Module.Cub.Citiri;

public static class Contabil {
    static IQueryable<Postare> Domeniu(IObjectSpace os) => os.GetObjectsQuery<Postare>()
        .Where(p => p.Carte == N.Carte.Contabil).Where(Transformare.FaraContrapondere);

    public static IQueryable<Postare> Postari(IObjectSpace os) {
        var toate = os.GetObjectsQuery<Postare>();
        return from p in Domeniu(os)
               join original in toate on new { Id = p.InversaDinId, Spatiu = p.InversaDinSpatiu }
                   equals new { Id = (Guid?)original.ID, Spatiu = (N.Spatiu?)original.Spatiu } into originale
               from original in originale.DefaultIfEmpty()
               where p.Tranzactie.Fel == N.FelTranzactie.Operare || p.Tranzactie.Fel == N.FelTranzactie.Deschidere
                   || p.Tranzactie.Fel == N.FelTranzactie.Storno && original != null
                       && (original.Tranzactie.Fel == N.FelTranzactie.Operare || original.Tranzactie.Fel == N.FelTranzactie.Deschidere)
               select p;
    }
}
