using Atlas.DXF.Core.Appearance.Attributes;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF;

namespace Atlas.Conta.BackOffice.Module.BusinessObjects;

// Al patrulea registru (F26-D2/D3); scris doar prin IDocumentCuRegistruPropriu.
[NavigationItem("Registre")]
[ForbidCRUD("ListView", "DetailView")]
[XafDisplayName("Registrul imobilizărilor")]
public class RegistruImobilizari : BaseObject {
    public virtual DateOnly Data { get; set; }
    public virtual Guid ImobilizareId { get; set; }
    public virtual Imobilizare Imobilizare { get; set; }
    [XafDisplayName("Fel")]
    public virtual FelMiscareImobilizare Fel { get; set; }

    [XafDisplayName("Valoare (brut contabil)")]
    public virtual decimal Valoare { get; set; }
    [XafDisplayName("Valoare fiscală (brut fiscal)")]
    public virtual decimal ValoareFiscala { get; set; }
    [XafDisplayName("Amortizare contabilă")]
    public virtual decimal Amortizare { get; set; }
    [XafDisplayName("Amortizare fiscală")]
    public virtual decimal AmortizareFiscala { get; set; }
    [XafDisplayName("Amortizare deductibilă")]
    public virtual decimal AmortizareDeductibila { get; set; }
    [XafDisplayName("Luni")]
    public virtual int Luni { get; set; }

    // Doar pe evenimente; null pe Amortizare/Iesire = „neschimbați” (F26-D2).
    [XafDisplayName("Metodă")]
    public virtual MetodaAmortizare? Metoda { get; set; }
    [XafDisplayName("Durată (luni)")]
    public virtual int? DurataLuni { get; set; }
    [XafDisplayName("Valoare reziduală")]
    public virtual decimal? ValoareReziduala { get; set; }
    [XafDisplayName("Metodă fiscală")]
    public virtual MetodaAmortizare? MetodaFiscala { get; set; }
    [XafDisplayName("Durată fiscală (luni)")]
    public virtual int? DurataFiscalaLuni { get; set; }
    [XafDisplayName("Categorie fiscală")]
    public virtual CategorieFiscala? CategorieFiscala { get; set; }
    [XafDisplayName("Utilizare exclusivă")]
    public virtual bool? UtilizareExclusiva { get; set; }

    // Locul la DATA faptului (F26-D8).
    public virtual Guid RepartitorId { get; set; }
    [XafDisplayName("Loc")]
    public virtual Repartitor Repartitor { get; set; }

    // NENULE: deschiderea e un PIF `Intrare` cu inițiale, deci are document (F26-r13).
    public virtual Guid DocumentId { get; set; }
    public virtual Document Document { get; set; }
    public virtual Guid DetaliuId { get; set; }
    public virtual DocumentDetaliu Detaliu { get; set; }

    public virtual bool Storno { get; set; }
}
