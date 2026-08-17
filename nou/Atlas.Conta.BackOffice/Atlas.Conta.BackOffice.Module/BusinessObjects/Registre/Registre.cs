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

// AL TREILEA REGISTRU (felia 11, JT-D1) — faptele fiscale ale operațiunilor
// taxabile, un rând per LINIE de document, scris de motor în aceeași tranzacție
// cu celelalte două.
//
// DE CE nu e o proiecție peste `RegistruContabil`: rândul contabil de TVA
// poartă valoarea și conturile, dar NU baza impozabilă și NU tipul de TVA —
// jurnalul le cere pe amândouă, pe fiecare rând. Mai grav, jumătate din jurnal
// nu postează deloc: liniile `Scutit`, `Neimpozabil` și `Capitalizat` (achiziție
// fără drept de deducere) nu produc niciun rând 4426/4427 — motorul le sare
// explicit — dar apar legal în jurnalul de cumpărări/vânzări și în D300. O
// proiecție peste rândurile de TVA le-ar pierde TĂCUT, adică ar fi un raport
// incomplet cu aparență de raport complet. A treia cale (proiecție peste
// `DocumentDetaliu`) ar fi fost gratuită, dar ar fi produs al doilea adevăr:
// niciun raport n-ar mai fi avut legătură structurală cu soldul lui 4426/4427,
// iar starea documentelor (Draft/Operat/Stornat) ar fi trebuit reimplementată în
// proiecție. Registrul se închide pe cifra contabilă PRIN CONSTRUCȚIE (JT-D6:
// per document, Σ Tva al regimurilor care postează == Σ Valoare a rândurilor lui
// din `RegistruContabil` pe conturile de TVA).
//
// DE CE nu are rânduri de deschidere: celelalte două registre au excepția
// declarată `DocumentId = null` (soldurile scrise de migrare — 25e/34d). Aici nu
// există, iar diferența e de natură, nu de scop: soldul lui 4426/4427 la
// deschidere e o POZIȚIE CONTABILĂ, nu o OPERAȚIUNE TAXABILĂ, iar un jurnal de
// TVA listează operațiuni. De aceea `DocumentId`/`DetaliuId` sunt NENULE — ceea
// ce face cusătura JT-D6 exactă, nu aproximativă.
//
// DE CE `Regim` și `Cota` se copiază pe rând, iar denumirea partenerului,
// CodFiscal-ul și codurile SAF-T nu: **snapshot pentru ce a intrat în calcul,
// join pentru ce doar se afișează** (JT-D3). `TipTva` e nomenclator editabil —
// o cotă corectată în 2026 nu are voie să rescrie jurnalul lui 2025 (invariantul
// III: rândul de registru se scrie o dată, complet rezolvat). Etichetele, în
// schimb, se rezolvă la citire (ca `ContSimbol` în balanță), iar codurile ANAF
// sunt ele însele nomenclator care se schimbă cu anul de raportare: o declarație
// se generează cu nomenclatorul în vigoare atunci, nu cu cel de la operare.
//
// Per LINIE, nu per document: SAF-T cere nivelul liniei și motorul lucrează
// oricum acolo — agregarea la document e gratuită în proiecție (JT-D7), inversul
// n-ar fi fost.
[NavigationItem("Registre")]
[ForbidCRUD("ListView", "DetailView")]
public class RegistruTva : BaseObject {
    public virtual DateOnly Data { get; set; }
    // Snapshot al laturii (vezi `SensTva`): derivat din `PoliticaTva.Directie`.
    public virtual SensTva Sens { get; set; }
    // NENULE — vezi mai sus: registrul n-are rânduri de deschidere.
    public virtual Guid DocumentId { get; set; }
    public virtual Document Document { get; set; }
    public virtual Guid DetaliuId { get; set; }
    public virtual DocumentDetaliu Detaliu { get; set; }
    // Contrapartida laturii din `PoliticaTva.SursaContrapartida`. Tipul e BAZA
    // `Repartitor`, nu `Partener`: pe `Decont` latura e ANGAJATUL (titularul de
    // avans), nu comerciantul de pe bon — jurnalul arată onest ce știe modelul.
    // Nullable fiindcă `SursaContrapartida` poate fi `Explicit`/`TipMaterial`,
    // adică nu o latură: gaura se raportează, nu se refuză (riscul 4 din design
    // — un refuz ar face documentul neoperabil pentru o preocupare strict de
    // RAPORTARE, iar postarea contabilă a aceluiași rând merge perfect,
    // contrapartida rezolvându-se din contul fallback).
    public virtual Guid? PartenerId { get; set; }
    public virtual Repartitor Partener { get; set; }
    // Identitatea fiscală a liniei; NENULĂ prin criteriul de generare (JT-D2:
    // politica tipului ȘI `TipTvaId` pe linie).
    public virtual Guid TipTvaId { get; set; }
    public virtual TipTva TipTva { get; set; }
    // SNAPSHOT (JT-D3) — au intrat în aritmetica bazei și a TVA-ului.
    [XafDisplayName("Regim TVA")]
    public virtual RegimTva Regim { get; set; }
    [XafDisplayName("Cotă")]
    public virtual decimal Cota { get; set; }
    // Cifrele rezolvate (JT-D4): la `Capitalizat` baza se desface înapoi din
    // valoarea brută; la `Scutit`/`Neimpozabil` TVA-ul e 0, dar baza există.
    [XafDisplayName("Bază impozabilă")]
    public virtual decimal Baza { get; set; }
    [XafDisplayName("TVA")]
    public virtual decimal Tva { get; set; }
    // Rând de stornare (inversul unui rând de operare, la data stornării).
    // Jurnalul nu filtrează NICIODATĂ `Storno`: registrul e append-only și suma
    // lui algebrică e adevărul (R-D7).
    public virtual bool Storno { get; set; }
}
