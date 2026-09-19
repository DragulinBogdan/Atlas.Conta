namespace Atlas.Conta.Nucleu;

public sealed class RefuzException(Refuz refuz) : Exception($"{refuz.Cod}: {refuz.Mesaj}") {
    public Refuz Refuz { get; } = refuz;
}
