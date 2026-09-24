using Atlas.Conta.BackOffice.Module.Api;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Declaratii;
using Atlas.Conta.BackOffice.Module.Motor;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.EFCore;
using N = Atlas.Conta.Nucleu;
using C = Atlas.Conta.BackOffice.Module.Cub;

namespace Atlas.Conta.BackOffice.ModelCheck;

sealed class ScenariiNir(Func<IObjectSpace> deschide, Action<string, bool> check,
    bool privat, Action<IObjectSpace, int, int> inchide)
    : ScenaDocumente(deschide, check, privat, inchide, "NIR", 2016) {
    int numar;
    FacturaScena Culege(params (decimal Q, decimal Pret, string Tip)[] linii) {
        using var os = Deschide();
        var d = os.CreateObject<NIR>(); d.Data = Ianuarie; d.DataInregistrare = Ianuarie;
        d.PredatorId = Furnizor; d.PrimitorId = Magazie;
        var rezultat = new List<LinieScena>();
        foreach (var (q, pret, tip) in linii) {
            var l = os.CreateObject<NirDetaliu>(); l.Document = d; l.Pozitie = rezultat.Count + 1;
            l.TipMaterialId = Tip(os, tip ?? Stoc); l.Cantitate = q; l.PretUnitar = pret; l.CodEconomicId = Economic;
            var p = os.CreateObject<Produs>(); p.Cod = Marcaj + "-P" + ++numar; p.Denumire = p.Cod;
            p.UM = "BUC"; p.TipMaterialId = l.TipMaterialId; l.ProdusId = p.ID;
            var lot = l.CreeazaLot(os, p, os.GetObjectByKey<Gestiune>(Magazie));
            rezultat.Add(new(l.ID, lot.ID, p.ID));
        }
        os.CommitChanges(); return new(d.ID, rezultat.ToArray());
    }

    RandScena[] Randuri(FacturaScena d, int i, decimal q, decimal v, string stoc = null, Guid? partida = null) {
        var l = d.Linii[i];
        return [new(Cont(stoc ?? Stoc), N.Latura.Debit, v, q, Magazie, l.Lot, l.Produs,
                Linie: l.Id, Spatiu: N.Spatiu.Stoc, Economic: Economic),
            new(Cont(ContFurnizor), N.Latura.Credit, v, -q, N.GestiuniVirtuale.Furnizor,
                partida ?? Partida(d.Id, ContFurnizor), l.Produs, Privat ? Furnizor : null, l.Id, Economic: Economic)];
    }
    static RandScena[] Inverse(params RandScena[] r) => [.. r.Select(p => p with { Cantitate = -p.Cantitate, Valoare = -p.Valoare })];
    void Sold(string id, LinieScena l, decimal q, decimal v, DateOnly? data = null, Guid? gest = null) =>
        SoldLot(id, l.Lot!.Value, gest ?? Magazie, data ?? new(An, 1, 31), q, v);
    void Datorie(string id, Guid doc, decimal suma, DateOnly? data = null) {
        if (Privat) SoldPartida(id, Partida(doc, ContFurnizor)!.Value, data ?? new(An, 1, 31), -suma);
    }

    protected override void Executa() {
        ProbeNirOperand.Ruleaza((mesaj, ok) => Verifica("SC-NIR-operand", mesaj, ok));
        var d = Culege((6, 12.5m, null));
        var c = CuSpatiu(os => Contractare.Contracteaza(os, os.GetObjectByKey<Document>(d.Id)));
        Verifica("SC-NIR-01", "prețul lotului zero în draft; contract acceptat și determinist", c.EsteAcceptat
            && CuSpatiu(os => os.GetObjectByKey<Lot>(d.Linii[0].Lot!.Value).PretUnitar) == 0
            && c == CuSpatiu(os => Contractare.Contracteaza(os, os.GetObjectByKey<Document>(d.Id))));
        Verifica("SC-NIR-01", "dry-run acceptat", CuSpatiu(os => OperareApi.Valideaza(os, d.Id)).Count == 0);
        FaraEfecte("SC-NIR-01", d.Id); Opereaza(d.Id);
        Postari("SC-NIR-01", d.Id, N.FelTranzactie.Operare, Ianuarie, Randuri(d, 0, 6, 75));
        Sold("SC-NIR-01", d.Linii[0], 6, 75); Datorie("SC-NIR-01", d.Id, 75);
        Storneaza(d.Id, new(An, 1, 20));
        Postari("SC-NIR-03", d.Id, N.FelTranzactie.Storno, new(An, 1, 20), Inverse(Randuri(d, 0, 6, 75)));
        Sold("SC-NIR-03", d.Linii[0], 0, 0); Datorie("SC-NIR-03", d.Id, 0);
        var stamp = Amprenta(d.Id); Refuza("SC-NIR-03", () => Storneaza(d.Id, new(An, 1, 21)), "Operat");
        Verifica("SC-NIR-03", "repetarea nu scrie", stamp == Amprenta(d.Id));
        var m = Culege((2, 10, null), (3, 20, null)); Opereaza(m.Id);
        Postari("SC-NIR-02", m.Id, N.FelTranzactie.Operare, Ianuarie, [.. Randuri(m, 0, 2, 20), .. Randuri(m, 1, 3, 60)]);
        Sold("SC-NIR-02", m.Linii[0], 2, 20); Sold("SC-NIR-02", m.Linii[1], 3, 60); Datorie("SC-NIR-02", m.Id, 80);
        var a = Culege((2, 10, null)); Opereaza(a.Id); Anuleaza(a.Id); FaraEfecte("SC-NIR-05", a.Id);
        Sold("SC-NIR-05", a.Linii[0], 0, 0); Opereaza(a.Id);
        Postari("SC-NIR-05", a.Id, N.FelTranzactie.Operare, Ianuarie, Randuri(a, 0, 2, 20));
        Dependenti(); Plata(); Conex(); Refuzuri(); Performanta(); Tipuri();
        var s = Culege((2, 15, null)); Opereaza(s.Id);
        var original = Culege((2, 15, null)); Opereaza(original.Id);
        var tarziu = Culege((1, 10, null));
        InchideIanuarie();
        Refuza("SC-NIR-04", () => Anuleaza(s.Id), "închis");
        Storneaza(s.Id, Februarie);
        Postari("SC-NIR-04", s.Id, N.FelTranzactie.Storno, Februarie, Inverse(Randuri(s, 0, 2, 30)));
        Sold("SC-NIR-04", s.Linii[0], 2, 30); Sold("SC-NIR-04", s.Linii[0], 0, 0, Februarie);
        Corectie(original);
        Refuza("SC-NIR-11", () => Opereaza(tarziu.Id), "închis"); FaraEfecte("SC-NIR-11", tarziu.Id);
        Comanda(os => { os.GetObjectByKey<Document>(tarziu.Id).DataInregistrare = Februarie; os.CommitChanges(); });
        Opereaza(tarziu.Id);
        var partida = Privat ? N.Unitate.DeschidePartida(Cont(ContFurnizor), Furnizor, tarziu.Id, Februarie).Id : (Guid?)null;
        Postari("SC-NIR-11", tarziu.Id, N.FelTranzactie.Operare, Februarie, Randuri(tarziu, 0, 1, 10, partida: partida));
        Sold("SC-NIR-11", tarziu.Linii[0], 0, 0); Sold("SC-NIR-11", tarziu.Linii[0], 1, 10, Februarie);
    }

    void Dependenti() {
        var d = Culege((2, 25, null)); Opereaza(d.Id);
        var bcs = Consum(d.Linii[0].Lot!.Value, 1); Opereaza(bcs);
        Refuza("SC-NIR-07", () => Storneaza(d.Id, new(An, 1, 20)), "Sold negativ");
        Sold("SC-NIR-07", d.Linii[0], 1, 25);
        Storneaza(bcs, new(An, 1, 20)); Storneaza(d.Id, new(An, 1, 21));
        Sold("SC-NIR-07", d.Linii[0], 0, 0);
    }

    void Plata() {
        var d = Culege((2, 25, null)); Opereaza(d.Id);
        var p = Trezorerie(false, 20); Opereaza(p.Id); var imp = Imperecheaza(p.Id, d.Id, 20, Ianuarie);
        Datorie("SC-NIR-08", d.Id, 30);
        Refuza("SC-NIR-08", () => Storneaza(p.Id, new(An, 1, 20)), "imperecheri");
        Comanda(os => ImperechereService.Desfa(os, imp, new(An, 1, 20)));
        Datorie("SC-NIR-08", d.Id, 50);
        Storneaza(p.Id, new(An, 1, 20)); Datorie("SC-NIR-08", d.Id, 50);
        Storneaza(d.Id, new(An, 1, 21)); Datorie("SC-NIR-08", d.Id, 0);
    }

    void Corectie(FacturaScena original) {
        var id = Corecteaza(original.Id);
        var nou = CuSpatiu(os => {
            var d = os.GetObjectByKey<NIR>(id); var l = d.Detalii.OfType<NirDetaliu>().Single();
            l.Cantitate = 3; l.PretUnitar = 20;
            Verifica("SC-NIR-06", "corecție legată cu lot nou", d.CorecteazaId == original.Id && l.LotId != original.Linii[0].Lot);
            os.CommitChanges(); return new FacturaScena(id, [new(l.ID, l.LotId, l.ProdusId)]);
        });
        Opereaza(id);
        Postari("SC-NIR-06", original.Id, N.FelTranzactie.Storno, Februarie, Inverse(Randuri(original, 0, 2, 30)));
        var partida = Privat ? N.Unitate.DeschidePartida(Cont(ContFurnizor), Furnizor, id, Februarie).Id : (Guid?)null;
        Postari("SC-NIR-06", id, N.FelTranzactie.Operare, Februarie, Randuri(nou, 0, 3, 60, partida: partida));
        Sold("SC-NIR-06", original.Linii[0], 2, 30); Sold("SC-NIR-06", original.Linii[0], 0, 0, Februarie);
        Sold("SC-NIR-06", nou.Linii[0], 3, 60, Februarie);
    }

    void Conex() {
        var f = Factura(Ianuarie, new LinieFctScena(4, 25, Privat ? "SFD" : "CAP0"));
        var nir = Opereaza(f.Id).ConexId!.Value; var stamp = Amprenta(f.Id);
        void Neschimbat(string pas) => Verifica("SC-NIR-09", pas + ": fără cub propriu, sursa intactă", stamp == Amprenta(f.Id)
            && CuSpatiu(os => !os.GetObjectsQuery<C.Tranzactie>().Any(t => t.DocumentId == nir)));
        Verifica("SC-NIR-09", "dry-run conex acceptat", CuSpatiu(os => OperareApi.Valideaza(os, nir)).Count == 0);
        Opereaza(nir); Neschimbat("operare");
        Sold("SC-NIR-09", f.Linii[0], 4, 100);
        using (var os = Deschide()) {
            var raport = DiagnosticValoriStoc.Citeste(((EFCoreObjectSpace)os).DbContext, new(An, 1, 31), [f.Linii[0].Lot!.Value]);
            Verifica("SC-NIR-09", "diagnostic: conexul atribuit sursei chiar cu tipul NIR migrat", !raport.AreAbateri);
            var abateri = ReconciliereCub.Ruleaza(((EFCoreObjectSpace)os).DbContext, [f.Id, nir]);
            Verifica("SC-NIR-09", "reconciliere: conexul nu este un al doilea cap de grup", abateri.Count == 0);
        }
        Anuleaza(nir); Neschimbat("anulare"); Opereaza(nir); Neschimbat("reoperare");
        Comanda(os => OperareApi.Storneaza(os, nir, new(An, 1, 20))); Neschimbat("storno");
        Storneaza(f.Id, new(An, 1, 20)); Sold("SC-NIR-09", f.Linii[0], 0, 0);
        var sursa = Culege((1, 1, null)); Opereaza(sursa.Id);
        foreach (var cuSursa in new[] { false, true }) {
            var d = Culege((1, 10, null));
            Comanda(os => { var doc = os.GetObjectByKey<Document>(d.Id); doc.Autogenerat = true;
                doc.DocumentSursaId = cuSursa ? sursa.Id : null; os.CommitChanges(); });
            Opereaza(d.Id); Postari("SC-NIR-10", d.Id, N.FelTranzactie.Operare, Ianuarie, Randuri(d, 0, 1, 10));
        }
    }

    void Refuzuri() {
        foreach (var caz in new[] { "zero", "negativa", "pret", "pretNegativ", "lot", "gest", "tip" }) {
            var d = Culege((1, 10, null));
            Comanda(os => {
                var l = os.GetObjectByKey<NirDetaliu>(d.Linii[0].Id);
                if (caz == "zero") l.Cantitate = 0;
                if (caz == "negativa") l.Cantitate = -1;
                if (caz == "pret") l.PretUnitar = 0;
                if (caz == "pretNegativ") l.PretUnitar = -1;
                if (caz == "lot") l.LotId = null;
                if (caz == "gest") os.GetObjectByKey<Document>(d.Id).PrimitorId = Destinatie;
                if (caz == "tip") l.TipMaterialId = Tip(os, Serviciu);
                os.CommitChanges();
            });
            var cod = caz switch { "zero" or "negativa" => CoduriRefuz.CantitateNepozitiva,
                "pret" or "pretNegativ" => CoduriRefuz.ValoareNepozitiva, "lot" => CoduriRefuz.LotLipsa,
                "gest" => CoduriRefuz.ReceptieStructuraInvalida, _ => CoduriRefuz.ProdusAltTip };
            RefuzDeclaratie("SC-NIR-12/" + caz, d.Id, cod);
            Refuza("SC-NIR-12/" + caz, () => Opereaza(d.Id), ""); FaraEfecte("SC-NIR-12/" + caz, d.Id);
        }
        var mic = Culege((.001m, .001m, null)); Opereaza(mic.Id);
        Postari("SC-NIR-14", mic.Id, N.FelTranzactie.Operare, Ianuarie, Randuri(mic, 0, .001m, 0));
        var rotund = Culege((3, 3.333333m, null)); Opereaza(rotund.Id);
        Postari("SC-NIR-14", rotund.Id, N.FelTranzactie.Operare, Ianuarie, Randuri(rotund, 0, 3, 10));
    }

    void Performanta() {
        int Citiri(int nr) {
            var d = Culege(Enumerable.Repeat((1m, 10m, (string)null), nr).ToArray());
            using var os = Deschide(); var doc = os.GetObjectByKey<Document>(d.Id);
            NumaratorSql.Instanta.Reseteaza(); var c = Contractare.Contracteaza(os, doc); var n = NumaratorSql.Instanta.Numar;
            Verifica("SC-NIR-13", "contract acceptat și determinist", c.EsteAcceptat && c == Contractare.Contracteaza(os, doc)); return n;
        }
        var mic = Citiri(2); var mare = Citiri(51);
        Verifica("SC-NIR-13", $"2/51 linii: {mic}/{mare} interogări", mic == mare && mare > 0 && mare <= ProbeNucleu.PragInterogari);
    }

    void Tipuri() {
        var tip = Privat ? "371" : "303.02.00";
        var d = Culege((2, 25, tip)); Opereaza(d.Id);
        Postari("SC-NIR-15", d.Id, N.FelTranzactie.Operare, Ianuarie, Randuri(d, 0, 2, 50, tip));
        var btr = Iesire(true, (d.Linii[0], 1)); Opereaza(btr.Id);
        Sold("SC-NIR-15", d.Linii[0], 1, 25); Sold("SC-NIR-15", d.Linii[0], 1, 25, gest: Destinatie);
    }
}
