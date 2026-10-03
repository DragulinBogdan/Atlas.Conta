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
        VerificaFiscal(os);
        Loturi.VerificaAcoperire(os);
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
        Explicatii.VerificaAcoperire(os);
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

    public static void VerificaFiscal(IObjectSpace os) {
        var postari = os.GetObjectsQuery<Postare>();
        var fapte = Fiscale.Postari(os);
        if (fapte.Any(p => p.DocumentId == null || p.LinieId == null || p.SensTva == null
                || p.RolTva == null || p.RegimTva == null || p.CotaTva == null || p.DeImport == null
                || p.DocumentFiscalId == null || p.DataDocument == null || p.DataExigibilitate == null
                || p.DataInregistrare == null || p.PerioadaDeclarare == null || p.PerioadaD394 == null
                || (p.SensTva == N.SensTva.Achizitie && p.DataPrimire == null)
                || p.RolTva < N.RolTva.Baza || p.RolTva > N.RolTva.Autocolectare))
            throw new OperareException("CITIRE_FISCAL_INCOMPLET: calificare sau reper fiscal absent.");
        if (fapte.Any(p => p.RolTva == N.RolTva.Autocolectare
                && (p.RegimTva != N.RegimTva.TaxareInversa || p.SensTva != N.SensTva.Achizitie)))
            throw new OperareException("CITIRE_FISCAL_AUTOLICHIDARE: autocolectare în afara unei achiziții cu taxare inversă.");
        if (fapte.GroupBy(p => new { p.TranzactieId, p.LinieId, p.TipTvaId, p.SensTva, p.Partener, p.RolTva })
                .Any(g => g.Count() != 1))
            throw new OperareException("CITIRE_FISCAL_DUPLICAT: mai multe postări pentru același rol fiscal.");
        if (fapte.Select(p => new { p.TranzactieId, p.LinieId, p.TipTvaId, p.SensTva, p.Partener,
                p.RegimTva, p.CotaTva, p.DeImport, p.DocumentFiscalId, p.DataDocument, p.DataExigibilitate,
                p.DataPrimire, p.DataInregistrare, p.PerioadaDeclarare, p.PerioadaD394,
                p.InversaTehnica, p.RegularizareD300 }).Distinct()
                .GroupBy(p => new { p.TranzactieId, p.LinieId, p.TipTvaId, p.SensTva, p.Partener })
                .Any(g => g.Count() != 1))
            throw new OperareException("CITIRE_FISCAL_CALIFICARE: calificări incompatibile în același fapt.");
        if (fapte.Any(p => p.InversaDinId != null && !postari.Any(o => o.ID == p.InversaDinId
                && o.Spatiu == p.InversaDinSpatiu && o.TipTvaId == p.TipTvaId && o.RolTva == p.RolTva
                && o.SensTva == p.SensTva && o.RegimTva == p.RegimTva && o.CotaTva == p.CotaTva
                && o.DeImport == p.DeImport && o.DocumentFiscalId == p.DocumentFiscalId
                && o.Valoare == -p.Valoare)))
            throw new OperareException("CITIRE_FISCAL_INVERSA: inversa nu păstrează calificarea și suma originalului.");
    }
}
