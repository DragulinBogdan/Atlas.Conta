using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Declaratii;
using Atlas.Conta.BackOffice.Module.Motor;
using DevExpress.ExpressApp.EFCore;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using C = Atlas.Conta.BackOffice.Module.Cub;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.ModelCheck;

sealed partial class ScenaImo {
    void ReviewDeschidere() {
        foreach (var (cont, latura, contra) in new[] {
                (contActiv, N.Latura.Credit, N.Latura.Debit),
                (contAmortizare, N.Latura.Debit, N.Latura.Credit) }) {
            using var os = Deschide(); using var tx = TranzactieComanda.Incepe(os);
            Refuza("SC-IMO-33", () => C.Materializare.Deschide(os, Ianuarie,
                [new(cont, latura, 1), new(Cont(Capital), contra, 1)], [], []), CoduriRefuz.PozitieFaraFisaNegativa);
            Verifica("SC-IMO-33", "deschiderea refuzată nu creează entități de cub", !os.ModifiedObjects.OfType<C.Postare>().Any()
                && !os.ModifiedObjects.OfType<C.Tranzactie>().Any());
        }
    }

    void ReviewConcurenta() {
        SuportNota(); var p = Pif(Fisa(), 800, 800);
        var nota = Nota(Zi(1, 10), new LinieNtcScena(Capital, Activ, 800, Loc, Magazie));
        Task<string> concurent;
        using (var os = Deschide()) using (var tx = TranzactieComanda.Incepe(os)) {
            MotorOperare.Opereaza(os, os.GetObjectByKey<Document>(p.Id));
            os.CommitChanges();
            concurent = Task.Run(() => {
                try { Opereaza(nota.Id); return "OK"; }
                catch (Exception e) { return e.Message; }
            });
            var db = ((EFCoreObjectSpace)os).DbContext;
            var pid = ((NpgsqlConnection)db.Database.GetDbConnection()).ProcessID;
            string interogare = null;
            var ceas = System.Diagnostics.Stopwatch.StartNew();
            while (ceas.Elapsed < TimeSpan.FromSeconds(5) && !concurent.IsCompleted) {
                db.Database.ExecuteSqlRaw("SELECT pg_stat_clear_snapshot()");
                interogare = db.Database.SqlQuery<string>($"""
                    SELECT query AS "Value" FROM pg_stat_activity WHERE {pid} = ANY(pg_blocking_pids(pid))
                    """).FirstOrDefault();
                if (interogare != null) break;
                Thread.Sleep(20);
            }
            Verifica("SC-IMO-31", "consumul NTC așteaptă nominalizarea PIF", interogare?.Contains("pg_advisory_xact_lock") == true);
            tx.Commit();
        }
        var rezultat = concurent.GetAwaiter().GetResult();
        Verifica("SC-IMO-31", "800 NTC refuzat după PIF 800 din suport 1200", rezultat.Contains(CoduriRefuz.PozitieFaraFisaNegativa));
        FaraEfecte("SC-IMO-31", nota.Id);
        Verifica("SC-IMO-31", "poziția anonimă rămâne 400", CuSpatiu(os => os.GetObjectsQuery<C.Postare>()
            .Where(p => p.Cont == contActiv && p.Carte == N.Carte.Contabil && p.Unitate == null)
            .Sum(p => p.Latura == N.Latura.Debit ? p.Valoare : -p.Valoare)) == 400);
    }

    void ReviewBlocare() {
        foreach (var anulare in new[] { true, false }) {
            var plata = Trezorerie(false, 20); Opereaza(plata.Id);
            Task<string> concurent;
            using (var os = Deschide()) using (var tx = ((EFCoreObjectSpace)os).DbContext.Database.BeginTransaction()) {
                C.Materializare.BlocheazaFise(os);
                concurent = Task.Run(() => {
                    try {
                        Comanda(alt => {
                            ((EFCoreObjectSpace)alt).DbContext.Database.SetCommandTimeout(3);
                            if (anulare) Atlas.Conta.BackOffice.Module.Api.ComenziDocument.Sistem(alt).AnuleazaOperarea(plata.Id);
                            else Atlas.Conta.BackOffice.Module.Api.ComenziDocument.Sistem(alt).Storneaza(plata.Id, Ianuarie);
                        }); return "OK";
                    }
                    catch (Exception e) { return e.Message; }
                });
                Verifica("SC-IMO-32", "plata fără suport/fișă termină sub blocaj IMO străin", concurent.Wait(TimeSpan.FromSeconds(5))
                    && concurent.Result == "OK");
            }
            concurent.GetAwaiter().GetResult();
        }
    }

    void ReviewSursa() {
        var factura = Factura(Ianuarie, new LinieFctScena(1, 1200, Stoc: false, Tip: Activ));
        if (!Privat) Comanda(os => {
            var cf = os.CreateObject<CodFunctional>(); cf.Cod = Marcaj; cf.Denumire = Marcaj;
            var sf = os.CreateObject<SursaFinantare>(); sf.Cod = Marcaj; sf.Denumire = Marcaj;
            var pr = os.CreateObject<Proiect>(); pr.Cod = Marcaj; pr.Denumire = Marcaj;
            var l = os.GetObjectByKey<FacturaIntrareDetaliu>(factura.Linii[0].Id);
            l.CodFunctionalId = cf.ID; l.SursaFinantareId = sf.ID; l.ProiectId = pr.ID; os.CommitChanges();
        });
        Opereaza(factura.Id);
        var p = Pif(Fisa());
        RefuzDeclaratie("SC-IMO-26", p.Id, CoduriRefuz.SuportInsuficient);
        Refuza("SC-IMO-26", () => Opereaza(p.Id), CoduriRefuz.SuportInsuficient);
        FaraEfecte("SC-IMO-26", p.Id);
        Comanda(os => {
            var erori = C.Materializare.Refuzuri(os, os.GetObjectByKey<Document>(p.Id),
                os.GetObjectsQuery<TipDocument>().Single(t => t.Cod == "PIF"));
            Verifica("SC-IMO-26", "refuzul numește simbolul contului", erori.Any(e => e.Contains(Activ))
                && erori.All(e => !e.Contains(contActiv.ToString())));
        });
    }

    void ReviewPozitie() {
        SuportNota(cumulat: 200); var f = Fisa(); var p = Pif(f, initial: 200);
        Comanda(os => { var d = os.GetObjectByKey<Document>(p.Id); d.Data = Zi(1, 20); d.DataInregistrare = d.Data; os.CommitChanges(); });
        Opereaza(p.Id);
        foreach (var zi in new[] { 10, 25 }) {
            var nota = Nota(Zi(1, zi), new LinieNtcScena(Capital, Activ, 1, Loc, Magazie));
            RefuzDeclaratie("SC-IMO-27", nota.Id, "POZITIE_FARA_FISA_NEGATIVA");
            Refuza("SC-IMO-27", () => Opereaza(nota.Id), "POZITIE_FARA_FISA_NEGATIVA");
            FaraEfecte("SC-IMO-27", nota.Id);
        }
        Sold("SC-IMO-27", f, 1200, 200, 900, 0);
        var amort = Nota(Zi(1, 25), new LinieNtcScena(Amortizare, Capital, 1, Magazie, Loc));
        RefuzDeclaratie("SC-IMO-27", amort.Id, CoduriRefuz.PozitieFaraFisaNegativa);
        Refuza("SC-IMO-27", () => Opereaza(amort.Id), CoduriRefuz.PozitieFaraFisaNegativa);
        FaraEfecte("SC-IMO-27", amort.Id);
        var supliment = SuportNota(20);
        var altaGestiune = Nota(Zi(1, 25), new LinieNtcScena(Capital, Activ, 1, Loc, Destinatie));
        RefuzDeclaratie("SC-IMO-27", altaGestiune.Id, CoduriRefuz.PozitieFaraFisaNegativa);
        Refuza("SC-IMO-27", () => Opereaza(altaGestiune.Id), CoduriRefuz.PozitieFaraFisaNegativa);
        FaraEfecte("SC-IMO-27", altaGestiune.Id);
        var iesire = Nota(Zi(1, 25), new LinieNtcScena(Capital, Activ, 20, Loc, Magazie)); Opereaza(iesire.Id);
        Refuza("SC-IMO-28", () => Anuleaza(supliment.Id), "POZITIE_FARA_FISA_NEGATIVA");
        Refuza("SC-IMO-28", () => Storneaza(supliment.Id, Zi(1, 26)), "POZITIE_FARA_FISA_NEGATIVA");
        Verifica("SC-IMO-28", "suportul consumat rămâne operat", CuSpatiu(os => os.GetObjectByKey<Document>(supliment.Id).Stare) == StareDocument.Operat);
    }

    void ReviewConturi() {
        SuportNota(); var f = Fisa(); var p = Pif(f); Opereaza(p.Id);
        var a = Amo(2); Opereaza(a);
        var alte = CuSpatiu(os => os.GetObjectsQuery<PoliticaAmortizare>()
            .Where(p => p.ContAmortizareId != null && p.ContAmortizareId != contAmortizare)
            .Select(p => new { Activ = p.TipMaterial.ContImplicitId, Amort = p.ContAmortizareId }).First());
        try {
            Comanda(os => {
                os.GetObjectByKey<TipMaterial>(tip).ContImplicitId = alte.Activ;
                os.GetObjectsQuery<PoliticaAmortizare>().Single(p => p.TipMaterialId == tip).ContAmortizareId = alte.Amort;
                os.CommitChanges();
            });
            var b = Amo(3);
            Comanda(os => { os.GetObjectsQuery<PoliticaAmortizare>().Single(p => p.TipMaterialId == tip).ContAmortizareId = null; os.CommitChanges(); });
            Opereaza(b); Sold("SC-IMO-29", f, 1200, 200, 900, 100);
            Comanda(os => { os.GetObjectsQuery<PoliticaAmortizare>().Single(p => p.TipMaterialId == tip).ContAmortizareId = alte.Amort; os.CommitChanges(); });
            var c = Cas(f, 4); Opereaza(c); Sold("SC-IMO-29", f, 0, 0, 0, 0);
            Verifica("SC-IMO-29", "conturile noi nu primesc postări ale fișei", CuSpatiu(os => !os.GetObjectsQuery<C.Postare>()
                .Any(p => p.Unitate == f && (p.Cont == alte.Activ || p.Cont == alte.Amort))));
            Storneaza(c, Zi(4)); Origini("SC-IMO-29", c); Sold("SC-IMO-29", f, 1200, 200, 900, 100);
        }
        finally {
            Comanda(os => {
                os.GetObjectByKey<TipMaterial>(tip).ContImplicitId = contActiv;
                os.GetObjectsQuery<PoliticaAmortizare>().Single(p => p.TipMaterialId == tip).ContAmortizareId = contAmortizare;
                os.CommitChanges();
            });
        }
    }

    void ReviewInverse() {
        var n = SuportNota(); var f = Fisa(); var p = Pif(f); Opereaza(p.Id);
        Storneaza(p.Id, Zi(1, 15));
        var stamp = Amprenta(n.Id);
        Refuza("SC-IMO-30", () => Anuleaza(n.Id), CoduriRefuz.SuportCuDependenti);
        Verifica("SC-IMO-30", "anularea nu șterge suportul istoric", Amprenta(n.Id) == stamp);
        Storneaza(n.Id, Zi(1, 15)); Origini("SC-IMO-30", n.Id);
        Sold("SC-IMO-30", f, 0, 0, 0, 0);
    }
}
