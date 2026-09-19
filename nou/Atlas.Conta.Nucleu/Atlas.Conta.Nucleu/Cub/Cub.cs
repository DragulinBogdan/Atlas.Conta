namespace Atlas.Conta.Nucleu;

public static class Cub {
    public static IReadOnlyDictionary<TCheie, Sold> Sold<TCheie>(
        IEnumerable<Tranzactie> tranzactii,
        Func<Postare, TCheie> cheie,
        Func<Postare, bool>? filtru = null,
        DateOnly? panaLa = null,
        DateOnly? dupa = null,
        bool includeTransfer = true)
        where TCheie : notnull {
        ArgumentNullException.ThrowIfNull(tranzactii);
        ArgumentNullException.ThrowIfNull(cheie);
        var solduri = new Dictionary<TCheie, Sold>();
        foreach (var tranzactie in tranzactii) {
            if (!Intra(tranzactie, panaLa, dupa, includeTransfer))
                continue;
            foreach (var postare in tranzactie.Postari) {
                if (filtru is not null && !filtru(postare))
                    continue;
                var k = cheie(postare);
                var acumulat = solduri.TryGetValue(k, out var sold) ? sold : Nucleu.Sold.Zero;
                solduri[k] = acumulat + Nucleu.Sold.Din(postare);
            }
        }
        return solduri;
    }

    // Excluderea transferului e parametru al citirii, nu însușire a postării (090f).
    static bool Intra(Tranzactie tranzactie, DateOnly? panaLa, DateOnly? dupa, bool includeTransfer) =>
        (includeTransfer || tranzactie.Fel != FelTranzactie.Transfer)
        && (panaLa is not { } sus || tranzactie.Data <= sus)
        && (dupa is not { } jos || tranzactie.Data > jos);
}
