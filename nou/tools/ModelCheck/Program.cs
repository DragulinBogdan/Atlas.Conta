using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using Atlas.Conta.BackOffice.ModelCheck;
using Atlas.Conta.BackOffice.Module.Anaf;
using Atlas.Conta.BackOffice.Module.Api;
using Atlas.Conta.BackOffice.Module.Api.Asm;
using Atlas.Conta.BackOffice.Module.Api.Bcs;
using Atlas.Conta.BackOffice.Module.Api.Btr;
using Atlas.Conta.BackOffice.Module.Api.Dec;
using Atlas.Conta.BackOffice.Module.Api.Dsc;
using Atlas.Conta.BackOffice.Module.Api.Dvi;
using Atlas.Conta.BackOffice.Module.Api.Fcl;
using Atlas.Conta.BackOffice.Module.Api.Amo;
using Atlas.Conta.BackOffice.Module.Api.Cas;
using Atlas.Conta.BackOffice.Module.Api.Fct;
using Atlas.Conta.BackOffice.Module.Api.Imo;
using Atlas.Conta.BackOffice.Module.Api.Itv;
using Atlas.Conta.BackOffice.Module.Api.Ldi;
using Atlas.Conta.BackOffice.Module.Api.Nir;
using Atlas.Conta.BackOffice.Module.Api.Ntc;
using Atlas.Conta.BackOffice.Module.Api.Perioade;
using Atlas.Conta.BackOffice.Module.Api.Pif;
using Atlas.Conta.BackOffice.Module.Api.Politici;
using Atlas.Conta.BackOffice.Module.Api.Rdc;
using Atlas.Conta.BackOffice.Module.Api.Rlf;
using Atlas.Conta.BackOffice.Module.Api.Trz;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.DatabaseUpdate;
using Atlas.Conta.BackOffice.Module.Motor;
using Atlas.Conta.BackOffice.Module.Proiectii;
using Atlas.Conta.BackOffice.Module.Saft;
using DevExpress.ExpressApp;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.EFCore;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF.PermissionPolicy;
using DevExtreme.AspNet.Data;
using Microsoft.EntityFrameworkCore;
using SecurityPermissionPolicy = DevExpress.Persistent.Base.SecurityPermissionPolicy;
using SecurityPermissionState = DevExpress.Persistent.Base.SecurityPermissionState;
using N = Atlas.Conta.Nucleu;

// Șablonul de conexiune al uneltei: o singură definiție pentru toate comenzile.
static string Conexiunea(string baza) => new Npgsql.NpgsqlConnectionStringBuilder(
    "Host=127.0.0.1;Port=5446;Username=postgres;Password=postgres;Database=" + baza
    + (Environment.GetEnvironmentVariable("MODELCHECK_CONEXIUNE_EXTRA") is { Length: > 0 } extra ? ";" + extra : ""))
    .ConnectionString;

// Validare model EF + (dacă baza există) verificare migrații/seed + scenariile
// end-to-end ale motorului de operare pe un IObjectSpace real — aceeași
// infrastructură XAF pe care o folosește și UI-ul (docs 113709).
//
// Parametrizat pe PROFIL (P1, design §7): implicit rulează suita bugetară pe
// baza aplicației (aceeași țintă ca appsettings.json); `ModelCheck privat`
// rulează blocul e2e privat pe o bază DEDICATĂ (profil-per-bază — 35d), pe
// care unealta o migrează și o seed-uiește singură (ContaSeeder, Privat).
// D10 — emitorul de metadata pentru clientul React. Rulează pe REFLECȚIE pură
// (fără bază, fără XafApplication) și iese imediat: `ModelCheck --dump-metadata
// [cale]`. Fără argument scrie la calea implicită documentată în MetadataDump.
{
    var indexDump = Array.FindIndex(args, a => a.Equals("--dump-metadata", StringComparison.OrdinalIgnoreCase));
    if (indexDump >= 0) {
        var caleDump = args.Length > indexDump + 1 && !args[indexDump + 1].StartsWith('-')
            ? Path.GetFullPath(args[indexDump + 1])
            : MetadataDump.CaleImplicita();
        MetadataDump.Scrie(caleDump);
        Console.WriteLine($"Metadata scrisă: {caleDump}");
        return;
    }
}
{
    var indexTph = Array.FindIndex(args, a => a.Equals("--dump-integritate-tph", StringComparison.OrdinalIgnoreCase));
    if (indexTph >= 0) {
        if (args.Length <= indexTph + 1 || args[indexTph + 1].StartsWith('-')) {
            Console.WriteLine("Folosire: ModelCheck --dump-integritate-tph <cale.sql>");
            Environment.ExitCode = 2;
            return;
        }
        var caleTph = Path.GetFullPath(args[indexTph + 1]);
        using var ctxTph = new BackOfficeEFCoreDbContext(new DbContextOptionsBuilder<BackOfficeEFCoreDbContext>()
            .UseNpgsql("Host=127.0.0.1").UseChangeTrackingProxies().Options);
        var probeTph = IntegritateTph.Probe(ctxTph);
        File.WriteAllText(caleTph, IntegritateTph.Script(probeTph), new UTF8Encoding(false));
        Console.WriteLine($"Integritate TPH scrisă: {caleTph} ({probeTph.Count} interogări)");
        return;
    }
}

// 091-r1 — `ModelCheck --scenarii <TIP>[,<TIP>…] [privat]`: doar scenele tipului, pe baza profilului.
var filtruScenarii = Scenarii.Filtru(args, out var eroareScenarii);
if (eroareScenarii != null) {
    Console.WriteLine(eroareScenarii);
    Environment.ExitCode = 2;
    return;
}

var profil = args.Any(a => a.Contains("privat", StringComparison.OrdinalIgnoreCase))
    ? ProfilContabil.Privat : ProfilContabil.Bugetar;
var sufixBaza = Environment.GetEnvironmentVariable("MODELCHECK_BAZA_SUFIX") ?? "";
var connectionString = Conexiunea(
    (profil == ProfilContabil.Privat ? "Atlas.Conta.ModelCheck.Privat" : "Atlas.Conta.BackOffice") + sufixBaza);

var s = new Suita(args, profil, connectionString, filtruScenarii);

// X-D2 — `ModelCheck --probe-sursa [--lista]`: numai probele pe sursă, fără bază.
if (args.Contains("--probe-sursa")) {
    ProbeCulegere.VerificaSursa(s.Check);
    ProbeCititoriCub.VerificaSursa(s.Check);
    ProbeCititoriRegistre.VerificaSursa(s.Check, args.Contains("--lista"));
    ProbeTransferCititori.VerificaSursa(s.Check);
    ProbeBlocajScriere.VerificaSursa(s.Check);
    ProbeRegim.VerificaSursa(s.Check);
    s.Rezumat();
    return;
}

// SAF-B8 D3: procesul-copil al unui punct perf (rece = proces nou, pool gol) și DUK-ul separat pe XML-urile lui.
if (args.Contains("--perf-saft-masura")) {
    var i = Array.IndexOf(args, "--perf-saft-masura");
    var punct = new PerfSaft.Punct(int.Parse(args[i + 1]), int.Parse(args[i + 2]), int.Parse(args[i + 3]), int.Parse(args[i + 4]),
        args[i + 5], args[i + 6] == "1");
    using var providerPerf = new EFCoreObjectSpaceProvider<BackOfficeEFCoreDbContext>(
        (builder, _) => builder.UseNpgsql(connectionString).UseChangeTrackingProxies().UseObjectSpaceLinkProxies().UseLazyLoadingProxies()
            .ConfigureLoggingCacheTime(TimeSpan.Zero));
    var masuri = PerfSaft.MasoaraInProces(() => providerPerf.CreateObjectSpace(), punct, args[i + 7], s.Check);
    Console.WriteLine(PerfSaft.Json(masuri));
    s.Rezumat();
    return;
}
// X-D5: procesul-copil al unui punct (m, k, operație) al scării transversale.
if (args.Contains("--perf-cub-masura")) {
    var i = Array.IndexOf(args, "--perf-cub-masura");
    Console.WriteLine(PerfCub.Json(PerfCub.MasoaraInProces(connectionString, PerfCub.DinArgument(args[i + 1]), args[i + 2])));
    return;
}
// D9-A1: procesul-copil al primei reconstrucții de după multiplicare.
if (args.Contains("--scara-volum-reconstructie")) {
    Console.WriteLine(ScaraVolum.ReconstruiesteInProces(connectionString));
    return;
}
if (args.Contains("--perf-saft-duk")) {
    PerfSaft.ValideazaDuk(args[Array.IndexOf(args, "--perf-saft-duk") + 1], s.Check);
    s.Rezumat();
    return;
}

// D10 — disciplina migrațiilor aplicată codegen-ului (43d): canonic e artefactul
// COMIS, unealta doar verifică. Dacă `metadata.json` există și nu mai corespunde
// modelului (caption adăugat, enum extins, DefaultProperty mutat), rularea
// normală PICĂ — clientul nu are voie să se compileze pe captions fantomă.
if (filtruScenarii == null) {
    var caleMetadata = MetadataDump.CaleImplicita();
    var (exista, identic) = MetadataDump.VerificaDrift(caleMetadata);
    if (!exista)
        Console.WriteLine($"Metadata client: absentă ({caleMetadata}) — sări verificarea de drift.");
    else
        s.Check("Metadata clientului e la zi (altfel: rulați ModelCheck --dump-metadata)", identic);
}

// Harness-ul șterge ca aplicația: fizic, fără ștergere amânată (104f). Ce NU
// s-a adus din `AddEFCore`: `UseXafCalculatedProperties`,
// `UseXafServiceProviderContainer`, `UseMultipleActiveResultSetEmulation`,
// `EFCoreOptimisticLockInterceptor` — pe restul, un provider standalone.
ProbeRamuraDeNul.Porneste();
var optsBuilder = new DbContextOptionsBuilder<BackOfficeEFCoreDbContext>()
    .UseNpgsql(connectionString)
    .UseChangeTrackingProxies();
var opts = optsBuilder.Options;
s.Opts = opts;

using (var ctx = new BackOfficeEFCoreDbContext(opts)) {
    Console.WriteLine($"Model OK: {ctx.Model.GetEntityTypes().Count()} entity types; profil: {profil}");
    ProbeStraturi.Verifica(ctx, s.Check);
    ProbeStraturi.VerificaCoaja(s.Check);
    ProbeCulegere.VerificaSursa(s.Check);
    ProbeCititoriCub.VerificaSursa(s.Check);
    ProbeCititoriRegistre.VerificaSursa(s.Check);
    ProbeTransferCititori.VerificaSursa(s.Check);
    ProbeBlocajScriere.VerificaSursa(s.Check);
    ProbeRegim.VerificaActiuni(s.Check);
    ProbeRegim.VerificaSursa(s.Check);
    ProbeGardAnaliza.Ruleaza(s.Check);

    if (profil == ProfilContabil.Privat) {
        // Baza privată aparține uneltei: se creează/migrează aici.
        await ctx.Database.MigrateAsync();
    }
    else {
        if (!await ctx.Database.CanConnectAsync()) {
            Console.WriteLine("Baza nu există încă — doar validare de model.");
            Environment.ExitCode = 2;
            return;
        }
        var pending = (await ctx.Database.GetPendingMigrationsAsync()).ToList();
        var applied = (await ctx.Database.GetAppliedMigrationsAsync()).ToList();
        Console.WriteLine($"Migrații aplicate: {applied.Count}; în așteptare: {pending.Count}"
            + (pending.Count > 0 ? $" ({string.Join(", ", pending)})" : ""));
        if (pending.Count > 0) {
            Console.WriteLine("Aplicați migrațiile înainte de scenariul e2e (dotnet ef database update).");
            Environment.ExitCode = 2;
            return;
        }
    }
}

using var provider = new EFCoreObjectSpaceProvider<BackOfficeEFCoreDbContext>(
    (builder, _) => builder
        .UseNpgsql(connectionString)
        .UseChangeTrackingProxies()
        .UseObjectSpaceLinkProxies()
        .UseLazyLoadingProxies()
        // D9-A8: contorul rândului de politică crește la salvare ca pe hosturi (`AddSecuredEFCore`).
        .AddInterceptors(NumaratorSql.Instanta, new DevExpress.ExpressApp.EFCore.DataLocking.EFCoreOptimisticLockInterceptor()));
s.Provider = provider;

using (var osPlan = provider.CreateObjectSpace())
    PerfCub.ProbaPlanRespins(osPlan, profil == ProfilContabil.Privat, s.Check);

// 091: scenele folosesc politicile curente ale profilului pe ambele baze.
{
    using var osSeed = provider.CreateObjectSpace();
    ContaSeeder.Seed(osSeed, profil);
}

// Convenția de rotunjire a banilor = dată a bazei (decizia 51c): pe calea privată
// seed-ul tocmai a fixat-o, pe cea bugetară (bază deja seed-uită) se citește aici.
using (var osConventie = provider.CreateObjectSpace()) {
    var citita = ContaSeeder.AplicaConventiaRotunjire(osConventie);
    Console.WriteLine($"Convenție rotunjire bani: {Scara.ConventieBani}"
        + (citita ? " (din SetareProfil)" : " (implicit — baza nu are rând SetareProfil)"));
}

if (filtruScenarii == null)
    await E2eSondeBaza.Ruleaza(s);

if (args.Contains("--perf-saft")) {
    var directorPerf = Environment.GetEnvironmentVariable("PERF_SAFT_DIR")
        ?? Path.Combine(Duk.DirectorTemporar(), $"perf-saft-{DateTime.UtcNow:yyyyMMdd-HHmmss}");
    Directory.CreateDirectory(directorPerf);
    List<PerfSaft.Masura> MasoaraProces(PerfSaft.Punct p) {
        var psi = new ProcessStartInfo(Environment.ProcessPath!) { RedirectStandardOutput = true, UseShellExecute = false };
        foreach (var a in new[] { typeof(PerfSaft).Assembly.Location, "--perf-saft-masura", $"{p.Istoric}", $"{p.An}", $"{p.Luna}",
                     $"{p.Unitati}", p.Modul, p.Planuri ? "1" : "0", directorPerf, "privat" })
            psi.ArgumentList.Add(a);
        using var copil = Process.Start(psi)!;
        List<PerfSaft.Masura> masuri = null;
        for (string linie; (linie = copil.StandardOutput.ReadLine()) != null;) {
            if (linie.StartsWith(PerfSaft.PrefixJson, StringComparison.Ordinal)) masuri = PerfSaft.DinJson(linie);
            else if (linie.StartsWith("     ") || linie.StartsWith("FAIL")) Console.WriteLine(linie);
        }
        copil.WaitForExit();
        s.Check($"SAF-B8-D3: procesul de măsurare {p.Modul} m{p.Istoric} k{p.Unitati} se încheie fără eșec", copil.ExitCode == 0 && masuri != null);
        return masuri ?? [];
    }
    var masuriPerf = new List<PerfSaft.Masura>();
    foreach (var (anPerf, istoricPerf, unitatiPerf) in new[] { (2050, 0, 0), (2052, 6, 16), (2054, 12, 16) }) {
        var scenaPerf = new PerfSaft(() => provider.CreateObjectSpace(), s.Check, (os, an, luna) => s.InchideAcceptTot(os, an, luna),
            anPerf, istoricPerf, unitatiPerf, [1, 4, 16, 64], MasoaraProces);
        var ceasPerf = Stopwatch.StartNew();
        scenaPerf.Ruleaza();
        masuriPerf.AddRange(scenaPerf.Masuri);
        Console.WriteLine($"     PERF m{istoricPerf}: {ceasPerf.Elapsed.TotalSeconds:0} s");
    }
    PerfSaft.Evalueaza(masuriPerf, s.Check, directorPerf);
    s.Rezumat();
    return;
}

// X-D5 / X-D3: `ModelCheck --perf-cub [privat]` — scara transversală a cititorilor comuni și reconcilierea pe baza de volum.
if (args.Contains("--perf-cub")) {
    var privatPerf = profil == ProfilContabil.Privat;
    var directorPerf = Environment.GetEnvironmentVariable("PERF_CUB_DIR")
        ?? Path.Combine(Duk.DirectorTemporar(), $"perf-cub-{DateTime.UtcNow:yyyyMMdd-HHmmss}");
    Directory.CreateDirectory(directorPerf);
    int[] Lista(string nume, string lipsa) => (Environment.GetEnvironmentVariable(nume) ?? lipsa)
        .Split(',', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToArray();
    var istoricePerf = Lista("PERF_CUB_M", "0,6,12");
    var treptePerf = Lista("PERF_CUB_K", "1,4,16,64");
    var unitatiPerf = Lista("PERF_CUB_UNITATI", "16")[0];
    List<PerfCub.Masura> MasoaraProces(PerfCub.Punct p) {
        var psi = new ProcessStartInfo(Environment.ProcessPath!) { RedirectStandardOutput = true, UseShellExecute = false };
        foreach (var a in new[] { typeof(PerfCub).Assembly.Location, "--perf-cub-masura", PerfCub.Argument(p), directorPerf }
                     .Concat(privatPerf ? ["privat"] : Array.Empty<string>()))
            psi.ArgumentList.Add(a);
        using var copil = Process.Start(psi)!;
        List<PerfCub.Masura> masuri = null;
        for (string linie; (linie = copil.StandardOutput.ReadLine()) != null;) {
            if (linie.StartsWith(PerfCub.PrefixJson, StringComparison.Ordinal)) masuri = PerfCub.DinJson(linie);
            else if (linie.StartsWith("     ") || linie.StartsWith("FAIL")) Console.WriteLine(linie);
        }
        copil.WaitForExit();
        s.Check($"X-D5: procesul de măsurare {p.Operatie} m{p.Istoric} k{p.Unitati} se încheie fără eșec", copil.ExitCode == 0 && masuri != null);
        return masuri ?? [];
    }
    var scenePerf = new List<PerfCub>();
    foreach (var istoricPerf in istoricePerf) {
        var scenaPerf = new PerfCub(() => provider.CreateObjectSpace(), s.Check, privatPerf, (os, an, luna) => s.InchideAcceptTot(os, an, luna),
            2060 + istoricPerf / 3, istoricPerf, unitatiPerf, treptePerf, MasoaraProces, directorPerf, opts);
        var ceasPerf = Stopwatch.StartNew();
        scenaPerf.Ruleaza();
        scenePerf.Add(scenaPerf);
        var neucise = AcoperireInvarianti.Neucise.ToList();
        File.AppendAllText(Path.Combine(directorPerf, $"xd3-{(privatPerf ? "privat" : "bugetar")}-m{istoricPerf}.md"),
            $"\n## Mutanții INV-CUB după m = {istoricPerf}\n\nNeuciși încă: {(neucise.Count == 0 ? "niciunul" : string.Join(", ", neucise))}.\n");
        Console.WriteLine($"     PERFCUB m{istoricPerf}: {ceasPerf.Elapsed.TotalSeconds:0} s; mutanți INV-CUB neuciși: {(neucise.Count == 0 ? "niciunul" : string.Join(", ", neucise))}");
    }
    PerfCub.Evalueaza(scenePerf, s.Check, directorPerf, privatPerf);
    s.Rezumat();
    return;
}

// D9-A1: `ModelCheck --scara-volum privat` — cititorii comuni pe scena PerfCub multiplicată; raport, fără prag.
if (args.Contains("--scara-volum")) {
    if (profil != ProfilContabil.Privat) {
        Console.WriteLine("Folosire: ModelCheck --scara-volum privat");
        Environment.ExitCode = 2;
        return;
    }
    ScaraVolum.Ruleaza(connectionString, () => provider.CreateObjectSpace(), s.Check, (os, an, luna) => s.InchideAcceptTot(os, an, luna), opts);
    s.Rezumat();
    return;
}

if (filtruScenarii != null) {
    if (RuleazaScenele.Ruleaza(s, profil == ProfilContabil.Privat) == 0) {
        Environment.ExitCode = 2;
        return;
    }
    VerificaInvariantiCub.Ruleaza(s);
    ProbeRamuraDeNul.Verifica(s);
    s.Rezumat();
    return;
}

if (profil == ProfilContabil.Privat) {
    E2eP1Privat.Ruleaza(s);
    E2eP2Descarcare.Ruleaza(s);
    E2eNotaContabilaPrivat.Ruleaza(s);
    E2eCompensare.Ruleaza(s);
    E2eInchidereTvaPrivat.Ruleaza(s);
    E2eApiItv.Ruleaza(s);
    E2eAsamblare.Ruleaza(s);
    E2eRetururi.Ruleaza(s);
    E2eApiFctPrivat.Ruleaza(s);
    E2eCulegere.Ruleaza(s);
    E2eApiDecPrivat.Ruleaza(s);
    E2eApiFcl.Ruleaza(s);

    // Felia 9 rulează pe AMBELE profiluri: proiecția e agnostică la plan (nu
    // cunoaște niciun simbol), dar tocmai de asta merită probată pe amândouă —
    // datele preexistente ale bazei diferă, iar invarianții globali (partidă
    // dublă, continuitate) se verifică peste ELE, nu doar peste scenariul propriu.
    VerificaBalanta.Ruleaza(s, true);
    VerificaFisaJurnal.Ruleaza(s, true);
    VerificaRegistruTva.Ruleaza(s, cuTva: true);
    VerificaD300Seed.Ruleaza(s, privat: true);
    VerificaD394Seed.Ruleaza(s, privat: true);
    VerificaAdresaPartener.Ruleaza(s, privat: true);
    VerificaSincronizareAnaf.Ruleaza(s);
    VerificaSaftModel.Ruleaza(s, privat: true);
    VerificaMiscariSaft.Ruleaza(s, privat: true);
    VerificaD300.Ruleaza(s, cuTva: true);
    VerificaD394.Ruleaza(s, cuTva: true);
    VerificaAxaTaxareInversa.Ruleaza(s);
    VerificaGardianCicluCont.Ruleaza(s);
    VerificaValoareIesire.Ruleaza(s, privat: true);
    VerificaApiNtc.Ruleaza(s, privat: true);
    // F19-D14: ASM are politici DOAR pe privat (numerotare + reguli de stoc) —
    // pe bugetar tipul e inert, deci blocul ar măsura cifre moarte.
    VerificaApiAsm.Ruleaza(s);
    // F19-D14: RLF/RDC au politici (numerotare, stoc, contare, TVA) DOAR pe
    // privat — pe bugetar tipurile sunt inerte, iar blocurile ar măsura cifre
    // moarte (retururile sunt singurele tipuri ale feliei cu TVA).
    VerificaApiRlf.Ruleaza(s);
    VerificaApiRdc.Ruleaza(s);
    // Felia 23 — implicitele de culegere și întreținerea politicilor (F23-V1…V6).
    VerificaF23Model.Ruleaza(s, privat: true);
    VerificaF23Rezolvare.Ruleaza(s, privat: true);
    VerificaF23Seed.Ruleaza(s, privat: true);
    VerificaF24Seed.Ruleaza(s, privat: true);
    VerificaF23Gardian.Ruleaza(s, privat: true);
    VerificaF23Raport.Ruleaza(s, privat: true);
    VerificaF23ClasaFiscala.Ruleaza(s);
    // Felia 24 — lista unică a tipurilor configurabile și rolul `Configurator`.
    VerificaF24Rol.Ruleaza(s, privat: true);
    // Felia 24, review advers — gardianul pe enum-uri și pe ștergerea unui TipTva.
    VerificaF24Gardian.Ruleaza(s, privat: true);
    // Felia 24 track B — potrivirea ca funcții pure (F24-P1…P7).
    VerificaPotrivire.Ruleaza(s);
    VerificaExplicaStocGolit.Ruleaza(s, privat: true);
    // Felia 24 track B — explicația configurației (F24-E1…E7), doar pe privat.
    VerificaF24Explica.Ruleaza(s);
    // Felia 25 — declarația vamală de import, pe scenă (DVI-V1…V17, DVI-r7).
    VerificaDvi.Ruleaza(s, privat: true);
    // Felia 25, pasul 2 — ușa `api/dvi` prin `DviApply` (E2E-API-DVI).
    VerificaApiDvi.Ruleaza(s);
    // Felia 27, pasul 1 — perioada ca lanț și comanda de închidere (PER-V0…V10).
    VerificaPerioade.Ruleaza(s, privat: true);
    // Felia 27, pasul 2a — soldurile materializate la închidere (SOL-V*) și cursa (PER-C*).
    VerificaSolduriPerioada.Ruleaza(s, privat: true);
    // Felia 27, pasul 3 — data înregistrării (DIR-V0…V13).
    VerificaDataInregistrare.Ruleaza(s, privat: true);
    // Felia 27, pasul 4a — perioada de declarare pe registrul fiscal (PDT-V0…V14).
    VerificaPerioadaDeclarare.Ruleaza(s, privat: true);
    // Felia 27, pasul 5 — corecția = storno legat + document nou, cu motiv (COR-V0…V19).
    VerificaCorectie.Ruleaza(s, privat: true);
    // Felia 27, pasul 6 — totalul scris, partidele deschise, imperecherea datată (PAR-V0…V24).
    VerificaPartide.Ruleaza(s, privat: true);
    // Felia 27, pasul 7 — constatările de închidere și acceptarea (ACC-V0…V18).
    VerificaAcceptare.Ruleaza(s, privat: true);
    VerificaReviewAcceptare.Ruleaza(s, privat: true);
    // Felia 26, pasul 1 — imobilizările pe scenă (IMO-V0…V28).
    VerificaImobilizari.Ruleaza(s, privat: true);
    // Felia 26, pasul 3 — ușile `api/pif|cas|amo|imobilizari` prin `*Apply` (E2E-API-IMO).
    VerificaImobilizariApi.Ruleaza(s, privat: true);
    // Probele review-ului advers al feliei 26 (`IMO-R*`, 87).
    VerificaReviewF26.Ruleaza(s, privat: true);
    // Felia 26, pasul 2 — formula contra cifrelor postate în Flax (RECONCILIERE-MF).
    VerificaReconciliereMf.Ruleaza(s);
    // Decizia 85 — modul de acces al ListView-urilor (D85-M1/M2/R1/R2/R3).
    VerificaD85.Ruleaza(s, privat: true);
    // Review advers felia 27, pasul 8b (F27-R*).
    VerificaReviewF27.Ruleaza(s, privat: true);
    // Felia 28 — TPH cu discriminatorul `ClrType` (F28-A…G).
    VerificaF28.Ruleaza(s, privat: true);
    // Felia 31 (TR-D7a), pasul 1 — schema cubului (STR-SCHEMA-*) și `Pozitie` (STR-POZITIE).
    VerificaSchemaCub.Ruleaza(s, privat: true);
    VerificaPozitieLinii.Ruleaza(s, privat: true);
    // Scenele catalogului pe tip (091); aceleași pe care le selectează `--scenarii`.
    RuleazaScenele.Ruleaza(s, privat: true);
    // Felia 32, pasul 2b — laturile ca structură (STR-LATURI-*), după toate scenele.
    VerificaLaturi.Ruleaza(s, privat: true);
    VerificaInvariantiCub.Ruleaza(s);
    ProbeRamuraDeNul.Verifica(s);

    s.Rezumat();
    return;
}

E2eNotaTransfer.Ruleaza(s);
E2eApiBtr.Ruleaza(s);
E2eFacturaIntrare.Ruleaza(s);
E2eApiFct.Ruleaza(s);
E2eBonConsum.Ruleaza(s);
E2eListaDiferente.Ruleaza(s);
E2eFacturaIesire.Ruleaza(s);
E2eTrezorerie.Ruleaza(s);
E2eApiTrz.Ruleaza(s);
E2eVirament.Ruleaza(s);
E2eDecont.Ruleaza(s);
E2eNotaContabila.Ruleaza(s);
E2eInchidereTvaBugetar.Ruleaza(s);
E2eRetururiBugetar.Ruleaza(s);
E2eApiNir.Ruleaza(s);
E2eApiBcs.Ruleaza(s);
E2eApiLdi.Ruleaza(s);
E2eApiDec.Ruleaza(s);
E2eApiPereche.Ruleaza(s);

VerificaBalanta.Ruleaza(s, false);
VerificaFisaJurnal.Ruleaza(s, false);
VerificaRegistruTva.Ruleaza(s, cuTva: false);
VerificaD300Seed.Ruleaza(s, privat: false);
VerificaD394Seed.Ruleaza(s, privat: false);
VerificaAdresaPartener.Ruleaza(s, privat: false);
VerificaSincronizareAnaf.Ruleaza(s);
VerificaSaftModel.Ruleaza(s, privat: false);
VerificaMiscariSaft.Ruleaza(s, privat: false);
VerificaD300.Ruleaza(s, cuTva: false);
VerificaD394.Ruleaza(s, cuTva: false);
VerificaAxaTaxareInversa.Ruleaza(s);
VerificaGardianCicluCont.Ruleaza(s);
VerificaValoareIesire.Ruleaza(s, privat: false);
VerificaApiNtc.Ruleaza(s, privat: false);
// Felia 23 — implicitele de culegere și întreținerea politicilor (F23-V1…V6).
VerificaF23Model.Ruleaza(s, privat: false);
VerificaF23Rezolvare.Ruleaza(s, privat: false);
VerificaF23Seed.Ruleaza(s, privat: false);
VerificaF24Seed.Ruleaza(s, privat: false);
VerificaF23Gardian.Ruleaza(s, privat: false);
VerificaF23Raport.Ruleaza(s, privat: false);
VerificaF23ClasaFiscala.Ruleaza(s);
// Felia 24 — lista unică a tipurilor configurabile și rolul `Configurator`.
VerificaF24Rol.Ruleaza(s, privat: false);
// Felia 24, review advers — gardianul pe enum-uri și pe ștergerea unui TipTva.
VerificaF24Gardian.Ruleaza(s, privat: false);
// Felia 25 — declarația vamală de import (DVI-V0 pe bugetar: ancoră inertă).
VerificaDvi.Ruleaza(s, privat: false);
// Felia 27, pasul 1 — perioada ca lanț și comanda de închidere (PER-V0…V10).
VerificaPerioade.Ruleaza(s, privat: false);
// Felia 27, pasul 2a — soldurile materializate la închidere (SOL-V*) și cursa (PER-C*).
VerificaSolduriPerioada.Ruleaza(s, privat: false);
// Felia 27, pasul 3 — data înregistrării (DIR-V0…V13).
VerificaDataInregistrare.Ruleaza(s, privat: false);
// Felia 27, pasul 4a — perioada de declarare (PDT-V0 pe bugetar: ancoră inertă).
VerificaPerioadaDeclarare.Ruleaza(s, privat: false);
// Felia 27, pasul 5 — corecția (COR-V0…V13 pe bugetar: fără partea fiscală).
VerificaCorectie.Ruleaza(s, privat: false);
// Felia 27, pasul 6 — totalul scris, partidele deschise, imperecherea datată (PAR-V0…V21).
VerificaPartide.Ruleaza(s, privat: false);
// Felia 27, pasul 7 — constatările de închidere și acceptarea (ACC-V0…V18).
VerificaAcceptare.Ruleaza(s, privat: false);
VerificaReviewAcceptare.Ruleaza(s, privat: false);
// Felia 26, pasul 1 — imobilizările pe scenă (IMO-V0…V28), și pe bugetar.
VerificaImobilizari.Ruleaza(s, privat: false);
// Felia 26, pasul 3 — ușile `api/pif|cas|amo|imobilizari` prin `*Apply` (E2E-API-IMO).
VerificaImobilizariApi.Ruleaza(s, privat: false);
// Probele review-ului advers al feliei 26 (`IMO-R*`, 87), și pe bugetar.
VerificaReviewF26.Ruleaza(s, privat: false);
// Decizia 85 — modul de acces al ListView-urilor (D85-M1/M2/R1/R2/R3).
VerificaD85.Ruleaza(s, privat: false);
// Felia 24 track B — potrivirea ca funcții pure (F24-P1…P7).
VerificaPotrivire.Ruleaza(s);
VerificaExplicaStocGolit.Ruleaza(s, privat: false);
// Review advers felia 27, pasul 8b (F27-R*).
VerificaReviewF27.Ruleaza(s, privat: false);
// Felia 28 — TPH cu discriminatorul `ClrType` (F28-A…G).
VerificaF28.Ruleaza(s, privat: false);
// Felia 31 (TR-D7a), pasul 1 — schema cubului (STR-SCHEMA-*) și `Pozitie` (STR-POZITIE).
VerificaSchemaCub.Ruleaza(s, privat: false);
VerificaPozitieLinii.Ruleaza(s, privat: false);
// Scenele catalogului pe tip (091); aceleași pe care le selectează `--scenarii`.
RuleazaScenele.Ruleaza(s, privat: false);
// Felia 32, pasul 2b — laturile ca structură (STR-LATURI-*), după toate scenele.
VerificaLaturi.Ruleaza(s, privat: false);
VerificaInvariantiCub.Ruleaza(s);
ProbeRamuraDeNul.Verifica(s);

s.Rezumat();
