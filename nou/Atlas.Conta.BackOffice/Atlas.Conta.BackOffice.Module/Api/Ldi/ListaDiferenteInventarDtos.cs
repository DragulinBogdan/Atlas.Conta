namespace Atlas.Conta.BackOffice.Module.Api.Ldi;

// Felia LDI (F6): inventarierea culeasă manual — scriere + citire + comenzi.
// Singurul tip BIDIRECȚIONAL pe loturi: minusul descarcă un lot existent (ca un
// consum), plusul NAȘTE lot nou cu preț de evaluare cules (ca o recepție
// manuală — F5 pe NIR, restanța 53i închisă pe LDI+ la pasul 1 al feliei).
// Direcția explicită de pe linie decide care dintre cele două e.
//
// ═══ Ce e specific față de NIR (șablonul cel mai apropiat) ═══
//   * `Directie` — enumerarea pe SÂRMĂ ca STRING (numele membrului), parsată la
//     graniță ÎNAINTE de orice `CreateObject` (`ApiEnum.Directie`, F6-D5/D7).
//     Nu are default: o linie fără direcție se refuză de la culegere.
//   * `LotId` intră în WriteDto, dar se aplică DOAR pe MINUS (pinul lotului
//     descărcat). Pe PLUS e server-owned — lotul se naște prin
//     `LoturiCulegereService` din `ProdusId`, în gestiunea INVENTARIATĂ
//     (predatorul — hook-ul `GestiuneLoturiCulese`, F6-D2).
//   * `PretEvaluare` înlocuiește `PretUnitar`: e prețul cu care se naște lotul
//     plusului (validarea de operare îl cere pozitiv — 28e).
//   * O singură dimensiune de frunză: `CodEconomicId` (plusul postează pe
//     791/7588, care cere E — DIM-2, inventar §2).
// La fel ca NIR/BTR (și invers față de FCT): FĂRĂ `Numar` — LDI are
// `PoliticaNumerotare` („LDI-") în ambele profiluri, deci seria e SERVER-OWNED,
// consumată la MATERIALIZARE, în propria operare (F6-D4, GATE XAF D6).
// FĂRĂ TVA (F6-D5): LDI n-are `PoliticaTva` în niciun profil.

// ── Scriere: agregatul per document (PUT header + linii, 42d) ──────────────
public sealed class LdiWriteDto {
    public DateOnly Data { get; set; }
    // Gestiunea inventariată → comisia de inventariere (calitatea `Comisie` —
    // decizia 28d). Tipul laturilor se validează abia la operare; `Aplica`
    // verifică doar existența.
    public Guid PredatorId { get; set; }
    public Guid PrimitorId { get; set; }
    public List<LdiLinieWriteDto> Linii { get; set; } = new();
}

// `Id` null = linie NOUĂ; `Id` cunoscut = actualizare. Liniile existente absente
// din payload se ȘTERG — reconcilierea e server-side, clientul trimite agregatul
// întreg (43c).
public sealed class LdiLinieWriteDto {
    public Guid? Id { get; set; }
    // Numele membrului `DirectieDiferenta` („Plus"/„Minus"), nu numărul lui —
    // contractul nu depinde de ordinea membrilor. OBLIGATORIU (vezi antetul).
    public string Directie { get; set; }
    public Guid TipMaterialId { get; set; }
    // Marfa găsită în plus — mecanismul lotului nou (F6-D2), obligatoriu pe PLUS
    // prin validarea de operare („Linia de plus își creează lotul la culegere").
    // Pe MINUS se GOLEȘTE (F6-D3): acolo marfa e a lotului descărcat, iar un
    // produs rămas din starea de plus ar naște lot-artefact pe draft.
    public Guid? ProdusId { get; set; }
    // Pinul lotului descărcat — aplicat DOAR pe MINUS (F6-D5). Pe PLUS lotul e
    // server-owned și valoarea trimisă aici se IGNORĂ: ReadDto îl întoarce (e
    // lotul născut de serviciu), deci round-trip-ul agregatului l-ar retrimite
    // mereu, iar o aplicare oarbă ar re-lega linia la propriul ei lot pe o cale
    // care nu-i aparține.
    public Guid? LotId { get; set; }
    // CULEASĂ POZITIV pe ambele direcții — semnul îl pune operarea
    // (`PregatesteOperare`, 28a). Pe un LDI deja operat linia poartă cantitatea
    // semnată, dar atunci documentul e oricum read-only.
    public decimal Cantitate { get; set; }
    // Prețul cu care se naște lotul plusului. Pe MINUS se golește — evaluarea
    // minusului e a lotului descărcat.
    public decimal? PretEvaluare { get; set; }
    // Atributele lotului plusului, culese pe poziție (inventar 05); motorul le
    // copiază pe Lot la operare (`ILinieCuAtributeLot`). Pe MINUS se golesc.
    public DateOnly? DataExpirare { get; set; }
    public string LotFabricatie { get; set; }
    // Dimensiunea frunzei (DIM-2) + angajamentul de pe BAZĂ (testul bazei 22c).
    public Guid? CodEconomicId { get; set; }
    public Guid? AngajamentId { get; set; }
}

// ── Citire: agregatul + affordances ────────────────────────────────────────
public sealed class LdiReadDto {
    public Guid Id { get; set; }
    // Server-owned: seria „LDI-" se consumă la materializare, în propria operare;
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
    // Efectul NET al inventarului: plusurile pozitiv, minusurile negativ (F6-D6).
    public decimal Total { get; set; }
    public List<LdiLinieReadDto> Linii { get; set; } = new();

    // Affordances pe RESURSĂ (42e): aceeași sursă ca acțiunile XAF.
    public bool PoateEdita { get; set; }
    public bool PoateOpera { get; set; }
    public bool PoateAnula { get; set; }
    public bool PoateStorna { get; set; }
}

public sealed class LdiLinieReadDto {
    public Guid Id { get; set; }
    // Numele membrului („Plus"/„Minus"). NULL pe o linie de tip BAZĂ (LDI
    // istoric/importat): frunza nu există, deci nici direcția — clientul o
    // afișează ca atare, iar reconcilierea refuză o astfel de linie explicit.
    // Fără `LotStrain` (F6-D7): pe LDI direcția conduce singură UI-ul — listele
    // de inventar se culeg întotdeauna manual, n-au clonă conexă.
    public string Directie { get; set; }
    public Guid TipMaterialId { get; set; }
    public string TipMaterialCod { get; set; }
    public string TipMaterialDenumire { get; set; }
    public Guid? ProdusId { get; set; }
    public string ProdusCod { get; set; }
    public string ProdusDenumire { get; set; }
    public Guid? LotId { get; set; }
    public string LotEticheta { get; set; }
    // Pe un LDI OPERAT iese SEMNATĂ — e fapta operării (28a), nu o culegere.
    public decimal Cantitate { get; set; }
    public decimal? PretEvaluare { get; set; }
    // REZULTAT, nu culegere (GATE 53c): o materializează `Aplica` la culegere și
    // `PregatesteOperare` la operare, din aceeași formulă SEMNATĂ.
    public decimal Valoare { get; set; }
    public DateOnly? DataExpirare { get; set; }
    public string LotFabricatie { get; set; }
    public Guid? CodEconomicId { get; set; }
    public string CodEconomicCod { get; set; }
    public Guid? AngajamentId { get; set; }
    public string AngajamentCod { get; set; }
}

// ── Listă: exact coloanele grilei ──────────────────────────────────────────
// Fără coloana `Autogenerat`: LDI nu se generează niciodată automat.
public sealed class LdiListDto {
    public Guid Id { get; set; }
    public string Numar { get; set; }
    public DateOnly Data { get; set; }
    public string Stare { get; set; }
    public string PredatorDenumire { get; set; }
    public string PrimitorDenumire { get; set; }
    public decimal Total { get; set; }
}
