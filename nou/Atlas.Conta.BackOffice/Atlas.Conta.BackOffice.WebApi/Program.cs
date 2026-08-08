using DevExpress.ExpressApp;
using DevExpress.ExpressApp.AspNetCore.DesignTime;
using DevExpress.ExpressApp.Design;

namespace Atlas.Conta.BackOffice.WebApi {
    public class Program : IDesignTimeApplicationFactory {
        static bool ContainsArgument(string[] args, string argument) {
            return args.Any(arg => arg.TrimStart('/').TrimStart('-').ToLower() == argument.ToLower());
        }
        public static int Main(string[] args) {
            if (ContainsArgument(args, "help") || ContainsArgument(args, "h")) {
                // Host-ul API nu updatează NICIODATĂ baza (decizia 42f): schema vine
                // din migrațiile EF Core, seed-ul din ModuleUpdater — ambele rulate
                // exclusiv prin `Atlas.Conta.BackOffice.Blazor.Server --updateDatabase`.
                // Ramura `--updateDatabase` a scaffold-ului a fost ȘTEARSĂ: acest host
                // nu citește `ProfilContabil` ca autoritate și ar seed-ui greșit.
                Console.WriteLine("Atlas.Conta.BackOffice Web API host.");
                Console.WriteLine();
                Console.WriteLine("Nu actualizează baza de date. Pentru migrații + seed folosiți:");
                Console.WriteLine("    Atlas.Conta.BackOffice.Blazor.Server.exe --updateDatabase [--forceUpdate --silent]");
            } else {
                DevExpress.ExpressApp.FrameworkSettings.DefaultSettingsCompatibilityMode = DevExpress.ExpressApp.FrameworkSettingsCompatibilityMode.Latest;
                DevExpress.ExpressApp.Security.SecurityStrategy.AutoAssociationReferencePropertyMode = DevExpress.ExpressApp.Security.ReferenceWithoutAssociationPermissionsMode.AllMembers;
                IHost host = CreateHostBuilder(args).Build();
                // Decizia 51c: convenția de rotunjire e dată a bazei, citită O DATĂ
                // înainte de a servi primul request (motorul rulează în cerere și
                // rotunjește la materializare). Același helper ca în Blazor.Server.
                FixeazaConventiaRotunjire(host);
                host.Run();
            }
            return 0;
        }
        static void FixeazaConventiaRotunjire(IHost host) {
            var logger = host.Services.GetRequiredService<ILoggerFactory>().CreateLogger<Program>();
            Atlas.Conta.BackOffice.Module.DatabaseUpdate.ScaraBootstrap.FixeazaConventia(
                host.Services.GetRequiredService<IConfiguration>().GetConnectionString("ConnectionString"),
                (mesaj, exceptie) => logger.LogWarning(exceptie, "{Mesaj}", mesaj),
                mesaj => logger.LogInformation("{Mesaj}", mesaj));
        }
        XafApplication IDesignTimeApplicationFactory.Create() {
            IHostBuilder hostBuilder = CreateHostBuilder(Array.Empty<string>());
            return DesignTimeApplicationFactoryHelper.Create(hostBuilder);
        }
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
