namespace Atlas.Conta.Nucleu;

public sealed record Tranzactie(
    FelTranzactie Fel,
    DateOnly Data,
    Guid? Document,
    IReadOnlyList<Postare> Postari);
