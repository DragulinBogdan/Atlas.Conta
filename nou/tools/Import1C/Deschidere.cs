using System.Globalization;
using System.Text.RegularExpressions;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using DevExpress.ExpressApp;

namespace Import1C;

// PASUL 3 al feliei 1C-b: DESCHIDEREA la 01.01.2025.
//
// Rândurile de deschidere sunt SINGURELE cu `DocumentId = null` (decizia 25e) —
// proprietate exclusivă a importului, deci se rescriu INTEGRAL la fiecare rulare
// (nu au nevoie de legături: identitatea lor e „tot ce n-are document"). Restul —
// produsele și loturile — sunt nomenclator și se upsert-ează prin
// `MigrareLegatura` ca tot ce ține de idempotența importului (decizia 34b).
//
// Precedentul mecanic e faza 5 din `tools/Migrare` (decizia 34d/34e); diferența
// de fond ține de sursă, nu de mecanică: 1C nu ține soldurile de deschidere pe
// dimensiuni (BalantaNivel1 nu defalcă 401/411 pe partener la 01.01), iar
// „lotul" 1C e un subconto (produs × document creator × depozit) ale cărui
// poziții pot fi NEGATIVE — artefactul returului-ca-lot (design §3). Atlas cere
// sold ≥ 0 per lot (gardianul din motor, decizia 25d), deci negativele se
// NETEAZĂ în interiorul grupei produs×depozit, conservând sumele.
static partial class Deschidere {

    // Ancora bilanțului de deschidere în planul OMFP (echivalentul lui 891.01.00
    // din profilul bugetar — decizia 34d).
    public const string Ancora = "891";

    // Toleranțele contractului: valorile în lei la bani, cantitățile la gram.
    const decimal EpsV = 0.005m;
    const decimal EpsQ = 0.0005m;

    // ==================== C. Soldurile contabile ====================

    public record RezultatContabil(
        int Randuri, int Nerezolvate, int PeSumator,
        int Extrabilantiere, decimal SumaExtrabilantiera,
        decimal ReziduuAncora);

    // Un rând per cont 1C cu sold nenul, contra ancorei: sold pozitiv (debitor)
    // → Debit = contul, Credit = 891; negativ → invers, cu valoarea absolută.
    //
    // FĂRĂ dimensiuni — și e un FAPT AL SURSEI, nu o simplificare: 1C nu poartă
    // defalcarea analitică pe soldul de deschidere al terților (verificat:
    // BalantaNivel1 are 2 rânduri cont×partener la 01.01.2025, față de soldurile
    // de sute de mii de lei de pe 401/411). Consecința asumată e că stingerile
    // din 2025 pe facturi din 2024 vor stinge soldul global, fără imperechere —
    // exact modelul pasului 4 (decizia 34d, „terții pornesc pe sold per partener").
    public static RezultatContabil Contabile(
            IObjectSpaceProvider provider,
            IReadOnlyList<FlaxSold> solduri,
            IReadOnlySet<string> extrabilantiere,
            Func<string, string> mapeaza,
            DateOnly data, Action<string> avert, Action<string, bool> check) {
        using var os = provider.CreateObjectSpace();
        os.Delete(os.GetObjectsQuery<RegistruContabil>().Where(r => r.DocumentId == null).ToList());
        os.CommitChanges();

        var plan = os.GetObjectsQuery<Cont>().ToDictionary(c => c.Simbol, c => c.ID);
        var sumatori = os.GetObjectsQuery<Cont>().Where(c => c.Sumator).Select(c => c.Simbol).ToHashSet();
        if (!plan.TryGetValue(Ancora, out var ancora))
            throw new InvalidOperationException($"Planul OMFP nu conține ancora {Ancora}.");

        var randuri = 0;
        var nerezolvate = new List<FlaxSold>();
        var peSumator = new List<(FlaxSold Sold, string Simbol)>();
        var extra = new List<FlaxSold>();
        var peAncora = new List<FlaxSold>();

        foreach (var sold in solduri.OrderBy(s => s.Cont, StringComparer.Ordinal)) {
            // Conturile extrabilanțiere 1C (clasa 8 a planului lor) NU au ce căuta
            // contra ancorei: 891 e o balanță de deschidere bilanțieră, iar un
            // rând extrabilanțier ar lăsa-o dezechilibrată. Se sar și se
            // raportează cu suma — echivalentul „clasa 8 amânată" de la pasul 4
            // (deciziile 9/22c: evidența angajamentelor e alt modul).
            if (extrabilantiere.Contains(sold.Cont)) {
                extra.Add(sold);
                avert($"Cont 1C {sold.Cont} (sold {sold.SoldIni:N2}) e EXTRABILANȚIER în planul 1C "
                    + "— nu se scrie contra ancorei 891 (ar dezechilibra deschiderea).");
                continue;
            }
            var simbol = mapeaza(sold.Cont);
            if (simbol == null) {
                nerezolvate.Add(sold);
                continue;
            }
            if (sumatori.Contains(simbol)) {
                peSumator.Add((sold, simbol));
                continue;
            }
            // Sursa are propriul cont de bilanț de deschidere, cu un reziduu de
            // rotunjire pe el (`891.` = −0,01 la 01.01.2025). E ACELAȘI cont cu
            // ancora noastră: scrierea lui ar produce un rând 891/891, care nu
            // poartă informație. Se sare, iar reziduul devine contractul de
            // verificare de mai jos — ancora Atlas trebuie să reproducă EXACT
            // soldul 1C al ancorei, altfel s-a pierdut un leu pe drum.
            if (simbol == Ancora) {
                peAncora.Add(sold);
                avert($"Cont 1C {sold.Cont} (sold {sold.SoldIni:N2}) E ancora de deschidere "
                    + $"({Ancora}) — sursa își parchează pe el reziduul de rotunjire; nu se scrie "
                    + "ca rând (ar fi 891/891), ci se verifică prin soldul ancorei.");
                continue;
            }
            var r = os.CreateObject<RegistruContabil>();
            r.Data = data;
            r.NumarNota = "DESCHIDERE";
            r.Valoare = Math.Abs(sold.SoldIni);
            if (sold.SoldIni > 0) {
                r.ContDebitId = plan[simbol];
                r.ContCreditId = ancora;
            }
            else {
                r.ContDebitId = ancora;
                r.ContCreditId = plan[simbol];
            }
            randuri++;
        }
        os.CommitChanges();

        // La deschidere nu mai tolerăm găuri (spre deosebire de smoke-ul pasului
        // 2, unde erau intrare de lucru): un sold nescris e un leu pierdut din
        // bilanț, deci eșec, nu avertisment.
        foreach (var s in nerezolvate.OrderByDescending(s => Math.Abs(s.SoldIni)))
            check($"Sold 1C {s.Cont} ({s.SoldIni:N2}) se mapează pe planul OMFP", false);
        foreach (var (s, simbol) in peSumator.OrderByDescending(x => Math.Abs(x.Sold.SoldIni)))
            check($"Sold 1C {s.Cont} ({s.SoldIni:N2}) → {simbol}: cont NE-sumator", false);
        check($"Toate cele {solduri.Count} solduri 1C se rezolvă pe conturi ne-sumatoare "
            + $"({randuri} rânduri scrise, {extra.Count} extrabilanțiere + {peAncora.Count} pe ancoră sărite)",
            nerezolvate.Count == 0 && peSumator.Count == 0);

        // Verificarea internă a fazei: rândurile scrise se citesc ÎNAPOI și se
        // netează per simbol. Ancora trebuie să se închidă la zero — adică
        // balanța 1C e echilibrată ȘI n-am pierdut/dublat rânduri pe drum.
        var scrise = os.GetObjectsQuery<RegistruContabil>()
            .Where(r => r.DocumentId == null)
            .Select(r => new { D = r.ContDebit.Simbol, C = r.ContCredit.Simbol, r.Valoare })
            .ToList();
        var net = new Dictionary<string, decimal>();
        foreach (var r in scrise) {
            net[r.D] = net.GetValueOrDefault(r.D) + r.Valoare;
            net[r.C] = net.GetValueOrDefault(r.C) - r.Valoare;
        }
        check($"Registru contabil: {scrise.Count} rânduri de deschidere citite înapoi "
            + $"= {randuri} scrise", scrise.Count == randuri);
        check($"Σ debit = Σ credit pe rândurile scrise (net pe toate conturile: "
            + $"{net.Values.Sum():N2})", Math.Abs(net.Values.Sum()) < EpsV);
        // Ancora se închide pe reziduul PROPRIU al sursei, nu neapărat la zero:
        // Σ soldurilor 1C e zero doar dacă numeri și contul lor de deschidere.
        // Cu sursa curată (fără rând pe 891) verificarea e „ancora = 0".
        var reziduu = peAncora.Sum(s => s.SoldIni);
        check($"Ancora {Ancora} reproduce soldul 1C al aceluiași cont: "
            + $"{net.GetValueOrDefault(Ancora):N2} = {reziduu:N2}",
            Math.Abs(net.GetValueOrDefault(Ancora) - reziduu) < EpsV);

        return new RezultatContabil(randuri, nerezolvate.Count, peSumator.Count,
            extra.Count, extra.Sum(s => s.SoldIni), reziduu);
    }

    // ==================== D + E. Stocul de deschidere ====================

    // O celulă = (lot × depozit): exact granularitatea rândului de RegistruStoc
    // (Atlas ține soldul per Lot × Repartitor × TipStoc). Mutabilă — netarea
    // lucrează pe ea în memorie, înainte de orice scriere.
    sealed class Celula {
        public string CheieLot;
        public string ProdusHex;
        public string DepozitHex;
        // Denumirile 1C, purtate doar ca să fie lizibil raportul: grupele sărite
        // ajung la om, iar un hex de 32 de caractere nu se poate căuta în 1C.
        public string ProdusDesc;
        public string DepozitDesc;
        public DateOnly DataLot;
        public decimal Cantitate;
        public decimal Valoare;
    }

    // Descriptorul lotului, construit înainte de entități: identitatea 1C
    // (document creator × produs), data FIFO și gestiunea nașterii.
    sealed class DescriptorLot {
        public string Cheie;
        public string ProdusHex;
        public string SimbolCont;
        public string DepozitHex;
        public DateOnly Data;
        public bool DataParsata;
        public decimal CantitateBruta;
        public decimal ValoareBruta;
    }

    // O poziție a sursei care NU ajunge în registrul de stoc, cu identitatea 1C
    // păstrată (hex, nu doar denumire): e intrarea reconcilierii (pasul 4), care
    // compară baza cu sursa BRUTĂ și trebuie să poată recunoaște nepotrivirile
    // deja explicate ca „diferență a sursei" (34f) — altfel le-ar raporta ca
    // eșecuri. Δq/Δv = ce lipsește din registru față de sursă.
    public record DiferentaSursa(
        string ProdusHex, string DepozitHex, string ProdusDesc, string DepozitDesc,
        decimal Cantitate, decimal Valoare, string Motiv);

    public record RezultatStoc(
        int Loturi, int LoturiNoi, int Produse, int ProduseNoi, int ProduseFaraTip,
        int RanduriStoc, int PozitiiNegative, int GrupeSarite,
        decimal CantitateSarita, decimal ValoareSarita,
        int CeluleDegenerate, int DateNeparsate,
        decimal CantitateScrisa, decimal ValoareScrisa,
        IReadOnlyList<DiferentaSursa> DiferenteJustificate);

    public static RezultatStoc Stoc(
            IObjectSpaceProvider provider, ImportLaCerere laCerere,
            IReadOnlyList<FlaxPozitieStoc> pozitii,
            Func<string, string> mapeaza,
            DateOnly data, Action<string> avert, Action<string, bool> check) {

        // ---- 1. Gruparea pe loturi (document creator × produs × CONT) ----
        // Același lot poate sta în MAI MULTE depozite: Atlas ține soldul per
        // Lot × Repartitor, iar `Lot.Gestiune` e doar gestiunea NAȘTERII —
        // deci cheia lotului NU include depozitul (design §3). Include în schimb
        // SIMBOLUL contului (fix review 1C-b): subconto-ul 1C e per cont, iar 46
        // de perechi (document, produs) stau pe mai multe conturi de stoc la
        // deschidere — fuzionarea lor ar amesteca marfă (371) cu materiale
        // (3028) într-un singur lot, cu TipStoc-ul și contarea 1C-c ale celui
        // greșit.
        string CheieLot(FlaxPozitieStoc p) =>
            $"{p.DocTip}:{p.DocId}:{p.NomenclatorId}:{mapeaza(p.Cont)}";
        var descriptori = new Dictionary<string, DescriptorLot>(StringComparer.Ordinal);
        foreach (var p in pozitii.OrderBy(p => CheieLot(p), StringComparer.Ordinal)
                     .ThenBy(p => p.Cont, StringComparer.Ordinal)
                     .ThenBy(p => p.DepozitId, StringComparer.Ordinal)) {
            var cheie = CheieLot(p);
            if (!descriptori.TryGetValue(cheie, out var d)) {
                var parsata = ParseData(p.DocDesc);
                d = new DescriptorLot {
                    Cheie = cheie,
                    ProdusHex = p.NomenclatorId,
                    // Simbolul e parte din cheie — toate pozițiile lotului îl au
                    // pe același (decizia 26b: Cod-ul Tipului E simbolul de cont).
                    SimbolCont = mapeaza(p.Cont),
                    DepozitHex = p.DepozitId,
                    Data = parsata ?? data,
                    DataParsata = parsata != null,
                };
                descriptori[cheie] = d;
            }
            d.CantitateBruta += p.Cantitate;
            d.ValoareBruta += p.Valoare;
        }

        // ---- 2. Celulele (lot × depozit) ----
        var celule = new List<Celula>();
        var indexCelule = new Dictionary<(string, string), Celula>();
        foreach (var p in pozitii) {
            var cheie = CheieLot(p);
            if (!indexCelule.TryGetValue((cheie, p.DepozitId), out var c)) {
                var d = descriptori[cheie];
                c = new Celula {
                    CheieLot = cheie, ProdusHex = p.NomenclatorId,
                    DepozitHex = p.DepozitId, DataLot = d.Data,
                    ProdusDesc = p.NomenclatorDesc, DepozitDesc = p.DepozitDesc,
                };
                indexCelule[(cheie, p.DepozitId)] = c;
                celule.Add(c);
            }
            c.Cantitate += p.Cantitate;
            c.Valoare += p.Valoare;
        }
        var negativeInitial = celule.Count(c => c.Cantitate < 0 || c.Valoare < 0);
        // Totalurile de dinaintea netării: `Neteaza` mută cifrele ÎN LOC, deci
        // martorul invariantului trebuie luat acum, nu după.
        var qInitial = celule.Sum(c => c.Cantitate);
        var vInitial = celule.Sum(c => c.Valoare);

        // ---- 3. Netarea negativelor, per produs × depozit ----
        var grupeSarite = new List<DiferentaSursa>();
        var sariteHex = new HashSet<(string, string)>();
        var grupeDezechilibrate = 0;
        foreach (var grup in celule.GroupBy(c => (c.ProdusHex, c.DepozitHex))) {
            var lot = grup
                .OrderBy(c => c.DataLot)
                .ThenBy(c => c.CheieLot, StringComparer.Ordinal)
                .ToList();
            var qInainte = lot.Sum(c => c.Cantitate);
            var vInainte = lot.Sum(c => c.Valoare);
            if (!lot.Any(c => c.Cantitate < 0 || c.Valoare < 0))
                continue;

            // Grupă cu TOTAL negativ: nu se poate reprezenta (gardianul cere sold
            // ≥ 0), și nu se îndoaie invariantul pentru ea — se raportează
            // integral ca diferență a sursei (34f) și nu se scrie deloc.
            if (qInainte < -EpsQ || vInainte < -EpsV) {
                grupeSarite.Add(new DiferentaSursa(grup.Key.ProdusHex, grup.Key.DepozitHex,
                    lot[0].ProdusDesc, lot[0].DepozitDesc, qInainte, vInainte,
                    "grupă produs × depozit cu TOTAL negativ"));
                sariteHex.Add(grup.Key);
                continue;
            }
            Neteaza(lot);
            var qDupa = lot.Sum(c => c.Cantitate);
            var vDupa = lot.Sum(c => c.Valoare);
            if (Math.Abs(qDupa - qInainte) >= EpsQ || Math.Abs(vDupa - vInainte) >= EpsV) {
                grupeDezechilibrate++;
                avert($"Netare dezechilibrată pe produs {grup.Key.ProdusHex} × depozit "
                    + $"{grup.Key.DepozitHex}: {qInainte:N3}/{vInainte:N2} → {qDupa:N3}/{vDupa:N2}.");
            }
        }
        check($"Netarea conservă Σ per produs × depozit ({grupeDezechilibrate} grupe dezechilibrate)",
            grupeDezechilibrate == 0);

        var deScris = celule
            .Where(c => !sariteHex.Contains((c.ProdusHex, c.DepozitHex)))
            .Where(c => Math.Abs(c.Cantitate) >= EpsQ || Math.Abs(c.Valoare) >= EpsV)
            .ToList();
        var degenerate = deScris.Count(c => Math.Abs(c.Cantitate) < EpsQ || Math.Abs(c.Valoare) < EpsV);

        // Invariantul DUR al fazei, pe total: netarea mută bani între loturi, nu-i
        // creează și nu-i pierde. Se compară ce se scrie cu totalul de DINAINTE de
        // netare, din care se scad grupele sărite (raportate separat, ca diferență
        // a sursei) — acoperă atât netarea, cât și celulele căzute la zero.
        var qBrut = qInitial - grupeSarite.Sum(g => g.Cantitate);
        var vBrut = vInitial - grupeSarite.Sum(g => g.Valoare);
        var qNet = deScris.Sum(c => c.Cantitate);
        var vNet = deScris.Sum(c => c.Valoare);
        check($"Σ cantitate scrisă = Σ înainte de netare, fără grupele sărite: "
            + $"{qNet:N3} = {qBrut:N3}", Math.Abs(qBrut - qNet) < EpsQ);
        check($"Σ valoare scrisă = Σ înainte de netare, fără grupele sărite: "
            + $"{vNet:N2} = {vBrut:N2}", Math.Abs(vBrut - vNet) < EpsV);

        // ---- 4. Produsele (la cerere) ----
        // Produsul are UN TipMaterial, dar 74 de nomenclatoare stau pe mai multe
        // conturi la deschidere (fix review 1C-b): Tipul se alege pe contul
        // DOMINANT pe valoare absolută, nu pe primul întâlnit (ordinea hex e
        // arbitrară, iar „primul" putea fi contul rezidual negativ). Cazurile
        // multi-cont se raportează — la 1C-c pot cere Tip per linie, nu per produs.
        var simbolDominant = descriptori.Values
            .GroupBy(d => d.ProdusHex, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g
                .GroupBy(d => d.SimbolCont)
                .OrderByDescending(sg => sg.Sum(d => Math.Abs(d.ValoareBruta)))
                .ThenBy(sg => sg.Key, StringComparer.Ordinal)
                .First().Key, StringComparer.Ordinal);
        var multiCont = descriptori.Values
            .GroupBy(d => d.ProdusHex, StringComparer.Ordinal)
            .Count(g => g.Select(d => d.SimbolCont).Distinct().Count() > 1);
        if (multiCont > 0)
            avert($"{multiCont} produse stau pe MAI MULTE conturi de stoc la deschidere — "
                + "TipMaterial-ul s-a ales pe contul dominant pe valoare; loturile rămân "
                + "separate per cont (cheia include simbolul).");

        var produsPerLot = new Dictionary<string, Guid>(StringComparer.Ordinal);
        var faraTip = new HashSet<string>(StringComparer.Ordinal);
        var procesate = 0;
        foreach (var d in descriptori.Values.OrderBy(d => d.Cheie, StringComparer.Ordinal)) {
            var id = laCerere.AsiguraProdus(d.ProdusHex, simbolDominant[d.ProdusHex]);
            if (id != null)
                produsPerLot[d.Cheie] = id.Value;
            else
                faraTip.Add(d.ProdusHex);
            if (++procesate % 1000 == 0)
                Console.WriteLine($"    …{procesate}/{descriptori.Count} loturi: produse rezolvate "
                    + $"({laCerere.ProduseNoi} noi).");
        }
        check($"Toate produsele stocului de deschidere au TipMaterial în profil "
            + $"({faraTip.Count} refuzate)", faraTip.Count == 0);

        // ---- 5. Loturile (upsert prin legătură "1C:Lot") ----
        int loturiNoi;
        var lotId = new Dictionary<string, Guid>(StringComparer.Ordinal);
        using (var os = provider.CreateObjectSpace()) {
            var depozite = Legaturi.Incarca(os, "Depozite");
            var legaturi = Legaturi.Incarca(os, "Lot");
            var existente = os.GetObjectsQuery<Lot>().ToList().ToDictionary(l => l.ID);
            var deLegat = new List<(string Cheie, Lot Entitate)>();
            var faraDepozit = new List<DescriptorLot>();

            // PretUnitar = Σvaloare/Σcantitate a lotului DUPĂ netare. Divergență
            // asumată față de spec (care spunea „a grupei", adică brut): Atlas
            // evaluează ieșirile la `Lot.PretUnitar`, deci un preț rupt de
            // valoarea din registru ar lăsa reziduu nedescărcabil în stoc la
            // pasul 1C-c.
            //
            // Cine se creează (fix review 1C-b, rafinat): lotul trăiește dacă are
            // măcar o celulă în afara grupelor total-negative sărite. Loturile
            // GOLITE de netare (sold 0 după absorbția retururilor — mii) se
            // creează totuși, cu prețul BRUT istoric: documentele 2025 le pot
            // pin-ui (rândurile 607=371 ale 1C referă lotul original), iar un lot
            // absent sau cu preț 0 ar strica evaluarea acolo unde soldul revine.
            // NU se creează doar loturile ale căror celule aparțin TOATE grupelor
            // sărite — un Lot cu preț negativ n-ar avea nicio utilizare legală.
            var loturiVii = celule
                .Where(c => !sariteHex.Contains((c.ProdusHex, c.DepozitHex)))
                .Select(c => c.CheieLot)
                .ToHashSet(StringComparer.Ordinal);
            var netPerLot = deScris
                .GroupBy(c => c.CheieLot, StringComparer.Ordinal)
                .ToDictionary(g => g.Key, g => (Q: g.Sum(c => c.Cantitate), V: g.Sum(c => c.Valoare)),
                    StringComparer.Ordinal);

            var loturiFaraCelule = 0;
            foreach (var d in descriptori.Values.OrderBy(d => d.Cheie, StringComparer.Ordinal)) {
                if (!produsPerLot.TryGetValue(d.Cheie, out var produsId))
                    continue; // produs refuzat — deja Check FAIL mai sus
                if (!loturiVii.Contains(d.Cheie)) {
                    loturiFaraCelule++;
                    continue; // grupele lui, sărite integral — raportate ca diferență a sursei
                }
                if (!depozite.TryGetValue(d.DepozitHex, out var gestiuneId)) {
                    faraDepozit.Add(d);
                    continue;
                }
                Lot lot = null;
                if (legaturi.TryGetValue(d.Cheie, out var tinta) && !existente.TryGetValue(tinta, out lot)) {
                    avert($"Legătură orfană 1C:Lot/{d.Cheie} — lotul lipsește; recreat.");
                    var moarta = os.FirstOrDefault<MigrareLegatura>(
                        m => m.Tabela == Legaturi.Tabela("Lot") && m.CheieLegacy == d.Cheie);
                    if (moarta != null)
                        os.Delete(moarta);
                }
                if (lot == null) {
                    lot = os.CreateObject<Lot>();
                    deLegat.Add((d.Cheie, lot));
                }
                var (q, v) = netPerLot.TryGetValue(d.Cheie, out var n)
                    ? n : (d.CantitateBruta, d.ValoareBruta);
                lot.ProdusId = produsId;
                lot.GestiuneId = gestiuneId;
                lot.PretUnitar = Math.Abs(q) >= EpsQ ? Math.Round(v / q, 4) : 0m;
                lot.Data = d.Data;
                lotId[d.Cheie] = lot.ID;
            }
            os.CommitChanges();
            foreach (var (cheie, e) in deLegat) {
                Legaturi.Leaga(os, "Lot", cheie, e.ID);
                lotId[cheie] = e.ID;
            }
            os.CommitChanges();
            loturiNoi = deLegat.Count;
            if (loturiFaraCelule > 0)
                avert($"{loturiFaraCelule} loturi identificate în sursă nu s-au creat: toate "
                    + "celulele lor aparțin grupelor total-negative sărite (diferență a sursei).");

            foreach (var d in faraDepozit.Take(10))
                check($"Depozitul 1C {d.DepozitHex} (lot {d.Cheie}) e legat de o Gestiune", false);
            check($"Toate pozițiile de stoc cad pe depozite legate ({faraDepozit.Count} loturi orfane)",
                faraDepozit.Count == 0);
        }

        // ---- 6. Rândurile de RegistruStoc ----
        int randuri;
        using (var os = provider.CreateObjectSpace()) {
            os.Delete(os.GetObjectsQuery<RegistruStoc>().Where(r => r.DocumentId == null).ToList());
            os.CommitChanges();

            var depozite = Legaturi.Incarca(os, "Depozite");
            // TipStoc-ul oglindește regulile de stoc private (NIR/LDI/DSC, decizia
            // 37a): marfa merge în registrul Marfuri, restul în Magazie —
            // reconcilierea pasului 1C-c citește aceleași chei. Se derivă din
            // SIMBOLUL lotului (fix review 1C-b), nu din Tipul produsului: un
            // produs multi-cont poartă Tipul contului dominant, dar lotul lui de
            // pe 3028 tot în Magazie trebuie să stea, nu în Marfuri.
            var tipStocPerSimbol = os.GetObjectsQuery<TipMaterial>()
                .Select(t => new { t.Cod, Clasa = t.Clasa.Cod })
                .ToList()
                .ToDictionary(x => x.Cod, x => x.Clasa == "MF" ? TipStoc.Marfuri : TipStoc.Magazie);

            randuri = 0;
            foreach (var c in deScris.OrderBy(c => c.CheieLot, StringComparer.Ordinal)
                         .ThenBy(c => c.DepozitHex, StringComparer.Ordinal)) {
                if (!lotId.TryGetValue(c.CheieLot, out var id)
                    || !depozite.TryGetValue(c.DepozitHex, out var repartitorId))
                    continue; // raportat mai sus (produs fără Tip / depozit nelegat)
                var simbol = descriptori[c.CheieLot].SimbolCont;
                var r = os.CreateObject<RegistruStoc>();
                r.Data = data;
                r.LotId = id;
                r.RepartitorId = repartitorId;
                r.TipStoc = simbol != null && tipStocPerSimbol.TryGetValue(simbol, out var t)
                    ? t : TipStoc.Magazie;
                r.Cantitate = c.Cantitate;
                r.Valoare = c.Valoare;
                randuri++;
            }
            os.CommitChanges();

            // Citire ÎNAPOI, ca la deschiderea contabilă: ce s-a scris trebuie să
            // fie exact ce s-a calculat. Și, mai important, rândurile trebuie să
            // fie NENEGATIVE pe ambele coordonate — un sold de deschidere negativ
            // per Lot × Repartitor ar bloca prima ieșire din 2025 în gardianul de
            // sold intermediar (decizia 25d), adică netarea ar fi eșuat tăcut.
            var scrise = os.GetObjectsQuery<RegistruStoc>()
                .Where(r => r.DocumentId == null)
                .Select(r => new { r.Cantitate, r.Valoare })
                .ToList();
            check($"Registru stoc: {scrise.Count} rânduri citite înapoi = {randuri} scrise",
                scrise.Count == randuri);
            check($"Registru stoc: Σ cantitate {scrise.Sum(r => r.Cantitate):N3} "
                + $"= {deScris.Sum(c => c.Cantitate):N3} calculat",
                Math.Abs(scrise.Sum(r => r.Cantitate) - deScris.Sum(c => c.Cantitate)) < EpsQ);
            check($"Registru stoc: Σ valoare {scrise.Sum(r => r.Valoare):N2} "
                + $"= {deScris.Sum(c => c.Valoare):N2} calculat",
                Math.Abs(scrise.Sum(r => r.Valoare) - deScris.Sum(c => c.Valoare)) < EpsV);
            check($"Registru stoc: niciun sold de deschidere negativ "
                + $"({scrise.Count(r => r.Cantitate < 0)} cantități, "
                + $"{scrise.Count(r => r.Valoare < 0)} valori)",
                !scrise.Any(r => r.Cantitate < 0 || r.Valoare < 0));
        }

        foreach (var g in grupeSarite.OrderBy(g => g.Valoare))
            avert($"Grupă „{g.ProdusDesc ?? g.ProdusHex}” × depozit „{g.DepozitDesc ?? g.DepozitHex}” "
                + $"cu TOTAL negativ ({g.Cantitate:N3} buc, {g.Valoare:N2} lei) — nereprezentabilă "
                + "(Atlas cere sold ≥ 0 per lot); NU se scrie. Diferență a sursei.");

        return new RezultatStoc(
            descriptori.Count, loturiNoi,
            descriptori.Values.Select(d => d.ProdusHex).Distinct().Count(),
            laCerere.ProduseNoi, faraTip.Count,
            randuri, negativeInitial, grupeSarite.Count,
            grupeSarite.Sum(g => g.Cantitate), grupeSarite.Sum(g => g.Valoare),
            degenerate, descriptori.Values.Count(d => !d.DataParsata),
            deScris.Sum(c => c.Cantitate), deScris.Sum(c => c.Valoare),
            grupeSarite);
    }

    // Netarea unei grupe produs × depozit. Negativul se consumă din loturile
    // POZITIVE ale aceleiași grupe, în ordine FIFO (data lotului, tie-break pe
    // cheie); valoarea negativă se alocă PRO-RATA pe cantitatea luată, iar
    // negativul pur-valoric (cantitate 0) se scade din primul lot cu valoare.
    //
    // Se iterează până la fixpoint: alocarea pro-rata poate împinge la rândul ei
    // un lot pozitiv în valoare negativă (când prețul negativului e mai mare
    // decât al lotului din care se ia) — acela devine negativ în iterația
    // următoare. Convergența e garantată de gardianul de grupă: aici ajung doar
    // grupele cu total ≥ 0, deci există întotdeauna acoperire.
    static void Neteaza(List<Celula> grup) {
        for (var iteratie = 0; iteratie < 50; iteratie++) {
            var negative = grup.Where(c => c.Cantitate < 0 || c.Valoare < 0).ToList();
            if (negative.Count == 0)
                return;
            var progres = false;
            foreach (var neg in negative) {
                var q = neg.Cantitate < 0 ? -neg.Cantitate : 0m;
                var v = neg.Valoare < 0 ? -neg.Valoare : 0m;
                if (q > 0) {
                    var luari = new List<(Celula C, decimal Q)>();
                    var ramas = q;
                    foreach (var p in grup) {
                        if (ramas <= 0)
                            break;
                        if (p == neg || p.Cantitate <= 0)
                            continue;
                        var take = Math.Min(p.Cantitate, ramas);
                        luari.Add((p, take));
                        ramas -= take;
                    }
                    var luat = q - ramas;
                    if (luat <= 0)
                        continue;
                    // Valoarea alocată în total (rotunjită o dată), apoi
                    // distribuită cu rest pe ultimul lot — conservare exactă.
                    var alocatTotal = luat == q ? v : Math.Round(v * luat / q, 2);
                    var alocat = 0m;
                    for (var i = 0; i < luari.Count; i++) {
                        var (p, take) = luari[i];
                        var parte = i == luari.Count - 1
                            ? alocatTotal - alocat
                            : Math.Round(alocatTotal * take / luat, 2);
                        p.Cantitate -= take;
                        p.Valoare -= parte;
                        alocat += parte;
                    }
                    neg.Cantitate += luat;
                    neg.Valoare += alocatTotal;
                    progres = true;
                }
                else if (v > 0) {
                    var p = grup.FirstOrDefault(c => c != neg && c.Valoare > 0);
                    if (p == null)
                        continue;
                    var take = Math.Min(p.Valoare, v);
                    p.Valoare -= take;
                    neg.Valoare += take;
                    progres = true;
                }
            }
            if (!progres)
                return;
        }
    }

    // Data lotului = data documentului creator, extrasă din descrierea 1C
    // („Factura achiziție X din 31.12.2013", „AsamblareSED00000434/30.08.2021").
    // Se ia ULTIMA potrivire: numerele de factură conțin uneori grupuri
    // asemănătoare, dar data stă întotdeauna la coadă. Ordinea FIFO istorică
    // depinde de ea (decizia 34e) — de aceea rata de parsare se raportează.
    [GeneratedRegex(@"\d{2}\.\d{2}\.\d{4}")]
    private static partial Regex RegexData();

    static DateOnly? ParseData(string descriere) {
        if (string.IsNullOrEmpty(descriere))
            return null;
        var potriviri = RegexData().Matches(descriere);
        if (potriviri.Count == 0)
            return null;
        return DateOnly.TryParseExact(potriviri[^1].Value, "dd.MM.yyyy",
            CultureInfo.InvariantCulture, DateTimeStyles.None, out var d) ? d : null;
    }
}
