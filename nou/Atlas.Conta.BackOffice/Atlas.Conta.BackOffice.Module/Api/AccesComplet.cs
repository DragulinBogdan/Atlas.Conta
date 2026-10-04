using DevExpress.ExpressApp;
using DevExpress.ExpressApp.EFCore;
using DevExpress.ExpressApp.Security;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Conta.BackOffice.Module.Api;

/// <summary>
/// Accesul complet la un set de tipuri (SAF-D4, X-D4 c): niciun criteriu de rând sau de membru
/// pe tipurile citite, inclusiv pe toate tipurile mapate în aceleași tabele.
/// </summary>
public static class AccesComplet {
    public static IReadOnlySet<string> Tabele(IObjectSpace os, IEnumerable<Type> citite) {
        var model = ((EFCoreObjectSpace)os).DbContext.Model;
        return citite.Select(t => model.FindEntityType(t)?.GetTableName()).OfType<string>()
            .ToHashSet(StringComparer.Ordinal);
    }

    /// <summary>
    /// Tipurile și membrii pe care utilizatorul nu îi poate citi necondiționat; lista goală = acces complet.
    /// Tipurile citite vin primele; frunzele unei tabele cu baza restricționată nu se mai enumeră.
    /// </summary>
    public static List<string> Lipsuri(IObjectSpace os, ISelectDataSecurityProvider securitate, IEnumerable<Type> citite) {
        ArgumentNullException.ThrowIfNull(securitate);
        var lista = citite.ToList();
        var model = ((EFCoreObjectSpace)os).DbContext.Model;
        var tabele = Tabele(os, lista);
        var tipuri = model.GetEntityTypes().Where(e => e.GetTableName() is { } t && tabele.Contains(t))
            .Select(e => e.ClrType).Distinct()
            .OrderBy(t => lista.IndexOf(t) is var i && i >= 0 ? i : int.MaxValue).ThenBy(t => t.Name, StringComparer.Ordinal).ToList();
        var citire = securitate.CreateSelectDataSecurity(os);
        var acoperite = new HashSet<string>(StringComparer.Ordinal);
        var membriAcoperiti = new HashSet<(string, string)>();
        var lipsuri = new List<string>();
        foreach (var tip in tipuri) {
            var tabela = model.FindEntityType(tip)?.GetTableName();
            if (acoperite.Contains(tabela)) continue;
            if (Restrictiv(citire.GetObjectCriteria(tip))) {
                lipsuri.Add(tip.Name);
                if (lista.Contains(tip)) acoperite.Add(tabela);
                continue;
            }
            foreach (var membru in SecurityMembersHelper.GetSecurityMembers(tip))
                if (!membriAcoperiti.Contains((tabela, membru)) && Restrictiv(citire.GetMemberCriteria(tip, membru))) {
                    lipsuri.Add($"{tip.Name}.{membru}");
                    if (lista.Contains(tip)) membriAcoperiti.Add((tabela, membru));
                }
        }
        return lipsuri;
    }

    static bool Restrictiv(IList<string> criterii) => criterii.Any(c => !string.IsNullOrEmpty(c));
}
