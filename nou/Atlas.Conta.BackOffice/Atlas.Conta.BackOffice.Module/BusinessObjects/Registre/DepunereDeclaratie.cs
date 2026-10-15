using System.ComponentModel.DataAnnotations;
using Atlas.DXF.Core.Appearance.Attributes;
using DevExpress.ExpressApp.DC;

namespace Atlas.Conta.BackOffice.Module.BusinessObjects;

public enum FormularFiscal { D300 = 1, D394 = 2 }

[ForbidCRUD("ListView", "DetailView")]
[XafDisplayName("Depunere declarație")]
public class DepunereDeclaratie : RandRegistru, IVerificabilLaCommit {
    public void Verifica(DevExpress.ExpressApp.IObjectSpace os, ICollection<string> erori) =>
        erori.Add("Depunerea se confirmă numai prin comanda dedicată.");

    public virtual FormularFiscal Formular { get; set; }
    public virtual int Perioada { get; set; }
    [XafDisplayName("Versiune exportată")]
    public virtual string VersiuneExportata { get; set; }
    [XafDisplayName("Confirmată la")]
    public virtual DateTime ConfirmataLa { get; set; }
    [XafDisplayName("Confirmată de")]
    public virtual string ConfirmataDe { get; set; }
    [XafDisplayName("SHA-256 fișier")]
    [MaxLength(64)]
    public virtual string Sha256Fisier { get; set; }
    [XafDisplayName("Parametrii fișierului")]
    public virtual string ParametriFisier { get; set; }
    [XafDisplayName("Înlocuiește")]
    public virtual Guid? Inlocuieste { get; set; }
    [XafDisplayName("Retrasă la")]
    public virtual DateTime? RetrasaLa { get; set; }
    [XafDisplayName("Retrasă de")]
    public virtual string RetrasaDe { get; set; }
    [XafDisplayName("Motivul retragerii")]
    public virtual string MotivRetragere { get; set; }
}
