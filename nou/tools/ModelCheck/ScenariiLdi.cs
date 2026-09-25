using Atlas.Conta.BackOffice.Module.Api;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Declaratii;
using DevExpress.ExpressApp;
using N = Atlas.Conta.Nucleu;
using C = Atlas.Conta.BackOffice.Module.Cub;

namespace Atlas.Conta.BackOffice.ModelCheck;

sealed partial class ScenariiLdi(Func<IObjectSpace> deschide, Action<string, bool> check,
    bool privat, Action<IObjectSpace, int, int> inchide)
    : ScenaDocumente(deschide, check, privat, inchide, "LDI", 2015) {
    record Linie(DirectieDiferenta Directie, decimal Q, decimal Pret = 0,
        LinieScena Lot = null, string Tip = null);
    string Cost => Privat ? "602" : "602.01.00";
    string Venit => Privat ? "7588" : "791.00.00";
    int numar;

    FacturaScena Culege(params Linie[] linii) => CulegeLa(Magazie, Ianuarie, linii);

    FacturaScena CulegeLa(Guid gestiune, DateOnly data, params Linie[] linii) {
        using var os = Deschide();
        var doc = os.CreateObject<ListaDiferenteInventar>();
        doc.Data = data; doc.DataInregistrare = data;
        doc.PredatorId = gestiune; doc.PrimitorId = Loc;
        var rezultat = new List<LinieScena>();
        foreach (var spec in linii) {
            var l = os.CreateObject<ListaDiferenteInventarDetaliu>(); l.Document = doc;
            l.Pozitie = rezultat.Count + 1; l.Directie = spec.Directie; l.Cantitate = spec.Q;
            l.TipMaterialId = Tip(os, spec.Tip ?? Stoc); l.CodEconomicId = Economic;
            if (spec.Directie == DirectieDiferenta.Plus) {
                l.PretEvaluare = spec.Pret;
                Produs produs;
                if (spec.Lot?.Produs is Guid id) produs = os.GetObjectByKey<Produs>(id);
                else {
                    produs = os.CreateObject<Produs>(); produs.Cod = Marcaj + "-P" + ++numar;
                    produs.Denumire = produs.Cod; produs.UM = "BUC"; produs.TipMaterialId = l.TipMaterialId;
                }
                l.ProdusId = produs.ID;
                var lot = l.CreeazaLot(os, produs, os.GetObjectByKey<Gestiune>(gestiune));
                rezultat.Add(new(l.ID, lot.ID, produs.ID));
            }
            else { l.LotId = spec.Lot?.Lot; rezultat.Add(new(l.ID, l.LotId, spec.Lot?.Produs)); }
        }
        os.CommitChanges(); return new(doc.ID, rezultat.ToArray());
    }

    RandScena[] Randuri(FacturaScena doc, int i, decimal q, decimal v,
            string stoc = null, string cost = null, Guid? gest = null) {
        var l = doc.Linii[i]; var plus = q > 0;
        return [new(Cont(stoc ?? Stoc), plus ? N.Latura.Debit : N.Latura.Credit,
                v, q, gest ?? Magazie, l.Lot, l.Produs, Linie: l.Id, Spatiu: N.Spatiu.Stoc, Economic: Economic),
            new(Cont(plus ? Venit : cost ?? Cost), plus ? N.Latura.Credit : N.Latura.Debit,
                v, -q, plus ? N.GestiuniVirtuale.Inventar : N.GestiuniVirtuale.Consum,
                Produs: l.Produs, Linie: l.Id, Economic: Economic)];
    }
    static RandScena[] Inverse(params RandScena[] r) =>
        [.. r.Select(p => p with { Cantitate = -p.Cantitate, Valoare = -p.Valoare })];
    void Sold(string id, LinieScena lot, decimal q, decimal v, DateOnly? data = null, Guid? gest = null) =>
        SoldLot(id, lot.Lot!.Value, gest ?? Magazie, data ?? new(An, 1, 31), q, v);

    protected override void Executa() {
        ProbeLdiOperand.Ruleaza((nume, rezultat) => Verifica("SC-LDI-PUR", nume, rezultat));
        Comanda(os => { os.GetObjectByKey<Repartitor>(Loc).Calitati |= CalitateRepartitor.Comisie; os.CommitChanges(); });
        var plus = Culege(new Linie(DirectieDiferenta.Plus, 2, 15));
        var minusLot = Receptioneaza(new LinieFctScena(10, 10)).Linii[0];
        var minus = Culege(new Linie(DirectieDiferenta.Minus, 4, Lot: minusLot));
        var p = Contract(plus.Id); var m = Contract(minus.Id);
        Verifica("SC-LDI-01", "plusul draft acceptat cu prețul lotului încă zero", p.EsteAcceptat
            && CuSpatiu(os => os.GetObjectByKey<Lot>(plus.Linii[0].Lot!.Value).PretUnitar) == 0);
        Verifica("SC-LDI-02", "minusul cu cantitate pozitivă culeasă este acceptat", m.EsteAcceptat);
        Comanda(os => { os.GetObjectByKey<DocumentDetaliu>(minus.Linii[0].Id).Cantitate = -4; os.CommitChanges(); });
        Verifica("SC-LDI-02", "semnul normalizat nu schimbă declarația", m == Contract(minus.Id));
        Verifica("SC-LDI-01", "dry-run acceptat", CuSpatiu(os => OperareApi.Valideaza(os, plus.Id)).Count == 0);
        FaraEfecte("SC-LDI-01", plus.Id); Opereaza(plus.Id); Opereaza(minus.Id);
        Postari("SC-LDI-01", plus.Id, N.FelTranzactie.Operare, Ianuarie, Randuri(plus, 0, 2, 30));
        Postari("SC-LDI-02", minus.Id, N.FelTranzactie.Operare, Ianuarie, Randuri(minus, 0, -4, 40));
        Sold("SC-LDI-01", plus.Linii[0], 2, 30); Sold("SC-LDI-02", minusLot, 6, 60);
        Storneaza(plus.Id, Ianuarie);
        Postari("SC-LDI-04", plus.Id, N.FelTranzactie.Storno, Ianuarie, Inverse(Randuri(plus, 0, 2, 30)));
        Sold("SC-LDI-04", plus.Linii[0], 0, 0);
        var stamp = Amprenta(plus.Id);
        Refuza("SC-LDI-04", () => Storneaza(plus.Id, Ianuarie), "Operat");
        Verifica("SC-LDI-04", "repetarea nu scrie", stamp == Amprenta(plus.Id));
        Mixt(); Rotunjiri(); Dependenti(); Refuzuri(); Performanta(); Marfuri();
        var dupaInchidereFolosinta = Folosinta();
        var corectie = Culege(new Linie(DirectieDiferenta.Plus, 2, 15)); Opereaza(corectie.Id);
        var intarziat = Culege(new Linie(DirectieDiferenta.Plus, 1, 10));
        InchideIanuarie();
        dupaInchidereFolosinta();
        Refuza("SC-LDI-05", () => Anuleaza(minus.Id), "închis");
        Storneaza(minus.Id, Februarie);
        Postari("SC-LDI-05", minus.Id, N.FelTranzactie.Storno, Februarie, Inverse(Randuri(minus, 0, -4, 40)));
        Sold("SC-LDI-05", minusLot, 6, 60); Sold("SC-LDI-05", minusLot, 10, 100, Februarie);
        Corectie(corectie);
        Refuza("SC-LDI-12", () => Opereaza(intarziat.Id), "închis"); FaraEfecte("SC-LDI-12", intarziat.Id);
        Comanda(os => { os.GetObjectByKey<Document>(intarziat.Id).DataInregistrare = Februarie; os.CommitChanges(); });
        Opereaza(intarziat.Id);
        Postari("SC-LDI-12", intarziat.Id, N.FelTranzactie.Operare, Februarie, Randuri(intarziat, 0, 1, 10));
        Sold("SC-LDI-12", intarziat.Linii[0], 0, 0); Sold("SC-LDI-12", intarziat.Linii[0], 1, 10, Februarie);
    }

    N.Contract Contract(Guid id) => CuSpatiu(os => Contractare.Contracteaza(os, os.GetObjectByKey<Document>(id)));

    void Mixt() {
        var lot = Receptioneaza(new LinieFctScena(5, 20)).Linii[0];
        var doc = Culege(new Linie(DirectieDiferenta.Minus, 2, Lot: lot), new(DirectieDiferenta.Plus, 3, 7));
        Opereaza(doc.Id);
        RandScena[] r = [.. Randuri(doc, 0, -2, 40), .. Randuri(doc, 1, 3, 21)];
        Postari("SC-LDI-03", doc.Id, N.FelTranzactie.Operare, Ianuarie, r);
        Sold("SC-LDI-03", lot, 3, 60); Sold("SC-LDI-03", doc.Linii[1], 3, 21);
        Anuleaza(doc.Id); FaraEfecte("SC-LDI-06", doc.Id);
        Sold("SC-LDI-06", lot, 5, 100); Sold("SC-LDI-06", doc.Linii[1], 0, 0);
        Opereaza(doc.Id); Postari("SC-LDI-06", doc.Id, N.FelTranzactie.Operare, Ianuarie, r);
    }

    void Rotunjiri() {
        var lot = Receptioneaza(new LinieFctScena(3, 3.333333m)).Linii[0];
        var doc = Culege(new Linie(DirectieDiferenta.Minus, 1, Lot: lot), new(DirectieDiferenta.Minus, 2, Lot: lot));
        Opereaza(doc.Id);
        Postari("SC-LDI-08", doc.Id, N.FelTranzactie.Operare, Ianuarie,
            [.. Randuri(doc, 0, -1, 3.33m), .. Randuri(doc, 1, -2, 6.67m)]);
        Sold("SC-LDI-08", lot, 0, 0);
        var dual = Receptioneaza(new LinieFctScena(3, 3.333333m)).Linii[0];
        foreach (var v in new[] { 3.33m, 3.34m, 3.33m }) {
            var d = Culege(new Linie(DirectieDiferenta.Minus, 1, Lot: dual)); Opereaza(d.Id);
            Postari("SC-LDI-09", d.Id, N.FelTranzactie.Operare, Ianuarie, Randuri(d, 0, -1, v));
        }
        Sold("SC-LDI-09/T-r13", dual, 0, 0);
    }

    void Dependenti() {
        var p = Culege(new Linie(DirectieDiferenta.Plus, 1, 25)); Opereaza(p.Id);
        var bcs = Consum(p.Linii[0].Lot!.Value, 1); Opereaza(bcs);
        var stamp = Amprenta(p.Id);
        Refuza("SC-LDI-10", () => Storneaza(p.Id, Ianuarie), "STOC_INSUFICIENT");
        Verifica("SC-LDI-10", "original intact", stamp == Amprenta(p.Id));
        Storneaza(bcs, new(An, 1, 20)); Storneaza(p.Id, new(An, 1, 21)); Sold("SC-LDI-10", p.Linii[0], 0, 0);
        var initial = Culege(new Linie(DirectieDiferenta.Plus, 1, 10)); Opereaza(initial.Id);
        Opereaza(Culege(new Linie(DirectieDiferenta.Minus, 1, Lot: initial.Linii[0])).Id);
        var nou = Culege(new Linie(DirectieDiferenta.Plus, 1, 12, initial.Linii[0])); Opereaza(nou.Id);
        Verifica("SC-LDI-11", "același produs, lot nou", initial.Linii[0].Produs == nou.Linii[0].Produs
            && initial.Linii[0].Lot != nou.Linii[0].Lot);
        Sold("SC-LDI-11", initial.Linii[0], 0, 0); Sold("SC-LDI-11", nou.Linii[0], 1, 12);
    }

    void Corectie(FacturaScena original) {
        var id = Corecteaza(original.Id); FaraEfecte("SC-LDI-07", id);
        var nou = CuSpatiu(os => {
            var doc = os.GetObjectByKey<ListaDiferenteInventar>(id);
            var l = doc.Detalii.OfType<ListaDiferenteInventarDetaliu>().Single(); l.Cantitate = 3; l.PretEvaluare = 20;
            Verifica("SC-LDI-07", "corecție legată și lot nou", doc.CorecteazaId == original.Id && l.LotId != original.Linii[0].Lot);
            os.CommitChanges(); return new FacturaScena(id, [new(l.ID, l.LotId, l.ProdusId)]);
        });
        Opereaza(id);
        Postari("SC-LDI-07", original.Id, N.FelTranzactie.Storno, Februarie, Inverse(Randuri(original, 0, 2, 30)));
        Postari("SC-LDI-07", id, N.FelTranzactie.Operare, Februarie, Randuri(nou, 0, 3, 60));
        Sold("SC-LDI-07", original.Linii[0], 0, 0, Februarie); Sold("SC-LDI-07", nou.Linii[0], 3, 60, Februarie);
    }

    void Refuzuri() {
        var lot = Receptioneaza(new LinieFctScena(1, 10)).Linii[0];
        var mic = Culege(new Linie(DirectieDiferenta.Plus, .001m, .001m)); Opereaza(mic.Id);
        Postari("SC-LDI-20", mic.Id, N.FelTranzactie.Operare, Ianuarie, Randuri(mic, 0, .001m, 0m));
        Sold("SC-LDI-20", mic.Linii[0], .001m, 0m);
        foreach (var caz in new[] { "zero", "lot", "directie", "pret", "negativ", "strain", "gestiune", "insuficient" }) {
            var plus = caz is "pret" or "negativ" or "strain" or "gestiune";
            var d = Culege(new Linie(plus ? DirectieDiferenta.Plus : DirectieDiferenta.Minus,
                caz == "insuficient" ? 2 : 1, 10, plus ? null : lot));
            Comanda(os => {
                var l = os.GetObjectByKey<ListaDiferenteInventarDetaliu>(d.Linii[0].Id);
                if (caz == "zero") l.Cantitate = 0;
                if (caz == "lot") l.LotId = null;
                if (caz == "directie") l.Directie = default;
                if (caz == "pret") l.PretEvaluare = 0;
                if (caz == "negativ") l.PretEvaluare = -1;
                if (caz == "strain") { l.LotId = lot.Lot; l.ProdusId = lot.Produs; }
                if (caz == "gestiune") os.GetObjectByKey<Lot>(l.LotId!.Value).GestiuneId = Destinatie;
                os.CommitChanges();
            });
            var cod = caz switch { "zero" => CoduriRefuz.CantitateNepozitiva, "lot" => CoduriRefuz.LotLipsa,
                "pret" or "negativ" => CoduriRefuz.ValoareNepozitiva,
                "insuficient" => "STOC_INSUFICIENT", _ => CoduriRefuz.InventarStructuraInvalida };
            RefuzDeclaratie("SC-LDI-13/" + caz, d.Id, cod);
            Refuza("SC-LDI-13/" + caz, () => Opereaza(d.Id), ""); FaraEfecte("SC-LDI-13/" + caz, d.Id);
        }
        var propriu = Culege(new Linie(DirectieDiferenta.Plus, 1, 10), new(DirectieDiferenta.Minus, 1, Lot: lot));
        Comanda(os => { os.GetObjectByKey<DocumentDetaliu>(propriu.Linii[1].Id).LotId = propriu.Linii[0].Lot; os.CommitChanges(); });
        RefuzDeclaratie("SC-LDI-13/propriu", propriu.Id, CoduriRefuz.InventarStructuraInvalida);
        Refuza("SC-LDI-13/propriu", () => Opereaza(propriu.Id), ""); FaraEfecte("SC-LDI-13/propriu", propriu.Id);
    }

    void Performanta() {
        var lot = Receptioneaza(new LinieFctScena(50, 1)).Linii[0];
        var mic = Culege(new Linie(DirectieDiferenta.Minus, 1, Lot: lot), new(DirectieDiferenta.Plus, 1, 1));
        var mare = Culege([.. Enumerable.Repeat(new Linie(DirectieDiferenta.Minus, 1, Lot: lot), 50), new(DirectieDiferenta.Plus, 1, 1)]);
        int Citiri(Guid id) {
            using var os = Deschide(); var doc = os.GetObjectByKey<Document>(id);
            NumaratorSql.Instanta.Reseteaza(); var c = Contractare.Contracteaza(os, doc); var n = NumaratorSql.Instanta.Numar;
            Verifica("SC-LDI-16", "declarație acceptată și deterministă", c.EsteAcceptat && c == Contractare.Contracteaza(os, doc)); return n;
        }
        var mici = Citiri(mic.Id); var mari = Citiri(mare.Id);
        Verifica("SC-LDI-16", $"2/51 linii: {mici}/{mari} interogări", mici == mari && mari > 0 && mari <= ProbeNucleu.PragInterogari);
    }

    void Marfuri() {
        if (!Privat) return;
        var p = Culege(new Linie(DirectieDiferenta.Plus, 2, 15, Tip: "371")); Opereaza(p.Id);
        var m = Culege(new Linie(DirectieDiferenta.Minus, 1, Lot: p.Linii[0], Tip: "371")); Opereaza(m.Id);
        Postari("SC-LDI-17", p.Id, N.FelTranzactie.Operare, Ianuarie, Randuri(p, 0, 2, 30, "371", "607"));
        Postari("SC-LDI-17", m.Id, N.FelTranzactie.Operare, Ianuarie, Randuri(m, 0, -1, 15, "371", "607"));
        Sold("SC-LDI-17", p.Linii[0], 1, 15);
    }
}
