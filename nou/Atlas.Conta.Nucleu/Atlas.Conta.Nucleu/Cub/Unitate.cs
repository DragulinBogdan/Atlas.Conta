namespace Atlas.Conta.Nucleu;

public sealed record Unitate(
    Guid Id,
    FelUnitate Fel,
    Guid Cont,
    Guid? Partener,
    Guid? Produs,
    DateOnly Deschisa);
