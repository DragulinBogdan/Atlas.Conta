namespace Atlas.Conta.Nucleu;

public sealed record Analiza(
    Guid? CodFunctional,
    Guid? CodEconomic,
    Guid? SursaFinantare,
    Guid? UnitateOrganizatorica,
    Guid? Proiect,
    Guid? CentruCost) {

    public static readonly Analiza Fara = new(null, null, null, null, null, null);
}
