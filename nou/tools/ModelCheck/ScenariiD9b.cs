using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Motor;
using Atlas.Conta.BackOffice.Module.Cub.Citiri;
using Atlas.Conta.BackOffice.Module.Proiectii;
using DevExpress.ExpressApp;
using P = Atlas.Conta.BackOffice.Module.Cub.Citiri.Partide;
using C = Atlas.Conta.BackOffice.Module.Cub;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.ModelCheck;

sealed partial class ScenariiD9b(Func<IObjectSpace> deschide, Action<string, bool> check,
    bool privat, Action<IObjectSpace, int, int> inchide)
    : ScenaDocumente(deschide, check, privat, inchide, "D9B", 1977) {
    protected override void Executa() {
        Nominalizata();
        Titulari();
        DouaConturi();
        Transfer();
        Retragere();
        Storno();
        Fifo();
        IstorieNeutra();
        Atomicitate();
        Concurenta();
        Securitate();
        ScrierePeSet();
        Initiale();
        Snapshot();
    }

    void Titular(string nume) => Furnizor = CuSpatiu(os => {
        var p = os.CreateObject<Partener>(); p.Cod = Marcaj + "-" + nume; p.Denumire = p.Cod;
        os.CommitChanges(); return p.ID;
    });

    Guid Factura(decimal suma) {
        var f = base.Factura(Ianuarie, new LinieFctScena(1, suma, Stoc: false));
        Opereaza(f.Id); return f.Id;
    }

    Guid Plata(Guid factura, decimal suma, DateOnly? data = null) {
        var p = Trezorerie(false, suma);
        Comanda(os => {
            var d = os.GetObjectByKey<Document>(p.Id);
            d.Autogenerat = true; d.DocumentSursaId = factura; d.Data = data ?? Ianuarie; d.DataInregistrare = d.Data;
            os.CommitChanges();
        });
        Opereaza(p.Id); return p.Id;
    }

    void Rest(string cod, Guid doc, decimal net, DateOnly? data = null, CitireCumul citire = CitireCumul.Vizibila) {
        var id = Partida(doc, ContFurnizor).Value;
        Verifica(cod, $"A: sold semnat {net}, origine {doc}, {citire}", CuSpatiu(os => {
            var s = P.Cumulate(os, citire, data ?? Ianuarie).Where(x => x.UnitateId == id).ToArray();
            var r = ImperecheriProiectii.PartideCuRest(os, Furnizor, laData: data ?? Ianuarie, citire: citire)
                .SingleOrDefault(x => x.UnitateId == id);
            return s.Sum(x => x.Debit - x.Credit) == net && (net == 0 ? r == null
                : r != null && r.DocumentId == doc && r.Rest == Math.Abs(net));
        }));
    }

    void Origini(string cod, params Guid[] documente) => Verifica(cod, $"P: exact {documente.Length} origini persistate", CuSpatiu(os =>
        P.Origini(os).Where(x => x.PartenerId == Furnizor).Select(x => x.DocumentId).ToArray()
            .Order().SequenceEqual(documente.Select(x => (Guid?)x).Order())));

    void Nominalizata() {
        Titular("NOM"); var a = Factura(100); var p = Plata(a, 40);
        Rest("SC-D9B-01", a, -60); Origini("SC-D9B-01", a);
        var b = Factura(70);
        Rest("SC-D9B-02", a, -60); Rest("SC-D9B-02", b, -70); Origini("SC-D9B-02", a, b);
        var imp = CuSpatiu(os => os.GetObjectsQuery<Imperechere>().Single(x => x.DocumentStingatorId == p).ID);
        Comanda(os => ImperechereService.Sterge(os, imp));
        Anuleaza(p); Rest("SC-D9B-05", a, -100);
        Opereaza(p); Rest("SC-D9B-05", a, -60); Origini("SC-D9B-05", a, b);
    }

    void Titulari() {
        Titular("T1"); var t1 = Furnizor; Titular("T2"); var t2 = Furnizor;
        var n = Nota(Ianuarie, new(Serviciu, ContFurnizor, 100, RepartitorCredit: t1),
            new(Serviciu, ContFurnizor, 100, RepartitorCredit: t2)); Opereaza(n.Id);
        Verifica("SC-D9B-03a", "A: două origini pe același cont, titulari diferiți", CuSpatiu(os =>
            P.Origini(os).Count(x => x.DocumentId == n.Id) == 2));
        var s = Nota(Ianuarie, new LinieNtcScena(ContFurnizor, Serviciu, 100, t1)); Opereaza(s.Id);
        Furnizor = t1; Rest("SC-D9B-03a", n.Id, 0);
        Furnizor = t2; Rest("SC-D9B-03a", n.Id, -100);
    }

    void Transfer() {
        Titular("TRANSFER"); var f = Factura(100); var p = Trezorerie(false, 40); Opereaza(p.Id);
        var imp = Imperecheaza(p.Id, f, 40, Ianuarie);
        Rest("SC-D9B-04", f, -60); Rest("SC-D9B-04", p.Id, 0); Origini("SC-D9B-04", f, p.Id);
        Comanda(os => ImperechereService.Desfa(os, imp, Ianuarie));
        Rest("SC-D9B-04", f, -100); Rest("SC-D9B-04", p.Id, 40);
        Verifica("SC-D9B-04", "A: transferurile au zero rulaj pe cont și sens", CuSpatiu(os =>
            P.Postari(os).Where(x => x.Repartitor == Furnizor && x.Tranzactie.Fel == N.FelTranzactie.Transfer)
                .GroupBy(x => new { x.Cont, x.Sens }).Select(g => g.Sum(x => x.Valoare)).ToArray().All(x => x == 0)));
    }

    void DouaConturi() {
        Titular("DOUA-CONTURI");
        var tipuri = CuSpatiu(os => new[] { ContFurnizor, ContClient }.Select((simbol, i) => {
            var tip = os.CreateObject<TipMaterial>(); tip.Cod = $"{Marcaj}-TRZ-{i}"; tip.Denumire = tip.Cod;
            tip.ClasaId = os.GetObjectByKey<TipMaterial>(Tip(os, "TRZ")).ClasaId;
            var r = os.CreateObject<RegulaContare>(); r.TipMaterial = tip;
            r.TipDocumentId = os.GetObjectsQuery<TipDocument>().Single(x => x.Cod == "PLT").ID;
            r.SursaContDebit = SursaCont.Explicit; r.ContDebitId = Cont(simbol);
            r.SursaContCredit = SursaCont.RepartitorPredator;
            os.CommitChanges(); return tip.ID;
        }).ToArray());
        var p = Trezorerie(false, 40, 60);
        Comanda(os => {
            for (var i = 0; i < 2; i++) os.GetObjectByKey<DocumentTrezorerieDetaliu>(p.Linii[i].Id).TipMaterialId = tipuri[i];
            os.CommitChanges();
        });
        Opereaza(p.Id);
        Verifica("SC-D9B-03b", "A: plata are două origini, pe conturi distincte, resturi 40/60", CuSpatiu(os => {
            var origini = P.Origini(os).Where(x => x.DocumentId == p.Id).ToArray();
            var solduri = P.Solduri(os, Ianuarie).Where(x => x.PartenerId == Furnizor).ToArray();
            return origini.Length == 2 && origini.Select(x => x.ContId).Distinct().Count() == 2
                && solduri.Single(x => x.ContId == Cont(ContFurnizor)).Debit == 40
                && solduri.Single(x => x.ContId == Cont(ContClient)).Debit == 60;
        }));
    }

    void Retragere() {
        Titular("RETRAGERE"); var f = Factura(100); var id = Partida(f, ContFurnizor).Value;
        Anuleaza(f); Origini("SC-D9B-05");
        var alta = new DateOnly(An, 1, 7);
        Comanda(os => { var d = os.GetObjectByKey<Document>(f); d.Data = alta; d.DataInregistrare = alta; os.CommitChanges(); });
        Opereaza(f); Origini("SC-D9B-05", f); Rest("SC-D9B-05", f, -100, alta);
        Verifica("SC-D9B-05", "A: aceeași identitate, data noii operări", CuSpatiu(os =>
            P.Postari(os).Where(x => x.DocumentId == f).All(x => x.Unitate == id && x.UnitateDeschisa == alta)));
    }

    void Storno() {
        Titular("STORNO"); var f = Factura(100);
        Storneaza(f, new(An, 1, 10));
        Rest("SC-D9B-06", f, -100); Rest("SC-D9B-06", f, 0, new(An, 1, 10)); Origini("SC-D9B-06", f);
    }

    void Fifo() {
        Titular("FIFO"); var a = Factura(60); var b = Factura(40);
        var n = Nota(Ianuarie, new LinieNtcScena(ContFurnizor, Serviciu, 30, Furnizor)); Opereaza(n.Id);
        Rest("SC-D9B-08", a, -30); Rest("SC-D9B-08", b, -40);
    }

    void IstorieNeutra() {
        Titular("NEUTRU"); var f = Factura(100);
        var n = Nota(Ianuarie, new LinieNtcScena(ContFurnizor, Serviciu, 40, Furnizor,
            PartidaDebit: Partida(f, ContFurnizor))); Opereaza(n.Id); Storneaza(n.Id, Ianuarie);
        Anuleaza(f); Origini("SC-D9B-12");
        Verifica("SC-D9B-12", "A: două postări străine +40/-40 rămân", CuSpatiu(os => {
            var id = Partida(f, ContFurnizor).Value;
            var p = P.Postari(os).Where(x => x.Unitate == id).Select(x => x.Valoare).ToArray();
            return p.Order().SequenceEqual(new[] { -40m, 40m });
        }));
        Opereaza(f); Origini("SC-D9B-12", f); Rest("SC-D9B-12", f, -100);
    }

    void Snapshot() {
        Titular("SNAPSHOT"); var f = Factura(100); Plata(f, 40);
        Inchide(An, 1);
        foreach (var c in new[] { CitireCumul.Vizibila, CitireCumul.Integrala })
            Rest("SC-D9B-09", f, -60, new(An, 1, 31), c);
        Plata(f, 20, Februarie);
        foreach (var c in new[] { CitireCumul.Vizibila, CitireCumul.Integrala }) {
            Rest("SC-D9B-09", f, -40, Februarie, c);
            Rest("SC-D9B-09", f, -60, new(An, 1, 31), c);
        }
    }

    void Initiale() {
        Titular("INITIAL"); var titular = Furnizor;
        Titular("ORDINAL"); var geaman = Furnizor;
        var cont = Cont(ContFurnizor);
        var referinte = Enumerable.Range(0, 4).Select(_ => Guid.NewGuid()).ToArray();
        Guid Id(int i) => N.UnitateSold.DeschidePartidaInitiala(cont, i < 2 ? titular : geaman, referinte[i], Ianuarie).Id;
        if (Id(2).CompareTo(Id(3)) < 0) (referinte[2], referinte[3]) = (referinte[3], referinte[2]);
        Comanda(os => {
            var ancora = os.GetObjectsQuery<Cont>().First(c => c.Simbol.StartsWith("891") && !c.Sumator).ID;
            using var tx = TranzactieComanda.Incepe(os);
            C.Materializare.Deschide(os, Ianuarie,
                [new(cont, N.Sens.Credit, 180, true), new(ancora, N.Sens.Debit, 180)], [],
                Enumerable.Range(0, 4).Select(i => new C.PartidaInitiala(cont, i < 2 ? titular : geaman,
                    referinte[i], N.Sens.Credit, new[] { 60m, 40m, 30m, 50m }[i])
                    { Analiza = new(null, Economic, null, null, null, null) }).ToArray());
            os.CommitChanges(); tx.Commit();
        });
        Verifica("SC-D9B-07/08", "P: referințe reale și ordinalele 1–4 ale deschiderii", CuSpatiu(os => {
            var randuri = os.GetObjectsQuery<C.OriginePartidaPersistata>().Where(o => o.DocumentId == null).ToArray();
            return randuri.Length == 4 && Enumerable.Range(0, 4).All(i => randuri.Any(o => o.ID == Id(i)
                && o.ReferintaInitiala == referinte[i] && o.OrdinalInitial == i + 1
                && o.ContId == cont && o.PartenerId == (i < 2 ? titular : geaman)));
        }));
        void Sold(string cod, int i, decimal rest) => Verifica(cod, $"A: inițială {i}, rest {rest}, fără document", CuSpatiu(os => {
            var id = Id(i);
            var s = P.Solduri(os, Ianuarie).Single(x => x.UnitateId == id);
            return s.Credit - s.Debit == rest && P.Origini(os).Single(x => x.UnitateId == id).DocumentId == null;
        }));
        Furnizor = titular;
        var p = Trezorerie(false, 20); Opereaza(p.Id);
        Comanda(os => {
            using var tx = TranzactieComanda.Incepe(os);
            C.Materializare.Imperecheaza(os, os.GetObjectByKey<Document>(p.Id), Id(0), 20, Ianuarie);
            os.CommitChanges(); tx.Commit();
        });
        Sold("SC-D9B-07", 0, 40); Sold("SC-D9B-07", 1, 40);
        var amprenta = Amprenta(p.Id);
        Refuza("SC-D9B-07", () => Anuleaza(p.Id), "stingere");
        Verifica("SC-D9B-07", "A: refuzul retragerii nu schimbă postările", amprenta == Amprenta(p.Id));
        Storneaza(p.Id, Ianuarie); Sold("SC-D9B-07", 0, 60); Sold("SC-D9B-07", 1, 40);
        Furnizor = geaman;
        var n = Nota(Ianuarie, new LinieNtcScena(ContFurnizor, Serviciu, 40, geaman)); Opereaza(n.Id);
        Sold("SC-D9B-08", 2, 0); Sold("SC-D9B-08", 3, 40);
    }

    protected override void CurataCubSuplimentar(IObjectSpace os, Purja purja) {
        var terti = os.GetObjectsQuery<Partener>().Where(p => p.Cod.StartsWith(Marcaj)).Select(p => p.ID);
        var ids = os.GetObjectsQuery<C.Postare>().Where(p => p.DocumentId == null && terti.Contains(p.Repartitor.Value))
            .Select(p => p.TranzactieId).Distinct().ToList();
        purja.AdaugaCheie<C.Postare>(os.GetObjectsQuery<C.Postare>().Where(p => ids.Contains(p.TranzactieId)).Select(p => p.ID).ToList());
        purja.AdaugaCheie<C.Tranzactie>(ids);
    }

    protected override void CurataNomenclatoare(IObjectSpace os, Purja purja) {
        for (var i = 0; i < 2; i++) CurataPolitica(os, purja, $"{Marcaj}-TRZ-{i}");
    }
}
