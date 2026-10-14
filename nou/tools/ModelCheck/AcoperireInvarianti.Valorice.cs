using Atlas.Conta.BackOffice.Module.BusinessObjects;
using DevExpress.ExpressApp;
using Microsoft.EntityFrameworkCore;
using C = Atlas.Conta.BackOffice.Module.Cub;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.ModelCheck;

// CV-D8: mutanții pe bază ai invariantului de audit al corecțiilor; proba pură îi are pe toți în `CorectieValoricaTeste`.
static partial class AcoperireInvarianti {
    static C.Explicatie PonderiInversate(C.Explicatie e) {
        var alocari = e.Decizii.OfType<N.AlocareValorica>().ToList();
        if (alocari.Count < 2 || alocari[0].Pondere == alocari[1].Pondere) return null;
        var (prima, aDoua) = (alocari[0], alocari[1]);
        return e with { Decizii = [.. e.Decizii.Select(d => ReferenceEquals(d, prima) ? prima with { Pondere = aDoua.Pondere }
            : ReferenceEquals(d, aDoua) ? aDoua with { Pondere = prima.Pondere } : d)] };
    }

    static C.Explicatie CompensareDiferita(C.Explicatie e) {
        var tinta = e.Decizii.OfType<N.CompensareValorica>().FirstOrDefault();
        return tinta == null ? null : e with {
            Decizii = [.. e.Decizii.Select(d => ReferenceEquals(d, tinta) ? tinta with { Valoare = tinta.Valoare + 1m } : d)],
        };
    }

    static bool LinieCorectieDiferita(IObjectSpace os, DbContext db) {
        foreach (var json in os.GetObjectsQuery<C.Tranzactie>().Where(t => t.Explicatie != null && t.Fel == N.FelTranzactie.Operare)
                .OrderBy(t => t.ID).Select(t => t.Explicatie).ToList()) {
            if (C.Explicatie.Citeste(json).Decizii.OfType<N.AlocareValorica>().FirstOrDefault() is not { } alocare) continue;
            return db.Set<DocumentDetaliu>().Where(l => l.ID == alocare.Linie)
                .ExecuteUpdate(s => s.SetProperty(l => l.Valoare, l => l.Valoare + 1m)) > 0;
        }
        return false;
    }

    static bool AtribuireStraina(IObjectSpace os, DbContext db) {
        var tinta = os.GetObjectsQuery<C.Postare>()
            .Where(p => p.FelUnitate == N.FelUnitate.Lot && p.Cantitate == 0m && p.Atribuit != null && p.Tranzactie.Fel == N.FelTranzactie.Operare)
            .OrderBy(p => p.ID).Select(p => new { p.ID, p.Spatiu }).FirstOrDefault();
        if (tinta == null) return false;
        var straina = Guid.NewGuid();
        db.Set<C.Postare>().Where(p => p.ID == tinta.ID && p.Spatiu == tinta.Spatiu)
            .ExecuteUpdate(s => s.SetProperty(p => p.Atribuit, straina));
        return true;
    }
}
