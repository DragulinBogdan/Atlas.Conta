using Xunit;

namespace Atlas.Conta.Nucleu.Teste;

public class RotunjireTeste {
    [Fact]
    public void BaniiRespectaConventia() =>
        Proprietate.Verifica(Proprietate.Cazuri, (aleator, _) => {
            var conventie = aleator.Next(2) == 0 ? MidpointRounding.AwayFromZero : MidpointRounding.ToEven;
            var valoare = Gen.Zecimal(aleator, -9999.999999m, 9999.999999m, Scara.Pret);
            Assert.Equal(Math.Round(valoare, Scara.Bani, conventie), new Rotunjire(conventie).Bani(valoare));
        });

    [Fact]
    public void CantitateaRespectaConventia() =>
        Proprietate.Verifica(Proprietate.Cazuri, (aleator, _) => {
            var conventie = aleator.Next(2) == 0 ? MidpointRounding.AwayFromZero : MidpointRounding.ToEven;
            var valoare = Gen.Zecimal(aleator, -9999.999999m, 9999.999999m, Scara.Pret);
            Assert.Equal(
                Math.Round(valoare, Scara.Cantitate, conventie),
                new Rotunjire(conventie).Cantitate(valoare));
        });

    [Fact]
    public void ContorulNumaraExactJumatatileDeBan() {
        var rotunjire = new Rotunjire(MidpointRounding.ToEven);
        var aleator = new Random(Gen.Samanta);
        var jumatati = 0;
        for (var i = 0; i < Proprietate.Cazuri; i++) {
            var bani = Gen.Zecimal(aleator, -9999.99m, 9999.99m, Scara.Bani);
            if (aleator.Next(2) == 0) {
                rotunjire.Bani(bani + (bani < 0m ? -0.005m : 0.005m));
                jumatati++;
            } else {
                rotunjire.Bani(bani);
            }
            Assert.Equal(jumatati, rotunjire.JumatatiDeBan);
        }
        Assert.True(jumatati > 0);
    }

    [Fact]
    public void ContorulEAlInstantei() {
        var una = new Rotunjire(MidpointRounding.AwayFromZero);
        var alta = new Rotunjire(MidpointRounding.AwayFromZero);
        una.Bani(1.005m);
        Assert.Equal(1, una.JumatatiDeBan);
        Assert.Equal(0, alta.JumatatiDeBan);
    }

    [Fact]
    public void PretulNuUrmeazaConventia() {
        var rotunjire = new Rotunjire(MidpointRounding.ToEven);
        Assert.Equal(0m, Math.Round(0.0000005m, Scara.Pret, MidpointRounding.ToEven));
        Assert.Equal(0.000001m, rotunjire.Pret(0.0000005m));
        Assert.Equal(-0.000001m, rotunjire.Pret(-0.0000005m));
        Assert.Equal(0, rotunjire.JumatatiDeBan);
    }

    [Fact]
    public void RotunjireaDaValoriLaScara() =>
        Proprietate.Verifica(Proprietate.Cazuri, (aleator, _) => {
            var rotunjire = new Rotunjire(MidpointRounding.AwayFromZero);
            var valoare = Gen.Zecimal(aleator, -9999.999999m, 9999.999999m, Scara.Pret);
            Assert.True(Scara.EsteLa(rotunjire.Bani(valoare), Scara.Bani));
            Assert.True(Scara.EsteLa(rotunjire.Cantitate(valoare), Scara.Cantitate));
            Assert.True(Scara.EsteLa(rotunjire.Pret(valoare), Scara.Pret));
        });
}
