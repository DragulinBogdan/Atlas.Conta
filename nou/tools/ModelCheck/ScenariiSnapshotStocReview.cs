using System.Diagnostics;
using System.Reflection;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Declaratii;
using Atlas.Conta.BackOffice.Module.Motor;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.EFCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using C = Atlas.Conta.BackOffice.Module.Cub;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.ModelCheck;

sealed partial class ScenariiSnapshotStoc {
    void EvaluareDinSnapshot(Guid lot) {
        var id = Consum(lot, 1);
        Comanda(os => {
            var doc = os.GetObjectByKey<Document>(id);
            doc.Data = Februarie;
            doc.DataInregistrare = Februarie;
            os.CommitChanges();
        });
        Comanda(os => {
            var db = ((EFCoreObjectSpace)os).DbContext;
            using var tx = db.Database.BeginTransaction();
            var doc = os.GetObjectByKey<Document>(id);
            decimal Evaluare() {
                var contract = Contractare.Contracteaza(os, doc);
                if (!contract.EsteAcceptat)
                    throw new InvalidOperationException(string.Join("; ", contract.Refuzuri.Select(Contractare.Mesaj)));
                return contract.Decizii.OfType<N.ValoareIesire>().Single().Valoare;
            }
            Verifica("SC-CIT-76", "evaluarea ieșirii pornește de la 4/40: valoare 10", Evaluare() == 10);
            db.Database.ExecuteSqlInterpolated($"""
                UPDATE "SolduriPerioadaStoc" SET "Valoare" = "Valoare" + 8
                WHERE "An" = {An} AND "Luna" = 1 AND "LotId" = {lot} AND "GestiuneId" = {Magazie}
                """);
            Verifica("SC-CIT-76", "snapshot 4/48: aceeași ieșire se evaluează la 12", Evaluare() == 12);
            var raport = SolduriService.Reconstruieste(os).Referinte.Single(r => r.An == An && r.Luna == 1);
            Verifica("SC-CIT-76", "reconstrucția raportează 8 și evaluarea revine la 10",
                raport.StocDiferite == 1 && raport.DiferentaValoare == 8 && Evaluare() == 10);
            tx.Rollback();
        });
    }

    void CitirePesteSchimbareaReferintei(Guid lot, Guid produs, Guid factura) {
        using var os = Deschide();
        var db = ((EFCoreObjectSpace)os).DbContext;
        using var comenzi = new NumarComenzi(db);
        var zi = new DateOnly(An, 2, 28);
        var partida = Partida(factura, ContFurnizor)!.Value;
        var cont = Cont(Stoc);
        var stoc = C.Citiri.Loturi.Cumulate(os, zi).Where(s => s.LotId == lot);
        var contabil = SolduriService.AtomiCumulati(os, zi, new(An, 1, 31))
            .Where(a => a.ContId == cont && a.MaterialId == produs);
        var partide = C.Citiri.Partide.Cumulate(os, zi).Where(p => p.UnitateId == partida);
        Verifica("SC-CIT-77", "compunerea celor trei citiri nu execută SQL", comenzi.Numar == 0);
        Comanda(altul => inchidePerioada(altul, An, 2));
        Verifica("SC-CIT-77", "cealaltă conexiune a eliminat referința ianuarie", CuSpatiu(altul =>
            !altul.GetObjectsQuery<SoldPerioadaStoc>().Any(s => s.An == An && s.Luna == 1)
            && !altul.GetObjectsQuery<SoldPerioadaContabil>().Any(s => s.An == An && s.Luna == 1)
            && !altul.GetObjectsQuery<PartidaDeschisa>().Any(s => s.An == An && s.Luna == 1)));
        var s = stoc.ToArray();
        Verifica("SC-CIT-77", "stoc: o instrucțiune, sursa 8/80 și consumul 2/20",
            comenzi.Numar == 1 && s.Length == 2
            && s.Any(r => r.GestiuneId == Magazie && r.Cantitate == 8 && r.Valoare == 80)
            && s.Any(r => r.GestiuneId == Loc && r.Cantitate == 2 && r.Valoare == 20));
        var a = contabil.ToArray();
        Verifica("SC-CIT-77", "contabil: o instrucțiune, graniță ianuarie, debit 100 / credit 20",
            comenzi.Numar == 2 && a.Sum(r => r.Debit) == 100 && a.Sum(r => r.Credit) == 20);
        var p = partide.ToArray();
        Verifica("SC-CIT-77", "partide: o instrucțiune, datoria facturii rămâne 100",
            comenzi.Numar == 3 && p.Length == 1 && p[0].Credit - p[0].Debit == 100);
    }

    void RefuzaScriereaSecurizata() {
        var os = DispatchProxy.Create<ISpatiuSnapshotSecurizat, SpatiuSnapshotInterzis>();
        (string Nume, Action Actiune)[] scrieri = [
            ("materializare", () => SolduriService.Materializeaza(os, An, 1)),
            ("partide", () => SolduriService.MaterializeazaPartide(os, An, 1)),
            ("reconstrucție", () => SolduriService.Reconstruieste(os)),
            ("eliminare", () => SolduriService.Elimina(os, An, 1))
        ];
        foreach (var (nume, actiune) in scrieri) {
            var refuzat = false;
            try { actiune(); }
            catch (InvalidOperationException e) when (e.Message.StartsWith("SNAPSHOT_OS_SECURIZAT:")) {
                refuzat = true;
            }
            Verifica("SC-CIT-78", nume + ": refuz înaintea oricărui acces la ObjectSpace", refuzat);
        }
    }

    sealed class NumarComenzi : IObserver<DiagnosticListener>,
            IObserver<KeyValuePair<string, object>>, IDisposable {
        readonly DbContext context;
        readonly List<IDisposable> abonamente = [];
        public int Numar { get; private set; }
        public NumarComenzi(DbContext context) {
            this.context = context;
            abonamente.Add(DiagnosticListener.AllListeners.Subscribe(this));
        }
        public void OnNext(DiagnosticListener value) {
            if (value.Name == "Microsoft.EntityFrameworkCore") abonamente.Add(value.Subscribe(this));
        }
        public void OnNext(KeyValuePair<string, object> value) {
            if (value.Key == RelationalEventId.CommandExecuting.Name
                    && value.Value is CommandEventData e && ReferenceEquals(e.Context, context)) Numar++;
        }
        public void OnCompleted() { }
        public void OnError(Exception error) { }
        public void Dispose() {
            foreach (var abonament in abonamente) abonament.Dispose();
        }
    }
}

public interface ISpatiuSnapshotSecurizat : IObjectSpace, ISecuredObjectSpace { }

public class SpatiuSnapshotInterzis : DispatchProxy {
    protected override object Invoke(MethodInfo targetMethod, object[] args) =>
        throw new InvalidOperationException("Acces neașteptat înaintea refuzului: " + targetMethod.Name);
}
