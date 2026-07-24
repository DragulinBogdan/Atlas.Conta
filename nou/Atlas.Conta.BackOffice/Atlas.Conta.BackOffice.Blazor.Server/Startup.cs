using Atlas.Conta.BackOffice.Blazor.Server.Services;
using Atlas.DXF.Blazor.Application.Extensions;
using Atlas.DXF.EfCore.Database.Exceptions;
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
                    .WithAuditedDbContext(contexts => {
                        contexts.Configure<Atlas.Conta.BackOffice.Module.BusinessObjects.BackOfficeEFCoreDbContext, Atlas.Conta.BackOffice.Module.BusinessObjects.BackOfficeAuditingDbContext>(
                            (serviceProvider, businessObjectDbContextOptions) => {
                                // Uncomment this code to use an in-memory database. This database is recreated each time the server starts. With the in-memory database, you don't need to make a migration when the data model is changed.
                                // Do not use this code in production environment to avoid data loss.
                                // We recommend that you refer to the following help topic before you use an in-memory database: https://docs.microsoft.com/en-us/ef/core/testing/in-memory
                                //businessObjectDbContextOptions.UseInMemoryDatabase();
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
            // După AddXaf: AtlasDxfExceptionService câștigă rezoluția IExceptionHandlerService,
            // iar violările de constraint DB apar în UI ca mesaje prietenoase.
            services.AddAtlasDxfServices();
            ConstraintViolationMessages.ForeignKeyDeleteTemplate =
                "Nu se poate șterge înregistrarea „{0}”: există înregistrări „{1}” care o referă.";
            ConstraintViolationMessages.ForeignKeyTemplate =
                "Operația intră în conflict cu o referință între înregistrările „{0}” și „{1}”.";
            ConstraintViolationMessages.UniqueTemplate =
                "Există deja o înregistrare „{0}” cu aceleași valori pentru {1}.";
            ConstraintViolationMessages.NotNullTemplate =
                "„{1}” este obligatoriu pe „{0}”.";
            ConstraintViolationMessages.CheckTemplate =
                "Înregistrarea „{0}” încalcă regula „{1}”.";
            ConstraintViolationMessages.FallbackTemplate =
                "Operația încalcă restricția de bază de date „{0}”.";
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
