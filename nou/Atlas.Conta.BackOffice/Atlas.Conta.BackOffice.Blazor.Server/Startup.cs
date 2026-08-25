using Atlas.Conta.BackOffice.Blazor.Server.Services;
using Atlas.Conta.BackOffice.Module.Anaf;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Motor;
using Atlas.DXF.Blazor.Application.Extensions;
using Azure.AI.OpenAI;
using DevExpress.AIIntegration;
using DevExpress.AspNetCore.Reporting;
using DevExpress.Blazor.Reporting;
using DevExpress.ExpressApp.ApplicationBuilder;
using DevExpress.ExpressApp.Blazor.ApplicationBuilder;
using DevExpress.ExpressApp.Blazor.Services;
using DevExpress.ExpressApp.Security;
using DevExpress.Persistent.BaseImpl.EF.PermissionPolicy;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;

namespace Atlas.Conta.BackOffice.Blazor.Server {
    public class Startup {
        public Startup(IConfiguration configuration) {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services) {
            // https://www.npgsql.org/doc/types/datetime.html#timestamps-and-timezones
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

            services.AddSingleton(typeof(Microsoft.AspNetCore.SignalR.HubConnectionHandler<>), typeof(ProxyHubConnectionHandler<>));

            #region AI Chat Client
            services.AddSingleton<IChatClient>(services => {
                string azureOpenAIEndpoint = "http://rama-gx:8000/v1";
                string azureOpenAIKey = null;
                string azureOpenAIDeployment = null;
                if (string.IsNullOrEmpty(azureOpenAIEndpoint) || string.IsNullOrEmpty(azureOpenAIKey) || string.IsNullOrEmpty(azureOpenAIDeployment)) {
                    var message = "Credentials for AI Client were not found." +
                        " Specify azureOpenAIEndpoint, Key and Deployment at Startup.cs" +
                        "\nSee more: https://docs.devexpress.com/Blazor/405228/ai-powered-extensions#prerequisites";
                    throw new InvalidOperationException(message);
                }
                return new AzureOpenAIClient(new(azureOpenAIEndpoint), new System.ClientModel.ApiKeyCredential(azureOpenAIKey))
                        .GetChatClient(azureOpenAIDeployment).AsIChatClient();
            });
            #endregion

            services.AddRazorPages();
            services.AddServerSideBlazor();
            services.AddHttpContextAccessor();
            services.AddScoped<CircuitHandler, CircuitHandlerProxy>();
            services.AddXaf(Configuration, builder => {
                builder.UseApplication<BackOfficeBlazorApplication>();
                builder.Modules
                    // AuditTrail (EF Core) REACTIVAT la DIM-3 (53e re-evaluat):
                    // blocajul era exclusiv owned-ul `Dimensiuni` (PK shadow →
                    // NRE în `AuditTrailService.GetKeyAsObject`, care citește PK-ul
                    // prin reflecție CLR, înaintea oricărei filtrări pe tip). După
                    // DIM-2/DIM-3 modelul nu mai are NICIUN owned type — liniile
                    // poartă FK-uri pe frunze, registrul/regula coloane plate.
                    .AddAuditTrailEFCore()
                    .AddCloning()
                    .AddConditionalAppearance()
                    .AddDashboards(options => {
                        options.DashboardDataType = typeof(DevExpress.Persistent.BaseImpl.EF.DashboardData);
                    })
                    .AddFileAttachments()
                    .AddNotifications()
                    .AddOffice()
                    .AddReports(options => {
                        options.EnableInplaceReports = true;
                        options.ReportDataType = typeof(DevExpress.Persistent.BaseImpl.EF.ReportDataV2);
                        options.ReportStoreMode = DevExpress.ExpressApp.ReportsV2.ReportStoreModes.XML;
                    })
                    .AddScheduler()
                    .AddStateMachine(options => {
                        options.StateMachineStorageType = typeof(DevExpress.Persistent.BaseImpl.EF.StateMachine.StateMachine);
                    })
                    .AddValidation(options => {
                        options.AllowValidationDetailsAccess = false;
                    })
                    .AddViewVariants()
                    .AddAI((options, aiOptions) => {
                        aiOptions.AddBlazorReportingAIIntegration(reportingOptions =>
                            reportingOptions
                                .AddSummarization(summarizeOptions =>
                                    summarizeOptions.SetSummarizationMode(SummarizationMode.Abstractive)
                                        .SetTemperature(0.1f))
                                .AddTranslation(translateOptions =>
                                    translateOptions.SetLanguages(options.Languages)
                                        .EnableTranslation()
                                        .EnableInlineTranslation())
                        );
                        aiOptions.AddWebReportingAIIntegration(reportingOptions =>
                            reportingOptions
                               .AddPromptToReportConverter()
                               .AddPromptToExpressionConverter()
                               .AddLocalization()
                               .AddSummarization(summarizeOptions =>
                                    summarizeOptions.SetSummarizationMode(SummarizationMode.Abstractive))
                               .AddTranslation(translationCfg =>
                                   translationCfg.SetLanguages(options.Languages)
                                       .EnableTranslation()
                                       .EnableInlineTranslation())
                        );
                    })
                    .AddAtlasDxf()
                    .Add<Atlas.Conta.BackOffice.Module.BackOfficeModule>()
                    .Add<BackOfficeBlazorModule>();
                builder.ObjectSpaceProviders
                    .AddSecuredEFCore(options => {
                        options.PreFetchReferenceProperties();
                        // Schema e gestionată prin migrații EF Core (vezi
                        // BackOfficeDesignTimeDbContextFactory); update-ul automat
                        // ar intra în conflict cu ele.
                        options.SchemaUpdateOptions.DisableUpdateSchema = true;
                    })
                    // Contextul AUDITAT (reactivat la DIM-3, ca în WebApi):
                    // hook-ul `AuditedDbContext_SavingChanges` colectează din nou —
                    // fără owned types în model nu mai are pe ce să arunce.
                    .WithAuditedDbContext(contexts => {
                        contexts.Configure<Atlas.Conta.BackOffice.Module.BusinessObjects.BackOfficeEFCoreDbContext,
                            Atlas.Conta.BackOffice.Module.BusinessObjects.BackOfficeAuditingDbContext>(
                            (serviceProvider, businessObjectDbContextOptions) => {
                                string connectionString = null;
                                if (Configuration.GetConnectionString("ConnectionString") != null) {
                                    connectionString = Configuration.GetConnectionString("ConnectionString");
                                }
#if EASYTEST
                                if(Configuration.GetConnectionString("EasyTestConnectionString") != null) {
                                    connectionString = Configuration.GetConnectionString("EasyTestConnectionString");
                                }
#endif
                                ArgumentNullException.ThrowIfNull(connectionString);
                                businessObjectDbContextOptions.UseConnectionString(connectionString);
                            },
                            (serviceProvider, auditHistoryDbContextOptions) => {
                                string connectionString = null;
                                if (Configuration.GetConnectionString("ConnectionString") != null) {
                                    connectionString = Configuration.GetConnectionString("ConnectionString");
                                }
#if EASYTEST
                                if(Configuration.GetConnectionString("EasyTestConnectionString") != null) {
                                    connectionString = Configuration.GetConnectionString("EasyTestConnectionString");
                                }
#endif
                                ArgumentNullException.ThrowIfNull(connectionString);
                                auditHistoryDbContextOptions.UseConnectionString(connectionString);
                            });
                    })
                    .AddNonPersistent();
                builder.Security
                    .UseIntegratedMode(options => {
                        options.Lockout.Enabled = true;

                        options.RoleType = typeof(PermissionPolicyRole);
                        // ApplicationUser descends from PermissionPolicyUser and supports the OAuth authentication. For more information, refer to the following topic: https://docs.devexpress.com/eXpressAppFramework/402197
                        // If your application uses PermissionPolicyUser or a custom user type, set the UserType property as follows:
                        options.UserType = typeof(Atlas.Conta.BackOffice.Module.BusinessObjects.ApplicationUser);
                        // ApplicationUserLoginInfo is only necessary for applications that use the ApplicationUser user type.
                        // If you use PermissionPolicyUser or a custom user type, comment out the following line:
                        options.UserLoginInfoType = typeof(Atlas.Conta.BackOffice.Module.BusinessObjects.ApplicationUserLoginInfo);
                        options.Events.OnSecurityStrategyCreated += securityStrategy => {
                            // Use the 'PermissionsReloadMode.NoCache' option to load the most recent permissions from the database once
                            // for every DbContext instance when secured data is accessed through this instance for the first time.
                            // Use the 'PermissionsReloadMode.CacheOnFirstAccess' option to reduce the number of database queries.
                            // In this case, permission requests are loaded and cached when secured data is accessed for the first time
                            // and used until the current user logs out.
                            // See the following article for more details: https://docs.devexpress.com/eXpressAppFramework/DevExpress.ExpressApp.Security.SecurityStrategy.PermissionsReloadMode.
                            ((SecurityStrategy)securityStrategy).PermissionsReloadMode = PermissionsReloadMode.NoCache;
                        };
                    })
                    .AddPasswordAuthentication(options => {
                        options.IsSupportChangePassword = true;
                    });
            });
            // Gardianul transversal de scriere (spike pasul 5 / D4, decizia 42a):
            // se înregistrează în FIECARE host, fiindcă modulele XAF nu pot adăuga
            // servicii în DI. Seam-ul (`IObjectSpaceCustomizer`) prinde EXACT
            // ObjectSpace-urile secured — vezi probele din `GardianEditare`.
            services.AddContaGardianEditare();
            // După AddXaf: AtlasDxfExceptionService câștigă rezoluția IExceptionHandlerService,
            // iar violările de constraint DB apar în UI ca mesaje prietenoase.
            services.AddAtlasDxfServices();
            // Template-urile RO, partajate cu WebApi (F4-M2): o singură sursă.
            MesajeConstraintRo.Aplica();
            // Clientul registrului ANAF `PlatitorTva` (felia 15, D15-D2) — singurul
            // apel HTTP IEȘITOR al aplicației. Se cablează în host fiindcă modulele
            // XAF nu pot adăuga servicii în DI (aceeași constatare ca la
            // `AddContaGardianEditare` de mai sus); `Module` livrează doar clasa.
            //
            // `AddHttpClient` și nu `new HttpClient()`: fabrica reciclează
            // conexiunile și handler-ele (un `HttpClient` per circuit Blazor ar
            // epuiza socket-urile). URL-ul vine din `Anaf:PlatitorTvaUrl` —
            // secțiune OPȚIONALĂ, absentă azi din `appsettings.json`: lipsa ei lasă
            // URL-ul oficial v9 din cod, prezența o folosește (mediu de test ANAF,
            // proxy de întreprindere).
            //
            // Timeout generos: ANAF răspunde lent pe loturi de 100 de coduri, iar
            // depășirea lui iese ca eroare TRANZITORIE de lot (se poate relua), nu
            // ca excepție care aruncă toată comanda.
            services.AddHttpClient<PlatitorTvaClient>(http => http.Timeout = TimeSpan.FromSeconds(60))
                .AddTypedClient((http, sp) => new PlatitorTvaClient(
                    http, Configuration["Anaf:PlatitorTvaUrl"] ?? PlatitorTvaClient.UrlImplicit));
            var authentication = services.AddAuthentication(options => {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            });
            authentication.AddCookie(options => {
                options.LoginPath = "/LoginPage";
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            } else {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. To change this for production scenarios, see: https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseRequestLocalization();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseAntiforgery();
            app.UseXaf();
            app.UseEndpoints(endpoints => {
                endpoints.MapXafEndpoints();
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
                endpoints.MapControllers();
            });
        }
    }
}
