using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF;

namespace Atlas.Conta.BackOffice.Module.BusinessObjects;

// Nomenclatoarele-țintă ale componentelor din `Dimensiuni` (decizia 11/15).

[NavigationItem("Nomenclatoare")]
[XafDefaultProperty(nameof(Denumire))]
public class CodFunctional : BaseObject {
    public virtual string Cod { get; set; }
    public virtual string Denumire { get; set; }
}

[NavigationItem("Nomenclatoare")]
[XafDefaultProperty(nameof(Denumire))]
public class CodEconomic : BaseObject {
    public virtual string Cod { get; set; }
    public virtual string Denumire { get; set; }
}

// Decizia 11: sursa de finanțare devine dimensiune explicită.
[NavigationItem("Nomenclatoare")]
[XafDefaultProperty(nameof(Denumire))]
public class SursaFinantare : BaseObject {
    public virtual string Cod { get; set; }
    public virtual string Denumire { get; set; }
}

[NavigationItem("Nomenclatoare")]
[XafDefaultProperty(nameof(Denumire))]
public class Proiect : BaseObject {
    public virtual string Cod { get; set; }
    public virtual string Denumire { get; set; }
}

[NavigationItem("Nomenclatoare")]
[XafDefaultProperty(nameof(Denumire))]
public class Unitate : BaseObject {
    public virtual string Cod { get; set; }
    public virtual string Denumire { get; set; }
}

// STUB (testul bazei §7.1): modulul de angajamente (head + detaliu + self-reference,
// tipuri legal/buget anual/multianual) se proiectează separat; până atunci ancora
// FK de pe linia de document rămâne stabilă.
[NavigationItem("Nomenclatoare")]
[XafDefaultProperty(nameof(Denumire))]
public class Angajament : BaseObject {
    public virtual string Cod { get; set; }
    public virtual string Denumire { get; set; }
}

[NavigationItem("Nomenclatoare")]
public class PerioadaFiscala : BaseObject {
    public virtual int An { get; set; }
    public virtual int Luna { get; set; }
    // Pivotul gardienilor din decizia 14: perioada închisă = graniță absolută.
    public virtual bool Inchisa { get; set; }
}
