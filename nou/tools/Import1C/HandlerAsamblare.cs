using Atlas.Conta.BackOffice.Module.BusinessObjects;
using DevExpress.ExpressApp;

namespace Import1C;

// PASUL 4, tipul 7: Asamblare / Dezasamblare → ASM (tipul nou din felia 1C-a,
// decizia 46d): kitting n → m pe stoc, într-o gestiune, cu invariantul
// Σ valori produse = Σ valori consumuri.
//
// **Se construiește din RÂNDURILE DE NOTĂ, nu din secțiuni** — și asta rezolvă
// singură inversarea de direcție dintre cele două view-uri (Articole = produsul la
// asamblare, consumul la dezasamblare): pe rândul contabil 371 = 371 latura
// DEBIT poartă întotdeauna lotul NOU, iar latura CREDIT lotul consumat (verificat
// pe subconto-ul lui 2025). Secțiunile n-au deloc loturi, deci n-ar fi putut
// spune ce se consumă; direcția devine o consecință a datelor, nu o convenție de
// citire.
//
// Cantitatea produsului stă pe PRIMUL rând al grupului (1C o scrie o singură dată,
// restul rândurilor au CountDt = 0 și doar repartizează valoarea) — agregarea pe
// lotul de debit o adună corect fără să știe asta.
//
// **Valorile**: consumurile se evaluează la prețul lotului ATLAS (așa lucrează
// motorul), care după netarea deschiderii (47d) diferă de cel al sursei. Dacă am
// prelua valorile produselor din 1C, invariantul ar pica pe diferența de evaluare.
// De aceea valorile produselor se SCALEAZĂ: pro-rata din valorile 1C, aduse la
// suma consumurilor Atlas. Cheia de distribuție rămâne a sursei, mărimea e a
// noastră — diferența față de 1C e diferența justificată a netării (§8.3).
static class HandlerAsamblare {
    public static readonly HandlerTip HandlerAsm =
        new("Asamblare", "Asamblare (kitting n→m)", ctx => Importa(ctx, "Asamblare"));

    public static readonly HandlerTip HandlerDez =
        new("Dezasamblare", "Dezasamblare (același tip ASM)", ctx => Importa(ctx, "Dezasamblare"));

    public static int Documente { get; private set; }
    public static int LiniiConsum { get; private set; }
    public static int LiniiProdus { get; private set; }
    public static int TransferuriProduse { get; private set; }
    public static int RanduriNerezolvate { get; private set; }
    public static decimal AbatereScalare { get; private set; }

    sealed record Consum(Guid LotId, Guid TipMaterialId, decimal Cantitate);

    sealed record Produs1C(string NomenclatorId, string Simbol, string CheieLot, Guid ProdusId,
        TipInfo Tip, decimal Cantitate, decimal Valoare1C) {
        public decimal ValoareAtlas { get; set; }
    }

    sealed class Plan {
        public DateOnly Data;
        public string Numar;
        public Guid GestiuneConsum;
        public Guid GestiuneProduse;
        public string DepozitProduseHex;
        public List<Consum> Consumuri = [];
        public List<Produs1C> Produse = [];
        public Punte Punte;

        public bool AreDocument => Consumuri.Count > 0 && Produse.Count > 0;
        // Produsele se nasc în gestiunea în care se lucrează (regula ASM: lotul
        // produs aparține predatorului); când sursa le pune în ALT depozit, mutarea
        // se face după aceea, cu un transfer — stocul ajunge unde spune 1C.
        public bool CereTransfer => GestiuneProduse != Guid.Empty && GestiuneProduse != GestiuneConsum;
    }

    static void Importa(ContextLuna ctx, string view) {
        var bucla = ctx.Bucla;
        var antete = view == "Asamblare"
            ? bucla.Flax.Asamblari(ctx.An, ctx.Luna)
            : bucla.Flax.Dezasamblari(ctx.An, ctx.Luna);

        foreach (var h in antete)
            ctx.Planifica(h.Data, h.Numar, () => {
                var randuri = bucla.RanduriLuna.GetValueOrDefault(h.Id) ?? [];
                if (randuri.Count == 0)
                    return;
                Plan plan = null;
                if (!bucla.EsteCunoscut(view, h.Id)) {
                    try {
                        plan = Planifica(ctx, view, h, randuri);
                    }
                    catch (Exception ex) {
                        bucla.EsecPlanificare(view, h.Id, ex);
                        return;
                    }
                    Punti.Scrie(bucla, view, plan.AreDocument ? h.Id + "#punte" : h.Id,
                        h.Numar, plan.Data, plan.Punte, bucla.ContorPunti, bucla.Avert);
                    if (!plan.AreDocument && !plan.Punte.AreCeva)
                        bucla.NumaraSursaFaraCorespondent();
                }
                if (plan == null || plan.AreDocument) {
                    bucla.ImportaDocument(view, h.Id, os => Materializeaza(os, bucla.Catalog, plan));
                    // Transferul produselor în depozitul lor: document propriu, DUPĂ
                    // asamblare (loturile există abia acum — se caută în index pe cheia
                    // lor 1C, exact ca orice pin ulterior). Rămâne în ACEEAȘI unitate:
                    // ordinea asamblare → transfer e internă, nu cronologică.
                    if (plan == null || plan.CereTransfer)
                        bucla.ImportaDocument(view, h.Id + "#btr",
                            os => TransferaProduse(os, bucla.Catalog, plan, h.Numar));
                }
            });
    }

    static Plan Planifica(ContextLuna ctx, string view, FlaxAsamblare h,
            IReadOnlyList<FlaxRandNota> randuri) {
        var bucla = ctx.Bucla;
        var cat = bucla.Catalog;
        var plan = new Plan {
            Data = DateOnly.FromDateTime(h.Data),
            Numar = h.Numar,
            Punte = new Punte(),
        };
        var index = Subconto.Indexeaza(bucla.SubcontoLuna.GetValueOrDefault(h.Id) ?? []);
        var tipRef = cat.TipRef(view);

        // Latura CREDIT = consumurile (loturi existente); latura DEBIT = produsele
        // (loturi noi, create de documentul curent).
        var consumuri = new Dictionary<string, (FlaxRef Lot, FlaxRef Nom, string Simbol, decimal Cantitate)>(StringComparer.Ordinal);
        var produse = new Dictionary<string, (FlaxRef Nom, string Simbol, decimal Cantitate, decimal Valoare)>(StringComparer.Ordinal);

        foreach (var r in randuri) {
            var simbolDebit = cat.Mapeaza(r.ContDebit);
            var simbolCredit = cat.Mapeaza(r.ContCredit);
            var context = $"1C:{view}/{h.Id} rândul {r.Linie}";
            if (!cat.EsteContDeStoc(simbolDebit) || !cat.EsteContDeStoc(simbolCredit))
                throw new InvalidOperationException(
                    $"{context}: asamblarea mișcă doar conturi de stoc, dar sursa are "
                    + $"{r.ContDebit} = {r.ContCredit} — formă nouă, de tranșat explicit.");

            var lotCredit = index.Ia(r.Linie, Subconto.Credit, Subconto.Loturi);
            var nomCredit = index.Ia(r.Linie, Subconto.Credit, Subconto.Nomenclator);
            var lotDebit = index.Ia(r.Linie, Subconto.Debit, Subconto.Loturi);
            var nomDebit = index.Ia(r.Linie, Subconto.Debit, Subconto.Nomenclator);
            if (lotCredit == null || nomCredit == null || lotDebit == null || nomDebit == null)
                throw new InvalidOperationException(
                    $"{context}: rândul de asamblare n-are subconto de lot/nomenclator pe ambele laturi.");

            var cheieConsum = $"{lotCredit.TipRef}:{lotCredit.Id}:{nomCredit.Id}:{simbolCredit}";
            var (lc, nc, sc, qc) = consumuri.GetValueOrDefault(cheieConsum);
            consumuri[cheieConsum] = (lotCredit, nomCredit, simbolCredit, qc + Math.Abs(r.CantitateCredit));

            var cheieProdus = Catalog.CheieLot(tipRef, h.Id, nomDebit.Id, simbolDebit);
            var (np, sp, qp, vp) = produse.GetValueOrDefault(cheieProdus);
            produse[cheieProdus] = (nomDebit, simbolDebit,
                qp + Math.Abs(r.CantitateDebit), vp + Math.Abs(r.Suma));

            // Gestiunile: consumul de pe latura credit, produsul de pe latura debit.
            plan.GestiuneConsum = Gestiune(cat, index.Ia(r.Linie, Subconto.Credit, Subconto.Depozite)?.Id
                ?? h.DepozitSubasambleId, plan.GestiuneConsum, context, "consumului");
            var depozitProduseHex = index.Ia(r.Linie, Subconto.Debit, Subconto.Depozite)?.Id
                ?? h.DepozitArticoleId;
            plan.DepozitProduseHex = depozitProduseHex;
            plan.GestiuneProduse = Gestiune(cat, depozitProduseHex, plan.GestiuneProduse, context, "produsului");

            // Asamblarea nu contează în Atlas (marfă→marfă la sintetic e zgomot —
            // 46d), ceea ce e corect cât timp cele două conturi coincid: rândul se
            // anulează singur. O asamblare care RECLASIFICĂ (371 = 302) ar lăsa o
            // mișcare contabilă reală neacoperită și ar cere o punte la valoarea
            // Atlas (mecanica reclasificării de la transfer) — sursa n-are niciun
            // caz pe 2025, deci nu se scrie cod pe presupuneri: eșec zgomotos.
            if (simbolDebit != simbolCredit)
                throw new InvalidOperationException(
                    $"{context}: asamblare care reclasifică ({simbolDebit} = {simbolCredit}) — "
                    + "formă neacoperită (Atlas nu contează asamblarea), de tranșat explicit.");
        }

        // Consumurile: pin pe lotul sursei, prin supapa 48a (netarea poate să-l fi
        // golit). Un lot creat de ACEST document nu poate fi consumat de el
        // (motorul refuză — 46d), deci acolo pinul cade și se alocă FIFO.
        var dejaAlocat = new Dictionary<Guid, decimal>();
        var valoareConsum = 0m;
        using var os = bucla.CreeazaObjectSpace();
        foreach (var (cheie, c) in consumuri) {
            var context = $"1C:{view}/{h.Id} lotul {cheie}";
            var rezolvat = MiscareStoc1C.Rezolva(bucla, c.Lot, c.Nom, c.Simbol, context);
            if (rezolvat is not var (lot, tipCont, produsId)) {
                RanduriNerezolvate++;
                continue;
            }
            // Tipul liniei ȘI registrul de căutare vin din PRODUS (vezi Catalog):
            // altfel alocarea verifică soldul într-un registru și motorul scrie
            // mișcarea în celălalt — prima rulare a picat exact aici.
            var tip = cat.TipAlProdusului(os, produsId, tipCont);
            var pin = lot != null && !string.Equals(c.Lot.Id, h.Id, StringComparison.Ordinal)
                ? lot.Id : (Guid?)null;
            var (alocari, ramas) = bucla.Alocare.Aloca(os, pin, produsId, plan.GestiuneConsum,
                tip.Registru, plan.Data, c.Cantitate, dejaAlocat);
            foreach (var (lotId, q) in alocari) {
                plan.Consumuri.Add(new Consum(lotId, tip.Id, q));
                // Rotunjit PER LINIE, exact ca `Asamblare.PregatesteOperare`:
                // `valoareConsum` e o PREDICȚIE a ceea ce va calcula motorul, iar
                // `Scaleaza` o distribuie pe produse ca invariantul (|Σ produse −
                // Σ consumuri| ≤ 0,005) să treacă exact. De când valorile de
                // postare au scară fixă (`Scara`, bani), motorul scrie
                // `round(q × preț, 2)` pe fiecare linie — o sumă neroturnjită aici
                // se abate de la el cu până la o jumătate de ban pe linie și pică
                // invariantul pe documentele cu mai multe consumuri.
                valoareConsum += Scara.RotunjesteBani(q * HandlerTransfer.PretLot(os, lotId));
                LiniiConsum++;
            }
            if (ramas > 0)
                bucla.Avert($"{context}: {ramas:N3} din {c.Cantitate:N3} n-au acoperire în gestiune — "
                    + "asamblarea consumă parțial (diferență de stoc raportată).");
        }

        foreach (var (cheie, p) in produse) {
            var tip = cat.TipStocPentru(p.Simbol)
                ?? throw new InvalidOperationException(
                    $"1C:{view}/{h.Id}: contul de stoc {p.Simbol} al produsului n-are TipMaterial în profil.");
            if (p.Cantitate <= 0)
                throw new InvalidOperationException(
                    $"1C:{view}/{h.Id}: produsul {p.Nom.Id} n-are cantitate în rândurile sursei.");
            var produsId = bucla.LaCerere.AsiguraProdus(p.Nom.Id, tip.Cod)
                ?? throw new InvalidOperationException(
                    $"1C:{view}/{h.Id}: nomenclatorul {p.Nom.Id} nu s-a putut importa pe Tipul {tip.Cod}.");
            plan.Produse.Add(new Produs1C(p.Nom.Id, p.Simbol, cheie, produsId, tip,
                p.Cantitate, p.Valoare));
            LiniiProdus++;
        }

        Scaleaza(plan, valoareConsum);
        if (plan.AreDocument)
            Documente++;
        return plan;
    }

    static Guid Gestiune(Catalog cat, string depozitHex, Guid deja, string context, string rol) {
        var id = cat.Gestiuni.TryGetValue(depozitHex ?? "", out var g)
            ? g
            : throw new InvalidOperationException(
                $"{context}: depozitul 1C {depozitHex} al {rol} nu e legat de o Gestiune.");
        if (deja != Guid.Empty && deja != id)
            throw new InvalidOperationException(
                $"{context}: asamblarea atinge mai multe gestiuni pe aceeași latură — nesuportat.");
        return id;
    }

    // Valorile produselor = cheia de distribuție a sursei × valoarea consumurilor
    // ATLAS. Restul de rotunjire cade pe ultima linie, ca invariantul motorului
    // (|Σ produse − Σ consumuri| ≤ 0,005) să treacă exact.
    static void Scaleaza(Plan plan, decimal valoareConsum) {
        var total1C = plan.Produse.Sum(p => p.Valoare1C);
        if (plan.Produse.Count == 0 || valoareConsum <= 0m)
            return;
        var repartizat = 0m;
        for (var i = 0; i < plan.Produse.Count; i++) {
            var valoare = i == plan.Produse.Count - 1 || total1C <= 0m
                ? valoareConsum - repartizat
                : Math.Round(valoareConsum * plan.Produse[i].Valoare1C / total1C, 2);
            repartizat += valoare;
            plan.Produse[i].ValoareAtlas = valoare;
        }
        AbatereScalare += Math.Abs(valoareConsum - total1C);
    }

    static Document Materializeaza(IObjectSpace os, Catalog cat, Plan plan) {
        if (plan == null || !plan.AreDocument)
            return null;
        var asm = os.CreateObject<Asamblare>();
        asm.Data = plan.Data;
        asm.Numar = plan.Numar;
        // Se lucrează în gestiunea din care ies consumurile; produsele se nasc
        // acolo (regula ASM) și pleacă mai departe cu transferul, dacă e cazul.
        asm.PredatorId = plan.GestiuneConsum;
        asm.PrimitorId = plan.GestiuneConsum;
        var gestiune = os.GetObjectByKey<Gestiune>(plan.GestiuneConsum);
        foreach (var c in plan.Consumuri) {
            var d = os.CreateObject<AsamblareDetaliu>();
            d.Document = asm;
            d.TipMaterialId = c.TipMaterialId;
            d.Directie = DirectieAsamblare.Consum;
            d.LotId = c.LotId;
            d.Cantitate = c.Cantitate;
        }
        foreach (var p in plan.Produse) {
            var d = os.CreateObject<AsamblareDetaliu>();
            d.Document = asm;
            d.TipMaterialId = p.Tip.Id;
            d.Directie = DirectieAsamblare.Produs;
            d.Cantitate = p.Cantitate;
            d.PretEvaluare = p.ValoareAtlas / p.Cantitate;
            var lot = d.CreeazaLot(os, os.GetObjectByKey<Produs>(p.ProdusId), gestiune);
            cat.LeagaLotNou(os, p.CheieLot, lot.ID);
        }
        return asm;
    }

    // Mutarea produselor în depozitul lor (când sursa îl are altul decât cel al
    // consumurilor): transfer normal, pe loturile abia create — se regăsesc în
    // index pe cheia lor 1C, ca orice pin.
    static Document TransferaProduse(IObjectSpace os, Catalog cat, Plan plan, string numar1C) {
        if (plan == null || !plan.CereTransfer || plan.Produse.Count == 0)
            return null;
        var btr = os.CreateObject<NotaTransfer>();
        btr.Data = plan.Data;
        btr.Numar = $"{numar1C}-T";
        btr.PredatorId = plan.GestiuneConsum;
        btr.PrimitorId = plan.GestiuneProduse;
        var linii = 0;
        foreach (var p in plan.Produse) {
            var parti = p.CheieLot.Split(':');
            var lot = cat.Lot(parti[0], parti[1], parti[2], parti[3]);
            if (lot == null)
                continue;
            var d = os.CreateObject<DocumentDetaliu>();
            d.Document = btr;
            d.TipMaterialId = p.Tip.Id;
            d.LotId = lot.Id;
            d.Cantitate = p.Cantitate;
            linii++;
        }
        if (linii == 0)
            return null;
        TransferuriProduse++;
        return btr;
    }

    public static void Raporteaza() =>
        Console.WriteLine($"  ASM: {Documente} asamblări/dezasamblări, {LiniiConsum} linii de consum, "
            + $"{LiniiProdus} linii de produs (valori scalate la consumul Atlas; abatere cumulată față "
            + $"de 1C {AbatereScalare:N2} lei), {TransferuriProduse} transferuri de produse în alt "
            + $"depozit, {RanduriNerezolvate} loturi de consum nerezolvate.");
}
