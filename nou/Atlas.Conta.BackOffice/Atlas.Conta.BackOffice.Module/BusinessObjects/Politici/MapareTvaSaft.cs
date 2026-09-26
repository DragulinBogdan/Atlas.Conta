using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.Module.BusinessObjects;

public enum SectiuneTvaSaft { GeneralLedger = 1, Facturi = 2 }

[NavigationItem("Politici")]
[XafDisplayName("Mapare TVA SAF-T")]
public class MapareTvaSaft : Politica, ICuProvenienta, IVerificabilLaCommit {
    public void Verifica(DevExpress.ExpressApp.IObjectSpace os, ICollection<string> erori) {
        if (string.IsNullOrWhiteSpace(Versiune) || TipTvaId == Guid.Empty
                || !Enum.IsDefined(Sectiune) || !Enum.IsDefined(Regim) || !Enum.IsDefined(Sens)
                || Cota < 0 || Cota > 100 || Rol is not (N.RolTva.Taxa or N.RolTva.Autocolectare))
            erori.Add("Maparea TVA SAF-T cere versiune, tip, secțiune și calificare fiscală valide.");
        if (Rol == N.RolTva.Autocolectare && (Sectiune != SectiuneTvaSaft.GeneralLedger
                || Sens != SensTva.Achizitie || Regim != RegimTva.TaxareInversa))
            erori.Add("Autocolectarea se mapează numai în GeneralLedger, la achiziții cu taxare inversă.");
        if (TaxType?.Length != 3 || !TaxType.All(char.IsAsciiDigit)
                || TaxCode?.Length != 6 || !TaxCode.All(char.IsAsciiDigit))
            erori.Add("Tipul și codul taxei SAF-T cer trei, respectiv șase cifre.");
    }

    [XafDisplayName("Din seed"), ModelDefault("AllowEdit", "False")]
    public virtual bool DinSeed { get; set; }
    [XafDisplayName("Versiune export")]
    public virtual string Versiune { get; set; }
    [XafDisplayName("Secțiune")]
    public virtual SectiuneTvaSaft Sectiune { get; set; }
    public virtual Guid TipTvaId { get; set; }
    [XafDisplayName("Tip TVA")]
    public virtual TipTva TipTva { get; set; }
    public virtual RegimTva Regim { get; set; }
    [XafDisplayName("Cotă")]
    public virtual decimal Cota { get; set; }
    [XafDisplayName("Import")]
    public virtual bool DeImport { get; set; }
    public virtual SensTva Sens { get; set; }
    [XafDisplayName("Rol fiscal")]
    public virtual N.RolTva Rol { get; set; }
    [XafDisplayName("Tip taxă")]
    public virtual string TaxType { get; set; }
    [XafDisplayName("Cod taxă")]
    public virtual string TaxCode { get; set; }
}
