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
// PASUL 1 (felia 1C-b): schelet + citirea sursei + baza pregătită. Faza de
// conținut e deocamdată doar SMOKE — nomenclatoarele, deschiderea și
// reconcilierea vin în pașii 2-4.

var flaxCs = args.Length > 0
    ? args[0]
    : "Server=(local);Database=EServicesFlx;Integrated Security=True;TrustServerCertificate=True";
var pgCs = args.Length > 1
    ? args[1]
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
Dictionary<string, string> overrideCont = new();

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

// ============================ Faza SMOKE ============================
// Citește sursa integral la data deschiderii și verifică invarianții pe care se
// sprijină pașii 2-4: planul Atlas are ancorele, maparea de cont funcționează
// pe formele reale, balanța 1C e echilibrată, iar stocul per lot reconciliază
// cu soldul contabil (dovada că BalantaNivel3 e o defalcare fidelă a Balanței).

using var flax = new FlaxDb(flaxCs);

var depozite = flax.Depozite();
var casierii = flax.Casierii();
var conturiBancare = flax.ConturiBancareProprii();
var persoane = flax.PersoaneFizice();
var plan1C = flax.PlanConturi();
var solduri = flax.SolduriDeschidere(dataDeschidere);
var solduriPartener = flax.SolduriPartener(dataDeschidere);
var stoc = flax.StocDeschidere(dataDeschidere);
var stocOrfan = flax.StocFaraIdentitate(dataDeschidere);

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
            + "fără produs/depozit — nu pot deveni loturi; de tranșat la pasul 3.");
    if (importabil == 0 && orfan == 0 && sold != 0)
        Avert($"Cont de stoc {cont} are sold {sold:N2} dar NICIO poziție în BalantaNivel3 "
            + "— deschiderea lui nu are detaliu de lot (de tranșat la pasul 3).");
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
            + "— de tranșat la pasul 3 (override sau cont nou în profil).");

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
    Console.WriteLine($"Solduri care cad pe cont SUMATOR (cer override la pasul 3): {peSumator.Count}");
    foreach (var x in peSumator.OrderByDescending(x => Math.Abs(x.SoldIni)))
        Avert($"Cont 1C {x.Cont} (sold {x.SoldIni:N2}) → {x.Simbol}, care e SUMATOR în OMFP "
            + "— cere override pe un cont de gradul II (decizie de profil, pasul 3).");

    // Legăturile de idempotență: goale la prima rulare, populate de pașii 2-3.
    Console.WriteLine($"Legături 1C existente: "
        + $"depozite {Legaturi.Incarca(os, "Depozite").Count}, "
        + $"parteneri {Legaturi.Incarca(os, "Partenerii").Count}, "
        + $"nomenclator {Legaturi.Incarca(os, "Nomenclator").Count}.");
}

// ==================== Rezumat ====================

foreach (var a in avertismente)
    Console.WriteLine($"AVERT {a}");
Console.WriteLine(esecuri == 0
    ? "\nSmoke 1C încheiat fără eșecuri."
    : $"\nSmoke 1C încheiat cu {esecuri} eșecuri.");
return esecuri == 0 ? 0 : 1;
