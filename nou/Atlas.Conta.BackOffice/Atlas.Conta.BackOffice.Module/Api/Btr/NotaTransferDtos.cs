namespace Atlas.Conta.BackOffice.Module.Api.Btr;

// Felia verticală BTR (spike D1/D8): DTO-urile de citire și de scriere ale
// notei de transfer. Trăiesc în Module (fără nicio referință ASP.NET) ca să fie
// testabile din ModelCheck — endpoint-urile din host sunt doar transport.
//
// ═══ De ce WriteDto ≠ ReadDto (D8, decizia 42d) ═══
// Diferența nu e stilistică: e granița dintre ce CULEGE operatorul și ce
// DEȚINE serverul. Un DTO unic ar pune pe sârmă câmpuri pe care clientul nu are
// voie să le trimită (`Stare`, `Numar`, `Valoare`, `DataOperare`) — și, mai rău,
// l-ar învăța pe autorul clientului că le poate seta. Aici lipsa lor din
// WriteDto e vizibilă în TIPURI, deci și în TypeScript-ul generat:
//   - `Stare`/`DataOperare` — le schimbă doar motorul (gardianul de Committing
//     le refuză oricum din OS secured);
//   - `Numar` — se consumă din PoliticaNumerotare la MATERIALIZARE (GATE XAF D6);
//   - `Valoare` pe linie — `NotaTransfer.PregatesteOperare` o rescrie la operare
//     (preț lot × cantitate, decizia 04): trimisă de client ar fi minciună.
// Fără TVA/Angajament/dimensiuni: BTR nu le poartă (n-au semantică pe transfer —
// detaliul lui e baza pură, iar DIM-2 nu i-a dat frunză).

// ── Scriere: agregatul per document (PUT header + linii, 42d) ──────────────
public sealed class NotaTransferWriteDto {
    public DateOnly Data { get; set; }
    public Guid PredatorId { get; set; }
    public Guid PrimitorId { get; set; }
    public string NumarPV { get; set; }
    public DateOnly? DataPV { get; set; }
    public List<NotaTransferLinieWriteDto> Linii { get; set; } = new();
}

// `Id` null = linie NOUĂ; `Id` cunoscut = actualizare. Liniile existente care nu
// apar în payload se ȘTERG — reconcilierea e server-side, clientul trimite
// agregatul întreg (43c: formularul ține WriteDto local și îl trimite ca tot).
public sealed class NotaTransferLinieWriteDto {
    public Guid? Id { get; set; }
    public Guid TipMaterialId { get; set; }
    public Guid? LotId { get; set; }
    public decimal Cantitate { get; set; }
}

// ── Citire: agregatul + affordances ────────────────────────────────────────
public sealed class NotaTransferReadDto {
    public Guid Id { get; set; }
    public string Numar { get; set; }
    public DateOnly Data { get; set; }
    // STRING, nu enum (vezi ApiDtos): contractul nu depinde de ordinea membrilor.
    public string Stare { get; set; }
    public DateTime? DataOperare { get; set; }
    public Guid PredatorId { get; set; }
    public string PredatorDenumire { get; set; }
    public Guid PrimitorId { get; set; }
    public string PrimitorDenumire { get; set; }
    public string NumarPV { get; set; }
    public DateOnly? DataPV { get; set; }
    public decimal Total { get; set; }
    public bool Autogenerat { get; set; }
    public Guid? DocumentSursaId { get; set; }
    public List<NotaTransferLinieReadDto> Linii { get; set; } = new();

    // Affordances pe RESURSĂ (decizia 42e): clientul nu re-derivă regulile din
    // `Stare` — le primește. Sursa e aceeași ca a acțiunilor XAF
    // (DocumentOperareController.ActualizeazaDisponibilitatea).
    public bool PoateEdita { get; set; }
    public bool PoateOpera { get; set; }
    public bool PoateAnula { get; set; }
    public bool PoateStorna { get; set; }
}

public sealed class NotaTransferLinieReadDto {
    public Guid Id { get; set; }
    public Guid TipMaterialId { get; set; }
    public string TipMaterialCod { get; set; }
    public string TipMaterialDenumire { get; set; }
    public Guid? LotId { get; set; }
    // Compusă ca `Lot.Eticheta` (produs · dată · preț), dar din câmpuri
    // PROIECTATE PLAT: `Eticheta` e [NotMapped], deci nu traversează SQL-ul.
    public string LotEticheta { get; set; }
    public decimal Cantitate { get; set; }
    public decimal Valoare { get; set; }
}

// ── Listă: exact coloanele grilei (fără linii — N+1 pe `Total` real) ───────
public sealed class NotaTransferListDto {
    public Guid Id { get; set; }
    public string Numar { get; set; }
    public DateOnly Data { get; set; }
    public string Stare { get; set; }
    public string PredatorDenumire { get; set; }
    public string PrimitorDenumire { get; set; }
    public decimal Total { get; set; }
}
