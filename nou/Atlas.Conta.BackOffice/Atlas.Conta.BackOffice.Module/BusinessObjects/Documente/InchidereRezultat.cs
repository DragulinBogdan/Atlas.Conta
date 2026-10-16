using Atlas.Conta.BackOffice.Module.UI;
using DevExpress.ExpressApp.DC;

namespace Atlas.Conta.BackOffice.Module.BusinessObjects;

/// <summary>Închiderea lunară a conturilor de rezultat: notă generată din soldurile de la sfârșitul lunii (E3-D3).</summary>
[TipDetaliu(typeof(InchidereRezultatDetaliu))]
[XafDisplayName("Închidere conturi de rezultat")]
public class InchidereRezultat : NotaContabila {
    public override Declaratii.IDeclarant Declarant() => Declaratii.DeclarantInchidereRezultat.Instanta;

    public override void ContribuieRegim(DevExpress.ExpressApp.IObjectSpace os, Api.RegimDocument.Constructor regim) =>
        regim.Decide(Api.RegimDocument.Regenereaza, regim.Draft ? null : "Se regenerează doar un draft.");

    public override void ValideazaOperare(DevExpress.ExpressApp.IObjectSpace os, ICollection<string> erori) {
        base.ValideazaOperare(os, erori);
        foreach (var eroare in Motor.InchidereRezultatService.ValideazaOperare(os, this))
            erori.Add(eroare);
    }
}

/// <summary>O cheie-sursă închisă: latura sursei poartă cheia completă, destinația doar <see cref="CoordonatePastrate"/>.</summary>
public class InchidereRezultatDetaliu : NotaContabilaDetaliu, ILinieCuInchidere {
    [XafDisplayName("Sursa pe debit")]
    public virtual bool SursaPeDebit { get; set; }

    [XafDisplayName("Coordonate păstrate")]
    public virtual DimensiuneFlags CoordonatePastrate { get; set; }

    public virtual Guid? ProdusId { get; set; }
    [XafDisplayName("Material")]
    public virtual Produs Produs { get; set; }

    public virtual Guid? CodFunctionalId { get; set; }
    [XafDisplayName("Cod funcțional")]
    public virtual CodFunctional CodFunctional { get; set; }

    public virtual Guid? SursaFinantareId { get; set; }
    [XafDisplayName("Sursă de finanțare")]
    public virtual SursaFinantare SursaFinantare { get; set; }

    public virtual Guid? UnitateId { get; set; }
    [XafDisplayName("Unitate")]
    public virtual Unitate Unitate { get; set; }

    public virtual Guid? ProiectId { get; set; }
    [XafDisplayName("Proiect")]
    public virtual Proiect Proiect { get; set; }

    public virtual Guid? CentruCostId { get; set; }
    [XafDisplayName("Centru de cost")]
    public virtual Repartitor CentruCost { get; set; }

    public override Guid? ProdusCules() => ProdusId;

    public override Dimensiuni DimensiuniCulese() => new() {
        MaterialId = ProdusId, CodFunctionalId = CodFunctionalId, CodEconomicId = CodEconomicId,
        SursaFinantareId = SursaFinantareId, UnitateId = UnitateId, ProiectId = ProiectId, CentruCostId = CentruCostId,
    };

    public override void PreiaDimensiuni(Dimensiuni s) {
        ProdusId = s.MaterialId; CodFunctionalId = s.CodFunctionalId; CodEconomicId = s.CodEconomicId;
        SursaFinantareId = s.SursaFinantareId; UnitateId = s.UnitateId; ProiectId = s.ProiectId; CentruCostId = s.CentruCostId;
    }

    public Declaratii.InchidereFapt InchidereCuleasa() => new(SursaPeDebit, CoordonatePastrate);
}
