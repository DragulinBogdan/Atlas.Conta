using Xunit;

namespace Atlas.Conta.Nucleu.Teste;

public class FifoTeste {
    [Fact]
    public void LoturileRespectaCerereaPinurileSiOrdinea() => Nominalizari(FelUnitate.Lot, Scara.Cantitate);

    [Fact]
    public void PartideleRespectaAceeasiPrimitiva() => Nominalizari(FelUnitate.Partida, Scara.Bani);

    [Fact]
    public void PinulPesteDisponibilIaDoarDisponibilul() {
        var unul = Unitatea(1, new DateOnly(2026, 1, 1));
        var altul = Unitatea(2, new DateOnly(2026, 1, 2));
        var nominalizare = Fifo.Nominalizeaza(
            10.000m,
            [new Disponibil(unul, 2.000m), new Disponibil(altul, 5.000m)],
            [new Pin(altul.Id, 99.000m)]);
        Assert.Equal(
            new[] { (altul.Id, 5.000m), (unul.Id, 2.000m) },
            nominalizare.Alocari.Select(a => (a.Unitate.Id, a.Masura)).ToList());
        Assert.Equal(3.000m, nominalizare.Ramas);
    }

    [Fact]
    public void PinulNuCadePeFifoCandNuAjunge() {
        var unul = Unitatea(1, new DateOnly(2026, 1, 1));
        var altul = Unitatea(2, new DateOnly(2026, 1, 2));
        var nominalizare = Fifo.Nominalizeaza(
            3.000m,
            [new Disponibil(unul, 9.000m), new Disponibil(altul, 1.000m)],
            [new Pin(altul.Id, 1.000m)]);
        // Pin-ul ia 1, restul cererii merge pe FIFO, nu se oprește la pin.
        Assert.Equal(
            new[] { (altul.Id, 1.000m), (unul.Id, 2.000m) },
            nominalizare.Alocari.Select(a => (a.Unitate.Id, a.Masura)).ToList());
        Assert.Equal(0m, nominalizare.Ramas);
    }

    [Fact]
    public void RestulUnitatiiPinuiteRamaneLaFifo() {
        var unul = Unitatea(1, new DateOnly(2026, 1, 1));
        var altul = Unitatea(2, new DateOnly(2026, 1, 2));
        var nominalizare = Fifo.Nominalizeaza(
            8.000m,
            [new Disponibil(unul, 1.000m), new Disponibil(altul, 9.000m)],
            [new Pin(altul.Id, 2.000m)]);
        Assert.Equal(
            new[] { (altul.Id, 2.000m), (unul.Id, 1.000m), (altul.Id, 5.000m) },
            nominalizare.Alocari.Select(a => (a.Unitate.Id, a.Masura)).ToList());
        Assert.Equal(0m, nominalizare.Ramas);
    }

    [Fact]
    public void OrdineaEDupaDeschisaApoiId() {
        var tarziu = Unitatea(1, new DateOnly(2026, 3, 1));
        var devreme = Unitatea(9, new DateOnly(2026, 1, 1));
        var deopotriva = Unitatea(2, new DateOnly(2026, 3, 1));
        var nominalizare = Fifo.Nominalizeaza(
            30.000m,
            [new Disponibil(tarziu, 10.000m), new Disponibil(devreme, 10.000m), new Disponibil(deopotriva, 10.000m)]);
        Assert.Equal(
            new[] { devreme.Id, tarziu.Id, deopotriva.Id },
            nominalizare.Alocari.Select(a => a.Unitate.Id).ToList());
    }

    [Fact]
    public void CandidatulGolNuPrimesteNimic() {
        var unul = Unitatea(1, new DateOnly(2026, 1, 1));
        var altul = Unitatea(2, new DateOnly(2026, 1, 2));
        var nominalizare = Fifo.Nominalizeaza(
            1.000m,
            [new Disponibil(unul, 0m), new Disponibil(altul, 5.000m)],
            [new Pin(unul.Id, 1.000m)]);
        Assert.Equal(new[] { (altul.Id, 1.000m) }, nominalizare.Alocari.Select(a => (a.Unitate.Id, a.Masura)).ToList());
    }

    [Fact]
    public void PinulSpreOUnitateNecunoscutaERefuzat() {
        var unul = Unitatea(1, new DateOnly(2026, 1, 1));
        Assert.Throws<ArgumentException>(() => Fifo.Nominalizeaza(
            1.000m,
            [new Disponibil(unul, 5.000m)],
            [new Pin(Unitatea(7, new DateOnly(2026, 1, 1)).Id, 1.000m)]));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-2)]
    public void PinulCuMasuraNepozitivaERefuzat(int masura) {
        var unul = Unitatea(1, new DateOnly(2026, 1, 1));
        Assert.Throws<ArgumentException>(() => Fifo.Nominalizeaza(
            1.000m,
            [new Disponibil(unul, 5.000m)],
            [new Pin(unul.Id, masura)]));
    }

    [Fact]
    public void CandidatulDublatERefuzat() {
        var unul = Unitatea(1, new DateOnly(2026, 1, 1));
        Assert.Throws<ArgumentException>(() => Fifo.Nominalizeaza(
            1.000m,
            [new Disponibil(unul, 5.000m), new Disponibil(unul with { Deschisa = new DateOnly(2026, 2, 1) }, 3.000m)]));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-3)]
    public void CerereaNepozitivaERefuzata(int cerere) =>
        Assert.Throws<ArgumentException>(() => Fifo.Nominalizeaza(
            cerere,
            [new Disponibil(Unitatea(1, new DateOnly(2026, 1, 1)), 5.000m)]));

    [Fact]
    public void FaraCandidatiCerereaRamaneIntreaga() =>
        Assert.Equal(4.000m, Fifo.Nominalizeaza(4.000m, []).Ramas);

    static void Nominalizari(FelUnitate fel, int scara) =>
        Proprietate.Verifica(Proprietate.Cazuri, (aleator, _) => {
            var candidati = Candidati(aleator, fel, scara);
            var pinuri = Pinuri(aleator, candidati, scara);
            var cerere = Gen.Zecimal(aleator, Gen.Pas(scara), 2000m, scara);
            var nominalizare = Fifo.Nominalizeaza(cerere, candidati, pinuri);
            var alocari = nominalizare.Alocari;

            Assert.True(nominalizare.Ramas >= 0m, $"ramas {nominalizare.Ramas} e negativ");
            Assert.Equal(cerere, alocari.Sum(a => a.Masura) + nominalizare.Ramas);
            Assert.All(alocari, alocare => Assert.True(alocare.Masura > 0m, "alocare de măsură nulă"));

            var disponibil = candidati.ToDictionary(
                candidat => candidat.Unitate.Id,
                candidat => Math.Max(candidat.Masura, 0m));
            foreach (var grup in alocari.GroupBy(alocare => alocare.Unitate.Id))
                Assert.True(
                    grup.Sum(alocare => alocare.Masura) <= disponibil[grup.Key],
                    $"unitatea {grup.Key} a dat {grup.Sum(a => a.Masura)} din {disponibil[grup.Key]}");

            var peNume = PePinuri(cerere, candidati, pinuri);
            Assert.Equal(
                peNume,
                alocari.Take(peNume.Count).Select(alocare => (alocare.Unitate.Id, alocare.Masura)).ToList());

            var ramasDupaPinuri = new Dictionary<Guid, decimal>(disponibil);
            foreach (var (id, masura) in peNume)
                ramasDupaPinuri[id] -= masura;
            var peFifo = alocari.Skip(peNume.Count).ToList();
            for (var i = 1; i < peFifo.Count; i++)
                Assert.True(
                    Fifo.Intai(peFifo[i - 1].Unitate, peFifo[i].Unitate) < 0,
                    $"{peFifo[i - 1].Unitate.Id} ar trebui înaintea lui {peFifo[i].Unitate.Id}");
            for (var i = 0; i < peFifo.Count - 1; i++)
                Assert.Equal(ramasDupaPinuri[peFifo[i].Unitate.Id], peFifo[i].Masura);
            if (nominalizare.Ramas > 0m)
                foreach (var candidat in candidati)
                    Assert.Equal(
                        disponibil[candidat.Unitate.Id],
                        alocari.Where(a => a.Unitate.Id == candidat.Unitate.Id).Sum(a => a.Masura));
        });

    static List<(Guid Id, decimal Masura)> PePinuri(
        decimal cerere,
        List<Disponibil> candidati,
        List<Pin> pinuri) {
        var liber = candidati.ToDictionary(
            candidat => candidat.Unitate.Id,
            candidat => Math.Max(candidat.Masura, 0m));
        var luate = new List<(Guid, decimal)>();
        var ramas = cerere;
        foreach (var pin in pinuri) {
            var masura = Math.Min(Math.Min(pin.Masura, liber[pin.Unitate]), ramas);
            if (masura <= 0m)
                continue;
            luate.Add((pin.Unitate, masura));
            liber[pin.Unitate] -= masura;
            ramas -= masura;
        }
        return luate;
    }

    static List<Disponibil> Candidati(Random aleator, FelUnitate fel, int scara) {
        var cate = aleator.Next(7);
        var ids = Gen.Unitati.OrderBy(_ => aleator.Next()).Take(cate).ToList();
        return ids
            .Select(id => new Disponibil(
                Unitatea(id, fel, aleator),
                aleator.Next(6) == 0 ? 0m : Gen.Zecimal(aleator, Gen.Pas(scara), 999m, scara)))
            .ToList();
    }

    static List<Pin> Pinuri(Random aleator, List<Disponibil> candidati, int scara) {
        var pinuri = new List<Pin>();
        if (candidati.Count == 0)
            return pinuri;
        var cate = aleator.Next(4);
        for (var i = 0; i < cate; i++) {
            var candidat = candidati[aleator.Next(candidati.Count)];
            pinuri.Add(new Pin(candidat.Unitate.Id, Gen.Zecimal(aleator, Gen.Pas(scara), 1500m, scara)));
        }
        return pinuri;
    }

    static Unitate Unitatea(Guid id, FelUnitate fel, Random aleator) => new(
        id,
        fel,
        Gen.Unul(aleator, Gen.Conturi),
        fel == FelUnitate.Partida ? Gen.Unul(aleator, Gen.Parteneri) : null,
        fel == FelUnitate.Lot ? Gen.Unul(aleator, Gen.Produse) : null,
        Gen.Data(aleator));

    static Unitate Unitatea(int marca, DateOnly deschisa) => new(
        new Guid(marca, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0),
        FelUnitate.Lot,
        Gen.Conturi[0],
        null,
        Gen.Produse[0],
        deschisa);
}
