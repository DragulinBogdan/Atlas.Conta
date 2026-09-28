namespace Atlas.Conta.BackOffice.Module.BusinessObjects;

public interface ILinieCuAvans {
    Guid? LinieAvansId { get; set; }
    DocumentDetaliu LinieAvans { get; set; }
}
