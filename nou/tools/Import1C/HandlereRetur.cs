using Atlas.Conta.BackOffice.Module.BusinessObjects;
using DevExpress.ExpressApp;

namespace Import1C;

// PASUL 4, tipurile 5–6: retururile (RLF/RDC — tipurile de produs din felia 1C-a,
// decizia 46e). Reprezentarea e a motorului, nu a importului: liniile se culeg
// POZITIVE, iar `PregatesteOperare` le semnează negativ pe corespondența
// ORIGINALĂ (achiziție/vânzare), cu `RegulaContare.PastreazaSemn` — exact
// rândurile negative ale sursei.
//
// Amândouă construiesc liniile de STOC din rândurile de notă + subconto (ca
// BTR/BCS la pasul 3): secțiunile n-au lotul, iar câmpul `DocumentIntrare` /
// `DocumentVanzare` al secțiunilor e GOL pe toate liniile lui 2025 (verificat) —
// deci identitatea lotului vine exclusiv din subconto.

// ======================= 5. ReturLaFurnizor → RLF =======================
//
// Marfa iese din gestiune înapoi la furnizor: 3xx = 401 cu −V (stornarea
// achiziției) + 4426 = 401 / 4426 = 4427 cu −TVA. Valoarea o pune motorul din
// prețul lotului ATLAS (`PregatesteOperare`), nu din sursă — deci rândurile de
// stoc nu se declară în punte (diferența rămasă din netarea deschiderii e
// diferență justificată, §8.3, nu ceva de „reparat" cu o notă).
//
// TVA-ul: 1C îl postează pe rânduri separate, fără legătură cu linia de stoc, dar
// Atlas îl generează din `TipTva` + `ValoareTva` ale liniei. Regimul și suma vin
// din SECȚIUNE, potrivite pe (nomenclator × cont de evidență) — cheia care leagă
// secțiunea de rândurile de notă (verificat pe ianuarie: toate cele 36 de chei ale
// rândurilor au secțiune corespondentă) — și se distribuie pe liniile cheii
// pro-rata cu cantitatea, cu restul de rotunjire pe ultima.
static class HandlerReturFurnizor {
    public const string View = "ReturLaFurnizor";

    public static readonly HandlerTip Handler = new(View, "Retur la furnizor", Importa);

    public static int NepostateSarite { get; private set; }
    public static int Linii { get; private set; }
    public static int LiniiFaraSectiune { get; private set; }
    public static int DocumenteSparte { get; private set; }
    public static int RanduriNerezolvate { get; private set; }
    public static int LiniiNeacoperite { get; private set; }

    sealed record LinieRetur(Guid LotId, Guid TipMaterialId, decimal Cantitate) {
        public Guid? TipTvaId { get; set; }
        public decimal Tva { get; set; }
    }

    sealed class Grup {
        public string DepozitHex;
        public Guid GestiuneId;
        public List<LinieRetur> Linii = [];
    }

    sealed class Plan {
        public Guid PartenerId;
        public DateOnly Data;
        public string Numar;
        public List<Grup> Grupuri = [];
        public Punte Punte;
    }

    static void Importa(ContextLuna ctx) {
        var bucla = ctx.Bucla;
        var cat = bucla.Catalog;
        var sectiuni = bucla.Flax.RetururiFurnizorMarfuri(ctx.An, ctx.Luna)
            .GroupBy(m => m.DocumentId, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.Ordinal);

        foreach (var h in bucla.Flax.RetururiFurnizor(ctx.An, ctx.Luna))
            ctx.Planifica(h.Data, h.Numar, () => {
                var randuri = bucla.RanduriLuna.GetValueOrDefault(h.Id) ?? [];
                if (randuri.Count == 0) {
                    NepostateSarite++;
                    return;
                }
                var index = Subconto.Indexeaza(bucla.SubcontoLuna.GetValueOrDefault(h.Id) ?? []);
                // Gestiunile atinse, derivate din sursă — cheile documentelor Atlas
                // (mecanica bonului de consum: un document per gestiune predatoare).
                var depozite = Depozite(cat, randuri, index, h.DepozitId);
                var chei = depozite.Select(d => Cheie(h.Id, d, depozite.Count)).ToList();
                var punteVeche = bucla.EsteCunoscut(View, h.Id + "#punte");
                if (depozite.Count > 1)
                    DocumenteSparte++;
                // D1: grupurile de depozit sunt de stoc, dar puntea returului e a
                // DOCUMENTULUI (`Clasificare1C.Declara` peste toate rândurile) —
                // deci planul nu se sparge per grup ca la bonul de consum.
                var partiala = Reluare1C.UnitatePartiala(bucla, View, h.Id, chei, chei,
                    "unitate parțial importată de o rulare anterioară (necesită --deblocheaza)");
                if (partiala == Reluare1C.Partiala.Refuzata)
                    return;

                Plan plan = null;
                if (partiala != Reluare1C.Partiala.DoarDrafturi
                        && (!chei.All(c => bucla.EsteCunoscut(View, c))
                            || (chei.Count == 0 && !bucla.EsteCunoscut(View, h.Id)))) {
                    try {
                        plan = Planifica(ctx, h, randuri, index, sectiuni.GetValueOrDefault(h.Id) ?? []);
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
                foreach (var depozit in depozite) {
                    var cheie = Cheie(h.Id, depozit, depozite.Count);
                    if (Reluare1C.Blocheaza(bucla, View, punteVeche, cheie))
                        continue;
                    bucla.ImportaDocument(View, cheie,
                        os => plan == null ? null
                            : Materializeaza(os, plan, plan.Grupuri.FirstOrDefault(g => g.DepozitHex == depozit)),
                        motivFaraDraft: Motive.FaraPlan(plan,
                            "grupul de retur al depozitului n-a rămas cu nicio linie"));
                }
            });
    }

    static string Cheie(string docId, string depozitHex, int total) =>
        total <= 1 ? docId : $"{docId}@{depozitHex}";

    static List<string> Depozite(Catalog cat, IEnumerable<FlaxRandNota> randuri,
            Dictionary<(int, int), Dictionary<string, FlaxRef>> index, string depozitImplicit) {
        var depozite = new List<string>();
        foreach (var r in randuri) {
            if (!EsteRandDeRetur(cat, r))
                continue;
            var depozitHex = index.Ia(r.Linie, Subconto.Debit, Subconto.Depozite)?.Id
                ?? depozitImplicit ?? "";
            if (!depozite.Contains(depozitHex))
                depozite.Add(depozitHex);
        }
        return depozite;
    }

    // Rândul de retur: contul de stoc pe DEBIT (marfa se stornează), datoria pe
    // credit. Suma e negativă în sursă — semnul îl pune motorul, nu importul.
    static bool EsteRandDeRetur(Catalog cat, FlaxRandNota r) =>
        cat.EsteContDeStoc(cat.Mapeaza(r.ContDebit)) && !cat.EsteContDeStoc(cat.Mapeaza(r.ContCredit));

    static Plan Planifica(ContextLuna ctx, FlaxReturFurnizor h, IReadOnlyList<FlaxRandNota> randuri,
            Dictionary<(int, int), Dictionary<string, FlaxRef>> index,
            List<FlaxReturFurnizorMarfa> sectiune) {
        var bucla = ctx.Bucla;
        var cat = bucla.Catalog;
        var plan = new Plan {
            Data = DateOnly.FromDateTime(h.Data),
            Numar = string.Concat(h.SeriaFactura, h.NumarFactura).Trim() is { Length: > 0 } n ? n : h.Numar,
            Punte = new Punte(),
        };
        plan.PartenerId = bucla.LaCerere.AsiguraPartener(h.PartenerId)
            ?? throw new InvalidOperationException(
                $"Partenerul 1C {h.PartenerId} al returului la furnizor nu s-a putut importa.");

        // TVA-ul secțiunii, pe cheia care leagă secțiunea de rândurile de notă.
        var tvaPeCheie = sectiune
            .GroupBy(s => (s.NomenclatorId, Simbol: cat.Mapeaza(s.ContEvidenta)))
            .ToDictionary(g => g.Key, g => (
                Cota: g.First().CotaTva,
                Tva: g.Sum(x => h.InLei(x.SumaTva)),
                Cantitate: g.Sum(x => Math.Abs(x.Cantitate))));

        var dejaAlocat = new Dictionary<Guid, decimal>();
        var peCheie = new Dictionary<(string, string), List<LinieRetur>>();
        using var os = bucla.CreeazaObjectSpace();

        foreach (var r in randuri) {
            if (!EsteRandDeRetur(cat, r))
                continue;
            var simbolDebit = cat.Mapeaza(r.ContDebit);
            var context = $"1C:{View}/{h.Id} rândul {r.Linie}";
            var lotRef = index.Ia(r.Linie, Subconto.Debit, Subconto.Loturi);
            var nomRef = index.Ia(r.Linie, Subconto.Debit, Subconto.Nomenclator);
            var depozitHex = index.Ia(r.Linie, Subconto.Debit, Subconto.Depozite)?.Id
                ?? h.DepozitId ?? "";
            var rezolvat = MiscareStoc1C.Rezolva(bucla, lotRef, nomRef, simbolDebit, context);
            if (rezolvat is not var (lot, tip, produsId)) {
                // Nici stoc, nici contabilitate: se măsoară, ca rândul să nu fie
                // singura mișcare a sursei invizibilă în ambele contracte.
                RanduriNerezolvate++;
                bucla.Divergenta($"{View}/{h.Id}", "RLF: rând cu lot/produs nerezolvabil — nereturnat",
                    nomRef == null ? null
                        : [new EfectStoc(nomRef.Id, depozitHex, Math.Abs(r.CantitateDebit), -r.Suma)],
                    simbolDebit, cat.Mapeaza(r.ContCredit), r.Suma);
                // Rândul e deja înregistrat integral ca nepostat, mai sus: se scoate
                // din acumularea evaluată (unde clasificarea îl pune ca pe orice
                // rând de stornare), ca să nu fie explicat de două ori.
                plan.Punte.ActualEvaluat(simbolDebit, cat.Mapeaza(r.ContCredit), r.Suma);
                continue;
            }
            var gestiuneId = cat.Gestiuni.TryGetValue(depozitHex, out var g)
                ? g
                : throw new InvalidOperationException(
                    $"{context}: depozitul 1C {depozitHex} nu e legat de o Gestiune.");
            var grup = plan.Grupuri.FirstOrDefault(x => x.GestiuneId == gestiuneId);
            if (grup == null)
                plan.Grupuri.Add(grup = new Grup { DepozitHex = depozitHex, GestiuneId = gestiuneId });

            // **Pinul pe lotul original, cu supapa 48a**: 27 din 36 de rânduri ale
            // lui ianuarie au ca „lot" un lot creat de RETURUL ÎNSUȘI (1C
            // reprezintă returul ca lot nou, cu cantitate negativă — artefactul
            // retur-ca-lot din 47d). Un asemenea lot nu există în Atlas și nici
            // n-ar avea sold: cererea intră în supapă fără pin și se acoperă FIFO
            // din produs × gestiune, cu raport.
            var pin = lot != null && lot.Id != Guid.Empty && !EsteLotPropriu(lotRef, h.Id) ? lot.Id : (Guid?)null;
            var cantitate = Math.Abs(r.CantitateDebit);
            var (alocari, ramas) = bucla.Alocare.Aloca(os, pin, produsId, gestiuneId,
                tip.Registru, plan.Data, cantitate, dejaAlocat);
            foreach (var (lotId, q) in alocari) {
                var linie = new LinieRetur(lotId, tip.Id, q);
                grup.Linii.Add(linie);
                var cheie = (nomRef.Id, simbolDebit);
                if (!peCheie.TryGetValue(cheie, out var lista))
                    peCheie[cheie] = lista = [];
                lista.Add(linie);
                Linii++;
                // Ce postează motorul: stornarea achiziției la costul lotului ATLAS
                // (3xx = 401, cu semn negativ — 46e). Diferența față de cifra
                // sursei e diferența de EVALUARE, iar ea cade pe 401 — un cont pe
                // care nicio măsurătoare de stoc nu-l atinge. Se declară aici ca să
                // fie măsurată, nu presupusă (era singura explicație a abaterii de
                // 401 din contractul 1, și venea din plafon).
                plan.Punte.ActualEvaluat(tip.Simbol, Catalog.ContDatorieImplicit,
                    -Scara.RotunjesteBani(q * HandlerTransfer.PretLot(os, lotId)));
            }
            if (ramas > 0) {
                bucla.Avert($"{context}: {ramas:N3} din {cantitate:N3} n-au acoperire în gestiune — "
                    + "returul iese parțial (diferență de stoc raportată).");
                // Partea neacoperită n-are linie, deci Atlas nu stornează nici
                // datoria față de furnizor: rândul 1C s-ar pierde tăcut. Se
                // transcrie proporțional în punte (verificat pe ianuarie: jumătate
                // din rândurile de retur n-au marfa în stocul Atlas, fiindcă 1C le
                // ține pe loturi-retur pe care netarea deschiderii nu le are).
                LiniiNeacoperite++;
                var valoareRamas = cantitate == 0 ? r.Suma : r.Suma * ramas / cantitate;
                plan.Punte.Categoria("RLF: retur fără acoperire în stoc — stornarea se transcrie contabil")
                    .Tinta1C(simbolDebit, cat.Mapeaza(r.ContCredit), valoareRamas);
                // Partea transcrisă e postată (de nota-punte) ⇒ pleacă din
                // acumularea evaluată.
                plan.Punte.ActualEvaluat(simbolDebit, cat.Mapeaza(r.ContCredit), valoareRamas);
                // Măsurătoarea: 1C a scos marfa (mișcare de debit NEGATIVĂ pe contul
                // de stoc — storno), Atlas n-a scos-o, deci Atlas are în plus exact
                // opusul mișcării sursei. Contabil nu divergem: puntea de mai sus a
                // transcris partea neacoperită la valoarea ei din sursă.
                bucla.Divergenta($"{View}/{h.Id}",
                    "RLF: retur fără acoperire în stoc — marfa rămâne în stocul Atlas",
                    [new EfectStoc(nomRef.Id, depozitHex, ramas, -valoareRamas)],
                    simbolDebit, cat.Mapeaza(r.ContCredit));
            }
        }

        DistribuieTva(cat, peCheie, tvaPeCheie);
        ConstruiestePunte(cat, plan, h.Id, randuri);
        return plan;
    }

    // Lotul din subconto e creat chiar de documentul curent (retur-ca-lot)?
    static bool EsteLotPropriu(FlaxRef lotRef, string docId) =>
        lotRef != null && string.Equals(lotRef.Id, docId, StringComparison.Ordinal);

    // TVA-ul cheii (nomenclator × cont) se împarte pe liniile ei pro-rata cu
    // cantitatea; ultima linie primește restul, ca suma să iasă EXACT cea a
    // sursei (36a: TVA-ul cules nu se recalculează din cotă).
    static void DistribuieTva(Catalog cat,
            Dictionary<(string, string), List<LinieRetur>> peCheie,
            Dictionary<(string NomenclatorId, string Simbol), (string Cota, decimal Tva, decimal Cantitate)> tvaPeCheie) {
        foreach (var (cheie, linii) in peCheie) {
            if (!tvaPeCheie.TryGetValue(cheie, out var info)) {
                LiniiFaraSectiune += linii.Count;
                continue;
            }
            var tipTva = cat.TipTvaCules(info.Cota, info.Tva);
            var total = linii.Sum(l => l.Cantitate);
            var repartizat = 0m;
            for (var i = 0; i < linii.Count; i++) {
                var tva = i == linii.Count - 1 || total == 0m
                    ? info.Tva - repartizat
                    : Math.Round(info.Tva * linii[i].Cantitate / total, 2);
                repartizat += tva;
                linii[i].TipTvaId = tipTva;
                linii[i].Tva = tva;
            }
        }
    }

    static void ConstruiestePunte(Catalog cat, Plan plan, string docId,
            IReadOnlyList<FlaxRandNota> randuri) {
        // Pasul TVA al motorului postează doar dacă returul are linii CU regim de
        // TVA: fără ele, rândul 4426 al sursei n-are cine să-l acopere (aserțiunea
        // F1c) și se transcrie.
        var acoperitori = plan.Grupuri.SelectMany(g => g.Linii)
                .Any(l => l.TipTvaId != null && l.Tva != 0m)
            ? new HashSet<string>(StringComparer.Ordinal) { "tva" }
            : Clasificare1C.Niciunul;

        Clasificare1C.Declara(plan.Punte, cat, View, docId, randuri, acoperitori,
            (rand, debit, credit) => {
            if (debit == null || credit == null)
                return null;
            // Stornarea achiziției: Atlas o postează la costul LOTULUI lui.
            if (cat.EsteContDeStoc(debit) && !cat.EsteContDeStoc(credit))
                return Rand.Evaluat;
            // TVA-ul stornat (4426 = 401 la regim normal, 4426 = 4427 la taxare
            // inversă) — îl generează pasul TVA din motor, din liniile de mai sus.
            if (debit == "4426")
                return Rand.Acoperit("tva",
                    "RLF: TVA stornat fără linie de retur în Atlas, transcris");
            // Ajustarea de cost pe care 1C o postează pe retur (6xx = 3xx): Atlas
            // n-are regulă pentru ea. Se transcrie — MIȘCAREA DE STOC pe care o
            // însoțește în 1C NU se preia (raportată aici, nu inventată).
            if (debit.StartsWith('6') && cat.EsteContDeStoc(credit))
                return Rand.Punte("RLF: ajustare de cost pe retur (6xx = 3xx), fără mișcare de stoc în Atlas");
            // Serviciile returnate (transport, comisioane) n-au lot ⇒ nu pot sta
            // pe un retur Atlas (fiecare linie descarcă un lot).
            if (debit.StartsWith('6'))
                return Rand.Punte("RLF: linie de serviciu returnată (fără lot)");
            return null;
        });

        // Ce postează Atlas pe TVA: 4426 = 401 (fallback-ul politicii pe latura
        // primitorului — furnizorul importat n-are cont implicit), cu semnul
        // storno; la taxare inversă 4426 = 4427.
        foreach (var linie in plan.Grupuri.SelectMany(g => g.Linii)) {
            if (linie.TipTvaId == null || linie.Tva == 0m)
                continue;
            plan.Punte.ActualAtlas("4426",
                cat.EsteTaxareInversa(linie.TipTvaId) ? "4427" : "401", -linie.Tva);
        }
    }

    static Document Materializeaza(IObjectSpace os, Plan plan, Grup grup) {
        if (grup == null || grup.Linii.Count == 0)
            return null;
        var rlf = os.CreateObject<ReturFurnizor>();
        rlf.Data = plan.Data;
        rlf.Numar = plan.Numar;
        rlf.PredatorId = grup.GestiuneId;
        rlf.PrimitorId = plan.PartenerId;
        foreach (var l in grup.Linii) {
            var d = os.CreateObject<DocumentDetaliu>();
            d.Document = rlf;
            d.TipMaterialId = l.TipMaterialId;
            d.LotId = l.LotId;
            // Cantitatea și TVA-ul se culeg POZITIVE — semnul e al motorului (46a).
            d.Cantitate = l.Cantitate;
            d.TipTvaId = l.TipTvaId;
            d.ValoareTva = l.Tva;
        }
        return rlf;
    }

    public static void Raporteaza() =>
        Console.WriteLine($"  RLF: {Linii} linii de retur pe lot, {LiniiNeacoperite} rânduri fără "
            + $"acoperire în stoc (stornarea transcrisă în punte), {LiniiFaraSectiune} fără secțiune "
            + $"corespondentă (rămân fără TVA), {DocumenteSparte} documente sparte pe gestiuni, "
            + $"{RanduriNerezolvate} rânduri nerezolvate, {NepostateSarite} antete fără rânduri.");
}

// ======================= 6. ReturDeLaClient → RDC =======================
//
// UN SINGUR document cu linii pe DOUĂ roluri (decizia 46e): venit (fără lot, Tip
// per contul de venit al sursei, valoare culeasă) și cost (lotul ORIGINAL,
// cantitate — valoarea o pune motorul din prețul lotului). Ambele se culeg
// pozitiv; motorul le semnează negativ pe corespondența originală.
static class HandlerReturClient {
    public const string View = "ReturDeLaClient";

    public static readonly HandlerTip Handler = new(View, "Retur de la client", Importa);

    public static int NepostateSarite { get; private set; }
    public static int LiniiVenit { get; private set; }
    public static int LiniiCost { get; private set; }
    public static int LoturiCreate { get; private set; }
    public static int AliasuriLot { get; private set; }
    public static int RanduriNerezolvate { get; private set; }

    // Linia de cost: ori revine pe un lot EXISTENT (`LotId`), ori pe unul pe care
    // importul îl creează din cheia sursei (`CheieLot` + `Pret` — vezi nota din
    // `Planifica`). `CheieAlias` = identitatea 1C a lotului ORIGINAL, când acesta
    // nu există în Atlas: lotul creat de retur îi ține locul, iar aliasul face ca
    // mișcările 1C ulterioare pin-uite pe original să-l regăsească.
    sealed record LinieCost(Guid TipMaterialId, decimal Cantitate, Guid? LotId,
        Guid ProdusId, string CheieLot, decimal Pret, string CheieAlias);

    sealed class Plan {
        public Guid PartenerId;
        public Guid GestiuneId;
        public DateOnly Data;
        public string Numar;
        public DateOnly? Scadenta;
        public List<LinieVenit> Venituri = [];
        public List<LinieCost> Costuri = [];
        public Punte Punte;

        public bool AreDocument => Venituri.Count > 0 || Costuri.Count > 0;
    }

    static void Importa(ContextLuna ctx) {
        var bucla = ctx.Bucla;
        var marfuri = bucla.Flax.RetururiClientMarfuri(ctx.An, ctx.Luna)
            .GroupBy(m => m.DocumentId, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.Ordinal);
        var servicii = bucla.Flax.RetururiClientServicii(ctx.An, ctx.Luna)
            .GroupBy(s => s.DocumentId, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.Ordinal);

        foreach (var h in bucla.Flax.RetururiClient(ctx.An, ctx.Luna))
            ctx.Planifica(h.Data, h.Numar, () => {
                var randuri = bucla.RanduriLuna.GetValueOrDefault(h.Id) ?? [];
                if (randuri.Count == 0) {
                    NepostateSarite++;
                    return;
                }
                Plan plan = null;
                if (!bucla.EsteCunoscut(View, h.Id)) {
                    try {
                        plan = Planifica(ctx, h, randuri,
                            marfuri.GetValueOrDefault(h.Id) ?? [], servicii.GetValueOrDefault(h.Id) ?? []);
                    }
                    catch (Exception ex) {
                        bucla.EsecPlanificare(View, h.Id, ex);
                        return;
                    }
                    Punti.Scrie(bucla, View, plan.AreDocument ? h.Id + "#punte" : h.Id,
                        plan.Numar, plan.Data, plan.Punte, bucla.ContorPunti, bucla.Avert);
                    if (!plan.AreDocument && !plan.Punte.AreCeva)
                        bucla.NumaraSursaFaraCorespondent();
                }
                if (plan == null || plan.AreDocument)
                    bucla.ImportaDocument(View, h.Id, os => Materializeaza(os, bucla.Catalog, plan),
                        motivFaraDraft: Motive.FaraPlan(plan,
                            "returul n-a rămas nici cu venit, nici cu cost"));
            });
    }

    static Plan Planifica(ContextLuna ctx, FlaxReturClient h, IReadOnlyList<FlaxRandNota> randuri,
            List<FlaxReturClientMarfa> marfuri, List<FlaxReturClientServiciu> servicii) {
        var bucla = ctx.Bucla;
        var cat = bucla.Catalog;
        var plan = new Plan {
            Data = DateOnly.FromDateTime(h.Data),
            Numar = string.Concat(h.SeriaFactura, h.NumarFactura).Trim() is { Length: > 0 } n ? n : h.Numar,
            Scadenta = h.DataScadenta is { } s ? DateOnly.FromDateTime(s) : null,
            Punte = new Punte(),
        };
        plan.PartenerId = bucla.LaCerere.AsiguraPartener(h.PartenerId)
            ?? throw new InvalidOperationException(
                $"Partenerul 1C {h.PartenerId} al returului de la client nu s-a putut importa.");
        plan.GestiuneId = cat.Gestiuni.TryGetValue(h.DepozitId ?? "", out var g)
            ? g
            : throw new InvalidOperationException(
                $"Depozitul 1C {h.DepozitId} al returului de la client nu e legat de o Gestiune.");

        plan.Venituri = Venituri1C.Aduna(cat, h, h.SumaIncludeTva,
            marfuri.Select(m => (m.ContVenituri, m.CotaTva, m.Suma, m.SumaTva))
                .Concat(servicii.Select(s => (s.ContVenituri, s.CotaTva, s.Suma, s.SumaTva))));
        LiniiVenit += plan.Venituri.Count;

        // RDC-ul nu ALOCĂ din stoc (linia de cost revine pe lotul original sau îl
        // recreează), dar are nevoie de PREȚUL lotului original: costul care revine
        // e cel al lotului Atlas, iar diferența față de cifra sursei e o divergență
        // de evaluare care trebuie măsurată (Punte.cs), nu presupusă.
        using var os = bucla.CreeazaObjectSpace();
        var index = Subconto.Indexeaza(bucla.SubcontoLuna.GetValueOrDefault(h.Id) ?? []);
        foreach (var r in randuri) {
            if (!Descarcare1C.EsteRandDeCost(cat, r))
                continue;
            var context = $"1C:{View}/{h.Id} rândul {r.Linie}";
            var simbolCredit = cat.Mapeaza(r.ContCredit);
            var lotRef = index.Ia(r.Linie, Subconto.Credit, Subconto.Loturi);
            var nomRef = index.Ia(r.Linie, Subconto.Credit, Subconto.Nomenclator);
            var rezolvat = MiscareStoc1C.Rezolva(bucla, lotRef, nomRef, simbolCredit, context);
            if (rezolvat is not var (lot, tip, produsId)) {
                RanduriNerezolvate++;
                // Rândul nu produce nici linie, nici punte (comportamentul de azi):
                // se înregistrează ca nepostat, altfel acumularea evaluată l-ar
                // raporta pe conturile SURSEI, iar stocul lui n-ar fi măsurat deloc.
                bucla.Divergenta($"{View}/{h.Id}",
                    "RDC: rând de cost cu lot/produs nerezolvabil — marfa nu revine în stoc",
                    nomRef == null ? null
                        : [new EfectStoc(nomRef.Id,
                            index.Ia(r.Linie, Subconto.Credit, Subconto.Depozite)?.Id ?? h.DepozitId ?? "",
                            -Math.Abs(r.CantitateCredit), r.Suma)],
                    cat.Mapeaza(r.ContDebit), simbolCredit, r.Suma);
                plan.Punte.ActualEvaluat(cat.Mapeaza(r.ContDebit), simbolCredit, r.Suma);
                continue;
            }
            // Depozitul rândului bate antetul (marfa revine unde spune sursa).
            var depozitHex = index.Ia(r.Linie, Subconto.Credit, Subconto.Depozite)?.Id ?? h.DepozitId ?? "";
            if (cat.Gestiuni.TryGetValue(depozitHex, out var gestiuneRand) && gestiuneRand != plan.GestiuneId) {
                if (plan.Costuri.Count > 0)
                    throw new InvalidOperationException(
                        $"{context}: returul atinge mai multe gestiuni — nesuportat (sursa n-are cazuri).");
                plan.GestiuneId = gestiuneRand;
            }

            var cantitate = Math.Abs(r.CantitateCredit);

            // Marfa REVINE pe lotul original — dar în 1C returul își creează
            // adesea PROPRIUL lot (retur-ca-lot: 5 din 180 de rânduri pe ianuarie
            // au ca lot documentul de retur însuși), iar uneori produsul n-are
            // niciun lot în Atlas (achiziția e dinaintea ferestrei importate, iar
            // deschiderea n-a creat lot fiindcă stocul era zero — 17 documente pe
            // ianuarie). Atlas refuză explicit lotul creat de linia proprie (46e),
            // deci importul creează LOTUL SURSEI ca lot de sine stătător, cu
            // prețul din rândul 1C: aceeași rețetă ca pozițiile deschiderii (47d),
            // iar identitatea din sursă se păstrează — pin-urile ulterioare
            // (vânzarea mărfii returnate) o regăsesc prin index.
            var lotId = lot?.Id;
            // Cheia lotului creat de retur poartă sufixul „#retur": lotul-sursă la
            // care trimite subconto-ul poate fi unul pe care un ALT document al
            // lunii îl va crea mai târziu (o asamblare, de pildă — retururile se
            // importă înaintea transformărilor), iar cheia canonică e a aceluia.
            // Fără sufix, cele două s-ar bate pe aceeași cheie de legătură;
            // identitatea separată e oricum fidelă sursei (1C ține returul pe lot
            // propriu), iar contractul de stoc e per produs × gestiune (§8.3).
            // Sufixul stă pe segmentul documentului, NU la coada cheii: ultimul
            // segment e SIMBOLUL contului (convenția 47d, din care indexul citește
            // registrul de stoc al lotului) — un „#retur" acolo l-ar face de
            // nerecunoscut.
            // Cheia lotului CREAT de retur se ancorează pe RETURUL însuși (document
            // × rând), nu pe lotul-sursă: două retururi diferite pot readuce marfa
            // aceluiași lot original, iar două rânduri ale aceluiași retur pot
            // returna aceeași poziție în tranșe — ambele s-ar bate pe o cheie
            // ancorată în sursă. Ancorarea pe retur e și fidelă lui 1C, care ține
            // exact așa lotul de retur (retur-ca-lot, unde `lotRef.Id == h.Id`).
            // Ultimul segment rămâne SIMBOLUL contului (convenția 47d, din care
            // indexul citește registrul de stoc al lotului) — și anume simbolul
            // TIPULUI rezolvat, nu contul rândului 1C: lotul creat aici primește
            // produsul geamăn al acelui Tip, iar cele două trebuie să spună
            // același lucru (altfel un pin ulterior ar cădea pe celălalt geamăn).
            var cheieLot = Catalog.CheieLot(cat.TipRef(View), $"{h.Id}#retur{r.Linie}",
                nomRef?.Id ?? produsId.ToString(), tip.Cod);
            if (lotId != null && EsteLotPropriu(lotRef, h.Id))
                lotId = null;
            // Aliasul identității 1C a lotului-sursă → lotul creat de retur
            // (mecanismul `1C:LotAlias`, folosit deja pentru reclasificările BTR).
            // Fără el, orice mișcare 1C ulterioară pin-uită pe lotul ORIGINAL —
            // vânzarea mărfii tocmai returnate — n-ar rezolva niciodată și ar cădea
            // în supapa FIFO, cu costul altui lot. Se scrie DOAR când originalul
            // lipsește (când există, comportamentul rămâne cel de azi: se creditează
            // lotul original și nu se creează nimic).
            string cheieAlias = null;
            if (lotId == null) {
                LoturiCreate++;
                if (cantitate == 0)
                    throw new InvalidOperationException(
                        $"{context}: marfa returnată n-are lot original și nici cantitate — "
                        + "nu se poate crea lotul de retur.");
                cheieAlias = Catalog.CheieLot(lotRef.TipRef, lotRef.Id,
                    nomRef?.Id ?? produsId.ToString(), simbolCredit);
            }
            plan.Costuri.Add(new LinieCost(tip.Id, cantitate, lotId, produsId, cheieLot,
                cantitate == 0 ? 0m : Math.Abs(r.Suma) / cantitate, cheieAlias));
            LiniiCost++;
            // Ce postează motorul: costul care revine, cu semn storno (46e). Pe lot
            // EXISTENT e prețul lotului Atlas (poate diferi de cifra sursei — netarea
            // deschiderii); pe lot recreat din cheia sursei e exact cifra sursei,
            // deci declarația se anulează singură. Perechea de conturi vine din
            // regula RDC a Tipului, nu din rândul 1C: dacă lotul a fost
            // reclasificat, Atlas postează pe geamănul lui, iar diferența trebuie
            // să se vadă pe conturile amândurora.
            if (cat.Contare("RDC", tip.Id) is { } contareRdc)
                plan.Punte.ActualEvaluat(contareRdc.Debit, contareRdc.Credit,
                    lotId is { } existent
                        ? -Scara.RotunjesteBani(cantitate * HandlerTransfer.PretLot(os, existent))
                        : r.Suma);
        }

        ConstruiestePunte(cat, plan, h.Id, randuri);
        return plan;
    }

    static bool EsteLotPropriu(FlaxRef lotRef, string docId) =>
        lotRef != null && string.Equals(lotRef.Id, docId, StringComparison.Ordinal);

    static void ConstruiestePunte(Catalog cat, Plan plan, string docId,
            IReadOnlyList<FlaxRandNota> randuri) {
        var acoperitori = plan.Venituri.Count > 0
            ? new HashSet<string>(StringComparer.Ordinal) { "venit" }
            : Clasificare1C.Niciunul;

        Clasificare1C.Declara(plan.Punte, cat, View, docId, randuri, acoperitori,
            (rand, debit, credit) => {
            if (debit == null || credit == null)
                return null;
            // Costul care revine: Atlas îl postează la prețul lotului lui.
            if (debit.StartsWith('6') && cat.EsteContDeStoc(credit))
                return Rand.Evaluat;
            // Venitul stornat + TVA-ul colectat stornat.
            if (debit.StartsWith("411"))
                return Rand.Acoperit("venit",
                    "RDC: venit/TVA stornat fără secțiune de venit, transcris");
            // Sconturile acordate, stornate odată cu returul: n-au linie proprie
            // pe un retur Atlas (nici venit, nici marfă).
            if (debit.StartsWith("667"))
                return Rand.Punte("RDC: sconto acordat, stornat pe retur (667 = 411)");
            return null;
        });
        // Semnul storno: Atlas postează 4111 = 70x și 4111 = 4427 cu MINUS.
        Venituri1C.DeclaraInPunte(plan.Punte, plan.Venituri, semn: -1);
    }

    static Document Materializeaza(IObjectSpace os, Catalog cat, Plan plan) {
        if (plan == null || (plan.Venituri.Count == 0 && plan.Costuri.Count == 0))
            return null;
        var rdc = os.CreateObject<ReturClient>();
        rdc.Data = plan.Data;
        rdc.Numar = plan.Numar;
        rdc.PredatorId = plan.PartenerId;
        rdc.PrimitorId = plan.GestiuneId;
        foreach (var v in plan.Venituri) {
            var d = os.CreateObject<DocumentDetaliu>();
            d.Document = rdc;
            d.TipMaterialId = v.Tip.Id;
            d.Cantitate = 1m;
            d.Valoare = v.Net;
            d.TipTvaId = v.TipTvaId;
            d.ValoareTva = v.Tva;
        }
        var gestiune = os.GetObjectByKey<Gestiune>(plan.GestiuneId);
        foreach (var c in plan.Costuri) {
            var d = os.CreateObject<DocumentDetaliu>();
            d.Document = rdc;
            d.TipMaterialId = c.TipMaterialId;
            d.Cantitate = c.Cantitate;
            if (c.LotId is { } existent) {
                d.LotId = existent;
                continue;
            }
            // Lotul original al mărfii returnate, creat de import (nu de linie —
            // `LinieIntrareId` rămâne null, deci validarea RDC îl acceptă ca lot
            // existent, iar motorul nu-i mai rescrie prețul). Prețul e cel al
            // sursei, deci costul care revine iese EXACT ca în 1C.
            var lot = os.CreateObject<Lot>();
            lot.ProdusId = c.ProdusId;
            lot.GestiuneId = plan.GestiuneId;
            lot.PretUnitar = c.Pret;
            lot.Data = plan.Data;
            d.Lot = lot;
            cat.LeagaLotNou(os, c.CheieLot, lot.ID);
            // …și identitatea 1C a lotului original trimite tot aici (aceeași
            // tranzacție cu lotul), ca pin-urile ulterioare să nu rămână orfane.
            if (c.CheieAlias != null && cat.LeagaAliasLot(os, c.CheieAlias, lot.ID))
                AliasuriLot++;
        }
        return rdc;
    }

    public static void Raporteaza() =>
        Console.WriteLine($"  RDC: {LiniiVenit} linii de venit stornat, {LiniiCost} linii de cost pe lot "
            + $"({LoturiCreate} pe lot creat din cheia sursei — retur-ca-lot / achiziție dinaintea ferestrei, "
            + $"{AliasuriLot} cu alias de la identitatea lotului original), "
            + $"{RanduriNerezolvate} rânduri nerezolvate, {NepostateSarite} antete fără rânduri.");
}
