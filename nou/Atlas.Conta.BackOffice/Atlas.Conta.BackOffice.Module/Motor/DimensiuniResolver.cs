using Atlas.Conta.BackOffice.Module.BusinessObjects;

namespace Atlas.Conta.BackOffice.Module.Motor;

// Decizia 15: rezolvarea dimensiunilor e o funcție GENERICĂ — coalesce pe
// componente, prima sursă nenulă câștigă. Ordinea canonică per latură:
// linie → override(latură) → comun(regulă) → default header
// (debit ← Predator, credit ← Primitor — 00 §5).
public static class DimensiuniResolver {
    public static Dimensiuni Rezolva(params Dimensiuni[] surse) {
        var r = new Dimensiuni();
        foreach (var s in surse) {
            if (s == null)
                continue;
            r.RepartitorId ??= s.RepartitorId;
            r.MaterialId ??= s.MaterialId;
            r.CodFunctionalId ??= s.CodFunctionalId;
            r.CodEconomicId ??= s.CodEconomicId;
            r.SursaFinantareId ??= s.SursaFinantareId;
            r.UnitateId ??= s.UnitateId;
            r.ProiectId ??= s.ProiectId;
            r.CentruCostId ??= s.CentruCostId;
        }
        return r;
    }
}
