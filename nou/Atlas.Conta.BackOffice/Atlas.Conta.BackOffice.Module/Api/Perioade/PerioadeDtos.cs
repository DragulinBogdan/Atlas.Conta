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
public sealed record InchiderePerioadaRezultatDto(PerioadaDto Perioada, string Fel, DateTime La, string De,
    string[] Acceptari);

/// <summary>Un rând al istoricului unei perioade: cine, când, ce fel, cu ce motiv și ce a acceptat.</summary>
public sealed record IstoricPerioadaDto(int An, int Luna, string Fel, DateTime La, string De, string Motiv,
    string[] Acceptari);

/// <summary>Rezultatul reconstrucției soldurilor: un rând per perioadă de referință, chiar fără diferențe.</summary>
public sealed record ReconstructieRezultatDto(ReconstructieReferintaDto[] Referinte);

/// <summary>Cifrele unei perioade de referință: ce era, ce a ieșit din recalcul, cât diferă.</summary>
public sealed record ReconstructieReferintaDto(int An, int Luna,
    long ContabilExistente, long ContabilRecalculate, long ContabilDiferite,
    long StocExistente, long StocRecalculate, long StocDiferite,
    decimal DiferentaDebit, decimal DiferentaCredit, decimal DiferentaCantitate, decimal DiferentaValoare,
    long PartideExistente, long PartideRecalculate, long PartideDiferite, decimal DiferentaRest);
