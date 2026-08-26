using System.Globalization;
using System.Text;

namespace Atlas.Conta.BackOffice.Module.BusinessObjects;

// PUNTEA de la unitățile de măsură scrise LIBER (legacy, 1C, `Produs.UM`) la
// codurile UN/ECE pe care le cere SAF-T (felia 16, D16-D2).
//
// De ce e nevoie: `Produs.UM` e un string cules de om („buc", „BUC.", „bucată",
// „kg", „mp"). SAF-T cere `UOMBase`/`UOMStandard` din nomenclatorul UN/ECE, iar
// un cod inventat e respins la validare. Puntea are DOUĂ trepte:
//   1. e o grafie românească uzuală? — dicționarul de mai jos;
//   2. e chiar un cod UN/ECE? („KGM", „kgm", „h87") — se ia ca atare.
// Altfel `null` — NICIODATĂ o ghicire. Un produs nerezolvat e un avertisment
// agregat în fișier (`FaraUnitateMasura` + `H87`), nu o valoare pusă „ca să
// treacă": `H87` ales de generator SPUNE că a ales, dicționarul care ar ghici
// n-ar spune nimic.
//
// Funcție PURĂ, fără `IObjectSpace`: migrația și conectorul o folosesc înainte
// de a atinge baza, iar D16-V1 o probează fără scenă (ca `JudeteRo`).
//
// Dicționarul e DELIBERAT scurt. Nu e o listă de sinonime „cât mai completă":
// fiecare intrare e o grafie văzută în datele reale (legacy/1C) sau abrevierea
// standard românească. Ce nu e aici se rezolvă prin corectarea nomenclatorului,
// nu prin lărgirea punții — altfel „t" ar trebui să însemne simultan tonă și
// timp, iar puntea ar începe să inventeze.
public static class UnitatiMasuraRo {
    // Grafia NORMALIZATĂ (trim, minuscule, fără diacritice) → cod UN/ECE.
    // Punctul final se taie la normalizare, deci „buc." intră pe „buc".
    static readonly Dictionary<string, string> dupaGrafie = new(StringComparer.Ordinal) {
        ["buc"] = "H87",
        ["bucata"] = "H87",     // acoperă și „bucată" (diacriticele cad la normalizare)
        ["bucati"] = "H87",
        ["kg"] = "KGM",
        ["kilogram"] = "KGM",
        ["l"] = "LTR",
        ["litru"] = "LTR",
        ["litri"] = "LTR",
        ["m"] = "MTR",
        ["metru"] = "MTR",
        // `ml` NU e în dicționar, DELIBERAT: în grafia românească înseamnă și
        // „metri liniari" (MTR) și „mililitru" (MLT), iar `ML` nu e cod UN/ECE,
        // deci nimic nu tranșează. Un cod ghicit ar trece de validator și ar fi
        // fals — rămâne nerezolvat, cu avertisment, până îl alege omul.
        ["mp"] = "MTK",
        ["m2"] = "MTK",
        ["mc"] = "MTQ",
        ["m3"] = "MTQ",
        ["ora"] = "HUR",
        ["ore"] = "HUR",
        ["h"] = "HUR",
        ["set"] = "SET",
        ["to"] = "TNE",
        ["tona"] = "TNE",
        ["tone"] = "TNE",
        ["cm"] = "CMT",
        ["g"] = "GRM",
        ["gram"] = "GRM",
        ["pereche"] = "PR",
        ["zi"] = "DAY",
        ["zile"] = "DAY",
        ["luna"] = "MON",
        ["luni"] = "MON",
    };

    // Codurile UN/ECE indexate pe forma normalizată, pentru treapta 2: „KGM",
    // „kgm" și „ Kgm " cad pe același cod. Construit din `UnitatiMasuraUnEce`,
    // nu scris cu mâna — lista e sursa.
    static readonly Dictionary<string, string> dupaCod = UnitatiMasuraUnEce.Toate
        .ToDictionary(u => Normalizeaza(u.Cod), u => u.Cod, StringComparer.Ordinal);

    // DE CE dicționarul de grafii e ÎNAINTEA codurilor — o constatare MĂSURATĂ
    // peste cele 2.163 de coduri, nu o preferință. Doar DOUĂ chei ale
    // dicționarului sunt și coduri UN/ECE:
    //   · „set" → codul `SET` = „set"        — același rezultat, indiferent de ordine;
    //   · „mc"  → codul `MC`  = „microgram"  — DIFERIT, și fatal pe date reale.
    // „mc" într-un nomenclator românesc de produse înseamnă metru cub, nu
    // microgram, în 100% din cazuri. Cu ordinea inversă („codul întâi") fiecare
    // produs vândut la metru cub ar fi ieșit în fișier ca microgram — o valoare
    // sintactic VALIDĂ, deci trecută de validator, și semantic falsă: exact
    // genul de eroare pe care n-o prinde nimeni.
    //
    // Dicționarul rămâne mărginit tocmai ca să poată avea prioritate: 30 de
    // grafii curate, dintre care 2 coliziuni cunoscute. Un cod UN/ECE tastat de
    // om („KGM", „H87") nu e în dicționar, deci ajunge intact la treapta 2.
    public static string Rezolva(string um) {
        var cheie = Normalizeaza(um);
        if (cheie.Length == 0)
            return null;
        return dupaGrafie.TryGetValue(cheie, out var din) ? din
            : dupaCod.TryGetValue(cheie, out var cod) ? cod
            : null;
    }

    // Normalizarea de comparație: trim, MINUSCULE, fără diacritice, fără spații
    // interioare și fără puncte („buc.", „m. p.", „BUCĂȚI" cad pe o cheie
    // fiecare). Aceeași mecanică de descompunere canonică (FormD + eliminarea
    // semnelor ne-spațiale) ca `JudeteRo.Normalizeaza` — acoperă și ș/ț cu
    // virgulă, și cele cu cedilă din bazele legacy.
    public static string Normalizeaza(string valoare) {
        if (string.IsNullOrWhiteSpace(valoare))
            return "";
        var descompus = valoare.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(descompus.Length);
        foreach (var ch in descompus) {
            if (CharUnicodeInfo.GetUnicodeCategory(ch) == UnicodeCategory.NonSpacingMark)
                continue;
            if (char.IsWhiteSpace(ch) || ch == '.')
                continue;
            sb.Append(ch);
        }
        return sb.ToString().Normalize(NormalizationForm.FormC);
    }
}
