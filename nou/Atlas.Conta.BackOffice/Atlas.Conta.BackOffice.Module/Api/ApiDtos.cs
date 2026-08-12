using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Motor;

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

// Un copil al grupului conex, cât să-l poți deschide și eticheta. PARTAJAT
// (felia trezoreriei): grupul conex e mecanism de bază (decizia 17), nu al unui
// tip anume — FCT îl folosește pentru NIR + plata autogenerată, trezoreria
// pentru simetria affordance-urilor. Proiecția care îl umple stă în
// `ApiProiectii.Copii` — o singură dată, ca și tipul.
public sealed class DocumentCopilDto {
    public Guid Id { get; set; }
    // Codul ancorei `TipDocument` (NIR, PLT…) — vocabularul de rutare al
    // clientului (`/nir/{id}`), nu numele clasei CLR.
    public string Tip { get; set; }
    public string Numar { get; set; }
    public string Stare { get; set; }
    public bool Autogenerat { get; set; }
}

// Traducerea enum ↔ sârmă, în ADAPTOR, o dată (nota de la începutul fișierului):
// pe sârmă valorile de enum sunt STRING-uri, iar intrarea vine de la un client
// care poate greși. Refuzul e de DOMENIU, cu valorile valide enumerate — nu o
// `ArgumentException` de infrastructură și nici o conversie tăcută la 0.
internal static class ApiEnum {
    // Header-ul de trezorerie: `TipInstrument` NU e nullable pe model, iar
    // absența din payload înseamnă „instrumentul obișnuit" (F3-D1).
    public static TipInstrumentPlata TipInstrument(string valoare) =>
        TipInstrumentOptional(valoare) ?? TipInstrumentPlata.OrdinPlata;

    // Varianta pentru câmpurile NULLABLE (`FacturaIntrare.PlataTipInstrument`):
    // acolo null e o valoare cu înțeles propriu — parametrul plății autogenerate
    // n-a fost cules, iar `GenereazaSecundar` aplică el default-ul. Parse-ul e
    // același; doar tratarea absenței diferă.
    public static TipInstrumentPlata? TipInstrumentOptional(string valoare) {
        if (string.IsNullOrWhiteSpace(valoare))
            return null;
        var cerut = valoare.Trim();
        foreach (var nume in Enum.GetNames<TipInstrumentPlata>())
            if (string.Equals(nume, cerut, StringComparison.OrdinalIgnoreCase))
                return Enum.Parse<TipInstrumentPlata>(nume);
        // Potrivirea e pe NUME, nu prin `Enum.TryParse`: acela ar accepta și
        // numărul („1"), adică exact contractul fragil pe care string-ul îl evită.
        throw new OperareException(
            $"Instrumentul de plată „{valoare}” nu există — valorile acceptate: "
            + string.Join(", ", Enum.GetNames<TipInstrumentPlata>()) + ".");
    }

    // Direcția liniei de LDI (F6-D5), parsată la GRANIȚĂ — înaintea oricărui
    // `CreateObject`. Spre deosebire de `TipInstrument`, absența NU are default:
    // `DirectieDiferenta` n-are membru 0 tocmai ca linia culeasă fără direcție
    // să nu treacă drept ceva (28e), iar direcția decide TOATĂ semantica liniei
    // în `LdiApply` (ce câmpuri se golesc, dacă `LotId` se aplică, cum se
    // materializează valoarea). O linie fără direcție ar fi oricum ne-operabilă;
    // o refuzăm de la culegere, cu valorile valide enumerate.
    public static DirectieDiferenta Directie(string valoare) {
        if (!string.IsNullOrWhiteSpace(valoare)) {
            var cerut = valoare.Trim();
            foreach (var nume in Enum.GetNames<DirectieDiferenta>())
                if (string.Equals(nume, cerut, StringComparison.OrdinalIgnoreCase))
                    return Enum.Parse<DirectieDiferenta>(nume);
        }
        throw new OperareException(
            $"Direcția liniei de inventar „{valoare}” nu există — valorile acceptate: "
            + string.Join(", ", Enum.GetNames<DirectieDiferenta>()) + ".");
    }
}
