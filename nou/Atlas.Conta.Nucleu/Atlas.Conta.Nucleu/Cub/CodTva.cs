namespace Atlas.Conta.Nucleu;

public sealed record CodTva(Guid TipTva, SensTva Sens, RolTva Rol) {
    public RegimTva Regim { get; init; }
    public decimal Cota { get; init; }
    public bool DeImport { get; init; }
}
