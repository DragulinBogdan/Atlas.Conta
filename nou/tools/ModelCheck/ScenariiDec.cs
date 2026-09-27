using Atlas.Conta.BackOffice.Module.Api;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Declaratii;
using Atlas.Conta.BackOffice.Module.Motor;
using Atlas.Conta.BackOffice.Module.Saft;
using DevExpress.ExpressApp;
using N = Atlas.Conta.Nucleu;
using C = Atlas.Conta.BackOffice.Module.Cub;

namespace Atlas.Conta.BackOffice.ModelCheck;

sealed class ScenariiDec(Func<IObjectSpace> deschide, Action<string, bool> check, bool privat,
    Action<IObjectSpace, int, int> inchide) : ScenaDocumente(deschide, check, privat, inchide, "DEC", 2018) {
    Guid titular;
    string Avans => Privat ? "542" : "542.01.00";
    FacturaScena Culege(decimal pret = 100, string tva = null, int cate = 1) => CuSpatiu(os => {
        var d = os.CreateObject<Decont>(); d.Data = Ianuarie; d.PredatorId = titular; d.PrimitorId = Loc;
        var linii = new List<LinieScena>();
        for (var i = 0; i < cate; i++) {
            var l = os.CreateObject<DecontDetaliu>(); l.Document = d; l.Pozitie = i + 1;
            l.TipMaterialId = Tip(os, Serviciu); l.Cantitate = 1; l.PretUnitar = pret;
            l.CodEconomicId = Economic; if (tva != null) l.TipTvaId = Tva(tva);
            linii.Add(new(l.ID, null, null));
        }
        os.CommitChanges(); return new FacturaScena(d.ID, linii.ToArray());
    });
    RandScena[] Simple(FacturaScena d, decimal suma, Guid? linie = null) => [
        new(Cont(Serviciu), N.Latura.Debit, suma, Partener: titular, Linie: linie ?? d.Linii[0].Id, Economic: Economic),
        new(Cont(Avans), N.Latura.Credit, suma, Unitate: N.Unitate.DeschidePartida(Cont(Avans), titular, d.Id, Ianuarie).Id,
            Partener: titular, Linie: linie ?? d.Linii[0].Id, Economic: Economic)];
    RandScena[] Fiscale(FacturaScena d, string cod, decimal net, decimal taxa) {
        var t = Tva(cod); var l = d.Linii[0].Id;
        var baza = new RandScena(Cont(Serviciu), N.Latura.Debit, net, Partener: titular,
            Linie: l, Tva: t, Rol: N.RolTva.Baza, Sens: N.SensTva.Achizitie, Perioada: An * 100 + 1, Economic: Economic);
        var credit = new RandScena(Cont(Avans), N.Latura.Credit, net,
            Unitate: N.Unitate.DeschidePartida(Cont(Avans), titular, d.Id, Ianuarie).Id, Partener: titular, Linie: l, Economic: Economic);
        if (cod == "NED21") return [baza, credit, baza with { Valoare = taxa, Rol = N.RolTva.Taxa }, credit with { Valoare = taxa }];
        var tax = new RandScena(Cont("4426"), N.Latura.Debit, taxa, Gestiune: Loc, Partener: titular,
            Linie: l, Tva: t, Rol: N.RolTva.Taxa, Sens: N.SensTva.Achizitie, Perioada: An * 100 + 1, Economic: Economic);
        return [baza, credit, tax, credit with { Cont = cod == "TI21" ? Cont("4427") : Cont(Avans),
            Unitate = cod == "TI21" ? null : credit.Unitate, Valoare = taxa,
            Tva = cod == "TI21" ? t : null, Rol = cod == "TI21" ? N.RolTva.Autocolectare : null,
            Sens = cod == "TI21" ? N.SensTva.Achizitie : null, Perioada = cod == "TI21" ? An * 100 + 1 : null }];
    }
    static RandScena[] Inverse(RandScena[] r, int? perioada = null) => [.. r.Select(p => p with {
        Valoare = -p.Valoare, Perioada = p.Perioada != null && perioada != null ? perioada : p.Perioada })];

    protected override void Executa() {
        Comanda(os => { var a = os.CreateObject<Angajat>(); a.Cod = Marcaj + "-A"; a.Denumire = a.Cod;
            a.ContImplicitId = Cont(Avans); titular = a.ID; os.CommitChanges(); });
        var d = Culege();
        Verifica("SC-DEC-01", "dry-run acceptat", CuSpatiu(os => ComenziDocument.Sistem(os).Valideaza(d.Id)).Count == 0);
        FaraEfecte("SC-DEC-01", d.Id); Opereaza(d.Id);
        Postari("SC-DEC-01", d.Id, N.FelTranzactie.Operare, Ianuarie, Simple(d, 100));
        Storneaza(d.Id, new(An, 1, 20));
        Postari("SC-DEC-03", d.Id, N.FelTranzactie.Storno, new(An, 1, 20), Inverse(Simple(d, 100)));
        Refuza("SC-DEC-03", () => Storneaza(d.Id, new(An, 1, 21)), "Operat");
        var m = Culege(40, cate: 2);
        Comanda(os => { os.GetObjectByKey<DecontDetaliu>(m.Linii[1].Id).PretUnitar = 60; os.CommitChanges(); });
        Opereaza(m.Id);
        Postari("SC-DEC-02", m.Id, N.FelTranzactie.Operare, Ianuarie, [.. Simple(m, 40), .. Simple(m, 60, m.Linii[1].Id)]);
        SoldPartida("SC-DEC-02", P(m.Id), Ianuarie, -100);
        Stingere(); Explicite(); Configurare();
        var a = Culege(); Opereaza(a.Id); Anuleaza(a.Id); FaraEfecte("SC-DEC-05", a.Id); Opereaza(a.Id);
        Postari("SC-DEC-05", a.Id, N.FelTranzactie.Operare, Ianuarie, Simple(a, 100));
        if (Privat) foreach (var cod in new[] { "N21", "NED21", "TI21" }) {
            var f = Culege(100, cod); Opereaza(f.Id);
            Postari("SC-DEC-08", f.Id, N.FelTranzactie.Operare, Ianuarie, Fiscale(f, cod, 100, 21));
        }
        else { var f = Culege(100, "CAP21"); Opereaza(f.Id);
            Postari("SC-DEC-09", f.Id, N.FelTranzactie.Operare, Ianuarie, Simple(f, 121)); }
        if (Privat) {
            var cote = Culege(100, "N21", 2);
            Comanda(os => { os.GetObjectByKey<DecontDetaliu>(cote.Linii[1].Id).TipTvaId = Tva("N11"); os.CommitChanges(); });
            Opereaza(cote.Id);
            Postari("SC-DEC-09", cote.Id, N.FelTranzactie.Operare, Ianuarie,
                [.. Fiscale(cote, "N21", 100, 21), .. Fiscale(new(cote.Id, [cote.Linii[1]]), "N11", 100, 11)]);
            SoldPartida("SC-DEC-09", P(cote.Id), Ianuarie, -232);
            var cules = Culege(100, "N21");
            Comanda(os => { os.GetObjectByKey<DecontDetaliu>(cules.Linii[0].Id).ValoareTva = 20.99m; os.CommitChanges(); });
            Opereaza(cules.Id);
            Postari("SC-DEC-09", cules.Id, N.FelTranzactie.Operare, Ianuarie, Fiscale(cules, "N21", 100, 20.99m));
        }
        Verifica("SC-DEC-15", "contul de avans urmărește partide fără rol comercial", CuSpatiu(os => {
            var c = os.GetObjectByKey<Cont>(Cont(Avans)); return c.UrmarestePartide && c.RolTert == RolTertCont.Niciunul;
        }));
        if (Privat) Verifica("SC-DEC-15", "titularul nu apare în Customers/Suppliers SAF-T", CuSpatiu(os => {
            var saft = SaftProiectii.Saft(os, An, 1);
            return !saft.Clienti.Any(t => t.PartenerId == titular) && !saft.Furnizori.Any(t => t.PartenerId == titular);
        }));
        var q = Culege(); Comanda(os => { os.GetObjectByKey<DecontDetaliu>(q.Linii[0].Id).Cantitate = 0; os.CommitChanges(); });
        Opereaza(q.Id); Postari("SC-DEC-12", q.Id, N.FelTranzactie.Operare, Ianuarie, Simple(q, 100));
        var negativ = Culege(-1);
        RefuzDeclaratie("SC-DEC-11", negativ.Id, CoduriRefuz.ValoareNepozitiva);
        Refuza("SC-DEC-11", () => Opereaza(negativ.Id), "pozitiv"); FaraEfecte("SC-DEC-11", negativ.Id);
        var gol = Culege(cate: 0);
        RefuzDeclaratie("SC-DEC-11", gol.Id, CoduriRefuz.LiniiLipsa);
        Refuza("SC-DEC-11", () => Opereaza(gol.Id), "lini"); FaraEfecte("SC-DEC-11", gol.Id);
        var gresit = Culege(); Comanda(os => { os.GetObjectByKey<Document>(gresit.Id).PredatorId = Loc; os.CommitChanges(); });
        RefuzDeclaratie("SC-DEC-11", gresit.Id, CoduriRefuz.PredatorNepotrivit);
        Refuza("SC-DEC-11", () => Opereaza(gresit.Id), "predator"); FaraEfecte("SC-DEC-11", gresit.Id);
        int Citiri(int cate) { var f = Culege(1, cate: cate); using var os = Deschide();
            var doc = os.GetObjectByKey<Document>(f.Id); doc.PregatesteOperare(os);
            NumaratorSql.Instanta.Reseteaza(); var contract = Contractare.Contracteaza(os, doc);
            var n = NumaratorSql.Instanta.Numar; Verifica("SC-DEC-13", "contract acceptat", contract.EsteAcceptat); return n; }
        var mic = Citiri(2); var mare = Citiri(51);
        Verifica("SC-DEC-13", $"2/51 linii: {mic}/{mare} interogări", mic == mare && mic > 0 && mare <= 20);
        var s = Culege(100, Privat ? "N21" : null); Opereaza(s.Id);
        var original = Culege(); Opereaza(original.Id);
        var tarziu = Culege(100, Privat ? "N21" : null);
        InchideIanuarie(); Refuza("SC-DEC-04", () => Anuleaza(s.Id), "închis"); Storneaza(s.Id, Februarie);
        var rs = Privat ? Fiscale(s, "N21", 100, 21) : Simple(s, 100);
        Postari("SC-DEC-04", s.Id, N.FelTranzactie.Storno, Februarie, Inverse(rs, An * 100 + 2));
        var corectie = Corecteaza(original.Id);
        Comanda(os => { os.GetObjectsQuery<DecontDetaliu>().Single(l => l.DocumentId == corectie).PretUnitar = 80; os.CommitChanges(); });
        Opereaza(corectie);
        var lid = CuSpatiu(os => os.GetObjectsQuery<DocumentDetaliu>().Single(l => l.DocumentId == corectie).ID);
        Postari("SC-DEC-06", corectie, N.FelTranzactie.Operare, Februarie, Simple(new(corectie, [new(lid, null, null)]), 80));
        Postari("SC-DEC-06", original.Id, N.FelTranzactie.Storno, Februarie, Inverse(Simple(original, 100)));
        SoldPartida("SC-DEC-06", P(original.Id), Ianuarie, -100);
        SoldPartida("SC-DEC-06", P(original.Id), Februarie, 0);
        SoldPartida("SC-DEC-06", P(corectie), Februarie, -80);
        Refuza("SC-DEC-14", () => Opereaza(tarziu.Id), "închis"); FaraEfecte("SC-DEC-14", tarziu.Id);
        Comanda(os => { os.GetObjectByKey<Document>(tarziu.Id).DataInregistrare = Februarie; os.CommitChanges(); }); Opereaza(tarziu.Id);
        var rt = Privat ? Fiscale(tarziu, "N21", 100, 21) : Simple(tarziu, 100);
        Postari("SC-DEC-14", tarziu.Id, N.FelTranzactie.Operare, Februarie,
            [.. rt.Select(p => p with { Perioada = p.Perioada != null ? An * 100 + 2 : null })]);
    }

    Guid P(Guid doc, Guid? partener = null) => N.Unitate.DeschidePartida(Cont(Avans), partener ?? titular, doc, Ianuarie).Id;

    void Configurare() {
        var tip = Guid.NewGuid(); var linie = Guid.NewGuid();
        var titularFapt = new RepartitorFapt(titular, FelRepartitor.Angajat, Cont(Avans), default);
        var locFapt = new RepartitorFapt(Loc, FelRepartitor.UnitateInterna, null, CalitateRepartitor.LocConsum);
        var operand = new Operand(
            new(Guid.NewGuid(), "DEC", Guid.NewGuid(), Ianuarie, Ianuarie, null, false, null,
                titularFapt, locFapt, null, null, null),
            [new(linie, tip, null, NaturaClasa.Serviciu, Cont(Serviciu), null, null, 1, 100, 0,
                null, 100, null, null, null, null, null, null, N.Analiza.Fara, null)],
            [new(Guid.NewGuid(), null, NaturaClasa.Serviciu, null, false,
                SursaCont.TipMaterial, null, SursaCont.RepartitorPredator, Cont(Avans), true, null, null, null)],
            [], null, new Dictionary<Guid, TipTvaFapt>(),
            new Dictionary<Guid, ContFapt> { [Cont(Serviciu)] = new(Cont(Serviciu), Serviciu, false),
                [Cont(Avans)] = new(Cont(Avans), Avans, true) },
            new Dictionary<CheieLotFapt, N.Sold>(), null, [], null, null, null,
            new(An, 1), new("scena-dec", Ianuarie)) {
                Repartitori = new Dictionary<Guid, RepartitorFapt> { [titular] = titularFapt, [Loc] = locFapt },
            };
        foreach (var (o, cod) in new[] {
            (operand with { ReguliContare = [] }, CoduriRefuz.RegulaContareLipsa),
            (operand with { Conturi = new Dictionary<Guid, ContFapt>() }, CoduriRefuz.ContExplicitLipsa) }) {
            var refuzuri = new List<N.Refuz>();
            var declaratie = DeclarantDecont.Instanta.Declara(o, new N.Rotunjire(MidpointRounding.AwayFromZero), refuzuri);
            Verifica("SC-DEC-11", "operand incomplet: " + cod, declaratie == null && refuzuri.Any(r => r.Cod == cod));
        }
        var conturi = operand.Conturi.ToDictionary(c => c.Key, c => c.Value);
        conturi[Cont(Avans)] = conturi[Cont(Avans)] with { UrmarestePartide = false };
        var refuz = new List<N.Refuz>();
        var fara = DeclarantDecont.Instanta.Declara(operand with { Conturi = conturi }, new N.Rotunjire(MidpointRounding.AwayFromZero), refuz);
        Verifica("SC-DEC-16", "atributul oprit păstrează cei 100 fără partidă", refuz.Count == 0
            && fara?.Miscari.Count == 1 && fara.Miscari[0].Valoare == 100 && fara.Miscari[0].DeLa.Unitate == null);
    }

    void Stingere() {
        var d = Culege(); Opereaza(d.Id);
        var p = Trezorerie(false, 40);
        Comanda(os => { os.GetObjectByKey<Document>(p.Id).PrimitorId = titular; os.CommitChanges(); });
        Opereaza(p.Id); var imp = Imperecheaza(p.Id, d.Id, 40, Ianuarie);
        Postari("SC-DEC-07", p.Id, N.FelTranzactie.Transfer, Ianuarie,
            new(Cont(Avans), N.Latura.Debit, -40, Unitate: P(p.Id), Partener: titular),
            new(Cont(Avans), N.Latura.Debit, 40, Unitate: P(d.Id), Partener: titular));
        SoldPartida("SC-DEC-07", P(d.Id), Ianuarie, -60);
        var inainte = Amprenta(d.Id);
        Refuza("SC-DEC-07", () => Storneaza(d.Id, Ianuarie), "stinger");
        Verifica("SC-DEC-07", "refuzul păstrează postările", Amprenta(d.Id) == inainte);
        Comanda(os => ImperechereService.Desfa(os, imp, Ianuarie));
        SoldPartida("SC-DEC-07", P(d.Id), Ianuarie, -100);
        Storneaza(d.Id, Ianuarie); SoldPartida("SC-DEC-07", P(d.Id), Ianuarie, 0);
        var avans = Trezorerie(false, 150);
        Comanda(os => { os.GetObjectByKey<Document>(avans.Id).PrimitorId = titular; os.CommitChanges(); });
        Opereaza(avans.Id); var justificare = Culege(); Opereaza(justificare.Id);
        Imperecheaza(avans.Id, justificare.Id, 100, Ianuarie);
        SoldPartida("SC-DEC-17", P(avans.Id), Ianuarie, 50);
        SoldPartida("SC-DEC-17", P(justificare.Id), Ianuarie, 0);
        var restituire = Trezorerie(true, 50);
        Comanda(os => { os.GetObjectByKey<Document>(restituire.Id).PredatorId = titular; os.CommitChanges(); });
        Opereaza(restituire.Id); Imperecheaza(restituire.Id, avans.Id, 50, Ianuarie);
        Postari("SC-DEC-17", restituire.Id, N.FelTranzactie.Transfer, Ianuarie,
            new(Cont(Avans), N.Latura.Credit, -50, Unitate: P(restituire.Id), Partener: titular),
            new(Cont(Avans), N.Latura.Credit, 50, Unitate: P(avans.Id), Partener: titular));
        SoldPartida("SC-DEC-17", P(avans.Id), Ianuarie, 0);
        SoldPartida("SC-DEC-17", P(restituire.Id), Ianuarie, 0);
    }

    void Explicite() {
        var d = Culege(100, cate: 2);
        Comanda(os => { os.GetObjectByKey<DecontDetaliu>(d.Linii[1].Id).RepartitorCreditId = Furnizor; os.CommitChanges(); });
        Opereaza(d.Id);
        var a = Simple(d, 100, d.Linii[1].Id);
        a[1] = a[1] with { Partener = Furnizor, Unitate = P(d.Id, Furnizor) };
        Postari("SC-DEC-02", d.Id, N.FelTranzactie.Operare, Ianuarie, [.. Simple(d, 100), .. a]);
        SoldPartida("SC-DEC-02", P(d.Id), Ianuarie, -100);
        SoldPartida("SC-DEC-02", P(d.Id, Furnizor), Ianuarie, -100);
        if (!Privat) return;
        var explicitul = Culege(100, "N21");
        Comanda(os => { os.GetObjectByKey<DecontDetaliu>(explicitul.Linii[0].Id).ContCreditId = Cont("462"); os.CommitChanges(); });
        Opereaza(explicitul.Id);
        var r = Fiscale(explicitul, "N21", 100, 21);
        r[1] = r[1] with { Cont = Cont("462"), Unitate = null };
        Postari("SC-DEC-10", explicitul.Id, N.FelTranzactie.Operare, Ianuarie, r);
        SoldPartida("SC-DEC-10", P(explicitul.Id), Ianuarie, -21);
        Verifica("SC-DEC-10", "totalul de stins este 21, cât ține partida, nu 121", CuSpatiu(os =>
            os.GetObjectByKey<Decont>(explicitul.Id).TotalStingere == 21m && ImperechereService.Total(os, explicitul.Id) == 21m));
    }
}
