using Xunit;

namespace Atlas.Conta.Nucleu.Teste;

public class RepartizareTeste {
    [Fact]
    public void SumaCotelorEExactTotalul() =>
        Proprietate.Verifica(Proprietate.Cazuri, (aleator, _) => {
            var scara = ScaraAleatoare(aleator);
            var total = Total(aleator, scara);
            var ponderi = Ponderi(aleator);
            var cote = Repartizare.Hamilton(total, ponderi, scara);
            Assert.Equal(ponderi.Count, cote.Length);
            Assert.Equal(total, cote.Sum());
            foreach (var cota in cote)
                Assert.True(Scara.EsteLa(cota, scara), $"cota {cota} nu e la scara {scara}");
        });

    [Fact]
    public void FiecareCotaELaCelMultOUnitateDeProportie() =>
        Proprietate.Verifica(Proprietate.Cazuri, (aleator, _) => {
            var scara = ScaraAleatoare(aleator);
            var total = Total(aleator, scara);
            var ponderi = Ponderi(aleator);
            var suma = ponderi.Sum();
            var unitate = Gen.Pas(scara);
            var cote = Repartizare.Hamilton(total, ponderi, scara);
            for (var i = 0; i < ponderi.Count; i++) {
                var exact = total * ponderi[i] / suma;
                Assert.True(
                    Math.Abs(cote[i] - exact) < unitate,
                    $"cota {cote[i]} se abate de {exact} cu cel puțin o unitate de {unitate}");
            }
        });

    [Fact]
    public void CoteleIauSemnulTotalului() =>
        Proprietate.Verifica(Proprietate.Cazuri, (aleator, _) => {
            var scara = ScaraAleatoare(aleator);
            var total = Total(aleator, scara);
            var ponderi = Ponderi(aleator);
            var cote = Repartizare.Hamilton(total, ponderi, scara);
            for (var i = 0; i < ponderi.Count; i++) {
                Assert.True(
                    cote[i] == 0m || Math.Sign(cote[i]) == Math.Sign(total),
                    $"cota {cote[i]} nu are semnul totalului {total}");
                if (ponderi[i] == 0m)
                    Assert.Equal(0m, cote[i]);
            }
        });

    [Fact]
    public void LaEgalitateRestulMergeLaPrimele() =>
        Assert.Equal(new[] { 0.34m, 0.33m, 0.33m }, Repartizare.Hamilton(1.00m, [1m, 1m, 1m], Scara.Bani));

    [Fact]
    public void RestulSeImparteInUnitati_NuIlIaUltima() {
        var cote = Repartizare.Hamilton(1.00m, [1m, 1m, 1m, 1m, 1m, 1m], Scara.Bani);
        Assert.Equal(new[] { 0.17m, 0.17m, 0.17m, 0.17m, 0.16m, 0.16m }, cote);
        // „ultima ia restul" ar da 0.16 × 5 + 0.20 (AsamblareApply.cs:384-391, N-D8).
        Assert.NotEqual(0.20m, cote[5]);
    }

    [Fact]
    public void TotalulNegativSeImparteSimetric() =>
        Assert.Equal(new[] { -0.34m, -0.33m, -0.33m }, Repartizare.Hamilton(-1.00m, [1m, 1m, 1m], Scara.Bani));

    [Fact]
    public void TotalulZeroDaCoteZero() =>
        Assert.Equal(new[] { 0m, 0m }, Repartizare.Hamilton(0m, [3m, 7m], Scara.Bani));

    [Fact]
    public void PondereaZeroNuPrimesteNimic() =>
        Assert.Equal(new[] { 0m, 1.00m, 0m }, Repartizare.Hamilton(1.00m, [0m, 5m, 0m], Scara.Bani));

    [Fact]
    public void PonderileTotalZeroSuntRefuzate() =>
        Assert.Throws<ArgumentException>(() => Repartizare.Hamilton(1.00m, [0m, 0m], Scara.Bani));

    [Fact]
    public void ListaGoalaERefuzata() =>
        Assert.Throws<ArgumentException>(() => Repartizare.Hamilton(1.00m, Array.Empty<decimal>(), Scara.Bani));

    [Fact]
    public void PondereaNegativaERefuzata() =>
        Assert.Throws<ArgumentException>(() => Repartizare.Hamilton(1.00m, [1m, -1m, 3m], Scara.Bani));

    [Fact]
    public void TotalulInAfaraScariiERefuzat() =>
        Assert.Throws<ArgumentException>(() => Repartizare.Hamilton(1.005m, [1m, 1m], Scara.Bani));

    [Fact]
    public void ScaraInAfaraIntervaluluiERefuzata() =>
        Assert.ThrowsAny<ArgumentException>(() => Repartizare.Hamilton(1m, [1m], -1));

    [Fact]
    public void GrupurileTotalZeroSuntRefuzate() =>
        Assert.Throws<ArgumentException>(() => Repartizare.Hamilton(
            1.00m,
            new IReadOnlyList<decimal>[] { new[] { 0m }, new[] { 0m, 0m } },
            Scara.Bani));

    [Fact]
    public void IerarhicIntaiIntreGrupuriApoiInInterior() =>
        Proprietate.Verifica(Proprietate.Cazuri, (aleator, _) => {
            var scara = ScaraAleatoare(aleator);
            var total = Total(aleator, scara);
            var grupuri = Grupuri(aleator);
            var sume = grupuri.Select(grup => grup.Sum()).ToList();
            var cote = Repartizare.Hamilton(total, grupuri, scara);
            var peGrupuri = Repartizare.Hamilton(total, sume, scara);
            Assert.Equal(grupuri.Count, cote.Length);
            for (var g = 0; g < grupuri.Count; g++) {
                Assert.Equal(grupuri[g].Count, cote[g].Length);
                Assert.Equal(peGrupuri[g], cote[g].Sum());
                if (sume[g] == 0m)
                    Assert.All(cote[g], cota => Assert.Equal(0m, cota));
                else
                    Assert.Equal(Repartizare.Hamilton(peGrupuri[g], grupuri[g], scara), cote[g]);
            }
            Assert.Equal(total, cote.SelectMany(grup => grup).Sum());
        });

    static int ScaraAleatoare(Random aleator) => aleator.Next(2) == 0 ? Scara.Bani : Scara.Cantitate;

    static decimal Total(Random aleator, int scara) => Gen.Zecimal(aleator, -99999m, 99999m, scara);

    static List<decimal> Ponderi(Random aleator) {
        List<decimal> ponderi;
        var cate = 1 + aleator.Next(12);
        do {
            ponderi = [];
            for (var i = 0; i < cate; i++)
                ponderi.Add(aleator.Next(4) == 0 ? 0m : Gen.Zecimal(aleator, 0.001m, 999.999m, Scara.Cantitate));
        } while (ponderi.Sum() == 0m);
        return ponderi;
    }

    static List<IReadOnlyList<decimal>> Grupuri(Random aleator) {
        var grupuri = new List<IReadOnlyList<decimal>>();
        var cate = 1 + aleator.Next(4);
        for (var g = 0; g < cate; g++) {
            if (aleator.Next(5) == 0)
                grupuri.Add(new decimal[] { 0m, 0m });
            else
                grupuri.Add(Ponderi(aleator));
        }
        if (grupuri.All(grup => grup.Sum() == 0m))
            grupuri[0] = Ponderi(aleator);
        return grupuri;
    }
}
