namespace Import1C;

// Maparea cont 1C → cont OMFP ca LISTĂ, nu regulă (decizia 48c).
//
// Contra-exemplul care a decis forma: `43111` s-ar rezolva mecanic pe 4311
// (există, ne-sumator — o regulă s-ar opri mulțumită), dar semantic e 4316
// (CASS). Familia contribuțiilor nu urmează structura sintetică OMFP, iar un
// colaps semantic TĂCUT e mai scump decât ~130 de rânduri de dicționar.
// Fișierul e o sesiune de decizii (`mapari-conturi.csv`, comentat rând cu rând),
// alimentată de raportul pre-flight — sugestiile mecanice NU se aplică automat.
//
// Livrare: `Content` + `CopyToOutputDirectory=PreserveNewest` (csproj), citit de
// lângă executabil. Alegerea față de `EmbeddedResource` (convenția Module-ului
// pentru planurile de conturi) e deliberată: planul e seed — parte din produs;
// dicționarul ăsta e unealtă de import, se editează între rulări de om, și
// trebuie să poată fi citit și corectat în folderul de output fără să fie
// îngropat în asamblare.
sealed class MapariConturi {
    readonly Dictionary<string, string> dictionar = new(StringComparer.Ordinal);

    public record Rand(int Linie, string Cod1C, string ContOmfp, string Comentariu);

    public IReadOnlyList<Rand> Randuri { get; private set; } = [];
    public IReadOnlyDictionary<string, string> Dictionar => dictionar;

    // Normalizarea E parte din contract: `MapeazaCont` taie punctul terminal al
    // codului 1C (`623.` → `623`), deci cheile trebuie normalizate identic la
    // încărcare — altfel rândurile scrise cu punct în CSV (cum sunt cele mai
    // multe: `132.`, `646.`, `765.`) n-ar fi găsite NICIODATĂ, iar mecanica
    // generică ar prelua tăcut exact deciziile pe care CSV-ul le fixează.
    public static string Normalizeaza(string cod) {
        var s = cod?.Trim().TrimEnd('.');
        return string.IsNullOrEmpty(s) ? null : s;
    }

    public static string CaleImplicita() =>
        Path.Combine(AppContext.BaseDirectory, "mapari-conturi.csv");

    public static MapariConturi Incarca(string cale, Action<string> avert) {
        var m = new MapariConturi();
        var randuri = new List<Rand>();
        var linie = 0;
        foreach (var brut in File.ReadLines(cale)) {
            linie++;
            var text = brut.Trim();
            if (text.Length == 0 || text.StartsWith('#'))
                continue;
            // Comentariul poate conține virgule (denumirile de cont le au) —
            // se taie doar primele două câmpuri.
            var parti = text.Split(',', 3);
            if (parti.Length < 2) {
                avert($"mapari-conturi.csv:{linie}: rând fără cont țintă („{text}”) — sărit.");
                continue;
            }
            var cod = Normalizeaza(parti[0]);
            var tinta = parti[1].Trim();
            if (cod == null || tinta.Length == 0) {
                avert($"mapari-conturi.csv:{linie}: cod sau țintă goale — sărit.");
                continue;
            }
            if (m.dictionar.TryGetValue(cod, out var deja)) {
                // Un cod mapat de două ori e o decizie contradictorie, nu o
                // scăpare de formatare: se raportează și rămâne PRIMA (ordinea
                // fișierului = ordinea deciziilor).
                avert($"mapari-conturi.csv:{linie}: codul 1C „{cod}” e deja mapat pe {deja} "
                    + $"— rândul nou ({tinta}) se IGNORĂ.");
                continue;
            }
            m.dictionar[cod] = tinta;
            randuri.Add(new Rand(linie, cod, tinta, parti.Length > 2 ? parti[2].Trim() : null));
        }
        m.Randuri = randuri;
        return m;
    }

    // Întâi dicționarul (un rând bate orice derivare), apoi mecanica generică:
    // codurile 1C sunt PUNCTATE cu altă semantică decât cele legacy (punctul
    // separă cifrele contului sintetic, nu analiticul — `442.6` = 4426 TVA), deci
    // se CONCATENEAZĂ segmentele, iar dacă simbolul rezultat nu există în planul
    // OMFP se taie ultimul segment și se reia (`12125`? nu → `121` da).
    public string Mapeaza(string cod1C, IReadOnlyDictionary<string, Guid> plan) {
        var s = Normalizeaza(cod1C);
        if (s == null)
            return null;
        if (dictionar.TryGetValue(s, out var fortat))
            return plan.ContainsKey(fortat) ? fortat : null;
        var segmente = s.Split('.', StringSplitOptions.RemoveEmptyEntries).ToList();
        while (segmente.Count > 0) {
            var candidat = string.Concat(segmente);
            if (plan.ContainsKey(candidat))
                return candidat;
            segmente.RemoveAt(segmente.Count - 1);
        }
        return null;
    }

    public bool EstePrinDictionar(string cod1C) {
        var s = Normalizeaza(cod1C);
        return s != null && dictionar.ContainsKey(s);
    }
}
