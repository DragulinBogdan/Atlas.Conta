using System.Globalization;
using System.Text.Json;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.EFCore;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Atlas.Conta.BackOffice.Module.BusinessObjects;

/// <summary>Cheia unui rând `ICuProvenienta` șters: seed-ul nu o mai recreează până nu se șterge refuzul (104i, 83a/j).</summary>
[NavigationItem("Configurare")]
[XafDisplayName("Rând de seed refuzat")]
[XafDefaultProperty(nameof(Cheie))]
public class RefuzSeed : Politica {
    [ModelDefault("AllowEdit", "False")]
    public virtual string Tip { get; set; }
    [ModelDefault("AllowEdit", "False")]
    public virtual string Cheie { get; set; }
    [ModelDefault("AllowEdit", "False")]
    [XafDisplayName("Refuzat la")]
    public virtual DateTime La { get; set; }

    /// <summary>Refuzul pentru rândul dat, dacă nu există deja (în bază sau în aceeași tranzacție).</summary>
    public static void Inregistreaza(IObjectSpace os, ICuProvenienta rand) {
        var tip = rand.GetType();
        var cheie = CheiaRandului(os, rand);
        var nume = NumeTip(os, tip);
        if (Refuzat(os, nume, cheie))
            return;
        var refuz = os.CreateObject<RefuzSeed>();
        refuz.Tip = nume;
        refuz.Cheie = cheie;
        refuz.La = DateTime.UtcNow;
    }

    public static bool Refuzat(IObjectSpace os, ICuProvenienta rand) =>
        Refuzat(os, NumeTip(os, rand.GetType()), CheiaRandului(os, rand));

    /// <summary>Valorile de cheie ale refuzurilor pe tipul dat, pe numele coloanei.</summary>
    public static IReadOnlyList<IReadOnlyDictionary<string, string>> Chei<T>(IObjectSpace os) where T : ICuProvenienta {
        var nume = typeof(T).Name;
        return os.GetObjectsQuery<RefuzSeed>().Where(r => r.Tip == nume).Select(r => r.Cheie).ToList()
            .Select(c => (IReadOnlyDictionary<string, string>)JsonSerializer.Deserialize<Dictionary<string, string>>(c))
            .ToList();
    }

    static bool Refuzat(IObjectSpace os, string tip, string cheie) =>
        os.ModifiedObjects.OfType<RefuzSeed>().Any(r => r.Tip == tip && r.Cheie == cheie && !os.IsObjectToDelete(r))
        || os.GetObjectsQuery<RefuzSeed>().Any(r => r.Tip == tip && r.Cheie == cheie);

    static string NumeTip(IObjectSpace os, Type tip) => Entitate(os, tip).ClrType.Name;

    /// <summary>Coloanele indexurilor unice (83b), ordonate: identitatea rândului de seed.</summary>
    public static IReadOnlyList<IProperty> ColoaneCheie(IObjectSpace os, Type tip) =>
        Entitate(os, tip).GetIndexes().Where(i => i.IsUnique).SelectMany(i => i.Properties)
            .Distinct().OrderBy(p => p.Name, StringComparer.Ordinal).ToList();

    static IEntityType Entitate(IObjectSpace os, Type tip) {
        var model = (os as EFCoreObjectSpace)?.DbContext.GetService<IDesignTimeModel>().Model
            ?? throw new InvalidOperationException("Cheia de seed cere un EFCoreObjectSpace (modelul design-time) — 83b.");
        for (var t = tip; t != null; t = t.BaseType)
            if (model.FindEntityType(t) is { } entitate)
                return entitate;
        throw new InvalidOperationException($"{tip.Name} nu e în model.");
    }

    static string CheiaRandului(IObjectSpace os, object rand) {
        var coloane = ColoaneCheie(os, rand.GetType());
        if (coloane.Count == 0)
            throw new InvalidOperationException(
                $"{rand.GetType().Name} n-are index unic: refuzul de seed n-are cheie (104i).");
        var valori = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (var p in coloane)
            valori[p.Name] = Text(Valoare(p, rand));
        return JsonSerializer.Serialize(valori);
    }

    // Navigația setată fără FK scalar (rând nou, încă nesincronizat de EF) dă cheia principalului.
    static object Valoare(IProperty p, object rand) {
        var valoare = p.PropertyInfo.GetValue(rand);
        var gol = valoare == null || valoare is Guid g && g == Guid.Empty;
        if (gol && p.GetContainingForeignKeys().FirstOrDefault()?.DependentToPrincipal?.PropertyInfo?.GetValue(rand)
                is EntitateConta principal)
            return principal.ID;
        return valoare;
    }

    static string Text(object valoare) => valoare switch {
        null => null,
        DateOnly d => d.ToString("O", CultureInfo.InvariantCulture),
        IFormattable f => f.ToString(null, CultureInfo.InvariantCulture),
        _ => valoare.ToString(),
    };
}
