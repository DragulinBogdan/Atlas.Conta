namespace Atlas.Conta.BackOffice.Module.Api.Nir;

// Felia NIR: CITIRE + comenzi (F2-D3) + SCRIERE (F5-D8, felia 5 — ridicarea
// excluderii declarate atunci: „POST/PUT/DELETE pe NIR + `ProdusId` pe
// `NirDetaliu` sunt felie separată, pur aditivă"). Cele două fluxuri ale
// recepției trăiesc acum amândouă aici:
//   * CONEX — FCT operată → `ConexId` → `/nir/{id}` → Operează: liniile vin din
//     clona pe care motorul o generează în tranzacția operării facturii;
//   * MANUAL — marfa intră pe aviz/bon, factura vine ulterior: NIR-ul se culege
//     din client, iar loturile se nasc pe PROPRIILE lui linii.
//
// Comenzile (opereaza/anuleaza/storneaza/valideaza) nu cer nimic aici: sunt
// agnostice de tip (`OperareApi` + `OperareRezultatDto`/`StornoRequestDto`).

// ── Scriere: agregatul per document (PUT header + linii, 42d) ──────────────
//
// ═══ Ce e DIFERIT față de WriteDto-ul FCT (diferențele sunt vizibile în TIPURI) ═══
//   * FĂRĂ `Numar`: NIR ARE `PoliticaNumerotare` („NIR-") în ambele profiluri ⇒
//     seria e SERVER-OWNED, consumată la materializare, în propria operare
//     (contractul BTR/FCL/TRZ). Pe FCT numărul e al FURNIZORULUI, deci cules.
//   * FĂRĂ `TipTvaId`/`ValoareTva` (F5-D5): NIR n-are `PoliticaTva` în niciun
//     profil, deci pasul TVA al motorului nu se declanșează pe el — TVA-ul se
//     postează pe factură, NIR-ul duce netul (36b). Un TVA cules aici ar fi
//     cifră moartă în cel mai bun caz și dublă postare când sosește factura.
//     Clona conexă își păstrează `TipTvaId` informativ — PUT-ul NU-l atinge,
//     iar ReadDto continuă să-l arate.
//   * FĂRĂ `LotId` (F5-D4): server-owned, ca pe FCT. Liniile manuale îl primesc
//     de la `LoturiCulegereService` la commit; liniile clonei conexe își
//     păstrează lotul STRĂIN (născut pe linia facturii), pe care seam-ul de
//     culegere îl lasă deliberat neatins.
public sealed class NirWriteDto {
    public DateOnly Data { get; set; }
    // Furnizorul (Partener) → gestiunea recepționară; TIPUL laturilor se
    // validează abia la operare (`NIR.ValideazaOperare`) — un draft are voie să
    // fie incomplet până atunci, `Aplica` verifică doar existența.
    public Guid PredatorId { get; set; }
    public Guid PrimitorId { get; set; }
    public List<NirLinieWriteDto> Linii { get; set; } = new();
}

// `Id` null = linie NOUĂ; `Id` cunoscut = actualizare. Liniile existente absente
// din payload se ȘTERG — reconcilierea e server-side, clientul trimite
// agregatul întreg (43c).
public sealed class NirLinieWriteDto {
    public Guid? Id { get; set; }
    // Rămâne obligatoriu chiar și pe liniile cu produs: precompletarea Tipului
    // din Produs e UX de client (OData `Produs` expune `TipMaterialId`), NU
    // magie în Apply — altfel serverul ar „ghici" identitatea contabilă a liniei.
    public Guid TipMaterialId { get; set; }
    // Obligatoriu pe liniile de STOC ale recepției MANUALE (validat la operare):
    // fără el lotul nu se naște. Pe liniile clonei conexe rămâne gol — marfa e a
    // facturii, recepția o moștenește (F5-D4).
    public Guid? ProdusId { get; set; }
    public decimal Cantitate { get; set; }
    // Prețul de pe hârtia furnizorului. IGNORAT pe liniile cu lot STRĂIN:
    // acolo valoarea vine din prețul lotului (F5-D6b) — clientul nici nu-l oferă.
    public decimal PretUnitar { get; set; }
    public Guid? AngajamentId { get; set; }
    // Atributele lotului, culese pe linie; motorul le copiază pe Lot la operare.
    public DateOnly? DataExpirare { get; set; }
    public string LotFabricatie { get; set; }
    // Dimensiunile frunzei (DIM-2, decizia 54c) — reuniunea FCT, culegibilă și
    // pe NIR-ul manual (Î3): fără ea n-ar putea satisface defalcarea 3xx.
    public Guid? CodEconomicId { get; set; }
    public Guid? SursaFinantareId { get; set; }
    public Guid? CodFunctionalId { get; set; }
    public Guid? ProiectId { get; set; }
}

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
    // Codul de TIP al sursei (azi FCT; mecanismul e generic) — clientul rutează
    // „Generat din" prin `rutaTip`, nu printr-un `/fct/` hardcodat (D-6b).
    public string DocumentSursaTip { get; set; }
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
    // Produsul care NAȘTE lotul pe recepția manuală (F5-D1/D2). Pe liniile
    // clonei conexe e gol: acolo marfa e a lotului străin.
    public Guid? ProdusId { get; set; }
    public string ProdusCod { get; set; }
    public string ProdusDenumire { get; set; }
    // Recepția e PE LOT (decizia 13): fiecare linie referă fie lotul născut la
    // culegerea facturii (conex), fie pe cel născut de linia proprie (manual).
    public Guid? LotId { get; set; }
    public string LotEticheta { get; set; }
    // Lotul liniei NU e născut de ea (`Lot.LinieIntrareId != linia.ID`) — cazul
    // clonei conexe. Îl spunem EXPLICIT, ca affordance de câmp: pe astfel de
    // linii produsul și prețul cules sunt inerte (F5-D4/D6b), iar clientul le
    // arată read-only fără să re-derive proveniența lotului printr-o euristică
    // („ProdusId e gol ⇒ probabil e conex" ar minți pe o linie manuală abia
    // începută).
    public bool LotStrain { get; set; }
    public decimal Cantitate { get; set; }
    // Prețul de recepție CULES (F5-D1). `Valoare` rămâne REZULTAT (GATE 53c):
    // o materializează `Aplica` la culegere și `PregatesteOperare` la operare,
    // din aceeași formulă.
    public decimal PretUnitar { get; set; }
    public decimal Valoare { get; set; }
    public decimal ValoareTva { get; set; }
    public Guid? TipTvaId { get; set; }
    public string TipTvaCod { get; set; }
    // Atributele lotului, culese pe linie la recepția manuală; motorul le
    // copiază pe Lot la operare (`ILinieCuAtributeLot`).
    public DateOnly? DataExpirare { get; set; }
    public string LotFabricatie { get; set; }
    // Pe BAZA detaliului (testul bazei 22c), ca pe FCT. Iese la citire fiindcă
    // intră la scriere: clientul retrimite agregatul ÎNTREG, iar un câmp scris
    // și necitit s-ar goli tăcut la primul PUT (liniile clonei conexe îl preiau
    // de pe factură).
    public Guid? AngajamentId { get; set; }
    public string AngajamentCod { get; set; }
    // Dimensiunile frunzei NIR (DIM-2), culese pe NIR-ul manual sau primite
    // prin contractul DimensiuniCulese/PreiaDimensiuni de la linia de factură.
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
