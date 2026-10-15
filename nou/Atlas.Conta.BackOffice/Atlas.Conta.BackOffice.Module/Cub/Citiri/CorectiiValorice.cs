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

    /// <summary>Loturile cu corecții de valoare încă active, cu valoarea lor pe toate pozițiile lotului (CV-D10).</summary>
    public static Dictionary<Guid, Declaratii.CorectieActivaFapt> Active(IObjectSpace os, IReadOnlyCollection<Guid> loturi) {
        if (loturi.Count == 0) return [];
        var valori = Valorice(os, loturi).Where(p => p.Carte == N.Carte.Contabil)
            .GroupBy(p => p.Unitate!.Value)
            .Select(g => new { Lot = g.Key, Valoare = g.Sum(p => p.Sens == N.Sens.Debit ? p.Valoare : -p.Valoare) })
            .ToList().Where(g => g.Valoare != 0m).ToList();
        if (valori.Count == 0) return [];
        var corectate = valori.Select(v => v.Lot).ToList();
        var receptionate = os.GetObjectsQuery<BusinessObjects.Lot>().Where(l => corectate.Contains(l.ID))
            .Join(os.GetObjectsQuery<BusinessObjects.DocumentDetaliu>(), l => l.LinieIntrareId, d => (Guid?)d.ID,
                (l, d) => new { l.ID, d.Cantitate })
            .ToDictionary(l => l.ID, l => l.Cantitate);
        return valori.ToDictionary(v => v.Lot, v => new Declaratii.CorectieActivaFapt(v.Valoare,
            receptionate.TryGetValue(v.Lot, out var q) ? q
                : throw new OperareException($"Lotul corectat {v.Lot} n-are linie de intrare.")));
    }

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

    /// <summary>Pe un lot cu alocare activă, o mișcare cu cantitate nu e antedatată față de ea și nu poartă
    /// valoarea recepției în locul costului ajustat (CV-D5, CV-D10).</summary>
    public static void Verifica(IObjectSpace os, N.Contract contract, bool dinReceptie) {
        var miscari = contract.Tranzactii.SelectMany(t => t.Postari)
            .Where(p => p.Cantitate != 0m && p.Coordonate.Unitate is { Fel: N.FelUnitate.Lot })
            .Select(p => (Lot: p.Coordonate.Unitate!.Id, p.Coordonate.Data)).Distinct().ToList();
        if (miscari.Count == 0) return;
        var loturi = miscari.Select(m => m.Lot).Distinct().ToList();
        var postari = os.GetObjectsQuery<Postare>();
        var active = Valorice(os, loturi)
            .Where(p => p.Tranzactie.Fel == N.FelTranzactie.Operare
                && postari.Any(t => t.ID == p.Atribuit && t.Cantitate != 0m)
                && !postari.Any(i => i.InversaDinId == p.ID && i.InversaDinSpatiu == p.Spatiu))
            .Select(p => new { Lot = p.Unitate!.Value, p.Data }).ToList();
        if (dinReceptie && active.Count != 0)
            throw new OperareException($"{Declaratii.CoduriRefuz.CvDependenti}: Lotul are o corecție de valoare activă; "
                + "recepția nu se re-evaluează peste ea. Stornați întâi corecția.");
        if (miscari.Any(m => active.Any(a => a.Lot == m.Lot && a.Data > m.Data)))
            throw new OperareException($"{Declaratii.CoduriRefuz.CvCronologie}: Lotul are o corecție de valoare activă, "
                + "datată după această mișcare.");
    }

    sealed record Rand(Postare Postare, N.FelTranzactie Fel, DateTime ScrisLa);
}
