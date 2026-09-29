using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Xml;
using System.Xml.Schema;
using Atlas.Conta.BackOffice.Module.Saft;

namespace Atlas.Conta.BackOffice.ModelCheck;

/// <summary>Artefactele ANAF contra cărora se certifică un XML D406 (TR-D8 S0).</summary>
public static class ManifestD406 {
    public sealed record Pin(string Cale, string Sha256);

    public const string VersiuneValidator = "J2.2.18";
    public const string SpatiuXsdPublicat = "mfp:anaf:dgti:d406t:declaratie:v1";

    public static readonly Pin Xsd = new("Anaf/Ro_SAFT_Schema_v249_2025.xsd",
        "80AD7EAAF2AAFD656A6E3C0E69E3A8FCDB23262640287EBBA6383FF3014DCCC2");

    public static readonly Pin Nomenclator = new("RO_SAFT_SchemaDefCod_16.02.2026.xlsx",
        "050508BF6EADBD6BB4684A0016296253C629D4E9B54A1118DD9B9BFD0C1D0F2D");

    public static readonly Pin[] Kit = [
        new("DUKIntegrator.jar", "3432F30E41B52DC7DEE7C1DA7BEDF37A3D5BAEA064FC7FDF6322D4C0D1EE7610"),
        new("DUKIntegrator_AnLunaUI.jar", "DEECD4E232CDECEB3345E56F9D28EEBFD0B524B5DB08FF560993CEEFFF4F37D8"),
        new("lib/D406Validator.jar", "197FD169721022C43F8A3AE5BA66950A7DC2354CF1355B0DD012CEADA9A32373"),
        new("lib/DecValidation.jar", "C3599C099179CBD4E39FEF16E2B31869763238682EF6CBD3A9DD9D2D9B11798D"),
        new("lib/Validator.jar", "57384EE185CEF279BD0592BB7C5061FFCB423CB15A0DCF05D5D23A230ABAD1B0"),
    ];

    public static string Sha256(string cale) {
        using var f = File.OpenRead(cale);
        return Convert.ToHexString(SHA256.HashData(f));
    }

    public static string Sha256(byte[] octeti) => Convert.ToHexString(SHA256.HashData(octeti));

    /// <summary>Pin-urile nepotrivite din `radacina`; lista goală = kitul e cel pin-uit.</summary>
    public static List<string> Abateri(string radacina, IEnumerable<Pin> pinuri) => pinuri
        .Select(p => (p, cale: Path.Combine(radacina, p.Cale)))
        .Select(x => !File.Exists(x.cale) ? $"{x.p.Cale} lipsește"
            : Sha256(x.cale) is var h && h != x.p.Sha256 ? $"{x.p.Cale} are SHA-256 {h}, pin {x.p.Sha256}" : null)
        .Where(m => m != null)
        .ToList();
}

/// <summary>
/// Schema publicată v249 are `targetNamespace` de test (`d406t`); se validează fișierul de producție
/// (`d406`) prin substituția declarată a celor două atribute, cu SHA-256 al schemei derivate în manifest.
/// </summary>
public static class XsdD406 {
    static readonly Lazy<(XmlSchemaSet Set, string ShaDerivat)> Schema = new(Incarca);

    public static string ShaDerivat => Schema.Value.ShaDerivat;

    static (XmlSchemaSet, string) Incarca() {
        var cale = Path.Combine(AppContext.BaseDirectory, ManifestD406.Xsd.Cale);
        if (ManifestD406.Abateri(AppContext.BaseDirectory, [ManifestD406.Xsd]) is [var abatere, ..])
            throw new InvalidOperationException($"XSD D406 nepotrivit manifestului: {abatere}");
        var text = File.ReadAllText(cale, Encoding.UTF8);
        foreach (var atribut in new[] { "targetNamespace", "xmlns:nsSAFT" }) {
            var vechi = $"{atribut}=\"{ManifestD406.SpatiuXsdPublicat}\"";
            if (text.Split(vechi).Length != 2)
                throw new InvalidOperationException($"XSD D406: `{vechi}` nu apare exact o dată");
            text = text.Replace(vechi, $"{atribut}=\"{SaftXml.SpatiuNume}\"");
        }
        var set = new XmlSchemaSet();
        using (var r = XmlReader.Create(new StringReader(text)))
            set.Add(SaftXml.SpatiuNume, r);
        set.Compile();
        return (set, ManifestD406.Sha256(Encoding.UTF8.GetBytes(text)));
    }

    /// <summary>Erorile și avertismentele schemei (avertismentul „fără informație de schemă” e eroare).</summary>
    public static List<string> Valideaza(string caleXml) {
        var erori = new List<string>();
        var setari = new XmlReaderSettings {
            ValidationType = ValidationType.Schema,
            Schemas = Schema.Value.Set,
            ValidationFlags = XmlSchemaValidationFlags.ReportValidationWarnings,
        };
        setari.ValidationEventHandler += (_, e) =>
            erori.Add($"{e.Severity} {e.Exception.LineNumber}:{e.Exception.LinePosition} {e.Message}");
        using var r = XmlReader.Create(caleXml, setari);
        var radacinaValidata = false;
        while (r.Read())
            if (r is { NodeType: XmlNodeType.Element, Depth: 0 })
                radacinaValidata = r.SchemaInfo?.SchemaElement != null;
        if (!radacinaValidata)
            erori.Add($"rădăcina nu are declarație în schema `{SaftXml.SpatiuNume}`");
        return erori;
    }

    public static XmlSchemaElement Element(params string[] cale) {
        var set = Schema.Value.Set;
        XmlSchemaParticle Particula(XmlSchemaType tip) => tip is XmlSchemaComplexType c ? c.ContentTypeParticle : null;
        var curent = (XmlSchemaElement)set.GlobalElements[new XmlQualifiedName(cale[0], SaftXml.SpatiuNume)];
        foreach (var nume in cale.Skip(1))
            curent = Copii(Particula(curent?.ElementSchemaType)).FirstOrDefault(e => e.Name == nume);
        return curent;
    }

    static IEnumerable<XmlSchemaElement> Copii(XmlSchemaParticle p) => p switch {
        XmlSchemaElement e => [e],
        XmlSchemaGroupBase g => g.Items.OfType<XmlSchemaParticle>().SelectMany(Copii),
        _ => [],
    };
}

/// <summary>Linia GL a fișierului legată de cheia completă a postării din cub (S1-D2).</summary>
public sealed record LegaturaGl(string TransactionID, string RecordID, Atlas.Conta.Nucleu.Spatiu? Spatiu, Guid PostareId,
    Guid DocumentId, Guid? LinieId);

/// <summary>Factura fișierului legată de evenimentul ei: documentul, stornoul și tranzacția cubului (S1-D2).</summary>
public sealed record LegaturaFactura(string Sectiune, string InvoiceNo, string InvoiceType, string TransactionID,
    Guid DocumentId, bool Storno);

/// <summary>Linia de plată a fișierului legată de postările și transferurile care o justifică, și de ținta ei (S2-D3).</summary>
public sealed record LegaturaPlata(string TransactionID, int LineNumber, Guid DocumentId, bool Storno,
    Guid? TintaDocumentId, List<SaftSursa> Surse);

public sealed record ProvenientaD406(List<LegaturaGl> Gl, List<LegaturaFactura> Facturi, List<LegaturaPlata> Plati) {
    public static ProvenientaD406 Din(SaftDto dto) => new(
        dto.Jurnale.SelectMany(j => j.Tranzactii).SelectMany(t => t.Linii.Select(l =>
            new LegaturaGl(t.TransactionID, l.RecordID, l.Spatiu, l.RandRegistruId, t.DocumentId, l.DetaliuId))).ToList(),
        dto.FacturiEmise.Select(f => ("SalesInvoices", f)).Concat(dto.FacturiPrimite.Select(f => ("PurchaseInvoices", f)))
            .Select(x => new LegaturaFactura(x.Item1, x.f.InvoiceNo, x.f.InvoiceType, x.f.TransactionID, x.f.DocumentId, x.f.Storno))
            .ToList(),
        dto.Plati.SelectMany(p => p.Linii.Select(l =>
            new LegaturaPlata(p.TransactionID, l.LineNumber, p.DocumentId, p.Storno, l.TintaDocumentId, l.Surse))).ToList());
}

/// <summary>O validare XSD + DUK a unui fișier, cu intrarea ei în manifestul rulării.</summary>
public sealed record ValidareD406(string Fisier, string Sha256, int An, int Luna,
    List<string> EroriXsd, DukRezultat Duk, ProvenientaD406 Provenienta) {
    public bool Valid => EroriXsd.Count == 0 && Duk.Valid;

    /// <summary>`dto` e declarația din care s-a scris exact `caleXml`; fără el, fișierul n-are proveniență.</summary>
    public static ValidareD406 Ruleaza(string caleXml, SaftDto dto = null) {
        var xsd = XsdD406.Valideaza(caleXml);
        var duk = global::Atlas.Conta.BackOffice.ModelCheck.Duk.Valideaza(caleXml);
        return new(Path.GetFileName(caleXml), ManifestD406.Sha256(caleXml), duk.Perioada.An, duk.Perioada.Luna, xsd, duk,
            dto == null ? null : ProvenientaD406.Din(dto));
    }

    public static readonly JsonSerializerOptions Json = new() {
        WriteIndented = true,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() },
    };

    /// <summary>Proveniența fișierului `fisier`, citită înapoi din manifestul scris.</summary>
    public static ProvenientaD406 CitesteProvenienta(string caleManifest, string fisier) {
        using var doc = JsonDocument.Parse(File.ReadAllText(caleManifest));
        var intrare = doc.RootElement.GetProperty("fisiere").EnumerateArray()
            .Single(f => f.GetProperty("Fisier").GetString() == fisier);
        return intrare.GetProperty("Provenienta").Deserialize<ProvenientaD406>(Json);
    }

    public string Rezumat => $"XSD {(EroriXsd.Count == 0 ? "ok" : $"{EroriXsd.Count} erori")}, DUK {Duk.Rezumat}";

    /// <summary>Scrie `manifest-d406.json` lângă fișierele validate și întoarce calea.</summary>
    public static string ScrieManifest(string director, IEnumerable<ValidareD406> validari) {
        var cale = Path.Combine(director, "manifest-d406.json");
        var manifest = new {
            scrisLa = DateTime.UtcNow,
            spatiuNume = SaftXml.SpatiuNume,
            xsd = new {
                ManifestD406.Xsd.Cale, ManifestD406.Xsd.Sha256,
                spatiuPublicat = ManifestD406.SpatiuXsdPublicat,
                substitutie = $"targetNamespace, xmlns:nsSAFT: {ManifestD406.SpatiuXsdPublicat} → {SaftXml.SpatiuNume}",
                shaDerivat = XsdD406.ShaDerivat,
            },
            validator = new { versiune = ManifestD406.VersiuneValidator, kit = ManifestD406.Kit },
            nomenclator = ManifestD406.Nomenclator,
            fisiere = validari.Select(v => new {
                v.Fisier, v.Sha256, v.An, v.Luna, v.Valid, v.EroriXsd,
                duk = new { v.Duk.Rezumat, v.Duk.Erori, v.Duk.Avertismente, v.Duk.Comanda },
                v.Provenienta,
            }),
        };
        File.WriteAllText(cale, JsonSerializer.Serialize(manifest, Json));
        return cale;
    }
}
