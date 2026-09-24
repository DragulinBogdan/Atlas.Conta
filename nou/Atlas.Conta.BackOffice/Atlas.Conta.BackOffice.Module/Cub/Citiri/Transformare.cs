using System.Linq.Expressions;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.Module.Cub.Citiri;

public static class Transformare {
    public static readonly Expression<Func<Postare, bool>> Contrapondere = p =>
        p.Gestiune == N.GestiuniVirtuale.Transformare && p.Unitate == null && p.Produs != null
        && p.Carte == N.Carte.Contabil && p.Partener == null && p.TipTvaId == null
        && p.PerioadaDeclarare == null && p.Valuta == null
        && p.Valoare == 0m && p.ValoareValuta == 0m && p.Cantitate != 0m;

    public static readonly Expression<Func<Postare, bool>> FaraContrapondere =
        Expression.Lambda<Func<Postare, bool>>(Expression.Not(Contrapondere.Body), Contrapondere.Parameters);

    static readonly Func<Postare, bool> verifica = Contrapondere.Compile();

    public static bool EsteContrapondere(N.Postare p) => verifica(new Postare {
        Gestiune = p.Coordonate.Gestiune, Unitate = p.Coordonate.Unitate?.Id,
        Produs = p.Coordonate.Produs, Carte = p.Coordonate.Carte,
        Partener = p.Coordonate.Partener, TipTvaId = p.Coordonate.CodTva?.TipTva,
        PerioadaDeclarare = p.Coordonate.PerioadaDeclarare, Valuta = p.Coordonate.Valuta,
        Valoare = p.Valoare, ValoareValuta = p.ValoareValuta, Cantitate = p.Cantitate,
    });
}
