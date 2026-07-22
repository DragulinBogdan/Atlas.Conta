namespace Atlas.Conta.BackOffice.Module.BusinessObjects;

// Owned type (decizia 15): linia de document poartă un set PARȚIAL (tot nullable),
// rândul de registru poartă unul rezolvat; regula de contare poartă
// Comun/OverrideDebit/OverrideCredit. Rezolvarea (coalesce linie → regulă → header)
// e o funcție generică în motor.
public class Dimensiuni {
    public virtual Guid? RepartitorId { get; set; }
    public virtual Repartitor Repartitor { get; set; }

    // Ținta „Material" = Produs (analitic de stoc); de confirmat la motor.
    public virtual Guid? MaterialId { get; set; }
    public virtual Produs Material { get; set; }

    public virtual Guid? CodFunctionalId { get; set; }
    public virtual CodFunctional CodFunctional { get; set; }

    public virtual Guid? CodEconomicId { get; set; }
    public virtual CodEconomic CodEconomic { get; set; }

    public virtual Guid? SursaFinantareId { get; set; }
    public virtual SursaFinantare SursaFinantare { get; set; }

    public virtual Guid? UnitateId { get; set; }
    public virtual Unitate Unitate { get; set; }

    public virtual Guid? ProiectId { get; set; }
    public virtual Proiect Proiect { get; set; }

    // Centru de cost = calitate pe repartitor (decizia 16), deci FK tot spre Repartitor.
    public virtual Guid? CentruCostId { get; set; }
    public virtual Repartitor CentruCost { get; set; }
}
