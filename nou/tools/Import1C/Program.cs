using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.DatabaseUpdate;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.EFCore;
using Import1C;
using Microsoft.EntityFrameworkCore;

// FAZA 1C — conectorul de import (deciziile 44/45): profilul privat se
// finalizează PRIN import + reconciliere pe un an fiscal complet (2025), NU în
// abstract. 1C are statutul deciziei 21 — evidență și direcție, niciodată canon.
//
// Unealta e consolă ca `Migrare` (precedentul de formă, decizia 45f): fără XAF
// Application, `EFCoreObjectSpaceProvider` standalone, idempotență prin
// `MigrareLegatura` (cheiată "1C:<view>"). Diferența de fond față de Migrare:
// documentele NU se copiază în registre, ci se OPEREAZĂ prin MotorOperare
// (pasul 3 al feliei încolo) — deschiderea rămâne singurul lucru scris direct.
//
// Baza țintă e DEDICATĂ și aparține uneltei: o migrează și o seed-uiește
// singură pe profilul Privat, exact calea updater-ului (ContaSeeder) — la fel
// ca ModelCheck pe baza lui privată (profil-per-bază, decizia 35d).
//
// PASUL 1 (felia 1C-b): schelet + citirea sursei + baza pregătită.
// PASUL 2: nomenclatoarele mici (integral) + helper-ele de import la cerere
// pentru cele mari (Nomenclatoare.cs).
// PASUL 3: DESCHIDEREA la 01.01.2025 — solduri contabile + loturi/stoc, scrise
// direct în registre cu `DocumentId = null` (Deschidere.cs). Reconcilierea
// deschiderii: pasul 4.

// Argumentele pozitionale sunt conexiunile; flag-urile „--" se filtrează.
var pozitionale = args.Where(a => !a.StartsWith("--")).ToArray();

var flaxCs = pozitionale.Length > 0
    ? pozitionale[0]
    : "Server=(local);Database=EServicesFlx;Integrated Security=True;TrustServerCertificate=True";
var pgCs = pozitionale.Length > 1
    ? pozitionale[1]
    : "Host=localhost;Port=5444;Username=postgres;Password=postgres;Database=Atlas.Conta.Import1C.Flax";

// Anul fiscal importat: deschiderea = soldurile la 01.01 (design §3).
var dataDeschidere = new DateTime(2025, 1, 1);

var esecuri = 0;
void Check(string nume, bool ok) {
    Console.WriteLine($"{(ok ? "OK  " : "FAIL")} {nume}");
    if (!ok)
        esecuri++;
}
var avertismente = new List<string>();
void Avert(string mesaj) => avertismente.Add(mesaj);

// ==================== Faza 0: baza țintă (migrare + seed) ====================

var opts = new DbContextOptionsBuilder<BackOfficeEFCoreDbContext>()
    .UseNpgsql(pgCs)
    .UseChangeTrackingProxies()
    .Options;

using (var ctx = new BackOfficeEFCoreDbContext(opts)) {
    await ctx.Database.MigrateAsync();
    Console.WriteLine($"Bază țintă migrată: {ctx.Model.GetEntityTypes().Count()} entity types.");
}

using var provider = new EFCoreObjectSpaceProvider<BackOfficeEFCoreDbContext>(
    (builder, _) => builder
        .UseNpgsql(pgCs)
        .UseChangeTrackingProxies()
        .UseObjectSpaceLinkProxies()
        .UseLazyLoadingProxies());

// Seed-ul profilului privat pe calea updater-ului; idempotent la re-rulare, iar
// `VerificaProfil` din seeder protejează ancora (o bază cu alt plan e refuzată).
using (var os = provider.CreateObjectSpace()) {
    ContaSeeder.Seed(os, ProfilContabil.Privat);
    os.CommitChanges();
}
Console.WriteLine($"Seed profil Privat aplicat pe „{pgCs.Split("Database=")[^1]}”.");

// Amprenta seed-ului: la o a doua rulare trebuie să fie IDENTICĂ (seed-ul e
// incremental, nu aditiv) — proba de idempotență a fazei 0.
using (var os = provider.CreateObjectSpace()) {
    Console.WriteLine($"Seed: {os.GetObjectsCount(typeof(TipDocument), null)} tipuri document, "
        + $"{os.GetObjectsCount(typeof(Cont), null)} conturi, "
        + $"{os.GetObjectsCount(typeof(TipMaterial), null)} tipuri material, "
        + $"{os.GetObjectsCount(typeof(TipTva), null)} tipuri TVA, "
        + $"{os.GetObjectsCount(typeof(RegulaContare), null)} reguli contare, "
        + $"{os.GetObjectsCount(typeof(RegulaStoc), null)} reguli stoc.");
}

// ==================== Maparea planului de conturi ====================
// (Idempotența — `Legaturi.Incarca`/`Leaga` — trăiește în Legaturi.cs.)
//
// Codurile 1C sunt PUNCTATE cu altă semantică decât cele legacy: punctul separă
// cifrele contului sintetic, nu analiticul (`442.6` = 4426 TVA, nu „442
// analitic 6"). De aici mecanica proprie: se CONCATENEAZĂ segmentele, iar dacă
// simbolul rezultat nu există în planul OMFP se taie ultimul segment și se reia
// (`121.25` → `12125`? nu → `121` da). Override-urile rămân pentru cazurile în
// care mecanica greșește — se populează pe măsură ce importul le scoate
// (decizia 21: fiecare gaură = decizie explicită, nu transcriere).
//
// Cele 9 de mai jos sunt exact găurile scoase de smoke-ul pasului 2 (4 coduri
// nerezolvabile + 5 solduri care cădeau pe conturi SUMATOARE). Fiecare e o
// decizie de profil, luată pe denumirea contului din ambele planuri — nu o
// transcriere mecanică (decizia 21/35b).
Dictionary<string, string> overrideCont = new() {
    // 1C ține subvențiile pentru investiții pe „132."; OMFP are aceeași
    // denumire pe 4752 („Împrumuturi nerambursabile cu caracter de subvenții").
    ["132"] = "4752",
    // Impozitul pe dividende → „Alte impozite, taxe și vărsăminte asimilate".
    ["4465"] = "446",
    // Contribuțiile salariatului: 43111 = CASS (4316), 43115 = CAS (4315).
    ["43111"] = "4316",
    ["43115"] = "4315",
    // Profitul nerepartizat per an (117.21/22/23) → 1171; anul se pierde ca
    // analitic, iar 117 e SUMATOR în OMFP.
    ["117.21"] = "1171",
    ["117.22"] = "1171",
    ["117.23"] = "1171",
    // Terenuri: 211 e sumator în OMFP, soldul aparține lui 2111.
    ["211.3"] = "2111",
    // Clienți din străinătate: 411 e sumator, soldul aparține lui 4111.
    ["411.2"] = "4111",
};

string MapeazaCont(string cod1C, IReadOnlyDictionary<string, Guid> plan) {
    var s = cod1C?.Trim().TrimEnd('.') ?? "";
    if (s.Length == 0)
        return null;
    if (overrideCont.TryGetValue(s, out var forțat))
        return plan.ContainsKey(forțat) ? forțat : null;
    var segmente = s.Split('.', StringSplitOptions.RemoveEmptyEntries).ToList();
    while (segmente.Count > 0) {
        var candidat = string.Concat(segmente);
        if (plan.ContainsKey(candidat))
            return candidat;
        segmente.RemoveAt(segmente.Count - 1);
    }
    return null;
}

// ======================= Faza Nomenclatoare (pasul 2) =======================
// Nomenclatoarele MICI se importă integral: sunt laturi de document (gestiuni,
// conturi proprii, angajați) și trebuie să existe înainte de orice document.
// Cele MARI (parteneri, nomenclator) rămân la cerere — vezi Nomenclatoare.cs.

using var flax = new FlaxDb(flaxCs);

var depozite = flax.Depozite();
var casierii = flax.Casierii();
var conturiBancare = flax.ConturiBancareProprii();
var persoane = flax.PersoaneFizice();

(int Procesate, int Noi) impDepozite, impCasierii, impConturi, impPersoane;
using (var os = provider.CreateObjectSpace()) {
    var plan = os.GetObjectsQuery<Cont>().ToDictionary(c => c.Simbol, c => c.ID);
    var sumatori = os.GetObjectsQuery<Cont>().Where(c => c.Sumator).Select(c => c.Simbol).ToHashSet();

    impDepozite = Nomenclatoare.Depozite(os, depozite, Avert);
    impCasierii = Nomenclatoare.Casierii(os, casierii, plan, Avert);
    impConturi = Nomenclatoare.ConturiBancare(os, conturiBancare, plan, sumatori,
        cod => MapeazaCont(cod, plan), Avert);
    impPersoane = Nomenclatoare.PersoaneFizice(os, persoane, Avert);
}

Console.WriteLine($"\n--- Nomenclatoare importate (procesate / noi în rularea asta) ---");
Console.WriteLine($"Depozite → Gestiune:            {impDepozite.Procesate,5} / {impDepozite.Noi}");
Console.WriteLine($"Casierii → ContPropriu:         {impCasierii.Procesate,5} / {impCasierii.Noi}");
Console.WriteLine($"Conturi bancare → ContPropriu:  {impConturi.Procesate,5} / {impConturi.Noi}");
Console.WriteLine($"Persoane fizice → Angajat:      {impPersoane.Procesate,5} / {impPersoane.Noi}");

// ======================= Citirea sursei la deschidere =======================
// Se citește o singură dată, integral, și se folosește de toate fazele care
// urmează (deschiderea propriu-zisă + smoke-ul de la final).

var plan1C = flax.PlanConturi();
var solduri = flax.SolduriDeschidere(dataDeschidere);
var solduriPartener = flax.SolduriPartener(dataDeschidere);
var stoc = flax.StocDeschidere(dataDeschidere);
var stocOrfan = flax.StocFaraIdentitate(dataDeschidere);

// ==================== Faza DESCHIDERE (pasul 3) ====================
// Rândurile de deschidere poartă data ULTIMEI zile a anului precedent
// (precedentul pasului 4, decizia 34d): soldul e rezultatul lui 2024, iar
// documentele importate ale lui 2025 se așază după el, nu peste el.
var dataRanduri = DateOnly.FromDateTime(dataDeschidere.AddDays(-1));

// Snapshot-ul planului Atlas: maparea de cont se aplică în toate fazele.
Dictionary<string, Guid> planAtlas;
using (var os = provider.CreateObjectSpace())
    planAtlas = os.GetObjectsQuery<Cont>().ToDictionary(c => c.Simbol, c => c.ID);
string Mapeaza(string cod1C) => MapeazaCont(cod1C, planAtlas);

var cronometru = System.Diagnostics.Stopwatch.StartNew();

Console.WriteLine($"\n--- Deschiderea contabilă la {dataRanduri:yyyy-MM-dd} ---");
var extrabilantiere1C = plan1C.Where(c => c.Extrabilantier).Select(c => c.Cod).ToHashSet();
var rezContabil = Deschidere.Contabile(provider, solduri, extrabilantiere1C, Mapeaza,
    dataRanduri, Avert, Check);
Console.WriteLine($"Registru contabil: {rezContabil.Randuri} rânduri contra ancorei "
    + $"{Deschidere.Ancora}; extrabilanțiere sărite: {rezContabil.Extrabilantiere} "
    + $"({rezContabil.SumaExtrabilantiera:N2}); reziduul propriu al sursei pe ancoră: "
    + $"{rezContabil.ReziduuAncora:N2}.");

Console.WriteLine($"\n--- Stocul de deschidere la {dataRanduri:yyyy-MM-dd} ---");
var laCerere = new ImportLaCerere(provider, flax, Avert);
var rezStoc = Deschidere.Stoc(provider, laCerere, stoc, Mapeaza, dataRanduri, Avert, Check);
Console.WriteLine($"Loturi: {rezStoc.Loturi} (noi în rularea asta: {rezStoc.LoturiNoi}) "
    + $"din {stoc.Count} poziții; produse: {rezStoc.Produse} distincte "
    + $"({rezStoc.ProduseNoi} noi, {rezStoc.ProduseFaraTip} refuzate).");
Console.WriteLine($"Data lotului parsată din descrierea documentului creator: "
    + $"{rezStoc.Loturi - rezStoc.DateNeparsate}/{rezStoc.Loturi} "
    + $"({(rezStoc.Loturi == 0 ? 0 : 100.0 * (rezStoc.Loturi - rezStoc.DateNeparsate) / rezStoc.Loturi):N1}%).");
Console.WriteLine($"Netare: {rezStoc.PozitiiNegative} celule negative absorbite; "
    + $"{rezStoc.GrupeSarite} grupe produs×depozit cu total negativ SĂRITE "
    + $"({rezStoc.CantitateSarita:N3} buc, {rezStoc.ValoareSarita:N2} lei); "
    + $"{rezStoc.CeluleDegenerate} celule rămase cu o singură coordonată nenulă.");
Console.WriteLine($"Registru stoc: {rezStoc.RanduriStoc} rânduri de deschidere; "
    + $"Σ {rezStoc.ValoareScrisa:N2} lei / {rezStoc.CantitateScrisa:N3} buc.");
Console.WriteLine($"La cerere: {laCerere.ParteneriNoi} parteneri noi, {laCerere.ProduseNoi} produse noi, "
    + $"{laCerere.Recuperate} recuperate, {laCerere.ReferinteMoarte} referințe moarte, "
    + $"{laCerere.ProduseFaraTip} fără TipMaterial.");
Console.WriteLine($"Durata fazei de deschidere: {cronometru.Elapsed:hh\\:mm\\:ss}.");

// Diferența stoc↔contabilitate a SURSEI: pozițiile orfane (fără produs sau fără
// depozit) rămân doar în soldul contabil, iar grupele total-negative nu se pot
// reprezenta. Se raportează, nu se ascund (34f).
Console.WriteLine($"Diferență stoc↔contabilitate a sursei: {stocOrfan.Count} poziții orfane "
    + $"({stocOrfan.Sum(s => s.Valoare):N2} lei) + {rezStoc.GrupeSarite} grupe total-negative "
    + $"({rezStoc.ValoareSarita:N2} lei) = {stocOrfan.Sum(s => s.Valoare) + rezStoc.ValoareSarita:N2} lei "
    + "diferență algebrică sold contabil − registru de stoc.");

// ============================ Faza SMOKE ============================
// Verifică invarianții pe care se sprijină pașii 3-4: planul Atlas are ancorele,
// maparea de cont funcționează pe formele reale, balanța 1C e echilibrată, iar
// stocul per lot reconciliază cu soldul contabil (dovada că BalantaNivel3 e o
// defalcare fidelă a Balanței).

Console.WriteLine($"\n--- Sursa 1C (flax), deschidere {dataDeschidere:yyyy-MM-dd} ---");
Console.WriteLine($"Depozite:          {depozite.Count} (elemente: {depozite.Count(d => d.EsteElement)}, marcate șterse: {depozite.Count(d => d.Marcat)})");
Console.WriteLine($"Casierii:          {casierii.Count}");
Console.WriteLine($"Conturi bancare:   {conturiBancare.Count} proprii (marcate șterse: {conturiBancare.Count(c => c.Marcat)})");
Console.WriteLine($"Persoane fizice:   {persoane.Count} (marcate șterse: {persoane.Count(p => p.Marcat)})");
Console.WriteLine($"Plan conturi 1C:   {plan1C.Count} ({plan1C.Count(c => c.Sintetic)} sintetice, {plan1C.Count(c => c.Extrabilantier)} extrabilanțiere)");

var pozitive = solduri.Where(s => s.SoldIni > 0).Sum(s => s.SoldIni);
var negative = solduri.Where(s => s.SoldIni < 0).Sum(s => s.SoldIni);
Console.WriteLine($"Solduri nenule:    {solduri.Count} conturi; Σ+ {pozitive:N2}; Σ− {negative:N2}");
Console.WriteLine($"Solduri partener:  {solduriPartener.Count} (cont × partener)");
Console.WriteLine($"Poziții stoc:      {stoc.Count}; Σ valoare {stoc.Sum(s => s.Valoare):N2}; negative: {stoc.Count(s => s.Valoare < 0)}");

Check($"Balanța 1C e echilibrată: Σ+ {pozitive:N2} = |Σ−| {Math.Abs(negative):N2}",
    Math.Abs(pozitive + negative) < 0.005m);

// BalantaNivel3 trebuie să fie o defalcare EXACTĂ a soldului contabil: fiecare
// leu de pe un cont de stoc trebuie să se regăsească fie într-o poziție
// importabilă (produs × document × depozit ⇒ lot), fie într-una orfană (fără
// produs sau fără depozit ⇒ NU poate deveni lot). Invariantul verificat e
// „importabil + orfan = sold" — adică nu pierdem bani pe drum; orfanii se
// raportează separat, ca decizie explicită a pasului 3 (34f: diferențele
// sursei se raportează, nu se ascund).
Console.WriteLine($"Poziții orfane:    {stocOrfan.Count}; Σ valoare {stocOrfan.Sum(s => s.Valoare):N2} "
    + "(fără produs sau fără depozit — neimportabile ca lot)");
foreach (var cont in stoc.Select(s => s.Cont).Concat(stocOrfan.Select(s => s.Cont))
             .Concat(solduri.Where(s => s.Cont.StartsWith('3')).Select(s => s.Cont))
             .Distinct().OrderBy(c => c)) {
    var sold = solduri.Where(s => s.Cont == cont).Sum(s => s.SoldIni);
    var importabil = stoc.Where(s => s.Cont == cont).Sum(s => s.Valoare);
    var orfan = stocOrfan.Where(s => s.Cont == cont).Sum(s => s.Valoare);
    Check($"Stoc {cont,-8} {stoc.Count(s => s.Cont == cont),6} poziții Σ {importabil,15:N2}"
        + $" + orfan {orfan,10:N2} = sold contabil {sold,15:N2}",
        Math.Abs(importabil + orfan - sold) < 0.005m);
    if (orfan != 0 || stocOrfan.Any(s => s.Cont == cont))
        Avert($"Cont de stoc {cont}: {stocOrfan.Count(s => s.Cont == cont)} poziții orfane "
            + $"({orfan:N2} lei, {stocOrfan.Where(s => s.Cont == cont).Sum(s => s.Cantitate):N3} buc) "
            + "fără produs/depozit — nu au devenit loturi; rămân doar în soldul contabil "
            + "(diferență a sursei, raportată).");
    if (importabil == 0 && orfan == 0 && sold != 0)
        Avert($"Cont de stoc {cont} are sold {sold:N2} dar NICIO poziție în BalantaNivel3 "
            + "— deschiderea lui nu are detaliu de lot (diferență a sursei).");
}

// ==================== Planul Atlas: ancore + maparea de cont ====================

using (var os = provider.CreateObjectSpace()) {
    var plan = os.GetObjectsQuery<Cont>().ToDictionary(c => c.Simbol, c => c.ID);
    var sumatori = os.GetObjectsQuery<Cont>().Where(c => c.Sumator).Select(c => c.Simbol).ToHashSet();
    Console.WriteLine($"\n--- Planul Atlas (OMFP 1802) ---");
    Console.WriteLine($"Conturi: {plan.Count} ({sumatori.Count} sumatoare).");

    // Ancora deschiderii (echivalentul lui 891.01.00 din profilul bugetar).
    var simbol891 = plan.Keys.Where(s => s.StartsWith("891")).OrderBy(s => s.Length).ToList();
    Console.WriteLine($"Ancora bilanț de deschidere: {(simbol891.Count > 0 ? string.Join(", ", simbol891) : "LIPSEȘTE")}");
    Check("Planul OMFP conține contul de bilanț de deschidere (891)", simbol891.Count > 0);

    // Maparea 1C→OMFP pe formele reale ale codurilor punctate.
    (string Cod1C, string Asteptat)[] probe = [
        ("442.6", "4426"), ("121.25", "121"), ("401.1", "401"), ("371.1", "371"), ("401.", "401"),
    ];
    foreach (var (cod, asteptat) in probe) {
        var rezolvat = MapeazaCont(cod, plan);
        Check($"MapeazaCont({cod}) = {rezolvat ?? "null"} (așteptat {asteptat})", rezolvat == asteptat);
    }

    // Acoperirea reală: câte conturi 1C cu sold se rezolvă în planul OMFP.
    var nerezolvate = solduri
        .Select(s => (s.Cont, s.SoldIni, Simbol: MapeazaCont(s.Cont, plan)))
        .Where(x => x.Simbol == null)
        .ToList();
    Console.WriteLine($"Conturi 1C cu sold nerezolvabile în OMFP: {nerezolvate.Count} / {solduri.Count}"
        + (nerezolvate.Count > 0 ? $" ({nerezolvate.Sum(x => Math.Abs(x.SoldIni)):N2} în valoare absolută)" : ""));
    foreach (var x in nerezolvate.OrderByDescending(x => Math.Abs(x.SoldIni)))
        Avert($"Cont 1C {x.Cont} (sold {x.SoldIni:N2}) nu se mapează pe planul OMFP "
            + "— cere un rând în `overrideCont` (deschiderea a eșuat deja pe el).");

    // Soldurile nu au voie să cadă pe un cont SUMATOR — dar rezolvarea NU e
    // aici: 1C ține soldul pe sinteticul de gradul I (`411.`, `211.`) acolo
    // unde OMFP cere gradul II (4111, 2111…), iar alegerea copilului corect e o
    // decizie de profil, pe date (decizia 21/45f: profilul se completează pe
    // parcurs, fiecare gaură = decizie explicită). Se raportează cu sumele, ca
    // intrare a pasului 3 — care le tranșează prin `overrideCont`.
    var peSumator = solduri
        .Select(s => (s.Cont, s.SoldIni, Simbol: MapeazaCont(s.Cont, plan)))
        .Where(x => x.Simbol != null && sumatori.Contains(x.Simbol))
        .ToList();
    Console.WriteLine($"Solduri care cad pe cont SUMATOR (cer override): {peSumator.Count}");
    foreach (var x in peSumator.OrderByDescending(x => Math.Abs(x.SoldIni)))
        Avert($"Cont 1C {x.Cont} (sold {x.SoldIni:N2}) → {x.Simbol}, care e SUMATOR în OMFP "
            + "— cere override pe un cont de gradul II (deschiderea a eșuat deja pe el).");

    // Acoperirea Clasă/Tip a stocului de deschidere. Fiecare poziție va cere un
    // `Produs` cu TipMaterial = Tipul al cărui Cod E simbolul contului (decizia
    // 26b) — deci un simbol fără Tip în profil oprește importul lotului. Se
    // raportează AICI, agregat, ca intrare a pasului 3; altfel gaura ar apărea
    // abia la a 11.762-a poziție, câte un avertisment pe fiecare.
    var tipuriProfil = os.GetObjectsQuery<TipMaterial>().Select(t => t.Cod).ToHashSet();
    Console.WriteLine("Acoperirea Clasă/Tip a pozițiilor de stoc (cont 1C → simbol OMFP → Tip în profil):");
    foreach (var grup in stoc
                 .GroupBy(s => (s.Cont, Simbol: MapeazaCont(s.Cont, plan)))
                 .OrderByDescending(g => Math.Abs(g.Sum(s => s.Valoare)))) {
        var are = grup.Key.Simbol != null && tipuriProfil.Contains(grup.Key.Simbol);
        Console.WriteLine($"     {grup.Key.Cont,-10} → {grup.Key.Simbol ?? "(nemapat)",-8} "
            + $"{grup.Count(),6} poziții  Σ {grup.Sum(s => s.Valoare),15:N2}  "
            + (are ? "Tip OK" : "TIP LIPSĂ"));
        if (!are)
            Avert($"Stoc pe contul 1C {grup.Key.Cont} → OMFP „{grup.Key.Simbol ?? "(nemapat)"}”: "
                + $"profilul privat NU are TipMaterial cu acest cod — {grup.Count()} poziții "
                + $"({grup.Sum(s => s.Valoare):N2} lei) nu pot deveni loturi. Gaură de profil, "
                + "de tranșat la pasul 3 (Tip nou sau tăiere la sintetic).");
    }
}

// ================= Verificarea nomenclatoarelor (pasul 2) =================
// Contorul „noi" de mai sus arată ce s-a creat în rularea CURENTĂ; el nu poate
// fi verificat (0 la a doua rulare, >0 la prima). Invariantul verificabil pe
// ORICE rulare e altul: o legătură per rând-sursă, o entitate per legătură,
// coduri distincte. Un import care dublează sparge prima verificare (2× legături
// pentru aceeași sursă) — deci idempotența e prinsă, nu doar observată.

using (var os = provider.CreateObjectSpace()) {
    var gestiuni = os.GetObjectsQuery<Gestiune>().ToList();
    var conturiProprii = os.GetObjectsQuery<ContPropriu>().ToList();
    var angajati = os.GetObjectsQuery<Angajat>().ToList();
    var parteneriNoi = os.GetObjectsCount(typeof(Partener), null);
    var produseNoi = os.GetObjectsCount(typeof(Produs), null);

    Console.WriteLine($"\n--- Nomenclatoare în Postgres ---");
    Console.WriteLine($"Gestiuni: {gestiuni.Count}, conturi proprii: {conturiProprii.Count}, "
        + $"angajați: {angajati.Count}, parteneri: {parteneriNoi}, produse: {produseNoi}, "
        + $"loturi: {os.GetObjectsCount(typeof(Lot), null)}.");

    void VerificaSursa<T>(string view, int randuriSursa, IReadOnlyList<T> entitati, int dinSeed)
            where T : Repartitor {
        var legaturi = Legaturi.Incarca(os, view);
        var dupaId = entitati.ToDictionary(e => e.ID);
        var legate = legaturi.Values.Where(dupaId.ContainsKey).ToList();
        Check($"1C:{view}: {legaturi.Count} legături = {randuriSursa} rânduri sursă importabile",
            legaturi.Count == randuriSursa);
        Check($"1C:{view}: {legate.Count} entități legate (nicio legătură orfană)",
            legate.Count == legaturi.Count);
        var coduri = legate.Select(id => dupaId[id].Cod).ToList();
        Check($"1C:{view}: {coduri.Distinct().Count()} coduri distincte pe {coduri.Count} entități "
            + "(fără duplicate)", coduri.Distinct().Count() == coduri.Count);
        var neLegate = entitati.Count(e => !legaturi.Values.Contains(e.ID));
        if (neLegate != dinSeed)
            Avert($"1C:{view}: {neLegate} entități {typeof(T).Name} fără legătură 1C "
                + $"(așteptate {dinSeed} din seed) — de verificat dacă importul a dublat.");
    }

    // Cele două ContProprii din seed (CASA/BANCA) sunt împărțite între cele două
    // view-uri sursă, deci se numără o singură dată — la casierii.
    VerificaSursa("Depozite", depozite.Count(d => d.EsteElement), gestiuni, dinSeed: 2);
    VerificaSursa("Casierii", casierii.Count, conturiProprii,
        dinSeed: 2 + conturiBancare.Count);
    VerificaSursa("ConturiBancare", conturiBancare.Count, conturiProprii,
        dinSeed: 2 + casierii.Count);
    VerificaSursa("PersoaneFizice", persoane.Count(p => p.EsteElement), angajati, dinSeed: 0);

    // Contul implicit al conturilor proprii e maparea cea mai consecventă:
    // contarea PLT/INC îl ia ca latură de trezorerie FĂRĂ fallback (31c), deci
    // un null aici devine eroare abia la operarea plății. Se afișează distribuția.
    var peCont = conturiProprii
        .GroupBy(c => c.ContImplicit?.Simbol ?? "(fără cont)")
        .OrderBy(g => g.Key);
    Console.WriteLine("Conturi proprii pe cont implicit: "
        + string.Join(", ", peCont.Select(g => $"{g.Key} × {g.Count()}")));
    Check($"Toate conturile proprii au cont implicit",
        conturiProprii.All(c => c.ContImplicitId != null));

    Console.WriteLine($"Legături la cerere: parteneri {Legaturi.Incarca(os, "Partenerii").Count}, "
        + $"nomenclator {Legaturi.Incarca(os, "Nomenclator").Count}.");
}

// ==================== Rezumat ====================

foreach (var a in avertismente)
    Console.WriteLine($"AVERT {a}");
Console.WriteLine(esecuri == 0
    ? "\nSmoke 1C încheiat fără eșecuri."
    : $"\nSmoke 1C încheiat cu {esecuri} eșecuri.");
return esecuri == 0 ? 0 : 1;
