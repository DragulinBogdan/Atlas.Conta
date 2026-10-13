using Atlas.Conta.BackOffice.Module.Declaratii;
using Atlas.Conta.BackOffice.Module.Motor;
using DevExpress.ExpressApp;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.Module.Cub.Citiri;

public static class OriginiPartide {
    public static void Verifica(IObjectSpace os) {
        var origini = os.GetObjectsQuery<OriginePartidaPersistata>();
        var postari = Partide.Postari(os);
        var proprii = postari.Where(p => p.DocumentId != null
            && p.Unitate == Partide.Identitate(p.DocumentId.Value, p.Cont, p.Repartitor.Value)
            || p.DocumentId == null && p.Tranzactie.Fel == N.FelTranzactie.Deschidere);
        if (proprii.Any(p => !origini.Any(o => o.ID == p.Unitate && o.ContId == p.Cont
                && o.PartenerId == p.Repartitor && (o.DocumentId ?? Guid.Empty) == (p.DocumentId ?? Guid.Empty)))
            || origini.Any(o => !proprii.Any(p => p.Unitate == o.ID && p.Cont == o.ContId
                && p.Repartitor == o.PartenerId && (p.DocumentId ?? Guid.Empty) == (o.DocumentId ?? Guid.Empty))))
            Refuza("suport propriu lipsă sau proveniență diferită");
        var initiale = origini.Where(o => o.DocumentId == null).ToArray();
        foreach (var o in initiale)
            if (o.ReferintaInitiala == null || N.UnitateSold.DeschidePartidaInitiala(o.ContId, o.PartenerId,
                    o.ReferintaInitiala.Value, DateOnly.MinValue).Id != o.ID)
                Refuza("referință inițială diferită de identitate");
        if (origini.Any(o => o.DocumentId == null && !proprii.Any(p => p.Unitate == o.ID
                && p.DocumentId == null && p.Ordinal == o.OrdinalInitial)))
            Refuza("ordinal inițial diferit de postare");
    }

    static void Refuza(string motiv) => throw new OperareException($"{CoduriRefuz.OriginePartidaInconsistenta}: {motiv}.");
}
