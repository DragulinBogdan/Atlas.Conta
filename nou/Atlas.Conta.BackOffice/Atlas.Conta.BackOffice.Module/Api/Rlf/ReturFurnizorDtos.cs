namespace Atlas.Conta.BackOffice.Module.Api.Rlf;

// Felia RLF (F19, track 3): returul la furnizor — scriere + citire + comenzi.
// Marfa se întoarce la furnizor pe LOTUL ORIGINAL: laturi Gestiune → Partener,
// stoc −q, contare `3xx = 401` cu −V și `4426 = 401` cu −TVA.
//
// Structural e BCS + TVA: acelaș detaliu de BAZĂ (`DocumentDetaliu` — retururile
// n-au frunză, DIM-2 nu le-a dat una), aceeași linie „Tip + Lot + Cantitate",
// plus lanțul de TVA la culegere al FCT-ului. De aceea citirea merge DIRECT pe
// `DocumentDetaliu`, fără `as`-cast (F19-D9), iar șablonul de scriere e cel al
// lui BCS cu pasul de TVA intercalat.
//
// ═══ Cele două lucruri proprii feliei ═══
//  1. **Semnarea e a OPERĂRII, nu a culegerii** (F19-D8): pe draft valorile sunt
//     POZITIVE — cifra de pe nota de credit a furnizorului, aia pe care o
//     confruntă operatorul. `ReturFurnizor.PregatesteOperare` le semnează negativ
//     la operare, idempotent prin `Abs` (46e). Un document OPERAT își citește deci
//     cantitățile și valorile SEMNATE (fapta operării) — e oricum read-only.
//  2. **RLF nu absoarbe restul lotului** (F18/F5, `IDocumentCuIesireFiscala`):
//     valoarea liniei e `cantitate × prețul lotului` și ATÂT. Culegerea prezice
//     exact cifra asta — nu consultă soldul valoric al cheii și nu prezice
//     golirea. Reziduul de cenți rămâne pe lot (raportat în SAF-T S), nu cade pe
//     401. Calea de API n-are voie să introducă un al doilea adevăr despre
//     valoarea ieșirii.
// La fel ca BTR/BCS/NIR (și invers față de FCT): FĂRĂ `Numar` — RLF are
// `PoliticaNumerotare` („RLF-") în profilul privat, deci seria e SERVER-OWNED,
// consumată la MATERIALIZARE, în propria operare (F19-D6).

// ── Scriere: agregatul per document (PUT header + linii, 42d) ──────────────
public sealed class RlfWriteDto {
    public DateOnly Data { get; set; }
    // Gestiunea din care iese marfa → furnizorul care o primește înapoi. Tipul
    // laturilor rămâne invariant al OPERĂRII (`ReturFurnizor.ValideazaOperare`);
    // `Aplica` verifică doar existența.
    public Guid PredatorId { get; set; }
    public Guid PrimitorId { get; set; }
    public List<RlfLinieWriteDto> Linii { get; set; } = new();
}

// `Id` null = linie NOUĂ; `Id` cunoscut = actualizare. Liniile existente absente
// din payload se ȘTERG — reconcilierea e server-side, clientul trimite agregatul
// întreg (43c).
public sealed class RlfLinieWriteDto {
    public Guid? Id { get; set; }
    public Guid TipMaterialId { get; set; }
    // Lotul ORIGINAL al intrării. NULLABLE pe draft (ca BTR/BCS): culegerea are
    // voie să fie incompletă, iar `ValideazaOperare` îl cere la operare („returul
    // descarcă lotul original — decizia 13"). Liniile de RLF nu nasc niciodată
    // loturi (nu declară `ILinieCareNasteLot`).
    public Guid? LotId { get; set; }
    // MAGNITUDINEA returnată. Semnul e al operării (28a/46e), deci un `-4` venit
    // dintr-un ReadDto de document operat se normalizează la `4` — vezi
    // `ReturFurnizorApply.ReconciliazaLinii`.
    public decimal Cantitate { get; set; }
    public Guid? TipTvaId { get; set; }
    // Override-ul manual (36a: nota de credit a furnizorului bate rotunjirea
    // noastră). `null` = calculul standard din cotă; o valoare = TVA-ul de pe
    // hârtie. Acceptat doar pe regimurile care postează TVA separat
    // (Normal/TaxareInversă) — ca pe FCT. La operare îl păstrează
    // `pastreazaTvaCules: true`.
    public decimal? ValoareTva { get; set; }
}

// ── Citire: agregatul + affordances ────────────────────────────────────────
public sealed class RlfReadDto {
    public Guid Id { get; set; }
    // Server-owned: seria „RLF-" se consumă la materializare, în propria operare;
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
    // BRUT (Σ Valoare + ValoareTva), ca `Document.Total`: POZITIV pe draft,
    // NEGATIV pe documentul operat — datoria către furnizor scade.
    public decimal Total { get; set; }
    public List<RlfLinieReadDto> Linii { get; set; } = new();

    // Affordances pe RESURSĂ (42e): aceeași sursă ca acțiunile XAF.
    public bool PoateEdita { get; set; }
    public bool PoateOpera { get; set; }
    public bool PoateAnula { get; set; }
    public bool PoateStorna { get; set; }
}

public sealed class RlfLinieReadDto {
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
    // `ReturFurnizor.PregatesteOperare` la operare, din aceeași formulă
    // (`cantitate × prețul lotului` — prețul returului nu se culege niciodată).
    public decimal Valoare { get; set; }
    public decimal ValoareTva { get; set; }
    public Guid? TipTvaId { get; set; }
    public string TipTvaCod { get; set; }
    public string TipTvaDenumire { get; set; }
    public decimal? TipTvaCota { get; set; }
}

// ── Listă: exact coloanele grilei ──────────────────────────────────────────
// Fără coloana `Autogenerat`: RLF nu se generează niciodată automat (nu e țintă
// de `PoliticaConex` și niciun tip nu-l produce ca secundar).
public sealed class RlfListDto {
    public Guid Id { get; set; }
    public string Numar { get; set; }
    public DateOnly Data { get; set; }
    public string Stare { get; set; }
    public string PredatorDenumire { get; set; }
    public string PrimitorDenumire { get; set; }
    public decimal Total { get; set; }
}
