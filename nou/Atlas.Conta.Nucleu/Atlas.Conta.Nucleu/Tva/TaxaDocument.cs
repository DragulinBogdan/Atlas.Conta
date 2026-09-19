namespace Atlas.Conta.Nucleu;

public sealed record TaxaDocument(
    IReadOnlyDictionary<(RegimTva Regim, decimal Cota), decimal> PerCota,
    IReadOnlyDictionary<Guid, decimal> PerLinie);
