namespace Atlas.Conta.BackOffice.Module.Api.Trz;

// Felia verticală a TREZORERIEI (contract p5-felia-trz, F3-D1): DTO-urile
// plății și ale încasării. UN SINGUR nucleu pentru ambele — `Plata` și
// `Incasare` sunt aceeași formă cu laturile inversate (decizia 31a: liniile
// sunt DEFALCAREA sumei, iar sensul îl dă tipul), deci DTO-urile sunt comune și
// `TrezorerieApply` e generic pe `T : DocumentTrezorerie`. Cele două rute
// (`api/plt`, `api/inc`) rămân distincte în transport: clientul lucrează pe
// resurse concrete, nu pe o abstracție polimorfă (decizia 6).
//
// ═══ Ce e ALTFEL față de BTR/FCT (și de ce se vede în tipuri) ═══
//   * `Numar` NU e în WriteDto: PLT/INC au `PoliticaNumerotare` (seriile PLT-/
//     INC-) ⇒ numărul e SERVER-OWNED, consumat la materializare (GATE XAF D6),
//     iar gardianul de editare refuză culegerea lui. E exact INVERS față de FCT,
//     unde numărul e al furnizorului. Numărul plății AUTOGENERATE face excepție
//     și vine din `FacturaIntrare.PlataNumar` — pe calea motorului, nu pe asta.
//   * `Valoare` E în WriteDto, pe linie: trezoreria n-are `PregatesteOperare`
//     (nu există lanț de valori — nici preț, nici lot, nici cantitate), suma se
//     CULEGE. Invers față de BTR/FCT, unde `Valoare` e rezultat.
//   * `Cantitate`/`LotId`/`TipTvaId`/`ValoareTva` LIPSESC: n-au semantică pe
//     trezorerie. Rămân 0/null în model — inclusiv pe liniile clonate din
//     factură de plata autogenerată (31e).
//   * dimensiunile frunzei UNICE `DocumentTrezorerieDetaliu` (DIM-2, Î1):
//     aceleași patru ca pe FCT, fiindcă plata autogenerată clonează defalcarea
//     facturii. Obligativitatea lor e POLITICĂ per profil (PoliticaValidare +
//     `Cont.DimensiuniObligatorii`), nu contract de DTO.
//   * `TipInstrument` = STRING pe sârmă (convenția `Stare`): contractul nu
//     depinde de ordinea membrilor enum-ului, iar TypeScript-ul nu-l tipează
//     `number`. null ⇒ `OrdinPlata`; o valoare necunoscută = refuz de domeniu.

// ── Scriere: agregatul per document (PUT header + linii, 42d) ──────────────
public sealed class TrezorerieWriteDto {
    public DateOnly Data { get; set; }
    // PLT: predator = contul propriu, primitor = beneficiarul (partener/angajat).
    // INC: oglindit. TIPUL laturilor NU se verifică la scriere — e invariant al
    // OPERĂRII (`Plata/Incasare.ValideazaOperare`); Apply validează doar
    // EXISTENȚA repartitorilor, ca la BTR (un draft are voie să fie greșit).
    public Guid PredatorId { get; set; }
    public Guid PrimitorId { get; set; }
    public string TipInstrument { get; set; }
    // Extrasul de cont pe care apare operațiunea (bancă) — informativ.
    public string NumarExtras { get; set; }
    public DateOnly? DataExtras { get; set; }
    public List<TrezorerieLinieWriteDto> Linii { get; set; } = new();
}

// `Id` null = linie NOUĂ; `Id` cunoscut = actualizare. Liniile existente absente
// din payload se ȘTERG — reconcilierea e server-side, clientul trimite
// agregatul întreg (43c).
public sealed class TrezorerieLinieWriteDto {
    public Guid? Id { get; set; }
    // La culegere manuală clientul precompletează Tipul tehnic `TRZ` (F3-D7) —
    // convenție de CLIENT, nu validare: liniile clonate din factură poartă Tipul
    // liniei-sursă (302/628…), iar refuzul lor aici ar rupe plata autogenerată.
    public Guid TipMaterialId { get; set; }
    // CULEASĂ (vezi antetul): suma plătită/încasată pe această poziție a
    // defalcării. `ValideazaOperare` o cere strict pozitivă.
    public decimal Valoare { get; set; }
    public Guid? AngajamentId { get; set; }
    // Dimensiunile frunzei (DIM-2, decizia 54c/Î1).
    public Guid? CodEconomicId { get; set; }
    public Guid? SursaFinantareId { get; set; }
    public Guid? CodFunctionalId { get; set; }
    public Guid? ProiectId { get; set; }
}

// ── Citire: agregatul + server-owned + affordances ─────────────────────────
public sealed class TrezorerieReadDto {
    public Guid Id { get; set; }
    // SERVER-OWNED: null pe draft (seria se consumă la operare), completat după.
    public string Numar { get; set; }
    public DateOnly Data { get; set; }
    // STRING, nu enum (vezi ApiDtos).
    public string Stare { get; set; }
    public DateTime? DataOperare { get; set; }
    public Guid PredatorId { get; set; }
    public string PredatorDenumire { get; set; }
    public Guid PrimitorId { get; set; }
    public string PrimitorDenumire { get; set; }
    public string TipInstrument { get; set; }
    public string NumarExtras { get; set; }
    public DateOnly? DataExtras { get; set; }
    // BRUT (Σ Valoare + ValoareTva), ca `Document.Total`; pe trezorerie ValoareTva
    // e 0 prin construcție, deci = Σ defalcării. E și plafonul stingerii (31d).
    public decimal Total { get; set; }
    public bool Autogenerat { get; set; }
    public Guid? DocumentSursaId { get; set; }
    // Numărul documentului-sursă: plata autogenerată → link înapoi la FACTURA
    // care a generat-o (clientul n-o mai caută după DocumentSursaId).
    public string DocumentSursaNumar { get; set; }
    public List<TrezorerieLinieReadDto> Linii { get; set; } = new();
    // Grupul conex (decizia 17). Azi gol pe trezorerie — plata/încasarea sunt
    // COPII, nu părinți; câmpul există pentru simetria affordance-urilor (formula
    // de mai jos) și pentru perechea PLT+INC a transferului 581, când intră.
    public List<DocumentCopilDto> Copii { get; set; } = new();

    // Affordances pe RESURSĂ (42e). ATENȚIE — F3-D2 (pasul 2 al feliei):
    // `PoateAnula`/`PoateStorna` NU țin încă cont de IMPERECHERI, deși motorul
    // refuză anularea/stornarea cât timp există un link pe oricare rol
    // (`VerificaFaraImperecheri`). Aici formula e cea de la FCT (stare + copii
    // operați); helper-ul comun `AreImperecheri` intră odată cu
    // `ImperechereApply`, transversal pe FCT/NIR/PLT/INC.
    public bool PoateEdita { get; set; }
    public bool PoateOpera { get; set; }
    public bool PoateAnula { get; set; }
    public bool PoateStorna { get; set; }
}

public sealed class TrezorerieLinieReadDto {
    public Guid Id { get; set; }
    public Guid TipMaterialId { get; set; }
    public string TipMaterialCod { get; set; }
    public string TipMaterialDenumire { get; set; }
    public decimal Valoare { get; set; }
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

// ── Listă: exact coloanele grilei (fără linii — N+1 pe `Total` real) ───────
public sealed class TrezorerieListDto {
    public Guid Id { get; set; }
    public string Numar { get; set; }
    public DateOnly Data { get; set; }
    public string Stare { get; set; }
    public string PredatorDenumire { get; set; }
    public string PrimitorDenumire { get; set; }
    public string TipInstrument { get; set; }
    // Coloană de grilă cerută explicit (F3-D8): operatorul distinge plățile
    // culese de cele născute din facturi.
    public bool Autogenerat { get; set; }
    public decimal Total { get; set; }
}
