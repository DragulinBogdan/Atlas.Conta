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
    /// <param name="OperareStins">
    /// tranzacția <c>Operare</c> a stinsului CU conexul autogenerat absorbit (TR-D3):
    /// recepția stă pe NIR-ul conex, dar partida e a facturii.
    /// </param>
    /// <param name="TransferuriStins">
    /// postările de <c>Transfer</c> așezate deja pe PARTIDELE stinsului, de oricare
    /// stingător: plafonul e RESTUL partidei, nu ce a adus operarea.
    /// </param>
    /// <param name="Data">ziua faptului de stingere (<c>Imperechere.Data</c>).</param>
    public sealed record Cerere(
        Guid StingatorId,
        DateOnly DataStingator,
        IReadOnlyList<N.Postare> OperareStingator,
        IReadOnlyList<N.Postare> TransferuriStingator,
        Guid StinsId,
        DateOnly DataStins,
        IReadOnlyList<N.Postare> OperareStins,
        IReadOnlyList<N.Postare> TransferuriStins,
        decimal Suma,
        DateOnly Data);

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

        // Împerecherea e pe DOCUMENT, partida e pe CONT: se mută cel mult RESTUL
        // partidei stinsului pe contul de referință — operarea (cu conexul absorbit)
        // plus ce a primit deja partida —, restul rămâne pe a stingătorului.
        // Rândul invers (desfacerea, inversul la storno) desface EXACT transferurile
        // ACESTUI stingător către partida stinsului, nu restul de azi al ei.
        var invers = cerere.Suma < 0m;
        var plafon = invers
            ? Math.Abs(Net(
                cerere.TransferuriStins.Where(p => p.Cauza.Document == cerere.StingatorId),
                referinta.Cont))
            : Math.Abs(Net(cerere.OperareStins.Concat(cerere.TransferuriStins), referinta.Cont));
        var mutata = Math.Min(Math.Abs(cerere.Suma), plafon);
        if (mutata <= 0m)
            return Sare(invers
                ? $"partida stinsului n-a primit nimic de la acest stingător pe contul {referinta.Cont}"
                : $"documentul stins n-are rest pe contul de referință {referinta.Cont}");

        var proprie = N.Unitate.DeschidePartida(
            referinta.Cont, tert, cerere.StingatorId, cerere.DataStingator);
        var stinsa = N.Unitate.DeschidePartida(
            referinta.Cont, tert, cerere.StinsId, cerere.DataStins);
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
            cerere.Data,
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
