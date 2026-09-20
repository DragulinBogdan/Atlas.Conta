#nullable enable
using System.Globalization;
using System.Text;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.ModelCheck;

/// <summary>
/// Proiecția pe care se compară oracolul cu declarația (B-D7): coordonatele care
/// contează, măsurile semnate, egalitate structurală, diff pe MULTISET.
/// </summary>
sealed record PostareComparabila(
    Guid Cont,
    N.Latura Latura,
    Guid? Gestiune,
    Guid? Produs,
    Guid? Unitate,
    Guid? Partener,
    N.CodTva? CodTva,
    int? PerioadaDeclarare,
    N.Analiza Analiza,
    decimal Cantitate,
    decimal ValoareSemnata,
    Guid? Linie);

sealed class RaportComparatie {
    readonly Func<Guid, string> nume;

    internal RaportComparatie(
            IReadOnlyList<PostareComparabila> inPlus,
            IReadOnlyList<PostareComparabila> lipsa,
            Func<Guid, string> nume) {
        InPlus = inPlus;
        Lipsa = lipsa;
        this.nume = nume;
    }

    public IReadOnlyList<PostareComparabila> InPlus { get; }

    public IReadOnlyList<PostareComparabila> Lipsa { get; }

    public bool Egal => InPlus.Count == 0 && Lipsa.Count == 0;

    public override string ToString() {
        if (Egal)
            return "multiset-urile coincid";
        var text = new StringBuilder();
        foreach (var postare in Lipsa)
            text.AppendLine($"       LIPSĂ  {Comparabil.Scrie(postare, nume)}");
        foreach (var postare in InPlus)
            text.AppendLine($"       ÎN PLUS {Comparabil.Scrie(postare, nume)}");
        return text.ToString().TrimEnd();
    }
}

static class Comparabil {
    /// <summary>
    /// N-D4: cantitatea se compară DOAR pe postările cu unitate de fel lot, iar
    /// gestiunea VIRTUALĂ se citește ca lipsă — capătul virtual al declarantului
    /// (−q pe postarea de terț, pe gestiunea structurală) nu există în oracol.
    /// </summary>
    public static PostareComparabila Proiecteaza(N.Postare postare) {
        ArgumentNullException.ThrowIfNull(postare);
        var coordonate = postare.Coordonate;
        return new PostareComparabila(
            coordonate.Cont,
            coordonate.Latura,
            N.GestiuniVirtuale.Este(coordonate.Gestiune) ? null : coordonate.Gestiune,
            coordonate.Produs,
            coordonate.Unitate?.Id,
            coordonate.Partener,
            coordonate.CodTva,
            coordonate.PerioadaDeclarare,
            coordonate.Analiza,
            coordonate.Unitate?.Fel == N.FelUnitate.Lot ? postare.Cantitate : 0m,
            coordonate.Latura == N.Latura.Debit ? postare.Valoare : -postare.Valoare,
            postare.Cauza.Linie);
    }

    public static List<PostareComparabila> Proiecteaza(N.Tranzactie tranzactie) {
        ArgumentNullException.ThrowIfNull(tranzactie);
        return [.. tranzactie.Postari.Select(Proiecteaza)];
    }

    public static List<PostareComparabila> Proiecteaza(IEnumerable<N.Tranzactie> tranzactii) {
        ArgumentNullException.ThrowIfNull(tranzactii);
        return [.. tranzactii.SelectMany(t => t.Postari).Select(Proiecteaza)];
    }

    public static RaportComparatie Compara(
            IEnumerable<PostareComparabila> asteptat,
            IEnumerable<PostareComparabila> obtinut,
            Func<Guid, string>? nume = null) {
        ArgumentNullException.ThrowIfNull(asteptat);
        ArgumentNullException.ThrowIfNull(obtinut);
        var deAsteptat = Numara(asteptat);
        var deObtinut = Numara(obtinut);
        var lipsa = new List<PostareComparabila>();
        var inPlus = new List<PostareComparabila>();
        foreach (var (postare, cate) in deAsteptat)
            for (var i = deObtinut.GetValueOrDefault(postare); i < cate; i++)
                lipsa.Add(postare);
        foreach (var (postare, cate) in deObtinut)
            for (var i = deAsteptat.GetValueOrDefault(postare); i < cate; i++)
                inPlus.Add(postare);
        return new RaportComparatie(inPlus, lipsa, nume ?? Scurt);
    }

    public static string Scrie(PostareComparabila postare, Func<Guid, string>? nume = null) {
        ArgumentNullException.ThrowIfNull(postare);
        var simbol = nume ?? Scurt;
        var text = new StringBuilder();
        text.Append(postare.Latura == N.Latura.Debit ? "D " : "C ")
            .Append(simbol(postare.Cont))
            .Append(' ')
            .Append(Numar(Math.Abs(postare.ValoareSemnata)));
        if (postare.Cantitate != 0m)
            text.Append(" × ").Append(Numar(postare.Cantitate));
        if (postare.Gestiune is Guid gestiune)
            text.Append(" gest=").Append(simbol(gestiune));
        if (postare.Produs is Guid produs)
            text.Append(" prod=").Append(simbol(produs));
        if (postare.Unitate is Guid unitate)
            text.Append(" unit=").Append(Scurt(unitate));
        if (postare.Partener is Guid partener)
            text.Append(" part=").Append(simbol(partener));
        if (postare.CodTva is { } cod)
            text.Append(" tva=").Append(Scurt(cod.TipTva)).Append('/').Append(cod.Sens).Append('/').Append(cod.Rol);
        if (postare.PerioadaDeclarare is int perioada)
            text.Append(" per=").Append(perioada);
        if (postare.Analiza != N.Analiza.Fara)
            text.Append(" analiza=").Append(Analiza(postare.Analiza, simbol));
        if (postare.Linie is Guid linie)
            text.Append(" linie=").Append(Scurt(linie));
        return text.ToString();
    }

    static string Analiza(N.Analiza analiza, Func<Guid, string> simbol) {
        var parti = new List<string>();
        void Adauga(string eticheta, Guid? id) {
            if (id is Guid valoare)
                parti.Add($"{eticheta}:{simbol(valoare)}");
        }
        Adauga("cf", analiza.CodFunctional);
        Adauga("ce", analiza.CodEconomic);
        Adauga("sf", analiza.SursaFinantare);
        Adauga("uo", analiza.UnitateOrganizatorica);
        Adauga("pr", analiza.Proiect);
        Adauga("cc", analiza.CentruCost);
        return string.Join("+", parti);
    }

    static Dictionary<PostareComparabila, int> Numara(IEnumerable<PostareComparabila> postari) {
        var cate = new Dictionary<PostareComparabila, int>();
        foreach (var postare in postari)
            cate[postare] = cate.GetValueOrDefault(postare) + 1;
        return cate;
    }

    static string Numar(decimal valoare) => valoare.ToString("0.####", CultureInfo.InvariantCulture);

    static string Scurt(Guid id) => id.ToString()[..8];
}
