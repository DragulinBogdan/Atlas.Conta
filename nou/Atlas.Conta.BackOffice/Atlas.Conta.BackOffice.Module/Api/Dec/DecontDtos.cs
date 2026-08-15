namespace Atlas.Conta.BackOffice.Module.Api.Dec;

// Felia DEC (F8): justificarea unui avans / a cheltuielilor unui titular —
// scriere + citire + comenzi. Ridică excluderea F6-D12.
//
// ═══ Ce e PROPRIU tipului față de restul feliilor ═══
//   * POSTAREA EXPLICITĂ PE LINIE (`ILinieCuPostareExplicita`, 32a): cont și
//     repartitor, per latură, toate patru OPȚIONALE. E trăsătura tipului, nu un
//     mecanism generic de override — motorul o consultă înaintea rezolvării
//     declarative (contul liniei bate `SursaCont`) și ca nivel MAXIM al
//     coalesce-ului de dimensiuni. Nerezolvate, cad pe regulă (debit din contul
//     Tipului, credit pe titular cu fallback 542).
//   * CANTITATEA E PRO-FORMĂ (32d): 0 → 1. Din F8-D2 normalizarea se face și la
//     CULEGERE (`Aplica`), nu doar în `PregatesteOperare` — culegerea o arată,
//     nu o face în spate.
//   * LANȚUL DE VALORI (`ILinieCuPretUnitar`, F8-D2): `Valoare`/`ValoareTva` se
//     materializează la culegere din `PretUnitar × Cantitate`, prin ACELAȘI
//     helper ca FCT/FCL (`TvaService`) — deci nu intră în WriteDto.
//   * Laturile: predator = titularul (`Angajat`), primitor = unitatea internă
//     care primește justificarea (`UnitateInterna`/`Gestiune`). Tipul lor rămâne
//     invariant al OPERĂRII (`Decont.ValideazaOperare`).
//
// Ca la BTR/NIR/LDI/BCS/FCL (și invers față de FCT): FĂRĂ `Numar` — DEC are
// `PoliticaNumerotare` („DEC-") în AMBELE profiluri, deci seria e SERVER-OWNED,
// consumată la MATERIALIZARE, în propria operare (F8-D3, GATE XAF D6).
//
// TVA: DEC are `TipTvaImplicit` în ambele profiluri și `PoliticaTva`
// (`Deductibil`, 4426 = 542) DOAR la privat — la bugetar toate regimurile sunt
// Capitalizat (TVA-ul intră în `Valoare`), deci calea de override nici nu se
// deschide. Regulile sunt IDENTICE cu F2/F4, fără variantă a treia.

// ── Scriere: agregatul per document (PUT header + linii, 42d) ──────────────
public sealed class DecontWriteDto {
    public DateOnly Data { get; set; }
    // Titularul (`Angajat`) → unitatea internă care primește justificarea.
    public Guid PredatorId { get; set; }
    public Guid PrimitorId { get; set; }
    // `IDocumentCuPV` — procesul-verbal al decontului (testul bazei 22e).
    public string NumarPV { get; set; }
    public DateOnly? DataPV { get; set; }
    public List<DecontLinieWriteDto> Linii { get; set; } = new();
}

// `Id` null = linie NOUĂ; `Id` cunoscut = actualizare. Liniile existente absente
// din payload se ȘTERG — reconcilierea e server-side, clientul trimite agregatul
// întreg (43c).
public sealed class DecontLinieWriteDto {
    public Guid? Id { get; set; }
    public Guid TipMaterialId { get; set; }
    public string Descriere { get; set; }
    // PRO-FORMĂ: 0 se normalizează la 1 chiar la culegere (F8-D2).
    public decimal Cantitate { get; set; }
    public decimal PretUnitar { get; set; }
    public Guid? TipTvaId { get; set; }
    // Override-ul manual al TVA-ului (36a): bonul justificat bate rotunjirea
    // noastră. Acceptat DOAR pe regimurile care postează TVA separat și
    // niciodată negativ — exact regulile F2/F4.
    public decimal? ValoareTva { get; set; }
    // Dimensiunea frunzei (DIM-2) + angajamentul de pe BAZĂ (testul bazei 22c).
    // Politica bugetară cere clasificație bugetară pe DEC: angajament SAU cod
    // economic (33c) — invariant al OPERĂRII, nu al culegerii.
    public Guid? CodEconomicId { get; set; }
    public Guid? AngajamentId { get; set; }
    // Postarea explicită pe linie — trăsătura PROPRIE a tipului (32a). Toate
    // patru opționale; ce rămâne gol cade pe regula de contare.
    public Guid? ContDebitId { get; set; }
    public Guid? ContCreditId { get; set; }
    public Guid? RepartitorDebitId { get; set; }
    public Guid? RepartitorCreditId { get; set; }
}

// ── Citire: agregatul + affordances ────────────────────────────────────────
public sealed class DecontReadDto {
    public Guid Id { get; set; }
    // Server-owned: seria „DEC-" se consumă la materializare, în propria operare;
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
    public string NumarPV { get; set; }
    public DateOnly? DataPV { get; set; }
    // BRUT (Σ Valoare + ValoareTva) — definiția `Document.Total`, aceeași cifră
    // pe care o stinge `ImperechereService`.
    public decimal Total { get; set; }
    public List<DecontLinieReadDto> Linii { get; set; } = new();

    // Affordances pe RESURSĂ (42e): aceeași sursă ca acțiunile XAF.
    // `PoateAnula`/`PoateStorna` includ STINGERILE (F3-D2/57d): decontul stă pe
    // lanțul avans↔decont↔regularizare (31d/32d), deci imperecherea e cazul
    // NORMAL al tipului, nu o excepție — un affordance care o ignoră ar minți la
    // fiecare decont justificat.
    public bool PoateEdita { get; set; }
    public bool PoateOpera { get; set; }
    public bool PoateAnula { get; set; }
    public bool PoateStorna { get; set; }
}

public sealed class DecontLinieReadDto {
    public Guid Id { get; set; }
    public Guid TipMaterialId { get; set; }
    public string TipMaterialCod { get; set; }
    public string TipMaterialDenumire { get; set; }
    // NULL pe o linie de tip BAZĂ (decont istoric/importat): frunza nu există,
    // deci nici prețul, nici descrierea, nici postarea explicită. Reconcilierea
    // refuză explicit o astfel de linie referită prin `Id`.
    public string Descriere { get; set; }
    public decimal Cantitate { get; set; }
    public decimal? PretUnitar { get; set; }
    // REZULTAT, nu culegere: le materializează `Aplica` la culegere și
    // `PregatesteOperare` la operare, din aceeași formulă (`TvaService`).
    public decimal Valoare { get; set; }
    public decimal ValoareTva { get; set; }
    public Guid? TipTvaId { get; set; }
    public string TipTvaCod { get; set; }
    public string TipTvaDenumire { get; set; }
    public decimal? TipTvaCota { get; set; }
    public Guid? CodEconomicId { get; set; }
    public string CodEconomicCod { get; set; }
    public Guid? AngajamentId { get; set; }
    public string AngajamentCod { get; set; }
    // Postarea explicită, cu etichetele ei read-only (61b: liniile nesalvate
    // își culeg etichetele în client, cele salvate le primesc de aici).
    public Guid? ContDebitId { get; set; }
    public string ContDebitSimbol { get; set; }
    public Guid? ContCreditId { get; set; }
    public string ContCreditSimbol { get; set; }
    public Guid? RepartitorDebitId { get; set; }
    public string RepartitorDebitDenumire { get; set; }
    public Guid? RepartitorCreditId { get; set; }
    public string RepartitorCreditDenumire { get; set; }
}

// ── Listă: exact coloanele grilei ──────────────────────────────────────────
// Fără coloana `Autogenerat`: decontul nu se generează niciodată automat (nu e
// țintă de `PoliticaConex` și niciun tip nu-l produce ca secundar).
public sealed class DecontListDto {
    public Guid Id { get; set; }
    public string Numar { get; set; }
    public DateOnly Data { get; set; }
    public string Stare { get; set; }
    public string PredatorDenumire { get; set; }
    public string PrimitorDenumire { get; set; }
    public decimal Total { get; set; }
}
