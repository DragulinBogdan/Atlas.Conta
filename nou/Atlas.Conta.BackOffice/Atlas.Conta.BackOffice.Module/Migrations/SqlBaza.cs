using System.Reflection;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Atlas.Conta.BackOffice.Module.Migrations;

/// <summary>Schema pe care modelul EF nu o declară, chemată din migrația de bază (120e).</summary>
public static class SqlBaza {
    /// <summary>`Postare` partiționată pe `Spatiu` (S-D2, S-r4), `cub_partida_id` (092a), `PostareVizual` (D9-A12).</summary>
    public static void Creeaza(MigrationBuilder migrationBuilder) {
        foreach (var fisier in new[] { "Postare", "CubPartidaId", "PostareVizual" })
            migrationBuilder.Sql(Citeste(fisier));
    }

    public static void Sterge(MigrationBuilder migrationBuilder) => migrationBuilder.Sql(Citeste("Sterge"));

    static string Citeste(string fisier) {
        var resursa = $"{typeof(SqlBaza).Namespace}.Sql.{fisier}.sql";
        using var flux = Assembly.GetExecutingAssembly().GetManifestResourceStream(resursa)
            ?? throw new InvalidOperationException($"Resursa SQL {resursa} lipsește din asamblare.");
        using var cititor = new StreamReader(flux);
        return cititor.ReadToEnd();
    }
}
