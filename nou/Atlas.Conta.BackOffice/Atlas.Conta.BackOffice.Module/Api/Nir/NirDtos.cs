namespace Atlas.Conta.BackOffice.Module.Api.Nir;

// Felia NIR (F2-D3): CITIRE + comenzi. Fluxul-ancoră al feliei e
// FCT operată → `ConexId` → clientul deschide `/nir/{id}` → Operează — deci NIR-ul
// n-are nevoie de agregat de scriere: liniile lui vin din clona conexă a facturii
// (motorul, în tranzacția operării sursei). POST/PUT/DELETE pe NIR + `ProdusId`
// pe `NirDetaliu` (NIR-ul cules MANUAL) sunt felie separată, pur aditivă.
//
// Comenzile (opereaza/anuleaza/storneaza/valideaza) nu cer nimic aici: sunt
// agnostice de tip (`OperareApi` + `OperareRezultatDto`/`StornoRequestDto`).

// ── Citire: agregatul + affordances ────────────────────────────────────────
public sealed class NirReadDto {
    public Guid Id { get; set; }
    // Server-owned: NIR ARE politică de numerotare (seria „NIR-"), consumată la
    // MATERIALIZARE, în propria operare (GATE XAF D6) — pe draft e null.
    public string Numar { get; set; }
    public DateOnly Data { get; set; }
    // STRING, nu enum (vezi ApiDtos): contractul nu depinde de ordinea membrilor.
    public string Stare { get; set; }
    public DateTime? DataOperare { get; set; }
    public Guid PredatorId { get; set; }
    public string PredatorDenumire { get; set; }
    public Guid PrimitorId { get; set; }
    public string PrimitorDenumire { get; set; }
    // BRUT (Σ Valoare + ValoareTva), ca `Document.Total`; pe NIR liniile clonate
    // poartă valoarea netă/brută deja decisă de factură (TVA-ul se postează acolo).
    public decimal Total { get; set; }
    public bool Autogenerat { get; set; }
    public Guid? DocumentSursaId { get; set; }
    // Numărul facturii-sursă: eticheta cu care UI-ul arată de unde vine NIR-ul
    // („generat din factura X"), fără un al doilea apel.
    public string DocumentSursaNumar { get; set; }
    public List<NirLinieReadDto> Linii { get; set; } = new();

    // Affordances pe RESURSĂ (42e): aceeași sursă ca acțiunile XAF.
    public bool PoateEdita { get; set; }
    public bool PoateOpera { get; set; }
    public bool PoateAnula { get; set; }
    public bool PoateStorna { get; set; }
}

public sealed class NirLinieReadDto {
    public Guid Id { get; set; }
    public Guid TipMaterialId { get; set; }
    public string TipMaterialCod { get; set; }
    public string TipMaterialDenumire { get; set; }
    // Recepția e PE LOT (decizia 13): fiecare linie referă lotul născut la
    // culegerea facturii, finalizat de motor la operarea ei.
    public Guid? LotId { get; set; }
    public string LotEticheta { get; set; }
    public decimal Cantitate { get; set; }
    public decimal Valoare { get; set; }
    public decimal ValoareTva { get; set; }
    public Guid? TipTvaId { get; set; }
    public string TipTvaCod { get; set; }
    // Dimensiunile frunzei NIR (DIM-2), primite prin contractul
    // DimensiuniCulese/PreiaDimensiuni de la linia de factură.
    public Guid? CodEconomicId { get; set; }
    public string CodEconomicCod { get; set; }
    public Guid? SursaFinantareId { get; set; }
    public string SursaFinantareCod { get; set; }
    public Guid? CodFunctionalId { get; set; }
    public string CodFunctionalCod { get; set; }
    public Guid? ProiectId { get; set; }
    public string ProiectCod { get; set; }
}

// ── Listă: exact coloanele grilei ──────────────────────────────────────────
public sealed class NirListDto {
    public Guid Id { get; set; }
    public string Numar { get; set; }
    public DateOnly Data { get; set; }
    public string Stare { get; set; }
    public string PredatorDenumire { get; set; }
    public string PrimitorDenumire { get; set; }
    // Coloană proprie NIR-ului: majoritatea rândurilor sunt clone conexe, iar
    // grila trebuie să distingă recepția generată de cea culeasă manual.
    public bool Autogenerat { get; set; }
    public decimal Total { get; set; }
}
