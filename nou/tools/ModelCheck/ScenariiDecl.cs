using System.Xml.Linq;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Proiectii;
using DevExpress.ExpressApp;

namespace Atlas.Conta.BackOffice.ModelCheck;

// docs/nucleu/scenarii/DECL.md: D300 și D394 în XML, validate de XSD și de DUKIntegrator (E2).
sealed partial class ScenariiDecl(Func<IObjectSpace> deschide, Action<string, bool> check,
    bool privat, Action<IObjectSpace, int, int> inchide)
    : ScenaDocumente(deschide, check, privat, inchide, "DECL", 2043) {
    const string CuiSocietate = "4221306";
    Guid banca;

    DateOnly Luna(int luna, int zi = 10) => new(An, luna, zi);
    (DateOnly, DateOnly) Perioada(int luna, int luni = 1) => (new(An, luna, 1), new DateOnly(An, luna, 1).AddMonths(luni).AddDays(-1));

    protected override void CurataNomenclatoare(IObjectSpace os, Purja purja) {
        var reps = os.GetObjectsQuery<Repartitor>().Where(r => r.Cod.StartsWith(Marcaj)).Select(r => r.ID).ToList();
        foreach (var s in os.GetObjectsQuery<Societate>().Where(s => s.ContBancarId != null && reps.Contains(s.ContBancarId.Value)).ToList())
            s.ContBancarId = null;
        os.CommitChanges();
    }

    protected override void Executa() {
        var original = CuSpatiu(Instantaneu.Din);
        try {
            Comanda(os => {
                var c = os.CreateObject<ContPropriu>();
                c.Cod = Marcaj + "-BANCA"; c.Denumire = "Banca Test"; c.EsteBanca = true; c.Iban = "RO49AAAA1B31007593840000";
                banca = c.ID;
                var p = os.GetObjectByKey<Partener>(Furnizor);
                p.Tara = "RO"; p.InregistratTva = true; p.CodFiscal = "RO12345674";
                var k = os.GetObjectByKey<Partener>(Client);
                k.Tara = "RO"; k.InregistratTva = true; k.CodFiscal = "RO18547290";
                os.CommitChanges();
            });
            Antet();
            Refuzuri();
            LunaGoala();
            if (Privat) {
                LunaCuOperatiuni();
                Nemapate();
                Trimestru();
            }
        }
        finally {
            Comanda(os => { original.Restaureaza(os); os.CommitChanges(); });
        }
    }

    void Antet(Action<Societate> modifica = null) => Comanda(os => {
        var s = os.GetObjectsQuery<Societate>().Single();
        s.Denumire = "Societatea de probă SRL"; s.CodFiscal = "RO" + CuiSocietate; s.InregistratTva = true;
        s.Strada = "Lungă"; s.Numar = "1"; s.Localitate = "Brașov"; s.Telefon = "0268000000"; s.Email = "proba@example.ro";
        s.ContBancarId = banca; s.Caen = "4690"; s.PeriodicitateTva = PeriodicitateTva.Lunar;
        s.DeclarantNume = "Popescu"; s.DeclarantPrenume = "Ion"; s.DeclarantFunctie = "Administrator";
        modifica?.Invoke(s);
        os.CommitChanges();
    });

    void Refuzuri() {
        var (inceput, sfarsit) = Perioada(3);
        Antet(s => s.Caen = null);
        Verifica("SC-DECL-02", "CAEN lipsă: refuz antet, fără fișier", Refuz(inceput, sfarsit, CoduriDeclaratie.AntetIncomplet, "CAEN"));
        Antet(s => s.PeriodicitateTva = PeriodicitateTva.Trimestrial);
        Verifica("SC-DECL-02", "trimestrial pe o lună: refuz de perioadă", Refuz(inceput, sfarsit, CoduriDeclaratie.PerioadaNesuportata, "trimestru"));
        Antet();
        Verifica("SC-DECL-02", "perioadă din 2025: refuz de formular",
            Refuz(new(2025, 12, 1), new(2025, 12, 31), CoduriDeclaratie.PerioadaNesuportata, "v12"));
        Verifica("SC-DECL-02", "perioadă de 15 zile: refuz de perioadă",
            Refuz(inceput, inceput.AddDays(14), CoduriDeclaratie.PerioadaNesuportata, "lună"));
    }

    bool Refuz(DateOnly inceput, DateOnly sfarsit, string cod, string fragment) => CuSpatiu(os => {
        var f = D300Xml.Genereaza(os, inceput, sfarsit, null, null);
        var dto = D300Proiectii.D300(os, inceput, sfarsit, null);
        return f.Continut == null && f.Refuzuri.Any(r => r.Cod == cod && r.Mesaj.Contains(fragment))
            && dto.RefuzuriXml.Select(r => r.Cod).SequenceEqual(f.Refuzuri.Select(r => r.Cod));
    });

    void LunaGoala() {
        var (inceput, sfarsit) = Perioada(3);
        var xml = Exporta("SC-DECL-03", inceput, sfarsit);
        var d = Atribute(xml);
        Verifica("SC-DECL-03", "luna fără operațiuni: totaluri zero, decont simplificat, L/3",
            d["luna"] == "3" && d["tip_decont"] == "L" && d["bifa_interne"] == "1" && d["totalPlata_A"] == "0"
            && d["R17_2"] == "0" && d["R27_2"] == "0" && d["cui"] == CuiSocietate);
        Verifica("SC-DECL-03", "nr_evid: obligația 301, perioada 03" + (An % 100) + ", scadența 25.04, cifra de control",
            d["nr_evid"] == D300Xml.NumarEvidenta(new PerioadaDeclarata(An, 3, PeriodicitateTva.Lunar))
            && d["nr_evid"].StartsWith($"1030101" + $"03{An % 100:00}2504{An % 100:00}0000", StringComparison.Ordinal));
        var aDoua = CuSpatiu(os => D300Xml.Genereaza(os, inceput, sfarsit, null, null)).Continut;
        Verifica("SC-DECL-06", "reexportul aceleiași perioade e identic byte cu byte", aDoua.AsSpan().SequenceEqual(File.ReadAllBytes(xml)));
    }

    void LunaCuOperatiuni() {
        var fct = Factura(Luna(1), new LinieFctScena(1, 100.40m, "N21", Stoc: false));
        var ti = Factura(Luna(1), new LinieFctScena(1, 300, "TI21", Stoc: false));
        Opereaza(fct.Id); Opereaza(ti.Id);
        Opereaza(Vanzare(Luna(1), 1000.50m, "N21"));
        Opereaza(Vanzare(Luna(1), 200, "N11"));
        var (inceput, sfarsit) = Perioada(1);
        var bani = CuSpatiu(os => D300Proiectii.D300(os, inceput, sfarsit, null));
        Verifica("SC-DECL-01", "proiecția în bani rămâne 1.000,50 / 210,11 pe rd. 9",
            bani.Randuri.Single(r => r.Cod == "9") is { Baza: 1000.50m, Tva: 210.11m });
        var xml = Exporta("SC-DECL-01", inceput, sfarsit);
        var d = Atribute(xml);
        string V(string a) => d.GetValueOrDefault(a);
        Verifica("SC-DECL-01", "rândurile în lei: 9 = 1001/210, 10 = 200/22, 12.1 = 300/63, 24 = 100/21, 26.1 = 300/63",
            (V("R9_1"), V("R9_2"), V("R10_1"), V("R10_2"), V("R12_1_1"), V("R12_1_2"), V("R22_1"), V("R22_2"), V("R25_1_1"), V("R25_1_2"))
            == ("1001", "210", "200", "22", "300", "63", "100", "21", "300", "63"));
        Verifica("SC-DECL-01", "totalurile din rândurile rotunjite: 19 = 295, 30 = 31 = 35 = 84, 37 = 40 = 44 = 211",
            (V("R17_2"), V("R27_2"), V("R28_2"), V("R32_2"), V("R34_2"), V("R37_2"), V("R41_2"), V("R42_2"))
            == ("295", "84", "84", "84", "211", "211", "211", "0"));
        Verifica("SC-DECL-01", "numai operațiuni interne, inclusiv taxarea inversă: bifa_interne 1", V("bifa_interne") == "1");
        var lei = CuSpatiu(os => D300Proiectii.D300(os, inceput, sfarsit, null, lei: true));
        Verifica("SC-DECL-01", "fiecare atribut R e rândul proiecției în lei, iar totalPlata_A e suma lor",
            RecalculIndependent(lei, d));
        Mutanti(xml);
    }

    static bool RecalculIndependent(D300Dto lei, Dictionary<string, string> xml) {
        var r = xml.Where(a => a.Key.StartsWith('R') && char.IsAsciiDigit(a.Key[1])).Select(a => long.Parse(a.Value)).ToList();
        var coloane = lei.Randuri.SelectMany(x => new[] { x.Baza, x.Tva }).Where(v => v != null).Select(v => v.Value).ToList();
        return coloane.All(v => v == decimal.Truncate(v)) && r.Count == coloane.Count
            && r.Sum() == coloane.Sum() && long.Parse(xml["totalPlata_A"]) == r.Sum();
    }

    void Mutanti(string xml) {
        foreach (var (eticheta, atribut, valoare, poarta) in new[] {
            ("rd. 9 TVA +1 fără total", "R9_2", "211", "DUK"),
            ("CUI cu cifra de control greșită", "cui", "4221307", "DUK"),
            ("tip_decont în afara listei", "tip_decont", "X", "XSD"),
        }) {
            var doc = XDocument.Load(xml);
            doc.Root.SetAttributeValue(atribut, valoare);
            var cale = Path.Combine(Duk.DirectorTemporar(), $"{Marcaj}-mutant-{atribut}.xml");
            doc.Save(cale);
            var xsd = ValidareDeclaratii.Xsd(ValidareDeclaratii.D300, cale);
            var respins = poarta == "XSD" ? xsd.Count > 0 : !ValidareDeclaratii.Duk(ValidareDeclaratii.D300, cale).Valid && xsd.Count == 0;
            Verifica("SC-DECL-05", $"mutantul „{eticheta}” e respins de {poarta}", respins);
        }
    }

    void Nemapate() {
        var n9 = Factura(Luna(2), new LinieFctScena(1, 50, "N9", Stoc: false));
        Opereaza(n9.Id);
        var (inceput, sfarsit) = Perioada(2);
        Verifica("SC-DECL-02", "achiziția N9 fără rând în formular: refuz cu tipul numit",
            Refuz(inceput, sfarsit, CoduriDeclaratie.DateNeincluse, "N9"));
        Storneaza(n9.Id, Luna(2, 20));
        Exporta("SC-DECL-03", inceput, sfarsit);
    }

    void Trimestru() {
        Antet(s => s.PeriodicitateTva = PeriodicitateTva.Trimestrial);
        var (inceput, sfarsit) = Perioada(1, 3);
        var d = Atribute(Exporta("SC-DECL-04", inceput, sfarsit));
        Verifica("SC-DECL-04", "trimestrul I: T/3, rândurile lunii ianuarie, nr_evid pe obligația 302",
            d["tip_decont"] == "T" && d["luna"] == "3" && d["R9_1"] == "1001" && d["nr_evid"].StartsWith("10302", StringComparison.Ordinal));
        Antet();
    }

    Guid Vanzare(DateOnly data, decimal baza, string tva) => CuSpatiu(os => {
        var d = os.CreateObject<FacturaIesire>(); d.Data = data; d.Numar = Marcaj + "-FCL-" + baza;
        d.PredatorId = Loc; d.PrimitorId = Client;
        var l = os.CreateObject<FacturaIesireDetaliu>(); l.Document = d; l.Pozitie = 1;
        l.TipMaterialId = Tip(os, "704"); l.Cantitate = 1; l.PretUnitar = baza; l.TipTvaId = Tva(tva);
        os.CommitChanges(); return d.ID;
    });

    string Exporta(string id, DateOnly inceput, DateOnly sfarsit) {
        var f = CuSpatiu(os => D300Xml.Genereaza(os, inceput, sfarsit, null, null));
        Verifica(id, $"fișier D300 {inceput:yyyy-MM}…{sfarsit:yyyy-MM} fără refuzuri ({string.Join("; ", f.Refuzuri.Select(r => r.Mesaj))})",
            f.Continut != null && f.Refuzuri.Count == 0);
        var cale = Path.Combine(Duk.DirectorTemporar(), $"{Marcaj}-{(Privat ? "privat" : "bugetar")}-{f.NumeFisier ?? "refuzat.xml"}");
        File.WriteAllBytes(cale, f.Continut ?? []);
        var xsd = f.Continut == null ? ["fără conținut"] : ValidareDeclaratii.Xsd(ValidareDeclaratii.D300, cale);
        Verifica(id, $"XSD v12: {(xsd.Count == 0 ? "valid" : string.Join("; ", xsd.Take(3)))}", xsd.Count == 0);
        var duk = ValidareDeclaratii.Duk(ValidareDeclaratii.D300, cale);
        Verifica(id, $"DUKIntegrator {duk.Versiune}: {duk.Rezumat}{(duk.Valid ? "" : " — " + string.Join("; ", duk.Erori.Take(4)))}", duk.Valid);
        return cale;
    }

    static Dictionary<string, string> Atribute(string cale) =>
        XDocument.Load(cale).Root.Attributes().ToDictionary(a => a.Name.LocalName, a => a.Value);

    sealed record Instantaneu(Guid Id, string Denumire, string CodFiscal, bool InregistratTva, string Strada, string Numar,
        string Localitate, string Telefon, string Email, Guid? ContBancarId, string Caen, PeriodicitateTva Periodicitate,
        string DeclarantNume, string DeclarantPrenume, string DeclarantFunctie) {
        public static Instantaneu Din(IObjectSpace os) => os.GetObjectsQuery<Societate>().ToList()
            .Select(s => new Instantaneu(s.ID, s.Denumire, s.CodFiscal, s.InregistratTva, s.Strada, s.Numar, s.Localitate,
                s.Telefon, s.Email, s.ContBancarId, s.Caen, s.PeriodicitateTva, s.DeclarantNume, s.DeclarantPrenume,
                s.DeclarantFunctie)).Single();

        public void Restaureaza(IObjectSpace os) {
            var s = os.GetObjectByKey<Societate>(Id);
            (s.Denumire, s.CodFiscal, s.InregistratTva, s.Strada, s.Numar, s.Localitate, s.Telefon, s.Email) =
                (Denumire, CodFiscal, InregistratTva, Strada, Numar, Localitate, Telefon, Email);
            (s.ContBancarId, s.Caen, s.PeriodicitateTva, s.DeclarantNume, s.DeclarantPrenume, s.DeclarantFunctie) =
                (ContBancarId, Caen, Periodicitate, DeclarantNume, DeclarantPrenume, DeclarantFunctie);
        }
    }
}
