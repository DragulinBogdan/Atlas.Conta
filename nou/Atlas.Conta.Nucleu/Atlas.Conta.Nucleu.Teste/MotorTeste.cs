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
            Assert.Equal(declaratie.Decizii, contract.Decizii);
            Assert.Equal(declaratie.Ipoteze, contract.Ipoteze);
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
    public void DataMutataPeOSinguraPostarePica() =>
        Perturbarea(
            aleator => Gen.Declaratie(aleator),
            p => p with { Coordonate = p.Coordonate with { Data = p.Coordonate.Data.AddDays(1) } },
            Coduri.DataStraina);

    [Fact]
    public void ListaApelantuluiNuMaiAtingeDeclaratia() {
        var document = Gen.Documente[0];
        var miscare = new Miscare(
            new Capat { Cont = Gen.Conturi[0] },
            new Capat { Cont = Gen.Conturi[1] },
            0m,
            0m,
            10m,
            new Cauza(document, null));
        var miscari = new List<Miscare> { miscare };
        var decizii = new List<Decizie> { new ContRezolvat(Gen.Linii[0], Gen.Conturi[0], "politica") };
        var declaratie = new Declaratie(document, new DateOnly(2026, 1, 1), miscari, decizii, []);
        var contract = Motor.Opereaza(declaratie, Rotunjire());
        miscari.Add(miscare);
        decizii.Clear();
        Assert.Single(declaratie.Miscari);
        Assert.Single(declaratie.Decizii);
        Assert.Single(contract.Decizii);
        Assert.Equal(2, Acceptata(contract).Postari.Count);
    }

    [Fact]
    public void ListeleApelantuluiNuMaiAtingContractul() {
        var document = Gen.Documente[0];
        var data = new DateOnly(2026, 1, 1);
        var tranzactie = new Tranzactie(FelTranzactie.Operare, data, document, []);
        var decizii = new List<Decizie> { new ContRezolvat(Gen.Linii[0], Gen.Conturi[0], "politica") };
        var ipoteze = new List<Ipoteza> { new PerioadaDeschisa(2026, 1) };
        var refuzuri = new List<Refuz> { new(Coduri.PostariLipsa, "fără postări", null) };
        var acceptat = Contract.Accepta([tranzactie], decizii, ipoteze, 0);
        var refuzat = Contract.Refuza(refuzuri, decizii, ipoteze, 0);
        decizii.Clear();
        ipoteze.Clear();
        refuzuri.Clear();
        Assert.Single(acceptat.Decizii);
        Assert.Single(acceptat.Ipoteze);
        Assert.Single(refuzat.Decizii);
        Assert.Single(refuzat.Ipoteze);
        Assert.Single(refuzat.Refuzuri);
    }

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
            Assert.Empty(contract.Tranzactii);
            Assert.Contains(contract.Refuzuri, refuz => refuz.Cod == Coduri.GestiuneLipsa);
            Assert.Equal(fara.Decizii, contract.Decizii);
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

    // ── T-D2: `Miscari` → `Operare`, `Mutari` → `Transfer`, ambele → două tranzacții ──
    [Fact]
    public void DeclaratiaDoarCuMutariDaOSinguraTranzactieDeTransfer() =>
        Proprietate.Verifica(Proprietate.Cazuri, (aleator, _) => {
            var declaratie = Gen.DoarMutari(aleator);
            var tranzactie = Acceptata(Motor.Opereaza(declaratie, Rotunjire()));
            Assert.Equal(FelTranzactie.Transfer, tranzactie.Fel);
            Assert.Equal(declaratie.Document, tranzactie.Document);
            Assert.Equal(declaratie.Data, tranzactie.Data);
            Assert.Equal(2 * declaratie.Mutari.Count, tranzactie.Postari.Count);
            Assert.Empty(Conservare.Verifica(tranzactie));
            foreach (var (_, suma) in Aduna(
                         tranzactie.Postari,
                         p => (p.Coordonate.Cont, p.Coordonate.Latura),
                         p => p.Valoare))
                Assert.Equal(0m, suma);
            foreach (var (_, suma) in Aduna(
                         tranzactie.Postari.Where(p => p.Cantitate != 0m),
                         p => (p.Coordonate.Cont, p.Coordonate.Produs),
                         p => p.Cantitate))
                Assert.Equal(0m, suma);
        });

    [Fact]
    public void DeclaratiaCuAmbeleListeDaOperareaApoiTransferul() =>
        Proprietate.Verifica(Proprietate.Cazuri, (aleator, _) => {
            var declaratie = Gen.Declaratie(aleator, cuMutari: true);
            var tranzactii = Tranzactiile(Motor.Opereaza(declaratie, Rotunjire()));
            Assert.Equal(2, tranzactii.Count);
            Assert.Equal(FelTranzactie.Operare, tranzactii[0].Fel);
            Assert.Equal(FelTranzactie.Transfer, tranzactii[1].Fel);
            Assert.Equal(2 * declaratie.Miscari.Count, tranzactii[0].Postari.Count);
            Assert.Equal(2 * declaratie.Mutari.Count, tranzactii[1].Postari.Count);
            foreach (var tranzactie in tranzactii) {
                Assert.Equal(declaratie.Document, tranzactie.Document);
                Assert.Equal(declaratie.Data, tranzactie.Data);
                Assert.Empty(Conservare.Verifica(tranzactie));
            }
        });

    [Fact]
    public void DeclaratiaCuMutariGoaleRamaneOSinguraOperare() =>
        Proprietate.Verifica(Proprietate.Cazuri, (aleator, _) => {
            var declaratie = Gen.Declaratie(aleator);
            Assert.Empty(declaratie.Mutari);
            var tranzactie = Acceptata(Motor.Opereaza(declaratie, Rotunjire()));
            Assert.Equal(FelTranzactie.Operare, tranzactie.Fel);
            Assert.Equal(2 * declaratie.Miscari.Count, tranzactie.Postari.Count);
        });

    [Fact]
    public void DeclaratiaCuMutarePeAltDocumentERefuzataDeApelant() =>
        Proprietate.Verifica(Proprietate.Cazuri, (aleator, _) => {
            var declaratie = Gen.DoarMutari(aleator);
            var mutare = declaratie.Mutari[0];
            var strain = Strain(declaratie.Document);
            Assert.Throws<ArgumentException>(() =>
                declaratie with { Mutari = [mutare with { Cauza = mutare.Cauza with { Document = strain } }] });
        });

    [Fact]
    public void DeclaratiaGolitaDeAmbeleListeERefuzataDeApelant() =>
        Proprietate.Verifica(Proprietate.Cazuri, (aleator, _) => {
            var declaratie = Gen.Declaratie(aleator, cuMutari: true);
            Assert.Throws<ArgumentException>(() => declaratie with { Miscari = [], Mutari = [] });
            Assert.Equal(
                declaratie.Mutari,
                (declaratie with { Miscari = [] }).Mutari);
            Assert.Equal(
                declaratie.Miscari,
                (declaratie with { Mutari = [] }).Miscari);
        });

    static List<(TCheie Cheie, decimal Suma)> Aduna<TCheie>(
            IEnumerable<Postare> postari, Func<Postare, TCheie> cheie, Func<Postare, decimal> masura)
            where TCheie : notnull {
        var sume = new Dictionary<TCheie, decimal>();
        foreach (var postare in postari)
            sume[cheie(postare)] = sume.GetValueOrDefault(cheie(postare)) + masura(postare);
        return [.. sume.Select(p => (p.Key, p.Value))];
    }

    static Rotunjire Rotunjire() => new(MidpointRounding.AwayFromZero);

    static Guid Strain(Guid document) => Gen.Documente.First(d => d != document);

    static Tranzactie Acceptata(Contract contract) => Assert.Single(Tranzactiile(contract));

    static IReadOnlyList<Tranzactie> Tranzactiile(Contract contract) {
        Assert.True(
            contract.EsteAcceptat,
            string.Join(" | ", contract.Refuzuri.Select(r => $"{r.Cod}: {r.Mesaj}")));
        return contract.Tranzactii;
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
