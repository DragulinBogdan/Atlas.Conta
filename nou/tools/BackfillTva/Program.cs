using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.DatabaseUpdate;
using BackfillTva;
using DevExpress.ExpressApp.EFCore;
using Microsoft.EntityFrameworkCore;

// Felia 11, JT-D8 — BACKFILL-ul registrului de TVA.
//
// DE CE unealtă proprie, și nu un flag pe una existentă:
//   • `ModelCheck` e VERIFICATOR pe bază mică — scenele lui construiesc și
//     curăță documente de probă; o rulare peste 200.000 de documente reale n-are
//     ce căuta acolo.
//   • `Import1C` e legat de sursa Flax (view-urile SkyConta, dicționarul de
//     conturi, contractul lunar de reconciliere); backfill-ul nu citește NIMIC
//     din afară — derivă din documentele deja operate ale bazei-țintă.
//   • `Migrare` e one-shot legacy→nou, cu propria sursă SQL Server.
// Backfill-ul e altceva, și NU e one-shot: modelul e „bază per client”
// (decizia 35d), deci ORICE bază creată înaintea feliei 11 cere pasul ăsta o
// dată — dev, clone de import, baze de producție. Rămâne folositor cât timp
// există baze mai vechi decât felia.
//
// Derivarea NU se duplică (invariantul V + nota din `RegistruTvaService`):
// unealta apelează exact funcția motorului, `RegistruTvaService.Deriva(os, doc)`,
// pe documente DEJA operate — nu le re-operează și nu scrie în registru „de
// mână”. Consecința e că o schimbare a regulilor fiscale se propagă automat și
// aici; o a doua implementare ar fi divergit tăcut.
//
// Uzaj:
//   BackfillTva [connectionString] [--dry-run] [--doar-verifica]
//     --dry-run       derivă și raportează TOT (inclusiv reconcilierea, care
//                     ține cont de rândurile care S-AR fi scris), fără să scrie
//                     nimic în bază.
//     --doar-verifica sare peste backfill și rulează doar reconcilierea JT-D6
//                     peste rândurile deja existente.
//
// Codul de ieșire: 0 = contract îndeplinit, 1 = divergențe/verificări picate,
// 2 = argumente sau schemă imposibile (contractul nu se raportează, se impune —
// tiparul Import1C).

var pozitionale = new List<string>();
var dryRun = false;
var doarVerifica = false;
foreach (var arg in args) {
    if (!arg.StartsWith("--")) {
        pozitionale.Add(arg);
        continue;
    }
    switch (arg) {
        case "--dry-run":
            dryRun = true;
            break;
        case "--doar-verifica":
            doarVerifica = true;
            break;
        default:
            Console.Error.WriteLine($"Argument necunoscut: {arg}. "
                + "Uzaj: BackfillTva [connectionString] [--dry-run] [--doar-verifica]");
            return 2;
    }
}

var pgCs = pozitionale.Count > 0
    ? pozitionale[0]
    : "Host=localhost;Port=5444;Username=postgres;Password=postgres;Database=Atlas.Conta.BackOffice";
var numeBaza = pgCs.Contains("Database=") ? pgCs.Split("Database=")[^1].Split(';')[0] : pgCs;

var esecuri = 0;
void Check(string nume, bool ok) {
    Console.WriteLine($"{(ok ? "OK  " : "FAIL")} {nume}");
    if (!ok)
        esecuri++;
}
var avertismente = new List<string>();
void Avert(string mesaj) => avertismente.Add(mesaj);

Console.WriteLine($"Backfill registru TVA (JT-D8) pe „{numeBaza}”"
    + (doarVerifica ? " — DOAR VERIFICARE" : dryRun ? " — DRY-RUN (nu se scrie nimic)" : "") + ".");

// Provider standalone, fără XAF Application — tiparul Import1C/Migrare.
// Unealta NU migrează baza: schema e treaba operatorului (`dotnet ef database
// update`), iar un backfill peste o schemă veche ar fi cea mai proastă formă de
// ajutor. Se verifică explicit, mai jos.
using var provider = new EFCoreObjectSpaceProvider<BackOfficeEFCoreDbContext>(
    (builder, _) => builder
        .UseNpgsql(pgCs)
        .UseChangeTrackingProxies()
        .UseObjectSpaceLinkProxies()
        .UseLazyLoadingProxies());

using (var os = provider.CreateObjectSpace()) {
    var db = ((EFCoreObjectSpace)os).DbContext;
    List<string> neaplicate;
    try {
        neaplicate = db.Database.GetPendingMigrations().ToList();
    }
    catch (Exception ex) {
        Console.Error.WriteLine($"Baza „{numeBaza}” nu se poate interoga: {ex.Message}");
        return 2;
    }
    if (neaplicate.Count > 0) {
        Console.Error.WriteLine($"Baza „{numeBaza}” are {neaplicate.Count} migrații neaplicate "
            + $"(prima: {neaplicate[0]}). Aplică-le întâi:\n"
            + "  dotnet ef database update --context BackOfficeEFCoreDbContext "
            + $"--connection \"{pgCs}\"");
        return 2;
    }
}

// Convenția de rotunjire a banilor e dată a bazei (decizia 51c/52a) și derivarea
// rotunjește (`Capitalizat` desface baza din brut) — se citește ÎNAINTE de
// primul rând scris, ca la Import1C/Migrare.
using (var os = provider.CreateObjectSpace()) {
    if (!ContaSeeder.AplicaConventiaRotunjire(os))
        Avert("Baza n-are rând `SetareProfil` — convenția de rotunjire rămâne implicită "
            + $"({Scara.ConventieBani}). Bază neseed-uită de un updater recent?");
}
Console.WriteLine($"Convenție rotunjire bani: {Scara.ConventieBani}.");

var cronometru = System.Diagnostics.Stopwatch.StartNew();

RezultatBackfill rez = null;
if (!doarVerifica)
    rez = Backfill.Executa(provider, dryRun, Avert, Check);
else
    Console.WriteLine("\n--- Backfill sărit (--doar-verifica) ---");

// Reconcilierea rulează ÎNTOTDEAUNA, inclusiv în dry-run: acolo primește
// rândurile care S-AR fi scris, altfel proba ar fi vacuă pe o bază care încă
// n-are niciun rând fiscal.
var rezRec = Reconciliere.Executa(provider, rez?.Derivate, Avert, Check);

Console.WriteLine($"""

    ╔══════════════════ SUMARUL RULĂRII (backfill TVA, {numeBaza}) ══════════════════
    ║ BACKFILL (JT-D8){(doarVerifica ? "  — sărit (--doar-verifica)" : dryRun ? "  — DRY-RUN, nimic scris" : "")}
    ║   candidați (Operat/Stornat)  {rez?.Candidati,10} documente de tipuri cu PoliticaTva
    ║   deja cu rânduri fiscale     {rez?.Sarite,10} sărite (idempotență)
    ║   documente atinse            {rez?.Atinse,10} ({rez?.CuRanduri} au produs rânduri)
    ║   rânduri fiscale             {rez?.Randuri,10} ({rez?.RanduriStorno} inverse de storno)
    ║   stornate reconstituite      {rez?.StornateReconstituite,10} (fără urmă de storno: {rez?.StornateFaraUrma}, ambigue: {rez?.StornateAmbigue})
    ║   durata backfill             {rez?.Durata,10:hh\:mm\:ss}
    ║ RECONCILIEREA (JT-D6, per document, peste TOATĂ baza)
    ║   documente cu rânduri fiscale{rezRec.Documente,10} ({rezRec.Randuri} rânduri fiscale)
    ║   divergențe                  {rezRec.Divergente,10}
    ║ MĂSURĂTORI (JT-D2 / riscul 4 din design — se numără, nu se umplu)
    ║   linii fără TipTva           {rezRec.LiniiFaraTipTva,10} din {rezRec.LiniiCuPolitica} linii de documente operate cu politică
    ║   rânduri fără partener       {rezRec.FaraPartener,10} din {rezRec.Randuri}
    ║ REZULTAT: {(esecuri == 0 ? "CONTRACT ÎNDEPLINIT" : $"{esecuri} VERIFICĂRI PICATE")}, {avertismente.Count} avertismente, total {cronometru.Elapsed:hh\:mm\:ss}
    ╚═══════════════════════════════════════════════════════════════════════════════
    """);

foreach (var a in avertismente)
    Console.WriteLine($"AVERT {a}");
Console.WriteLine(esecuri == 0
    ? "\nBackfill TVA încheiat fără eșecuri."
    : $"\nBackfill TVA încheiat cu {esecuri} eșecuri.");
return esecuri == 0 ? 0 : 1;
