using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Motor;
using DevExpress.ExpressApp;
using P = Atlas.Conta.BackOffice.Module.Cub.Citiri.Partide;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.Module.Proiectii;


public sealed class DocumentCuRestRand {
    public Guid DocumentId { get; set; }
    public string Tip { get; set; }
    public string Numar { get; set; }
    public DateOnly Data { get; set; }
    public Guid ContrapartidaId { get; set; }
    public string ContrapartidaDenumire { get; set; }
    public string Sens { get; set; }

    public decimal Total { get; set; }
    public decimal Asignat { get; set; }
    public decimal Rest { get; set; }
    public decimal Disponibil { get; set; }
}

sealed class AntetCuRest {
    public Guid DocumentId { get; set; }
    public string Tip { get; set; }
    public string Numar { get; set; }
    public DateOnly Data { get; set; }
    public DateOnly DataInregistrare { get; set; }
    public Guid ContrapartidaId { get; set; }
    public string ContrapartidaDenumire { get; set; }
    public string Sens { get; set; }
    public decimal Total { get; set; }
}

public static class ImperecheriProiectii {
    static readonly string SensDatorie = SensStingere.Datorie.ToString();
    static readonly string SensCreanta = SensStingere.Creanta.ToString();



    static IQueryable<AntetCuRest> Antete(IObjectSpace os, bool istoric = false) {
        return
            os.GetObjectsQuery<FacturaIntrare>().Where(d => (d.Stare == StareDocument.Operat || istoric && d.Stare == StareDocument.Stornat))
                .Select(d => new AntetCuRest {
                    DocumentId = d.ID, Tip = "FCT", Numar = d.Numar, Data = d.Data,
                    DataInregistrare = d.DataInregistrare,
                    ContrapartidaId = d.PredatorId, ContrapartidaDenumire = d.Predator.Denumire,
                    Sens = SensDatorie, Total = d.TotalStingere ?? 0m })
            .Concat(os.GetObjectsQuery<FacturaIesire>().Where(d => (d.Stare == StareDocument.Operat || istoric && d.Stare == StareDocument.Stornat))
                .Select(d => new AntetCuRest {
                    DocumentId = d.ID, Tip = "FCL", Numar = d.Numar, Data = d.Data,
                    DataInregistrare = d.DataInregistrare,
                    ContrapartidaId = d.PrimitorId, ContrapartidaDenumire = d.Primitor.Denumire,
                    Sens = SensCreanta, Total = d.TotalStingere ?? 0m }))
            .Concat(os.GetObjectsQuery<Plata>()
                .Where(d => (d.Stare == StareDocument.Operat || istoric && d.Stare == StareDocument.Stornat)
                    && !(d.Predator is ContPropriu && d.Primitor is ContPropriu))
                .Select(d => new AntetCuRest {
                    DocumentId = d.ID, Tip = "PLT", Numar = d.Numar, Data = d.Data,
                    DataInregistrare = d.DataInregistrare,
                    ContrapartidaId = d.PrimitorId, ContrapartidaDenumire = d.Primitor.Denumire,
                    Sens = SensCreanta, Total = d.TotalStingere ?? 0m }))
            .Concat(os.GetObjectsQuery<Incasare>()
                .Where(d => (d.Stare == StareDocument.Operat || istoric && d.Stare == StareDocument.Stornat)
                    && !(d.Predator is ContPropriu && d.Primitor is ContPropriu))
                .Select(d => new AntetCuRest {
                    DocumentId = d.ID, Tip = "INC", Numar = d.Numar, Data = d.Data,
                    DataInregistrare = d.DataInregistrare,
                    ContrapartidaId = d.PredatorId, ContrapartidaDenumire = d.Predator.Denumire,
                    Sens = SensDatorie, Total = d.TotalStingere ?? 0m }))
            .Concat(os.GetObjectsQuery<Decont>().Where(d => (d.Stare == StareDocument.Operat || istoric && d.Stare == StareDocument.Stornat))
                .Select(d => new AntetCuRest {
                    DocumentId = d.ID, Tip = "DEC", Numar = d.Numar, Data = d.Data,
                    DataInregistrare = d.DataInregistrare,
                    ContrapartidaId = d.PredatorId, ContrapartidaDenumire = d.Predator.Denumire,
                    Sens = SensDatorie, Total = d.TotalStingere ?? 0m }))
            .Concat(os.GetObjectsQuery<ReturClient>().Where(d => (d.Stare == StareDocument.Operat || istoric && d.Stare == StareDocument.Stornat))
                .Select(d => new AntetCuRest {
                    DocumentId = d.ID, Tip = "RDC", Numar = d.Numar, Data = d.Data,
                    DataInregistrare = d.DataInregistrare,
                    ContrapartidaId = d.PredatorId, ContrapartidaDenumire = d.Predator.Denumire,
                    Sens = SensDatorie, Total = d.TotalStingere ?? 0m }));

    }

    public static void VerificaAcoperire(IObjectSpace os) {
        // Antetul este martor de diagnostic al acoperirii, nu sursă de rest.
        // Numai Operare: storno/transferurile nu schimbă obligația de a avea unități.
        var parti = P.Postari(os).Where(p => p.Tranzactie.Fel == N.FelTranzactie.Operare)
            .GroupBy(p => new { p.DocumentId, p.Cont, p.Partener })
            .Select(g => new { g.Key.DocumentId,
                Valoare = Math.Abs(g.Sum(p => p.Latura == N.Latura.Debit ? p.Valoare : -p.Valoare)) })
            .GroupBy(p => p.DocumentId).Select(g => new { DocumentId = g.Key, Total = g.Sum(p => p.Valoare) });
        var lipsuri = from a in Antete(os, istoric: true)
                      join p in parti on (Guid?)a.DocumentId equals p.DocumentId into acoperire
                      from p in acoperire.DefaultIfEmpty()
                      let gasit = (decimal?)p.Total ?? 0m
                      where gasit != Math.Abs(a.Total)
                      select new { a.DocumentId, Asteptat = Math.Abs(a.Total), Gasit = gasit };
        var exemple = lipsuri.Take(10).ToArray();
        if (exemple.Length != 0) throw new OperareException("CITIRE_PARTIDE_POLITICA: total de decontare fără acoperire integrală pe partide; "
            + string.Join("; ", exemple.Select(p => $"document {p.DocumentId}, așteptat {p.Asteptat}, găsit {p.Gasit}")));
    }

    public static IQueryable<DocumentCuRestRand> DocumenteCuRest(
        IObjectSpace os, Guid? contrapartidaId = null, SensStingere? sens = null,
        DateOnly? laData = null, Guid? documentCurentId = null, bool stinge = true) {

        var antete = Antete(os);

        if (laData is DateOnly zi)
            antete = antete.Where(a => a.DataInregistrare <= zi);
        if (contrapartidaId is Guid cp)
            antete = antete.Where(a => a.ContrapartidaId == cp);
        if (sens is SensStingere sensCerut) {
            var literal = sensCerut.ToString();
            antete = antete.Where(a => a.Sens == literal);
        }

        var solduri = PartideCuRest(os, contrapartidaId, sens, laData)
            .Where(p => p.DocumentId != null)
            .GroupBy(p => new { p.DocumentId, p.ContrapartidaId, p.Sens })
            .Select(g => new { g.Key.DocumentId, g.Key.ContrapartidaId, g.Key.Sens,
                Rest = g.Sum(p => p.Rest) });
        var ziTotal = laData ?? DateOnly.MaxValue;
        var totale = Cub.Citiri.Contabil.Postari(os)
            .Where(p => p.Data <= ziTotal && p.FelUnitate == N.FelUnitate.Partida)
            .GroupBy(p => new { p.DocumentId, p.Partener })
            .Select(g => new { g.Key.DocumentId, ContrapartidaId = g.Key.Partener,
                Total = g.Sum(p => p.Latura == N.Latura.Debit ? p.Valoare : -p.Valoare) });
        var raport = from a in antete
               join s in solduri on new { DocumentId = (Guid?)a.DocumentId, a.ContrapartidaId, a.Sens }
                   equals new { s.DocumentId, s.ContrapartidaId, s.Sens }
               join t in totale on new { s.DocumentId, ContrapartidaId = (Guid?)s.ContrapartidaId }
                   equals new { t.DocumentId, t.ContrapartidaId }
               select new DocumentCuRestRand {
                   DocumentId = a.DocumentId, Tip = a.Tip, Numar = a.Numar, Data = a.Data,
                   ContrapartidaId = a.ContrapartidaId, ContrapartidaDenumire = a.ContrapartidaDenumire,
                   Sens = a.Sens, Total = Math.Abs(t.Total), Asignat = Math.Abs(t.Total) - s.Rest,
                   Rest = s.Rest, Disponibil = s.Rest
               };
        if (documentCurentId is not { } curent) return raport;
        var perechi = P.Perechi(os, curent, stinge, contrapartidaId)
            .Select(p => new { DocumentId = stinge ? p.StinsId : p.StingatorId, p.Disponibil });
        return from r in raport
               join p in perechi on r.DocumentId equals p.DocumentId
               select new DocumentCuRestRand { DocumentId = r.DocumentId, Tip = r.Tip, Numar = r.Numar,
                   Data = r.Data, ContrapartidaId = r.ContrapartidaId, ContrapartidaDenumire = r.ContrapartidaDenumire,
                   Sens = r.Sens, Total = r.Total, Asignat = r.Asignat, Rest = r.Rest, Disponibil = p.Disponibil };
    }

    public static IQueryable<PartidaCuRestRand> PartideCuRest(IObjectSpace os, Guid? contrapartidaId = null,
            SensStingere? sens = null, DateOnly? laData = null) {
        var zi = laData ?? DateOnly.MaxValue;
        var solduri = P.Cumulate(os, zi).Where(s => s.Debit != s.Credit);
        if (contrapartidaId is { } cp) solduri = solduri.Where(s => s.PartenerId == cp);
        if (sens == SensStingere.Datorie) solduri = solduri.Where(s => s.Credit > s.Debit);
        if (sens == SensStingere.Creanta) solduri = solduri.Where(s => s.Debit > s.Credit);
        return from s in solduri
               join o in P.Origini(os) on new { s.UnitateId, s.ContId, s.PartenerId }
                   equals new { o.UnitateId, o.ContId, o.PartenerId } into origine
               from o in origine.DefaultIfEmpty()
               join d in os.GetObjectsQuery<Document>() on o.DocumentId equals (Guid?)d.ID into document
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
                   Sens = s.Debit > s.Credit ? SensCreanta : SensDatorie,
                   Rest = rest
               };
    }
}

public sealed class PartidaCuRestRand : IRandCuDocument {
    public Guid UnitateId { get; set; }
    public Guid ContId { get; set; }
    public string ContSimbol { get; set; }
    public Guid ContrapartidaId { get; set; }
    public string ContrapartidaDenumire { get; set; }
    public Guid? DocumentId { get; set; }
    public string DocumentTip { get; set; }
    public string Numar { get; set; }
    public DateOnly Data { get; set; }
    public string Sens { get; set; }
    public decimal Rest { get; set; }
}
