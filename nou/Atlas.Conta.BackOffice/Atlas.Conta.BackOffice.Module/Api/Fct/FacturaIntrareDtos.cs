namespace Atlas.Conta.BackOffice.Module.Api.Fct;

// Felia verticală FCT (contract p5-felia-fct, F2-D2/D5): DTO-urile facturii de
// intrare. Trăiesc în Module (fără nicio referință ASP.NET) — endpoint-urile din
// host sunt transport, iar ModelCheck exersează exact acest cod.
//
// ═══ Ce exersează felia asta și BTR nu a atins ═══
//   * `Numar` INTRĂ în WriteDto: FCT poartă numărul FURNIZORULUI (nu are
//     PoliticaNumerotare — `ValideazaOperare` îl cere explicit). Pe BTR era
//     server-owned; aici e câmp cules, iar diferența e vizibilă în TIPURI.
//   * `ProdusId` pe linie = mecanismul prin care LOTUL SE NAȘTE la culegere
//     (GATE XAF D1 / decizia 25c). Clientul NU trimite `LotId` — lotul e
//     server-owned pe FCT: îl creează/sincronizează `LoturiCulegereService` la
//     `Aplica`, îl finalizează motorul la operare.
//   * `PretUnitar` + `TipTvaId` → `Valoare`/`ValoareTva` materializate LA
//     CULEGERE (GATE 53c): serverul e autorul lanțului de valori, clientul dă
//     doar baza. `Valoare` lipsește din WriteDto tocmai pentru că e rezultat.
//   * `ValoareTva?` e SINGURA excepție: null = calculul standard, valoare =
//     override-ul operatorului (factura furnizorului bate rotunjirea noastră —
//     regula 36a), aplicat DUPĂ calcul, ca în fluxul UI.
//   * dimensiunile FRUNZEI (DIM-2): patru FK-uri pe `FacturaIntrareDetaliu`,
//     clonate pe NIR-ul conex prin contractul `DimensiuniCulese/PreiaDimensiuni`.
//
// EXCLUSE deliberat din WriteDto: `GenereazaPlata`/`Plata*` (felia trezoreriei),
// `TethysId` (import), `Chitanta*` (câmpuri moarte — 31e).

// ── Scriere: agregatul per document (PUT header + linii, 42d) ──────────────
public sealed class FacturaIntrareWriteDto {
    // Numărul FURNIZORULUI — cules, nu generat (FCT n-are politică de numerotare).
    public string Numar { get; set; }
    public DateOnly Data { get; set; }
    public Guid PredatorId { get; set; }
    public Guid PrimitorId { get; set; }
    public DateOnly? DataScadenta { get; set; }
    public string NumarPV { get; set; }
    public DateOnly? DataPV { get; set; }
    public string CodCpv { get; set; }
    public string Valuta { get; set; }
    public decimal? Curs { get; set; }
    public List<FacturaIntrareLinieWriteDto> Linii { get; set; } = new();
}

// `Id` null = linie NOUĂ; `Id` cunoscut = actualizare. Liniile existente absente
// din payload se ȘTERG — reconcilierea e server-side, clientul trimite
// agregatul întreg (43c).
public sealed class FacturaIntrareLinieWriteDto {
    public Guid? Id { get; set; }
    // Rămâne obligatoriu chiar și pe liniile cu produs: precompletarea Tipului
    // din Produs e UX de client (OData `Produs` expune `TipMaterialId`), NU
    // magie în Apply — altfel serverul ar „ghici" identitatea contabilă a liniei.
    public Guid TipMaterialId { get; set; }
    // Obligatoriu pe liniile de STOC (validat la operare): fără el lotul nu se
    // naște, iar `FacturaIntrare.ValideazaOperare` refuză linia.
    public Guid? ProdusId { get; set; }
    public decimal Cantitate { get; set; }
    public decimal PretUnitar { get; set; }
    public Guid? TipTvaId { get; set; }
    // null = calculul standard din regim × cotă; valoare = override-ul
    // operatorului (36a — factura furnizorului bate rotunjirea), aplicat DUPĂ
    // `CalculeazaLaCulegere`; acceptat DOAR pe regimurile cu TVA separat
    // (Normal/TaxareInversă — review F2-D1) și niciodată negativ (F2-D7).
    // LIMITE ASUMATE ALE SEMANTICII (review F2-D2/D6, documentate nu fixate):
    // (a) override-ul EXPLICIT 0 nu supraviețuiește operării — condiția 36a din
    // motor e `ValoareTva != 0`, deci 0 se recalculează la operare; (b) singura
    // cale de a RENUNȚA la un override salvat e re-atingerea unui declanșator
    // (baza sau TipTva) — recalculul nu rulează fără ei. Fix-ul de fond pentru
    // ambele ar fi un flag persistat `TvaSuprascris` pe frunză — aditiv, dacă
    // nevoia devine reală.
    public decimal? ValoareTva { get; set; }
    // Atributele lotului, culese pe linie; motorul le copiază pe Lot la operare.
    public DateOnly? DataExpirare { get; set; }
    public string LotFabricatie { get; set; }
    public string CodCpv { get; set; }
    public Guid? AngajamentId { get; set; }
    // Dimensiunile frunzei (DIM-2, decizia 54c).
    public Guid? CodEconomicId { get; set; }
    public Guid? SursaFinantareId { get; set; }
    public Guid? CodFunctionalId { get; set; }
    public Guid? ProiectId { get; set; }
}

// ── Citire: agregatul + server-owned + affordances ─────────────────────────
public sealed class FacturaIntrareReadDto {
    public Guid Id { get; set; }
    public string Numar { get; set; }
    public DateOnly Data { get; set; }
    // STRING, nu enum (vezi ApiDtos): contractul nu depinde de ordinea membrilor.
    public string Stare { get; set; }
    public DateTime? DataOperare { get; set; }
    public Guid PredatorId { get; set; }
    public string PredatorDenumire { get; set; }
    // Furnizorul e Partener; pe un predator de alt tip rămâne null (latura se
    // validează abia la operare — Apply verifică doar existența).
    public string PredatorCodFiscal { get; set; }
    public Guid PrimitorId { get; set; }
    public string PrimitorDenumire { get; set; }
    public DateOnly? DataScadenta { get; set; }
    public string NumarPV { get; set; }
    public DateOnly? DataPV { get; set; }
    public string CodCpv { get; set; }
    public string Valuta { get; set; }
    public decimal? Curs { get; set; }
    // BRUT (Σ Valoare + ValoareTva), ca `Document.Total`.
    public decimal Total { get; set; }
    public bool Autogenerat { get; set; }
    public Guid? DocumentSursaId { get; set; }
    public List<FacturaIntrareLinieReadDto> Linii { get; set; } = new();
    // Grupul conex (decizia 17): NIR-ul generat la operare, plata autogenerată.
    // Link-ul UI „Deschide NIR" — clientul nu re-interoghează după DocumentSursaId.
    public List<DocumentCopilDto> Copii { get; set; } = new();

    // Affordances pe RESURSĂ (42e): clientul nu re-derivă regulile din `Stare`.
    public bool PoateEdita { get; set; }
    public bool PoateOpera { get; set; }
    public bool PoateAnula { get; set; }
    public bool PoateStorna { get; set; }
}

public sealed class FacturaIntrareLinieReadDto {
    public Guid Id { get; set; }
    public Guid TipMaterialId { get; set; }
    public string TipMaterialCod { get; set; }
    public string TipMaterialDenumire { get; set; }
    public Guid? ProdusId { get; set; }
    public string ProdusCod { get; set; }
    public string ProdusDenumire { get; set; }
    // Server-owned pe FCT (53a): lotul se naște/sincronizează la `Aplica`.
    public Guid? LotId { get; set; }
    // Compusă ca `Lot.Eticheta` din câmpuri PROIECTATE PLAT — inclusiv starea
    // „(în culegere)" a lotului încă nefinalizat de motor.
    public string LotEticheta { get; set; }
    public decimal Cantitate { get; set; }
    public decimal PretUnitar { get; set; }
    public decimal Valoare { get; set; }
    public decimal ValoareTva { get; set; }
    public Guid? TipTvaId { get; set; }
    public string TipTvaCod { get; set; }
    public string TipTvaDenumire { get; set; }
    public decimal? TipTvaCota { get; set; }
    public DateOnly? DataExpirare { get; set; }
    public string LotFabricatie { get; set; }
    public string CodCpv { get; set; }
    public Guid? AngajamentId { get; set; }
    public string AngajamentCod { get; set; }
    public Guid? CodEconomicId { get; set; }
    public string CodEconomicCod { get; set; }
    public Guid? SursaFinantareId { get; set; }
    public string SursaFinantareCod { get; set; }
    public Guid? CodFunctionalId { get; set; }
    public string CodFunctionalCod { get; set; }
    public Guid? ProiectId { get; set; }
    public string ProiectCod { get; set; }
}

// Un copil al grupului conex, cât să-l poți deschide și eticheta.
public sealed class DocumentCopilDto {
    public Guid Id { get; set; }
    // Codul ancorei `TipDocument` (NIR, PLT…) — vocabularul de rutare al
    // clientului (`/nir/{id}`), nu numele clasei CLR.
    public string Tip { get; set; }
    public string Numar { get; set; }
    public string Stare { get; set; }
    public bool Autogenerat { get; set; }
}

// ── Listă: exact coloanele grilei (fără linii — N+1 pe `Total` real) ───────
public sealed class FacturaIntrareListDto {
    public Guid Id { get; set; }
    public string Numar { get; set; }
    public DateOnly Data { get; set; }
    public string Stare { get; set; }
    // Predatorul FCT e furnizorul; primitorul, gestiunea recepționară.
    public string PredatorDenumire { get; set; }
    public string PrimitorDenumire { get; set; }
    public DateOnly? DataScadenta { get; set; }
    public decimal Total { get; set; }
}
