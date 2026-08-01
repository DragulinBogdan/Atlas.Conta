using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Blazor.DesignTime;
using DevExpress.ExpressApp.Design;
using DevExpress.ExpressApp.Utils;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Atlas.Conta.BackOffice.Blazor.Server {
    public class Program : IDesignTimeApplicationFactory {
        static bool ContainsArgument(string[] args, string argument) {
            return args.Any(arg => arg.TrimStart('/').TrimStart('-').ToLower() == argument.ToLower());
        }
        public static int Main(string[] args) {
            if (ContainsArgument(args, "help") || ContainsArgument(args, "h")) {
                Console.WriteLine("Updates the database when its version does not match the application's version.");
                Console.WriteLine();
                Console.WriteLine($"    {Assembly.GetExecutingAssembly().GetName().Name}.exe --updateDatabase [--forceUpdate --silent]");
                Console.WriteLine();
                Console.WriteLine("--forceUpdate - Marks that the database must be updated whether its version matches the application's version or not.");
                Console.WriteLine("--silent - Marks that database update proceeds automatically and does not require any interaction with the user.");
                Console.WriteLine();
                Console.WriteLine($"Exit codes: 0 - {DBUpdaterStatus.UpdateCompleted}");
                Console.WriteLine($"            1 - {DBUpdaterStatus.UpdateError}");
                Console.WriteLine($"            2 - {DBUpdaterStatus.UpdateNotNeeded}");
            } else {
                DevExpress.ExpressApp.Blazor.Editors.LookupPropertyEditor.DefaultUseViewMode = true;
                DevExpress.ExpressApp.FrameworkSettings.DefaultSettingsCompatibilityMode = DevExpress.ExpressApp.FrameworkSettingsCompatibilityMode.Latest;
                DevExpress.ExpressApp.Security.SecurityStrategy.AutoAssociationReferencePropertyMode = DevExpress.ExpressApp.Security.ReferenceWithoutAssociationPermissionsMode.AllMembers;
                IHost host = CreateHostBuilder(args).Build();
                if (ContainsArgument(args, "updateDatabase")) {
                    using (var serviceScope = host.Services.CreateScope()) {
                        return serviceScope.ServiceProvider.GetRequiredService<DevExpress.ExpressApp.Utils.IDBUpdater>().Update(ContainsArgument(args, "forceUpdate"), ContainsArgument(args, "silent"));
                    }
                } else {
                    // Decizia 51c: convenția de rotunjire e dată a bazei, citită O
                    // DATĂ înainte de a servi primul request (motorul rulează în
                    // cerere și rotunjește la materializare). Pe calea
                    // --updateDatabase o fixează seed-ul însuși.
                    FixeazaConventiaRotunjire(host);
                    host.Run();
                }
            }
            return 0;
        }
        // Citirea e best-effort (bază încă nemigrată / rând absent ⇒ warning și
        // default-ul AwayFromZero, adică exact comportamentul de dinainte de
        // 51c); APLICAREA stă în afara try-ului — o convenție citită și refuzată
        // de `FixeazaConventia` e o contradicție reală, nu o problemă de acces.
        static void FixeazaConventiaRotunjire(IHost host) {
            var logger = host.Services.GetRequiredService<ILoggerFactory>().CreateLogger<Program>();
            MidpointRounding? conventie = null;
            try {
                var connectionString = host.Services.GetRequiredService<IConfiguration>()
                    .GetConnectionString("ConnectionString");
                if (!string.IsNullOrEmpty(connectionString)) {
                    // `UseConnectionString` (extensia XAF), NU `UseNpgsql`:
                    // connection string-ul din appsettings poartă prefixul
                    // `EFCoreProvider=Postgres;`, pe care Npgsql îl refuză —
                    // citirea pica în catch, iar convenția rămânea tăcut pe
                    // default. Extensia scoate prefixul, alege providerul din el
                    // și pune aceleași proxy-uri ca restul aplicației.
                    var builder = new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<
                        Atlas.Conta.BackOffice.Module.BusinessObjects.BackOfficeEFCoreDbContext>();
                    builder.UseConnectionString(connectionString);
                    using var context = new Atlas.Conta.BackOffice.Module.BusinessObjects
                        .BackOfficeEFCoreDbContext(builder.Options);
                    conventie = context.SetariProfil.AsNoTracking()
                        .Select(s => (MidpointRounding?)s.RotunjireBani).FirstOrDefault();
                }
            }
            catch (Exception e) {
                logger.LogWarning(e, "Convenția de rotunjire nu a putut fi citită din bază; "
                    + "se folosește {Conventie}.", Atlas.Conta.BackOffice.Module.BusinessObjects.Scara.ConventieBani);
                return;
            }
            if (conventie == null) {
                logger.LogWarning("Baza nu are rând SetareProfil (încă ne-seed-uită?); convenția de "
                    + "rotunjire rămâne {Conventie}.", Atlas.Conta.BackOffice.Module.BusinessObjects.Scara.ConventieBani);
                return;
            }
            Atlas.Conta.BackOffice.Module.BusinessObjects.Scara.FixeazaConventia(conventie.Value);
        }
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => {
                    // GATE XAF (D13): sub `dotnet run`, static web assets sunt
                    // activate implicit DOAR pe env Development — pe un env custom
                    // (Privat) blazor.server.js și tot /_content/* dau 404, iar
                    // pagina rămâne albă. La publish e no-op (manifestul runtime
                    // nu există; asset-urile sunt copiate fizic în wwwroot).
                    webBuilder.UseStaticWebAssets();
                    webBuilder.UseStartup<Startup>();
                });
        XafApplication IDesignTimeApplicationFactory.Create() {
            IHostBuilder hostBuilder = CreateHostBuilder(Array.Empty<string>());
            return DesignTimeApplicationFactoryHelper.Create(hostBuilder);
        }
    }
}
