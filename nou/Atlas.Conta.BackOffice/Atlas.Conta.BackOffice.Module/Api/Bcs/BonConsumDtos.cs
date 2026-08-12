namespace Atlas.Conta.BackOffice.Module.Api.Bcs;

// Felia BCS (F6): consumul cules manual — scriere + citire + comenzi. Clona
// șablonului BTR: același detaliu (baza pură — DIM-2 nu i-a dat frunză), aceleași
// trei rute, aceeași linie „Tip + Lot + Cantitate".
//
// ═══ Ce e DIFERIT față de WriteDto-ul BTR (diferențele sunt vizibile în TIPURI) ═══
//   * FĂRĂ `NumarPV`/`DataPV`: BCS nu e `IDocumentCuPV` (procesul-verbal e al
//     transferului). Câmpurile nici nu există pe model.
//   * Laturile au alte roluri: predator = GESTIUNEA care dă marfa, primitor =
//     LOCUL DE CONSUM (calitatea transversală `LocConsum` — decizia 27b, nu o
//     clasă). Tipul lor rămâne invariant al OPERĂRII, ca peste tot.
// La fel ca BTR (și invers față de FCT): FĂRĂ `Numar` — BCS are
// `PoliticaNumerotare` („BCS-") în ambele profiluri, deci seria e SERVER-OWNED,
// consumată la MATERIALIZARE, în propria operare (GATE XAF D6).
// FĂRĂ TVA (F6-D5): BCS n-are `PoliticaTva` în niciun profil, deci pasul TVA al
// motorului nu se declanșează pe el — un TVA cules aici ar fi cifră moartă.

// ── Scriere: agregatul per document (PUT header + linii, 42d) ──────────────
public sealed class BcsWriteDto {
    public DateOnly Data { get; set; }
    // Gestiunea → locul de consum; tipul laturilor se validează abia la operare
    // (`BonConsum.ValideazaOperare`) — un draft are voie să fie incomplet până
    // atunci, `Aplica` verifică doar existența.
    public Guid PredatorId { get; set; }
    public Guid PrimitorId { get; set; }
    public List<BcsLinieWriteDto> Linii { get; set; } = new();
}

// `Id` null = linie NOUĂ; `Id` cunoscut = actualizare. Liniile existente absente
// din payload se ȘTERG — reconcilierea e server-side, clientul trimite agregatul
// întreg (43c).
public sealed class BcsLinieWriteDto {
    public Guid? Id { get; set; }
    public Guid TipMaterialId { get; set; }
    // Lotul descărcat. NULLABLE pe draft (ca BTR): culegerea are voie să fie
    // incompletă, iar `ValideazaOperare` îl cere la operare („descărcarea e pe
    // lot — decizia 13"). Liniile de BCS nu nasc niciodată loturi.
    public Guid? LotId { get; set; }
    public decimal Cantitate { get; set; }
}

// ── Citire: agregatul + affordances ────────────────────────────────────────
public sealed class BcsReadDto {
    public Guid Id { get; set; }
    // Server-owned: seria „BCS-" se consumă la materializare, în propria operare;
    // pe draft e null.
    public string Numar { get; set; }
    public DateOnly Data { get; set; }
    // STRING, nu enum (vezi ApiDtos): contractul nu depinde de ordinea membrilor.
    public string Stare { get; set; }
    public DateTime? DataOperare { get; set; }
    public Guid PredatorId { get; set; }
    public string PredatorDenumire { get; set; }
    public Guid PrimitorId { get; set; }
    public string PrimitorDenumire { get; set; }
    public decimal Total { get; set; }
    public List<BcsLinieReadDto> Linii { get; set; } = new();

    // Affordances pe RESURSĂ (42e): aceeași sursă ca acțiunile XAF.
    public bool PoateEdita { get; set; }
    public bool PoateOpera { get; set; }
    public bool PoateAnula { get; set; }
    public bool PoateStorna { get; set; }
}

public sealed class BcsLinieReadDto {
    public Guid Id { get; set; }
    public Guid TipMaterialId { get; set; }
    public string TipMaterialCod { get; set; }
    public string TipMaterialDenumire { get; set; }
    public Guid? LotId { get; set; }
    // Compusă ca `Lot.Eticheta` (produs · dată · preț), dar din câmpuri
    // PROIECTATE PLAT: `Eticheta` e [NotMapped], deci nu traversează SQL-ul.
    public string LotEticheta { get; set; }
    public decimal Cantitate { get; set; }
    // REZULTAT, nu culegere (GATE 53c): o materializează `Aplica` la culegere și
    // `BonConsum.PregatesteOperare` la operare, din aceeași formulă (preț lot ×
    // cantitate — prețul consumului nu se culege niciodată).
    public decimal Valoare { get; set; }
}

// ── Listă: exact coloanele grilei ──────────────────────────────────────────
// Fără coloana `Autogenerat`: BCS nu se generează niciodată automat (nu e țintă
// de `PoliticaConex` și niciun tip nu-l produce ca secundar).
public sealed class BcsListDto {
    public Guid Id { get; set; }
    public string Numar { get; set; }
    public DateOnly Data { get; set; }
    public string Stare { get; set; }
    public string PredatorDenumire { get; set; }
    public string PrimitorDenumire { get; set; }
    public decimal Total { get; set; }
}
