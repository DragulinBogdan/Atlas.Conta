using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Atlas.Conta.BackOffice.ModelCheck;

/// <summary>Comenzile SQL emise de EF Core în timpul unei acțiuni, prin DiagnosticListener-ul global al EF Core.</summary>
sealed partial class CapturaSql : IObserver<DiagnosticListener>, IObserver<KeyValuePair<string, object>>, IDisposable {
    readonly List<string> comenzi = [];
    readonly List<IDisposable> abonari = [];

    CapturaSql() => abonari.Add(DiagnosticListener.AllListeners.Subscribe(this));

    public static List<string> Comenzi(Action actiune) {
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
        if (eveniment.Key == RelationalEventId.CommandExecuting.Name && eveniment.Value is CommandEventData date)
            lock (comenzi) comenzi.Add(date.Command.CommandText);
    }

    public void OnCompleted() { }
    public void OnError(Exception error) { }

    public void Dispose() {
        lock (abonari) foreach (var a in abonari) a.Dispose();
    }
}
