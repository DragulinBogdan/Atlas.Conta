using Atlas.Conta.BackOffice.Module.BusinessObjects;
using DevExpress.ExpressApp;

namespace Import1C;

// PASUL 3 al feliei 1C-c: datele „de fundal" ale handlerelor — planul de conturi,
// gestiunile, Clasă/Tip, cotele de TVA, identitatea tipurilor 1C și indexul de
// loturi. Se construiește O DATĂ, după seed și nomenclatoare, și se citește de
// toate handlerele (bucla îl poartă, ca serviciile din ContextLuna).
//
// Ce NU e aici: nimic care ține de un tip de document anume. Catalogul răspunde
// la întrebări („ce Tip are contul 302.8?", „unde e lotul ăsta?"), politica de
// import rămâne a handlerului.

// Clasă/Tip, pre-încărcat: `Registru` oglindește rândurile RegulaStoc private
// (generic → Magazie, MF → Marfuri) — handlerele au nevoie de registru înaintea
// motorului, ca să ceară supapei de alocare soldul din cutia corectă.
// `Simbol` = contul pe care Tipul chiar POSTEAZĂ (ContImplicit) — diferă de
// `Cod` doar la gemenii ne-stoc („S371" postează pe 371). Punțile îl folosesc pe
// EL: declararea pe Cod ar produce simboluri inexistente în plan („S371"), iar
// nota de punte ar pica la materializare (defect D5 al review-ului advers).
sealed record TipInfo(Guid Id, string Cod, string ClasaCod, NaturaClasa Natura, string Simbol) {
    public TipStoc Registru => ClasaCod == "MF" ? TipStoc.Marfuri : TipStoc.Magazie;
}

// Un lot, așa cum îl vede importul: identitatea Atlas + SIMBOLUL pe care s-a
// născut. Simbolul e cel care decide registrul de stoc în care stă soldul lui
// (deschiderea l-a scris așa — 47d), deci ieșirile trebuie să-l folosească pe el,
// nu contul de pe rândul 1C care poate să fi fost reclasificat între timp.
sealed record LotImport(Guid Id, string Simbol);

sealed class Catalog {
    readonly IObjectSpaceProvider provider;
    readonly Action<string> avert;

    public Func<string, string> Mapeaza { get; }
    public IReadOnlyDictionary<string, Guid> Plan { get; }
    public IReadOnlyDictionary<string, Guid> Gestiuni { get; }
    // Conturile proprii importate (casierii + conturi bancare, 47b): laturile
    // încasărilor generate de import (cardul retailului, încasarea în numerar).
    public IReadOnlyDictionary<string, Guid> ConturiProprii { get; }
    // Persoanele fizice importate → `Angajat` (47b): contrapartida plăților și
    // încasărilor pe care 1C le ține pe persoană, nu pe partener (pasul 5).
    public IReadOnlyDictionary<string, Guid> Angajati { get; }
    public Guid SediuId { get; }
    public Guid ComisieId { get; }
    public Guid TipTrezorerieId { get; }
    // Partenerul generic de retail (seed privat, decizia 48b): latura FCL-ului
    // surogat al raportului de vânzări cu amănuntul.
    public Guid ConsumatorFinalId { get; }

    readonly Dictionary<string, TipInfo> tipuri = new(StringComparer.Ordinal);
    readonly Dictionary<string, Guid> tipuriTva = new(StringComparer.Ordinal);
    readonly Dictionary<Guid, string> simboluriContPropriu = [];
    readonly Dictionary<string, string> tipRefPeNume = new(StringComparer.Ordinal);

    // Indexul de loturi: cheia canonică (convenția `Deschidere.CheieLot`) →
    // lotul Atlas. Aliasurile de reclasificare stau într-o tabelă SEPARATĂ de
    // legături („1C:LotAlias"), tocmai ca indexul canonic să rămână fără
    // ambiguitate — simbolul de naștere al lotului se citește din cheia
    // canonică, iar un alias scris peste ea l-ar face nedeterminabil la reluare.
    readonly Dictionary<string, LotImport> loturi = new(StringComparer.Ordinal);
    // Prefixul (document creator × nomenclator) → loturile lui, pentru pin-urile
    // care vin cu ALT cont decât cel de naștere (reclasificările BTR).
    readonly Dictionary<string, List<LotImport>> loturiPePrefix = new(StringComparer.Ordinal);
    // Lotul după ID-ul Atlas: aliasurile trebuie să trimită la simbolul CANONIC,
    // nu la cel din cheia lor. De când produsul e (nomenclator × cont), simbolul
    // lotului determină și Tipul liniei de ieșire — un alias care ar raporta
    // simbolul lui ar trimite linia pe produsul geamăn greșit, iar motorul ar
    // refuza-o („Lotul liniei aparține unui produs cu alt Tip decât Tipul liniei").
    readonly Dictionary<Guid, LotImport> loturiPeId = [];

    public int TipuriMaterialNoi { get; private set; }
    public int LoturiRezolvatePePrefix { get; private set; }
    public int LoturiNerezolvate { get; private set; }
    readonly HashSet<string> tipuriLipsa = new(StringComparer.Ordinal);
    readonly HashSet<string> coteNecunoscute = new(StringComparer.Ordinal);

    public Catalog(IObjectSpaceProvider provider, FlaxDb flax, int an,
            Func<string, string> mapeaza, IReadOnlyDictionary<string, Guid> plan, Action<string> avert) {
        this.provider = provider;
        this.avert = avert;
        Mapeaza = mapeaza;
        Plan = plan;

        using var os = provider.CreateObjectSpace();
        Gestiuni = Legaturi.Incarca(os, "Depozite");
        var proprii = new Dictionary<string, Guid>(Legaturi.Incarca(os, "Casierii"), StringComparer.Ordinal);
        foreach (var (cheie, id) in Legaturi.Incarca(os, "ConturiBancare"))
            proprii[cheie] = id;
        ConturiProprii = proprii;
        Angajati = Legaturi.Incarca(os, "PersoaneFizice");
        SediuId = CereRepartitor(os, "SEDIU");
        ComisieId = CereRepartitor(os, "COMISIE");
        ConsumatorFinalId = CereRepartitor(os, "CF");
        foreach (var t in os.GetObjectsQuery<TipMaterial>()
                     .Select(t => new { t.ID, t.Cod, ClasaCod = t.Clasa.Cod, t.Clasa.Natura,
                         SimbolCont = t.ContImplicit.Simbol }).ToList())
            tipuri[t.Cod] = new TipInfo(t.ID, t.Cod, t.ClasaCod, t.Natura, t.SimbolCont ?? t.Cod);
        TipTrezorerieId = tipuri.TryGetValue("TRZ", out var trz)
            ? trz.Id
            : throw new InvalidOperationException("Profilul privat nu are Tipul tehnic TRZ (seed).");
        foreach (var t in os.GetObjectsQuery<TipTva>().Select(t => new { t.ID, t.Cod }).ToList())
            tipuriTva[t.Cod] = t.ID;
        // Simbolul contului propriu (5311/5121/5125…): handlerele au nevoie de el
        // ca să declare în punte CE postează Atlas pe latura de trezorerie.
        foreach (var c in os.GetObjectsQuery<ContPropriu>()
                     .Select(c => new { c.ID, Simbol = c.ContImplicit.Simbol }).ToList())
            simboluriContPropriu[c.ID] = c.Simbol;

        // Identitatea 1C a tipurilor de document (TypeRef) — intră în cheia de
        // lot. Se ia din recensământul Recorder al anului, adică din DATE: un
        // dicționar hardcodat de TypeRef-uri ar fi exact contractul pe care §2
        // îl refuză.
        foreach (var t in flax.TipuriRecorder(an).Where(t => t.Nume != null))
            tipRefPeNume[t.Nume] = t.TypeRef;

        IncarcaContare(os);

        foreach (var (cheie, id) in Legaturi.Incarca(os, "Lot"))
            IndexeazaLot(cheie, id);
        // Aliasurile de reclasificare NU intră în index cu simbolul lor (ar minți
        // despre registrul în care stă lotul ȘI despre produsul lui): trimit la
        // lotul canonic, cu simbolul LUI. Un alias al cărui canonic lipsește n-ar
        // trebui să existe (aliasurile se scriu doar pentru loturi deja legate),
        // deci nu se inventează un simbol — se raportează și se sare.
        foreach (var (cheie, id) in Legaturi.Incarca(os, "LotAlias")) {
            if (loturiPeId.TryGetValue(id, out var canonic))
                loturi.TryAdd(cheie, canonic);
            else
                avert($"Alias de lot 1C:LotAlias/{cheie} → {id} fără lot canonic în index — "
                    + "ignorat (simbolul lui nu se poate deduce din cheia aliasului).");
        }
    }

    // Contul propriu al ÎNCASĂRILOR PE CARD (5125 „Sume în curs de decontare"):
    // 1C nu-l ține ca nomenclator — e doar un cont de evidență pe rândurile
    // 512.5 = 411.1 — dar latura încasării Atlas cere un `ContPropriu`. Se
    // creează la prima cerere, cheiat pe cod natural (recuperabil la reluare, ca
    // nomenclatoarele mici — 47b), și rămâne în cache pe durata rulării.
    public Guid ContPropriuCard() {
        if (contCard is { } id)
            return id;
        using var os = provider.CreateObjectSpace();
        var cont = os.FirstOrDefault<ContPropriu>(c => c.Cod == "5125");
        if (cont == null) {
            cont = os.CreateObject<ContPropriu>();
            cont.Cod = "5125";
            cont.Denumire = "Sume în curs de decontare (card)";
            cont.EsteBanca = true;
            avert("Cont propriu creat de import: „5125” (sume în curs de decontare) — "
                + "latura încasărilor pe card, pe care 1C le ține doar ca simbol de cont.");
        }
        cont.ContImplicitId = Plan.TryGetValue("5125", out var simbol) ? simbol : null;
        os.CommitChanges();
        contCard = cont.ID;
        simboluriContPropriu[cont.ID] = "5125";
        return cont.ID;
    }

    Guid? contCard;

    // Contul de evidență al unui cont propriu (latura de trezorerie pe care o
    // postează motorul prin `SursaCont.Repartitor*`).
    public string SimbolContPropriu(Guid id) => simboluriContPropriu.GetValueOrDefault(id);

    // Contul pe care motorul îl pune pe latura de CREANȚĂ a facturii de ieșire /
    // încasării: `SursaCont.Repartitor*` citește `ContImplicit`-ul partenerului,
    // iar partenerii importați din 1C nu au unul (47b) ⇒ fallback-ul regulii.
    // Constantă, nu interogare per document: dacă vreodată importul va seta
    // conturi implicite pe parteneri, aici e locul care trebuie să afle.
    public const string ContCreantaImplicit = "4111";

    // Simetricul, pe latura de DATORIE: contul pe care regula plății îl pune pe
    // debit (`SursaCont.RepartitorPrimitor`, fallback-ul seed-ului privat) când
    // partenerul n-are cont implicit — adică întotdeauna, la partenerii importați.
    public const string ContDatorieImplicit = "401";

    static Guid CereRepartitor(IObjectSpace os, string cod) =>
        os.FirstOrDefault<Repartitor>(r => r.Cod == cod)?.ID
            ?? throw new InvalidOperationException($"Profilul privat nu are repartitorul {cod} (seed).");

    // ======================= Clasă/Tip =======================

    public TipInfo Tip(string simbol) =>
        simbol != null && tipuri.TryGetValue(simbol, out var t) ? t : null;

    // Tipul unei linii de STOC: trebuie să existe în profil și să fie de natură
    // Stoc. Lipsa lui e o GAURĂ DE PROFIL (decizia 21) — se raportează o singură
    // dată per simbol și linia eșuează zgomotos, nu se inventează un Tip de stoc.
    public TipInfo TipStocPentru(string simbol) {
        var t = Tip(simbol);
        if (t != null && t.Natura == NaturaClasa.Stoc)
            return t;
        if (tipuriLipsa.Add($"S|{simbol}"))
            avert($"Profilul privat nu are TipMaterial de STOC cu codul „{simbol}” — "
                + "liniile de pe contul ăsta nu se pot importa (gaură de profil, decizia 21).");
        return null;
    }

    // Tipul unei linii de SERVICIU/CHELTUIALĂ/IMOBILIZARE: se creează la cerere
    // (Cod = simbolul de cont, decizia 26b), în clasa potrivită naturii. Natura
    // se alege din clasa contului: 2xx = imobilizare (creditul cade pe 404 prin
    // regula FCT), restul = serviciu. Un simbol care are deja Tip de STOC în
    // profil (ex. o linie de servicii pe contul de marfă) primește un geamăn
    // „S<simbol>" — altfel linia ar intra în filtrul de natură al conexului NIR
    // și ar cere lot pe care nu-l are.
    public TipInfo TipCheltuialaPentru(string simbol) => TipNestocPentru(simbol);

    // Același mecanism, pe latura de VENIT (pasul 4): contul de venit al liniei
    // 1C (707.1/704.x/418/419.1…) devine Cod-ul unui TipMaterial de natură
    // Serviciu, iar regula generică de facturare îl postează pe credit
    // (`SursaCont.TipMaterial`). Tipurile de venit ale profilului (704/706/707/
    // 708 — clasa VEN) sunt deja în seed și se refolosesc ca atare.
    public TipInfo TipVenitPentru(string simbol) => TipNestocPentru(simbol);

    // Contul e de STOC în profil? (fără avertisment — se folosește la
    // CLASIFICAREA rândurilor 1C, unde absența e informație, nu gaură.)
    public bool EsteContDeStoc(string simbol) => Tip(simbol)?.Natura == NaturaClasa.Stoc;

    public TipInfo TipNestocPentru(string simbol) {
        if (simbol == null)
            return null;
        var existent = Tip(simbol);
        if (existent != null && existent.Natura != NaturaClasa.Stoc)
            return existent;
        var cod = existent == null ? simbol : "S" + simbol;
        if (tipuri.TryGetValue(cod, out var geaman))
            return geaman;
        if (!Plan.TryGetValue(simbol, out var contId)) {
            if (tipuriLipsa.Add($"C|{simbol}"))
                avert($"Contul „{simbol}” nu există în planul OMFP — nu se poate crea TipMaterial "
                    + "pentru liniile de cheltuială de pe el.");
            return null;
        }
        var imobilizare = simbol.StartsWith('2');
        var clasaCod = imobilizare ? "F" : "S";
        using var os = provider.CreateObjectSpace();
        var clasa = os.FirstOrDefault<ClasaProdus>(c => c.Cod == clasaCod)
            ?? throw new InvalidOperationException($"Profilul privat nu are clasa {clasaCod} (seed).");
        // Recuperarea rulării întrerupte, ca la nomenclatoare (47b): Tipul e
        // cheiat pe cod natural, deci se adoptă în loc să se dubleze.
        var tip = os.FirstOrDefault<TipMaterial>(t => t.Cod == cod);
        if (tip == null) {
            tip = os.CreateObject<TipMaterial>();
            tip.Cod = cod;
            TipuriMaterialNoi++;
            avert($"Gaură de profil completată ad-hoc: TipMaterial „{cod}” "
                + $"(cont {simbol}, clasa {clasaCod}) — cerut de o linie de cheltuială 1C.");
        }
        tip.Denumire = $"1C: cont {simbol}";
        tip.Clasa = clasa;
        tip.ContImplicitId = contId;
        os.CommitChanges();
        var info = new TipInfo(tip.ID, cod, clasaCod,
            imobilizare ? NaturaClasa.Imobilizare : NaturaClasa.Serviciu, simbol);
        tipuri[cod] = info;
        return info;
    }

    // Contul pe care REGULA de contare îl pune pe latura creditoare a facturii de
    // intrare: `SursaCont.RepartitorPredator` fără cont implicit pe partener cade
    // pe fallback-ul regulii — 404 la imobilizări, 401 în rest (seed privat).
    // Handlerul are nevoie de el ca să știe ce postează Atlas, nu ca să decidă.
    public static string CreditorFacturaIntrare(NaturaClasa natura) =>
        natura == NaturaClasa.Imobilizare ? "404" : "401";

    // ======================= Contarea, citită din politici =======================
    // Handlerele au nevoie să știe CE postează Atlas, ca să transcrie doar
    // diferența. Regulile se CITESC din `RegulaContare` (politică-date, decizia
    // 4) — o re-derivare „6xx din 3xx" în unealtă ar fi a doua sursă de adevăr
    // și ar minți exact când seed-ul se schimbă.
    readonly Dictionary<Guid, (string Debit, string Credit)> contareConsum = new();
    readonly Dictionary<Guid, (string Debit, string Credit)> contareMinusInventar = new();

    public string ContPlusInventar { get; private set; }

    public (string Debit, string Credit)? ContareConsum(Guid tipMaterialId) =>
        contareConsum.TryGetValue(tipMaterialId, out var c) ? c : null;

    public (string Debit, string Credit)? ContareMinusInventar(Guid tipMaterialId) =>
        contareMinusInventar.TryGetValue(tipMaterialId, out var c) ? c : null;

    // Rândurile derivate 6xx = 3xx au toate aceeași formă (debit Explicit, credit
    // din contul Tipului); ce iese din formă nu se pretinde cunoscut — handlerul
    // tratează absența ca „Atlas nu postează nimic" și transcrie integral.
    void IncarcaContare(IObjectSpace os) {
        foreach (var r in os.GetObjectsQuery<RegulaContare>()
                     .Where(r => r.TipMaterialId != null
                         && r.SursaContDebit == SursaCont.Explicit
                         && r.SursaContCredit == SursaCont.TipMaterial)
                     .Select(r => new {
                         Tip = r.TipDocument.Cod, r.TipMaterialId, r.SemnFiltru,
                         Debit = r.ContDebit.Simbol, Credit = r.TipMaterial.ContImplicit.Simbol,
                     }).ToList()) {
            if (r.Debit == null || r.Credit == null)
                continue;
            if (r.Tip == "BCS")
                contareConsum[r.TipMaterialId.Value] = (r.Debit, r.Credit);
            else if (r.Tip == "LDI" && r.SemnFiltru == -1)
                contareMinusInventar[r.TipMaterialId.Value] = (r.Debit, r.Credit);
        }
        // Venitul din plusul de inventar: rândul generic LDI cu SemnFiltru = +1
        // (7588 la privat, 791 la bugetar — se citește, nu se presupune).
        ContPlusInventar = os.GetObjectsQuery<RegulaContare>()
            .Where(r => r.TipDocument.Cod == "LDI" && r.TipMaterialId == null && r.SemnFiltru == 1)
            .Select(r => r.ContCredit.Simbol)
            .FirstOrDefault()
            ?? throw new InvalidOperationException(
                "Profilul nu are regula de contare a plusului de inventar (LDI, SemnFiltru = +1).");
    }

    // ======================= TVA =======================

    // Cotele 1C → nomenclatorul TipTva al profilului privat. `TaxareInversa` e
    // eticheta unică a sursei pentru ambele cote istorice; regimul e ce contează
    // (4426 = 4427), iar valoarea TVA se ia CULEASĂ din sursă (36a), deci
    // eticheta de cotă nu intră în niciun calcul.
    static readonly Dictionary<string, string> CoteTva = new(StringComparer.OrdinalIgnoreCase) {
        ["TVA19"] = "N19", ["TVA21"] = "N21", ["TVA11"] = "N11", ["TVA9"] = "N9",
        ["TaxareInversa"] = "TI19", ["TaxareInversa21"] = "TI21",
        ["Neimpozabile"] = "NIM", ["ScutiteTVAFara"] = "SFD", ["ScutiteTVACu"] = "SDD",
    };

    // Cota liniei → TipTva, cu regula 36a a importului (pasul 3, ridicată aici ca
    // s-o folosească toate handlerele): un regim care POSTEAZĂ dar cu TVA zero în
    // sursă ar face motorul să recalculeze TVA-ul din cotă și să inventeze un rând
    // 4426/4427 pe care 1C nu-l are — acolo linia rămâne fără TipTva.
    public Guid? TipTvaCules(string cota1C, decimal tva) {
        var tip = TipTvaPentru(cota1C);
        if (tip == null || tva != 0m)
            return tip;
        LiniiTvaZeroFaraTip++;
        return null;
    }

    public int LiniiTvaZeroFaraTip { get; private set; }

    public Guid? TipTvaPentru(string cota1C) {
        if (string.IsNullOrEmpty(cota1C))
            return null;
        if (CoteTva.TryGetValue(cota1C, out var cod) && tipuriTva.TryGetValue(cod, out var id))
            return id;
        if (coteNecunoscute.Add(cota1C))
            avert($"Cota de TVA 1C „{cota1C}” nu are corespondent în nomenclatorul TipTva privat — "
                + "liniile ei rămân fără TipTva (TVA-ul lor nu se postează).");
        return null;
    }

    public bool EsteTaxareInversa(Guid? tipTvaId) =>
        tipTvaId != null && tipuriTva.TryGetValue("TI19", out var ti19)
            && (tipTvaId == ti19 || (tipuriTva.TryGetValue("TI21", out var ti21) && tipTvaId == ti21));

    // ======================= Identitatea 1C =======================

    public string TipRef(string numeTip1C) =>
        tipRefPeNume.TryGetValue(numeTip1C, out var t) ? t
            : throw new InvalidOperationException(
                $"Tipul 1C „{numeTip1C}” nu apare în recensământul Recorder al anului — "
                + "TypeRef-ul lui e parte din cheia de lot și nu se poate ghici.");

    // ======================= Loturi =======================

    // Convenția cheii de lot e a deschiderii (47d) și NU se schimbă: document
    // creator (TypeRef + id) × nomenclator × simbol OMFP al contului.
    public static string CheieLot(string tipRefDoc, string docId, string nomenclatorId, string simbol) =>
        $"{tipRefDoc}:{docId}:{nomenclatorId}:{simbol}";

    static string Prefix(string cheie) => cheie[..cheie.LastIndexOf(':')];

    static string Simbol(string cheie) => cheie[(cheie.LastIndexOf(':') + 1)..];

    void IndexeazaLot(string cheie, Guid id) {
        var lot = new LotImport(id, Simbol(cheie));
        loturi[cheie] = lot;
        loturiPeId[id] = lot;
        var prefix = Prefix(cheie);
        if (!loturiPePrefix.TryGetValue(prefix, out var lista))
            loturiPePrefix[prefix] = lista = [];
        lista.Add(lot);
    }

    // Lotul pin-uit de un rând 1C. Întâi cheia exactă; dacă lipsește, prefixul
    // (document × nomenclator) — cazul reclasificării, unde 1C mută lotul pe alt
    // cont fără să-i schimbe identitatea. Prefixul rezolvă doar când e
    // NEAMBIGUU: cele 46 de perechi document×produs de pe mai multe conturi
    // (fix-ul de review 1C-b) rămân nerezolvate aici și cad în supapa de
    // realocare FIFO (48a), care e exact locul lor.
    public LotImport Lot(string tipRefDoc, string docId, string nomenclatorId, string simbol) {
        var cheie = CheieLot(tipRefDoc, docId, nomenclatorId, simbol);
        if (loturi.TryGetValue(cheie, out var lot))
            return lot;
        if (loturiPePrefix.TryGetValue(Prefix(cheie), out var lista) && lista.Count == 1) {
            LoturiRezolvatePePrefix++;
            return lista[0];
        }
        LoturiNerezolvate++;
        return null;
    }

    // NOTĂ (amendament la 47d, vezi `ImportLaCerere`): aici a stat
    // `TipAlProdusului` — fallback-ul care lua Tipul liniei de STOC din produs,
    // fiindcă un produs multi-cont purta Tipul contului dominant și nu putea
    // corespunde tuturor loturilor lui. De când identitatea produsului include
    // simbolul de cont, produsul lotului are ÎNTOTDEAUNA Tipul simbolului
    // lotului: Tipul rezolvat de `MiscareStoc1C.Rezolva` E cel al produsului,
    // deci fallback-ul n-are ce corecta și ar putea doar să mintă.

    // Lotul a dispărut din bază (draftul care l-a născut s-a șters la reluare —
    // D4): indexul din memorie trebuie să-l uite pe loc, altfel un pin de mai
    // târziu l-ar rezolva și operarea ar pica pe un lot inexistent. Se curăță
    // toate intrările care trimit la el — cheia canonică ȘI aliasurile.
    public void UitaLot(Guid lotId) {
        foreach (var cheie in loturi.Where(x => x.Value.Id == lotId).Select(x => x.Key).ToList())
            loturi.Remove(cheie);
        foreach (var (prefix, lista) in loturiPePrefix.ToList()) {
            lista.RemoveAll(l => l.Id == lotId);
            if (lista.Count == 0)
                loturiPePrefix.Remove(prefix);
        }
        loturiPeId.Remove(lotId);
    }

    // Lotul născut de o linie de import (factură de intrare, plus de inventar):
    // legătura se scrie în ACELAȘI ObjectSpace cu documentul, deci în același
    // commit — ca legătura documentului (§12.4).
    public void LeagaLotNou(IObjectSpace os, string cheie, Guid lotId) {
        Legaturi.Leaga(os, "Lot", cheie, lotId);
        IndexeazaLot(cheie, lotId);
    }

    // Aliasul de reclasificare: cheia cu simbolul NOU trimite la ACELAȘI lot
    // Atlas, ca pin-urile ulterioare de sub contul nou să rezolve. Lotul își
    // păstrează simbolul canonic (deci și registrul de stoc în care îi stă
    // soldul). Aliasurile stau într-o tabelă de legături PROPRIE, ca indexul
    // canonic să rămână fără ambiguitate la reluare; se scriu într-un
    // ObjectSpace propriu (sunt nomenclator, nu parte din documentul curent).
    public bool LeagaAliasLot(string cheie, LotImport lot) {
        if (!loturi.TryAdd(cheie, lot))
            return false;
        using var os = provider.CreateObjectSpace();
        Legaturi.Leaga(os, "LotAlias", cheie, lot.Id);
        os.CommitChanges();
        return true;
    }

    // Varianta pentru loturile care se NASC odată cu documentul (returul de la
    // client, care recreează lotul-sursă absent): aliasul se scrie în
    // ObjectSpace-ul documentului, deci în ACELAȘI commit cu lotul — altfel o
    // rulare întreruptă între cele două ar lăsa un alias către un lot inexistent,
    // pe care pin-urile ulterioare l-ar rezolva și operarea l-ar refuza.
    // Simbolul rămâne cel CANONIC al lotului (apelantul îl leagă cu `LeagaLotNou`
    // înainte), nu cel din cheia aliasului: pin-urile care ajung aici trebuie să
    // cadă pe produsul geamăn al lotului, nu pe cel al contului 1C de pe rând.
    public bool LeagaAliasLot(IObjectSpace os, string cheie, Guid lotId) {
        if (!loturiPeId.TryGetValue(lotId, out var canonic))
            throw new InvalidOperationException(
                $"Alias de lot {cheie} → {lotId} cerut înaintea legăturii canonice a lotului.");
        if (!loturi.TryAdd(cheie, canonic))
            return false;
        Legaturi.Leaga(os, "LotAlias", cheie, lotId);
        return true;
    }
}
