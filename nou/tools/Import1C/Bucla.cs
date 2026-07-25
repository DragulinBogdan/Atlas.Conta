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
sealed record ContextLuna(int An, int Luna, DateOnly Prima, DateOnly Ultima, BuclaImport Bucla);

// Un tip de document 1C și rutina care îi mătură luna. `TipRecorder` e numele
// tipului din sursă (coloana tipizată `DocReferinta_<Tip>` — vezi FlaxDb) și e
// cheia cu care pre-flight-ul verifică acoperirea.
sealed record HandlerTip(string TipRecorder, string Descriere, Action<ContextLuna> Importa);

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
    // Ordinea din listă = ordinea ÎN INTERIORUL lunii, iar criteriul e unul
    // singur: **tot ce ADAUGĂ stoc înaintea a tot ce CONSUMĂ stoc.** Gardianul de
    // sold e prefix-sum pe zile (25d), deci datele nu contează pentru solduri —
    // dar o ieșire planificată înainte ca intrarea ei să existe în registru nu-și
    // găsește acoperirea și cade în supapa 48a, cu tot cortegiul de realocări și
    // transcrieri. Măsurat pe ianuarie: ordinea greșită lăsa 190 de documente
    // nematerializate la prima trecere (le prindea a doua rulare).
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

    public FlaxDb Flax { get; }
    public ImportLaCerere LaCerere { get; }
    public AlocareIesire Alocare { get; }
    public Catalog Catalog { get; }
    public ContorPunti ContorPunti { get; } = new();
    public Action<string> Avert => avert;

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

    public void NumaraSursaFaraCorespondent() => SurseFaraCorespondent++;

    // Eșecul de PLANIFICARE (handlerele pasului 3 planifică înaintea lui
    // `ImportaDocument`, ca puntea să se poată scrie prima). Fără el, o excepție
    // la un document ar urca până la prinderea de la nivelul handlerului și ar
    // opri tot tipul pe luna aia; aici rămâne ce trebuie să fie — eșecul UNUI
    // document, numărat și detaliat ca oricare altul.
    public void EsecPlanificare(string view, string cheie, Exception ex) =>
        Esec(view, cheie, "planificarea", ex);

    // Documentul e deja cunoscut (importat sau lăsat Draft de o rulare
    // întreruptă)? Handlerele o întreabă ca să NU replanifice degeaba la reluare;
    // decizia propriu-zisă (skip / re-operare) rămâne a lui `ImportaDocument`.
    // O legătură ORFANĂ (documentul a dispărut din bază) nu contează ca
    // „cunoscut": `Executa` o șterge și cere draftul din nou, deci planul trebuie
    // să existe — altfel reluarea ar intra în materializare cu mâna goală.
    public bool EsteCunoscut(string view, string cheie) =>
        legaturi.TryGetValue((Legaturi.Tabela(view), cheie), out var tinta) && stari.ContainsKey(tinta);

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
        foreach (var l in os.GetObjectsQuery<MigrareLegatura>()
                     .Select(m => new { m.Tabela, m.CheieLegacy, m.TintaId }).ToList())
            legaturi[(l.Tabela, l.CheieLegacy)] = l.TintaId;
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
        Alocare.IncepeLuna();
        cronometruLuna = Stopwatch.StartNew();
        var prima = new DateOnly(an, luna, 1);
        var ctx = new ContextLuna(an, luna, prima, prima.AddMonths(1).AddDays(-1), this);

        Console.WriteLine($"\n--- Luna {luna:00}/{an} ---");
        RanduriLuna = Flax.RanduriNotaPeLuna(an, luna);
        SubcontoLuna = Flax.SubcontoNotaPeLuna(an, luna);
        foreach (var h in Handlere.Toate) {
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

        Imperecheri(ctx);
        InchidereTva(ctx);
        if (sabotajLuna && !sabotajFacut)
            SaboteazaLuna(ctx);
        var cronometruContract = Stopwatch.StartNew();
        var contract = ReconciliereLunara(ctx);
        var durataContract = cronometruContract.Elapsed;

        var (realocari, cantitate) = Alocare.DeltaLunii();
        var rez = new RezultatLuna(an, luna, documente, sarite, copii, esecuri,
            realocari, cantitate, cronometruLuna.Elapsed,
            contract.Picate, contract.TvaDePlata, durataContract);
        Handlere.Raporteaza();
        ContorPunti.Raporteaza();
        if (SurseFaraCorespondent > 0)
            Console.WriteLine($"  {SurseFaraCorespondent} documente-sursă fără corespondent în Atlas "
                + "(nici document, nici punte) — se replanifică la fiecare rulare, fără efect.");
        Console.WriteLine($"  Luna {luna:00}/{an}: {documente} documente importate, {sarite} sărite, "
            + $"{copii} copii autogenerați operați, {esecuri} eșecuri, {realocari} realocări de lot "
            + $"({cantitate:N3} buc) — {rez.Durata:hh\\:mm\\:ss} "
            + $"(din care contractul {durataContract:hh\\:mm\\:ss}).");
        check($"Luna {luna:00}/{an}: importul documentelor fără eșecuri "
            + $"({documente} importate, {esecuri} eșuate)", esecuri == 0);
        return rez;
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
        var stare = ImportaDocument("InchidereTva", $"{ctx.An:0000}-{ctx.Luna:00}",
            os => InchidereTvaService.Genereaza(os, ctx.An, ctx.Luna, Catalog.SediuId));
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

    // Auto-testul contractului LUNAR (`--sabotaj`, partea a doua): +1 leu pe un
    // rând de registru al unui DOCUMENT din prima lună procesată, după import și
    // înaintea reconcilierii. Contul se alege din AFARA bucket-ului netării —
    // altfel diferența ar fi (corect) declarată justificată, iar proba n-ar
    // dovedi nimic.
    //
    // Spre deosebire de sabotajul deschiderii, ăsta NU se vindecă: deschiderea se
    // rescrie la fiecare rulare, documentele nu. Baza rămâne alterată, deci o
    // rulare de sabotaj e o probă, nu un import.
    bool sabotajLuna;
    bool sabotajFacut;

    public void ActiveazaSabotajLuna() => sabotajLuna = true;

    void SaboteazaLuna(ContextLuna ctx) {
        sabotajFacut = true;
        using var os = provider.CreateObjectSpace();
        var candidat = os.GetObjectsQuery<RegistruContabil>()
            .Where(r => r.DocumentId != null && r.Data >= ctx.Prima && r.Data <= ctx.Ultima)
            .Select(r => new { r.ID, Debit = r.ContDebit.Simbol, Credit = r.ContCredit.Simbol, r.Valoare })
            .ToList()
            .Where(r => !ReconciliereLuna.EsteInBucketNetare(r.Debit)
                && !ReconciliereLuna.EsteInBucketNetare(r.Credit))
            .OrderBy(r => r.ID)
            .FirstOrDefault();
        if (candidat == null) {
            avert($"--sabotaj: luna {ctx.Luna:00}/{ctx.An} n-are niciun rând de document în afara "
                + "bucket-ului netării — proba contractului lunar nu s-a putut face.");
            return;
        }
        var rand = os.GetObjectByKey<RegistruContabil>(candidat.ID);
        rand.Valoare += 1m;
        os.CommitChanges();
        Console.WriteLine($"\n*** SABOTAJ (--sabotaj): +1 leu pe rândul contabil {candidat.ID} "
            + $"({candidat.Debit} = {candidat.Credit}, {candidat.Valoare:N2}) al lunii "
            + $"{ctx.Luna:00}/{ctx.An}. Contractul (1) al lunii TREBUIE să pice pe ambele conturi. ***");
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
    public StareImport ImportaDocument(string view, string cheieHex,
            Func<IObjectSpace, Document> construiesteDraft) {
        var rezultat = Executa(view, cheieHex, construiesteDraft);
        Numara(view);
        return rezultat;
    }

    StareImport Executa(string view, string cheieHex, Func<IObjectSpace, Document> construiesteDraft) {
        var cheie = (Legaturi.Tabela(view), cheieHex);
        if (legaturi.TryGetValue(cheie, out var tinta)) {
            if (!stari.TryGetValue(tinta, out var stare)) {
                // Legătură fără document: baza a fost golită parțial. Se șterge
                // legătura moartă și se reimportă — altfel fiecare rulare ar
                // sări documentul crezând că există (mecanica 47b).
                avert($"Legătură orfană 1C:{view}/{cheieHex} — documentul lipsește; se reimportă.");
                StergeLegatura(view, cheieHex);
                legaturi.Remove(cheie);
            }
            else if (stare == StareDocument.Operat) {
                // Crash între operarea documentului și cea a copilului autogenerat:
                // părintele e Operat, copilul a rămas Draft. Se termină lanțul.
                copii += OpereazaCopiiRamasi(tinta, view, cheieHex);
                sarite++;
                return StareImport.Sarit;
            }
            else if (stare == StareDocument.Draft)
                return ReOpereaza(tinta, view, cheieHex);
            else {
                avert($"1C:{view}/{cheieHex}: documentul din bază e {stare} — sărit "
                    + "(importul nu re-operează un document stornat).");
                sarite++;
                return StareImport.Sarit;
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
            sarite++;
            return StareImport.Sarit;
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
            os.CommitChanges();
        }
        catch (Exception ex) {
            return Esec(view, cheieHex, "commit-ul draftului + legăturii", ex);
        }
        legaturi[cheie] = doc.ID;
        stari[doc.ID] = StareDocument.Draft;

        var rezultat = Opereaza(os, doc, view, cheieHex);
        if (rezultat == StareImport.Importat)
            documente++;
        return rezultat;
    }

    StareImport ReOpereaza(Guid id, string view, string cheieHex) {
        using var os = provider.CreateObjectSpace();
        var doc = os.GetObjectByKey<Document>(id);
        if (doc == null) {
            avert($"Legătură 1C:{view}/{cheieHex} către documentul {id}, negăsit la re-operare.");
            sarite++;
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

    void StergeLegatura(string view, string cheieHex) {
        using var os = provider.CreateObjectSpace();
        var tabela = Legaturi.Tabela(view);
        var moarta = os.FirstOrDefault<MigrareLegatura>(
            m => m.Tabela == tabela && m.CheieLegacy == cheieHex);
        if (moarta != null) {
            os.Delete(moarta);
            os.CommitChanges();
        }
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
