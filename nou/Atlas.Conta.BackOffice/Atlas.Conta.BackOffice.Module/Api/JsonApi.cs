using System.Text.Json;

namespace Atlas.Conta.BackOffice.Module.Api;

/// <summary>
/// Opțiunile de serializare JSON ale tierului API — O SINGURĂ definiție, ca
/// proba din ModelCheck să serializeze exact ce serializează host-ul.
/// </summary>
/// <remarks>
/// Nu e o preferință de stil: coliziunea `PartenerID`/`PartenerId` de pe
/// `SaftFactura` a ieșit 500 pe calea reală (V4, felia 16) fiindcă nimic din
/// suită nu serializa DTO-urile — iar o probă cu ALTE opțiuni decât ale
/// host-ului ar fi ratat-o la fel de tăcut (numele conflictuale depind de
/// `PropertyNamingPolicy` și de potrivirea insensibilă la caz din
/// `JsonSerializerDefaults.Web`).
/// <para>
/// Baza e `JsonSerializerDefaults.Web` fiindcă ASA își construiește ASP.NET Core
/// `Mvc.JsonOptions.JsonSerializerOptions`; singura abatere e
/// `PropertyNamingPolicy = null` (numele CLR EXACTE pe sârmă — moștenit din
/// scaffold-ul XAF, ca exemplele Swagger să fie valide, și pin-uit de codegen:
/// `api-types.ts` are `Rezumat`, nu `rezumat`).
/// </para>
/// Enum-urile NU au nevoie de convertor: pe DTO-urile feliilor pleacă deja ca
/// `string` (57a), scrise de proiecții cu `.ToString()`.
/// </remarks>
public static class JsonApi {
    /// <summary>
    /// Abaterile tierului de la `JsonSerializerDefaults.Web`. Apelată de host pe
    /// `Mvc.JsonOptions.JsonSerializerOptions` (care PORNEȘTE de la Web) și de
    /// <see cref="Optiuni"/>.
    /// </summary>
    public static void Configureaza(JsonSerializerOptions optiuni) {
        ArgumentNullException.ThrowIfNull(optiuni);
        optiuni.PropertyNamingPolicy = null;
    }

    /// <summary>
    /// Aceleași opțiuni, ca instanță de sine stătătoare — pentru apelanții care
    /// serializează în afara pipeline-ului MVC (ModelCheck, unelte de consolă).
    /// </summary>
    public static JsonSerializerOptions Optiuni { get; } = Creeaza();

    static JsonSerializerOptions Creeaza() {
        var optiuni = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        Configureaza(optiuni);
        // FĂRĂ `MakeReadOnly()`: fără `TypeInfoResolver` explicit, suprasarcina
        // fără parametri ARUNCĂ (măsurat — `TypeInitializationException` la prima
        // atingere a clasei). Instanța devine oricum read-only la prima
        // serializare, care e chiar contractul dorit.
        return optiuni;
    }
}
