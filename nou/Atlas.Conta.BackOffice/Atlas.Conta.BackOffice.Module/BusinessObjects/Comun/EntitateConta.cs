using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.DataLocking;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;

namespace Atlas.Conta.BackOffice.Module.BusinessObjects;

/// <summary>Cheia comună a entităților proprii și a celor de securitate (pe `BaseObject`-ul DX).</summary>
public interface ICuCheie {
    Guid ID { get; }
}

/// <summary>Baza entităților de domeniu: contractele XAF și cheia; mecanismele tehnice vin din familie (104e).</summary>
[ModelGeneration(ModelNodes.DetailView | ModelNodes.ListView | ModelNodes.LookupListView)]
public abstract class EntitateConta : ICuCheie, IXafEntityObject, IObjectSpaceLink {
    [Key]
    [VisibleInListView(false), VisibleInDetailView(false), VisibleInLookupListView(false)]
    public virtual Guid ID { get; set; }

    protected IObjectSpace ObjectSpace;
    IObjectSpace IObjectSpaceLink.ObjectSpace {
        get => ObjectSpace;
        set => ObjectSpace = value;
    }

    public virtual void OnCreated() { }
    public virtual void OnSaving() { }
    public virtual void OnLoaded() { }

    public override string ToString() {
        if (ObjectSpace?.IsDisposed == true)
            return base.ToString();
        var membru = (ObjectSpace?.TypesInfo ?? XafTypesInfo.Instance).FindTypeInfo(GetType()).DeclaredDefaultMember;
        try {
            return membru?.GetValue(this)?.ToString() ?? base.ToString();
        }
        catch {
            return base.ToString();
        }
    }
}

/// <summary>Entitate culeasă de operator: blocare optimistă.</summary>
[ModelGeneration(ModelNodes.DetailView | ModelNodes.ListView | ModelNodes.LookupListView)]
public abstract class Editabila : EntitateConta, IOptimisticLock {
    // Ca pe `BaseObject`-ul DX: `[NotMapped]` îl scoate din tipurile XAF (UI, OData), `UseOptimisticLock` îl mapează.
    [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
    [ConcurrencyCheck, NotMapped, UseInAuditTrail(false)]
    public virtual int OptimisticLockField { get; set; }
}

/// <summary>Nomenclator: se inactivează, nu se șterge cât e referit (104f).</summary>
[ModelGeneration(ModelNodes.DetailView | ModelNodes.ListView | ModelNodes.LookupListView)]
public abstract class Nomenclator : Editabila {
    [XafDisplayName("Activ")]
    public virtual bool Activ { get; set; } = true;
}

/// <summary>Rând de politică (decizia 4): date editabile fără release.</summary>
[ModelGeneration(ModelNodes.DetailView | ModelNodes.ListView | ModelNodes.LookupListView)]
public abstract class Politica : Editabila { }

/// <summary>Rând scris doar de motor: fără blocare, fără ștergere de către operator.</summary>
[ModelGeneration(ModelNodes.DetailView | ModelNodes.ListView | ModelNodes.LookupListView)]
public abstract class RandRegistru : EntitateConta { }
