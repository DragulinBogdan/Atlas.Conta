#nullable enable
using Atlas.Conta.BackOffice.Module.Motor;
using DevExpress.ExpressApp;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.Module.Cub.Citiri;

/// <summary>Faptele loturilor pe care le judecă corecția valorică (CV-D10).</summary>
public static class CorectiiValorice {
    static IQueryable<Postare> Valorice(IObjectSpace os, IReadOnlyCollection<Guid> loturi) =>
        os.GetObjectsQuery<Postare>().Where(p => p.Spatiu == N.Spatiu.Stoc && p.FelUnitate == N.FelUnitate.Lot
            && p.Unitate != null && loturi.Contains(p.Unitate.Value) && p.Cantitate == 0m && p.Atribuit != null);

    /// <summary>Cel puțin un lot poartă o alocare, o compensare sau inversa lor.</summary>
    public static bool Exista(IObjectSpace os, IReadOnlyCollection<Guid> loturi) =>
        loturi.Count != 0 && Valorice(os, loturi).Any();

    /// <summary>Loturile pe care documentul are postări și care poartă corecții de valoare, într-o singură citire.</summary>
    public static List<Guid> LoturiCorectate(IObjectSpace os, Guid document) {
        var proprii = os.GetObjectsQuery<Postare>()
            .Where(p => p.DocumentId == document && p.FelUnitate == N.FelUnitate.Lot && p.Unitate != null)
            .Select(p => p.Unitate!.Value);
        return [.. os.GetObjectsQuery<Postare>().Where(p => p.Spatiu == N.Spatiu.Stoc && p.FelUnitate == N.FelUnitate.Lot
                && p.Unitate != null && p.Cantitate == 0m && p.Atribuit != null && proprii.Contains(p.Unitate.Value))
            .Select(p => p.Unitate!.Value).Distinct()];
    }

    /// <summary>Toate postările loturilor, cu contrapicioarele perechilor lor din afara lotului.</summary>
    public static List<N.FaptPostat> Fapte(IObjectSpace os, IReadOnlyCollection<Guid> loturi, Guid? faraDocument = null) {
        if (loturi.Count == 0) return [];
        var peLot = os.GetObjectsQuery<Postare>().Where(p => p.Spatiu == N.Spatiu.Stoc
            && p.FelUnitate == N.FelUnitate.Lot && p.Unitate != null && loturi.Contains(p.Unitate.Value));
        if (faraDocument is Guid document) peLot = peLot.Where(p => p.DocumentId != document);
        var randuri = peLot.Select(p => new Rand(p, p.Tranzactie.Fel, p.Tranzactie.ScrisLa)).ToList();
        var ids = randuri.Select(r => r.Postare.ID).ToHashSet();
        var perechi = randuri.Where(r => r.Postare.Pereche != null)
            .Select(r => (r.Postare.TranzactieId, r.Postare.Pereche!.Value)).ToHashSet();
        var tranzactii = perechi.Select(p => p.TranzactieId).Distinct().ToList();
        randuri.AddRange(os.GetObjectsQuery<Postare>()
            .Where(p => tranzactii.Contains(p.TranzactieId) && p.Pereche != null && p.Spatiu == N.Spatiu.Contabil)
            .Select(p => new Rand(p, p.Tranzactie.Fel, p.Tranzactie.ScrisLa)).ToList()
            .Where(r => perechi.Contains((r.Postare.TranzactieId, r.Postare.Pereche!.Value)) && ids.Add(r.Postare.ID)));
        var repartitori = Randuri.Repartitori(os, randuri.Select(r => r.Postare));
        return [.. randuri.Select(r => new N.FaptPostat(r.Postare.ID, r.Postare.TranzactieId, r.Fel, r.ScrisLa,
            r.Postare.Ordinal, Randuri.Citeste(r.Postare, repartitori)))];
    }

    /// <summary>O mișcare cu cantitate nu se operează înaintea unei alocări active a lotului ei (CV-D5).</summary>
    public static void VerificaCronologia(IObjectSpace os, IEnumerable<N.Postare> propuse) {
        var miscari = propuse.Where(p => p.Cantitate != 0m && p.Coordonate.Unitate is { Fel: N.FelUnitate.Lot })
            .Select(p => (Lot: p.Coordonate.Unitate!.Id, p.Coordonate.Data)).Distinct().ToList();
        if (miscari.Count == 0) return;
        var loturi = miscari.Select(m => m.Lot).Distinct().ToList();
        var prima = miscari.Min(m => m.Data);
        var postari = os.GetObjectsQuery<Postare>();
        var active = Valorice(os, loturi)
            .Where(p => p.Tranzactie.Fel == N.FelTranzactie.Operare && p.Data > prima
                && postari.Any(t => t.ID == p.Atribuit && t.Cantitate != 0m)
                && !postari.Any(i => i.InversaDinId == p.ID && i.InversaDinSpatiu == p.Spatiu))
            .Select(p => new { Lot = p.Unitate!.Value, p.Data }).ToList();
        if (miscari.Any(m => active.Any(a => a.Lot == m.Lot && a.Data > m.Data)))
            throw new OperareException($"{N.Coduri.CvCronologie}: Lotul are o corecție de valoare activă, "
                + "datată după această mișcare.");
    }

    sealed record Rand(Postare Postare, N.FelTranzactie Fel, DateTime ScrisLa);
}
