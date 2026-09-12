using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF;

namespace Atlas.Conta.BackOffice.Module.BusinessObjects;

// P1 (design §2): regimul fiscal e nomenclator, nu procent — cotă × regim,
// cu conturile de TVA ca DATE (per profil: 4426/4427/4428 la privat; profilul
// bugetar are doar rânduri Capitalizat, fără conturi) și mapările de raportare
// ca atribute (SAF-T e direcțional: serii separate livrare/achiziție).
[NavigationItem("Nomenclatoare")]
[XafDefaultProperty(nameof(Denumire))]
public class TipTva : BaseObject, ICuCautare, ICuProvenienta {
    // F23-D4 — proveniența rândului: o scrie SEED-ul (pe cel creat și pe cel
    // găsit pe cheia lui), o stinge GARDIANUL la orice scriere securizată.
    [XafDisplayName("Din seed")]
    [ModelDefault("AllowEdit", "False")]
    public virtual bool DinSeed { get; set; }

    public virtual string Cod { get; set; }
    public virtual string Denumire { get; set; }
    public virtual decimal Cota { get; set; }
    public virtual RegimTva Regim { get; set; }

    // F23-D3 — tipul VIU al nomenclatorului. Un tip inactiv nu mai apare în
    // lookup-urile de CULEGERE ale clientului (`Activ eq true`) și nu e ales
    // niciodată de rezolvarea implicitelor (`ImpliciteService`: treapta care
    // l-ar întoarce se SARE, cu motiv). Ce NU face: nu invalidează istoria — un
    // tip inactiv pe o linie EXISTENTĂ e legitim (cota de 19% de dinainte de
    // august 2025 rămâne pe facturile ei) și nu se refuză nicăieri.
    //
    // Inițializatorul `= true` e jumătatea de CULEGERE (un tip nou e viu), iar
    // `HasDefaultValue(true)` din `BackOfficeDbContext` e jumătatea de MIGRAȚIE
    // (rândurile existente rămân vii la adăugarea coloanei).
    [XafDisplayName("Activ")]
    public virtual bool Activ { get; set; } = true;

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

    // DVI-D1 — tipul care poartă TVA-ul datorat în VAMĂ: singura cale de a spune
    // „de import" fără a hardcoda coduri (29). Clientul filtrează pe el
    // lookup-ul liniei de declarație vamală, iar `Dvi.ValideazaOperare` îl cere.
    [XafDisplayName("De import")]
    public virtual bool DeImport { get; set; }

    // Maparea D406 (nomenclatorul ANAF de coduri de taxă, direcțional).
    public virtual string CodSafTLivrare { get; set; }
    public virtual string CodSafTAchizitie { get; set; }

    // F20-D1 — coloana GENERATĂ de căutare fără diacritice; valoarea e a
    // BAZEI de date (vezi `Cautare` / `ICuCautare`), EF n-o scrie niciodată.
    [XafDisplayName("Căutare")]
    [VisibleInListView(false), VisibleInDetailView(false), VisibleInLookupListView(false)]
    public virtual string Cautare { get; set; }
    // Fosta `CategorieD394` (36d) a MURIT la felia 14: tipul de operațiune D394
    // e direcțional (N21 = L pe livrare, A pe achiziție), deci e politică
    // `(TipTva × Sens) → tip` — `MapareD394` —, nu atribut pe tip (D4-D2).
}
