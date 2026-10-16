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
        purja.Adauga(os.GetObjectsQuery<PlajaFacturi>().Where(p => p.Serie == SeriePlaja));
        purja.Adauga(os.GetObjectsQuery<DepunereDeclaratie>().Where(d => d.Perioada >= An * 100 && d.Perioada < (An + 1) * 100));
        purja.Adauga(os.GetObjectsQuery<TipTva>().Where(t => t.Cod.StartsWith(Marcaj)));
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
            RotunjireaPartilor();
            Refuzuri();
            LunaGoala();
            if (Privat) {
                LunaCuOperatiuni();
                Nemapate();
                Trimestru();
            }
            D394();
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
        s.IntocmitDe = TipIntocmitor.PersoanaJuridica; s.IntocmitCalitate = "Contabil";
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
        Verifica("SC-DECL-01", "fiecare atribut R e rândul proiecției în lei pe harta oficială transcrisă în probă, iar totalPlata_A e suma lor",
            RecalculIndependent(lei, d));
        Mutanti(xml);
    }

    // d300-structura-2026.md §5, transcris separat de harta din D300Xml: rândul tipărit → (bază, TVA).
    static readonly Dictionary<string, (string Baza, string Tva)> HartaV12 = new() {
        ["1"] = ("R1_1", null), ["2"] = ("R2_1", null), ["3"] = ("R3_1", null), ["3.1"] = ("R3_1_1", null),
        ["4"] = ("R4_1", null), ["5"] = ("R5_1", "R5_2"), ["5.1"] = ("R5_1_1", "R5_1_2"), ["6"] = ("R6_1", "R6_2"),
        ["7"] = ("R7_1", "R7_2"), ["7.1"] = ("R7_1_1", "R7_1_2"), ["8"] = ("R8_1", "R8_2"), ["9"] = ("R9_1", "R9_2"),
        ["10"] = ("R10_1", "R10_2"), ["11"] = ("R11_1", "R11_2"), ["12"] = ("R12_1", "R12_2"),
        ["12.1"] = ("R12_1_1", "R12_1_2"), ["12.2"] = ("R12_2_1", "R12_2_2"), ["13"] = ("R13_1", null),
        ["14"] = ("R14_1", null), ["15"] = ("R15_1", null), ["16"] = ("R16_1", "R16_2"), ["17"] = ("R64_1", "R64_2"),
        ["18"] = ("R65_1", "R65_2"), ["19"] = ("R17_1", "R17_2"), ["20"] = ("R18_1", "R18_2"),
        ["20.1"] = ("R18_1_1", "R18_1_2"), ["21"] = ("R19_1", "R19_2"), ["22"] = ("R20_1", "R20_2"),
        ["22.1"] = ("R20_1_1", "R20_1_2"), ["23"] = ("R21_1", "R21_2"), ["24"] = ("R22_1", "R22_2"),
        ["25"] = ("R23_1", "R23_2"), ["26"] = ("R25_1", "R25_2"), ["26.1"] = ("R25_1_1", "R25_1_2"),
        ["26.2"] = ("R25_2_1", "R25_2_2"), ["27"] = (null, "R43_2"), ["28"] = (null, "R44_2"), ["29"] = ("R26_1", null),
        ["29.1"] = ("R26_1_1", null), ["30"] = ("R27_1", "R27_2"), ["31"] = (null, "R28_2"), ["32"] = (null, "R29_2"),
        ["33"] = ("R30_1", "R30_2"), ["34"] = (null, "R31_2"), ["35"] = (null, "R32_2"), ["36"] = (null, "R33_2"),
        ["37"] = (null, "R34_2"), ["38"] = (null, "R35_2"), ["39"] = (null, "R36_2"), ["40"] = (null, "R37_2"),
        ["41"] = (null, "R38_2"), ["42"] = (null, "R39_2"), ["43"] = (null, "R40_2"), ["44"] = (null, "R41_2"),
        ["45"] = (null, "R42_2"),
    };

    static bool RecalculIndependent(D300Dto lei, Dictionary<string, string> xml) {
        var r = xml.Where(a => a.Key.StartsWith('R') && char.IsAsciiDigit(a.Key[1])).ToDictionary(a => a.Key, a => long.Parse(a.Value));
        var asteptate = new Dictionary<string, long>();
        foreach (var rand in lei.Randuri) {
            if (!HartaV12.TryGetValue(rand.Cod, out var h)) return false;
            if (h.Baza != null && rand.Baza is decimal b) asteptate[h.Baza] = (long)b;
            if (h.Tva != null && rand.Tva is decimal t) asteptate[h.Tva] = (long)t;
        }
        return asteptate.Count == r.Count && asteptate.All(a => r.TryGetValue(a.Key, out var v) && v == a.Value)
            && long.Parse(xml["totalPlata_A"]) == r.Values.Sum();
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
        var propriu = Marcaj + "-T9";
        Comanda(os => {
            var n9 = os.GetObjectsQuery<TipTva>().Single(t => t.Cod == "N9");
            var t = os.CreateObject<TipTva>(); t.Cod = propriu; t.Denumire = "TVA 9% propriu, fără mapare";
            t.Cota = 9m; t.Regim = RegimTva.Normal;
            t.ContTvaDeductibilId = n9.ContTvaDeductibilId; t.ContTvaColectatId = n9.ContTvaColectatId;
            t.ContTvaNeexigibilId = n9.ContTvaNeexigibilId;
            os.CommitChanges();
        });
        var n9Achizitie = Factura(Luna(2), new LinieFctScena(1, 50, "N9", Stoc: false));
        var nimLivrare = Vanzare(Luna(2), 110, "NIM");
        var nemapat = Factura(Luna(2), new LinieFctScena(1, 70, propriu, Stoc: false));
        Opereaza(n9Achizitie.Id); Opereaza(nimLivrare); Opereaza(nemapat.Id);
        var (inceput, sfarsit) = Perioada(2);
        var dto = CuSpatiu(os => D300Proiectii.D300(os, inceput, sfarsit, null));
        Verifica("SC-DECL-02", "tipul propriu fără mapare: refuz cu tipul numit; N9 și NIM nu refuză",
            Refuz(inceput, sfarsit, CoduriDeclaratie.DateNeincluse, propriu)
            && dto.RefuzuriXml.Count == 1 && dto.Nemapate.Single().TipTvaCod == propriu);
        Storneaza(nemapat.Id, Luna(2, 20));
        var d = Atribute(Exporta("SC-DECL-03", inceput, sfarsit));
        dto = CuSpatiu(os => D300Proiectii.D300(os, inceput, sfarsit, null));
        Verifica("SC-DECL-19", "achiziția N9 pe rd. 33 (50 / 4,50 → R30 50 / 5), regularizare a taxei deduse (123 e)",
            dto.Randuri.Single(r => r.Cod == "33") is { Baza: 50m, Tva: 4.50m, Surse: "N9" }
            && d.GetValueOrDefault("R30_1") == "50" && d.GetValueOrDefault("R30_2") == "5");
        Verifica("SC-DECL-19", "livrarea NIM 110 iese în `Excluse` cu motivul, nu în `Nemapate`, și nu oprește fișierul (123 d)",
            dto.Excluse.SingleOrDefault() is { TipTvaCod: "NIM", SensTva: "Livrare", Baza: 110m, Tva: 0m } x
            && !string.IsNullOrWhiteSpace(x.Motiv) && dto.Nemapate.All(n => n.Baza == 0m && n.Tva == 0m));
        var d394 = CuSpatiu(os => D394Proiectii.D394(os, inceput, sfarsit));
        Verifica("SC-DECL-19", "în D394 aceeași lună: N9 pe A cu cota 9, NIM exclusă, niciuna neinclusă",
            d394.Operatiuni.SingleOrDefault(o => o.Tip == "A" && o.Cota == 9) is { Baza: 50m }
            && d394.Excluse.Any(e => e.TipTvaCod == "NIM" && e.SensTva == "Livrare" && e.Baza == 110m)
            && !d394.Neincluse.Any(n => n.TipTvaCod is "N9" or "NIM"));
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
        Verifica(id, $"DUKIntegrator {duk.Versiune}: {duk.Rezumat}{(duk.Valid ? "" : " — " + string.Join("; ", duk.Erori.Take(4)))}"
            + string.Concat(duk.Avertismente.Take(2).Select(a => " | " + a)), duk.Valid && duk.Avertismente.Count == 0);
        return cale;
    }

    static Dictionary<string, string> Atribute(string cale) =>
        XDocument.Load(cale).Root.Attributes().ToDictionary(a => a.Name.LocalName, a => a.Value);

    sealed record Instantaneu(Guid Id, string Denumire, string CodFiscal, bool InregistratTva, string Strada, string Numar,
        string Localitate, string Telefon, string Email, Guid? ContBancarId, string Caen, PeriodicitateTva Periodicitate,
        string DeclarantNume, string DeclarantPrenume, string DeclarantFunctie, TipIntocmitor IntocmitDe, string IntocmitCalitate) {
        public static Instantaneu Din(IObjectSpace os) => os.GetObjectsQuery<Societate>().ToList()
            .Select(s => new Instantaneu(s.ID, s.Denumire, s.CodFiscal, s.InregistratTva, s.Strada, s.Numar, s.Localitate,
                s.Telefon, s.Email, s.ContBancarId, s.Caen, s.PeriodicitateTva, s.DeclarantNume, s.DeclarantPrenume,
                s.DeclarantFunctie, s.IntocmitDe, s.IntocmitCalitate)).Single();

        public void Restaureaza(IObjectSpace os) {
            var s = os.GetObjectByKey<Societate>(Id);
            (s.Denumire, s.CodFiscal, s.InregistratTva, s.Strada, s.Numar, s.Localitate, s.Telefon, s.Email) =
                (Denumire, CodFiscal, InregistratTva, Strada, Numar, Localitate, Telefon, Email);
            (s.ContBancarId, s.Caen, s.PeriodicitateTva, s.DeclarantNume, s.DeclarantPrenume, s.DeclarantFunctie) =
                (ContBancarId, Caen, Periodicitate, DeclarantNume, DeclarantPrenume, DeclarantFunctie);
            (s.IntocmitDe, s.IntocmitCalitate) = (IntocmitDe, IntocmitCalitate);
        }
    }
}
