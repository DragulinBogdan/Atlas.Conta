using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Declaratii;
using Atlas.Conta.BackOffice.Module.Motor;
using DevExpress.ExpressApp;
using Microsoft.EntityFrameworkCore;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.Module.Cub;

public static partial class Materializare {
    sealed record ContNominalizat(Guid Cont, N.Latura Latura);

    static List<ContNominalizat> ConturiNominalizare(IObjectSpace os, List<Guid> conturi) {
        var active = os.GetObjectsQuery<TipMaterial>().Where(t => t.Clasa.Natura == NaturaClasa.Imobilizare
                && t.ContImplicitId != null && conturi.Contains(t.ContImplicitId.Value))
            .Select(t => new { Cont = t.ContImplicitId.Value, Latura = N.Latura.Debit });
        var amortizari = os.GetObjectsQuery<PoliticaAmortizare>().Where(p => p.ContAmortizareId != null
                && conturi.Contains(p.ContAmortizareId.Value))
            .Select(p => new { Cont = p.ContAmortizareId.Value, Latura = N.Latura.Credit });
        var istorice = Citiri.Imobilizari.Nominalizari(os).Where(p => conturi.Contains(p.Cont))
            .Select(p => new { p.Cont, p.Latura });
        return active.Concat(amortizari).Concat(istorice).Distinct().ToList()
            .Select(c => new ContNominalizat(c.Cont, c.Latura)).ToList();
    }

    internal static void BlocheazaFise(IObjectSpace os, Document document) {
        var conturi = os.GetObjectsQuery<Postare>().Where(p => p.DocumentId == document.ID
            && (p.Carte == N.Carte.Contabil || p.FelUnitate == N.FelUnitate.Fisa)).Select(p => p.Cont).Distinct().ToList();
        if (ConturiNominalizare(os, conturi).Count > 0) BlocheazaNominalizarea(os);
    }

    static void VerificaPozitiaFaraFisa(IObjectSpace os, IEnumerable<N.Postare> schimbari, bool blocheaza) {
        var delte = schimbari.Where(p => p.Coordonate.Carte == N.Carte.Contabil
            && p.Coordonate.Unitate == null).ToList();
        if (delte.Count == 0) return;
        var conturi = delte.Select(p => p.Coordonate.Cont).Distinct().ToList();
        var protejate = ConturiNominalizare(os, conturi);
        if (protejate.Count == 0) return;
        if (blocheaza) BlocheazaNominalizarea(os);
        conturi = protejate.Select(p => p.Cont).Distinct().ToList();
        var citite = os.GetObjectsQuery<Postare>().Where(p => p.Carte == N.Carte.Contabil
                && p.Unitate == null && conturi.Contains(p.Cont))
            .Include(p => p.Tranzactie).ToList();
        var inMemorie = os.ModifiedObjects.OfType<Postare>().Where(p => os.IsNewObject(p)
            && p.Carte == N.Carte.Contabil && p.Unitate == null && conturi.Contains(p.Cont));
        var toate = citite.Concat(inMemorie).DistinctBy(p => p.ID).Select(Randuri.Citeste).Concat(delte).ToList();
        static N.Capat Capat(N.Postare p) => new() {
            Cont = p.Coordonate.Cont, Carte = p.Coordonate.Carte, Partener = p.Coordonate.Partener,
            Gestiune = p.Coordonate.Gestiune, Produs = p.Coordonate.Produs,
            Valuta = p.Coordonate.Valuta, Analiza = p.Coordonate.Analiza,
        };
        var atinse = delte.Select(Capat).ToHashSet();
        foreach (var cont in protejate)
            foreach (var grup in toate.Where(p => p.Coordonate.Cont == cont.Cont).GroupBy(Capat).Where(g => atinse.Contains(g.Key))) {
                decimal sold = 0;
                foreach (var zi in grup.GroupBy(p => p.Coordonate.Data).OrderBy(g => g.Key)) {
                    sold += zi.Sum(p => p.Coordonate.Latura == cont.Latura ? p.Valoare : -p.Valoare);
                    if (sold >= 0) continue;
                    var simbol = os.GetObjectsQuery<Cont>().Where(c => c.ID == cont.Cont).Select(c => c.Simbol).Single();
                    throw new OperareException($"{CoduriRefuz.PozitieFaraFisaNegativa}: Poziția fără fișă pe contul {simbol} devine {sold} la {zi.Key:dd.MM.yyyy}.");
                }
            }
    }
}
