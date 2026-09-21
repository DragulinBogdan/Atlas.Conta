using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF;

namespace Atlas.Conta.BackOffice.Module.BusinessObjects;

// Fișa = nomenclator subțire; parametrii sunt fapte datate în registru (F26-D1).
[NavigationItem("Nomenclatoare")]
[XafDefaultProperty(nameof(Denumire))]
[XafDisplayName("Imobilizare")]
public class Imobilizare : BaseObject, ICuCautare, IVerificabilLaCommit {
    [XafDisplayName("Număr de inventar")]
    public virtual string NumarInventar { get; set; }
    public virtual string Denumire { get; set; }

    public virtual Guid TipMaterialId { get; set; }
    [EditorAlias(EditorAliases.LookupPropertyEditor)]
    [XafDisplayName("Tip (cont/clasă)")]
    public virtual TipMaterial TipMaterial { get; set; }

    public virtual Guid? ClasificareId { get; set; }
    [EditorAlias(EditorAliases.LookupPropertyEditor)]
    [XafDisplayName("Clasificare")]
    public virtual ClasificareImobilizari Clasificare { get; set; }

    public virtual Guid LocId { get; set; }
    [EditorAlias(EditorAliases.LookupPropertyEditor)]
    [XafDisplayName("Loc")]
    public virtual Repartitor Loc { get; set; }

    public virtual Guid? CentruCostId { get; set; }
    [EditorAlias(EditorAliases.LookupPropertyEditor)]
    [XafDisplayName("Centru de cost")]
    public virtual Repartitor CentruCost { get; set; }

    // Dimensiunea cerută de defalcarea contului de cheltuială pe planul bugetar (F26-r16).
    public virtual Guid? CodEconomicId { get; set; }
    [EditorAlias(EditorAliases.LookupPropertyEditor)]
    [XafDisplayName("Cod economic")]
    public virtual CodEconomic CodEconomic { get; set; }

    public virtual Guid? ResponsabilId { get; set; }
    [EditorAlias(EditorAliases.LookupPropertyEditor)]
    [XafDisplayName("Responsabil")]
    public virtual Angajat Responsabil { get; set; }

    // Cele trei sunt ale MOTORULUI, ca `Document.Stare` (F26-D3).
    [ModelDefault("AllowEdit", "False")]
    [XafDisplayName("Stare")]
    public virtual StareImobilizare Stare { get; set; } = StareImobilizare.Noua;
    [ModelDefault("AllowEdit", "False")]
    [XafDisplayName("Data punerii în funcțiune")]
    public virtual DateOnly? DataPunereInFunctiune { get; set; }
    [ModelDefault("AllowEdit", "False")]
    [XafDisplayName("Data ieșirii")]
    public virtual DateOnly? DataIesire { get; set; }

    [XafDisplayName("Căutare")]
    [VisibleInListView(false), VisibleInDetailView(false), VisibleInLookupListView(false)]
    public virtual string Cautare { get; set; }

    // Regulile de editare pe starea ORIGINALĂ din persistență (F26-D1, 55a).
    public void Verifica(DevExpress.ExpressApp.IObjectSpace os, ICollection<string> erori) {
        var sters = os.IsObjectToDelete(this) || os.IsDeletedObject(this);
        if (os.IsNewObject(this)) {
            if (Stare != StareImobilizare.Noua || DataPunereInFunctiune != null || DataIesire != null)
                erori.Add("O fișă nouă se creează în starea Nouă, fără date de punere în funcțiune "
                    + "sau de ieșire — le scrie motorul, la operarea documentelor.");
            VerificaNaturaTipului(os, erori);
            return;
        }
        var originale = Motor.GardianEditare.Originale(os, this);
        var stareOriginala = (originale?[nameof(Stare)] as StareImobilizare?) ?? Stare;
        if (sters) {
            var id = ID;
            if (stareOriginala != StareImobilizare.Noua)
                erori.Add($"Fișa {Eticheta()} e în starea „{stareOriginala}” — se șterge doar cât e Nouă. "
                    + "Anulați sau stornați documentele care au mișcat-o.");
            else if (os.GetObjectsQuery<RegistruImobilizari>().Any(r => r.ImobilizareId == id))
                erori.Add($"Fișa {Eticheta()} are rânduri de registru — nu se șterge.");
            else if (os.GetObjectsQuery<PunereInFunctiuneDetaliu>().Any(d => d.ImobilizareId == id)
                    || os.GetObjectsQuery<IesireImobilizareDetaliu>().Any(d => d.ImobilizareId == id)
                    || os.GetObjectsQuery<AmortizareLunaraDetaliu>().Any(d => d.ImobilizareId == id))
                erori.Add($"Fișa {Eticheta()} e purtată de linii de documente (PIF / CAS / AMO, chiar în "
                    + "Draft) — ștergeți liniile întâi.");
            return;
        }
        if (originale == null)
            return;
        if (!Equals(originale[nameof(TipMaterialId)], TipMaterialId))
            VerificaNaturaTipului(os, erori);
        if (Stare != stareOriginala
                || !Equals(originale[nameof(DataPunereInFunctiune)], DataPunereInFunctiune)
                || !Equals(originale[nameof(DataIesire)], DataIesire))
            erori.Add($"Starea și datele de punere în funcțiune / ieșire ale fișei {Eticheta()} "
                + "le scrie doar motorul (operarea PIF / CAS).");
        if (stareOriginala == StareImobilizare.Iesita) {
            erori.Add($"Fișa {Eticheta()} e ieșită — nu se mai modifică.");
            return;
        }
        if (stareOriginala != StareImobilizare.Noua
                && !Equals(originale[nameof(TipMaterialId)], TipMaterialId))
            erori.Add($"Tipul (contul) fișei {Eticheta()} se schimbă doar cât e Nouă: "
                + "contul de imobilizare a intrat deja în politica de amortizare și în note.");
    }

    void VerificaNaturaTipului(DevExpress.ExpressApp.IObjectSpace os, ICollection<string> erori) {
        var natura = NaturaTipului(os, TipMaterialId);
        if (natura != NaturaClasa.Imobilizare)
            erori.Add($"Tipul (contul) fișei {Eticheta()} trebuie să fie de clasă de imobilizări "
                + $"(natura „{natura}”).");
    }

    internal static NaturaClasa NaturaTipului(DevExpress.ExpressApp.IObjectSpace os, Guid tipId) =>
        Motor.Fapte.ClaseTip(os, [tipId]).GetValueOrDefault(tipId).Natura;

    string Eticheta() => string.IsNullOrWhiteSpace(NumarInventar) ? Denumire ?? "(fără număr)" : NumarInventar;
}

// Catalogul HG 2139/2004, nomenclator de seed în nucleu (F26-D4).
[NavigationItem("Nomenclatoare")]
[XafDisplayName("Clasificare de imobilizări")]
public class ClasificareImobilizari : Dimensiune {
    // Banda duratei normale; null pe rândurile de grupă, care nu se aleg pe o fișă.
    [XafDisplayName("Durată minimă (ani)")]
    public virtual int? DurataMinAni { get; set; }
    [XafDisplayName("Durată maximă (ani)")]
    public virtual int? DurataMaxAni { get; set; }
    [XafDisplayName("Grupă")]
    public virtual string Grupa { get; set; }
}
