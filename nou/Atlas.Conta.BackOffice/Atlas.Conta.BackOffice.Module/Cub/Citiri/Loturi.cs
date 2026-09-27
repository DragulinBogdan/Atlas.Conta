using DevExpress.ExpressApp;
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
        .Where(p => p.Carte == N.Carte.Contabil && p.FelUnitate == N.FelUnitate.Lot
            && p.Unitate != null && p.Produs != null && p.Gestiune != null);

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
        var istoric = Postari(os).Where(p => loturi.Contains(p.Unitate.Value))
            .GroupBy(p => new { p.Unitate, p.Cont, p.Produs, p.Gestiune, p.Data })
            .Select(g => new { g.Key, Cantitate = g.Sum(p => p.Cantitate) }).ToList()
            .Select(p => (Cheie: new CheieLotFapt(p.Key.Unitate.Value, p.Key.Cont,
                p.Key.Produs.Value, p.Key.Gestiune.Value), p.Key.Data, p.Cantitate))
            .Where(p => chei.Contains(p.Cheie));
        var primaData = delta.Min(p => p.Data);
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
