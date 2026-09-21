using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF;

namespace Atlas.Conta.BackOffice.Module.BusinessObjects;

/// <summary>
/// Contractul nomenclatoarelor-dimensiune (țintele componentelor din `Dimensiuni`):
/// cod + denumire, căutabile fără diacritice. Bază CLR, în afara modelului EF —
/// fiecare derivată își are tabelul și coloana `Cautare` proprie.
/// </summary>
[XafDefaultProperty(nameof(Denumire))]
public abstract class Dimensiune : BaseObject, ICuCautare {
    public virtual string Cod { get; set; }
    public virtual string Denumire { get; set; }

    // F20-D1 — coloană GENERATĂ de bază; EF n-o scrie niciodată.
    [XafDisplayName("Căutare")]
    [VisibleInListView(false), VisibleInDetailView(false), VisibleInLookupListView(false)]
    public virtual string Cautare { get; set; }
}

[NavigationItem("Nomenclatoare")]
public class CodFunctional : Dimensiune { }

[NavigationItem("Nomenclatoare")]
public class CodEconomic : Dimensiune { }

// Decizia 11: sursa de finanțare devine dimensiune explicită.
[NavigationItem("Nomenclatoare")]
public class SursaFinantare : Dimensiune { }

[NavigationItem("Nomenclatoare")]
public class Proiect : Dimensiune { }

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
public class Angajament : Dimensiune { }
