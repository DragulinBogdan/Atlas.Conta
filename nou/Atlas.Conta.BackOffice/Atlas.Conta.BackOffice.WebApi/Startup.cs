using Atlas.Conta.BackOffice.Module.Api;
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
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
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
            // Template-urile RO ale traducerii violărilor de constraint DB (F4-M2)
            // — consumate de catch-ul din `ContaApiController.Domeniu`; aceeași
            // sursă ca în Blazor.Server.
            Atlas.Conta.BackOffice.Module.BusinessObjects.MesajeConstraintRo.Aplica();
            // Clientul registrului ANAF `PlatitorTva` (felia 15, D15-D2) —
            // singurul apel HTTP IEȘITOR al aplicației, cablat IDENTIC cu
            // `Blazor.Server/Startup.cs`: modulele XAF nu adaugă servicii în DI
            // (aceeași constatare ca la `AddContaGardianEditare` de mai sus),
            // deci `Module` livrează clasa, iar fiecare host o construiește.
            // `ParteneriController` îl primește prin constructor.
            //
            // `AddHttpClient` și nu `new HttpClient()`: fabrica reciclează
            // conexiunile și handler-ele. URL-ul vine din `Anaf:PlatitorTvaUrl` —
            // secțiune OPȚIONALĂ, absentă azi din `appsettings.json`: lipsa ei
            // lasă URL-ul oficial v9 din cod, prezența o folosește (mediu de test
            // ANAF, proxy de întreprindere).
            //
            // Timeout de 60 s: ANAF răspunde lent pe loturi de 100 de coduri, iar
            // depășirea lui iese ca eroare TRANZITORIE de lot — pe REST, un 503
            // pe care clientul îl poate reîncerca, nu o excepție de 500.
            services.AddHttpClient<Atlas.Conta.BackOffice.Module.Anaf.PlatitorTvaClient>(
                    http => http.Timeout = TimeSpan.FromSeconds(60))
                .AddTypedClient((http, sp) => new Atlas.Conta.BackOffice.Module.Anaf.PlatitorTvaClient(
                    http, Configuration["Anaf:PlatitorTvaUrl"]
                        ?? Atlas.Conta.BackOffice.Module.Anaf.PlatitorTvaClient.UrlImplicit));

            services.AddXafWebApi(builder => {
                builder.ConfigureOptions(options => {
                    // OData OPT-IN (decizia 42f, spike 1 / D7): DOAR nomenclatoarele
                    // consumate de lookup-urile clientului React. NIMIC din
                    // `Document*`/`Registru*` — scrierea trece prin agregatul per
                    // document, citirea prin proiecții; ierarhia nu se expune
                    // polimorf (decizia 6).
                    options.BusinessObject<Gestiune>();
                    options.BusinessObject<TipMaterial>();
                    // Nomenclatoare VII, întreținute din fluxul operațional
                    // (F2-D4): furnizorul nou și produsul nou apar la culegerea
                    // facturii, deci CRUD-ul implicit rămâne — securitatea XAF
                    // decide cine are voie, ca peste tot.
                    options.BusinessObject<Partener>();
                    options.BusinessObject<Produs>();
                    // Angajatul e tot nomenclator VIU (F3-D6): beneficiarul unei
                    // plăți poate fi un salariat nou (avans/decont), cules în
                    // fluxul operațional ca și furnizorul.
                    options.BusinessObject<Angajat>();
                    // Restul e READ-ONLY prin construcție: sunt nomenclatoare de
                    // POLITICĂ, administrate în back-office (planul de conturi,
                    // regimurile de TVA cu conturile lor, clasificația bugetară).
                    // Clientul le citește pentru lookup-uri; a le lăsa scriibile
                    // prin OData ar însemna politică editată pe ușa din dos, în
                    // afara oricărei validări de profil (deciziile 4/29).
                    options.BusinessObject<TipTva>().ConfigureController(c => c.ReadOnly());
                    // Județele (felia 15, D15-D1) sunt LEGE (ISO 3166-2:RO),
                    // seed-uite de nucleu și `[ForbidCRUD]` în XAF: clientul le
                    // citește pentru lookup-ul de adresă, nimeni nu le scrie.
                    options.BusinessObject<Judet>().ConfigureController(c => c.ReadOnly());
                    options.BusinessObject<CodEconomic>().ConfigureController(c => c.ReadOnly());
                    options.BusinessObject<SursaFinantare>().ConfigureController(c => c.ReadOnly());
                    options.BusinessObject<CodFunctional>().ConfigureController(c => c.ReadOnly());
                    options.BusinessObject<Proiect>().ConfigureController(c => c.ReadOnly());
                    // Conturile proprii (casierii/bănci) sunt POLITICĂ, nu
                    // nomenclator viu (F3-D6): `ContImplicit`/`EsteBanca` decid
                    // cum contează motorul plata (31c), deci se administrează în
                    // back-office. Clientul le citește pentru lookup-ul de latură.
                    options.BusinessObject<ContPropriu>().ConfigureController(c => c.ReadOnly());
                    // Unitatea internă = STRUCTURA firmei (sediul emitent al FCL
                    // — F4, amendament la „nimic nou" din F4-D5): se administrează
                    // în back-office, clientul o citește pentru lookup-ul de
                    // emitent. Nomenclator de structură, nu viu — deci ReadOnly.
                    options.BusinessObject<UnitateInterna>().ConfigureController(c => c.ReadOnly());
                    // `Lot` NU e nomenclator obișnuit (review advers M1): PretUnitar/
                    // Data/Gestiune sunt load-bearing pentru evaluare și FIFO —
                    // loturile se nasc la culegere și se finalizează de motor
                    // (deciziile 13/25c). Prin OData rămâne DOAR de citit
                    // (lookup-ul de pin — 37d); scrierea, când va fi nevoie, vine
                    // prin felia documentului care îl creează.
                    options.BusinessObject<Lot>().ConfigureController(c => c.ReadOnly());
                    // Felia DEC (F8-D4) — postarea explicită pe linie și
                    // angajamentul au nevoie de lookup-uri proprii:
                    //   * `Cont` — planul de conturi (1.679 bugetar / 644 privat).
                    //     Nomenclator de POLITICĂ prin excelență (contarea se
                    //     rezolvă din el), deci ReadOnly ca `TipTva`.
                    //   * `Angajament` — entitatea există, tabela e azi GOALĂ
                    //     (modulul de angajamente e amânat — 22c). Lookup-ul e
                    //     onest: gol înseamnă gol; datele vor veni din alt modul.
                    //   * `Repartitor` — BAZA TPT, deliberat: cele două câmpuri
                    //     `RepartitorDebit/Credit` ale postării explicite acceptă
                    //     ORICE repartitor (`ILinieCuPostareExplicita` e tipată pe
                    //     bază), deci un lookup pe una dintre derivatele expuse ar
                    //     minți prin omisiune.
                    options.BusinessObject<Cont>().ConfigureController(c => c.ReadOnly());
                    options.BusinessObject<Angajament>().ConfigureController(c => c.ReadOnly());
                    options.BusinessObject<Repartitor>().ConfigureController(c => c.ReadOnly());
                    // Felia D300 (F12, fix F7 al review-ului advers) — ecranul de
                    // decont are nevoie să ARATE politica de așezare, nu doar
                    // cifrele pe care ea le produce: „nemapat" e o afirmație pe
                    // care operatorul trebuie s-o poată verifica singur.
                    //   * `RandD300` — nomenclatorul rândurilor. E LEGE, nu
                    //     configurare: `[ForbidCRUD]` în XAF, ReadOnly aici, se
                    //     schimbă prin seed odată cu ordinul (D3-D1).
                    //   * `MapareD300` — politică, deci EDITABILĂ… dar în
                    //     back-office, ca toate politicile (56): ReadOnly pe
                    //     OData înseamnă „citește-o din client, schimb-o unde are
                    //     validare de profil". Prin ușa din dos ar fi ocolit atât
                    //     regula rândului de operațiuni, cât și gardul de
                    //     ascendent (F4).
                    options.BusinessObject<RandD300>().ConfigureController(c => c.ReadOnly());
                    options.BusinessObject<MapareD300>().ConfigureController(c => c.ReadOnly());
                    // Felia D394: aceeași regulă — politica se CITEȘTE din client
                    // (ecranul arată de ce un grup e „neinclus"), se schimbă în
                    // back-office, unde are validarea de țintă (D4-D2).
                    options.BusinessObject<MapareD394>().ConfigureController(c => c.ReadOnly());
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
                })
                // ═══ Un singur 400 pe sârmă: `EroriDto` (F13-D3) ═══
                // `[ApiController]` (pe `ContaApiController`) răspunde automat la
                // eșecurile de model binding cu `ValidationProblemDetails` —
                // un al DOILEA shape de 400, nedeclarat în `openapi.json` (toate
                // răspunsurile 400 ale feliilor referă `EroriDto`) și IGNORAT de
                // `nucleu/http.ts`, care caută `Erori[]` și cade pe eroarea
                // tehnică generică, fără detaliul per câmp.
                //
                // Fabrica de mai jos traduce `ModelState` în ACEEAȘI formă ca
                // refuzurile de domeniu ale proiecțiilor: un element per
                // (câmp, mesaj), textul „{câmp}: {mesaj}". Câmpul gol (erorile
                // de formatter fără cale, pe cheia "") rămâne doar cu mesajul;
                // la fel rădăcina JSON "$" (corpul întreg malformat — măsurat:
                // `$: Expected depth to be zero…`), unde prefixul n-ar spune
                // nimic; `$.Data` rămâne, fiindcă numește câmpul.
                //
                // Statusul rămâne **400**, nu 422: cererea e malformată
                // SINTACTIC. 422 e al comenzilor — refuzul DOMENIULUI pe o
                // cerere bine formată (spike D2). Distincția de status rămâne a
                // serverului; clientul afișează `Erori[]` la fel în ambele.
                .ConfigureApiBehaviorOptions(options => {
                    options.InvalidModelStateResponseFactory = context => {
                        var erori = context.ModelState
                            .SelectMany(intrare => intrare.Value.Errors
                                .Select(eroare => string.IsNullOrEmpty(intrare.Key) || intrare.Key == "$"
                                    ? MesajEroare(eroare)
                                    : $"{intrare.Key}: {MesajEroare(eroare)}"))
                            .Where(mesaj => !string.IsNullOrWhiteSpace(mesaj));
                        return new BadRequestObjectResult(EroriDto.Din(erori));
                    };
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

        // Textul unei erori de `ModelState`. `ErrorMessage` e umplut de binder
        // pentru erorile de valoare („The value 'x' is not valid for …") ȘI, cu
        // `AllowInputFormatterExceptionMessages` implicit (true), de
        // `JsonException` la corp malformat. Când lipsește, sursa e o excepție
        // de server: NU o punem pe sârmă (aceeași politică ca handler-ul de 500
        // de mai jos — fără detalii de server în răspuns), ci un mesaj de
        // domeniu, ca răspunsul să rămână util fără a fi indiscret.
        static string MesajEroare(ModelError eroare) =>
            !string.IsNullOrWhiteSpace(eroare.ErrorMessage)
                ? eroare.ErrorMessage
                : "Valoare invalidă.";

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
