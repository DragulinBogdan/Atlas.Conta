using Atlas.Conta.BackOffice.Module.Api;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Declaratii;
using Atlas.Conta.BackOffice.Module.Motor;
using DevExpress.ExpressApp;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.ModelCheck;

record LinieFclScena(decimal Cantitate, decimal Pret, string Tva = null, LinieScena Lot = null);

sealed class ScenariiVanzare(Func<IObjectSpace> deschide, Action<string, bool> check,
    bool privat, Action<IObjectSpace, int, int> inchide)
    : ScenaDocumente(deschide, check, privat, inchide, "VNZ", 2005) {
    string Venit => Privat ? "704" : "751.01.00";

    FacturaScena Vinde(params LinieFclScena[] linii) {
        using var os = Deschide();
        var d = os.CreateObject<FacturaIesire>(); d.Data = Ianuarie;
        d.PredatorId = Magazie; d.PrimitorId = Client;
        if (linii.Any(l => l.Lot != null)) d.GestiuneDescarcareId = Magazie;
        var rezultat = new List<LinieScena>();
        foreach (var spec in linii) {
            var l = os.CreateObject<FacturaIesireDetaliu>(); l.Document = d;
            l.Pozitie = rezultat.Count + 1; l.Cantitate = spec.Cantitate; l.PretUnitar = spec.Pret;
            l.TipMaterialId = Tip(os, spec.Lot != null ? "371" : Venit); l.CodEconomicId = Economic;
            if (spec.Tva != null) l.TipTva = os.GetObjectsQuery<TipTva>().Single(t => t.Cod == spec.Tva);
            l.LotId = spec.Lot?.Lot; l.ProdusId = spec.Lot?.Produs;
            rezultat.Add(new(l.ID, l.LotId, l.ProdusId));
        }
        os.CommitChanges(); return new(d.ID, rezultat.ToArray());
    }

    RandScena[] Venituri(FacturaScena f, int i, decimal net, decimal taxa = 0, string tva = null, int? perioada = null) {
        var l = f.Linii[i]; var tv = tva == null ? (Guid?)null : Tva(tva);
        var debit = new RandScena(Cont(ContClient), N.Latura.Debit, net,
            Unitate: Partida(f.Id, ContClient, Client), Produs: l.Produs, Partener: Privat ? Client : null,
            Linie: l.Id, Economic: Economic);
        var credit = new RandScena(Cont(l.Lot != null ? "707" : Venit), N.Latura.Credit, net,
            Gestiune: Magazie, Produs: l.Produs, Partener: tv != null ? Client : null,
            Linie: l.Id, Tva: tv, Rol: tv != null ? N.RolTva.Baza : null,
            Perioada: tv != null ? perioada ?? An * 100 + 1 : null,
            Sens: tv != null ? N.SensTva.Livrare : null, Economic: Economic);
        return taxa == 0 ? [debit, credit] : [debit, credit,
            debit with { Valoare = taxa }, credit with { Cont = Cont("4427"), Valoare = taxa, Rol = N.RolTva.Taxa }];
    }

    RandScena[] Costuri(FacturaScena d, int i, decimal q, decimal v) {
        var l = d.Linii[i];
        return [new(Cont("607"), N.Latura.Debit, v, q, N.GestiuniVirtuale.Client,
                    Produs: l.Produs, Partener: Client, Linie: l.Id),
            new(Cont("371"), N.Latura.Credit, v, -q, Magazie, l.Lot, l.Produs,
                    Linie: l.Id, Spatiu: N.Spatiu.Stoc)];
    }

    protected override void Executa() {
        FacturiDeschise();
        var f = Vinde(new LinieFclScena(1, 100)); Opereaza(f.Id);
        var inc = Trezorerie(true, 40); Opereaza(inc.Id);
        var original = Amprenta(f.Id);
        var imp = Imperecheaza(inc.Id, f.Id, 40, Ianuarie);
        TransferPartida("SC-FCL-09", inc.Id, f.Id, ContClient, Client, N.Latura.Credit, 40, Ianuarie);
        if (Privat) SoldPartida("SC-FCL-09", Partida(f.Id, ContClient, Client)!.Value, Ianuarie, 60);
        var tva = Privat ? "N21" : null;
        var p = Vinde(new LinieFclScena(1, 100, tva)); Opereaza(p.Id);
        var c = Vinde(new LinieFclScena(1, 100, tva)); Opereaza(c.Id);
        (FacturaScena Peste, FacturaScena Corectie)? dsc = Privat ? DescarcariDeschise() : null;
        InchideIanuarie();
        Comanda(os => ImperechereService.Desfa(os, imp, Februarie));
        TransferPartida("SC-FCL-09", inc.Id, f.Id, ContClient, Client, N.Latura.Credit, -40, Februarie);
        Verifica("SC-FCL-09", "factura intactă și împerechere inversată", Amprenta(f.Id) == original
            && CuSpatiu(os => os.GetObjectsQuery<Imperechere>().Where(i => i.DocumentId == f.Id).Sum(i => i.Suma)) == 0);
        if (Privat) {
            SoldPartida("SC-FCL-09", Partida(f.Id, ContClient, Client)!.Value, new(An, 1, 31), 60);
            SoldPartida("SC-FCL-09", Partida(f.Id, ContClient, Client)!.Value, Februarie, 100);
            SoldPartida("SC-FCL-09", Partida(inc.Id, ContClient, Client)!.Value, Februarie, -40);
        }
        FacturiInchise(p, c, tva);
        if (dsc is { } stoc) DescarcariInchise(stoc.Peste, stoc.Corectie);
    }

    void FacturiDeschise() {
        var f = Vinde(new LinieFclScena(1, 100));
        Verifica("SC-FCL-01", "dry-run acceptat", CuSpatiu(os => OperareApi.Valideaza(os, f.Id)).Count == 0);
        FaraEfecte("SC-FCL-01", f.Id); Opereaza(f.Id);
        Postari("SC-FCL-01", f.Id, N.FelTranzactie.Operare, Ianuarie, Venituri(f, 0, 100));
        if (Privat) SoldPartida("SC-FCL-01", Partida(f.Id, ContClient, Client)!.Value, Ianuarie, 100);
        var m = Vinde(new LinieFclScena(1, 100), new LinieFclScena(1, 50)); Opereaza(m.Id);
        Postari("SC-FCL-02", m.Id, N.FelTranzactie.Operare, Ianuarie, [.. Venituri(m, 0, 100), .. Venituri(m, 1, 50)]);
        Storneaza(f.Id, new(An, 1, 20));
        Postari("SC-FCL-03", f.Id, N.FelTranzactie.Storno, new(An, 1, 20), Venituri(f, 0, -100));
        Postari("SC-FCL-03", f.Id, N.FelTranzactie.Operare, Ianuarie, Venituri(f, 0, 100));
        if (Privat) SoldPartida("SC-FCL-03", Partida(f.Id, ContClient, Client)!.Value, new(An, 1, 20), 0);
        var amprenta = Amprenta(f.Id);
        Refuza("SC-FCL-03", () => Storneaza(f.Id, new(An, 1, 20)), "Operat");
        Verifica("SC-FCL-03", "repetarea nu scrie", Amprenta(f.Id) == amprenta);
        Anuleaza(m.Id); FaraEfecte("SC-FCL-05", m.Id);
        var invalid = Vinde(new LinieFclScena(0, 100));
        RefuzDeclaratie("SC-FCL-08", invalid.Id, CoduriRefuz.CantitateNepozitiva);
        Refuza("SC-FCL-08", () => Opereaza(invalid.Id), "cantitate"); FaraEfecte("SC-FCL-08", invalid.Id);
        if (!Privat) return;
        foreach (var cod in new[] { "TI21", "SDD" }) {
            var special = Vinde(new LinieFclScena(1, 100, cod)); Opereaza(special.Id);
            Postari("SC-X-14", special.Id, N.FelTranzactie.Operare, Ianuarie, Venituri(special, 0, 100, tva: cod));
            Storneaza(special.Id, Ianuarie);
            Postari("SC-X-14", special.Id, N.FelTranzactie.Storno, Ianuarie, Venituri(special, 0, -100, tva: cod));
        }
        var fiscal = Vinde(new LinieFclScena(1, 100, "N21"), new LinieFclScena(1, 50, "N11")); Opereaza(fiscal.Id);
        Postari("SC-FCL-07", fiscal.Id, N.FelTranzactie.Operare, Ianuarie,
            [.. Venituri(fiscal, 0, 100, 21, "N21"), .. Venituri(fiscal, 1, 50, 5.50m, "N11")]);
        SoldPartida("SC-FCL-07", Partida(fiscal.Id, ContClient, Client)!.Value, Ianuarie, 176.50m);
        var lot = Receptioneaza(new LinieFctScena(10, 10, Tip: "371")).Linii[0];
        var vanzare = Vinde(new LinieFclScena(4, 20, Lot: lot));
        var conex = Opereaza(vanzare.Id).ConexId!.Value;
        Postari("SC-FCL-10", vanzare.Id, N.FelTranzactie.Operare, Ianuarie, Venituri(vanzare, 0, 80));
        FaraEfecte("SC-FCL-10", conex); Opereaza(conex);
        var linii = CuSpatiu(os => os.GetObjectsQuery<DescarcareGestiuneDetaliu>().Where(l => l.DocumentId == conex)
            .Select(l => new LinieScena(l.ID, l.LotId, l.Lot.ProdusId)).ToArray());
        Postari("SC-FCL-10", conex, N.FelTranzactie.Operare, Ianuarie, Costuri(new(conex, linii), 0, 4, 40));
        SoldLot("SC-FCL-10", lot.Lot!.Value, Magazie, Ianuarie, 6, 60);
        var inainte = Amprenta(vanzare.Id) + Amprenta(conex);
        Refuza("SC-FCL-10", () => Storneaza(vanzare.Id, new(An, 1, 20)), "conex");
        Verifica("SC-FCL-10", "refuzul păstrează factură și descărcare", Amprenta(vanzare.Id) + Amprenta(conex) == inainte);
    }

    void FacturiInchise(FacturaScena p, FacturaScena c, string tva) {
        var taxa = Privat ? 21m : 0m;
        Refuza("SC-FCL-04", () => Anuleaza(p.Id), "închis"); Storneaza(p.Id, Februarie);
        Postari("SC-FCL-04", p.Id, N.FelTranzactie.Operare, Ianuarie, Venituri(p, 0, 100, taxa, tva));
        Postari("SC-FCL-04", p.Id, N.FelTranzactie.Storno, Februarie, Venituri(p, 0, -100, -taxa, tva, An * 100 + 2));
        if (Privat) {
            SoldPartida("SC-FCL-04", Partida(p.Id, ContClient, Client)!.Value, new(An, 1, 31), 121);
            SoldPartida("SC-FCL-04", Partida(p.Id, ContClient, Client)!.Value, Februarie, 0);
        }
        var nou = Corecteaza(c.Id); FaraEfecte("SC-FCL-06", nou);
        FacturaScena corectie;
        using (var os = Deschide()) {
            var d = os.GetObjectsQuery<FacturaIesire>().Single(d => d.ID == nou);
            Verifica("SC-FCL-06", "corecție legată, motiv și date", d.CorecteazaId == c.Id
                && d.MotivCorectie == MotivCorectie.EroareMateriala && d.Data == Ianuarie && d.DataInregistrare == Februarie);
            var l = os.GetObjectsQuery<FacturaIesireDetaliu>().Single(l => l.DocumentId == nou);
            l.PretUnitar = 80; l.ValoareTva = 0; corectie = new(nou, [new(l.ID, null, null)]); os.CommitChanges();
        }
        Opereaza(nou);
        Postari("SC-FCL-06", c.Id, N.FelTranzactie.Operare, Ianuarie, Venituri(c, 0, 100, taxa, tva));
        Postari("SC-FCL-06", c.Id, N.FelTranzactie.Storno, Februarie, Venituri(c, 0, -100, -taxa, tva));
        Postari("SC-FCL-06", nou, N.FelTranzactie.Operare, Februarie, Venituri(corectie, 0, 80, Privat ? 16.80m : 0, tva));
        if (Privat) SoldPartida("SC-FCL-06", Partida(nou, ContClient, Client)!.Value, Februarie, 96.80m);
    }

    (FacturaScena, FacturaScena) DescarcariDeschise() {
        var lot = Receptioneaza(new LinieFctScena(10, 10, Tip: "371")).Linii[0];
        var d = Iesire(false, (lot, 4));
        Verifica("SC-DSC-01", "dry-run acceptat", CuSpatiu(os => OperareApi.Valideaza(os, d.Id)).Count == 0);
        FaraEfecte("SC-DSC-01", d.Id); Opereaza(d.Id);
        Postari("SC-DSC-01", d.Id, N.FelTranzactie.Operare, Ianuarie, Costuri(d, 0, 4, 40));
        SoldLot("SC-DSC-01", lot.Lot!.Value, Magazie, Ianuarie, 6, 60);
        Storneaza(d.Id, new(An, 1, 20));
        Postari("SC-DSC-03", d.Id, N.FelTranzactie.Operare, Ianuarie, Costuri(d, 0, 4, 40));
        Postari("SC-DSC-03", d.Id, N.FelTranzactie.Storno, new(An, 1, 20), Costuri(d, 0, -4, -40));
        SoldLot("SC-DSC-03", lot.Lot.Value, Magazie, new(An, 1, 20), 10, 100);
        var amprenta = Amprenta(d.Id);
        Refuza("SC-DSC-03", () => Storneaza(d.Id, new(An, 1, 20)), "Operat");
        Verifica("SC-DSC-03", "repetarea nu scrie", Amprenta(d.Id) == amprenta);
        var a = Iesire(false, (lot, 4)); Opereaza(a.Id); Anuleaza(a.Id); FaraEfecte("SC-DSC-05", a.Id);
        SoldLot("SC-DSC-05", lot.Lot.Value, Magazie, new(An, 1, 20), 10, 100);
        var f = Receptioneaza(new LinieFctScena(10, 10, Tip: "371"), new LinieFctScena(5, 20, Tip: "371"));
        var m = Iesire(false, (f.Linii[0], 2), (f.Linii[1], 3)); Opereaza(m.Id);
        Postari("SC-DSC-02", m.Id, N.FelTranzactie.Operare, Ianuarie, [.. Costuri(m, 0, 2, 20), .. Costuri(m, 1, 3, 60)]);
        SoldLot("SC-DSC-02", f.Linii[0].Lot!.Value, Magazie, Ianuarie, 8, 80);
        SoldLot("SC-DSC-02", f.Linii[1].Lot!.Value, Magazie, Ianuarie, 2, 40);
        var mic = Receptioneaza(new LinieFctScena(3, 0.333333m, Tip: "371")).Linii[0];
        var r = Iesire(false, (mic, 1), (mic, 2)); Opereaza(r.Id);
        Postari("SC-DSC-07", r.Id, N.FelTranzactie.Operare, Ianuarie, [.. Costuri(r, 0, 1, 0.33m), .. Costuri(r, 1, 2, 0.67m)]);
        SoldLot("SC-DSC-07", mic.Lot!.Value, Magazie, Ianuarie, 0, 0);
        var invalid = Iesire(false, (lot, 11));
        RefuzDeclaratie("SC-DSC-08", invalid.Id, "STOC_INSUFICIENT");
        Refuza("SC-DSC-08", () => Opereaza(invalid.Id), "Sold negativ"); FaraEfecte("SC-DSC-08", invalid.Id);
        var p = Iesire(false, (Receptioneaza(new LinieFctScena(10, 10, Tip: "371")).Linii[0], 4)); Opereaza(p.Id);
        var c = Iesire(false, (Receptioneaza(new LinieFctScena(10, 10, Tip: "371")).Linii[0], 4)); Opereaza(c.Id);
        return (p, c);
    }

    void DescarcariInchise(FacturaScena p, FacturaScena c) {
        Refuza("SC-DSC-04", () => Anuleaza(p.Id), "închis"); Storneaza(p.Id, Februarie);
        Postari("SC-DSC-04", p.Id, N.FelTranzactie.Storno, Februarie, Costuri(p, 0, -4, -40));
        SoldLot("SC-DSC-04", p.Linii[0].Lot!.Value, Magazie, new(An, 1, 31), 6, 60);
        SoldLot("SC-DSC-04", p.Linii[0].Lot!.Value, Magazie, Februarie, 10, 100);
        var nou = Corecteaza(c.Id); FaraEfecte("SC-DSC-06", nou);
        FacturaScena corectie;
        using (var os = Deschide()) {
            var d = os.GetObjectsQuery<DescarcareGestiune>().Single(d => d.ID == nou);
            Verifica("SC-DSC-06", "corecție legată, motiv și date", d.CorecteazaId == c.Id
                && d.MotivCorectie == MotivCorectie.EroareMateriala && d.Data == Ianuarie && d.DataInregistrare == Februarie);
            var l = os.GetObjectsQuery<DescarcareGestiuneDetaliu>().Single(l => l.DocumentId == nou);
            l.Cantitate = 3; corectie = new(nou, [new(l.ID, l.LotId, c.Linii[0].Produs)]); os.CommitChanges();
        }
        Opereaza(nou);
        Postari("SC-DSC-06", c.Id, N.FelTranzactie.Operare, Ianuarie, Costuri(c, 0, 4, 40));
        Postari("SC-DSC-06", c.Id, N.FelTranzactie.Storno, Februarie, Costuri(c, 0, -4, -40));
        Postari("SC-DSC-06", nou, N.FelTranzactie.Operare, Februarie, Costuri(corectie, 0, 3, 30));
        SoldLot("SC-DSC-06", c.Linii[0].Lot!.Value, Magazie, Februarie, 7, 70);
    }
}
