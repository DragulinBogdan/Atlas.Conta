using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Editors;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF;

namespace Atlas.Conta.BackOffice.Module.BusinessObjects;

// Decizia 16: TPT — moștenire doar unde schema diferă și identitatea e exclusivă;
// calitățile transversale sunt flags, nu clase.
[NavigationItem("Nomenclatoare")]
[XafDefaultProperty(nameof(Denumire))]
public abstract class Repartitor : BaseObject {
    public virtual string Cod { get; set; }
    public virtual string Denumire { get; set; }
    public virtual CalitateRepartitor Calitati { get; set; }
    public virtual bool Activ { get; set; } = true;
    // Tranșarea (d) din testul bazei, lărgită la decizia 31: contul purtat de
    // repartitor intră în rezolvarea declarativă a regulilor de contare
    // (SursaCont.Repartitor*) pentru ORICE latură — partener 401/404/411,
    // cont propriu 5xx/770, angajat 542 (avansuri) — deci stă pe bază.
    public virtual Guid? ContImplicitId { get; set; }
    // Plan de conturi mare: match exact pe Simbol în locul lookup-ului standard.
    [EditorAlias(EditorAliases.LookupPropertyEditor)]
    public virtual Cont ContImplicit { get; set; }
}

// Furnizor/client = rol contextual dat de poziția pe document (decizia 16).
public class Partener : Repartitor {
    public virtual string CodFiscal { get; set; }
    public virtual string RegistruComert { get; set; }
}

public class Angajat : Repartitor {
    public virtual string Marca { get; set; }
}

public class Gestiune : Repartitor {
}

public class UnitateInterna : Repartitor {
}

// Casele și conturile proprii (legacy `casierie`) — decizia din 09 §3.
public class ContPropriu : Repartitor {
    public virtual string Iban { get; set; }
    public virtual bool EsteBanca { get; set; }
}
