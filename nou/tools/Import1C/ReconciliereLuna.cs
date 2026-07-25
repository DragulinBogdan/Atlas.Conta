using Atlas.Conta.BackOffice.Module.BusinessObjects;
using DevExpress.ExpressApp;

namespace Import1C;

// PASUL 6 al feliei 1C-c: CONTRACTUL DE RECONCILIERE PER LUNĂ (design §8, cu
// amendamentul §8.3 și regula de purtare înainte din §12.4).
//
// Principiul e al lui `Reconciliere.cs` (deschiderea) și nu se schimbă: baza se
// RECITEȘTE integral din Postgres prin proiecții proprii, iar sursa se re-citește
// din view-uri — nimic din structurile în memorie ale importului nu intră aici.
// O reconciliere care ar compara importul cu el însuși ar trece și când scrierea
// a eșuat.
//
// Cele trei numere care trebuie să bată la fine de lună:
//   1. sold per cont sintetic OMFP = Balanța 1C mapată (891 inclus, ca orice cont);
//   2. rândurile de TVA generate de ÎNCHIDEREA Atlas = sumele închiderii 1C
//      (rândurile pe care importul le-a sărit — forcing function-ul P1);
//   3. stoc per produs × gestiune (cantitate + valoare) = BalantaNivel3 agregat.
//
// Ce se acceptă ca DIFERENȚĂ JUSTIFICATĂ, și de ce (§8.3 amendat, 47d/48a):
// netarea deschiderii a rearanjat cantitățile ȘI prețurile în interiorul grupei
// produs × depozit. Cantitățile s-au conservat exact, prețurile NU — deci Atlas
// descarcă la alt cost decât 1C. Consecința se vede în două locuri, și numai în
// două: valoarea stocului produselor atinse de netare (contractul 3) și perechea
// „ce a rămas în stoc / ce a ieșit pe cost" din contabilitate (contractul 1).
// Justificarea nu e o toleranță lărgită, ci o afirmație verificabilă:
//   (a) suma abaterilor bucket-ului ≈ 0 — banii nu s-au pierdut, s-au mutat între
//       stoc și cost (dacă suma NU e zero, e o diferență reală, deci eșec);
//   (b) fiecare abatere ≤ plafonul dat de diferența de valoare a stocului
//       justificat cumulat la zi — abaterea contabilă nu poate depăși valoarea
//       pe care netarea a rearanjat-o.
// Orice cont din afara bucket-ului care deviază = EȘEC, oricât de mic.
static class ReconciliereLuna {
    const decimal EpsV = 0.005m;
    const decimal EpsQ = 0.0005m;

    // Scara la care se rotunjesc valorile ÎNAINTE de agregarea server-side.
    // Nu e 2 (banul), deși toate cifrele comparate sunt bani: valoarea unei linii
    // e preț de lot × cantitate și poate avea sub-bani reali, iar o rotunjire la
    // ban pe FIECARE rând ar introduce, pe zeci de mii de rânduri, o abatere mai
    // mare decât toleranța contractului — adică exact diferența pe care
    // reconcilierea trebuie s-o poată vedea. La 8 zecimale eroarea introdusă e
    // sub 1e-8 per rând (invizibilă la orice agregare realistă), iar scara
    // rezultatului rămâne mult sub mantisa lui `decimal`.
    const int Scara = 8;

    // Conturile în care efectul netării are voie să apară. Lista e SCURTĂ și
    // explicită (nu o regulă generoasă): stocurile, unde stă valoarea rearanjată;
    // conturile de cost 60x, unde iese la descărcare (perechea derivată 6xx = 3xx);
    // și 401, contrapartida stornării transcrise a retururilor fără marfă în stoc
    // (RLF: 1C descarcă un lot pe care netarea l-a absorbit, Atlas transcrie
    // contabil fără mișcare de stoc). Măsurat pe ianuarie: tripleta 371 / 607 /
    // 401 cu suma exact zero.
    //
    // Riscul asumat, scris ca să fie văzut: o eroare REALĂ pe unul din conturile
    // astea, mai mică decât plafonul, ar trece drept justificată. De aceea
    // abaterile se raportează per cont per lună cu cifra lor — nu se ascund în
    // spatele unui „ok".
    internal static bool EsteInBucketNetare(string simbol) =>
        simbol.StartsWith('3') || simbol.StartsWith("60") || simbol == "401";

    // Starea purtată de la o lună la alta (§12.4): fără ea, o diferență din
    // ianuarie ar fi raportată integral în toate lunile următoare, iar raportul
    // lui decembrie ar fi ilizibil.
    public sealed class Stare {
        // Din deschidere (pasul 3): produsele atinse de netare + diferențele deja
        // raportate ale sursei (grupe total-negative, poziții orfane).
        public IReadOnlySet<string> ProduseNetate = new HashSet<string>(StringComparer.Ordinal);
        public IReadOnlyList<Deschidere.DiferentaSursa> JustificateDeschidere = [];
        public IReadOnlySet<string> Extrabilantiere1C = new HashSet<string>(StringComparer.Ordinal);

        // Plafonul (b): valoarea de stoc justificată, cumulată la zi. E maximum
        // ISTORIC, nu valoarea lunii: o grupă care se golește face să dispară
        // diferența din stoc, dar efectul ei rămâne pe contul de cost până la
        // sfârșitul anului.
        public decimal PlafonStoc;

        // Ce s-a raportat deja, ca să se poată spune „purtată" în loc s-o repete:
        // abaterea per cont și per cheie de stoc, din luna precedentă.
        public readonly Dictionary<string, decimal> AbateriConturi = new(StringComparer.Ordinal);
        public readonly Dictionary<(string P, string D), (decimal Q, decimal V)> AbateriStoc = [];
    }

    public record Rezultat(int Contracte, int Picate, int AbateriJustificate,
        decimal PlafonStoc, decimal TvaDePlata, decimal TvaDeRecuperat);

    public static Rezultat Executa(ContextLuna ctx, Stare stare,
            Action<string> avert, Action<string, bool> check) {
        var bucla = ctx.Bucla;
        var cat = bucla.Catalog;
        using var os = bucla.CreeazaObjectSpace();

        Console.WriteLine($"  --- Reconcilierea lunii {ctx.Luna:00}/{ctx.An} "
            + $"(la {ctx.Ultima:yyyy-MM-dd}) ---");

        // Contractul 3 se calculează PRIMUL, deși se raportează ultimul: plafonul
        // pe care îl produce e intrarea contractului 1.
        var stoc = Stoc(os, ctx, stare, avert);
        stare.PlafonStoc = Math.Max(stare.PlafonStoc, stoc.Justificat);

        var picate = 0;
        void Contract(string nume, bool ok) {
            check($"  [{ctx.Luna:00}/{ctx.An}] {nume}", ok);
            if (!ok)
                picate++;
        }

        Contabil(os, ctx, stare, cat, avert, Contract);
        var (dePlata, deRecuperat) = Tva(os, ctx, cat, avert, Contract);
        RaporteazaStoc(stoc, ctx, stare, avert, Contract);

        return new Rezultat(3, picate, stoc.Justificate, stare.PlafonStoc, dePlata, deRecuperat);
    }

    // ==================== 1. Sold per cont sintetic OMFP ====================

    static void Contabil(IObjectSpace os, ContextLuna ctx, Stare stare, Catalog cat,
            Action<string> avert, Action<string, bool> contract) {
        // ---- Baza: TOATE rândurile contabile până la fine de lună, agregate
        // server-side. `Math.Round(…, Scara)` nu e cosmetică: valorile
        // materializate de motor moștenesc scara împărțirii care le-a produs
        // (PretUnitar = net / cantitate ⇒ 25 de zecimale), iar SUM-ul
        // server-side al câtorva sute de mii de asemenea numere depășește
        // mantisa lui `decimal` (același defect de Module semnalat la
        // imperecheri). Vezi `Scara` pentru de ce 8 și nu 2.
        var simbolPeId = cat.Plan.ToDictionary(x => x.Value, x => x.Key);
        var db = new Dictionary<string, decimal>(StringComparer.Ordinal);
        void Acumuleaza(Guid contId, decimal suma) {
            if (!simbolPeId.TryGetValue(contId, out var simbol))
                return;
            db[simbol] = db.GetValueOrDefault(simbol) + suma;
        }
        foreach (var g in os.GetObjectsQuery<RegistruContabil>()
                     .Where(r => r.Data <= ctx.Ultima)
                     .GroupBy(r => r.ContDebitId)
                     .Select(g => new { Cont = g.Key, Suma = g.Sum(r => Math.Round(r.Valoare, Scara)) })
                     .ToList())
            Acumuleaza(g.Cont, g.Suma);
        foreach (var g in os.GetObjectsQuery<RegistruContabil>()
                     .Where(r => r.Data <= ctx.Ultima)
                     .GroupBy(r => r.ContCreditId)
                     .Select(g => new { Cont = g.Key, Suma = g.Sum(r => Math.Round(r.Valoare, Scara)) })
                     .ToList())
            Acumuleaza(g.Cont, -g.Suma);

        // ---- Sursa: Balanța 1C la fine de lună (= SoldIni al lunii următoare) ----
        var sursa = new Dictionary<string, decimal>(StringComparer.Ordinal);
        var nemapate = new List<FlaxSold>();
        foreach (var s in ctx.Bucla.Flax.SolduriLaFineDeLuna(ctx.An, ctx.Luna)) {
            if (stare.Extrabilantiere1C.Contains(s.Cont))
                continue;
            var simbol = cat.Mapeaza(s.Cont);
            if (simbol == null) {
                nemapate.Add(s);
                continue;
            }
            sursa[simbol] = sursa.GetValueOrDefault(simbol) + s.SoldIni;
        }
        if (nemapate.Count > 0)
            foreach (var s in nemapate.OrderByDescending(s => Math.Abs(s.SoldIni)))
                avert($"[{ctx.Luna:00}/{ctx.An}] cont 1C {s.Cont} (sold {s.SoldIni:N2}) nu se mapează "
                    + "pe planul OMFP — banii lui lipsesc din comparație.");

        // ---- Diferențele ----
        var abateri = db.Keys.Union(sursa.Keys)
            .Select(s => (Simbol: s, Db: db.GetValueOrDefault(s), Sursa: sursa.GetValueOrDefault(s)))
            .Select(x => (x.Simbol, x.Db, x.Sursa, Delta: x.Db - x.Sursa))
            .Where(x => Math.Abs(x.Delta) >= EpsV)
            .OrderByDescending(x => Math.Abs(x.Delta))
            .ToList();

        var inBucket = abateri.Where(x => EsteInBucketNetare(x.Simbol)).ToList();
        var inafara = abateri.Where(x => !EsteInBucketNetare(x.Simbol)).ToList();

        // Condiția (a): bucket-ul se închide la zero — banii s-au mutat între stoc
        // și cost, nu s-au pierdut. Un bucket care NU se închide nu mai e efectul
        // netării, ci o diferență reală care se întâmplă să cadă pe conturile ei.
        var sumaBucket = inBucket.Sum(x => x.Delta);
        var bucketSeInchide = Math.Abs(sumaBucket) < EpsV * Math.Max(1, inBucket.Count);
        // Condiția (b): nicio abatere nu depășește valoarea rearanjată de netare.
        var pestePlafon = inBucket.Where(x => Math.Abs(x.Delta) > stare.PlafonStoc + EpsV).ToList();

        var justificate = bucketSeInchide && pestePlafon.Count == 0 ? inBucket : [];
        var picate = inafara
            .Concat(bucketSeInchide ? pestePlafon : inBucket)
            .OrderByDescending(x => Math.Abs(x.Delta))
            .ToList();

        foreach (var x in picate)
            contract($"  cont {x.Simbol}: bază {x.Db:N2} = sursă 1C {x.Sursa:N2} (Δ {x.Delta:N2})"
                + (EsteInBucketNetare(x.Simbol)
                    ? bucketSeInchide
                        ? $" — în bucket-ul netării, dar peste plafonul de {stare.PlafonStoc:N2}"
                        : $" — bucket-ul netării NU se închide la zero (Σ {sumaBucket:N2})"
                    : ""), false);

        foreach (var x in justificate) {
            var purtata = stare.AbateriConturi.GetValueOrDefault(x.Simbol);
            var nou = x.Delta - purtata;
            avert($"[{ctx.Luna:00}/{ctx.An}] cont {x.Simbol}: bază {x.Db:N2} vs sursă 1C {x.Sursa:N2} "
                + $"(Δ {x.Delta:N2}"
                + (Math.Abs(nou) < EpsV && purtata != 0 ? ", purtată neschimbată din luna trecută"
                    : purtata == 0 ? ", nouă" : $", din care {nou:N2} nou în luna asta")
                + ") — diferență justificată de netarea deschiderii (§8.3).");
        }
        stare.AbateriConturi.Clear();
        foreach (var x in justificate)
            stare.AbateriConturi[x.Simbol] = x.Delta;

        contract($"1. Sold per cont OMFP: {db.Keys.Union(sursa.Keys).Count()} simboluri, "
            + $"{picate.Count} diferențe nejustificate, {justificate.Count} justificate de netare "
            + $"(Σ bucket {sumaBucket:N2}, plafon {stare.PlafonStoc:N2}), {nemapate.Count} conturi 1C nemapate",
            picate.Count == 0 && nemapate.Count == 0);
    }

    // ==================== 2. Închiderea de TVA (4423/4424) ====================

    static (decimal DePlata, decimal DeRecuperat) Tva(IObjectSpace os, ContextLuna ctx, Catalog cat,
            Action<string> avert, Action<string, bool> contract) {
        var simbolPeId = cat.Plan.ToDictionary(x => x.Value, x => x.Key);

        // ---- Baza: rândurile documentelor ITV ale lunii (recitite din registru,
        // nu din obiectul generat) ----
        var itv = os.GetObjectsQuery<InchidereTva>()
            .Where(d => d.Data >= ctx.Prima && d.Data <= ctx.Ultima && d.Stare == StareDocument.Operat)
            .Select(d => d.ID)
            .ToList();
        var db = new Dictionary<(string D, string C), decimal>();
        if (itv.Count > 0)
            foreach (var r in os.GetObjectsQuery<RegistruContabil>()
                         .Where(r => r.DocumentId != null && itv.Contains(r.DocumentId.Value))
                         .Select(r => new { r.ContDebitId, r.ContCreditId, r.Valoare })
                         .ToList()) {
                var cheie = (simbolPeId.GetValueOrDefault(r.ContDebitId, "?"),
                    simbolPeId.GetValueOrDefault(r.ContCreditId, "?"));
                db[cheie] = db.GetValueOrDefault(cheie) + r.Valoare;
            }

        // ---- Sursa: rândurile de TVA ale închiderii 1C, exact cele sărite la import ----
        var sursa = new Dictionary<(string D, string C), decimal>();
        foreach (var (contDebit, contCredit, suma) in ctx.Bucla.Flax.SumeInchidereLuna(ctx.An, ctx.Luna)) {
            var d = cat.Mapeaza(contDebit);
            var c = cat.Mapeaza(contCredit);
            if (d == null || c == null || !NoteComune.EsteRandDeInchidereTva(d, c))
                continue;
            sursa[(d, c)] = sursa.GetValueOrDefault((d, c)) + suma;
        }

        var perechi = db.Keys.Union(sursa.Keys).OrderBy(k => k.D).ThenBy(k => k.C).ToList();
        var diferente = 0;
        foreach (var k in perechi) {
            var vDb = db.GetValueOrDefault(k);
            var vSursa = sursa.GetValueOrDefault(k);
            var ok = Math.Abs(vDb - vSursa) < EpsV;
            if (!ok)
                diferente++;
            Console.WriteLine($"     {(ok ? "  " : "!!")} {k.D} = {k.C,-6} Atlas {vDb,15:N2}   1C {vSursa,15:N2}"
                + (ok ? "" : $"   Δ {vDb - vSursa:N2}"));
        }
        contract($"2. Închiderea de TVA: {perechi.Count} corespondențe comparate, {diferente} diferențe "
            + $"(ITV generate: {itv.Count})", diferente == 0);

        var dePlata = db.Where(x => x.Key.C == "4423").Sum(x => x.Value);
        var deRecuperat = db.Where(x => x.Key.D == "4424").Sum(x => x.Value);
        // Fapt al anului 2025 verificat pe sursă: TVA de recuperat nu apare
        // NICIODATĂ. Un 4424 generat de Atlas ar însemna că soldurile de TVA s-au
        // inversat față de 1C — se strigă, chiar dacă perechile de mai sus au bătut.
        if (deRecuperat != 0m)
            avert($"[{ctx.Luna:00}/{ctx.An}] închiderea Atlas a generat TVA de recuperat "
                + $"({deRecuperat:N2}) — 1C nu are 4424 în 2025.");
        return (dePlata, deRecuperat);
    }

    // ==================== 3. Stoc per produs × gestiune ====================

    sealed record RezultatStoc(int Chei, int Nepotriviri, int Justificate,
        decimal Justificat, List<(string P, string D, decimal QDb, decimal VDb, decimal QSursa,
            decimal VSursa, string Motiv)> Detalii,
        decimal ValoareAltRegistru, decimal CantitateAltRegistru);

    // Registrele de stoc care au corespondent în sursă. `BalantaNivel3` e
    // defalcarea conturilor 3xx, deci comparabile sunt DOAR registrele care
    // oglindesc soldul de pe 3xx: Magazie și Mărfuri (maparea profilului privat —
    // generic → Magazie, MF → Mărfuri). `Consum` NU are corespondent și nici n-ar
    // putea avea: e mecanismul Atlas prin care consumul rămâne pe responsabilul
    // locului (27a) — în 1C marfa a ieșit pur și simplu din 3xx. Restul
    // (Folosință, Custodie, Gratuit, ProducțieNeterminată) n-au reguli în profilul
    // privat azi; dacă apar, intră în același raport de volum exclus, nu tăcut.
    static readonly TipStoc[] RegistreComparabile = [TipStoc.Magazie, TipStoc.Marfuri];

    static RezultatStoc Stoc(IObjectSpace os, ContextLuna ctx, Stare stare, Action<string> avert) {
        // ---- Baza: registrul de stoc cumulat la fine de lună ----
        // Gruparea se face pe (lot, repartitor) și se traduce în produs pe urmă:
        // gruparea directă pe `r.Lot.ProdusId` ar cere un join în agregare, iar
        // dicționarul de loturi e oricum ieftin (zeci de mii de rânduri).
        var produsPeLot = os.GetObjectsQuery<Lot>()
            .Select(l => new { l.ID, l.ProdusId })
            .ToList()
            .ToDictionary(l => l.ID, l => l.ProdusId);
        var randuri = os.GetObjectsQuery<RegistruStoc>()
            .Where(r => r.Data <= ctx.Ultima && RegistreComparabile.Contains(r.TipStoc))
            .GroupBy(r => new { r.LotId, r.RepartitorId })
            .Select(g => new {
                g.Key.LotId, g.Key.RepartitorId,
                Q = g.Sum(r => Math.Round(r.Cantitate, Scara)),
                V = g.Sum(r => Math.Round(r.Valoare, Scara)),
            })
            .ToList();
        // Volumul EXCLUS se raportează, ca excluderea să fie o afirmație
        // verificabilă, nu o tăcere: e soldul registrului de consum, adică exact
        // marfa pe care 1C a scos-o din 3xx și Atlas o ține mai departe pe
        // responsabilul locului de consum.
        var altRegistru = os.GetObjectsQuery<RegistruStoc>()
            .Where(r => r.Data <= ctx.Ultima && !RegistreComparabile.Contains(r.TipStoc))
            .GroupBy(r => r.TipStoc)
            .Select(g => new {
                Registru = g.Key,
                Q = g.Sum(r => Math.Round(r.Cantitate, Scara)),
                V = g.Sum(r => Math.Round(r.Valoare, Scara)),
            })
            .ToList();
        if (altRegistru.Count > 0)
            Console.WriteLine($"     registre fără corespondent în BalantaNivel3, excluse din contract: "
                + string.Join(", ", altRegistru.Select(x => $"{x.Registru} {x.Q:N3} buc / {x.V:N2} lei")));

        var produsHex = Reconciliere.Inverseaza(os, "Nomenclator", avert);
        var depozitHex = Reconciliere.Inverseaza(os, "Depozite", avert);

        var db = new Dictionary<(string P, string D), (decimal Q, decimal V)>();
        foreach (var r in randuri) {
            var p = produsPeLot.TryGetValue(r.LotId, out var produsId)
                ? produsHex.GetValueOrDefault(produsId) ?? $"(produs nelegat {produsId})"
                : $"(lot necunoscut {r.LotId})";
            var d = depozitHex.GetValueOrDefault(r.RepartitorId) ?? $"(gestiune nelegată {r.RepartitorId})";
            var acum = db.GetValueOrDefault((p, d));
            db[(p, d)] = (acum.Q + r.Q, acum.V + r.V);
        }

        // ---- Sursa: BalantaNivel3 la fine de lună, plus pozițiile orfane ----
        // Pozițiile BRUTE (per LOT) se păstrează: agregarea pe produs × depozit
        // ascunde exact ce trebuie măsurat mai jos — celula negativă a sursei se
        // netează cu o celulă pozitivă a aceleiași perechi și devine invizibilă.
        var pozitii = ctx.Bucla.Flax.StocLaFineDeLuna(ctx.An, ctx.Luna)
            .Concat(ctx.Bucla.Flax.StocFaraIdentitateLaFineDeLuna(ctx.An, ctx.Luna))
            .ToList();
        var sursa = new Dictionary<(string P, string D), (decimal Q, decimal V)>();
        foreach (var p in pozitii) {
            var cheie = (p.NomenclatorId, p.DepozitId);
            var acum = sursa.GetValueOrDefault(cheie);
            sursa[cheie] = (acum.Q + p.Cantitate, acum.V + p.Valoare);
        }

        // Cheile deja explicate la deschidere (grupe total-negative, poziții
        // orfane): rămân diferite tot anul, prin construcție.
        var deschidere = stare.JustificateDeschidere
            .Select(j => (j.ProdusHex, j.DepozitHex)).ToHashSet();

        // Produsele pe care supapa 48a le-a atins efectiv, traduse în identitatea
        // 1C. Mulțimea vine din EVIDENȚA rulării (`AlocareIesire`), nu dintr-o
        // presupunere despre ce ar fi putut atinge netarea.
        var realocateHex = ctx.Bucla.Alocare.ProduseRealocate
            .Select(id => produsHex.GetValueOrDefault(id))
            .Where(h => h != null)
            .ToHashSet(StringComparer.Ordinal);

        // CELULELE NEGATIVE ALE SURSEI ÎN CURSUL ANULUI. Netarea din 1C-b a
        // rezolvat artefactul retur-ca-lot la DESCHIDERE, dar 1C îl produce în
        // continuare: o ieșire pe un lot pe care depozitul nu-l are intră pur și
        // simplu pe minus în celula lui. Atlas nu poate reprezenta asta
        // (gardianul cere sold ≥ 0 per lot × repartitor), și de aici DOUĂ efecte
        // măsurate, amândouă ale aceluiași artefact:
        //  * supapa de import (48a) realocă ieșirea pe alt lot al produsului ⇒
        //    marfa e la Atlas în altă gestiune decât spune sursa, dar totalul pe
        //    produs bate exact (perechi de chei oglindite);
        //  * ieșirea NU are acoperire nicăieri în produs (retur fără marfă în
        //    stoc — RLF): Atlas transcrie doar contabil, stocul nu se mișcă ⇒
        //    Atlas rămâne cu marfa pe care 1C a scos-o pe minus.
        //
        // Justificarea e o mărime a SURSEI, nu o toleranță: abaterea pe produs
        // trebuie să încapă în valoarea absolută a celulelor negative pe care
        // sursa le are chiar ea pe produsul ăla. Zero (oglindirea) și „exact cât
        // negativul" (returul neurmat) sunt capetele intervalului; peste el,
        // nimic nu se justifică.
        var negativeSursa = pozitii
            .Where(p => p.Cantitate < -EpsQ || p.Valoare < -EpsV)
            .GroupBy(p => p.NomenclatorId, StringComparer.Ordinal)
            .ToDictionary(g => g.Key,
                g => (Q: g.Sum(p => Math.Abs(p.Cantitate)), V: g.Sum(p => Math.Abs(p.Valoare))),
                StringComparer.Ordinal);
        var totalDb = db.GroupBy(x => x.Key.P, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => (Q: g.Sum(x => x.Value.Q), V: g.Sum(x => x.Value.V)),
                StringComparer.Ordinal);
        var totalSursa = sursa.GroupBy(x => x.Key.P, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => (Q: g.Sum(x => x.Value.Q), V: g.Sum(x => x.Value.V)),
                StringComparer.Ordinal);
        (decimal Q, decimal V) DeltaProdus(string produs) {
            var (qDb, vDb) = totalDb.GetValueOrDefault(produs);
            var (qSursa, vSursa) = totalSursa.GetValueOrDefault(produs);
            return (qDb - qSursa, vDb - vSursa);
        }
        bool IncapeInNegativulSursei(string produs) {
            if (!negativeSursa.TryGetValue(produs, out var negativ))
                return false;
            var (dq, dv) = DeltaProdus(produs);
            return Math.Abs(dq) <= negativ.Q + EpsQ && Math.Abs(dv) <= negativ.V + EpsV;
        }

        // Pozițiile ORFANE ale sursei: fără produs sau fără depozit (id-ul 1C e
        // plin de zerouri). Nu pot deveni lot în Atlas (decizia 13) — deschiderea
        // le declară deja diferență a sursei, iar în cursul anului rămân la fel.
        const string IdGol = "00000000000000000000000000000000";

        var detalii = new List<(string P, string D, decimal QDb, decimal VDb, decimal QSursa,
            decimal VSursa, string Motiv)>();
        var nepotriviri = 0;
        var justificate = 0;
        foreach (var k in db.Keys.Union(sursa.Keys)) {
            var (qDb, vDb) = db.GetValueOrDefault(k);
            var (qSursa, vSursa) = sursa.GetValueOrDefault(k);
            var dq = qDb - qSursa;
            var dv = vDb - vSursa;
            if (Math.Abs(dq) < EpsQ && Math.Abs(dv) < EpsV)
                continue;
            // Cantitatea e STRICTĂ peste tot (§8.3 amendat), cu două excepții
            // amândouă ale SURSEI: pozițiile pe care deschiderea le-a declarat
            // deja nereprezentabile, și celulele negative intra-an de mai sus.
            // Rearanjarea prețurilor de către netare justifică DOAR valoarea —
            // acolo cantitatea trebuie să fie exactă.
            var motiv = k.P == IdGol || k.D == IdGol || k.P == null || k.D == null
                ? "poziție orfană a sursei (fără produs sau fără depozit — nereprezentabilă ca lot)"
                : deschidere.Contains(k)
                    ? "diferență a sursei, raportată la deschidere (grupă total-negativă / poziție orfană)"
                    : IncapeInNegativulSursei(k.P)
                        ? "celulă negativă în 1C intra-an (realocare 48a sau retur fără acoperire); "
                            + $"abaterea pe produs ({DeltaProdus(k.P).Q:N3} buc / {DeltaProdus(k.P).V:N2} lei) "
                            + $"încape în negativul sursei ({negativeSursa[k.P].Q:N3} buc / "
                            + $"{negativeSursa[k.P].V:N2} lei)"
                        : Math.Abs(dq) < EpsQ
                                && (stare.ProduseNetate.Contains(k.P) || realocateHex.Contains(k.P))
                            ? "cost per lot rearanjat, cantitate exactă (netarea deschiderii sau "
                                + "realocarea supapei 48a)"
                            : null;
            if (motiv == null)
                nepotriviri++;
            else
                justificate++;
            detalii.Add((k.P, k.D, qDb, vDb, qSursa, vSursa, motiv));
        }

        // Plafonul contractului (1) numără doar ce SE SCURGE din stoc în
        // contabilitate: abaterea NETĂ pe produs. Perechile oglindite se anulează
        // în interiorul produsului (Δ = 0) și n-au mișcat niciun sold de cont —
        // incluse per cheie, ar lărgi plafonul cu bani care nu există nicăieri.
        var justificat = detalii.Where(d => d.Motiv != null)
            .GroupBy(d => d.P, StringComparer.Ordinal)
            .Sum(g => Math.Abs(g.Sum(d => d.VDb - d.VSursa)));

        return new RezultatStoc(db.Keys.Union(sursa.Keys).Count(), nepotriviri, justificate,
            justificat, detalii,
            altRegistru.Sum(x => x.V), altRegistru.Sum(x => x.Q));
    }

    // Câte chei justificate se detaliază per lună: raportul trebuie să rămână
    // citibil (sute de grupe netate), iar cifra agregată nu se pierde.
    const int DetaliiJustificate = 15;

    static void RaporteazaStoc(RezultatStoc stoc, ContextLuna ctx, Stare stare,
            Action<string> avert, Action<string, bool> contract) {
        foreach (var d in stoc.Detalii.Where(d => d.Motiv == null)
                     .OrderByDescending(d => Math.Abs(d.VDb - d.VSursa)).Take(30))
            contract($"  stoc produs {d.P} × gestiune {d.D}: bază {d.QDb:N3} buc / {d.VDb:N2} lei "
                + $"= sursă {d.QSursa:N3} buc / {d.VSursa:N2} lei "
                + $"(Δ {d.QDb - d.QSursa:N3} buc / {d.VDb - d.VSursa:N2} lei)", false);

        // Purtarea înainte: se detaliază doar cheile NOI sau schimbate față de
        // luna trecută; restul se numără.
        var noi = 0;
        var purtate = 0;
        foreach (var d in stoc.Detalii.Where(d => d.Motiv != null)
                     .OrderByDescending(d => Math.Abs(d.VDb - d.VSursa))) {
            var acum = (Q: d.QDb - d.QSursa, V: d.VDb - d.VSursa);
            var inainte = stare.AbateriStoc.GetValueOrDefault((d.P, d.D));
            if (Math.Abs(acum.Q - inainte.Q) < EpsQ && Math.Abs(acum.V - inainte.V) < EpsV) {
                purtate++;
                continue;
            }
            noi++;
            if (noi <= DetaliiJustificate)
                avert($"[{ctx.Luna:00}/{ctx.An}] stoc produs {d.P} × gestiune {d.D}: "
                    + $"bază {d.QDb:N3} buc / {d.VDb:N2} lei vs sursă {d.QSursa:N3} buc / {d.VSursa:N2} lei "
                    + $"(Δ {acum.Q:N3} buc / {acum.V:N2} lei) — {d.Motiv}.");
        }
        if (noi > DetaliiJustificate)
            avert($"[{ctx.Luna:00}/{ctx.An}] încă {noi - DetaliiJustificate} chei de stoc cu diferență "
                + "justificată nedetaliate (contorul de mai jos le are pe toate).");

        stare.AbateriStoc.Clear();
        foreach (var d in stoc.Detalii.Where(d => d.Motiv != null))
            stare.AbateriStoc[(d.P, d.D)] = (d.QDb - d.QSursa, d.VDb - d.VSursa);

        // Triajul pe categorii: „câte sunt" nu spune nimic, „de ce sunt" spune tot.
        foreach (var g in stoc.Detalii.Where(d => d.Motiv != null)
                     .GroupBy(d => d.Motiv).OrderByDescending(g => g.Count()))
            Console.WriteLine($"     {g.Count(),6} chei justificate — {g.Key} "
                + $"(Σ |Δ| {g.Sum(d => Math.Abs(d.VDb - d.VSursa)):N2} lei)");

        contract($"3. Stoc per produs × gestiune: {stoc.Chei} chei comparate, {stoc.Nepotriviri} "
            + $"nepotriviri nejustificate ({stoc.Justificate} justificate — Σ {stoc.Justificat:N2} lei "
            + $"în plafon, din care {noi} noi / {purtate} purtate; "
            + $"{stoc.ValoareAltRegistru:N2} lei în registre necomparabile)", stoc.Nepotriviri == 0);
    }
}
