using Atlas.Conta.BackOffice.Module.BusinessObjects;
using DevExpress.ExpressApp;

namespace Import1C;

// PASUL 4 al feliei 1C-c: mecanismele COMUNE ale ieșirilor de marfă — liniile de
// venit ale facturii, descărcarea de gestiune construită din rândurile de cost și
// clasificarea rândurilor 1C față de forma tipizată Atlas.
//
// Toate cele trei familii de ieșire (factura de ieșire, raportul de vânzări cu
// amănuntul, avizul de ieșire) au aceeași anatomie în 1C: rânduri de VENIT
// (creanță/casă = 7xx), rânduri de TVA și rânduri de COST (6xx = 3xx, cu lotul în
// subconto). Diferă doar latura care încasează și documentul purtător — deci
// mecanismele stau aici, iar handlerele rămân politică.

// ======================= Liniile de venit =======================
//
// **Decizia de formă a pasului 4** (raportată explicit): factura de ieșire de
// import poartă DOAR linii de venit (natura Serviciu, Tipul = contul de venit al
// liniei 1C), NICIODATĂ linii de stoc. Motivele sunt de model, nu de comoditate:
//
//  1. `FacturaIesire.ValideazaOperare` cere `GestiuneDescarcare` pe orice factură
//     cu linii de stoc, iar cu ea setată motorul generează SINGUR descărcarea, la
//     picking FIFO (`DescarcareService.Genereaza`). Designul fazei (§4) spune
//     explicit contrariul: „descărcarea NU se re-pichează — liniile DSC se
//     construiesc din rândurile 607 = 371 ale 1C cu lotul explicit ca pin".
//     Cele două nu pot coexista pe același document: ori generăm noi DSC-ul din
//     sursă, ori îl generează motorul din produse.
//  2. Contul de venit REAL al liniei 1C (707.1 la marfă, dar și 418 la factura
//     după aviz, 419.1 la avansul consumat, 767.1 la sconto) e purtat de linie,
//     nu derivat din Tipul de stoc. Derivarea de vânzare a profilului (37e:
//     371→707, 345→701, 381→708) ar posta ALT cont pe pozițiile care nu sunt
//     mărfuri — o diferență de formă inventată de import, nu de sursă.
//
// Consecința e curată: creanța și veniturile ies EXACT ca în 1C (contract §8.1),
// iar mișcarea de stoc trăiește integral pe DSC, cu loturile sursei (contract
// §8.3). Identitatea produs/lot nu se pierde — e pe descărcare, unde contează.
// `Simbol` e contul OMFP al venitului, ținut SEPARAT de `Tip.Cod`: Tipul creat la
// cerere primește codul „S<simbol>" când simbolul are deja un Tip de STOC în
// profil (mecanismul geamănului din Catalog), iar puntea lucrează pe conturi.
sealed record LinieVenit(TipInfo Tip, string Simbol, Guid? TipTvaId, decimal Net, decimal Tva);

static class Venituri1C {
    public static int TvaLivrareTaxareInversa { get; private set; }

    // Agregarea pe (cont de venit × cotă de TVA): 1C postează un rând de notă per
    // linie de secțiune, dar factura Atlas n-are nevoie de granularitatea aia —
    // contul și cota sunt tot ce decide postarea. Agregarea taie liniile la
    // jumătate pe facturile mari și nu pierde nimic reconciliabil.
    public static List<LinieVenit> Aduna(Catalog cat, IDocumentCuValuta doc, bool sumaIncludeTva,
            IEnumerable<(string ContVenituri, string CotaTva, decimal Suma, decimal SumaTva)> linii) {
        var pe = new Dictionary<(string Cont, string Cota), (decimal Net, decimal Tva)>();
        var ordine = new List<(string Cont, string Cota)>();
        foreach (var l in linii) {
            var simbol = cat.Mapeaza(l.ContVenituri);
            if (simbol == null)
                throw new InvalidOperationException(
                    $"Contul de venit 1C „{l.ContVenituri}” nu se mapează pe planul OMFP.");
            var cheie = (simbol, l.CotaTva ?? "");
            var (net, tva) = doc.NetSiTva(l.Suma, l.SumaTva, sumaIncludeTva);
            if (!pe.TryGetValue(cheie, out var acum))
                ordine.Add(cheie);
            pe[cheie] = (acum.Net + net, acum.Tva + tva);
        }

        var rezultat = new List<LinieVenit>();
        foreach (var cheie in ordine) {
            var (net, tva) = pe[cheie];
            if (net == 0m && tva == 0m)
                continue;
            var tip = cat.TipVenitPentru(cheie.Cont)
                ?? throw new InvalidOperationException(
                    $"Contul de venit {cheie.Cont} n-are TipMaterial în profil și nu s-a putut crea.");
            var tipTva = cat.TipTvaCules(cheie.Cota, tva);
            // **Taxarea inversă pe latura de LIVRARE nu postează nimic**: taxa o
            // autolichidează cumpărătorul, iar `SumaTVA` a secțiunii e informativă
            // (verificat pe rândurile sursei — 1C nu scrie niciun rând de TVA
            // pentru liniile astea, doar venitul net). Regimul din motor e cel al
            // ACHIZIȚIEI (4426 = 4427, autolichidare), deci linia rămâne FĂRĂ
            // TipTva — nu doar cu valoare zero: `PregatesteOperare` recalculează
            // TVA-ul din cotă exact când valoarea culeasă e zero (36a păstrează
            // doar un ValoareTva NENUL), iar linia ar reînvia rândul inexistent.
            // Măsurat pe ianuarie: 70.964,55 lei de 4426 = 4427 inventați așa.
            if (cat.EsteTaxareInversa(tipTva)) {
                TvaLivrareTaxareInversa++;
                tipTva = null;
                tva = 0m;
            }
            rezultat.Add(new LinieVenit(tip, cheie.Cont, tipTva, net, tva));
        }
        return rezultat;
    }

    // Linia de factură de ieșire: cantitatea e pro-forma (identitatea liniei e
    // contul de venit, nu produsul — vezi nota de sus), prețul unitar poartă
    // netul, iar TVA-ul cules NU se recalculează (36a uniformizat — decizia 48b).
    public static void Materializeaza(IObjectSpace os, FacturaIesire fcl, IEnumerable<LinieVenit> venituri) {
        foreach (var v in venituri) {
            var d = os.CreateObject<FacturaIesireDetaliu>();
            d.Document = fcl;
            d.TipMaterialId = v.Tip.Id;
            d.Cantitate = 1m;
            d.PretUnitar = v.Net;
            d.TipTvaId = v.TipTvaId;
            d.ValoareTva = v.Tva;
        }
    }

    // Ce postează Atlas pentru liniile de venit + TVA — se declară în punte ca să
    // se anuleze cu rândurile 1C corespondente (mecanica deltei, Punte.cs).
    // `semn` = −1 la retur (returul de la client postează aceeași corespondență cu
    // valori NEGATIVE — rezoluția spike-ului storno, decizia 46a).
    public static void DeclaraInPunte(Punte punte, IEnumerable<LinieVenit> venituri, int semn = 1) {
        foreach (var v in venituri) {
            punte.ActualAtlas(Catalog.ContCreantaImplicit, v.Simbol, semn * v.Net);
            if (v.TipTvaId != null && v.Tva != 0m)
                punte.ActualAtlas(Catalog.ContCreantaImplicit, "4427", semn * v.Tva);
        }
    }
}

// ======================= Descărcarea de gestiune =======================
//
// Rândurile 6xx = 3xx ale documentului de vânzare sunt costul mărfii ieșite, cu
// LOTUL în subconto — exact materia primă a unui DSC (design §4: „liniile DSC se
// construiesc din rândurile 607 = 371 ale 1C cu lotul explicit ca pin"). Valoarea
// NU se preia din 1C: o pune motorul din prețul lotului Atlas (`PregatesteOperare`),
// iar diferența rămasă din netarea deschiderii e diferență justificată (§8.3) —
// de aceea rândurile de cost nu se declară deloc în punte (nici țintă, nici
// actual): o punte pe ele ar „repara" o VALOARE, ceea ce e interzis.
static class Descarcare1C {
    public sealed record Grup(string DepozitHex, Guid GestiuneId, List<LiniePeLot> Linii);

    public static int RanduriNerezolvate { get; private set; }
    public static int LiniiDescarcate { get; private set; }
    public static int DocumenteSparte { get; private set; }
    public static int NeacoperitTranscris { get; private set; }

    // Rândul de COST: cheltuială pe debit, cont de stoc al profilului pe credit.
    // Clasificarea se face pe simbolurile MAPATE (fără avertismente — absența
    // Tipului e informație aici, nu gaură de profil).
    public static bool EsteRandDeCost(Catalog cat, FlaxRandNota r) {
        var debit = cat.Mapeaza(r.ContDebit);
        return debit != null && debit.StartsWith('6') && cat.EsteContDeStoc(cat.Mapeaza(r.ContCredit));
    }

    // Planificarea descărcării: un grup per gestiune (sursa n-are documente
    // multi-gestiune pe 2025 — verificat — dar cheia poartă gestiunea când sunt
    // mai multe, ca reluarea să rămână exactă, mecanica bonului de consum).
    public static List<Grup> Planifica(ContextLuna ctx, string view, string docId, DateOnly data,
            IReadOnlyList<FlaxRandNota> randuri,
            Dictionary<(int, int), Dictionary<string, FlaxRef>> index, string depozitImplicit,
            Punte punte) {
        var bucla = ctx.Bucla;
        var cat = bucla.Catalog;
        var grupuri = new List<Grup>();
        var dejaAlocat = new Dictionary<Guid, decimal>();
        using var os = bucla.CreeazaObjectSpace();

        var sursa = $"{view}/{docId}";
        foreach (var r in randuri) {
            if (!EsteRandDeCost(cat, r))
                continue;
            var context = $"1C:{view}/{docId} rândul {r.Linie}";
            var nomRef = index.Ia(r.Linie, Subconto.Credit, Subconto.Nomenclator);
            var depozitHex = index.Ia(r.Linie, Subconto.Credit, Subconto.Depozite)?.Id
                ?? depozitImplicit ?? "";
            var rezolvat = MiscareStoc1C.Rezolva(bucla,
                index.Ia(r.Linie, Subconto.Credit, Subconto.Loturi), nomRef,
                cat.Mapeaza(r.ContCredit), context);
            if (rezolvat is not var (lot, tip, produsId)) {
                // Rând de cost pe care nu-l putem duce în stoc (lot/produs
                // nerezolvabil): la fel ca partea neacoperită, se transcrie
                // contabil — altfel ar dispărea tăcut din solduri.
                RanduriNerezolvate++;
                punte.Categoria("Ieșire de marfă cu lot nerezolvabil — costul se transcrie contabil")
                    .Tinta1C(cat.Mapeaza(r.ContDebit), cat.Mapeaza(r.ContCredit), r.Suma);
                // Contabil, rândul e acoperit de puntea de mai sus: se scoate din
                // acumularea EVALUATĂ (unde clasificarea l-a pus, ca pe orice rând
                // de cost), altfel ar fi explicat de două ori.
                punte.ActualEvaluat(cat.Mapeaza(r.ContDebit), cat.Mapeaza(r.ContCredit), r.Suma);
                // Contabil nu divergem (puntea a transcris rândul), dar marfa RĂMÂNE
                // în stocul Atlas: se măsoară, ca să nu se ceară mai târziu unei
                // euristici s-o ghicească.
                bucla.Divergenta(sursa,
                    "DSC: rând de cost cu lot nerezolvabil — marfa rămâne în stocul Atlas",
                    nomRef == null ? null
                        : [new EfectStoc(nomRef.Id, depozitHex, Math.Abs(r.CantitateCredit), r.Suma)],
                    cat.Mapeaza(r.ContDebit), cat.Mapeaza(r.ContCredit));
                continue;
            }
            var gestiuneId = cat.Gestiuni.TryGetValue(depozitHex, out var g)
                ? g
                : throw new InvalidOperationException(
                    $"Depozitul 1C {depozitHex} al rândului de cost nu e legat de o Gestiune.");
            var grup = grupuri.FirstOrDefault(x => x.GestiuneId == gestiuneId);
            if (grup == null)
                grupuri.Add(grup = new Grup(depozitHex, gestiuneId, []));

            var cantitate = Math.Abs(r.CantitateCredit);
            var (alocari, ramas) = bucla.Alocare.Aloca(os, lot?.Id, produsId, gestiuneId,
                tip.Registru, data, cantitate, dejaAlocat);
            // Ce postează motorul pentru liniile care se nasc din rândul ăsta:
            // perechea de conturi a regulii DSC pe Tipul lotului, la costul lotului
            // ATLAS, rotunjit cum rotunjește materializarea. Declararea închide
            // cercul deschis de `Rand.Evaluat`: diferența față de cifra sursei
            // devine divergență MĂSURATĂ, nu „încape în plafon". Ea acoperă și
            // cazul în care 1C a reclasificat lotul (rândul sursei spune 607 = 371,
            // Atlas postează 608 = 381 fiindcă lotul a rămas pe geamănul lui).
            var contare = cat.Contare("DSC", tip.Id);
            foreach (var (lotId, q) in alocari) {
                grup.Linii.Add(new LiniePeLot(lotId, tip.Id, q));
                LiniiDescarcate++;
                if (contare is { } c)
                    punte.ActualEvaluat(c.Debit, c.Credit,
                        Scara.RotunjesteBani(q * HandlerTransfer.PretLot(os, lotId)));
            }
            if (ramas > 0) {
                bucla.Avert($"{context}: {ramas:N3} din {cantitate:N3} n-au acoperire în gestiunea "
                    + "care descarcă — marfa se descarcă parțial (diferență de stoc raportată).");
                // Partea NEACOPERITĂ nu produce linie, deci Atlas nu postează
                // costul ei: rândul 1C ar rămâne fără corespondent tăcut, iar
                // soldurile 6xx/3xx ar diverge fără urmă. Se transcrie proporțional
                // în punte — contabilitatea rămâne a sursei, stocul e cel pe care
                // Atlas chiar îl are (diferența de stoc se raportează separat).
                NeacoperitTranscris++;
                var valoareRamas = cantitate == 0 ? r.Suma : r.Suma * ramas / cantitate;
                punte.Categoria("Ieșire de marfă fără acoperire în stoc — costul se transcrie contabil")
                    .Tinta1C(cat.Mapeaza(r.ContDebit), cat.Mapeaza(r.ContCredit), valoareRamas);
                // Partea transcrisă e postată (de nota-punte), deci pleacă din
                // acumularea evaluată: acolo rămâne exact partea pe care motorul o
                // postează la costul lotului.
                punte.ActualEvaluat(cat.Mapeaza(r.ContDebit), cat.Mapeaza(r.ContCredit), valoareRamas);
                // Măsurătoarea: marfa neieșită rămâne în stocul Atlas exact cu
                // `ramas` bucăți. Contabil nu divergem (puntea a transcris partea
                // neacoperită), deci `valoareNepostata` rămâne 0.
                bucla.Divergenta(sursa,
                    "DSC: ieșire fără acoperire în stoc — marfa rămâne în stocul Atlas",
                    nomRef == null ? null : [new EfectStoc(nomRef.Id, depozitHex, ramas, valoareRamas)],
                    cat.Mapeaza(r.ContDebit), cat.Mapeaza(r.ContCredit));
            }
        }
        if (grupuri.Count > 1)
            DocumenteSparte++;
        return grupuri.Where(g => g.Linii.Count > 0).ToList();
    }

    // Cheia de idempotență a descărcării: „#dsc@<depozit>" pe cheia documentului
    // sursă. Sufixul poartă ÎNTOTDEAUNA depozitul (nu doar la documentele
    // multi-gestiune) fiindcă e derivabil din sursă FĂRĂ planificare — de el
    // atârnă gardul de replanificare al reluării: a doua rulare trebuie să știe
    // ce chei ar fi trebuit să existe înainte de a re-aloca vreun lot.
    public static string Cheie(string docId, string depozitHex) => $"{docId}#dsc@{depozitHex}";

    // Depozitele atinse de rândurile de cost, în ordinea apariției — sursa
    // cheilor așteptate. Nu atinge baza: doar subconto-ul lunii, deja în memorie.
    public static List<string> Depozite(Catalog cat, IEnumerable<FlaxRandNota> randuri,
            Dictionary<(int, int), Dictionary<string, FlaxRef>> index, string depozitImplicit) {
        var depozite = new List<string>();
        foreach (var r in randuri) {
            if (!EsteRandDeCost(cat, r))
                continue;
            var depozitHex = index.Ia(r.Linie, Subconto.Credit, Subconto.Depozite)?.Id
                ?? depozitImplicit ?? "";
            if (!depozite.Contains(depozitHex))
                depozite.Add(depozitHex);
        }
        return depozite;
    }

    // `sursaId` = documentul Atlas care a produs descărcarea (factura de ieșire /
    // FCL-ul surogat al raportului de amănunt). Cu el, DSC-ul e marcat
    // `Autogenerat` + `DocumentSursa` și intră natural în gardianul de grup al
    // motorului (copiii operați blochează anularea sursei). Avizul de ieșire n-are
    // document purtător ⇒ descărcare de sine stătătoare, legală prin construcție
    // (DSC-ul cules manual e document normal de ieșire din gestiune).
    // `grup == null` = planul lipsește (documentul e deja importat) sau toate
    // rândurile lui de cost au rămas nerezolvate: „sursa n-are ce importa".
    public static Document Materializeaza(IObjectSpace os, Grup grup, DateOnly data, string numar,
            Guid primitorId, Guid? sursaId) {
        if (grup == null || grup.Linii.Count == 0)
            return null;
        var dsc = os.CreateObject<DescarcareGestiune>();
        dsc.Data = data;
        dsc.Numar = numar;
        dsc.PredatorId = grup.GestiuneId;
        dsc.PrimitorId = primitorId;
        if (sursaId is { } sursa) {
            dsc.DocumentSursaId = sursa;
            dsc.Autogenerat = true;
        }
        foreach (var l in grup.Linii) {
            var d = os.CreateObject<DescarcareGestiuneDetaliu>();
            d.Document = dsc;
            d.TipMaterialId = l.TipMaterialId;
            d.LotId = l.LotId;
            d.Cantitate = l.Cantitate;
        }
        return dsc;
    }

    public static void Raporteaza() =>
        Console.WriteLine($"  DSC: {LiniiDescarcate} linii de descărcare pe lot, "
            + $"{DocumenteSparte} documente cu mai multe gestiuni, "
            + $"{RanduriNerezolvate} rânduri de cost nerezolvate, {NeacoperitTranscris} rânduri "
            + "cu marfă fără acoperire (costul transcris în punte).");
}

// ======================= Gardianul reluării =======================
//
// Consecința transcrierii părții neacoperite (mai sus): puntea unei rulări
// ANTERIOARE conține deja costul mărfii pe care documentul de stoc nu l-a putut
// mișca atunci. Dacă o rulare ulterioară găsește între timp acoperire (stocul s-a
// schimbat fiindcă tipurile se importă grupat, nu strict cronologic) și
// materializează documentul acum, costul s-ar posta A DOUA OARĂ — puntea veche nu
// se rescrie (cheia ei e deja legată).
//
// Ordinea handlerelor (intrările înaintea ieșirilor — Bucla.cs) face cazul să nu
// mai apară pe datele reale; gardianul rămâne fiindcă „nu mai apare pe ianuarie"
// nu e o garanție, iar simptomul ar fi o dublare TĂCUTĂ de cost.
//
// UNDE SE APLICĂ (verificat la lotul de robustețe, pasul 3): exact acolo unde o
// punte poate purta costul unei mișcări de stoc nematerializate — descărcarea
// (`Descarcare1C.Planifica`: rând de cost nerezolvabil / fără acoperire) și
// returul la furnizor. Bonul de consum și transferul doar AVERTIZEAZĂ la lipsa de
// acoperire (nu transcriu nimic), iar asamblarea nu scrie punte deloc: rândurile
// ei sunt 371 = 371 pe același simbol prin construcție, orice altă formă e eșec
// zgomotos la planificare. Deci gardianul n-are ce apăra acolo — nu se „aplică
// pentru simetrie", s-ar bloca documente fără motiv.
//
// Ieșirea din blocaj nu mai e reconstrucția întregii baze: `--deblocheaza
// <view>:<cheie>` (Reluare.cs) dă înapoi artefactele rulării anterioare pentru
// documentul-sursă și îl lasă să se replanifice integral.
static class Reluare1C {
    public static int DocumenteBlocate { get; private set; }
    public static int UnitatiPartiale { get; private set; }
    public static int CheiRefuzate { get; private set; }

    // ======================= D1: unitatea PARȚIAL importată =======================
    //
    // Defectul: handlerele compuse planifică ÎNTREG documentul când oricare dintre
    // cheile lui lipsește. La reluare, `Aloca` rulează atunci contra unui registru
    // care conține DEJA mișcările componentelor importate de rularea anterioară —
    // deci componenta aia „nu mai are acoperire", iar planificarea produce
    // divergențe FANTOMĂ (persistate), marcaje de realocare fantomă și, când
    // puntea nu apucase să fie scrisă, chiar o notă-punte care transcrie contabil
    // un cost pe care Atlas îl postează deja.
    //
    // Șablonul BCS (`HandlereStoc.Grupeaza`) rezolvă asta planificând PER GRUP —
    // dar el poate, fiindcă bonul de consum are punte PER GRUP. La documentele de
    // vânzare/retur/asamblare puntea NU e separabilă: `Clasificare1C.Declara`
    // parcurge TOATE rândurile documentului și cere fiecăruia un rost declarat
    // (rândul de venit nu aparține niciunui depozit), iar acumularea EVALUATĂ
    // (`Punte.RestEvaluat`, sursa divergenței de evaluare) e per document. O
    // planificare parțială ar produce o punte parțială și ar rescrie registrul
    // divergențelor cu jumătate din fapte.
    //
    // Deci regula e cea sancționată de spec pentru asamblare, generalizată: **o
    // unitate cu componente de STOC deja așezate nu se mai replanifică**. Cheile
    // care lipsesc se refuză ZGOMOTOS, cu motiv itemizat și cu remediul scris —
    // `--deblocheaza` dă înapoi tot ce a produs sursa (inclusiv divergențele ei,
    // Reluare.cs) și documentul se replanifică integral, de la zero.
    //
    // De ce contează doar cheile de STOC: ele sunt singurele care lasă în registru
    // mișcări pe care alocarea le citește. O factură operată fără descărcare, sau
    // o descărcare rămasă DRAFT (draftul nu scrie registre, iar `EsteCunoscut` îl
    // declară oricum nedezvoltat — D4), nu poluează nimic: acolo replanificarea
    // rămâne corectă și D4 își face treaba.
    public static bool UnitatePartiala(BuclaImport bucla, string view, string cheieSursa,
            IReadOnlyList<string> chei, IReadOnlyCollection<string> cheiStoc, string motiv) {
        if (chei.Count == 0 || !cheiStoc.Any(c => bucla.EsteCunoscut(view, c)))
            return false;
        var lipsa = chei.Where(c => !bucla.EsteCunoscut(view, c)).ToList();
        if (lipsa.Count == 0)
            return false;
        UnitatiPartiale++;
        CheiRefuzate += lipsa.Count;
        bucla.Avert($"1C:{view}/{cheieSursa}: unitate PARȚIAL importată — {chei.Count - lipsa.Count} "
            + $"din {chei.Count} chei există deja (cel puțin una de stoc, operată), {lipsa.Count} "
            + $"lipsesc ({string.Join(", ", lipsa)}). NU se replanifică: alocarea ar rula peste "
            + "propriile mișcări deja comise și ar produce divergențe fantomă. Deblocare țintită: "
            + $"--deblocheaza {view}:{cheieSursa}");
        foreach (var _ in lipsa)
            bucla.RefuzaCuMotiv(view, motiv);
        return true;
    }

    public static bool Blocheaza(BuclaImport bucla, string view, bool punteVeche, string cheieDocument) {
        if (!punteVeche || bucla.EsteCunoscut(view, cheieDocument))
            return false;
        DocumenteBlocate++;
        bucla.Avert($"1C:{view}/{cheieDocument}: documentul de stoc lipsește, dar puntea sursei a fost "
            + "scrisă de o rulare ANTERIOARĂ (partea neacoperită e deja transcrisă contabil acolo) — "
            + "nu se materializează acum, ca să nu se posteze costul de două ori. Deblocare țintită: "
            + $"--deblocheaza {view}:{CheieSursa(cheieDocument)}");
        return true;
    }

    // Cheia documentului de stoc poartă sufixele handlerului („#dsc@…", „@…");
    // deblocarea lucrează pe cheia SURSEI, care le adună pe toate.
    static string CheieSursa(string cheieDocument) =>
        cheieDocument.IndexOfAny(['#', '@']) is var i && i > 0 ? cheieDocument[..i] : cheieDocument;

    public static void Raporteaza() {
        if (DocumenteBlocate > 0)
            Console.WriteLine($"  {DocumenteBlocate} documente de stoc blocate de puntea unei rulări "
                + "anterioare (vezi avertismentele) — deblocare țintită cu --deblocheaza <view>:<cheie>.");
        if (UnitatiPartiale > 0)
            Console.WriteLine($"  {UnitatiPartiale} unități PARȚIAL importate de o rulare anterioară, "
                + $"nereplanificate ({CheiRefuzate} chei refuzate) — deblocare țintită cu "
                + "--deblocheaza <view>:<cheie>.");
    }
}

// ======================= Clasificarea rândurilor 1C =======================
//
// Regula feliei (contractul pasului 4): fiecare rând al documentului 1C trebuie
// să aibă un rost DECLARAT. Ori Atlas îl postează (și atunci se declară în punte
// pe ambele laturi, ca delta să se anuleze), ori Atlas îl postează la valoarea LUI
// (cost din lot — nu se declară nimic, diferența e justificată), ori nu-l poate
// exprima și se transcrie în nota-punte. Un rând care nu intră în niciuna dintre
// categorii NU se ignoră tăcut: documentul eșuează zgomotos, cu perechea de
// conturi în mesaj — descoperirea unei forme noi e o decizie de luat, nu un
// accident de rulare (decizia 21).
enum FelRand {
    // Atlas postează același rând, la aceeași valoare (venit, TVA, trezorerie).
    Acoperit,
    // Atlas postează aceeași corespondență, dar evaluată din prețul lotului lui
    // (cost de descărcare, retur pe lot) — diferența e a netării, se raportează.
    Evaluat,
    // Nu se poate exprima în forma tipizată — se transcrie în punte.
    Punte,
}

// Verdictul unei reguli de clasificare. `Acoperit` NU mai e o afirmație liberă:
// poartă NUMELE acoperitorului (ce anume din planul Atlas postează rândul) și
// eticheta de punte pe care handlerul o declară pentru cazul în care acoperitorul
// LIPSEȘTE — vezi `Declara` pentru de ce.
readonly record struct Verdict(FelRand Fel, string Eticheta, string Acoperitor);

// Fabricile de verdicte: numele lor e contractul pe care îl citește handlerul.
static class Rand {
    // „Atlas postează rândul ăsta prin <acoperitor>". Dacă acoperitorul nu ajunge
    // în planul care se materializează, rândul merge pe punte cu eticheta dată —
    // deci fiecare afirmație de acoperire vine cu planul B declarat.
    public static Verdict Acoperit(string acoperitor, string etichetaFaraAcoperitor) =>
        new(FelRand.Acoperit, etichetaFaraAcoperitor, acoperitor);

    public static Verdict Evaluat => new(FelRand.Evaluat, null, null);

    public static Verdict Punte(string eticheta) => new(FelRand.Punte, eticheta, null);
}

// Perechea de conturi MAPATE a unui rând + clasificarea ei; ține și eticheta de
// raport a punții, ca handlerul să spună O DATĂ ce înseamnă fiecare formă.
sealed record RandClasificat(FlaxRandNota Rand, string Debit, string Credit, FelRand Fel,
    string Eticheta, string Acoperitor);

static class Clasificare1C {
    public static readonly IReadOnlySet<string> Niciunul =
        new HashSet<string>(StringComparer.Ordinal);

    // Clasifică rândurile documentului și declară în punte ce trebuie declarat, în
    // aceeași trecere (handlerele nu aveau niciodată ce face între cele două).
    //
    // `acoperitori` = acoperitorii pe care planul îi produce CHIAR — se calculează
    // după planificare, din planul gata făcut. Aserțiunea sistemică (F1c): un rând
    // declarat `Acoperit` printr-un acoperitor ABSENT nu mai e acoperit de nimeni,
    // iar tăcerea de acolo e exact felul în care 17 facturi de ieșire pe an au
    // dispărut fără urmă (nici document, nici punte, nici avertisment). Rândul
    // trece pe punte cu eticheta declarată de handler; un handler care n-a declarat
    // nici eticheta pică zgomotos — descoperirea unei forme noi rămâne decizie
    // explicită (decizia 21), niciodată accident.
    public static void Declara(Punte punte, Catalog cat, string view, string docId,
            IEnumerable<FlaxRandNota> randuri, IReadOnlySet<string> acoperitori,
            Func<FlaxRandNota, string, string, Verdict?> regula) {
        foreach (var r in Clasifica(cat, view, docId, randuri, regula)) {
            if (r.Fel == FelRand.Evaluat) {
                // Rândul nu intră în puntea de FORMĂ (o notă pe el ar repara o
                // valoare), dar ținta lui se declară în acumularea EVALUATĂ:
                // handlerul declară alături ce postează Atlas, iar diferența ajunge
                // măsurată în registrul divergențelor (Punte.cs).
                punte.TintaEvaluata(r.Debit, r.Credit, r.Rand.Suma);
                continue;
            }
            var eticheta = r.Eticheta;
            if (r.Fel == FelRand.Acoperit) {
                if (r.Acoperitor == null)
                    throw new InvalidOperationException(
                        $"Rândul {r.Rand.Linie} ({r.Debit} = {r.Credit}) al documentului "
                        + $"{view}/{docId} e declarat acoperit fără să spună de CE — "
                        + "regula trebuie să numească acoperitorul (Rand.Acoperit).");
                if (acoperitori.Contains(r.Acoperitor)) {
                    // Acoperit cu adevărat: ținta se declară fără etichetă, ca să se
                    // anuleze în deltă cu `ActualAtlas` al handlerului.
                    punte.Categoria(null).Tinta1C(r.Debit, r.Credit, r.Rand.Suma);
                    continue;
                }
                if (eticheta == null)
                    throw new InvalidOperationException(
                        $"Rândul {r.Rand.Linie} ({r.Debit} = {r.Credit}, {r.Rand.Suma:N2} lei) al "
                        + $"documentului {view}/{docId} se declara acoperit prin „{r.Acoperitor}”, "
                        + "dar planul nu-l produce — iar handlerul n-a declarat nicio categorie de "
                        + "punte pentru cazul ăsta (decizia 21: forma nouă se tranșează explicit).");
            }
            punte.Categoria(eticheta).Tinta1C(r.Debit, r.Credit, r.Rand.Suma);
        }
    }

    // Aplică lista de reguli (prima care se potrivește câștigă) și aruncă la
    // primul rând necunoscut — eșecul e per DOCUMENT (îl prinde `EsecPlanificare`).
    static List<RandClasificat> Clasifica(Catalog cat, string view, string docId,
            IEnumerable<FlaxRandNota> randuri,
            Func<FlaxRandNota, string, string, Verdict?> regula) {
        var rezultat = new List<RandClasificat>();
        foreach (var r in randuri) {
            var debit = cat.Mapeaza(r.ContDebit);
            var credit = cat.Mapeaza(r.ContCredit);
            var fel = regula(r, debit, credit)
                ?? throw new InvalidOperationException(
                    $"Rândul {r.Linie} ({r.ContDebit} = {r.ContCredit} → {debit} = {credit}, "
                    + $"{r.Suma:N2} lei) n-are corespondent declarat în handlerul {view} — "
                    + "formă nouă a sursei, de tranșat explicit (decizia 21).");
            rezultat.Add(new RandClasificat(r, debit, credit, fel.Fel, fel.Eticheta, fel.Acoperitor));
        }
        return rezultat;
    }
}
