using System.Globalization;
using System.Text;

namespace Atlas.Conta.BackOffice.Module.BusinessObjects;

// Cele 42 de subdiviziuni ale României (41 de județe + municipiul București) —
// LEGE, nu politică, exact ca `TariUe`: sursa e foaia `ISO3166-1A2 - RO Dept
// Codes` din `RO_SAFT_SchemaDefCod_16.02.2026.xlsx` (r23–64), aceeași listă pe
// care o validează schema SAF-T.
//
// De ce lista stă AICI și nu în seed: `SeedJudete` o consumă ca să scrie
// nomenclatorul `Judet`, dar conversiile (`DupaCodAuto`, `DupaCodCnp`,
// `DupaDenumire`) sunt cerute de conectoare ÎNAINTE de a atinge baza — ANAF
// întoarce indicativul auto, 1C poartă codul din CNP sau denumirea. Funcții
// PURE, fără `IObjectSpace`: se probează în ModelCheck fără scenă.
//
// Ce întorc: codul ISO („RO-CJ") sau `null`. NICIODATĂ o ghicire — un județ
// nerezolvat e un avertisment raportat (D15-D3/D15-D6), nu o valoare pusă „ca
// să treacă".
public static class JudeteRo {
    // Cod ISO 3166-2 · Denumire oficială · indicativ auto · cod de județ din CNP.
    //
    // Codul din CNP urmează ordinea alfabetică DINAINTE de 1981: 1..39 fără
    // Călărași și Giurgiu (județe înființate atunci, care au primit 51 și 52),
    // 40 = București. Sectoarele Capitalei (41–46) nu sunt subdiviziuni ISO și
    // nu se modelează — un CNP cu 41..46 se rezolvă tot pe RO-B, prin
    // `DupaCodCnp`.
    public static readonly IReadOnlyList<(string Cod, string Denumire, string CodAuto, int CodCnp)> Toate = [
        ("RO-AB", "Alba", "AB", 1),
        ("RO-AR", "Arad", "AR", 2),
        ("RO-AG", "Argeș", "AG", 3),
        ("RO-BC", "Bacău", "BC", 4),
        ("RO-BH", "Bihor", "BH", 5),
        ("RO-BN", "Bistrița-Năsăud", "BN", 6),
        ("RO-BT", "Botoșani", "BT", 7),
        ("RO-BV", "Brașov", "BV", 8),
        ("RO-BR", "Brăila", "BR", 9),
        ("RO-BZ", "Buzău", "BZ", 10),
        ("RO-CS", "Caraș-Severin", "CS", 11),
        ("RO-CL", "Călărași", "CL", 51),
        ("RO-CJ", "Cluj", "CJ", 12),
        ("RO-CT", "Constanța", "CT", 13),
        ("RO-CV", "Covasna", "CV", 14),
        ("RO-DB", "Dâmbovița", "DB", 15),
        ("RO-DJ", "Dolj", "DJ", 16),
        ("RO-GL", "Galați", "GL", 17),
        ("RO-GR", "Giurgiu", "GR", 52),
        ("RO-GJ", "Gorj", "GJ", 18),
        ("RO-HR", "Harghita", "HR", 19),
        ("RO-HD", "Hunedoara", "HD", 20),
        ("RO-IL", "Ialomița", "IL", 21),
        ("RO-IS", "Iași", "IS", 22),
        ("RO-IF", "Ilfov", "IF", 23),
        ("RO-MM", "Maramureș", "MM", 24),
        ("RO-MH", "Mehedinți", "MH", 25),
        ("RO-MS", "Mureș", "MS", 26),
        ("RO-NT", "Neamț", "NT", 27),
        ("RO-OT", "Olt", "OT", 28),
        ("RO-PH", "Prahova", "PH", 29),
        ("RO-SM", "Satu Mare", "SM", 30),
        ("RO-SJ", "Sălaj", "SJ", 31),
        ("RO-SB", "Sibiu", "SB", 32),
        ("RO-SV", "Suceava", "SV", 33),
        ("RO-TR", "Teleorman", "TR", 34),
        ("RO-TM", "Timiș", "TM", 35),
        ("RO-TL", "Tulcea", "TL", 36),
        ("RO-VS", "Vaslui", "VS", 37),
        ("RO-VL", "Vâlcea", "VL", 38),
        ("RO-VN", "Vrancea", "VN", 39),
        ("RO-B", "București", "B", 40),
    ];

    // Prefixele deja NORMALIZATE (fără diacritice, majuscule) — se taie după
    // pliere, deci acoperă și „JUDEȚUL", și „Judetul". Declarat ÎNAINTEA
    // dicționarelor: inițializatoarele statice rulează în ordine TEXTUALĂ, iar
    // `dupaDenumire` cheamă `Normalizeaza`, care le citește (altfel: `null` la
    // prima atingere a clasei).
    static readonly string[] Prefixe = ["MUNICIPIUL ", "JUDETUL "];

    static readonly Dictionary<string, string> dupaCodAuto =
        Toate.ToDictionary(j => j.CodAuto, j => j.Cod, StringComparer.Ordinal);

    static readonly Dictionary<int, string> dupaCodCnp =
        Toate.ToDictionary(j => j.CodCnp, j => j.Cod);

    static readonly Dictionary<string, string> dupaDenumire =
        Toate.ToDictionary(j => Normalizeaza(j.Denumire), j => j.Cod, StringComparer.Ordinal);

    // Indicativul auto → cod ISO. Sursa: `*cod_JudetAuto` din răspunsul ANAF.
    public static string DupaCodAuto(string codAuto) {
        var cheie = codAuto?.Trim().ToUpperInvariant();
        return string.IsNullOrEmpty(cheie) ? null
            : dupaCodAuto.TryGetValue(cheie, out var cod) ? cod : null;
    }

    // Codul de județ din CNP → cod ISO. Sursa: `CodJudet` din 1C.
    // 41..46 = sectoarele Capitalei: tot București (subdiviziunea ISO e una).
    public static string DupaCodCnp(int codCnp) {
        if (codCnp >= 41 && codCnp <= 46)
            return "RO-B";
        return dupaCodCnp.TryGetValue(codCnp, out var cod) ? cod : null;
    }

    // Denumirea → cod ISO, prin normalizarea de mai jos. Sursa: `Field3` din 1C
    // (denumire liberă, cu sau fără diacritice) și `*denumire_Judet` de la ANAF.
    public static string DupaDenumire(string denumire) {
        var cheie = Normalizeaza(denumire);
        return cheie.Length == 0 ? null
            : dupaDenumire.TryGetValue(cheie, out var cod) ? cod : null;
    }

    // Normalizarea de comparație (D15-D1): trim, MAJUSCULE, fără diacritice,
    // cratimele și spațiile multiple pliate la UN spațiu, prefixele
    // „MUNICIPIUL " / „JUDETUL " tăiate. Așa „Bistrița-Năsăud", „BISTRITA
    // NASAUD" și „Judetul Bistrita - Nasaud" cad pe aceeași cheie, iar
    // „Municipiul Bucureşti" (cu ş-cedilă, grafia veche a bazelor legacy) pe
    // „BUCURESTI".
    //
    // Diacriticele se scot prin descompunere canonică (FormD) + eliminarea
    // semnelor ne-spațiale: acoperă și virgula dedesubt (ș/ț corecte, U+0219/
    // U+021B) și cedila (ş/ţ, U+015F/U+0163) fără tabel de substituții.
    public static string Normalizeaza(string valoare) {
        if (string.IsNullOrWhiteSpace(valoare))
            return "";
        var descompus = valoare.Trim().ToUpperInvariant().Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(descompus.Length);
        var spatiuInAsteptare = false;
        foreach (var ch in descompus) {
            if (CharUnicodeInfo.GetUnicodeCategory(ch) == UnicodeCategory.NonSpacingMark)
                continue;
            if (char.IsWhiteSpace(ch) || ch == '-') {
                spatiuInAsteptare = sb.Length > 0;
                continue;
            }
            if (spatiuInAsteptare) {
                sb.Append(' ');
                spatiuInAsteptare = false;
            }
            sb.Append(ch);
        }
        var text = sb.ToString().Normalize(NormalizationForm.FormC);
        foreach (var prefix in Prefixe)
            if (text.StartsWith(prefix, StringComparison.Ordinal))
                return text[prefix.Length..];
        return text;
    }
}
