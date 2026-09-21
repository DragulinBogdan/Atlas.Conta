using System.Security.Cryptography;
using System.Text;

namespace Atlas.Conta.Nucleu;

/// <summary>
/// Gestiunile STRUCTURALE ale capetelor din afara evidenței (090g): id-uri
/// deterministe, fără rând de nomenclator.
/// </summary>
public static class GestiuniVirtuale {
    public static Guid Furnizor { get; } = Identitate("Atlas.Conta.GestiuneVirtuala:Furnizor");

    public static Guid Client { get; } = Identitate("Atlas.Conta.GestiuneVirtuala:Client");

    public static Guid Consum { get; } = Identitate("Atlas.Conta.GestiuneVirtuala:Consum");

    public static bool Este(Guid? gestiune) =>
        gestiune == Furnizor || gestiune == Client || gestiune == Consum;

    // Aceeași amprentă ca `Unitate.DeschidePartida` (N-D6): SHA-256, primii 16
    // octeți, nibble-ul de versiune (octetul 7) pus pe 8.
    static Guid Identitate(string nume) {
        var amprenta = SHA256.HashData(Encoding.UTF8.GetBytes(nume));
        var id = amprenta[..16];
        id[7] = (byte)((id[7] & 0x0F) | 0x80);
        return new Guid(id);
    }
}
