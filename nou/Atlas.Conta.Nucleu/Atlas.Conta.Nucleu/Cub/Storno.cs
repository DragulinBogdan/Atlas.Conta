namespace Atlas.Conta.Nucleu;

public static class Storno {
    public static Tranzactie Inverseaza(IEnumerable<Postare> cauzateSiAtribuite, Guid document, DateOnly data) =>
        Inverseaza(cauzateSiAtribuite, document, data, null);

    public static Tranzactie Inverseaza(
            IEnumerable<Postare> cauzateSiAtribuite, Guid document, DateOnly data, int? perioadaDeclarare) {
        ArgumentNullException.ThrowIfNull(cauzateSiAtribuite);
        return Inverseaza(cauzateSiAtribuite.Select(p => (Guid.Empty, p)), document, data, perioadaDeclarare);
    }

    // N-D10 amendat (TR-D7a S-D5): reperul fiscal al stornoului e perioada stornării.
    // D9-A2: stornoul unește mai multe tranzacții sursă; perechile fiecărei surse se decalează după
    // ordinalul maxim al surselor dinaintea ei (ordinea identificatorului), ca să rămână distincte.
    public static Tranzactie Inverseaza(
            IEnumerable<(Guid Sursa, Postare Postare)> dinSurse, Guid document, DateOnly data, int? perioadaDeclarare) {
        ArgumentNullException.ThrowIfNull(dinSurse);
        var lista = dinSurse.ToList();
        var decalaje = new Dictionary<Guid, int>();
        var decalaj = 0;
        foreach (var sursa in lista.Select(x => x.Sursa).Distinct().OrderBy(s => s)) {
            decalaje[sursa] = decalaj;
            decalaj += lista.Where(x => x.Sursa == sursa).Max(x => x.Postare.Pereche ?? 0);
        }
        // N-D10: fără schimb de latură; cauza și atribuirea rămân ale postării stornate.
        var postari = lista
            .Select(x => x.Postare with {
                Coordonate = x.Postare.Coordonate with {
                    Data = data,
                    ReperFiscal = x.Postare.Coordonate.ReperFiscal is { } reper
                        ? reper with { PerioadaD394 = data.Year * 100 + data.Month, DataInregistrare = data }
                        : null,
                    PerioadaDeclarare = perioadaDeclarare is { } noua && x.Postare.Coordonate.PerioadaDeclarare is not null
                        ? noua
                        : x.Postare.Coordonate.PerioadaDeclarare,
                },
                Cantitate = -x.Postare.Cantitate,
                ValoareValuta = -x.Postare.ValoareValuta,
                Valoare = -x.Postare.Valoare,
                Pereche = x.Postare.Pereche is { } pereche ? pereche + decalaje[x.Sursa] : null,
            })
            .ToArray();
        if (postari.Length == 0)
            throw new ArgumentException("nu există nicio postare de stornat.", nameof(dinSurse));
        return new Tranzactie(FelTranzactie.Storno, data, document, postari);
    }

    public static IReadOnlyList<Postare> Selecteaza(
            IReadOnlyList<(Guid Id, Postare Postare)> postari,
            Guid document) {
        ArgumentNullException.ThrowIfNull(postari);
        return Selecteaza([.. postari.Select(x => (x.Id, Guid.Empty, x.Postare))], document)
            .Select(x => x.Postare).ToList();
    }

    public static IReadOnlyList<(Guid Sursa, Postare Postare)> Selecteaza(
        IReadOnlyList<(Guid Id, Guid Sursa, Postare Postare)> postari,
        Guid document) {
        ArgumentNullException.ThrowIfNull(postari);
        var cauzate = new HashSet<Guid>();
        foreach (var (id, _, postare) in postari)
            if (postare.Cauza.Document == document)
                cauzate.Add(id);
        var selectate = new List<(Guid, Postare)>();
        // Un singur salt: atribuitul e o postare de valoare care arată spre o postare de stoc (N-D10).
        foreach (var (id, sursa, postare) in postari)
            if (cauzate.Contains(id) || (postare.Atribuit is { } tinta && cauzate.Contains(tinta)))
                selectate.Add((sursa, postare));
        return selectate;
    }
}
