using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Declaratii;
using DevExpress.ExpressApp;

namespace Atlas.Conta.BackOffice.ModelCheck;

sealed partial class ScenariiCvl {
    string Fabricat => Privat ? "345" : "303.01.00";

    void Perimetru() {
        var transferat = Lot();
        Opereaza(Iesire(true, (new LinieScena(transferat.Linie, transferat.Lot, null), 10m)).Id);
        Refuza("SC-CVL-19", () => Opereaza(Draft(Zi(15), (transferat, 200m))), CoduriRefuz.CvLotNeeligibil);

        var asamblat = Lot();
        var produs = Asambleaza(asamblat, 10m, 100m);
        Refuza("SC-CVL-19", () => Opereaza(Draft(Zi(15), (asamblat, 200m))), CoduriRefuz.CvLotNeeligibil);
        Refuza("SC-CVL-19", () => Opereaza(Draft(Zi(15), (os, _, d) => d.TipMaterialId = Tip(os, Fabricat), (produs, 50m))),
            CoduriRefuz.CvLotNeeligibil);

        var inventariat = Lot();
        Opereaza(Inventar(inventariat, 5m));
        Refuza("SC-CVL-19", () => Opereaza(Draft(Zi(15), (inventariat, 200m))), CoduriRefuz.CvLotNeeligibil);
        Este("SC-CVL-19", "refuzurile pe asamblare și inventar nu lasă efecte", inventariat, 95m, 950m, 0m, 1000m);

        if (!Privat)
            return;
        var returnat = Lot();
        Opereaza(Retur(returnat, 10m, Zi(10)));
        Refuza("SC-CVL-19", () => Opereaza(Draft(Zi(15), (returnat, 200m))), CoduriRefuz.CvLotNeeligibil);

        var readus = Lot();
        Opereaza(ReturClient(readus, 1m, Zi(10)));
        Refuza("SC-CVL-19", () => Opereaza(Draft(Zi(15), (readus, 200m))), CoduriRefuz.CvLotNeeligibil);

        var constatat = Lot(constatat: 99m);
        Refuza("SC-CVL-19", () => Opereaza(Draft(Zi(15), (constatat, 200m))), CoduriRefuz.CvLotNeeligibil);

        var corectat = Lot();
        var a = Corectie(corectat, 200m);
        Refuza("SC-CVL-19", () => Opereaza(Retur(corectat, 100m, Zi(20))), CoduriRefuz.CvDependenti);
        Refuza("SC-CVL-19", () => Opereaza(ReturClient(corectat, 1m, Zi(20))), CoduriRefuz.CvDependenti);
        Este("SC-CVL-19", "returul la prețul lotului nu lasă 200 pe stocul gol", corectat, 100m, 1200m, 0m, 1200m);
        Storneaza(a, Zi(20));
        Opereaza(Retur(corectat, 20m, Zi(25)));
        Este("SC-CVL-19", "după storno corecției, returul trece la prețul lotului", corectat, 80m, 800m, 0m, 1000m);
    }

    Caz Asambleaza(Caz caz, decimal cantitate, decimal valoare) {
        var (id, linie, lot) = CuSpatiu(os => {
            var d = os.CreateObject<Asamblare>(); d.Data = Zi(10); d.DataInregistrare = d.Data;
            d.PredatorId = Magazie; d.PrimitorId = Magazie;
            var consum = os.CreateObject<AsamblareDetaliu>(); consum.Document = d; consum.Pozitie = 1;
            consum.Directie = DirectieAsamblare.Consum; consum.LotId = caz.Lot; consum.Cantitate = cantitate;
            consum.TipMaterialId = Tip(os, Stoc);
            var l = os.CreateObject<AsamblareDetaliu>(); l.Document = d; l.Pozitie = 2;
            l.Directie = DirectieAsamblare.Produs; l.TipMaterialId = Tip(os, Fabricat); l.Cantitate = 1m; l.PretEvaluare = valoare;
            var p = os.CreateObject<Produs>(); p.Cod = $"{Marcaj}-ASM{++numar}"; p.Denumire = p.Cod; p.UM = "BUC";
            p.TipMaterialId = l.TipMaterialId; l.ProdusId = p.ID;
            var lot = l.CreeazaLot(os, p, os.GetObjectByKey<Gestiune>(Magazie));
            os.CommitChanges(); return (d.ID, l.ID, lot.ID);
        });
        Opereaza(id);
        caz.Documente.Add(id);
        return new(id, Guid.Empty, linie, lot, [id]);
    }

    Guid Inventar(Caz caz, decimal lipsa) => CuSpatiu(os => {
        os.GetObjectByKey<Repartitor>(Loc).Calitati |= CalitateRepartitor.Comisie;
        var d = os.CreateObject<ListaDiferenteInventar>(); d.Data = Zi(10); d.DataInregistrare = d.Data;
        d.PredatorId = Magazie; d.PrimitorId = Loc;
        var l = os.CreateObject<ListaDiferenteInventarDetaliu>(); l.Document = d; l.Pozitie = 1;
        l.Directie = DirectieDiferenta.Minus; l.Cantitate = lipsa; l.LotId = caz.Lot;
        l.TipMaterialId = Tip(os, Stoc); l.CodEconomicId = Economic;
        os.CommitChanges(); return d.ID;
    });

    Guid Retur(Caz caz, decimal cantitate, DateOnly data) => CuSpatiu(os => {
        var d = os.CreateObject<ReturFurnizor>(); d.Data = data; d.PredatorId = Magazie; d.PrimitorId = Furnizor;
        var l = os.CreateObject<DocumentDetaliu>(); l.Document = d; l.Pozitie = 1;
        l.TipMaterialId = Tip(os, Stoc); l.LotId = caz.Lot; l.Cantitate = cantitate;
        os.CommitChanges(); return d.ID;
    });

    Guid ReturClient(Caz caz, decimal cantitate, DateOnly data) => CuSpatiu(os => {
        var d = os.CreateObject<ReturClient>(); d.Data = data; d.PredatorId = Client; d.PrimitorId = Magazie;
        var l = os.CreateObject<DocumentDetaliu>(); l.Document = d; l.Pozitie = 1;
        l.TipMaterialId = Tip(os, Stoc); l.LotId = caz.Lot; l.Cantitate = cantitate;
        os.CommitChanges(); return d.ID;
    });
}
