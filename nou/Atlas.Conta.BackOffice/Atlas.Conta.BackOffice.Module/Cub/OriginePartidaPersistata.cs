using Atlas.DXF.Core.Appearance.Attributes;

namespace Atlas.Conta.BackOffice.Module.Cub;

[ForbidCRUD("ListView", "DetailView")]
public class OriginePartidaPersistata {
    public virtual Guid ID { get; set; }
    public virtual Guid ContId { get; set; }
    public virtual Guid PartenerId { get; set; }
    public virtual Guid? DocumentId { get; set; }
    public virtual Guid? ReferintaInitiala { get; set; }
    public virtual int? OrdinalInitial { get; set; }
}
