using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.BaseImpl.EF;

namespace Atlas.Conta.BackOffice.Module.BusinessObjects;

// Proiecțiile persistate ale registrelor la sfârșitul unei perioade DE
// REFERINȚĂ (F27-D3): ultima perioadă închisă și fiecare decembrie închis.
// Nu sunt registre și nu sunt urme (invariantul I): sunt derivabile integral
// din registre și se rescriu prin `Motor/SolduriService`. Cheia e cheia
// COMPLETĂ a atomului, ca orice raport să fie rollup ADITIV peste ea; cheile
// integral zero se omit, iar cheia absentă înseamnă zero pentru consumator.
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

// Aceeași regulă, pe cheia registrului de stoc (`Lot × Repartitor × TipStoc`):
// un lot consumat integral iese din snapshot, nu rămâne mort în el.
[XafDisplayName("Sold de perioadă (stoc)")]
public class SoldPerioadaStoc : BaseObject {
    [DevExpress.ExpressApp.Model.ModelDefault("EditMask", "d")]
    [DevExpress.ExpressApp.Model.ModelDefault("DisplayFormat", "{0:0}")]
    public virtual int An { get; set; }
    public virtual int Luna { get; set; }

    public virtual Guid LotId { get; set; }
    [XafDisplayName("Lot")]
    public virtual Lot Lot { get; set; }
    public virtual Guid RepartitorId { get; set; }
    [XafDisplayName("Repartitor")]
    public virtual Repartitor Repartitor { get; set; }
    [XafDisplayName("Tip stoc")]
    public virtual TipStoc TipStoc { get; set; }

    [XafDisplayName("Cantitate")]
    public virtual decimal Cantitate { get; set; }
    [XafDisplayName("Valoare")]
    public virtual decimal Valoare { get; set; }
}
