using System.Collections;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Encodings.Web;
using System.Text.Json;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Saft;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.BaseImpl.EF;

namespace Atlas.Conta.BackOffice.ModelCheck;

// D10 — dump-ul de metadata pentru clientul React (decizia 43d: „codegen =
// tipuri, nu clienți"; artefactele se COMIT, unealta verifică drift-ul, exact
// disciplina migrațiilor EF).
//
// Ce emite: captions (`[XafDisplayName]`, fallback = numele membrului),
// `DefaultProperty` per tip (display-ul lookup-urilor — 43f) și label-urile de
// enum. Ce NU emite: nimic care ar cere un `XafApplication` viu.
//
// ═══ Limita ASUMATĂ (D10) ═══
// Sursa e REFLECȚIA pe assembly-ul Module, nu Application Model-ul XAF: diff-urile
// `.xafml` (Model Editor) NU intră în dump. `CaptionHelper` — calea completă —
// cere `XafApplication`, pe care ModelCheck nu-l bootează. Consecința practică:
// un caption schimbat DOAR din Model Editor nu ajunge în client. Calea completă
// (dump dintr-un host viu) e amânată; până atunci captions-urile canonice ale
// clientului sunt cele din ATRIBUTE — locul unde oricum trăiesc azi toate.
static class MetadataDump {
    // Calea implicită, RELATIVĂ LA DIRECTORUL PROIECTULUI `tools/ModelCheck`
    // (nu la cwd — `dotnet run` nu garantează cwd-ul). Ancorarea se face pe
    // `ModelCheck.csproj` găsit urcând din directorul binarului; ca plasă de
    // siguranță (publish, bin mutat) rămâne calea sursei la compilare.
    public const string CaleRelativaImplicita = "../../Atlas.Conta.Client/src/generated/metadata.json";

    const string SpatiuBusinessObjects = "Atlas.Conta.BackOffice.Module.BusinessObjects";

    public static string CaleImplicita() =>
        Path.GetFullPath(Path.Combine(DirectorProiect(), CaleRelativaImplicita));

    static string DirectorProiect([CallerFilePath] string sursa = null) {
        DirectoryInfo dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null) {
            if (File.Exists(Path.Combine(dir.FullName, "ModelCheck.csproj")))
                return dir.FullName;
            dir = dir.Parent;
        }
        return Path.GetDirectoryName(sursa);
    }

    // JSON stabil: chei sortate ordinal, indentat, diacriticele NEescapate
    // (fișierul e citit de om în diff-uri, nu doar de `JSON.parse`).
    static readonly JsonSerializerOptions Optiuni = new() {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    public static string Genereaza() {
        var assembly = typeof(Document).Assembly;
        var tipuri = new SortedDictionary<string, TipMeta>(StringComparer.Ordinal);
        // Enumurile păstrează ordinea de DECLARAȚIE a membrilor (review F3-D7b),
        // nu alfabetică: SelectBox-ul clientului le arată în ordinea din C#
        // (default-ul primul), nu cu default-ul rătăcit alfabetic. Cheia externă
        // (numele enum-ului) rămâne sortată ordinal — determinism pentru drift.
        var enumuri = new SortedDictionary<string, Dictionary<string, string>>(StringComparer.Ordinal);

        foreach (var t in assembly.GetTypes().Where(EsteRelevant)) {
            if (t.IsEnum)
                enumuri[t.Name] = LabeluriEnum(t);
            else
                tipuri[t.Name] = new TipMeta(DefaultProperty(t), Membri(t));
        }

        return JsonSerializer.Serialize(new DumpMeta(enumuri, Nomenclatoare(), tipuri), Optiuni)
            .ReplaceLineEndings("\n") + "\n";
    }

    // ── A treia secțiune: `Nomenclatoare` (felia 20, F20-D6) ───────────────
    //
    // Liste care sunt LEGE ÎN COD (`static readonly` pe tipuri C#, deciziile
    // 73d/74b), deci invizibile pe OData, pe REST și în `openapi.json` — dar de
    // care clientul are nevoie ca să compună un `SelectBox` sau să traducă un
    // cod în denumire. NU devin entități: motivația scrisă în
    // `SaftReguli.cs:311-316` („lista e a legii, nu variază per client") rămâne.
    //
    // DECLARATĂ EXPLICIT, nu prin reflecție pe câmpuri statice: prezența în dump
    // e o decizie per listă (ca `[TipDetaliu]`), nu un efect colateral al
    // faptului că cineva a scris `public static readonly`. Ordinea cheilor e cea
    // de mai jos — `Dictionary` păstrează ordinea de inserție la serializare,
    // deci dump-ul e determinist fără sortare.
    static Dictionary<string, object> Nomenclatoare() => new() {
        // `TaxAccountingBasis` din antetul SAF-T: cele 12 planuri ANAF.
        ["BazeContabile"] = Societate.BazeContabileDescrise
            .Select(b => new CodDescriere(b.Cod, b.Descriere)).ToArray(),
        // Cele 19 tipuri de mișcare de stoc admise de D406 S.
        ["CoduriMiscare"] = SaftReguli.CoduriMiscare
            .Select(c => new CodDenumire(c.Key, c.Value)).ToArray(),
        // Tabelul de normalizare al coloanei generate `Cautare` (F20-D1): TS-ul
        // normalizează literalul tastat cu ACELAȘI tabel, nu cu o copie.
        ["Cautare"] = new TabelCautare(Cautare.De, Cautare.La),
    };

    sealed record CodDescriere(string Cod, string Descriere);
    sealed record CodDenumire(string Cod, string Denumire);
    sealed record TabelCautare(string De, string La);

    static bool EsteRelevant(Type t) {
        if (!t.IsPublic || t.Namespace == null)
            return false;
        if (!t.Namespace.StartsWith(SpatiuBusinessObjects, StringComparison.Ordinal))
            return false;
        return t.IsEnum || (t.IsClass && typeof(BaseObject).IsAssignableFrom(t));
    }

    // `DefaultProperty` = display-ul lookup-urilor (43f). Se caută pe ierarhie
    // (`inherit: true`): derivatele de document îl moștenesc de la bază.
    static string DefaultProperty(Type t) =>
        (Attribute.GetCustomAttribute(t, typeof(XafDefaultPropertyAttribute), inherit: true)
            as XafDefaultPropertyAttribute)?.Name
        ?? (Attribute.GetCustomAttribute(t, typeof(DefaultPropertyAttribute), inherit: true)
            as DefaultPropertyAttribute)?.Name;

    // Plumbing-ul XAF/EF: niciodată câmp de formular, doar zgomot în diff.
    static readonly string[] MembriTehnici = { "GCRecord", "OptimisticLockField" };

    // Membrii se emit APLATIZAȚI (declarați + moșteniți): clientul cere
    // `useCampMeta("NotaTransfer", "Data")` și primește caption-ul de pe baza
    // `Document` fără să urce el ierarhia în TypeScript.
    static SortedDictionary<string, string> Membri(Type t) {
        var proprietati = t.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var dupaNume = proprietati.GroupBy(p => p.Name).ToDictionary(g => g.Key, g => g.First(), StringComparer.Ordinal);
        var membri = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (var p in proprietati) {
            if (p.GetIndexParameters().Length > 0 || p.GetMethod == null)
                continue;
            if (MembriTehnici.Contains(p.Name))
                continue;
            // Colecțiile de navigație nu sunt câmpuri de formular; grilele de
            // linii își iau captions-urile de pe TIPUL liniei.
            if (p.PropertyType != typeof(string) && typeof(IEnumerable).IsAssignableFrom(p.PropertyType))
                continue;
            membri[p.Name] = Caption(p) ?? CaptionNavigatiePereche(p, dupaNume) ?? p.Name;
        }
        return membri;
    }

    // Convenția FK `{Nav}Id` + navigație pereche (aceeași cu `HideForeignKeys`,
    // 41b): caption-ul trăiește pe NAVIGAȚIE, dar DTO-ul poartă FK-ul —
    // `useCampMeta("NotaTransfer", "PredatorId")` trebuie să dea „Predator (de
    // la)", nu „PredatorId". Fără asta clientul ar trebui să știe el convenția.
    static string CaptionNavigatiePereche(PropertyInfo fk, Dictionary<string, PropertyInfo> dupaNume) {
        if (!fk.Name.EndsWith("Id", StringComparison.Ordinal) || fk.Name.Length <= 2)
            return null;
        var tipFk = Nullable.GetUnderlyingType(fk.PropertyType) ?? fk.PropertyType;
        if (tipFk != typeof(Guid) && tipFk != typeof(int) && tipFk != typeof(long))
            return null;
        if (!dupaNume.TryGetValue(fk.Name[..^2], out var navigatie) || navigatie.PropertyType.IsValueType
            || navigatie.PropertyType == typeof(string))
            return null;
        return Caption(navigatie) ?? navigatie.Name;
    }

    static string Caption(MemberInfo m) =>
        (Attribute.GetCustomAttribute(m, typeof(XafDisplayNameAttribute), inherit: true)
            as XafDisplayNameAttribute)?.DisplayName;

    // Ordinea de DECLARAȚIE (după valoarea numerică — determinist, spre deosebire
    // de ordinea reflectată a `GetFields`); `Dictionary` păstrează ordinea de
    // inserție la serializare (System.Text.Json).
    static Dictionary<string, string> LabeluriEnum(Type t) {
        var labeluri = new Dictionary<string, string>();
        foreach (var valoare in Enum.GetValues(t)) {
            var nume = Enum.GetName(t, valoare);
            var camp = t.GetField(nume);
            labeluri[nume] = Caption(camp) ?? nume;
        }
        return labeluri;
    }

    sealed record DumpMeta(
        SortedDictionary<string, Dictionary<string, string>> Enumuri,
        Dictionary<string, object> Nomenclatoare,
        SortedDictionary<string, TipMeta> Tipuri);

    sealed record TipMeta(string DefaultProperty, SortedDictionary<string, string> Membri);

    public static void Scrie(string cale) {
        Directory.CreateDirectory(Path.GetDirectoryName(cale));
        File.WriteAllText(cale, Genereaza());
    }

    // Drift (43d): canonic e fișierul COMIS. Dacă lipsește, verificarea nu are
    // ce compara și se raportează ca „absent" — nu ca eșec (clientul poate lipsi
    // dintr-un checkout parțial).
    public static (bool exista, bool identic) VerificaDrift(string cale) {
        if (!File.Exists(cale))
            return (false, true);
        var comis = File.ReadAllText(cale).ReplaceLineEndings("\n");
        return (true, comis == Genereaza());
    }
}
