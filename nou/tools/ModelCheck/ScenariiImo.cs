using Atlas.Conta.BackOffice.Module.Api;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Declaratii;
using Atlas.Conta.BackOffice.Module.Motor;
using DevExpress.ExpressApp;
using C = Atlas.Conta.BackOffice.Module.Cub;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.ModelCheck;

static class ScenariiImo {
    public static void Ruleaza(Func<IObjectSpace> deschide, Action<string, bool> check, bool privat,
            Action<IObjectSpace, int, int> inchide) {
        foreach (var caz in new[] { "suport", "concurenta", "ciclu", "initial", "fiscal", "liniara", "intarziat",
                "modernizare", "revizuire", "deductibil", "loc", "perf", "accelerata", "degresiva", "reziduala", "inchis",
                "deschidere", "cas-zero", "refuzuri", "concurenta-inversa", "fara-politica" })
            new ScenaImo(deschide, check, privat, inchide, caz).Ruleaza();
    }
}

sealed class ScenaImo(Func<IObjectSpace> deschide, Action<string, bool> check, bool privat,
    Action<IObjectSpace, int, int> inchide, string caz)
    : ScenaDocumente(deschide, check, privat, inchide, "IMO", 2019) {
    string Activ => Privat ? "214" : "214.00.00";
    string Capital => Privat ? "1012" : "117.00.00";
    string Amortizare;
    Guid contActiv, contAmortizare, contCheltuiala, tip;
    int numar;
    Guid? deschidere;
    protected override int UltimulAn => caz is "accelerata" or "degresiva" ? An + 3 : An;

    protected override void Executa() {
        Comanda(os => {
            for (var luna = 3; luna <= 12; luna++) {
                var perioada = os.CreateObject<PerioadaFiscala>(); perioada.An = An; perioada.Luna = luna;
            }
            for (var an = An + 1; an <= UltimulAn; an++)
                for (var luna = 1; luna <= 12; luna++) {
                    var perioada = os.CreateObject<PerioadaFiscala>(); perioada.An = an; perioada.Luna = luna;
                }
            os.CommitChanges();
            tip = Tip(os, Activ); contActiv = Cont(Activ);
            var p = os.GetObjectsQuery<PoliticaAmortizare>().Single(p => p.TipMaterialId == tip);
            contAmortizare = p.ContAmortizareId.Value; contCheltuiala = p.ContCheltuialaAmortizareId.Value;
            Amortizare = os.GetObjectByKey<Cont>(contAmortizare).Simbol;
        });
        switch (caz) {
            case "suport": Suport(); break;
            case "concurenta": Concurenta(); break;
            case "ciclu": Ciclu(); break;
            case "initial": Initial(); break;
            case "fiscal": NumaiFiscal(); break;
            case "liniara": Liniara(); break;
            case "intarziat": Intarziat(); break;
            case "modernizare": Modernizare(); break;
            case "revizuire": Revizuire(); break;
            case "deductibil": Deductibil(); break;
            case "loc": Locuri(); break;
            case "perf": Perf(); break;
            case "accelerata": Grafic(MetodaAmortizare.Accelerata, "SC-IMO-23"); break;
            case "degresiva": Grafic(MetodaAmortizare.Degresiva, "SC-IMO-24"); break;
            case "reziduala": Reziduala(); break;
            case "inchis": Inchis(); break;
            case "deschidere": Deschidere(); break;
            case "cas-zero": CasZero(); break;
            case "refuzuri": Refuzuri(); break;
            case "concurenta-inversa": ConcurentaInversa(); break;
            case "fara-politica": FaraPolitica(); break;
        }
    }
    DateOnly Zi(int luna, int zi = 5) => new(An, luna, zi);
    DateOnly Sfarsit(int luna) => new(An, luna, DateTime.DaysInMonth(An, luna));
    Guid Fisa() => CuSpatiu(os => {
        var f = os.CreateObject<Imobilizare>(); f.NumarInventar = Marcaj + "-F" + ++numar;
        f.Denumire = f.NumarInventar; f.TipMaterialId = tip; f.LocId = Magazie; f.CodEconomicId = Economic;
        os.CommitChanges(); return f.ID;
    });
    FacturaScena Pif(Guid fisa, decimal valoare = 1200, decimal fiscal = 900, int durata = 12,
            int durataFiscala = 18, Guid? sursa = null, decimal initial = 0, decimal initialFiscal = 0,
            int luna = 1, FelLiniePif fel = FelLiniePif.Intrare) => CuSpatiu(os => {
        var p = os.CreateObject<PunereInFunctiune>(); p.Data = Zi(luna); p.DataInregistrare = p.Data;
        p.PredatorId = Loc; p.PrimitorId = Magazie;
        var l = os.CreateObject<PunereInFunctiuneDetaliu>(); l.Document = p; l.Pozitie = 1;
        l.ImobilizareId = fisa; l.TipMaterialId = tip; l.Cantitate = 1; l.Valoare = valoare;
        l.ValoareFiscala = fiscal; l.LinieSursaId = sursa; l.Fel = fel;
        l.AmortizareInitiala = initial; l.AmortizareFiscalaInitiala = initialFiscal;
        l.Metoda = MetodaAmortizare.Liniara; l.MetodaFiscala = MetodaAmortizare.Liniara;
        l.DurataLuni = durata; l.DurataFiscalaLuni = durataFiscala;
        l.CategorieFiscala = CategorieFiscala.Standard; l.UtilizareExclusiva = true;
        os.CommitChanges(); return new FacturaScena(p.ID, [new(l.ID, null, null)]);
    });
    FacturaScena SuportNota(decimal brut = 1200, decimal cumulat = 0, int luna = 1) {
        var linii = new List<LinieNtcScena> { new(Activ, Capital, brut, Magazie, Loc) };
        if (cumulat != 0m) linii.Add(new(Capital, Amortizare, cumulat, Loc, Magazie));
        var n = Nota(Zi(luna), [.. linii]); Opereaza(n.Id); return n;
    }
    Guid Amo(int luna) => Amo(Zi(luna));
    Guid Amo(DateOnly data) => CuSpatiu(os => {
        var d = AmortizareService.Genereaza(os, data.Year, data.Month, Loc)
            ?? throw new InvalidOperationException("Scena cere o AMO eligibilă.");
        os.CommitChanges(); return d.ID;
    });
    Guid Cas(Guid fisa, int luna) => CuSpatiu(os => {
        var p = os.GetObjectsQuery<PoliticaAmortizare>().Single(p => p.TipMaterialId == tip);
        var d = os.CreateObject<IesireImobilizare>(); d.Data = Zi(luna); d.PredatorId = Magazie; d.PrimitorId = Loc;
        foreach (var x in AmortizareService.LiniiIesire(os, fisa, d.Data, p)) {
            var l = os.CreateObject<IesireImobilizareDetaliu>(); l.Document = d; l.ImobilizareId = fisa;
            l.TipMaterialId = tip; l.Fel = x.Fel; l.Valoare = x.Valoare; l.Cantitate = 1;
            l.ContDebitId = x.ContDebitId; l.ContCreditId = x.ContCreditId;
            l.RepartitorDebitId = Magazie; l.RepartitorCreditId = Magazie; l.CodEconomicId = Economic;
        }
        os.CommitChanges(); return d.ID;
    });
    void Sold(string id, Guid fisa, decimal brut, decimal cumul, decimal fiscal, decimal cumulFiscal,
            DateOnly? data = null) {
        Comanda(C.Citiri.Imobilizari.VerificaAcoperire);
        var randuri = CuSpatiu(os => os.GetObjectsQuery<C.Postare>().Where(p => p.Unitate == fisa
            && p.Data <= (data ?? new DateOnly(An, 12, 31))).ToList());
        decimal Net(Guid cont, N.Carte carte) => randuri.Where(p => p.Cont == cont && p.Carte == carte)
            .Sum(p => p.Latura == N.Latura.Debit ? p.Valoare : -p.Valoare);
        Verifica(id, $"fișă: brut/cumulat {brut}/{cumul}, fiscal {fiscal}/{cumulFiscal}",
            Net(contActiv, N.Carte.Contabil) == brut && Net(contAmortizare, N.Carte.Contabil) == -cumul
            && Net(contActiv, N.Carte.Fiscal) == fiscal && Net(contAmortizare, N.Carte.Fiscal) == -cumulFiscal);
        var s = CuSpatiu(os => AmortizareService.Situatie(os, fisa, data ?? new(An, 12, 31)));
        Verifica(id, "cititorul operațional are aceleași patru constante", s.Valoare == brut
            && s.Amortizare == cumul && s.ValoareFiscala == fiscal && s.AmortizareFiscala == cumulFiscal);
    }
    void Origini(string id, Guid doc) {
        var p = CuSpatiu(os => os.GetObjectsQuery<C.Postare>().Where(p => p.DocumentId == doc).ToList());
        var originale = p.Where(p => p.InversaDinId == null).ToDictionary(p => p.ID);
        var inverse = p.Where(p => p.InversaDinId != null).ToList();
        Verifica(id, "fiecare inversă identifică originalul și îi neagă măsurile", inverse.Count == originale.Count
            && inverse.All(i => originale.TryGetValue(i.InversaDinId.Value, out var o)
                && i.InversaDinSpatiu == o.Spatiu && i.Valoare == -o.Valoare && i.Carte == o.Carte
                && i.Unitate == o.Unitate && i.FelUnitate == o.FelUnitate && i.SuportId == o.SuportId));
    }
    void Suport() {
        var f = Fisa(); var lipsa = Pif(f, initial: 200);
        RefuzDeclaratie("SC-IMO-03", lipsa.Id, CoduriRefuz.SuportInsuficient);
        Refuza("SC-IMO-03", () => Opereaza(lipsa.Id), CoduriRefuz.SuportInsuficient); FaraEfecte("SC-IMO-03", lipsa.Id);
        Verifica("SC-IMO-03", "refuzul nu schimbă fișa și nu scrie registrul dual", CuSpatiu(os =>
            os.GetObjectByKey<Imobilizare>(f).Stare == StareImobilizare.Noua
            && !os.GetObjectsQuery<RegistruImobilizari>().Any(r => r.DocumentId == lipsa.Id)));
        var factura = Factura(Ianuarie, new LinieFctScena(1, 1200, Stoc: false, Tip: Activ));
        if (!Privat) Comanda(os => {
            var cf = os.CreateObject<CodFunctional>(); cf.Cod = Marcaj; cf.Denumire = Marcaj;
            var sf = os.CreateObject<SursaFinantare>(); sf.Cod = Marcaj; sf.Denumire = Marcaj;
            var pr = os.CreateObject<Proiect>(); pr.Cod = Marcaj; pr.Denumire = Marcaj;
            var l = os.GetObjectByKey<FacturaIntrareDetaliu>(factura.Linii[0].Id);
            l.CodFunctionalId = cf.ID; l.SursaFinantareId = sf.ID; l.ProiectId = pr.ID; os.CommitChanges();
        });
        Opereaza(factura.Id);
        var p = Pif(f, sursa: factura.Linii[0].Id); Opereaza(p.Id); Sold("SC-IMO-01", f, 1200, 0, 900, 0);
        var transfer = CuSpatiu(os => os.GetObjectsQuery<C.Postare>().Where(r => r.DocumentId == p.Id
            && r.Tranzactie.Fel == N.FelTranzactie.Transfer).ToList());
        Verifica("SC-IMO-01", "Transfer D anonim −1200 / D fișă +1200, cu suport explicit", transfer.Count == 2
            && transfer.Single(r => r.Unitate == null).Valoare == -1200
            && transfer.Single(r => r.Unitate == f).Valoare == 1200
            && transfer.All(r => r.Latura == N.Latura.Debit && r.SuportId != null));
        Verifica("SC-CIT-01", "PIF nu introduce rânduri în citirea contabilă generală", CuSpatiu(os =>
            !C.Citiri.Contabil.Postari(os).Any(r => r.DocumentId == p.Id)));
        Refuza("SC-IMO-06", () => Storneaza(factura.Id, Ianuarie), CoduriRefuz.SuportCuDependenti);
        Anuleaza(p.Id); FaraEfecte("SC-IMO-14", p.Id); Opereaza(p.Id); Storneaza(p.Id, Ianuarie);
        Origini("SC-IMO-15", p.Id); Sold("SC-IMO-15", f, 0, 0, 0, 0);
        Verifica("SC-CIT-01", "storno PIF nu introduce inversele Transfer în contabil", CuSpatiu(os =>
            !C.Citiri.Contabil.Postari(os).Any(r => r.DocumentId == p.Id)));
        var p1 = Pif(Fisa(), 400, 400, sursa: factura.Linii[0].Id);
        var p2 = Pif(Fisa(), 800, 800, sursa: factura.Linii[0].Id);
        Opereaza(p1.Id); Opereaza(p2.Id);
        var p3 = Pif(Fisa(), 1, 1, sursa: factura.Linii[0].Id);
        Refuza("SC-IMO-02", () => Opereaza(p3.Id), "plafon"); FaraEfecte("SC-IMO-02", p3.Id);
        Storneaza(p2.Id, Ianuarie); Storneaza(p1.Id, Ianuarie); Storneaza(factura.Id, Ianuarie);
    }
    void Concurenta() {
        SuportNota(); var p1 = Pif(Fisa(), 800, 800); var p2 = Pif(Fisa(), 800, 800);
        using var start = new Barrier(2);
        string Executa(Guid id) { start.SignalAndWait(); try { Opereaza(id); return "OK"; }
            catch (OperareException e) { return e.Message; } }
        var rezultate = Task.WhenAll(Task.Run(() => Executa(p1.Id)), Task.Run(() => Executa(p2.Id))).GetAwaiter().GetResult();
        Verifica("SC-IMO-05", "două sesiuni: 800 acceptat și 800 refuzat din suportul 1200", rezultate.Count(r => r == "OK") == 1
            && rezultate.Count(r => r.Contains(CoduriRefuz.SuportInsuficient)) == 1);
        var rest = CuSpatiu(os => os.GetObjectsQuery<C.Postare>().Where(p => p.Cont == contActiv
            && p.Carte == N.Carte.Contabil && p.Unitate == null).Sum(p => p.Latura == N.Latura.Debit ? p.Valoare : -p.Valoare));
        Verifica("SC-IMO-05", "disponibilul rămas este 400", rest == 400);
    }
    void Ciclu() {
        SuportNota(); var f = Fisa(); var p = Pif(f); Opereaza(p.Id);
        var amo = Amo(2); Opereaza(amo); Sold("SC-IMO-07", f, 1200, 100, 900, 50);
        Anuleaza(amo); Sold("SC-IMO-14", f, 1200, 0, 900, 0); Opereaza(amo);
        Refuza("SC-IMO-17", () => Storneaza(p.Id, Ianuarie), "ulterioare");
        Refuza("SC-IMO-16", () => Storneaza(amo, Zi(3)), "luna lui");
        var cas = Cas(f, 3); Opereaza(cas); Sold("SC-IMO-12", f, 0, 0, 0, 0);
        Refuza("SC-IMO-17", () => Storneaza(amo, Sfarsit(2)), "ulterioare");
        Anuleaza(cas); Sold("SC-IMO-14", f, 1200, 100, 900, 50); Opereaza(cas);
        Storneaza(cas, Zi(3)); Origini("SC-IMO-15", cas); Sold("SC-IMO-15", f, 1200, 100, 900, 50);
        Storneaza(amo, Sfarsit(2)); Origini("SC-IMO-15", amo); Sold("SC-IMO-15", f, 1200, 0, 900, 0);
        Verifica("SC-CIT-02", "AMO contabil: două originale 100 și două inverse −100", CuSpatiu(os => {
            var randuri = C.Citiri.Contabil.Postari(os).Where(r => r.DocumentId == amo).ToList();
            return randuri.Count == 4 && randuri.Count(r => r.Valoare == 100) == 2
                && randuri.Count(r => r.Valoare == -100) == 2;
        }));
        Storneaza(p.Id, Ianuarie); Sold("SC-IMO-15", f, 0, 0, 0, 0);
    }
    void Initial() {
        var n = SuportNota(cumulat: 200); var f = Fisa(); var p = Pif(f, initial: 200, initialFiscal: 150);
        Opereaza(p.Id); Sold("SC-IMO-04", f, 1200, 200, 900, 150);
        Refuza("SC-IMO-06", () => Storneaza(n.Id, Ianuarie), CoduriRefuz.SuportCuDependenti);
        Storneaza(p.Id, Ianuarie); Origini("SC-IMO-15", p.Id); Storneaza(n.Id, Ianuarie);
    }
    void NumaiFiscal() {
        SuportNota(1200, 1200); var f = Fisa(); var p = Pif(f, fiscal: 100, durataFiscala: 2, initial: 1200);
        Opereaza(p.Id); var a = Amo(2); Opereaza(a); Sold("SC-IMO-08", f, 1200, 1200, 100, 50);
        var postari = CuSpatiu(os => os.GetObjectsQuery<C.Postare>().Where(p => p.DocumentId == a).ToList());
        Verifica("SC-IMO-08", "exact două postări, numai fiscale, de 50", postari.Count == 2
            && postari.All(p => p.Carte == N.Carte.Fiscal && p.Valoare == 50));
    }
    void Liniara() {
        SuportNota(100); var f = Fisa(); var p = Pif(f, 100, 100, 3, 3); Opereaza(p.Id);
        foreach (var (luna, suma) in new[] { (2, 33.33m), (3, 33.33m), (4, 33.33m), (5, 0.01m) }) {
            var a = Amo(luna); Opereaza(a);
            Verifica("SC-IMO-09", $"luna {luna}: {suma}", CuSpatiu(os => os.GetObjectsQuery<C.Postare>()
                .Where(p => p.DocumentId == a && p.Carte == N.Carte.Contabil && p.Latura == N.Latura.Credit)
                .Sum(p => p.Valoare)) == suma);
        }
        Sold("SC-IMO-09", f, 100, 100, 100, 100);
        var c = Cas(f, 6); Opereaza(c); Sold("SC-IMO-13", f, 0, 0, 0, 0);
    }
    void Intarziat() {
        SuportNota(); var f = Fisa(); var p = Pif(f, fiscal: 1200, durataFiscala: 12);
        Comanda(os => { os.GetObjectByKey<Document>(p.Id).DataInregistrare = Zi(3); os.CommitChanges(); });
        Opereaza(p.Id); var a = Amo(3); Opereaza(a); Sold("SC-IMO-18", f, 1200, 200, 1200, 200);
        Verifica("SC-IMO-18", "recuperarea poartă două luni", CuSpatiu(os =>
            os.GetObjectsQuery<AmortizareLunaraDetaliu>().Single(l => l.DocumentId == a).Luni) == 2);
        var aprilie = Amo(4); Opereaza(aprilie); Sold("SC-IMO-18", f, 1200, 300, 1200, 300);
    }
    void ConcurentaInversa() {
        var n = SuportNota(); var p = Pif(Fisa(), 800, 800);
        using var start = new Barrier(2);
        string Executa(Action actiune) { start.SignalAndWait(); try { actiune(); return "OK"; }
            catch (OperareException e) { return e.Message; } }
        var rezultate = Task.WhenAll(Task.Run(() => Executa(() => Opereaza(p.Id))),
            Task.Run(() => Executa(() => Anuleaza(n.Id)))).GetAwaiter().GetResult();
        Verifica("SC-IMO-05/06", "nominalizare concurentă cu anularea suportului: exact una reușește",
            rezultate.Count(r => r == "OK") == 1
            && rezultate.Any(r => r.Contains(CoduriRefuz.SuportInsuficient) || r.Contains(CoduriRefuz.SuportCuDependenti)));
        Verifica("SC-IMO-05/06", "nu există fișă nominalizată cu sursă anulată", CuSpatiu(os =>
            os.GetObjectByKey<Document>(p.Id).Stare == StareDocument.Draft
            || os.GetObjectByKey<Document>(n.Id).Stare == StareDocument.Operat));
    }
    void Modernizare() {
        SuportNota(); var f = Fisa(); var p = Pif(f, fiscal: 1200, durataFiscala: 12); Opereaza(p.Id);
        Opereaza(Amo(2)); SuportNota(120, luna: 3);
        var m = Pif(f, 120, 120, durataFiscala: 12, luna: 3, fel: FelLiniePif.Modernizare); Opereaza(m.Id);
        Opereaza(Amo(3)); Sold("SC-IMO-10", f, 1320, 200, 1320, 200);
        Opereaza(Amo(4)); Sold("SC-IMO-10", f, 1320, 312, 1320, 312);
    }
    void Revizuire() {
        SuportNota(); var f = Fisa(); var p = Pif(f, fiscal: 1200, durataFiscala: 12); Opereaza(p.Id);
        Opereaza(Amo(2));
        var rev = Pif(f, 0, 0, 22, 22, luna: 3, fel: FelLiniePif.Revizuire); Opereaza(rev.Id);
        Verifica("SC-IMO-11", "revizuirea păstrează postările zero cu fișă și cauză", CuSpatiu(os => {
            var randuri = os.GetObjectsQuery<C.Postare>().Where(p => p.DocumentId == rev.Id).ToList();
            return randuri.Count == 2 && randuri.All(p => p.Valoare == 0 && p.Unitate == f && p.LinieId == rev.Linii[0].Id);
        }));
        var martie = Amo(3); Opereaza(martie); var aprilie = Amo(4); Opereaza(aprilie);
        Sold("SC-IMO-11", f, 1200, 250, 1200, 250);
        Anuleaza(aprilie); Anuleaza(martie); Storneaza(rev.Id, Zi(3)); Origini("SC-IMO-15", rev.Id);
        var s = CuSpatiu(os => AmortizareService.Situatie(os, f, Sfarsit(3)));
        Verifica("SC-IMO-11", "inversa evenimentului zero restabilește durata 12", s.DurataLuni == 12 && s.DurataFiscalaLuni == 12);
    }
    void Deductibil() {
        SuportNota(48000); var f = Fisa(); var p = Pif(f, 48000, 48000, 12, 12);
        Comanda(os => {
            os.GetObjectByKey<Document>(p.Id).DataInregistrare = Zi(3);
            var l = os.GetObjectByKey<PunereInFunctiuneDetaliu>(p.Linii[0].Id);
            l.CategorieFiscala = CategorieFiscala.VehiculPersoaneMax9Locuri; l.UtilizareExclusiva = false;
            os.CommitChanges();
        });
        Opereaza(p.Id); var a = Amo(3); Opereaza(a); Sold("SC-IMO-19", f, 48000, 8000, 48000, 8000);
        var s = CuSpatiu(os => AmortizareService.Situatie(os, f, Sfarsit(3)));
        Verifica("SC-IMO-19", "plafon o singură dată în luna recuperării", s.AmortizareDeductibila == (Privat ? 1500 : 8000));
    }
    void Locuri() {
        SuportNota(); var f = Fisa(); var p = Pif(f, fiscal: 1200, durataFiscala: 12); Opereaza(p.Id);
        var a = Amo(2); Opereaza(a); var amprenta = Amprenta(a);
        Comanda(os => { os.GetObjectByKey<Imobilizare>(f).LocId = Destinatie; os.CommitChanges(); });
        var b = Amo(3); Opereaza(b);
        Verifica("SC-IMO-20", "locul vechi rămâne înghețat, luna nouă folosește locul nou", Amprenta(a) == amprenta
            && CuSpatiu(os => os.GetObjectsQuery<C.Postare>().Where(p => p.DocumentId == b)
                .All(p => p.Gestiune == Destinatie)));
        Sold("SC-IMO-20", f, 1200, 200, 1200, 200);
    }
    void Perf() {
        SuportNota(10000);
        int Citiri(int cate) {
            var p = Pif(Fisa(), 1, 1);
            for (var i = 1; i < cate; i++) {
                var f = Fisa();
                Comanda(os => {
                    var l = os.CreateObject<PunereInFunctiuneDetaliu>();
                    l.Document = os.GetObjectByKey<PunereInFunctiune>(p.Id); l.Pozitie = i + 1;
                    l.ImobilizareId = f; l.TipMaterialId = tip; l.Cantitate = 1; l.Valoare = 1;
                    l.ValoareFiscala = 1; l.Metoda = MetodaAmortizare.Liniara; l.MetodaFiscala = MetodaAmortizare.Liniara;
                    l.DurataLuni = 12; l.DurataFiscalaLuni = 18;
                    l.CategorieFiscala = CategorieFiscala.Standard; l.UtilizareExclusiva = true;
                    os.CommitChanges();
                });
            }
            using var os = Deschide(); var doc = os.GetObjectByKey<Document>(p.Id); doc.PregatesteOperare(os);
            NumaratorSql.Instanta.Reseteaza(); var contract = Contractare.Contracteaza(os, doc);
            var n = NumaratorSql.Instanta.Numar;
            Verifica("SC-IMO-22", $"contract cu {cate} fișe acceptat: {string.Join(';', contract.Refuzuri.Select(r => r.Mesaj))}",
                contract.EsteAcceptat && doc.Detalii.Count == cate); return n;
        }
        var mic = Citiri(2); var mare = Citiri(51);
        Verifica("SC-IMO-22", $"2/51 fișe: {mic}/{mare} interogări", mic > 0 && mic == mare && mare <= 20);
    }
    void Grafic(MetodaAmortizare metoda, string id) {
        SuportNota(); var f = Fisa(); var p = Pif(f, 1200, 1200, 36, 36);
        Comanda(os => { var l = os.GetObjectByKey<PunereInFunctiuneDetaliu>(p.Linii[0].Id);
            l.Metoda = metoda; l.MetodaFiscala = metoda; os.CommitChanges(); });
        Opereaza(p.Id);
        for (var i = 1; i <= 36; i++) {
            var data = Ianuarie.AddMonths(i); var a = Amo(data); Opereaza(a);
            var suma = i <= 12 ? 50m : 25m;
            var postari = CuSpatiu(os => os.GetObjectsQuery<C.Postare>().Where(p => p.DocumentId == a).ToList());
            Verifica(id, $"luna {i}: {suma} în fiecare carte", postari.Count == 4 && postari.All(p => p.Valoare == suma));
        }
        Sold(id, f, 1200, 1200, 1200, 1200, new(An + 3, 1, 31));
    }
    void Reziduala() {
        SuportNota(1200, 200); var f = Fisa(); var p = Pif(f, 1200, 1200, 12, 12, initial: 200, initialFiscal: 200);
        Comanda(os => { var l = os.GetObjectByKey<PunereInFunctiuneDetaliu>(p.Linii[0].Id);
            l.LuniAmortizateInitial = 2; l.ValoareReziduala = 100; os.CommitChanges(); });
        Opereaza(p.Id);
        for (var luna = 2; luna <= 11; luna++) {
            var a = Amo(luna); Opereaza(a);
            Verifica("SC-IMO-25", "cotă contabilă 90 și fiscală 100", CuSpatiu(os => os.GetObjectsQuery<C.Postare>()
                .Where(p => p.DocumentId == a).All(p => p.Valoare == (p.Carte == N.Carte.Contabil ? 90 : 100))));
        }
        Sold("SC-IMO-25", f, 1200, 1100, 1200, 1200);
    }
    void Inchis() {
        SuportNota(); var f = Fisa(); var p = Pif(f); Opereaza(p.Id);
        InchideIanuarie(); var amprenta = Amprenta(p.Id);
        Refuza("SC-IMO-16", () => Corecteaza(p.Id), "luna lui");
        Verifica("SC-IMO-16", "corecția refuzată păstrează originalele", amprenta == Amprenta(p.Id));
    }
    protected override void CurataCubSuplimentar(IObjectSpace os, Purja pj) {
        if (deschidere is not Guid id) return;
        pj.AdaugaCheie<C.Postare>(os.GetObjectsQuery<C.Postare>().Where(p => p.TranzactieId == id).Select(p => p.ID).ToList());
        pj.AdaugaCheie<C.Tranzactie>([id]);
    }
    void Deschidere() {
        Comanda(os => {
            using var tx = TranzactieComanda.Incepe(os);
            deschidere = C.Materializare.Deschide(os, Ianuarie,
                [new(contActiv, N.Latura.Debit, 1200), new(contAmortizare, N.Latura.Credit, 200),
                    new(Cont(Capital), N.Latura.Credit, 1000)], [], []);
            os.CommitChanges(); tx.Commit();
        });
        var f = Fisa(); var p = Pif(f, initial: 200, initialFiscal: 150); Opereaza(p.Id);
        Sold("SC-IMO-04", f, 1200, 200, 900, 150);
        Verifica("SC-CIT-06", "citirea contabilă păstrează numai cele trei postări de deschidere", CuSpatiu(os =>
            C.Citiri.Contabil.Postari(os).Count(r => r.TranzactieId == deschidere || r.DocumentId == p.Id) == 3));
        Verifica("SC-IMO-04", "deschiderea nominalizată păstrează soldurile generale 1200/200", CuSpatiu(os => {
            var randuri = os.GetObjectsQuery<C.Postare>().Where(p => p.Carte == N.Carte.Contabil
                && (p.Cont == contActiv || p.Cont == contAmortizare)).ToList();
            return randuri.Where(p => p.Cont == contActiv).Sum(p => p.Latura == N.Latura.Debit ? p.Valoare : -p.Valoare) == 1200
                && randuri.Where(p => p.Cont == contAmortizare).Sum(p => p.Latura == N.Latura.Credit ? p.Valoare : -p.Valoare) == 200;
        }));
        Storneaza(p.Id, Ianuarie); Origini("SC-IMO-15", p.Id);
    }
    void CasZero() {
        SuportNota(2400); var f = Fisa(); var p = Pif(f); Opereaza(p.Id);
        Opereaza(Pif(Fisa()).Id);
        var c = Cas(f, 2); Opereaza(c); Sold("SC-IMO-13", f, 0, 0, 0, 0);
        Verifica("SC-IMO-13", "CAS fără amortizare: exact două perechi, 1200 contabil și 900 fiscal", CuSpatiu(os => {
            var randuri = os.GetObjectsQuery<C.Postare>().Where(p => p.DocumentId == c).ToList();
            return randuri.Count == 4 && randuri.All(p => p.Valoare == (p.Carte == N.Carte.Contabil ? 1200 : 900));
        }));
        Opereaza(Amo(2));
        Refuza("SC-IMO-17", () => Storneaza(c, Zi(2)), "amortizarea");
    }
    void FaraPolitica() {
        SuportNota();
        tip = CuSpatiu(os => {
            var sursa = os.GetObjectByKey<TipMaterial>(tip);
            var t = os.CreateObject<TipMaterial>(); t.Cod = Marcaj + "-NP"; t.Denumire = t.Cod;
            t.ClasaId = sursa.ClasaId; t.ContImplicitId = contActiv; os.CommitChanges(); return t.ID;
        });
        var p = Pif(Fisa()); Opereaza(p.Id);
        Verifica("SC-IMO-21", "generatorul refuză nominal fișa fără politică", CuSpatiu(os => {
            var rezultat = AmortizareService.Incearca(os, An, 2, Loc);
            return rezultat.Document == null && rezultat.Motiv == MotivNegenerare.FisaFaraPolitica;
        }));
        Verifica("SC-IMO-21", "lipsa politicii nu creează AMO parțială", CuSpatiu(os =>
            !os.GetObjectsQuery<AmortizareLunara>().Any(a => a.Data.Year == An)));
    }
    void Refuzuri() {
        SuportNota(); var f = Fisa(); var p = Pif(f, durata: -1);
        Refuza("SC-IMO-21", () => Opereaza(p.Id), "durat"); FaraEfecte("SC-IMO-21", p.Id);
        Comanda(os => { os.GetObjectByKey<PunereInFunctiuneDetaliu>(p.Linii[0].Id).DurataLuni = 12; os.CommitChanges(); });
        Opereaza(p.Id); var a = Amo(2);
        Comanda(os => { os.GetObjectsQuery<AmortizareLunaraDetaliu>().Single(l => l.DocumentId == a).Valoare += 1; os.CommitChanges(); });
        Refuza("SC-IMO-21", () => Opereaza(a), "corespund"); FaraEfecte("SC-IMO-21", a);
        Comanda(os => { os.GetObjectsQuery<AmortizareLunaraDetaliu>().Single(l => l.DocumentId == a).Valoare -= 1; os.CommitChanges(); });
        Opereaza(a); var c = Cas(f, 2);
        Refuza("SC-IMO-21", () => Opereaza(c), "amortizare"); FaraEfecte("SC-IMO-21", c);
        var factura = Factura(Ianuarie, new LinieFctScena(1, 1200, Stoc: false, Tip: Activ));
        var q = Pif(Fisa(), sursa: factura.Linii[0].Id);
        Refuza("SC-IMO-21", () => Opereaza(q.Id), "operat"); FaraEfecte("SC-IMO-21", q.Id);
    }
}
