using System.Xml;
using System.Xml.Schema;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Proiectii;

namespace Atlas.Conta.BackOffice.ModelCheck;

/// <summary>Schema și validatorul oficial pe care se certifică o declarație D300 sau D394 (E2-D9).</summary>
public sealed record KitDeclaratie(FormularFiscal Formular, string Tip, ManifestD406.Pin Xsd, string SpatiuNume,
    string VersiuneValidator, ManifestD406.Pin[] Kit);

public static class ValidareDeclaratii {
    static readonly ManifestD406.Pin[] Comun = ManifestD406.Kit.Where(p => p.Cale != "DUKIntegrator_AnLunaUI.jar"
        && p.Cale != "lib/D406Validator.jar").ToArray();

    public static readonly KitDeclaratie D300 = new(FormularFiscal.D300, "D300",
        new("Anaf/d300_v12_11022026.xsd", "92D5F4BCFCC85563518957D5B23D86F79CBAFB94C1A8545586128186F412AEB6"),
        FormulareAnaf.D300v12.SpatiuNume, "J12.0.1",
        [.. Comun, new("lib/D300Validator.jar", "91A252F5669C5A6DE680AF3953CD1E62E162ED236563FC0E249161432C750EE8")]);

    public static readonly KitDeclaratie D394 = new(FormularFiscal.D394, "D394",
        new("Anaf/d394_20250917.xsd", "D803EA37D4C17430729F3244B93087357C2BE71EA488CF299B78FB83AB4822BD"),
        FormulareAnaf.D394v5.SpatiuNume, "J8.0.2",
        [.. Comun, new("lib/D394Validator.jar", "586E8A4A62C84CD9076B0AB43387DC0518D3BF322476F29AE942354889523E54")]);

    /// <summary>Erorile schemei oficiale; schema nepotrivită manifestului e eroare.</summary>
    public static List<string> Xsd(KitDeclaratie kit, string caleXml) {
        if (ManifestD406.Abateri(AppContext.BaseDirectory, [kit.Xsd]) is [var abatere, ..])
            return [$"XSD {kit.Tip} nepotrivit manifestului: {abatere}"];
        var set = new XmlSchemaSet();
        set.Add(kit.SpatiuNume, Path.Combine(AppContext.BaseDirectory, kit.Xsd.Cale));
        set.Compile();
        var erori = new List<string>();
        var setari = new XmlReaderSettings {
            ValidationType = ValidationType.Schema, Schemas = set,
            ValidationFlags = XmlSchemaValidationFlags.ReportValidationWarnings,
        };
        setari.ValidationEventHandler += (_, e) =>
            erori.Add($"{e.Severity} {e.Exception.LineNumber}:{e.Exception.LinePosition} {e.Message}");
        using var r = XmlReader.Create(caleXml, setari);
        var radacina = false;
        while (r.Read())
            if (r is { NodeType: XmlNodeType.Element, Depth: 0 })
                radacina = r.SchemaInfo?.SchemaElement != null;
        if (!radacina)
            erori.Add($"rădăcina nu are declarație în schema `{kit.SpatiuNume}`");
        return erori;
    }

    /// <summary>`DUKIntegrator.jar -v` pe kitul pin-uit; `E:` e eroare, restul liniilor sunt atenționări.</summary>
    public static DukRezultat Duk(KitDeclaratie kit, string caleXml, int timeoutSecunde = 120) {
        var anaf = ModelCheck.Duk.GasesteAnaf();
        if (anaf == null)
            return new DukRezultat { Motiv = "directorul `anaf/` cu kitul DUK lipsește (arborele repo-ului sau ATLAS_ANAF)", CaleXml = caleXml };
        var dist = Path.Combine(anaf, "duk_SAFT_an_luna", "dist");
        if (ManifestD406.Abateri(dist, kit.Kit) is { Count: > 0 } abateri)
            return new DukRezultat { Motiv = "kit nepotrivit manifestului: " + string.Join("; ", abateri), CaleXml = caleXml };
        if (ModelCheck.Duk.Java(dist) is not { } java)
            return new DukRezultat { Motiv = $"java lipsește din kit ({dist})", CaleXml = caleXml };
        var caleErori = caleXml + ".err.txt";
        var rulare = ModelCheck.Duk.Ruleaza(java, dist,
            ["-jar", Path.Combine(dist, "DUKIntegrator.jar"), "-v", kit.Tip, caleXml, caleErori], [caleErori],
            timeoutSecunde, caleXml);
        if (rulare.Esec != null)
            return rulare.Esec;
        var linii = ModelCheck.Duk.CitesteLinii(caleErori);
        var ok = linii is [var unic] && unic.Trim().Equals("ok", StringComparison.OrdinalIgnoreCase);
        var erori = new List<string>();
        var avertismente = new List<string>();
        if (!ok) {
            for (var i = 0; i < linii.Count; i++) {
                var mesaj = i + 1 < linii.Count && !Antet(linii[i + 1]) ? $"{linii[i]} {linii[++i].Trim()}" : linii[i];
                (mesaj.StartsWith("E:", StringComparison.Ordinal) ? erori : avertismente).Add(mesaj);
            }
            if (linii.Count == 0)
                erori.Add($"(fișierul de erori lipsește sau e gol) ieșire: {ModelCheck.Duk.Scurt(rulare.Iesire)}");
        }
        var dupa = ManifestD406.Abateri(dist, kit.Kit);
        erori.AddRange(dupa.Select(a => $"kitul s-a schimbat în timpul rulării: {a}"));
        return new DukRezultat {
            Disponibil = true, Valid = erori.Count == 0 && dupa.Count == 0, Versiune = kit.VersiuneValidator,
            Erori = erori, Avertismente = avertismente, Comanda = rulare.Comanda, CaleXml = caleXml,
        };
    }

    static bool Antet(string linie) => linie.Length > 1 && linie[1] == ':' && char.IsAsciiLetterUpper(linie[0]);
}
