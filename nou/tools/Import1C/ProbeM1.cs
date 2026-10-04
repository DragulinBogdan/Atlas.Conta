using Atlas.Conta.BackOffice.Module.Cub;
using Atlas.Conta.BackOffice.Module.Motor;
using N = Atlas.Conta.Nucleu;

namespace Import1C;

// Probele fără bază ale verificărilor M1 (review advers M1-R1, R3, R4); rulează la fiecare pornire.
static class ProbeM1 {
    public static void Ruleaza(Action<string, bool> check) {
        var cont = Guid.NewGuid();
        var partener = Guid.NewGuid();
        Guid a = Guid.NewGuid(), b = Guid.NewGuid(), c = Guid.NewGuid();
        Deschidere.Detaliu P(Guid unitate, decimal valoare) =>
            new(N.FelUnitate.Partida, unitate, null, cont, N.Latura.Credit, valoare, 0m, partener);
        Deschidere.Detaliu[] cub = [P(a, 60m), P(b, 40m)];
        check("probă M1-R1: reluarea identică n-are diferențe de detaliu",
            Deschidere.DiferenteDetaliu([P(a, 60.00m), P(b, 40.00m)], cub).Count == 0);
        check("probă M1-R1: redistribuirea 60/40 → 61/39, cu total și număr constante, e detectată pe ambele partide",
            Deschidere.DiferenteDetaliu([P(a, 61m), P(b, 39m)], cub).Count == 2);
        check("probă M1-R1: referința înlocuită, cu total și număr constante, e detectată în ambele sensuri",
            Deschidere.DiferenteDetaliu([P(a, 60m), P(c, 40m)], cub).Count == 2);
        check("probă M1-R1: partida mutată pe altă latură, cu aceeași valoare, e detectată",
            Deschidere.DiferenteDetaliu([P(a, 60m), P(b, 40m) with { Latura = N.Latura.Debit }], cub).Count == 1);

        var Refuz = ReconciliereLuna.ExplicatiePartida.Refuz;
        var Fara = ReconciliereLuna.ExplicatiePartida.Fara;
        check("probă M1-R3: stingerea normală refuzată (creditoare 100, debit 100 neaplicat) e explicată",
            ReconciliereLuna.Explica(-100m, 100m, 0m) == Refuz);
        check("probă M1-R3: mișcarea în sensul soldului refuzată (cub −201,67, sursă −447,67, credit 246) e explicată",
            ReconciliereLuna.Explica(-201.67m - -447.67m, -246m, 0m) == Refuz);
        check("probă M1-R3: aceeași diferență cu sensul stingerii normale NU e explicată",
            ReconciliereLuna.Explica(-201.67m - -447.67m, 246m, 0m) == Fara);
        check("probă M1-R3: plafonarea (creditoare 2.463,86, debit 2.464,75) e explicată",
            ReconciliereLuna.Explica(0m - 0.89m, 0m, 0.89m) == ReconciliereLuna.ExplicatiePartida.Plafonare);
        check("probă M1-R3: refuzul și plafonarea de pe aceeași partidă se cumulează (debitoare 100, 110 plafonat, 20 refuzat)",
            ReconciliereLuna.Explica(0m - -30m, -20m, -10m) == ReconciliereLuna.ExplicatiePartida.RefuzSiPlafonare
                && ReconciliereLuna.Explica(30m, -20m, 0m) == Fara && ReconciliereLuna.Explica(30m, 0m, -10m) == Fara);
        check("probă M1-R3: diferența fără nicio mișcare neaplicată nu e explicată",
            ReconciliereLuna.Explica(5m, 0m, 0m) == Fara);

        var refA = new FlaxRef("00000075", "Vanzare", "A", null);
        var refB = new FlaxRef("00000038", "Aprovizionare", "B", null);
        bool Rand(decimal suma, int sensDebit) {
            var s = HandlerCompensare.StingeriRand("nota", refA, refB, suma).ToList();
            return s.Count == 2 && s[0].Tinta == refA && s[0].Sens == sensDebit && s[0].Suma == Math.Abs(suma)
                && s[1].Tinta == refB && s[1].Sens == -sensDebit && s[1].Suma == Math.Abs(suma);
        }
        check("probă M1-R3a: rândul de compensare cu referințe distincte pe ambele laturi dă două stingeri de sens opus, "
            + "iar suma negativă le întoarce", Rand(100m, 1) && Rand(-100m, -1));
        (decimal Suma, int? Sens) Net(params (decimal Suma, int? Sens)[] randuri) {
            var p = Imperecheri1C.Agrega(randuri.Select(r => new StingereSursa("Compensare", "nota", refA, r.Suma, r.Sens)));
            return p.Count == 1 ? (p[0].Suma, p[0].Sens) : (-1m, null);
        }
        check("probă M1-R3a: +100 și −40 pe aceeași poziție se agregă la +60", Net((100m, 1), (40m, -1)) == (60m, 1));
        check("probă M1-R3a: −100 și +40 pe aceeași poziție se agregă la −60", Net((100m, -1), (40m, 1)) == (60m, -1));
        check("probă M1-R3a: +100 și −100 dau net zero (sumă zero, sărită), nu sens necunoscut",
            Net((100m, 1), (100m, -1)) == (0m, 0));
        check("probă M1-R3a: sursa fără sens (trezoreria) rămâne cu suma adunată și sens necunoscut",
            Net((100m, null), (40m, null)) == (140m, null));
        var peLaturi = Imperecheri1C.Agrega(HandlerCompensare.StingeriRand("nota", refA, refB, 100m)
            .Concat(HandlerCompensare.StingeriRand("nota", refA, refB, -40m)));
        check("probă M1-R3a: două rânduri cu referințe distincte pe laturi dau două perechi, +60 și −60",
            peLaturi.Count == 2 && peLaturi.Any(p => p.Key.TintaId == "A" && p.Suma == 60m && p.Sens == 1)
                && peLaturi.Any(p => p.Key.TintaId == "B" && p.Suma == 60m && p.Sens == -1));

        var nominal = new OperareException($"{Materializare.StingereDeschidereInvalida}: Cont/partener incompatibil "
            + "sau suma depășește restul disponibil.");
        var ocupata = new OperareException($"{TranzactieComanda.ScriereOcupata}: altă comandă scrie în bază; reîncercați.");
        var altRefuz = new OperareException($"{Materializare.StingereDeschidereInvalida}: Partidă, stare, sumă sau dată invalidă.");
        check("probă M1-R4: numai refuzul nominalizat al motorului explică o stingere neaplicată",
            Imperecheri1C.EsteRefuzDeBusiness(nominal) && !Imperecheri1C.EsteRefuzDeBusiness(altRefuz)
                && !Imperecheri1C.EsteRefuzDeBusiness(ocupata) && !Imperecheri1C.EsteRefuzDeBusiness(new TimeoutException())
                && !Imperecheri1C.EsteRefuzDeBusiness(new InvalidOperationException()));
        check("probă M1-R4: blocajul ocupat și excepțiile neașteptate sunt erori tehnice pe calea documentelor",
            Imperecheri1C.EsteEroareTehnica(ocupata) && Imperecheri1C.EsteEroareTehnica(new TimeoutException())
                && !Imperecheri1C.EsteEroareTehnica(new OperareException("Suma depășește restul nestins.")));
    }
}
