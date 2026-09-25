using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Motor;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.EFCore;
using Microsoft.EntityFrameworkCore;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.Module.Cub.Citiri;

/// <summary>Invarianții cubului pe baza întreagă, negarantați de scriere; proba ModelCheck, nu cale de host (102d).</summary>
public static class Invarianti {
    public static void Verifica(IObjectSpace os) {
        VerificaProvenienta(os);
        var ctx = ((EFCoreObjectSpace)os).DbContext;
        var postari = os.GetObjectsQuery<Postare>();
        var lipsuri = os.GetObjectsQuery<RegistruContabil>().Where(r => r.DocumentId != null
            && (!postari.Any(p => p.DocumentId == r.DocumentId && p.Carte == N.Carte.Contabil
                && (r.DetaliuId == null || p.LinieId == r.DetaliuId)
                && p.Cont == r.ContDebitId && p.Latura == N.Latura.Debit
                && (p.Tranzactie.Fel == N.FelTranzactie.Storno) == r.Storno)
                || !postari.Any(p => p.DocumentId == r.DocumentId && p.Carte == N.Carte.Contabil
                    && (r.DetaliuId == null || p.LinieId == r.DetaliuId)
                    && p.Cont == r.ContCreditId && p.Latura == N.Latura.Credit
                    && (p.Tranzactie.Fel == N.FelTranzactie.Storno) == r.Storno)))
            .Select(r => r.DocumentId.Value).Distinct().ToList();
        var absorbite = Receptii.Legaturi(ctx, lipsuri);
        lipsuri = lipsuri.Where(id => !absorbite.ContainsKey(id)).ToList();
        if (lipsuri.Count != 0)
            throw new OperareException($"CITIRE_ISTORIC_INCOMPLET: {lipsuri.Count} documente cu linii sau laturi contabile neacoperite; exemple: "
                + string.Join(", ", lipsuri.Take(10)));

        var dezechilibrate = postari.GroupBy(p => new { p.TranzactieId, p.Carte })
            .Where(g => g.Sum(p => p.Latura == N.Latura.Debit ? p.Valoare : -p.Valoare) != 0m).LongCount();
        if (dezechilibrate != 0)
            throw new OperareException($"CITIRE_CUB_DEZECHILIBRAT: {dezechilibrate} tranzacții/cărți cu debit diferit de credit.");

        var deschideri = os.GetObjectsQuery<RegistruContabil>().Where(r => r.DocumentId == null);
        if (deschideri.Any()) {
            var vechi = deschideri.Select(r => new { Cont = r.ContDebitId, Debit = r.Valoare, Credit = 0m })
                .Concat(deschideri.Select(r => new { Cont = r.ContCreditId, Debit = 0m, Credit = r.Valoare }))
                .GroupBy(r => r.Cont).Select(g => new { Cont = g.Key, Debit = g.Sum(r => r.Debit), Credit = g.Sum(r => r.Credit) })
                .ToList();
            var noi = Contabil.Postari(os).Where(p => p.Tranzactie.Fel == N.FelTranzactie.Deschidere)
                .GroupBy(p => p.Cont).Select(g => new { Cont = g.Key,
                    Debit = g.Sum(p => p.Latura == N.Latura.Debit ? p.Valoare : 0m),
                    Credit = g.Sum(p => p.Latura == N.Latura.Credit ? p.Valoare : 0m) }).ToDictionary(r => r.Cont);
            if (vechi.Any(v => !noi.TryGetValue(v.Cont, out var n) || n.Debit != v.Debit || n.Credit != v.Credit))
                throw new OperareException("CITIRE_DESCHIDERE_INCOMPLETA: soldurile istorice fără document nu sunt acoperite de deschiderea cubului.");
        }
        Partide.VerificaAcoperire(os);
        Proiectii.ImperecheriProiectii.VerificaAcoperire(os);
        Imobilizari.VerificaAcoperire(os);
    }

    public static void VerificaProvenienta(IObjectSpace os) {
        var toate = os.GetObjectsQuery<Postare>();
        var lipsa = os.GetObjectsQuery<Postare>().Where(p => p.Carte == N.Carte.Contabil).Where(Transformare.FaraContrapondere)
            .LongCount(p => p.Tranzactie.Fel == N.FelTranzactie.Storno
                && !toate.Any(o => o.ID == p.InversaDinId && o.Spatiu == p.InversaDinSpatiu
                    && o.Tranzactie.Fel != N.FelTranzactie.Storno));
        if (lipsa != 0)
            throw new OperareException($"CITIRE_PROVENIENTA_LIPSA: {lipsa} postări Storno fără origine verificabilă.");
    }
}
