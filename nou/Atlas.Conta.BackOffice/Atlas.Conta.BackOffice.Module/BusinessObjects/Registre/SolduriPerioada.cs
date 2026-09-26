using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.BaseImpl.EF;

namespace Atlas.Conta.BackOffice.Module.BusinessObjects;

// Proiecții reconstruibile din cub la sfârșitul perioadelor de referință (090f).
[XafDisplayName("Sold de perioadă (contabil)")]
public class SoldPerioadaContabil : BaseObject {
    // Fără separator de mii, ca pe `PerioadaFiscala`.
    [DevExpress.ExpressApp.Model.ModelDefault("EditMask", "d")]
    [DevExpress.ExpressApp.Model.ModelDefault("DisplayFormat", "{0:0}")]
    public virtual int An { get; set; }
    public virtual int Luna { get; set; }

    public virtual Guid ContId { get; set; }
    [XafDisplayName("Cont")]
    public virtual Cont Cont { get; set; }

    // Cele 8 dimensiuni ale LATURII, cu numele lui `AtomContabil`: snapshot-ul
    // e suma atomilor, nu a rândurilor de registru.
    public virtual Guid? RepartitorId { get; set; }
    [XafDisplayName("Repartitor")]
    public virtual Repartitor Repartitor { get; set; }
    [XafDisplayName("Gestiune")]
    public virtual Guid? GestiuneId { get; set; }
    public virtual Guid? MaterialId { get; set; }
    [XafDisplayName("Material")]
    public virtual Produs Material { get; set; }
    public virtual Guid? CodFunctionalId { get; set; }
    [XafDisplayName("Cod funcțional")]
    public virtual CodFunctional CodFunctional { get; set; }
    public virtual Guid? CodEconomicId { get; set; }
    [XafDisplayName("Cod economic")]
    public virtual CodEconomic CodEconomic { get; set; }
    public virtual Guid? SursaFinantareId { get; set; }
    [XafDisplayName("Sursă finanțare")]
    public virtual SursaFinantare SursaFinantare { get; set; }
    public virtual Guid? UnitateId { get; set; }
    [XafDisplayName("Unitate")]
    public virtual Unitate Unitate { get; set; }
    public virtual Guid? ProiectId { get; set; }
    [XafDisplayName("Proiect")]
    public virtual Proiect Proiect { get; set; }
    public virtual Guid? CentruCostId { get; set; }
    [XafDisplayName("Centru cost")]
    public virtual Repartitor CentruCost { get; set; }

    // Cumulate de la începutul bazei până la sfârșitul perioadei, SEPARAT pe
    // laturi: netarea nu e aditivă (66d), deci se face abia la nivelul cerut.
    [XafDisplayName("Debit")]
    public virtual decimal Debit { get; set; }
    [XafDisplayName("Credit")]
    public virtual decimal Credit { get; set; }
}

[XafDisplayName("Sold de perioadă (stoc)")]
public class SoldPerioadaStoc : BaseObject {
    [DevExpress.ExpressApp.Model.ModelDefault("EditMask", "d")]
    [DevExpress.ExpressApp.Model.ModelDefault("DisplayFormat", "{0:0}")]
    public virtual int An { get; set; }
    public virtual int Luna { get; set; }

    public virtual Guid LotId { get; set; }
    [XafDisplayName("Lot")]
    public virtual Lot Lot { get; set; }
    [XafDisplayName("Cont")]
    public virtual Guid ContId { get; set; }
    [XafDisplayName("Produs")]
    public virtual Guid ProdusId { get; set; }
    [XafDisplayName("Gestiune")]
    public virtual Guid GestiuneId { get; set; }
    [XafDisplayName("Data deschiderii")]
    public virtual DateOnly Deschisa { get; set; }

    [XafDisplayName("Cantitate")]
    public virtual decimal Cantitate { get; set; }
    [XafDisplayName("Valoare")]
    public virtual decimal Valoare { get; set; }
}

[XafDisplayName("Partidă deschisă")]
public class PartidaDeschisa : BaseObject {
    [DevExpress.ExpressApp.Model.ModelDefault("EditMask", "d")]
    [DevExpress.ExpressApp.Model.ModelDefault("DisplayFormat", "{0:0}")]
    public virtual int An { get; set; }
    public virtual int Luna { get; set; }

    public virtual Guid? DocumentId { get; set; }
    [XafDisplayName("Document")]
    public virtual Document Document { get; set; }

    public virtual Guid UnitateId { get; set; }
    public virtual Guid ContId { get; set; }
    public virtual Guid PartenerId { get; set; }
    public virtual DateOnly Deschisa { get; set; }
    public virtual decimal Debit { get; set; }
    public virtual decimal Credit { get; set; }
    [XafDisplayName("Rest")]
    public virtual decimal Rest { get; set; }
}
