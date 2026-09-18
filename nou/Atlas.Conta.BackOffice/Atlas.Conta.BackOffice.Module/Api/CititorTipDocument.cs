using Atlas.Conta.BackOffice.Module.BusinessObjects;
using DevExpress.ExpressApp;

namespace Atlas.Conta.BackOffice.Module.Api;

// Tipul unui document ca dată: discriminatorul `ClrType`, ancorat în `TipDocument.ClrType` (20, 89).
public static class CititorTipDocument {
    static readonly Lazy<Dictionary<string, Type>> claseConcrete = new(() => typeof(Document).Assembly.GetTypes()
        .Where(t => !t.IsAbstract && typeof(Document).IsAssignableFrom(t))
        .ToDictionary(t => t.Name, StringComparer.Ordinal));

    // id → discriminator, într-o singură proiecție; id-urile inexistente sau invizibile pe `os` lipsesc.
    public static Dictionary<Guid, string> Clase(IObjectSpace os, IReadOnlyCollection<Guid> ids) {
        if (ids == null || ids.Count == 0)
            return [];
        var cerute = ids.Distinct().ToList();
        return os.GetObjectsQuery<Document>()
            .Where(d => cerute.Contains(d.ID))
            .Select(d => new { d.ID, d.ClrType })
            .ToList()
            .ToDictionary(d => d.ID, d => d.ClrType);
    }

    // id → `TipDocument.Cod` (discriminatorul, dacă ancora lipsește); inexistent sau invizibil → null.
    public static Dictionary<Guid, string> Coduri(IObjectSpace os, IReadOnlyCollection<Guid> ids) {
        var rezultat = new Dictionary<Guid, string>();
        if (ids == null || ids.Count == 0)
            return rezultat;
        var clase = Clase(os, ids);
        var nume = clase.Values.Distinct().ToList();
        var codPeClasa = new Dictionary<string, string>(StringComparer.Ordinal);
        if (nume.Count > 0)
            foreach (var t in os.GetObjectsQuery<TipDocument>()
                         .Where(t => nume.Contains(t.ClrType))
                         .Select(t => new { t.ClrType, t.Cod })
                         .ToList())
                codPeClasa.TryAdd(t.ClrType, t.Cod);
        foreach (var id in ids)
            rezultat[id] = clase.TryGetValue(id, out var clasa)
                ? codPeClasa.TryGetValue(clasa, out var cod) ? cod : clasa
                : null;
        return rezultat;
    }

    // Clasa concretă a unui discriminator; null dacă nu există.
    public static Type Clasa(string clrType) =>
        clrType != null && claseConcrete.Value.TryGetValue(clrType, out var tip) ? tip : null;
}
