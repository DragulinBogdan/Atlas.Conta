using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Culegere;
using Atlas.Conta.BackOffice.Module.Motor;
using DevExpress.ExpressApp;

namespace Import1C;

record FlaxSoldMF(string Mf, bool Necorporal, string Cont, decimal Sold, string Cod, string Denumire);
record FlaxParametriMF(string Mf, int Termen, int TermenAmortizare, decimal ValoareCalcul, string ContAmortizare);
record FlaxPunereMF(string Id, string Numar, DateTime Data, string Mf, string Cod, string Denumire,
    string ContActiv, string ContAmortizare, int Termen, decimal Valoare);
record FlaxIesireMF(string Id, string Numar, DateTime Data, string Mf);

partial class FlaxDb {
    public List<FlaxSoldMF> SolduriMF(DateTime period) =>
        Query(@"with s as (
                    select coalesce(b.Valoare1_ImobilizariCorporale_ID, b.Valoare1_ImobilizariNecorporale_ID) mf,
                           case when b.Valoare1_ImobilizariCorporale_ID is null then 1 else 0 end necorporal,
                           ltrim(rtrim(b.Cont)) cont, b.SoldIni
                    from flax.BalantaNivel1 b
                    where b.Period = @p and b.Cont like '2%'
                      and coalesce(b.Valoare1_ImobilizariCorporale_ID, b.Valoare1_ImobilizariNecorporale_ID) is not null)
                select s.mf, s.necorporal, s.cont, sum(s.SoldIni),
                       max(coalesce(c.Code, n.Code)), max(coalesce(c.Description, n.Description))
                from s
                left join flax.ImobilizariCorporale c on c.KeyField = s.mf
                left join flax.ImobilizariNecorporale n on n.KeyField = s.mf
                group by s.mf, s.necorporal, s.cont
                having sum(s.SoldIni) <> 0",
            r => new FlaxSoldMF(Hex(r, 0), Int(r, 1) == 1, Text(r, 2), Dec(r, 3), Text(r, 4), Text(r, 5)),
            ("@p", period));

    public Dictionary<string, FlaxParametriMF> ParametriMF(DateTime inainte) =>
        Query(@"select p.MijlocFix_ID, p.TermenDeUtilizare, p.TermenDeUtilizarePentruAmortizare,
                       p.ValoareaPentruCalculAmortizarii,
                       (select top 1 ltrim(rtrim(h.ContAmortizare))
                        from flax.PunereaInEvidentaMF h
                        join flax.PunereaInEvidentaMF_ImobilizariCorporale l on l.ParentRef = h.KeyField
                        where l.ImobilizariCorporale_ID = p.MijlocFix_ID and h.Posted <> 0x00
                        order by h.DateTime desc)
                from (select *, row_number() over (partition by MijlocFix_ID order by Period desc) rn
                      from flax.InfoRg_ParametriiDeAmortizareMF where Period < @p) p
                where p.rn = 1
                union all
                select n.ImobilizareNecorporala_ID, n.TermenDeUtilizare, n.TermenDeUtilizare, n.ValoareaInitiala, null
                from (select *, row_number() over (partition by ImobilizareNecorporala_ID order by Period desc) rn
                      from flax.InfoRg_InformatiiInitialeImobilizariNecorporale
                      where Period < @p and SupunereaAmortizarii <> 0x00) n
                where n.rn = 1",
            r => new FlaxParametriMF(Hex(r, 0), Int(r, 1), Int(r, 2), Dec(r, 3), Text(r, 4)),
            ("@p", inainte))
        .GroupBy(p => p.Mf).ToDictionary(g => g.Key, g => g.First(), StringComparer.Ordinal);

    public List<FlaxPunereMF> PuneriInEvidentaMF(int an, int luna) =>
        Query($@"select h.KeyField, h.Number, h.DateTime, l.ImobilizariCorporale_ID, m.Code, m.Description,
                        ltrim(rtrim(h.ContDeEvidenta)), ltrim(rtrim(h.ContAmortizare)), h.TermenDeUtilizare,
                        (select sum(n.Suma)
                         from flax.NoteContabile n
                         join flax.DefalcareNote d on d.Recorder_Id = n.DocReferinta_Id and d.[LineNo] = n.[LineNo]
                         where d.Correspond = 0 and d.KindRef = N'Imobilizări corporale'
                           and d.Value_Id = l.ImobilizariCorporale_ID
                           and n.ContDebit like '2%' and n.ContDebit not like '28%' and n.Period <= h.DateTime)
                 from flax.PunereaInEvidentaMF h
                 join flax.PunereaInEvidentaMF_ImobilizariCorporale l on l.ParentRef = h.KeyField
                 left join flax.ImobilizariCorporale m on m.KeyField = l.ImobilizariCorporale_ID
                 {FiltruLuna} order by h.DateTime, h.Number, h.KeyField, l.[LineNo]",
            r => new FlaxPunereMF(Hex(r, 0), Text(r, 1), Data(r, 2), Hex(r, 3), Text(r, 4), Text(r, 5),
                Text(r, 6), Text(r, 7), Int(r, 8), Dec(r, 9)),
            Fereastra(an, luna));

    public List<FlaxIesireMF> IesiriMF(int an, int luna) =>
        Query($@"select h.KeyField, h.Number, h.DateTime, s.Nomenclator_ImobilizariCorporale_ID
                 from flax.VanzareMarfuriSiServiciiPrestate h
                 join flax.VanzareMarfuriSiServiciiPrestate_Imobilizari s on s.ParentRef = h.KeyField
                 {FiltruLuna} and s.Nomenclator_ImobilizariCorporale_ID is not null
                 {OrdineAntete}",
            r => new FlaxIesireMF(Hex(r, 0), Text(r, 1), Data(r, 2), Hex(r, 3)),
            Fereastra(an, luna));
}

// E4-D3: fișele 1C intră prin PIF-ul de deschidere; amortizarea lunii o generează motorul (AMO),
// iar ieșirile sunt CAS pe fișă. Diferențele față de rândurile 1C sunt divergențe cu nume.
static class Imobilizari1C {
    public const string ViewFisa = "MijlocFix";
    public const string ViewDeschidere = "IntroducereSolduriInitialeMF";
    public const string ViewPunere = "PunereaInEvidentaMF";
    public const string ViewIesire = "IesireMF";
    public const string ViewAmortizare = "AmortizareLunara";
    const decimal Eps = 0.005m;
    static readonly string[] FeluriMF = ["Imobilizări corporale", "Imobilizări necorporale"];

    public const string CategorieCota = "amortizarea lunii: cota Atlas pe restul de la PIF față de cota 1C (E4-D3)";
    public const string CategorieIesire = "ieșirea fișei: cumulul Atlas față de rândurile 1C (E4-D3)";

    public sealed record FisaPlan(string Mf, bool Necorporal, string Cod, string Denumire, string SimbolActiv,
        string SimbolAmortizare, decimal Valoare, decimal Amortizare, int Durata, int LuniAmortizate);
    public sealed record FaraFisa(string Mf, string Simbol, decimal Sold, string Motiv);
    public sealed record Recensamant(IReadOnlyList<FisaPlan> Fise, IReadOnlyList<FaraFisa> FaraFisa);

    static Dictionary<string, Guid> fise;
    static readonly Dictionary<string, List<string>> mfPeFactura = new(StringComparer.Ordinal);

    public static int FiseDeschise { get; private set; }
    public static int Puneri { get; private set; }
    public static int Iesiri { get; private set; }
    public static int Amortizari { get; private set; }
    public static int RanduriAmortizareSarite { get; private set; }
    public static decimal DivergentaCota { get; private set; }
    public static decimal DivergentaIesire { get; private set; }

    public static IEnumerable<HandlerTip> HandlereIntrare => [
        new(ViewDeschidere, "Fișele de imobilizări la graniță (PIF de deschidere)", Deschide),
        new(ViewPunere, "Punerea în evidență a unei imobilizări (PIF)", ImportaPuneri),
    ];

    public static HandlerTip HandlerIesire => new(ViewIesire, "Ieșirea unei imobilizări vândute (CAS)", ImportaIesiri);

    static Dictionary<string, Guid> Fise(BuclaImport bucla) {
        if (fise == null) {
            using var os = bucla.CreeazaObjectSpace();
            fise = Legaturi.Incarca(os, ViewFisa);
        }
        return fise;
    }

    public static bool AreFisa(BuclaImport bucla, string mf) => mf != null && Fise(bucla).ContainsKey(mf);

    // ======================= Recensământul la graniță =======================

    public static Recensamant LaGranita { get; private set; }

    public static Recensamant Recenzeaza(FlaxDb flax, Func<string, string> mapeaza, DateOnly granita) {
        var perioada = granita.ToDateTime(TimeOnly.MinValue);
        var parametri = flax.ParametriMF(perioada);
        var fisePlan = new List<FisaPlan>();
        var faraFisa = new List<FaraFisa>();
        foreach (var g in flax.SolduriMF(perioada).GroupBy(s => s.Mf)) {
            var randuri = g.Select(s => (s, Simbol: mapeaza(s.Cont))).ToList();
            var active = randuri.Where(r => r.Simbol != null && EsteActiv(r.Simbol)).ToList();
            var amortizari = randuri.Where(r => r.Simbol != null && EsteAmortizare(r.Simbol)).ToList();
            var altele = randuri.Except(active).Except(amortizari).ToList();
            void Afara(IEnumerable<(FlaxSoldMF s, string Simbol)> rr, string motiv) =>
                faraFisa.AddRange(rr.Select(r => new FaraFisa(g.Key, r.Simbol ?? r.s.Cont, r.s.Sold, motiv)));
            Afara(altele, "MF pe cont de imobilizări în curs sau financiare");
            var prima = g.First();
            if (active.Count != 1 || active[0].s.Sold <= 0m) {
                Afara(active.Concat(amortizari), active.Count == 0
                    ? "MF fără valoare pe cont de imobilizare (doar amortizare)"
                    : "MF cu valoare pe mai multe conturi de activ sau negativă");
                continue;
            }
            if (!parametri.TryGetValue(g.Key, out var p) || p.TermenAmortizare <= 0) {
                Afara(active.Concat(amortizari), "MF fără parametri de amortizare în 1C (teren, neamortizat)");
                continue;
            }
            var (activ, simbolActiv) = active[0];
            var amortizare = -amortizari.Sum(r => r.s.Sold);
            var simbolAm = amortizari.OrderByDescending(r => Math.Abs(r.s.Sold)).Select(r => r.Simbol).FirstOrDefault()
                ?? mapeaza(p.ContAmortizare);
            if (amortizari.Select(r => r.Simbol).Distinct().Count() > 1) {
                Afara(amortizari.Where(r => r.Simbol != simbolAm), "amortizarea MF pe un al doilea cont");
                amortizare = -amortizari.Where(r => r.Simbol == simbolAm).Sum(r => r.s.Sold);
            }
            if (amortizare > activ.Sold) {
                faraFisa.Add(new FaraFisa(g.Key, simbolAm, -(amortizare - activ.Sold), "amortizare peste valoare în 1C"));
                amortizare = activ.Sold;
            }
            if (amortizare < 0m) {
                faraFisa.Add(new FaraFisa(g.Key, simbolAm, -amortizare, "amortizare cu sold debitor în 1C"));
                amortizare = 0m;
            }
            var (durata, luni) = Durata(activ.Sold - amortizare, p);
            fisePlan.Add(new FisaPlan(g.Key, prima.Necorporal, prima.Cod, prima.Denumire, simbolActiv, simbolAm,
                activ.Sold, amortizare, durata, luni));
        }
        return LaGranita = new Recensamant(fisePlan, faraFisa);
    }

    static bool EsteActiv(string simbol) => simbol.StartsWith("20") || simbol.StartsWith("21");
    static bool EsteAmortizare(string simbol) => simbol.StartsWith("28");

    // Soldul fără subconto MF cu semnul opus naturii contului (debit pe 28x, credit pe 20x/21x) n-are
    // fișă care să-l poarte: deschiderea scrie pe cont fișele + soldurile fără fișă, iar diferența
    // față de Balanță e Δ declarat la deschidere (E4-D3).
    public static Dictionary<string, decimal> Ajustari(Recensamant r, IReadOnlyDictionary<string, decimal> net) {
        var ajustari = new Dictionary<string, decimal>(StringComparer.Ordinal);
        foreach (var (simbol, fise, fara, sold) in Linii(r, net)) {
            var rest = sold - fise - fara;
            if (EsteAmortizare(simbol) ? rest > Eps : EsteActiv(simbol) && rest < -Eps)
                ajustari[simbol] = -rest;
        }
        return ajustari;
    }

    static IEnumerable<(string Simbol, decimal Fise, decimal Fara, decimal Sold)> Linii(Recensamant r,
            IReadOnlyDictionary<string, decimal> net) {
        var peFise = r.Fise.GroupBy(f => f.SimbolActiv).ToDictionary(g => g.Key, g => g.Sum(f => f.Valoare), StringComparer.Ordinal);
        foreach (var g in r.Fise.GroupBy(f => f.SimbolAmortizare))
            peFise[g.Key] = peFise.GetValueOrDefault(g.Key) - g.Sum(f => f.Amortizare);
        var solduri = net.Where(x => EsteActiv(x.Key) || EsteAmortizare(x.Key)).Select(x => x.Key);
        foreach (var simbol in solduri.Union(peFise.Keys).Union(r.FaraFisa.Select(f => f.Simbol)).OrderBy(s => s, StringComparer.Ordinal))
            yield return (simbol, peFise.GetValueOrDefault(simbol), r.FaraFisa.Where(f => f.Simbol == simbol).Sum(f => f.Sold),
                net.GetValueOrDefault(simbol));
    }

    public static void Raporteaza(Recensamant r, IReadOnlyDictionary<string, decimal> net, DateOnly granita) {
        Console.WriteLine($"Imobilizări la {granita:yyyy-MM-dd}: {r.Fise.Count} fișe (valoare Σ {r.Fise.Sum(f => f.Valoare):N2}, "
            + $"amortizare Σ {r.Fise.Sum(f => f.Amortizare):N2}, {r.Fise.Count(f => f.LuniAmortizate == f.Durata)} amortizate integral); "
            + $"rămân fără fișă {r.FaraFisa.Count} solduri de MF:");
        foreach (var g in r.FaraFisa.GroupBy(f => f.Motiv).OrderBy(g => g.Key, StringComparer.Ordinal))
            Console.WriteLine($"  {g.Count(),4} × {g.Key}: "
                + string.Join(", ", g.GroupBy(f => f.Simbol).OrderBy(x => x.Key, StringComparer.Ordinal)
                    .Select(x => $"{x.Key} {x.Sum(f => f.Sold):N2}")));
        foreach (var (simbol, fise, fara, sold) in Linii(r, net))
            Console.WriteLine($"  {simbol}: Balanța {sold:N2} = fișe {fise:N2} + fără fișă {fara:N2} + fără subconto MF {sold - fise - fara:N2}");
    }

    // Lunile rămase = restul în cote 1C rotunjit la cel mai apropiat întreg: restul 1C e k cote plus
    // reziduul rotunjirilor, iar o lună în plus pentru reziduu ar coborî cota Atlas pe toată durata.
    static (int Durata, int Luni) Durata(decimal rest, FlaxParametriMF p) {
        var cota = Math.Round(p.ValoareCalcul / p.TermenAmortizare, 2);
        var ramase = rest < Eps ? 0 : cota <= 0m ? p.TermenAmortizare : Math.Max(1, (int)Math.Round(rest / cota, MidpointRounding.AwayFromZero));
        var durata = Math.Max(Math.Max(p.Termen, p.TermenAmortizare), ramase);
        return (durata, durata - ramase);
    }

    static void Deschide(ContextLuna ctx) {
        if (ctx.Luna != 1)
            return;
        var bucla = ctx.Bucla;
        var granita = new DateOnly(ctx.An, 1, 1);
        ctx.Planifica(granita.ToDateTime(TimeOnly.MinValue), "", () => {
            var r = LaGranita ?? throw new InvalidOperationException("Recensământul imobilizărilor n-a rulat la deschidere.");
            var noi = new Dictionary<string, Guid>(StringComparer.Ordinal);
            var stare = bucla.ImportaDocument(ViewDeschidere, $"{granita:yyyy-MM}", os => {
                var cat = bucla.Catalog;
                var pif = os.CreateObject<PunereInFunctiune>();
                pif.Data = granita.AddDays(-1);
                pif.DataInregistrare = granita;
                pif.Numar = $"MF-{granita:yyyy-MM}";
                pif.PredatorId = cat.SediuId;
                pif.PrimitorId = cat.SediuId;
                foreach (var f in r.Fise) {
                    var tip = TipFisa(bucla, f.SimbolActiv, f.SimbolAmortizare);
                    var fisa = Fisa(os, bucla, f.Mf, f.Necorporal, f.Cod, f.Denumire, tip, noi);
                    Linie(os, pif, fisa, tip, f.Valoare, f.Amortizare, f.Durata, f.LuniAmortizate);
                }
                return r.Fise.Count == 0 ? null : pif;
            }, motivFaraDraft: "granița n-are fișe de imobilizări");
            if (stare == StareImport.Importat) {
                foreach (var (mf, id) in noi)
                    fise[mf] = id;
                FiseDeschise += noi.Count;
            }
        });
    }

    // ======================= Tipul și fișa =======================

    // Tipul fișei poartă politica perechii (activ, amortizare) a 1C; geamănul apare doar când
    // politica tipului de bază numește alt cont de amortizare.
    static readonly Dictionary<(string, string), Guid> tipuri = [];

    static Guid TipFisa(BuclaImport bucla, string simbolActiv, string simbolAm) {
        if (tipuri.TryGetValue((simbolActiv, simbolAm), out var id))
            return id;
        var cat = bucla.Catalog;
        var baza = cat.TipNestocPentru(simbolActiv)
            ?? throw new InvalidOperationException($"Contul de imobilizare {simbolActiv} n-are tip în profil.");
        using var os = bucla.CreeazaObjectSpace();
        var politica = os.FirstOrDefault<PoliticaAmortizare>(p => p.TipMaterialId == baza.Id);
        var contAm = simbolAm != null && cat.Plan.TryGetValue(simbolAm, out var c) ? c : politica?.ContAmortizareId;
        if (politica != null && politica.ContAmortizareId == contAm)
            return tipuri[(simbolActiv, simbolAm)] = baza.Id;
        var cod = $"{simbolActiv}/{simbolAm}";
        var tip = os.FirstOrDefault<TipMaterial>(t => t.Cod == cod);
        if (tip == null) {
            var tipBaza = os.GetObjectByKey<TipMaterial>(baza.Id);
            tip = os.CreateObject<TipMaterial>();
            tip.Cod = cod;
            tip.Denumire = $"1C: imobilizare pe {simbolActiv}, amortizată pe {simbolAm}";
            tip.ClasaId = tipBaza.ClasaId;
            tip.ContImplicitId = tipBaza.ContImplicitId;
            bucla.Avert($"Tip de imobilizare „{cod}” creat: perechea 1C (activ, amortizare) diferă de politica tipului {simbolActiv}.");
        }
        if (!os.GetObjectsQuery<PoliticaAmortizare>().Any(p => p.TipMaterialId == tip.ID)) {
            var p = os.CreateObject<PoliticaAmortizare>();
            p.TipMaterialId = tip.ID;
            p.ContAmortizareId = contAm;
            p.ContCheltuialaAmortizareId = politica?.ContCheltuialaAmortizareId ?? cat.Plan["6811"];
            p.ContCheltuialaCedareId = politica?.ContCheltuialaCedareId ?? cat.Plan["6583"];
        }
        os.CommitChanges();
        return tipuri[(simbolActiv, simbolAm)] = tip.ID;
    }

    static Imobilizare Fisa(IObjectSpace os, BuclaImport bucla, string mf, bool necorporal, string cod,
            string denumire, Guid tip, Dictionary<string, Guid> noi) {
        if (Fise(bucla).TryGetValue(mf, out var id) && os.GetObjectByKey<Imobilizare>(id) is { } legata) {
            legata.TipMaterialId = tip;
            return legata;
        }
        var numar = necorporal ? $"N-{cod}" : cod;
        var fisa = os.FirstOrDefault<Imobilizare>(f => f.NumarInventar == numar) ?? os.CreateObject<Imobilizare>();
        fisa.NumarInventar = numar;
        fisa.Denumire = string.IsNullOrWhiteSpace(denumire) ? numar : denumire;
        fisa.TipMaterialId = tip;
        fisa.LocId = bucla.Catalog.SediuId;
        Legaturi.Leaga(os, ViewFisa, mf, fisa.ID);
        noi[mf] = fisa.ID;
        return fisa;
    }

    static void Linie(IObjectSpace os, PunereInFunctiune pif, Imobilizare fisa, Guid tip, decimal valoare,
            decimal amortizare, int durata, int luni) {
        var l = os.CreateObject<PunereInFunctiuneDetaliu>();
        l.Document = pif;
        l.ImobilizareId = fisa.ID;
        l.TipMaterialId = tip;
        l.Fel = FelLiniePif.Intrare;
        l.Cantitate = 1m;
        l.Valoare = valoare;
        l.ValoareFiscala = valoare;
        l.AmortizareInitiala = amortizare;
        l.AmortizareFiscalaInitiala = amortizare;
        l.LuniAmortizateInitial = luni;
        l.Metoda = MetodaAmortizare.Liniara;
        l.DurataLuni = durata;
        l.MetodaFiscala = MetodaAmortizare.Liniara;
        l.DurataFiscalaLuni = durata;
        l.CategorieFiscala = CategorieFiscala.Standard;
        l.UtilizareExclusiva = true;
    }

    // ======================= Punerea în evidență din an =======================

    // Achiziția a intrat pe contul de imobilizare fără fișă (puntea facturii); PIF-ul fără linie
    // sursă o nominalizează pe fișă la data punerii 1C (SC-IMO-04).
    static void ImportaPuneri(ContextLuna ctx) {
        var bucla = ctx.Bucla;
        foreach (var p in bucla.Flax.PuneriInEvidentaMF(ctx.An, ctx.Luna))
            ctx.Planifica(p.Data, p.Numar, () => {
                if (AreFisa(bucla, p.Mf) && !bucla.EsteCunoscut(ViewPunere, p.Id)) {
                    bucla.RefuzaCuMotiv(ViewPunere, "MF-ul are deja fișă (deschidere sau punere anterioară)");
                    return;
                }
                var simbolActiv = bucla.Catalog.Mapeaza(p.ContActiv);
                var simbolAm = bucla.Catalog.Mapeaza(p.ContAmortizare);
                var noi = new Dictionary<string, Guid>(StringComparer.Ordinal);
                var stare = bucla.ImportaDocument(ViewPunere, p.Id, os => {
                    if (p.Valoare <= 0m)
                        return null;
                    var tip = TipFisa(bucla, simbolActiv, simbolAm);
                    var fisa = Fisa(os, bucla, p.Mf, false, p.Cod, p.Denumire, tip, noi);
                    var pif = os.CreateObject<PunereInFunctiune>();
                    pif.Data = DateOnly.FromDateTime(p.Data);
                    pif.DataInregistrare = pif.Data;
                    pif.Numar = p.Numar;
                    pif.PredatorId = bucla.Catalog.SediuId;
                    pif.PrimitorId = bucla.Catalog.SediuId;
                    Linie(os, pif, fisa, tip, p.Valoare, 0m, p.Termen, 0);
                    return pif;
                }, motivFaraDraft: "MF fără valoare de intrare pe contul de imobilizare până la punere");
                if (stare == StareImport.Importat) {
                    foreach (var (mf, id) in noi)
                        fise[mf] = id;
                    Puneri++;
                }
            });
    }

    // ======================= Ieșirea (vânzarea) =======================

    public static bool FacturaCuIesire(BuclaImport bucla, string fcl) =>
        mfPeFactura.TryGetValue(fcl, out var mf) && mf.Any(m => AreFisa(bucla, m));

    static void ImportaIesiri(ContextLuna ctx) {
        var bucla = ctx.Bucla;
        foreach (var g in bucla.Flax.IesiriMF(ctx.An, ctx.Luna).GroupBy(i => i.Id)) {
            var h = g.First();
            mfPeFactura[h.Id] = g.Select(i => i.Mf).Distinct().ToList();
            ctx.Planifica(h.Data, h.Numar, () => {
                var mf = mfPeFactura[h.Id].Where(m => AreFisa(bucla, m)).ToList();
                if (mf.Count == 0) {
                    bucla.RefuzaCuMotiv(ViewIesire, "MF vândut fără fișă în Atlas (rândurile rămân pe puntea facturii)");
                    return;
                }
                if (bucla.ImportaDocument(ViewIesire, h.Id, os => Cas(os, bucla, h, mf)) == StareImport.Importat)
                    Iesiri++;
            });
        }
    }

    static IesireImobilizare Cas(IObjectSpace os, BuclaImport bucla, FlaxIesireMF h, List<string> mf) {
        var cas = os.CreateObject<IesireImobilizare>();
        cas.Data = DateOnly.FromDateTime(h.Data);
        cas.DataInregistrare = cas.Data;
        cas.Numar = h.Numar;
        cas.Cauza = CauzaIesire.Vanzare;
        cas.PredatorId = bucla.Catalog.SediuId;
        cas.PrimitorId = bucla.Catalog.SediuId;
        NormalizariTip.RegenereazaCas(os, cas, mf.Select(m => fise[m]).ToList());
        var atlas = cas.Detalii.OfType<IesireImobilizareDetaliu>()
            .Select(l => (Debit: bucla.Catalog.Simbol(l.ContDebitId), Credit: bucla.Catalog.Simbol(l.ContCreditId), l.Valoare));
        var sursa = (bucla.RanduriLuna.GetValueOrDefault(h.Id) ?? [])
            .Select(r => (Debit: bucla.Catalog.Mapeaza(r.ContDebit), Credit: bucla.Catalog.Mapeaza(r.ContCredit), Valoare: r.Suma))
            .Where(r => EsteRandIesire(r.Debit, r.Credit));
        DivergentaIesire += Declara(bucla, RegistruDivergente.Sursa(ViewIesire, h.Id), CategorieIesire, sursa, atlas);
        return cas;
    }

    public static bool EsteRandIesire(string debit, string credit) =>
        debit != null && credit != null && (credit.StartsWith("20") || credit.StartsWith("21"))
        && (debit.StartsWith("28") || debit.StartsWith("658"));

    // ======================= Amortizarea lunii =======================

    // Rândul 68x = 28x al închiderii 1C pe un MF cu fișă îl postează AMO-ul lunii.
    public static IReadOnlyList<FlaxRandNota> FaraAmortizareaFiselor(BuclaImport bucla, string docId,
            IReadOnlyList<FlaxRandNota> randuri) {
        var index = Subconto.Indexeaza(bucla.SubcontoLuna.GetValueOrDefault(docId) ?? []);
        var ramase = randuri.Where(r => !EsteAmortizareFisa(bucla, index, r)).ToList();
        RanduriAmortizareSarite += randuri.Count - ramase.Count;
        return ramase;
    }

    static bool EsteAmortizareFisa(BuclaImport bucla, Dictionary<(int, int), Dictionary<string, FlaxRef>> index,
            FlaxRandNota r) {
        var debit = bucla.Catalog.Mapeaza(r.ContDebit);
        var credit = bucla.Catalog.Mapeaza(r.ContCredit);
        if (debit == null || credit == null || !debit.StartsWith("68") || !credit.StartsWith("28"))
            return false;
        return FeluriMF.Any(fel => AreFisa(bucla, index.Ia(r.Linie, Subconto.Credit, fel)?.Id));
    }

    public static AmortizareLunara Genereaza(IObjectSpace os, BuclaImport bucla, ContextLuna ctx) {
        var amo = AmortizareService.Incearca(os, ctx.An, ctx.Luna, bucla.Catalog.SediuId).Document;
        var atlas = amo == null ? []
            : amo.Detalii.OfType<AmortizareLunaraDetaliu>().Where(l => l.Valoare != 0m)
                .Select(l => (Debit: bucla.Catalog.Simbol(l.ContDebitId), Credit: bucla.Catalog.Simbol(l.ContCreditId), l.Valoare));
        var sursa = new List<(string Debit, string Credit, decimal Valoare)>();
        foreach (var h in bucla.Flax.InchideriLuna(ctx.An, ctx.Luna)) {
            var index = Subconto.Indexeaza(bucla.SubcontoLuna.GetValueOrDefault(h.Id) ?? []);
            sursa.AddRange((bucla.RanduriLuna.GetValueOrDefault(h.Id) ?? [])
                .Where(r => EsteAmortizareFisa(bucla, index, r))
                .Select(r => (bucla.Catalog.Mapeaza(r.ContDebit), bucla.Catalog.Mapeaza(r.ContCredit), r.Suma)));
        }
        DivergentaCota += Declara(bucla, RegistruDivergente.Sursa(ViewAmortizare, $"{ctx.An:0000}-{ctx.Luna:00}"),
            CategorieCota, sursa, atlas);
        if (amo != null)
            Amortizari++;
        return amo;
    }

    // ValoareNepostata = 1C − Atlas pe pereche (convenția registrului).
    static decimal Declara(BuclaImport bucla, string sursa, string categorie,
            IEnumerable<(string Debit, string Credit, decimal Valoare)> din1C,
            IEnumerable<(string Debit, string Credit, decimal Valoare)> dinAtlas) {
        var delta = new Dictionary<(string, string), decimal>();
        foreach (var r in din1C)
            delta[(r.Debit, r.Credit)] = delta.GetValueOrDefault((r.Debit, r.Credit)) + r.Valoare;
        foreach (var r in dinAtlas)
            delta[(r.Debit, r.Credit)] = delta.GetValueOrDefault((r.Debit, r.Credit)) - r.Valoare;
        var total = 0m;
        foreach (var ((debit, credit), v) in delta.Where(x => Math.Abs(x.Value) >= Eps)) {
            bucla.Divergenta(sursa, categorie, contDebit: debit, contCredit: credit, valoareNepostata: v);
            total += Math.Abs(v);
        }
        return total;
    }

    public static void Raporteaza() {
        if (FiseDeschise + Puneri + Iesiri + Amortizari == 0)
            return;
        Console.WriteLine($"  Imobilizări: {FiseDeschise} fișe deschise la graniță, {Puneri} puneri în evidență, "
            + $"{Iesiri} ieșiri (CAS), {Amortizari} amortizări lunare; {RanduriAmortizareSarite} rânduri 68x = 28x "
            + $"ale închiderii 1C lăsate AMO-ului; divergențe |Σ| cotă {DivergentaCota:N2}, ieșire {DivergentaIesire:N2}.");
    }
}
