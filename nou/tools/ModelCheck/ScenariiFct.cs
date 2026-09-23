using Atlas.Conta.BackOffice.Module.Api;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Declaratii;
using DevExpress.ExpressApp;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.ModelCheck;

sealed class ScenariiFct(Func<IObjectSpace> deschide, Action<string, bool> check,
    bool privat, Action<IObjectSpace, int, int> inchide)
    : ScenaDocumente(deschide, check, privat, inchide, "FCT", 2002) {
    protected override void Executa() {
        Simpla();
        Multiple();
        StornoSiAnulare();
        Fiscal();
        Refuzuri();
        Dependenti();
        PestePerioada();
    }

    RandScena[] Randuri(FacturaScena f, int index, decimal q, decimal net, decimal taxa = 0,
        string tva = null, int? perioada = null) {
        var l = f.Linii[index];
        var intern = l.Lot != null ? Stoc : Serviciu;
        var cont = Cont(intern);
        if (!Privat) { net += taxa; taxa = 0; tva = null; }
        var tv = tva == null ? (Guid?)null : Tva(tva);
        var debit = new RandScena(cont, N.Latura.Debit, net, q, Magazie, l.Lot, l.Produs,
            tv != null ? Furnizor : null, l.Id, tv, tv != null ? N.RolTva.Baza : null,
            tv != null ? perioada ?? An * 100 + 1 : null, tv != null ? N.SensTva.Achizitie : null,
            l.Lot != null ? N.Spatiu.Stoc : N.Spatiu.Contabil, Economic);
        var credit = new RandScena(Cont(ContFurnizor), N.Latura.Credit, net, -q,
            l.Lot != null ? N.GestiuniVirtuale.Furnizor : null, Partida(f.Id, ContFurnizor),
            l.Produs, Privat ? Furnizor : null, l.Id, Economic: Economic);
        var randuri = new List<RandScena> { debit, credit };
        if (taxa != 0) {
            randuri.Add(debit with { Cont = Privat ? Cont("4426") : cont, Cantitate = 0, Valoare = taxa,
                Unitate = Privat ? null : l.Lot, Spatiu = Privat ? N.Spatiu.Contabil : debit.Spatiu, Rol = N.RolTva.Taxa });
            randuri.Add(credit with { Cantitate = 0, Valoare = taxa, Gestiune = Privat ? null : credit.Gestiune });
        }
        return randuri.ToArray();
    }

    void Simpla() {
        var f = Factura(Ianuarie, new LinieFctScena(10, 10));
        Verifica("SC-FCT-01", "dry-run acceptat", CuSpatiu(os => OperareApi.Valideaza(os, f.Id)).Count == 0);
        FaraEfecte("SC-FCT-01", f.Id);
        var nir = Opereaza(f.Id).ConexId!.Value;
        Postari("SC-FCT-01", f.Id, N.FelTranzactie.Operare, Ianuarie, Randuri(f, 0, 10, 100));
        SoldLot("SC-FCT-01", f.Linii[0].Lot!.Value, Magazie, Ianuarie, 10, 100);
        if (Privat) SoldPartida("SC-FCT-01", Partida(f.Id, ContFurnizor)!.Value, Ianuarie, -100);
        var inainte = Amprenta(f.Id);
        Opereaza(nir);
        Verifica("SC-X-02", "NIR conex nu schimbă postările FCT", Amprenta(f.Id) == inainte);
        SoldLot("SC-X-02", f.Linii[0].Lot!.Value, Magazie, Ianuarie, 10, 100);
    }

    void Multiple() {
        var f = Factura(Ianuarie, new LinieFctScena(4, 10), new LinieFctScena(2, 15));
        Opereaza(f.Id);
        Postari("SC-FCT-02", f.Id, N.FelTranzactie.Operare, Ianuarie,
            [.. Randuri(f, 0, 4, 40), .. Randuri(f, 1, 2, 30)]);
        SoldLot("SC-FCT-02", f.Linii[0].Lot!.Value, Magazie, Ianuarie, 4, 40);
        SoldLot("SC-FCT-02", f.Linii[1].Lot!.Value, Magazie, Ianuarie, 2, 30);
        if (Privat) SoldPartida("SC-FCT-02", Partida(f.Id, ContFurnizor)!.Value, Ianuarie, -70);
    }

    void StornoSiAnulare() {
        var f = Factura(Ianuarie, new LinieFctScena(10, 10)); Opereaza(f.Id);
        var data = new DateOnly(An, 1, 20);
        Storneaza(f.Id, data);
        Postari("SC-FCT-03", f.Id, N.FelTranzactie.Operare, Ianuarie, Randuri(f, 0, 10, 100));
        Postari("SC-FCT-03", f.Id, N.FelTranzactie.Storno, data, Randuri(f, 0, -10, -100));
        SoldLot("SC-FCT-03", f.Linii[0].Lot!.Value, Magazie, data, 0, 0);
        if (Privat) SoldPartida("SC-FCT-03", Partida(f.Id, ContFurnizor)!.Value, data, 0);
        var neschimbat = Amprenta(f.Id);
        Refuza("SC-FCT-03", () => Storneaza(f.Id, data), "Operat");
        Verifica("SC-FCT-03", "storno repetat nu scrie", Amprenta(f.Id) == neschimbat);
        var a = Factura(Ianuarie, new LinieFctScena(10, 10)); var conex = Opereaza(a.Id).ConexId!.Value;
        Anuleaza(a.Id); FaraEfecte("SC-FCT-05", a.Id);
        Verifica("SC-FCT-05", "conex Draft eliminat; lotul de culegere păstrat fără sold", CuSpatiu(os =>
            !os.GetObjectsQuery<Document>().Any(d => d.ID == conex)
            && os.GetObjectsQuery<Lot>().Any(l => l.ID == a.Linii[0].Lot)));
        SoldLot("SC-FCT-05", a.Linii[0].Lot!.Value, Magazie, data, 0, 0);
    }

    void Fiscal() {
        var t21 = Privat ? "N21" : "CAP21"; var t11 = Privat ? "N11" : "CAP11";
        var f = Factura(Ianuarie, new LinieFctScena(10, 10, t21), new LinieFctScena(5, 10, t11)); Opereaza(f.Id);
        Postari("SC-FCT-07", f.Id, N.FelTranzactie.Operare, Ianuarie,
            [.. Randuri(f, 0, 10, 100, 21, t21), .. Randuri(f, 1, 5, 50, 5.50m, t11)]);
        SoldLot("SC-FCT-07", f.Linii[0].Lot!.Value, Magazie, Ianuarie, 10, Privat ? 100 : 121);
        SoldLot("SC-FCT-07", f.Linii[1].Lot!.Value, Magazie, Ianuarie, 5, Privat ? 50 : 55.50m);
        if (!Privat) return;
        foreach (var cod in new[] { "NED21", "TI21" }) {
            var special = Factura(Ianuarie, new LinieFctScena(1, 100, cod, false)); Opereaza(special.Id);
            var r = Randuri(special, 0, 0, 100, 21, cod);
            if (cod == "NED21") r[2] = r[2] with { Cont = Cont(Serviciu) };
            else r[3] = r[3] with { Cont = Cont("4427"), Partener = null, Unitate = null };
            Postari("SC-X-14", special.Id, N.FelTranzactie.Operare, Ianuarie, r);
            Storneaza(special.Id, Ianuarie);
            Postari("SC-X-14", special.Id, N.FelTranzactie.Storno, Ianuarie,
                r.Select(p => p with { Valoare = -p.Valoare }).ToArray());
        }
        SoldPartida("SC-FCT-07", Partida(f.Id, ContFurnizor)!.Value, Ianuarie, -176.50m);
        var mic = Factura(Ianuarie, new LinieFctScena(1, 0.01m, "N21", false), new LinieFctScena(1, 0.01m, "N21", false));
        Opereaza(mic.Id);
        Postari("SC-FCT-08", mic.Id, N.FelTranzactie.Operare, Ianuarie,
            [.. Randuri(mic, 0, 0, 0.01m, 0, "N21"), .. Randuri(mic, 1, 0, 0.01m, 0, "N21")]);
        SoldPartida("SC-FCT-08", Partida(mic.Id, ContFurnizor)!.Value, Ianuarie, -0.02m);
        var cules = Factura(Ianuarie, new LinieFctScena(1, 100, "N21", false, 21.01m)); Opereaza(cules.Id);
        Postari("SC-FCT-08", cules.Id, N.FelTranzactie.Operare, Ianuarie,
            Randuri(cules, 0, 0, 100, 21.01m, "N21"));
        SoldPartida("SC-FCT-08", Partida(cules.Id, ContFurnizor)!.Value, Ianuarie, -121.01m);
    }

    void Refuzuri() {
        foreach (var (id, cod, fragment) in new[] {
            ("SC-FCT-09a", CoduriRefuz.NumarLipsa, "număr"),
            ("SC-FCT-09b", CoduriRefuz.CantitateNepozitiva, "cantitate"),
            ("SC-FCT-09c", CoduriRefuz.LotLipsa, "lot") }) {
            var f = Factura(Ianuarie, new LinieFctScena(1, 10));
            using (var os = Deschide()) {
                var doc = os.GetObjectsQuery<FacturaIntrare>().Single(d => d.ID == f.Id);
                var l = os.GetObjectsQuery<FacturaIntrareDetaliu>().Single(d => d.DocumentId == f.Id);
                if (id.EndsWith('a')) doc.Numar = null;
                if (id.EndsWith('b')) l.Cantitate = 0;
                if (id.EndsWith('c')) { l.Lot = null; l.LotId = null; }
                os.CommitChanges();
            }
            RefuzDeclaratie(id, f.Id, cod);
            Verifica(id, "dry-run refuzat", CuSpatiu(os => OperareApi.Valideaza(os, f.Id)).Count > 0);
            FaraEfecte(id, f.Id);
            Refuza(id, () => Opereaza(f.Id), fragment);
            FaraEfecte(id, f.Id);
        }
    }

    void Dependenti() {
        var f = Factura(Ianuarie, new LinieFctScena(10, 10)); var nir = Opereaza(f.Id).ConexId!.Value;
        Opereaza(nir);
        var bcs = Consum(f.Linii[0].Lot!.Value, 4); Opereaza(bcs);
        var amprente = new[] { f.Id, nir, bcs }.Select(Amprenta).ToArray();
        Refuza("SC-X-01", () => Storneaza(f.Id, new(An, 1, 20)), "conex");
        Refuza("SC-X-01", () => Storneaza(nir, new(An, 1, 20)), "Sold negativ");
        Verifica("SC-X-01", "refuzurile păstrează postările întregului lanț",
            new[] { f.Id, nir, bcs }.Select(Amprenta).SequenceEqual(amprente));
        SoldLot("SC-X-01", f.Linii[0].Lot!.Value, Magazie, new(An, 1, 20), 6, 60);
        if (Privat) SoldPartida("SC-X-01", Partida(f.Id, ContFurnizor)!.Value, new(An, 1, 20), -100);
    }

    void PestePerioada() {
        var tva = Privat ? "N21" : "CAP21";
        var f = Factura(Ianuarie, new LinieFctScena(1, 100, tva, false)); Opereaza(f.Id);
        var c = Factura(Ianuarie, new LinieFctScena(1, 100, tva, false)); Opereaza(c.Id);
        InchideIanuarie();
        var inainte = Amprenta(f.Id);
        Refuza("SC-FCT-04", () => Anuleaza(f.Id), "închis");
        Verifica("SC-FCT-04", "anularea refuzată păstrează postările", Amprenta(f.Id) == inainte);
        Storneaza(f.Id, Februarie);
        Postari("SC-FCT-04", f.Id, N.FelTranzactie.Operare, Ianuarie, Randuri(f, 0, 0, 100, 21, tva));
        Postari("SC-FCT-04", f.Id, N.FelTranzactie.Storno, Februarie, Randuri(f, 0, 0, -100, -21, tva, An * 100 + 2));
        var nou = Corecteaza(c.Id); FaraEfecte("SC-FCT-06", nou);
        Postari("SC-FCT-06", c.Id, N.FelTranzactie.Storno, Februarie, Randuri(c, 0, 0, -100, -21, tva));
        FacturaScena corectie;
        using (var os = Deschide()) {
            var d = os.GetObjectsQuery<FacturaIntrare>().Single(d => d.ID == nou);
            Verifica("SC-FCT-06", "corecție legată, motiv și date", d.CorecteazaId == c.Id
                && d.MotivCorectie == MotivCorectie.EroareMateriala && d.Data == Ianuarie && d.DataInregistrare == Februarie);
            var l = os.GetObjectsQuery<FacturaIntrareDetaliu>().Single(l => l.DocumentId == nou);
            l.PretUnitar = 80; l.ValoareTva = 0;
            corectie = new(nou, [new(l.ID, null, null)]); os.CommitChanges();
        }
        Opereaza(nou);
        Postari("SC-FCT-06", nou, N.FelTranzactie.Operare, Februarie, Randuri(corectie, 0, 0, 80, 16.80m, tva));
        Postari("SC-FCT-06", c.Id, N.FelTranzactie.Operare, Ianuarie, Randuri(c, 0, 0, 100, 21, tva));
        if (Privat) {
            SoldPartida("SC-FCT-06", Partida(c.Id, ContFurnizor)!.Value, new(An, 1, 31), -121);
            SoldPartida("SC-FCT-06", Partida(c.Id, ContFurnizor)!.Value, Februarie, 0);
            SoldPartida("SC-FCT-06", Partida(nou, ContFurnizor)!.Value, Februarie, -96.80m);
        }
    }
}
