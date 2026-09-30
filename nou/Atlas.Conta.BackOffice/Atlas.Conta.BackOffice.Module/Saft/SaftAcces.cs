using Atlas.Conta.BackOffice.Module.BusinessObjects;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.EFCore;
using DevExpress.ExpressApp.Security;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Conta.BackOffice.Module.Saft;

/// <summary>
/// Accesul complet cerut de D406 L pe cub (SAF-D4, S2-D5): niciun criteriu de rând sau de membru pe tipurile
/// citite de <see cref="SaftProiectii.SaftPeCub"/>, inclusiv pe toate tipurile mapate în aceleași tabele.
/// </summary>
public static class SaftAcces {
    public const string Refuz = "SAFT_ACCES_INCOMPLET";

    /// <summary>Tipurile ale căror tabele le citește exportul L pe cub; proba SC-SAFT-36 le compară cu SQL-ul emis.</summary>
    public static readonly Type[] Citite = [
        typeof(Cub.Postare), typeof(Cub.Tranzactie), typeof(Document), typeof(DocumentDetaliu), typeof(Cont),
        typeof(Repartitor), typeof(TipDocument), typeof(TipTva), typeof(Societate), typeof(Produs), typeof(Lot),
        typeof(UnitateMasura), typeof(TipMaterial), typeof(CodFunctional), typeof(CodEconomic), typeof(SursaFinantare),
        typeof(Unitate), typeof(Proiect), typeof(Judet), typeof(ClasaProdus), typeof(MapareTvaSaft), typeof(SetareProfil),
    ];

    /// <summary>Tipurile citite de exportul S pe cub (S3-D6); proba SC-SAFT-48 le compară cu SQL-ul emis.</summary>
    public static readonly Type[] CititeStocuri = [
        typeof(Cub.Postare), typeof(Cub.Tranzactie), typeof(Document), typeof(DocumentDetaliu), typeof(Cont),
        typeof(Repartitor), typeof(TipDocument), typeof(TipTva), typeof(Societate), typeof(Produs),
        typeof(UnitateMasura), typeof(TipMaterial), typeof(Judet), typeof(ClasaProdus), typeof(MapareTvaSaft),
        typeof(SetareProfil), typeof(PoliticaMiscareSaft), typeof(PerioadaFiscala), typeof(SoldPerioadaStoc),
    ];

    public static IReadOnlySet<string> Tabele(IObjectSpace os, IEnumerable<Type> citite = null) {
        var model = ((EFCoreObjectSpace)os).DbContext.Model;
        return (citite ?? Citite).Select(t => model.FindEntityType(t)?.GetTableName()).OfType<string>()
            .ToHashSet(StringComparer.Ordinal);
    }

    /// <summary>
    /// Tipurile și membrii pe care utilizatorul nu îi poate citi necondiționat; lista goală = acces complet.
    /// Tipurile citite de secțiune vin primele; frunzele unei tabele cu baza restricționată nu se mai enumeră.
    /// </summary>
    public static List<string> Lipsuri(IObjectSpace os, ISelectDataSecurityProvider securitate, IEnumerable<Type> citite = null) {
        ArgumentNullException.ThrowIfNull(securitate);
        var lista = (citite ?? Citite).ToList();
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
