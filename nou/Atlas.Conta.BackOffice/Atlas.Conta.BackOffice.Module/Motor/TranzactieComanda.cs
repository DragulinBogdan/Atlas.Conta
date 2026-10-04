using DevExpress.ExpressApp;
using DevExpress.ExpressApp.EFCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace Atlas.Conta.BackOffice.Module.Motor;

// Tranzacția explicită a unei comenzi (F27-D1): `CommitChanges` se înrolează în
// ea (`BatchExecutor` deschide tranzacție proprie doar dacă
// `CurrentTransaction == null`), deci blocarea luată înainte ține și după commit.
public static class TranzactieComanda {
    /// <summary>Blocajul scrierii, serial per bază, ținut până la sfârșitul tranzacției (X-D6).</summary>
    public const string BlocajScriere = "SELECT pg_advisory_xact_lock(97000)";
    public const string ScriereOcupata = "SCRIERE_OCUPATA";

    internal static IDbContextTransaction Asigura(IObjectSpace os) =>
        os is EFCoreObjectSpace ef && ef.DbContext.Database.CurrentTransaction != null ? null : Incepe(os);

    /// <summary>Deschide tranzacția comenzii pe `DbContext`-ul ObjectSpace-ului primit și ia blocajul scrierii.</summary>
    public static IDbContextTransaction Incepe(IObjectSpace os) {
        if (os is not EFCoreObjectSpace efCore)
            throw new InvalidOperationException(
                $"Comanda cere un ObjectSpace EF Core; „{os?.GetType().Name ?? "null"}” nu expune `DbContext`, "
                + "deci blocarea rândului de perioadă nu poate fi luată.");
        return Incepe(efCore.DbContext.Database);
    }

    /// <summary>Tranzacția cu blocajul scrierii; null când blocajul intră în tranzacția existentă.</summary>
    internal static IDbContextTransaction Asigura(DatabaseFacade baza) {
        if (baza.CurrentTransaction == null) return Incepe(baza);
        Blocheaza(baza);
        return null;
    }

    internal static async Task<IDbContextTransaction> AsiguraAsync(DatabaseFacade baza, CancellationToken anulare) {
        var tranzactie = baza.CurrentTransaction == null ? await baza.BeginTransactionAsync(anulare) : null;
        try {
            await baza.ExecuteSqlRawAsync(BlocajScriere, anulare);
            return tranzactie;
        }
        catch (Exception e) {
            if (tranzactie != null) await tranzactie.DisposeAsync();
            if (e.InnerException is TimeoutException) throw Ocupata();
            throw;
        }
    }

    static IDbContextTransaction Incepe(DatabaseFacade baza) {
        var tranzactie = baza.BeginTransaction();
        try {
            Blocheaza(baza);
            return tranzactie;
        }
        catch {
            tranzactie.Dispose();
            throw;
        }
    }

    static void Blocheaza(DatabaseFacade baza) {
        try { baza.ExecuteSqlRaw(BlocajScriere); }
        catch (Exception e) when (e.InnerException is TimeoutException) { throw Ocupata(); }
    }

    static OperareException Ocupata() =>
        new($"{ScriereOcupata}: altă comandă scrie în bază; reîncercați.");
}
