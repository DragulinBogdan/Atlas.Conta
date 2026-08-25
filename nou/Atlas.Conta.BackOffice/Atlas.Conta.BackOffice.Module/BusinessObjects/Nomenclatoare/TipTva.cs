using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Editors;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF;

namespace Atlas.Conta.BackOffice.Module.BusinessObjects;

// P1 (design §2): regimul fiscal e nomenclator, nu procent — cotă × regim,
// cu conturile de TVA ca DATE (per profil: 4426/4427/4428 la privat; profilul
// bugetar are doar rânduri Capitalizat, fără conturi) și mapările de raportare
// ca atribute (SAF-T e direcțional: serii separate livrare/achiziție).
[NavigationItem("Nomenclatoare")]
[XafDefaultProperty(nameof(Denumire))]
public class TipTva : BaseObject {
    public virtual string Cod { get; set; }
    public virtual string Denumire { get; set; }
    public virtual decimal Cota { get; set; }
    public virtual RegimTva Regim { get; set; }

    // Conturile de TVA (4426/4427/4428) trăiesc în planul mare — lookup standard
    // (SmartLookup revertat, decizia 40d/gate).
    public virtual Guid? ContTvaDeductibilId { get; set; }
    [EditorAlias(EditorAliases.LookupPropertyEditor)]
    public virtual Cont ContTvaDeductibil { get; set; }
    public virtual Guid? ContTvaColectatId { get; set; }
    [EditorAlias(EditorAliases.LookupPropertyEditor)]
    public virtual Cont ContTvaColectat { get; set; }
    // REZERVAT (design §8): TVA la încasare / facturi nesosite — mecanismul e
    // amânat, nomenclatorul fixează doar contul.
    public virtual Guid? ContTvaNeexigibilId { get; set; }
    [EditorAlias(EditorAliases.LookupPropertyEditor)]
    public virtual Cont ContTvaNeexigibil { get; set; }

    // Maparea D406 (nomenclatorul ANAF de coduri de taxă, direcțional).
    public virtual string CodSafTLivrare { get; set; }
    public virtual string CodSafTAchizitie { get; set; }
    // Fosta `CategorieD394` (36d) a MURIT la felia 14: tipul de operațiune D394
    // e direcțional (N21 = L pe livrare, A pe achiziție), deci e politică
    // `(TipTva × Sens) → tip` — `MapareD394` —, nu atribut pe tip (D4-D2).
}
