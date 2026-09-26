using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Motor;
using Atlas.Conta.BackOffice.Module.Proiectii;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.EFCore;
using Microsoft.EntityFrameworkCore;
using C = Atlas.Conta.BackOffice.Module.Cub;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.ModelCheck;

sealed partial class ScenariiSnapshotStoc(Func<IObjectSpace> deschide, Action<string, bool> check,
    bool privat, Action<IObjectSpace, int, int> inchide)
    : ScenaDocumente(deschide, check, privat, inchide, "SNAPSTOC", 1997) {
    readonly Action<IObjectSpace, int, int> inchidePerioada = inchide;
    protected override void CurataCubSuplimentar(IObjectSpace os, Purja pj) {
        var produse = os.GetObjectsQuery<Produs>().Where(p => p.Cod.StartsWith(Marcaj)).Select(p => p.ID);
        var ids = os.GetObjectsQuery<C.Postare>().Where(p => p.DocumentId == null
            && p.Produs != null && produse.Contains(p.Produs.Value)).Select(p => p.TranzactieId).Distinct().ToList();
        pj.AdaugaCheie<C.Postare>(os.GetObjectsQuery<C.Postare>().Where(p => ids.Contains(p.TranzactieId))
            .Select(p => p.ID).ToList());
        pj.AdaugaCheie<C.Tranzactie>(ids);
    }

    protected override void Executa() {
        var f = Factura(Ianuarie, new LinieFctScena(10, 10, Privat ? "SFD" : "CAP0"));
        var nir = Opereaza(f.Id).ConexId!.Value;
        var lot = f.Linii[0].Lot!.Value;
        var (initial, produsInitial) = CuSpatiu(os => {
            var p = os.CreateObject<Produs>();
            p.Cod = Marcaj + "-INITIAL";
            p.Denumire = p.Cod;
            p.UM = "BUC";
            p.TipMaterialId = Tip(os, Stoc);
            var l = os.CreateObject<Lot>();
            l.Produs = p;
            l.Data = new(An, 1, 1);
            l.GestiuneId = Magazie;
            l.PretUnitar = 10;
            os.CommitChanges();
            using var tx = TranzactieComanda.Incepe(os);
            var ancora = os.GetObjectsQuery<Cont>().First(c => c.Simbol.StartsWith("891")).ID;
            C.Materializare.Deschide(os, l.Data,
                [new(Cont(Stoc), N.Latura.Debit, 40, true), new(ancora, N.Latura.Credit, 40)],
                [new(Cont(Stoc), l.ID, Magazie, 4, 40)], []);
            os.CommitChanges();
            tx.Commit();
            return (l.ID, p.ID);
        });
        var btr = CuSpatiu(os => {
            var d = os.CreateObject<NotaTransfer>();
            d.Data = new(An, 1, 8);
            d.PredatorId = Magazie;
            d.PrimitorId = Destinatie;
            var l = os.CreateObject<DocumentDetaliu>();
            l.Document = d;
            l.TipMaterialId = Tip(os, Stoc);
            l.LotId = lot;
            l.Cantitate = 4;
            os.CommitChanges();
            return d.ID;
        });
        Opereaza(btr);
        Opereaza(Consum(lot, 2));
        Guid? reziduu = null;
        if (Privat) {
            var r = Receptioneaza(new LinieFctScena(3, 10.006667m, "SFD", Tip: "371")).Linii[0];
            reziduu = r.Lot;
            Opereaza(Iesire(false, (r, 1)).Id);
            Opereaza(Iesire(false, (r, 1)).Id);
            var retur = CuSpatiu(os => {
                var d = os.CreateObject<ReturFurnizor>();
                d.Data = Ianuarie;
                d.PredatorId = Magazie;
                d.PrimitorId = Furnizor;
                var l = os.CreateObject<DocumentDetaliu>();
                l.Document = d;
                l.TipMaterialId = Tip(os, "371");
                l.LotId = r.Lot;
                l.Cantitate = 1;
                l.TipTvaId = Tva("SFD");
                os.CommitChanges();
                return d.ID;
            });
            Opereaza(retur);
        }
        InchideIanuarie();
        EvaluareDinSnapshot(initial);
        RefuzaScriereaSecurizata();
        if (reziduu is { } rez) Verifica("SC-CIT-75",
            "reziduul 0/−0,01 rămâne în snapshot și raport, fără disponibil FIFO", CuSpatiu(os => {
            var s = os.GetObjectsQuery<SoldPerioadaStoc>().Single(s => s.An == An && s.Luna == 1 && s.LotId == rez
                && s.GestiuneId == Magazie);
            var r = StocProiectii.SoldStoc(os, new(An, 1, 31)).Single(s => s.LotId == rez && s.RepartitorId == Magazie);
            return s.Cantitate == 0 && s.Valoare == -.01m && r.Cantitate == 0 && r.Valoare == -.01m
                && r.LotPretUnitar == 0
                && !C.Citiri.Loturi.Disponibile(os, Februarie, s.ProdusId, Magazie, Cont("371")).Any()
                && !C.Citiri.Loturi.Disponibile(os, Februarie, s.ProdusId, N.GestiuniVirtuale.Client, Cont("607"))
                    .Any();
        }));
        var cheltuiala = Cont(Privat ? "602" : "602.01.00");
        Verifica("SC-CIT-69", "snapshot include FCT cu NIR draft și deschiderea fără registru", CuSpatiu(os =>
            os.GetObjectByKey<NIR>(nir).Stare == StareDocument.Draft
            && !os.GetObjectsQuery<RegistruStoc>().Any(r => r.DocumentId == nir || r.LotId == initial)
            && os.GetObjectsQuery<SoldPerioadaStoc>().Any(s => s.An == An && s.Luna == 1
                && s.LotId == initial && s.ContId == Cont(Stoc) && s.ProdusId == produsInitial
                && s.GestiuneId == Magazie && s.Cantitate == 4 && s.Valoare == 40
                    && s.Deschisa == new DateOnly(An, 1, 1))));
        Verifica("SC-CIT-70", "același lot: stoc 4/40 + 4/40 și cheltuială 2/20", CuSpatiu(os => {
            var s = os.GetObjectsQuery<SoldPerioadaStoc>().Where(s => s.An == An && s.Luna == 1
                && s.LotId == lot).ToArray();
            return s.Length == 3
                && s.Any(s => s.ContId == Cont(Stoc) && s.GestiuneId == Magazie && s.Cantitate == 4 && s.Valoare == 40)
                && s.Any(s => s.ContId == Cont(Stoc) && s.GestiuneId == Destinatie && s.Cantitate == 4
                    && s.Valoare == 40)
                && s.Any(s => s.ContId == cheltuiala && s.GestiuneId == Loc && s.Cantitate == 2 && s.Valoare == 20);
        }));
        Coincid("SC-CIT-70", new(An, 1, 31));
        Verifica("SC-CIT-69", "citirea registrului dual nu preia snapshot-ul cubului", CuSpatiu(os =>
            StocService.SolduriLaData(os, [lot], new(An, 1, 31))
                .Where(s => s.Key.RepartitorId == Magazie).Sum(s => s.Value.Cantitate) == -6));
        Verifica("SC-CIT-73", "FIFO păstrează data deschiderii din snapshot", CuSpatiu(os =>
            C.Citiri.Loturi.Disponibile(os, Februarie, produsInitial, Magazie, Cont(Stoc))
                .Single().Deschisa == new DateOnly(An, 1, 1)));
        Verifica("SC-CIT-73", "excluderea facturii nu rămâne ascunsă în snapshot", CuSpatiu(os => {
            var s = C.Citiri.Loturi.Cumulate(os, Februarie, f.Id).Single(s => s.LotId == lot
                && s.GestiuneId == Magazie && s.ContId == Cont(Stoc));
            return s.Cantitate == -6 && s.Valoare == -60;
        }));
        Storneaza(btr, Februarie);
        Verifica("SC-CIT-71", "storno peste închidere: sursa 8/80, destinația zero, ianuarie intact", CuSpatiu(os => {
            var s = C.Citiri.Loturi.Cumulate(os, Februarie).Where(s => s.LotId == lot).ToArray();
            return s.Length == 2 && s.Any(s => s.ContId == Cont(Stoc) && s.GestiuneId == Magazie && s.Cantitate == 8
                && s.Valoare == 80)
                && s.Any(s => s.ContId == cheltuiala && s.GestiuneId == Loc && s.Cantitate == 2 && s.Valoare == 20)
                && os.GetObjectsQuery<SoldPerioadaStoc>().Count(s => s.An == An && s.Luna == 1 && s.LotId == lot) == 3;
        }));
        Coincid("SC-CIT-71", Februarie);
        var consum = Consum(initial, 4);
        Comanda(os => {
            os.GetObjectByKey<Document>(consum).Data = Februarie;
            os.CommitChanges();
        });
        Opereaza(consum);
        Verifica("SC-CIT-73", "deschiderea se golește cu valoarea 40 după închidere", CuSpatiu(os =>
            !C.Citiri.Loturi.Disponibile(os, Februarie, produsInitial, Magazie, Cont(Stoc)).Any()
            && os.GetObjectByKey<Document>(consum).Detalii.Single().Valoare == 40));
        CitirePesteSchimbareaReferintei(lot, f.Linii[0].Produs!.Value, f.Id);
        Coincid("SC-CIT-71", new(An, 2, 28));
        Comanda(os => {
            using var tx = TranzactieComanda.Incepe(os);
            var raport = SolduriService.Reconstruieste(os);
            Verifica("SC-CIT-71", "incrementala coincide cu reconstrucția integrală",
                raport.Referinte.All(r => r.StocDiferite == 0));
            os.CommitChanges();
            tx.Commit();
        });
        foreach (var data in new[] { false, true }) Comanda(os => {
            using var tx = TranzactieComanda.Incepe(os);
            var db = ((EFCoreObjectSpace)os).DbContext;
            var id = os.GetObjectsQuery<SoldPerioadaStoc>().Where(s => s.An == An && s.Luna == 2 && s.LotId == lot
                && s.GestiuneId == Magazie).Select(s => s.ID).Single();
            if (data) db.Database.ExecuteSqlInterpolated($"""
                UPDATE "SolduriPerioadaStoc" SET "Deschisa" = "Deschisa" + 1 WHERE "ID" = {id}
                """);
            else db.Database.ExecuteSqlInterpolated($"""
                UPDATE "SolduriPerioadaStoc" SET "Valoare" = "Valoare" + 7 WHERE "ID" = {id}
                """);
            var citit = C.Citiri.Loturi.Cumulate(os, new(An, 2, 28))
                .Single(s => s.LotId == lot && s.GestiuneId == Magazie);
            Verifica("SC-CIT-72", "proba-capcană confirmă folosirea snapshot-ului înaintea reconstrucției",
                data ? citit.Deschisa == Ianuarie.AddDays(1) : citit.Valoare == 87);
            var r = SolduriService.Reconstruieste(os).Referinte.Single(r => r.An == An && r.Luna == 2);
            Verifica("SC-CIT-72", data ? "data alterată este raportată fără diferență de bani"
                : "+7 este raportat înainte de rescriere",
                r.StocDiferite == 1 && r.DiferentaValoare == (data ? 0 : 7) && r.DiferentaCantitate == 0);
            Verifica("SC-CIT-72", "reconstrucția repetată are zero diferențe",
                SolduriService.Reconstruieste(os).Referinte.All(r => r.StocDiferite == 0));
            os.CommitChanges();
            tx.Commit();
        });
        Comanda(os => {
            var db = ((EFCoreObjectSpace)os).DbContext;
            using var tx = db.Database.BeginTransaction();
            db.Database.ExecuteSqlInterpolated($"DELETE FROM \"SolduriPerioadaStoc\" WHERE \"An\" = {An}");
            Verifica("SC-CIT-73", "snapshot absent: citire integrală din cub", C.Citiri.Loturi.Cumulate(os, Februarie)
                .Single(s => s.LotId == lot && s.ContId == Cont(Stoc) && s.GestiuneId == Magazie).Valoare == 80);
            tx.Rollback();
        });
    }

    void Coincid(string id, DateOnly zi) => Verifica(id,
        "direct, snapshot + fereastră și raport au aceleași coordonate și măsuri", CuSpatiu(os => {
        var direct = C.Citiri.Loturi.Solduri(os, zi).ToArray();
        var cumul = C.Citiri.Loturi.Cumulate(os, zi).ToArray();
        var raport = StocProiectii.SoldStoc(os, zi).ToArray();
        return direct.Length == cumul.Length && direct.All(d => cumul.Contains(d))
            && raport.Length == direct.Length && direct.All(d => raport.Any(r => r.LotId == d.LotId
                && r.ContId == d.ContId && r.ProdusId == d.ProdusId && r.RepartitorId == d.GestiuneId
                && r.LotData == d.Deschisa && r.Cantitate == d.Cantitate && r.Valoare == d.Valoare));
    }));
}
