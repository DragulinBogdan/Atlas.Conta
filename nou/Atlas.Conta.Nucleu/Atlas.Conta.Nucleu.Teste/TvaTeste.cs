using System.Globalization;
using Xunit;

namespace Atlas.Conta.Nucleu.Teste;

public class TvaTeste {
    static readonly decimal[] Cote = [0m, 5m, 9m, 11m, 19m, 21m];

    static readonly RegimTva[] Regimuri = [
        RegimTva.Normal,
        RegimTva.Capitalizat,
        RegimTva.TaxareInversa,
        RegimTva.Scutit,
        RegimTva.Neimpozabil,
    ];

    [Theory]
    [InlineData(RegimTva.Capitalizat, DirectieTva.Deductibil, "100", "19", "119", "0")]
    [InlineData(RegimTva.Capitalizat, DirectieTva.Colectat, "100", "19", "119", "0")]
    [InlineData(RegimTva.TaxareInversa, DirectieTva.Colectat, "100", "19", "100", "0")]
    [InlineData(RegimTva.TaxareInversa, DirectieTva.Deductibil, "100", "19", "100", "19")]
    [InlineData(RegimTva.Normal, DirectieTva.Deductibil, "100", "19", "100", "19")]
    [InlineData(RegimTva.Normal, DirectieTva.Colectat, "100", "19", "100", "19")]
    [InlineData(RegimTva.Normal, DirectieTva.Deductibil, "0.01", "19", "0.01", "0.0019")]
    [InlineData(RegimTva.Scutit, DirectieTva.Deductibil, "100", "19", "100", "0")]
    [InlineData(RegimTva.Scutit, DirectieTva.Colectat, "100", "0", "100", "0")]
    [InlineData(RegimTva.Neimpozabil, DirectieTva.Colectat, "100", "19", "100", "0")]
    [InlineData(RegimTva.Normal, DirectieTva.Colectat, "-250.50", "21", "-250.50", "-52.605")]
    public void RamurileLinieiSuntCeleDeAzi(
        RegimTva regim,
        DirectieTva directie,
        string net,
        string cota,
        string valoareAsteptata,
        string taxaAsteptata) {
        var (valoare, taxa) = Tva.Linie(Numar(net), regim, Numar(cota), directie);
        Assert.Equal(Numar(valoareAsteptata), valoare);
        Assert.Equal(Numar(taxaAsteptata), taxa);
    }

    [Fact]
    public void TaxaPeCotaEStransaDinLiniiNerotunjite() =>
        Proprietate.Verifica(Proprietate.Cazuri, (aleator, _) => {
            var directie = aleator.Next(2) == 0 ? DirectieTva.Deductibil : DirectieTva.Colectat;
            var conventie = aleator.Next(2) == 0 ? MidpointRounding.AwayFromZero : MidpointRounding.ToEven;
            var linii = Linii(aleator);
            var taxe = Tva.PeDocument(linii, directie, new Rotunjire(conventie));
            foreach (var grup in linii.GroupBy(linie => (linie.Regim, linie.Cota))) {
                var nerotunjita = grup.Sum(linie => Tva.Linie(linie.Net, linie.Regim, linie.Cota, directie).Taxa);
                Assert.Equal(new Rotunjire(conventie).Bani(nerotunjita), taxe.PerCota[grup.Key]);
            }
            Assert.Equal(
                linii.Select(linie => (linie.Regim, linie.Cota)).Distinct().Count(),
                taxe.PerCota.Count);
        });

    [Fact]
    public void SumaPeLiniiEExactTaxaPeCota() =>
        Proprietate.Verifica(Proprietate.Cazuri, (aleator, _) => {
            var directie = aleator.Next(2) == 0 ? DirectieTva.Deductibil : DirectieTva.Colectat;
            var linii = Linii(aleator);
            var taxe = Tva.PeDocument(linii, directie, new Rotunjire(MidpointRounding.AwayFromZero));
            Assert.Equal(linii.Count, taxe.PerLinie.Count);
            foreach (var linie in linii)
                Assert.True(taxe.PerLinie.ContainsKey(linie.Linie), $"linia {linie.Linie} lipsește");
            foreach (var grup in linii.GroupBy(linie => (linie.Regim, linie.Cota)))
                Assert.Equal(taxe.PerCota[grup.Key], grup.Sum(linie => taxe.PerLinie[linie.Linie]));
            foreach (var cota in taxe.PerLinie.Values)
                Assert.True(Scara.EsteLa(cota, Scara.Bani), $"cota {cota} nu e la scara banilor");
        });

    [Fact]
    public void RegimurileFaraTaxaDauZeroPeLinie() =>
        Proprietate.Verifica(Proprietate.Cazuri, (aleator, _) => {
            var directie = aleator.Next(2) == 0 ? DirectieTva.Deductibil : DirectieTva.Colectat;
            var linii = Linii(aleator);
            var taxe = Tva.PeDocument(linii, directie, new Rotunjire(MidpointRounding.AwayFromZero));
            foreach (var linie in linii)
                if (Tva.Linie(1m, linie.Regim, linie.Cota, directie).Taxa == 0m)
                    Assert.Equal(0m, taxe.PerLinie[linie.Linie]);
        });

    // Diferența DECLARATĂ față de `TvaService.cs:79-80` (rotunjire per linie), 090j.
    [Fact]
    public void TaxaPeDocumentDiferaDeRotunjireaPeLinie() {
        var linii = new List<LinieTva> {
            new(Id(0), 0.01m, RegimTva.Normal, 19m),
            new(Id(1), 0.01m, RegimTva.Normal, 19m),
            new(Id(2), 0.01m, RegimTva.Normal, 19m),
        };
        var rotunjire = new Rotunjire(MidpointRounding.AwayFromZero);
        var taxe = Tva.PeDocument(linii, DirectieTva.Deductibil, rotunjire);
        Assert.Equal(0.01m, taxe.PerCota[(RegimTva.Normal, 19m)]);
        Assert.Equal(new[] { 0.01m, 0m, 0m }, linii.Select(linie => taxe.PerLinie[linie.Linie]));
        foreach (var linie in linii)
            Assert.Equal(
                0m,
                new Rotunjire(MidpointRounding.AwayFromZero)
                    .Bani(Tva.Linie(linie.Net, linie.Regim, linie.Cota, DirectieTva.Deductibil).Taxa));
    }

    // Grup mixt: latura de semn se repartizează separat, altfel linia își pierde semnul (F1).
    [Theory]
    [InlineData("100", "-10", "19.00", "-1.90", "17.10")]
    [InlineData("100", "-100", "19.00", "-19.00", "0")]
    public void GrupulCuSemneMixteTinePeFiecareLinieSemnulEi(
        string unu,
        string doi,
        string asteptatUnu,
        string asteptatDoi,
        string peCota) {
        var linii = new List<LinieTva> {
            new(Id(0), Numar(unu), RegimTva.Normal, 19m),
            new(Id(1), Numar(doi), RegimTva.Normal, 19m),
        };
        var taxe = Tva.PeDocument(linii, DirectieTva.Deductibil, new Rotunjire(MidpointRounding.AwayFromZero));
        Assert.Equal(Numar(peCota), taxe.PerCota[(RegimTva.Normal, 19m)]);
        Assert.Equal(Numar(asteptatUnu), taxe.PerLinie[Id(0)]);
        Assert.Equal(Numar(asteptatDoi), taxe.PerLinie[Id(1)]);
    }

    [Fact]
    public void SemnulPeLinieEAlTaxeiEi() =>
        Proprietate.Verifica(Proprietate.Cazuri, (aleator, _) => {
            var directie = aleator.Next(2) == 0 ? DirectieTva.Deductibil : DirectieTva.Colectat;
            var linii = Linii(aleator);
            var taxe = Tva.PeDocument(linii, directie, new Rotunjire(MidpointRounding.AwayFromZero));
            foreach (var linie in linii) {
                var semn = Math.Sign(Tva.Linie(linie.Net, linie.Regim, linie.Cota, directie).Taxa);
                var cota = taxe.PerLinie[linie.Linie];
                Assert.True(
                    Math.Sign(cota) == 0 || Math.Sign(cota) == semn,
                    $"linia {linie.Linie} a primit {cota} pentru o taxă de semn {semn}");
            }
        });

    [Fact]
    public void GrupulCuBazeZeroNuCereRepartizare() {
        var linii = new List<LinieTva> {
            new(Id(0), 0m, RegimTva.Normal, 19m),
            new(Id(1), 0m, RegimTva.Normal, 19m),
        };
        var taxe = Tva.PeDocument(linii, DirectieTva.Deductibil, new Rotunjire(MidpointRounding.AwayFromZero));
        Assert.Equal(0m, taxe.PerCota[(RegimTva.Normal, 19m)]);
        Assert.Equal(new[] { 0m, 0m }, linii.Select(linie => taxe.PerLinie[linie.Linie]));
    }

    [Fact]
    public void DocumentulFaraLiniiDaTabeleGoale() {
        var taxe = Tva.PeDocument([], DirectieTva.Colectat, new Rotunjire(MidpointRounding.AwayFromZero));
        Assert.Empty(taxe.PerCota);
        Assert.Empty(taxe.PerLinie);
    }

    [Fact]
    public void TaxaDataInToleranteTrece() {
        Assert.Null(Tva.ValideazaData(19.00m, 19.00m, 0m));
        Assert.Null(Tva.ValideazaData(19.02m, 19.00m, 0.02m));
        Assert.Null(Tva.ValideazaData(18.98m, 19.00m, 0.02m));
    }

    [Fact]
    public void TaxaDataPesteToleranteERefuzata() {
        var refuz = Tva.ValideazaData(19.03m, 19.00m, 0.02m);
        Assert.NotNull(refuz);
        Assert.Equal(Coduri.TvaInAfaraTolerantei, refuz.Cod);
        Assert.Null(refuz.Linie);
        Assert.NotNull(Tva.ValideazaData(18.97m, 19.00m, 0.02m));
        Assert.NotNull(Tva.ValideazaData(19.01m, 19.00m, 0m));
    }

    [Fact]
    public void TolerantaNegativaERefuzata() =>
        Assert.Throws<ArgumentException>(() => Tva.ValideazaData(19m, 19m, -0.01m));

    [Fact]
    public void TolerantaEFrontieraInchisa() =>
        Proprietate.Verifica(Proprietate.Cazuri, (aleator, _) => {
            var calculata = Gen.Zecimal(aleator, -9999.99m, 9999.99m, Scara.Bani);
            var toleranta = Gen.Zecimal(aleator, 0m, 0.50m, Scara.Bani);
            var semn = aleator.Next(2) == 0 ? 1m : -1m;
            Assert.Null(Tva.ValideazaData(calculata + semn * toleranta, calculata, toleranta));
            Assert.NotNull(Tva.ValideazaData(calculata + semn * (toleranta + 0.01m), calculata, toleranta));
        });

    static List<LinieTva> Linii(Random aleator) {
        var linii = new List<LinieTva>();
        var cate = 1 + aleator.Next(8);
        for (var i = 0; i < cate; i++)
            linii.Add(new LinieTva(
                Id(i),
                aleator.Next(6) == 0 ? 0m : Gen.Zecimal(aleator, -9999.99m, 9999.99m, Scara.Bani),
                Regimuri[aleator.Next(Regimuri.Length)],
                Cote[aleator.Next(Cote.Length)]));
        return linii;
    }

    static Guid Id(int indice) => new(9, (short)indice, 0, 0, 0, 0, 0, 0, 0, 0, 0);

    static decimal Numar(string text) => decimal.Parse(text, CultureInfo.InvariantCulture);
}
