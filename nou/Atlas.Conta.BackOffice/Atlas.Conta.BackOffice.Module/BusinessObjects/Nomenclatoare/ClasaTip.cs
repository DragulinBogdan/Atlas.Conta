using Atlas.DXF.Core.Editors;
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
    // Doar clasele cu Natura=Stoc intră în regulile de stoc; cele tehnice
    // (TVA, diferențe) participă numai la contare.
    public virtual NaturaClasa Natura { get; set; }
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
    // Maparea Clasă/Tip → cont e DATE (decizia 4). Seed-ul o derivă din simbol
    // (Cod-ul Tipului E un simbol de cont — 10 §2); editabilă fără release.
    // Consumată de RegulaContare prin SursaCont.TipMaterial.
    public virtual Guid? ContImplicitId { get; set; }
    // Plan de conturi mare (1.679 rânduri la bugetar): match exact pe Simbol.
    [EditorAlias(AtlasEditorAliases.SmartLookupPropertyEditor)]
    public virtual Cont ContImplicit { get; set; }
}
