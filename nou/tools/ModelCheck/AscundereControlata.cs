using Microsoft.EntityFrameworkCore;

namespace Atlas.Conta.BackOffice.ModelCheck;

static class AscundereControlata {
    /// <summary>Rândul de nomenclator dispare în tranzacția curentă, cu FK-urile suspendate; apelantul face rollback.</summary>
    public static void Ascunde(DbContext db, string tabela, Guid id) {
        if (db.Database.CurrentTransaction == null)
            throw new InvalidOperationException("Ascunderea controlată cere o tranzacție deschisă, rulată înapoi de apelant.");
        db.Database.ExecuteSqlRaw("SET LOCAL session_replication_role = replica");
        db.Database.ExecuteSqlRaw($"DELETE FROM \"{tabela}\" WHERE \"ID\" = {{0}}", id);
    }
}
