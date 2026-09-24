#nullable enable
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Microsoft.EntityFrameworkCore;
using C = Atlas.Conta.BackOffice.Module.Cub;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.ModelCheck;

static class DiagnosticValoriStoc {
    internal sealed record Cheie(Guid Lot, Guid Gestiune, Guid? Cont);
    internal sealed record CheieMiscare(Guid Lot, Guid Gestiune, Guid? Document, DateOnly Data, bool Storno);
    internal sealed record CheieRegistru(Guid Lot, Guid Gestiune, TipStoc Tip, Guid? Document, bool Storno);
    internal sealed record Repartitie(CheieRegistru Cheie, decimal Q, decimal V);
    internal sealed record Fapt(Cheie Cheie, DateOnly Data, Guid? Document, Guid? Linie,
        bool Storno, decimal Q, decimal V, bool Cub, string Provenienta);
    internal sealed record Contributie(DateOnly Data, Guid? Document, bool Storno,
        decimal Qcub, decimal Qregistru, decimal Vcub, decimal Vregistru,
        bool AreCub, bool AreRegistru, string Provenienta) {
        public decimal Delta => Vcub - Vregistru;
    }
    internal sealed record Pozitie(Cheie Cheie, decimal Qcub, decimal Qregistru,
        decimal Vcub, decimal Vregistru, decimal Initial, decimal Intrari, decimal Iesiri,
        bool Incomplet, IReadOnlyList<Contributie> Contributii) {
        public decimal Delta => Vcub - Vregistru;
        public bool Reziduu => (Qcub == 0 && Vcub != 0) || (Qregistru == 0 && Vregistru != 0);
    }
    internal sealed record Raport(DateOnly LaData, IReadOnlyList<Pozitie> Pozitii,
        int Contraponderi, int RegistreInAfaraDomeniului, int CubInAfaraDomeniului) {
        public IReadOnlyList<Repartitie> IstoricFolosinta { get; init; } = [];
        public bool AreAbateri => Pozitii.Any(p => p.Delta != 0 || p.Qcub != p.Qregistru || p.Incomplet || p.Reziduu);
        public IEnumerable<string> Linii() {
            yield return $"Valori stoc la {LaData:yyyy-MM-dd}: lot × gestiune × cont, Carte=Contabil; "
                + $"{Pozitii.Count} poziții; {Contraponderi} contraponderi Transformare excluse structural.";
            yield return $"Domeniu comun: Magazie/Marfuri/Folosinta; în afara comparației: {RegistreInAfaraDomeniului} rânduri registru, {CubInAfaraDomeniului} postări cub."
                + " Cubul fără corespondent rămâne istoric incomplet; domeniul se identifică din mișcările registrelor.";
            foreach (var r in IstoricFolosinta)
                yield return $"CHEIE ISTORICĂ Folosinta: lot={r.Cheie.Lot}, gest={r.Cheie.Gestiune}, TipStoc={r.Cheie.Tip}, "
                    + $"doc={r.Cheie.Document}, storno={r.Cheie.Storno}: {r.Q}/{r.V}; politica actuală nu rescrie cheia.";
            foreach (var p in Pozitii.Where(p => p.Delta != 0 || p.Qcub != p.Qregistru || p.Incomplet || p.Reziduu)) {
                var stare = p.Incomplet ? "ISTORIC INCOMPLET / corespondență lipsă"
                    : p.Delta != 0 ? "DIFERENȚĂ NEEXPLICATĂ (cauza nu este certificată)" : "NECONCORDANȚĂ";
                yield return $"{stare}: lot={p.Cheie.Lot}, gest={p.Cheie.Gestiune}, cont={p.Cheie.Cont}; "
                    + $"cub={p.Qcub}/{p.Vcub}, registre={p.Qregistru}/{p.Vregistru}; "
                    + $"D={p.Delta} = inițial {p.Initial} + intrări {p.Intrari} − ieșiri {p.Iesiri}"
                    + (p.Reziduu ? "; CANTITATE ZERO CU VALOARE NENULĂ" : "");
                foreach (var c in p.Contributii.Where(c => c.Delta != 0 || c.Qcub != c.Qregistru || !c.AreCub || !c.AreRegistru))
                    yield return $"  {c.Data:yyyy-MM-dd} doc={c.Document} storno={c.Storno}: "
                        + $"cub={c.Qcub}/{c.Vcub}, registre={c.Qregistru}/{c.Vregistru}, D={c.Delta}; {c.Provenienta}";
            }
        }
    }

    public static Raport Citeste(DbContext ctx, DateOnly laData, IReadOnlyCollection<Guid>? loturi = null,
            DateOnly? deLa = null) {
        var set = loturi?.ToArray();
        var reg = ctx.Set<RegistruStoc>().Where(r => r.Data <= laData && (set == null || set.Contains(r.LotId)))
            .Select(r => new { r.ID, r.LotId, r.RepartitorId, r.Data, r.DocumentId, r.DetaliuId,
                r.Storno, r.Cantitate, r.Valoare, r.TipStoc, Cont = r.Lot.Produs.TipMaterial.ContImplicitId,
                Clasa = r.Lot.Produs.TipMaterial.ClasaId }).ToList();
        var claseFolosinta = ctx.Set<RegulaStoc>().Where(r => r.TipStoc == TipStoc.Folosinta && r.ClasaId != null)
            .Select(r => r.ClasaId).Distinct().ToHashSet();
        var loturiFolosinta = reg.Where(r => r.TipStoc == TipStoc.Folosinta || claseFolosinta.Contains(r.Clasa))
            .Select(r => r.LotId).ToHashSet();
        var cub = ctx.Set<C.Postare>().Where(p => p.Data <= laData && p.Carte == N.Carte.Contabil
                && p.Unitate != null && p.Produs != null && p.Gestiune != null
                && (set == null || set.Contains(p.Unitate.Value)))
            .Select(p => new { p.ID, p.Unitate, p.Gestiune, p.Cont, p.Data, p.DocumentId, p.LinieId,
                p.Cantitate, p.Valoare, p.Latura, p.Tranzactie.Fel }).ToList();
        var gestiuni = ctx.Set<Gestiune>().Select(g => g.ID).ToHashSet();
        var docIds = reg.Select(r => r.DocumentId).Concat(cub.Select(p => p.DocumentId)).OfType<Guid>().Distinct().ToArray();
        var documente = ctx.Set<Document>().Where(d => docIds.Contains(d.ID)
                || ctx.Set<Document>().Any(c => docIds.Contains(c.ID) && c.DocumentSursaId == d.ID))
            .Select(d => new { d.ID, d.ClrType, d.Autogenerat, d.DocumentSursaId }).ToDictionary(d => d.ID);
        var politiciConex = ctx.Set<PoliticaConex>().Where(p => p.TipDocumentSursa.PosteazaInCub)
            .Select(p => new { Sursa = p.TipDocumentSursa.ClrType, Tinta = p.TipDocumentTinta.ClrType })
            .ToList().Select(p => (p.Sursa, p.Tinta)).ToHashSet();
        Guid? Cap(Guid? id) => id is Guid d && documente.TryGetValue(d, out var doc)
            && doc.Autogenerat && doc.DocumentSursaId is Guid sursa && documente.TryGetValue(sursa, out var original)
            && politiciConex.Contains((original.ClrType, doc.ClrType)) ? sursa : id;
        var inAfara = InAfaraDomeniului(reg.Select(r =>
            (new CheieMiscare(r.LotId, r.RepartitorId, Cap(r.DocumentId), r.Data, r.Storno), r.TipStoc)));
        var cubReal = cub.Where(p => gestiuni.Contains(p.Gestiune!.Value)).ToArray();
        var cubExclus = cubReal.Where(p => inAfara.Contains(new(p.Unitate!.Value, p.Gestiune!.Value,
            p.DocumentId, p.Data, p.Fel == N.FelTranzactie.Storno))).Select(p => p.ID).ToHashSet();
        var fapte = new List<Fapt>();
        foreach (var r in reg.Where(r => r.TipStoc is TipStoc.Magazie or TipStoc.Marfuri or TipStoc.Folosinta))
            fapte.Add(new(new(r.LotId, r.RepartitorId, r.Cont), r.Data, Cap(r.DocumentId), r.DetaliuId,
                r.Storno, r.Cantitate, r.Valoare, false, $"registru {r.ID}, doc {r.DocumentId}, linie {r.DetaliuId}"));
        foreach (var p in cubReal.Where(p => !cubExclus.Contains(p.ID)))
            fapte.Add(new(new(p.Unitate!.Value, p.Gestiune!.Value, p.Cont), p.Data, p.DocumentId, p.LinieId,
                p.Fel == N.FelTranzactie.Storno, p.Cantitate,
                p.Latura == N.Latura.Debit ? p.Valoare : -p.Valoare, true,
                $"cub {p.ID}, doc {p.DocumentId}, linie {p.LinieId}, fel {p.Fel}"));
        var virtuale = ctx.Set<C.Postare>().Where(p => p.Data <= laData
                && p.Gestiune == N.GestiuniVirtuale.Transformare && p.Unitate == null
                && p.Produs != null && p.Carte == N.Carte.Contabil && p.Partener == null
                && p.TipTvaId == null && p.PerioadaDeclarare == null && p.Valuta == null
                && p.Valoare == 0 && p.ValoareValuta == 0 && p.Cantitate != 0
                && (set == null || p.DocumentId != null && docIds.Contains(p.DocumentId.Value))).Count();
        return new(laData, Calculeaza(fapte, deLa), virtuale,
            reg.Count(r => r.TipStoc is not (TipStoc.Magazie or TipStoc.Marfuri or TipStoc.Folosinta)), cubExclus.Count) {
            IstoricFolosinta = [.. reg.Where(r => loturiFolosinta.Contains(r.LotId))
                .GroupBy(r => new CheieRegistru(r.LotId, r.RepartitorId, r.TipStoc, r.DocumentId, r.Storno))
                .Select(g => new Repartitie(g.Key, g.Sum(r => r.Cantitate), g.Sum(r => r.Valoare)))],
        };
    }

    internal static HashSet<CheieMiscare> InAfaraDomeniului(IEnumerable<(CheieMiscare Cheie, TipStoc Tip)> miscari) =>
        miscari.GroupBy(m => m.Cheie).Where(g => g.All(m => m.Tip is not (TipStoc.Magazie or TipStoc.Marfuri or TipStoc.Folosinta)))
            .Select(g => g.Key).ToHashSet();

    internal static List<Pozitie> Calculeaza(IEnumerable<Fapt> fapte, DateOnly? deLa = null) =>
        [.. fapte.GroupBy(f => f.Cheie).Select(g => {
            var c = g.GroupBy(f => (f.Data, f.Document, f.Storno))
                .OrderBy(x => x.Key.Data).ThenBy(x => x.Key.Document).ThenBy(x => x.Key.Storno)
                .Select(x => new Contributie(x.Key.Data, x.Key.Document, x.Key.Storno,
                    x.Where(f => f.Cub).Sum(f => f.Q), x.Where(f => !f.Cub).Sum(f => f.Q),
                    x.Where(f => f.Cub).Sum(f => f.V), x.Where(f => !f.Cub).Sum(f => f.V),
                    x.Any(f => f.Cub), x.Any(f => !f.Cub), string.Join("; ", x.Select(f => f.Provenienta)))).ToArray();
            var initial = c.Where(x => deLa != null && x.Data < deLa).Sum(x => x.Delta);
            var curent = c.Where(x => deLa == null || x.Data >= deLa).ToArray();
            return new Pozitie(g.Key, c.Sum(x => x.Qcub), c.Sum(x => x.Qregistru),
                c.Sum(x => x.Vcub), c.Sum(x => x.Vregistru), initial,
                curent.Where(x => x.Qregistru >= 0).Sum(x => x.Delta),
                -curent.Where(x => x.Qregistru < 0).Sum(x => x.Delta),
                c.Any(x => !x.AreCub || !x.AreRegistru) || c[0].Qcub <= 0 || c[0].Qregistru <= 0, c);
        })];
}
