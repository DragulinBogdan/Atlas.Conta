using Atlas.Conta.BackOffice.Module.BusinessObjects;
using DevExpress.ExpressApp;

namespace Import1C;

// PASUL 3, tipul 1: `AprovizionareMarfuriSiServiciiPrimite` → **FCT + NIR conex**
// (maparea §4). Factura poartă liniile de marfă ȘI cele de servicii; recepția o
// contează NIR-ul generat de motor (26a), TVA-ul deductibil stă pe factură.
//
// Convențiile sursei, VERIFICATE pe datele 2025 (nu presupuse):
//  * `Suma` de pe linie e NETUL, iar `SumaDocument` e brutul (17.007 din 18.927
//    de facturi cu TVA au SumaDocument = ΣSuma + ΣSumaTVA). Excepția e
//    `SumaIncludeTVA` — 4 documente pe tot anul, unde `Suma` e brutul: verificat
//    pe rândurile de notă ale unuia dintre ele (linie 170,00 cu TVA 29,50 →
//    nota 371 = 401 de 140,50), de aceea netul se scade acolo explicit.
//  * **Sumele din secțiuni sunt în VALUTA documentului**, registrul contabil e în
//    lei: 1.270 de facturi în EUR și 4 în USD pe 2025. Prima rulare a scos-o la
//    iveală singură — puntea a raportat 118 rânduri 371 = 401 de 758.978 lei,
//    adică exact diferența dintre sumele în EUR și cele în lei. Conversia e
//    `round(Suma × CursDeDecontari, 2)`, PER LINIE-SURSĂ (vezi mai jos).
//  * **Liniile n-au depozit propriu** (toate cele 34.455 au `Depozit_ID` NULL),
//    deci gestiunea lotului e cea a antetului — nicio factură multi-gestiune,
//    deci NIR-ul conex nu are cum să pice pe „lotul în gestiunea primitoare".
//  * 5 facturi pe an n-au număr de furnizor → se cade pe `Number`-ul 1C.
//  * `Count ≤ 0`: 44 de linii de marfă și 251 de servicii pe an. La servicii
//    cantitatea e pro-forma (se normalizează la |Count| sau 1, valoarea rămâne
//    a sursei — inclusiv negativă: sconturile 667 sunt linii de minus). La marfă
//    cantitatea negativă e un RETUR strecurat pe factură: nu se poate reprezenta
//    pe FCT (motorul cere cantitate pozitivă la linia care naște lot), deci
//    linia se SARE, iar rândurile ei contabile rămân în punte. Reprezentarea
//    curată e un RLF (tipul există din 1C-a) — semnalat, nu forțat aici.
static class HandlerFactura {
    public const string View = "AprovizionareMarfuriSiServiciiPrimite";

    public static readonly HandlerTip Handler =
        new(View, "Factură de intrare (+ NIR conex)", Importa);

    // Contoare de raport ale tipului.
    public static int NepostateSarite { get; private set; }
    public static int NumereLipsa { get; private set; }
    public static int LiniiCantitateNepozitiva { get; private set; }
    public static int LiniiFuzionate { get; private set; }
    public static int LiniiMarfaDejaReceptionata { get; private set; }
    public static int LiniiTvaZeroFaraTip { get; private set; }

    sealed record LinieMarfa(Guid ProdusId, TipInfo Tip, string CheieLot,
        decimal Cantitate, decimal Net, decimal Tva, Guid? TipTvaId);

    sealed record LinieServiciu(TipInfo Tip, decimal Cantitate, decimal Net, decimal Tva, Guid? TipTvaId);

    sealed class Plan {
        public Guid PartenerId;
        public Guid GestiuneId;
        public string Numar;
        public DateOnly Data;
        public DateOnly? Scadenta;
        public List<LinieMarfa> Marfuri = [];
        public List<LinieServiciu> Servicii = [];
        public Punte Punte;

        // Oglinda condiției din `Materializeaza` — de ea atârnă cheia punții.
        public bool AreDocument => Marfuri.Count > 0 || Servicii.Count > 0;
    }

    static void Importa(ContextLuna ctx) {
        var bucla = ctx.Bucla;
        var antete = bucla.Flax.Aprovizionari(ctx.An, ctx.Luna);
        var marfuri = bucla.Flax.AprovizionariMarfuri(ctx.An, ctx.Luna)
            .GroupBy(m => m.DocumentId, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.Ordinal);
        var servicii = bucla.Flax.AprovizionariServicii(ctx.An, ctx.Luna)
            .GroupBy(s => s.DocumentId, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.Ordinal);

        foreach (var h in antete)
            ctx.Planifica(h.Data, h.Numar, () => {
                // Antet POSTAT care nu POSTEAZĂ (22 pe an, verificat): `Posted` spune
                // doar că documentul e validat în 1C, nu că a produs înregistrări.
                // Factura se construiește din secțiuni, deci fără garda asta un astfel
                // de document ar intra în Atlas cu recepție și stoc — o mișcare pe care
                // sursa n-o are. Tipurile care se construiesc DIN rândurile contabile
                // (BTR/BCS/LDI−) sunt imune prin construcție.
                if ((bucla.RanduriLuna.GetValueOrDefault(h.Id)?.Count ?? 0) == 0) {
                    NepostateSarite++;
                    return;
                }
                Plan plan = null;
                if (!bucla.EsteCunoscut(View, h.Id)) {
                    try {
                        plan = Planifica(ctx, h,
                            marfuri.GetValueOrDefault(h.Id) ?? [], servicii.GetValueOrDefault(h.Id) ?? []);
                    }
                    catch (Exception ex) {
                        bucla.EsecPlanificare(View, h.Id, ex);
                        return;
                    }
                    // Puntea se scrie ÎNAINTEA facturii: dacă rularea moare între
                    // cele două, la reluare factura încă lipsește și se reface tot
                    // lanțul. Invers, un document operat fără puntea lui ar fi
                    // irecuperabil (calea de skip nu mai replanifică).
                    Punti.Scrie(bucla, View, plan.AreDocument ? h.Id + "#punte" : h.Id,
                        plan.Numar, plan.Data, plan.Punte, bucla.ContorPunti, bucla.Avert);
                    if (!plan.AreDocument && !plan.Punte.AreCeva)
                        bucla.NumaraSursaFaraCorespondent();
                }
                if (plan == null || plan.AreDocument)
                    bucla.ImportaDocument(View, h.Id, os => Materializeaza(os, bucla.Catalog, plan));
            });
    }

    static Plan Planifica(ContextLuna ctx, FlaxAprovizionare h,
            List<FlaxAprovizionareMarfa> marfuri, List<FlaxAprovizionareServiciu> servicii) {
        var bucla = ctx.Bucla;
        var cat = bucla.Catalog;
        var plan = new Plan {
            Data = DateOnly.FromDateTime(h.Data),
            Scadenta = h.DataScadenta is { } s ? DateOnly.FromDateTime(s) : null,
            Punte = new Punte(),
        };

        var numar = string.Concat(h.SeriaFactura, h.NumarFactura).Trim();
        if (numar.Length == 0) {
            numar = h.Numar;
            NumereLipsa++;
        }
        plan.Numar = numar;

        plan.GestiuneId = cat.Gestiuni.TryGetValue(h.DepozitId ?? "", out var g)
            ? g
            : throw new InvalidOperationException(
                $"Depozitul 1C {h.DepozitId} al facturii nu e legat de o Gestiune.");
        plan.PartenerId = bucla.LaCerere.AsiguraPartener(h.PartenerId)
            ?? throw new InvalidOperationException(
                $"Partenerul 1C {h.PartenerId} al facturii nu s-a putut importa.");

        // MARFA DEJA RECEPȚIONATĂ PE AVIZ (decizia lead-ului, varianta a-2 din
        // diagnosticul pasului 6). Când 1C închide un aviz de intrare, factura
        // postează `408 = 401` (+ TVA) și NU atinge niciun cont de stoc: marfa a
        // intrat în gestiune pe aviz, eventual în luna precedentă. Atlas, care
        // construiește factura din SECȚIUNEA de mărfuri, ar naște lot și NIR
        // conex — adică stoc pe care sursa nu-l are (măsurat pe 2025: 17 facturi,
        // 835.488,22 lei; contabil se ascundea în punte, stocul rămânea umflat).
        //
        // Discriminatorul e al DOCUMENTULUI, nu al liniei, și e verificat pe tot
        // anul: toate cele 17 sunt „doar 408", niciuna mixtă cu 3xx. Liniile de
        // marfă ale unei asemenea facturi devin linii NE-STOC pe Tipul contului
        // 408: factura există (rămâne țintă de imperechere pentru extras),
        // postează exact rândurile 1C prin regula generică, iar stocul nu se mișcă.
        var conturiDebit = (bucla.RanduriLuna.GetValueOrDefault(h.Id) ?? [])
            .Select(r => cat.Mapeaza(r.ContDebit)).Where(c => c != null).ToList();
        var simbolFacturiNesosite = conturiDebit.FirstOrDefault(c => c.StartsWith("408"));
        var marfaDejaReceptionata = simbolFacturiNesosite != null
            && !conturiDebit.Any(c => c.StartsWith('3'));

        // Liniile de marfă se FUZIONEAZĂ pe (nomenclator × cont): lotul 1C e
        // tripletul (document, produs, cont), deci două linii pe aceeași pereche
        // sunt un singur lot în sursă — 102 grupuri pe tot anul, TOATE cu aceeași
        // cotă de TVA (verificat), deci fuziunea nu amestecă regimuri.
        foreach (var grup in marfuri
                     .GroupBy(m => (m.NomenclatorId, Simbol: cat.Mapeaza(m.ContEvidenta)))
                     .OrderBy(x => x.Key.NomenclatorId, StringComparer.Ordinal)) {
            var linii = grup.ToList();
            if (linii.Count > 1)
                LiniiFuzionate += linii.Count - 1;
            var simbol = grup.Key.Simbol;
            if (marfaDejaReceptionata) {
                var tipNesosite = cat.TipNestocPentru(simbolFacturiNesosite)
                    ?? throw new InvalidOperationException(
                        $"Contul „{simbolFacturiNesosite}” (facturi nesosite) n-are TipMaterial "
                        + "în profil și nu s-a putut crea.");
                var tvaNesosite = linii.Sum(l => h.InLei(l.SumaTva));
                var brutNesosite = linii.Sum(l => h.InLei(l.Suma));
                plan.Servicii.Add(new LinieServiciu(tipNesosite,
                    Math.Max(1m, Math.Abs(linii.Sum(l => l.Cantitate))),
                    h.SumaIncludeTva ? brutNesosite - tvaNesosite : brutNesosite,
                    tvaNesosite, TipTvaLinie(cat, linii[0].CotaTva, tvaNesosite)));
                LiniiMarfaDejaReceptionata++;
                continue;
            }
            var tip = cat.TipStocPentru(simbol)
                ?? throw new InvalidOperationException(
                    $"Contul de stoc 1C „{linii[0].ContEvidenta}” (→ {simbol ?? "nemapat"}) "
                    + "n-are TipMaterial de stoc în profil.");
            var cantitate = linii.Sum(l => l.Cantitate);
            // Conversia în lei se face PER LINIE-SURSĂ, apoi se însumează: 1C
            // postează un rând de notă per linie, rotunjit la bani, iar o
            // conversie a sumei deja agregate ar diferi de el prin rotunjire.
            var brutSauNet = linii.Sum(l => h.InLei(l.Suma));
            var tva = linii.Sum(l => h.InLei(l.SumaTva));
            var net = h.SumaIncludeTva ? brutSauNet - tva : brutSauNet;
            var tipTva = TipTvaLinie(cat, linii[0].CotaTva, tva);

            if (cantitate <= 0) {
                // Retur strecurat pe factură (44 de linii pe an): nu se
                // reprezintă pe FCT. Rândurile 1C ale liniei rămân în punte —
                // contabilitatea se reconciliază, stocul NU se mișcă (nici în
                // Atlas, nici raportat ca mișcat).
                LiniiCantitateNepozitiva++;
                bucla.Avert($"1C:{View}/{linii[0].DocumentId}: linie de marfă cu cantitate "
                    + $"{cantitate:N3} (retur pe factură, {net:N2} lei) — sărită de pe FCT; "
                    + "rândurile ei contabile intră în puntea NTC. Reprezentarea curată e un RLF.");
                continue;
            }

            var produsId = bucla.LaCerere.AsiguraProdus(grup.Key.NomenclatorId, tip.Cod)
                ?? throw new InvalidOperationException(
                    $"Nomenclatorul 1C {grup.Key.NomenclatorId} nu s-a putut importa pe Tipul {tip.Cod}.");
            plan.Marfuri.Add(new LinieMarfa(produsId, tip,
                Catalog.CheieLot(cat.TipRef(View), h.Id, grup.Key.NomenclatorId, simbol),
                cantitate, net, tva, tipTva));
        }

        foreach (var serviciu in servicii) {
            var simbol = cat.Mapeaza(serviciu.ContCheltuieli);
            var tip = cat.TipCheltuialaPentru(simbol)
                ?? throw new InvalidOperationException(
                    $"Contul de cheltuială 1C „{serviciu.ContCheltuieli}” (→ {simbol ?? "nemapat"}) "
                    + "n-are TipMaterial în profil și nu s-a putut crea.");
            // Cantitatea serviciului e pro-forma (precedentul Decont, 32d):
            // valoarea e ce contează, iar motorul cere cantitate pozitivă.
            var cantitate = Math.Abs(serviciu.Cantitate);
            if (cantitate == 0)
                cantitate = 1m;
            var tva = h.InLei(serviciu.SumaTva);
            var net = h.InLei(serviciu.Suma) - (h.SumaIncludeTva ? tva : 0m);
            plan.Servicii.Add(new LinieServiciu(tip, cantitate, net, tva,
                TipTvaLinie(cat, serviciu.CotaTva, tva)));
        }

        ConstruiestePunte(ctx, h, plan);
        return plan;
    }

    // Cota liniei → TipTva. Un regim care POSTEAZĂ, dar cu TVA zero în sursă, ar
    // face motorul să recalculeze TVA-ul din cotă (36a păstrează doar un
    // ValoareTva NENUL) și să inventeze un rând 4426 care nu există în 1C: acolo
    // linia rămâne fără TipTva.
    static Guid? TipTvaLinie(Catalog cat, string cota1C, decimal tva) {
        var tip = cat.TipTvaPentru(cota1C);
        if (tip == null || tva != 0m)
            return tip;
        LiniiTvaZeroFaraTip++;
        return null;
    }

    // Puntea facturii = diferența dintre rândurile 1C ale documentului și ce
    // postează Atlas pe lanțul FCT + NIR. Aici diferența e SIGURĂ să fie tratată
    // integral (spre deosebire de consum): valorile Atlas vin din aceleași
    // numere ale sursei — loturile se NASC pe factură, cu prețul ei — deci orice
    // rest e pură diferență de CONT (creditorul 404/408/409, TVA-ul neexigibil
    // 4428, liniile de retur sărite), nu de evaluare.
    static void ConstruiestePunte(ContextLuna ctx, FlaxAprovizionare h, Plan plan) {
        var cat = ctx.Bucla.Catalog;
        var punte = plan.Punte.Categoria("FCT: rând 1C fără corespondent exact în Atlas");
        foreach (var r in ctx.Bucla.RanduriLuna.GetValueOrDefault(h.Id) ?? [])
            punte.Tinta1C(cat.Mapeaza(r.ContDebit), cat.Mapeaza(r.ContCredit), r.Suma);

        // Recepția liniilor de stoc se postează pe NIR-ul conex (26a), la net.
        foreach (var m in plan.Marfuri) {
            punte.ActualAtlas(m.Tip.Simbol, Catalog.CreditorFacturaIntrare(m.Tip.Natura), m.Net);
            PunteTva(cat, punte, m.TipTvaId, m.Tva, m.Tip.Natura);
        }
        foreach (var s in plan.Servicii) {
            punte.ActualAtlas(s.Tip.Simbol, Catalog.CreditorFacturaIntrare(s.Tip.Natura), s.Net);
            PunteTva(cat, punte, s.TipTvaId, s.Tva, s.Tip.Natura);
        }
    }

    // Rândul de TVA pe care îl scrie pasul TVA din motor: taxare inversă =
    // 4426 = 4427, altfel 4426 = contrapartida PoliticaTva (RepartitorPredator
    // fără cont implicit ⇒ fallback-ul 401, indiferent de natura liniei).
    static void PunteTva(Catalog cat, Punte punte, Guid? tipTvaId, decimal tva, NaturaClasa natura) {
        if (tipTvaId == null || tva == 0m)
            return;
        punte.ActualAtlas("4426", cat.EsteTaxareInversa(tipTvaId) ? "4427" : "401", tva);
    }

    static Document Materializeaza(IObjectSpace os, Catalog cat, Plan plan) {
        if (plan.Marfuri.Count == 0 && plan.Servicii.Count == 0)
            return null;
        var fct = os.CreateObject<FacturaIntrare>();
        fct.Numar = plan.Numar;
        fct.Data = plan.Data;
        fct.DataScadenta = plan.Scadenta;
        fct.PredatorId = plan.PartenerId;
        fct.PrimitorId = plan.GestiuneId;
        var gestiune = os.GetObjectByKey<Gestiune>(plan.GestiuneId);

        foreach (var m in plan.Marfuri) {
            var d = os.CreateObject<FacturaIntrareDetaliu>();
            d.Document = fct;
            d.TipMaterialId = m.Tip.Id;
            d.Cantitate = m.Cantitate;
            d.PretUnitar = m.Net / m.Cantitate;
            d.TipTvaId = m.TipTvaId;
            d.ValoareTva = m.Tva;
            // Lotul se naște pe linia FACTURII (26e) — NIR-ul conex îl preia.
            var lot = d.CreeazaLot(os, os.GetObjectByKey<Produs>(m.ProdusId), gestiune);
            cat.LeagaLotNou(os, m.CheieLot, lot.ID);
        }
        foreach (var s in plan.Servicii) {
            var d = os.CreateObject<FacturaIntrareDetaliu>();
            d.Document = fct;
            d.TipMaterialId = s.Tip.Id;
            d.Cantitate = s.Cantitate;
            d.PretUnitar = s.Net / s.Cantitate;
            d.TipTvaId = s.TipTvaId;
            d.ValoareTva = s.Tva;
        }
        return fct;
    }

    public static void Raporteaza() {
        Console.WriteLine($"  FCT: {NepostateSarite} antete Posted care NU postează în 1C (sărite), "
            + $"{NumereLipsa} facturi fără număr de furnizor (s-a folosit numărul 1C), "
            + $"{LiniiFuzionate} linii fuzionate pe lot, {LiniiCantitateNepozitiva} linii de marfă "
            + $"cu cantitate ≤ 0 sărite, {LiniiTvaZeroFaraTip} linii cu regim de TVA dar TVA zero "
            + $"(lăsate fără TipTva), {LiniiMarfaDejaReceptionata} linii de marfă deja recepționată "
            + "pe aviz (importate ca linii ne-stoc pe 408 — factura postează, stocul nu se mișcă).");
    }
}
