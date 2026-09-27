using Atlas.Conta.BackOffice.Module.Api;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Declaratii;
using DevExpress.ExpressApp;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.ModelCheck;

record LinieRlfScena(LinieScena Lot, decimal Cantitate, string Tva = null, string Tip = null);

sealed class ScenariiRlf(Func<IObjectSpace> deschide, Action<string, bool> check,
    bool privat, Action<IObjectSpace, int, int> inchide)
    : ScenaDocumente(deschide, check, privat, inchide, "RLF", 2009) {
    FacturaScena Retur(params LinieRlfScena[] linii) {
        using var os = Deschide();
        var d = os.CreateObject<ReturFurnizor>(); d.Data = Ianuarie; d.PredatorId = Magazie; d.PrimitorId = Furnizor;
        var rezultat = new List<LinieScena>();
        foreach (var spec in linii) {
            var l = os.CreateObject<DocumentDetaliu>(); l.Document = d; l.Pozitie = rezultat.Count + 1;
            l.TipMaterialId = Tip(os, spec.Tip ?? (Privat ? "371" : "302.01.00"));
            l.Cantitate = spec.Cantitate; l.LotId = spec.Lot?.Lot;
            if (spec.Tva != null) l.TipTvaId = Tva(spec.Tva);
            rezultat.Add(new(l.ID, l.LotId, spec.Lot?.Produs));
        }
        os.CommitChanges(); return new(d.ID, rezultat.ToArray());
    }

    Guid P(Guid d) => Partida(d, "401")!.Value;
    RandScena[] Randuri(FacturaScena d, int i, decimal q, decimal v, decimal taxa = 0,
            string tva = null, int? perioada = null, bool inversa = false) {
        var l = d.Linii[i]; var tv = tva == null ? (Guid?)null : Tva(tva);
        var debit = new RandScena(Cont("371"), N.Latura.Debit, v, q, Magazie,
            l.Lot, l.Produs, tv != null ? Furnizor : null, Linie: l.Id, Tva: tv,
            Rol: tv != null ? N.RolTva.Baza : null, Perioada: tv != null ? perioada ?? An * 100 + 1 : null,
            Sens: tv != null ? N.SensTva.Achizitie : null, Spatiu: N.Spatiu.Stoc);
        var credit = new RandScena(Cont("401"), N.Latura.Credit, v, -q, N.GestiuniVirtuale.Furnizor,
            P(d.Id), l.Produs, Furnizor, Linie: l.Id);
        return taxa == 0 ? [debit, credit] : [debit, credit,
            debit with { Cont = Cont("4426"), Valoare = taxa, Cantitate = 0, Unitate = null,
                Rol = N.RolTva.Taxa, Spatiu = N.Spatiu.Contabil },
            credit with { Cont = Cont(inversa ? "4427" : "401"), Valoare = taxa,
                Cantitate = 0, Gestiune = null, Unitate = inversa ? null : P(d.Id), Partener = Furnizor, Tva = inversa ? tv : null,
                Rol = inversa ? N.RolTva.Autocolectare : null, Sens = inversa ? N.SensTva.Achizitie : null,
                Perioada = inversa ? perioada ?? An * 100 + 1 : null }];
    }

    protected override void Executa() {
        if (!Privat) {
            Verifica("SC-RLF-12", "profil fără politică RLF și fără activare cub", CuSpatiu(os =>
                !os.GetObjectsQuery<RegulaContare>().Any(r => r.TipDocument.Cod == "RLF")
                && !os.GetObjectsQuery<TipDocument>().Single(t => t.Cod == "RLF").PosteazaInCub));
            var lot = Receptioneaza(new LinieFctScena(10, 10)).Linii[0];
            var inert = Retur(new LinieRlfScena(lot, 2));
            Refuza("SC-RLF-12", () => Opereaza(inert.Id), "politică de numerotare"); FaraEfecte("SC-RLF-12", inert.Id); return;
        }
        Simple(); Reziduu(); Compensare(); Refuzuri(); PestePerioada();
    }

    void Simple() {
        var f = Receptioneaza(new LinieFctScena(10, 10, Tip: "371")); var lot = f.Linii[0];
        var d = Retur(new LinieRlfScena(lot, 2));
        Verifica("SC-RLF-01", "dry-run acceptat", CuSpatiu(os => ComenziDocument.Sistem(os).Valideaza(d.Id)).Count == 0);
        FaraEfecte("SC-RLF-01", d.Id); Opereaza(d.Id);
        Postari("SC-RLF-01", d.Id, N.FelTranzactie.Operare, Ianuarie, Randuri(d, 0, -2, -20));
        SoldLot("SC-RLF-01", lot.Lot!.Value, Magazie, Ianuarie, 8, 80);
        SoldPartida("SC-RLF-01", P(d.Id), Ianuarie, 20); SoldPartida("SC-RLF-01", P(f.Id), Ianuarie, -100);
        Storneaza(d.Id, new(An, 1, 20));
        Postari("SC-RLF-03", d.Id, N.FelTranzactie.Storno, new(An, 1, 20), Randuri(d, 0, 2, 20));
        SoldLot("SC-RLF-03", lot.Lot.Value, Magazie, new(An, 1, 20), 10, 100);
        SoldPartida("SC-RLF-03", P(d.Id), new(An, 1, 20), 0);
        var intact = Amprenta(d.Id); Refuza("SC-RLF-03", () => Storneaza(d.Id, new(An, 1, 20)), "Operat");
        Verifica("SC-RLF-03", "repetare fără efecte", Amprenta(d.Id) == intact);
        var a = Retur(new LinieRlfScena(lot, 2)); Opereaza(a.Id); Anuleaza(a.Id); FaraEfecte("SC-RLF-04", a.Id);
        SoldLot("SC-RLF-04", lot.Lot.Value, Magazie, new(An, 1, 20), 10, 100);
        Opereaza(a.Id); Postari("SC-RLF-04", a.Id, N.FelTranzactie.Operare, Ianuarie, Randuri(a, 0, -2, -20));
        var mlot = Receptioneaza(new LinieFctScena(10, 10, Tip: "371"), new LinieFctScena(5, 20, Tip: "371"));
        var m = Retur(new LinieRlfScena(mlot.Linii[0], 2, "N21"), new LinieRlfScena(mlot.Linii[1], 3, "N11")); Opereaza(m.Id);
        Postari("SC-RLF-02", m.Id, N.FelTranzactie.Operare, Ianuarie,
            [.. Randuri(m, 0, -2, -20, -4.2m, "N21"), .. Randuri(m, 1, -3, -60, -6.6m, "N11")]);
        SoldLot("SC-RLF-02", mlot.Linii[0].Lot!.Value, Magazie, Ianuarie, 8, 80);
        SoldLot("SC-RLF-02", mlot.Linii[1].Lot!.Value, Magazie, Ianuarie, 2, 40);
        SoldPartida("SC-RLF-02", P(m.Id), Ianuarie, 90.8m);
        var tilot = Receptioneaza(new LinieFctScena(10, 10, Tip: "371")).Linii[0];
        var ti = Retur(new LinieRlfScena(tilot, 2, "TI21")); Opereaza(ti.Id);
        Postari("SC-RLF-10", ti.Id, N.FelTranzactie.Operare, Ianuarie, Randuri(ti, 0, -2, -20, -4.2m, "TI21", inversa: true));
        SoldPartida("SC-RLF-10", P(ti.Id), Ianuarie, 20);
    }

    void Reziduu() {
        var lot = Receptioneaza(new LinieFctScena(3, .333333m, Tip: "371")).Linii[0];
        Opereaza(Iesire(false, (lot, 2)).Id);
        var d = Retur(new LinieRlfScena(lot, 1)); Opereaza(d.Id);
        Postari("SC-RLF-05", d.Id, N.FelTranzactie.Operare, Ianuarie, Randuri(d, 0, -1, -.33m));
        SoldLot("SC-RLF-05", lot.Lot!.Value, Magazie, Ianuarie, 0, 0);
        var rez = Receptioneaza(new LinieFctScena(3, 10.006667m, Tip: "371")).Linii[0];
        Opereaza(Iesire(false, (rez, 1)).Id); Opereaza(Iesire(false, (rez, 1)).Id);
        var r = Retur(new LinieRlfScena(rez, 1)); Opereaza(r.Id);
        Postari("SC-RLF-05", r.Id, N.FelTranzactie.Operare, Ianuarie, Randuri(r, 0, -1, -10.01m));
        SoldLot("SC-RLF-05", rez.Lot!.Value, Magazie, Ianuarie, 0, -.01m);
        SoldPartida("SC-RLF-05", P(r.Id), Ianuarie, 10.01m);
        var intact = Amprenta(r.Id);
        var rdc = CuSpatiu(os => {
            var d = os.CreateObject<ReturClient>(); d.Data = Ianuarie; d.PredatorId = Client; d.PrimitorId = Magazie;
            var l = os.CreateObject<DocumentDetaliu>(); l.Document = d; l.Pozitie = 1;
            l.TipMaterialId = Tip(os, "371"); l.LotId = rez.Lot; l.Cantitate = 1;
            os.CommitChanges(); return d.ID;
        });
        Opereaza(rdc);
        SoldLot("SC-RLF-13", rez.Lot.Value, Magazie, Ianuarie, 1, 10);
        Opereaza(Iesire(false, (rez, 1)).Id);
        SoldLot("SC-RLF-13", rez.Lot.Value, Magazie, Ianuarie, 0, 0);
        SoldPartida("SC-RLF-13", P(r.Id), Ianuarie, 10.01m);
        Verifica("SC-RLF-13", "nota fiscală inițială intactă", Amprenta(r.Id) == intact);
    }

    void Compensare() {
        Furnizor = CuSpatiu(os => { var p = os.CreateObject<Partener>(); p.Cod = Marcaj + "-COMP";
            p.Denumire = p.Cod; os.CommitChanges(); return p.ID; });
        var f = Receptioneaza(new LinieFctScena(10, 10, Tip: "371"));
        var d = Retur(new LinieRlfScena(f.Linii[0], 2)); Opereaza(d.Id);
        var n = Nota(Ianuarie, new LinieNtcScena("401", "401", 20, Furnizor, Furnizor)); Opereaza(n.Id);
        SoldPartida("SC-RLF-06", P(f.Id), Ianuarie, -80); SoldPartida("SC-RLF-06", P(d.Id), Ianuarie, 0);
        var intact = Amprenta(d.Id);
        Refuza("SC-RLF-06", () => Storneaza(d.Id, Ianuarie), "PARTIDA_CU_DEPENDENTI");
        Verifica("SC-RLF-06", "refuz atomic", Amprenta(d.Id) == intact);
        Storneaza(n.Id, Ianuarie);
        SoldPartida("SC-RLF-06", P(f.Id), Ianuarie, -100); SoldPartida("SC-RLF-06", P(d.Id), Ianuarie, 20);
    }

    void Refuzuri() {
        var lot = Receptioneaza(new LinieFctScena(10, 10, Tip: "371")).Linii[0];
        var cap = CuSpatiu(os => os.GetObjectsQuery<TipTva>().First(t => t.Regim == RegimTva.Capitalizat).Cod);
        foreach (var (d, cod, mesaj) in new[] {
            (Retur(), CoduriRefuz.LiniiLipsa, "linie"),
            (Retur(new LinieRlfScena(null, 2)), CoduriRefuz.LotLipsa, "ORIGINAL"),
            (Retur(new LinieRlfScena(lot, 0)), CoduriRefuz.CantitateZero, "zero"),
            (Retur(new LinieRlfScena(lot, 2, Tip: "3028")), CoduriRefuz.ProdusAltTip, "alt Tip"),
            (Retur(new LinieRlfScena(lot, 2, cap)), CoduriRefuz.TvaCapitalizat, "capitalizat") }) {
            RefuzDeclaratie("SC-RLF-11", d.Id, cod);
            Refuza("SC-RLF-11", () => Opereaza(d.Id), mesaj); FaraEfecte("SC-RLF-11", d.Id);
        }
        var peste = Retur(new LinieRlfScena(lot, 11));
        Refuza("SC-RLF-11", () => Opereaza(peste.Id), "negativ"); FaraEfecte("SC-RLF-11", peste.Id);
    }

    void PestePerioada() {
        var lot = Receptioneaza(new LinieFctScena(10, 10, Tip: "371")).Linii[0];
        var alt = Receptioneaza(new LinieFctScena(10, 10, Tip: "371")).Linii[0];
        var tarziu = Receptioneaza(new LinieFctScena(10, 10, Tip: "371")).Linii[0];
        var p = Retur(new LinieRlfScena(lot, 2, "N21")); Opereaza(p.Id);
        var c = Retur(new LinieRlfScena(alt, 2, "N21")); Opereaza(c.Id);
        var t = Retur(new LinieRlfScena(tarziu, 2));
        Comanda(os => { os.GetObjectByKey<Document>(t.Id).DataInregistrare = Februarie; os.CommitChanges(); });
        InchideIanuarie(); Refuza("SC-RLF-07", () => Anuleaza(p.Id), "închis");
        Storneaza(p.Id, Februarie);
        Postari("SC-RLF-07", p.Id, N.FelTranzactie.Storno, Februarie, Randuri(p, 0, 2, 20, 4.2m, "N21", An * 100 + 2));
        SoldLot("SC-RLF-07", lot.Lot!.Value, Magazie, new(An, 1, 31), 8, 80);
        SoldLot("SC-RLF-07", lot.Lot.Value, Magazie, Februarie, 10, 100);
        var nou = Corecteaza(c.Id); FaraEfecte("SC-RLF-08", nou);
        var corectie = CuSpatiu(os => {
            var d = os.GetObjectByKey<Document>(nou);
            Verifica("SC-RLF-08", "corecție legată, date și motiv", d.CorecteazaId == c.Id
                && d.MotivCorectie == MotivCorectie.EroareMateriala && d.Data == Ianuarie && d.DataInregistrare == Februarie);
            var l = os.GetObjectsQuery<DocumentDetaliu>().Single(l => l.DocumentId == nou);
            l.Cantitate = 1; l.ValoareTva = 0; os.CommitChanges();
            return new FacturaScena(nou, [new(l.ID, l.LotId, alt.Produs)]);
        });
        Opereaza(nou);
        Postari("SC-RLF-08", c.Id, N.FelTranzactie.Storno, Februarie, Randuri(c, 0, 2, 20, 4.2m, "N21"));
        Postari("SC-RLF-08", nou, N.FelTranzactie.Operare, Februarie, Randuri(corectie, 0, -1, -10, -2.1m, "N21"));
        SoldPartida("SC-RLF-08", P(nou), Februarie, 12.1m);
        SoldLot("SC-RLF-08", alt.Lot!.Value, Magazie, new(An, 1, 31), 8, 80);
        SoldLot("SC-RLF-08", alt.Lot.Value, Magazie, Februarie, 9, 90);
        Opereaza(t.Id); Postari("SC-RLF-09", t.Id, N.FelTranzactie.Operare, Februarie, Randuri(t, 0, -2, -20));
        SoldPartida("SC-RLF-09", P(t.Id), new(An, 1, 31), 0); SoldPartida("SC-RLF-09", P(t.Id), Februarie, 20);
        SoldLot("SC-RLF-09", tarziu.Lot!.Value, Magazie, new(An, 1, 31), 10, 100);
        SoldLot("SC-RLF-09", tarziu.Lot.Value, Magazie, Februarie, 8, 80);
    }
}
