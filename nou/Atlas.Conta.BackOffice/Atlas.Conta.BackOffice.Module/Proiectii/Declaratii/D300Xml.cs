using System.Globalization;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using DevExpress.ExpressApp;

namespace Atlas.Conta.BackOffice.Module.Proiectii;

/// <summary>Datele de perioadă ale decontului, cu implicitele formularului (E2-D4).</summary>
public sealed record ParametriXmlD300(
    int Temei = 0,
    bool DepusReprezentant = false,
    bool? BifaInterne = null,
    decimal ProRata = 0m,
    bool SolicitRambursare = false,
    bool Cereale = false,
    bool Mobile = false,
    bool Dispozitive = false,
    bool Console = false);

/// <summary>Decontul 300 pe schema v12 (E2-D1…D5).</summary>
public static class D300Xml {
    // Rândul tipărit → atributele bazei și ale TVA-ului din schema v12 (d300-structura-2026.md §5).
    static readonly (string Cod, string Baza, string Tva)[] Elemente = [
        ("1", "R1_1", null), ("2", "R2_1", null), ("3", "R3_1", null), ("3.1", "R3_1_1", null), ("4", "R4_1", null),
        ("5", "R5_1", "R5_2"), ("5.1", "R5_1_1", "R5_1_2"), ("6", "R6_1", "R6_2"), ("7", "R7_1", "R7_2"),
        ("7.1", "R7_1_1", "R7_1_2"), ("8", "R8_1", "R8_2"), ("9", "R9_1", "R9_2"), ("10", "R10_1", "R10_2"),
        ("11", "R11_1", "R11_2"), ("12", "R12_1", "R12_2"), ("12.1", "R12_1_1", "R12_1_2"), ("12.2", "R12_2_1", "R12_2_2"),
        ("13", "R13_1", null), ("14", "R14_1", null), ("15", "R15_1", null), ("16", "R16_1", "R16_2"),
        ("17", "R64_1", "R64_2"), ("18", "R65_1", "R65_2"), ("19", "R17_1", "R17_2"),
        ("20", "R18_1", "R18_2"), ("20.1", "R18_1_1", "R18_1_2"), ("21", "R19_1", "R19_2"), ("22", "R20_1", "R20_2"),
        ("22.1", "R20_1_1", "R20_1_2"), ("23", "R21_1", "R21_2"), ("24", "R22_1", "R22_2"), ("25", "R23_1", "R23_2"),
        ("26", "R25_1", "R25_2"), ("26.1", "R25_1_1", "R25_1_2"), ("26.2", "R25_2_1", "R25_2_2"),
        ("27", null, "R43_2"), ("28", null, "R44_2"), ("29", "R26_1", null), ("29.1", "R26_1_1", null),
        ("30", "R27_1", "R27_2"), ("31", null, "R28_2"), ("32", null, "R29_2"), ("33", "R30_1", "R30_2"),
        ("34", null, "R31_2"), ("35", null, "R32_2"), ("36", null, "R33_2"), ("37", null, "R34_2"),
        ("38", null, "R35_2"), ("39", null, "R36_2"), ("40", null, "R37_2"), ("41", null, "R38_2"),
        ("42", null, "R39_2"), ("43", null, "R40_2"), ("44", null, "R41_2"), ("45", null, "R42_2"),
    ];

    static readonly string[] Intracomunitare =
        ["1", "2", "3", "3.1", "4", "5", "5.1", "6", "7", "7.1", "8", "20", "20.1", "21", "22", "22.1", "23", "29.1"];

    public static FisierDeclaratie Genereaza(IObjectSpace os, DateOnly inceput, DateOnly sfarsit,
            ParametriD300 externi, ParametriXmlD300 parametri) {
        parametri ??= new ParametriXmlD300();
        var dto = D300Proiectii.D300(os, inceput, sfarsit, externi, lei: true);
        var fisier = new FisierDeclaratie { VersiuneExportata = dto.VersiuneExportata };
        fisier.Refuzuri.AddRange(dto.RefuzuriXml);
        if (fisier.Refuzuri.Count > 0)
            return fisier;
        var antet = AntetDeclaratie.Citeste(os);

        var perioada = PerioadaDeclarata.Din(inceput, sfarsit, antet.Periodicitate, out _);
        var randuri = dto.Randuri.ToDictionary(r => r.Cod);
        var sume = new List<(string Nume, long Valoare)>();
        foreach (var (cod, baza, tva) in Elemente) {
            var rand = randuri[cod];
            if (baza != null && rand.Baza is decimal b) sume.Add((baza, (long)b));
            if (tva != null && rand.Tva is decimal t) sume.Add((tva, (long)t));
        }
        var interne = parametri.BifaInterne
            ?? Intracomunitare.All(c => (randuri[c].Baza ?? 0m) == 0m && (randuri[c].Tva ?? 0m) == 0m);

        XNamespace ns = FormulareAnaf.D300v12.SpatiuNume;
        var radacina = new XElement(ns + "declaratie300",
            new XAttribute("luna", perioada.Luna),
            new XAttribute("an", perioada.An),
            new XAttribute("depusReprezentant", parametri.DepusReprezentant ? 1 : 0),
            new XAttribute("bifa_interne", interne ? 1 : 0),
            new XAttribute("temei", parametri.Temei),
            new XAttribute("nume_declar", antet.DeclarantNume),
            new XAttribute("prenume_declar", antet.DeclarantPrenume),
            new XAttribute("functie_declar", antet.DeclarantFunctie),
            new XAttribute("cui", antet.Cui),
            new XAttribute("den", antet.Denumire),
            new XAttribute("adresa", antet.Adresa),
            antet.Telefon == null ? null : new XAttribute("telefon", antet.Telefon),
            antet.Email == null ? null : new XAttribute("mail", antet.Email),
            new XAttribute("banca", antet.Banca),
            new XAttribute("cont", antet.Cont),
            new XAttribute("caen", antet.Caen),
            new XAttribute("tip_decont", perioada.Tip),
            new XAttribute("pro_rata", parametri.ProRata.ToString("0.##", CultureInfo.InvariantCulture)),
            new XAttribute("bifa_cereale", DaNu(parametri.Cereale)),
            new XAttribute("bifa_mob", DaNu(parametri.Mobile)),
            new XAttribute("bifa_disp", DaNu(parametri.Dispozitive)),
            new XAttribute("bifa_cons", DaNu(parametri.Console)),
            new XAttribute("solicit_ramb", DaNu(parametri.SolicitRambursare)),
            new XAttribute("nr_evid", NumarEvidenta(perioada)),
            new XAttribute("totalPlata_A", sume.Sum(s => s.Valoare)),
            sume.Select(s => new XAttribute(s.Nume, s.Valoare)));

        fisier.Continut = Serializeaza(new XDocument(new XDeclaration("1.0", "UTF-8", null), radacina));
        fisier.NumeFisier = $"D300_{antet.Cui}_{perioada.An:0000}{perioada.Luna:00}.xml";
        return fisier;
    }

    /// <summary>Refuzurile fișierului pentru perioada cerută, înaintea serializării (E2-D1, E2-D4, E2-D5).</summary>
    public static List<RefuzDeclaratie> Refuzuri(AntetDeclaratie antet, DateOnly inceput, DateOnly sfarsit, D300Dto dto) {
        var refuzuri = new List<RefuzDeclaratie>();
        void Refuz(string cod, string mesaj) => refuzuri.Add(new RefuzDeclaratie { Cod = cod, Mesaj = mesaj });

        if (FormulareAnaf.Pentru(FormularFiscal.D300, inceput) == null)
            Refuz(CoduriDeclaratie.PerioadaNesuportata,
                $"Fișierul D300 se generează pe formularul din 2026 (schema v12), pentru perioade de la {FormulareAnaf.D300v12.ValabilDeLa:dd.MM.yyyy}.");
        if (antet == null)
            Refuz(CoduriDeclaratie.AntetIncomplet, "Societatea nu are rândul de identificare completat.");
        else {
            if (PerioadaDeclarata.Din(inceput, sfarsit, antet.Periodicitate, out var motiv) == null)
                Refuz(CoduriDeclaratie.PerioadaNesuportata, motiv);
            if (antet.LipsaD300() is { Count: > 0 } lipsa)
                Refuz(CoduriDeclaratie.AntetIncomplet, "Completați la societate: " + string.Join(", ", lipsa) + ".");
        }
        foreach (var n in dto.Nemapate.Where(n => n.Baza != 0m || n.Tva != 0m))
            Refuz(CoduriDeclaratie.DateNeincluse,
                $"Tipul de TVA {n.TipTvaCod ?? n.TipTvaId.ToString()} ({n.SensTva}, cota {n.Cota:0.##}) nu are rând în decont: "
                + $"baza {n.Baza.ToString("N2", Ro)}, TVA {n.Tva.ToString("N2", Ro)}.");
        var lipsaRanduri = Elemente.Select(e => e.Cod).Where(c => dto.Randuri.All(r => r.Cod != c)).ToList();
        if (lipsaRanduri.Count > 0)
            Refuz(CoduriDeclaratie.DateNeincluse,
                "Nomenclatorul D300 nu are rândurile " + string.Join(", ", lipsaRanduri) + ".");
        return refuzuri;
    }

    static readonly CultureInfo Ro = CultureInfo.GetCultureInfo("ro-RO");

    static string DaNu(bool da) => da ? "D" : "N";

    /// <summary>Numărul de evidență a plății: obligația, perioada, scadența (25 a lunii următoare), cifra de control.</summary>
    public static string NumarEvidenta(PerioadaDeclarata perioada) {
        var scadenta = new DateOnly(perioada.An, perioada.Luna, 25).AddMonths(1);
        var cod = perioada.Periodicitate == PeriodicitateTva.Trimestrial ? "302" : "301";
        var corp = $"10{cod}01{perioada.Luna:00}{perioada.An % 100:00}{scadenta:ddMMyy}0000";
        return corp + (corp.Sum(c => c - '0') % 100).ToString("00");
    }

    static byte[] Serializeaza(XDocument document) {
        using var flux = new MemoryStream();
        using (var scriitor = XmlWriter.Create(flux, new XmlWriterSettings {
            Encoding = new UTF8Encoding(false), Indent = true,
        }))
            document.Save(scriitor);
        return flux.ToArray();
    }
}
