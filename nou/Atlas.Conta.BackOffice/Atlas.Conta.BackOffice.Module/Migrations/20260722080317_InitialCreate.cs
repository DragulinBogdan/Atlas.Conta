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
                    Cod = table.Column<string>(type: "text", nullable: true),
                    Denumire = table.Column<string>(type: "text", nullable: true),
                    GCRecord = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    OptimisticLockField = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Angajamente", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "AuditEFCoreWeakReferences",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    GCRecord = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
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
                    Cod = table.Column<string>(type: "text", nullable: true),
                    Denumire = table.Column<string>(type: "text", nullable: true),
                    Natura = table.Column<int>(type: "integer", nullable: false),
                    GCRecord = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    OptimisticLockField = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClaseProduse", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "CoduriEconomice",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    Cod = table.Column<string>(type: "text", nullable: true),
                    Denumire = table.Column<string>(type: "text", nullable: true),
                    GCRecord = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    OptimisticLockField = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoduriEconomice", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "CoduriFunctionale",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    Cod = table.Column<string>(type: "text", nullable: true),
                    Denumire = table.Column<string>(type: "text", nullable: true),
                    GCRecord = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    OptimisticLockField = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoduriFunctionale", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Conturi",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    Simbol = table.Column<string>(type: "text", nullable: true),
                    Denumire = table.Column<string>(type: "text", nullable: true),
                    ParinteId = table.Column<Guid>(type: "uuid", nullable: true),
                    Functie = table.Column<string>(type: "text", nullable: true),
                    Sumator = table.Column<bool>(type: "boolean", nullable: false),
                    DimensiuniObligatorii = table.Column<int>(type: "integer", nullable: false),
                    GCRecord = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    OptimisticLockField = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Conturi", x => x.ID);
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
                    GCRecord = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    OptimisticLockField = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DashboardData", x => x.ID);
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
                    GCRecord = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
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
                    GCRecord = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    OptimisticLockField = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileData", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "HCategories",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    ParentID = table.Column<Guid>(type: "uuid", nullable: true),
                    GCRecord = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    OptimisticLockField = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HCategories", x => x.ID);
                    table.ForeignKey(
                        name: "FK_HCategories_HCategories_ParentID",
                        column: x => x.ParentID,
                        principalTable: "HCategories",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "ModelDifferences",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: true),
                    ContextId = table.Column<string>(type: "text", nullable: true),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    GCRecord = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
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
                    GCRecord = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
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
                    GCRecord = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
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
                    GCRecord = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    OptimisticLockField = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PermissionPolicyUser", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Proiecte",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    Cod = table.Column<string>(type: "text", nullable: true),
                    Denumire = table.Column<string>(type: "text", nullable: true),
                    GCRecord = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    OptimisticLockField = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Proiecte", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Repartitori",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    Cod = table.Column<string>(type: "text", nullable: true),
                    Denumire = table.Column<string>(type: "text", nullable: true),
                    Calitati = table.Column<int>(type: "integer", nullable: false),
                    Activ = table.Column<bool>(type: "boolean", nullable: false),
                    GCRecord = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    OptimisticLockField = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Repartitori", x => x.ID);
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
                    GCRecord = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
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
                name: "SurseFinantare",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    Cod = table.Column<string>(type: "text", nullable: true),
                    Denumire = table.Column<string>(type: "text", nullable: true),
                    GCRecord = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    OptimisticLockField = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SurseFinantare", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "TipuriDocument",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    Cod = table.Column<string>(type: "text", nullable: true),
                    Denumire = table.Column<string>(type: "text", nullable: true),
                    ClrType = table.Column<string>(type: "text", nullable: true),
                    GCRecord = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    OptimisticLockField = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TipuriDocument", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Unitati",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    Cod = table.Column<string>(type: "text", nullable: true),
                    Denumire = table.Column<string>(type: "text", nullable: true),
                    GCRecord = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    OptimisticLockField = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Unitati", x => x.ID);
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
                name: "ModelDifferenceAspects",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Xml = table.Column<string>(type: "text", nullable: true),
                    OwnerID = table.Column<Guid>(type: "uuid", nullable: true),
                    GCRecord = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
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
                name: "PermissionPolicyActionPermissionObject",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleID = table.Column<Guid>(type: "uuid", nullable: true),
                    ActionId = table.Column<string>(type: "text", nullable: true),
                    GCRecord = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
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
                    GCRecord = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
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
                    GCRecord = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
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
                    GCRecord = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
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
                name: "Angajati",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    Marca = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Angajati", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Angajati_Repartitori_ID",
                        column: x => x.ID,
                        principalTable: "Repartitori",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ConturiProprii",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    Iban = table.Column<string>(type: "text", nullable: true),
                    EsteBanca = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConturiProprii", x => x.ID);
                    table.ForeignKey(
                        name: "FK_ConturiProprii_Repartitori_ID",
                        column: x => x.ID,
                        principalTable: "Repartitori",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Documente",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    Numar = table.Column<string>(type: "text", nullable: true),
                    Data = table.Column<DateOnly>(type: "date", nullable: false),
                    PredatorId = table.Column<Guid>(type: "uuid", nullable: false),
                    PrimitorId = table.Column<Guid>(type: "uuid", nullable: false),
                    Stare = table.Column<int>(type: "integer", nullable: false),
                    DataOperare = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DocumentSursaId = table.Column<Guid>(type: "uuid", nullable: true),
                    Autogenerat = table.Column<bool>(type: "boolean", nullable: false),
                    GCRecord = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    OptimisticLockField = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Documente", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Documente_Documente_DocumentSursaId",
                        column: x => x.DocumentSursaId,
                        principalTable: "Documente",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_Documente_Repartitori_PredatorId",
                        column: x => x.PredatorId,
                        principalTable: "Repartitori",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Documente_Repartitori_PrimitorId",
                        column: x => x.PrimitorId,
                        principalTable: "Repartitori",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Gestiuni",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Gestiuni", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Gestiuni_Repartitori_ID",
                        column: x => x.ID,
                        principalTable: "Repartitori",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Parteneri",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    CodFiscal = table.Column<string>(type: "text", nullable: true),
                    RegistruComert = table.Column<string>(type: "text", nullable: true),
                    ContImplicitId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Parteneri", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Parteneri_Conturi_ContImplicitId",
                        column: x => x.ContImplicitId,
                        principalTable: "Conturi",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_Parteneri_Repartitori_ID",
                        column: x => x.ID,
                        principalTable: "Repartitori",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UnitatiInterne",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnitatiInterne", x => x.ID);
                    table.ForeignKey(
                        name: "FK_UnitatiInterne_Repartitori_ID",
                        column: x => x.ID,
                        principalTable: "Repartitori",
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
                name: "PoliticiConex",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    TipDocumentSursaId = table.Column<Guid>(type: "uuid", nullable: false),
                    TipDocumentTintaId = table.Column<Guid>(type: "uuid", nullable: false),
                    InverseazaLaturi = table.Column<bool>(type: "boolean", nullable: false),
                    GCRecord = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    OptimisticLockField = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PoliticiConex", x => x.ID);
                    table.ForeignKey(
                        name: "FK_PoliticiConex_TipuriDocument_TipDocumentSursaId",
                        column: x => x.TipDocumentSursaId,
                        principalTable: "TipuriDocument",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PoliticiConex_TipuriDocument_TipDocumentTintaId",
                        column: x => x.TipDocumentTintaId,
                        principalTable: "TipuriDocument",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PoliticiNumerotare",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    TipDocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Serie = table.Column<string>(type: "text", nullable: true),
                    UrmatorulNumar = table.Column<int>(type: "integer", nullable: false),
                    Format = table.Column<string>(type: "text", nullable: true),
                    GCRecord = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    OptimisticLockField = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PoliticiNumerotare", x => x.ID);
                    table.ForeignKey(
                        name: "FK_PoliticiNumerotare_TipuriDocument_TipDocumentId",
                        column: x => x.TipDocumentId,
                        principalTable: "TipuriDocument",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReguliStoc",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    TipDocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Latura = table.Column<int>(type: "integer", nullable: false),
                    ClasaId = table.Column<Guid>(type: "uuid", nullable: true),
                    TipStoc = table.Column<int>(type: "integer", nullable: false),
                    Semn = table.Column<int>(type: "integer", nullable: false),
                    GCRecord = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
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
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
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
                    GCRecord = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
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
                    GCRecord = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
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
                name: "BonuriConsum",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BonuriConsum", x => x.ID);
                    table.ForeignKey(
                        name: "FK_BonuriConsum_Documente_ID",
                        column: x => x.ID,
                        principalTable: "Documente",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Deconturi",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    NumarPV = table.Column<string>(type: "text", nullable: true),
                    DataPV = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Deconturi", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Deconturi_Documente_ID",
                        column: x => x.ID,
                        principalTable: "Documente",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DocumentTrezorerie",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    TipInstrument = table.Column<int>(type: "integer", nullable: false),
                    NumarExtras = table.Column<string>(type: "text", nullable: true),
                    DataExtras = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentTrezorerie", x => x.ID);
                    table.ForeignKey(
                        name: "FK_DocumentTrezorerie_Documente_ID",
                        column: x => x.ID,
                        principalTable: "Documente",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FacturiIesire",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    DataScadenta = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FacturiIesire", x => x.ID);
                    table.ForeignKey(
                        name: "FK_FacturiIesire_Documente_ID",
                        column: x => x.ID,
                        principalTable: "Documente",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FacturiIntrare",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    DataScadenta = table.Column<DateOnly>(type: "date", nullable: true),
                    NumarPV = table.Column<string>(type: "text", nullable: true),
                    DataPV = table.Column<DateOnly>(type: "date", nullable: true),
                    CodCpv = table.Column<string>(type: "text", nullable: true),
                    TethysId = table.Column<string>(type: "text", nullable: true),
                    Valuta = table.Column<string>(type: "text", nullable: true),
                    Curs = table.Column<decimal>(type: "numeric", nullable: true),
                    GenereazaPlata = table.Column<bool>(type: "boolean", nullable: false),
                    PlataContPropriuId = table.Column<Guid>(type: "uuid", nullable: true),
                    PlataNumar = table.Column<string>(type: "text", nullable: true),
                    PlataData = table.Column<DateOnly>(type: "date", nullable: true),
                    PlataTipInstrument = table.Column<int>(type: "integer", nullable: true),
                    GenereazaChitanta = table.Column<bool>(type: "boolean", nullable: false),
                    ChitantaNumar = table.Column<string>(type: "text", nullable: true),
                    ChitantaData = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FacturiIntrare", x => x.ID);
                    table.ForeignKey(
                        name: "FK_FacturiIntrare_ConturiProprii_PlataContPropriuId",
                        column: x => x.PlataContPropriuId,
                        principalTable: "ConturiProprii",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_FacturiIntrare_Documente_ID",
                        column: x => x.ID,
                        principalTable: "Documente",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ListeDiferenteInventar",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ListeDiferenteInventar", x => x.ID);
                    table.ForeignKey(
                        name: "FK_ListeDiferenteInventar_Documente_ID",
                        column: x => x.ID,
                        principalTable: "Documente",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NIRuri",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NIRuri", x => x.ID);
                    table.ForeignKey(
                        name: "FK_NIRuri_Documente_ID",
                        column: x => x.ID,
                        principalTable: "Documente",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NoteTransfer",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    NumarPV = table.Column<string>(type: "text", nullable: true),
                    DataPV = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NoteTransfer", x => x.ID);
                    table.ForeignKey(
                        name: "FK_NoteTransfer_Documente_ID",
                        column: x => x.ID,
                        principalTable: "Documente",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RapoarteProductie",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RapoarteProductie", x => x.ID);
                    table.ForeignKey(
                        name: "FK_RapoarteProductie_Documente_ID",
                        column: x => x.ID,
                        principalTable: "Documente",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TipuriMaterial",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    Cod = table.Column<string>(type: "text", nullable: true),
                    Denumire = table.Column<string>(type: "text", nullable: true),
                    ClasaId = table.Column<Guid>(type: "uuid", nullable: false),
                    PoliticaConexID = table.Column<Guid>(type: "uuid", nullable: true),
                    GCRecord = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    OptimisticLockField = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TipuriMaterial", x => x.ID);
                    table.ForeignKey(
                        name: "FK_TipuriMaterial_ClaseProduse_ClasaId",
                        column: x => x.ClasaId,
                        principalTable: "ClaseProduse",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TipuriMaterial_PoliticiConex_PoliticaConexID",
                        column: x => x.PoliticaConexID,
                        principalTable: "PoliticiConex",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "Imperecheri",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentTrezorerieId = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Suma = table.Column<decimal>(type: "numeric", nullable: false),
                    GCRecord = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    OptimisticLockField = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Imperecheri", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Imperecheri_DocumentTrezorerie_DocumentTrezorerieId",
                        column: x => x.DocumentTrezorerieId,
                        principalTable: "DocumentTrezorerie",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Imperecheri_Documente_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "Documente",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Incasari",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Incasari", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Incasari_DocumentTrezorerie_ID",
                        column: x => x.ID,
                        principalTable: "DocumentTrezorerie",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Plati",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Plati", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Plati_DocumentTrezorerie_ID",
                        column: x => x.ID,
                        principalTable: "DocumentTrezorerie",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Produse",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    Cod = table.Column<string>(type: "text", nullable: true),
                    Denumire = table.Column<string>(type: "text", nullable: true),
                    UM = table.Column<string>(type: "text", nullable: true),
                    TipMaterialId = table.Column<Guid>(type: "uuid", nullable: true),
                    GCRecord = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    OptimisticLockField = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Produse", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Produse_TipuriMaterial_TipMaterialId",
                        column: x => x.TipMaterialId,
                        principalTable: "TipuriMaterial",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "ReguliContare",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    TipDocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    TipMaterialId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    GCRecord = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
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
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
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
                name: "DecontDetalii",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    Descriere = table.Column<string>(type: "text", nullable: true),
                    PretUnitar = table.Column<decimal>(type: "numeric", nullable: false),
                    CotaTva = table.Column<decimal>(type: "numeric", nullable: false),
                    ContDebitId = table.Column<Guid>(type: "uuid", nullable: true),
                    ContCreditId = table.Column<Guid>(type: "uuid", nullable: true),
                    RepartitorDebitId = table.Column<Guid>(type: "uuid", nullable: true),
                    RepartitorCreditId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DecontDetalii", x => x.ID);
                    table.ForeignKey(
                        name: "FK_DecontDetalii_Conturi_ContCreditId",
                        column: x => x.ContCreditId,
                        principalTable: "Conturi",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_DecontDetalii_Conturi_ContDebitId",
                        column: x => x.ContDebitId,
                        principalTable: "Conturi",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_DecontDetalii_Repartitori_RepartitorCreditId",
                        column: x => x.RepartitorCreditId,
                        principalTable: "Repartitori",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_DecontDetalii_Repartitori_RepartitorDebitId",
                        column: x => x.RepartitorDebitId,
                        principalTable: "Repartitori",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "DocumentDetalii",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    TipMaterialId = table.Column<Guid>(type: "uuid", nullable: false),
                    LotId = table.Column<Guid>(type: "uuid", nullable: true),
                    Cantitate = table.Column<decimal>(type: "numeric", nullable: false),
                    Valoare = table.Column<decimal>(type: "numeric", nullable: false),
                    AngajamentId = table.Column<Guid>(type: "uuid", nullable: true),
                    Dimensiuni_RepartitorId = table.Column<Guid>(type: "uuid", nullable: true),
                    Dimensiuni_MaterialId = table.Column<Guid>(type: "uuid", nullable: true),
                    Dimensiuni_CodFunctionalId = table.Column<Guid>(type: "uuid", nullable: true),
                    Dimensiuni_CodEconomicId = table.Column<Guid>(type: "uuid", nullable: true),
                    Dimensiuni_SursaFinantareId = table.Column<Guid>(type: "uuid", nullable: true),
                    Dimensiuni_UnitateId = table.Column<Guid>(type: "uuid", nullable: true),
                    Dimensiuni_ProiectId = table.Column<Guid>(type: "uuid", nullable: true),
                    Dimensiuni_CentruCostId = table.Column<Guid>(type: "uuid", nullable: true),
                    GCRecord = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    OptimisticLockField = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentDetalii", x => x.ID);
                    table.ForeignKey(
                        name: "FK_DocumentDetalii_Angajamente_AngajamentId",
                        column: x => x.AngajamentId,
                        principalTable: "Angajamente",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_DocumentDetalii_CoduriEconomice_Dimensiuni_CodEconomicId",
                        column: x => x.Dimensiuni_CodEconomicId,
                        principalTable: "CoduriEconomice",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_DocumentDetalii_CoduriFunctionale_Dimensiuni_CodFunctionalId",
                        column: x => x.Dimensiuni_CodFunctionalId,
                        principalTable: "CoduriFunctionale",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_DocumentDetalii_Documente_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "Documente",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DocumentDetalii_Produse_Dimensiuni_MaterialId",
                        column: x => x.Dimensiuni_MaterialId,
                        principalTable: "Produse",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_DocumentDetalii_Proiecte_Dimensiuni_ProiectId",
                        column: x => x.Dimensiuni_ProiectId,
                        principalTable: "Proiecte",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_DocumentDetalii_Repartitori_Dimensiuni_CentruCostId",
                        column: x => x.Dimensiuni_CentruCostId,
                        principalTable: "Repartitori",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_DocumentDetalii_Repartitori_Dimensiuni_RepartitorId",
                        column: x => x.Dimensiuni_RepartitorId,
                        principalTable: "Repartitori",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_DocumentDetalii_SurseFinantare_Dimensiuni_SursaFinantareId",
                        column: x => x.Dimensiuni_SursaFinantareId,
                        principalTable: "SurseFinantare",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_DocumentDetalii_TipuriMaterial_TipMaterialId",
                        column: x => x.TipMaterialId,
                        principalTable: "TipuriMaterial",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DocumentDetalii_Unitati_Dimensiuni_UnitateId",
                        column: x => x.Dimensiuni_UnitateId,
                        principalTable: "Unitati",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "FacturiIesireDetalii",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    Descriere = table.Column<string>(type: "text", nullable: true),
                    PretUnitar = table.Column<decimal>(type: "numeric", nullable: false),
                    CotaTva = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FacturiIesireDetalii", x => x.ID);
                    table.ForeignKey(
                        name: "FK_FacturiIesireDetalii_DocumentDetalii_ID",
                        column: x => x.ID,
                        principalTable: "DocumentDetalii",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FacturiIntrareDetalii",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    PretUnitar = table.Column<decimal>(type: "numeric", nullable: false),
                    CotaTva = table.Column<decimal>(type: "numeric", nullable: false),
                    CodCpv = table.Column<string>(type: "text", nullable: true),
                    DataExpirare = table.Column<DateOnly>(type: "date", nullable: true),
                    LotFabricatie = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FacturiIntrareDetalii", x => x.ID);
                    table.ForeignKey(
                        name: "FK_FacturiIntrareDetalii_DocumentDetalii_ID",
                        column: x => x.ID,
                        principalTable: "DocumentDetalii",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ListeDiferenteInventarDetalii",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    Directie = table.Column<int>(type: "integer", nullable: false),
                    PretEvaluare = table.Column<decimal>(type: "numeric", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ListeDiferenteInventarDetalii", x => x.ID);
                    table.ForeignKey(
                        name: "FK_ListeDiferenteInventarDetalii_DocumentDetalii_ID",
                        column: x => x.ID,
                        principalTable: "DocumentDetalii",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Loturi",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    ProdusId = table.Column<Guid>(type: "uuid", nullable: false),
                    PretUnitar = table.Column<decimal>(type: "numeric", nullable: false),
                    GestiuneId = table.Column<Guid>(type: "uuid", nullable: false),
                    Data = table.Column<DateOnly>(type: "date", nullable: false),
                    DataExpirare = table.Column<DateOnly>(type: "date", nullable: true),
                    LotFabricatie = table.Column<string>(type: "text", nullable: true),
                    LinieIntrareId = table.Column<Guid>(type: "uuid", nullable: true),
                    GCRecord = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    OptimisticLockField = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Loturi", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Loturi_DocumentDetalii_LinieIntrareId",
                        column: x => x.LinieIntrareId,
                        principalTable: "DocumentDetalii",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_Loturi_Gestiuni_GestiuneId",
                        column: x => x.GestiuneId,
                        principalTable: "Gestiuni",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Loturi_Produse_ProdusId",
                        column: x => x.ProdusId,
                        principalTable: "Produse",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RegistruContabil",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    Data = table.Column<DateOnly>(type: "date", nullable: false),
                    NumarNota = table.Column<string>(type: "text", nullable: true),
                    ContDebitId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContCreditId = table.Column<Guid>(type: "uuid", nullable: false),
                    RepartitorDebitId = table.Column<Guid>(type: "uuid", nullable: true),
                    RepartitorCreditId = table.Column<Guid>(type: "uuid", nullable: true),
                    Valoare = table.Column<decimal>(type: "numeric", nullable: false),
                    Dimensiuni_RepartitorId = table.Column<Guid>(type: "uuid", nullable: true),
                    Dimensiuni_MaterialId = table.Column<Guid>(type: "uuid", nullable: true),
                    Dimensiuni_CodFunctionalId = table.Column<Guid>(type: "uuid", nullable: true),
                    Dimensiuni_CodEconomicId = table.Column<Guid>(type: "uuid", nullable: true),
                    Dimensiuni_SursaFinantareId = table.Column<Guid>(type: "uuid", nullable: true),
                    Dimensiuni_UnitateId = table.Column<Guid>(type: "uuid", nullable: true),
                    Dimensiuni_ProiectId = table.Column<Guid>(type: "uuid", nullable: true),
                    Dimensiuni_CentruCostId = table.Column<Guid>(type: "uuid", nullable: true),
                    DocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    DetaliuId = table.Column<Guid>(type: "uuid", nullable: true),
                    GCRecord = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    OptimisticLockField = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegistruContabil", x => x.ID);
                    table.ForeignKey(
                        name: "FK_RegistruContabil_CoduriEconomice_Dimensiuni_CodEconomicId",
                        column: x => x.Dimensiuni_CodEconomicId,
                        principalTable: "CoduriEconomice",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_RegistruContabil_CoduriFunctionale_Dimensiuni_CodFunctional~",
                        column: x => x.Dimensiuni_CodFunctionalId,
                        principalTable: "CoduriFunctionale",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_RegistruContabil_Conturi_ContCreditId",
                        column: x => x.ContCreditId,
                        principalTable: "Conturi",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RegistruContabil_Conturi_ContDebitId",
                        column: x => x.ContDebitId,
                        principalTable: "Conturi",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RegistruContabil_DocumentDetalii_DetaliuId",
                        column: x => x.DetaliuId,
                        principalTable: "DocumentDetalii",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_RegistruContabil_Documente_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "Documente",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RegistruContabil_Produse_Dimensiuni_MaterialId",
                        column: x => x.Dimensiuni_MaterialId,
                        principalTable: "Produse",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_RegistruContabil_Proiecte_Dimensiuni_ProiectId",
                        column: x => x.Dimensiuni_ProiectId,
                        principalTable: "Proiecte",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_RegistruContabil_Repartitori_Dimensiuni_CentruCostId",
                        column: x => x.Dimensiuni_CentruCostId,
                        principalTable: "Repartitori",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_RegistruContabil_Repartitori_Dimensiuni_RepartitorId",
                        column: x => x.Dimensiuni_RepartitorId,
                        principalTable: "Repartitori",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_RegistruContabil_Repartitori_RepartitorCreditId",
                        column: x => x.RepartitorCreditId,
                        principalTable: "Repartitori",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_RegistruContabil_Repartitori_RepartitorDebitId",
                        column: x => x.RepartitorDebitId,
                        principalTable: "Repartitori",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_RegistruContabil_SurseFinantare_Dimensiuni_SursaFinantareId",
                        column: x => x.Dimensiuni_SursaFinantareId,
                        principalTable: "SurseFinantare",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_RegistruContabil_Unitati_Dimensiuni_UnitateId",
                        column: x => x.Dimensiuni_UnitateId,
                        principalTable: "Unitati",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "RegistruStoc",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    Data = table.Column<DateOnly>(type: "date", nullable: false),
                    TipStoc = table.Column<int>(type: "integer", nullable: false),
                    LotId = table.Column<Guid>(type: "uuid", nullable: false),
                    RepartitorId = table.Column<Guid>(type: "uuid", nullable: false),
                    Cantitate = table.Column<decimal>(type: "numeric", nullable: false),
                    Valoare = table.Column<decimal>(type: "numeric", nullable: false),
                    DocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    DetaliuId = table.Column<Guid>(type: "uuid", nullable: true),
                    GCRecord = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    OptimisticLockField = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegistruStoc", x => x.ID);
                    table.ForeignKey(
                        name: "FK_RegistruStoc_DocumentDetalii_DetaliuId",
                        column: x => x.DetaliuId,
                        principalTable: "DocumentDetalii",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_RegistruStoc_Documente_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "Documente",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RegistruStoc_Loturi_LotId",
                        column: x => x.LotId,
                        principalTable: "Loturi",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RegistruStoc_Repartitori_RepartitorId",
                        column: x => x.RepartitorId,
                        principalTable: "Repartitori",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
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
                    GCRecord = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
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
                    GCRecord = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
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
                    GCRecord = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
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
                name: "IX_Conturi_ParinteId",
                table: "Conturi",
                column: "ParinteId");

            migrationBuilder.CreateIndex(
                name: "IX_DecontDetalii_ContCreditId",
                table: "DecontDetalii",
                column: "ContCreditId");

            migrationBuilder.CreateIndex(
                name: "IX_DecontDetalii_ContDebitId",
                table: "DecontDetalii",
                column: "ContDebitId");

            migrationBuilder.CreateIndex(
                name: "IX_DecontDetalii_RepartitorCreditId",
                table: "DecontDetalii",
                column: "RepartitorCreditId");

            migrationBuilder.CreateIndex(
                name: "IX_DecontDetalii_RepartitorDebitId",
                table: "DecontDetalii",
                column: "RepartitorDebitId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentDetalii_AngajamentId",
                table: "DocumentDetalii",
                column: "AngajamentId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentDetalii_Dimensiuni_CentruCostId",
                table: "DocumentDetalii",
                column: "Dimensiuni_CentruCostId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentDetalii_Dimensiuni_CodEconomicId",
                table: "DocumentDetalii",
                column: "Dimensiuni_CodEconomicId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentDetalii_Dimensiuni_CodFunctionalId",
                table: "DocumentDetalii",
                column: "Dimensiuni_CodFunctionalId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentDetalii_Dimensiuni_MaterialId",
                table: "DocumentDetalii",
                column: "Dimensiuni_MaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentDetalii_Dimensiuni_ProiectId",
                table: "DocumentDetalii",
                column: "Dimensiuni_ProiectId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentDetalii_Dimensiuni_RepartitorId",
                table: "DocumentDetalii",
                column: "Dimensiuni_RepartitorId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentDetalii_Dimensiuni_SursaFinantareId",
                table: "DocumentDetalii",
                column: "Dimensiuni_SursaFinantareId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentDetalii_Dimensiuni_UnitateId",
                table: "DocumentDetalii",
                column: "Dimensiuni_UnitateId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentDetalii_DocumentId",
                table: "DocumentDetalii",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentDetalii_LotId",
                table: "DocumentDetalii",
                column: "LotId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentDetalii_TipMaterialId",
                table: "DocumentDetalii",
                column: "TipMaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_Documente_DocumentSursaId",
                table: "Documente",
                column: "DocumentSursaId");

            migrationBuilder.CreateIndex(
                name: "IX_Documente_PredatorId",
                table: "Documente",
                column: "PredatorId");

            migrationBuilder.CreateIndex(
                name: "IX_Documente_PrimitorId",
                table: "Documente",
                column: "PrimitorId");

            migrationBuilder.CreateIndex(
                name: "IX_EventResource_ResourcesKey",
                table: "EventResource",
                column: "ResourcesKey");

            migrationBuilder.CreateIndex(
                name: "IX_Events_RecurrencePatternID",
                table: "Events",
                column: "RecurrencePatternID");

            migrationBuilder.CreateIndex(
                name: "IX_FacturiIntrare_PlataContPropriuId",
                table: "FacturiIntrare",
                column: "PlataContPropriuId");

            migrationBuilder.CreateIndex(
                name: "IX_HCategories_ParentID",
                table: "HCategories",
                column: "ParentID");

            migrationBuilder.CreateIndex(
                name: "IX_Imperecheri_DocumentId",
                table: "Imperecheri",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_Imperecheri_DocumentTrezorerieId",
                table: "Imperecheri",
                column: "DocumentTrezorerieId");

            migrationBuilder.CreateIndex(
                name: "IX_Loturi_GestiuneId",
                table: "Loturi",
                column: "GestiuneId");

            migrationBuilder.CreateIndex(
                name: "IX_Loturi_LinieIntrareId",
                table: "Loturi",
                column: "LinieIntrareId");

            migrationBuilder.CreateIndex(
                name: "IX_Loturi_ProdusId",
                table: "Loturi",
                column: "ProdusId");

            migrationBuilder.CreateIndex(
                name: "IX_ModelDifferenceAspects_OwnerID",
                table: "ModelDifferenceAspects",
                column: "OwnerID");

            migrationBuilder.CreateIndex(
                name: "IX_Parteneri_ContImplicitId",
                table: "Parteneri",
                column: "ContImplicitId");

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
                name: "IX_PoliticiConex_TipDocumentSursaId",
                table: "PoliticiConex",
                column: "TipDocumentSursaId");

            migrationBuilder.CreateIndex(
                name: "IX_PoliticiConex_TipDocumentTintaId",
                table: "PoliticiConex",
                column: "TipDocumentTintaId");

            migrationBuilder.CreateIndex(
                name: "IX_PoliticiNumerotare_TipDocumentId",
                table: "PoliticiNumerotare",
                column: "TipDocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_Produse_TipMaterialId",
                table: "Produse",
                column: "TipMaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistruContabil_ContCreditId",
                table: "RegistruContabil",
                column: "ContCreditId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistruContabil_ContDebitId",
                table: "RegistruContabil",
                column: "ContDebitId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistruContabil_DetaliuId",
                table: "RegistruContabil",
                column: "DetaliuId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistruContabil_Dimensiuni_CentruCostId",
                table: "RegistruContabil",
                column: "Dimensiuni_CentruCostId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistruContabil_Dimensiuni_CodEconomicId",
                table: "RegistruContabil",
                column: "Dimensiuni_CodEconomicId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistruContabil_Dimensiuni_CodFunctionalId",
                table: "RegistruContabil",
                column: "Dimensiuni_CodFunctionalId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistruContabil_Dimensiuni_MaterialId",
                table: "RegistruContabil",
                column: "Dimensiuni_MaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistruContabil_Dimensiuni_ProiectId",
                table: "RegistruContabil",
                column: "Dimensiuni_ProiectId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistruContabil_Dimensiuni_RepartitorId",
                table: "RegistruContabil",
                column: "Dimensiuni_RepartitorId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistruContabil_Dimensiuni_SursaFinantareId",
                table: "RegistruContabil",
                column: "Dimensiuni_SursaFinantareId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistruContabil_Dimensiuni_UnitateId",
                table: "RegistruContabil",
                column: "Dimensiuni_UnitateId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistruContabil_DocumentId",
                table: "RegistruContabil",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistruContabil_RepartitorCreditId",
                table: "RegistruContabil",
                column: "RepartitorCreditId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistruContabil_RepartitorDebitId",
                table: "RegistruContabil",
                column: "RepartitorDebitId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistruStoc_DetaliuId",
                table: "RegistruStoc",
                column: "DetaliuId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistruStoc_DocumentId",
                table: "RegistruStoc",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistruStoc_LotId",
                table: "RegistruStoc",
                column: "LotId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistruStoc_RepartitorId",
                table: "RegistruStoc",
                column: "RepartitorId");

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
                name: "IX_ReguliContare_TipDocumentId",
                table: "ReguliContare",
                column: "TipDocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_ReguliContare_TipMaterialId",
                table: "ReguliContare",
                column: "TipMaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_ReguliStoc_ClasaId",
                table: "ReguliStoc",
                column: "ClasaId");

            migrationBuilder.CreateIndex(
                name: "IX_ReguliStoc_TipDocumentId",
                table: "ReguliStoc",
                column: "TipDocumentId");

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
                name: "IX_TipuriMaterial_ClasaId",
                table: "TipuriMaterial",
                column: "ClasaId");

            migrationBuilder.CreateIndex(
                name: "IX_TipuriMaterial_PoliticaConexID",
                table: "TipuriMaterial",
                column: "PoliticaConexID");

            migrationBuilder.AddForeignKey(
                name: "FK_DecontDetalii_DocumentDetalii_ID",
                table: "DecontDetalii",
                column: "ID",
                principalTable: "DocumentDetalii",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentDetalii_Loturi_LotId",
                table: "DocumentDetalii",
                column: "LotId",
                principalTable: "Loturi",
                principalColumn: "ID");

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DocumentDetalii_Repartitori_Dimensiuni_CentruCostId",
                table: "DocumentDetalii");

            migrationBuilder.DropForeignKey(
                name: "FK_DocumentDetalii_Repartitori_Dimensiuni_RepartitorId",
                table: "DocumentDetalii");

            migrationBuilder.DropForeignKey(
                name: "FK_Documente_Repartitori_PredatorId",
                table: "Documente");

            migrationBuilder.DropForeignKey(
                name: "FK_Documente_Repartitori_PrimitorId",
                table: "Documente");

            migrationBuilder.DropForeignKey(
                name: "FK_Gestiuni_Repartitori_ID",
                table: "Gestiuni");

            migrationBuilder.DropForeignKey(
                name: "FK_DocumentDetalii_Documente_DocumentId",
                table: "DocumentDetalii");

            migrationBuilder.DropForeignKey(
                name: "FK_Loturi_DocumentDetalii_LinieIntrareId",
                table: "Loturi");

            migrationBuilder.DropForeignKey(
                name: "FK_StateMachines_StateMachineStates_StartStateID",
                table: "StateMachines");

            migrationBuilder.DropTable(
                name: "Angajati");

            migrationBuilder.DropTable(
                name: "AuditData");

            migrationBuilder.DropTable(
                name: "BonuriConsum");

            migrationBuilder.DropTable(
                name: "DashboardData");

            migrationBuilder.DropTable(
                name: "DecontDetalii");

            migrationBuilder.DropTable(
                name: "Deconturi");

            migrationBuilder.DropTable(
                name: "EventResource");

            migrationBuilder.DropTable(
                name: "FacturiIesire");

            migrationBuilder.DropTable(
                name: "FacturiIesireDetalii");

            migrationBuilder.DropTable(
                name: "FacturiIntrare");

            migrationBuilder.DropTable(
                name: "FacturiIntrareDetalii");

            migrationBuilder.DropTable(
                name: "FileData");

            migrationBuilder.DropTable(
                name: "HCategories");

            migrationBuilder.DropTable(
                name: "Imperecheri");

            migrationBuilder.DropTable(
                name: "Incasari");

            migrationBuilder.DropTable(
                name: "ListeDiferenteInventar");

            migrationBuilder.DropTable(
                name: "ListeDiferenteInventarDetalii");

            migrationBuilder.DropTable(
                name: "ModelDifferenceAspects");

            migrationBuilder.DropTable(
                name: "NIRuri");

            migrationBuilder.DropTable(
                name: "NoteTransfer");

            migrationBuilder.DropTable(
                name: "Parteneri");

            migrationBuilder.DropTable(
                name: "PerioadeFiscale");

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
                name: "Plati");

            migrationBuilder.DropTable(
                name: "PoliticiNumerotare");

            migrationBuilder.DropTable(
                name: "RapoarteProductie");

            migrationBuilder.DropTable(
                name: "RegistruContabil");

            migrationBuilder.DropTable(
                name: "RegistruStoc");

            migrationBuilder.DropTable(
                name: "ReguliContare");

            migrationBuilder.DropTable(
                name: "ReguliStoc");

            migrationBuilder.DropTable(
                name: "ReportDataV2");

            migrationBuilder.DropTable(
                name: "StateMachineAppearances");

            migrationBuilder.DropTable(
                name: "StateMachineTransitions");

            migrationBuilder.DropTable(
                name: "UnitatiInterne");

            migrationBuilder.DropTable(
                name: "AuditEFCoreWeakReferences");

            migrationBuilder.DropTable(
                name: "Events");

            migrationBuilder.DropTable(
                name: "Resource");

            migrationBuilder.DropTable(
                name: "ConturiProprii");

            migrationBuilder.DropTable(
                name: "ModelDifferences");

            migrationBuilder.DropTable(
                name: "PermissionPolicyTypePermissionObject");

            migrationBuilder.DropTable(
                name: "PermissionPolicyUser");

            migrationBuilder.DropTable(
                name: "DocumentTrezorerie");

            migrationBuilder.DropTable(
                name: "Conturi");

            migrationBuilder.DropTable(
                name: "PermissionPolicyRoleBase");

            migrationBuilder.DropTable(
                name: "Repartitori");

            migrationBuilder.DropTable(
                name: "Documente");

            migrationBuilder.DropTable(
                name: "DocumentDetalii");

            migrationBuilder.DropTable(
                name: "Angajamente");

            migrationBuilder.DropTable(
                name: "CoduriEconomice");

            migrationBuilder.DropTable(
                name: "CoduriFunctionale");

            migrationBuilder.DropTable(
                name: "Loturi");

            migrationBuilder.DropTable(
                name: "Proiecte");

            migrationBuilder.DropTable(
                name: "SurseFinantare");

            migrationBuilder.DropTable(
                name: "Unitati");

            migrationBuilder.DropTable(
                name: "Gestiuni");

            migrationBuilder.DropTable(
                name: "Produse");

            migrationBuilder.DropTable(
                name: "TipuriMaterial");

            migrationBuilder.DropTable(
                name: "ClaseProduse");

            migrationBuilder.DropTable(
                name: "PoliticiConex");

            migrationBuilder.DropTable(
                name: "TipuriDocument");

            migrationBuilder.DropTable(
                name: "StateMachineStates");

            migrationBuilder.DropTable(
                name: "StateMachines");
        }
    }
}
