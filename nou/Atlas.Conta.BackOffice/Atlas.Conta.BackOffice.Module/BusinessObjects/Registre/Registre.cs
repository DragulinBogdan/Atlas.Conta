using System.ComponentModel.DataAnnotations.Schema;
using Atlas.DXF.Core.Appearance.Attributes;
using DevExpress.ExpressApp.DC;
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
    // DIM-3 (decizia 54c): coloane PLATE — [Column] conservă schema owned-ului
    // (DimensiuniDebit_*/DimensiuniCredit_*), migrația e doar de mapare.
    // Motorul scrie din value object prin AplicaDimensiuni*, storno-ul
    // recitește prin Dimensiuni*(); navigațiile se încarcă EAGER (AutoInclude
    // în DbContext — 41c: grid-urile le afișează pe fiecare rând, lazy = N+1).
    [Column("DimensiuniDebit_RepartitorId")]
    public virtual Guid? DebitRepartitorId { get; set; }
    [XafDisplayName("Repartitor debit")]
    public virtual Repartitor DebitRepartitor { get; set; }
    [Column("DimensiuniDebit_MaterialId")]
    public virtual Guid? DebitMaterialId { get; set; }
    [XafDisplayName("Material debit")]
    public virtual Produs DebitMaterial { get; set; }
    [Column("DimensiuniDebit_CodFunctionalId")]
    public virtual Guid? DebitCodFunctionalId { get; set; }
    [XafDisplayName("Cod funcțional debit")]
    public virtual CodFunctional DebitCodFunctional { get; set; }
    [Column("DimensiuniDebit_CodEconomicId")]
    public virtual Guid? DebitCodEconomicId { get; set; }
    [XafDisplayName("Cod economic debit")]
    public virtual CodEconomic DebitCodEconomic { get; set; }
    [Column("DimensiuniDebit_SursaFinantareId")]
    public virtual Guid? DebitSursaFinantareId { get; set; }
    [XafDisplayName("Sursă finanțare debit")]
    public virtual SursaFinantare DebitSursaFinantare { get; set; }
    [Column("DimensiuniDebit_UnitateId")]
    public virtual Guid? DebitUnitateId { get; set; }
    [XafDisplayName("Unitate debit")]
    public virtual Unitate DebitUnitate { get; set; }
    [Column("DimensiuniDebit_ProiectId")]
    public virtual Guid? DebitProiectId { get; set; }
    [XafDisplayName("Proiect debit")]
    public virtual Proiect DebitProiect { get; set; }
    [Column("DimensiuniDebit_CentruCostId")]
    public virtual Guid? DebitCentruCostId { get; set; }
    [XafDisplayName("Centru cost debit")]
    public virtual Repartitor DebitCentruCost { get; set; }

    [Column("DimensiuniCredit_RepartitorId")]
    public virtual Guid? CreditRepartitorId { get; set; }
    [XafDisplayName("Repartitor credit")]
    public virtual Repartitor CreditRepartitor { get; set; }
    [Column("DimensiuniCredit_MaterialId")]
    public virtual Guid? CreditMaterialId { get; set; }
    [XafDisplayName("Material credit")]
    public virtual Produs CreditMaterial { get; set; }
    [Column("DimensiuniCredit_CodFunctionalId")]
    public virtual Guid? CreditCodFunctionalId { get; set; }
    [XafDisplayName("Cod funcțional credit")]
    public virtual CodFunctional CreditCodFunctional { get; set; }
    [Column("DimensiuniCredit_CodEconomicId")]
    public virtual Guid? CreditCodEconomicId { get; set; }
    [XafDisplayName("Cod economic credit")]
    public virtual CodEconomic CreditCodEconomic { get; set; }
    [Column("DimensiuniCredit_SursaFinantareId")]
    public virtual Guid? CreditSursaFinantareId { get; set; }
    [XafDisplayName("Sursă finanțare credit")]
    public virtual SursaFinantare CreditSursaFinantare { get; set; }
    [Column("DimensiuniCredit_UnitateId")]
    public virtual Guid? CreditUnitateId { get; set; }
    [XafDisplayName("Unitate credit")]
    public virtual Unitate CreditUnitate { get; set; }
    [Column("DimensiuniCredit_ProiectId")]
    public virtual Guid? CreditProiectId { get; set; }
    [XafDisplayName("Proiect credit")]
    public virtual Proiect CreditProiect { get; set; }
    [Column("DimensiuniCredit_CentruCostId")]
    public virtual Guid? CreditCentruCostId { get; set; }
    [XafDisplayName("Centru cost credit")]
    public virtual Repartitor CreditCentruCost { get; set; }

    // Puntea spre value object-ul motorului: citire (storno, proiecții) și
    // scriere (materializarea notelor) — metode, nu proprietăți (XAF nu are
    // ce căuta pe ele, ca GetContrapartidaId).
    public Dimensiuni DimensiuniDebit() => new() {
        RepartitorId = DebitRepartitorId, MaterialId = DebitMaterialId,
        CodFunctionalId = DebitCodFunctionalId, CodEconomicId = DebitCodEconomicId,
        SursaFinantareId = DebitSursaFinantareId, UnitateId = DebitUnitateId,
        ProiectId = DebitProiectId, CentruCostId = DebitCentruCostId
    };
    public Dimensiuni DimensiuniCredit() => new() {
        RepartitorId = CreditRepartitorId, MaterialId = CreditMaterialId,
        CodFunctionalId = CreditCodFunctionalId, CodEconomicId = CreditCodEconomicId,
        SursaFinantareId = CreditSursaFinantareId, UnitateId = CreditUnitateId,
        ProiectId = CreditProiectId, CentruCostId = CreditCentruCostId
    };
    public void AplicaDimensiuniDebit(Dimensiuni d) {
        DebitRepartitorId = d.RepartitorId; DebitMaterialId = d.MaterialId;
        DebitCodFunctionalId = d.CodFunctionalId; DebitCodEconomicId = d.CodEconomicId;
        DebitSursaFinantareId = d.SursaFinantareId; DebitUnitateId = d.UnitateId;
        DebitProiectId = d.ProiectId; DebitCentruCostId = d.CentruCostId;
    }
    public void AplicaDimensiuniCredit(Dimensiuni d) {
        CreditRepartitorId = d.RepartitorId; CreditMaterialId = d.MaterialId;
        CreditCodFunctionalId = d.CodFunctionalId; CreditCodEconomicId = d.CodEconomicId;
        CreditSursaFinantareId = d.SursaFinantareId; CreditUnitateId = d.UnitateId;
        CreditProiectId = d.ProiectId; CreditCentruCostId = d.CentruCostId;
    }
    // Rând de stornare (inversul unui rând de operare, la data stornării).
    public virtual bool Storno { get; set; }
    // Null = rând de deschidere scris de migrare (decizia 12), fără document sursă.
    public virtual Guid? DocumentId { get; set; }
    public virtual Document Document { get; set; }
    public virtual Guid? DetaliuId { get; set; }
    public virtual DocumentDetaliu Detaliu { get; set; }
}
