namespace Atlas.Conta.BackOffice.Module.Api.Dsc;

// Felia DSC (F4-D2): CITIRE + comenzi, exact ca NIR-ul la felia 2 — și din
// același motiv. Descărcarea de gestiune se NAȘTE exclusiv prin
// `DescarcareService`: automat, în tranzacția operării facturii
// (`FacturaIesire.GenereazaSecundar`), sau prin comanda de backorder
// `POST /api/fcl/{id}/genereaza-descarcare` (F4-D3). Liniile ei sunt REZULTATUL
// pickingului pe loturi (spargere 1→N la generare, decizia 13), nu o culegere —
// deci n-are agregat de scriere.
//
// POST/PUT/DELETE pe DSC (descărcarea culeasă MANUAL — legală în model, azi
// acoperită de ecranul XAF) e felie separată, pur aditivă: `Aplica`/`Sterge` se
// adaugă AICI, fără să mute nimic.
//
// Comenzile (opereaza/anuleaza/storneaza/valideaza) nu cer nimic aici: sunt
// agnostice de tip (`OperareApi` + `OperareRezultatDto`/`StornoRequestDto`).

// ── Citire: agregatul + affordances ────────────────────────────────────────
public sealed class DscReadDto {
    public Guid Id { get; set; }
    // Server-owned: DSC ARE politică de numerotare (seria „DSC-"), consumată la
    // MATERIALIZAREA propriei operări (GATE XAF D6) — pe draft e null.
    public string Numar { get; set; }
    public DateOnly Data { get; set; }
    // STRING, nu enum (vezi ApiDtos): contractul nu depinde de ordinea membrilor.
    public string Stare { get; set; }
    public DateTime? DataOperare { get; set; }
    // Predatorul e GESTIUNEA de descărcare (marfa iese din ea), primitorul e
    // clientul de pe factură — DSC nu inversează laturile facturii, le înlocuiește.
    public Guid PredatorId { get; set; }
    public string PredatorDenumire { get; set; }
    public Guid PrimitorId { get; set; }
    public string PrimitorDenumire { get; set; }
    // Σ Valoare + ValoareTva, ca `Document.Total`. Pe DSC valoarea e COSTUL
    // (preț lot × cantitate), iar TVA-ul e 0 prin construcție: TVA-ul vânzării
    // se postează integral pe FCL (P1) — deci `Total` = costul descărcării.
    public decimal Total { get; set; }
    public bool Autogenerat { get; set; }
    // Factura care a generat descărcarea; clientul o rutează prin `rutaTip`
    // („Generat din"), fără a hardcoda `/fcl/`.
    public Guid? DocumentSursaId { get; set; }
    public string DocumentSursaNumar { get; set; }
    public List<DscLinieReadDto> Linii { get; set; } = new();

    // Affordances pe RESURSĂ (42e): aceeași sursă ca acțiunile XAF.
    public bool PoateEdita { get; set; }
    public bool PoateOpera { get; set; }
    public bool PoateAnula { get; set; }
    public bool PoateStorna { get; set; }
}

public sealed class DscLinieReadDto {
    public Guid Id { get; set; }
    public Guid TipMaterialId { get; set; }
    public string TipMaterialCod { get; set; }
    public string TipMaterialDenumire { get; set; }
    // Trasabilitatea acoperirii per linie de factură (design P2 §3): pe ce
    // poziție a FCL-ului răspunde rândul ăsta. Null pe o descărcare culeasă
    // manual (fără factură-sursă).
    public Guid? LinieSursaId { get; set; }
    // Ieșirea e PE LOT (decizia 13) — identitatea rândului de descărcare.
    public Guid? LotId { get; set; }
    public string LotEticheta { get; set; }
    // Produsul vine PRIN LOT (`DescarcareGestiuneDetaliu` nu poartă `ProdusId`:
    // lotul îl determină), spre deosebire de linia de FCL unde e cules.
    public Guid? ProdusId { get; set; }
    public string ProdusDenumire { get; set; }
    public decimal Cantitate { get; set; }
    // COSTUL (preț lot × cantitate), server-owned: `PregatesteOperare` îl
    // rescrie la operare din aceeași formulă — prețul nu se culege niciodată
    // pe DSC (pattern BTR/BCS).
    public decimal Valoare { get; set; }
    // Singura dimensiune-frunză (DIM-2), CLONATĂ de pe linia FCL sursă.
    public Guid? CodEconomicId { get; set; }
    public string CodEconomicCod { get; set; }
}

// ── Listă: exact coloanele grilei ──────────────────────────────────────────
public sealed class DscListDto {
    public Guid Id { get; set; }
    public string Numar { get; set; }
    public DateOnly Data { get; set; }
    public string Stare { get; set; }
    // Gestiunea (predator) și clientul (primitor).
    public string PredatorDenumire { get; set; }
    public string PrimitorDenumire { get; set; }
    // Ca la NIR: majoritatea rândurilor sunt generate, iar grila trebuie să
    // distingă descărcarea generată de cea culeasă manual.
    public bool Autogenerat { get; set; }
    // Costul descărcării (vezi `DscReadDto.Total`).
    public decimal Total { get; set; }
}
