using Atlas.DXF.EfCore.Owned;

namespace Atlas.Conta.BackOffice.Module.BusinessObjects;

// Owned type (decizia 15): linia de document poartă un set PARȚIAL (tot nullable),
// rândul de registru poartă unul rezolvat; regula de contare poartă
// Comun/OverrideDebit/OverrideCredit. Rezolvarea (coalesce linie → regulă → header)
// e o funcție generică în motor.
//
// OwnedObjectBase (decizia 24): DbContext-ul XAF cere notificări complete pe toate
// tipurile, dar owned-ul se instanțiază cu `new()` (POCO, nu proxy EF) — baza din
// Atlas.DXF implementează INotifyPropertyChanging/Changed; aici rămâne doar forma
// backing-field + SetPropertyValue per proprietate.
public class Dimensiuni : OwnedObjectBase {
    private Guid? repartitorId;
    public virtual Guid? RepartitorId { get => repartitorId; set => SetPropertyValue(ref repartitorId, value); }
    private Repartitor repartitor;
    public virtual Repartitor Repartitor { get => repartitor; set => SetPropertyValue(ref repartitor, value); }

    // Ținta „Material" = Produs (analitic de stoc); de confirmat la motor.
    private Guid? materialId;
    public virtual Guid? MaterialId { get => materialId; set => SetPropertyValue(ref materialId, value); }
    private Produs material;
    public virtual Produs Material { get => material; set => SetPropertyValue(ref material, value); }

    private Guid? codFunctionalId;
    public virtual Guid? CodFunctionalId { get => codFunctionalId; set => SetPropertyValue(ref codFunctionalId, value); }
    private CodFunctional codFunctional;
    public virtual CodFunctional CodFunctional { get => codFunctional; set => SetPropertyValue(ref codFunctional, value); }

    private Guid? codEconomicId;
    public virtual Guid? CodEconomicId { get => codEconomicId; set => SetPropertyValue(ref codEconomicId, value); }
    private CodEconomic codEconomic;
    public virtual CodEconomic CodEconomic { get => codEconomic; set => SetPropertyValue(ref codEconomic, value); }

    private Guid? sursaFinantareId;
    public virtual Guid? SursaFinantareId { get => sursaFinantareId; set => SetPropertyValue(ref sursaFinantareId, value); }
    private SursaFinantare sursaFinantare;
    public virtual SursaFinantare SursaFinantare { get => sursaFinantare; set => SetPropertyValue(ref sursaFinantare, value); }

    private Guid? unitateId;
    public virtual Guid? UnitateId { get => unitateId; set => SetPropertyValue(ref unitateId, value); }
    private Unitate unitate;
    public virtual Unitate Unitate { get => unitate; set => SetPropertyValue(ref unitate, value); }

    private Guid? proiectId;
    public virtual Guid? ProiectId { get => proiectId; set => SetPropertyValue(ref proiectId, value); }
    private Proiect proiect;
    public virtual Proiect Proiect { get => proiect; set => SetPropertyValue(ref proiect, value); }

    // Centru de cost = calitate pe repartitor (decizia 16), deci FK tot spre Repartitor.
    private Guid? centruCostId;
    public virtual Guid? CentruCostId { get => centruCostId; set => SetPropertyValue(ref centruCostId, value); }
    private Repartitor centruCost;
    public virtual Repartitor CentruCost { get => centruCost; set => SetPropertyValue(ref centruCost, value); }

    // Enumerarea compactă a dimensiunilor SETATE, pentru celulele de grid și
    // lookup-uri (înlocuiește ToString-ul de tip din OwnedObjectBase). CRUCIAL:
    // navigația se citește DOAR când FK-ul scalar e non-null — altfel accesul la
    // proprietatea virtuală ar declanșa un lazy-load inutil pe dimensiunea goală
    // (owned-ul materializat e proxy). Toate null → string gol.
    public override string ToString() {
        var parti = new List<string>(8);
        if (repartitorId != null) parti.Add("R:" + Eticheta(Repartitor));
        if (materialId != null) parti.Add("M:" + Eticheta(Material));
        if (codFunctionalId != null) parti.Add("CF:" + Eticheta(CodFunctional));
        if (codEconomicId != null) parti.Add("CE:" + Eticheta(CodEconomic));
        if (sursaFinantareId != null) parti.Add("F:" + Eticheta(SursaFinantare));
        if (unitateId != null) parti.Add("U:" + Eticheta(Unitate));
        if (proiectId != null) parti.Add("P:" + Eticheta(Proiect));
        if (centruCostId != null) parti.Add("CC:" + Eticheta(CentruCost));
        return string.Join("; ", parti);
    }

    // DIM-1 (decizia 54c): copie detașată ca VALUE OBJECT pentru motor — POCO
    // simplu, fără legătură cu instanța owned persistată. Doar FK-urile scalare;
    // navigațiile nu se ating (ar declanșa lazy-load).
    public Dimensiuni Copie() => new() {
        RepartitorId = RepartitorId,
        MaterialId = MaterialId,
        CodFunctionalId = CodFunctionalId,
        CodEconomicId = CodEconomicId,
        SursaFinantareId = SursaFinantareId,
        UnitateId = UnitateId,
        ProiectId = ProiectId,
        CentruCostId = CentruCostId
    };

    public void CopiazaDin(Dimensiuni s) {
        RepartitorId = s.RepartitorId; MaterialId = s.MaterialId;
        CodFunctionalId = s.CodFunctionalId; CodEconomicId = s.CodEconomicId;
        SursaFinantareId = s.SursaFinantareId; UnitateId = s.UnitateId;
        ProiectId = s.ProiectId; CentruCostId = s.CentruCostId;
    }

    // Toate țintele au un `Cod` scurt natural; cădem pe ToString() dacă lipsește
    // (nav nerezolvat sau cod gol).
    static string Eticheta(Repartitor r) => !string.IsNullOrWhiteSpace(r?.Cod) ? r.Cod : r?.ToString() ?? "";
    static string Eticheta(Produs p) => !string.IsNullOrWhiteSpace(p?.Cod) ? p.Cod : p?.ToString() ?? "";
    static string Eticheta(CodFunctional c) => !string.IsNullOrWhiteSpace(c?.Cod) ? c.Cod : c?.ToString() ?? "";
    static string Eticheta(CodEconomic c) => !string.IsNullOrWhiteSpace(c?.Cod) ? c.Cod : c?.ToString() ?? "";
    static string Eticheta(SursaFinantare s) => !string.IsNullOrWhiteSpace(s?.Cod) ? s.Cod : s?.ToString() ?? "";
    static string Eticheta(Unitate u) => !string.IsNullOrWhiteSpace(u?.Cod) ? u.Cod : u?.ToString() ?? "";
    static string Eticheta(Proiect p) => !string.IsNullOrWhiteSpace(p?.Cod) ? p.Cod : p?.ToString() ?? "";
}
