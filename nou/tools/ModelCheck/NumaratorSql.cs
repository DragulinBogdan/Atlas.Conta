using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Atlas.Conta.BackOffice.ModelCheck;

// Numără comenzile SQL trimise prin DbContext-urile ObjectSpace-urilor (D85-R2)
// și păstrează textul lor, ca dovadă a traducerii unei expresii.
sealed class NumaratorSql : DbCommandInterceptor {
    public static readonly NumaratorSql Instanta = new();
    readonly List<string> comenzi = [];

    public IReadOnlyList<string> Comenzi { get { lock (comenzi) return comenzi.ToArray(); } }
    public int Numar { get { lock (comenzi) return comenzi.Count; } }
    public void Reseteaza() { lock (comenzi) comenzi.Clear(); }

    public override InterceptionResult<DbDataReader> ReaderExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result) {
        Noteaza(command);
        return result;
    }
    public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result, CancellationToken cancellationToken = default) {
        Noteaza(command);
        return ValueTask.FromResult(result);
    }
    public override InterceptionResult<object> ScalarExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<object> result) {
        Noteaza(command);
        return result;
    }
    public override InterceptionResult<int> NonQueryExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<int> result) {
        Noteaza(command);
        return result;
    }

    void Noteaza(DbCommand command) { lock (comenzi) comenzi.Add(command.CommandText); }
}
