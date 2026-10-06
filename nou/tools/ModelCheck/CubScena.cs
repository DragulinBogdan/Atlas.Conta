using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Cub.Citiri;
using Atlas.Conta.BackOffice.Module.Motor;
using DevExpress.ExpressApp;
using C = Atlas.Conta.BackOffice.Module.Cub;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.ModelCheck;

record PostareScena(Guid? DocumentId, Guid? LinieId, N.FelTranzactie Fel, DateOnly Data, Guid Cont, N.Latura Latura,
    decimal Valoare, decimal Cantitate, Guid? Partener, Guid? Gestiune, Guid? Unitate, Guid? Produs,
    Guid? CodEconomic, Guid? CodFunctional, Guid? SursaFinantare, Guid? Proiect, Guid? CentruCost, Guid? TipTvaId) {
    public bool Storno => Fel == N.FelTranzactie.Storno;
    public bool Debit => Latura == N.Latura.Debit;
    public bool Credit => Latura == N.Latura.Credit;
    public decimal Semnata => Latura == N.Latura.Debit ? Valoare : -Valoare;
}

/// <summary>Intrările comune ale cubului, restrânse la documentele unei scene.</summary>
static class CubScena {
    static List<PostareScena> Citeste(IQueryable<C.Postare> postari) => postari
        .Select(p => new PostareScena(p.DocumentId, p.LinieId, p.Tranzactie.Fel, p.Data, p.Cont, p.Latura,
            p.Valoare, p.Cantitate, p.Partener, p.Gestiune, p.Unitate, p.Produs,
            p.CodEconomic, p.CodFunctional, p.SursaFinantare, p.Proiect, p.CentruCost, p.TipTvaId)).ToList();

    public static List<PostareScena> Note(IObjectSpace os, params Guid[] documente) =>
        Citeste(Contabil.Postari(os).Where(p => p.DocumentId != null && documente.Contains(p.DocumentId.Value)));

    public static List<PostareScena> Stoc(IObjectSpace os, params Guid[] documente) =>
        Citeste(Loturi.Postari(os).Where(p => p.DocumentId != null && documente.Contains(p.DocumentId.Value)));

    public static List<FaptFiscal> Fapte(IObjectSpace os, params Guid[] documente) =>
        Fiscale.Fapte(os).Where(f => documente.Contains(f.DocumentId)).ToList();

    public static bool FaraNote(IObjectSpace os, Guid document) =>
        !Contabil.Postari(os).Any(p => p.DocumentId == document);

    public static bool FaraStoc(IObjectSpace os, Guid document) =>
        !Loturi.Postari(os).Any(p => p.DocumentId == document);

    public static bool FaraFapte(IObjectSpace os, Guid document) =>
        !Fiscale.Postari(os).Any(p => p.DocumentId == document);

    public static bool FaraMiscari(IObjectSpace os, Guid lot) =>
        !Loturi.Postari(os).Any(p => p.Unitate == lot);

    public static (decimal Cantitate, decimal Valoare) SoldCheie(IObjectSpace os, Guid lot, Guid gestiune, Guid? cont = null, DateOnly? laData = null) {
        var sold = Sold(os, lot, gestiune, cont, laData);
        return (sold.Cantitate, sold.Valoare);
    }

    public static bool FaraFapte(IObjectSpace os) => !Fiscale.Postari(os).Any();

    public static bool FaraPostari(IObjectSpace os, Guid document) =>
        FaraNote(os, document) && FaraStoc(os, document) && FaraFapte(os, document);

    /// <summary>O notă contabilă = două postări ale aceleiași linii: debitul și creditul, cu aceeași valoare.</summary>
    public static bool Nota(this IEnumerable<PostareScena> postari, Guid? debit, Guid? credit, decimal? valoare = null, Guid? linie = null) =>
        postari.Any(d => d.Debit && d.Cont == debit && (valoare == null || d.Valoare == valoare) && (linie == null || d.LinieId == linie)
            && postari.Any(c => c.Credit && c.Cont == credit && c.Valoare == d.Valoare
                && c.LinieId == d.LinieId && c.DocumentId == d.DocumentId && c.Fel == d.Fel));

    public static decimal Rulaj(this IEnumerable<PostareScena> postari, Guid? cont, N.Latura latura) =>
        postari.Where(p => p.Cont == cont && p.Latura == latura).Sum(p => p.Valoare);

    public static SoldLot Sold(IObjectSpace os, Guid lot, Guid gestiune, Guid? cont = null, DateOnly? laData = null) {
        var solduri = Loturi.Solduri(os, laData).Where(s => s.LotId == lot && s.GestiuneId == gestiune
            && (cont == null || s.ContId == cont)).ToList();
        return new SoldLot { LotId = lot, GestiuneId = gestiune, Cantitate = solduri.Sum(s => s.Cantitate),
            Valoare = solduri.Sum(s => s.Valoare) };
    }

    /// <summary>Toate cheile (lot, gestiune, cont) ale loturilor date, inclusiv cele golite.</summary>
    public static List<SoldLot> Chei(IObjectSpace os, IReadOnlyCollection<Guid> loturi) =>
        Loturi.Postari(os).Where(p => loturi.Contains(p.Unitate.Value))
            .Select(p => new { Lot = p.Unitate.Value, Gestiune = p.Gestiune.Value, p.Cont, p.Latura, p.Valoare, p.Cantitate }).ToList()
            .GroupBy(p => new { p.Lot, p.Gestiune, p.Cont })
            .Select(g => new SoldLot { LotId = g.Key.Lot, GestiuneId = g.Key.Gestiune, ContId = g.Key.Cont,
                Cantitate = g.Sum(p => p.Cantitate),
                Valoare = g.Sum(p => p.Latura == N.Latura.Debit ? p.Valoare : -p.Valoare) }).ToList();

    /// <summary>Refuzul gardului de sold al cubului pentru o mișcare propusă pe lot, sau null dacă trece.</summary>
    public static string RefuzSold(IObjectSpace os, Guid lot, Guid gestiune, Guid produs, Guid cont, DateOnly data, decimal cantitate) {
        try {
            Loturi.VerificaSoldIntermediar(os, [C.Randuri.Citeste(new C.Postare {
                DocumentId = Guid.NewGuid(), Cont = cont, Latura = cantitate < 0m ? N.Latura.Credit : N.Latura.Debit, Data = data,
                Gestiune = gestiune, Produs = produs, Unitate = lot, FelUnitate = N.FelUnitate.Lot, UnitateDeschisa = data,
                Carte = N.Carte.Contabil, Cantitate = cantitate,
            })]);
            return null;
        }
        catch (OperareException e) { return e.Message; }
    }

    /// <summary>Rândurile fișelor date, opțional numai ale unui document; fără fișe = toate fișele bazei.</summary>
    public static List<Imobilizari.RandCuDocument> Fisa(IObjectSpace os, IEnumerable<Guid> fise = null, Guid? document = null) {
        var ids = fise?.ToList() ?? os.GetObjectsQuery<Imobilizare>().Select(i => i.ID).ToList();
        return Imobilizari.Randuri(os, ids, DateOnly.MaxValue).Where(r => document == null || r.DocumentId == document).ToList();
    }

    /// <summary>Locul fișei pe postările unui document: gestiunile distincte ale postărilor ei.</summary>
    public static List<Guid> LocFisa(IObjectSpace os, Guid fisa, Guid document) =>
        os.GetObjectsQuery<C.Postare>().Where(p => p.FelUnitate == N.FelUnitate.Fisa && p.Unitate == fisa
            && p.DocumentId == document && p.Gestiune != null).Select(p => p.Gestiune.Value).Distinct().ToList();

    /// <summary>Soldul debitor al contului (debit − credit) până la dată, pe toată baza sau pe documentele date.</summary>
    public static decimal SoldCont(IObjectSpace os, Guid cont, DateOnly? panaLa = null, IReadOnlyCollection<Guid> documente = null) {
        var postari = Contabil.Postari(os).Where(p => p.Cont == cont && p.Data <= (panaLa ?? DateOnly.MaxValue));
        if (documente != null) postari = postari.Where(p => p.DocumentId != null && documente.Contains(p.DocumentId.Value));
        return postari.Select(p => new { p.Latura, p.Valoare }).ToList()
            .Sum(p => p.Latura == N.Latura.Debit ? p.Valoare : -p.Valoare);
    }

    /// <summary>Data faptului în jurnalul fiscal: a documentului la operare, a stornării la storno.</summary>
    public static DateOnly DataJurnal(this FaptFiscal fapt) => fapt.Storno ? fapt.Data : fapt.DataDocument;

    public static List<FaptFiscal> FapteIntre(IObjectSpace os, DateOnly deLa, DateOnly panaLa) =>
        Fiscale.Fapte(os).ToList().Where(f => f.DataJurnal() >= deLa && f.DataJurnal() <= panaLa).ToList();

    public static int NoteIntre(IObjectSpace os, DateOnly deLa, DateOnly panaLa) =>
        Contabil.Jurnal(os, deLa, panaLa).Count(p => p.DocumentId != null);

    public static int MiscariIntre(IObjectSpace os, DateOnly deLa, DateOnly panaLa, bool cuDocument = false) =>
        Loturi.Postari(os).Count(p => p.Data >= deLa && p.Data <= panaLa && (!cuDocument || p.DocumentId != null));
}
