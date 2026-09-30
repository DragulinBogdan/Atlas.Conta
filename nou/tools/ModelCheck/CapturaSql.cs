using System.Data.Common;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Atlas.Conta.BackOffice.ModelCheck;

/// <summary>Comenzile SQL emise de EF Core în timpul unei acțiuni, prin DiagnosticListener-ul global al EF Core.</summary>
sealed partial class CapturaSql : IObserver<DiagnosticListener>, IObserver<KeyValuePair<string, object>>, IDisposable {
    public sealed record Comanda(string Text, IReadOnlyList<DbParameter> Parametri) {
        public TimeSpan Durata { get; set; }
        public long Randuri { get; set; }
    }

    readonly List<Comanda> comenzi = [];
    readonly Dictionary<Guid, Comanda> inCurs = [];
    readonly List<IDisposable> abonari = [];

    CapturaSql() => abonari.Add(DiagnosticListener.AllListeners.Subscribe(this));

    public static List<string> Comenzi(Action actiune) => [.. Masoara(actiune).Select(c => c.Text)];

    public static List<Comanda> Masoara(Action actiune) {
        using var captura = new CapturaSql();
        actiune();
        lock (captura.comenzi) return [.. captura.comenzi];
    }

    public static HashSet<string> Tabele(IEnumerable<string> comenzi) =>
        comenzi.SelectMany(c => TabelaRegex().Matches(c).Select(m => m.Groups[1].Value)).ToHashSet(StringComparer.Ordinal);

    [GeneratedRegex("(?:FROM|JOIN)\\s+\"([^\"]+)\"")]
    private static partial Regex TabelaRegex();

    public void OnNext(DiagnosticListener listener) {
        if (listener.Name == DbLoggerCategory.Name)
            lock (abonari) abonari.Add(listener.Subscribe(this));
    }

    public void OnNext(KeyValuePair<string, object> eveniment) {
        lock (comenzi) {
            if (eveniment.Key == RelationalEventId.CommandExecuting.Name && eveniment.Value is CommandEventData date) {
                var parametri = date.Command.Parameters.Cast<DbParameter>()
                    .Select(p => p is ICloneable c ? (DbParameter)c.Clone() : p).ToList();
                var c = new Comanda(date.Command.CommandText, parametri);
                comenzi.Add(c);
                inCurs[date.CommandId] = c;
            }
            else if (eveniment.Key == RelationalEventId.CommandExecuted.Name && eveniment.Value is CommandExecutedEventData executat
                     && inCurs.TryGetValue(executat.CommandId, out var c))
                c.Durata += executat.Duration;
            else if (eveniment.Key == RelationalEventId.DataReaderClosing.Name && eveniment.Value is DataReaderClosingEventData citit
                     && inCurs.TryGetValue(citit.CommandId, out var r))
                r.Randuri += citit.ReadCount;
        }
    }

    public void OnCompleted() { }
    public void OnError(Exception error) { }

    public void Dispose() {
        lock (abonari) foreach (var a in abonari) a.Dispose();
    }
}
