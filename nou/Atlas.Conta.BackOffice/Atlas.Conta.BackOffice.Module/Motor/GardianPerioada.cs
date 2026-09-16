using System.Runtime.CompilerServices;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.EFCore;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Conta.BackOffice.Module.Motor;

// Decizia 14: perioada închisă = graniță absolută. O perioadă NEDEFINITĂ e
// tratată ca închisă — exercițiul de lucru se deschide explicit prin seed/UI.
public static class GardianPerioada {
    // `FOR SHARE` ține rândul perioadei cât durează tranzacția comenzii, în locul
    // `FirstOrDefault`-ului (F27-D1): zero statement-uri în plus. `"GCRecord" = 0`
    // explicit — `SqlQuery` nu trece prin filtrul global (66). `AS "Value"`: forma
    // cerută de `SqlQuery<T>` pentru un scalar.
    const string Sql = """
        SELECT "Inchisa" AS "Value"
        FROM "PerioadeFiscale"
        WHERE "An" = {0} AND "Luna" = {1} AND "GCRecord" = 0
        FOR SHARE
        """;

    public static void VerificaDeschisa(IObjectSpace os, DateOnly data) {
        if (os is not EFCoreObjectSpace efCore)
            throw new InvalidOperationException(
                $"Gardianul de perioadă cere un ObjectSpace EF Core; „{os?.GetType().Name ?? "null"}” nu expune `DbContext`.");
        var stare = efCore.DbContext.Database
            .SqlQuery<bool>(FormattableStringFactory.Create(Sql, data.Year, data.Month))
            .ToList();
        if (stare.Count == 0)
            throw new OperareException($"Perioada {data.Month:00}/{data.Year} nu e definită — e tratată ca închisă.");
        if (stare[0])
            throw new OperareException($"Perioada {data.Month:00}/{data.Year} e închisă.");
    }
}
