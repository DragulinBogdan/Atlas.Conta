using Atlas.DXF.Core.Appearance.Attributes;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.Module.Cub;

/// <summary>Lista de evidență a cubului: postarea cu codurile nomenclatoarelor, citită din view-ul `PostareVizual` (D9-A12).</summary>
[NavigationItem("Registre")]
[ForbidCRUD("ListView", "DetailView")]
[XafDisplayName("Postări")]
public class PostareVizual {
    public virtual Guid ID { get; set; }

    public virtual N.Spatiu Spatiu { get; set; }

    public virtual Guid TranzactieId { get; set; }
    [XafDisplayName("Tranzacție")]
    public virtual N.FelTranzactie TranzactieFel { get; set; }

    public virtual Guid? DocumentId { get; set; }
    [XafDisplayName("Document")]
    public virtual string DocumentNumar { get; set; }
    public virtual Guid? LinieId { get; set; }

    public virtual DateOnly Data { get; set; }
    public virtual Guid Cont { get; set; }
    [XafDisplayName("Cont")]
    public virtual string ContSimbol { get; set; }
    [XafDisplayName("Latură")]
    public virtual N.Latura Latura { get; set; }
    public virtual Guid? Partener { get; set; }
    [XafDisplayName("Partener")]
    public virtual string PartenerCod { get; set; }
    public virtual Guid? Gestiune { get; set; }
    [XafDisplayName("Gestiune")]
    public virtual string GestiuneCod { get; set; }
    public virtual Guid? Produs { get; set; }
    [XafDisplayName("Produs")]
    public virtual string ProdusCod { get; set; }
    public virtual Guid? Unitate { get; set; }
    [XafDisplayName("Unitate")]
    public virtual string UnitateCod { get; set; }
    public virtual DateOnly? UnitateDeschisa { get; set; }
    public virtual N.FelUnitate? FelUnitate { get; set; }

    public virtual Guid? SuportId { get; set; }
    public virtual N.Spatiu? SuportSpatiu { get; set; }
    public virtual Guid? InversaDinId { get; set; }
    public virtual N.Spatiu? InversaDinSpatiu { get; set; }

    public virtual Guid? TipTvaId { get; set; }
    [XafDisplayName("Tip TVA")]
    public virtual string TipTvaCod { get; set; }
    public virtual N.SensTva? SensTva { get; set; }
    public virtual N.RolTva? RolTva { get; set; }
    public virtual int? PerioadaDeclarare { get; set; }
    public virtual N.RegimTva? RegimTva { get; set; }
    public virtual decimal? CotaTva { get; set; }
    public virtual bool? DeImport { get; set; }
    public virtual Guid? DocumentFiscalId { get; set; }
    public virtual DateOnly? DataDocument { get; set; }
    public virtual DateOnly? DataExigibilitate { get; set; }
    public virtual DateOnly? DataPrimire { get; set; }
    public virtual DateOnly? DataInregistrare { get; set; }
    public virtual int? PerioadaD394 { get; set; }
    public virtual bool RegularizareD300 { get; set; }
    public virtual bool InversaTehnica { get; set; }

    public virtual Guid? Valuta { get; set; }
    public virtual N.Carte Carte { get; set; }

    public virtual Guid? CodFunctional { get; set; }
    [XafDisplayName("Cod funcțional")]
    public virtual string CodFunctionalCod { get; set; }
    public virtual Guid? CodEconomic { get; set; }
    [XafDisplayName("Cod economic")]
    public virtual string CodEconomicCod { get; set; }
    public virtual Guid? SursaFinantare { get; set; }
    [XafDisplayName("Sursă de finanțare")]
    public virtual string SursaFinantareCod { get; set; }
    public virtual Guid? UnitateOrganizatorica { get; set; }
    [XafDisplayName("Unitate organizatorică")]
    public virtual string UnitateOrganizatoricaCod { get; set; }
    public virtual Guid? Proiect { get; set; }
    [XafDisplayName("Proiect")]
    public virtual string ProiectCod { get; set; }
    public virtual Guid? CentruCost { get; set; }
    [XafDisplayName("Centru de cost")]
    public virtual string CentruCostCod { get; set; }

    public virtual Guid? Atribuit { get; set; }
    [XafDisplayName("Pereche")]
    public virtual int? Pereche { get; set; }

    public virtual decimal Cantitate { get; set; }
    [XafDisplayName("Valoare în valută")]
    public virtual decimal ValoareValuta { get; set; }
    public virtual decimal Valoare { get; set; }
}
