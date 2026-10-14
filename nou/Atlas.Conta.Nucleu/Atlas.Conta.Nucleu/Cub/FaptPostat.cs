namespace Atlas.Conta.Nucleu;

/// <summary>Postarea scrisă, cu identitatea ei și cu ordinea scrierii.</summary>
public sealed record FaptPostat(
    Guid Id, Guid Tranzactie, FelTranzactie Fel, DateTime ScrisLa, int Ordinal, Postare Postare);
