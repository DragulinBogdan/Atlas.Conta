#nullable enable
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Declaratii;
using Atlas.Conta.BackOffice.Module.Motor;
using DevExpress.ExpressApp;
using C = Atlas.Conta.BackOffice.Module.Cub;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.ModelCheck;

/// <summary>
/// Probele cubului (S-D8): ce a rămas PERSISTAT după comanda motorului.
/// </summary>
static class ProbeCub {
    /// <summary>
    /// S-D15: seed-ul lasă `PoliticaTva.TolerantaTaxa` pe `null` (taxa culeasă
    /// autoritară); scena care probează gardul o pune LOCAL și o restaurează.
    /// </summary>
    public static IDisposable CuToleranta(IObjectSpace os, Document doc, decimal? valoare) {
        ArgumentNullException.ThrowIfNull(os);
        ArgumentNullException.ThrowIfNull(doc);
        var clrType = MotorOperare.ClasaReala(doc).Name;
        var tip = os.FirstOrDefault<TipDocument>(t => t.ClrType == clrType)
            ?? throw new InvalidOperationException($"Lipsește ancora TipDocument pentru {clrType}.");
        var politica = os.FirstOrDefault<PoliticaTva>(x => x.TipDocumentId == tip.ID)
            ?? throw new InvalidOperationException($"Tipul {clrType} n-are politică de TVA pe baza asta.");
        return new ComutatorToleranta(os, politica, valoare);
    }

    public static List<C.Tranzactie> Tranzactii(IObjectSpace os, Guid document) =>
        os.GetObjectsQuery<C.Tranzactie>().Where(t => t.DocumentId == document).ToList();

    public static List<C.Postare> Postari(IObjectSpace os, Guid document, N.FelTranzactie? fel = null) =>
        os.GetObjectsQuery<C.Postare>()
            .Where(p => p.DocumentId == document && (fel == null || p.Tranzactie.Fel == fel))
            .ToList();

    /// <summary>Rândurile de cub ale documentelor scenei, în ordinea FK (S-D8).</summary>
    public static void Purjeaza(Purja purja, IObjectSpace os, IReadOnlyList<Guid> documente) {
        ArgumentNullException.ThrowIfNull(purja);
        ArgumentNullException.ThrowIfNull(os);
        ArgumentNullException.ThrowIfNull(documente);
        if (documente.Count == 0)
            return;
        purja.AdaugaCheie<C.Postare>(os.GetObjectsQuery<C.Postare>()
            .Where(p => p.DocumentId != null && documente.Contains(p.DocumentId.Value))
            .Select(p => p.ID).ToList());
        purja.AdaugaCheie<C.Tranzactie>(os.GetObjectsQuery<C.Tranzactie>()
            .Where(t => t.DocumentId != null && documente.Contains(t.DocumentId.Value))
            .Select(t => t.ID).ToList());
    }

    /// <summary>
    /// T-D2: tranzacțiile PROPRII ale documentului — `Operare` ⊕ transferul lui de
    /// STOC; transferurile pe PARTIDĂ sunt ale împerecherii (S-D13).
    /// </summary>
    public static List<C.Tranzactie> Proprii(IObjectSpace os, Guid document) {
        var cuStoc = Postari(os, document)
            .Where(r => r.Spatiu == N.Spatiu.Stoc)
            .Select(r => r.TranzactieId)
            .ToHashSet();
        return [.. Tranzactii(os, document)
            .Where(t => t.Fel == N.FelTranzactie.Operare
                || (t.Fel == N.FelTranzactie.Transfer && cuStoc.Contains(t.ID)))];
    }

    public static List<C.Tranzactie> Transferuri(IObjectSpace os, Guid document) =>
        os.GetObjectsQuery<C.Tranzactie>()
            .Where(t => t.DocumentId == document && t.Fel == N.FelTranzactie.Transfer)
            .ToList();

    /// <summary>Σ al unei unități peste tot ce a scris documentul în cub (090g).</summary>
    public static N.Sold SoldUnitate(IObjectSpace os, Guid document, Guid unitate) {
        ArgumentNullException.ThrowIfNull(os);
        return Postari(os, document)
            .Where(p => p.Unitate == unitate)
            .Select(C.Randuri.Citeste)
            .Aggregate(N.Sold.Zero, (acumulat, postare) => acumulat + N.Sold.Din(postare));
    }

    /// <summary>Σ semnat (D − C) al unei partide, peste TOATE tranzacțiile cubului.</summary>
    public static decimal SoldPartida(IObjectSpace os, Guid unitate) =>
        os.GetObjectsQuery<C.Postare>()
            .Where(p => p.Unitate == unitate)
            .ToList()
            .Sum(p => p.Latura == N.Latura.Debit ? p.Valoare : -p.Valoare);

    public static void FaraRanduri(IObjectSpace os, Action<string, bool> check, string nume, Guid document) {
        ArgumentNullException.ThrowIfNull(check);
        check(nume, Tranzactii(os, document).Count == 0 && Postari(os, document).Count == 0);
    }

    /// <summary>STR-OPERARE + STR-ROUNDTRIP.</summary>
    public static void ProbaOperare(
            IObjectSpace os,
            Action<string, bool> check,
            string prefix,
            Document doc) {
        ArgumentNullException.ThrowIfNull(os);
        ArgumentNullException.ThrowIfNull(check);
        ArgumentNullException.ThrowIfNull(doc);

        var ale = Proprii(os, doc.ID);
        var operari = ale.Count(t => t.Fel == N.FelTranzactie.Operare);
        var mutari = ale.Count(t => t.Fel == N.FelTranzactie.Transfer);
        check($"STR-OPERARE {prefix}: cel mult o `Operare`, cel mult un `Transfer` de stoc, cel puțin "
            + "una din ele, cu data înregistrării (T-D2)",
            operari <= 1 && mutari <= 1 && ale.Count >= 1
            && ale.All(t => t.Data == doc.DataInregistrare && t.ScrisLa != default));

        var aleLor = ale.Select(t => t.ID).ToHashSet();
        var randuri = Postari(os, doc.ID).Where(r => aleLor.Contains(r.TranzactieId)).ToList();
        var citite = randuri.Select(C.Randuri.Citeste).ToList();
        check($"STR-OPERARE {prefix}: fiecare postare persistată e pe partiția spațiului ei "
            + "(`Spatiu` = `postare.Spatiu()`)",
            randuri.Count > 0
            && randuri.Zip(citite).All(pereche => pereche.First.Spatiu == N.Postari.Spatiu(pereche.Second)));

        var contract = Contractare.Contracteaza(os, doc);
        var asteptate = contract.Tranzactii.SelectMany(t => t.Postari).ToList();
        if (!MultisetEgal(citite, asteptate))
            Scrie(os, citite, asteptate);
        check($"STR-ROUNDTRIP {prefix}: `Randuri.Citeste` pe rândurile scrise = postările contractului, "
            + "egalitate STRUCTURALĂ pe multiset",
            contract.EsteAcceptat && MultisetEgal(citite, asteptate));
    }

    /// <summary>STR-STORNO.</summary>
    public static void ProbaStorno(
            IObjectSpace os, Action<string, bool> check, string prefix, Document doc, DateOnly dataStorno) {
        ArgumentNullException.ThrowIfNull(os);
        ArgumentNullException.ThrowIfNull(check);
        ArgumentNullException.ThrowIfNull(doc);

        var tranzactii = Tranzactii(os, doc.ID);
        var stornari = tranzactii.Where(t => t.Fel == N.FelTranzactie.Storno).ToList();
        var propriile = Proprii(os, doc.ID);
        check($"STR-STORNO {prefix}: O SINGURĂ tranzacție `Storno` peste tranzacțiile proprii "
            + $"({propriile.Count}), la data stornării (cubul e append-only, T-D2/N-r8)",
            tranzactii.Count == propriile.Count + 1 && stornari.Count == 1
            && stornari[0].Data == dataStorno);

        var aleLor = propriile.Select(t => t.ID).ToHashSet();
        var operare = Postari(os, doc.ID)
            .Where(r => aleLor.Contains(r.TranzactieId))
            .Select(r => C.Randuri.Citeste(r) with {
                InversaDin = new N.ReferintaPostare(r.ID, r.Spatiu),
            })
            .ToList();
        var storno = Postari(os, doc.ID, N.FelTranzactie.Storno).Select(C.Randuri.Citeste).ToList();
        var perioada = (dataStorno.Year * 100) + dataStorno.Month;
        var asteptate = N.Storno.Inverseaza(operare, doc.ID, dataStorno, perioada).Postari;
        if (!MultisetEgal(storno, asteptate))
            Scrie(os, storno, asteptate);
        check($"STR-STORNO {prefix}: postările stornării = `Storno.Inverseaza` pe rândurile proprii citite",
            MultisetEgal(storno, asteptate));

        var fiscale = storno.Where(p => p.Coordonate.PerioadaDeclarare != null).ToList();
        check($"STR-STORNO {prefix}: reperul fiscal al stornării e perioada STORNĂRII ({perioada}) pe "
            + $"postările care poartă una ({fiscale.Count}), restul rămân fără (N-D10 amendat)",
            fiscale.All(p => p.Coordonate.PerioadaDeclarare == perioada)
            && operare.Count(p => p.Coordonate.PerioadaDeclarare != null) == fiscale.Count);

        var solduri = operare.Concat(storno)
            .GroupBy(p => (p.Coordonate.Cont, p.Coordonate.Latura, p.Coordonate.Gestiune, p.Coordonate.Produs,
                p.Coordonate.Unitate, p.Coordonate.Partener, p.Coordonate.CodTva, p.Coordonate.Analiza, p.Cauza.Linie))
            .Select(g => (Valoare: g.Sum(p => p.Valoare), Cantitate: g.Sum(p => p.Cantitate)))
            .ToList();
        check($"STR-STORNO {prefix}: Σ cub a documentului = 0 pe FIECARE coordonată "
            + $"({solduri.Count} coordonate distincte), valoare și cantitate",
            solduri.Count > 0 && solduri.All(s => s.Valoare == 0m && s.Cantitate == 0m));
    }

    static bool MultisetEgal(IEnumerable<N.Postare> unele, IEnumerable<N.Postare> altele) {
        var stanga = Numara(unele);
        var dreapta = Numara(altele);
        return stanga.Count == dreapta.Count
            && stanga.All(pereche => dreapta.GetValueOrDefault(pereche.Key) == pereche.Value);
    }

    static Dictionary<N.Postare, int> Numara(IEnumerable<N.Postare> postari) {
        var cate = new Dictionary<N.Postare, int>();
        foreach (var postare in postari)
            cate[postare] = cate.GetValueOrDefault(postare) + 1;
        return cate;
    }

    static void Scrie(IObjectSpace os, IReadOnlyList<N.Postare> obtinut, IReadOnlyList<N.Postare> asteptat) {
        var stanga = Numara(obtinut);
        var dreapta = Numara(asteptat);
        foreach (var (postare, cate) in dreapta.Where(p => stanga.GetValueOrDefault(p.Key) != p.Value))
            Console.WriteLine($"       așteptat ×{cate}, obținut ×{stanga.GetValueOrDefault(postare)}: {postare}");
        foreach (var (postare, cate) in stanga.Where(p => !dreapta.ContainsKey(p.Key)))
            Console.WriteLine($"       în plus ×{cate}: {postare}");
    }

    sealed class ComutatorToleranta : IDisposable {
        readonly IObjectSpace os;
        readonly PoliticaTva politica;
        readonly decimal? vechi;

        public ComutatorToleranta(IObjectSpace os, PoliticaTva politica, decimal? valoare) {
            this.os = os;
            this.politica = politica;
            vechi = politica.TolerantaTaxa;
            politica.TolerantaTaxa = valoare;
            os.CommitChanges();
        }

        public void Dispose() {
            politica.TolerantaTaxa = vechi;
            os.CommitChanges();
        }
    }
}
