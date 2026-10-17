using Atlas.Conta.BackOffice.Module.Api;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Declaratii;
using Atlas.Conta.BackOffice.Module.Motor;
using DevExpress.ExpressApp;
using N = Atlas.Conta.Nucleu;
using C = Atlas.Conta.BackOffice.Module.Cub;

namespace Atlas.Conta.BackOffice.ModelCheck;

/// <summary>E3-D10 pe calea reală: exercițiul închis din bază ajunge la corecția valorică prin `Fapte`.</summary>
sealed class ScenariiExCv(Func<IObjectSpace> deschide, Action<string, bool> check,
    bool privat, Action<IObjectSpace, int, int> inchide)
    : ScenaDocumente(deschide, check, privat, inchide, "EXCV", 1973) {
    protected override int UltimulAn => An + 1;

    protected override void Executa() {
        var f = Factura(Ianuarie, new LinieFctScena(100m, 10m));
        var nir = Opereaza(f.Id).ConexId ?? throw new InvalidOperationException("Recepția cere NIR conex.");
        Opereaza(nir);
        var lot = f.Linii[0].Lot!.Value;
        Opereaza(CuSpatiu(os => {
            var d = os.CreateObject<BonConsum>(); d.Data = new DateOnly(An, 1, 10);
            d.PredatorId = Magazie; d.PrimitorId = Loc;
            var l = os.CreateObject<DocumentDetaliu>(); l.Document = d;
            l.TipMaterialId = Tip(os, Stoc); l.LotId = lot; l.Cantitate = 60m;
            os.CommitChanges(); return d.ID;
        }));

        Comanda(os => {
            foreach (var (an, luna) in new[] { (An, 12), (UltimulAn, 1) }) {
                var p = os.CreateObject<PerioadaFiscala>(); p.An = an; p.Luna = luna;
            }
            os.CommitChanges();
        });
        var irz = CuSpatiu(os => {
            var d = InchidereRezultatService.Genereaza(os, An, 12, Loc)
                ?? throw new InvalidOperationException("Scena cere închiderea lui decembrie.");
            os.CommitChanges(); return d.ID;
        });
        Opereaza(irz);
        var rezultat = Cont(Privat ? "121" : "121.00.00");
        var (cuMaterial, ramase, destinatiiCuMaterial) = CuSpatiu(os => (
            os.GetObjectsQuery<InchidereRezultatDetaliu>().Count(l => l.DocumentId == irz && l.ProdusId != null),
            InchidereRezultatService.CalculeazaLinii(os, new DateOnly(An, 12, 31), cuAnuale: true).Count,
            os.GetObjectsQuery<C.Postare>().Count(p => p.DocumentId == irz && p.Cont == rezultat && p.Produs != null)));
        Verifica("SC-EX-04", $"cheia-sursă cu material se operează și se pliază pe 121: {cuMaterial} linii cu material, "
                + $"{ramase} chei rămase la 31.12, {destinatiiCuMaterial} postări pe 121 cu material",
            cuMaterial > 0 && ramase == 0 && destinatiiCuMaterial == 0);
        foreach (var luna in new[] { 1, 2, 12 })
            Inchide(An, luna);
        Comanda(os => PerioadaService.InchideExercitiul(os, An, [], null, "ModelCheck"));

        var corectie = CuSpatiu(os => {
            var doc = os.CreateObject<FacturaIntrare>();
            doc.Numar = $"{Marcaj}-C"; doc.Data = new DateOnly(UltimulAn, 1, 15);
            doc.PredatorId = Furnizor; doc.PrimitorId = Magazie;
            var d = os.CreateObject<FacturaIntrareDetaliu>();
            d.Document = doc; d.TipMaterialId = Tip(os, Stoc); d.CorectieValoareLot = true;
            d.LotId = lot; d.Valoare = 200m; d.CodEconomicId = Economic;
            os.CommitChanges(); return doc.ID;
        });
        RefuzaOperarea("SC-EX-26", corectie, CoduriRefuz.CvAnInchis);
        Verifica("SC-EX-26", "corecția refuzată pe consumul din exercițiul închis nu scrie nimic",
            CuSpatiu(os => !os.GetObjectsQuery<C.Postare>().Any(p => p.DocumentId == corectie)));

        Comanda(os => PerioadaService.RedeschideExercitiul(os, An, "SC-EX-26: ajustare", null, "ModelCheck"));
        Opereaza(corectie);
        var consum = CuSpatiu(os => os.GetObjectsQuery<C.Postare>()
            .Where(p => p.DocumentId == corectie && p.Carte == N.Carte.Contabil && p.Repartitor == Loc)
            .Sum(p => p.Sens == N.Sens.Debit ? p.Valoare : -p.Valoare));
        Verifica("SC-EX-26", $"cu exercițiul redeschis, consumul din anul anterior primește cota lui: 120 (obținut {consum})",
            consum == 120m);
    }
}
