using System.Data.Common;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Conta.BackOffice.Module.Proiectii;

internal static class SqlInterogare {
    public static string Compune<T>(IQueryable<T> interogare, Func<object, string> parametru) where T : struct {
        // XAF 26.1 SecurityQueryCompiler decorează rezultatele referință; aici se proiectează numai scalari.
        using var comanda = interogare.CreateDbCommand();
        var parametri = comanda.Parameters.Cast<DbParameter>()
            .ToDictionary(p => "@" + p.ParameterName.TrimStart('@'), p => parametru(p.Value));
        var sql = comanda.CommandText.Replace("{", "{{").Replace("}", "}}");
        return Regex.Replace(sql, "'(?:''|[^'])*'|\"(?:\"\"|[^\"])*\"|@[A-Za-z0-9_]+",
            m => parametri.TryGetValue(m.Value, out var valoare) ? valoare : m.Value);
    }
}
