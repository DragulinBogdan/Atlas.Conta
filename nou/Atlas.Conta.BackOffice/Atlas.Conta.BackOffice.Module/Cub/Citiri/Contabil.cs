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
    public N.Sens Sens { get; set; }
    public decimal Valoare { get; set; }
    /// <summary>Terțul postării pe un cont cu rol comercial (C111-D10).</summary>
    public Guid? TertComercial { get; set; }
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

/// <summary>Postarea contabilă cu axa ei analitică, repartitorul de sold (C111-D10).</summary>
public sealed class PostareAnalitica {
    public Guid Id { get; set; }
    public N.Spatiu Spatiu { get; set; }
    public Guid TranzactieId { get; set; }
    public N.FelTranzactie Fel { get; set; }
    public int? Pereche { get; set; }
    public Guid? DocumentId { get; set; }
    public DateOnly Data { get; set; }
    public Guid Cont { get; set; }
    public N.Sens Sens { get; set; }
    public decimal Valoare { get; set; }
    public Guid? Repartitor { get; set; }
    public Guid? Produs { get; set; }
    public Guid? CodFunctional { get; set; }
    public Guid? CodEconomic { get; set; }
    public Guid? SursaFinantare { get; set; }
    public Guid? UnitateOrganizatorica { get; set; }
    public Guid? Proiect { get; set; }
    public Guid? CentruCost { get; set; }
}

public static class Contabil {
    /// <summary>Repartitorii externi: terțul unei postări e repartitorul ei, când e unul dintre ei (C111-D10).</summary>
    public static IQueryable<Guid> Externi(IObjectSpace os) => os.GetObjectsQuery<BusinessObjects.Repartitor>()
        .Where(r => r.ClrType == nameof(N.FelRepartitor.Partener) || r.ClrType == nameof(N.FelRepartitor.Angajat))
        .Select(r => r.ID);

    static IQueryable<Postare> Domeniu(IObjectSpace os) => os.GetObjectsQuery<Postare>()
        .Where(p => p.Carte == N.Carte.Contabil).Where(Transformare.FaraContrapondere);

    /// <summary>Sursa celeilalte postări a unei perechi, pe intervalul cititorului: se caută pe (tranzacție, ordinal), iar vizibilitatea și data sunt ale perechii.</summary>
    public static IQueryable<Postare> Perechi(IObjectSpace os, DateOnly? deLa, DateOnly? panaLa) {
        var postari = Domeniu(os);
        if (deLa is { } inceput) postari = postari.Where(p => p.Data >= inceput);
        if (panaLa is { } sfarsit) postari = postari.Where(p => p.Data <= sfarsit);
        return postari;
    }

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

    /// <summary>Tranzacțiile `Transfer` și stornourile lor: mișcarea între repartitori sau materiale pe același cont, netă zero pe cont (126e).</summary>
    public static IQueryable<Postare> Transferuri(IObjectSpace os) {
        var toate = os.GetObjectsQuery<Postare>();
        return from p in Domeniu(os)
               join original in toate on new { Id = p.InversaDinId, Spatiu = p.InversaDinSpatiu }
                   equals new { Id = (Guid?)original.ID, Spatiu = (N.Spatiu?)original.Spatiu } into originale
               from original in originale.DefaultIfEmpty()
               where p.Tranzactie.Fel == N.FelTranzactie.Transfer
                   || p.Tranzactie.Fel == N.FelTranzactie.Storno && original != null
                       && original.Tranzactie.Fel == N.FelTranzactie.Transfer
               select p;
    }

    public static IQueryable<PostareAnalitica> Analitice(IObjectSpace os) => Analitice(Postari(os));

    public static IQueryable<PostareAnalitica> AnaliticeTransfer(IObjectSpace os) => Analitice(Transferuri(os));

    static IQueryable<PostareAnalitica> Analitice(IQueryable<Postare> postari) =>
        postari.Select(p => new PostareAnalitica {
            Id = p.ID, Spatiu = p.Spatiu, TranzactieId = p.TranzactieId, Fel = p.Tranzactie.Fel, Pereche = p.Pereche,
            DocumentId = p.DocumentId, Data = p.Data, Cont = p.Cont, Sens = p.Sens, Valoare = p.Valoare,
            Repartitor = p.Repartitor, Produs = p.Produs,
            CodFunctional = p.CodFunctional, CodEconomic = p.CodEconomic, SursaFinantare = p.SursaFinantare,
            UnitateOrganizatorica = p.UnitateOrganizatorica, Proiect = p.Proiect, CentruCost = p.CentruCost,
        });

    public static IQueryable<PostareJurnal> Jurnal(IObjectSpace os, DateOnly deLa, DateOnly panaLa) {
        var externi = Externi(os);
        return Postari(os).Where(p => p.Data >= deLa && p.Data <= panaLa).Select(p => new PostareJurnal {
            Id = p.ID, Spatiu = p.Spatiu, TranzactieId = p.TranzactieId, Fel = p.Tranzactie.Fel,
            DataTranzactie = p.Tranzactie.Data, ScrisLa = p.Tranzactie.ScrisLa,
            DocumentId = p.DocumentId, LinieId = p.LinieId, Data = p.Data, Cont = p.Cont,
            Sens = p.Sens, Valoare = p.Valoare,
            TertComercial = p.Repartitor != null && externi.Contains(p.Repartitor.Value) ? p.Repartitor : null,
            TipTvaId = p.TipTvaId, RolTva = p.RolTva, SensTva = p.SensTva,
            CodFunctional = p.CodFunctional, CodEconomic = p.CodEconomic, SursaFinantare = p.SursaFinantare,
            UnitateOrganizatorica = p.UnitateOrganizatorica, Proiect = p.Proiect, CentruCost = p.CentruCost,
        });
    }
}
