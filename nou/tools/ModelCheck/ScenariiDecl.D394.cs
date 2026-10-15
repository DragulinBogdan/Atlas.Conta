using System.Xml.Linq;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Proiectii;
using DevExpress.ExpressApp;

namespace Atlas.Conta.BackOffice.ModelCheck;

sealed partial class ScenariiDecl {
    const string SeriePlaja = "DCLT";
    int numarFactura = 100;

    void D394() {
        LunaGoalaD394();
        if (!Privat)
            return;
        Comanda(os => {
            foreach (var luna in new[] { 4, 5, 6, 7, 10, 11, 12 }) {
                var p = os.CreateObject<PerioadaFiscala>(); p.An = An; p.Luna = luna;
            }
            var plaja = os.CreateObject<PlajaFacturi>();
            plaja.Serie = SeriePlaja; plaja.NumarInitial = 1; plaja.NumarFinal = 1000; plaja.DataDecizie = new(An, 1, 1);
            os.CommitChanges();
        });
        LunaCuOperatiuniD394();
        RefuzuriD394();
        TrimestruD394();
        StornoInAltaLuna();
        DepunereaFisierului();
    }

    void StornoInAltaLuna() {
        var octombrie = VanzareNumerotata(Luna(10), 100, "N21");
        Opereaza(octombrie);
        Opereaza(VanzareNumerotata(Luna(11), 50, "N21"));
        var numarNoiembrie = numarFactura;
        Storneaza(octombrie, Luna(11, 20));
        var (inceput, sfarsit) = Perioada(11);
        var doc = XDocument.Load(ExportaD394("SC-DECL-11", inceput, sfarsit));
        XNamespace ns = FormulareAnaf.D394v5.SpatiuNume;
        var tip2 = doc.Root.Elements(ns + "serieFacturi").Where(x => (string)x.Attribute("tip") == "2")
            .Select(x => $"{x.Attribute("nrI").Value}-{x.Attribute("nrF").Value}").ToList();
        Verifica("SC-DECL-11", $"stornoul unei facturi din octombrie nu e factură emisă în noiembrie: nrFacturi 1, emise {numarNoiembrie}–{numarNoiembrie}",
            (string)doc.Root.Element(ns + "informatii").Attribute("nrFacturi") == "1"
            && tip2.SequenceEqual([$"{numarNoiembrie}-{numarNoiembrie}"]));
    }

    void LunaGoalaD394() {
        var (inceput, sfarsit) = Perioada(3);
        Antet(s => s.Telefon = null);
        Verifica("SC-DECL-08", "D394 fără telefonul societății: refuz antet",
            RefuzD394(inceput, sfarsit, CoduriDeclaratie.AntetIncomplet, "telefonul"));
        Antet();
        var d = Atribute(ExportaD394("SC-DECL-09", inceput, sfarsit));
        Verifica("SC-DECL-09", "luna fără operațiuni: op_efectuate 0, L/3, totalPlata_A 0, fără op1",
            d["op_efectuate"] == "0" && d["tip_D394"] == "L" && d["luna"] == "3" && d["totalPlata_A"] == "0");
    }

    void LunaCuOperatiuniD394() {
        Opereaza(VanzareNumerotata(Luna(4), 1000.50m, "N21"));
        Opereaza(VanzareNumerotata(Luna(4), 200, "N11"));
        Opereaza(VanzareNumerotata(Luna(4), 500, "TI21", Produs(CategorieTaxareInversa.Deseuri, "704")));
        Opereaza(Achizitie(Luna(4), 100.40m, "N21"));
        Opereaza(Achizitie(Luna(4), 300, "TI21", Produs(CategorieTaxareInversa.Deseuri, Serviciu)));
        var (inceput, sfarsit) = Perioada(4);
        var xml = ExportaD394("SC-DECL-07", inceput, sfarsit);
        var doc = XDocument.Load(xml);
        XNamespace ns = FormulareAnaf.D394v5.SpatiuNume;
        string Op(string tip, int cota) => doc.Root.Elements(ns + "op1")
            .Where(o => (string)o.Attribute("tip") == tip && (int)o.Attribute("cota") == cota)
            .Select(o => $"{o.Attribute("nrFact")?.Value}/{o.Attribute("baza")?.Value}/{o.Attribute("tva")?.Value}"
                + string.Concat(o.Elements(ns + "op11").Select(b =>
                    $"+{b.Attribute("codPR")?.Value}:{b.Attribute("nrFactPR")?.Value}/{b.Attribute("bazaPR")?.Value}/{b.Attribute("tvaPR")?.Value}")))
            .SingleOrDefault();
        Verifica("SC-DECL-07", "op1: L21 1/1001/210, L11 1/200/22, V 1/500 cu op11 22, A21 1/100/21, C21 1/300/63 cu op11 22",
            (Op("L", 21), Op("L", 11), Op("V", 0), Op("A", 21), Op("C", 21))
            == ("1/1001/210", "1/200/22", "1/500/+22:1/500/", "1/100/21", "1/300/63+22:1/300/63"));
        var r2 = doc.Root.Elements(ns + "rezumat2").ToDictionary(r => (int)r.Attribute("cota"),
            r => $"{r.Attribute("nrFacturiL").Value}/{r.Attribute("bazaL").Value}/{r.Attribute("tvaL").Value}|"
                + $"{r.Attribute("nrFacturiA").Value}/{r.Attribute("bazaA").Value}/{r.Attribute("tvaA").Value}");
        Verifica("SC-DECL-07", "rezumat2: 21 = L 1/1001/210 și A+C 2/400/84; 11 = L 1/200/22",
            r2.Count == 2 && r2[21] == "1/1001/210|2/400/84" && r2[11] == "1/200/22|0/0/0");
        var detalii = doc.Root.Elements(ns + "rezumat1").SelectMany(r => r.Elements(ns + "detaliu")
            .Select(x => $"{r.Attribute("cota").Value}:{x.Attribute("bun").Value}")).Order().ToList();
        var serii = doc.Root.Elements(ns + "serieFacturi")
            .Select(s => $"{s.Attribute("tip").Value}:{s.Attribute("serieI")?.Value}:{s.Attribute("nrI").Value}-{s.Attribute("nrF").Value}").ToList();
        var info = doc.Root.Element(ns + "informatii");
        Verifica("SC-DECL-07", "rezumat1 cu detaliul bunului 22 pe V (cota 0) și pe C (21); plaja 1–1000 și emise 101–103; nrFacturi 3; nrCui1 2; totalPlata_A 1603",
            detalii.SequenceEqual(["0:22", "21:22"])
            && serii.SequenceEqual([$"1:{SeriePlaja}:1-1000", $"2:{SeriePlaja}:101-103"])
            && (string)info.Attribute("nrFacturi") == "3" && (string)info.Attribute("nrCui1") == "2"
            && (string)doc.Root.Attribute("totalPlata_A") == "1603" && (string)doc.Root.Attribute("op_efectuate") == "1");
        var aDoua = CuSpatiu(os => D394Xml.Genereaza(os, inceput, sfarsit, null)).Continut;
        Verifica("SC-DECL-06", "reexportul D394 al aceleiași perioade e identic byte cu byte", aDoua.AsSpan().SequenceEqual(File.ReadAllBytes(xml)));
        MutantiD394(xml);
    }

    void MutantiD394(string xml) {
        XNamespace ns = FormulareAnaf.D394v5.SpatiuNume;
        foreach (var (eticheta, schimba, poarta) in new (string, Action<XDocument>, string)[] {
            ("baza op1 L21 + 1 fără rezumat", d => Op1(d, ns, "L", 21).SetAttributeValue("baza", "1002"), "DUK"),
            ("totalPlata_A + 1", d => d.Root.SetAttributeValue("totalPlata_A", "1604"), "DUK"),
            ("codPR în afara listei", d => Op1(d, ns, "C", 21).Element(ns + "op11").SetAttributeValue("codPR", "99"), "XSD"),
        }) {
            var doc = XDocument.Load(xml);
            schimba(doc);
            var cale = Path.Combine(Duk.DirectorTemporar(), $"{Marcaj}-mutant-d394-{eticheta.Split(' ')[0]}.xml");
            doc.Save(cale);
            var xsd = ValidareDeclaratii.Xsd(ValidareDeclaratii.D394, cale);
            var respins = poarta == "XSD" ? xsd.Count > 0 : !ValidareDeclaratii.Duk(ValidareDeclaratii.D394, cale).Valid && xsd.Count == 0;
            Verifica("SC-DECL-10", $"mutantul D394 „{eticheta}” e respins de {poarta}", respins);
        }
    }

    static XElement Op1(XDocument d, XNamespace ns, string tip, int cota) => d.Root.Elements(ns + "op1")
        .Single(o => (string)o.Attribute("tip") == tip && (int)o.Attribute("cota") == cota);

    void RefuzuriD394() {
        var afara = VanzareNumerotata(Luna(5), 100, "N21", numar: SeriePlaja + "2000");
        Opereaza(afara);
        var (mai, maiSfarsit) = Perioada(5);
        Verifica("SC-DECL-08", "factura emisă în afara plajei: refuz cu numărul ei",
            RefuzD394(mai, maiSfarsit, CoduriDeclaratie.DateNeincluse, SeriePlaja + "2000"));
        Opereaza(VanzareNumerotata(Luna(6), 300, "TI21", Produs(null, "704")));
        var (iunie, iunieSfarsit) = Perioada(6);
        Verifica("SC-DECL-08", "livrare cu taxare inversă pe un produs fără categorie: refuz FaraOp11",
            RefuzD394(iunie, iunieSfarsit, CoduriDeclaratie.DateNeincluse, nameof(CodAvertismentD394.FaraOp11)));
    }

    void TrimestruD394() {
        Opereaza(VanzareNumerotata(Luna(7), 100, "N21"));
        Antet(s => s.PeriodicitateTva = PeriodicitateTva.Trimestrial);
        var (inceput, sfarsit) = Perioada(7, 3);
        var d = Atribute(ExportaD394("SC-DECL-09", inceput, sfarsit));
        Verifica("SC-DECL-09", "trimestrul III: T/9 cu factura din iulie", d["tip_D394"] == "T" && d["luna"] == "9" && d["op_efectuate"] == "1");
        Antet();
    }

    bool RefuzD394(DateOnly inceput, DateOnly sfarsit, string cod, string fragment) => CuSpatiu(os => {
        var f = D394Xml.Genereaza(os, inceput, sfarsit, null);
        return f.Continut == null && f.Refuzuri.Any(r => r.Cod == cod && r.Mesaj.Contains(fragment));
    });

    string ExportaD394(string id, DateOnly inceput, DateOnly sfarsit) {
        var f = CuSpatiu(os => D394Xml.Genereaza(os, inceput, sfarsit, null));
        Verifica(id, $"fișier D394 {inceput:yyyy-MM}…{sfarsit:yyyy-MM} fără refuzuri ({string.Join("; ", f.Refuzuri.Select(r => r.Mesaj))})",
            f.Continut != null && f.Refuzuri.Count == 0);
        var cale = Path.Combine(Duk.DirectorTemporar(), $"{Marcaj}-{(Privat ? "privat" : "bugetar")}-{f.NumeFisier ?? $"refuzat-d394-{inceput:MM}.xml"}");
        File.WriteAllBytes(cale, f.Continut ?? []);
        var xsd = f.Continut == null ? ["fără conținut"] : ValidareDeclaratii.Xsd(ValidareDeclaratii.D394, cale);
        Verifica(id, $"XSD D394 v5: {(xsd.Count == 0 ? "valid" : string.Join("; ", xsd.Take(3)))}", xsd.Count == 0);
        var duk = ValidareDeclaratii.Duk(ValidareDeclaratii.D394, cale);
        Verifica(id, $"DUKIntegrator {duk.Versiune}: {duk.Rezumat}{(duk.Valid ? "" : " — " + string.Join("; ", duk.Erori.Take(4)))}"
            + string.Concat(duk.Avertismente.Take(2).Select(a => " | " + a)), duk.Valid && duk.Avertismente.Count == 0);
        return cale;
    }

    Guid Produs(CategorieTaxareInversa? categorie, string tip) => CuSpatiu(os => {
        var p = os.CreateObject<Produs>(); p.Cod = Marcaj + "-PR-" + Guid.NewGuid().ToString("N")[..8]; p.Denumire = p.Cod; p.UM = "BUC";
        p.TipMaterialId = Tip(os, tip); p.CategorieTaxareInversa = categorie;
        os.CommitChanges(); return p.ID;
    });

    Guid VanzareNumerotata(DateOnly data, decimal baza, string tva, Guid? produs = null, string numar = null,
            Guid? primitor = null) => CuSpatiu(os => {
        var d = os.CreateObject<FacturaIesire>(); d.Data = data; d.Numar = numar ?? SeriePlaja + ++numarFactura;
        d.PredatorId = Loc; d.PrimitorId = primitor ?? Client;
        var l = os.CreateObject<FacturaIesireDetaliu>(); l.Document = d; l.Pozitie = 1;
        l.TipMaterialId = Tip(os, "704"); l.Cantitate = 1; l.PretUnitar = baza; l.TipTvaId = Tva(tva); l.ProdusId = produs;
        os.CommitChanges(); return d.ID;
    });

    Guid Achizitie(DateOnly data, decimal baza, string tva, Guid? produs = null) => CuSpatiu(os => {
        var d = os.CreateObject<FacturaIntrare>(); d.Data = data; d.Numar = Marcaj + "-FCT-" + ++numarFactura;
        d.PredatorId = Furnizor; d.PrimitorId = Magazie;
        var l = os.CreateObject<FacturaIntrareDetaliu>(); l.Document = d; l.Pozitie = 1;
        l.TipMaterialId = Tip(os, Serviciu); l.Cantitate = 1; l.PretUnitar = baza; l.TipTvaId = Tva(tva); l.ProdusId = produs;
        os.CommitChanges(); return d.ID;
    });
}
