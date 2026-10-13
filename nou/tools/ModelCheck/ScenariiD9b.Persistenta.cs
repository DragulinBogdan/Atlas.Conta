using Atlas.Conta.BackOffice.Module.Api;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Declaratii;
using Atlas.Conta.BackOffice.Module.Motor;
using DevExpress.ExpressApp.EFCore;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using C = Atlas.Conta.BackOffice.Module.Cub;

namespace Atlas.Conta.BackOffice.ModelCheck;

sealed partial class ScenariiD9b {
    void ScrierePeSet() {
        var citiri = new List<int>();
        foreach (var cate in new[] { 2, 51 }) {
            var titulari = new List<Guid>();
            for (var i = 0; i < cate; i++) { Titular($"SET-{cate}-{i}"); titulari.Add(Furnizor); }
            var n = Nota(Ianuarie, titulari.Select(t => new LinieNtcScena(Serviciu, ContFurnizor, 100, null, t)).ToArray());
            var sql = CapturaSql.Comenzi(() => Opereaza(n.Id));
            citiri.Add(sql.Count(s => s.StartsWith("SELECT", StringComparison.Ordinal) && s.Contains("\"OriginiPartide\"")));
            Verifica("SC-D9B-11", $"scriere pe set: {cate} origini și {100 * cate} credit", CuSpatiu(os =>
                os.GetObjectsQuery<C.OriginePartidaPersistata>().Count(o => o.DocumentId == n.Id) == cate
                && C.Citiri.Partide.Postari(os).Where(p => p.DocumentId == n.Id).Sum(p => p.Valoare) == 100 * cate));
        }
        Verifica("SC-D9B-11", $"citiri de origine constante pentru 2/51 partide: {citiri[0]}/{citiri[1]}", citiri[0] > 0 && citiri[0] == citiri[1]);
    }

    bool FaraEfecte(Guid document) => CuSpatiu(os =>
        os.GetObjectByKey<Document>(document).Stare == StareDocument.Draft
        && !os.GetObjectsQuery<C.OriginePartidaPersistata>().Any(o => o.DocumentId == document)
        && !os.GetObjectsQuery<C.Postare>().Any(p => p.DocumentId == document)
        && !os.GetObjectsQuery<C.Tranzactie>().Any(t => t.DocumentId == document));

    void Atomicitate() {
        Titular("ATOMIC");
        var f = base.Factura(Ianuarie, new LinieFctScena(1, 100, Stoc: false));
        Verifica("SC-D9B-11", "dry-run acceptat fără origine, tranzacție sau postare", CuSpatiu(os =>
            ComenziDocument.Sistem(os).Valideaza(f.Id).Count == 0) && FaraEfecte(f.Id));
        var pregatita = false;
        try {
            Comanda(os => {
                os.Committing += (_, _) => {
                    if (!os.ModifiedObjects.OfType<C.OriginePartidaPersistata>().Any()) return;
                    pregatita = true;
                    throw new InvalidOperationException("D9B-ESEC-INJECTAT");
                };
                ComenziDocument.Sistem(os).Opereaza(f.Id);
            });
        }
        catch (InvalidOperationException e) when (e.Message == "D9B-ESEC-INJECTAT") { }
        Verifica("SC-D9B-11", "eșec după pregătirea originii: rollback complet", pregatita && FaraEfecte(f.Id));
        var scrisa = false;
        try {
            Comanda(os => {
                os.Committed += (_, _) => {
                    if (!os.GetObjectsQuery<C.OriginePartidaPersistata>().Any(o => o.DocumentId == f.Id)) return;
                    scrisa = true;
                    throw new InvalidOperationException("D9B-ESEC-DUPA-SAVE");
                };
                ComenziDocument.Sistem(os).Opereaza(f.Id);
            });
        }
        catch (InvalidOperationException e) when (e.Message == "D9B-ESEC-DUPA-SAVE") { }
        Verifica("SC-D9B-11", "eșec după SaveChanges, înaintea commit-ului tranzacției: rollback complet", scrisa && FaraEfecte(f.Id));
        var id = Partida(f.Id, ContFurnizor).Value;
        Refuza("SC-D9B-11", () => Comanda(os => {
            var o = os.CreateObject<C.OriginePartidaPersistata>();
            o.ID = id; o.ContId = Cont(ContFurnizor); o.PartenerId = Client; o.DocumentId = f.Id;
            ComenziDocument.Sistem(os).Opereaza(f.Id);
        }), CoduriRefuz.OriginePartidaInconsistenta);
        Verifica("SC-D9B-11", "refuzul materializării nu lasă efecte", FaraEfecte(f.Id));
        Opereaza(f.Id); Origini("SC-D9B-11", f.Id); Rest("SC-D9B-11", f.Id, -100);
        foreach (var aceeasiCheie in new[] { true, false })
            Verifica("SC-D9B-11", aceeasiCheie ? "PK refuză dublarea ID-ului" : "cheia naturală refuză alt ID pe aceeași origine", CuSpatiu(os => {
                var db = ((EFCoreObjectSpace)os).DbContext;
                using var tx = db.Database.BeginTransaction();
                try {
                    var duplicat = aceeasiCheie ? id : Guid.NewGuid();
                    db.Database.ExecuteSqlInterpolated($"""
                        INSERT INTO "OriginiPartide" ("ID", "ContId", "PartenerId", "DocumentId")
                        SELECT {duplicat}, "ContId", "PartenerId", "DocumentId" FROM "OriginiPartide" WHERE "ID" = {id}
                        """);
                    return false;
                }
                catch (PostgresException e) { return e.SqlState == PostgresErrorCodes.UniqueViolation
                    && e.ConstraintName == (aceeasiCheie ? "PK_OriginiPartide" : "IX_OriginiPartide_DocumentId_ContId_PartenerId"); }
                finally { tx.Rollback(); }
            }));
    }

    void Concurenta() {
        Titular("CONCURENT");
        var f = base.Factura(Ianuarie, new LinieFctScena(1, 100, Stoc: false));
        using var start = new Barrier(2);
        Task<string> Lanseaza() => Task.Run(() => {
            using var os = Deschide();
            if (!start.SignalAndWait(TimeSpan.FromSeconds(20))) throw new TimeoutException("Bariera D9B");
            try { ComenziDocument.Sistem(os).Opereaza(f.Id); return "operat"; }
            catch (OperareException e) { return e.Message; }
        });
        var a = Lanseaza(); var b = Lanseaza();
        var rezultate = Task.WhenAll(a, b).GetAwaiter().GetResult();
        Verifica("SC-D9B-11", "două comenzi simultane: una operează, cealaltă refuză starea", rezultate.Count(x => x == "operat") == 1
            && rezultate.Count(x => x.Contains("operat", StringComparison.OrdinalIgnoreCase)) == 2);
        Origini("SC-D9B-11", f.Id); Rest("SC-D9B-11", f.Id, -100);
        Verifica("SC-D9B-11", "concurența lasă o singură tranzacție", CuSpatiu(os =>
            os.GetObjectsQuery<C.Tranzactie>().Count(t => t.DocumentId == f.Id) == 1));
    }
}
