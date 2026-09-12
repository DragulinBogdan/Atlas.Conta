using Atlas.Conta.BackOffice.Blazor.Server;
using Atlas.Conta.BackOffice.Module.DatabaseUpdate;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.AmbientContext;
using DevExpress.ExpressApp.AspNetCore.WebApi.Core;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Atlas.Conta.BackOffice.ModelCheck;

// Modelul REAL al aplicației XAF Blazor (module + `Model.xafml` al hostului),
// construit pe calea `--updateDatabase` a hostului (`AspNetCoreDBUpdater`):
// host-ul se construiește, aplicația se creează din DI și `Setup()` produce
// modelul, fără circuit Blazor și fără conexiune la bază. Rădăcina de conținut
// e directorul proiectului Blazor.Server (appsettings + `Model.xafml`).
static class ModelAplicatie {
    public static IModelApplication Incarca(string connectionString, ProfilContabil profil) {
        // `Startup` îl setează global; harness-ul rămâne pe implicitul Npgsql.
        const string comutator = "Npgsql.EnableLegacyTimestampBehavior";
        var setat = AppContext.TryGetSwitch(comutator, out var valoare);
        try {
            return IncarcaCore(connectionString, profil);
        }
        finally {
            AppContext.SetSwitch(comutator, setat && valoare);
        }
    }

    static IModelApplication IncarcaCore(string connectionString, ProfilContabil profil) {
        var radacina = Path.GetFullPath(Path.Combine(MetadataDump.DirectorProiect(),
            "../../Atlas.Conta.BackOffice/Atlas.Conta.BackOffice.Blazor.Server"));
        var host = Host.CreateDefaultBuilder()
            .UseEnvironment(profil == ProfilContabil.Privat ? "Privat" : "Production")
            .UseContentRoot(radacina)
            .ConfigureAppConfiguration(c => c.AddInMemoryCollection(new Dictionary<string, string> {
                ["ConnectionStrings:ConnectionString"] = connectionString,
            }))
            .ConfigureLogging(l => l.ClearProviders())
            .ConfigureWebHostDefaults(w => w.UseStartup<Startup>())
            .Build();
        var scope = host.Services.CreateScope();
        var typesInfo = scope.ServiceProvider.GetRequiredService<ITypesInfo>();
        var aplicatie = scope.ServiceProvider.GetRequiredService<IWebApiApplicationFactory>().CreateApplication(typesInfo);
        scope.ServiceProvider.GetRequiredService<IValueManagerStorageContext>().RunWithStorage(aplicatie.Setup);
        return aplicatie.Model;
    }
}
