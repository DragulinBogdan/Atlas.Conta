using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF;

namespace Atlas.Conta.BackOffice.Module.BusinessObjects;

public enum CauzaDiferentei {
    [XafDisplayName("Perisabilitate")] Perisabilitate = 1,
    [XafDisplayName("Imputabilă")] Imputabila = 2,
    [XafDisplayName("Neimputabilă")] Neimputabila = 3,
    [XafDisplayName("Pe drum")] PeDrum = 4,
    [XafDisplayName("În clarificare")] InClarificare = 5,
    [XafDisplayName("Plus")] Plus = 6,
}

// 099: tip × cauză × clasă; politica nu inventează cauze sau direcții.
[NavigationItem("Politici")]
[XafDisplayName("Politică diferență")]
public class PoliticaDiferenta : BaseObject, ICuProvenienta {
    [XafDisplayName("Din seed"), ModelDefault("AllowEdit", "False")]
    public virtual bool DinSeed { get; set; }
    public virtual Guid TipDocumentId { get; set; }
    [XafDisplayName("Tip document")]
    public virtual TipDocument TipDocument { get; set; }
    public virtual Guid ClasaId { get; set; }
    public virtual ClasaProdus Clasa { get; set; }
    [XafDisplayName("Cauza diferenței")]
    public virtual CauzaDiferentei Cauza { get; set; }
    public virtual Guid ContId { get; set; }
    public virtual Cont Cont { get; set; }
    public virtual Guid? ContPersonalId { get; set; }
    [XafDisplayName("Cont personal")]
    public virtual Cont ContPersonal { get; set; }
}
