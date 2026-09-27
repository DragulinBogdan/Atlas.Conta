using Atlas.Conta.BackOffice.Module.BusinessObjects;
using DevExpress.ExpressApp;
using C = Atlas.Conta.BackOffice.Module.Cub;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.ModelCheck;

static class UnicitateFiscala {
    public static bool Corecte(IReadOnlyList<RandScena> actual, IReadOnlyList<RandScena> asteptate) {
        var fiscale = actual.Where(p => p.Tva != null).ToList();
        var matrice = asteptate.Where(p => p.Tva != null).ToList();
        var linii = asteptate.Select(p => p.Linie).ToHashSet();
        if (!linii.SetEquals(actual.Select(p => p.Linie))) return false;
        if (actual.Any(p => !linii.Contains(p.Linie)
            || (p.Tva == null && (p.Rol != null || p.Sens != null)))) return false;
        if (fiscale.Count != matrice.Count) return false;
        foreach (var grup in matrice.GroupBy(p => (p.Linie, p.Tva, p.Sens))) {
            var ale = fiscale.Where(p => (p.Linie, p.Tva, p.Sens) == grup.Key).ToList();
            if (grup.Count(p => p.Rol == N.RolTva.Baza) != 1 || grup.Count(p => p.Rol == N.RolTva.Taxa) > 1 || grup.Count(p => p.Rol == N.RolTva.Autocolectare) > 1) return false;
            foreach (var rol in new[] { N.RolTva.Baza, N.RolTva.Taxa, N.RolTva.Autocolectare }) {
                var a = grup.Where(p => p.Rol == rol).ToList(); var r = ale.Where(p => p.Rol == rol).ToList();
                if (a.Count != r.Count || a.Sum(p => p.Valoare) != r.Sum(p => p.Valoare)) return false;
            }
        }
        return fiscale.All(p => matrice.Any(a => (p.Linie, p.Tva, p.Sens, p.Rol) == (a.Linie, a.Tva, a.Sens, a.Rol)));
    }

    public static void Verifica(IObjectSpace os, Action<string, bool> check, bool privat,
            Guid doc, N.FelTranzactie fel, IReadOnlyList<RandScena> actual, IReadOnlyList<RandScena> asteptate) {
        var clr = os.GetObjectsQuery<Document>().Where(d => d.ID == doc).Select(d => d.ClrType).Single();
        var tip = os.GetObjectsQuery<TipDocument>().Where(t => t.ClrType == clr).Select(t => t.Cod).Single();
        var ids = asteptate.Select(p => p.Tva).OfType<Guid>().Distinct().ToList();
        var regimuri = os.GetObjectsQuery<TipTva>().Where(t => ids.Contains(t.ID))
            .Select(t => t.Regim).ToList().Distinct().ToArray();
        var eticheta = $"SC-X-14 ({(privat ? "privat" : "bugetar")}) {tip}/{fel}/"
            + (regimuri.Length == 0 ? "fără fapt fiscal" : string.Join(",", regimuri));
        check(eticheta + ": cardinalități și măsuri pe matricea numerică, fără Carte în cheie", Corecte(actual, asteptate));
        if (asteptate.FirstOrDefault(p => p.Rol == N.RolTva.Baza) is { } baza) {
            var martor = asteptate.ToList();
            martor.Add(baza with { Carte = baza.Carte == N.Carte.Fiscal ? N.Carte.Contabil : N.Carte.Fiscal });
            check(eticheta + ": detectează baza duplicată în cealaltă carte", !Corecte(martor, asteptate));
            martor = asteptate.ToList(); martor.Remove(baza);
            check(eticheta + ": detectează baza absentă", !Corecte(martor, asteptate));
        }
        if (asteptate.FirstOrDefault(p => p.Rol == N.RolTva.Taxa) is { } taxa)
            check(eticheta + ": detectează taxa duplicată", !Corecte([.. asteptate, taxa], asteptate));
    }

    public static void FaraFapte(IObjectSpace os, Action<string, bool> check, bool privat, Guid doc,
            N.FelTranzactie fel, Guid[] linii) {
        var p = os.GetObjectsQuery<C.Postare>().Where(p => p.DocumentId == doc && p.Tranzactie.Fel == fel).ToList();
        Verifica(os, check, privat, doc, fel,
            p.Select(p => new RandScena(p.Cont, p.Latura, p.Valoare, Linie: p.LinieId, Tva: p.TipTvaId,
                Rol: p.RolTva, Sens: p.SensTva, Carte: p.Carte)).ToList(),
            linii.Select(l => new RandScena(Guid.Empty, N.Latura.Debit, 0, Linie: l)).ToList());
    }
}
