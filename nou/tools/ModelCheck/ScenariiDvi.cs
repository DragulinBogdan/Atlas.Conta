using Atlas.Conta.BackOffice.Module.Api;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Declaratii;
using Atlas.Conta.BackOffice.Module.Proiectii;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.EFCore;
using C = Atlas.Conta.BackOffice.Module.Cub;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.ModelCheck;

record LinieDviScena(decimal Baza, string Tva = "IMP21", decimal Taxa = 0);

sealed class ScenariiDvi(Func<IObjectSpace> deschide, Action<string, bool> check,
    bool privat, Action<IObjectSpace, int, int> inchide)
    : ScenaDocumente(deschide, check, privat, inchide, "DVI", 2010) {
    Guid vama;
    int numar;

    FacturaScena Declaratie(params LinieDviScena[] linii) {
        using var os = Deschide();
        var d = os.CreateObject<Dvi>(); d.Numar = Marcaj + "-MRN-" + ++numar;
        d.Data = Ianuarie; d.PredatorId = vama; d.PrimitorId = Loc;
        var rezultat = new List<LinieScena>();
        foreach (var spec in linii) {
            var l = os.CreateObject<DocumentDetaliu>(); l.Document = d; l.Pozitie = rezultat.Count + 1;
            l.TipMaterialId = Tip(os, Stoc); l.Valoare = spec.Baza; l.ValoareTva = spec.Taxa;
            if (spec.Tva != null) l.TipTvaId = Tva(spec.Tva);
            rezultat.Add(new(l.ID, null, null));
        }
        os.CommitChanges(); return new(d.ID, rezultat.ToArray());
    }

    RandScena[] Randuri(FacturaScena d, int i, decimal baza, decimal taxa, string tva = "IMP21",
            int? perioada = null, string contra = "446", Guid? partener = null) {
        var tert = partener ?? vama; var linie = d.Linii[i].Id;
        var debit = new RandScena(Cont("4426"), N.Latura.Debit, baza, Gestiune: Loc, Partener: tert,
            Linie: linie, Tva: Tva(tva), Rol: N.RolTva.Baza, Perioada: perioada ?? An * 100 + 1,
            Sens: N.SensTva.Achizitie, Carte: N.Carte.Fiscal);
        var credit = debit with { Latura = N.Latura.Credit, Partener = null, Tva = null, Rol = null,
            Sens = null, Perioada = null };
        return taxa == 0 ? [debit, credit] : [
            debit with { Valoare = taxa, Rol = N.RolTva.Taxa, Carte = N.Carte.Contabil },
            new(Cont(contra), N.Latura.Credit, taxa, Linie: linie,
                Partener: contra == "401" ? tert : null,
                Unitate: contra == "401" ? Partida(d.Id, contra, tert) : null), debit, credit];
    }

    protected override void Executa() {
        vama = CuSpatiu(os => {
            var p = os.CreateObject<Partener>(); p.Cod = Marcaj + "-VAMA"; p.Denumire = p.Cod;
            if (Privat) p.ContImplicitId = Cont("446");
            os.CommitChanges(); return p.ID;
        });
        if (!Privat) {
            Verifica("SC-DVI-12", "fără politică fiscală și fără activare cub", CuSpatiu(os =>
                !os.GetObjectsQuery<PoliticaTva>().Any(p => p.TipDocument.Cod == "DVI")
                && !os.GetObjectsQuery<TipDocument>().Single(t => t.Cod == "DVI").PosteazaInCub));
            var d = Declaratie(new LinieDviScena(100, null));
            Refuza("SC-DVI-12", () => Opereaza(d.Id), "TVA de import"); FaraEfecte("SC-DVI-12", d.Id);
            return;
        }
        Simple(); Variante(); Refuzuri(); FacturiLegate(); Dependenti(); TransferPur(); PestePerioada();
    }

    void Simple() {
        var d = Declaratie(new LinieDviScena(100));
        Verifica("SC-DVI-01", "dry-run acceptat", CuSpatiu(os => OperareApi.Valideaza(os, d.Id)).Count == 0);
        FaraEfecte("SC-DVI-01", d.Id); Opereaza(d.Id);
        Postari("SC-DVI-01", d.Id, N.FelTranzactie.Operare, Ianuarie, Randuri(d, 0, 100, 21));
        Verifica("SC-CIT-03", "intrarea contabilă include numai perechea TVA 21", CuSpatiu(os => {
            var randuri = C.Citiri.Contabil.Postari(os).Where(p => p.DocumentId == d.Id).ToList();
            return randuri.Count == 2 && randuri.All(p => p.Valoare == 21 && p.Carte == N.Carte.Contabil)
                && randuri.Single(p => p.Cont == Cont("4426")).Latura == N.Latura.Debit;
        }));
        Jurnal("SC-DVI-01", [d.Id], An * 100 + 1, 100, 21);
        Citiri(d); Normalizare(d, Randuri(d, 0, 100, 21));
        Anuleaza(d.Id); FaraEfecte("SC-DVI-03", d.Id); Opereaza(d.Id);
        Postari("SC-DVI-03", d.Id, N.FelTranzactie.Operare, Ianuarie, Randuri(d, 0, 100, 21));
        Storneaza(d.Id, new(An, 1, 20));
        Postari("SC-DVI-04", d.Id, N.FelTranzactie.Storno, new(An, 1, 20), Randuri(d, 0, -100, -21));
        Jurnal("SC-DVI-04", [d.Id], An * 100 + 1, 0, 0);
        var intact = Amprenta(d.Id); Refuza("SC-DVI-04", () => Storneaza(d.Id, new(An, 1, 20)), "Operat");
        Verifica("SC-DVI-04", "refuz repetat atomic", Amprenta(d.Id) == intact);
        var m = Declaratie(new LinieDviScena(100), new LinieDviScena(50, "IMP11")); Opereaza(m.Id);
        Postari("SC-DVI-02", m.Id, N.FelTranzactie.Operare, Ianuarie,
            [.. Randuri(m, 0, 100, 21), .. Randuri(m, 1, 50, 5.50m, "IMP11")]);
        Jurnal("SC-DVI-02", [m.Id], An * 100 + 1, 150, 26.50m);
    }

    void Variante() {
        var broker = Declaratie(new LinieDviScena(100));
        Comanda(os => { os.GetObjectByKey<Dvi>(broker.Id).PredatorId = Furnizor;
            os.GetObjectByKey<Partener>(Furnizor).ContImplicitId = Cont("401"); os.CommitChanges(); });
        Opereaza(broker.Id);
        Postari("SC-DVI-08", broker.Id, N.FelTranzactie.Operare, Ianuarie,
            Randuri(broker, 0, 100, 21, contra: "401", partener: Furnizor));
        SoldPartida("SC-DVI-08", Partida(broker.Id, "401")!.Value, Ianuarie, -21);
        Storneaza(broker.Id, Ianuarie); SoldPartida("SC-DVI-08", Partida(broker.Id, "401")!.Value, Ianuarie, 0);
        Postari("SC-DVI-08", broker.Id, N.FelTranzactie.Storno, Ianuarie,
            Randuri(broker, 0, -100, -21, contra: "401", partener: Furnizor));
        var ti = Declaratie(new LinieDviScena(100, "IMPTI21")); Opereaza(ti.Id);
        var rt = Randuri(ti, 0, 100, 21, "IMPTI21", contra: "4427");
        Postari("SC-DVI-09", ti.Id, N.FelTranzactie.Operare, Ianuarie, rt); Normalizare(ti, rt);
        var cules = Declaratie(new LinieDviScena(100, Taxa: 21.03m)); Opereaza(cules.Id);
        var rc = Randuri(cules, 0, 100, 21.03m);
        Postari("SC-DVI-13", cules.Id, N.FelTranzactie.Operare, Ianuarie, rc); Normalizare(cules, rc);
        var a = Declaratie(new LinieDviScena(100)); var b = Declaratie(new LinieDviScena(100.01m));
        Opereaza(a.Id); Opereaza(b.Id);
        Postari("SC-DVI-14", a.Id, N.FelTranzactie.Operare, Ianuarie, Randuri(a, 0, 100, 21));
        Postari("SC-DVI-14", b.Id, N.FelTranzactie.Operare, Ianuarie, Randuri(b, 0, 100.01m, 21));
        Jurnal("SC-DVI-14", [a.Id, b.Id], An * 100 + 1, 200.01m, 42);
        var mic = Declaratie(new LinieDviScena(.01m)); Opereaza(mic.Id);
        var rm = Randuri(mic, 0, .01m, 0);
        Postari("SC-DVI-15", mic.Id, N.FelTranzactie.Operare, Ianuarie, rm); Normalizare(mic, rm);
        Storneaza(mic.Id, Ianuarie);
        Postari("SC-DVI-15", mic.Id, N.FelTranzactie.Storno, Ianuarie, Randuri(mic, 0, -.01m, 0));
        Jurnal("SC-DVI-15", [mic.Id], An * 100 + 1, 0, 0);
    }

    void Citiri(FacturaScena d) {
        using var os = Deschide();
        var jurnalContabil = ContabilProiectii.RegistruJurnal(os).Where(r => r.DocumentId == d.Id).ToList();
        var fisaContabila = ContabilProiectii.FisaCont(os, Cont("4426"), Ianuarie, Ianuarie)
            .Where(r => r.DocumentId == d.Id).ToList();
        var balanta = ContabilProiectii.Balanta(os, Ianuarie, Ianuarie, repartitorId: vama)
            .Single(r => r.ContId == Cont("4426"));
        Verifica("SC-CIT-15", "DVI: jurnal 21/21, fișă și balanță 21, fără baza fiscală 100",
            jurnalContabil.Count == 2 && jurnalContabil.Sum(r => r.Debit) == 21
            && jurnalContabil.Sum(r => r.Credit) == 21 && fisaContabila.Count == 1
            && fisaContabila[0].Debit == 21 && balanta.RulajDebit == 21 && balanta.RulajCredit == 0);
        var p = os.GetObjectsQuery<C.Postare>().Where(p => p.DocumentId == d.Id).ToList();
        decimal Sold(IEnumerable<C.Postare> r) => r.Sum(p => p.Latura == N.Latura.Debit ? p.Valoare : -p.Valoare);
        var contabil = p.Where(p => p.Carte == N.Carte.Contabil).ToList();
        Verifica("SC-DVI-16", "rulaje contabile 21/21; fiscale 100/100, fără unități",
            contabil.Where(p => p.Latura == N.Latura.Debit).Sum(p => p.Valoare) == 21
            && contabil.Where(p => p.Latura == N.Latura.Credit).Sum(p => p.Valoare) == 21
            && p.Where(p => p.Carte == N.Carte.Fiscal).All(p => p.Unitate == null && p.Valoare == 100)
            && p.GroupBy(p => p.Carte).All(g => Sold(g) == 0));
        Verifica("SC-DVI-16", "sold 4426 contabil, inclusiv pe partener: 21; fiscal pe cont: 0",
            Sold(contabil.Where(p => p.Cont == Cont("4426"))) == 21
            && Sold(contabil.Where(p => p.Cont == Cont("4426") && p.Partener == vama)) == 21
            && Sold(p.Where(p => p.Carte == N.Carte.Fiscal && p.Cont == Cont("4426"))) == 0);
        Verifica("SC-DVI-16", "martor fără Carte: rulaje 121/121 și sold cont × partener 121",
            p.Where(p => p.Latura == N.Latura.Debit).Sum(p => p.Valoare) == 121
            && p.Where(p => p.Latura == N.Latura.Credit).Sum(p => p.Valoare) == 121
            && Sold(p.Where(p => p.Cont == Cont("4426") && p.Partener == vama)) == 121);
        var delta = ReconciliereCub.Ruleaza(((EFCoreObjectSpace)os).DbContext, [d.Id]);
        Verifica("SC-DVI-16", "reconciliere contabilă (a) fără diferențe", !delta.Any(r => r.Litera.StartsWith("(a)")));
        Jurnal("SC-DVI-16", [d.Id], An * 100 + 1, 100, 21);
    }

    void Jurnal(string id, Guid[] docs, int perioada, decimal baza, decimal taxa) {
        var p = CuSpatiu(os => os.GetObjectsQuery<C.Postare>().Where(p => p.DocumentId != null
            && docs.Contains(p.DocumentId.Value) && p.TipTvaId != null && p.PerioadaDeclarare == perioada).ToList());
        Verifica(id, $"jurnal {perioada}: {baza}/{taxa}", p.Where(p => p.RolTva == N.RolTva.Baza).Sum(p => p.Valoare) == baza
            && p.Where(p => p.RolTva == N.RolTva.Taxa).Sum(p => p.Valoare) == taxa
            && p.All(p => p.SensTva == N.SensTva.Achizitie));
    }

    void Normalizare(FacturaScena d, RandScena[] asteptate) {
        using var os = Deschide(); Normalizari.Reseteaza();
        var t = Normalizari.Toate(CubDinRegistre.Transforma(os, [d.Id]), Normalizari.Citeste(os, [d.Id]));
        var p = t.SelectMany(t => t.Postari).ToList();
        var actual = p.Select(p => new RandScena(p.Coordonate.Cont, p.Coordonate.Latura, p.Valoare, p.Cantitate,
            p.Coordonate.Gestiune, p.Coordonate.Unitate?.Id, p.Coordonate.Produs, p.Coordonate.Partener,
            p.Cauza.Linie, p.Coordonate.CodTva?.TipTva, p.Coordonate.CodTva?.Rol, p.Coordonate.PerioadaDeclarare,
            p.Coordonate.CodTva?.Sens, N.Postari.Spatiu(p), p.Coordonate.Analiza.CodEconomic, p.Coordonate.Carte)).ToList();
        var ok = t.Count == 1 && t[0].Data == Ianuarie && actual.Count == asteptate.Length
            && asteptate.All(a => actual.Count(r => r == a) == asteptate.Count(r => r == a))
            && p.All(p => p.Coordonate.Data == Ianuarie && p.Cauza.Document == d.Id && p.Atribuit == null
                && p.Coordonate.Valuta == null && p.ValoareValuta == 0)
            && Normalizari.Avertismente.Count == 0
            && Normalizari.Contoare.GetValueOrDefault("DVI-B2: pereche de bază în cartea fiscală") == 1;
        Verifica("SC-DVI-19", $"adaptor: {asteptate.Length} postări exacte, contor 1, fără diagnostic", ok);
        if (!ok) foreach (var a in actual) Console.WriteLine("     NORMALIZAT " + a);
    }

    void Refuzuri() {
        foreach (var (d, cod, mesaj) in new[] {
            (Declaratie(), CoduriRefuz.LiniiLipsa, "linie"),
            (Declaratie(new LinieDviScena(0)), CoduriRefuz.ValoareNepozitiva, "pozitiv"),
            (Declaratie(new LinieDviScena(-1)), CoduriRefuz.ValoareNepozitiva, "pozitiv"),
            (Declaratie(new LinieDviScena(100, "N21")), CoduriRefuz.TvaImportNepotrivit, "nu e de import"),
            (Declaratie(new LinieDviScena(100, "IMP")), CoduriRefuz.TvaImportNepotrivit, "cotă"),
        }) {
            RefuzDeclaratie("SC-DVI-10", d.Id, cod);
            Refuza("SC-DVI-10", () => Opereaza(d.Id), mesaj); FaraEfecte("SC-DVI-10", d.Id);
        }
        var mrn = Declaratie(new LinieDviScena(100));
        Comanda(os => { os.GetObjectByKey<Dvi>(mrn.Id).Numar = null; os.CommitChanges(); });
        Refuza("SC-DVI-10", () => Opereaza(mrn.Id), "MRN"); FaraEfecte("SC-DVI-10", mrn.Id);
    }

    void Leaga(Guid dvi, params Guid[] facturi) => Comanda(os => {
        foreach (var id in facturi) {
            var leg = os.CreateObject<DviFactura>(); leg.DviId = dvi; leg.FacturaId = id;
            var erori = new List<string>(); leg.Verifica(os, erori);
            if (erori.Count > 0) throw new InvalidOperationException(string.Join("\n", erori));
        }
        os.CommitChanges();
    });

    void FacturiLegate() {
        var f1 = Receptioneaza(new LinieFctScena(1, 60, "IMP"));
        var f2 = Receptioneaza(new LinieFctScena(1, 40, "IMP"));
        var d = Declaratie(new LinieDviScena(100)); Leaga(d.Id, f1.Id, f2.Id);
        var inainte = new[] { Amprenta(f1.Id), Amprenta(f2.Id) };
        Opereaza(d.Id); Postari("SC-DVI-11", d.Id, N.FelTranzactie.Operare, Ianuarie, Randuri(d, 0, 100, 21));
        Verifica("SC-DVI-11", "cele două facturi își păstrează postările", inainte.SequenceEqual(new[] { Amprenta(f1.Id), Amprenta(f2.Id) }));
        SoldPartida("SC-DVI-11", Partida(f1.Id, "401")!.Value, Ianuarie, -60);
        SoldPartida("SC-DVI-11", Partida(f2.Id, "401")!.Value, Ianuarie, -40);
        SoldLot("SC-DVI-11", f1.Linii[0].Lot!.Value, Magazie, Ianuarie, 1, 60);
        SoldLot("SC-DVI-11", f2.Linii[0].Lot!.Value, Magazie, Ianuarie, 1, 40);
        var f = Factura(Ianuarie, new LinieFctScena(1, 10, "IMP", false)); Opereaza(f.Id);
        var refuz = Declaratie(new LinieDviScena(100)); Leaga(refuz.Id, f.Id); Anuleaza(f.Id);
        Refuza("SC-DVI-10", () => Opereaza(refuz.Id), "nu Operat"); FaraEfecte("SC-DVI-10", refuz.Id);
    }

    void Dependenti() {
        var broker = CuSpatiu(os => { var p = os.CreateObject<Partener>(); p.Cod = Marcaj + "-BROKER";
            p.Denumire = p.Cod; p.ContImplicitId = Cont("401"); os.CommitChanges(); return p.ID; });
        var d = Declaratie(new LinieDviScena(100));
        Comanda(os => { os.GetObjectByKey<Dvi>(d.Id).PredatorId = broker; os.CommitChanges(); });
        Opereaza(d.Id);
        var n = Nota(Ianuarie, new LinieNtcScena("401", "5121", 10, broker)); Opereaza(n.Id);
        var partida = Partida(d.Id, "401", broker)!.Value;
        SoldPartida("SC-DVI-17", partida, Ianuarie, -11);
        var intact = Amprenta(d.Id);
        Refuza("SC-DVI-17", () => Anuleaza(d.Id), CoduriRefuz.PartidaCuDependenti);
        Refuza("SC-DVI-17", () => Storneaza(d.Id, Ianuarie), CoduriRefuz.PartidaCuDependenti);
        Verifica("SC-DVI-17", "refuzuri atomice", Amprenta(d.Id) == intact);
        Storneaza(n.Id, Ianuarie); SoldPartida("SC-DVI-17", partida, Ianuarie, -21);
        Storneaza(d.Id, Ianuarie); SoldPartida("SC-DVI-17", partida, Ianuarie, 0);
        Postari("SC-DVI-17", d.Id, N.FelTranzactie.Operare, Ianuarie,
            Randuri(d, 0, 100, 21, contra: "401", partener: broker));
        Postari("SC-DVI-17", d.Id, N.FelTranzactie.Storno, Ianuarie,
            Randuri(d, 0, -100, -21, contra: "401", partener: broker));
        Jurnal("SC-DVI-17", [d.Id], An * 100 + 1, 0, 0);
    }

    void TransferPur() {
        var stins = Guid.NewGuid(); var stingator = Guid.NewGuid();
        N.Postare P(Guid doc, N.Latura latura, decimal v) => new(new N.Coordonate {
            Cont = Cont("401"), Latura = latura, Data = Ianuarie, Partener = Furnizor,
            Unitate = N.Unitate.DeschidePartida(Cont("401"), Furnizor, doc, Ianuarie),
        }, 0, 0, v, new(doc, null));
        var cerere = new C.Transferuri.Cerere(stingator, Ianuarie, [P(stingator, N.Latura.Debit, 10)], [],
            stins, Ianuarie, [P(stins, N.Latura.Credit, 21)], [], 10, Ianuarie);
        N.Postare[] Baza(Guid doc) => [.. new[] { N.Latura.Debit, N.Latura.Credit }.Select(l =>
            new N.Postare(new N.Coordonate { Cont = Cont("4426"), Latura = l, Data = Ianuarie,
                Carte = N.Carte.Fiscal, Gestiune = Loc, Partener = l == N.Latura.Debit ? Furnizor : null,
                CodTva = l == N.Latura.Debit ? new(Tva("IMP21"), N.SensTva.Achizitie, N.RolTva.Baza) : null,
                PerioadaDeclarare = l == N.Latura.Debit ? An * 100 + 1 : null }, 0, 0, 100, new(doc, null)))];
        var original = C.Transferuri.Muta(cerere);
        Verifica("SC-DVI-20", "mutare 10; baza fără unități nu afectează niciuna dintre intrări",
            original.Mutare?.Valoare == 10 && original.Refuz == null && original.Sarit == null
            && C.Transferuri.Muta(cerere with { OperareStins = [.. cerere.OperareStins, .. Baza(stins)] }) == original
            && C.Transferuri.Muta(cerere with { OperareStingator = [.. cerere.OperareStingator, .. Baza(stingator)] }) == original);
    }

    void PestePerioada() {
        var deschis = Declaratie(new LinieDviScena(100));
        Comanda(os => { os.GetObjectByKey<Dvi>(deschis.Id).DataInregistrare = Februarie; os.CommitChanges(); });
        Opereaza(deschis.Id);
        Postari("SC-DVI-18", deschis.Id, N.FelTranzactie.Operare, Februarie, Randuri(deschis, 0, 100, 21));
        Jurnal("SC-DVI-18", [deschis.Id], An * 100 + 1, 100, 21);
        var p = Declaratie(new LinieDviScena(100)); Opereaza(p.Id);
        var c = Declaratie(new LinieDviScena(100)); Opereaza(c.Id);
        var tarziu = Declaratie(new LinieDviScena(100));
        Comanda(os => { os.GetObjectByKey<Dvi>(tarziu.Id).DataInregistrare = Februarie; os.CommitChanges(); });
        InchideIanuarie(); var intact = Amprenta(p.Id);
        Refuza("SC-DVI-05", () => Anuleaza(p.Id), "închis");
        Verifica("SC-DVI-05", "anulare refuzată atomic", Amprenta(p.Id) == intact);
        Storneaza(p.Id, Februarie);
        Postari("SC-DVI-05", p.Id, N.FelTranzactie.Operare, Ianuarie, Randuri(p, 0, 100, 21));
        Postari("SC-DVI-05", p.Id, N.FelTranzactie.Storno, Februarie, Randuri(p, 0, -100, -21, perioada: An * 100 + 2));
        Jurnal("SC-DVI-05", [p.Id], An * 100 + 1, 100, 21); Jurnal("SC-DVI-05", [p.Id], An * 100 + 2, -100, -21);
        var nou = Corecteaza(c.Id); FaraEfecte("SC-DVI-06", nou);
        var corectie = CuSpatiu(os => {
            var d = os.GetObjectByKey<Dvi>(nou);
            Verifica("SC-DVI-06", "corecție legată, motiv și date", d.CorecteazaId == c.Id
                && d.MotivCorectie == MotivCorectie.EroareMateriala && d.Data == Ianuarie && d.DataInregistrare == Februarie);
            var l = os.GetObjectsQuery<DocumentDetaliu>().Single(l => l.DocumentId == nou);
            l.Valoare = 80; l.ValoareTva = 0; os.CommitChanges(); return new FacturaScena(nou, [new(l.ID, null, null)]);
        });
        Opereaza(nou);
        Postari("SC-DVI-06", c.Id, N.FelTranzactie.Storno, Februarie, Randuri(c, 0, -100, -21));
        Postari("SC-DVI-06", nou, N.FelTranzactie.Operare, Februarie, Randuri(corectie, 0, 80, 16.80m));
        Jurnal("SC-DVI-06", [c.Id, nou], An * 100 + 1, 80, 16.80m);
        Jurnal("SC-DVI-06", [c.Id, nou], An * 100 + 2, 0, 0);
        Opereaza(tarziu.Id);
        Postari("SC-DVI-07", tarziu.Id, N.FelTranzactie.Operare, Februarie, Randuri(tarziu, 0, 100, 21, perioada: An * 100 + 2));
        Jurnal("SC-DVI-07", [tarziu.Id], An * 100 + 1, 0, 0); Jurnal("SC-DVI-07", [tarziu.Id], An * 100 + 2, 100, 21);
        foreach (var (id, d) in new[] { ("SC-DVI-07", tarziu), ("SC-DVI-18", deschis) }) {
            var pcont = CuSpatiu(os => os.GetObjectsQuery<C.Postare>().Where(p => p.DocumentId == d.Id
                && p.Carte == N.Carte.Contabil && p.Latura == N.Latura.Debit).ToList());
            Verifica(id, "contabil: 0 în ianuarie, 21 în februarie", pcont.Where(p => p.Data <= new DateOnly(An, 1, 31)).Sum(p => p.Valoare) == 0
                && pcont.Where(p => p.Data == Februarie).Sum(p => p.Valoare) == 21);
        }
    }
}
