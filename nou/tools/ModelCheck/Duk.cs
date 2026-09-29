using System.Diagnostics;
using System.Xml.Linq;

namespace Atlas.Conta.BackOffice.ModelCheck;

/// <summary>Rezultatul DUKIntegrator pe un fișier; kitul absent sau nepotrivit manifestului e respingere.</summary>
public sealed class DukRezultat {
    /// <summary>Validatorul a rulat pe kitul pin-uit; altfel `Motiv`.</summary>
    public bool Disponibil { get; init; }
    public string Motiv { get; init; }
    /// <summary>Fișierul de erori conține literal `ok`, iar kitul e neschimbat după rulare.</summary>
    public bool Valid { get; init; }
    public string Versiune { get; init; } = ManifestD406.VersiuneValidator;
    /// <summary>Perioada din antetul fișierului, transmisă validatorului.</summary>
    public (int An, int Luna) Perioada { get; init; }
    public List<string> Erori { get; init; } = [];
    /// <summary>Atenționările (prefixul `!`) se listează, nu blochează (73).</summary>
    public List<string> Avertismente { get; init; } = [];
    public string Comanda { get; init; }
    public string CaleXml { get; init; }

    public string Rezumat =>
        !Disponibil ? $"INDISPONIBIL ({Motiv})"
        : Valid ? $"ok ({Versiune}), {Avertismente.Count} atenționări"
        : $"RESPINS ({Versiune}): {Erori.Count} erori";
}

public static class Duk {

    public const string TipDeclaratie = "D406";

    /// <summary>
    /// Validează `caleXml` cu `DUKIntegrator_AnLunaUI.jar` pe perioada din antet: `an`/`luna` aleg nomenclatorul
    /// validatorului și nu se compară cu antetul, deci o perioadă dată separat ar putea masca erori (TR-D8 S0).
    /// </summary>
    public static DukRezultat Valideaza(string caleXml, int timeoutSecunde = 300) {
        if (PerioadaDinAntet(caleXml) is not { } perioada)
            return new DukRezultat { Motiv = "antetul nu declară o singură lună (SelectionCriteria PeriodStart = PeriodEnd)", CaleXml = caleXml };
        var (an, luna) = perioada;
        var anaf = GasesteAnaf();
        if (anaf == null)
            return new DukRezultat { Motiv = "directorul `anaf/` cu kitul DUK lipsește (arborele repo-ului sau ATLAS_ANAF)" };
        var dist = Path.Combine(anaf, "duk_SAFT_an_luna", "dist");
        var abateri = ManifestD406.Abateri(dist, ManifestD406.Kit)
            .Concat(ManifestD406.Abateri(anaf, [ManifestD406.Nomenclator])).ToList();
        if (abateri.Count > 0)
            return new DukRezultat { Motiv = "kit nepotrivit manifestului: " + string.Join("; ", abateri) };
        var java = Directory.EnumerateDirectories(dist, "jre*").OrderByDescending(d => d)
            .Select(jre => Path.Combine(jre, "bin", "java.exe")).FirstOrDefault(File.Exists);
        if (java == null)
            return new DukRezultat { Motiv = $"java lipsește din kit ({dist}\\jre*\\bin\\java.exe)" };

        var caleErori = caleXml + ".err.txt";
        var caleAvertismente = caleXml + ".wrn.txt";
        foreach (var f in new[] { caleErori, caleAvertismente })
            File.Delete(f);
        // `!` pune atenționările în `.wrn.txt`, altfel se pierd; `-d` agață procesul în CLI (doc/Instructiuni.txt, 73-r8).
        string[] argumente = [
            "-jar", Path.Combine(dist, "DUKIntegrator_AnLunaUI.jar"), "-v", TipDeclaratie, caleXml, "!" + caleErori,
            "$", $"an={an}", $"luna={luna:00}",
        ];
        var psi = new ProcessStartInfo(java) {
            WorkingDirectory = dist,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };
        foreach (var a in argumente)
            psi.ArgumentList.Add(a);
        var comanda = $"\"{java}\" {string.Join(" ", argumente.Select(a => a.Contains(' ') ? $"\"{a}\"" : a))}";

        string iesire;
        using (var proces = Process.Start(psi)) {
            if (proces == null)
                return new DukRezultat { Motiv = "procesul java n-a pornit", Comanda = comanda };
            var stdout = proces.StandardOutput.ReadToEndAsync();
            var stderr = proces.StandardError.ReadToEndAsync();
            if (!proces.WaitForExit(timeoutSecunde * 1000)) {
                try { proces.Kill(entireProcessTree: true); } catch (InvalidOperationException) { }
                return new DukRezultat { Motiv = $"validatorul n-a terminat în {timeoutSecunde} s", Comanda = comanda, CaleXml = caleXml };
            }
            iesire = (stdout.Result + "\n" + stderr.Result).Trim();
        }

        var erori = CitesteLinii(caleErori);
        var avertismente = CitesteLinii(caleAvertismente);
        var ok = erori is [var unic] && unic.Trim().Equals("ok", StringComparison.OrdinalIgnoreCase);
        if (ok)
            erori.Clear();
        else if (erori.Count == 0)
            erori.Add($"(fișierul de erori lipsește sau e gol) ieșire: {Scurt(iesire)}");
        var dupa = ManifestD406.Abateri(dist, ManifestD406.Kit);
        erori.AddRange(dupa.Select(a => $"kitul s-a schimbat în timpul rulării: {a}"));
        return new DukRezultat {
            Disponibil = true,
            Perioada = perioada,
            Valid = ok && dupa.Count == 0,
            Erori = erori,
            Avertismente = avertismente,
            Comanda = comanda,
            CaleXml = caleXml,
        };
    }

    static (int, int)? PerioadaDinAntet(string caleXml) {
        static XElement Copil(XElement e, string nume) => e?.Elements().FirstOrDefault(x => x.Name.LocalName == nume);
        var criterii = Copil(Copil(XDocument.Load(caleXml).Root, "Header"), "SelectionCriteria");
        int? Valoare(string nume) => int.TryParse((string)Copil(criterii, nume), out var v) ? v : null;
        return (Valoare("PeriodStartYear"), Valoare("PeriodStart"), Valoare("PeriodEndYear"), Valoare("PeriodEnd")) is
            (int an, int luna, int anSfarsit, int lunaSfarsit) && an == anSfarsit && luna == lunaSfarsit ? (an, luna) : null;
    }

    public static string DirectorTemporar() {
        var cale = Path.Combine(Path.GetTempPath(), "atlas-saft");
        Directory.CreateDirectory(cale);
        return cale;
    }

    static List<string> CitesteLinii(string cale) =>
        !File.Exists(cale) ? [] : File.ReadAllLines(cale).Select(l => l.TrimEnd()).Where(l => l.Length > 0).ToList();

    static string Scurt(string text) =>
        string.IsNullOrWhiteSpace(text) ? "(goală)"
        : text.Length <= 400 ? text.ReplaceLineEndings(" ")
        : text[..400].ReplaceLineEndings(" ") + "…";

    static string GasesteAnaf() {
        var dinMediu = Environment.GetEnvironmentVariable("ATLAS_ANAF");
        if (!string.IsNullOrWhiteSpace(dinMediu) && Directory.Exists(dinMediu))
            return dinMediu;
        for (var d = new DirectoryInfo(AppContext.BaseDirectory); d != null; d = d.Parent) {
            var candidat = Path.Combine(d.FullName, "anaf");
            if (Directory.Exists(Path.Combine(candidat, "duk_SAFT_an_luna", "dist")))
                return candidat;
        }
        return null;
    }
}
