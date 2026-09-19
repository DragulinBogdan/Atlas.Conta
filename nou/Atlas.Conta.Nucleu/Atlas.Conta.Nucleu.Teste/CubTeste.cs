using Xunit;

namespace Atlas.Conta.Nucleu.Teste;

public class CubTeste {
    [Fact]
    public void SnapshotulESumaLaGranita() =>
        Proprietate.Verifica(Proprietate.Cazuri, (aleator, _) => {
            var tranzactii = Istorie(aleator);
            var (t0, t) = Granite(aleator);
            foreach (var includeTransfer in new[] { true, false })
                foreach (var (numeFiltru, filtru) in Filtre)
                    foreach (var (nume, cheie) in Proiectii.Toate) {
                        var pana = Cub.Sold(tranzactii, cheie, filtru, t, null, includeTransfer);
                        var inainte = Cub.Sold(tranzactii, cheie, filtru, t0, null, includeTransfer);
                        var intre = Cub.Sold(tranzactii, cheie, filtru, t, t0, includeTransfer);
                        Proiectii.Egale(
                            pana,
                            Proiectii.Aduna(inainte, intre),
                            $"{nume}/{numeFiltru}/transfer={includeTransfer} la {t0}…{t}");
                    }
        });

    [Fact]
    public void FaraTransferCadeExactTransferul() =>
        Proprietate.Verifica(Proprietate.Cazuri, (aleator, _) => {
            var tranzactii = Istorie(aleator);
            var fara = tranzactii.Where(tranzactie => tranzactie.Fel != FelTranzactie.Transfer).ToList();
            foreach (var (nume, cheie) in Proiectii.Toate)
                Proiectii.Egale(
                    Cub.Sold(tranzactii, cheie, includeTransfer: false),
                    Cub.Sold(fara, cheie),
                    nume);
        });

    [Fact]
    public void GranitaDeSusEInclusivaIarCeaDeJosExclusiva() {
        var tranzactii = Trei();
        Func<Postare, Guid> cheie = p => p.Coordonate.Cont;
        Assert.Equal(2m, Cub.Sold(tranzactii, cheie, panaLa: new DateOnly(2026, 2, 1)).Values.Single().Debit);
        Assert.Equal(1m, Cub.Sold(tranzactii, cheie, dupa: new DateOnly(2026, 2, 1)).Values.Single().Debit);
        Assert.Equal(
            1m,
            Cub.Sold(tranzactii, cheie, panaLa: new DateOnly(2026, 2, 1), dupa: new DateOnly(2026, 1, 1))
                .Values.Single().Debit);
        Assert.Empty(Cub.Sold(tranzactii, cheie, panaLa: new DateOnly(2025, 12, 31)));
    }

    [Fact]
    public void IntrareaGoalaDaDictionarGol() =>
        Assert.Empty(Cub.Sold(Array.Empty<Tranzactie>(), p => p.Coordonate.Cont));

    [Fact]
    public void FiltrulTaieExactPostarile() =>
        Proprietate.Verifica(Proprietate.Cazuri, (aleator, _) => {
            var tranzactii = Istorie(aleator);
            var debit = Cub.Sold(tranzactii, p => p.Coordonate.Cont, p => p.Coordonate.Latura == Latura.Debit);
            foreach (var sold in debit.Values)
                Assert.Equal(0m, sold.Credit);
        });

    [Fact]
    public void SoldulAdunaLaturileSiMasurile() {
        var sold = Sold.Zero + new Sold(3m, 1m, 2m, 5m) + new Sold(0m, 1m, -2m, 0m);
        Assert.Equal(new Sold(3m, 2m, 0m, 5m), sold);
        Assert.Equal(1m, sold.Net);
    }

    [Fact]
    public void SoldulUneiPostariUrmeazaLatura() {
        var pe = Gen.CapatContabil(new Random(Gen.Samanta));
        var data = new DateOnly(2026, 1, 1);
        var cauza = new Cauza(Gen.Documente[0], null);
        Assert.Equal(
            new Sold(7.50m, 0m, 0m, 1.20m),
            Sold.Din(new Postare(pe.Pe(Latura.Debit, data), 0m, 1.20m, 7.50m, cauza)));
        Assert.Equal(
            new Sold(0m, -7.50m, 0m, 0m),
            Sold.Din(new Postare(pe.Pe(Latura.Credit, data), 0m, 0m, -7.50m, cauza)));
    }

    internal static List<Tranzactie> Istorie(Random aleator) {
        var tranzactii = new List<Tranzactie>();
        var cate = 1 + aleator.Next(6);
        for (var i = 0; i < cate; i++)
            tranzactii.Add(aleator.Next(4) switch {
                0 => Gen.Transfer(aleator),
                1 => Gen.Deschidere(aleator),
                2 => Stornata(Gen.Operare(aleator), aleator),
                _ => Gen.Operare(aleator),
            });
        return tranzactii;
    }

    static Tranzactie Stornata(Tranzactie tranzactie, Random aleator) =>
        Storno.Inverseaza(tranzactie.Postari, tranzactie.Document!.Value, Gen.Data(aleator));

    static (DateOnly Jos, DateOnly Sus) Granite(Random aleator) {
        var jos = Gen.Data(aleator);
        return (jos, jos.AddDays(aleator.Next(1, 400)));
    }

    static readonly (string Nume, Func<Postare, bool>? Filtru)[] Filtre = [
        ("fără filtru", null),
        ("doar debit", p => p.Coordonate.Latura == Latura.Debit),
        ("doar cu cantitate", p => p.Cantitate != 0m),
    ];

    static List<Tranzactie> Trei() {
        var cont = Gen.Conturi[0];
        var document = Gen.Documente[0];
        return [
            Una(cont, document, new DateOnly(2026, 1, 1)),
            Una(cont, document, new DateOnly(2026, 2, 1)),
            Una(cont, document, new DateOnly(2026, 3, 1)),
        ];
    }

    static Tranzactie Una(Guid cont, Guid document, DateOnly data) =>
        new(FelTranzactie.Operare, data, document, [
            new Postare(
                new Coordonate { Cont = cont, Latura = Latura.Debit, Data = data },
                0m,
                0m,
                1m,
                new Cauza(document, null)),
        ]);
}
