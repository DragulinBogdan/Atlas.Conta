namespace Atlas.Conta.BackOffice.Module.Api;

// DTO-urile PARTAJATE între feliile verticale (decizia 42d / spike D1): tot ce
// nu ține de un tip anume de document trăiește aici, ca feliile să nu-și
// redeclare fiecare propriul contract de comandă.
//
// De ce DTO peste `OperareRezultat` (record-ul motorului): pe sârmă starea e
// STRING, nu enum. Serializarea implicită a unui enum e numărul — un contract
// care s-ar rupe tăcut la reordonarea membrilor (`StareDocument`), și pe care
// TypeScript-ul l-ar tipa `number`. Traducerea se face în adaptor, o dată.

// Rezultatul unei comenzi (POST .../opereaza|anuleaza|storneaza).
public sealed record OperareRezultatDto(
    Guid DocumentId,
    string StareNoua,
    Guid? ConexId,
    string[] Mesaje) {
    public static OperareRezultatDto Din(OperareRezultat r) =>
        new(r.DocumentId, r.StareNoua.ToString(), r.ConexId, r.Mesaje.ToArray());
}

// Corpul comenzii de stornare: data la care se scriu rândurile inverse (GATE
// XAF D10 — se CULEGE, nu e „azi" implicit; motorul o primea deja ca parametru).
public sealed class StornoRequestDto {
    public DateOnly Data { get; set; }
}

// Erorile de domeniu, ca DATE (D2): răspunsul lui `POST .../valideaza`
// (dry-run — listă goală = documentul trece toți gardienii) ȘI corpul lui 422
// pentru orice `OperareException` scăpată de un endpoint de scriere. Aceeași
// formă în ambele locuri: clientul are un singur mod de a afișa erorile.
public sealed record EroriDto(string[] Erori) {
    public static EroriDto Din(IEnumerable<string> erori) => new(erori.ToArray());

    // `OperareException.Message` cumulează liniile pe „\n" (convenția motorului
    // și a gardianului de Committing) — pe sârmă devin elemente de listă.
    public static EroriDto DinMesaj(string mesaj) => new((mesaj ?? string.Empty)
        .Split('\n', StringSplitOptions.RemoveEmptyEntries)
        .Select(l => l.Trim())
        .Where(l => l.Length > 0)
        .ToArray());
}
