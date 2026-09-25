using Atlas.Conta.BackOffice.Module.BusinessObjects;
using DevExpress.ExpressApp;

namespace Atlas.Conta.BackOffice.Module.Proiectii;

// Cheia istorică vine integral din cub. Etichetele sunt opționale și nu
// hotărăsc dacă un sold există; politica curentă nu reconstruiește TipStoc.
public sealed class SoldStocRand {
    public Guid LotId { get; set; }
    public Guid ContId { get; set; }
    public string ContSimbol { get; set; }
    public Guid RepartitorId { get; set; }
    public Guid ProdusId { get; set; }
    public string ProdusCod { get; set; }
    public string ProdusDenumire { get; set; }
    public string ProdusUM { get; set; }
    public DateOnly LotData { get; set; }
    public decimal LotPretUnitar { get; set; }
    public string GestiuneDenumire { get; set; }
    public decimal Cantitate { get; set; }
    public decimal Valoare { get; set; }
}

public static class StocProiectii {
    public static IQueryable<SoldStocRand> SoldStoc(IObjectSpace os, DateOnly? laData = null) =>
        from a in Cub.Citiri.Loturi.Solduri(os, laData)
        join cont in os.GetObjectsQuery<Cont>() on a.ContId equals cont.ID into conturi
        from cont in conturi.DefaultIfEmpty()
        join produs in os.GetObjectsQuery<Produs>() on a.ProdusId equals produs.ID into produse
        from produs in produse.DefaultIfEmpty()
        join rep in os.GetObjectsQuery<Repartitor>() on a.GestiuneId equals rep.ID into repartitori
        from rep in repartitori.DefaultIfEmpty()
        select new SoldStocRand {
            LotId = a.LotId, ContId = a.ContId, ContSimbol = cont.Simbol,
            RepartitorId = a.GestiuneId, ProdusId = a.ProdusId,
            ProdusCod = produs.Cod, ProdusDenumire = produs.Denumire, ProdusUM = produs.UM,
            LotData = a.Deschisa,
            LotPretUnitar = a.Cantitate == 0m ? 0m : a.Valoare / a.Cantitate,
            GestiuneDenumire = rep.Denumire,
            Cantitate = a.Cantitate, Valoare = a.Valoare
        };
}
