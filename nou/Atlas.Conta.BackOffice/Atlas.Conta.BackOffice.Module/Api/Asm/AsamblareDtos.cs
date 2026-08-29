namespace Atlas.Conta.BackOffice.Module.Api.Asm;

// Felia ASM (F19, track 2): asamblarea/kitting-ul cules manual — scriere +
// citire + comenzi + comanda proprie `distribuie-valoarea` (F19-D4).
//
// ASM e GEAMĂNUL STRUCTURAL al LDI-ului (F6): direcție explicită pe linie, o
// ramură NAȘTE lot (Produs, ca plusul de inventar), cealaltă descarcă un lot
// EXISTENT (Consum, ca minusul). Diferențele față de LDI, toate în contract:
//   * gestiunea lotului nou e a PREDATORULUI (`Asamblare.GestiuneLoturiCulese`,
//     F19-D3), nu a primitorului — laturile POT diferi;
//   * documentul poartă un INVARIANT VALORIC (46d): Σ produse = Σ consumuri, cu
//     toleranța 0,005. De aceea ReadDto duce `SumaConsum`/`SumaProdus`/
//     `Diferenta` calculate SERVER-SIDE (42c: „TS nu calculează niciodată") —
//     clientul citește invariantul, nu îl reface;
//   * FĂRĂ dimensiune de frunză: `AsamblareDetaliu` n-are `CodEconomic` (ASM nu
//     postează — zero `RegulaContare`, 23c), deci pe linie rămâne doar
//     `AngajamentId`-ul BAZEI (testul bazei 22c).
// La fel ca LDI: FĂRĂ `Numar` (seria „ASM-" e server-owned, consumată la
// materializare — F19-D6) și FĂRĂ TVA (F19-D7: ASM n-are `PoliticaTva` în
// niciun profil, deci `TipTvaId`/`ValoareTva` ar fi cifră moartă).

// ── Scriere: agregatul per document (PUT header + linii, 42d) ──────────────
public sealed class AsmWriteDto {
    public DateOnly Data { get; set; }
    // Gestiunea în care se asamblează → gestiunea care primește (de regulă
    // aceeași). Tipul laturilor se validează abia la operare; `Aplica` verifică
    // doar existența.
    public Guid PredatorId { get; set; }
    public Guid PrimitorId { get; set; }
    public List<AsmLinieWriteDto> Linii { get; set; } = new();
}

// `Id` null = linie NOUĂ; `Id` cunoscut = actualizare. Liniile existente absente
// din payload se ȘTERG — reconcilierea e server-side, clientul trimite agregatul
// întreg (43c).
public sealed class AsmLinieWriteDto {
    public Guid? Id { get; set; }
    // Numele membrului `DirectieAsamblare` („Consum"/„Produs"), nu numărul lui.
    // OBLIGATORIU: enumerarea n-are membru 0 tocmai ca o linie fără rol cules să
    // nu treacă drept ceva, iar direcția decide TOATĂ semantica liniei aici.
    public string Directie { get; set; }
    public Guid TipMaterialId { get; set; }
    // Marfa produsă — mecanismul lotului nou (F19-D3), obligatorie pe PRODUS
    // prin validarea de operare. Pe CONSUM se GOLEȘTE (F6-D3 aplicat pe ASM):
    // acolo marfa e a lotului descărcat, iar un produs rămas din starea de
    // produs ar naște lot-artefact pe draft.
    public Guid? ProdusId { get; set; }
    // Pinul lotului consumat — aplicat DOAR pe CONSUM (oglinda F6-D5). Pe PRODUS
    // lotul e server-owned și valoarea trimisă aici se IGNORĂ: ReadDto îl
    // întoarce (e lotul născut de `LoturiCulegereService`), deci round-trip-ul
    // agregatului l-ar retrimite mereu, iar o aplicare oarbă ar re-lega linia la
    // propriul ei lot pe o cale care nu-i aparține.
    public Guid? LotId { get; set; }
    // CULEASĂ POZITIV pe ambele direcții — semnul îl pune operarea
    // (`PregatesteOperare`, 28a). Pe un ASM deja operat linia poartă cantitatea
    // semnată, dar atunci documentul e oricum read-only.
    public decimal Cantitate { get; set; }
    // Prețul cu care se naște lotul produsului (validarea de operare îl cere
    // POZITIV). Pe CONSUM se golește — evaluarea consumului e a lotului
    // descărcat. Îl poate rescrie comanda `distribuie-valoarea` (F19-D4).
    public decimal? PretEvaluare { get; set; }
    // Atributele lotului produs; motorul le copiază pe `Lot` la operare
    // (`ILinieCuAtributeLot`). Pe CONSUM se golesc.
    public DateOnly? DataExpirare { get; set; }
    public string LotFabricatie { get; set; }
    // Angajamentul de pe BAZĂ (testul bazei 22c). Frunza ASM n-are dimensiuni
    // proprii (documentul nu postează).
    public Guid? AngajamentId { get; set; }
}

// ── Citire: agregatul + invariantul + affordances ──────────────────────────
public sealed class AsmReadDto {
    public Guid Id { get; set; }
    // Server-owned: seria „ASM-" se consumă la materializare, în propria operare;
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
    // Σ tuturor liniilor (definiția `Document.Total`). Pe ASM valorile sunt
    // SEMNATE de la culegere (F19-D8), deci `Total` E diferența invariantului —
    // pe un document fără linii de tip BAZĂ, `Total == Diferenta`.
    public decimal Total { get; set; }

    // ═══ Invariantul 46d, calculat SERVER-SIDE (F19-D9) ═══
    // Clientul afișează cifrele, nu le compune (42c).
    // Ambele sume ies POZITIVE (magnitudini), ca pe hârtie: `SumaConsum` e
    // `−Σ Valoare` pe liniile de consum (care sunt negative prin convenția
    // frunzei), `SumaProdus` e `Σ Valoare` pe liniile de produs.
    //
    // ATENȚIE — sunt cifrele CULEGERII, nu ale operării (75a/F19-D8): valoarea
    // liniei de consum e previzualizarea `preț lot × cantitate`, iar regula
    // golirii (D18-D2) o poate schimba la operare, când consumul GOLEȘTE cheia.
    // Pe scena tipică (lot 3 × 30,02 ⇒ preț 10,006667, rest 1 buc / 10,00) un
    // consum de 1 apare aici cu 10,01, deși va posta 10,00 — deci după
    // `distribuie-valoarea` (care aliniază produsele la 10,00) `Diferenta` iese
    // −0,01 pe un document care operează perfect. Nu e o scăpare a proiecției:
    // culegerea nu are voie să prezică golirea (ar fi al doilea adevăr al
    // regulii, exact ce interzice 75a). Verdictul AUTORITAR e dry-run-ul
    // (`POST /valideaza` — 43b), iar cifrele prezise ies din
    // `AsmDistribuireDto`; ele două sunt ce arată clientul după comandă.
    public decimal SumaConsum { get; set; }
    public decimal SumaProdus { get; set; }
    // `SumaProdus − SumaConsum`. Liniile de tip BAZĂ (ASM-uri istorice de
    // import) nu intră în niciuna dintre sume — n-au direcție —, deci pe ele
    // `Total` și `Diferenta` diferă deliberat.
    public decimal Diferenta { get; set; }

    public List<AsmLinieReadDto> Linii { get; set; } = new();

    // Affordances pe RESURSĂ (42e): aceeași sursă ca acțiunile XAF.
    public bool PoateEdita { get; set; }
    public bool PoateOpera { get; set; }
    public bool PoateAnula { get; set; }
    public bool PoateStorna { get; set; }
    // Comanda proprie feliei (F19-D4): are rost doar pe un Draft cu cel puțin o
    // linie de fiecare rol. Restul refuzurilor (consum fără lot, cantitate 0,
    // reziduu nereprezentabil) rămân ale comenzii — o afordanță nu poate
    // prezice ce prezice comanda.
    public bool PoateDistribui { get; set; }
}

public sealed class AsmLinieReadDto {
    public Guid Id { get; set; }
    // Numele membrului („Consum"/„Produs"). NULL pe o linie de tip BAZĂ (ASM
    // istoric/importat): frunza nu există, deci nici direcția — clientul o
    // afișează ca atare, iar reconcilierea refuză o astfel de linie explicit.
    public string Directie { get; set; }
    public Guid TipMaterialId { get; set; }
    public string TipMaterialCod { get; set; }
    public string TipMaterialDenumire { get; set; }
    public Guid? ProdusId { get; set; }
    public string ProdusCod { get; set; }
    public string ProdusDenumire { get; set; }
    public Guid? LotId { get; set; }
    public string LotEticheta { get; set; }
    // Pe un ASM OPERAT iese SEMNATĂ — e fapta operării (28a), nu o culegere.
    public decimal Cantitate { get; set; }
    public decimal? PretEvaluare { get; set; }
    // REZULTAT, nu culegere: o materializează `Aplica` la culegere și
    // `PregatesteOperare` la operare, din aceeași formulă SEMNATĂ (F19-D8).
    public decimal Valoare { get; set; }
    public DateOnly? DataExpirare { get; set; }
    public string LotFabricatie { get; set; }
    public Guid? AngajamentId { get; set; }
    public string AngajamentCod { get; set; }
}

// ── Listă: exact coloanele grilei ──────────────────────────────────────────
// Fără coloana `Autogenerat`: ASM nu se generează niciodată automat.
public sealed class AsmListDto {
    public Guid Id { get; set; }
    public string Numar { get; set; }
    public DateOnly Data { get; set; }
    public string Stare { get; set; }
    public string PredatorDenumire { get; set; }
    public string PrimitorDenumire { get; set; }
    // Diferența invariantului (Σ liniilor semnate) — pe listă, 0 înseamnă
    // „echilibrat".
    public decimal Total { get; set; }
}

// ── Rezultatul comenzii `distribuie-valoarea` (F19-D4) ─────────────────────
// Comanda întoarce agregatul RECITIT (cifrele noi, invariantul la zero) plus
// cifrele deciziei ei — ca operatorul să vadă CE s-a distribuit, nu doar că
// „s-a făcut ceva". Reziduul plimbat e raportat explicit: e singura urmă a
// faptului că prețurile nu ies din împărțire exactă.
public sealed class AsmDistribuireDto {
    // Valoarea pe care o vor scrie consumurile la operare — PREDICȚIA, prin
    // aceleași funcții ale motorului (vezi `AsamblareApply.DistribuieValoarea`).
    public decimal SumaConsum { get; set; }
    // Suma repartizată pe liniile de produs; egală, la ban, cu `SumaConsum`.
    public decimal SumaProdus { get; set; }
    // Cât a trebuit plimbat între linii ca sumele să se închidă la cent (0 când
    // împărțirea a ieșit exact din prima).
    public decimal ReziduuPlimbat { get; set; }
    public AsmReadDto Document { get; set; }
}
