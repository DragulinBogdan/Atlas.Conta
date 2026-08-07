namespace Atlas.Conta.BackOffice.Module.BusinessObjects;

// DIM-3 (decizia 54c): value object-ul motorului — setul de dimensiuni ca
// VALOARE, ne-persistat, în afara modelului EF. Trăiește doar în memorie:
// frunzele îl construiesc (DimensiuniCulese), DimensiuniResolver îl
// coalesce-ază, gardianul de dimensiuni îl verifică, registrul îl scrie în
// coloanele plate (AplicaDimensiuni*). Fostul owned type (deciziile 15/24,
// OwnedObjectBase + navigații + CreateProxy) a murit odată cu maparea.
public class Dimensiuni {
    public Guid? RepartitorId { get; set; }
    // Ținta „Material" = Produs (analitic de stoc, rezolvat implicit din lot — 33b).
    public Guid? MaterialId { get; set; }
    public Guid? CodFunctionalId { get; set; }
    public Guid? CodEconomicId { get; set; }
    public Guid? SursaFinantareId { get; set; }
    public Guid? UnitateId { get; set; }
    public Guid? ProiectId { get; set; }
    // Centru de cost = calitate pe repartitor (decizia 16), deci FK tot spre Repartitor.
    public Guid? CentruCostId { get; set; }
}
