using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF;

namespace Atlas.Conta.BackOffice.Module.BusinessObjects;

// Decizia 4/20/21: structura e cod, politica e date. Rândurile de aici se
// SEED-uiesc pe funcționalitate (nu se transcriu din config-ul legacy).

// Ancoră seed care oglindește clasele 1:1 (decizia 20) — doar FK + UI.
[NavigationItem("Politici")]
[XafDefaultProperty(nameof(Denumire))]
public class TipDocument : BaseObject {
    public virtual string Cod { get; set; }
    public virtual string Denumire { get; set; }
    // Numele CLR al clasei derivate corespunzătoare (ex. "FacturaIntrare").
    public virtual string ClrType { get; set; }
}

// Regula de alimentare a registrului de stoc: tip document × latură × filtru
// Clasă → tip stoc + semn (00 §4, curățat: filtrul SEMN_ITEMS moare).
[NavigationItem("Politici")]
public class RegulaStoc : BaseObject {
    public virtual Guid TipDocumentId { get; set; }
    public virtual TipDocument TipDocument { get; set; }
    public virtual LaturaDocument Latura { get; set; }
    public virtual Guid? ClasaId { get; set; }
    public virtual ClasaProdus Clasa { get; set; }
    public virtual TipStoc TipStoc { get; set; }
    public virtual int Semn { get; set; }
}

// Maparea contabilă: tip document × Clasă/Tip → cont D/C + dimensiuni
// (decizia 15: Comun / OverrideDebit / OverrideCredit).
// Potrivirea pe linie (motor): TipMaterial exact → NaturaFiltru → regula
// generică (ambele null). Fără regulă potrivită = linia nu contează pe acest
// tip de document (așa se împarte lanțul FCT/NIR fără dublă postare).
[NavigationItem("Politici")]
public class RegulaContare : BaseObject {
    public virtual Guid TipDocumentId { get; set; }
    public virtual TipDocument TipDocument { get; set; }
    public virtual Guid? TipMaterialId { get; set; }
    public virtual TipMaterial TipMaterial { get; set; }
    // Filtru pe natura Clasei liniei (ex. FCT contează DOAR non-stoc — recepția
    // o postează NIR-ul); mai slab decât potrivirea exactă pe TipMaterial.
    public virtual NaturaClasa? NaturaFiltru { get; set; }
    // Conturile se rezolvă declarativ per latură (testul bazei §7.2): sursa
    // indică nomenclatorul purtător (Tip material / partenerul unei laturi),
    // iar contul explicit de mai jos rămâne valoare directă sau fallback.
    public virtual SursaCont SursaContDebit { get; set; }
    public virtual SursaCont SursaContCredit { get; set; }
    public virtual Guid? ContDebitId { get; set; }
    public virtual Cont ContDebit { get; set; }
    public virtual Guid? ContCreditId { get; set; }
    public virtual Cont ContCredit { get; set; }
    public virtual Dimensiuni DimensiuniComun { get; set; } = new();
    public virtual Dimensiuni DimensiuniOverrideDebit { get; set; } = new();
    public virtual Dimensiuni DimensiuniOverrideCredit { get; set; } = new();
}

// Documentul conex (decizia 17, 00 §6): sursă → țintă + filtrul de conținut
// (ce linii trec pe documentul generat). Filtrul legacy enumera tipuri de
// material per defa (GEST_ITEMSI_TIP_MATERIAL); funcțional criteriul e natura
// liniei (pe NIR trec exact liniile purtătoare de stoc) — decizia 21: politica
// se definește pe funcționalitate, nu prin transcrierea listelor legacy.
[NavigationItem("Politici")]
public class PoliticaConex : BaseObject {
    public virtual Guid TipDocumentSursaId { get; set; }
    public virtual TipDocument TipDocumentSursa { get; set; }
    public virtual Guid TipDocumentTintaId { get; set; }
    public virtual TipDocument TipDocumentTinta { get; set; }
    // TIP_DESCARCARE legacy: ținta inversează laturile (predator ↔ primitor).
    public virtual bool InverseazaLaturi { get; set; }
    // Null = trec toate liniile; altfel doar cele cu Clasa.Natura potrivită.
    public virtual NaturaClasa? NaturaFiltru { get; set; }
}

[NavigationItem("Politici")]
public class PoliticaNumerotare : BaseObject {
    public virtual Guid TipDocumentId { get; set; }
    public virtual TipDocument TipDocument { get; set; }
    public virtual string Serie { get; set; }
    public virtual int UrmatorulNumar { get; set; }
    public virtual string Format { get; set; }
}
