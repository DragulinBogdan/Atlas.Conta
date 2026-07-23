using Atlas.DXF.Core.Appearance.Attributes;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF;

namespace Atlas.Conta.BackOffice.Module.BusinessObjects;

// Decizia 14: registre persistate, append-only, scrise tranzacțional la operare.
// Schema = COD; regulile de alimentare = DATE (Politici/RegulaStoc, RegulaContare).
// Append-only: DOAR motorul și migrarea scriu rândurile; corecția = anulare/storno
// pe document, niciodată editarea rândului. ForbidCRUD ascunde acțiunile
// (New/Save/Delete/Cancel pe ListView + DetailView) ȘI — din Atlas.DXF 26.1.3.6 —
// taie de FOND capabilitățile view-ului (AllowEdit/New/Delete, cheia "ForbidCRUD")
// prin ForbidCrudCapabilitiesController: acoperă lista goală și Save-ul din
// dialogul de modificări nesalvate. (Fostul RegistruReadOnlyController local a fost
// eliminat — enforcement-ul e acum în bibliotecă.)

[NavigationItem("Registre")]
[ForbidCRUD("ListView", "DetailView")]
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
    // Rând de stornare (inversul unui rând de operare, la data stornării).
    public virtual bool Storno { get; set; }
    // Null = rând de deschidere scris de migrare (decizia 12), fără document sursă.
    public virtual Guid? DocumentId { get; set; }
    public virtual Document Document { get; set; }
    public virtual Guid? DetaliuId { get; set; }
    public virtual DocumentDetaliu Detaliu { get; set; }
}

[NavigationItem("Registre")]
[ForbidCRUD("ListView", "DetailView")]
public class RegistruContabil : BaseObject {
    public virtual DateOnly Data { get; set; }
    public virtual string NumarNota { get; set; }
    public virtual Guid ContDebitId { get; set; }
    public virtual Cont ContDebit { get; set; }
    public virtual Guid ContCreditId { get; set; }
    public virtual Cont ContCredit { get; set; }
    public virtual decimal Valoare { get; set; }
    // Seturi complet rezolvate PER LATURĂ (decizia 15 + tripletele dim_d/dim_c
    // din CNOTE): regula poartă Comun/OverrideDebit/OverrideCredit, deci
    // rezultatul coalesce-ului diferă pe debit față de credit. Repartitorul
    // laturii (implicit Predator→debit, Primitor→credit — 00 §5) e componenta
    // Repartitor a fiecărui set.
    // ExpandObjectMembers (experiment, Sarcina 4): expandă membrii owned-ului
    // direct în DetailView-ul rândului (fără popup), DOAR pe aceste două
    // proprietăți și DOAR InDetailView. La orice problemă cauzată de el se scoate.
    [ExpandObjectMembers(DevExpress.Persistent.Base.ExpandObjectMembers.InDetailView)]
    public virtual Dimensiuni DimensiuniDebit { get; set; } = new();
    [ExpandObjectMembers(DevExpress.Persistent.Base.ExpandObjectMembers.InDetailView)]
    public virtual Dimensiuni DimensiuniCredit { get; set; } = new();
    // Rând de stornare (inversul unui rând de operare, la data stornării).
    public virtual bool Storno { get; set; }
    // Null = rând de deschidere scris de migrare (decizia 12), fără document sursă.
    public virtual Guid? DocumentId { get; set; }
    public virtual Document Document { get; set; }
    public virtual Guid? DetaliuId { get; set; }
    public virtual DocumentDetaliu Detaliu { get; set; }
}
