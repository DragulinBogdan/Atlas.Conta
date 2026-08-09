namespace Atlas.Conta.BackOffice.Module.Api.Fcl;

// Felia verticală FCL (contract p5-felia-fcl, F4-D1): DTO-urile facturii de
// ieșire. Trăiesc în Module (fără nicio referință ASP.NET) — endpoint-urile din
// host sunt transport, iar ModelCheck exersează exact acest cod.
//
// ═══ Ce o deosebește de FCT (aceeași formă, alt contract) ═══
//   * `Numar` LIPSEȘTE din WriteDto: FCL are PoliticaNumerotare („FCL-", serie
//     FISCALĂ) ⇒ server-owned, ca BTR/PLT/INC — exact invers față de FCT, unde
//     numărul e al furnizorului și se culege. Îl expune doar ReadDto, iar
//     motorul îl consumă abia la materializarea operării (GATE XAF D6).
//   * `ProdusId` NU naște lot (FCL nu e document de intrare): e IDENTITATEA
//     liniei de stoc (poziția din site ↔ produs) și cheia pickingului la
//     generarea descărcării — „General!" din P2 §4, obligatoriu pe liniile de
//     stoc prin validarea de OPERARE.
//   * `LotId` (proprietate de BAZĂ) e „Specific?": PINUL opțional al
//     operatorului, care REFERĂ un lot existent și bate FIFO la spargere. Pe
//     FCT lotul era server-owned; aici e cules — de aceea intră în WriteDto.
//   * dimensiunea FRUNZEI e una singură (DIM-2, inventar §2): `CodEconomicId`.
//     FCL nu poartă SursaFinantare/CodFunctional/Proiect — nu sunt pe
//     `FacturaIesireDetaliu`, deci nu se pot inventa în DTO.
//   * fără `AngajamentId`: veniturile sunt exceptate de la obligativitatea
//     angajamentului (00 §10) — FCL n-are rând `CereClasificatieBugetara`.
//
// Regulile de TVA sunt IDENTICE cu ale FCT (o singură sursă — `TvaService`):
// default-ul `TipTvaImplicit` doar pe liniile NOI fără TipTva în payload,
// recalculul CONDIȚIONAT de declanșatori, override-ul acceptat doar pe
// regimurile cu TVA separat și niciodată negativ.

// ── Scriere: agregatul per document (PUT header + linii, 42d) ──────────────
public sealed class FacturaIesireWriteDto {
    // `Numar` nu apare deliberat (vezi antetul): seria fiscală e a serverului.
    public DateOnly Data { get; set; }
    // Emitentul (repartitor intern) — tipul laturii se validează la OPERARE.
    public Guid PredatorId { get; set; }
    // Clientul (Partener) — la fel, tipul se cere abia la operare.
    public Guid PrimitorId { get; set; }
    // Opțională: `PoliticaScadenta` aplică default-ul (+30) la operare DOAR dacă
    // n-a fost culeasă (decizia 30c).
    public DateOnly? DataScadenta { get; set; }
    // P2 §4: o singură gestiune per factură. Obligatorie când există linii de
    // stoc (validare la operare) — draftul are voie să fie incomplet.
    public Guid? GestiuneDescarcareId { get; set; }

    public List<FacturaIesireLinieWriteDto> Linii { get; set; } = new();
}

// `Id` null = linie NOUĂ; `Id` cunoscut = actualizare. Liniile existente absente
// din payload se ȘTERG — reconcilierea e server-side, clientul trimite
// agregatul întreg (43c).
public sealed class FacturaIesireLinieWriteDto {
    public Guid? Id { get; set; }
    // Rămâne obligatoriu chiar și pe liniile cu produs: precompletarea Tipului
    // din Produs e UX de client (OData `Produs` expune `TipMaterialId`), NU
    // magie în Apply — altfel serverul ar „ghici" identitatea contabilă a liniei.
    public Guid TipMaterialId { get; set; }
    // „General!" (P2 §4): obligatoriu pe liniile de STOC — validat la operare
    // (`FacturaIesire.ValideazaOperare`), împreună cu coerența Tip ↔ Produs.
    public Guid? ProdusId { get; set; }
    // „Specific?" (P2 §4): pinul de lot, opțional, pe proprietatea de BAZĂ.
    // Validat la operare ca aparținând produsului liniei ȘI ca având sold în
    // gestiunea de descărcare („întâi transfer BTR") — invarianți ai MOTORULUI,
    // pe care clientul îi vede prin dry-run/422, nu reguli de culegere.
    public Guid? LotId { get; set; }
    public string Descriere { get; set; }
    public decimal Cantitate { get; set; }
    public decimal PretUnitar { get; set; }
    public Guid? TipTvaId { get; set; }
    // null = calculul standard din regim × cotă; valoare = override-ul
    // operatorului (36a — documentul emis poartă rotunjirea lui: e-Factura,
    // agregarea retailului), aplicat DUPĂ `CalculeazaLaCulegere`; acceptat DOAR
    // pe regimurile cu TVA separat (Normal/TaxareInversă) și niciodată negativ.
    // Aceleași două limite asumate ca la FCT (F2-D2/D6): override-ul explicit 0
    // nu supraviețuiește operării (condiția 36a din motor e `ValoareTva != 0`),
    // iar renunțarea la un override salvat cere re-atingerea unui declanșator.
    public decimal? ValoareTva { get; set; }
    // Singura dimensiune-frunză a FCL (DIM-2); DSC-ul o primește prin clonă.
    public Guid? CodEconomicId { get; set; }
}

// ── Citire: agregatul + server-owned + affordances ─────────────────────────
public sealed class FacturaIesireReadDto {
    public Guid Id { get; set; }
    // Server-owned: null pe draft, seria „FCL-" abia după operare.
    public string Numar { get; set; }
    public DateOnly Data { get; set; }
    // STRING, nu enum (vezi ApiDtos): contractul nu depinde de ordinea membrilor.
    public string Stare { get; set; }
    public DateTime? DataOperare { get; set; }
    public Guid PredatorId { get; set; }
    public string PredatorDenumire { get; set; }
    public Guid PrimitorId { get; set; }
    public string PrimitorDenumire { get; set; }
    // Clientul e Partener (oglinda lui `PredatorCodFiscal` de pe FCT); pe un
    // primitor de alt tip rămâne null — latura se validează abia la operare.
    public string PrimitorCodFiscal { get; set; }
    public DateOnly? DataScadenta { get; set; }
    public Guid? GestiuneDescarcareId { get; set; }
    public string GestiuneDescarcareDenumire { get; set; }

    // BRUT (Σ Valoare + ValoareTva), ca `Document.Total`.
    public decimal Total { get; set; }
    public bool Autogenerat { get; set; }
    public Guid? DocumentSursaId { get; set; }
    public List<FacturaIesireLinieReadDto> Linii { get; set; } = new();
    // Grupul conex (decizia 17): descărcările de gestiune generate la operare
    // (Tip="DSC") — link-ul UI, fără o a doua interogare după DocumentSursaId.
    public List<DocumentCopilDto> Copii { get; set; } = new();

    // Affordances pe RESURSĂ (42e): clientul nu re-derivă regulile din `Stare`.
    // `PoateGeneraDescarcare` + restul nedescărcat sunt ale pasului 2 (F4-D4).
    public bool PoateEdita { get; set; }
    public bool PoateOpera { get; set; }
    public bool PoateAnula { get; set; }
    public bool PoateStorna { get; set; }
}

public sealed class FacturaIesireLinieReadDto {
    public Guid Id { get; set; }
    public Guid TipMaterialId { get; set; }
    public string TipMaterialCod { get; set; }
    public string TipMaterialDenumire { get; set; }
    public Guid? ProdusId { get; set; }
    public string ProdusCod { get; set; }
    public string ProdusDenumire { get; set; }
    // Pinul CULES (P2 §4), spre deosebire de FCT unde lotul e server-owned.
    public Guid? LotId { get; set; }
    // Compusă ca `Lot.Eticheta` din câmpuri PROIECTATE PLAT (`Eticheta` e
    // [NotMapped], deci inaccesibilă în SQL — restanța 40d).
    public string LotEticheta { get; set; }
    public string Descriere { get; set; }
    public decimal Cantitate { get; set; }
    public decimal PretUnitar { get; set; }
    // Server-owned (GATE 53c): materializate la culegere, rescrise la operare.
    public decimal Valoare { get; set; }
    public decimal ValoareTva { get; set; }
    public Guid? TipTvaId { get; set; }
    public string TipTvaCod { get; set; }
    public string TipTvaDenumire { get; set; }
    public decimal? TipTvaCota { get; set; }
    public Guid? CodEconomicId { get; set; }
    public string CodEconomicCod { get; set; }
}

// ── Listă: exact coloanele grilei (fără linii — N+1 pe `Total` real) ───────
public sealed class FacturaIesireListDto {
    public Guid Id { get; set; }
    public string Numar { get; set; }
    public DateOnly Data { get; set; }
    public string Stare { get; set; }
    // Predatorul FCL e emitentul; primitorul, clientul.
    public string PredatorDenumire { get; set; }
    public string PrimitorDenumire { get; set; }
    public DateOnly? DataScadenta { get; set; }
    public decimal Total { get; set; }
}
