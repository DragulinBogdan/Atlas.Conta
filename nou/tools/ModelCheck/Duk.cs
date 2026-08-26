using System.Diagnostics;

namespace Atlas.Conta.BackOffice.ModelCheck;

// ORACOLUL — validatorul oficial ANAF (DUKIntegrator), rulat OFFLINE peste
// fișierul scenei (felia 16, D16-D5, proba V3).
//
// DE CE un oracol extern: schema D406 nu există ca XSD în kit — validarea e
// compilată în `D406Validator.jar`, iar singurul XSD care se poate citi
// (`Ro_SAFT_Schema_v247_20230306.xsd`, din analiza 1C) e din 2023. Ordinea și
// numele elementelor se pot deci CONFIRMA doar rulând validatorul. Rezultatul
// lui e un fapt măsurat, nu o opinie despre schemă.
//
// Ce NU face: nu blochează suita dacă lipsește (kitul e ~252 MB și e gitignored,
// java vine cu el). Absența se raportează ZGOMOTOS — o linie `SĂRIT` în rezumat
// — fiindcă „proba n-a rulat" și „proba a trecut" sunt lucruri diferite.
public sealed class DukRezultat {
    /// <summary>Kitul și java-ul lui există; altfel `Motiv` spune ce lipsește.</summary>
    public bool Disponibil { get; init; }
    public string Motiv { get; init; }
    /// <summary>Fișierul de erori conține literal `ok` — singurul semnal al validatorului.</summary>
    public bool Valid { get; init; }
    /// <summary>Versiunea validatorului D406 din `config/versiuniCurente.txt` (ex. `J2.2.8`).</summary>
    public string Versiune { get; init; }
    public List<string> Erori { get; init; } = [];
    /// <summary>Atenționările (prefixul `!`): se LISTEAZĂ, nu blochează (§C.3).</summary>
    public List<string> Avertismente { get; init; } = [];
    /// <summary>Comanda exactă, ca proba să fie reproductibilă manual.</summary>
    public string Comanda { get; init; }
    /// <summary>Fișierul validat — păstrat pe disc la eșec.</summary>
    public string CaleXml { get; init; }

    public string Rezumat =>
        !Disponibil ? $"SĂRIT ({Motiv})"
        : Valid ? $"ok ({Versiune}), {Avertismente.Count} atenționări"
        : $"RESPINS ({Versiune}): {Erori.Count} erori";
}

public static class Duk {

    public const string TipDeclaratie = "D406";

    /// <summary>
    /// Rulează validatorul pe `caleXml`. `an`/`luna` merg la
    /// `DUKIntegrator_AnLunaUI.jar` (validatorul corelează tipul declarației cu
    /// perioada — `validateDeclaredPeriod`).
    /// </summary>
    public static DukRezultat Valideaza(string caleXml, int an, int luna, int timeoutSecunde = 300) {
        var dist = GasesteKit();
        if (dist == null)
            return new DukRezultat {
                Motiv = "kitul DUK lipsește (căutat `anaf/duk_SAFT_an_luna/dist` în arborele repo-ului "
                    + "și în variabila de mediu ATLAS_DUK)",
            };
        var java = GasesteJava(dist);
        if (java == null)
            return new DukRezultat { Motiv = $"java lipsește din kit ({dist}\\jre*\\bin\\java.exe)" };
        // Jar-ul cu an/lună e cel care primește parametrii de perioadă; fără el,
        // jar-ul de bază validează la fel, dar fără corelația de perioadă.
        var jar = Path.Combine(dist, "DUKIntegrator_AnLunaUI.jar");
        var cuPerioada = File.Exists(jar);
        if (!cuPerioada)
            jar = Path.Combine(dist, "DUKIntegrator.jar");
        if (!File.Exists(jar))
            return new DukRezultat { Motiv = $"DUKIntegrator.jar lipsește din {dist}" };

        // `!` pe fișierul de erori = atenționările se păstrează separat, în
        // `.wrn.txt`, iar `.err.txt` primește marca `ok`. FĂRĂ prefix ele se
        // PIERD (doc/Instructiuni.txt).
        var caleErori = caleXml + ".err.txt";
        var caleAvertismente = caleXml + ".wrn.txt";
        foreach (var f in new[] { caleErori, caleAvertismente })
            if (File.Exists(f))
                File.Delete(f);

        var argumente = new List<string> {
            "-jar", jar, "-v", TipDeclaratie, caleXml, "!" + caleErori,
        };
        if (cuPerioada) {
            // `$` = „ia valoarea implicită a parametrului opțional".
            argumente.Add("$");
            argumente.Add($"an={an}");
            argumente.Add($"luna={luna:00}");
        }
        // ATENȚIE, măsurat: `-d` (fără auto-update) e documentat pentru modul
        // GRAFIC; pus înaintea lui `-v`, procesul rămâne agățat (fereastră care
        // nu se deschide) până la timeout. În linie de comandă se rulează FĂRĂ.
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
                try { proces.Kill(entireProcessTree: true); } catch { /* deja mort */ }
                return new DukRezultat {
                    Disponibil = true,
                    Motiv = $"validatorul n-a terminat în {timeoutSecunde} s",
                    Comanda = comanda,
                    CaleXml = caleXml,
                    Versiune = Versiunea(dist),
                };
            }
            iesire = (stdout.Result + "\n" + stderr.Result).Trim();
        }

        var erori = CitesteLinii(caleErori);
        var avertismente = CitesteLinii(caleAvertismente);
        var valid = erori.Count == 1 && erori[0].Trim().Equals("ok", StringComparison.OrdinalIgnoreCase);
        if (valid)
            erori.Clear();
        if (erori.Count == 0 && !valid)
            // Nici `ok`, nici erori în fișier: iese la iveală ce a spus procesul.
            erori.Add($"(fișierul de erori lipsește sau e gol) ieșire: {Scurt(iesire)}");
        return new DukRezultat {
            Disponibil = true,
            Valid = valid,
            Versiune = Versiunea(dist),
            Erori = erori,
            Avertismente = avertismente,
            Comanda = comanda,
            CaleXml = caleXml,
        };
    }

    /// <summary>Directorul în care se scriu fișierele probei (păstrate la eșec).</summary>
    public static string DirectorTemporar() {
        var cale = Path.Combine(Path.GetTempPath(), "atlas-saft");
        Directory.CreateDirectory(cale);
        return cale;
    }

    static List<string> CitesteLinii(string cale) {
        if (!File.Exists(cale))
            return [];
        return File.ReadAllLines(cale)
            .Select(l => l.TrimEnd())
            .Where(l => l.Length > 0)
            .ToList();
    }

    static string Scurt(string text) =>
        string.IsNullOrWhiteSpace(text) ? "(goală)"
        : text.Length <= 400 ? text.ReplaceLineEndings(" ")
        : text[..400].ReplaceLineEndings(" ") + "…";

    static string Versiunea(string dist) {
        var cale = Path.Combine(dist, "config", "versiuniCurente.txt");
        if (!File.Exists(cale))
            return "necunoscută";
        var linie = File.ReadAllLines(cale)
            .FirstOrDefault(l => l.StartsWith(TipDeclaratie + ";", StringComparison.Ordinal));
        // Formatul e `D406;J2.2.8;P2.0.1` — versiunea validatorului e a doua.
        return linie?.Split(';').Skip(1).FirstOrDefault() ?? "necunoscută";
    }

    static string GasesteKit() {
        var dinMediu = Environment.GetEnvironmentVariable("ATLAS_DUK");
        if (!string.IsNullOrWhiteSpace(dinMediu) && Directory.Exists(dinMediu))
            return dinMediu;
        var director = new DirectoryInfo(AppContext.BaseDirectory);
        while (director != null) {
            var candidat = Path.Combine(director.FullName, "anaf", "duk_SAFT_an_luna", "dist");
            if (Directory.Exists(candidat))
                return candidat;
            director = director.Parent;
        }
        return null;
    }

    static string GasesteJava(string dist) {
        foreach (var jre in Directory.EnumerateDirectories(dist, "jre*").OrderByDescending(d => d)) {
            var java = Path.Combine(jre, "bin", "java.exe");
            if (File.Exists(java))
                return java;
        }
        return null;
    }
}
