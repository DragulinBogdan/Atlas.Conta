namespace Atlas.Conta.BackOffice.Module.Api.Trz;

// STINGEREA prin API (contract p5-felia-trz, F3-D3). Trăiește în felia
// trezoreriei fiindcă acolo e stingătorul „clasic" (31d), dar resursa e a
// DOCUMENTULUI: panoul de stingeri apare la fel pe FCT, pe PLT/INC și (când va
// intra prin API) pe nota de compensare.
//
// ═══ Ce e ALTFEL față de celelalte felii ═══
//   * `Imperechere` NU e document: n-are Draft/Operat, deci n-are agregat de
//     reconciliat și nici comenzi. Contractul are exact trei verbe: CREEAZĂ,
//     ȘTERGE (liber — 31d), CITEȘTE panoul. Nu există UPDATE: gardianul UI
//     refuză deja Edit (41d), iar re-validarea sumei ar cere excluderea
//     propriului rând din plafon.
//   * Invarianții nu se rescriu aici: `ImperechereService.ValideazaCreare` e
//     singura autoritate (o singură sursă de reguli — 42a). Apply traduce doar
//     GRANIȚA: chei → entități, cu mesaje de domeniu.

// ── Scriere: legătura, cu documentele date prin CHEI (42b) ─────────────────
public sealed class ImperechereWriteDto {
    // Documentul care STINGE (plata/încasarea; nota de compensare pe calea
    // XAF). Rolul e polimorf — îl declară `Document.CapacitateStingere` (48b),
    // deci contractul acceptă orice id de document, iar refuzul vine din
    // serviciu, cu mesajul lui.
    public Guid DocumentStingatorId { get; set; }
    // Documentul STINS (factura, decontul, avansul aflat pe rolul de stins).
    public Guid DocumentId { get; set; }
    public decimal Suma { get; set; }
}

// Legătura creată, plată. Numerele de rest NU sunt aici: după creare clientul
// reîncarcă panoul (`Stingeri`) al documentului pe care lucrează — un singur
// loc care spune cât a mai rămas, pentru ambele părți.
public sealed class ImperechereReadDto {
    public Guid Id { get; set; }
    public Guid DocumentStingatorId { get; set; }
    public Guid DocumentId { get; set; }
    public decimal Suma { get; set; }
    public bool Autogenerat { get; set; }
}

// ── Citire: panoul de stingeri al unui document (un singur apel) ───────────
public sealed class StingeriDto {
    public Guid DocumentId { get; set; }
    // Cele trei numere vin din `ImperechereService` — sursa de adevăr a
    // stingerii (`Total` trece prin `LiniiCreanta`, deci ReturClient dă brutul
    // de venit, nu Σ tuturor liniilor). TypeScript-ul NU le recalculează (42c).
    public decimal Total { get; set; }
    public decimal Asignat { get; set; }
    public decimal Ramas { get; set; }
    public List<StingereRandDto> Imperecheri { get; set; } = new();
}

// Un rând al panoului, văzut DIN documentul interogat: `EsteStingator` spune pe
// ce rol stă el, iar `Celalalt*` descrie partea opusă — un document poate
// apărea pe AMBELE roluri (avansul din lanțul avans↔decont↔regularizare, 31d),
// caz în care panoul lui are rânduri cu `EsteStingator` diferit.
public sealed class StingereRandDto {
    public Guid Id { get; set; }
    public bool EsteStingator { get; set; }
    public Guid CelalaltDocumentId { get; set; }
    // Codul ancorei `TipDocument` (FCT, PLT…) — vocabularul de rutare al
    // clientului (`/fct/{id}`), ca la `DocumentCopilDto`.
    public string CelalaltTip { get; set; }
    public string CelalaltNumar { get; set; }
    public decimal Suma { get; set; }
    public bool Autogenerat { get; set; }
}
