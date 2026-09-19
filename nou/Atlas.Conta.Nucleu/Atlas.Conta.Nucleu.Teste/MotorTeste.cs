using Xunit;

namespace Atlas.Conta.Nucleu.Teste;

public class MotorTeste {
    [Fact]
    public void OperareaDaOSinguraTranzactieBalansata() =>
        Proprietate.Verifica(Proprietate.Cazuri, (aleator, _) => {
            var declaratie = Gen.Declaratie(aleator);
            var contract = Motor.Opereaza(declaratie, Rotunjire());
            var tranzactie = Acceptata(contract);
            Assert.Equal(FelTranzactie.Operare, tranzactie.Fel);
            Assert.Equal(declaratie.Document, tranzactie.Document);
            Assert.Equal(declaratie.Data, tranzactie.Data);
            Assert.Equal(2 * declaratie.Miscari.Count, tranzactie.Postari.Count);
            Assert.Empty(Conservare.Verifica(tranzactie));
            Assert.Same(declaratie.Decizii, contract.Decizii);
            Assert.Same(declaratie.Ipoteze, contract.Ipoteze);
            Assert.Equal(0, contract.JumatatiDeBan);
        });

    [Fact]
    public void FiecareMiscareDaDebitulApoiCreditul() =>
        Proprietate.Verifica(Proprietate.Cazuri, (aleator, _) => {
            var declaratie = Gen.Declaratie(aleator, cuCantitate: true);
            var tranzactie = Acceptata(Motor.Opereaza(declaratie, Rotunjire()));
            for (var i = 0; i < declaratie.Miscari.Count; i++) {
                var miscare = declaratie.Miscari[i];
                var debit = tranzactie.Postari[2 * i];
                var credit = tranzactie.Postari[2 * i + 1];
                Assert.Equal(Latura.Debit, debit.Coordonate.Latura);
                Assert.Equal(miscare.La.Cont, debit.Coordonate.Cont);
                Assert.Equal(miscare.Cantitate, debit.Cantitate);
                Assert.Equal(Latura.Credit, credit.Coordonate.Latura);
                Assert.Equal(miscare.DeLa.Cont, credit.Coordonate.Cont);
                Assert.Equal(-miscare.Cantitate, credit.Cantitate);
                Assert.Equal(miscare.Valoare, debit.Valoare);
                Assert.Equal(miscare.Valoare, credit.Valoare);
                Assert.Equal(miscare.Cauza, debit.Cauza);
                Assert.Equal(declaratie.Data, debit.Coordonate.Data);
                Assert.Equal(declaratie.Data, credit.Coordonate.Data);
            }
        });

    [Fact]
    public void ValoareaPerturbataPeOSinguraPostarePica() =>
        Perturbarea(
            aleator => Gen.Declaratie(aleator),
            p => p with { Valoare = p.Valoare + 0.01m },
            Coduri.ConservareValoare);

    [Fact]
    public void CantitateaPerturbataPeOSinguraPostarePica() =>
        Perturbarea(
            aleator => Gen.Declaratie(aleator, cuCantitate: true),
            p => p with { Cantitate = p.Cantitate + 0.001m },
            Coduri.ConservareCantitate);

    [Fact]
    public void LaturaIntoarsaPeOSinguraPostarePica() =>
        Perturbarea(
            aleator => Gen.Declaratie(aleator),
            p => p with {
                Coordonate = p.Coordonate with {
                    Latura = p.Coordonate.Latura == Latura.Debit ? Latura.Credit : Latura.Debit,
                },
            },
            Coduri.ConservareValoare);

    [Fact]
    public void CauzaMutataPeAltDocumentPica() =>
        Perturbarea(
            aleator => Gen.Declaratie(aleator),
            p => p with { Cauza = p.Cauza with { Document = Strain(p.Cauza.Document) } },
            Coduri.CauzaStraina);

    [Fact]
    public void DeclaratiaCuCantitateFaraGestiuneERefuzata() =>
        Proprietate.Verifica(Proprietate.Cazuri, (aleator, _) => {
            var declaratie = Gen.Declaratie(aleator, cuCantitate: true);
            var miscare = declaratie.Miscari[0];
            var fara = declaratie with {
                Miscari = [miscare with { La = miscare.La with { Gestiune = null, Unitate = null } }],
            };
            var contract = Motor.Opereaza(fara, Rotunjire());
            Assert.False(contract.EsteAcceptat);
            Assert.Null(contract.Tranzactie);
            Assert.Contains(contract.Refuzuri, refuz => refuz.Cod == Coduri.GestiuneLipsa);
            Assert.Same(fara.Decizii, contract.Decizii);
        });

    [Fact]
    public void TransferulDaOSinguraTranzactieConservata() =>
        Proprietate.Verifica(Proprietate.Cazuri, (aleator, _) => {
            var document = Gen.Unul(aleator, Gen.Documente);
            var data = Gen.Data(aleator);
            var mutari = Gen.Mutari(aleator, document);
            var contract = Motor.Transfera(document, data, mutari, Rotunjire());
            var tranzactie = Acceptata(contract);
            Assert.Equal(FelTranzactie.Transfer, tranzactie.Fel);
            Assert.Equal(document, tranzactie.Document);
            Assert.Equal(data, tranzactie.Data);
            Assert.Equal(2 * mutari.Count, tranzactie.Postari.Count);
            Assert.Empty(Conservare.Verifica(tranzactie));
            for (var i = 0; i < mutari.Count; i++) {
                var mutare = mutari[i];
                var iesire = tranzactie.Postari[2 * i];
                var intrare = tranzactie.Postari[2 * i + 1];
                Assert.Equal(mutare.Latura, iesire.Coordonate.Latura);
                Assert.Equal(mutare.Latura, intrare.Coordonate.Latura);
                Assert.Equal(-mutare.Valoare, iesire.Valoare);
                Assert.Equal(mutare.Valoare, intrare.Valoare);
                Assert.Equal(-mutare.Cantitate, iesire.Cantitate);
                Assert.Equal(mutare.Cantitate, intrare.Cantitate);
            }
        });

    [Fact]
    public void TransferulCuOPostareMutataPeAltContPica() =>
        Proprietate.Verifica(Proprietate.Cazuri, (aleator, _) => {
            var document = Gen.Unul(aleator, Gen.Documente);
            var tranzactie = Acceptata(Motor.Transfera(
                document,
                Gen.Data(aleator),
                Gen.Mutari(aleator, document),
                Rotunjire()));
            var perturbata = Perturba(tranzactie, aleator, p => p with {
                Coordonate = p.Coordonate with {
                    Cont = Gen.AltulDecat(aleator, Gen.Conturi, p.Coordonate.Cont),
                },
            });
            Assert.Contains(Conservare.Verifica(perturbata), refuz => refuz.Cod == Coduri.ConservareTransfer);
        });

    [Fact]
    public void TransferulIntreContruiDiferitiERefuzatDeApelant() {
        var document = Gen.Documente[0];
        var mutare = new Mutare(
            new Capat { Cont = Gen.Conturi[0] },
            new Capat { Cont = Gen.Conturi[1] },
            Latura.Debit,
            0m,
            0m,
            10m,
            new Cauza(document, null));
        Assert.Throws<ArgumentException>(() =>
            Motor.Transfera(document, new DateOnly(2026, 1, 1), [mutare], Rotunjire()));
    }

    [Fact]
    public void TransferulCuCauzaPeAltDocumentERefuzatDeApelant() {
        var mutare = new Mutare(
            new Capat { Cont = Gen.Conturi[0] },
            new Capat { Cont = Gen.Conturi[0] },
            Latura.Debit,
            0m,
            0m,
            10m,
            new Cauza(Gen.Documente[1], null));
        Assert.Throws<ArgumentException>(() =>
            Motor.Transfera(Gen.Documente[0], new DateOnly(2026, 1, 1), [mutare], Rotunjire()));
    }

    [Fact]
    public void TransferulFaraMutariERefuzatDeApelant() =>
        Assert.Throws<ArgumentException>(() =>
            Motor.Transfera(Gen.Documente[0], new DateOnly(2026, 1, 1), [], Rotunjire()));

    [Fact]
    public void DeclaratiaFaraMiscariERefuzataDeApelant() =>
        Assert.Throws<ArgumentException>(() =>
            new Declaratie(Gen.Documente[0], new DateOnly(2026, 1, 1), [], [], []));

    [Fact]
    public void DeclaratiaCuMiscarePeAltDocumentERefuzataDeApelant() =>
        Proprietate.Verifica(Proprietate.Cazuri, (aleator, _) => {
            var declaratie = Gen.Declaratie(aleator);
            var miscare = declaratie.Miscari[0];
            var strain = Strain(declaratie.Document);
            Assert.Throws<ArgumentException>(() =>
                declaratie with { Miscari = [miscare with { Cauza = miscare.Cauza with { Document = strain } }] });
        });

    [Fact]
    public void ContractulRefuzatFaraRefuzERefuzatDeApelant() =>
        Assert.Throws<ArgumentException>(() => Contract.Refuza([], [], [], 0));

    [Fact]
    public void ContorulDeJumatatiEAlInstanteiPrimite() =>
        Proprietate.Verifica(Proprietate.Cazuri, (aleator, _) => {
            var rotunjire = Rotunjire();
            var cate = aleator.Next(4);
            for (var i = 0; i < cate; i++)
                rotunjire.Bani(0.005m);
            var contract = Motor.Opereaza(Gen.Declaratie(aleator), rotunjire);
            Assert.Equal(cate, contract.JumatatiDeBan);
        });

    static Rotunjire Rotunjire() => new(MidpointRounding.AwayFromZero);

    static Guid Strain(Guid document) => Gen.Documente.First(d => d != document);

    static Tranzactie Acceptata(Contract contract) {
        Assert.True(
            contract.EsteAcceptat,
            string.Join(" | ", contract.Refuzuri.Select(r => $"{r.Cod}: {r.Mesaj}")));
        Assert.NotNull(contract.Tranzactie);
        return contract.Tranzactie;
    }

    static void Perturbarea(Func<Random, Declaratie> declara, Func<Postare, Postare> schimba, string cod) =>
        Proprietate.Verifica(Proprietate.Cazuri, (aleator, _) => {
            var tranzactie = Acceptata(Motor.Opereaza(declara(aleator), Rotunjire()));
            var perturbata = Perturba(tranzactie, aleator, schimba);
            Assert.Contains(Conservare.Verifica(perturbata), refuz => refuz.Cod == cod);
        });

    static Tranzactie Perturba(Tranzactie tranzactie, Random aleator, Func<Postare, Postare> schimba) {
        var postari = tranzactie.Postari.ToList();
        var ales = aleator.Next(postari.Count);
        postari[ales] = schimba(postari[ales]);
        return tranzactie with { Postari = postari };
    }
}
