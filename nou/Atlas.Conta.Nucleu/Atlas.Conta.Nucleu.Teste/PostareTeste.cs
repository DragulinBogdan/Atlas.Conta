using System.Globalization;
using Xunit;

namespace Atlas.Conta.Nucleu.Teste;

public class PostareTeste {
    [Theory]
    [InlineData("0.0001", "0", "0", "Cantitate")]
    [InlineData("-1.00005", "0", "0", "Cantitate")]
    [InlineData("0", "0.001", "0", "ValoareValuta")]
    [InlineData("0", "0", "0.005", "Valoare")]
    [InlineData("0", "0", "-12.345", "Valoare")]
    public void MasuraInAfaraScariiERefuzata(string cantitate, string valoareValuta, string valoare, string masura) {
        var eroare = Assert.Throws<ArgumentException>(() => new Postare(
            Coordonata(),
            Numar(cantitate),
            Numar(valoareValuta),
            Numar(valoare),
            new Cauza(Guid.NewGuid(), null)));
        Assert.Equal(masura, eroare.ParamName);
    }

    [Theory]
    [InlineData("1.234", "0.01", "-9.99")]
    [InlineData("0", "0", "0")]
    [InlineData("-999.999", "12.30", "0.01")]
    public void MasuraLaScaraEPastrata(string cantitate, string valoareValuta, string valoare) {
        var postare = new Postare(
            Coordonata(),
            Numar(cantitate),
            Numar(valoareValuta),
            Numar(valoare),
            new Cauza(Guid.NewGuid(), null));
        Assert.Equal(Numar(cantitate), postare.Cantitate);
        Assert.Equal(Numar(valoareValuta), postare.ValoareValuta);
        Assert.Equal(Numar(valoare), postare.Valoare);
    }

    [Fact]
    public void CopiaPrinWithApaparaScara() =>
        Assert.Throws<ArgumentException>(() => new Postare(
            Coordonata(),
            0m,
            0m,
            0m,
            new Cauza(Guid.NewGuid(), null)) with { Valoare = 0.001m });

    [Fact]
    public void SpatiulUrmeazaLotul() =>
        Proprietate.Verifica(Proprietate.Cazuri, (aleator, _) => {
            foreach (var postare in Gen.Operare(aleator).Postari)
                Assert.Equal(
                    postare.Coordonate.Unitate?.Fel == FelUnitate.Lot ? Spatiu.Stoc : Spatiu.Contabil,
                    postare.Spatiu());
        });

    static decimal Numar(string text) => decimal.Parse(text, CultureInfo.InvariantCulture);

    static Coordonate Coordonata() => new() {
        Cont = Guid.NewGuid(),
        Latura = Latura.Debit,
        Data = new DateOnly(2026, 1, 1),
    };
}
