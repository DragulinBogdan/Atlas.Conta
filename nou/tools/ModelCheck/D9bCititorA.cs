using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Cub.Citiri;
using Atlas.Conta.BackOffice.Module.Proiectii;
using DevExpress.ExpressApp;
using P = Atlas.Conta.BackOffice.Module.Cub.Citiri.Partide;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.ModelCheck;

static class D9bCititorA {
    static readonly string SensDatorie = SensStingere.Datorie.ToString();
    static readonly string SensCreanta = SensStingere.Creanta.ToString();

    public static IQueryable<OriginePartida> Origini(IObjectSpace os) => P.Postari(os)
        .Where(p => p.DocumentId == null && p.Tranzactie.Fel == N.FelTranzactie.Deschidere
            || p.DocumentId != null && p.Unitate == P.Identitate(p.DocumentId.Value, p.Cont, p.Repartitor.Value))
        .Select(p => new OriginePartida { UnitateId = p.Unitate.Value, ContId = p.Cont,
            PartenerId = p.Repartitor.Value, DocumentId = p.DocumentId }).Distinct();

    static IQueryable<OriginePartida> OriginiDocument(IObjectSpace os) => P.Postari(os)
        .Where(p => p.DocumentId != null && p.Unitate == P.Identitate(p.DocumentId.Value, p.Cont, p.Repartitor.Value))
        .Select(p => new OriginePartida { UnitateId = p.Unitate.Value, ContId = p.Cont,
            PartenerId = p.Repartitor.Value, DocumentId = p.DocumentId }).Distinct();

    static IQueryable<PartidaCuOrigine> CuOrigine(IObjectSpace os, IQueryable<SoldPartida> solduri) =>
        from s in solduri
        join o in OriginiDocument(os)
            on new { UnitateId = (Guid?)s.UnitateId ?? Guid.Empty, s.ContId, PartenerId = (Guid?)s.PartenerId ?? Guid.Empty }
            equals new { o.UnitateId, o.ContId, o.PartenerId } into origine
        from o in origine.DefaultIfEmpty()
        select new PartidaCuOrigine { UnitateId = s.UnitateId, ContId = s.ContId, PartenerId = s.PartenerId,
            Deschisa = s.Deschisa, DocumentId = o.DocumentId, Debit = s.Debit, Credit = s.Credit };

    public static IQueryable<PartidaCuRestRand> PartideCuRest(IObjectSpace os, DateOnly zi) {
        var solduri = P.Cumulate(os, CitireCumul.Vizibila, zi).Where(s => s.Debit != s.Credit);
        return from s in CuOrigine(os, solduri)
               join d in os.GetObjectsQuery<Document>() on s.DocumentId equals (Guid?)d.ID into document
               from d in document.DefaultIfEmpty()
               join c in os.GetObjectsQuery<Cont>() on s.ContId equals c.ID into cont
               from c in cont.DefaultIfEmpty()
               join p in os.GetObjectsQuery<Repartitor>() on s.PartenerId equals p.ID into partener
               from p in partener.DefaultIfEmpty()
               let rest = Math.Abs(s.Debit - s.Credit)
               select new PartidaCuRestRand {
                   UnitateId = s.UnitateId, ContId = s.ContId, ContSimbol = c.Simbol,
                   ContrapartidaId = s.PartenerId, ContrapartidaDenumire = p.Denumire,
                   DocumentId = (Guid?)d.ID, DocumentTip = null, Numar = d.Numar, Data = s.Deschisa,
                   SensStingere = s.Debit > s.Credit ? SensCreanta : SensDatorie, Rest = rest
               };
    }
}
