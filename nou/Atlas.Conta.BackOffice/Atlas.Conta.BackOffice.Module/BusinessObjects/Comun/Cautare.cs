using System.Text;

namespace Atlas.Conta.BackOffice.Module.BusinessObjects;

// CĂUTAREA FĂRĂ DIACRITICE (felia 20, F20-D1) — o singură definiție a
// normalizării, consumată în TREI locuri: SQL-ul coloanei generate
// (`ExpresieSql`, chemat din `OnModelCreating` și, prin el, din migrație),
// oracolul din ModelCheck și clientul React (tabelul se publică în
// `metadata.json`, F20-D6, ca TS-ul să nu-l copieze).
//
// De ce coloană GENERATĂ și nu colație / `unaccent` (tranșat în contract):
//   * colație ICU nedeterministă — `contains` al OData se traduce prin
//     `LIKE`/`strpos`, al căror suport pe colații nedeterministe e recent și
//     parțial, iar colația ar schimba și semantica egalității și a indexurilor
//     unice existente (`Cod`, `Simbol`);
//   * `unaccent(...)` în jurul coloanei — filtrul îl compune `ODataStore` în
//     client și EF îl traduce mecanic; nimeni nu poate injecta o funcție în
//     jurul coloanei fără să intercepteze query-ul OData. În plus `unaccent`
//     e STABLE (dicționar de text search), deci nici n-ar putea sta într-o
//     coloană generată.
//   * `translate` ȘI `lower` sunt IMMUTABLE în Postgres (`pg_proc.provolatile
//     = 'i'`, verificat pe 18.4), deci expresia de mai jos e legală într-un
//     `GENERATED ALWAYS AS (...) STORED`. `concat()` NU e (e STABLE) — de-aia
//     concatenarea se scrie cu `||` + `coalesce`, nu cu `concat`.
//
// O SINGURĂ coloană acoperă căutarea pe cod ȘI pe denumire: operatorul tastează
// oricare dintre ele, iar `contains` peste concatenare le prinde pe amândouă.
public static class Cautare {
    // Perechile de normalizare, poziție cu poziție (`De[i]` → `La[i]`).
    //
    // Doar MINUSCULE: expresia aplică `lower()` ÎNAINTE de `translate`, deci
    // „Ș" a devenit deja „ș" când se face înlocuirea. (Postgres `lower()` pe
    // colația `en_US.utf8` și `string.ToLowerInvariant()` din C# coincid pe tot
    // setul de mai jos — divergența clasică e „I" turcesc, care nu apare aici.)
    //
    // Românești: ambele grafii — cea CORECTĂ (virguliță, U+0219/U+021B) și cea
    // veche cu sedilă (U+015F/U+0163), care umple nomenclatoarele importate.
    // Latine uzuale în denumiri de parteneri străini: setul restrâns fixat în
    // contract. Nu intră mapările 1→n (`ß` → `ss`): `translate` e strict
    // caracter-la-caracter, iar un `La` mai scurt ȘTERGE caracterul în loc
    // să-l înlocuiască.
    public const string De = "ăâîșşțţéèêëáàäöüçñ";
    public const string La = "aaisstteeeeaaaoucn";

    // Gardianul tabelului: lungimi egale (altfel Postgres ar ȘTERGE tăcut
    // caracterele fără corespondent) și fără dubluri în `De` (a doua apariție
    // ar fi ignorată de `translate`, dar ar minți în C#). Rulează la prima
    // atingere a clasei — inclusiv la construirea modelului EF.
    static Cautare() {
        if (De.Length != La.Length)
            throw new InvalidOperationException(
                $"Cautare.De ({De.Length}) și Cautare.La ({La.Length}) trebuie să aibă aceeași lungime: " +
                "`translate` mapează caracter-la-caracter, iar un `La` mai scurt ȘTERGE restul.");
        if (De.Distinct().Count() != De.Length)
            throw new InvalidOperationException("Cautare.De are caractere duplicate — maparea ar fi ambiguă.");
    }

    /// <summary>
    /// Aceeași transformare ca expresia SQL, în memorie: `lower` + `translate`.
    /// Sursa oracolului din ModelCheck (SQL-ul și C#-ul spun același lucru) și
    /// a normalizării literalului tastat, în client.
    /// </summary>
    public static string Normalizeaza(string text) {
        if (string.IsNullOrEmpty(text))
            return text ?? "";
        var mic = text.ToLowerInvariant();
        var sb = new StringBuilder(mic.Length);
        foreach (var c in mic) {
            var i = De.IndexOf(c);
            sb.Append(i >= 0 ? La[i] : c);
        }
        return sb.ToString();
    }

    /// <summary>
    /// Textul căutabil al unui rând, din perechea (cod, denumire) — jumătatea
    /// C# a expresiei generate. `null` se comportă ca `coalesce(..., '')`.
    /// </summary>
    public static string Compune(string cod, string denumire) =>
        Normalizeaza((cod ?? "") + " " + (denumire ?? ""));

    /// <summary>
    /// SQL-ul coloanei generate. <paramref name="coloanaCod"/> poate fi
    /// <c>null</c> pentru entitățile fără cod (atunci rămâne doar denumirea,
    /// tot cu spațiul din față — ca forma din C# să fie identică).
    /// </summary>
    public static string ExpresieSql(string coloanaCod, string coloanaDenumire) {
        var cod = coloanaCod == null ? "''" : $"coalesce(\"{coloanaCod}\", '')";
        var denumire = $"coalesce(\"{coloanaDenumire}\", '')";
        return $"translate(lower({cod} || ' ' || {denumire}), '{De}', '{La}')";
    }

    /// <summary>Numele proprietății/coloanei — o singură ortografie.</summary>
    public const string NumeColoana = "Cautare";
}

/// <summary>
/// Nomenclatorul căutabil fără diacritice (F20-D1): coloana generată
/// <see cref="Cautare"/> = `lower` + `translate` peste (cod, denumire).
/// <para>
/// Declarativă: prezența interfeței e TOT ce cere configurarea generică din
/// <c>BackOfficeEFCoreDbContext.AplicaColoanaCautare</c> — numele coloanei de
/// cod se deduce din entitate (`Cod`, altfel `Simbol`), iar sub TPT coloana se
/// așază pe tipul care DECLARĂ proprietatea (deci pe tabelul bazei
/// <c>Repartitor</c>, o singură coloană pentru toate frunzele).
/// </para>
/// <para>
/// Get-only pe contract: valoarea e a BAZEI de date. Proprietatea concretă are
/// setter public (ca tot restul modelului — proxy-urile de change-tracking le
/// cer virtuale), dar EF o marchează `ValueGeneratedOnAddOrUpdate`, deci nu o
/// scrie niciodată: un PATCH pe ea e ignorat, nu refuzat.
/// </para>
/// </summary>
public interface ICuCautare {
    string Cautare { get; }
}
