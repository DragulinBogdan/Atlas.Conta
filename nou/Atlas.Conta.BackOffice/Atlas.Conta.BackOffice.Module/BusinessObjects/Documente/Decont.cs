using System.ComponentModel.DataAnnotations.Schema;

namespace Atlas.Conta.BackOffice.Module.BusinessObjects;

// DEC (06): justificarea avansurilor; predator = Angajat (titularul). Fără stoc.
public class Decont : Document, IDocumentCuPV {
    public virtual string NumarPV { get; set; }
    public virtual DateOnly? DataPV { get; set; }
}

// Trăsătura PROPRIE a tipului (06, decizia 15 nuanțată): postarea explicită pe
// linie — cont și repartitor, ambele laturi — ca date de primă clasă, NU ca
// mecanism generic de override.
public class DecontDetaliu : DocumentDetaliu {
    public virtual string Descriere { get; set; }
    public virtual decimal PretUnitar { get; set; }
    public virtual decimal CotaTva { get; set; }

    public virtual Guid? ContDebitId { get; set; }
    public virtual Cont ContDebit { get; set; }
    public virtual Guid? ContCreditId { get; set; }
    public virtual Cont ContCredit { get; set; }
    public virtual Guid? RepartitorDebitId { get; set; }
    public virtual Repartitor RepartitorDebit { get; set; }
    public virtual Guid? RepartitorCreditId { get; set; }
    public virtual Repartitor RepartitorCredit { get; set; }

    public void RecalculeazaValoare() => Valoare = PretUnitar * Cantitate * (1 + CotaTva / 100m);
}
