using DevExpress.ExpressApp;
using DevExpress.ExpressApp.EFCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Atlas.Conta.BackOffice.Module.Motor;

// Tranzacția explicită a unei comenzi (F27-D1): `CommitChanges` se înrolează în
// ea (`BatchExecutor` deschide tranzacție proprie doar dacă
// `CurrentTransaction == null`), deci blocarea luată înainte ține și după commit.
public static class TranzactieComanda {
    /// <summary>Deschide tranzacția comenzii pe `DbContext`-ul ObjectSpace-ului primit.</summary>
    public static IDbContextTransaction Incepe(IObjectSpace os) {
        if (os is not EFCoreObjectSpace efCore)
            throw new InvalidOperationException(
                $"Comanda cere un ObjectSpace EF Core; „{os?.GetType().Name ?? "null"}” nu expune `DbContext`, "
                + "deci blocarea rândului de perioadă nu poate fi luată.");
        return efCore.DbContext.Database.BeginTransaction();
    }
}
