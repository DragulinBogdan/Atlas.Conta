using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Motor;
using Atlas.Conta.BackOffice.WebApi.JWT;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.ApplicationBuilder;
using DevExpress.ExpressApp.Security;
using DevExpress.ExpressApp.Security.Authentication.ClientServer;
using DevExpress.ExpressApp.WebApi.Services;
using DevExpress.Persistent.BaseImpl.EF.PermissionPolicy;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

namespace Atlas.Conta.BackOffice.WebApi {
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

            services.AddScoped<IAuthenticationTokenProvider, JwtTokenProviderService>();

            // Gardianul transversal de scriere (spike pasul 5 / D4, decizia 42a):
            // aceeași înregistrare ca în Blazor.Server — modulele XAF nu pot adăuga
            // servicii în DI, iar seam-ul (`IObjectSpaceCustomizer`) e singurul care
            // acoperă ȘI OS-urile secured din `IObjectSpaceFactory` (DataService /
            // OData / endpoint-urile feliei), nu doar View-urile Blazor. Probele pe
            // surse: `Atlas.Conta.BackOffice.Module/Motor/GardianEditare.cs`.
            services.AddContaGardianEditare();

            services.AddXafWebApi(builder => {
                builder.ConfigureOptions(options => {
                    // OData OPT-IN (decizia 42f, spike 1 / D7): DOAR nomenclatoarele
                    // consumate de lookup-urile clientului React. NIMIC din
                    // `Document*`/`Registru*` — scrierea trece prin agregatul per
                    // document, citirea prin proiecții; ierarhia nu se expune
                    // polimorf (decizia 6).
                    options.BusinessObject<Gestiune>();
                    options.BusinessObject<TipMaterial>();
                    // `Lot` NU e nomenclator obișnuit (review advers M1): PretUnitar/
                    // Data/Gestiune sunt load-bearing pentru evaluare și FIFO —
                    // loturile se nasc la culegere și se finalizează de motor
                    // (deciziile 13/25c). Prin OData rămâne DOAR de citit
                    // (lookup-ul de pin — 37d); scrierea, când va fi nevoie, vine
                    // prin felia documentului care îl creează.
                    options.BusinessObject<Lot>().ConfigureController(c => c.ReadOnly());
                });

                // Paritate de configurare cu `Blazor.Server/Startup.cs` (aceeași
                // ordine): modulele de mai jos intră oricum în aplicație prin
                // `BackOfficeModule.RequiredModuleTypes`, deci fără liniile astea
                // ar intra NECONFIGURATE (ex. `DashboardDataType` null → excepție
                // la warmup).
                //
                // Formele diferă de Blazor pentru că extensiile `AddCloning`,
                // `AddConditionalAppearance`, `AddStateMachine`, `AddViewVariants`
                // sunt constrânse pe `IApplicationBuilder<TBuilder>` (doar
                // Blazor/Win), iar `AddDashboards`/`AddNotifications`/`AddOffice`/
                // `AddScheduler`/`AddFileAttachments` există DOAR în pachetele de
                // platformă UI (înregistrează servicii și endpoint-uri de UI).
                // Aici folosim `Add<TModule>(setup)` — exact ce fac extensiile
                // acelea în interior, minus partea de UI.
                //
                // Lipsesc deliberat față de Blazor: FileAttachments (nici nu e în
                // RequiredModuleTypes — n-ar avea ce configura), Notifications /
                // Office / Scheduler (Blazor le adaugă fără opțiuni; modulele
                // platform-agnostice vin din RequiredModuleTypes cu default-uri),
                // AddAI și AddAtlasDxf (module de UI Blazor).
                builder.Modules
                    .AddAuditTrailEFCore()
                    .Add<DevExpress.ExpressApp.CloneObject.CloneObjectModule>()
                    .Add<DevExpress.ExpressApp.ConditionalAppearance.ConditionalAppearanceModule>()
                    .Add<DevExpress.ExpressApp.Dashboards.DashboardsModule>(module => {
                        module.DashboardDataType = typeof(DevExpress.Persistent.BaseImpl.EF.DashboardData);
                    })
                    .AddReports(options => {
                        options.EnableInplaceReports = true;
                        options.ReportDataType = typeof(DevExpress.Persistent.BaseImpl.EF.ReportDataV2);
                        options.ReportStoreMode = DevExpress.ExpressApp.ReportsV2.ReportStoreModes.XML;
                    })
                    .Add<DevExpress.ExpressApp.StateMachine.StateMachineModule>(module => {
                        module.StateMachineStorageType = typeof(DevExpress.Persistent.BaseImpl.EF.StateMachine.StateMachine);
                    })
                    .AddValidation(options => {
                        options.AllowValidationDetailsAccess = false;
                    })
                    .Add<DevExpress.ExpressApp.ViewVariantsModule.ViewVariantsModule>()
                    .Add<Atlas.Conta.BackOffice.Module.BackOfficeModule>();

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
                                    ArgumentNullException.ThrowIfNull(connectionString);
                                    businessObjectDbContextOptions.UseConnectionString(connectionString);
                                },
                                (serviceProvider, auditHistoryDbContextOptions) => {
                                    string connectionString = null;
                                    if (Configuration.GetConnectionString("ConnectionString") != null) {
                                        connectionString = Configuration.GetConnectionString("ConnectionString");
                                    }
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
                            // Aliniat cu Blazor.Server (review advers M4): ambele
                            // host-uri servesc ACEEAȘI bază — cache-ul per proces al
                            // scaffold-ului (CacheOnFirstAccess) ar fi făcut ca o
                            // retrogradare de rol să se aplice pe Blazor imediat și
                            // pe API abia la restart.
                            ((SecurityStrategy)securityStrategy).PermissionsReloadMode = PermissionsReloadMode.NoCache;
                        };
                    })
                    .AddPasswordAuthentication(options => {
                        options.IsSupportChangePassword = true;
                    });

                builder.AddBuildStep(application => {
                    application.ApplicationName = "SetupApplication.Atlas.Conta.BackOffice";
                    // Host-ul API VERIFICĂ, nu updatează NICIODATĂ (decizia 42f):
                    // `CheckCompatibilityType.DatabaseSchema` + `DatabaseUpdateMode`
                    // pe default (`UpdateOldDatabase`) înseamnă că
                    // `CheckCompatibilityCore` rulează verificarea, iar la
                    // nepotrivire ridică `DatabaseVersionMismatch`; fără handler
                    // (spre deosebire de Blazor, care apelează `e.Updater.Update()`)
                    // aruncă `CompatibilityException` — exact ce vrem.
                    // Blocul DEBUG al scaffold-ului (`UpdateDatabaseAlways` +
                    // handler care rulează updater-ul) a fost ȘTERS: seed-ul și
                    // migrațiile rămân exclusiv pe calea Blazor `--updateDatabase`,
                    // iar `DisableUpdateSchema` de mai sus ține schema neatinsă.
                    application.CheckCompatibilityType = DevExpress.ExpressApp.CheckCompatibilityType.DatabaseSchema;
                });
            }, Configuration);

            services
                .AddControllers()
                .AddOData((options, serviceProvider) => {
                    options
                        .AddRouteComponents("api/odata", new EdmModelBuilder(serviceProvider).GetEdmModel(), Microsoft.OData.ODataVersion.V401, _routeServices => {
                            _routeServices.ConfigureXafWebApiServices();
                        })
                        .EnableQueryFeatures(100);
                });

            services.AddAuthentication()
                .AddJwtBearer(options => {
                    options.TokenValidationParameters = new TokenValidationParameters() {
                        ValidateIssuerSigningKey = true,
                        //ValidIssuer = Configuration["Authentication:Jwt:Issuer"],
                        //ValidAudience = Configuration["Authentication:Jwt:Audience"],
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Authentication:Jwt:IssuerSigningKey"])),
                        AuthenticationType = JwtBearerDefaults.AuthenticationScheme
                    };
                });

            services.AddAuthorization(options => {
                options.DefaultPolicy = new AuthorizationPolicyBuilder(
                    JwtBearerDefaults.AuthenticationScheme)
                        .RequireAuthenticatedUser()
                        .RequireXafAuthentication()
                        .Build();
            });

            services.AddSwaggerGen(c => {
                c.EnableAnnotations();
                c.SwaggerDoc("v1", new OpenApiInfo {
                    Title = "Atlas.Conta.BackOffice API",
                    Version = "v1",
                    Description = @"Use AddXafWebApi(options) in the Atlas.Conta.BackOffice.WebApi\Startup.cs file to make Business Objects available in the Web API."
                });
                c.AddSecurityDefinition("JWT", new OpenApiSecurityScheme() {
                    Type = SecuritySchemeType.Http,
                    Name = "Bearer",
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement() {
                    {
                        new OpenApiSecurityScheme() {
                            Reference = new OpenApiReference() {
                                Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                                Id = "JWT"
                            }
                        },
                        new string[0]
                    },
                });
            });

            services.Configure<Microsoft.AspNetCore.Mvc.JsonOptions>(o => {
                //The code below specifies that the naming of properties in an object serialized to JSON must always exactly match
                //the property names within the corresponding CLR type so that the property names are displayed correctly in the Swagger UI.
                //XPO is case-sensitive and requires this setting so that the example request data displayed by Swagger is always valid.
                //Comment this code out to revert to the default behavior.
                //See the following article for more information: https://learn.microsoft.com/en-us/dotnet/api/system.text.json.jsonserializeroptions.propertynamingpolicy
                o.JsonSerializerOptions.PropertyNamingPolicy = null;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Atlas.Conta.BackOffice WebApi v1");
                });
            } else {
                // Scaffold-ul trimitea spre o rută „/Error" care nu există în acest
                // host (review advers M3) — răspunsul de producție pe excepții
                // ne-domeniu (DbUpdateException, parse-uri DataSourceLoader) e un
                // 500 JSON generic, fără detalii de server.
                app.UseExceptionHandler(handler => handler.Run(async context => {
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    context.Response.ContentType = "application/json; charset=utf-8";
                    await context.Response.WriteAsync(
                        "{\"Erori\":[\"Eroare internă de server. Reîncercați sau contactați administratorul.\"]}");
                }));
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
            app.UseEndpoints(endpoints => {
                endpoints.MapControllers();
                endpoints.MapXafEndpoints();
            });
        }
    }
}
