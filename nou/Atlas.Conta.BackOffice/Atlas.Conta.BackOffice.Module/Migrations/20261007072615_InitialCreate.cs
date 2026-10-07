using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atlas.Conta.BackOffice.Module.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Angajamente",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    OptimisticLockField = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Activ = table.Column<bool>(type: "boolean", nullable: false),
                    Cod = table.Column<string>(type: "text", nullable: false),
                    Denumire = table.Column<string>(type: "text", nullable: false),
                    Cautare = table.Column<string>(type: "text", nullable: true, computedColumnSql: "translate(lower(coalesce(\"Cod\", '') || ' ' || coalesce(\"Denumire\", '')), 'ăâîșşțţéèêëáàäöüçñ', 'aaisstteeeeaaaoucn')", stored: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Angajamente", x => x.ID);
                    table.CheckConstraint("CK_Angajamente_Cod_negol", "btrim(\"Cod\") <> ''");
                    table.CheckConstraint("CK_Angajamente_Denumire_negol", "btrim(\"Denumire\") <> ''");
                });

            migrationBuilder.CreateTable(
                name: "AuditEFCoreWeakReferences",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    TypeName = table.Column<string>(type: "text", nullable: true),
                    Key = table.Column<string>(type: "text", nullable: true),
                    DefaultString = table.Column<string>(type: "text", nullable: true),
                    LastModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditEFCoreWeakReferences", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "ClaseProduse",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    DinSeed = table.Column<bool>(type: "boolean", nullable: false),
                    Cod = table.Column<string>(type: "text", nullable: false),
                    Denumire = table.Column<string>(type: "text", nullable: false),
                    Natura = table.Column<int>(type: "integer", nullable: false),
                    Cautare = table.Column<string>(type: "text", nullable: true, computedColumnSql: "translate(lower(coalesce(\"Cod\", '') || ' ' || coalesce(\"Denumire\", '')), 'ăâîșşțţéèêëáàäöüçñ', 'aaisstteeeeaaaoucn')", stored: true),
                    OptimisticLockField = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Activ = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClaseProduse", x => x.ID);
                    table.CheckConstraint("CK_ClaseProduse_Cod_negol", "btrim(\"Cod\") <> ''");
                    table.CheckConstraint("CK_ClaseProduse_Denumire_negol", "btrim(\"Denumire\") <> ''");
                });

            migrationBuilder.CreateTable(
                name: "ClasificariImobilizari",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    DurataMinAni = table.Column<int>(type: "integer", nullable: true),
                    DurataMaxAni = table.Column<int>(type: "integer", nullable: true),
                    Grupa = table.Column<string>(type: "text", nullable: true),
                    OptimisticLockField = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Activ = table.Column<bool>(type: "boolean", nullable: false),
                    Cod = table.Column<string>(type: "text", nullable: false),
                    Denumire = table.Column<string>(type: "text", nullable: false),
                    Cautare = table.Column<string>(type: "text", nullable: true, computedColumnSql: "translate(lower(coalesce(\"Cod\", '') || ' ' || coalesce(\"Denumire\", '')), 'ăâîșşțţéèêëáàäöüçñ', 'aaisstteeeeaaaoucn')", stored: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClasificariImobilizari", x => x.ID);
                    table.CheckConstraint("CK_ClasificariImobilizari_Cod_negol", "btrim(\"Cod\") <> ''");
                    table.CheckConstraint("CK_ClasificariImobilizari_Denumire_negol", "btrim(\"Denumire\") <> ''");
                });

            migrationBuilder.CreateTable(
                name: "CoduriEconomice",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    OptimisticLockField = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Activ = table.Column<bool>(type: "boolean", nullable: false),
                    Cod = table.Column<string>(type: "text", nullable: false),
                    Denumire = table.Column<string>(type: "text", nullable: false),
                    Cautare = table.Column<string>(type: "text", nullable: true, computedColumnSql: "translate(lower(coalesce(\"Cod\", '') || ' ' || coalesce(\"Denumire\", '')), 'ăâîșşțţéèêëáàäöüçñ', 'aaisstteeeeaaaoucn')", stored: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoduriEconomice", x => x.ID);
                    table.CheckConstraint("CK_CoduriEconomice_Cod_negol", "btrim(\"Cod\") <> ''");
                    table.CheckConstraint("CK_CoduriEconomice_Denumire_negol", "btrim(\"Denumire\") <> ''");
                });

            migrationBuilder.CreateTable(
                name: "CoduriFunctionale",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    OptimisticLockField = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Activ = table.Column<bool>(type: "boolean", nullable: false),
                    Cod = table.Column<string>(type: "text", nullable: false),
                    Denumire = table.Column<string>(type: "text", nullable: false),
                    Cautare = table.Column<string>(type: "text", nullable: true, computedColumnSql: "translate(lower(coalesce(\"Cod\", '') || ' ' || coalesce(\"Denumire\", '')), 'ăâîșşțţéèêëáàäöüçñ', 'aaisstteeeeaaaoucn')", stored: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoduriFunctionale", x => x.ID);
                    table.CheckConstraint("CK_CoduriFunctionale_Cod_negol", "btrim(\"Cod\") <> ''");
                    table.CheckConstraint("CK_CoduriFunctionale_Denumire_negol", "btrim(\"Denumire\") <> ''");
                });

            migrationBuilder.CreateTable(
                name: "Conturi",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    DinSeed = table.Column<bool>(type: "boolean", nullable: false),
                    Simbol = table.Column<string>(type: "text", nullable: false),
                    Denumire = table.Column<string>(type: "text", nullable: false),
                    ParinteId = table.Column<Guid>(type: "uuid", nullable: true),
                    Functie = table.Column<string>(type: "text", nullable: true),
                    Sumator = table.Column<bool>(type: "boolean", nullable: false),
                    DimensiuniObligatorii = table.Column<int>(type: "integer", nullable: false),
                    RolTert = table.Column<int>(type: "integer", nullable: false),
                    UrmarestePartide = table.Column<bool>(type: "boolean", nullable: false),
                    CategorieStoc = table.Column<int>(type: "integer", nullable: true),
                    Cautare = table.Column<string>(type: "text", nullable: true, computedColumnSql: "translate(lower(coalesce(\"Simbol\", '') || ' ' || coalesce(\"Denumire\", '')), 'ăâîșşțţéèêëáàäöüçñ', 'aaisstteeeeaaaoucn')", stored: true),
                    OptimisticLockField = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Activ = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Conturi", x => x.ID);
                    table.CheckConstraint("CK_Conturi_Denumire_negol", "btrim(\"Denumire\") <> ''");
                    table.CheckConstraint("CK_Conturi_Simbol_negol", "btrim(\"Simbol\") <> ''");
                    table.ForeignKey(
                        name: "FK_Conturi_Conturi_ParinteId",
                        column: x => x.ParinteId,
                        principalTable: "Conturi",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "DashboardData",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: true),
                    Title = table.Column<string>(type: "text", nullable: true),
                    SynchronizeTitle = table.Column<bool>(type: "boolean", nullable: false),
                    OptimisticLockField = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DashboardData", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "DepuneriDeclaratii",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    Formular = table.Column<int>(type: "integer", nullable: false),
                    Perioada = table.Column<int>(type: "integer", nullable: false),
                    VersiuneExportata = table.Column<string>(type: "text", nullable: true),
                    ConfirmataLa = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ConfirmataDe = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DepuneriDeclaratii", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Events",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    Subject = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    StartOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EndOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AllDay = table.Column<bool>(type: "boolean", nullable: false),
                    Location = table.Column<string>(type: "text", nullable: true),
                    Label = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    RecurrenceInfoXml = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    RecurrencePatternID = table.Column<Guid>(type: "uuid", nullable: true),
                    ReminderInfoXml = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    RemindIn = table.Column<TimeSpan>(type: "interval", nullable: true),
                    AlarmTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsPostponed = table.Column<bool>(type: "boolean", nullable: false),
                    OptimisticLockField = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Events", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Events_Events_RecurrencePatternID",
                        column: x => x.RecurrencePatternID,
                        principalTable: "Events",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "FileData",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    Size = table.Column<int>(type: "integer", nullable: false),
                    FileName = table.Column<string>(type: "text", nullable: true),
                    Content = table.Column<byte[]>(type: "bytea", nullable: true),
                    OptimisticLockField = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileData", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Judete",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    Cod = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    Denumire = table.Column<string>(type: "character varying(35)", maxLength: 35, nullable: false),
                    CodAuto = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                    CodCnp = table.Column<int>(type: "integer", nullable: false),
                    Cautare = table.Column<string>(type: "text", nullable: true, computedColumnSql: "translate(lower(coalesce(\"Cod\", '') || ' ' || coalesce(\"Denumire\", '')), 'ăâîșşțţéèêëáàäöüçñ', 'aaisstteeeeaaaoucn')", stored: true),
                    OptimisticLockField = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Activ = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Judete", x => x.ID);
                    table.CheckConstraint("CK_Judete_Cod_negol", "btrim(\"Cod\") <> ''");
                    table.CheckConstraint("CK_Judete_Denumire_negol", "btrim(\"Denumire\") <> ''");
                });

            migrationBuilder.CreateTable(
                name: "MigrareLegaturi",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    Tabela = table.Column<string>(type: "text", nullable: true),
                    CheieLegacy = table.Column<string>(type: "text", nullable: true),
                    TintaId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MigrareLegaturi", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "ModelDifferences",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: true),
                    ContextId = table.Column<string>(type: "text", nullable: true),
                    Version = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModelDifferences", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "PerioadeFiscale",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    An = table.Column<int>(type: "integer", nullable: false),
                    Luna = table.Column<int>(type: "integer", nullable: false),
                    Inchisa = table.Column<bool>(type: "boolean", nullable: false),
                    InchisaLa = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    InchisaPrimaOara = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    OptimisticLockField = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PerioadeFiscale", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "PermissionPolicyRoleBase",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    IsAdministrative = table.Column<bool>(type: "boolean", nullable: false),
                    CanEditModel = table.Column<bool>(type: "boolean", nullable: false),
                    PermissionPolicy = table.Column<int>(type: "integer", nullable: false),
                    IsAllowPermissionPriority = table.Column<bool>(type: "boolean", nullable: false),
                    Discriminator = table.Column<string>(type: "character varying(34)", maxLength: 34, nullable: false),
                    OptimisticLockField = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PermissionPolicyRoleBase", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "PermissionPolicyUser",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    UserName = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    ChangePasswordOnFirstLogon = table.Column<bool>(type: "boolean", nullable: false),
                    StoredPassword = table.Column<string>(type: "text", nullable: true),
                    Discriminator = table.Column<string>(type: "character varying(21)", maxLength: 21, nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: true),
                    LockoutEnd = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    OptimisticLockField = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PermissionPolicyUser", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "PoliticiInchidere",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    DinSeed = table.Column<bool>(type: "boolean", nullable: false),
                    Fel = table.Column<int>(type: "integer", nullable: false),
                    Severitate = table.Column<int>(type: "integer", nullable: false),
                    OptimisticLockField = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PoliticiInchidere", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Proiecte",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    OptimisticLockField = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Activ = table.Column<bool>(type: "boolean", nullable: false),
                    Cod = table.Column<string>(type: "text", nullable: false),
                    Denumire = table.Column<string>(type: "text", nullable: false),
                    Cautare = table.Column<string>(type: "text", nullable: true, computedColumnSql: "translate(lower(coalesce(\"Cod\", '') || ' ' || coalesce(\"Denumire\", '')), 'ăâîșşțţéèêëáàäöüçñ', 'aaisstteeeeaaaoucn')", stored: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Proiecte", x => x.ID);
                    table.CheckConstraint("CK_Proiecte_Cod_negol", "btrim(\"Cod\") <> ''");
                    table.CheckConstraint("CK_Proiecte_Denumire_negol", "btrim(\"Denumire\") <> ''");
                });

            migrationBuilder.CreateTable(
                name: "RanduriD300",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    Cod = table.Column<string>(type: "text", nullable: true),
                    Denumire = table.Column<string>(type: "text", nullable: true),
                    Sectiune = table.Column<int>(type: "integer", nullable: false),
                    Ordine = table.Column<int>(type: "integer", nullable: false),
                    AreBaza = table.Column<bool>(type: "boolean", nullable: false),
                    AreTva = table.Column<bool>(type: "boolean", nullable: false),
                    Fel = table.Column<int>(type: "integer", nullable: false),
                    ParinteId = table.Column<Guid>(type: "uuid", nullable: true),
                    OglindaAId = table.Column<Guid>(type: "uuid", nullable: true),
                    OptimisticLockField = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Activ = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RanduriD300", x => x.ID);
                    table.ForeignKey(
                        name: "FK_RanduriD300_RanduriD300_OglindaAId",
                        column: x => x.OglindaAId,
                        principalTable: "RanduriD300",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_RanduriD300_RanduriD300_ParinteId",
                        column: x => x.ParinteId,
                        principalTable: "RanduriD300",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "RefuzuriSeed",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    Tip = table.Column<string>(type: "text", nullable: true),
                    Cheie = table.Column<string>(type: "text", nullable: true),
                    La = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    OptimisticLockField = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefuzuriSeed", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "ReguliDeductibilitate",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    DinSeed = table.Column<bool>(type: "boolean", nullable: false),
                    Categorie = table.Column<int>(type: "integer", nullable: false),
                    DoarNeexclusiv = table.Column<bool>(type: "boolean", nullable: false),
                    Fel = table.Column<int>(type: "integer", nullable: false),
                    Valoare = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    DeLa = table.Column<DateOnly>(type: "date", nullable: false),
                    PanaLa = table.Column<DateOnly>(type: "date", nullable: true),
                    Temei = table.Column<string>(type: "text", nullable: true),
                    OptimisticLockField = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReguliDeductibilitate", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "ReportDataV2",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    DataTypeName = table.Column<string>(type: "text", nullable: true),
                    IsInplaceReport = table.Column<bool>(type: "boolean", nullable: false),
                    PredefinedReportTypeName = table.Column<string>(type: "text", nullable: true),
                    Content = table.Column<byte[]>(type: "bytea", nullable: true),
                    DisplayName = table.Column<string>(type: "text", nullable: true),
                    ParametersObjectTypeName = table.Column<string>(type: "text", nullable: true),
                    OptimisticLockField = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportDataV2", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Resource",
                columns: table => new
                {
                    Key = table.Column<Guid>(type: "uuid", nullable: false),
                    Caption = table.Column<string>(type: "text", nullable: true),
                    Color_Int = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Resource", x => x.Key);
                });

            migrationBuilder.CreateTable(
                name: "SetariProfil",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    Profil = table.Column<int>(type: "integer", nullable: false),
                    RotunjireBani = table.Column<int>(type: "integer", nullable: false),
                    OptimisticLockField = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SetariProfil", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "SurseFinantare",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    OptimisticLockField = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Activ = table.Column<bool>(type: "boolean", nullable: false),
                    Cod = table.Column<string>(type: "text", nullable: false),
                    Denumire = table.Column<string>(type: "text", nullable: false),
                    Cautare = table.Column<string>(type: "text", nullable: true, computedColumnSql: "translate(lower(coalesce(\"Cod\", '') || ' ' || coalesce(\"Denumire\", '')), 'ăâîșşțţéèêëáàäöüçñ', 'aaisstteeeeaaaoucn')", stored: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SurseFinantare", x => x.ID);
                    table.CheckConstraint("CK_SurseFinantare_Cod_negol", "btrim(\"Cod\") <> ''");
                    table.CheckConstraint("CK_SurseFinantare_Denumire_negol", "btrim(\"Denumire\") <> ''");
                });

            migrationBuilder.CreateTable(
                name: "Unitati",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    Cod = table.Column<string>(type: "text", nullable: true),
                    Denumire = table.Column<string>(type: "text", nullable: true),
                    OptimisticLockField = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Activ = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Unitati", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "UnitatiMasura",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    Cod = table.Column<string>(type: "character varying(9)", maxLength: 9, nullable: false),
                    Denumire = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Cautare = table.Column<string>(type: "text", nullable: true, computedColumnSql: "translate(lower(coalesce(\"Cod\", '') || ' ' || coalesce(\"Denumire\", '')), 'ăâîșşțţéèêëáàäöüçñ', 'aaisstteeeeaaaoucn')", stored: true),
                    OptimisticLockField = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Activ = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnitatiMasura", x => x.ID);
                    table.CheckConstraint("CK_UnitatiMasura_Cod_negol", "btrim(\"Cod\") <> ''");
                    table.CheckConstraint("CK_UnitatiMasura_Denumire_negol", "btrim(\"Denumire\") <> ''");
                });

            migrationBuilder.CreateTable(
                name: "AuditData",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    OperationType = table.Column<string>(type: "text", nullable: true),
                    PropertyName = table.Column<string>(type: "text", nullable: true),
                    OldValue = table.Column<string>(type: "text", nullable: true),
                    NewValue = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    AuditedObjectID = table.Column<Guid>(type: "uuid", nullable: true),
                    OldObjectID = table.Column<Guid>(type: "uuid", nullable: true),
                    NewObjectID = table.Column<Guid>(type: "uuid", nullable: true),
                    UserObjectID = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditData", x => x.ID);
                    table.ForeignKey(
                        name: "FK_AuditData_AuditEFCoreWeakReferences_AuditedObjectID",
                        column: x => x.AuditedObjectID,
                        principalTable: "AuditEFCoreWeakReferences",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_AuditData_AuditEFCoreWeakReferences_NewObjectID",
                        column: x => x.NewObjectID,
                        principalTable: "AuditEFCoreWeakReferences",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_AuditData_AuditEFCoreWeakReferences_OldObjectID",
                        column: x => x.OldObjectID,
                        principalTable: "AuditEFCoreWeakReferences",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_AuditData_AuditEFCoreWeakReferences_UserObjectID",
                        column: x => x.UserObjectID,
                        principalTable: "AuditEFCoreWeakReferences",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "TipuriMaterial",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    RegularizareAvans = table.Column<bool>(type: "boolean", nullable: false),
                    DinSeed = table.Column<bool>(type: "boolean", nullable: false),
                    Cod = table.Column<string>(type: "text", nullable: false),
                    Denumire = table.Column<string>(type: "text", nullable: false),
                    ClasaId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContImplicitId = table.Column<Guid>(type: "uuid", nullable: true),
                    Cautare = table.Column<string>(type: "text", nullable: true, computedColumnSql: "translate(lower(coalesce(\"Cod\", '') || ' ' || coalesce(\"Denumire\", '')), 'ăâîșşțţéèêëáàäöüçñ', 'aaisstteeeeaaaoucn')", stored: true),
                    OptimisticLockField = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Activ = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TipuriMaterial", x => x.ID);
                    table.CheckConstraint("CK_TipuriMaterial_Cod_negol", "btrim(\"Cod\") <> ''");
                    table.CheckConstraint("CK_TipuriMaterial_Denumire_negol", "btrim(\"Denumire\") <> ''");
                    table.ForeignKey(
                        name: "FK_TipuriMaterial_ClaseProduse_ClasaId",
                        column: x => x.ClasaId,
                        principalTable: "ClaseProduse",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_TipuriMaterial_Conturi_ContImplicitId",
                        column: x => x.ContImplicitId,
                        principalTable: "Conturi",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "TipuriTva",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    DinSeed = table.Column<bool>(type: "boolean", nullable: false),
                    Cod = table.Column<string>(type: "text", nullable: false),
                    Denumire = table.Column<string>(type: "text", nullable: false),
                    Cota = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    Regim = table.Column<int>(type: "integer", nullable: false),
                    ValabilDeLa = table.Column<DateOnly>(type: "date", nullable: true),
                    ValabilPanaLa = table.Column<DateOnly>(type: "date", nullable: true),
                    ContTvaDeductibilId = table.Column<Guid>(type: "uuid", nullable: true),
                    ContTvaColectatId = table.Column<Guid>(type: "uuid", nullable: true),
                    ContTvaNeexigibilId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeImport = table.Column<bool>(type: "boolean", nullable: false),
                    CodSafTLivrare = table.Column<string>(type: "text", nullable: true),
                    CodSafTAchizitie = table.Column<string>(type: "text", nullable: true),
                    Cautare = table.Column<string>(type: "text", nullable: true, computedColumnSql: "translate(lower(coalesce(\"Cod\", '') || ' ' || coalesce(\"Denumire\", '')), 'ăâîșşțţéèêëáàäöüçñ', 'aaisstteeeeaaaoucn')", stored: true),
                    OptimisticLockField = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Activ = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TipuriTva", x => x.ID);
                    table.CheckConstraint("CK_TipTva_Interval", "\"ValabilDeLa\" IS NULL OR \"ValabilPanaLa\" IS NULL OR \"ValabilDeLa\" <= \"ValabilPanaLa\"");
                    table.CheckConstraint("CK_TipuriTva_Cod_negol", "btrim(\"Cod\") <> ''");
                    table.CheckConstraint("CK_TipuriTva_Denumire_negol", "btrim(\"Denumire\") <> ''");
                    table.ForeignKey(
                        name: "FK_TipuriTva_Conturi_ContTvaColectatId",
                        column: x => x.ContTvaColectatId,
                        principalTable: "Conturi",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_TipuriTva_Conturi_ContTvaDeductibilId",
                        column: x => x.ContTvaDeductibilId,
                        principalTable: "Conturi",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_TipuriTva_Conturi_ContTvaNeexigibilId",
                        column: x => x.ContTvaNeexigibilId,
                        principalTable: "Conturi",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "ModelDifferenceAspects",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Xml = table.Column<string>(type: "text", nullable: true),
                    OwnerID = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModelDifferenceAspects", x => x.ID);
                    table.ForeignKey(
                        name: "FK_ModelDifferenceAspects_ModelDifferences_OwnerID",
                        column: x => x.OwnerID,
                        principalTable: "ModelDifferences",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InchideriPerioade",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    PerioadaId = table.Column<Guid>(type: "uuid", nullable: false),
                    Fel = table.Column<int>(type: "integer", nullable: false),
                    La = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DeId = table.Column<Guid>(type: "uuid", nullable: true),
                    De = table.Column<string>(type: "text", nullable: true),
                    Motiv = table.Column<string>(type: "text", nullable: true),
                    Acceptari = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InchideriPerioade", x => x.ID);
                    table.ForeignKey(
                        name: "FK_InchideriPerioade_PerioadeFiscale_PerioadaId",
                        column: x => x.PerioadaId,
                        principalTable: "PerioadeFiscale",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "PermissionPolicyActionPermissionObject",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleID = table.Column<Guid>(type: "uuid", nullable: true),
                    ActionId = table.Column<string>(type: "text", nullable: true),
                    OptimisticLockField = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PermissionPolicyActionPermissionObject", x => x.ID);
                    table.ForeignKey(
                        name: "FK_PermissionPolicyActionPermissionObject_PermissionPolicyRole~",
                        column: x => x.RoleID,
                        principalTable: "PermissionPolicyRoleBase",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PermissionPolicyNavigationPermissionObject",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleID = table.Column<Guid>(type: "uuid", nullable: true),
                    ItemPath = table.Column<string>(type: "text", nullable: true),
                    TargetTypeFullName = table.Column<string>(type: "text", nullable: true),
                    NavigateState = table.Column<int>(type: "integer", nullable: true),
                    OptimisticLockField = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PermissionPolicyNavigationPermissionObject", x => x.ID);
                    table.ForeignKey(
                        name: "FK_PermissionPolicyNavigationPermissionObject_PermissionPolicy~",
                        column: x => x.RoleID,
                        principalTable: "PermissionPolicyRoleBase",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PermissionPolicyTypePermissionObject",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetTypeFullName = table.Column<string>(type: "text", nullable: true),
                    RoleID = table.Column<Guid>(type: "uuid", nullable: true),
                    ReadState = table.Column<int>(type: "integer", nullable: true),
                    WriteState = table.Column<int>(type: "integer", nullable: true),
                    CreateState = table.Column<int>(type: "integer", nullable: true),
                    DeleteState = table.Column<int>(type: "integer", nullable: true),
                    NavigateState = table.Column<int>(type: "integer", nullable: true),
                    OptimisticLockField = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PermissionPolicyTypePermissionObject", x => x.ID);
                    table.ForeignKey(
                        name: "FK_PermissionPolicyTypePermissionObject_PermissionPolicyRoleBa~",
                        column: x => x.RoleID,
                        principalTable: "PermissionPolicyRoleBase",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PermissionPolicyRolePermissionPolicyUser",
                columns: table => new
                {
                    RolesID = table.Column<Guid>(type: "uuid", nullable: false),
                    UsersID = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PermissionPolicyRolePermissionPolicyUser", x => new { x.RolesID, x.UsersID });
                    table.ForeignKey(
                        name: "FK_PermissionPolicyRolePermissionPolicyUser_PermissionPolicyRo~",
                        column: x => x.RolesID,
                        principalTable: "PermissionPolicyRoleBase",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PermissionPolicyRolePermissionPolicyUser_PermissionPolicyUs~",
                        column: x => x.UsersID,
                        principalTable: "PermissionPolicyUser",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PermissionPolicyUserLoginInfo",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    LoginProviderName = table.Column<string>(type: "text", nullable: true),
                    ProviderUserKey = table.Column<string>(type: "text", nullable: true),
                    UserForeignKey = table.Column<Guid>(type: "uuid", nullable: false),
                    OptimisticLockField = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PermissionPolicyUserLoginInfo", x => x.ID);
                    table.ForeignKey(
                        name: "FK_PermissionPolicyUserLoginInfo_PermissionPolicyUser_UserFore~",
                        column: x => x.UserForeignKey,
                        principalTable: "PermissionPolicyUser",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EventResource",
                columns: table => new
                {
                    EventsID = table.Column<Guid>(type: "uuid", nullable: false),
                    ResourcesKey = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventResource", x => new { x.EventsID, x.ResourcesKey });
                    table.ForeignKey(
                        name: "FK_EventResource_Events_EventsID",
                        column: x => x.EventsID,
                        principalTable: "Events",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EventResource_Resource_ResourcesKey",
                        column: x => x.ResourcesKey,
                        principalTable: "Resource",
                        principalColumn: "Key",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PoliticiAmortizare",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    DinSeed = table.Column<bool>(type: "boolean", nullable: false),
                    TipMaterialId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContAmortizareId = table.Column<Guid>(type: "uuid", nullable: true),
                    ContCheltuialaAmortizareId = table.Column<Guid>(type: "uuid", nullable: true),
                    ContCheltuialaCedareId = table.Column<Guid>(type: "uuid", nullable: true),
                    OptimisticLockField = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PoliticiAmortizare", x => x.ID);
                    table.ForeignKey(
                        name: "FK_PoliticiAmortizare_Conturi_ContAmortizareId",
                        column: x => x.ContAmortizareId,
                        principalTable: "Conturi",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_PoliticiAmortizare_Conturi_ContCheltuialaAmortizareId",
                        column: x => x.ContCheltuialaAmortizareId,
                        principalTable: "Conturi",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_PoliticiAmortizare_Conturi_ContCheltuialaCedareId",
                        column: x => x.ContCheltuialaCedareId,
                        principalTable: "Conturi",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_PoliticiAmortizare_TipuriMaterial_TipMaterialId",
                        column: x => x.TipMaterialId,
                        principalTable: "TipuriMaterial",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "MapariD300",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    DinSeed = table.Column<bool>(type: "boolean", nullable: false),
                    TipTvaId = table.Column<Guid>(type: "uuid", nullable: false),
                    Sens = table.Column<int>(type: "integer", nullable: false),
                    RandId = table.Column<Guid>(type: "uuid", nullable: false),
                    OptimisticLockField = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MapariD300", x => x.ID);
                    table.ForeignKey(
                        name: "FK_MapariD300_RanduriD300_RandId",
                        column: x => x.RandId,
                        principalTable: "RanduriD300",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_MapariD300_TipuriTva_TipTvaId",
                        column: x => x.TipTvaId,
                        principalTable: "TipuriTva",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "MapariD394",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    DinSeed = table.Column<bool>(type: "boolean", nullable: false),
                    TipTvaId = table.Column<Guid>(type: "uuid", nullable: false),
                    Sens = table.Column<int>(type: "integer", nullable: false),
                    Tip = table.Column<int>(type: "integer", nullable: false),
                    OptimisticLockField = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MapariD394", x => x.ID);
                    table.ForeignKey(
                        name: "FK_MapariD394_TipuriTva_TipTvaId",
                        column: x => x.TipTvaId,
                        principalTable: "TipuriTva",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "MapariTvaSaft",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    DinSeed = table.Column<bool>(type: "boolean", nullable: false),
                    Versiune = table.Column<string>(type: "text", nullable: true),
                    Sectiune = table.Column<int>(type: "integer", nullable: false),
                    TipTvaId = table.Column<Guid>(type: "uuid", nullable: false),
                    Regim = table.Column<int>(type: "integer", nullable: false),
                    Cota = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    DeImport = table.Column<bool>(type: "boolean", nullable: false),
                    Sens = table.Column<int>(type: "integer", nullable: false),
                    Rol = table.Column<int>(type: "integer", nullable: false),
                    TaxType = table.Column<string>(type: "text", nullable: true),
                    TaxCode = table.Column<string>(type: "text", nullable: true),
                    OptimisticLockField = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MapariTvaSaft", x => x.ID);
                    table.ForeignKey(
                        name: "FK_MapariTvaSaft_TipuriTva_TipTvaId",
                        column: x => x.TipTvaId,
                        principalTable: "TipuriTva",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "Produse",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    Cod = table.Column<string>(type: "text", nullable: false),
                    Denumire = table.Column<string>(type: "text", nullable: false),
                    UM = table.Column<string>(type: "text", nullable: true),
                    TipMaterialId = table.Column<Guid>(type: "uuid", nullable: true),
                    UnitateMasuraId = table.Column<Guid>(type: "uuid", nullable: true),
                    CodNc = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: true),
                    TipTvaImplicitId = table.Column<Guid>(type: "uuid", nullable: true),
                    Cautare = table.Column<string>(type: "text", nullable: true, computedColumnSql: "translate(lower(coalesce(\"Cod\", '') || ' ' || coalesce(\"Denumire\", '')), 'ăâîșşțţéèêëáàäöüçñ', 'aaisstteeeeaaaoucn')", stored: true),
                    OptimisticLockField = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Activ = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Produse", x => x.ID);
                    table.CheckConstraint("CK_Produse_Cod_negol", "btrim(\"Cod\") <> ''");
                    table.CheckConstraint("CK_Produse_Denumire_negol", "btrim(\"Denumire\") <> ''");
                    table.ForeignKey(
                        name: "FK_Produse_TipuriMaterial_TipMaterialId",
                        column: x => x.TipMaterialId,
                        principalTable: "TipuriMaterial",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_Produse_TipuriTva_TipTvaImplicitId",
                        column: x => x.TipTvaImplicitId,
                        principalTable: "TipuriTva",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_Produse_UnitatiMasura_UnitateMasuraId",
                        column: x => x.UnitateMasuraId,
                        principalTable: "UnitatiMasura",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "Repartitori",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    ClrType = table.Column<string>(type: "character varying(21)", maxLength: 21, nullable: false),
                    Cod = table.Column<string>(type: "text", nullable: false),
                    Denumire = table.Column<string>(type: "text", nullable: false),
                    Calitati = table.Column<int>(type: "integer", nullable: false),
                    ContImplicitId = table.Column<Guid>(type: "uuid", nullable: true),
                    Cautare = table.Column<string>(type: "text", nullable: true, computedColumnSql: "translate(lower(coalesce(\"Cod\", '') || ' ' || coalesce(\"Denumire\", '')), 'ăâîșşțţéèêëáàäöüçñ', 'aaisstteeeeaaaoucn')", stored: true),
                    Marca = table.Column<string>(type: "text", nullable: true),
                    Iban = table.Column<string>(type: "text", nullable: true),
                    EsteBanca = table.Column<bool>(type: "boolean", nullable: true),
                    CodFiscal = table.Column<string>(type: "text", nullable: true),
                    RegistruComert = table.Column<string>(type: "text", nullable: true),
                    TipPersoana = table.Column<int>(type: "integer", nullable: true),
                    Tara = table.Column<string>(type: "text", nullable: true),
                    InregistratTva = table.Column<bool>(type: "boolean", nullable: true),
                    TvaLaIncasare = table.Column<bool>(type: "boolean", nullable: true),
                    TipTvaImplicitId = table.Column<Guid>(type: "uuid", nullable: true),
                    Strada = table.Column<string>(type: "character varying(70)", maxLength: 70, nullable: true),
                    Numar = table.Column<string>(type: "character varying(18)", maxLength: 18, nullable: true),
                    DetaliiAdresa = table.Column<string>(type: "character varying(70)", maxLength: 70, nullable: true),
                    Localitate = table.Column<string>(type: "character varying(35)", maxLength: 35, nullable: true),
                    CodPostal = table.Column<string>(type: "character varying(18)", maxLength: 18, nullable: true),
                    JudetId = table.Column<Guid>(type: "uuid", nullable: true),
                    DataSincronizareAnaf = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    InactivFiscal = table.Column<bool>(type: "boolean", nullable: true),
                    OptimisticLockField = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Activ = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Repartitori", x => x.ID);
                    table.CheckConstraint("CK_Repartitori_Cod_negol", "btrim(\"Cod\") <> ''");
                    table.CheckConstraint("CK_Repartitori_Denumire_negol", "btrim(\"Denumire\") <> ''");
                    table.ForeignKey(
                        name: "FK_Repartitori_Conturi_ContImplicitId",
                        column: x => x.ContImplicitId,
                        principalTable: "Conturi",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_Repartitori_Judete_JudetId",
                        column: x => x.JudetId,
                        principalTable: "Judete",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_Repartitori_TipuriTva_TipTvaImplicitId",
                        column: x => x.TipTvaImplicitId,
                        principalTable: "TipuriTva",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "TipuriDocument",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    DinSeed = table.Column<bool>(type: "boolean", nullable: false),
                    Cod = table.Column<string>(type: "text", nullable: false),
                    Denumire = table.Column<string>(type: "text", nullable: false),
                    ClrType = table.Column<string>(type: "text", nullable: true),
                    TipTvaImplicitId = table.Column<Guid>(type: "uuid", nullable: true),
                    Cautare = table.Column<string>(type: "text", nullable: true, computedColumnSql: "translate(lower(coalesce(\"Cod\", '') || ' ' || coalesce(\"Denumire\", '')), 'ăâîșşțţéèêëáàäöüçñ', 'aaisstteeeeaaaoucn')", stored: true),
                    OptimisticLockField = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TipuriDocument", x => x.ID);
                    table.CheckConstraint("CK_TipuriDocument_Cod_negol", "btrim(\"Cod\") <> ''");
                    table.CheckConstraint("CK_TipuriDocument_Denumire_negol", "btrim(\"Denumire\") <> ''");
                    table.ForeignKey(
                        name: "FK_TipuriDocument_TipuriTva_TipTvaImplicitId",
                        column: x => x.TipTvaImplicitId,
                        principalTable: "TipuriTva",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "PermissionPolicyMemberPermissionsObject",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    Members = table.Column<string>(type: "text", nullable: true),
                    Criteria = table.Column<string>(type: "text", nullable: true),
                    ReadState = table.Column<int>(type: "integer", nullable: true),
                    WriteState = table.Column<int>(type: "integer", nullable: true),
                    TypePermissionObjectID = table.Column<Guid>(type: "uuid", nullable: true),
                    OptimisticLockField = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PermissionPolicyMemberPermissionsObject", x => x.ID);
                    table.ForeignKey(
                        name: "FK_PermissionPolicyMemberPermissionsObject_PermissionPolicyTyp~",
                        column: x => x.TypePermissionObjectID,
                        principalTable: "PermissionPolicyTypePermissionObject",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PermissionPolicyObjectPermissionsObject",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    Criteria = table.Column<string>(type: "text", nullable: true),
                    ReadState = table.Column<int>(type: "integer", nullable: true),
                    WriteState = table.Column<int>(type: "integer", nullable: true),
                    DeleteState = table.Column<int>(type: "integer", nullable: true),
                    NavigateState = table.Column<int>(type: "integer", nullable: true),
                    TypePermissionObjectID = table.Column<Guid>(type: "uuid", nullable: true),
                    OptimisticLockField = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PermissionPolicyObjectPermissionsObject", x => x.ID);
                    table.ForeignKey(
                        name: "FK_PermissionPolicyObjectPermissionsObject_PermissionPolicyTyp~",
                        column: x => x.TypePermissionObjectID,
                        principalTable: "PermissionPolicyTypePermissionObject",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Documente",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    ClrType = table.Column<string>(type: "character varying(34)", maxLength: 34, nullable: false),
                    Numar = table.Column<string>(type: "text", nullable: true),
                    Data = table.Column<DateOnly>(type: "date", nullable: false),
                    DataInregistrare = table.Column<DateOnly>(type: "date", nullable: false),
                    PredatorId = table.Column<Guid>(type: "uuid", nullable: false),
                    PrimitorId = table.Column<Guid>(type: "uuid", nullable: false),
                    Stare = table.Column<int>(type: "integer", nullable: false),
                    DataOperare = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DocumentSursaId = table.Column<Guid>(type: "uuid", nullable: true),
                    Autogenerat = table.Column<bool>(type: "boolean", nullable: false),
                    CorecteazaId = table.Column<Guid>(type: "uuid", nullable: true),
                    MotivCorectie = table.Column<int>(type: "integer", nullable: true),
                    DataExigibilitate = table.Column<DateOnly>(type: "date", nullable: true),
                    DataPrimire = table.Column<DateOnly>(type: "date", nullable: true),
                    NumarPV = table.Column<string>(type: "text", nullable: true),
                    DataPV = table.Column<DateOnly>(type: "date", nullable: true),
                    TipInstrument = table.Column<int>(type: "integer", nullable: true),
                    NumarExtras = table.Column<string>(type: "text", nullable: true),
                    DataExtras = table.Column<DateOnly>(type: "date", nullable: true),
                    LaturaPerecheId = table.Column<Guid>(type: "uuid", nullable: true),
                    DataScadenta = table.Column<DateOnly>(type: "date", nullable: true),
                    GestiuneDescarcareId = table.Column<Guid>(type: "uuid", nullable: true),
                    CodCpv = table.Column<string>(type: "text", nullable: true),
                    TethysId = table.Column<string>(type: "text", nullable: true),
                    Valuta = table.Column<string>(type: "text", nullable: true),
                    Curs = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: true),
                    GenereazaPlata = table.Column<bool>(type: "boolean", nullable: true),
                    PlataContPropriuId = table.Column<Guid>(type: "uuid", nullable: true),
                    PlataNumar = table.Column<string>(type: "text", nullable: true),
                    PlataData = table.Column<DateOnly>(type: "date", nullable: true),
                    PlataTipInstrument = table.Column<int>(type: "integer", nullable: true),
                    GenereazaChitanta = table.Column<bool>(type: "boolean", nullable: true),
                    ChitantaNumar = table.Column<string>(type: "text", nullable: true),
                    ChitantaData = table.Column<DateOnly>(type: "date", nullable: true),
                    Cauza = table.Column<int>(type: "integer", nullable: true),
                    SursaReceptieiId = table.Column<Guid>(type: "uuid", nullable: true),
                    TranzactieReceptieSursaId = table.Column<Guid>(type: "uuid", nullable: true),
                    OptimisticLockField = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Documente", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Documente_Documente_CorecteazaId",
                        column: x => x.CorecteazaId,
                        principalTable: "Documente",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_Documente_Documente_DocumentSursaId",
                        column: x => x.DocumentSursaId,
                        principalTable: "Documente",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_Documente_Documente_LaturaPerecheId",
                        column: x => x.LaturaPerecheId,
                        principalTable: "Documente",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_Documente_Repartitori_GestiuneDescarcareId",
                        column: x => x.GestiuneDescarcareId,
                        principalTable: "Repartitori",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_Documente_Repartitori_PlataContPropriuId",
                        column: x => x.PlataContPropriuId,
                        principalTable: "Repartitori",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_Documente_Repartitori_PredatorId",
                        column: x => x.PredatorId,
                        principalTable: "Repartitori",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_Documente_Repartitori_PrimitorId",
                        column: x => x.PrimitorId,
                        principalTable: "Repartitori",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "Imobilizari",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    NumarInventar = table.Column<string>(type: "text", nullable: false),
                    Denumire = table.Column<string>(type: "text", nullable: false),
                    TipMaterialId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClasificareId = table.Column<Guid>(type: "uuid", nullable: true),
                    LocId = table.Column<Guid>(type: "uuid", nullable: false),
                    CentruCostId = table.Column<Guid>(type: "uuid", nullable: true),
                    CodEconomicId = table.Column<Guid>(type: "uuid", nullable: true),
                    ResponsabilId = table.Column<Guid>(type: "uuid", nullable: true),
                    Stare = table.Column<int>(type: "integer", nullable: false),
                    DataPunereInFunctiune = table.Column<DateOnly>(type: "date", nullable: true),
                    DataIesire = table.Column<DateOnly>(type: "date", nullable: true),
                    Cautare = table.Column<string>(type: "text", nullable: true, computedColumnSql: "translate(lower(coalesce(\"NumarInventar\", '') || ' ' || coalesce(\"Denumire\", '')), 'ăâîșşțţéèêëáàäöüçñ', 'aaisstteeeeaaaoucn')", stored: true),
                    OptimisticLockField = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Activ = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Imobilizari", x => x.ID);
                    table.CheckConstraint("CK_Imobilizari_Denumire_negol", "btrim(\"Denumire\") <> ''");
                    table.CheckConstraint("CK_Imobilizari_NumarInventar_negol", "btrim(\"NumarInventar\") <> ''");
                    table.ForeignKey(
                        name: "FK_Imobilizari_ClasificariImobilizari_ClasificareId",
                        column: x => x.ClasificareId,
                        principalTable: "ClasificariImobilizari",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_Imobilizari_CoduriEconomice_CodEconomicId",
                        column: x => x.CodEconomicId,
                        principalTable: "CoduriEconomice",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_Imobilizari_Repartitori_CentruCostId",
                        column: x => x.CentruCostId,
                        principalTable: "Repartitori",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_Imobilizari_Repartitori_LocId",
                        column: x => x.LocId,
                        principalTable: "Repartitori",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_Imobilizari_Repartitori_ResponsabilId",
                        column: x => x.ResponsabilId,
                        principalTable: "Repartitori",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_Imobilizari_TipuriMaterial_TipMaterialId",
                        column: x => x.TipMaterialId,
                        principalTable: "TipuriMaterial",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "Loturi",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    ProdusId = table.Column<Guid>(type: "uuid", nullable: false),
                    PretUnitar = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    GestiuneId = table.Column<Guid>(type: "uuid", nullable: false),
                    Data = table.Column<DateOnly>(type: "date", nullable: false),
                    DataExpirare = table.Column<DateOnly>(type: "date", nullable: true),
                    LotFabricatie = table.Column<string>(type: "text", nullable: true),
                    LinieIntrareId = table.Column<Guid>(type: "uuid", nullable: true),
                    OptimisticLockField = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Loturi", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Loturi_Produse_ProdusId",
                        column: x => x.ProdusId,
                        principalTable: "Produse",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_Loturi_Repartitori_GestiuneId",
                        column: x => x.GestiuneId,
                        principalTable: "Repartitori",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "Societati",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    Denumire = table.Column<string>(type: "character varying(70)", maxLength: 70, nullable: true),
                    CodFiscal = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    InregistratTva = table.Column<bool>(type: "boolean", nullable: false),
                    RegistruComert = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Tara = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                    Strada = table.Column<string>(type: "character varying(70)", maxLength: 70, nullable: true),
                    Numar = table.Column<string>(type: "character varying(18)", maxLength: 18, nullable: true),
                    DetaliiAdresa = table.Column<string>(type: "character varying(70)", maxLength: 70, nullable: true),
                    Localitate = table.Column<string>(type: "character varying(35)", maxLength: 35, nullable: true),
                    CodPostal = table.Column<string>(type: "character varying(18)", maxLength: 18, nullable: true),
                    JudetId = table.Column<Guid>(type: "uuid", nullable: true),
                    ContactNume = table.Column<string>(type: "character varying(35)", maxLength: 35, nullable: true),
                    ContactPrenume = table.Column<string>(type: "character varying(35)", maxLength: 35, nullable: true),
                    Telefon = table.Column<string>(type: "character varying(35)", maxLength: 35, nullable: true),
                    Email = table.Column<string>(type: "character varying(70)", maxLength: 70, nullable: true),
                    ContBancarId = table.Column<Guid>(type: "uuid", nullable: true),
                    BazaContabila = table.Column<string>(type: "character varying(18)", maxLength: 18, nullable: true),
                    RaporteazaCnp = table.Column<bool>(type: "boolean", nullable: false),
                    OptimisticLockField = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Societati", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Societati_Judete_JudetId",
                        column: x => x.JudetId,
                        principalTable: "Judete",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_Societati_Repartitori_ContBancarId",
                        column: x => x.ContBancarId,
                        principalTable: "Repartitori",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "SolduriPerioadaContabil",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    An = table.Column<int>(type: "integer", nullable: false),
                    Luna = table.Column<int>(type: "integer", nullable: false),
                    ContId = table.Column<Guid>(type: "uuid", nullable: false),
                    RepartitorId = table.Column<Guid>(type: "uuid", nullable: true),
                    GestiuneId = table.Column<Guid>(type: "uuid", nullable: true),
                    MaterialId = table.Column<Guid>(type: "uuid", nullable: true),
                    CodFunctionalId = table.Column<Guid>(type: "uuid", nullable: true),
                    CodEconomicId = table.Column<Guid>(type: "uuid", nullable: true),
                    SursaFinantareId = table.Column<Guid>(type: "uuid", nullable: true),
                    UnitateId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProiectId = table.Column<Guid>(type: "uuid", nullable: true),
                    CentruCostId = table.Column<Guid>(type: "uuid", nullable: true),
                    Debit = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Credit = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolduriPerioadaContabil", x => x.ID);
                    table.ForeignKey(
                        name: "FK_SolduriPerioadaContabil_CoduriEconomice_CodEconomicId",
                        column: x => x.CodEconomicId,
                        principalTable: "CoduriEconomice",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_SolduriPerioadaContabil_CoduriFunctionale_CodFunctionalId",
                        column: x => x.CodFunctionalId,
                        principalTable: "CoduriFunctionale",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_SolduriPerioadaContabil_Conturi_ContId",
                        column: x => x.ContId,
                        principalTable: "Conturi",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_SolduriPerioadaContabil_Produse_MaterialId",
                        column: x => x.MaterialId,
                        principalTable: "Produse",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_SolduriPerioadaContabil_Proiecte_ProiectId",
                        column: x => x.ProiectId,
                        principalTable: "Proiecte",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_SolduriPerioadaContabil_Repartitori_CentruCostId",
                        column: x => x.CentruCostId,
                        principalTable: "Repartitori",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_SolduriPerioadaContabil_Repartitori_RepartitorId",
                        column: x => x.RepartitorId,
                        principalTable: "Repartitori",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_SolduriPerioadaContabil_SurseFinantare_SursaFinantareId",
                        column: x => x.SursaFinantareId,
                        principalTable: "SurseFinantare",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_SolduriPerioadaContabil_Unitati_UnitateId",
                        column: x => x.UnitateId,
                        principalTable: "Unitati",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "PoliticiConex",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    DinSeed = table.Column<bool>(type: "boolean", nullable: false),
                    TipDocumentSursaId = table.Column<Guid>(type: "uuid", nullable: false),
                    TipDocumentTintaId = table.Column<Guid>(type: "uuid", nullable: false),
                    InverseazaLaturi = table.Column<bool>(type: "boolean", nullable: false),
                    NaturaFiltru = table.Column<int>(type: "integer", nullable: true),
                    OptimisticLockField = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PoliticiConex", x => x.ID);
                    table.ForeignKey(
                        name: "FK_PoliticiConex_TipuriDocument_TipDocumentSursaId",
                        column: x => x.TipDocumentSursaId,
                        principalTable: "TipuriDocument",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_PoliticiConex_TipuriDocument_TipDocumentTintaId",
                        column: x => x.TipDocumentTintaId,
                        principalTable: "TipuriDocument",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "PoliticiDiferenta",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    DinSeed = table.Column<bool>(type: "boolean", nullable: false),
                    TipDocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClasaId = table.Column<Guid>(type: "uuid", nullable: false),
                    Cauza = table.Column<int>(type: "integer", nullable: false),
                    ContId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContPersonalId = table.Column<Guid>(type: "uuid", nullable: true),
                    OptimisticLockField = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PoliticiDiferenta", x => x.ID);
                    table.ForeignKey(
                        name: "FK_PoliticiDiferenta_ClaseProduse_ClasaId",
                        column: x => x.ClasaId,
                        principalTable: "ClaseProduse",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_PoliticiDiferenta_Conturi_ContId",
                        column: x => x.ContId,
                        principalTable: "Conturi",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_PoliticiDiferenta_Conturi_ContPersonalId",
                        column: x => x.ContPersonalId,
                        principalTable: "Conturi",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_PoliticiDiferenta_TipuriDocument_TipDocumentId",
                        column: x => x.TipDocumentId,
                        principalTable: "TipuriDocument",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "PoliticiInchidereTva",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    DinSeed = table.Column<bool>(type: "boolean", nullable: false),
                    TipDocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContDeductibilaId = table.Column<Guid>(type: "uuid", nullable: true),
                    ContColectataId = table.Column<Guid>(type: "uuid", nullable: true),
                    ContDePlataId = table.Column<Guid>(type: "uuid", nullable: true),
                    ContDeRecuperatId = table.Column<Guid>(type: "uuid", nullable: true),
                    OptimisticLockField = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PoliticiInchidereTva", x => x.ID);
                    table.ForeignKey(
                        name: "FK_PoliticiInchidereTva_Conturi_ContColectataId",
                        column: x => x.ContColectataId,
                        principalTable: "Conturi",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_PoliticiInchidereTva_Conturi_ContDePlataId",
                        column: x => x.ContDePlataId,
                        principalTable: "Conturi",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_PoliticiInchidereTva_Conturi_ContDeRecuperatId",
                        column: x => x.ContDeRecuperatId,
                        principalTable: "Conturi",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_PoliticiInchidereTva_Conturi_ContDeductibilaId",
                        column: x => x.ContDeductibilaId,
                        principalTable: "Conturi",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_PoliticiInchidereTva_TipuriDocument_TipDocumentId",
                        column: x => x.TipDocumentId,
                        principalTable: "TipuriDocument",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "PoliticiMiscareSaft",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    DinSeed = table.Column<bool>(type: "boolean", nullable: false),
                    TipDocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    TipStoc = table.Column<int>(type: "integer", nullable: false),
                    Semn = table.Column<int>(type: "integer", nullable: true),
                    CodMiscare = table.Column<string>(type: "character varying(9)", maxLength: 9, nullable: true),
                    RolTert = table.Column<int>(type: "integer", nullable: false),
                    Motiv = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    OptimisticLockField = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PoliticiMiscareSaft", x => x.ID);
                    table.ForeignKey(
                        name: "FK_PoliticiMiscareSaft_TipuriDocument_TipDocumentId",
                        column: x => x.TipDocumentId,
                        principalTable: "TipuriDocument",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "PoliticiNumerotare",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    DinSeed = table.Column<bool>(type: "boolean", nullable: false),
                    TipDocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Serie = table.Column<string>(type: "text", nullable: true),
                    UrmatorulNumar = table.Column<int>(type: "integer", nullable: false),
                    Format = table.Column<string>(type: "text", nullable: true),
                    OptimisticLockField = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PoliticiNumerotare", x => x.ID);
                    table.ForeignKey(
                        name: "FK_PoliticiNumerotare_TipuriDocument_TipDocumentId",
                        column: x => x.TipDocumentId,
                        principalTable: "TipuriDocument",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "PoliticiScadenta",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    DinSeed = table.Column<bool>(type: "boolean", nullable: false),
                    TipDocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    ZileDefault = table.Column<int>(type: "integer", nullable: false),
                    OptimisticLockField = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PoliticiScadenta", x => x.ID);
                    table.ForeignKey(
                        name: "FK_PoliticiScadenta_TipuriDocument_TipDocumentId",
                        column: x => x.TipDocumentId,
                        principalTable: "TipuriDocument",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "PoliticiTva",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    DinSeed = table.Column<bool>(type: "boolean", nullable: false),
                    TipDocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Directie = table.Column<int>(type: "integer", nullable: false),
                    SursaContrapartida = table.Column<int>(type: "integer", nullable: false),
                    ContrapartidaFallbackId = table.Column<Guid>(type: "uuid", nullable: true),
                    TolerantaTaxa = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    OptimisticLockField = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PoliticiTva", x => x.ID);
                    table.ForeignKey(
                        name: "FK_PoliticiTva_Conturi_ContrapartidaFallbackId",
                        column: x => x.ContrapartidaFallbackId,
                        principalTable: "Conturi",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_PoliticiTva_TipuriDocument_TipDocumentId",
                        column: x => x.TipDocumentId,
                        principalTable: "TipuriDocument",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "PoliticiTvaImplicit",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    DinSeed = table.Column<bool>(type: "boolean", nullable: false),
                    TipDocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClasaFiscala = table.Column<int>(type: "integer", nullable: true),
                    ValabilDeLa = table.Column<DateOnly>(type: "date", nullable: true),
                    TipTvaId = table.Column<Guid>(type: "uuid", nullable: false),
                    OptimisticLockField = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PoliticiTvaImplicit", x => x.ID);
                    table.ForeignKey(
                        name: "FK_PoliticiTvaImplicit_TipuriDocument_TipDocumentId",
                        column: x => x.TipDocumentId,
                        principalTable: "TipuriDocument",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_PoliticiTvaImplicit_TipuriTva_TipTvaId",
                        column: x => x.TipTvaId,
                        principalTable: "TipuriTva",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "PoliticiValidare",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    DinSeed = table.Column<bool>(type: "boolean", nullable: false),
                    TipDocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    CereClasificatieBugetara = table.Column<bool>(type: "boolean", nullable: false),
                    NaturaInterzisa = table.Column<int>(type: "integer", nullable: true),
                    OptimisticLockField = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PoliticiValidare", x => x.ID);
                    table.ForeignKey(
                        name: "FK_PoliticiValidare_TipuriDocument_TipDocumentId",
                        column: x => x.TipDocumentId,
                        principalTable: "TipuriDocument",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "ReguliContare",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    DinSeed = table.Column<bool>(type: "boolean", nullable: false),
                    TipDocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    TipMaterialId = table.Column<Guid>(type: "uuid", nullable: true),
                    NaturaFiltru = table.Column<int>(type: "integer", nullable: true),
                    SemnFiltru = table.Column<int>(type: "integer", nullable: true),
                    PastreazaSemn = table.Column<bool>(type: "boolean", nullable: false),
                    SursaContDebit = table.Column<int>(type: "integer", nullable: false),
                    SursaContCredit = table.Column<int>(type: "integer", nullable: false),
                    ContDebitId = table.Column<Guid>(type: "uuid", nullable: true),
                    ContCreditId = table.Column<Guid>(type: "uuid", nullable: true),
                    DimensiuniComun_RepartitorId = table.Column<Guid>(type: "uuid", nullable: true),
                    DimensiuniComun_MaterialId = table.Column<Guid>(type: "uuid", nullable: true),
                    DimensiuniComun_CodFunctionalId = table.Column<Guid>(type: "uuid", nullable: true),
                    DimensiuniComun_CodEconomicId = table.Column<Guid>(type: "uuid", nullable: true),
                    DimensiuniComun_SursaFinantareId = table.Column<Guid>(type: "uuid", nullable: true),
                    DimensiuniComun_UnitateId = table.Column<Guid>(type: "uuid", nullable: true),
                    DimensiuniComun_ProiectId = table.Column<Guid>(type: "uuid", nullable: true),
                    DimensiuniComun_CentruCostId = table.Column<Guid>(type: "uuid", nullable: true),
                    DimensiuniOverrideDebit_RepartitorId = table.Column<Guid>(type: "uuid", nullable: true),
                    DimensiuniOverrideDebit_MaterialId = table.Column<Guid>(type: "uuid", nullable: true),
                    DimensiuniOverrideDebit_CodFunctionalId = table.Column<Guid>(type: "uuid", nullable: true),
                    DimensiuniOverrideDebit_CodEconomicId = table.Column<Guid>(type: "uuid", nullable: true),
                    DimensiuniOverrideDebit_SursaFinantareId = table.Column<Guid>(type: "uuid", nullable: true),
                    DimensiuniOverrideDebit_UnitateId = table.Column<Guid>(type: "uuid", nullable: true),
                    DimensiuniOverrideDebit_ProiectId = table.Column<Guid>(type: "uuid", nullable: true),
                    DimensiuniOverrideDebit_CentruCostId = table.Column<Guid>(type: "uuid", nullable: true),
                    DimensiuniOverrideCredit_RepartitorId = table.Column<Guid>(type: "uuid", nullable: true),
                    DimensiuniOverrideCredit_MaterialId = table.Column<Guid>(type: "uuid", nullable: true),
                    DimensiuniOverrideCredit_CodFunctionalId = table.Column<Guid>(type: "uuid", nullable: true),
                    DimensiuniOverrideCredit_CodEconomicId = table.Column<Guid>(type: "uuid", nullable: true),
                    DimensiuniOverrideCredit_SursaFinantareId = table.Column<Guid>(type: "uuid", nullable: true),
                    DimensiuniOverrideCredit_UnitateId = table.Column<Guid>(type: "uuid", nullable: true),
                    DimensiuniOverrideCredit_ProiectId = table.Column<Guid>(type: "uuid", nullable: true),
                    DimensiuniOverrideCredit_CentruCostId = table.Column<Guid>(type: "uuid", nullable: true),
                    OptimisticLockField = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReguliContare", x => x.ID);
                    table.ForeignKey(
                        name: "FK_ReguliContare_CoduriEconomice_DimensiuniComun_CodEconomicId",
                        column: x => x.DimensiuniComun_CodEconomicId,
                        principalTable: "CoduriEconomice",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_ReguliContare_CoduriEconomice_DimensiuniOverrideCredit_CodE~",
                        column: x => x.DimensiuniOverrideCredit_CodEconomicId,
                        principalTable: "CoduriEconomice",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_ReguliContare_CoduriEconomice_DimensiuniOverrideDebit_CodEc~",
                        column: x => x.DimensiuniOverrideDebit_CodEconomicId,
                        principalTable: "CoduriEconomice",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_ReguliContare_CoduriFunctionale_DimensiuniComun_CodFunction~",
                        column: x => x.DimensiuniComun_CodFunctionalId,
                        principalTable: "CoduriFunctionale",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_ReguliContare_CoduriFunctionale_DimensiuniOverrideCredit_Co~",
                        column: x => x.DimensiuniOverrideCredit_CodFunctionalId,
                        principalTable: "CoduriFunctionale",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_ReguliContare_CoduriFunctionale_DimensiuniOverrideDebit_Cod~",
                        column: x => x.DimensiuniOverrideDebit_CodFunctionalId,
                        principalTable: "CoduriFunctionale",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_ReguliContare_Conturi_ContCreditId",
                        column: x => x.ContCreditId,
                        principalTable: "Conturi",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_ReguliContare_Conturi_ContDebitId",
                        column: x => x.ContDebitId,
                        principalTable: "Conturi",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_ReguliContare_Produse_DimensiuniComun_MaterialId",
                        column: x => x.DimensiuniComun_MaterialId,
                        principalTable: "Produse",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_ReguliContare_Produse_DimensiuniOverrideCredit_MaterialId",
                        column: x => x.DimensiuniOverrideCredit_MaterialId,
                        principalTable: "Produse",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_ReguliContare_Produse_DimensiuniOverrideDebit_MaterialId",
                        column: x => x.DimensiuniOverrideDebit_MaterialId,
                        principalTable: "Produse",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_ReguliContare_Proiecte_DimensiuniComun_ProiectId",
                        column: x => x.DimensiuniComun_ProiectId,
                        principalTable: "Proiecte",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_ReguliContare_Proiecte_DimensiuniOverrideCredit_ProiectId",
                        column: x => x.DimensiuniOverrideCredit_ProiectId,
                        principalTable: "Proiecte",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_ReguliContare_Proiecte_DimensiuniOverrideDebit_ProiectId",
                        column: x => x.DimensiuniOverrideDebit_ProiectId,
                        principalTable: "Proiecte",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_ReguliContare_Repartitori_DimensiuniComun_CentruCostId",
                        column: x => x.DimensiuniComun_CentruCostId,
                        principalTable: "Repartitori",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_ReguliContare_Repartitori_DimensiuniComun_RepartitorId",
                        column: x => x.DimensiuniComun_RepartitorId,
                        principalTable: "Repartitori",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_ReguliContare_Repartitori_DimensiuniOverrideCredit_CentruCo~",
                        column: x => x.DimensiuniOverrideCredit_CentruCostId,
                        principalTable: "Repartitori",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_ReguliContare_Repartitori_DimensiuniOverrideCredit_Repartit~",
                        column: x => x.DimensiuniOverrideCredit_RepartitorId,
                        principalTable: "Repartitori",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_ReguliContare_Repartitori_DimensiuniOverrideDebit_CentruCos~",
                        column: x => x.DimensiuniOverrideDebit_CentruCostId,
                        principalTable: "Repartitori",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_ReguliContare_Repartitori_DimensiuniOverrideDebit_Repartito~",
                        column: x => x.DimensiuniOverrideDebit_RepartitorId,
                        principalTable: "Repartitori",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_ReguliContare_SurseFinantare_DimensiuniComun_SursaFinantare~",
                        column: x => x.DimensiuniComun_SursaFinantareId,
                        principalTable: "SurseFinantare",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_ReguliContare_SurseFinantare_DimensiuniOverrideCredit_Sursa~",
                        column: x => x.DimensiuniOverrideCredit_SursaFinantareId,
                        principalTable: "SurseFinantare",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_ReguliContare_SurseFinantare_DimensiuniOverrideDebit_SursaF~",
                        column: x => x.DimensiuniOverrideDebit_SursaFinantareId,
                        principalTable: "SurseFinantare",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_ReguliContare_TipuriDocument_TipDocumentId",
                        column: x => x.TipDocumentId,
                        principalTable: "TipuriDocument",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_ReguliContare_TipuriMaterial_TipMaterialId",
                        column: x => x.TipMaterialId,
                        principalTable: "TipuriMaterial",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_ReguliContare_Unitati_DimensiuniComun_UnitateId",
                        column: x => x.DimensiuniComun_UnitateId,
                        principalTable: "Unitati",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_ReguliContare_Unitati_DimensiuniOverrideCredit_UnitateId",
                        column: x => x.DimensiuniOverrideCredit_UnitateId,
                        principalTable: "Unitati",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_ReguliContare_Unitati_DimensiuniOverrideDebit_UnitateId",
                        column: x => x.DimensiuniOverrideDebit_UnitateId,
                        principalTable: "Unitati",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "ReguliStoc",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    DinSeed = table.Column<bool>(type: "boolean", nullable: false),
                    TipDocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Latura = table.Column<int>(type: "integer", nullable: false),
                    ClasaId = table.Column<Guid>(type: "uuid", nullable: true),
                    TipStoc = table.Column<int>(type: "integer", nullable: false),
                    Semn = table.Column<int>(type: "integer", nullable: false),
                    OptimisticLockField = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReguliStoc", x => x.ID);
                    table.ForeignKey(
                        name: "FK_ReguliStoc_ClaseProduse_ClasaId",
                        column: x => x.ClasaId,
                        principalTable: "ClaseProduse",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_ReguliStoc_TipuriDocument_TipDocumentId",
                        column: x => x.TipDocumentId,
                        principalTable: "TipuriDocument",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "DviFacturi",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    DviId = table.Column<Guid>(type: "uuid", nullable: false),
                    FacturaId = table.Column<Guid>(type: "uuid", nullable: false),
                    OptimisticLockField = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DviFacturi", x => x.ID);
                    table.ForeignKey(
                        name: "FK_DviFacturi_Documente_DviId",
                        column: x => x.DviId,
                        principalTable: "Documente",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DviFacturi_Documente_FacturaId",
                        column: x => x.FacturaId,
                        principalTable: "Documente",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "Imperecheri",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    TranzactieCubId = table.Column<Guid>(type: "uuid", nullable: true),
                    DocumentStingatorId = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Suma = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Data = table.Column<DateOnly>(type: "date", nullable: false),
                    InverseazaId = table.Column<Guid>(type: "uuid", nullable: true),
                    Autogenerat = table.Column<bool>(type: "boolean", nullable: false),
                    OptimisticLockField = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Imperecheri", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Imperecheri_Documente_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "Documente",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_Imperecheri_Documente_DocumentStingatorId",
                        column: x => x.DocumentStingatorId,
                        principalTable: "Documente",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_Imperecheri_Imperecheri_InverseazaId",
                        column: x => x.InverseazaId,
                        principalTable: "Imperecheri",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "PartideDeschise",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    An = table.Column<int>(type: "integer", nullable: false),
                    Luna = table.Column<int>(type: "integer", nullable: false),
                    DocumentId = table.Column<Guid>(type: "uuid", nullable: true),
                    UnitateId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContId = table.Column<Guid>(type: "uuid", nullable: false),
                    PartenerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Deschisa = table.Column<DateOnly>(type: "date", nullable: false),
                    Debit = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Credit = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Rest = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartideDeschise", x => x.ID);
                    table.ForeignKey(
                        name: "FK_PartideDeschise_Documente_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "Documente",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "Tranzactie",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentId = table.Column<Guid>(type: "uuid", nullable: true),
                    Fel = table.Column<short>(type: "smallint", nullable: false),
                    Data = table.Column<DateOnly>(type: "date", nullable: false),
                    ScrisLa = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Explicatie = table.Column<string>(type: "jsonb", nullable: true),
                    ExplicatieDinId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tranzactie", x => x.ID);
                    table.CheckConstraint("CK_Tranzactie_Explicatie", "\"Explicatie\" IS NULL OR (\"ExplicatieDinId\" IS NULL AND \"DocumentId\" IS NOT NULL AND \"Fel\" IN (1, 3))");
                    table.CheckConstraint("CK_Tranzactie_ExplicatieDin", "\"ExplicatieDinId\" IS NULL OR (\"DocumentId\" IS NOT NULL AND \"Fel\" = 3)");
                    table.ForeignKey(
                        name: "FK_Tranzactie_Documente_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "Documente",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_Tranzactie_Tranzactie_ExplicatieDinId",
                        column: x => x.ExplicatieDinId,
                        principalTable: "Tranzactie",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "DocumentDetalii",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    ClrType = table.Column<string>(type: "character varying(34)", maxLength: 34, nullable: false),
                    DocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Pozitie = table.Column<int>(type: "integer", nullable: false),
                    TipMaterialId = table.Column<Guid>(type: "uuid", nullable: false),
                    LotId = table.Column<Guid>(type: "uuid", nullable: true),
                    Cantitate = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    Valoare = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TipTvaId = table.Column<Guid>(type: "uuid", nullable: true),
                    ValoareTva = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TvaCules = table.Column<bool>(type: "boolean", nullable: false),
                    AngajamentId = table.Column<Guid>(type: "uuid", nullable: true),
                    ImobilizareId = table.Column<Guid>(type: "uuid", nullable: true),
                    ValoareFiscala = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    ValoareDeductibila = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    Luni = table.Column<int>(type: "integer", nullable: true),
                    ContDebitId = table.Column<Guid>(type: "uuid", nullable: true),
                    ContCreditId = table.Column<Guid>(type: "uuid", nullable: true),
                    RepartitorDebitId = table.Column<Guid>(type: "uuid", nullable: true),
                    RepartitorCreditId = table.Column<Guid>(type: "uuid", nullable: true),
                    CentruCostId = table.Column<Guid>(type: "uuid", nullable: true),
                    CodEconomicId = table.Column<Guid>(type: "uuid", nullable: true),
                    Directie = table.Column<int>(type: "integer", nullable: true),
                    ProdusId = table.Column<Guid>(type: "uuid", nullable: true),
                    PretEvaluare = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: true),
                    DataExpirare = table.Column<DateOnly>(type: "date", nullable: true),
                    LotFabricatie = table.Column<string>(type: "text", nullable: true),
                    Descriere = table.Column<string>(type: "text", nullable: true),
                    PretUnitar = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: true),
                    LinieSursaId = table.Column<Guid>(type: "uuid", nullable: true),
                    SursaFinantareId = table.Column<Guid>(type: "uuid", nullable: true),
                    CodFunctionalId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProiectId = table.Column<Guid>(type: "uuid", nullable: true),
                    LinieAvansId = table.Column<Guid>(type: "uuid", nullable: true),
                    CodCpv = table.Column<string>(type: "text", nullable: true),
                    Fel = table.Column<int>(type: "integer", nullable: true),
                    LinieSursaReceptieId = table.Column<Guid>(type: "uuid", nullable: true),
                    CauzaDiferentei = table.Column<int>(type: "integer", nullable: true),
                    PartenerDiferentaId = table.Column<Guid>(type: "uuid", nullable: true),
                    AmortizareInitiala = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    AmortizareFiscalaInitiala = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    LuniAmortizateInitial = table.Column<int>(type: "integer", nullable: true),
                    Metoda = table.Column<int>(type: "integer", nullable: true),
                    DurataLuni = table.Column<int>(type: "integer", nullable: true),
                    ValoareReziduala = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    MetodaFiscala = table.Column<int>(type: "integer", nullable: true),
                    DurataFiscalaLuni = table.Column<int>(type: "integer", nullable: true),
                    CategorieFiscala = table.Column<int>(type: "integer", nullable: true),
                    UtilizareExclusiva = table.Column<bool>(type: "boolean", nullable: true),
                    OptimisticLockField = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentDetalii", x => x.ID);
                    table.CheckConstraint("CK_DocumentDetalii_TvaCules", "NOT \"TvaCules\" OR \"ValoareTva\" <> 0");
                    table.ForeignKey(
                        name: "FK_DocumentDetalii_Angajamente_AngajamentId",
                        column: x => x.AngajamentId,
                        principalTable: "Angajamente",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_DocumentDetalii_CoduriEconomice_CodEconomicId",
                        column: x => x.CodEconomicId,
                        principalTable: "CoduriEconomice",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_DocumentDetalii_CoduriFunctionale_CodFunctionalId",
                        column: x => x.CodFunctionalId,
                        principalTable: "CoduriFunctionale",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_DocumentDetalii_Conturi_ContCreditId",
                        column: x => x.ContCreditId,
                        principalTable: "Conturi",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_DocumentDetalii_Conturi_ContDebitId",
                        column: x => x.ContDebitId,
                        principalTable: "Conturi",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_DocumentDetalii_DocumentDetalii_LinieAvansId",
                        column: x => x.LinieAvansId,
                        principalTable: "DocumentDetalii",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_DocumentDetalii_DocumentDetalii_LinieSursaId",
                        column: x => x.LinieSursaId,
                        principalTable: "DocumentDetalii",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_DocumentDetalii_Documente_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "Documente",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DocumentDetalii_Imobilizari_ImobilizareId",
                        column: x => x.ImobilizareId,
                        principalTable: "Imobilizari",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_DocumentDetalii_Loturi_LotId",
                        column: x => x.LotId,
                        principalTable: "Loturi",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_DocumentDetalii_Produse_ProdusId",
                        column: x => x.ProdusId,
                        principalTable: "Produse",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_DocumentDetalii_Proiecte_ProiectId",
                        column: x => x.ProiectId,
                        principalTable: "Proiecte",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_DocumentDetalii_Repartitori_CentruCostId",
                        column: x => x.CentruCostId,
                        principalTable: "Repartitori",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_DocumentDetalii_Repartitori_PartenerDiferentaId",
                        column: x => x.PartenerDiferentaId,
                        principalTable: "Repartitori",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_DocumentDetalii_Repartitori_RepartitorCreditId",
                        column: x => x.RepartitorCreditId,
                        principalTable: "Repartitori",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_DocumentDetalii_Repartitori_RepartitorDebitId",
                        column: x => x.RepartitorDebitId,
                        principalTable: "Repartitori",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_DocumentDetalii_SurseFinantare_SursaFinantareId",
                        column: x => x.SursaFinantareId,
                        principalTable: "SurseFinantare",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_DocumentDetalii_TipuriMaterial_TipMaterialId",
                        column: x => x.TipMaterialId,
                        principalTable: "TipuriMaterial",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_DocumentDetalii_TipuriTva_TipTvaId",
                        column: x => x.TipTvaId,
                        principalTable: "TipuriTva",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "SolduriPerioadaStoc",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    An = table.Column<int>(type: "integer", nullable: false),
                    Luna = table.Column<int>(type: "integer", nullable: false),
                    LotId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProdusId = table.Column<Guid>(type: "uuid", nullable: false),
                    GestiuneId = table.Column<Guid>(type: "uuid", nullable: false),
                    Deschisa = table.Column<DateOnly>(type: "date", nullable: false),
                    Cantitate = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    Valoare = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolduriPerioadaStoc", x => x.ID);
                    table.ForeignKey(
                        name: "FK_SolduriPerioadaStoc_Loturi_LotId",
                        column: x => x.LotId,
                        principalTable: "Loturi",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "StateMachineAppearances",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetItems = table.Column<string>(type: "text", nullable: true),
                    AppearanceItemType = table.Column<string>(type: "text", nullable: true),
                    Criteria = table.Column<string>(type: "text", nullable: true),
                    Context = table.Column<string>(type: "text", nullable: true),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    FontStyle = table.Column<int>(type: "integer", nullable: true),
                    FontColorInt = table.Column<int>(type: "integer", nullable: false),
                    BackColorInt = table.Column<int>(type: "integer", nullable: false),
                    Visibility = table.Column<int>(type: "integer", nullable: true),
                    Enabled = table.Column<bool>(type: "boolean", nullable: true),
                    Method = table.Column<string>(type: "text", nullable: true),
                    StateID = table.Column<Guid>(type: "uuid", nullable: true),
                    OptimisticLockField = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StateMachineAppearances", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "StateMachines",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Active = table.Column<bool>(type: "boolean", nullable: false),
                    TargetObjectTypeName = table.Column<string>(type: "text", nullable: true),
                    StatePropertyNameBase = table.Column<string>(type: "text", nullable: true),
                    StartStateID = table.Column<Guid>(type: "uuid", nullable: true),
                    ExpandActionsInDetailView = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StateMachines", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "StateMachineStates",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    StateMachineID = table.Column<Guid>(type: "uuid", nullable: true),
                    Caption = table.Column<string>(type: "text", nullable: true),
                    MarkerValue = table.Column<string>(type: "text", nullable: true),
                    TargetObjectCriteria = table.Column<string>(type: "text", nullable: true),
                    OptimisticLockField = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StateMachineStates", x => x.ID);
                    table.ForeignKey(
                        name: "FK_StateMachineStates_StateMachines_StateMachineID",
                        column: x => x.StateMachineID,
                        principalTable: "StateMachines",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StateMachineTransitions",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    Caption = table.Column<string>(type: "text", nullable: true),
                    SourceStateID = table.Column<Guid>(type: "uuid", nullable: true),
                    TargetStateID = table.Column<Guid>(type: "uuid", nullable: true),
                    Index = table.Column<int>(type: "integer", nullable: false),
                    SaveAndCloseView = table.Column<bool>(type: "boolean", nullable: false),
                    OptimisticLockField = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StateMachineTransitions", x => x.ID);
                    table.ForeignKey(
                        name: "FK_StateMachineTransitions_StateMachineStates_SourceStateID",
                        column: x => x.SourceStateID,
                        principalTable: "StateMachineStates",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StateMachineTransitions_StateMachineStates_TargetStateID",
                        column: x => x.TargetStateID,
                        principalTable: "StateMachineStates",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditData_AuditedObjectID",
                table: "AuditData",
                column: "AuditedObjectID");

            migrationBuilder.CreateIndex(
                name: "IX_AuditData_NewObjectID",
                table: "AuditData",
                column: "NewObjectID");

            migrationBuilder.CreateIndex(
                name: "IX_AuditData_OldObjectID",
                table: "AuditData",
                column: "OldObjectID");

            migrationBuilder.CreateIndex(
                name: "IX_AuditData_UserObjectID",
                table: "AuditData",
                column: "UserObjectID");

            migrationBuilder.CreateIndex(
                name: "IX_AuditEFCoreWeakReferences_Key_TypeName",
                table: "AuditEFCoreWeakReferences",
                columns: new[] { "Key", "TypeName" });

            migrationBuilder.CreateIndex(
                name: "IX_ClaseProduse_Cod",
                table: "ClaseProduse",
                column: "Cod",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClasificariImobilizari_Cod",
                table: "ClasificariImobilizari",
                column: "Cod",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Conturi_ParinteId",
                table: "Conturi",
                column: "ParinteId");

            migrationBuilder.CreateIndex(
                name: "IX_Conturi_Simbol",
                table: "Conturi",
                column: "Simbol",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DepuneriDeclaratii_Formular_Perioada_VersiuneExportata",
                table: "DepuneriDeclaratii",
                columns: new[] { "Formular", "Perioada", "VersiuneExportata" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DocumentDetalii_AngajamentId",
                table: "DocumentDetalii",
                column: "AngajamentId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentDetalii_CentruCostId",
                table: "DocumentDetalii",
                column: "CentruCostId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentDetalii_ClrType",
                table: "DocumentDetalii",
                column: "ClrType");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentDetalii_CodEconomicId",
                table: "DocumentDetalii",
                column: "CodEconomicId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentDetalii_CodFunctionalId",
                table: "DocumentDetalii",
                column: "CodFunctionalId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentDetalii_ContCreditId",
                table: "DocumentDetalii",
                column: "ContCreditId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentDetalii_ContDebitId",
                table: "DocumentDetalii",
                column: "ContDebitId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentDetalii_DocumentId",
                table: "DocumentDetalii",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentDetalii_ImobilizareId",
                table: "DocumentDetalii",
                column: "ImobilizareId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentDetalii_LinieAvansId",
                table: "DocumentDetalii",
                column: "LinieAvansId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentDetalii_LinieSursaId",
                table: "DocumentDetalii",
                column: "LinieSursaId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentDetalii_LotId",
                table: "DocumentDetalii",
                column: "LotId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentDetalii_PartenerDiferentaId",
                table: "DocumentDetalii",
                column: "PartenerDiferentaId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentDetalii_ProdusId",
                table: "DocumentDetalii",
                column: "ProdusId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentDetalii_ProiectId",
                table: "DocumentDetalii",
                column: "ProiectId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentDetalii_RepartitorCreditId",
                table: "DocumentDetalii",
                column: "RepartitorCreditId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentDetalii_RepartitorDebitId",
                table: "DocumentDetalii",
                column: "RepartitorDebitId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentDetalii_SursaFinantareId",
                table: "DocumentDetalii",
                column: "SursaFinantareId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentDetalii_TipMaterialId",
                table: "DocumentDetalii",
                column: "TipMaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentDetalii_TipTvaId",
                table: "DocumentDetalii",
                column: "TipTvaId");

            migrationBuilder.CreateIndex(
                name: "IX_Documente_ClrType",
                table: "Documente",
                column: "ClrType");

            migrationBuilder.CreateIndex(
                name: "IX_Documente_CorecteazaId",
                table: "Documente",
                column: "CorecteazaId");

            migrationBuilder.CreateIndex(
                name: "IX_Documente_DataInregistrare",
                table: "Documente",
                column: "DataInregistrare");

            migrationBuilder.CreateIndex(
                name: "IX_Documente_DocumentSursaId",
                table: "Documente",
                column: "DocumentSursaId");

            migrationBuilder.CreateIndex(
                name: "IX_Documente_GestiuneDescarcareId",
                table: "Documente",
                column: "GestiuneDescarcareId");

            migrationBuilder.CreateIndex(
                name: "IX_Documente_LaturaPerecheId",
                table: "Documente",
                column: "LaturaPerecheId");

            migrationBuilder.CreateIndex(
                name: "IX_Documente_PlataContPropriuId",
                table: "Documente",
                column: "PlataContPropriuId");

            migrationBuilder.CreateIndex(
                name: "IX_Documente_PredatorId",
                table: "Documente",
                column: "PredatorId");

            migrationBuilder.CreateIndex(
                name: "IX_Documente_PrimitorId",
                table: "Documente",
                column: "PrimitorId");

            migrationBuilder.CreateIndex(
                name: "IX_DviFacturi_DviId_FacturaId",
                table: "DviFacturi",
                columns: new[] { "DviId", "FacturaId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DviFacturi_FacturaId",
                table: "DviFacturi",
                column: "FacturaId");

            migrationBuilder.CreateIndex(
                name: "IX_EventResource_ResourcesKey",
                table: "EventResource",
                column: "ResourcesKey");

            migrationBuilder.CreateIndex(
                name: "IX_Events_RecurrencePatternID",
                table: "Events",
                column: "RecurrencePatternID");

            migrationBuilder.CreateIndex(
                name: "IX_Imobilizari_CentruCostId",
                table: "Imobilizari",
                column: "CentruCostId");

            migrationBuilder.CreateIndex(
                name: "IX_Imobilizari_ClasificareId",
                table: "Imobilizari",
                column: "ClasificareId");

            migrationBuilder.CreateIndex(
                name: "IX_Imobilizari_CodEconomicId",
                table: "Imobilizari",
                column: "CodEconomicId");

            migrationBuilder.CreateIndex(
                name: "IX_Imobilizari_LocId",
                table: "Imobilizari",
                column: "LocId");

            migrationBuilder.CreateIndex(
                name: "IX_Imobilizari_NumarInventar",
                table: "Imobilizari",
                column: "NumarInventar",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Imobilizari_ResponsabilId",
                table: "Imobilizari",
                column: "ResponsabilId");

            migrationBuilder.CreateIndex(
                name: "IX_Imobilizari_TipMaterialId",
                table: "Imobilizari",
                column: "TipMaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_Imperecheri_Data",
                table: "Imperecheri",
                column: "Data");

            migrationBuilder.CreateIndex(
                name: "IX_Imperecheri_DocumentId",
                table: "Imperecheri",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_Imperecheri_DocumentStingatorId",
                table: "Imperecheri",
                column: "DocumentStingatorId");

            migrationBuilder.CreateIndex(
                name: "IX_Imperecheri_InverseazaId",
                table: "Imperecheri",
                column: "InverseazaId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InchideriPerioade_PerioadaId",
                table: "InchideriPerioade",
                column: "PerioadaId");

            migrationBuilder.CreateIndex(
                name: "IX_Judete_Cod",
                table: "Judete",
                column: "Cod",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Loturi_GestiuneId",
                table: "Loturi",
                column: "GestiuneId");

            migrationBuilder.CreateIndex(
                name: "IX_Loturi_ProdusId",
                table: "Loturi",
                column: "ProdusId");

            migrationBuilder.CreateIndex(
                name: "IX_MapariD300_RandId",
                table: "MapariD300",
                column: "RandId");

            migrationBuilder.CreateIndex(
                name: "IX_MapariD300_TipTvaId_Sens_RandId",
                table: "MapariD300",
                columns: new[] { "TipTvaId", "Sens", "RandId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MapariD394_TipTvaId_Sens",
                table: "MapariD394",
                columns: new[] { "TipTvaId", "Sens" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MapariTvaSaft_TipTvaId",
                table: "MapariTvaSaft",
                column: "TipTvaId");

            migrationBuilder.CreateIndex(
                name: "IX_MapariTvaSaft_Versiune_Sectiune_TipTvaId_Regim_Cota_DeImpor~",
                table: "MapariTvaSaft",
                columns: new[] { "Versiune", "Sectiune", "TipTvaId", "Regim", "Cota", "DeImport", "Sens", "Rol" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MigrareLegaturi_Tabela_CheieLegacy",
                table: "MigrareLegaturi",
                columns: new[] { "Tabela", "CheieLegacy" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ModelDifferenceAspects_OwnerID",
                table: "ModelDifferenceAspects",
                column: "OwnerID");

            migrationBuilder.CreateIndex(
                name: "IX_PartideDeschise_An_Luna",
                table: "PartideDeschise",
                columns: new[] { "An", "Luna" });

            migrationBuilder.CreateIndex(
                name: "IX_PartideDeschise_An_Luna_UnitateId_ContId_PartenerId",
                table: "PartideDeschise",
                columns: new[] { "An", "Luna", "UnitateId", "ContId", "PartenerId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PartideDeschise_DocumentId",
                table: "PartideDeschise",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_PerioadeFiscale_An_Luna",
                table: "PerioadeFiscale",
                columns: new[] { "An", "Luna" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PermissionPolicyActionPermissionObject_RoleID",
                table: "PermissionPolicyActionPermissionObject",
                column: "RoleID");

            migrationBuilder.CreateIndex(
                name: "IX_PermissionPolicyMemberPermissionsObject_TypePermissionObjec~",
                table: "PermissionPolicyMemberPermissionsObject",
                column: "TypePermissionObjectID");

            migrationBuilder.CreateIndex(
                name: "IX_PermissionPolicyNavigationPermissionObject_RoleID",
                table: "PermissionPolicyNavigationPermissionObject",
                column: "RoleID");

            migrationBuilder.CreateIndex(
                name: "IX_PermissionPolicyObjectPermissionsObject_TypePermissionObjec~",
                table: "PermissionPolicyObjectPermissionsObject",
                column: "TypePermissionObjectID");

            migrationBuilder.CreateIndex(
                name: "IX_PermissionPolicyRolePermissionPolicyUser_UsersID",
                table: "PermissionPolicyRolePermissionPolicyUser",
                column: "UsersID");

            migrationBuilder.CreateIndex(
                name: "IX_PermissionPolicyTypePermissionObject_RoleID",
                table: "PermissionPolicyTypePermissionObject",
                column: "RoleID");

            migrationBuilder.CreateIndex(
                name: "IX_PermissionPolicyUserLoginInfo_LoginProviderName_ProviderUse~",
                table: "PermissionPolicyUserLoginInfo",
                columns: new[] { "LoginProviderName", "ProviderUserKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PermissionPolicyUserLoginInfo_UserForeignKey",
                table: "PermissionPolicyUserLoginInfo",
                column: "UserForeignKey");

            migrationBuilder.CreateIndex(
                name: "IX_PoliticiAmortizare_ContAmortizareId",
                table: "PoliticiAmortizare",
                column: "ContAmortizareId");

            migrationBuilder.CreateIndex(
                name: "IX_PoliticiAmortizare_ContCheltuialaAmortizareId",
                table: "PoliticiAmortizare",
                column: "ContCheltuialaAmortizareId");

            migrationBuilder.CreateIndex(
                name: "IX_PoliticiAmortizare_ContCheltuialaCedareId",
                table: "PoliticiAmortizare",
                column: "ContCheltuialaCedareId");

            migrationBuilder.CreateIndex(
                name: "IX_PoliticiAmortizare_TipMaterialId",
                table: "PoliticiAmortizare",
                column: "TipMaterialId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PoliticiConex_TipDocumentSursaId",
                table: "PoliticiConex",
                column: "TipDocumentSursaId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PoliticiConex_TipDocumentTintaId",
                table: "PoliticiConex",
                column: "TipDocumentTintaId");

            migrationBuilder.CreateIndex(
                name: "IX_PoliticiDiferenta_ClasaId",
                table: "PoliticiDiferenta",
                column: "ClasaId");

            migrationBuilder.CreateIndex(
                name: "IX_PoliticiDiferenta_ContId",
                table: "PoliticiDiferenta",
                column: "ContId");

            migrationBuilder.CreateIndex(
                name: "IX_PoliticiDiferenta_ContPersonalId",
                table: "PoliticiDiferenta",
                column: "ContPersonalId");

            migrationBuilder.CreateIndex(
                name: "IX_PoliticiDiferenta_TipDocumentId_Cauza_ClasaId",
                table: "PoliticiDiferenta",
                columns: new[] { "TipDocumentId", "Cauza", "ClasaId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PoliticiInchidere_Fel",
                table: "PoliticiInchidere",
                column: "Fel",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PoliticiInchidereTva_ContColectataId",
                table: "PoliticiInchidereTva",
                column: "ContColectataId");

            migrationBuilder.CreateIndex(
                name: "IX_PoliticiInchidereTva_ContDeductibilaId",
                table: "PoliticiInchidereTva",
                column: "ContDeductibilaId");

            migrationBuilder.CreateIndex(
                name: "IX_PoliticiInchidereTva_ContDePlataId",
                table: "PoliticiInchidereTva",
                column: "ContDePlataId");

            migrationBuilder.CreateIndex(
                name: "IX_PoliticiInchidereTva_ContDeRecuperatId",
                table: "PoliticiInchidereTva",
                column: "ContDeRecuperatId");

            migrationBuilder.CreateIndex(
                name: "IX_PoliticiInchidereTva_TipDocumentId",
                table: "PoliticiInchidereTva",
                column: "TipDocumentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PoliticiMiscareSaft_TipDocumentId_TipStoc",
                table: "PoliticiMiscareSaft",
                columns: new[] { "TipDocumentId", "TipStoc" },
                unique: true,
                filter: "\"Semn\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PoliticiMiscareSaft_TipDocumentId_TipStoc_Semn",
                table: "PoliticiMiscareSaft",
                columns: new[] { "TipDocumentId", "TipStoc", "Semn" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PoliticiNumerotare_TipDocumentId",
                table: "PoliticiNumerotare",
                column: "TipDocumentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PoliticiScadenta_TipDocumentId",
                table: "PoliticiScadenta",
                column: "TipDocumentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PoliticiTva_ContrapartidaFallbackId",
                table: "PoliticiTva",
                column: "ContrapartidaFallbackId");

            migrationBuilder.CreateIndex(
                name: "IX_PoliticiTva_TipDocumentId",
                table: "PoliticiTva",
                column: "TipDocumentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PoliticiTvaImplicit_TipDocumentId_ClasaFiscala_ValabilDeLa",
                table: "PoliticiTvaImplicit",
                columns: new[] { "TipDocumentId", "ClasaFiscala", "ValabilDeLa" },
                unique: true)
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_PoliticiTvaImplicit_TipTvaId",
                table: "PoliticiTvaImplicit",
                column: "TipTvaId");

            migrationBuilder.CreateIndex(
                name: "IX_PoliticiValidare_TipDocumentId",
                table: "PoliticiValidare",
                column: "TipDocumentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Produse_TipMaterialId",
                table: "Produse",
                column: "TipMaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_Produse_TipTvaImplicitId",
                table: "Produse",
                column: "TipTvaImplicitId");

            migrationBuilder.CreateIndex(
                name: "IX_Produse_UnitateMasuraId",
                table: "Produse",
                column: "UnitateMasuraId");

            migrationBuilder.CreateIndex(
                name: "IX_RanduriD300_Cod",
                table: "RanduriD300",
                column: "Cod",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RanduriD300_OglindaAId",
                table: "RanduriD300",
                column: "OglindaAId");

            migrationBuilder.CreateIndex(
                name: "IX_RanduriD300_ParinteId",
                table: "RanduriD300",
                column: "ParinteId");

            migrationBuilder.CreateIndex(
                name: "IX_RefuzuriSeed_Tip_Cheie",
                table: "RefuzuriSeed",
                columns: new[] { "Tip", "Cheie" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReguliContare_ContCreditId",
                table: "ReguliContare",
                column: "ContCreditId");

            migrationBuilder.CreateIndex(
                name: "IX_ReguliContare_ContDebitId",
                table: "ReguliContare",
                column: "ContDebitId");

            migrationBuilder.CreateIndex(
                name: "IX_ReguliContare_DimensiuniComun_CentruCostId",
                table: "ReguliContare",
                column: "DimensiuniComun_CentruCostId");

            migrationBuilder.CreateIndex(
                name: "IX_ReguliContare_DimensiuniComun_CodEconomicId",
                table: "ReguliContare",
                column: "DimensiuniComun_CodEconomicId");

            migrationBuilder.CreateIndex(
                name: "IX_ReguliContare_DimensiuniComun_CodFunctionalId",
                table: "ReguliContare",
                column: "DimensiuniComun_CodFunctionalId");

            migrationBuilder.CreateIndex(
                name: "IX_ReguliContare_DimensiuniComun_MaterialId",
                table: "ReguliContare",
                column: "DimensiuniComun_MaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_ReguliContare_DimensiuniComun_ProiectId",
                table: "ReguliContare",
                column: "DimensiuniComun_ProiectId");

            migrationBuilder.CreateIndex(
                name: "IX_ReguliContare_DimensiuniComun_RepartitorId",
                table: "ReguliContare",
                column: "DimensiuniComun_RepartitorId");

            migrationBuilder.CreateIndex(
                name: "IX_ReguliContare_DimensiuniComun_SursaFinantareId",
                table: "ReguliContare",
                column: "DimensiuniComun_SursaFinantareId");

            migrationBuilder.CreateIndex(
                name: "IX_ReguliContare_DimensiuniComun_UnitateId",
                table: "ReguliContare",
                column: "DimensiuniComun_UnitateId");

            migrationBuilder.CreateIndex(
                name: "IX_ReguliContare_DimensiuniOverrideCredit_CentruCostId",
                table: "ReguliContare",
                column: "DimensiuniOverrideCredit_CentruCostId");

            migrationBuilder.CreateIndex(
                name: "IX_ReguliContare_DimensiuniOverrideCredit_CodEconomicId",
                table: "ReguliContare",
                column: "DimensiuniOverrideCredit_CodEconomicId");

            migrationBuilder.CreateIndex(
                name: "IX_ReguliContare_DimensiuniOverrideCredit_CodFunctionalId",
                table: "ReguliContare",
                column: "DimensiuniOverrideCredit_CodFunctionalId");

            migrationBuilder.CreateIndex(
                name: "IX_ReguliContare_DimensiuniOverrideCredit_MaterialId",
                table: "ReguliContare",
                column: "DimensiuniOverrideCredit_MaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_ReguliContare_DimensiuniOverrideCredit_ProiectId",
                table: "ReguliContare",
                column: "DimensiuniOverrideCredit_ProiectId");

            migrationBuilder.CreateIndex(
                name: "IX_ReguliContare_DimensiuniOverrideCredit_RepartitorId",
                table: "ReguliContare",
                column: "DimensiuniOverrideCredit_RepartitorId");

            migrationBuilder.CreateIndex(
                name: "IX_ReguliContare_DimensiuniOverrideCredit_SursaFinantareId",
                table: "ReguliContare",
                column: "DimensiuniOverrideCredit_SursaFinantareId");

            migrationBuilder.CreateIndex(
                name: "IX_ReguliContare_DimensiuniOverrideCredit_UnitateId",
                table: "ReguliContare",
                column: "DimensiuniOverrideCredit_UnitateId");

            migrationBuilder.CreateIndex(
                name: "IX_ReguliContare_DimensiuniOverrideDebit_CentruCostId",
                table: "ReguliContare",
                column: "DimensiuniOverrideDebit_CentruCostId");

            migrationBuilder.CreateIndex(
                name: "IX_ReguliContare_DimensiuniOverrideDebit_CodEconomicId",
                table: "ReguliContare",
                column: "DimensiuniOverrideDebit_CodEconomicId");

            migrationBuilder.CreateIndex(
                name: "IX_ReguliContare_DimensiuniOverrideDebit_CodFunctionalId",
                table: "ReguliContare",
                column: "DimensiuniOverrideDebit_CodFunctionalId");

            migrationBuilder.CreateIndex(
                name: "IX_ReguliContare_DimensiuniOverrideDebit_MaterialId",
                table: "ReguliContare",
                column: "DimensiuniOverrideDebit_MaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_ReguliContare_DimensiuniOverrideDebit_ProiectId",
                table: "ReguliContare",
                column: "DimensiuniOverrideDebit_ProiectId");

            migrationBuilder.CreateIndex(
                name: "IX_ReguliContare_DimensiuniOverrideDebit_RepartitorId",
                table: "ReguliContare",
                column: "DimensiuniOverrideDebit_RepartitorId");

            migrationBuilder.CreateIndex(
                name: "IX_ReguliContare_DimensiuniOverrideDebit_SursaFinantareId",
                table: "ReguliContare",
                column: "DimensiuniOverrideDebit_SursaFinantareId");

            migrationBuilder.CreateIndex(
                name: "IX_ReguliContare_DimensiuniOverrideDebit_UnitateId",
                table: "ReguliContare",
                column: "DimensiuniOverrideDebit_UnitateId");

            migrationBuilder.CreateIndex(
                name: "IX_ReguliContare_TipDocumentId_TipMaterialId_NaturaFiltru_Semn~",
                table: "ReguliContare",
                columns: new[] { "TipDocumentId", "TipMaterialId", "NaturaFiltru", "SemnFiltru" },
                unique: true)
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ReguliContare_TipMaterialId",
                table: "ReguliContare",
                column: "TipMaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_ReguliDeductibilitate_Categorie_Fel_DeLa",
                table: "ReguliDeductibilitate",
                columns: new[] { "Categorie", "Fel", "DeLa" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReguliStoc_ClasaId",
                table: "ReguliStoc",
                column: "ClasaId");

            migrationBuilder.CreateIndex(
                name: "IX_ReguliStoc_TipDocumentId_Latura_ClasaId",
                table: "ReguliStoc",
                columns: new[] { "TipDocumentId", "Latura", "ClasaId" },
                unique: true)
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_Repartitori_ClrType",
                table: "Repartitori",
                column: "ClrType");

            migrationBuilder.CreateIndex(
                name: "IX_Repartitori_ContImplicitId",
                table: "Repartitori",
                column: "ContImplicitId");

            migrationBuilder.CreateIndex(
                name: "IX_Repartitori_JudetId",
                table: "Repartitori",
                column: "JudetId");

            migrationBuilder.CreateIndex(
                name: "IX_Repartitori_TipTvaImplicitId",
                table: "Repartitori",
                column: "TipTvaImplicitId");

            migrationBuilder.CreateIndex(
                name: "IX_Societati_ContBancarId",
                table: "Societati",
                column: "ContBancarId");

            migrationBuilder.CreateIndex(
                name: "IX_Societati_JudetId",
                table: "Societati",
                column: "JudetId");

            migrationBuilder.CreateIndex(
                name: "IX_SolduriPerioadaContabil_An_Luna",
                table: "SolduriPerioadaContabil",
                columns: new[] { "An", "Luna" });

            migrationBuilder.CreateIndex(
                name: "IX_SolduriPerioadaContabil_An_Luna_ContId_RepartitorId_Gestiun~",
                table: "SolduriPerioadaContabil",
                columns: new[] { "An", "Luna", "ContId", "RepartitorId", "GestiuneId", "MaterialId", "CodFunctionalId", "CodEconomicId", "SursaFinantareId", "UnitateId", "ProiectId", "CentruCostId" },
                unique: true)
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_SolduriPerioadaContabil_CentruCostId",
                table: "SolduriPerioadaContabil",
                column: "CentruCostId");

            migrationBuilder.CreateIndex(
                name: "IX_SolduriPerioadaContabil_CodEconomicId",
                table: "SolduriPerioadaContabil",
                column: "CodEconomicId");

            migrationBuilder.CreateIndex(
                name: "IX_SolduriPerioadaContabil_CodFunctionalId",
                table: "SolduriPerioadaContabil",
                column: "CodFunctionalId");

            migrationBuilder.CreateIndex(
                name: "IX_SolduriPerioadaContabil_ContId",
                table: "SolduriPerioadaContabil",
                column: "ContId");

            migrationBuilder.CreateIndex(
                name: "IX_SolduriPerioadaContabil_MaterialId",
                table: "SolduriPerioadaContabil",
                column: "MaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_SolduriPerioadaContabil_ProiectId",
                table: "SolduriPerioadaContabil",
                column: "ProiectId");

            migrationBuilder.CreateIndex(
                name: "IX_SolduriPerioadaContabil_RepartitorId",
                table: "SolduriPerioadaContabil",
                column: "RepartitorId");

            migrationBuilder.CreateIndex(
                name: "IX_SolduriPerioadaContabil_SursaFinantareId",
                table: "SolduriPerioadaContabil",
                column: "SursaFinantareId");

            migrationBuilder.CreateIndex(
                name: "IX_SolduriPerioadaContabil_UnitateId",
                table: "SolduriPerioadaContabil",
                column: "UnitateId");

            migrationBuilder.CreateIndex(
                name: "IX_SolduriPerioadaStoc_An_Luna",
                table: "SolduriPerioadaStoc",
                columns: new[] { "An", "Luna" });

            migrationBuilder.CreateIndex(
                name: "IX_SolduriPerioadaStoc_An_Luna_LotId_ContId_ProdusId_GestiuneId",
                table: "SolduriPerioadaStoc",
                columns: new[] { "An", "Luna", "LotId", "ContId", "ProdusId", "GestiuneId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SolduriPerioadaStoc_LotId",
                table: "SolduriPerioadaStoc",
                column: "LotId");

            migrationBuilder.CreateIndex(
                name: "IX_StateMachineAppearances_StateID",
                table: "StateMachineAppearances",
                column: "StateID");

            migrationBuilder.CreateIndex(
                name: "IX_StateMachines_StartStateID",
                table: "StateMachines",
                column: "StartStateID");

            migrationBuilder.CreateIndex(
                name: "IX_StateMachineStates_StateMachineID",
                table: "StateMachineStates",
                column: "StateMachineID");

            migrationBuilder.CreateIndex(
                name: "IX_StateMachineTransitions_SourceStateID",
                table: "StateMachineTransitions",
                column: "SourceStateID");

            migrationBuilder.CreateIndex(
                name: "IX_StateMachineTransitions_TargetStateID",
                table: "StateMachineTransitions",
                column: "TargetStateID");

            migrationBuilder.CreateIndex(
                name: "IX_TipuriDocument_ClrType",
                table: "TipuriDocument",
                column: "ClrType",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TipuriDocument_Cod",
                table: "TipuriDocument",
                column: "Cod",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TipuriDocument_TipTvaImplicitId",
                table: "TipuriDocument",
                column: "TipTvaImplicitId");

            migrationBuilder.CreateIndex(
                name: "IX_TipuriMaterial_ClasaId",
                table: "TipuriMaterial",
                column: "ClasaId");

            migrationBuilder.CreateIndex(
                name: "IX_TipuriMaterial_Cod",
                table: "TipuriMaterial",
                column: "Cod",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TipuriMaterial_ContImplicitId",
                table: "TipuriMaterial",
                column: "ContImplicitId");

            migrationBuilder.CreateIndex(
                name: "IX_TipuriTva_Cod",
                table: "TipuriTva",
                column: "Cod",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TipuriTva_ContTvaColectatId",
                table: "TipuriTva",
                column: "ContTvaColectatId");

            migrationBuilder.CreateIndex(
                name: "IX_TipuriTva_ContTvaDeductibilId",
                table: "TipuriTva",
                column: "ContTvaDeductibilId");

            migrationBuilder.CreateIndex(
                name: "IX_TipuriTva_ContTvaNeexigibilId",
                table: "TipuriTva",
                column: "ContTvaNeexigibilId");

            migrationBuilder.CreateIndex(
                name: "IX_Tranzactie_DocumentId",
                table: "Tranzactie",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_Tranzactie_ExplicatieDinId",
                table: "Tranzactie",
                column: "ExplicatieDinId");

            migrationBuilder.CreateIndex(
                name: "IX_Tranzactie_Fel",
                table: "Tranzactie",
                column: "Fel",
                unique: true,
                filter: "\"Fel\" = 4");

            migrationBuilder.CreateIndex(
                name: "IX_UnitatiMasura_Cod",
                table: "UnitatiMasura",
                column: "Cod",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_StateMachineAppearances_StateMachineStates_StateID",
                table: "StateMachineAppearances",
                column: "StateID",
                principalTable: "StateMachineStates",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StateMachines_StateMachineStates_StartStateID",
                table: "StateMachines",
                column: "StartStateID",
                principalTable: "StateMachineStates",
                principalColumn: "ID");

            // S-D2: Postare partiționată pe Spatiu, cheia (Spatiu, ID), FK-urile și indecșii pe partiții (S-r4).
            migrationBuilder.Sql("""
                CREATE TABLE "Postare" (
                    "ID" uuid NOT NULL,
                    "Spatiu" smallint NOT NULL,
                    "TranzactieId" uuid NOT NULL,
                    "DocumentId" uuid NULL,
                    "LinieId" uuid NULL,
                    "Data" date NOT NULL,
                    "Cont" uuid NOT NULL,
                    "Latura" smallint NOT NULL,
                    "Partener" uuid NULL,
                    "Gestiune" uuid NULL,
                    "Produs" uuid NULL,
                    "Unitate" uuid NULL,
                    "UnitateDeschisa" date NULL,
                    "FelUnitate" smallint NULL,
                    "SuportId" uuid NULL,
                    "SuportSpatiu" smallint NULL,
                    "InversaDinId" uuid NULL,
                    "InversaDinSpatiu" smallint NULL,
                    "TipTvaId" uuid NULL,
                    "SensTva" smallint NULL,
                    "RolTva" smallint NULL,
                    "PerioadaDeclarare" integer NULL,
                    "Valuta" uuid NULL,
                    "Carte" smallint NOT NULL,
                    "CodFunctional" uuid NULL,
                    "CodEconomic" uuid NULL,
                    "SursaFinantare" uuid NULL,
                    "UnitateOrganizatorica" uuid NULL,
                    "Proiect" uuid NULL,
                    "CentruCost" uuid NULL,
                    "Atribuit" uuid NULL,
                    "Cantitate" numeric(18,3) NOT NULL,
                    "ValoareValuta" numeric(18,2) NOT NULL,
                    "Valoare" numeric(18,2) NOT NULL,
                    "CotaTva" numeric(18,4) NULL,
                    "DataDocument" date NULL,
                    "DataExigibilitate" date NULL,
                    "DataInregistrare" date NULL,
                    "DataPrimire" date NULL,
                    "DeImport" boolean NULL,
                    "DocumentFiscalId" uuid NULL,
                    "InversaTehnica" boolean NOT NULL DEFAULT false,
                    "PerioadaD394" integer NULL,
                    "RegimTva" smallint NULL,
                    "RegularizareD300" boolean NOT NULL DEFAULT false,
                    CONSTRAINT "PK_Postare" PRIMARY KEY ("Spatiu", "ID"),
                    CONSTRAINT "CK_Postare_FelUnitate" CHECK (
                        ("Unitate" IS NULL AND "FelUnitate" IS NULL AND "UnitateDeschisa" IS NULL)
                        OR ("Unitate" IS NOT NULL AND "UnitateDeschisa" IS NOT NULL AND "FelUnitate" IS NOT NULL
                            AND (("Spatiu" = 2 AND "FelUnitate" = 1)
                                OR ("Spatiu" = 1 AND "FelUnitate" IN (2, 3))))),
                    CONSTRAINT "CK_Postare_Referinte" CHECK (
                        (("InversaDinId" IS NULL AND "InversaDinSpatiu" IS NULL)
                            OR ("InversaDinId" IS NOT NULL AND "InversaDinSpatiu" IS NOT NULL AND "InversaDinSpatiu" IN (1, 2)))
                        AND (("SuportId" IS NULL AND "SuportSpatiu" IS NULL)
                            OR ("SuportId" IS NOT NULL AND "SuportSpatiu" IS NOT NULL AND "SuportSpatiu" IN (1, 2)))),
                    CONSTRAINT "CK_Postare_FiscalComplet" CHECK (
                        "TipTvaId" IS NULL OR (
                            "DocumentId" IS NOT NULL AND "LinieId" IS NOT NULL AND
                            "SensTva" IS NOT NULL AND "RolTva" IS NOT NULL AND
                            "RegimTva" IS NOT NULL AND "CotaTva" IS NOT NULL AND "DeImport" IS NOT NULL AND
                            "DocumentFiscalId" IS NOT NULL AND "DataDocument" IS NOT NULL AND
                            "DataExigibilitate" IS NOT NULL AND "DataInregistrare" IS NOT NULL AND
                            "PerioadaDeclarare" IS NOT NULL AND "PerioadaD394" IS NOT NULL AND
                            ("SensTva" <> 1 OR "DataPrimire" IS NOT NULL)))
                ) PARTITION BY LIST ("Spatiu");
                CREATE TABLE "Postare_Contabil" PARTITION OF "Postare" FOR VALUES IN (1);
                CREATE TABLE "Postare_Stoc" PARTITION OF "Postare" FOR VALUES IN (2);
                ALTER TABLE "Postare_Contabil"
                    ADD CONSTRAINT "FK_Postare_Contabil_Tranzactie_TranzactieId"
                        FOREIGN KEY ("TranzactieId") REFERENCES "Tranzactie" ("ID"),
                    ADD CONSTRAINT "FK_Postare_Contabil_Documente_DocumentId"
                        FOREIGN KEY ("DocumentId") REFERENCES "Documente" ("ID"),
                    ADD CONSTRAINT "FK_Postare_Contabil_Conturi_Cont"
                        FOREIGN KEY ("Cont") REFERENCES "Conturi" ("ID"),
                    ADD CONSTRAINT "FK_Postare_Contabil_Repartitori_Partener"
                        FOREIGN KEY ("Partener") REFERENCES "Repartitori" ("ID"),
                    ADD CONSTRAINT "FK_Postare_Contabil_Produse_Produs"
                        FOREIGN KEY ("Produs") REFERENCES "Produse" ("ID");
                ALTER TABLE "Postare_Stoc"
                    ADD CONSTRAINT "FK_Postare_Stoc_Tranzactie_TranzactieId"
                        FOREIGN KEY ("TranzactieId") REFERENCES "Tranzactie" ("ID"),
                    ADD CONSTRAINT "FK_Postare_Stoc_Documente_DocumentId"
                        FOREIGN KEY ("DocumentId") REFERENCES "Documente" ("ID"),
                    ADD CONSTRAINT "FK_Postare_Stoc_Conturi_Cont"
                        FOREIGN KEY ("Cont") REFERENCES "Conturi" ("ID"),
                    ADD CONSTRAINT "FK_Postare_Stoc_Repartitori_Partener"
                        FOREIGN KEY ("Partener") REFERENCES "Repartitori" ("ID"),
                    ADD CONSTRAINT "FK_Postare_Stoc_Produse_Produs"
                        FOREIGN KEY ("Produs") REFERENCES "Produse" ("ID"),
                    ADD CONSTRAINT "FK_Postare_Stoc_Loturi_Unitate"
                        FOREIGN KEY ("Unitate") REFERENCES "Loturi" ("ID");
                CREATE INDEX "IX_Postare_Stoc_Produs_Data" ON "Postare_Stoc" ("Produs", "Data")
                    INCLUDE ("Cantitate", "Valoare", "Gestiune", "Unitate");
                CREATE INDEX "IX_Postare_Stoc_Data" ON "Postare_Stoc" ("Data");
                CREATE INDEX "IX_Postare_Stoc_PerioadaDeclarare_TipTvaId" ON "Postare_Stoc"
                    ("PerioadaDeclarare", "TipTvaId") WHERE "PerioadaDeclarare" IS NOT NULL;
                CREATE INDEX "IX_Postare_Contabil_Partener_Cont_Data" ON "Postare_Contabil"
                    ("Partener", "Cont", "Data") WHERE "Partener" IS NOT NULL;
                CREATE INDEX "IX_Postare_Contabil_Data" ON "Postare_Contabil" ("Data");
                CREATE INDEX "IX_Postare_Contabil_PerioadaDeclarare_TipTvaId" ON "Postare_Contabil"
                    ("PerioadaDeclarare", "TipTvaId") WHERE "PerioadaDeclarare" IS NOT NULL;
                CREATE INDEX "IX_Postare_DocumentId" ON "Postare" ("DocumentId");
                CREATE INDEX "IX_Postare_TranzactieId" ON "Postare" ("TranzactieId");
                CREATE INDEX "IX_Postare_Suport" ON "Postare" ("SuportSpatiu", "SuportId") WHERE "SuportId" IS NOT NULL;
                CREATE INDEX "IX_Postare_InversaDin" ON "Postare" ("InversaDinSpatiu", "InversaDinId") WHERE "InversaDinId" IS NOT NULL;
                CREATE INDEX "IX_Postare_PerioadaD394" ON "Postare" ("PerioadaD394") WHERE "PerioadaD394" IS NOT NULL;
                CREATE INDEX "IX_Postare_DataExigibilitate" ON "Postare" ("DataExigibilitate") WHERE "DataExigibilitate" IS NOT NULL;
                CREATE INDEX "IX_Postare_Unitate_Data" ON "Postare" ("Unitate", "Data") WHERE "Unitate" IS NOT NULL;
                """);

            // 092a: identitatea partidei, aceeași cu N.Unitate.DeschidePartida (SC-CIT-53).
            migrationBuilder.Sql("""
                CREATE FUNCTION cub_partida_id(document uuid, cont uuid, partener uuid) RETURNS uuid
                LANGUAGE plpgsql IMMUTABLE STRICT PARALLEL SAFE AS $body$
                DECLARE element uuid; raw bytea; intrare bytea := ''::bytea;
                BEGIN
                    FOREACH element IN ARRAY ARRAY[document, cont, partener] LOOP
                        raw := uuid_send(element);
                        intrare := intrare || substring(raw FROM 4 FOR 1) || substring(raw FROM 3 FOR 1) || substring(raw FROM 2 FOR 1) || substring(raw FROM 1 FOR 1) || substring(raw FROM 6 FOR 1) || substring(raw FROM 5 FOR 1) || substring(raw FROM 8 FOR 1) || substring(raw FROM 7 FOR 1) || substring(raw FROM 9 FOR 1) || substring(raw FROM 10 FOR 1) || substring(raw FROM 11 FOR 1) || substring(raw FROM 12 FOR 1) || substring(raw FROM 13 FOR 1) || substring(raw FROM 14 FOR 1) || substring(raw FROM 15 FOR 1) || substring(raw FROM 16 FOR 1);
                    END LOOP;
                    raw := substring(sha256(intrare) FROM 1 FOR 16);
                    raw := set_byte(raw, 7, (get_byte(raw, 7) & 15) | 128);
                    RETURN encode(substring(raw FROM 4 FOR 1) || substring(raw FROM 3 FOR 1) || substring(raw FROM 2 FOR 1) || substring(raw FROM 1 FOR 1) || substring(raw FROM 6 FOR 1) || substring(raw FROM 5 FOR 1) || substring(raw FROM 8 FOR 1) || substring(raw FROM 7 FOR 1) || substring(raw FROM 9 FOR 1) || substring(raw FROM 10 FOR 1) || substring(raw FROM 11 FOR 1) || substring(raw FROM 12 FOR 1) || substring(raw FROM 13 FOR 1) || substring(raw FROM 14 FOR 1) || substring(raw FROM 15 FOR 1) || substring(raw FROM 16 FOR 1), 'hex')::uuid;
                END
                $body$;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP FUNCTION cub_partida_id(uuid, uuid, uuid);
                DROP TABLE "Postare";
                """);

            migrationBuilder.DropForeignKey(
                name: "FK_StateMachines_StateMachineStates_StartStateID",
                table: "StateMachines");

            migrationBuilder.DropTable(
                name: "AuditData");

            migrationBuilder.DropTable(
                name: "DashboardData");

            migrationBuilder.DropTable(
                name: "DepuneriDeclaratii");

            migrationBuilder.DropTable(
                name: "DocumentDetalii");

            migrationBuilder.DropTable(
                name: "DviFacturi");

            migrationBuilder.DropTable(
                name: "EventResource");

            migrationBuilder.DropTable(
                name: "FileData");

            migrationBuilder.DropTable(
                name: "Imperecheri");

            migrationBuilder.DropTable(
                name: "InchideriPerioade");

            migrationBuilder.DropTable(
                name: "MapariD300");

            migrationBuilder.DropTable(
                name: "MapariD394");

            migrationBuilder.DropTable(
                name: "MapariTvaSaft");

            migrationBuilder.DropTable(
                name: "MigrareLegaturi");

            migrationBuilder.DropTable(
                name: "ModelDifferenceAspects");

            migrationBuilder.DropTable(
                name: "PartideDeschise");

            migrationBuilder.DropTable(
                name: "PermissionPolicyActionPermissionObject");

            migrationBuilder.DropTable(
                name: "PermissionPolicyMemberPermissionsObject");

            migrationBuilder.DropTable(
                name: "PermissionPolicyNavigationPermissionObject");

            migrationBuilder.DropTable(
                name: "PermissionPolicyObjectPermissionsObject");

            migrationBuilder.DropTable(
                name: "PermissionPolicyRolePermissionPolicyUser");

            migrationBuilder.DropTable(
                name: "PermissionPolicyUserLoginInfo");

            migrationBuilder.DropTable(
                name: "PoliticiAmortizare");

            migrationBuilder.DropTable(
                name: "PoliticiConex");

            migrationBuilder.DropTable(
                name: "PoliticiDiferenta");

            migrationBuilder.DropTable(
                name: "PoliticiInchidere");

            migrationBuilder.DropTable(
                name: "PoliticiInchidereTva");

            migrationBuilder.DropTable(
                name: "PoliticiMiscareSaft");

            migrationBuilder.DropTable(
                name: "PoliticiNumerotare");

            migrationBuilder.DropTable(
                name: "PoliticiScadenta");

            migrationBuilder.DropTable(
                name: "PoliticiTva");

            migrationBuilder.DropTable(
                name: "PoliticiTvaImplicit");

            migrationBuilder.DropTable(
                name: "PoliticiValidare");

            migrationBuilder.DropTable(
                name: "RefuzuriSeed");

            migrationBuilder.DropTable(
                name: "ReguliContare");

            migrationBuilder.DropTable(
                name: "ReguliDeductibilitate");

            migrationBuilder.DropTable(
                name: "ReguliStoc");

            migrationBuilder.DropTable(
                name: "ReportDataV2");

            migrationBuilder.DropTable(
                name: "SetariProfil");

            migrationBuilder.DropTable(
                name: "Societati");

            migrationBuilder.DropTable(
                name: "SolduriPerioadaContabil");

            migrationBuilder.DropTable(
                name: "SolduriPerioadaStoc");

            migrationBuilder.DropTable(
                name: "StateMachineAppearances");

            migrationBuilder.DropTable(
                name: "StateMachineTransitions");

            migrationBuilder.DropTable(
                name: "AuditEFCoreWeakReferences");

            migrationBuilder.DropTable(
                name: "Angajamente");

            migrationBuilder.DropTable(
                name: "Imobilizari");

            migrationBuilder.DropTable(
                name: "Events");

            migrationBuilder.DropTable(
                name: "Resource");

            migrationBuilder.DropTable(
                name: "PerioadeFiscale");

            migrationBuilder.DropTable(
                name: "RanduriD300");

            migrationBuilder.DropTable(
                name: "ModelDifferences");

            migrationBuilder.DropTable(
                name: "PermissionPolicyTypePermissionObject");

            migrationBuilder.DropTable(
                name: "PermissionPolicyUser");

            migrationBuilder.DropTable(
                name: "Tranzactie");

            migrationBuilder.DropTable(
                name: "TipuriDocument");

            migrationBuilder.DropTable(
                name: "CoduriFunctionale");

            migrationBuilder.DropTable(
                name: "Proiecte");

            migrationBuilder.DropTable(
                name: "SurseFinantare");

            migrationBuilder.DropTable(
                name: "Unitati");

            migrationBuilder.DropTable(
                name: "Loturi");

            migrationBuilder.DropTable(
                name: "ClasificariImobilizari");

            migrationBuilder.DropTable(
                name: "CoduriEconomice");

            migrationBuilder.DropTable(
                name: "PermissionPolicyRoleBase");

            migrationBuilder.DropTable(
                name: "Documente");

            migrationBuilder.DropTable(
                name: "Produse");

            migrationBuilder.DropTable(
                name: "Repartitori");

            migrationBuilder.DropTable(
                name: "TipuriMaterial");

            migrationBuilder.DropTable(
                name: "UnitatiMasura");

            migrationBuilder.DropTable(
                name: "Judete");

            migrationBuilder.DropTable(
                name: "TipuriTva");

            migrationBuilder.DropTable(
                name: "ClaseProduse");

            migrationBuilder.DropTable(
                name: "Conturi");

            migrationBuilder.DropTable(
                name: "StateMachineStates");

            migrationBuilder.DropTable(
                name: "StateMachines");
        }
    }
}
