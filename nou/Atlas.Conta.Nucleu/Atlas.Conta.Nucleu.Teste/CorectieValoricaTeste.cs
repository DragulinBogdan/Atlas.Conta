using Xunit;

namespace Atlas.Conta.Nucleu.Teste;

public partial class CorectieValoricaTeste {
    static readonly DateOnly Z05 = new(2026, 1, 5), Z10 = new(2026, 1, 10), Z11 = new(2026, 1, 11),
        Z14 = new(2026, 1, 14), Z15 = new(2026, 1, 15), Z18 = new(2026, 1, 18), Z20 = new(2026, 1, 20), Z25 = new(2026, 1, 25);

    [Fact]
    public void CorectiaSeImpartePeStoculRamasSiPeConsum() {
        var cub = Partial();
        var a = cub.Corecteaza(200m, Z15);
        Assert.Equal((40m, 480m, 720m, 1200m), cub.Stare());

        var radacini = cub.Operare(a).Where(CorectieValorica.EsteValoric).ToList();
        Assert.Equal([80m, 120m], radacini.Select(r => r.Postare.Valoare).Order());
        Assert.All(radacini, r => Assert.Equal(0m, r.Postare.Cantitate));
        Assert.All(radacini, r => Assert.Equal(Sens.Debit, r.Postare.Coordonate.Sens));
        Assert.All(cub.Operare(a).Where(f => !CorectieValorica.EsteValoric(f)), f => {
            Assert.Null(f.Postare.Atribuit);
            Assert.Equal(Contrapondere.Tert, f.Postare.Coordonate.Contrapondere);
        });
        Assert.Equal(600m, cub.Operare(cub.Bon).Single(f => f.Postare.Coordonate.Sens == Sens.Debit).Postare.Valoare);
    }

    [Theory]
    [InlineData(0, 200, 0)]
    [InlineData(100, 0, 200)]
    public void LotulIntactIaTotPeStocIarCelEpuizatTotPeConsum(int consumat, int peStoc, int peConsum) {
        var cub = new CubMic();
        cub.Receptioneaza(100m, 1000m, Z05);
        if (consumat > 0) cub.Consuma(consumat, Z10);
        cub.Corecteaza(200m, Z15);
        var ramas = 100m - consumat;
        Assert.Equal((ramas, ramas * 10m + peStoc, consumat * 10m + peConsum, 1200m), cub.Stare());
    }

    [Fact]
    public void StornoulBonuluiMutaAlocareaActivaPeStocIarCelAlCorectieiInverseazaSiMutarea() {
        var cub = Partial();
        var a = cub.Corecteaza(200m, Z15);
        cub.Storneaza(cub.Bon, Z20);
        Assert.Equal((100m, 1200m, 0m, 1200m), cub.Stare());
        Assert.Equal(2, cub.Tranzactii(a, FelTranzactie.Operare));
        Assert.All(cub.Fapte.Where(f => f.Postare.Cauza.Document == cub.Bon), f => Assert.NotEqual(a, cub.Proprietar(f)));
        Assert.Equal(0m, cub.Fapte.Where(f => f.Postare.Cauza.Document == cub.Bon && f.Postare.Coordonate.Cont == CubMic.Stoc)
            .Sum(CorectieValorica.Masura));
        Assert.Equal(0m, cub.Fapte.Where(f => f.Postare.Cauza.Document == cub.Bon && f.Postare.Coordonate.Cont == CubMic.Consum)
            .Sum(CorectieValorica.Masura));

        cub.Storneaza(a, Z25);
        Assert.Equal((100m, 1000m, 0m, 1000m), cub.Stare());
        Assert.All(cub.Fapte, f => Assert.Equal(cub.Proprietar(f), f.Postare.Cauza.Document));
        Assert.Null(cub.Audit());
    }

    [Fact]
    public void CorectiaStornataInainteNuMaiCompenseazaNimic() {
        var cub = Partial();
        var a = cub.Corecteaza(200m, Z15);
        cub.Storneaza(a, Z20);
        Assert.Equal((40m, 400m, 600m, 1000m), cub.Stare());
        cub.Storneaza(cub.Bon, Z20);
        Assert.Equal((100m, 1000m, 0m, 1000m), cub.Stare());
        Assert.Equal(1, cub.Tranzactii(a, FelTranzactie.Operare));
    }

    [Fact]
    public void DiminuareaPastreazaSensurileCuValoriNegative() {
        var cub = Partial();
        var a = cub.Corecteaza(-200m, Z15);
        Assert.Equal((40m, 320m, 480m, 800m), cub.Stare());
        Assert.All(cub.Operare(a).Where(CorectieValorica.EsteValoric), r => Assert.Equal(Sens.Debit, r.Postare.Coordonate.Sens));
        cub.Storneaza(cub.Bon, Z20);
        Assert.Equal((100m, 800m, 0m, 800m), cub.Stare());
        cub.Storneaza(a, Z25);
        Assert.Equal((100m, 1000m, 0m, 1000m), cub.Stare());
    }

    public static TheoryData<int, int, string> Ordini() {
        var date = new TheoryData<int, int, string>();
        foreach (var (prima, a_doua) in new[] { (200, 100), (200, -100) })
            foreach (var ordine in new[] { "BAC", "BCA", "ABC", "ACB", "CBA", "CAB" })
                date.Add(prima, a_doua, ordine);
        foreach (var una in new[] { 200, -200 })
            foreach (var ordine in new[] { "BA", "AB" })
                date.Add(una, 0, ordine);
        return date;
    }

    [Theory]
    [MemberData(nameof(Ordini))]
    public void OriceOrdineAStornarilorRevineLaLotulInitial(int prima, int aDoua, string ordine) {
        var cub = Partial();
        var corectii = new Dictionary<char, Guid> { ['A'] = cub.Corecteaza(prima, Z15) };
        if (aDoua != 0) corectii['C'] = cub.Corecteaza(aDoua, Z15);
        var active = new Dictionary<char, decimal> { ['A'] = prima, ['C'] = aDoua };
        var bonActiv = true;
        foreach (var pas in ordine) {
            if (pas == 'B') { cub.Storneaza(cub.Bon, Z20); bonActiv = false; }
            else { cub.Storneaza(corectii[pas], Z20); active[pas] = 0m; }
            var delta = active.Values.Sum();
            var asteptat = bonActiv
                ? (40m, 400m + delta * 0.4m, 600m + delta * 0.6m, 1000m + delta)
                : (100m, 1000m + delta, 0m, 1000m + delta);
            Assert.Equal(asteptat, cub.Stare());
        }
        Assert.Equal((100m, 1000m, 0m, 1000m), cub.Stare());
        Assert.All(cub.Fapte, f => Assert.Equal(cub.Proprietar(f), f.Postare.Cauza.Document));
        Assert.Null(cub.Audit());
    }

    [Fact]
    public void DouaCorectiiAuCompensarileInTranzactiiDistincte() {
        var cub = Partial();
        var a = cub.Corecteaza(200m, Z15);
        var c = cub.Corecteaza(100m, Z15);
        Assert.Equal((40m, 520m, 780m, 1300m), cub.Stare());
        cub.Storneaza(cub.Bon, Z20);
        Assert.Equal((100m, 1300m, 0m, 1300m), cub.Stare());
        Assert.Equal(2, cub.Tranzactii(a, FelTranzactie.Operare));
        Assert.Equal(2, cub.Tranzactii(c, FelTranzactie.Operare));
        Assert.Equal([120m, 120m], cub.Operare(a).Where(f => f.Postare.Coordonate.Data == Z20).Select(f => -f.Postare.Valoare));
        Assert.Equal([60m, 60m], cub.Operare(c).Where(f => f.Postare.Coordonate.Data == Z20).Select(f => -f.Postare.Valoare));
        Assert.Null(cub.Audit());
    }

    [Fact]
    public void HamiltonPePozitiiApoiPeBeneficiariCuCotaZeroFaraPostare() {
        var cub = new CubMic();
        cub.Receptioneaza(3m, 1m, Z05);
        cub.Consuma(1m, Z10);
        cub.Consuma(1m, Z11);
        Assert.Equal((1m, 0.33m, 0.67m, 1m), cub.Stare());

        var alocari = CorectieValorica.Aloca(CorectieValorica.Beneficiari(cub.Reper, cub.Fapte, Z15), 0.02m);
        Assert.Equal([(true, 1m, 0.01m), (false, 1m, 0.01m), (false, 1m, 0m)],
            alocari.Select(a => (a.Beneficiar.Stoc, a.Beneficiar.Pondere, a.Valoare)));
        Assert.Equal(Z10, alocari[1].Beneficiar.Fapt.Postare.Coordonate.Data);

        var a = cub.Corecteaza(0.02m, Z15);
        Assert.Equal((1m, 0.34m, 0.68m, 1.02m), cub.Stare());
        Assert.Equal(2, cub.Operare(a).Count(CorectieValorica.EsteValoric));
        Assert.Null(cub.Audit());
        cub.Consuma(1m, Z18);
        Assert.Equal((0m, 0m, 1.02m, 1.02m), cub.Stare());
    }

    [Fact]
    public void IesireaNouaSeEvalueazaDinSoldulAjustat() {
        var cub = Partial();
        cub.Corecteaza(200m, Z15);
        cub.Consuma(10m, Z18);
        Assert.Equal((30m, 360m, 840m, 1200m), cub.Stare());
        cub.Consuma(30m, Z18);
        Assert.Equal((0m, 0m, 1200m, 1200m), cub.Stare());
    }

    [Fact]
    public void DiminuareaMergePanaLaZeroNuSubZero() {
        var cub = Partial();
        Assert.Equal(MotivCorectieValorica.ValoareInsuficienta, Refuz(() => cub.Corecteaza(-1000.01m, Z15)));
        Assert.Equal((40m, 400m, 600m, 1000m), cub.Stare());
        cub.Corecteaza(-1000m, Z15);
        Assert.Equal((40m, 0m, 0m, 0m), cub.Stare());
    }

    [Fact]
    public void StornareaCorectieiCareArLasaValoareNegativaEsteRefuzata() {
        var cub = new CubMic();
        cub.Receptioneaza(100m, 1000m, Z05);
        var a = cub.Corecteaza(200m, Z15);
        cub.Corecteaza(-1100m, Z15);
        Assert.Equal(MotivCorectieValorica.ValoareInsuficienta, Refuz(() => cub.Storneaza(a, Z20)));
        Assert.Equal(MotivCorectieValorica.Dependenti, Refuz(() => cub.Retrage(a)));
    }

    [Fact]
    public void GardulTemporalPrecedeSuma() {
        var cub = Partial();
        var a = cub.Corecteaza(200m, Z15);
        Assert.Equal(MotivCorectieValorica.Cronologie, Refuz(() => cub.Storneaza(cub.Bon, Z14)));
        cub.Storneaza(a, Z25);
        Assert.Equal(MotivCorectieValorica.Cronologie, Refuz(() => cub.Storneaza(cub.Bon, Z20)));
        Assert.Equal((40m, 400m, 600m, 1000m), cub.Stare());
        cub.Storneaza(cub.Bon, Z25);
        Assert.Equal((100m, 1000m, 0m, 1000m), cub.Stare());

        var oglinda = Partial();
        var b = oglinda.Corecteaza(200m, Z15);
        oglinda.Storneaza(oglinda.Bon, Z25);
        Assert.Equal(MotivCorectieValorica.Cronologie, Refuz(() => oglinda.Storneaza(b, Z20)));
        Assert.Equal((100m, 1200m, 0m, 1200m), oglinda.Stare());
    }

    [Fact]
    public void CorectiaNuSeAntedateazaFataDeMiscarileLotului() {
        var cub = Partial();
        Assert.Equal(MotivCorectieValorica.Cronologie, Refuz(() => cub.Corecteaza(200m, Z05.AddDays(1))));
        Assert.Equal(MotivCorectieValorica.Cronologie, Refuz(() => cub.Corecteaza(200m, Z05.AddDays(-1))));
    }

    [Fact]
    public void StornareaReceptieiCereCorectiileNeutralizate() {
        var cub = new CubMic();
        cub.Receptioneaza(100m, 1000m, Z05);
        var a = cub.Corecteaza(200m, Z15);
        Assert.Equal(MotivCorectieValorica.Dependenti, Refuz(() => cub.Storneaza(cub.Receptie, Z20)));
        cub.Storneaza(a, Z20);
        Assert.Equal(MotivCorectieValorica.Cronologie, Refuz(() => cub.Storneaza(cub.Receptie, Z18)));
        var inainte = cub.Fapte.Count;
        cub.Storneaza(cub.Receptie, Z25);
        Assert.Equal(inainte + 2, cub.Fapte.Count);
        Assert.Equal((0m, 0m, 0m, 0m), cub.Stare());

        var relocat = Partial();
        relocat.Corecteaza(200m, Z15);
        relocat.Storneaza(relocat.Bon, Z20);
        Assert.Equal(MotivCorectieValorica.Dependenti, Refuz(() => relocat.Storneaza(relocat.Receptie, Z25)));
    }

    [Fact]
    public void BeneficiariiUneiCorectiiNuSeMaiRetragNiciDupaStornareaEi() {
        var cub = Partial();
        var a = cub.Corecteaza(200m, Z15);
        Assert.Equal(MotivCorectieValorica.Dependenti, Refuz(() => cub.Retrage(cub.Bon)));
        Assert.Equal(MotivCorectieValorica.Dependenti, Refuz(() => cub.Retrage(cub.Receptie)));
        cub.Storneaza(a, Z20);
        Assert.Equal(MotivCorectieValorica.Dependenti, Refuz(() => cub.Retrage(cub.Bon)));
        cub.Storneaza(cub.Bon, Z20);
        Assert.Equal((100m, 1000m, 0m, 1000m), cub.Stare());
    }

    [Fact]
    public void RetragereaCorectieiFaraDependentiNuLasaNimic() {
        var cub = Partial();
        var inainte = cub.Fapte.Count;
        var a = cub.Corecteaza(200m, Z15);
        cub.Retrage(a);
        Assert.Equal(inainte, cub.Fapte.Count);
        Assert.Equal((40m, 400m, 600m, 1000m), cub.Stare());

        a = cub.Corecteaza(200m, Z15);
        cub.Storneaza(cub.Bon, Z20);
        Assert.Equal(MotivCorectieValorica.Dependenti, Refuz(() => cub.Retrage(a)));
    }

    [Fact]
    public void ReturulUlteriorPeLotOpresteInversareaCorectieiSiSeStorneazaPrimul() {
        var cub = Partial();
        var a = cub.Corecteaza(200m, Z15);
        var retur = cub.Readuce(10m, 120m, Z18);
        Assert.Equal((50m, 600m, 720m, 1200m), cub.Stare());
        Assert.Equal(MotivCorectieValorica.Dependenti, Refuz(() => cub.Storneaza(a, Z20)));
        Assert.Equal(MotivCorectieValorica.Dependenti, Refuz(() => cub.Retrage(a)));
        cub.Storneaza(retur, Z20);
        Assert.Equal((40m, 480m, 720m, 1200m), cub.Stare());
        cub.Storneaza(a, Z20);
        Assert.Equal((40m, 400m, 600m, 1000m), cub.Stare());
    }

    [Fact]
    public void IesireaUlterioaraActivaOpresteInversareaCorectiei() {
        var cub = Partial();
        var a = cub.Corecteaza(200m, Z15);
        var nou = cub.Consuma(20m, Z18);
        Assert.Equal((20m, 240m, 960m, 1200m), cub.Stare());
        Assert.Equal(MotivCorectieValorica.Dependenti, Refuz(() => cub.Storneaza(a, Z20)));
        Assert.Equal(MotivCorectieValorica.Dependenti, Refuz(() => cub.Retrage(a)));
        cub.Storneaza(nou, Z20);
        Assert.Equal((40m, 480m, 720m, 1200m), cub.Stare());
        Assert.Equal(1, cub.Tranzactii(a, FelTranzactie.Operare));
        cub.Storneaza(a, Z20);
        Assert.Equal((40m, 400m, 600m, 1000m), cub.Stare());
    }

    [Fact]
    public void ConsumulDinAnulAnteriorEsteRefuzatDarStoculRamasNu() {
        var decembrie = new DateOnly(2025, 12, 20);
        var cub = new CubMic();
        cub.Receptioneaza(100m, 1000m, decembrie);
        cub.Corecteaza(200m, Z15);
        Assert.Equal((100m, 1200m, 0m, 1200m), cub.Stare());

        var consumat = new CubMic();
        consumat.Receptioneaza(100m, 1000m, decembrie);
        consumat.Consuma(60m, decembrie);
        Assert.Equal(MotivCorectieValorica.AnInchis, Refuz(() => consumat.Corecteaza(200m, Z15)));
    }

    [Fact]
    public void LotulCuAltaFormaDeMiscareNuEsteCorectabil() {
        var transferat = Partial();
        transferat.Transfera(10m, Z11);
        Assert.Equal(MotivCorectieValorica.LotNeeligibil, Refuz(() => transferat.Corecteaza(200m, Z15)));

        var vandut = Partial();
        vandut.Vinde(10m, Z11);
        Assert.Equal(MotivCorectieValorica.LotNeeligibil, Refuz(() => vandut.Corecteaza(200m, Z15)));

        var altFurnizor = Partial();
        Assert.Equal(MotivCorectieValorica.LotNeeligibil, Refuz(() => altFurnizor.Corecteaza(200m, Z15, altFurnizor.Reper with { Furnizor = Gen.Terti[1].Id })));

        var stornata = new CubMic();
        stornata.Receptioneaza(100m, 1000m, Z05);
        stornata.Storneaza(stornata.Receptie, Z10);
        Assert.Equal(MotivCorectieValorica.LotNeeligibil, Refuz(() => stornata.Corecteaza(200m, Z15)));
    }

    [Fact]
    public void ConsumulStornatNuRamanePondere() {
        var cub = Partial();
        cub.Storneaza(cub.Bon, Z11);
        var beneficiari = CorectieValorica.Beneficiari(cub.Reper, cub.Fapte, Z15);
        Assert.Equal([(true, 100m, 1000m)], beneficiari.Select(b => (b.Stoc, b.Pondere, b.ValoareActiva)));
    }

    [Fact]
    public void CorectiaUlterioaraOpresteRetragereaCeleiDinainte() {
        var cub = Partial();
        var a = cub.Corecteaza(200m, Z15);
        var b = cub.Corecteaza(100m, Z18);
        Assert.Equal(MotivCorectieValorica.Dependenti, Refuz(() => cub.Retrage(a)));
        cub.Storneaza(b, Z20);
        Assert.Equal(MotivCorectieValorica.Dependenti, Refuz(() => cub.Retrage(a)));
        cub.Storneaza(a, Z20);
        Assert.Equal((40m, 400m, 600m, 1000m), cub.Stare());

        var retras = Partial();
        var c = retras.Corecteaza(200m, Z15);
        retras.Retrage(retras.Corecteaza(100m, Z18));
        retras.Retrage(c);
        Assert.Equal((40m, 400m, 600m, 1000m), retras.Stare());
    }

    [Fact]
    public void ValoareaPeStoculFaraCantitateNuSeCorecteaza() {
        var cub = new CubMic();
        cub.Receptioneaza(100m, 1000m, Z05);
        cub.Corecteaza(200m, Z10);
        cub.Consuma(100m, Z11);
        Assert.Equal((0m, 0m, 1200m, 1200m), cub.Stare());
        cub.Muta(f => Radacina(f, CubMic.Stoc, Z10), f => f with { Postare = f.Postare with { Valoare = f.Postare.Valoare + 0.01m } });
        Assert.Equal(MotivCorectieValorica.LotNeeligibil, Refuz(() => cub.Corecteaza(50m, Z15)));
    }

    static CubMic Compensat(out Guid corectie) {
        var cub = Partial();
        corectie = cub.Corecteaza(200m, Z15);
        cub.Storneaza(cub.Bon, Z20);
        Assert.Null(cub.Audit());
        return cub;
    }

    static bool Radacina(FaptPostat f, Guid cont, DateOnly? data = null) => CorectieValorica.EsteValoric(f)
        && f.Fel == FelTranzactie.Operare && f.Postare.Coordonate.Data == (data ?? Z15) && f.Postare.Coordonate.Cont == cont;

    public static TheoryData<string, string> Mutanti() => new() {
        { "fără decizie", "nu are decizie" },
        { "decizie fără postare", "nu are rădăcina ei" },
        { "suma liniei", "suma alocărilor" },
        { "cote din ponderi", "cotele nu rezultă" },
        { "beneficiar greșit", "nu e o intrare" },
        { "rădăcină greșită", "nu e alocarea corecției" },
        { "după inversa rădăcinii", "după inversa ei" },
        { "proveniență străină", "postări absente" },
        { "ciclu", "nu e o intrare" },
        { "compensare duplicată", "de mai multe ori" },
        { "inversă duplicată", "inverse." },
        { "alocare spre alocare", "nu e o intrare" },
        { "ponderi altele decât cantitățile", "nu e cantitatea lui" },
        { "compensare sub beneficiar", "nu aparține corecției" },
    };

    [Theory]
    [MemberData(nameof(Mutanti))]
    public void AuditulRespingeMutantii(string mutant, string motiv) {
        var cub = Compensat(out var a);
        var peConsum = cub.Fapte.Single(f => Radacina(f, CubMic.Consum));
        var peStoc = cub.Fapte.Single(f => Radacina(f, CubMic.Stoc));
        var aleBonului = cub.Fapte.Where(f => f.Postare.Cauza.Document == cub.Bon && f.Fel == FelTranzactie.Operare).ToList();
        var bon = aleBonului.Single(f => f.Postare.Coordonate.Sens == Sens.Debit);
        var bonStoc = aleBonului.Single(f => f.Postare.Coordonate.Sens == Sens.Credit);
        var compensare = cub.Explicatii.Single(e => e.Decizii.OfType<CompensareValorica>().Any());
        void Atribuie(FaptPostat radacina, Guid tinta) =>
            cub.Muta(f => f.Id == radacina.Id, f => f with { Postare = f.Postare with { Atribuit = tinta } });
        void Beneficiar(Guid tinta) =>
            cub.MutaDecizii(d => d is AlocareValorica { Valoare: 120m } v ? v with { Beneficiar = new(tinta, Spatiu.Stoc) } : d);
        switch (mutant) {
            case "fără decizie":
                cub.Explicatii[0] = cub.Explicatii[0] with {
                    Decizii = [.. cub.Explicatii[0].Decizii.Where(d => d is not AlocareValorica { Valoare: 120m })] };
                break;
            case "decizie fără postare":
                cub.Explicatii[0] = cub.Explicatii[0] with { Decizii = [.. cub.Explicatii[0].Decizii,
                    new AlocareValorica(peConsum.Postare.Cauza.Linie!.Value, peStoc.Postare.Coordonate.Unitate!,
                        new(Guid.NewGuid(), Spatiu.Stoc), 1m, 5m)] };
                break;
            case "suma liniei":
                cub.ValoriLinii[peConsum.Postare.Cauza.Linie!.Value] += 1m;
                break;
            case "cote din ponderi":
                static decimal Cota(decimal v) => v == 80m ? 81m : v == 120m ? 119m : v;
                cub.MutaDecizii(d => d is AlocareValorica v ? v with { Valoare = Cota(v.Valoare) } : d);
                cub.Muta(f => f.Id == peStoc.Id || f.Id == peConsum.Id,
                    f => f with { Postare = f.Postare with { Valoare = Cota(f.Postare.Valoare) } });
                break;
            case "beneficiar greșit":
                Beneficiar(bonStoc.Id);
                Atribuie(peConsum, bonStoc.Id);
                break;
            case "rădăcină greșită":
                cub.MutaDecizii(d => d is CompensareValorica c ? c with { Radacina = new(peStoc.Id, Spatiu.Stoc) } : d);
                cub.Muta(f => f.Postare.Atribuit == peConsum.Id, f => f with { Postare = f.Postare with { Atribuit = peStoc.Id } });
                break;
            case "după inversa rădăcinii":
                cub.Storneaza(a, Z20);
                Assert.Null(cub.Audit());
                var scrisa = cub.Fapte.First(f => f.Tranzactie == compensare.Tranzactie).ScrisLa;
                cub.Muta(f => f.Postare.InversaDin?.Id == peConsum.Id, f => f with { ScrisLa = scrisa.AddTicks(-1) });
                break;
            case "proveniență străină":
                Atribuie(peConsum, Guid.NewGuid());
                break;
            case "ciclu":
                var picior = cub.Fapte.First(f => f.Tranzactie == compensare.Tranzactie && CorectieValorica.EsteValoric(f));
                Beneficiar(picior.Id);
                Atribuie(peConsum, picior.Id);
                break;
            case "compensare duplicată":
                var copie = cub.Adauga(a, [.. cub.Fapte.Where(f => f.Tranzactie == compensare.Tranzactie)]);
                cub.Explicatii.Add(compensare with { Tranzactie = copie });
                break;
            case "inversă duplicată":
                cub.Adauga(cub.Bon, [.. cub.Fapte.Where(f => f.Postare.InversaDin?.Id == bon.Id)]);
                break;
            case "alocare spre alocare":
                Beneficiar(peStoc.Id);
                Atribuie(peConsum, peStoc.Id);
                break;
            case "ponderi altele decât cantitățile":
                cub.MutaDecizii(d => d is AlocareValorica v ? v with { Pondere = 50m } : d);
                break;
            case "compensare sub beneficiar":
                cub.Muta(f => f.Tranzactie == compensare.Tranzactie,
                    f => f with { Postare = f.Postare with { Cauza = bon.Postare.Cauza } });
                break;
        }
        Assert.Contains(motiv, cub.Audit() ?? "");
    }

    static CubMic Partial() {
        var cub = new CubMic();
        cub.Receptioneaza(100m, 1000m, Z05);
        cub.Consuma(60m, Z10);
        return cub;
    }

    static MotivCorectieValorica Refuz(Action comanda) => Assert.Throws<CorectieValoricaRefuzata>(comanda).Motiv;
}
