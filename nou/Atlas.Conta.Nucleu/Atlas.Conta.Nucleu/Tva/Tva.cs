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
            var taxe = membri
                .Select(membru => Linie(membru.Net, membru.Regim, membru.Cota, directie).Taxa)
                .ToList();
            var taxa = rotunjire.Bani(taxe.Sum());
            var negativa = rotunjire.Bani(taxe.Where(t => t < 0m).Sum());
            perCota.Add(grup.Key, taxa);
            // Taxa se decide pe document × cotă, apoi se POSTEAZĂ per linie (090j); laturile de semn
            // se repartizează separat, ca o linie să nu-și piardă semnul într-un grup mixt.
            var cote = new decimal[membri.Count];
            Repartizeaza(membri, taxe, 1, taxa - negativa, cote);
            Repartizeaza(membri, taxe, -1, negativa, cote);
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

    static void Repartizeaza(
        IReadOnlyList<LinieTva> membri,
        IReadOnlyList<decimal> taxe,
        int semn,
        decimal total,
        decimal[] cote) {
        var indici = new List<int>();
        var ponderi = new List<decimal>();
        for (var i = 0; i < membri.Count; i++)
            if (Math.Sign(taxe[i]) == semn) {
                indici.Add(i);
                ponderi.Add(Math.Abs(membri[i].Net));
            }
        if (ponderi.Sum() == 0m) {
            if (total != 0m)
                throw new InvalidOperationException(
                    $"latura de semn {semn} n-are pondere, dar îi revine taxa {total}.");
            return;
        }
        var parti = Repartizare.Hamilton(total, ponderi, Scara.Bani);
        for (var i = 0; i < indici.Count; i++)
            cote[indici[i]] = parti[i];
    }
}
