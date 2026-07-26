using Atlas.Conta.BackOffice.Module.BusinessObjects;
using DevExpress.ExpressApp;

namespace Import1C;

// PASUL 6 al feliei 1C-c: CONTRACTUL DE RECONCILIERE PER LUNĂ (design §8, cu
// amendamentul §8.3 și regula de purtare înainte din §12.4), rescris la pasul 4
// al lotului de robustețe pre-1C-d ca să repare defectul D6 (49f).
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
// CE S-A SCHIMBAT LA D6 — justificarea nu mai e euristică, e MĂSURATĂ.
//
// Până acum, o diferență era declarată justificată dacă ARĂTA plauzibil: cădea pe
// un prefix de cont din „bucket-ul netării" și încăpea sub un plafon GLOBAL
// (valoarea de stoc justificată, cumulată pe tot anul), sau, la stoc, dacă
// „încăpea în negativul sursei" pe produs. Ambele mint. Un document de stoc
// pierdut cu totul produce exact o pereche 6xx/3xx cu suma zero: bucket-ul „se
// închide", plafonul îl acoperă, nimeni nu-l vede. Invers, o linie aruncată
// legitim rămâne nejustificată când artefactul sursei care a cauzat-o nu se mai
// vede la finele lunii (celula negativă TRANZITORIE — cazul S24 Ultra).
//
// Acum unealta ÎNREGISTREAZĂ, la locul faptei, fiecare linie pe care a
// aruncat-o sau a descărcat-o parțial (`RegistruDivergente`), iar contractul
// întreabă o EGALITATE: „este Δ-ul cheii egal cu suma liniilor pe care le-am
// aruncat chiar eu pe cheia asta?". Consecințe:
//   * cantitatea rămâne STRICTĂ peste tot (§8.3 amendat) și devine
//     DISCRIMINANTUL: un document de stoc pierdut lasă marfă în plus în Atlas
//     fără nicio înregistrare corespondentă, deci pică zgomotos la contractul 3;
//   * valoarea, acolo unde cantitatea bate exact, rămâne justificată de netarea
//     deschiderii (§8.3) — dar numai pe produsele pe care netarea sau supapa
//     48a chiar le-au atins, și se raportează cu cifra ei;
//   * plafonul GLOBAL moare. Contractul de sold explică fiecare cont din
//     registru + din divergența de stoc MĂSURATĂ pe contul ăla (și pe oglinda
//     lui de cheltuială, citită din politici) — un cont fără explicație e eșec,
//     oricât de mic.
//
// Limitarea, scrisă ca să fie văzută: o statement de VALOARE nu poate deosebi
// „am descărcat la alt cost" de „am pierdut documentul" — ambele mută aceeași
// sumă între stoc și cheltuială. Puterea de discriminare stă în contractul 3
// (cantitatea măsurată), nu în contractul 1. De aceea contul rămâne justificabil
// prin plafonul lui MĂSURAT, iar detecția documentului pierdut e treaba
// cantității.
static class ReconciliereLuna {
    const decimal EpsV = 0.005m;
    const decimal EpsQ = 0.0005m;

    // Reziduul de rotunjire acceptat pe o cheie de stoc cu cantitate EXACTĂ:
    // valoarea Atlas e Σ round(cantitate × preț unitar, 2) pe rândurile de
    // registru, iar sursa își ține propria valoare exactă — cele două se despart
    // cu bani mărunți. Măsurat pe ianuarie: 103 chei între ±0,01 și ±0,03.
    // Se raportează AGREGAT, cu suma algebrică: un pumn de reziduuri e zgomot de
    // rotunjire doar dacă se compensează; dacă merg toate în același sens, nu mai
    // e rotunjire, e o eroare sistematică — și se strigă.
    const decimal EpsRotunjire = 0.03m;
    const decimal PragRotunjireSistematica = 1m;

    // Toleranța la comparațiile cu praguri EXACTE ale sursei (negativul pe
    // produs): sursa își rotunjește propriile cifre, deci un prag exact rupe
    // pe 3 bani (cazul 34,98 vs 35,01 din ianuarie).
    const decimal EpsPrag = 0.05m;

    // Scara la care se rotunjesc valorile ÎNAINTE de agregarea server-side.
    // Nu e 2 (banul), deși toate cifrele comparate sunt bani: valoarea unei linii
    // e preț de lot × cantitate și poate avea sub-bani reali, iar o rotunjire la
    // ban pe FIECARE rând ar introduce, pe zeci de mii de rânduri, o abatere mai
    // mare decât toleranța contractului — adică exact diferența pe care
    // reconcilierea trebuie s-o poată vedea. La 8 zecimale eroarea introdusă e
    // sub 1e-8 per rând (invizibilă la orice agregare realistă), iar scara
    // rezultatului rămâne mult sub mantisa lui `decimal`.
    const int Scara = 8;

    // Rămasă DOAR pentru alegerea rândului de sabotat (`--sabotaj`): proba trebuie
    // să lovească un cont care nu poate fi acoperit de nicio divergență de
    // evaluare. Contractul nu mai folosește prefixe — autoritatea e măsurătoarea.
    internal static bool EsteInBucketNetare(string simbol) =>
        simbol.StartsWith('3') || simbol.StartsWith("60") || simbol == "401";

    // Starea purtată de la o lună la alta (§12.4): fără ea, o diferență din
    // ianuarie ar fi raportată integral în toate lunile următoare, iar raportul
    // lui decembrie ar fi ilizibil. Purtarea CIFRELOR nu mai trece pe aici —
    // registrul divergențelor e cumulativ prin construcție, deci verdictul e
    // același la rularea care scrie și la cea care doar recitește (D).
    public sealed class Stare {
        // Din deschidere (pasul 3): produsele atinse de netare + diferențele deja
        // raportate ale sursei (grupe total-negative, poziții orfane).
        public IReadOnlySet<string> ProduseNetate = new HashSet<string>(StringComparer.Ordinal);
        public IReadOnlyList<Deschidere.DiferentaSursa> JustificateDeschidere = [];
        public IReadOnlySet<string> Extrabilantiere1C = new HashSet<string>(StringComparer.Ordinal);

        // Doar pentru raport: valoarea de stoc justificată, cumulată la zi. NU mai
        // e plafon (D6) — a rămas cifra care spune cât de mare e efectul netării.
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

        // Registrul divergențelor CUNOSCUTE, cumulat la zi: intrarea amânduror
        // contractelor. E persistat, deci identic la o rulare care nu mai importă
        // nimic — de asta verdictul e determinist (D).
        var registru = bucla.Divergente.PanaLa(ctx.An, ctx.Luna);

        // Contractul 3 se calculează PRIMUL, deși se raportează ultimul: divergența
        // de stoc pe care o măsoară e intrarea contractului 1.
        var stoc = Stoc(os, ctx, stare, registru, avert);
        stare.PlafonStoc = Math.Max(stare.PlafonStoc, stoc.Justificat);

        var picate = 0;
        void Contract(string nume, bool ok) {
            check($"  [{ctx.Luna:00}/{ctx.An}] {nume}", ok);
            if (!ok)
                picate++;
        }

        Contabil(os, ctx, stare, cat, registru, stoc, avert, Contract);
        var (dePlata, deRecuperat) = Tva(os, ctx, cat, avert, Contract);
        RaporteazaStoc(stoc, ctx, stare, avert, Contract);

        return new Rezultat(3, picate, stoc.Justificate, stare.PlafonStoc, dePlata, deRecuperat);
    }

    // ==================== 1. Sold per cont sintetic OMFP ====================

    static void Contabil(IObjectSpace os, ContextLuna ctx, Stare stare, Catalog cat,
            IReadOnlyList<Divergenta> registru, RezultatStoc stoc,
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

        // ---- Explicațiile MĂSURATE, per cont ----
        // (a) Registrul: ce anume din rândurile sursei NU postează Atlas deloc.
        // Convenția e a sursei: 1C mișcă debitul cu +v și creditul cu −v, iar
        // Atlas nu mișcă nimic ⇒ Δ(Atlas − 1C) e exact opusul.
        var explicat = new Dictionary<string, decimal>(StringComparer.Ordinal);
        var inregistrari = new Dictionary<string, int>(StringComparer.Ordinal);
        void Explica(string cont, decimal suma) {
            if (cont == null)
                return;
            explicat[cont] = explicat.GetValueOrDefault(cont) + suma;
            inregistrari[cont] = inregistrari.GetValueOrDefault(cont) + 1;
        }
        foreach (var d in registru.Where(d => d.ValoareNepostata != 0m)) {
            Explica(d.ContDebit, -d.ValoareNepostata);
            Explica(d.ContCredit, d.ValoareNepostata);
        }

        // (b) Plafonul MĂSURAT al contului: diferența de EVALUARE, adică exact cât
        // s-a rearanjat sub el în registrul de stoc (contul de stoc) sau sub
        // conturile de stoc a căror oglindă de cheltuială este (citite din
        // politici, nu presupuse). Plus valoarea liniilor pe care puntea le-a
        // transcris la valoarea SURSEI deși marfa a rămas în Atlas — acolo
        // contabilitatea e a sursei, iar stocul e al nostru.
        var plafon = new Dictionary<string, decimal>(StringComparer.Ordinal);
        void Plafon(string cont, decimal suma) {
            if (cont != null && suma != 0m)
                plafon[cont] = plafon.GetValueOrDefault(cont) + Math.Abs(suma);
        }
        foreach (var (contStoc, delta) in stoc.PeCont) {
            Plafon(contStoc, delta);
            foreach (var contCost in cat.CosturiPentruContStoc(contStoc) ?? (IReadOnlySet<string>)new HashSet<string>())
                Plafon(contCost, delta);
        }
        foreach (var d in registru.Where(d => d.Valoare != 0m)) {
            Plafon(d.ContDebit, d.Valoare);
            Plafon(d.ContCredit, d.Valoare);
        }

        // ---- Verdictul, per cont ----
        var abateri = db.Keys.Union(sursa.Keys)
            .Select(s => (Simbol: s, Db: db.GetValueOrDefault(s), Sursa: sursa.GetValueOrDefault(s)))
            .Select(x => (x.Simbol, x.Db, x.Sursa, Delta: x.Db - x.Sursa))
            .Where(x => Math.Abs(x.Delta) >= EpsV)
            .OrderByDescending(x => Math.Abs(x.Delta))
            .ToList();

        var picate = new List<(string Simbol, decimal Db, decimal Sursa, decimal Delta, string Motiv)>();
        var justificate = new List<(string Simbol, decimal Delta, string Motiv)>();
        foreach (var x in abateri) {
            var explicatie = explicat.GetValueOrDefault(x.Simbol);
            var rezidual = x.Delta - explicatie;
            var toleranta = EpsV * Math.Max(1, inregistrari.GetValueOrDefault(x.Simbol));
            var plafonCont = plafon.GetValueOrDefault(x.Simbol);
            if (Math.Abs(rezidual) < toleranta)
                justificate.Add((x.Simbol, x.Delta,
                    $"explicată exact de registrul divergențelor ({inregistrari.GetValueOrDefault(x.Simbol)} "
                    + $"înregistrări, Σ {explicatie:N2}; rest {rezidual:N2})"));
            else if (Math.Abs(rezidual) <= plafonCont + toleranta)
                justificate.Add((x.Simbol, x.Delta,
                    $"registrul explică {explicatie:N2}, restul de {rezidual:N2} încape în diferența de "
                    + $"EVALUARE măsurată pe contul ăsta ({plafonCont:N2}) — netarea deschiderii (§8.3)"));
            else
                picate.Add((x.Simbol, x.Db, x.Sursa, x.Delta,
                    explicatie == 0m && plafonCont == 0m
                        ? "nicio divergență înregistrată și nicio diferență de evaluare măsurată pe contul ăsta"
                        : $"registrul explică {explicatie:N2}, evaluarea măsurată acoperă {plafonCont:N2}, "
                            + $"rămân {rezidual:N2} fără explicație"));
        }

        foreach (var x in picate)
            contract($"  cont {x.Simbol}: bază {x.Db:N2} = sursă 1C {x.Sursa:N2} (Δ {x.Delta:N2}) "
                + $"— {x.Motiv}", false);

        // Suma abaterilor justificate: banii nu se pierd, se mută între stoc și
        // cost. Nu mai e CONDIȚIE (se satisfăcea trivial — D6), dar rămâne
        // semnal: o sumă care nu se închide arată o diferență reală strecurată
        // printre cele justificate.
        var sumaJustificate = justificate.Sum(x => x.Delta);
        if (Math.Abs(sumaJustificate) > EpsV * Math.Max(1, justificate.Count))
            avert($"[{ctx.Luna:00}/{ctx.An}] abaterile justificate NU se închid la zero "
                + $"(Σ {sumaJustificate:N2}) — banii ar trebui doar să se mute între stoc și cost. "
                + "Semnal, nu verdict: verdictul e per cont, mai sus.");

        foreach (var x in justificate) {
            var purtata = stare.AbateriConturi.GetValueOrDefault(x.Simbol);
            var nou = x.Delta - purtata;
            avert($"[{ctx.Luna:00}/{ctx.An}] cont {x.Simbol}: Δ {x.Delta:N2}"
                + (Math.Abs(nou) < EpsV && purtata != 0 ? " (purtată neschimbată din luna trecută)"
                    : purtata == 0 ? " (nouă)" : $" (din care {nou:N2} nou în luna asta)")
                + $" — {x.Motiv}.");
        }
        stare.AbateriConturi.Clear();
        foreach (var x in justificate)
            stare.AbateriConturi[x.Simbol] = x.Delta;

        contract($"1. Sold per cont OMFP: {db.Keys.Union(sursa.Keys).Count()} simboluri, "
            + $"{picate.Count} conturi fără explicație, {justificate.Count} explicate "
            + $"({registru.Count(d => d.ValoareNepostata != 0m)} divergențe înregistrate pe "
            + $"{explicat.Count} conturi), {nemapate.Count} conturi 1C nemapate",
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
        decimal ValoareAltRegistru, decimal CantitateAltRegistru,
        // Divergența de VALOARE per cont de stoc (Atlas − 1C), măsurată pe ambele
        // părți: intrarea contractului 1.
        IReadOnlyDictionary<string, decimal> PeCont,
        int CheiRotunjire, decimal RotunjireAbsoluta, decimal RotunjireAlgebrica);

    // Registrele de stoc care au corespondent în sursă. `BalantaNivel3` e
    // defalcarea conturilor 3xx, deci comparabile sunt DOAR registrele care
    // oglindesc soldul de pe 3xx: Magazie și Mărfuri (maparea profilului privat —
    // generic → Magazie, MF → Mărfuri). `Consum` NU are corespondent și nici n-ar
    // putea avea: e mecanismul Atlas prin care consumul rămâne pe responsabilul
    // locului (27a) — în 1C marfa a ieșit pur și simplu din 3xx. Restul
    // (Folosință, Custodie, Gratuit, ProducțieNeterminată) n-au reguli în profilul
    // privat azi; dacă apar, intră în același raport de volum exclus, nu tăcut.
    static readonly TipStoc[] RegistreComparabile = [TipStoc.Magazie, TipStoc.Marfuri];

    static RezultatStoc Stoc(IObjectSpace os, ContextLuna ctx, Stare stare,
            IReadOnlyList<Divergenta> registru, Action<string> avert) {
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
        // Valoarea de stoc a Atlas-ului per CONT (prin Tipul produsului lotului):
        // contractul 1 are nevoie de ea ca să știe cât s-a rearanjat sub fiecare
        // cont, în loc de un plafon global.
        var peContDb = os.GetObjectsQuery<RegistruStoc>()
            .Where(r => r.Data <= ctx.Ultima && RegistreComparabile.Contains(r.TipStoc))
            .GroupBy(r => r.Lot.Produs.TipMaterial.ContImplicit.Simbol)
            .Select(g => new { Simbol = g.Key, V = g.Sum(r => Math.Round(r.Valoare, Scara)) })
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

        var produsHex = Reconciliere.InverseazaProduse(os, avert);
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
        var peContSursa = new Dictionary<string, decimal>(StringComparer.Ordinal);
        foreach (var p in pozitii) {
            var cheie = (p.NomenclatorId, p.DepozitId);
            var acum = sursa.GetValueOrDefault(cheie);
            sursa[cheie] = (acum.Q + p.Cantitate, acum.V + p.Valoare);
            var simbol = ctx.Bucla.Catalog.Mapeaza(p.Cont);
            if (simbol != null)
                peContSursa[simbol] = peContSursa.GetValueOrDefault(simbol) + p.Valoare;
        }
        var peCont = new Dictionary<string, decimal>(StringComparer.Ordinal);
        foreach (var x in peContDb)
            if (x.Simbol != null)
                peCont[x.Simbol] = x.V;
        foreach (var (simbol, v) in peContSursa)
            peCont[simbol] = peCont.GetValueOrDefault(simbol) - v;
        foreach (var simbol in peCont.Where(x => Math.Abs(x.Value) < EpsV).Select(x => x.Key).ToList())
            peCont.Remove(simbol);

        // Divergențele MĂSURATE ale rulării, agregate pe cheia contractului: câte
        // bucăți și cât în bani a lăsat unealta în stocul Atlas pentru că sursa
        // i-a cerut o ieșire pe care n-o putea face.
        var masurat = registru
            .Where(d => d.ProdusHex != null && (d.Cantitate != 0m || d.Valoare != 0m))
            .GroupBy(d => (P: d.ProdusHex, D: d.DepozitHex ?? ""))
            .ToDictionary(g => g.Key,
                g => (Q: g.Sum(d => d.Cantitate), V: g.Sum(d => d.Valoare), N: g.Count()));

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
        // simplu pe minus în celula lui. Categoria rămâne pentru celulele în care
        // sursa CHIAR e negativă la ziua de referință; ieșirile pe care unealta
        // le-a aruncat au acum categoria lor măsurată, mai sus.
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
            // Pragurile sursei sunt cifre ROTUNJITE de ea; o comparație exactă
            // rupe pe câțiva bani (34,98 vs 35,01 — ianuarie).
            return Math.Abs(dq) <= negativ.Q + EpsQ && Math.Abs(dv) <= negativ.V + EpsPrag;
        }

        // Pozițiile ORFANE ale sursei: fără produs sau fără depozit (id-ul 1C e
        // plin de zerouri). Nu pot deveni lot în Atlas (decizia 13) — deschiderea
        // le declară deja diferență a sursei, iar în cursul anului rămân la fel.
        const string IdGol = "00000000000000000000000000000000";

        var detalii = new List<(string P, string D, decimal QDb, decimal VDb, decimal QSursa,
            decimal VSursa, string Motiv)>();
        var nepotriviri = 0;
        var justificate = 0;
        var cheiRotunjire = 0;
        var rotunjireAbs = 0m;
        var rotunjireAlg = 0m;
        foreach (var k in db.Keys.Union(sursa.Keys)) {
            var (qDb, vDb) = db.GetValueOrDefault(k);
            var (qSursa, vSursa) = sursa.GetValueOrDefault(k);
            var dq = qDb - qSursa;
            var dv = vDb - vSursa;
            if (Math.Abs(dq) < EpsQ && Math.Abs(dv) < EpsV)
                continue;
            var m = masurat.GetValueOrDefault((k.P, k.D));
            var cantitateMasurata = m.N > 0 && Math.Abs(dq - m.Q) < EpsQ;
            var netat = stare.ProduseNetate.Contains(k.P) || realocateHex.Contains(k.P);
            var motiv =
                k.P == IdGol || k.D == IdGol || k.P == null || k.D == null
                    ? "poziție orfană a sursei (fără produs sau fără depozit — nereprezentabilă ca lot)"
                : deschidere.Contains(k)
                    ? "diferență a sursei, raportată la deschidere (grupă total-negativă / poziție orfană)"
                // 1. JUSTIFICAT MĂSURAT — cantitatea e EGALĂ cu suma liniilor pe
                //    care unealta le-a aruncat chiar ea pe cheia asta.
                : cantitateMasurata && Math.Abs(dv - m.V) <= Math.Max(EpsPrag, EpsV * m.N)
                    ? $"măsurat: {m.N} linii aruncate/parțiale înregistrate pe cheia asta "
                        + $"({m.Q:N3} buc / {m.V:N2} lei)"
                : cantitateMasurata && netat
                    ? $"măsurat pe cantitate ({m.N} linii aruncate, {m.Q:N3} buc); valoarea diferă de "
                        + $"cea a sursei ({m.V:N2} lei) fiindcă Atlas evaluează la costul lui — "
                        + "netarea deschiderii / supapa 48a"
                // 3. REZIDUU DE ROTUNJIRE — cantitate exactă, bani mărunți.
                : Math.Abs(dq) < EpsQ && Math.Abs(dv) <= EpsRotunjire
                    ? "reziduu de rotunjire (cantitate exactă, sub un ban pe rând)"
                // 2. NEGATIVUL SURSEI la ziua de referință.
                : IncapeInNegativulSursei(k.P)
                    ? "celulă negativă în 1C la ziua de referință; "
                        + $"abaterea pe produs ({DeltaProdus(k.P).Q:N3} buc / {DeltaProdus(k.P).V:N2} lei) "
                        + $"încape în negativul sursei ({negativeSursa[k.P].Q:N3} buc / "
                        + $"{negativeSursa[k.P].V:N2} lei)"
                : Math.Abs(dq) < EpsQ && netat
                    ? "cost per lot rearanjat, cantitate exactă (netarea deschiderii sau "
                        + "realocarea supapei 48a)"
                : null;
            if (motiv == null)
                nepotriviri++;
            else {
                justificate++;
                if (motiv.StartsWith("reziduu de rotunjire")) {
                    cheiRotunjire++;
                    rotunjireAbs += Math.Abs(dv);
                    rotunjireAlg += dv;
                }
            }
            detalii.Add((k.P, k.D, qDb, vDb, qSursa, vSursa, motiv));
        }

        if (cheiRotunjire > 0 && Math.Abs(rotunjireAlg) > PragRotunjireSistematica)
            avert($"[{ctx.Luna:00}/{ctx.An}] cele {cheiRotunjire} chei de stoc puse pe seama rotunjirii "
                + $"NU se compensează (Σ algebrică {rotunjireAlg:N2} lei din {rotunjireAbs:N2} lei în "
                + "valoare absolută) — rotunjirea care merge toată în același sens nu mai e rotunjire.");

        // Cifra de raport a efectului netării (nu mai e plafon — D6): abaterea
        // NETĂ pe produs, adică ce se scurge din stoc în contabilitate. Perechile
        // oglindite se anulează în interiorul produsului și n-au mișcat niciun sold.
        var justificat = detalii.Where(d => d.Motiv != null)
            .GroupBy(d => d.P, StringComparer.Ordinal)
            .Sum(g => Math.Abs(g.Sum(d => d.VDb - d.VSursa)));

        return new RezultatStoc(db.Keys.Union(sursa.Keys).Count(), nepotriviri, justificate,
            justificat, detalii,
            altRegistru.Sum(x => x.V), altRegistru.Sum(x => x.Q), peCont,
            cheiRotunjire, rotunjireAbs, rotunjireAlg);
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
        // luna trecută; restul se numără. Cheile de rotunjire nu se detaliază
        // niciodată una câte una — au raportul lor agregat mai jos.
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
            if (noi <= DetaliiJustificate && !d.Motiv.StartsWith("reziduu de rotunjire"))
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
        // Categoriile măsurate poartă cifrele lor în motiv, deci se grupează pe
        // prima propoziție.
        foreach (var g in stoc.Detalii.Where(d => d.Motiv != null)
                     .GroupBy(d => Familie(d.Motiv)).OrderByDescending(g => g.Count()))
            Console.WriteLine($"     {g.Count(),6} chei justificate — {g.Key} "
                + $"(Σ |Δ| {g.Sum(d => Math.Abs(d.VDb - d.VSursa)):N2} lei)");
        if (stoc.CheiRotunjire > 0)
            Console.WriteLine($"     din care rotunjire: {stoc.CheiRotunjire} chei, "
                + $"Σ |Δ| {stoc.RotunjireAbsoluta:N2} lei, Σ algebrică {stoc.RotunjireAlgebrica:N2} lei "
                + $"(prag de alarmă {PragRotunjireSistematica:N2}).");

        contract($"3. Stoc per produs × gestiune: {stoc.Chei} chei comparate, {stoc.Nepotriviri} "
            + $"nepotriviri nejustificate ({stoc.Justificate} justificate — Σ {stoc.Justificat:N2} lei; "
            + $"{noi} noi / {purtate} purtate; "
            + $"{stoc.ValoareAltRegistru:N2} lei în registre necomparabile)", stoc.Nepotriviri == 0);
    }

    // Familia unei categorii: motivele măsurate poartă cifrele cheii în text, deci
    // gruparea se face pe partea dinaintea parantezei / două puncte.
    static string Familie(string motiv) {
        var i = motiv.IndexOfAny([':', '(', ';']);
        return i > 0 ? motiv[..i].TrimEnd() : motiv;
    }
}
