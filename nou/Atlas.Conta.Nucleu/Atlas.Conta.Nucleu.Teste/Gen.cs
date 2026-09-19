namespace Atlas.Conta.Nucleu.Teste;

public static class Gen {
    public const int Samanta = 20260920;

    public static readonly Guid[] Conturi = Pool(6, 1);
    public static readonly Guid[] Parteneri = Pool(4, 2);
    public static readonly Guid[] Produse = Pool(5, 3);
    public static readonly Guid[] Gestiuni = Pool(3, 4);
    public static readonly Guid[] Documente = Pool(3, 5);
    public static readonly Guid[] Linii = Pool(6, 6);
    public static readonly Guid[] Unitati = Pool(8, 7);

    static Guid[] Pool(int cate, int marca) {
        var pool = new Guid[cate];
        for (var i = 0; i < cate; i++)
            pool[i] = new Guid(marca, (short)i, 0, 0, 0, 0, 0, 0, 0, 0, 0);
        return pool;
    }

    public static Guid Unul(Random aleator, Guid[] pool) => pool[aleator.Next(pool.Length)];

    public static Guid AltulDecat(Random aleator, Guid[] pool, Guid exclus) {
        Guid ales;
        do {
            ales = Unul(aleator, pool);
        } while (ales == exclus);
        return ales;
    }

    public static decimal Pas(int scara) => new(1, 0, 0, false, (byte)scara);

    public static decimal Zecimal(Random aleator, decimal min, decimal max, int scara) {
        var pas = Pas(scara);
        var jos = (long)(min / pas);
        var sus = (long)(max / pas);
        return (jos + aleator.NextInt64(sus - jos + 1)) * pas;
    }

    public static DateOnly Data(Random aleator) => new DateOnly(2026, 1, 1).AddDays(aleator.Next(365));

    public static Unitate Lot(Random aleator, Guid cont, Guid produs) =>
        new(Unul(aleator, Unitati), FelUnitate.Lot, cont, null, produs, Data(aleator));

    public static Unitate Partida(Random aleator, Guid cont, Guid partener) =>
        new(Unul(aleator, Unitati), FelUnitate.Partida, cont, partener, null, Data(aleator));

    public static Unitate Fisa(Random aleator, Guid cont) =>
        new(Unul(aleator, Unitati), FelUnitate.Fisa, cont, null, null, Data(aleator));

    public static Capat CapatContabil(Random aleator, Guid? peCont = null) {
        var cont = peCont ?? Unul(aleator, Conturi);
        var partener = aleator.Next(2) == 0 ? Unul(aleator, Parteneri) : (Guid?)null;
        Unitate? unitate = aleator.Next(3) switch {
            0 => Fisa(aleator, cont),
            1 when partener is { } p => Partida(aleator, cont, p),
            _ => null,
        };
        return new Capat {
            Cont = cont,
            Partener = partener,
            Unitate = unitate,
            PerioadaDeclarare = aleator.Next(2) == 0 ? 202601 + aleator.Next(12) : null,
        };
    }

    public static Capat CapatCuCantitate(Random aleator, Guid produs, FelUnitate fel, Guid? peCont = null) {
        var cont = peCont ?? Unul(aleator, Conturi);
        var partener = Unul(aleator, Parteneri);
        return new Capat {
            Cont = cont,
            Partener = partener,
            Gestiune = Unul(aleator, Gestiuni),
            Produs = produs,
            Unitate = fel == FelUnitate.Lot ? Lot(aleator, cont, produs) : Partida(aleator, cont, partener),
        };
    }

    public static Miscare Miscare(Random aleator, Guid document, bool cuCantitate) {
        var cauza = new Cauza(document, Unul(aleator, Linii));
        var valoare = Zecimal(aleator, 0.01m, 9999.99m, Scara.Bani);
        var valoareValuta = Zecimal(aleator, 0m, 9999.99m, Scara.Bani);
        if (!cuCantitate)
            return new Miscare(CapatContabil(aleator), CapatContabil(aleator), 0m, valoareValuta, valoare, cauza);
        var produs = Unul(aleator, Produse);
        // N-D4: intrarea stă pe lot (gestiune reală), ieșirea pe partida terțului (gestiune virtuală).
        return new Miscare(
            CapatCuCantitate(aleator, produs, FelUnitate.Partida),
            CapatCuCantitate(aleator, produs, FelUnitate.Lot),
            Zecimal(aleator, 0.001m, 999.999m, Scara.Cantitate),
            valoareValuta,
            valoare,
            cauza);
    }

    public static Tranzactie Operare(
        Random aleator,
        int? cateMiscari = null,
        bool? cuCantitate = null,
        Guid? peDocument = null) {
        var document = peDocument ?? Unul(aleator, Documente);
        var data = Data(aleator);
        var postari = new List<Postare>();
        var cate = cateMiscari ?? 1 + aleator.Next(4);
        for (var i = 0; i < cate; i++) {
            var miscare = Miscare(aleator, document, cuCantitate ?? aleator.Next(2) == 0);
            var (debit, credit) = Nucleu.Miscare.Postari(miscare, data);
            postari.Add(debit);
            postari.Add(credit);
        }
        return new Tranzactie(FelTranzactie.Operare, data, document, postari);
    }

    public static Tranzactie Transfer(Random aleator, Guid? peDocument = null) {
        var document = peDocument ?? Unul(aleator, Documente);
        var data = Data(aleator);
        var postari = new List<Postare>();
        var perechi = 1 + aleator.Next(3);
        for (var i = 0; i < perechi; i++) {
            var cont = Unul(aleator, Conturi);
            var latura = aleator.Next(2) == 0 ? Latura.Debit : Latura.Credit;
            var produs = Unul(aleator, Produse);
            var cauza = new Cauza(document, Unul(aleator, Linii));
            var valoare = Zecimal(aleator, 0.01m, 9999.99m, Scara.Bani);
            var cantitate = aleator.Next(2) == 0 ? Zecimal(aleator, 0.001m, 999.999m, Scara.Cantitate) : 0m;
            postari.Add(DeTransfer(aleator, cont, latura, data, produs, cantitate, valoare, cauza));
            postari.Add(DeTransfer(aleator, cont, latura, data, produs, -cantitate, -valoare, cauza));
        }
        return new Tranzactie(FelTranzactie.Transfer, data, document, postari);
    }

    public static Tranzactie Deschidere(Random aleator, Guid? peDocument = null) {
        var data = Data(aleator);
        var document = peDocument ?? Unul(aleator, Documente);
        var postari = new List<Postare>();
        var cate = 1 + aleator.Next(4);
        for (var i = 0; i < cate; i++) {
            var latura = aleator.Next(2) == 0 ? Latura.Debit : Latura.Credit;
            postari.Add(new Postare(
                CapatContabil(aleator).Pe(latura, data),
                0m,
                0m,
                Zecimal(aleator, 0.01m, 9999.99m, Scara.Bani),
                new Cauza(document, Unul(aleator, Linii))));
        }
        return new Tranzactie(FelTranzactie.Deschidere, data, null, postari);
    }

    public static Declaratie Declaratie(Random aleator, bool? cuCantitate = null, Guid? peDocument = null) {
        var document = peDocument ?? Unul(aleator, Documente);
        var miscari = new List<Miscare>();
        var cate = 1 + aleator.Next(4);
        for (var i = 0; i < cate; i++)
            miscari.Add(Miscare(aleator, document, cuCantitate ?? aleator.Next(2) == 0));
        return new Nucleu.Declaratie(document, Data(aleator), miscari, Decizii(aleator), Ipoteze(aleator));
    }

    public static IReadOnlyList<Decizie> Decizii(Random aleator) {
        var decizii = new List<Decizie>();
        var cate = aleator.Next(4);
        for (var i = 0; i < cate; i++) {
            var linie = Unul(aleator, Linii);
            var cont = Unul(aleator, Conturi);
            var produs = Unul(aleator, Produse);
            decizii.Add(aleator.Next(4) switch {
                0 => new AlocareFifo(
                    linie,
                    Lot(aleator, cont, produs),
                    Zecimal(aleator, 0.001m, 999.999m, Scara.Cantitate)),
                1 => new ValoareIesire(
                    linie,
                    Lot(aleator, cont, produs),
                    Zecimal(aleator, 0.001m, 999.999m, Scara.Cantitate),
                    Zecimal(aleator, 0.01m, 9999.99m, Scara.Bani)),
                2 => new PartidaDeschisa(linie, Partida(aleator, cont, Unul(aleator, Parteneri))),
                _ => new ContRezolvat(linie, cont, "politica"),
            });
        }
        return decizii;
    }

    public static IReadOnlyList<Ipoteza> Ipoteze(Random aleator) {
        var ipoteze = new List<Ipoteza>();
        var cate = aleator.Next(3);
        for (var i = 0; i < cate; i++)
            ipoteze.Add(aleator.Next(3) switch {
                0 => new SoldUnitateCitit(
                    Lot(aleator, Unul(aleator, Conturi), Unul(aleator, Produse)),
                    new Sold(
                        Zecimal(aleator, 0m, 9999.99m, Scara.Bani),
                        Zecimal(aleator, 0m, 9999.99m, Scara.Bani),
                        Zecimal(aleator, 0m, 999.999m, Scara.Cantitate),
                        0m)),
                1 => new PerioadaDeschisa(2026, 1 + aleator.Next(12)),
                _ => new VersiunePolitica("politica", Data(aleator)),
            });
        return ipoteze;
    }

    public static IReadOnlyList<Mutare> Mutari(Random aleator, Guid document, bool? cuCantitate = null) {
        var mutari = new List<Mutare>();
        var cate = 1 + aleator.Next(3);
        for (var i = 0; i < cate; i++) {
            var cont = Unul(aleator, Conturi);
            var produs = Unul(aleator, Produse);
            var cuMasura = cuCantitate ?? aleator.Next(2) == 0;
            mutari.Add(new Mutare(
                CapatDeMutare(aleator, cont, produs, cuMasura),
                CapatDeMutare(aleator, cont, produs, cuMasura),
                aleator.Next(2) == 0 ? Latura.Debit : Latura.Credit,
                cuMasura ? Zecimal(aleator, 0.001m, 999.999m, Scara.Cantitate) : 0m,
                0m,
                Zecimal(aleator, 0.01m, 9999.99m, Scara.Bani),
                new Cauza(document, Unul(aleator, Linii))));
        }
        return mutari;
    }

    static Capat CapatDeMutare(Random aleator, Guid cont, Guid produs, bool cuCantitate) =>
        cuCantitate
            ? CapatCuCantitate(aleator, produs, FelUnitate.Lot, cont)
            : CapatContabil(aleator, cont);

    static Postare DeTransfer(
        Random aleator,
        Guid cont,
        Latura latura,
        DateOnly data,
        Guid produs,
        decimal cantitate,
        decimal valoare,
        Cauza cauza) {
        var capat = cantitate != 0m
            ? CapatCuCantitate(aleator, produs, FelUnitate.Lot, cont)
            : CapatContabil(aleator, cont);
        return new Postare(capat.Pe(latura, data), cantitate, 0m, valoare, cauza);
    }
}
