using Atlas.Conta.BackOffice.Module.Proiectii;

namespace Atlas.Conta.BackOffice.Module.Api.Ntc;

// Felia NTC (F19, track-ul 1): nota contabilă — scriere + citire + comenzi +
// panoul de compensare. Ridică ultima excludere de scriere a pasului 5.
//
// ═══ Ce e PROPRIU tipului față de restul feliilor ═══
//   * LINIA E POSTAREA (`ILinieCuPostareExplicita`, 32a, extins în motor prin
//     `IDocumentCuPostareExplicita`): pe NTC postarea explicită COMPLETĂ bate
//     ABSENȚA oricărei reguli de contare — nota nu are `RegulaContare` în niciun
//     profil. Conturile sunt OBLIGATORII (invariant al tipului, verificat de
//     `NotaContabila.ValideazaOperare`); repartitorii per latură rămân opționali,
//     iar fără ei cade default-ul polimorf al header-ului (32c).
//   * `Valoare` E CULEASĂ DIRECT (F19-D8): nu există lanț de valori — nici
//     cantitate, nici preț, nici TVA calculat — deci nu există nici
//     `MaterializeazaValori`. NEGATIVUL E PERMIS (note storno, importul 1C le
//     aduce cu minus); ZERO e refuzat de TIP, la operare, iar felia NU duplică
//     refuzul (o a doua sursă a aceleiași reguli e exact ce evită 42a).
//   * FĂRĂ TVA, FĂRĂ LOT, FĂRĂ CANTITATE (F19-D7): NTC n-are `PoliticaTva` în
//     niciun profil, deci `TipTvaId`/`ValoareTva` ar fi cifră moartă pe sârmă
//     (precedentul F5-D5/F6-D5); baseline-ul XAF ascunde deja exact aceleași
//     coloane moștenite pe frunză (`ContaUiBaseline.NotaContabila`).
//   * ROLUL DE STINGĂTOR (48b, compensarea): nota operată stinge documente, cu
//     plafon PER CONTRAPARTIDĂ (`NotaContabila.CapacitateStingere`). De aici
//     endpoint-ul propriu al feliei — `GET api/ntc/{id}/candidati` (F19-D10).
//   * Laturile sunt repartitori INTERNI (contrapartidele trăiesc pe LINII).
//     DTO-ul le poartă ca simple FK-uri: refuzul rămâne al motorului (F6-D8 —
//     autoritatea e operarea, nu filtrul de lookup).
//
// Ca la BTR/NIR/FCL/TRZ/LDI/BCS/DEC (și invers față de FCT): FĂRĂ `Numar` — NTC
// are `PoliticaNumerotare` („NTC-") în AMBELE profiluri, deci seria e
// SERVER-OWNED, consumată la MATERIALIZARE, în propria operare (F19-D6).

// ── Scriere: agregatul per document (PUT header + linii, 42d) ──────────────
public sealed class NtcWriteDto {
    public DateOnly Data { get; set; }
    // Ambele laturi = repartitori INTERNI (ex. SEDIU). Tipul lor e invariant al
    // OPERĂRII (`NotaContabila.ValideazaOperare` refuză `Partener`), nu al
    // culegerii: draftul are voie să fie greșit, operarea nu.
    public Guid PredatorId { get; set; }
    public Guid PrimitorId { get; set; }
    public List<NtcLinieWriteDto> Linii { get; set; } = new();
}

// `Id` null = linie NOUĂ; `Id` cunoscut = actualizare. Liniile existente absente
// din payload se ȘTERG — reconcilierea e server-side, clientul trimite agregatul
// întreg (43c).
public sealed class NtcLinieWriteDto {
    public Guid? Id { get; set; }
    // ═══ Singurul câmp fără rol semantic pe notă, și totuși OBLIGATORIU ═══
    // `DocumentDetaliu.TipMaterialId` e NOT NULL pe BAZĂ (cheia contării și a
    // regulilor de stoc pentru toate celelalte tipuri). Nota nu-l consultă
    // NICIODATĂ: n-are reguli de stoc, n-are reguli de contare, iar conturile
    // vin de pe linie. Baseline-ul XAF îl ascunde („TipMaterial convențional
    // TRZ"), Import1C îl pune convențional pe tipul tehnic „TRZ".
    // Felia NU inventează un default: un fallback în API ar însemna un simbol de
    // profil hardcodat în afara politicilor (29) — deci câmpul rămâne CERUT, iar
    // absența lui iese ca refuz de DOMENIU („Tipul (contul/clasa) … nu există
    // sau nu e vizibil(ă)…", fraza unică din `Refuzuri.ReferintaInvizibila`, 22-D6),
    // nu ca violare de FK. Clientul îl culege dintr-un lookup, ca pe orice linie.
    public Guid TipMaterialId { get; set; }
    public string Descriere { get; set; }
    // Postarea explicită — trăsătura PROPRIE a tipului (32a). Conturile sunt
    // OBLIGATORII pe NTC (validarea tipului le cere pe amândouă), repartitorii
    // per latură rămân opționali. Nullable pe sârmă fiindcă draftul are voie să
    // fie incomplet: refuzul e al operării, nu al culegerii.
    public Guid? ContDebitId { get; set; }
    public Guid? ContCreditId { get; set; }
    public Guid? RepartitorDebitId { get; set; }
    public Guid? RepartitorCreditId { get; set; }
    // Dimensiunea frunzei (DIM-2): conturile care cer defalcarea E (trezorerie,
    // venituri, unele cheltuieli bugetare) o primesc de pe linie.
    public Guid? CodEconomicId { get; set; }
    // CULEASĂ direct (F19-D8), NEGATIVUL PERMIS (note storno). Zero îl refuză
    // tipul la operare — felia nu-l duplică.
    public decimal Valoare { get; set; }
}

// ── Citire: agregatul + affordances ────────────────────────────────────────
public sealed class NtcReadDto {
    public Guid Id { get; set; }
    // Server-owned: seria „NTC-" se consumă la materializare, în propria operare;
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
    // Σ liniilor (definiția `Document.Total`, pe BAZA detaliului). Pe notă suma
    // NU e o creanță și nici o datorie — e cât s-a postat; poate fi negativă
    // (note storno). De-aia nota lipsește deliberat din `DocumenteCuRest`
    // (F19-D10): n-are semantică de „rest".
    public decimal Total { get; set; }
    public List<NtcLinieReadDto> Linii { get; set; } = new();

    // Affordances pe RESURSĂ (42e): aceeași sursă ca acțiunile XAF.
    // `PoateAnula`/`PoateStorna` includ STINGERILE (F3-D2/57d) — pe NTC nu e o
    // precauție teoretică: compensarea E cazul normal al tipului (48b), deci
    // gardianul `VerificaFaraImperecheri` se aprinde des.
    public bool PoateEdita { get; set; }
    public bool PoateOpera { get; set; }
    public bool PoateAnula { get; set; }
    public bool PoateStorna { get; set; }
}

public sealed class NtcLinieReadDto {
    public Guid Id { get; set; }
    public Guid TipMaterialId { get; set; }
    public string TipMaterialCod { get; set; }
    public string TipMaterialDenumire { get; set; }
    // NULL pe o linie de tip BAZĂ (notă istorică/importată): frunza nu există,
    // deci nici descrierea, nici postarea explicită. Reconcilierea refuză
    // explicit o astfel de linie referită prin `Id` (și motorul refuză operarea
    // documentului care o poartă).
    public string Descriere { get; set; }
    public Guid? ContDebitId { get; set; }
    public string ContDebitSimbol { get; set; }
    public string ContDebitDenumire { get; set; }
    public Guid? ContCreditId { get; set; }
    public string ContCreditSimbol { get; set; }
    public string ContCreditDenumire { get; set; }
    public Guid? RepartitorDebitId { get; set; }
    public string RepartitorDebitDenumire { get; set; }
    public Guid? RepartitorCreditId { get; set; }
    public string RepartitorCreditDenumire { get; set; }
    public Guid? CodEconomicId { get; set; }
    public string CodEconomicCod { get; set; }
    public decimal Valoare { get; set; }
}

// ── Listă: exact coloanele grilei ──────────────────────────────────────────
// Fără coloana `Autogenerat`: nota nu se generează niciodată automat (nu e țintă
// de `PoliticaConex` și niciun tip nu o produce ca secundar; ITV-ul își are
// propriul tip).
public sealed class NtcListDto {
    public Guid Id { get; set; }
    public string Numar { get; set; }
    public DateOnly Data { get; set; }
    public string Stare { get; set; }
    public string PredatorDenumire { get; set; }
    public string PrimitorDenumire { get; set; }
    public decimal Total { get; set; }
}

// ── Panoul de compensare: candidații de stins, per CONTRAPARTIDĂ (F19-D10) ──
//
// De ce un endpoint propriu și nu `documente-cu-rest?contrapartidaId=…`:
// `ImperecheriProiectii.DocumenteCuRest` filtrează pe O SINGURĂ contrapartidă,
// fiindcă toate tipurile din uniune o au pe LATURĂ. Nota o are pe LINII, și pot
// fi mai multe — deci întrebarea „ce pot stinge cu nota asta" are n răspunsuri,
// unul per contrapartidă, fiecare cu plafonul lui. Serverul le rezolvă;
// clientul afișează (42c: TS nu calculează niciodată sold/rest/plafon).
public sealed class NtcCandidatiDto {
    public Guid DocumentId { get; set; }
    // Nota stinge doar OPERATĂ (`ImperechereService.ValideazaCreare`) — clientul
    // vede din prima de ce panoul e inert pe un draft.
    public string Stare { get; set; }
    public bool PoateStinge { get; set; }
    public List<NtcContrapartidaDto> Contrapartide { get; set; } = new();
}

public sealed class NtcContrapartidaDto {
    public Guid RepartitorId { get; set; }
    public string RepartitorCod { get; set; }
    public string RepartitorDenumire { get; set; }
    // Jumătatea de plafon (F19-D16): `Datorie` = repartitorii de pe DEBIT (nota
    // debitează 401 ⇒ stinge ce datorăm), `Creanta` = cei de pe CREDIT. Panoul
    // are un rând PER (contrapartidă × sens), fiindcă și plafonul, și candidații
    // sunt ai jumătății — un singur rând pe 401 = 4111 ar promite o capacitate
    // dublă și ar propune facturi de client sub jumătatea de datorie.
    // Enum pe sârmă ca STRING (57a), parse pe NUME.
    public string Sens { get; set; }
    // Plafonul TIPULUI pe jumătatea asta: Σ |Valoare| a liniilor pe care
    // contrapartida apare pe latura corespunzătoare sensului. Vine din
    // `NotaContabila.CapacitateStingere`, nu dintr-o a doua formulă.
    public decimal Capacitate { get; set; }
    // Cât s-a consumat deja din plafonul ăsta — `ImperechereService.AsignatFataDe`,
    // ADICĂ EXACT funcția pe care o cheamă `ValideazaCreare`. Cusătura e o
    // partajare de cod, nu o coincidență de formulă: altfel panoul ar propune
    // sume pe care serverul le refuză (sau invers).
    public decimal Asignat { get; set; }
    // `Capacitate − Asignat`: plafonul REAL al următoarei stingeri față de
    // contrapartida asta. Suma efectivă rămâne plafonată și de `Rest`-ul
    // documentului ales (`DocumentCuRestRand.Rest`) — cele două se compun în
    // client ca `min`, iar `ImperechereService` le verifică pe amândouă.
    public decimal Disponibil { get; set; }
    // Rândurile `DocumenteCuRest` filtrate pe contrapartida asta ȘI pe sens —
    // proiecția EXISTENTĂ, refolosită ca atare (fără a doua definiție a
    // restului, fără o a doua definiție a sensului).
    public List<DocumentCuRestRand> Candidati { get; set; } = new();
    // Plafon de pagină, ca la orice listă (`ContaApiController.Incarca`): pe baza
    // de import un partener poate avea sute de documente deschise. Nu se
    // trunchiază TĂCUT — clientul află că mai sunt.
    public bool MaiSunt { get; set; }
}
