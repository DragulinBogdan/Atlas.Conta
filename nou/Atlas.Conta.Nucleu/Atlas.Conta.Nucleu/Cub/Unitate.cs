using System.Security.Cryptography;

namespace Atlas.Conta.Nucleu;

public sealed record Unitate(
    Guid Id,
    FelUnitate Fel,
    Guid Cont,
    Guid? Partener,
    Guid? Produs,
    DateOnly Deschisa) {

    // `with` ocolește constructorul; cerința se apără și în `init` (N-D6).
    readonly Guid? partener = Ceruta(Fel, FelUnitate.Partida, Partener, nameof(Partener));
    readonly Guid? produs = Ceruta(Fel, FelUnitate.Lot, Produs, nameof(Produs));

    public Guid? Partener {
        get => partener;
        init => partener = Ceruta(Fel, FelUnitate.Partida, value, nameof(Partener));
    }

    public Guid? Produs {
        get => produs;
        init => produs = Ceruta(Fel, FelUnitate.Lot, value, nameof(Produs));
    }

    public static Unitate DeschidePartida(Guid cont, Guid partener, Guid documentDeschizator, DateOnly data) =>
        new(Identitate(documentDeschizator, cont), FelUnitate.Partida, cont, partener, null, data);

    public decimal? Raport(Sold sold) {
        ArgumentNullException.ThrowIfNull(sold);
        return Fel switch {
            FelUnitate.Lot => sold.Cantitate == 0m ? null : sold.Net / sold.Cantitate,
            FelUnitate.Partida => sold.ValoareValuta == 0m ? null : sold.Net / sold.ValoareValuta,
            _ => sold.Net,
        };
    }

    // N-D6: SHA-256 peste document.ToByteArray() ‖ cont.ToByteArray() (16 + 16), primii 16 octeți ca Guid, nibble-ul de versiune (octetul 7) pus pe 8.
    static Guid Identitate(Guid document, Guid cont) {
        var intrare = new byte[32];
        document.ToByteArray().CopyTo(intrare, 0);
        cont.ToByteArray().CopyTo(intrare, 16);
        var amprenta = SHA256.HashData(intrare);
        var id = amprenta[..16];
        id[7] = (byte)((id[7] & 0x0F) | 0x80);
        return new Guid(id);
    }

    static Guid? Ceruta(FelUnitate fel, FelUnitate cere, Guid? valoare, string nume) =>
        fel == cere && valoare is null
            ? throw new ArgumentException($"unitatea de fel {fel} se cere cu {nume}.", nume)
            : valoare;
}
