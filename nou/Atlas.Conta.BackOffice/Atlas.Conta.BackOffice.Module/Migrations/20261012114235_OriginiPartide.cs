using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atlas.Conta.BackOffice.Module.Migrations
{
    /// <inheritdoc />
    public partial class OriginiPartide : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OriginiPartide",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    ContId = table.Column<Guid>(type: "uuid", nullable: false),
                    PartenerId = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReferintaInitiala = table.Column<Guid>(type: "uuid", nullable: true),
                    OrdinalInitial = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OriginiPartide", x => x.ID);
                    table.CheckConstraint("CK_OriginiPartide_Origine", "(\"DocumentId\" IS NOT NULL AND \"ReferintaInitiala\" IS NULL AND \"OrdinalInitial\" IS NULL)\nOR (\"DocumentId\" IS NULL AND \"ReferintaInitiala\" IS NOT NULL\n    AND \"ReferintaInitiala\" <> '00000000-0000-0000-0000-000000000000'::uuid\n    AND \"OrdinalInitial\" IS NOT NULL AND \"OrdinalInitial\" > 0)");
                    table.ForeignKey(
                        name: "FK_OriginiPartide_Conturi_ContId",
                        column: x => x.ContId,
                        principalTable: "Conturi",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_OriginiPartide_Documente_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "Documente",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_OriginiPartide_Repartitori_PartenerId",
                        column: x => x.PartenerId,
                        principalTable: "Repartitori",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_OriginiPartide_ContId",
                table: "OriginiPartide",
                column: "ContId");

            migrationBuilder.CreateIndex(
                name: "IX_OriginiPartide_DocumentId_ContId_PartenerId",
                table: "OriginiPartide",
                columns: new[] { "DocumentId", "ContId", "PartenerId" },
                unique: true,
                filter: "\"DocumentId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_OriginiPartide_PartenerId",
                table: "OriginiPartide",
                column: "PartenerId");

            migrationBuilder.CreateIndex(
                name: "IX_OriginiPartide_ReferintaInitiala_ContId_PartenerId",
                table: "OriginiPartide",
                columns: new[] { "ReferintaInitiala", "ContId", "PartenerId" },
                unique: true,
                filter: "\"ReferintaInitiala\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OriginiPartide");
        }
    }
}
