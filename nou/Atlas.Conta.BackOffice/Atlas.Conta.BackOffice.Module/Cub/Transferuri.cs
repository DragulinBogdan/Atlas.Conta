#nullable enable
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.Module.Cub;

/// <summary>
/// S-D13: împerecherea creată DUPĂ operare mută suma de pe partida proprie a
/// stingătorului pe partida stinsului, pe contul comun cu rol de terț. Funcție
/// pură, refolosită de materializare și de gate-ul read-only.
/// </summary>
public static class Transferuri {
    public const string PartidaProprieInsuficienta = "PARTIDA_PROPRIE_INSUFICIENTA";

    /// <param name="TransferuriStingator">
    /// postările de <c>Transfer</c> deja scrise ale stingătorului: plafonul partidei
    /// proprii e ce a mai rămas pe ea, nu ce a adus operarea.
    /// </param>
    public sealed record Cerere(
        Guid StingatorId,
        DateOnly DataStingator,
        IReadOnlyList<N.Postare> OperareStingator,
        IReadOnlyList<N.Postare> TransferuriStingator,
        Guid StinsId,
        DateOnly DataStins,
        IReadOnlyList<N.Postare> OperareStins,
        decimal Suma);

    /// <param name="Sarit">motivul pentru care împerecherea n-are corespondent în cub.</param>
    public sealed record Rezultat(N.Mutare? Mutare, DateOnly Data, N.Refuz? Refuz, string? Sarit);

    /// <summary>Partida de referință a unui document: postarea lui de terț cu |Valoare| maximă.</summary>
    readonly record struct Referinta(Guid Cont, N.Latura Latura, Guid? Partener);

    public static Rezultat Muta(Cerere cerere) {
        ArgumentNullException.ThrowIfNull(cerere);
        if (cerere.OperareStingator.Count == 0)
            return Sare("documentul care stinge n-are tranzacție `Operare` în cub");
        if (cerere.OperareStins.Count == 0)
            return Sare("documentul stins n-are tranzacție `Operare` în cub");
        // Profil fără conturi cu rol de terț (bugetar): nu există partidă de mutat.
        if (Cea(cerere.OperareStingator) is not { } referinta)
            return Sare("documentul care stinge n-are nicio postare pe un cont cu rol de terț");
        if ((Cea(cerere.OperareStins)?.Partener ?? referinta.Partener) is not Guid tert)
            return Sare("nicio partidă a celor două documente nu poartă partener");

        // Împerecherea e pe DOCUMENT, partida e pe CONT: se mută cel mult cât ține
        // partida stinsului pe contul de referință, restul rămâne pe a stingătorului.
        var mutata = Math.Min(Math.Abs(cerere.Suma), Math.Abs(Net(cerere.OperareStins, referinta.Cont)));
        if (mutata <= 0m)
            return Sare($"documentul stins n-are sold pe contul de referință {referinta.Cont}");

        var proprie = N.Unitate.DeschidePartida(
            referinta.Cont, tert, cerere.StingatorId, cerere.DataStingator);
        var stinsa = N.Unitate.DeschidePartida(
            referinta.Cont, tert, cerere.StinsId, cerere.DataStins);
        // Rândul invers (desfacerea, inversul la storno) desface EXACT transferul
        // dinainte: plafonul lui e suma originalului, nu soldul de azi al partidei.
        var invers = cerere.Suma < 0m;
        if (!invers) {
            var disponibil = PePartida(
                cerere.OperareStingator.Concat(cerere.TransferuriStingator), proprie.Id, referinta.Latura);
            if (disponibil < mutata)
                return Refuza(PartidaProprieInsuficienta,
                    $"Partida proprie a documentului care stinge ține {disponibil} pe contul "
                    + $"{referinta.Cont}, iar stingerea cere {mutata}.");
        }

        return new Rezultat(
            new N.Mutare(
                Capatul(referinta.Cont, tert, invers ? stinsa : proprie),
                Capatul(referinta.Cont, tert, invers ? proprie : stinsa),
                referinta.Latura,
                0m,
                0m,
                mutata,
                new N.Cauza(cerere.StingatorId, null)),
            cerere.DataStingator > cerere.DataStins ? cerere.DataStingator : cerere.DataStins,
            null,
            null);
    }

    static N.Capat Capatul(Guid cont, Guid tert, N.Unitate partida) =>
        new() { Cont = cont, Partener = tert, Unitate = partida };

    static Referinta? Cea(IReadOnlyList<N.Postare> operare) {
        Referinta? cea = null;
        var maxim = 0m;
        foreach (var postare in operare) {
            if (postare.Coordonate.Unitate is not { Fel: N.FelUnitate.Partida })
                continue;
            var absolut = Math.Abs(postare.Valoare);
            if (cea is not null && absolut <= maxim)
                continue;
            cea = new Referinta(
                postare.Coordonate.Cont, postare.Coordonate.Latura, postare.Coordonate.Partener);
            maxim = absolut;
        }
        return cea;
    }

    static decimal Net(IEnumerable<N.Postare> postari, Guid cont) =>
        postari.Where(p => p.Coordonate.Cont == cont)
            .Sum(p => p.Coordonate.Latura == N.Latura.Debit ? p.Valoare : -p.Valoare);

    static decimal PePartida(IEnumerable<N.Postare> postari, Guid unitate, N.Latura latura) =>
        postari.Where(p => p.Coordonate.Unitate?.Id == unitate)
            .Sum(p => p.Coordonate.Latura == latura ? p.Valoare : -p.Valoare);

    static Rezultat Sare(string motiv) => new(null, default, null, motiv);

    static Rezultat Refuza(string cod, string mesaj) =>
        new(null, default, new N.Refuz(cod, mesaj, null), null);
}
