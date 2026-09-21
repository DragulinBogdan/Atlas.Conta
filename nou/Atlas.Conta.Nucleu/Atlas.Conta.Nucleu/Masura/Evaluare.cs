namespace Atlas.Conta.Nucleu;

public static class Evaluare {
    public static decimal Iesire(Sold inainte, decimal cantitate, Rotunjire rotunjire) {
        ArgumentNullException.ThrowIfNull(inainte);
        ArgumentNullException.ThrowIfNull(rotunjire);
        if (cantitate <= 0m)
            throw new ArgumentException($"cantitatea {cantitate} se cere > 0.", nameof(cantitate));
        if (inainte.Cantitate <= 0m || cantitate > inainte.Cantitate)
            throw new RefuzException(new Refuz(
                Coduri.StocInsuficient,
                $"ies {cantitate}, dar unitatea are {inainte.Cantitate}",
                null));
        // N-D7: ultima ieșire ia restul, ca valoarea să nu rămână pe cantitate zero.
        return cantitate == inainte.Cantitate
            ? inainte.Net
            : rotunjire.Bani(cantitate * inainte.Net / inainte.Cantitate);
    }
}
