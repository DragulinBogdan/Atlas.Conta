using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF;

namespace Atlas.Conta.BackOffice.Module.BusinessObjects;

// Clasă/Tip = echivalentul operațional al planului de conturi (CLAUDE.md):
// Clasa dă natura liniei (și registrul de stoc), Tipul dă maparea contabilă.

[NavigationItem("Nomenclatoare")]
[XafDefaultProperty(nameof(Denumire))]
public class ClasaProdus : BaseObject {
    public virtual string Cod { get; set; }
    public virtual string Denumire { get; set; }
}

[NavigationItem("Nomenclatoare")]
[XafDefaultProperty(nameof(Denumire))]
public class TipMaterial : BaseObject {
    public virtual string Cod { get; set; }
    public virtual string Denumire { get; set; }
    // Fiecare Tip aparține unei Clase (10 §2) — de aceea baza detaliului poartă
    // un singur FK (TipMaterial) iar Clasa se derivă.
    public virtual Guid ClasaId { get; set; }
    public virtual ClasaProdus Clasa { get; set; }
}
