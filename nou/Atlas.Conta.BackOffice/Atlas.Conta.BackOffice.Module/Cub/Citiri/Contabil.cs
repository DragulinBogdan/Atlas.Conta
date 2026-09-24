using DevExpress.ExpressApp;
using Atlas.Conta.BackOffice.Module.Motor;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.Module.Cub.Citiri;

public static class Contabil {
    public static IQueryable<Postare> Postari(IObjectSpace os) {
        var toate = os.GetObjectsQuery<Postare>();
        var domeniu = toate.Where(p => p.Carte == N.Carte.Contabil).Where(Transformare.FaraContrapondere);
        var lipsa = domeniu.Count(p => p.Tranzactie.Fel == N.FelTranzactie.Storno
            && !toate.Any(o => o.ID == p.InversaDinId && o.Spatiu == p.InversaDinSpatiu
                && o.Tranzactie.Fel != N.FelTranzactie.Storno));
        if (lipsa != 0)
            throw new OperareException($"CITIRE_PROVENIENTA_LIPSA: {lipsa} postări Storno fără origine verificabilă; completați proveniența înaintea citirii contabile.");
        return domeniu.Where(p => p.Tranzactie.Fel == N.FelTranzactie.Operare || p.Tranzactie.Fel == N.FelTranzactie.Deschidere
                || (p.Tranzactie.Fel == N.FelTranzactie.Storno && toate.Any(o =>
                    o.ID == p.InversaDinId && o.Spatiu == p.InversaDinSpatiu
                    && o.Tranzactie.Fel != N.FelTranzactie.Transfer)));
    }
}
