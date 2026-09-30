using Atlas.Conta.BackOffice.Module.Api;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Cub.Citiri;
using Atlas.Conta.BackOffice.Module.Motor;
using Atlas.Conta.BackOffice.Module.Proiectii;
using Atlas.Conta.BackOffice.Module.Saft;
using System.Xml.Linq;
using DevExpress.ExpressApp;
using C = Atlas.Conta.BackOffice.Module.Cub;

namespace Atlas.Conta.BackOffice.ModelCheck;

// TR-D8 S1: catalogul docs/nucleu/scenarii/SAFT.md pe exportul L din cub (SaftPeCub).
sealed class ScenariiSaft(Func<IObjectSpace> deschide, Action<string, bool> check,
    bool privat, Action<IObjectSpace, int, int> inchide)
    : ScenaDocumente(deschide, check, privat, inchide, "SAFT", 2040) {
    protected override void CurataNomenclatoare(IObjectSpace os, Purja purja) =>
        purja.Adauga(os.GetObjectsQuery<DepunereDeclaratie>().Where(d => d.Perioada >= An * 100 && d.Perioada < (An + 1) * 100));

    static readonly System.Globalization.CultureInfo Inv = System.Globalization.CultureInfo.InvariantCulture;
    DateOnly Zi(int luna, int zi) => new(An, luna, zi);
    SaftDto Export(int luna) => CuSpatiu(os => SaftProiectii.SaftPeCub(os, An, luna, Zi(luna, 28)));

    static byte[] Xml(SaftDto d) {
        using var ms = new MemoryStream();
        SaftXml.Scrie(d, ms);
        return ms.ToArray();
    }

    static IEnumerable<SaftLinieTranzactie> Gl(SaftDto d, params Guid[] docs) =>
        d.Jurnale.SelectMany(j => j.Tranzactii).Where(t => docs.Contains(t.DocumentId)).SelectMany(t => t.Linii);
    static decimal Debit(IEnumerable<SaftLinieTranzactie> l, string cont = null) =>
        l.Where(x => x.DebitCreditIndicator == "D" && (cont == null || x.AccountID == cont)).Sum(x => x.Amount);
    static decimal Credit(IEnumerable<SaftLinieTranzactie> l, string cont = null) =>
        l.Where(x => x.DebitCreditIndicator == "C" && (cont == null || x.AccountID == cont)).Sum(x => x.Amount);
    static List<SaftFactura> Facturi(SaftDto d, params Guid[] docs) =>
        d.FacturiEmise.Concat(d.FacturiPrimite).Where(f => docs.Contains(f.DocumentId)).ToList();
    static decimal Taxa(SaftFactura f) => f.Linii.Sum(l => l.TaxInformation.TaxAmount);
    static bool Suma(SaftFactura f, string tip, decimal net, decimal taxa, decimal brut) =>
        f.InvoiceType == tip && f.NetTotal == net && Taxa(f) == taxa && f.GrossTotal == brut;
    static decimal? Sold(List<SaftTert> terti, Guid partener) =>
        terti.SingleOrDefault(t => t.PartenerId == partener) is { } t
            ? (t.ClosingDebitBalance ?? 0m) - (t.ClosingCreditBalance ?? 0m) : null;

    bool FaraRefuzuri(SaftDto d, string id) {
        foreach (var r in d.Refuzuri) Console.WriteLine($"     REFUZ {r.Cod}: {r.Mesaj}");
        Verifica(id, $"luna {d.Luna}: exportul pe cub fără refuzuri", d.Refuzuri.Count == 0);
        return d.Refuzuri.Count == 0;
    }

    void Perioada(int luna) => Comanda(os => {
        var p = os.CreateObject<PerioadaFiscala>(); p.An = An; p.Luna = luna; os.CommitChanges();
    });

    Guid NouPartener(string sufix) => CuSpatiu(os => {
        var p = os.CreateObject<Partener>(); p.Cod = Marcaj + "-" + sufix; p.Denumire = p.Cod;
        os.CommitChanges(); return p.ID;
    });

    FacturaScena Fct(Guid partener, DateOnly data, params LinieFctScena[] linii) {
        var f = Factura(data, linii);
        Comanda(os => { os.GetObjectByKey<FacturaIntrare>(f.Id).PredatorId = partener; os.CommitChanges(); });
        return f;
    }

    void Inregistrare(Guid doc, DateOnly data) => Comanda(os => {
        var f = os.GetObjectByKey<FacturaIntrare>(doc); f.DataInregistrare = data; f.DataPrimire = data; os.CommitChanges();
    });

    Guid Fcl(Guid client, decimal baza, string tva) => CuSpatiu(os => {
        var d = os.CreateObject<FacturaIesire>(); d.Data = Ianuarie; d.PredatorId = Loc; d.PrimitorId = client;
        var l = os.CreateObject<FacturaIesireDetaliu>(); l.Document = d; l.Pozitie = 1;
        l.TipMaterialId = Tip(os, "704"); l.Cantitate = 1; l.PretUnitar = baza; l.TipTvaId = Tva(tva);
        os.CommitChanges(); return d.ID;
    });

    Guid Trezorerie(bool incasare, Guid partener, decimal suma) {
        var doc = Trezorerie(incasare, suma).Id;
        Comanda(os => {
            var d = os.GetObjectByKey<Document>(doc);
            if (incasare) d.PredatorId = partener; else d.PrimitorId = partener;
            os.CommitChanges();
        });
        return doc;
    }

    Guid Corecteaza(Guid doc, DateOnly data, Action<DocumentDetaliu> linie) {
        var corectie = CuSpatiu(os => ComenziDocument.Sistem(os).Corecteaza(doc, data, MotivCorectie.EroareMateriala).CorectieId);
        Comanda(os => {
            linie(os.GetObjectsQuery<DocumentDetaliu>().Single(l => l.DocumentId == corectie));
            os.CommitChanges();
        });
        return corectie;
    }

    protected override void Executa() {
        if (!Privat) {
            var inert = Factura(Ianuarie, new LinieFctScena(1, 100, Stoc: false));
            Opereaza(inert.Id);
            Verifica("SC-SAFT-14", "bugetar: L neaplicabil, fără antet, GL sau facturi", CuSpatiu(os => {
                var d = SaftProiectii.SaftPeCub(os, An, 1);
                return d.Neaplicabil != null && d.Header == null && d.Jurnale.Count == 0 && d.FacturiPrimite.Count == 0;
            }));
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

    void Privat1() {
        Comutare();
        Timbre();
        foreach (var luna in new[] { 3, 4, 5 }) Perioada(luna);
        var fa = NouPartener("FA"); var fb = NouPartener("FB"); var ca = NouPartener("CA");

        var f01 = Fct(fa, Ianuarie, new LinieFctScena(1, 100, "N21", Stoc: false)); Opereaza(f01.Id);
        var p01 = Trezorerie(false, fa, 40); Opereaza(p01);
        var v02 = Fcl(ca, 200, "N21"); Opereaza(v02);
        var i02 = Trezorerie(true, ca, 100); Opereaza(i02);
        var f03 = Factura(Ianuarie, new LinieFctScena(1, 100, "NED21", Stoc: false)); Opereaza(f03.Id);
        var dvi = Dvi(); Opereaza(dvi);
        var f19 = Factura(Ianuarie, new LinieFctScena(1, 100, "N21", false), new LinieFctScena(1, 50, "N11", false));
        Opereaza(f19.Id);
        var f20a = Factura(Ianuarie, new LinieFctScena(1, .01m, "N21", false), new LinieFctScena(1, .01m, "N21", false));
        Opereaza(f20a.Id);
        var f20b = Factura(Ianuarie, new LinieFctScena(1, 100, "N21", false, 21.01m)); Opereaza(f20b.Id);
        var f21 = Factura(Ianuarie, new LinieFctScena(1, 100, "TI21", false)); Opereaza(f21.Id);
        var f22 = Factura(Ianuarie, new LinieFctScena(2, 50, "N21", false)); Opereaza(f22.Id);
        var f18 = Fct(fb, Zi(1, 8), new LinieFctScena(1, 100, "N21", false)); Inregistrare(f18.Id, Zi(1, 10)); Opereaza(f18.Id);
        var f23 = Factura(Zi(1, 8), new LinieFctScena(1, 100, "N21", false)); Inregistrare(f23.Id, Februarie); Opereaza(f23.Id);
        var anulata = Factura(Ianuarie, new LinieFctScena(1, 100, "N21", false)); Opereaza(anulata.Id); Anuleaza(anulata.Id);
        var stornoIan = Factura(Ianuarie, new LinieFctScena(1, 100, "N21", false)); Opereaza(stornoIan.Id);
        Storneaza(stornoIan.Id, Zi(1, 20));
        var stornoFeb = Factura(Ianuarie, new LinieFctScena(1, 100, "N21", false)); Opereaza(stornoFeb.Id);
        var receptie = Receptioneaza(new LinieFctScena(10, 10, "N21", Tip: "371"));
        var nir = CuSpatiu(os => os.GetObjectsQuery<Document>().Single(d => d.Autogenerat && d.DocumentSursaId == receptie.Id).ID);

        PlatiIanuarie();

        var ian = Export(1);
        FaraRefuzuri(ian, "SC-SAFT-01");
        Ianuarie1(ian, f01.Id, v02, f03.Id, dvi, fa, ca);
        Ianuarie2(ian, f19.Id, f20a.Id, f20b.Id, f21.Id, f22.Id, f18.Id, f23.Id, anulata.Id, stornoIan.Id);
        VerificaPlatiIanuarie(ian);
        AccesCitit();
        Cusaturi(ian, 1);
        UnitateIstorica(receptie.Linii[0].Produs!.Value, receptie.Id);
        var dtoIanuarie = Export(1);
        var artefactIanuarie = Xml(dtoIanuarie);

        Comanda(os => {
            foreach (var formular in new[] { FormularFiscal.D300, FormularFiscal.D394 })
                FiscalitateService.ConfirmaDepunerea(os, formular, An, 1,
                    Fiscale.Versiune(os, formular, Zi(1, 1), Zi(1, 31)), "ModelCheck");
        });
        InchideIanuarie();

        var c18 = CuSpatiu(os => ComenziDocument.Sistem(os).Corecteaza(f18.Id, Februarie, MotivCorectie.EroareMateriala).CorectieId);
        var draft = Export(2);
        Verifica("SC-SAFT-18", "înlocuitor Draft: refuz SAFT_CORECTIE_INCOMPLETA înaintea XML", draft.Refuzuri.Any(r =>
            r.Cod == SaftProiectii.RefuzCorectie && r.DocumentId == f18.Id) && Refuzat(() => Xml(draft)));
        Comanda(os => {
            var l = os.GetObjectsQuery<FacturaIntrareDetaliu>().Single(l => l.DocumentId == c18);
            l.PretUnitar = 80; l.ValoareTva = 16.80m; os.CommitChanges();
        });
        Opereaza(c18);
        var c22 = Corecteaza(f22.Id, Februarie, l => {
            var d = (FacturaIntrareDetaliu)l; d.Cantitate = 3; d.PretUnitar = 40; d.ValoareTva = 25.20m;
        });
        Opereaza(c22);
        Refuza("SC-SAFT-22", () => Comanda(os => {
            os.GetObjectsQuery<DocumentDetaliu>().Single(l => l.DocumentId == f22.Id).Cantitate = 5;
            GardianEditare.Verifica(os);
        }), "nu se mai modifică");
        Verifica("SC-SAFT-22", "linia operată refuzată rămâne cu cantitatea 2",
            CuSpatiu(os => os.GetObjectsQuery<DocumentDetaliu>().Single(l => l.DocumentId == f22.Id).Cantitate) == 2);
        Storneaza(stornoFeb.Id, Februarie);
        var retur = CuSpatiu(os => {
            var d = os.CreateObject<ReturFurnizor>(); d.Data = Februarie; d.DataPrimire = Februarie;
            d.PredatorId = Magazie; d.PrimitorId = Furnizor;
            var l = os.CreateObject<DocumentDetaliu>(); l.Document = d; l.Pozitie = 1; l.TipMaterialId = Tip(os, "371");
            l.Cantitate = 2; l.LotId = receptie.Linii[0].Lot; l.TipTvaId = Tva("N21");
            os.CommitChanges(); return d.ID;
        });
        Opereaza(retur);
        PlatiFebruarie();

        var feb = Export(2);
        FaraRefuzuri(feb, "SC-SAFT-18");
        Februarie1(feb, f18.Id, c18, f22.Id, c22, f23.Id, stornoFeb.Id, retur, receptie.Id);
        VerificaPlatiFebruarie(feb);
        Cusaturi(feb, 2);
        Verifica("SC-SAFT-18", "reexportul lunii închise e identic octet cu octet după corecția din februarie",
            Xml(Export(1)).AsSpan().SequenceEqual(artefactIanuarie));
        var artefactFebruarie = Xml(feb);

        var c18b = Corecteaza(c18, Zi(3, 5), l => { var d = (FacturaIntrareDetaliu)l; d.PretUnitar = 70; d.ValoareTva = 14.70m; });
        Opereaza(c18b);
        PlatiMartie();
        var mar = Export(3);
        FaraRefuzuri(mar, "SC-SAFT-18");
        var martie = Facturi(mar, c18, c18b);
        Verifica("SC-SAFT-18", "martie: 381 −80/−16,80/−96,80 și 384 70/14,70/84,70; GL net −12,10; furnizor 84,70",
            martie.Count == 2 && Suma(martie.Single(f => f.DocumentId == c18), "381", -80, -16.80m, -96.80m)
            && Suma(martie.Single(f => f.DocumentId == c18b), "384", 70, 14.70m, 84.70m)
            && Debit(Gl(mar, c18, c18b)) == -12.10m && Credit(Gl(mar, c18, c18b)) == -12.10m
            && Sold(mar.Furnizori, fb) == -84.70m);
        Verifica("SC-SAFT-18", "februarie reexportat după corecția din martie rămâne identic",
            Xml(Export(2)).AsSpan().SequenceEqual(artefactFebruarie));
        VerificaPlatiMartie(mar);

        Certificare((dtoIanuarie, artefactIanuarie), (feb, artefactFebruarie), (mar, Xml(mar)));

        RepeatableRead();
        RepeatableReadPlati();
        NominalizareDesfacuta();
        MapareLipsa();
    }

    void Certificare((SaftDto Dto, byte[] Xml) ian, (SaftDto Dto, byte[] Xml) feb, (SaftDto Dto, byte[] Xml) mar) {
        var (ianuarie, februarie, martie) = (ian.Xml, feb.Xml, mar.Xml);
        var director = Path.Combine(Duk.DirectorTemporar(), $"s0-{DateTime.UtcNow:yyyyMMdd-HHmmss}");
        Directory.CreateDirectory(director);
        var validari = new List<ValidareD406>();
        string Scrie(string nume, byte[] continut) {
            var cale = Path.Combine(director, nume + ".xml");
            File.WriteAllBytes(cale, continut);
            return cale;
        }
        ValidareD406 Valideaza(string nume, byte[] continut, SaftDto dto = null) {
            var v = ValidareD406.Ruleaza(Scrie(nume, continut), dto);
            validari.Add(v);
            Console.WriteLine($"     MĂSURAT (S0 {nume}, perioada din antet {v.An}-{v.Luna:00}): {v.Rezumat}");
            foreach (var e in v.EroriXsd.Take(10)) Console.WriteLine($"         EROARE XSD: {e}");
            foreach (var e in v.Duk.Erori.Take(10)) Console.WriteLine($"         EROARE DUK: {e}");
            foreach (var a in v.Duk.Avertismente.Take(10)) Console.WriteLine($"         ATENȚIONARE DUK: {a}");
            return v;
        }
        static byte[] Muta(byte[] xml, Action<XDocument> mutatie) {
            var doc = XDocument.Load(new MemoryStream(xml));
            mutatie(doc);
            using var ms = new MemoryStream();
            doc.Save(ms);
            return ms.ToArray();
        }
        XNamespace ns = SaftXml.SpatiuNume;

        Verifica("SC-SAFT-24", "luna 4 fără rulaj: GL, facturi și plăți goale trec XSD și DUK",
            Export(4) is { Jurnale.Count: 0, FacturiPrimite.Count: 0, FacturiEmise.Count: 0, Plati.Count: 0 } gol
            && Valideaza("fara-rulaj-cub", Xml(gol)).Valid);

        var luni = new[] { (1, ian), (2, feb), (3, mar) }
            .Select(x => (Luna: x.Item1, V: Valideaza($"saft-L-{An}-{x.Item1:00}", x.Item2.Xml, x.Item2.Dto))).ToList();
        foreach (var (luna, v) in luni)
            Verifica("SC-SAFT-24", $"luna {luna}: XML L acceptat de XSD v249 (d406) și de DUK {ManifestD406.VersiuneValidator} pe perioada din antet",
                v.Valid && (v.An, v.Luna) == (An, luna));
        Verifica("SC-SAFT-24", "februarie și martie fără vânzări: SalesInvoices gol, fără totaluri zero",
            new[] { februarie, martie }.All(x => XDocument.Load(new MemoryStream(x)).Descendants(ns + "SalesInvoices").Single() is { IsEmpty: true }));

        var facturiFeb = XDocument.Load(new MemoryStream(februarie)).Descendants(ns + "PurchaseInvoices").Elements(ns + "Invoice")
            .Select(f => (No: (string)f.Element(ns + "InvoiceNo"), Tip: (string)f.Element(ns + "InvoiceType"))).ToList();
        Verifica("SC-SAFT-24", "februarie: 381 și 384 cu același InvoiceNo în același fișier, acceptate de DUK",
            luni[1].V.Valid && facturiFeb.Where(f => f.Tip == "384")
                .Any(r => facturiFeb.Any(i => i.Tip == "381" && i.No == r.No)));
        var tranzactieIan = XDocument.Load(new MemoryStream(ianuarie)).Descendants(ns + "Transaction").ToList();
        Verifica("SC-SAFT-24", "ianuarie: SystemEntryDate din afara perioadei (timbrul real) acceptat, GLPostingDate = TransactionDate în perioadă",
            luni[0].V.Valid
            && tranzactieIan.Any(t => DateOnly.Parse((string)t.Element(ns + "SystemEntryDate")!).Year != An)
            && tranzactieIan.All(t => (string)t.Element(ns + "GLPostingDate") == (string)t.Element(ns + "TransactionDate")));

        static bool Obligatoriu(params string[] cale) => XsdD406.Element(cale) is { MinOccurs: 1 };
        static bool Optional(params string[] cale) => XsdD406.Element(cale) is { MinOccurs: 0 };
        string[] tranzactie = ["AuditFile", "GeneralLedgerEntries", "Journal", "Transaction"];
        Verifica("SC-SAFT-24", "schema fixată: Transaction cere SystemEntryDate și GLPostingDate; Invoice.GLPostingDate și PaymentLine.SourceDocumentID sunt opționale",
            Obligatoriu([.. tranzactie, "SystemEntryDate"]) && Obligatoriu([.. tranzactie, "GLPostingDate"])
            && Optional("AuditFile", "SourceDocuments", "PurchaseInvoices", "Invoice", "GLPostingDate")
            && Optional("AuditFile", "SourceDocuments", "Payments", "Payment", "PaymentLine", "SourceDocumentID"));

        byte[] CuAntet(int an, int luna) => Muta(ianuarie, d => {
            foreach (var (camp, valoare) in new[] { ("PeriodStartYear", an), ("PeriodEndYear", an), ("PeriodStart", luna), ("PeriodEnd", luna) })
                d.Descendants(ns + camp).Single().Value = valoare.ToString();
        });
        var santinela = Valideaza("santinela-antet-2025-09", CuAntet(2025, 9));
        Verifica("SC-SAFT-24", "santinelă: ianuarie validat pe nomenclatorul lunii reale 2025-09 (regimul 21%) trece ca pe 2040",
            santinela.Duk is { Valid: true, Perioada: (2025, 9) } && santinela.EroriXsd.Count == 0);
        var perioada = Valideaza("mutant-antet-2024", CuAntet(2024, 1));
        Verifica("SC-SAFT-25", "mutant: același fișier cu antetul pe 2024 e validat pe nomenclatorul 2024, iar codurile cotei 21% sunt respinse",
            perioada.Duk is { Disponibil: true, Valid: false, Perioada: (2024, 1) }
            && perioada.Duk.Erori.Any(e => e.Contains("nu se afla in lista", StringComparison.Ordinal)));
        var faraData = Valideaza("mutant-fara-glpostingdate", Muta(ianuarie, d =>
            d.Descendants(ns + "Transaction").First().Element(ns + "GLPostingDate")!.Remove()));
        Verifica("SC-SAFT-25", "mutant: tranzacția fără GLPostingDate este respinsă de XSD (schema nu e vidă)",
            faraData.EroriXsd.Count > 0);
        var mixt = Scrie("mutant-copil-fara-namespace", Muta(ianuarie, d =>
            d.Descendants(ns + "Transaction").First().Element(ns + "GLPostingDate")!.Name = "GLPostingDate"));
        Verifica("SC-SAFT-25", "mutant: un copil obligatoriu fără namespace este respins de XSD", XsdD406.Valideaza(mixt).Count > 0);
        var test = Valideaza("mutant-namespace-d406t", Muta(ianuarie, d => {
            d.Root!.Attributes().Where(a => a.IsNamespaceDeclaration).Remove();
            foreach (var e in d.Descendants()) e.Name = XNamespace.Get(ManifestD406.SpatiuXsdPublicat) + e.Name.LocalName;
        }));
        Verifica("SC-SAFT-25", "mutant: fișierul cu namespace-ul de test d406t este respins de XSD-ul derivat și de DUK D406",
            test.EroriXsd.Count > 0 && test.Duk is { Disponibil: true, Valid: false });
        var captura = Valideaza("masura-glpostingdate-captura", Muta(ianuarie, d => {
            foreach (var t in d.Descendants(ns + "Transaction"))
                t.Element(ns + "GLPostingDate")!.Value = (string)t.Element(ns + "SystemEntryDate")!;
        }));
        Console.WriteLine($"     MĂSURAT (S1-R3 la S0): GLPostingDate = data capturării, în afara perioadei → {captura.Rezumat}");

        var manifest = ValidareD406.ScrieManifest(director, validari);
        Console.WriteLine($"     MANIFEST S0: {manifest}");
        foreach (var (luna, v) in luni) {
            var continut = new[] { ianuarie, februarie, martie }[luna - 1];
            var p = ValidareD406.CitesteProvenienta(manifest, v.Fisier);
            Console.WriteLine($"     MĂSURAT (S0 proveniență luna {luna}): {p.Gl.Count} linii GL, {p.Facturi.Count} facturi, {p.Plati.Count} linii de plată");
            Verifica("SC-SAFT-24", $"luna {luna}: manifestul leagă fiecare (TransactionID, RecordID) de postarea lui exactă (cont, latură, sumă, ordinea Spatiu/ID) și fiecare factură de tranzacția reală (document, storno)",
                Provenienta(p, continut));
            if (luna == 1)
            {
                var (a, b) = (p.Gl[0], p.Gl[1]);
                List<LegaturaGl> permutate = [a with { Spatiu = b.Spatiu, PostareId = b.PostareId }, b with { Spatiu = a.Spatiu, PostareId = a.PostareId }, .. p.Gl.Skip(2)];
                var f0 = p.Facturi[0];
                Verifica("SC-SAFT-25", "mutanți de proveniență: linie GL omisă, cheie de postare dublată, surse permutate între RecordID 1 și 2, "
                    + "factură omisă, DocumentId schimbat și Storno inversat sunt respinse",
                    a.TransactionID == b.TransactionID
                    && !Provenienta(p with { Gl = p.Gl.Skip(1).ToList() }, continut)
                    && !Provenienta(p with { Gl = [.. p.Gl.Take(p.Gl.Count - 1), p.Gl[^1] with { PostareId = p.Gl[0].PostareId, Spatiu = p.Gl[0].Spatiu }] }, continut)
                    && !Provenienta(p with { Gl = permutate }, continut)
                    && !Provenienta(p with { Facturi = p.Facturi.Skip(1).ToList() }, continut)
                    && !Provenienta(p with { Facturi = [f0 with { DocumentId = Guid.NewGuid() }, .. p.Facturi.Skip(1)] }, continut)
                    && !Provenienta(p with { Facturi = [f0 with { Storno = !f0.Storno }, .. p.Facturi.Skip(1)] }, continut));
                var cuTinta = p.Plati.First(l => l.TintaDocumentId != null);
                var alta = p.Plati.First(l => l.TransactionID != cuTinta.TransactionID);
                List<LegaturaPlata> Inlocuieste(LegaturaPlata nou) => [.. p.Plati.Select(l => l == cuTinta ? nou : l)];
                Verifica("SC-SAFT-34", "mutanți de proveniență pe plăți: linie omisă, surse luate de la altă plată și ținta schimbată sunt respinse",
                    p.Plati.Count > 1
                    && !Provenienta(p with { Plati = p.Plati.Skip(1).ToList() }, continut)
                    && !Provenienta(p with { Plati = Inlocuieste(cuTinta with { Surse = alta.Surse }) }, continut)
                    && !Provenienta(p with { Plati = Inlocuieste(cuTinta with { TintaDocumentId = Guid.NewGuid() }) }, continut));
            }
        }
        Verifica("SC-SAFT-24", "manifestul rulării fixează XSD, schema derivată, kitul, nomenclatorul și SHA-256 al fiecărui fișier",
            File.Exists(manifest) && validari.All(v => v.Sha256.Length == 64));
    }

    bool Provenienta(ProvenientaD406 p, byte[] xml) {
        XNamespace ns = SaftXml.SpatiuNume;
        var doc = XDocument.Load(new MemoryStream(xml));
        var liniiXml = doc.Descendants(ns + "Transaction").SelectMany(t => t.Elements(ns + "TransactionLine").Select(l => {
            var suma = l.Element(ns + "DebitAmount") ?? l.Element(ns + "CreditAmount");
            return (Cheie: ((string)t.Element(ns + "TransactionID"), (string)l.Element(ns + "RecordID")),
                Cont: (string)l.Element(ns + "AccountID"), Latura: suma?.Name.LocalName == "DebitAmount" ? "D" : "C",
                Suma: decimal.Parse((string)suma?.Element(ns + "Amount") ?? "", Inv));
        })).ToList();
        var facturiXml = new[] { "SalesInvoices", "PurchaseInvoices" }.SelectMany(s => doc.Descendants(ns + s).Elements(ns + "Invoice")
            .Select(f => (s, (string)f.Element(ns + "InvoiceNo"), (string)f.Element(ns + "InvoiceType"), (string)f.Element(ns + "TransactionID"))))
            .OrderBy(x => x).ToList();
        var ids = p.Gl.Select(l => l.PostareId).ToList();
        var tranzactii = p.Facturi.Select(f => Guid.Parse(f.TransactionID)).ToList();
        var (cub, simboluri, evenimente) = CuSpatiu(os => (
            os.GetObjectsQuery<C.Postare>().Where(x => ids.Contains(x.ID))
                .Select(x => new { x.Spatiu, x.ID, x.TranzactieId, x.DocumentId, x.LinieId, x.Cont, x.Latura, x.Valoare }).ToList(),
            os.GetObjectsQuery<Cont>().ToDictionary(c => c.ID, c => c.Simbol),
            os.GetObjectsQuery<C.Tranzactie>().Where(t => tranzactii.Contains(t.ID))
                .Select(t => new { t.ID, t.DocumentId, t.Fel }).ToList().ToDictionary(t => t.ID.ToString())));
        var dupaCheie = liniiXml.GroupBy(l => l.Cheie).ToDictionary(g => g.Key, g => g.First());
        bool Sursa(LegaturaGl l) => dupaCheie.TryGetValue((l.TransactionID, l.RecordID), out var x)
            && cub.SingleOrDefault(c => c.Spatiu == l.Spatiu && c.ID == l.PostareId) is { } c
            && c.TranzactieId.ToString() == l.TransactionID && (c.DocumentId ?? Guid.Empty) == l.DocumentId && c.LinieId == l.LinieId
            && simboluri.GetValueOrDefault(c.Cont) == x.Cont && (c.Latura == Atlas.Conta.Nucleu.Latura.Debit ? "D" : "C") == x.Latura
            && c.Valoare == x.Suma;
        bool Ordonata(IEnumerable<LegaturaGl> tranzactie) {
            var surse = tranzactie.OrderBy(l => int.Parse(l.RecordID, Inv)).Select(l => (l.Spatiu, l.PostareId)).ToList();
            return surse.Zip(surse.Skip(1)).All(x => x.First.Spatiu < x.Second.Spatiu
                || x.First.Spatiu == x.Second.Spatiu && x.First.PostareId.CompareTo(x.Second.PostareId) < 0);
        }
        return p.Gl.Count > 0 && p.Gl.Count == liniiXml.Count
            && p.Gl.Select(l => (l.TransactionID, l.RecordID)).ToHashSet().SetEquals(liniiXml.Select(l => l.Cheie))
            && dupaCheie.Count == liniiXml.Count
            && p.Gl.All(l => l.Spatiu != null) && p.Gl.Select(l => (l.Spatiu, l.PostareId)).Distinct().Count() == p.Gl.Count
            && p.Gl.All(Sursa) && p.Gl.GroupBy(l => l.TransactionID).All(Ordonata)
            && p.Facturi.Select(f => (f.Sectiune, f.InvoiceNo, f.InvoiceType, f.TransactionID)).OrderBy(x => x).SequenceEqual(facturiXml)
            && p.Facturi.All(f => evenimente.TryGetValue(f.TransactionID, out var t) && t.DocumentId == f.DocumentId
                && (t.Fel == Atlas.Conta.Nucleu.FelTranzactie.Storno) == f.Storno)
            && ProvenientaPlati(p, doc);
    }

    bool ProvenientaPlati(ProvenientaD406 p, XDocument doc) {
        XNamespace ns = SaftXml.SpatiuNume;
        const Atlas.Conta.Nucleu.FelTranzactie Transfer = Atlas.Conta.Nucleu.FelTranzactie.Transfer;
        var liniiXml = doc.Descendants(ns + "Payment").SelectMany(x => x.Elements(ns + "PaymentLine").Select(l => (
            Cheie: ((string)x.Element(ns + "TransactionID"), int.Parse((string)l.Element(ns + "LineNumber"), Inv)),
            Cont: (string)l.Element(ns + "AccountID"), Sursa: (string)l.Element(ns + "SourceDocumentID"),
            Brut: decimal.Parse((string)x.Element(ns + "PaymentDocumentTotals")!.Element(ns + "GrossTotal"), Inv)))).ToList();
        var plati = p.Plati ?? [];
        if (plati.Count != liniiXml.Count || liniiXml.Select(l => l.Cheie).Distinct().Count() != liniiXml.Count
                || !plati.Select(l => (l.TransactionID, l.LineNumber)).ToHashSet().SetEquals(liniiXml.Select(l => l.Cheie)))
            return false;
        var ids = plati.SelectMany(l => l.Surse).Select(x => x.Id).Distinct().ToList();
        var tranzactii = plati.Select(l => Guid.Parse(l.TransactionID)).Distinct().ToList();
        var tinte = plati.Select(l => l.TintaDocumentId).OfType<Guid>().Distinct().ToList();
        var (cub, simboluri, evenimente, numereTinte) = CuSpatiu(os => (
            os.GetObjectsQuery<C.Postare>().Where(x => ids.Contains(x.ID))
                .Select(x => new { x.Spatiu, x.ID, x.TranzactieId, x.Tranzactie.Fel, x.DocumentId, x.Cont, x.Valoare }).ToList(),
            os.GetObjectsQuery<Cont>().ToDictionary(c => c.ID, c => c.Simbol),
            os.GetObjectsQuery<C.Tranzactie>().Where(t => tranzactii.Contains(t.ID))
                .Select(t => new { t.ID, t.DocumentId, t.Fel }).ToList().ToDictionary(t => t.ID.ToString()),
            os.GetObjectsQuery<Document>().Where(d => tinte.Contains(d.ID)).ToDictionary(d => d.ID, d => d.Numar)));
        var dupaCheie = liniiXml.ToDictionary(l => l.Cheie);
        bool Linie(LegaturaPlata l) {
            var x = dupaCheie[(l.TransactionID, l.LineNumber)];
            return evenimente.TryGetValue(l.TransactionID, out var t) && t.DocumentId == l.DocumentId
                && (t.Fel == Atlas.Conta.Nucleu.FelTranzactie.Storno) == l.Storno
                && x.Sursa == (l.TintaDocumentId is Guid tinta ? numereTinte.GetValueOrDefault(tinta) : null)
                && l.Surse.Count > 0 && l.Surse.All(s => cub.SingleOrDefault(c => c.Spatiu == s.Spatiu && c.ID == s.Id) is { } c
                    && simboluri.GetValueOrDefault(c.Cont) == x.Cont
                    && (c.Fel == Transfer ? c.DocumentId != null : c.TranzactieId.ToString() == l.TransactionID));
        }
        return plati.All(Linie) && plati.GroupBy(l => l.TransactionID).All(g =>
            g.SelectMany(l => l.Surse).Select(x => (x.Spatiu, x.Id)).Distinct()
                .Select(x => cub.Single(c => c.Spatiu == x.Spatiu && c.ID == x.Id)).Where(c => c.Fel != Transfer).Sum(c => c.Valoare)
            == liniiXml.First(l => l.Cheie.Item1 == g.Key).Brut);
    }

    Guid fp, fP1, fP2, fP3, fP4, fP5, fP6, p05, p05b, p06, p26, p26b, p27, p27b, link27b, i28, v28, vir, pAnaf, pAng, p31, ntc31, p32, c32, p33;
    readonly Dictionary<Guid, string> numere = [];

    Guid Trz(bool incasare, Guid partener, decimal[] sume, Action<IObjectSpace, DocumentTrezorerie> ajusteaza = null) {
        var doc = Trezorerie(incasare, sume).Id;
        Comanda(os => {
            var d = os.GetObjectByKey<DocumentTrezorerie>(doc);
            if (incasare) d.PredatorId = partener; else d.PrimitorId = partener;
            d.DataInregistrare = d.Data;
            ajusteaza?.Invoke(os, d);
            os.CommitChanges();
        });
        Opereaza(doc);
        return doc;
    }

    Guid FctPlata(Guid partener) { var f = Fct(partener, Ianuarie, new LinieFctScena(1, 100, Stoc: false)).Id; Opereaza(f); return f; }

    string Nr(Guid doc) {
        if (!numere.TryGetValue(doc, out var n))
            numere[doc] = n = CuSpatiu(os => os.GetObjectByKey<Document>(doc).Numar);
        return n;
    }

    static string FormaPlata(SaftPlata p) => $"{p.GrossTotal.ToString("0.00", Inv)} " + string.Join("|", p.Linii
        .Select(l => $"{l.DebitCreditIndicator}{l.AccountID} {l.PaymentLineAmount.ToString("0.00", Inv)} {l.SourceDocumentID ?? "-"}")
        .Order(StringComparer.Ordinal));

    static List<SaftPlata> Plati(SaftDto d, Guid doc) => d.Plati.Where(p => p.DocumentId == doc).ToList();

    bool Linii(SaftDto d, Guid doc, bool storno, params (decimal Suma, Guid? Tinta)[] asteptate) {
        var p = Plati(d, doc).Where(x => x.Storno == storno).ToList();
        return p.Count == 1 && p[0].Linii.Count == asteptate.Length
            && p[0].Linii.Zip(asteptate).All(x => x.First.PaymentLineAmount == x.Second.Suma
                && x.First.TintaDocumentId == x.Second.Tinta
                && x.First.SourceDocumentID == (x.Second.Tinta is Guid t ? Nr(t) : null))
            && p[0].GrossTotal == asteptate.Sum(a => a.Suma);
    }

    void PlatiIanuarie() {
        fp = NouPartener("FP");
        (fP1, fP2, fP3, fP4, fP5, fP6) = (FctPlata(fp), FctPlata(fp), FctPlata(fp), FctPlata(fp), FctPlata(fp), FctPlata(fp));
        p05 = Trz(false, fp, [70]); Imperecheaza(p05, fP1, 50, Zi(1, 20));
        p05b = Trz(false, fp, [60]);
        var legatura = Imperecheaza(p05b, fP2, 60, Zi(1, 20));
        Comanda(os => ImperechereService.Desfa(os, legatura, Zi(1, 25)));
        p06 = Trz(false, fp, [70]);
        p26 = Trz(false, fp, [50], (os, d) => { d.Autogenerat = true; d.DocumentSursaId = fP3; });
        p26b = Trz(false, fp, [120], (os, d) => { d.Autogenerat = true; d.DocumentSursaId = fP4; });
        p27 = Trz(false, fp, [70]); Imperecheaza(p27, fP5, 50, Zi(1, 20));
        p27b = Trz(false, fp, [70]);
        var cb = NouPartener("CB");
        v28 = Fcl(cb, 100, "N21"); Opereaza(v28);
        i28 = Trz(true, cb, [100], (os, d) => d.TipInstrument = TipInstrumentPlata.DispozitieCasa);
        Imperecheaza(i28, v28, 100, Zi(1, 20));
        vir = CuSpatiu(os => {
            var d = os.CreateObject<Plata>(); d.Data = Ianuarie; d.DataInregistrare = Ianuarie;
            var banca = os.CreateObject<ContPropriu>(); banca.Cod = Marcaj + "-BANCA"; banca.Denumire = banca.Cod;
            banca.EsteBanca = true; banca.ContImplicitId = Cont("5121");
            d.Predator = banca;
            d.PrimitorId = os.GetObjectsQuery<ContPropriu>().Single(c => c.Cod == "CASA").ID;
            var l = os.CreateObject<DocumentTrezorerieDetaliu>(); l.Document = d; l.Pozitie = 1;
            l.TipMaterialId = Tip(os, "VIR"); l.Valoare = 500;
            os.CommitChanges(); return d.ID;
        });
        Opereaza(vir);
        var anaf = CuSpatiu(os => {
            var p = os.CreateObject<Partener>(); p.Cod = Marcaj + "-ANAF"; p.Denumire = p.Cod; p.ContImplicitId = Cont("4423");
            os.CommitChanges(); return p.ID;
        });
        pAnaf = Trz(false, anaf, [300]);
        var angajat = CuSpatiu(os => {
            var a = os.CreateObject<Angajat>(); a.Cod = Marcaj + "-ANG"; a.Denumire = a.Cod; os.CommitChanges(); return a.ID;
        });
        pAng = Trz(false, angajat, [200]);
        var fq = NouPartener("FQ");
        ntc31 = Nota(Ianuarie, new LinieNtcScena(Serviciu, ContFurnizor, 20, RepartitorCredit: fq)).Id;
        Opereaza(ntc31);
        p31 = Trz(false, fq, [20]);
        Imperecheaza(ntc31, p31, 20, Zi(1, 20));
        p32 = Trz(false, fp, [100]);
        p33 = Trz(false, fp, [40, 60], (os, d) => {
            var proiect = os.CreateObject<Proiect>(); proiect.Cod = Marcaj + "-PR"; proiect.Denumire = proiect.Cod;
            os.GetObjectsQuery<DocumentTrezorerieDetaliu>().Single(l => l.DocumentId == d.ID && l.Pozitie == 1).Proiect = proiect;
        });
    }

    void VerificaPlatiIanuarie(SaftDto d) {
        var societate = CuSpatiu(os => SaftReguli.IdSocietate(os.GetObjectsQuery<Societate>().First()));
        var tranzactii = d.Jurnale.SelectMany(j => j.Tranzactii).ToList();
        var p = Plati(d, p05).SingleOrDefault();
        Verifica("SC-SAFT-05", "PLT 70 + legătură 50: D 401 50 cu SourceDocumentID F și D 401 20 fără referință, brut 70, tranzacția GL a plății",
            Linii(d, p05, false, (50, fP1), (20, null))
            && p!.Linii.All(l => l is { DebitCreditIndicator: "D", AccountID: "401" } && l.CustomerID == societate && l.SupplierID != societate)
            && tranzactii.Any(t => t.TransactionID == p.TransactionID && t.DocumentId == p05));
        Verifica("SC-SAFT-05", "legătura desfăcută în aceeași lună: plata rămâne 60 rest", Linii(d, p05b, false, (60, null)));
        Verifica("SC-SAFT-06", "ianuarie, înaintea legăturii din februarie: 70 rest", Linii(d, p06, false, (70, null)));
        Verifica("SC-SAFT-26", "nominalizată la operare: 50 pe G o singură dată; 120 pe G de rest 100: 100 G + 20 rest",
            Linii(d, p26, false, (50, fP3)) && Linii(d, p26b, false, (100, fP4), (20, null)));
        Verifica("SC-SAFT-27", "ianuarie: 50 F + 20 rest; a doua plată 70 rest",
            Linii(d, p27, false, (50, fP5), (20, null)) && Linii(d, p27b, false, (70, null)));
        var i = Plati(d, i28).SingleOrDefault();
        Verifica("SC-SAFT-28", "INC 100 legată de FCL: C 4111 100 cu SourceDocumentID FCL, client pe CustomerID, dispoziție de casă 01/10",
            Linii(d, i28, false, (100, v28)) && i!.Linii.Single() is { DebitCreditIndicator: "C", AccountID: "4111" } li
            && li.SupplierID == societate && li.CustomerID != societate && (i.PaymentMethod, i.PaymentMechanism) == ("01", "10"));
        Verifica("SC-SAFT-29", "virament intern: în GL, fără plată și fără Neincluse", Plati(d, vir).Count == 0
            && tranzactii.Any(t => t.DocumentId == vir) && d.Neincluse.All(n => n.DocumentId != vir));
        Verifica("SC-SAFT-29", "PLT 300 pe 4423 către partener: Neincluse ContFaraRol cu 300, avertismentul PlataFaraContTert, GL păstrat",
            Plati(d, pAnaf).Count == 0 && tranzactii.Any(t => t.DocumentId == pAnaf)
            && d.Neincluse.SingleOrDefault(n => n.DocumentId == pAnaf) is { Cauza: nameof(CauzaNeincludere.ContFaraRol), Sectiune: "Payments", Debit: 300 }
            && d.Avertismente.Any(a => a.Cod == nameof(CodAvertismentSaft.PlataFaraContTert)));
        Verifica("SC-SAFT-29", "PLT 200 angajatului: codul societății pe ambele identificatoare, avertismentul PlataCatreAngajat",
            Linii(d, pAng, false, (200, null)) && Plati(d, pAng).Single().Linii.All(l => l.CustomerID == societate && l.SupplierID == societate)
            && d.Avertismente.Any(a => a.Cod == nameof(CodAvertismentSaft.PlataCatreAngajat)));
        Verifica("SC-SAFT-31", "avansul 20 stins de NTC: linia are ținta și numărul NTC; NTC nu devine plată",
            Linii(d, p31, false, (20, ntc31)) && Plati(d, ntc31).Count == 0);
        Verifica("SC-SAFT-33", "PLT 40 + 60 pe aceeași partidă: o linie 100, analiza omisă, avertismentul PlataAnalizaMixta",
            Linii(d, p33, false, (100, null)) && Plati(d, p33).Single().Linii.Single().Analiza.Count == 0
            && d.Avertismente.Any(a => a.Cod == nameof(CodAvertismentSaft.PlataAnalizaMixta)));
        Verifica("SC-SAFT-15", "ianuarie: fiecare plată are tranzacția GL a evenimentului, iar Σ liniilor = contrapartida din GL",
            d.Plati.Count > 0 && d.Plati.All(x => tranzactii.SingleOrDefault(t => t.TransactionID == x.TransactionID) is { } t
                && x.Linii.GroupBy(l => (l.AccountID, l.DebitCreditIndicator)).All(g => g.Sum(l => l.PaymentLineAmount)
                    == t.Linii.Where(l => l.AccountID == g.Key.AccountID && l.DebitCreditIndicator == g.Key.DebitCreditIndicator).Sum(l => l.Amount))));
    }

    void AccesCitit() {
        var comenzi = CapturaSql.Comenzi(() => Export(1));
        var citite = CapturaSql.Tabele(comenzi);
        var verificate = CuSpatiu(os => SaftAcces.Tabele(os));
        Console.WriteLine($"     MĂSURAT (SC-SAFT-36): {comenzi.Count} comenzi SQL; tabele citite [{string.Join(", ", citite.Order())}]; "
            + $"neverificate [{string.Join(", ", citite.Except(verificate).Order())}]; necitite [{string.Join(", ", verificate.Except(citite).Order())}]");
        Verifica("SC-SAFT-36", "SAFT_ACCES_INCOMPLET verifică exact tabelele citite de exportul L pe cub", citite.SetEquals(verificate));
    }

    void PlatiFebruarie() {
        Imperecheaza(p06, fP2, 50, Februarie);
        Storneaza(p27, Februarie);
        link27b = Imperecheaza(p27b, fP6, 50, Februarie);
        c32 = Corecteaza(p32, Februarie, l => l.Valoare = 80);
        Opereaza(c32);
    }

    void VerificaPlatiFebruarie(SaftDto d) {
        var s = Plati(d, p27).SingleOrDefault();
        var gl = Gl(d, p27).ToList();
        Verifica("SC-SAFT-27", "storno în februarie: −50 F și −20 rest (liniile declarate în ianuarie, negate), tranzacția stornoului; GL D 401 −70 / C 5311 −70",
            Linii(d, p27, true, (-50, fP5), (-20, null)) && s!.Storno
            && d.Jurnale.SelectMany(j => j.Tranzactii).Single(t => t.TransactionID == s.TransactionID).DocumentId == p27
            && Debit(gl, "401") == -70 && Credit(gl, "5311") == -70);
        Verifica("SC-SAFT-06", "februarie: legătura nu e plată (p06, p27b absente) și nici rulaj GL",
            Plati(d, p06).Count == 0 && Plati(d, p27b).Count == 0 && !Gl(d, p06).Any() && !Gl(d, p27b).Any());
        Verifica("SC-SAFT-32", "corecția plății: stornoul −100 rest și plata nouă 80 rest",
            Linii(d, p32, true, (-100, null)) && Linii(d, c32, false, (80, null)));
    }

    void PlatiMartie() {
        Comanda(os => ImperechereService.Sterge(os, link27b));
        Storneaza(p27b, Zi(3, 10));
    }

    void VerificaPlatiMartie(SaftDto d) {
        Verifica("SC-SAFT-27", "legătura din februarie ștearsă, storno în martie: −70 rest",
            Linii(d, p27b, true, (-70, null)) && Plati(d, p27b).Count == 1);
        var masuri = new[] { 1, 2, 3 }.Select(luna => {
            SaftDto dto = null;
            var comenzi = CapturaSql.Comenzi(() => dto = Export(luna)).Count;
            return (Luna: luna, Comenzi: comenzi, Plati: dto.Plati.Count, Facturi: dto.FacturiEmise.Count + dto.FacturiPrimite.Count);
        }).ToList();
        Console.WriteLine("     MĂSURAT (S2-D6 perf): " + string.Join("; ", masuri.Select(m => $"luna {m.Luna}: {m.Plati} plăți, {m.Facturi} facturi, {m.Comenzi} comenzi SQL")));
        Verifica("SC-SAFT-34", "exportul L nu face o interogare per plată sau factură: numărul de comenzi SQL nu crește cu volumul lunii",
            masuri[0].Plati > masuri[2].Plati * 5 && masuri.Max(m => m.Comenzi) - masuri.Min(m => m.Comenzi) <= 2);
    }

    void NominalizareDesfacuta() {
        var fr = NouPartener("FR");
        Guid Factura() { var f = Fct(fr, Zi(4, 6), new LinieFctScena(1, 100, Stoc: false)).Id; Opereaza(f); return f; }
        Guid Automata(decimal suma, Guid sursa) => Trz(false, fr, [suma], (os, d) => {
            d.Data = Zi(4, 7); d.DataInregistrare = d.Data; d.Autogenerat = true; d.DocumentSursaId = sursa;
        });
        void Desface(Guid plata) {
            var legatura = CuSpatiu(os => os.GetObjectsQuery<Imperechere>().Single(i => i.DocumentStingatorId == plata && i.Autogenerat).ID);
            Comanda(os => ImperechereService.Desfa(os, legatura, Zi(4, 10)));
        }
        var (g1, g2, g3, g4) = (Factura(), Factura(), Factura(), Factura());
        var integrala = Automata(50, g1); Desface(integrala);
        var excedent = Automata(120, g2); Desface(excedent);
        var realocata = Automata(50, g3); Desface(realocata);
        Imperecheaza(realocata, g4, 30, Zi(4, 15));
        var aprilie = Export(4);
        Verifica("SC-SAFT-37", "nominalizarea automată desfăcută în lună: 50 integral → 50 rest; 120 pe G de rest 100 → 120 rest; realocată 30 pe altă factură → 30 G4 + 20 rest",
            aprilie.Refuzuri.Count == 0 && Linii(aprilie, integrala, false, (50, null))
            && Linii(aprilie, excedent, false, (120, null)) && Linii(aprilie, realocata, false, (30, g4), (20, null)));
        Verifica("SC-SAFT-37", "aprilie: proveniența fiecărei linii de plată acoperă postările operării, fără sursă pierdută la desfacere",
            Provenienta(ProvenientaD406.Din(aprilie), Xml(aprilie)));
        Storneaza(integrala, Zi(5, 8));
        Storneaza(excedent, Zi(5, 8));
        var mai = Export(5);
        Verifica("SC-SAFT-37", "storno în mai: −50 rest și −120 rest (liniile din aprilie negate), cu proveniența stornoului",
            mai.Refuzuri.Count == 0 && Linii(mai, integrala, true, (-50, null)) && Linii(mai, excedent, true, (-120, null))
            && Provenienta(ProvenientaD406.Din(mai), Xml(mai)));
    }

    void RepeatableReadPlati() {
        var fd = NouPartener("FD");
        var f = Fct(fd, Zi(4, 6), new LinieFctScena(1, 100, Stoc: false)).Id; Opereaza(f);
        var p = Trz(false, fd, [70], (os, d) => { d.Data = Zi(4, 7); d.DataInregistrare = d.Data; });
        using (var os = Deschide()) {
            using var citire = Fiscale.DeschideCitirea(os, cereIzolare: true);
            _ = C.Citiri.Contabil.Jurnal(os, Zi(4, 1), Zi(4, 30)).Count();
            Imperecheaza(p, f, 50, Zi(4, 10));
            var d = SaftProiectii.SaftPeCub(os, An, 4, Zi(4, 28));
            Verifica("SC-SAFT-35", "legătura comisă de altă sesiune în timpul citirii nu intră: 70 rest", Linii(d, p, false, (70, null)));
        }
        Verifica("SC-SAFT-35", "exportul următor vede legătura: 50 F + 20 rest", Linii(Export(4), p, false, (50, f), (20, null)));
    }

    void UnitateIstorica(Guid produs, Guid factura) {
        Guid Unitate(IObjectSpace os, string cod) => os.GetObjectsQuery<UnitateMasura>().Single(u => u.Cod == cod).ID;
        void Schimba(Guid id, string cod) => Comanda(os => {
            os.GetObjectByKey<Produs>(id).UnitateMasuraId = Unitate(os, cod);
            GardianEditare.Verifica(os); os.CommitChanges();
        });
        Schimba(produs, "H87");
        var inainte = Xml(Export(1));
        Refuza("SC-SAFT-22", () => Schimba(produs, "KGM"), "unitatea de măsură nu se mai schimbă");
        var linie = Export(1).FacturiPrimite.Single(f => f.DocumentId == factura).Linii.Single();
        Verifica("SC-SAFT-22", "UM istorică: completarea H87 e permisă, KGM după operare e refuzată, exportul rămâne 10 H87",
            linie is { Quantity: 10, InvoiceUOM: "H87" } && Xml(Export(1)).AsSpan().SequenceEqual(inainte));
        var liber = CuSpatiu(os => {
            var p = os.CreateObject<Produs>(); p.Cod = Marcaj + "-LIBER"; p.Denumire = p.Cod; p.UnitateMasuraId = Unitate(os, "H87");
            os.CommitChanges(); return p.ID;
        });
        Schimba(liber, "KGM");
        Verifica("SC-SAFT-22", "produsul fără mișcări operate își poate schimba unitatea",
            CuSpatiu(os => os.GetObjectByKey<Produs>(liber).UnitateMasura.Cod) == "KGM");
    }

    static bool Refuzat(Action actiune) {
        try { actiune(); return false; }
        catch (InvalidOperationException) { return true; }
    }

    Guid Dvi() => CuSpatiu(os => {
        var p = os.CreateObject<Partener>(); p.Cod = Marcaj + "-VAMA"; p.Denumire = p.Cod; p.ContImplicitId = Cont("446");
        var d = os.CreateObject<Dvi>(); d.Data = Ianuarie; d.Numar = Marcaj + "-MRN"; d.Predator = p; d.PrimitorId = Loc;
        var l = os.CreateObject<DocumentDetaliu>(); l.Document = d; l.TipMaterialId = Tip(os, Stoc);
        l.TipTvaId = Tva("IMP21"); l.Valoare = 100;
        os.CommitChanges(); return d.ID;
    });

    void Comutare() {
        var sursa = File.ReadAllText(Path.Combine(MetadataDump.DirectorProiect(), "..", "..", "Atlas.Conta.BackOffice",
            "Atlas.Conta.BackOffice.WebApi", "API", "Conta", "SaftController.cs"));
        Verifica("SC-SAFT-15", "S1 + S2 + S3: ușile L citesc SaftPeCub, ușile S SaftStocuriPeCub; exporturile vechi L și S nu mai sunt publice",
            System.Text.RegularExpressions.Regex.Matches(sursa, @"\(an, luna, SaftProiectii\.SaftPeCub\b").Count == 2
            && System.Text.RegularExpressions.Regex.Matches(sursa, @"\(an, luna, SaftProiectii\.SaftStocuriPeCub\b").Count == 2
            && !System.Text.RegularExpressions.Regex.IsMatch(sursa, @"SaftProiectii\.(Saft|SaftStocuri)\b"));
    }

    void Timbre() => Verifica("SC-SAFT-16", "data sistemului e data UTC a timbrului, fără fusul mașinii",
        SaftProiectii.DataSistem(new DateTime(2026, 1, 10, 23, 30, 0, DateTimeKind.Utc)) == new DateOnly(2026, 1, 10)
        && SaftProiectii.DataSistem(new DateTime(2026, 2, 5, 0, 15, 0, DateTimeKind.Utc)) == new DateOnly(2026, 2, 5)
        && SaftProiectii.DataSistem(new DateTime(2026, 2, 6, 12, 0, 0, DateTimeKind.Unspecified)) == new DateOnly(2026, 2, 6));

    void Ianuarie1(SaftDto d, Guid f01, Guid v02, Guid f03, Guid dvi, Guid fa, Guid ca) {
        var gl01 = Gl(d, f01).ToList();
        var fact01 = Facturi(d, f01);
        var tranzactie01 = d.Jurnale.SelectMany(j => j.Tranzactii).Where(t => t.DocumentId == f01).ToList();
        Verifica("SC-SAFT-01", "FCT 100+21: GL D 628 100, D 4426 21, C 401 121; factura 380 100/21/121 pe aceeași tranzacție",
            Debit(gl01, "628") == 100 && Debit(gl01, "4426") == 21 && Credit(gl01, "401") == 121 && Debit(gl01) == 121 && Credit(gl01) == 121
            && fact01.Count == 1 && Suma(fact01[0], "380", 100, 21, 121) && fact01[0].AccountID == "401"
            && fact01[0].Linii.Single().TaxInformation.TaxCode == "301104"
            && tranzactie01.Count == 1 && fact01[0].TransactionID == tranzactie01[0].TransactionID
            && fact01[0].GLPostingDate == Ianuarie && tranzactie01[0].GLPostingDate == Ianuarie);
        Verifica("SC-SAFT-01", "plata 40 lasă furnizorul cu 81 credit", Sold(d.Furnizori, fa) == -81);

        var gl02 = Gl(d, v02).ToList();
        var fact02 = Facturi(d, v02);
        Verifica("SC-SAFT-02", "FCL 200+42: D 4111 242, C 4427 42, C venit 200; factura 380 200/42/242; client 142",
            Debit(gl02, "4111") == 242 && Credit(gl02, "4427") == 42 && Credit(gl02) == 242
            && fact02.Count == 1 && Suma(fact02[0], "380", 200, 42, 242) && d.FacturiEmise.Contains(fact02[0])
            && Sold(d.Clienti, ca) == 142);

        var gl03 = Gl(d, f03).ToList();
        var fact03 = Facturi(d, f03);
        Verifica("SC-SAFT-03", "capitalizat: GL D/C 121/121, factura net 100, taxă 21, brut 121",
            Debit(gl03) == 121 && Credit(gl03, "401") == 121 && fact03.Count == 1 && Suma(fact03[0], "380", 100, 21, 121));

        var glDvi = Gl(d, dvi).ToList();
        Verifica("SC-SAFT-04", "DVI: GL 21/21 (D 4426, C 446), baza 100 nu intră în GL, zero facturi",
            Debit(glDvi, "4426") == 21 && Credit(glDvi, "446") == 21 && Debit(glDvi) == 21 && Credit(glDvi) == 21
            && Facturi(d, dvi).Count == 0);
    }

    void Ianuarie2(SaftDto d, Guid f19, Guid f20a, Guid f20b, Guid f21, Guid f22, Guid f18, Guid f23, Guid anulata, Guid stornoIan) {
        var fact19 = Facturi(d, f19);
        Verifica("SC-SAFT-19", "două cote: net 150, TVA 26,50, brut 176,50; două linii, două coduri, fără multiplicare",
            fact19.Count == 1 && Suma(fact19[0], "380", 150, 26.50m, 176.50m) && fact19[0].Linii.Count == 2
            && fact19[0].TaxInformationTotals.Count == 2);
        var fact20a = Facturi(d, f20a); var fact20b = Facturi(d, f20b);
        Verifica("SC-SAFT-20", "0,01 + 0,01 → net 0,02 TVA 0,00; taxa culeasă 21,01 → brut 121,01, fără recalcul",
            fact20a.Count == 1 && Suma(fact20a[0], "380", .02m, 0, .02m)
            && fact20b.Count == 1 && Suma(fact20b[0], "380", 100, 21.01m, 121.01m));
        var gl21 = Gl(d, f21).ToList();
        var fact21 = Facturi(d, f21);
        Verifica("SC-SAFT-21", "TI21: GL 121/121 (C 401 100), factura brut 100, taxa 21 pe 300906; 4427 pe 380006",
            Debit(gl21) == 121 && Credit(gl21) == 121 && Credit(gl21, "401") == 100
            && fact21.Count == 1 && Suma(fact21[0], "380", 100, 21, 100)
            && fact21[0].Linii.Single().TaxInformation.TaxCode == "300906"
            && gl21.Single(l => l.AccountID == "4427").TaxInformation.TaxCode == "380006");
        var fact22 = Facturi(d, f22);
        Verifica("SC-SAFT-22", "serviciu 2 ore × 50: cub cu cantitate 0, factura cu 2 și preț 50", fact22.Count == 1
            && fact22[0].Linii.Single() is { Quantity: 2, UnitPrice: 50, InvoiceLineAmount: 100 }
            && CuSpatiu(os => C.Citiri.Contabil.Postari(os).Where(p => p.DocumentId == f22).All(p => p.Cantitate == 0)));
        var fact18 = Facturi(d, f18);
        Verifica("SC-SAFT-18", "ianuarie: F 380 100/21/121, datat 8, înregistrat 10",
            fact18.Count == 1 && Suma(fact18[0], "380", 100, 21, 121)
            && fact18[0].InvoiceDate == Zi(1, 8) && fact18[0].GLPostingDate == Zi(1, 10)
            && Debit(Gl(d, f18)) == 121 && Credit(Gl(d, f18)) == 121);
        Verifica("SC-SAFT-23", "factura din 8 ianuarie înregistrată pe 5 februarie lipsește din ianuarie",
            Facturi(d, f23).Count == 0 && !Gl(d, f23).Any());
        Verifica("SC-SAFT-18", "anularea în perioadă deschisă nu lasă GL și nici factură 381",
            Facturi(d, anulata).Count == 0 && !Gl(d, anulata).Any());
        var stornate = Facturi(d, stornoIan);
        Verifica("SC-SAFT-18", "storno în aceeași lună: 380 +121 și 381 −121, GL net 0, identități distincte",
            stornate.Count == 2 && stornate.Any(f => Suma(f, "380", 100, 21, 121)) && stornate.Any(f => Suma(f, "381", -100, -21, -121))
            && stornate.Select(f => f.TransactionID).Distinct().Count() == 2
            && Debit(Gl(d, stornoIan)) == 0 && Credit(Gl(d, stornoIan)) == 0);
    }

    void Februarie1(SaftDto d, Guid f18, Guid c18, Guid f22, Guid c22, Guid f23, Guid stornoFeb, Guid retur, Guid receptie) {
        var fact18 = Facturi(d, f18, c18);
        var invers = fact18.SingleOrDefault(f => f.DocumentId == f18);
        var reemisa = fact18.SingleOrDefault(f => f.DocumentId == c18);
        Verifica("SC-SAFT-18", "februarie (B'): 381 −100/−21/−121 și 384 80/16,80/96,80, același număr, date originale",
            fact18.Count == 2 && invers != null && reemisa != null
            && Suma(invers, "381", -100, -21, -121) && Suma(reemisa, "384", 80, 16.80m, 96.80m)
            && invers.InvoiceNo == reemisa.InvoiceNo && invers.InvoiceDate == Zi(1, 8) && reemisa.InvoiceDate == Zi(1, 8)
            && invers.GLPostingDate == Februarie && reemisa.GLPostingDate == Februarie
            && invers.TransactionID != reemisa.TransactionID);
        Verifica("SC-SAFT-18", "GL februarie net D/C −24,20/−24,20", Debit(Gl(d, f18, c18)) == -24.20m && Credit(Gl(d, f18, c18)) == -24.20m);
        var fact22 = Facturi(d, f22, c22);
        Verifica("SC-SAFT-22", "corecția la 3 ore: inversa leagă cantitatea 2 și prețul 50, reemisa 3 × 40",
            fact22.Count == 2
            && fact22.Single(f => f.DocumentId == f22) is { InvoiceType: "381" } s
            && s.Linii.Single() is { Quantity: 2, UnitPrice: 50, InvoiceLineAmount: -100 }
            && fact22.Single(f => f.DocumentId == c22) is { InvoiceType: "384" } r
            && r.Linii.Single() is { Quantity: 3, UnitPrice: 40, InvoiceLineAmount: 120 });
        var fact23 = Facturi(d, f23);
        Verifica("SC-SAFT-23", "februarie: 380 100/21/121, InvoiceDate și TaxPointDate 8 ianuarie, GLPostingDate 5 februarie",
            fact23.Count == 1 && Suma(fact23[0], "380", 100, 21, 121) && fact23[0].InvoiceDate == Zi(1, 8)
            && fact23[0].Linii.Single().TaxPointDate == Zi(1, 8) && fact23[0].GLPostingDate == Februarie);
        var fs = Facturi(d, stornoFeb);
        Verifica("SC-SAFT-18", "storno în februarie al unei facturi din ianuarie: 381 −100/−21/−121",
            fs.Count == 1 && Suma(fs[0], "381", -100, -21, -121));
        var fr = Facturi(d, retur);
        Verifica("SC-SAFT-18", "retur RLF 2 buc: 381 −20/−4,20/−24,20 pe 401, fără a atinge recepția",
            fr.Count == 1 && Suma(fr[0], "381", -20, -4.20m, -24.20m) && fr[0].AccountID == "401"
            && Facturi(d, receptie).Count == 0);
    }

    void Cusaturi(SaftDto d, int luna) {
        var balanta = CuSpatiu(os => ContabilProiectii.Balanta(os, Zi(luna, 1), Zi(luna, DateTime.DaysInMonth(An, luna))).ToList());
        var tranzactii = d.Jurnale.SelectMany(j => j.Tranzactii).ToList();
        Verifica("SC-SAFT-15", $"luna {luna}: GL echilibrat pe fiecare tranzacție și total = rulajul balanței",
            tranzactii.All(t => Debit(t.Linii) == Credit(t.Linii))
            && d.Rezumat.TotalDebit == balanta.Sum(b => b.RulajDebit) && d.Rezumat.TotalCredit == balanta.Sum(b => b.RulajCredit)
            && d.Rezumat.ClosingGla == d.Rezumat.ClosingBalanta);
        var r = d.Rezumat;
        Console.WriteLine($"     MĂSURAT (cusături L luna {luna}): D {r.TotalDebit}/balanță {r.ValoareRegistruContabil}; "
            + $"TVA GL {r.TvaGl} + capitalizat {r.TvaCapitalizat} / fapte {r.TvaRegistru}; "
            + $"bază achiziții {r.BazaFacturiAchizitie} + {r.BazaNeincluseAchizitie} / {r.BazaRegistruAchizitie}; "
            + $"livrări {r.BazaFacturiLivrare} + {r.BazaNeincluseLivrare} / {r.BazaRegistruLivrare}; plăți {r.TotalPlati}");
        Verifica("SC-SAFT-15", $"luna {luna}: cusăturile ecranului L pe cub sunt egale (jurnal = rulajul balanței, TVA GL + capitalizat = faptele fiscale, "
            + "baza facturilor + baza neinclusă = faptele fiscale pe sens, plățile = Σ brut)",
            r.TotalDebit == r.ValoareRegistruContabil && r.TotalDebit != 0
            && r.TvaGl + r.TvaCapitalizat + r.TvaFaraCodSaft == r.TvaRegistru
            && r.BazaFacturiAchizitie + r.BazaNeincluseAchizitie == r.BazaRegistruAchizitie
            && r.BazaFacturiLivrare + r.BazaNeincluseLivrare == r.BazaRegistruLivrare
            && r.TotalPlati == d.Plati.Sum(p => p.GrossTotal) && r.NumarPlati == d.Plati.Count);
        Verifica("SC-SAFT-15", $"luna {luna}: fiecare factură are tranzacția GL a evenimentului ei",
            d.FacturiEmise.Concat(d.FacturiPrimite).All(f => tranzactii.Any(t => t.TransactionID == f.TransactionID)));
        var societate = CuSpatiu(os => SaftReguli.IdSocietate(os.GetObjectsQuery<Societate>().First()));
        var sisteme = CuSpatiu(os => os.GetObjectsQuery<C.Tranzactie>().Where(t => t.Data >= Zi(luna, 1) && t.Data <= Zi(luna, DateTime.DaysInMonth(An, luna)))
            .Select(t => new { t.ID, t.ScrisLa }).ToList().ToDictionary(t => t.ID.ToString(), t => t.ScrisLa));
        Verifica("SC-SAFT-16", $"luna {luna}: SystemEntryDate = data UTC a timbrului propriu al fiecărei tranzacții",
            tranzactii.All(t => !sisteme.TryGetValue(t.TransactionID, out var s) || t.SystemEntryDate == SaftProiectii.DataSistem(s)));
        Verifica("SC-SAFT-15", $"luna {luna}: fiecare partener referit în GL și facturi are intrare în Customers/Suppliers",
            tranzactii.SelectMany(t => t.Linii).Select(l => l.CustomerID).Where(i => i != societate)
                .All(i => d.Clienti.Any(c => c.Id == i))
            && tranzactii.SelectMany(t => t.Linii).Select(l => l.SupplierID).Where(i => i != societate)
                .All(i => d.Furnizori.Any(c => c.Id == i))
            && d.FacturiPrimite.All(f => d.Furnizori.Any(c => c.Id == f.PartenerID))
            && d.FacturiEmise.All(f => d.Clienti.Any(c => c.Id == f.PartenerID)));
    }


    void RepeatableRead() {
        var fc = NouPartener("FC");
        var primul = Fct(fc, Zi(4, 5), new LinieFctScena(1, 100, "N21", false)); Opereaza(primul.Id);
        var alDoilea = Fct(fc, Zi(4, 6), new LinieFctScena(1, 50, "N21", false));
        using (var os = Deschide()) {
            using var citire = Fiscale.DeschideCitirea(os, cereIzolare: true);
            var inainte = C.Citiri.Contabil.Jurnal(os, Zi(4, 1), Zi(4, 30)).Count();
            Opereaza(alDoilea.Id);
            var d = SaftProiectii.SaftPeCub(os, An, 4, Zi(4, 28));
            var gl = d.Jurnale.SelectMany(j => j.Tranzactii).SelectMany(t => t.Linii).ToList();
            Console.WriteLine($"     MĂSURAT (SC-SAFT-17): postări la deschidere {inainte}; GL D/C {Debit(gl)}/{Credit(gl)}; "
                + $"facturi {d.FacturiPrimite.Sum(f => f.NetTotal)}/{d.FacturiPrimite.Sum(f => f.GrossTotal)}; furnizor {Sold(d.Furnizori, fc)}");
            Verifica("SC-SAFT-17", "scriere concurentă între citiri: fișierul rămâne integral 121 (GL, facturi, furnizor)",
                inainte == 4 && Debit(gl) == 121 && Credit(gl) == 121
                && d.FacturiPrimite.Sum(f => f.NetTotal) == 100 && d.FacturiPrimite.Sum(f => f.GrossTotal) == 121
                && Sold(d.Furnizori, fc) == -121);
        }
        var dupa = Export(4);
        var glDupa = dupa.Jurnale.SelectMany(j => j.Tranzactii).SelectMany(t => t.Linii).ToList();
        Verifica("SC-SAFT-17", "exportul următor: 181,50 pe GL, facturi 150/31,50/181,50 și furnizor",
            Debit(glDupa) == 181.50m && dupa.FacturiPrimite.Sum(f => f.NetTotal) == 150
            && dupa.FacturiPrimite.Sum(Taxa) == 31.50m && dupa.FacturiPrimite.Sum(f => f.GrossTotal) == 181.50m
            && Sold(dupa.Furnizori, fc) == -181.50m);
        using (var os = Deschide()) {
            using var tx = TranzactieComanda.Incepe(os);
            var refuz = "";
            try { SaftProiectii.SaftPeCub(os, An, 4); }
            catch (InvalidOperationException e) { refuz = e.Message; }
            Verifica("SC-SAFT-17", "tranzacție ambiantă ReadCommitted: refuz înaintea citirii",
                refuz.StartsWith(Fiscale.IzolareInsuficienta, StringComparison.Ordinal));
        }
    }

    void MapareLipsa() {
        var tva = Tva("N21");
        var f = Factura(Zi(5, 5), new LinieFctScena(1, 100, "N21", false));
        try {
            Comanda(os => { os.GetObjectByKey<TipTva>(tva).Cota = 19; os.CommitChanges(); });
            Opereaza(f.Id);
        } finally {
            Comanda(os => { os.GetObjectByKey<TipTva>(tva).Cota = 21; os.CommitChanges(); });
        }
        var d = Export(5);
        Verifica("SC-SAFT-14", "cota istorică 19 fără mapare: SAFT_MAPARE_LIPSA pe GL și factură, XML refuzat, nu cod implicit",
            d.Refuzuri.Count(r => r.Cod == SaftProiectii.RefuzMapare && r.DocumentId == f.Id) == 2 && Refuzat(() => Xml(d)));
    }
}
