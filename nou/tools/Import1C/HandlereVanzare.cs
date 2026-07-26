using Atlas.Conta.BackOffice.Module.BusinessObjects;
using DevExpress.ExpressApp;

namespace Import1C;

// PASUL 4, tipurile 1–4: ieșirile de marfă — factura de ieșire, raportul de
// vânzări cu amănuntul și avizele. Mecanismele comune (liniile de venit,
// descărcarea din rândurile de cost, clasificarea rândurilor) stau în Vanzare1C.cs.
//
// Un document 1C de vânzare produce ÎN ATLAS mai multe documente: factura (venit
// + TVA), descărcarea de gestiune (cost + stoc, cu loturile sursei) și, când
// banii intră pe loc, încasarea. Fiecare are cheia lui de idempotență — sufixe pe
// cheia sursei — iar gardul de replanificare al reluării se calculează DIN SURSĂ,
// fără planificare: altfel a doua rulare ar re-aloca loturi pentru documente pe
// care le-a importat deja (§12.4).

// ======================= 1. VanzareMarfuriSiServiciiPrestate → FCL + DSC =======================
static class HandlerVanzare {
    public const string View = "VanzareMarfuriSiServiciiPrestate";

    public static readonly HandlerTip Handler =
        new(View, "Factură de ieșire (+ descărcare de gestiune)", Importa);

    public static int NepostateSarite { get; private set; }
    public static int LiniiVenit { get; private set; }
    public static int NumereLipsa { get; private set; }
    public static int IncasariCard { get; private set; }
    public static int FaraVenit { get; private set; }
    public static int TranscriseIntegral { get; private set; }

    sealed class Plan {
        public Guid PartenerId;
        public DateOnly Data;
        public string Numar;
        public DateOnly? Scadenta;
        public List<LinieVenit> Venituri = [];
        public List<Descarcare1C.Grup> Descarcari = [];
        public decimal SumaCard;
        public Punte Punte;

        public bool AreDocument => Venituri.Count > 0;
    }

    static void Importa(ContextLuna ctx) {
        var bucla = ctx.Bucla;
        var cat = bucla.Catalog;
        var marfuri = bucla.Flax.VanzariMarfuri(ctx.An, ctx.Luna)
            .GroupBy(m => m.DocumentId, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.Ordinal);
        var servicii = bucla.Flax.VanzariServicii(ctx.An, ctx.Luna)
            .GroupBy(s => s.DocumentId, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.Ordinal);

        foreach (var h in bucla.Flax.Vanzari(ctx.An, ctx.Luna))
            ctx.Planifica(h.Data, h.Numar, () => {
                var randuri = bucla.RanduriLuna.GetValueOrDefault(h.Id) ?? [];
                // Antet POSTAT care nu POSTEAZĂ (garda facturii de intrare, pasul 3):
                // fără rânduri contabile n-are ce muta nici în stoc, nici în conturi —
                // l-am inventa noi din secțiuni.
                if (randuri.Count == 0) {
                    NepostateSarite++;
                    return;
                }
                var index = Subconto.Indexeaza(bucla.SubcontoLuna.GetValueOrDefault(h.Id) ?? []);

                // Cheile pe care documentul TREBUIE să le producă, deduse din sursă
                // fără planificare (gardul reluării).
                var areVenit = marfuri.ContainsKey(h.Id) || servicii.ContainsKey(h.Id);
                var depozite = Descarcare1C.Depozite(cat, randuri, index, h.DepozitId);
                // Cardul intră în gard doar cu sumă POZITIVĂ: `Trezorerie1C.Incasare`
                // refuză restul (o încasare negativă n-are reprezentare — 31a), deci
                // o cheie așteptată pentru un document care nu se poate naște ar fi
                // un skip mut la fiecare rulare (F4).
                var areCard = randuri.Where(r => EsteIncasareCard(cat, r)).Sum(r => r.Suma) > 0;
                // Se citește ÎNAINTE de planificare: `Punti.Scrie` leagă cheia punții
                // în aceeași trecere, deci după ea n-am mai putea distinge „puntea e a
                // rulării de acum" de „puntea e a unei rulări anterioare".
                var punteVeche = bucla.EsteCunoscut(View, h.Id + "#punte");
                // Gardul NU poate fi vacuu (F1a): un document Posted care nu produce
                // NICIO cheie de plan (factura de imobilizări, cu liniile în secțiuni
                // pe care nu le citim; storno de comision fără secțiuni) trecea prin
                // conjuncția asta ca „deja cunoscut" și dispărea complet — nici
                // document, nici punte, nici contor. Când nu există chei de plan,
                // cheia sursei e cea a PUNȚII (rețeta avizului de ieșire).
                var chei = new List<string>();
                if (areVenit)
                    chei.Add(h.Id);
                chei.AddRange(depozite.Select(d => Descarcare1C.Cheie(h.Id, d)));
                if (areCard)
                    chei.Add(h.Id + "#card");
                var cunoscut = chei.Count > 0
                    ? chei.All(c => bucla.EsteCunoscut(View, c))
                    : bucla.EsteCunoscut(View, h.Id);

                Plan plan = null;
                if (!cunoscut) {
                    try {
                        plan = Planifica(ctx, h, randuri, index,
                            marfuri.GetValueOrDefault(h.Id) ?? [], servicii.GetValueOrDefault(h.Id) ?? []);
                    }
                    catch (Exception ex) {
                        bucla.EsecPlanificare(View, h.Id, ex);
                        return;
                    }
                    Punti.Scrie(bucla, View, chei.Count > 0 ? h.Id + "#punte" : h.Id,
                        plan.Numar, plan.Data, plan.Punte, bucla.ContorPunti, bucla.Avert);
                    if (chei.Count == 0) {
                        TranscriseIntegral++;
                        if (!plan.Punte.AreCeva)
                            bucla.NumaraSursaFaraCorespondent();
                    }
                }

                // Fabricile de mai jos primesc `plan == null` pe două căi: documentul
                // e deja importat (cheia e cunoscută, deci nici nu se apelează) sau
                // legătura e ORFANĂ și `ImportaDocument` cere draftul din nou — atunci
                // nu-l putem construi fără plan, deci întoarcem null (se raportează ca
                // sărit, iar rularea următoare replanifică pe legătura ștearsă).
                if (areVenit)
                    bucla.ImportaDocument(View, h.Id, os => Materializeaza(os, cat, plan),
                        motivFaraDraft: Motive.FaraPlan(plan, "factura n-are linii de venit"));
                foreach (var depozit in depozite) {
                    var cheie = Descarcare1C.Cheie(h.Id, depozit);
                    if (Reluare1C.Blocheaza(bucla, View, punteVeche, cheie))
                        continue;
                    bucla.ImportaDocument(View, cheie,
                        os => plan == null ? null
                            : Descarcare1C.Materializeaza(os, Grup(plan, depozit), plan.Data,
                                $"{h.Numar}-D", plan.PartenerId, bucla.Tinta(View, h.Id)),
                        motivFaraDraft: Motive.FaraPlan(plan,
                            "grupul de descărcare al depozitului n-a rămas cu nicio linie"));
                }
                if (areCard)
                    bucla.ImportaDocument(View, h.Id + "#card", os => {
                        if (plan == null)
                            return null;
                        IncasariCard++;
                        return Trezorerie1C.Incasare(os, cat, plan.Data, $"{h.Numar}-C",
                            plan.PartenerId, cat.ContPropriuCard(), plan.SumaCard);
                    }, motivFaraDraft: Motive.FaraPlan(plan, "încasarea pe card n-are sumă pozitivă"));
            });
    }

    // Grupul de descărcare al unui depozit; `null` când documentul e deja importat
    // (planul lipsește) sau când toate rândurile lui au rămas nerezolvate —
    // `ImportaDocument` tratează null ca „sursa n-are ce importa".
    static Descarcare1C.Grup Grup(Plan plan, string depozitHex) =>
        plan?.Descarcari.FirstOrDefault(g => g.DepozitHex == depozitHex);

    static bool EsteIncasareCard(Catalog cat, FlaxRandNota r) =>
        cat.Mapeaza(r.ContDebit) == "5125" && (cat.Mapeaza(r.ContCredit)?.StartsWith("411") ?? false);

    static Plan Planifica(ContextLuna ctx, FlaxVanzare h, IReadOnlyList<FlaxRandNota> randuri,
            Dictionary<(int, int), Dictionary<string, FlaxRef>> index,
            List<FlaxVanzareMarfa> marfuri, List<FlaxVanzareServiciu> servicii) {
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
        plan.PartenerId = bucla.LaCerere.AsiguraPartener(h.PartenerId)
            ?? throw new InvalidOperationException(
                $"Partenerul 1C {h.PartenerId} al facturii de ieșire nu s-a putut importa.");

        plan.Venituri = Venituri1C.Aduna(cat, h, h.SumaIncludeTva,
            marfuri.Select(m => (m.ContVenituri, m.CotaTva, m.Suma, m.SumaTva))
                .Concat(servicii.Select(s => (s.ContVenituri, s.CotaTva, s.Suma, s.SumaTva))));
        LiniiVenit += plan.Venituri.Count;
        if (plan.Venituri.Count == 0)
            FaraVenit++;

        plan.Descarcari = Descarcare1C.Planifica(ctx, View, h.Id, plan.Data, randuri, index, h.DepozitId, plan.Punte);

        // Încasarea pe card, inline pe factură (512.5 = 411.1): în Atlas e un
        // document propriu — `Incasare` pe contul „sume în curs de decontare".
        // Imperecherea ei cu factura rămâne pasului 5 (§12.2).
        plan.SumaCard = randuri.Where(r => EsteIncasareCard(cat, r)).Sum(r => r.Suma);

        ConstruiestePunte(cat, plan, h.Id, randuri);
        return plan;
    }

    // Clasificarea rândurilor facturii de ieșire (regula feliei — Vanzare1C.cs).
    static void ConstruiestePunte(Catalog cat, Plan plan, string docId,
            IReadOnlyList<FlaxRandNota> randuri) {
        // Ce postează CHIAR planul: creanța (liniile de venit + pasul TVA) și
        // încasarea pe card. Fără ele, rândurile care se declarau acoperite prin
        // ele trec pe punte (aserțiunea F1c din `Clasificare1C.Declara`).
        var acoperitori = new HashSet<string>(StringComparer.Ordinal);
        if (plan.Venituri.Count > 0)
            acoperitori.Add("venit");
        if (plan.SumaCard > 0)
            acoperitori.Add("card");

        Clasificare1C.Declara(plan.Punte, cat, View, docId, randuri, acoperitori,
            (rand, debit, credit) => {
            if (debit == null || credit == null)
                return null;
            if (debit.StartsWith('6') && cat.EsteContDeStoc(credit))
                return Rand.Evaluat;
            // Regularizarea TVA-ului avansului consumat: 1C stornează pe factură
            // TVA-ul colectat la încasarea avansului (rând 411 = 4427 NEGATIV),
            // iar factura Atlas n-are linie pentru el (avansul consumat e oricum
            // punte, mai jos) — se transcrie.
            if (debit.StartsWith("411") && credit == "4427" && rand.Suma < 0)
                return Rand.Punte("FCL: stornarea TVA colectată a avansului consumat");
            // Creanța: venitul liniei, TVA-ul colectat, avansul facturat (419) —
            // tot ce postează Atlas prin liniile de venit + pasul TVA. Fără nicio
            // linie de venit (3 facturi de imobilizări + 14 storno de comision pe
            // an, cu liniile în secțiuni pe care unealta nu le citește), creanța
            // Atlas nu există și rândul se transcrie integral.
            if (debit.StartsWith("411"))
                return Rand.Acoperit("venit", rand.Suma >= 0
                    ? "FCL fără secțiune de venit: venitul/TVA cesiunii de imobilizări, transcris"
                    : "FCL fără secțiune de venit: stornarea venitului/TVA (comision, avans), transcrisă");
            if (debit == "5125" && credit.StartsWith("411"))
                return Rand.Acoperit("card",
                    "FCL: storno de încasare pe card (512.5 = 411.1), transcris");
            // Consumul avansului încasat: mișcare 419 = 411 fără linie proprie pe
            // factură (nici venit, nici TVA) — inexprimabilă pe factura Atlas.
            if (debit == "419" && credit.StartsWith("411"))
                return Rand.Punte("FCL: consumul avansului facturat (419 = 411)");
            // Regularizarea TVA-ului neexigibil al avizului facturat.
            if (debit == "4428" && credit == "4427")
                return Rand.Punte("FCL: regularizarea TVA neexigibilă a avizului (4428 = 4427)");
            // Cesiuni/casări de imobilizări strecurate pe factură.
            if (debit.StartsWith('2') || credit.StartsWith('2'))
                return Rand.Punte("FCL: rând de imobilizări pe factura de ieșire");
            return null;
        });

        Venituri1C.DeclaraInPunte(plan.Punte, plan.Venituri);
        // Doar încasarea care se NAȘTE se declară postată (F4): suma ne-pozitivă
        // rămâne pe punte, unde clasificarea de mai sus a trimis-o.
        if (plan.SumaCard > 0)
            plan.Punte.ActualAtlas("5125", Catalog.ContCreantaImplicit, plan.SumaCard);
    }

    static Document Materializeaza(IObjectSpace os, Catalog cat, Plan plan) {
        if (plan == null || plan.Venituri.Count == 0)
            return null;
        var fcl = os.CreateObject<FacturaIesire>();
        fcl.Numar = plan.Numar;
        fcl.Data = plan.Data;
        fcl.DataScadenta = plan.Scadenta;
        // Combo-ul viu: emitentul (intern) → clientul. Gestiunea de descărcare
        // rămâne NEcompletată deliberat — descărcarea o construim din sursă, cu
        // loturile ei (vezi nota de formă din Vanzare1C.cs).
        fcl.PredatorId = cat.SediuId;
        fcl.PrimitorId = plan.PartenerId;
        Venituri1C.Materializeaza(os, fcl, plan.Venituri);
        return fcl;
    }

    public static void Raporteaza() {
        Console.WriteLine($"  FCL: {NepostateSarite} antete Posted care NU postează în 1C (sărite), "
            + $"{LiniiVenit} linii de venit (cont × cotă), {FaraVenit} documente fără linie de venit "
            + $"(doar punte), {TranscriseIntegral} documente fără NICIO cheie de plan (transcrise "
            + $"integral pe punte — F1), {NumereLipsa} facturi fără serie/număr, {IncasariCard} "
            + "încasări pe card.");
        Console.WriteLine($"  Venituri: {Venituri1C.TvaLivrareTaxareInversa} linii de livrare cu taxare "
            + "inversă (TVA informativ, fără rând contabil — ca în sursă).");
    }
}

// ======================= Încasările generate de import =======================
// Rândurile 5xx = 411 ale documentelor de vânzare (cardul inline al facturii,
// casa raportului de amănunt) sunt încasări reale, nu artefacte: în Atlas sunt
// documente `Incasare` proprii, cu latura de trezorerie pe contul propriu și linia
// pe Tipul tehnic TRZ (31c). Imperecherea cu documentul stins rămâne pasului 5
// (§12.2 — imperecherea nu postează registre, deci amânarea e gratuită).
static class Trezorerie1C {
    public static Document Incasare(IObjectSpace os, Catalog cat, DateOnly data, string numar,
            Guid partenerId, Guid contPropriuId, decimal suma) {
        if (suma <= 0)
            return null;
        var inc = os.CreateObject<Incasare>();
        inc.Data = data;
        inc.Numar = numar;
        inc.PredatorId = partenerId;
        inc.PrimitorId = contPropriuId;
        var linie = os.CreateObject<DocumentDetaliu>();
        linie.Document = inc;
        linie.TipMaterialId = cat.TipTrezorerieId;
        linie.Valoare = suma;
        return inc;
    }
}

// ======================= 2. RaportDeVanzariCuAmanunt → FCL surogat + DSC + încasări =======================
//
// Surogatul din §12.2: retailul n-are client identificat, dar are venit, TVA, cost
// și bani încasați. FCL pe partenerul generic „CONSUMATOR FINAL" (seed privat) +
// DSC din rândurile de cost + câte o `Incasare` per formă de plată; rândurile
// 5xx = 411 (banii încasați în casă pe facturi NOMINALE, emise separat ca
// documente de vânzare) devin încasări de sine stătătoare, una per rând.
static class HandlerAmanunt {
    public const string View = "RaportDeVanzariCuAmanunt";

    public static readonly HandlerTip Handler =
        new(View, "Raport de vânzări cu amănuntul (FCL surogat)", Importa);

    public static int NepostateSarite { get; private set; }
    public static int IncasariRetail { get; private set; }
    public static int IncasariPeFacturi { get; private set; }
    public static int FacturiFaraPartener { get; private set; }

    sealed record IncasarePeFactura(int Linie, Guid ContPropriuId, Guid PartenerId, decimal Suma);

    sealed class Plan {
        public DateOnly Data;
        public string Numar;
        public Guid ContCasaId;
        public string SimbolCasa;
        public string SimbolCard;
        public List<LinieVenit> Venituri = [];
        public List<Descarcare1C.Grup> Descarcari = [];
        public decimal RetailNumerar;
        public decimal RetailCard;
        public List<IncasarePeFactura> PeFacturi = [];
        public Punte Punte;

        public bool AreDocument => Venituri.Count > 0;
    }

    static void Importa(ContextLuna ctx) {
        var bucla = ctx.Bucla;
        var cat = bucla.Catalog;
        var marfuri = bucla.Flax.RapoarteAmanuntMarfuri(ctx.An, ctx.Luna)
            .GroupBy(m => m.DocumentId, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.Ordinal);
        var servicii = bucla.Flax.RapoarteAmanuntServicii(ctx.An, ctx.Luna)
            .GroupBy(s => s.DocumentId, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.Ordinal);

        foreach (var h in bucla.Flax.RapoarteAmanunt(ctx.An, ctx.Luna))
            ctx.Planifica(h.Data, h.Numar, () => {
                var randuri = bucla.RanduriLuna.GetValueOrDefault(h.Id) ?? [];
                if (randuri.Count == 0) {
                    NepostateSarite++;
                    return;
                }
                var index = Subconto.Indexeaza(bucla.SubcontoLuna.GetValueOrDefault(h.Id) ?? []);
                var areVenit = marfuri.ContainsKey(h.Id) || servicii.ContainsKey(h.Id);
                var depozite = Descarcare1C.Depozite(cat, randuri, index, h.DepozitId);
                // Cheile încasărilor sunt derivabile din rândurile de trezorerie, deci
                // intră și ele în gardul reluării (ca descărcările). Doar cele cu sumă
                // POZITIVĂ: restul n-are document (F4, ca la cardul facturii).
                var cheiIncasari = randuri.Where(r => (cat.Mapeaza(r.ContCredit)?.StartsWith("411") ?? false)
                        && (cat.Mapeaza(r.ContDebit)?.StartsWith('5') ?? false) && r.Suma > 0)
                    .Select(r => $"{h.Id}#inc{r.Linie}").ToList();
                var punteVeche = bucla.EsteCunoscut(View, h.Id + "#punte");
                // Gardul nu poate fi vacuu (F1a) — vezi nota de la factura de ieșire.
                var chei = new List<string>();
                if (areVenit)
                    chei.Add(h.Id);
                chei.AddRange(depozite.Select(d => Descarcare1C.Cheie(h.Id, d)));
                chei.AddRange(cheiIncasari);
                var cunoscut = chei.Count > 0
                    ? chei.All(c => bucla.EsteCunoscut(View, c))
                    : bucla.EsteCunoscut(View, h.Id);

                Plan plan = null;
                if (!cunoscut) {
                    try {
                        plan = Planifica(ctx, h, randuri, index,
                            marfuri.GetValueOrDefault(h.Id) ?? [], servicii.GetValueOrDefault(h.Id) ?? []);
                    }
                    catch (Exception ex) {
                        bucla.EsecPlanificare(View, h.Id, ex);
                        return;
                    }
                    Punti.Scrie(bucla, View, chei.Count > 0 ? h.Id + "#punte" : h.Id,
                        h.Numar, plan.Data, plan.Punte, bucla.ContorPunti, bucla.Avert);
                    if (chei.Count == 0 && !plan.Punte.AreCeva)
                        bucla.NumaraSursaFaraCorespondent();
                }

                if (areVenit)
                    bucla.ImportaDocument(View, h.Id, os => Materializeaza(os, cat, plan),
                        motivFaraDraft: Motive.FaraPlan(plan, "raportul n-are linii de venit"));
                var sursaId = bucla.Tinta(View, h.Id);
                foreach (var depozit in depozite) {
                    var cheieDsc = Descarcare1C.Cheie(h.Id, depozit);
                    if (Reluare1C.Blocheaza(bucla, View, punteVeche, cheieDsc))
                        continue;
                    bucla.ImportaDocument(View, cheieDsc,
                        os => plan == null ? null
                            : Descarcare1C.Materializeaza(os,
                                plan.Descarcari.FirstOrDefault(g => g.DepozitHex == depozit), plan.Data,
                                $"{h.Numar}-D", cat.ConsumatorFinalId, sursaId),
                        motivFaraDraft: Motive.FaraPlan(plan,
                            "grupul de descărcare al depozitului n-a rămas cu nicio linie"));
                }
                // Încasarea retailului: una per formă de plată, pe consumatorul final.
                if (plan is { RetailNumerar: > 0 })
                    bucla.ImportaDocument(View, h.Id + "#numerar", os => Trezorerie1C.Incasare(os, cat,
                        plan.Data, $"{h.Numar}-N", cat.ConsumatorFinalId, plan.ContCasaId, plan.RetailNumerar));
                else if (plan == null)
                    bucla.ImportaDocument(View, h.Id + "#numerar", _ => null,
                        motivFaraDraft: Motive.FaraPlanLaReluare);
                if (plan is { RetailCard: > 0 })
                    bucla.ImportaDocument(View, h.Id + "#card", os => Trezorerie1C.Incasare(os, cat,
                        plan.Data, $"{h.Numar}-C", cat.ConsumatorFinalId, cat.ContPropriuCard(), plan.RetailCard));
                else if (plan == null)
                    bucla.ImportaDocument(View, h.Id + "#card", _ => null,
                        motivFaraDraft: Motive.FaraPlanLaReluare);
                foreach (var cheie in cheiIncasari) {
                    var inc = plan?.PeFacturi.FirstOrDefault(x => $"{h.Id}#inc{x.Linie}" == cheie);
                    bucla.ImportaDocument(View, cheie, os => inc == null ? null
                        : Trezorerie1C.Incasare(os, cat, plan.Data, $"{h.Numar}-{inc.Linie}",
                            inc.PartenerId, inc.ContPropriuId, inc.Suma),
                        motivFaraDraft: Motive.FaraPlan(plan,
                            "încasarea pe factură nominală n-a rămas în plan"));
                }
            });
    }

    static Plan Planifica(ContextLuna ctx, FlaxRaportAmanunt h, IReadOnlyList<FlaxRandNota> randuri,
            Dictionary<(int, int), Dictionary<string, FlaxRef>> index,
            List<FlaxRaportAmanuntMarfa> marfuri, List<FlaxRaportAmanuntServiciu> servicii) {
        var bucla = ctx.Bucla;
        var cat = bucla.Catalog;
        var plan = new Plan {
            Data = DateOnly.FromDateTime(h.Data),
            Numar = h.Numar,
            Punte = new Punte(),
        };
        plan.ContCasaId = cat.ConturiProprii.TryGetValue(h.CasierieId ?? "", out var casa)
            ? casa
            : throw new InvalidOperationException(
                $"Casieria 1C {h.CasierieId} a raportului de amănunt nu e legată de un ContPropriu.");
        plan.SimbolCasa = cat.SimbolContPropriu(plan.ContCasaId)
            ?? throw new InvalidOperationException(
                "Casieria raportului de amănunt n-are cont de evidență (ContImplicit).");
        var cardId = cat.ContPropriuCard();
        plan.SimbolCard = cat.SimbolContPropriu(cardId);

        plan.Venituri = Venituri1C.Aduna(cat, h, h.SumaIncludeTva,
            marfuri.Select(m => (m.ContVenituri, m.CotaTva, m.Suma, m.SumaTva))
                .Concat(servicii.Select(s => (s.ContVenituri, s.CotaTva, s.Suma, s.SumaTva))));
        plan.Descarcari = Descarcare1C.Planifica(ctx, View, h.Id, plan.Data, randuri, index, h.DepozitId, plan.Punte);

        // Banii: rândurile de trezorerie ale raportului. Contra unui cont de
        // creanță (411) sunt încasări pe facturi NOMINALE emise separat — una per
        // rând, cu partenerul din subconto; contra veniturilor/TVA sunt încasarea
        // retailului însuși, agregată per formă de plată.
        foreach (var r in randuri) {
            var debit = cat.Mapeaza(r.ContDebit);
            var credit = cat.Mapeaza(r.ContCredit);
            var contPropriuId = debit == plan.SimbolCasa ? plan.ContCasaId
                : debit == plan.SimbolCard ? cardId : (Guid?)null;
            if (contPropriuId == null || credit == null)
                continue;
            if (credit.StartsWith("411")) {
                var partenerRef = index.Ia(r.Linie, Subconto.Credit, "Parteneri");
                var partenerId = partenerRef == null ? null : bucla.LaCerere.AsiguraPartener(partenerRef.Id);
                if (partenerId == null) {
                    FacturiFaraPartener++;
                    bucla.Avert($"1C:{View}/{h.Id} rândul {r.Linie}: încasarea pe factură n-are partener "
                        + "determinabil în subconto — se încasează pe consumatorul final.");
                }
                plan.PeFacturi.Add(new IncasarePeFactura(r.Linie, contPropriuId.Value,
                    partenerId ?? cat.ConsumatorFinalId, r.Suma));
                IncasariPeFacturi++;
            }
            else if (contPropriuId == plan.ContCasaId)
                plan.RetailNumerar += r.Suma;
            else
                plan.RetailCard += r.Suma;
        }
        if (plan.RetailNumerar > 0)
            IncasariRetail++;
        if (plan.RetailCard > 0)
            IncasariRetail++;

        ConstruiestePunte(cat, plan, h.Id, randuri);
        return plan;
    }

    static void ConstruiestePunte(Catalog cat, Plan plan, string docId,
            IReadOnlyList<FlaxRandNota> randuri) {
        // Acoperitorii banilor, unul per document care se NAȘTE: încasarea de
        // retail per formă de plată (agregată) și câte una per factură nominală
        // (per rând). Sumele ne-pozitive nu produc document (31a), deci nu declară
        // nici acoperire — rândurile lor pleacă pe punte (F4).
        var acoperitori = new HashSet<string>(StringComparer.Ordinal);
        if (plan.RetailNumerar > 0)
            acoperitori.Add("numerar");
        if (plan.RetailCard > 0)
            acoperitori.Add("card");
        foreach (var inc in plan.PeFacturi.Where(x => x.Suma > 0))
            acoperitori.Add($"inc{inc.Linie}");

        Clasificare1C.Declara(plan.Punte, cat, View, docId, randuri, acoperitori,
            (rand, debit, credit) => {
            if (debit == null || credit == null)
                return null;
            if (debit.StartsWith('6') && cat.EsteContDeStoc(credit))
                return Rand.Evaluat;
            if (debit == plan.SimbolCasa || debit == plan.SimbolCard)
                return Rand.Acoperit(
                    credit.StartsWith("411") ? $"inc{rand.Linie}"
                        : debit == plan.SimbolCasa ? "numerar" : "card",
                    "RVA: încasare fără sumă pozitivă (storno de retail), transcrisă");
            return null;
        });

        Venituri1C.DeclaraInPunte(plan.Punte, plan.Venituri);
        if (plan.RetailNumerar > 0)
            plan.Punte.ActualAtlas(plan.SimbolCasa, Catalog.ContCreantaImplicit, plan.RetailNumerar);
        if (plan.RetailCard > 0)
            plan.Punte.ActualAtlas(plan.SimbolCard, Catalog.ContCreantaImplicit, plan.RetailCard);
        foreach (var inc in plan.PeFacturi.Where(x => x.Suma > 0))
            plan.Punte.ActualAtlas(cat.SimbolContPropriu(inc.ContPropriuId),
                Catalog.ContCreantaImplicit, inc.Suma);
    }

    static Document Materializeaza(IObjectSpace os, Catalog cat, Plan plan) {
        if (plan == null || plan.Venituri.Count == 0)
            return null;
        var fcl = os.CreateObject<FacturaIesire>();
        fcl.Numar = plan.Numar;
        fcl.Data = plan.Data;
        // Retailul se încasează pe loc: scadența = data raportului (altfel
        // politica ar pune +30 pe un document care n-are termen).
        fcl.DataScadenta = plan.Data;
        fcl.PredatorId = cat.SediuId;
        fcl.PrimitorId = cat.ConsumatorFinalId;
        Venituri1C.Materializeaza(os, fcl, plan.Venituri);
        return fcl;
    }

    public static void Raporteaza() =>
        Console.WriteLine($"  RVA: {NepostateSarite} antete fără rânduri (sărite), "
            + $"{IncasariRetail} încasări de retail (numerar/card), {IncasariPeFacturi} încasări pe "
            + $"facturi nominale, {FacturiFaraPartener} fără partener determinabil (consumator final).");
}

// ======================= 3. AvizDeIesire → DSC de sine stătător + punte =======================
//
// Rețeta generală de surogat (§12.2): latura de stoc = descărcare cu loturile 1C,
// latura contabilă = transcrierea exactă a rândurilor (418 = 707 creanța de
// facturat, 418 = 4428 TVA-ul neexigibil). 418/4428 rămân FAPTE ale sursei, nu
// mecanisme Atlas — tipul „Aviz" propriu e amânat (§10), iar regularizarea lor la
// facturare vine tot ca punte, pe factura care urmează.
static class HandlerAvizIesire {
    public const string View = "AvizDeIesire";

    public static readonly HandlerTip Handler =
        new(View, "Aviz de ieșire (descărcare + notă)", Importa);

    public static int NepostateSarite { get; private set; }
    public static int Descarcari { get; private set; }

    sealed class Plan {
        public DateOnly Data;
        public Guid PartenerId;
        public List<Descarcare1C.Grup> Descarcari = [];
        public Punte Punte;

        public bool AreDocument => Descarcari.Count > 0;
    }

    static void Importa(ContextLuna ctx) {
        var bucla = ctx.Bucla;
        var cat = bucla.Catalog;
        foreach (var h in bucla.Flax.AvizeIesire(ctx.An, ctx.Luna))
            ctx.Planifica(h.Data, h.Numar, () => {
                var randuri = bucla.RanduriLuna.GetValueOrDefault(h.Id) ?? [];
                if (randuri.Count == 0) {
                    NepostateSarite++;
                    return;
                }
                var index = Subconto.Indexeaza(bucla.SubcontoLuna.GetValueOrDefault(h.Id) ?? []);
                var depozite = Descarcare1C.Depozite(cat, randuri, index, h.DepozitId);
                var punteVeche = bucla.EsteCunoscut(View, h.Id + "#punte");
                var cunoscut = depozite.Count > 0
                    ? depozite.All(d => bucla.EsteCunoscut(View, Descarcare1C.Cheie(h.Id, d)))
                    : bucla.EsteCunoscut(View, h.Id);

                Plan plan = null;
                if (!cunoscut) {
                    try {
                        plan = Planifica(ctx, h, randuri, index);
                    }
                    catch (Exception ex) {
                        bucla.EsecPlanificare(View, h.Id, ex);
                        return;
                    }
                    Punti.Scrie(bucla, View, depozite.Count > 0 ? h.Id + "#punte" : h.Id,
                        h.Numar, plan.Data, plan.Punte, bucla.ContorPunti, bucla.Avert);
                    if (depozite.Count == 0 && !plan.Punte.AreCeva)
                        bucla.NumaraSursaFaraCorespondent();
                }
                foreach (var depozit in depozite) {
                    if (Reluare1C.Blocheaza(bucla, View, punteVeche, Descarcare1C.Cheie(h.Id, depozit)))
                        continue;
                    Descarcari++;
                    bucla.ImportaDocument(View, Descarcare1C.Cheie(h.Id, depozit),
                        os => plan == null ? null
                            : Descarcare1C.Materializeaza(os,
                                plan.Descarcari.FirstOrDefault(g => g.DepozitHex == depozit),
                                plan.Data, h.Numar, plan.PartenerId, null),
                        motivFaraDraft: Motive.FaraPlan(plan,
                            "grupul de descărcare al depozitului n-a rămas cu nicio linie"));
                }
            });
    }

    static Plan Planifica(ContextLuna ctx, FlaxAvizIesire h, IReadOnlyList<FlaxRandNota> randuri,
            Dictionary<(int, int), Dictionary<string, FlaxRef>> index) {
        var bucla = ctx.Bucla;
        var cat = bucla.Catalog;
        var plan = new Plan {
            Data = DateOnly.FromDateTime(h.Data),
            Punte = new Punte(),
        };
        plan.PartenerId = bucla.LaCerere.AsiguraPartener(h.PartenerId)
            ?? throw new InvalidOperationException(
                $"Partenerul 1C {h.PartenerId} al avizului de ieșire nu s-a putut importa.");
        plan.Descarcari = Descarcare1C.Planifica(ctx, View, h.Id, plan.Data, randuri, index, h.DepozitId, plan.Punte);

        Clasificare1C.Declara(plan.Punte, cat, View, h.Id, randuri, Clasificare1C.Niciunul,
            (rand, debit, credit) => {
            if (debit == null || credit == null)
                return null;
            if (debit.StartsWith('6') && cat.EsteContDeStoc(credit))
                return Rand.Evaluat;
            if (debit == "418")
                return Rand.Punte("AVE: creanța de facturat a avizului (418 = venit / TVA neexigibilă)");
            return null;
        });
        return plan;
    }

    public static void Raporteaza() =>
        Console.WriteLine($"  AVE: {Descarcari} descărcări de gestiune de sine stătătoare, "
            + $"{NepostateSarite} antete fără rânduri (sărite).");
}

// ======================= 4. AvizDeIntrare → LDI+ + punte =======================
//
// Simetricul avizului de ieșire: marfa intră în gestiune FĂRĂ factură (371 = 408).
// Stocul se mișcă printr-un plus de inventar (lotul se naște pe linia lui, cu
// prețul din rând), iar diferența de formă — Atlas contează plusul pe venitul din
// exploatare al profilului (7588), 1C pe furnizorul de facturi nesosite — se
// transcrie în punte (7588 = 408). Regularizarea vine cu factura, tot ca punte.
static class HandlerAvizIntrare {
    public const string View = "AvizDeIntrare";

    public static readonly HandlerTip Handler =
        new(View, "Aviz de intrare (plus de stoc + notă)", Importa);

    public static int NepostateSarite { get; private set; }
    public static int Linii { get; private set; }

    sealed record LinieIntrare(Guid ProdusId, TipInfo Tip, string CheieLot, decimal Cantitate, decimal Pret);

    sealed class Plan {
        public DateOnly Data;
        public Guid GestiuneId;
        public List<LinieIntrare> Linii = [];
        public Punte Punte;

        public bool AreDocument => Linii.Count > 0;
    }

    static void Importa(ContextLuna ctx) {
        var bucla = ctx.Bucla;
        foreach (var h in bucla.Flax.AvizeIntrare(ctx.An, ctx.Luna))
            ctx.Planifica(h.Data, h.Numar, () => {
                var randuri = bucla.RanduriLuna.GetValueOrDefault(h.Id) ?? [];
                if (randuri.Count == 0) {
                    NepostateSarite++;
                    return;
                }
                Plan plan = null;
                if (!bucla.EsteCunoscut(View, h.Id)) {
                    try {
                        plan = Planifica(ctx, h, randuri);
                    }
                    catch (Exception ex) {
                        bucla.EsecPlanificare(View, h.Id, ex);
                        return;
                    }
                    Punti.Scrie(bucla, View, plan.AreDocument ? h.Id + "#punte" : h.Id,
                        h.Numar, plan.Data, plan.Punte, bucla.ContorPunti, bucla.Avert);
                    if (!plan.AreDocument && !plan.Punte.AreCeva)
                        bucla.NumaraSursaFaraCorespondent();
                }
                if (plan == null || plan.AreDocument)
                    bucla.ImportaDocument(View, h.Id, os => Materializeaza(os, bucla.Catalog, plan),
                        motivFaraDraft: Motive.FaraPlan(plan,
                            "avizul de intrare n-a rămas cu nicio linie de plus"));
            });
    }

    static Plan Planifica(ContextLuna ctx, FlaxAvizIntrare h, IReadOnlyList<FlaxRandNota> randuri) {
        var bucla = ctx.Bucla;
        var cat = bucla.Catalog;
        var plan = new Plan {
            Data = DateOnly.FromDateTime(h.Data),
            Punte = new Punte(),
        };
        var index = Subconto.Indexeaza(bucla.SubcontoLuna.GetValueOrDefault(h.Id) ?? []);

        foreach (var r in randuri) {
            var debit = cat.Mapeaza(r.ContDebit);
            if (!cat.EsteContDeStoc(debit))
                continue;
            var context = $"1C:{View}/{h.Id} rândul {r.Linie}";
            var lotRef = index.Ia(r.Linie, Subconto.Debit, Subconto.Loturi);
            var nomRef = index.Ia(r.Linie, Subconto.Debit, Subconto.Nomenclator);
            if (lotRef == null || nomRef == null)
                throw new InvalidOperationException(
                    $"{context}: rândul de intrare n-are subconto de lot/nomenclator.");
            var tip = cat.TipStocPentru(debit)
                ?? throw new InvalidOperationException(
                    $"{context}: contul de stoc {debit} n-are TipMaterial în profil.");
            var depozitHex = index.Ia(r.Linie, Subconto.Debit, Subconto.Depozite)?.Id ?? h.DepozitId ?? "";
            var gestiuneId = cat.Gestiuni.TryGetValue(depozitHex, out var g)
                ? g
                : throw new InvalidOperationException(
                    $"{context}: depozitul 1C {depozitHex} nu e legat de o Gestiune.");
            if (plan.GestiuneId != Guid.Empty && plan.GestiuneId != gestiuneId)
                throw new InvalidOperationException(
                    $"{context}: avizul de intrare atinge mai multe gestiuni — nesuportat (sursa n-are cazuri).");
            plan.GestiuneId = gestiuneId;

            var produsId = bucla.LaCerere.AsiguraProdus(nomRef.Id, tip.Cod)
                ?? throw new InvalidOperationException(
                    $"{context}: nomenclatorul 1C {nomRef.Id} nu s-a putut importa pe Tipul {tip.Cod}.");
            var cantitate = Math.Abs(r.CantitateDebit);
            if (cantitate == 0)
                throw new InvalidOperationException($"{context}: rând de intrare fără cantitate.");
            var valoare = Math.Abs(r.Suma);
            // Lotul se naște pe linia PROPRIE, cu cheia 1C a avizului (creatorul
            // lotului în sursă e avizul însuși — verificat pe subconto).
            plan.Linii.Add(new LinieIntrare(produsId, tip,
                Catalog.CheieLot(cat.TipRef(View), h.Id, nomRef.Id, debit), cantitate, valoare / cantitate));
            Linii++;
            plan.Punte.ActualAtlas(debit, cat.ContPlusInventar, valoare);
        }

        Clasificare1C.Declara(plan.Punte, cat, View, h.Id, randuri, Clasificare1C.Niciunul,
            (rand, debit, credit) => {
            if (debit == null || credit == null)
                return null;
            if (cat.EsteContDeStoc(debit))
                return Rand.Punte("AVI: intrarea fără factură (408) contra plusului de inventar");
            return null;
        });
        return plan;
    }

    static Document Materializeaza(IObjectSpace os, Catalog cat, Plan plan) {
        if (plan == null || plan.Linii.Count == 0)
            return null;
        var ldi = os.CreateObject<ListaDiferenteInventar>();
        ldi.Data = plan.Data;
        ldi.PredatorId = plan.GestiuneId;
        ldi.PrimitorId = cat.ComisieId;
        var gestiune = os.GetObjectByKey<Gestiune>(plan.GestiuneId);
        foreach (var l in plan.Linii) {
            var d = os.CreateObject<ListaDiferenteInventarDetaliu>();
            d.Document = ldi;
            d.TipMaterialId = l.Tip.Id;
            d.Directie = DirectieDiferenta.Plus;
            d.Cantitate = l.Cantitate;
            d.PretEvaluare = l.Pret;
            var lot = d.CreeazaLot(os, os.GetObjectByKey<Produs>(l.ProdusId), gestiune);
            cat.LeagaLotNou(os, l.CheieLot, lot.ID);
        }
        return ldi;
    }

    public static void Raporteaza() =>
        Console.WriteLine($"  AVI: {Linii} linii de intrare fără factură (plus de inventar + punte 408), "
            + $"{NepostateSarite} antete fără rânduri (sărite).");
}
