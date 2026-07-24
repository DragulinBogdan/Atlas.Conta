using Atlas.Conta.BackOffice.WebApi.JWT;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.ApplicationBuilder;
using DevExpress.ExpressApp.Security;
using DevExpress.ExpressApp.Security.Authentication.ClientServer;
using DevExpress.ExpressApp.WebApi.Services;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF.PermissionPolicy;
using DevExpress.Persistent.BaseImpl.EFCore.AuditTrail;
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

            services.AddXafWebApi(builder => {
                builder.ConfigureOptions(options => {
                    // Make your business objects available in the Web API and generate the GET, POST, PUT, and DELETE HTTP methods for it.
                    // options.BusinessObject<YourBusinessObject>();
                });

                builder.Modules
                    .AddReports(options => {
                        options.ReportDataType = typeof(DevExpress.Persistent.BaseImpl.EF.ReportDataV2);
                    })
                    .AddValidation()
                    .Add<Atlas.Conta.BackOffice.Module.BackOfficeModule>();

                builder.ObjectSpaceProviders
                    .AddSecuredEFCore(options => {
                        options.PreFetchReferenceProperties();
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
                            ((SecurityStrategy)securityStrategy).PermissionsReloadMode = PermissionsReloadMode.CacheOnFirstAccess;
                        };
                    })
                    .AddPasswordAuthentication(options => {
                        options.IsSupportChangePassword = true;
                    });

                builder.AddBuildStep(application => {
                    application.ApplicationName = "SetupApplication.Atlas.Conta.BackOffice";
                    application.CheckCompatibilityType = DevExpress.ExpressApp.CheckCompatibilityType.DatabaseSchema;
#if DEBUG
                    if(System.Diagnostics.Debugger.IsAttached && application.CheckCompatibilityType == CheckCompatibilityType.DatabaseSchema) {
                        application.DatabaseUpdateMode = DatabaseUpdateMode.UpdateDatabaseAlways;
                        application.DatabaseVersionMismatch += (s, e) => {
                            e.Updater.Update();
                            e.Handled = true;
                        };
                    }
#endif
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
            app.UseEndpoints(endpoints => {
                endpoints.MapControllers();
                endpoints.MapXafEndpoints();
            });
        }
    }
}
