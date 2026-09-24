using Xunit;

namespace Atlas.Conta.Nucleu.Teste;

public class TransformareTeste {
    static readonly DateOnly Data = new(2026, 1, 10);

    static Transformare Linie(Guid document, Guid cont, Guid produs, RolTransformare rol,
            FelTranzactie fel, decimal q, decimal v) => new(new Capat {
        Cont = cont, Produs = produs, Gestiune = Gen.Gestiuni[0],
        Unitate = new Unitate(produs, FelUnitate.Lot, cont, null, produs, Data),
    }, rol, fel, q, v, new Cauza(document, produs));

    static Declaratie Scena(Random rng) {
        var doc = Gen.Documente[0];
        var linii = new List<Transformare>();
        foreach (var fel in new[] { FelTranzactie.Transfer, FelTranzactie.Operare }) {
            var n = rng.Next(1, 8);
            var m = rng.Next(1, 8);
            var pret = rng.Next(1, 10000) / 100m;
            var cont = Gen.Conturi[0];
            var tinta = fel == FelTranzactie.Transfer ? cont : Gen.Conturi[1];
            for (var i = 0; i < n + m; i++) {
                var produs = new Guid(rng.Next(), (short)rng.Next(short.MaxValue), 1, new byte[8]);
                linii.Add(Linie(doc, i < n ? cont : tinta, produs,
                    i < n ? RolTransformare.Consum : RolTransformare.Produs, fel,
                    rng.Next(1, 100000) / 1000m, pret * (i < n ? m : n)));
            }
        }
        return new(doc, Data, [], [], linii, [], []);
    }

    [Fact]
    public void TransformareaNLaMConservaValoareaSiCantitatea() =>
        Proprietate.Verifica(Proprietate.Cazuri, (rng, _) => {
            var d = Scena(rng);
            var c = Motor.Opereaza(d, new(MidpointRounding.ToEven));
            Assert.True(c.EsteAcceptat);
            Assert.Equal(2, c.Tranzactii.Count);
            foreach (var t in c.Tranzactii) {
                Assert.Empty(Conservare.Verifica(t));
                var inverse = Storno.Inverseaza(t.Postari, d.Document, Data.AddMonths(1), 202602);
                Assert.Empty(Conservare.Verifica(inverse));
                Assert.Equal(0m, t.Postari.Concat(inverse.Postari).Sum(p => p.Valoare));
                Assert.Equal(0m, t.Postari.Concat(inverse.Postari).Sum(p => p.Cantitate));
            }
            foreach (var p in c.Tranzactii.SelectMany(t => t.Postari)
                         .Where(p => p.Coordonate.Gestiune == GestiuniVirtuale.Transformare)) {
                Assert.Null(p.Coordonate.Unitate);
                Assert.Equal(0m, p.Valoare);
                Assert.Equal(0m, p.ValoareValuta);
            }
        });

    [Theory]
    [InlineData(FelTranzactie.Transfer, 0)]
    [InlineData(FelTranzactie.Transfer, 1)]
    [InlineData(FelTranzactie.Transfer, 2)]
    [InlineData(FelTranzactie.Operare, 0)]
    [InlineData(FelTranzactie.Operare, 1)]
    [InlineData(FelTranzactie.Operare, 2)]
    [InlineData(FelTranzactie.Storno, 0)]
    [InlineData(FelTranzactie.Storno, 1)]
    [InlineData(FelTranzactie.Storno, 2)]
    public void ContrapondereaLipsaSauModificataEsteDetectata(FelTranzactie fel, int modificare) =>
        Proprietate.Verifica(Proprietate.Cazuri, (rng, _) => {
            var t = Motor.Opereaza(Scena(rng), new(MidpointRounding.ToEven))
                .Tranzactii.Single(t => t.Fel == (fel == FelTranzactie.Storno ? FelTranzactie.Operare : fel));
            if (fel == FelTranzactie.Storno)
                t = Storno.Inverseaza(t.Postari, t.Document!.Value, Data.AddMonths(1), 202602);
            var postari = t.Postari.ToList();
            var p = postari[1];
            if (modificare == 0) postari.RemoveAt(1);
            else postari[1] = p with { Coordonate = modificare == 1
                ? p.Coordonate with { Produs = Gen.Produse[0] }
                : p.Coordonate with { Cont = Gen.Conturi[2] } };
            Assert.NotEmpty(Conservare.Verifica(t with { Postari = postari }));
        });

    [Fact]
    public void CapatulTransformariiRefuzaFormeleStraine() {
        var linie = Linie(Gen.Documente[0], Gen.Conturi[0], Gen.Produse[0],
            RolTransformare.Consum, FelTranzactie.Transfer, 1m, 1m);
        var real = linie.Real;
        Capat[] invalide = [real with { Unitate = null },
            real with { Unitate = real.Unitate! with { Fel = FelUnitate.Partida } },
            real with { Gestiune = null }, real with { Produs = null },
            real with { Cont = Gen.Conturi[1] }, real with { Produs = Gen.Produse[1] },
            real with { Carte = Carte.Fiscal }, real with { Partener = Gen.Documente[1] },
            real with { Unitate = real.Unitate! with { Partener = Gen.Documente[1] } },
            real with { CodTva = new(Guid.NewGuid(), SensTva.Achizitie, RolTva.Baza) },
            real with { PerioadaDeclarare = 202601 }, real with { Valuta = Guid.NewGuid() },
            .. new[] { GestiuniVirtuale.Furnizor, GestiuniVirtuale.Client, GestiuniVirtuale.Consum,
                GestiuniVirtuale.Transformare }.Select(g => real with { Gestiune = g })];
        foreach (var invalid in invalide)
            Assert.Throws<ArgumentException>(() => Transformare.Postari(linie with { Real = invalid }, Data));
        Assert.Throws<ArgumentException>(() => Transformare.Postari(linie with {
            Real = real with { Gestiune = GestiuniVirtuale.Transformare, Unitate = null } }, Data));
    }

    [Fact]
    public void DeclaratiaTransformariiEsteStructuralaSiCauzata() {
        var d = Scena(new Random(1234));
        Assert.Equal(d, d with { Transformari = d.Transformari.ToArray() });
        Assert.Equal(d.GetHashCode(), (d with { Transformari = d.Transformari.ToArray() }).GetHashCode());
        Assert.Throws<ArgumentException>(() => d with { Transformari = [] });
        Assert.Throws<ArgumentException>(() => d with { Document = Gen.Documente[1] });
        Assert.Throws<ArgumentException>(() => d with {
            Transformari = [d.Transformari[0] with { Cauza = new(Gen.Documente[1], null) }],
        });
    }

    [Fact]
    public void TransformareaDezechilibrataRefuzaIntregulContract() {
        var d = Scena(new Random(1234));
        var l = d.Transformari.ToArray();
        l[0] = l[0] with { Valoare = l[0].Valoare + 0.01m };
        var c = Motor.Opereaza(d with { Transformari = l }, new(MidpointRounding.ToEven));
        Assert.False(c.EsteAcceptat);
        Assert.Empty(c.Tranzactii);
    }
}
