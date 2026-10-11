using System.Linq.Expressions;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.Module.Cub.Citiri;

public static class Transformare {
    public static readonly Expression<Func<Postare, bool>> Contrapondere = p =>
        p.Contrapondere == N.Contrapondere.Transformare;

    public static readonly Expression<Func<Postare, bool>> FaraContrapondere =
        Expression.Lambda<Func<Postare, bool>>(Expression.Not(Contrapondere.Body), Contrapondere.Parameters);

    public static bool EsteContrapondere(N.Postare p) =>
        p.Coordonate.Contrapondere == N.Contrapondere.Transformare;
}
