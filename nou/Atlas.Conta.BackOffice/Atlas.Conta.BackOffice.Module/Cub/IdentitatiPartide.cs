#nullable enable
using System.Security.Cryptography;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.Module.Cub;

public static class IdentitatiPartide {
    public static bool EsteProprie(N.Unitate unitate, Guid document) =>
        unitate.Fel == N.FelUnitate.Partida && unitate.Partener is Guid partener
        && (unitate.Id == N.Unitate.DeschidePartida(unitate.Cont, partener, document, unitate.Deschisa).Id
            || unitate.Id == Anterioara(document, unitate.Cont));

    public static N.Unitate? Gaseste(IEnumerable<N.Unitate> unitati, Guid document, Guid cont, Guid partener) =>
        unitati.Where(u => u.Cont == cont && u.Partener == partener && EsteProprie(u, document))
            .Distinct().SingleOrDefault();

    // 092b: compatibilitate la citire cu cheia N-D6, fără rescrierea postărilor.
    public static Guid Anterioara(Guid document, Guid cont) {
        var intrare = new byte[32];
        document.ToByteArray().CopyTo(intrare, 0);
        cont.ToByteArray().CopyTo(intrare, 16);
        var id = SHA256.HashData(intrare)[..16];
        id[7] = (byte)((id[7] & 0x0F) | 0x80);
        return new Guid(id);
    }
}
