namespace Atlas.Conta.Nucleu;

public static class Motor {
    // T-D2: `Miscari` → `Operare`, `Mutari` → `Transfer`; un refuz pe oricare refuză contractul.
    public static Contract Opereaza(Declaratie declaratie, Rotunjire rotunjire) {
        ArgumentNullException.ThrowIfNull(declaratie);
        ArgumentNullException.ThrowIfNull(rotunjire);
        var tranzactii = new List<Tranzactie>(2);
        if (declaratie.Miscari.Count > 0) {
            var postari = new List<Postare>(declaratie.Miscari.Count * 2);
            foreach (var miscare in declaratie.Miscari) {
                var (debit, credit) = Miscare.Postari(miscare, declaratie.Data);
                postari.Add(debit);
                postari.Add(credit);
            }
            tranzactii.Add(new Tranzactie(
                FelTranzactie.Operare, declaratie.Data, declaratie.Document, postari.ToArray()));
        }
        if (declaratie.Mutari.Count > 0)
            tranzactii.Add(Transferul(declaratie.Document, declaratie.Data, declaratie.Mutari));
        return Incheie(tranzactii, declaratie.Decizii, declaratie.Ipoteze, rotunjire);
    }

    public static Contract Transfera(
        Guid document,
        DateOnly data,
        IReadOnlyList<Mutare> mutari,
        Rotunjire rotunjire) {
        ArgumentNullException.ThrowIfNull(mutari);
        ArgumentNullException.ThrowIfNull(rotunjire);
        if (mutari.Count == 0)
            throw new ArgumentException("transferul se cere cu cel puțin o mutare.", nameof(mutari));
        return Incheie([Transferul(document, data, mutari)], [], [], rotunjire);
    }

    static Tranzactie Transferul(Guid document, DateOnly data, IReadOnlyList<Mutare> mutari) {
        var postari = new List<Postare>(mutari.Count * 2);
        foreach (var mutare in mutari) {
            ArgumentNullException.ThrowIfNull(mutare, nameof(mutari));
            if (mutare.Cauza.Document != document)
                throw new ArgumentException(
                    $"mutarea are cauza pe documentul {mutare.Cauza.Document}, nu pe {document}.",
                    nameof(mutari));
            var (iesire, intrare) = Mutare.Postari(mutare, data);
            postari.Add(iesire);
            postari.Add(intrare);
        }
        return new Tranzactie(FelTranzactie.Transfer, data, document, postari.ToArray());
    }

    // Contorul e al instanței primite: nucleul nu rotunjește nimic la D6a, doar raportează (N-D5).
    static Contract Incheie(
        IReadOnlyList<Tranzactie> tranzactii,
        IReadOnlyList<Decizie> decizii,
        IReadOnlyList<Ipoteza> ipoteze,
        Rotunjire rotunjire) {
        var refuzuri = new List<Refuz>();
        foreach (var tranzactie in tranzactii)
            refuzuri.AddRange(Conservare.Verifica(tranzactie));
        return refuzuri.Count == 0
            ? Contract.Accepta(tranzactii, decizii, ipoteze, rotunjire.JumatatiDeBan)
            : Contract.Refuza(refuzuri, decizii, ipoteze, rotunjire.JumatatiDeBan);
    }
}
