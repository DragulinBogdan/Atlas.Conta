namespace Atlas.Conta.BackOffice.Module.Api.Implicite;

// Felia 23 (F23-D6) — implicitul de culegere, ca RĂSPUNS.
//
// Endpoint-ul nu decide nimic: pune aceeași întrebare pe care o pun cele cinci
// Apply-uri la PUT (`ImpliciteService.TipTva`) și o ambalează. Serverul și
// clientul nu pot diverge fiindcă amândoi cheamă o singură funcție — clientul
// PRECOMPLETEAZĂ perechea (id, etichetă) și AFIȘEAZĂ sursa, nu recalculează
// nimic (43b: zero motor de reguli în TS).
//
// De ce DTO-ul poartă și codul, și denumirea, și cota: precompletarea din client
// scrie perechea (id, etichetă) prin convenția 77c, iar eticheta trebuie să vină
// din același răspuns — altfel ar fi urmat un al doilea drum (lookup pe OData
// după id), care poate întoarce altceva sau nimic pentru cine n-are drept pe
// nomenclator. Cota e informativă (indiciul de sub câmp), nu se recalculează cu
// ea nimic: TVA-ul îl calculează motorul.
public sealed class RezultatImplicitDto {
    // `null` = nu există implicit pentru contextul cerut. NU e o eroare: e
    // răspunsul onest al unui profil care n-are ancoră pe tipul ăsta (BTR,
    // NTC…), iar `Motiv` spune de ce.
    public Guid? TipTvaId { get; set; }
    public string TipTvaCod { get; set; }
    public string TipTvaDenumire { get; set; }
    public decimal? Cota { get; set; }

    // Numele enum-ului `SursaImplicit`, pe sârmă ca STRING (convenția feliilor
    // 57a/63): `Niciuna`, `Partener`, `Produs`, `Politica`, `Ancora`. Clientul
    // are eticheta lizibilă în `metadata.json` (`[XafDisplayName]` pe enum), deci
    // o singură sursă pentru XAF și React.
    public string Sursa { get; set; }

    // Fraza care explică verdictul, gata formată de `ImpliciteService`: notele
    // adunate pe drum (partenerul care nu se vede, tipul inactiv sărit) plus
    // verdictul. Se afișează ca indiciu sub câmp — de aceea e text, nu cod: e
    // adresată operatorului, nu unei ramuri de TS.
    public string Motiv { get; set; }
}
