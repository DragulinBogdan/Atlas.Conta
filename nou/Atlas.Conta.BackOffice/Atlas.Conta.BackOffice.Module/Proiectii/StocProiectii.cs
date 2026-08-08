using Atlas.Conta.BackOffice.Module.BusinessObjects;
using DevExpress.ExpressApp;

namespace Atlas.Conta.BackOffice.Module.Proiectii;

// Citirea = REGISTRE + PROIECȚII (decizia 42c): rapoartele nu interoghează
// niciodată documentele polimorf — trăiesc pe registrele append-only, unde
// agregarea e un `GROUP BY`. Proiecțiile sunt `IQueryable` pur (fără ASP.NET),
// ca `DataSourceLoader` să pună filtrarea/sortarea/paginarea DEASUPRA lor și
// SQL-ul să se execute o singură dată, server-side.
//
// Regula de aur a modulului (42c): nimic nu se calculează în client. `Cantitate`
// și `Valoare` de mai jos SUNT soldurile — TypeScript-ul le afișează, nu le
// însumează.

// Un rând de sold = exact cheia registrului de stoc (`CheieStoc`), plus
// etichetele necesare afișării. PLAT prin construcție: DTO-urile pasului 5 nu
// poartă grafuri (deciziile 6/7).
public sealed class SoldStocRand {
    public Guid LotId { get; set; }
    public Guid RepartitorId { get; set; }
    // STRING, ca `Stare` pe ReadDto-uri: contractul nu depinde de ordinea
    // membrilor enum-ului, iar filtrarea din grilă vine tot ca text.
    public string TipStoc { get; set; }

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
    // Soldul per `Lot × Repartitor × TipStoc` — exact cheia pe care o însumează
    // `StocService.Sold`; consistența celor două e verificată în ModelCheck
    // (D9: o proiecție care ar diverge de motor ar fi un al doilea adevăr).
    //
    // Structura: agregarea ÎNTÂI, join-urile pe REZULTATUL agregat (42c) — nu
    // subquery corelat per rând și nu navigație lazy per instanță (25b/41c).
    // Rândurile de storno sunt incluse deliberat: registrul e append-only, iar
    // soldul E suma lui algebrică (rândurile inverse se anulează singure).
    public static IQueryable<SoldStocRand> SoldStoc(IObjectSpace os) {
        var agregate = os.GetObjectsQuery<RegistruStoc>()
            .GroupBy(r => new { r.LotId, r.RepartitorId, r.TipStoc })
            .Select(g => new {
                g.Key.LotId,
                g.Key.RepartitorId,
                g.Key.TipStoc,
                Cantitate = g.Sum(r => r.Cantitate),
                Valoare = g.Sum(r => r.Valoare)
            });

        return from a in agregate
               join l in os.GetObjectsQuery<Lot>() on a.LotId equals l.ID
               join rep in os.GetObjectsQuery<Repartitor>() on a.RepartitorId equals rep.ID
               select new SoldStocRand {
                   LotId = a.LotId,
                   RepartitorId = a.RepartitorId,
                   // Enum → string ÎN SQL (`CASE`), ca `Stare` pe lista BTR:
                   // filtrarea/sortarea rămân server-side. Lanțul acoperă TOATE
                   // valorile `TipStoc` — un membru nou adăugat fără rând aici
                   // ar apărea ca „ProductieNeterminata" (ultima ramură), deci
                   // enum-ul și proiecția se modifică împreună.
                   TipStoc = a.TipStoc == BusinessObjects.TipStoc.Magazie ? "Magazie"
                       : a.TipStoc == BusinessObjects.TipStoc.Consum ? "Consum"
                       : a.TipStoc == BusinessObjects.TipStoc.Folosinta ? "Folosinta"
                       : a.TipStoc == BusinessObjects.TipStoc.Custodie ? "Custodie"
                       : a.TipStoc == BusinessObjects.TipStoc.Marfuri ? "Marfuri"
                       : a.TipStoc == BusinessObjects.TipStoc.Gratuit ? "Gratuit"
                       : "ProductieNeterminata",
                   ProdusId = l.ProdusId,
                   ProdusCod = l.Produs.Cod,
                   ProdusDenumire = l.Produs.Denumire,
                   ProdusUM = l.Produs.UM,
                   LotData = l.Data,
                   LotPretUnitar = l.PretUnitar,
                   GestiuneDenumire = rep.Denumire,
                   Cantitate = a.Cantitate,
                   Valoare = a.Valoare
               };
    }
}
