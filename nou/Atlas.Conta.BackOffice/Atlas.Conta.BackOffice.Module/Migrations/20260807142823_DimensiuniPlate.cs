using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atlas.Conta.BackOffice.Module.Migrations
{
    // DIM-3 (decizia 54c): owned-urile Dimensiuni de pe RegistruContabil și
    // RegulaContare au devenit coloane PLATE cu [Column] pe aceleași nume —
    // schema relațională e IDENTICĂ (aceleași coloane, FK-uri și indexuri),
    // deci migrația e goală intenționat: există doar ca să înregistreze
    // schimbarea de model în snapshot.
    /// <inheritdoc />
    public partial class DimensiuniPlate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
