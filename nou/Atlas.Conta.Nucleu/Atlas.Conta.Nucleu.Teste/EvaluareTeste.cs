using Xunit;

namespace Atlas.Conta.Nucleu.Teste;

public class EvaluareTeste {
    [Fact]
    public void CantitateaZeroLasaValoareaZero() {
        var incalcariCuPretInghetat = 0;
        var goliri = 0;
        Proprietate.Verifica(Proprietate.Cazuri, (aleator, _) => {
            var rotunjire = new Rotunjire(MidpointRounding.AwayFromZero);
            var sold = Sold.Zero;
            var inghetat = Sold.Zero;
            var pretIntai = 0m;
            var intrari = 0m;
            var iesiri = 0m;
            foreach (var pas in Istorie(aleator)) {
                if (pas.Intrare) {
                    var valoare = rotunjire.Bani(pas.Cantitate * pas.Pret);
                    if (pretIntai == 0m)
                        pretIntai = pas.Pret;
                    sold += new Sold(valoare, 0m, pas.Cantitate, 0m);
                    inghetat += new Sold(valoare, 0m, pas.Cantitate, 0m);
                    intrari += valoare;
                } else {
                    if (sold.Cantitate <= 0m)
                        continue;
                    var cantitate = pas.Goleste
                        ? sold.Cantitate
                        : Gen.Zecimal(aleator, 0.001m, sold.Cantitate, Scara.Cantitate);
                    var valoare = Evaluare.Iesire(sold, cantitate, rotunjire);
                    sold += new Sold(0m, valoare, -cantitate, 0m);
                    iesiri += valoare;
                    // Contra-proba N-D7: regula veche evaluează cu prețul înghețat al lotului.
                    inghetat += new Sold(0m, IesirePretInghetat(cantitate, pretIntai, rotunjire), -cantitate, 0m);
                }
                if (sold.Cantitate == 0m) {
                    goliri++;
                    Assert.Equal(0m, sold.Net);
                    Assert.Equal(intrari, iesiri);
                }
                if (inghetat.Cantitate == 0m && inghetat.Net != 0m)
                    incalcariCuPretInghetat++;
            }
        });
        Assert.True(goliri > 0, "generatorul n-a golit niciun lot");
        Assert.True(
            incalcariCuPretInghetat > 0,
            $"prețul înghețat n-a încălcat proprietatea în niciun caz ({goliri} goliri) — contra-proba N-D7 nu mai spune nimic");
    }

    [Fact]
    public void UltimaIesireIaRestul() {
        var rotunjire = new Rotunjire(MidpointRounding.AwayFromZero);
        var sold = new Sold(10.00m, 0m, 3.000m, 0m);
        var iesiri = new List<decimal>();
        for (var i = 0; i < 3; i++) {
            var valoare = Evaluare.Iesire(sold, 1.000m, rotunjire);
            iesiri.Add(valoare);
            sold += new Sold(0m, valoare, -1.000m, 0m);
        }
        Assert.Equal(new[] { 3.33m, 3.34m, 3.33m }, iesiri);
        Assert.Equal(0m, sold.Net);
    }

    [Fact]
    public void GolireaIaNetulFaraRotunjire() {
        var rotunjire = new Rotunjire(MidpointRounding.ToEven);
        var sold = new Sold(7.77m, 0.04m, 3.000m, 0m);
        Assert.Equal(7.73m, Evaluare.Iesire(sold, 3.000m, rotunjire));
        Assert.Equal(0, rotunjire.JumatatiDeBan);
    }

    // La scara 2 ramura de golire e redundantă aritmetic ((q × Net) / q = Net exact);
    // ce apără ea e netul NErotunjit, iar `Sold` nu are gardian de scară — N-D7.
    [Fact]
    public void SoldulInAfaraScariiERefuzat() {
        Assert.Throws<ArgumentException>(() => new Sold(10.005m, 0m, 2.000m, 0m));
        Assert.Throws<ArgumentException>(() => new Sold(10.00m, 0m, 2.0005m, 0m));
        Assert.Throws<ArgumentException>(() => Sold.Zero with { Credit = 0.001m });
    }

    [Fact]
    public void CantitateaPesteSoldERefuzDeDomeniu() {
        var eroare = Assert.Throws<RefuzException>(() =>
            Evaluare.Iesire(new Sold(10m, 0m, 2.000m, 0m), 2.001m, new Rotunjire(MidpointRounding.AwayFromZero)));
        Assert.Equal(Coduri.StocInsuficient, eroare.Refuz.Cod);
    }

    [Fact]
    public void SoldulGolERefuzDeDomeniu() {
        var eroare = Assert.Throws<RefuzException>(() =>
            Evaluare.Iesire(Sold.Zero, 1.000m, new Rotunjire(MidpointRounding.AwayFromZero)));
        Assert.Equal(Coduri.StocInsuficient, eroare.Refuz.Cod);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void CantitateaNepozitivaEEroareDeApelant(int cantitate) =>
        Assert.Throws<ArgumentException>(() =>
            Evaluare.Iesire(new Sold(10m, 0m, 5.000m, 0m), cantitate, new Rotunjire(MidpointRounding.AwayFromZero)));

    // Regula VECHE (`StocService.cs:41`), păstrată doar ca oracol al contra-probei — N-D7.
    static decimal IesirePretInghetat(decimal cantitate, decimal pretUnitar, Rotunjire rotunjire) =>
        rotunjire.Bani(cantitate * pretUnitar);

    static List<(bool Intrare, bool Goleste, decimal Cantitate, decimal Pret)> Istorie(Random aleator) {
        var pasi = new List<(bool, bool, decimal, decimal)>();
        var cate = 4 + aleator.Next(10);
        for (var i = 0; i < cate; i++) {
            var intrare = i == 0 || aleator.Next(2) == 0;
            pasi.Add((
                intrare,
                !intrare && aleator.Next(3) == 0,
                Gen.Zecimal(aleator, 0.001m, 99.999m, Scara.Cantitate),
                Gen.Zecimal(aleator, 0.000001m, 99.999999m, Scara.Pret)));
        }
        return pasi;
    }
}
