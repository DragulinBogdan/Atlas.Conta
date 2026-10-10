using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Motor;
using DevExpress.ExpressApp;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.Module.Cub.Citiri;

/// <summary>Invarianții cubului pe baza întreagă, negarantați de scriere; proba ModelCheck, nu cale de host (102d).</summary>
public static class Invarianti {
    public static void Verifica(IObjectSpace os) {
        VerificaProvenienta(os);
        VerificaFiscal(os);
        VerificaTaxaLiniilor(os);
        var postari = os.GetObjectsQuery<Postare>();
        var dezechilibrate = postari.GroupBy(p => new { p.TranzactieId, p.Carte })
            .Where(g => g.Sum(p => p.Latura == N.Latura.Debit ? p.Valoare : -p.Valoare) != 0m).LongCount();
        if (dezechilibrate != 0)
            throw new OperareException($"CITIRE_CUB_DEZECHILIBRAT: {dezechilibrate} tranzacții/cărți cu debit diferit de credit.");

        VerificaTransferuri(os);

        Partide.VerificaAcoperire(os);
        Imobilizari.VerificaProvenienta(os);
        Explicatii.VerificaAcoperire(os);
        VerificaPerechi(os);
        VerificaOrdinale(os);
        VerificaCoordonate(os);
    }

    public const string CoordonateInvalide = "CITIRE_COORDONATE_INVALIDE";

    /// <summary>Forma coordonatelor (C111-D2): lotul pe repartitor intern, partida pe repartitor extern, blocul fiscal numai cu fapt și cu loc intern.</summary>
    public static void VerificaCoordonate(IObjectSpace os) {
        var repartitori = os.GetObjectsQuery<Repartitor>();
        string[] interne = [nameof(N.FelRepartitor.Gestiune), nameof(N.FelRepartitor.UnitateInterna)];
        string[] externe = [nameof(N.FelRepartitor.Partener), nameof(N.FelRepartitor.Angajat)];
        var invalide = os.GetObjectsQuery<Postare>().Where(p =>
                (p.TipTvaId == null && (p.PartenerFiscal != null || p.LocFiscal != null))
                || (p.TipTvaId != null && p.LocFiscal == null)
                || (p.Cantitate != 0m && p.Contrapondere == null && p.FelUnitate != N.FelUnitate.Lot)
                || (p.FelUnitate == N.FelUnitate.Lot
                    && !repartitori.Any(r => r.ID == p.Repartitor && interne.Contains(r.ClrType)))
                || (p.FelUnitate == N.FelUnitate.Partida
                    && !repartitori.Any(r => r.ID == p.Repartitor && externe.Contains(r.ClrType)))
                || (p.LocFiscal != null
                    && !repartitori.Any(r => r.ID == p.LocFiscal && interne.Contains(r.ClrType))))
            .Select(p => p.ID).Take(10).ToList();
        if (invalide.Count != 0)
            throw new OperareException($"{CoordonateInvalide}: postări cu repartitor, contrapondere sau bloc fiscal "
                + "în afara formei; exemple: " + string.Join(", ", invalide));
    }

    public const string OrdinalInvalid = "CITIRE_ORDINAL_INVALID";

    /// <summary>Ordinalul numără postările tranzacției de la 1, fără goluri și fără dubluri (117).</summary>
    public static void VerificaOrdinale(IObjectSpace os) {
        var invalide = os.GetObjectsQuery<Postare>().GroupBy(p => p.TranzactieId)
            .Where(g => g.Min(p => p.Ordinal) != 1 || g.Max(p => p.Ordinal) != g.Count()
                || g.Select(p => p.Ordinal).Distinct().Count() != g.Count())
            .Select(g => g.Key).Take(10).ToList();
        if (invalide.Count != 0)
            throw new OperareException($"{OrdinalInvalid}: tranzacții ale căror postări nu sunt numărate de la 1 fără goluri; exemple: "
                + string.Join(", ", invalide));
    }

    public const string PerecheInvalida = "CITIRE_PERECHE_INVALIDA";
    public const string PerecheLipsa = "CITIRE_PERECHE_LIPSA";

    /// <summary>Perechea persistată e cea de la contractare (D9-A2): exact două postări pe (tranzacție, ordinal), mișcarea și contrapartida ei; fără ordinal rămân numai transformările și deschiderea.</summary>
    public static void VerificaPerechi(IObjectSpace os) {
        var postari = os.GetObjectsQuery<Postare>();
        var invalide = postari.Where(p => p.Pereche != null)
            .GroupBy(p => new { p.TranzactieId, p.Tranzactie.Fel, p.Pereche })
            .Select(g => new {
                g.Key.TranzactieId, g.Key.Fel, g.Key.Pereche,
                Cate = g.Count(),
                Documente = g.Select(p => p.DocumentId ?? Guid.Empty).Distinct().Count(),
                Linii = g.Select(p => p.LinieId ?? Guid.Empty).Distinct().Count(),
                Laturi = g.Select(p => p.Latura).Distinct().Count(),
                Cantitate = g.Sum(p => p.Cantitate),
                Valoare = g.Sum(p => p.Valoare), ValoareMin = g.Min(p => p.Valoare), ValoareMax = g.Max(p => p.Valoare),
                Valuta = g.Sum(p => p.ValoareValuta), ValutaMin = g.Min(p => p.ValoareValuta), ValutaMax = g.Max(p => p.ValoareValuta),
            })
            .Where(x => x.Pereche < 1 || x.Cate != 2 || x.Documente != 1 || x.Linii != 1 || x.Cantitate != 0m
                || x.Fel == N.FelTranzactie.Deschidere
                || (x.Fel == N.FelTranzactie.Operare && x.Laturi != 2)
                || (x.Fel == N.FelTranzactie.Transfer && x.Laturi != 1)
                || !((x.Laturi == 2 && x.ValoareMin == x.ValoareMax && x.ValutaMin == x.ValutaMax)
                    || (x.Laturi == 1 && x.Valoare == 0m && x.Valuta == 0m)))
            .Select(x => new { x.TranzactieId, x.Pereche })
            .Take(10).ToList();
        if (invalide.Count != 0)
            throw new OperareException($"{PerecheInvalida}: perechi care nu sunt o mișcare cu contrapartida ei; exemple: "
                + string.Join(", ", invalide.Select(x => $"{x.TranzactieId}/{x.Pereche}")));
        var lipsa = postari.Where(p => p.Pereche == null && p.Tranzactie.Fel != N.FelTranzactie.Deschidere)
            .Select(p => new {
                p.TranzactieId, p.DocumentId, p.LinieId, p.Cont, p.Produs, p.Latura,
                p.CodFunctional, p.CodEconomic, p.SursaFinantare, p.UnitateOrganizatorica, p.Proiect, p.CentruCost,
                Cantitate = p.Contrapondere == N.Contrapondere.Transformare ? -p.Cantitate : p.Cantitate,
                Contrapondere = p.Contrapondere == N.Contrapondere.Transformare,
                Forma = p.Cantitate != 0m && p.Produs != null && p.Carte == N.Carte.Contabil
                    && p.TipTvaId == null && p.PerioadaDeclarare == null
                    && p.Valuta == null && p.ValoareValuta == 0m
                    && (p.Contrapondere == N.Contrapondere.Transformare
                        ? p.Unitate == null && p.Valoare == 0m && p.Repartitor == null
                        : p.Contrapondere == null && p.Unitate != null && p.FelUnitate == N.FelUnitate.Lot
                            && p.Repartitor != null),
            })
            .GroupBy(p => new { p.TranzactieId, p.DocumentId, p.LinieId, p.Cont, p.Produs, p.Latura,
                p.CodFunctional, p.CodEconomic, p.SursaFinantare, p.UnitateOrganizatorica, p.Proiect, p.CentruCost,
                p.Cantitate })
            .Where(g => g.Any(p => !p.Forma) || g.Count(p => p.Contrapondere) * 2 != g.Count())
            .Select(g => g.Key.TranzactieId).Distinct().Take(10).ToList();
        if (lipsa.Count != 0)
            throw new OperareException($"{PerecheLipsa}: postări fără ordinal de pereche în afara transformărilor și a deschiderii; exemple: "
                + string.Join(", ", lipsa));
    }

    public const string TransferNeconservat = "CITIRE_TRANSFER_NECONSERVAT";

    /// <summary>Un transfer persistat conservă valoarea pe (cont, latură) și cantitatea pe (cont, produs), ca la contractare (090f).</summary>
    public static void VerificaTransferuri(IObjectSpace os) {
        var transferuri = os.GetObjectsQuery<Postare>().Where(p => p.Tranzactie.Fel == N.FelTranzactie.Transfer);
        var valoric = transferuri.GroupBy(p => new { p.TranzactieId, p.Cont, p.Latura })
            .Where(g => g.Sum(p => p.Valoare) != 0m).Select(g => g.Key.TranzactieId).Take(10).ToList();
        var cantitativ = transferuri.Where(p => p.Cantitate != 0m).GroupBy(p => new { p.TranzactieId, p.Cont, p.Produs })
            .Where(g => g.Sum(p => p.Cantitate) != 0m).Select(g => g.Key.TranzactieId).Take(10).ToList();
        var neconservate = valoric.Concat(cantitativ).Distinct().Take(10).ToList();
        if (neconservate.Count != 0)
            throw new OperareException($"{TransferNeconservat}: transferuri care nu conservă valoarea pe (cont, latură) "
                + "sau cantitatea pe (cont, produs); exemple: " + string.Join(", ", neconservate));
    }

    public const string TaxaDiferitaDeLinie = "CITIRE_TAXA_DIFERITA_DE_LINIE";

    /// <summary>Taxa postată separat la operare este taxa liniei documentului (109b).</summary>
    public static void VerificaTaxaLiniilor(IObjectSpace os) {
        var diferite = os.GetObjectsQuery<Postare>()
            .Where(p => p.RolTva == N.RolTva.Taxa && p.RegimTva != N.RegimTva.Capitalizat && p.LinieId != null
                && p.Tranzactie.Fel == N.FelTranzactie.Operare)
            .GroupBy(p => p.LinieId)
            .Select(g => new { Linie = g.Key, Taxa = g.Sum(p => p.Valoare) })
            .Join(os.GetObjectsQuery<DocumentDetaliu>(), t => t.Linie, l => l.ID,
                (t, l) => new { l.DocumentId, t.Taxa, l.ValoareTva })
            .Where(x => x.Taxa != x.ValoareTva)
            .Take(10).ToList();
        if (diferite.Count != 0)
            throw new OperareException($"{TaxaDiferitaDeLinie}: documente a căror taxă postată diferă de taxa liniei; exemple: "
                + string.Join(", ", diferite.Select(x => $"{x.DocumentId} (postat {x.Taxa}, pe linie {x.ValoareTva})")));
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
        if (fapte.GroupBy(p => new { p.TranzactieId, p.LinieId, p.TipTvaId, p.SensTva, p.PartenerFiscal, p.RolTva })
                .Any(g => g.Count() != 1))
            throw new OperareException("CITIRE_FISCAL_DUPLICAT: mai multe postări pentru același rol fiscal.");
        if (fapte.Select(p => new { p.TranzactieId, p.LinieId, p.TipTvaId, p.SensTva, p.PartenerFiscal,
                p.RegimTva, p.CotaTva, p.DeImport, p.DocumentFiscalId, p.DataDocument, p.DataExigibilitate,
                p.DataPrimire, p.DataInregistrare, p.PerioadaDeclarare, p.PerioadaD394,
                p.InversaTehnica, p.RegularizareD300 }).Distinct()
                .GroupBy(p => new { p.TranzactieId, p.LinieId, p.TipTvaId, p.SensTva, p.PartenerFiscal })
                .Any(g => g.Count() != 1))
            throw new OperareException("CITIRE_FISCAL_CALIFICARE: calificări incompatibile în același fapt.");
        if (fapte.Any(p => p.InversaDinId != null && !postari.Any(o => o.ID == p.InversaDinId
                && o.Spatiu == p.InversaDinSpatiu && o.TipTvaId == p.TipTvaId && o.RolTva == p.RolTva
                && o.SensTva == p.SensTva && o.RegimTva == p.RegimTva && o.CotaTva == p.CotaTva
                && o.DeImport == p.DeImport && o.DocumentFiscalId == p.DocumentFiscalId
                && o.PartenerFiscal == p.PartenerFiscal && o.LocFiscal == p.LocFiscal
                && o.Valoare == -p.Valoare)))
            throw new OperareException("CITIRE_FISCAL_INVERSA: inversa nu păstrează calificarea și suma originalului.");
    }
}
