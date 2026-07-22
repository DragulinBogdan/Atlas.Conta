using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF;

namespace Atlas.Conta.BackOffice.Module.BusinessObjects;

// Decizia 17 + 09: plățile/încasările sunt tipuri de document; BREGISTRU dispare.
// Laturile: ContPropriu ↔ Partener/Angajat (direcția dă sensul).
public abstract class DocumentTrezorerie : Document {
    public virtual TipInstrumentPlata TipInstrument { get; set; }
    public virtual string NumarExtras { get; set; }
    public virtual DateOnly? DataExtras { get; set; }
}

// Predator = ContPropriu, primitor = beneficiarul.
public class Plata : DocumentTrezorerie {
}

// Predator = plătitorul, primitor = ContPropriu.
public class Incasare : DocumentTrezorerie {
}

// Decizia 17: stingerea — m2m plată↔document cu sume parțiale.
// Invarianți (motor): Σ imperecheri ≤ totalul plății și ≤ totalul documentului.
[NavigationItem("Documente")]
public class Imperechere : BaseObject {
    public virtual Guid DocumentTrezorerieId { get; set; }
    public virtual DocumentTrezorerie DocumentTrezorerie { get; set; }
    public virtual Guid DocumentId { get; set; }
    public virtual Document Document { get; set; }
    public virtual decimal Suma { get; set; }
}
