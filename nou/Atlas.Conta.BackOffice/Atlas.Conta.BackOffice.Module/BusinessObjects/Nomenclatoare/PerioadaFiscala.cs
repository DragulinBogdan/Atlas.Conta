using System.Collections.ObjectModel;
using Atlas.DXF.Core.Appearance.Attributes;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF;

namespace Atlas.Conta.BackOffice.Module.BusinessObjects;

// Perioada fiscală iese din `DimensiuniBugetare.cs` în fișier propriu (F27-D1):
// nu mai e un nomenclator de patru câmpuri, ci veriga unui LANȚ, cu istoric
// append-only și cu starea scrisă exclusiv de motor (`Motor/PerioadaService`).
[NavigationItem("Nomenclatoare")]
[XafDisplayName("Perioadă fiscală")]
public class PerioadaFiscala : BaseObject {
    // Fără separator de mii: editorul numeric implicit ar arăta „2.026”.
    [ModelDefault("EditMask", "d")]
    [ModelDefault("DisplayFormat", "{0:0}")]
    public virtual int An { get; set; }
    public virtual int Luna { get; set; }

    // Pivotul gardienilor din decizia 14: perioada închisă = graniță absolută.
    [ModelDefault("AllowEdit", "False")]
    [XafDisplayName("Închisă")]
    public virtual bool Inchisa { get; set; }

    [ModelDefault("AllowEdit", "False")]
    [XafDisplayName("Închisă la")]
    public virtual DateTime? InchisaLa { get; set; }

    // NU se șterge la redeschidere: e reperul conținutului de rectificativă (F27-D5).
    [ModelDefault("AllowEdit", "False")]
    [XafDisplayName("Închisă prima oară")]
    public virtual DateTime? InchisaPrimaOara { get; set; }

    [XafDisplayName("Istoric")]
    public virtual ObservableCollection<InchiderePerioada> Istoric { get; set; } = new();
}

// Rândul de istoric al perioadei (F27-D1): append-only, al motorului, ca
// registrele. Nu e agregat al perioadei — ștergerea perioadei nu-l ia cu ea.
[ForbidCRUD("ListView", "DetailView")]
[XafDisplayName("Închidere de perioadă")]
public class InchiderePerioada : BaseObject {
    public virtual Guid PerioadaId { get; set; }
    [XafDisplayName("Perioada")]
    public virtual PerioadaFiscala Perioada { get; set; }

    [XafDisplayName("Fel")]
    public virtual FelInchiderePerioada Fel { get; set; }

    [XafDisplayName("La")]
    public virtual DateTime La { get; set; }

    // Null pe căile STANDALONE (seed, Import1C, ModelCheck): acolo nu există
    // utilizator XAF. `De` rămâne snapshot lizibil — numele de atunci.
    [XafDisplayName("Utilizator")]
    public virtual Guid? DeId { get; set; }
    [XafDisplayName("De")]
    public virtual string De { get; set; }

    [XafDisplayName("Motiv")]
    public virtual string Motiv { get; set; }

    [XafDisplayName("Acceptări")]
    public virtual string Acceptari { get; set; }
}

public enum FelInchiderePerioada {
    [XafDisplayName("Închidere")] Inchidere = 1,
    [XafDisplayName("Redeschidere")] Redeschidere = 2,
}
