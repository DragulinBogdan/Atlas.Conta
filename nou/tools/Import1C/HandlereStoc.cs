using Atlas.Conta.BackOffice.Module.BusinessObjects;
using DevExpress.ExpressApp;

namespace Import1C;

// PASUL 3, tipurile 2–4: mișcările de stoc fără partener — transferul, consumul
// și diferențele de inventar.
//
// Pentru toate trei, liniile se construiesc din **rândurile registrului contabil
// 1C + subconto**, nu din secțiunile tabulare ale documentului. Motivul e al
// datelor: secțiunile n-au lotul (1C îl ține exclusiv ca subconto), iar
// granularitatea diferă — pe ianuarie transferurile au 6.890 de linii de secțiune
// și 7.089 de rânduri de notă, adică o linie se sparge pe mai multe loturi.
// Rândul de notă poartă totul într-un singur loc (cont, sumă, cantitate, iar prin
// subconto lotul, depozitul și nomenclatorul), deci nu e nevoie de nicio potrivire
// pozițională între cititori — capcana §4.3 a inventarului.
//
// Verificat pe 2025: NICIUN rând de transfer sau de consum n-are cantitate ≤ 0,
// sumă negativă sau subconto de lot lipsă pe latura creditoare.
static class Subconto {
    public const string Loturi = "Loturi";
    public const string Depozite = "Depozite";
    public const string Nomenclator = "Nomenclator";

    // Correspond: 0 = latura DEBIT, 1 = latura CREDIT (verificat pe date).
    public const int Debit = 0;
    public const int Credit = 1;

    public static Dictionary<(int Linie, int Latura), Dictionary<string, FlaxRef>> Indexeaza(
            IEnumerable<FlaxSubcontoNota> randuri) {
        var index = new Dictionary<(int, int), Dictionary<string, FlaxRef>>();
        foreach (var s in randuri) {
            if (s.Valoare == null)
                continue;
            if (!index.TryGetValue((s.Linie, s.Correspond), out var fel))
                index[(s.Linie, s.Correspond)] = fel = new Dictionary<string, FlaxRef>(StringComparer.Ordinal);
            fel[s.Fel] = s.Valoare;
        }
        return index;
    }

    public static FlaxRef Ia(this Dictionary<(int, int), Dictionary<string, FlaxRef>> index,
            int linie, int latura, string fel) =>
        index.TryGetValue((linie, latura), out var f) && f.TryGetValue(fel, out var v) ? v : null;

    // ---- Indexul COMPLET, per latură (pasul 5: trezoreria și notele) ----
    // Forma de mai sus ține un singur analitic per FEL, ceea ce ajunge pentru
    // stoc (lot × nomenclator × depozit sunt feluri distincte). Trezoreria are
    // nevoie de întreaga listă a laturii: partenerul și documentul stins stau pe
    // ACEEAȘI latură, iar felul partenerului diferă („Parteneri" vs „Angajați
    // încadrați") — se caută după TIPUL referinței, nu după eticheta felului,
    // fiindcă eticheta e text 1C cu diacritice, iar tipul vine din coloana
    // tipizată a view-ului (contractul de coloane, §2).
    public const string FelDocumente = "Documente";
    public const string TipPartener = "Partenerii";
    public const string TipPersoana = "PersoaneFizice";

    public static Dictionary<(int Linie, int Latura), List<FlaxSubcontoNota>> IndexeazaTot(
            IEnumerable<FlaxSubcontoNota> randuri) {
        var index = new Dictionary<(int, int), List<FlaxSubcontoNota>>();
        foreach (var s in randuri) {
            if (!index.TryGetValue((s.Linie, s.Correspond), out var lista))
                index[(s.Linie, s.Correspond)] = lista = [];
            lista.Add(s);
        }
        return index;
    }

    public static List<FlaxSubcontoNota> Latura(
            this Dictionary<(int Linie, int Latura), List<FlaxSubcontoNota>> index, int linie, int latura) =>
        index.TryGetValue((linie, latura), out var lista) ? lista : [];

    public static FlaxRef DeTip(this List<FlaxSubcontoNota> latura, params string[] tipuri) =>
        latura.FirstOrDefault(s => s.Valoare?.Tip != null && tipuri.Contains(s.Valoare.Tip))?.Valoare;

    public static FlaxRef DeFel(this List<FlaxSubcontoNota> latura, string fel) =>
        latura.FirstOrDefault(s => s.Fel == fel && s.Valoare != null)?.Valoare;
}

// O linie de ieșire/intrare pe lot, gata de materializat.
sealed record LiniePeLot(Guid LotId, Guid TipMaterialId, decimal Cantitate);

// Rezolvarea unui rând de notă 1C care mișcă stoc: lotul Atlas, Tipul care îi dă
// registrul, produsul și cantitatea. Aici trăiește regula care ține importul
// coerent: **Tipul liniei vine din simbolul de NAȘTERE al lotului, nu din contul
// rândului 1C** — contul rândului rămâne sursa DOAR unde nu există lot (intrări,
// plusuri, produse de asamblare, care își nasc lotul pe linia proprie).
//
// Două motive, amândouă tari. Registrul de stoc în care stă soldul a fost fixat
// la deschidere din simbolul lotului (47d); dacă 1C a reclasificat lotul între
// timp (BTR cu NewContEvidenta), contul rândului ar trimite ieșirea în altă cutie
// decât cea în care e marfa, iar gardianul de sold ar refuza-o pe bună dreptate.
// Și, de la amendamentul „produs = nomenclator × cont" (`ImportLaCerere`),
// simbolul alege și PRODUSUL geamăn: un Tip luat din contul rândului ar cere
// produsul altui geamăn decât cel al lotului, iar motorul refuză linia explicit
// („Lotul liniei aparține unui produs cu alt Tip decât Tipul liniei" — validare
// de model pe ASM/DSC/RLF/RDC). Divergența de cont rămâne o diferență de FORMĂ
// și se transcrie în puntea NTC.
//
// Consecința pentru apelanți: `Tip` de aici E Tipul produsului întors alături —
// nu mai există niciun fallback „Tipul produsului" de aplicat peste el.
static class MiscareStoc1C {
    public static (LotImport Lot, TipInfo Tip, Guid ProdusId)? Rezolva(BuclaImport bucla,
            FlaxRef lotRef, FlaxRef nomRef, string simbolCont1C, string context) {
        var cat = bucla.Catalog;
        if (lotRef == null || nomRef == null) {
            bucla.Avert($"{context}: rând de stoc fără subconto de lot/nomenclator — sărit.");
            return null;
        }
        var lot = cat.Lot(lotRef.TipRef, lotRef.Id, nomRef.Id, simbolCont1C);
        var simbol = lot?.Simbol ?? simbolCont1C;
        var tip = cat.TipStocPentru(simbol);
        if (tip == null)
            return null;
        var produsId = bucla.LaCerere.AsiguraProdus(nomRef.Id, tip.Cod);
        return produsId == null ? null : (lot, tip, produsId.Value);
    }
}

// ======================= 2. TransferDeMarfuri → BTR =======================
static class HandlerTransfer {
    public const string View = "TransferDeMarfuri";

    public static readonly HandlerTip Handler = new(View, "Notă de transfer", Importa);

    public static int Reclasificari { get; private set; }
    public static int AliasuriLot { get; private set; }
    public static int DocumenteAceeasiGestiune { get; private set; }
    public static int RanduriNerezolvate { get; private set; }

    sealed class Plan {
        public Guid PredatorId;
        public Guid PrimitorId;
        public DateOnly Data;
        public List<LiniePeLot> Linii = [];
        public Punte Punte;
        public bool FaraDocument;

        public bool AreDocument => !FaraDocument && Linii.Count > 0;
    }

    static void Importa(ContextLuna ctx) {
        var bucla = ctx.Bucla;
        foreach (var h in bucla.Flax.Transferuri(ctx.An, ctx.Luna))
            ctx.Planifica(h.Data, h.Numar, () => {
                Plan plan = null;
                if (!bucla.EsteCunoscut(View, h.Id)) {
                    try {
                        plan = Planifica(ctx, h);
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
                    bucla.ImportaDocument(View, h.Id, os => Materializeaza(os, plan),
                        motivFaraDraft: Motive.FaraPlan(plan,
                            "transferul n-a rămas cu nicio linie de stoc"));
            });
    }

    static Plan Planifica(ContextLuna ctx, FlaxTransfer h) {
        var bucla = ctx.Bucla;
        var cat = bucla.Catalog;
        var plan = new Plan {
            Data = DateOnly.FromDateTime(h.Data),
            Punte = new Punte(),
        };
        plan.PredatorId = Gestiune(cat, h.DepozitExpeditorId, "expeditorul transferului");
        plan.PrimitorId = Gestiune(cat, h.DepozitDestinatarId, "destinatarul transferului");
        // 169 de transferuri pe an au aceeași gestiune pe ambele laturi: sunt
        // pure reclasificări de cont, iar motorul refuză (pe drept) un transfer
        // în aceeași gestiune. Stocul nu se mișcă nicăieri; rămâne puntea.
        plan.FaraDocument = plan.PredatorId == plan.PrimitorId;
        if (plan.FaraDocument)
            DocumenteAceeasiGestiune++;

        var index = Subconto.Indexeaza(bucla.SubcontoLuna.GetValueOrDefault(h.Id) ?? []);
        var dejaAlocat = new Dictionary<Guid, decimal>();
        using var os = bucla.CreeazaObjectSpace();

        foreach (var r in bucla.RanduriLuna.GetValueOrDefault(h.Id) ?? []) {
            var simbolCredit = cat.Mapeaza(r.ContCredit);
            var simbolDebit = cat.Mapeaza(r.ContDebit);
            var context = $"1C:{View}/{h.Id} rândul {r.Linie}";
            var nomRef = index.Ia(r.Linie, Subconto.Credit, Subconto.Nomenclator);
            var rezolvat = MiscareStoc1C.Rezolva(bucla,
                index.Ia(r.Linie, Subconto.Credit, Subconto.Loturi), nomRef, simbolCredit, context);
            if (rezolvat is not var (lot, tip, produsId)) {
                // Rând pe care nu-l putem duce nici în stoc, nici în contabilitate:
                // se măsoară, altfel ar fi singura mișcare a sursei complet
                // invizibilă în ambele contracte.
                RanduriNerezolvate++;
                bucla.Divergenta($"{View}/{h.Id}", "BTR: rând cu lot/produs nerezolvabil — netransferat",
                    nomRef == null ? null : [
                        new EfectStoc(nomRef.Id, h.DepozitExpeditorId, r.CantitateCredit, r.Suma),
                        new EfectStoc(nomRef.Id, h.DepozitDestinatarId, -r.CantitateCredit, -r.Suma),
                    ],
                    simbolDebit, simbolCredit, simbolDebit == simbolCredit ? 0m : r.Suma);
                continue;
            }

            decimal valoareAtlas = 0m;
            // Partea pe care Atlas n-o mișcă deloc: e înregistrată separat mai jos
            // ca nepostată, deci trebuie scoasă din ținta evaluată — altfel ar fi
            // explicată de două ori.
            var valoareNeacoperita = 0m;
            if (!plan.FaraDocument) {
                var (alocari, ramas) = bucla.Alocare.Aloca(os, lot?.Id, produsId, plan.PredatorId,
                    tip.Registru, plan.Data, r.CantitateCredit, dejaAlocat);
                foreach (var (lotId, cantitate) in alocari) {
                    plan.Linii.Add(new LiniePeLot(lotId, tip.Id, cantitate));
                    valoareAtlas += cantitate * PretLot(os, lotId);
                }
                if (ramas > 0) {
                    bucla.Avert($"{context}: {ramas:N3} din {r.CantitateCredit:N3} n-au acoperire "
                        + "în gestiunea expeditoare — linia se transferă parțial.");
                    // Măsurătoarea, pe DOUĂ chei: marfa netransferată rămâne la
                    // expeditor ȘI lipsește de la destinatar. Contabil, un transfer
                    // nu postează nimic — mai puțin reclasificarea de mai jos, a
                    // cărei punte transcrie doar partea transferată, deci restul e
                    // exact ce nu se postează (când conturile coincid se anulează
                    // singur în agregarea contractului).
                    var valoareRamas = valoareNeacoperita = r.CantitateCredit == 0m
                        ? r.Suma : r.Suma * ramas / r.CantitateCredit;
                    bucla.Divergenta($"{View}/{h.Id}",
                        "BTR: linie transferată parțial (lipsă acoperire la expeditor)",
                        nomRef == null ? null : [
                            new EfectStoc(nomRef.Id, h.DepozitExpeditorId, ramas, valoareRamas),
                            new EfectStoc(nomRef.Id, h.DepozitDestinatarId, -ramas, -valoareRamas),
                        ],
                        simbolDebit, simbolCredit, simbolDebit == simbolCredit ? 0m : valoareRamas);
                }
            }
            else if (lot != null)
                valoareAtlas = r.CantitateCredit * PretLot(os, lot.Id);

            // Diferența de EVALUARE a reclasificării, măsurată ca peste tot
            // (Punte.cs): sursa mută lotul de pe un cont de stoc pe altul la
            // valoarea EI, puntea de mai jos îl mută la a NOASTRĂ. Fără declarația
            // asta diferența rămâne agățată pe cele două conturi de stoc, iar
            // plafonul măsurat n-o mai acoperă când reclasificările se îndesesc:
            // măsurat pe rularea integrală, contul 3028 rămânea cu 1.013,65 lei
            // neexplicați la decembrie — singurul cont picat din tot contractul 1.
            // Când cele două conturi coincid declarația se anulează singură (aceeași
            // sumă pe aceeași latură), deci se face necondiționat.
            var sursaAcoperita = r.Suma - valoareNeacoperita;
            plan.Punte.TintaEvaluata(simbolDebit, simbolCredit, sursaAcoperita);
            plan.Punte.ActualEvaluat(simbolDebit, simbolCredit, valoareAtlas);

            // **1C ține un cost PER DEPOZIT pentru ACELAȘI lot; Atlas ține unul
            // singur** (identificare specifică — decizia 13). Transferul e locul
            // unde se vede: sursa scoate din expeditor și pune în destinatar o
            // sumă care NU e cantitatea × prețul lotului, ci costul ei per depozit,
            // iar totalul lotului rămâne exact. Măsurat pe factura în EUR
            // FA/EU-25 00072678: 111 bucăți intrate cu 6.527,14 lei (58,803/buc),
            // pe care 1C le ține apoi ca 100 × 58,7648 + 11 × 59,1509 = 6.527,14 —
            // aceeași sumă, alt cost pe depozit. Cele 2 bucăți ajunse în MAGAZIN
            // valorează 118,30 la sursă și 117,61 la noi: exact abaterea de −0,69.
            //
            // Nu e rotunjire de conversie valutară (acolo secțiunea și rândul de
            // notă dau aceeași cifră — verificat pe factura asta), și nu e ceva de
            // reparat: e diferența dintre două modele de evaluare. Se măsoară pe
            // AMBELE chei, cu semne opuse — valoarea doar se rearanjează între
            // gestiuni, nu se creează și nu se pierde.
            if (!plan.FaraDocument && nomRef != null
                    && Math.Abs(valoareAtlas - sursaAcoperita) >= 0.005m) {
                var delta = valoareAtlas - sursaAcoperita;
                bucla.Divergenta($"{View}/{h.Id}",
                    "BTR: 1C mută lotul la alt cost unitar decât al lotului — "
                        + "valoarea se rearanjează între gestiuni",
                    [new EfectStoc(nomRef.Id, h.DepozitExpeditorId, 0m, -delta),
                        new EfectStoc(nomRef.Id, h.DepozitDestinatarId, 0m, delta)]);
            }

            // Reclasificarea (556 de rânduri pe an): 1C mută lotul pe alt cont
            // fără să-i schimbe identitatea. Atlas nu contează transferul deloc
            // (23c), deci rândul 1C se transcrie ca atare — la valoarea ATLAS a
            // mișcării, ca soldul contabil să rămână în pas cu registrul de stoc.
            if (simbolDebit != simbolCredit) {
                Reclasificari++;
                plan.Punte.Categoria("BTR: reclasificare de cont pe transfer")
                    .Tinta1C(simbolDebit, simbolCredit, valoareAtlas);
                if (lot != null && simbolDebit != null) {
                    var lotRef = index.Ia(r.Linie, Subconto.Credit, Subconto.Loturi);
                    if (cat.LeagaAliasLot(Catalog.CheieLot(lotRef.TipRef, lotRef.Id, nomRef.Id, simbolDebit), lot))
                        AliasuriLot++;
                }
            }
        }
        return plan;
    }

    static Guid Gestiune(Catalog cat, string depozitHex, string rol) =>
        cat.Gestiuni.TryGetValue(depozitHex ?? "", out var g)
            ? g
            : throw new InvalidOperationException($"Depozitul 1C {depozitHex} ({rol}) nu e legat de o Gestiune.");

    internal static decimal PretLot(IObjectSpace os, Guid lotId) =>
        os.GetObjectByKey<Lot>(lotId)?.PretUnitar ?? 0m;

    static Document Materializeaza(IObjectSpace os, Plan plan) {
        if (plan == null || !plan.AreDocument)
            return null;
        var btr = os.CreateObject<NotaTransfer>();
        btr.Data = plan.Data;
        btr.PredatorId = plan.PredatorId;
        btr.PrimitorId = plan.PrimitorId;
        foreach (var l in plan.Linii) {
            var d = os.CreateObject<DocumentDetaliu>();
            d.Document = btr;
            d.TipMaterialId = l.TipMaterialId;
            d.LotId = l.LotId;
            d.Cantitate = l.Cantitate;
        }
        return btr;
    }

    public static void Raporteaza() =>
        Console.WriteLine($"  BTR: {Reclasificari} rânduri de reclasificare de cont (punte + "
            + $"{AliasuriLot} aliasuri de lot), {DocumenteAceeasiGestiune} documente cu aceeași "
            + $"gestiune pe ambele laturi (doar punte), {RanduriNerezolvate} rânduri nerezolvate.");
}

// ======================= 3. BonDeConsum → BCS =======================
static class HandlerConsum {
    public const string View = "BonDeConsum";

    public static readonly HandlerTip Handler = new(View, "Bon de consum", Importa);

    public static int RanduriNeutre { get; private set; }
    public static int DocumenteSparte { get; private set; }
    public static int PuntiCheltuiala { get; private set; }
    public static int RanduriNerezolvate { get; private set; }

    sealed class Plan {
        public Guid PredatorId;
        public DateOnly Data;
        public List<LiniePeLot> Linii = [];
        public Punte Punte;

        public bool AreDocument => Linii.Count > 0;
    }

    static void Importa(ContextLuna ctx) {
        var bucla = ctx.Bucla;
        foreach (var h in bucla.Flax.BonuriConsum(ctx.An, ctx.Luna))
            ctx.Planifica(h.Data, h.Numar, () => {
                // Un bon de consum poate descărca din MAI MULTE gestiuni (5 din 542
                // pe an), iar documentul Atlas are o singură latură predatoare: se
                // sparge în câte un bon per gestiune. Cheia de idempotență poartă
                // gestiunea, deci reluarea rămâne exactă. Bonurile sparte rămân O
                // unitate: gruparea e a sursei, nu a ordinii.
                List<(string Depozit, Plan Plan)> peGestiune;
                try {
                    peGestiune = Grupeaza(ctx, h);
                }
                catch (Exception ex) {
                    bucla.EsecPlanificare(View, h.Id, ex);
                    return;
                }
                if (peGestiune.Count > 1)
                    DocumenteSparte++;
                foreach (var (depozitHex, plan) in peGestiune) {
                    var cheie = peGestiune.Count == 1 ? h.Id : $"{h.Id}@{depozitHex}";
                    if (plan != null)
                        Punti.Scrie(bucla, View, plan.AreDocument ? cheie + "#punte" : cheie,
                            h.Numar, plan.Data, plan.Punte, bucla.ContorPunti, bucla.Avert);
                    if (plan is { AreDocument: false } && !plan.Punte.AreCeva)
                        bucla.NumaraSursaFaraCorespondent();
                    if (plan == null || plan.AreDocument)
                        bucla.ImportaDocument(View, cheie, os => Materializeaza(os, bucla.Catalog, plan),
                            motivFaraDraft: Motive.FaraPlan(plan,
                                "bonul de consum n-a rămas cu nicio linie de stoc"));
                }
            });
    }

    // Gruparea pe gestiune se face pe SURSĂ, deci și când documentul e deja
    // importat (cheile trebuie să iasă identice) — planificarea propriu-zisă
    // (alocare + punte) se face doar pentru grupurile necunoscute.
    static List<(string Depozit, Plan Plan)> Grupeaza(ContextLuna ctx, FlaxBonConsum h) {
        var bucla = ctx.Bucla;
        var index = Subconto.Indexeaza(bucla.SubcontoLuna.GetValueOrDefault(h.Id) ?? []);
        var randuri = bucla.RanduriLuna.GetValueOrDefault(h.Id) ?? [];
        var peDepozit = new Dictionary<string, List<FlaxRandNota>>(StringComparer.Ordinal);
        foreach (var r in randuri) {
            var depozit = index.Ia(r.Linie, Subconto.Credit, Subconto.Depozite);
            var cheie = depozit?.Id ?? h.DepozitId ?? "";
            if (!peDepozit.TryGetValue(cheie, out var lista))
                peDepozit[cheie] = lista = [];
            lista.Add(r);
        }

        var rezultat = new List<(string, Plan)>();
        var unicul = peDepozit.Count == 1;
        foreach (var (depozitHex, lista) in peDepozit.OrderBy(x => x.Key, StringComparer.Ordinal)) {
            var cheie = unicul ? h.Id : $"{h.Id}@{depozitHex}";
            rezultat.Add((depozitHex, bucla.EsteCunoscut(View, cheie)
                ? null
                : Planifica(ctx, h, depozitHex, lista, index)));
        }
        return rezultat;
    }

    static Plan Planifica(ContextLuna ctx, FlaxBonConsum h, string depozitHex,
            List<FlaxRandNota> randuri, Dictionary<(int, int), Dictionary<string, FlaxRef>> index) {
        var bucla = ctx.Bucla;
        var cat = bucla.Catalog;
        var plan = new Plan {
            Data = DateOnly.FromDateTime(h.Data),
            Punte = new Punte(),
        };
        plan.PredatorId = cat.Gestiuni.TryGetValue(depozitHex, out var g)
            ? g
            : throw new InvalidOperationException(
                $"Depozitul 1C {depozitHex} al bonului de consum nu e legat de o Gestiune.");

        var dejaAlocat = new Dictionary<Guid, decimal>();
        using var os = bucla.CreeazaObjectSpace();
        foreach (var r in randuri) {
            var simbolCredit = cat.Mapeaza(r.ContCredit);
            var simbolDebit = cat.Mapeaza(r.ContDebit);
            var context = $"1C:{View}/{h.Id} rândul {r.Linie}";

            // Rândurile 302.8 = 302.8 pe ACELAȘI lot (3 pe tot anul): în sursă
            // n-au niciun efect — nici contabil, nici pe stoc. Se sar întregi.
            if (simbolDebit == simbolCredit
                    && index.Ia(r.Linie, Subconto.Debit, Subconto.Loturi) is { } lotDebit
                    && lotDebit.Id == index.Ia(r.Linie, Subconto.Credit, Subconto.Loturi)?.Id) {
                RanduriNeutre++;
                continue;
            }

            var nomRef = index.Ia(r.Linie, Subconto.Credit, Subconto.Nomenclator);
            var rezolvat = MiscareStoc1C.Rezolva(bucla,
                index.Ia(r.Linie, Subconto.Credit, Subconto.Loturi), nomRef, simbolCredit, context);
            if (rezolvat is not var (lot, tip, produsId)) {
                RanduriNerezolvate++;
                bucla.Divergenta($"{View}/{h.Id}", "BCS: rând cu lot/produs nerezolvabil — neconsumat",
                    nomRef == null ? null
                        : [new EfectStoc(nomRef.Id, depozitHex, r.CantitateCredit, r.Suma)],
                    simbolDebit, simbolCredit, r.Suma);
                continue;
            }

            var (alocari, ramas) = bucla.Alocare.Aloca(os, lot?.Id, produsId, plan.PredatorId,
                tip.Registru, plan.Data, r.CantitateCredit, dejaAlocat);
            var valoareAtlas = 0m;
            foreach (var (lotId, cantitate) in alocari) {
                plan.Linii.Add(new LiniePeLot(lotId, tip.Id, cantitate));
                // Rotunjirea e PER LINIE, ca la materializare: motorul postează un
                // rând per linie, rotunjit la bani. O sumă nerotunjită aici ar
                // declara în punte altceva decât postează motorul, iar reziduul de
                // sub-ban s-ar acumula pe contul de cheltuială.
                valoareAtlas += Scara.RotunjesteBani(cantitate * HandlerTransfer.PretLot(os, lotId));
            }
            // Diferența de EVALUARE a consumului, măsurată ca peste tot (Punte.cs):
            // sursa își descarcă marfa la costul ei, Atlas la costul lotului lui.
            // Fără declarația asta diferența rămânea agățată pe contul de cheltuială
            // al SURSEI (603, 6584, 6588…): puntea de mai jos mută pe perechea Atlas
            // valoarea NOASTRĂ, iar închiderea lunară a lui 1C stinge cifra LUI —
            // reziduul nu mai pleacă de acolo niciodată (măsurat: 603 rămânea cu
            // 23,97 lei din aprilie până la finele anului, singurul cont fără
            // explicație din tot contractul 1).
            var valoareNeacoperita = ramas <= 0 ? 0m
                : r.CantitateCredit == 0m ? r.Suma : r.Suma * ramas / r.CantitateCredit;
            plan.Punte.TintaEvaluata(simbolDebit, simbolCredit, r.Suma - valoareNeacoperita);
            plan.Punte.ActualEvaluat(simbolDebit, simbolCredit, valoareAtlas);
            if (ramas > 0) {
                bucla.Avert($"{context}: {ramas:N3} din {r.CantitateCredit:N3} n-au acoperire în "
                    + "gestiunea predatoare — consumul se descarcă parțial.");
                // Măsurătoarea: marfa neconsumată rămâne în gestiune, iar cheltuiala
                // ei nu se postează nicăieri (puntea de mai jos transcrie DOAR
                // partea consumată, la valoarea Atlas). Aceeași cifră intră și în
                // ținta evaluată de mai sus, ca partea neacoperită să fie explicată
                // O SINGURĂ dată — aici.
                var valoareRamas = valoareNeacoperita;
                bucla.Divergenta($"{View}/{h.Id}",
                    "BCS: consum fără acoperire — marfa rămâne în stocul Atlas",
                    nomRef == null ? null : [new EfectStoc(nomRef.Id, depozitHex, ramas, valoareRamas)],
                    simbolDebit, simbolCredit, valoareRamas);
            }

            // Puntea de cheltuială: contarea Atlas e derivarea 6xx = 3xx per Tip
            // (politică-DATE, citită din RegulaContare, nu re-derivată aici). Când
            // 1C a folosit alt cont (sponsorizare 6584, alte cheltuieli 6588,
            // materii prime 601) sau alt cont de stoc decât cel de naștere al
            // lotului, se transcrie diferența — la VALOAREA Atlas, ca diferența de
            // evaluare rămasă din netarea deschiderii să nu se strecoare în notă.
            var contare = cat.ContareConsum(tip.Id);
            if (contare == null)
                plan.Punte.Categoria("BCS: Tip fără regulă de contare în profil")
                    .Tinta1C(simbolDebit, simbolCredit, valoareAtlas);
            else if (contare.Value.Debit != simbolDebit || contare.Value.Credit != simbolCredit) {
                PuntiCheltuiala++;
                plan.Punte.Categoria($"BCS: {simbolDebit} = {simbolCredit} în 1C, "
                        + $"{contare.Value.Debit} = {contare.Value.Credit} în Atlas")
                    .Tinta1C(simbolDebit, simbolCredit, valoareAtlas);
                plan.Punte.ActualAtlas(contare.Value.Debit, contare.Value.Credit, valoareAtlas);
            }
        }
        return plan;
    }

    static Document Materializeaza(IObjectSpace os, Catalog cat, Plan plan) {
        if (plan == null || plan.Linii.Count == 0)
            return null;
        var bcs = os.CreateObject<BonConsum>();
        bcs.Data = plan.Data;
        bcs.PredatorId = plan.PredatorId;
        // Locul de consum: repartitorul intern purtător al calității LocConsum
        // (decizia 16) — 1C ține destinația pe subconto-ul de cheltuieli
        // (Departamente), care n-are corespondent în modelul privat de azi.
        bcs.PrimitorId = cat.SediuId;
        foreach (var l in plan.Linii) {
            var d = os.CreateObject<DocumentDetaliu>();
            d.Document = bcs;
            d.TipMaterialId = l.TipMaterialId;
            d.LotId = l.LotId;
            d.Cantitate = l.Cantitate;
        }
        return bcs;
    }

    public static void Raporteaza() =>
        Console.WriteLine($"  BCS: {DocumenteSparte} bonuri sparte pe gestiuni, {RanduriNeutre} rânduri "
            + $"neutre în sursă (același cont și lot pe ambele laturi) sărite, {PuntiCheltuiala} punți "
            + $"de cheltuială, {RanduriNerezolvate} rânduri nerezolvate.");
}

// ======================= 4. Mărire / Diminuare stoc → LDI ± =======================
static class HandlerDiferente {
    public const string ViewPlus = "MarireStocDeMarfuri";
    public const string ViewMinus = "DiminuareStocDeMarfuri";

    public static readonly HandlerTip HandlerPlus =
        new(ViewPlus, "Listă diferențe de inventar (plus)", ctx => ImportaPlus(ctx));

    public static readonly HandlerTip HandlerMinus =
        new(ViewMinus, "Listă diferențe de inventar (minus)", ctx => ImportaMinus(ctx));

    public static int NepostateSarite { get; private set; }
    public static int Plusuri { get; private set; }
    public static int Minusuri { get; private set; }

    sealed record LiniePlus(Guid ProdusId, TipInfo Tip, string CheieLot, decimal Cantitate, decimal Pret);

    sealed class Plan {
        public Guid PredatorId;
        public DateOnly Data;
        public List<LiniePlus> Plusuri = [];
        public List<LiniePeLot> Minusuri = [];
        public Punte Punte;

        public bool AreDocument => Plusuri.Count > 0 || Minusuri.Count > 0;
    }

    // Plusul de inventar: lotul se NAȘTE pe propria linie (28d), cu prețul de
    // evaluare al sursei. 1C postează invers (cheltuiala se stornează: 607 = 371
    // cu sumă negativă), Atlas postează venitul din plus (371 = 7588) — puntea
    // ține diferența de formă, la valoarea Atlas.
    static void ImportaPlus(ContextLuna ctx) {
        var bucla = ctx.Bucla;
        var linii = bucla.Flax.MaririStocMarfuri(ctx.An, ctx.Luna)
            .GroupBy(l => l.DocumentId, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.Ordinal);
        foreach (var h in bucla.Flax.MaririStoc(ctx.An, ctx.Luna))
            ctx.Planifica(h.Data, h.Numar, () => {
                // Aceeași gardă ca la factură: plusul se construiește din SECȚIUNE,
                // deci un antet Posted care nu postează în 1C ar intra în Atlas cu lot
                // și stoc pe care sursa nu le are (2 astfel de antete pe 2025).
                if ((bucla.RanduriLuna.GetValueOrDefault(h.Id)?.Count ?? 0) == 0) {
                    NepostateSarite++;
                    return;
                }
                Plan plan = null;
                if (!bucla.EsteCunoscut(ViewPlus, h.Id)) {
                    try {
                        plan = PlanificaPlus(ctx, h, linii.GetValueOrDefault(h.Id) ?? []);
                    }
                    catch (Exception ex) {
                        bucla.EsecPlanificare(ViewPlus, h.Id, ex);
                        return;
                    }
                    Punti.Scrie(bucla, ViewPlus, plan.AreDocument ? h.Id + "#punte" : h.Id,
                        h.Numar, plan.Data, plan.Punte, bucla.ContorPunti, bucla.Avert);
                    if (!plan.AreDocument && !plan.Punte.AreCeva)
                        bucla.NumaraSursaFaraCorespondent();
                }
                if (plan == null || plan.AreDocument)
                    bucla.ImportaDocument(ViewPlus, h.Id, os => Materializeaza(os, bucla.Catalog, plan),
                        motivFaraDraft: Motive.FaraPlan(plan, "plusul n-a rămas cu nicio linie"));
            });
    }

    static Plan PlanificaPlus(ContextLuna ctx, FlaxAjustareStoc h, List<FlaxAjustareStocMarfa> linii) {
        var bucla = ctx.Bucla;
        var cat = bucla.Catalog;
        var plan = new Plan {
            Data = DateOnly.FromDateTime(h.Data),
            Punte = new Punte(),
        };
        plan.PredatorId = cat.Gestiuni.TryGetValue(h.DepozitId ?? "", out var g)
            ? g
            : throw new InvalidOperationException(
                $"Depozitul 1C {h.DepozitId} al măririi de stoc nu e legat de o Gestiune.");
        var contCheltuiala1C = cat.Mapeaza(h.ContCheltuieli);

        foreach (var l in linii) {
            var simbol = cat.Mapeaza(l.ContEvidenta);
            var tip = cat.TipStocPentru(simbol)
                ?? throw new InvalidOperationException(
                    $"Contul de stoc 1C „{l.ContEvidenta}” n-are TipMaterial de stoc în profil.");
            var cantitate = Math.Abs(l.Cantitate);
            if (cantitate == 0)
                continue;
            var valoare = Math.Abs(l.Suma);
            var produsId = bucla.LaCerere.AsiguraProdus(l.NomenclatorId, tip.Cod)
                ?? throw new InvalidOperationException(
                    $"Nomenclatorul 1C {l.NomenclatorId} nu s-a putut importa pe Tipul {tip.Cod}.");
            plan.Plusuri.Add(new LiniePlus(produsId, tip,
                Catalog.CheieLot(cat.TipRef(ViewPlus), h.Id, l.NomenclatorId, simbol),
                cantitate, valoare / cantitate));
            Plusuri++;

            // Ținta = rândul 1C (cheltuiala stornată, cu semnul sursei); actualul
            // = venitul din plus pe care îl postează regula LDI a profilului.
            plan.Punte.Categoria("LDI+: plusul de stoc e storno de cheltuială în 1C")
                .Tinta1C(contCheltuiala1C, simbol, -valoare);
            plan.Punte.ActualAtlas(simbol, cat.ContPlusInventar, valoare);
        }
        return plan;
    }

    // Diminuarea de stoc: minusul descarcă un lot EXISTENT (pin din subconto).
    // Pe 2025 nu există niciun document de acest fel — codul e simetric plusului
    // și NETESTAT pe date reale; apariția unuia îl exersează.
    static void ImportaMinus(ContextLuna ctx) {
        var bucla = ctx.Bucla;
        foreach (var h in bucla.Flax.DiminuariStoc(ctx.An, ctx.Luna))
            ctx.Planifica(h.Data, h.Numar, () => {
                Plan plan = null;
                if (!bucla.EsteCunoscut(ViewMinus, h.Id)) {
                    try {
                        plan = PlanificaMinus(ctx, h);
                    }
                    catch (Exception ex) {
                        bucla.EsecPlanificare(ViewMinus, h.Id, ex);
                        return;
                    }
                    Punti.Scrie(bucla, ViewMinus, plan.AreDocument ? h.Id + "#punte" : h.Id,
                        h.Numar, plan.Data, plan.Punte, bucla.ContorPunti, bucla.Avert);
                    if (!plan.AreDocument && !plan.Punte.AreCeva)
                        bucla.NumaraSursaFaraCorespondent();
                }
                if (plan == null || plan.AreDocument)
                    bucla.ImportaDocument(ViewMinus, h.Id, os => Materializeaza(os, bucla.Catalog, plan),
                        motivFaraDraft: Motive.FaraPlan(plan, "minusul n-a rămas cu nicio linie"));
            });
    }

    static Plan PlanificaMinus(ContextLuna ctx, FlaxAjustareStoc h) {
        var bucla = ctx.Bucla;
        var cat = bucla.Catalog;
        var plan = new Plan {
            Data = DateOnly.FromDateTime(h.Data),
            Punte = new Punte(),
        };
        plan.PredatorId = cat.Gestiuni.TryGetValue(h.DepozitId ?? "", out var g)
            ? g
            : throw new InvalidOperationException(
                $"Depozitul 1C {h.DepozitId} al diminuării de stoc nu e legat de o Gestiune.");

        var index = Subconto.Indexeaza(bucla.SubcontoLuna.GetValueOrDefault(h.Id) ?? []);
        var dejaAlocat = new Dictionary<Guid, decimal>();
        using var os = bucla.CreeazaObjectSpace();
        foreach (var r in bucla.RanduriLuna.GetValueOrDefault(h.Id) ?? []) {
            var simbolCredit = cat.Mapeaza(r.ContCredit);
            var simbolDebit = cat.Mapeaza(r.ContDebit);
            var context = $"1C:{ViewMinus}/{h.Id} rândul {r.Linie}";
            var nomRef = index.Ia(r.Linie, Subconto.Credit, Subconto.Nomenclator);
            var rezolvat = MiscareStoc1C.Rezolva(bucla,
                index.Ia(r.Linie, Subconto.Credit, Subconto.Loturi), nomRef, simbolCredit, context);
            if (rezolvat is not var (lot, tip, produsId))
                continue;
            var (alocari, ramas) = bucla.Alocare.Aloca(os, lot?.Id, produsId, plan.PredatorId,
                tip.Registru, plan.Data, r.CantitateCredit, dejaAlocat);
            var valoareAtlas = 0m;
            foreach (var (lotId, cantitate) in alocari) {
                plan.Minusuri.Add(new LiniePeLot(lotId, tip.Id, cantitate));
                valoareAtlas += cantitate * HandlerTransfer.PretLot(os, lotId);
                Minusuri++;
            }
            var valoareNeacoperita = 0m;
            if (ramas > 0) {
                bucla.Avert($"{context}: {ramas:N3} din {r.CantitateCredit:N3} n-au acoperire.");
                var valoareRamas = valoareNeacoperita = r.CantitateCredit == 0m
                    ? r.Suma : r.Suma * ramas / r.CantitateCredit;
                bucla.Divergenta($"{ViewMinus}/{h.Id}",
                    "LDI−: minus fără acoperire — marfa rămâne în stocul Atlas",
                    nomRef == null ? null
                        : [new EfectStoc(nomRef.Id, h.DepozitId ?? "", ramas, valoareRamas)],
                    simbolDebit, simbolCredit, valoareRamas);
            }
            // Aceeași declarație de evaluare ca la consum și la transfer (Punte.cs).
            // Pe 2025 nu există niciun document de tipul ăsta, deci linia e
            // netestată pe date reale — stă aici ca minusul să nu fie singurul loc
            // în care puntea transcrie la valoarea Atlas fără s-o declare.
            plan.Punte.TintaEvaluata(simbolDebit, simbolCredit, r.Suma - valoareNeacoperita);
            plan.Punte.ActualEvaluat(simbolDebit, simbolCredit, valoareAtlas);
            var contare = cat.ContareMinusInventar(tip.Id);
            if (contare == null)
                plan.Punte.Categoria("LDI−: Tip fără regulă de contare în profil")
                    .Tinta1C(simbolDebit, simbolCredit, valoareAtlas);
            else if (contare.Value.Debit != simbolDebit || contare.Value.Credit != simbolCredit) {
                plan.Punte.Categoria($"LDI−: {simbolDebit} = {simbolCredit} în 1C, "
                        + $"{contare.Value.Debit} = {contare.Value.Credit} în Atlas")
                    .Tinta1C(simbolDebit, simbolCredit, valoareAtlas);
                plan.Punte.ActualAtlas(contare.Value.Debit, contare.Value.Credit, valoareAtlas);
            }
        }
        return plan;
    }

    static Document Materializeaza(IObjectSpace os, Catalog cat, Plan plan) {
        if (plan == null || (plan.Plusuri.Count == 0 && plan.Minusuri.Count == 0))
            return null;
        var ldi = os.CreateObject<ListaDiferenteInventar>();
        ldi.Data = plan.Data;
        ldi.PredatorId = plan.PredatorId;
        // Primitorul listei e comisia de inventariere (28d).
        ldi.PrimitorId = cat.ComisieId;
        var gestiune = os.GetObjectByKey<Gestiune>(plan.PredatorId);
        foreach (var p in plan.Plusuri) {
            var d = os.CreateObject<ListaDiferenteInventarDetaliu>();
            d.Document = ldi;
            d.TipMaterialId = p.Tip.Id;
            d.Directie = DirectieDiferenta.Plus;
            d.Cantitate = p.Cantitate;
            d.PretEvaluare = p.Pret;
            var lot = d.CreeazaLot(os, os.GetObjectByKey<Produs>(p.ProdusId), gestiune);
            cat.LeagaLotNou(os, p.CheieLot, lot.ID);
        }
        foreach (var m in plan.Minusuri) {
            var d = os.CreateObject<ListaDiferenteInventarDetaliu>();
            d.Document = ldi;
            d.TipMaterialId = m.TipMaterialId;
            d.Directie = DirectieDiferenta.Minus;
            d.LotId = m.LotId;
            d.Cantitate = m.Cantitate;
        }
        return ldi;
    }

    public static void Raporteaza() =>
        Console.WriteLine($"  LDI: {NepostateSarite} antete Posted care NU postează în 1C (sărite), "
            + $"{Plusuri} linii de plus (lot nou), {Minusuri} linii de minus "
            + "(cod simetric, fără documente în 2025).");
}
