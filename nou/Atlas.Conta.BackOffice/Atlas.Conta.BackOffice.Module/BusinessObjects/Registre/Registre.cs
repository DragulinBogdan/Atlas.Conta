using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF;

namespace Atlas.Conta.BackOffice.Module.BusinessObjects;

// Decizia 14: registre persistate, append-only, scrise tranzacțional la operare.
// Schema = COD; regulile de alimentare = DATE (Politici/RegulaStoc, RegulaContare).

[NavigationItem("Registre")]
public class RegistruStoc : BaseObject {
    public virtual DateOnly Data { get; set; }
    public virtual TipStoc TipStoc { get; set; }
    public virtual Guid LotId { get; set; }
    public virtual Lot Lot { get; set; }
    // Repartitorul afectat (latura predator sau primitor, după regulă).
    public virtual Guid RepartitorId { get; set; }
    public virtual Repartitor Repartitor { get; set; }
    // Semnul e inclus (semn × cantitate / semn × valoare) — sold prin SUM.
    public virtual decimal Cantitate { get; set; }
    public virtual decimal Valoare { get; set; }
    public virtual Guid DocumentId { get; set; }
    public virtual Document Document { get; set; }
    public virtual Guid? DetaliuId { get; set; }
    public virtual DocumentDetaliu Detaliu { get; set; }
}

[NavigationItem("Registre")]
public class RegistruContabil : BaseObject {
    public virtual DateOnly Data { get; set; }
    public virtual string NumarNota { get; set; }
    public virtual Guid ContDebitId { get; set; }
    public virtual Cont ContDebit { get; set; }
    public virtual Guid ContCreditId { get; set; }
    public virtual Cont ContCredit { get; set; }
    public virtual Guid? RepartitorDebitId { get; set; }
    public virtual Repartitor RepartitorDebit { get; set; }
    public virtual Guid? RepartitorCreditId { get; set; }
    public virtual Repartitor RepartitorCredit { get; set; }
    public virtual decimal Valoare { get; set; }
    // Set complet rezolvat (decizia 15). Dacă motorul cere seturi diferite pe
    // debit/credit (tripletele dim_d/dim_c din CNOTE), se tranșează la motor.
    public virtual Dimensiuni Dimensiuni { get; set; } = new();
    public virtual Guid DocumentId { get; set; }
    public virtual Document Document { get; set; }
    public virtual Guid? DetaliuId { get; set; }
    public virtual DocumentDetaliu Detaliu { get; set; }
}
