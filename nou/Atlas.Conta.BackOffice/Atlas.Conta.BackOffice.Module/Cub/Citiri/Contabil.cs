using DevExpress.ExpressApp;
using Atlas.Conta.BackOffice.Module.Motor;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.Module.Cub.Citiri;

public sealed class PostareJurnal {
    public Guid Id { get; set; }
    public N.Spatiu Spatiu { get; set; }
    public Guid TranzactieId { get; set; }
    public N.FelTranzactie Fel { get; set; }
    public DateOnly DataTranzactie { get; set; }
    public DateTime ScrisLa { get; set; }
    public Guid? DocumentId { get; set; }
    public Guid? LinieId { get; set; }
    public DateOnly Data { get; set; }
    public Guid Cont { get; set; }
    public N.Latura Latura { get; set; }
    public decimal Valoare { get; set; }
    public Guid? Partener { get; set; }
    public Guid? TipTvaId { get; set; }
    public N.RolTva? RolTva { get; set; }
    public N.SensTva? SensTva { get; set; }
    public Guid? CodFunctional { get; set; }
    public Guid? CodEconomic { get; set; }
    public Guid? SursaFinantare { get; set; }
    public Guid? UnitateOrganizatorica { get; set; }
    public Guid? Proiect { get; set; }
    public Guid? CentruCost { get; set; }
}

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

    public static IQueryable<PostareJurnal> Jurnal(IObjectSpace os, DateOnly deLa, DateOnly panaLa) =>
        Postari(os).Where(p => p.Data >= deLa && p.Data <= panaLa).Select(p => new PostareJurnal {
            Id = p.ID, Spatiu = p.Spatiu, TranzactieId = p.TranzactieId, Fel = p.Tranzactie.Fel,
            DataTranzactie = p.Tranzactie.Data, ScrisLa = p.Tranzactie.ScrisLa,
            DocumentId = p.DocumentId, LinieId = p.LinieId, Data = p.Data, Cont = p.Cont,
            Latura = p.Latura, Valoare = p.Valoare, Partener = p.Partener,
            TipTvaId = p.TipTvaId, RolTva = p.RolTva, SensTva = p.SensTva,
            CodFunctional = p.CodFunctional, CodEconomic = p.CodEconomic, SursaFinantare = p.SursaFinantare,
            UnitateOrganizatorica = p.UnitateOrganizatorica, Proiect = p.Proiect, CentruCost = p.CentruCost,
        });
}
