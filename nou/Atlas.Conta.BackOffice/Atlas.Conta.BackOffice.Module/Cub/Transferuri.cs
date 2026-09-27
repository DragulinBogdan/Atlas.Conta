#nullable enable
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.Module.Cub;

public static class Transferuri {
    public const string PartidaProprieInsuficienta = "PARTIDA_PROPRIE_INSUFICIENTA";

    public sealed record Cerere(
        Guid StingatorId,
        DateOnly DataStingator,
        IReadOnlyList<N.Postare> OperareStingator,
        IReadOnlyList<N.Postare> TransferuriStingator,
        Guid StinsId,
        DateOnly DataStins,
        IReadOnlyList<N.Postare> OperareStins,
        IReadOnlyList<N.Postare> TransferuriStins,
        decimal Suma,
        DateOnly Data,
        Guid? PartidaTinta = null,
        Guid? PartenerCerut = null);

    public sealed record Rezultat(N.Mutare? Mutare, DateOnly Data, N.Refuz? Refuz, string? Sarit);


    public static Rezultat Muta(Cerere cerere) {
        ArgumentNullException.ThrowIfNull(cerere);
        if (cerere.OperareStingator.Count == 0)
            return Sare("documentul care stinge n-are tranzacție `Operare` în cub");
        if (cerere.OperareStins.Count == 0)
            return Sare("documentul stins n-are tranzacție `Operare` în cub");
        if (cerere.Suma > 0m) return MutaPozitiv(cerere);
        var primite = cerere.TransferuriStins.Where(p => p.Cauza.Document == cerere.StingatorId
                && p.Coordonate.Unitate is { Fel: N.FelUnitate.Partida }
                && (cerere.PartidaTinta == null || p.Coordonate.Unitate.Id == cerere.PartidaTinta))
            .GroupBy(p => p.Coordonate.Unitate!)
            .Select(g => new { Unitate = g.Key, Net = g.Sum(p => p.Coordonate.Latura == N.Latura.Debit ? p.Valoare : -p.Valoare) })
            .Where(p => p.Net != 0m).ToArray();
        if (primite.Length == 0) return Sare("nu există efect de desfăcut pe partida stinsului");
        if (primite.Length != 1) return Refuza("IMPERECHERE_AMBIGUA", "nominalizarea nu identifică un singur efect de desfăcut");
        var efect = primite[0]; var tinta = efect.Unitate;
        if (tinta.Partener is not Guid tert) return Sare("partida nu poartă partener");
        var proprie = IdentitatiPartide.Gaseste(cerere.OperareStingator.Concat(cerere.TransferuriStingator)
            .Select(p => p.Coordonate.Unitate).OfType<N.Unitate>(), cerere.StingatorId, tinta.Cont, tert);
        proprie ??= N.Unitate.DeschidePartida(tinta.Cont, tert, cerere.StingatorId, cerere.DataStingator);
        return new(new N.Mutare(Capatul(tinta.Cont, tert, tinta), Capatul(tinta.Cont, tert, proprie),
            efect.Net > 0m ? N.Latura.Debit : N.Latura.Credit, 0m, 0m,
            Math.Min(Math.Abs(cerere.Suma), Math.Abs(efect.Net)), new(cerere.StingatorId, null)), cerere.Data, null, null);
    }

    static Rezultat MutaPozitiv(Cerere c) {
        var aleStingatorului = c.OperareStingator.Concat(c.TransferuriStingator).ToArray();
        var aleStinsului = c.OperareStins.Concat(c.TransferuriStins).ToArray();
        var proprii = aleStingatorului.Select(p => p.Coordonate.Unitate).OfType<N.Unitate>()
            .Where(u => IdentitatiPartide.EsteProprie(u, c.StingatorId)
                && (c.PartenerCerut == null || u.Partener == c.PartenerCerut)).Distinct().ToArray();
        var tinte = aleStinsului.Select(p => p.Coordonate.Unitate).OfType<N.Unitate>()
            .Where(u => c.PartidaTinta is Guid tinta ? u.Id == tinta : IdentitatiPartide.EsteProprie(u, c.StinsId)).Distinct().ToArray();
        var perechi = (from deLa in proprii
                       from la in tinte
                       where deLa.Cont == la.Cont && deLa.Partener == la.Partener
                       let disponibil = aleStingatorului.Where(p => p.Coordonate.Unitate?.Id == deLa.Id)
                           .Sum(p => p.Coordonate.Latura == N.Latura.Debit ? p.Valoare : -p.Valoare)
                       let rest = aleStinsului.Where(p => p.Coordonate.Unitate?.Id == la.Id)
                           .Sum(p => p.Coordonate.Latura == N.Latura.Debit ? p.Valoare : -p.Valoare)
                       where disponibil * rest < 0m
                       select new { deLa, la, disponibil, rest }).ToArray();
        if (perechi.Length == 0) {
            var compatibile = proprii.Any(s => tinte.Any(t => s.Cont == t.Cont && s.Partener == t.Partener));
            return Refuza(compatibile ? PartidaProprieInsuficienta : "IMPERECHERE_FARA_EFECT",
                "nu există partide disponibile de sens opus pe același cont și partener");
        }
        if (perechi.Length != 1)
            return Refuza("IMPERECHERE_AMBIGUA", "există mai multe perechi eligibile; comanda pe document nu identifică o singură partidă");
        var p = perechi[0];
        if (c.Suma > Math.Abs(p.disponibil) || c.Suma > Math.Abs(p.rest))
            return Refuza(PartidaProprieInsuficienta,
                $"stingerea cere {c.Suma}; disponibil {Math.Abs(p.disponibil)}, rest {Math.Abs(p.rest)} pe contul {p.deLa.Cont}");
        return new(new N.Mutare(Capatul(p.deLa.Cont, p.deLa.Partener!.Value, p.deLa),
            Capatul(p.la.Cont, p.la.Partener!.Value, p.la), p.disponibil > 0m ? N.Latura.Debit : N.Latura.Credit,
            0m, 0m, c.Suma, new N.Cauza(c.StingatorId, null)), c.Data, null, null);
    }

    static N.Capat Capatul(Guid cont, Guid tert, N.Unitate partida) =>
        new() { Cont = cont, Partener = tert, Unitate = partida };

    static Rezultat Sare(string motiv) => new(null, default, null, motiv);

    static Rezultat Refuza(string cod, string mesaj) =>
        new(null, default, new N.Refuz(cod, mesaj, null), null);
}
