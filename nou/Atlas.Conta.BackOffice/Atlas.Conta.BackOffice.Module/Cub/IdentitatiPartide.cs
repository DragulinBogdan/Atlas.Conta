#nullable enable
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.Module.Cub;

public static class IdentitatiPartide {
    public static bool EsteProprie(N.Unitate unitate, Guid document) =>
        unitate.Fel == N.FelUnitate.Partida && unitate.Partener is Guid partener
        && unitate.Id == N.Unitate.DeschidePartida(unitate.Cont, partener, document, unitate.Deschisa).Id;

    public static N.Unitate? Gaseste(IEnumerable<N.Unitate> unitati, Guid document, Guid cont, Guid partener) =>
        unitati.Where(u => u.Cont == cont && u.Partener == partener && EsteProprie(u, document))
            .Distinct().SingleOrDefault();
}
