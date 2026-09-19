namespace Atlas.Conta.Nucleu;

public static class Storno {
    public static Tranzactie Inverseaza(IEnumerable<Postare> cauzateSiAtribuite, Guid document, DateOnly data) {
        ArgumentNullException.ThrowIfNull(cauzateSiAtribuite);
        // N-D10: fără schimb de latură; cauza și atribuirea rămân ale postării stornate.
        var postari = cauzateSiAtribuite
            .Select(postare => postare with {
                Coordonate = postare.Coordonate with { Data = data },
                Cantitate = -postare.Cantitate,
                ValoareValuta = -postare.ValoareValuta,
                Valoare = -postare.Valoare,
            })
            .ToArray();
        if (postari.Length == 0)
            throw new ArgumentException("nu există nicio postare de stornat.", nameof(cauzateSiAtribuite));
        return new Tranzactie(FelTranzactie.Storno, data, document, postari);
    }

    public static IReadOnlyList<Postare> Selecteaza(
        IReadOnlyList<(Guid Id, Postare Postare)> postari,
        Guid document) {
        ArgumentNullException.ThrowIfNull(postari);
        var cauzate = new HashSet<Guid>();
        foreach (var (id, postare) in postari)
            if (postare.Cauza.Document == document)
                cauzate.Add(id);
        var selectate = new List<Postare>();
        // Un singur salt: atribuitul e o postare de valoare care arată spre o postare de stoc (N-D10).
        foreach (var (id, postare) in postari)
            if (cauzate.Contains(id) || (postare.Atribuit is { } tinta && cauzate.Contains(tinta)))
                selectate.Add(postare);
        return selectate;
    }
}
