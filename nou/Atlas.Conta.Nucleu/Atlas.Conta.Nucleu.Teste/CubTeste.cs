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

    [Fact]
    public void CantitateaSeAdunaDoarPeStoc() {
        var (receptia, lot, partida) = Receptie();
        var peProdus = Cub.Sold([receptia], p => p.Coordonate.Produs ?? Guid.Empty);
        Assert.Equal(5m, peProdus.Values.Single().Cantitate);
        var peUnitate = Cub.Sold([receptia], p => p.Coordonate.Unitate?.Id ?? Guid.Empty);
        Assert.Equal(5m, peUnitate[lot.Id].Cantitate);
        Assert.Equal(0m, peUnitate[partida.Id].Cantitate);
    }

    [Fact]
    public void PartidaStinsaIntegralRamaneFaraSoldSiFaraCantitate() {
        var (receptia, _, partida) = Receptie();
        // Stingerea sub TR-D2 e postarea care numește partida: laturile se adună
        // separat, deci proba e pe net.
        var nominalizata = Stingere(Latura.Debit, 100m);
        var dupaPlata = Cub.Sold(
            [receptia, nominalizata], p => p.Coordonate.Unitate?.Id ?? Guid.Empty)[partida.Id];
        Assert.Equal(0m, dupaPlata.Net);
        Assert.Equal(0m, dupaPlata.Cantitate);
        // Mutarea cu valoare SEMNATĂ (tranzacția de împerechere) golește chiar soldul:
        // cu cantitatea capătului virtual încă adunată, `Sold.Zero` era de neatins.
        var mutata = Stingere(Latura.Credit, -100m);
        var dupaTransfer = Cub.Sold(
            [receptia, mutata], p => p.Coordonate.Unitate?.Id ?? Guid.Empty)[partida.Id];
        Assert.Equal(Sold.Zero, dupaTransfer);

        Tranzactie Stingere(Latura latura, decimal valoare) {
            var data = new DateOnly(2026, 1, 8);
            return new Tranzactie(FelTranzactie.Transfer, data, Gen.Documente[1], [
                new Postare(
                    new Capat { Cont = partida.Cont, Partener = partida.Partener, Unitate = partida }
                        .Pe(latura, data),
                    0m,
                    0m,
                    valoare,
                    new Cauza(Gen.Documente[1], Gen.Linii[1])),
            ]);
        }
    }

    // N-D4: recepția are capătul virtual (−q) pe partida furnizorului și +q pe lot.
    static (Tranzactie Tranzactie, Unitate Lot, Unitate Partida) Receptie() {
        var aleator = new Random(Gen.Samanta);
        var produs = Gen.Produse[0];
        var contStoc = Gen.Conturi[0];
        var contTert = Gen.Conturi[1];
        var partener = Gen.Parteneri[0];
        var document = Gen.Documente[0];
        var data = new DateOnly(2026, 1, 5);
        var lot = Gen.Lot(aleator, contStoc, produs);
        var partida = Unitate.DeschidePartida(contTert, partener, document, data);
        var (debit, credit) = Miscare.Postari(
            new Miscare(
                new Capat {
                    Cont = contTert,
                    Partener = partener,
                    Gestiune = GestiuniVirtuale.Furnizor,
                    Produs = produs,
                    Unitate = partida,
                },
                new Capat {
                    Cont = contStoc,
                    Gestiune = Gen.Gestiuni[0],
                    Produs = produs,
                    Unitate = lot,
                },
                5m,
                0m,
                100m,
                new Cauza(document, Gen.Linii[0])),
            data);
        return (new Tranzactie(FelTranzactie.Operare, data, document, [debit, credit]), lot, partida);
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
