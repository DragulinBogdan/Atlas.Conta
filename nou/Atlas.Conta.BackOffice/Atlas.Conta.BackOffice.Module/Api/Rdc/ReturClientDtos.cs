namespace Atlas.Conta.BackOffice.Module.Api.Rdc;

// Felia RDC (F19, track 3): returul de la client — scriere + citire + comenzi.
// UN SINGUR document cu linii pe DOUĂ ROLURI (design FAZA 1C §7): laturi
// Partener → Gestiune, venitul stornat pe unele linii, marfa care revine pe
// lotul ORIGINAL pe celelalte.
//
// ═══ Rolul liniei e o PREZENȚĂ, nu un enum (riscul 5 al contractului) ═══
// Modelul distinge rolurile prin `LotId`: linia de VENIT n-are lot (Tip de venit,
// natura Serviciu, `Valoare` culeasă, TVA), linia de COST îl are (Tip de stoc,
// cantitate, FĂRĂ TVA). Pe sârmă rolul rămâne exact asta — prezența lui `LotId` —
// iar traducerea în comutator explicit („Venit" / „Marfă returnată") e a
// EDITORULUI, nu a modelului (F19-D12).
// Consecința pe care Apply o impune: pe o linie EXISTENTĂ rolul NU se schimbă.
// Un PUT care scoate `LotId` de pe o linie de cost (sau îl adaugă pe una de
// venit) e REFUZAT explicit, nu convertit tăcut — o conversie ar trebui să fie
// COMPLETĂ (TVA, natura Tipului, valoarea, cantitatea pro-formă), iar o conversie
// pe jumătate ar lăsa în document o linie care nu e niciunul din cele două
// lucruri. Schimbarea de rol se face ștergând linia și culegând-o din nou —
// operație pe care agregatul o exprimă deja (id-ul lipsă din payload = ștergere).
//
// ═══ Cele două totaluri ═══
// `Total` e VIRTUAL pe `ReturClient`: DOAR liniile de venit (brutul care ajustează
// creanța). ReadDto îl oglindește și duce SEPARAT `TotalCost` — ca operatorul să
// nu citească „121" acolo unde documentul valorează „−121 creanță + −30 cost"
// (F19-D9).
//
// Ca RLF: detaliu de BAZĂ (fără frunză ⇒ citire directă, fără `as`-cast), `Numar`
// server-owned (seria „RDC-", F19-D6), semnarea storno lăsată OPERĂRII — pe draft
// cifrele sunt POZITIVE (F19-D8).

// ── Scriere: agregatul per document (PUT header + linii, 42d) ──────────────
public sealed class RdcWriteDto {
    public DateOnly Data { get; set; }
    // Clientul care returnează → gestiunea în care revine marfa. Tipul laturilor
    // rămâne invariant al OPERĂRII; `Aplica` verifică doar existența.
    public Guid PredatorId { get; set; }
    public Guid PrimitorId { get; set; }
    public List<RdcLinieWriteDto> Linii { get; set; } = new();
}

// `Id` null = linie NOUĂ; `Id` cunoscut = actualizare. Liniile existente absente
// din payload se ȘTERG — reconcilierea e server-side, clientul trimite agregatul
// întreg (43c).
public sealed class RdcLinieWriteDto {
    public Guid? Id { get; set; }
    public Guid TipMaterialId { get; set; }
    // ROLUL liniei: null = VENIT, valoare = COST (lotul ORIGINAL al livrării).
    // Pe o linie existentă rolul e IMUABIL — vezi nota de fișier.
    public Guid? LotId { get; set; }
    // MAGNITUDINE. Pe linia de COST = marfa care revine (semnul e al operării).
    // Pe linia de VENIT e pro-formă: 0 devine 1 la operare (32d).
    public decimal Cantitate { get; set; }
    // CULEASĂ, dar DOAR pe linia de VENIT: venitul stornat = prețul de vânzare de
    // pe factura originală. Pe linia de COST se IGNORĂ — costul e al lotului
    // (`|q| × PretUnitar`), nu se culege niciodată.
    public decimal Valoare { get; set; }
    // Numai pe linia de VENIT. Pe linia de COST ele nu intră în model:
    // `Apply` persistă `TipTvaId = null` și `ValoareTva = 0` (F19-D7), oglinda
    // exactă a lui `PregatesteOperare`.
    public Guid? TipTvaId { get; set; }
    // Override-ul manual (36a). `null` = calculul standard din cotă. Acceptat doar
    // pe regimurile care postează TVA separat (Normal/TaxareInversă), ca pe FCT.
    public decimal? ValoareTva { get; set; }
}

// ── Citire: agregatul + cele două totaluri + affordances ───────────────────
public sealed class RdcReadDto {
    public Guid Id { get; set; }
    // Server-owned: seria „RDC-" se consumă la materializare, în propria operare;
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
    // Oglinda lui `ReturClient.Total` (virtual): DOAR liniile de VENIT, brut
    // (Valoare + ValoareTva). Ăsta e și plafonul pe care îl stinge o imperechere
    // (`ReturClient.LiniiCreanta`).
    public decimal Total { get; set; }
    // Σ liniilor cu lot — mișcare internă venit↔stoc, NU creanță. Separat tocmai
    // ca cele două cifre să nu se adune niciodată pe ecran (F19-D9).
    public decimal TotalCost { get; set; }
    public List<RdcLinieReadDto> Linii { get; set; } = new();

    // Affordances pe RESURSĂ (42e): aceeași sursă ca acțiunile XAF.
    public bool PoateEdita { get; set; }
    public bool PoateOpera { get; set; }
    public bool PoateAnula { get; set; }
    public bool PoateStorna { get; set; }
}

public sealed class RdcLinieReadDto {
    public Guid Id { get; set; }
    public Guid TipMaterialId { get; set; }
    public string TipMaterialCod { get; set; }
    public string TipMaterialDenumire { get; set; }
    // Rolul liniei, tot ca PREZENȚĂ (vezi nota de fișier): null ⇒ venit.
    public Guid? LotId { get; set; }
    // Compusă ca `Lot.Eticheta` (produs · dată · preț), dar din câmpuri
    // PROIECTATE PLAT: `Eticheta` e [NotMapped], deci nu traversează SQL-ul.
    public string LotEticheta { get; set; }
    public decimal Cantitate { get; set; }
    public decimal Valoare { get; set; }
    public decimal ValoareTva { get; set; }
    public Guid? TipTvaId { get; set; }
    public string TipTvaCod { get; set; }
    public string TipTvaDenumire { get; set; }
    public decimal? TipTvaCota { get; set; }
}

// ── Listă: exact coloanele grilei ──────────────────────────────────────────
// `Total` = tot DOAR venitul (aceeași definiție ca pe agregat): grila nu are voie
// să arate altă cifră decât detaliul.
public sealed class RdcListDto {
    public Guid Id { get; set; }
    public string Numar { get; set; }
    public DateOnly Data { get; set; }
    public string Stare { get; set; }
    public string PredatorDenumire { get; set; }
    public string PrimitorDenumire { get; set; }
    public decimal Total { get; set; }
    public decimal TotalCost { get; set; }
}
