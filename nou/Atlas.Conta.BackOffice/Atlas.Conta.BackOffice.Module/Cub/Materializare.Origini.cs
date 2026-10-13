using Atlas.Conta.BackOffice.Module.Declaratii;
using Atlas.Conta.BackOffice.Module.Motor;
using DevExpress.ExpressApp;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.Module.Cub;

public static partial class Materializare {
    static void ScrieOrigini(IObjectSpace os, IReadOnlyCollection<Postare> postari,
            IReadOnlyDictionary<Guid, Guid> referinteInitiale) {
        var eligibile = postari.Where(Citiri.Partide.EstePartida).ToArray();
        if (eligibile.Length == 0) return;
        var ids = eligibile.Select(p => p.Unitate.Value).Distinct().ToArray();
        var existente = os.GetObjectsQuery<OriginePartidaPersistata>().Where(o => ids.Contains(o.ID)).ToList()
            .Concat(os.ModifiedObjects.OfType<OriginePartidaPersistata>())
            .Where(o => !os.IsObjectToDelete(o) && !os.IsDeletedObject(o))
            .DistinctBy(o => o.ID).ToDictionary(o => o.ID);
        foreach (var p in eligibile) {
            var id = p.Unitate.Value;
            existente.TryGetValue(id, out var origine);
            if (origine != null && (origine.ContId != p.Cont || origine.PartenerId != p.Repartitor))
                RefuzaOrigine(id);
            var proprie = p.DocumentId is Guid doc && id == Citiri.Partide.Identitate(doc, p.Cont, p.Repartitor.Value);
            var initiala = p.DocumentId == null && p.Tranzactie.Fel == N.FelTranzactie.Deschidere;
            if (!proprie && !initiala) continue;
            Guid? referinta = null;
            int? ordinal = null;
            if (initiala) {
                if (referinteInitiale == null || !referinteInitiale.TryGetValue(id, out var r)
                    || N.UnitateSold.DeschidePartidaInitiala(p.Cont, p.Repartitor.Value, r, p.UnitateDeschisa.Value).Id != id)
                    RefuzaOrigine(id);
                referinta = referinteInitiale[id]; ordinal = p.Ordinal;
            }
            if (origine != null) {
                if (origine.DocumentId != p.DocumentId || origine.ReferintaInitiala != referinta
                    || origine.OrdinalInitial != ordinal) RefuzaOrigine(id);
                continue;
            }
            origine = os.CreateObject<OriginePartidaPersistata>();
            origine.ID = id; origine.ContId = p.Cont; origine.PartenerId = p.Repartitor.Value;
            origine.DocumentId = p.DocumentId; origine.ReferintaInitiala = referinta; origine.OrdinalInitial = ordinal;
            existente.Add(id, origine);
        }
    }

    static void RefuzaOrigine(Guid id) => throw new OperareException(
        $"{CoduriRefuz.OriginePartidaInconsistenta}: Originea partidei {id} nu corespunde faptelor comenzii.");

    static void RetrageOrigini(IObjectSpace os, Guid document, IReadOnlyCollection<Guid> tranzactii) {
        var ramase = Citiri.Partide.Postari(os).Where(p => p.DocumentId == document && !tranzactii.Contains(p.TranzactieId))
            .Select(p => p.Unitate.Value).ToHashSet();
        foreach (var p in os.ModifiedObjects.OfType<Postare>().Where(p => os.IsNewObject(p)
                && !os.IsObjectToDelete(p) && p.DocumentId == document && p.Unitate != null))
            ramase.Add(p.Unitate.Value);
        var origini = os.GetObjectsQuery<OriginePartidaPersistata>().Where(o => o.DocumentId == document).ToList()
            .Concat(os.ModifiedObjects.OfType<OriginePartidaPersistata>().Where(o => o.DocumentId == document))
            .DistinctBy(o => o.ID).ToArray();
        os.Delete(origini.Where(o => !ramase.Contains(o.ID) && !os.IsObjectToDelete(o)).ToArray());
    }
}
