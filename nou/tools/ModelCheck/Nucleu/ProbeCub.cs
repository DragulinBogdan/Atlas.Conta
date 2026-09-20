#nullable enable
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Declaratii;
using Atlas.Conta.BackOffice.Module.Motor;
using DevExpress.ExpressApp;
using C = Atlas.Conta.BackOffice.Module.Cub;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.ModelCheck;

/// <summary>
/// Probele strangler-ului (S-D8): ce a rămas PERSISTAT după comanda motorului
/// vechi, pe tipul comutat local pe <c>PosteazaInCub</c>.
/// </summary>
static class ProbeCub {
    /// <summary>
    /// Comută `PosteazaInCub` pe tipurile documentelor date și îl RESTAUREAZĂ la
    /// ieșire (S-D8): seed-ul rămâne `false` până la pașii 4–5.
    /// </summary>
    public static IDisposable Migrat(IObjectSpace os, params Document[] documente) {
        ArgumentNullException.ThrowIfNull(os);
        ArgumentNullException.ThrowIfNull(documente);
        return new Comutator(os, [.. documente.Select(d => MotorOperare.ClasaReala(d).Name).Distinct()]);
    }

    public static IDisposable MigratPeClasa(IObjectSpace os, params string[] clrTypes) =>
        new Comutator(os, clrTypes);

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

    public static void FaraRanduri(IObjectSpace os, Action<string, bool> check, string nume, Guid document) {
        ArgumentNullException.ThrowIfNull(check);
        check(nume, Tranzactii(os, document).Count == 0 && Postari(os, document).Count == 0);
    }

    /// <summary>STR-OPERARE + STR-ROUNDTRIP.</summary>
    public static void ProbaOperare(
            IObjectSpace os,
            Action<string, bool> check,
            string prefix,
            Document doc,
            IReadOnlyDictionary<Guid, Guid>? conexe = null) {
        ArgumentNullException.ThrowIfNull(os);
        ArgumentNullException.ThrowIfNull(check);
        ArgumentNullException.ThrowIfNull(doc);

        var tranzactii = Tranzactii(os, doc.ID);
        var operari = tranzactii.Where(t => t.Fel == N.FelTranzactie.Operare).ToList();
        check($"STR-OPERARE {prefix}: EXACT o tranzacție `Operare` a documentului, cu data înregistrării",
            operari.Count == 1 && operari[0].Data == doc.DataInregistrare && operari[0].ScrisLa != default);

        var randuri = Postari(os, doc.ID, N.FelTranzactie.Operare);
        var citite = randuri.Select(C.Randuri.Citeste).ToList();
        check($"STR-OPERARE {prefix}: fiecare postare persistată e pe partiția spațiului ei "
            + "(`Spatiu` = `postare.Spatiu()`)",
            randuri.Count > 0
            && randuri.Zip(citite).All(pereche => pereche.First.Spatiu == N.Postari.Spatiu(pereche.Second)));

        var aleLui = new List<Guid> { doc.ID };
        var conexeAleLui = new Dictionary<Guid, Guid>();
        if (conexe is not null)
            foreach (var (conex, sursa) in conexe)
                if (sursa == doc.ID) {
                    aleLui.Add(conex);
                    conexeAleLui[conex] = sursa;
                }

        Normalizari.Reseteaza();
        var oracol = Normalizari.Toate(
            CubDinRegistre.Transforma(os, aleLui),
            Normalizari.Citeste(os, aleLui, conexeAleLui));
        Normalizari.Reseteaza();
        var iar = Normalizari.Toate(
            CubDinRegistre.Transforma(os, aleLui),
            Normalizari.Citeste(os, aleLui, conexeAleLui));
        check($"STR-ORACOL {prefix}: două transformări consecutive dau ACELAȘI oracol — citirile sunt "
            + "ordonate pe secvența liniilor, nu pe heap-ul Postgres (S-D6/B-r11)",
            MultisetEgal(oracol.SelectMany(t => t.Postari), iar.SelectMany(t => t.Postari)));
        var raport = Comparabil.Compara(
            Comparabil.Proiecteaza(oracol),
            [.. citite.Select(Comparabil.Proiecteaza)],
            ProbeNucleu.Nume(os, oracol));
        if (!raport.Egal)
            Console.WriteLine(raport.ToString());
        check($"STR-OPERARE {prefix}: postările PERSISTATE = registrele normalizate (B-D8)",
            raport.Egal && Normalizari.Avertismente.Count == 0);

        var contract = Contractare.Contracteaza(os, doc);
        var asteptate = contract.Tranzactie?.Postari ?? [];
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
        check($"STR-STORNO {prefix}: a DOUA tranzacție, de fel `Storno`, la data stornării "
            + "(cubul e append-only)",
            tranzactii.Count == 2 && stornari.Count == 1 && stornari[0].Data == dataStorno);

        var operare = Postari(os, doc.ID, N.FelTranzactie.Operare).Select(C.Randuri.Citeste).ToList();
        var storno = Postari(os, doc.ID, N.FelTranzactie.Storno).Select(C.Randuri.Citeste).ToList();
        var perioada = (dataStorno.Year * 100) + dataStorno.Month;
        var asteptate = N.Storno.Inverseaza(operare, doc.ID, dataStorno, perioada).Postari;
        if (!MultisetEgal(storno, asteptate))
            Scrie(os, storno, asteptate);
        check($"STR-STORNO {prefix}: postările stornării = `Storno.Inverseaza` pe rândurile `Operare` citite",
            MultisetEgal(storno, asteptate));

        var fiscale = storno.Where(p => p.Coordonate.PerioadaDeclarare != null).ToList();
        check($"STR-STORNO {prefix}: reperul fiscal al stornării e perioada STORNĂRII ({perioada}) pe "
            + $"postările care poartă una ({fiscale.Count}), restul rămân fără (N-D10 amendat)",
            fiscale.All(p => p.Coordonate.PerioadaDeclarare == perioada)
            && operare.Count(p => p.Coordonate.PerioadaDeclarare != null) == fiscale.Count);

        var solduri = operare.Concat(storno)
            .Select(Comparabil.Proiecteaza)
            .GroupBy(p => p with { Cantitate = 0m, ValoareSemnata = 0m, PerioadaDeclarare = null })
            .Select(g => (g.Sum(p => p.ValoareSemnata), g.Sum(p => p.Cantitate)))
            .ToList();
        check($"STR-STORNO {prefix}: Σ cub a documentului = 0 pe FIECARE coordonată "
            + $"({solduri.Count} coordonate distincte), valoare și cantitate",
            solduri.Count > 0 && solduri.All(s => s.Item1 == 0m && s.Item2 == 0m));
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
        var nume = ProbeNucleu.Nume(
            os,
            [new N.Tranzactie(N.FelTranzactie.Operare, default, Guid.Empty, asteptat)],
            new N.Tranzactie(N.FelTranzactie.Operare, default, Guid.Empty, obtinut));
        Console.WriteLine(Comparabil.Compara(
            [.. asteptat.Select(Comparabil.Proiecteaza)],
            [.. obtinut.Select(Comparabil.Proiecteaza)],
            nume).ToString());
    }

    sealed class Comutator : IDisposable {
        readonly IObjectSpace os;
        readonly List<(TipDocument Tip, bool Vechi)> stari = [];

        public Comutator(IObjectSpace os, IReadOnlyList<string> clrTypes) {
            this.os = os;
            foreach (var clrType in clrTypes.Distinct()) {
                var tip = os.FirstOrDefault<TipDocument>(t => t.ClrType == clrType)
                    ?? throw new InvalidOperationException($"Lipsește ancora TipDocument pentru {clrType}.");
                stari.Add((tip, tip.PosteazaInCub));
                tip.PosteazaInCub = true;
            }
            os.CommitChanges();
        }

        public void Dispose() {
            foreach (var (tip, vechi) in stari)
                tip.PosteazaInCub = vechi;
            os.CommitChanges();
        }
    }
}
