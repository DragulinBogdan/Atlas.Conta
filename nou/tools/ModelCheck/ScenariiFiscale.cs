using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Cub.Citiri;
using Atlas.Conta.BackOffice.Module.Motor;
using Atlas.Conta.BackOffice.Module.Proiectii;
using Atlas.Conta.BackOffice.Module.Saft;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.EFCore;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using C = Atlas.Conta.BackOffice.Module.Cub;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.ModelCheck;

sealed class ScenariiFiscale(Func<IObjectSpace> deschide, Action<string, bool> check,
    bool privat, Action<IObjectSpace, int, int> inchide)
    : ScenaDocumente(deschide, check, privat, inchide, "FISCAL", 2023) {
    protected override void CurataNomenclatoare(IObjectSpace os, Purja purja) {
        purja.Adauga(os.GetObjectsQuery<DepunereDeclaratie>().Where(d => d.Perioada >= An * 100 && d.Perioada < (An + 1) * 100));
        purja.Adauga(os.GetObjectsQuery<PoliticaTva>().Where(p => !p.DinSeed && p.TipDocument.Cod == "FCT"
            && os.GetObjectsQuery<TipTva>().Any(t => t.Cod == Marcaj)));
        purja.Adauga(os.GetObjectsQuery<TipTva>().Where(t => t.Cod == Marcaj));
    }

    protected override void Executa() {
        if (!Privat) {
            var inert = Factura(Ianuarie, new LinieFctScena(1, 100, "CAP21", Stoc: false));
            Opereaza(inert.Id);
            Verifica("SC-CIT-87", "bugetar fără politică: valoare contabilă, fără fapte fiscale inventate", CuSpatiu(os =>
                !Fiscale.Fapte(os).Any(f => f.DocumentId == inert.Id)
                && C.Citiri.Contabil.Postari(os).Any(p => p.DocumentId == inert.Id)));
            MecanismBugetar();
            return;
        }
        Comanda(os => {
            var p = os.GetObjectByKey<Partener>(Furnizor);
            p.Tara = "RO"; p.InregistratTva = true; p.CodFiscal = "RO12345674";
            os.CommitChanges();
        });
        Versiuni("N21");
        var normal = Factura(Ianuarie, new LinieFctScena(1, 100, "N21", Stoc: false));
        var capitalizat = Factura(Ianuarie, new LinieFctScena(1, 100, "NED21", Stoc: false));
        Opereaza(normal.Id); Opereaza(capitalizat.Id);
        StructuraFiscala(normal.Id);
        Verifica("SC-CIT-79", "jurnal SQL: două facturi, baza 200, taxa 42", CuSpatiu(os => {
            var jurnal = TvaProiectii.JurnalTva(os, SensTva.Achizitie, Ianuarie, new(An, 1, 31)).ToArray();
            var decont = TvaProiectii.DecontTva(os, Ianuarie, new(An, 1, 31)).ToArray();
            return jurnal.Length == 2 && jurnal.Sum(f => f.Baza) == 200 && jurnal.Sum(f => f.Tva) == 42
                && decont.Sum(f => f.Randuri) == 2 && decont.Sum(f => f.Baza) == 200;
        }));
        Verifica("SC-CIT-79", "D300: taxă deductibilă 42, dedusă 21", CuSpatiu(os => {
            var d = D300Proiectii.D300(os, new(An, 1, 1), new(An, 1, 31), null);
            return d.Randuri.Single(r => r.Cod == "30").Tva == 42
                && d.Randuri.Single(r => r.Cod == "31").Tva == 21;
        }));
        var ti = Factura(Ianuarie, new LinieFctScena(1, 100, "TI21", Stoc: false));
        Opereaza(ti.Id);
        Verifica("SC-CIT-81", "TI: bază unică 100 și cele două obligații 21", CuSpatiu(os => {
            var f = Fiscale.Fapte(os).Single(f => f.DocumentId == ti.Id);
            var p = Fiscale.Postari(os).Where(p => p.DocumentId == ti.Id).ToArray();
            return f.Baza == 100 && f.Tva == 21 && f.Autocolectare == 21 && p.Length == 3
                && p.Single(p => p.RolTva == N.RolTva.Autocolectare).SensTva == N.SensTva.Achizitie;
        }));
        Verifica("SC-CIT-81", "D300: colectată și deductibilă 21 fără oglindire dublă", CuSpatiu(os => {
            var d = D300Proiectii.D300(os, new(An, 1, 1), new(An, 1, 31), null);
            return d.Randuri.Single(r => r.Cod == "12.1").Tva == 21
                && d.Randuri.Single(r => r.Cod == "26.1").Tva == 21;
        }));
        Verifica("SC-CIT-81", "D394: o singură achiziție C, bază 100", CuSpatiu(os => {
            var d = D394Proiectii.D394(os, new(An, 1, 1), new(An, 1, 31));
            var c = d.Operatiuni.Single(r => r.Tip == "C");
            return c.NrFact == 1 && c.Baza == 100;
        }));
        Verifica("SC-CIT-81", "SAF-T: deducere 300906, autocolectare 380006 numai pe credit", CuSpatiu(os => {
            var d = SaftProiectii.Saft(os, An, 1);
            var l = d.Jurnale.SelectMany(j => j.Tranzactii).Single(t => t.DocumentId == ti.Id).Linii;
            return l.Single(p => p.AccountID == "4426").TaxInformation.TaxCode == "300906"
                && l.Single(p => p.AccountID == "4427").TaxInformation.TaxCode == "380006"
                && l.Where(p => p.AccountID == "4427").Sum(p => p.TaxInformation.TaxAmount) == 21;
        }));
        Vanzare("N21", 200, 42, "SC-CIT-79");
        Vanzare("TI21", 100, 0, "SC-CIT-81");
        var primit = Factura(new(An, 1, 31), new LinieFctScena(1, 100, "N21", Stoc: false));
        Comanda(os => {
            var f = os.GetObjectByKey<FacturaIntrare>(primit.Id);
            f.DataInregistrare = Februarie; f.DataPrimire = Februarie; os.CommitChanges();
        });
        Opereaza(primit.Id);
        Verifica("SC-CIT-85", "ianuarie deschis: factura primită în februarie are ambele perioade februarie", CuSpatiu(os => {
            var f = Fiscale.Fapte(os).Single(f => f.DocumentId == primit.Id);
            return f.DataDocument == new DateOnly(An, 1, 31) && f.DataPrimire == Februarie
                && f.PerioadaD300 == An * 100 + 2 && f.PerioadaD394 == An * 100 + 2;
        }));
        var rotunjire = Factura(Ianuarie, new LinieFctScena(1, .03m, "N21", false, .01m),
            new LinieFctScena(1, .03m, "N21", false, .01m), new LinieFctScena(1, .03m, "N21", false, .01m));
        Opereaza(rotunjire.Id);
        Verifica("SC-CIT-86", "trei linii: suma taxelor păstrează .03, nu recalcul .02", CuSpatiu(os => {
            var j = TvaProiectii.JurnalTva(os, SensTva.Achizitie).Single(f => f.DocumentId == rotunjire.Id);
            return j.Baza == .09m && j.Tva == .03m;
        }));
        var receptie = Receptioneaza(new LinieFctScena(10, 10, "N21", Tip: "371"));
        var vama = CuSpatiu(os => {
            var p = os.CreateObject<Partener>(); p.Cod = Marcaj + "-VAMA"; p.Denumire = p.Cod; p.ContImplicitId = Cont("446");
            var d = os.CreateObject<Dvi>(); d.Data = Ianuarie; d.Numar = Marcaj + "-MRN"; d.Predator = p; d.PrimitorId = Loc;
            var l = os.CreateObject<DocumentDetaliu>(); l.Document = d; l.TipMaterialId = Tip(os, Stoc);
            l.TipTvaId = Tva("IMP21"); l.Valoare = 100;
            os.CommitChanges(); return d.ID;
        });
        Opereaza(vama);
        Verifica("SC-CIT-82", "DVI: cititor comun 100/21, cartea fiscală nu dublează baza; datorie 446 de 21", CuSpatiu(os => {
            var f = Fiscale.Fapte(os).Single(f => f.DocumentId == vama);
            var d394 = D394Proiectii.D394(os, new(An, 1, 1), new(An, 1, 31));
            return f.Baza == 100 && f.Tva == 21 && f.DeImport
                && C.Citiri.Contabil.Postari(os).Where(p => p.DocumentId == vama && p.Latura == N.Latura.Credit).Sum(p => p.Valoare) == 21
                && d394.Neincluse.Any(r => r.Baza == 100 && r.Tva == 21 && r.TipTvaCod == "IMP21");
        }));
        Comanda(os => {
            FiscalitateService.ConfirmaDepunerea(os, FormularFiscal.D300, An, 1,
                Fiscale.Versiune(os, FormularFiscal.D300, new(An, 1, 1), new(An, 1, 31)), "ModelCheck");
            FiscalitateService.ConfirmaDepunerea(os, FormularFiscal.D394, An, 1,
                Fiscale.Versiune(os, FormularFiscal.D394, new(An, 1, 1), new(An, 1, 31)), "ModelCheck");
        });
        InchideIanuarie();
        var corectie = Corecteaza(normal.Id);
        Refuza("SC-CIT-84", () => Comanda(os => {
            os.Delete(os.GetObjectByKey<Document>(corectie));
            GardianEditare.Verifica(os);
        }), "TVA_CORECTIE_INCEPUTA");
        Refuza("SC-CIT-84", () => Comanda(os => FiscalitateService.ConfirmaDepunerea(os,
            FormularFiscal.D394, An, 1, "SC-CIT-84-incompleta", "ModelCheck")), "DEPUNERE_CORECTIE_DRAFT");
        Comanda(os => {
            var f = os.GetObjectByKey<FacturaIntrare>(corectie);
            var l = f.Detalii.OfType<FacturaIntrareDetaliu>().Single();
            l.PretUnitar = 80; l.ValoareTva = 16.80m; os.CommitChanges();
        });
        Opereaza(corectie);
        Verifica("SC-CIT-84", "D300: ianuarie 100/21, februarie diferență −20/−4,20", CuSpatiu(os => {
            var f = Fiscale.Fapte(os).Where(f => f.DocumentFiscalId == normal.Id).ToArray();
            return f.Length == 3 && f.Where(f => f.PerioadaD300 == An * 100 + 1).Sum(f => f.Baza) == 100
                && f.Where(f => f.PerioadaD300 == An * 100 + 2).Sum(f => f.Baza) == -20
                && f.Where(f => f.RegularizareD300).Sum(f => f.Tva) == -4.20m;
        }));
        Verifica("SC-CIT-84", "D394: corecția rămâne în ianuarie, net 80/16,80 sub aceeași factură", CuSpatiu(os => {
            var f = Fiscale.Fapte(os).Where(f => f.DocumentFiscalId == normal.Id).ToArray();
            var d = D394Proiectii.D394(os, new(An, 1, 1), new(An, 1, 31));
            return f.All(f => f.PerioadaD394 == An * 100 + 1) && f.Sum(f => f.Baza) == 80
                && f.Sum(f => f.Tva) == 16.80m && d.Rectificativa
                && d.Operatiuni.Single(r => r.Tip == "A").NrFact == 4;
        }));
        var corectieCapitalizata = Corecteaza(capitalizat.Id);
        Comanda(os => {
            var f = os.GetObjectByKey<FacturaIntrare>(corectieCapitalizata);
            f.Detalii.OfType<FacturaIntrareDetaliu>().Single().PretUnitar = 80;
            os.CommitChanges();
        });
        Opereaza(corectieCapitalizata);
        Verifica("SC-CIT-84", "corecția TVA capitalizate nu inventează deducere la regularizare", CuSpatiu(os => {
            var f = Fiscale.Fapte(os).Where(f => f.DocumentFiscalId == capitalizat.Id && f.RegularizareD300).ToArray();
            var d = D300Proiectii.D300(os, new(An, 2, 1), new(An, 2, 28), null);
            return f.Sum(r => r.Baza) == -20 && f.Sum(r => r.Tva) == -4.2m
                && d.Randuri.Single(r => r.Cod == "33").Tva == -4.2m;
        }));
        var intact = Amprenta(receptie.Id);
        var retur = CuSpatiu(os => {
            var d = os.CreateObject<ReturFurnizor>(); d.Data = Februarie; d.DataPrimire = Februarie;
            d.PredatorId = Magazie; d.PrimitorId = Furnizor;
            var l = os.CreateObject<DocumentDetaliu>(); l.Document = d; l.TipMaterialId = Tip(os, "371");
            l.Cantitate = 2; l.LotId = receptie.Linii[0].Lot; l.TipTvaId = Tva("N21");
            os.CommitChanges(); return d.ID;
        });
        Opereaza(retur);
        Verifica("SC-CIT-83", "reducere distinctă: ianuarie 100/21, februarie −20/−4,20; două documente", CuSpatiu(os => {
            var f = Fiscale.Fapte(os).Where(f => f.DocumentId == receptie.Id || f.DocumentId == retur).ToArray();
            return f.Length == 2 && f.Select(f => f.DocumentFiscalId).Distinct().Count() == 2
                && f.Single(f => f.DocumentId == receptie.Id).Baza == 100
                && f.Single(f => f.DocumentId == retur).Baza == -20
                && f.Single(f => f.DocumentId == retur).Tva == -4.20m
                && f.Single(f => f.DocumentId == retur).PerioadaD394 == An * 100 + 2
                && f.All(f => !f.InversaTehnica);
        }) && Amprenta(receptie.Id) == intact);
        Storneaza(vama, Februarie);
        Verifica("SC-CIT-82", "DVI inversă comună −100/−21", CuSpatiu(os => {
            var f = Fiscale.Fapte(os).Single(f => f.DocumentId == vama && f.Storno);
            return f.Baza == -100 && f.Tva == -21;
        }));
        Storneaza(rotunjire.Id, Februarie);
        Verifica("SC-CIT-86", "inversa păstrează −0,09/−0,03", CuSpatiu(os => {
            var f = Fiscale.Fapte(os).Where(f => f.DocumentId == rotunjire.Id && f.Storno).ToArray();
            return f.Sum(f => f.Baza) == -.09m && f.Sum(f => f.Tva) == -.03m;
        }));
        Istoric();
    }

    void Vanzare(string tva, decimal baza, decimal taxa, string proba) {
        var id = CuSpatiu(os => {
            var d = os.CreateObject<FacturaIesire>(); d.Data = Ianuarie;
            d.PredatorId = Loc; d.PrimitorId = Client;
            var l = os.CreateObject<FacturaIesireDetaliu>(); l.Document = d;
            l.TipMaterialId = Tip(os, "704"); l.Cantitate = 1; l.PretUnitar = baza; l.TipTvaId = Tva(tva);
            os.CommitChanges(); return d.ID;
        });
        Opereaza(id);
        Verifica(proba, $"livrare {tva}: {baza}/{taxa}, fără autocolectare a beneficiarului", CuSpatiu(os => {
            var f = Fiscale.Fapte(os).Single(f => f.DocumentId == id);
            return f.Sens == SensTva.Livrare && f.Baza == baza && f.Tva == taxa && f.Autocolectare == 0;
        }));
    }

    void Istoric() {
        var tva = Tva("N21");
        var vechi = Factura(Februarie, new LinieFctScena(1, 100, "N21", false));
        try {
            Comanda(os => { os.GetObjectByKey<TipTva>(tva).Cota = 19; os.CommitChanges(); });
            Opereaza(vechi.Id);
        } finally {
            Comanda(os => { os.GetObjectByKey<TipTva>(tva).Cota = 21; os.CommitChanges(); });
        }
        var nou = Factura(Februarie, new LinieFctScena(1, 100, "N21", false)); Opereaza(nou.Id);
        Verifica("SC-CIT-80", "cota tipului schimbată: istoric 19 și 21, total 40; maparea absentă nu se inventează", CuSpatiu(os => {
            var f = Fiscale.Fapte(os).Where(f => f.DocumentId == vechi.Id || f.DocumentId == nou.Id).ToArray();
            var m = new MapariFiscale(os);
            return f.Sum(f => f.Baza) == 200 && f.Sum(f => f.Tva) == 40 && f.Select(f => f.Cota).Order().SequenceEqual([19m, 21m])
                && m.Pentru(f.Single(f => f.Cota == 19), SectiuneTvaSaft.Facturi) == null
                && m.Pentru(f.Single(f => f.Cota == 21), SectiuneTvaSaft.Facturi)?.TaxCode == "301104";
        }));
        Verifica("SC-CIT-80", "D300 nu reclasifică istoricul de 19 pe rândul cotei curente 21", CuSpatiu(os => {
            var d = D300Proiectii.D300(os, new(An, 2, 1), new(An, 2, 28), null);
            return d.Nemapate.Any(r => r.TipTvaId == tva && r.Cota == 19 && r.Baza == 100 && r.Tva == 19);
        }));
        using (var os = Deschide()) {
            using var tx = TranzactieComanda.Incepe(os);
            ((EFCoreObjectSpace)os).DbContext.Entry(os.GetObjectByKey<TipTva>(tva)).Property("GCRecord").CurrentValue = 1;
            os.CommitChanges();
            var jurnal = TvaProiectii.JurnalTva(os, SensTva.Achizitie, new(An, 2, 1), new(An, 2, 28))
                .Where(r => r.DocumentId == vechi.Id || r.DocumentId == nou.Id).ToArray();
            Verifica("SC-CIT-80", "eticheta TVA ștearsă nu pierde faptele și cotele istorice", jurnal.Length == 2
                && jurnal.All(r => r.TipTvaCod == null) && jurnal.Sum(r => r.Baza) == 200 && jurnal.Sum(r => r.Tva) == 40);
        }
        Storneaza(vechi.Id, Februarie); Storneaza(nou.Id, Februarie);
        Verifica("SC-CIT-80", "inverse istorice −19 și −21, fără recotare", CuSpatiu(os => {
            var f = Fiscale.Fapte(os).Where(f => (f.DocumentId == vechi.Id || f.DocumentId == nou.Id) && f.Storno).ToArray();
            return f.Sum(f => f.Tva) == -40 && f.Sum(f => f.Baza) == -200;
        }));
        Mutanti(vechi.Id);
    }

    void Mutanti(Guid document) {
        foreach (var defect in new[] { "calificare", "autocolectare", "duplicat" }) {
            using var os = Deschide();
            using var tx = TranzactieComanda.Incepe(os);
            var p = Fiscale.Postari(os).First(p => p.DocumentId == document && p.Tranzactie.Fel == N.FelTranzactie.Operare
                && p.RolTva == N.RolTva.Taxa);
            if (defect == "calificare") p.CotaTva = 21;
            if (defect == "autocolectare") p.RolTva = N.RolTva.Autocolectare;
            if (defect == "duplicat") p.RolTva = N.RolTva.Baza;
            os.CommitChanges();
            var cod = defect switch { "calificare" => "CITIRE_FISCAL_CALIFICARE",
                "duplicat" => "CITIRE_FISCAL_DUPLICAT", _ => "CITIRE_FISCAL_AUTOLICHIDARE" };
            Refuza("SC-CIT-80", () => Invarianti.VerificaFiscal(os), cod);
        }
    }

    void MecanismBugetar() {
        Comanda(os => {
            var t = os.CreateObject<TipTva>(); t.Cod = Marcaj; t.Denumire = Marcaj;
            t.Cota = 21; t.Regim = RegimTva.Normal;
            t.ContTvaDeductibilId = Cont("473.01.09");
            var p = os.CreateObject<PoliticaTva>();
            p.TipDocumentId = os.GetObjectsQuery<TipDocument>().Single(t => t.Cod == "FCT").ID;
            p.Directie = DirectieTva.Deductibil; p.SursaContrapartida = SursaCont.RepartitorPredator;
            p.ContrapartidaFallbackId = Cont(ContFurnizor); os.CommitChanges();
        });
        Versiuni(Marcaj);
        var f = Factura(Ianuarie, new LinieFctScena(1, 100, Marcaj, false)); Opereaza(f.Id);
        StructuraFiscala(f.Id);
        Comanda(os => { os.GetObjectsQuery<TipTva>().Single(t => t.Cod == Marcaj).Cota = 19; os.CommitChanges(); });
        Storneaza(f.Id, Februarie);
        Verifica("SC-CIT-87", "bugetar cu politică explicită: istoricul și inversa folosesc 21, nu cota curentă 19", CuSpatiu(os => {
            var a = Fiscale.Fapte(os).Where(a => a.DocumentId == f.Id).ToArray();
            return a.Length == 2 && a.All(a => a.Cota == 21) && a.Sum(a => a.Tva) == 0
                && a.Single(a => !a.Storno).Tva == 21;
        }));
    }

    void Versiuni(string tva) {
        var start = new DateOnly(An, 3, 1);
        var end = new DateOnly(An, 3, 31);
        Comanda(os => { var p = os.CreateObject<PerioadaFiscala>(); p.An = An; p.Luna = 3; os.CommitChanges(); });
        var f = Factura(start, new LinieFctScena(1, 100, tva, false)); Opereaza(f.Id);
        var vechi300 = CuSpatiu(os => D300Proiectii.D300(os, start, end, null).VersiuneExportata);
        var vechi394 = CuSpatiu(os => D394Proiectii.D394(os, start, end).VersiuneExportata);
        var g = Factura(start, new LinieFctScena(1, 25, tva, false));
        using (var os = Deschide()) {
            using var tx = Fiscale.DeschideCitirea(os);
            var versiune = Fiscale.Versiune(os, FormularFiscal.D300, start, end);
            Opereaza(g.Id);
            var raport = D300Proiectii.D300(os, start, end, null);
            var fapte = Fiscale.IntreLuni(Fiscale.Fapte(os), start, end).ToArray();
            Verifica("SC-CIT-88", "scriere concurentă: versiunea și raportul păstrează snapshot-ul 100/21",
                raport.VersiuneExportata == versiune && fapte.Sum(f => f.Baza) == 100 && fapte.Sum(f => f.Tva) == 21);
        }
        foreach (var (formular, vechi) in new[] { (FormularFiscal.D300, vechi300), (FormularFiscal.D394, vechi394) })
            Refuza("SC-CIT-88", () => Comanda(os => FiscalitateService.ConfirmaDepunerea(os, formular,
                An, 3, vechi, Marcaj)), "DEPUNERE_VERSIUNE_DEPASITA");
        Verifica("SC-CIT-88", "refuz atomic, 125/26,25 și nicio depunere", CuSpatiu(os => {
            var a = Fiscale.IntreLuni(Fiscale.Fapte(os), start, end).ToArray();
            return a.Sum(r => r.Baza) == 125 && a.Sum(r => r.Tva) == 26.25m
                && !os.GetObjectsQuery<DepunereDeclaratie>().Any(d => d.Perioada == An * 100 + 3);
        }));
        var inainte = CuSpatiu(os => Fiscale.Versiune(os, FormularFiscal.D300, start, end));
        Anuleaza(g.Id); Opereaza(g.Id);
        Verifica("SC-CIT-88", "aceleași sume, alte identități: versiunea se schimbă",
            inainte != CuSpatiu(os => Fiscale.Versiune(os, FormularFiscal.D300, start, end)));
        foreach (var formular in new[] { FormularFiscal.D300, FormularFiscal.D394 }) {
            var versiune = CuSpatiu(os => formular == FormularFiscal.D300
                ? D300Proiectii.D300(os, start, end, null).VersiuneExportata
                : D394Proiectii.D394(os, start, end).VersiuneExportata);
            var altFormular = formular == FormularFiscal.D300 ? FormularFiscal.D394 : FormularFiscal.D300;
            Refuza("SC-CIT-88", () => Comanda(os => FiscalitateService.ConfirmaDepunerea(os, altFormular,
                An, 3, versiune, Marcaj)), "DEPUNERE_VERSIUNE_DEPASITA");
            Refuza("SC-CIT-88", () => Comanda(os => FiscalitateService.ConfirmaDepunerea(os, formular,
                An, 2, versiune, Marcaj)), "DEPUNERE_VERSIUNE_DEPASITA");
            var id = CuSpatiu(os => FiscalitateService.ConfirmaDepunerea(os, formular, An, 3, versiune, Marcaj));
            Verifica("SC-CIT-88", "export nou confirmat, repetare idempotentă",
                id == CuSpatiu(os => FiscalitateService.ConfirmaDepunerea(os, formular, An, 3, versiune, Marcaj)));
        }
    }

    void StructuraFiscala(Guid document) {
        foreach (var coloana in new[] { "DocumentId", "LinieId", "SensTva", "RolTva", "RegimTva", "CotaTva",
                "DeImport", "DocumentFiscalId", "DataDocument", "DataExigibilitate", "DataInregistrare",
                "PerioadaDeclarare", "PerioadaD394", "DataPrimire" }) {
            using var os = Deschide();
            using var tx = TranzactieComanda.Incepe(os);
            var refuzat = false;
            try {
                ((EFCoreObjectSpace)os).DbContext.Database.ExecuteSqlRaw(
                    $"UPDATE \"Postare\" SET \"{coloana}\" = NULL WHERE \"DocumentId\" = {{0}} AND \"TipTvaId\" IS NOT NULL", document);
            } catch (PostgresException e) when (e.SqlState == PostgresErrorCodes.CheckViolation
                    && e.ConstraintName == "CK_Postare_FiscalComplet") { refuzat = true; }
            Verifica("SC-CIT-89", $"baza refuză lipsa {coloana}", refuzat);
        }
    }
}
