namespace Atlas.Conta.Nucleu;

public static class Tva {
    public static (decimal Valoare, decimal Taxa) Linie(
        decimal net,
        RegimTva regim,
        decimal cota,
        DirectieTva directie) =>
        regim switch {
            RegimTva.Capitalizat => (net * (1m + cota / 100m), 0m),
            RegimTva.TaxareInversa when directie == DirectieTva.Colectat => (net, 0m),
            RegimTva.Normal or RegimTva.TaxareInversa => (net, net * cota / 100m),
            _ => (net, 0m),
        };

    public static TaxaDocument PeDocument(IReadOnlyList<LinieTva> linii, DirectieTva directie, Rotunjire rotunjire) {
        ArgumentNullException.ThrowIfNull(linii);
        ArgumentNullException.ThrowIfNull(rotunjire);
        var perCota = new Dictionary<(RegimTva Regim, decimal Cota), decimal>();
        var perLinie = new Dictionary<Guid, decimal>();
        foreach (var grup in linii.GroupBy(linie => (linie.Regim, linie.Cota))) {
            var membri = grup.ToList();
            var nerotunjita = 0m;
            foreach (var membru in membri)
                nerotunjita += Linie(membru.Net, membru.Regim, membru.Cota, directie).Taxa;
            var taxa = rotunjire.Bani(nerotunjita);
            perCota.Add(grup.Key, taxa);
            var ponderi = membri.Select(membru => Math.Abs(membru.Net)).ToList();
            // Taxa se decide pe document × cotă, apoi se POSTEAZĂ per linie (090j).
            var cote = ponderi.Sum() == 0m
                ? new decimal[membri.Count]
                : Repartizare.Hamilton(taxa, ponderi, Scara.Bani);
            for (var i = 0; i < membri.Count; i++)
                perLinie.Add(membri[i].Linie, cote[i]);
        }
        return new TaxaDocument(perCota, perLinie);
    }

    public static Refuz? ValideazaData(decimal taxaData, decimal taxaCalculata, decimal toleranta) {
        if (toleranta < 0m)
            throw new ArgumentException($"toleranța {toleranta} e negativă.", nameof(toleranta));
        var abatere = Math.Abs(taxaData - taxaCalculata);
        return abatere <= toleranta
            ? null
            : new Refuz(
                Coduri.TvaInAfaraTolerantei,
                $"taxa dată {taxaData} se abate cu {abatere} de la {taxaCalculata}, peste toleranța {toleranta}",
                null);
    }
}
