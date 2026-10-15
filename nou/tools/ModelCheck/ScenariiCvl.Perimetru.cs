using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Declaratii;
using Atlas.Conta.BackOffice.Module.Motor;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.EFCore;
using Microsoft.EntityFrameworkCore;
using C = Atlas.Conta.BackOffice.Module.Cub;
using N = Atlas.Conta.Nucleu;

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

        var tipGresit = Lot();
        Refuza("SC-CVL-18", () => Opereaza(Draft(Zi(15), (os, _, d) => d.TipMaterialId = Tip(os, Serviciu), (tipGresit, 200m))),
            CoduriRefuz.CvStructura);

        var dubla = Partial();
        var prima = Corectie(dubla, 200m);
        var aDoua = Corectie(dubla, 100m, 16);
        Refuza("SC-CVL-08", () => Anuleaza(prima), CoduriRefuz.CvDependenti);
        Anuleaza(aDoua);
        Anuleaza(prima);
        Este("SC-CVL-08", "corecția ulterioară se retrage întâi, apoi prima", dubla, 40m, 400m, 600m, 1000m);

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

        var reNir = Lot();
        Corectie(reNir, 200m);
        Anuleaza(reNir.Nir);
        foreach (var q in new[] { 98m, 102m }) {
            ConstatNir(reNir, q);
            Refuza("SC-CVL-19", () => Opereaza(reNir.Nir), CoduriRefuz.CvDependenti);
        }
        ConstatNir(reNir, 100m);
        Opereaza(reNir.Nir);
        Este("SC-CVL-19", "NIR-ul conex re-operat fără diferență după corecție", reNir, 100m, 1200m, 0m, 1200m);

        var corectat = Lot();
        var a = Corectie(corectat, 200m);
        var partial = Retur(corectat, 20m, Zi(20));
        Opereaza(partial);
        Este("SC-CVL-19", "returul la furnizor ia prețul ajustat", corectat, 80m, 960m, 0m, 960m);
        Refuza("SC-CVL-19", () => Storneaza(a, Zi(25)), CoduriRefuz.CvDependenti);
        var integral = Retur(corectat, 80m, Zi(20));
        Opereaza(integral);
        Este("SC-CVL-19", "returul integral golește lotul fără reziduu", corectat, 0m, 0m, 0m, 0m);
        Storneaza(integral, Zi(25));
        Storneaza(partial, Zi(25));
        Storneaza(a, Zi(25));
        Opereaza(Retur(corectat, 20m, Zi(25)));
        Este("SC-CVL-19", "după storno corecției, returul trece la prețul lotului", corectat, 80m, 800m, 0m, 800m);

        var cuTva = Lot();
        Corectie(cuTva, 200m);
        var tva = CuSpatiu(os => os.GetObjectsQuery<TipTva>().First(t => t.Regim == RegimTva.Normal && t.Cota > 0m).ID);
        var fiscal = Retur(cuTva, 20m, Zi(20), tva);
        Opereaza(fiscal);
        var (valoare, taxa) = CuSpatiu(os => os.GetObjectsQuery<DocumentDetaliu>().Where(d => d.DocumentId == fiscal)
            .Select(d => new { d.Valoare, d.ValoareTva }).ToList().Select(d => (d.Valoare, d.ValoareTva)).Single());
        var baza = CuSpatiu(os => os.GetObjectsQuery<C.Postare>()
            .Where(p => p.DocumentId == fiscal && p.RolTva == N.RolTva.Baza).ToList().Sum(p => p.Valoare));
        var iesire = Dto(fiscal, N.FelTranzactie.Operare).Origini.Single().Linii.Single().Iesiri.Single();
        Verifica("SC-CVL-19", $"returul cu TVA: baza fiscală e prețul facturii, cota corecției iese fără TVA (linie {valoare}, "
                + $"TVA {taxa}, baza {baza}, ieșire {iesire.Valoare} + cota {iesire.Corectie})",
            valoare == -200m && taxa < 0m && baza == -200m && iesire.Valoare == 200m && iesire.Corectie == 40m);
        Este("SC-CVL-19", "returul cu TVA scoate din stoc prețul ajustat", cuTva, 80m, 960m, 0m, 960m + taxa);
        foreach (var (cum, schimba) in new (string, Func<decimal, decimal>)[] { ("fără cotă", _ => 0m), ("cu altă cotă", c => c + 1m) })
            Verifica("SC-CVL-19", $"auditul refuză explicația returului {cum}", CotaMutata(fiscal, schimba));

        var vandut = Partial();
        var b = Corectie(vandut, 200m);
        var rdc = ReturClient(vandut, 10m, Zi(20));
        Opereaza(rdc);
        Este("SC-CVL-19", "returul de la client revine la prețul ajustat", vandut, 50m, 600m, 720m, 1200m);
        Refuza("SC-CVL-19", () => Storneaza(b, Zi(25)), CoduriRefuz.CvDependenti);
        Refuza("SC-CVL-19", () => Anuleaza(b), CoduriRefuz.CvDependenti);
        Storneaza(rdc, Zi(25));
        Storneaza(b, Zi(25));
        Este("SC-CVL-19", "returul de la client se stornează înaintea corecției", vandut, 40m, 400m, 600m, 1000m);
    }

    bool CotaMutata(Guid retur, Func<decimal, decimal> schimba) => CuSpatiu(os => {
        var db = ((EFCoreObjectSpace)os).DbContext;
        using var tx = db.Database.BeginTransaction();
        try {
            var t = os.GetObjectsQuery<C.Tranzactie>().Single(t => t.DocumentId == retur && t.Explicatie != null);
            var e = C.Explicatie.Citeste(t.Explicatie);
            var json = (e with { Decizii = [.. e.Decizii.Select(d => d is N.ValoareDeclarata v ? v with { Corectie = schimba(v.Corectie) } : d)] }).Scrie();
            db.Set<C.Tranzactie>().Where(x => x.ID == t.ID).ExecuteUpdate(s => s.SetProperty(x => x.Explicatie, json));
            C.Citiri.Invarianti.Verifica(os);
            return false;
        }
        catch (OperareException eroare) {
            return eroare.Message.Contains(C.Citiri.Explicatii.Iesire);
        }
        finally {
            tx.Rollback();
            db.ChangeTracker.Clear();
        }
    });

    void ConstatNir(Caz caz, decimal cantitate) => Comanda(os => {
        os.GetObjectByKey<NIR>(caz.Nir).Detalii.OfType<NirDetaliu>().Single().Cantitate = cantitate; os.CommitChanges();
    });

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

    Guid Retur(Caz caz, decimal cantitate, DateOnly data, Guid? tva = null) => Adauga(caz, CuSpatiu(os => {
        var d = os.CreateObject<ReturFurnizor>(); d.Data = data; d.PredatorId = Magazie; d.PrimitorId = Furnizor;
        var l = os.CreateObject<DocumentDetaliu>(); l.Document = d; l.Pozitie = 1;
        l.TipMaterialId = Tip(os, Stoc); l.LotId = caz.Lot; l.Cantitate = cantitate; l.TipTvaId = tva;
        os.CommitChanges(); return d.ID;
    }));

    Guid ReturClient(Caz caz, decimal cantitate, DateOnly data) => Adauga(caz, CuSpatiu(os => {
        var d = os.CreateObject<ReturClient>(); d.Data = data; d.PredatorId = Client; d.PrimitorId = Magazie;
        var l = os.CreateObject<DocumentDetaliu>(); l.Document = d; l.Pozitie = 1;
        l.TipMaterialId = Tip(os, Stoc); l.LotId = caz.Lot; l.Cantitate = cantitate;
        os.CommitChanges(); return d.ID;
    }));

    static Guid Adauga(Caz caz, Guid id) { caz.Documente.Add(id); return id; }
}
