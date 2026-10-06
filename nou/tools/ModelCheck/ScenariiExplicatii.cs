using Atlas.Conta.BackOffice.Module.Api;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Declaratii;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.EFCore;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using C = Atlas.Conta.BackOffice.Module.Cub;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.ModelCheck;

// X-D4: explicația deciziei, persistată pe prima tranzacție a contractului (SC-CIT-96, S-r11).
sealed class ScenariiExplicatii(Func<IObjectSpace> deschide, Action<string, bool> check,
    bool privat, Action<IObjectSpace, int, int> inchide)
    : ScenaDocumente(deschide, check, privat, inchide, "EXPL", 2011) {
    FacturaScena Bon(params (LinieScena Lot, decimal Cantitate)[] linii) {
        using var os = Deschide();
        var d = os.CreateObject<BonConsum>(); d.Data = new(An, 1, 10); d.PredatorId = Magazie; d.PrimitorId = Loc;
        var rezultat = new List<LinieScena>();
        foreach (var (lot, q) in linii) {
            var l = os.CreateObject<DocumentDetaliu>(); l.Document = d; l.Pozitie = rezultat.Count + 1;
            l.TipMaterialId = Tip(os, Stoc); l.LotId = lot.Lot; l.Cantitate = q;
            rezultat.Add(new(l.ID, lot.Lot, lot.Produs));
        }
        os.CommitChanges(); return new(d.ID, rezultat.ToArray());
    }

    static bool Sold(N.Sold sold, decimal debit, decimal credit, decimal cantitate) =>
        sold is not null && sold.Debit == debit && sold.Credit == credit && sold.Cantitate == cantitate;

    protected override void Executa() {
        Codec();
        var f = Receptioneaza(new LinieFctScena(3, .333333m), new LinieFctScena(10, 12.5m));
        var (ieftin, scump) = (f.Linii[0], f.Linii[1]);
        var bon = Bon((ieftin, 1), (scump, 4), (ieftin, 1));
        Verifica("SC-CIT-96", "dry-run acceptat, fără nimic persistat",
            CuSpatiu(os => ComenziDocument.Sistem(os).Valideaza(bon.Id)).Count == 0);
        FaraEfecte("SC-CIT-96", bon.Id);
        Opereaza(bon.Id);
        var operare = Tranzactii(bon.Id).Single();
        var peBon = Explicatia(bon.Id, N.FelTranzactie.Operare);
        var e = peBon.Origini.Single().Explicatie;
        var linii = e.Linii();
        Verifica("SC-CIT-96", "BCS: o singură explicație, pe tranzacția Operare, cu declarantul și perioada ei",
            operare is { Fel: N.FelTranzactie.Operare, Explicata: true, Din: null }
            && peBon.Origini.Single().Purtator == operare.Id && e.Declarant == nameof(DeclarantBonConsum)
            && e.Ipoteze.OfType<N.PerioadaDeschisa>().Single() == new N.PerioadaDeschisa(An, 1)
            && e.Ipoteze.OfType<N.VersiunePolitica>().Count() == 1
            && e.Ipoteze.OfType<N.SoldUnitateCitit>().Count() == 2);
        Verifica("SC-CIT-96", "„de ce acest lot”: 1 din 3/1,00 = 0,33; 4 din 10/125 = 50; al doilea 1 din 2/0,67 = 0,34",
            linii.Select(l => l.Linie).SequenceEqual(bon.Linii.Select(l => l.Id))
            && linii.All(l => l.Iesiri.Count == 1 && l.Iesiri[0].Sursa == null && l.Conturi.Count == 2 && l.Stingeri.Count == 0)
            && linii[0].Iesiri[0] is { Cantitate: 1, Valoare: .33m } prima && prima.Unitate.Id == ieftin.Lot
            && Sold(prima.SoldInainte, 1, 0, 3)
            && linii[1].Iesiri[0] is { Cantitate: 4, Valoare: 50 } aDoua && aDoua.Unitate.Id == scump.Lot
            && Sold(aDoua.SoldInainte, 125, 0, 10)
            && linii[2].Iesiri[0] is { Cantitate: 1, Valoare: .34m } aTreia && aTreia.Unitate.Id == ieftin.Lot
            && Sold(aTreia.SoldInainte, 1, .33m, 2));
        SoldLot("SC-CIT-96", ieftin.Lot!.Value, Magazie, new(An, 1, 31), 1, .33m);

        var btr = Iesire(true, (scump, 2));
        Comanda(os => { os.GetObjectByKey<Document>(btr.Id).Data = new(An, 1, 12); os.CommitChanges(); });
        Opereaza(btr.Id);
        var transfer = Tranzactii(btr.Id).Single();
        var peBtr = Explicatia(btr.Id, N.FelTranzactie.Transfer).Origini.Single();
        var mutat = peBtr.Explicatie.Linii().Single().Iesiri.Single();
        Verifica("SC-CIT-96", "BTR: purtătorul e tranzacția Transfer; decizia +2/+25 explică sursa postată −2/−25",
            transfer is { Fel: N.FelTranzactie.Transfer, Explicata: true, Din: null } && peBtr.Purtator == transfer.Id
            && peBtr.Explicatie.Declarant == nameof(DeclarantNotaTransfer)
            && mutat is { Cantitate: 2, Valoare: 25 } && Sold(mutat.SoldInainte, 75, 0, 6)
            && CuSpatiu(os => os.GetObjectsQuery<C.Postare>().Count(p => p.TranzactieId == transfer.Id
                && p.Gestiune == Magazie && p.Cantitate == -2 && p.Valoare == -25 && p.Latura == N.Latura.Debit) == 1));

        var data = new DateOnly(An, 1, 20);
        Storneaza(btr.Id, data); Storneaza(bon.Id, data);
        var storno = Tranzactii(bon.Id).Single(t => t.Fel == N.FelTranzactie.Storno);
        var peStorno = Explicatia(bon.Id, N.FelTranzactie.Storno);
        Verifica("SC-CIT-96", "storno: fără explicație proprie; cititorul întoarce explicația originalului",
            storno is { Explicata: false, Din: null } && peStorno.Fel == N.FelTranzactie.Storno
            && peStorno.Origini.Single().Purtator == operare.Id && peStorno.Origini.Single().Explicatie == e);
        Verifica("SC-CIT-96", "baza refuză explicația pe o tranzacție Storno (CK_Tranzactie_Explicatie)", CuSpatiu(os => {
            var db = ((EFCoreObjectSpace)os).DbContext;
            using var tx = db.Database.BeginTransaction();
            try {
                db.Database.ExecuteSqlInterpolated(
                    $"""UPDATE "Tranzactie" SET "Explicatie" = '1'::jsonb WHERE "ID" = {storno.Id}""");
                return false;
            }
            catch (PostgresException ex) { return ex.SqlState == PostgresErrorCodes.CheckViolation; }
            finally { tx.Rollback(); }
        }));

        var anulat = Bon((scump, 1)); Opereaza(anulat.Id);
        Verifica("SC-CIT-96", "operarea scrie explicația", Tranzactii(anulat.Id).Single().Explicata);
        Anuleaza(anulat.Id); FaraEfecte("SC-CIT-96", anulat.Id);

        DeclaratieInvalida(scump);
    }

    // S-r11: excepția de construcție a nucleului iese ca refuz cu cod stabil, pe dry-run și pe operare.
    void DeclaratieInvalida(LinieScena lot) {
        var rau = Bon((lot, 1));
        Verifica("S-r11", "cantitate în afara scării: refuz `DECLARATIE_INVALIDA`, nu excepție", CuSpatiu(os => {
            var d = os.GetObjectByKey<Document>(rau.Id);
            d.DataInregistrare = d.Data;
            d.Detalii.Single().Cantitate = 1.2345m;
            var tip = os.GetObjectsQuery<TipDocument>().Single(t => t.ClrType == d.ClrType);
            try {
                var refuzuri = C.Materializare.Refuzuri(os, d, tip);
                Console.WriteLine("     MĂSURAT (S-r11): " + string.Join(" | ", refuzuri));
                return refuzuri.Count == 1 && refuzuri[0].StartsWith(CoduriRefuz.DeclaratieInvalida + ":", StringComparison.Ordinal);
            }
            catch (Exception ex) {
                Console.WriteLine("     MĂSURAT (S-r11): " + ex.GetType().Name + ": " + ex.Message);
                return false;
            }
        }));
        FaraEfecte("S-r11", rau.Id);
    }

    void Codec() {
        var lot = new N.Unitate(Guid.NewGuid(), N.FelUnitate.Lot, Guid.NewGuid(), null, Guid.NewGuid(), new(An, 1, 5));
        var partida = N.Unitate.DeschidePartida(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), new(An, 1, 6));
        var linie = Guid.NewGuid();
        var explicatie = new C.Explicatie("DeclarantDeProba", 3, [
            new N.ContRezolvat(linie, lot.Cont, "Diferență: Plus"),
            new N.ValoareIesire(linie, lot, 1.5m, 20.10m),
            new N.ValoareDeclarata(linie, lot, 2m, .33m, SurseValoare.Receptie),
            new N.AlocareFifo(linie, partida, 61m),
            new N.PartidaDeschisa(linie, partida),
        ], [
            new N.SoldUnitateCitit(lot, new N.Sold(100m, 20.10m, 8.5m, 0m)),
            new N.SoldUnitateCitit(partida, new N.Sold(0m, 61m, 0m, 12.5m)),
            new N.PerioadaDeschisa(An, 1),
            new N.VersiunePolitica("seed", new(An, 1, 5)),
        ]);
        var json = explicatie.Scrie();
        var prinBaza = CuSpatiu(os => ((EFCoreObjectSpace)os).DbContext.Database
            .SqlQuery<string>($"""SELECT ({json}::jsonb)::text AS "Value" """).Single());
        bool Refuza(string text) {
            try { C.Explicatie.Citeste(text); return false; }
            catch (InvalidOperationException) { return true; }
        }
        Verifica("SC-CIT-96", "forma persistată: toate deciziile și ipotezele se citesc înapoi identic, și după trecerea prin jsonb",
            C.Explicatie.Citeste(json) == explicatie && C.Explicatie.Citeste(prinBaza) == explicatie
            && json.StartsWith("{\"v\":1,\"declarant\":\"DeclarantDeProba\",\"jumatati\":3,\"decizii\":[", StringComparison.Ordinal));
        Verifica("SC-CIT-96", "versiune, decizie sau ipoteză necunoscută: cititorul refuză, nu ghicește",
            Refuza(json.Replace("\"v\":1", "\"v\":2")) && Refuza(json.Replace(nameof(N.ValoareIesire), "ValoareNoua"))
            && Refuza(json.Replace(nameof(N.PerioadaDeschisa), "PerioadaNoua")));
    }
}
