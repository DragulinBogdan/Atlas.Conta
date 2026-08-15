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
//
// ═══ Viramentul intern (transferul 581) NU schimbă contractul de SCRIERE ═══
// F7-D1/F7-D7: viramentul e o pereche PLT+INC pe laturi obișnuite, nu un tip de
// document nou — header, linii și ciclu de viață identice; diferă doar FELUL
// contrapartidei (al doilea cont propriu în loc de partener/angajat) și, în
// consecință, contul de postare. Contrapartida e deja un `Guid` liber în
// `TrezorerieWriteDto`, iar cuplajul „laturi ↔ natura liniilor" e invariant al
// OPERĂRII (`DocumentTrezorerie.ValideazaOperare`), unde stau toate celelalte
// reguli de laturi ⇒ WriteDto, rutele și `TrezorerieApply.Aplica` rămân
// NEATINSE. Singura adăugire e `EsteVirament` — pe ReadDto (affordance de FORMĂ,
// vezi mai jos) și pe ListDto (marcajul de grilă, review advers: fără el
// picioarele transferului sunt indistinguibile de plățile reale) —, nu un al
// doilea contract de scriere.
//
// `ImperecheriProiectii.DocumenteCuRest` EXCLUDE picioarele de virament pe
// ramurile PLT/INC (review advers al feliei): contractul susținea că e imposibil
// structural să apară — fals. Un picior de virament are `Rest > 0` pe veci (nu se
// stinge niciodată — `CapacitateStingere` e null și `PoateFiStins` e false pe el),
// iar contrapartida lui E chiar un cont propriu, adică exact filtrul pe care
// proiecția îl primește când panoul se deschide pentru un alt document de
// trezorerie. Ascunderea panoului pe virament (F7-D8) e formă de ecran, nu
// garanție de date. Filtrul e un anti-join pe tabela mică `ContPropriu`, în
// aceeași formă cu predicatul domeniului (AMBELE laturi conturi proprii).

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
    // NUMERELE STINGERII (F3-D2), din `ImperechereService` — sursa de adevăr:
    // `Asignat` numără AMBELE roluri (un avans stinge un decont ȘI e stins de
    // regularizare), `Ramas` = plafonul rămas al stingătorului. Clientul le
    // AFIȘEAZĂ, nu le calculează (42c). Sunt pe ReadDto, NU pe ListDto: acolo ar
    // fi un al doilea agregat pe fiecare rând de grilă.
    public decimal Asignat { get; set; }
    public decimal Ramas { get; set; }
    // AMBELE laturi sunt conturi proprii ⇒ documentul e un VIRAMENT INTERN
    // (F7-D7): transfer între casă și bancă, nu plată/încasare către un terț.
    // Calculată pe SERVER (predicatul e al domeniului —
    // `DocumentTrezorerie.EsteVirament`), ca să nu-l refacă TypeScript-ul din
    // felul repartitorilor. Clientul decide din ea FORMA ecranului: Tipul
    // implicit al liniei (`VIR` în loc de `TRZ`), ascunderea panoului de
    // stingeri (un virament nu stinge nimic) și indiciul laturii pereche.
    public bool EsteVirament { get; set; }
    public bool Autogenerat { get; set; }
    public Guid? DocumentSursaId { get; set; }
    // Numărul documentului-sursă: plata autogenerată → link înapoi la FACTURA
    // care a generat-o (clientul n-o mai caută după DocumentSursaId).
    public string DocumentSursaNumar { get; set; }
    // Codul de TIP al sursei (azi FCT — plata autogenerată; la transferul 581
    // va fi PLT/INC) — clientul rutează „Generat din" prin `rutaTip` (D-6b).
    public string DocumentSursaTip { get; set; }
    public List<TrezorerieLinieReadDto> Linii { get; set; } = new();
    // Grupul conex (decizia 17). Azi gol pe trezorerie — plata/încasarea sunt
    // COPII, nu părinți; câmpul există pentru simetria affordance-urilor (formula
    // de mai jos) și pentru perechea PLT+INC a transferului 581, când intră.
    public List<DocumentCopilDto> Copii { get; set; } = new();

    // Affordances pe RESURSĂ (42e), ONESTE pe AMBELE condiții ale motorului
    // (F3-D2): starea + grupul conex (copil operat) + STINGERILE
    // (`VerificaFaraImperecheri` — o plată cu imperechere nu se anulează până
    // nu se șterge link-ul). Aceeași formulă pe FCT/NIR/PLT/INC, prin
    // `ApiProiectii.AreImperecheri`.
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
    // Aceeași distincție, pentru virament: fără ea picioarele transferului 581
    // sunt indistinguibile în grilă de plățile/încasările reale (au numere din
    // aceleași serii și contrapartidă ca oricare). Formula e IDENTICĂ celei din
    // `Citeste` și cu predicatul domeniului — AMBELE laturi conturi proprii —,
    // calculată în același query, nu într-o a doua interogare per rând.
    public bool EsteVirament { get; set; }
    public decimal Total { get; set; }
}
