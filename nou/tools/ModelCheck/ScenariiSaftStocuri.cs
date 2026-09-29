using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Cub.Citiri;
using Atlas.Conta.BackOffice.Module.Motor;
using Atlas.Conta.BackOffice.Module.Saft;
using System.Xml.Linq;
using DevExpress.ExpressApp;
using C = Atlas.Conta.BackOffice.Module.Cub;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.ModelCheck;

// TR-D8 S3: catalogul docs/nucleu/scenarii/SAFT.md (SC-SAFT-38…49) pe exportul S din cub (SaftStocuriPeCub).
sealed class ScenariiSaftStocuri(Func<IObjectSpace> deschide, Action<string, bool> check,
    bool privat, Action<IObjectSpace, int, int> inchide)
    : ScenaDocumente(deschide, check, privat, inchide, "SAFTS", 2041) {
    static readonly System.Globalization.CultureInfo Inv = System.Globalization.CultureInfo.InvariantCulture;
    int numarProdus;

    DateOnly Zi(int luna, int zi) => new(An, luna, zi);
    SaftDto Export(int luna) => CuSpatiu(os => SaftProiectii.SaftStocuriPeCub(os, An, luna, Zi(luna, 28)));
    SaftDto ExportVechi(int luna) => CuSpatiu(os => SaftProiectii.SaftStocuri(os, An, luna, Zi(luna, 28)));

    static byte[] Xml(SaftDto d) {
        using var ms = new MemoryStream();
        SaftXml.Scrie(d, ms);
        return ms.ToArray();
    }

    static bool Refuzat(Action actiune) {
        try { actiune(); return false; }
        catch (InvalidOperationException) { return true; }
    }

    bool FaraRefuzuri(SaftDto d, string id) {
        foreach (var r in d.Refuzuri) Console.WriteLine($"     REFUZ {r.Cod}: {r.Mesaj}");
        Verifica(id, $"luna {d.Luna}: exportul S pe cub fără refuzuri", d.Refuzuri.Count == 0);
        return d.Refuzuri.Count == 0;
    }

    static List<SaftMiscareStoc> Miscari(SaftDto d, Guid doc, bool storno = false) =>
        d.MiscariStoc.Where(m => m.DocumentId == doc && m.Storno == storno).ToList();

    static string Forma(SaftMiscareStoc m) => $"{m.MovementType} " + string.Join("|", m.Linii
        .Select(l => $"{l.Quantity.ToString("0.###", Inv)}/{l.BookValue.ToString("0.00", Inv)}"));

    bool Miscare(SaftDto d, Guid doc, bool storno, string cod, params (LinieScena Lot, decimal Q, decimal V)[] linii) {
        var m = Miscari(d, doc, storno).Where(x => x.MovementType == cod).ToList();
        var ok = m.Count == 1 && m[0].Linii.Count == linii.Length
            && m[0].Linii.Zip(linii).All(x => x.First.LotId == x.Second.Lot.Lot && x.First.Quantity == x.Second.Q
                && x.First.BookValue == x.Second.V && x.First.MovementSubType == cod);
        if (!ok) Console.WriteLine($"     AȘTEPTAT {cod} [{string.Join("|", linii.Select(l => $"{l.Q}/{l.V}"))}], "
            + $"GĂSIT [{string.Join("; ", Miscari(d, doc, storno).Select(Forma))}]");
        return ok;
    }

    static SaftStocFizic Pozitie(SaftDto d, LinieScena lot, Guid gestiune) =>
        d.StocFizic.SingleOrDefault(e => e.LotId == lot.Lot && e.RepartitorId == gestiune);

    static bool Pozitie(SaftDto d, LinieScena lot, Guid gestiune, decimal qi, decimal vi, decimal qf, decimal vf) =>
        Pozitie(d, lot, gestiune) is { } e && (e.OpeningQuantity, e.OpeningValue, e.ClosingQuantity, e.ClosingValue) == (qi, vi, qf, vf);

    protected override void Executa() {
        if (!Privat) {
            var inert = Receptioneaza(new LinieFctScena(10, 10));
            Verifica("SC-SAFT-48", "bugetar: S neaplicabil, fără antet, stoc fizic sau mișcări", CuSpatiu(os => {
                var d = SaftProiectii.SaftStocuriPeCub(os, An, 1);
                return d.Neaplicabil != null && d.Header == null && d.StocFizic.Count == 0 && d.MiscariStoc.Count == 0;
            }) && inert.Linii.Length == 1);
            return;
        }
        var inainte = CuSpatiu(os => os.GetObjectsQuery<Societate>().Select(x => new {
            x.CodFiscal, x.InregistratTva, x.Tara, x.Denumire, x.ContactNume, x.ContactPrenume, x.Telefon, x.ContBancarId,
        }).First());
        var ibanInainte = CuSpatiu(os => os.GetObjectsQuery<ContPropriu>().Single(c => c.Cod == "BANCA").Iban);
        Comanda(os => {
            var soc = os.GetObjectsQuery<Societate>().First();
            var banca = os.GetObjectsQuery<ContPropriu>().Single(c => c.Cod == "BANCA");
            banca.Iban = "RO49AAAA1B31007593840000";
            soc.CodFiscal = "12345674"; soc.InregistratTva = true; soc.Tara = "RO";
            soc.Denumire = "Atlas Probă SAF-T SRL"; soc.ContactNume = "Popescu"; soc.ContactPrenume = "Ion";
            soc.Telefon = "0264000000"; soc.ContBancarId = banca.ID;
            os.CommitChanges();
        });
        try { Privat1(); }
        finally {
            Comanda(os => {
                var soc = os.GetObjectsQuery<Societate>().First();
                soc.CodFiscal = inainte.CodFiscal; soc.InregistratTva = inainte.InregistratTva; soc.Tara = inainte.Tara;
                soc.Denumire = inainte.Denumire; soc.ContactNume = inainte.ContactNume; soc.ContactPrenume = inainte.ContactPrenume;
                soc.Telefon = inainte.Telefon; soc.ContBancarId = inainte.ContBancarId;
                os.GetObjectsQuery<ContPropriu>().Single(c => c.Cod == "BANCA").Iban = ibanInainte;
                os.CommitChanges();
            });
        }
    }

    FacturaScena Ldi(DateOnly data, params (DirectieDiferenta Directie, decimal Q, decimal Pret, LinieScena Lot)[] linii) {
        using var os = Deschide();
        var doc = os.CreateObject<ListaDiferenteInventar>();
        doc.Data = data; doc.DataInregistrare = data; doc.PredatorId = Magazie; doc.PrimitorId = Loc;
        var rezultat = new List<LinieScena>();
        foreach (var (directie, q, pret, lot) in linii) {
            var l = os.CreateObject<ListaDiferenteInventarDetaliu>(); l.Document = doc;
            l.Pozitie = rezultat.Count + 1; l.Directie = directie; l.Cantitate = q; l.TipMaterialId = Tip(os, Stoc);
            if (directie == DirectieDiferenta.Plus) {
                l.PretEvaluare = pret;
                var p = os.CreateObject<Produs>(); p.Cod = Marcaj + "-LDI" + ++numarProdus;
                p.Denumire = p.Cod; p.UM = "BUC"; p.TipMaterialId = l.TipMaterialId;
                l.ProdusId = p.ID;
                rezultat.Add(new(l.ID, l.CreeazaLot(os, p, os.GetObjectByKey<Gestiune>(Magazie)).ID, p.ID));
            }
            else { l.LotId = lot.Lot; rezultat.Add(new(l.ID, lot.Lot, lot.Produs)); }
        }
        os.CommitChanges(); return new(doc.ID, rezultat.ToArray());
    }

    Guid Rlf(LinieScena lot, decimal q) => CuSpatiu(os => {
        var d = os.CreateObject<ReturFurnizor>(); d.Data = Ianuarie; d.DataPrimire = Ianuarie;
        d.PredatorId = Magazie; d.PrimitorId = Furnizor;
        var l = os.CreateObject<DocumentDetaliu>(); l.Document = d; l.Pozitie = 1; l.TipMaterialId = Tip(os, "371");
        l.Cantitate = q; l.LotId = lot.Lot; l.TipTvaId = Tva("N21");
        os.CommitChanges(); return d.ID;
    });

    Guid Rdc(LinieScena lot, decimal q) => CuSpatiu(os => {
        var d = os.CreateObject<ReturClient>(); d.Data = Ianuarie; d.PredatorId = Client; d.PrimitorId = Magazie;
        var l = os.CreateObject<DocumentDetaliu>(); l.Document = d; l.Pozitie = 1; l.TipMaterialId = Tip(os, "371");
        l.Cantitate = q; l.LotId = lot.Lot;
        os.CommitChanges(); return d.ID;
    });

    FacturaScena Asamblare(LinieScena consum, decimal q, decimal valoareProdus) {
        using var os = Deschide();
        var d = os.CreateObject<Asamblare>(); d.Data = Ianuarie; d.DataInregistrare = Ianuarie;
        d.PredatorId = Magazie; d.PrimitorId = Magazie;
        var c = os.CreateObject<AsamblareDetaliu>(); c.Document = d; c.Pozitie = 1; c.Directie = DirectieAsamblare.Consum;
        c.LotId = consum.Lot; c.Cantitate = q; c.TipMaterialId = os.GetObjectByKey<Produs>(consum.Produs!.Value).TipMaterialId!.Value;
        var l = os.CreateObject<AsamblareDetaliu>(); l.Document = d; l.Pozitie = 2; l.Directie = DirectieAsamblare.Produs;
        l.TipMaterialId = Tip(os, Stoc); l.Cantitate = 1; l.PretEvaluare = valoareProdus;
        var p = os.CreateObject<Produs>(); p.Cod = Marcaj + "-ASM" + ++numarProdus; p.Denumire = p.Cod; p.UM = "BUC";
        p.TipMaterialId = l.TipMaterialId; l.ProdusId = p.ID;
        var lot = l.CreeazaLot(os, p, os.GetObjectByKey<Gestiune>(Magazie));
        os.CommitChanges(); return new(d.ID, [new(c.ID, consum.Lot, consum.Produs), new(l.ID, lot.ID, p.ID)]);
    }

    Guid Bcs(LinieScena lot, decimal q, DateOnly data) {
        var bcs = Consum(lot.Lot!.Value, q);
        Comanda(os => { var d = os.GetObjectByKey<Document>(bcs); d.Data = data; d.DataInregistrare = data; os.CommitChanges(); });
        Opereaza(bcs);
        return bcs;
    }

    string Numar(Guid doc) => CuSpatiu(os => os.GetObjectByKey<Document>(doc).Numar);

    void Privat1() {
        Comanda(os => { os.GetObjectByKey<Repartitor>(Loc).Calitati |= CalitateRepartitor.Comisie; os.CommitChanges(); });
        Comanda(os => { var p = os.CreateObject<PerioadaFiscala>(); p.An = An; p.Luna = 3; os.CommitChanges(); });

        var receptie = Receptioneaza(new LinieFctScena(10, 10, "N21"));
        var la = receptie.Linii[0];
        var marfuri = Receptioneaza(new LinieFctScena(2, 10, "N21", Tip: "371"), new LinieFctScena(5, 12, "N21", Tip: "371"));
        var (l1, l2) = (marfuri.Linii[0], marfuri.Linii[1]);
        var bcs = Bcs(la, 3, Zi(1, 10));
        var dsc = Iesire(false, (l1, 2), (l2, 2)); Opereaza(dsc.Id);
        var btr = Iesire(true, (l2, 1)); Opereaza(btr.Id);
        var ldi = Ldi(Zi(1, 12), (DirectieDiferenta.Plus, 2, 10, null), (DirectieDiferenta.Minus, 1, 0, la)); Opereaza(ldi.Id);
        var rlf = Rlf(l2, 1); Opereaza(rlf);
        var rdc = Rdc(l1, 1); Opereaza(rdc);
        var fld = Receptioneaza(new LinieFctScena(3, 3.333333m)); var ld = fld.Linii[0];
        var asm1 = Asamblare(ld, 1, 3.33m); Opereaza(asm1.Id);
        var asm2 = Asamblare(ld, 1, 3.33m); Opereaza(asm2.Id);
        var flm = Receptioneaza(new LinieFctScena(3, 10)); var lm = flm.Linii[0];
        var asm3 = Asamblare(lm, 2, 20m); Opereaza(asm3.Id);
        var fnir = Factura(Ianuarie, new LinieFctScena(4, 25, "N21"));
        var nirMinus = Opereaza(fnir.Id).ConexId!.Value;
        var fplus = Factura(Ianuarie, new LinieFctScena(4, 25, "N21"));
        var nirPlus = Opereaza(fplus.Id).ConexId!.Value;

        var ian = Export(1);
        FaraRefuzuri(ian, "SC-SAFT-49");
        Verifica("SC-SAFT-07", "FCT 10 × 10: mișcarea 10 +10/+100 pe factură, NIR-ul egal fără mișcare, SupplierID furnizorul",
            Miscare(ian, receptie.Id, false, "10", (la, 10, 100))
            && Miscari(ian, receptie.Id)[0].Linii.Single() is { CustomerId: SaftReguli.TertNeaplicabil, AccountId: "302" } r
            && r.SupplierId != SaftReguli.TertNeaplicabil && r.ShipToWarehouseId == "MAG1"
            && CuSpatiu(os => os.GetObjectsQuery<Document>().Where(d => d.DocumentSursaId == receptie.Id).Select(d => d.ID).ToList())
                .All(nir => Miscari(ian, nir).Count == 0));
        Verifica("SC-SAFT-40", "BCS 3: 70 −3/−30 pe 302; capătul de consum +3/+30 în Excluse (BCS/Consum/+1), fără poziție",
            Miscare(ian, bcs, false, "70", (la, -3, -30))
            && ian.Excluse.SingleOrDefault(x => x.TipDocument == "BCS") is { TipStoc: "Consum", Semn: 1, Numar: 1, Cantitate: 3, Valoare: 30 }
            && ian.StocFizic.All(e => e.ProductType != "602"));
        Verifica("SC-SAFT-41", "DSC pe două loturi: o mișcare 30 cu L1 −2/−20 și L2 −2/−24, CustomerID clientul, ShipFrom gestiunea",
            Miscare(ian, dsc.Id, false, "30", (l1, -2, -20), (l2, -2, -24))
            && Miscari(ian, dsc.Id)[0].Linii.All(l => l.SupplierId == SaftReguli.TertNeaplicabil
                && l.CustomerId != SaftReguli.TertNeaplicabil && l.ShipFromWarehouseId == "MAG1" && l.ShipToWarehouseId == null));
        Verifica("SC-SAFT-08", "BTR 1 din L2: 80 −1/−12 din MAG1 și +1/+12 în destinație, referința /T, fără TransactionID (Transfer)",
            Miscari(ian, btr.Id).SingleOrDefault() is { MovementType: "80", TransactionId: null } m
            && m.MovementReference.EndsWith("/T", StringComparison.Ordinal)
            && m.Linii.Count == 2 && m.Linii.Sum(l => l.Quantity) == 0 && m.Linii.Sum(l => l.BookValue) == 0
            && m.Linii.Any(l => l is { Quantity: -1, BookValue: -12, ShipFromWarehouseId: "MAG1" })
            && m.Linii.Any(l => l.Quantity == 1 && l.BookValue == 12 && l.ShipToWarehouseId == Marcaj + "-MAG"));
        Verifica("SC-SAFT-42", "LDI: plus 2 × 10 → 110 +2/+20; minus 1 din lotul 302 → 120 −1/−10",
            Miscare(ian, ldi.Id, false, "110", (ldi.Linii[0], 2, 20)) && Miscare(ian, ldi.Id, false, "120", (la, -1, -10)));
        Verifica("SC-SAFT-43", "RLF 1 din L2 → 50 −1/−12 cu SupplierID; RDC 1 în L1 → 40 +1/+10 cu CustomerID",
            Miscare(ian, rlf, false, "50", (l2, -1, -12)) && Miscari(ian, rlf)[0].Linii[0].SupplierId != SaftReguli.TertNeaplicabil
            && Miscare(ian, rdc, false, "40", (l1, 1, 10)) && Miscari(ian, rdc)[0].Linii[0].CustomerId != SaftReguli.TertNeaplicabil);
        Verifica("SC-SAFT-44", "ASM cu Δ: al doilea consum 70 −1/−3,34 și produsul 20 +1/+3,34 (ΣC), fără cod 100",
            Miscare(ian, asm2.Id, false, "70", (ld, -1, -3.34m)) && Miscare(ian, asm2.Id, false, "20", (asm2.Linii[1], 1, 3.34m))
            && ian.MiscariStoc.All(x => x.MovementType != "100"));
        Verifica("SC-SAFT-09", "ASM 2 → 1 pe același cont: Transfer cu 70 −2/−20 și 20 +1/+20, fără TransactionID; ΣV = 0, ΣQ = −1",
            Miscare(ian, asm3.Id, false, "70", (lm, -2, -20)) && Miscare(ian, asm3.Id, false, "20", (asm3.Linii[1], 1, 20))
            && Miscari(ian, asm3.Id).All(m => m.TransactionId == null && m.MovementReference.Contains("/T/", StringComparison.Ordinal)));
        Verifica("SC-SAFT-38", "ianuarie: FCT 4 × 25 intră 10 +4/+100; NIR-ul Draft nu are mișcare",
            Miscare(ian, fnir.Id, false, "10", (fnir.Linii[0], 4, 100)) && Miscari(ian, nirMinus).Count == 0);
        Verifica("SC-SAFT-40", "ianuarie: lotul 302 inițial 0, final 10 − 3 − 1 = 6/60",
            Pozitie(ian, la, Magazie, 0, 0, 6, 60));
        Verifica("SC-SAFT-41", "ianuarie: L1 2 − 2 + 1 = 1/10; L2 5 − 2 − 1 − 1 = 1/12 în MAG1 și 1/12 în destinație",
            Pozitie(ian, l1, Magazie, 0, 0, 1, 10) && Pozitie(ian, l2, Magazie, 0, 0, 1, 12) && Pozitie(ian, l2, Destinatie, 0, 0, 1, 12));
        Cusaturi(ian);
        AccesCitit();
        Chei(ian, btr.Id, asm2.Id);
        var artefactIan = Xml(ian);
        Comanda(os => {
            foreach (var formular in new[] { FormularFiscal.D300, FormularFiscal.D394 })
                FiscalitateService.ConfirmaDepunerea(os, formular, An, 1,
                    Fiscale.Versiune(os, formular, Zi(1, 1), Zi(1, 31)), "ModelCheck");
        });
        InchideIanuarie();
        Verifica("SC-SAFT-49", "ianuarie închis: exportul citit din snapshot e identic octet cu octet",
            Xml(Export(1)).AsSpan().SequenceEqual(artefactIan));

        Comanda(os => { var n = os.GetObjectByKey<NIR>(nirMinus); n.Data = Februarie; n.DataInregistrare = Februarie; os.CommitChanges(); });
        Schimba(nirMinus, 3);
        Opereaza(nirMinus);
        Comanda(os => { var n = os.GetObjectByKey<NIR>(nirPlus); n.Data = Februarie; n.DataInregistrare = Februarie; os.CommitChanges(); });
        Schimba(nirPlus, 5);
        Opereaza(nirPlus);
        Storneaza(bcs, Februarie);
        var bcs2 = Bcs(la, 2, Februarie);
        var bcs3 = Bcs(la, 1, Februarie);
        Storneaza(bcs3, Februarie);
        Storneaza(asm3.Id, Februarie);
        var ntc = Nota(Februarie, new LinieNtcScena("301", ContFurnizor, 30, RepartitorCredit: Furnizor)).Id;
        Opereaza(ntc);

        var feb = Export(2);
        FaraRefuzuri(feb, "SC-SAFT-49");
        Verifica("SC-SAFT-38", "februarie: NIR constată 3 (InClarificare) → 10 −1/−25 pe lotul facturii; final 3/75",
            Miscare(feb, nirMinus, false, "10", (fnir.Linii[0], -1, -25)) && Pozitie(feb, fnir.Linii[0], Magazie, 4, 100, 3, 75));
        Verifica("SC-SAFT-39", "februarie: NIR constată 5 (Plus) → 10 +1/+25 cu SupplierID furnizorul; final 5/125",
            Miscare(feb, nirPlus, false, "10", (fplus.Linii[0], 1, 25))
            && Miscari(feb, nirPlus)[0].Linii[0].SupplierId != SaftReguli.TertNeaplicabil
            && Pozitie(feb, fplus.Linii[0], Magazie, 4, 100, 5, 125));
        Verifica("SC-SAFT-40", "februarie: stornoul BCS 70 +3/+30 (/S), BCS nou 70 −2/−20; lotul inițial 6/60, final 7/70",
            Miscare(feb, bcs, true, "70", (la, 3, 30)) && Miscari(feb, bcs, true)[0].MovementReference.EndsWith("/S", StringComparison.Ordinal)
            && Miscare(feb, bcs2, false, "70", (la, -2, -20)) && Pozitie(feb, la, Magazie, 6, 60, 7, 70));
        Verifica("SC-SAFT-40", "februarie: Excluse pe BCS/Consum/+1 (semnul originii) = −3 + 2 + 1 − 1 = −1/−10 din 4 linii",
            feb.Excluse.SingleOrDefault(x => x.TipDocument == "BCS") is { Semn: 1, Cantitate: -1, Valoare: -10, Numar: 4 });
        Verifica("SC-SAFT-47", "operare și storno în aceeași lună: BCS-n și BCS-n/S, distincte și lizibile",
            Miscari(feb, bcs3).Single().MovementReference == $"BCS-{Numar(bcs3)}"
            && Miscari(feb, bcs3, true).Single().MovementReference == $"BCS-{Numar(bcs3)}/S");
        Verifica("SC-SAFT-09", "inversa în februarie: 70 +2/+20 și 20 −1/−20 (/S); lotul consumat 1/10 → 3/30, produsul 1/20 → 0/0",
            Miscare(feb, asm3.Id, true, "70", (lm, 2, 20)) && Miscare(feb, asm3.Id, true, "20", (asm3.Linii[1], -1, -20))
            && Pozitie(feb, lm, Magazie, 1, 10, 3, 30) && Pozitie(feb, asm3.Linii[1], Magazie, 1, 20, 0, 0));
        Verifica("SC-SAFT-49", "S3-RV1: NTC D301/C401 30 fără lot — 301 apare în reconciliere cu stoc 0, sold 30, diferența −30 explicată de NTC",
            feb.Rezumat.StocPerCont.SingleOrDefault(c => c.Cont == "301") is { ClosingStocFizic: 0, ClosingBalanta: 30, Diferenta: -30 } c301
            && c301.Componente.SingleOrDefault(x => x.Diferenta != 0) is { TipDocument: "NTC", StocFizic: 0, Balanta: 30 });
        Cusaturi(feb);

        Verifica("SC-SAFT-49", "reexportul lui ianuarie după mișcările din februarie e identic octet cu octet",
            Xml(Export(1)).AsSpan().SequenceEqual(artefactIan));
        Guid Conex(Guid fct) => CuSpatiu(os => os.GetObjectsQuery<Document>().Single(d => d.DocumentSursaId == fct).ID);
        void Receptie(Guid fct, LinieScena lot, decimal q, decimal v) {
            DeclaraAb(1, Conex(fct), lot, (q, v), null, "recepția e pe FCT în cub, pe NIR-ul conex în registru");
            DeclaraAb(1, fct, lot, null, (q, v), "recepția e pe FCT în cub, pe NIR-ul conex în registru");
        }
        Receptie(receptie.Id, la, 10, 100); Receptie(marfuri.Id, l1, 2, 20); Receptie(marfuri.Id, l2, 5, 60);
        Receptie(fld.Id, ld, 3, 10); Receptie(flm.Id, lm, 3, 30);
        DeclaraAb(1, fnir.Id, fnir.Linii[0], null, (4, 100), "FCT cu NIR conex încă Draft: recepția e numai în cub");
        DeclaraAb(1, fplus.Id, fplus.Linii[0], null, (4, 100), "FCT cu NIR conex încă Draft: recepția e numai în cub");
        DeclaraAb(1, asm2.Id, ld, (-1, -3.33m), (-1, -3.34m), "Δ ASM: consumul FIFO pe cub față de registru (S3-R2)");
        DeclaraAb(1, asm2.Id, asm2.Linii[1], (1, 3.33m), (1, 3.34m), "Δ ASM: produsul la ΣC față de P în registru (S3-R2)");
        DeclaraAb(2, nirMinus, fnir.Linii[0], (3, 75), (-1, -25), "NIR delta (S3-R1) față de recepția integrală în registru");
        DeclaraAb(2, nirPlus, fplus.Linii[0], (5, 125), (1, 25), "NIR delta (S3-R1) față de recepția integrală în registru");
        Ab(1);
        Ab(2, (bcs3, "omisiunea unei operări BCS și a stornoului ei"));
        ProbaDuala(ian);
        Perf();
        Certificare((ian, artefactIan), (feb, Xml(feb)), (Export(3), null));

        ChRefuzuri(bcs2, bcs3);
        CategorieLipsa();
    }

    void Schimba(Guid nir, decimal q) => Comanda(os => {
        var l = os.GetObjectByKey<NIR>(nir).Detalii.OfType<NirDetaliu>().Single();
        l.Cantitate = q; os.CommitChanges();
    });

    void Cusaturi(SaftDto d) {
        var r = d.Rezumat;
        Console.WriteLine($"     MĂSURAT (cusături S luna {d.Luna}): {r.StocIntrari} poziții, {r.StocIntrariDiferite} diferite; "
            + $"emise {r.StocEmiseCantitate}/{r.StocEmiseValoare} + excluse {r.ExcluseCantitate}/{r.ExcluseValoare} = "
            + $"postări {r.RegistruStocCantitate}/{r.RegistruStocValoare}; conturi {r.ConturiStocVerificate}, diferite {r.ConturiStocDiferite}");
        Verifica("SC-SAFT-49", $"luna {d.Luna}: fiecare poziție închide Opening + Σ liniilor = Closing (S3-D7a)",
            r.StocIntrari > 0 && r.StocIntrariDiferite == 0 && r.StocFizicBate);
        Verifica("SC-SAFT-49", $"luna {d.Luna}: linii emise + excluse = toate postările pe lot ale lunii (S3-D7d)", r.RegistruStocBate);
        Verifica("SC-SAFT-49", $"luna {d.Luna}: fiecare diferență stoc–sold pe cont e Σ componentelor ei, iar componentele nenule n-au stoc fizic (S3-D7b)",
            r.ConturiStocVerificate > 0 && d.StocFizic.All(e => e.OwnerId != null)
            && r.StocPerCont.All(c => c.Componente.Sum(x => x.Diferenta) == c.Diferenta
                && c.Componente.Where(x => x.Diferenta != 0).All(x => x.StocFizic == 0)));
        var gl = CuSpatiu(os => {
            var ids = d.MiscariStoc.Where(m => m.TransactionId != null).Select(m => m.TranzactieId!.Value).ToList();
            return Contabil.Postari(os).Where(p => ids.Contains(p.TranzactieId) && p.Unitate != null && p.FelUnitate == N.FelUnitate.Lot)
                .Select(p => p.ID).ToList().ToHashSet();
        });
        Verifica("SC-SAFT-49", $"luna {d.Luna}: postările fiecărei mișcări cu TransactionID sunt postări pe lot ale aceleiași tranzacții din GL (S3-D7c); Transferul, fără TransactionID, conservă valoarea pe tranzacție, iar mutarea 80 și cantitatea pe lot",
            d.MiscariStoc.Where(m => m.TransactionId != null).SelectMany(m => m.Linii).SelectMany(l => l.Postari).All(x => gl.Contains(x.Id))
            && d.MiscariStoc.Where(m => m.TransactionId == null).GroupBy(m => m.TranzactieId)
                .All(g => g.SelectMany(m => m.Linii).Sum(l => l.BookValue) == 0
                    && g.Where(m => m.MovementType == "80").SelectMany(m => m.Linii).GroupBy(l => l.LotId).All(x => x.Sum(l => l.Quantity) == 0)));
        Verifica("SC-SAFT-49", $"luna {d.Luna}: referințe unice, coduri declarate, produse declarate, identități valide",
            r.ReferinteBat && r.CoduriMiscareLipsa == 0 && r.ProduseLipsa == 0 && r.IdentitatiTertInvalide == 0
            && d.TipuriMiscare.All(t => t.Descriere == SaftReguli.CoduriMiscare[t.Cod]));
    }

    void AccesCitit() {
        var comenzi = CapturaSql.Comenzi(() => Export(1));
        var citite = CapturaSql.Tabele(comenzi);
        var verificate = CuSpatiu(os => SaftAcces.Tabele(os, SaftAcces.CititeStocuri));
        Console.WriteLine($"     MĂSURAT (SC-SAFT-48): {comenzi.Count} comenzi SQL; tabele citite [{string.Join(", ", citite.Order())}]; "
            + $"neverificate [{string.Join(", ", citite.Except(verificate).Order())}]; necitite [{string.Join(", ", verificate.Except(citite).Order())}]");
        Verifica("SC-SAFT-48", "SAFT_ACCES_INCOMPLET verifică exact tabelele citite de exportul S pe cub", citite.SetEquals(verificate));
    }

    void Chei(SaftDto d, Guid btr, Guid asm) {
        Verifica("SC-SAFT-47", "ASM mixt pe același cont: Transfer cu două coduri → ASM-n/T/70 și ASM-n/T/20",
            Miscari(d, asm).Select(m => m.MovementReference).Order(StringComparer.Ordinal)
                .SequenceEqual([$"ASM-{Numar(asm)}/T/20", $"ASM-{Numar(asm)}/T/70"]));
        Verifica("SC-SAFT-47", "StockAccountNo = lotul pe toate pozițiile și liniile; poziția XML e unică",
            d.StocFizic.All(e => e.StockAccountNo == e.LotId.ToString("N"))
            && d.MiscariStoc.SelectMany(m => m.Linii).All(l => l.StockAccountNo == l.LotId.ToString("N"))
            && d.StocFizic.Select(e => (e.WarehouseId, e.ProductCode, e.StockAccountNo, e.ProductType)).Distinct().Count() == d.StocFizic.Count);
        Verifica("SC-SAFT-12", "același lot în două gestiuni: poziții distincte după WarehouseID, suma conservată",
            d.StocFizic.Where(e => d.MiscariStoc.Single(m => m.DocumentId == btr).Linii.Select(l => l.LotId).Contains(e.LotId))
                .Select(e => e.WarehouseId).Distinct().Count() == 2);
    }

    void ChRefuzuri(Guid a, Guid b) {
        var (na, nb) = (Numar(a), Numar(b));
        try {
            Comanda(os => { os.GetObjectByKey<Document>(b).Numar = na; os.CommitChanges(); });
            var d = Export(2);
            var refs = d.MiscariStoc.Where(m => m.DocumentId == a || m.DocumentId == b).ToList();
            Verifica("SC-SAFT-47", "două BCS cu același număr: toate mișcările lor, inclusiv stornoul, primesc rezerva {TranzactieId:N}{cod}",
                refs.Count == 3 && refs.All(m => m.MovementReference == $"{m.TranzactieId:N}{m.MovementType}")
                && d.Rezumat.ReferinteBat && d.Refuzuri.Count == 0);
            Comanda(os => { os.GetObjectByKey<Document>(b).Numar = new string('9', 34); os.CommitChanges(); });
            d = Export(2);
            Verifica("SC-SAFT-47", "număr de 34 de caractere: referința lizibilă ar depăși 35 → rezerva, fără trunchiere",
                d.MiscariStoc.Where(m => m.DocumentId == b).All(m => m.MovementReference == $"{m.TranzactieId:N}{m.MovementType}")
                && d.MiscariStoc.All(m => m.MovementReference.Length <= SaftReguli.LungimeMovementReference));
        }
        finally { Comanda(os => { os.GetObjectByKey<Document>(b).Numar = nb; os.CommitChanges(); }); }

        var cod = CuSpatiu(os => os.GetObjectByKey<Repartitor>(Destinatie).Cod);
        try {
            Comanda(os => { os.GetObjectByKey<Repartitor>(Destinatie).Cod = "MAG1"; os.CommitChanges(); });
            var d = Export(1);
            Verifica("SC-SAFT-47", "două gestiuni cu același cod: SAFT_CHEIE_NEINJECTIVA, XML refuzat",
                d.Refuzuri.Any(r => r.Cod == SaftProiectii.RefuzCheie) && Refuzat(() => Xml(d)));
        }
        finally { Comanda(os => { os.GetObjectByKey<Repartitor>(Destinatie).Cod = cod; os.CommitChanges(); }); }
    }

    void CategorieLipsa() {
        var cont = Cont("37");
        var inainte = CuSpatiu(os => os.GetObjectByKey<Cont>(cont).CategorieStoc);
        try {
            Comanda(os => { os.GetObjectByKey<Cont>(cont).CategorieStoc = null; os.CommitChanges(); });
            var d = Export(1);
            Verifica("SC-SAFT-46", "371 fără categorie pe tot lanțul: SAFT_CATEGORIE_LIPSA care numește contul, XML refuzat",
                d.Refuzuri.Any(r => r.Cod == SaftProiectii.RefuzCategorieLipsa && r.Mesaj.Contains("371", StringComparison.Ordinal))
                && Refuzat(() => Xml(d)));
            Comanda(os => { os.GetObjectByKey<Cont>(Cont("371")).CategorieStoc = TipStoc.Marfuri; os.CommitChanges(); });
            var repus = Export(1);
            Verifica("SC-SAFT-46", "categoria pusă pe contul analitic repară exportul, iar configurația efectivă apare în manifest",
                repus.Refuzuri.Count == 0 && repus.CategoriiStoc.GetValueOrDefault("371") == nameof(TipStoc.Marfuri)
                && repus.CategoriiStoc.GetValueOrDefault("302") == nameof(TipStoc.Magazie)
                && repus.CategoriiStoc.GetValueOrDefault("602") == nameof(TipStoc.Consum));
        }
        finally {
            Comanda(os => {
                os.GetObjectByKey<Cont>(Cont("371")).CategorieStoc = null;
                os.GetObjectByKey<Cont>(cont).CategorieStoc = inainte;
                os.CommitChanges();
            });
        }
        var fct = CuSpatiu(os => os.GetObjectsQuery<TipDocument>().Single(t => t.Cod == "FCT").ID);
        Refuza("SC-SAFT-46", () => Comanda(os => {
            var p = os.CreateObject<PoliticaMiscareSaft>(); p.TipDocumentId = fct; p.TipStoc = TipStoc.Magazie; p.Semn = -1;
            p.Motiv = "probă"; GardianEditare.Verifica(os);
        }), "nu poate exclude");
    }

    // A/B pe (document, storno, lot, gestiune), în ambele sensuri; excepțiile au valorile vechi/noi exacte (S3-D8, S3-RV4).
    readonly record struct CheieAb(Guid Document, bool Storno, Guid Lot, Guid Gestiune);
    readonly Dictionary<(int Luna, CheieAb Cheie), ((decimal, decimal)? Vechi, (decimal, decimal)? Nou, string Motiv)> declarateAb = [];

    void DeclaraAb(int luna, Guid doc, LinieScena lot, (decimal, decimal)? vechi, (decimal, decimal)? nou, string motiv) =>
        declarateAb[(luna, new CheieAb(doc, false, lot.Lot!.Value, Magazie))] = (vechi, nou, motiv);

    static Dictionary<CheieAb, (decimal, decimal)> NetAb(IEnumerable<SaftMiscareStoc> miscari) => miscari
        .SelectMany(m => m.Linii.Select(l => (Cheie: new CheieAb(m.DocumentId, m.Storno, l.LotId, l.RepartitorId), l.Quantity, l.BookValue)))
        .GroupBy(x => x.Cheie).ToDictionary(g => g.Key, g => (g.Sum(x => x.Quantity), g.Sum(x => x.BookValue)));

    bool EchivalentAb(int luna, Dictionary<CheieAb, (decimal, decimal)> vechi, Dictionary<CheieAb, (decimal, decimal)> nou,
            out List<string> neclasificate) {
        var diferite = vechi.Keys.Union(nou.Keys)
            .Where(k => !vechi.TryGetValue(k, out var v) || !nou.TryGetValue(k, out var n) || v != n)
            .ToDictionary(k => k, k => (Vechi: vechi.TryGetValue(k, out var v) ? v : ((decimal, decimal)?)null,
                Nou: nou.TryGetValue(k, out var n) ? n : ((decimal, decimal)?)null));
        var declarate = declarateAb.Where(d => d.Key.Luna == luna).ToDictionary(d => d.Key.Cheie, d => d.Value);
        neclasificate = diferite.Where(d => !declarate.TryGetValue(d.Key, out var x) || x.Vechi != d.Value.Vechi || x.Nou != d.Value.Nou)
            .Select(d => $"{d.Key} {d.Value.Vechi} → {d.Value.Nou}").ToList();
        return neclasificate.Count == 0 && declarate.Keys.All(diferite.ContainsKey);
    }

    void Ab(int luna, params (Guid Document, string Omisiune)[] omisiuni) {
        var vechi = NetAb(ExportVechi(luna).MiscariStoc);
        var nou = Export(luna);
        var ok = EchivalentAb(luna, vechi, NetAb(nou.MiscariStoc), out var neclasificate);
        Console.WriteLine($"     MĂSURAT (A/B S luna {luna}): {vechi.Count} chei vechi, {NetAb(nou.MiscariStoc).Count} pe cub; "
            + $"declarate [{string.Join("; ", declarateAb.Where(d => d.Key.Luna == luna).Select(d => d.Value.Motiv).Distinct())}]; "
            + $"neclasificate [{string.Join("; ", neclasificate)}]");
        Verifica("SC-SAFT-49", $"A/B S luna {luna}: pe document × storno × lot × gestiune, diferențele coincid exact (cheie, vechi, nou) cu cele declarate",
            ok);
        var exceptata = declarateAb.Keys.First(k => k.Luna == luna && declarateAb[k].Nou != null).Cheie;
        var excesiv = NetAb(nou.MiscariStoc);
        excesiv[exceptata] = (excesiv[exceptata].Item1, excesiv[exceptata].Item2 + 100);
        var mutanti = omisiuni.All(o => !EchivalentAb(luna, vechi, NetAb(nou.MiscariStoc.Where(m => m.DocumentId != o.Document)), out _))
            && !EchivalentAb(luna, vechi, excesiv, out _);
        Verifica("SC-SAFT-49", $"A/B S luna {luna}: mutanții sunt respinși — o diferență de 100 pe o cheie exceptată"
            + (omisiuni.Length > 0 ? $", {string.Join(", ", omisiuni.Select(o => o.Omisiune))}" : ""), mutanti);
    }

    void ProbaDuala(SaftDto d) {
        var categorii = CuSpatiu(os => {
            var c = new CategoriiStoc(os);
            var ids = d.StocFizic.Select(e => e.ContId).Distinct().ToList();
            return ids.ToDictionary(id => id, id => c.Rezolva(id));
        });
        var registru = CuSpatiu(os => {
            var loturi = d.StocFizic.Select(e => e.LotId).Distinct().ToList();
            return os.GetObjectsQuery<RegistruStoc>().Where(r => loturi.Contains(r.LotId))
                .Select(r => new { r.LotId, r.RepartitorId, r.TipStoc }).Distinct().ToList();
        });
        var rezultat = d.StocFizic.Select(e => {
            var vechi = registru.Where(r => r.LotId == e.LotId && r.RepartitorId == e.RepartitorId).Select(r => r.TipStoc).Distinct().ToList();
            var nou = categorii[e.ContId];
            return (e.LotId, Stare: vechi.Count == 0 ? "absent" : vechi.Count > 1 ? "ambiguu" : vechi[0] == nou ? "egal" : "diferit");
        }).ToList();
        Console.WriteLine("     MĂSURAT (S3-D7e proba duală, ianuarie): " + string.Join(", ",
            rezultat.GroupBy(x => x.Stare).OrderBy(g => g.Key, StringComparer.Ordinal).Select(g => $"{g.Key} {g.Count()}")));
        Verifica("SC-SAFT-49", "proba duală: categoria derivată din cont coincide cu TipStoc-ul registrului pe fiecare poziție cu corespondent unic",
            rezultat.Any(x => x.Stare == "egal") && rezultat.All(x => x.Stare is "egal" or "absent"));
    }

    void Perf() {
        var masuri = new[] { 1, 2 }.Select(luna => {
            SaftDto dto = null;
            var comenzi = CapturaSql.Comenzi(() => dto = Export(luna)).Count;
            return (Luna: luna, Comenzi: comenzi, Miscari: dto.MiscariStoc.Count, Pozitii: dto.StocFizic.Count);
        }).ToList();
        Console.WriteLine("     MĂSURAT (S3-D8 perf): " + string.Join("; ", masuri.Select(m => $"luna {m.Luna}: {m.Miscari} mișcări, {m.Pozitii} poziții, {m.Comenzi} comenzi SQL")));
        Verifica("SC-SAFT-49", "exportul S nu face o interogare per mișcare: numărul de comenzi SQL nu crește cu volumul lunii",
            masuri[0].Miscari > masuri[1].Miscari * 2 && masuri.Max(m => m.Comenzi) - masuri.Min(m => m.Comenzi) <= 2);
    }

    void Certificare(params (SaftDto Dto, byte[] Xml)[] luni) {
        var director = Path.Combine(Duk.DirectorTemporar(), $"s3-{DateTime.UtcNow:yyyyMMdd-HHmmss}");
        Directory.CreateDirectory(director);
        var validari = new List<ValidareD406>();
        ValidareD406 Valideaza(string nume, byte[] continut, SaftDto dto) {
            var cale = Path.Combine(director, nume + ".xml");
            File.WriteAllBytes(cale, continut);
            var v = ValidareD406.Ruleaza(cale, dto);
            v = v with { Provenienta = v.Provenienta with { Pozitii = [.. v.Provenienta.Pozitii.Select(z => {
                var (numar, sha) = Surse(z, dto.DataEnd);
                return z with { Surse = numar, ShaSurse = sha };
            })] } };
            validari.Add(v);
            Console.WriteLine($"     MĂSURAT (S3 {nume}, perioada din antet {v.An}-{v.Luna:00}): {v.Rezumat}");
            foreach (var e in v.EroriXsd.Take(10)) Console.WriteLine($"         EROARE XSD: {e}");
            foreach (var e in v.Duk.Erori.Take(10)) Console.WriteLine($"         EROARE DUK: {e}");
            foreach (var a in v.Duk.Avertismente.Take(10)) Console.WriteLine($"         ATENȚIONARE DUK: {a}");
            return v;
        }
        var rezultate = luni.Select(l => {
            var xml = l.Xml ?? Xml(l.Dto);
            return (l.Dto, Xml: xml, V: Valideaza($"saft-S-{An}-{l.Dto.Luna:00}", xml, l.Dto));
        }).ToList();
        foreach (var (dto, _, v) in rezultate)
            Verifica("SC-SAFT-49", $"luna {dto.Luna} ({dto.MiscariStoc.Count} mișcări): XML S acceptat de XSD v249 și de DUK {ManifestD406.VersiuneValidator}, fără atenționări",
                v.Valid && v.Duk.Avertismente.Count == 0 && (v.An, v.Luna) == (An, dto.Luna));
        var manifest = ValidareD406.ScrieManifest(director, validari);
        Console.WriteLine($"     MANIFEST S3: {manifest}");
        XNamespace ns = SaftXml.SpatiuNume;
        static byte[] Muta(byte[] xml, Action<XDocument> mutatie) {
            var doc = XDocument.Load(new MemoryStream(xml));
            mutatie(doc);
            using var ms = new MemoryStream();
            doc.Save(ms);
            return ms.ToArray();
        }
        foreach (var (dto, xml, v) in rezultate) {
            var p = ValidareD406.CitesteProvenienta(manifest, v.Fisier);
            Verifica("SC-SAFT-49", $"luna {dto.Luna}: manifestul leagă fiecare (MovementReference, LineNumber) de postările (Spatiu, ID), codul "
                + "fiecărei mișcări e cel al politicii pe tipul, categoria și semnul originii, iar fiecare poziție are sursele "
                + "(număr și SHA-256) din care Opening și Closing se recalculează direct din postări",
                Provenienta(p, xml, dto.DataStart, dto.DataEnd) && ValidareD406.ShaCategorii(p.CategoriiStoc)?.Length == 64);
            if (dto.Luna != 1) continue;
            var a = p.Miscari[0];
            var b = p.Miscari.First(x => !x.Postari.Any(s => a.Postari.Any(t => t.Id == s.Id)));
            var sold = Muta(xml, d => {
                var e = d.Descendants(ns + "ClosingStockValue").First();
                e.Value = (decimal.Parse(e.Value, Inv) + 1).ToString("0.00", Inv);
            });
            var cod = Muta(xml, d => {
                var m = d.Descendants(ns + "StockMovement").First();
                var nou = (string)m.Element(ns + "MovementType") == "180" ? "160" : "180";
                m.Element(ns + "MovementType")!.Value = nou;
                foreach (var st in m.Descendants(ns + "MovementSubType")) st.Value = nou;
            });
            Verifica("SC-SAFT-49", "mutanți de proveniență S: linie omisă, postări permutate între linii, poziție cu alt lot, sursele poziției "
                + "schimbate, ClosingStockValue + 1 și alt cod pe o mișcare sunt respinși",
                !Provenienta(p with { Miscari = p.Miscari.Skip(1).ToList() }, xml, dto.DataStart, dto.DataEnd)
                && !Provenienta(p with { Miscari = [.. p.Miscari.Select(x => x == a ? a with { Postari = b.Postari } : x == b ? b with { Postari = a.Postari } : x)] }, xml, dto.DataStart, dto.DataEnd)
                && !Provenienta(p with { Pozitii = [p.Pozitii[0] with { Lot = p.Pozitii[^1].Lot }, .. p.Pozitii.Skip(1)] }, xml, dto.DataStart, dto.DataEnd)
                && !Provenienta(p with { Pozitii = [p.Pozitii[0] with { ShaSurse = p.Pozitii[^1].ShaSurse }, .. p.Pozitii.Skip(1)] }, xml, dto.DataStart, dto.DataEnd)
                && !Provenienta(p, sold, dto.DataStart, dto.DataEnd)
                && !Provenienta(p, cod, dto.DataStart, dto.DataEnd));
        }
    }

    // Sursele poziției: postările pe lot ale cheii ei, până la capătul lunii (S3-RV2).
    (int Numar, string Sha) Surse(LegaturaPozitie z, DateOnly capat) {
        var ids = CuSpatiu(os => Loturi.Postari(os).Where(x => x.Unitate == z.Lot && x.Cont == z.Cont && x.Produs == z.Produs
            && x.Gestiune == z.Gestiune && x.Data <= capat).Select(x => new { x.Spatiu, x.ID }).ToList());
        var text = string.Join('\n', ids.OrderBy(x => x.Spatiu).ThenBy(x => x.ID).Select(x => $"{x.Spatiu}/{x.ID:N}"));
        return (ids.Count, Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(text))));
    }

    bool Provenienta(ProvenientaD406 p, byte[] xml, DateOnly start, DateOnly capat) {
        XNamespace ns = SaftXml.SpatiuNume;
        var doc = XDocument.Load(new MemoryStream(xml));
        var miscariXml = doc.Descendants(ns + "StockMovement").Select(m => (
            Referinta: (string)m.Element(ns + "MovementReference"), Cod: (string)m.Element(ns + "MovementType"),
            Linii: m.Elements(ns + "StockMovementLine").Select(l => (
                Numar: int.Parse((string)l.Element(ns + "LineNumber"), Inv), SubCod: (string)l.Element(ns + "MovementSubType"),
                Cont: (string)l.Element(ns + "AccountID"), Lot: (string)l.Element(ns + "StockAccountNo"),
                Q: decimal.Parse((string)l.Element(ns + "Quantity"), Inv), V: decimal.Parse((string)l.Element(ns + "BookValue"), Inv),
                Gestiune: (string)(l.Element(ns + "ShipTo") ?? l.Element(ns + "ShipFrom"))?.Element(ns + "WarehouseID"))).ToList())).ToList();
        var linii = miscariXml.SelectMany(m => m.Linii.Select(l => (Cheie: (m.Referinta, l.Numar), m.Cod, L: l))).ToList();
        var pozitiiXml = doc.Descendants(ns + "PhysicalStockEntry").Select(e => (
            Cheie: ((string)e.Element(ns + "WarehouseID"), (string)e.Element(ns + "ProductCode"),
                (string)e.Element(ns + "StockAccountNo"), (string)e.Element(ns + "ProductType")),
            Qi: decimal.Parse((string)e.Element(ns + "OpeningStockQuantity"), Inv), Vi: decimal.Parse((string)e.Element(ns + "OpeningStockValue"), Inv),
            Qf: decimal.Parse((string)e.Element(ns + "ClosingStockQuantity"), Inv), Vf: decimal.Parse((string)e.Element(ns + "ClosingStockValue"), Inv)))
            .ToList();
        var miscari = p.Miscari ?? [];
        var ids = miscari.SelectMany(m => m.Postari).Select(x => x.Id).ToList();
        var cub = CuSpatiu(os => os.GetObjectsQuery<C.Postare>().Where(x => ids.Contains(x.ID)).Select(x => new {
            x.ID, x.Spatiu, x.TranzactieId, x.Tranzactie.Fel, x.Tranzactie.DocumentId, x.Cont, x.Unitate, x.Gestiune, x.Produs, x.Cantitate,
            Valoare = x.Latura == N.Latura.Debit ? x.Valoare : -x.Valoare }).ToList());
        var simboluri = CuSpatiu(os => os.GetObjectsQuery<Cont>().ToDictionary(k => k.ID, k => SaftReguli.SimbolSaft(k.Simbol)));
        var gestiuni = CuSpatiu(os => os.GetObjectsQuery<Repartitor>().ToDictionary(r => r.ID, r => r.Cod));
        var produse = CuSpatiu(os => os.GetObjectsQuery<Produs>().ToDictionary(x => x.ID, x => x.Cod));
        var coduriTip = CuSpatiu(os => {
            var docs = cub.Where(x => x.DocumentId != null).Select(x => x.DocumentId.Value).Distinct().ToList();
            var tipuri = os.GetObjectsQuery<TipDocument>().Select(t => new { t.ClrType, t.Cod }).ToList();
            return os.GetObjectsQuery<Document>().Where(x => docs.Contains(x.ID)).Select(x => new { x.ID, x.ClrType }).ToList()
                .ToDictionary(x => x.ID, x => tipuri.Single(t => t.ClrType == x.ClrType).Cod);
        });
        var politici = CuSpatiu(os => os.GetObjectsQuery<PoliticaMiscareSaft>()
            .Select(x => new { Tip = x.TipDocument.Cod, x.TipStoc, x.Semn, x.CodMiscare }).ToList());
        var categorii = CuSpatiu(os => {
            var cat = new CategoriiStoc(os);
            return cub.Select(x => x.Cont).Distinct().ToDictionary(id => id, id => cat.Rezolva(id));
        });
        string CodPolitica(IReadOnlyCollection<Guid> surse) {
            var x = cub.Where(c => surse.Contains(c.ID)).ToList();
            if (x.Count == 0 || x[0].DocumentId is not Guid d || categorii[x[0].Cont] is not TipStoc categorie) return null;
            var semn = Math.Sign(x.Sum(c => c.Cantitate)) * (x[0].Fel == N.FelTranzactie.Storno ? -1 : 1);
            var tip = coduriTip.GetValueOrDefault(d);
            var candidati = politici.Where(r => r.Tip == tip && r.TipStoc == categorie).ToList();
            return (candidati.FirstOrDefault(r => r.Semn == semn) ?? candidati.FirstOrDefault(r => r.Semn == null))?.CodMiscare;
        }
        var dupaCheie = linii.GroupBy(l => l.Cheie).ToDictionary(g => g.Key, g => g.First());
        bool Linie(LegaturaMiscare m) {
            if (!dupaCheie.TryGetValue((m.MovementReference, m.LineNumber), out var x) || m.Postari.Count == 0) return false;
            var surse = cub.Where(c => m.Postari.Any(s => s.Id == c.ID && s.Spatiu == c.Spatiu)).ToList();
            var cod = CodPolitica(surse.Select(c => c.ID).ToList());
            return surse.Count == m.Postari.Count && surse.All(c => c.TranzactieId == m.TranzactieId)
                && surse.Select(c => (c.Cont, c.Unitate, c.Gestiune)).Distinct().Count() == 1
                && simboluri[surse[0].Cont] == x.L.Cont && surse[0].Unitate!.Value.ToString("N") == x.L.Lot
                && gestiuni[surse[0].Gestiune!.Value] == x.L.Gestiune
                && surse.Sum(c => c.Cantitate) == x.L.Q && surse.Sum(c => c.Valoare) == x.L.V
                && cod != null && x.Cod == cod && x.L.SubCod == cod;
        }
        var legate = (p.Pozitii ?? []).ToList();
        bool Pozitie(LegaturaPozitie z) {
            var gasite = pozitiiXml.Where(e => e.Cheie == (z.WarehouseID, z.ProductCode, z.StockAccountNo, z.ProductType)).ToList();
            if (gasite.Count != 1 || Surse(z, capat) != (z.Surse, z.ShaSurse)) return false;
            var sursa = CuSpatiu(os => Loturi.Postari(os).Where(x => x.Unitate == z.Lot && x.Cont == z.Cont && x.Produs == z.Produs
                && x.Gestiune == z.Gestiune && x.Data <= capat).Select(x => new { x.Data, x.Tranzactie.Fel, x.Cantitate,
                    Valoare = x.Latura == N.Latura.Debit ? x.Valoare : -x.Valoare }).ToList());
            var initiale = sursa.Where(x => x.Data < start || x.Fel == N.FelTranzactie.Deschidere).ToList();
            return z.StockAccountNo == z.Lot.ToString("N") && gestiuni[z.Gestiune] == z.WarehouseID
                && simboluri[z.Cont] == z.ProductType && produse[z.Produs] == z.ProductCode
                && (gasite[0].Qi, gasite[0].Vi, gasite[0].Qf, gasite[0].Vf)
                    == (initiale.Sum(x => x.Cantitate), initiale.Sum(x => x.Valoare), sursa.Sum(x => x.Cantitate), sursa.Sum(x => x.Valoare));
        }
        return miscari.Count == linii.Count && dupaCheie.Count == linii.Count
            && miscari.Select(m => (m.MovementReference, m.LineNumber)).ToHashSet().SetEquals(linii.Select(l => l.Cheie))
            && ids.Distinct().Count() == ids.Count && miscari.All(Linie)
            && miscariXml.All(m => m.Linii.All(l => l.SubCod == m.Cod))
            && legate.Count == pozitiiXml.Count && legate.All(Pozitie);
    }
}
