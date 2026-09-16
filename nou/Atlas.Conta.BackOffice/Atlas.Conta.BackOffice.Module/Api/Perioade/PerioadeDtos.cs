namespace Atlas.Conta.BackOffice.Module.Api.Perioade;

// Contractul de pe sârmă al perioadelor (F27-D1/D2). Enum-urile pleacă ca STRING
// (numele membrului, convenția 57a): eticheta lizibilă are o singură sursă,
// `metadata.json`.

/// <summary>O verigă a lanțului de perioade.</summary>
public sealed record PerioadaDto(int An, int Luna, bool Inchisa, DateTime? InchisaLa, DateTime? InchisaPrimaOara);

/// <summary>O constatare a verificării de închidere, plată.</summary>
public sealed record ConstatareInchidereDto(string Cheie, string Fel, string Severitate, string Text,
    Guid? ObiectId, string ObiectEticheta);

/// <summary>Corpul comenzii de închidere: cheile constatărilor acceptate conștient.</summary>
public sealed class InchidePerioadaRequestDto {
    public string[] Acceptate { get; set; }
}

/// <summary>Corpul comenzii de redeschidere: motivul, obligatoriu.</summary>
public sealed class RedeschidePerioadaRequestDto {
    public string Motiv { get; set; }
}

/// <summary>Rezultatul unei comenzi de perioadă: starea nouă plus rândul de istoric scris.</summary>
public sealed record InchiderePerioadaRezultatDto(PerioadaDto Perioada, string Fel, DateTime La, string De);
