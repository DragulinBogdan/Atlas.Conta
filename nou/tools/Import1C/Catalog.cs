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
sealed record TipInfo(Guid Id, string Cod, string ClasaCod, NaturaClasa Natura) {
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
    public Guid SediuId { get; }
    public Guid ComisieId { get; }
    public Guid TipTrezorerieId { get; }

    readonly Dictionary<string, TipInfo> tipuri = new(StringComparer.Ordinal);
    readonly Dictionary<string, Guid> tipuriTva = new(StringComparer.Ordinal);
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
        SediuId = CereRepartitor(os, "SEDIU");
        ComisieId = CereRepartitor(os, "COMISIE");
        foreach (var t in os.GetObjectsQuery<TipMaterial>()
                     .Select(t => new { t.ID, t.Cod, ClasaCod = t.Clasa.Cod, t.Clasa.Natura }).ToList())
            tipuri[t.Cod] = new TipInfo(t.ID, t.Cod, t.ClasaCod, t.Natura);
        TipTrezorerieId = tipuri.TryGetValue("TRZ", out var trz)
            ? trz.Id
            : throw new InvalidOperationException("Profilul privat nu are Tipul tehnic TRZ (seed).");
        foreach (var t in os.GetObjectsQuery<TipTva>().Select(t => new { t.ID, t.Cod }).ToList())
            tipuriTva[t.Cod] = t.ID;

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
        // despre registrul în care stă lotul): trimit la lotul canonic, cu
        // simbolul LUI. Un alias fără canonic încărcat (lot creat de altă rulare)
        // rămâne pe simbolul din cheie — cazul degenerat, contorizat mai jos.
        var canonicePeId = loturi.Values.GroupBy(l => l.Id).ToDictionary(g => g.Key, g => g.First());
        foreach (var (cheie, id) in Legaturi.Incarca(os, "LotAlias"))
            loturi.TryAdd(cheie, canonicePeId.TryGetValue(id, out var canonic)
                ? canonic : new LotImport(id, Simbol(cheie)));
    }

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
    public TipInfo TipCheltuialaPentru(string simbol) {
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
            imobilizare ? NaturaClasa.Imobilizare : NaturaClasa.Serviciu);
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
}
