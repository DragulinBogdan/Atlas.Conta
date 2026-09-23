using Atlas.Conta.BackOffice.Module.Api;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Declaratii;
using Atlas.Conta.BackOffice.Module.Motor;
using DevExpress.ExpressApp;
using C = Atlas.Conta.BackOffice.Module.Cub;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.ModelCheck;

record LinieRdcScena(decimal Valoare, string Tva = null, LinieScena Lot = null,
    decimal Cantitate = 1, string Tip = null);

sealed class ScenariiRdc(Func<IObjectSpace> deschide, Action<string, bool> check,
    bool privat, Action<IObjectSpace, int, int> inchide)
    : ScenaDocumente(deschide, check, privat, inchide, "RDC", 2008) {
    FacturaScena Retur(params LinieRdcScena[] linii) {
        using var os = Deschide();
        var d = os.CreateObject<ReturClient>(); d.Data = Ianuarie; d.PredatorId = Client; d.PrimitorId = Magazie;
        var rezultat = new List<LinieScena>();
        foreach (var spec in linii) {
            var l = os.CreateObject<DocumentDetaliu>(); l.Document = d; l.Pozitie = rezultat.Count + 1;
            l.TipMaterialId = Tip(os, spec.Tip ?? (spec.Lot != null ? "371" : Privat ? "704" : "751.01.00"));
            l.Cantitate = spec.Cantitate; l.Valoare = spec.Valoare; l.LotId = spec.Lot?.Lot;
            if (spec.Tva != null) l.TipTvaId = Tva(spec.Tva);
            rezultat.Add(new(l.ID, l.LotId, spec.Lot?.Produs));
        }
        os.CommitChanges(); return new(d.ID, rezultat.ToArray());
    }

    FacturaScena Vinde(decimal q, decimal pret, string tva = null, LinieScena lot = null) {
        using var os = Deschide();
        var d = os.CreateObject<FacturaIesire>(); d.Data = Ianuarie;
        d.PredatorId = Magazie; d.PrimitorId = Client;
        if (lot != null) d.GestiuneDescarcareId = Magazie;
        var l = os.CreateObject<FacturaIesireDetaliu>(); l.Document = d; l.Pozitie = 1;
        l.Cantitate = q; l.PretUnitar = pret; l.TipMaterialId = Tip(os, lot == null ? "704" : "371");
        l.LotId = lot?.Lot; l.ProdusId = lot?.Produs;
        if (tva != null) l.TipTvaId = Tva(tva);
        os.CommitChanges(); return new(d.ID, [new(l.ID, l.LotId, l.ProdusId)]);
    }

    Guid P(Guid d) => Partida(d, "4111", Client)!.Value;
    RandScena[] Venit(FacturaScena d, int i, decimal v, decimal taxa = 0,
            string tva = null, int? perioada = null, string cont = "704") {
        var tv = tva == null ? (Guid?)null : Tva(tva);
        var debit = new RandScena(Cont("4111"), N.Latura.Debit, v,
            Unitate: P(d.Id), Partener: Client, Linie: d.Linii[i].Id);
        var credit = new RandScena(Cont(cont), N.Latura.Credit, v, Gestiune: Magazie,
            Partener: tv != null ? Client : null, Linie: d.Linii[i].Id, Tva: tv,
            Rol: tv != null ? N.RolTva.Baza : null, Perioada: tv != null ? perioada ?? An * 100 + 1 : null,
            Sens: tv != null ? N.SensTva.Livrare : null);
        return taxa == 0 ? [debit, credit] : [debit, credit,
            debit with { Valoare = taxa }, credit with { Cont = Cont("4427"), Valoare = taxa, Rol = N.RolTva.Taxa }];
    }
    RandScena[] Cost(FacturaScena d, int i, decimal q, decimal v) {
        var l = d.Linii[i];
        return [new(Cont("607"), N.Latura.Debit, -v, -q, N.GestiuniVirtuale.Client,
                Produs: l.Produs, Partener: Client, Linie: l.Id),
            new(Cont("371"), N.Latura.Credit, -v, q, Magazie, l.Lot, l.Produs,
                Linie: l.Id, Spatiu: N.Spatiu.Stoc)];
    }

    protected override void Executa() {
        if (!Privat) {
            Verifica("SC-RDC-16", "profil fără politică RDC și fără activare cub", CuSpatiu(os =>
                !os.GetObjectsQuery<RegulaContare>().Any(r => r.TipDocument.Cod == "RDC")
                && !os.GetObjectsQuery<TipDocument>().Single(t => t.Cod == "RDC").PosteazaInCub));
            var inert = Retur(new LinieRdcScena(100));
            Refuza("SC-RDC-16", () => Opereaza(inert.Id), "politică de numerotare"); FaraEfecte("SC-RDC-16", inert.Id); return;
        }
        Venituri(); Stocuri(); Compensare(); Refuzuri(); PestePerioada();
    }

    void Venituri() {
        var d = Retur(new LinieRdcScena(100));
        Verifica("SC-RDC-01", "dry-run acceptat", CuSpatiu(os => OperareApi.Valideaza(os, d.Id)).Count == 0);
        FaraEfecte("SC-RDC-01", d.Id); Opereaza(d.Id);
        Postari("SC-RDC-01", d.Id, N.FelTranzactie.Operare, Ianuarie, Venit(d, 0, -100));
        SoldPartida("SC-RDC-01", P(d.Id), Ianuarie, -100);
        Storneaza(d.Id, new(An, 1, 20));
        Postari("SC-RDC-03", d.Id, N.FelTranzactie.Storno, new(An, 1, 20), Venit(d, 0, 100));
        SoldPartida("SC-RDC-03", P(d.Id), new(An, 1, 20), 0);
        var original = Amprenta(d.Id);
        Refuza("SC-RDC-03", () => Storneaza(d.Id, new(An, 1, 20)), "Operat");
        Verifica("SC-RDC-03", "repetare fără efecte", Amprenta(d.Id) == original);
        var a = Retur(new LinieRdcScena(100)); Opereaza(a.Id); Anuleaza(a.Id); FaraEfecte("SC-RDC-04", a.Id);
        Opereaza(a.Id); Postari("SC-RDC-04", a.Id, N.FelTranzactie.Operare, Ianuarie, Venit(a, 0, -100));
        var m = Retur(new LinieRdcScena(100, "N21"), new LinieRdcScena(50, "N11")); Opereaza(m.Id);
        Postari("SC-RDC-02", m.Id, N.FelTranzactie.Operare, Ianuarie,
            [.. Venit(m, 0, -100, -21, "N21"), .. Venit(m, 1, -50, -5.50m, "N11")]);
        SoldPartida("SC-RDC-02", P(m.Id), Ianuarie, -176.50m);
        var ti = Retur(new LinieRdcScena(100, "TI21")); Opereaza(ti.Id);
        Postari("SC-RDC-14", ti.Id, N.FelTranzactie.Operare, Ianuarie, Venit(ti, 0, -100, tva: "TI21"));
    }

    void Stocuri() {
        var lot = Receptioneaza(new LinieFctScena(10, 10, Tip: "371")).Linii[0];
        var f = Vinde(6, 20, "N21", lot); var dsc = Opereaza(f.Id).ConexId!.Value; Opereaza(dsc);
        var d = Retur(new LinieRdcScena(40, "N21", Tip: "707"), new LinieRdcScena(0, Lot: lot, Cantitate: 2));
        Opereaza(d.Id);
        Postari("SC-RDC-05", d.Id, N.FelTranzactie.Operare, Ianuarie,
            [.. Venit(d, 0, -40, -8.40m, "N21", cont: "707"), .. Cost(d, 1, 2, 20)]);
        SoldLot("SC-RDC-05", lot.Lot!.Value, Magazie, Ianuarie, 6, 60);
        SoldPartida("SC-RDC-05", P(f.Id), Ianuarie, 145.20m); SoldPartida("SC-RDC-05", P(d.Id), Ianuarie, -48.40m);
        Verifica("SC-RDC-05", "numai venitul intră în jurnalul TVA", CuSpatiu(os =>
            os.GetObjectsQuery<RegistruTva>().Count(r => r.DocumentId == d.Id) == 1));
        var rest = Retur(new LinieRdcScena(80, "N21", Tip: "707"), new LinieRdcScena(0, Lot: lot, Cantitate: 4));
        Opereaza(rest.Id);
        Postari("SC-RDC-06", rest.Id, N.FelTranzactie.Operare, Ianuarie,
            [.. Venit(rest, 0, -80, -16.80m, "N21", cont: "707"), .. Cost(rest, 1, 4, 40)]);
        SoldLot("SC-RDC-06", lot.Lot.Value, Magazie, Ianuarie, 10, 100);
        var ids = new[] { f.Id, dsc, d.Id, rest.Id };
        foreach (var cont in new[] { "707", "607", "4427" })
            Verifica("SC-RDC-06", "net zero pe " + cont, CuSpatiu(os => os.GetObjectsQuery<C.Postare>()
                .Where(p => p.DocumentId != null && ids.Contains(p.DocumentId.Value) && p.Cont == Cont(cont))
                .Sum(p => p.Latura == N.Latura.Debit ? p.Valoare : -p.Valoare)) == 0);
        var gol = Receptioneaza(new LinieFctScena(10, 10, Tip: "371")).Linii[0];
        Opereaza(Iesire(false, (gol, 10)).Id);
        var inapoi = Retur(new LinieRdcScena(0, Lot: gol, Cantitate: 2)); Opereaza(inapoi.Id);
        Postari("SC-RDC-07", inapoi.Id, N.FelTranzactie.Operare, Ianuarie, Cost(inapoi, 0, 2, 20));
        SoldLot("SC-RDC-07", gol.Lot!.Value, Magazie, Ianuarie, 2, 20);
        Storneaza(inapoi.Id, Ianuarie); SoldLot("SC-RDC-07", gol.Lot.Value, Magazie, Ianuarie, 0, 0);
        var doua = Receptioneaza(new LinieFctScena(10, 10, Tip: "371"), new LinieFctScena(10, 20, Tip: "371"));
        Opereaza(Iesire(false, (doua.Linii[0], 2), (doua.Linii[1], 3)).Id);
        var m = Retur(new LinieRdcScena(0, Lot: doua.Linii[0], Cantitate: 2), new LinieRdcScena(0, Lot: doua.Linii[1], Cantitate: 3));
        Opereaza(m.Id); Postari("SC-RDC-08", m.Id, N.FelTranzactie.Operare, Ianuarie, [.. Cost(m, 0, 2, 20), .. Cost(m, 1, 3, 60)]);
        var zero = Receptioneaza(new LinieFctScena(10, 0, Tip: "371")).Linii[0];
        var z = Retur(new LinieRdcScena(0, Lot: zero, Cantitate: 2)); Opereaza(z.Id);
        Postari("SC-RDC-15", z.Id, N.FelTranzactie.Operare, Ianuarie, Cost(z, 0, 2, 0));
        var dependent = Receptioneaza(new LinieFctScena(2, 10, Tip: "371")).Linii[0];
        Opereaza(Iesire(false, (dependent, 2)).Id);
        var sursa = Retur(new LinieRdcScena(0, Lot: dependent, Cantitate: 2)); Opereaza(sursa.Id);
        var consum = Iesire(false, (dependent, 2)); Opereaza(consum.Id);
        var intact = Amprenta(sursa.Id);
        Refuza("SC-RDC-17", () => Storneaza(sursa.Id, Ianuarie), "negativ");
        Verifica("SC-RDC-17", "refuz de stoc atomic", Amprenta(sursa.Id) == intact);
        Storneaza(consum.Id, Ianuarie); Storneaza(sursa.Id, Ianuarie);
        SoldLot("SC-RDC-17", dependent.Lot!.Value, Magazie, Ianuarie, 0, 0);
    }

    void Compensare() {
        Client = CuSpatiu(os => { var p = os.CreateObject<Partener>(); p.Cod = Marcaj + "-COMP";
            p.Denumire = p.Cod; os.CommitChanges(); return p.ID; });
        var f = Vinde(1, 100); Opereaza(f.Id);
        var inc = Trezorerie(true, 40); Opereaza(inc.Id); Imperecheaza(inc.Id, f.Id, 40, Ianuarie);
        var d = Retur(new LinieRdcScena(30)); Opereaza(d.Id);
        SoldPartida("SC-RDC-09", P(f.Id), Ianuarie, 60); SoldPartida("SC-RDC-09", P(d.Id), Ianuarie, -30);
        var n = Nota(Ianuarie, new LinieNtcScena("4111", "4111", 30, Client, Client)); Opereaza(n.Id);
        SoldPartida("SC-RDC-09", P(f.Id), Ianuarie, 30); SoldPartida("SC-RDC-09", P(d.Id), Ianuarie, 0);
        var intact = Amprenta(d.Id);
        Refuza("SC-RDC-09", () => Storneaza(d.Id, Ianuarie), "PARTIDA_CU_DEPENDENTI");
        Verifica("SC-RDC-09", "refuz atomic", Amprenta(d.Id) == intact);
        Storneaza(n.Id, Ianuarie);
        SoldPartida("SC-RDC-09", P(f.Id), Ianuarie, 60); SoldPartida("SC-RDC-09", P(d.Id), Ianuarie, -30);
    }

    void Refuzuri() {
        var lot = Receptioneaza(new LinieFctScena(10, 10, Tip: "371")).Linii[0];
        var capitalizat = CuSpatiu(os => os.GetObjectsQuery<TipTva>().First(t => t.Regim == RegimTva.Capitalizat).Cod);
        var externul = Retur(new LinieRdcScena(100));
        Comanda(os => { os.GetObjectByKey<Document>(externul.Id).PredatorId = Loc; os.CommitChanges(); });
        foreach (var (d, cod, mesaj) in new[] {
            (Retur(), CoduriRefuz.LiniiLipsa, "linie"),
            (Retur(new LinieRdcScena(0)), CoduriRefuz.ValoareZero, "valoarea"),
            (Retur(new LinieRdcScena(100, Tip: "371")), CoduriRefuz.NaturaNepotrivita, "venit"),
            (Retur(new LinieRdcScena(0, Lot: lot, Cantitate: 0)), CoduriRefuz.CantitateZero, "zero"),
            (Retur(new LinieRdcScena(100, capitalizat)), CoduriRefuz.TvaCapitalizat, "capitalizat"),
            (externul, CoduriRefuz.PredatorNepotrivit, CoduriRefuz.PredatorNepotrivit) }) {
            RefuzDeclaratie("SC-RDC-13", d.Id, cod);
            Refuza("SC-RDC-13", () => Opereaza(d.Id), mesaj); FaraEfecte("SC-RDC-13", d.Id);
        }
    }

    void PestePerioada() {
        var lot = Receptioneaza(new LinieFctScena(10, 10, Tip: "371")).Linii[0];
        Opereaza(Iesire(false, (lot, 4)).Id);
        var p = Retur(new LinieRdcScena(100, "N21"), new LinieRdcScena(0, Lot: lot, Cantitate: 2)); Opereaza(p.Id);
        var c = Retur(new LinieRdcScena(100, "N21"), new LinieRdcScena(0, Lot: lot, Cantitate: 2)); Opereaza(c.Id);
        var t = Retur(new LinieRdcScena(100));
        Comanda(os => { os.GetObjectByKey<Document>(t.Id).DataInregistrare = Februarie; os.CommitChanges(); });
        InchideIanuarie(); Refuza("SC-RDC-10", () => Anuleaza(p.Id), "închis");
        Storneaza(p.Id, Februarie);
        Postari("SC-RDC-10", p.Id, N.FelTranzactie.Storno, Februarie,
            [.. Venit(p, 0, 100, 21, "N21", An * 100 + 2), .. Cost(p, 1, -2, -20)]);
        SoldPartida("SC-RDC-10", P(p.Id), new(An, 1, 31), -121); SoldPartida("SC-RDC-10", P(p.Id), Februarie, 0);
        var nou = Corecteaza(c.Id); FaraEfecte("SC-RDC-11", nou);
        var corectie = CuSpatiu(os => {
            var d = os.GetObjectByKey<Document>(nou);
            Verifica("SC-RDC-11", "corecție legată, date și motiv", d.CorecteazaId == c.Id
                && d.MotivCorectie == MotivCorectie.EroareMateriala && d.Data == Ianuarie && d.DataInregistrare == Februarie);
            var l = os.GetObjectsQuery<DocumentDetaliu>().Single(l => l.DocumentId == nou && l.LotId == null);
            var cost = os.GetObjectsQuery<DocumentDetaliu>().Single(l => l.DocumentId == nou && l.LotId != null);
            l.Valoare = 80; l.ValoareTva = 0; cost.Cantitate = 1;
            os.CommitChanges(); return new FacturaScena(nou, [new(l.ID, null, null), new(cost.ID, lot.Lot, lot.Produs)]);
        });
        Opereaza(nou);
        Postari("SC-RDC-11", c.Id, N.FelTranzactie.Storno, Februarie, [.. Venit(c, 0, 100, 21, "N21"), .. Cost(c, 1, -2, -20)]);
        Postari("SC-RDC-11", nou, N.FelTranzactie.Operare, Februarie,
            [.. Venit(corectie, 0, -80, -16.80m, "N21"), .. Cost(corectie, 1, 1, 10)]);
        SoldPartida("SC-RDC-11", P(nou), Februarie, -96.80m);
        SoldLot("SC-RDC-18", lot.Lot!.Value, Magazie, new(An, 1, 31), 10, 100);
        SoldLot("SC-RDC-18", lot.Lot.Value, Magazie, Februarie, 7, 70);
        Opereaza(t.Id); Postari("SC-RDC-12", t.Id, N.FelTranzactie.Operare, Februarie, Venit(t, 0, -100));
        SoldPartida("SC-RDC-12", P(t.Id), new(An, 1, 31), 0); SoldPartida("SC-RDC-12", P(t.Id), Februarie, -100);
    }
}
