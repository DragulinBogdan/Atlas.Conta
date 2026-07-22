using System.ComponentModel.DataAnnotations.Schema;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF;

namespace Atlas.Conta.BackOffice.Module.BusinessObjects;

// FCT (01): predator = Partener (furnizor), primitor = Gestiune; nu mișcă stoc —
// intrarea o face NIR-ul conex. Import extern (Tethys) = cale de primă clasă.
public class FacturaIntrare : Document, IDocumentCuScadenta, IDocumentCuPV {
    public virtual DateOnly? DataScadenta { get; set; }
    public virtual string NumarPV { get; set; }
    public virtual DateOnly? DataPV { get; set; }
    public virtual string CodCpv { get; set; }
    public virtual string TethysId { get; set; }

    public virtual string Valuta { get; set; }
    public virtual decimal? Curs { get; set; }

    // Fostul grup DECONT_* — parametrii generării documentului conex Plata;
    // câmpuri persistate (testul bazei §7.3).
    public virtual bool GenereazaPlata { get; set; }
    public virtual Guid? PlataContPropriuId { get; set; }
    public virtual ContPropriu PlataContPropriu { get; set; }
    public virtual string PlataNumar { get; set; }
    public virtual DateOnly? PlataData { get; set; }
    public virtual TipInstrumentPlata? PlataTipInstrument { get; set; }
    public virtual bool GenereazaChitanta { get; set; }
    public virtual string ChitantaNumar { get; set; }
    public virtual DateOnly? ChitantaData { get; set; }
}

public class FacturaIntrareDetaliu : DocumentDetaliu {
    // Lanțul de valori trăiește pe derivată (testul bazei §3); doar capătul
    // (Valoare din bază) intră în registre.
    public virtual decimal PretUnitar { get; set; }
    public virtual decimal CotaTva { get; set; }

    [NotMapped] public decimal PretReceptie => PretUnitar;
    [NotMapped] public decimal PretReceptieTva => PretReceptie * (1 + CotaTva / 100m);
    [NotMapped] public decimal ValoareReceptie => PretReceptie * Cantitate;
    [NotMapped] public decimal TvaReceptie => ValoareReceptie * CotaTva / 100m;

    public virtual string CodCpv { get; set; }

    // Atribute de lot culese la intrare; motorul le copiază pe Lot la operare.
    public virtual DateOnly? DataExpirare { get; set; }
    public virtual string LotFabricatie { get; set; }

    public void RecalculeazaValoare() => Valoare = ValoareReceptie + TvaReceptie;
}
