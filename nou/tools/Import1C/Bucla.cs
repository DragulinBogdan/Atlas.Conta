using System.Diagnostics;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Motor;
using DevExpress.ExpressApp;

namespace Import1C;

// PASUL 1 al feliei 1C-c: infrastructura buclei lunare.
//
// Unitatea de IDEMPOTENȚĂ e documentul, unitatea de CONTRACT e luna (§12.4).
// Bucla lunii: documente → imperecheri → închiderea de TVA → reconciliere.
// Handler-ele per tip 1C se înregistrează în `Handlere.Toate` (pașii 3–5) — bucla
// nu se rescrie când apar, doar lista crește.

// Contextul unei luni importate. Poartă bucla (deci și serviciile partajate:
// sursa, importul la cerere, supapa de alocare), ca handler-ele să fie funcții
// pure de context — se pot înregistra STATIC, înainte ca bucla să existe (faza
// pre-flight are nevoie de lista lor înainte de orice import).
sealed record ContextLuna(int An, int Luna, DateOnly Prima, DateOnly Ultima, BuclaImport Bucla) {
    // Înregistrează o unitate de import. `moment` e data-oră a ANTETULUI sursă
    // (nu `DateOnly` — ora e ordinea de introducere a sursei și e singurul
    // criteriu care desparte două documente din aceeași zi), `numar` e numărul
    // documentului, folosit doar ca tiebreak stabil.
    public void Planifica(DateTime moment, string numar, Action executa) =>
        Bucla.Planifica(moment, numar, executa);
}

// Un tip de document 1C și rutina care îi mătură luna. `TipRecorder` e numele
// tipului din sursă (coloana tipizată `DocReferinta_<Tip>` — vezi FlaxDb) și e
// cheia cu care pre-flight-ul verifică acoperirea.
//
// `Importa` NU mai importă când e chemată: **colectează unități** (`ctx.Planifica`),
// pe care bucla le execută în ordinea cronologiei sursei. Citirea antetelor și a
// secțiunilor rămâne per view, o dată pe lună — se amână doar execuția.
sealed record HandlerTip(string TipRecorder, string Descriere, Action<ContextLuna> Importa);

// O unitate de import amânată: TOT ce se întâmplă pentru un document-sursă
// (planificarea cu alocările ei, puntea, materializarea, operarea, documentele
// secundare pe care le naște). Handlerele care GRUPEAZĂ mai multe rânduri-sursă
// într-un document Atlas (extrasul, raportul de amănunt, compensarea) rămân cu
// unitatea lor de azi — gruparea nu se sparge, doar se amână.
//
// De ce (măsurat pe ianuarie): nicio ordine fixă pe TIPURI nu poate fi corectă
// simultan pentru toate cazurile. Transferurile din 10–30.01, importate înaintea
// vânzărilor, coborau soldul unui lot la zero pe 17.01, iar vânzarea din 03.01 —
// care la data ei avea acoperire — rămânea fără linie de stoc; invers, o
// dezasamblare din 14.01 naște un lot pe care transferul din 18.01 îl mută, dar
// mutările veneau înaintea transformărilor și mișcarea se pierdea. Singura ordine
// care le satisface pe toate e ordinea SURSEI: antetele 1C au oră, adică ordinea
// în care documentele au fost introduse.
sealed record UnitateImport(DateTime Moment, int OrdineTip, string Tip, string Numar, Action Executa);

static class Handlere {
    // Maparea §4 a designului, ca listă de tipuri CUNOSCUTE: tipurile pe care
    // felia 1C-c și-a propus să le acopere. Un tip din date care lipsește de aici
    // e o gaură de design (pre-flight îl raportează), nu doar muncă rămasă.
    public static readonly IReadOnlySet<string> Cunoscute = new HashSet<string>(StringComparer.Ordinal) {
        "AprovizionareMarfuriSiServiciiPrimite", "VanzareMarfuriSiServiciiPrestate",
        "TransferDeMarfuri", "BonDeConsum", "MarireStocDeMarfuri", "DiminuareStocDeMarfuri",
        "ExtrasDeCont", "Plata", "Incasare", "Compensare",
        "ReturDeLaClient", "ReturLaFurnizor", "Asamblare", "Dezasamblare",
        "RaportDeVanzariCuAmanunt", "AvizDeIesire", "AvizDeIntrare",
        "Operatia", "Salarii", "CasareMF", "InchidereLunaDeExercitiu", "Import", "ReevaluareMF",
        // Fără view generat, deci fără nume propriu în sursă: numele e al uneltei
        // (`FlaxDb.TipuriFaraColoana`), iar tipul se citește din structura
        // generică 1C. Pre-flight-ul îl vede prin recensământul Recorder.
        "IncasareCard",
    };

    // Pașii 3–5 adaugă aici câte o înregistrare per tip.
    //
    // Ordinea listei NU mai e ordinea de import: documentele lunii se execută
    // CRONOLOGIC, după data-oră a antetului sursă (vezi `UnitateImport`). Poziția
    // în listă a rămas doar TIEBREAK la timestamp identic — și în rolul ăsta
    // criteriul de mai jos e în continuare cel bun: la aceeași secundă, tot ce
    // ADAUGĂ stoc înaintea a tot ce CONSUMĂ stoc, ca ieșirea să-și găsească
    // acoperirea fără să treacă prin supapa 48a.
    public static readonly IReadOnlyList<HandlerTip> Toate = [
        // 1. INTRĂRILE — nasc loturi: factura (cu NIR-ul conex), avizul de intrare
        //    (plus de inventar), returul de la client (marfa revine în gestiune).
        HandlerFactura.Handler,
        HandlerAvizIntrare.Handler,
        HandlerReturClient.Handler,
        // 2. MUTĂRILE — duc marfa în gestiunea în care se lucrează cu ea.
        HandlerTransfer.Handler,
        // 3. TRANSFORMĂRILE — consumă din gestiunea de lucru și produc loturi noi
        //    (măsurat: înaintea transferurilor, asamblarea nu-și găsea consumurile;
        //    după ele, se recuperează integral).
        HandlerAsamblare.HandlerAsm,
        HandlerAsamblare.HandlerDez,
        // 4. IEȘIRILE — consumă tot ce s-a creat mai sus.
        HandlerConsum.Handler,
        HandlerDiferente.HandlerPlus,
        HandlerDiferente.HandlerMinus,
        HandlerVanzare.Handler,
        HandlerAmanunt.Handler,
        HandlerAvizIesire.Handler,
        HandlerReturFurnizor.Handler,
        // 5. TREZORERIA ȘI NOTELE — nu ating stocul deloc, deci poziția lor în
        //    lună e liberă; stau la coadă fiindcă stingerile (trecerea 2) se
        //    calculează după ce toate documentele lunii există.
        HandlerExtras.Handler,
        HandlerPlataCasa.Handler,
        HandlerIncasareCasa.Handler,
        HandlerCard.Handler,
        HandlerCompensare.Handler,
        .. HandlereNoteSimple.Handlere,
    ];

    // Rapoartele de tip ale pasului 3 (contoarele proprii fiecărui handler).
    public static void Raporteaza() {
        HandlerFactura.Raporteaza();
        HandlerTransfer.Raporteaza();
        HandlerConsum.Raporteaza();
        HandlerDiferente.Raporteaza();
        HandlerVanzare.Raporteaza();
        Descarcare1C.Raporteaza();
        HandlerAmanunt.Raporteaza();
        HandlerAvizIesire.Raporteaza();
        HandlerAvizIntrare.Raporteaza();
        HandlerReturFurnizor.Raporteaza();
        HandlerReturClient.Raporteaza();
        HandlerAsamblare.Raporteaza();
        MotorTrezorerie.Raporteaza();
        HandlerCompensare.Raporteaza();
        HandlereNoteSimple.Raporteaza();
        StocDinNota.Raporteaza();
        Evaluare.Raporteaza();
        NoteComune.Raporteaza();
        Imperecheri1C.Raporteaza();
        Reluare1C.Raporteaza();
    }

    public static IReadOnlySet<string> Implementate =>
        Toate.Select(h => h.TipRecorder).ToHashSet(StringComparer.Ordinal);
}

// Perioadele fiscale = pivotul gardianului (decizia 14): o perioadă NEDEFINITĂ e
// tratată ca ÎNCHISĂ, deci fără rândurile astea niciun document al anului nu s-ar
// putea opera. Se creează deschise și idempotent, ca tot restul importului.
static class Perioade {
    public static (int Existente, int Create) Asigura(IObjectSpaceProvider provider, int an) {
        using var os = provider.CreateObjectSpace();
        var existente = os.GetObjectsQuery<PerioadaFiscala>().Where(p => p.An == an).ToList();
        var create = 0;
        for (var luna = 1; luna <= 12; luna++) {
            if (existente.Any(p => p.Luna == luna))
                continue;
            var p = os.CreateObject<PerioadaFiscala>();
            p.An = an;
            p.Luna = luna;
            p.Inchisa = false;
            create++;
        }
        os.CommitChanges();
        return (existente.Count, create);
    }
}

enum StareImport { Importat, Reoperat, Sarit, Esec }

// Motivele de skip pe care le declară APELANTUL: fabrica de draft întoarce null
// din două feluri de cauze, iar diferența dintre ele e diagnostic, nu detaliu.
static class Motive {
    public const string FaraPlanLaReluare =
        "reluare fără plan (cheia nu intră în gardul handlerului — se replanifică la rularea următoare)";

    public static string FaraPlan(object plan, string motivSursa) =>
        plan == null ? FaraPlanLaReluare : motivSursa;
}

// Soarta unei unități de import (= un document-sursă 1C, cu tot ce naște el în
// Atlas). Fiecare unitate se termină în EXACT una dintre categoriile de mai jos,
// iar suma lor trebuie să dea numărul de unități planificate: aritmetica asta e
// singura probă că niciun document Posted nu dispare fără urmă.
enum SoartaUnitate {
    // A produs sau a confirmat cel puțin un document tipizat în Atlas.
    Document,
    // Singurul corespondent e nota-punte (forma sursei nu încape în niciun tip).
    DoarPunte,
    // Nu produce nimic în Atlas și se știe de ce (antete goale, transferuri în
    // aceeași gestiune) — se replanifică la fiecare rulare, fără efect.
    FaraCorespondent,
    // Sărită, cu motiv declarat (vezi contorul de motive).
    Sarita,
    // Handlerul a ieșit înainte de orice acțiune — practic „antet Posted care nu
    // postează în 1C" (garda per tip). Se numără per tip, ca să se poată pune
    // față în față cu contorul propriu al handlerului.
    FaraActiune,
    Esec,
}

sealed record RezultatLuna(int An, int Luna, int Documente, int Sarite, int Copii, int Esecuri,
    int Realocari, decimal CantitateRealocata, TimeSpan Durata,
    // Verdictul contractului lunii (pasul 6), ținut SEPARAT de eșecurile de
    // import: un import fără eșecuri și o reconciliere picată sunt două
    // diagnostice diferite, iar ambele opresc rularea (§12.4).
    int ContractePicate, decimal TvaDePlata, TimeSpan DurataContract);

sealed class BuclaImport {
    readonly IObjectSpaceProvider provider;
    readonly Action<string> avert;
    readonly Action<string, bool> check;

    // Indexurile de idempotență, ținute în memorie pentru toată rularea: 130k
    // documente × un punct-lookup pe bază fiecare ar fi jumătate din rulare.
    // Se actualizează pe măsură ce importul scrie — sunt sursa de adevăr a
    // deciziei „skip / re-operare / import".
    readonly Dictionary<(string Tabela, string Cheie), Guid> legaturi = [];
    readonly Dictionary<Guid, StareDocument> stari = [];
    readonly Dictionary<Guid, List<Guid>> copiiAutogenerati = [];

    // Contoarele lunii curente (progresul se raportează la fiecare 1.000).
    readonly Dictionary<string, int> peTip = new(StringComparer.Ordinal);
    int documente, sarite, copii, esecuri;
    int an, luna;
    Stopwatch cronometruLuna;

    // Motivul FIECĂRUI skip, agregat pe (tip × motiv): un `sarite++` mut e o
    // gaură de observabilitate — cifra din raport („188 sărite pe an") nu spune
    // nimic dacă nu poate fi itemizată la ultimul document.
    readonly Dictionary<(string Tip, string Motiv), int> motiveLuna = [];
    readonly Dictionary<(string Tip, string Motiv), int> motiveRulare = [];
    public IReadOnlyDictionary<(string Tip, string Motiv), int> MotiveSkip => motiveRulare;

    // Soarta unităților, per lună și cumulat pe rulare (vezi `SoartaUnitate`).
    readonly Dictionary<(string Tip, SoartaUnitate Soarta), int> soarteLuna = [];
    readonly Dictionary<(string Tip, SoartaUnitate Soarta), int> soarteRulare = [];
    public IReadOnlyDictionary<(string Tip, SoartaUnitate Soarta), int> Soarte => soarteRulare;

    // Ce a observat unitatea în curs de execuție (se resetează la fiecare unitate).
    int unitDocumente, unitPunti, unitSkipuri, unitEsecuri, unitFaraCorespondent;

    // Unitățile lunii, colectate de handlere și executate cronologic.
    readonly List<UnitateImport> unitati = [];
    int ordineTipCurent;
    string tipCurent = "";

    public void Planifica(DateTime moment, string numar, Action executa) =>
        unitati.Add(new UnitateImport(moment, ordineTipCurent, tipCurent, numar ?? "", executa));

    public FlaxDb Flax { get; }
    public ImportLaCerere LaCerere { get; }
    public AlocareIesire Alocare { get; }
    public Catalog Catalog { get; }
    public ContorPunti ContorPunti { get; } = new();
    public Action<string> Avert => avert;

    // Registrul divergențelor cunoscute (pasul 4 al lotului de robustețe): tot ce
    // unealta ARUNCĂ sau nu poate posta se înregistrează la locul faptei, ca
    // justificarea contractului lunar să fie o măsurătoare, nu o euristică.
    public RegistruDivergente Divergente { get; } = new();

    // Înregistrarea, stampilată cu luna în curs de import — handlerele n-au de ce
    // să care contextul lunii până la fiecare avertisment.
    public void Divergenta(string sursa, string categorie, IEnumerable<EfectStoc> stoc = null,
            string contDebit = null, string contCredit = null, decimal valoareNepostata = 0m) =>
        Divergente.Inregistreaza(an, luna, sursa, categorie, stoc,
            contDebit, contCredit, valoareNepostata);

    // Registrul contabil 1C al lunii, indexat pe document: sursa identității de
    // LOT pentru tipurile ale căror secțiuni n-o poartă (BTR/BCS/LDI). Se citește
    // O DATĂ per lună, nu per document — măsurat la pasul 2, diferența e 0,8 s
    // per lună față de ~70 s dacă fiecare document și-ar cere rândurile.
    public Dictionary<string, List<FlaxRandNota>> RanduriLuna { get; private set; } = [];
    public Dictionary<string, List<FlaxSubcontoNota>> SubcontoLuna { get; private set; } = [];

    public IObjectSpace CreeazaObjectSpace() => provider.CreateObjectSpace();

    // Documente-sursă care nu produc NIMIC în Atlas: nici document tipizat, nici
    // punte (antete goale, transferuri în aceeași gestiune fără reclasificare).
    // N-au unde primi legătură, deci se replanifică la fiecare rulare — inofensiv,
    // dar se numără, ca „de ce mai lucrează unealta la a doua rulare?" să aibă
    // un răspuns scris.
    public int SurseFaraCorespondent { get; private set; }

    public void NumaraSursaFaraCorespondent() {
        SurseFaraCorespondent++;
        unitFaraCorespondent++;
    }

    // Puntea unității: marcată la locul faptei (`Punti.Scrie`), fiindcă o punte
    // dezechilibrată NU se scrie ca document, dar tot e un corespondent declarat
    // al sursei (se înregistrează în registrul divergențelor).
    public void MarcheazaPunte() => unitPunti++;

    // Skip-ul, cu motiv OBLIGATORIU. `documentExista` = documentul e deja în bază
    // (operat sau stornat de o rulare anterioară): pentru aritmetica de închidere
    // unitatea are document, chiar dacă rularea de acum n-a mai scris nimic.
    void Sare(string view, string motiv, bool documentExista = false) {
        sarite++;
        unitSkipuri++;
        if (documentExista)
            unitDocumente++;
        var cheie = (view, motiv);
        motiveLuna[cheie] = motiveLuna.GetValueOrDefault(cheie) + 1;
        motiveRulare[cheie] = motiveRulare.GetValueOrDefault(cheie) + 1;
    }

    // Refuzul EXPLICIT al unei chei, fără a trece prin `ImportaDocument`: unitatea
    // parțial importată (D1) nu vrea nici măcar re-operarea unui draft rămas —
    // alocarea lui e învechită, iar `Executa` ar face exact asta pentru o fabrică
    // fără plan. Aici se refuză curat, cu motivul itemizat ca la orice skip.
    public void RefuzaCuMotiv(string view, string motiv) => Sare(view, motiv);

    // Eșecul de PLANIFICARE (handlerele pasului 3 planifică înaintea lui
    // `ImportaDocument`, ca puntea să se poată scrie prima). Fără el, o excepție
    // la un document ar urca până la prinderea de la nivelul handlerului și ar
    // opri tot tipul pe luna aia; aici rămâne ce trebuie să fie — eșecul UNUI
    // document, numărat și detaliat ca oricare altul.
    public void EsecPlanificare(string view, string cheie, Exception ex) =>
        Esec(view, cheie, "planificarea", ex);

    // Documentul e deja AȘEZAT (operat sau stornat)? Handlerele o întreabă ca să
    // NU replanifice degeaba la reluare; decizia propriu-zisă (skip / ștergere +
    // reimport) rămâne a lui `ImportaDocument`.
    //
    // Un DRAFT nu contează ca așezat (defectul D4 al lotului de robustețe): draftul
    // rămas de la o operare eșuată poartă alocarea de atunci, iar re-operarea lui ar
    // rula-o peste o stare de stoc schimbată. Se replanifică, iar `Executa` șterge
    // draftul vechi și îl reimportă întreg. Același răspuns îl cere și o legătură
    // ORFANĂ (documentul a dispărut din bază): planul trebuie să existe, altfel
    // reluarea ar intra în materializare cu mâna goală.
    public bool EsteCunoscut(string view, string cheie) =>
        legaturi.TryGetValue((Legaturi.Tabela(view), cheie), out var tinta)
            && (tinta == FaraDocument
                || stari.TryGetValue(tinta, out var stare) && stare != StareDocument.Draft);

    // CHEIA DECISĂ FĂRĂ DOCUMENT (F18, review advers F9): o unitate compusă
    // (transferul cu reclasificare) derivă cheile din SURSĂ, dar planificarea
    // poate decide legitim că una dintre ele nu produce document (ASM `#reclas`
    // fără nicio linie acoperită, lot la valoare 0, contul nou fără Tip, lotul
    // deja pe contul nou). Nelegată, cheia ar fi „inexistentă" la rulările
    // următoare și `Reluare1C.UnitatePartiala` ar refuza unitatea la nesfârșit
    // (frații de stoc operați + o cheie lipsă). Se leagă deci cu ținta
    // `Guid.Empty` — aceeași convenție de „țintă goală" ca rândurile registrului
    // divergențelor și contoarele de midpoint — și e CUNOSCUTĂ fără document:
    // `Executa` o sare cu motivul ei, `Tinta` întoarce null, idempotența din
    // Program.cs n-o numără (nu e document), iar `--deblocheaza` o șterge ca pe
    // o legătură orfană. Scrisă în ObjectSpace propriu, DUPĂ ce frații au fost
    // comiși — o decizie, nu o promisiune.
    public static readonly Guid FaraDocument = Guid.Empty;

    public void LeagaFaraDocument(string view, string cheie, string motiv) {
        var tabela = (Legaturi.Tabela(view), cheie);
        if (legaturi.ContainsKey(tabela))
            return;
        using var os = provider.CreateObjectSpace();
        Legaturi.Leaga(os, view, cheie, FaraDocument);
        os.CommitChanges();
        legaturi[tabela] = FaraDocument;
        Sare(view, motiv);
    }

    // Documentul Atlas al unei chei deja importate — pasul 4 are nevoie de el ca
    // FK (descărcarea de gestiune poartă `DocumentSursa` = factura de ieșire,
    // creată de un apel `ImportaDocument` anterior, în alt ObjectSpace).
    // Starea unui document Atlas, din indexul rulării: imperecherea (trecerea 2)
    // leagă doar documente OPERATE, iar un document care a eșuat la operare a
    // rămas Draft — se sare, nu se încearcă.
    public StareDocument? Stare(Guid id) => stari.TryGetValue(id, out var s) ? s : null;

    public Guid? Tinta(string view, string cheie) =>
        legaturi.TryGetValue((Legaturi.Tabela(view), cheie), out var tinta) && stari.ContainsKey(tinta)
            ? tinta : null;

    public BuclaImport(IObjectSpaceProvider provider, FlaxDb flax, ImportLaCerere laCerere,
            AlocareIesire alocare, Catalog catalog, Action<string> avert, Action<string, bool> check) {
        this.provider = provider;
        this.avert = avert;
        this.check = check;
        Flax = flax;
        LaCerere = laCerere;
        Alocare = alocare;
        Catalog = catalog;

        using var os = provider.CreateObjectSpace();
        // Evidența supapei de alocare (48a), din rulările anterioare: contractul
        // lunar o citește ca să poată NUMI diferențele de cost pe care le-a produs.
        Alocare.Incarca(os);
        // Registrul divergențelor, din rulările anterioare: fără el, o reluare care
        // sare documentele n-ar mai ști ce a aruncat rularea care le-a importat, iar
        // contractul ar declara nejustificate exact diferențele produse deliberat
        // (cerința de determinism, D).
        Divergente.Incarca(os, avert);
        var tabelaDivergente = Legaturi.Tabela(RegistruDivergente.View);
        var tabelaMidpoint = Legaturi.Tabela(ReconciliereLuna.ViewMidpoint);
        foreach (var l in os.GetObjectsQuery<MigrareLegatura>()
                     .Select(m => new { m.Tabela, m.CheieLegacy, m.TintaId }).ToList()) {
            // Rândurile registrului și contoarele de midpoint nu sunt legături
            // (țintă goală, cheie sintetică): aici ar umple degeaba indexul.
            if (l.Tabela == tabelaDivergente || l.Tabela == tabelaMidpoint)
                continue;
            legaturi[(l.Tabela, l.CheieLegacy)] = l.TintaId;
        }
        foreach (var d in os.GetObjectsQuery<Document>()
                     .Select(d => new { d.ID, d.Stare, d.Autogenerat, d.DocumentSursaId }).ToList()) {
            stari[d.ID] = d.Stare;
            if (d.Autogenerat && d.DocumentSursaId is { } sursa)
                (copiiAutogenerati.TryGetValue(sursa, out var lista)
                    ? lista : copiiAutogenerati[sursa] = []).Add(d.ID);
        }
    }

    // ======================= Bucla unei luni =======================

    public RezultatLuna ImportaLuna(int an, int luna) {
        this.an = an;
        this.luna = luna;
        documente = sarite = copii = esecuri = 0;
        peTip.Clear();
        motiveLuna.Clear();
        soarteLuna.Clear();
        Alocare.IncepeLuna();
        // Câte rotunjiri au căzut EXACT pe jumătatea de ban în luna asta: cifra din
        // care contractul 4 (D4) își derivă pragul derivei sistematice, în loc s-o
        // presupună. Se acumulează DOAR în jurul materializării prin motor
        // (`Opereaza` — review 1C-d-final, defect 5): rotunjirile din faza de
        // PLANIFICARE a uneltei (evaluări de linii, inclusiv pentru documente
        // sărite sau replanificate) nu ajung în niciun rând de registru, iar
        // numărate ar umfla pragul — și, prin MAX-ul persistat, l-ar umfla
        // PERMANENT: o rulare zgomotoasă ar lărgi definitiv contractul lunii.
        MidpointLuna = 0;
        cronometruLuna = Stopwatch.StartNew();
        var prima = new DateOnly(an, luna, 1);
        var ctx = new ContextLuna(an, luna, prima, prima.AddMonths(1).AddDays(-1), this);

        Console.WriteLine($"\n--- Luna {luna:00}/{an} ---");
        RanduriLuna = Flax.RanduriNotaPeLuna(an, luna);
        SubcontoLuna = Flax.SubcontoNotaPeLuna(an, luna);

        // Trecerea 1a — COLECTAREA: fiecare handler își citește antetele lunii
        // (o interogare per view, ca înainte) și înregistrează câte o unitate per
        // document-sursă. Nu se atinge încă nimic în baza țintă.
        unitati.Clear();
        for (var i = 0; i < Handlere.Toate.Count; i++) {
            var h = Handlere.Toate[i];
            ordineTipCurent = i;
            tipCurent = h.TipRecorder;
            try {
                h.Importa(ctx);
            }
            catch (Exception ex) {
                esecuri++;
                check($"Handler {h.TipRecorder} pe {luna:00}/{an}: {ex.Message}", false);
            }
        }
        if (Handlere.Toate.Count == 0)
            Console.WriteLine("  (niciun handler înregistrat — pașii 3–5 îi adaugă; "
                + "bucla rulează în gol, deliberat.)");

        // Trecerea 1b — EXECUȚIA, în ordinea sursei: data-oră a antetului, apoi
        // poziția tipului în `Handlere.Toate` (tiebreak la timestamp identic),
        // apoi numărul documentului. `OrderBy` e stabil, deci la chei egale rămâne
        // ordinea de enumerare a sursei.
        var planificate = unitati
            .OrderBy(u => u.Moment)
            .ThenBy(u => u.OrdineTip)
            .ThenBy(u => u.Numar, StringComparer.Ordinal)
            .ToList();
        unitati.Clear();
        Console.WriteLine($"  {planificate.Count} unități de import, executate cronologic.");
        foreach (var u in planificate) {
            unitDocumente = unitPunti = unitSkipuri = unitEsecuri = unitFaraCorespondent = 0;
            try {
                u.Executa();
                // Unitatea s-a executat până la capăt, dar poate să nu fi
                // materializat nimic (linie aruncată integral ⇒ document fără
                // linii ⇒ niciun commit în care să se strecoare evidența). Ce a
                // rămas în așteptare se scrie acum, în ObjectSpace propriu.
                Divergente.PersistaRamase(provider);
            }
            catch (Exception ex) {
                // Eșecul UNEI unități e diagnostic, ca eșecul unui document
                // (`Esec`): se numără complet, se detaliază plafonat, iar
                // verdictul rămâne al lunii.
                Esec(u.Tip, u.Numar, "importul documentului", ex);
            }
            finally {
                // Marcajele supapei decise în planificarea unității, dar rămase
                // nescrise (nimic nu s-a materializat): se abandonează aici, ca
                // evidența să nu treacă ÎNAINTEA faptelor și nici să nu se lipească
                // de commit-ul unității următoare.
                Alocare.RenuntaLaNepersistate();
                Divergente.RenuntaLaNepersistate();
                NoteazaSoarta(u.Tip);
            }
        }

        Imperecheri(ctx);
        InchidereTva(ctx);
        if (sabotajLuna && !sabotajFacut)
            SaboteazaLuna(ctx);
        // `MidpointLuna` e deja acumulat de `Opereaza` (documente + copii + ITV).
        // Sabotajul nu trece prin motor, deci nu-l atinge.
        var cronometruContract = Stopwatch.StartNew();
        var contract = ReconciliereLunara(ctx);
        var durataContract = cronometruContract.Elapsed;
        VerificaSabotaj();

        var (realocari, cantitate) = Alocare.DeltaLunii();
        var rez = new RezultatLuna(an, luna, documente, sarite, copii, esecuri,
            realocari, cantitate, cronometruLuna.Elapsed,
            contract.Picate, contract.TvaDePlata, durataContract);
        Handlere.Raporteaza();
        // Rezoluția pin-urilor de lot (cumulat pe rulare): pe prefix = cheia
        // exactă lipsea și prefixul (document × nomenclator) era neambiguu;
        // nerezolvate = prefix ambiguu (de la D18-D3 și perechile lot vechi /
        // lot reclasificat) ⇒ supapa 48a.
        Console.WriteLine($"  Loturi (cumulat): {Catalog.LoturiRezolvatePePrefix} pin-uri rezolvate pe prefix, "
            + $"{Catalog.LoturiNerezolvate} nerezolvate (prefix ambiguu sau lipsă ⇒ supapa 48a).");
        ContorPunti.Raporteaza();
        if (DrafturiSterse > 0 || DrafturiRefuzate > 0 || Alocare.MarcajeAbandonate > 0)
            Console.WriteLine($"  Reluare (cumulat pe rulare): {DrafturiSterse} drafturi ale rulărilor "
                + $"anterioare șterse și reimportate, {DrafturiRefuzate} refuzate, "
                + $"{Alocare.MarcajeAbandonate} marcaje de realocare abandonate (planificare fără "
                + "materializare).");
        if (SurseFaraCorespondent > 0)
            Console.WriteLine($"  {SurseFaraCorespondent} documente-sursă fără corespondent în Atlas "
                + "(nici document, nici punte) — se replanifică la fiecare rulare, fără efect.");
        RaporteazaInchidere(planificate.Count);
        Console.WriteLine($"  Luna {luna:00}/{an}: {documente} documente importate, {sarite} sărite, "
            + $"{copii} copii autogenerați operați, {esecuri} eșecuri, {realocari} realocări de lot "
            + $"({cantitate:N3} buc) — {rez.Durata:hh\\:mm\\:ss} "
            + $"(din care contractul {durataContract:hh\\:mm\\:ss}).");
        check($"Luna {luna:00}/{an}: importul documentelor fără eșecuri "
            + $"({documente} importate, {esecuri} eșuate)", esecuri == 0);
        return rez;
    }

    // Clasificarea unei unități executate, din ce s-a OBSERVAT (nu din ce declară
    // handlerele): un handler care iese devreme fără nicio acțiune cade în
    // `FaraActiune` și devine vizibil, în loc să dispară din aritmetică.
    void NoteazaSoarta(string tip) {
        var soarta = unitEsecuri > 0 ? SoartaUnitate.Esec
            : unitDocumente > 0 ? SoartaUnitate.Document
            : unitPunti > 0 ? SoartaUnitate.DoarPunte
            : unitFaraCorespondent > 0 ? SoartaUnitate.FaraCorespondent
            : unitSkipuri > 0 ? SoartaUnitate.Sarita
            : SoartaUnitate.FaraActiune;
        var cheie = (tip, soarta);
        soarteLuna[cheie] = soarteLuna.GetValueOrDefault(cheie) + 1;
        soarteRulare[cheie] = soarteRulare.GetValueOrDefault(cheie) + 1;
    }

    // Aritmetica de închidere a lunii (§12.4, cerința de observabilitate): unitățile
    // planificate se despart în categorii disjuncte, iar identitatea se VERIFICĂ —
    // o unitate neclasificată ar însemna un document-sursă pierdut pe drum.
    void RaporteazaInchidere(int planificate) {
        int Cate(SoartaUnitate s) => soarteLuna.Where(x => x.Key.Soarta == s).Sum(x => x.Value);
        var total = soarteLuna.Values.Sum();
        Console.WriteLine($"  Închiderea documentelor-sursă: {planificate} unități planificate = "
            + $"{Cate(SoartaUnitate.Document)} cu document + {Cate(SoartaUnitate.DoarPunte)} doar punte + "
            + $"{Cate(SoartaUnitate.FaraCorespondent)} fără corespondent + "
            + $"{Cate(SoartaUnitate.Sarita)} sărite cu motiv + "
            + $"{Cate(SoartaUnitate.FaraActiune)} fără acțiune (antet care nu postează) + "
            + $"{Cate(SoartaUnitate.Esec)} eșuate.");
        foreach (var g in soarteLuna.Where(x => x.Key.Soarta is SoartaUnitate.FaraActiune
                    or SoartaUnitate.Sarita or SoartaUnitate.FaraCorespondent)
                .OrderByDescending(x => x.Value))
            Console.WriteLine($"    {g.Value,8} × {g.Key.Tip} — {g.Key.Soarta}");
        if (motiveLuna.Count > 0) {
            Console.WriteLine($"  Motivele celor {sarite} skip-uri ale lunii "
                + "(toate, per tip × motiv):");
            foreach (var m in motiveLuna.OrderByDescending(x => x.Value))
                Console.WriteLine($"    {m.Value,8} × {m.Key.Tip}: {m.Key.Motiv}");
        }
        check($"Luna {luna:00}/{an}: aritmetica documentelor-sursă se închide "
            + $"({total} unități clasificate din {planificate} planificate)", total == planificate);
    }

    // Stingerile din subconto → `Imperechere`, trecerea 2 a lunii (§12.2 —
    // imperecherea nu postează registre, deci amânarea față de operare e gratuită
    // și scapă de problema de ordine). Vezi Imperecheri.cs.
    void Imperecheri(ContextLuna ctx) => Imperecheri1C.Executa(ctx);

    // PASUL 6: `InchidereTvaService.Genereaza` + operarea ei, la fine de lună
    // (§12.4 — fără ea, contractul de sold ar pica lunar pe 4426/4427/4423,
    // fiindcă Balanța 1C ARE închiderea în ea).
    //
    // Trece prin `ImportaDocument` ca orice document 1C, deși sursa nu e un
    // document 1C, și e deliberat: închiderea Atlas capătă astfel legătură (deci
    // idempotență la reluare ȘI o poziție corectă în invariantul „orice document
    // fără legătură e autogenerat de motor" din Program.cs), iar un eșec al ei
    // pică luna ca oricare altul. Cheia e luna însăși — o închidere per lună.
    //
    // Serviciul ÎȘI FACE singur idempotența (închidere vie ⇒ null) și cere
    // cronologie; aici nu se ocolește niciunul: gardienii lui sunt ai modelului,
    // nu ai importului.
    void InchidereTva(ContextLuna ctx) {
        // Gardul review-ului advers (D2a): o lună cu eșecuri de import are solduri
        // de TVA INCOMPLETE — închiderea generată acum ar îngheța cifre greșite,
        // iar rularea care repară documentele n-ar mai putea-o regenera (ITV viu ⇒
        // serviciul întoarce null). Se sare zgomotos; rularea reparată o generează.
        if (esecuri > 0) {
            avert($"Luna {ctx.Luna:00}/{ctx.An}: {esecuri} eșecuri de import — închiderea de TVA "
                + "NU se generează (ar prinde solduri incomplete); se generează la rularea care repară.");
            itvSarite++;
            return;
        }
        var stare = ImportaDocument("InchidereTva", $"{ctx.An:0000}-{ctx.Luna:00}",
            os => InchidereTvaService.Genereaza(os, ctx.An, ctx.Luna, Catalog.SediuId),
            regenerabilLaStorno: true,
            motivFaraDraft: "luna n-are ce închide (fără sold de TVA sau închidere deja vie)");
        if (stare == StareImport.Sarit)
            itvSarite++;
    }

    int itvSarite;

    // PASUL 6: contractul design §8 per lună — sold per cont OMFP, 4423/4424,
    // stoc per produs × gestiune; diferențele justificate se poartă înainte
    // dintr-o lună în alta (§12.4).
    ReconciliereLuna.Rezultat ReconciliereLunara(ContextLuna ctx) =>
        ReconciliereLuna.Executa(ctx, StareContract, avert, check);

    // Starea purtată între luni (plafonul netării, abaterile deja raportate);
    // Program.cs îi pune datele deschiderii înainte de prima lună.
    public ReconciliereLuna.Stare StareContract { get; } = new();

    // Valorile căzute pe jumătatea de ban în luna curentă, numărate DOAR în
    // materializarea prin motor (contractul 4 — D4/defect 5). 0 la o reluare care
    // nu mai importă nimic: contractul citește atunci cifra persistată a lunii.
    public long MidpointLuna { get; private set; }

    // Auto-testul contractului LUNAR (`--sabotaj`, partea a doua): două probe pe
    // prima lună procesată — un rând contabil și un rând de stoc ale unor
    // DOCUMENTE ale lunii, alterate după import și înaintea reconcilierii.
    // Alegerea țintelor și verdictul stau lângă contractul pe care îl probează
    // (Sabotaj.cs, partial din ReconciliereLuna): despărțirea lor a fost defectul
    // D6.
    //
    // Spre deosebire de sabotajul deschiderii, ăsta NU se vindecă: deschiderea se
    // rescrie la fiecare rulare, documentele nu. Baza rămâne alterată, deci o
    // rulare de sabotaj e o probă, nu un import.
    bool sabotajLuna;
    bool sabotajFacut;
    ReconciliereLuna.ProbeSabotaj probeSabotaj;

    // Verdictul auto-testului, citit de Program.cs pentru codul de ieșire. Null =
    // `--sabotaj` n-a rulat (sau n-a apucat să pună probele).
    public ReconciliereLuna.VerdictSabotaj Sabotaj { get; private set; }

    void SaboteazaLuna(ContextLuna ctx) {
        probeSabotaj = ReconciliereLuna.PuneProbele(ctx, StareContract, avert);
        // O lună în care NICIO probă nu s-a putut pune nu consumă auto-testul —
        // se încearcă luna următoare (review 1C-d-final, semnalare mică). Dacă
        // măcar una s-a pus, baza e deja alterată: verdictul se ia pe luna asta,
        // iar proba nepusă e raportată pe nume (exit 3).
        if (probeSabotaj.RandContabil != null || probeSabotaj.RandStoc != null)
            sabotajFacut = true;
        else
            probeSabotaj = null;
    }

    public void ActiveazaSabotajLuna() => sabotajLuna = true;

    // Verdictul se ia IMEDIAT după reconcilierea lunii sabotate: `Stare` ține
    // conturile și cheile picate ale lunii curente, iar luna următoare le golește.
    void VerificaSabotaj() {
        if (probeSabotaj == null || Sabotaj != null)
            return;
        Sabotaj = ReconciliereLuna.Verifica(probeSabotaj, StareContract);
        Console.WriteLine("\n  --- Verdictul auto-testului (--sabotaj) ---");
        foreach (var m in Sabotaj.Mesaje)
            Console.WriteLine($"     {m}");
        check("  sabotaj: proba CONTABILĂ detectată de contractul (1)", Sabotaj.ContabilDetectat);
        check("  sabotaj: proba de STOC detectată de contractul (3)", Sabotaj.StocDetectat);
    }

    public int ItvSarite => itvSarite;

    // ======================= Un document =======================

    // Idempotența per document (§12.4): legătura `1C:<view>` se scrie în ACELAȘI
    // commit cu draftul — documentele n-au cod natural de recuperare, spre
    // deosebire de nomenclatoare (47b), deci un commit separat ar lăsa duplicate
    // irecuperabile la o rulare întreruptă între cele două.
    //
    // `construiesteDraft` primește un ObjectSpace PROPRIU documentului și
    // întoarce draftul (sau null dacă sursa nu are ce importa — un document 1C
    // fără linii utile). Operarea se face imediat, în același ObjectSpace:
    // `MotorOperare.Opereaza` își comite singur tranzacția și întoarce copilul
    // autogenerat (conex NIR / secundar Plata), care se operează la rândul lui.
    // `regenerabilLaStorno` (review advers, D2b): pentru documentele UNELTEI
    // (azi doar ITV), stornarea e calea documentată de regenerare (46f) — la
    // reluare legătura moartă se șterge și documentul se regenerează, altfel
    // luna ar rămâne închisă pe cifre vechi pentru totdeauna. Documentele SURSEI
    // rămân la comportamentul conservator (stornat = sărit).
    //
    // `motivFaraDraft` = motivul de skip pe care îl declară APELANTUL pentru cazul
    // în care fabrica întoarce null (fără el, cazul ăsta incrementa `sarite` mut).
    // `punte` = documentul e o notă-punte, nu un tip al sursei: contează separat în
    // aritmetica de închidere („doar punte" ≠ „are document").
    public StareImport ImportaDocument(string view, string cheieHex,
            Func<IObjectSpace, Document> construiesteDraft, bool regenerabilLaStorno = false,
            string motivFaraDraft = null, bool punte = false) {
        var rezultat = Executa(view, cheieHex, construiesteDraft, regenerabilLaStorno, motivFaraDraft);
        if (rezultat is StareImport.Importat or StareImport.Reoperat) {
            if (punte)
                unitPunti++;
            else
                unitDocumente++;
        }
        Numara(view);
        return rezultat;
    }

    StareImport Executa(string view, string cheieHex, Func<IObjectSpace, Document> construiesteDraft,
            bool regenerabilLaStorno, string motivFaraDraft) {
        var cheie = (Legaturi.Tabela(view), cheieHex);
        // Draftul rămas de la o rulare anterioară, de dat înapoi înaintea
        // reimportului (D4). Se ține până DUPĂ construcția draftului nou: dacă
        // apelantul nu poate reconstrui (n-a planificat — cheia lui nu intră în
        // gardul de replanificare al handlerului), nu se șterge nimic și rămâne
        // comportamentul de până acum, re-operarea.
        var draftVechi = Guid.Empty;
        if (legaturi.TryGetValue(cheie, out var tinta)) {
            if (tinta == FaraDocument) {
                // Cheie decisă fără document de o rulare anterioară (F9): nu se
                // reconstruiește nimic — decizia stă până la `--deblocheaza`.
                Sare(view, motivFaraDraft ?? "cheie decisă fără document de o rulare anterioară");
                return StareImport.Sarit;
            }
            if (!stari.TryGetValue(tinta, out var stare)) {
                // Legătură fără document: baza a fost golită parțial. Se șterge
                // legătura moartă și se reimportă — altfel fiecare rulare ar
                // sări documentul crezând că există (mecanica 47b).
                avert($"Legătură orfană 1C:{view}/{cheieHex} — documentul lipsește; se reimportă.");
                StergeLegatura(view, cheieHex, cuDivergente: true);
                legaturi.Remove(cheie);
            }
            else if (stare == StareDocument.Operat) {
                // Crash între operarea documentului și cea a copilului autogenerat:
                // părintele e Operat, copilul a rămas Draft. Se termină lanțul.
                copii += OpereazaCopiiRamasi(tinta, view, cheieHex);
                Sare(view, "document deja importat de o rulare anterioară (operat)",
                    documentExista: true);
                return StareImport.Sarit;
            }
            else if (stare == StareDocument.Draft) {
                // D4: alocarea draftului e învechită. Documentele UNELTEI (ITV) se
                // dau înapoi ÎNAINTE de reconstrucție — generatorul lor își face
                // idempotența pe închiderea VIE a lunii (46c), deci un draft
                // existent l-ar face să întoarcă null și luna ar rămâne pe cifrele
                // rulării eșuate. Documentele SURSEI se reconstruiesc întâi (mai
                // jos): dacă handlerul n-a planificat, nu se pierde nimic.
                if (regenerabilLaStorno) {
                    if (!StergeDraftVechi(tinta, view, cheieHex)) {
                        Sare(view, "draftul uneltei rămas de la o rulare anterioară nu s-a putut șterge");
                        return StareImport.Sarit;
                    }
                    legaturi.Remove(cheie);
                }
                else {
                    // Gardianul se întreabă ÎNAINTE de reconstrucție (interogare
                    // read-only): un refuz de aici ar lăsa altfel în urmă un draft
                    // nou pe jumătate construit și loturi indexate care n-ajung
                    // niciodată în bază.
                    var refuz = Drafturi.Refuz(provider, tinta);
                    if (refuz != null) {
                        avert($"1C:{view}/{cheieHex}: draft rămas de la o rulare anterioară care NU se "
                            + $"poate șterge ({refuz}) — se lasă neatins, documentul rămâne neimportat. "
                            + "Rezolvă dependența și reia.");
                        Sare(view, "draft al unei rulări anterioare cu dependențe (nu se poate reface)");
                        return StareImport.Sarit;
                    }
                    draftVechi = tinta;
                }
            }
            else {
                if (!regenerabilLaStorno) {
                    avert($"1C:{view}/{cheieHex}: documentul din bază e {stare} — sărit "
                        + "(importul nu re-operează un document stornat).");
                    Sare(view, "document stornat în bază (importul nu re-operează un stornat)",
                        documentExista: true);
                    return StareImport.Sarit;
                }
                // Documentul uneltei, stornat deliberat (regenerarea 46f): legătura
                // moare, iar mai jos se generează unul proaspăt.
                avert($"1C:{view}/{cheieHex}: document al uneltei stornat — se regenerează.");
                StergeLegatura(view, cheieHex);
                legaturi.Remove(cheie);
            }
        }

        using var os = provider.CreateObjectSpace();
        Document doc;
        try {
            doc = construiesteDraft(os);
        }
        catch (Exception ex) {
            return Esec(view, cheieHex, "construcția draftului", ex);
        }
        if (doc == null) {
            // Apelantul n-a putut reconstrui (handlerul n-a planificat pentru cheia
            // asta — cazul cheilor secundare care nu intră în gardul lui). Draftul
            // vechi rămâne cum era și se re-operează: nu e ce vrea D4, dar e strict
            // mai bine decât să-l ștergem fără să-l putem înlocui.
            if (draftVechi != Guid.Empty)
                return ReOpereaza(draftVechi, view, cheieHex);
            Sare(view, motivFaraDraft ?? "sursa n-are ce importa pe cheia asta (motiv NEDECLARAT de handler)");
            return StareImport.Sarit;
        }
        // Draftul vechi pleacă abia acum, când înlocuitorul lui e construit (dar
        // încă necomis): între ștergere și commit-ul de mai jos nu mai poate cădea
        // nicio decizie.
        if (draftVechi != Guid.Empty) {
            if (!StergeDraftVechi(draftVechi, view, cheieHex)) {
                Sare(view, "draftul rămas de la o rulare anterioară nu s-a putut șterge");
                return StareImport.Sarit;
            }
            legaturi.Remove(cheie);
        }
        // Cheia scrisă în legătură e ID-ul obiectului ÎNAINTE de commit: EF îl
        // generează la `Add` (valoare reală, nu temporară, pentru chei Guid), iar
        // toată idempotența §12.4 stă pe asta. Dacă ipoteza ar cădea vreodată,
        // trebuie să cadă zgomotos AICI, nu prin legături spre `Guid.Empty`.
        if (doc.ID == Guid.Empty)
            throw new InvalidOperationException(
                $"1C:{view}/{cheieHex}: documentul nu are ID înainte de commit — legătura nu se "
                + "poate scrie în același commit cu draftul (§12.4).");
        try {
            Legaturi.Leaga(os, view, cheieHex, doc.ID);
            // Evidența supapei de alocare și registrul divergențelor merg în ACELAȘI
            // commit cu documentul (altfel n-ajung nicăieri — ObjectSpace-ul de
            // planificare se aruncă).
            Alocare.Persista(os);
            Divergente.Persista(os);
            os.CommitChanges();
        }
        catch (Exception ex) {
            return Esec(view, cheieHex, "commit-ul draftului + legăturii", ex);
        }
        Alocare.Confirma();
        Divergente.Confirma();
        legaturi[cheie] = doc.ID;
        stari[doc.ID] = StareDocument.Draft;

        var rezultat = Opereaza(os, doc, view, cheieHex);
        if (rezultat == StareImport.Importat)
            documente++;
        return rezultat;
    }

    // D4: draftul unei rulări eșuate se dă înapoi INTEGRAL (documentul, liniile,
    // loturile născute de ele, legăturile lor) — vezi `Drafturi`. Indexurile din
    // memorie se curăță odată cu baza: `stari`/`copiiAutogenerati` (altfel copilul
    // șters ar fi „operat" mai târziu) și indexul de loturi al catalogului (altfel
    // un pin de mai târziu ar rezolva pe un lot inexistent).
    public int DrafturiSterse { get; private set; }
    public int DrafturiRefuzate { get; private set; }

    bool StergeDraftVechi(Guid id, string view, string cheieHex) {
        // Divergențele înregistrate de rularea care a produs draftul pleacă odată cu
        // el: planificarea nouă le rescrie pe ale ei, iar un rând rămas ar explica o
        // diferență care nu mai există. Merg în ACELAȘI ObjectSpace și în același
        // commit cu ștergerea documentului (D5): două tranzacții lăsau, la un crash
        // între ele, exact orfanii pe care mecanismul îi repară.
        Action confirmaDivergente = null;
        var rezultat = Drafturi.Sterge(provider, id, out var refuz,
            os => confirmaDivergente = Divergente.UitaSursa(os, RegistruDivergente.Sursa(view, cheieHex)));
        if (rezultat == null) {
            DrafturiRefuzate++;
            avert($"1C:{view}/{cheieHex}: draftul rămas de la o rulare anterioară NU s-a putut șterge "
                + $"({refuz}) — documentul rămâne neimportat.");
            return false;
        }
        DrafturiSterse++;
        confirmaDivergente?.Invoke();
        avert($"1C:{view}/{cheieHex}: draft rămas de la o rulare anterioară (alocare învechită) — "
            + $"șters și reimportat: {rezultat.Documente} documente, {rezultat.Linii} linii, "
            + $"{rezultat.Loturi} loturi, {rezultat.Registre} rânduri de registru, "
            + $"{rezultat.Legaturi} legături.");
        foreach (var lotId in rezultat.LoturiSterse)
            Catalog.UitaLot(lotId);
        foreach (var copil in copiiAutogenerati.GetValueOrDefault(id) ?? new List<Guid>())
            stari.Remove(copil);
        copiiAutogenerati.Remove(id);
        stari.Remove(id);
        return true;
    }

    StareImport ReOpereaza(Guid id, string view, string cheieHex) {
        using var os = provider.CreateObjectSpace();
        var doc = os.GetObjectByKey<Document>(id);
        if (doc == null) {
            avert($"Legătură 1C:{view}/{cheieHex} către documentul {id}, negăsit la re-operare.");
            Sare(view, "documentul legăturii nu s-a găsit la re-operare");
            return StareImport.Sarit;
        }
        var rezultat = Opereaza(os, doc, view, cheieHex);
        if (rezultat == StareImport.Importat) {
            documente++;
            rezultat = StareImport.Reoperat;
        }
        return rezultat;
    }

    // Operarea + lanțul de copii autogenerați (conexul NIR al facturii, plata
    // secundară — 26d/31e): `Opereaza` întoarce copilul, care poate genera la
    // rândul lui. Lanțul e scurt prin construcție; limita e o plasă de siguranță.
    StareImport Opereaza(IObjectSpace os, Document doc, string view, string cheieHex) {
        // Fereastra de măsurare a contractului 4: doar ce rotunjește MOTORUL la
        // materializare (aici trece tot: documentul, copiii autogenerați, ITV).
        var midpointInainte = Scara.MidpointBani;
        try {
            var curent = doc;
            for (var pas = 0; curent != null && pas < 5; pas++) {
                var copil = MotorOperare.Opereaza(os, curent);
                stari[curent.ID] = StareDocument.Operat;
                // Pasul 0 e documentul propriu-zis; de la 1 încolo se operează
                // copii autogenerați (conexul NIR, plata secundară).
                if (pas > 0)
                    copii++;
                if (copil != null) {
                    stari[copil.ID] = copil.Stare;
                    if (copil.DocumentSursaId is { } sursa)
                        (copiiAutogenerati.TryGetValue(sursa, out var lista)
                            ? lista : copiiAutogenerati[sursa] = []).Add(copil.ID);
                }
                curent = copil;
            }
            return StareImport.Importat;
        }
        catch (Exception ex) {
            // Draftul rămâne în bază, legat: la reluare intră pe calea
            // „legat + Draft → re-operare" (§12.4), fără duplicat.
            return Esec(view, cheieHex, "operarea", ex);
        }
        finally {
            MidpointLuna += Scara.MidpointBani - midpointInainte;
        }
    }

    int OpereazaCopiiRamasi(Guid parinteId, string view, string cheieHex) {
        if (!copiiAutogenerati.TryGetValue(parinteId, out var lista))
            return 0;
        var operati = 0;
        foreach (var copilId in lista.ToList()) {
            if (stari.GetValueOrDefault(copilId) != StareDocument.Draft)
                continue;
            using var os = provider.CreateObjectSpace();
            var copil = os.GetObjectByKey<Document>(copilId);
            if (copil == null)
                continue;
            avert($"1C:{view}/{cheieHex}: copil autogenerat rămas în Draft (rulare întreruptă "
                + "între operarea sursei și a lui) — se operează acum.");
            if (Opereaza(os, copil, view, cheieHex) == StareImport.Importat)
                operati++;
        }
        return operati;
    }

    // Eșecul unui document e DIAGNOSTIC, nu verdict: verdictul e al lunii (un
    // singur `check` la final, §12.4 — unitatea de contract e luna). Detaliile se
    // plafonează, altfel o rulare de recoltat găuri (`--continua`) ar scoate zeci
    // de mii de linii identice și ar îneca raportul; contorul rămâne complet.
    const int DetaliiEsecPeLuna = 20;

    StareImport Esec(string view, string cheieHex, string faza, Exception ex) {
        esecuri++;
        unitEsecuri++;
        // Excepțiile de persistență (EF) își țin cauza REALĂ în inner exception
        // („An error occurred while saving the entity changes" nu spune nimic):
        // se desfășoară tot lanțul, altfel diagnosticul cere un debugger.
        var cauze = new List<string>();
        for (var e = ex; e != null; e = e.InnerException)
            cauze.Add(e.Message);
        var mesaj = $"1C:{view}/{cheieHex} ({luna:00}/{an}) — {faza} a eșuat: {string.Join(" ← ", cauze)}";
        if (esecuri <= DetaliiEsecPeLuna) {
            Console.WriteLine($"  EȘEC {mesaj}");
            avert(mesaj);
        }
        else if (esecuri == DetaliiEsecPeLuna + 1) {
            var nota = $"Luna {luna:00}/{an}: peste {DetaliiEsecPeLuna} eșecuri — restul nu se mai "
                + "detaliază (contorul lunii rămâne complet).";
            Console.WriteLine($"  EȘEC {nota}");
            avert(nota);
        }
        return StareImport.Esec;
    }

    // `cuDivergente` (D5): legătura moartă înseamnă că documentul pe care îl
    // descria nu mai există, deci nici divergențele înregistrate de rularea care
    // l-a produs nu mai descriu un fapt — pleacă odată cu ea, în ACELAȘI commit
    // (un commit separat ar lăsa exact orfanii pe care mecanismul îi repară).
    // Nu pleacă la ștergerea legăturii unui document al UNELTEI stornat
    // deliberat (ITV): acolo documentul EXISTĂ în bază, iar regenerarea lui își
    // rescrie oricum rândurile la primul rând nou.
    void StergeLegatura(string view, string cheieHex, bool cuDivergente = false) {
        using var os = provider.CreateObjectSpace();
        var tabela = Legaturi.Tabela(view);
        var moarta = os.FirstOrDefault<MigrareLegatura>(
            m => m.Tabela == tabela && m.CheieLegacy == cheieHex);
        var confirmaDivergente = cuDivergente
            ? Divergente.UitaSursa(os, RegistruDivergente.Sursa(view, cheieHex))
            : null;
        if (moarta != null)
            os.Delete(moarta);
        if (moarta != null || confirmaDivergente != null)
            os.CommitChanges();
        confirmaDivergente?.Invoke();
    }

    // Progresul (§12.4): contoare per tip la fiecare 1.000 de documente atinse,
    // ca o rulare de ore să spună unde e, nu doar că trăiește.
    void Numara(string view) {
        peTip[view] = peTip.GetValueOrDefault(view) + 1;
        var total = documente + sarite;
        if (total > 0 && total % 1000 == 0)
            Console.WriteLine($"    …{luna:00}/{an}: {documente} importate / {sarite} sărite "
                + $"({cronometruLuna.Elapsed:hh\\:mm\\:ss}) — "
                + string.Join(", ", peTip.OrderByDescending(x => x.Value).Select(x => $"{x.Key} {x.Value}")));
    }
}
