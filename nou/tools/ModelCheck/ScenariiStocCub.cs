using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Motor;
using Atlas.Conta.BackOffice.Module.Proiectii;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.EFCore;
using Microsoft.EntityFrameworkCore;
using C = Atlas.Conta.BackOffice.Module.Cub;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.ModelCheck;

sealed class ScenariiStocCub(Func<IObjectSpace> deschide, Action<string, bool> check,
    bool privat, Action<IObjectSpace, int, int> inchide)
    : ScenaDocumente(deschide, check, privat, inchide, "STOCCUB", 1999) {
    protected override void CurataCubSuplimentar(IObjectSpace os, Purja pj) {
        var produse = os.GetObjectsQuery<Produs>().Where(p => p.Cod.StartsWith(Marcaj)).Select(p => p.ID);
        var ids = os.GetObjectsQuery<C.Postare>().Where(p => p.DocumentId == null
            && p.Produs != null && produse.Contains(p.Produs.Value)).Select(p => p.TranzactieId).Distinct().ToList();
        pj.AdaugaCheie<C.Postare>(os.GetObjectsQuery<C.Postare>().Where(p => ids.Contains(p.TranzactieId)).Select(p => p.ID).ToList());
        pj.AdaugaCheie<C.Tranzactie>(ids);
    }

    protected override void Executa() {
        if (!Privat) return; // DSC este inert în pachetul bugetar.
        var (produs, primul, alDoilea) = CuSpatiu(os => {
            var p = os.CreateObject<Produs>(); p.Cod = Marcaj; p.Denumire = Marcaj;
            p.UM = "BUC"; p.TipMaterialId = Tip(os, "371");
            Lot Lot(DateOnly data, decimal pret) {
                var l = os.CreateObject<Lot>(); l.Produs = p; l.Data = data;
                l.GestiuneId = Magazie; l.PretUnitar = pret; return l;
            }
            var a = Lot(Ianuarie.AddDays(-2), 10); var b = Lot(Ianuarie.AddDays(-1), 20);
            os.CommitChanges();
            using var tx = TranzactieComanda.Incepe(os);
            var ancora = os.GetObjectsQuery<Cont>().First(c => c.Simbol.StartsWith("891")).ID;
            C.Materializare.Deschide(os, Ianuarie,
                [new(Cont("371"), N.Latura.Debit, 80, true), new(ancora, N.Latura.Credit, 80)],
                [new(Cont("371"), a.ID, Magazie, 2, 20), new(Cont("371"), b.ID, Magazie, 3, 60)], []);
            os.CommitChanges(); tx.Commit(); return (p.ID, a.ID, b.ID);
        });

        Guid Factura(bool pin) => CuSpatiu(os => {
            var f = os.CreateObject<FacturaIesire>(); f.Data = Ianuarie.AddDays(-1);
            f.DataInregistrare = Ianuarie; f.PredatorId = Loc; f.PrimitorId = Client;
            f.GestiuneDescarcareId = Magazie;
            void Linie(decimal q, Guid? lot = null) {
                var l = os.CreateObject<FacturaIesireDetaliu>(); l.Document = f;
                l.Pozitie = f.Detalii.Count; l.TipMaterialId = Tip(os, "371");
                l.ProdusId = produs; l.LotId = lot; l.Cantitate = q; l.PretUnitar = 30;
            }
            if (pin) { Linie(2, alDoilea); Linie(3); } else Linie(4);
            os.CommitChanges(); return f.ID;
        });
        var factura = Factura(false); var dsc = Opereaza(factura).ConexId!.Value;
        Verifica("SC-CIT-38", "FIFO din deschidere: 2 + 2 fără registru", CuSpatiu(os => {
            var linii = os.GetObjectByKey<DescarcareGestiune>(dsc).Detalii.ToList();
            return linii.Count == 2 && linii.Single(l => l.LotId == primul).Cantitate == 2
                && linii.Single(l => l.LotId == alDoilea).Cantitate == 2;
        }));
        Opereaza(dsc);
        Verifica("SC-CIT-38", "cost 60, disponibil 1/20", CuSpatiu(os => {
            var r = StocProiectii.SoldStoc(os, Ianuarie).Single(s => s.ProdusId == produs);
            return r.LotId == alDoilea && r.ContId == Cont("371") && r.Cantitate == 1 && r.Valoare == 20
                && C.Citiri.Contabil.Postari(os).Where(p => p.DocumentId == dsc && p.Cont == Cont("607")).Sum(p => p.Valoare) == 60;
        }));
        Comanda(os => {
            var db = ((EFCoreObjectSpace)os).DbContext;
            using var tx = db.Database.BeginTransaction();
            AscundereControlata.Ascunde(db, "Produse", produs);
            var r = StocProiectii.SoldStoc(os, Ianuarie).Single(s => s.ProdusId == produs);
            Verifica("SC-CIT-40", "eticheta ascunsă nu elimină soldul; cost unitar 20",
                r.ProdusCod == null && r.Cantitate == 1 && r.Valoare == 20 && r.LotPretUnitar == 20);
            tx.Rollback();
        });
        Anuleaza(dsc); Anuleaza(factura);
        foreach (var altCont in new[] { true, false }) Comanda(os => {
            var db = ((EFCoreObjectSpace)os).DbContext;
            using var tx = db.Database.BeginTransaction();
            if (altCont)
                db.Database.ExecuteSqlInterpolated($"UPDATE \"Postare\" SET \"Cont\" = {Cont(Stoc)} WHERE \"Unitate\" = {primul}");
            else
                db.Database.ExecuteSqlInterpolated($"UPDATE \"Postare\" SET \"Gestiune\" = {Destinatie} WHERE \"Unitate\" = {primul}");
            var draft = DescarcareService.Genereaza(os, os.GetObjectByKey<FacturaIesire>(factura), Ianuarie);
            Verifica("SC-CIT-39", altCont ? "FIFO exclude alt cont" : "FIFO exclude altă gestiune",
                draft != null && draft.Detalii.Count == 1
                && draft.Detalii.Single().LotId == alDoilea && draft.Detalii.Single().Cantitate == 3);
            tx.Rollback();
        });
        var cuPin = Factura(true); var dscPin = Opereaza(cuPin).ConexId!.Value;
        Verifica("SC-CIT-39", "pin prioritar, apoi FIFO: fără dublarea disponibilului", CuSpatiu(os => {
            var linii = os.GetObjectByKey<DescarcareGestiune>(dscPin).Detalii.ToList();
            return linii.Count == 3 && linii.Where(l => l.LotId == primul).Sum(l => l.Cantitate) == 2
                && linii.Where(l => l.LotId == alDoilea).Sum(l => l.Cantitate) == 3;
        }));
        Opereaza(dscPin);
        Verifica("SC-CIT-39", "golire 0/0; data înregistrării permite intrarea după data fizică", CuSpatiu(os =>
            !C.Citiri.Loturi.Solduri(os, Ianuarie).Any(s => s.ProdusId == produs)));
    }
}
