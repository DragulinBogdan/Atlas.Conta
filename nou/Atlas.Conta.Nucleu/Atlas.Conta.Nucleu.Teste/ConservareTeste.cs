using Xunit;

namespace Atlas.Conta.Nucleu.Teste;

public class ConservareTeste {
    [Fact]
    public void OperareaDinMiscariEBalansata() =>
        Proprietate.Verifica(Proprietate.Cazuri, (aleator, _) =>
            FaraRefuz(Conservare.Verifica(Gen.Operare(aleator))));

    [Fact]
    public void ValoareaPerturbataPeOSinguraPostarePica() =>
        Proprietate.Verifica(Proprietate.Cazuri, (aleator, _) => {
            var tranzactie = Gen.Operare(aleator);
            var perturbata = Perturba(tranzactie, aleator, p => p with { Valoare = p.Valoare + 0.01m });
            AreCodul(Conservare.Verifica(perturbata), Coduri.ConservareValoare);
        });

    [Fact]
    public void CantitateaPerturbataPeOSinguraPostarePica() =>
        Proprietate.Verifica(Proprietate.Cazuri, (aleator, _) => {
            var tranzactie = Gen.Operare(aleator, cuCantitate: true);
            var perturbata = Perturba(tranzactie, aleator, p => p with { Cantitate = p.Cantitate + 0.001m });
            AreCodul(Conservare.Verifica(perturbata), Coduri.ConservareCantitate);
        });

    [Fact]
    public void LaturaIntoarsaPeOSinguraPostarePica() =>
        Proprietate.Verifica(Proprietate.Cazuri, (aleator, _) => {
            var tranzactie = Gen.Operare(aleator);
            var perturbata = Perturba(tranzactie, aleator, p => p with {
                Coordonate = p.Coordonate with {
                    Latura = p.Coordonate.Latura == Latura.Debit ? Latura.Credit : Latura.Debit,
                },
            });
            AreCodul(Conservare.Verifica(perturbata), Coduri.ConservareValoare);
        });

    [Fact]
    public void CauzaStrainaPeOSinguraPostarePica() =>
        Proprietate.Verifica(Proprietate.Cazuri, (aleator, _) => {
            var tranzactie = Gen.Operare(aleator);
            var perturbata = Perturba(tranzactie, aleator, p => p with {
                Cauza = p.Cauza with { Document = Gen.AltulDecat(aleator, Gen.Documente, tranzactie.Document!.Value) },
            });
            AreCodul(Conservare.Verifica(perturbata), Coduri.CauzaStraina);
        });

    [Fact]
    public void TranzactiaFaraDocumentPica() =>
        Proprietate.Verifica(Proprietate.Cazuri, (aleator, _) => {
            var tranzactie = Gen.Operare(aleator);
            AreCodul(Conservare.Verifica(tranzactie with { Document = null }), Coduri.DocumentLipsa);
        });

    [Fact]
    public void TransferulEchilibratTrece() =>
        Proprietate.Verifica(Proprietate.Cazuri, (aleator, _) =>
            FaraRefuz(Conservare.Verifica(Gen.Transfer(aleator))));

    [Fact]
    public void TransferulCuOPostareMutataPeAltContPica() =>
        Proprietate.Verifica(Proprietate.Cazuri, (aleator, _) => {
            var tranzactie = Gen.Transfer(aleator);
            var perturbata = Perturba(tranzactie, aleator, p => p with {
                Coordonate = p.Coordonate with {
                    Cont = Gen.AltulDecat(aleator, Gen.Conturi, p.Coordonate.Cont),
                },
            });
            AreCodul(Conservare.Verifica(perturbata), Coduri.ConservareTransfer);
        });

    [Fact]
    public void ValoareaNegativaPicaLaOperareSiTreceLaStorno() =>
        Proprietate.Verifica(Proprietate.Cazuri, (aleator, _) => {
            var tranzactie = Gen.Operare(aleator);
            var negata = tranzactie with {
                Postari = tranzactie.Postari.Select(p => p with { Valoare = -p.Valoare }).ToList(),
            };
            AreCodul(Conservare.Verifica(negata), Coduri.SemnNegativ);
            FaraRefuz(Conservare.Verifica(negata with { Fel = FelTranzactie.Storno }));
        });

    [Fact]
    public void DeschidereaEScutitaDeEgalitateDarNuAreDocument() {
        var dezechilibrate = 0;
        Proprietate.Verifica(Proprietate.Cazuri, (aleator, _) => {
            var tranzactie = Gen.Deschidere(aleator);
            if (SumaPeLaturi(tranzactie) != 0m)
                dezechilibrate++;
            FaraRefuz(Conservare.Verifica(tranzactie));
            AreCodul(
                Conservare.Verifica(tranzactie with { Document = Gen.Unul(aleator, Gen.Documente) }),
                Coduri.DocumentNeasteptat);
        });
        Assert.True(dezechilibrate > 0, "generatorul n-a produs nicio deschidere dezechilibrată");
    }

    [Fact]
    public void GestiuneaLipsaPeOCantitatePica() =>
        Proprietate.Verifica(Proprietate.Cazuri, (aleator, _) => {
            var tranzactie = Gen.Operare(aleator, cuCantitate: true);
            var perturbata = Perturba(tranzactie, aleator, p => p with {
                Coordonate = p.Coordonate with { Gestiune = null },
            });
            AreCodul(Conservare.Verifica(perturbata), Coduri.GestiuneLipsa);
        });

    [Fact]
    public void UnitateaLipsaPeOCantitatePica() =>
        Proprietate.Verifica(Proprietate.Cazuri, (aleator, _) => {
            var tranzactie = Gen.Operare(aleator, cuCantitate: true);
            var perturbata = Perturba(tranzactie, aleator, p => p with {
                Coordonate = p.Coordonate with { Unitate = null },
            });
            AreCodul(Conservare.Verifica(perturbata), Coduri.UnitateLipsa);
        });

    [Fact]
    public void CantitateaFaraUnitatePeGestiuneVirtualaTrece() =>
        Proprietate.Verifica(Proprietate.Cazuri, (aleator, _) => {
            var tranzactie = Gen.Operare(aleator, cuCantitate: true);
            var perturbata = Perturba(tranzactie, aleator, p => p with {
                Coordonate = p.Coordonate with { Gestiune = GestiuniVirtuale.Furnizor, Unitate = null },
            });
            FaraRefuz(Conservare.Verifica(perturbata));
        });

    [Fact]
    public void CantitateaFaraUnitatePeGestiuneRealaPica() =>
        Proprietate.Verifica(Proprietate.Cazuri, (aleator, _) => {
            var tranzactie = Gen.Operare(aleator, cuCantitate: true);
            var perturbata = Perturba(tranzactie, aleator, p => p with {
                Coordonate = p.Coordonate with { Gestiune = Gen.Unul(aleator, Gen.Gestiuni), Unitate = null },
            });
            AreCodul(Conservare.Verifica(perturbata), Coduri.UnitateLipsa);
        });

    [Fact]
    public void ProdusulLipsaPeUnLotPica() =>
        Proprietate.Verifica(Proprietate.Cazuri, (aleator, _) => {
            var tranzactie = Gen.Operare(aleator, cuCantitate: true);
            var perturbata = Perturba(
                tranzactie,
                aleator,
                p => p with { Coordonate = p.Coordonate with { Produs = null } },
                p => p.Coordonate.Unitate?.Fel == FelUnitate.Lot);
            AreCodul(Conservare.Verifica(perturbata), Coduri.ProdusLipsa);
        });

    [Fact]
    public void PartenerulLipsaPeOPartidaPica() =>
        Proprietate.Verifica(Proprietate.Cazuri, (aleator, _) => {
            var tranzactie = Gen.Operare(aleator, cuCantitate: true);
            var perturbata = Perturba(
                tranzactie,
                aleator,
                p => p with { Coordonate = p.Coordonate with { Partener = null } },
                p => p.Coordonate.Unitate?.Fel == FelUnitate.Partida);
            AreCodul(Conservare.Verifica(perturbata), Coduri.PartenerLipsa);
        });

    [Fact]
    public void ProdusulStrainFataDeLotPica() =>
        Proprietate.Verifica(Proprietate.Cazuri, (aleator, _) => {
            var tranzactie = Gen.Operare(aleator, cuCantitate: true);
            var perturbata = Perturba(
                tranzactie,
                aleator,
                p => p with {
                    Coordonate = p.Coordonate with {
                        Produs = Gen.AltulDecat(aleator, Gen.Produse, p.Coordonate.Produs!.Value),
                    },
                },
                p => p.Coordonate.Unitate?.Fel == FelUnitate.Lot);
            AreCodul(Conservare.Verifica(perturbata), Coduri.UnitateNepotrivita);
        });

    [Fact]
    public void PartenerulStrainFataDePartidaPica() =>
        Proprietate.Verifica(Proprietate.Cazuri, (aleator, _) => {
            var tranzactie = Gen.Operare(aleator, cuCantitate: true);
            var perturbata = Perturba(
                tranzactie,
                aleator,
                p => p with {
                    Coordonate = p.Coordonate with {
                        Partener = Gen.AltulDecat(aleator, Gen.Parteneri, p.Coordonate.Partener!.Value),
                    },
                },
                p => p.Coordonate.Unitate?.Fel == FelUnitate.Partida);
            AreCodul(Conservare.Verifica(perturbata), Coduri.UnitateNepotrivita);
        });

    [Fact]
    public void UnitateaPeAltContPica() =>
        Proprietate.Verifica(Proprietate.Cazuri, (aleator, _) => {
            var tranzactie = Gen.Operare(aleator, cuCantitate: true);
            var perturbata = Perturba(
                tranzactie,
                aleator,
                p => p with {
                    Coordonate = p.Coordonate with {
                        Unitate = p.Coordonate.Unitate! with {
                            Cont = Gen.AltulDecat(aleator, Gen.Conturi, p.Coordonate.Unitate!.Cont),
                        },
                    },
                },
                p => p.Coordonate.Unitate?.Fel == FelUnitate.Lot);
            AreCodul(Conservare.Verifica(perturbata), Coduri.UnitateNepotrivita);
        });

    [Fact]
    public void DataMutataPeOSinguraPostarePica() =>
        Proprietate.Verifica(Proprietate.Cazuri, (aleator, _) => {
            var tranzactie = Gen.Operare(aleator);
            var perturbata = Perturba(tranzactie, aleator, p => p with {
                Coordonate = p.Coordonate with { Data = p.Coordonate.Data.AddDays(1) },
            });
            AreCodul(Conservare.Verifica(perturbata), Coduri.DataStraina);
        });

    [Theory]
    [InlineData(FelTranzactie.Operare)]
    [InlineData(FelTranzactie.Storno)]
    [InlineData(FelTranzactie.Transfer)]
    [InlineData(FelTranzactie.Deschidere)]
    public void TranzactiaFaraPostariPica(FelTranzactie fel) {
        var tranzactie = new Tranzactie(
            fel,
            new DateOnly(2026, 1, 1),
            fel == FelTranzactie.Deschidere ? null : Gen.Documente[0],
            []);
        var refuzuri = Conservare.Verifica(tranzactie);
        AreCodul(refuzuri, Coduri.PostariLipsa);
        Assert.Null(refuzuri.First(r => r.Cod == Coduri.PostariLipsa).Linie);
    }

    [Fact]
    public void CartileNuSeBalanseazaUnaPrinAlta() {
        var document = Gen.Documente[0];
        var data = new DateOnly(2026, 1, 1);
        var cauza = new Cauza(document, Gen.Linii[0]);
        var tranzactie = new Tranzactie(FelTranzactie.Operare, data, document, [
            new Postare(
                new Coordonate {
                    Cont = Gen.Conturi[0],
                    Latura = Latura.Debit,
                    Data = data,
                    Carte = Carte.Contabil,
                },
                0m,
                0m,
                10m,
                cauza),
            new Postare(
                new Coordonate {
                    Cont = Gen.Conturi[1],
                    Latura = Latura.Credit,
                    Data = data,
                    Carte = Carte.Fiscal,
                },
                0m,
                0m,
                10m,
                cauza),
        ]);
        AreCodul(Conservare.Verifica(tranzactie), Coduri.ConservareValoare);
        var doarContabil = tranzactie with {
            Postari = tranzactie.Postari
                .Select(p => p with { Coordonate = p.Coordonate with { Carte = Carte.Contabil } })
                .ToList(),
        };
        FaraRefuz(Conservare.Verifica(doarContabil));
    }

    [Fact]
    public void CantitateaFaraProdusPica() =>
        Proprietate.Verifica(Proprietate.Cazuri, (aleator, _) => {
            var tranzactie = Gen.Operare(aleator, cuCantitate: true);
            var perturbata = Perturba(tranzactie, aleator, p => p with {
                Coordonate = p.Coordonate with { Produs = null, Unitate = null },
            });
            AreCodul(Conservare.Verifica(perturbata), Coduri.ProdusLipsa);
        });

    static decimal SumaPeLaturi(Tranzactie tranzactie) =>
        tranzactie.Postari.Sum(p => p.Coordonate.Latura == Latura.Debit ? p.Valoare : -p.Valoare);

    static Tranzactie Perturba(
        Tranzactie tranzactie,
        Random aleator,
        Func<Postare, Postare> schimba,
        Func<Postare, bool>? candidat = null) {
        var indici = Enumerable.Range(0, tranzactie.Postari.Count)
            .Where(i => candidat?.Invoke(tranzactie.Postari[i]) ?? true)
            .ToList();
        Assert.NotEmpty(indici);
        var ales = indici[aleator.Next(indici.Count)];
        var postari = tranzactie.Postari.ToList();
        postari[ales] = schimba(postari[ales]);
        return tranzactie with { Postari = postari };
    }

    static void FaraRefuz(IReadOnlyList<Refuz> refuzuri) =>
        Assert.True(refuzuri.Count == 0, Descrie(refuzuri));

    static void AreCodul(IReadOnlyList<Refuz> refuzuri, string cod) =>
        Assert.True(refuzuri.Any(r => r.Cod == cod), $"aștept {cod}, am {Descrie(refuzuri)}");

    static string Descrie(IReadOnlyList<Refuz> refuzuri) =>
        refuzuri.Count == 0 ? "(niciun refuz)" : string.Join(" | ", refuzuri.Select(r => $"{r.Cod}: {r.Mesaj}"));
}
