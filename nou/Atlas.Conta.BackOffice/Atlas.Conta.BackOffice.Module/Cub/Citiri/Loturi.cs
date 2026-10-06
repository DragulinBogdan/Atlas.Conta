using DevExpress.ExpressApp;
using DevExpress.ExpressApp.EFCore;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Motor;
using Atlas.Conta.BackOffice.Module.Declaratii;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.Module.Cub.Citiri;

public struct SoldLot {
    public Guid LotId { get; set; }
    public Guid ContId { get; set; }
    public Guid ProdusId { get; set; }
    public Guid GestiuneId { get; set; }
    public DateOnly Deschisa { get; set; }
    public decimal Cantitate { get; set; }
    public decimal Valoare { get; set; }
}

public static class Loturi {
    public static IQueryable<Postare> Postari(IObjectSpace os) => os.GetObjectsQuery<Postare>()
        .Where(p => p.Spatiu == N.Spatiu.Stoc && p.Carte == N.Carte.Contabil && p.FelUnitate == N.FelUnitate.Lot
            && p.Unitate != null && p.Produs != null && p.Gestiune != null);

    /// <summary>Lotul e unitatea a cel puțin unei postări, pe orice fel de tranzacție.</summary>
    public static bool AreMiscari(IObjectSpace os, Guid lot) =>
        os.GetObjectsQuery<Postare>().Any(p => p.FelUnitate == N.FelUnitate.Lot && p.Unitate == lot);

    public static IQueryable<RandDatat<SoldLot>> Miscari(IObjectSpace os, Guid? faraDocumentId = null) {
        var postari = Postari(os);
        if (faraDocumentId is { } document) postari = postari.Where(p => p.DocumentId != document);
        return postari.Select(p => new RandDatat<SoldLot> {
            Data = p.Data,
            Rand = new SoldLot {
                LotId = p.Unitate.Value, ContId = p.Cont, ProdusId = p.Produs.Value, GestiuneId = p.Gestiune.Value,
                Deschisa = p.UnitateDeschisa ?? p.Data, Cantitate = p.Cantitate,
                Valoare = p.Latura == N.Latura.Debit ? p.Valoare : -p.Valoare
            }
        });
    }

    static IQueryable<SoldLot> Grupeaza(IQueryable<SoldLot> miscari) => miscari
        .GroupBy(s => new { s.LotId, s.ContId, s.ProdusId, s.GestiuneId })
        .Select(g => new SoldLot { LotId = g.Key.LotId, ContId = g.Key.ContId,
            ProdusId = g.Key.ProdusId, GestiuneId = g.Key.GestiuneId, Deschisa = g.Min(s => s.Deschisa),
            Cantitate = g.Sum(s => s.Cantitate), Valoare = g.Sum(s => s.Valoare) })
        .Where(s => s.Cantitate != 0m || s.Valoare != 0m);

    public static IQueryable<SoldLot> Solduri(IObjectSpace os, DateOnly? laData = null,
            Guid? faraDocumentId = null) =>
        Grupeaza(Miscari(os, faraDocumentId).Where(m => m.Data <= (laData ?? DateOnly.MaxValue)).Select(m => m.Rand));

    public static IQueryable<SoldLot> Disponibile(IObjectSpace os, CitireCumul citire, DateOnly laData,
            Guid produsId, Guid gestiuneId, Guid contId) =>
        from sold in Cumulate(os, citire, laData)
        join gestiune in os.GetObjectsQuery<BusinessObjects.Gestiune>() on sold.GestiuneId equals gestiune.ID
        where sold.ProdusId == produsId && sold.GestiuneId == gestiuneId
            && sold.ContId == contId && sold.Cantitate > 0m
        orderby sold.Deschisa, sold.LotId
        select sold;

    /// <summary>Excluderea poate folosi snapshot-ul numai cu o graniță anterioară tuturor postărilor documentului exclus.</summary>
    public static IQueryable<SoldLot> Cumulate(IObjectSpace os, CitireCumul citire, DateOnly? laData = null,
            Guid? faraDocumentId = null, DateOnly? granita = null) {
        var zi = laData ?? DateOnly.MaxValue;
        if (faraDocumentId != null && granita == null)
            return Solduri(os, laData, faraDocumentId);
        var snapshot = os.GetObjectsQuery<SoldPerioadaStoc>().Select(s => new SoldLunar<SoldLot> {
            An = s.An, Luna = s.Luna, Rand = new SoldLot {
                LotId = s.LotId, ContId = s.ContId, ProdusId = s.ProdusId, GestiuneId = s.GestiuneId,
                Deschisa = s.Deschisa, Cantitate = s.Cantitate, Valoare = s.Valoare
            }
        });
        return Grupeaza(CumulPerioade.Citeste(os, citire, Miscari(os, faraDocumentId), snapshot, zi, granita));
    }

    // Gardul verifică fiecare prefix zilnic, inclusiv zilele ulterioare unei
    // operații retroactive. Istoricul se citește pe set, fără politica curentă.
    public static void VerificaSoldIntermediar(IObjectSpace os, IEnumerable<N.Postare> propuse,
            string cod = "STOC_INSUFICIENT") {
        var delta = propuse.Where(p => p.Coordonate.Carte == N.Carte.Contabil
                && p.Coordonate.Unitate?.Fel == N.FelUnitate.Lot
                && p.Coordonate.Gestiune != null && p.Coordonate.Produs != null)
            .Select(p => (Cheie: new CheieLotFapt(p.Coordonate.Unitate.Id, p.Coordonate.Cont,
                p.Coordonate.Produs.Value, p.Coordonate.Gestiune.Value), p.Coordonate.Data, p.Cantitate))
            .ToArray();
        if (delta.Length == 0) return;
        var chei = delta.Select(p => p.Cheie).ToHashSet();
        var loturi = chei.Select(c => c.Lot).Distinct().ToArray();
        var primaData = delta.Min(p => p.Data);
        var initial = Cumulate(os, CitireCumul.Integrala, primaData.AddDays(-1)).Where(s => loturi.Contains(s.LotId)).ToList()
            .Select(s => (Cheie: new CheieLotFapt(s.LotId, s.ContId, s.ProdusId, s.GestiuneId), Data: DateOnly.MinValue, s.Cantitate));
        var istoric = initial.Concat(Postari(os).Where(p => loturi.Contains(p.Unitate.Value) && p.Data >= primaData)
            .GroupBy(p => new { p.Unitate, p.Cont, p.Produs, p.Gestiune, p.Data })
            .Select(g => new { g.Key, Cantitate = g.Sum(p => p.Cantitate) }).ToList()
            .Select(p => (Cheie: new CheieLotFapt(p.Key.Unitate.Value, p.Key.Cont,
                p.Key.Produs.Value, p.Key.Gestiune.Value), p.Key.Data, p.Cantitate)))
            .Where(p => chei.Contains(p.Cheie));
        foreach (var grup in istoric.Concat(delta).GroupBy(p => p.Cheie)) {
            decimal sold = 0m;
            foreach (var zi in grup.GroupBy(p => p.Data).OrderBy(g => g.Key)) {
                sold += zi.Sum(p => p.Cantitate);
                if (zi.Key >= primaData && sold < 0m)
                    throw new OperareException($"{cod}: Sold negativ ({sold}) la {zi.Key:yyyy-MM-dd} "
                        + $"pe lotul {grup.Key.Lot}, cont {grup.Key.Cont}, gestiune {grup.Key.Gestiune}.");
            }
        }
    }

    public const string IstoricIncomplet = "CITIRE_ISTORIC_STOC_INCOMPLET";

    sealed record GrupStoc(Guid? Document, Guid Lot, Guid Gestiune, bool Storno, bool Plus);

    /// <summary>Fiecare grup de mișcări din registrul stocului are aceeași cantitate pe loturile cubului; recepția conexă se compară pe grupul sursei.</summary>
    public static void VerificaAcoperire(IObjectSpace os) {
        var registru = os.GetObjectsQuery<RegistruStoc>()
            .Where(r => r.TipStoc == TipStoc.Magazie || r.TipStoc == TipStoc.Marfuri || r.TipStoc == TipStoc.Folosinta)
            .GroupBy(r => new { r.DocumentId, r.LotId, r.RepartitorId, r.Storno, Plus = r.Cantitate >= 0m })
            .Select(g => new { g.Key.DocumentId, g.Key.LotId, g.Key.RepartitorId, g.Key.Storno, g.Key.Plus,
                Cantitate = g.Sum(r => r.Cantitate) }).ToList();
        if (registru.Count == 0) return;
        var cub = Postari(os)
            .GroupBy(p => new { p.DocumentId, p.Unitate, p.Gestiune,
                Storno = p.Tranzactie.Fel == N.FelTranzactie.Storno, Plus = p.Cantitate >= 0m })
            .Select(g => new { g.Key.DocumentId, g.Key.Unitate, g.Key.Gestiune, g.Key.Storno, g.Key.Plus,
                Cantitate = g.Sum(p => p.Cantitate) }).ToList();
        var legaturi = Receptii.Legaturi(((EFCoreObjectSpace)os).DbContext);
        var surse = legaturi.Values.ToHashSet();
        var cumulActiv = os.GetObjectsQuery<NIR>()
            .Where(d => d.SursaReceptieiId != null && d.Stare == StareDocument.Operat).Select(d => d.ID).ToList()
            .Where(legaturi.ContainsKey).Select(id => legaturi[id]).ToHashSet();
        Guid? Cap(Guid? document) => document is Guid id && legaturi.TryGetValue(id, out var sursa) ? sursa : document;
        bool Conex(Guid? document) => document is Guid id && (legaturi.ContainsKey(id) || surse.Contains(id));
        // Grupul recepției conexe se compară net: registrul ține cumulul pe NIR, cubul recepția pe FCT și delta pe NIR (098, 099).
        GrupStoc Cheie(Guid? document, Guid lot, Guid gestiune, bool storno, bool plus) => Conex(document)
            ? new(Cap(document), lot, gestiune, false, true) : new(document, lot, gestiune, storno, plus);
        var cantitati = cub.GroupBy(p => Cheie(p.DocumentId, p.Unitate.Value, p.Gestiune.Value, p.Storno, p.Plus))
            .ToDictionary(g => g.Key, g => g.Sum(p => p.Cantitate));
        var lipsuri = registru.GroupBy(r => Cheie(r.DocumentId, r.LotId, r.RepartitorId, r.Storno, r.Plus))
            .Select(g => (g.Key, Registru: g.Sum(r => r.Cantitate),
                Cub: Conex(g.Key.Document) && !cumulActiv.Contains(g.Key.Document.Value) ? 0m : cantitati.GetValueOrDefault(g.Key)))
            .Where(g => g.Registru != g.Cub).OrderBy(g => g.Key.Document).ThenBy(g => g.Key.Lot).ToList();
        if (lipsuri.Count != 0)
            throw new OperareException($"{IstoricIncomplet}: {lipsuri.Count} grupuri de mișcări din registrul stocului fără aceeași cantitate în cub; exemple: "
                + string.Join("; ", lipsuri.Take(10).Select(g => $"document {g.Key.Document}, lot {g.Key.Lot}, gestiune {g.Key.Gestiune}"
                    + $"{(g.Key.Storno ? ", storno" : "")}: registru {g.Registru}, cub {g.Cub}")));
    }

    // Refuzul cantitativ precedă orice schimbare în tracker-ul registrelor.
    // Materializatorul verifică din nou inversa completă înaintea scrierii.
    internal static void VerificaRetragere(IObjectSpace os, BusinessObjects.Document doc,
            DateOnly? laData = null) {
        var proprii = Postari(os).Where(p => p.DocumentId == doc.ID
            && (p.Tranzactie.Fel == N.FelTranzactie.Operare || p.Tranzactie.Fel == N.FelTranzactie.Transfer)).ToList();
        VerificaSoldIntermediar(os, proprii.Select(p => {
            var citit = Randuri.Citeste(p);
            return citit with { Cantitate = -p.Cantitate,
                Coordonate = citit.Coordonate with { Data = laData ?? p.Data } };
        }), ReceptiiConexe.CodRefuzStoc(doc));
    }
}
