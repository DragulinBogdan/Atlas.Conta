using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Motor;
using Atlas.Conta.BackOffice.Module.Proiectii;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.EFCore;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Conta.BackOffice.ModelCheck;

sealed class ScenariiCitiri(Func<IObjectSpace> deschide, Action<string, bool> check,
    bool privat, Action<IObjectSpace, int, int> inchide)
    : ScenaDocumente(deschide, check, privat, inchide, "CITIRI", 2020) {
    protected override void Executa() {
        var f = Receptioneaza(new LinieFctScena(10, 10));
        var l = f.Linii[0];
        var btr = Iesire(true, (l, 4));
        Opereaza(btr.Id);
        Verifica("SC-CIT-31", "transferul păstrează separat sursa 6/60 și destinația 4/40", CuSpatiu(os => {
            var solduri = Atlas.Conta.BackOffice.Module.Cub.Citiri.Loturi.Solduri(os, new(An, 1, 11))
                .Where(s => s.LotId == l.Lot.Value).ToList();
            return solduri.Count == 2 && solduri.Any(s => s.GestiuneId == Magazie && s.Cantitate == 6 && s.Valoare == 60)
                && solduri.Any(s => s.GestiuneId != Magazie && s.Cantitate == 4 && s.Valoare == 40);
        }));
        Storneaza(btr.Id, new(An, 1, 12));
        Verifica("SC-CIT-31", "inversa transferului reface sursa 10/100 și elimină poziția zero", CuSpatiu(os => {
            var solduri = Atlas.Conta.BackOffice.Module.Cub.Citiri.Loturi.Solduri(os, new(An, 1, 31))
                .Where(s => s.LotId == l.Lot.Value).ToList();
            return solduri.Count == 1 && solduri[0].GestiuneId == Magazie
                && solduri[0].Cantitate == 10 && solduri[0].Valoare == 100;
        }));
        VerificaBalanta("SC-CIT-11", l.Produs!.Value, Ianuarie, new(An, 1, 31), 0, 100, 0, 100);
        Verifica("SC-CIT-11", "gestiunea se filtrează separat de partener", CuSpatiu(os => {
            var a = ContabilProiectii.Balanta(os, Ianuarie, new(An, 1, 31),
                materialId: l.Produs, gestiuneId: Magazie).Single();
            return a.ContId == Cont(Stoc) && a.RulajDebit == 100 && a.RulajCredit == 0;
        }));
        if (Privat)
            Verifica("SC-CIT-11", "sold furnizor 100 pe coordonata partener", CuSpatiu(os =>
                ContabilProiectii.SoldParteneri(os, new(An, 1, 31), Cont(ContFurnizor),
                    repartitorId: Furnizor, materialId: l.Produs).Single().SoldCreditor == 100));

        InchideIanuarie();
        var consum = Consum(l.Lot!.Value, 2);
        Comanda(os => {
            var d = os.GetObjectByKey<BonConsum>(consum);
            d.Data = Februarie; d.DataInregistrare = Februarie; os.CommitChanges();
        });
        Opereaza(consum);
        Verifica("SC-CIT-32", "consumul separă 8/80 în magazie de 2/20 pe cheltuială", CuSpatiu(os => {
            var solduri = Atlas.Conta.BackOffice.Module.Cub.Citiri.Loturi.Solduri(os, Februarie)
                .Where(s => s.LotId == l.Lot.Value).ToList();
            var disponibil = Atlas.Conta.BackOffice.Module.Cub.Citiri.Loturi.Disponibile(os, Februarie,
                l.Produs.Value, Magazie, Cont(Stoc)).Single(s => s.LotId == l.Lot.Value);
            var consumat = solduri.Single(s => s.GestiuneId != Magazie);
            return solduri.Count == 2 && disponibil.Cantitate == 8 && disponibil.Valoare == 80
                && consumat.Cantitate == 2 && consumat.Valoare == 20 && consumat.ContId != Cont(Stoc)
                && !Atlas.Conta.BackOffice.Module.Cub.Citiri.Loturi.Disponibile(os, Februarie,
                    l.Produs.Value, consumat.GestiuneId, consumat.ContId).Any();
        }));
        Verifica("SC-CIT-16", "fișa: credit 20, sold 80 și contrapartidă", CuSpatiu(os => {
            var r = ContabilProiectii.FisaCont(os, Cont(Stoc), Februarie, new(An, 2, 28),
                materialId: l.Produs, gestiuneId: Magazie).Single();
            return r.Debit == 0 && r.Credit == 20 && r.SoldCurent == 80
                && r.ContrapartidaId != null && r.ContrapartidaSimbol != null;
        }));
        Verifica("SC-CIT-17", "jurnal pe postări: debit 120 și credit 120 fără BTR", CuSpatiu(os => {
            var r = ContabilProiectii.RegistruJurnal(os, Ianuarie, new(An, 2, 28)).ToList();
            return r.Count == 4 && r.Sum(p => p.Debit) == 120 && r.Sum(p => p.Credit) == 120
                && r.Select(p => (p.Id, p.Spatiu)).Distinct().Count() == 4
                && r.Select(p => p.TranzactieId).Distinct().Count() == 2;
        }));
        VerificaBalanta("SC-CIT-12", l.Produs!.Value, Februarie, new(An, 2, 28), 100, 0, 20, 80);
        Comanda(os => {
            var db = ((EFCoreObjectSpace)os).DbContext;
            using var tx = db.Database.BeginTransaction();
            db.Database.ExecuteSqlInterpolated($"DELETE FROM \"SolduriPerioadaContabil\" WHERE \"An\" = {An}");
            var r = ContabilProiectii.Balanta(os, Februarie, new(An, 2, 28), materialId: l.Produs)
                .Single(x => x.ContId == Cont(Stoc));
            Verifica("SC-CIT-12", "fără snapshot: inițial 100, credit 20, final 80",
                r.InitialDebit == 100 && r.RulajCredit == 20 && r.SoldFinalDebit == 80);
            tx.Rollback();
        });
        Comanda(os => {
            var db = ((EFCoreObjectSpace)os).DbContext;
            using var tx = db.Database.BeginTransaction();
            var prima = SolduriService.Reconstruieste(os).Referinte.Single(r => r.An == An && r.Luna == 1);
            Verifica("SC-CIT-13", "reconstrucția păstrează sumele și coordonatele",
                prima.ContabilDiferite == 0 && prima.DiferentaDebit == 0 && prima.DiferentaCredit == 0);
            var repetata = SolduriService.Reconstruieste(os).Referinte.Single(r => r.An == An && r.Luna == 1);
            Verifica("SC-CIT-13", "reconstrucție repetată fără diferențe", repetata.ContabilDiferite == 0);
            db.Database.ExecuteSqlInterpolated($"UPDATE \"SolduriPerioadaContabil\" SET \"Debit\" = \"Debit\" + 7 WHERE \"An\" = {An} AND \"Luna\" = 1 AND \"ContId\" = {Cont(Stoc)}");
            var corupta = SolduriService.Reconstruieste(os).Referinte.Single(r => r.An == An && r.Luna == 1);
            Verifica("SC-CIT-14", "reconstrucția raportează diferența înainte de reparare",
                corupta.ContabilDiferite != 0 && corupta.DiferentaDebit > 0);
            var r = ContabilProiectii.Balanta(os, Februarie, new(An, 2, 28), materialId: l.Produs)
                .Single(x => x.ContId == Cont(Stoc));
            Verifica("SC-CIT-14", "reconstrucția permite citirea: final 80", r.SoldFinalDebit == 80);
            tx.Rollback();
        });
        if (Privat) {
            var faraNir = Factura(Februarie, new LinieFctScena(10, 10));
            var rezultat = Opereaza(faraNir.Id);
            Verifica("SC-CIT-33", "Suppliers: inițial 100, final 200 cu recepția FCT și NIR draft", CuSpatiu(os => {
                var saft = Atlas.Conta.BackOffice.Module.Saft.SaftProiectii.Saft(os, An, 2);
                var furnizor = saft.Furnizori.Single(p => p.PartenerId == Furnizor);
                var cont = saft.Conturi.Single(c => c.ContId == Cont(ContFurnizor));
                return os.GetObjectByKey<Document>(rezultat.ConexId!.Value).Stare == StareDocument.Draft
                    && furnizor.OpeningCreditBalance == 100 && furnizor.ClosingCreditBalance == 200
                    && cont.ClosingCreditBalance == 200;
            }));
        }
        EvaluareSuccesiva();
    }

    void EvaluareSuccesiva() {
        var f = Factura(Februarie, new LinieFctScena(3, 3.333333m));
        var rezultat = Opereaza(f.Id);
        if (rezultat.ConexId is Guid conex) Opereaza(conex);
        var lot = f.Linii[0].Lot!.Value;
        foreach (var valoare in new[] { 3.33m, 3.34m, 3.33m }) {
            var id = Consum(lot, 1);
            Comanda(os => {
                var d = os.GetObjectByKey<BonConsum>(id);
                d.Data = Februarie; d.DataInregistrare = Februarie; os.CommitChanges();
            });
            Opereaza(id);
            Verifica("SC-CIT-35", $"consum succesiv {valoare}", CuSpatiu(os =>
                Atlas.Conta.BackOffice.Module.Cub.Citiri.Contabil.Postari(os)
                    .Where(p => p.DocumentId == id && p.Cont == Cont(Stoc)).Sum(p => p.Valoare) == valoare));
        }
        Verifica("SC-CIT-35", "lot golit exact: 0/0", CuSpatiu(os =>
            !Atlas.Conta.BackOffice.Module.Cub.Citiri.Loturi.Solduri(os, Februarie)
                .Any(s => s.LotId == lot && s.ContId == Cont(Stoc) && s.GestiuneId == Magazie)));
    }

    void VerificaBalanta(string id, Guid produs, DateOnly deLa, DateOnly panaLa,
            decimal initial, decimal debit, decimal credit, decimal final) {
        var r = CuSpatiu(os => ContabilProiectii.Balanta(os, deLa, panaLa, materialId: produs)
            .Single(x => x.ContId == Cont(Stoc)));
        Verifica(id, $"stoc: inițial {initial}, D {debit}, C {credit}, final {final}",
            r.InitialDebit == initial && r.InitialCredit == 0 && r.RulajDebit == debit
            && r.RulajCredit == credit && r.SoldFinalDebit == final && r.SoldFinalCredit == 0);
    }
}
