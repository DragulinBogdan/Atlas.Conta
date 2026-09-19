using Xunit;

namespace Atlas.Conta.Nucleu.Teste;

public class UnitateTeste {
    [Fact]
    public void PartidaDeschisaEDeterminista() =>
        Proprietate.Verifica(Proprietate.Cazuri, (aleator, _) => {
            var cont = Gen.Unul(aleator, Gen.Conturi);
            var partener = Gen.Unul(aleator, Gen.Parteneri);
            var document = Gen.Unul(aleator, Gen.Documente);
            var data = Gen.Data(aleator);
            var unitate = Unitate.DeschidePartida(cont, partener, document, data);
            Assert.Equal(unitate.Id, Unitate.DeschidePartida(cont, partener, document, data).Id);
            // Data și partenerul nu intră în identitate: doar (document, cont) — N-D6.
            Assert.Equal(
                unitate.Id,
                Unitate.DeschidePartida(cont, Gen.AltulDecat(aleator, Gen.Parteneri, partener), document, data.AddDays(7)).Id);
            Assert.NotEqual(
                unitate.Id,
                Unitate.DeschidePartida(Gen.AltulDecat(aleator, Gen.Conturi, cont), partener, document, data).Id);
            Assert.NotEqual(
                unitate.Id,
                Unitate.DeschidePartida(cont, partener, Gen.AltulDecat(aleator, Gen.Documente, document), data).Id);
            Assert.Equal(FelUnitate.Partida, unitate.Fel);
            Assert.Equal(cont, unitate.Cont);
            Assert.Equal(partener, unitate.Partener);
            Assert.Equal(data, unitate.Deschisa);
        });

    [Fact]
    public void IdentitateaPartideiEPinuitaPeOcteti() =>
        Assert.Equal(
            Guid.Parse("6a865294-d354-8a72-4249-185390c213bb"),
            Unitate.DeschidePartida(
                Guid.Parse("00000001-0000-0000-0000-000000000000"),
                Guid.Parse("00000002-0000-0000-0000-000000000000"),
                Guid.Parse("00000005-0000-0000-0000-000000000000"),
                new DateOnly(2026, 1, 1)).Id);

    [Fact]
    public void LotulFaraProdusERefuzat() {
        var eroare = Assert.Throws<ArgumentException>(() =>
            new Unitate(Guid.NewGuid(), FelUnitate.Lot, Guid.NewGuid(), null, null, new DateOnly(2026, 1, 1)));
        Assert.Equal("Produs", eroare.ParamName);
    }

    [Fact]
    public void PartidaFaraPartenerERefuzata() {
        var eroare = Assert.Throws<ArgumentException>(() =>
            new Unitate(Guid.NewGuid(), FelUnitate.Partida, Guid.NewGuid(), null, null, new DateOnly(2026, 1, 1)));
        Assert.Equal("Partener", eroare.ParamName);
    }

    [Fact]
    public void FisaNuCereNimic() {
        var fisa = new Unitate(Guid.NewGuid(), FelUnitate.Fisa, Guid.NewGuid(), null, null, new DateOnly(2026, 1, 1));
        Assert.Equal(FelUnitate.Fisa, fisa.Fel);
    }

    [Fact]
    public void CopiaPrinWithApaparaCerinta() {
        var lot = new Unitate(Guid.NewGuid(), FelUnitate.Lot, Guid.NewGuid(), null, Guid.NewGuid(), new DateOnly(2026, 1, 1));
        Assert.Throws<ArgumentException>(() => lot with { Produs = null });
        var partida = new Unitate(Guid.NewGuid(), FelUnitate.Partida, Guid.NewGuid(), Guid.NewGuid(), null, new DateOnly(2026, 1, 1));
        Assert.Throws<ArgumentException>(() => partida with { Partener = null });
    }

    [Fact]
    public void RaportulLotuluiECostul() {
        var lot = new Unitate(Guid.NewGuid(), FelUnitate.Lot, Guid.NewGuid(), null, Guid.NewGuid(), new DateOnly(2026, 1, 1));
        Assert.Equal(5m, lot.Raport(new Sold(12.50m, 2.50m, 2m, 0m)));
        Assert.Null(lot.Raport(new Sold(10m, 0m, 0m, 0m)));
    }

    [Fact]
    public void RaportulPartideiECursul() {
        var partida = new Unitate(Guid.NewGuid(), FelUnitate.Partida, Guid.NewGuid(), Guid.NewGuid(), null, new DateOnly(2026, 1, 1));
        Assert.Equal(4.97m, partida.Raport(new Sold(497m, 0m, 0m, 100m)));
        Assert.Null(partida.Raport(new Sold(497m, 0m, 0m, 0m)));
    }

    [Fact]
    public void RaportulFiseiERestul() {
        var fisa = new Unitate(Guid.NewGuid(), FelUnitate.Fisa, Guid.NewGuid(), null, null, new DateOnly(2026, 1, 1));
        Assert.Equal(-3m, fisa.Raport(new Sold(7m, 10m, 0m, 0m)));
        Assert.Equal(0m, fisa.Raport(Sold.Zero));
    }
}
