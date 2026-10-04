using System.Diagnostics;
using Atlas.Conta.BackOffice.Module.Api;
using Atlas.Conta.BackOffice.Module.Api.Amo;
using Atlas.Conta.BackOffice.Module.Api.Itv;
using Atlas.Conta.BackOffice.Module.Api.Perioade;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Motor;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.EFCore;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using C = Atlas.Conta.BackOffice.Module.Cub;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.ModelCheck;

// X-D6: scrierea serială per bază, probată pe conexiuni distincte (SC-X-15…SC-X-23).
sealed class ScenariiConcurenta(Func<IObjectSpace> deschide, Action<string, bool> check,
    bool privat, Action<IObjectSpace, int, int> inchide)
    : ScenaDocumente(deschide, check, privat, inchide, "CONC", 2012) {
    const string Trecut = "";
    const string Cheie = "pg_advisory_xact_lock(97000";
    DateOnly Sfarsit => new(An, 1, 31);

    static string Rezultat(Action comanda) {
        try { comanda(); return Trecut; }
        catch (OperareException e) { return "REFUZ " + e.Message.Split('\n')[0]; }
        catch (Exception e) { return $"EXCEPȚIE {e.GetType().Name}: {e.Message.Split('\n')[0]}"; }
    }

    static bool Refuzat(string rezultat, string fragment) =>
        rezultat.StartsWith("REFUZ ", StringComparison.Ordinal) && rezultat.Contains(fragment, StringComparison.OrdinalIgnoreCase);

    // Scena ține blocajul scrierii; cele două comenzi intră pe rând în coada lui, iar eliberarea le rulează serial, în ordinea cozii.
    (string Prima, string ADoua) Serial(string id, Action prima, Action aDoua) {
        Task<string> a, b;
        using (var os = Deschide()) using (var poarta = TranzactieComanda.Incepe(os)) {
            a = Task.Run(() => Rezultat(prima));
            var unu = InCoada(os, 1, a);
            b = Task.Run(() => Rezultat(aDoua));
            var doi = InCoada(os, 2, a, b);
            Verifica(id, "două sesiuni pe conexiuni distincte așteaptă blocajul scrierii, în ordinea intrării",
                unu.Count == 1 && doi.Count == 2 && doi.Contains(unu[0]));
            poarta.Commit();
        }
        var r = (a.GetAwaiter().GetResult(), b.GetAwaiter().GetResult());
        Console.WriteLine($"     MĂSURAT ({id}): prima → „{(r.Item1 == Trecut ? "<a trecut>" : r.Item1)}”, "
            + $"a doua → „{(r.Item2 == Trecut ? "<a trecut>" : r.Item2)}”.");
        Verifica(id, "nicio excepție în afara refuzului de domeniu",
            !r.Item1.StartsWith("EXCEPȚIE", StringComparison.Ordinal) && !r.Item2.StartsWith("EXCEPȚIE", StringComparison.Ordinal));
        return r;
    }

    static List<int> InCoada(IObjectSpace os, int cate, params Task[] comenzi) {
        var db = ((EFCoreObjectSpace)os).DbContext;
        var pid = ((NpgsqlConnection)db.Database.GetDbConnection()).ProcessID;
        var ceas = Stopwatch.StartNew();
        List<int> blocate = [];
        while (ceas.Elapsed < TimeSpan.FromSeconds(10) && !comenzi.Any(c => c.IsCompleted)) {
            db.Database.ExecuteSqlRaw("SELECT pg_stat_clear_snapshot()");
            blocate = db.Database.SqlQuery<int>($"""
                SELECT pid AS "Value" FROM pg_stat_activity
                WHERE {pid} = ANY(pg_blocking_pids(pid)) AND query = {TranzactieComanda.BlocajScriere}
                ORDER BY query_start
                """).ToList();
            if (blocate.Count >= cate) break;
            Thread.Sleep(20);
        }
        return blocate;
    }

    protected override void Executa() {
        ComenziSubBlocaj();
        ConsumPesteLot();
        ConsumIncape();
        ConsumContraRetragere();
        Stingeri();
        Pozitii();
        ScriereOcupata();
        OperareContraInchidere();
    }

    C.IesireExplicata Iesirea(Guid bon) =>
        Explicatia(bon, N.FelTranzactie.Operare).Origini.Single().Explicatie.Linii().Single().Iesiri.Single();

    static bool Sold(N.Sold sold, decimal debit, decimal credit, decimal cantitate) =>
        sold is not null && sold.Debit == debit && sold.Credit == credit && sold.Cantitate == cantitate;

    void ConsumPesteLot() {
        var lot = Receptioneaza(new LinieFctScena(10, 10)).Linii[0].Lot!.Value;
        var (a, b) = (Consum(lot, 6), Consum(lot, 6));
        var r = Serial("SC-X-15", () => Opereaza(a), () => Opereaza(b));
        Verifica("SC-X-15", "două consumuri de 6 din lotul 10/100: primul trece, al doilea e refuzat cu STOC_INSUFICIENT",
            r.Prima == Trecut && Refuzat(r.ADoua, "STOC_INSUFICIENT"));
        FaraEfecte("SC-X-15", b);
        SoldLot("SC-X-15", lot, Magazie, Sfarsit, 4, 40);
    }

    void ConsumIncape() {
        var lot = Receptioneaza(new LinieFctScena(3, .333333m)).Linii[0].Lot!.Value;
        var (a, b) = (Consum(lot, 1), Consum(lot, 1));
        var r = Serial("SC-X-16", () => Opereaza(a), () => Opereaza(b));
        var (prima, aDoua) = r == (Trecut, Trecut) ? (Iesirea(a), Iesirea(b)) : (null, null);
        Console.WriteLine($"     MĂSURAT (SC-X-16): prima {prima?.Valoare} din {prima?.SoldInainte}; a doua {aDoua?.Valoare} din {aDoua?.SoldInainte}.");
        Verifica("SC-X-16", "două consumuri de 1 din lotul 3/1,00: ambele trec, cu 0,33 și 0,34 în ordinea serializată",
            prima is { Cantitate: 1, Valoare: .33m } && aDoua is { Cantitate: 1, Valoare: .34m });
        Verifica("SC-X-16", "explicațiile persistate arată două solduri citite diferite: 3/1,00, apoi 2/0,67",
            Sold(prima?.SoldInainte, 1, 0, 3) && Sold(aDoua?.SoldInainte, .67m, 0, 2));
        SoldLot("SC-X-16", lot, Magazie, Sfarsit, 1, .33m);
    }

    void ConsumContraRetragere() {
        foreach (var intaiConsumul in new[] { true, false }) {
            var id = "SC-X-17/" + (intaiConsumul ? "consum → retragere" : "retragere → consum");
            var f = Factura(Ianuarie, new LinieFctScena(10, 10));
            Opereaza(f.Id);
            var lot = f.Linii[0].Lot!.Value;
            var bon = Consum(lot, 4);
            Action consum = () => Opereaza(bon);
            Action retragere = () => Comanda(os => ComenziDocument.Sistem(os).Storneaza(f.Id, new(An, 1, 20)));
            var r = intaiConsumul ? Serial(id, consum, retragere) : Serial(id, retragere, consum);
            Verifica(id, "prima comandă trece, a doua e refuzată cu STOC_INSUFICIENT",
                r.Prima == Trecut && Refuzat(r.ADoua, "STOC_INSUFICIENT"));
            var stare = CuSpatiu(os => os.GetObjectByKey<Document>(f.Id).Stare);
            if (intaiConsumul) {
                Verifica(id, "recepția rămâne operată", stare == StareDocument.Operat);
                SoldLot(id, lot, Magazie, Sfarsit, 6, 60);
            }
            else {
                Verifica(id, "recepția e stornată", stare == StareDocument.Stornat);
                FaraEfecte(id, bon);
                SoldLot(id, lot, Magazie, Sfarsit, 0, 0);
            }
        }
    }

    decimal Rest(Guid document) => CuSpatiu(os => ImperechereService.Ramas(os, document));

    void Stingeri() {
        var f = Factura(Ianuarie, new LinieFctScena(1, 100, Stoc: false)); Opereaza(f.Id);
        var (p1, p2) = (Trezorerie(false, 60), Trezorerie(false, 60)); Opereaza(p1.Id); Opereaza(p2.Id);
        var r = Serial("SC-X-18", () => Imperecheaza(p1.Id, f.Id, 60, Ianuarie), () => Imperecheaza(p2.Id, f.Id, 60, Ianuarie));
        Verifica("SC-X-18", "două stingeri de 60 pe partida de 100: prima trece, a doua e refuzată peste rest",
            r.Prima == Trecut && r.ADoua.StartsWith("REFUZ ", StringComparison.Ordinal));
        Verifica("SC-X-18", "restul facturii e 40, plata refuzată rămâne nealocată", Rest(f.Id) == 40 && Rest(p1.Id) == 0 && Rest(p2.Id) == 60);

        var g = Factura(Ianuarie, new LinieFctScena(1, 100, Stoc: false)); Opereaza(g.Id);
        var p = Trezorerie(false, 100); Opereaza(p.Id);
        r = Serial("SC-X-19", () => Imperecheaza(p.Id, g.Id, 40, Ianuarie), () => Imperecheaza(p.Id, g.Id, 40, Ianuarie));
        Verifica("SC-X-19", "aceeași pereche 100/100, de două ori 40: ambele trec, rest 20 de ambele părți",
            r == (Trecut, Trecut) && Rest(g.Id) == 20 && Rest(p.Id) == 20);
        r = Serial("SC-X-19", () => Imperecheaza(p.Id, g.Id, 15, Ianuarie), () => Imperecheaza(p.Id, g.Id, 15, Ianuarie));
        Verifica("SC-X-19", "aceeași pereche, de două ori 15 din restul 20: prima trece, a doua e refuzată, rest 5",
            r.Prima == Trecut && r.ADoua.StartsWith("REFUZ ", StringComparison.Ordinal) && Rest(g.Id) == 5 && Rest(p.Id) == 5);
    }

    void Adauga(Guid bon, Guid lot) => Comanda(os => {
        var l = os.CreateObject<DocumentDetaliu>(); l.Document = os.GetObjectByKey<Document>(bon);
        l.TipMaterialId = Tip(os, Stoc); l.LotId = lot; l.Cantitate = 1;
        os.CommitChanges();
    });

    // S-r9
    void Pozitii() {
        var lot = Receptioneaza(new LinieFctScena(10, 10)).Linii[0].Lot!.Value;
        var bon = Consum(lot, 1);
        var r = Serial("SC-X-20", () => Adauga(bon, lot), () => Adauga(bon, lot));
        var pozitii = CuSpatiu(os => os.GetObjectsQuery<DocumentDetaliu>().Where(l => l.DocumentId == bon)
            .Select(l => l.Pozitie).OrderBy(x => x).ToList());
        Console.WriteLine($"     MĂSURAT (SC-X-20): pozițiile [{string.Join(", ", pozitii)}].");
        Verifica("SC-X-20", "două detalii noi pe același document, din două sesiuni: ambele trec, pozițiile sunt 1, 2, 3",
            r == (Trecut, Trecut) && pozitii.SequenceEqual([1, 2, 3]));
    }

    void ScriereOcupata() {
        var lot = Receptioneaza(new LinieFctScena(10, 10)).Linii[0].Lot!.Value;
        var bon = Consum(lot, 1);
        string rezultat;
        using (var os = Deschide()) using (var poarta = TranzactieComanda.Incepe(os)) {
            rezultat = Task.Run(() => Rezultat(() => Comanda(alt => {
                ((EFCoreObjectSpace)alt).DbContext.Database.SetCommandTimeout(1);
                ComenziDocument.Sistem(alt).Opereaza(bon);
            }))).GetAwaiter().GetResult();
        }
        Console.WriteLine($"     MĂSURAT (SC-X-23): „{rezultat}”.");
        Verifica("SC-X-23", "așteptarea peste timpul comenzii iese ca refuz `SCRIERE_OCUPATA`, nu ca eroare de bază",
            Refuzat(rezultat, TranzactieComanda.ScriereOcupata));
        FaraEfecte("SC-X-23", bon);
        Verifica("SC-X-23", "după eliberare, aceeași comandă trece", Rezultat(() => Opereaza(bon)) == Trecut);
    }

    (decimal Cantitate, decimal Valoare) Snapshot(int luna, Guid lot) {
        var randuri = CuSpatiu(os => os.GetObjectsQuery<SoldPerioadaStoc>()
            .Where(s => s.An == An && s.Luna == luna && s.LotId == lot)
            .Select(s => new { s.GestiuneId, s.ContId, s.Cantitate, s.Valoare }).ToList());
        Console.WriteLine($"     MĂSURAT (SC-X-21): snapshot {luna:00}/{An}: " + string.Join("; ", randuri.Select(r =>
            $"{(r.GestiuneId == Magazie ? "magazie" : r.GestiuneId)} {r.Cantitate}/{r.Valoare}")) + ".");
        var magazie = randuri.Where(r => r.GestiuneId == Magazie).ToList();
        return (magazie.Sum(r => r.Cantitate), magazie.Sum(r => r.Valoare));
    }

    // F27-r8
    void OperareContraInchidere() {
        var lot = Receptioneaza(new LinieFctScena(10, 10)).Linii[0].Lot!.Value;
        var ianuarie = Consum(lot, 4);
        var r = Serial("SC-X-21/operare → închidere", () => Opereaza(ianuarie), () => Inchide(An, 1));
        Verifica("SC-X-21/operare → închidere", "ambele trec; snapshot-ul lunii închise poartă consumul: 6/60",
            r == (Trecut, Trecut) && Snapshot(1, lot) == (6m, 60m));

        var februarie = Consum(lot, 4);
        Comanda(os => { os.GetObjectByKey<Document>(februarie).Data = Februarie; os.CommitChanges(); });
        r = Serial("SC-X-21/închidere → operare", () => Inchide(An, 2), () => Opereaza(februarie));
        Verifica("SC-X-21/închidere → operare", "închiderea trece, operarea e refuzată de perioada închisă; snapshot 6/60",
            r.Prima == Trecut && Refuzat(r.ADoua, "închisă") && Snapshot(2, lot) == (6m, 60m));
        FaraEfecte("SC-X-21/închidere → operare", februarie);
    }

    // X-D6 (c): captura SQL a fiecărui fel de comandă începe cu blocajul scrierii; citirile și dry-run-ul nu îl iau.
    void ComenziSubBlocaj() {
        var lot = Receptioneaza(new LinieFctScena(10, 10)).Linii[0].Lot!.Value;
        var (bon, corectat, validat) = (Consum(lot, 1), Consum(lot, 1), Consum(lot, 1));
        var f = Factura(Ianuarie, new LinieFctScena(1, 100, Stoc: false)); Opereaza(f.Id);
        var plata = Trezorerie(false, 100); Opereaza(plata.Id);
        Opereaza(corectat);
        var rezultate = new List<(string Nume, bool Prima, string Rezultat)>();
        void Sub(string nume, Action comanda) {
            var rezultat = Trecut;
            var sql = CapturaSql.Comenzi(() => rezultat = Rezultat(comanda));
            rezultate.Add((nume, sql.Count > 0 && sql[0] == TranzactieComanda.BlocajScriere, rezultat));
        }
        void InSpatiu(string nume, Action<IObjectSpace> comanda) => Comanda(os => Sub(nume, () => comanda(os)));
        void InTranzactiaApelantului(string nume, Action<IObjectSpace> usa) => Comanda(os => {
            var db = ((EFCoreObjectSpace)os).DbContext.Database;
            var stingator = os.GetObjectByKey<Document>(plata.Id);
            _ = stingator.Stare;
            using var tx = db.BeginTransaction();
            Sub(nume, () => usa(os));
            tx.Rollback();
        });

        Sub("operare", () => Opereaza(bon));
        InSpatiu("anulare", os => ComenziDocument.Sistem(os).AnuleazaOperarea(bon));
        Opereaza(bon);
        InSpatiu("storno", os => ComenziDocument.Sistem(os).Storneaza(bon, new(An, 1, 20)));
        InSpatiu("corecție", os => ComenziDocument.Sistem(os).Corecteaza(corectat, Februarie, MotivCorectie.EroareMateriala));
        Guid legatura = default;
        Comanda(os => {
            var (s, d) = (os.GetObjectByKey<Document>(plata.Id), os.GetObjectByKey<Document>(f.Id));
            _ = (s.Stare, d.Stare);
            Sub("împerechere", () => legatura = ImperechereService.Imperecheaza(os, s, d, 40, data: Ianuarie).ID);
        });
        InSpatiu("desfacere", os => ImperechereService.Desfa(os, legatura, Ianuarie));
        var deSters = Imperecheaza(plata.Id, f.Id, 10, Ianuarie);
        InSpatiu("ștergerea împerecherii", os => ImperechereService.Sterge(os, deSters));
        InSpatiu("închidere", os => PerioadaService.Inchide(os, An, 2, [], null, Marcaj));
        InSpatiu("redeschidere", os => PerioadaService.Redeschide(os, An, 2, Marcaj, null, Marcaj));
        InSpatiu("reconstrucție", os => PerioadeApply.Reconstruieste(os));
        InSpatiu("generare AMO", os => AmoApply.Genereaza(os, new() { An = An, Luna = 1, UnitateId = Loc }));
        InSpatiu("generare ITV", os => InchidereTvaApply.Genereaza(os, new() { An = An, Luna = 1, UnitateId = Loc }));
        InSpatiu("confirmarea depunerii", os => FiscalitateService.ConfirmaDepunerea(os, FormularFiscal.D300, An, 1, Marcaj, Marcaj));
        InTranzactiaApelantului("deschidere", os => C.Materializare.Deschide(os, Ianuarie, [], [], []));
        InTranzactiaApelantului("stingerea deschiderii", os =>
            C.Materializare.Imperecheaza(os, os.GetObjectByKey<Document>(plata.Id), Guid.NewGuid(), 1, Ianuarie));
        Comanda(os => {
            var l = os.CreateObject<DocumentDetaliu>(); l.Document = os.GetObjectByKey<Document>(validat);
            l.TipMaterialId = Tip(os, Stoc); l.LotId = lot; l.Cantitate = 1;
            Sub("detaliu nou", os.CommitChanges);
        });
        foreach (var (nume, prima, rezultat) in rezultate)
            Console.WriteLine($"     MĂSURAT (SC-X-22): {nume} → {(prima ? "blocajul întâi" : "FĂRĂ blocaj întâi")}; {(rezultat == Trecut ? "a trecut" : rezultat)}.");
        Verifica("SC-X-22", $"cele {rezultate.Count} feluri de comandă iau blocajul scrierii ca primă instrucțiune, fără excepții în afara refuzului",
            rezultate.Count == 16 && rezultate.All(r => r.Prima && !r.Rezultat.StartsWith("EXCEPȚIE", StringComparison.Ordinal)));
        Verifica("SC-X-22", "perioada probei a rămas deschisă", CuSpatiu(os =>
            !os.GetObjectsQuery<PerioadaFiscala>().Single(p => p.An == An && p.Luna == 2).Inchisa));

        var tranzactie = Tranzactii(corectat).First().Id;
        var citiri = new (string Nume, Action<IObjectSpace> Citire)[] {
            ("dry-run", os => ComenziDocument.Sistem(os).Valideaza(validat)),
            ("verificarea închiderii", os => PerioadaService.Verifica(os, An, 1)),
            ("restul documentului", os => ImperechereService.Ramas(os, f.Id)),
            ("explicația", os => C.Citiri.Explicatii.PeTranzactie(os, tranzactie)),
            ("cititorii comuni", os => { foreach (var i in ProbeTransferCititori.Intrari) _ = i.Postari(os).Count(); }),
            ("invarianții cubului", os => C.Citiri.Invarianti.Verifica(os)),
            ("salvarea unui antet", os => { os.GetObjectByKey<Document>(validat).Numar = Marcaj; os.CommitChanges(); }),
        };
        var cuBlocaj = citiri.Where(c => CapturaSql.Comenzi(() => Comanda(c.Citire)) is var sql
            && (sql.Count == 0 || sql.Any(s => s.Contains(Cheie, StringComparison.Ordinal)))).Select(c => c.Nume).ToList();
        Console.WriteLine($"     MĂSURAT (SC-X-22): {citiri.Length} citiri; fără SQL sau cu blocaj [{string.Join(", ", cuBlocaj)}].");
        Verifica("SC-X-22", "dry-run-ul, citirile și salvarea fără detalii noi nu iau blocajul scrierii", cuBlocaj.Count == 0);
    }
}
