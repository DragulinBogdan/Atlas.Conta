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
// direct în registre cu `DocumentId = null` (Deschidere.cs).
// PASUL 4: RECONCILIEREA deschiderii (Reconciliere.cs) — contractul design §8:
// baza se RECITEȘTE din Postgres și se compară cu sursa brută, independent de
// structurile fazei 3.

// Argumentele pozitionale sunt conexiunile; restul sunt flag-uri „--".
// Parsarea e explicită (nu un filtru pe prefix) fiindcă `--pana-la` are VALOARE:
// un filtru ar lăsa „3" să treacă drept connection string.
var pozitionale = new List<string>();
// Auto-testul reconcilierii (`--sabotaj`): alterează cu 1 leu rânduri deja
// SCRISE, între deschidere și reconciliere. E singura cale onestă de a dovedi
// sensibilitatea — deschiderea se rescrie integral la fiecare rulare, deci o
// alterare făcută înaintea ei ar fi reparată de ea însăși, iar una făcută pe
// sursă n-ar testa citirea din bază. Flag-ul rămâne ca unealtă permanentă:
// contractul de reconciliere e cod ca oricare altul și trebuie să poată fi
// verificat că mai e viu, nu doar că e verde.
var sabotaj = false;
// `--pana-la N` = importă lunile 1..N (măsurătoarea cerută de §12.4 pornește de
// la ianuarie singur); `--continua` = o lună picată nu oprește rularea, se
// raportează și diferențele se poartă înainte.
var panaLa = 12;
var continua = false;
// `--cititori` = auto-testul contractului de coloane (SmokeCititori.cs): cheamă
// o dată fiecare cititor de document pe prima lună a ferestrei. Opt-in fiindcă e
// probă de FORMĂ, nu de conținut — se rulează după orice regenerare de view-uri.
var smokeCititori = false;
// `--recreeaza` = baza țintă se ARUNCĂ și se reconstruiește de la zero. Baza de
// import e de unică folosință prin design (§3), iar documentele deja operate nu
// se re-importă (idempotența e pe legături): când o convenție a sursei se
// corectează — cum a fost conversia valutară a facturilor — singura cale onestă
// spre un import curat e reconstrucția, nu peticirea rândurilor scrise.
var recreeaza = false;
// `--diag=<hexProdus>` = interogare de diagnostic pe baza existentă (nu importă
// nimic): mișcările de stoc ale unui produs 1C, cu documentul și gestiunea.
string diagProdus = null;
// `--deblocheaza <view>:<cheieHex>[,…]` (repetabil) = comanda de OPERATOR care dă
// înapoi artefactele unei rulări anterioare pentru un document-sursă anume (D3):
// puntea operată se stornează, drafturile se șterg, legăturile pleacă, iar
// rularea următoare replanifică documentul integral. Nu se face automat:
// dezlegarea unei punți vechi înseamnă riscul dublei postări, deci cere o decizie.
var deblocari = new List<(string View, string Cheie)>();
for (var i = 0; i < args.Length; i++) {
    var arg = args[i];
    if (!arg.StartsWith("--")) {
        pozitionale.Add(arg);
        continue;
    }
    var (nume, valoare) = arg.Split('=', 2) is [var n, var v] ? (n, v) : (arg, null);
    switch (nume) {
        case "--sabotaj":
            sabotaj = true;
            break;
        case "--continua":
            continua = true;
            break;
        case "--cititori":
            smokeCititori = true;
            break;
        case "--recreeaza":
            recreeaza = true;
            break;
        case "--diag":
            diagProdus = valoare ?? (i + 1 < args.Length ? args[++i] : null);
            if (string.IsNullOrWhiteSpace(diagProdus)) {
                Console.Error.WriteLine("--diag cere hex-ul 1C al unui produs.");
                return 2;
            }
            break;
        case "--deblocheaza":
            valoare ??= i + 1 < args.Length ? args[++i] : null;
            if (valoare == null || !Deblocare.Parseaza(valoare, deblocari)) {
                Console.Error.WriteLine("--deblocheaza cere <view>:<cheieHex> (repetabil sau "
                    + $"separat prin virgulă); primit „{valoare}”.");
                return 2;
            }
            break;
        case "--pana-la":
            valoare ??= i + 1 < args.Length ? args[++i] : null;
            if (!int.TryParse(valoare, out panaLa) || panaLa is < 1 or > 12) {
                Console.Error.WriteLine($"--pana-la cere o lună între 1 și 12 (primit „{valoare}”).");
                return 2;
            }
            break;
        default:
            Console.Error.WriteLine($"Argument necunoscut: {arg}. Uzaj: Import1C [flaxCs] [pgCs] "
                + "[--pana-la <lună>] [--continua] [--sabotaj] [--cititori] [--recreeaza] "
                + "[--deblocheaza <view>:<cheie>]");
            return 2;
    }
}

var flaxCs = pozitionale.Count > 0
    ? pozitionale[0]
    : "Server=(local);Database=EServicesFlx;Integrated Security=True;TrustServerCertificate=True";
var pgCs = pozitionale.Count > 1
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
    if (recreeaza) {
        await ctx.Database.EnsureDeletedAsync();
        Console.WriteLine("*** --recreeaza: baza țintă a fost ștearsă; se reconstruiește de la zero. ***");
    }
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
// Dicționarul 1C→OMFP e o LISTĂ, nu o regulă (decizia 48c): trăiește în
// `mapari-conturi.csv`, comentat rând cu rând, iar mecanica generică (concatenare
// + tăiere de segmente) rămâne doar pentru codurile pe care nimeni nu le-a
// contestat. Vezi MapariConturi.cs pentru mecanică și pentru forma de livrare.
var caleMapari = MapariConturi.CaleImplicita();
if (!File.Exists(caleMapari)) {
    Console.Error.WriteLine($"Lipsește dicționarul de mapare a conturilor: {caleMapari}");
    return 2;
}
var mapari = MapariConturi.Incarca(caleMapari, Avert);
Console.WriteLine($"Mapări de cont încărcate din „{caleMapari}”: {mapari.Randuri.Count} rânduri.");

string MapeazaCont(string cod1C, IReadOnlyDictionary<string, Guid> plan) =>
    mapari.Mapeaza(cod1C, plan);

// Snapshot-ul planului Atlas, luat o dată, imediat după seed: maparea de cont,
// pre-flight-ul și toate fazele lucrează pe el (planul nu se mai schimbă).
Dictionary<string, Guid> planAtlas;
HashSet<string> sumatoriAtlas;
Dictionary<string, string> denumiriAtlas;
Dictionary<string, List<string>> copiiAtlas;
using (var os = provider.CreateObjectSpace()) {
    var conturi = os.GetObjectsQuery<Cont>()
        .Select(c => new { c.Simbol, c.ID, c.Sumator, c.Denumire, Parinte = c.Parinte.Simbol })
        .ToList();
    planAtlas = conturi.ToDictionary(c => c.Simbol, c => c.ID);
    sumatoriAtlas = conturi.Where(c => c.Sumator).Select(c => c.Simbol).ToHashSet();
    denumiriAtlas = conturi.ToDictionary(c => c.Simbol, c => c.Denumire);
    copiiAtlas = conturi.Where(c => c.Parinte != null)
        .GroupBy(c => c.Parinte)
        .ToDictionary(g => g.Key, g => g.Select(c => c.Simbol).ToList());
}
string Mapeaza(string cod1C) => MapeazaCont(cod1C, planAtlas);

// `--diag=<hexProdus>`: interogarea țintită de diagnostic (Diagnostic.cs), pe
// baza deja importată. Rulează aici — după seed, înaintea fazelor scumpe — și
// oprește procesul: e o întrebare pusă bazei, nu o rulare de import.
if (diagProdus != null) {
    Diagnostic.Produs(provider, diagProdus, Avert);
    return 0;
}

// ======================= Faza PRE-FLIGHT (decizia 48c) =======================
// Rulează după seed și ÎNAINTEA oricărui import: triajul conturilor și
// inventarul tipurilor de document-sursă se emit ca raport unic, nu descoperit
// în mers. Vezi PreFlight.cs.

using var flax = new FlaxDb(flaxCs);
var plan1C = flax.PlanConturi();

// `panaLa` intră în pre-flight fiindcă una dintre verificări e a FERESTREI, nu a
// anului: cotele de TVA de 21% lipsesc din view-urile stale abia din august, iar
// o rulare pe ianuarie n-are de ce să pice pentru date pe care nu le atinge.
var preflight = PreFlight.Executa(flax, dataDeschidere.Year, panaLa, mapari, planAtlas,
    sumatoriAtlas, denumiriAtlas, copiiAtlas, plan1C, Handlere.Cunoscute, Handlere.Implementate,
    Avert, Check);

if (smokeCititori)
    SmokeCititori.Executa(flax, dataDeschidere.Year, 1, Check);

// ======================= Faza DEBLOCARE (D3, opt-in) =======================
// Rulează după pre-flight și ÎNAINTEA oricărei faze care citește baza în memorie
// (catalogul de loturi, indexurile buclei): artefactele date înapoi trebuie să
// dispară înainte ca cineva să le indexeze. Un refuz oprește rularea — nu se
// importă peste o deblocare pe jumătate făcută.
if (deblocari.Count > 0 && !Deblocare.Executa(provider, dataDeschidere.Year, deblocari, Avert, Check)) {
    foreach (var a in avertismente)
        Console.WriteLine($"AVERT {a}");
    Console.Error.WriteLine("\nDeblocarea a eșuat — importul nu pornește.");
    return 1;
}

// ======================= Faza Nomenclatoare (pasul 2) =======================
// Nomenclatoarele MICI se importă integral: sunt laturi de document (gestiuni,
// conturi proprii, angajați) și trebuie să existe înainte de orice document.
// Cele MARI (parteneri, nomenclator) rămân la cerere — vezi Nomenclatoare.cs.

var depozite = flax.Depozite();
var casierii = flax.Casierii();
var conturiBancare = flax.ConturiBancareProprii();
var persoane = flax.PersoaneFizice();

(int Procesate, int Noi) impDepozite, impCasierii, impConturi, impPersoane;
using (var os = provider.CreateObjectSpace()) {
    impDepozite = Nomenclatoare.Depozite(os, depozite, Avert);
    impCasierii = Nomenclatoare.Casierii(os, casierii, planAtlas, Avert);
    impConturi = Nomenclatoare.ConturiBancare(os, conturiBancare, planAtlas, sumatoriAtlas,
        Mapeaza, Avert);
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

var solduri = flax.SolduriDeschidere(dataDeschidere);
var solduriPartener = flax.SolduriPartener(dataDeschidere);
var stoc = flax.StocDeschidere(dataDeschidere);
var stocOrfan = flax.StocFaraIdentitate(dataDeschidere);

// ==================== Faza DESCHIDERE (pasul 3) ====================
// Rândurile de deschidere poartă data ULTIMEI zile a anului precedent
// (precedentul pasului 4, decizia 34d): soldul e rezultatul lui 2024, iar
// documentele importate ale lui 2025 se așază după el, nu peste el.
var dataRanduri = DateOnly.FromDateTime(dataDeschidere.AddDays(-1));

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
var durataDeschidere = cronometru.Elapsed;
Console.WriteLine($"Durata fazei de deschidere: {durataDeschidere:hh\\:mm\\:ss}.");

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

    // Produsele se numără și pe nomenclatoare distincte: identitatea de import e
    // (nomenclator × cont), deci diferența dintre cele două cifre = gemenii.
    var produseLegate = Legaturi.Incarca(os, ImportLaCerere.ViewProduse);
    var nomenclatoare = produseLegate.Keys
        .Select(ImportLaCerere.NomenclatorDinCheie).Distinct().Count();
    Console.WriteLine($"Legături la cerere: parteneri {Legaturi.Incarca(os, "Partenerii").Count}, "
        + $"produse {produseLegate.Count} pe {nomenclatoare} nomenclatoare 1C "
        + $"({produseLegate.Count - nomenclatoare} gemeni pe conturi separate).");
}

// ==================== Faza RECONCILIERE (pasul 4) ====================
// Contractul design §8, restrâns la deschidere. Recitește TOTUL din Postgres și
// compară cu sursa brută — vezi principiul din Reconciliere.cs.

if (sabotaj) {
    using var os = provider.CreateObjectSpace();
    var rc = os.GetObjectsQuery<RegistruContabil>()
        .Where(r => r.DocumentId == null)
        .OrderBy(r => r.Valoare).ThenBy(r => r.ID)
        .FirstOrDefault();
    var rs = os.GetObjectsQuery<RegistruStoc>()
        .Where(r => r.DocumentId == null)
        .OrderBy(r => r.Valoare).ThenBy(r => r.ID)
        .FirstOrDefault();
    if (rc != null)
        rc.Valoare += 1m;
    if (rs != null)
        rs.Valoare += 1m;
    os.CommitChanges();
    Console.WriteLine($"\n*** SABOTAJ (--sabotaj): +1 leu pe rândul contabil {rc?.ID} și pe "
        + $"rândul de stoc {rs?.ID}. Reconcilierea TREBUIE să pice. ***");
}

Console.WriteLine($"\n=== Reconcilierea deschiderii la {dataRanduri:yyyy-MM-dd} ===");
var rezRec = Reconciliere.Executa(provider, solduri, extrabilantiere1C, stoc, stocOrfan,
    rezStoc.DiferenteJustificate, Mapeaza, Avert, Check);

// ==================== Faza DOCUMENTE: bucla lunară (pasul 1) ====================
// Perioadele fiscale întâi (motorul tratează perioada LIPSĂ ca închisă, decizia
// 14), apoi bucla: documente → imperecheri → ITV → reconciliere lunară.
// Handler-ele per tip 1C se înregistrează în `Handlere.Toate` (pașii 3–5); azi
// lista e goală, deci bucla rulează în gol — deliberat, ca infrastructura să fie
// verificabilă înaintea primului tip.

var anImport = dataDeschidere.Year;
var (perioadeExistente, perioadeCreate) = Perioade.Asigura(provider, anImport);
Console.WriteLine($"\n=== Documentele {anImport} (lunile 1..{panaLa}) ===");
Console.WriteLine($"Perioade fiscale {anImport}: {perioadeExistente} existente, {perioadeCreate} create "
    + "(deschise — perioada lipsă e tratată ca închisă de gardian).");

// Catalogul (pasul 3) se construiește DUPĂ nomenclatoare și după deschidere:
// citește gestiunile legate, Clasă/Tip, cotele de TVA, regulile de contare și
// indexul de loturi pe care le folosesc handlerele.
var catalog = new Catalog(provider, flax, anImport, Mapeaza, planAtlas, Avert);
Console.WriteLine($"Catalog: {catalog.Gestiuni.Count} gestiuni legate, "
    + $"plusul de inventar contează pe {catalog.ContPlusInventar}.");

var bucla = new BuclaImport(provider, flax, laCerere, new AlocareIesire(), catalog, Avert, Check);
// Intrarea contractului lunar (pasul 6): ce a lăsat deschiderea în urmă —
// produsele ale căror prețuri per lot le-a rearanjat netarea, diferențele deja
// raportate ale sursei și clasa 8 a planului 1C (nu intră în bilanț).
bucla.StareContract.ProduseNetate = rezStoc.ProduseNetate;
bucla.StareContract.JustificateDeschidere = rezStoc.DiferenteJustificate;
bucla.StareContract.Extrabilantiere1C = extrabilantiere1C;
if (sabotaj)
    bucla.ActiveazaSabotajLuna();
var luni = new List<RezultatLuna>();
var lunaPicata = 0;
var cronometruDocumente = System.Diagnostics.Stopwatch.StartNew();
for (var luna = 1; luna <= panaLa; luna++) {
    var rez = bucla.ImportaLuna(anImport, luna);
    luni.Add(rez);
    // Verdictul lunii = importul ȘI contractul (§12.4, pasul 6): o lună în care
    // toate documentele au intrat, dar soldurile nu bat, e tot o lună picată.
    if (rez.Esecuri == 0 && rez.ContractePicate == 0)
        continue;
    // Stop dur implicit (§12.4): o lună picată oprește rularea cu raportul
    // complet; `--continua` o transformă în recoltare de găuri, cu diferențele
    // purtate înainte.
    lunaPicata = luna;
    if (!continua) {
        Avert($"Luna {luna:00}/{anImport} a picat cu {rez.Esecuri} eșecuri de import și "
            + $"{rez.ContractePicate} contracte de reconciliere picate — rularea se oprește "
            + "(stop dur implicit, §12.4). Rulează cu --continua pentru a recolta găurile "
            + "din toate lunile.");
        break;
    }
}
var durataDocumente = cronometruDocumente.Elapsed;

// Invariantul de IDEMPOTENȚĂ al importului de documente, verificabil pe ORICE
// rulare (contorul „importate" nu e: e 0 la a doua rulare prin construcție).
// Formularea: fiecare legătură 1C trimite la un document DISTINCT, iar
// documentele FĂRĂ legătură sunt exact cele născute de motor (conexul NIR, plata
// secundară) — un import care dublează sparge una dintre cele două.
//
// De ce nu „documente ne-autogenerate = legături" (forma pasului 3): pasul 4
// creează documente care sunt AMÂNDOUĂ — descărcarea de gestiune a unei facturi
// de ieșire e marcată `Autogenerat` + `DocumentSursa` (ca să intre în gardianul
// de grup al motorului), dar e construită de import și are legătura ei.
using (var os = provider.CreateObjectSpace()) {
    var documente = os.GetObjectsQuery<Document>()
        .Select(d => new { d.ID, d.Autogenerat }).ToList();
    var legate = os.GetObjectsQuery<MigrareLegatura>()
        .Where(m => m.Tabela.StartsWith("1C:")).Select(m => m.TintaId).ToList();
    var idsDocumente = documente.Select(d => d.ID).ToHashSet();
    var legateDocumente = legate.Where(idsDocumente.Contains).ToList();
    var idsLegate = legateDocumente.ToHashSet();
    var faraLegatura = documente.Where(d => !idsLegate.Contains(d.ID)).ToList();
    var orfaneNeautogenerate = faraLegatura.Count(d => !d.Autogenerat);
    Console.WriteLine($"\nDocumente în bază: {documente.Count} "
        + $"({faraLegatura.Count} fără legătură 1C — copii autogenerați de motor); "
        + $"legături 1C către documente: {legateDocumente.Count}.");
    Check($"Idempotență: {legateDocumente.Distinct().Count()} ținte distincte pe "
        + $"{legateDocumente.Count} legături (o legătură per document)",
        legateDocumente.Distinct().Count() == legateDocumente.Count);
    Check($"Idempotență: {orfaneNeautogenerate} documente fără legătură 1C și fără marcaj "
        + "de autogenerare (0 = niciun document dublat)", orfaneNeautogenerate == 0);
}
Console.WriteLine($"\nDocumente {anImport}: {luni.Sum(l => l.Documente)} importate, "
    + $"{luni.Sum(l => l.Sarite)} sărite, {luni.Sum(l => l.Copii)} copii autogenerați, "
    + $"{luni.Sum(l => l.Esecuri)} eșecuri, {luni.Sum(l => l.Realocari)} realocări de lot "
    + $"(supapa 48a) în {luni.Count} luni — {durataDocumente:hh\\:mm\\:ss}.");

// ==================== Sumarul rulării ====================
// Raportul pe care îl citește omul la fiecare rulare a deschiderii: cifrele-cheie
// ale întregii rulări, într-un singur loc, cu rezultatul contractului la coadă.

Console.WriteLine($"""

    ╔══════════════════ SUMARUL RULĂRII (deschidere {dataRanduri:yyyy-MM-dd}) ══════════════════
    ║ SURSA 1C ({flaxCs.Split("Database=")[^1].Split(';')[0]})
    ║   plan de conturi          {plan1C.Count,10} conturi ({plan1C.Count(c => c.Extrabilantier)} extrabilanțiere)
    ║   solduri de deschidere    {solduri.Count,10} conturi cu sold nenul
    ║   poziții de stoc          {stoc.Count,10} (Σ {stoc.Sum(s => s.Valoare):N2} lei) + {stocOrfan.Count} orfane (Σ {stocOrfan.Sum(s => s.Valoare):N2} lei)
    ║ PRE-FLIGHT (decizia 48c — triaj înaintea primului document)
    ║   coduri de cont {anImport}      {preflight.Coduri,10} ({preflight.PrinDictionar} prin dicționar, {preflight.PrinMecanica} prin mecanică, {preflight.Nerezolvabile + preflight.PeSumator} probleme)
    ║   tipuri Recorder {anImport}     {preflight.Tipuri,10} ({preflight.TipuriNecunoscute} necunoscute, {preflight.TipuriNeimplementate} fără handler → {preflight.DocumenteNeacoperite} documente neacoperite)
    ║   antete de citit {anImport}     {preflight.AntetePostate,10} Posted din {preflight.Antete} ({preflight.PostateFaraNote} fără rânduri contabile)
    ║   linii fără cotă TVA      {preflight.LiniiFaraCota,10} în lunile 1..{panaLa} (view-uri stale ⇒ regenerare)
    ║ NOMENCLATOARE IMPORTATE (procesate / noi în rularea asta)
    ║   gestiuni                 {impDepozite.Procesate,10} / {impDepozite.Noi}
    ║   conturi proprii          {impCasierii.Procesate + impConturi.Procesate,10} / {impCasierii.Noi + impConturi.Noi}  (casierii + bancare)
    ║   angajați                 {impPersoane.Procesate,10} / {impPersoane.Noi}
    ║   produse (la cerere)      {rezStoc.Produse,10} / {rezStoc.ProduseNoi}
    ║ DESCHIDEREA SCRISĂ (DocumentId = null)
    ║   rânduri contabile        {rezContabil.Randuri,10} contra ancorei {Deschidere.Ancora}
    ║   extrabilanțiere sărite   {rezContabil.Extrabilantiere,10} (Σ {rezContabil.SumaExtrabilantiera:N2} lei — clasa 8, alt modul)
    ║   loturi                   {rezStoc.Loturi,10} ({rezStoc.LoturiNoi} noi; data FIFO parsată pe {rezStoc.Loturi - rezStoc.DateNeparsate})
    ║   rânduri de stoc          {rezStoc.RanduriStoc,10} (Σ {rezStoc.ValoareScrisa:N2} lei / {rezStoc.CantitateScrisa:N3} buc)
    ║ DIFERENȚELE SURSEI (raportate, nu ascunse — decizia 34f)
    ║   grupe total-negative     {rezStoc.GrupeSarite,10} (Σ {rezStoc.ValoareSarita:N2} lei / {rezStoc.CantitateSarita:N3} buc)
    ║   poziții orfane           {stocOrfan.Count,10} (Σ {stocOrfan.Sum(s => s.Valoare):N2} lei)
    ║   Δ sold contabil − stoc   {stocOrfan.Sum(s => s.Valoare) + rezStoc.ValoareSarita,10:N2} lei
    ║ CONTRACTUL DE RECONCILIERE (design §8, restrâns la deschidere)
    ║   1. sold per cont OMFP    {rezRec.SimboluriContabile,10} simboluri comparate, {rezRec.DiferenteContabile} diferențe
    ║   2. ancora {Deschidere.Ancora,-14}  {rezRec.AncoraDb,10:N2} în bază = {rezRec.AncoraSursa:N2} în sursă
    ║   3. stoc produs×gestiune  {rezRec.CheiStoc,10} chei comparate, {rezRec.Nejustificate} nejustificate
    ║      justificate           {rezRec.JustificateGasite,10} chei (Σ {rezRec.JustificatV:N2} lei / {rezRec.JustificatQ:N3} buc)
    ║ DOCUMENTELE {anImport} (lunile 1..{panaLa}{(lunaPicata > 0 && !continua ? $", oprit la {lunaPicata:00}" : "")})
    ║   importate / sărite      {luni.Sum(l => l.Documente),10} / {luni.Sum(l => l.Sarite)} ({luni.Sum(l => l.Copii)} copii autogenerați operați)
    ║   eșecuri de operare      {luni.Sum(l => l.Esecuri),10} pe {luni.Count(l => l.Esecuri > 0)} luni
    ║   realocări de lot (48a)  {luni.Sum(l => l.Realocari),10} (Σ {luni.Sum(l => l.CantitateRealocata):N3} buc mutate de pe pin pe FIFO)
    ║ CONTRACTUL LUNAR (design §8 per lună — pasul 6)
    ║   contracte picate        {luni.Sum(l => l.ContractePicate),10} pe {luni.Count(l => l.ContractePicate > 0)} luni (3 contracte × {luni.Count} luni)
    ║   TVA de plată (4423)     {luni.Sum(l => l.TvaDePlata),10:N2} lei pe închiderile Atlas ale lunilor rulate ({bucla.ItvSarite} luni fără ITV nou — deja închise sau fără sold)
    ║   plafonul netării        {bucla.StareContract.PlafonStoc,10:N2} lei (valoarea de stoc justificată, cumulată)
    ║   durata contractelor     {TimeSpan.FromTicks(luni.Sum(l => l.DurataContract.Ticks)),10:hh\:mm\:ss}
    ║ REZULTAT: {(esecuri == 0 ? "CONTRACT ÎNDEPLINIT" : $"{esecuri} VERIFICĂRI PICATE")}, {avertismente.Count} avertismente, deschidere {durataDeschidere:hh\:mm\:ss} / documente {durataDocumente:hh\:mm\:ss} / total {cronometru.Elapsed:hh\:mm\:ss}
    ╚═══════════════════════════════════════════════════════════════════════════════
    """);

// ==================== Rezumat ====================

foreach (var a in avertismente)
    Console.WriteLine($"AVERT {a}");
Console.WriteLine(esecuri == 0
    ? "\nImport 1C (deschidere + reconciliere) încheiat fără eșecuri."
    : $"\nImport 1C (deschidere + reconciliere) încheiat cu {esecuri} eșecuri.");
return esecuri == 0 ? 0 : 1;
